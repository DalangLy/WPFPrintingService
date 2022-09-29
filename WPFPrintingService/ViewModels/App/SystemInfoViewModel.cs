using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPrintingService
{
    internal class SystemInfoViewModel : BaseViewModel
    {
		private string _ip = "";

        public string IP
        {
            get { return _ip; }
            set
            {
                _ip = value;
                OnPropertyChanged();
            }
        }

        private string _port;

        public string Port
        {
            get { return _port; }
            set { 
                _port = value;
                OnPropertyChanged();
            }
        }


        public static SystemInfoViewModel Instance => new SystemInfoViewModel();

		public SystemInfoViewModel()
		{
			this.IP = "TCP Host IP : ws://"+AppSingleton.GetInstance.SystemIP;
            this.Port = "Server PORT : " + AppSingleton.GetInstance.Port;
		}

	}
}
