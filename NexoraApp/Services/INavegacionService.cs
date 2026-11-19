namespace NexoraApp.Services
{
    // Contrato de navegación (Clase 11): el ViewModel pide navegar sin conocer Shell
    public interface INavegacionService
    {
        Task IrAAsync(string ruta);
        Task VolverAsync();
        Task<bool> ConfirmarAsync(string titulo, string mensaje, string aceptar, string cancelar);
    }
}
