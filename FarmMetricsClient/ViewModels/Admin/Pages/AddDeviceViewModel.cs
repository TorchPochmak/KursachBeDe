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

namespace FarmMetricsClient.ViewModels.Admin.Pages
{
    public class AddDeviceViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private readonly int _settlementId;
        private Metric _selectedMetric;
        private string _minValue;
        private string _maxValue;

        public ObservableCollection<Metric> Metrics { get; } = new();
        public Action? CloseAction { get; set; }

        private ObservableCollection<Metric> _availableMetrics = new();
        public ObservableCollection<Metric> AvailableMetrics
        {
            get => _availableMetrics;
            set => SetField(ref _availableMetrics, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetField(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasErrorMessage));
                }
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);


        public Metric SelectedMetric
        {
            get => _selectedMetric;
            set => SetField(ref _selectedMetric, value);
        }

        public string MinValue
        {
            get => _minValue;
            set => SetField(ref _minValue, value);
        }

        public string MaxValue
        {
            get => _maxValue;
            set => SetField(ref _maxValue, value);
        }

        public AddDeviceViewModel(int settlementId)
        {
            _apiClient = new ApiClient("http://localhost:5148/");
            _settlementId = settlementId;

            _ = LoadMetricsAsync();
        }
        private RelayCommand _addDeviceCommand;
        public ICommand AddDeviceCommand => _addDeviceCommand ??= new RelayCommand(async _ => await AddDevice());
        private async Task LoadMetricsAsync()
        {
            try
            {
                var allMetrics = await _apiClient.GetAllMetricsAsync();
                var existingDevices = await _apiClient.GetDevicesBySettlementAsync(_settlementId);

                var availableMetrics = allMetrics
                    .Where(m => !existingDevices.Any(d => d.MetricName == m.Name))
                    .ToList();

                AvailableMetrics.Clear();
                foreach (var metric in availableMetrics)
                {
                    AvailableMetrics.Add(metric);
                }

                if (AvailableMetrics.Any())
                {
                    SelectedMetric = AvailableMetrics.First();
                }
                else
                {
                    ErrorMessage = "Все доступные метрики уже добавлены в этот населенный пункт";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке метрик: {ex.Message}";
                Console.WriteLine(ErrorMessage);
            }
        }
        public async Task AddDevice()
        {
            try
            {
                var device = new SettleMetricDevice
                {
                    MetricId = SelectedMetric.Id,
                    SettlementId = _settlementId,
                    RegisteredAt = DateTime.UtcNow,
                    MinValue = double.TryParse(MinValue, out var minVal) ? minVal : SelectedMetric.MinValue,
                    MaxValue = double.TryParse(MaxValue, out var maxVal) ? maxVal : SelectedMetric.MaxValue
                };

                var success = await _apiClient.AddDeviceAsync(device);
                if (success)
                {
                    CloseAction?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении устройства: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}