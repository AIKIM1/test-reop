/*************************************************************************************
 Created Date : 2019.05.14
      Creator : 오화백
   Decription : MEB 라미모니터링 - Port 정보 조회 및 정보변경
--------------------------------------------------------------------------------------
 [Change History]
 2019.05.14   오화백  :  INIT
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001;


namespace LGC.GMES.MES.MCS001
{

    public partial class MCS001_004_PORT_INFO : C1Window, IWorkArea
    {
        #region Declaration
         private string _PORT = string.Empty;  //RACKID or RACK
        //private string _TYPE = string.Empty; //PORT 인지 RACK인지 구분
        private string _Excute = "N";// Rack변경 LOT수정, 삭제시 체크
        private string _UserID = string.Empty; //직접 실행하는 USerID


        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        BizDataSet _bizRule = new BizDataSet();
        DataTable INPUT_LOT = null;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public MCS001_004_PORT_INFO()
        {
            InitializeComponent();
        }

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
               
                object[] parameters = C1WindowExtension.GetParameters(this);
                _PORT = parameters[0] as string;
                if (_PORT == string.Empty) return;
                InitCombo();
                BindPortPosition();//PORT 정보조회
                BindReturnInfo(); //반송정보
                _load = false;
            }

        }
        #endregion

        #endregion

        #region Event
   
        #region LOT 조회 : txtLotid_KeyDown()
        /// <summary>
        /// LOT 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!Validation())
                {
                    return;
                }
                string sLotid = txtLotid.Text.Trim();
                if (dgList.GetRowCount() > 0)
                {
                    for (int i = 0; i < dgList.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            dgList.SelectedIndex = i;
                            dgList.ScrollIntoView(i, dgList.Columns["CHK"].Index);
                            DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", 1);
                            txtLotid.Focus();
                            txtLotid.Text = string.Empty;
                            return;
                        }
                    }
                }
                InputLot();
            }
        }
        #endregion

        #region 닫기  : btnClose_Click(), C1Window_Closing()
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_Excute == "Y")
            {
                this.DialogResult = MessageBoxResult.OK;
            }
            else
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }

        }
        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_Excute == "Y")
            {
                this.DialogResult = MessageBoxResult.OK;
            }
            else
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }


        }
        #endregion

        #region 추가된 LOT 색깔변경 : dgList_LoadedCellPresenter()
        /// <summary>
        /// 추가된 LOT은 색깔 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if(e.Cell.Column.Name == "")
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SAVE_YN").ToString() == "N")
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFF00"));

                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        #endregion

        #region LOT 삭제 버튼  : btnDeleteLot_Click()
        /// <summary>
        /// LOT 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteLot_Click(object sender, RoutedEventArgs e)
        {
            if (!DeleteValidation())
            {
                return;
            }

            // 삭제하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteLOT();
                }
            });

          
            //LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
            //authConfirm.FrameOperation = FrameOperation;
            //if (authConfirm != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = "LOGIS_MANA";    // 관리권한
            //    C1WindowExtension.SetParameters(authConfirm, Parameters);
            //    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Delete);

            //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            //    {
            //        if (tmp.Name == "grdMain")
            //        {
            //            tmp.Children.Add(authConfirm);
            //            authConfirm.BringToFront();
            //            break;
            //        }
            //    }
            //}
        }
        #endregion

        #region LOT 삭제 인증 팝업 닫기  : OnCloseAuthConfirm_Delete()
        /// <summary>
        /// LOT 삭제 인증 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnCloseAuthConfirm_Delete(object sender, EventArgs e)
        //{
        //    LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {
        //        _UserID = window.UserID;
        //        DeleteLOT();
        //    }
                

        //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
        //    {
        //        if (tmp.Name == "grdMain")
        //        {
        //            tmp.Children.Remove(window);
        //            break;
        //        }
        //    }
        //}
        #endregion

        #region LOT 변경 : btnLotInfoChange_Click()
        /// <summary>
        /// LOT 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLotInfoChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ModifyValidation())
            {
                return;
            }
            // 수정하시겠습니까?
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyLOT();
                }
            });
          
            //LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
            //authConfirm.FrameOperation = FrameOperation;
            //if (authConfirm != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = "LOGIS_MANA";    // 관리권한
            //    C1WindowExtension.SetParameters(authConfirm, Parameters);
            //    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Modify);

            //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            //    {
            //        if (tmp.Name == "grdMain")
            //        {
            //            tmp.Children.Add(authConfirm);
            //            authConfirm.BringToFront();
            //            break;
            //        }
            //    }
            //}

        }
        #endregion

        #region LOT 변경 인증 팝업 닫기 : OnCloseAuthConfirm_Modify()

        //private void OnCloseAuthConfirm_Modify(object sender, EventArgs e)
        //{
        //    LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {
        //        ModifyLOT();
        //        _UserID = window.UserID;
        //    }


        //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
        //    {
        //        if (tmp.Name == "grdMain")
        //        {
        //            tmp.Children.Remove(window);
        //            break;
        //        }
        //    }
        //}
        #endregion

        #region 특별관리 체크 : CheckBox_Checked()
        /// <summary>
        /// 특별관리 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL.Visibility = Visibility.Visible;
        }

        #endregion

        #region 특별관리 해제 : CheckBox_Unchecked()
        /// <summary>
        /// 특별관리 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 특별관리 실행 : ApplyButton_Click()
        /// <summary>
        /// 특별관리 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string sSPCLCode = this.cboSPCL.SelectedValue.ToString();
            string sRemarks = this.tbSPCLREMARKS.Text.Trim();
            //string sRackId = rack.RackId;

            if (sSPCLCode == "SELECT")
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4542"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);   //메세지 작성요
                //this.cboSPCL.SelectAll();
                this.cboSPCL.Focus();
                return;
            }
            Util.MessageConfirm("SFU4545", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));
                    RQSTDT.Columns.Add("SPCL_RSNCODE", typeof(string));
                    RQSTDT.Columns.Add("WIP_REMARKS", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["RACK_ID"] = _PORT;
                    dr["SPCL_RSNCODE"] = sSPCLCode;
                    dr["WIP_REMARKS"] = sRemarks;
                    RQSTDT.Rows.Add(dr);

                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_SPCL", "INDATA", null, RQSTDT);

                   // BindLotInfo();

                    this.cboSPCL.SelectedIndex = 0;
                    this.tbSPCLREMARKS.Text = "";
                    this.ChkSPCL.IsChecked = false;
                    this.StackSPCL.Visibility = Visibility.Collapsed;
                    Util.MessageInfo("SFU1275");
                    _Excute = "Y";
                }
            });
        }
        #endregion

        #region 특별관리해제 체크 : ChkSPCL_UNDO_Checked()
        /// <summary>
        /// 특별관리해제 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkSPCL_UNDO_Checked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL_UNDO.Visibility = Visibility.Visible;
        }
        #endregion

        #region 특별관리해제 체크 해제 : ChkSPCL_UNDO_Unchecked()
        /// <summary>
        /// 특별관리해제 체크 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkSPCL_UNDO_Unchecked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL_UNDO.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 특별관리해제 실행 : ApplyButton_UNFO_Click()
        /// <summary>
        /// 특별관리해제 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_UNFO_Click(object sender, RoutedEventArgs e)
        {
            string sSPCLCode = this.cboSPCL.SelectedValue.ToString();
            Util.MessageConfirm("SFU5092", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));


                    DataRow dr = RQSTDT.NewRow();
                    dr["RACK_ID"] = _PORT;

                    RQSTDT.Rows.Add(dr);

                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_SPCL_CLEAR", "INDATA", null, RQSTDT);
                   // BindLotInfo();

                    this.ChkSPCL_UNDO.IsChecked = false;
                    this.StackSPCL_UNDO.Visibility = Visibility.Collapsed;
                    Util.MessageInfo("SFU1275");
                    _Excute = "Y";
                }
            });
        }
        #endregion


        #endregion

        #region User Method

        #region 추가 LOT Bind : InputLot()
        /// <summary>
        /// 조회된 LOT 추가
        /// </summary>
        private void InputLot()
        {

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LOTID"] = txtLotid.Text;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);
            INPUT_LOT = new DataTable();
            INPUT_LOT = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_LAMI_CHG_LOT", "INDATA", "OUTDATA", dtRqst);

            if (INPUT_LOT.Rows.Count > 0)
            {
                INPUT_LOT.Rows[0]["CHK"] = 1;
                INPUT_LOT.Rows[0]["SAVE_YN"] = "N";
                if (dgList.Rows.Count == 0)
                {
                    Util.GridSetData(dgList, INPUT_LOT, FrameOperation);
                }
                else
                {
                    DataTable dtSource = DataTableConverter.Convert(dgList.ItemsSource);
                    DataRow newRow = null;
                    newRow = dtSource.NewRow();
                    newRow["CHK"] = 1;
                    newRow["LOTID"] = INPUT_LOT.Rows[0]["LOTID"].ToString();
                    newRow["PRJT_NAME"] = INPUT_LOT.Rows[0]["PRJT_NAME"].ToString();
                    newRow["PRODID"] = INPUT_LOT.Rows[0]["PRODID"].ToString();
                    newRow["PRODNAME"] = INPUT_LOT.Rows[0]["PRODNAME"].ToString();
                    newRow["MODLID"] = INPUT_LOT.Rows[0]["MODLID"].ToString();
                    newRow["WIP_QTY"] = INPUT_LOT.Rows[0]["WIP_QTY"].ToString();
                    newRow["JUDG_VALUE"] = INPUT_LOT.Rows[0]["JUDG_VALUE"].ToString();
                    newRow["VLD_DATE"] = INPUT_LOT.Rows[0]["VLD_DATE"].ToString();
                    newRow["SPCL_FLAG"] = INPUT_LOT.Rows[0]["SPCL_FLAG"].ToString();
                    newRow["SPCL_RSNCODE"] = INPUT_LOT.Rows[0]["SPCL_RSNCODE"].ToString();
                    newRow["WIP_REMARKS"] = INPUT_LOT.Rows[0]["WIP_REMARKS"].ToString();
                    newRow["WIPHOLD"] = INPUT_LOT.Rows[0]["WIPHOLD"].ToString();
                    newRow["SAVE_YN"] = INPUT_LOT.Rows[0]["SAVE_YN"].ToString();
                    newRow["EQSGID"] = INPUT_LOT.Rows[0]["EQSGID"].ToString();
                    newRow["EQSGNAME"] = INPUT_LOT.Rows[0]["EQSGNAME"].ToString();
                    dtSource.Rows.Add(newRow);
                    //for (int i = 0; i < dtSource.Rows.Count; i++)
                    //{
                    //    dtSource.Rows[i]["CHK"] = 0;
                    //}
                    Util.GridSetData(dgList, dtSource, FrameOperation);
                }
                txtLotid.Text = string.Empty;
                txtLotid.Focus();
            }
        }

        #endregion
       
        #region 로드시 Port 위치 정보 Bind : BindPortPosition()
        /// <summary>
        /// Port위치 
        /// </summary>
        private void BindPortPosition()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("PORT_ID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["PORT_ID"] = _PORT;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_PORT_POSITION_INFO", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {
                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgPort, result, FrameOperation, true);
                 
                }
                BindPortLotInfo();
                loadingIndicator.Visibility = Visibility.Collapsed;
            });
        }

        #endregion

     

        #region 로드시 PORT 정보로 LOT 조회 Bind : BindPortLotInfo()
        /// <summary>
        /// PORT LOT 정보 조회 
        /// </summary>
        private void BindPortLotInfo()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("PORTID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["PORTID"] = _PORT;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_MEB_INPUT_LOT", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {
                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                Util.gridClear(dgList);
                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, result, FrameOperation, true);
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            });
        }

        #endregion

        #region 로드시 반송정보 Bind : BindReturnInfo()
        /// <summary>
        /// 반송정보
        /// </summary>
        private void BindReturnInfo()
        {
            loadingIndicator.Visibility = Visibility.Visible;

            string BizName = string.Empty;
            BizName = "DA_MCS_SEL_RETURN_POSITION_INFO_PORT";

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("RACKORPORT", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["RACKORPORT"] = _PORT;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService(BizName, "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {
                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgReturn, result, FrameOperation, true);
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            });
        }

        #endregion

        #region 콤보박스 초기화 : InitCombo()
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            // 특별 SKID  사유 Combo
            String[] sFilter3 = { "SPCL_RSNCODE" };
            _combo.SetCombo(cboSPCL, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");
        }

        #endregion
      

        #region Lot 정보 삭제 : DeleteLOT()

        private void DeleteLOT()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("RACK_ID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("ACT_TYPE", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("EMPTY_REEL_TYPE_CODE", typeof(string));
                inLot.Columns.Add("REEL_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["RACK_ID"] = _PORT;
                newRow["UPDUSER"] = LoginInfo.USERID;//_UserID;
                newRow["ACT_TYPE"] = "D";
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    if (drDel["SAVE_YN"].ToString() == "Y")
                    {
                        newRow = inLot.NewRow();
                        newRow["LOTID"] = drDel["LOTID"].ToString();
                        inLot.Rows.Add(newRow);
                    }
                }
                new ClientProxy().ExecuteService_Multi("BR_MCS_REG_LOT_INFO_ON_RACK", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                         BindReturnInfo(); //반송정보
                        _Excute = "Y";
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);

                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        #endregion

        #region LOT 정보 수정 : ModifyLOT()
        private void ModifyLOT()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("RACK_ID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("ACT_TYPE", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("EMPTY_REEL_TYPE_CODE", typeof(string));
                inLot.Columns.Add("REEL_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["RACK_ID"] = _PORT;
                newRow["UPDUSER"] = LoginInfo.USERID;//_UserID;
                newRow["ACT_TYPE"] = "C";
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    if (drDel["SAVE_YN"].ToString() == "N")
                    {
                        newRow = inLot.NewRow();
                        newRow["LOTID"] = drDel["LOTID"].ToString();
                        inLot.Rows.Add(newRow);
                    }
                }
                new ClientProxy().ExecuteService_Multi("BR_MCS_REG_LOT_INFO_ON_RACK", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                         BindReturnInfo(); //반송정보
                        _Excute = "Y";
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        #endregion

        #region Validation  : DeleteValidation(), Validation(), ModifyValidation(), ModifyRackValidation()

        // <summary>
        /// 삭제Validation
        /// </summary>
        private bool DeleteValidation()
        {
            if(dgList.Rows.Count == 0)
            {
                //Util.Alert("조회된 데이터가 없습니다.");
                Util.MessageValidation("SFU3537");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }
        // <summary>
        /// Validation
        /// </summary>
        private bool Validation()
        {

            if (dgList.GetRowCount() == 2)
            {
                //하나의 RACK에는 2개의 LOT만 입력가능합니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU5071"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtLotid.Focus();
                        txtLotid.Text = string.Empty;
                    }
                });

                return false;
            }


            if (txtLotid.Text == string.Empty)
            {

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1366"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtLotid.Focus();
                        txtLotid.Text = string.Empty;
                    }
                });

                return false;
            }
            return true;
        }

        // <summary>
        /// 수정Validation
        /// </summary>
        private bool ModifyValidation()
        {
            if (dgList.Rows.Count == 0)
            {
                //Util.Alert("조회된 데이터가 없습니다.");
                Util.MessageValidation("SFU3537");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }


     


        #endregion

        #region Func : ShowLoadingIndicator(), HiddenLoadingIndicator()

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











        #endregion

        #endregion

      
    }
}
