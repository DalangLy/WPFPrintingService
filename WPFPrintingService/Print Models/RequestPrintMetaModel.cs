using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class RequestPrintMetaModel
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

    public partial class RequestPrintMetaModel
    {
        public static RequestPrintMetaModel FromJson(string json) => JsonConvert.DeserializeObject<RequestPrintMetaModel>(json, Converter.Settings);
    }
}
