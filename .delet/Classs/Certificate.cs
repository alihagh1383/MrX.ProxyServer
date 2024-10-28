using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MrX.ProxyServer.Data;

namespace MrX.ProxyServer.Classs;

public class Certificate
{
    public static X509Certificate2 getCertificate(string hostname)
    {
        if (Statics.Certificatestore == null)
        {
            Statics.Certificatestore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            Statics.Certificatestore.Open(OpenFlags.ReadOnly);
        }

        var CachedCertificate = getCertifiateFromStore(hostname);
        if (CachedCertificate == null)
            Statics.Certificatestore.Certificates.Add(CreateClientCertificate(hostname));


        return getCertifiateFromStore(hostname);
    }

    public static X509Certificate2 getCertifiateFromStore(string hostname)
    {
        X509Certificate2Collection certificateCollection =
            Statics.Certificatestore.Certificates.Find(X509FindType.FindBySubjectName, hostname, true);

        foreach (var certificate in certificateCollection)
        {
            if (certificate.SubjectName.Name.StartsWith("CN=" + hostname) &&
                certificate.IssuerName.Name.StartsWith("CN=Titanium_Proxy_Test_Root"))
                return certificate;
        }

        return null;
    }


    public static X509Certificate2 CreateClientCertificate(string hostname)
    {
        string tempCert;
        var cr = new CertificateRequest(
            "CN=" + hostname,
            ECDsa.Create(),
            HashAlgorithmName.SHA256);
        using (
            var cert =
            cr.CreateSelfSigned(
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddYears(+1)))
            return cert;
    }
}