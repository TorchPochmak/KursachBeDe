using System;
using Avalonia.Controls;
using FarmMetricsClient.ViewModels;
using FarmMetricsClient.Views.Admin;
using FarmMetricsClient.Views.Client;
using FarmMetricsClient.Views.Employee;


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

        // Обрабатываем успешный логин
       private void OnLoginSuccessful(string role, int? userId)
        {
            switch (role)
            {
                case "Admin":
                    OpenWindow(new AdminWindow());
                    break;
                case "Employee":
                    OpenWindow(new EmployeeWindow(userId ?? 0));
                    break;
                case "Client":
                    OpenWindow(new ClientWindow(userId ?? 0));
                    break;
            }
        }


    }
}
