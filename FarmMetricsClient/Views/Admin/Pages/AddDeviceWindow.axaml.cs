using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Admin.Pages;

namespace FarmMetricsClient.Views.Admin.Pages
{
    public partial class AddDeviceWindow : Window
    {
        public AddDeviceWindow()
        {
            InitializeComponent();
        }

        public AddDeviceWindow(int settlementId) : this()
        {
            var viewModel = new AddDeviceViewModel(settlementId);
            viewModel.CloseAction = Close;
            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}