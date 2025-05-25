// UserWindowViewModel.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FarmMetricsClient.ViewModels.User
{
    public class UserWindowViewModel : INotifyPropertyChanged
    {
        private readonly int _userId;
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowProfileCommand { get; }
        public ICommand ShowFarmsCommand { get; }
        public ICommand ShowHarvestsCommand { get; }
        public ICommand LogoutCommand { get; }

        public UserWindowViewModel(int userId, Action onLogoutCallback)
        {
            _userId = userId;
            
         //   CurrentView = new UserProfileViewModel(userId);
            
         //   ShowProfileCommand = new RelayCommand(_ => CurrentView = new UserProfileViewModel(userId));
         //  ShowFarmsCommand = new RelayCommand(_ => CurrentView = new UserFarmsViewModel(userId));
         //   ShowHarvestsCommand = new RelayCommand(_ => CurrentView = new FarmHarvestsViewModel(userId));
            
            LogoutCommand = new RelayCommand(_ => onLogoutCallback());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}