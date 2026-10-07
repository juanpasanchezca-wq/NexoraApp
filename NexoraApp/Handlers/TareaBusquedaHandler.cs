using NexoraApp.DataAccess;
using NexoraApp.Views;

namespace NexoraApp.Handlers
{
    public class TareaBusquedaHandler : SearchHandler
    {
        NexoraDbContext dbContext;

        public TareaBusquedaHandler()
        {
            this.dbContext = new NexoraDbContext();
        }

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
                return;
            }

            var resultados = dbContext.Tareas
                .Where(t => t.Titulo.ToLowerInvariant()
                    .Contains(newValue.ToLowerInvariant()))
                .ToList();

            ItemsSource = resultados;
        }

        protected override async void OnItemSelected(object item)
        {
            await Shell.Current
                .GoToAsync($"{nameof(TareaDetailPage)}?id={((Tarea)item).Id}");
        }
    }
}
