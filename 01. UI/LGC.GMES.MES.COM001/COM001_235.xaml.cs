/*************************************************************************************
 Created Date : 2018.05.10
      Creator : 
   Decription : 유효일자 수정
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.10  DEVELOPER : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Windows.Media;
using System.Linq;
using C1.WPF;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_235 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        List<string> LotList = new List<string>();

        public COM001_235()
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
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboLine };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboLine };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);


            String[] sFilter = { LoginInfo.CFG_AREA_ID };

            //동
            C1ComboBox[] cboVldAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(AREA, CommonCombo.ComboStatus.SELECT, cbChild: cboVldAreaChild);

            C1ComboBox[] cboLineParent = { AREA };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

        }
        #endregion



        #region Funct
        private void Init()
        {
            Util.gridClear(dgSearch);
            Util.gridClear(dgSearchList);

            chkOnlyOne.IsChecked = false;
            dtpDateFix.IsEnabled = false;
            txtRemark.Text = "";
        }

        private void saveVLDDate()
        {
            try
            {
                ShowLoadingIndicator();

                bool bCheck = (bool)chkOnlyOne.IsChecked;

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("VLD_DATE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPNOTE"] = txtRemark.Text;
                if (bCheck)
                {
                    newRow["VLD_DATE"] = Util.GetCondition(dtpDateFix);
                } else
                {
                    newRow["VLD_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                }
                inDataTable.Rows.Add(newRow);

                DataTable inLOT = indataSet.Tables.Add("INLOT");
                inLOT.Columns.Add("LOTID", typeof(string));
                inLOT.Columns.Add("PRODID", typeof(string));
                inLOT.Columns.Add("PROD_VER_CODE", typeof(string));

                for (int i = 0; i < dgSearch.GetRowCount(); i++)
                {
                    newRow = null;
                    newRow = inLOT.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "LOTID"));
                    newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "PRODID"));
                    newRow["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "PROD_VER_CODE"));
                    inLOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_VLD_DATE_ELTR", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Util.gridClear(dgSearch);
                        Util.gridClear(dgSearchList);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void getLotList()
        {
            try
            {
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                //{
                //    Util.MessageValidation("SFU2042", "31");   //기간은 {0}일 이내 입니다.
                //    return;
                //}

                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("SKIDID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.GetCondition(cboLine, "SFU1223");
                if (dr["EQSGID"].Equals("")) return;
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");
                if (dr["PROCID"].Equals("")) return;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                if (!string.IsNullOrWhiteSpace(txtLOTID.Text))
                    dr["LOTID"] = txtLOTID.Text;
                //if ((bool)rdoLot.IsChecked)
                //{
                //if (!string.IsNullOrWhiteSpace(txtLOTID.Text))
                //    dr["LOTID"] = txtLOTID.Text;
                //}
                //if ((bool)rdoSkid.IsChecked)
                //{
                //    if (!string.IsNullOrWhiteSpace(txtLOTID.Text))
                //        dr["SKIDID"] = txtLOTID.Text;
                //}
                if (!string.IsNullOrWhiteSpace(txtPRODID.Text))
                    dr["PRODID"] = txtPRODID.Text;
                if (!string.IsNullOrWhiteSpace(txtPRJ.Text))
                    dr["PRJT_NAME"] = txtPRJ.Text;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_VLDDATE_LOT_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSearch, dtRslt, FrameOperation);

                Util.gridClear(dgSearchList);

                chkOnlyOne.IsChecked = false;
                dtpDateFix.IsEnabled = false;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private bool getMultiLotList(string sLotid)
        {
            try
            {
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("SKIDID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");
                if (dr["AREAID"].Equals("")) return false;
                dr["EQSGID"] = Util.GetCondition(cboLine, "SFU1223");
                if (dr["EQSGID"].Equals("")) return false;
                if (!string.IsNullOrWhiteSpace(sLotid))
                    dr["LOTID"] = sLotid;
                //if ((bool)rdoLot.IsChecked)
                //    dr["LOTID"] = sLotid;
                //if ((bool)rdoSkid.IsChecked)
                //    dr["SKIDID"] = sLotid;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_VLDDATE_LOT_LIST", "INDATA", "OUTDATA", inTable);

                if (dgSearch.GetRowCount() == 0)
                {
                    Util.GridSetData(dgSearch, dtResult, FrameOperation);
                }
                else
                {
                    for (int i = 0; i < dgSearch.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "LOTID").ToString() == Util.NVC(dtResult.Rows[0]["LOTID"]))
                        {
                            LotList.Add(sLotid); //동일한 LOT이 스캔되었습니다.
                            return true;
                        }
                    }
                    DataTable dtInfo = DataTableConverter.Convert(dgSearch.ItemsSource);

                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgSearch, dtInfo, FrameOperation);

                    if (dgSearch.GetRowCount() > 0)
                    {

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void getChangeLotList(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_VLDDATE_TARGET_LOT", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSearchList, dtRslt, FrameOperation);
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
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private bool IsPersonByAuth(string sUserID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = sUserID;
                dr["AUTHID"] = "MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getLotList();
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }

        private void btnSave_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgSearch.GetRowCount() == 0)
            {
                HiddenLoadingIndicator();
                Util.MessageInfo("SFU3536");    //조회된 결과가 없습니다.
                return;
            }

            bool bCheck = (bool)chkOnlyOne.IsChecked;

            if (bCheck)
            {
                string sRemark = Util.GetCondition(txtRemark, "SFU1594"); //"사유를 입력하세요
                if (sRemark.Equals(""))
                {
                    HiddenLoadingIndicator();
                    return;
                }
                string IsPerson = IsPersonByAuth(LoginInfo.USERID).ToString();
                if (!IsPersonByAuth(LoginInfo.USERID))
                {
                    //해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                    Util.MessageValidation("SFU3520", LoginInfo.USERID, ObjectDic.Instance.GetObjectName("관리자권한"));
                    HiddenLoadingIndicator();
                    return;
                }
            }

            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    saveVLDDate();
                }
                HiddenLoadingIndicator();
            });

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgSearch.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(false) ||
                DataTableConverter.GetValue(rb.DataContext, "CHK").Nvc().Equals("0"))
            {
                //체크시 처리될 로직
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                getChangeLotList(sLotId);
            }
        }

        private void chkOnlyOne_Click(object sender, RoutedEventArgs e)
        {
            bool bCheck = (bool)chkOnlyOne.IsChecked;
            if (bCheck)
                dtpDateFix.IsEnabled = true;
            else
                dtpDateFix.IsEnabled = false;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgSearch.ItemsSource);

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

            dt.Rows.Remove(dt.Rows[idx]);
            Util.GridSetData(dgSearch, dt, FrameOperation);
        }
/*
        private void txtLOTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    Init();
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 50)
                    {
                        Util.MessageValidation("SFU4412");   //최대 50개 까지 가능합니다.
                        return;
                    }

                    string _ValueToFind = string.Empty;

                    LotList.Clear();

                    if (sPasteStrings.Length >= 1)
                    {
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sPasteStrings[i]) && getMultiLotList(sPasteStrings[i].ToString()) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }

                        _ValueToFind = string.Join(",", LotList);

                        if (_ValueToFind != "")
                        {
                            Util.MessageValidation("SFU4306", _ValueToFind);  // 입력한 LOTID[%1] 정보를 확인하십시오.
                        }

                        e.Handled = true;
                    }
                    else
                        e.Handled = false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtLOTID.Text = "";
                    txtLOTID.Focus();

                    //HiddenLoadingIndicator();
                }
            }
        }
*/
        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Init();
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);


                    string _ValueToFind = string.Empty;

                    LotList.Clear();


                            if (!string.IsNullOrEmpty(sPasteStrings[0]) && getMultiLotList(sPasteStrings[0].ToString()) == false)


                            System.Windows.Forms.Application.DoEvents();
                        

                        _ValueToFind = string.Join(",", LotList);

                        if (_ValueToFind != "")
                        {
                            Util.MessageValidation("SFU4306", _ValueToFind);  // 입력한 LOTID[%1] 정보를 확인하십시오.
                        }

                        e.Handled = true;

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtLOTID.Text = "";
                    txtLOTID.Focus();

                    //HiddenLoadingIndicator();
                }
            }
        }

        #endregion

        private void txtVldLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            vldHistList();
        }

        private void txtVldSKIDID_KeyDown(object sender, KeyEventArgs e)
        {
            vldHistList();
        }

        private void txtVldProd_KeyDown(object sender, KeyEventArgs e)
        {
            vldHistList();
        }

        private void btnVldSearch_Click(object sender, RoutedEventArgs e)
        {
            vldHistList();
        }
        private void vldHistList()
        {
            try { 
            ShowLoadingIndicator();

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("EQSGID", typeof(string));
            dtRqst.Columns.Add("PROCID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("SKIDID", typeof(string));
            dtRqst.Columns.Add("FROM_DATE", typeof(string));
            dtRqst.Columns.Add("TO_DATE", typeof(string));
            dtRqst.Columns.Add("PRODID", typeof(string));
            dtRqst.Columns.Add("PRJT_NAME", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;

            dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");
            if (dr["AREAID"].Equals("")) return;
            dr["EQSGID"] = Util.GetCondition(cboLine, "SFU1223");
            if (dr["EQSGID"].Equals("")) return;                        
            dr["FROM_DATE"] = Util.GetCondition(dtpVldDateFrom);
            dr["TO_DATE"] = Util.GetCondition(dtpVldDateTo);
            if (!string.IsNullOrWhiteSpace(txtVldLotID.Text))
                  dr["LOTID"] = txtVldLotID.Text;
            if (!string.IsNullOrWhiteSpace(txtVldSkidID.Text))
                dr["SKIDID"] = txtVldSkidID.Text;
            if (!string.IsNullOrWhiteSpace(txtVldProdID.Text))
                dr["PRODID"] = txtVldProdID.Text;

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_CHANGE_VLDDATE_HIST", "INDATA", "OUTDATA", dtRqst);

            Util.GridSetData(dgVldHistlist, dtRslt, FrameOperation);
            }catch(Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

    }
}
