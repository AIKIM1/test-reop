/*************************************************************************************
 Created Date : 2017.12.08
      Creator : 
   Decription : CST 관리
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.08  DEVELOPER : Initial Created.
  2022.02.10  sjnyn     : Jelly Roll Tab 추가
  2022.03.04  sjnyn     : 단 적재 버튼 : btnLayerStack 추가(소형 2동 전용)
  2024.01.29  오수현    : E20230901-001504 대차 Tab추가 - '현재LOT ID' text 수정 가능하고 값 저장시 CST상태는 Using, 빈값 저장시는 Empty 처리.
                        : E20230901-001504 CST 정보 조회 부분 함수 합침.'현재LOT ID' text 수정 가능(노란 배경색 지정)
  2024.01.31  오수현    : E20230901-001504 대차 Tab의 CST상태는 Using 처리 BIZ명 수정. 저장시 에러 수정
**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Threading.Tasks;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_124 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        DataTable dtiUse = new DataTable();
        DataTable dtType = new DataTable();
        string[] cellDetlList = { };

        private bool isExceptionErr = false;    // Exception 여부
        private bool isSaveResultUSE = false;   // USE 저장 성공 여부
        private bool isSaveResultEmpty = false; // Empty 저장 성공 여부
        private bool isSaveResultUsing = false; // Using 저장 성공 여부

        public COM001_124()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPrint);
            listAuth.Add(btnSave);
            listAuth.Add(btnCreate);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            chkPrint.IsChecked = true;
            if (LoginInfo.CFG_AREA_ID == "M2")
                btnLayerStack.Visibility = Visibility.Visible;

            SetEvent();

            SetCellDetlList();
        }

        private void SetCellDetlList()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID");

            DataRow dr = dt.NewRow();
            dr["AREAID"] = cboCreateArea.SelectedValue.GetString();
            dt.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_BAS_SEL_CELL_DETL_MNGT_FLAG_AREAID", "INDATA", "OUTDATA", dt, (result, exception) => {
                if(exception != null)
                {
                    Util.MessageException(exception);
                    return;
                }

                if (result.Rows.Count > 0)
                {
                    if (cboCreateCstType.GetString() == "BOX")
                    {
                        cboCreateCellClass.IsEnabled = false;
                        cboCreateProcess.IsEnabled = false;
                    }
                    else if (cboCreateCstType.GetString() == "MGZ")
                    {
                        cboCreateCellClass.IsEnabled = true;
                        cboCreateProcess.IsEnabled = true;
                    }
                }
            });
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo combo = new CommonCombo();

            //동
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE);

            //CST 유형
            String[] sFilter1 = { "", "CST_TYPE" };
            //combo.SetCombo(cboCstType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODES");

            //CST 상태
            String[] sFilter2 = { "", "CSTSTAT" };
            combo.SetCombo(cboCstStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

            //사용여부
            String[] sFilter3 = { "", "IUSE" };
            combo.SetCombo(cboiUse, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODES");

            //생성동
            combo.SetCombo(cboCreateArea, CommonCombo.ComboStatus.NONE, sCase: "AREA");

            //생성 CST 유형
            combo.SetCombo(cboCreateCstType, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODES");

            //셀 상세 코드
            string[] sFilter4 = { "CELL_DETL_CLSS_CODE", "", "Y" };
            combo.SetCombo(cboCreateCellClass, CommonCombo.ComboStatus.SELECT, sFilter: sFilter4, sCase: "COMMCODEATTRS");

            //사용공정
            string[] sFilter5 = { "2D_MAGAZINE_PROC_CODE" };
            combo.SetCombo(cboCreateProcess, CommonCombo.ComboStatus.SELECT, sFilter: sFilter5, sCase: "COMMCODE");

            cboiUse.SelectedIndex = 1;
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

            dtiUse = CommonCodeS("IUSE");

            if (dtiUse != null && dtiUse.Rows.Count > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE");
                dt.Columns.Add("CBO_NAME");

                DataRow newRow = null;

                for (int i = 0; i < dtiUse.Rows.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { dtiUse.Rows[i]["CBO_CODE"].ToString(), dtiUse.Rows[i]["CBO_NAME"].ToString() };
                    dt.Rows.Add(newRow);
                }
            }

            dtType = CommonCodeS("CST_TYPE");

            if (dtType != null && dtType.Rows.Count > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE");
                dt.Columns.Add("CBO_NAME");

                DataRow newRow = null;

                for (int i = 0; i < dtType.Rows.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { dtType.Rows[i]["CBO_CODE"].ToString(), dtType.Rows[i]["CBO_NAME"].ToString() };
                    dt.Rows.Add(newRow);
                }
            }
        }

        private DataTable CommonCodeS(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
                else
                    return null;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }
        #endregion

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


        #region 조회 

        private void CallSearchCstInfo(string searchArea, string messageId, bool isSaveDelay)
        {
            SearchCstInfo(searchArea);

            if (!string.IsNullOrEmpty(messageId))
            {
                Util.MessageInfo(messageId);
            }
        }

        private async Task<bool> WaitCallback()
        {
            bool succeeded = false;
            while (!succeeded)
            {
                if (isSaveResultUSE && isSaveResultEmpty && isSaveResultUsing) succeeded = true;

                if (isExceptionErr) succeeded = true;
                else  await Task.Delay(500);
            }

            return true;
        }

        #region CST 정보 조회
        private void SearchCstInfo(string searchArea)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                if (searchArea.Equals("SEARCH"))
                {
                    inTable.Columns.Add("CSTOWNER", typeof(string));
                    inTable.Columns.Add("CSTTYPE", typeof(string));
                    inTable.Columns.Add("CSTSTAT", typeof(string));
                    inTable.Columns.Add("CSTIUSE", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("CURR_LOTID", typeof(string));
                    inTable.Columns.Add("LANGID", typeof(string));
                }
                else if (searchArea.Equals("ENTER_KEY"))
                {
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("CURR_LOTID", typeof(string));
                    inTable.Columns.Add("LANGID", typeof(string));
                }
                else if (searchArea.Equals("CREATE_INFO"))
                {
                    inTable.Columns.Add("CSTOWNER", typeof(string));
                    inTable.Columns.Add("CSTTYPE", typeof(string));
                    inTable.Columns.Add("CSTSTAT", typeof(string));
                    inTable.Columns.Add("CSTIUSE", typeof(string));
                    inTable.Columns.Add("FROM_DATE", typeof(string));
                    inTable.Columns.Add("TO_DATE", typeof(string));
                    inTable.Columns.Add("LANGID", typeof(string));
                }


                DataRow newRow = inTable.NewRow();
                if (searchArea.Equals("SEARCH"))
                {
                    newRow["CSTOWNER"] = cboArea.SelectedValue.ToString();
                    newRow["CSTSTAT"] = Util.GetCondition(cboCstStatus, bAllNull: true);
                    newRow["CSTIUSE"] = Util.GetCondition(cboiUse, bAllNull: true);
                    newRow["CSTID"] = string.IsNullOrEmpty(txtCSTid.Text.ToString()) ? DBNull.Value : (object)txtCSTid.Text;
                    newRow["CURR_LOTID"] = string.IsNullOrEmpty(txtLotid.Text.ToString()) ? DBNull.Value : (object)txtLotid.Text;
                    newRow["LANGID"] = LoginInfo.LANGID;
                }
                else if (searchArea.Equals("ENTER_KEY"))
                {
                    newRow["CSTID"] = string.IsNullOrEmpty(txtCSTid.Text.ToString()) ? DBNull.Value : (object)txtCSTid.Text;
                    newRow["CURR_LOTID"] = string.IsNullOrEmpty(txtLotid.Text.ToString()) ? DBNull.Value : (object)txtLotid.Text;
                    newRow["LANGID"] = LoginInfo.LANGID;
                }
                else if (searchArea.Equals("CREATE_INFO"))
                {
                    newRow["CSTOWNER"] = cboArea.SelectedValue.ToString();
                    newRow["CSTTYPE"] = (TabLotControl.SelectedItem as C1.WPF.C1TabItem).Tag.GetString();
                    newRow["CSTSTAT"] = Util.GetCondition(cboCstStatus, bAllNull: true);
                    newRow["CSTIUSE"] = Util.GetCondition(cboiUse, bAllNull: true);
                    newRow["FROM_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                    newRow["TO_DATE"] = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
                    newRow["LANGID"] = LoginInfo.LANGID;
                }

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_INFO", "INDATA", "OUTDATA", inTable);

                if (dtMain.Select("CSTTYPE = 'BOX'").Count() > 0)
                {
                    DataTable dtBoxList = dtMain.Select("CSTTYPE = 'BOX'").Count() > 0 ? dtMain.Select("CSTTYPE = 'BOX'").CopyToDataTable() : null;
                    Util.GridSetData(dgdBoxList, dtBoxList, FrameOperation);
                    (dgdBoxList.Columns["CSTIUSE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                    (dgdBoxList.Columns["CSTTYPE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtType.Copy());
                }

                if (dtMain.Select("CSTTYPE = 'MGZ'").Count() > 0)
                {
                    DataTable dtMagList = dtMain.Select("CSTTYPE = 'MGZ'").Count() > 0 ? dtMain.Select("CSTTYPE = 'MGZ'").CopyToDataTable() : null;
                    Util.GridSetData(dgdMagList, dtMagList, FrameOperation);
                    (dgdMagList.Columns["CSTIUSE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                    (dgdMagList.Columns["CSTTYPE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtType.Copy());
                }

                if (dtMain.Select("CSTTYPE = 'JR_TR'").Count() > 0)
                {
                    DataTable dtJRList = dtMain.Select("CSTTYPE = 'JR_TR'").Count() > 0 ? dtMain.Select("CSTTYPE = 'JR_TR'").CopyToDataTable() : null;
                    Util.GridSetData(dgdJRList, dtJRList, FrameOperation);
                }

                if (dtMain.Select("CSTTYPE = 'CT'").Count() > 0)
                {
                    DataTable dtCTList = dtMain.Select("CSTTYPE = 'CT'").Count() > 0 ? dtMain.Select("CSTTYPE = 'CT'").CopyToDataTable() : null;
                    Util.GridSetData(dgdCTList, dtCTList, FrameOperation);
                }
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
        #endregion
        
        #endregion 조회


        #region 저장

        #region 저장 - CST상태 (Empty, Using)
        private void SaveCSTStat()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inEmptyTable = new DataTable();
                inEmptyTable.Columns.Add("SRCTYPE", typeof(string));
                inEmptyTable.Columns.Add("IFMODE", typeof(string));
                inEmptyTable.Columns.Add("CSTID", typeof(string));
                inEmptyTable.Columns.Add("USERID", typeof(string));

                DataTable inUsingTable = new DataTable();
                inUsingTable.Columns.Add("LOTID", typeof(string));
                inUsingTable.Columns.Add("CSTID", typeof(string));
                inUsingTable.Columns.Add("USERID", typeof(string));
                inUsingTable.Columns.Add("SRCTYPE", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgdCTList.Rows)
                {
                    if (row.Type == DataGridRowType.Item &&
                       // && ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified
                        (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True" || DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CURR_LOTID")).Equals("")) // 현재LOTID 공백이면 Empty 처리
                        {
                            DataRow newRow = inEmptyTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CSTID"));
                            newRow["USERID"] = LoginInfo.USERID;

                            inEmptyTable.Rows.Add(newRow);
                        }
                        else // 현재LOTID 공백이 아니면 Using 처리
                        {
                            DataRow newRow = inUsingTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CURR_LOTID"));
                            newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CSTID"));
                            newRow["USERID"] = LoginInfo.USERID;

                            inUsingTable.Rows.Add(newRow);
                        }
                    }
                }

                #region Empty 처리 BIZ
                if (inEmptyTable.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_EMPTY_UI", "INDATA", null, inEmptyTable, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                isExceptionErr = true;
                                HiddenLoadingIndicator();
                                return;
                            }

                            isSaveResultEmpty = true;
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            isExceptionErr = true;
                            HiddenLoadingIndicator();
                        }

                    });
                }
                else
                {
                    isSaveResultEmpty = true;
                }
                #endregion Empty 처리 BIZ

                #region Using 처리 BIZ
                if (inUsingTable.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteService("BR_PRD_REG_CSTID_USING_WND_CT", "INDATA", null, inUsingTable, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                isExceptionErr = true;
                                HiddenLoadingIndicator();
                                return;
                            }

                            isSaveResultUsing = true;

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            isExceptionErr = true;
                            HiddenLoadingIndicator();
                        }
                    });
                }
                else
                {
                    isSaveResultUsing = true;
                }
                #endregion Using 처리 BIZ

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                isExceptionErr = true;
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 저장 - 사용여부
        private void SaveCstIUse()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTIUSE", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("UPDDTTM", typeof(DateTime));

                for (int i = 0; i < dgdBoxList.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgdBoxList, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgdBoxList.Rows[i].DataItem, "CSTID"));
                    newRow["CSTIUSE"] = Util.NVC(DataTableConverter.GetValue(dgdBoxList.Rows[i].DataItem, "CSTIUSE"));
                    newRow["CSTTYPE"] = Util.NVC(DataTableConverter.GetValue(dgdBoxList.Rows[i].DataItem, "CSTTYPE"));
                    newRow["UPDUSER"] = LoginInfo.USERID;
                    newRow["UPDDTTM"] = System.DateTime.Now;

                    inTable.Rows.Add(newRow);
                }

                for (int i = 0; i < dgdMagList.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgdMagList, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgdMagList.Rows[i].DataItem, "CSTID"));
                    newRow["CSTIUSE"] = Util.NVC(DataTableConverter.GetValue(dgdMagList.Rows[i].DataItem, "CSTIUSE"));
                    newRow["CSTTYPE"] = Util.NVC(DataTableConverter.GetValue(dgdMagList.Rows[i].DataItem, "CSTTYPE"));
                    newRow["UPDUSER"] = LoginInfo.USERID;
                    newRow["UPDDTTM"] = System.DateTime.Now;

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("DA_BAS_UPD_CARRIER_OPT", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            isExceptionErr = true;
                            HiddenLoadingIndicator();
                            return;
                        } 
                    
                        isSaveResultUSE = true;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        isExceptionErr = true;
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                isExceptionErr = true;
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion 저장


        #region Print
        private void Label_Print(string sCSTID)
        {
            try
            {
                // 프린터 정보 조회
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                string sDark = string.Empty;

                DataRow drPrtInfo = null;

                //바코드일때 셋팅값 확인
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                PrintLabel(sPrt, sRes, sXpos, sYpos, sCSTID, drPrtInfo);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintLabel(string sPrt, string sRes, string sXpos, string sYpos, string sCSTID, DataRow drPrtInfo)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                dtRqst.Rows[0]["LBCD"] = "LBL0004";
                dtRqst.Rows[0]["PRMK"] = sPrt;
                dtRqst.Rows[0]["RESO"] = sRes;
                dtRqst.Rows[0]["PRCN"] = "1";
                dtRqst.Rows[0]["MARH"] = sXpos;
                dtRqst.Rows[0]["MARV"] = sYpos;
                dtRqst.Rows[0]["ATTVAL001"] = sCSTID;
                dtRqst.Rows[0]["ATTVAL002"] = sCSTID;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    SetLabelPrtHist(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo, sCSTID, "LBL0004");
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

        private void PrintLabel_Test(string sPrt, string sRes, string sXpos, string sYpos, string sCSTID, DataRow drPrtInfo)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                dtRqst.Rows[0]["LBCD"] = "LBL0004";
                dtRqst.Rows[0]["PRMK"] = sPrt;
                dtRqst.Rows[0]["RESO"] = sRes;
                dtRqst.Rows[0]["PRCN"] = "1";
                dtRqst.Rows[0]["MARH"] = sXpos;
                dtRqst.Rows[0]["MARV"] = sYpos;
                dtRqst.Rows[0]["ATTVAL001"] = sCSTID;
                dtRqst.Rows[0]["ATTVAL002"] = sCSTID;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    btnCreate.IsEnabled = true;
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

        private bool PrintZPL(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                // 프린터 환경설정 정보가 없습니다.
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        // Barcode Print 실패
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        // Barcode Print 실패
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        // Barcode Print 실패
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                // 프린터 환경설정에 포트명 항목이 없습니다.
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sLBCD)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = "1";
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = "1";
                newRow["PRT_ITEM03"] = "CST_Manage";

                // 프린터 셋팅정보
                if (drPrtInfo?.Table != null)
                {
                    newRow["PRT_ITEM04"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT]) : "";          // DEFAULTYN
                    newRow["PRT_ITEM05"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME]) : "";        // PORTNAME
                    newRow["PRT_ITEM06"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME]) : "";  // PRINTERNAME
                    newRow["PRT_ITEM07"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE]) : "";  // PRINTERTYPE
                    newRow["PRT_ITEM08"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI]) : "";                  // DPI
                    newRow["PRT_ITEM09"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_X) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_X]) : "";                      // XPOS
                    newRow["PRT_ITEM10"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_Y) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y]) : "";                      // YPOS
                    newRow["PRT_ITEM11"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS]) : "";        // DARKNESS
                    newRow["PRT_ITEM12"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]) : "";      // EQUIPMENT
                    newRow["PRT_ITEM13"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY]) : "";    // PRINTERKEY
                    newRow["PRT_ITEM14"] = drPrtInfo.Table.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE) ? Util.NVC(drPrtInfo[CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE]) : "";        // ISACTIVE
                }
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
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

        #endregion Print


        #region 신규 CST 생성 영역

        // [TEST발행] 버튼
        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                // 프린터 정보 조회
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                string sDark = string.Empty;

                DataRow drPrtInfo = null;

                //바코드일때 셋팅값 확인
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                PrintLabel_Test(sPrt, sRes, sXpos, sYpos, "TESTCSTID00", drPrtInfo);
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

        // [생성] 버튼
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region validation
                if ((Convert.ToDecimal(txtCnt.Value.ToString()) < 0) || (Convert.ToDecimal(txtCnt.Value.ToString()) == 0))
                {
                    Util.MessageValidation("SFU3371");	//수량이 0이상이어야 합니다.
                    return;
                }

                if (Convert.ToDecimal(txtCnt.Value.ToString()) > 50)
                {
                    Util.MessageValidation("SFU4412");	//최대 50개 까지 가능합니다.
                    return;
                }

                if (cboCreateCstType.SelectedValue.GetString() == "MGZ")
                {
                    if (cboCreateCellClass.IsEnabled && cboCreateCellClass.SelectedIndex == 0)
                    {
                        Util.MessageValidation("SFU4891"); //셀 상세 코드를 선택해주세요.
                        return;
                    }

                    if (cboCreateProcess.IsEnabled && cboCreateProcess.SelectedIndex == 0)
                    {
                        Util.MessageValidation("SFU4902"); //사용공정을 선택하세요.
                        return;
                    }
                }
                #endregion

                ShowLoadingIndicator();

                string sCSTID = string.Empty;

                DataTable dtBasicInfo = new DataTable();

                dtBasicInfo.Columns.Add("CSTID", typeof(string));

                for (int i = 0; i < txtCnt.Value; i++)
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("CSTOWNER", typeof(string));
                    inTable.Columns.Add("CSTTYPE", typeof(string));
                    inTable.Columns.Add("CSTIUSE", typeof(string));
                    inTable.Columns.Add("CELL_DETL_CLSS_CODE", typeof(string));
                    inTable.Columns.Add("PROC_CODE", typeof(string));
                    inTable.Columns.Add("INSUSER", typeof(string));
                    inTable.Columns.Add("INSDTTM", typeof(DateTime));
                    inTable.Columns.Add("UPDUSER", typeof(string));
                    inTable.Columns.Add("UPDDTTM", typeof(DateTime));
                    inTable.Columns.Add("CST_DFCT_FLAG", typeof(string));
                    inTable.Columns.Add("CSTSTAT", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["CSTOWNER"] = cboCreateArea.SelectedValue.ToString();
                    newRow["CSTTYPE"] = cboCreateCstType.SelectedValue.ToString();
                    newRow["CSTIUSE"] = "Y";
                    if (cboCreateCellClass.IsEnabled && cboCreateProcess.IsEnabled)
                    {
                        newRow["CELL_DETL_CLSS_CODE"] = cboCreateCellClass.SelectedValue.GetString();
                        newRow["PROC_CODE"] = cboCreateProcess.SelectedValue.GetString();
                    }
                    newRow["INSUSER"] = LoginInfo.USERID;
                    newRow["INSDTTM"] = System.DateTime.Now;
                    newRow["UPDUSER"] = LoginInfo.USERID;
                    newRow["UPDDTTM"] = System.DateTime.Now;
                    newRow["CST_DFCT_FLAG"] = "N";
                    newRow["CSTSTAT"] = "E";

                    inTable.Rows.Add(newRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CREATE_CSTID", "INDATA", "OUTDATA", inTable);

                    if (dtResult.Rows.Count > 0)
                    {
                        sCSTID = Util.NVC(dtResult.Rows[0]["CSTID"]);

                        if (chkPrint.IsChecked.HasValue && (bool)chkPrint.IsChecked)
                            Label_Print(sCSTID);

                        DataRow drInfo = dtBasicInfo.NewRow();
                        drInfo["CSTID"] = sCSTID;
                        dtBasicInfo.Rows.Add(drInfo);
                    }
                    else
                    {
                        Util.MessageValidation("SFU3010");  //CST ID 가 생성되지 않았습니다.
                        return;
                    }
                }

                if (chkCnt.IsChecked.HasValue && (bool)chkCnt.IsChecked)
                {
                    for (int i = 0; i < dtBasicInfo.Rows.Count; i++)
                    {
                        Label_Print(Util.NVC(dtBasicInfo.Rows[i]["CSTID"]));
                        Thread.Sleep(500);
                    }
                }
                else if (chkCnt4.IsChecked.HasValue && (bool)chkCnt4.IsChecked)
                {
                    for (int count = 0; count < 3; count++)
                    {
                        for (int i = 0; i < dtBasicInfo.Rows.Count; i++)
                        {
                            Label_Print(Util.NVC(dtBasicInfo.Rows[i]["CSTID"]));
                            Thread.Sleep(500);
                        }
                    }
                }

                CallSearchCstInfo("CREATE_INFO", "SFU1275", false); // SFU1275:정상 처리 되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        // [CST유형] 콤보값 변경 
        private void cboCreateCstType_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID");

            DataRow dr = dt.NewRow();
            dr["AREAID"] = cboCreateArea.SelectedValue.GetString();
            dt.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_BAS_SEL_CELL_DETL_MNGT_FLAG_AREAID", "INDATA", "OUTDATA", dt, (result, exception) =>
            {

                if (exception != null)
                {
                    Util.MessageException(exception);
                    return;
                }

                if (result.Rows.Count > 0)
                {
                    if (e.NewValue.GetString() == "BOX")
                    {
                        cboCreateCellClass.IsEnabled = false;
                        cboCreateProcess.IsEnabled = false;
                    }
                    else if (e.NewValue.GetString() == "MGZ")
                    {
                        cboCreateCellClass.IsEnabled = true;
                        cboCreateProcess.IsEnabled = true;
                    }
                }
            });
        }

        // [동] 콤보값 변경
        private void cboCreateArea_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID");

            DataRow dr = dt.NewRow();
            dr["AREAID"] = cboCreateArea.SelectedValue.GetString();
            dt.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_BAS_SEL_CELL_DETL_MNGT_FLAG_AREAID", "INDATA", "OUTDATA", dt, (result, exception) =>
            {
                if (exception != null)
                {
                    Util.MessageException(exception);
                    return;
                }

                if (result.Rows.Count > 0)
                {
                    if (cboCreateCstType.SelectedValue.GetString() == "MGZ")
                    {
                        cboCreateCellClass.IsEnabled = true;
                        cboCreateProcess.IsEnabled = true;
                    }
                    else
                    {
                        cboCreateCellClass.IsEnabled = false;
                        cboCreateProcess.IsEnabled = false;
                    }
                }
            });
        }

        // [2장 발행] 체크
        private void chkCnt_Checked(object sender, RoutedEventArgs e)
        {
            if (chkCnt.IsChecked.Value == true)
                chkCnt4.IsChecked = false;
        }
        // [4장 발행] 체크
        private void chkCnt4_Checked(object sender, RoutedEventArgs e)
        {
            if (chkCnt4.IsChecked.Value == true)
                chkCnt.IsChecked = false;
        }

        #endregion 신규 CST 생성 영역


        #region 조회영역 Event

        private void txtCSTid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int cnt = 6;

                if (txtCSTid.Text.Trim() != "")
                {
                    if (txtCSTid.Text.Trim().Length < cnt)
                    {
                        //SFU4342	[%1] 자리수 이상 입력하세요.
                        Util.MessageInfo("SFU4342", new object[] { cnt });
                        return;
                    }
                    else
                    {
                        Util.gridClear(dgdBoxList);
                        Util.gridClear(dgdMagList);
                        Util.gridClear(dgdJRList);
                        Util.gridClear(dgdCTList);
                        CallSearchCstInfo("ENTER_KEY", string.Empty, false);
                    }
                }
            }
        }

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtLotid.Text != "")
                {
                    Util.gridClear(dgdMagList);
                    Util.gridClear(dgdBoxList);
                    Util.gridClear(dgdJRList);
                    Util.gridClear(dgdCTList);
                    CallSearchCstInfo("ENTER_KEY", string.Empty, false);
                }
            }
        }

        // [재발행] 버튼
        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                C1.WPF.DataGrid.C1DataGrid selectedDataGrid = null;

                string sSelectedTabTag = (TabLotControl.SelectedItem as C1.WPF.C1TabItem).Tag.ToString();

                if (sSelectedTabTag == "BOX")
                    selectedDataGrid = dgdBoxList;
                else if(sSelectedTabTag == "MGZ")
                    selectedDataGrid = dgdMagList;
                else if (sSelectedTabTag == "JR_TR")
                    selectedDataGrid = dgdJRList;
                else if (sSelectedTabTag == "CT")
                    selectedDataGrid = dgdCTList;

                int iRowCntTab = _Util.GetDataGridCheckCnt(selectedDataGrid, "CHK");
                if (iRowCntTab < 0)
                {
                    Util.MessageValidation("SFU1651");	//선택된 항목이 없습니다.
                    return;
                }

                for (int i = 0; i < selectedDataGrid.Rows.Count - selectedDataGrid.BottomRows.Count; i++)
                {
                    //Unchecked
                    if ((int)DataTableConverter.GetValue(selectedDataGrid.Rows[i].DataItem, "CHK") == 0)
                        continue;

                    Label_Print((string)DataTableConverter.GetValue(selectedDataGrid.Rows[i].DataItem, "CSTID"));
                    Thread.Sleep(500);
                }
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

        // [단적재] 버튼
        private void btnLayerStack_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_LAYER_STACK popupLayerStack = new CMM_ASSY_LAYER_STACK { FrameOperation = FrameOperation };

            object[] parameters = new object[0];
            C1WindowExtension.SetParameters(popupLayerStack, parameters);

            popupLayerStack.Closed += new EventHandler(btnLayerStack_Click_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupLayerStack.ShowModal()));
        }

        // 단적재 팝업창 [닫기] 버튼
        private void btnLayerStack_Click_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_LAYER_STACK window = sender as CMM_ASSY_LAYER_STACK;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        // [조회] 버튼
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string messageId = string.Empty;
            CallSearchCstInfo("SEARCH", messageId, false);
        }

        // [발행] 버튼
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                # region Validation
                int iRow = -1;

                if (tabBox.IsSealed)
                    iRow = _Util.GetDataGridCheckFirstRowIndex(dgdBoxList, "CHK");
                else if (tabMagzine.IsSealed)
                    iRow = _Util.GetDataGridCheckFirstRowIndex(dgdMagList, "CHK");
                else if (tabJellyRoll.IsSealed)
                    iRow = _Util.GetDataGridCheckFirstRowIndex(dgdJRList, "CHK");
                else if (tabCT.IsSealed)
                    iRow = _Util.GetDataGridCheckFirstRowIndex(dgdCTList, "CHK");

                if (iRow < 0)
                {
                    Util.MessageValidation("SFU1651");	//선택된 항목이 없습니다.
                    return;
                }
                # endregion Validation

                ShowLoadingIndicator();

                // 프린터 정보 조회
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                string sDark = string.Empty;

                DataRow drPrtInfo = null;

                //바코드일때 셋팅값 확인
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                if (tabBox.IsSealed)
                {
                    for (int i = 0; i < dgdBoxList.GetRowCount(); i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgdBoxList, "CHK", i)) continue;
                        PrintLabel(sPrt, sRes, sXpos, sYpos, Util.NVC(DataTableConverter.GetValue(dgdBoxList.Rows[i].DataItem, "CSTID")), drPrtInfo);
                    }
                }
                else if (tabMagzine.IsSealed)
                {
                    for (int i = 0; i < dgdMagList.GetRowCount(); i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgdMagList, "CHK", i)) continue;
                        PrintLabel(sPrt, sRes, sXpos, sYpos, Util.NVC(DataTableConverter.GetValue(dgdMagList.Rows[i].DataItem, "CSTID")), drPrtInfo);
                    }
                }
                else if (tabJellyRoll.IsSealed)
                {
                    for (int i = 0; i < dgdJRList.GetRowCount(); i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgdJRList, "CHK", i)) continue;
                        PrintLabel(sPrt, sRes, sXpos, sYpos, Util.NVC(DataTableConverter.GetValue(dgdJRList.Rows[i].DataItem, "CSTID")), drPrtInfo);
                    }
                }
                else if (tabCT.IsSealed)
                {
                    for (int i = 0; i < dgdCTList.GetRowCount(); i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgdCTList, "CHK", i)) continue;
                        PrintLabel(sPrt, sRes, sXpos, sYpos, Util.NVC(DataTableConverter.GetValue(dgdCTList.Rows[i].DataItem, "CSTID")), drPrtInfo);
                    }
                }
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

        // [저장] 버튼
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int iRowCntTab1 = _Util.GetDataGridCheckCnt(dgdBoxList, "CHK");
            int iRowCntTab2 = _Util.GetDataGridCheckCnt(dgdMagList, "CHK");
            int iRowCntTab3 = _Util.GetDataGridCheckCnt(dgdJRList, "CHK");
            int iRowCntTab4 = _Util.GetDataGridCheckCnt(dgdCTList, "CHK");

            if (iRowCntTab1 < 0 && iRowCntTab2 < 0 && iRowCntTab3 < 0 && iRowCntTab4 < 0)
            {
                Util.MessageValidation("SFU1651");	//선택된 항목이 없습니다.
                return;
            }

            isExceptionErr = false;

            // 1. 사용여부 Save
            if (iRowCntTab1 > 0 || iRowCntTab2 > 0)
            {
                isSaveResultUSE = false;

                SaveCstIUse();
            }
            else if (iRowCntTab1 == 0 && iRowCntTab2 == 0)
            {
                isSaveResultUSE = true;
            }

            // 2. CST상태 (Empty, Using) Save
            if (iRowCntTab4 > 0)
            {
                isSaveResultEmpty = false;
                isSaveResultUsing = false;
                
                SaveCSTStat();
            }
            else if (iRowCntTab4 == 0)
            {
                isSaveResultEmpty = true;
                isSaveResultUsing = true;
            }

            // 3. CST 조회
            Task<bool> task = WaitCallback();
            task.ContinueWith(_ =>
            {
                if (!isExceptionErr)
                {
                    CallSearchCstInfo("SEARCH", "SFU1275", true); // SFU1275:정상 처리 되었습니다.
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion 조회영역 Event


        private void dgdCTList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["CURR_LOTID"].Index).Presenter.Background = new SolidColorBrush(Colors.Yellow); //현재 LOTID 
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}

