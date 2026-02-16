using System.ComponentModel;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

using Perch.Desktop.ViewModels.Wizard;

namespace Perch.Desktop.Views;

public partial class WizardWindow : FluentWindow
{
    public WizardShellViewModel ViewModel { get; }

    public event Action? WizardCompleted;

    public WizardWindow(WizardShellViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        SystemThemeWatcher.Watch(this);

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateStepVisibility();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WizardShellViewModel.CurrentStepIndex))
        {
            UpdateStepVisibility();
            ViewModel.NotifySelectionCounts();
        }
    }

    private void UpdateStepVisibility()
    {
        var stepName = ViewModel.GetCurrentStepName();

        DotfilesStep.Visibility = stepName == "Dotfiles" ? Visibility.Visible : Visibility.Collapsed;
        AppsStep.Visibility = stepName == "Apps" ? Visibility.Visible : Visibility.Collapsed;
        TweaksStep.Visibility = stepName == "System Tweaks" ? Visibility.Visible : Visibility.Collapsed;
        ReviewStep.Visibility = stepName == "Review" ? Visibility.Visible : Visibility.Collapsed;
        DeployStep.Visibility = stepName == "Deploy" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnOpenDashboard(object sender, RoutedEventArgs e)
    {
        WizardCompleted?.Invoke();
        Close();
    }
}
