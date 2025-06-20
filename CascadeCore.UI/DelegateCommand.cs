using System;
using System.Windows.Input;

namespace CascadePass.CascadeCore.UI
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly Action _executeNoParam;

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute;
        }

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            this._executeNoParam = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute == null ? (Func<object, bool>)null : new Func<object, bool>(_ => canExecute());
        }

        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (this._execute != null)
            {
                this._execute(parameter);
            }
            else
            {
                this._executeNoParam();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
