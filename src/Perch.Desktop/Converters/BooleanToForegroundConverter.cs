using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Perch.Desktop.Converters;

public sealed class BooleanToForegroundConverter : IValueConverter
{
    private static readonly Brush AppliedBrush = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
    private static readonly Brush DriftedBrush = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? AppliedBrush : DriftedBrush;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
