using System.Net;
using System.Net.Sockets;
using System.Text;
using MrX.ProxyServer.Data;
using MrX.ProxyServer.Tools;

namespace MrX.ProxyServer.Classs;

public class ProcessClientClass
{
    public void ProcessClient(Object Parameter)
    {
        Pact pact = new Pact();
        TcpClient client = (TcpClient)Parameter;

        pact.ConnectionGroup = Dns.GetHostEntry(((IPEndPoint)client.Client.RemoteEndPoint).Address).HostName;
        ;
        pact.Client = client;
        pact.clientStream = client.GetStream();
        pact.clientStreamReader = new ReadFromStream(pact.clientStream, Encoding.ASCII);
        List<string> requestLines = new List<string>();
        string tmpLine;
        while (!String.IsNullOrEmpty(tmpLine = pact.clientStreamReader.ReadLine()))
        {
            requestLines.Add(tmpLine);
        }

        pact.Lines = requestLines;
        pact.httpCmd = requestLines.Count > 0 ? requestLines[0] : null;
        if (String.IsNullOrEmpty(pact.httpCmd))
        {
            //throw new EndOfStreamException("HTTP CMD IS NULL");
        }

        RequestData(pact);
        RequestMetod(pact);
    }

    void RequestMetod(Pact p)
    {
        HandleMetods HandleMetods = new HandleMetods();
        switch (p.method.ToUpper())
        {
            case "CONNEC":
            {
                HandleMetods.METODCONNECT(p);
                break;
            }
            case "POST":
            {
                break;
            }
            case "GET":
            {
                break;
            }
            case "PUT":
            {
                break;
            }
        }
    }

    void RequestData(Pact p)
    {
        var split = p.httpCmd.Split(Statics.spaceSplit, 3);
        p.method = split[0];
        p.remoteUri = split[1];

        if (split[2] == "HTTP/1.1")
        {
            p.version = new Version(1, 1);
            p.RequestVersion = "HTTP/1.1";
        }
        else
        {
            p.version = new Version(1, 0);
            p.RequestVersion = "HTTP/1.0";
        }
    }
}