using Microsoft.EntityFrameworkCore;
using NexoraApp.DataAccess;

namespace NexoraApp.Views
{
    public partial class TareaDetailPage : ContentPage, IQueryAttributable
    {
        public TareaDetailPage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            container.Children.Clear();

            var dbContext = new NexoraDbContext();
            var id = int.Parse(query["id"].ToString()!);
            var tarea = dbContext.Tareas
                .Include(t => t.Proyecto)
                .Include(t => t.Usuario)
                .First(t => t.Id == id);

            container.Children.Add(new Label { Text = tarea.Titulo, FontSize = 22, FontAttributes = FontAttributes.Bold });
            container.Children.Add(new Label { Text = $"Estado: {tarea.Estado}" });
            container.Children.Add(new Label { Text = $"Prioridad: {tarea.Prioridad}" });
            container.Children.Add(new Label { Text = $"Fecha límite: {tarea.FechaLimite:dd/MM/yyyy}" });
            container.Children.Add(new Label { Text = $"Proyecto: {tarea.Proyecto.Nombre}" });
            container.Children.Add(new Label { Text = $"Asignada a: {tarea.Usuario.Nombre}" });
            container.Children.Add(new Label { Text = tarea.Descripcion, Margin = new Thickness(0, 12, 0, 0) });
        }
    }
}
