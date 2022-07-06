using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class RequestPrintMeta
    {
        [JsonProperty("printMeta")]
        public PrintMeta PrintMeta { get; set; } = new PrintMeta();
    }

    public partial class PrintMeta
    {
        [JsonProperty("printMethod")]
        public string PrintMethod { get; set; } = string.Empty;

        [JsonProperty("printerName")]
        public string PrinterName { get; set; } = string.Empty;
    }

    public partial class RequestPrintMeta
    {
        public static RequestPrintMeta FromJson(string json) => JsonConvert.DeserializeObject<RequestPrintMeta>(json, Converter.Settings);
    }
}
