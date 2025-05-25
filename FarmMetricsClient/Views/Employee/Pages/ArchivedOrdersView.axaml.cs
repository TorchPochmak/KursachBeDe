using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Employee.Pages;

namespace FarmMetricsClient.Views.Employee.Pages
{
    public partial class ArchivedOrdersView : UserControl
    {
        public ArchivedOrdersView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ArchivedOrdersView(ArchivedOrdersViewModel viewModel) : this()
        {
            DataContext = viewModel; 

        }
    }
}
