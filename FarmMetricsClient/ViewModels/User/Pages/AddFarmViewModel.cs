using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;
using static FarmMetricsClient.Services.ApiClient;

namespace FarmMetricsClient.ViewModels.User.Pages
{
    public class AddFarmViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private readonly int _userId;
        private readonly Action _onSuccess;
        private readonly Action _onCancel;

        private string _name = string.Empty;
        private string _settlementName = string.Empty;
        private string _statusMessage = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand CancelCommand { get; }

        public AddFarmViewModel(int userId, string settlementName, Action onSuccess, Action onCancel)
        {
            _userId = userId;
            _settlementName = settlementName;
            _apiClient = new ApiClient("http://localhost:5148/");
            _onSuccess = onSuccess;
            _onCancel = onCancel;

            AddCommand = new RelayCommand(async _ => await AddFarm());
            CancelCommand = new RelayCommand(_ => _onCancel?.Invoke());
        }

        private async Task AddFarm()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                StatusMessage = "Введите название участка";
                return;
            }

            try
            {
                var farm = new Farm
                {
                    Name = Name,
                    UserId = _userId,
                    Settlement = SettlementName,
                    Cultures = new List<Culture>(),
                    Metrics = new List<Metric>(),
                    Harvests = new List<Harvest>()
                };

                var response = await _apiClient.AddFarmAsync(farm);

                if (response.IsSuccessStatusCode)
                {
                    _onSuccess?.Invoke();
                }
                else
                {
                    StatusMessage = $"Ошибка: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}