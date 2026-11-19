using NexoraApp.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NexoraApp.Converters
{
    // Prioridad (Alta, Media, Baja) -> color de fondo del chip
    public class PrioridadColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Estilos.FondoPrioridad(value as string ?? "");

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
