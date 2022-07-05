using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class RequestCode
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public partial class RequestCode
    {
        public static RequestCode FromJson(string json) => JsonConvert.DeserializeObject<RequestCode>(json, Converter.Settings);
    }
}
