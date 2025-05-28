using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Admin.Pages;

namespace FarmMetricsClient.Views.Admin.Pages
{
    public partial class AdminDevicesView : UserControl
    {
        public AdminDevicesView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnGoBackClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminDevicesViewModel viewModel)
            {
                viewModel.GoBackAction?.Invoke();
            }
        }

        private async void OnDeleteDeviceClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int deviceId &&
                DataContext is AdminDevicesViewModel viewModel)
            {
                await viewModel.DeleteDeviceAsync(deviceId);
            }
        }

        private async void OnAddDeviceClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminDevicesViewModel viewModel)
            {
                await viewModel.AddDeviceAsync();
            }
        }
    }
}