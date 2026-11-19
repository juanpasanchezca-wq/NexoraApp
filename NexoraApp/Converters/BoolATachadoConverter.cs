using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NexoraApp.Converters
{
    // true (subtarea completada) -> texto tachado
    public class BoolATachadoConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true ? TextDecorations.Strikethrough : TextDecorations.None;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
