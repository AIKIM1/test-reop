/*************************************************************************************
 Created Date : 2018.04.12
      Creator : 오화백
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.04.12  DEVELOPER : Initial Created.
   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;

using Application = System.Windows.Application;
namespace LGC.GMES.MES.COM001
{
    public partial class COM001_227 : UserControl, IWorkArea
    {

        Util _Util = new Util();
        DataTable dtAllDataTable = null;
        DataTable dtAllinboxTable = null;
        private Double _MAXCELL_QTY = 0;
        private string _MIX_LOT_TYPE = string.Empty;
        private string _LOTTYPE = string.Empty;
        private string _LOTID_RT = string.Empty;
        private string _CTNR_ID = string.Empty;
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        #region Declaration & Constructor 
        public COM001_227()
        {
            this.Loaded += UserControl_Loaded;
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void Initialize()
        {

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

        }
        /// <summary>
        /// 콥보박스 셋팅
        /// </summary>
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
          
            #region 대차 생성 콤보 셋팅
            //동
            C1ComboBox[] cboAreaChild_Create = { cboProcessCreate };
            _combo.SetCombo(cboAreaCreate, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild_Create, sCase: "AREA");
            cboAreaCreate.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboProcessParent_Create = { cboAreaCreate };
            C1ComboBox[] cboProcessChild_Create = { cboLineCreate, cboInboxTyple };
            _combo.SetCombo(cboProcessCreate, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbChild: cboProcessChild_Create, cbParent: cboProcessParent_Create);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent_Create = { cboAreaCreate, cboProcessCreate };
            _combo.SetCombo(cboLineCreate, CommonCombo.ComboStatus.SELECT, sCase: "PROCESS_EQUIPMENT", cbParent: cboEquipmentSegmentParent_Create);
            cboLineCreate.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //품질유형
            String[] sFilterQLTY_Create = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboMtrlTylpeCreate, CommonCombo.ComboStatus.SELECT, sFilter: sFilterQLTY_Create, sCase: "COMMCODES");

            //INBOX 유형
            C1ComboBox[] cboInboxTypeParent = { cboAreaCreate, cboProcessCreate };
            _combo.SetCombo(cboInboxTyple, CommonCombo.ComboStatus.SELECT, cbParent: cboInboxTypeParent, sCase: "cboInboxType");
            
            //요청자 셋팅
            txtReqUserCreate.Text = LoginInfo.USERNAME;
            txtReqUserCreate.Tag = LoginInfo.USERID;

            #endregion

            #region 대차 삭제 콤보 셋팅

            //동
             _combo.SetCombo(cboAreaDelete, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
             cboAreaDelete.SelectedValue = LoginInfo.CFG_AREA_ID;

            //요청자 셋팅
            txtReqUserDelete.Text = LoginInfo.USERNAME;
            txtReqUserDelete.Tag = LoginInfo.USERID;

            #endregion

            #region 대차 복원 콤보 셋팅

            //동
            _combo.SetCombo(cboAreaRestore, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
            cboAreaRestore.SelectedValue = LoginInfo.CFG_AREA_ID;

            //요청자 셋팅
            txtReqUserRestore.Text = LoginInfo.USERNAME;
            txtReqUserRestore.Tag = LoginInfo.USERID;

            #endregion

            #region INBOX 변경 콤보 셋팅

            //동
            _combo.SetCombo(cboAreaChange, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
            cboAreaChange.SelectedValue = LoginInfo.CFG_AREA_ID;

            //요청자 셋팅
            txtReqUserChange.Text = LoginInfo.USERNAME;
            txtReqUserChange.Tag = LoginInfo.USERID;

            #endregion

        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreate);
            listAuth.Add(btnInputLotID_RTCreate);
            listAuth.Add(btnDeleteCreate);
            listAuth.Add(btnInboxPrintCreate);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //생성대차 조회
            GetCartInfoCreate("NEW");
            //콤보셋팅
            InitCombo();
            SetEvent();
        }
        #endregion

        #region 대차 생성

        #region [품질유형 변경 cboMtrlTylpeCreate_SelectedValueChanged]

        private void cboMtrlTylpeCreate_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboMtrlTylpeCreate.SelectedIndex != 0)
            {
                Reset(false);
                DataTable TBCtnr_Create = DataTableConverter.Convert(dgCtnrInfoCreate.ItemsSource);
                DataRow row = null;
                row = TBCtnr_Create.NewRow();
                row["CTNR_ID"] = "[NEW]";

                if (cboMtrlTylpeCreate.SelectedValue.ToString() == "G")
                {
                    row["WIP_QLTY_TYPE_NAME"] = ObjectDic.Instance.GetObjectName("양품");
                  
                }
                else
                {
                    row["WIP_QLTY_TYPE_NAME"] = ObjectDic.Instance.GetObjectName("불량");
                    btnInboxPrintCreate.IsEnabled = false;
                }

                row["WIP_QLTY_TYPE_CODE"] = cboMtrlTylpeCreate.SelectedValue.ToString();
                TBCtnr_Create.Rows.Add(row);
                Util.GridSetData(dgCtnrInfoCreate, TBCtnr_Create, FrameOperation, false);
                //품질유형에 대한 스프레드 변경
                DefectColumChange(cboMtrlTylpeCreate.SelectedValue.ToString());


            }
            else
            {
                Reset(false);
               
            }
        }


        #endregion

        #region [대차 정보 품질정보 색깔변경 dgCtnrInfoCreate_LoadedCellPresenter]
        private void dgCtnrInfoCreate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_QLTY_TYPE_CODE").ToString() == "G")
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }

                }
            }));
        }

        #endregion

        #region [조립 LOT 적재 btnInputLotID_RTCreate_Click]

        private void btnInputLotID_RTCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputLOTRT()) return;
            {
                COM001_227_ASSYLOT_LOAD popupRunStart = new COM001_227_ASSYLOT_LOAD { FrameOperation = FrameOperation };

                //if (ValidationGridAdd(popupRunStart.Name) == false)
                //    return;
                popupRunStart.DgAssyLot = dgLotID_RTCreate;
                popupRunStart.DgProductionInbox = dgInboxCreate;

                object[] parameters = new object[7];
                parameters[0] = Util.NVC(cboProcessCreate.SelectedValue);
                parameters[1] = Util.NVC(cboProcessCreate.Text);
                parameters[2] = Util.NVC(cboLineCreate.SelectedValue);
                parameters[3] = Util.NVC(cboInboxTyple.SelectedValue);
                parameters[4] = Util.NVC(cboMtrlTylpeCreate.SelectedValue);
                parameters[5] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "PRODID")); // 제품ID
                parameters[6] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "MKT_TYPE_CODE")); //시장유형
                C1WindowExtension.SetParameters(popupRunStart, parameters);

                popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
                grdMain.Children.Add(popupRunStart);
                popupRunStart.BringToFront();
            }
        }
        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            COM001_227_ASSYLOT_LOAD popup = sender as COM001_227_ASSYLOT_LOAD;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

                //MIX LOT 여부
                _MIX_LOT_TYPE = popup.MIX_LOT_TYPE;
                _LOTTYPE = popup._LOTTYPE;
                
                //조립LOT 바인드
                if (dgLotID_RTCreate.Rows.Count == 0)
                {

                    Util.GridSetData(dgLotID_RTCreate, popup.dtOutPutLotRt, FrameOperation, false);
                    GetCartSetting(popup.dtOutPutLotRt);
               }
                else
                {
                    DataTable dt = DataTableConverter.Convert(dgLotID_RTCreate.ItemsSource);
                    dt.Merge(popup.dtOutPutLotRt);
                    Util.GridSetData(dgLotID_RTCreate, dt, FrameOperation, false);
                    GetCartSetting(dt);
                }

                //INBOX 바인드

                if(dtAllDataTable  == null)
                {
                    dtAllDataTable = popup.dtOutPutInbox.Copy();
                    //Util.GridSetData(dgInboxCreate, popup.dtOutPutInbox, FrameOperation, false);
                }
                else
                {
                    dtAllDataTable.Merge(popup.dtOutPutInbox);
                    //Util.GridSetData(dgInboxCreate, dt, FrameOperation, false);
                }
             
            }
            grdMain.Children.Remove(popup);
        }
        #endregion
       
        #region [INBOX 삭제 btnDeleteCreate_Click]
        private void btnDeleteCreate_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInbox = DataTableConverter.Convert(dgInboxCreate.ItemsSource);
            if(cboMtrlTylpeCreate.SelectedValue.ToString() == "G")
            {
                for (int i = 0; i < dtInbox.Rows.Count; i++)
                {
                    if (dtInbox.Rows[i]["CHK"].ToString() == "1")
                    {
                        dtAllDataTable.Select("LOTID_RT = '" + dtInbox.Rows[i]["LOTID_RT"].ToString() + "' And "
                                            + "INBOX_ID = '" + dtInbox.Rows[i]["INBOX_ID"].ToString() + "' And "
                                            + "CAPA_GRD_CODE = '" + dtInbox.Rows[i]["CAPA_GRD_CODE"].ToString() + "'").ToList<DataRow>().ForEach(row => row.Delete());
                    }
                }
            }
            else
            {
                for (int i = 0; i < dtInbox.Rows.Count; i++)
                {
                    if (dtInbox.Rows[i]["CHK"].ToString() == "1")
                    {
                        dtAllDataTable.Select("LOTID_RT = '" + dtInbox.Rows[i]["LOTID_RT"].ToString() + "' And "
                                            + "INBOX_ID_DEF = '" + dtInbox.Rows[i]["INBOX_ID_DEF"].ToString() + "' And "
                                            + "DFCT_RSN_GR_ID = '" + dtInbox.Rows[i]["DFCT_RSN_GR_ID"].ToString() + "' And "
                                            + "CAPA_GRD_CODE = '" + dtInbox.Rows[i]["CAPA_GRD_CODE"].ToString() + "'").ToList<DataRow>().ForEach(row => row.Delete());
                    }
                }

            }
           
            dtInbox.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
            Util.gridClear(dgInboxCreate);
            Util.GridSetData(dgInboxCreate, dtInbox, FrameOperation, false);
            //조립LOT 계산
            if (dtInbox.Rows.Count > 0)
            {
                GetLotID_RT_Setting();
            }
            else //전체 삭제되었을 경우 선택된 조립LOT 전체 삭제
            {
                DataTable DeleteLotID_RT = DataTableConverter.Convert(dgLotID_RTCreate.ItemsSource);
                DeleteLotID_RT.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());



                Util.gridClear(dgLotID_RTCreate);
                Util.GridSetData(dgLotID_RTCreate, DeleteLotID_RT, FrameOperation, false);
                //조립LOT 정보가 없으면 대차 정보 초기화
                if (dgLotID_RTCreate.Rows.Count == 0)
                {
                    Util.gridClear(dgCtnrInfoCreate);
                    DataTable TBCtnr_Create = DataTableConverter.Convert(dgCtnrInfoCreate.ItemsSource);
                    DataRow row = null;
                    row = TBCtnr_Create.NewRow();
                    row["CTNR_ID"] = "[NEW]";

                    if (cboMtrlTylpeCreate.SelectedValue.ToString() == "G")
                    {
                        row["WIP_QLTY_TYPE_NAME"] = ObjectDic.Instance.GetObjectName("양품");
                        btnInboxPrintCreate.IsEnabled = true;
                    }
                    else
                    {
                        row["WIP_QLTY_TYPE_NAME"] = ObjectDic.Instance.GetObjectName("불량");
                        btnInboxPrintCreate.IsEnabled = false;
                    }

                    row["WIP_QLTY_TYPE_CODE"] = cboMtrlTylpeCreate.SelectedValue.ToString();
                    TBCtnr_Create.Rows.Add(row);
                    Util.GridSetData(dgCtnrInfoCreate, TBCtnr_Create, FrameOperation, false);
                }
            }

        }
        #endregion

        #region [조립LOT 선택 dgLotID_RTCreate_MouseLeftButtonUp]
        private void dgLotID_RTCreate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgLotID_RTCreate.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgLotID_RTCreate.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }
            //선택 조립LOT 의 INBOX 정보 BIND
            CHK_Inbox_Bind();
        }
        #endregion

        #region Cell 수량 MAX, MIN  dgInboxCreate_LoadedCellPresenter
        private void dgInboxCreate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name.Equals("WIPQTY"))
            {
                (e.Cell.Column as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = _MAXCELL_QTY;
                (e.Cell.Column as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 1;
            }
        }
        #endregion

        #region Cell 수량 변경시 조립LOT의 Cell 수량, 대차의 Cell 수량을 변경 dgInboxCreate_KeyDown
        private void dgInboxCreate_KeyDown(object sender, KeyEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid grd = (sender as C1.WPF.DataGrid.C1DataGrid);
            if (e.Key == Key.Enter)
            {

                if (grd != null &&
                        grd.CurrentCell != null &&
                        grd.CurrentCell.Column != null && grd.CurrentCell.Column.Name.Equals("WIPQTY"))
                {
                    if (Util.NVC_Int(DataTableConverter.GetValue(dgInboxCreate.Rows[grd.CurrentCell.Row.Index].DataItem, "WIPQTY")) == 0)
                    {
                        DataTableConverter.SetValue(dgInboxCreate.Rows[grd.CurrentCell.Row.Index].DataItem, "WIPQTY", 1);
                    }

                    GetLotID_RT_Setting();
                }
            }
        }

        #endregion

        #region ComboBox 이벤트 cboInboxTyple_SelectedValueChanged, cboProcessCreate_SelectedValueChanged, cboLineCreate_SelectedValueChanged

        /// <summary>
        /// INBOX TYPE
        /// </summary>
        private void cboInboxTyple_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboInboxTyple.SelectedIndex != 0)
            {
                Reset(false);
                //품질정보가 선택되어 있으면
                if (cboMtrlTylpeCreate.SelectedIndex != 0)
                {
                    Util.gridClear(dgCtnrInfoCreate);
                    DataTable TBCtnr_Create = DataTableConverter.Convert(dgCtnrInfoCreate.ItemsSource);
                    DataRow row = null;
                    row = TBCtnr_Create.NewRow();
                    row["CTNR_ID"] = "[NEW]";

                    if (cboMtrlTylpeCreate.SelectedValue.ToString() == "G")
                    {
                        row["WIP_QLTY_TYPE_NAME"] = ObjectDic.Instance.GetObjectName("양품");
                        btnInboxPrintCreate.IsEnabled = true;
                    }
                    else
                    {
                        row["WIP_QLTY_TYPE_NAME"] = ObjectDic.Instance.GetObjectName("불량");
                        btnInboxPrintCreate.IsEnabled = false;
                    }

                    row["WIP_QLTY_TYPE_CODE"] = cboMtrlTylpeCreate.SelectedValue.ToString();
                    TBCtnr_Create.Rows.Add(row);
                    Util.GridSetData(dgCtnrInfoCreate, TBCtnr_Create, FrameOperation, false);
                    SetEqptInboxTypeQTY();
                }
            }
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void cboProcessCreate_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboMtrlTylpeCreate.SelectedIndex = 0;
            Reset(false);
        }

        /// <summary>
        /// 라인
        /// </summary>
        private void cboLineCreate_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboMtrlTylpeCreate.SelectedIndex = 0;
            Reset(false);
        }


        #endregion

        #region 초기화 btnResetCreate_Click
        private void btnResetCreate_Click(object sender, RoutedEventArgs e)
        {
            Reset(true);
        }
        #endregion

        #region 요청자 txtReqUserCreate_KeyDown, btnReqUserCreate_Click, wndUser_Closed
        /// <summary>
        ///  텍스트박스 이벤트
        /// </summary>
        private void txtReqUserCreate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        /// <summary>
        /// 요청자 버튼
        /// </summary>
        private void btnReqUserCreate_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        /// <summary>
        /// 요청자 팝업닫기
        /// </summary>
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtReqUserCreate.Text = wndPerson.USERNAME;
                txtReqUserCreate.Tag = wndPerson.USERID;

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

        #region [대차생성 btnCreate_Click]
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //생성하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4629"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                if (cboMtrlTylpeCreate.SelectedValue.ToString() == "G")
                                {
                                    CreateCtnr();
                                }
                                else
                                {
                                    CreateCtnr_DEFC();
                                }

                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
         #endregion

        #region [대차시트발행 btnPrintCreate_Click]
        //대차시트 발행
        private void btnPrintCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint()) return;

            DataTable dt = PrintCartData();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return;
            }
            // Page수 산출
            int PageCount = dt.Rows.Count % 40 != 0 ? (dt.Rows.Count / 40) + 1 : dt.Rows.Count / 40;
            int start = 0;
            int end = 0;
            DataRow[] dr;

            // Page 수만큼 Pallet List를 채운다
            for (int cnt = 0; cnt < PageCount; cnt++)
            {
                start = (cnt * 40) + 1;
                end = ((cnt + 1) * 40);

                dr = dt.Select("ROWNUM >=" + start + "And ROWNUM <=" + end);
                CartRePrint(dr, cnt + 1);
            }
        }
        #endregion

        #region [태그발행 btnInboxPrintCreate_Click]
        //태그발행
        private void btnInboxPrintCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;

            string processName = cboProcessCreate.Text;
            string modelId = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[0].DataItem, "MODLID")).ToString();
            string projectName = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "PJT")).ToString();
            string marketTypeName = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "MKT_TYPE_NAME")).ToString();

            // 불량 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));     //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            foreach (DataGridRow row in dgInboxCreate.Rows)
            {
                if (row.Type == DataGridRowType.Item &&
                   (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                    DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                {
                    DataRow dr = dtLabelItem.NewRow();
                    dr["LABEL_CODE"] = "LBL0106";
                    dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    dr["ITEM002"] = modelId + "(" + projectName + ") ";
                    dr["ITEM003"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID_RT"));   //assyLotId;
                    dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "WIPQTY")).GetString();
                    dr["ITEM005"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTSHORTNAME"));
                    dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                    dr["ITEM007"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "VISL_INSP_USERNAME"));
                    dr["ITEM008"] = marketTypeName;
                    dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    dr["ITEM010"] = string.Empty;
                    dr["ITEM011"] = string.Empty;
                    dtLabelItem.Rows.Add(dr);

                }
            }
            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            //string portName = dr["PORTNAME"].GetString();
            DataRow drPrintInfo;

            if (!_Util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {

            }
        }
        #endregion

        #region [대차 생성후 체크박스 변경 dgLotID_RTCreate_BeginningEdit]
        private void dgLotID_RTCreate_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type.Equals(DataGridRowType.Top) || e.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Column.Name.Equals("CHK"))
            {
                if (btnCreate.IsEnabled == true)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
        #endregion

        #endregion

        #region 대차 삭제

        #region 대차조회 txtCtnrIdDelete_KeyDown()
        //대차 삭제 : 대차 조회
        private void txtCtnrIdDelete_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (e.Key == Key.Enter)
                {
                    if (txtCtnrIdDelete.Text == string.Empty) return;
                    //Cart 정보
                    GetCartInfo(txtCtnrIdDelete.Text, "D");
                    if (txtCtnrIdDelete.Text != string.Empty)
                    {
                        //조립LOT 정보
                        GetLotId_RT_Info(txtCtnrIdDelete.Text.Trim(), "D");
                        //INBOX 정보
                        Inbox_Info(txtCtnrIdDelete.Text.Trim(), "D");

                        //양품/불량에 따른 스프레드 컬럼 변경
                        DefectColumChangeDelete(Util.NVC(DataTableConverter.GetValue(dgCtnrInfoDelete.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")));
                    }
                    txtCtnrIdDelete.Text = string.Empty;
                    txtCtnrIdDelete.Focus();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 품질정보에 대한 스프레드 색변경 dgCtnrInfoDelete_LoadedCellPresenter()
        private void dgCtnrInfoDelete_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_QLTY_TYPE_CODE").ToString() == "G")
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }

                }
            }));
        }
        #endregion

        #region 삭제 요청자 txtReqUserDelete_KeyDown, btnReqUserDelete_Click, GetUserWindow_Delete
        /// <summary>
        ///  텍스트박스 이벤트
        /// </summary>
        private void txtReqUserDelete_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow_Delete();
            }
        }
        /// <summary>
        /// 요청자 버튼
        /// </summary>
        private void btnReqUserDelete_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_Delete();
        }
        /// <summary>
        /// 요청자 팝업닫기
        /// </summary>
        private void wndUserDelete_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtReqUserDelete.Text = wndPerson.USERNAME;
                txtReqUserDelete.Tag = wndPerson.USERID;

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

        #region 초기화 btnResetDelete_Click
        private void btnResetDelete_Click(object sender, RoutedEventArgs e)
        {
            Reset_Delete();
        }
        #endregion

        #region 대차삭제 btnDelete_Click
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Delete())
                {
                    return;
                }
                //생성하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4630"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DeleteCtnr();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region 대차 복원

        #region 대차조회 txtCtnrIdRestore_KeyDown()
        //대차 복원 : 대차 조회
        private void txtCtnrIdRestore_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (e.Key == Key.Enter)
                {
                    if (txtCtnrIdRestore.Text == string.Empty) return;
                    //Cart 정보
                    GetDeleteCartInfo(txtCtnrIdRestore.Text);
                    if (txtCtnrIdRestore.Text != string.Empty)
                    {
                        //조립LOT 정보
                        GetLotId_RT_Info(txtCtnrIdRestore.Text.Trim(), "R");
                        //INBOX 정보
                        Inbox_Info(txtCtnrIdRestore.Text.Trim(), "R");

                        //양품/불량에 따른 스프레드 컬럼 변경
                        DefectColumChangeRestore(Util.NVC(DataTableConverter.GetValue(dgCtnrInfoRestore.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")));
                    }
                    txtCtnrIdRestore.Text = string.Empty;
                    txtCtnrIdRestore.Focus();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        
        #region 품질정보에 대한 스프레드 색변경 dgCtnrInfoRestore_LoadedCellPresenter()
        private void dgCtnrInfoRestore_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_QLTY_TYPE_CODE").ToString() == "G")
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }

                }
            }));
        }
        #endregion

        #region 삭제 요청자 txtReqUserDelete_KeyDown, btnReqUserDelete_Click, GetUserWindow_Delete
        /// <summary>
        ///  텍스트박스 이벤트
        /// </summary>
        private void txtReqUserRestore_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow_Restore();
            }
        }
        /// <summary>
        /// 요청자 버튼
        /// </summary>
        private void btnReqUserRestore_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_Restore();
        }
        /// <summary>
        /// 요청자 팝업닫기
        /// </summary>
        private void wndUserRestore_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtReqUserRestore.Text = wndPerson.USERNAME;
                txtReqUserRestore.Tag = wndPerson.USERID;

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

        #region 초기화 btnResetRestore_Click
        private void btnResetRestore_Click(object sender, RoutedEventArgs e)
        {
            Reset_Restore();
        }
        #endregion

        #region 대차복원 btnRestore_Click
        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Restore())
                {
                    return;
                }
                //생성하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4631"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                RestoreCtnr();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region 대차 INBOX 변경

        #region 대차조회 txtCtnrIdChange_KeyDown()
        //대차 INBOX 변경 : 대차 조회
        private void txtCtnrIdChange_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (e.Key == Key.Enter)
                {
                    if (txtCtnrIdChange.Text == string.Empty) return;

                    Reset_Change();
                    //Cart 정보
                    GetCartInfo(txtCtnrIdChange.Text, "C");
                    if (txtCtnrIdChange.Text != string.Empty)
                    {
                        //조립LOT 정보
                        GetLotId_RT_Info(txtCtnrIdChange.Text.Trim(), "C");
                        //INBOX 정보
                        Inbox_Info(txtCtnrIdChange.Text.Trim(), "C");

                        //양품/불량에 따른 스프레드 컬럼 변경
                        DefectColumChangeChange(Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")));
                    }
                    txtCtnrIdChange.Text = string.Empty;
                    txtCtnrIdChange.Focus();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 품질정보에 대한 스프레드 색변경 dgCtnrInfoChange_LoadedCellPresenter()
        private void dgCtnrInfoChange_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_QLTY_TYPE_CODE").ToString() == "G")
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }

                }
            }));
        }
        #endregion

        #region 변경요청자 요청자 txtReqUserChange_KeyDown, btnReqUserChange_Click, wndUserChange_Closed

        /// <summary>
        ///  텍스트박스 이벤트
        /// </summary>
        private void txtReqUserChange_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow_Change();
            }
        }
        /// <summary>
        /// 요청자 버튼
        /// </summary>
        private void btnReqUserChange_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_Change();
        }

        /// <summary>
        /// 요청자 팝업닫기
        /// </summary>
        private void wndUserChange_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtReqUserChange.Text = wndPerson.USERNAME;
                txtReqUserChange.Tag = wndPerson.USERID;

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

        #region [조립LOT 선택 dgLotID_RTChange_MouseLeftButtonUp]
        private void dgLotID_RTChange_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgLotID_RTChange.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgLotID_RTChange.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }
            //선택 조립LOT 의 INBOX 정보 BIND
            CHK_Inbox_Bind_Change();
        }
        #endregion
        
        #region [대차 INBOX 추가 btnInput_Click, popupInboxCreate_Select_Closed]

        /// <summary>
        /// INBOX 추가
        /// </summary>
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Inbox_Input_Validation())
                {
                    return;
                }

                int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgLotID_RTChange, "CHK");
                _LOTID_RT = Util.NVC(DataTableConverter.GetValue(dgLotID_RTChange.Rows[idxchk].DataItem, "LOTID_RT"));
                _CTNR_ID = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "CTNR_ID"));
                COM001_227_CREATE_INBOX popupInboxCreate = new COM001_227_CREATE_INBOX();
                popupInboxCreate.FrameOperation = this.FrameOperation;
                object[] parameters = new object[5];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "CTNR_ID"));  // 대차 ID
                if (dgInboxChange.Rows.Count > 0)
                {
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgInboxChange.Rows[0].DataItem, "INBOX_TYPE_CODE"));// INBOX TYPE
                }
                else
                {
                    parameters[1] = string.Empty;            // INBOX TYPE
                }

                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "PROCID"));
                parameters[3] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "AREAID"));
                parameters[4] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")); // 품질정보

                C1WindowExtension.SetParameters(popupInboxCreate, parameters);

                popupInboxCreate.Closed += new EventHandler(popupInboxCreate_Select_Closed);
                grdMain.Children.Add(popupInboxCreate);
                popupInboxCreate.BringToFront();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        /// <summary>
        /// INBOX 추가 팝업 닫기
        /// </summary>
        private void popupInboxCreate_Select_Closed(object sender, EventArgs e)
        {
            COM001_227_CREATE_INBOX popupCreateInbox = sender as COM001_227_CREATE_INBOX;


            if (popupCreateInbox.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                //Cart 정보
                GetCartInfo(txtCtnrIdChange.Text, "C");

                //조립LOT 정보
                GetLotId_RT_Info(txtCtnrIdChange.Text.Trim(), "C");

                //INBOX 정보
                Inbox_Info(txtCtnrIdChange.Text.Trim(), "C");

                for (int i = 0; i < dgLotID_RTCreate.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotID_RTChange.Rows[i].DataItem, "LOTID_RT")) == _LOTID_RT)
                    {
                        DataTableConverter.SetValue(dgLotID_RTCreate.Rows[i].DataItem, "CHK", 1);
                    }
                }
                CHK_Inbox_Bind_Change();

            }
            this.grdMain.Children.Remove(popupCreateInbox);
        }

        #endregion
        
        #region [대차 INBOX 삭제 btnInput_Click, popupInboxCreate_Select_Closed]
        /// <summary>
        /// INBOX 삭제
        /// </summary>
        private void btnDeleteChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationInboxDelete())
                {
                    return;
                }
                //생성하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4632"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DeleteInbox();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion
        
        #region [대차 INBOX 변경 btnChange_Click, popupInboxChange_Closed]

        /// <summary>
        /// INBOX 변경
        /// </summary>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Inbox_Change_Validation())
                {
                    return;
                }

                int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgInboxChange, "CHK");
                _LOTID_RT = Util.NVC(DataTableConverter.GetValue(dgInboxChange.Rows[idxchk].DataItem, "LOTID_RT"));
                _CTNR_ID = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "CTNR_ID"));

                CMM_POLYMER_FORM_INBOX_INFO_CHANGE popupInboxChange = new CMM_POLYMER_FORM_INBOX_INFO_CHANGE();
                popupInboxChange.FrameOperation = this.FrameOperation;

                popupInboxChange.ProcessCall = true;
                popupInboxChange.Ctnr_Create = "Y";

                object[] parameters = new object[4];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "PROCID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "PROCNAME"));
                parameters[2] = string.Empty;
                parameters[3] = _Util.GetDataGridFirstRowBycheck(dgInboxChange, "CHK");

                C1WindowExtension.SetParameters(popupInboxChange, parameters);

                popupInboxChange.Closed += new EventHandler(popupInboxChange_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupInboxChange);
                        popupInboxChange.BringToFront();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        /// <summary>
        /// INBOX 변경 팝업닫기
        /// </summary>
        private void popupInboxChange_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_INBOX_INFO_CHANGE popup = sender as CMM_POLYMER_FORM_INBOX_INFO_CHANGE;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                //Cart 정보
                GetCartInfo(_CTNR_ID, "C");

                //조립LOT 정보
                GetLotId_RT_Info(_CTNR_ID, "C");

                //INBOX 정보
                Inbox_Info(_CTNR_ID, "C");

                for (int i = 0; i < dgLotID_RTCreate.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotID_RTChange.Rows[i].DataItem, "LOTID_RT")) == _LOTID_RT)
                    {
                        DataTableConverter.SetValue(dgLotID_RTCreate.Rows[i].DataItem, "CHK", 1);
                    }
                }
                CHK_Inbox_Bind_Change();

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        #endregion

        #region 초기화 btnResetChange_Click
        /// <summary>
        /// INBOX 초기화
        /// </summary>
        private void btnResetChange_Click(object sender, RoutedEventArgs e)
        {
            Reset_Change();
        }
        #endregion

        #region [태그발행 btnInboxPrintChange_Click]
        /// <summary>
        /// INBOX 태그 발행
        /// </summary>
        private void btnInboxPrintChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint_Change()) return;

            string processName = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "PROCNAME")).ToString();
            string modelId = Util.NVC(DataTableConverter.GetValue(dgInboxChange.Rows[0].DataItem, "MODLID")).ToString();
            string projectName = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "PJT")).ToString();
            string marketTypeName = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoChange.Rows[0].DataItem, "MKT_TYPE_NAME")).ToString();

            // 불량 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));     //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            foreach (DataGridRow row in dgInboxChange.Rows)
            {
                if (row.Type == DataGridRowType.Item &&
                   (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                    DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                {
                    DataRow dr = dtLabelItem.NewRow();
                    dr["LABEL_CODE"] = "LBL0106";
                    dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                    dr["ITEM002"] = modelId + "(" + projectName + ") ";
                    dr["ITEM003"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID_RT"));   //assyLotId;
                    dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "WIPQTY")).GetString();
                    dr["ITEM005"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "EQPTSHORTNAME"));
                    dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                    dr["ITEM007"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "VISL_INSP_USERNAME"));
                    dr["ITEM008"] = marketTypeName;
                    dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                    dr["ITEM010"] = string.Empty;
                    dr["ITEM011"] = string.Empty;
                    dtLabelItem.Rows.Add(dr);

                }
            }
            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            //string portName = dr["PORTNAME"].GetString();
            DataRow drPrintInfo;

            if (!_Util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {

            }
        }
        #endregion


        #endregion

        #endregion

        #region Mehod

        #region 대차 생성

        #region [생성 대차 조회 GetCartInfoCreate(string cstid)] 
        public void GetCartInfoCreate(string cstid)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INFO", "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgCtnrInfoCreate);
                Util.GridSetData(dgCtnrInfoCreate, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조립 LOT 적재 후 대차 리스트 셋팅 GetCartSetting(DataTable dt)]
        public void GetCartSetting(DataTable dt)
        {
            try
            {
                //대차 List 변경
                DataTable dtCtnr = DataTableConverter.Convert(dgCtnrInfoCreate.ItemsSource);

                double SumInbox = 0;
                double SumCell = 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SumInbox = SumInbox + Convert.ToDouble(dt.Rows[i]["INBOX_QTY"].ToString());
                    SumCell = SumCell + Convert.ToDouble(dt.Rows[i]["WIPQTY"].ToString());
                }
                dtCtnr.Rows[0]["PJT"] = dt.Rows[0]["PJT"];
                dtCtnr.Rows[0]["PRODID"] = dt.Rows[0]["PRODID"];
                dtCtnr.Rows[0]["MKT_TYPE_NAME"] = dt.Rows[0]["MKT_TYPE_NAME"];
                dtCtnr.Rows[0]["INBOX_QTY"] = SumInbox;
                dtCtnr.Rows[0]["WIPQTY"] = SumCell;
                dtCtnr.Rows[0]["PROCID"] = cboProcessCreate.SelectedValue.ToString();
                dtCtnr.Rows[0]["AREAID"] = cboAreaCreate.SelectedValue.ToString();
                dtCtnr.Rows[0]["MKT_TYPE_CODE"] = dt.Rows[0]["MKT_TYPE_CODE"];
                Util.GridSetData(dgCtnrInfoCreate, dtCtnr, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 조립 LOT 적재 후 조립 LOT 리스트 셋팅 GetLotID_RT_Setting()
        public void GetLotID_RT_Setting()
        {
            try
            {
                DataTable dtInbox = DataTableConverter.Convert(dgInboxCreate.ItemsSource);
                var summarydata = from row in dtInbox.AsEnumerable()
                                  group row by new
                                  {
                                      LOTID_RT = row.Field<string>("LOTID_RT")
                                  } into grp
                                  select new
                                  {
                                      LOTID_RT = grp.Key.LOTID_RT
                                      ,
                                      WIPQTY = grp.Sum(r => r.Field<int>("WIPQTY"))
                                      ,
                                      INBOXQTY = grp.Count()
                                  };


                DataTable dtSum = new DataTable();
                dtSum.Columns.Add("LOTID_RT", typeof(string));
                dtSum.Columns.Add("WIPQTY", typeof(int));
                dtSum.Columns.Add("INBOX_QTY", typeof(int));

                foreach (var data in summarydata)
                {
                    DataRow nrow = dtSum.NewRow();
                    nrow["LOTID_RT"] = data.LOTID_RT;
                    nrow["WIPQTY"] = data.WIPQTY == 0 ? 1 : data.WIPQTY;
                    nrow["INBOX_QTY"] = data.INBOXQTY;
                    dtSum.Rows.Add(nrow);
                }
                DataTable dtLotIDRT = DataTableConverter.Convert(dgLotID_RTCreate.ItemsSource);

                for (int i = 0; i < dtLotIDRT.Rows.Count; i++)
                {
                    for (int j = 0; j < dtSum.Rows.Count; j++)
                    {
                        if (dtLotIDRT.Rows[i]["LOTID_RT"].ToString() == dtSum.Rows[j]["LOTID_RT"].ToString())
                        {
                            dtLotIDRT.Rows[i]["WIPQTY"] = dtSum.Rows[j]["WIPQTY"];
                            dtLotIDRT.Rows[i]["INBOX_QTY"] = dtSum.Rows[j]["INBOX_QTY"];
                            dtLotIDRT.Rows[i]["INBOX_QTY_DEF"] = dtSum.Rows[j]["INBOX_QTY"];
                        }
                    }
                }

                //Cell 수량이 O이 아닌 데이터만 조회
                dtLotIDRT.Select("WIPQTY = 0").ToList<DataRow>().ForEach(row => row.Delete());

                Util.gridClear(dgLotID_RTCreate);
                Util.GridSetData(dgLotID_RTCreate, dtLotIDRT, FrameOperation, false);
                //대차 정보 계산
                GetCartSetting(dtLotIDRT);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 조립 LOT 리스트 선택시 INBOX 정보 조회 CHK_Inbox_Bind()
        public void CHK_Inbox_Bind()
        {
            DataTable dtBindInbox = new DataTable();
            dtBindInbox = dtAllDataTable.Clone();

            for (int i = 0; i < dgLotID_RTCreate.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgLotID_RTCreate.Rows[i].DataItem, "CHK").ToString() == "1")
                {
                    DataRow[] drInbox = dtAllDataTable.Select("LOTID_RT ='" + DataTableConverter.GetValue(dgLotID_RTCreate.Rows[i].DataItem, "LOTID_RT").ToString() + "'");
                    foreach (DataRow dr in drInbox)
                    {
                      
                        DataRow drBindInBox = dtBindInbox.NewRow();
                        drBindInBox["CHK"] = 0;
                        drBindInBox["LOTID_RT"] = dr["LOTID_RT"];
                        drBindInBox["INBOX_ID"] = dr["INBOX_ID"];
                        drBindInBox["INBOX_ID_DEF"] = dr["INBOX_ID_DEF"];
                        drBindInBox["INBOX_TYPE_NAME"] = dr["INBOX_TYPE_NAME"];
                        drBindInBox["DFCT_RSN_GR_NAME"] = dr["DFCT_RSN_GR_NAME"];
                        drBindInBox["DFCT_RSN_GR_ID"] = dr["DFCT_RSN_GR_ID"];
                        drBindInBox["CAPA_GRD_CODE"] = dr["CAPA_GRD_CODE"];
                        drBindInBox["WIPQTY"] = dr["WIPQTY"];
                        drBindInBox["FORM_WRK_TYPE_CODE"] = dr["FORM_WRK_TYPE_CODE"];
                        drBindInBox["RESNGR_ABBR_CODE"] = dr["RESNGR_ABBR_CODE"];
                        drBindInBox["MODLID"] = dr["MODLID"];
                        drBindInBox["CALDATE"] = dr["CALDATE"];
                        drBindInBox["SHIFT_NAME"] = dr["SHIFT_NAME"];
                        drBindInBox["EQPTSHORTNAME"] = dr["EQPTSHORTNAME"];
                        drBindInBox["VISL_INSP_USERNAME"] = dr["VISL_INSP_USERNAME"];
                        dtBindInbox.Rows.Add(drBindInBox);
                    }

                }
            }
            Util.GridSetData(dgInboxCreate, dtBindInbox, FrameOperation, false);

        }

        #endregion

        #region 생성 초기화 Reset(bool Chk)
        private void Reset(bool Chk)
        {
            //초기화 버튼 클릭시
            if (Chk)
            {
                //대차 정보 초기화
                Util.gridClear(dgCtnrInfoCreate);
                //조립LOT 정보 초기화
                Util.gridClear(dgLotID_RTCreate);
                //INBOX 정보 초기화
                Util.gridClear(dgInboxCreate);
                //INBOX BInd DataTable 초기화
                dtAllDataTable = null;
                //ERP 체크 해제
                chkERP.IsChecked = false;
                //비고 초기화
                txtNoteCreate.Text = string.Empty;
                txtNoteCreate.IsEnabled = true;
                //요청자 초기화
                txtReqUserCreate.Text = string.Empty;
                txtReqUserCreate.Tag = null;
                //요청자 활성화
                txtReqUserCreate.IsEnabled = true;
                btnReqUserCreate.IsEnabled = true;

                //공정 초기화
                cboProcessCreate.SelectedIndex = 0;
                cboProcessCreate.IsEnabled = true;
                //라인 초기화
                cboLineCreate.SelectedIndex = 0;
                cboLineCreate.IsEnabled = true;
                //품질유형 초기화
                cboMtrlTylpeCreate.SelectedIndex = 0;
                cboMtrlTylpeCreate.IsEnabled = true;
                //INBOX 유형 초기화
                cboInboxTyple.SelectedIndex = 0;
                cboInboxTyple.IsEnabled = true;
                //태그발행 초기화
                btnInboxPrintCreate.IsEnabled = false;
                //대차시트 초기화
                btnPrintCreate.IsEnabled = false;
                //생성버튼 초기화
                btnCreate.IsEnabled = true;

                //삭제 활성화 
                btnDeleteCreate.IsEnabled = false;

                //요청자 셋팅
                txtReqUserCreate.Text = LoginInfo.USERNAME;
                txtReqUserCreate.Tag = LoginInfo.USERID;
                //조립LOT 적재 활성화 
                btnInputLotID_RTCreate.IsEnabled = true;
            }
            else
            {
                //대차 정보 초기화
                Util.gridClear(dgCtnrInfoCreate);
                //조립LOT 정보 초기화
                Util.gridClear(dgLotID_RTCreate);
                //INBOX 정보 초기화
                Util.gridClear(dgInboxCreate);
                //INBOX BInd DataTable 초기화
                dtAllDataTable = null;
                //ERP 체크 해제
                chkERP.IsChecked = false;
                //비고 초기화
                txtNoteCreate.Text = string.Empty;
                //요청자 초기화
                txtReqUserCreate.Text = string.Empty;
                txtReqUserCreate.Tag = null;

                //요청자 셋팅
                txtReqUserCreate.Text = LoginInfo.USERNAME;
                txtReqUserCreate.Tag = LoginInfo.USERID;

            }

        }

        #endregion

        #region 품질정보에 대한 컬럼변경 DefectColumChange(string Qlty_Type)
        public void DefectColumChange(string Qlty_Type)
        {

            if (Qlty_Type == "G")
            {
                //양품INBOX 스프레드
                dgInboxCreate.Columns["INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInboxCreate.Columns["INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInboxCreate.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInboxCreate.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명

                btnInboxPrintCreate.IsEnabled = true;

            }
            else
            {
                //양품INBOX 스프레드
                dgInboxCreate.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;           // INBOX ID
                dgInboxCreate.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInboxCreate.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;    // INBOX 유형
                dgInboxCreate.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명

                btnInboxPrintCreate.IsEnabled = false;
            }
       }



        #endregion

        #region INBOX Type에 대한 Cell  수량 SetEqptInboxTypeQTY()
        private void SetEqptInboxTypeQTY()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = cboProcessCreate.SelectedValue.ToString();
                newRow["INBOX_TYPE_CODE"] = cboInboxTyple.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_TYPE_QTY", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _MAXCELL_QTY = Convert.ToDouble(dtResult.Rows[0]["INBOX_LOAD_QTY"].ToString());

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 요청자 팝업 GetUserWindow()
        /// <summary>
        /// 요청자
        /// </summary>
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtReqUserCreate.Text;
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

        #region[[Validation]
        /// <summary>
        /// 조립LOT 적재 Validation
        /// </summary>
        private bool ValidateInputLOTRT()
        {

            if (cboProcessCreate.SelectedIndex == 0)
            {
                // 공정을 선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboLineCreate.SelectedIndex == 0)
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboMtrlTylpeCreate.SelectedIndex == 0)
            {
                // 품질유형을 선택하세요
                Util.MessageValidation("SFU4633");
                return false;
            }

            if (cboInboxTyple.SelectedIndex == 0)
            {
                // INBOX TYPE 정보를 선택하세요
                Util.MessageValidation("SFU4005");
                return false;
            }



            return true;
        }

        /// <summary>
        /// 팝업 실행여부
        /// </summary>
        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 대차 생성 Validation
        /// </summary>
         private bool Validation()
        {

            if (string.IsNullOrWhiteSpace(txtReqUserCreate.Text) || txtReqUserCreate.Tag == null)
            {
                Util.MessageValidation("SFU4151"); //의뢰자를 선택하세요
                return false;
            }

            if (dgInboxCreate.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4470"); //INBOX 정보가 없습니다.
                return false;
            }

            return true;
        }


        /// <summary>
        /// 라벨 Validation
        /// </summary>
        private bool ValidationPrint()
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgInboxCreate, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                         where t.Field<string>("LABELID") == "LBL0106"
                         select t).ToList();

            if (!query.Any())
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            return true;
        }

        /// <summary>
        /// 대차시트 Validation
        /// </summary>
        private bool ValidationCartRePrint()
        {

            if (dgCtnrInfoCreate.Rows.Count == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }

        #endregion
        
        #region 양품 대차 생성 CreateCtnr()
        //양품대차
        private void CreateCtnr()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string)); //동
            inDataTable.Columns.Add("PROCID", typeof(string)); //공정
            inDataTable.Columns.Add("EQSGID", typeof(string)); //라인
            inDataTable.Columns.Add("PRODID", typeof(string)); //제품
            inDataTable.Columns.Add("LOTTYPE", typeof(string)); //LOT 유형
            inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string)); //
            inDataTable.Columns.Add("MIX_LOT_TYPE_CODE", typeof(string));


            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["USERID"] = txtReqUserCreate.Tag;
            row["AREAID"] = cboAreaCreate.SelectedValue.ToString();
            row["PROCID"] = cboProcessCreate.SelectedValue.ToString();
            row["EQSGID"] = cboLineCreate.SelectedValue.ToString();
            row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "PRODID"));
            row["LOTTYPE"] = _LOTTYPE;
            row["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "MKT_TYPE_CODE"));
            row["MIX_LOT_TYPE_CODE"] = _MIX_LOT_TYPE;
            inDataTable.Rows.Add(row);

            //INBOX
            DataTable inInBox = inData.Tables.Add("INBOX");
            inInBox.Columns.Add("ASSY_LOTID", typeof(string));
            inInBox.Columns.Add("GRD_CODE", typeof(string));
            inInBox.Columns.Add("ACTQTY", typeof(Decimal));
            inInBox.Columns.Add("INBOX_TYPE_CODE", typeof(string));
            inInBox.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));

            for (int i = 0; i < dgInboxCreate.Rows.Count; i++)
            {
                row = inInBox.NewRow();
                row["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "LOTID_RT"));
                row["GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "CAPA_GRD_CODE"));
                row["ACTQTY"] = Util.NVC_Int(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "WIPQTY"));
                row["INBOX_TYPE_CODE"] = cboInboxTyple.SelectedValue.ToString();
                row["FORM_WRK_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "FORM_WRK_TYPE_CODE"));
                inInBox.Rows.Add(row);
            }
            try
            {
                //양품대차 생성
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_STOCK", "INDATA,INBOX", "OUTLOT,OUTCTNR", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //생성후 셋팅
                            AfterCreate();
                            //대차조회
                            GetCartInfoCreate(Result.Tables["OUTCTNR"].Rows[0]["CTNR_ID"].ToString());
                            //INBOX 정보
                            Inbox_Info(Result.Tables["OUTCTNR"].Rows[0]["CTNR_ID"].ToString());
                        }
                    });
                    return;
                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_CREATE_STOCK", ex.Message, ex.ToString());

            }
        }

        #endregion

        #region 불량 대차 생성 CreateCtnr_DEFC()
        //불량대차
        private void CreateCtnr_DEFC()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string)); //동
            inDataTable.Columns.Add("PROCID", typeof(string)); //공정
            inDataTable.Columns.Add("EQSGID", typeof(string)); //라인
            inDataTable.Columns.Add("PRODID", typeof(string)); //제품
            inDataTable.Columns.Add("LOTTYPE", typeof(string)); //LOT 유형
            inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string)); //


            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["USERID"] = txtReqUserCreate.Tag;
            row["AREAID"] = cboAreaCreate.SelectedValue.ToString();
            row["PROCID"] = cboProcessCreate.SelectedValue.ToString();
            row["EQSGID"] = cboLineCreate.SelectedValue.ToString();
            row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "PRODID"));
            row["LOTTYPE"] = _LOTTYPE;
            row["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "MKT_TYPE_CODE"));
            inDataTable.Rows.Add(row);

            //INGRD
            DataTable inInBox = inData.Tables.Add("INGRD");
            inInBox.Columns.Add("ASSY_LOTID", typeof(string));
            inInBox.Columns.Add("RESNGRID", typeof(string));
            inInBox.Columns.Add("CAPA_GRD_CODE", typeof(string));
            inInBox.Columns.Add("RESNQTY", typeof(Decimal));
            inInBox.Columns.Add("RESNGR_ABBR_CODE", typeof(string));
            inInBox.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));

            for (int i = 0; i < dgInboxCreate.Rows.Count; i++)
            {
                row = inInBox.NewRow();
                row["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "LOTID_RT"));
                row["RESNGRID"] = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "DFCT_RSN_GR_ID"));
                row["CAPA_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "CAPA_GRD_CODE"));
                row["RESNQTY"] = Util.NVC_Int(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "WIPQTY"));
                row["RESNGR_ABBR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "RESNGR_ABBR_CODE"));
                row["FORM_WRK_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInboxCreate.Rows[i].DataItem, "FORM_WRK_TYPE_CODE"));
                inInBox.Rows.Add(row);
            }
            try
            {
                //불량대차 생성
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_STOCK_DFCT", "INDATA,INGRD", "OUTLOT,OUTCTNR", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //생성후 셋팅
                            AfterCreate();
                            //대차조회
                            GetCartInfoCreate(Result.Tables["OUTCTNR"].Rows[0]["CTNR_ID"].ToString());
                            //INBOX 정보
                            Inbox_Info(Result.Tables["OUTCTNR"].Rows[0]["CTNR_ID"].ToString());
                        }
                    });
                    return;
                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_SCRAP_MCP", ex.Message, ex.ToString());

            }
        }

        #endregion

        #region 대차 생성 후 버튼 셋팅 AfterCreate()
        //생성후 셋팅
        private void AfterCreate()
        {
            //공정 비활성화
            cboProcessCreate.IsEnabled = false;
            //라인 비활성화
            cboLineCreate.IsEnabled = false;
            //품질유형 비활성화
            cboMtrlTylpeCreate.IsEnabled = false;
            //INBOX 유형 비활성화
            cboInboxTyple.IsEnabled = false;
            //삭제 비활성화 
            btnDeleteCreate.IsEnabled = false;

            //대차시트 활성화
            btnPrintCreate.IsEnabled = true;

            //태그발행 활성화
            btnInboxPrintCreate.IsEnabled = true;

            //생성버튼 비활성화
            btnCreate.IsEnabled = false;

            //요청자 비활성화
            txtReqUserCreate.IsEnabled = false;
            btnReqUserCreate.IsEnabled = false;

            //비고 비활성화
            txtNoteCreate.IsEnabled = false;

            //조립LOT 적재 비활성화 
            btnInputLotID_RTCreate.IsEnabled = false;

        }

        #endregion

        #region 대차 생성 후 INBOX 정보 재조회 Inbox_Info()
        //INBOX 정보
        public void Inbox_Info(string cstid)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INBOX_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.gridClear(dgInboxCreate);
                    Util.GridSetData(dgInboxCreate, dtRslt, FrameOperation, false);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 대차 시트  PrintCartData, CartRePrint, popupCartPrint_Closed
        //대차 프린트
        private DataTable PrintCartData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "CTNR_ID")).ToString();
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        //대차시트 팝업
        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {


            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;
            if (cboMtrlTylpeCreate.SelectedValue.ToString() == "N")
            {
                popupCartPrint.DefectGroupLotYN = "Y";
            }
            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;


            object[] parameters = new object[5];
            parameters[0] = LoginInfo.CFG_PROC_ID;
            parameters[1] = string.Empty;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoCreate.Rows[0].DataItem, "CTNR_ID")).ToString();
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartPrint);
                    popupCartPrint.BringToFront();
                    break;
                }
            }
        }
        //대차시트 팝업 닫기
        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }



        #endregion

        #endregion

        #region 대차 삭제

        #region 삭제할 대차 조회 GetCartInfo(string cstid, string check)
        public void GetCartInfo(string cstid, string check)
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INFO", "INDATA", "OUTDATA", dtRqst);
                if(dtRslt.Rows.Count > 0)
                {
                    if (check == "D") // 대차 삭제
                    {
                        Util.gridClear(dgCtnrInfoDelete);
                        Util.GridSetData(dgCtnrInfoDelete, dtRslt, FrameOperation, false);
                    }
                    else if (check == "C") //INBOX 변경
                    {

                        Util.gridClear(dgCtnrInfoChange);
                        Util.GridSetData(dgCtnrInfoChange, dtRslt, FrameOperation, false);
                    }

                }
                else
                {
                    if (check == "D") // 대차 삭제
                    {
                        txtCtnrIdDelete.Text = string.Empty;
                    }
                    else if (check == "C") //INBOX 변경
                    {
                        txtCtnrIdChange.Text = string.Empty;
                    }
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                   
                    return;
                }
              



            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 삭제할 조립LOT 조회 GetLotId_RT_Info(string cstid, string chk)
        public void GetLotId_RT_Info(string cstid, string chk)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_LOTID_RT_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    if (chk == "D") // 삭제
                    {
                        Util.gridClear(dgLotID_RTDelete);
                        Util.GridSetData(dgLotID_RTDelete, dtRslt, FrameOperation, false);

                    }
                    else if(chk == "R") //복원
                    {
                        Util.gridClear(dgLotID_RTRestore);
                        Util.GridSetData(dgLotID_RTRestore, dtRslt, FrameOperation, false);
                    }
                    else if (chk == "C") //INBOX 변경
                    {
                        Util.gridClear(dgLotID_RTChange);
                        Util.GridSetData(dgLotID_RTChange, dtRslt, FrameOperation, false);
                    }

                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 삭제할 INBOX 조회 Inbox_Info(string cstid, string chk)
        public void Inbox_Info(string cstid, string chk)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_INBOX_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    if (chk == "D")
                    {
                        Util.gridClear(dgInboxDelete);
                        Util.GridSetData(dgInboxDelete, dtRslt, FrameOperation, false);

                    }
                    else if (chk == "R") //복원
                    {
                        Util.gridClear(dgInboxRestore);
                        Util.GridSetData(dgInboxRestore, dtRslt, FrameOperation, false);
                    }
                    else if (chk == "C") //INBOX 변경
                    {
                        //Util.gridClear(dgInboxChange);
                        //Util.GridSetData(dgInboxChange, dtRslt, FrameOperation, false);
                        dtAllinboxTable = new DataTable();
                        dtAllinboxTable = dtRslt.Copy();
 
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 품질정보에 대한  스프레드 변경 DefectColumChangeDelete(string Qlty_Type)
        public void DefectColumChangeDelete(string Qlty_Type)
        {

            if (Qlty_Type == "G")
            {
                //조립LOT 스프레드
                dgLotID_RTDelete.Columns["INBOX_QTY"].Visibility = Visibility.Visible;       // INBOX 수량
                dgLotID_RTDelete.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Collapsed; // 불량그룹 LOT 수
                //INBOX 스프레드
                dgInboxDelete.Columns["INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInboxDelete.Columns["INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInboxDelete.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInboxDelete.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명
            }
            else
            {
                //조립LOT 스프레드
                dgLotID_RTDelete.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;   // INBOX 수량
                dgLotID_RTDelete.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Visible; // 불량그룹 LOT 수
                //INBOX 스프레드
                dgInboxDelete.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInboxDelete.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInboxDelete.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInboxDelete.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명

            }
        }

        #endregion

        #region 요청자 팝업  GetUserWindow_Delete()
        /// <summary>
        /// 요청자
        /// </summary>
        private void GetUserWindow_Delete()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtReqUserDelete.Text;
                wndPerson.Closed += new EventHandler(wndUserDelete_Closed);

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

        #region[Validation]
        /// <summary>
        /// 대차 삭제 Validation
        /// </summary>
        private bool Validation_Delete()
        {

            if (dgCtnrInfoDelete.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4365"); //대차정보가 없습니다.
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtReqUserDelete.Text) || txtReqUserDelete.Tag == null)
            {
                Util.MessageValidation("SFU4151"); //의뢰자를 선택하세요
                return false;
            }


            return true;
        }

        #endregion

        #region 삭제 초기화 Reset_Delete()
        private void Reset_Delete()
        {
            //대차 정보 초기화
            Util.gridClear(dgCtnrInfoDelete);
            //조립LOT 정보 초기화
            Util.gridClear(dgLotID_RTDelete);
            //INBOX 정보 초기화
            Util.gridClear(dgInboxDelete);
            //비고 초기화
            txtNoteDelete.Text = string.Empty;
            //대차 ID 초기화
            txtCtnrIdDelete.Text = string.Empty;
            //요청자 셋팅
            txtReqUserDelete.Text = LoginInfo.USERNAME;
            txtReqUserDelete.Tag = LoginInfo.USERID;


        }

        #endregion

        #region 대차삭제 DeleteCtnr()
        private void DeleteCtnr()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("ACT_USERID", typeof(string));
            inDataTable.Columns.Add("SM_FLAG", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;
            row["ACT_USERID"] = txtReqUserDelete.Tag;
            row["SM_FLAG"] = "Y";
            inDataTable.Rows.Add(row);


            //INCTNR
            DataTable inCtnr = inData.Tables.Add("INCTNR");
            inCtnr.Columns.Add("CTNR_ID", typeof(string));
            inCtnr.Columns.Add("CART_DEL_RSN_CODE", typeof(string));

            for (int i = 0; i < dgCtnrInfoDelete.Rows.Count; i++)
            {
                row = inCtnr.NewRow();
                row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoDelete.Rows[i].DataItem, "CTNR_ID"));
                row["CART_DEL_RSN_CODE"] = "DELETED";
                inCtnr.Rows.Add(row);
            }
            try
            {
                //대차삭제
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_DELETE_CONTAINER", "INDATA,INCTNR", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Reset_Delete();
                        }
                    });
                    return;
                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_DELETE_CONTAINER", ex.Message, ex.ToString());

            }
        }
        #endregion

        #endregion

        #region 대차 복원 

        #region 복원할 대차 조회 GetDeleteCartInfo(string cstid)
        public void GetDeleteCartInfo(string cstid)
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = cstid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_DELETE_CART_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {

                    Util.gridClear(dgCtnrInfoRestore);
                    Util.GridSetData(dgCtnrInfoRestore, dtRslt, FrameOperation, false);

                }
                else
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    txtCtnrIdRestore.Text = string.Empty;
                    return;
                }




            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 품질정보에 대한  스프레드 변경 DefectColumChangeRestore(string Qlty_Type)
        public void DefectColumChangeRestore(string Qlty_Type)
        {

            if (Qlty_Type == "G")
            {
                //조립LOT 스프레드
                dgLotID_RTRestore.Columns["INBOX_QTY"].Visibility = Visibility.Visible;       // INBOX 수량
                dgLotID_RTRestore.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Collapsed; // 불량그룹 LOT 수
                //INBOX 스프레드
                dgInboxRestore.Columns["INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInboxRestore.Columns["INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInboxRestore.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInboxRestore.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명
            }
            else
            {
                //조립LOT 스프레드
                dgLotID_RTRestore.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;   // INBOX 수량
                dgLotID_RTRestore.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Visible; // 불량그룹 LOT 수
                //INBOX 스프레드
                dgInboxRestore.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInboxRestore.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInboxRestore.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInboxRestore.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명

            }
        }

        #endregion

        #region 요청자 팝업 GetUserWindow_Restore()
        /// <summary>
        /// 요청자
        /// </summary>
        private void GetUserWindow_Restore()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtReqUserRestore.Text;
                wndPerson.Closed += new EventHandler(wndUserRestore_Closed);

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

        #region[Validation]
        /// <summary>
        /// 대차 삭제 Validation
        /// </summary>
        private bool Validation_Restore()
        {

            if (dgCtnrInfoRestore.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4365"); //대차정보가 없습니다.
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtReqUserRestore.Text) || txtReqUserRestore.Tag == null)
            {
                Util.MessageValidation("SFU4151"); //의뢰자를 선택하세요
                return false;
            }


            return true;
        }

        #endregion

        #region 복원 초기화  Reset_Restore()
        private void Reset_Restore()
        {
            //대차 정보 초기화
            Util.gridClear(dgCtnrInfoRestore);
            //조립LOT 정보 초기화
            Util.gridClear(dgLotID_RTRestore);
            //INBOX 정보 초기화
            Util.gridClear(dgInboxRestore);
            //비고 초기화
            txtNoteRestore.Text = string.Empty;
            //대차 ID 초기화
            txtCtnrIdRestore.Text = string.Empty;
            //요청자 셋팅
            txtReqUserRestore.Text = LoginInfo.USERNAME;
            txtReqUserRestore.Tag = LoginInfo.USERID;


        }

        #endregion

        #region 대차복원 RestoreCtnr() 
        private void RestoreCtnr()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("ACT_USERID", typeof(string));


            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;
            row["ACT_USERID"] = txtReqUserRestore.Tag;
            inDataTable.Rows.Add(row);


            //INCTNR
            DataTable inCtnr = inData.Tables.Add("INCTNR");
            inCtnr.Columns.Add("CTNR_ID", typeof(string));

            for (int i = 0; i < dgCtnrInfoRestore.Rows.Count; i++)
            {
                row = inCtnr.NewRow();
                row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnrInfoRestore.Rows[i].DataItem, "CTNR_ID"));
                inCtnr.Rows.Add(row);
            }
            try
            {
                //대차삭제
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_REVOKE_CONTAINER", "INDATA,INCTNR", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Reset_Restore();
                        }
                    });
                    return;
                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_DELETE_CONTAINER", ex.Message, ex.ToString());

            }
        }


























        #endregion

        #endregion

        #region 대차 INBOX 변경

        #region 품질정보에 대한  스프레드 변경 DefectColumChangeChange(string Qlty_Type)
        public void DefectColumChangeChange(string Qlty_Type)
        {

            if (Qlty_Type == "G")
            {
                //조립LOT 스프레드
                dgLotID_RTChange.Columns["INBOX_QTY"].Visibility = Visibility.Visible;       // INBOX 수량
                dgLotID_RTChange.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Collapsed; // 불량그룹 LOT 수
                //INBOX 스프레드
                dgInboxChange.Columns["INBOX_ID"].Visibility = Visibility.Visible;           // INBOX ID
                dgInboxChange.Columns["INBOX_ID_DEF"].Visibility = Visibility.Collapsed;     // 불량그룹 LOT
                dgInboxChange.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;    // INBOX 유형
                dgInboxChange.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Collapsed;     // 불량그룹명
                btnInboxPrintChange.IsEnabled = true;
            }
            else
            {
                //조립LOT 스프레드
                dgLotID_RTChange.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;   // INBOX 수량
                dgLotID_RTChange.Columns["INBOX_QTY_DEF"].Visibility = Visibility.Visible; // 불량그룹 LOT 수
                //INBOX 스프레드
                dgInboxChange.Columns["INBOX_ID"].Visibility = Visibility.Collapsed;       // INBOX ID
                dgInboxChange.Columns["INBOX_ID_DEF"].Visibility = Visibility.Visible;     // 불량그룹 LOT
                dgInboxChange.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;// INBOX 유형
                dgInboxChange.Columns["DFCT_RSN_GR_NAME"].Visibility = Visibility.Visible;     // 불량그룹명
                btnInboxPrintChange.IsEnabled = false;

            }
        }

        #endregion

        #region 조립 LOT 리스트 선택시 INBOX 정보 조회 CHK_Inbox_Bind_Change()
        public void CHK_Inbox_Bind_Change()
        {
            DataTable dtBindInbox = new DataTable();
            dtBindInbox = dtAllinboxTable.Clone();

            for (int i = 0; i < dgLotID_RTChange.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgLotID_RTChange.Rows[i].DataItem, "CHK").ToString() == "1")
                {
                    DataRow[] drInbox = dtAllinboxTable.Select("LOTID_RT ='" + DataTableConverter.GetValue(dgLotID_RTChange.Rows[i].DataItem, "LOTID_RT").ToString() + "'");
                    foreach (DataRow dr in drInbox)
                    {

                        DataRow drBindInBox = dtBindInbox.NewRow();
                        drBindInBox["CHK"] = 0;
                        drBindInBox["LOTID_RT"] = dr["LOTID_RT"];
                        drBindInBox["INBOX_ID"] = dr["INBOX_ID"];
                        drBindInBox["INBOX_ID_DEF"] = dr["INBOX_ID_DEF"];
                        drBindInBox["INBOX_TYPE_NAME"] = dr["INBOX_TYPE_NAME"];
                        drBindInBox["DFCT_RSN_GR_NAME"] = dr["DFCT_RSN_GR_NAME"];
                        drBindInBox["DFCT_RSN_GR_ID"] = dr["DFCT_RSN_GR_ID"];
                        drBindInBox["CAPA_GRD_CODE"] = dr["CAPA_GRD_CODE"];
                        drBindInBox["WIPQTY"] = dr["WIPQTY"];
                        drBindInBox["FORM_WRK_TYPE_CODE"] = dr["FORM_WRK_TYPE_CODE"];
                        drBindInBox["RESNGR_ABBR_CODE"] = dr["RESNGR_ABBR_CODE"];
                        drBindInBox["MODLID"] = dr["MODLID"];
                        drBindInBox["CALDATE"] = dr["CALDATE"];
                        drBindInBox["SHFT_NAME"] = dr["SHFT_NAME"];
                        drBindInBox["EQPTSHORTNAME"] = dr["EQPTSHORTNAME"];
                        drBindInBox["VISL_INSP_USERNAME"] = dr["VISL_INSP_USERNAME"];
                        drBindInBox["INBOX_TYPE_CODE"] = dr["INBOX_TYPE_CODE"];
                        drBindInBox["EQSGID"] = dr["EQSGID"];
                        drBindInBox["QLTY_TYPE_CODE"] = dr["QLTY_TYPE_CODE"];
                        dtBindInbox.Rows.Add(drBindInBox);
                    }

                }
            }
            Util.GridSetData(dgInboxChange, dtBindInbox, FrameOperation, false);

        }

        #endregion

        #region 요청자 팝업 GetUserWindow_Change()
        /// <summary>
        /// 요청자
        /// </summary>
        private void GetUserWindow_Change()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtReqUserChange.Text;
                wndPerson.Closed += new EventHandler(wndUserChange_Closed);

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

        #region 대차 INBOX 삭제 DeleteInbox()
        private void DeleteInbox()
        {
            //try
            //{
            //    // DATA Table
            //    DataSet inDataSet = new DataSet();
            //    DataTable inTable = inDataSet.Tables.Add("INDATA");
            //    inTable.Columns.Add("SRCTYPE", typeof(string));
            //    inTable.Columns.Add("IFMODE", typeof(string));
            //    inTable.Columns.Add("EQPTID", typeof(string));
            //    inTable.Columns.Add("USERID", typeof(string));
            //    inTable.Columns.Add("MOD_FLAG", typeof(string));
            //    inTable.Columns.Add("PROCID", typeof(string));
            //    DataTable inLot = inDataSet.Tables.Add("INLOT");
            //    inLot.Columns.Add("PALLETID", typeof(string));

            //    DataRow newRow = inTable.NewRow();
            //    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            //    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            //    newRow["EQPTID"] = _EQPTID;
            //    newRow["USERID"] = LoginInfo.USERID;
            //    newRow["MOD_FLAG"] = "Y";
            //    newRow["PROCID"] = _PROCID;
            //    inTable.Rows.Add(newRow);

            //    DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            //    foreach (DataRow drDel in dr)
            //    {
            //        newRow = inLot.NewRow();
            //        newRow["PALLETID"] = drDel["INBOX_ID"].ToString();
            //        inLot.Rows.Add(newRow);
            //    }

            //    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_PALLET", "INDATA,INLOT", null, (bizResult, bizException) =>
            //    {
            //        try
            //        {


            //            if (bizException != null)
            //            {
            //                Util.MessageException(bizException);
            //                return;
            //            }

            //        }
            //        catch (Exception ex)
            //        {

            //            Util.MessageException(ex);
            //        }
            //    }, inDataSet);
            //}
            //catch (Exception ex)
            //{

            //    Util.MessageException(ex);
            //}
        }

        #endregion

        #region[[Validation]
        /// <summary>
        /// INBOX 투입 Validation
        /// </summary>
        private bool Inbox_Input_Validation()
        {

            if (dgLotID_RTChange.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            int CheckCount = 0;

            for (int i = 0; i < dgLotID_RTChange.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotID_RTChange.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU4634");//선택된 조립LOT이 없습니다.
                return false;
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4635");//조립LOT을 한건만 선택하세요.
                return false;
            }


            return true;
        }

        /// <summary>
        /// INBOX 삭제 팝업
        /// </summary>
        private bool ValidationInboxDelete()
        {

            DataRow[] dr = DataTableConverter.Convert(dgInboxChange.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        /// <summary>
        /// INBOX 변경 팝업 Validation
        /// </summary>

        private bool Inbox_Change_Validation()
        {

            if (dgInboxChange.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            int CheckCount = 0;

            for (int i = 0; i < dgInboxChange.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgInboxChange.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU4636");//선택된 INBOX 정보가 없습니다.
                return false;
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4637");//INBOX를 한건만 선택하세요.
                return false;
            }


            return true;
        }


        /// <summary>
        /// 라벨 Validation
        /// </summary>
        private bool ValidationPrint_Change()
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgInboxChange, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                         where t.Field<string>("LABELID") == "LBL0106"
                         select t).ToList();

            if (!query.Any())
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            return true;
        }

        #endregion

        #region INBOX 변경 초기화 Reset_Change()
        private void Reset_Change()
        {
            //대차 정보 초기화
            Util.gridClear(dgCtnrInfoChange);
            //조립LOT 정보 초기화
            Util.gridClear(dgLotID_RTChange);
            //INBOX 정보 초기화
            Util.gridClear(dgInboxChange);
            //태그발행 초기화
            btnInboxPrintChange.IsEnabled = false;
        }

        #endregion

        #endregion


        #endregion

       
    }
}
