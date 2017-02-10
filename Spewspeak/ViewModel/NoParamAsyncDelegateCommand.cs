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
    /// An asynchronous version of DelegateCommand with no parameters.
    /// </summary>
    class NoParamAsyncDelegateCommand : ICommand
    {
        BackgroundWorker _worker = new BackgroundWorker();
        Func<bool> _canExecute;

        public NoParamAsyncDelegateCommand(Action action, Func<bool> canExecute = null, Action<object> completed = null, Action<Exception> error = null)
        {
            _worker.DoWork += (s, e) =>
            {
                CommandManager.InvalidateRequerySuggested();
                action();
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
        /// To cancel an ongoing execution.
        /// </summary>
        public void Cancel()
        {
            if (_worker.IsBusy) { _worker.CancelAsync(); }
        }

        /// <summary>
        /// Returns whether the DelegateCommand can be executed currently. Will return false if the async process is currently in progress.
        /// </summary>
        /// <returns>Boolean true if the Command can be started. Boolean false otherwise.</returns>
        public bool CanExecute()
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

        public void Execute()
        {
            _worker.RunWorkerAsync();
        }

        public bool CanExecute(object parameter)
        {
            return this.CanExecute();
        }

        public void Execute(object parameter)
        {
            this.Execute();
        }
    }
}
