/*************************************************************************************
 Created Date : 2017.12.22
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - C 생산 공정진척 화면 - 대기LOT 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.22  :        Initial Created.
   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_021_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_021_WAITLOT : C1Window, IWorkArea
    {        
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY003_021_WAITLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event       
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _LineID = Util.NVC(tmps[0]);
            }
            else
            {
                _LineID = "";
            }
            ApplyPermissions();

            // HOLD 사유 코드
            CommonCombo _combo = new CommonCombo();

            String[] sFilter1 = { "CPROD_WRK_TYPE_CODE" };
            C1ComboBox[] cboProcessChild = { cboEquipmentSegment };
            _combo.SetCombo(cboCPROD_WRK_TYPE_CODE, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, sCase: "COMMCODE", sFilter: sFilter1);

            //라인 Combo
            String[] sFilter2 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineParent2 = { cboCPROD_WRK_TYPE_CODE };
            C1ComboBox[] cboLineChild2 = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent2, cbChild: cboLineChild2, sCase: "FROM_EQUIPMENTSEGMENT_CPROD", sFilter: sFilter2);

            //설비 Combo
            C1ComboBox[] cboEquipmentParent2 = { cboCPROD_WRK_TYPE_CODE, cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent2, sCase: "FROM_EQUIPMENT_CPROD");
            


            //HOLD 사유
            //string[] sFilter = { "HOLD_LOT" };
            //_combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            //string[] sFilter2 = { "BICELL_TYPE_FD" };
            //_combo.SetCombo(cboCellType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODE");

            //string[] sFilter3 = { LoginInfo.CFG_AREA_ID, Process.STACKING_FOLDING, EquipmentGroup.STACKING, _LineID };
            //_combo.SetCombo(cboOtherEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "EQUIPMENTSEGMENT_WITHOUT_SEL_EQSGID");


            cboCPROD_WRK_TYPE_CODE.SelectedValueChanged += cboCPROD_WRK_TYPE_CODE_SelectedValueChanged;
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtWaitLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWait_List();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWait_List();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWait_List()
        {
            try
            {
                Util.gridClear(dgWaitLot);

                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("CPROD_RWK_LOT_EQSGID", typeof(String));
                RQSTDT.Columns.Add("CPROD_RWK_LOT_EQPTID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String)); 

                DataRow newRow = RQSTDT.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.CPROD;
                newRow["LOTID"] = txtWaitLot.Text.Trim();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CPROD_WRK_TYPE_CODE"] = Util.GetCondition(cboCPROD_WRK_TYPE_CODE, bAllNull: true);
                newRow["CPROD_RWK_LOT_EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                newRow["CPROD_RWK_LOT_EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                newRow["EQSGID"] = Util.NVC(_LineID);

                RQSTDT.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_WAIT_LOT_LIST", "INDATA", "OUTDATA", RQSTDT, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(dgWaitLot, bizResult, null, true);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow newRow = RQSTDT.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_REWRK_NJ", "INDATA", "OUTDATA", RQSTDT);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnHoldRelease);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void printWaitMaz_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #region [Validation]

        #endregion

        #endregion

        private void btnWaitMagazinePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                string sBoxID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
                decimal dQty = 0;
                decimal.TryParse(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPQTY")), out dQty);
                string sEqsgName = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "CPROD_RWK_LOT_EQSGNAME"));
                string sEqptName = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "CPROD_RWK_LOT_EQPTNAME"));

                if (!sBoxID.Equals(""))
                {
                    CProdSendPrint(sBoxID, dQty, sEqsgName, sEqptName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CProdSendPrint(string sLotid, decimal dQty, string sLineName, string sEqptName)
        {
            try
            {
                // 발행..
                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                dicParam.Add("LOTID", sLotid);
                //dicParam.Add("QTY", Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgHistList.Rows[i].DataItem, "SEND_QTY"))).ToString());
                dicParam.Add("QTY", dQty.ToString());
                dicParam.Add("EQSGNAME", sLineName);
                dicParam.Add("EQPTNAME", sEqptName);

                dicParam.Add("PRINTQTY", "1");  // 발행 수                
                dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                dicList.Add(dicParam);


                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT();
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = dicList;
                    Parameters[1] = Process.STACKING_FOLDING;
                    Parameters[2] = _LineID;
                    Parameters[3] = _EqptID;
                    Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                    Parameters[5] = "CREATE_CPRODUCT";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.ShowModal();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT;

            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void cboCPROD_WRK_TYPE_CODE_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWait_List();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWait_List();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWait_List();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanDelete())
                    return;
                Util.MessageConfirm("SFU4509", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Term();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanDelete()
        {
            bool bRet = false;

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgWaitLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (txtRemark.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1594");
                return bRet;
            }

            if (Util.NVC(txtUserNameDel.Tag).Equals(""))
            {
                Util.MessageValidation("SFU1842");
                return bRet;
            }
            
            bRet = true;

            return bRet;
        }

        private void Term()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName = "BR_PRD_REG_STOCK_INV_TERM_LOT";

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));

                DataRow row = null;

                row = inTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["USERID"] = LoginInfo.USERID;
                row["REQ_USERID"] = Util.NVC(txtUserNameDel.Tag);
                row["WIPNOTE"] = Util.NVC(txtRemark.Text);

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("LOTSTAT", typeof(string));

                row = null;


                for (int i = 0; i < dgWaitLot.Rows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgWaitLot, "CHK", i)) continue;

                    row = inLot.NewRow();

                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "LOTID"));
                    row["LOTSTAT"] = "EMPTIED";

                    inLot.Rows.Add(row);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                GetWait_List();

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

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserNameDel.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserNameDel.Text = wndPerson.USERNAME;
                txtUserNameDel.Tag = wndPerson.USERID;
            }
        }
    }
}
