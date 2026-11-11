using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls.Shapes;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;

namespace NexoraApp.Views
{
    // Sirve para crear (sin id) y para editar (con ?id=)
    public partial class ProyectoFormPage : ContentPage, IQueryAttributable
    {
        private static readonly string[] Estados = { "Planificado", "En curso", "Pausado", "Finalizado" };

        private int? proyectoId;
        private string estadoSeleccionado = "Planificado";
        private readonly List<Usuario> usuarios;
        private readonly Dictionary<int, CheckBox> checks = new();
        private readonly List<(Border borde, Label etiqueta, string estado)> botonesEstado = new();

        public ProyectoFormPage()
        {
            InitializeComponent();

            var dbContext = new NexoraDbContext();
            usuarios = dbContext.Usuarios.OrderBy(u => u.Nombre).ToList();
            responsablePicker.ItemsSource = usuarios;

            inicioPicker.Date = DateTime.Today;
            finPicker.Date = DateTime.Today.AddDays(30);

            // Selector de estado (2 x 2)
            for (int i = 0; i < Estados.Length; i++)
            {
                var estado = Estados[i];
                var etiqueta = new Label { Text = estado, HorizontalOptions = LayoutOptions.Center };
                var borde = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Stroke = Color.FromArgb("#BDBDBD"),
                    Padding = new Thickness(0, 12),
                    Content = etiqueta
                };
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) => SeleccionarEstado(estado);
                borde.GestureRecognizers.Add(tap);
                Grid.SetRow(borde, i / 2);
                Grid.SetColumn(borde, i % 2);
                estadoGrid.Children.Add(borde);
                botonesEstado.Add((borde, etiqueta, estado));
            }
            SeleccionarEstado("Planificado");

            // Lista de miembros con casillas
            foreach (var u in usuarios)
            {
                var check = new CheckBox { Color = Estilos.Primario, VerticalOptions = LayoutOptions.Center };
                checks[u.Id] = check;
                var fila = new HorizontalStackLayout { Spacing = 8 };
                fila.Children.Add(check);
                fila.Children.Add(new Label { Text = $"{u.Nombre}  ·  {u.Rol}", VerticalOptions = LayoutOptions.Center });
                miembrosLayout.Children.Add(fila);
            }
        }

        // Modo edición: carga los datos del proyecto
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (!query.ContainsKey("id")) return;

            proyectoId = int.Parse(query["id"].ToString()!);
            Title = "Editar proyecto";

            var dbContext = new NexoraDbContext();
            var proyecto = dbContext.Proyectos.First(p => p.Id == proyectoId);

            nombreEntry.Text = proyecto.Nombre;
            descripcionEditor.Text = proyecto.Descripcion;
            inicioPicker.Date = proyecto.FechaInicio;
            finPicker.Date = proyecto.FechaFin;
            SeleccionarEstado(proyecto.Estado);
            responsablePicker.SelectedItem = usuarios.First(u => u.Id == proyecto.ResponsableId);

            foreach (var m in dbContext.Miembros.Where(m => m.ProyectoId == proyectoId))
                checks[m.UsuarioId].IsChecked = true;
        }

        private void SeleccionarEstado(string estado)
        {
            estadoSeleccionado = estado;
            foreach (var (borde, etiqueta, nombre) in botonesEstado)
            {
                var activo = nombre == estado;
                borde.BackgroundColor = activo ? Estilos.Primario : Colors.Transparent;
                borde.StrokeThickness = activo ? 0 : 1;
                etiqueta.FontAttributes = activo ? FontAttributes.Bold : FontAttributes.None;
                if (activo) etiqueta.TextColor = Estilos.Texto;
                else etiqueta.ClearValue(Label.TextColorProperty);
            }
        }

        private void MostrarError(string mensaje)
        {
            errorLabel.Text = mensaje;
            errorLabel.IsVisible = true;
        }

        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            errorLabel.IsVisible = false;

            var nombre = nombreEntry.Text?.Trim() ?? "";
            var descripcion = descripcionEditor.Text?.Trim() ?? "";
            var inicio = Convert.ToDateTime((object?)inicioPicker.Date);
            var fin = Convert.ToDateTime((object?)finPicker.Date);

            if (nombre.Length == 0) { MostrarError("Escribe el nombre del proyecto."); return; }
            if (fin < inicio) { MostrarError("La fecha de entrega no puede ser anterior al inicio."); return; }
            if (responsablePicker.SelectedItem is not Usuario responsable)
            {
                MostrarError("Selecciona al responsable del proyecto.");
                return;
            }

            var dbContext = new NexoraDbContext();
            int id;

            if (proyectoId == null)
            {
                // Crear
                id = (dbContext.Proyectos.Any() ? dbContext.Proyectos.Max(p => p.Id) : 0) + 1;
                dbContext.Proyectos.Add(new Proyecto(id, nombre, descripcion, inicio, fin, estadoSeleccionado, responsable.Id));
            }
            else
            {
                // Editar: se copian los valores nuevos sobre el registro existente
                id = proyectoId.Value;
                var original = dbContext.Proyectos.First(p => p.Id == id);
                dbContext.Entry(original).CurrentValues.SetValues(
                    new Proyecto(id, nombre, descripcion, inicio, fin, estadoSeleccionado, responsable.Id));
                dbContext.Miembros.RemoveRange(dbContext.Miembros.Where(m => m.ProyectoId == id));
            }

            // El responsable siempre es parte del equipo
            checks[responsable.Id].IsChecked = true;
            var siguienteId = (dbContext.Miembros.Any() ? dbContext.Miembros.Max(m => m.Id) : 0) + 1;
            foreach (var (usuarioId, check) in checks.Where(c => c.Value.IsChecked))
                dbContext.Miembros.Add(new Miembro(siguienteId++, id, usuarioId));

            dbContext.SaveChanges();
            await Shell.Current.GoToAsync("..");
        }
    }
}
