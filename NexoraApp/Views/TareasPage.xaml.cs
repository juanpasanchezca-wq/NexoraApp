using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls.Shapes;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    public partial class TareasPage : ContentPage
    {
        private static readonly string[] Estados = { "Por hacer", "En progreso", "En revisión", "Hecha" };
        private static readonly string[] Filtros = { "Todas", "Mis tareas", "Prioridad alta", "Vencidas" };

        private bool verKanban;
        private string filtro = "Todas";

        public TareasPage()
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
            var tareas = dbContext.Tareas
                .Include(t => t.Proyecto)
                .Include(t => t.Usuario)
                .OrderBy(t => t.FechaLimite)
                .ToList();

            // ---------- Filtros con contador ----------
            chips.Children.Clear();
            foreach (var nombre in Filtros)
            {
                var cantidad = AplicarFiltro(tareas, nombre).Count;
                var activo = nombre == filtro;

                var etiqueta = new Label
                {
                    Text = $"{nombre} ({cantidad})",
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
                var elegido = nombre;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) =>
                {
                    filtro = elegido;
                    Cargar();
                };
                chip.GestureRecognizers.Add(tap);
                chips.Children.Add(chip);
            }

            // ---------- Grupos por estado ----------
            var filtradas = AplicarFiltro(tareas, filtro);
            var grupos = Estados
                .Select(estado =>
                {
                    var tarjetas = filtradas
                        .Where(t => t.Estado == estado)
                        .Select(t => new TareaCard(
                            t.Id, t.Titulo, t.Proyecto.Nombre,
                            t.Prioridad, Estilos.FondoPrioridad(t.Prioridad),
                            t.Estado, Estilos.ColorEstadoTarea(t.Estado),
                            Estilos.TextoVencimiento(t.FechaLimite, t.Estado), Estilos.ColorVencimiento(t.FechaLimite, t.Estado),
                            Estilos.Iniciales(t.Usuario.Nombre), Estilos.ColorAvatar(t.UsuarioId)))
                        .ToList();
                    return new GrupoTareas(estado, tarjetas.Count, Estilos.ColorEstadoTarea(estado), tarjetas);
                })
                .ToList();

            BindableLayout.SetItemsSource(panelLista, grupos);
            BindableLayout.SetItemsSource(kanbanColumnas, grupos);
            vacioLabel.IsVisible = filtradas.Count == 0;
            MostrarVista();
        }

        private List<Tarea> AplicarFiltro(List<Tarea> tareas, string nombre) => nombre switch
        {
            "Mis tareas" => tareas.Where(t => t.UsuarioId == Sesion.UsuarioActualId).ToList(),
            "Prioridad alta" => tareas.Where(t => t.Prioridad == "Alta" && t.Estado != "Hecha").ToList(),
            "Vencidas" => tareas.Where(t => t.Estado != "Hecha" && t.FechaLimite.Date < DateTime.Today).ToList(),
            _ => tareas
        };

        // ---------- Lista / Kanban ----------
        private void MostrarVista()
        {
            panelLista.IsVisible = !verKanban;
            panelKanban.IsVisible = verKanban;

            vistaLista.BackgroundColor = !verKanban ? Estilos.Primario : Color.FromArgb("#33808080");
            vistaKanban.BackgroundColor = verKanban ? Estilos.Primario : Color.FromArgb("#33808080");
            vistaListaLabel.TextColor = !verKanban ? Estilos.Texto : Estilos.Gris;
            vistaKanbanLabel.TextColor = verKanban ? Estilos.Texto : Estilos.Gris;
            vistaListaLabel.FontAttributes = !verKanban ? FontAttributes.Bold : FontAttributes.None;
            vistaKanbanLabel.FontAttributes = verKanban ? FontAttributes.Bold : FontAttributes.None;
        }

        private void OnVistaListaTapped(object? sender, TappedEventArgs e)
        {
            verKanban = false;
            MostrarVista();
        }

        private void OnVistaKanbanTapped(object? sender, TappedEventArgs e)
        {
            verKanban = true;
            MostrarVista();
        }

        private async void OnTareaTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject { BindingContext: TareaCard tarjeta })
                await Shell.Current.GoToAsync($"{nameof(TareaDetailPage)}?id={tarjeta.Id}");
        }

        private async void OnNuevaTareaTapped(object? sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(TareaFormPage));
        }
    }
}
