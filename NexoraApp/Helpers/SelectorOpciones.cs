using Microsoft.Maui.Controls.Shapes;

namespace NexoraApp.Helpers
{
    // Botones de opción (estado, prioridad...) dentro de un Grid; solo uno queda activo
    public class SelectorOpciones
    {
        private readonly List<(Border borde, Label etiqueta, string valor)> botones = new();
        private readonly Action<string>? alCambiar;

        public string Valor { get; private set; } = "";

        public SelectorOpciones(Grid destino, string[] opciones, int columnas, Action<string>? alCambiar = null)
        {
            this.alCambiar = alCambiar;
            for (int i = 0; i < opciones.Length; i++)
            {
                var valor = opciones[i];
                var etiqueta = new Label { Text = valor, FontSize = 13, HorizontalOptions = LayoutOptions.Center };
                var borde = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Stroke = Color.FromArgb("#BDBDBD"),
                    Padding = new Thickness(0, 11),
                    Content = etiqueta
                };
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) => Seleccionar(valor, true);
                borde.GestureRecognizers.Add(tap);
                Grid.SetRow(borde, i / columnas);
                Grid.SetColumn(borde, i % columnas);
                destino.Children.Add(borde);
                botones.Add((borde, etiqueta, valor));
            }
        }

        public void Seleccionar(string valor, bool notificar = false)
        {
            Valor = valor;
            foreach (var (borde, etiqueta, v) in botones)
            {
                var activo = v == valor;
                borde.BackgroundColor = activo ? Estilos.Primario : Colors.Transparent;
                borde.StrokeThickness = activo ? 0 : 1;
                etiqueta.FontAttributes = activo ? FontAttributes.Bold : FontAttributes.None;
                if (activo) etiqueta.TextColor = Estilos.Texto;
                else etiqueta.ClearValue(Label.TextColorProperty);
            }
            if (notificar) alCambiar?.Invoke(valor);
        }
    }
}
