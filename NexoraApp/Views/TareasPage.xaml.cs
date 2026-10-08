using NexoraApp.DataAccess;

namespace NexoraApp.Views
{
    public partial class TareasPage : ContentPage
    {
        public TareasPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            container.Children.Clear();

            var dbContext = new NexoraDbContext();

            foreach (var tarea in dbContext.Tareas.OrderBy(t => t.FechaLimite))
            {
                var boton = new Button
                {
                    Text = $"{tarea.Titulo}  ·  {tarea.Estado}",
                    BackgroundColor = Color.FromArgb("#D3ECCD"),
                    TextColor = Color.FromArgb("#212121")
                };

                boton.Clicked += async (s, e) =>
                {
                    var uri = $"{nameof(TareaDetailPage)}?id={tarea.Id}";
                    await Shell.Current.GoToAsync(uri);
                };

                container.Children.Add(boton);
            }
        }
    }
}
