using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfAppIndustryProtocolTestTool.BLL
{
    internal class GlobalViewManager
    {

        static Hashtable _viewManager = new Hashtable();

        public static object GetView(string ViewName)
        {
            try
            {
                if (!_viewManager.ContainsKey(ViewName))
                {
                    Type type = Type.GetType("WpfAppIndustryProtocolTestTool.View." + ViewName + "View");
                    UserControl module = Activator.CreateInstance(type) as UserControl;

                    _viewManager.Add(ViewName, module);

                    return module;
                }
                else
                {
                    return _viewManager[ViewName];
                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        public static void RemoveView(string ViewName)
        {
            try
            {
                if (_viewManager.ContainsKey(ViewName))
                {
                    _viewManager.Remove(ViewName);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
