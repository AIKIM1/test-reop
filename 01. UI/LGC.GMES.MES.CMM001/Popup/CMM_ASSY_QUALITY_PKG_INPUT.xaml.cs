
/*************************************************************************************
 Created Date : 2017.03.13
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 품질정보 입력 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.13  INS 김동일K : Initial Created.
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
    /// CMM_ASSY_QUALITY_PKG_INPUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_QUALITY_PKG_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _Proc = string.Empty;
        private string _Line = string.Empty;
        private string _Eqpt = string.Empty;
        private string _LotId = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WO_DETL_ID = string.Empty;

        private bool bLoad = false;

        private DataTable dtDimension;
        private DataTable dtQuality;
        #endregion

        #region Initialize        
        public CMM_ASSY_QUALITY_PKG_INPUT()
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

            if (tmps.Length >= 6)
            {
                _Line = Util.NVC(tmps[0]);
                _Eqpt = Util.NVC(tmps[1]);
                _Proc = Util.NVC(tmps[2]);
                _LotId = Util.NVC(tmps[3]);
                _WipSeq = Util.NVC(tmps[4]);
                _WO_DETL_ID = Util.NVC(tmps[5]);
            }

            // 남경인 경우 화면 SIZE 조정..
            if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
            {
                this.Width = 800;
                this.Height = 700;
            }

            GetBasicInfo(dg: dgQualityInfo, sType: "A");    // Tensil Strength

            //GetBasicInfo(dg: dgQualityInfoSealing, sType: "C");    // Sealing      

            //GetBasicInfoDimen();    // Dimension      

            bLoad = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            if (dgQualityInfo == null || dgQualityInfo.Rows.Count < 1) return;

            // "SFU1241:저장 할까요?", "SFU4311:스펙이 벗어났는데 저장하시겠습니까?"
            Util.MessageConfirm(EDCCheck(dgQualityInfo) ? "SFU1241" : "SFU4311", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveQuality(dgQualityInfo, "A");
                }
            });
            
        }

        private void btnQualitySaveDimen_Click(object sender, RoutedEventArgs e)
        {
            if (dgQualityInfoDimen == null || dgQualityInfoDimen.Rows.Count < 1) return;

            // "SFU1241:저장 할까요?", "SFU4311:스펙이 벗어났는데 저장하시겠습니까?"
            Util.MessageConfirm(DimensionEDCCheck(dgQualityInfoDimen) ? "SFU1241" : "SFU4311", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveQualityDimension(GetDBDateTime());
                }
            });
        }

        private void btnQualitySaveSealing_Click(object sender, RoutedEventArgs e)
        {
            if (dgQualityInfoSealing == null || dgQualityInfoSealing.Rows.Count < 1) return;

            // "SFU1241:저장 할까요?", "SFU4304:스펙이 벗어났는데 저장하시겠습니까?"
            Util.MessageConfirm(EDCCheck(dgQualityInfoSealing) ? "SFU1241" : "SFU4304", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveQuality(dgQualityInfoSealing, "B");
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

        private void dgQualityInfoDimen_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.ToUpper().IndexOf("NEST") >= 0)
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

        private void dgQualityInfoDimen_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

        private void dgQualityInfoSealing_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgQualityInfoSealing_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
        private void GetBasicInfo(C1.WPF.DataGrid.C1DataGrid dg, string sType)
        {
            try
            {
                ShowLoadingIndicator();
                
                DataTable dtRqstAdd = new DataTable();
                dtRqstAdd.Columns.Add("LANGID", typeof(string));
                dtRqstAdd.Columns.Add("AREAID", typeof(string));
                dtRqstAdd.Columns.Add("PROCID", typeof(string));
                dtRqstAdd.Columns.Add("LOTID", typeof(string));
                dtRqstAdd.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqstAdd.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqstAdd.Columns.Add("EQPTID", typeof(string));

                DataRow drAdd = dtRqstAdd.NewRow();
                drAdd["LANGID"] = LoginInfo.LANGID;
                drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
                drAdd["PROCID"] = _Proc;
                drAdd["LOTID"] = _LotId;
                drAdd["CLCTITEM_CLSS4"] = sType;
                drAdd["EQPTID"] = _Eqpt;
                
                dtRqstAdd.Rows.Add(drAdd);

                DataTable dtRsltAdd = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM_CL", "INDATA", "OUTDATA", dtRqstAdd);

                Util.gridClear(dg);
                dg.ItemsSource = dtRsltAdd.DefaultView;

                DataTable dt = ((DataView)dg.ItemsSource).Table;
                foreach (DataRow row in dt.Rows)
                {
                    if (row["CLCTUNIT"].ToString().Equals("OK/NG"))
                    {
                        row["CLCTVAL01"] = "OK";
                    }
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

        private void GetBasicInfoDimen()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqstAdd = new DataTable();
                dtRqstAdd.Columns.Add("LANGID", typeof(string));
                dtRqstAdd.Columns.Add("AREAID", typeof(string));
                dtRqstAdd.Columns.Add("PROCID", typeof(string));
                dtRqstAdd.Columns.Add("LOTID", typeof(string));
                dtRqstAdd.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqstAdd.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqstAdd.Columns.Add("EQPTID", typeof(string));

                DataRow drAdd = dtRqstAdd.NewRow();
                drAdd["LANGID"] = LoginInfo.LANGID;
                drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
                drAdd["PROCID"] = _Proc;
                drAdd["LOTID"] = _LotId;
                drAdd["CLCTITEM_CLSS4"] = "B";
                drAdd["EQPTID"] = _Eqpt;

                dtRqstAdd.Rows.Add(drAdd);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM_CL_DIMEN", "INDATA", "OUTDATA", dtRqstAdd);

                dtDimension = dtRslt.Copy();    // 저장용 DataTable.

                if (dtRslt != null && dtRslt.Columns.Count > 0)
                {
                    C1.WPF.DataGrid.DataGridLength width = new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto);

                    for (int i = 0; i < dtRslt.Columns.Count; i++)
                    {
                        if (!dgQualityInfoDimen.Columns.Contains(dtRslt.Columns[i].ColumnName))
                        {
                            //Util.SetGridColumnText(dgQualityInfoDimen, dtRslt.Columns[i].ColumnName, null, dtRslt.Columns[i].ColumnName, true, false, false, false, width, HorizontalAlignment.Center, Visibility.Visible, "#,##0", true);
                            Util.SetGridColumnNumeric(dgQualityInfoDimen, dtRslt.Columns[i].ColumnName, null, dtRslt.Columns[i].ColumnName, true, false, false, false, width, HorizontalAlignment.Right, Visibility.Visible, "#,###.###", true, false);

                            // Dimension 코드값 삭제 처리..
                            for (int j = 0; j < dtRslt.Rows.Count; j++)
                            {
                                dtRslt.Rows[j][i] = "";
                            }
                        }
                    }
                }

                dgQualityInfoDimen.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);

                //Util.GridSetData(dgQualityInfoDimen, dtRslt, FrameOperation, true);
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
            catch(Exception ex)
            {
                //Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        private void SaveQuality(C1.WPF.DataGrid.C1DataGrid dg, string sType)
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
                }
                
                if (dtRqst.Rows.Count > 0)
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst);

                    Util.MessageInfo("SFU1270");      //저장되었습니다.

                    if (sType.Equals("A"))
                        btnQualitySave.IsEnabled = false;
                    else if (sType.Equals("B"))
                        btnQualitySaveSealing.IsEnabled = false;
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
        
        /// <summary>
        /// 품질Spec Check
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        private bool EDCCheck(C1.WPF.DataGrid.C1DataGrid dg)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataRowView drvTmp = (row.DataItem as DataRowView);
                string _USL = string.Empty;
                string _LSL = string.Empty;
                string _VALUE = string.Empty;

                _USL = Util.NVC(drvTmp["USL"]); _LSL = Util.NVC(drvTmp["LSL"]); _VALUE = Util.NVC(drvTmp["CLCTVAL01"]);

                if (_USL == "" && _LSL == "")
                    continue;

                double vLSL, vUSL, vVALUE;

                //상한값/하한값
                if (_USL != "" && _LSL != "")
                {
                    if (double.TryParse(_LSL, out vLSL) && double.TryParse(_USL, out vUSL))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vLSL > vVALUE || vUSL < vVALUE)
                            {
                                return false;
                            }
                        }
                    }
                }
                //상한값
                if (_USL != "" && _LSL == "")
                {
                    if (double.TryParse(_USL, out vUSL))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vUSL < vVALUE)
                            {
                                return false;
                            }
                        }
                    }
                }
                //하한값
                if (_USL == "" && _LSL != "")
                {
                    if (double.TryParse(_LSL, out vLSL))
                    {
                        if (double.TryParse(_VALUE, out vVALUE))
                        {
                            if (vLSL > vVALUE)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Dimension 품질Spec Check
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        private bool DimensionEDCCheck(C1.WPF.DataGrid.C1DataGrid dg)
        {
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgQualityInfoDimen.Columns)
            {
                // Nest 컬럼 찾기.
                if (col.Name.ToUpper().IndexOf("NEST") >= 0 &&
                    dtDimension.Columns.Contains(col.Name))
                {
                    foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                    {
                        DataRowView drvTmp = (row.DataItem as DataRowView);
                        string _USL = string.Empty;
                        string _LSL = string.Empty;
                        string _VALUE = string.Empty;

                        _USL = Util.NVC(drvTmp["USL"]); _LSL = Util.NVC(drvTmp["LSL"]); _VALUE = Util.NVC(drvTmp[col.Name]);

                        if (_USL == "" && _LSL == "")
                            continue;

                        double vLSL, vUSL, vVALUE;

                        //상한값/하한값
                        if (_USL != "" && _LSL != "")
                        {
                            if (double.TryParse(_LSL, out vLSL) && double.TryParse(_USL, out vUSL))
                            {
                                if (double.TryParse(_VALUE, out vVALUE))
                                {
                                    if (vLSL > vVALUE || vUSL < vVALUE)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        //상한값
                        if (_USL != "" && _LSL == "")
                        {
                            if (double.TryParse(_USL, out vUSL))
                            {
                                if (double.TryParse(_VALUE, out vVALUE))
                                {
                                    if (vUSL < vVALUE)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        //하한값
                        if (_USL == "" && _LSL != "")
                        {
                            if (double.TryParse(_LSL, out vLSL))
                            {
                                if (double.TryParse(_VALUE, out vVALUE))
                                {
                                    if (vLSL > vVALUE)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        private void SaveQualityDimension(DateTime actDttm)
        {
            try
            {
                bool bSave = false;

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
                dtRqst.Columns.Add("ACTDTTM", typeof(DateTime));

                dgQualityInfoDimen.EndEdit();

                //DateTime actDttm = DateTime.Now;

                //차수별로 따로 던지기 한꺼번에 던지면 비즈에서 처리안됨
                foreach (C1.WPF.DataGrid.DataGridColumn col in dgQualityInfoDimen.Columns)
                {
                    // Nest 컬럼 찾기.
                    if (col.Name.ToUpper().IndexOf("NEST") >= 0 &&
                        dtDimension.Columns.Contains(col.Name))
                    {
                        foreach (C1.WPF.DataGrid.DataGridRow row in dgQualityInfoDimen.Rows) // DataGridHandler.GetAddedItems(dg))
                        {
                            DataRowView drvTmp = (row.DataItem as DataRowView);
                            DataRow dr = dtRqst.NewRow();
                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            dr["LOTID"] = _LotId;
                            dr["WIPSEQ"] = _WipSeq;
                            dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                            dr["CLCTITEM"] = dtDimension.Rows[row.Index][col.Name];
                            dr["CLCTVAL01"] = Util.NVC(dtDimension.Rows[row.Index][col.Name]).Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, col.Name))) ? "" : DataTableConverter.GetValue(row.DataItem, col.Name);

                            if (Util.NVC(dtDimension.Rows[row.Index]["MAND_INSP_ITEM_FLAG"]).ToUpper().Equals("Y"))
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
                            dr["ACTDTTM"] = actDttm;

                            dtRqst.Rows.Add(dr);
                        }

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst);

                        dtRqst.Clear();
                        bSave = true;
                    }
                }                

                if (bSave)
                {
                    Util.MessageInfo("SFU1270");      //저장되었습니다.

                    btnQualitySaveDimen.IsEnabled = false;
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
            listAuth.Add(btnQualitySaveDimen);
            listAuth.Add(btnQualitySaveSealing);
            
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

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!bLoad) return;

            if (c1tabDimen.IsSelected)
            {
                GetBasicInfoDimen();    // Dimension 
            }

            if (c1tab.IsSelected)
            {
                GetBasicInfo(dg: dgQualityInfo, sType: "A");    // Tensil Strength
            }

            if (c1tabSealing.IsSelected)
            {
                GetBasicInfo(dg: dgQualityInfoSealing, sType: "C");    // Sealing
            }

        }

        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        { 
            ComboBox cbo = sender as ComboBox;
            //cbo.SelectedIndex = 0;
        }

    }
}
