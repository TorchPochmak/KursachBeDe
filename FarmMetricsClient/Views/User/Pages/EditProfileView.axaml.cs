using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FarmMetricsClient.ViewModels.User.Pages;

namespace FarmMetricsClient.Views.User.Pages
{
    public partial class EditProfileView : UserControl
    {
        public EditProfileView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}