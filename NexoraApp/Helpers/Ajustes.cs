using Microsoft.Maui.Storage;

namespace NexoraApp.Helpers
{
    // Preferencias del usuario que se guardan en el dispositivo (sobreviven al cerrar la app)
    public static class Ajustes
    {
        private const string ClaveTema = "tema";
        private const string ClaveNotificaciones = "notificaciones";
        private const string ClaveServidor = "urlServidor";

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

        // Dirección del backend. El emulador de Android ve el PC como 10.0.2.2;
        // en Windows el servidor está en localhost; en un celular real hay que poner la IP del PC.
        public static string UrlServidor
        {
            get => Preferences.Default.Get(ClaveServidor, UrlPorDefecto());
            set => Preferences.Default.Set(ClaveServidor, value);
        }

        private static string UrlPorDefecto() =>
            DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:8080" : "http://localhost:8080";

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
