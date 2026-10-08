using Microsoft.EntityFrameworkCore;
using NexoraApp.DataAccess;

namespace NexoraApp.Views
{
    public partial class ProyectoDetailPage : ContentPage, IQueryAttributable
    {
        public ProyectoDetailPage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            container.Children.Clear();

            var dbContext = new NexoraDbContext();
            var id = int.Parse(query["id"].ToString()!);
            var proyecto = dbContext.Proyectos
                .Include(p => p.Responsable)
                .First(p => p.Id == id);
            var tareas = dbContext.Tareas
                .Where(t => t.ProyectoId == id)
                .OrderBy(t => t.FechaLimite)
                .ToList();

            container.Children.Add(new Label { Text = proyecto.Nombre, FontSize = 22, FontAttributes = FontAttributes.Bold });
            container.Children.Add(new Label { Text = proyecto.Estado, FontSize = 14 });
            container.Children.Add(new Label { Text = proyecto.Descripcion });
            container.Children.Add(new Label { Text = $"Inicio: {proyecto.FechaInicio:dd/MM/yyyy}   ·   Entrega: {proyecto.FechaFin:dd/MM/yyyy}" });
            container.Children.Add(new Label { Text = $"Responsable: {proyecto.Responsable.Nombre}" });

            container.Children.Add(new Label
            {
                Text = $"Tareas ({tareas.Count})",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 16, 0, 0)
            });

            foreach (var tarea in tareas)
            {
                var boton = new Button
                {
                    Text = $"{tarea.Titulo}  ·  {tarea.Estado}",
                    BackgroundColor = Color.FromArgb("#D3ECCD"),
                    TextColor = Color.FromArgb("#212121")
                };

                boton.Clicked += async (s, e) =>
                {
                    await Shell.Current.GoToAsync($"{nameof(TareaDetailPage)}?id={tarea.Id}");
                };

                container.Children.Add(boton);
            }
        }
    }
}
