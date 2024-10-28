using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace MrX.ProxyServer.Data;

public class Statics
{
    public static readonly int BUFFER_SIZE = 8192;
    public static readonly char[] semiSplit = new char[] { ';' };
    public static readonly char[] equalSplit = new char[] { '=' };
    public static readonly String[] colonSpaceSplit = new string[] { ": " };
    public static readonly char[] spaceSplit = new char[] { ' ' };
    public static readonly char[] commaSplit = new char[] { ',' };
    public static object _outputLockObj = new object();
    public static List<string> _pinnedCertificateClients = new List<string>();
    public static X509Store Certificatestore;
    public static readonly Regex cookieSplitRegEx = new Regex(@",(?! )");

}