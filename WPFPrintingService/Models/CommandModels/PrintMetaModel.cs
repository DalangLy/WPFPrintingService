using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class PrintMetaModel
    {
        [JsonProperty("printMeta", Required = Required.Always)]
        public PrintMeta PrintMeta { get; set; } = new PrintMeta();
    }

    public partial class PrintMeta
    {
        [JsonProperty("printMethod", Required = Required.Always)]
        public string PrintMethod { get; set; } = string.Empty;

        [JsonProperty("printerName", Required = Required.Always)]
        public string PrinterName { get; set; } = string.Empty;
    }

    public partial class PrintMetaModel
    {
        public static PrintMetaModel? FromJson(string json) => JsonConvert.DeserializeObject<PrintMetaModel>(json, ModelJsonConverter.Settings);
    }

    public static class PrintMetaSerialize
    {
        public static string ToJson(this PrintMetaModel self) => JsonConvert.SerializeObject(self, ModelJsonConverter.Settings);
    }
}
