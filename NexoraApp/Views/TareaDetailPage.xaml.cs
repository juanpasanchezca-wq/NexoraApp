using NexoraApp.Helpers;
using NexoraApp.ViewModels;

namespace NexoraApp.Views
{
    // La vista ya no tiene lógica de datos: recibe su ViewModel por inyección (Clase 11)
    public partial class TareaDetailPage : ContentPage
    {
        private static readonly string[] Estados = { "Por hacer", "En progreso", "En revisión", "Hecha" };

        private readonly TareaDetailViewModel viewModel;
        private readonly SelectorOpciones selectorEstado;

        public TareaDetailPage(TareaDetailViewModel viewModel)
        {
            InitializeComponent();

            this.viewModel = viewModel;
            BindingContext = viewModel;

            selectorEstado = new SelectorOpciones(estadoGrid, Estados, 2, nuevo => viewModel.CambiarEstado(nuevo));
        }

        // Se recarga al volver del formulario de edición
        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.Cargar();
            selectorEstado.Seleccionar(viewModel.Estado);
        }
    }
}
