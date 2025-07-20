/*************************************************************************************
 Created Date : 2020.10.22
      Creator : Dooly
   Decription : Tray별 Cell Data
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.22  DEVELOPER : Initial Created.
  2024.12.02  최도훈    : null 예외처리 추가

 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;
using System.IO;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_024 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private string _sTrayId;
        private string _iTrayNo;
        private string _sFinCd;
        private string _sActYN = "N"; //다른 창에서 넘어오는지 체크 해서 Active Event 제어

        public FCS001_024()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = this.FrameOperation.Parameters;
            if(tmps!=null && tmps.Length>=1)
            {
                _sTrayId = Util.NVC(tmps[0]);
                _iTrayNo = Util.NVC(tmps[1]);
                _sFinCd = Util.NVC(tmps[2]);
                _sActYN = Util.NVC(tmps[3]);
            }
            //다른화면에서 넘어온 경우

            if (!string.IsNullOrEmpty(_sTrayId) && _sActYN.Equals("Y"))
            {
                txtLotID.Text = _sTrayId;
                GetOp();
                GetList();
            }

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Event
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void fpsOp_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.HeaderPresenter == null)
            {
                return;
            }

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;

                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }

            }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetList();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetOp();
                GetList();
            }
        }

        #endregion

        #region Method
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(fpsOp.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            fpsOp.ItemsSource = DataTableConverter.Convert(dt);

        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(fpsOp.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            fpsOp.ItemsSource = DataTableConverter.Convert(dt);

        }
        private void GetOp()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = _iTrayNo;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_OPER", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(fpsOp, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PROCNAME", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("PROC_DETL_TYPE_CODE", typeof(string));

                for (int i = 0; i < fpsOp.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(fpsOp.Rows[i].DataItem, "CHK")).Equals("True")
                     || Util.NVC(DataTableConverter.GetValue(fpsOp.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["LOTID"] =_iTrayNo;
                        dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(fpsOp.Rows[i].DataItem, "PROCID"));
                        dr["PROCNAME"] = Util.NVC(DataTableConverter.GetValue(fpsOp.Rows[i].DataItem, "PROCNAME"));
                        dr["PROC_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(fpsOp.Rows[i].DataItem, "PROC_GR_CODE"));
                        dr["PROC_DETL_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(fpsOp.Rows[i].DataItem, "PROC_DETL_TYPE_CODE"));
                        dtRqst.Rows.Add(dr);
                    }
                }

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                ShowLoadingIndicator();
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CELL_INFO_BY_TRAYNO", "INDATA", "APD_DATA,SUBLOT_INFO", dsRqst);

                //dgCellData = null;

                Util.gridClear(dgCellData);

                CombineCellInfo(dsRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void CombineCellInfo(DataSet dsRet)
        {
            dgCellData.Columns.Clear();
            DataSet dsCellInfo = new DataSet();
            try
            {
                DataTable dtCellInfo = new DataTable();

                for (int i = 0; i < dsRet.Tables["SUBLOT_INFO"].Columns.Count; i++)
                {
                    SetGridHeaderSingle(dsRet.Tables["SUBLOT_INFO"].Columns[i].ColumnName, dgCellData);
                    DataColumn dcCellInfo = new DataColumn(dsRet.Tables["SUBLOT_INFO"].Columns[i].ColumnName.ToString());
                    dtCellInfo.Columns.Add(dcCellInfo);
                }

                DataSet[] dsApd = new DataSet[dsRet.Tables["APD_DATA"].Rows.Count];

                for (int i = 0; i < dsRet.Tables["APD_DATA"].Rows.Count; i++)
                {
                    SetXMLWithSchema(dsRet.Tables["APD_DATA"].Rows[i]["XMLDATA"].ToString(), out dsApd[i]);
                    dsApd[i].DataSetName = dsRet.Tables["APD_DATA"].Rows[i]["PROCID"].ToString();

                    SetGridHeaderMulti(dsApd[i].Tables[0], dgCellData);

                    for (int j = 0; j < dsApd[i].Tables[0].Columns.Count; j++)
                    {
                        DataColumn dcApd = new DataColumn(dsApd[i].Tables[0].TableName + dsApd[i].Tables[0].Columns[j].ColumnName.ToString());
                        dtCellInfo.Columns.Add(dcApd);
                    }
                }

                for (int i = 0; i < dsRet.Tables["SUBLOT_INFO"].Rows.Count; i++)
                {
                    DataRow dr = dtCellInfo.NewRow();
                    for (int j = 0; j < dsRet.Tables["SUBLOT_INFO"].Columns.Count; j++)
                    {
                        dr[dsRet.Tables["SUBLOT_INFO"].Columns[j].ColumnName] = dsRet.Tables["SUBLOT_INFO"].Rows[i][dsRet.Tables["SUBLOT_INFO"].Columns[j].ColumnName].ToString();
                    }

                    for (int x = 0; x < dsApd.Length; x++)
                    {
                        for (int n = 0; n < dsApd[x].Tables[0].Columns.Count; n++)
                        {
                            if(dsApd[x].Tables[0].Rows.Count > i)
                            {
                                dr[dsApd[x].Tables[0].TableName + dsApd[x].Tables[0].Columns[n].ColumnName]
                                    = dsApd[x].Tables[0].Rows[i][dsApd[x].Tables[0].Columns[n].ColumnName].ToString();
                            }
                        }
                    }

                    dtCellInfo.Rows.Add(dr);
                }

                //dgCellData.ItemsSource = DataTableConverter.Convert(dtCellInfo);
                //dgCellData.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                Util.GridSetData(dgCellData, dtCellInfo, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetXMLWithSchema(string XML, out DataSet dsParam)
        {
            try
            {
                dsParam = new DataSet();
                byte[] byteXML = System.Text.Encoding.UTF8.GetBytes(XML);

                MemoryStream ms = new MemoryStream(byteXML, 0, byteXML.Length);
                dsParam.ReadXml(ms, XmlReadMode.Auto);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 가변적인 Single Header 생성
        private void SetGridHeaderSingle(string sColName, C1DataGrid dg)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                //Header = ObjectDic.Instance.GetObjectName(sColName),
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
        }
        #endregion

        #region 가변적인 Multi Header 생성
        private void SetGridHeaderMulti(DataTable dt, C1DataGrid dg)
        {
            string sTopHeader = dt.TableName.ToString();

            C1.WPF.DataGrid.DataGridTemplateColumn dc = new C1.WPF.DataGrid.DataGridTemplateColumn();

            //DataGridTemplateColumn.Header 처리를 위한 GRID
            Grid g = new Grid();
            g.Width = double.NaN;

            //Row 생성(두줄)
            for (int i = 0; i < 2; i++)
            {
                RowDefinition r = new RowDefinition();
                g.RowDefinitions.Add(r);
            }

            //컬럼 Count만큼 컬럼 생성
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ColumnDefinition c = new ColumnDefinition();
                g.ColumnDefinitions.Add(c);
            }

            //DataGridTemplateColumn.CellTemplate의 DataTemplate 처리를 위한 GRID
            Grid g2 = new Grid();
            g2.Width = double.NaN;
            g2.HorizontalAlignment = HorizontalAlignment.Center;

            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Grid));

            //첫번째 Row Header 생성
            TextBlock tbh = new TextBlock();
            tbh.Text = sTopHeader;

            Grid.SetRow(tbh, 0);
            Grid.SetColumn(tbh, 0);
            Grid.SetColumnSpan(tbh, dt.Columns.Count);
            g.Children.Add(tbh);

            tbh.TextAlignment = TextAlignment.Center;
            tbh.Padding = new Thickness(5, 1, 5, 1);
            //tbh.Background = Brushes.Yellow;

            g.HorizontalAlignment = HorizontalAlignment.Center;

            //두번째 Row의 Header 처리
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                //DataGridTemplateColumn.Header 처리
                TextBlock tb = new TextBlock();
                //tb.Text = dt.Columns[i].ColumnName.ToString();
                tb.Text = ObjectDic.Instance.GetObjectName(dt.Columns[i].ColumnName.ToString());
                tb.TextAlignment = TextAlignment.Center;
                //tb.Background = Brushes.Yellow;
                tb.Padding = new Thickness(5, 1, 5, 1);
                tb.TextWrapping = TextWrapping.Wrap;

                Grid.SetRow(tb, i + 1);
                Grid.SetColumn(tb, i);

                g.Children.Add(tb);

                //DataGridTemplateColumn.CellTemplate의 DataTemplate 처리
                FrameworkElementFactory columnDefinitionFactory = new FrameworkElementFactory(typeof(ColumnDefinition));
                factory.AppendChild(columnDefinitionFactory);

                factory.SetValue(Grid.WidthProperty, double.NaN);

                FrameworkElementFactory factorytb = new FrameworkElementFactory(typeof(TextBlock));
                factorytb.SetValue(Grid.ColumnProperty, i);
                factorytb.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                factorytb.SetValue(TextBlock.PaddingProperty, new Thickness(5, 1, 5, 1));

                //DataBinding 처리
                Binding b = new Binding(sTopHeader + dt.Columns[i].ColumnName.ToString());
                b.Mode = BindingMode.TwoWay;
                factorytb.SetBinding(TextBlock.TextProperty, b);
                factory.AppendChild(factorytb);
            }

            DataTemplate template = new DataTemplate();
            template.VisualTree = factory;
            dc.CellTemplate = template;
            template.Seal();

            dc.Header = g;
            dc.HorizontalAlignment = HorizontalAlignment.Center;
            dc.VerticalAlignment = VerticalAlignment.Center;

            dg.Columns.Add(dc);

        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion


    }
}
