using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kurs_AgileDashbord.Converters
{
    /// <summary>
    /// Конвертирует строку CurrentView в Visibility.
    /// ConverterParameter = имя нужного вида (например, "Kanban")
    /// </summary>
    public class StringToKanbanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var current = value as string;
            var target = parameter as string;
            return current == target ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
