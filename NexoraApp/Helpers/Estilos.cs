using NexoraApp.DataAccess;

namespace NexoraApp.Helpers
{
    // Colores y textos que se repiten en varias pantallas
    public static class Estilos
    {
        public static readonly Color Primario = Color.FromArgb("#E67514");
        public static readonly Color Texto = Color.FromArgb("#212121");
        public static readonly Color Exito = Color.FromArgb("#06923E");
        public static readonly Color FondoSuave = Color.FromArgb("#D3ECCD");
        public static readonly Color Gris = Color.FromArgb("#757575");

        public static string Iniciales(string nombre) =>
            string.Concat(nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => p[0]));

        // Cada usuario siempre recibe el mismo color de avatar (el texto siempre es oscuro)
        public static Color ColorAvatar(int usuarioId) => (usuarioId % 3) switch
        {
            0 => FondoSuave,
            1 => Color.FromArgb("#F7D2B0"),
            _ => Color.FromArgb("#E0E0E0"),
        };

        public static Color FondoEstadoProyecto(string estado) => estado switch
        {
            "En curso" => Color.FromArgb("#33E67514"),
            "Finalizado" => Color.FromArgb("#3306923E"),
            _ => Color.FromArgb("#33808080"),
        };

        public static Color PuntoEstadoProyecto(string estado) => estado switch
        {
            "En curso" => Primario,
            "Finalizado" => Exito,
            _ => Gris,
        };

        public static Color ColorEstadoTarea(string estado) => estado switch
        {
            "Hecha" => Exito,
            "En progreso" => Primario,
            "En revisión" => Color.FromArgb("#F7B27A"),
            _ => Color.FromArgb("#BDBDBD"),
        };

        public static Color FondoPrioridad(string prioridad) => prioridad switch
        {
            "Alta" => Color.FromArgb("#33E67514"),
            "Media" => Color.FromArgb("#3306923E"),
            _ => Color.FromArgb("#33808080"),
        };

        public static Color ColorVencimiento(DateTime fecha, string estado) =>
            estado != "Hecha" && fecha.Date < DateTime.Today ? Color.FromArgb("#D32F2F") : Gris;

        public static string TextoVencimiento(DateTime fecha, string estado)
        {
            if (estado == "Hecha") return $"Entregada";
            var dias = (fecha.Date - DateTime.Today).Days;
            return dias switch
            {
                0 => "Vence hoy",
                1 => "Vence mañana",
                < 0 => $"Vencida hace {-dias} d",
                _ => $"Vence {fecha:dd/MM}",
            };
        }
    }
}
