using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MrX.ProxyServer.Classs;
using MrX.ProxyServer.Tools;
namespace MrX.ProxyServer;
public class ProxyServer
{
    private TcpListener _listener;
    private Thread _listenerThread;
    private int _port;
    public bool Start(int port = 4000)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        _listenerThread = new Thread(new ParameterizedThreadStart(Lesten));
        _listenerThread.Start(_listener);
        _listenerThread.IsBackground = true;
        Console.WriteLine($"Start On {_listener.LocalEndpoint}");
        return true;
    }
    public void Lesten(Object Parameter)
    {
        TcpListener listener = (TcpListener)Parameter;
        try
        {
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                while (!ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(new ProcessClientClass().ProcessClient), client)) ;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    
    
  
    
    
}