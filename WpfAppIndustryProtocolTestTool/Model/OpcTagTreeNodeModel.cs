using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace WpfAppIndustryProtocolTestTool.Model
{
    public class OpcTagTreeNodeModel : ObservableObject
    {
        public string? NodeName { get; set; }
        public string? ItemID { get; set; }
        public ObservableCollection<OpcTagTreeNodeModel> Children { get; set; }

        public OpcTagTreeNodeModel()
        {
            Children = new ObservableCollection<OpcTagTreeNodeModel>();
        }
    }
}
