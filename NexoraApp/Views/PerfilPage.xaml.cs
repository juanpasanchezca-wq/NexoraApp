using Microsoft.EntityFrameworkCore;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    public partial class PerfilPage : ContentPage
    {
        private readonly SelectorOpciones selectorTema;
        private bool cargando;

        public PerfilPage()
        {
            InitializeComponent();

            // Claro / Oscuro / Sistema: se guarda y se aplica al instante
            selectorTema = new SelectorOpciones(temaGrid, new[] { "Claro", "Oscuro", "Sistema" }, 3,
                tema => Ajustes.Tema = tema);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Cargar();
        }

        private void Cargar()
        {
            cargando = true;

            var dbContext = new NexoraDbContext();
            var usuario = dbContext.Usuarios.First(u => u.Id == Sesion.UsuarioActualId);
            var tareas = dbContext.Tareas
                .Include(t => t.Proyecto)
                .Where(t => t.UsuarioId == usuario.Id)
                .OrderBy(t => t.FechaLimite)
                .ToList();
            var proyectos = dbContext.Miembros.Count(m => m.UsuarioId == usuario.Id);
            var sinLeer = dbContext.Notificaciones.Count(n => n.UsuarioId == usuario.Id && !n.Leida);

            // ---------- Datos del usuario ----------
            inicialesLabel.Text = Estilos.Iniciales(usuario.Nombre);
            avatar.BackgroundColor = Estilos.ColorAvatar(usuario.Id);
            nombreLabel.Text = usuario.Nombre;
            correoLabel.Text = usuario.Correo;
            rolLabel.Text = usuario.Rol;
            rolChip.BackgroundColor = Estilos.FondoRol(usuario.Rol);

            // ---------- Resumen ----------
            var pendientes = tareas.Where(t => t.Estado != "Hecha").ToList();
            proyectosLabel.Text = proyectos.ToString();
            pendientesLabel.Text = pendientes.Count.ToString();
            hechasLabel.Text = tareas.Count(t => t.Estado == "Hecha").ToString();

            // ---------- Preferencias ----------
            selectorTema.Seleccionar(Ajustes.Tema);
            notifSwitch.IsToggled = Ajustes.NotificacionesActivas;
            sinLeerChip.IsVisible = Ajustes.NotificacionesActivas && sinLeer > 0;
            sinLeerLabel.Text = sinLeer.ToString();

            // ---------- Mis tareas pendientes (las 5 más próximas) ----------
            var filas = pendientes
                .Take(5)
                .Select(t => new TareaFila(t.Id, t.Titulo, t.Estado, Estilos.ColorEstadoTarea(t.Estado),
                    Estilos.TextoVencimiento(t.FechaLimite, t.Estado), t.Proyecto.Nombre))
                .ToList();
            sinTareasLabel.IsVisible = filas.Count == 0;
            BindableLayout.SetItemsSource(listaTareas, filas);

            cargando = false;
        }

        private void OnNotifToggled(object? sender, ToggledEventArgs e)
        {
            if (cargando) return;
            Ajustes.NotificacionesActivas = e.Value;
            Cargar();
        }

        private async void OnCerrarSesionClicked(object? sender, EventArgs e)
        {
            await Sesion.CerrarConConfirmacionAsync();
        }

        private async void OnVerNotificacionesTapped(object? sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NotificacionesPage));
        }

        private async void OnVerTodasTapped(object? sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("//TareasPage");
        }

        private async void OnTareaTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject { BindingContext: TareaFila fila })
                await Shell.Current.GoToAsync($"{nameof(TareaDetailPage)}?id={fila.Id}");
        }
    }
}
