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
            CurrentView = new AdminEmployeesViewModel();

            ShowEmployeesCommand = new RelayCommand(_ => CurrentView = new AdminEmployeesViewModel());
            ShowBackupsCommand = new RelayCommand(_ => CurrentView = new AdminBackupsViewModel());

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

        public ICommand ShowEmployeesCommand { get; }
        public ICommand ShowBackupsCommand { get; }
        public ICommand LogoutCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
