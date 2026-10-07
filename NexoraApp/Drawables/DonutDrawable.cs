using Microsoft.Maui.Graphics;

namespace NexoraApp.Drawables
{
    // Dibuja el gráfico de dona del avance general
    public class DonutDrawable : IDrawable
    {
        public List<(float Valor, Color Color)> Segmentos { get; set; } = new();
        public float Grosor { get; set; } = 14;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float cx = dirtyRect.Center.X;
            float cy = dirtyRect.Center.Y;
            float radio = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2 - Grosor / 2;
            float total = Segmentos.Sum(s => s.Valor);

            canvas.StrokeSize = Grosor;
            canvas.StrokeLineCap = LineCap.Butt;

            if (total <= 0)
            {
                canvas.StrokeColor = Color.FromRgba(0, 0, 0, 0.12f);
                canvas.DrawCircle(cx, cy, radio);
                return;
            }

            int activos = Segmentos.Count(s => s.Valor > 0);
            float hueco = activos > 1 ? 3f : 0f;   // separación entre segmentos (grados)
            float angulo = 0f;                      // grados desde arriba, en sentido horario

            foreach (var (valor, color) in Segmentos)
            {
                if (valor <= 0) continue;

                float barrido = valor / total * 360f;
                float inicio = angulo + hueco / 2;
                float fin = angulo + barrido - hueco / 2;
                angulo += barrido;
                if (fin <= inicio) continue;

                var path = new PathF();
                int pasos = Math.Max(2, (int)((fin - inicio) / 2));
                for (int i = 0; i <= pasos; i++)
                {
                    float a = inicio + (fin - inicio) * i / pasos;
                    double rad = a * Math.PI / 180.0;
                    float x = cx + radio * (float)Math.Sin(rad);
                    float y = cy - radio * (float)Math.Cos(rad);
                    if (i == 0) path.MoveTo(x, y);
                    else path.LineTo(x, y);
                }

                canvas.StrokeColor = color;
                canvas.DrawPath(path);
            }
        }
    }
}
