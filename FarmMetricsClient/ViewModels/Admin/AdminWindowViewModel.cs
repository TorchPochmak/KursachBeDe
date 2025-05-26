using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FarmMetricsClient.ViewModels.Admin.Pages;

namespace FarmMetricsClient.ViewModels.Admin
{
    public class AdminWindowViewModel : INotifyPropertyChanged
    {
        private object _currentView;

        public AdminWindowViewModel(Action onLogout)
        {
            CurrentView = new AdminUsersViewModel();

            ShowUsersCommand = new RelayCommand(_ => CurrentView = new AdminUsersViewModel());
           // ShowSettlementsCommand = new RelayCommand(_ => CurrentView = new AdminSettlementsViewModel());
            LogoutCommand = new RelayCommand(_ => onLogout());
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

        public ICommand ShowUsersCommand { get; }
        public ICommand ShowSettlementsCommand { get; }
        public ICommand LogoutCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}