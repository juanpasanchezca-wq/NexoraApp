using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    public partial class NotificacionesPage : ContentPage
    {
        public NotificacionesPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Cargar();
        }

        private void Cargar()
        {
            // Si el usuario las apagó en Perfil, se explica y se ofrece activarlas
            if (!Ajustes.NotificacionesActivas)
            {
                resumenLabel.Text = "";
                lista.IsVisible = false;
                estadoVista.Mostrar("icono_campana_vacia.svg", Estilos.Primario,
                    "Notificaciones desactivadas",
                    "Actívalas para enterarte de tareas vencidas, comentarios y asignaciones.",
                    "Activar", () =>
                    {
                        Ajustes.NotificacionesActivas = true;
                        Cargar();
                    });
                return;
            }

            var dbContext = new NexoraDbContext();
            var datos = Sesion.DemoEstado == 1
                ? new List<Notificacion>()
                : dbContext.Notificaciones
                    .Where(n => n.UsuarioId == Sesion.UsuarioActualId)
                    .OrderByDescending(n => n.Fecha)
                    .ToList();

            var sinLeer = datos.Count(n => !n.Leida);
            resumenLabel.Text = sinLeer == 0 ? "Estás al día" : sinLeer == 1 ? "1 sin leer" : $"{sinLeer} sin leer";

            var items = datos
                .Select(n => new NotificacionItem(
                    n.Id, n.TareaId, n.Titulo, n.Mensaje, Estilos.TextoCuando(n.Fecha),
                    !n.Leida, Estilos.ColorTipoNotificacion(n.Tipo),
                    n.Leida ? Colors.Transparent : Color.FromArgb("#14E67514")))
                .ToList();

            BindableLayout.SetItemsSource(lista, items);
            lista.IsVisible = items.Count > 0;

            if (items.Count == 0)
            {
                resumenLabel.Text = "";
                estadoVista.Mostrar("icono_campana_vacia.svg", Estilos.Primario,
                    "No tienes notificaciones",
                    "Cuando haya novedades en tus tareas y proyectos, las verás aquí.");
            }
            else
            {
                estadoVista.IsVisible = false;
            }
        }

        private async void OnNotificacionTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: NotificacionItem item }) return;

            // Se marca como leída y se abre la tarea relacionada
            var dbContext = new NexoraDbContext();
            var notificacion = dbContext.Notificaciones.FirstOrDefault(n => n.Id == item.Id);
            if (notificacion != null && !notificacion.Leida)
            {
                dbContext.Entry(notificacion).Property(n => n.Leida).CurrentValue = true;
                dbContext.SaveChanges();
            }

            if (item.TareaId != null)
                await Shell.Current.GoToAsync($"{nameof(TareaDetailPage)}?id={item.TareaId}");
        }

        private void OnMarcarLeidasClicked(object? sender, EventArgs e)
        {
            var dbContext = new NexoraDbContext();
            foreach (var n in dbContext.Notificaciones.Where(n => n.UsuarioId == Sesion.UsuarioActualId && !n.Leida).ToList())
                dbContext.Entry(n).Property(x => x.Leida).CurrentValue = true;
            dbContext.SaveChanges();
            Cargar();
        }
    }
}
