using NexoraApp.DataAccess;

namespace NexoraApp.Views
{
    public partial class ProyectosPage : ContentPage
    {
        public ProyectosPage()
        {
            InitializeComponent();
        }

        // Se carga en OnAppearing para que la lista se actualice cada vez que se abre la pestaña
        protected override void OnAppearing()
        {
            base.OnAppearing();
            container.Children.Clear();

            var dbContext = new NexoraDbContext();

            foreach (var proyecto in dbContext.Proyectos)
            {
                var boton = new Button
                {
                    Text = $"{proyecto.Nombre}  ·  {proyecto.Estado}",
                    BackgroundColor = Color.FromArgb("#D3ECCD"),
                    TextColor = Color.FromArgb("#212121")
                };

                boton.Clicked += async (s, e) =>
                {
                    var uri = $"{nameof(ProyectoDetailPage)}?id={proyecto.Id}";
                    await Shell.Current.GoToAsync(uri);
                };

                container.Children.Add(boton);
            }
        }
    }
}
