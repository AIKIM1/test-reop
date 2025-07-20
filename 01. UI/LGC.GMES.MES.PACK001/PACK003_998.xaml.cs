/*************************************************************************************
 Created Date : 2022.07.05
      Creator : 이태규
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2022.07.05  이태규 : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_998 : UserControl, IWorkArea
    {
        #region #. Member Variable Lists...
        CommonCombo _combo = new CommonCombo();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_998()
        {
            InitializeComponent();
            PackCommon.SetPopupDraggable(this.popupAlert, this.pnlTitleTransferConfirm); //생성자에 추가
        }
        #endregion

        #region #. Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                //이하 코드 추가
                SetDataToGrind();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void grdMain_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {

        }
        #endregion

        #region #. Member Function Lists...
        public DataTable initTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(string));
            dt.Columns.Add("COL1", typeof(string));
            dt.Columns.Add("COL2", typeof(string));
            dt.Columns.Add("COL3", typeof(string));
            dt.Columns.Add("COL4", typeof(string));
            dt.Columns.Add("COL5", typeof(string));
            dt.Columns.Add("COL6", typeof(string));
            dt.Columns.Add("COL7", typeof(string));
            dt.Columns.Add("COL8", typeof(string));
            dt.Columns.Add("COL9", typeof(string));
            dt.Columns.Add("COL10", typeof(string));
            dt.Columns.Add("COL11", typeof(string));
            dt.Columns.Add("COL12", typeof(string));
            dt.Columns.Add("COL13", typeof(string));
            dt.Columns.Add("COL14", typeof(string));
            dt.Columns.Add("COL15", typeof(string));
            dt.Columns.Add("COL16", typeof(string));
            dt.Columns.Add("COL17", typeof(string));
            dt.Columns.Add("COL18", typeof(string));
            dt.Columns.Add("COL19", typeof(string));
            dt.Columns.Add("COL20", typeof(string));
            dt.Columns.Add("COL21", typeof(string));
            dt.Columns.Add("COL22", typeof(string));
            dt.Columns.Add("COL23", typeof(string));
            dt.Columns.Add("COL24", typeof(string));
            dt.Columns.Add("COL25", typeof(string));
            dt.Columns.Add("COL26", typeof(string));
            dt.Columns.Add("COL27", typeof(string));
            dt.Columns.Add("COL28", typeof(string));
            dt.Columns.Add("COL29", typeof(string));
            dt.Columns.Add("COL30", typeof(string));
            dt.Columns.Add("COL31", typeof(string));
            dt.Columns.Add("COL32", typeof(string));
            dt.Columns.Add("COL33", typeof(string));
            dt.Columns.Add("COL34", typeof(string));
            dt.Columns.Add("COL35", typeof(string));
            dt.Columns.Add("COL36", typeof(string));
            dt.Columns.Add("COL37", typeof(string));
            dt.Columns.Add("COL38", typeof(string));
            dt.Columns.Add("COL39", typeof(string));
            dt.Columns.Add("COL40", typeof(string));
            return dt;
        }

        public void SetDataToGrind()
        {
            DataTable dt = initTable();
            dt.AcceptChanges();
            //row start
            DataRow row = dt.NewRow();
            row["CHK"] = "True";
            row["COL1"] = "폴란드 자동차 Pack2동";
            row["COL2"] = "P8";
            row["COL3"] = "MEB2P";
            row["COL4"] = "MFR";
            row["COL5"] = "MTP02276AC";
            row["COL6"] = "폴란드 Pack2동 모듈8라인";
            row["COL7"] = "W-P8-Q08-S01-A1";
            row["COL8"] = "4";
            row["COL9"] = "4";
            row["COL10"] = "Fault";
            row["COL11"] = "";
            row["COL12"] = "True";
            row["COL13"] = "4";
            row["COL14"] = "20";
            row["COL15"] = "";
            row["COL16"] = "";
            row["COL17"] = "";
            row["COL18"] = "";
            row["COL19"] = "";
            row["COL20"] = "";
            row["COL21"] = "10";
            row["COL22"] = "10";
            row["COL23"] = "10";
            row["COL24"] = "10";
            row["COL25"] = "10";
            row["COL26"] = "20";
            row["COL27"] = "20";
            row["COL28"] = "20";
            row["COL29"] = "20";
            row["COL30"] = "20";
            row["COL31"] = "30";
            row["COL32"] = "30";
            row["COL33"] = "30";
            row["COL34"] = "30";
            row["COL35"] = "30";
            row["COL36"] = "40";
            row["COL37"] = "40";
            row["COL38"] = "40";
            row["COL39"] = "1";
            row["COL40"] = "0";
            dt.Rows.Add(row);
            //row end
            //row2 start
            DataRow row2 = dt.NewRow();
            row2["CHK"] = "True";
            row2["COL1"] = "폴란드 자동차 Pack2동";
            row2["COL2"] = "P8";
            row2["COL3"] = "MEB2P";
            row2["COL4"] = "MFR";
            row2["COL5"] = "MTP02276AC";
            row2["COL6"] = "폴란드 Pack2동 모듈9라인";
            row2["COL7"] = "W-P8-Q09-S03-G1";
            row2["COL8"] = "4";
            row2["COL9"] = "4";
            row2["COL10"] = "Fault";
            row2["COL11"] = "";
            row2["COL12"] = "True";
            row2["COL13"] = "4";
            row2["COL14"] = "20";
            row2["COL15"] = "";
            row2["COL16"] = "";
            row2["COL17"] = "";
            row2["COL18"] = "";
            row2["COL19"] = "";
            row2["COL20"] = "";
            row2["COL21"] = "110";
            row2["COL22"] = "110";
            row2["COL23"] = "110";
            row2["COL24"] = "110";
            row2["COL25"] = "110";
            row2["COL26"] = "120";
            row2["COL27"] = "120";
            row2["COL28"] = "120";
            row2["COL29"] = "120";
            row2["COL30"] = "120";
            row2["COL31"] = "130";
            row2["COL32"] = "130";
            row2["COL33"] = "130";
            row2["COL34"] = "130";
            row2["COL35"] = "130";
            row2["COL36"] = "140";
            row2["COL37"] = "140";
            row2["COL38"] = "140";
            row2["COL39"] = "2";
            row2["COL40"] = "1";
            dt.Rows.Add(row2);
            //row2 end
            //row3 start
            DataRow row3 = dt.NewRow();
            row3["CHK"] = "True";
            row3["COL1"] = "폴란드 자동차 Pack2동";
            row3["COL2"] = "P8";
            row3["COL3"] = "MEB2P";
            row3["COL4"] = "MFR";
            row3["COL5"] = "MTP02276AC";
            row3["COL6"] = "폴란드 Pack2동 모듈10라인";
            row3["COL7"] = "W-P8-Q10-S06-F1";
            row3["COL8"] = "4";
            row3["COL9"] = "1";
            row3["COL10"] = "Fault";
            row3["COL11"] = "";
            row3["COL12"] = "True";
            row3["COL13"] = "4";
            row3["COL14"] = "20";
            row3["COL15"] = "";
            row3["COL16"] = "";
            row3["COL17"] = "";
            row3["COL18"] = "";
            row3["COL19"] = "";
            row3["COL20"] = "";
            row3["COL21"] = "210";
            row3["COL22"] = "210";
            row3["COL23"] = "210";
            row3["COL24"] = "210";
            row3["COL25"] = "210";
            row3["COL26"] = "220";
            row3["COL27"] = "220";
            row3["COL28"] = "220";
            row3["COL29"] = "220";
            row3["COL30"] = "220";
            row3["COL31"] = "230";
            row3["COL32"] = "230";
            row3["COL33"] = "230";
            row3["COL34"] = "230";
            row3["COL35"] = "230";
            row3["COL36"] = "240";
            row3["COL37"] = "240";
            row3["COL38"] = "240";
            row3["COL39"] = "3";
            row3["COL40"] = "2";
            dt.Rows.Add(row3);
            //row3 end
            //row4 start
            DataRow row4 = dt.NewRow();
            row4["CHK"] = "True";
            row4["COL1"] = "폴란드 자동차 Pack2동";
            row4["COL2"] = "P8";
            row4["COL3"] = "MEB2P";
            row4["COL4"] = "MFR";
            row4["COL5"] = "FUEU287EC";
            row4["COL6"] = "폴란드 Pack2동 모듈11라인";
            row4["COL7"] = "W-P8-Q15-S02-B1";
            row4["COL8"] = "4";
            row4["COL9"] = "3";
            row4["COL10"] = "Fault";
            row4["COL11"] = "";
            row4["COL12"] = "True";
            row4["COL13"] = "4";
            row4["COL14"] = "40";
            row4["COL15"] = "";
            row4["COL16"] = "";
            row4["COL17"] = "";
            row4["COL18"] = "";
            row4["COL19"] = "";
            row4["COL20"] = "";
            row4["COL21"] = "310";
            row4["COL22"] = "310";
            row4["COL23"] = "310";
            row4["COL24"] = "310";
            row4["COL25"] = "310";
            row4["COL26"] = "320";
            row4["COL27"] = "320";
            row4["COL28"] = "320";
            row4["COL29"] = "320";
            row4["COL30"] = "320";
            row4["COL31"] = "330";
            row4["COL32"] = "330";
            row4["COL33"] = "330";
            row4["COL34"] = "330";
            row4["COL35"] = "330";
            row4["COL36"] = "340";
            row4["COL37"] = "340";
            row4["COL38"] = "340";
            row4["COL39"] = "4";
            row4["COL40"] = "0";
            dt.Rows.Add(row4);
            //row4 end
            //row5 start
            DataRow row5 = dt.NewRow();
            row5["CHK"] = "True";
            row5["COL1"] = "<span style=\"color:red; font-size:20px; font-weight:bold\">hello</span>";
            row5["COL2"] = "P8";
            row5["COL3"] = "MEB2P";
            row5["COL4"] = "MFR";
            row5["COL5"] = "EABT1234BB";
            row5["COL6"] = "폴란드 Pack2동 모듈15라인";
            row5["COL7"] = "W-P8-Q15-S03-C1";
            row5["COL8"] = "5";
            row5["COL9"] = "1";
            row5["COL10"] = "Fault";
            row5["COL11"] = "";
            row5["COL12"] = "True";
            row5["COL13"] = "5";
            row5["COL14"] = "36";
            row5["COL15"] = "";
            row5["COL16"] = "";
            row5["COL17"] = "";
            row5["COL18"] = "";
            row5["COL19"] = "";
            row5["COL20"] = "";
            row5["COL21"] = "";
            row5["COL22"] = "";
            row5["COL23"] = "";
            row5["COL24"] = "";
            row5["COL25"] = "";
            row5["COL26"] = "";
            row5["COL27"] = "";
            row5["COL28"] = "";
            row5["COL29"] = "";
            row5["COL30"] = "";
            row5["COL31"] = "";
            row5["COL32"] = "";
            row5["COL33"] = "";
            row5["COL34"] = "";
            row5["COL35"] = "";
            row5["COL36"] = "";
            row5["COL37"] = "";
            row5["COL38"] = "";
            row5["COL39"] = "5";
            row5["COL40"] = "1";
            dt.Rows.Add(row5);
            //row5 end
            //row6 start
            DataRow row6 = dt.NewRow();
            row6["CHK"] = "True";
            row6["COL1"] = "폴란드 자동차 Pack2동";
            row6["COL2"] = "P8";
            row6["COL3"] = "MEB2P";
            row6["COL4"] = "MFR";
            row6["COL5"] = "EDBC8342AA";
            row6["COL6"] = "폴란드 Pack2동 모듈15라인";
            row6["COL7"] = "W-P8-Q15-S04-D1";
            row6["COL8"] = "10";
            row6["COL9"] = "6";
            row6["COL10"] = "True";
            row6["COL11"] = "";
            row6["COL12"] = "True";
            row6["COL13"] = "10";
            row6["COL14"] = "6";
            row6["COL15"] = "";
            row6["COL16"] = "";
            row6["COL17"] = "";
            row6["COL18"] = "";
            row6["COL19"] = "";
            row6["COL20"] = "";
            row6["COL21"] = "";
            row6["COL22"] = "";
            row6["COL23"] = "";
            row6["COL24"] = "";
            row6["COL25"] = "";
            row6["COL26"] = "";
            row6["COL27"] = "";
            row6["COL28"] = "";
            row6["COL29"] = "";
            row6["COL30"] = "";
            row6["COL31"] = "";
            row6["COL32"] = "";
            row6["COL33"] = "";
            row6["COL34"] = "";
            row6["COL35"] = "";
            row6["COL36"] = "";
            row6["COL37"] = "";
            row6["COL38"] = "";
            row6["COL39"] = "6";
            row6["COL40"] = "3";
            dt.Rows.Add(row6);
            //row6 end

            #region #. add grid to grid

            Color FColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//ForegroundColor
            Color BColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//BackgroundColor
            Color C = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//하얀색
            Color R = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF92D050");//초록색
            Color W = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF00");//노란색
            Color T = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFC000");//빨간색
            Color U = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD0CECE");//회색
            Color F = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");//검정색
            Color B = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF638EC6");//파란색
            Color BorderColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEFF1F3");//바탕색  

            System.Windows.Style style = new System.Windows.Style(typeof(System.Windows.Controls.Control));
            BColor = R; FColor = F;
            style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(F)));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));

            Label lbl = null;
            lbl = new Label() { Content = new TextBlock() { Text = "1111", TextWrapping = TextWrapping.Wrap, FontSize = 8 }, Width = 100, Height = 20 };
            lbl.Style = style;
            lbl.SetValue(Grid.ColumnProperty, 0);
            lbl.SetValue(Grid.RowProperty, 0);
            lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
            lbl.VerticalContentAlignment = VerticalAlignment.Top;
            ((TextBlock)lbl.Content).VerticalAlignment = VerticalAlignment.Top;
            ((TextBlock)lbl.Content).Padding = new Thickness(0, 0, 0, 0);
            ((TextBlock)lbl.Content).Margin = new Thickness(0, 0, 0, 0);

            DataGridTemplateColumn col = new DataGridTemplateColumn();
            col.Header = "Actions";
            col.Width = 300;
            DataTemplate dt1 = new DataTemplate();
            var sp = new FrameworkElementFactory(typeof(StackPanel));
            sp.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var btn1 = new FrameworkElementFactory(typeof(Button));
            btn1.SetValue(Button.ContentProperty, "Delete");
            btn1.SetValue(FrameworkElement.WidthProperty, 100.0);
            btn1.SetValue(Button.MarginProperty, new Thickness(0, 0, 10, 0));
            sp.AppendChild(btn1);

            var btn2 = new FrameworkElementFactory(typeof(Button));
            btn2.SetValue(Button.ContentProperty, "Edit");
            btn2.SetValue(FrameworkElement.WidthProperty, 100.0);
            sp.AppendChild(btn2);

            dt1.VisualTree = sp;
            col.CellTemplate = dt1;
            //col.CellTemplateSelector = null;
            //ActualControl.CellTemplate = "";


            #endregion

            DataView dv = dt.DefaultView;
            grdMain.BeginEdit();
            grdMain.ItemsSource = DataTableConverter.Convert(dv.ToTable());
            grdMain.Columns[10] = col;
            #region #. add DataGridTemplateColumn of DataGrid in runtime
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XElement xDataTemplate = new XElement(ns + "DataTemplate", new XElement(ns + "Button", new XAttribute("Content", "test")));
            //String[] names = new String[] { "abc", "kim", "peter", "Tistory", "Z" };
            //XElement xDataTemplate = new XElement(ns + "DataTemplate", from name in names select new XElement(ns + "Button", new XAttribute("Content", "test")));
            //int iItemCnt = 1;
            //XElement xDataTemplate = new XElement(ns + "DataTemplate", from item in dt.AsEnumerable() where item.Field<string>("COL39") == Convert.ToString(iItemCnt++) select new XElement(ns + "Button", new XAttribute("Content", item.Field<string>("COL8"))));
            //XElement xDataTemplate = new XElement(ns + "DataTemplate", from item in dt.AsEnumerable() where item.Field<string>("COL39") == Convert.ToString(iItemCnt++) select new XElement(ns + "Button", new XAttribute("Content", item.Field<string>("COL8"))));
            #region #. 실행가능 소스 
            //XElement xDataTemplate = new XElement(ns + "DataTemplate",
            //                            new XElement(ns + "ContentControl",
            //                                new XElement(ns + "ContentControl.Style",
            //                                    new XElement(ns + "Style", new XAttribute("TargetType", "ContentControl"),
            //                                        new XElement(ns + "Style.Triggers",
            //                                            new XElement(ns + "DataTrigger", new XAttribute("Binding", "{Binding COL40}"), new XAttribute("Value", "0"),
            //                                                new XElement(ns + "Setter", new XAttribute("Property", "ContentTemplate"),
            //                                                    new XElement(ns + "Setter.Value",
            //                                                        new XElement(ns + "DataTemplate",
            //                                                            new XElement(ns + "Grid",
            //                                                                new XElement(ns + "Grid.ColumnDefinitions",
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*"))),
            //                                                                new XElement(ns + "TextBlock", new XAttribute("Grid.Column", "0"), new XAttribute("Text", "1")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "1"), new XAttribute("Content", "2")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "2"), new XAttribute("Content", "3"))))))),
            //                                            new XElement(ns + "DataTrigger", new XAttribute("Binding", "{Binding COL40}"), new XAttribute("Value", "1"),
            //                                                new XElement(ns + "Setter", new XAttribute("Property", "ContentTemplate"),
            //                                                    new XElement(ns + "Setter.Value",
            //                                                        new XElement(ns + "DataTemplate",
            //                                                            new XElement(ns + "Grid",
            //                                                                new XElement(ns + "Grid.ColumnDefinitions",
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*"))),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "0"), new XAttribute("Content", "1")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "1"), new XAttribute("Content", "2")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "2"), new XAttribute("Content", "3")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "3"), new XAttribute("Content", "4"))))))),
            //                                            new XElement(ns + "DataTrigger", new XAttribute("Binding", "{Binding COL40}"), new XAttribute("Value", "2"),
            //                                                new XElement(ns + "Setter", new XAttribute("Property", "ContentTemplate"),
            //                                                    new XElement(ns + "Setter.Value",
            //                                                        new XElement(ns + "DataTemplate",
            //                                                            new XElement(ns + "Grid",
            //                                                                new XElement(ns + "Grid.ColumnDefinitions",
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*"))),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "0"), new XAttribute("Content", "1")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "1"), new XAttribute("Content", "2")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "2"), new XAttribute("Content", "3")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "3"), new XAttribute("Content", "4")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "4"), new XAttribute("Content", "5"))))))),
            //                                            new XElement(ns + "DataTrigger", new XAttribute("Binding", "{Binding COL40}"), new XAttribute("Value", "3"),
            //                                                new XElement(ns + "Setter", new XAttribute("Property", "ContentTemplate"),
            //                                                    new XElement(ns + "Setter.Value",
            //                                                        new XElement(ns + "DataTemplate",
            //                                                            new XElement(ns + "Grid",
            //                                                                new XElement(ns + "Grid.ColumnDefinitions",
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*"))),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "0"), new XAttribute("Content", "1")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "1"), new XAttribute("Content", "2")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "2"), new XAttribute("Content", "3")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "3"), new XAttribute("Content", "4")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "4"), new XAttribute("Content", "5")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "5"), new XAttribute("Content", "6")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "6"), new XAttribute("Content", "7")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "7"), new XAttribute("Content", "8")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "8"), new XAttribute("Content", "9")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "9"), new XAttribute("Content", "10")))))))
            //                                                                )))));
            #endregion
            //XElement xDataTemplate = new XElement(ns + "DataTemplate",
            //                            new XElement(ns + "ContentControl",
            //                                new XElement(ns + "ContentControl.Style",
            //                                    new XElement(ns + "Style", new XAttribute("TargetType", "ContentControl"),
            //                                        new XElement(ns + "Style.Triggers",
            //                                            new XElement(ns + "DataTrigger", new XAttribute("Binding", "{Binding COL40}"), new XAttribute("Value", "0"),
            //                                                new XElement(ns + "Setter", new XAttribute("Property", "ContentTemplate"),
            //                                                    new XElement(ns + "Setter.Value",
            //                                                        new XElement(ns + "DataTemplate",
            //                                                            new XElement(ns + "Grid",
            //                                                                new XElement(ns + "Grid.ColumnDefinitions",
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*"))),
            //                                                                new XElement(ns + "TextBlock", new XAttribute("Grid.Column", "0"), new XAttribute("Text", "1")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "1"), new XAttribute("Content", "2")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "2"), new XAttribute("Content", "3"))))))),
            //                                            new XElement(ns + "DataTrigger", new XAttribute("Binding", "{Binding COL40}"), new XAttribute("Value", "1"),
            //                                                new XElement(ns + "Setter", new XAttribute("Property", "ContentTemplate"),
            //                                                    new XElement(ns + "Setter.Value",
            //                                                        new XElement(ns + "DataTemplate",
            //                                                            new XElement(ns + "Grid",
            //                                                                new XElement(ns + "Grid.ColumnDefinitions",
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*"))),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "0"), new XAttribute("Content", "1")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "1"), new XAttribute("Content", "2")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "2"), new XAttribute("Content", "3")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "3"), new XAttribute("Content", "4"))))))),
            //                                            new XElement(ns + "DataTrigger", new XAttribute("Binding", "{Binding COL40}"), new XAttribute("Value", "2"),
            //                                                new XElement(ns + "Setter", new XAttribute("Property", "ContentTemplate"),
            //                                                    new XElement(ns + "Setter.Value",
            //                                                        new XElement(ns + "DataTemplate",
            //                                                            new XElement(ns + "Grid",
            //                                                                new XElement(ns + "Grid.ColumnDefinitions",
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*"))),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "0"), new XAttribute("Content", "1")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "1"), new XAttribute("Content", "2")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "2"), new XAttribute("Content", "3")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "3"), new XAttribute("Content", "4")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "4"), new XAttribute("Content", "5"))))))),
            //                                            new XElement(ns + "DataTrigger", new XAttribute("Binding", "{Binding COL40}"), new XAttribute("Value", "3"),
            //                                                new XElement(ns + "Setter", new XAttribute("Property", "ContentTemplate"),
            //                                                    new XElement(ns + "Setter.Value",
            //                                                        new XElement(ns + "DataTemplate",
            //                                                            new XElement(ns + "Grid",
            //                                                                new XElement(ns + "Grid.ColumnDefinitions",
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
            //                                                                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*"))),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "0"), new XAttribute("Content", "1")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "1"), new XAttribute("Content", "2")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "2"), new XAttribute("Content", "3")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "3"), new XAttribute("Content", "4")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "4"), new XAttribute("Content", "5")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "5"), new XAttribute("Content", "6")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "6"), new XAttribute("Content", "7")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "7"), new XAttribute("Content", "8")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "8"), new XAttribute("Content", "9")),
            //                                                                new XElement(ns + "Button", new XAttribute("Grid.Column", "9"), new XAttribute("Content", "10")))))))
            //                                                                )))));
            StringReader sr = new StringReader(xDataTemplate.ToString());
            XmlReader xr = XmlReader.Create(sr);
            DataTemplate dataTemplateObect = System.Windows.Markup.XamlReader.Load(xr) as DataTemplate;
            ActualControl2.CellTemplate = dataTemplateObect;
            #endregion

            grdMain.CommitEdit();
        }


        #region #. Alert Popup
        private void Lbl_MouseEnter(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            ShowPopup(sender);
        }

        private void Lbl_MouseLeave(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            HidePopUp();
        }

        private void btnHideConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.popupAlert.IsOpen = false;
            this.popupAlert.HorizontalOffset = 0;
            this.popupAlert.VerticalOffset = 0;
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HidePopUp();
        }

        // Popup - Close Popup
        private void HidePopUp()
        {
            this.popupAlert.IsOpen = false;
            this.popupAlert.HorizontalOffset = 0;
            this.popupAlert.VerticalOffset = 0;
        }

        // Popup - Show Popup
        private void ShowPopup(object sender)
        {
            try
            {
                this.popupAlert.Tag = null;
                this.popupAlert.Tag = null;

                Label lbl = (Label)sender;
                string sText = ((TextBlock)lbl.Content).Tag.ToString();
                txtMessageAlert.Text = sText;
                this.popupAlert.PlacementTarget = lbl;
                this.popupAlert.IsOpen = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Content = "C";
        }

        #endregion



        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            iniCombo();

            this.Loaded -= C1Window_Loaded;
        }

        private void iniCombo()
        {
            try
            {

                SetAreaByShopId(cboSnapArea);


                cboSnapArea.SelectedValueChanged += cboSnapArea_SelectedValueChanged;

                cboSnapEqsg.SelectionChanged += cboSnapEqsg_SelectionChanged;
                cboSnapProc.SelectionChanged += cboSnapProc_SelectionChanged;
                //cboSnapModel.SelectionChanged += cboSnapModel_SelectionChanged;
                //cboSnapBizType.SelectionChanged += cboSnapBizType_SelectionChanged;

                //ldpSnapMonthShot.SelectedDataTimeChanged += ldpSnapMonthShot_SelectedDataTimeChanged;



                string[] sFilter = { "PACK_WIPSTAT_WIPSEARCH" };
                string strCase = "COMMCODE";

                _combo.SetCombo(cboSnapWipstat, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: strCase);
                //_combo.SetCombo(cboRsltWipstat, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: strCase);

                //cboSnapArea.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }






        #region [ WPF Event ]

        #region ( 전산재고 Header )

        #region < 동 >
        private void cboSnapArea_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (cboSnapArea.ItemsSource == null) return;

                    C1ComboBox[] cb = { cboSnapArea };
                    //MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    //SetcboEQSG(cboSnapEqsg, msb: msb, cb:cb );
                    //SetcboProcess(cboSnapProc, msb: msb, cb:cb);
                    //SetcboProductModel(cboSnapModel, msb: msb, cb: cb);
                    //SetcboBizType(cboSnapBizType, msb: msb, cb: cb);
                    //SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    //object[] objStockSeqShotParent = { cboSnapArea, ldpSnapMonthShot };
                    //String[] sFilterSnapAll = { "" };
                    //_combo.SetComboObjParent(cboSnapStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterSnapAll);

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 라인 >

        private void cboSnapEqsg_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboSnapEqsg.SelectionChanged -= cboSnapEqsg_SelectionChanged;

                    //C1ComboBox[] cb = { cboSnapArea };
                    //MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    //SetcboProcess(cboSnapProc, msb: msb, cb: cb);
                    //SetcboProductModel(cboSnapModel, msb: msb, cb: cb);
                    //SetcboBizType(cboSnapBizType, msb: msb, cb: cb);
                    //SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    //cboSnapEqsg.SelectionChanged += cboSnapEqsg_SelectionChanged;

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 공정 >
        private void cboSnapProc_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //cboSnapProc.SelectionChanged -= cboSnapProc_SelectionChanged;

                    //C1ComboBox[] cb = { cboSnapArea };
                    //MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    //SetcboBizType(cboSnapBizType, msb: msb, cb: cb);
                    //SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    //cboSnapProc.SelectionChanged += cboSnapProc_SelectionChanged;

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 모델 > 
        private void cboSnapModel_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //cboSnapModel.SelectionChanged -= cboSnapModel_SelectionChanged;

                    C1ComboBox[] cb = { cboSnapArea };
                    //MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    //SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    //cboSnapModel.SelectionChanged += cboSnapModel_SelectionChanged;
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < Biz Type >
        private void cboSnapBizType_SelectionChanged(object sender, EventArgs e)
        {
            //try
            //{
            //    this.Dispatcher.BeginInvoke(new System.Action(() =>
            //    {
            //        cboSnapBizType.SelectionChanged -= cboSnapBizType_SelectionChanged;

            //        C1ComboBox[] cb = { cboSnapArea };
            //        MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

            //        SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

            //        cboSnapBizType.SelectionChanged += cboSnapBizType_SelectionChanged;
            //    }));
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }
        #endregion

        #endregion

        #endregion

        #region [ Biz Caller ] 

        #region < 동 호출 >
        /// <summary>
        /// CWA 경우 DB 분리로 인해서, 자신의 동만 보이도록 처리,
        /// 오창의 경우, DB 분리가 되지 않아서, 모든 곳을 검색할수 있도록 콤보 선택 처리 
        /// </summary>
        private void SetAreaByShopId(C1ComboBox cb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";
                    dt.Columns.Add("CBO_CODE", typeof(string));
                    dt.Columns.Add("CBO_NAME", typeof(string));

                    DataRow dtDr = dt.NewRow();
                    dtDr["CBO_CODE"] = "";
                    dtDr["CBO_NAME"] = "-SELECT-";
                    dt.Rows.Add(dtDr);

                    DataRow dtDr2 = dt.NewRow();
                    dtDr2["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                    dtDr2["CBO_NAME"] = LoginInfo.CFG_AREA_NAME;
                    dt.Rows.Add(dtDr2);

                    cb.ItemsSource = DataTableConverter.Convert(dt);
                    cb.SelectedIndex = 0;
                }
                else
                {
                    DataRow cbDr = dtResult.NewRow();
                    cbDr["CBO_CODE"] = "";
                    cbDr["CBO_NAME"] = "-SELECT-";
                    dtResult.Rows.InsertAt(cbDr, 0);

                    cb.ItemsSource = DataTableConverter.Convert(dtResult);


                    cb.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < 라인 콤보 생성 >
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, AREAID
        /// Event : cboSnapArea_SelectedValueChanged
        /// </summary>
        private void SetcboEQSG(MultiSelectionBox msbEqsg, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                //cboEqsg.SelectionChanged -= cboSnapEqsg_SelectionChanged;
                string sSelectedValue = msbEqsg.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {

                    msbEqsg.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    msbEqsg.Check(i);
                                    break;
                                }
                            }
                        }
                        else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                        {
                            msbEqsg.Check(i);
                            break;
                        }
                    }
                }
                //cboEqsg.SelectionChanged += cboSnapEqsg_SelectionChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 공정 콤보 생성 >
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, EQSGID
        /// Event : cboSnapEqsg_SelectionChanged
        /// </summary>
        private void SetcboProcess(MultiSelectionBox msbProc, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                //cboProc.SelectionChanged -= cboSnapProc_SelectionChanged;
                string sSelectedValue = msbProc.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {

                    msbProc.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    msbProc.Check(i);
                                    break;
                                }
                            }
                        }
                        else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_PROC_ID)
                        {
                            msbProc.Check(i);
                            break;
                        }
                    }
                }
                //cboProc.SelectionChanged += cboSnapProc_SelectionChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 모델 콤보 생성 > 
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : SHOPID, AREAID, EQSGID, SYSTEM_ID, USERID
        /// Event : cboSnapArea_SelectedValueChanged, cboSnapEqsg_SelectionChanged
        /// Fixed : SYSTEM_ID
        /// </summary>
        private void SetcboProductModel(MultiSelectionBox cboModel, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                //cboModel.SelectionChanged -= cboSnapModel_SelectionChanged;

                string sSelectedValue = cboModel.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    cboModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    cboModel.Check(i);
                                    break;
                                }
                            }
                        }
                        else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                        {
                            cboModel.Check(i);
                            break;
                        }
                    }
                }
                //cboModel.SelectionChanged += cboSnapModel_SelectionChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < Biz Type >
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, SHOPID, AREAID, EQSGID, PROCID, AREA_TYPE_CODE
        /// Event : cboSnapArea_SelectedValueChanged, cboSnapEqsg_SelectionChanged, cboSnapProc_SelectionChanged
        /// Optional : PROCID
        /// Fixed : AREA_TYPE_CODE
        /// </summary>
        private void SetcboBizType(MultiSelectionBox cboBizType, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                //cboBizType.SelectionChanged -= cboSnapBizType_SelectionChanged
                string sSelectedValue = cboBizType.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PROCID"] = msb[1] == null ? null : msb[1].SelectedItemsToString == "" ? null : msb[1].SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    cboBizType.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    cboBizType.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
                //cboBizType.SelectionChanged += cboSnapBizType_SelectionChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < PRODUCT > 
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, SHOPID, AREAID, EQSGID, PROCID, MODELID, AREA_TYPE_CODE, PRDT_CLSS_CODE 
        /// Event : cboSnapArea_SelectedValueChanged, cboSnapEqsg_SelectionChanged, cboSnapProc_SelectionChanged, cboSnapModel_SelectionChanged, cboSnapBizType_SelectionChanged
        /// Optional : PROCID, MODELID, PRDT_CLSS_CODE
        /// Fixed : AREA_TYPE_CODE
        /// </summary>
        private void SetcboProduct(MultiSelectionBox cboProd, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                string sSelectedValue = cboProd.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                dr["PROCID"] = msb[1] == null ? null : msb[1].SelectedItemsToString;
                dr["MODLID"] = msb[2] == null ? null : msb[2].SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = msb[3] == null ? null : msb[3].SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_PACK_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    cboProd.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    cboProd.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < 제공 상태 >
        private void SetcboWipState(MultiSelectionBox cboWipState)
        {
            try
            {
                string sSelectedValue = cboWipState.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_WIPSTAT_WIPSEARCH";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {

                    cboWipState.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    cboWipState.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        private void btnSnapSearch_Click(object sender, RoutedEventArgs e)
        {
            SetListShot();
        }

        private void SetListShot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                if (cboSnapArea.SelectedValue == null)
                {
                    Util.MessageInfo("SFU1499");
                    return;
                }

                if (Util.ConvertEmptyToNull(cboSnapEqsg.SelectedItemsToString) == null)
                {
                    Util.MessageInfo("SFU1223");
                    return;
                }


                if (Util.ConvertEmptyToNull(cboSnapProc.SelectedItemsToString) == null)
                {
                    Util.MessageInfo("SFU1459");
                    return;
                }

                //if (Util.ConvertEmptyToNull(cboSnapModel.SelectedItemsToString) == null)
                //{
                //    Util.MessageInfo("SFU8231");
                //    return;
                //}

                if (cboSnapStockSeqShot.SelectedValue == null)
                {
                    Util.MessageInfo("SFU2958");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int64));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WIPSTAT", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                RQSTDT.Columns.Add("PRJT_ABBR_NAME", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_SEQNO"] = Util.GetCondition(cboSnapStockSeqShot);
                //dr["STCK_CNT_YM"] = Util.GetCondition(ldpSnapMonthShot);
                dr["AREAID"] = Util.GetCondition(cboSnapArea);
                if (string.IsNullOrWhiteSpace(Util.GetCondition(txtSnapLotId)))
                {
                    dr["WIPSTAT"] = null;
                }
                else
                {
                    dr["WIPSTAT"] = Util.GetCondition(cboSnapWipstat);
                }


                if (string.IsNullOrWhiteSpace(Util.GetCondition(txtSnapLotId)))
                {

                    dr["EQSGID"] = Util.ConvertEmptyToNull(cboSnapEqsg.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapEqsg.SelectedItemsToString);
                    dr["PROCID"] = Util.ConvertEmptyToNull(cboSnapProc.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapProc.SelectedItemsToString);
                    //dr["PRODID"] = Util.ConvertEmptyToNull(cboSnapProduct.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapProduct.SelectedItemsToString);
                    //dr["PRDTYPE"] = Util.ConvertEmptyToNull(cboSnapBizType.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapBizType.SelectedItemsToString);
                    //dr["PRJT_ABBR_NAME"] = Util.ConvertEmptyToNull(cboSnapModel.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapModel.SelectedItemsToString);
                }
                else
                {
                    dr["LOTID"] = string.IsNullOrWhiteSpace(Util.GetCondition(txtSnapLotId)) ? null : Util.GetCondition(txtSnapLotId);
                }

                RQSTDT.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_SNAP", "RQSTDT", "RSLTDT", RQSTDT);
                //Util.GridSetData(dgListSnapStock, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void ldpSnapMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            //    if (cboSnapArea.SelectedValue == null) return;

            //    object[] objStockSeqShotParent = { cboSnapArea, ldpSnapMonthShot };
            //    String[] sFilterSnapAll = { "" };
            //    _combo.SetComboObjParent(cboSnapStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterSnapAll);
            //}
            //catch(Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void SetStockSeq(C1ComboBox cbo, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_CMPL_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.ConvertEmptyToNull(sFilter[0]);
                dr["STCK_CNT_YM"] = Util.ConvertEmptyToNull(sFilter[1]);
                dr["STCK_CNT_CMPL_FLAG"] = Util.ConvertEmptyToNull(sFilter[2]);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCKCNT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (sender == null) return;

            //    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
            //    object objRowIdx = dgListSnapStock.Rows[idx].DataItem;

            //    if (objRowIdx != null)
            //    {
            //        if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
            //        {
            //            DataTableConverter.SetValue(dgListSnapStock.Rows[idx].DataItem, "CHK", true);
            //        }
            //        else
            //        {
            //            DataTableConverter.SetValue(dgListSnapStock.Rows[idx].DataItem, "CHK", false);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void btnExclude_SNAP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //    int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListSnapStock, "CHK");

                //    if (iRow < 0)
                //    {
                //        Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                //        return;
                //    }

                //TextRange textRange = new TextRange(txtExcludeNote_SNAP.Document.ContentStart, txtExcludeNote_SNAP.Document.ContentEnd);

                //if (string.IsNullOrEmpty(textRange.Text.Trim()))
                //{
                //    Util.MessageValidation("SFU1590");  //비고를 입력해 주세요.
                //    return;
                //}

                //전산재고 LOTID를 제외 하시겠습니까?
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4212"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //{
                //    if (result.ToString().Equals("OK"))
                //    {
                //        string sSTCK_CNT_EXCL_FLAG = "Y";
                //        Exclude_SNAP(sSTCK_CNT_EXCL_FLAG);
                //    }
                //}
                //);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Exclude_SNAP(string sSTCK_CNT_EXCL_FLAG)
        {
            //try
            //{
            //    this.dgListSnapStock.EndEdit();
            //    this.dgListSnapStock.EndEditRow(true);

            //    DataTable dtRSLT = ((DataView)dgListSnapStock.ItemsSource).Table;
            //    TextRange textRange = new TextRange(txtExcludeNote_SNAP.Document.ContentStart, txtExcludeNote_SNAP.Document.ContentEnd);

            //    //RQSTDT
            //    DataSet inData = new DataSet();
            //    DataTable RQSTDT = inData.Tables.Add("RQSTDT");
            //    RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
            //    RQSTDT.Columns.Add("AREAID", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
            //    RQSTDT.Columns.Add("LOTID", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_EXCL_FLAG", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_EXCL_USERID", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_EXCL_NOTE", typeof(string));
            //    RQSTDT.Columns.Add("USERID", typeof(string));

            //    for (int i = 0; i < dtRSLT.Rows.Count; i++)
            //    {
            //        if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
            //        {
            //            DataRow dr = RQSTDT.NewRow();
            //            dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
            //            dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
            //            dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
            //            dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
            //            dr["STCK_CNT_EXCL_FLAG"] = sSTCK_CNT_EXCL_FLAG;
            //            dr["STCK_CNT_EXCL_USERID"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? LoginInfo.USERID : "";
            //            dr["STCK_CNT_EXCL_NOTE"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? textRange.Text.ToString() : "";
            //            dr["USERID"] = LoginInfo.USERID;

            //            RQSTDT.Rows.Add(dr);
            //        }
            //    }

            //    loadingIndicator.Visibility = Visibility.Visible;

            //    new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_SNAP", "RQSTDT", null, (bizResult, bizException) =>
            //    {
            //        try
            //        {
            //            if (bizException != null)
            //            {
            //                Util.MessageException(bizException);
            //                return;
            //            }

            //            Util.MessageInfo("SFU1275");//정상처리되었습니다.

            //            SetListShot();
            //            txtExcludeNote_SNAP.AppendText(string.Empty);
            //        }
            //        catch (Exception ex)
            //        {
            //            Util.MessageException(ex);
            //        }
            //        finally
            //        {
            //            loadingIndicator.Visibility = Visibility.Collapsed;
            //        }

            //    }, inData);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void SetListCompareDetail(string sProdID = null, string sEqsgID = null, string sProcID = null, string sElecType = null, string sPrjtName = null, string sAutoWhStckFlag = null, string sStckAdjFlag = null)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDegreeAdd_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{


            //    COM001_011_STOCKCNT_START wndSTOCKCNT_START = new COM001_011_STOCKCNT_START();
            //    wndSTOCKCNT_START.FrameOperation = FrameOperation;

            //    if (wndSTOCKCNT_START != null)
            //    {
            //        object[] Parameters = new object[6];
            //        Parameters[0] = Convert.ToString(cboSnapArea.SelectedValue);
            //        Parameters[1] = ldpSnapMonthShot.SelectedDateTime;

            //        C1WindowExtension.SetParameters(wndSTOCKCNT_START, Parameters);

            //        wndSTOCKCNT_START.Closed += new EventHandler(wndSTOCKCNT_START_Closed);

            //        // 팝업 화면 숨겨지는 문제 수정.
            //        this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_START.ShowModal()));
            //        wndSTOCKCNT_START.BringToFront();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void btnDegreeClose_Click(object sender, RoutedEventArgs e)
        {
            //if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            //{
            //    Util.MessageValidation("SFU3499"); //마감된 차수입니다.
            //    return;
            //}

            //마감하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1276"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DegreeClose();
                }
            }
            );
        }

        private void DegreeClose()
        {
            //try
            //{

            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
            //    RQSTDT.Columns.Add("AREAID", typeof(string));
            //    RQSTDT.Columns.Add("USERID", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["STCK_CNT_YM"] = Util.GetCondition(ldpSnapMonthShot);
            //    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboSnapStockSeqShot, "SFU2958"));
            //    dr["AREAID"] = Util.GetCondition(cboSnapArea, "SFU3203"); //동은필수입니다.
            //    dr["USERID"] = LoginInfo.USERID;

            //    if (dr["AREAID"].Equals("")) return;

            //    RQSTDT.Rows.Add(dr);
            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT", "INDATA", null, RQSTDT);
            //    SetStockSeq();

            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void wndSTOCKCNT_START_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_011_STOCKCNT_START window = sender as COM001_011_STOCKCNT_START;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SetStockSeq();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //차수 마감, 추가시 콤보 생성
        //여기
        private void SetStockSeq()
        {
            //try
            //{

            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("AREAID", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));


            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["AREAID"] = Util.NVC(cboSnapArea.SelectedValue) == "" ? null : cboSnapArea.SelectedValue.ToString();
            //    dr["STCK_CNT_YM"] = Util.GetCondition(ldpSnapMonthShot);

            //    RQSTDT.Rows.Add(dr);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCKCNT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            //    if (dtResult.Rows.Count > 0)
            //    {
            //        cboSnapStockSeqShot.ItemsSource = null;
            //        cboSnapStockSeqShot.ItemsSource = DataTableConverter.Convert(dtResult);
            //        cboSnapStockSeqShot.SelectedIndex = 0;
            //    }

            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private void grdSub_AccessKeyPressed(object sender, System.Windows.Input.AccessKeyPressedEventArgs e)
        {

        }


    }

    public class ViewModel2
    {
        public ObservableCollection<string> ControlTypes
        {
            get;
            private set;
        }
        public ViewModel2()
        {
            ControlTypes = new ObservableCollection<string>() { "Button", "TextBox", "CheckBox" };
        }
    }

}

