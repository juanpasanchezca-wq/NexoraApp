namespace NexoraApp.Views.Componentes
{
    // Pantalla reutilizable para estados vacíos y de error
    public partial class EstadoVista : ContentView
    {
        private Action? alPulsar;

        public EstadoVista()
        {
            InitializeComponent();
        }

        // textoBoton = null oculta el botón
        public void Mostrar(string iconoArchivo, Color acento, string titulo, string mensaje,
                            string? textoBoton = null, Action? accion = null)
        {
            icono.Source = iconoArchivo;
            circulo.BackgroundColor = acento.WithAlpha(0.15f);
            tituloLabel.Text = titulo;
            mensajeLabel.Text = mensaje;
            boton.Text = textoBoton ?? "";
            boton.IsVisible = textoBoton != null;
            alPulsar = accion;
            IsVisible = true;
        }

        private void OnBotonClicked(object? sender, EventArgs e)
        {
            alPulsar?.Invoke();
        }
    }
}
