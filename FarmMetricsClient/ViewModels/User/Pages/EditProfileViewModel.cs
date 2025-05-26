using System;
using System.ComponentModel;
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

            if (response.IsSuccessStatusCode)
            {
                _onUpdateSuccess?.Invoke();
                _closeAction?.Invoke(); 
            }
            else
            {
                StatusMessage = "Ошибка при обновлении профиля";
            }
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