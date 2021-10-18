using GalaSoft.MvvmLight;
using System.ComponentModel;

namespace WpfAppIndustryProtocolTestTool.Model
{
    public class MainWindowNaviBarModel : ObservableObject
    {
        public string BtnName { get; set; }
        public string PackIconNum { get; set; }
        public string CmdParameter { get; set; }

        private bool _lanVisibility;
        public bool LanVisibility
        {
            get => _lanVisibility;
            set { Set(ref _lanVisibility, value); }

        }



        public bool HorVertical { get; set; }



    }
}
