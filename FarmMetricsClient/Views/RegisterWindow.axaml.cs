using Avalonia.Controls;
using FarmMetricsClient.ViewModels;

namespace FarmMetricsClient.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            DataContext = new RegisterWindowViewModel();
        }
    }
}
