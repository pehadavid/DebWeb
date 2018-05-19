using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DebWeb
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!File.Exists("environment.json") || !File.Exists("application.json"))
            {
                Console.WriteLine($"environment.json or application.json could not be found. Aborting :o");
            }
            else
            {

                var systemSettingsContent = File.ReadAllText("environment.json");
                EnvSettings.SystemSettings systemSettings = JsonConvert.DeserializeObject<EnvSettings.SystemSettings>(systemSettingsContent);
                var appSettingsContent = File.ReadAllText("application.json");
                EnvSettings.AppSettings appSettings = JsonConvert.DeserializeObject<EnvSettings.AppSettings>(appSettingsContent);
                try
                {
                    appSettings.CheckConfiguration();
                    systemSettings.CheckConfiguration(appSettings);
                }
                catch (InvalidConfigurationException e)
                {
                    Console.WriteLine($"Aborting (configuration invalid): {e.Message}");
                    return ;
                }
                await RunAsync(systemSettings, appSettings);
            }
        }

        private static Task RunAsync(EnvSettings.SystemSettings systemSettings, EnvSettings.AppSettings appSettings)
        {
            throw new NotImplementedException();
        }


    }
}
