using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DebWeb.EnvSettings;

namespace DebWeb
{
    public class NginxTemplate : FileTemplate
    {
        protected bool sslMode;
        public NginxTemplate(SystemSettings sSettings, AppSettings settings, bool useSslTemplate = false) : base(sSettings, settings)
        {
            this.sslMode = useSslTemplate;
        }

        public override string GetFilePath()
        {
            return $"{systemSettings.SitesAvailableNginx}/{appSettings.ProjectName}";
        }

        public override string GetTemplate()
        {
            return sslMode ? GetSslTemplate() : GetNoSslTemplate();
        }

        protected string GetNoSslTemplate()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"#### AUTO GENERATED BY DebWeb - {DateTime.UtcNow} ####");
            builder.AppendLine("server {");
            builder.AppendLine($"  listen 80;");
            builder.AppendLine($"  server_name {string.Join(" ", appSettings.Dns)};");
            builder.AppendLine($"  root {appSettings.ProjectPath};");
            builder.AppendLine(@"  location ~ /\.well-known/acme-challenge {");
            builder.AppendLine(@"     allow all;");
            builder.AppendLine(@"  }");
            builder.AppendLine(@"  location / {");
            builder.AppendLine($"     proxy_pass {appSettings.ProxyPass};");
            builder.AppendLine($"     proxy_redirect     off;");
            builder.AppendLine($"     proxy_set_header   Host $host;");
            builder.AppendLine($"     proxy_set_header   X-Real-IP $remote_addr;");
            builder.AppendLine($"     proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;");
            builder.AppendLine($"     proxy_set_header   X-Forwarded-Host $server_name;");
            builder.AppendLine("  }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        protected string GetSslTemplate()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"#### AUTO GENERATED BY DebWeb - {DateTime.UtcNow} ####");

            builder.AppendLine("server {");
            builder.AppendLine($"  listen 80;");
            builder.AppendLine($"  server_name {string.Join(" ", appSettings.Dns)};");
            builder.AppendLine($"  root {appSettings.ProjectPath};");
            builder.AppendLine(@"  location ~ /\.well-known/acme-challenge {");
            builder.AppendLine(@"     allow all;");
            builder.AppendLine(@"  }");
            builder.AppendLine(@"  location / {");
            builder.AppendLine($"    return 301 https://$host$request_uri;");
            builder.AppendLine("  }");
            builder.AppendLine("}");

            builder.AppendLine("### SSL Endpoint ####");

            builder.AppendLine("server {");
            builder.AppendLine($"  listen 443 ssl http2;");
            builder.AppendLine($"  server_name {string.Join(" ", appSettings.Dns)};");
            builder.AppendLine($"  root {appSettings.ProjectPath};");
            builder.AppendLine(@"  location / {");
            builder.AppendLine($"     proxy_pass {appSettings.ProxyPass};");
            builder.AppendLine($"     proxy_redirect     off;");
            builder.AppendLine($"     proxy_set_header   Host $host;");
            builder.AppendLine($"     proxy_set_header   X-Real-IP $remote_addr;");
            builder.AppendLine($"     proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;");
            builder.AppendLine($"     proxy_set_header   X-Forwarded-Host $server_name;");
            builder.AppendLine("  }");
            builder.AppendLine(@"  ssl on;");
            builder.AppendLine($"  ssl_certificate {LetsencryptGenerator.GetSslCertFullchain(appSettings.Dns.First())};");
            builder.AppendLine($"  ssl_certificate_key {LetsencryptGenerator.GetSslKey(appSettings.Dns.First())};");
            builder.AppendLine(@"  ssl_stapling on;");
            builder.AppendLine(@"  ssl_stapling_verify on;");
            builder.AppendLine($"  ssl_trusted_certificate {LetsencryptGenerator.GetSslCertFullchain(appSettings.Dns.First())};");
            builder.AppendLine(@"  ssl_protocols TLSv1 TLSv1.1 TLSv1.2;");
            builder.AppendLine(@"  ssl_prefer_server_ciphers on;");
            builder.AppendLine(@"  ssl_ciphers 'ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-AES256-GCM-SHA384:DHE-RSA-AES128-GCM-SHA256:DHE-DSS-AES128-GCM-SHA256:kEDH+AESGCM:ECDHE-RSA-AES128-SHA256:ECDHE-ECDSA-AES128-SHA256:ECDHE-RSA-AES128-SHA:ECDHE-ECDSA-AES128-SHA:ECDHE-RSA-AES256-SHA384:ECDHE-ECDSA-AES256-SHA384:ECDHE-RSA-AES256-SHA:ECDHE-ECDSA-AES256-SHA:DHE-RSA-AES128-SHA256:DHE-RSA-AES128-SHA:DHE-DSS-AES128-SHA256:DHE-RSA-AES256-SHA256:DHE-DSS-AES256-SHA:DHE-RSA-AES256-SHA:!aNULL:!eNULL:!EXPORT:!DES:!RC4:!3DES:!MD5:!PSK';");
            builder.AppendLine("##### dhparams #####");
            builder.AppendLine($"  ssl_dhparam {DHparamsGenerator.DhparamsPath}; ");
            builder.AppendLine("}");

            return builder.ToString();
        }


    }
}