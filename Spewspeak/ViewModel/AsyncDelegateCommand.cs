using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Spewspeak.ViewModel
{
    /// <summary>
    /// An async version for delegate command
    /// </summary>
    class AsyncDelegateCommand : ICommand
    {
        BackgroundWorker _worker = new BackgroundWorker();
        Func<bool> _canExecute;

        /// <summary>
        /// Constructor for an asynchronous DelegateCommand.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="canExecute">Used to determine if the action can be executed.</param>
        /// <param name="completed">Invoked when the action is completed.</param>
        /// <param name="error">Invoked if the action throws an error.</param>
        public AsyncDelegateCommand(Action<object> action, Func<bool> canExecute = null, Action<object> completed = null, Action<Exception> error = null)
        {
            _worker.DoWork += (s, e) =>
            {
                CommandManager.InvalidateRequerySuggested();
                action(e.Argument);
            };

            _worker.RunWorkerCompleted += (s, e) =>
            {
                if (completed != null && e.Error == null) { completed(e.Result); }

                if (error != null && e.Error != null) { error(e.Error); }

                CommandManager.InvalidateRequerySuggested();
            };

            _canExecute = canExecute;
        }

        /// <summary>
        /// To cancel an ongoing execution
        /// </summary>
        public void Cancel()
        {
            if (_worker.IsBusy) { _worker.CancelAsync(); }
        }

        /// <summary>
        /// Returns whether the DelegateCommand can be executed currently. Willreturn false if the async process is currently in progress.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>True if the Command can be started. False otherwise.</returns>
        public bool CanExecute(object parameter)
        {
            return (_canExecute == null) ?
                !(_worker.IsBusy) : !(_worker.IsBusy)
                    && _canExecute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _worker.RunWorkerAsync(parameter);
        }
    }
}
