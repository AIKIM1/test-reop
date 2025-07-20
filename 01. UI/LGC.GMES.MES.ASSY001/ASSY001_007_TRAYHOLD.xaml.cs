/*************************************************************************************
 Created Date : 2019.09.19
      Creator : 최상민
   Decription : 자동차 조립 PKG 공정 Tray Hold 기능
                -. Tray Hold 처리시 포장공정 파렛트 구성 Hold 처리
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.ASSY001
{
  
    public partial class ASSY001_007_TRAYHOLD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _TrayID = string.Empty;
        private string _TrayQty = string.Empty;
        private string _OutLotID = string.Empty;
        private string _ProdID = string.Empty;
        private string _holdTrgtCode = string.Empty;
        private string _holdUnHoldGubun = string.Empty;

        BizDataSet _Biz = new BizDataSet();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_007_TRAYHOLD()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();         
            ApplyPermissions();
            SetBasicInfo();
            GetTrayCellList();
        }
        
        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 8)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _TrayID = Util.NVC(tmps[4]);
                _TrayQty = Util.NVC(tmps[5]);
                _OutLotID = Util.NVC(tmps[6]);
                _ProdID = Util.NVC(tmps[7]);
                _holdTrgtCode = Util.NVC(tmps[8]);
                _holdUnHoldGubun = Util.NVC(tmps[9]);
            }
                       
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetBasicInfo()
        {
            txtLotId.Text = _LotID;
            txtTrayId.Text = _TrayID;
            txtCellCnt.Text = _TrayQty;
            txtProdid.Text = _ProdID;
        }        

        public void GetTrayCellList()
        {
            try
            {                
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PROD_LOTID");
                RQSTDT.Columns.Add("OUT_LOTID");
                RQSTDT.Columns.Add("EQPTID");
                RQSTDT.Columns.Add("TRAYID");
                

                DataRow newRow = RQSTDT.NewRow();
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["EQPTID"] = _EqptID;
                newRow["TRAYID"] = _TrayID;

                RQSTDT.Rows.Add(newRow);

                try
                {
                    //return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_LIST", "INDATA", "OUTDATA", inTable);
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_HOLD_LIST", "INDATA", "OUTDATA", RQSTDT);

                    if (dtResult != null && dtResult.Rows.Count > 0)
                        txtHoldCnt.Text = Util.NVC(dtResult.Rows[0]["HOLD_COUNT"]);

                    Util.GridSetData(dgHold, dtResult, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageException(ex);
                    return;                }

               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        

        private void dgHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
        /*    try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            */
        }

        #region 저장/닫기 버튼 이벤트

        /// <summary>
        /// HOLD 등록
        /// BIZ : BR_PRD_REG_ASSY_HOLD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// btnHoldRelease_Click
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
            if (dtInfo.Rows.Count < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }
            
            if (txtHoldCnt.Text != "0")
            {
                //SFU1340		HOLD 된 LOT ID 입니다.
                Util.MessageValidation("SFU1340");
                return;
            }

            if (_holdTrgtCode == "TRAY" && dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("CELLID"))
                                        )).ToList().Count > 0)
            {
                //SFU1209		Cell 정보가 없습니다.	
                Util.MessageValidation("SFU1209");
                return;
            }

            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                // SFU1740  오늘 이후 날짜만 지정 가능합니다.
                Util.MessageValidation("SFU1740");
            }

            if (string.IsNullOrEmpty(txtUser.Text))
            {
                //SFU4350 해제 예정 담당자를 선택하세요.
                Util.MessageValidation("SFU4350");
                return;
            }

            if (string.IsNullOrEmpty(txtNote.Text))
            {
                //SFU4300 Hold 사유를 입력하세요.
                Util.MessageValidation("SFU4300");
                return;
            }

            //SFU1345	HOLD 하시겠습니까?
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void btnHoldRelease_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
            if (dtInfo.Rows.Count < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }

            if (txtHoldCnt.Text == "0")
            {
                //SFU1343		Hold 상태가 아닙니다.
                Util.MessageValidation("SFU1343");
                return;
            }

            if (_holdTrgtCode == "TRAY" && dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("CELLID"))
                                        )).ToList().Count > 0)
            {
                //SFU1209		Cell 정보가 없습니다.	
                Util.MessageValidation("SFU1209");
                return;
            }           

            if (string.IsNullOrEmpty(txtNote.Text))
            {
                //SFU4301		Hold 해제 사유를 입력하세요.	
                Util.MessageValidation("SFU4301");
                return;
            }

            //SFU4046	HOLD 해제 하시겠습니까?
            Util.MessageConfirm("SFU4046", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Release();
                }
            });
        }

        private void Release()
        {
            try
            {
              

                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
              

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("UNHOLD_NOTE");
                inDataTable.Columns.Add("SHOPID");


                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("HOLD_ID");
                inHoldTable.Columns.Add("SUBLOTID");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_NOTE"] = txtNote.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataTable.Rows.Add(newRow);
                newRow = null;
                
                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {
                    newRow = inHoldTable.NewRow();
                    newRow["HOLD_ID"] = dtInfo.Rows[row]["HOLD_ID"];
                    inHoldTable.Rows.Add(newRow);
                }                

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ASSY_UNHOLD", "INDATA,INHOLD", null, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region Biz

        /// <summary>
        /// Hold 등록 
        /// </summary>
        private void Save()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("UNHOLD_SCHD_DATE");
                inDataTable.Columns.Add("UNHOLD_CHARGE_USERID");
                inDataTable.Columns.Add("HOLD_NOTE");
                inDataTable.Columns.Add("HOLD_TRGT_CODE");
                inDataTable.Columns.Add("SHOPID");


                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("ASSY_LOTID");
                //inHoldTable.Columns.Add("HOLD_TRGT_CODE");
                //inHoldTable.Columns.Add("MKT_TYPE_CODE");
                inHoldTable.Columns.Add("STRT_SUBLOTID");
                inHoldTable.Columns.Add("END_SUBLOTID");
                inHoldTable.Columns.Add("HOLD_REG_QTY");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_SCHD_DATE"] = dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["UNHOLD_CHARGE_USERID"] = txtUser.Tag;
                newRow["HOLD_NOTE"] = txtNote.Text;
                newRow["HOLD_TRGT_CODE"] = _holdTrgtCode;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                string sHold_reg_qty = "1";
                
                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {
                    newRow = inHoldTable.NewRow();
                    /*
                    newRow["ASSY_LOTID"] = dtInfo.Rows[row]["ASSY_LOTID"];                        
                    newRow["STRT_SUBLOTID"] = dtInfo.Rows[row]["STRT_SUBLOTID"];
                    newRow["END_SUBLOTID"] = dtInfo.Rows[row]["END_SUBLOTID"];
                    newRow["HOLD_REG_QTY"] = dtInfo.Rows[row]["HOLD_REG_QTY"];
                    inHoldTable.Rows.Add(newRow);
                    */
                    newRow["ASSY_LOTID"] = txtTrayId.Text;
                    //newRow["MKT_TYPE_CODE"] = dtInfo.Rows[row]["MKT_TYPE_CODE"];
                    newRow["STRT_SUBLOTID"] = dtInfo.Rows[row]["CELLID"];
                    newRow["END_SUBLOTID"] = "N";
                    newRow["HOLD_REG_QTY"] = sHold_reg_qty;
                    inHoldTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ASSY_HOLD", "INDATA,INHOLD", null, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageValidation("SFU1267");  //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
               
        #endregion

        #region Validation
        private void dgHold_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
         /*
            if (e.Cell.Column.Name == "HOLD_REG_QTY")
            {
                string hold_req_qty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_REG_QTY"].Index).Value);
                int iHold_req_qty;

                if (!string.IsNullOrWhiteSpace(hold_req_qty) && !int.TryParse(hold_req_qty, out iHold_req_qty))
                {
                    //SFU3435	숫자만 입력해주세요
                    Util.MessageInfo("SFU3435");
                }
            }
          */
        }
        #endregion

        #region Method
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);

                //this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));

                //grdMain.Children.Add(wndPerson);
                //wndPerson.BringToFront();

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;
                txtDept.Text = wndPerson.DEPTNAME;
                txtDept.Tag = wndPerson.DEPTID;

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }        

        #endregion

        private void dgHold_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
          
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void dtpSchdDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                //SFU1740  오늘이후날짜만지정가능합니다.
                Util.MessageValidation("SFU1740");
                dtpSchdDate.SelectedDateTime = DateTime.Now;
            }
        }

        private void txtUser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                GetUserWindow();

                e.Handled = true;
            }
        }
    }
}
#endregion