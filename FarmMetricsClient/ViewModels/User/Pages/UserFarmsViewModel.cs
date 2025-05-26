using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;

namespace FarmMetricsClient.ViewModels.User.Pages
{
    public class UserFarmsViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private readonly int _userId;
        private string _filterText = string.Empty;
        private readonly Action<int> _showFarmDetails;

        public ObservableCollection<ApiClient.Farm> Farms { get; } = new();

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged();
                    _ = LoadFarmsAsync();
                }
            }
        }

        public ICommand ReloadFarmsCommand { get; }
        public ICommand AddFarmCommand { get; }
        public ICommand DeleteFarmCommand { get; }
        public ICommand OpenFarmDetailsCommand { get; }

        public UserFarmsViewModel(int userId, Action showAddFarmDialog, Action<int> showFarmDetails)
        {
            _userId = userId;
            _apiClient = new ApiClient("http://localhost:5148/");
            _showFarmDetails = showFarmDetails;

            ReloadFarmsCommand = new RelayCommand(async _ => await ReloadFarms());
            AddFarmCommand = new RelayCommand(_ => showAddFarmDialog());
            DeleteFarmCommand = new RelayCommand(async farmId =>
            {
                if (farmId is int id)
                {
                    await DeleteFarmAsync(id);
                }
            });
            OpenFarmDetailsCommand = new RelayCommand(farmId =>
            {
                if (farmId is int id)
                {
                    _showFarmDetails(id);
                }
            });

            _ = LoadFarmsAsync();
        }

        public async Task LoadFarmsAsync()
        {
            try
            {
                var farms = await _apiClient.GetUserFarmsAsync(_userId);

                Farms.Clear();
                if (farms != null)
                {
                    foreach (var farm in farms)
                    {
                        if (string.IsNullOrEmpty(FilterText) ||
                            farm.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                        {
                            Farms.Add(farm);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке участков: {ex.Message}");
            }
        }

        private async Task DeleteFarmAsync(int farmId)
        {
            var response = await _apiClient.DeleteFarmAsync(farmId);
            if (response.IsSuccessStatusCode)
            {
                await ReloadFarms();
            }
        }

        public async Task ReloadFarms()
        {
            await LoadFarmsAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
