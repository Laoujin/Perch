using System.Windows;
using System.Windows.Controls;

namespace Perch.Desktop.Views.Controls;

public partial class CardGalleryLayout : UserControl
{
    public static readonly DependencyProperty HeaderContentProperty =
        DependencyProperty.Register(nameof(HeaderContent), typeof(object), typeof(CardGalleryLayout),
            new PropertyMetadata(null));

    public static readonly DependencyProperty GridContentProperty =
        DependencyProperty.Register(nameof(GridContent), typeof(object), typeof(CardGalleryLayout),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DetailContentProperty =
        DependencyProperty.Register(nameof(DetailContent), typeof(object), typeof(CardGalleryLayout),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ErrorTitleProperty =
        DependencyProperty.Register(nameof(ErrorTitle), typeof(string), typeof(CardGalleryLayout),
            new PropertyMetadata(string.Empty));

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public object? GridContent
    {
        get => GetValue(GridContentProperty);
        set => SetValue(GridContentProperty, value);
    }

    public object? DetailContent
    {
        get => GetValue(DetailContentProperty);
        set => SetValue(DetailContentProperty, value);
    }

    public string ErrorTitle
    {
        get => (string)GetValue(ErrorTitleProperty);
        set => SetValue(ErrorTitleProperty, value);
    }

    public CardGalleryLayout()
    {
        InitializeComponent();
    }
}
