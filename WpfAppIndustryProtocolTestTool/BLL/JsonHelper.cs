using Newtonsoft.Json;
using System;
using WpfAppIndustryProtocolTestTool.Model.SerializedMessage;

namespace WpfAppIndustryProtocolTestTool.BLL
{
    public class JsonHelper
    {
        public static string SerializeMessage(string name, byte[] buffer, SerializedMsgTypeEnum MessageType = SerializedMsgTypeEnum.Text, SerializedMsgFunctionEnum MessageFunction = SerializedMsgFunctionEnum.ActualData)
        {
            try
            {
                SerializedMessageModel sMessage = new SerializedMessageModel
                {
                    Name = name,
                    MessageType = MessageType,
                    MessageFunction = MessageFunction,
                    Buffer = buffer
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
