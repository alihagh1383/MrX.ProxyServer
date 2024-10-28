using MrX.ProxyServer.DataClasss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MrX.ProxyServer.HandlerClasss
{
    public class HandleEvents
    {
        public static event EventHandler<Stream> OnBeforHandelRequest;
        public static event EventHandler<Requset> OnAfterHandelRequest;
        public static event EventHandler<ProxySendData> OnBeforSendProxyRequest;
        internal static void OnBeforHandelRequestInvoke(object? se, Stream st)
        {
            OnBeforHandelRequest?.Invoke(se, st);
        }
        internal static void OnAfterHandelRequestInvoke(object? se, Requset st)
        {
            OnAfterHandelRequest?.Invoke(se, st);
        }
        internal static void OnBeforSendProxyRequestInvoke(object? se, ProxySendData st)
        {
            OnBeforSendProxyRequest?.Invoke(se, st);
        }




    }
}
