
using Jint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace NC.Lib
{
    public class JsCommand : ICommand
    {
        /// <summary>
        /// Optional Reference Value
        /// </summary>
        public object ReferenceValue { get; set; }

        /// <summary>
        /// Script to run
        /// </summary>
        public string ExecuteScript { get; set; }

        /// <summary>
        /// Script, which returns bool, to specify whether script can be ran
        /// </summary>
        public string CanExecuteScript { get; set; }

        public event EventHandler CanExecuteChanged = delegate { };

        public void ChangeCanExecute()
        {
            this.CanExecuteChanged(this, EventArgs.Empty);
        }

        private JIntExecutor _canExecuteScript = new JIntExecutor();
        private JIntExecutor _executeScript = new JIntExecutor()
        {
            ConfigureEngine = (jint) =>
            {
                jint.SetValue("alert", new Action<string, string, string>((msg, title, ok) =>
                {
                    Debug.WriteLine(msg);
                }));
                jint.SetValue("log", new Action<string>((msg) =>
                {
                    Debug.WriteLine(msg);
                }));
            }
        };

        public bool CanExecute(object parameter)
        {
            if (this.CanExecuteScript == null)
            {
                return true;
            }

            try
            {
                var obj = _canExecuteScript.Eval( this.CanExecuteScript, typeof(bool), parameter, this.ReferenceValue);
                Debug.Assert(obj is bool, "JsCommand result is not boolean");

                if (obj is bool b)
                {
                    return b;                
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Execute(object parameter)
        {
            try
            {
                _executeScript.Eval( this.ExecuteScript,
                    inputValue: parameter,
                    referenceValue: this.ReferenceValue);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debugger.Break();
#endif
            }
        }
    }
}
