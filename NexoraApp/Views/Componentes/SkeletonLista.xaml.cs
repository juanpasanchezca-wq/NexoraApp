namespace NexoraApp.Views.Componentes
{
    // Esqueleto de carga: tarjetas grises que pulsan hasta que llegan los datos
    public partial class SkeletonLista : ContentView
    {
        private bool animando;

        public SkeletonLista()
        {
            InitializeComponent();
        }

        public async void Iniciar()
        {
            if (animando) return;
            animando = true;
            while (animando)
            {
                await contenedor.FadeToAsync(0.4, 700);
                if (!animando) break;
                await contenedor.FadeToAsync(1, 700);
            }
        }

        public void Detener()
        {
            animando = false;
            contenedor.CancelAnimations();
            contenedor.Opacity = 1;
        }
    }
}
