using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NC.DownloadClevo.Core
{
    public class ModelDrivers
    {
        public string Model { get; set; }

        /// <summary>
        /// Drivers that this model requires
        /// </summary>
        public List<DriverChoice> Drivers { get; set; } = new();
    }
}
