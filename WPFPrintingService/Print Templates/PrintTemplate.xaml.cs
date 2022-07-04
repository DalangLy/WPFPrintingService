using System.Collections.Generic;
using System.Windows.Controls;

namespace WPFPrintingService
{
    public partial class PrintTemplate : UserControl
    {
        public List<PrintTemplateModel> PrintTemplateModels { get; set; } = new List<PrintTemplateModel>();

        public PrintTemplate()
        {
            InitializeComponent();
            DataContext = this;

            //create summary shift
            PrintTemplateModel summaryShift = new PrintTemplateModel();
            summaryShift.Header = new Header()
            {
                Text = "Summary Shift",
                Background = "black",
                Foreground = "white",
                Align = "center",
            };

            //build summary shift body
            summaryShift.Body = new Body()
            {
                BodyRows = new List<BodyRow>()
                {
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Batch ID"
                            },
                            new BodyColumn()
                            {
                                Text = "CH-1495, CH1496",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Outlet Name"
                            },
                            new BodyColumn()
                            {
                                Text = "FPTP",
                                Align = "Right"
                            }
                        }
                    }
                }
            };

            //create handle by
            PrintTemplateModel handleBy = new PrintTemplateModel();
            handleBy.Header = new Header()
            {
                Text = "Handle By",
                Foreground = "white",
                Background = "black",
                Align = "center",
            };

            //create admin, admin
            PrintTemplateModel adminadmin = new PrintTemplateModel();
            adminadmin.Header = new Header()
            {
                Text = "Admin, Admin",
                Foreground = "black",
                Background = "gray",
                Align = "center",
            };
            //build admin admin body
            adminadmin.Body = new Body()
            {
                BodyRows = new List<BodyRow>()
                {
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Phone Number"
                            },
                            new BodyColumn()
                            {
                                Text = "",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Email"
                            },
                            new BodyColumn()
                            {
                                Text = "",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Print Date"
                            },
                            new BodyColumn()
                            {
                                Text = "4/21/2022 11:00:11 AM",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Batch Period"
                            },
                            new BodyColumn()
                            {
                                Text = "4/18/2022 4:52:19 PM",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = ""
                            },
                            new BodyColumn()
                            {
                                Text = "4/21/2022 10:59:56 AM",
                                Align = "Right"
                            }
                        }
                    }
                }
            };


            //create cash collection status
            PrintTemplateModel cashCollectionStatus = new PrintTemplateModel();
            cashCollectionStatus.Header = new Header()
            {
                Text = "Cash Collection Status",
                Foreground = "black",
                Background = "gray",
                Align = "center",
            };
            //build cash collection status body
            cashCollectionStatus.Body = new Body()
            {
                BodyRows = new List<BodyRow>()
                {
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Total System Cash Collection"
                            },
                            new BodyColumn()
                            {
                                Text = "$ 59.53",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Total Actual Cash Collection"
                            },
                            new BodyColumn()
                            {
                                Text = "$ 0.00",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Cash Collection Variance[Short]"
                            },
                            new BodyColumn()
                            {
                                Text = "-$ 59.53",
                                Align = "Right"
                            }
                        }
                    },
                }
            };


            //create Payment Collection Summary
            PrintTemplateModel paymentCollectionSummary = new PrintTemplateModel();
            paymentCollectionSummary.Header = new Header()
            {
                Text = "Payment Collection Summary",
                Foreground = "black",
                Background = "gray",
                Align = "center",
            };
            //build Payment collection summary body
            paymentCollectionSummary.Body = new Body()
            {
                BodyRows = new List<BodyRow>()
                {
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Cash"
                            },
                            new BodyColumn()
                            {
                                Text = "USD",
                            },
                            new BodyColumn()
                            {
                                Text = "2",
                            },
                            new BodyColumn()
                            {
                                Text = "$ 4.65",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "E-Payment"
                            },
                            new BodyColumn()
                            {
                                Text = "Voucher"
                            },
                            new BodyColumn()
                            {
                                Text = "1"
                            },
                            new BodyColumn()
                            {
                                Text = "$ 2.39",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = ""
                            },
                            new BodyColumn()
                            {
                                Text = "Total Payment",
                                Bold = true,
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "$ 7.04",
                                Align = "Right"
                            }
                        }
                    },
                }
            };


            //create Sale Transaction Summary
            PrintTemplateModel saleTransactionSummary = new PrintTemplateModel();
            saleTransactionSummary.Header = new Header()
            {
                Text = "Sale Transaction Summary",
                Foreground = "black",
                Background = "gray",
                Align = "center",
            };
            //build sale transaction summary body
            saleTransactionSummary.Body = new Body()
            {
                BodyRows = new List<BodyRow>()
                {
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Total Sale Before VAT"
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "2",
                            },
                            new BodyColumn()
                            {
                                Text = "$ 6.40",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Total Discount"
                            },
                            new BodyColumn()
                            {
                                Text = ""
                            },
                            new BodyColumn()
                            {
                                Text = ""
                            },
                            new BodyColumn()
                            {
                                Text = "$ 0.00",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Total Sale After Discount"
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "2",
                            },
                            new BodyColumn()
                            {
                                Text = "$ 6.40",
                                Align = "Right"
                            }
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Total VAT"
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "2",
                            },
                            new BodyColumn()
                            {
                                Text = "$ 0.64",
                                Align = "Right"
                            },
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Total Sale After VAT"
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "$ 7.04",
                                Align = "Right"
                            },
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Repayment"
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "$ 0.00",
                                Align = "Right"
                            },
                        }
                    },
                    new BodyRow()
                    {
                        BodyColumns = new List<BodyColumn>()
                        {
                            new BodyColumn()
                            {
                                Text = "Total Sale Transaction Summary Total Sale Transaction Summary Total Sale Transaction Summary Total Sale Transaction Summary",
                                Bold = true,
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "",
                            },
                            new BodyColumn()
                            {
                                Text = "$ 7.04",
                                Align = "Right"
                            },
                        }
                    },
                }
            };


            PrintTemplateModels.Add(summaryShift);
            PrintTemplateModels.Add(handleBy);
            PrintTemplateModels.Add(adminadmin);
            PrintTemplateModels.Add(cashCollectionStatus);
            PrintTemplateModels.Add(paymentCollectionSummary);
            PrintTemplateModels.Add(saleTransactionSummary);
        }
    }
}
