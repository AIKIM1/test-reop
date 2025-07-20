/*************************************************************************************
 Created Date : 2019.03.25
      Creator : 김도형
   Decription : 레진 투입
--------------------------------------------------------------------------------------
 [Change History]
  2019.03.25 김도형 : Initial Created.  25번 화면 참고
  2019.06.11 남기운 [CSR ID:3995063] CWA Pack Resin 투입이력 관리 및 ERP Push처리 | [요청번호]C20190515_95063
  2019.06.13 남기운 그리드 항목 수정




 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
//using System.Windows.Forms;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_040 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        private DataSet dsResult = new DataSet();
        private C1.WPF.DataGrid.DataGridRow drSelectedDataGrid;
        int iwoListIndex;
        String globalEQPTID = "";

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_040()
        {
            InitializeComponent();

            tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            InitCombo();
        }


        #endregion

        #region Initialize
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //왼쪽
            C1ComboBox cboTargetArea = new C1ComboBox();
            cboTargetArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //라인
            C1ComboBox[] cboTargetEQSGIDParent = { cboTargetArea };
            _combo.SetCombo(cboTargetEQSGID, CommonCombo.ComboStatus.SELECT, cbParent: cboTargetEQSGIDParent,  sCase: "EQUIPMENTSEGMENT");

            //  C1ComboBox[] cboTargetEQSGIDChild = { cboTargetProcessSegmentByEqsgid };
            //  _combo.SetCombo(cboTargetEQSGID, CommonCombo.ComboStatus.SELECT, cbParent: cboTargetEQSGIDParent, cbChild: cboTargetEQSGIDChild, sCase: "EQUIPMENTSEGMENT");

            //공정
            //C1ComboBox[] cboTargetProcessSegmentByEqsgidParent = { cboTargetEQSGID };
            //_combo.SetCombo(cboTargetProcessSegmentByEqsgid, CommonCombo.ComboStatus.SELECT, cbParent: cboTargetProcessSegmentByEqsgidParent, sCase: "PROCESS");

            //오른쪽
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
           // C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT,  cbParent: cboEquipmentSegmentParent);

            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            //C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);

            C1ComboBox[] cboProcessChild = { cboEquipment };
            string strCase = string.Empty;
            strCase = "cboProcessPack";
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, sCase:strCase );


            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);
            //초기화
            if (cboTargetProcessSegmentByEqsgid.Items.Count == 1)
                cboTargetProcessSegmentByEqsgid.SelectedIndex = 0;
            else if (cboTargetProcessSegmentByEqsgid.Items.Count > 1)
            {
                cboTargetProcessSegmentByEqsgid.SelectedIndex = 1;
                txtLotID.Focus();
            }

            //SelectedProc();
        }

        #endregion

        #region Event
        //조회 버튼 눌렀을 때
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
 

            GetList_BR();

            txtLotIdR.Text = "";
            txtJjcodeR.Text = "";
            txtWorkOrderR.Text = "";
            txtInpitDate.Text = "";
      
        }

        //투입 Lot ID 입력 후 엔터 눌럿을 때 dgTagetList에 투입한 정보 나오게 하기.
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            //Keydown일 때 메소드 호출

            try
            {
                if (txtLotID.Text.Length > 0)
                {                                     
                    if (e.Key == Key.Enter)
                    {

                        txtLotID.Text = txtLotID.Text.Trim();

                        if(!txtLotID.Text.ToString().Equals(""))
                           GetResinList(true, txtLotID.Text.Trim());

                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
                      
        }

        private void dgTagetList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //아마도 안씀?
        }

        private void dgTagetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //아마도 안씀?
        }

        //투입 취소 버튼 눌렀을 때
        private void btnTagetInputCancel_Click(object sender, RoutedEventArgs e)
        {
       
            
             try
             {
                 if (txtLotIdR.Text.Length == 0)
                 {
                     return;
                 }
                 if (txtJjcodeR.Text.Length == 0)
                 {
                     return;
                 }
                 if (txtWorkOrderR.Text.Length == 0)
                 {
                     return;
                 }

                 LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1982"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                 //투입을 취소 하시겠습니까?
                 {
                     if (result == MessageBoxResult.OK)
                     {
                         ResinInputCancel(txtLotIdR.Text);
                     }
                 }
                  );

             }
             catch (Exception ex)
             {
                 Util.Alert(ex.ToString());
             }
             
        }

        //투입 버튼 눌렀을 때
        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            int nSelectRow;
            try
            {
                nSelectRow = getCHKGrid();
                if(nSelectRow >= 0)
                {
                    //ResinInputStart();
                    ResinInputStart(dgTagetList.Rows[nSelectRow]);
                } else
                {
                    ms.AlertWarning("10008"); //선택된 데이터가 없습니다.

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //아마도 안씀?
        }


        /*
         * MouseDoubleClick="dgSearchResultList_MouseDoubleClick" CurrentCellChanged="dgSearchResultList_CurrentCellChanged"
        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //더블 클릭하면 하단 내용 체인지

            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string tLotid = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "INPUT_BCD_ID"));
                    string tJcode = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "MTRLID"));
                    string tWorkorder = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "WOID"));
                    string tDateTime = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "INPUT_DTTM"));

                    txtLotIdR.Text = tLotid;
                    txtJjcodeR.Text = tJcode;
                    txtWorkOrderR.Text = tWorkorder;

                    txtInpitDate.Text = tDateTime;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }


        private void dgSearchResultList_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

            try
            {

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    int rowindex = e.Cell.Row.Index;
                    //Util.Alert(Convert.ToString(rowindex));
                    if (rowindex >= 0)
                    {
                        string tLotid = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[rowindex].DataItem, "INPUT_BCD_ID"));
                        string tJcode = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[rowindex].DataItem, "MTRLID"));
                        string tWorkorder = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[rowindex].DataItem, "WOID"));
                        string tDateTime = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[rowindex].DataItem, "INPUT_DTTM"));

                        txtLotIdR.Text = tLotid;
                        txtJjcodeR.Text = tJcode;
                        txtWorkOrderR.Text = tWorkorder;
                        txtInpitDate.Text = tDateTime;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        

        }
        */

        private void cboTargetEQSGID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            clearLotInfoText_Left();

            cboTargetProcessSegmentByEqsgid.DisplayMemberPath = "PROCNAME";
            cboTargetProcessSegmentByEqsgid.SelectedValuePath = "PROCID";

            string sEqsgID = Util.NVC(cboTargetEQSGID.SelectedValue);
            DataTable dtWoInfo = getSelectedProc(sEqsgID);

            DataRow dr = dtWoInfo.NewRow();
            dr["PROCNAME"] = "-SELECT-";
            dr["PROCID"] = "";
            dtWoInfo.Rows.InsertAt(dr, 0);

            if (dtWoInfo != null)
            {
                if (dtWoInfo.Rows.Count > 0)
                {
                     cboTargetProcessSegmentByEqsgid.ItemsSource = dtWoInfo.Copy().AsDataView();
                    if (dtWoInfo.Rows.Count == 1)
                        cboTargetProcessSegmentByEqsgid.SelectedIndex = 0;
                    else if (dtWoInfo.Rows.Count > 1)
                        cboTargetProcessSegmentByEqsgid.SelectedIndex = 1;

                }

            }
        }


        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            /*
            cboProcess.DisplayMemberPath = "PROCNAME";
            cboProcess.SelectedValuePath = "PROCID";

            string sEqsgID = Util.NVC(cboEquipmentSegment.SelectedValue);


            DataTable dtWoInfo = getSelectedProc(sEqsgID);

            DataRow dr = dtWoInfo.NewRow();
            dr["PROCNAME"] = "-ALL-";
            dr["PROCID"] = "";
            dtWoInfo.Rows.InsertAt(dr, 0);

            if (dtWoInfo != null)
            {
                if (dtWoInfo.Rows.Count > 0)
                {
                 //   Util.Alert(sEqsgID);
                    cboProcess.ItemsSource = dtWoInfo.Copy().AsDataView();
                    if (dtWoInfo.Rows.Count == 1)
                        cboProcess.SelectedIndex = 0;
                    else if (dtWoInfo.Rows.Count > 1)
                        cboProcess.SelectedIndex = 1;

                }

            }
            */
            SelectedProc();
        }
        private void SelectedProc()
        {
            cboProcess.DisplayMemberPath = "PROCNAME";
            cboProcess.SelectedValuePath = "PROCID";

            string sEqsgID = Util.NVC(cboEquipmentSegment.SelectedValue);


            DataTable dtWoInfo = getSelectedProc(sEqsgID);

            DataRow dr = dtWoInfo.NewRow();
            dr["PROCNAME"] = "-ALL-";
            dr["PROCID"] = "";
            dtWoInfo.Rows.InsertAt(dr, 0);

            if (dtWoInfo != null)
            {
                if (dtWoInfo.Rows.Count > 0)
                {
                    //   Util.Alert(sEqsgID);
                    cboProcess.ItemsSource = dtWoInfo.Copy().AsDataView();
                    if (dtWoInfo.Rows.Count == 1)
                        cboProcess.SelectedIndex = 0;
                    else if (dtWoInfo.Rows.Count > 1)
                        cboProcess.SelectedIndex = 1;

                }

            }

        }


        private void cboTargetProcessSegmentByEqsgid_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            clearLotInfoText_Left();

            txtReworkWOID.Text = "";

            string sEqsgID = Util.NVC(cboTargetEQSGID.SelectedValue);
            string sProcid = Util.NVC(cboTargetProcessSegmentByEqsgid.SelectedValue);
            string sRouteid = string.Empty;
            string sWorkorder = string.Empty;

           

            DataTable dtWoInfo =  getSelectedWoInfo(sEqsgID, sProcid, sRouteid);

            if (dtWoInfo != null)
            {
                if (dtWoInfo.Rows.Count > 0)
                {

                    sWorkorder = Util.NVC(dtWoInfo.Rows[0]["WOID"]);
                    txtReworkWOID.Text = sWorkorder;
                    txtLotID.Focus();

                }
            }
        }
        #endregion

        #region Method

        public void GetResinList(bool bMainLot_SubLot_Flag, string sLotid)
        {
            // 투입보고 테이블 호출
            DataSet dsResult = null;


            string sEQSGID = Util.NVC(cboTargetEQSGID.SelectedValue);
            string sPROCID = Util.NVC(cboTargetProcessSegmentByEqsgid.SelectedValue);
            string sWOID = Util.NVC(txtReworkWOID.Text);

            if(sWOID.Equals(""))
            { 
                 ms.AlertWarning("100876"); //Work Order가 존재하지 않습니다
                return;
            }

             
            try
            {
                if (bMainLot_SubLot_Flag)
                {
                    //clearLotInfoText();
                    clearLotInfoText_Left();
                }

                txtLotID.Text = sLotid;
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("LOTPID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("MTRLID", typeof(string));
                INDATA.Columns.Add("MTRLCOUNT", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
  
                dr["WOID"] = sWOID;
                dr["PROCID"] = sPROCID;
                dr["EQSGID"] = sEQSGID;

                dr["LOTPID"] = sLotid;
                dr["LOTID"] = "";
                dr["MTRLID"] = "";
                dr["MTRLCOUNT"] = "";

                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_RESIN_MMD_EQPT_LIST", "INDATA", "RSLTDT", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    setLotInfoText(dsResult.Tables["RSLTDT"]);
                   // txtReworkWOID.Text = Util.NVC(dr["WOID"]);
                }
                DataTable dt = dsResult.Tables["RSLTDT"];

                Util.GridSetData(dgTagetList, dt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                
               
                Util.MessageException(ex);

                //txtLotID.Text = "";
                txtLotID.Focus();
               
            }



        }


        public void GetList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {

                    Util.MessageValidation("SFU2042", "31");   //기간은 {0}일 이내 입니다.
                    return;
                }

                ShowLoadingIndicator();
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("DATE_FROM", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("BCD_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                // 동을 선택하세요.
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                // 라인을선택하세요
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                if (dr["EQSGID"].Equals("")) return;

                // 공정을선택하세요.
                // dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                // if (dr["PROCID"].Equals("")) return;

                // 설비를 선택하세요.
                // dr["EQPTID"] = Util.GetCondition(cboEquipment, MessageDic.Instance.GetMessage("SFU1673"));
                // if (dr["EQPTID"].Equals("")) return;


                dr["PROCID"] = Util.GetCondition(cboProcess);
                if (dr["PROCID"].Equals("")) dr["PROCID"] = null;


                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                if (dr["EQPTID"].Equals("")) dr["EQPTID"] = null;

                dr["DATE_FROM"] = Util.GetCondition(dtpDateFrom);
                dr["DATE_TO"] = Util.GetCondition(dtpDateTo);
                dr["BCD_ID"] = txtFINDLot.Text.ToString().Trim();



                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESIN_INPUTLIST", "INDATA", "OUTDATA", dtRqst);

                
                Util.GridSetData(dgSearchResultList, dtRslt, FrameOperation, true);
              
                tbSearchListCount.Text = "[ "+ dgSearchResultList.GetRowCount().ToString() + " 건]";

                txtLotIdR.Text = "";
                txtJjcodeR.Text = "";
                txtWorkOrderR.Text = "";
                txtInpitDate.Text = "";

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

        public void GetList_BR()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }
                ShowLoadingIndicator();


                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("DATE_FROM", typeof(string));
                INDATA.Columns.Add("DATE_TO", typeof(string));
                INDATA.Columns.Add("BCD_ID", typeof(string));
                

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                // 동을 선택하세요.
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                // 라인을선택하세요
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                if (dr["EQSGID"].Equals("")) return;

                // 공정을선택하세요.
                // dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                // if (dr["PROCID"].Equals("")) return;

                // 설비를 선택하세요.
                // dr["EQPTID"] = Util.GetCondition(cboEquipment, MessageDic.Instance.GetMessage("SFU1673"));
                // if (dr["EQPTID"].Equals("")) return;


                dr["PROCID"] = Util.GetCondition(cboProcess);
                if (dr["PROCID"].Equals("")) dr["PROCID"] = null;


                dr["EQPTID"] = Util.GetCondition(cboEquipment);
                if (dr["EQPTID"].Equals("")) dr["EQPTID"] = null;

                dr["DATE_FROM"] = Util.GetCondition(dtpDateFrom);
                dr["DATE_TO"] = Util.GetCondition(dtpDateTo);

                dr["BCD_ID"] = txtFINDLot.Text.Trim().ToString();

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);



                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_RESIN_INPUTLIST", "INDATA", "RSLTDT", dsInput, null);

                /*
                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    //setLotInfoText(dsResult.Tables["RSLTDT"]);
                }
                */

                DataTable dt = dsResult.Tables["RSLTDT"];
                Util.GridSetData(dgSearchResultList, dt, FrameOperation, true);
                tbSearchListCount.Text = "[ " + dgSearchResultList.GetRowCount().ToString() + " 건]";

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

        private void clearLotInfoText()
        {
            try
            {
                jjcode.Text = "";
                jjamount.Text = "";
                txtLotIdR.Text = "";
                txtJjcodeR.Text = "";
                txtWorkOrderR.Text = "";
                txtInpitDate.Text = "";

                Util.gridClear(dgTagetList);
                Util.gridClear(dgSearchResultList);
                tbSearchListCount.Text = "[ 0 건 ]";

                 

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void clearLotInfoText_Left()
        {
            try
            {
                jjcode.Text = "";
                jjamount.Text = "";

                txtLotID.Text = "";

                Util.gridClear(dgTagetList);

            }
            catch (Exception ex)
            {
                throw ex;
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

        private void DoEvents()
        {
            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        //private void ResinInputStart()
        private void ResinInputStart(C1.WPF.DataGrid.DataGridRow drDataGrid)
        {
            try
            {
   
                string sEQPTID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "EQPTID"));
                string sLOTID  = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "LOTPID"));
                string sMTRLID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "MTRLID"));


                //dgTagetList.GetValue(drDataGrid.DataItem, "EQPTID")

                string sEQSGID = Util.NVC(cboTargetEQSGID.SelectedValue);
                string sPROCID = Util.NVC(cboTargetProcessSegmentByEqsgid.SelectedValue);
                string sWOID = Util.NVC(txtReworkWOID.Text);


                

                if (!(sLOTID.Length > 0))
                {
                    ms.AlertWarning("SFU1366"); //LOT ID를 입력해주세요
                    return;
                }

                //BR_PRD_REG_RESIN_INPUT

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("MTRLID", typeof(string));
                INDATA.Columns.Add("INPUT_BCD_ID", typeof(string));
                INDATA.Columns.Add("QTY", typeof(string));
                INDATA.Columns.Add("INPUT_NOTE", typeof(string));
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("WORKUSER", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = sEQSGID;
                dr["PROCID"] = sPROCID;
                dr["WOID"] = sWOID;
                dr["EQPTID"] = sEQPTID;
                dr["MTRLID"] = sMTRLID;
                dr["INPUT_BCD_ID"] = sLOTID;
                dr["QTY"] = jjamount.Text;
                dr["INPUT_NOTE"] = "";
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["WORKUSER"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RESIN_INPUT", "INDATA", "", dsInput, null);


                Util.MessageInfo("SFU1275"); //정상처리되었습니다.

                clearLotInfoText_Left();
                //Util.gridClear(dgTagetList);
                txtLotID.Text = "";

            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        public void ResinInputCancel(string sLotid)
        {
            try
            {
                if (!(sLotid.Length > 0))
                {
                    ms.AlertWarning("SFU1366"); //LOT ID를 입력해주세요
                    return;
                }

                //BR_PRD_REG_RESIN_INPUT

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("INPUT_DATE", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("MTRLID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("WORKUSER", typeof(string));

                DataRow dr = INDATA.NewRow();

                dr["INPUT_DATE"] = txtInpitDate.Text;
                dr["EQPTID"] = null;//"P1QP4";
                dr["MTRLID"] = txtJjcodeR.Text;
                dr["LOTID"] = txtLotIdR.Text;
                dr["WOID"] = txtWorkOrderR.Text;
                dr["WORKUSER"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RESIN_INPUT_CANCEL", "INDATA", "", dsInput, null);

                GetList();

                Util.MessageInfo("SFU1275"); //정상처리되었습니다.

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_REWORK_START_LOT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        #endregion

        private void dgResinInputList_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                iwoListIndex = Util.gridFindDataRow(ref dgTagetList, "CHK", "1", false);

                if (iwoListIndex == -1)
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgResinInputListChoice_Unchecked(object sender, RoutedEventArgs e)
        {

        }


        private void dgSearchResinList_Checked(object sender, RoutedEventArgs e)
        {
            try
            {

                int rowindex = getCHKResinGrid();
                if (rowindex >= 0)
                {
                    string tLotid = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[rowindex].DataItem, "INPUT_BCD_ID"));
                    string tJcode = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[rowindex].DataItem, "MTRLID"));
                    string tWorkorder = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[rowindex].DataItem, "WOID"));
                    string tDateTime = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[rowindex].DataItem, "INPUT_DTTM"));

                    txtLotIdR.Text = tLotid;
                    txtJjcodeR.Text = tJcode;
                    txtWorkOrderR.Text = tWorkorder;
                    txtInpitDate.Text = tDateTime;

                   // Util.Alert(DataTableConverter.GetValue(dgSearchResultList.Rows[nIndex].DataItem, "INPUT_BCD_ID").ToString());
                }
         
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private void dgWorkOrderList_Checked(object sender, RoutedEventArgs e)
        {
            try
            {


            }
            catch (Exception ex)
            {
              
                Util.MessageException(ex);
            }
        }

        private void dgWorkOrderListChoice_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void setLotInfoText(DataTable dtLotInfo)
        {
            try
            {
                if (dtLotInfo != null)
                {
                    if (dtLotInfo.Rows.Count > 0)
                    {
                        //txtReworkWOID.Text = Util.NVC(dtLotInfo.Rows[0]["LOTDTTM_CR"]);
                        jjcode.Text = Util.NVC(dtLotInfo.Rows[0]["MTRLID"]);
                        jjamount.Text = Util.NVC(dtLotInfo.Rows[0]["MTRLCOUNT"]);
                        
                        //txtWorkOrderR.Text = Util.NVC(dtLotInfo.Rows[0]["MODLNAME"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private int getCHKGrid()
        {
            int nResult = -1 ; 
            int counter;



            //nCount = dgTagetList.GetRowCount
           for (counter = 0; counter < (dgTagetList.Rows.Count);
           counter++)
            {
                if( DataTableConverter.GetValue(dgTagetList.Rows[counter].DataItem, "CHK").ToString().Equals("True") )
                {
                    nResult = counter;
                    return nResult;
                }

            }
            return nResult;

        }


        private int getCHKResinGrid()
        {
            int nResult = -1;
            int counter = 0;

           

            for (counter = 0; counter < (dgSearchResultList.Rows.Count);counter++)
            {
                if (DataTableConverter.GetValue(dgSearchResultList.Rows[counter].DataItem, "CHK").ToString().Trim().Equals("True"))
                {
                    nResult = counter;
                    return nResult;
                }

            }
            return nResult;
        }

        private DataTable getSelectedWoInfo(string sEqsgID, string sProcid, string sRouteid)
        {
            DataTable dtReturn = null;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sProcid;
                dr["EQSGID"] = sEqsgID;
                dr["PRODID"] = null;
                dr["ROUTID"] = null;
                RQSTDT.Rows.Add(dr);

                dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROUT_WO_PACK", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_ROUT_WO_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dtReturn;
        }


        private DataTable getSelectedProc(string sEqsgID)
        {
            DataTable dtReturn = null;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEqsgID;
   
                RQSTDT.Rows.Add(dr);

                dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESIN_PROC_LIST", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
               
                Util.MessageException(ex);
            }
            return dtReturn;
        }


    }
}

