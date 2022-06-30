using Newtonsoft.Json;

namespace WPFPrintingService
{
    internal partial class RequestPrint
    {
        [JsonProperty("code")]
        public string? Code { get; set; }
        [JsonProperty("data")]
        public object? Data { get; set; }
    }

    internal partial class RequestPrint
    {
        public static RequestPrint FromJson(string json) => JsonConvert.DeserializeObject<RequestPrint>(json, Converter.Settings);
    }

    internal partial class RequestPrintData
    {
        [JsonProperty("printerName")]
        public string? PrinterName { get; set; }
        [JsonProperty("printMethod")]
        public string? PrintMethod { get; set; }
        [JsonProperty("templateName")]
        public string? TemplateName { get; set; }
    }

    internal partial class RequestPrintData
    {
        public static RequestPrintData FromJson(string json) => JsonConvert.DeserializeObject<RequestPrintData>(json, Converter.Settings);
    }
}
