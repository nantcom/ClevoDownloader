using Jint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NC.Lib
{
    /// <summary>
    /// This converter executes Small JS Code quickly, good for custom calculations
    /// </summary>
    public class JSExpressionConverter : IValueConverter
    {
        /// <summary>
        /// Optional Reference Value for the Converter
        /// </summary>
        public object ReferenceValue { get; set; }

        /// <summary>
        /// Optional Reference Value for the Converter
        /// </summary>
        public object ReferenceValue2 { get; set; }

        /// <summary>
        /// Optional Resource Dictionary Source to use when RES- is returned from script
        /// </summary>
        public FrameworkElement ResourceContainer { get; set; }

        /// <summary>
        /// Default value for the script
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// The Default script for this converter, parameter will override this script
        /// </summary>
        public string Script { get; set; }

        private JIntExecutor _jsFunction = new JIntExecutor();

        private object GetDefaultValue(Type targetType, object fallbackValue)
        {
            if (targetType == typeof(bool))
            {
                return false;
            }

            if (targetType == typeof(Color))
            {
                return Colors.Transparent;
            }

            return fallbackValue;
        }

        public object Convert(object value, Type targetType, object script, CultureInfo culture)
        {
            if (this.Script != null)
            {
                script = this.Script;
            }

            if (script == null)
            {
                Debug.Fail("Script is Empty");
                return this.GetDefaultValue( targetType, value);
            }

            object result = null;
            try
            {
                result = _jsFunction.Eval(script as string, targetType, value, this.ReferenceValue, this.ReferenceValue2, this.DefaultValue);
            }
            catch (Exception ex)
            {
                Debug.Fail("Script Exception :" + ex.Message);
                return this.GetDefaultValue(targetType, value);
            }

            if (result is string s && s.StartsWith("RES-"))
            {
                return this.FindResourceKey(s.Substring(4));
            }

            if (targetType == typeof(Color) && result == null)
            {
                return Colors.Transparent;
            }

            return result;

        }

        private object FindResourceKey( string key )
        {
            if (this.ResourceContainer != null)
            {
                if (this.ResourceContainer.Resources.Contains(key))
                {
                    return this.ResourceContainer.Resources[key];
                }
            }

            if (Application.Current.Resources.Contains(key)) // cannot find, try application resources
            {
                return Application.Current.Resources[key];
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}