using System.Windows;
using System.Windows.Controls;
using WpfAppIndustryProtocolTestTool.Model;

namespace WpfAppIndustryProtocolTestTool.BLL.TemplateSelector
{
    public class NaviBarDataTemplateSelector : DataTemplateSelector
    {

        public DataTemplate? VerticalTemplate { get; set; }
        public DataTemplate? HorizontalTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            MainWindowNaviBarModel? naviItem = item as MainWindowNaviBarModel;
            if (naviItem != null)
            {
                if (!naviItem.HorVertical)
                {
                    return VerticalTemplate;
                }
                else
                {
                    return HorizontalTemplate;
                }
            }
            return null;

        }
    }
}
