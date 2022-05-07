using DebounceThrottle;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NC.Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
                       var contains = string.IsNullOrEmpty(_filterModel) == false &&
                                       item.Key.ToLowerInvariant().Contains(_filterModel);
                       if (item.IsIncluded || contains)
                       {
                           return true;
                       }

                       return false;
                   })
                    .OrderBy( item => item.IsIncluded ? 1 : 0);
            }
        }

        public IEnumerable<Driver> DiscoveredDrivers
        {
            get
            {
                var selected = this.ClevoParser.Models.Where(m => m.IsIncluded)
                                    .Select(m => m.Key)
                                    .ToDictionary(m => m);

                return ClevoParser.Drivers
                                    .Where(d => selected.ContainsKey(d.ModelName))
                                    .OrderBy(d => d.FriendlyDisplay);

            }
        }

        /// <summary>
        /// List of Driver after grouping
        /// </summary>
        public IEnumerable<DriverGroup> MergedDrivers
        {
            get
            {
                var selected = this.ClevoParser.Models.Where(m => m.IsIncluded)
                                    .Select(m => m.Key)
                                    .ToDictionary(m => m);

                var groups = this.ClevoParser.Drivers
                                .Where(d => selected.ContainsKey(d.ModelName))
                                .ToLookup(d => d.DriverGroup);

                return groups.Select( g =>
                {
                    var newest = g.OrderBy(d => d.Date).Last();
                    return new DriverGroup()
                    {
                        Drivers = g.ToList(),
                        GroupName = newest.DriverGroup,
                        Newest = newest,
                    };

                }).OrderBy( d => d.GroupName);
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
                    case "Discovery":
                        this.OnPropertyChanged(nameof(DiscoveredDrivers));
                        break;
                    case "Merge":
                        this.OnPropertyChanged(nameof(MergedDrivers));
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

                        this.OnPropertyChanged(nameof(DiscoveredDrivers));
                    });
                }
            );

            this.Initialize();
        }

        private async void Initialize()
        {
            await this.ClevoParser.Load();

            _ = this.OnPropertyChanged();

        }
    }
}
