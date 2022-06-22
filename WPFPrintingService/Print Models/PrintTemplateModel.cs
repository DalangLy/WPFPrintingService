using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WPFPrintingService.Print_Models
{
    public partial class PrintTemplateModel
    {
        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("data")]
        public Data? Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("printerName")]
        public string? PrinterName { get; set; }

        [JsonProperty("printMethod")]
        public string? PrintMethod { get; set; }

        [JsonProperty("templateName")]
        public string? TemplateName { get; set; }

        [JsonProperty("printModel")]
        public PrintModel? PrintModel { get; set; }
    }

    public partial class PrintModel
    {
        [JsonProperty("date")]
        public string? Date { get; set; }

        [JsonProperty("prices")]
        public Price[]? Prices { get; set; }
    }

    public partial class Price
    {
        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }
    }

    public partial class PrintTemplateModel
    {
        public static PrintTemplateModel FromJson(string json) => JsonConvert.DeserializeObject<PrintTemplateModel>(json, WPFPrintingService.Print_Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this PrintTemplateModel self) => JsonConvert.SerializeObject(self, WPFPrintingService.Print_Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}