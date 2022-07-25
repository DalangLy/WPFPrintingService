using Newtonsoft.Json;

namespace WPFPrintingService
{
    public partial class RequestPrintDataModel
    {
        [JsonProperty("printMeta")]
        public PrintMeta PrintMeta { get; set; } = new PrintMeta();
    }

    public partial class PrintMeta
    {
        [JsonProperty("printTemplateLayout")]
        public PrintTemplateModel PrintTemplateModel { get; set; } = new PrintTemplateModel();
    }

    public partial class RequestPrintDataModel
    {
        public static RequestPrintDataModel FromJson(string json) => JsonConvert.DeserializeObject<RequestPrintDataModel>(json, WPFPrintingService.Converter.Settings);
    }
}
