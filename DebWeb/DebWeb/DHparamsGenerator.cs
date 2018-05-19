using System.IO;

namespace DebWeb
{
    public static class DHparamsGenerator
    {
        private static  string ticketPath = "/etc/nginx/ssl/dwticket.key";
        public static  string DhparamsPath = "/etc/nginx/ssl/dhparams.pem";
        public static void VerifyParams()
        {
            if (!Directory.Exists("/etc/nginx/ssl") && !File.Exists(ticketPath))
            {   
                "mkdir -p /etc/nginx/ssl".Bash();
                $"openssl rand 48 -out {ticketPath}".Bash();
                $"openssl dhparam -out {DhparamsPath} 4096".Bash();
            }
        }
    }
}