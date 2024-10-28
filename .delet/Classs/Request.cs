using System.Net;
using MrX.ProxyServer.Data;

namespace MrX.ProxyServer.Classs;

public class Request
{
    public static void ReadRequestHeaders(ref List<string> requestLines, HttpWebRequest webReq)
    {
        for (int i = 1; i < requestLines.Count; i++)
        {
            String httpCmd = requestLines[i];

            String[] header = httpCmd.Split(Statics.colonSpaceSplit, 2, StringSplitOptions.None);

            if (!String.IsNullOrEmpty(header[0].Trim()))
                switch (header[0].ToLower())
                {
                    case "accept":
                        webReq.Accept = header[1];
                        break;
                    case "accept-encoding":
                        webReq.Headers.Add(header[0], "gzip,deflate,zlib");
                        break;
                    case "cookie":
                        webReq.Headers["Cookie"] = header[1];
                        break;
                    case "connection":
                        if (header[1].ToLower() == "keep-alive")
                            webReq.KeepAlive = true;

                        break;
                    case "content-length":
                        int contentLen;
                        int.TryParse(header[1], out contentLen);
                        if (contentLen != 0)
                            webReq.ContentLength = contentLen;
                        break;
                    case "content-type":
                        webReq.ContentType = header[1];
                        break;
                    case "expect":
                        if (header[1].ToLower() == "100-continue")
                            webReq.ServicePoint.Expect100Continue = true;
                        else
                            webReq.Expect = header[1];
                        break;
                    case "host":
                        webReq.Host = header[1];
                        break;
                    case "if-modified-since":
                        String[] sb = header[1].Trim().Split(Statics.semiSplit);
                        DateTime d;
                        if (DateTime.TryParse(sb[0], out d))
                            webReq.IfModifiedSince = d;
                        break;
                    case "proxy-connection":
                        break;
                    case "range":
                        var startEnd = header[1].Replace(Environment.NewLine, "").Remove(0, 6).Split('-');
                        if (startEnd.Length > 1)
                        {
                            if (!String.IsNullOrEmpty(startEnd[1]))
                                webReq.AddRange(int.Parse(startEnd[0]), int.Parse(startEnd[1]));
                            else webReq.AddRange(int.Parse(startEnd[0]));
                        }
                        else
                            webReq.AddRange(int.Parse(startEnd[0]));

                        break;
                    case "referer":
                        webReq.Referer = header[1];
                        break;
                    case "user-agent":
                        webReq.UserAgent = header[1];
                        break;
                    case "transfer-encoding":
                        if (header[1].ToLower() == "chunked")
                            webReq.SendChunked = true;
                        else
                            webReq.SendChunked = false;
                        break;
                    case "upgrade":
                        if (header[1].ToLower() == "http/1.1")
                            webReq.Headers.Add(header[0], header[1]);
                        break;

                    default:
                        if (header.Length > 0)
                            webReq.Headers.Add(header[0], header[1]);
                        else
                            webReq.Headers.Add(header[0], "");

                        break;
                }
        }
    }
}