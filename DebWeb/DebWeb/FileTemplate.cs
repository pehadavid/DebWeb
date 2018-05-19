using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static DebWeb.EnvSettings;

namespace DebWeb
{
    public abstract class FileTemplate
    {
        protected AppSettings appSettings;
        protected SystemSettings systemSettings;
        protected FileTemplate( SystemSettings sSettings, AppSettings settings)
        {
            this.appSettings = settings;
            this.systemSettings = sSettings;
        }
        public abstract string GetTemplate();
        protected Task WriteDataAsync(string path){
            var data = GetTemplate();
            return File.WriteAllTextAsync(path, data);
        }
        public virtual Task WriteFileAsync(){
            return this.WriteDataAsync(GetFilePath());
        }
        public abstract string GetFilePath();
        public static Dictionary<string, FileTemplate> GetAllGenerators(SystemSettings sysSettings,AppSettings settings) => new Dictionary<string, FileTemplate> {
               {"nginx", new NginxTemplate(sysSettings, settings)},
               {"systemd", new SystemdTemplate(sysSettings, settings)},
               {"sudoer", new SudoerTemplate(sysSettings, settings)}
           };
        

        

        
    }
}