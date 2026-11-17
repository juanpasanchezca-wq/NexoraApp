namespace NexoraApp
{
    public static class Sesion
    {
        // Usuario que inició sesión. Por ahora es Laura Méndez (Id = 1);
        // cuando esté el inicio de sesión, este valor lo cambia el login.
        public static int UsuarioActualId { get; set; } = 1;

        // Solo para la demostración de estados: 0 = normal, 1 = sin datos, 2 = error de carga.
        // Se cambia desde el menú lateral ("Probar estados").
        public static int DemoEstado { get; set; } = 0;

    }
}
