using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Kurs_AgileDashbord.Converters
{
    /// <summary>
    /// Конвертирует статус задачи в цвет для UI
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            var colorHex = status switch
            {
                "To Do" => "#78909C",       // серо-голубой
                "In Progress" => "#42A5F5", // синий
                "Review" => "#FFA726",      // оранжевый
                "Done" => "#66BB6A",        // зелёный
                _ => "#BDBDBD"
            };
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Конвертирует приоритет задачи в цвет
    /// </summary>
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var priority = value as string;
            var colorHex = priority switch
            {
                "Low" => "#78909C",
                "Medium" => "#42A5F5",
                "High" => "#FFA726",
                "Critical" => "#EF5350",
                _ => "#BDBDBD"
            };
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Конвертирует HEX-строку цвета (#FF5722) в SolidColorBrush
    /// </summary>
    public class HexToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var hex = value as string ?? "#7C4DFF";
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch
            {
                return new SolidColorBrush(Colors.Purple);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Конвертирует bool в Visibility (для IsActive и т.п.)
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return System.Windows.Visibility.Visible;
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Конвертирует null/не-null в Visibility
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
