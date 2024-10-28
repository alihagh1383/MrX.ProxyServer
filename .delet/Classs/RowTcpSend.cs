using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using MrX.ProxyServer.Data;

namespace MrX.ProxyServer.Classs;

public class RowTcpSend
{
    public static void Do(string hostname, int tunnelPort, Stream clientStream)
    {
        TcpClient tunnelClient = new(hostname, tunnelPort);
        var tunnelStream = tunnelClient.GetStream();
        var tunnelReadBuffer = new byte[Statics.BUFFER_SIZE];
        Thread sendRelay =
            new Thread(() => MrX.Extensions.Stream.CopyTo(clientStream, tunnelStream, Statics.BUFFER_SIZE));
        Thread receiveRelay =
            new Thread(() => MrX.Extensions.Stream.CopyTo(tunnelStream, clientStream, Statics.BUFFER_SIZE));

        sendRelay.Start();
        receiveRelay.Start();

        sendRelay.Join();
        receiveRelay.Join();
        if (tunnelStream != null)
            tunnelStream.Close();

        if (tunnelClient != null)
            tunnelClient.Close();
    }

    public static void Do(string httpCmd, string secureHostName, ref List<string> requestLines, bool isSecure,
        Stream clientStream)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(httpCmd);
        sb.Append(Environment.NewLine);

        string hostname = secureHostName;
        for (int i = 1; i < requestLines.Count; i++)
        {
            var header = requestLines[i];


            if (secureHostName == null)
            {
                String[] headerParsed = httpCmd.Split(Statics.colonSpaceSplit, 2, StringSplitOptions.None);
                switch (headerParsed[0].ToLower())
                {
                    case "host":
                        var hostdetail = headerParsed[1];
                        if (hostdetail.Contains(":"))
                            hostname = hostdetail.Split(':')[0].Trim();
                        else
                            hostname = hostdetail.Trim();
                        break;
                    default:
                        break;
                }
            }

            sb.Append(header);
            sb.Append(Environment.NewLine);
        }

        sb.Append(Environment.NewLine);

        if (hostname == null)
        {
            //  Dns.geth
        }

        int tunnelPort = 80;
        if (isSecure) tunnelPort = 443;

        System.Net.Sockets.TcpClient tunnelClient = new System.Net.Sockets.TcpClient(hostname, tunnelPort);
        var tunnelStream = (System.IO.Stream)tunnelClient.GetStream();

        if (isSecure)
        {
            var sslStream = new SslStream(tunnelStream);
            sslStream.AuthenticateAsClient(hostname);
            tunnelStream = sslStream;
        }


        Thread sendRelay = new Thread(() =>
            MrX.Extensions.Stream.CopyTo(clientStream, sb.ToString(), tunnelStream, Statics.BUFFER_SIZE));
        Thread receiveRelay =
            new Thread(() => MrX.Extensions.Stream.CopyTo(tunnelStream, clientStream, Statics.BUFFER_SIZE));

        sendRelay.Start();
        receiveRelay.Start();

        sendRelay.Join();
        receiveRelay.Join();

        if (tunnelStream != null)
            tunnelStream.Close();

        if (tunnelClient != null)
            tunnelClient.Close();
    }
}