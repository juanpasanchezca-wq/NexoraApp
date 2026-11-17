using Microsoft.EntityFrameworkCore;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    public partial class ProyectoDetailPage : ContentPage, IQueryAttributable
    {
        private int proyectoId;
        private int tabActual;

        public ProyectoDetailPage()
        {
            InitializeComponent();
            MostrarTab(0);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            proyectoId = int.Parse(query["id"].ToString()!);
        }

        // Se recarga al volver del formulario de edición
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Cargar();
        }

        private void Cargar()
        {
            var dbContext = new NexoraDbContext();
            var proyecto = dbContext.Proyectos
                .Include(p => p.Responsable)
                .FirstOrDefault(p => p.Id == proyectoId);
            if (proyecto == null) return;

            var tareas = dbContext.Tareas
                .Include(t => t.Usuario)
                .Where(t => t.ProyectoId == proyectoId)
                .OrderBy(t => t.FechaLimite)
                .ToList();
            var miembros = dbContext.Miembros
                .Include(m => m.Usuario)
                .Where(m => m.ProyectoId == proyectoId)
                .ToList();

            // ---------- Encabezado ----------
            Title = "Proyecto";
            nombreLabel.Text = proyecto.Nombre;
            descripcionLabel.Text = proyecto.Descripcion;
            estadoLabel.Text = proyecto.Estado;
            estadoChip.BackgroundColor = Estilos.FondoEstadoProyecto(proyecto.Estado);
            estadoPunto.BackgroundColor = Estilos.PuntoEstadoProyecto(proyecto.Estado);
            inicioLabel.Text = proyecto.FechaInicio.ToString("dd/MM/yyyy");
            entregaLabel.Text = proyecto.FechaFin.ToString("dd/MM/yyyy");
            responsableLabel.Text = proyecto.Responsable.Nombre.Split(' ')[0];

            // ---------- Resumen ----------
            var hechas = tareas.Count(t => t.Estado == "Hecha");
            var avance = tareas.Count == 0 ? 0 : (int)Math.Round(100.0 * hechas / tareas.Count);
            totalLabel.Text = tareas.Count.ToString();
            hechasLabel.Text = hechas.ToString();
            activasLabel.Text = tareas.Count(t => t.Estado == "En progreso" || t.Estado == "En revisión").ToString();
            vencidasLabel.Text = tareas.Count(t => t.Estado != "Hecha" && t.FechaLimite.Date < DateTime.Today).ToString();
            avanceLabel.Text = $"{avance}%";
            avanceBarra.Progress = avance / 100.0;

            var filas = new[] { "Por hacer", "En progreso", "En revisión", "Hecha" }
                .Select(estado =>
                {
                    var cantidad = tareas.Count(t => t.Estado == estado);
                    return new EstadoFila(estado, cantidad,
                        tareas.Count == 0 ? 0 : (double)cantidad / tareas.Count,
                        Estilos.ColorEstadoTarea(estado));
                })
                .ToList();
            BindableLayout.SetItemsSource(porEstado, filas);

            // ---------- Tareas ----------
            var filasTareas = tareas
                .Select(t => new TareaFila(t.Id, t.Titulo, t.Estado, Estilos.ColorEstadoTarea(t.Estado),
                    Estilos.TextoVencimiento(t.FechaLimite, t.Estado), t.Usuario.Nombre.Split(' ')[0]))
                .ToList();
            sinTareasLabel.IsVisible = filasTareas.Count == 0;
            BindableLayout.SetItemsSource(listaTareas, filasTareas);

            // ---------- Miembros ----------
            var filasMiembros = miembros
                .Select(m => new MiembroItem(Estilos.Iniciales(m.Usuario.Nombre), Estilos.ColorAvatar(m.UsuarioId),
                    m.Usuario.Nombre, m.Usuario.Rol))
                .ToList();
            BindableLayout.SetItemsSource(listaMiembros, filasMiembros);
        }

        // ---------- Pestañas ----------
        private void OnTabTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject bo)
                MostrarTab(Grid.GetColumn(bo));
        }

        private void MostrarTab(int indice)
        {
            tabActual = indice;
            panelResumen.IsVisible = indice == 0;
            panelTareas.IsVisible = indice == 1;
            panelMiembros.IsVisible = indice == 2;

            var tabs = new[] { tab0, tab1, tab2 };
            var etiquetas = new[] { tabLabel0, tabLabel1, tabLabel2 };
            for (int i = 0; i < 3; i++)
            {
                var activa = i == indice;
                tabs[i].BackgroundColor = activa ? Estilos.Primario : Color.FromArgb("#33808080");
                etiquetas[i].TextColor = activa ? Estilos.Texto : Color.FromArgb("#757575");
                etiquetas[i].FontAttributes = activa ? FontAttributes.Bold : FontAttributes.None;
            }
        }

        private async void OnTareaTapped(object? sender, TappedEventArgs e)
        {
            if (sender is BindableObject { BindingContext: TareaFila fila })
                await Shell.Current.GoToAsync($"{nameof(TareaDetailPage)}?id={fila.Id}");
        }

        // ---------- Editar y eliminar ----------
        private async void OnNuevaTareaTapped(object? sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(TareaFormPage)}?proyectoId={proyectoId}");
        }

        private async void OnEditarClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(ProyectoFormPage)}?id={proyectoId}");
        }

        private async void OnEliminarClicked(object? sender, EventArgs e)
        {
            var confirmar = await DisplayAlert("Eliminar proyecto",
                "Se eliminarán también sus tareas y miembros. Esta acción no se puede deshacer.",
                "Eliminar", "Cancelar");
            if (!confirmar) return;

            var dbContext = new NexoraDbContext();
            dbContext.Tareas.RemoveRange(dbContext.Tareas.Where(t => t.ProyectoId == proyectoId));
            dbContext.Miembros.RemoveRange(dbContext.Miembros.Where(m => m.ProyectoId == proyectoId));
            var proyecto = dbContext.Proyectos.FirstOrDefault(p => p.Id == proyectoId);
            if (proyecto != null) dbContext.Proyectos.Remove(proyecto);
            dbContext.SaveChanges();

            await Shell.Current.GoToAsync("..");
        }
    }
}
