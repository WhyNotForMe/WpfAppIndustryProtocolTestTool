using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIndustryProtocolTestTool.Model.Enum
{
    public enum ModbusSlaveDataTypeEnum
    {
        Bool_TrueFalse,

        Signed_16bits,

        Signed_32bits,
        Float_32bits,

        Signed_64bits,
        Double_64bits
    }
}
