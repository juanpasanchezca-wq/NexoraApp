namespace NexoraApp.Helpers
{
    // Ubicación del archivo SQLite (Clase 14).
    // Se cambió el nombre a "v2" porque el modelo ganó la columna de contraseña:
    // así se crea una base nueva sin tener que desinstalar la app.
    public static class UbicacionBd
    {
        public const string NombreArchivo = "nexora_v2.db3";

        public static string Ruta => Path.Combine(FileSystem.AppDataDirectory, NombreArchivo);
    }
}
