/*************************************************************************************
 Created Date : 2018.3.27
      Creator : 
   Decription : 불량 대차/불량 그룹LOT 등록'
--------------------------------------------------------------------------------------
 [Change History]
 
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
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_231_INPUT_UPDATE : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        private string _LOTID_RT = string.Empty; // 조립LOT
        private DataTable dtBindData = null;
        public string INPUT_UPDATE_CHK { get; set; }  //입력/수정 구분
       
        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        public C1DataGrid DgNonWip { get; set; }


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

        public COM001_231_INPUT_UPDATE()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();
            //동
            C1ComboBox[] cboAreaChild = { cboProcessScrap };
            _combo.SetCombo(cboAreaScrap, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");
            cboAreaScrap.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboProcessParent = { cboAreaScrap };
            C1ComboBox[] cboScrapLine = { cboEquipmentSegmentScrap };
            _combo.SetCombo(cboProcessScrap, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbChild: cboScrapLine, cbParent: cboProcessParent);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaScrap, cboProcessScrap };
            _combo.SetCombo(cboEquipmentSegmentScrap, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboEquipmentSegmentParent);
            cboEquipmentSegmentScrap.SelectedValue = LoginInfo.CFG_EQSG_ID;

        }


        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                // 공정,라인 콤보셋팅
                InitCombo();
                // 비재공 등록일 경우
                if (INPUT_UPDATE_CHK == "INPUT")
                {
                    //비재공 정보 셋팅
                    SetNonWipControl();
                }
                else // 비재공 수정일 경우
                {

                   
                    dtBindData = DataTableConverter.Convert(DgNonWip.ItemsSource);
                    dtBindData.Select("CHK <> 1").ToList<DataRow>().ForEach(row => row.Delete());
                    dtBindData.AcceptChanges();
                   
                    SetGridNonWipList(dtBindData);
                    setNonWipUpdateSetting();
                }
                _load = false;
            }

        }
        
        #endregion

        #region 비재공 등록 

        #region 조립LOT 조회 txtLotRt_KeyDown(), btnLotRtSearch_Click()

        /// <summary>
        ///  조립LOT 조회  텍스트 박스 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotRt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (INPUT_UPDATE_CHK == "INPUT")
                {
                    AssyLot();
                }

            }
        }
        /// <summary>
        /// 조립LOT 버튼 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLotRtSearch_Click(object sender, RoutedEventArgs e)
        {
            AssyLot();
        }
        #endregion

        #region 조립LOT 선택 dgLotRT_MouseLeftButtonUp()
       
        /// <summary>
        /// 조립LOT 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void dgLotRT_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgLotRT.GetCellFromPoint(pnt);

            if (cell != null)
            {

                try
                {
                    DataTableConverter.SetValue(dgNonWip.Rows[0].DataItem, "PRJT_NAME", Util.NVC(DataTableConverter.GetValue(dgLotRT.CurrentRow.DataItem, "PRJT_NAME")));
                    DataTableConverter.SetValue(dgNonWip.Rows[0].DataItem, "PRODID", Util.NVC(DataTableConverter.GetValue(dgLotRT.CurrentRow.DataItem, "PRODID")));
                    DataTableConverter.SetValue(dgNonWip.Rows[0].DataItem, "MKT_TYPE_NAME", Util.NVC(DataTableConverter.GetValue(dgLotRT.CurrentRow.DataItem, "MKT_TYPE_NAME")));
                    DataTableConverter.SetValue(dgNonWip.Rows[0].DataItem, "MKT_TYPE_CODE", Util.NVC(DataTableConverter.GetValue(dgLotRT.CurrentRow.DataItem, "MKT_TYPE_CODE")));
                    _LOTID_RT = Util.NVC(DataTableConverter.GetValue(dgLotRT.CurrentRow.DataItem, "LOTID")).Substring(0,8);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }
        #endregion
        
        #region 비재공 등록 btnInput_Click()
        /// <summary>
        /// 비재공 등록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation())
            {
                return;
            }
            // 등록하시겠습니까?
            Util.MessageConfirm("SFU4615", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputNonwip();
                }
            });
        }

        #endregion


        #endregion

        #region 비재공 수정 
      
        #region 비재공 수정 btnUpdate_Click()
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation_Update())
            {
                return;
            }
            // 수정하시겠습니까?
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    UpdateNonwip();
                }
            });
        }

        #endregion

        #endregion

        #region [공통]
        #region [작업자 조회] txtUser_KeyDown(),btnUser_Click(), wndUser_Closed()
        /// <summary>
        /// 작업자 텍스트박스 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        /// <summary>
        /// 작업자 팝업 열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        /// <summary>
        ///  작업자 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;

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
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;

        }
        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region Method

        #region 비재공 등록 
       
        #region 비재공 정보 셋팅 SetNonWipControl()
        //비재공 정보 셋팅
        private void SetNonWipControl()
        {
            DataTable dtNonWip = new DataTable();
            dtNonWip.Columns.Add("NON_WIP_ID", typeof(string));
            dtNonWip.Columns.Add("PRJT_NAME", typeof(string));
            dtNonWip.Columns.Add("PRODID", typeof(string));
            dtNonWip.Columns.Add("MKT_TYPE_NAME", typeof(string));
            dtNonWip.Columns.Add("NON_WIP_QTY", typeof(int));
            dtNonWip.Columns.Add("MKT_TYPE_CODE", typeof(string));

            DataRow row = null;
            row = dtNonWip.NewRow();
            row["NON_WIP_ID"] = "[NEW]";
            row["PRJT_NAME"] = string.Empty;
            row["PRODID"] = string.Empty;
            row["MKT_TYPE_NAME"] = string.Empty;
            row["MKT_TYPE_CODE"] = string.Empty;
            dtNonWip.Rows.Add(row);

            Util.gridClear(dgNonWip);
            Util.GridSetData(dgNonWip, dtNonWip, FrameOperation, false);

        }
        #endregion

        #region 비재공 등록처리InputNonwip()
        /// <summary>
        ///  비재공 등록
        /// </summary>
        private void InputNonwip()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("NON_WIP_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACT_USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgNonWip.Rows[0].DataItem, "PRODID"));
                row["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgNonWip.Rows[0].DataItem, "MKT_TYPE_CODE"));
                row["PROCID"] = cboProcessScrap.SelectedValue.ToString();
                row["EQSGID"] = cboEquipmentSegmentScrap.SelectedValue.ToString();
                row["NON_WIP_QTY"] = txtNonWorkQty.Value;
                row["USERID"] = LoginInfo.USERID;
                row["ACT_USERID"] = txtUser.Tag;
                row["NOTE"] = string.Empty;

                inDataTable.Rows.Add(row);
                             
                try
                {
                    //
                    new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CREATE_NON_WIP", "INDATA", null, (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                            return;
                        }
                        this.DialogResult = MessageBoxResult.OK;

                    }, inData);

                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.AlertByBiz("BR_ACT_REG_CREATE_NON_WIP", ex.Message, ex.ToString());

                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조립 LOT 조회 AssyLot()
        /// <summary>
        ///  조립LOT 조회
        /// </summary>
        private void AssyLot()
        {
            try
            {

                if (txtLotRt.Text.Length < 4)
                {
                    // Lot ID는 4자리 이상 넣어 주세요.
                    Util.MessageValidation("SFU3450");
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("ASSYLOT", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["ASSYLOT"] = txtLotRt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSYLOT", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgLotRT, dtRslt, FrameOperation);
                    txtLotRt.Text = string.Empty;

                }
                else
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    txtLotRt.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region[[Validation]
        private bool Validation()
        {
            if (_LOTID_RT == string.Empty)
            {
                Util.MessageValidation("SFU4613");// 조립LOT을 선택하세요
                return false;
            }
            if (txtNonWorkQty.Value == 0)
            {
                Util.MessageValidation("SFU4639"); // 비재공수량을 입력하세요
                return false;
            }

            if(cboProcessScrap.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1459"); // 공정을 선택하세요
                return false;
            }
            if (cboEquipmentSegmentScrap.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1255"); // 라인을 선택하세요
                return false;
            }

            if (txtUser.Text == string.Empty || txtUser.Tag == null)
            {
                Util.MessageValidation("SFU4591"); // 작업자를 선택하세요
                return false;
            }

            

            return true;
        }

        #endregion


        #endregion

        #region 비재공 수정 

        #region 수정할 비재공 리스트 정보 셋팅 SetGridNonWipList()
        /// <summary>
        /// 수정할 비재공 리스트 정보 셋팅
        /// </summary>
        /// <param name="dt"></param>
        private void SetGridNonWipList(DataTable dt)
        {
            try
            {
                Util.GridSetData(dgNonWip, dt, FrameOperation);
                cboProcessScrap.SelectedValue = dt.Rows[0]["PROCID"].ToString();
                cboEquipmentSegmentScrap.SelectedValue = dt.Rows[0]["EQSGID"].ToString();  

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }


        #endregion

        #region 비재공 수정 팝업 셋팅 setNonWipUpdateSetting()
        /// <summary>
        ///  비재공 수정 팝업 셋팅 
        /// </summary>
        private void setNonWipUpdateSetting()
        {
            try
            {
                //등록버튼 숨기기
                btnInput.Visibility = Visibility.Collapsed;
                //수정버튼 보이기
                btnUpdate.Visibility = Visibility.Visible;
                //조립LOT 텍스트박스 비활성화
                txtLotRt.IsEnabled = false;
                //조립LOT 버튼 비활성화
                btnLotRtSearch.IsEnabled = false;
                //공정 비활성화 
                cboProcessScrap.IsEnabled = false;
                //라인 비활성화
                cboEquipmentSegmentScrap.IsEnabled = false;

                txtNonWorkQty.Value = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgNonWip.Rows[0].DataItem, "NON_WIP_QTY")).Replace(",", ""));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        #endregion

        #region 비재공 수정처리 UpdateNonwip()
        /// <summary>
        ///  비재공 수정
        /// </summary>
        private void UpdateNonwip()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("NON_WIP_ID", typeof(string));
                inDataTable.Columns.Add("NON_WIP_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACT_USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["NON_WIP_ID"] = Util.NVC(DataTableConverter.GetValue(dgNonWip.Rows[0].DataItem, "NON_WIP_ID"));
                row["NON_WIP_QTY"] = txtNonWorkQty.Value;
                row["USERID"] = LoginInfo.USERID;
                row["ACT_USERID"] = txtUser.Tag;
                row["NOTE"] = string.Empty;

                inDataTable.Rows.Add(row);

                try
                {
                    //
                    new ClientProxy().ExecuteService_Multi("BR_ACT_REG_MODIFY_NON_WIP", "INDATA", null, (Result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                            return;
                        }
                        this.DialogResult = MessageBoxResult.OK;

                    }, inData);

                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.AlertByBiz("BR_ACT_REG_MODIFY_NON_WIP", ex.Message, ex.ToString());

                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region[[Validation]
        private bool Validation_Update()
        {
          
            if (txtNonWorkQty.Value == 0)
            {
                Util.MessageValidation("비재공수량을 입력하세요"); // 비재공수량을 입력하세요
                return false;
            }
      

            if (txtUser.Text == string.Empty || txtUser.Tag == null)
            {
                Util.MessageValidation("SFU4591"); // 작업자를 선택하세요
                return false;
            }



            return true;
        }

        #endregion

        #endregion

        #region 공통

        #region 작업자 팝업 GetUserWindow()

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

                Parameters[0] = txtUser.Text;
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
        #endregion
       
        #endregion
        
        #region [Func]
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
