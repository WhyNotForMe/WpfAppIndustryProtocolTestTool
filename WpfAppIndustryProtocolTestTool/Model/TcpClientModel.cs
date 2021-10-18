using GalaSoft.MvvmLight;

namespace WpfAppIndustryProtocolTestTool.Model
{
    public class TcpClientModel : ObservableObject
    {
        public bool IsChecked { get; set; }

        private ushort _code;
        public ushort Code
        {
            get { return _code; }
            set { Set(ref _code, value); }
        }


        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }


        public string EndPoint { get; set; }
    }
}
