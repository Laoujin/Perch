using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Perch.Desktop.Models;
using Perch.Desktop.ViewModels;

namespace Perch.Desktop.Views.Pages;

public partial class SystemTweaksPage : Page
{
    public SystemTweaksViewModel ViewModel { get; }

    public SystemTweaksPage(SystemTweaksViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SystemTweaksViewModel.SelectedCategory))
                UpdateDetailPanelVisibility();
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.RefreshCommand.CanExecute(null))
            ViewModel.RefreshCommand.Execute(null);
    }

    private void OnCategoryCardClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TweakCategoryCardModel card })
        {
            ViewModel.SelectCategoryCommand.Execute(card.Category);
        }
    }

    private void OnGroupExpandClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: FontFamilyGroupModel group })
            group.IsExpanded = !group.IsExpanded;
    }

    private void OnStartupToggleChecked(object sender, RoutedEventArgs e)
    {
        if (GetStartupCardModel(sender) is { IsEnabled: false } card)
            ViewModel.ToggleStartupEnabledCommand.Execute(card);
    }

    private void OnStartupToggleUnchecked(object sender, RoutedEventArgs e)
    {
        if (GetStartupCardModel(sender) is { IsEnabled: true } card)
            ViewModel.ToggleStartupEnabledCommand.Execute(card);
    }

    private void OnStartupRemoveClick(object sender, RoutedEventArgs e)
    {
        if (GetStartupCardModel(sender) is { } card)
            ViewModel.RemoveStartupItemCommand.Execute(card);
    }

    private void UpdateDetailPanelVisibility()
    {
        var category = ViewModel.SelectedCategory;
        var isFonts = string.Equals(category, "Fonts", StringComparison.OrdinalIgnoreCase);
        var isStartup = string.Equals(category, "Startup", StringComparison.OrdinalIgnoreCase);

        TweakDetailPanel.Visibility = category is not null && !isFonts && !isStartup
            ? Visibility.Visible : Visibility.Collapsed;
        FontDetailPanel.Visibility = isFonts
            ? Visibility.Visible : Visibility.Collapsed;
        StartupDetailPanel.Visibility = isStartup
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private static StartupCardModel? GetStartupCardModel(object sender) =>
        (sender as FrameworkElement)?.DataContext as StartupCardModel;
}
