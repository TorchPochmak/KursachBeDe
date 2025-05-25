using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Employee.Pages;

namespace FarmMetricsClient.Views.Employee.Pages
{
    public partial class AddWatchImageWindow : Window
    {
        public AddWatchImageWindow()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

