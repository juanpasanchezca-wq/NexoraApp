using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls.Shapes;
using NexoraApp.DataAccess;
using NexoraApp.Drawables;

namespace NexoraApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Se recalcula cada vez que se abre la pestaña Inicio
        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarResumen();
        }

        private void CargarResumen()
        {
            var dbContext = new NexoraDbContext();
            var cultura = new CultureInfo("es-ES");
            var hoy = DateTime.Today;

            // ---------- Saludo ----------
            var usuario = dbContext.Usuarios.First(u => u.Id == Sesion.UsuarioActualId);
            var fecha = hoy.ToString("dddd, d 'de' MMMM", cultura);
            fechaLabel.Text = char.ToUpper(fecha[0]) + fecha[1..];
            saludoLabel.Text = $"Hola, {usuario.Nombre.Split(' ')[0]}";
            inicialesLabel.Text = string.Concat(
                usuario.Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => p[0]));

            // ---------- Tarjetas de resumen ----------
            var tareas = dbContext.Tareas.Include(t => t.Proyecto).ToList();
            var pendientes = tareas.Where(t => t.Estado != "Hecha").ToList();

            activosLabel.Text = dbContext.Proyectos.Count(p => p.Estado == "En curso").ToString();
            hoyLabel.Text = pendientes.Count(t => t.FechaLimite.Date == hoy).ToString();
            vencidasLabel.Text = pendientes.Count(t => t.FechaLimite.Date < hoy).ToString();

            // ---------- Avance general ----------
            int porHacer = tareas.Count(t => t.Estado == "Por hacer");
            int enProgreso = tareas.Count(t => t.Estado == "En progreso");
            int enRevision = tareas.Count(t => t.Estado == "En revisión");
            int hechas = tareas.Count(t => t.Estado == "Hecha");

            mesLabel.Text = cultura.TextInfo.ToTitleCase(hoy.ToString("MMMM", cultura));
            porcentajeLabel.Text = tareas.Count == 0 ? "0%" : $"{hechas * 100 / tareas.Count}%";
            porHacerLabel.Text = porHacer.ToString();
            enProgresoLabel.Text = enProgreso.ToString();
            enRevisionLabel.Text = enRevision.ToString();
            hechaLabel.Text = hechas.ToString();

            donut.Drawable = new DonutDrawable
            {
                Segmentos = new List<(float Valor, Color Color)>
                {
                    ((float)porHacer, Color.FromArgb("#BDBDBD")),
                    ((float)enProgreso, Color.FromArgb("#E67514")),
                    ((float)enRevision, Color.FromArgb("#757575")),
                    ((float)hechas, Color.FromArgb("#06923E"))
                }
            };
            donut.Invalidate();

            // ---------- Tareas completadas esta semana ----------
            CargarSemana(tareas, hoy);

            // ---------- Proyectos activos ----------
            proyectosActivos.ItemsSource = dbContext.Proyectos
                .Where(p => p.Estado == "En curso")
                .ToList()
                .Select(p =>
                {
                    var delProyecto = tareas.Where(t => t.ProyectoId == p.Id).ToList();
                    double avance = delProyecto.Count == 0
                        ? 0
                        : (double)delProyecto.Count(t => t.Estado == "Hecha") / delProyecto.Count;
                    return new ProyectoResumen(
                        p.Id,
                        p.Nombre,
                        p.Estado,
                        avance,
                        $"{Math.Round(avance * 100)}%",
                        $"Entrega {p.FechaFin.ToString("dd MMM yyyy", cultura)}");
                })
                .ToList();

            // ---------- Tareas de hoy y vencidas ----------
            var lista = pendientes
                .Where(t => t.FechaLimite.Date <= hoy)
                .OrderBy(t => t.FechaLimite)
                .Select(t =>
                {
                    int dias = (hoy - t.FechaLimite.Date).Days;
                    string vencimiento = dias == 0
                        ? "Vence hoy"
                        : $"Vencida hace {dias} {(dias == 1 ? "día" : "días")}";
                    return new TareaResumen(
                        t.Id,
                        t.Titulo,
                        t.Proyecto.Nombre,
                        vencimiento,
                        dias > 0,
                        t.Prioridad,
                        ColorPrioridad(t.Prioridad));
                })
                .ToList();

            BindableLayout.SetItemsSource(tareasLista, lista);
            sinTareasLabel.IsVisible = lista.Count == 0;
        }

        // Barras de lunes a domingo con las tareas completadas cada día
        private void CargarSemana(List<Tarea> tareas, DateTime hoy)
        {
            semanaGrid.Children.Clear();

            string[] dias = { "L", "M", "X", "J", "V", "S", "D" };
            int indiceHoy = ((int)hoy.DayOfWeek + 6) % 7;   // lunes = 0
            var lunes = hoy.AddDays(-indiceHoy);

            for (int i = 0; i < 7; i++)
            {
                int completadas = tareas.Count(t => t.FechaCompletada?.Date == lunes.AddDays(i));

                var barra = new Border
                {
                    HeightRequest = Math.Min(80, Math.Max(6, completadas * 24)),
                    WidthRequest = 28,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
                    StrokeThickness = 0,
                    BackgroundColor = i == indiceHoy ? Color.FromArgb("#E67514") : Color.FromArgb("#06923E"),
                    Opacity = completadas == 0 ? 0.35 : 1,
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.Center
                };

                var etiqueta = new Label
                {
                    Text = dias[i],
                    FontSize = 12,
                    FontAttributes = i == indiceHoy ? FontAttributes.Bold : FontAttributes.None,
                    HorizontalOptions = LayoutOptions.Center
                };

                semanaGrid.Add(barra, i, 0);
                semanaGrid.Add(etiqueta, i, 1);
            }
        }

        private static Color ColorPrioridad(string prioridad) => prioridad switch
        {
            "Alta" => Color.FromArgb("#E67514"),
            "Baja" => Color.FromArgb("#D3ECCD"),
            _ => Color.FromArgb("#E0E0E0")
        };

        private async void OnVerTodosTapped(object? sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("//ProyectosPage");
        }

        private async void OnProyectoTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject { BindingContext: ProyectoResumen proyecto })
            {
                await Shell.Current.GoToAsync($"{nameof(ProyectoDetailPage)}?id={proyecto.Id}");
            }
        }

        private async void OnTareaTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject { BindingContext: TareaResumen tarea })
            {
                await Shell.Current.GoToAsync($"{nameof(TareaDetailPage)}?id={tarea.Id}");
            }
        }
    }

    // Datos ya calculados para mostrar en las listas del resumen
    public record ProyectoResumen(int Id, string Nombre, string Estado, double Avance, string AvanceTexto, string Entrega);

    public record TareaResumen(int Id, string Titulo, string Proyecto, string Vencimiento, bool EstaVencida, string Prioridad, Color PrioridadFondo);
}
