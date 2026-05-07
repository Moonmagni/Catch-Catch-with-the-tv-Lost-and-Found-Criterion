using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Func<object?, bool>? _canExecute;
        private bool _isRunning;

        public AsyncRelayCommand(Func<object?, Task> execute)
            : this(execute, _ => true)
        { }

        public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !_isRunning && _canExecute(parameter);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            _isRunning = true;
            try
            {
                RaiseCanExecuteChanged();
                await _execute(parameter);
            }
            finally
            {
                _isRunning = false;
                RaiseCanExecuteChanged();
            }
        }

        protected virtual void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}