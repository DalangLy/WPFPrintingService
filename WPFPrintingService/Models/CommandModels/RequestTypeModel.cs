using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class RequestTypeModel
    {
        [JsonProperty("requestType", Required = Required.Always)]
        public string RequestType { get; set; } = string.Empty;
    }

    public partial class RequestTypeModel
    {
        public static RequestTypeModel? FromJson(string json) => JsonConvert.DeserializeObject<RequestTypeModel>(json, ModelJsonConverter.Settings);
    }

    public static class RequestTypeModelSerialize
    {
        public static string ToJson(this RequestTypeModel self) => JsonConvert.SerializeObject(self, ModelJsonConverter.Settings);
    }
}
