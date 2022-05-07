using Jint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NC.Lib
{
    public class JIntExecutor
    {
        public Action<Engine> ConfigureEngine { get; set; }

        private string _lastScript;
        private Engine _parsedFunction;
        private JIntContext _context = new JIntContext();

        private class JIntContext
        {
            public object value;
            public object referenceValue;
            public object referenceValue2;
            public object defaultValue;

            public void log(string message)
            {
                Debug.WriteLine(message, "JIntExecutor");
            }
        }

        /// <summary>
        /// Parse the script, cache it and return execution value
        /// In subsequent run, parsed script will be used
        /// </summary>
        /// <param name="script"></param>
        /// <param name="inputValue"></param>
        /// <param name="referenceValue"></param>
        /// <param name="referenceValue2"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public object Eval( string script, Type targetType = null, object inputValue = null, object referenceValue = null, object referenceValue2 = null, object defaultValue = null )
        {
            if (script == null)
            {
                return null;
            }

            if (_lastScript != script)
            {
                _lastScript = script;

                var e = new Engine((opt) =>
                {
                });

                this.ConfigureEngine?.Invoke(e);

                e.SetValue("input", _context);
                _parsedFunction = e.Execute($"function ex() {{ {script} }}");
            }

            if (_parsedFunction == null)
            {
                return "Script Could Not Compile";
            }

            _context.value = inputValue;
            _context.referenceValue = referenceValue;
            _context.referenceValue2 = referenceValue2;
            _context.defaultValue = defaultValue;

            return _parsedFunction.Invoke("ex").ToObject();
        }
    }
}
