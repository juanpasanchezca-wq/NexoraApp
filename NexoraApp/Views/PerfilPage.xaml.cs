using NexoraApp.DataAccess;

namespace NexoraApp.Views
{
    public partial class PerfilPage : ContentPage
    {
        public PerfilPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            container.Children.Clear();

            var dbContext = new NexoraDbContext();
            var usuario = dbContext.Usuarios.First(u => u.Id == Sesion.UsuarioActualId);
            var misTareas = dbContext.Tareas
                .Where(t => t.UsuarioId == usuario.Id)
                .OrderBy(t => t.FechaLimite)
                .ToList();

            container.Children.Add(new Label { Text = usuario.Nombre, FontSize = 22, FontAttributes = FontAttributes.Bold });
            container.Children.Add(new Label { Text = usuario.Correo });
            container.Children.Add(new Label { Text = usuario.Rol });

            container.Children.Add(new Label
            {
                Text = "Mis tareas asignadas",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 16, 0, 0)
            });

            foreach (var tarea in misTareas)
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
