using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;
using FarmMetricsClient.Views;

namespace FarmMetricsClient.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private int? _userId;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _loginStatus = string.Empty;

        private readonly ApiClient _apiClient;
        private readonly Action<string, int?> _onLoginSuccessful;

        public int? UserId
        {
            get => _userId;
            private set => SetField(ref _userId, value);
        }

        public MainWindowViewModel(Action<string, int?> onLoginSuccessful)
        {
            _apiClient = new ApiClient("http://localhost:5148/"); 
            _onLoginSuccessful = onLoginSuccessful; 

            LoginCommand = new RelayCommand(async _ => await LoginAsync());
            OpenRegisterWindowCommand = new RelayCommand(_ => OpenRegisterWindow());
        }

        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        public string LoginStatus
        {
            get => _loginStatus;
            set => SetField(ref _loginStatus, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand OpenRegisterWindowCommand { get; }

        private async Task LoginAsync()
        {
            try
            {
                var response = await _apiClient.LoginAsync(Email, Password);

                if (response != null)
                {
                    Console.WriteLine($"[DEBUG] Авторизация прошла успешно для пользователя с ID: {response.UserId}");
                    UserId = response.UserId;
                    _onLoginSuccessful(response.Role, response.UserId);
                    LoginStatus = string.Empty;
                }
                else
                {
                    LoginStatus = "Неверные email или пароль.";
                }
            }
            catch (Exception ex)
            {
                LoginStatus = $"Ошибка подключения: {ex.Message}";
            }
        }

        private void OpenRegisterWindow()
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
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


    // Реализация команды (RelayCommand) для обработки действий UI
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);
    }
}
