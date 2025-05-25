using Avalonia.Controls;
using Avalonia.Interactivity;
using FarmMetricsClient.ViewModels.Client.Pages;

namespace FarmMetricsClient.Views.Client.Pages
{
    public partial class ClientOrdersView : UserControl
    {
        public ClientOrdersView()
        {
            InitializeComponent();

        }

        private async void OnReloadOrdersButtonClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ClientOrdersViewModel viewModel)
            {
                await viewModel.LoadOrdersAsync();
            }
        }
    }
}
