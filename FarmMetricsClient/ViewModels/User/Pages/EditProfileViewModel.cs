using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;

namespace FarmMetricsClient.ViewModels.User.Pages
{
    public class EditProfileViewModel : INotifyPropertyChanged
    {
        private readonly int _userId;
        private readonly ApiClient _apiClient;
        private readonly Action _onUpdateSuccess;

        private string _name;
        private string _email;
        private string _phone;
        private string _password;
        private string _confirmPassword;
        private string _statusMessage;

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetField(ref _phone, value);
        }

        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetField(ref _confirmPassword, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }
        private List<ApiClient.Settlement> _settlements;
        private ApiClient.Settlement _selectedSettlement;

        public List<ApiClient.Settlement> Settlements
        {
            get => _settlements;
            set => SetField(ref _settlements, value);
        }

        public ApiClient.Settlement? SelectedSettlement
        {
            get => _selectedSettlement;
            set => SetField(ref _selectedSettlement, value);
        }

        public ICommand UpdateProfileCommand { get; }
        private readonly Action _closeAction;

        public EditProfileViewModel(int userId, ApiClient.UserProfile profile,
                                  Action onUpdateSuccess, Action closeAction)
        {
            _userId = userId;
            _onUpdateSuccess = onUpdateSuccess;
            _closeAction = closeAction;
            _apiClient = new ApiClient("http://localhost:5148/");

            _name = profile.Name;
            _email = profile.Email;
            _phone = profile.Phone;

            UpdateProfileCommand = new RelayCommand(async _ => await UpdateProfile());
            Task.Run(() => LoadSettlements(profile.Settlement)).ConfigureAwait(false);
        }
        private async Task LoadSettlements(string currentSettlementName)
        {
            var settlements = await _apiClient.GetSettlementsAsync();

            Settlements = new List<ApiClient.Settlement>{new ApiClient.Settlement { Id = -1,
                Name = "Не выбрано" }}.Concat(settlements).ToList();

            if (!string.IsNullOrEmpty(currentSettlementName))
            {
                SelectedSettlement = Settlements.FirstOrDefault(s => s.Name == currentSettlementName);
            }
            else
            {
                SelectedSettlement = Settlements.FirstOrDefault(s => s.Id == -1);
            }
        }
        private async Task UpdateProfile()
        {
            if (!string.IsNullOrEmpty(Password) && Password != ConfirmPassword)
            {
                StatusMessage = "Пароли не совпадают";
                return;
            }

            var request = new ApiClient.UserUpdateRequest
            {
                Name = Name,
                Email = Email,
                Phone = Phone,
                Password = Password
            };

            var response = await _apiClient.UpdateUserProfileAsync(_userId, request);

            if (!response.IsSuccessStatusCode)
            {
                StatusMessage = "Ошибка при обновлении профиля";
                return;
            }

            if (SelectedSettlement != null)
            {
                if (SelectedSettlement.Id == -1)
                {
                    var removeResponse = await _apiClient.RemoveUserSettlementAsync(_userId);
                    if (!removeResponse.IsSuccessStatusCode)
                    {
                        StatusMessage = "Профиль обновлен, но не удалось удалить населенный пункт";
                        return;
                    }
                }
                else
                {
                    var settlementResponse = await _apiClient.UpdateUserSettlementAsync(_userId, SelectedSettlement.Id);
                    if (!settlementResponse.IsSuccessStatusCode)
                    {
                        StatusMessage = "Профиль обновлен, но не удалось изменить населенный пункт";
                        return;
                    }
                }
            }

            _onUpdateSuccess?.Invoke();
            _closeAction?.Invoke();
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}