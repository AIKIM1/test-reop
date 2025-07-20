/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack Lot이력- 검사데이터 입력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01 Jeong Hyeon Sik : Initial Created.
  2019.12.11 손우석 SM CMI Pack 메시지 다국어 처리 요청 
  2020.01.20 손우석 CSR ID 6935 Inspection Data history tap 상 Operator column 추가 요청 건 [요청번호 : C20191121-000303]
  2020.09.23 김준겸 Resin 무게 전 데이터  이전 데이터 선택 할 수 있도록 기능 구현.  CSR ID : C20200813-000315  
  2021.04.14 김준겸 공정 추가에 따른 Resin 주입 전 무게 데이터 수정 CSR 진행 요청 건 [요청번호 : C20210330-000604]
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
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_005_ADDINSPECTIONDATA : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //2020.01.20
        private string strAu = string.Empty;

        public PACK001_005_ADDINSPECTIONDATA()
        {
            InitializeComponent();
        }

        private DataTable dtLotINFO = null;
        private string sLotid = string.Empty;
        private string sProcid = string.Empty;
        private string sEqptid = string.Empty;
        private string sWIPSEQ = string.Empty;
        private string sClctseq = string.Empty;
        private string sClctitem = string.Empty;
        private string sClctVal = string.Empty;
        private string sClctVal01 = string.Empty;
        private int iinspinputListIndex = -1;

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {  
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnOK);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                // 검사데이터 선택 저장 버튼 비활성화
                btnSave.Visibility = Visibility.Collapsed;
                Insp_Check.Visibility = Visibility.Collapsed;

                //2020.01.20
                chkAUTH();

                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        txtSearchLot.Text = Util.NVC(dtText.Rows[0]["LOTID"]);

                        getLotInfo();

                        getActHistory();

                        getWipdatacollect_Q(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());

            }
        }

        #region Text
        private void txtSearchLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    // 초기화 선언.
                    initInspInput();

                    getLotInfo();

                    getActHistory();

                    getWipdatacollect_Q(null);
                }
            }
            catch(Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void initInspInput()
        {
            btnSave.Visibility = Visibility.Collapsed;    // 검사 데이터 선택 버튼 비활성화
            Insp_Check.Visibility = Visibility.Collapsed; // 검사 데이터 선택 텍스트 박스 비활성화
            CHK.Visibility = Visibility.Collapsed;        // 선택 Radio 박스 비활성화
            clctseq.Visibility = Visibility.Collapsed;    // 차수 컬럼 비활성화
            txtNote.Text = string.Empty;                  // 비고 텍스트 박스 초기화
            btnOK.Visibility = Visibility.Visible;        // 검사 데이터 입력 버튼 활성화
            Insp_Input.Visibility = Visibility.Visible;
            btnInspectionBefore.IsEnabled = true;
            dgInspectionInput.ItemsSource = null;

            sEqptid = string.Empty;
            sProcid = string.Empty;
            sClctseq = string.Empty;
            sClctitem = string.Empty;
            sClctVal = string.Empty;
            sClctVal01 = string.Empty;

        }
        #endregion Text

        #region Grid
        private void dgActHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgActHistory.GetCellFromPoint(pnt);
                CHK.Visibility = Visibility.Collapsed;
                btnInspectionBefore.IsEnabled = true;
                clctseq.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;
                btnOK.Visibility = Visibility.Visible;
                Insp_Input.Visibility = Visibility.Visible;
                Insp_Check.Visibility = Visibility.Collapsed;

                if (cell != null)
                {
                    sLotid = txtSearchLot.Text;
                    sEqptid = Util.NVC(DataTableConverter.GetValue(dgActHistory.Rows[cell.Row.Index].DataItem, "EQPTID"));
                    sProcid = Util.NVC(DataTableConverter.GetValue(dgActHistory.Rows[cell.Row.Index].DataItem, "PROCID"));
                    sWIPSEQ = Util.NVC(DataTableConverter.GetValue(dgActHistory.Rows[cell.Row.Index].DataItem, "WIPSEQ"));
                    string sSelectProdid = Util.NVC(DataTableConverter.GetValue(dgActHistory.Rows[cell.Row.Index].DataItem, "PRODID"));

                    if (sEqptid.Length > 0)
                    {
                        getWipdatacollect_Q_inputTargetList(sSelectProdid, sProcid);

                        //2020.01.20
                        if (LoginInfo.CFG_SHOP_ID != "A040")
                        {
                            dgInspectionInput.IsReadOnly = true;                            
                        }
                        else
                        {
                            if (LoginInfo.CFG_AREA_ID != "P1")
                            {
                                if (strAu != "DEV")
                                {
                                    dgInspectionInput.IsReadOnly = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgInspectionData_MouseDobuleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                initInspInput();

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgInspectionData.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    sLotid = txtSearchLot.Text;
                    sEqptid = Util.NVC(DataTableConverter.GetValue(dgInspectionData.Rows[cell.Row.Index].DataItem, "EQPTID"));
                    sProcid = Util.NVC(DataTableConverter.GetValue(dgInspectionData.Rows[cell.Row.Index].DataItem, "PROCID"));
                    sClctseq = Util.NVC(DataTableConverter.GetValue(dgInspectionData.Rows[cell.Row.Index].DataItem, "CLCTSEQ"));
                    sClctitem = Util.NVC(DataTableConverter.GetValue(dgInspectionData.Rows[cell.Row.Index].DataItem, "CLCTITEM"));
                    sClctVal = Util.NVC(DataTableConverter.GetValue(dgInspectionData.Rows[cell.Row.Index].DataItem, "CLCTVAL"));
                }

                if (chkInspProcModify(sProcid))  // 2021.04.12 공통코드에 적힌 공정일 경우 검사데이터 수정 가능하도록 변경.
                {
                    if (chkINSPAUTH())
                    {     
                        sClctVal01 = string.Empty;
                        btnInspectionBefore.IsEnabled = false;
                        btnOK.Visibility = Visibility.Collapsed;
                        btnSave.Visibility = Visibility.Visible;
                        Insp_Input.Visibility = Visibility.Collapsed;
                        Insp_Check.Visibility = Visibility.Visible;
                        dgInspectionInput.IsReadOnly = true;
                        iinspinputListIndex = -1;

                        if (sEqptid.Length > 0)
                        {
                            getWipdatacollect_Q_ChkList(sLotid, sProcid);
                        }                        
                    }
                }                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void getWipdatacollect_Q_ChkList(string sLotid, string sProcid)
        {
            try
            {
                CHK.Visibility = Visibility.Visible;
                clctseq.Visibility = Visibility.Visible;                

                //DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_Q
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                dr["PROCID"] = sProcid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_Q_CHK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgInspectionInput, dtResult, FrameOperation, true);
                }
                else
                {
                    Util.gridClear(dgInspectionInput);
                }

            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_Q", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgInspectionInput_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                setInspectionOkNG_Display(e.Cell);
                //int iRow = e.Cell.Row.Index;
                //if (e.Cell.Text == "")
                //{
                //    return;
                //}

                //Decimal dValue = Convert.ToDecimal(e.Cell.Text);
                //Decimal dCLCTLSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspectionInput.Rows[iRow].DataItem, "LCL"));
                //Decimal dCLCTUSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspectionInput.Rows[iRow].DataItem, "UCL"));

                //if (dValue >= dCLCTLSL &&
                //   dValue <= dCLCTUSL)
                //{
                //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                //    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                //    DataTableConverter.SetValue(dgInspectionInput.Rows[iRow].DataItem, "PASSYN", "Y");
                //}
                //else
                //{
                //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                //    DataTableConverter.SetValue(dgInspectionInput.Rows[iRow].DataItem, "PASSYN", "N");
                //}
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgInspectionInput_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                setInspectionOkNG_Display(e.Cell);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion Grid

        #region Button
        private void btnInspectionBefore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setInspectionBefore();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (rtxNote.DataContext.ToString() == "")
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("교체사유를 입력하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                //    rtxNote.Focus();
                //    return;
                //}
                //if (txtID.Text.Length == 0)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("USER ID 를 입력하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                //    txtID.Focus();
                //    return;
                //}
                //if (txtPassWord.Text.Length == 0)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("비밀번호를 입력하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                //    txtPassWord.Focus();
                //    return;
                //}                                
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("검사이력을추가하시겠습니까?"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        setWipdataCollect();

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                });
                             
                
            }
            catch(Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void updChkWipdataCollect()
        {
            try
            {
                //BR_PRD_REG_DATA
                DataSet dsResult = new DataSet();
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));


                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = sLotid;
                drINDATA["PROCID"] = sProcid;
                drINDATA["EQPTID"] = sEqptid;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["NOTE"] = txtNote.Text;
                INDATA.Rows.Add(drINDATA);

                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL01", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTSEQ", typeof(string));

                DataRow drIN_CLCTITEM = IN_CLCTITEM.NewRow();
                drIN_CLCTITEM["CLCTITEM"] = sClctitem;
                drIN_CLCTITEM["CLCTVAL"] = sClctVal;
                drIN_CLCTITEM["CLCTVAL01"] = sClctVal01;
                drIN_CLCTITEM["CLCTSEQ"] = sClctseq;
                IN_CLCTITEM.Rows.Add(drIN_CLCTITEM);


                dsInput.Tables.Add(IN_CLCTITEM);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_Q_DATA_PACK_CHK", "INDATA,IN_CLCTITEM", "", dsInput, null);

                if (dsResult != null)
                {                    
                    Util.MessageInfo("SFU1275"); //정상처리 되었습니다.
                }
                else
                {                    
                    Util.Alert("SFU3000"); // 데이터를 확인 중 문제가 발생하였습니다.                    
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 공통 코드 관리.
        private bool chkINSPAUTH()
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
                dr["CMCDTYPE"] = "PACK_RESIN_INSP_ALTER_USERID";
                dr["CMCODE"] = LoginInfo.USERID;


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

        private bool chkInspProcModify(string sProcid)  // 검사데이터 수정할 수 있는 공정(하드코딩 -> 공통코드 관리 하는 공정)
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
                dr["CMCDTYPE"] = "PACK_CHK_INSPALTER_PROC";
                dr["CMCODE"] = sProcid;


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
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //if(this.DialogResult != MessageBoxResult.OK)
            //{
            //    this.DialogResult = MessageBoxResult.OK;
            //}
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }
        #endregion Button

        #endregion Event

        #region Mehod

        private void getLotInfo()
        {
            try
            {
                //DA_PRD_SEL_WIPACTHISTORY_PACK

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtSearchLot.Text;
                RQSTDT.Rows.Add(dr);

                dtLotINFO = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                

            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_LOT_INFO_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getActHistory()
        {
            try
            {
                //DA_PRD_SEL_WIPACTHISTORY_PACK

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtSearchLot.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPACTHISTORY_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                //dgActHistory.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgActHistory, dtResult, FrameOperation, true);
            }
            catch(Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_WIPACTHISTORY_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getWipdatacollect_Q(string sProcid)
        {
            try
            {
                //DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_Q
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtSearchLot.Text;
                dr["PROCID"] = sProcid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_Q", "RQSTDT", "RSLTDT", RQSTDT);

                //dgInspectionData.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgInspectionData, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_WIPDATACOLLECT_CLCTTYPE_Q", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getWipdatacollect_Q_inputTargetList(string sProdid , string sProcid)
        {
            try
            {
                
                string sArea = null;

                if (dtLotINFO.Rows.Count > 0)
                {
                    sArea = Util.NVC(dtLotINFO.Rows[0]["AREAID"]);
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("CLCTTYPE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = sProdid;
                dr["PROCID"] = sProcid;
                dr["CLCTTYPE"] = "Q";
                dr["AREAID"] = sArea;
                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITEM_BY_CLCTTYPE", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITEM_BY_CLCTTYPE_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                //dgInspectionInput.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgInspectionInput, dtResult, FrameOperation, true);
                }
                else
                {
                    Util.gridClear(dgInspectionInput);
                }
            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_QUALITEM_BY_CLCTTYPE", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void setWipdataCollect()
        {
            try
            {
                //BR_PRD_REG_DATA
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));


                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = txtSearchLot.Text;
                drINDATA["PROCID"] = sProcid;
                drINDATA["EQPTID"] = sEqptid;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["NOTE"] = txtNote.Text;
                INDATA.Rows.Add(drINDATA);

                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));

                DataRow drIN_CLCTITEM = null;
                if (dgInspectionInput.Rows.Count > 0)
                {
                    for (int i = 0; i < dgInspectionInput.Rows.Count; i++)
                    {
                        drIN_CLCTITEM = IN_CLCTITEM.NewRow(); 
                        drIN_CLCTITEM["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgInspectionInput.Rows[i].DataItem, "CLCTITEM"));
                        drIN_CLCTITEM["CLCTVAL"] = Util.NVC(DataTableConverter.GetValue(dgInspectionInput.Rows[i].DataItem, "CLCTVAL01"));
                        IN_CLCTITEM.Rows.Add(drIN_CLCTITEM);
                    }
                }
                else
                {
                    drIN_CLCTITEM = INDATA.NewRow();
                    IN_CLCTITEM.Rows.Add(drIN_CLCTITEM);
                }

                dsInput.Tables.Add(IN_CLCTITEM);
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DATA_PACK", "INDATA,IN_CLCTITEM", "", dsInput, null);
            }
            catch(Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("BR_PRD_REG_DATA_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void setInspectionBefore()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                dr["PROCID"] = sProcid;
                RQSTDT.Rows.Add(dr);

                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_LOT_MAX", "INDATA", "OUTDATA", RQSTDT);

                if (dtReturn.Rows.Count > 0)
                {
                    for (int i = 0; i < dgInspectionInput.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(dgInspectionInput.Rows[i].DataItem, "CLCTVAL01", "0");
                    }

                    for (int i = 0; i < dtReturn.Rows.Count; i++)
                    {
                        int iRow = Util.gridFindDataRow(ref dgInspectionInput, "CLCTITEM", Util.NVC(dtReturn.Rows[i]["CLCTITEM"]), false);
                        if(iRow > -1)
                        {
                            DataTableConverter.SetValue(dgInspectionInput.Rows[iRow].DataItem, "CLCTVAL01", Util.NVC(dtReturn.Rows[i]["CLCTVAL01"]));
                        }
                    }

                    //rtxNote.AppendText(MessageDic.Instance.GetMessage("수동 검사 이전값 동일 입력"));
                    //rtxNote.SelectAll();
                    //rtxNote.Focus();
                }
                else
                {
                    for (int i = 0; i < dgInspectionInput.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(dgInspectionInput.Rows[i].DataItem, "CLCTVAL01", "0");
                    }
                }
                setInspectionOkNg_result();
            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_WIPDATACOLLECT_LOT_MAX", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void setInspectionOkNG_Display(C1.WPF.DataGrid.DataGridCell cell)
        {
            try
            {
                int iRow = cell.Row.Index;
                if (cell.Text == "")
                {
                    return;
                }
                if (cell.Column.Name == "CLCTVAL01")
                {
                    Decimal dValue = Convert.ToDecimal(cell.Text);
                    Decimal dCLCTLSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspectionInput.Rows[iRow].DataItem, "LCL"));
                    Decimal dCLCTUSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspectionInput.Rows[iRow].DataItem, "UCL"));

                    if (dValue >= dCLCTLSL &&
                       dValue <= dCLCTUSL)
                    {
                        if (cell.Presenter != null)
                        {
                            cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            cell.Presenter.FontWeight = FontWeights.Regular;

                        }
                        DataTableConverter.SetValue(dgInspectionInput.Rows[iRow].DataItem, "PASSYN", "Y");
                    }
                    else
                    {
                        if (cell.Presenter != null)
                        {
                            cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        DataTableConverter.SetValue(dgInspectionInput.Rows[iRow].DataItem, "PASSYN", "N");
                    }
                }
                else
                {
                    cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    cell.Presenter.FontWeight = FontWeights.Regular;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setInspectionOkNg_result()
        {
            try
            {
                if (dgInspectionInput.Rows.Count > 0)
                {
                    for (int i = 0; i < dgInspectionInput.Rows.Count; i++)
                    {

                        DataTableConverter.SetValue(dgInspectionInput.Rows[i].DataItem, "PASSYN", "");

                        if (Util.NVC(DataTableConverter.GetValue(dgInspectionInput.Rows[i].DataItem, "CLCTVAL01")) == "")
                        {
                            return;
                        }

                        Decimal dValue = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspectionInput.Rows[i].DataItem, "CLCTVAL01"));
                        Decimal dCLCTLSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspectionInput.Rows[i].DataItem, "LCL"));
                        Decimal dCLCTUSL = Util.NVC_Decimal(DataTableConverter.GetValue(dgInspectionInput.Rows[i].DataItem, "UCL"));


                        C1.WPF.DataGrid.DataGridCell cell = dgInspectionInput.GetCell(i, dgInspectionInput.Columns["CLCTVAL01"].Index);
                        if (dValue >= dCLCTLSL &&
                           dValue <= dCLCTUSL)
                        {
                            if (cell.Presenter != null)
                            {
                                cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                cell.Presenter.FontWeight = FontWeights.Regular;
                            }
                            DataTableConverter.SetValue(dgInspectionInput.Rows[i].DataItem, "PASSYN", "Y");
                        }
                        else
                        {
                            if (cell.Presenter != null)
                            {
                                cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            DataTableConverter.SetValue(dgInspectionInput.Rows[i].DataItem, "PASSYN", "N");
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2020.01.20
        private void chkAUTH()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_HIDDEN_MODE_AUTH";
                dr["ATTRIBUTE1"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_BY_ATTR1", "INDATA", "OUTDATA", RQSTDT);

                if (dtAuth.Rows.Count > 0)
                {
                    strAu = "DEV";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void dgInspectionInputList_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                iinspinputListIndex = Util.gridFindDataRow(ref dgInspectionInput, "CHK", "1", false);

                if (iinspinputListIndex == -1)
                {
                    return;
                }

                sClctVal01 = Util.NVC(DataTableConverter.GetValue(dgInspectionInput.Rows[iinspinputListIndex].DataItem, "CLCTVAL01"));
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {            
            // 차수 선택 하지 않을 경우.
            if (iinspinputListIndex == -1)
            {
                Util.Alert("SFU1651");
                return;
            }

            if (chkINSPAUTH())
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8250", sClctVal, sClctVal01), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4340"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        updChkWipdataCollect();

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                });
            }
            else
            {
                Util.MessageInfo("SFU5142"); // 수작업 모드를 진행할 권한이 없습니다. (엔지니어에게 문의 바랍니다.)
                return;
            }            
        }
    }
}
