using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.Admin.Pages;

namespace FarmMetricsClient.Views.Admin.Pages
{
    public partial class AdminBackupsView : UserControl
    {
        public AdminBackupsView()
        {
            InitializeComponent();
            DataContext = new AdminBackupsViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

