using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AutoLavadoApp.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return false;
            if (value is int i) return i > 0;
            if (int.TryParse(value.ToString(), out var r)) return r > 0;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
