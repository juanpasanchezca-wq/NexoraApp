using System.Windows.Input;

namespace NexoraApp.Helpers
{
    // Command personalizado (Clase 10): implementa ICommand
    //  - Execute: lo que se hace al invocar el comando (el "action")
    //  - CanExecute: condición que decide si se puede ejecutar (si es false, el botón se desactiva)
    //  - CanExecuteChanged: aviso a la vista de que esa condición cambió
    public class MiComando : ICommand
    {
        private readonly Action<object?> action;
        private readonly Func<object?, bool>? canExecute;

        public MiComando(Action<object?> action, Func<object?, bool>? canExecute = null)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => action(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
