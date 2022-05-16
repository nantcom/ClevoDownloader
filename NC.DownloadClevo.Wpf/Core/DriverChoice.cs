using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NC.DownloadClevo.Core
{
    public class DriverChoice
    {
        public string GroupName { get; set; }

        public string Model { get; set; }

        public Driver Newest { get; set; }

        public Driver NewestForModel { get; set; }
    }
}
