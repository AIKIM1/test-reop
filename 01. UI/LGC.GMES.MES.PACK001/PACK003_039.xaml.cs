/*************************************************************************************
 Created Date : 2023.01.17
      Creator : seonjun
   Decription : 자재 보류/적재 처리 및 이력 조회 화면
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2023.01.17      seonjun                            Initial Created.
***************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_039 : UserControl, IWorkArea
    {
        #region #1. Member Variable Lists...
        private PACK003_039_DataHelper dataHelper = new PACK003_039_DataHelper(); 

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();

        int cboRtnStateCount = 0;
        int cboLineCount = 0;
        int cboMtrlRackCount = 0;
 
        private const string constCOMPLETE = "COMPLETE";
        private const string constWAIT = "WAIT";

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
        public PACK003_039()
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

                //PackCommon.SetC1ComboBox(this.dataHelper.GetStateData("PACK_RACK_MTRL_BOX_STCK_REQ_STAT_CODE"), this.cboStatus, string.Empty);
                this.txtStatus.Text = constWAIT;

                txtBoxID.Focus();
                #endregion #. tp1

                #region #. tp2 
                this.txtBoxID_Detl.Clear();
                this.txtMtrlID.Clear();
                
                PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetStateData("PACK_RACK_MTRL_BOX_STCK_REQ_STAT_CODE"), this.cboStatus1, ref this.cboRtnStateCount);

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
                        var query_boxCHK = ((DataView)grdMainTp1.ItemsSource).Table.AsEnumerable().Where(x => x.Field<string>("REPACK_BOX_ID").Equals(sBoxID));
                        var query_kanCHK = ((DataView)grdMainTp1.ItemsSource).Table.AsEnumerable().Where(x => x.Field<string>("KANBAN_ID").Equals(sBoxID));

                        if ((null != query_boxCHK && query_boxCHK.Count() > 0) || (null != query_kanCHK && query_kanCHK.Count() > 0))
                        {
                            //SFU2882 : %1은(는) 이미 스캔한 값 입니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2882", new object[] { sBoxID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                this.txtBoxID.Clear();
                                this.txtBoxID.Focus();
                            });
                            return;
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

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        this.txtStatus.Text = constWAIT;
                        this.btnChange.IsEnabled = true;
                        txtBoxID.Focus();
                    }
                });
            }
            else
            {
                txtBoxID.Focus();
            }
        }

        /// <summary>
        /// 보류/적재 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnComplete_Click(object sender, RoutedEventArgs e)
        { 
            DataTable dtData = DataTableConverter.Convert(grdMainTp1.ItemsSource);

            if (dtData.Rows.Count == 0 || util.GetDataGridCheckCnt(grdMainTp1, "CHK") == 0)
            {
                //SFU1651 : 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                txtBoxID.Focus();
                return;
            }

            //SFU1925 : 처리하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1925"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
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
                        INDATA.Columns.Add("REQ_NO", typeof(string));
                        INDATA.Columns.Add("REQ_STAT_CODE", typeof(string)); 
                        INDATA.Columns.Add("UPDUSER", typeof(string));

                        IEnumerable<DataRow> query = from sel in DataTableConverter.Convert(grdMainTp1.ItemsSource).AsEnumerable()
                                                     where sel.Field<string>("CHK") == "True"
                                                     select sel;
 
                        foreach (DataRow item in query)
                        { 
                            INDATA.Rows.Add(LoginInfo.LANGID, SRCTYPE.SRCTYPE_UI, item["REQ_NO"], this.txtStatus.Text.Trim(), LoginInfo.USERID);
                        }
                        //보류/적재
                        int iRow = INDATA.Rows.Count;
                        DataSet dsRst = new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_REG_WAIT_RACK_MTRL_BOX_STCK", "INDATA", "OUTDATA", indataSet);

                        if (dsRst.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            //SFU4889 총[%1]건 중 성공 [%2]건 진행중[%3]건, 실패[%4]건
                            //Util.MessageInfo("", new [] { iRow, iRow - dsRst.Tables["OUTDATA"].Rows.Count, 0, dsRst.Tables["OUTDATA"].Rows.Count });

                            PACK003_039_POPUP popup = new PACK003_039_POPUP();
                            popup.FrameOperation = this.FrameOperation;

                            if (popup != null)
                            {
                                object[] Parameters = null;
                                Parameters = new object[] { dsRst.Tables["OUTDATA"] };

                                C1WindowExtension.SetParameters(popup, Parameters);
                                popup.ShowModal();
                                popup.CenterOnScreen(); 
                            }
                        }
                        else
                        {
                            ms.AlertInfo("PSS9072");  // 처리가 완료되었습니다.                    
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.txtBoxID.Clear();
                        Util.gridClear(grdMainTp1);
                        Util.DataGridCheckAllUnChecked(grdMainTp1);

                        this.btnChange.IsEnabled = true; //처리유형코드 활성화

                        PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, this.grdMainTp1.GetRowCount());
                        txtBoxID.Focus();
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
            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //if (e.Cell.Column.Name == "RTN_QTY")
            //{
            //    string Rtn_qty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["RTN_QTY"].Index).Value);
            //    int iRtn_qty;

            //    if (!string.IsNullOrWhiteSpace(Rtn_qty) && !int.TryParse(Rtn_qty, out iRtn_qty))
            //    {
            //        //SFU3435 : 숫자만 입력해주세요
            //        Util.MessageInfo("SFU3435");
            //    }
            //}
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtStatus.Text.Trim().Equals(constCOMPLETE))
            {
                this.txtStatus.Text = constWAIT;
            }
            else
            {
                this.txtStatus.Text = constCOMPLETE;
            }
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

            //처리유형코드활성화
            if (dtInfo.Rows.Count == 0)
            {
                this.btnChange.IsEnabled = true;
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

                // LOT정보조회
                DataTable dtResult = this.dataHelper.GetBoxData(sBoxID);

                #region Validation
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

                    //선택된 유형과 BOX의 처리유형코드가 같으면 안됨.(체크)     
                    string sStat = this.txtStatus.Text.Trim();
                    if (dtResult.Rows[0]["REQ_STAT_CODE"].ToString().Equals(constCOMPLETE) || dtResult.Rows[0]["REQ_STAT_CODE"].ToString().Equals(constWAIT))
                    {
                        if (dtResult.Rows[0]["REQ_STAT_CODE"].ToString().Equals(sStat)) 
                        {
                            Util.MessageInfo("100268", sBoxID, sStat); //100268 :  [%1]는 [%2] 처리 대상이 아닙니다.
                            return;
                        }
                    }
                    else
                    {
                        Util.MessageInfo("100268", sBoxID, sStat); //100268 :  [%1]는 [%2] 처리 대상이 아닙니다.
                        return;
                    }

                    this.AddGridgrdMainTp1(dtResult);
                }
                #endregion //Validation
                
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

        /// <summary>
        /// Grid에 행 추가
        /// </summary>
        /// <param name="dt"></param>
        private void AddGridgrdMainTp1(DataTable dt)
        {            
            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            } 
            //체크 true
            dt.Rows[0]["CHK"] = true;
            //Grid Data
            DataTable dtData = DataTableConverter.Convert(grdMainTp1.ItemsSource);

            if (!CommonVerify.HasTableRow(dtData))
            {
                dtData = dt.Copy();
            }
            else
            {
                // MES 2.0 ItemArray 위치 오류 Patch
                //dtData.Rows.Add(dt.Rows[0].ItemArray);
                dtData.AddDataRow(dt.Rows[0]);
            } 

            Util.GridSetData(this.grdMainTp1, dtData, FrameOperation, true); 
            PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, this.grdMainTp1.GetRowCount());
            
            this.btnChange.IsEnabled = false; //처리유형코드 비활성화
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

        /// <summary>
        /// 자재 Box 보류적재 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {  
            Util.gridClear(grdMainTp2);
            this.SearchProcess();
        }

        /// <summary>
        /// BOXID Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 자재코드 Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        // 조회 - 
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

                //기간은 1년으로 함
                DataTable dtTp2 = this.dataHelper.GetBoxHistoryData(this.cboStatus1.SelectedItemsToString, this.txtBoxID_Detl.Text.Trim(), this.cboLine.SelectedItemsToString, this.cboMtrlRack.SelectedItemsToString,
                    this.txtMtrlID.Text.Trim(), DateTime.Today.AddYears(-1), DateTime.Today.AddDays(2));

                if (CommonVerify.HasTableRow(dtTp2))
                {
                    Util.GridSetData(this.grdMainTp2, dtTp2, FrameOperation);
                    grdMainTp2.FrozenColumnCount = 5;
                    PackCommon.SearchRowCount(ref this.tbGrdMainTp2Cnt, this.grdMainTp2.GetRowCount());
                }                 
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
    }

    #region #999. DataHelper (Biz 호출)
    internal class PACK003_039_DataHelper
    {
        #region #999-1. Member Variable Lists...
        #endregion #999-1. Member Variable Lists...

        #region #999-2. Constructor
        internal PACK003_039_DataHelper()
        {
        }
        #endregion #999-2. Constructor

        #region #999-3. Member Function Lists...
        /// <summary>
        /// BOX SCAN
        /// </summary>
        /// <param name="BOXID"></param>
        /// <returns></returns>
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

                dtINDATA.Rows.Add(LoginInfo.LANGID, BOXID, "W");
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

        /// <summary>
        /// 공통코드조회
        /// </summary>
        /// <param name="cmcdType"></param>
        /// <param name="attrVal">속성번호</param>
        /// <param name="attrCond">속성조회조건</param>
        /// <returns></returns>
        internal DataTable GetStateData(string cmcdType)
        {
            string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string)); 
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string)); 
                 
                dtRQSTDT.Rows.Add(LoginInfo.LANGID, cmcdType, "W");

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT); 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT.Select("", "CBO_CODE DESC").CopyToDataTable<DataRow>();
        }

        /// <summary>
        /// 라인조회
        /// </summary>
        /// <returns></returns>
        internal DataTable GetLineData()
        {
            string bizRuleName = "DA_MTRL_SEL_EQUIPMENTSEGMENT_MTRL_PORT_CBO";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string)); 

                dtRQSTDT.Rows.Add(LoginInfo.LANGID, LoginInfo.CFG_AREA_ID);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        /// <summary>
        /// RACK조회
        /// </summary>
        /// <param name="EqsgID"></param>
        /// <returns></returns>
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
                 
                dtRQSTDT.Rows.Add(LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, string.IsNullOrWhiteSpace(EqsgID) || EqsgID.Equals("ALL") ? null : EqsgID);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        /// <summary>
        /// 박스적재정보조회
        /// </summary>
        /// <param name="REQ_STAT_CODE">처리유형코드</param>
        /// <param name="REPACK_BOX_ID">BOXID</param>
        /// <param name="EQSGID">라인ID</param>
        /// <param name="MTRL_PORT_ID">자재RACK ID</param>
        /// <param name="MTRLID">자재코드</param>
        /// <param name="dteFromDate">기간From</param>
        /// <param name="dteToDate">기간To</param>
        /// <returns></returns>
        internal DataTable GetBoxHistoryData(string REQ_STAT_CODE, string REPACK_BOX_ID,  string EQSGID, string MTRL_PORT_ID, string MTRLID, DateTime dteFromDate, DateTime dteToDate)
        {
            string bizRuleName = "DA_MTRL_SEL_TB_SFC_PROD_RACK_MTRL_BOX_STCK_OPT";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            { 
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("REQ_STAT_CODE", typeof(string));
                dtRQSTDT.Columns.Add("REPACK_BOX_ID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtRQSTDT.Columns.Add("MTRLID", typeof(string));
                dtRQSTDT.Columns.Add("ISS_STAT_CODE", typeof(string));
                dtRQSTDT.Columns.Add("WAIT_YN", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                dtRQSTDT.Columns.Add("END_DTTM", typeof(DateTime)); 

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrEmpty(REQ_STAT_CODE)) drRQSTDT["REQ_STAT_CODE"] = REQ_STAT_CODE;
                if (!string.IsNullOrEmpty(REPACK_BOX_ID)) drRQSTDT["REPACK_BOX_ID"] = REPACK_BOX_ID;
                if (!string.IsNullOrEmpty(EQSGID)) drRQSTDT["EQSGID"] = EQSGID;
                if (!string.IsNullOrEmpty(MTRL_PORT_ID)) drRQSTDT["MTRL_PORT_ID"] = MTRL_PORT_ID;
                if (!string.IsNullOrEmpty(MTRLID)) drRQSTDT["MTRLID"] = MTRLID;

                drRQSTDT["WAIT_YN"] = "Y";  //자재 보류만 조회
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
        #endregion #999-3. Member Function Lists...
    }
    #endregion #999. DataHelper (Biz 호출)
}