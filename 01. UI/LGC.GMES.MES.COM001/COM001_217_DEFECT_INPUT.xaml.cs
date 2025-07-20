/*************************************************************************************
 Created Date : 2018.02.28
      Creator : 오화백
   Decription : Pouch 활성화 불량창고 관리 - 대차 불량창고 입고
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_217_DEFECT_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _eqptID = string.Empty;        // 설비코드

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();


        private bool _load = true;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_217_DEFECT_INPUT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetCombo();
                txtUserNameCr.Text = LoginInfo.USERNAME;
                txtUserNameCr.Tag = LoginInfo.USERID;
                _load = false;
            }

        }
        //대차ID
        private void txtCtnrID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtCtnrID.Text == string.Empty) return;
                    //Cart 정보
                    SetGridCartList(txtCtnrID.Text);
                    if (txtCtnrID.Text != string.Empty)
                    {
                        //조립LOT 정보 조회
                        SetGridAssyLotList(txtCtnrID.Text);
                        //불량LOT 정보 조회
                        SetGridDefectList(txtCtnrID.Text);
                        //기본정보 셋팅
                        SetControl();
                    }
                    txtCtnrID.Text = string.Empty;
                    txtCtnrID.Focus();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


        }
        private void InitializeUserControls()
        {
        }
        private void SetControl()
        {
            //공정
            txtProcess.Text = LoginInfo.CFG_PROC_NAME;
            txtProcess.Tag = LoginInfo.CFG_PROC_ID;
            //라인
            txtEqsgid.Text = DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "EQSGNAME").ToString();
            txtEqsgid.Tag = DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "EQSGID").ToString();
            //불량창고
            txtWhid.Text = DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "WH_NAME").ToString();
            txtWhid.Tag = DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "WH_ID").ToString();
        }
        private void SetCombo()
        {
            CommonCombo _combo = new CommonCombo();
            //String[] sFilter2 = { "", "WIP_PRCS_TYPE_CODE" };
            //_combo.SetCombo(cboWIP_QLTY_TYPE_CODE, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODES");


            String[] sFilter2 = { "WIP_PRCS_TYPE_CODE", "WH" };
            _combo.SetCombo(cboWIP_QLTY_TYPE_CODE, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODEATTR");

        }
        //창고 입고
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Input_Validation())
                {
                    return;
                }


                //입고처리 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4589"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Defect_Input();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //작업자 조회
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        //작업자 버튼 클릭
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        //Spread 색변경
        private void dgCtnr_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }));
        }

        private void cboWIP_QLTY_TYPE_CODE_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dgCtnr.Rows.Count == 0) return;
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("WH_ID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID").ToString();
                dr["WIP_PRCS_TYPE_CODE"] = cboWIP_QLTY_TYPE_CODE.SelectedValue.ToString();
                dr["WH_ID"] = txtWhid.Tag;
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_DFEC_WH_INBOX", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.gridClear(dgDefectLotGoup);
                    Util.GridSetData(dgDefectLotGoup, dtRslt, FrameOperation, true);

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        #endregion

        #region Mehod

        /// <summary>
        /// 대차 정보 조회
        /// </summary>
        private void SetGridCartList(string CtnrId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = CtnrId;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_DEFC_WH", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if(dtRslt.Rows[0]["WIP_QLTY_TYPE_CODE"].ToString() == "Y")
                    {
                         
                        Util.MessageValidation("SFU4626"); //"양품대차를 입고할 수 없습니다."


                        return;
                    }

                    Util.gridClear(dgCtnr);
                    Util.GridSetData(dgCtnr, dtRslt, FrameOperation, false);
                }
                else
                {
                 
                    Util.MessageValidation("SFU1905");  //"조회된 Data가 없습니다."                  
                    txtCtnrID.Text = string.Empty;
                    return;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 조립LOT List
        /// </summary>
        private void SetGridAssyLotList(string CtnrId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = CtnrId;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_LOTID_RT_DEFC_WH", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.gridClear(dgAssyLot);
                    Util.GridSetData(dgAssyLot, dtRslt, FrameOperation, false);

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        // <summary>
        /// 불량 LOT List
        /// </summary>
        private void SetGridDefectList(string CtnrId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = CtnrId;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_DFEC_WH_INBOX", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.gridClear(dgDefectLotGoup);
                    Util.GridSetData(dgDefectLotGoup, dtRslt, FrameOperation, true);

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        //작업자
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            wndPerson.Width = 600;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtUserNameCr.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        grdMain.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }


            }
        }
        //작업자 팝업닫기
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;

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

        //입고처리
        private void Defect_Input()
        {
            try
            {
                DataSet inData = new DataSet();

                //INDATA
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("CTNR_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("RCPT_USERID", typeof(string));
                inDataTable.Columns.Add("MGNT_TYPE", typeof(string));
                inDataTable.Columns.Add("WH_ID", typeof(string));
                inDataTable.Columns.Add("WIP_PRCS_TYPE_CODE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["PROCID"] = LoginInfo.CFG_PROC_ID;
                row["CTNR_ID"] = DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID").ToString();
                row["USERID"] = LoginInfo.USERID;
                row["RCPT_USERID"] = txtUserNameCr.Tag.ToString();
                row["MGNT_TYPE"] = "Y";
                row["WH_ID"] = txtWhid.Tag.ToString(); // cboWhID.SelectedValue.ToString();
                row["WIP_PRCS_TYPE_CODE"] = cboWIP_QLTY_TYPE_CODE.SelectedValue.ToString(); ; // cboWhID.SelectedValue.ToString();
                inDataTable.Rows.Add(row);

                //INLOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("DFCT_GR_LOTID", typeof(string));
                inLot.Columns.Add("ASSY_LOTID", typeof(string));
                inLot.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                inLot.Columns.Add("DFCT_RSN_GR_CODE", typeof(string));
                inLot.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inLot.Columns.Add("ACTQTY", typeof(decimal));
                inLot.Columns.Add("DFCT_STOCK_LOTID", typeof(string));

                DataTable dtDefectInbox = DataTableConverter.Convert(dgDefectLotGoup.ItemsSource);


                for (int i = 0; i < dtDefectInbox.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["DFCT_GR_LOTID"] = Convert.ToString(dtDefectInbox.Rows[i]["INBOX_ID_DEF"]); //불량그룹LOT
                    row["ASSY_LOTID"] = Convert.ToString(dtDefectInbox.Rows[i]["LOTID_RT"]); //조립LOT
                    row["DFCT_RSN_GR_ID"] = Convert.ToString(dtDefectInbox.Rows[i]["DFCT_RSN_GR_ID"]); //불량그룹코드
                    row["DFCT_RSN_GR_CODE"] = Convert.ToString(dtDefectInbox.Rows[i]["RESNGR_ABBR_CODE"]); //불량코드
                    row["CAPA_GRD_CODE"] = Convert.ToString(dtDefectInbox.Rows[i]["CAPA_GRD_CODE"]); //등급
                    row["ACTQTY"] = Convert.ToDecimal(dtDefectInbox.Rows[i]["WIPQTY"]); //Cell수량
                    //if (Convert.ToString(dtDefectInbox.Rows[i]["WH_DFEC_LOT"]) != "NEW")
                    //{
                    //    row["DFCT_STOCK_LOTID"] = Convert.ToString(dtDefectInbox.Rows[i]["WH_DFEC_LOT"]); //불량창고 불량LOT
                    //}

                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 등록 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_IN_CTNR", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_STOCK_IN_CTNR", ex.Message, ex.ToString());
            }
        }

        //입고 Valldation
        private bool Input_Validation()
        {

            if (dgDefectLotGoup.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4590"); //입고할 대차 정보가 없습니다.
                return false;
            }

            if (txtUserNameCr.Text == string.Empty || txtUserNameCr.Tag == null)
            {
                Util.MessageValidation("SFU4591"); //작업자를 입력하세요
                return false;
            }
            if (txtWhid.Text == string.Empty && txtWhid.Tag == null)
            {
                Util.MessageValidation("SFU4604"); //창고정보가 없습니다.
                return false;
            }
            if (cboWIP_QLTY_TYPE_CODE.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4606"); //불량재공구분을 선택하세요
                return false;
            }
            return true;
        }




        #endregion

        
    }
    }