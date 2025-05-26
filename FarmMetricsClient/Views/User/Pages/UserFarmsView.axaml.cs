using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.Views.User.Pages;

namespace FarmMetricsClient.Views.User.Pages
{
    public partial class UserFarmsView : UserControl
    {
        public UserFarmsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}