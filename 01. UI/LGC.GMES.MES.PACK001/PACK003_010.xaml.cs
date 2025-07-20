/*************************************************************************************
 Created Date : 2021.01.09
      Creator : 김길용A
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author        CSR         Description...
  2021.01.09  DEVELOPER    SI            Initial Created.
  2021.03.04  김길용           SI            DA_MCS_SEL_MCS_CONFIG_INFO 데이터로 PACK BIZ 호출 되도록 수정, 창고ComboBox 수정
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Management;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_010 : UserControl, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        //비즈 Config
        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        private static string REQUEST = "APR";
        private static string CANCEL = "REJ";
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public PACK003_010()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            GetBizActorServerInfo();
        }

        #region 수동출고 비즈 CONFIG 설정
        private void GetBizActorServerInfo()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "TB_PACK2_TESTCONFIG";      // 팩2동운영 TB_PACK2_CONFIG 실전 TB_PACK2_TESTCONFIG
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }
        }
        #endregion


        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                tbChkListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜
                InitCombo();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();


            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = "G481";

            //C1ComboBox cboSLOC_TYPE_CODE = new C1ComboBox();
            //cboSLOC_TYPE_CODE.SelectedValue = "SLOC02,SLOC03";

            //C1ComboBox cboSHIP_TYPE_CODE = new C1ComboBox();
            //cboSHIP_TYPE_CODE.SelectedValue = Ship_Type.CELL;
            
            string[] cboSearchSLocTo_Parent = { cboSHOPID.SelectedValue.ToString(), null, null, "PACK", null };
            _combo.SetCombo(cboSLoc, CommonCombo.ComboStatus.ALL, sFilter: cboSearchSLocTo_Parent, sCase: "SLOC_BY_TOSLOC_PROC");

            //현상태 멀티콤보
            this.cboStat.isAllUsed = true;
            cboStat.ApplyTemplate();
            this.SetMultiSelectionBoxRequestStatus(this.cboStat);
        }

        private void SetMultiSelectionBoxRequestStatus(MultiSelectionBox cboMulti)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_LOGIS_TRF_REQ_STAT_CODE";
                dr["ATTRIBUTE2"] = "B";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows.Count == 1)
                    {
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboMulti.Uncheck(-1);
                    }
                    else
                    {
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);

                        for (int i = 0; i < dtResult.Rows.Count; ++i)
                        {
                            if ("REQUEST_LOGIS,CONFIRMED_LOGIS,REJECTED_LOGIS".Contains(dtResult.Rows[i]["CBO_CODE"].ToString()))
                            {
                                cboMulti.Check(i);
                            }
                            else
                            {
                                cboMulti.Uncheck(i);
                            }
                        }
                    }
                }
                else
                {
                    cboMulti.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgReturnCellList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        // 반품 승인 요청 또는 취소 Transaction Validation Check
        private bool ValidationCheck(string requestType)
        {
            bool returnValue = false;

            switch (requestType.ToUpper())
            {
                case "APR":
                    returnValue = this.ValidationCheckApprovalConfirm();
                    break;
                case "REJ":
                    returnValue = this.ValidationCheckApprovalCancel();
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        private void btnReturnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // 반품 승인 요청을 하시겠습니까?
            Util.MessageConfirm("SFU5101", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.ApprovalRequest();
                }
            });
        }
        
        
        private void btnReturnCencel_Click(object sender, RoutedEventArgs e)
        {
            // 반품거부 하시겠습니까?
            Util.MessageConfirm("반품거부 하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.ApprovalCancel();
                }
            }
            );
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgReturnCellList);
                txtNote.Text = "";
                if (!dtDateCompare())
                {
                    return;
                }
                Search();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion




        #region Mehod
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void dgReturnCellList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            {
                try
                {
                    C1DataGrid dataGrid = (sender as C1DataGrid);

                    Action act = () =>
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }
                        if (e.Cell.Column.Name.Equals("TRF_REQ_STAT_NAME"))
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TRF_REQ_STAT_CODE")) == "REQUEST_LOGIS")
                            {
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                                return;
                            }
                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TRF_REQ_STAT_CODE")) == "CONFIRMED_LOGIS")
                            {
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Green);
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                                return;
                            }
                            if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TRF_REQ_STAT_CODE")) == "REJECTED_LOGIS")
                            {
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                dgReturnCellList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                                return;
                            }
                            
                        }
                    };

                    dataGrid.Dispatcher.BeginInvoke(act);
                }
                catch
                {
                    HiddenLoadingIndicator();

                }
            }
        }
        
        // 반품 승인 요청 Transaction
        private void ApprovalRequest()
        {
            try
            {
                if (!this.ValidationCheck(REQUEST))
                {
                    return;
                }

                if (this.ApprovalTransaction(REQUEST))
                {
                    this.ClearApprovalData();
                    this.Search();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ApprovalCancel()
        {
            try
            {
                if (!this.ValidationCheck(CANCEL))
                {
                    return;
                }

                if (this.ApprovalTransaction(CANCEL))
                {
                    this.Search();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool ApprovalTransaction(string requestType)
        {
            // Declarations...
            bool returnValue = false;
            DataSet ds = new DataSet();
            ds.Tables.Add(this.SetApprovalMasterData(requestType));
            ds.Tables.Add(this.SetApprovalDetailData(requestType));

            string inDataTableNameList = string.Join(",", ds.Tables.OfType<DataTable>().Select(dt => dt.TableName).ToList());
            //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_LOGIS_APPR_REQUEST", inDataTableNameList, string.Empty, ds, null);
            DataSet dsResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync_Multi("BR_PRD_REG_LOGIS_APPR_REQUEST", inDataTableNameList, null, ds);
            if (dsResult != null)
            {
                switch (requestType)
                {
                    case "REQ":
                        Util.MessageInfo("SFU1747");        // 요청되었습니다.
                        break;
                    case "CANCEL":
                        Util.MessageInfo("SFU5032");        // 취소되었습니다.
                        break;
                    default:
                        break;
                }

                returnValue = true;
            }

            return returnValue;
        }


        // 반품 승인 요청 또는 취소 입력 데이터 만들기 1호
        private DataTable SetApprovalMasterData(string requestType)
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "INDATA";
            dt.Columns.Add("SRCTYPE", typeof(string));
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("RESNCODE", typeof(string));
            dt.Columns.Add("REQ_NOTE", typeof(string));
            dt.Columns.Add("INSUSER", typeof(string));
            dt.Columns.Add("REQTYPE", typeof(string));
            dt.Columns.Add("UPDUSER", typeof(string));

            // Insert Data
            if (requestType.Equals(REQUEST))
            {
                DataTable dtRequestData = DataTableConverter.Convert(this.dgReturnCellList.ItemsSource);
                var query = from d1 in dtRequestData.AsEnumerable()
                            where d1.Field<string>("CHK").ToUpper().Equals("TRUE") || d1.Field<string>("CHK").ToUpper().Equals("1")
                            group d1 by 1 into grp
                            select new
                            {
                                //TRF_LOT_QTY = grp.Max(x => x.Field<string>("PLT_LOT_QTY"))
                            };

                foreach (var item in query)
                {
                    DataRow dr = dt.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["PRODID"] = string.Empty;
                    dr["RESNCODE"] = string.Empty;
                    dr["REQ_NOTE"] = this.txtNote.Text;
                    dr["INSUSER"] = LoginInfo.USERID;
                    dr["REQTYPE"] = requestType;
                    dr["UPDUSER"] = this.ucPersonInfo.UserID;
                    dt.Rows.Add(dr);
                }
            }
            else if (requestType.Equals(CANCEL))
            {
                DataRow dr = dt.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = string.Empty;
                dr["PRODID"] = string.Empty;
                dr["RESNCODE"] = string.Empty;
                dr["REQ_NOTE"] = string.Empty;
                dr["INSUSER"] = LoginInfo.USERID;
                dr["REQTYPE"] = requestType;
                dr["UPDUSER"] = this.ucPersonInfo.UserID;
                dt.Rows.Add(dr);
            }

            dt.AcceptChanges();     // Apply
            return dt;
        }

        // 반품 승인 요청 또는 취소 입력 데이터 만들기 2호
        private DataTable SetApprovalDetailData(string requestType)
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "INCST";
            dt.Columns.Add("TRF_REQ_NO", typeof(string));
            dt.Columns.Add("CSTID", typeof(string));
            dt.Columns.Add("PLTID", typeof(string));
            dt.Columns.Add("PLT_LOT_QTY", typeof(string));

            // Insert Data
            if (requestType.Equals(REQUEST))
            {
                DataTable dtData = DataTableConverter.Convert(this.dgReturnCellList.ItemsSource);
                foreach (DataRow drData in dtData.Select())
                {
                    if (drData["CHK"].ToString().Equals("True"))
                    {
                        DataRow dr = dt.NewRow();
                        dr["TRF_REQ_NO"] = drData["TRF_REQ_NO"];
                        dr["CSTID"] = drData["CSTID"];
                        dr["PLTID"] = drData["PLLT_ID"];
                        dr["PLT_LOT_QTY"] = drData["PLLT_LOT_QTY"];
                        dt.Rows.Add(dr);
                    }
                }
            }
            else if (requestType.Equals(CANCEL))
            {
                DataTable dtData = DataTableConverter.Convert(this.dgReturnCellList.ItemsSource);
                foreach (DataRow drData in dtData.Select())
                {
                    if (drData["CHK"].ToString().Equals("True"))
                    {
                        DataRow dr = dt.NewRow();
                        dr["TRF_REQ_NO"] = drData["TRF_REQ_NO"];
                        dr["CSTID"] = drData["CSTID"];
                        dr["PLTID"] = drData["PLLT_ID"];
                        dr["PLT_LOT_QTY"] = drData["PLLT_LOT_QTY"];
                        dt.Rows.Add(dr);
                    }
                }
            }
            dt.AcceptChanges();     // Apply
            return dt;
        }

        private bool ValidationCheckApprovalConfirm()
        {
            if (string.IsNullOrEmpty(this.txtNote.Text))
            {
                Util.MessageValidation("SFU1554");  // 결제사유를 입력하세요
                this.txtNote.Focus();
                return false;
            }

            DataTable dt = DataTableConverter.Convert(this.dgReturnCellList.ItemsSource);

            var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU1654");  // 선택된 요청이 없습니다.
                return false;
            }

            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); //작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return false;
            }

            return true;
        }

        // 반품 승인 취소 Validation
        private bool ValidationCheckApprovalCancel()
        {
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591"); //작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return false;
            }

            DataTable dt = DataTableConverter.Convert(this.dgReturnCellList.ItemsSource);
            var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));

            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU1654");  // 선택된 요청이 없습니다.
                return false;
            }

            return true;
        }
        private void ClearApprovalData()
        {
            this.txtPltid.Text = string.Empty;
            this.txtNote.Text = string.Empty;
            Util.gridClear(this.dgReturnCellList);
            // 건수표시
            this.tbChkListCount.Text = "[ 0 건 ]";
        }
        private Boolean dtDateCompare()
        {
            try
            {
                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return false;
                }

                //if (timeSpan.Days > 30)
                //{
                //    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                //    Util.MessageValidation("SFU4466");
                //    return false;
                //
                //}

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void Search()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_LOGIS_APPR_REQ_LIST";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("REQ_FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("REQ_TO_DATE", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));
                RQSTDT.Columns.Add("TRF_REQ_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("FROM_SLOC_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["REQ_FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["REQ_TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["CSTID"] = txtPltid.Text.ToString() == "" ? null : txtPltid.Text.ToString();
                dr["TRF_REQ_STAT_CODE"] = Convert.ToString(this.cboStat.SelectedItemsToString);
                dr["FROM_SLOC_ID"] = Util.GetCondition(cboSLoc) == "" ? null : Util.GetCondition(cboSLoc);


                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                //new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_APPR_REQ_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgReturnCellList, dtResult, FrameOperation);

                        Util.SetTextBlockText_DataGridRowCount(tbChkListCount, Util.NVC(dtResult.Rows.Count));
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            this.SelectSameApprNo(sender);
        }
        // 같은 반품승인번호를 가지고 있는 모든 Row Select
        private void SelectSameApprNo(object sender)
        {
            if (sender == null)
            {
                return;
            }

            try
            {
                CheckBox checkBox = sender as CheckBox;
                DataTable dt = DataTableConverter.Convert(this.dgReturnCellList.ItemsSource);
                string selectedApprReqNo = DataTableConverter.GetValue(checkBox.DataContext, "TRF_REQ_NO").ToString();
                var query = dt.AsEnumerable().Cast<DataRow>().Select((x, i) => new { ROW_NUMBER = i++, TRF_REQ_NO = x.Field<string>("TRF_REQ_NO") }).Where(x => x.TRF_REQ_NO.Equals(selectedApprReqNo));
                foreach (var item in query)
                {
                    DataTableConverter.SetValue(this.dgReturnCellList.Rows[item.ROW_NUMBER].DataItem, "CHK", checkBox.IsChecked);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }





        #endregion

    }
}