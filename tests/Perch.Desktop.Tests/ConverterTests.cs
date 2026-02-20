using System.Globalization;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Media;

using Perch.Desktop.Converters;

namespace Perch.Desktop.Tests;

[TestFixture]
[Platform("Win")]
[SupportedOSPlatform("windows")]
public sealed class ConverterTests
{
    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class PathEllipsisConverterTests
    {
        private PathEllipsisConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new PathEllipsisConverter();

        [Test]
        public void Convert_NullValue_ReturnsEmptyString()
        {
            var result = _converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Convert_EmptyString_ReturnsEmptyString()
        {
            var result = _converter.Convert("", typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Convert_ShortPath_ReturnsUnchanged()
        {
            var result = _converter.Convert(@"C:\short", typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(@"C:\short"));
        }

        [Test]
        public void Convert_LongPath_Truncates()
        {
            string longPath = @"C:\Users\wouter\AppData\Local\Some\Very\Deep\Path\That\Is\Way\Too\Long\For\Display\settings.json";
            var result = (string)_converter.Convert(longPath, typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.That(result.Length, Is.LessThanOrEqualTo(75));
        }

        [Test]
        public void Convert_WithParameter_UsesCustomMaxLength()
        {
            string path = @"C:\Users\wouter\Documents\file.txt";
            var result = (string)_converter.Convert(path, typeof(string), "20", CultureInfo.InvariantCulture);
            Assert.That(result.Length, Is.LessThanOrEqualTo(20));
        }

        [Test]
        public void Convert_NonStringValue_ReturnsSameValue()
        {
            var result = _converter.Convert(42, typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertBack("test", typeof(string), null!, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class IndexToVisibilityConverterTests
    {
        private IndexToVisibilityConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new IndexToVisibilityConverter();

        [Test]
        public void Convert_MatchingIndex_ReturnsVisible()
        {
            var result = _converter.Convert(2, typeof(Visibility), "2", CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void Convert_NonMatchingIndex_ReturnsCollapsed()
        {
            var result = _converter.Convert(1, typeof(Visibility), "2", CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void Convert_InvalidParameter_ReturnsCollapsed()
        {
            var result = _converter.Convert(1, typeof(Visibility), "notanumber", CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void Convert_NonIntValue_ReturnsCollapsed()
        {
            var result = _converter.Convert("text", typeof(Visibility), "0", CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertBack(Visibility.Visible, typeof(int), null!, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class BooleanToChevronConverterTests
    {
        private BooleanToChevronConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new BooleanToChevronConverter();

        [Test]
        public void Convert_True_ReturnsDownChevron()
        {
            var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo("\u25BC"));
        }

        [Test]
        public void Convert_False_ReturnsRightChevron()
        {
            var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo("\u25B6"));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertBack("test", typeof(bool), null!, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class InverseBooleanConverterTests
    {
        private InverseBooleanConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new InverseBooleanConverter();

        [Test]
        public void Convert_True_ReturnsFalse()
        {
            var result = _converter.Convert(true, typeof(bool), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Convert_False_ReturnsTrue()
        {
            var result = _converter.Convert(false, typeof(bool), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void ConvertBack_True_ReturnsFalse()
        {
            var result = _converter.ConvertBack(true, typeof(bool), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void ConvertBack_False_ReturnsTrue()
        {
            var result = _converter.ConvertBack(false, typeof(bool), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(true));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class InverseBooleanToVisibilityConverterTests
    {
        private InverseBooleanToVisibilityConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new InverseBooleanToVisibilityConverter();

        [Test]
        public void Convert_True_ReturnsCollapsed()
        {
            var result = _converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void Convert_False_ReturnsVisible()
        {
            var result = _converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void ConvertBack_Collapsed_ReturnsTrue()
        {
            var result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(true));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class BooleanToBorderBrushConverterTests
    {
        private BooleanToBorderBrushConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new BooleanToBorderBrushConverter();

        [Test]
        public void Convert_True_ReturnsGreenBrush()
        {
            var result = (SolidColorBrush)_converter.Convert(true, typeof(Brush), null!, CultureInfo.InvariantCulture);
            Assert.That(result.Color, Is.EqualTo(Color.FromRgb(0x10, 0xB9, 0x81)));
        }

        [Test]
        public void Convert_False_ReturnsTransparent()
        {
            var result = _converter.Convert(false, typeof(Brush), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Brushes.Transparent));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertBack(Brushes.Red, typeof(bool), null!, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class BooleanToForegroundConverterTests
    {
        private BooleanToForegroundConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new BooleanToForegroundConverter();

        [Test]
        public void Convert_True_ReturnsGreenBrush()
        {
            var result = (SolidColorBrush)_converter.Convert(true, typeof(Brush), null!, CultureInfo.InvariantCulture);
            Assert.That(result.Color, Is.EqualTo(Color.FromRgb(0x10, 0xB9, 0x81)));
        }

        [Test]
        public void Convert_False_ReturnsAmberBrush()
        {
            var result = (SolidColorBrush)_converter.Convert(false, typeof(Brush), null!, CultureInfo.InvariantCulture);
            Assert.That(result.Color, Is.EqualTo(Color.FromRgb(0xF5, 0x9E, 0x0B)));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertBack(Brushes.Red, typeof(bool), null!, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class CountToVisibilityConverterTests
    {
        private CountToVisibilityConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new CountToVisibilityConverter();

        [Test]
        public void Convert_PositiveCount_ReturnsVisible()
        {
            var result = _converter.Convert(5, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void Convert_Zero_ReturnsCollapsed()
        {
            var result = _converter.Convert(0, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void Convert_NonInt_ReturnsCollapsed()
        {
            var result = _converter.Convert("text", typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertBack(Visibility.Visible, typeof(int), null!, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class NullToVisibilityConverterTests
    {
        private NullToVisibilityConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new NullToVisibilityConverter();

        [Test]
        public void Convert_Null_ReturnsCollapsed()
        {
            var result = _converter.Convert(null, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void Convert_EmptyString_ReturnsCollapsed()
        {
            var result = _converter.Convert("", typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void Convert_NonEmptyString_ReturnsVisible()
        {
            var result = _converter.Convert("hello", typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void Convert_NonNullObject_ReturnsVisible()
        {
            var result = _converter.Convert(42, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertBack(Visibility.Visible, typeof(object), null!, CultureInfo.InvariantCulture));
        }
    }

    [TestFixture]
    [Platform("Win")]
    [SupportedOSPlatform("windows")]
    public sealed class UrlToImageSourceConverterTests
    {
        private UrlToImageSourceConverter _converter = null!;

        [SetUp]
        public void SetUp() => _converter = new UrlToImageSourceConverter();

        [Test]
        public void Convert_Null_ReturnsNull()
        {
            var result = _converter.Convert(null, typeof(object), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_EmptyString_ReturnsNull()
        {
            var result = _converter.Convert("", typeof(object), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_NonString_ReturnsNull()
        {
            var result = _converter.Convert(42, typeof(object), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Convert_InvalidUrl_ReturnsNull()
        {
            var result = _converter.Convert("not-a-url", typeof(object), null!, CultureInfo.InvariantCulture);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ConvertBack_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() =>
                _converter.ConvertBack("test", typeof(string), null!, CultureInfo.InvariantCulture));
        }
    }
}
