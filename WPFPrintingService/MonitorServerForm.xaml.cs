﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFPrintingService
{
    /// <summary>
    /// Interaction logic for MonitorServerForm.xaml
    /// </summary>
    public partial class MonitorServerForm : UserControl
    {
        private OnPopUpFormClickCallBack _onPopUpFormClickCallBack;
        public MonitorServerForm(OnPopUpFormClickCallBack onPopUpFormClickCallBack)
        {
            InitializeComponent();
            _onPopUpFormClickCallBack = onPopUpFormClickCallBack;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this._onPopUpFormClickCallBack(this);
        }
    }
}
