/*************************************************************************************
 Created Date : 2018.11.16
      Creator : 오화백
   Decription : 라미모니터링 - Rack/Port 정보 조회 및 정보변경
--------------------------------------------------------------------------------------
 [Change History]
 2019.02.20   오화백  :  LOT삭제/수정에서 인증로직 주석
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

    public partial class MCS001_006_LOT_INFO : C1Window, IWorkArea
    {
        #region Declaration
        private string _PORTORRACK = string.Empty;  //RACKID or RACK
        private string _TYPE = string.Empty; //PORT 인지 RACK인지 구분
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

        public MCS001_006_LOT_INFO()
        {
            InitializeComponent();
        }

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
               
                object[] parameters = C1WindowExtension.GetParameters(this);
                _PORTORRACK = parameters[0] as string;
                _TYPE = parameters[1] as string;
                if (_PORTORRACK == string.Empty) return;
                InitCombo();
                LocationCheck();  //Rack일 경우와 Port일경우가 틀림
                BindReturnInfo(); //반송정보
                if(_TYPE == "PORT")
                {
                    this.Header = ObjectDic.Instance.GetObjectName("PORT정보");
                    ChkSPCL.Visibility = Visibility.Collapsed;
                    ChkSPCL_UNDO.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.Header = ObjectDic.Instance.GetObjectName("RACK정보");

                }

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

        #region RACK상태변경 : btnRackChange_Click()
        /// <summary>
        /// RACK상태변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRackChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ModifyRackValidation())
            {
                return;
            }
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
            authConfirm.FrameOperation = FrameOperation;
            if (authConfirm != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = "LOGIS_MANA";    // 관리권한
                C1WindowExtension.SetParameters(authConfirm, Parameters);
                authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(authConfirm);
                        authConfirm.BringToFront();
                        break;
                    }
                }
            }
        }

        #endregion

        #region RACK상태변경 인증 팝업 닫기 : OnCloseAuthConfirm()
        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _UserID = window.UserID;
                ModifyRackLOT();
            }
               

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
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

        #region RACK인지 PORT인지 체크 후 팝업 화면셋팅 : LocationCheck()
        /// <summary>
        /// RACK인지 PORT인지 체크 
        /// </summary>
        private void LocationCheck()
        {
            if (_TYPE == "PORT")
            {
                txtRackState.Visibility = Visibility.Collapsed;
                cboRackState.Visibility = Visibility.Collapsed;
                txtRack.Visibility = Visibility.Collapsed;
                dgRack.Visibility = Visibility.Collapsed;
                btnRackChange.Visibility = Visibility.Collapsed;
                //txtLotid.Visibility = Visibility.Collapsed;
                //btnDeleteLot.Visibility = Visibility.Collapsed;
                //btnLotInfoChange.Visibility = Visibility.Collapsed;
                //txtBlockLot.Visibility = Visibility.Collapsed;

                txtPort.Visibility = Visibility.Visible;
                dgPort.Visibility = Visibility.Visible;

                BindPortPosition();
              

            }
            else
            {
                txtRackState.Visibility = Visibility.Visible;
                cboRackState.Visibility = Visibility.Visible;
                txtRack.Visibility = Visibility.Visible;
                dgRack.Visibility = Visibility.Visible;
                btnRackChange.Visibility = Visibility.Visible;
                //txtLotid.Visibility = Visibility.Visible;
                //btnDeleteLot.Visibility = Visibility.Visible;
                //btnLotInfoChange.Visibility = Visibility.Visible;
                //txtBlockLot.Visibility = Visibility.Collapsed;

                txtPort.Visibility = Visibility.Collapsed;
                dgPort.Visibility = Visibility.Collapsed;

                BindRackPosition();
             
            }
        }

        #endregion

        #region 로드시 Rack 위치 정보 Bind : BindRackPosition()
        /// <summary>
        /// Rack위치 
        /// </summary>
        private void BindRackPosition()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("RACK_ID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["RACK_ID"] = _PORTORRACK;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_RACK_POSITION_INFO", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {
                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgRack, result, FrameOperation, true);
                }
                BindLotInfo();
                loadingIndicator.Visibility = Visibility.Collapsed;
            });
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
            dr["PORT_ID"] = _PORTORRACK;
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

        #region 로드시 Rack 정보로 LOT 정보 Bind : BindLotInfo()
        /// <summary>
        /// LOT 정보 조회 
        /// </summary>
        private void BindLotInfo()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("RACKID", typeof(string));
            dtRqst.Columns.Add("PORTID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["RACKID"] = _PORTORRACK;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_INPUT_LOT", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
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

                    if(result.Rows[0]["RACK_STAT_CODE"].ToString() == "CHECK" || result.Rows[0]["RACK_STAT_CODE"].ToString() == "UNUSE")
                    {
                        ChkSPCL.Visibility = Visibility.Collapsed;
                        ChkSPCL_UNDO.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //특별해제 여부 체크
                        if(result.Rows[0]["SPCL_FLAG"].ToString() == "Y")
                        {
                            ChkSPCL.Visibility = Visibility.Collapsed;
                            ChkSPCL_UNDO.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ChkSPCL.Visibility = Visibility.Visible;
                            ChkSPCL_UNDO.Visibility = Visibility.Collapsed;
                        }
                        
                    }

                }
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
            dr["PORTID"] = _PORTORRACK;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_INPUT_PORT", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
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
            if (_TYPE == "PORT")
            {
                BizName = "DA_MCS_SEL_RETURN_POSITION_INFO_PORT";
            }
            else
            {
                BizName = "DA_MCS_SEL_RETURN_POSITION_INFO_RACK";
            }

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("RACKORPORT", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["RACKORPORT"] = _PORTORRACK;
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
            //Rack 정보 상태 변경
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MCS_RACK_STAT_CODE";
            dr["ATTRIBUTE1"] = "Y";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboRackState.DisplayMemberPath = "CBO_NAME";
            cboRackState.SelectedValuePath = "CBO_CODE";

            DataRow dataRow = dtResult.NewRow();
            dataRow["CBO_CODE"] = string.Empty;
            dataRow["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(dataRow, 0);

            cboRackState.ItemsSource = dtResult.Copy().AsDataView();
            cboRackState.SelectedIndex = 0;
            CommonCombo _combo = new CommonCombo();
            // 특별 SKID  사유 Combo
            String[] sFilter3 = { "SPCL_RSNCODE" };
            _combo.SetCombo(cboSPCL, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");
        }

        #endregion

        #region Rack 정보 변경 : ModifyRackLOT()
        private void ModifyRackLOT()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("RACK_ID", typeof(string));
                inTable.Columns.Add("RACK_STAT_CODE", typeof(string));
                inTable.Columns.Add("EMPTY_REEL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("REEL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["RACK_ID"] = _PORTORRACK;
                newRow["RACK_STAT_CODE"] = cboRackState.SelectedValue.ToString();
                newRow["UPDUSER"] = _UserID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_MCS_REG_RACK_INFO_CHANGE", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        LocationCheck();  //Rack일 경우와 Port일경우가 틀림
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
                newRow["RACK_ID"] = _PORTORRACK;
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
                        LocationCheck();  //Rack일 경우와 Port일경우가 틀림
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
                newRow["RACK_ID"] = _PORTORRACK;
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
                        LocationCheck();  //Rack일 경우와 Port일경우가 틀림
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


        // <summary>
        /// Rack 수정 Validation
        /// </summary>
        private bool ModifyRackValidation()
        {
            if (cboRackState.SelectedIndex == 0)
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("상태코드"));
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

        /// <summary>
        /// 특별관리 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 특별관리 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL.Visibility = Visibility.Collapsed;
        }
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
                    dr["RACK_ID"] = _PORTORRACK;
                    dr["SPCL_RSNCODE"] = sSPCLCode;
                    dr["WIP_REMARKS"] = sRemarks;
                    RQSTDT.Rows.Add(dr);

                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_SPCL", "INDATA", null, RQSTDT);

                    BindLotInfo();

                    this.cboSPCL.SelectedIndex = 0;
                    this.tbSPCLREMARKS.Text = "";
                    this.ChkSPCL.IsChecked = false;
                    this.StackSPCL.Visibility = Visibility.Collapsed;
                    Util.MessageInfo("SFU1275");
                    _Excute = "Y";
                }
            });
        }
        /// <summary>
        /// 특별관리해제 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkSPCL_UNDO_Checked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL_UNDO.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 특별관리해제 체크 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkSPCL_UNDO_Unchecked(object sender, RoutedEventArgs e)
        {
            this.StackSPCL_UNDO.Visibility = Visibility.Collapsed;
        }
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
                    dr["RACK_ID"] = _PORTORRACK;
                 
                    RQSTDT.Rows.Add(dr);

                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_SPCL_CLEAR", "INDATA", null, RQSTDT);
                    BindLotInfo();
                
                    this.ChkSPCL_UNDO.IsChecked = false;
                    this.StackSPCL_UNDO.Visibility = Visibility.Collapsed;
                    Util.MessageInfo("SFU1275");
                    _Excute = "Y";
                }
            });
        }
    }
}
