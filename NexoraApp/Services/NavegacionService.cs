namespace NexoraApp.Services
{
    // Implementación con Shell. Es la única clase que depende de la API de navegación.
    public class NavegacionService : INavegacionService
    {
        public Task IrAAsync(string ruta) => Shell.Current.GoToAsync(ruta);

        public Task VolverAsync() => Shell.Current.GoToAsync("..");

        public Task<bool> ConfirmarAsync(string titulo, string mensaje, string aceptar, string cancelar)
            => Shell.Current.DisplayAlert(titulo, mensaje, aceptar, cancelar);
    }
}
