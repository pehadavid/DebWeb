using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace DebWeb
{
    class Program
    {
        static async Task Main(string[] args)
        {

            ThrowIfNotRoot();

            Console.WriteLine("Welcome to Debweb, the ultimate webservice creator !");
            Console.WriteLine("######################################################");

            try
            {
                ThrowIfNoConfFile();
                ThrowIfNoOpenSSL();
                var systemSettingsContent = File.ReadAllText("environment.json");
                EnvSettings.SystemSettings systemSettings = JsonConvert.DeserializeObject<EnvSettings.SystemSettings>(systemSettingsContent);
                var appSettingsContent = File.ReadAllText("application.json");
                EnvSettings.AppSettings appSettings = JsonConvert.DeserializeObject<EnvSettings.AppSettings>(appSettingsContent);

                appSettings.CheckConfiguration();
                systemSettings.CheckConfiguration(appSettings);
                Console.WriteLine("Verifying DH parameters ... (this may take a while)");
                DHparamsGenerator.VerifyParams();
                await StartAsync(systemSettings, appSettings);
            }
            catch (InvalidConfigurationException e)
            {
                Console.WriteLine($"Aborting (configuration invalid): {e.Message}");
                return;
            }

        }

        private static void ThrowIfNoOpenSSL()
        {
            var osslResult = "which openssl".Bash();
            if (!osslResult.Contains("openssl"))
                throw new InvalidConfigurationException("OpenSSL is not installed.");
        }

        private static Task StartAsync(EnvSettings.SystemSettings systemSettings, EnvSettings.AppSettings appSettings)
        {
            if (File.Exists($"{systemSettings.SystemdPath}/{appSettings.ProjectName}.service"))
            {
                Console.Write($"A project named {appSettings.ProjectName} is already configured. [A]bort (default),[R]econfigure,[D]elete : ");
                var key = Console.ReadKey();
                Console.WriteLine(string.Empty);
                switch (key.Key)
                {

                    case ConsoleKey.R:
                        return ReconfigureAsync(systemSettings, appSettings);
                    case ConsoleKey.D:
                        return DeleteServiceAsync(systemSettings, appSettings);
                    default:
                    case ConsoleKey.A:
                        return Task.CompletedTask;

                }


            }
            else
            {
                return CreateServiceAsync(systemSettings, appSettings);
            }
        }

        private static Task DeleteServiceAsync(EnvSettings.SystemSettings systemSettings, EnvSettings.AppSettings appSettings)
        {


            "service nginx reload".Bash();
            $"systemctl stop {appSettings.ProjectName}".Bash();
            var generators = FileTemplate.GetAllGenerators(systemSettings, appSettings);
            foreach (var gen in generators)
            {
                File.Delete(gen.Value.GetFilePath());
            }
            return Task.CompletedTask;

        }

        private static Task ReconfigureAsync(EnvSettings.SystemSettings systemSettings, EnvSettings.AppSettings appSettings)
        {
            File.Delete($"{systemSettings.SitesEnabledNginx}/{appSettings.ProjectName}");
            "service nginx reload".Bash();
            $"systemctl stop {appSettings.ProjectName}".Bash();
            return CreateServiceAsync(systemSettings, appSettings);
        }

        private static void ThrowIfNotRoot()
        {
            var whoami = "whoami".Bash().Trim();
            if (whoami != "root")
            {
                throw new InvalidConfigurationException("cannot start DebWeb, you must have root privileges.");
            }
        }

        private static void ThrowIfNoConfFile()
        {
            if (!File.Exists("environment.json"))
                throw new InvalidConfigurationException("environment.json could not be found.");
            if (!File.Exists("application.json"))
                throw new InvalidConfigurationException("application.json could not be found.");

        }
        private static async Task CreateServiceAsync(EnvSettings.SystemSettings systemSettings, EnvSettings.AppSettings appSettings)
        {
            Console.WriteLine($"Creating system file for {appSettings.ProjectName} ...");
            var files = await CreateFiles(systemSettings, appSettings);
            foreach (var file in files)
            {
                Console.WriteLine($"{file} written ....");
            }

            ReviewFiles(files);
            if (MustRollback(files))
            {
                foreach (var file in files)
                {
                    File.Delete(file);
                    Console.WriteLine($"{file} deleted !");
                    return;
                }
            }
            else
            {
                StartServices(systemSettings, appSettings);
                SetupSsl(systemSettings, appSettings);
            }

        }

        private static void SetupSsl(EnvSettings.SystemSettings systemSettings, EnvSettings.AppSettings appSettings)
        {
            if (appSettings.UseLetsencrypt)
            {
                LetsencryptGenerator.GenerateCert(systemSettings, appSettings);
                NginxTemplate nginxTemplate = new NginxTemplate(systemSettings, appSettings, true);
                nginxTemplate.WriteFileAsync();
                "service nginx reload".Bash();
            }
        }

        private static bool MustRollback(IEnumerable<string> files)
        {
            Console.Write("If you want to cancel the process, you can press 'c' to rollback (generated files will be deleted), otherwise press any other key to continue and start services : ");
            var cancelKey = Console.ReadKey();
            return (cancelKey.Key == ConsoleKey.C);

        }

        private static void ReviewFiles(IEnumerable<string> files)
        {
            var arrayFile = files.ToArray();
            bool reviewDone = false;
            while (!reviewDone)
            {
                Console.WriteLine("File Reviewer");
                Console.WriteLine("###############");
                for (int i = 0; i < arrayFile.Length; i++)
                {
                    Console.WriteLine($"[{i + 1}] {arrayFile[i]}");
                }
                Console.WriteLine("###############");
                Console.Write("Choose a file by index, or press enter to continue : ");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter)
                    reviewDone = true;

                Console.WriteLine(string.Empty);

                var numValue = Convert.ToInt32(char.GetNumericValue(key.KeyChar));
                if (numValue > 0 && numValue < 4)
                {
                    var textFile = File.ReadAllText(arrayFile[numValue - 1]);
                    Console.WriteLine(textFile);
                }
            }


        }

        private static void StartServices(EnvSettings.SystemSettings systemSettings, EnvSettings.AppSettings appSettings)
        {
            $"systemctl start {appSettings.ProjectName}".Bash();
            $"ln -s {systemSettings.SitesAvailableNginx}/{appSettings.ProjectName} {systemSettings.SitesEnabledNginx}/{appSettings.ProjectName}".Bash();
            "service nginx reload".Bash();

        }

        private static async Task<IEnumerable<string>> CreateFiles(EnvSettings.SystemSettings systemSettings, EnvSettings.AppSettings appSettings)
        {
            var allGenerators = FileTemplate.GetAllGenerators(systemSettings, appSettings);
            List<Task> genTasks = new List<Task>();
            foreach (var item in allGenerators)
            {
                genTasks.Add(item.Value.WriteFileAsync());
            }

            await Task.WhenAll(genTasks);
            return allGenerators.Select(x => x.Value.GetFilePath());
        }
    }
}
