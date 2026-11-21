using Microsoft.Maui.Storage;
using NexoraApp.DataAccess;

namespace NexoraApp
{
    public static class Sesion
    {
        private const string ClaveUsuario = "usuarioId";

        // Usuario que inició sesión (0 = nadie). Lo asigna el login.
        public static int UsuarioActualId { get; private set; }

        public static bool HaySesion => UsuarioActualId != 0;

        // Solo para la demostración de estados: 0 = normal, 1 = sin datos, 2 = error de carga.
        // Se cambia desde el menú lateral ("Probar estados").
        public static int DemoEstado { get; set; } = 0;

        // Se guarda el Id para recordar la sesión al reabrir la app
        public static void Iniciar(int usuarioId)
        {
            UsuarioActualId = usuarioId;
            Preferences.Default.Set(ClaveUsuario, usuarioId);
        }

        // Recupera la sesión guardada (si el usuario todavía existe)
        public static bool Restaurar()
        {
            if (HaySesion) return true;

            var id = Preferences.Default.Get(ClaveUsuario, 0);
            if (id == 0) return false;

            using var dbContext = new NexoraDbContext();
            if (!dbContext.Usuarios.Any(u => u.Id == id)) return false;

            UsuarioActualId = id;
            return true;
        }

        public static void Cerrar()
        {
            UsuarioActualId = 0;
            Preferences.Default.Remove(ClaveUsuario);
        }

        // Pide confirmación, cierra la sesión y vuelve a la pantalla de acceso
        public static async Task CerrarConConfirmacionAsync()
        {
            var confirmar = await Shell.Current.DisplayAlert("Cerrar sesión",
                "¿Quieres salir de tu cuenta?", "Cerrar sesión", "Cancelar");
            if (!confirmar) return;

            Cerrar();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
