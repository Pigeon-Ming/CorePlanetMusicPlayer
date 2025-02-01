using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class ExceptionManager
    {
        public event EventHandler CorePMPException;
        public void ThrowException(String Content)
        {
            CorePMPException?.Invoke(Content,null);
        }
    }
}
