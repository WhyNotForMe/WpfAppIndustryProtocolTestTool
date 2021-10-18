using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace WpfAppIndustryProtocolTestTool.Model
{
    public class SerialPortCfgModel : ObservableObject
    {
        public string ChipContent { get; set; }
        public string ChipIcon { get; set; }

        private string _selectedValue;
        public string SelectedValue
        {
            get { return _selectedValue; }
            set { Set(ref _selectedValue, value); }
        }

        public List<string> ContentList { get; set; }
    }
}
