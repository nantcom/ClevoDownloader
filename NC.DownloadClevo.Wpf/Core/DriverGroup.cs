using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NC.DownloadClevo.Core
{
    public class DriverGroup
    {
        public string GroupName { get; set; }

        public Driver Newest { get; set; }

        public List<Driver> Drivers { get; set; } = new();

        /// <summary>
        /// Model Names
        /// </summary>
        public IEnumerable<string> Models
        {
            get
            {
                return this.Drivers.Select(d => d.ModelName ).Distinct();
            }
        }

        /// <summary>
        /// Series Name
        /// </summary>
        public IEnumerable<string> Series
        {
            get
            {
                return this.Drivers.Select(d => d.SeriesName).Distinct();
            }
        }

        /// <summary>
        /// Sample model, description for debugging
        /// </summary>
        public IEnumerable<string> Sample
        {
            get
            {
                foreach (var item in this.Drivers)
                {
                    yield return $"{item.ModelName} - {item.Description} (Filename: {item.FileName})";
                }
            }
        }
    }
}
