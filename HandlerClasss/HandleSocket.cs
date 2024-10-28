using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MrX.ProxyServer.HandlerClasss
{
    internal static class HandleSocket
    {
        internal static int ReceveInternet(this Socket socket, byte[] data)
        {
           Thread.Sleep((data.Length)/10);
            var buf = new ArraySegment<byte>(data);
            int r = socket.ReceiveAsync(buf,SocketFlags.None).Result;
            return r;
        }



    }
}

