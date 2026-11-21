using NexoraApp.ViewModels;

namespace NexoraApp.Views
{
    public partial class SincronizacionPage : ContentPage
    {
        private readonly SincronizacionViewModel viewModel;

        public SincronizacionPage(SincronizacionViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.Iniciar();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewModel.Detener();
        }
    }
}
