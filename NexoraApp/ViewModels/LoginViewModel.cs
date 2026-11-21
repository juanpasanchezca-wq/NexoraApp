using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexoraApp.DataAccess;
using NexoraApp.Helpers;
using NexoraApp.Services;

namespace NexoraApp.ViewModels
{
    // Pantalla de acceso: valida correo y contraseña contra la tabla Usuarios (SQLite)
    public partial class LoginViewModel : ViewModelGlobal
    {
        private readonly INavegacionService navegacion;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(IniciarSesionCommand))]
        private string correo = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(IniciarSesionCommand))]
        private string contrasena = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HayError))]
        private string mensajeError = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoMostrar))]
        private bool ocultarContrasena = true;

        public bool HayError => !string.IsNullOrEmpty(MensajeError);
        public string TextoMostrar => OcultarContrasena ? "Mostrar" : "Ocultar";

        public LoginViewModel(INavegacionService navegacion)
        {
            this.navegacion = navegacion;
        }

        // Si ya había una sesión guardada, entra directo
        public async Task RestaurarSesionAsync()
        {
            if (Sesion.Restaurar())
                await navegacion.IrAAsync("//MainPage");
        }

        [RelayCommand]
        private void AlternarContrasena() => OcultarContrasena = !OcultarContrasena;

        private bool PuedeEntrar() =>
            !string.IsNullOrWhiteSpace(Correo) && !string.IsNullOrEmpty(Contrasena);

        [RelayCommand(CanExecute = nameof(PuedeEntrar))]
        private async Task IniciarSesionAsync()
        {
            MensajeError = "";
            var correoBuscado = Correo.Trim().ToLowerInvariant();

            var dbContext = new NexoraDbContext();
            var usuario = dbContext.Usuarios.FirstOrDefault(u => u.Correo.ToLower() == correoBuscado);

            // Mismo mensaje si falla el correo o la contraseña (no se revela cuál)
            if (usuario == null || !Seguridad.Verificar(usuario.Correo, Contrasena, usuario.ContrasenaHash))
            {
                MensajeError = "Correo o contraseña incorrectos.";
                return;
            }

            Sesion.Iniciar(usuario.Id);
            Contrasena = "";
            Correo = "";
            await navegacion.IrAAsync("//MainPage");
        }
    }
}
