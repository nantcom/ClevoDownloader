using Aria2NET;
using DebounceThrottle;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NC.Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NC.DownloadClevo.Core
{
    public class ProjectData : PropertyChangedBase
    {
        public static ProjectData Default { get; } = new ProjectData();

        public ClevoParser ClevoParser { get; } = new ClevoParser();

        private DebounceDispatcher _filterDebounce = new DebounceDispatcher(200);
        private string _filterModel;
        public string FilterModel
        {
            get => _filterModel;
            set
            {
                _filterModel = value;
                _filterDebounce.Debounce( ()=>
                {
                    _ = this.OnPropertyChanged("Models");
                });
                
            }
        }

        public IEnumerable<Model> Models
        {
            get
            {
                return ClevoParser.Models.Where(item =>
                   {
                       if (string.IsNullOrEmpty(_filterModel))
                       {
                           return item.IsIncluded;
                       }

                       var contains = item.Key.ToLowerInvariant().Contains(_filterModel);
                       if (contains == true)
                       {
                           return true;
                       }

                       return false;
                   })
                    .OrderBy( item => item.IsIncluded ? 1 : 0);
            }
        }

        private ObservableCollection<ModelDrivers> _modelWithDrivers = new();

        public ObservableCollection<ModelDrivers> ModelWithDrivers
        {
            get
            {
                return _modelWithDrivers;
            }
        }

        /// <summary>
        /// List of Driver after grouping
        /// </summary>
        public IEnumerable<DriverGroup> MergedDrivers
        {
            get
            {
                return this.ClevoParser.MergedDrivers;
            }
        }

        /// <summary>
        /// List of Drivers to be download
        /// </summary>
        public IEnumerable<DriverDownload> DriverToDownload
        {
            get
            {
                return this.ClevoParser.DownloadList;
            }
        }

        public bool IsBusy { get; set; }

        public bool IsProgressUnknown { get; set; }

        public double ProgressValue { get; set; }

        public string ProgressText { get; set; } = "";

        public string ActiveTab
        {
            set
            {
                switch (value)
                {
                    case "Group":
                        _ = this.OnPropertyChanged(nameof(MergedDrivers));
                        break;
                    case "Select":
                        this.BuildModelDrivers();
                        break;
                    case "Download":
                        this.BuildDriverDownloadList();
                        break;
                    default:
                        break;
                }
            }
        }

        public string NormalizeScript
        {
            get => this.ClevoParser.NormalizeScript;
            set
            {
                this.ClevoParser.NormalizeScript = value;
            }
        }

        public MetroWindow MainWindow { get; set; }

        public BasicCommand RefreshModel { get; }

        public BasicCommand PerformGrouping { get; }

        public BasicCommand DownloadDrivers { get; }

        private DebounceDispatcher _saveDebounce = new DebounceDispatcher(1000);
        public BasicCommand Save { get; }

        private ThrottleDispatcher _progressThrottle = new ThrottleDispatcher(500);
        private void HandleProgress(string message, bool unknown, int total, int finished )
        {
            _progressThrottle.Throttle(() =>
            {
                this.ProgressText = message;
                this.ProgressValue = unknown || total <= 0 ? 0 : finished / (double)total;
                this.IsProgressUnknown = unknown;

                _ = this.OnPropertyChanged(nameof(ProgressText));
                _ = this.OnPropertyChanged(nameof(ProgressValue));
                _ = this.OnPropertyChanged(nameof(IsProgressUnknown));
            });
        }

        public ProjectData()
        {
            this.RefreshModel = new BasicCommand(
                async (parameter) =>
                {
                    this.IsBusy = true;

                    await this.ClevoParser.DiscoverModels(this.HandleProgress);
                    await this.ClevoParser.DiscoverDrivers(this.HandleProgress);

                    this.IsBusy = false;
                    _ = this.OnPropertyChanged("Models");
                }
            );

            this.PerformGrouping = new BasicCommand(
                async (parameter) =>
                {
                    var devScript = @"D:\NantCom\NantCom - Dev\Nant\NC.DownloadClevo\NC.DownloadClevo.Wpf\GroupingScript.js";
                    if (File.Exists(devScript))
                    {
                        this.NormalizeScript = File.ReadAllText(devScript);
                    }

                    this.IsBusy = true;
                    this.PerformGrouping.OnCanExecuteChanged();

                    await this.ClevoParser.PerformDriverGrouping(this.HandleProgress);

                    this.IsBusy = false;
                    this.PerformGrouping.OnCanExecuteChanged();

                    this.OnPropertyChanged(nameof(MergedDrivers));

                    this.Save.Execute(null);
                },
                (parameter) =>  this.IsBusy == false
            );

            this.Save = new BasicCommand(
                async (parameter) =>
                {
                    _saveDebounce.Debounce( ()=>
                    {
                        this.IsProgressUnknown = true;
                        this.ProgressText = "Saving...";
                        this.OnPropertyChanged(nameof(ProgressText));
                        this.OnPropertyChanged(nameof(IsProgressUnknown));

                        _ = this.ClevoParser.Save();

                        this.IsProgressUnknown = false;
                        this.ProgressText = "Saved.";
                        this.OnPropertyChanged(nameof(ProgressText));
                        this.OnPropertyChanged(nameof(IsProgressUnknown));

                    });
                }
            );

            this.DownloadDrivers = new BasicCommand(
                (parameter) =>
                {
                    this.DownloadAllDrivers();
                }
            );

            this.Initialize();
        }

        private async void Initialize()
        {
            await this.ClevoParser.Load();

            _ = this.OnPropertyChanged();

        }

        private async void BuildModelDrivers()
        {
            this.HandleProgress("Creating UI", false,
                this.ClevoParser.ModelDrivers.Count, 0);

            _modelWithDrivers = new ObservableCollection<ModelDrivers>();
            await this.OnPropertyChanged(nameof(ModelWithDrivers));
            await Task.Delay(100);

            await this.ClevoParser.BuildModelDrivers(this.HandleProgress);


            foreach (var item in this.ClevoParser.ModelDrivers)
            {
                _modelWithDrivers.Add(item);

                await Task.Delay(100);

                this.HandleProgress("Creating UI", false,
                    this.ClevoParser.ModelDrivers.Count, _modelWithDrivers.Count);
            }

            this.HandleProgress("Done", false, 1, 1);

        }

        private async void BuildDriverDownloadList()
        {
            await this.ClevoParser.BuildDriverDownloadList(this.HandleProgress);
            await this.OnPropertyChanged(nameof(DriverToDownload));
        }

        private  void DownloadAllDrivers()
        {
            Task.Run(() =>
            {
                Parallel.ForEach(this.DriverToDownload, driver =>
                {
                    driver.Download(CancellationToken.None).Wait();
                });

            });
        }
    }
}
