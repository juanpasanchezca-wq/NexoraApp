using Microsoft.Maui.Storage;

namespace NexoraApp.Helpers
{
    // Preferencias del usuario que se guardan en el dispositivo (sobreviven al cerrar la app)
    public static class Ajustes
    {
        private const string ClaveTema = "tema";
        private const string ClaveNotificaciones = "notificaciones";

        // "Claro", "Oscuro" o "Sistema"
        public static string Tema
        {
            get => Preferences.Default.Get(ClaveTema, "Sistema");
            set
            {
                Preferences.Default.Set(ClaveTema, value);
                AplicarTema();
            }
        }

        public static bool NotificacionesActivas
        {
            get => Preferences.Default.Get(ClaveNotificaciones, true);
            set => Preferences.Default.Set(ClaveNotificaciones, value);
        }

        public static void AplicarTema()
        {
            if (Application.Current == null) return;
            Application.Current.UserAppTheme = Tema switch
            {
                "Claro" => AppTheme.Light,
                "Oscuro" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }
    }
}
