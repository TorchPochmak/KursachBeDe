using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;
using System.Linq;
using System.Runtime.CompilerServices;
using System;

namespace FarmMetricsClient.ViewModels.Admin.Pages
{
    public class AdminSettlementsViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        public ObservableCollection<ApiClient.Settlement> Settlements { get; } = new();
        public ObservableCollection<ApiClient.SettleMetricDevice> Devices { get; } = new();
        private ApiClient.Settlement? _selectedSettlement;

        public string NewSettlementName { get; set; } = "";

        public ApiClient.Settlement? SelectedSettlement
        {
            get => _selectedSettlement;
            set
            {
                _selectedSettlement = value;
                OnPropertyChanged();
                if (value != null) LoadDevicesAsync(value.Id);
            }
        }

        public ICommand AddSettlementCommand { get; }
        public ICommand AddDeviceCommand { get; }
        public ICommand DeleteDeviceCommand { get; }

        // Для добавления устройства
        public int NewDeviceMetricId { get; set; }
        public DateTime NewDeviceRegisteredAt { get; set; } = DateTime.Now;

        public AdminSettlementsViewModel()
        {
            _apiClient = new ApiClient("http://localhost:5148/");
            AddSettlementCommand = new RelayCommand(async _ => await AddSettlementAsync());
            AddDeviceCommand = new RelayCommand(async _ => await AddDeviceAsync());
            DeleteDeviceCommand = new RelayCommand(async id => await DeleteDeviceAsync((int)id));
            _ = LoadSettlementsAsync();
        }

        public async Task LoadSettlementsAsync()
        {
            var settlements = await _apiClient.GetSettlementsAsync();
            Settlements.Clear();
            foreach (var s in settlements)
                Settlements.Add(s);
        }

        public async Task AddSettlementAsync()
        {
            if (string.IsNullOrWhiteSpace(NewSettlementName)) return;
            var added = await _apiClient.AddSettlementAsync(NewSettlementName);
            if (added != null)
            {
                Settlements.Add(added);
                NewSettlementName = "";
                OnPropertyChanged(nameof(NewSettlementName));
            }
        }

        public async Task LoadDevicesAsync(int settlementId)
        {
            var devs = await _apiClient.GetDevicesBySettlementAsync(settlementId);
            Devices.Clear();
            foreach (var d in devs)
                Devices.Add(d);
        }

        public async Task AddDeviceAsync()
        {
            if (SelectedSettlement == null) return;
            var device = new ApiClient.SettleMetricDevice
            {
                SettlementId = SelectedSettlement.Id,
                MetricId = NewDeviceMetricId,
                RegisteredAt = NewDeviceRegisteredAt
            };
            var added = await _apiClient.AddDeviceAsync(device);
            if (added != null)
                Devices.Add(added);
        }

        public async Task DeleteDeviceAsync(int deviceId)
        {
            if (await _apiClient.DeleteDeviceAsync(deviceId))
                Devices.Remove(Devices.First(d => d.Id == deviceId));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string prop = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

}
