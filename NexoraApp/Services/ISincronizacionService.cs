namespace NexoraApp.Services
{
    // Servicio que habla con el backend (Clase 13)
    public interface ISincronizacionService
    {
        Task<int> EnviarTareasAsync(string urlBase, IEnumerable<TareaDto> tareas);
        Task<List<TareaDto>> ObtenerTareasAsync(string urlBase);
    }
}
