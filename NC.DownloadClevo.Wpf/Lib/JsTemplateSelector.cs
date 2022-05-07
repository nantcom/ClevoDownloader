using Jint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NC.Lib
{
    public class JsTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Reference value for the script
        /// </summary>
        public object ReferenceValue { get; set; }

        /// <summary>
        /// Resource Container
        /// </summary>
        public FrameworkElement ResourceContainer { get; set; }

        /// <summary>
        /// Script to perform template selection. The script must return
        /// resource name
        /// </summary>
        public string Script { get; set; }

        private JIntExecutor _selectTemplate = new JIntExecutor();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            
            string resourceName = null;
            try
            {
                resourceName = _selectTemplate.Eval(this.Script, typeof(string), item, this.ReferenceValue ) as string;
                Debug.Assert(resourceName != null, "JsTemplateSelector script returned null");
            }
            catch (Exception ex)
            {
#if DEBUG
                Debugger.Break();
#endif
            }

            if (resourceName == null)
            {
                return null;
            }

            var resource = this.FindResourceKey(resourceName) as DataTemplate;

            Debug.Assert(resource != null, "Resource is null from JsTemplateSelector");
            return resource;
        }

        private object FindResourceKey(string key)
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
    }
}
