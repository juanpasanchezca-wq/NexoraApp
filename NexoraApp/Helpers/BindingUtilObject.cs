using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NexoraApp.Helpers
{
    // Clase de utilidad para el binding (Clase 8):
    // cualquier modelo que herede de ella avisa a la vista cuando una propiedad cambia.
    public class BindingUtilObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
        }

        // Asigna el valor y notifica solo si realmente cambió
        protected bool SetProperty<T>(ref T campo, T valor, [CallerMemberName] string? propiedad = null)
        {
            if (EqualityComparer<T>.Default.Equals(campo, valor)) return false;
            campo = valor;
            OnPropertyChanged(propiedad);
            return true;
        }
    }
}
