using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer6.Composition
{
    public static class AppRuntime
    {
        public static AppBootstrapper Bootstrapper { get; set; }

        public static AppServices Services
        {
            get
            {
                if (Bootstrapper == null)
                {
                    return null;
                }

                return Bootstrapper.Services;
            }
        }

        public static bool IsInitialized
        {
            get
            {
                return Bootstrapper != null &&
                       Bootstrapper.Services != null;
            }
        }
    }
}
