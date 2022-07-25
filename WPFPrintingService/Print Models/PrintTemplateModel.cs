using Newtonsoft.Json;
using System.Collections.Generic;

namespace WPFPrintingService
{
    public partial class PrintTemplateModel
    {
        [JsonProperty("printTemplateLayout")]
        public PrintTemplateLayout PrintTemplateLayout { get; set; } = new PrintTemplateLayout();
    }

    public partial class PrintTemplateLayout
    {
        [JsonProperty("paddingTop")]
        public long PaddingTop { get; set; } = 0;

        [JsonProperty("paddingRight")]
        public long PaddingRight { get; set; } = 0;

        [JsonProperty("paddingBottom")]
        public long PaddingBottom { get; set; } = 0;

        [JsonProperty("paddingLeft")]
        public long PaddingLeft { get; set; } = 0;

        [JsonProperty("rowGap")]
        public long RowGap { get; set; } = 10;

        [JsonProperty("paperWidth")]
        public long PaperWidth { get; set; } = 500;

        [JsonProperty("paperBackground")]
        public string PaperBackground { get; set; } = "transparent";

        [JsonProperty("fontSize")]
        public long FontSize { get; set; } = 12;

        [JsonProperty("fontFamily")]
        public string FontFamily { get; set; } = "arial";

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
        public long RowMarginTop { get; set; } = 0;

        [JsonProperty("rowMarginRight")]
        public long RowMarginRight { get; set; } = 0;

        [JsonProperty("rowMarginBottom")]
        public long RowMarginBottom { get; set; } = 0;

        [JsonProperty("rowMarginLeft")]
        public long RowMarginLeft { get; set; } = 0;

        [JsonProperty("rowPaddingTop")]
        public long RowPaddingTop { get; set; } = 0;

        [JsonProperty("rowPaddingRight")]
        public long RowPaddingRight { get; set; } = 0;

        [JsonProperty("rowPaddingBottom")]
        public long RowPaddingBottom { get; set; } = 0;

        [JsonProperty("rowPaddingLeft")]
        public long RowPaddingLeft { get; set; } = 0;

        [JsonProperty("rowBorderTop")]
        public long RowBorderTop { get; set; } = 0;

        [JsonProperty("rowBorderRight")]
        public long RowBorderRight { get; set; } = 0;

        [JsonProperty("rowBorderBottom")]
        public long RowBorderBottom { get; set; } = 0;

        [JsonProperty("rowBorderLeft")]
        public long RowBorderLeft { get; set; } = 0;

        [JsonProperty("rowBackground")]
        public string RowBackground { get; set; } = "transparent";

        [JsonProperty("rowHeight")]
        public long RowHeight { get; set; } = 0;

        [JsonProperty("columnVerticalAlign")]
        public string ColumnVerticalAlign { get; set; } = "stretch";

        [JsonProperty("columnHorizontalAlign")]
        public string ColumnHorizontalAlign { get; set; } = "stretch";

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
        public string ContentType { get; set; } = "text";

        [JsonProperty("qrCodeLogo")]
        public string QrCodeLogo { get; set; } = string.Empty;

        [JsonProperty("bold")]
        public bool Bold { get; set; }

        [JsonProperty("foreground")]
        public string Foreground { get; set; } = "black";

        [JsonProperty("fontSize")]
        public long FontSize { get; set; } = 0;

        [JsonProperty("fontFamily")]
        public string FontFamily { get; set; } = string.Empty;

        [JsonProperty("contentWidth")]
        public long ContentWidth { get; set; } = 0;

        [JsonProperty("contentHeight")]
        public long ContentHeight { get; set; } = 0;

        [JsonProperty("contentHorizontalAlign")]
        public string ContentHorizontalAlign { get; set; } = "stretch";

        [JsonProperty("contentVerticalAlign")]
        public string ContentVerticalAlign { get; set; } = "stretch";

        [JsonProperty("columnBackground")]
        public string ColumnBackground { get; set; } = "transparent";

        [JsonProperty("columnHorizontalAlign")]
        public string ColumnHorizontalAlign { get; set; } = "stretch";

        [JsonProperty("columnVerticalAlign")]
        public string ColumnVerticalAlign { get; set; } = "stretch";

        [JsonProperty("columnMarginTop")]
        public long ColumnMarginTop { get; set; } = 0;

        [JsonProperty("columnMarginRight")]
        public long ColumnMarginRight { get; set; } = 0;

        [JsonProperty("columnMarginBottom")]
        public long ColumnMarginBottom { get; set; } = 0;

        [JsonProperty("columnMarginLeft")]
        public long ColumnMarginLeft { get; set; } = 0;

        [JsonProperty("columnWidth")]
        public long ColumnWidth { get; set; } = 0;

        [JsonProperty("columnHeight")]
        public long ColumnHeight { get; set; } = 0;

        [JsonProperty("columnPaddingTop")]
        public long ColumnPaddingTop { get; set; } = 0;

        [JsonProperty("columnPaddingRight")]
        public long ColumnPaddingRight { get; set; } = 0;

        [JsonProperty("columnPaddingBottom")]
        public long ColumnPaddingBottom { get; set; } = 0;

        [JsonProperty("columnPaddingLeft")]
        public long ColumnPaddingLeft { get; set; } = 0;

        [JsonProperty("columnBorderTop")]
        public long ColumnBorderTop { get; set; } = 0;

        [JsonProperty("columnBorderRight")]
        public long ColumnBorderRight { get; set; } = 0;

        [JsonProperty("columnBorderBottom")]
        public long ColumnBorderBottom { get; set; } = 0;

        [JsonProperty("columnBorderLeft")]
        public long ColumnBorderLeft { get; set; } = 0;

        [JsonProperty("rowSpan")]
        public int RowSpan { get; set; } = 1;

        [JsonProperty("colSpan")]
        public int ColSpan { get; set; } = 1;
    }

    public partial class PrintTemplateModel
    {
        public static PrintTemplateModel FromJson(string json) => JsonConvert.DeserializeObject<PrintTemplateModel>(json, Converter.Settings);
    }
}
