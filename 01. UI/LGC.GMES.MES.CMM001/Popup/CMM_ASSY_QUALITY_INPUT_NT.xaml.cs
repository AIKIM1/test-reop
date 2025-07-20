/*************************************************************************************
 Created Date : 2018.08.13
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 남경 노칭 품질정보 입력 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.13  INS 김동일K : Initial Created.
  2017.09.15  INS 염규범S : GMES 품질 치수 내용 저장시 System 확인후 " 저장 할까요" Message Pop up 요청의 - C20170911_81274, C20170911_81363
**************************************************************************************/

using System;
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
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_QUALITY_INPUT_NT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_QUALITY_INPUT_NT : C1Window, IWorkArea
    {   
        #region Declaration & Constructor 
        private string _Proc = string.Empty;
        private string _Line = string.Empty;
        private string _Eqpt = string.Empty;
        private string _LotId = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WO_DETL_ID = string.Empty;
        private string _LotId2 = string.Empty;

        private bool bLoad = false;
        #endregion

        #region Initialize        
        public CMM_ASSY_QUALITY_INPUT_NT()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 7)
            {
                _Line = Util.NVC(tmps[0]);
                _Eqpt = Util.NVC(tmps[1]);
                _Proc = Util.NVC(tmps[2]);
                _LotId = Util.NVC(tmps[3]);
                _WipSeq = Util.NVC(tmps[4]);
                _WO_DETL_ID = Util.NVC(tmps[5]);
                _LotId2 = Util.NVC(tmps[6]);
            }

            GetBasicInfo(dg: dgQualityInfo);

            if (_LotId2 != "")
            {
                if (dgQualityInfo.Columns.Contains("CLCTVAL02"))
                {
                    dgQualityInfo.Columns["CLCTVAL02"].Visibility = Visibility.Visible;
                    dgQualityInfo.Columns["CLCTVAL02"].Header = _LotId2;
                }
            }

            if (dgQualityInfo.Columns.Contains("CLCTVAL01"))
            {
                dgQualityInfo.Columns["CLCTVAL01"].Header = _LotId;
            }

            bLoad = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        /// <summary>
        /// INS 염규범S : GMES 품질 치수 내용 저장시 System 확인후 " 저장 할까요" Message Pop up 요청의
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            if (dgQualityInfo == null || dgQualityInfo.Rows.Count < 1) return;

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveQuality(dgQualityInfo);
                }
            });
        }

        private void CLCTVAL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                int rIdx = 0;
                int cIdx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox n = sender as C1NumericBox;
                    StackPanel panel = n.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    rIdx = p.Cell.Row.Index;
                    cIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;
                }

                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    StackPanel panel = n.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    rIdx = p.Cell.Row.Index;
                    cIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;
                }
                else
                    return;

                if (grid.GetRowCount() > ++rIdx)
                {
                    C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                    StackPanel panel = p.Content as StackPanel;

                    for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                    {
                        if (panel.Children[cnt].Visibility == Visibility.Visible)
                            panel.Children[cnt].Focus();
                    }
                }
            }
        }

        private void dgQualityInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.ToUpper().IndexOf("CLCTVAL01") >= 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (e.Cell.Column.Name.ToUpper().IndexOf("CLCTVAL02") >= 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgQualityInfo_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void CLCTVAL_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() > ++rIdx)
                    {
                        C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() > --rIdx)
                    {
                        if (rIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox n = sender as C1NumericBox;
                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox n = sender as C1NumericBox;
                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                }
                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        #region Biz
        private void GetBasicInfo(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqstAdd = new DataTable();
                dtRqstAdd.Columns.Add("LANGID", typeof(string));
                dtRqstAdd.Columns.Add("AREAID", typeof(string));
                dtRqstAdd.Columns.Add("PROCID", typeof(string));
                dtRqstAdd.Columns.Add("LOTID", typeof(string));
                //dtRqstAdd.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                //dtRqstAdd.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqstAdd.Columns.Add("EQPTID", typeof(string));

                DataRow drAdd = dtRqstAdd.NewRow();
                drAdd["LANGID"] = LoginInfo.LANGID;
                drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
                drAdd["PROCID"] = _Proc;
                drAdd["LOTID"] = _LotId;
                //drAdd["CLCTITEM_CLSS4"] = sType;
                drAdd["EQPTID"] = _Eqpt;

                dtRqstAdd.Rows.Add(drAdd);

                DataTable dtRsltAdd = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM_CL", "INDATA", "OUTDATA", dtRqstAdd);

                Util.gridClear(dg);
                dg.ItemsSource = dtRsltAdd.DefaultView;//DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DateTime GetDBDateTime()
        {
            try
            {
                DateTime tmpDttm = DateTime.Now;
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_GETDATE", null, "OUTDATA", null);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    tmpDttm = (DateTime)dtRslt.Rows[0]["DATE"];
                }

                return tmpDttm;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        private void SaveQuality(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("CLCTVAL01", typeof(string));
                dtRqst.Columns.Add("CLCTMAX", typeof(string));
                dtRqst.Columns.Add("CLCTMIN", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CLCTSEQ_ORG", typeof(Int16));

                dg.EndEdit();

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows) // DataGridHandler.GetAddedItems(dg))
                {
                    DataRowView drvTmp = (row.DataItem as DataRowView);
                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LOTID"] = _LotId;
                    dr["WIPSEQ"] = _WipSeq;
                    dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                    dr["CLCTITEM"] = drvTmp["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                    dr["CLCTVAL01"] = (!drvTmp["CLCTVAL01"].Equals(DBNull.Value) && !drvTmp["CLCTVAL01"].Equals("-")) ? drvTmp["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");

                    if (Util.NVC(drvTmp["MAND_INSP_ITEM_FLAG"]).ToUpper().Equals("Y"))
                    {
                        if (Util.NVC(dr["CLCTVAL01"]).Length < 1)
                        {
                            object[] parameters = new object[1];
                            parameters[0] = Util.NVC(drvTmp["CLSS_NAME1"]) + (Util.NVC(drvTmp["CLSS_NAME2"]).Length > 0 ? " - " + Util.NVC(drvTmp["CLSS_NAME2"]) : string.Empty);
                            Util.MessageInfo("SFU3589", parameters); // 품질 항목[%1]은 필수 항목 입니다.

                            return;
                        }
                    }

                    dr["CLCTMAX"] = drvTmp["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                    dr["CLCTMIN"] = drvTmp["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                    dr["EQPTID"] = _Eqpt;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["CLCTSEQ_ORG"] = drvTmp["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");

                    dtRqst.Rows.Add(dr);

                    if (_LotId2 != "")
                    {
                        if (rdoAll.IsChecked.HasValue && (bool)rdoAll.IsChecked)
                        {
                            dr = dtRqst.NewRow();

                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            dr["LOTID"] = _LotId2;
                            dr["WIPSEQ"] = _WipSeq;
                            dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                            dr["CLCTITEM"] = drvTmp["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                            dr["CLCTVAL01"] = (!drvTmp["CLCTVAL01"].Equals(DBNull.Value) && !drvTmp["CLCTVAL01"].Equals("-")) ? drvTmp["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");

                            if (Util.NVC(drvTmp["MAND_INSP_ITEM_FLAG"]).ToUpper().Equals("Y"))
                            {
                                if (Util.NVC(dr["CLCTVAL01"]).Length < 1)
                                {
                                    object[] parameters = new object[1];
                                    parameters[0] = Util.NVC(drvTmp["CLSS_NAME1"]) + (Util.NVC(drvTmp["CLSS_NAME2"]).Length > 0 ? " - " + Util.NVC(drvTmp["CLSS_NAME2"]) : string.Empty);
                                    Util.MessageInfo("SFU3589", parameters); // 품질 항목[%1]은 필수 항목 입니다.

                                    return;
                                }
                            }

                            dr["CLCTMAX"] = drvTmp["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                            dr["CLCTMIN"] = drvTmp["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                            dr["EQPTID"] = _Eqpt;
                            dr["USERID"] = LoginInfo.USERID;
                            dr["CLCTSEQ_ORG"] = drvTmp["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");

                            dtRqst.Rows.Add(dr);
                        }
                        else
                        {
                            dr = dtRqst.NewRow();

                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            dr["LOTID"] = _LotId2;
                            dr["WIPSEQ"] = _WipSeq;
                            dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                            dr["CLCTITEM"] = drvTmp["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                            dr["CLCTVAL01"] = (!drvTmp["CLCTVAL02"].Equals(DBNull.Value) && !drvTmp["CLCTVAL02"].Equals("-")) ? drvTmp["CLCTVAL02"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");

                            if (Util.NVC(drvTmp["MAND_INSP_ITEM_FLAG"]).ToUpper().Equals("Y"))
                            {
                                if (Util.NVC(dr["CLCTVAL01"]).Length < 1)
                                {
                                    object[] parameters = new object[1];
                                    parameters[0] = Util.NVC(drvTmp["CLSS_NAME1"]) + (Util.NVC(drvTmp["CLSS_NAME2"]).Length > 0 ? " - " + Util.NVC(drvTmp["CLSS_NAME2"]) : string.Empty);
                                    Util.MessageInfo("SFU3589", parameters); // 품질 항목[%1]은 필수 항목 입니다.

                                    return;
                                }
                            }

                            dr["CLCTMAX"] = drvTmp["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                            dr["CLCTMIN"] = drvTmp["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                            dr["EQPTID"] = _Eqpt;
                            dr["USERID"] = LoginInfo.USERID;
                            dr["CLCTSEQ_ORG"] = drvTmp["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");

                            dtRqst.Rows.Add(dr);
                        }
                    }
                }

                if (dtRqst.Rows.Count > 0)
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst);

                    Util.MessageInfo("SFU1270");      //저장되었습니다.

                    btnQualitySave.IsEnabled = false;
                }
                else
                {
                    Util.MessageInfo("SFU1566");      //변경된데이타가없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region Func
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnQualitySave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo.IsVisible)
                cbo.SelectedIndex = 0;
        }

        private void rdoLot_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                if (dgQualityInfo == null) return;

                RadioButton rdo = (RadioButton)sender;

                if (rdo != null)
                {
                    if (_LotId2 != "")
                    {
                        if (dgQualityInfo.Columns.Contains("CLCTVAL02"))
                        {
                            dgQualityInfo.Columns["CLCTVAL02"].Visibility = Visibility.Visible;
                            dgQualityInfo.Columns["CLCTVAL02"].Header = _LotId2;
                        }
                    }

                    if (dgQualityInfo.Columns.Contains("CLCTVAL01"))
                    {
                        dgQualityInfo.Columns["CLCTVAL01"].Header = _LotId;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                if (dgQualityInfo == null) return;

                RadioButton rdo = (RadioButton)sender;

                if (rdo != null)
                {
                    if (_LotId2 != "")
                    {
                        if (dgQualityInfo.Columns.Contains("CLCTVAL02"))
                        {
                            dgQualityInfo.Columns["CLCTVAL02"].Visibility = Visibility.Collapsed;
                        }
                    }

                    if (dgQualityInfo.Columns.Contains("CLCTVAL01"))
                    {
                        dgQualityInfo.Columns["CLCTVAL01"].Header = Util.NVC(ObjectDic.Instance.GetObjectName("측정값"));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
