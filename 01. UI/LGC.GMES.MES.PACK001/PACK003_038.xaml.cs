/*************************************************************************************
 Created Date : 2022.12.14
      Creator : 김우련
   Decription : 자재 반품/반품취소 처리 및 반품 이력 조회 화면
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2022.12.14      김우련                             Initial Created.
  2023.01.10      이태규                             기능추가 : 반품사유 수정.
  2023.01.19      이태규                             기능추가 : WAIT 상태 추가.
  2023.01.26      김선준                             기능추가 : 반품사유(Others)History 추가
***************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_038 : UserControl, IWorkArea
    {
        #region #1. Member Variable Lists...
        private PACK003_038_DataHelper dataHelper = new PACK003_038_DataHelper();
        private const string ERROR_BOX_USING = "ERROR_BOX_USING";

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();

        int cboRtnStateCount = 0;
        int cboLineCount = 0;
        int cboMtrlRackCount = 0;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion #1. 

        #region #2. Declaration & Constructor
        public PACK003_038()
        {
            InitializeComponent();
        }
        #endregion #2. Declaration & Constructor

        #region #3. UserControl_Loaded
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                
                #region #. tp1
                this.txtBoxID.Clear();
                Util.gridClear(grdMainTp1);
                Util.DataGridCheckAllUnChecked(grdMainTp1);
                
                DataTable dt = this.dataHelper.GetStateData("PACK_RACK_MTRL_BOX_STCK_RTN_REASON_CODE");

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["CBO_NAME"] = Convert.ToString(dt.Rows[i]["ATTRIBUTE1"]) + " : " + Convert.ToString(dt.Rows[i]["CBO_NAME"]);

                PackCommon.SetC1ComboBox(dt, this.cboRtnReason, string.Empty);

                txtBoxID.Focus();
                #endregion #. tp1

                #region #. tp2
                DateTime firstOfDay = DateTime.Now.AddDays(-7);
                DateTime endOfDay = DateTime.Now;

                dtpFr.IsNullInitValue = true;
                dtpTo.IsNullInitValue = true;

                dtpFr.SelectedDateTime = firstOfDay;
                dtpTo.SelectedDateTime = endOfDay;

                this.txtBoxID_Detl.Clear();
                this.txtMtrlID.Clear();
                
                PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetStateData("PACK_RACK_MTRL_BOX_STCK_RTN_STAT_CODE"), this.cboRtnState, ref this.cboRtnStateCount);

                //SetComboBox(this.cboLine);
                PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetLineData(), this.cboLine, ref this.cboLineCount, LoginInfo.CFG_EQSG_ID);
                Util.gridClear(grdMainTp2);
                #endregion #. tp2

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion #3. UserControl_Loaded

        #region #4. tp1 처리 내용
        #region #4-1. Event_TAB1
        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sBoxID = txtBoxID.Text.Trim();
                if (string.IsNullOrWhiteSpace(sBoxID))
                {
                    //SFU2060 : 스캔한 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtBoxID.Focus();
                    });
                }
                else
                {
                    if (grdMainTp1.Rows.Count > 0)
                    {
                        DataTable dt = ((DataView)grdMainTp1.ItemsSource).Table;

                        string sTempBoxID = txtBoxID.Text.ToString();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i]["REPACK_BOX_ID"].ToString() == sTempBoxID || dt.Rows[i]["KANBAN_ID"].ToString() == sTempBoxID)
                            {
                                //SFU2882 : %1은(는) 이미 스캔한 값 입니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2882", new object[] { sTempBoxID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    this.txtBoxID.Clear();
                                    this.txtBoxID.Focus();
                                });
                                return;
                            }
                        }
                    }

                    AddGridgrdMainTp1(sBoxID);
                }
            }
        }

        private void btnDelRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (util.GetDataGridCheckCnt(grdMainTp1, "CHK") == 0)
                {
                    txtBoxID.Focus();
                    return;
                }

                if (!DeleteNoteValidation())
                {
                    txtBoxID.Focus();
                    return;
                }

                //SFU1230 : 삭제하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DeleteNote();
                                txtBoxID.Focus();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            if (this.grdMainTp1.Rows.Count != 0)
            {
                //SFU1815 : 입력한 데이터가 삭제됩니다. 계속 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1815"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
                {
                    if (Result == MessageBoxResult.OK)
                    {
                        txtBoxID.Focus();
                        this.txtBoxID.Clear();
                        Util.gridClear(grdMainTp1);
                        Util.DataGridCheckAllUnChecked(grdMainTp1);
                        txtBoxID.Focus();
                    }
                });
            }
            else
            {
                txtBoxID.Focus();
            }
        }

        private void btnRtnReq_Click(object sender, RoutedEventArgs e)
        {
            string sRtnRsnCode = this.cboRtnReason.SelectedValue.ToString();
            string sRtnRsnNote = this.txtReason.Text;

            DataTable dtData = DataTableConverter.Convert(grdMainTp1.ItemsSource);

            if (dtData.Rows.Count == 0 || util.GetDataGridCheckCnt(grdMainTp1, "CHK") == 0)
            {
                //SFU1651 : 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                txtBoxID.Focus();
                return;
            }

            if (sRtnRsnCode.Contains("OTHERS"))
            {
                if (string.IsNullOrWhiteSpace(sRtnRsnNote))
                {
                    //SFU1554 : 반품사유를 입력하세요
                    Util.MessageValidation("SFU1554");

                    txtBoxID.Focus();
                    return;
                }
            }

            for (int iChk = 0; iChk < dtData.Rows.Count; iChk++)
            {
                if (string.IsNullOrWhiteSpace(dtData.Rows[iChk]["RTN_QTY"].ToString()))
                {
                    //SFU3065 : 수량을 정수로 입력 하세요.
                    Util.MessageInfo("SFU3065");
                    return;
                }

                int iRtn_qty = Convert.ToInt32(dtData.Rows[iChk]["RTN_QTY"].ToString());
                if (iRtn_qty < 1)
                {
                    //SFU3064 : 수량은 0보다 큰 정수로 입력 하세요.
                    Util.MessageInfo("SFU3064");
                    return;
                }
                
                int iChk_qty = Convert.ToInt32(dtData.Rows[iChk]["ISS_QTY"].ToString());
                if (iRtn_qty > iChk_qty)
                {
                    //SFU1500 : 수량을 초과하였습니다
                    Util.MessageInfo("SFU1500");
                    return;
                }
            }

            //SFU2074 : 반품 처리 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2074"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
            {
                if (Result == MessageBoxResult.OK)
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;

                    try
                    {
                        DataSet indataSet = new DataSet();

                        DataTable INDATA = indataSet.Tables.Add("INDATA");
                        INDATA.Columns.Add("LANGID", typeof(string));
                        INDATA.Columns.Add("SRCTYPE", typeof(string));
                        INDATA.Columns.Add("UPDUSER", typeof(string));
                        INDATA.Columns.Add("RTN_RSN_CODE", typeof(string));
                        INDATA.Columns.Add("RTN_RSN_NOTE", typeof(string));

                        DataRow dr = INDATA.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["RTN_RSN_CODE"] = sRtnRsnCode;
                        dr["RTN_RSN_NOTE"] = sRtnRsnNote;
                        INDATA.Rows.Add(dr);

                        DataTable IN_RTN = indataSet.Tables.Add("IN_RTN");
                        IN_RTN.Columns.Add("REQ_NO", typeof(string));
                        IN_RTN.Columns.Add("REPACK_BOX_ID", typeof(string));
                        IN_RTN.Columns.Add("RTN_QTY", typeof(string));
                        
                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {
                            if (dtData.Rows[i]["CHK"].Equals("True"))
                            {
                                DataRow drBox = IN_RTN.NewRow();
                                drBox["REQ_NO"] = dtData.Rows[i]["REQ_NO"];
                                drBox["REPACK_BOX_ID"] = dtData.Rows[i]["REPACK_BOX_ID"];
                                drBox["RTN_QTY"] = dtData.Rows[i]["RTN_QTY"];
                                IN_RTN.Rows.Add(drBox);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_RETURN_RACK_MTRL_BOX_STCK", "INDATA,IN_RTN", "OUTDATA", indataSet);

                        ms.AlertInfo("PSS9072");  // 처리가 완료되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.txtBoxID.Clear();
                        this.txtReason.Clear();
                        Util.gridClear(grdMainTp1);
                        Util.DataGridCheckAllUnChecked(grdMainTp1);
                        txtBoxID.Focus();
                        PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, this.grdMainTp1.GetRowCount());
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }

        private void btnRtnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtData = DataTableConverter.Convert(grdMainTp2.ItemsSource);

            if (dtData.Rows.Count == 0 || util.GetDataGridCheckCnt(grdMainTp2, "CHK") == 0)
            {
                //SFU1651 : 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                txtBoxID.Focus();
                return;
            }

            string sRtnCnclRsnNote = this.txtRtnCnlReason.Text;

            if (string.IsNullOrWhiteSpace(sRtnCnclRsnNote))
            {
                //SFU1594 : 사유를 입력하세요
                Util.MessageValidation("SFU1594");

                txtRtnCnlReason.Focus();
                return;
            }

            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                if (dtData.Rows[i]["CHK"].Equals("True"))
                {
                    if (!(dtData.Rows[i]["ISS_STAT_CODE"].ToString().Equals("RETURN") || dtData.Rows[i]["ISS_STAT_CODE"].ToString().Equals("RETURN_SUCCESS") || dtData.Rows[i]["ISS_STAT_CODE"].ToString().Equals("RETURN_CANCEL_FAIL")))
                    {
                        //SFU5145 : 해당 Lot[%1]은 %2 처리할 수 없습니다.
                        string[] sErrorMsg = { dtData.Rows[i]["REPACK_BOX_ID"].ToString(), "RETURN_CANCEL" };

                        Util.MessageValidation("SFU5145", sErrorMsg);

                        txtBoxID.Focus();
                        return;
                    }
                }
            }

            //SFU3258 : 반품취소하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3258"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
            {
                if (Result == MessageBoxResult.OK)
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;

                    try
                    {
                        DataSet indataSet = new DataSet();

                        DataTable INDATA = indataSet.Tables.Add("INDATA");
                        INDATA.Columns.Add("LANGID", typeof(string));
                        INDATA.Columns.Add("SRCTYPE", typeof(string));
                        INDATA.Columns.Add("UPDUSER", typeof(string));
                        INDATA.Columns.Add("RTN_CNCL_RSN_NOTE", typeof(string));

                        DataRow dr = INDATA.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["RTN_CNCL_RSN_NOTE"] = sRtnCnclRsnNote;
                        INDATA.Rows.Add(dr);

                        DataTable IN_RTN_CNCL = indataSet.Tables.Add("IN_RTN_CNCL");
                        IN_RTN_CNCL.Columns.Add("REQ_NO", typeof(string));
                        IN_RTN_CNCL.Columns.Add("REPACK_BOX_ID", typeof(string));
                        IN_RTN_CNCL.Columns.Add("RTN_QTY", typeof(string));

                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {
                            if (dtData.Rows[i]["CHK"].Equals("True"))
                            {
                                DataRow drReq = IN_RTN_CNCL.NewRow();
                                drReq["REQ_NO"] = dtData.Rows[i]["REQ_NO"];
                                IN_RTN_CNCL.Rows.Add(drReq);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_RETURN_CNCL_RACK_MTRL_BOX_STCK", "INDATA,IN_RTN_CNCL", "OUTDATA", indataSet);

                        ms.AlertInfo("PSS9072");  // 처리가 완료되었습니다.
                        Util.gridClear(grdMainTp2);
                        this.SearchProcess();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.txtBoxID.Clear();
                        this.txtRtnCnlReason.Clear();
                        Util.gridClear(grdMainTp1);
                        Util.DataGridCheckAllUnChecked(grdMainTp1);
                        txtBoxID.Focus();
                        PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, this.grdMainTp1.GetRowCount());
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }

        void checkAllLEFT_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < grdMainTp1.Rows.Count; i++)
            {
                DataTableConverter.SetValue(grdMainTp1.Rows[i].DataItem, "CHK", true);
            }
        }

        void checkAllLEFT_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(grdMainTp1);
        }

        private void grdMainTp1_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Cell.Column.Name == "RTN_QTY")
            {
                string Rtn_qty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["RTN_QTY"].Index).Value);
                int iRtn_qty;

                if (!string.IsNullOrWhiteSpace(Rtn_qty) && !int.TryParse(Rtn_qty, out iRtn_qty))
                {
                    //SFU3435 : 숫자만 입력해주세요
                    Util.MessageInfo("SFU3435");
                }
            }
        }

        void check2AllLEFT_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < grdMainTp2.Rows.Count; i++)
            {
                DataTableConverter.SetValue(grdMainTp2.Rows[i].DataItem, "CHK", true);
            }
        }

        void check2AllLEFT_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(grdMainTp2);
        }
        #endregion #4-1. Event_TAB1

        #region #4-2. Function_TAB1. 함수 모음
        private void DeleteNote()
        {
            DataTable dtInfo = DataTableConverter.Convert(grdMainTp1.ItemsSource);

            List<DataRow> drInfo = dtInfo.Select(@"CHK = 'True'")?.ToList();
            foreach (DataRow dr in drInfo)
            {
                dtInfo.Rows.Remove(dr);
            }

            Util.GridSetData(grdMainTp1, dtInfo, FrameOperation, true);
        }

        private bool DeleteNoteValidation()
        {
            DataRow[] drchk = DataTableConverter.Convert(grdMainTp1.ItemsSource).Select(@"CHK = 'True'");

            if (drchk.Length == 0)
            {
                //SFU1651 : 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        // BOX 입력
        private void AddGridgrdMainTp1(string sBoxID)
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                this.txtBoxID.Text = string.Empty;

                // Validation Check...
                DataTable dtResult = this.dataHelper.GetBoxData(sBoxID);

                if (dtResult.Rows.Count == 0)
                {
                    //SFU1905 : 조회된 Data가 없습니다.
                    Util.MessageInfo("SFU1905");
                    return;
                }
                else if (dtResult.Rows.Count > 1)
                {
                    //SFU2887 : {%1}건 이상이 조회되었습니다.
                    Util.MessageInfo("SFU2887", "1");
                    return;
                }
                else
                {
                    if (dtResult.Rows[0]["REQ_NO"] == null) //SFU1905 : 조회된 Data가 없습니다.
                    {
                        Util.MessageInfo("SFU1905");
                        return;
                    }

                    if (!(dtResult.Rows[0]["RTN_ABLE_FLAG"].ToString() == "RETURNABLE_STATE")) //90047 : 해당 LOT[%1]은 반품 대상이 아닙니다.
                    {
                        Util.MessageInfo("90047", sBoxID);
                        return;
                    }
                }

                this.AddGridgrdMainTp1(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                txtBoxID.Focus();
            }
        }

        // Hold LOT Grid에 Hold 대상 Data Add
        private void AddGridgrdMainTp1(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            int totalRow = this.grdMainTp1.GetRowCount();
            if (this.grdMainTp1.GetRowCount() <= 0)
            {
                PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, dt.Rows.Count);
                Util.GridSetData(this.grdMainTp1, dt, FrameOperation, true);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[0].DataItem, "CHK", true);

                if (dt.Rows.Count > 1)
                {
                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        //Util.DataGridRowAdd(this.grdMainTp1, 1);

                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "CHK", true);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "AREANAME", dt.Rows[i]["AREANAME"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "EQSGNAME", dt.Rows[i]["EQSGNAME"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "MTRL_PORT_ID", dt.Rows[i]["MTRL_PORT_ID"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "MTRLID", dt.Rows[i]["MTRLID"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "REQ_NO", dt.Rows[i]["REQ_NO"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "REPACK_BOX_ID", dt.Rows[i]["REPACK_BOX_ID"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "PLLT_ID", dt.Rows[i]["PLLT_ID"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "REQ_STAT_CODE", dt.Rows[i]["REQ_STAT_CODE"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "ISS_STAT_CODE", dt.Rows[i]["ISS_STAT_CODE"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "ISS_QTY", dt.Rows[i]["ISS_QTY"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "RTN_QTY", dt.Rows[i]["RTN_QTY"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "REQ_WRK_DTTM", dt.Rows[i]["REQ_WRK_DTTM"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "ISS_RACK_LOAD_WRKRNAME", dt.Rows[i]["ISS_RACK_LOAD_WRKRNAME"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "ISS_RACK_LOAD_DTTM", dt.Rows[i]["ISS_RACK_LOAD_DTTM"]);
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "KANBAN_ID", dt.Rows[i]["KANBAN_ID"]);
                    }
                }
                
                return;
            }

            //DataTable dtDr = DataTableConverter.Convert(grdMainTp1.ItemsSource);
            //dtDr.AsEnumerable().CopyToDataTable(dt, LoadOption.Upsert);
            //Util.GridSetData(grdMainTp1, dt, FrameOperation);

            foreach (DataRowView drv in dt.AsDataView())
            {
                Util.DataGridRowAdd(this.grdMainTp1, 1);

                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "CHK", true);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "AREANAME", drv["AREANAME"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "EQSGNAME", drv["EQSGNAME"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "MTRL_PORT_ID", drv["MTRL_PORT_ID"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "MTRLID", drv["MTRLID"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "REQ_NO", drv["REQ_NO"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "REPACK_BOX_ID", drv["REPACK_BOX_ID"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "PLLT_ID", drv["PLLT_ID"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "REQ_STAT_CODE", drv["REQ_STAT_CODE"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "ISS_STAT_CODE", drv["ISS_STAT_CODE"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "ISS_QTY", drv["ISS_QTY"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "RTN_QTY", drv["RTN_QTY"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "REQ_WRK_DTTM", drv["REQ_WRK_DTTM"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "ISS_RACK_LOAD_WRKRNAME", drv["ISS_RACK_LOAD_WRKRNAME"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "ISS_RACK_LOAD_DTTM", drv["ISS_RACK_LOAD_DTTM"]);
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "KANBAN_ID", drv["KANBAN_ID"]);
                totalRow++;
            }

            PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, this.grdMainTp1.GetRowCount());
        }
        #endregion #4-2. Function_TAB1. 함수 모음
        #endregion #4. tp1 처리 내용

        #region #5. tp2 처리 내용
        #region #5-1. Event_TAB2
        private void cboLine_cfg_SelectedValueChanged(object sender, EventArgs e)
        {

            string sTemp = Util.NVC(cboLine.SelectedItemsToString);

            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlRack, ref this.cboMtrlRackCount);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DateTime firstOfDay = dtpFr.SelectedDateTime;
            DateTime endOfDay = dtpTo.SelectedDateTime;
            TimeSpan dateDiff = endOfDay - firstOfDay;

            if (dateDiff.Days > 31)
            {
                //SFU5033 : 기간은 %1달 이내 입니다.
                Util.MessageValidation("SFU5033", "");
                return;
            }

            Util.gridClear(grdMainTp2);
            this.SearchProcess();
        }

        private void txtBoxID_Detl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(txtBoxID_Detl.Text.Trim()))
                {
                    Util.gridClear(grdMainTp2);
                    this.SearchProcess();
                }
            }
        }

        private void txtMtrlID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(txtMtrlID.Text.Trim()))
                {
                    Util.gridClear(grdMainTp2);
                    this.SearchProcess();
                }
            }
        }
        #endregion #5-1. Event_TAB2
        #endregion #. tp2 처리 내용

        #region #7. Function 공통 조회 함수 모음
        // 조회 - LOT Hold Relase 이력 Tab
        private void SearchProcess()
        {
            this.loadingIndicator.Visibility = Visibility.Visible;

            try
            {
                string sEqsgID = string.Empty;
                string sRackID = string.Empty;
                string sRtnStateCode = string.Empty;
                string sBoxID = string.Empty;
                string sMtrlID = string.Empty;
                
                DateTime dteFromDate = this.dtpFr.SelectedDateTime.Date;
                DateTime dteToDate = this.dtpTo.SelectedDateTime.Date.AddDays(1);

                if (string.IsNullOrWhiteSpace(Convert.ToString(this.cboLine.SelectedItemsToString)))
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                    return;
                }

                if (string.IsNullOrWhiteSpace(Convert.ToString(this.cboMtrlRack.SelectedItemsToString)))
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자재 RACK ID")); // %1(을)를 선택하세요.
                    return;
                }

                sEqsgID = this.cboLine.SelectedItemsToString;
                sRackID = this.cboMtrlRack.SelectedItemsToString;
                sRtnStateCode = this.cboRtnState.SelectedItemsToString;
                sBoxID = string.IsNullOrWhiteSpace(this.txtBoxID_Detl.Text.Trim()) ? string.Empty : this.txtBoxID_Detl.Text.Trim();
                sMtrlID = string.IsNullOrWhiteSpace(this.txtMtrlID.Text.Trim()) ? string.Empty : this.txtMtrlID.Text.Trim();

                DataTable dtTp2 = this.dataHelper.GetBoxHistoryData(sRackID, sMtrlID, sRtnStateCode, sBoxID, sEqsgID, dteFromDate, dteToDate);

                if (CommonVerify.HasTableRow(dtTp2))
                {
                    Util.GridSetData(this.grdMainTp2, dtTp2, FrameOperation);
                    grdMainTp2.FrozenColumnCount = 5;
                    PackCommon.SearchRowCount(ref this.tbGrdMainTp2Cnt, this.grdMainTp2.GetRowCount());
                }

                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion #7. Function 공통 조회 함수 모음

        bool isShowPopupMsg = false;

        private void btnSaveReturnNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtVali = DataTableConverter.Convert(grdMainTp2.ItemsSource);

                var queryValidationCheck = dtVali.AsEnumerable().Where(x => x.Field<string>("CHK").Equals("True")); //

                if (queryValidationCheck.Count() <= 0)
                {
                    isShowPopupMsg = true;
                    Util.Alert("10008");  // 선택된 데이터가 없습니다.                   
                    return;
                }

                var query = dtVali.AsEnumerable().Where(x => x.Field<string>("CHK").Equals("True") && (x.Field<string>("RTN_NAME").Equals("OTHERS(OK_PARTS)") || x.Field<string>("RTN_NAME").Equals("OTHERS(NOK_PARTS)")));

                if (null == query || query.Count() == 0)
                {
                    Util.MessageInfo("SUF9011");
                    isShowPopupMsg = false;
                    return;
                }
                 
                PACK003_038_POPUP popup = new PACK003_038_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = null;
                    Parameters = new object[] { query.CopyToDataTable() };

                    C1WindowExtension.SetParameters(popup, Parameters);
                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.Closed += Popup_Closed;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            this.SearchProcess();
        }

        /// <summary>
        /// 마우스 Double 클릭시 팝업호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdMainTp2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (DataTableConverter.Convert(this.grdMainTp2.ItemsSource).Rows.Count == 0) return;
            if (!this.grdMainTp2.CurrentCell.Column.Name.Equals("RTN_NOTE")) return;
            if (this.grdMainTp2.CurrentRow.Index < 0) return;
            if (!this.grdMainTp2.SelectedItem.GetValue("RTN_NAME").ToString().ToUpper().Contains("OTHERS")) return;

            try
            {
                //GetOtherHistoryData
                DataTable dtTp2 = this.dataHelper.GetOtherHistoryData("RETURN_NOTE", this.grdMainTp2.SelectedItem.GetValue("REQ_NO").ToString());

                if (CommonVerify.HasTableRow(dtTp2))
                {
                    //if (dtTp2.Rows.Count <= 1) return;

                    PACK003_038_POPUP_HIS popup = new PACK003_038_POPUP_HIS();
                    popup.FrameOperation = this.FrameOperation;

                    if (popup != null)
                    {
                        object[] Parameters = null;
                        Parameters = new object[] { dtTp2 };

                        C1WindowExtension.SetParameters(popup, Parameters);
                        popup.ShowModal();
                        popup.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }


    #region #999. DataHelper (Biz 호출)
    internal class PACK003_038_DataHelper
    {
        #region #999-1. Member Variable Lists...
        #endregion #999-1. Member Variable Lists...

        #region #999-2. Constructor
        internal PACK003_038_DataHelper()
        {
        }
        #endregion #999-2. Constructor

        #region #999-3. Member Function Lists...
        internal DataTable GetBoxData(string BOXID)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string bizRuleName = "BR_MTRL_SEL_TB_SFC_PROD_RACK_MTRL_BOX_STCK";
                DataSet dsINDATA = new DataSet();
                DataSet dsOUTDATA = new DataSet();
                string outDataSetName = "OUTDATA";

                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("REPACK_BOX_ID", typeof(string));
                dtINDATA.Columns.Add("RTN_SEARCH_FLAG", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["REPACK_BOX_ID"] = BOXID;
                drINDATA["RTN_SEARCH_FLAG"] = "Y";
                dtINDATA.Rows.Add(drINDATA);

                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);

                if (CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    dtReturn = dsOUTDATA.Tables["OUTDATA"].Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        internal DataTable GetStateData(string cmcdType)
        {
            string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        internal DataTable GetLineData()
        {
            string bizRuleName = "DA_MTRL_SEL_EQUIPMENTSEGMENT_MTRL_PORT_CBO";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        internal DataTable GetRackData(string EqsgID)
        {
            string bizRuleName = "DA_MTRL_SEL_MTRL_PORT_CBO";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = string.IsNullOrWhiteSpace(EqsgID) || EqsgID.Equals("ALL") ? null : EqsgID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        internal DataTable GetBoxHistoryData(string sRackID, string sMtrlID, string sRtnState, string sBoxID, string sEqsgID, DateTime dteFromDate, DateTime dteToDate)
        {
            string bizRuleName = "DA_MTRL_SEL_TB_SFC_PROD_RACK_MTRL_BOX_STCK_RTN_OPT";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtRQSTDT.Columns.Add("MTRLID", typeof(string));
                dtRQSTDT.Columns.Add("ISS_STAT_CODE", typeof(string));
                dtRQSTDT.Columns.Add("REPACK_BOX_ID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                dtRQSTDT.Columns.Add("END_DTTM", typeof(DateTime));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrWhiteSpace(sRackID)) drRQSTDT["MTRL_PORT_ID"] = sRackID;
                if (!string.IsNullOrWhiteSpace(sMtrlID)) drRQSTDT["MTRLID"] = sMtrlID;
                if (!string.IsNullOrWhiteSpace(sRtnState)) drRQSTDT["ISS_STAT_CODE"] = sRtnState;
                if (!string.IsNullOrWhiteSpace(sBoxID)) drRQSTDT["REPACK_BOX_ID"] = sBoxID;
                if (!string.IsNullOrWhiteSpace(sEqsgID)) drRQSTDT["EQSGID"] = sEqsgID;

                drRQSTDT["FROM_DTTM"] = dteFromDate;
                drRQSTDT["END_DTTM"] = dteToDate;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        /// <summary>
        /// 요청번호 반품기타사유
        /// </summary>
        /// <param name="ACTID"></param>
        /// <param name="REQ_NO"></param>
        /// <returns></returns>
        internal DataTable GetOtherHistoryData(string ACTID, string REQ_NO)
        {
            string bizRuleName = "DA_MTRL_SEL_TB_SFC_PROD_RACK_MTRL_BOX_STCK_RTN_HIS";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            { 
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("REQ_NO", typeof(string)); 

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["ACTID"] = ACTID;
                drRQSTDT["REQ_NO"] = REQ_NO;
                 
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }
        #endregion #999-3. Member Function Lists...
    }
    #endregion #999. DataHelper (Biz 호출)
}