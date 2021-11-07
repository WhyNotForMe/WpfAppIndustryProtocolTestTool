using GalaSoft.MvvmLight;

namespace WpfAppIndustryProtocolTestTool.Model
{
    public class ModbusRegisterModel : ObservableObject
    {
        private ushort _address;
        public ushort Address
        {
            get { return _address; }
            set { Set(ref _address, value); }
        }


        private byte _registerSize;
        public byte RegisterSize
        {
            get { return _registerSize; }
            set { Set(ref _registerSize, value); }
        }


        private string? _addressRange;
        public string? AddressRange
        {
            get { return _addressRange; }
            set { Set(ref _addressRange, value); }
        }



        private string? _value;
        public string? Value
        {
            get { return _value; }
            set { Set(ref _value, value); }

        }
    }
}
