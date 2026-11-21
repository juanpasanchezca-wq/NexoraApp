using NexoraApp.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NexoraApp.Converters
{
    public class EstadoColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Estilos.ColorEstadoTarea(value as string ?? "");

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
