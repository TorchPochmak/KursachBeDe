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

        private async void OnDeleteSettlementClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int settlementId &&
                DataContext is AdminSettlementsViewModel viewModel)
            {
                await viewModel.DeleteSettlementAsync(settlementId);
            }
        }

        private async void OnShowDevicesClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int settlementId &&
                DataContext is AdminSettlementsViewModel viewModel)
            {
                await viewModel.ShowDevicesAsync(settlementId);
            }
        }
    }
}