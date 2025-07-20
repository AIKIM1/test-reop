/*************************************************************************************
 Created Date : 2020.08.11
      Creator : 최우석
   Decription : 소포장LOT 재공조회(Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.00  담당자 : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK002_002 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        DateTime dtFromDate;
        DateTime dtToDate;

        string sCrrWipEqsgid = null;
        string sCrrWipPortGpId = null;
        string sCrrWipMtrlId = null;
        string sCrrWipStat = null;
        string sCrrWipHold = null;
        bool bCrrStat = false;

        string sCrrTrmEqsgid = null;
        string sCrrTrmPortGpId = null;
        string sCrrTrmMtrlId = null;
        string sCrrTrmHold = null;
        bool bTrmStat = false;

        public PACK002_002()
        {
            InitializeComponent();


        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = FrameOperation.Parameters;
                if (tmps != null)
                {
                    if (tmps.Length > 0)
                    {
                        //string sLotid = tmps[0].ToString();
                        //txtSearchLotId.Text = sLotid;
                        //SetInfomation(txtSearchLotId.Text.Trim());
                    }
                }

                InitCombo();

                dtpTmDateFrom.IsNullInitValue = true;
                dtpTmDateTo.IsNullInitValue = true;

                dtpTmDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpTmDateTo.SelectedDateTime = DateTime.Now;

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);

                dtpTmDateFrom.UpdateLayout();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEqptLine01_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] cboMtrlGroupFilterParam = { "EQSGID" };

            C1ComboBox[] cboMtrlGroup01Parent = { cboEqptLine01 };

            SetMtrlGroupCode(cboMtrlGroup01, CommonCombo.ComboStatus.ALL, cbParent: cboMtrlGroup01Parent, sFilter: cboMtrlGroupFilterParam);


        }

        private void cboMtrlGroup01_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            CommonCombo _combo = new CMM001.Class.CommonCombo();

            string[] cboMtrlCodeFilterParam = { "MATGRPCODE" };

            C1ComboBox[] cboMtrlCode01Parent = { cboMtrlGroup01 };

            #region 재공현황조회
            SetMtrlCode(cbo: cboMtrlCode01, cs: CommonCombo.ComboStatus.ALL, cbParent: cboMtrlCode01Parent, sFilter: cboMtrlCodeFilterParam);
            #endregion  재공현황조회

        }

        private void chkCurrentDate_Checked(object sender, RoutedEventArgs e)
        {
            if(chkCurrentDate.IsChecked == true)
            {
                SetTermDateTime();

                dtpTmDateFrom.IsEnabled = false;
                dtpTmDateTo.IsEnabled = false;
                dtpTmDateFrom.IsReanOnly = true;
                dtpTmDateTo.IsReanOnly = true;
            }
            else
            {
                dtpTmDateFrom.IsEnabled = true;
                dtpTmDateTo.IsEnabled = true;
                dtpTmDateFrom.IsReanOnly = false;
                dtpTmDateTo.IsReanOnly = false;
            }
        }

        private void cboEqptLine02_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] cboMtrlGroupFilterParam = { "EQSGID" };

            C1ComboBox[] cboMtrlGroup02Parent = { cboEqptLine02 };


            SetMtrlGroupCode(cboMtrlGroup02, CommonCombo.ComboStatus.ALL, cbParent: cboMtrlGroup02Parent, sFilter: cboMtrlGroupFilterParam);
        }

        private void btnSearch01_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (Convert.ToString(cboEqptLine01.SelectedValue).Equals("SELECT"))
            {
                //Util.MessageValidation("SFU3206");//동을 선택해주세요
                ms.AlertWarning("SFU3206"); //동을 선택해주세요
                return;
            }

            try
            {
                CurrentMtrlList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CurrentMtrlList()
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PORT_MTRL_GR_CODE", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));

                DataRow dr = dtINDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Convert.ToString(cboEqptLine01.SelectedValue).Equals("") ? null : Convert.ToString(cboEqptLine01.SelectedValue);
                dr["PORT_MTRL_GR_CODE"] = Convert.ToString(cboMtrlGroup01.SelectedValue).Equals("") ? null : Convert.ToString(cboMtrlGroup01.SelectedValue);
                dr["MTRLID"] = Convert.ToString(cboMtrlCode01.SelectedValue).Equals("") ? null : Convert.ToString(cboMtrlCode01.SelectedValue);
                dtINDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_SBOX_WIPST_UI", "INDATA", "OUTDATA", dtINDATA);

                Util.gridClear(dgCurrentMtrlList);
                Util.gridClear(dgCurrentMtrlDetail);

                //txtBkResultCnt01.Text = "" + dtResult.Rows.Count + "";

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgCurrentMtrlList, dtResult, FrameOperation, false);
                }

                Util.SetTextBlockText_DataGridRowCount(txtBkCarrierInforCnt01, Util.NVC(dgCurrentMtrlDetail.Rows.Count));
                Util.SetTextBlockText_DataGridRowCount(txtBkResultCnt01, Util.NVC(dgCurrentMtrlList.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (e.Cell.Presenter == null) return;
                if (e.Cell.Row.Type != DataGridRowType.Item) return;

                if (dgCurrentMtrlList == null)
                    return;

                switch(e.Cell.Column.Name)
                {
                    case "WAIT_CNT":
                    case "PROC_CNT":
                    case "END_CNT":
                    case "HOLD_CNT":
                    case "TERM_CNT":
                    case "S_BOX_ID":
                        {
                            if (e.Cell.Text.Equals("N/A"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                        }
                        break;
                    default:
                        {
                            if (e.Cell.Text.Equals("N/A"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                        break;

                }

               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCurrentMtrlList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgCurrentMtrlList == null || dgCurrentMtrlList.Rows.Count == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCurrentMtrlList.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int currentCol = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgCurrentMtrlList.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                string strEQSGID = DataTableConverter.GetValue(dgCurrentMtrlList.Rows[currentRow].DataItem, "EQSGID").ToString();
                string strPORT_MTRL_GR_CODE = DataTableConverter.GetValue(dgCurrentMtrlList.Rows[currentRow].DataItem, "MTRL_GR_CODE").ToString();
                string strMTRLID = DataTableConverter.GetValue(dgCurrentMtrlList.Rows[currentRow].DataItem, "MTRLCODE").ToString();
                string strS_BOX_STAT = null;// string.Empty;
                string strHoldFlag = null;

                switch (currentCol)
                {
                    case 6:
                        strS_BOX_STAT = "WAIT";
                        break;
                    case 7:
                        strS_BOX_STAT = "PROC";
                        break;
                    case 8:
                        strS_BOX_STAT = "END";
                        break;
                    case 9:
                        strHoldFlag = "Y";
                        break;
                    default:
                        return;
                        //break;

                }

                CurrentMtrlDetail(sCrrWipEqsgid = strEQSGID, sCrrWipPortGpId = strPORT_MTRL_GR_CODE, strMTRLID, strS_BOX_STAT, strHoldFlag);

                bCrrStat = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CurrentMtrlDetail(string sEqsgid, string sPortGp, string sMtrlid, string sStat, string hold)
        {
            try
            {
                sCrrWipEqsgid = sEqsgid ?? sCrrWipEqsgid;
                sCrrWipPortGpId = sPortGp ?? sCrrWipPortGpId;
                sCrrWipMtrlId = sMtrlid ?? sCrrWipMtrlId;

                if (string.IsNullOrWhiteSpace(hold))
                {
                    sCrrWipStat = sStat ?? sCrrWipStat;
                    sCrrWipHold = null;
                }
                else
                {
                    sCrrWipStat = null;
                    sCrrWipHold = "Y";
                }

                DataTable dtINDATA = new DataTable();

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PORT_MTRL_GR_CODE", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));
                dtINDATA.Columns.Add("S_BOX_STAT", typeof(string));
                dtINDATA.Columns.Add("HOLD_FLAG", typeof(string));

                DataRow searchCondition = dtINDATA.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["EQSGID"] = sCrrWipEqsgid;
                searchCondition["PORT_MTRL_GR_CODE"] = sCrrWipPortGpId;
                searchCondition["MTRLID"] = sCrrWipMtrlId;
                searchCondition["S_BOX_STAT"] = sCrrWipStat;
                searchCondition["HOLD_FLAG"] = sCrrWipHold;

                dtINDATA.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_ID_UI", "INDATA", "OUTDATA", dtINDATA);

                //txtBkCarrierInforCnt01.Text = "" + dtResult.Rows.Count + "";

                Util.gridClear(dgCurrentMtrlDetail);

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgCurrentMtrlDetail, dtResult, FrameOperation, false);
                }

                Util.SetTextBlockText_DataGridRowCount(txtBkCarrierInforCnt01, Util.NVC(dgCurrentMtrlDetail.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

        }



        private void dgCurrentMtrlDetail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgCurrentMtrlDetail == null || dgCurrentMtrlDetail.Rows.Count == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCurrentMtrlDetail.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int currentCol = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgCurrentMtrlDetail.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                string strSBoxId = string.Empty;

                switch (cell.Column.Name)
                {
                    case "S_BOX_ID":
                        {
                            this.FrameOperation.OpenMenu("SFU10189001", true, value);
                        }
                        break;
                    default:
                        break;

                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

      

        private void btnSearch02_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if (DateTime.Parse(dtpTmDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"))
                    > DateTime.Parse(dtpTmDateTo.SelectedDateTime.ToString("yyyy-MM-dd")))
            {
                Util.Alert("SFU1517");  //등록 시작일이 종료일보다 큽니다.
                dtpTmDateFrom.Focus();
                return ;
            }

            if (Convert.ToString(cboEqptLine02.SelectedValue).Equals("SELECT"))
            {
                //Util.MessageValidation("SFU3206");//동을 선택해주세요
                ms.AlertWarning("SFU3206");//동을 선택해주세요
                return;
            }

            try
            {
                TermMtrlList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TermMtrlList()
        {
            try
            {
                dtFromDate = dtpTmDateFrom.SelectedDateTime;
                dtToDate = dtpTmDateTo.SelectedDateTime;

                DataTable dtINDATA = new DataTable();
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("MTRL_GR_CODE", typeof(string));
                dtINDATA.Columns.Add("FROM_DT", typeof(DateTime));
                dtINDATA.Columns.Add("TO_DT", typeof(DateTime));

                DataRow dr = dtINDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Convert.ToString(cboEqptLine02.SelectedValue).Equals("") ? null : Convert.ToString(cboEqptLine02.SelectedValue);
                dr["MTRL_GR_CODE"] = Convert.ToString(cboMtrlGroup02.SelectedValue).Equals("") ? null : Convert.ToString(cboMtrlGroup02.SelectedValue);
                dr["FROM_DT"] = dtFromDate;
                dr["TO_DT"] = dtToDate;
                dtINDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_SBOX_TERMS_UI", "INDATA", "OUTDATA", dtINDATA);

                //txtBkResultCnt02.Text = "" + dtResult.Rows.Count + "";
                Util.gridClear(dgTermMtrl);
                Util.gridClear(dgTermMtrlDetail);

                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgTermMtrl, dtResult, FrameOperation, false);
                }
                Util.SetTextBlockText_DataGridRowCount(txtBkResultCnt02, Util.NVC(dgTermMtrl.Rows.Count));
                Util.SetTextBlockText_DataGridRowCount(txtBkCarrierInforCnt02, Util.NVC(dgTermMtrlDetail.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgTermMtrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTermMtrl == null || dgTermMtrl.Rows.Count == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTermMtrl.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null /*|| cell.Value.ToString() == "0"*/)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int currentCol = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgTermMtrl.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                string strEQSGID = DataTableConverter.GetValue(dgTermMtrl.Rows[currentRow].DataItem, "EQSGID").ToString();
                string strPORT_MTRL_GR_CODE = DataTableConverter.GetValue(dgTermMtrl.Rows[currentRow].DataItem, "MTRL_GR_CODE").ToString();
                string strMTRLID = DataTableConverter.GetValue(dgTermMtrl.Rows[currentRow].DataItem, "MTRLCODE").ToString();
                string strS_BOX_STAT = null;
                string strHoldFlag = null;

                switch (currentCol)
                {
                    case 6:
                        strS_BOX_STAT = "TERM";
                        break;
                    case 7:
                        strS_BOX_STAT = "TERM";
                        strHoldFlag = "Y";
                        break;
                    default:
                        return;
                        //break;

                }

                TermMtrlDetail(sCrrTrmEqsgid = strEQSGID, sCrrTrmPortGpId = strPORT_MTRL_GR_CODE, strMTRLID, strHoldFlag);

                bTrmStat = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TermMtrlDetail(string sEqsgid, string sPortGp, string sMtrlid, string hold)
        {
            try
            {
                sCrrTrmEqsgid = sEqsgid ?? sCrrTrmEqsgid;
                sCrrTrmPortGpId = sPortGp ?? sCrrTrmPortGpId;
                sCrrTrmMtrlId = sMtrlid ?? sCrrTrmMtrlId;

                if (string.IsNullOrWhiteSpace(hold))
                {
                    sCrrTrmHold = null;
                }
                else
                {
                    sCrrTrmHold = "Y";
                }

                DataTable dtINDATA = new DataTable();

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("MTRL_GR_CODE", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));
                dtINDATA.Columns.Add("FROM_DT", typeof(DateTime));
                dtINDATA.Columns.Add("TO_DT", typeof(DateTime));
                dtINDATA.Columns.Add("HOLD_FLAG", typeof(string));

                DataRow searchCondition = dtINDATA.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["EQSGID"] = sCrrTrmEqsgid;
                searchCondition["MTRL_GR_CODE"] = sCrrTrmPortGpId;
                searchCondition["MTRLID"] = sCrrTrmMtrlId;
                searchCondition["FROM_DT"] = dtFromDate;
                searchCondition["TO_DT"] = dtToDate;
                searchCondition["HOLD_FLAG"] = sCrrTrmHold;

                dtINDATA.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_S_BOX_ID_TERM_UI", "INDATA", "OUTDATA", dtINDATA);

                // txtBkCarrierInforCnt02.Text = "" + dtResult.Rows.Count + "";
                Util.gridClear(dgTermMtrlDetail);

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgTermMtrlDetail, dtResult, FrameOperation, false);
                }

                Util.SetTextBlockText_DataGridRowCount(txtBkCarrierInforCnt02, Util.NVC(dgTermMtrlDetail.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgTermMtrlDetail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTermMtrlDetail == null || dgTermMtrlDetail.Rows.Count == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTermMtrlDetail.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int currentCol = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgTermMtrlDetail.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                switch (cell.Column.Name)
                {
                    case "S_BOX_ID":
                        {
                            this.FrameOperation.OpenMenu("SFU10189001", true, value);
                        }
                        break;
                    default:
                        break;

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region InitCombo
        private void InitCombo()
        {
            CommonCombo _combo = new CMM001.Class.CommonCombo();

            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            LoginInfo.CFG_AREA_ID = "PA";
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;

            C1ComboBox cboLangSet = new C1ComboBox();
            cboLangSet.SelectedValue = LoginInfo.LANGID;

            string[] cboMtrlGroupFilterParam = {  "EQSGID" };
            string[] cboMtrlCodeFilterParam = { "MATGRPCODE" };

            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType };
            C1ComboBox[] cboMtrlGroup01Parent = { cboEqptLine01 };
            C1ComboBox[] cboMtrlGroup02Parent = { cboEqptLine02 };
            C1ComboBox[] cboMtrlCode01Parent = { cboMtrlGroup01 };
            
            #region 재공현황조회
            _combo.SetCombo(cboEqptLine01, CommonCombo.ComboStatus.SELECT,cbParent: cboEquipmentSegmentParent,  sCase: "EQUIPMENTSEGMENT");
            SetMtrlGroupCode(cboMtrlGroup01, CommonCombo.ComboStatus.ALL, cbParent: cboMtrlGroup01Parent, sFilter: cboMtrlGroupFilterParam);
            SetMtrlCode(cbo:cboMtrlCode01,cs: CommonCombo.ComboStatus.ALL, cbParent: cboMtrlCode01Parent, sFilter: cboMtrlCodeFilterParam);
            #endregion  재공현황조회

            #region 종료재공조회
            _combo.SetCombo(cboEqptLine02, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");
            SetMtrlGroupCode(cboMtrlGroup02, CommonCombo.ComboStatus.ALL, cbParent: cboMtrlGroup02Parent, sFilter: cboMtrlGroupFilterParam);
            #endregion  종료재공조회

            cboEqptLine01.SelectedValue = LoginInfo.CFG_EQSG_ID;
            cboEqptLine02.SelectedValue = LoginInfo.CFG_EQSG_ID;

        }
        #endregion InitCombo

        private void SetMtrlGroupCode(C1ComboBox cbo, CommonCombo.ComboStatus cs, C1ComboBox[] cbParent = null, string[] sFilter = null)
        {
            try
            {
                if (cbParent != null)
                {
                    C1ComboBox[] cbArray = cbParent;
                    int i = 0;
                    for (i = 0; i < cbArray.Length; i++)
                    {
                        if (cbArray[i].SelectedValue != null)
                        {
                            sFilter[i] = cbArray[i].SelectedValue.ToString();
                        }

                    }
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_GR_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }


        private void SetMtrlCode(C1ComboBox cbo, CommonCombo.ComboStatus cs, C1ComboBox[] cbParent = null, string[] sFilter = null)
        {
            try
            {
                if (cbParent != null)
                {
                    C1ComboBox[] cbArray = cbParent;
                    int i = 0;
                    for (i = 0; i < cbArray.Length; i++)
                    {
                        if (cbArray[i].SelectedValue != null)
                        {
                            sFilter[i] = cbArray[i].SelectedValue.ToString();
                        }
                    }
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MATGRPCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MATGRPCODE"] = string.IsNullOrWhiteSpace( sFilter[0].Trim()) ? null : sFilter[0].Trim();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_MAT_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }


        private void SetTermDateTime()
        {
            dtpTmDateFrom.SelectedDateTime = DateTime.Today;
            dtpTmDateTo.SelectedDateTime = DateTime.Today;

        }


        #endregion  Mehod

        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex == 0)
            {
                CurrentMtrlList();
                if (dgCurrentMtrlList.Rows.Count() > 0 && bCrrStat)
                    CurrentMtrlDetail(null, null, null, null, null);
            }
            else
            {
                TermMtrlList();
                if (dgTermMtrl.Rows.Count() > 0 && bTrmStat)
                    TermMtrlDetail(null, null, null, null);

            }
        }
    }
}
