using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MrX.ProxyServer.HandlerClasss
{
    internal static class HandleStreams
    {
        internal static string[] RequestToLines(this Stream Stream)
        {
            string data = "", headerString = "";
            bool flag = true;
            while (flag)
            {
                byte[] buffer = new byte[1];
                Stream.Read(buffer);
                headerString += Encoding.ASCII.GetString(buffer);
                if (headerString.Contains("\r\n\r\n"))
                {
                    data += headerString;
                    {
                        Regex cl = new Regex("\\\r\nContent-Length: (.*?)\\\r\n");
                        Match m = cl.Match(headerString);
                        if (m.Success)
                        {
                            flag = false;
                            data += HandleHeders.Content_Length(Stream, m.Groups[1].ToString());
                            break;
                        }
                    }
                    {
                        Regex cl = new Regex("\\\r\nTransfer-Encoding: (.*?)\\\r\n");
                        Match m = cl.Match(headerString);
                        if (m.Success)
                        {
                            flag = false;
                            data += HandleHeders.Transfer_Encoding(Stream, m.Groups[1].ToString());
                            break;

                        }
                    }
                    break;
                }
            }
            return data.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToArray();
        }
    }
}
