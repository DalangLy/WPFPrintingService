using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class TestJSonPrintTemplateViewModel : BaseViewModel
    {
        private static TestJSonPrintTemplateViewModel? _instance;
        public static TestJSonPrintTemplateViewModel Instance
        {
            get { return _instance ?? (_instance = new TestJSonPrintTemplateViewModel()); }
        }

        public ICommand ShowTestJsonTemplateDialogCommand { get; set; }

        public ICommand TestJsonTemplateCommand { get; set; }

        private bool _isShowPrintJSonTemplateDialog;

        public bool IsShowPrintJSonTemplateDialog
        {
            get { return _isShowPrintJSonTemplateDialog; }
            set {
                _isShowPrintJSonTemplateDialog = value;
                OnPropertyChanged();
            }
        }

        private string _printJsonTemplateStatus = string.Empty;

        public string PrintJsonTemplateStatus
        {
            get { return _printJsonTemplateStatus; }
            set { 
                _printJsonTemplateStatus = value;
                OnPropertyChanged();
            }
        }


        public TestJSonPrintTemplateViewModel()
        {
            this.TestJsonTemplateCommand = new TestJsonPrintTemplateCommand((e) => InvokePrintJsonTemplate(e));
            this.ShowTestJsonTemplateDialogCommand = new ShowTestJsonTemplateDialogCommandClass(() => InvokeShowPrintJsonTemplateDialog());
        }

        private void InvokeShowPrintJsonTemplateDialog()
        {
            IsShowPrintJSonTemplateDialog = true;
        }

        private void InvokePrintJsonTemplate(object? param)
        {
            if (param == null) return;
            string jsonString = (string)param;
            if (jsonString == "")
            {
                PrintJsonTemplateStatus = "Print Template Json is Required";
                return;
            };

            try
            {
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueueCollection printQueues = printServer.GetPrintQueues();
                PrintDialog dialog = new PrintDialog();
                dialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
                //dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "Microsoft Print to PDF");
                dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "POS80 Printer");



                //deserialize print template layout
                string cleanJsonString = jsonString.Replace(Environment.NewLine, " ");
                JObject json = JObject.Parse(cleanJsonString);
                JToken? printMeta = json["printMeta"];
                if (printMeta == null) throw new CustomException("invalid print meta");

                //check if print template layout is not valid
                JToken? printTemplateLayoutObject = printMeta["printTemplateLayout"];
                if (printTemplateLayoutObject == null || printTemplateLayoutObject.First == null) throw new CustomException("invalid print template layout");


                PrintTemplateLayoutModel? printTemplateLayoutModel = PrintTemplateLayoutModel.FromJson(printTemplateLayoutObject.ToString());
                if (printTemplateLayoutModel == null) throw new CustomException("Print Template Layout must be valid");

                PrintTemplate printTemplate = new PrintTemplate(printTemplateLayoutModel);
                printTemplate.Measure(new Size(Double.MaxValue, Double.MaxValue));
                Size visualSize = printTemplate.DesiredSize;
                printTemplate.Arrange(new Rect(new Point(0, 0), visualSize));
                printTemplate.UpdateLayout();
                dialog.PrintTicket.PageMediaSize = new PageMediaSize(visualSize.Width, visualSize.Height);
                dialog.PrintVisual(printTemplate, "Test JSon Template");

                this.IsShowPrintJSonTemplateDialog = false;
            }
            catch(CustomException ex)
            {
                PrintJsonTemplateStatus = $"Print Json Failed : {ex.Message}";
            }
            catch (Exception)
            {
                PrintJsonTemplateStatus = $"Invalid JSON Syntax";
            }
        }
    }

    internal class ShowTestJsonTemplateDialogCommandClass : ICommand
    {
        Action _execute;
        public ShowTestJsonTemplateDialogCommandClass(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._execute();
        }
    }

    internal class TestJsonPrintTemplateCommand : ICommand
    {
        private Action<object?> _execute;

        public TestJsonPrintTemplateCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._execute(parameter);
        }
    }
}
