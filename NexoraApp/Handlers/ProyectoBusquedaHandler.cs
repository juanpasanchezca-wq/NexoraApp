using NexoraApp.DataAccess;
using NexoraApp.Views;

namespace NexoraApp.Handlers
{
    public class ProyectoBusquedaHandler : SearchHandler
    {
        NexoraDbContext dbContext;

        public ProyectoBusquedaHandler()
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

            var resultados = dbContext.Proyectos
                .Where(p => p.Nombre.ToLowerInvariant()
                    .Contains(newValue.ToLowerInvariant()))
                .ToList();

            ItemsSource = resultados;
        }

        protected override async void OnItemSelected(object item)
        {
            await Shell.Current
                .GoToAsync($"{nameof(ProyectoDetailPage)}?id={((Proyecto)item).Id}");
        }
    }
}
