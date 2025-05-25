using System;
using Avalonia.Controls;
using FarmMetricsClient.ViewModels.Client;

namespace FarmMetricsClient.Views.Client
{
    public partial class ClientWindow : Window
    {
        public ClientWindow(int userId)
        {
            InitializeComponent();
         
            DataContext = new ClientWindowViewModel(userId, () =>
            {
                new MainWindow().Show();
                this.Close();
            });
        }
    }
}
