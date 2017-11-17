using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Cake.Deploy.Bot.LUIS
{
    public static class VersionExtension
    {
        public static Version Add(this Version version1, Version version2)
        {
            var major = version1.Major + version2.Major < 0 ? 0 : version1.Major + version2.Major;
            var minor = version1.Minor + version2.Minor < 0 ? 0 : version1.Minor + version2.Minor;
            var build = version1.Build + version2.Build < 0 ? -1 : version1.Build + version2.Build;
            var revision = version1.Revision + version2.Revision < 0 ? -1 : version1.Revision + version2.Revision;

            return new Version(major, minor);
        }
    }
}
