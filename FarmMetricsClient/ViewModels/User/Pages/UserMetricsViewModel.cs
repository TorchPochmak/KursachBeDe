using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;
using static FarmMetricsClient.Services.ApiClient;

namespace FarmMetricsClient.ViewModels.User.Pages
{
    public class UserMetricsViewModel : INotifyPropertyChanged
    {
        private readonly int _userId;
        private readonly ApiClient _apiClient;
        private bool _isLoading;
        private string _settlementName;

        public ObservableCollection<Settlement> Settlements { get; } = new ObservableCollection<Settlement>();
        private Settlement? _selectedSettlement;
        public Settlement? SelectedSettlement
        {
            get => _selectedSettlement;
            set
            {
                if (_selectedSettlement != value)
                {
                    _selectedSettlement = value;
                    OnPropertyChanged();
                    _ = OnSettlementChanged();
                }
            }
        }

        public ObservableCollection<Metric> AllMetrics { get; } = new ObservableCollection<Metric>();
        private Metric? _selectedMetric;
        public Metric? SelectedMetric
        {
            get => _selectedMetric;
            set
            {
                if (_selectedMetric != value)
                {
                    _selectedMetric = value;
                    OnPropertyChanged();
                    ApplyMetricFilter();
                }
            }
        }

        private List<MetricData> _allMetricData = new List<MetricData>();

        private List<Device> _currentDevices = new List<Device>();

        public ObservableCollection<MetricDataView> Metrics { get; } = new ObservableCollection<MetricDataView>();

        public string SettlementName
        {
            get => _settlementName;
            set
            {
                _settlementName = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool HasSettlement => SelectedSettlement != null;

        public ICommand GoBackCommand { get; }

        public UserMetricsViewModel(int userId, Action goBack)
        {
            _userId = userId;
            _apiClient = new ApiClient("http://localhost:5148/");
            GoBackCommand = new RelayCommand(_ => goBack());

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;

                var settlements = await _apiClient.GetSettlementsAsync();
                Settlements.Clear();
                foreach (var s in settlements.OrderBy(x => x.Name))
                    Settlements.Add(s);

                var profile = await _apiClient.GetUserProfileAsync(_userId);
                if (profile != null && profile.SettlementId.HasValue)
                {
                    SelectedSettlement = Settlements.FirstOrDefault(s => s.Id == profile.SettlementId.Value);
                }
                else if (Settlements.Count > 0)
                {
                    SelectedSettlement = Settlements[0];
                }
            }
            catch (Exception ex)
            {
                SettlementName = "Ошибка загрузки данных";
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnSettlementChanged()
        {
            if (SelectedSettlement == null)
            {
                SettlementName = "Не выбран населённый пункт";
                Metrics.Clear();
                AllMetrics.Clear();
                _allMetricData.Clear();
                _currentDevices.Clear();
                OnPropertyChanged(nameof(HasSettlement));
                return;
            }

            try
            {
                IsLoading = true;
                SettlementName = SelectedSettlement.Name;

                _currentDevices = await _apiClient.GetDevicesBySettlementAsync(SelectedSettlement.Id);
                var allMetricsList = await _apiClient.GetAllMetricsAsync();

                AllMetrics.Clear();
                foreach (var device in _currentDevices)
                {
                    var metric = allMetricsList.FirstOrDefault(m => m.Name == device.MetricName);
                    if (metric != null && !AllMetrics.Any(m => m.Id == metric.Id))
                        AllMetrics.Add(metric);
                }

                var metricDataList = await _apiClient.GetMetricsBySettlementAsync(SelectedSettlement.Id);
                _allMetricData = metricDataList;

                UpdateMetricsView(_currentDevices, _allMetricData);

                SelectedMetric = null;

                OnPropertyChanged(nameof(HasSettlement));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке метрик: {ex.Message}");
                SettlementName = "Ошибка загрузки данных";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyMetricFilter()
        {
            if (SelectedSettlement == null) return;

            var filtered = _allMetricData;
            if (SelectedMetric != null)
            {
                var metricDeviceIds = _currentDevices
                    .Where(d => d.MetricName == SelectedMetric.Name)
                    .Select(d => d.Id)
                    .ToHashSet();

                filtered = filtered
                    .Where(m => metricDeviceIds.Contains(m.SettleMetricDeviceId))
                    .ToList();
            }

            UpdateMetricsView(_currentDevices, filtered);
        }



        private void UpdateMetricsView(List<Device> devices, List<MetricData> data)
        {
            Metrics.Clear();
            var deviceDict = devices.ToDictionary(d => d.Id, d => d);

            foreach (var m in data.OrderByDescending(x => x.RegisteredAt))
            {
                string deviceName = "";
                if (deviceDict.TryGetValue(m.SettleMetricDeviceId, out var dev))
                    deviceName = dev.MetricName;

                Metrics.Add(new MetricDataView
                {
                    Id = m.Id,
                    RegisteredAt = m.RegisteredAt,
                    MetricValue = m.MetricValue,
                    DeviceName = deviceName
                });
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class MetricDataView
    {
        public int Id { get; set; }
        public DateTime RegisteredAt { get; set; }
        public double MetricValue { get; set; }
        public string DeviceName { get; set; } = "";
    }
}
