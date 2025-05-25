using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Employee.Pages;

namespace FarmMetricsClient.Views.Employee.Pages
{
    public partial class ArchivedRequestsView : UserControl
    {
        public ArchivedRequestsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public ArchivedRequestsView(ArchivedRequestsViewModel viewModel) : this()
        {
            DataContext = viewModel;

        }
    }
}
