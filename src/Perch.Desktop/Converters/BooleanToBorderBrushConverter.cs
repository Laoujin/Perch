using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Perch.Desktop.Converters;

public sealed class BooleanToBorderBrushConverter : IValueConverter
{
    private static readonly Brush SelectedBrush = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
    private static readonly Brush UnselectedBrush = Brushes.Transparent;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? SelectedBrush : UnselectedBrush;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
