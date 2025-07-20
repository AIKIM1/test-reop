/*************************************************************************************
 Created Date : 2022.05.26
      Creator : 이태규
   Decription : 창고 저장위치 입/출고
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2022.09.21      김우련                             Initial Created.
  2022.10.07      김우련      C20221011-000373       자재 입고/소진 이력 화면 추가
  2022.10.14      김우련      C20221011-000373       자재 재고현황 화면 추가
  2022.10.28      김진수      C20221011-000373       콤보박스 및 조회 데이터 컬럼 추가
  2023.01.13      이태규                             칼럼추가:ISS_STAT_CODENAME(반품상태)
  2023.01.19      이태규                             기능추가 : WAIT 상태 추가.
  2023.09.27      김길용      SM                     E-KANBAN JIT 자재적용에 대한 라인조건 추가
  2023.12.12      김길용      1080683                모듈3동 COMPLETE_SUCCESS 코드 추가
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
    public partial class PACK003_036 : UserControl, IWorkArea
    {
        #region #1. Member Variable Lists...
        private PACK003_036_DataHelper dataHelper = new PACK003_036_DataHelper();
        private const string ERROR_BOX_USING = "ERROR_BOX_USING";

        Color C = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//하얀색
        Color R = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF92D050");//초록색
        Color W = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF00");//노란색
        Color T = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000");//빨간색
        Color O = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFC000");//주황색
        Color G = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#8C8C8C");//회색
        Color F = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");//검정색
        Color B = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF638EC6");//파란색

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        int cboStateCount = 0;
        int cboLineCount = 0;
        int cboLineTp3Count = 0;
        int cboMtrlRackCount = 0;
        int cboMtrlRackTp3Count = 0;
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
        public PACK003_036()
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
                iniCombo();
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
                
                PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetStateData("PACK_RACK_MTRL_BOX_STCK_REQ_STAT_CODE"), this.cboState, ref this.cboStateCount);

                //SetComboBox(this.cboLine);
                PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetLineData(), this.cboLine, ref this.cboLineCount, LoginInfo.CFG_EQSG_ID);
                Util.gridClear(grdMainTp2);
                #endregion #. tp2

                #region #. tp3
                this.txtMtrlIDTp3.Clear();
                //SetComboBox(this.cboLineTp3);
                PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetLineData(), this.cboLineTp3, ref this.cboLineTp3Count, LoginInfo.CFG_EQSG_ID);
                Util.gridClear(grdMainTp3);
                #endregion #. tp3

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
                    if (grdMainTp1.Rows.Count > 0)
                    {
                        DataTable dt = ((DataView)grdMainTp1.ItemsSource).Table;

                        string sTempBoxID = txtBoxID.Text.ToString();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            //KANBAID 스캔 경우도 추가
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

                    AddGridgrdMainTp1(sBoxID, sEqsgID);
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

        private void btnUseComp_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtData = DataTableConverter.Convert(grdMainTp1.ItemsSource);

            if (dtData.Rows.Count == 0 || util.GetDataGridCheckCnt(grdMainTp1, "CHK") == 0)
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
                                dr["EQSGID"] = dtData.Rows[i]["EQSGID"];
                                RQSTDT.Rows.Add(dr);
                            }
                        }

                        DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_MTRL_REG_TERM_RACK_MTRL_BOX_STCK", "INDATA", "OUTDATA", RQSTDT);
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
        #endregion #4-1. Event_TAB1

        #region #4-2. Function_TAB1. 함수 모음
        private void iniCombo()
        {
            SetCboEQSG(cboSnapEqsg);
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
        private void AddGridgrdMainTp1(string sBoxID, string sEqsgID)
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                this.txtBoxID.Text = string.Empty;
                // Validation Check...
                DataTable dtResult = this.dataHelper.GetBoxData(sBoxID, sEqsgID);

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

        // Material box Term processing grid Data Add
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
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "AREANAME", dt.Rows[i]["AREANAME"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "EQSGNAME", dt.Rows[i]["EQSGNAME"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "EQSGID", dt.Rows[i]["EQSGID"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "MTRL_PORT_ID", dt.Rows[i]["MTRL_PORT_ID"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "MTRLID", dt.Rows[i]["MTRLID"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "REQ_NO", dt.Rows[i]["REQ_NO"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "KANBAN_ID", dt.Rows[i]["KANBAN_ID"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "REPACK_BOX_ID", dt.Rows[i]["REPACK_BOX_ID"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "PLLT_ID", dt.Rows[i]["PLLT_ID"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "REQ_STAT_CODE", dt.Rows[i]["REQ_STAT_CODE"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "ISS_QTY", dt.Rows[i]["ISS_QTY"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "REQ_WRK_DTTM", dt.Rows[i]["REQ_WRK_DTTM"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "ISS_RACK_LOAD_WRKRNAME", dt.Rows[i]["ISS_RACK_LOAD_WRKRNAME"].ToString());
                        DataTableConverter.SetValue(this.grdMainTp1.Rows[i].DataItem, "ISS_RACK_LOAD_DTTM", dt.Rows[i]["ISS_RACK_LOAD_DTTM"].ToString());
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
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "AREANAME", drv["AREANAME"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "EQSGNAME", drv["EQSGNAME"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "EQSGID", drv["EQSGID"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "MTRL_PORT_ID", drv["MTRL_PORT_ID"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "MTRLID", drv["MTRLID"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "REQ_NO", drv["REQ_NO"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "KANBAN_ID", drv["KANBAN_ID"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "REPACK_BOX_ID", drv["REPACK_BOX_ID"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "PLLT_ID", drv["PLLT_ID"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "REQ_STAT_CODE", drv["REQ_STAT_CODE"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "ISS_QTY", drv["ISS_QTY"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "REQ_WRK_DTTM", drv["REQ_WRK_DTTM"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "ISS_RACK_LOAD_WRKRNAME", drv["ISS_RACK_LOAD_WRKRNAME"].ToString());
                DataTableConverter.SetValue(this.grdMainTp1.Rows[totalRow].DataItem, "ISS_RACK_LOAD_DTTM", drv["ISS_RACK_LOAD_DTTM"].ToString());
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

        //private void cboLine_cfg_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    CommonCombo _combo = new CommonCombo();

        //    string sTemp = Util.NVC(cboLine.SelectedValue);
        //    string[] sArry = sTemp.Split('^');


        //    PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sArry[0]), this.cboMtrlRack, ref this.cboMtrlRackCount);

        //    //DataTable cbRack = this.dataHelper.GetRackData(sArry[0]);

        //    //DataTable cbs = new DataTable();
        //    //cbs.Columns.Add("CBO_CODE", typeof(string));
        //    //cbs.Columns.Add("CBO_NAME", typeof(string));

        //    //cbs.Rows.Add(new string[] { "ALL", "ALL" });

        //    //for (int i = 0; i < cbRack.Rows.Count; i++)
        //    //{
        //    //    cbs.Rows.Add(new string[] { cbRack.Rows[i]["CBO_CODE"].ToString(), cbRack.Rows[i]["CBO_NAME"].ToString() });
        //    //}

        //    //cboMtrlRack.ItemsSource = cbs.DefaultView;
        //    //cboMtrlRack.DisplayMemberPath = "CBO_NAME";
        //    //cboMtrlRack.SelectedValuePath = "CBO_CODE";

        //    //cboMtrlRack.SelectedIndex = 0;
        //}

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
            this.SearchProcess("TP2");
        }

        private void txtBoxID_Detl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(txtBoxID_Detl.Text.Trim()))
                {
                    Util.gridClear(grdMainTp2);
                    this.SearchProcess("TP2");
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
                    this.SearchProcess("TP2");
                }
            }
        }
        #endregion #5-1. Event_TAB2
        #endregion #. tp2 처리 내용

        #region #6. tp3 처리 내용
        #region #6-1. Event_TAB3
        private void btnSearchTp3_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(grdMainTp3);
            this.SearchProcess("TP3");
        }

        private void cboLineTp3_SelectedValueChanged(object sender, EventArgs e)
        {
            CommonCombo _combo = new CommonCombo();

            string sTemp = Util.NVC(cboLineTp3.SelectedItemsToString);
            //string[] sArry = sTemp.Split('^');

            DataTable cbRack = this.dataHelper.GetRackData(sTemp);


            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlRackTp3, ref this.cboMtrlRackTp3Count);
        }

        //private void cboLineTp3_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    CommonCombo _combo = new CommonCombo();

        //    string sTemp = Util.NVC(cboLineTp3.SelectedItemsToString);
        //    //string[] sArry = sTemp.Split('^');

        //    DataTable cbRack = this.dataHelper.GetRackData(sTemp);


        //    PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetRackData(sTemp), this.cboMtrlRackTp3, ref this.cboMtrlRackTp3Count);

        //    //DataTable cbs = new DataTable();
        //    //cbs.Columns.Add("CBO_CODE", typeof(string));
        //    //cbs.Columns.Add("CBO_NAME", typeof(string));

        //    //cbs.Rows.Add(new string[] { "ALL", "ALL" });

        //    //for (int i = 0; i < cbRack.Rows.Count; i++)
        //    //{
        //    //    cbs.Rows.Add(new string[] { cbRack.Rows[i]["CBO_CODE"].ToString(), cbRack.Rows[i]["CBO_NAME"].ToString() });
        //    //}

        //    //cboMtrlRackTp3.ItemsSource = cbs.DefaultView;
        //    //cboMtrlRackTp3.DisplayMemberPath = "CBO_NAME";
        //    //cboMtrlRackTp3.SelectedValuePath = "CBO_CODE";

        //    //cboMtrlRackTp3.SelectedIndex = 0;
        //}


        private void txtMtrlIDTp3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(txtMtrlIDTp3.Text.Trim()))
                {
                    Util.gridClear(grdMainTp3);
                    this.SearchProcess("TP3");
                }
            }
        }

        private void grdMainTp3_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "STATE_FLAG")
                    {
                        SetCellColor(dataGrid, e);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion #6-1. Event_TAB3
        #endregion #6. tp3 처리 내용

        #region #7. Function 공통 조회 함수 모음
        public void SetComboBox(C1ComboBox cbBox)
        {
            CommonCombo _combo = new CommonCombo();

            //Line
            DataTable dtLine = this.dataHelper.GetLineData();

            DataTable cbs = new DataTable();
            cbs.Columns.Add("CBO_CODE", typeof(string));
            cbs.Columns.Add("CBO_NAME", typeof(string));

            cbs.Rows.Add(new string[] { "ALL", "ALL" });

            for (int i = 0; i < dtLine.Rows.Count; i++)
            {
                cbs.Rows.Add(new string[] { dtLine.Rows[i]["CBO_CODE"].ToString(), dtLine.Rows[i]["CBO_NAME"].ToString() });
            }

            cbBox.ItemsSource = cbs.DefaultView;
            cbBox.DisplayMemberPath = "CBO_NAME";
            cbBox.SelectedValuePath = "CBO_CODE";

            //cbBox.SelectedIndex = 0;
            if ((from DataRow dr in dtLine.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQSG_ID) select dr).Count() > 0)
            {
                cbBox.SelectedValue = LoginInfo.CFG_EQSG_ID;
            }
            else
            {
                cbBox.SelectedIndex = 0;
            }
        }

        // 조회 - LOT Hold Relase 이력 Tab
        private void SearchProcess(string sTabNo)
        {
            this.loadingIndicator.Visibility = Visibility.Visible;

            try
            {
                string sEqsgID = string.Empty;
                string sRackID = string.Empty;
                string sStateCode = string.Empty;
                string sBoxID = string.Empty;
                string sMtrlID = string.Empty;

                if (sTabNo.Equals("TP2"))
                {
                    DateTime dteFromDate = this.dtpFr.SelectedDateTime.Date;
                    DateTime dteToDate = this.dtpTo.SelectedDateTime.Date.AddDays(1);

                    //if (string.IsNullOrEmpty(Convert.ToString(this.cboLine.SelectedItemsToString)))
                    //{
                    //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                    //    return;
                    //}
                    //if (string.IsNullOrEmpty(Convert.ToString(this.cboMtrlRack.SelectedItemsToString)))
                    //{
                    //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자재 RACK ID")); // %1(을)를 선택하세요.
                    //    return;
                    //}

                    sEqsgID = this.cboLine.SelectedItemsToString;
                    sRackID = this.cboMtrlRack.SelectedItemsToString;
                    sStateCode = this.cboState.SelectedItemsToString;
                    sBoxID = string.IsNullOrWhiteSpace(this.txtBoxID_Detl.Text.Trim()) ? string.Empty : this.txtBoxID_Detl.Text.Trim();
                    sMtrlID = string.IsNullOrWhiteSpace(this.txtMtrlID.Text.Trim()) ? string.Empty : this.txtMtrlID.Text.Trim();

                    DataTable dtTp2 = this.dataHelper.GetBoxHistoryData(sRackID, sMtrlID, sStateCode, sBoxID, sEqsgID, dteFromDate, dteToDate);

                    if (CommonVerify.HasTableRow(dtTp2))
                    {
                        Util.GridSetData(this.grdMainTp2, dtTp2, FrameOperation);
                        grdMainTp2.FrozenColumnCount = 5;
                        PackCommon.SearchRowCount(ref this.tbGrdMainTp2Cnt, this.grdMainTp2.GetRowCount());
                    }
                }
                else
                {
                    //if (string.IsNullOrEmpty(Convert.ToString(this.cboLineTp3.SelectedItemsToString)))
                    //{
                    //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인")); // %1(을)를 선택하세요.
                    //    return;
                    //}
                    //if (string.IsNullOrEmpty(Convert.ToString(this.cboMtrlRackTp3.SelectedItemsToString)))
                    //{
                    //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자재 RACK ID")); // %1(을)를 선택하세요.
                    //    return;
                    //}

                    sEqsgID = this.cboLineTp3.SelectedItemsToString;
                    sRackID = this.cboMtrlRackTp3.SelectedItemsToString;
                    sMtrlID = string.IsNullOrWhiteSpace(this.txtMtrlIDTp3.Text.Trim()) ? string.Empty : this.txtMtrlIDTp3.Text.Trim();

                    DataTable dtTp3 = this.dataHelper.GetBoxHistorySummary(sEqsgID, sRackID, sMtrlID);

                    if (CommonVerify.HasTableRow(dtTp3))
                    {
                        Util.GridSetData(this.grdMainTp3, dtTp3, FrameOperation);
                        grdMainTp3.FrozenColumnCount = 5;
                        PackCommon.SearchRowCount(ref this.tbGrdMainTp3Cnt, this.grdMainTp3.GetRowCount());
                    }
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

        private void SetCellColor(C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            Color G = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEEEEEE");//회색
            if (e.Cell.Row.DataItem != null)//  if (e.Cell.Row.Index >= 0)
            {
                if (dataGrid.Name.Equals("grdMainTp3"))
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                string sState_Flag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "STATE_FLAG"));

                                if (sState_Flag == "GREEN")
                                    e.Cell.Presenter.Background = new SolidColorBrush(R);
                                else if (sState_Flag == "ORANGE")
                                    e.Cell.Presenter.Background = new SolidColorBrush(O);
                                else if (sState_Flag == "RED")
                                    e.Cell.Presenter.Background = new SolidColorBrush(T);
                                else if (sState_Flag == "WHITE")
                                    e.Cell.Presenter.Background = new SolidColorBrush(C);
                            }
                        }
                    }
                }
            }
        }
        #endregion #7. Function 공통 조회 함수 모음
    }

    #region #999. DataHelper (Biz 호출)
    internal class PACK003_036_DataHelper
    {
        #region #999-1. Member Variable Lists...
        #endregion #999-1. Member Variable Lists...

        #region #999-2. Constructor
        internal PACK003_036_DataHelper()
        {
        }
        #endregion #999-2. Constructor

        #region #999-3. Member Function Lists...
        internal DataTable GetBoxData(string BOXID, string EQSGID)
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

        internal DataTable GetBoxHistoryData(string sRackID, string sMtrlID, string sState, string sBoxID, string sEqsgID, DateTime dteFromDate, DateTime dteToDate)
        {
            string bizRuleName = "DA_MTRL_SEL_TB_SFC_PROD_RACK_MTRL_BOX_STCK_OPT";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtRQSTDT.Columns.Add("MTRLID", typeof(string));
                dtRQSTDT.Columns.Add("REQ_STAT_CODE", typeof(string));
                dtRQSTDT.Columns.Add("REPACK_BOX_ID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                dtRQSTDT.Columns.Add("END_DTTM", typeof(DateTime));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                //if (!string.IsNullOrWhiteSpace(sRackID)) drRQSTDT["MTRL_PORT_ID"] = sRackID;
                //if (!string.IsNullOrWhiteSpace(sMtrlID)) drRQSTDT["MTRLID"] = sMtrlID;
                //if (!string.IsNullOrWhiteSpace(sState)) drRQSTDT["REQ_STAT_CODE"] = sState;
                //if (!string.IsNullOrWhiteSpace(sBoxID)) drRQSTDT["REPACK_BOX_ID"] = sBoxID;
                //if (!string.IsNullOrWhiteSpace(sEqsgID)) drRQSTDT["EQSGID"] = sEqsgID;
                drRQSTDT["MTRL_PORT_ID"] = string.IsNullOrWhiteSpace(sRackID) || sRackID.Equals("ALL") ? null : sRackID;
                drRQSTDT["MTRLID"] = string.IsNullOrWhiteSpace(sMtrlID) ? null : sMtrlID;
                drRQSTDT["REQ_STAT_CODE"] = string.IsNullOrWhiteSpace(sState) || sState.Equals("ALL") ? null : sState;
                drRQSTDT["REPACK_BOX_ID"] = string.IsNullOrWhiteSpace(sBoxID) ? null : sBoxID;
                drRQSTDT["EQSGID"] = string.IsNullOrWhiteSpace(sEqsgID) || sEqsgID.Equals("ALL") ? null : sEqsgID;
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

        internal DataTable GetBoxHistorySummary(string sEqsgID, string sRackID, string sMtrlID)
        {
            string bizRuleName = "DA_MTRL_SEL_TB_SFC_PROD_RACK_MTRL_BOX_STCK_SUM";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtRQSTDT.Columns.Add("MTRLID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                //if (!string.IsNullOrWhiteSpace(sEqsgID)) drRQSTDT["EQSGID"] = sEqsgID;
                //if (!string.IsNullOrWhiteSpace(sRackID)) drRQSTDT["MTRL_PORT_ID"] = sRackID;
                //if (!string.IsNullOrWhiteSpace(sMtrlID)) drRQSTDT["MTRLID"] = sMtrlID;
                drRQSTDT["EQSGID"] = string.IsNullOrWhiteSpace(sEqsgID) || sEqsgID.Equals("ALL") ? null : sEqsgID;
                drRQSTDT["MTRL_PORT_ID"] = string.IsNullOrWhiteSpace(sRackID) || sRackID.Equals("ALL") ? null : sRackID;
                drRQSTDT["MTRLID"] = string.IsNullOrWhiteSpace(sMtrlID) ? null : sMtrlID;

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