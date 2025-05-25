using Avalonia.Controls;
using FarmMetricsClient.ViewModels;
using FarmMetricsClient.Views.Admin;
using FarmMetricsClient.Views.User;
namespace FarmMetricsClient.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(OnLoginSuccessful);
        }

        private void OpenWindow(Window newWindow)
        {
            newWindow.Show();
            this.Close(); 
        }

        private void OnLoginSuccessful(string role, int? userId)
        {
            switch (role)
            {
                case "Admin":
                    OpenWindow(new AdminWindow());
                    break;
                case "User":
                    OpenWindow(new UserWindow(userId ?? 0));
                    break;
            }
        }
    }
}