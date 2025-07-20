/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Description : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.06.13  손우석      화면 재구성, 다국어 수정
  2017.06.28  손우석      화면 조회 결과 표시 항목 추가 및 기능 변경
  2017.07.12  손우석      계획수량, 생산수량 항목 추가
  2020.02.20  손우석      CSR ID 27479 GMES 자재불량처리 폐기유형 콤보박스 생성 요청 건 [요청번호] C20200203-000464
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_014 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtFalseListDtailResult = new DataTable();
        string userId = string.Empty;
        string userName = string.Empty;
        string department = string.Empty;
        string position = string.Empty;

        int current_row = 0;

        Util _Util = new Util();



        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_014()
        {
            InitializeComponent();

            this.Loaded += PACK001_014_Loaded;
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            //날자 초기값 세팅 : main 조회조건
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜

            //날자 초기값 세팅 : sub 조회조건
            dtpDateFrom_S.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
            dtpDateTo_S.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜

            dtpDateFrom_C.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
            dtpDateTo_C.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜

            InitCombo();

            setUser();

            setRdoMon();
        }
        #endregion

        private void setUser()
        {
            try
            {                
                txtUser.Text = LoginInfo.USERNAME;
               // txtUser_C.Text = LoginInfo.USERNAME;
                //txtUser_S.Text = LoginInfo.USERNAME;

                txtUser.Tag = LoginInfo.USERID;
                //txtUser_C.Tag = LoginInfo.USERID;
                //txtUser_S.Tag = LoginInfo.USERID;

                DataTable dt = getUser(LoginInfo.USERID, LoginInfo.USERNAME);

                if(dt.Rows.Count > 0 )
                {
                    userId = dt.Rows[0]["USERID"].ToString();
                    userName = dt.Rows[0]["USERNAME"].ToString();
                    department = dt.Rows[0]["DEPTNAME"].ToString();
                    position = dt.Rows[0]["POSITION"].ToString();
                }

                txtUser.Text = userName;
                //txtUser_C.Text = userName;
                //txtUser_S.Text = userName;

                txtUser.Tag = userId;
                //txtUser_C.Tag = userId;
                //txtUser_S.Tag = userId;

                tbUserInfo.Text = "(" + userId + "/" + position + ")" + " - " + department;
                //tbUserInfo_C.Text = "(" + userId + "/" + position + ")" + " - " + department;
                //tbUserInfo_S.Text = "(" + userId + "/" + position + ")" + " - " + department;
            }
            catch(Exception ex)
            {
                ms.AlertWarning(ex.Message);
            }
        }

        #region Event
        private void PACK001_014_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //tbGrid01_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                //tbGrid02_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                //tbGrid03_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                //tbGrid04_cnt.Text = "[ 0 " + ObjectDic.Ins]tance.GetObjectName("건") + " ]";

                Initialize();

                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();

                if (tcMain.SelectedIndex == 0) //자재불량입력
                {
                    dg = dgWordOrder;
                }
                else if(tcMain.SelectedIndex == 2) //자재불량실적조회
                {
                    dg = dgFalseListDtail;
                }

                if(LoginInfo.CFG_SHOP_ID.Equals("A040"))
                {
                    chkExclFlag.IsChecked = true;
                }

                //gridCombo(dg);

                this.Loaded -= PACK001_014_Loaded;
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                //Point pnt = e.GetPosition(null);
                //C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                //if (cell == null || cell.Value == null)
                //{
                //    return;
                //}
                C1.WPF.DataGrid.C1DataGrid dg = btn.Name.Equals("btnExcelDefectHIst") ? dgDefectHist : dgCancelListDtail;

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    if (col.Index < dg.Columns.Count)
                    {
                        if (col.Name.Equals("1"))
                        {
                            col.Visibility = Visibility.Visible;
                        }

                    }
                }

                new ExcelExporter().Export(dg);

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    if (col.Name.Equals("1"))
                    {
                        col.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setWoList();
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgWordOrder.ItemsSource = null;

                txtWOID.Text = string.Empty;
                txtProdID.Text = string.Empty;
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnTran_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgWordOrder.ItemsSource == null || dgWordOrder.Rows.Count == 0)
                {
                    return;
                }

                int chk_cnt = 0;

                for (int i = 0; i < dgWordOrder.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        chk_cnt++;

                        string qty = Util.NVC(DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "INPUT_QTY"));// MES 2.0 오류 Patch
                        string impute = Util.NVC(DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "IMPUTE"));// MES 2.0 오류 Patch
                        //2018-01-08
                        string scrapNote = Util.NVC(DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "SCRP_RSN_NOTE"));// MES 2.0 오류 Patch
                        //2020.02.20
                        //string costType = DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "COST_TYPE_CODE").ToString();
                        string resncode = Util.NVC(DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "RESNCODE"));// MES 2.0 오류 Patch

                        if (qty == "0")
                        {
                            ms.AlertWarning("SFU3063"); //수량이 없습니다.
                            return;
                        }

                        if (impute == "")
                        {
                            ms.AlertWarning("SFU3296"); //선택오류 : 귀책부서(필수조건) 콤보를 선택하지 않았습니다.[콤보선택]
                            return;
                        }

                        //2018-01-08
                        if (scrapNote == "")
                        {
                            ms.AlertWarning("SFU4448"); //입력오류 : 폐기사유(필수조건)를  입력하지 않았습니다.
                            return;
                        }

                        //2020.02.20
                        //if (costType == "")
                        //{
                        //    ms.AlertWarning("SFU4449"); //선택오류 : 비용구분(필수조건) 콤보를 선택하지 않았습니다.[콤보선택]
                        //    return;
                        //}

                        if (resncode == "")
                        {
                            ms.AlertWarning("SFU8151"); //입력오류 : 폐기유형(필수조건)을  선택하지 않았습니다.
                            return;
                        }
                    }
                }

                if (chk_cnt == 0)
                {
                    return;
                }

                if (txtUser.Text.Equals("") || txtUser.Tag.Equals(""))
                {
                    Util.MessageValidation("SFU1843");//작업자를 입력해주세요
                    return;
                }

                TranAccess();
                /*ms.AlertInfo("SFU1880"); //전송 완료 되었습니다.

                dgWordOrder.ItemsSource = null;

                //건수 조정
                setSeletedCacel(); //전송된 row 삭제  */
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setSeletedCacel();
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnReTran_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 전송상태가 FAIL인 것만 재전송함.
                if (dgFalseList.GetRowCount() == 0) 
                {
                    return;
                }                

                int CHK_CNT = 0;

                for (int i = 0; i < dgFalseList.GetRowCount(); i++) //전송하려는 row의 전송상태 체크
                {
                    if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        CHK_CNT++;

                        if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "TRAN_CODE").ToString() != "FAIL")
                        {                            
                            return;
                        }
                    }
                }

                if(CHK_CNT == 0)
                {
                    return;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                int chk_idx = 0;
                int total_row = dgFalseList.GetRowCount();

                for (int i = 0; i < total_row; i++)
                {
                    if (DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow drINDATA = INDATA.NewRow();
                        drINDATA["ERP_TRNF_SEQNO"] = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString();

                        INDATA.Rows.Add(drINDATA);

                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_ERP_RETRAN", "INDATA", null, INDATA);
              
                WoSearch("btnWoSearch_S");
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnWoSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                Button btn = sender as Button;
                WoSearch(btn.Name);
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void btnTrCancle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int CHK_CNT = 0;

                for (int i = 0; i < dgCancelList.GetRowCount(); i++) //전송하려는 row의 전송상태 체크
                {
                    if (DataTableConverter.GetValue(dgCancelList.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        CHK_CNT++;

                       
                    }
                }

                if (CHK_CNT == 0)
                {
                    return;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                int chk_idx = 0;
                int total_row = dgCancelList.GetRowCount();

                for (int i = 0; i < total_row; i++)
                {
                    if (DataTableConverter.GetValue(dgCancelList.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        if(DataTableConverter.GetValue(dgCancelList.Rows[i].DataItem, "CNCL_FLAG").ToString() == "N")
                        {
                            DataRow drINDATA = INDATA.NewRow();
                            drINDATA["ERP_TRNF_SEQNO"] = DataTableConverter.GetValue(dgCancelList.Rows[i].DataItem, "ERP_TRNF_SEQNO").ToString();

                            INDATA.Rows.Add(drINDATA);

                            chk_idx++;
                        }
                       
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_ACT_REG_CANCLESEND_ERP_PROD", "INDATA", null, INDATA);

               
                WoSearch("btnWoSearch_C");

                ms.AlertInfo("SFU1937"); //취소 되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }      

        private void dgSearchList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                if (dg == null || dg.GetRowCount() == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dg.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                if (cell.Column.Name != "CHK")
                {
                    return;
                }

                string sCHK = DataTableConverter.GetValue(dg.Rows[currentRow].DataItem, "CHK").ToString();
                string WO = DataTableConverter.GetValue(dg.Rows[currentRow].DataItem, "WOID").ToString();
                string SEQ = DataTableConverter.GetValue(dg.Rows[currentRow].DataItem, "PRCS_NO").ToString();

                setDetail(dg.Name,sCHK, WO, SEQ, dg.Rows[currentRow]);
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void Re_search()
        {
            try
            {
                string sCHK;
                string WO;
                string SEQ;

                dgFalseListDtail.ItemsSource = null;

                for (int i = 0; i < dgFalseList.GetRowCount(); i++)
                {
                    if(DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        sCHK = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "CHK").ToString();
                        WO = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "WOID").ToString();
                        SEQ = DataTableConverter.GetValue(dgFalseList.Rows[i].DataItem, "PRCS_NO").ToString();

                        setDetail(dgFalseList.Name, sCHK, WO, SEQ);
                    }
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWordOrderList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgWordOrderList == null || dgWordOrderList.GetRowCount() == 0)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWordOrderList.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgWordOrderList.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                string strWOID = DataTableConverter.GetValue(dgWordOrderList.Rows[currentRow].DataItem, "WOID").ToString();
                string strProdDesc = DataTableConverter.GetValue(dgWordOrderList.Rows[currentRow].DataItem, "PRODDESC").ToString();
                //int nPlanQTY = Convert.ToInt32(DataTableConverter.GetValue(dgWordOrderList.Rows[currentRow].DataItem, "PLANQTY").ToString());
                int nOutQTY = Convert.ToInt32(DataTableConverter.GetValue(dgWordOrderList.Rows[currentRow].DataItem, "OUTQTY").ToString());

                if (nOutQTY == 0)
                {
                    //ms.AlertWarning("SFU3641"); //생산실적이 없는 W/O 입니다
                    ms.AlertWarning("SFU3641");
                    return;
                }

                txtWOID.Text = strWOID;
                txtProdID.Text = strProdDesc;

                search();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        

        private void cboProductModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (txtWOID.Text != null)
                {
                    txtWOID.Text = string.Empty;
                    txtProdID.Text = string.Empty;
                }
                
                //2020.02.20
                //setWoList();
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }

        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboProductModel.SelectedIndex == 0)
                {
                    //2020.02.20
                    //setWoList();
                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }

        }

        private void btnWoSearch_S_hist_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            //if (Convert.ToString(cboArea_S_hist.SelectedValue).Equals("-SELECT-"))
            //{
            //    Util.MessageValidation("SFU3206");//동을 선택해주세요
            //    return;
            //}

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("MODEL", typeof(string));
                dt.Columns.Add("CLASS", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("ERP_STAT_CODE", typeof(string));
                dt.Columns.Add("FROMDATE", typeof(string));
                dt.Columns.Add("TODATE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Convert.ToString(cboArea_S_hist.SelectedValue).Equals("") ? null : Convert.ToString(cboArea_S_hist.SelectedValue);
                dr["EQSGID"] = Convert.ToString(cboEquipmentSegment_S_hist.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipmentSegment_S_hist.SelectedValue);
                dr["MODEL"] = Convert.ToString(cboProductModel_S_hist.SelectedValue).Equals("") ? null : Convert.ToString(cboProductModel_S_hist.SelectedValue);
                dr["CLASS"] = Convert.ToString(cboPrdtClass_S_hist.SelectedValue).Equals("") ? null : Convert.ToString(cboPrdtClass_S_hist.SelectedValue);
                dr["PRODID"] = Convert.ToString(cboProduct_S_hist.SelectedValue).Equals("") ? null : Convert.ToString(cboProduct_S_hist.SelectedValue);
                dr["ERP_STAT_CODE"] = Convert.ToString(cboStatus_hist.SelectedValue).Equals("") ? null : Convert.ToString(cboStatus_hist.SelectedValue);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom_S_hist);
                dr["TODATE"] = Util.GetCondition(dtpDateTo_S_hist);
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_WO_SCRP_HIST_DFCT_TRNAS_HIST", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgDefectHist, dtResult, FrameOperation, true);
              

            } catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DateTime dtFDMon = DateTime.Parse(GetSystemTime().ToString("yyyy-MM-01"));

                if (rdoCurMonth.IsChecked == true)
                {
                    if (!chkThisMonth())
                    {
                        dtpDateFrom.SelectedDateTime = DateTime.Parse(GetSystemTime().ToString("yyyy-MM-01"));
                        ms.AlertWarning("SFU8317"); //당월 W/O만 선택 가능합니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void gridCombo(C1.WPF.DataGrid.C1DataGrid dg, string imPute)
        {
            try
            {
                SetGridCboItem(dg.Columns[imPute]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void gridCostCombo(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.ItemsSource == null) return;// MES 2.0 오류 Patch

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITY_DFCT_CODE", "RQSTDT", "RSLTDT", dt);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                (dg.Columns["COST_TYPE_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);

            } catch (Exception ex)
            {

            }
                
        }

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RESP_DEPT";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2020.02.20
        private void gridResnCombo(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ACTIVITYREASON_DFCT_CODE", "RQSTDT", "RSLTDT", dt);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                (dg.Columns["RESNCODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {

            }

        }

        private void InitCombo()
        {
            CommonCombo _combo = new CMM001.Class.CommonCombo();

            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;
            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;           

            //라인            
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboWorkOrder };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProductModel };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            //2017-06-06 손우석 - 모델 추가
            //C1ComboBox[] cboProductModelChild = { cboWorkOrder };
            C1ComboBox[] cboProductModelParent = { cboAreaByAreaType, cboEquipmentSegment };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, sCase: "PRJ_MODEL");
            //_combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbChild: cboProductModelChild, cbParent: cboProductModelParent, sCase: "PRJ_MODEL");

            //WORK order
            //C1ComboBox[] cbocboWorkOrderParent = { cboEquipmentSegment };
            //_combo.SetCombo(cboWorkOrder, CommonCombo.ComboStatus.SELECT, cbParent : cbocboWorkOrderParent) ;

            //귀책부서
            //_combo.SetCombo(cboDepartment, CommonCombo.ComboStatus.SELECT); 

            //STAT
            string[] cboStatusParam = {  LoginInfo.LANGID, "ERP_STATUS_CODE" };
            _combo.SetCombo(cboStatus, CommonCombo.ComboStatus.ALL, sFilter : cboStatusParam, sCase : "COMMCODES");


            #region 자재불량실적조회

            //동           
            C1ComboBox[] cboAreaChild_S = { cboProductModel_S };
            _combo.SetCombo(cboArea_S, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild_S, sCase : "AREA");

            //라인            
            C1ComboBox[] cboEquipmentSegmentParent_S = { cboArea_S };
            C1ComboBox[] cboEquipmentSegmentChild_S = { cboProductModel_S, cboProduct_S };
            _combo.SetCombo(cboEquipmentSegment_S, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent_S, cbChild: cboEquipmentSegmentChild_S, sCase : "EQUIPMENTSEGMENT");

            //모델          
            C1ComboBox[] cboProductModelParent_S = { cboArea_S, cboEquipmentSegment_S };
            C1ComboBox[] cboProductModelChild_S = { cboProduct_S };
            _combo.SetCombo(cboProductModel_S, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent_S, cbChild: cboProductModelChild_S, sCase: "PRJ_MODEL");

            //제품분류(PACK 제품 분류)           
            C1ComboBox[] cboPrdtClassParent_S = { cboSHOPID, cboArea_S, cboEquipmentSegment_S, cboAreaByAreaType };
            C1ComboBox[] cboPrdtClassChild_S = { cboProduct_S };
            //C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
            _combo.SetCombo(cboPrdtClass_S, CommonCombo.ComboStatus.ALL, cbParent: cboPrdtClassParent_S, cbChild: cboPrdtClassChild_S, sCase : "PRDTCLASS");

            //제품코드  
            C1ComboBox[] cboProductParent_S = { cboSHOPID, cboArea_S, cboEquipmentSegment_S, cboProductModel_S, cboPrdtClass_S };
            _combo.SetCombo(cboProduct_S, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent_S, sCase: "PRJ_PRODUCT");

            

            #endregion

            #region 자재불량취소

            //동           
            C1ComboBox[] cboAreaChild_C = { cboProductModel_C };
            _combo.SetCombo(cboArea_C, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild_C, sCase: "AREA");

            //라인            
            C1ComboBox[] cboEquipmentSegmentParent_C = { cboArea_C };
            C1ComboBox[] cboEquipmentSegmentChild_C = { cboProductModel_C, cboProduct_C };
            _combo.SetCombo(cboEquipmentSegment_C, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent_C, cbChild: cboEquipmentSegmentChild_C, sCase: "EQUIPMENTSEGMENT");

            //모델          
            C1ComboBox[] cboProductModelParent_C = { cboArea_C, cboEquipmentSegment_C };
            C1ComboBox[] cboProductModelChild_C = { cboProduct_C };
            _combo.SetCombo(cboProductModel_C, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild_C, sCase: "PRJ_MODEL");

            //제품분류(PACK 제품 분류)           
            C1ComboBox[] cboPrdtClassParent_C = { cboSHOPID, cboArea_C, cboEquipmentSegment_C, cboAreaByAreaType };
            C1ComboBox[] cboPrdtClassChild_C = { cboProduct_C };
            //C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
            _combo.SetCombo(cboPrdtClass_C, CommonCombo.ComboStatus.ALL, cbParent: cboPrdtClassParent_C, cbChild: cboPrdtClassChild_C, sCase: "PRDTCLASS");

            //제품코드  
            C1ComboBox[] cboProductParent_C = { cboSHOPID, cboArea_C, cboEquipmentSegment_C, cboProductModel_C, cboPrdtClass_C };
            _combo.SetCombo(cboProduct_C, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent_C, sCase: "PRJ_PRODUCT");


            #endregion


            #region 자재불량실적조회2

            //STAT
            _combo.SetCombo(cboStatus_hist, CommonCombo.ComboStatus.ALL, sFilter: cboStatusParam, sCase: "COMMCODES");


            //동           
            _combo.SetCombo(cboArea_S_hist, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild_S, sCase: "AREA");

            //라인            
            C1ComboBox[] cboEquipmentSegmentParent_S_hist = { cboArea_S_hist };
            C1ComboBox[] cboEquipmentSegmentChild_S_hist = { cboProductModel_S_hist, cboProduct_S_hist };
            _combo.SetCombo(cboEquipmentSegment_S_hist, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent_S_hist, cbChild: cboEquipmentSegmentChild_S_hist, sCase: "EQUIPMENTSEGMENT");

            //모델          
            C1ComboBox[] cboProductModelParent_S_hist = { cboArea_S_hist, cboEquipmentSegment_S_hist };
            C1ComboBox[] cboProductModelChild_S_hist = { cboProduct_S_hist };
            _combo.SetCombo(cboProductModel_S_hist, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent_S_hist, cbChild: cboProductModelChild_S_hist, sCase: "PRJ_MODEL");

            //제품분류(PACK 제품 분류)           
            C1ComboBox[] cboPrdtClassParent_S_hist = { cboSHOPID, cboArea_S_hist, cboEquipmentSegment_S_hist, cboAreaByAreaType };
            C1ComboBox[] cboPrdtClassChild_S_hist = { cboProduct_S_hist };
            _combo.SetCombo(cboPrdtClass_S_hist, CommonCombo.ComboStatus.ALL, cbParent: cboPrdtClassParent_S_hist, cbChild: cboPrdtClassChild_S_hist, sCase: "PRDTCLASS");

            //제품코드  
            C1ComboBox[] cboProductParent_S_hist = { cboSHOPID, cboArea_S_hist, cboEquipmentSegment_S_hist, cboProductModel_S_hist, cboPrdtClass_S_hist };
            _combo.SetCombo(cboProduct_S_hist, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent_S_hist, sCase: "PRJ_PRODUCT");


            #endregion
        }



        private void search()
        {
            try
            {
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("EXCL_FLAG", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);              
                searchCondition["WOID"] = txtWOID.Text == "" ? null : txtWOID.Text;
                searchCondition["EXCL_FLAG"] = Util.GetCondition(chkExclFlag).Equals("Y") ? Util.GetCondition(chkExclFlag) : null;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ORDERMATERIAL_SEARCH", "INDATA", "OUTDATA", inDataTable);

                dgWordOrder.ItemsSource = null;

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgWordOrder, dtResult, FrameOperation, true);
                }

                //Util.SetTextBlockText_DataGridRowCount(tbGrid02_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setSeletedCacel()
        {
            try
            {
                if (dgWordOrder.ItemsSource != null)
                {
                    DataTable dt = DataTableConverter.Convert(dgWordOrder.ItemsSource);

                    for(int i = dt.Rows.Count-1; 0 <= i; i--)
                    {
                        var chkYn = dt.Rows[i]["CHK"].ToString();

                        if(chkYn == "True")
                        {
                            dt.Rows.RemoveAt(i);
                        }

                    }

                    dt.AcceptChanges();

                    dgWordOrder.ItemsSource = DataTableConverter.Convert(dt);

                    //for (int i = dgWordOrder.GetRowCount(); 0 < i; i--)
                    //{
                    //    var chkYn = DataTableConverter.GetValue(dgWordOrder.Rows[i - 1].DataItem, "CHK");

                    //    if (chkYn == null)
                    //    {
                    //        dgWordOrder.RemoveRow(i - 1);
                    //    }
                    //    else if (Convert.ToBoolean(chkYn))
                    //    {

                    //        dt.Rows.RemoveAt(1);
                            

                    //        DataRow[] drs = dt.RowDeleted();

                    //        DataRow dr = dt.Rows();

                    //        dt.Rows.Remove()
                    //        dgWordOrder.EndNewRow(true);
                    //        dgWordOrder.RemoveRow(i - 1);

                            


                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void TranAccess()
        {
            try
            {
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("WOID", typeof(string));                
                INDATA.Columns.Add("SCRP_USERID", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("POST_DATE", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();               
                drINDATA["WOID"] = txtWOID.Text;
                drINDATA["SCRP_USERID"] = txtUser.Tag.ToString();
                drINDATA["NOTE"] = Util.GetCondition(dtpPostDate) + "/" + userId + "/" + txtWOID.Text;
                drINDATA["POST_DATE"] = Util.GetCondition(dtpPostDate);
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;

                INDATA.Rows.Add(drINDATA);

                DataTable IN_MTRL = new DataTable();
                IN_MTRL.TableName = "INLOT";
                IN_MTRL.Columns.Add("MTRLID", typeof(string));
                IN_MTRL.Columns.Add("SCRP_QTY", typeof(string));
                IN_MTRL.Columns.Add("RESP_DEPT_CODE", typeof(string));
                IN_MTRL.Columns.Add("SCRP_RSN_NOTE", typeof(string));
                IN_MTRL.Columns.Add("COST_TYPE_CODE", typeof(string));
                //2020.02.20
                IN_MTRL.Columns.Add("ACTID", typeof(string));
                IN_MTRL.Columns.Add("RESNCODE", typeof(string));

                int chk_idx = 0;
                int total_row = dgWordOrder.GetRowCount();
                for (int i = 0; i < total_row; i++)
                {
                    if (DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow drMTRL = IN_MTRL.NewRow();
                        drMTRL["MTRLID"] = DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "MTRLID").ToString();
                        drMTRL["SCRP_QTY"] = DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "INPUT_QTY").ToString();
                        drMTRL["RESP_DEPT_CODE"] = DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "IMPUTE").ToString();
                        drMTRL["SCRP_RSN_NOTE"] = DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "SCRP_RSN_NOTE").ToString();
                        //2020.02.20
                        //drMTRL["COST_TYPE_CODE"] = DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "COST_TYPE_CODE").ToString();
                        drMTRL["COST_TYPE_CODE"] = null;
                        drMTRL["ACTID"] = "DEFECT_MTRL";
                        drMTRL["RESNCODE"] = DataTableConverter.GetValue(dgWordOrder.Rows[i].DataItem, "RESNCODE").ToString();

                        IN_MTRL.Rows.Add(drMTRL);

                        chk_idx++;
                    }
                }

                if (chk_idx == 0)
                {
                    return;
                }

                dsInput.Tables.Add(INDATA);
                dsInput.Tables.Add(IN_MTRL);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SCRAP_HIST", "INDATA,IN_MTRL", null, dsInput);


                ms.AlertInfo("SFU1880"); //전송 완료 되었습니다.

                dgWordOrder.ItemsSource = null;

                //건수 조정
                setSeletedCacel(); //전송된 row 삭제

            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void WoSearch(string button_name)
        {
            try
            {               
                C1ComboBox Area = new C1ComboBox();
                C1ComboBox Eqsg = new C1ComboBox();
                C1ComboBox Model = new C1ComboBox();
                C1ComboBox Class = new C1ComboBox();
                C1ComboBox Prd = new C1ComboBox();
                LGCDatePicker From = new LGCDatePicker();
                LGCDatePicker To = new LGCDatePicker();
                string USER = string.Empty;

                if (button_name == "btnWoSearch_S")
                {                    
                    Area = cboArea_S;
                    Eqsg = cboEquipmentSegment_S;
                    Model = cboProductModel_S;
                    Class = cboPrdtClass_S;
                    Prd = cboProduct_S;
                    From = dtpDateFrom_S;
                    To = dtpDateTo_S;
                    userId = "";              
                }
                else if(button_name == "btnWoSearch_C")
                {                    
                    Area = cboArea_S;
                    Eqsg = cboEquipmentSegment_C;
                    Model = cboProductModel_C;
                    Class = cboPrdtClass_C;
                    Prd = cboProduct_C;
                    From = dtpDateFrom_C;
                    To = dtpDateTo_C;
                    userId = "";
                    //if (txtUser_C.Tag == null || txtUser_C.Tag.ToString() == "")
                    //{
                    //    userId = "";
                    //}
                    //else
                    //{
                    //    userId = txtUser_C.Tag.ToString();
                    //}
                }

                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MODEL", typeof(string));
                inDataTable.Columns.Add("CLASS", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("ERP_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("FROMDATE", typeof(string));
                inDataTable.Columns.Add("TODATE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;

                if (button_name == "btnWoSearch_S")
                {
                    searchCondition["ERP_STAT_CODE"] = Util.GetCondition(cboStatus) == "" ? null : Util.GetCondition(cboStatus);
                }
                else if (button_name == "btnWoSearch_C")
                {
                    searchCondition["ERP_STAT_CODE"] = "SUCCESS";
                }
                
                searchCondition["FROMDATE"] = Util.GetCondition(From);
                searchCondition["TODATE"] = Util.GetCondition(To);

                searchCondition["AREAID"] = Util.GetCondition(Area) == "" ? null : Util.GetCondition(Area);
                searchCondition["EQSGID"] = Util.GetCondition(Eqsg) == "" ? null : Util.GetCondition(Eqsg);
                searchCondition["MODEL"] = Util.GetCondition(Model) == "" ? null : Util.GetCondition(Model);
                searchCondition["CLASS"] = Util.GetCondition(Class) == "" ? null : Util.GetCondition(Class);
                searchCondition["PRODID"] = Util.GetCondition(Prd) == "" ? null : Util.GetCondition(Prd);
                searchCondition["USERID"] = userId == "" ? null : userId;
                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRANMATERIAL_SEARCH_DETAIL", "INDATA", "OUTDATA", inDataTable);
                //실적 조회기능이 detail하게 변경됨에 따라 비즈도 변경 : DA_PRD_SEL_TRANMATERIAL_SEARCH --> DA_PRD_SEL_TRANMATERIAL_SEARCH_DETAIL
                            
                //grid 초기화
                if (button_name == "btnWoSearch_S")
                {
                    dgFalseList.ItemsSource = null;
                }
                else if (button_name == "btnWoSearch_C")
                {
                    dgCancelList.ItemsSource = null;
                }

                if (dtResult.Rows.Count != 0)
                {
                    if (button_name == "btnWoSearch_S")
                    {
                        Util.GridSetData(dgFalseList, dtResult, FrameOperation);
                        dgFalseListDtail.ItemsSource = null;
                    }
                    else if (button_name == "btnWoSearch_C")
                    {
                        Util.GridSetData(dgCancelList, dtResult, FrameOperation);
                        dgCancelListDtail.ItemsSource = null;
                    }                        
                }

                //Util.SetTextBlockText_DataGridRowCount(tbGrid04_cnt, Util.NVC(dtResult.Rows.Count));
                //tbDefectInformDtail_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]"; 

                dgFalseListDtail.ItemsSource = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setDetail(string dg_name, string sCHK, string wo, string seq, C1.WPF.DataGrid.DataGridRow dr = null)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();

                if (dg_name == "dgFalseList")
                {
                    dg = dgFalseListDtail;
                }
                else if (dg_name == "dgCancelList")
                {
                    dg = dgCancelListDtail;
                }

                if (sCHK == "True")
                {
                    DataTable inDataTable = new DataTable();

                    inDataTable.Columns.Add("WOID", typeof(string));
                    inDataTable.Columns.Add("PRCS_NO", typeof(string));
                    inDataTable.Columns.Add("LANGID", typeof(string));

                    DataRow searchCondition = inDataTable.NewRow();
                    searchCondition["WOID"] = wo;
                    searchCondition["PRCS_NO"] = seq;
                    searchCondition["LANGID"] = LoginInfo.LANGID;

                    inDataTable.Rows.Add(searchCondition);

                    dtFalseListDtailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRANMATERIAL_SEARCH_D", "INDATA", "OUTDATA", inDataTable);

                    //dgFalseListDtail.ItemsSource = null;


                    if (dtFalseListDtailResult.Rows.Count != 0)
                    {
                        if (dr != null)
                        {
                            dtFalseListDtailResult.Columns.Add("ERP_TRNF_SEQNO", typeof(string));
                            dtFalseListDtailResult.Columns.Add("POST_DATE", typeof(string));
                           // dtFalseListDtailResult.Columns.Add("WOID", typeof(string));
                           // dtFalseListDtailResult.Columns.Add("PRCS_NO", typeof(string));
                           // dtFalseListDtailResult.Columns.Add("PRODID", typeof(string));
                            dtFalseListDtailResult.Columns.Add("PLANSTDTTM", typeof(string));
                            dtFalseListDtailResult.Columns.Add("PLANEDDTTM", typeof(string));
                          //  dtFalseListDtailResult.Columns.Add("TRAN_STAT", typeof(string));
                            dtFalseListDtailResult.Columns.Add("TRAN_CODE", typeof(string));
                            dtFalseListDtailResult.Columns.Add("USERID", typeof(string));
                            dtFalseListDtailResult.Columns.Add("CNCL_FLAG", typeof(string));

                            for (int i = 0; i < dtFalseListDtailResult.Rows.Count; i++)
                            {
                                dtFalseListDtailResult.Rows[i]["ERP_TRNF_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "ERP_TRNF_SEQNO"));
                                dtFalseListDtailResult.Rows[i]["POST_DATE"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "POST_DATE"));
                                dtFalseListDtailResult.Rows[i]["PLANSTDTTM"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "PLANSTDTTM"));
                                dtFalseListDtailResult.Rows[i]["PLANEDDTTM"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "PLANEDDTTM"));
                                dtFalseListDtailResult.Rows[i]["TRAN_CODE"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "TRAN_CODE"));
                                dtFalseListDtailResult.Rows[i]["USERID"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "USERID"));
                                dtFalseListDtailResult.Rows[i]["CNCL_FLAG"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "CNCL_FLAG"));
                            }
                        }

                        if (dg.GetRowCount() > 0)
                        {
                            

                            int nDif = 0;

                            DataTable dtF = new DataTable();
                            dtF = DataTableConverter.Convert(dg.ItemsSource);

                            //상세 데이터를 합쳐서 보여주기
                            foreach (DataRow row2 in dtFalseListDtailResult.Rows)
                            {
                                for (int nCnt = 0; nCnt < dtF.Rows.Count; nCnt++)
                                {
                                    if (row2["WOID"].ToString() == dtF.Rows[nCnt]["WOID"].ToString())
                                    {
                                        if(row2["PRCS_NO"].ToString() == dtF.Rows[nCnt]["PRCS_NO"].ToString())
                                        {
                                            if (row2["MTRLID"].ToString() == dtF.Rows[nCnt]["MTRLID"].ToString())
                                            {
                                                nDif = nDif + 1;
                                            }
                                            else
                                            {
                                                nDif = nDif + 0;
                                            }
                                        }
                                        else
                                        {
                                            nDif = nDif + 0;
                                        }
                                    }
                                    else
                                    {
                                        nDif = nDif + 0;
                                    }
                                }

                                if (nDif == 0)
                                {
                                    DataRow row = dtF.Rows.Add();
                                    row[0] = row2[0];
                                    row[1] = row2[1];
                                    row[2] = row2[2];
                                    row[3] = row2[3];
                                    row[4] = row2[4];
                                    row[5] = row2[5];
                                    row[6] = row2[6];
                                    row[7] = row2[7];
                                    row[8] = row2[8];
                                    row[9] = row2[9];
                                    row[10] = row2[10];
                                    row[11] = row2[11];
                                    row[12] = row2[12];
                                    row[13] = row2[13];
                                    row[14] = row2[14];
                                    row[15] = row2[15];
                                    row[16] = row2[16];
                                    row[17] = row2[17];
                                }
                            }

                            dtFalseListDtailResult = dtF;
                            Util.GridSetData(dg, dtF, FrameOperation);
                        }
                        else
                        {
                            Util.GridSetData(dg, dtFalseListDtailResult, FrameOperation);
                        }
                    }
                }
                else
                {
                    //상세 데이터에서 언체크한 항목 row 지우기
                    DataTable dtR = new DataTable();
                    dtR = DataTableConverter.Convert(dg.ItemsSource);

                    for (int irow3 = dtR.Rows.Count - 1; 0 <= irow3; irow3--)
                    {
                        if (dtR.Rows[irow3]["WOID"].ToString() == wo)
                        {
                            if (dtR.Rows[irow3]["PRCS_NO"].ToString() == seq)
                            {
                                dtR.Rows[irow3].Delete();
                            }
                        }
                    }
                    dtFalseListDtailResult = dtR;
                    Util.GridSetData(dg, dtR, FrameOperation);
                    
                }
                //Util.SetTextBlockText_DataGridRowCount(tbGrid04_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setWoList()
        {
            try
            {
                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MOLDID", typeof(string));
                inDataTable.Columns.Add("FROMDATE", typeof(string));
                inDataTable.Columns.Add("TODATE", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? null : Util.GetCondition(cboEquipmentSegment);
                searchCondition["MOLDID"] = Util.GetCondition(cboProductModel) == "" ? null : Util.GetCondition(cboProductModel);
                //2020.02.20
                //searchCondition["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                //searchCondition["TODATE"] = Util.GetCondition(dtpDateTo);
                searchCondition["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"); 
                searchCondition["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");

                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_BY_FMATERIAL_PRODUCTDATE", "INDATA", "OUTDATA", inDataTable);
                //조회 날짜를 생산날짜에 해당하는 W/O를 가져 오도록 수정함 (2017-08-21)

                dgWordOrderList.ItemsSource = null;
                dgWordOrder.ItemsSource = null;

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgWordOrderList, dtResult, FrameOperation);
                   
                    // Util.SetTextBlockText_DataGridRowCount(tbGrid01_cnt, Util.NVC(dtResult.Rows.Count));
                }
                ///else
                //{
                    //dgWordOrderList.ItemsSource = null
                    //Util.SetTextBlockText_DataGridRowCount(tbGrid01_cnt, Util.NVC(dtResult.Rows.Count));
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Boolean chkAUTH()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_UI_WO_SELECT_ONLY_THIS_MONTH";
                dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;


                RQSTDT.Rows.Add(dr);

                DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", RQSTDT);

                if (dtAuth.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private Boolean chkPostAUTH()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_UI_FIXED_POSTING_DATE";
                dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;


                RQSTDT.Rows.Add(dr);

                DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", RQSTDT);

                if (dtAuth.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }
        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        private Boolean chkThisMonth()
        {
            try
            {
                //2021.01.13 이재호
                //계획시작일의 Month가 현재 월 보다 빠를경우 선택불가
                DateTime dtFDMon = DateTime.Parse(GetSystemTime().ToString("yyyy-MM-01"));

                DateTime sPlanStDttm = DateTime.Parse(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString("yyyy-MM-dd"));
                //DateTime sPlanEdDttm = DateTime.Parse(Convert.ToDateTime(dtpDateTo.SelectedDateTime).ToString("yyyy-MM-dd"));

                if (chkAUTH())
                {
                    if (sPlanStDttm >= dtFDMon)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void CurMonth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkPostAUTH())
                {
                    DateTime dtCur1stday = GetComSelCalDate().AddDays(1 - GetComSelCalDate().Day);

                    dtpPostDate.SelectedDateTime = GetComSelCalDate();
                    dtpDateFrom.SelectedDateTime = dtCur1stday;
                    dtpDateTo.SelectedDateTime = dtCur1stday.AddMonths(1).AddDays(-1);

                    rdoCurMonth.IsChecked = true;
                    dtpDateFrom.IsEnabled = true;
                    dtpDateTo.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PreMonth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkPostAUTH())
                {
                    DateTime PreLDay;
                    DateTime dtPre1stday = GetComSelCalDate().AddMonths(-1).AddDays(1 - GetComSelCalDate().Day);

                    DataTable dt = GetStockEnd();
                    PreLDay = DateTime.Parse(dt.Rows[0]["PRE_LAST_DAY"].ToString());

                    dtpPostDate.SelectedDateTime = PreLDay;
                    dtpDateFrom.SelectedDateTime = dtPre1stday;
                    dtpDateTo.SelectedDateTime = dtPre1stday.AddMonths(1).AddDays(-1);
                    
                    dtpDateFrom.IsEnabled = false;
                    dtpDateTo.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setRdoMon()
        {
            try
            {
                if (chkPostAUTH())
                {
                    rdoCurMonth.Visibility = Visibility.Visible;
                    rdoPreMonth.Visibility = Visibility.Visible;

                    string PostMon;
                    DateTime PreLDay;

                    DataTable dt = GetStockEnd();
                    PostMon = dt.Rows[0]["YM"].ToString();
                    PreLDay = DateTime.Parse(dt.Rows[0]["PRE_LAST_DAY"].ToString());

                    string dtFDMon = DateTime.Parse(GetComSelCalDate().ToString("yyyy-MM-01")).AddMonths(-1).ToString("yyyyMM");
                    DateTime dtCur1stday = GetComSelCalDate().AddDays(1 - GetComSelCalDate().Day);
                    
                    if (PostMon.Equals(dtFDMon))
                    {
                        rdoCurMonth.IsChecked = true;
                        rdoPreMonth.IsEnabled = false;

                        dtpPostDate.IsEnabled = false;
                        dtpPostDate.SelectedDateTime = GetComSelCalDate();
                    }
                    else
                    {
                        rdoCurMonth.IsChecked = true;
                        rdoPreMonth.IsEnabled = true;

                        dtpPostDate.IsEnabled = false;
                        dtpPostDate.SelectedDateTime = GetComSelCalDate();
                    }
                    
                    dtpDateFrom.SelectedDateTime = dtCur1stday;
                    dtpDateTo.SelectedDateTime = dtCur1stday.AddMonths(1).AddDays(-1);
                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static DataTable GetStockEnd()
        {
            const string bizRuleName = "DA_PRD_SEL_STOCK_END_INFO";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SHOPID", typeof(string));

            DataRow searchShop = inDataTable.NewRow();
            searchShop["SHOPID"] = LoginInfo.CFG_SHOP_ID;

            inDataTable.Rows.Add(searchShop);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            return dtResult;
        }



        #endregion



        private void getUser(C1.WPF.C1ComboBox cbo, TextBox tb)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("USERNAME", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["USERNAME"] = tb.Text;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER", "INDATA", "OUTDATA", inDataTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tab_idx = tcMain.SelectedIndex;

            if (tab_idx < 0)
            {
                return;
            }

            C1.WPF.DataGrid.C1DataGrid dg = new C1.WPF.DataGrid.C1DataGrid();
            DataTable dt = new DataTable();
            string imPute = string.Empty;

            if (tab_idx == 0)
            {
                dg = dgWordOrder;
                imPute = "IMPUTE";
            }          
            else if (tab_idx == 2)
            {                
                dg = dgFalseListDtail;
                imPute = "RESP_DEPT_CODE";
            }

            if (tab_idx != 1 && tab_idx != 3)
            {
                gridCombo(dg, imPute);
                gridCostCombo(dg);
            }

            //2020.02.20
            gridResnCombo(dg);
        }

        private void DataGridComboBoxColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {   
                if (dgFalseListDtail.CurrentColumn == null || dgFalseListDtail.CurrentRow == null)
                {
                    return;
                }

                if (dgFalseListDtail.CurrentColumn.Name != "RESP_DEPT_CODE")
                {
                    return;
                }

                if(dgFalseListDtail.GetRowCount() == 0)
                {
                    return;
                }

                int current_row = dgFalseListDtail.CurrentRow.Index;
                int current_col = dgFalseListDtail.CurrentColumn.Index;

                if(dgFalseListDtail.CurrentCell.Value == null)
                {
                    return;
                }

                string value = dgFalseListDtail.CurrentCell.Value.ToString();

                string old_value = (dtFalseListDtailResult.Rows[current_row] as DataRow)["RESP_DEPT_CODE"].ToString();

                //콤보박스 값이 변경 됐는지 확인
                if (value == old_value)
                {
                    return;
                }                

                if (value != null)
                {
                    DataRowView drv = dgFalseListDtail.Rows[current_row].DataItem as DataRowView;

                    if (drv == null)
                    {
                        return;
                    }

                    //선택한 귀책부서 수정
                    SetImPute(current_row, value, old_value, drv);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetImPute(int row_index, string impute, string impute_old, object obj = null)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }

                if (!(obj is DataRowView))
                {
                    return;
                }

                DataRowView drv = obj as DataRowView;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("PRCS_NO", typeof(string));
                inDataTable.Columns.Add("MTRLID", typeof(string));
                inDataTable.Columns.Add("RESP_DEPT_CODE_OLD", typeof(string));
                inDataTable.Columns.Add("ITEM_NO", typeof(string));
                inDataTable.Columns.Add("RESP_DEPT_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["WOID"] = drv["WOID"];
                searchCondition["PRCS_NO"] = drv["PRCS_NO"];
                searchCondition["MTRLID"] = drv["MTRLID"];
                searchCondition["RESP_DEPT_CODE_OLD"] = impute_old; //귀책부서코드(변경전)
                searchCondition["ITEM_NO"] = drv["ITEM_NO"];
                searchCondition["RESP_DEPT_CODE"] = impute;
                searchCondition["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtDdrectResult = new DataTable();
                new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_IMPUTE_CHANGE", "RQSTDT", "RSLTDT", inDataTable);

                Re_search();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnUserSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;                
              
                PACK001_014_USERSELECT popup = new PACK001_014_USERSELECT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("BUTTONNAME", typeof(string));                   
                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["BUTTONNAME"] = btn.Name;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_014_USERSELECT popup = sender as PACK001_014_USERSELECT;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    userId = popup.USERID;
                    userName = popup.USERNAME;
                    department = popup.DEPTNAME;
                    position = popup.POSITION;

                    TextBlock tb = new TextBlock();
                    TextBox txt = new TextBox();

                    if(popup.BUTTONNAME == "btnUserSearch")
                    {
                        tb = tbUserInfo;
                        txt = txtUser;
                    }
                    else if (popup.BUTTONNAME == "btnUserSearch_C")
                    {
                        //tb = tbUserInfo_C;
                        //txt = txtUser_C;
                    }
                    else if (popup.BUTTONNAME == "btnUserSearch_S")
                    {
                        //tb = tbUserInfo_S;
                        //txt = txtUser_S;
                    }

                    txt.Text = userName;
                    txt.Tag = userId;
                    tb.Text = "(" + userId + "/" + position + ")" + " - " + department;
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void txtUser_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBox tb = sender as TextBox;

                tb.Text = "";
                tb.Tag = null;

                if(tb.Name == "txtUser_S")
                {
                    //tbUserInfo_S.Text = "";
                }
                else if(tb.Name == "txtUser_C")
                {
                    //tbUserInfo_C.Text = "";
                }
            }
            catch(Exception ex)
            {
                ms.AlertWarning(ex.Message);
            }
        }

        private DataTable getUser(string userId, string userName)
        {
           
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("USERNAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = userId == ""? null : userId;
                dr["USERNAME"] = userName == ""? null : userName;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USERINFOR", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;          
        }


        public static T GetItemAtLocation<T>(FrameworkElement currentControl, Point location)
        {
            T foundItem = default(T);

            HitTestResult hitTestResults = VisualTreeHelper.HitTest(currentControl, location);

            if (hitTestResults.VisualHit is FrameworkElement)
            {
                object dataObject = (hitTestResults.VisualHit as FrameworkElement);

                if (dataObject is T)
                {
                    foundItem = (T)dataObject;
                }
            }

            return foundItem;

        }

        public static FrameworkElement GetItemAtlocation(FrameworkElement currentControl, Point location)
        {
            return GetItemAtLocation<FrameworkElement>(currentControl, location);
        }
        
    }
}
