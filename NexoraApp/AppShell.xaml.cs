using Microsoft.Maui.ApplicationModel;
using NexoraApp.Helpers;

namespace NexoraApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Ajustes.AplicarTema();   // tema guardado en Perfil
        }

        // Cambia entre: normal, sin datos y error de carga
        private async void Demo_Clicked(object sender, EventArgs e)
        {
            Sesion.DemoEstado = (Sesion.DemoEstado + 1) % 3;
            var modo = Sesion.DemoEstado switch
            {
                1 => "Sin datos (estado vacío)",
                2 => "Error de carga",
                _ => "Normal"
            };
            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.DisplayAlert("Modo de prueba", $"{modo}.\nCambia de pestaña (Proyectos o Tareas) para verlo.", "Entendido");
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            var uri = new Uri("https://learn.microsoft.com/dotnet/maui/");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
    }
}
