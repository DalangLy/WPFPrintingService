using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class RequestMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class RequestMessage
    {
        public static RequestMessage FromJson(string json) => JsonConvert.DeserializeObject<RequestMessage>(json, Converter.Settings);
    }
}

