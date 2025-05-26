using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.User.Pages;
using FarmMetricsClient.Views.User.Pages;

namespace FarmMetricsClient.Views.User.Pages
{
    public partial class UserFarmsView : UserControl
    {
        public UserFarmsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void OnDeleteFarmClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int farmId &&
            DataContext is UserFarmsViewModel viewModel)
        {
            viewModel.DeleteFarmAsync(farmId);
        }
    }

    private void OnOpenFarmDetailsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int farmId &&
            DataContext is UserFarmsViewModel viewModel)
        {
            viewModel.OpenFarmDetails(farmId);
        }
    }
    }
}