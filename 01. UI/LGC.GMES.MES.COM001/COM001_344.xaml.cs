/*************************************************************************************
 Created Date : 2020.12.28
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - HOLD 재공 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.28  조영대 : Initial Created.
  2021.06.30  조영대 : Hold Lot List 팝업 연결
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_344 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public COM001_344()
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
                InitCombo();
                SetElecOption();

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;

            dgSummary.ClearRows();
        }

        #endregion

        #region Event
  
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetStock();
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProcess();

                    dgSummary.ClearRows();
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboProcess_SelectionChanged(object sender, EventArgs e)
        {
            if (Util.IsNVC(cboProcess.SelectedItemsToString))
            {
                cboProcess.CheckAll();
            }

            dgSummary.ClearRows();
        }

        private void cboElecType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            dgSummary.ClearRows();
        }

        private void dgSummary_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn col in dgSummary.Columns)
                    {
                        List<string> header = col.Header as List<string>;
                        if (header == null) continue;

                        if (Util.NVC(header[0]).Contains("[#] "))
                        {
                            header[0] = header[0].Replace("[#] ", string.Empty);
                        }
                        dgSummary.Refresh(refreshRowHeaders: true);
                    }

                    Style style = new Style(typeof(DataGridColumnHeaderPresenter));
                    style.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromArgb(200, 153, 255, 153)) });
                    //style.Setters.Add(new Setter { Property = ForegroundProperty, Value = Brushes.Blue });
                    style.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
                    style.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Center });
                    style.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });
                    style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, ObjectDic.Instance.GetObjectName("오늘")));
                    dgSummary.Columns["HOLD_LOT_CNT_DDAY"].HeaderStyle = style;
                    dgSummary.Columns["HOLD_LOT_QTY_DDAY"].HeaderStyle = style;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null || e.Cell.Row.Type.Equals(DataGridRowType.Top))
                {
                    return;
                }

                if (e.Cell.Column.Name.Contains("HOLD_LOT_CNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }

                if (e.Cell.Column.Name.Contains("_TOTAL"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 204));
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
            }));


        }

        private void dgSummary_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgSummary.CurrentColumn == null || dgSummary.CurrentRow == null ||
                dgSummary.CurrentRow.Index < 0) return;

            if (Util.NVC(dgSummary.GetValue(dgSummary.CurrentRow.Index, dgSummary.CurrentColumn.Name)).Equals(string.Empty) ||
                Util.NVC(dgSummary.GetValue(dgSummary.CurrentRow.Index, dgSummary.CurrentColumn.Name)).Equals("0")) return;

            if (dgSummary.CurrentColumn.Name.Contains("HOLD_LOT_CNT_"))
            {
                COM001_344_HOLD_LOT_LIST popHoldLotList = new COM001_344_HOLD_LOT_LIST();
                popHoldLotList.FrameOperation = FrameOperation;

                if (popHoldLotList != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = GetLotList(dgSummary.CurrentRow.Index, dgSummary.CurrentColumn.Index);
                    C1WindowExtension.SetParameters(popHoldLotList, Parameters);

                    popHoldLotList.ShowModal();
                }
            }
         
        }

        private void radHoldDate_Checked(object sender, RoutedEventArgs e)
        {
            if (chkActionPerson == null || txtSubTitle == null) return;

            if (chkActionPerson.IsChecked.Equals(true))
            {
                txtSubTitle.Text = Util.NVC(radHoldDate.Content) + " - " + Util.NVC(chkActionPerson.Content);
            }
            else
            {
                txtSubTitle.Text = Util.NVC(radHoldDate.Content);
            }

            if (dgSummary == null) return;

            dgSummary.Columns["HOLD_LOT_CNT_DB3"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_QTY_DB3"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_CNT_DB2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_QTY_DB2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_CNT_DB1"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_QTY_DB1"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_CNT_DDAY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_QTY_DDAY"].Visibility = Visibility.Collapsed;

            (dgSummary.Columns["HOLD_LOT_CNT_DA1"].Header as List<string>)[0] =
            (dgSummary.Columns["HOLD_LOT_QTY_DA1"].Header as List<string>)[0] = "D+0 ~ 7";

            dgSummary.Refresh(refreshColumnHeaders: true);

            dgSummary.ClearRows();
        }

        private void radHoldCheduleDate_Checked(object sender, RoutedEventArgs e)
        {
            if (chkActionPerson == null || txtSubTitle == null) return;

            if (chkActionPerson.IsChecked.Equals(true))
            {
                txtSubTitle.Text = Util.NVC(radHoldCheduleDate.Content) + " - " + Util.NVC(chkActionPerson.Content);
            }
            else
            {
                txtSubTitle.Text = Util.NVC(radHoldCheduleDate.Content);
            }

            if (dgSummary == null) return;

            dgSummary.Columns["HOLD_LOT_CNT_DB3"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_QTY_DB3"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_CNT_DB2"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_QTY_DB2"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_CNT_DB1"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_QTY_DB1"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_CNT_DDAY"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_QTY_DDAY"].Visibility = Visibility.Visible;

            (dgSummary.Columns["HOLD_LOT_CNT_DA1"].Header as List<string>)[0] =  
            (dgSummary.Columns["HOLD_LOT_QTY_DA1"].Header as List<string>)[0] = "D+1 ~ 7";

            dgSummary.Refresh(refreshColumnHeaders: true);

            dgSummary.ClearRows();
        }
        
        private void chkActionPerson_Checked(object sender, RoutedEventArgs e)
        {
            dgSummary.ClearRows();

            if (chkActionPerson == null || txtSubTitle == null) return;

            if (chkActionPerson.IsChecked.Equals(true))
            {
                if (radHoldCheduleDate.IsChecked.Equals(true))
                {
                    txtSubTitle.Text = Util.NVC(radHoldCheduleDate.Content) + " - " + Util.NVC(chkActionPerson.Content);
                }
                else
                {
                    txtSubTitle.Text = Util.NVC(radHoldDate.Content) + " - " + Util.NVC(chkActionPerson.Content);
                }
            }
            else
            {
                if (radHoldCheduleDate.IsChecked.Equals(true))
                {
                    txtSubTitle.Text = Util.NVC(radHoldCheduleDate.Content);
                }
                else
                {
                    txtSubTitle.Text = Util.NVC(radHoldDate.Content);
                }
            }
        }

        private void chkActionPerson_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSummary.ClearRows();

            if (chkActionPerson == null || txtSubTitle == null) return;

            if (chkActionPerson.IsChecked.Equals(true))
            {
                txtSubTitle.Text = Util.NVC(radHoldCheduleDate.Content) + " - " + Util.NVC(chkActionPerson.Content);
            }
            else
            {
                txtSubTitle.Text = Util.NVC(radHoldCheduleDate.Content);
            }
        }
        #endregion

        #region Method

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

        private void GetStock()
        {
            try
            {
                if (Util.NVC(cboProcess.SelectedItemsToString) == "")
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }

                if (!Util.IsNVC(txtModlId.Text) && txtModlId.Text.Length < 4)
                {
                    //[%1] 자리수 이상 입력하세요.
                    Util.MessageValidation("SFU4342", "3");

                    txtModlId.Focus();
                    txtModlId.SelectAll();
                    return;
                }

                if (!Util.IsNVC(txtProdId.Text) && txtProdId.Text.Length < 4)
                {
                    //[%1] 자리수 이상 입력하세요.
                    Util.MessageValidation("SFU4342", "3");

                    txtProdId.Focus();
                    txtProdId.SelectAll();
                    return;
                }

                if (chkActionPerson.IsChecked.Equals(true))
                {
                    dgSummary.Columns["ACTION_PERSON_NAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgSummary.Columns["ACTION_PERSON_NAME"].Visibility = Visibility.Collapsed;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("WORK_TYPE", typeof(string));
                dtRqst.Columns.Add("IN_PERSON", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                if (radHoldCheduleDate.IsChecked.Equals(true))
                {
                    dr["WORK_TYPE"] = "RELEASE"; // 해제일자 기준
                }
                if (radHoldDate.IsChecked.Equals(true))
                {
                    dr["WORK_TYPE"] = "HOLD"; // Hold 일자 기준
                }

                if (chkActionPerson.IsChecked.Equals(true))
                {
                    dr["IN_PERSON"] = "Y"; // 조치 담당자 포함
                }
                else
                {
                    dr["IN_PERSON"] = "N";
                }
                
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = cboProcess.SelectedItemsToString;
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
                dr["PRODID"] = Util.GetCondition(txtProdId);
                dr["MODLID"] = Util.GetCondition(txtModlId);
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_HOLD_WORK_STATUS_DRB", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
                {
                    if (ex != null || result == null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }
                    
                    dgSummary.SetItemsSource(result, FrameOperation, true);

                    foreach (C1.WPF.DataGrid.DataGridColumn col in dgSummary.Columns)
                    {
                        List<string> header = col.Header as List<string>;
                        if (header == null) continue;

                        if (Util.NVC(header[0]).Contains("[#] "))
                        {
                            header[0] = header[0].Replace("[#] ", string.Empty);
                        }
                        dgSummary.Refresh(refreshRowHeaders: true);
                    }

                    HiddenLoadingIndicator();
                });
                
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetLotList(int row, int  col)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("WORK_TYPE", typeof(string));
                dtRqst.Columns.Add("COLUMN_NAME", typeof(string));
                dtRqst.Columns.Add("ACTION_PERSON", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                if (radHoldCheduleDate.IsChecked.Equals(true))
                {
                    dr["WORK_TYPE"] = "RELEASE"; // 해제일자 기준
                }
                if (radHoldDate.IsChecked.Equals(true))
                {
                    dr["WORK_TYPE"] = "HOLD"; // Hold 일자 기준
                }

                dr["COLUMN_NAME"] = dgSummary.Columns[col].Name;

                if (chkActionPerson.IsChecked.Equals(true))
                {
                    dr["ACTION_PERSON"] = dgSummary.GetValue(row, "ACTION_PERSON");
                }

                dr["AREAID"] = cboArea.GetBindValue();
                dr["EQSGID"] = cboEquipmentSegment.GetBindValue();                
                dr["PRDT_CLSS_CODE"] = cboElecType.GetBindValue();
                dr["PRODID"] = dgSummary.GetValue(row, "PRODID");
                dr["PROCID"] = dgSummary.GetValue(row, "PROCID");
                dr["PRJT_NAME"] = dgSummary.GetValue(row, "PRJT_NAME");
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_WORK_STATUS_LOT_LIST_DRB", "RQSTDT", "RSLTDT", dtRqst);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return null;
        }

        private void SetElecOption()
        {

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                dtRslt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["S01"].ToString().Equals("E"))
                    {
                        tbElecType.Visibility = Visibility.Visible;
                        cboElecType.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tbElecType.Visibility = Visibility.Collapsed;
                        cboElecType.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SetcboProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                cboProcess.CheckAll();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion
    }
}
