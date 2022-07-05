using Newtonsoft.Json;
using System.Collections.Generic;

namespace WPFPrintingService
{
    public partial class PrintTemplateModel
    {
        [JsonProperty("header")]
        public Header Header { get; set; } = new Header();

        [JsonProperty("body")]
        public Body? Body { get; set; }
    }

    public partial class Body
    {
        [JsonProperty("bodyRows")]
        public List<BodyRow> BodyRows { get; set; } = new List<BodyRow>();
    }

    public partial class BodyRow
    {
        [JsonProperty("bodyColumns")]
        public List<BodyColumn> BodyColumns { get; set; } = new List<BodyColumn>();
    }

    public partial class Header
    {
        [JsonProperty("background")]
        public string Background { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("foreground")]
        public string Foreground { get; set; } = string.Empty;

        [JsonProperty("align")]
        public string Align { get; set; } = string.Empty;

        [JsonProperty("bold")]
        public bool Bold { get; set; } = true;
    }

    public partial class BodyColumn
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("align")]
        public string Align { get; set; } = string.Empty;

        [JsonProperty("bold")]
        public bool Bold { get; set; } = false;
    }

    public partial class PrintTemplateModel
    {
        public static List<PrintTemplateModel> FromJson(string json) => JsonConvert.DeserializeObject<List<PrintTemplateModel>>(json, Converter.Settings);
    }
}
