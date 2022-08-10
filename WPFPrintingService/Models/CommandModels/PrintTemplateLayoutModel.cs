using Newtonsoft.Json;
using System.Collections.Generic;

namespace WPFPrintingService
{
    public partial class PrintTemplateLayoutModel
    {
        [JsonProperty("printTemplateLayout")]
        public PrintTemplateLayout PrintTemplateLayout { get; set; } = new PrintTemplateLayout();
    }

    public partial class PrintTemplateLayout
    {
        public long? PaddingTop { get; set; }

        [JsonProperty("paddingRight")]
        public long? PaddingRight { get; set; }

        [JsonProperty("paddingBottom")]
        public long? PaddingBottom { get; set; }

        [JsonProperty("paddingLeft")]
        public long? PaddingLeft { get; set; }

        [JsonProperty("rowGap")]
        public long? RowGap { get; set; }

        [JsonProperty("paperWidth")]
        public long? PaperWidth { get; set; }

        [JsonProperty("paperBackground")]
        public string PaperBackground { get; set; } = string.Empty;

        [JsonProperty("fontSize")]
        public long? FontSize { get; set; }

        [JsonProperty("fontFamily")]
        public string FontFamily { get; set; } = string.Empty;

        [JsonProperty("foreground")]
        public string Foreground { get; set; } = string.Empty;

        [JsonProperty("rows")]
        public List<RowElement> Rows { get; set; } = new List<RowElement>();
    }

    public partial class RowElement
    {
        [JsonProperty("row")]
        public RowRow Row { get; set; } = new RowRow();
    }

    public partial class RowRow
    {
        [JsonProperty("rowMarginTop")]
        public long? RowMarginTop { get; set; }

        [JsonProperty("rowMarginRight")]
        public long? RowMarginRight { get; set; }

        [JsonProperty("rowMarginBottom")]
        public long? RowMarginBottom { get; set; }

        [JsonProperty("rowMarginLeft")]
        public long? RowMarginLeft { get; set; }

        [JsonProperty("rowPaddingTop")]
        public long? RowPaddingTop { get; set; }

        [JsonProperty("rowPaddingRight")]
        public long? RowPaddingRight { get; set; }

        [JsonProperty("rowPaddingBottom")]
        public long? RowPaddingBottom { get; set; }

        [JsonProperty("rowPaddingLeft")]
        public long? RowPaddingLeft { get; set; }

        [JsonProperty("rowBorderTop")]
        public long? RowBorderTop { get; set; }

        [JsonProperty("rowBorderRight")]
        public long? RowBorderRight { get; set; }

        [JsonProperty("rowBorderBottom")]
        public long? RowBorderBottom { get; set; }

        [JsonProperty("rowBorderLeft")]
        public long? RowBorderLeft { get; set; }

        [JsonProperty("rowBackground")]
        public string RowBackground { get; set; } = string.Empty;

        [JsonProperty("rowHeight")]
        public long? RowHeight { get; set; }

        [JsonProperty("columnVerticalAlign")]
        public string ColumnVerticalAlign { get; set; } = string.Empty;

        [JsonProperty("columnHorizontalAlign")]
        public string ColumnHorizontalAlign { get; set; } = string.Empty;

        [JsonProperty("columns")]
        public List<ColumnElement> Columns { get; set; } = new List<ColumnElement>();
    }

    public partial class ColumnElement
    {
        [JsonProperty("column")]
        public ColumnColumn Column { get; set; } = new ColumnColumn();
    }

    public partial class ColumnColumn
    {
        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;

        [JsonProperty("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonProperty("qrCodeLogo")]
        public string QrCodeLogo { get; set; } = string.Empty;

        [JsonProperty("bold")]
        public bool? Bold { get; set; }

        [JsonProperty("foreground")]
        public string Foreground { get; set; } = string.Empty;

        [JsonProperty("fontSize")]
        public long? FontSize { get; set; }

        [JsonProperty("fontFamily")]
        public string FontFamily { get; set; } = string.Empty;

        [JsonProperty("contentWidth")]
        public long? ContentWidth { get; set; }

        [JsonProperty("contentHeight")]
        public long? ContentHeight { get; set; }

        [JsonProperty("contentHorizontalAlign")]
        public string ContentHorizontalAlign { get; set; } = string.Empty;

        [JsonProperty("contentVerticalAlign")]
        public string ContentVerticalAlign { get; set; } = string.Empty;

        [JsonProperty("columnBackground")]
        public string ColumnBackground { get; set; } = string.Empty;

        [JsonProperty("columnHorizontalAlign")]
        public string ColumnHorizontalAlign { get; set; } = string.Empty;

        [JsonProperty("columnVerticalAlign")]
        public string ColumnVerticalAlign { get; set; } = string.Empty;

        [JsonProperty("columnMarginTop")]
        public long? ColumnMarginTop { get; set; }

        [JsonProperty("columnMarginRight")]
        public long? ColumnMarginRight { get; set; }

        [JsonProperty("columnMarginBottom")]
        public long? ColumnMarginBottom { get; set; }

        [JsonProperty("columnMarginLeft")]
        public long? ColumnMarginLeft { get; set; }

        [JsonProperty("columnWidth")]
        public long? ColumnWidth { get; set; }

        [JsonProperty("columnHeight")]
        public long? ColumnHeight { get; set; }

        [JsonProperty("columnPaddingTop")]
        public long? ColumnPaddingTop { get; set; }

        [JsonProperty("columnPaddingRight")]
        public long? ColumnPaddingRight { get; set; }

        [JsonProperty("columnPaddingBottom")]
        public long? ColumnPaddingBottom { get; set; }

        [JsonProperty("columnPaddingLeft")]
        public long? ColumnPaddingLeft { get; set; }

        [JsonProperty("columnBorderTop")]
        public long? ColumnBorderTop { get; set; }

        [JsonProperty("columnBorderRight")]
        public long? ColumnBorderRight { get; set; }

        [JsonProperty("columnBorderBottom")]
        public long? ColumnBorderBottom { get; set; }

        [JsonProperty("columnBorderLeft")]
        public long? ColumnBorderLeft { get; set; }

        [JsonProperty("colSpan")]
        public int? ColSpan { get; set; }

        [JsonProperty("rowSpan")]
        public int? RowSpan { get; set; }
    }

    public partial class PrintTemplateLayoutModel
    {
        public static PrintTemplateLayoutModel? FromJson(string json) => JsonConvert.DeserializeObject<PrintTemplateLayoutModel>(json, ModelJsonConverter.Settings);
    }

    public static class PrintTemplateLayoutSerialize
    {
        public static string ToJson(this PrintTemplateLayoutModel self) => JsonConvert.SerializeObject(self, ModelJsonConverter.Settings);
    }
}