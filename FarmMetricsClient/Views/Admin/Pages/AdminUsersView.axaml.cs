using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Admin.Pages;

namespace FarmMetricsClient.Views.Admin.Pages
{
    public partial class AdminUsersView : UserControl
    {
        public AdminUsersView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void OnBanButtonClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId &&
                DataContext is AdminUsersViewModel viewModel)
            {
                viewModel.ToggleBanUserAsync(userId, true);
            }
        }

        private void OnUnbanButtonClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId &&
                DataContext is AdminUsersViewModel viewModel)
            {
                viewModel.ToggleBanUserAsync(userId, false);
            }
        }
    }
}