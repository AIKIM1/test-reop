/*************************************************************************************
 Created Date : 2020.10.22
      Creator : Kang Dong Hee
   Decription : 저전압 Lot SPEC
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.18  NAME : Initial Created
  2021.04.01  KDH : 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
  2021.04.09  KDH : 조회조건에 AREAID 추가
  2023.09.14  이의철 : E등급 SPEC 추가
  2023.09.28  이의철 : 기존 조회 기간 2달 -> 7일 로 변경
**************************************************************************************/
#define SAMPLE_DEV

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
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_019 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private bool bFCS001_019_E_GRD_SPEC = false; //E등급 SPEC 추가
        Util _Util = new Util();

        public FCS001_019()
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
            try
            {

                //E등급 SPEC 추가
                bFCS001_019_E_GRD_SPEC = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_019_E_GRD_SPEC");

                //Combo Setting
                InitCombo();
                //Control Setting
                InitControl();

                //E등급 SPEC 추가
                if (bFCS001_019_E_GRD_SPEC)
                {
                    this.btnESPECUpdate.Visibility = Visibility.Visible;

                    dgLowVoltage.Columns["E_BAD_CNT"].Visibility = Visibility.Visible;
                    dgLowVoltage.Columns["E_GR_PER"].Visibility = Visibility.Visible;
                    dgLowVoltage.Columns["E_BAD_RATE"].Visibility = Visibility.Visible;
                    dgLowVoltage.Columns["E_GR_PER_LIMIT"].Visibility = Visibility.Visible;
                    dgLowVoltage.Columns["PACK_INPUT_QTY"].Visibility = Visibility.Visible;
                    dgLowVoltage.Columns["WAIT_TIME"].Visibility = Visibility.Visible;
                }
                else
                {
                    this.btnESPECUpdate.Visibility = Visibility.Hidden;

                    dgLowVoltage.Columns["E_BAD_CNT"].Visibility = Visibility.Collapsed;
                    dgLowVoltage.Columns["E_GR_PER"].Visibility = Visibility.Collapsed;
                    dgLowVoltage.Columns["E_BAD_RATE"].Visibility = Visibility.Collapsed;
                    dgLowVoltage.Columns["E_GR_PER_LIMIT"].Visibility = Visibility.Collapsed;
                    dgLowVoltage.Columns["PACK_INPUT_QTY"].Visibility = Visibility.Collapsed;
                    dgLowVoltage.Columns["WAIT_TIME"].Visibility = Visibility.Collapsed;                    
                }

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };

            C1ComboBox[] cboLineChild = { cboModel };
            
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);
            
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.NONE, sCase: "LINEMODEL", cbParent: cboModelParent);
        }

        private void InitControl()
        {
            //dtpFromDate.SelectedDateTime = DateTime.Now.AddMonths(-2);
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpToDate.SelectedDateTime = DateTime.Now;
        }
        #endregion

        #region Event


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgLowVoltage_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    //E등급 SPEC 추가
                    //if (e.Cell.Column.Name.Equals("DAY_GR_LOTID"))
                    if (e.Cell.Column.Name.Equals("BAD_CNT"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue); //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    //E등급 SPEC 추가
                    if (bFCS001_019_E_GRD_SPEC)
                    {
                        if (e.Cell.Column.Name.Equals("E_BAD_CNT"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue); //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }
                    }                    
                }
            }));

        }

        private void dgLowVoltage_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("DAY_GR_LOTID"))
                    {
                        //e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue); //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        //e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;                 //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                    }
                }
            }));
        }

        private void dgLowVoltage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgLowVoltage.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("BAD_CNT"))
                    {
                        FCS001_019_TRAY_SEL TraySelectInfo = new FCS001_019_TRAY_SEL();
                        TraySelectInfo.FrameOperation = FrameOperation;

                        if (TraySelectInfo != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgLowVoltage.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID")).ToString(); //Lot ID;
                            Parameters[1] = "W";

                            C1WindowExtension.SetParameters(TraySelectInfo, Parameters);
                            TraySelectInfo.Closed += new EventHandler(TraySel_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => TraySelectInfo.ShowModal()));
                        }
                    }

                    //E등급 SPEC 추가
                    if (bFCS001_019_E_GRD_SPEC)
                    {
                        if (cell.Column.Name.Equals("E_BAD_CNT"))
                        {
                            FCS001_019_TRAY_SEL TraySelectInfo = new FCS001_019_TRAY_SEL();
                            TraySelectInfo.FrameOperation = FrameOperation;

                            if (TraySelectInfo != null)
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgLowVoltage.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID")).ToString(); //Lot ID;
                                Parameters[1] = "E";

                                C1WindowExtension.SetParameters(TraySelectInfo, Parameters);
                                TraySelectInfo.Closed += new EventHandler(TraySel_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => TraySelectInfo.ShowModal()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void cboLine_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            btnSearch_Click(null, null);
        }

        private void cboModel_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //btnSearch_Click(null, null);
        }

        private void TraySel_Closed(object sender, EventArgs e)
        {
            FCS001_019_TRAY_SEL popup = sender as FCS001_019_TRAY_SEL;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
            this.grdMain.Children.Remove(popup);
        }

        //E등급 SPEC 추가
        private void E_Spec_Change_Closed(object sender, EventArgs e)
        {
            FCS001_019_E_SPEC_CHANGE popup = sender as FCS001_019_E_SPEC_CHANGE;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
            this.grdMain.Children.Remove(popup);
        }

        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                Util.gridClear(dgLowVoltage);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2021.04.09 조회조건에 AREAID 추가

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel);
                dr["FROM_DATE"] = Util.GetCondition(dtpFromDate);
                dr["TO_DATE"] = Util.GetCondition(dtpToDate);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2021.04.09 조회조건에 AREAID 추가
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                string sBiz = string.Empty;

                //E등급 SPEC 추가
                if (bFCS001_019_E_GRD_SPEC)
                {
                    sBiz = "DA_SEL_W_LOT_RJUDG_E_LOT";
                }
                else
                {
                    sBiz = "DA_SEL_W_LOT_RJUDG_NEW";
                }
                
                //new ClientProxy().ExecuteService("DA_SEL_W_LOT_RJUDG_NEW", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                new ClientProxy().ExecuteService(sBiz, "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgLowVoltage, result, FrameOperation, true);
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
                });
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
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

        private void btnEGrUpdate_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
                       

            string sDAY_GR_LOTID = Util.NVC(DataTableConverter.GetValue(this.dgLowVoltage.Rows[clickedIndex].DataItem, "DAY_GR_LOTID"));
            string sE_GR_PER_LIMIT = Util.NVC(DataTableConverter.GetValue(this.dgLowVoltage.Rows[clickedIndex].DataItem, "E_GR_PER_LIMIT"));

            Util.MessageValidation("DAY_GR_LOTID:" + sDAY_GR_LOTID);

            if(string.IsNullOrEmpty(sE_GR_PER_LIMIT))
            {
                //return;
            }

            //저장하시겠습니까?
            Util.MessageConfirm("FM_ME_0214", (result) =>
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                    dtRqst.Columns.Add("E_GR_PER_LIMIT", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["DAY_GR_LOTID"] = sDAY_GR_LOTID;
                    dr["E_GR_PER_LIMIT"] = sE_GR_PER_LIMIT;

                    dr["USERID"] = LoginInfo.USERID;
                    dtRqst.Rows.Add(dr);

                    ShowLoadingIndicator();
                    new ClientProxy().ExecuteService("DA_UPD_TB_SFC_LINE_MDL_E_GRD_LIMIT", "INDATA", null, dtRqst, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //저장하였습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0215"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result2) =>
                            {
                                if (result2 == MessageBoxResult.OK)
                                {
                                    GetList();
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    });

                }
            });

            //DataTable dt = DataTableConverter.Convert(dgLowVoltage.ItemsSource);
            //dt.Rows.RemoveAt(clickedIndex);
            //Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);
        }

        //E등급 SPEC 추가
        private void btnESPECUpdate_Click(object sender, RoutedEventArgs e)
        {
            FCS001_019_E_SPEC_CHANGE E_Spec_Change = new FCS001_019_E_SPEC_CHANGE();
            E_Spec_Change.FrameOperation = FrameOperation;

            string line_id = string.Empty;

            if (cboModel.GetBindValue() == null)
            {
                // 모델을 선택해주세요
                Util.MessageValidation("SFU1257");
                return;
            }

            if (cboLine.GetBindValue() == null)
            {
                //    // 라인을 선택해주세요.
                //    Util.MessageValidation("SFU1223");
                //    return;
                if (cboModel.GetBindValue() != null)
                {
                    line_id = GetLineByModel(Util.GetCondition(cboModel));
                }
                    
            }
            else
            {
                line_id = cboLine.GetBindValue().ToString();
            }

            

            if (E_Spec_Change != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = line_id; // Util.GetCondition(cboLine, bAllNull: true);
                Parameters[1] = Util.GetCondition(cboModel);


                C1WindowExtension.SetParameters(E_Spec_Change, Parameters);
                E_Spec_Change.Closed += new EventHandler(E_Spec_Change_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => E_Spec_Change.ShowModal()));
            }
        }


        private string GetLineByModel(string mdllot_id)
        {
            string LINE_ID = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MDLLOT_ID"] = mdllot_id;
                dtRqst.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_SEL_LINE_BY_MODEL", "RQSTDT", "RSLTDT", dtRqst);

                if (result.Rows.Count > 0)
                {
                    if (result.Rows.Count == 1)
                    {
                        LINE_ID = Util.NVC(result.Rows[0]["EQSGID"].ToString());

                        return LINE_ID;
                    }
                }


                //new ClientProxy().ExecuteService("DA_SEL_LINE_BY_MODEL", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                //{
                //    try
                //    {
                //        if (Exception != null)
                //        {
                //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //            return;
                //        }

                //        if (result.Rows.Count > 0)
                //        {
                //            if(result.Rows.Count == 1)
                //            {
                //                LINE_ID = Util.NVC(result.Rows[0]["EQSGID"].ToString());                                
                //            }                            

                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {
                //        //HiddenLoadingIndicator();
                //    }
                //});
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }

            return LINE_ID;

        }

    }
}
