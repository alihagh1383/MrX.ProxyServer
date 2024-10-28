using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DnsClient;
using MrX.ProxyServer.DataClasss;
using MrX.ProxyServer.HandlerClasss;
using MrX.ProxyServer.Statics;

namespace MrX.ProxyServer;

public class ProxyServer
{
    public static LookupClient clientdns = new LookupClient(new List<IPAddress>() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("4.2.2.4"), IPAddress.Parse("1.1.1.1") }.ToArray());
    private Socket _listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
    public HandleEvents events = new HandleEvents();
    public ProxyServer(IPEndPoint startPoint)
    {
        Funcs.Do(() => _listener.Bind(startPoint));
    }
    public Task Stop()
    {
        _listener.Close();
        return Task.CompletedTask;
    }
    public Task Start(bool ssl)
    {
        _listener.Listen(10000000);
        new Thread(new ThreadStart(() => StartHttp(ssl))).UnsafeStart();
        return Task.CompletedTask;
    }
    private async Task StartHttp(bool ssl)
    {
        while (true)
        {
            Statics.Funcs.Do(() =>
            {
                Socket socket = _listener.Accept();
                Stream networkStream = new NetworkStream(socket);
                Task.Run(
                   () => Statics.Funcs.Do(() =>
                    {
                        var h = new HandlerRequset();
                        h.handle(networkStream, socket);
                    }));
            });
        }
    }
}
//var cr = new CertificateRequest("CN=" + "Freessl.com", ECDsa.Create(), HashAlgorithmName.SHA256);
//using (var cert = cr.CreateSelfSigned(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(+1)))
//    pfxGeneratedCert = new X509Certificate2(cert.Export(X509ContentType.Pfx));
//pfxGeneratedCert = new("ssl.pfx");
