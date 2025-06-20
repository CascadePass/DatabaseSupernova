using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CascadePass.CascadeCore.UI
{
    public class AsyncDelegateCommand<TProgress> : ICommand
    {
        private readonly Func<object, CancellationToken, IProgress<TProgress>, Task> _executeAsync;
        private readonly Func<object, bool> _canExecute;
        private readonly Func<CancellationToken, IProgress<TProgress>, Task> _executeNoParam;
        private CancellationTokenSource _cts;

        public AsyncDelegateCommand(Func<object, CancellationToken, IProgress<TProgress>, Task> executeAsync, Func<object, bool> canExecute = null)
        {
            this._executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this._canExecute = canExecute;
        }

        public AsyncDelegateCommand(Func<CancellationToken, IProgress<TProgress>, Task> executeAsync, Func<bool> canExecute = null)
        {
            this._executeNoParam = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this._canExecute = canExecute == null ? (Func<object, bool>)null : new Func<object, bool>(_ => canExecute());
        }

        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute(parameter);
        }

        public async Task ExecuteAsync(object parameter, IProgress<TProgress> progress)
        {
            this._cts?.Cancel();
            this._cts = new CancellationTokenSource();
            CancellationToken token = this._cts.Token;

            try
            {
                if (this._executeAsync != null)
                {
                    await this._executeAsync(parameter, token, progress);
                }
                else
                {
                    await this._executeNoParam(token, progress);
                }
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation gracefully
            }
        }

        public void Execute(object parameter)
        {
            var progress = new Progress<TProgress>(value => Console.WriteLine($"Progress: {value}")); // Example logging
            _ = ExecuteAsync(parameter, progress);
        }

        public void Cancel()
        {
            this._cts?.Cancel();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

}
