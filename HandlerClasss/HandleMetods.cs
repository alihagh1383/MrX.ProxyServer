using DnsClient;
using MrX.ProxyServer.DataClasss;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MrX.ProxyServer.HandlerClasss
{
    internal class HandleMetods
    {
        public HandleMetods(Requset Requset)
        {
            switch (Requset.Metod)
            {
                case Requset.Metods.CONNECT:
                    CONNECT(Requset); break;
                default:
                    Default(Requset); break;
            }
        }
        public void CONNECT(Requset Requset)
        {
            ProxySendData ProxyRequest = new ProxySendData() { Drop = false, Requset = Requset };
            HandleEvents.OnBeforSendProxyRequestInvoke(this, ProxyRequest);
            if (!ProxyRequest.Drop)
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                var dest = ProxyServer.clientdns.GetHostEntry(ProxyRequest.Requset.Host);
                var destip = dest.AddressList.FirstOrDefault();
                socket.Connect(destip, ProxyRequest.Requset.Port);



                if (socket.Connected)
                {
                    var data = Encoding.ASCII.GetBytes(""
                        + "HTTP/1.1 200 Connection Established\r\n"
                        + "Connection: close\r\n\r\n"
                        );
                    Requset.Stream.Write(data);
                    Requset.Stream.Flush();
                    int count = 0;
                    while (true)
                    {
                        {

                            var net = new NetworkStream(Requset.Socket);
                            // while (!net.DataAvailable) { }
                            while (net.DataAvailable)
                            {
                                List<byte> bytes = new List<byte>();
                                byte[] buffer = new byte[5];
                                net.ReadExactly(buffer);
                                bytes.AddRange(buffer);
                                var l = new byte[4] { buffer[4], buffer[3], 0, 0 };
                                buffer = new byte[BitConverter.ToInt32(l, 0)];
                                net.ReadExactly(buffer);
                                bytes.AddRange(buffer);
                                socket.Send(bytes.ToArray(), 0, bytes.Count, 0);
                            }
                        }
                        {

                            var net = new NetworkStream(socket);
                            // while (!net.DataAvailable) { }
                            while (net.DataAvailable)
                            {
                                List<byte> bytes = new List<byte>();
                                byte[] buffer = new byte[5];
                                net.ReadExactly(buffer);
                                bytes.AddRange(buffer);
                                var l = new byte[4] { buffer[4], buffer[3], 0, 0 };
                                buffer = new byte[BitConverter.ToInt32(l, 0)];
                                net.ReadExactly(buffer);
                                bytes.AddRange(buffer);
                                Requset.Socket.Send(bytes.ToArray(), 0, bytes.Count, 0);
                            }
                        }


                    }
                }
            }
            else
            {
                var data = Encoding.ASCII.GetBytes("Drop\r\n\r\n");
                Requset.Stream.Write(data);
            }
        }


        public void Default(Requset Requset)
        {
            ProxySendData ProxyRequest = new ProxySendData() { Drop = false, Requset = Requset };
            HandleEvents.OnBeforSendProxyRequestInvoke(this, ProxyRequest);
            if (!ProxyRequest.Drop)
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                var dest = ProxyServer.clientdns.GetHostEntry(ProxyRequest.Requset.Host);
                var destip = dest.AddressList.FirstOrDefault();
                socket.Connect(destip, ProxyRequest.Requset.Port);
                if (socket.Connected)
                {
                    socket.Send(Encoding.ASCII.GetBytes(string.Join("\r\n", ProxyRequest.Requset.Lines) + "\r\n\r\n"));

                    var net = new NetworkStream(socket);
                    while (!net.DataAvailable) { }
                    var l = net.RequestToLines();
                    Requset.Socket.Send(Encoding.ASCII.GetBytes(string.Join("\r\n", l) + "\r\n\r\n"));
                }
            }
            else
            {
                var data = Encoding.ASCII.GetBytes("Drop");
                Requset.Stream.Write(data);
            }

        }
    }
}

