using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;

namespace FarmMetricsClient.ViewModels.User.Pages
{

    // todo
    public class AddFarmViewModel : INotifyPropertyChanged
    {
        private readonly int _userId;
        private readonly ApiClient _apiClient;
        private readonly Action _onAdded;
        private readonly Action _onCancel;

        private string _name = "";
        private string _statusMessage = "";

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand AddFarmCommand { get; }
        public ICommand CancelCommand { get; }

        public AddFarmViewModel(int userId, Action onAdded, Action onCancel)
        {
            _userId = userId;
            _apiClient = new ApiClient("http://localhost:5148/");
            _onAdded = onAdded;
            _onCancel = onCancel;
            AddFarmCommand = new RelayCommand(async _ => await AddFarm());
            CancelCommand = new RelayCommand(_ => _onCancel?.Invoke());
        }

        private async Task AddFarm()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                StatusMessage = "Название участка обязательно";
                return;
            }

            var farm = new ApiClient.Farm
            {
                Name = Name,
            };

            var response = await _apiClient.AddFarmAsync(farm);
            if (response.IsSuccessStatusCode)
            {
                _onAdded?.Invoke();
            }
            else
            {
                StatusMessage = "Ошибка при добавлении участка";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
