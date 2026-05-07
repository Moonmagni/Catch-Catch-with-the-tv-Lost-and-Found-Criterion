using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Core
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Action<object> _executeSync;
        private readonly Predicate<object> _canExecute;

        // For async commands (Save, Update, Delete, Approve, Reject)
        public RelayCommand(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _executeAsync = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // For sync commands (Clear, Back)
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _executeSync = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // THIS is the critical fix — hook into WPF's global requery system
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
            => _canExecute == null || _canExecute(parameter);

        public async void Execute(object parameter)
        {
            if (_executeAsync != null)
                await _executeAsync(parameter);
            else
                _executeSync(parameter);
        }
    }
}