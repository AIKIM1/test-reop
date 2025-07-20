/*************************************************************************************
 Created Date : 2023.07.19
      Creator : 장만철
   Decription : 원자재 조회 화면의 자재 반품 관리 POPUT
--------------------------------------------------------------------------------------
 [Change History]
    Date         Author      CSR         Description...
  2023.07.19     장만철       SI         Initial Created.
  2023.09.27     김길용      SM          E-KANBAN JIT 자재적용에 대한 라인조건 추가
  2023.12.12     김길용    1080683       모듈3동 COMPLETE_SUCCESS 코드 추가
  2024.02.13     김길용    1101976       모듈3동 REMAIN_TERM 대응을 위한 구분추가
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

using LGC.GMES.MES.PACK001.Class;


namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_035_REMAIN_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor       
        private string sTrfCode = string.Empty;
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_035_REMAIN_POPUP()
        {
            InitializeComponent();
        }

        Util util = new Util();

        
        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                setComboBox();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //그리드 전체 체크
        void checkAllLEFT_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgScanList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgScanList.Rows[i].DataItem, "CHK", true);
            }
        }

        //그리드 전체 체크 해제
        void checkAllLEFT_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgScanList);
        }

        //kANBAN ID 입력시
        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sBoxID = txtBoxID.Text.Trim();
                string sEqsgID = this.cboSnapEqsg.SelectedValue.ToString();
                if (string.IsNullOrWhiteSpace(this.cboSnapEqsg.SelectedValue.ToString()))
                {
                    //SFU1223 : 라인을 선택하세요.
                    Util.MessageInfo("SFU1223");
                    return;
                }
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
                    if (dgScanList.Rows.Count > 0)
                    {
                        DataTable dt = ((DataView)dgScanList.ItemsSource).Table;

                        string sTempBoxID = txtBoxID.Text.ToString();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            //KANBAID 가 이미 그리드에 있는지 체크
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

                    chkKanbanID(sBoxID, sEqsgID);
                }
            }
        }

        private void btnDelRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (util.GetDataGridCheckCnt(dgScanList, "CHK") == 0)
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
            if (this.dgScanList.Rows.Count != 0)
            {
                //SFU1815 : 입력한 데이터가 삭제됩니다. 계속 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1815"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
                {
                    if (Result == MessageBoxResult.OK)
                    {
                        txtBoxID.Focus();
                        this.txtBoxID.Clear();
                        Util.gridClear(dgScanList);
                        Util.DataGridCheckAllUnChecked(dgScanList);
                        txtBoxID.Focus();
                    }
                });
            }
            else
            {
                txtBoxID.Focus();
            }
        }

        //SAVE BOUTTON 클릭시
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtData = DataTableConverter.Convert(dgScanList.ItemsSource);

            string reason = this.cboReason.SelectedValue.ToString();
            string memo = this.txtMemo.Text.Trim();

            if (dtData.Rows.Count == 0 || util.GetDataGridCheckCnt(dgScanList, "CHK") == 0)
            {
                //SFU1651 : 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                txtBoxID.Focus();
                return;
            }
            //SFU1745 : 완료 처리 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1745"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
            {
                if (Result == MessageBoxResult.OK)
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;

                    try
                    {
                        DataTable RQSTDT = new DataTable("INDATA");

                        RQSTDT.Columns.Add("LANGID");
                        RQSTDT.Columns.Add("REQ_NO");
                        RQSTDT.Columns.Add("UPDUSER");
                        RQSTDT.Columns.Add("SRCTYPE");
                        RQSTDT.Columns.Add("REASON");
                        RQSTDT.Columns.Add("NOTE");
                        RQSTDT.Columns.Add("EQSGID");

                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {
                            if (dtData.Rows[i]["CHK"].Equals("True"))
                            {
                                DataRow dr = RQSTDT.NewRow();
                                dr["LANGID"] = LoginInfo.LANGID;
                                dr["REQ_NO"] = dtData.Rows[i]["REQ_NO"];
                                dr["UPDUSER"] = LoginInfo.USERID;
                                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                dr["REASON"] = reason;
                                dr["NOTE"] = memo;
                                dr["EQSGID"] = dtData.Rows[i]["EQSGID"];
                                RQSTDT.Rows.Add(dr);
                            }
                        }

                        DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_MTRL_REG_REMAIN_RACK_MTRL_BOX_STCK", "INDATA", "OUTDATA", RQSTDT);

                        ms.AlertInfo("SFU1275"); //정상처리되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.txtBoxID.Clear();
                        Util.gridClear(dgScanList);
                        Util.DataGridCheckAllUnChecked(dgScanList);
                        txtBoxID.Focus();
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }

        //닫기 BOUTTON 클릭시
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion

        #region Mehod
        private void setComboBox()
        {
            try
            {
                CommonCombo combo = new CommonCombo();

                //String[] sFilter_EQSGID = { LoginInfo.CFG_AREA_ID };
                //combo.SetCombo(cboEqsgid, CommonCombo.ComboStatus.SELECT, sFilter: sFilter_EQSGID, sCase: "EQSGID_PACK");
                SetCboEQSG(cboSnapEqsg);

                String[] sFilter_KANBAN = { "KANBAN_REMAIN_REASON_CODE" };
                combo.SetCombo(cboReason, CommonCombo.ComboStatus.NONE, sFilter: sFilter_KANBAN, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetCboEQSG(C1ComboBox cboEqsg)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drn = RQSTDT.NewRow();
                drn["LANGID"] = LoginInfo.LANGID;
                drn["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(drn);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MTRL_SEL_EQUIPMENTSEGMENT_MTRL_PORT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dRow = dtResult.NewRow();

                dRow["CBO_CODE"] = "";
                dRow["CBO_NAME"] = "-SELECT-";
                dtResult.Rows.InsertAt(dRow, 0);

                cboEqsg.ItemsSource = DataTableConverter.Convert(dtResult);
                //cboEqsg.IsEnabled = false;
                if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQSG_ID) select dr).Count() > 0)
                {
                    cboEqsg.SelectedValue = LoginInfo.CFG_EQSG_ID;
                }
                else
                {
                    cboEqsg.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //입력한 KANBAN ID 체크
        private void chkKanbanID(string sBoxID, string sEqsgID)
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                this.txtBoxID.Text = string.Empty;

                // Validation Check...
                DataTable dtResult = GetBoxData(sBoxID, sEqsgID);

                if (dtResult.Rows.Count == 0)
                {
                    //SFU1905 : 조회된 Data가 없습니다.
                    Util.MessageInfo("SFU1905");
                    return;
                }
                else
                {
                    if (dtResult.Rows[0]["REQ_NO"] == null) //SFU1905 : 조회된 Data가 없습니다.
                    {
                        Util.MessageInfo("SFU1905");
                        return;
                    }

                    string req_stat_code = dtResult.Rows[0]["REQ_STAT_CODE"].ToString();

                    if (LoginInfo.CFG_AREA_ID !="PA")
                    {
                        if (req_stat_code == "TERM" || req_stat_code == "TERM_COMPLETE" || req_stat_code == "TERM_SUCCESS" ||
                        req_stat_code == "ERP_COMPLETE" || req_stat_code == "ERP_COMPLETE_FAIL" || req_stat_code == "ERP_COMPLETE_SUCCESS" ||
                        req_stat_code == "ERP_PROCESS" || req_stat_code == "ERP_PROCESS_FAIL" || req_stat_code == "ERP_PROCESS_SUCCESS"
                        ) //SFU8520 : 이미 소진 처리된 Box 입니다.
                        {
                            Util.MessageInfo("SFU8520");
                            return;
                        }

                        if (req_stat_code == "REMAIN" || req_stat_code == "REMAIN_SUCCESS") //SFU1775 : 이미 반품 된 LOT입니다.
                        {
                            Util.MessageInfo("SFU1775");
                            return;
                        }

                        if ((req_stat_code != "COMPLETE") && (req_stat_code != "WAIT") && (req_stat_code != "TERM_FAIL") && (req_stat_code != "REMAIN_FAIL") && (req_stat_code != "COMPLETE_SUCCESS")) //SFU8521 : 입고(또는 대기) 처리가 안된 Box 입니다. 입고(또는 대기) 처리 후 소진처리 하십시오.
                        {
                            Util.MessageInfo("SFU8540");
                            return;
                        }
                    }else
                    {
                        if (req_stat_code == "REMAIN" || req_stat_code == "REMAIN_SUCCESS") //SFU1775 : 이미 반품 된 LOT입니다.
                        {
                            Util.MessageInfo("SFU1775");
                            return;
                        }

                        if ((req_stat_code != "COMPLETE") && (req_stat_code != "TERM") && (req_stat_code != "TERM_COMPLETE") && (req_stat_code != "TERM_SUCCESS") && (req_stat_code != "ERP_COMPLETE") && (req_stat_code != "ERP_COMPLETE_FAIL") &&
                                (req_stat_code != "ERP_COMPLETE_SUCCESS") && (req_stat_code != "ERP_PROCESS") && (req_stat_code != "ERP_PROCESS_FAIL") && (req_stat_code != "ERP_PROCESS_SUCCESS") && (req_stat_code != "ERP_COMPLETE_SUCCESS") )
                        {
                            Util.MessageInfo("SFU8540");
                            return;
                        }

                    }
                    
                }

                this.AddGrid(dtResult);
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

        // Grid에 Kanban id Data Add
        private void AddGrid(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            int totalRow = this.dgScanList.GetRowCount();
            if (this.dgScanList.GetRowCount() <= 0)
            {
                PackCommon.SearchRowCount(ref this.tbScanListCount, dt.Rows.Count);
                Util.GridSetData(this.dgScanList, dt, FrameOperation, true);
                DataTableConverter.SetValue(this.dgScanList.Rows[0].DataItem, "CHK", true);

                if (dt.Rows.Count > 1)
                {
                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        //Util.DataGridRowAdd(this.grdMainTp1, 1);

                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "CHK", true);
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "AREANAME", dt.Rows[i]["AREANAME"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "EQSGNAME", dt.Rows[i]["EQSGNAME"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "EQSGID", dt.Rows[i]["EQSGID"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "MTRL_PORT_ID", dt.Rows[i]["MTRL_PORT_ID"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "MTRLID", dt.Rows[i]["MTRLID"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "REQ_NO", dt.Rows[i]["REQ_NO"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "KANBAN_ID", dt.Rows[i]["KANBAN_ID"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "REPACK_BOX_ID", dt.Rows[i]["REPACK_BOX_ID"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "PLLT_ID", dt.Rows[i]["PLLT_ID"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "REQ_STAT_CODE", dt.Rows[i]["REQ_STAT_CODE"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "ISS_QTY", dt.Rows[i]["ISS_QTY"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "REQ_WRK_DTTM", dt.Rows[i]["REQ_WRK_DTTM"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "ISS_RACK_LOAD_WRKRNAME", dt.Rows[i]["ISS_RACK_LOAD_WRKRNAME"].ToString());
                        DataTableConverter.SetValue(this.dgScanList.Rows[i].DataItem, "ISS_RACK_LOAD_DTTM", dt.Rows[i]["ISS_RACK_LOAD_DTTM"].ToString());
                    }
                }

                return;
            }

            //DataTable dtDr = DataTableConverter.Convert(grdMainTp1.ItemsSource);
            //dtDr.AsEnumerable().CopyToDataTable(dt, LoadOption.Upsert);
            //Util.GridSetData(grdMainTp1, dt, FrameOperation);

            foreach (DataRowView drv in dt.AsDataView())
            {
                Util.DataGridRowAdd(this.dgScanList, 1);

                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "CHK", true);
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "AREANAME", drv["AREANAME"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "EQSGNAME", drv["EQSGNAME"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "EQSGID", drv["EQSGID"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "MTRL_PORT_ID", drv["MTRL_PORT_ID"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "MTRLID", drv["MTRLID"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "REQ_NO", drv["REQ_NO"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "KANBAN_ID", drv["KANBAN_ID"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "REPACK_BOX_ID", drv["REPACK_BOX_ID"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "PLLT_ID", drv["PLLT_ID"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "REQ_STAT_CODE", drv["REQ_STAT_CODE"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "ISS_QTY", drv["ISS_QTY"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "REQ_WRK_DTTM", drv["REQ_WRK_DTTM"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "ISS_RACK_LOAD_WRKRNAME", drv["ISS_RACK_LOAD_WRKRNAME"].ToString());
                DataTableConverter.SetValue(this.dgScanList.Rows[totalRow].DataItem, "ISS_RACK_LOAD_DTTM", drv["ISS_RACK_LOAD_DTTM"].ToString());
                totalRow++;
            }

            PackCommon.SearchRowCount(ref this.tbScanListCount, this.dgScanList.GetRowCount());
        }

        //행삭제
        private bool DeleteNoteValidation()
        {
            DataRow[] drchk = DataTableConverter.Convert(dgScanList.ItemsSource).Select(@"CHK = 'True'");

            if (drchk.Length == 0)
            {
                //SFU1651 : 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private void DeleteNote()
        {
            DataTable dtInfo = DataTableConverter.Convert(dgScanList.ItemsSource);

            List<DataRow> drInfo = dtInfo.Select(@"CHK = 'True'")?.ToList();
            foreach (DataRow dr in drInfo)
            {
                dtInfo.Rows.Remove(dr);
            }

            Util.GridSetData(dgScanList, dtInfo, FrameOperation, true);
        }
        #endregion

        #region DATA Access
        private DataTable GetBoxData(string BOXID, string EQSGID)
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
                dtINDATA.Columns.Add("EQSGID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["REPACK_BOX_ID"] = BOXID;
                drINDATA["EQSGID"] = EQSGID;
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

       
        #endregion


    }
}
