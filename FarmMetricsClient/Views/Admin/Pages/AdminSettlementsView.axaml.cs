using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Admin.Pages;

namespace FarmMetricsClient.Views.Admin.Pages
{
    public partial class AdminSettlementsView : UserControl
    {
        public AdminSettlementsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnAddSettlementClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is AdminSettlementsViewModel vm)
                vm.AddSettlementAsync();
        }

        private void OnAddDeviceClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is AdminSettlementsViewModel vm)
                vm.AddDeviceAsync();
        }

        private void OnDeleteDeviceClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int deviceId &&
                DataContext is AdminSettlementsViewModel vm)
                vm.DeleteDeviceAsync(deviceId);
        }
    }
}