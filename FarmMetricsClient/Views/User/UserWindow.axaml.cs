using Avalonia.Controls;
using FarmMetricsClient.ViewModels.User;

namespace FarmMetricsClient.Views.User
{
    public partial class UserWindow : Window
    {
        private readonly int _userId;
        
        public UserWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            DataContext = new UserWindowViewModel(userId, OnLogout);
        }

        private void OnLogout()
        {
            new MainWindow().Show();
            Close();
        }
    }
}