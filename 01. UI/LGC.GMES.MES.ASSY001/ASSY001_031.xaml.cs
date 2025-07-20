/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_031 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        private UC_WORKORDER winWorkOrder = new UC_WORKORDER();
        string inputLot = string.Empty;
        DataTable dtMain = new DataTable();
        CurrentLotInfo cLot = new CurrentLotInfo();
        DateTime dtTime;
        TimeSpan spn;

        bool mbLoad = true;

        #region CurrentLotInfo
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _WIPSTAT = string.Empty;
        private string _txtShift = string.Empty;
        private string _txtWorker = string.Empty;
        private string _predver = string.Empty;
        private string _wipdttm = string.Empty;
        private string _wipnote = string.Empty;
        private string _resonqty = "0";
        private string _inputqty = string.Empty;
        private string _wipqty = string.Empty;
        private string _txtissue = string.Empty;
        private string cancelflag = string.Empty;
        private string _REMARK = string.Empty;
        private string _VERSION = string.Empty;
        #endregion

        private string _Unit = string.Empty;



        public ASSY001_031()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
      

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { Process.VD_LMN, LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboVDEquipment, cboCnfrmEqpt };
            _combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboLineChild, sFilter: sFilter);

            //공정
            String[] sFilter2 = { cboVDEquipmentSegment.SelectedValue.ToString() };
            //C1ComboBox[] cboLineChild1 = { cboVDEquipment, cboCnfrmEqpt };
            _combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, cbChild: cboLineChild, sFilter: sFilter2);

            String[] sFilter3 = { "ELEC_TYPE" };
            //C1ComboBox[] cboLineChild2 = { cboVDEquipment, cboCnfrmEqpt };
            _combo.SetCombo(cboEquipmentElec, CommonCombo.ComboStatus.NONE, cbChild: cboLineChild, sFilter: sFilter3, sCase: "COMMCODE");

            string sElect_Type = cboEquipmentElec.SelectedValue.ToString();            
            C1ComboBox[] cboEquipmentParent = { cboVDEquipmentSegment, cboEquipmentElec, cboVDProcess };              
            _combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.ALL,  cbParent: cboEquipmentParent, sCase: "EQUIPMENT_NT");
            _combo.SetCombo(cboCnfrmEqpt, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent, sCase: "EQUIPMENT_NT");

            mbLoad = false;


        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //GetProductLot();
            //ClearData();
            SearchData();
        }
        #endregion

        #region MainWindow
       

        #endregion



        #region Event


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            InitCombo();

            ApplyPermissions();
        }


        #endregion


        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

      
        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }


       private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //공정
            ////String[] sFilter = { cboVDEquipmentSegment.SelectedValue.ToString() };
            ////combo.SetCombo(cboVDProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
        }
        private void cboVDProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //설비
            ////String[] sFilter = { cboVDProcess.SelectedValue.ToString(), cboVDEquipmentSegment.SelectedValue.ToString() };
            ////combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            ////combo.SetCombo(cboCnfrmEqpt, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase : "cboVDEquipment");
        }

       
        private void SearchData()
        {
            try
            {
                ClearControls();
                GetEqptReserveList();
                GetWaitSkidInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 설비별 예약 현황
        /// </summary>
        private void GetEqptReserveList()
        {

            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("EQSGID", typeof(String));
            IndataTable.Columns.Add("ELEC_TYPE_CODE", typeof(String));            
            IndataTable.Columns.Add("LANGTYPE", typeof(String));
            IndataTable.Columns.Add("EQPTID", typeof(String));

            DataRow dr = IndataTable.NewRow();

            dr["EQSGID"] = Util.GetCondition(cboVDEquipmentSegment);
            dr["ELEC_TYPE_CODE"] = Util.GetCondition(cboEquipmentElec, "",true);            
            dr["LANGTYPE"] = LoginInfo.LANGID;
            dr["EQPTID"] = Util.GetCondition(cboVDEquipment, bAllNull: true);

            IndataTable.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_VD_EQPT_RSV_INFO", "RQSTDT", "RSLTDT", IndataTable);

            Util.GridSetData(dgEqpt, result, FrameOperation, true);
        }

        private void GetWaitSkidInfo()
        {


            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("EQSGID", typeof(String));
            IndataTable.Columns.Add("PROCID", typeof(String));
            IndataTable.Columns.Add("ELEC_TYPE_CODE", typeof(String));

            DataRow dr = IndataTable.NewRow();

            dr["EQSGID"] = Util.GetCondition(cboVDEquipmentSegment);
            dr["PROCID"] = Util.GetCondition(cboVDProcess);
            dr["ELEC_TYPE_CODE"] = Util.GetCondition(cboEquipmentElec, "", true);


            IndataTable.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_WAIT_SKID_INFO", "RQSTDT", "RSLTDT", IndataTable);

            Util.GridSetData(dgSkid, result, FrameOperation, true);
        }
        public void ClearControls()
        {
            

            try
            {
                Util.gridClear(dgEqpt);
                Util.gridClear(dgSkid);
                Util.gridClear(dgWait);
                Util.gridClear(dgEqptbyList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);            
            }
            
        }

        private int FindRow(C1DataGrid flex, string text, int startRow, int col, bool wrap)
        {
            int count = flex.Rows.Count;
            for (int off = startRow; off < count; off++)
            {
                // reached the bottom and not wrapping? quit now
                if (!wrap && startRow + off >= count)
                {
                    break;
                }

                // get text from row
                //int row = (startRow + off) ;
                string content = flex.GetCell(off, col).Text;

                // found? return row index
                if (content != "" &&
                    content.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return off;
                }
            }

            // not found...
            return -1;
        }

        private DataTable MadeCopyData()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("CHK", typeof(string));
            dt.Columns.Add("NO", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("EQPTNAME", typeof(string));
            dt.Columns.Add("SKIDID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("PJT", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("MODLID", typeof(string));
            dt.Columns.Add("WIPQTY", typeof(decimal));
            dt.Columns.Add("UNIT_CODE", typeof(string));
            dt.Columns.Add("REWORK_YN", typeof(string));
            dt.Columns.Add("REWORK_CNT", typeof(string));
            dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));

            return dt;
        }

        private DataTable CopyData()
        {
            DataTable dt = MadeCopyData();

            if(dgWait.GetRowCount() > 0)
                dt = (dgWait.ItemsSource as DataView).Table;
            
            return dt;
        }
        private void RowAdd(C1DataGrid dg, string sSkidID, int iEndCnt)
        {
            DataTable dt = CopyData();

            int iCol = dgSkid.Columns["SKIDID"].Index;
            int iStartRow = FindRow(dgSkid, sSkidID, 0, iCol, true);

            int iNo = 0;                
            int iCnt = dt.Rows.Count;

            if(iCnt > 0)
            {
                iNo = int.Parse(dt.Rows[iCnt - 1]["NO"].ToString());
            }

            string sRemCstID = "";
            for(int i = iStartRow; i <= iStartRow + (iEndCnt - 1); i++)
            {
                string sCSTID = dgSkid.GetCell(i, dgSkid.Columns["SKIDID"].Index).Text;
                if (sRemCstID != sCSTID)
                {
                    iNo++;
                }
                DataRow dr = dt.NewRow();
                dr["CHK"] = "0";
                dr["NO"] = iNo.ToString();
                dr["EQPTID"] = cboCnfrmEqpt.SelectedValue.ToString();
                dr["EQPTNAME"] = cboCnfrmEqpt.Text;
                dr["SKIDID"] = sCSTID;
                dr["LOTID"] = dgSkid.GetCell(i, dgSkid.Columns["LOTID"].Index).Text;
                dr["PJT"] = dgSkid.GetCell(i, dgSkid.Columns["PJT"].Index).Text;
                dr["PRODID"] = dgSkid.GetCell(i, dgSkid.Columns["PRODID"].Index).Text;
                dr["MODLID"] = dgSkid.GetCell(i, dgSkid.Columns["MODLID"].Index).Text;
                dr["WIPQTY"] = dgSkid.GetCell(i, dgSkid.Columns["WIPQTY"].Index).Text;
                dr["UNIT_CODE"] = dgSkid.GetCell(i, dgSkid.Columns["UNIT_CODE"].Index).Text;
                dr["REWORK_YN"] = dgSkid.GetCell(i, dgSkid.Columns["REWORK_YN"].Index).Text;
                dr["REWORK_CNT"] = dgSkid.GetCell(i, dgSkid.Columns["REWORK_CNT"].Index).Text;
                dr["EQPT_BTCH_WRK_NO"] = dgSkid.GetCell(i, dgSkid.Columns["EQPT_BTCH_WRK_NO"].Index).Text;

                dt.Rows.Add(dr);
                sRemCstID = sCSTID;
            }

            Util.GridSetData(dg, dt, FrameOperation, true);
        }

        private void RowRemove(C1DataGrid dg, string sSkidID)
        {
            DataTable dt = CopyData();

            DataRow[] dr = dt.Select("SKIDID = '" + sSkidID + "'");

            for(int i = 0; i < dr.Length; i++)
            {
                dt.Rows.Remove(dr[i]);
            }
            int iCnt = 0;
            string sRemCSTID = "";

            for(int i =0; i < dt.Rows.Count; i++)
            {
                string sCSTID = dt.Rows[i]["SKIDID"].ToString();
                if(sRemCSTID != sCSTID)
                    iCnt++;
                
                dt.Rows[i]["NO"] = iCnt;
                sRemCSTID = sCSTID;
            }

            Util.GridSetData(dg, dt, FrameOperation, true);
        }

        private void dgCheckBoxChecked(C1DataGrid dg )
        {
            if (dg.CurrentRow == null || dg.SelectedIndex == -1)
            {
                return;
            }

            int iCol = dg.CurrentColumn.Index;
            string sColName = dg.GetCell(dg.CurrentRow.Index, iCol).Column.Name;
            
            if (!sColName.Equals("CHK"))
                return;
            

            int iRow = 0;
            int iCurrRow = dg.CurrentRow.Index;
            
            string sCurrValue = Util.NVC(DataTableConverter.GetValue(dg.Rows[iCurrRow].DataItem, "CHK").ToString());
            bool bIn = false;

            for (int i = 0; i < dg.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);
            }


            if (sCurrValue.Equals("0") || sCurrValue.Equals("False"))    //선택한 경우
            {
                string sSkidID = dg.GetCell(iCurrRow, dg.Columns["SKIDID"].Index).Text;

                
                do   //선택한 SKIDID와 같은 Row를 찾아 CheckBox에 Check하기 위함.
                {

                    iRow = FindRow(dg, sSkidID, iRow, dg.Columns["SKIDID"].Index, true);
                    if (iRow >= 0)
                    {
                        DataTableConverter.SetValue(dg.Rows[iRow].DataItem, "CHK", true);

                        iRow++;
                        bIn = true;
                    }

                } while (iRow >= 0 && iRow < dg.GetRowCount());

                
            }
        }

        private void GetEqptReserveSkid(string sEqptID)
        {
            try
            {
                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("EQSGID", typeof(String));
                IndataTable.Columns.Add("ELEC_TYPE_CODE", typeof(String));
                IndataTable.Columns.Add("LANGTYPE", typeof(String));
                IndataTable.Columns.Add("EQPTID", typeof(String));
                IndataTable.Columns.Add("PROCID", typeof(String));

                DataRow dr = IndataTable.NewRow();

                dr["EQSGID"] = Util.GetCondition(cboVDEquipmentSegment);
                dr["ELEC_TYPE_CODE"] = Util.GetCondition(cboEquipmentElec, "", true);
                dr["LANGTYPE"] = LoginInfo.LANGID;
                dr["EQPTID"] = sEqptID;
                dr["PROCID"] = Util.GetCondition(cboVDProcess, bAllNull: true);

                IndataTable.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_RSV_EQPT_INFO", "RQSTDT", "RSLTDT", IndataTable);

                Util.GridSetData(dgEqptbyList, result, FrameOperation, true);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool Validation2Reserve(string sEqptID, string sProdID, string sEqpt_Btch_Wrk_NO)
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgEqpt.ItemsSource);

                if(dt.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU1498"); //데이터가 없습니다.
                    return false;
                }

                DataRow[] dr = dt.Select("EQPTID = '" + sEqptID + "'");

                if(dr.Length == 0)
                {
                    Util.MessageInfo("SFU1498"); //데이터가 없습니다.
                    return false;
                }

                if (dr[0]["AUTO_RSV_STOP_FLAG"].ToString() == "Y")
                {
                    Util.MessageInfo("선택한 설비는 예약 중지 중 입니다.");
                    return false;
                }

                if (dr[0]["EQPT_WRK_STOP_FLAG"].ToString() == "Y")
                {
                    Util.MessageInfo("선택한 설비는 설비 작업 정지 여부가 [Y] 입니다.");
                    return false;
                }

                if (int.Parse(dr[0]["PSB_CNT"].ToString()) == 0)
                {
                    Util.MessageInfo("선택한 설비에 예약 가능 수량이 [0] 입니다."); 
                    return false;
                }

                int iRsvPsbCnt = int.Parse(dr[0]["PSB_CNT"].ToString());

                int iRsvWtCnt = 0;
                int iWTDataCnt = dgWait.GetRowCount();
                if (iWTDataCnt > 0)
                {
                    int iCol = dgWait.Columns["NO"].Index;
                    iRsvWtCnt = int.Parse(dgWait.GetCell(iWTDataCnt - 1, iCol).Text);
                }
                

                if (iRsvPsbCnt == iRsvWtCnt)
                {

                    Util.MessageInfo("예약 가능 수량을 초과하여 선택할 수 없습니다.");
                    return false;
                }

                if (!string.IsNullOrEmpty(dr[0]["RESERVE_PRODID"].ToString()) && dr[0]["RESERVE_PRODID"].ToString() != sProdID)
                {
                    Util.MessageInfo("선택한 설비에 예약된 제품과 다른 제품 입니다.");
                    return false;
                }
                else if (string.IsNullOrEmpty(dr[0]["RESERVE_PRODID"].ToString()) && dgWait.GetRowCount() > 0)
                {
                    int iProdCol = dgWait.Columns["PRODID"].Index;
                    string sWaitProdID = dgWait.GetCell(0, iProdCol).Text;
                    if (sWaitProdID != sProdID)
                    {
                        Util.MessageInfo("예약대기 제품ID와 선택한 제품ID가 다릅니다.");
                        return false;
                    }
                }

                if(dgWait.GetRowCount() > 0 )
                {
                    DataTable dtWait = DataTableConverter.Convert(dgWait.ItemsSource);
                    string sWTEqpt_Btch_Wrk_NO = dtWait.Rows[0]["EQPT_BTCH_WRK_NO"].ToString();
                    if (!string.IsNullOrEmpty(sWTEqpt_Btch_Wrk_NO) && sWTEqpt_Btch_Wrk_NO != sEqpt_Btch_Wrk_NO)
                    {
                        Util.MessageInfo("재작업 예약은 같은 BATCH ID끼리 예약되어야 합니다.");
                        return false;
                    }
                    else if(string.IsNullOrEmpty(sWTEqpt_Btch_Wrk_NO) && !string.IsNullOrEmpty(sEqpt_Btch_Wrk_NO))
                    {
                        Util.MessageInfo("재작업 대상 SKID는 일반 SKID와 같이 작업할 수 없습니다.");
                        return false;
                    }

                    //dr = dtWait.Select("EQPT_BTCH_WRK_NO = '" + sEqpt_Btch_Wrk_NO + "'");

                }

                return true;
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void PopupOpen()
        {
            if (dgEqpt.GetRowCount() <= 0)
                return;
            if (_Util.GetDataGridCheckFirstRowIndex(dgEqpt, "CHK") < 0)
                return;

            ASSY001_031_ReserveStop wnPopup = new ASSY001_031_ReserveStop();
            wnPopup.FrameOperation = FrameOperation;

            if (wnPopup != null)
            {
                string sEqptID = Util.NVC(DataTableConverter.GetValue(dgEqpt.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEqpt, "CHK")].DataItem, "EQPTID"));
                string sEqptName = Util.NVC(DataTableConverter.GetValue(dgEqpt.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEqpt, "CHK")].DataItem, "EQPTNAME"));
                string sReservStop = Util.NVC(DataTableConverter.GetValue(dgEqpt.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEqpt, "CHK")].DataItem, "AUTO_RSV_STOP_FLAG"));
                string sEqptStop = Util.NVC(DataTableConverter.GetValue(dgEqpt.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEqpt, "CHK")].DataItem, "EQPT_WRK_STOP_FLAG"));

                object[] Parameters = new object[4];
                Parameters[0] = sEqptID;
                Parameters[1] = sEqptName;
                Parameters[2] = sReservStop;
                Parameters[3] = sEqptStop;


                C1WindowExtension.SetParameters(wnPopup, Parameters);

                wnPopup.Closed += new EventHandler(wnPopup_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wnPopup.ShowModal()));
                wnPopup.BringToFront();

            }
        }

        private int SetCheckBox2SameSkidID(C1DataGrid dg, string sSkidID, bool bValue)
        {
            int iRow = 0;
            int iSameRowCnt =0;
            do   //선택한 SKIDID와 같은 Row를 찾아 CheckBox에 Check하기 위함.
            {

                iRow = FindRow(dg, sSkidID, iRow, dg.Columns["SKIDID"].Index, true);
                if (iRow >= 0)
                {
                    DataTableConverter.SetValue(dg.Rows[iRow].DataItem, "CHK", bValue);
                    iSameRowCnt++;
                    iRow++;
                }

            } while (iRow >= 0 && iRow < dg.GetRowCount());

            return iSameRowCnt;
        }
        #endregion

        private void ClearData()
        {
            _LOTID = "";
            _EQPTID = "";
            _WIPSTAT = "";
            _txtShift = "";
            _txtWorker = "";
            _predver = "";
            _wipdttm = "";
            _wipnote = "";
            _inputqty = "";
            _wipqty = "";
            _txtissue = "";
            cancelflag = "";
            _REMARK = "";
            _VERSION = "";

        }

        private void cboVDEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboCnfrmEqpt.SelectedIndex = cboVDEquipment.SelectedIndex; //SelectedIndex 연결
        }

        private void cboCnfrmEqpt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            

        }

        private void dgEqpt_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

        }

        private void dgEqpt_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
            if (dgEqpt.CurrentRow == null || dgEqpt.SelectedIndex == -1 || dgEqpt.CurrentColumn == null )
            {
                return;
            }

            int iCol = dgEqpt.CurrentColumn.Index;
            string sColName = dgEqpt.GetCell(dgEqpt.CurrentRow.Index, dgEqpt.CurrentColumn.Index).Column.Name;

            if (!sColName.Equals("CHK"))
                return;

            string sEqptID = "";
            for (int i = 0; i < dgEqpt.GetRowCount(); i++)
            {
                if (i == dgEqpt.CurrentRow.Index)
                {
                    if (Util.NVC(dgEqpt.GetCell(dgEqpt.CurrentRow.Index, dgEqpt.Columns["CHK"].Index).Value) == "1")
                    {
                        DataTableConverter.SetValue(dgEqpt.Rows[i].DataItem, "CHK", false);
                        Util.gridClear(dgEqptbyList);
                        sEqptID = "";
                        mbLoad = true;
                        cboCnfrmEqpt.SelectedIndex = 0;
                        mbLoad = false;
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgEqpt.Rows[i].DataItem, "CHK", true);
                        int iEqptCol = dgEqpt.Columns["EQPTID"].Index;
                        sEqptID = dgEqpt.GetCell(i, iEqptCol).Text;
                        GetEqptReserveSkid(sEqptID);
                        cboCnfrmEqpt.SelectedValue = sEqptID;
                    }
                }
                else
                {
                    DataTableConverter.SetValue(dgEqpt.Rows[i].DataItem, "CHK", false);
                }                
            }

            
        }

        private void dgSkid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgSkid.CurrentRow == null || dgSkid.SelectedIndex == -1)
            {
                return;
            }

            if (dgSkid.GetRowCount() == 0)
                return;

            int iCol = dgSkid.CurrentColumn.Index;
            string sColName = dgSkid.GetCell(dgSkid.CurrentRow.Index, iCol).Column.Name;

            if (!sColName.Equals("CHK"))
                return;

            if (string.IsNullOrEmpty(Util.GetCondition(cboCnfrmEqpt, "", true)))
            {
                Util.MessageValidation("SFU1673");      //설비를 선택하세요.
                return;
            }

            int iRow = 0;
            int iCurrRow = dgSkid.CurrentRow.Index;

            //if (_Util.GetDataGridCheckFirstRowIndex(dgEqptbyList, "CHK") == -1)
            string sCurrValue = Util.NVC(DataTableConverter.GetValue(dgSkid.Rows[iCurrRow].DataItem, "CHK").ToString());

            string sSkidID = dgSkid.GetCell(iCurrRow, dgSkid.Columns["SKIDID"].Index).Text; // (string.IsNullOrEmpty(dgSkid.GetCell(iCurrRow, dgSkid.Columns["SKIDID"].Index).Text.ToString()) ? "" : dgSkid.GetCell(iCurrRow, dgSkid.Columns["SKIDID"].Index).Value.ToString());
            string sProdID = dgSkid.GetCell(iCurrRow, dgSkid.Columns["PRODID"].Index).Text; 
            string sEqpt_Btch_Wrk_NO = dgSkid.GetCell(iCurrRow, dgSkid.Columns["EQPT_BTCH_WRK_NO"].Index).Text;
            if ((sCurrValue.Equals("0") || sCurrValue.Equals("False")) && dgSkid.CurrentColumn.Index == 0)    //선택한 경우
            {
                if(!Validation2Reserve(cboCnfrmEqpt.SelectedValue.ToString(), sProdID, sEqpt_Btch_Wrk_NO))
                {
                    return;
                }

                #region [중복 Check]
                DataTable dt = CopyData();
                DataRow[] dr = dt.Select("SKIDID = '" + sSkidID + "'");
                if (dr.Length > 0)
                {
                    Util.MessageValidation("SFU2051", new object[] { "[SKID ID: " + sSkidID + "]" });  //중복 데이터가 존재 합니다. %1
                    return;
                }
                #endregion [중복 Check]


                int iAddCnt =  SetCheckBox2SameSkidID(dgSkid, sSkidID, true);
                RowAdd(dgWait, sSkidID, iAddCnt);
                
            }
            else
            {
                SetCheckBox2SameSkidID(dgSkid, sSkidID, false);
                RowRemove(dgWait, sSkidID);
            }

           
        }

        private void btnSelDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgWait, "CHK") == -1)
                {                
                    return;
                }

                DataRow[] dr = null;
            
                DataTable dtChk = DataTableConverter.Convert(dgWait.ItemsSource);
                if (dtChk.Columns.Contains("CHK"))
                {
                    dr = dtChk.Select("CHK" + " = 'True'");
                }

                for (int i = 0; i < dr.Length; i++)
                {
                    dtChk.Rows.Remove(dr[i]);
                }


                Util.GridSetData(dgWait, dtChk, FrameOperation, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgWait_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dgCheckBoxChecked(dgWait);
        }
        
        private void btnCancelRSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgEqptbyList, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1632");
                    // Util.Alert("SFU1632");  //선택된 LOT이 없습니다.
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgEqptbyList.ItemsSource);
                if (dt.Rows.Count == 0) return;

                //if (dt.Select("CHK = 0").Count() != 0)
                //{
                //    Util.MessageValidation("LOT 전체 선택 후 진행");//모든 LOT을 선택하세요
                //    return;
                //}

                string sEqptID = dt.Rows[0]["EQPTID"].ToString(); ;

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = "OFF";
                row["EQPTID"] = sEqptID;
                row["USERID"] = LoginInfo.USERID;
                row["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                indataSet.Tables["IN_EQP"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgEqptbyList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEqptbyList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgEqptbyList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgEqptbyList.Rows[i].DataItem, "LOTID"));
                        indataSet.Tables["IN_LOT"].Rows.Add(row);
                    }
                }              
                
                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_RESERVE_LOT", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_CANCEL_RESERVE_LOT", bizException.Message, bizException.ToString());
                                return;
                            }
                                                        
                            GetEqptReserveList();
                            GetWaitSkidInfo();
                            Util.MessageInfo("SFU1736");
                            //Util.AlertInfo("SFU1736");  //예약취소 완료
                            int iEqptCol = dgEqpt.Columns["EQPTID"].Index;
                            int iRow = FindRow(dgEqpt, sEqptID, 0, iEqptCol, true);
                            if (iRow >= 0)
                            {
                                DataTableConverter.SetValue(dgEqpt.Rows[iRow].DataItem, "CHK", true);
                                GetEqptReserveSkid(sEqptID);
                            }
                            else
                            {
                                Util.gridClear(dgEqptbyList);
                            }

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            //Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, indataSet);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.MessageException(ex);
            }
            finally
            {
                //chkAll.IsChecked = false;
            }
        }

        private void dgEqptbyList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgEqptbyList.CurrentRow == null || dgEqptbyList.SelectedIndex == -1)
            {
                return;
            }

            if (dgEqptbyList.GetRowCount() == 0)
                return;

            int iCol = dgEqptbyList.CurrentColumn.Index;
            string sColName = dgEqptbyList.GetCell(dgEqptbyList.CurrentRow.Index, iCol).Column.Name;

            if (!sColName.Equals("CHK"))
                return;

            int iRow = 0;
            int iCurrRow = dgEqptbyList.CurrentRow.Index;
            
            string sCurrValue = Util.NVC(DataTableConverter.GetValue(dgEqptbyList.Rows[iCurrRow].DataItem, "CHK").ToString());

            string sSkidID = dgEqptbyList.GetCell(iCurrRow, dgEqptbyList.Columns["SKIDID"].Index).Text; 
            
            if ((sCurrValue.Equals("0") || sCurrValue.Equals("False")) && dgEqptbyList.CurrentColumn.Index == 0)    //선택한 경우
            {
                SetCheckBox2SameSkidID(dgEqptbyList, sSkidID, true);
            }
            else
            {
                SetCheckBox2SameSkidID(dgEqptbyList, sSkidID, false);
               
            }
            //dgCheckBoxChecked(dgEqptbyList);
        }

        private void btnRsrv_Click(object sender, RoutedEventArgs e)
        {
            DataSet indataSet = new DataSet();
            string sEqptID = "";
            try
            {
                DataTable dt = DataTableConverter.Convert(dgWait.ItemsSource);
                if (dt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1632");
                    // Util.Alert("SFU1632");  //선택된 LOT이 없습니다.
                    return;
                }

                //string sEqptID = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWait, "CHK")].DataItem, "EQPTID"));
                //string sProdID = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWait, "CHK")].DataItem, "PRODID"));
                //string sCSTID = Util.NVC(DataTableConverter.GetValue(dgWait.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWait, "CHK")].DataItem, "SKIDID"));

                sEqptID = dt.Rows[0]["EQPTID"].ToString();
                string sProdID = dt.Rows[0]["PRODID"].ToString();
                string sCSTID = dt.Rows[0]["SKIDID"].ToString();

                //if (!Validation2Reserve(sEqptID, sProdID))
                //    return;

                
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));            
                inData.Columns.Add("PROCID", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = "OFF";
                row["EQPTID"] = sEqptID;
                row["USERID"] = LoginInfo.USERID;            
                row["PROCID"] = Convert.ToString(cboVDProcess.SelectedValue);
                indataSet.Tables["IN_EQP"].Rows.Add(row);

                string sRemSkidID = "";
                DataTable inLot = indataSet.Tables.Add("IN_INPUT");
                inLot.Columns.Add("CSTID", typeof(string));

                int iSuccessCnt = 0;
                int iNGCnt = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sCSTID = dt.Rows[i]["SKIDID"].ToString();
                    if (sRemSkidID != sCSTID)
                    {
                        
                        row = inLot.NewRow();
                        row["CSTID"] = sCSTID;
                        indataSet.Tables["IN_INPUT"].Rows.Add(row);

                        loadingIndicator.Visibility = Visibility.Visible;

                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_VD_EQPT_RESERVE", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);
                        if(dsResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString() == "OK")
                        {
                            RowRemove(dgWait, sCSTID);
                            iSuccessCnt++;
                            indataSet.Tables["IN_INPUT"].Rows.RemoveAt(0);
                        }
                        else
                        {
                            iNGCnt++;

                        }
                    }
                    sRemSkidID = sCSTID;
                }

                if(iNGCnt == 0)
                {
                    Util.MessageInfo("SFU2849");  //예약완료
                }
                else
                {
                    string sMSG = "예약성공[" + iSuccessCnt.ToString() + "]\r\n예약실패[" + iNGCnt.ToString() + "]"  ;
                    Util.MessageInfo(sMSG);
                }

                //if (bizException != null)
                //{
                //    Util.AlertByBiz("BR_PRD_REG_VD_EQPT_RESERVE", bizException.Message, bizException.ToString());
                //    return;
                //}

                //if (bizResult != null)
                //{
                //    if (bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString().Equals("OK"))
                //    {

                //        Util.MessageInfo("SFU2849");  //예약완료
                //                                      //Util.AlertInfo("SFU1736");  //예약취소 완료
                //        GetEqptReserveList();
                //        GetWaitSkidInfo();
                //        RowRemove(dgWait, sCSTID);
                //    }
                //    else
                //    {
                //        Util.MessageInfo(bizResult.Tables["OUTDATA"].Rows[0]["MESSAGE"].ToString());
                //    }
                //}

                

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                GetEqptReserveList();
                GetWaitSkidInfo();
                loadingIndicator.Visibility = Visibility.Collapsed;

                int iEqptCol = dgEqpt.Columns["EQPTID"].Index;
                int iRow = FindRow(dgEqpt, sEqptID, 0, iEqptCol, true);
                mbLoad = true;
                if (iRow >= 0)
                {
                    DataTableConverter.SetValue(dgEqpt.Rows[iRow].DataItem, "CHK", true);
                    GetEqptReserveSkid(sEqptID);

                    
                    cboCnfrmEqpt.SelectedIndex = iRow;
                }
                else
                {
                    cboCnfrmEqpt.SelectedIndex = 0;
                    Util.gridClear(dgEqptbyList);
                }

                mbLoad = false;

            }


        }

        private void cboCnfrmEqpt_SelectedValueChanged_1(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!mbLoad)
            {
                Util.gridClear(dgWait);
                
                if (dgEqpt.GetRowCount() > 0 )
                {
                    bool bEqual = false;
                    string scboEqptID = cboCnfrmEqpt.SelectedValue.ToString();// Util.NVC(DataTableConverter.GetValue(dgEqpt.Rows[_Util.GetDataGridCheckFirstRowIndex(dgEqpt, "CHK")].DataItem, "EQPTID"));
                    int iEqptCol = dgEqpt.Columns["EQPTID"].Index;

                    int iEqptRow = _Util.GetDataGridCheckFirstRowIndex(dgEqpt, "CHK");
                    if (iEqptRow != -1)
                    {
                        string sEqptID = dgEqpt.GetCell(iEqptRow, iEqptCol).Text;

                        bEqual = (sEqptID == scboEqptID);
                        
                    }

                    if (!bEqual)
                    {
                        for (int i = 0; i < dgEqpt.GetRowCount(); i++)
                        {
                            DataTableConverter.SetValue(dgEqpt.Rows[i].DataItem, "CHK", false);
                        }
                        int iRow = FindRow(dgEqpt, scboEqptID, 0, iEqptCol, true);
                        if (iRow >= 0)
                        {
                            DataTableConverter.SetValue(dgEqpt.Rows[iRow].DataItem, "CHK", true);
                            GetEqptReserveSkid(scboEqptID);
                        }
                        else
                        {
                            Util.gridClear(dgEqptbyList);
                        }
                    }
                    Util.gridClear(dgSkid);
                    GetWaitSkidInfo();
                }
                
            }
        }
        

        private void wnPopup_Closed(object sender, EventArgs e)
        {
            ASSY001_031_ReserveStop window = sender as ASSY001_031_ReserveStop;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetEqptReserveList();
            }

            
        }
        private void btnEqptStop_Click(object sender, RoutedEventArgs e)
        {
            PopupOpen();
        }

        private void btnSkidMapping_Click(object sender, RoutedEventArgs e)
        {
          /*  ASSY001_031_SKDPNMAPPING wnPopup = new ASSY001_031_SKDPNMAPPING();
            wnPopup.FrameOperation = FrameOperation;
            wnPopup.Show();*/
        }
    }
}
