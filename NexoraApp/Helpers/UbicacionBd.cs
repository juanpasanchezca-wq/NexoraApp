using System;
using System.Collections.Generic;
using System.Text;

namespace NexoraApp.Helpers
{
    public static class UbicacionBd
    {
        public const string NombreArchivo = "nexora.db3";

        public static string Ruta => Path.Combine(FileSystem.AppDataDirectory, NombreArchivo);
    }
}
