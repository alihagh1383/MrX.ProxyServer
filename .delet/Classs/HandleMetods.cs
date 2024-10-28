using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using MrX.ProxyServer.Data;

namespace MrX.ProxyServer.Classs;

public class HandleMetods
{
    public string[] Lines;


    public void METODCONNECT(Pact p)
    {
        StreamWriter connectStreamWriter = null;
        p.remoteUri = "https://" + p.remoteUri;
        p.tunnelHostName = p.remoteUri.Split(':')[0];
        int.TryParse(p.remoteUri.Split(':')[1], out p.tunnelPort);
        if (p.tunnelPort == 0) p.tunnelPort = 80;
        var isSecure = true;
        string securehost;
        for (int i = 1; i < p.Lines.Count(); i++)
        {
            var rawHeader = p.Lines[i];
            String[] header = rawHeader.ToLower().Trim().Split(Statics.colonSpaceSplit, 2, StringSplitOptions.None);
            if ((header[0] == "host"))
            {
                var hostDetails = header[1].ToLower().Trim().Split(':');
                if (hostDetails.Length > 1)
                {
                    isSecure = false;
                }
            }
        }

        p.Lines.Clear();
        connectStreamWriter = new StreamWriter(p.clientStream);
        connectStreamWriter.WriteLine(p.RequestVersion + " 200 Connection established");
        connectStreamWriter.WriteLine(String.Format("Timestamp: {0}", DateTime.Now.ToString()));
        connectStreamWriter.WriteLine(String.Format("connection:close"));
        connectStreamWriter.WriteLine();
        connectStreamWriter.Flush();


        if (p.tunnelPort != 443)
        {
            RowTcpSend.Do(p.tunnelHostName, p.tunnelPort, p.clientStreamReader.BaseStream);

            if (p.clientStream != null)
                p.clientStream.Close();

            return;
        }
        else
        {
            Monitor.Enter(Statics._outputLockObj);
            var _certificate = Certificate.getCertificate(p.tunnelHostName);
            Monitor.Exit(Statics._outputLockObj);

            SslStream sslStream = null;
            if (!Statics._pinnedCertificateClients.Contains(p.tunnelHostName) && isSecure)
            {
                sslStream = new SslStream(p.clientStream, true);
                try
                {
                    sslStream.AuthenticateAsServer(_certificate, false,
                        SslProtocols.Tls | SslProtocols.Ssl3 | SslProtocols.Ssl2, false);
                }

                catch (AuthenticationException ex)
                {
                    if (Statics._pinnedCertificateClients.Contains(p.tunnelHostName) == false)
                    {
                        Statics._pinnedCertificateClients.Add(p.tunnelHostName);
                    }

                    throw ex;
                }
            }
            else
            {
                RowTcpSend.Do(p.tunnelHostName, p.tunnelPort, p.clientStreamReader.BaseStream);

                if (p.clientStream != null)
                    p.clientStream.Close();

                return;
            }

            p.clientStreamReader = new(sslStream, Encoding.ASCII);
            p.clientStream = sslStream;
            string tmpLine;
            while (!String.IsNullOrEmpty(tmpLine = p.clientStreamReader.ReadLine())) ;
            {
                p.Lines.Add(tmpLine);
            }
            p.httpCmd = p.Lines.Count > 0 ? p.Lines[0] : null;
            if (String.IsNullOrEmpty(p.httpCmd))
            {
                throw new EndOfStreamException();
            }

            securehost = p.remoteUri;
        }

        int count = 0;
        SessionEventArgs Args = new SessionEventArgs(Statics.BUFFER_SIZE);
        while (!String.IsNullOrEmpty(p.httpCmd))
        {
            count++;

            MemoryStream mw = null;
            StreamWriter sw = null;
            Args = new SessionEventArgs(Statics.BUFFER_SIZE);

            try
            {
                String[] splitBuffer = p.httpCmd.Split(Statics.spaceSplit, 3);

                if (splitBuffer.Length != 3)
                {
                    RowTcpSend.Do(p.httpCmd, p.tunnelHostName, ref p.Lines, Args.isSecure,
                        p.clientStreamReader.BaseStream);

                    if (p.clientStream != null)
                        p.clientStream.Close();

                    return;
                }

                p.method = splitBuffer[0];
                p.remoteUri = splitBuffer[1];

                if (splitBuffer[2] == "HTTP/1.1")
                {
                    p.version = new Version(1, 1);
                }
                else
                {
                    p.version = new Version(1, 0);
                }

                if (securehost != null)
                {
                    p.remoteUri = securehost + p.remoteUri;
                    Args.isSecure = true;
                }

                //construct the web request that we are going to issue on behalf of the client.
                Args.proxyRequest = (HttpWebRequest)HttpWebRequest.Create(p.remoteUri.Trim());
                Args.proxyRequest.Proxy = null;
                Args.proxyRequest.UseDefaultCredentials = true;
                Args.proxyRequest.Method = p.method;
                Args.proxyRequest.ProtocolVersion = p.version;
                Args.clientStream = p.clientStream;
                Args.clientStreamReader = p.clientStreamReader;

                for (int i = 1; i < p.Lines.Count; i++)
                {
                    var rawHeader = p.Lines[i];
                    String[] header = rawHeader.ToLower().Trim()
                        .Split(Statics.colonSpaceSplit, 2, StringSplitOptions.None);

                    if ((header[0] == "upgrade") && (header[1] == "websocket"))
                    {
                        RowTcpSend.Do(p.httpCmd, p.tunnelHostName, ref p.Lines, Args.isSecure,
                            p.clientStreamReader.BaseStream);

                        if (p.clientStream != null)
                            p.clientStream.Close();

                        return;
                    }
                }

                Request.ReadRequestHeaders(ref p.Lines, Args.proxyRequest);


                int contentLen = (int)Args.proxyRequest.ContentLength;

                Args.proxyRequest.AllowAutoRedirect = false;
                Args.proxyRequest.AutomaticDecompression = DecompressionMethods.None;

             

                if (Args.cancel)
                {
                    if (Args.isAlive)
                    {
                        p.Lines.Clear();
                        string tmpLine;
                        while (!String.IsNullOrEmpty(tmpLine = p.clientStreamReader.ReadLine()))
                        {
                            p.Lines.Add(tmpLine);
                        }

                        p.httpCmd = p.Lines.Count > 0 ? p.Lines[0] : null;
                        continue;
                    }
                    else
                        break;
                }

                Args.proxyRequest.ConnectionGroupName = p.ConnectionGroup;
                Args.proxyRequest.AllowWriteStreamBuffering = true;

                Args.finishedRequestEvent = new ManualResetEvent(false);


                if (p.method.ToUpper() == "POST" || p.method.ToUpper() == "PUT")
                {
                    Args.proxyRequest.BeginGetRequestStream(new AsyncCallback(Callback. GetRequestStreamCallback), Args);
                }
                else
                {
                    Args.proxyRequest.BeginGetResponse(new AsyncCallback(Callback.GetResponseCallback), Args);
                }


                if (Args.isSecure)
                {
                    if (Args.proxyRequest.Method == "POST" || Args.proxyRequest.Method == "PUT")
                        Args.finishedRequestEvent.WaitOne();
                    else
                        Args.finishedRequestEvent.Set();
                }
                else
                    Args.finishedRequestEvent.WaitOne();

                p.httpCmd = null;
                if (Args.proxyRequest.KeepAlive)
                {
                    p.Lines.Clear();
                    string tmpLine;
                    while (!String.IsNullOrEmpty(tmpLine = p.clientStreamReader.ReadLine()))
                    {
                        p.Lines.Add(tmpLine);
                    }

                    p.httpCmd = p.Lines.Count() > 0 ? p.Lines[0] : null;
                }


                if (Args.serverResponse != null)
                    Args.serverResponse.Close();
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (UriFormatException ex)
            {
                throw ex;
            }
            catch (WebException ex)
            {
                throw ex;
            }
            finally
            {
                if (sw != null) sw.Close();
                if (mw != null) mw.Close();

                if (Args.proxyRequest != null) Args.proxyRequest.Abort();
                if (Args.serverResponseStream != null) Args.serverResponseStream.Close();
            }
        }
    }

   
}