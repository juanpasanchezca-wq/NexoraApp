using NexoraApp.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NexoraApp.Converters
{
    internal class EstadoColorConverterIValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Estilos.ColorEstadoTarea(value as string ?? "");

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
