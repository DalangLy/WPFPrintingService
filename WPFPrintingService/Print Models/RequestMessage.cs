using Newtonsoft.Json;

namespace WPFPrintingService
{
    internal partial class RequestMessage
    {
        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    internal partial class RequestMessage
    {
        public static RequestMessage FromJson(string json) => JsonConvert.DeserializeObject<RequestMessage>(json, Converter.Settings);
    }
}
