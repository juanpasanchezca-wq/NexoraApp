using NexoraApp.DataAccess;

namespace NexoraApp.Views
{
    public partial class EquipoPage : ContentPage
    {
        public EquipoPage()
        {
            InitializeComponent();

            var dbContext = new NexoraDbContext();

            foreach (var usuario in dbContext.Usuarios)
            {
                container.Children.Add(new Label
                {
                    Text = usuario.Nombre,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold
                });
                container.Children.Add(new Label
                {
                    Text = $"{usuario.Rol}  ·  {usuario.Correo}",
                    FontSize = 13
                });
            }
        }
    }
}
