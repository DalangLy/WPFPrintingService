using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class RequestTypeModel
    {
        [JsonProperty("requestType")]
        public string RequestType { get; set; } = "ping";
    }

    public partial class RequestTypeModel
    {
        public static RequestTypeModel FromJson(string json) => JsonConvert.DeserializeObject<RequestTypeModel>(json, Converter.Settings);
    }
}