using IronBarCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFPrintingService
{
    internal class PrintTemplateAP
    {
        public static PrintTemplateLayoutModel GetAutoGrid(DependencyObject obj)
        {
            return (PrintTemplateLayoutModel)obj.GetValue(AutoGridProperty);
        }

        public static void SetAutoGrid(DependencyObject obj, PrintTemplateLayoutModel value)
        {
            obj.SetValue(AutoGridProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoGrid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoGridProperty =
            DependencyProperty.RegisterAttached("AutoGrid", typeof(PrintTemplateLayoutModel), typeof(PrintTemplateAP), new UIPropertyMetadata(null, OnPropertyChange, OnPropertyUpdate));

        private static object OnPropertyUpdate(DependencyObject d, object baseValue)
        {
            if (!(d is StackPanel stackPanel)) return baseValue;

            if (baseValue == null) return baseValue;

            PrintTemplateLayoutModel printTemplate = (PrintTemplateLayoutModel)baseValue;

            long paperWidth = printTemplate.PrintTemplateLayout.PaperWidth;
            if (paperWidth > 0)
            {
                stackPanel.Width = paperWidth;
            }
            else
            {
                stackPanel.Width = 500;
            }
            string paperBackground = printTemplate.PrintTemplateLayout.PaperBackground;
            if (paperBackground == "")
            {
                stackPanel.Background = Brushes.White;
            }
            else
            {
                stackPanel.Background = _getColorByCode(paperBackground);
            }


            List<RowElement> rows = printTemplate.PrintTemplateLayout.Rows;
            int numberOfNestedRowsToCreate = 0;
            Grid tempRowGrid = new Grid();
            int rowIndex = 0;


            List<NextNestedRowColumnIndex> c = new List<NextNestedRowColumnIndex>();
            List<int> nextColIndex = new List<int>();
            for (int currentRowIndex = 1; currentRowIndex <= rows.Count; currentRowIndex++)
            {
                //done
                if (numberOfNestedRowsToCreate <= 0)
                {
                    int nextColumnIndex = 0;
                    rowIndex = 0;
                    //create new grid as row
                    Border rowBorder = new Border();
                    rowBorder.BorderBrush = Brushes.Black;
                    long rowBorderTop = rows[currentRowIndex - 1].Row.RowBorderTop;
                    long rowBorderBottom = rows[currentRowIndex - 1].Row.RowBorderBottom;
                    long rowBorderLeft = rows[currentRowIndex - 1].Row.RowBorderLeft;
                    long rowBorderRight = rows[currentRowIndex - 1].Row.RowBorderRight;
                    string rowBackground = rows[currentRowIndex - 1].Row.RowBackground;
                    if (rowBackground == "")
                    {
                        rowBackground = "transparent";
                    }
                    rowBorder.Background = _getColorByCode(rowBackground);
                    rowBorder.BorderThickness = new Thickness(rowBorderTop, rowBorderRight, rowBorderBottom, rowBorderLeft);
                    Grid rowGrid = new Grid();
                    rowBorder.Child = rowGrid;
                    long rowHeight = rows[currentRowIndex - 1].Row.RowHeight;
                    if (rowHeight > 0)
                    {
                        rowBorder.Height = rowHeight;
                    }

                    //create column for each row
                    List<ColumnElement> columns = rows[currentRowIndex - 1].Row.Columns;

                    for (int currentColumnIndex = 1; currentColumnIndex <= columns.Count; currentColumnIndex++)
                    {

                        int rowSpanCount = columns[currentColumnIndex - 1].Column.RowSpan;
                        if (rowSpanCount < 2)
                        {
                            //reset to 1
                            rowSpanCount = 0;
                        }

                        int expectedRowDefintionToCreate = rowSpanCount + (currentRowIndex - 1);
                        if (expectedRowDefintionToCreate > numberOfNestedRowsToCreate)
                        {
                            numberOfNestedRowsToCreate += (expectedRowDefintionToCreate - numberOfNestedRowsToCreate);
                        }

                        //check colspan
                        int colSpanCount = columns[currentColumnIndex - 1].Column.ColSpan;
                        if (colSpanCount < 2)
                        {
                            //reset to 1
                            colSpanCount = 1;
                        }
                        long columnWidth = columns[currentColumnIndex - 1].Column.ColumnWidth / colSpanCount;
                        //create column defintions
                        for (int columnDefintionIndex = 0; columnDefintionIndex < colSpanCount; columnDefintionIndex++)
                        {
                            ColumnDefinition columnDefinition = new ColumnDefinition();

                            if (columnWidth > 0)
                            {
                                columnDefinition.Width = new GridLength(columnWidth, GridUnitType.Pixel);
                            }
                            else
                            {
                                columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                            }
                            rowGrid.ColumnDefinitions.Add(columnDefinition);
                        }

                        //create content
                        //Border contentBorder = new Border();
                        //contentBorder.BorderThickness = new Thickness(2);
                        //contentBorder.BorderBrush = Brushes.Black;
                        //TextBlock textBlock = new TextBlock();
                        //textBlock.Text = columns[currentColumnIndex - 1].Column.Content;
                        //textBlock.VerticalAlignment = VerticalAlignment.Center;
                        //textBlock.TextAlignment = TextAlignment.Center;
                        //contentBorder.Child = textBlock;
                        Border contentBorder = _buildColumnContent(columns[currentColumnIndex - 1].Column, printTemplate);
                        rowGrid.Children.Add(contentBorder);
                        Grid.SetColumn(contentBorder, nextColumnIndex);
                        Grid.SetColumnSpan(contentBorder, colSpanCount);
                        Grid.SetRow(contentBorder, 0);
                        Grid.SetRowSpan(contentBorder, rowSpanCount == 0 ? 1 : rowSpanCount);

                        nextColumnIndex += colSpanCount;

                    }

                    //add row to stackpanel
                    tempRowGrid = rowGrid;
                    stackPanel.Children.Add(rowBorder);
                }
                else
                {
                    //add row definition to existed grid row
                    for (int rowDefinitionIndex = 0; rowDefinitionIndex < numberOfNestedRowsToCreate; rowDefinitionIndex++)
                    {
                        RowDefinition rowDefinition = new RowDefinition();
                        tempRowGrid.RowDefinitions.Add(rowDefinition);
                    }
                    numberOfNestedRowsToCreate--;


                    rowIndex++;


                    //check if more column defintions need to create
                    List<ColumnElement> columns = rows[currentRowIndex - 1].Row.Columns;
                    int checkNumberOfColumnToCreateMore = 0;
                    for (int columnDefintionIndex = 0; columnDefintionIndex < columns.Count; columnDefintionIndex++)
                    {
                        int colSpan = columns[columnDefintionIndex].Column.ColSpan;
                        if (colSpan < 2)
                        {
                            colSpan = 1;
                        }
                        checkNumberOfColumnToCreateMore += colSpan;
                    }

                    //create more new column defintion
                    int numberOfColumnToCreateMore = checkNumberOfColumnToCreateMore - tempRowGrid.ColumnDefinitions.Count;
                    if (numberOfColumnToCreateMore >= 1)
                    {
                        ColumnDefinition columnDefinition = new ColumnDefinition();
                        tempRowGrid.ColumnDefinitions.Add(columnDefinition);
                    }









                    //saparate each columns to and remove colspan to get single column
                    List<ColumnElement> previousColumnElements = rows[currentRowIndex - 2].Row.Columns;
                    int remainColSpan = 1;
                    int myIndex = 0;
                    NextNestedRowColumnIndex nextNestedRowColumnIndex = new NextNestedRowColumnIndex();
                    for (int i = 0; i < tempRowGrid.ColumnDefinitions.Count; i++)
                    {
                        int minStartIndex = 0;
                        if (nextColIndex.Count > 0)
                        {
                            minStartIndex = nextColIndex.Min();
                        }


                        if (i >= minStartIndex)
                        {
                            if (remainColSpan > 1)
                            {
                                nextNestedRowColumnIndex.ColumnIndex = i;
                                c.Add(nextNestedRowColumnIndex);
                                remainColSpan--;
                            }
                            else
                            {
                                if (myIndex < previousColumnElements.Count)
                                {
                                    int colSpan = previousColumnElements[myIndex].Column.ColSpan;
                                    if (colSpan > 1)
                                    {
                                        remainColSpan = colSpan;
                                        nextNestedRowColumnIndex = new NextNestedRowColumnIndex()
                                        {
                                            ColumnIndex = i,
                                            RowIndex = currentRowIndex - 2,
                                            RowSpan = previousColumnElements[myIndex].Column.RowSpan,
                                            ColSpan = 1
                                        };
                                        c.Add(nextNestedRowColumnIndex);
                                    }
                                    else
                                    {
                                        c.Add(new NextNestedRowColumnIndex()
                                        {
                                            ColumnIndex = i,
                                            RowIndex = currentRowIndex - 2,
                                            RowSpan = previousColumnElements[myIndex].Column.RowSpan,
                                            ColSpan = 1
                                        });
                                    }
                                    myIndex++;
                                }
                            }
                        }
                        else
                        {
                            c.Add(new NextNestedRowColumnIndex()
                            {
                                ColumnIndex = i,
                                RowIndex = currentRowIndex - 2,
                                RowSpan = 1,
                                ColSpan = 1
                            });
                        }
                    }



















                    //create index for this row by using last row data
                    nextColIndex.Clear();
                    List<NextNestedRowColumnIndex> lastRowColumns = c.Where(e => e.RowIndex == currentRowIndex - 2).ToList();
                    int skipIndex = 0;
                    //here the problem
                    for (int jj = 0; jj < tempRowGrid.ColumnDefinitions.Count; jj++)
                    {
                        if (jj < lastRowColumns.Count)
                        {
                            int rowSpanExisted = (lastRowColumns[jj].RowSpan + lastRowColumns[jj].RowIndex) - rowIndex;

                            if (rowSpanExisted > 0)
                            {
                                skipIndex = (lastRowColumns[jj].ColSpan + jj) - 1;
                                continue;
                            }


                            for (int m = 0; m < lastRowColumns[jj].ColSpan; m++)
                            {
                                skipIndex += (jj + m) - skipIndex;

                                nextColIndex.Add(skipIndex);
                            }


                        }
                    }



















                    //create content
                    for (int currentColumnIndex = 1; currentColumnIndex <= nextColIndex.Count; currentColumnIndex++)
                    {
                        if (currentColumnIndex <= columns.Count)
                        {
                            int colSpan = columns[currentColumnIndex - 1].Column.ColSpan;
                            if (colSpan < 2)
                            {
                                colSpan = 1;
                            }
                            int rowSpan = columns[currentColumnIndex - 1].Column.RowSpan;
                            if (rowSpan < 2)
                            {
                                rowSpan = 0;
                            }

                            //Border contentBorder = new Border();
                            //contentBorder.BorderThickness = new Thickness(2);
                            //contentBorder.BorderBrush = Brushes.Black;
                            //TextBlock textBlock = new TextBlock();
                            //textBlock.Text = columns[currentColumnIndex - 1].Column.Content;
                            //textBlock.VerticalAlignment = VerticalAlignment.Center;
                            //textBlock.TextAlignment = TextAlignment.Center;
                            //contentBorder.Child = textBlock;
                            Border contentBorder = _buildColumnContent(columns[currentColumnIndex - 1].Column, printTemplate);
                            tempRowGrid.Children.Add(contentBorder);
                            Grid.SetColumn(contentBorder, nextColIndex[currentColumnIndex - 1]);
                            Grid.SetColumnSpan(contentBorder, columns[currentColumnIndex - 1].Column.ColSpan);
                            Grid.SetRow(contentBorder, rowIndex);
                            Grid.SetRowSpan(contentBorder, columns[currentColumnIndex - 1].Column.RowSpan);
                        }
                    }
                }
            }



            return baseValue;
        }

        private static Border _buildColumnContent(ColumnColumn column, PrintTemplateLayoutModel printTemplate)
        {
            Border contentBorder = new Border();
            long columnBorderTop = column.ColumnBorderTop;
            long columnBorderRight = column.ColumnBorderRight;
            long columnBorderBottom = column.ColumnBorderBottom;
            long columnBorderLeft = column.ColumnBorderLeft;
            contentBorder.BorderThickness = new Thickness(columnBorderLeft, columnBorderTop, columnBorderRight, columnBorderBottom);
            contentBorder.BorderBrush = Brushes.Black;
            contentBorder.Background = _getColorByCode(column.ColumnBackground);
            long columnHeight = column.ColumnHeight;
            if (columnHeight > 0)
            {
                contentBorder.Height = columnHeight;
            }


            long contentWidth = column.ContentWidth;
            long contentHeight = column.ContentHeight;
            switch (column.ContentType)
            {
                case "image":
                    Image image = _buildImageFromBase64(column.Content);
                    if (contentWidth > 0)
                    {
                        image.Width = contentWidth;
                    }
                    if (contentHeight > 0)
                    {
                        image.Height = contentHeight;
                    }
                    contentBorder.HorizontalAlignment = _getHorizontalAlignContent(column.ContentHorizontalAlign);
                    contentBorder.Child = image;
                    break;
                case "barcode":
                    Image barCodeImage = _buildBarcodeImage(column.Content);
                    if (contentWidth > 0)
                    {
                        barCodeImage.Width = contentWidth;
                    }
                    if (contentHeight > 0)
                    {
                        barCodeImage.Height = contentHeight;
                    }
                    contentBorder.Child = barCodeImage;
                    break;
                case "qrcode":
                    Image qrCodeImage = _buildQRCodeImage(column.Content);
                    if (contentWidth > 0)
                    {
                        qrCodeImage.Width = contentWidth;
                    }
                    if (contentHeight > 0)
                    {
                        qrCodeImage.Height = contentHeight;
                    }
                    contentBorder.Child = qrCodeImage;
                    break;
                default:
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = column.Content;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.TextAlignment = TextAlignment.Center;
                    long masterFontSize = printTemplate.PrintTemplateLayout.FontSize;
                    long fontSize = column.FontSize;
                    if (fontSize <= 0)
                    {
                        fontSize = masterFontSize;
                    }
                    textBlock.FontSize = fontSize;
                    textBlock.FontWeight = column.Bold ? FontWeights.Bold : FontWeights.Regular;
                    textBlock.Foreground = _getColorByCode(column.Foreground);
                    contentBorder.Child = textBlock;
                    break;
            }

            return contentBorder;
        }

        private static void OnPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static HorizontalAlignment _getHorizontalAlignContent(string alignContent)
        {
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Stretch;
            Enum.TryParse(alignContent, true, out horizontalAlignment);

            return horizontalAlignment;
        }

        private static VerticalAlignment _getVerticalAlignContent(string alignContent)
        {
            VerticalAlignment verticalAlignment = VerticalAlignment.Stretch;
            Enum.TryParse(alignContent, true, out verticalAlignment);

            return verticalAlignment;
        }

        private static TextAlignment _getTextAlign(string align)
        {
            TextAlignment textAlignment = TextAlignment.Left;

            Enum.TryParse(align, true, out textAlignment);

            return textAlignment;
        }

        private static Image _buildImageFromBase64(string value)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(value);

                MemoryStream barcodeBinaryMemoryStream = new MemoryStream(bytes);
                BitmapImage barcodeImageSource = new BitmapImage();
                barcodeImageSource.BeginInit();
                barcodeImageSource.StreamSource = barcodeBinaryMemoryStream;
                barcodeImageSource.EndInit();

                // Assign the Source property of your image
                Image barcodeImg = new Image();
                barcodeImg.Source = barcodeImageSource;
                return barcodeImg;
            }
            catch (Exception)
            {
                //default not found image
                byte[] bytes = Convert.FromBase64String("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");

                MemoryStream barcodeBinaryMemoryStream = new MemoryStream(bytes);
                BitmapImage barcodeImageSource = new BitmapImage();
                barcodeImageSource.BeginInit();
                barcodeImageSource.StreamSource = barcodeBinaryMemoryStream;
                barcodeImageSource.EndInit();

                // Assign the Source property of your image
                Image barcodeImg = new Image();
                barcodeImg.Source = barcodeImageSource;
                return barcodeImg;
            }
        }

        private static Brush _getColorByCode(string colorCode)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));
        }

        private static Image _buildQRCodeImage(string value)
        {
            byte[] BinaryData = System.Text.Encoding.UTF8.GetBytes(value);
            //WRITE QR with Binary Content
            GeneratedBarcode MyQRCode = QRCodeWriter.CreateQrCode(BinaryData, 100);
            byte[] qrPngBinaryPngData = MyQRCode.ToPngBinaryData();

            MemoryStream qrcodeBinaryMemoryStream = new MemoryStream(qrPngBinaryPngData);
            BitmapImage qrcodeImageSource = new BitmapImage();
            qrcodeImageSource.BeginInit();
            qrcodeImageSource.StreamSource = qrcodeBinaryMemoryStream;
            qrcodeImageSource.EndInit();

            // Assign the Source property of your image
            Image qrcodeImg = new Image();
            qrcodeImg.Source = qrcodeImageSource;
            return qrcodeImg;
        }

        private static Image _buildBarcodeImage(string value)
        {
            GeneratedBarcode MyBarCode = BarcodeWriter.CreateBarcode(value, BarcodeWriterEncoding.Code128);
            byte[] barcodePngBinaryData = MyBarCode.ToPngBinaryData();

            MemoryStream barcodeBinaryMemoryStream = new MemoryStream(barcodePngBinaryData);
            BitmapImage barcodeImageSource = new BitmapImage();
            barcodeImageSource.BeginInit();
            barcodeImageSource.StreamSource = barcodeBinaryMemoryStream;
            barcodeImageSource.EndInit();

            // Assign the Source property of your image
            Image barcodeImg = new Image();
            barcodeImg.Source = barcodeImageSource;
            return barcodeImg;
        }
    }

    internal class NextNestedRowColumnIndex
    {
        public int ColSpan { get; set; }
        public int RowSpan { get; set; }
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
    }
}
