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

                PrintTemplateLayoutModel printTemplateLayoutModel = PrintTemplateLayoutModel.FromJson(jsonString.Replace(Environment.NewLine, " "));

                PrintTemplate printTemplate = new PrintTemplate(printTemplateLayoutModel);
                dialog.PrintVisual(printTemplate, "Test");

                this.IsShowPrintJSonTemplateDialog = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Print Json Failed : {ex.Message}");
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
