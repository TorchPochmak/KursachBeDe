using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;

namespace FarmMetricsClient.ViewModels.User.Pages
{
    public class FarmDetailsViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private readonly int _farmId;
        private readonly Action _onBack;
        private ApiClient.Farm _farm;

        public ApiClient.Farm Farm
        {
            get => _farm;
            set { _farm = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }

        public FarmDetailsViewModel(int farmId, Action onBack)
        {
            _apiClient = new ApiClient("http://localhost:5148/");
            _farmId = farmId;
            _onBack = onBack;
            BackCommand = new RelayCommand(_ => _onBack());

            _ = LoadFarm();
        }

        private async Task LoadFarm()
        {
            Farm = await _apiClient.GetFarmDetailsAsync(_farmId) ?? new ApiClient.Farm();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
