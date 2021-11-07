using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIndustryProtocolTestTool.Model.SerializedMessage
{
    public class SerializedMessageModel
    {
        public string? Name { get; set; }
        public SerializedMsgTypeEnum MessageType { get; set; }
        public SerializedMsgFunctionEnum MessageFunction { get; set; }

        public byte[]? Buffer { get; set; }

    }
}
