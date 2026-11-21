using System.Net.Http.Json;

namespace NexoraApp.Services
{
    public class SincronizacionService : ISincronizacionService
    {
        private readonly HttpClient cliente = new() { Timeout = TimeSpan.FromSeconds(10) };

        private static string Url(string urlBase) => $"{urlBase.TrimEnd('/')}/api/Tarea";

        // POST: manda las tareas locales al servidor
        public async Task<int> EnviarTareasAsync(string urlBase, IEnumerable<TareaDto> tareas)
        {
            var lista = tareas.ToList();
            var respuesta = await cliente.PostAsJsonAsync(Url(urlBase), lista);
            respuesta.EnsureSuccessStatusCode();
            return lista.Count;
        }

        // GET: trae las tareas guardadas en el servidor
        public async Task<List<TareaDto>> ObtenerTareasAsync(string urlBase)
        {
            return await cliente.GetFromJsonAsync<List<TareaDto>>(Url(urlBase)) ?? new List<TareaDto>();
        }
    }
}
