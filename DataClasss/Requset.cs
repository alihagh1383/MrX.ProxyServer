using MrX.ProxyServer.Statics;
using System.Net.Sockets;

namespace MrX.ProxyServer.DataClasss;

public class Requset
{
    /// <summary>
    /// Parse Request From Lines
    /// </summary>
    /// <param name="lines">Requset Lines</param>
    public Requset(string[] lines)
    {
        Lines = lines;

        Cmd = lines[0].Replace("\r", "");
        var temp = Cmd.Split(" ", 3);
        Version = temp[2].Replace("\r","");
        Url = temp[1];
        switch (temp[0].ToUpper())
        {
            case "GET": { Metod = Metods.GET; break; }
            case "POST": { Metod = Metods.POST; break; }
            case "PUT": { Metod = Metods.PUT; break; }
            case "CONNECT": { Metod = Metods.CONNECT; break; }
            case "DELETE": { Metod = Metods.DELETE; break; }
            default: { Metod = Metods.UNKNOWN; break; }
        }
        int EndHeders = -1;
        for (int i = 1; i < lines.Length - 1; i++)
        {
            if (lines[i].ToUpper() == "\r\n") { EndHeders = i; break; }
            Funcs.Do(() =>
            {
                var temp = lines[i].Split(": ", 2);
                Heders[temp[0]] = temp[1].Replace("\r", "");
            });
        }
        if (EndHeders != -1)
        {
            for (int i = EndHeders; i < lines.Length - 1; i++)
                Data += lines[i] + "\n";
        }
        Host = Heders["Host"];
        if (Host == "") Port = 80;
        else if (!Host.Contains(":")) Port = 80;
        else if (Int32.TryParse(Host.Split(":", 2)[1], out int o)) { Host = Host.Split(":", 2)[0];  Port = o; }
        else Port = 80;
    }
    /// <summary>
    /// Socket From
    /// </summary>
    public Socket Socket;
    /// <summary>
    /// Stream From
    /// </summary>
    public Stream Stream;
    /// <summary>
    /// Request Lines
    /// </summary>
    public string[] Lines;
    /// <summary>
    /// Request CMD
    /// </summary>
    public string Cmd;
    /// <summary>
    /// Request Metod ( CMD Part 1 )
    /// </summary>
    public Metods Metod;
    /// <summary>
    /// Request URL ( CMD Part 2 )
    /// </summary>
    public string Url;
    /// <summary>
    /// Request Version ( CMD Part 3 )
    /// </summary>
    public string Version;
    /// <summary>
    /// Request Heder Host
    /// </summary>
    public string Host;
    /// <summary>
    /// Request Port From Heder Host 
    /// Defult 80
    /// </summary>
    public int Port;
    /// <summary>
    /// Request Heders
    /// </summary>
    public Dictionary<string, string> Heders { get; set; } = new Dictionary<string, string>()
    {
        {"WWW-Authenticate","" },
        {"Authorization","" },
        {"Proxy-Authenticate","" },
        {"Proxy-Authorization","" },
        {"Age","" },
        {"Cache-Control","" },
        {"Clear-Site-Data","" },
        {"Expires","" },
        {"No-Vary-Search","" },
        {"Last-Modified","" },
        {"ETag","" },
        {"If-Match","" },
        {"If-None-Match","" },
        {"If-Modified-Since","" },
        {"If-Unmodified-Since","" },
        {"Vary","" },
        {"Connection","" },
        {"Keep-Alive","" },
        {"Accept","" },
        {"Accept-Encoding","" },
        {"Accept-Language","" },
        {"Expect","" },
        {"Max-Forwards","" },
        {"Cookie","" },
        {"Set-Cookie","" },
        {"Access-Control-Allow-Credentials","" },
        {"Access-Control-Allow-Headers","" },
        {"Access-Control-Allow-Methods","" },
        {"Access-Control-Allow-Origin","" },
        {"Access-Control-Expose-Headers","" },
        {"Access-Control-Max-Age","" },
        {"Access-Control-Request-Headers","" },
        {"Access-Control-Request-Method","" },
        {"Origin","" },
        {"Timing-Allow-Origin","" },
        {"Content-Disposition","" },
        {"Content-Length","" },
        {"Content-Type","" },
        {"Content-Encoding","" },
        {"Content-Language","" },
        {"Content-Location","" },
        {"Forwarded","" },
        {"Via","" },
        {"Location","" },
        {"Refresh","" },
        {"From","" },
        {"Host","" },
        {"Referer","" },
        {"Referrer-Policy","" },
        {"User-Agent","" },
        {"Allow","" },
        {"Server","" },
        {"Accept-Ranges","" },
        {"Range","" },
        {"If-Range","" },
        {"Content-Range","" },
        {"Cross-Origin-Embedder-Policy","" },
        {"Cross-Origin-Opener-Policy","" },
        {"Cross-Origin-Resource-Policy","" },
        {"Content-Security-Policy","" },
        {"Content-Security-Policy-Report-Only","" },
        {"Permissions-Policy","" },
        {"Strict-Transport-Security","" },
        {"Upgrade-Insecure-Requests","" },
        {"X-Content-Type-Options","" },
        {"X-Frame-Options","" },
        {"X-Permitted-Cross-Domain-Policies","" },
        {"X-Powered-By","" },
        {"X-XSS-Protection","" },
        {"Sec-Fetch-Site","" },
        {"Sec-Fetch-Mode","" },
        {"Sec-Fetch-User","" },
        {"Sec-Fetch-Dest","" },
        {"Sec-Purpose","" },
        {"Service-Worker-Navigation-Preload","" },
        {"Report-To","" },
        {"Transfer-Encoding","" },
        {"TE","" },
        {"Trailer","" },
        {"Alt-Svc","" },
        {"Alt-Used","" },
        {"Date","" },
        {"Link","" },
        {"Retry-After","" },
        {"Server-Timing","" },
        {"Service-Worker-Allowed","" },
        {"SourceMap","" },
        {"Upgrade","" },
        {"Accept-CH","" },
        {"Critical-CH","" },
        {"Sec-CH-UA","" },
        {"Sec-CH-UA-Arch","" },
        {"Sec-CH-UA-Bitness","" },
        {"Sec-CH-UA-Full-Version-List","" },
        {"Sec-CH-UA-Mobile","" },
        {"Sec-CH-UA-Model","" },
        {"Sec-CH-UA-Platform","" },
        {"Sec-CH-UA-Platform-Version","" },
        {"Sec-CH-UA-Prefers-Color-Scheme","" },
        {"Sec-CH-UA-Prefers-Reduced-Motion","" },
        {"Device-Memory","" },
        {"Downlink","" },
        {"ECT","" },
        {"RTT","" },
        {"Save-Data","" },
        {"Sec-GPC","" },
        {"Origin-Isolation","" },
        {"NEL","" },
        {"Accept-Push-Policy","" },
        {"Accept-Signature","" },
        {"Early-Data","" },
        {"Push-Policy","" },
        {"Signature","" },
        {"Signed-Headers","" },
        {"Speculation-Rules","" },
        {"Supports-Loading-Mode","" },
        {"X-Forwarded-For","" },
        {"X-Forwarded-Host","" },
        {"X-Forwarded-Proto","" },
        {"X-DNS-Prefetch-Control","" },
        {"X-Robots-Tag","" },
        {"Pragma","" },
        {"Warning","" },
    };
    /// <summary>
    /// Request After Heders
    /// </summary>
    public string Data { get; set; } = "";

    public enum Metods
    {
        GET, CONNECT, POST, PUT, DELETE, UNKNOWN, HEAD, OPTIONS, TRACE, PATCH
    }
}