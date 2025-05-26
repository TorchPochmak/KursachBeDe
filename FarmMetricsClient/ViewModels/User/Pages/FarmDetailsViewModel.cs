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
        private readonly int _farmId;
        private readonly ApiClient _apiClient;
        private readonly Action _onBack;

        private ApiClient.Farm? _farm;
        public ApiClient.Farm? Farm
        {
            get => _farm;
            set { _farm = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ApiClient.Culture> Cultures { get; } = new();
        public ObservableCollection<ApiClient.Metric> Metrics { get; } = new();
        public ObservableCollection<ApiClient.Harvest> Harvests { get; } = new();

        public ICommand BackCommand { get; }

        public FarmDetailsViewModel(int farmId, Action onBack)
        {
            _farmId = farmId;
            _apiClient = new ApiClient("http://localhost:5148/");
            _onBack = onBack;
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());

            _ = LoadFarmDetails();
        }

        private async Task LoadFarmDetails()
        {
            Farm = await _apiClient.GetFarmDetailsAsync(_farmId);
            Cultures.Clear();
            Metrics.Clear();
            Harvests.Clear();

            if (Farm != null)
            {
                foreach (var c in Farm.Cultures) Cultures.Add(c);
                foreach (var m in Farm.Metrics) Metrics.Add(m);
                foreach (var h in Farm.Harvests) Harvests.Add(h);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
