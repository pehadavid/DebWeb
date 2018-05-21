using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace DebWeb
{
    public class EnvSettings
    {

        public abstract class BaseSettings
        {
            public void ThrowIfEmpty(string fieldName, string fieldVal)
            {
                if (string.IsNullOrWhiteSpace(fieldVal))
                    throw new InvalidConfigurationException($"{fieldName} is Empty");
            }

            public void ThrowIfCollectionEmpty<T>(string fieldName, List<T> collection)
            {
                if (!(collection?.Any()).GetValueOrDefault(false))
                    throw new InvalidConfigurationException($"{fieldName} is Empty");
            }

            public void ThrowIfNot(string fieldName, bool cond, string additionalMessage = "")
            {
                if (!cond)
                    throw new InvalidConfigurationException($"{fieldName} is not valid. ({additionalMessage})");
            }
            public void ThrowIf(string fieldName, bool cond, string additionalMessage = "")
            {
                if (cond)
                    throw new InvalidConfigurationException($"{fieldName} is not valid. ({additionalMessage})");
            }
            public void ThrowIfNotDirectory(string path)
            {
                if (!Directory.Exists(path))
                    throw new InvalidConfigurationException($"{path} does not exists.");
            }
        }
        /// <summary>
        /// App related settings (app name, user related settings etc.).
        /// Should be deserialized from application.json
        /// </summary>
        public class AppSettings : BaseSettings
        {
            /// <summary>
            /// Path to project fold
            /// </summary>
            /// <returns></returns>
            public string ProjectPath { get; set; }
            /// <summary>
            /// Name of project
            /// </summary>
            /// <returns></returns>
            public string ProjectName { get; set; }

            /// <summary>
            /// Username for execution
            /// </summary>
            /// <returns></returns>
            public string UserName { get; set; }

            /// <summary>
            /// Email for let's encrypt setup
            /// </summary>
            /// <returns></returns>
            public string UserEmail { get; set; }

            /// <summary>
            /// If true, it will try to setup nginx to serve webservice with SSL
            /// </summary>
            /// <returns></returns>
            public bool UseLetsencrypt { get; set; }

            /// <summary>
            /// Dns that should be handled by your web app
            /// </summary>
            /// <returns></returns>
            public List<string> Dns { get; set; }
            public string ProjetCommand { get; set; }
            public string ProxyPass { get; set; }
            public void CheckConfiguration()
            {
                ThrowIfEmpty(nameof(ProjectPath), ProjectPath);
                ThrowIfEmpty(nameof(ProjectName), ProjectName);
                ThrowIfEmpty(nameof(UserName), UserName);
                ThrowIfEmpty(nameof(ProjetCommand), ProjetCommand);
                ThrowIfEmpty(nameof(ProxyPass), ProxyPass);
                ThrowIf(nameof(UserEmail), UseLetsencrypt && string.IsNullOrWhiteSpace(UserEmail));
                ThrowIfCollectionEmpty<string>(nameof(Dns), Dns);
            }
        }


        /// <summary>
        /// System settings (pathes)
        /// Should be deserialized from environment.json
        /// </summary>
        public class SystemSettings : BaseSettings
        {
            public string SystemdPath { get; set; }
            public string SitesAvailableNginx { get; set; }
            public string SitesEnabledNginx { get; set; }
            public string SudoersPath { get; set; }
            public string LetsencryptPath { get; set; }
            public void CheckConfiguration(AppSettings appsettings)
            {


                ThrowIfEmpty(nameof(SystemdPath), SystemdPath);
                ThrowIfEmpty(nameof(SitesAvailableNginx), SitesAvailableNginx);
                ThrowIfEmpty(nameof(SitesEnabledNginx), SitesEnabledNginx);
                ThrowIfEmpty(nameof(SudoersPath), SudoersPath);
                ThrowIfNot(nameof(LetsencryptPath), appsettings.UseLetsencrypt ? !string.IsNullOrWhiteSpace(LetsencryptPath) : true, "App is using SSL but let's encrypt path is empty");

                ThrowIfNotDirectory(SystemdPath);
                ThrowIfNotDirectory(SitesAvailableNginx);
                ThrowIfNotDirectory(SitesEnabledNginx);
                ThrowIfNotDirectory(SudoersPath);
                if (appsettings.UseLetsencrypt)
                    ThrowIfNotDirectory(LetsencryptPath);

            }
        }

    }
}