using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MrX.ProxyServer.HandlerClasss
{
    internal class HandleHeders
    {

        public static string Content_Length(Stream stream, string Length)
        {
            string data = "";
            int length = int.Parse(Length);
            for (int i = 0; i < length; i++)
            {
                byte[] buffer = new byte[1];
                stream.Read(buffer);
                data += Encoding.ASCII.GetString(buffer);
            }
            return data;
        }
        public static string Transfer_Encoding(Stream stream, string Mode)
        {
            string data = "";
            switch (Mode)
            {
                case "chunked":
                    {

                        using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                        {
                            StringBuilder sb = new StringBuilder();
                            try
                            {
                                while (!sr.EndOfStream)
                                {
                                    sb.Append((char)sr.Read());
                                }
                            }
                            catch (System.IO.IOException)
                            { }

                            data = sb.ToString();
                        }
                        /* bool countinue = true;
                         string sizes = "";
                         int sizei = 0;
                         while (countinue)
                         {
                             Thread.Sleep(100);
                             while (true)
                             {
                                 byte[] size = new byte[1];
                                 int read = stream.Read(size);
                                 if (read == 0) { countinue = false; break; }
                                 sizes += Encoding.ASCII.GetString(size);
                                 if (sizes.Contains("\r\n")) { sizei = Int32.Parse(sizes.Replace("\r\n", ""), System.Globalization.NumberStyles.AllowHexSpecifier); break; }
                             }
                             if (!countinue) break;
                             byte[] bodyBuff = new byte[sizei];
                             if (stream.Read(bodyBuff) != sizei) { countinue = false; break; }
                             data += Encoding.ASCII.GetString(bodyBuff);
                             sizei = 0;
                             sizes = "";
                             bodyBuff = new byte[3];
                             int rea = stream.Read(bodyBuff);
                             string temp = Encoding.ASCII.GetString(bodyBuff);
                             if (rea != 3) { countinue = false; break; }
                             sizes += temp[2];
                        }*/
                        break;
                    }


                default:
                    break;
            }
            return data;
        }
    }
}
