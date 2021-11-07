using Newtonsoft.Json;
using System;
using WpfAppIndustryProtocolTestTool.Model.SerializedMessage;

namespace WpfAppIndustryProtocolTestTool.BLL
{
    public class JsonHelper
    {
        public static string SerializeMessage(string Name, byte[]? Buffer, SerializedMsgTypeEnum MessageType = SerializedMsgTypeEnum.Text, SerializedMsgFunctionEnum MessageFunction = SerializedMsgFunctionEnum.ActualData)
        {
            try
            {
                SerializedMessageModel sMessage = new SerializedMessageModel
                {
                    Name = Name,
                    MessageType = MessageType,
                    MessageFunction = MessageFunction,
                    Buffer = Buffer
                };

                return JsonConvert.SerializeObject(sMessage);
            }
            catch (Exception)
            {

                throw;
            }


        }

        public static SerializedMessageModel? DeserializeMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    return JsonConvert.DeserializeObject<SerializedMessageModel>(message);

                }
                return null;
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
