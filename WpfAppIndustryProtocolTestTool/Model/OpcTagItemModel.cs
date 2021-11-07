using GalaSoft.MvvmLight;

namespace WpfAppIndustryProtocolTestTool.Model
{
    public class OpcTagItemModel : ObservableObject
    {
        public bool IsSelected { get; set; }

        public string? ItemID { get; set; }

        public string? DataType { get; set; }

        private dynamic? _itemValue;
        public dynamic? ItemValue
        {
            get { return _itemValue; }
            set { Set(ref _itemValue, value); }
        }



        private string? _timeStamp;
        public string? TimeStamp
        {
            get { return _timeStamp; }
            set { Set(ref _timeStamp, value); }
        }


        private string? _quality;
        public string? Quality
        {
            get { return _quality; }
            set { Set(ref _quality, value); }
        }


        private int _clientHandle;
        public int ClientHandle
        {
            get { return _clientHandle; }
            set { Set(ref _clientHandle, value); }
        }


        private int _transactionID;
        public int TransactionID
        {
            get { return _transactionID; }
            set { Set(ref _transactionID, value); }
        }
    }
}
