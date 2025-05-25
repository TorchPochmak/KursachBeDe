using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.Services;
using FarmMetricsClient.ViewModels.Employee.Pages;

namespace FarmMetricsClient.Views.Employee.Pages
{
    public partial class EmployeeRequestsView : UserControl
    {
        public EmployeeRequestsView()
        {
            InitializeComponent();


        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public EmployeeRequestsView(EmployeeRequestsViewModel viewModel) : this()
        {
            DataContext = viewModel;

        }
        private async void OnProcessClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId) 
            {
                if (DataContext is EmployeeRequestsViewModel viewModel)
                {
                    bool success = await viewModel.UpdateRequestStatusAsync(requestId, (int)EmployeeRequestsViewModel.RequestStatus.ВОбработке);

                    if (success)
                    {
                        await viewModel.LoadRequestsAsync(); 
                    }
                }
            }
        }

        private async void OnCompleteClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId) 
            {
                if (DataContext is EmployeeRequestsViewModel viewModel)
                {
                    bool success = await viewModel.UpdateRequestStatusAsync(requestId, (int)EmployeeRequestsViewModel.RequestStatus.Завершён);

                    if (success)
                    {
                        await viewModel.LoadRequestsAsync();
                    }
                }
            }
        }

        private async void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                if (DataContext is EmployeeRequestsViewModel viewModel)
                {
                    bool success = await viewModel.UpdateRequestStatusAsync(requestId, (int)EmployeeRequestsViewModel.RequestStatus.Закрыто);

                    if (success)
                    {
                        await viewModel.LoadRequestsAsync(); 
                    }
                }
            }
        }






    }
}
