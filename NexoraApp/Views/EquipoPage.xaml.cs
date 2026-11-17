using Microsoft.Maui.Controls.Shapes;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    public partial class EquipoPage : ContentPage
    {
        private static readonly string[] Roles = { "Todos", "Administrador", "Líder de proyecto", "Miembro" };
        private string filtro = "Todos";

        public EquipoPage()
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
            var dbContext = new NexoraDbContext();
            var usuarios = dbContext.Usuarios.OrderBy(u => u.Nombre).ToList();
            var miembros = dbContext.Miembros.ToList();
            var tareas = dbContext.Tareas.ToList();

            resumenLabel.Text = $"{usuarios.Count} personas en tu organización";

            // ---------- Filtros por rol con contador ----------
            chips.Children.Clear();
            foreach (var rol in Roles)
            {
                var cantidad = rol == "Todos" ? usuarios.Count : usuarios.Count(u => u.Rol == rol);
                var activo = rol == filtro;

                var etiqueta = new Label
                {
                    Text = $"{rol} ({cantidad})",
                    FontSize = 13,
                    FontAttributes = activo ? FontAttributes.Bold : FontAttributes.None
                };
                if (activo) etiqueta.TextColor = Estilos.Texto;

                var chip = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 16 },
                    StrokeThickness = activo ? 0 : 1,
                    Stroke = Color.FromArgb("#BDBDBD"),
                    BackgroundColor = activo ? Estilos.Primario : Colors.Transparent,
                    Padding = new Thickness(14, 8),
                    Content = etiqueta
                };
                var elegido = rol;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) =>
                {
                    filtro = elegido;
                    Cargar();
                };
                chip.GestureRecognizers.Add(tap);
                chips.Children.Add(chip);
            }

            // ---------- Tarjetas ----------
            var items = usuarios
                .Where(u => filtro == "Todos" || u.Rol == filtro)
                .Select(u =>
                {
                    var suyas = tareas.Where(t => t.UsuarioId == u.Id).ToList();
                    var hechas = suyas.Count(t => t.Estado == "Hecha");
                    var pendientes = suyas.Count - hechas;
                    var proyectos = miembros.Count(m => m.UsuarioId == u.Id);
                    var avance = suyas.Count == 0 ? 0 : (double)hechas / suyas.Count;

                    return new MiembroEquipo(
                        u.Id, u.Nombre, u.Correo, u.Rol, Estilos.Iniciales(u.Nombre),
                        Estilos.ColorAvatar(u.Id), Estilos.FondoRol(u.Rol),
                        proyectos == 1 ? "1 proyecto" : $"{proyectos} proyectos",
                        pendientes == 1 ? "1 tarea pendiente" : $"{pendientes} tareas pendientes",
                        avance, $"{Math.Round(avance * 100)}%");
                })
                .ToList();

            BindableLayout.SetItemsSource(lista, items);
            lista.IsVisible = items.Count > 0;

            // ---------- Estado vacío ----------
            if (items.Count == 0)
            {
                estadoVista.Mostrar("icono_personas_vacia.svg", Estilos.Primario,
                    $"No hay personas con el rol \"{filtro}\"",
                    "Prueba con otro rol para ver más personas.",
                    "Ver todos", () =>
                    {
                        filtro = "Todos";
                        Cargar();
                    });
            }
            else
            {
                estadoVista.IsVisible = false;
            }
        }
    }
}