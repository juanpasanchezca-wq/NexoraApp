using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls.Shapes;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    public partial class ProyectosPage : ContentPage
    {
        private static readonly string[] Estados = { "Todos", "Planificado", "En curso", "Pausado", "Finalizado" };
        private string filtro = "Todos";

        public ProyectosPage()
        {
            InitializeComponent();
        }

        // Se recarga al volver del formulario o del detalle (por si hubo cambios)
        private bool primeraCarga = true;

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Estado "cargando": la primera vez se muestra el esqueleto un instante
            // (los datos están en memoria, así que la espera es solo ilustrativa)
            if (primeraCarga)
            {
                primeraCarga = false;
                MostrarCargando();
                await Task.Delay(700);
            }
            Cargar();
        }

        private void MostrarCargando()
        {
            chips.IsVisible = false;
            lista.IsVisible = false;
            estadoVista.IsVisible = false;
            skeleton.IsVisible = true;
            skeleton.Iniciar();
        }

        private void OcultarCargando()
        {
            skeleton.Detener();
            skeleton.IsVisible = false;
        }

        // Estado "error": no se pudo leer la base de datos
        private void MostrarError()
        {
            OcultarCargando();
            chips.IsVisible = false;
            lista.IsVisible = false;
            estadoVista.Mostrar("icono_error.svg", Color.FromArgb("#D32F2F"),
                "No se pudieron cargar los proyectos",
                "Ocurrió un problema al leer los datos. Inténtalo de nuevo.",
                "Reintentar", async () =>
                {
                    MostrarCargando();
                    await Task.Delay(500);
                    Cargar();
                });
        }

        private void Cargar()
        {
            try
            {
                CargarDatos();
            }
            catch (Exception)
            {
                MostrarError();
            }
        }

        private void CargarDatos()
        {
            if (Sesion.DemoEstado == 2)
                throw new InvalidOperationException("Error simulado");

            var dbContext = new NexoraDbContext();
            var proyectos = Sesion.DemoEstado == 1
                ? new List<Proyecto>()
                : dbContext.Proyectos.OrderBy(p => p.FechaFin).ToList();
            var tareas = dbContext.Tareas.ToList();
            var miembros = dbContext.Miembros.Include(m => m.Usuario).ToList();

            OcultarCargando();
            estadoVista.IsVisible = false;
            chips.IsVisible = proyectos.Count > 0;
            lista.IsVisible = true;

            // ---------- Filtros con contador ----------
            chips.Children.Clear();
            foreach (var estado in Estados)
            {
                var cantidad = estado == "Todos" ? proyectos.Count : proyectos.Count(p => p.Estado == estado);
                var activo = estado == filtro;

                var etiqueta = new Label
                {
                    Text = $"{estado} ({cantidad})",
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
                var nombreEstado = estado;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) =>
                {
                    filtro = nombreEstado;
                    Cargar();
                };
                chip.GestureRecognizers.Add(tap);
                chips.Children.Add(chip);
            }

            // ---------- Tarjetas ----------
            var items = proyectos
                .Where(p => filtro == "Todos" || p.Estado == filtro)
                .Select(p =>
                {
                    var deProyecto = tareas.Where(t => t.ProyectoId == p.Id).ToList();
                    var avance = deProyecto.Count == 0 ? 0 : (int)Math.Round(100.0 * deProyecto.Count(t => t.Estado == "Hecha") / deProyecto.Count);
                    var equipo = miembros.Where(m => m.ProyectoId == p.Id).ToList();
                    var avatares = equipo.Take(3)
                        .Select(m => new AvatarItem(Estilos.Iniciales(m.Usuario.Nombre), Estilos.ColorAvatar(m.UsuarioId)))
                        .ToList();
                    var resto = equipo.Count - avatares.Count;
                    if (resto > 0)
                        avatares.Add(new AvatarItem($"+{resto}", Color.FromArgb("#E0E0E0")));

                    return new ProyectoItem(
                        p.Id, p.Nombre, p.Descripcion, p.Estado,
                        Estilos.FondoEstadoProyecto(p.Estado), Estilos.PuntoEstadoProyecto(p.Estado),
                        avance, avance / 100.0, $"Entrega {p.FechaFin:dd/MM/yyyy}",
                        avatares, "", false);
                })
                .ToList();

            BindableLayout.SetItemsSource(lista, items);

            // ---------- Estado vacío ----------
            if (proyectos.Count == 0)
            {
                lista.IsVisible = false;
                estadoVista.Mostrar("icono_carpeta_vacia.svg", Estilos.Primario,
                    "Aún no tienes proyectos",
                    "Crea el primero para empezar a organizar el trabajo de tu equipo.",
                    "Crear proyecto", async () => await Shell.Current.GoToAsync(nameof(ProyectoFormPage)));
            }
            else if (items.Count == 0)
            {
                lista.IsVisible = false;
                estadoVista.Mostrar("icono_buscar_naranja.svg", Estilos.Primario,
                    $"No hay proyectos \"{filtro}\"",
                    "Prueba con otro estado para ver más proyectos.",
                    "Ver todos", () =>
                    {
                        filtro = "Todos";
                        Cargar();
                    });
            }
        }

        private async void OnProyectoTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject { BindingContext: ProyectoItem item })
                await Shell.Current.GoToAsync($"{nameof(ProyectoDetailPage)}?id={item.Id}");
        }

        private async void OnNuevoProyectoTapped(object? sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ProyectoFormPage));
        }
    }
}
