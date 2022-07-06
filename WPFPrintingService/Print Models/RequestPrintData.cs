using Newtonsoft.Json;
using System.Collections.Generic;

namespace WPFPrintingService
{
    public partial class RequestPrintData
    {
        [JsonProperty("printMeta")]
        public PrintMeta PrintMeta { get; set; } = new PrintMeta();
    }

    public partial class PrintMeta
    {
        [JsonProperty("printData")]
        public List<PrintDatum> PrintData { get; set; } = new List<PrintDatum>();
    }

    public partial class PrintDatum
    {
        [JsonProperty("Header")]
        public Header Header { get; set; } = new Header();

        [JsonProperty("Body")]
        public Body Body { get; set; } = new Body();
    }

    public partial class Body
    {
        [JsonProperty("BodyRows")]
        public List<BodyRow> BodyRows { get; set; } = new List<BodyRow>();
    }

    public partial class BodyRow
    {
        [JsonProperty("BodyColumns")]
        public List<BodyColumn> BodyColumns { get; set; } = new List<BodyColumn>();
    }

    public partial class BodyColumn
    {
        [JsonProperty("Text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("Align")]
        public string Align { get; set; } = string.Empty;

        [JsonProperty("Bold")]
        public bool Bold { get; set; }
    }

    public partial class Header
    {
        [JsonProperty("Background")]
        public string Background { get; set; } = string.Empty;

        [JsonProperty("Text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("Foreground")]
        public string Foreground { get; set; } = string.Empty;

        [JsonProperty("Align")]
        public string Align { get; set; } = string.Empty;

        [JsonProperty("Bold")]
        public bool Bold { get; set; }
    }

    public partial class RequestPrintData
    {
        public static RequestPrintData FromJson(string json) => JsonConvert.DeserializeObject<RequestPrintData>(json, Converter.Settings);
    }
}