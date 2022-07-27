using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Threading.Tasks;
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

        private string _printJsonTemplateStatus;

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
            //IsShowPrintJSonTemplateDialog = true;
            this.TestJsonTemplateCommand = new TestJsonPrintTemplateCommand(async (e) => await InvokePrintJsonTemplate(e));
            this.ShowTestJsonTemplateDialogCommand = new ShowTestJsonTemplateDialogCommandClass(async () => await InvokeShowPrintJsonTemplateDialog());
        }

        private async Task InvokeShowPrintJsonTemplateDialog()
        {
            Debug.WriteLine("Show Dialog");
            IsShowPrintJSonTemplateDialog = true;
        }

        private async Task InvokePrintJsonTemplate(object? param)
        {
            if (param == null) return;
            string jsonString = (string)param;
            if (jsonString == "") return;
            Debug.WriteLine("Print PDF");

            try
            {
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueueCollection printQueues = printServer.GetPrintQueues();
                PrintDialog dialog = new PrintDialog();
                dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "Microsoft Print to PDF");



                //deserialize print template layout
                string cleanJsonString = jsonString.Replace(Environment.NewLine, " ");
                JObject json = JObject.Parse(cleanJsonString);
                JToken? printMeta = json["printMeta"];
                if (printMeta == null) throw new CustomException("invalid print meta");

                //check if print template layout is not valid
                JToken? printTemplateLayoutObject = printMeta["printTemplateLayout"];
                if (printTemplateLayoutObject == null || printTemplateLayoutObject.First == null) throw new CustomException("invalid print template layout");


                PrintTemplateLayoutModel printTemplateLayoutModel = PrintTemplateLayoutModel.FromJson(printTemplateLayoutObject.ToString());
                if (printTemplateLayoutModel == null) throw new CustomException("Print Template Layout must be valid");

                PrintTemplate printTemplate = new PrintTemplate(printTemplateLayoutModel);
                dialog.PrintVisual(printTemplate, "Test");

                this.IsShowPrintJSonTemplateDialog = false;
            }
            catch(CustomException ex)
            {
                PrintJsonTemplateStatus = $"Print Json Failed : {ex.Message}";
            }
            catch (Exception ex)
            {
                PrintJsonTemplateStatus = $"Print Json Failed : {ex.Message}";
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
