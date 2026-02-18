using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Perch.Desktop.Models;
using Perch.Desktop.ViewModels;
using Perch.Desktop.Views.Controls;

namespace Perch.Desktop.Views.Pages;

public partial class AppsPage : Page
{
    public AppsViewModel ViewModel { get; }

    public AppsPage(AppsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.RefreshCommand.CanExecute(null))
            ViewModel.RefreshCommand.Execute(null);
    }

    private void OnToggleChanged(object sender, RoutedEventArgs e)
    {
        if (GetAppModel(sender) is { } app && ViewModel.ToggleAppCommand.CanExecute(app))
            ViewModel.ToggleAppCommand.Execute(app);
    }

    private void OnBrowseCategoryClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: AppCategoryCardModel card })
        {
            ViewModel.ToggleCategoryExpandCommand.Execute(card);
        }
    }

    private void OnCategoryAppsPanelLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ItemsControl { Tag: string broadCategory } itemsControl)
        {
            itemsControl.ItemsSource = ViewModel.GetCategoryApps(broadCategory);
        }
    }

    private void OnExpandRequested(object sender, RoutedEventArgs e)
    {
        if (GetAppModel(sender) is { } app && ViewModel.ExpandAppCommand.CanExecute(app))
            ViewModel.ExpandAppCommand.Execute(app);
    }

    private void OnTagClicked(object sender, RoutedEventArgs e)
    {
        if (sender is AppCard card && card.DataContext is AppCardModel)
        {
            // The tag text is the DataContext of the clicked element inside the tag ItemsControl.
            // The AppCard raises the event; we find the tag from the original source.
            if (e.OriginalSource is FrameworkElement { DataContext: string tag })
                ViewModel.TagClickCommand.Execute(tag);
        }
    }

    private static AppCardModel? GetAppModel(object sender) =>
        (sender as AppCard)?.DataContext as AppCardModel;
}
