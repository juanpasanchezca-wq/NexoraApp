using System.Windows.Input;

namespace NexoraApp.ViewModels
{
    public class SubtareaItem : ViewModelGlobal
    {
        private bool completada;

        public SubtareaItem(int id, string titulo, bool completada)
        {
            Id = id;
            Titulo = titulo;
            this.completada = completada;
        }

        public int Id { get; }
        public string Titulo { get; }

        // Comando compartido para quitar la subtarea (CommandParameter = Id)
        public ICommand? EliminarCommand { get; set; }

        public bool Completada
        {
            get => completada;
            set => SetProperty(ref completada, value);
        }
    }
}
