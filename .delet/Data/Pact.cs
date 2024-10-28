using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using MrX.ProxyServer.Tools;

namespace MrX.ProxyServer.Data;

public class Pact
{
    public string ConnectionGroup;
    public List<String> Lines;
    public TcpClient Client; 
    public Stream clientStream;
    public String httpCmd;
    public String method;
    public ReadFromStream clientStreamReader;
    public String remoteUri;
    public Version version;
    public String RequestVersion;
    public String tunnelHostName;
    public Int32 tunnelPort;
}