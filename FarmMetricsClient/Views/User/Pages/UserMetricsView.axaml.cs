using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.User.Pages;

namespace FarmMetricsClient.Views.User.Pages
{
    public partial class UserMetricsView : UserControl
    {
        public UserMetricsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}