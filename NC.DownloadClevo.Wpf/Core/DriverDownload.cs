using Aria2NET;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NC.DownloadClevo.Core
{
    public class DriverDownload : PropertyChangedBase
    {
        public Driver Driver { get; set; }

        public long Size { get; set; }

        /// <summary>
        /// Download Progress
        /// </summary>
        public double DownloadProgress { get; set; } = 0;

        public long Downloaded { get; set; }

        /// <summary>
        /// Download Speed in MB/s
        /// </summary>
        public double DownloadSpeed { get; set; }

        public string DownloadUrl { get; set; }

        /// <summary>
        /// Whether the driver could be downloaded
        /// </summary>
        public bool IsDownloadError { get; set; }

        public string GetDownloadFilename()
        {
            var driverFolder = Path.Combine(Util.AppDataFolder, "drivers");
            var file = Path.Combine(driverFolder, Path.GetFileNameWithoutExtension(this.Driver.FileName) + "-" + this.Driver.DriverHash + ".zip");
            var oldFile = Path.Combine(driverFolder, this.Driver.DriverHash);

            if (File.Exists(oldFile))
            {
                File.Move(oldFile, file);
            }

            return file;
        }

        public bool CheckExist()
        {
            var file = this.GetDownloadFilename();
            if (File.Exists(file))
            {
                if (this.Size == (new FileInfo(file)).Length)
                {
                    this.DownloadProgress = 1;
                    _ = this.OnPropertyChanged();
                    return true; //Already downloaded
                }
            }

            return false;
        }

        public Task DiscoverSize()
        {
            if (this.DownloadUrl != null && this.Size > 0)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                var client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Head, this.Driver.USDownloadLink);
                var response = client.Send(req);

                this.DownloadUrl = req.RequestUri.OriginalString;
                this.Size = response.Content.Headers.ContentLength ?? 0;
                _ = this.OnPropertyChanged();
            });
        }

        public Task Download(CancellationToken cancel)
        {
            return Task.Run(() =>
            {
                this.DiscoverSize().Wait();

                if (this.CheckExist() == true)
                {
                    this.DownloadProgress = 1;
                    _ = this.OnPropertyChanged(nameof(DownloadProgress));
                    return;
                }

                var file = this.GetDownloadFilename();

                var completed = 0L;

                var speedSum = 0d;
                var count = 0d;

                try
                {
                    var client = new HttpClient();
                    using (var s = client.GetStreamAsync(this.DownloadUrl).Result)
                    using (var fs = File.OpenWrite(file))
                    {
                        byte[] buffer = new byte[256 * 1024];
                        var lastTime = DateTime.UtcNow;
                        int read;
                        while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, read);

                            completed += read;

                            var timeTaken = DateTime.UtcNow.Subtract(lastTime).TotalSeconds;
                            lastTime = DateTime.UtcNow;

                            var speed = (read / 1048576d) / timeTaken;
                            speedSum += speed;
                            count++;

                            if (count % 10 == 0)
                            {
                                this.DownloadSpeed = speedSum / count;
                                _ = this.OnPropertyChanged(nameof(DownloadSpeed));
                            }
                            if (this.Size > 0)
                            {
                                this.DownloadProgress = completed / (double)this.Size;
                                _ = this.OnPropertyChanged(nameof(DownloadProgress));
                            }

                            if (cancel.IsCancellationRequested)
                            {
                                return;
                            }
                        }

                    }
                }
                catch (Exception)
                {
                    this.IsDownloadError = true;
                }
            });
        }

        public DriverDownload() { }

        public DriverDownload(Driver d)
        {
            this.Driver = d;

            if (this.Driver.DriverHash == null)
            {
                this.Driver.DriverHash = this.Driver.USDownloadLink.GetSHA256();
            }

            this.DownloadUrl = this.Driver.DownloadUrl;
            this.Size = this.Driver.Size;
        }
    }
}
