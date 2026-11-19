using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NexoraApp.Converters
{
    // true (subtarea completada) -> texto atenuado
    public class BoolAOpacidadConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true ? 0.6 : 1.0;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
