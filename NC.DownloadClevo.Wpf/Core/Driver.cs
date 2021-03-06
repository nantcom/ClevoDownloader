using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NC.DownloadClevo.Core
{
    public class Driver
    {
        public string Description { get; set; }

        public string FileName { get; set; }

        public string USDownloadLink { get; set; }

        public string Version { get; set; }

        public DateTime Date { get; set; }

        public bool IsUrlValid { get; set; }

        public string DownloadUrl { get; set; }

        public long Size { get; set; }

        /// <summary>
        /// Hash to globally identify this driver
        /// </summary>
        public string DriverHash { get; set; }

        /// <summary>
        /// Whether this driver is chosen to be downloaded
        /// </summary>
        public bool IsChosen { get; set; }

        /// <summary>
        /// Original Model that we found this driver
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Clevo Friendly Name of the Model
        /// </summary>
        public string SeriesName { get; set; }

        private string _friendlyDisplay;
        public string FriendlyDisplay
        {
            get
            {
                if (_friendlyDisplay != null)
                {
                    return _friendlyDisplay;
                }

                var input = this.Description;
                var stripedNumber = Regex.Replace(input, @"^\d\.\s?", "");

                var strippedPrefix = Regex.Replace(stripedNumber, @"^update\s", "", RegexOptions.IgnoreCase);
                strippedPrefix = Regex.Replace(strippedPrefix, @"^release\s", "", RegexOptions.IgnoreCase);

                var final = strippedPrefix;

                var matchVersionText = Regex.Match(final, @"\sdriver\s", RegexOptions.IgnoreCase);
                if (matchVersionText.Success)
                {
                    final = final.Substring(0, matchVersionText.Index);
                }

                var matchVersionTextControlCenter = Regex.Match(final, @"\sap version\s", RegexOptions.IgnoreCase);
                if (matchVersionTextControlCenter.Success)
                {
                    final = final.Substring(0, matchVersionTextControlCenter.Index);
                }

                var matchVersionTextControlCenter2 = Regex.Match(final, @"\sap\s", RegexOptions.IgnoreCase);
                if (matchVersionTextControlCenter2.Success)
                {
                    final = final.Substring(0, matchVersionTextControlCenter2.Index);
                }

                var matchVersionOnly = Regex.Match(final, @"\sversion\s", RegexOptions.IgnoreCase);
                if (matchVersionOnly.Success)
                {
                    final = final.Substring(0, matchVersionOnly.Index);
                }

                _friendlyDisplay = final.Trim();
                if (_friendlyDisplay.Length == 0)
                {
                    _friendlyDisplay = this.FileName;
                }

                return _friendlyDisplay;
            }
        }

        private string _parsedVersion;
        public string ParsedVersion
        {
            get
            {
                if (_parsedVersion != null)
                {
                    return _parsedVersion;
                }

                var match = Regex.Matches(this.Description, @"(?:(?:\s)|(?:\sv))(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)", RegexOptions.IgnoreCase);
                if (match.Count > 0)
                {
                    _parsedVersion = match.Last().Groups[1].Value;
                }
                else
                {
                    var matchNumberOnly = Regex.Matches(this.Description, @"(\d+\.\d+(?:\.\d+)?(?:\.\d+))", RegexOptions.IgnoreCase);
                    if (matchNumberOnly.Count > 0)
                    {
                        _parsedVersion = matchNumberOnly.Last().Groups[1].Value;
                    }
                }

                if (string.IsNullOrEmpty(_parsedVersion))
                {
                    _parsedVersion = this.Version;
                }

                if (string.IsNullOrEmpty(_parsedVersion))
                {
                    _parsedVersion = this.Date.ToString("yyyy-MM-dd");
                }

                return _parsedVersion;
            }
        }

        public string DriverGroup { get; set; }

        /// <summary>
        /// Test URL whether this driver can be downloaded
        /// </summary>
        /// <returns></returns>
        public Task TestUrl()
        {
            if (this.DownloadUrl != null && this.Size > 0)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                var client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Head, this.USDownloadLink);
                var response = client.Send(req);

                this.IsUrlValid = response.StatusCode == System.Net.HttpStatusCode.OK ||
                            response.StatusCode == System.Net.HttpStatusCode.NoContent;

                this.DownloadUrl = req.RequestUri.OriginalString;
                this.Size = response.Content.Headers.ContentLength ?? 0;
            });
        }
    }
}
