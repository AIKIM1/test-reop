/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 자재입고 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2020.01.28  손우석 CSR ID 25026 시스템 개선을 통해 수동 포장시 타 모델 W/O로 오 포장 방지 [요청번호] C20200123-000214
  2020.02.12  손우석 CSR ID 25026 시스템 개선을 통해 수동 포장시 타 모델 W/O로 오 포장 방지 [요청번호] C20200123-000214
  2021.06.18  염규범      SI      PACK 반품 재투입 오류 수정의 건
  2021.11.11  김민석      SI      포장구분 콤보박스에서 Pack항목 제외처리
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

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_025 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        string sTagetArea = string.Empty;
        string sTagetEqsg = string.Empty;
        bool bUseReworkWO = true;
        //2020.01.28
        string sPRODID = string.Empty;
        string sProcessType = "";

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_025()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //작업자 조회 동
                C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.ALL, cbChild: cboSearchAREAIDChild, sCase: "AREA");

                //작업자 조회 라인
                C1ComboBox[] cboSearchEQSGIDParent = { cboSearchAREAID };
                C1ComboBox[] cboLineChild = { cboProcessSegmentByEqsgid };
                _combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.ALL, cbParent: cboSearchEQSGIDParent, cbChild: cboLineChild, sCase: "EQUIPMENTSEGMENT");

                //공정군
                C1ComboBox[] cbProcessParent = { cboSearchEQSGID };
                _combo.SetCombo(cboProcessSegmentByEqsgid, CommonCombo.ComboStatus.ALL, cbParent: cbProcessParent, sCase: "ProcessSegmentByEqsgid_M_P");

                C1ComboBox cboConfigArea = new C1ComboBox();
                cboConfigArea.SelectedValue = LoginInfo.CFG_AREA_ID;
                //라인
                C1ComboBox[] cboTargetEQSGIDParent = { cboConfigArea };
                C1ComboBox[] cboTargetEQSGIDChild = { cboTargetProcessSegmentByEqsgid };
                _combo.SetCombo(cboTargetEQSGID, CommonCombo.ComboStatus.NONE, cbParent: cboTargetEQSGIDParent, cbChild: cboTargetEQSGIDChild, sCase: "EQUIPMENTSEGMENT");

                //공정군
                C1ComboBox[] cboTargetProcessSegmentByEqsgidParent = { cboTargetEQSGID };
                _combo.SetCombo(cboTargetProcessSegmentByEqsgid, CommonCombo.ComboStatus.ALL, cbParent: cboTargetProcessSegmentByEqsgidParent, sCase: "ProcessSegmentByEqsgid_M_P");

                //포장구분
                setPackGubun(cboPackGubun);
                //btnWorkOroderSearch.IsEnabled = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
                             
        #endregion

        #region Event

        #region Event - UserControl

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnTagetInputComfirm);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                DateTime DateNow = DateTime.Now;
                DateTime firstOfThisMonth = new DateTime(DateNow.Year, DateNow.Month, 1);
                DateTime firstOfNextMonth = new DateTime(DateNow.Year, DateNow.Month, 1).AddMonths(1);
                DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);

                dtpDateFrom.SelectedDateTime = firstOfThisMonth;
                dtpDateTo.SelectedDateTime = lastOfThisMonth;
                 
                setComboBox();

                tbTagetListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                chkUSEReworkWO();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }

        }

        #endregion UserControl

        #region Event - Button        

        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTagetList.Rows.Count > 0)
                {
                    if (chkInputData()) //입력 체크
                    {
                        if ((bool)chkKeypartAllDetach.IsChecked)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1407"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            //KEYPART 해체 하시겠습니까?
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    setReworkDetach();
                                }
                            }
                            );
                        }
                        else
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1407"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            //PACK 재투입 하시겠습니까?
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    setReworkInput();
                                }
                            }
                            );
                        }
                        
                    }
                }
                else
                {                    
                    ms.AlertWarning("SFU1796"); //입고 대상이 없습니다. LOTID를 입력 하세요.
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //전체 취소 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    //for (int i = (dgTagetList.Rows.Count - 1); i >= 0; i--)
                    //{
                    //    dgTagetList.RemoveRow(i);
                    //}

                    Util.gridClear(dgTagetList);

                    clearInput();
                }
            }
            );
        }

        private void btnTagetSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (util.GetDataGridCheckCnt(dgTagetList, "CHK") > 0)
                {
                    DataTable dtTempTagetList = DataTableConverter.Convert(dgTagetList.ItemsSource);

                    for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                    {
                        if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                            Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                        {

                            dtTempTagetList.Rows[i].Delete();
                            dtTempTagetList.AcceptChanges();
                        }
                    }
                    dgTagetList.ItemsSource = DataTableConverter.Convert(dtTempTagetList);

                    if (!(dtTempTagetList.Rows.Count > 0))
                    {
                        dgTagetList.ItemsSource = null;
                        clearInput();
                    }
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getAs_ReworkInputHist();
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        private void btnReturnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnTagetCell_By_Excel();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnWorkOroderSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_025_WORKORDERSELECT popup = new PACK001_025_WORKORDERSELECT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("EQSGID", typeof(string));
                    //2020.01.28
                    dtData.Columns.Add("PRODID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["EQSGID"] = Util.NVC(cboTargetEQSGID.SelectedValue);
                    //2020.01.28
                    newRow["PRODID"] = sPRODID;

                    dtData.Rows.Add(newRow);

                    //2020.01.28
                    //object[] Parameters = new object[1];
                    object[] Parameters = new object[2];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion Button

        #region Event - TextBox

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    getTagetLotInfo();
                }
            }
            catch (Exception ex)
            {
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        #endregion TextBox

        #region Event - ComboBox

        private void cboSearchProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                txtSearchProduct.Text = e.NewValue.ToString();
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        private void cboTagetModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                //setComboBox_Route_schd(Util.NVC(cboTagetModel.SelectedValue), txtTagetPRODID.Text);
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        #endregion ComboBox

        #region Event - DataGrid

        private void dgSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name == "LOTID")
                    {
                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        private void dgTagetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTagetList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if(cell.Column.Name == "LOTID")
                        {
                            this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        private void dgTagetList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOTID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
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
                Util.Alert(ex.Message);
            }
        }

        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "LOTID")
                        {
                            this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }

        #endregion DataGrid

        #region Event - CheckBox

        private void chkKeypartAllDetach_Checked(object sender, RoutedEventArgs e)
        {
            
            //string sPCSGID = Util.NVC(txtTagetPCSGNAME.Tag);
            string sPCSGID = Util.NVC(cboTargetProcessSegmentByEqsgid.SelectedValue);

            if (sPCSGID != "P")
            {
                ms.AlertInfo("SFU3400"); // KEYPART 일괄해체는 BMA제품만 가능합니다.
                chkKeypartAllDetach.IsChecked = false;
            }
            else
            {
                cboTaget_INPUTPROCID.IsEnabled = false;
            }

             if((!sProcessType.Equals("BP") && !sProcessType.Equals("")))
            {

                ms.AlertInfo("SFU8526"); //  KEYPART 일괄해체는 처리유형코드가 BR(반품팩재출하)만 가능합니다. 
                chkKeypartAllDetach.IsChecked = false;
            }
        }

        private void chkKeypartAllDetach_Unchecked(object sender, RoutedEventArgs e)
        {
            cboTaget_INPUTPROCID.IsEnabled = true;
        }

        #endregion CheckBox

        #endregion Event

        #region Mehod        

        private bool chkInputData()
        {
            bool bReturn = true;

            try
            {
                #region 투입공정를선택하세요
                if (cboTaget_INPUTPROCID.SelectedIndex < 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1968"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    bReturn = false;
                    cboTaget_INPUTPROCID.Focus();
                    return bReturn;
                }
                #endregion

                if (bUseReworkWO)
                {
                    #region 재작업 Work Order 정보가 존재하지 않습니다.
                    if (!(txtReworkWOID.Text.Length > 0))
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("100092"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        bReturn = false;
                        return bReturn;
                    }
                    #endregion
                }




            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }

        private void setReworkInput()
        {
            try
            {
                DataTable dtTagetList = DataTableConverter.Convert(dgTagetList.ItemsSource);
                
                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";  
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PCSGID", typeof(string));
                if(bUseReworkWO) INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["EQSGID"] = Util.NVC(cboTargetEQSGID.SelectedValue);// txtTagetEQSGNAME.Tag;
                drINDATA["PCSGID"] = Util.NVC(cboTargetProcessSegmentByEqsgid.SelectedValue);//txtTagetPCSGNAME.Tag;
                //if(bUseReworkWO) drINDATA["WOID"] = txtReworkWOID.Text;
                if (bUseReworkWO) drINDATA["WOID"] = txtReworkWOID.Text == "" ? null : txtReworkWOID.Text;                
                drINDATA["PROCID"] = cboTaget_INPUTPROCID.SelectedValue;
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);


                DataRow drINLOT = null;
                DataTable dtINLOT = new DataTable();
                dtINLOT.TableName = "INLOT";
                dtINLOT.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    drINLOT = dtINLOT.NewRow();
                    drINLOT["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "LOTID"));
                    dtINLOT.Rows.Add(drINLOT);
                }

                DataSet dsIndata = new DataSet();
                dsIndata.Tables.Add(INDATA);
                dsIndata.Tables.Add(dtINLOT);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETURN_REWORK_LOT", "INDATA,INLOT", "OUTDATA", dsIndata, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {

                    if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                    {
                        if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {                            
                            ms.AlertInfo("SFU1380"); //LOT을 재투입하였습니다.

                            Util.gridClear(dgTagetList);
                            clearInput();
                            getAs_ReworkInputHist();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void setReworkDetach()
        {
            try
            {
                //CMA 재작업 WO 는 있으면 UPDATE 
                //if(!(Util.NVC(txtReworkWOID.Tag).Length > 0))
                //{
                //    ms.AlertInfo("SFU1442"); //WORK ORDER ID가 지정되지 않거나 없습니다.
                //    return;
                //}

                DataTable dtTagetList = DataTableConverter.Convert(dgTagetList.ItemsSource);

                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PCSGID", typeof(string));
                if (bUseReworkWO) INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                if (bUseReworkWO) INDATA.Columns.Add("WOID_CMA", typeof(string));

                drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["EQSGID"] = Util.NVC(cboTargetEQSGID.SelectedValue);// txtTagetEQSGNAME.Tag;
                drINDATA["PCSGID"] = Util.NVC(cboTargetProcessSegmentByEqsgid.SelectedValue);//txtTagetPCSGNAME.Tag;
                if (bUseReworkWO) drINDATA["WOID"] =  txtReworkWOID.Text == "" ? null : txtReworkWOID.Text;
                //drINDATA["WOID"] = txtReworkWOID.Text;
                drINDATA["PROCID"] = cboTaget_INPUTPROCID.SelectedValue;
                drINDATA["USERID"] = LoginInfo.USERID;
                //drINDATA["WOID_CMA"] = txtReworkWOID.Tag;
                if (bUseReworkWO) drINDATA["WOID_CMA"] = txtReworkWOID.Tag == "" ? null : txtReworkWOID.Tag;
                INDATA.Rows.Add(drINDATA);


                DataRow drINLOT = null;
                DataTable dtINLOT = new DataTable();
                dtINLOT.TableName = "INLOT";
                dtINLOT.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    drINLOT = dtINLOT.NewRow();
                    drINLOT["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "LOTID"));
                    dtINLOT.Rows.Add(drINLOT);
                }

                DataSet dsIndata = new DataSet();
                dsIndata.Tables.Add(INDATA);
                dsIndata.Tables.Add(dtINLOT);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETURN_REWORK_LOT_DETACH", "INDATA,INLOT", "OUTDATA", dsIndata, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {

                    if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                    {
                        if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            ms.AlertInfo("SFU1380"); //LOT을 재투입하였습니다.

                            Util.gridClear(dgTagetList);
                            clearInput();
                            getAs_ReworkInputHist();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void getAs_ReworkInputHist()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboSearchAREAID.SelectedValue) =="" ? null : Util.NVC(cboSearchAREAID.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboSearchEQSGID.SelectedValue) ==""? null : Util.NVC(cboSearchEQSGID.SelectedValue);
                dr["PCSGID"] = Util.NVC(cboProcessSegmentByEqsgid.SelectedValue) == "" ? null : Util.NVC(cboProcessSegmentByEqsgid.SelectedValue);
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_AS_PACK_INPUT_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                
                //dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dgSearchResultList.Rows.Count));
            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_RETURN_AS_PACK_INPUT_HIST", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            
        }

        private void getTagetLotInfo()
        {
            try
            {
                DataSet dsIndata = new DataSet();
                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtINDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotID.Text;
                dtINDATA.Rows.Add(dr);
                dsIndata.Tables.Add(dtINDATA);

                DataSet dsResult = new DataSet();

                if (Util.GetCondition(cboPackGubun) == "OWMS")
                {
                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_RETURN_REWORK_LOT_V01", "INDATA", "RETURN_LOT_INFO,RETURN_PROC", dsIndata, null);
                }
                else if (Util.GetCondition(cboPackGubun) == "PACK")
                {
                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_RETURN_REWORK_LOT_V01_PACK", "INDATA", "RETURN_LOT_INFO,RETURN_PROC", dsIndata, null);
                }
                
                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_RETURN_REWORK_LOT", "INDATA", "RETURN_LOT_INFO,RETURN_PROC", dsIndata, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    if ((dsResult.Tables.IndexOf("RETURN_LOT_INFO") > -1))
                    {
                        if (dsResult.Tables["RETURN_LOT_INFO"].Rows.Count > 0)
                        {
                            if (chkLotInput(dsResult.Tables["RETURN_LOT_INFO"]) ) //Lot 정보 체크
                            {
                                return;
                            }
                            else
                            {
                                DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);
                                dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["WOID"] = txtReworkWOID.Text;

                                dtBefore.Merge(dsResult.Tables["RETURN_LOT_INFO"]);
                                dgTagetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dgTagetList.Rows.Count));

                                string sPcsgID = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["PCSGID"]);     
                                
                                //txtTagetEQSGNAME.Tag = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["EQSGID"]);
                                //txtTagetEQSGNAME.Text = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["EQSGNAME"]);
                                //txtTagetPCSGNAME.Tag = sPcsgID;
                                //txtTagetPCSGNAME.Text = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["PCSGNAME"]);
                                //txtReworkWOID.Text = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["WOID"]);
                                //txtReworkWOID.Tag = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["WOID_CMA"]);

                                //2020.01.28
                                sPRODID = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["PRODID"]);

                                if (sPcsgID != "P")
                                {
                                    if ((bool)chkKeypartAllDetach.IsChecked)
                                    {
                                        Util.MessageInfo("SFU3400"); // KEYPART일괄 해체는 BMA제품만 가능합니다.
                                        chkKeypartAllDetach.IsChecked = false;
                                        return;
                                    }
                                }

                                // 2020.04.03 염규범S
                                // W/O 이미 선택시에는 선택 못하도록 처리
                                if (bUseReworkWO && String.IsNullOrWhiteSpace(txtReworkWOID.Text.ToString())){ 
                                    //2020.01.28
                                    btnWorkOroderSearch.IsEnabled = true;
                                }
                            }
                        }
                    }

                    if ((dsResult.Tables.IndexOf("RETURN_PROC") > -1))
                    {
                        if (dsResult.Tables["RETURN_PROC"].Rows.Count > 0)
                        {
                            cboTaget_INPUTPROCID.ItemsSource = DataTableConverter.Convert(dsResult.Tables["RETURN_PROC"]);

                            if (dsResult.Tables["RETURN_PROC"].Rows.Count > 0)
                            {
                                if (procIdFixed())
                                {
                                    cboTaget_INPUTPROCID.IsEnabled = false;
                                }
                                else
                                {
                                    cboTaget_INPUTPROCID.SelectedIndex = 0;
                                    cboTaget_INPUTPROCID.IsEnabled = true;
                                }
                            }
                        }
                    }

                    cboTargetEQSGID.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_CHK_RETURN_REWORK_LOT", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        private bool getTagetLotInfo_multi()
        {
            bool bReturnValue = true;
            try
            {
                DataSet dsIndata = new DataSet();
                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtINDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotID.Text;
                dtINDATA.Rows.Add(dr);
                dsIndata.Tables.Add(dtINDATA);

                DataSet dsResult = new DataSet();

                if(Util.GetCondition(cboPackGubun) == "OWMS")
                {
                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_RETURN_REWORK_LOT_V01", "INDATA", "RETURN_LOT_INFO,RETURN_PROC", dsIndata, null);
                }
                else if(Util.GetCondition(cboPackGubun) == "PACK")
                {
                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_RETURN_REWORK_LOT_V01_PACK", "INDATA", "RETURN_LOT_INFO,RETURN_PROC", dsIndata, null);
                }
                
                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_RETURN_REWORK_LOT", "INDATA", "RETURN_LOT_INFO,RETURN_PROC", dsIndata, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {

                    if ((dsResult.Tables.IndexOf("RETURN_LOT_INFO") > -1))
                    {

                        if (dsResult.Tables["RETURN_LOT_INFO"].Rows.Count > 0)
                        {

                            if (chkLotInput(dsResult.Tables["RETURN_LOT_INFO"])) //Lot 정보 체크
                            {
                                bReturnValue = false;
                            }
                            else
                            {
                                DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);
                                dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["WOID"] = txtReworkWOID.Text;

                                dtBefore.Merge(dsResult.Tables["RETURN_LOT_INFO"]);
                                dgTagetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dgTagetList.Rows.Count));

                                string sPcsgID = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["PCSGID"]);

                                //txtTagetEQSGNAME.Tag = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["EQSGID"]);
                                //txtTagetEQSGNAME.Text = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["EQSGNAME"]);
                                //txtTagetPCSGNAME.Tag = sPcsgID;
                                //txtTagetPCSGNAME.Text = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["PCSGNAME"]);
                                //txtReworkWOID.Text = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["WOID"]);
                                //txtReworkWOID.Tag = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["WOID_CMA"]);

                                //2020.01.28
                                sPRODID = Util.NVC(dsResult.Tables["RETURN_LOT_INFO"].Rows[0]["PRODID"]);

                                if (sPcsgID != "P")
                                {
                                    if ((bool)chkKeypartAllDetach.IsChecked)
                                    {
                                        ms.AlertInfo("SFU3400"); // KEYPART일괄 해체는 BMA제품만 가능합니다.
                                        chkKeypartAllDetach.IsChecked = false;
                                        bReturnValue = false;
                                    }
                                }

                                // 2020.04.03 염규범S
                                // W/O 이미 선택시에는 선택 못하도록 처리
                                if (String.IsNullOrWhiteSpace(txtReworkWOID.Text.ToString()))
                                {
                                    //2020.01.28
                                    btnWorkOroderSearch.IsEnabled = true;
                                }
                            }
                        }
                    }

                    if ((dsResult.Tables.IndexOf("RETURN_PROC") > -1))
                    {
                        if (dsResult.Tables["RETURN_PROC"].Rows.Count > 0)
                        {
                            cboTaget_INPUTPROCID.ItemsSource = DataTableConverter.Convert(dsResult.Tables["RETURN_PROC"]);

                            if (dsResult.Tables["RETURN_PROC"].Rows.Count > 0)
                            {
                                if (procIdFixed())
                                {
                                    cboTaget_INPUTPROCID.IsEnabled = false;
                                }
                                else
                                {
                                    cboTaget_INPUTPROCID.SelectedIndex = 0;
                                    cboTaget_INPUTPROCID.IsEnabled = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bReturnValue = false;
                //Util.AlertByBiz("BR_PRD_CHK_RETURN_REWORK_LOT", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }

            if (bReturnValue)
            {
                cboTargetEQSGID.IsEnabled = false;
            }

            return bReturnValue;
        }

        private bool chkLotInput(DataTable dtResult)
        {
            bool bResult = false;
            try
            {
                DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);


                if (Util.NVC(cboTargetEQSGID.SelectedValue) != Util.NVC(dtResult.Rows[0]["EQSGID"]))
                {
                    ms.AlertInfo("90068"); // 투입 가능한 라인이 아닙니다.
                    bResult = true;
                    return bResult;
                }

                if (!(dtResult.Rows.Count > 0))
                {                   
                    ms.AlertWarning("SFU1888"); //정보없는ID입니다.
                    bResult = true;
                    return bResult;
                }

                // 2022.11.09 KGS 추가
                if (dtBefore.Rows.Count > 0)
                {
                    string sWorkingType = dtBefore.Rows[0]["OCOP_INSP_CODE"].ToString();
                    if(sWorkingType != Util.NVC(dtResult.Rows[0]["OCOP_INSP_CODE"]))
                    {
                        ms.AlertInfo("SFU8525"); // 같은 처리유형끼리만 작업이 가능합니다.
                        bResult = true;
                        return bResult;
                    }
                }
                else
                {
                    sProcessType = Util.NVC(dtResult.Rows[0]["OCOP_INSP_CODE"]);
                }

                //2020.01.28
                //if (!(txtReworkWOID.Text.Length > 0))
                //{
                //    ms.AlertInfo("SFU1441"); //WORK ORDER ID가 선택되지 않았습니다.
                //    bResult = true;
                //    return bResult;
                //}

                #region 입력된이전값과 비교
                if (dtBefore.Rows.Count > 0)
                {
                    DataRow[] drTemp = dtBefore.Select("LOTID = '" + Util.NVC(dtResult.Rows[0]["LOTID"]) + "'");
                    if (drTemp.Length > 0)
                    {
                        
                        string sCheckLotId = Util.NVC(dtResult.Rows[0]["LOTID"]);                       
                        ms.AlertWarning("SFU1376", sCheckLotId); //LOT ID는 중복 입력할수 없습니다.\r\n({0})
                        bResult = true;
                        return bResult;
                    }
                }
                #endregion

                //if (dtBefore.Rows.Count > 0)
                //{
                //    DataRow[] drWOIDTemp = dtBefore.Select("WOID = '" + Util.NVC(dtResult.Rows[0]["WOID"]) + "'");
                //    if (!(drWOIDTemp.Length > 0))
                //    {
                //        ms.AlertInfo("SFU1440"); //WORK ORDER ID(WOID)가 다른정보가 존재합니다.
                //        bResult = true;
                //        return bResult;
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bResult;
        }

        private void setPackGubun(C1ComboBox cbo)
        {
            DataTable dtCombo = new DataTable();
            dtCombo.TableName = "RQSTDT";
            dtCombo.Columns.Add("CBO_CODE", typeof(string));
            dtCombo.Columns.Add("CBO_NAME", typeof(string));

            DataRow dr = dtCombo.NewRow();
            dr["CBO_CODE"] = "OWMS";
            dr["CBO_NAME"] = "OWMS";
            dtCombo.Rows.Add(dr);

            
            if (LoginInfo.CFG_SHOP_ID.ToString() != "G481")
            {
                DataRow dr1 = dtCombo.NewRow();
                dr1["CBO_CODE"] = "PACK";
                dr1["CBO_NAME"] = "PACK";
                dtCombo.Rows.Add(dr1);
            }
            else
            {
                cboPackGubun.IsEnabled = false;
            }
            

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = DataTableConverter.Convert(dtCombo);                        
            cbo.SelectedIndex = 0;
        }

        private void clearInput()
        {
            try
            {
                //txtTagetEQSGNAME.Text = string.Empty;
                //txtTagetEQSGNAME.Tag = null;
                //txtTagetPCSGNAME.Text = string.Empty;
                //txtTagetPCSGNAME.Tag = null;

                txtReworkWOID.Text = string.Empty;
                txtReworkWOID.Tag = string.Empty;

                cboTargetEQSGID.IsEnabled = true;
                btnWorkOroderSearch.IsEnabled = false;
                cboTaget_INPUTPROCID.IsEnabled = false;

                cboTaget_INPUTPROCID.ItemsSource = null;
                cboTaget_INPUTPROCID.SelectedValue = null;

                sProcessType = "";
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void getReturnTagetCell_By_Excel()
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);

                        if (dtExcelData != null)
                        {
                            CheckFile(dtExcelData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CheckFile(DataTable dtExcelData)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                for (int i = 0; i < dtExcelData.Rows.Count; i++)
                {
                    if (dtExcelData.Rows[i][0] != null)
                    {
                        txtLotID.Text = dtExcelData.Rows[i][0].ToString();

                        if (!getTagetLotInfo_multi())
                        {
                            return;
                        }
                        txtLotID.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.Alert(ex.ToString());
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void chkUSEReworkWO()
        {
            DataTable indata = new DataTable();
            indata.Columns.Add("LANGID");
            indata.Columns.Add("CMCDTYPE");
            indata.Columns.Add("CMCODE");
            DataRow dr = indata.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PACK_ERP_REWORK_FLAG";
            dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;
            indata.Rows.Add(dr);
            DataTable outdata = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", indata);

            if(outdata.Rows.Count > 0)
            {
                bUseReworkWO = outdata.Rows[0]["ATTRIBUTE1"].Equals("Y") == true ?  true : false;
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_025_WORKORDERSELECT popup = sender as PACK001_025_WORKORDERSELECT;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtReworkWOID.Text = popup.WOID;
                    cboTargetEQSGID.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private Boolean procIdFixed()
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
                dr["CMCDTYPE"] = "PACK_UI_RETURN_REWORK_PROCID_FIXED";
                dr["CMCODE"] = cboTargetEQSGID.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    if (cboTaget_INPUTPROCID.Items.Count > 0)
                    {
                        for (int i = 0; cboTaget_INPUTPROCID.Items.Count > i; i++)
                        {
                            DataRowView drv = cboTaget_INPUTPROCID.Items[i] as DataRowView;
                            if (drv.Row.ItemArray[1].ToString().Equals(dtResult.Rows[0]["ATTRIBUTE1"].ToString()))
                            {
                                cboTaget_INPUTPROCID.SelectedValue = drv.Row.ItemArray[1].ToString();
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion Method

        
    }
}