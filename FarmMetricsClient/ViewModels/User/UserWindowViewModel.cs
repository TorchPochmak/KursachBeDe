using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;
using FarmMetricsClient.ViewModels.User.Pages;
using static FarmMetricsClient.Services.ApiClient;

namespace FarmMetricsClient.ViewModels.User
{
    public class UserWindowViewModel : INotifyPropertyChanged
    {
        private readonly int _userId;
        private object _currentView;
        private UserProfile _userProfile;

        public UserProfile UserProfile
        {
            get => _userProfile;
            set
            {
                _userProfile = value;
                OnPropertyChanged();
            }
        }

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowEditProfileCommand { get; }
        public ICommand ShowFarmsCommand { get; }
        public ICommand ShowHarvestsCommand { get; }
        public ICommand ShowDeleteAccountCommand { get; }
        public ICommand LogoutCommand { get; }

        public ICommand ShowMetricsCommand { get; }

        private readonly ApiClient _apiClient;
        private readonly Action _onLogoutCallback;

        public UserWindowViewModel(int userId, Action onLogoutCallback)
        {
            _userId = userId;
            _onLogoutCallback = onLogoutCallback;
            _apiClient = new ApiClient("http://localhost:5148/");

            ShowEditProfileCommand = new RelayCommand(_ => ShowEditProfile());
            ShowFarmsCommand = new RelayCommand(_ => ShowFarms());
            ShowDeleteAccountCommand = new RelayCommand(async _ => await DeleteAccount());
            LogoutCommand = new RelayCommand(_ => _onLogoutCallback());
            ShowMetricsCommand = new RelayCommand(_ => ShowMetrics());
            _ = LoadUserProfile();

            CurrentView = null;
        }
        private void ShowMetrics()
        {
            CurrentView = new UserMetricsViewModel(
                _userId,
                () => CurrentView = null
            );
        }


        private async Task LoadUserProfile()
        {
            UserProfile = await _apiClient.GetUserProfileAsync(_userId);

            Console.WriteLine($"User Profile Loaded - SettlementId: {UserProfile?.SettlementId}, Settlement: {UserProfile?.Settlement}");
        }

        private void ShowEditProfile()
        {
            CurrentView = new EditProfileViewModel(
                _userId,
                UserProfile,
                async () => await LoadUserProfile(),
                () => CurrentView = null
            );
        }

        private void ShowFarms()
        {
            CurrentView = new UserFarmsViewModel(_userId);
        }

        private async Task DeleteAccount()
        {
            var response = await _apiClient.DeleteUserAsync(_userId);
            if (response.IsSuccessStatusCode)
            {
                _onLogoutCallback();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
