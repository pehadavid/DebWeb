using System.Text;
using System.Threading.Tasks;
using static DebWeb.EnvSettings;

namespace DebWeb
{
    public class SudoerTemplate : FileTemplate
    {
 

        public SudoerTemplate(SystemSettings sSettings, AppSettings settings ) : base(sSettings, settings)
        {
        }

        public override string GetFilePath()
        {
            return $"{systemSettings.SudoersPath}/{appSettings.ProjectName}";
        }

        public override string GetTemplate()
        {

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"%sudo ALL= NOPASSWD: /bin/systemctl start {appSettings.ProjectName}");
            builder.AppendLine($"%sudo ALL= NOPASSWD: /bin/systemctl stop {appSettings.ProjectName}");
            builder.AppendLine($"%sudo ALL= NOPASSWD: /bin/systemctl restart {appSettings.ProjectName}");
            return builder.ToString();

        }


    }
}