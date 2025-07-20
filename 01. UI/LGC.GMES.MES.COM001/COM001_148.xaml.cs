using C1.WPF;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid;

/*************************************************************************************
 Created Date : 2022.02.25
      Creator : 
   Decription : 슬라이딩 짝맞춤 그룹 관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.02.25  김지은 : Initial Created.
  2022.05.06  오화백 : 신규요구조건에 따른 화면디자인 및 소스 수정(Tab, Bottom에 따른 최대값 및 최소값, 조회쿼리, 저장 조건등)
**************************************************************************************/

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_148.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_148 : UserControl, IWorkArea
    {
        #region [Declaration]
        private string sSelLotProdId = string.Empty;
        private string sC_PRODID = string.Empty;
        private string sA_PRODID = string.Empty;

        private string sTrgt_Area   = string.Empty;
        private string sTrgt_Eqsgid = string.Empty;
        private string sTrgt_Equipment_C = string.Empty;
        private string sTrgt_Equipment_A = string.Empty;


        private decimal sC_MaxTabQty;
        private decimal sC_MinTabQty;

        private decimal sC_MaxBottomQty;
        private decimal sC_MinBottomQty;

        private decimal sA_MaxTabQty;
        private decimal sA_MinTabQty;

        private decimal sA_MaxBottomQty;
        private decimal sA_MinBottomQty;

        private readonly Util _util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region [Initialize]
        public COM001_148()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //grdResvTrgt.Visibility = Visibility.Collapsed;  // 목적지 예약 사용유무 확인 전, 안보이도록 처리함

            InitCombo();
            InitControl();
            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>-
        /// 콤보 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

           // 그룹유형
            String[] sFiltercboArea = { "SLID_MATC_GR_TYPE", LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboGroupType, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "COMMCODEATTR");
            _combo.SetCombo(cboDetailGroupType, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "COMMCODEATTR");

            SearchProduct(popSetCAProdID, cboDetailGroupType.SelectedValue?.ToString(), "CA");
            SearchProduct(popSetANProdID, cboDetailGroupType.SelectedValue?.ToString(), "AN");
            cboDetailGroupType.SelectedValueChanged += cboDetailGroupType_SelectedValueChanged;

            //목적지 동, 라인, 설비
            SetTrgtArea();
            SetTrgtLine();
            SetTrgtEquipment_C();
            SetTrgtEquipment_A();

        }

        /// <summary>
        /// 컨트롤 초기화
        /// </summary>
        private void InitControl()
        {
            dtpFDate.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpTDate.SelectedDateTime = DateTime.Now;

            SearchProduct(popSearchCAProdID, "PC,NP", "CA");
            SearchProduct(popSearchANProdID, "PC,NP", "AN");

            Util.gridClear(dgGroupList);

            ClearControl();
        }
        /// <summary>
        /// 초기화
        /// </summary>
        private void ClearControl()
        {
            Util.gridClear(dgLotList);
            Util.gridClear(dgLotList_A);

            txtGroupNo.Text = string.Empty;
            cboDetailGroupType.SelectedIndex = 0;

            popSetCAProdID.SelectedValue = null;
            popSetCAProdID.SelectedText = null;
            popSetANProdID.SelectedValue = null;
            popSetANProdID.SelectedText = null;
         

            txtMgrName.Text = string.Empty;
            txtMgrId.Text = string.Empty;
            txtRemark.Text = string.Empty;

            btnClear_A.Visibility = Visibility.Visible;
            btnClear_C.Visibility = Visibility.Visible;
            btnDeleteLot.Visibility = Visibility.Collapsed;
            btnDeleteLot_A.Visibility = Visibility.Collapsed;

            btnSaveLot_C.Visibility = Visibility.Visible;
            btnSaveLot_A.Visibility = Visibility.Visible;

            btnADDLot_C.Visibility = Visibility.Collapsed;
            btnADDLot_A.Visibility = Visibility.Collapsed;

            dgLotList.Columns["CHK"].Visibility = Visibility.Collapsed;
            dgLotList.Columns["DEL"].Visibility = Visibility.Visible;
            dgLotList_A.Columns["CHK"].Visibility = Visibility.Collapsed;
            dgLotList_A.Columns["DEL"].Visibility = Visibility.Visible;

            cboTrgtArea.SelectedIndex = 0;
            cboTrgtEquiptmentSegment.SelectedIndex = 0;
            // 양극 설비 초기화
            cboTrgtEquipment_C.SelectedIndex = 0;

            // 음극 목적지 초기화
            cboTrgtEquipment_A.SelectedIndex = 0;



            sC_PRODID = string.Empty;
            sA_PRODID = string.Empty;


            sTrgt_Area = string.Empty;
            sTrgt_Eqsgid = string.Empty;
            sTrgt_Equipment_C = string.Empty;
            sTrgt_Equipment_A = string.Empty;


    }
        /// <summary>
        /// 신규버전 초기화
        /// </summary>
        /// <param name="bIsEnabled"></param>
        private void SetControlEnable()
        {
            cboDetailGroupType.IsEnabled = true;
            popSetCAProdID.IsEnabled = true;
            popSetANProdID.IsEnabled = true;
          
            txtMgrName.IsEnabled = true;
            btnReqUser.IsEnabled = true;
            txtRemark.IsEnabled = true;
            btnSaveGroup.IsEnabled = true;

            cboTrgtArea.SelectedIndex = 0;
            cboTrgtEquiptmentSegment.SelectedIndex = 0;
            // 양극 설비 초기화
            cboTrgtEquipment_C.SelectedIndex = 0;

            // 음극 목적지 초기화
            cboTrgtEquipment_A.SelectedIndex = 0;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgGroupList.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", 0);
            }

        }

        /// <summary>
        /// 수정버전 초기화
        /// </summary>
        private void SetControlModify()
        {
            cboDetailGroupType.IsEnabled = false;
            popSetCAProdID.IsEnabled = false;
            popSetANProdID.IsEnabled = false;
          
            txtMgrName.IsEnabled = false;
            btnReqUser.IsEnabled = false;
            txtRemark.IsEnabled = true;
            btnSaveGroup.IsEnabled = true;

            btnClear_A.Visibility = Visibility.Collapsed;
            btnClear_C.Visibility = Visibility.Collapsed;
            btnDeleteLot.Visibility = Visibility.Visible;
            btnDeleteLot_A.Visibility = Visibility.Visible;

            btnSaveLot_C.Visibility = Visibility.Collapsed;
            btnSaveLot_A.Visibility = Visibility.Collapsed;
            btnADDLot_C.Visibility = Visibility.Visible;
            btnADDLot_A.Visibility = Visibility.Visible;

            dgLotList.Columns["CHK"].Visibility = Visibility.Visible;
            dgLotList.Columns["DEL"].Visibility = Visibility.Collapsed;
            dgLotList_A.Columns["CHK"].Visibility = Visibility.Visible;
            dgLotList_A.Columns["DEL"].Visibility = Visibility.Collapsed;

  
        }

        #endregion

        #region [Event]

        #region 타입콤보 선택시 제품정보 조회 : cboDetailGroupType_SelectedValueChanged()
        /// <summary>
        /// 타입선택시 제품정보 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboDetailGroupType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SearchProduct(popSetCAProdID, cboDetailGroupType.SelectedValue?.ToString(), "CA");
            SearchProduct(popSetANProdID, cboDetailGroupType.SelectedValue?.ToString(), "AN");
        }
        #endregion

        #region  GROUP 조회 :  btnSearch_Click()

        /// <summary>
        /// GROUP 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            SetControlEnable();
            Util.gridClear(dgGroupList);
            SearchGroupList();
        }

        #endregion

        #region 신규모드 선택 : btnNewGroup_Click()
        /// <summary>
        /// 신규모드 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNewGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            SetControlEnable();
        }

        #endregion

        #region GROUP 삭제 : btnDelGroup_Click()
        /// <summary>
        /// 그룹삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDeleteGroup()) return;
            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteGroup();
                }
            });

        }

        #endregion

        #region GROUP 저장 : btnSaveGroup_Click()
        /// <summary>
        /// 신규그룹저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveGroup()) return;
            if (txtGroupNo.Text == string.Empty)
            {
                // 저장하시겠습니까?
                Util.MessageConfirm("SFU1241", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveGroup();
                    }
                });

            }
            else
            {
                Util.MessageConfirm("SFU4340", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ModifyGroup("N");
                    }
                });


            }
        }


        #endregion

        #region 리스트에서 그룹 선택 : dgGrpChoice_Checked()

        /// <summary>
        /// Group List에서 Group 선택 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgGrpChoice_Checked(object sender, RoutedEventArgs e)
        {

            if (sender == null)
                return;
            RadioButton rb = sender as RadioButton;

            if (rb?.DataContext == null) return;

            if (!CommonVerify.HasDataGridRow(dgGroupList)) return;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                DataRowView drv = rb.DataContext as DataRowView;

                SetGroupContent(drv);

            }

        }

        #endregion
   
        #region 목적지 동 선택 : cboTrgtArea_SelectedValueChanged()
        /// <summary>
        /// 목적지 동 선택 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTrgtArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboTrgtArea == null || cboTrgtArea.ItemsSource == null) return;

                DataTable dtTmp = ((DataView)cboTrgtArea.ItemsSource).ToTable();

                if (dtTmp == null) return;

                this.cboTrgtEquiptmentSegment.ItemsSource = null;
                this.cboTrgtEquipment_C.ItemsSource = null;
                this.cboTrgtEquipment_A.ItemsSource = null;

                if (Util.NVC(dtTmp.Rows[cboTrgtArea.SelectedIndex]["LOGIS_TRF_TYPE_CODE"]).Equals("MANUAL"))  // 수동물류
                {
                    this.cboTrgtEquiptmentSegment.IsEnabled = true;
                    this.cboTrgtEquipment_C.IsEnabled = false;
                    this.cboTrgtEquipment_C.SelectedIndex = 0;
                    this.cboTrgtEquipment_A.IsEnabled = false;
                    this.cboTrgtEquipment_A.SelectedIndex = 0;
                    this.cboTrgtEquiptmentSegment.SelectedIndex = 0;
                
                    cboTrgtEquiptmentSegment.ApplyTemplate();
                    SetTrgtLine();
                }
                else
                {
                    //this.cboTrgtEquiptmentSegment.IsEnabled = false;
                    this.cboTrgtEquipment_C.IsEnabled = true;
                    this.cboTrgtEquipment_C.SelectedIndex = 0;
                    this.cboTrgtEquipment_A.IsEnabled = true;
                    this.cboTrgtEquipment_A.SelectedIndex = 0;
                    this.cboTrgtEquiptmentSegment.SelectedIndex = 0;
                    SetTrgtLine();
                    SetTrgtEquipment_A();
                    SetTrgtEquipment_C();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

         
        
        #region 담당자 이름 및 아이디 이벤트 : txtMgrName_KeyDown()
        /// <summary>
        /// 담당자 이름 혹은 ID 입력 후 Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtMgrName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow(txtMgrName);
            }
        }


        #endregion

        #region 담당자 찾기 버튼  : btnReqUser_Click()
        /// <summary>
        /// 담당자 찾기 버튼 누를 때
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow(txtMgrName);
        }

        #endregion

        #region 사용자 조회 팝업 종료  : wndUser_Closed()
        /// <summary>
        /// User 조회 팝업 종료 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                TextBox tb1 = txtMgrName;
                TextBox tb2 = txtMgrId;

                tb1.Text = wndPerson.USERNAME;
                tb1.Tag = wndPerson.USERID;
                tb2.Text = wndPerson.USERID;
            }
        }

        #endregion

      
        #region 양극 LOT 조회 버튼 클릭_신규입력시 : btnSaveLot_C_Click()
        /// <summary>
        /// 양극LOT 추가ㅣ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveLot_C_Click(object sender, RoutedEventArgs e)
        {
            //if (cboDetailGroupType.SelectedIndex == 0)
            //{
            //    // 그룹 유형을 선택하세요
            //    object[] parameters = new object[1];
            //    parameters[0] = ObjectDic.Instance.GetObjectName("그룹유형");

            //    Util.MessageValidation("SFU4925", parameters);
            //    return;
            //}
            if (string.IsNullOrEmpty(popSetCAProdID.SelectedValue?.ToString()))
            {
                // 양극 제품ID를 선택해주세요.
                object[] parameters = new object[1];
                parameters[0] = ObjectDic.Instance.GetObjectName("양극 제품ID");
                Util.MessageValidation("SFU4925", parameters);

                return;
            }
            AddGroupLot("C");
        }


        #endregion

        #region 음극 LOT 조회 버튼 클릭_신규입력시 : btnSaveLot_A_Click()
        /// <summary>
        /// 음극 LOT 조회 버튼 클릭 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveLot_A_Click(object sender, RoutedEventArgs e)
        {
            //if (cboDetailGroupType.SelectedIndex == 0)
            //{
            //    // 그룹 유형을 선택하세요
            //    object[] parameters = new object[1];
            //    parameters[0] = ObjectDic.Instance.GetObjectName("그룹유형");

            //    Util.MessageValidation("SFU4925", parameters);
            //    return;
            //}
            if (string.IsNullOrEmpty(popSetANProdID.SelectedValue?.ToString()))
            {
                // 음극 제품ID를 선택해주세요.
                object[] parameters = new object[1];
                parameters[0] = ObjectDic.Instance.GetObjectName("음극 제품ID");
                Util.MessageValidation("SFU4925", parameters);

                return;
            }
            AddGroupLot("A");
        }


        #endregion

        #region 양극 LOT 리스트 전체 해제 _신규입력시 :  btnClear_C_Click()
        /// <summary>
        /// 양극 LOT 리스트 전체 해제 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_C_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgLotList);
            sC_PRODID = string.Empty;
        }
        #endregion

        #region 음극 LOT 리스트 전체 해제 _신규입력시 :  btnClear_A_Click()
        /// <summary>
        /// 음극 LOT리스트 전체 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_A_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgLotList_A);
            sA_PRODID = string.Empty;
        }
        #endregion

        #region 양극 LOT 리스트 개별 ROW 삭제 _ 신규입력시 : btnDelRow_C_Click()
        /// <summary>
        /// 양극 LOT 리스트 개별 ROW 삭제 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelRow_C_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;

            DataTable dtLotList_C = DataTableConverter.Convert(dgLotList.ItemsSource);
            dtLotList_C.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, dtLotList_C, this.FrameOperation, true);

            if (dtLotList_C.Rows.Count == 0)
            {
                sC_PRODID = string.Empty;
            }
        }
        #endregion

        #region 음극 LOT 리스트 개별 ROW 삭제 _ 신규입력시 : btnDelRow_A_Click()
        /// <summary>
        /// 음극 LOT 리스트 개별 ROW 삭제 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelRow_A_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            DataTable dtLotList_A = DataTableConverter.Convert(dgLotList_A.ItemsSource);
            dtLotList_A.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, dtLotList_A, this.FrameOperation, true);

            if (dtLotList_A.Rows.Count == 0)
            {
                sA_PRODID = string.Empty;
            }
        }
        #endregion


        #region  양극 LOT 추가 처리 _ 그룹내용 수정시 : btnADDLot_C_Click()
        /// <summary>
        /// 양극 LOT 추가 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnADDLot_C_Click(object sender, RoutedEventArgs e)
        {

            // 담당자 : {%1} 와(과) 수정자 : {%2} 가 틀리면 삭제 및 수정할 수 없습니다.
            if (txtMgrId.Text.ToString() != LoginInfo.USERID)
            {
                object[] Checkparameters = new object[3];
                Checkparameters[0] = txtMgrName.Text;
                Checkparameters[1] = LoginInfo.USERNAME;
                Checkparameters[2] = ObjectDic.Instance.GetObjectName("추가");
                Util.MessageValidation("SFU8495", Checkparameters);
                return ;
            }

            COM001_148_LOTLIST popupLotList = new COM001_148_LOTLIST { FrameOperation = FrameOperation };
            object[] parameters = new object[3];
            parameters[0] = cboDetailGroupType.SelectedValue.ToString();
            parameters[1] = popSetCAProdID.SelectedText.ToString();
            parameters[2] = "C";
            C1WindowExtension.SetParameters(popupLotList, parameters);

            popupLotList.Closed += popupLotList_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupLotList.ShowModal()));
        }
        #endregion

        #region 음극 LOT 추가 처리_ 그룹내용 수정시  : btnADDLot_A_Click()
        /// <summary>
        /// 음극 LOT 추가 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnADDLot_A_Click(object sender, RoutedEventArgs e)
        {
            // 담당자 : {%1} 와(과) 수정자 : {%2} 가 틀리면 삭제 및 수정할 수 없습니다.
            if (txtMgrId.Text.ToString() != LoginInfo.USERID)
            {
                object[] Checkparameters = new object[3];
                Checkparameters[0] = txtMgrName.Text;
                Checkparameters[1] = LoginInfo.USERNAME;
                Checkparameters[2] = ObjectDic.Instance.GetObjectName("추가");
                Util.MessageValidation("SFU8495", Checkparameters);
                return;
            }

            COM001_148_LOTLIST popupLotList = new COM001_148_LOTLIST { FrameOperation = FrameOperation };
            object[] parameters = new object[3];
            parameters[0] = cboDetailGroupType.SelectedValue.ToString();
            parameters[1] = popSetANProdID.SelectedText.ToString();
            parameters[2] = "A";
            C1WindowExtension.SetParameters(popupLotList, parameters);

            popupLotList.Closed += popupLotList_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupLotList.ShowModal()));
        }
        #endregion

        #region  양극 제품 Lot 해제 버튼 클릭 _ 그룹내용 수정시 : btnDeleteLot_Click()
        /// <summary>
        /// 양극 제품 Lot 해제 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteLot_Click(object sender, RoutedEventArgs e)
        {

            // 담당자 : {%1} 와(과) 수정자 : {%2} 가 틀리면 삭제 및 수정할 수 없습니다.
            if (txtMgrId.Text.ToString() != LoginInfo.USERID)
            {
                object[] Checkparameters = new object[3];
                Checkparameters[0] = txtMgrName.Text;
                Checkparameters[1] = LoginInfo.USERNAME;
                Checkparameters[2] = ObjectDic.Instance.GetObjectName("해제");
                Util.MessageValidation("SFU8495", Checkparameters);
                return;
            }

            DataTable dtLotList_C = DataTableConverter.Convert(dgLotList.ItemsSource);
            DataTable dtLotList_A = DataTableConverter.Convert(dgLotList_A.ItemsSource);

            DataRow[] dr_C = dtLotList_C.Select("CHK = 0");
            DataRow[] dr_A = dtLotList_A.Select("CHK = 0");

            DataRow[] selectRows = dtLotList_C.Select("CHK = 1");

            if (selectRows.Length == 0)
            {
                // 선택 된 Lot이 없습니다.
                Util.MessageValidation("SFU1661");
                return;
            }
            DataTable dtLot = selectRows.AsEnumerable().CopyToDataTable();



            if (dr_C.Length == 0 && dr_A.Length == 0)
            {
                // 그룹에 속한 LOT이 존재하지 않아서 그룹 삭제 처리됩니다. 
                //  해제 처리 하시겠습니까?
                Util.MessageConfirm("SFU8493", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveGroupLot("DEL", "C", dtLot, "N");
                    }
                });
            }
            else
            {
                // 해제 처리하시겠습니까?
                Util.MessageConfirm("SFU4946", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveGroupLot("DEL", "C", dtLot, "N");
                    }
                });
            }


        }
        #endregion

        #region 음극 제품 Lot 해제 버튼 클릭 _ 그룹내용 수정시 : btnDeleteLot_A_Click()
        /// <summary>
        /// 음극 제품 LOT 해제 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteLot_A_Click(object sender, RoutedEventArgs e)
        {
            // 담당자 : {%1} 와(과) 수정자 : {%2} 가 틀리면 삭제 및 수정할 수 없습니다.
            if (txtMgrId.Text.ToString() != LoginInfo.USERID)
            {
                object[] Checkparameters = new object[3];
                Checkparameters[0] = txtMgrName.Text;
                Checkparameters[1] = LoginInfo.USERNAME;
                Checkparameters[2] = ObjectDic.Instance.GetObjectName("해제");
                Util.MessageValidation("SFU8495", Checkparameters);
                return;
            }

            DataTable dtLotList_C = DataTableConverter.Convert(dgLotList.ItemsSource);
            DataTable dtLotList_A = DataTableConverter.Convert(dgLotList_A.ItemsSource);

            //체크한 LOT외 데이터가 존재하는지 체크
            DataRow[] dr_C = dtLotList_C.Select("CHK = 0");
            DataRow[] dr_A = dtLotList_A.Select("CHK = 0");

            DataRow[] selectRows = dtLotList_A.Select("CHK = 1");

            if (selectRows.Length == 0)
            {
                // 선택 된 Lot이 없습니다.
                Util.MessageValidation("SFU1661");
                return;
            }
            DataTable dtLot = selectRows.AsEnumerable().CopyToDataTable();

            if (dr_C.Length == 0 && dr_A.Length == 0)
            {
                // 그룹에 속한 LOT이 존재하지 않아서 그룹 삭제 처리됩니다. 
                //  해제 처리 하시겠습니까?
                Util.MessageConfirm("SFU8493", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {


                        SaveGroupLot("DEL", "A", dtLot, "N");
                    }
                });
            }
            else
            {
                // 해제 처리하시겠습니까?
                Util.MessageConfirm("SFU4946", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveGroupLot("DEL", "A", dtLot, "N");
                    }
                });
            }




        }
        #endregion

        #region LOTLIST 팝업 닫기 : popupLotList_Closed()

        private void popupLotList_Closed(object sender, EventArgs e)
        {
            COM001_148_LOTLIST wndPerson = sender as COM001_148_LOTLIST;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                if (wndPerson._Polarity == "C")
                {
                    DataTable dtLotList_C = DataTableConverter.Convert(dgLotList.ItemsSource);
                    //동일한 LOT ID의 경우 DATA TABLE에서 삭제 처리

                    if (dtLotList_C.Rows.Count > 0)
                    {

                        for (int i = wndPerson.dtOutLotList.Rows.Count; i > 0; i--)
                        {
                            if (dtLotList_C.Select("LOTID = '" + wndPerson.dtOutLotList.Rows[i - 1]["LOTID"].ToString() + "'").Length > 0)
                            {
                                wndPerson.dtOutLotList.Rows[i - 1].Delete();
                            }
                        }
                    }
                    dtLotList_C.Merge(wndPerson.dtOutLotList);
                    dtLotList_C.AcceptChanges();
                    Util.GridSetData(dgLotList, dtLotList_C, FrameOperation, true);
                    sC_PRODID = wndPerson._Prodid;
                    sC_MaxTabQty = Convert.ToDecimal(dtLotList_C.Compute("MAX(TAB_VALUE)", null));
                    sC_MinTabQty = Convert.ToDecimal(dtLotList_C.Compute("MIN(TAB_VALUE)", null));
                    sC_MaxBottomQty = Convert.ToDecimal(dtLotList_C.Compute("MAX(BOTTOM_VALUE)", null));
                    sC_MinBottomQty = Convert.ToDecimal(dtLotList_C.Compute("MIN(BOTTOM_VALUE)", null));

                    if (txtGroupNo.Text != string.Empty)
                    {
                        DataRow[] selectRows = null;
                        selectRows = Util.gridGetChecked(ref dgLotList, "CHK");
                        DataTable dtLot = selectRows.AsEnumerable().CopyToDataTable();

                        SaveGroupLot("REG", "C", dtLot, "N");
                    }

                }
                else
                {
                    DataTable dtLotList_A = DataTableConverter.Convert(dgLotList_A.ItemsSource);
                    if (dtLotList_A.Rows.Count > 0)
                    {
                        //동일한 LOT ID의 경우 DATA TABLE에서 삭제 처리
                        for (int i = wndPerson.dtOutLotList.Rows.Count; i > 0; i--)
                        {
                            if (dtLotList_A.Select("LOTID = '" + wndPerson.dtOutLotList.Rows[i - 1]["LOTID"].ToString() + "'").Length > 0)
                            {
                                wndPerson.dtOutLotList.Rows[i - 1].Delete();
                            }
                        }
                    }
                    dtLotList_A.Merge(wndPerson.dtOutLotList);
                    dtLotList_A.AcceptChanges();
                    Util.GridSetData(dgLotList_A, dtLotList_A, FrameOperation, true);
                    sA_PRODID = wndPerson._Prodid;
                    sA_MaxTabQty = Convert.ToDecimal(dtLotList_A.Compute("MAX(TAB_VALUE)", null));
                    sA_MinTabQty = Convert.ToDecimal(dtLotList_A.Compute("MIN(TAB_VALUE)", null));
                    sA_MaxBottomQty = Convert.ToDecimal(dtLotList_A.Compute("MAX(BOTTOM_VALUE)", null));
                    sA_MinBottomQty = Convert.ToDecimal(dtLotList_A.Compute("MIN(BOTTOM_VALUE)", null));

                    if (txtGroupNo.Text != string.Empty)
                    {
                        DataRow[] selectRows = null;
                        selectRows = Util.gridGetChecked(ref dgLotList_A, "CHK");
                        DataTable dtLot = selectRows.AsEnumerable().CopyToDataTable();
                        SaveGroupLot("REG", "A", dtLot, "N");
                    }

                }
            }

        }
        #endregion

        #endregion

        #region [Method]
     
        #region  TYPE에 대한 제품 정보 조회 : SearchProduct()

        /// <summary>
        /// TYPE에 대한 제품 정보 조회
        /// </summary>
        /// <param name="pop"></param>
        /// <param name="sMtrlCategory"></param>
        /// <param name="sElecType"></param>
        private void SearchProduct(PopupFindControl pop, string sMtrlCategory, string sElecType)
        {
            try
            {
                pop.SelectedValue = null;
                pop.SelectedText = null;
                pop.ItemsSource = null;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PRODUCT_LEVEL2_CODE", typeof(string));
                inDataTable.Columns.Add("PRODUCT_LEVEL3_CODE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["PRODUCT_LEVEL2_CODE"] = sMtrlCategory;
                newRow["PRODUCT_LEVEL3_CODE"] = sElecType;
                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_VW_PRODUCT", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        pop.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }
        #endregion

        #region GROUP 정보 조회 : SearchGroupList()
        /// <summary>
        /// 등록된 Group 조회
        /// </summary>
        private void SearchGroupList()
        {
            try
            {
                DataTable dt = new DataTable("RQSTDT");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("FROM_DTTM", typeof(DateTime));
                dt.Columns.Add("TO_DTTM", typeof(DateTime));
                dt.Columns.Add("SLID_MATC_GR_TYPE", typeof(string));
                dt.Columns.Add("CA_PRODID", typeof(string));
                dt.Columns.Add("AN_PRODID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = dtpFDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TO_DTTM"] = dtpTDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                dr["SLID_MATC_GR_TYPE"] = Util.GetCondition(cboGroupType, bAllNull: true);
                dr["CA_PRODID"] = popSearchCAProdID.SelectedValue?.ToString();
                dr["AN_PRODID"] = popSearchANProdID.SelectedValue?.ToString();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_SLID_MATC_GR_MNGT", "RQSTDT", "RSLTDT", dt);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgGroupList, dtResult, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        #endregion

        #region GROUP 정보 삭제 : DeleteGroup()
        /// <summary>
        /// 등록된 Group 삭제
        /// </summary>
        private void DeleteGroup()
        {
            try
            {
                if (string.IsNullOrEmpty(Util.GetCondition(txtGroupNo)))
                {
                    // 삭제할 항목이 없습니다.
                    Util.MessageValidation("SFU1597");
                    return;
                }


                ShowLoadingIndicator();

                //그룹에 속한 LOT이 존재한다면 먼저 해제 처리 후  그룹 삭제
                DataTable dtLotList_C = DataTableConverter.Convert(dgLotList.ItemsSource);
                DataTable dtLotList_A = DataTableConverter.Convert(dgLotList_A.ItemsSource);

                if (dtLotList_C.Rows.Count > 0)
                {
                    sC_MaxTabQty = Convert.ToDecimal(dtLotList_C.Compute("MAX(TAB_VALUE)", null));
                    sC_MinTabQty = Convert.ToDecimal(dtLotList_C.Compute("MIN(TAB_VALUE)", null));
                    sC_MaxBottomQty = Convert.ToDecimal(dtLotList_C.Compute("MAX(BOTTOM_VALUE)", null));
                    sC_MinBottomQty = Convert.ToDecimal(dtLotList_C.Compute("MIN(BOTTOM_VALUE)", null));
                }
                else
                {
                    sC_MaxTabQty = 0;
                    sC_MinTabQty = 0;
                    sC_MaxBottomQty = 0;
                    sC_MinBottomQty = 0;
                }
               if (dtLotList_A.Rows.Count > 0)
                {
                    sA_MaxTabQty = Convert.ToDecimal(dtLotList_A.Compute("MAX(TAB_VALUE)", null));
                    sA_MinTabQty = Convert.ToDecimal(dtLotList_A.Compute("MIN(TAB_VALUE)", null));
                    sA_MaxBottomQty = Convert.ToDecimal(dtLotList_A.Compute("MAX(BOTTOM_VALUE)", null));
                    sA_MinBottomQty = Convert.ToDecimal(dtLotList_A.Compute("MIN(BOTTOM_VALUE)", null));
                }
                else
                {
                    sA_MaxTabQty = 0;
                    sA_MinTabQty = 0;
                    sA_MaxBottomQty = 0;
                    sA_MinBottomQty = 0;
                }


                //양극 제품LOT
                if (dtLotList_C.Rows.Count > 0)
                {
                    SaveGroupLot("DEL", "C", dtLotList_C, "Y");
                }
                //음극 제품LOT
                if (dgLotList_A.Rows.Count > 0)
                {
                    SaveGroupLot("DEL", "A", dtLotList_A, "Y");
                }

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("SLID_MATC_GR_NO", typeof(string));
                //양극 Tab 수량
                dt.Columns.Add("CA_PRODID", typeof(string));
                dt.Columns.Add("CA_SLID_MINVALUE", typeof(decimal));
                dt.Columns.Add("CA_SLID_MAXVALUE", typeof(decimal));
                //양극 BOTTOM 수향
                dt.Columns.Add("CA_SLID_MINVALUE2", typeof(decimal));
                dt.Columns.Add("CA_SLID_MAXVALUE2", typeof(decimal));
                //음극 Tab 수량
                dt.Columns.Add("AN_PRODID", typeof(string));
                dt.Columns.Add("AN_SLID_MINVALUE", typeof(decimal));
                dt.Columns.Add("AN_SLID_MAXVALUE", typeof(decimal));
                //음극 BOTTOM 수량
                dt.Columns.Add("AN_SLID_MINVALUE2", typeof(decimal));
                dt.Columns.Add("AN_SLID_MAXVALUE2", typeof(decimal));

                dt.Columns.Add("RSV_EQSGID", typeof(string));
                dt.Columns.Add("RSV_EQPTID", typeof(string));

                //비고 등록자
                dt.Columns.Add("REMARKS", typeof(string));
                dt.Columns.Add("MGR_USERID", typeof(string));
                dt.Columns.Add("USERID", typeof(string));

                DataRow newRow = dt.NewRow();

                newRow["ACTID"] = "DELETE_GR";
                newRow["SLID_MATC_GR_NO"] = Util.GetCondition(txtGroupNo);
                newRow["CA_PRODID"] = popSetCAProdID.SelectedText;
                newRow["CA_SLID_MINVALUE"] = sC_MinTabQty;
                newRow["CA_SLID_MAXVALUE"] = sC_MaxTabQty;
                newRow["CA_SLID_MINVALUE2"] = sC_MinBottomQty;
                newRow["CA_SLID_MAXVALUE2"] = sC_MaxBottomQty;

                newRow["AN_PRODID"] = popSetANProdID.SelectedText;
                newRow["AN_SLID_MINVALUE"] = sA_MinTabQty;
                newRow["AN_SLID_MAXVALUE"] = sA_MaxTabQty;
                newRow["AN_SLID_MINVALUE2"] = sA_MinBottomQty;
                newRow["AN_SLID_MAXVALUE2"] = sA_MaxBottomQty;

                if (Util.GetCondition(cboTrgtEquipment_C) == string.Empty || Util.GetCondition(cboTrgtEquipment_A) == string.Empty)
                {
                    newRow["RSV_EQSGID"] = Util.GetCondition(cboTrgtEquiptmentSegment);
                    newRow["RSV_EQPTID"] = null;
                }
                else
                {
                    newRow["RSV_EQSGID"] = null;
                    newRow["RSV_EQPTID"] = Util.GetCondition(cboTrgtEquipment_C) + "," + Util.GetCondition(cboTrgtEquipment_C);
                }
                newRow["REMARKS"] = Util.GetCondition(txtRemark);
                newRow["MGR_USERID"] = Util.GetCondition(txtMgrId);
                newRow["USERID"] = LoginInfo.USERID;

                dt.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_SLID_MATC_GR", "INDATA", null, dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        ClearControl();
                        SetControlEnable();
                        Util.gridClear(dgGroupList);
                        SearchGroupList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        #endregion

        #region GROUP 정보 신규등록 : SaveGroup()
        /// <summary>
        /// Group 신규 등록
        /// </summary>
        private void SaveGroup()
        {
            try
            {

                ShowLoadingIndicator();

                DataSet ds = new DataSet();
                // +++++++++++++++++++++ INDATA +++++++++++++++++++
                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("SLID_MATC_GR_NO", typeof(string));
                dt.Columns.Add("SLID_MATC_GR_TYPE", typeof(string));
                dt.Columns.Add("CA_PRODID", typeof(string));
                //양극 Tab 수량
                dt.Columns.Add("CA_SLID_MINVALUE", typeof(decimal));
                dt.Columns.Add("CA_SLID_MAXVALUE", typeof(decimal));
                //양극 BOTTOM 수향
                dt.Columns.Add("CA_SLID_MINVALUE2", typeof(decimal));
                dt.Columns.Add("CA_SLID_MAXVALUE2", typeof(decimal));
                dt.Columns.Add("AN_PRODID", typeof(string));
                //음극 Tab 수량
                dt.Columns.Add("AN_SLID_MINVALUE", typeof(decimal));
                dt.Columns.Add("AN_SLID_MAXVALUE", typeof(decimal));
                //음극 BOTTOM 수량
                dt.Columns.Add("AN_SLID_MINVALUE2", typeof(decimal));
                dt.Columns.Add("AN_SLID_MAXVALUE2", typeof(decimal));
                dt.Columns.Add("RSV_EQSGID", typeof(string));
                dt.Columns.Add("RSV_EQPTID", typeof(string));
                dt.Columns.Add("REMARKS", typeof(string));
                dt.Columns.Add("MGR_USERID", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow newRow = dt.NewRow();
                newRow["ACTID"] = "CREATE_GR";
                newRow["SLID_MATC_GR_NO"] = Util.GetCondition(txtGroupNo);
                newRow["SLID_MATC_GR_TYPE"] = cboDetailGroupType.SelectedValue.ToString();
                newRow["CA_PRODID"] = sC_PRODID;
                newRow["CA_SLID_MINVALUE"] = sC_MinTabQty;
                newRow["CA_SLID_MAXVALUE"] = sC_MaxTabQty;
                newRow["CA_SLID_MINVALUE2"] = sC_MinBottomQty;
                newRow["CA_SLID_MAXVALUE2"] = sC_MaxBottomQty;
                newRow["AN_PRODID"] = sA_PRODID;
                newRow["AN_SLID_MINVALUE"] = sA_MinTabQty;
                newRow["AN_SLID_MAXVALUE"] = sA_MaxTabQty;
                newRow["AN_SLID_MINVALUE2"] = sA_MinBottomQty;
                newRow["AN_SLID_MAXVALUE2"] = sA_MaxBottomQty;

                if(cboTrgtEquipment_C.IsEnabled == false || cboTrgtEquipment_A.IsEnabled == false)
                {
                    newRow["RSV_EQSGID"] = Util.GetCondition(cboTrgtEquiptmentSegment);
                    newRow["RSV_EQPTID"] = null;
                }
               else
                {
                    newRow["RSV_EQSGID"] = null;
                    newRow["RSV_EQPTID"] = Util.GetCondition(cboTrgtEquipment_C) + "," + Util.GetCondition(cboTrgtEquipment_A);
                }
                 newRow["REMARKS"] = Util.GetCondition(txtRemark);
                newRow["MGR_USERID"] = Util.GetCondition(txtMgrId);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(newRow);


                // +++++++++++++++++++++ IN_LOT +++++++++++++++++++
                DataTable dt_LOT = ds.Tables.Add("IN_LOT");
                dt_LOT.Columns.Add("LOTID", typeof(string));
                dt_LOT.Columns.Add("RSV_EQSGID", typeof(string));
                dt_LOT.Columns.Add("RSV_EQPTID", typeof(string));

                DataTable dtLotList_C = ((DataView)dgLotList.ItemsSource).Table;

                if (dtLotList_C.Rows.Count > 0)
                {
                    for (int i = 0; i < dtLotList_C.Rows.Count; i++)
                    {
                        DataRow newLotRow = dt_LOT.NewRow();
                        newLotRow["LOTID"] = dtLotList_C.Rows[i]["LOTID"].ToString();
                        if (cboTrgtEquipment_C.IsEnabled == false || cboTrgtEquipment_A.IsEnabled == false)
                        {
                            newLotRow["RSV_EQSGID"] = Util.GetCondition(cboTrgtEquiptmentSegment);
                            newLotRow["RSV_EQPTID"] = null;
                        }
                        else
                        {
                            newLotRow["RSV_EQSGID"] = null;
                            newLotRow["RSV_EQPTID"] = Util.GetCondition(cboTrgtEquipment_C);
                        }
                      
                        dt_LOT.Rows.Add(newLotRow);
                    }

                }
                DataTable dtLotList_A = ((DataView)dgLotList_A.ItemsSource).Table;

                if (dtLotList_A.Rows.Count > 0)
                {
                    for (int i = 0; i < dtLotList_A.Rows.Count; i++)
                    {
                        DataRow newLotRow = dt_LOT.NewRow();
                        newLotRow["LOTID"] = dtLotList_A.Rows[i]["LOTID"].ToString();
                        if (cboTrgtEquipment_C.IsEnabled == false || cboTrgtEquipment_A.IsEnabled == false)
                        {
                            newLotRow["RSV_EQSGID"] = Util.GetCondition(cboTrgtEquiptmentSegment);
                            newLotRow["RSV_EQPTID"] = null;
                        }
                        else
                        {
                            newLotRow["RSV_EQSGID"] = null;
                            newLotRow["RSV_EQPTID"] = Util.GetCondition(cboTrgtEquipment_A);
                        }
                       
                        dt_LOT.Rows.Add(newLotRow);
                    }

                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SLID_MATC_GR", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        ClearControl();
                        Util.gridClear(dgGroupList);
                        SearchGroupList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        #endregion

        #region GROUP 정보 수정 : ModifyGroup()
        /// <summary>
        ///그룹에 대한 LOT 추가후 MIN/MAX 값수정
        /// </summary>
        private void ModifyGroup(string AddBottonYN)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet ds = new DataSet();
                // +++++++++++++++++++++ INDATA +++++++++++++++++++
                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("SLID_MATC_GR_NO", typeof(string));
                //양극 Tab 수량
                dt.Columns.Add("CA_PRODID", typeof(string));
                dt.Columns.Add("CA_SLID_MINVALUE", typeof(decimal));
                dt.Columns.Add("CA_SLID_MAXVALUE", typeof(decimal));
                //양극 BOTTOM 수향
                dt.Columns.Add("CA_SLID_MINVALUE2", typeof(decimal));
                dt.Columns.Add("CA_SLID_MAXVALUE2", typeof(decimal));
                //음극 Tab 수량
                dt.Columns.Add("AN_PRODID", typeof(string));
                dt.Columns.Add("AN_SLID_MINVALUE", typeof(decimal));
                dt.Columns.Add("AN_SLID_MAXVALUE", typeof(decimal));
                //음극 BOTTOM 수량
                dt.Columns.Add("AN_SLID_MINVALUE2", typeof(decimal));
                dt.Columns.Add("AN_SLID_MAXVALUE2", typeof(decimal));

                dt.Columns.Add("RSV_EQSGID", typeof(string));
                dt.Columns.Add("RSV_EQPTID", typeof(string));

                //비고 등록자
                dt.Columns.Add("REMARKS", typeof(string));
                dt.Columns.Add("MGR_USERID", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataTable dtLotList_C = DataTableConverter.Convert(dgLotList.ItemsSource);
                if (dtLotList_C.Rows.Count > 0)
                {
                    sC_MaxTabQty = Convert.ToDecimal(dtLotList_C.Compute("MAX(TAB_VALUE)", null));
                    sC_MinTabQty = Convert.ToDecimal(dtLotList_C.Compute("MIN(TAB_VALUE)", null));
                    sC_MaxBottomQty = Convert.ToDecimal(dtLotList_C.Compute("MAX(BOTTOM_VALUE)", null));
                    sC_MinBottomQty = Convert.ToDecimal(dtLotList_C.Compute("MIN(BOTTOM_VALUE)", null));
                }
                else
                {
                    sC_MaxTabQty = 0;
                    sC_MinTabQty = 0;
                    sC_MaxBottomQty = 0;
                    sC_MinBottomQty = 0;
                }
                DataTable dtLotList_A = DataTableConverter.Convert(dgLotList_A.ItemsSource);
                if (dtLotList_A.Rows.Count > 0)
                {
                    sA_MaxTabQty = Convert.ToDecimal(dtLotList_A.Compute("MAX(TAB_VALUE)", null));
                    sA_MinTabQty = Convert.ToDecimal(dtLotList_A.Compute("MIN(TAB_VALUE)", null));
                    sA_MaxBottomQty = Convert.ToDecimal(dtLotList_A.Compute("MAX(BOTTOM_VALUE)", null));
                    sA_MinBottomQty = Convert.ToDecimal(dtLotList_A.Compute("MIN(BOTTOM_VALUE)", null));
                }
                else
                {
                    sA_MaxTabQty = 0;
                    sA_MinTabQty = 0;
                    sA_MaxBottomQty = 0;
                    sA_MinBottomQty = 0;
                }
                DataRow newRow = dt.NewRow();
                newRow["ACTID"] = "MODIFY_GR";
                newRow["SLID_MATC_GR_NO"] = Util.GetCondition(txtGroupNo);
                newRow["CA_PRODID"] = popSetCAProdID.SelectedText;
                newRow["CA_SLID_MINVALUE"] = sC_MinTabQty;
                newRow["CA_SLID_MAXVALUE"] = sC_MaxTabQty;
                newRow["CA_SLID_MINVALUE2"] = sC_MinBottomQty;
                newRow["CA_SLID_MAXVALUE2"] = sC_MaxBottomQty;

                newRow["AN_PRODID"] = popSetANProdID.SelectedText;
                newRow["AN_SLID_MINVALUE"] = sA_MinTabQty;
                newRow["AN_SLID_MAXVALUE"] = sA_MaxTabQty;
                newRow["AN_SLID_MINVALUE2"] = sA_MinBottomQty;
                newRow["AN_SLID_MAXVALUE2"] = sA_MaxBottomQty;
                if (cboTrgtEquipment_C.IsEnabled == false || cboTrgtEquipment_A.IsEnabled == false)
                {
                    newRow["RSV_EQSGID"] = Util.GetCondition(cboTrgtEquiptmentSegment);
                    newRow["RSV_EQPTID"] = null;
                }
                else
                {
                    newRow["RSV_EQSGID"] = null;
                    newRow["RSV_EQPTID"] = Util.GetCondition(cboTrgtEquipment_C) + "," + Util.GetCondition(cboTrgtEquipment_A);
                }
                newRow["REMARKS"] = Util.GetCondition(txtRemark);
                newRow["MGR_USERID"] = Util.GetCondition(txtMgrId);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(newRow);

                // +++++++++++++++++++++ IN_LOT +++++++++++++++++++
                DataTable dt_LOT = ds.Tables.Add("IN_LOT");
                dt_LOT.Columns.Add("LOTID", typeof(string));
                dt_LOT.Columns.Add("RSV_EQSGID", typeof(string));
                dt_LOT.Columns.Add("RSV_EQPTID", typeof(string));
                // 수정모드일 경우 추가 버튼클릭이 아닐경우
                if (AddBottonYN == "N")
                {
                    if (!(sTrgt_Area == Util.GetCondition(cboTrgtArea) && sTrgt_Eqsgid == Util.GetCondition(cboTrgtEquiptmentSegment) && sTrgt_Equipment_C == Util.GetCondition(cboTrgtEquipment_C)))
                    {
                        if (dtLotList_C.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtLotList_C.Rows.Count; i++)
                            {
                                DataRow newLotRow = dt_LOT.NewRow();
                                newLotRow["LOTID"] = dtLotList_C.Rows[i]["LOTID"].ToString();
                                if (cboTrgtEquipment_C.IsEnabled == false || cboTrgtEquipment_A.IsEnabled == false)
                                {
                                    newLotRow["RSV_EQSGID"] = Util.GetCondition(cboTrgtEquiptmentSegment);
                                    newLotRow["RSV_EQPTID"] = null;
                                }
                                else
                                {
                                    newLotRow["RSV_EQSGID"] = null;
                                    newLotRow["RSV_EQPTID"] = Util.GetCondition(cboTrgtEquipment_C);
                                }

                                dt_LOT.Rows.Add(newLotRow);
                            }

                        }

                    }
                    if (!(sTrgt_Area == Util.GetCondition(cboTrgtArea) && sTrgt_Eqsgid == Util.GetCondition(cboTrgtEquiptmentSegment) && sTrgt_Equipment_A == Util.GetCondition(cboTrgtEquipment_A)))
                    {
                        if (dtLotList_A.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtLotList_A.Rows.Count; i++)
                            {
                                DataRow newLotRow = dt_LOT.NewRow();
                                newLotRow["LOTID"] = dtLotList_A.Rows[i]["LOTID"].ToString();
                                if (cboTrgtEquipment_C.IsEnabled == false || cboTrgtEquipment_A.IsEnabled == false)
                                {
                                    newLotRow["RSV_EQSGID"] = Util.GetCondition(cboTrgtEquiptmentSegment);
                                    newLotRow["RSV_EQPTID"] = null;
                                }
                                else
                                {
                                    newLotRow["RSV_EQSGID"] = null;
                                    newLotRow["RSV_EQPTID"] = Util.GetCondition(cboTrgtEquipment_A);
                                }

                                dt_LOT.Rows.Add(newLotRow);
                            }
                        }
                    }
                }
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SLID_MATC_GR", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        SearchGroupList();

                        int idx = _util.GetDataGridRowIndex(dgGroupList, "SLID_MATC_GR_NO", Util.GetCondition(txtGroupNo));

                        if (idx >= 0)
                        {
                            DataTableConverter.SetValue(dgGroupList.Rows[idx].DataItem, "CHK", true);

                            //row 색 바꾸기
                            dgGroupList.SelectedIndex = idx;
                            // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                            dgGroupList.CurrentCell = dgGroupList.GetCell(idx, dgGroupList.Columns.Count - 1);
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        #endregion

        #region GROUP 선택시 항목 값 설정 : SetGroupContent()
        /// <summary>
        /// Group List 항목 선택 시 상세 내용에 값 설정
        /// </summary>
        /// <param name="drv"></param>
        private void SetGroupContent(DataRowView drv)
        {
            ClearControl();
            SetControlModify();

            txtGroupNo.Text = drv["SLID_MATC_GR_NO"].ToString();
            cboDetailGroupType.SelectedValue = drv["SLID_MATC_GR_TYPE"].ToString();
            popSetCAProdID.SelectedText = drv["CA_PRODID"].ToString();
            popSetANProdID.SelectedText = drv["AN_PRODID"].ToString();
        

            txtMgrId.Text = drv["MGR_USERID"].ToString();
            txtMgrName.Text = drv["MGR_USERNAME"].ToString();
            txtRemark.Text = drv["REMARKS"].ToString();

        //SetTrgtArea();
            string[] TrgtAreaID = drv["TRGT_AREAID"].ToString().Split(',');
            if (TrgtAreaID.Length > 0)
            {
                cboTrgtArea.SelectedValue = TrgtAreaID[0].ToString();
                sTrgt_Area = TrgtAreaID[0].ToString();
            }

            //SetTrgtLine();

            string[] TrgtEqsgid = drv["TRGT_EQSGID"].ToString().Split(',');
            if (TrgtEqsgid.Length > 0)
            {
                cboTrgtEquiptmentSegment.SelectedValue = TrgtEqsgid[0].ToString();
                sTrgt_Eqsgid = TrgtEqsgid[0].ToString();
            }

            //목적설비(양극)
            string[] TrgtEqptid = drv["RSV_EQPTID"].ToString().Split(',');

            if (string.IsNullOrEmpty(drv["RSV_EQPTID"].ToString()))
            {
                cboTrgtEquipment_C.IsEnabled = false;
                cboTrgtEquipment_C.IsEnabled = false;
            }
            else
            {
                //SetTrgtEquipment_C();
                if (TrgtEqptid.Length > 0)
                {
                    cboTrgtEquipment_C.SelectedValue = TrgtEqptid[0].ToString();
                    cboTrgtEquipment_A.SelectedValue = TrgtEqptid[1].ToString();

                    sTrgt_Equipment_C = TrgtEqptid[0].ToString();
                    sTrgt_Equipment_A = TrgtEqptid[1].ToString();
                }


            }


            SearchLotList(drv["CA_PRODID"].ToString(), "C");
            SearchLotList(drv["AN_PRODID"].ToString(), "A");


         

        
        }
        #endregion
        
        #region GROUP에 대한 LOT 추가 _ 신규입력시 : AddGroupLot()
        /// <summary>
        /// Lot 추가
        /// </summary>
        private void AddGroupLot(string polarity)
        {
            //이미 조회된 LOT과 추가할 LOT의 제품 정보 비교(하나의 그룹은  하나의 제품정보만 매핑되어야함)
            if (polarity == "C")
            {
                if (sC_PRODID != string.Empty)
                {
                    if (popSetCAProdID.SelectedValue.ToString() != sC_PRODID)
                    {
                        // 그룹 유형을 선택하세요
                        object[] parameters1 = new object[3];
                        parameters1[0] = ObjectDic.Instance.GetObjectName("양극 제품ID");
                        parameters1[1] = popSetCAProdID.SelectedValue.ToString();
                        parameters1[2] = sC_PRODID;

                        Util.MessageValidation("SFU8491", parameters1);
                        return;
                    }

                }
            }
            else
            {
                if (sA_PRODID != string.Empty)
                {
                    if (popSetANProdID.SelectedValue.ToString() != sA_PRODID)
                    {
                        // 그룹 유형을 선택하세요
                        object[] parameters2 = new object[3];
                        parameters2[0] = ObjectDic.Instance.GetObjectName("음극 제품ID");
                        parameters2[1] = popSetANProdID.SelectedValue.ToString();
                        parameters2[2] = sA_PRODID;

                        Util.MessageValidation("SFU8491", parameters2);
                        return;
                    }

                }
            }

            COM001_148_LOTLIST popupLotList = new COM001_148_LOTLIST { FrameOperation = FrameOperation };
            object[] parameters = new object[3];
            parameters[0] = cboDetailGroupType.SelectedValue.ToString();
            if (polarity == "C")
            {
                parameters[1] = popSetCAProdID.SelectedValue.ToString();
            }
            else
            {
                parameters[1] = popSetANProdID.SelectedValue.ToString();
            }

            parameters[2] = polarity;
            C1WindowExtension.SetParameters(popupLotList, parameters);

            popupLotList.Closed += popupLotList_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupLotList.ShowModal()));


        }

        #endregion

        #region GROUP에 대한 LOT 저장 및 삭제  : SaveGroupLot()

        /// <summary>
        /// GROUP에 대한 LOT 저장
        /// </summary>
        /// <param name="sActType"></param>
        /// <param name="polarity"></param>
        /// <param name="dt"></param>
        /// <param name="ALLDELETEYN"></param>
        private void SaveGroupLot(string sActType, string polarity, DataTable dt, string ALLDELETEYN)
        {

            try
            {
                ShowLoadingIndicator();

                DataTable dtLot = new DataTable("IN_LOT");
                dtLot.Columns.Add("LOTID", typeof(string));
                dtLot.Columns.Add("RSV_EQSGID", typeof(string));
                dtLot.Columns.Add("RSV_EQPTID", typeof(string));


                DataTable dtData = new DataTable("IN_DATA");
                dtData.Columns.Add("ACTID", typeof(string));
                dtData.Columns.Add("SLID_MATC_GR_NO", typeof(string));
                dtData.Columns.Add("USERID", typeof(string));
                dtData.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtData.NewRow();
                dr["ACTID"] = sActType;
                dr["SLID_MATC_GR_NO"] = Util.GetCondition(txtGroupNo);
                dr["USERID"] = LoginInfo.USERID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtData.Rows.Add(dr);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr_Lot = dtLot.NewRow();
                    dr_Lot["LOTID"] = dt.Rows[i]["LOTID"];
                    dr_Lot["RSV_EQSGID"] = sTrgt_Eqsgid;
                    if (polarity == "C")
                    {
                        dr_Lot["RSV_EQPTID"] = sTrgt_Equipment_C;
                    }
                    else
                    {
                        dr_Lot["RSV_EQPTID"] = sTrgt_Equipment_A;
                    }
                 

                    dtLot.Rows.Add(dr_Lot);
                }



                DataSet ds = new DataSet();
                ds.Tables.Add(dtData);
                ds.Tables.Add(dtLot);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SLID_MATC_GR_LOT", "IN_DATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        // 그룹삭제여부 : Y(그룹삭제 클릭시) 
                        if (ALLDELETEYN == "N")
                        {
                            SearchLotList(popSetCAProdID.SelectedText.ToString(), "C");
                            SearchLotList(popSetANProdID.SelectedText.ToString(), "A");

                            DataTable dtLotList_C = DataTableConverter.Convert(dgLotList.ItemsSource);
                            DataTable dtLotList_A = DataTableConverter.Convert(dgLotList_A.ItemsSource);
                            if (dtLotList_C.Rows.Count > 0 || dtLotList_A.Rows.Count > 0)
                            {
                                ModifyGroup("Y");
                            }
                            else
                            {
                                DeleteGroup();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, ds);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        #endregion

        #region  GROUP, 제품에 해당되는 Lot 조회  : SearchLotList()
        /// <summary>
        /// 그룹, 제품에 해당되는 Lot 조회
        /// </summary>
        /// <param name="sProdId"></param>
        private void SearchLotList(string sProdId, string polarity)
        {
            try
            {
                DataTable dt = new DataTable("RQSTDT");
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("SLID_MATC_GR_NO", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                DataRow dr = dt.NewRow();
                dr["PRODID"] = sProdId;
                dr["SLID_MATC_GR_NO"] = Util.GetCondition(txtGroupNo);
                dr["AREAID"] =  LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLID_MATC_GR_LOT_INFO_BY_PROD_ELEC", "RQSTDT", "RSLTDT", dt);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (polarity == "C")
                    {
                        Util.GridSetData(dgLotList, dtResult, FrameOperation, true);
                    

                    }
                    else
                    {
                        Util.GridSetData(dgLotList_A, dtResult, FrameOperation, true);
                       
                    }

                }
                else
                {
                    if (polarity == "C")
                    {
                        Util.gridClear(dgLotList);
                    }
                    else
                    {
                        Util.gridClear(dgLotList_A);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        #endregion

        #region  사용자 팝업 닫기 : GetUserWindow()
        /// <summary>
        /// User 조회 팝업
        /// </summary>
        /// <param name="tb"></param>
        private void GetUserWindow(TextBox tb)
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = tb.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        #endregion

        #region 저장 및 수정 Validation : ValidationSaveGroup()
        /// <summary>
        /// 저장 전 Validation
        /// </summary>
        private bool ValidationSaveGroup()
        {
            if (string.IsNullOrEmpty(txtGroupNo.Text.ToString())) // 신규버전
            {
                if (string.IsNullOrEmpty(cboDetailGroupType.SelectedValue?.ToString()))
                {
                    // 그룹 유형을 선택하세요
                    object[] parameters = new object[1];
                    parameters[0] = ObjectDic.Instance.GetObjectName("그룹유형");

                    Util.MessageValidation("SFU4925", parameters);
                    return false;
                }

                if (string.IsNullOrEmpty(sC_PRODID))
                {
                    // 양극 제품ID를 선택해주세요.
                    object[] parameters = new object[1];
                    parameters[0] = ObjectDic.Instance.GetObjectName("양극 제품ID");

                    Util.MessageValidation("SFU4925", parameters);
                    return false;
                }

                if (string.IsNullOrEmpty(sA_PRODID))
                {
                    // 음극 제품ID를 선택해주세요.
                    object[] parameters = new object[1];
                    parameters[0] = ObjectDic.Instance.GetObjectName("음극 제품ID");

                    Util.MessageValidation("SFU4925", parameters);
                    return false;
                }
                //양극 목적지 설비 정보
                if (SetAreaType() == "E" && cboTrgtEquipment_C.IsEnabled && (cboTrgtEquipment_C.SelectedValue == null || cboTrgtEquipment_C.SelectedValue.ToString() == "SELECT"))
                {
                    // 목적지 정보가 없습니다.
                    Util.MessageValidation("SFU7024");
                    return false;
                }
                if (string.IsNullOrEmpty(txtMgrId.Text.ToString()))
                {
                    // 담당자를 입력 하세요.
                    Util.MessageValidation("SFU4011");
                    return false;
                }
            }
            else //수정버전
            {
                // 담당자 : {%1} 와(과) 수정자 : {%2} 가 틀리면 삭제 및 수정할 수 없습니다.
                if (txtMgrId.Text.ToString() != LoginInfo.USERID)
                {
                    object[] parameters = new object[3];
                    parameters[0] = txtMgrName.Text;
                    parameters[1] = LoginInfo.USERNAME;
                    parameters[2] = ObjectDic.Instance.GetObjectName("수정");

                    Util.MessageValidation("SFU8495", parameters);
                    return false;
                }
            }
    

            return true;
        }

        #endregion

        #region 삭제 Validation : ValidationDeleteGroup()
        /// <summary>
        /// 그룹 삭제 전 Validation
        /// </summary>
        private bool ValidationDeleteGroup()
        {

            string sWipLot = string.Empty;
            //그룹에 속한 LOT 중에 WAIT 상태가 아닌 LOT 확인     
            DataTable dtLotList_C = DataTableConverter.Convert(dgLotList.ItemsSource);
            if (dtLotList_C.Rows.Count > 0)
            {
                for (int i = 0; i < dtLotList_C.Rows.Count; i++)
                {
                    if (dtLotList_C.Rows[i]["WIPSTAT"].ToString() != "WAIT")
                    {
                        if (sWipLot.Length < 1)
                            sWipLot = dtLotList_C.Rows[i]["LOTID"].ToString();
                        else
                            sWipLot = sWipLot + " , " + Util.NVC(dtLotList_C.Rows[i]["LOTID"]);
                    }
                }
            }
            DataTable dtLotList_A = DataTableConverter.Convert(dgLotList_A.ItemsSource);

            if (dtLotList_A.Rows.Count > 0)
            {
                for (int i = 0; i < dtLotList_A.Rows.Count; i++)
                {
                    if (dtLotList_A.Rows[i]["WIPSTAT"].ToString() != "WAIT")
                    {
                        if (sWipLot.Length < 1)
                            sWipLot = dtLotList_A.Rows[i]["LOTID"].ToString();
                        else
                            sWipLot = sWipLot + " , " + Util.NVC(dtLotList_A.Rows[i]["LOTID"]);
                    }
                }
            }
            if (sWipLot != string.Empty)
            {
                //대기 상태가 아닌 LOT이 존재하여 삭제 할 수 없습니다.
                object[] parameters = new object[1];
                parameters[0] = sWipLot;

                Util.MessageValidation("SFU8492", parameters);
                return false;
            }
            // 담당자 : {%1} 와(과) 수정자 : {%2} 가 틀리면 삭제 및 수정할 수 없습니다.
            if (txtMgrId.Text.ToString() != LoginInfo.USERID)
            {
                object[] parameters = new object[3];
                parameters[0] = txtMgrName.Text;
                parameters[1] = LoginInfo.USERNAME;
                parameters[2] = ObjectDic.Instance.GetObjectName("삭제");

                Util.MessageValidation("SFU8495", parameters);
                return false;
            }
            return true;
        }


        #endregion

        #region 목적지 동정보 조회 : SetTrgtArea()
        /// <summary>
        /// 목적지 동 조회
        /// </summary>
        private void SetTrgtArea()
        {
            try
            {
                if (SetAreaType().Equals("E"))
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID_FROM", typeof(string));
                    RQSTDT.Columns.Add("FROM_AREAID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SHOPID_FROM"] = LoginInfo.CFG_SHOP_ID;
                    dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_RELATION_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult != null)
                    {
                        cboTrgtArea.DisplayMemberPath = "CBO_NAME";
                        cboTrgtArea.SelectedValuePath = "CBO_CODE";
                        cboTrgtArea.ItemsSource = dtResult.Copy().AsDataView();
                        cboTrgtArea.SelectedIndex = 0;
                    }
                }
                else
                {
                    DataTable dtTmp = ((DataView)cboTrgtArea.ItemsSource).ToTable();
                    if (!dtTmp.Columns.Contains("LOGIS_TRF_TYPE_CODE"))
                        dtTmp.Columns.Add("LOGIS_TRF_TYPE_CODE", typeof(string));

                    for (int i = 0; i < dtTmp.Rows.Count; i++)
                    {
                        dtTmp.Rows[0]["LOGIS_TRF_TYPE_CODE"] = "AUTO";
                    }
                    cboTrgtArea.DisplayMemberPath = "CBO_NAME";
                    cboTrgtArea.SelectedValuePath = "CBO_CODE";
                    cboTrgtArea.ItemsSource = dtTmp.Copy().AsDataView();
                    cboTrgtArea.SelectedValue = LoginInfo.CFG_AREA_ID;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    
        #region 목적지 라인 조회 : SetTrgtLine()
        /// <summary>
        /// 목적지 라인 조회
        /// </summary>
        private void SetTrgtLine()
        {
            try
            {
                cboTrgtEquiptmentSegment.ItemsSource = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROD_GROUP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.NOTCHING;
                dr["AREAID"] = Util.NVC(cboTrgtArea.SelectedValue);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_PROCID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    dr = dtResult.NewRow();
                    dr["CBO_NAME"] = "-SELECT-";
                    dr["CBO_CODE"] = "SELECT";
                    dtResult.Rows.InsertAt(dr, 0);

                    cboTrgtEquiptmentSegment.DisplayMemberPath = "CBO_NAME";
                    cboTrgtEquiptmentSegment.SelectedValuePath = "CBO_CODE";

                    cboTrgtEquiptmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);
                }
                else
                {
                    cboTrgtEquiptmentSegment.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 양극 목적지 설비 조회 : SetTrgtEquipment_C()
        /// <summary>
        /// 목적지 설비 조회
        /// </summary>
        private void SetTrgtEquipment_C()
        {
            try
            {
                cboTrgtEquipment_C.ItemsSource = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboTrgtArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboTrgtEquiptmentSegment.SelectedValue);
                dr["PROCID"] = Process.NOTCHING;
                dr["ELTR_TYPE_CODE"] = "C";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    dr = dtResult.NewRow();
                    dr["CBO_NAME"] = "-SELECT-";
                    dr["CBO_CODE"] = "SELECT";
                    dtResult.Rows.InsertAt(dr, 0);

                    cboTrgtEquipment_C.DisplayMemberPath = "CBO_NAME";
                    cboTrgtEquipment_C.SelectedValuePath = "CBO_CODE";
                    cboTrgtEquipment_C.ItemsSource = dtResult.Copy().AsDataView();
                    cboTrgtEquipment_C.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 음극 목적지 설비 조회 : SetTrgtEquipment_A()
        /// <summary>
        /// 목적지 설비 조회
        /// </summary>
        private void SetTrgtEquipment_A()
        {
            try
            {
                cboTrgtEquipment_A.ItemsSource = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboTrgtArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboTrgtEquiptmentSegment.SelectedValue);
                dr["PROCID"] = Process.NOTCHING;
                dr["ELTR_TYPE_CODE"] ="A";
              
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    dr = dtResult.NewRow();
                    dr["CBO_NAME"] = "-SELECT-";
                    dr["CBO_CODE"] = "SELECT";
                    dtResult.Rows.InsertAt(dr, 0);

                    cboTrgtEquipment_A.DisplayMemberPath = "CBO_NAME";
                    cboTrgtEquipment_A.SelectedValuePath = "CBO_CODE";
                    cboTrgtEquipment_A.ItemsSource = dtResult.Copy().AsDataView();
                    cboTrgtEquipment_A.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 현재 동 Type 조회 (E:전극, A:조립) : SetAreaType()
        /// <summary>
        /// 동 Type > E : 전극, A : 조립
        /// </summary>
        private string SetAreaType()
        {
            try
            {
                string _AreaType = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_AREA_TYPE_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _AreaType = dtResult.Rows[0]["AREA_TYPE_CODE"].ToString();
                }

                return _AreaType;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }



        #endregion

        #region LoadingIndicato  : ShowLoadingIndicator(), HideLoadingIndicator()

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Collapsed)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #endregion

        private void cboTrgtEquiptmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(cboTrgtEquipment_A.IsEnabled != false || cboTrgtEquipment_C.IsEnabled != false)
            {
                SetTrgtEquipment_A();
                SetTrgtEquipment_C();
            }
           
        }
    }
}
