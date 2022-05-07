using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NC.DownloadClevo.Core
{
    public class Model
    {
        public string Key { get; set; }

        public string Series { get; set; }

        public string Url { get; set; }

        public bool IsIncluded { get; set; }
    }
}
