using System.Drawing;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DnsClient;
using MrX.ProxyServer.DataClasss;
using MrX.ProxyServer.Statics;

namespace MrX.ProxyServer.HandlerClasss;

public class HandlerRequset
{
    public bool handle(Stream handler, Socket socket)
    {
        return Funcs.Do(() =>
        {
            HandleEvents.OnBeforHandelRequestInvoke(this, handler);
            string[] lines = handler.RequestToLines();
            Requset Requset = new Requset(lines);
            Requset.Socket = socket;
            Requset.Stream = handler;
            HandleEvents.OnAfterHandelRequestInvoke(this, Requset);
            new HandleMetods(Requset);

        });
    }



}