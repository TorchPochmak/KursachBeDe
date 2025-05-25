using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Client.Pages;

namespace FarmMetricsClient.Views.Client.Pages
{
    public partial class ClientRequestsView : UserControl
    {
        public ClientRequestsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ClientRequestsView(ClientRequestsViewModel viewModel) : this()
        {
            DataContext = viewModel; 
        }
    }
}

