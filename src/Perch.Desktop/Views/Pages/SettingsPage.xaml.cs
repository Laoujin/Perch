using System.Windows.Controls;

using Perch.Desktop.ViewModels;

namespace Perch.Desktop.Views.Pages;

public partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private bool _isLoaded;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
            return;
        _isLoaded = true;

        if (ViewModel.LoadCommand.CanExecute(null))
            ViewModel.LoadCommand.Execute(null);
    }
}
