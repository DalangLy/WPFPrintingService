using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class RequestMessageModel
    {
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }

    public partial class RequestMessageModel
    {
        public static RequestMessageModel FromJson(string json) => JsonConvert.DeserializeObject<RequestMessageModel>(json, Converter.Settings);
    }
}

