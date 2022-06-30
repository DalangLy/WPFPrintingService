using Newtonsoft.Json;

namespace WPFPrintingService
{
    internal partial class RequestCode
    {
        [JsonProperty("code")]
        public string? Code { get; set; }
    }

    internal partial class RequestCode
    {
        public static RequestCode FromJson(string json) => JsonConvert.DeserializeObject<RequestCode>(json, Converter.Settings);
    }
}
