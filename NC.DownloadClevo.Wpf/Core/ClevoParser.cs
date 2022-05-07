using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Globalization;
using Fizzler.Systems.HtmlAgilityPack;
using RestSharp;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Newtonsoft.Json;
using System.Threading;
using NC.Lib;

namespace NC.DownloadClevo.Core
{
    public class ClevoParser
    {
        public delegate void ProgressCallback(string message, bool isUnknown = true, int total = 0, int finished = 0);

        /// <summary>
        /// List of Models to Download the Driver
        /// </summary>
        public List<Model> Models { get; set; } = new();

        /// <summary>
        /// List of Drivers Found
        /// </summary>
        public List<Driver> Drivers { get; set; } = new();

        /// <summary>
        /// Javascript which will be used to group the driver
        /// </summary>
        public string NormalizeScript { get; set; } = string.Empty;

        private string normalizeFileName(string s)
        {
            var output = new string(s.Replace("_D_", "").Where(c => char.IsLetter(c)).ToArray());
            return output;
        }

        private string Normalize(Driver input)
        {
            try
            {
                var exe = new JIntExecutor();
                var result = exe.Eval(this.NormalizeScript,
                    targetType: typeof(string),
                    inputValue: input);

                return (string)result;
            }
            catch (Exception ex)
            {
                return $"'{input.Description}' from {input.ModelName} could not be normalized, : {ex.Message}";
            }
        }

        private IEnumerable<Driver> ParsePage( Model entry )
        {
            var modelName = entry.Key;
            var seriesName = entry.Series;
            var url = entry.Url;

            var client = new RestClient("https://www.clevo.com.tw");
            var modelPageRequest = new RestRequest(url);
            var modelPageResponse = client.ExecuteAsync(modelPageRequest).Result;
            var modelPage = new HtmlDocument();
            modelPage.LoadHtml(modelPageResponse.Content);

            // get the TRs
            var drivers = modelPage.DocumentNode.QuerySelectorAll("table[bordercolorlight='#FFFFCC'] tr[valign='top']");

            //var results = new List<Driver>();

            foreach (var tr in drivers)
            {
                var tds = tr.QuerySelectorAll("td").ToList();
                var driver = new Driver();
                driver.Date = DateTime.Parse(tds[6].InnerText.Trim(), CultureInfo.InvariantCulture);
                driver.Description = tds[1].InnerText.Trim();
                driver.FileName = tds[2].InnerText.Trim();
                driver.USDownloadLink = "https://www.clevo.com.tw/en/e-services/download" + tds[3].QuerySelector("a").Attributes["href"].Value;
                driver.Version = tds[4].InnerText.Trim();
                driver.ModelName = modelName;
                driver.SeriesName = seriesName;

                //results.Add(driver);
                yield return driver;
            }

            //var groups = results.ToLookup(d => normalizeFileName(d.FileName));

            //foreach (var g in groups)
            //{
            //    yield return g.OrderBy(d => d.Date).Last();
            //}
        }

        public async Task Load()
        {
            var json = await Util.LoadString("clevoparser.json");
            if (string.IsNullOrEmpty(json) == false)
            {
                try
                {
                    JsonConvert.PopulateObject(json, this);
                }
                catch (Exception)
                {
                }
            }

            if (this.Models.Any( m => m.IsIncluded ) == false)
            {
                var defaultList = new string[] { "NH5xHP(Q)", "NH5x_7xHPx", "NH5x_7xHPx", "NV4xMJ_K(-D)", "NPxxPNK_J_H", "NPxxPNP", "PDxxPNx(-D)(-G)", "V1xxPNK_J_H(Q)", "V1xxPNP(Q)" };
                var selected = defaultList.ToHashSet();

                foreach (var item in this.Models)
                {
                    item.IsIncluded = selected.Contains(item.Key);
                }
            }
        }

        public async Task Save()
        {
            var json = JsonConvert.SerializeObject(this);
            await Util.SaveString("clevoparser.json", json);
        }

        /// <summary>
        /// Discover Models, this always spawn background thread
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Task DiscoverModels(ProgressCallback? callback)
        {
            return Task.Run(() =>
            {
                callback?.Invoke("Contacting Server...");

                var client = new RestClient("https://www.clevo.com.tw");
                var modelPageRequest = new RestRequest("/en/e-services/Download/default.asp");

                var modelPageResponse = client.ExecuteAsync(modelPageRequest).Result;

                callback?.Invoke("Parsing Driver List Page...");

                var modelPage = new HtmlDocument();
                modelPage.LoadHtml(modelPageResponse.Content);

                var options = modelPage.DocumentNode.QuerySelectorAll("select[NAME='Lmodel'] > option");

                var selectedModels = this.Models.Where(m => m.IsIncluded)
                                        .Select(m => m.Key)
                                        .ToDictionary(k => k);

                this.Models = options.Select(opt => new Model()
                {
                    Key = opt.Attributes["VALUE"].Value,
                    Series = opt.InnerText,
                    Url = $"https://www.clevo.com.tw/en/e-services/download/ftpOut.asp?Lmodel={opt.Attributes["VALUE"].Value}&ltype=9&submit=+GO+",
                    IsIncluded = selectedModels.ContainsKey(opt.Attributes["VALUE"].Value)
                })
                                            .Where(m =>
                                            {
                                                return m.Key != null && string.IsNullOrEmpty(m.Key.Trim()) == false;
                                            })
                                            .ToList();

                callback?.Invoke("Saving Database", true, 1, 1);
                this.Save().Wait();

                callback?.Invoke("Model Refresh Complete", false, 1, 1);
            });
        }

        /// <summary>
        /// Discover Drivers from list, this always spwan new Thread
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Task DiscoverDrivers( ProgressCallback? callback )
        {
            return Task.Run(() =>
            {
                callback?.Invoke("Discovering Driver...");

                var finishedPage = 0;
                var list = new ConcurrentBag<Driver>();

                Parallel.ForEach(this.Models, (entry) =>
                {
                    foreach (var driver in this.ParsePage(entry))
                    {
                        list.Add(driver);

                        callback?.Invoke($"Discovering Driver...{list.Count} found",
                            false, this.Models.Count, finishedPage);
                    }

                    callback?.Invoke($"Discovering Driver...{list.Count} found",
                        false, this.Models.Count, Interlocked.Increment( ref finishedPage ) );
                });

                this.Drivers = list.OrderBy(d => d.Description).ToList();

                callback?.Invoke("Saving Database");
                this.Save().Wait();

                callback?.Invoke("Finished", false, 1, 1);

            });
        }

        public Task PerformDriverGrouping( ProgressCallback? callback )
        {
            return Task.Run(() =>
            {
                callback?.Invoke("Grouping Driver...");

                var selected = this.Models.Where(m => m.IsIncluded)
                    .Select(m => m.Key)
                    .ToDictionary(m => m);

                var includedDrivers = this.Drivers
                                        .Where(d => selected.ContainsKey(d.ModelName))
                                        .ToList();

                int completed = 0;

                Parallel.ForEach(includedDrivers, (d) =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    d.DriverGroup = this.Normalize(d);

                    callback?.Invoke("Grouping...", true,
                        includedDrivers.Count, Interlocked.Increment(ref completed));
                });

                callback?.Invoke("Grouping Completed.", false, 1, 1);
            });
        }
    }
}
