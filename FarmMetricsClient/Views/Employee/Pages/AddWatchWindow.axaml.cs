using Avalonia.Controls;
using Avalonia.Interactivity;
using FarmMetricsClient.ViewModels.Employee.Pages;

namespace FarmMetricsClient.Views.Employee.Pages
{
    public partial class AddWatchWindow : Window
    {
        public AddWatchWindow()
        {
            InitializeComponent();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
