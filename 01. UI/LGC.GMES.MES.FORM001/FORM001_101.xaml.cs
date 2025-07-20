/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2018.04.20  이상훈  C20180330_49835 오창.소형 부문 한계불량률 관리 항목 추가에 따른 GMES 시스템 개선 요청 건
  2022.11.22  이주홍  C20220915-000045 출하가능여부 더블클릭시 LOT별 GQMS 검사이력 조회 화면 띄우고 해당 LOTID로 조회하도록 함




 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_101 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _util = new Util();
        public FORM001_101()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            InitGrid();
            SetEvent();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo(); 

            //_combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[]{LoginInfo.CFG_AREA_ID}, cbChild: new C1ComboBox[] {cboEquipment_Search }, sCase: "LINE_CP");
            //_combo.SetCombo(cboEquipment_Search, CommonCombo.ComboStatus.SELECT, cbParent: new C1ComboBox[] { cboLine }, sFilter: new string[] { _PROCID }, sCase: "cboEquipment");
            //_combo.SetCombo(cboWorkType_Search, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "PACK_WRK_TYPE_CODE1" }, sCase: "COMMCODE_WITHOUT_CODE");

            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { string.Empty, _PROCID }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            //_combo.SetCombo(cboInBox, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "INBOX_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
            //_combo.SetCombo(cboWorkType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "PACK_WRK_TYPE_CODE1" }, sCase: "COMMCODE_WITHOUT_CODE");

            //SetGridColumnCombo(dgInbox, "PRDT_GRD_CODE", "PRDT_GRD_CODE");
            //SetGridColumnCombo(dgInbox, "PRDT_GRD_LEVEL", "PRDT_GRD_LEVEL");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
          //  txtLotID.Text = "AD2QF077";
            dtpDateFrom.SelectedDateTime = DateTime.Today.AddDays(1 - DateTime.Today.Day);
        }

        /// <summary>
        /// C20180330_49835 화면 출력 대상 제어
        /// </summary>
        private void InitGrid()
        {
            //오창 소형일 때만 보여줌
            if (!LoginInfo.CFG_SHOP_ID.Equals("A010"))
            {
                dgSearchResult.Columns["CHRT_LOTID"].Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }        

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }



        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name == "NCR_CNT"
                    && Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["JUDG_FLAG"].Index).Value) == "F")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (e.Cell.Column.Name == "LOTID_VIEW")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (e.Cell.Column.Name == "RESULT")
                    {
                        if (Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["RESULT_YN"].Index).Value) == "N")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }

                    }
                    else if (e.Cell.Column.Name == "MES_HOLD_FLAG"
                    && Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["MES_HOLD_FLAG"].Index).Value) == "HOLD"
                    && Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["ISEDITABLE"].Index).Value) == "Y")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }

                   
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : DA_PRD_SEL_INPALLET_FM
        /// </summary>
        private void Search()
        {
            try
            {
                if (txtLotID.Text.Length < 5)
                {
                    //SFU4074  LOTID 5자리 이상 입력시 조회 가능합니다.
                    Util.MessageValidation("SFU4074",new object[] {5});
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                dgSearchResult.Columns["TESLA_RESULT"].Visibility = System.Windows.Visibility.Collapsed;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRJ_NAME", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                {
                    dr["LOTID"] = txtLotID.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtProdID.Text))
                {
                    dr["PRODID"] = txtProdID.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtProject.Text))
                {
                    dr["PRJ_NAME"] = txtProject.Text;
                }               
                RQSTDT.Rows.Add(dr);
                string bizName = string.Empty;

                //오창 소형과 나머지 법인 분기 처리
                if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
                {
                    bizName = "DA_PRD_SEL_QMS_INSP_LIST_CKO";
                }
                else
                {
                    bizName = "DA_PRD_SEL_QMS_INSP_LIST";
                }

                new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgSearchResult, searchResult, FrameOperation, true);
                        string[] sColumnName = new string[] { "LOTID", "LOTID_VIEW", "TESLA_RESULT", "PRJT_NAME", "PRODID", "LOTTYPE", "RESULT", "INSP_NAME", "HOLD_FLAG" };
                        _util.SetDataGridMergeExtensionCol(dgSearchResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI); //VERTICALHIERARCHI);

                        if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
                        {
                            for (int i = 0; i < searchResult.Rows.Count; i++)
                            {
                                if ((searchResult.Rows[i]["TESLA_FLAG"]).Equals("Y"))
                                {      //TESLA 모델 여부 판단후 테슬라 컬럼 Visible
                                    dgSearchResult.Columns["TESLA_RESULT"].Visibility = System.Windows.Visibility.Visible;
                                    return;
                                }
                            }
                        }

                     
                        
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
              );              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
               // loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

     

        #endregion

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                    return;

                if (datagrid.CurrentColumn.Name == "NCR_CNT")
                {
                    FORM001_101_NCR_HIST NCR = new FORM001_101_NCR_HIST();
                    NCR.FrameOperation = this.FrameOperation;

                    object[] parameters = new object[10];
                    parameters[0] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTID"].Index).Value); //변경대상
                    parameters[1] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PRJT_NAME"].Index).Value);
                    parameters[2] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PRODID"].Index).Value);
                    parameters[3] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTTYPE"].Index).Value);
                    parameters[4] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["RESULT"].Index).Value);
                    parameters[5] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["ELTR_LOTID"].Index).Value);
                    parameters[6] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["INSP_NAME"].Index).Value);
                    parameters[7] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["INSP_ID"].Index).Value);
                    parameters[8] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["JUDG_FLAG"].Index).Value);
                    parameters[9] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["ISEDITABLE"].Index).Value);
                    C1WindowExtension.SetParameters(NCR, parameters);

                    grdMain.Children.Add(NCR);
                    NCR.BringToFront();
                }

                else if (datagrid.CurrentColumn.Name == "MES_HOLD_FLAG" 
                    && Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["ISEDITABLE"].Index).Value) == "Y"
                    && datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["MES_HOLD_FLAG"].Index).Text == "HOLD")
                {

                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        Util.MessageValidation("실행 권한이 없습니다. 담당자에게 문의하세요");  /*실행 권한이 없습니다. 담당자에게 문의하세요.*/
                        return;
                    }
                    // SFU4046  HOLD 해제 하시겠습니까?
                    Util.MessageConfirm("SFU4046", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            string sLotId = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTID"].Index).Value); //변경대상
                            string sInspId = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["INSP_ID"].Index).Value);

                            DataTable RQSTDT = new DataTable();
                            RQSTDT.Columns.Add("LANGID", typeof(string));
                            RQSTDT.Columns.Add("LOTID", typeof(string));
                            RQSTDT.Columns.Add("INSP_ID", typeof(string));
                            RQSTDT.Columns.Add("MES_UNHOLD_DATE", typeof(string));
                            RQSTDT.Columns.Add("MES_UNHOLD_USERID", typeof(string));
                            RQSTDT.Columns.Add("MES_HOLD_FLAG", typeof(string));

                            DataRow dr = RQSTDT.NewRow();
                            dr["LANGID"] = LoginInfo.LANGID;
                            dr["LOTID"] = sLotId;
                            dr["INSP_ID"] = sInspId;
                            dr["MES_UNHOLD_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                            dr["MES_UNHOLD_USERID"] = LoginInfo.USERID;
                            if (LoginInfo.CFG_SHOP_ID == "A010")
                            {
                                dr["MES_HOLD_FLAG"] = "P";
                            }
                            else
                            {
                                dr["MES_HOLD_FLAG"] = "Y";
                            }

                            RQSTDT.Rows.Add(dr);

                            new ClientProxy().ExecuteService("DA_PRD_UPD_MES_UNHOLD", "RQSTDT", null, RQSTDT, (searchResult, searchException) =>
                            {
                                try
                                {
                                    if (searchException != null)
                                    {
                                        Util.MessageException(searchException);
                                        return;
                                    }
                                    Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.              
                                    Search();
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);

                                }
                                finally
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                }

                            });
                        }
                    });
                    
                }
                if (datagrid.CurrentColumn.Name == "LOTID_VIEW")
                {
                    
                    CMM_ASSY_RETUBING_LOT_INPUT RETUBING = new CMM_ASSY_RETUBING_LOT_INPUT();

                    RETUBING.FrameOperation = this.FrameOperation;

                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTID"].Index).Value); //변경대상
                    C1WindowExtension.SetParameters(RETUBING, parameters);

                    grdMain.Children.Add(RETUBING);
                    RETUBING.BringToFront();
                }

				// 2022.11.22 이주홍
				if (datagrid.CurrentColumn.Name == "RESULT") // 출하가능여부 컬럼 더블클릭
				{
					string lotid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTID"].Index).Value); // 해당 ROW의 LOTID
					this.FrameOperation.OpenMenu("SFU010160351", true, lotid); // LOT별 GQMS 검사이력 조회 화면 열리고 LOTID 전달
				}

			}
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
