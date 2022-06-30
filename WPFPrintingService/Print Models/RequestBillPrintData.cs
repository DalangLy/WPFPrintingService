using Newtonsoft.Json;

namespace WPFPrintingService
{
    internal partial class RequestBillPrintData
    {
        [JsonProperty("date")]
        public string? Date { get; set; }
    }

    internal partial class RequestBillPrintData
    {
        public static RequestBillPrintData FromJson(string json) => JsonConvert.DeserializeObject<RequestBillPrintData>(json, Converter.Settings);
    }
}
