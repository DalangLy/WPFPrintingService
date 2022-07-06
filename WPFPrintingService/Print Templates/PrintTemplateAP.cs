﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFPrintingService
{
    internal class PrintTemplateAP
    {

        public static List<PrintDatum> GetAutoGrid(DependencyObject obj)
        {
            return (List<PrintDatum>)obj.GetValue(AutoGridProperty);
        }

        public static void SetAutoGrid(DependencyObject obj, PrintDatum value)
        {
            obj.SetValue(AutoGridProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoGrid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoGridProperty =
            DependencyProperty.RegisterAttached("AutoGrid", typeof(List<PrintDatum>), typeof(PrintTemplateAP), new UIPropertyMetadata(null, OnPropertyChange, OnPropertyUpdate));

        private static object OnPropertyUpdate(DependencyObject d, object baseValue)
        {
            if (!(d is StackPanel stackPanel)) return baseValue;

            if (baseValue == null) return baseValue;

            List<PrintDatum> models = (List<PrintDatum>)baseValue;

                for (int i = 0; i < models.Count; i++)
                {

                    //create header
                    TextBlock headerText = new TextBlock();
                    headerText.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(models[i].Header.Background));
                    headerText.Text = models[i].Header.Text;
                    headerText.TextAlignment = _getTextAlign(models[i].Header.Align);
                    headerText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(models[i].Header.Foreground));
                    headerText.VerticalAlignment = VerticalAlignment.Center;
                    headerText.FontSize = 22;
                    headerText.Padding = new Thickness(10);
                    stackPanel.Children.Add(headerText);
                    Grid.SetRow(headerText, i);

                    //create body
                    if (models[i].Body == null) continue;
                    for (int j = 0; j < models[i].Body.BodyRows.Count; j++)
                    {
                        //create body row
                        Grid bodyGrid = new Grid();
                        bodyGrid.RowDefinitions.Add(new RowDefinition());

                        //create body column
                        int colspan = 1;
                        TextBlock lastText = new TextBlock();
                        int lastIndex = 0;
                        for (int k = 0; k < models[i].Body.BodyRows[j].BodyColumns.Count; k++)
                        {
                            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition());
                            TextBlock bodyText = new TextBlock();
                            string text = models[i].Body.BodyRows[j].BodyColumns[k].Text;
                            bodyText.Text = text;
                            bodyText.TextAlignment = _getTextAlign(models[i].Body.BodyRows[j].BodyColumns[k].Align);
                            bodyText.Padding = new Thickness(10, 5, 10, 5);
                            bodyText.FontWeight = models[i].Body.BodyRows[j].BodyColumns[k].Bold ? FontWeights.Bold : FontWeights.Regular;
                            bodyGrid.Children.Add(bodyText);

                            if (text == "")
                            {
                                colspan++;
                                bodyGrid.Children.RemoveAt(lastIndex);
                                bodyText.Text = lastText.Text;
                                bodyText.TextAlignment = lastText.TextAlignment;
                                bodyText.Padding = lastText.Padding;
                                bodyText.FontWeight = lastText.FontWeight;
                                Grid.SetColumnSpan(bodyText, colspan);
                            }
                            else
                            {
                                lastText.Text = bodyText.Text;
                                lastText.TextAlignment = bodyText.TextAlignment;
                                lastText.FontWeight = bodyText.FontWeight;
                                lastText.Padding = bodyText.Padding;
                                lastIndex = k;
                                Grid.SetColumn(bodyText, k);
                            }
                            Grid.SetRow(bodyText, j);
                        };
                        stackPanel.Children.Add(bodyGrid);
                    }
                }

            return baseValue;
        }

        private static void OnPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static TextAlignment _getTextAlign(string align)
        {
            TextAlignment textAlignment = TextAlignment.Center;

            Enum.TryParse(align, true, out textAlignment);

            return textAlignment;
        }
    }
}
