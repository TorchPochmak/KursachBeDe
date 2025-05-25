using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Client.Pages;

namespace FarmMetricsClient.Views.Client.Pages
{
    public partial class CreateRequestView : UserControl
    {
        public CreateRequestView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public CreateRequestView(CreateRequestViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}

