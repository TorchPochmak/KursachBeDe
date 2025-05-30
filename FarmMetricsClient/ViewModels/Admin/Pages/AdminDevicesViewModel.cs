using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;
using FarmMetricsClient.Views.Admin.Pages;

namespace FarmMetricsClient.ViewModels.Admin.Pages
{
    public class AdminDevicesViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private int _settlementId;
        private string _settlementName = string.Empty;

        public ObservableCollection<DeviceItem> Devices { get; } = new();

        public string SettlementName
        {
            get => _settlementName;
            set
            {
                if (_settlementName != value)
                {
                    _settlementName = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminDevicesViewModel(int settlementId, string settlementName, Action goBackAction)
        {
            _apiClient = new ApiClient("http://localhost:5148/");
            _settlementId = settlementId;
            SettlementName = settlementName;
            GoBackAction = goBackAction;

            _ = LoadDevicesAsync();
        }

        public Action GoBackAction { get; }

        private async Task LoadDevicesAsync()
        {
            try
            {
                var devices = await _apiClient.GetDevicesBySettlementAsync(_settlementId);
                Devices.Clear();

                foreach (var device in devices)
                {
                    Devices.Add(new DeviceItem
                    {
                        Id = device.Id,
                        MetricName = device.MetricName,
                        RegisteredAt = device.RegisteredAt.ToString("g")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке устройств: {ex.Message}");
            }
        }

        public async Task AddDeviceAsync()
        {
            try
            {
                var addDeviceWindow = new AddDeviceWindow(_settlementId);
                addDeviceWindow.Closed += async (sender, args) =>
                {
                    await LoadDevicesAsync();
                };
                addDeviceWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при открытии окна добавления устройства: {ex.Message}");
            }
        }
        private RelayCommand _refreshCommand;
        public ICommand RefreshCommand => _refreshCommand ??= new RelayCommand(async _ => await LoadDevicesAsync()); public async Task DeleteDeviceAsync(int deviceId)
        {
            try
            {
                var success = await _apiClient.DeleteDeviceAsync(deviceId);
                if (success)
                {
                    await LoadDevicesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении устройства: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DeviceItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public string RegisteredAt { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}