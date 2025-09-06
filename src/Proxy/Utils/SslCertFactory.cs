using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Proxy.Utils;

public static class SslCertFactory
{
    public static X509Certificate Create()
    {
        var cert = File.ReadAllText(StaticFilePaths.SslCertFilePath);
        return new X509Certificate(cert);
    }
}
