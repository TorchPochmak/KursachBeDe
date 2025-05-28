using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FarmMetricsClient.ViewModels.Admin.Pages;

namespace FarmMetricsClient.ViewModels.Admin
{
    public class AdminWindowViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        private readonly Stack<object> _viewStack = new Stack<object>();

        public AdminWindowViewModel(Action onLogout)
        {
            CurrentView = new AdminUsersViewModel();

            ShowUsersCommand = new RelayCommand(_ => NavigateTo(new AdminUsersViewModel()));
            ShowSettlementsCommand = new RelayCommand(_ => 
                NavigateTo(new AdminSettlementsViewModel(NavigateToDevices)));
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

        public void NavigateTo(object view)
        {
            _viewStack.Push(CurrentView);
            CurrentView = view;
        }

        public void NavigateToDevices(int settlementId, string settlementName)
        {
            var devicesViewModel = new AdminDevicesViewModel(
                settlementId, 
                settlementName, 
                () => GoBack());
            
            NavigateTo(devicesViewModel);
        }

        public void GoBack()
        {
            if (_viewStack.Count > 0)
            {
                CurrentView = _viewStack.Pop();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}