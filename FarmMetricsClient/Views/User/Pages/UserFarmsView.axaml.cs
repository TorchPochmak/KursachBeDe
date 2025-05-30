using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FarmMetricsClient.ViewModels.User.Pages;
using static FarmMetricsClient.Services.ApiClient;

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
        private void OnDeleteCultureClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string cultureName)
            {
                var expander = button.FindAncestorOfType<Expander>();
                if (expander?.DataContext is FarmViewModel farmVm)
                {
                    farmVm.DeleteCultureCommand.Execute(cultureName);
                }
            }
        }

        private void OnDeleteMetricsClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int deviceId)
            {
                var expander = button.FindAncestorOfType<Expander>();
                if (expander?.DataContext is FarmViewModel farmVm)
                {
                    farmVm.DeleteMetricCommand.Execute(deviceId);
                }
            }
        }


    }
}