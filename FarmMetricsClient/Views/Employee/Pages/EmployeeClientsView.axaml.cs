using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Employee.Pages;

namespace FarmMetricsClient.Views.Employee.Pages
{
    public partial class EmployeeClientsView : UserControl
    {
        public EmployeeClientsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public EmployeeClientsView(EmployeeClientsViewModel viewModel) : this()
        {
            DataContext = viewModel; 

        }
    }
}
