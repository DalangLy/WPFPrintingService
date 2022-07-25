using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class MessageModel
    {
        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; set; } = string.Empty;
    }

    public partial class MessageModel
    {
        public static MessageModel? FromJson(string json) => JsonConvert.DeserializeObject<MessageModel>(json, ModelJsonConverter.Settings);
    }

    public static class MessageModelSerialize
    {
        public static string ToJson(this MessageModel self) => JsonConvert.SerializeObject(self, ModelJsonConverter.Settings);
    }
}
