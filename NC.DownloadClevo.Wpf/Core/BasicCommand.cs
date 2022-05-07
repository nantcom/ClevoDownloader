using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NC.DownloadClevo.Core
{
    public class BasicCommand : ICommand
    {
        private static readonly Func<object?, bool> DefaultCanExecuteDelegate = (obj) => true;

        public Func<object?, bool> CanExecuteDelegate { get; }

        public Action<object?> ExecuteDelegate { get; set; }

        public event EventHandler? CanExecuteChanged = delegate { };

        /// <summary>
        /// Create new instance of Basic Command by providing Execute Delegate and optionally CanExecute Delegate
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public BasicCommand(Action<object?> execute, Func<object?, bool>? canExecute = null )
        {
            this.ExecuteDelegate = execute;
            this.CanExecuteDelegate = canExecute ?? DefaultCanExecuteDelegate;
        }

        public void OnCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter)
        {
            return this.CanExecuteDelegate?.Invoke(parameter) ?? false;
        }

        public void Execute(object? parameter)
        {
            this.ExecuteDelegate(parameter);
        }
    }
}
