using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using MrX.ProxyServer.Data;

namespace MrX.ProxyServer.Classs;

public class Callback
{
     public  static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
       {
           var Args = (SessionEventArgs)asynchronousResult.AsyncState;

           // End the operation
           Stream postStream = Args.proxyRequest.EndGetRequestStream(asynchronousResult);


           if (Args.proxyRequest.ContentLength > 0)
           {
               Args.proxyRequest.AllowWriteStreamBuffering = true;
               try
               {

                   int totalbytesRead = 0;

                   int bytesToRead;
                   if (Args.proxyRequest.ContentLength <Statics. BUFFER_SIZE)
                   {
                       bytesToRead = (int)Args.proxyRequest.ContentLength;
                   }
                   else
                       bytesToRead =Statics. BUFFER_SIZE;


                   while (totalbytesRead < (int)Args.proxyRequest.ContentLength)
                   {
                       var buffer = Args.clientStreamReader.ReadBytes(bytesToRead);
                       totalbytesRead += buffer.Length;

                       int RemainingBytes = (int)Args.proxyRequest.ContentLength - totalbytesRead;
                       if (RemainingBytes < bytesToRead)
                       {
                           bytesToRead = RemainingBytes;
                       }
                       postStream.Write(buffer, 0, buffer.Length);                      

                   }

                   postStream.Close();
               }
               catch (IOException ex)
               {


                   Args.proxyRequest.KeepAlive = false;
                   Args.finishedRequestEvent.Set();
                   Debug.WriteLine(ex.Message);
                   return;
               }
               catch (WebException ex)
               {


                   Args.proxyRequest.KeepAlive = false;
                   Args.finishedRequestEvent.Set();
                   Debug.WriteLine(ex.Message);
                   return;

               }

           }
           else if (Args.proxyRequest.SendChunked)
           {
               Args.proxyRequest.AllowWriteStreamBuffering = true;
               try
               {

                   StringBuilder sb = new StringBuilder();
                   byte[] byteRead = new byte[1];
                   while (true)
                   {

                       Args.clientStream.Read(byteRead, 0, 1);
                       sb.Append(Encoding.ASCII.GetString(byteRead));

                       if (sb.ToString().EndsWith(Environment.NewLine))
                       {
                           var chunkSizeInHex = sb.ToString().Replace(Environment.NewLine, String.Empty);
                           var chunckSize = int.Parse(chunkSizeInHex, System.Globalization.NumberStyles.HexNumber);
                           if (chunckSize == 0)
                           {
                               for (int i = 0; i < Encoding.ASCII.GetByteCount(Environment.NewLine); i++)
                               {
                                   Args.clientStream.ReadByte();
                               }
                               break;
                           }
                           var totalbytesRead = 0;
                           int bytesToRead;
                           if (chunckSize <Statics.  BUFFER_SIZE)
                           {
                               bytesToRead = chunckSize;
                           }
                           else
                               bytesToRead =Statics.  BUFFER_SIZE;


                           while (totalbytesRead < chunckSize)
                           {
                               var buffer = Args.clientStreamReader.ReadBytes(bytesToRead);
                               totalbytesRead += buffer.Length;

                               int RemainingBytes = chunckSize - totalbytesRead;
                               if (RemainingBytes < bytesToRead)
                               {
                                   bytesToRead = RemainingBytes;
                               }
                               postStream.Write(buffer, 0, buffer.Length);

                           }

                           for (int i = 0; i < Encoding.ASCII.GetByteCount(Environment.NewLine); i++)
                           {
                               Args.clientStream.ReadByte();
                           }
                           sb.Clear();
                       }

                   }
                   postStream.Close();
               }
               catch (IOException ex)
               {
                   if (postStream != null)
                       postStream.Close();

                   Args.proxyRequest.KeepAlive = false;
                   Args.finishedRequestEvent.Set();
                   Debug.WriteLine(ex.Message);
                   return;
               }
               catch (WebException ex)
               {
                   if (postStream != null)
                       postStream.Close();

                   Args.proxyRequest.KeepAlive = false;
                   Args.finishedRequestEvent.Set();
                   Debug.WriteLine(ex.Message);
                   return;

               }
           }

           Args.proxyRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), Args);

       }
       public static  void GetResponseCallback(IAsyncResult asynchronousResult)
        {
             static List<Tuple<String, String>> ProcessResponse(HttpWebResponse response)
            {
                String value = null;
                String header = null;
                List<Tuple<String, String>> returnHeaders = new List<Tuple<String, String>>();
                foreach (String s in response.Headers.Keys)
                {
                    if (s.ToLower() == "set-cookie")
                    {
                        header = s;
                        value = response.Headers[s];
                    }
                    else
                        returnHeaders.Add(new Tuple<String, String>(s, response.Headers[s]));
                }

                if (!String.IsNullOrWhiteSpace(value))
                {
                    response.Headers.Remove(header);
                    String[] cookies =Statics. cookieSplitRegEx.Split(value);
                    foreach (String cookie in cookies)
                        returnHeaders.Add(new Tuple<String, String>("Set-Cookie", cookie));

                }

                return returnHeaders;
            }
            SessionEventArgs Args = (SessionEventArgs)asynchronousResult.AsyncState;
            try
            {
                Args.serverResponse = (HttpWebResponse)Args.proxyRequest.EndGetResponse(asynchronousResult);
            }
            catch (WebException webEx)
            {
                Args.proxyRequest.KeepAlive = false;
                Args.serverResponse = webEx.Response as HttpWebResponse;
            }

            Stream serverResponseStream = null;
            Stream clientWriteStream = Args.clientStream;
            StreamWriter myResponseWriter = null;
            try
            {

                myResponseWriter = new StreamWriter(clientWriteStream);

                if (Args.serverResponse != null)
                {
                    List<Tuple<String, String>> responseHeaders = ProcessResponse(Args.serverResponse);

                    serverResponseStream = Args.serverResponse.GetResponseStream();
                    Args.serverResponseStream = serverResponseStream;

                    if (Args.serverResponse.Headers.Count == 0 && Args.serverResponse.ContentLength == -1)
                        Args.proxyRequest.KeepAlive = false;

                    bool isChunked = Args.serverResponse.GetResponseHeader("transfer-encoding") == null ? false : Args.serverResponse.GetResponseHeader("transfer-encoding").ToLower() == "chunked" ? true : false;
                    Args.proxyRequest.KeepAlive = Args.serverResponse.GetResponseHeader("connection") == null ? Args.proxyRequest.KeepAlive : (Args.serverResponse.GetResponseHeader("connection") == "close" ? false : Args.proxyRequest.KeepAlive);
                    Args.upgradeProtocol = Args.serverResponse.GetResponseHeader("upgrade") == null ? null : Args.serverResponse.GetResponseHeader("upgrade");

          
                  

                    clientWriteStream.Flush();

                }
                else
                    Args.proxyRequest.KeepAlive = false;


            }
            catch (IOException ex)
            {

                Args.proxyRequest.KeepAlive = false;
                Debug.WriteLine(ex.Message);

            }
            catch (WebSocketException ex)
            {

                Args.proxyRequest.KeepAlive = false;
                Debug.WriteLine(ex.Message);

            }
            catch (ArgumentException ex)
            {

                Args.proxyRequest.KeepAlive = false;
                Debug.WriteLine(ex.Message);

            }
            catch (WebException ex)
            {
                Args.proxyRequest.KeepAlive = false;
                Debug.WriteLine(ex.Message);
            }
            finally
            {

                    if (Args.proxyRequest.KeepAlive == false)
                    {
                        if (myResponseWriter != null)
                            myResponseWriter.Close();

                        if (clientWriteStream != null)
                            clientWriteStream.Close();
                    }

                    //if (serverResponseStream != null)
                    //    serverResponseStream.Close();

                    if (Args.serverResponse != null)
                        Args.serverResponse.Close();

                Args.finishedRequestEvent.Set();

            }

        }
}