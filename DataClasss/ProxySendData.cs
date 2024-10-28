namespace MrX.ProxyServer.DataClasss;

public class ProxySendData
{
    public bool Drop;
    public Requset Requset;


    public string CreateRequestToSend => ""
        + Requset.Metod + " " + Requset.Url + " " + Requset.Version + "\r\n"
        + string.Join("\r\n", Requset.Heders.ToList().Select(p => { return p.Key + ": " + p.Value; }))
        + "\r\n\r\n"
        + Requset.Data
        + "\r\n";
}