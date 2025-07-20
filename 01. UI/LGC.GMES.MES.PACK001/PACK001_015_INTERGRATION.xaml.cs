/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 장만철
   Decription : 포장및 라벨 발행, 재발행 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.10.08  손우석 같은 제품 다른 라인/라른 사이트 인경우 라인 정보 파라미터 추가
  2020.10.22  강호운         2nd OOCV 공정분리(P5200,P5400) 공정추가
  2021.03.22 염규범 DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG 타입 아웃 이슈 해결로 인한 타입 변경 처리
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_015_INTERGRATION : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtSearchResult; //조회 결과를 담기 위한 DAtaTable
        DataTable dtModelResult;
        DataTable dtResult;
        DataTable dtTextResult;
        System.ComponentModel.BackgroundWorker bkWorker; //bakcground thread

        #region MAIN 포장
        //flag 변수
        bool boxingYn = false;   //true - box 구성중, 미완료 / false - 박스포장 가능
        bool unPackYn = false;   //리스트에서 포장 해재 가능 여부 : true-포장해제 가능 / false-포장해제 불가능       
        bool rePrint = false;    //리스트에서 발행 할것인지 포장완료 후 발행 할것인지 : true-재발행 / false-최초발행
        bool reBoxing = false;   //재포장 여부

        //수량 관련 변수
        int boxLotmax_cnt = 0;   //박스에 담길 lot의 최대 수량
        int boxingLot_idx = 0;   //박스에 담긴 Lot의 수량
        int lotCountReverse = 0; //박스에 담길 lot의 남은 수량

        //box 관련 변수
        string seleted_boxid;
        string boxing_lot;    //포장중인 boxid
        string OKCancel_Desc; //포장완료시 진행 여부에 대한 text   
        string box_prod = ""; //박스의prod : 투입되는 lot의 prod와 비교하기 위한 변수
        string box_eqsg = "";
        string seleted_palletID;
        string model;

        //투입되는 lot 관련 변수
        string lot_prod = string.Empty;     //이전에 투입한 lot의 prodid         
        string lot_proc = string.Empty;     //이전에 투입한 lot의 procid
        string lot_eqsgid = string.Empty;   //이전에 투입한 lot의  eqsgid
        string lot_class_old = string.Empty;//이전에 투입한 lot의 class

        #region 재포장을 위한 포장해제 변수
        string seleted_Box_Prod = string.Empty;    // 재포장을 위한 prodid
        string seleted_Box_Procid = string.Empty;  // 재포장을 위한 procid
        string seleted_Box_Eqptid = string.Empty;  // 재포장을 위한 eqptid
        string seleted_Box_Eqsgid = string.Empty;  // 재포장을 위한 eqsgid
        string seleted_Box_PrdClass = string.Empty;// 재포장을 위한 prdclass
        string seleted_oqc_insp_id = string.Empty; // 재포장을 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string seleted_judg_value = string.Empty;  // 재포장을 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        #endregion

        #region 리스트에서 포장해제를 위한 변수
        string unPack_ProdID = string.Empty;       // 포장해제를 위한 prodid
        string unPack_EqsgID = string.Empty;       // 포장해제를 위한 eqsgid
        string unPack_EqptID = string.Empty;       // 포장해제를 위한 eqptid
        string unPack_ProcID = string.Empty;       // 포장해제를 위한 procid
        string unPack_PrdClasee = string.Empty;    // 포장해제를 위한 prdclass
        string unPack_oqc_insp_id = string.Empty;  // 포장해제를 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        string unPack_judg_value = string.Empty;   // 포장해제를 위해 포장해제시 oqc_insp_id를 초기화 시키기 위한 변수
        #endregion

        private bool blPrintStop = true;
        string label_code2 = "LBL0020";
        string zpl2 = string.Empty;
        string selectBox = string.Empty;
        string selectPallet = string.Empty;

        string combo_prd = string.Empty;
        bool palletYN = false;
        #endregion

        #region 315H
        bool boxingYn_315H = false; //포장중인지 유무
        bool reBoxing_315H = false; //재포장 유무 : box id 조회후 포장진행시(create/packed 모두 가능) - packed인 경우만 이후에 포장해제 진행
        bool unPackYN_315H = false; //포장해제 했는지 유무 : boxid 입력 후 최초 lot 입력시 or lot 삭제시 unpack 하기 위한 용도
        bool palletYN_315H = false; //팔래트에 포장된 box인지 유무

        string boxStat_315H = string.Empty; //box의 stat : created / packed인 경우만 포장진행 - packed인 경우 이후 포장해제 진행

        int boxingLot_idx_315H = 0;
        private bool blPrintStop_315H = true;
        string label_code_315H = "LBL0020";
        string zpl_315H = string.Empty;
        string prdClass_315H = "CMA";

        string combo_prd_315H = string.Empty;

        string lot_prod_315H = string.Empty; //이전에 투입한 lot의 prodid         
        string lot_proc_315H = string.Empty; //이전에 투입한 lot의 procid
        string lot_eqsgid_315H = string.Empty; //이전에 투입한 lot의  eqsgid
        string lot_class_old_315H = string.Empty; //이전에 투입한 lot의 class

        string box_prod_315H = string.Empty; //현재 포장중니 BOX의 PRODID
        string boxing_lot_315H = string.Empty; //현재 포장중인 BOXID

        string seleted_Box_Prod_315H = string.Empty;
        string seleted_Box_Procid_315H = string.Empty;
        string seleted_Box_Eqptid_315H = string.Empty;
        string seleted_Box_Eqsgid_315H = string.Empty;
        string seleted_BOX_LotCNT_315H = string.Empty;
        #endregion


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_015_INTERGRATION()
        {
            InitializeComponent();

            this.Loaded += PPACK001_015_INTERGRATION_Loaded;
        }

       

        #endregion

        #region Initialize
        private void Initialize()
        {
            //날자 초기값 세팅
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜

            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbBoxHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbBoxingWait_cnt_315H.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            #region 312H, 313H, 515H
            dtpDate2.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom2.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
            //dtpDateFrom2.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo2.SelectedDateTime = (DateTime)System.DateTime.Now;

            txtDate2.Text = PrintOutDate(DateTime.Now);  //txtZpl018.Text = PrintOutDate(DateTime.Now);
            //dtpDate_SelectedDateChanged(null, null); //dtp312HDay_ValueChanged(null, null); dtpDate

            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            setTexBox();
            #endregion

            InitCombo();
        }
        #endregion

        #region BOX 포장 MAIN
        #region Event

        #region MAIN EVENT
        private void PPACK001_015_INTERGRATION_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();

                search();

                this.Loaded -= PPACK001_015_INTERGRATION_Loaded;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcMain.SelectedIndex == 1) //315H
            {
                init_315H();
            }
            else if (tcMain.SelectedIndex == 2)
            {
                init_312_313_315_515();
            }
        }
        #endregion

        #region BUTTON EVENT
        private void btnBoxLabel_Click(object sender, RoutedEventArgs e)
        { 
            try
            {
                rePrint = true; //재발행
                if (txtBoxIdR.Text.Length > 0)
                {

                    model = model.Substring(4);

                    if (model == "312H" || model == "313H" || model == "515H" || model == "MOKA" || model == "KANG")
                    {
                        if(selectPallet != null && selectPallet.Length > 0)
                        {
                            set312_313_315_517();
                        }
                        else
                        {
                            labelPrint(sender);
                        }
                    }
                    else
                    {
                        labelPrint(sender);
                    }
                }
                rePrint = false;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        
        private void btnPacCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(txtBoxIdR.Text.Length == 0)
                {
                    return;
                }

                if (!unPackYn)
                {
                    Util.AlertInfo("이미 PALLET에 담긴 BOX 입니다. UPNPACK 할수 없습니다.");
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("정말 포장취소 하시겠습니까?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {  
                        pack_unPack_init(sender);

                        btnPack.Tag = ObjectDic.Instance.GetObjectName("재발행");

                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("포장이 해제 되었습니다.", null, "완료", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                        search();
                    }
                    else
                    {
                        return;
                    }
                }
                   );
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgBoxhistory);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                if (cboArea.SelectedIndex == 0)
                {
                    Util.AlertInfo("동(AREA)를 선택하세요");
                    return;
                }

                search();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }       

        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {
            if (dgBoxLot.ItemsSource != null)
            {
                for (int i = dgBoxLot.Rows.Count; 0 < i; i--)
                {
                    var chkYn = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "CHK");
                    var lot_id = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "LOTID");

                    if (chkYn == null)
                    {
                        dgBoxLot.RemoveRow(i - 1);
                    }
                    else if (Convert.ToBoolean(chkYn))
                    {
                        dgBoxLot.EndNewRow(true);
                        dgBoxLot.RemoveRow(i - 1);
                        boxingLot_idx--;
                        lotCountReverse++;

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse,  "포장중");
                    }
                }

                DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);

                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));
            }
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(dgBoxLot.GetRowCount() == 0)
                {
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("정말 포장리스트를 삭제 하시겠습니까?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        dgBoxLot.ItemsSource = null;
                        boxingLot_idx = 0;
                        lotCountReverse = boxLotmax_cnt;
                        //boxingYn = false;

                        tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                        if (txtBoxId.Text.ToString() == "")
                        {
                            boxLotmax_cnt = 0;
                        }

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
                    }
                    else
                    {
                        return;
                    }
                }
                  );
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장작업전환"))
                {
                    txtcnt.Visibility = Visibility.Visible;
                    btnUnPack.Visibility = Visibility.Hidden;

                    btnSelectCacel.Visibility = Visibility.Visible;
                    btncancel.Visibility = Visibility.Visible;
                    chkBoxId.IsChecked = false;
                    txtBoxId.IsEnabled = true;
                    txtLotId.IsEnabled = true;
                    chkBoxId.Visibility = Visibility.Visible;
                    txtBoxId.IsReadOnly = false;

                    btnPack.Content = ObjectDic.Instance.GetObjectName("포장");
                    btnPack.Tag = "신규";

                    boxingLot_idx = 0;
                    boxLotmax_cnt = 0; // 박싱 가능 수량 세팅 - 정해지면 수정  
                    nbBoxingCnt.Value = 5;
                    lotCountReverse = Convert.ToInt32(nbBoxingCnt.Value);
                    tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";



                    dgBoxLot.ItemsSource = null;
                    (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth =40;
                    txtLotId.IsEnabled = true;

                    boxingYn = false;

                    setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "대기중");

                    txtBoxId.Text = "";
                    txtLotId.Text = "";                   

                    search();
                }
                else
                {
                    if (txtBoxId.Text == "")
                    {
                        return;
                    }

                    if (dgBoxLot.GetRowCount() == 0)
                    {
                        return;
                    }

                    if (boxingYn == true && (boxLotmax_cnt == boxingLot_idx))
                    {
                        OKCancel_Desc = ObjectDic.Instance.GetObjectName("포장완료 하시겠습니까?");
                    }
                    else if (boxLotmax_cnt > boxingLot_idx)
                    {
                        OKCancel_Desc = ObjectDic.Instance.GetObjectName("BOX 포장 수량과 LOT 수량이 일치하지 않습니다.\n포장 완료 하시겠습니까 ?");
                    }

                   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(OKCancel_Desc, null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            boxingEnd(); //포장 완료 함수

                            //reSet();

                            reSet_Last_Stat();


                            //boxingYn = false; //포장완료 또는 포장 대기 flag

                            //control_Init(); //control 초기화

                            //setBoxCnt(5, 0, 5, "대기중");

                            //라벨발행
                            //신규발행, 재발행
                            //labelPrint(sender);

                            Util.AlertInfo("포장완료");
                            search();
                        }
                        else
                        {
                            return;
                        }
                    }
                   );
                }
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnUnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBoxId.Text.Length == 0)
                {
                    return;
                }

                bool UNPACKYN = false;

                if (seleted_palletID != null && seleted_palletID != "")
                {
                    UNPACKYN = false;

                    //box_eqsg = seleted_Pallet_Eqsgid;
                    //box_prod = seleted_Pallet_Prod;

                }
                else
                {
                    UNPACKYN = true;
                }

                if (!UNPACKYN)
                {
                    Util.AlertInfo("PALLET에 담긴 BOX는 포장취소 할수 없습니다.");
                    return;
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("정말 포장취소 하시겠습니까?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                {
                    if (caution_result == MessageBoxResult.OK)
                    {
                        //UNPACK 로직
                        pack_unPack_init(sender);

                        btnPack.Tag = ObjectDic.Instance.GetObjectName("재발행");

                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("포장이 취소 되었습니다.", null, "완료", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                        search();
                    }
                    else
                    {
                        return;
                    }
                }
                   );
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }           
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!boxingYn)
                {
                    reSet();

                    Util.AlertInfo("작업이 초기화 됐습니다.");
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("포장중인 내용이 있습니다. 정말 [작업초기화] 하시겠습니까?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            reSet();
                            Util.AlertInfo("작업이 초기화 됐습니다.");
                        }
                        else
                        {
                            return;
                        }
                    }
              );
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, ObjectDic.Instance.GetObjectName("완료"), MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                reBoxing = true; //재포장
                buttonAccess(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #region GRID EVENT
        private void dgBoxhistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgBoxhistory.Rows.Count == 0 || dgBoxhistory == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBoxhistory.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                if (dgBoxhistory.Rows[currentRow].DataItem == null)
                {
                    return;
                }

                selectBox = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXID").ToString();

                if (DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PALLETID") != null)
                {
                    selectPallet = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PALLETID").ToString();
                }

                //PALLET에 담긴 BOX는 해제 불가능
                if (selectPallet == null || selectPallet == "")
                {
                    unPackYn = true;
                }
                else
                {
                    unPackYn = false;
                }
                
                txtBoxIdR.Text = selectBox;

                unPack_ProdID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PRODID").ToString(); //PALLET의 제품
                unPack_ProcID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PROCID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "PROCID").ToString(); //PALLET의 제품
                unPack_EqptID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "EQPTID").ToString(); //PALLET의 제품
                unPack_EqsgID = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "EQSGID").ToString(); //PALLET의 제품
                
                boxLotmax_cnt = Convert.ToInt32(DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXLOTCNT")); //포장된 수량
                boxingLot_idx = Convert.ToInt32(DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "BOXLOTCNT")); //포장된 수량

                model = DataTableConverter.GetValue(dgBoxhistory.Rows[currentRow].DataItem, "MODEL").ToString(); //PALLET의 모델

            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }            
        }

        private void gridDoubleClickProcess(object sender, MouseButtonEventArgs e, C1.WPF.DataGrid.C1DataGrid grid)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = grid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }

                    if (cell.Column.Name == "BOXID")
                    {
                        PACK001_003_BOXINFO popup = new PACK001_003_BOXINFO();
                        popup.FrameOperation = this.FrameOperation;

                        if (popup != null)
                        {
                            DataTable dtData = new DataTable();
                            dtData.Columns.Add("BOXID", typeof(string));

                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["BOXID"] = cell.Text;

                            dtData.Rows.Add(newRow);

                            //========================================================================
                            object[] Parameters = new object[1];
                            Parameters[0] = dtData;
                            C1WindowExtension.SetParameters(popup, Parameters);
                            //========================================================================

                            //popup.Closed -= popup_Closed;
                            //popup.Closed += popup_Closed;
                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgBoxLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid grid = sender as C1.WPF.DataGrid.C1DataGrid;

                gridDoubleClickProcess(sender, e, grid);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region TEXTBOX EVENT
        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SetBoxkeyDown();
                }
                catch(Exception ex)
                {
                    Util.AlertInfo(ex.Message);
                }
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxId.Text == "") //boxid 가 입력 되지 않았고
                {
                    if ((bool)chkBoxId.IsChecked) // BOXID 체크 박스가 체크 있을경우 BOXID 자동 생성
                    {
                        try
                        {
                            //입력된 lot validation
                            if (!lotValidation_BR()) //if (!lotValidation())
                            {
                                txtLotId.Text = "";
                                return;
                            }

                            //BOXID 자동 생성
                            autoBoxIdCreate();

                            //생성된 boxid의 prod 가져옴.
                            getBoxProd();

                            if(lot_prod != box_prod)
                            {
                                Util.AlertInfo("BOX의 PROD와 LOT의 PROD가 다릅니다.");
                                return;
                            }

                            Util.gridClear(dgBoxLot); //그리드 clear

                            //포장 가능 수량 세팅
                            setBoxLotCnt();

                            //lot을 그리드(dgBoxLot)에 추가
                            addGridLot();

                            // BOX Label 자동발행
                            //labelPrint();

                            //boxing 상태 초기화
                            //boxingStatInit();

                            txtLotId.Text = "";

                            boxingYn = true; //박싱중.
                            //boxingLot_idx++; //box에 담긴 lot 수량 체크
                            //lotCountReverse--;

                            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
                        }
                        catch (Exception ex)
                        {
                            Util.AlertInfo(ex.Message);
                        }
                    }
                    else
                    {
                        Util.AlertInfo("BoxId 먼저 입력하세요");
                    }
                }
                else
                {
                    try
                    {
                        //box의 담길 lot의 수량체크
                        if (boxLotmax_cnt > boxingLot_idx)
                        {
                            if (gridDistinctCheck("lot")) //그리드 중복 체크
                            {
                                //입력된 lotid validation
                                if (!lotValidation_BR()) //if (!lotValidation())
                                {
                                    txtLotId.Text = "";
                                    return;
                                }

                                if (lot_prod != box_prod)
                                {
                                    Util.AlertInfo("BOX의 PROD와 LOT의 PROD가 다릅니다.");
                                    return;
                                }

                                addGridLot();
                                setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");

                                txtLotId.Text = "";
                            }
                        }
                        else
                        {
                            Util.AlertInfo("포장 가능 수량( " + boxLotmax_cnt.ToString() + " )을 넘습니다.");
                        }
                    }
                    catch (Exception ex)
                    {
                        txtLotId.Text = "";
                        Util.AlertInfo(ex.Message);
                    }
                }
            }
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (txtSearchBox.Text.Length == 0)
                    {
                        return;
                    }

                    SearchBox();
                }
                catch (Exception ex)
                {
                    Util.AlertInfo(ex.Message);
                }
            }
        }
        #endregion       

        #region CHECKBOX EVENT       

        private void chkBoxId_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkBoxId.IsChecked)
            {
                if(boxingYn)
                {
                    chkBoxId.IsChecked = false;
                    txtBoxId.Text = boxing_lot;
                    txtLotId.Text = "";

                    return;
                }

                txtBoxId.IsReadOnly = true;
                txtBoxId.Text = "";
            }
            else
            {
                if(boxingYn)
                {
                    chkBoxId.IsChecked = true;
                    return;
                }

                txtBoxId.IsReadOnly = false;
            }
        }
        #endregion

        #region 기타 이벤트
        void popup_Closed(object sender, EventArgs e)
        {

        }
        private void nbBoxingCnt_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt - boxingLot_idx;
            string stat = string.Empty;

            if (boxingYn)
            {
                stat = ObjectDic.Instance.GetObjectName("포장중");
            }
            else
            {
                stat = ObjectDic.Instance.GetObjectName("대기중");
            }

            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, stat);

        }
        #endregion

        #endregion

        #region Mehod
        private void search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["MODLID"] = Util.GetCondition(cboProductModel); 
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);  //dtpDateFrom.SelectedDateTime.ToString();
                dr["TODATE"] = Util.GetCondition(dtpDateTo); //dtpDateTo.SelectedDateTime.ToString();
                dr["BOXID"] = "";
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgBoxhistory.ItemsSource = null;
                txtBoxIdR.Text = "";

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    txtSearchBox.Text = "";
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }      

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = null; // LoginInfo.CFG_EQSG_ID;           
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";

            //동           
            C1ComboBox[] cboAreaChild = { cboProductModel };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild : cboAreaChild);         

            //모델          
            //C1ComboBox[] cboProductModelParent = { cboSHOPID, cboArea, cboEquipmentSegment };
            //C1ComboBox[] cboProductModelChild = { cboProduct };
            //_combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild);

            //모델          
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL_AUTH");

            //제품    
            //C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboAREA_TYPE_CODE, cboPrdtClass };
            //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent);

            //제품코드  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            //getProduct(cboProduct);

            //517H 라벨발행 tab의 제품콤보
            setCombo();

        }

        private void setCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL"] = label_code2; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtModelResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROJECTNAME_PRODID_COMBO", "INDATA", "OUTDATA", RQSTDT);

                cboProduct2.DisplayMemberPath = "CBO_NAME";
                cboProduct2.SelectedValuePath = "CBO_CODE";
                cboProduct2.ItemsSource = DataTableConverter.Convert(dtModelResult); //AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboProduct2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getBoxProd()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));


                DataRow searchCondition = RQSTDT.NewRow();
                searchCondition["BOXID"] = boxing_lot;


                RQSTDT.Rows.Add(searchCondition);

                DataTable dtBoxProd = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BOXCHECK_PROD", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtBoxProd.Rows.Count > 0)
                {
                    box_prod = dtBoxProd.Rows[0]["PRODID"].ToString();
                    box_eqsg = dtBoxProd.Rows[0]["EQSGID"] == null ? LoginInfo.CFG_EQSG_ID : dtBoxProd.Rows[0]["EQSGID"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetBoxkeyDown()
        {
            try
            {
                if (chkBoxId.IsChecked == true && btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장")) //boxid가 체크 되어 있으면 biz에서 validaiton 수행 후  boxid 생성 하므로 로직에서 validation 필요없음.
                {
                    Util.AlertInfo("chekbox를 풀고 boxid를 입력하세요");
                }
                else
                {
                    if (boxingYn)
                    {
                        Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");

                        txtBoxId.Text = boxing_lot;
                        return;
                    }

                    //입력된 boxid validation
                    boxValidation();                   
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setBoxCnt(int max_cnt, int lot_cnt, int lotCountReverse, string boxing_stat)
        {
            if(txtcnt == null)
            {
                return;
            }

            txtcnt.Text = ObjectDic.Instance.GetObjectName(boxing_stat) + " : " + lot_cnt.ToString() + " / " + max_cnt.ToString();
            tbCount.Text = lotCountReverse.ToString();
        }

        //CHECK BOX(chkBoxId ) 가 체크 됐고 LOTID 입력(KEYIN, SCAN)시 BOXID 생성하는 함수
        private void autoBoxIdCreate()
        {
            try
            {
                //boxid 생성 로직
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                RQSTDT.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                RQSTDT.Columns.Add("LOTID", typeof(string));    //투이LOT(처음 LOT)
                RQSTDT.Columns.Add("PROCID", typeof(string));   //ㅍ장공정 ID
                RQSTDT.Columns.Add("PRODID", typeof(string));   //lot의 제품
                RQSTDT.Columns.Add("LOTQTY", typeof(Int32));   //투입 총수량
                RQSTDT.Columns.Add("EQSGID", typeof(string));   //라인ID
                RQSTDT.Columns.Add("USERID", typeof(string));   //사용자ID

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotId.Text;
                dr["PROCID"] = lot_proc; // CMA:P5500, BMA:P9500
                dr["PRODID"] = lot_prod; 
                dr["LOTQTY"] = nbBoxingCnt.Value.ToString(); ; // 화면에서 선택한 수량....나중에 포장시 실제 담긴 수량으로 변경됨.         
                dr["EQSGID"] = lot_eqsgid;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBoxId.Text = dtResult.Rows[0][0].ToString();
                    boxing_lot = txtBoxId.Text.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool lotValidation()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                RQSTDT.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                RQSTDT.Columns.Add("LOTID", typeof(string));    //투이LOT(처음 LOT)               

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.GetCondition(txtLotId);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT", "INDATA", "OUTDATA", RQSTDT);

                //LOT TABLE과 WIP TABLE의 PROD 비교
                if (dtResult.Rows.Count > 0)
                {
                    lot_proc = string.Empty;

                    DataTable dtLOTINFO = new DataTable();
                    dtLOTINFO.TableName = "RQSTDT";
                    dtLOTINFO.Columns.Add("LOTID", typeof(string));    //투입LOT(처음 LOT)               

                    DataRow drLOTINFO = dtLOTINFO.NewRow();
                    drLOTINFO["LOTID"] = Util.GetCondition(txtLotId);

                    dtLOTINFO.Rows.Add(drLOTINFO);

                    DataTable dtLOTINFOResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTWIP_INFO", "INDATA", "OUTDATA", RQSTDT);

                    if (dtLOTINFOResult.Rows.Count > 0)
                    {
                        string lot_class = dtLOTINFOResult.Rows[0]["CLASS"].ToString();
                        string lot_prodtype = dtLOTINFOResult.Rows[0]["PRODTYPE"].ToString();
                        string lot_wiphold = dtLOTINFOResult.Rows[0]["WIPHOLD"].ToString();
                        string lot_wipstat = dtLOTINFOResult.Rows[0]["WIPSTAT"].ToString();
                        string lot_procid = dtLOTINFOResult.Rows[0]["PROCID"].ToString();
                        string lot_prodid = dtLOTINFOResult.Rows[0]["PRODID"].ToString();
                        string lot_eqsg = dtLOTINFOResult.Rows[0]["EQSGID"].ToString();
                        string lot_route = dtLOTINFOResult.Rows[0]["ROUTID"].ToString();

                        if (lot_wipstat == "TERM") // 페기된 lot인지 체크
                        {
                            Util.AlertInfo("폐기된 LOT입니다.");
                            return false;
                        }

                        if (lot_wiphold == "Y") //hold 상태인지 체크
                        {
                            Util.AlertInfo("HOLD LOT입니다.");
                            return false;
                        }

                        if (lot_class_old != null && lot_class_old != "") //이전 투입 제품 타입과 같은지 비교
                        {
                            if (lot_class_old != lot_class)
                            {
                                Util.AlertInfo("이전에 투입한 LOT의 제품 타입과 다릅니다.");
                                return false;
                            }
                        }

                        if (lot_class == "CMA") //제품타입별 포장 가능 공정 체크
                        {
                            if (lot_procid == "P5000" || lot_procid == "P5500" || lot_procid == "P5200" || lot_procid == "P5400")
                            {

                            }
                            else
                            {
                                Util.AlertInfo("포장 불가능한 공정입니다.");
                                return false;
                            }
                        }
                        else if (lot_class == "BMA")
                        {
                            if (lot_procid == "P9000" || lot_procid == "P9500")
                            {

                            }
                            else
                            {
                                Util.AlertInfo("포장 불가능한 공정입니다.");
                                return false;
                            }
                        }
                        else
                        {
                            Util.AlertInfo("포장가능한 제품타입이 아닙니다.");
                            return false;
                        }

                        if (lot_prod != null && lot_prod != "") //이전 투입 lot의 제품과 같은지 비교
                        {
                            if (lot_prod != lot_prodid)
                            {
                                Util.AlertInfo("이전에 투입한 LOT의 제품과 다릅니다.");
                                return false;
                            }
                        }

                        if (lot_eqsgid != null && lot_eqsgid != "") //이전 투입 lot의 라인과 같은지 비교
                        {
                            if (lot_eqsgid != lot_eqsg)
                            {
                                Util.AlertInfo("이전에 투입한 LOT의 라인과 다릅니다.");
                                return false;
                            }
                        }

                        //포장공정의 공정 id 찾기
                        DataTable dtPROC = new DataTable();
                        dtPROC.TableName = "RQSTDT";
                        dtPROC.Columns.Add("ROUTID", typeof(string));
                        //dtPROC.Columns.Add("PROCTYPE", typeof(string));

                        DataRow drPROC = dtPROC.NewRow();
                        drPROC["ROUTID"] = lot_route;
                        //drPROC["PROCTYPE"] = "B"; //포장공정 타입

                        dtPROC.Rows.Add(drPROC);

                        DataTable dtdtPROCResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ENDPROCID_PACK", "INDATA", "OUTDATA", dtPROC);

                        if (dtdtPROCResult == null || dtdtPROCResult.Rows.Count == 0)
                        {
                            Util.AlertInfo("포장공정을 찾을수 없습니다.");
                            return false;
                        }

                        lot_proc = dtdtPROCResult.Rows[0]["PROCID"].ToString();
                        lot_prod = lot_prodid;
                        lot_eqsgid = lot_eqsg;
                        lot_class_old = lot_class;

                        txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid + " : " + lot_proc;

                        return true;
                    }
                    else
                    {
                        Util.AlertInfo("LOT의 포장 가능 정보가 맞지 않아 포장할 수 없습니다.");
                        return false;
                    }
                }
                else
                {
                    Util.AlertInfo("포장 불가능한 LOT입니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private bool lotValidation_BR()
        {
            try
            {
                //lot_proc = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));    //투입LOT(처음 LOT)       
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOX_PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = Util.GetCondition(txtLotId); //LOTID
                dr["BOXTYPE"] = "LOT";
                dr["BOX_PRODID"] = lot_prod == "" ? null : lot_prod;
                dr["PRDT_CLSS"] = lot_class_old == "" ? null : lot_class_old;
                dr["EQSGID"] = lot_eqsgid == "" ? null : lot_eqsgid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT_PALLET", "INDATA", "OUTDATA", RQSTDT);

                //LOT TABLE과 WIP TABLE의 PROD 비교
                if (dtResult.Rows.Count > 0)
                {
                    string lot_class = dtResult.Rows[0]["CLASS"].ToString();
                    string lot_procid = dtResult.Rows[0]["PROCID"].ToString();
                    string lot_prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string lot_eqsg = dtResult.Rows[0]["EQSGID"].ToString();

                    if (getModel(lot_prodid) == "315H" || getModel(lot_prodid) == "MOKA")
                    {
                        set315H(txtLotId.Text, "LOT"); //315H 모델은 별도의 화면(tab 313H CMA) 에서 작업.

                        txtLotId.Text = "";
                        return false;
                    }

                    lot_proc = lot_procid;
                    lot_prod = lot_prodid;
                    lot_eqsgid = lot_eqsg;
                    lot_class_old = lot_class;

                    txtBoxInfo.Text = lot_class_old + " : " + lot_eqsgid + " : " + lot_prodid;

                    return true;
                }
                else
                {
                    Util.AlertInfo("LOT의 포장 가능 정보가 맞지 않아 포장할 수 없습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void boxValidation()
        {
            //이전 포장 작업 유무
            if (boxingYn)
            {
                Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");
                return;
            }

            //입력된 boxid 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] =  txtBoxId.Text;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    Util.AlertInfo("입력한 BOX가 존재하지 않습니다.");
                    return;
                }

                box_prod = dtResult.Rows[0]["PRODID"].ToString(); //box의 prod 담아둠.
                box_eqsg = dtResult.Rows[0]["PACK_EQSGID"] == null ? LoginInfo.CFG_EQSG_ID : dtResult.Rows[0]["PACK_EQSGID"].ToString(); //box의 eqsgid 담아둠.

                if (getModel(box_prod) == "315H" || getModel(box_prod) == "MOKA")
                {
                    if(seleted_palletID.Length == 0)
                    {
                        set315H(txtBoxId.Text, "BOX"); //315H 모델은 별도의 화면(tab 313H CMA) 에서 작업.

                        txtBoxId.Text = "";
                    }
                }

                if (btnPack.Content.ToString() == ObjectDic.Instance.GetObjectName("포장작업전환")) //히스토리 클릭해서 온 경우
                {
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED" && drw["BOXTYPE"].ToString() == "BOX") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT 미정의 정의 되면 수정 필요.
                        {
                            chkBoxId.IsChecked = true;

                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("BOXID", typeof(string));                                                                                                 

                            DataRow dr1 = RQSTDT1.NewRow();
                            dr1["BOXID"] = txtBoxId.Text;

                            RQSTDT1.Rows.Add(dr1);

                            DataTable dtBoxLots = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXLOTS_SEARCH", "INDATA", "OUTDATA", RQSTDT1);

                            txtcnt.Visibility = Visibility.Hidden;
                            btnUnPack.Visibility = Visibility.Visible;
                            btnSelectCacel.Visibility = Visibility.Hidden;
                            btncancel.Visibility = Visibility.Hidden;
                            txtLotId.IsEnabled = false;
                            chkBoxId.IsChecked = false;
                            chkBoxId.Visibility = Visibility.Hidden;
                                             
                            dgBoxLot.ItemsSource = null;
                            dgBoxLot.ItemsSource = DataTableConverter.Convert(dtBoxLots);
                            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dtBoxLots.Rows.Count));

                            //(dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 0;

                        }
                    }
                }
                else if (btnPack.Content.ToString() != ObjectDic.Instance.GetObjectName("포장작업전환"))
                {
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT 미정의 정의 되면 수정 필요.
                        {
                            Util.AlertInfo("작업불가! 이미포장된BOX입니다.");
                            return;
                        }
                    }

                    boxingYn = true; 
                    boxing_lot = txtBoxId.Text.ToString();

                    //boxing 가능 수량 세팅 필요
                    setBoxLotCnt();
                    boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                    lotCountReverse = boxLotmax_cnt;
                    
                }                

                setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string getModel(string prod)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));                                

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = prod;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MODEL", "INDATA", "OUTDATA", RQSTDT);

                if(dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0][0].ToString();
                }
                else
                {
                    return "";
                }           
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private bool boxValidation_accept(string boxid)
        {            
            //입력된 boxid 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = boxid;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    Util.AlertInfo("선택한 BOX가 존재하지 않습니다.");
                    return false;
                }
               
                foreach (DataRow drw in dtResult.Rows)
                {
                    if (drw["BOXSTAT"].ToString() == "CREATED" && drw["BOXTYPE"].ToString() == "BOX") 
                    {
                        Util.AlertInfo("이미 포장해제된 BOX입니다.");
                        return false;
                    }
                }

                return true;                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool gridDistinctCheck(string gubun)
        {
            if (gubun == "box")
            {
                return false;
            }
            else
            {
                DataRowView rowview = null;

                foreach (C1.WPF.DataGrid.DataGridRow row in dgBoxLot.Rows)
                {

                    if (row.DataItem != null)
                    {
                        rowview = row.DataItem as DataRowView;

                        if (rowview["LOTID"].ToString() == txtLotId.Text.ToString())
                        {
                            Util.AlertInfo("이미 포장 리스트에 있는 LOTID입니다.");
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        private void addGridLot()
        {
            if (boxingLot_idx == 0)
            {
                DataTable dtBoxLot = new DataTable();
                dtBoxLot.Columns.Add("CHK", typeof(bool));
                dtBoxLot.Columns.Add("BOXID", typeof(string));
                dtBoxLot.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtBoxLot.NewRow();
                dr["CHK"] = false;
                dr["BOXID"] = Util.GetCondition(txtBoxId);
                dr["LOTID"] = Util.GetCondition(txtLotId);

                dtBoxLot.Rows.Add(dr);

                dgBoxLot.ItemsSource = DataTableConverter.Convert(dtBoxLot);

                boxingYn = true;
            }
            else
            {
                int TotalRow = dgBoxLot.GetRowCount(); //헤더제거

                dgBoxLot.EndNewRow(true);
                DataGridRowAdd(dgBoxLot);

                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "CHK", "false");
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "BOXID", Util.GetCondition(txtBoxId));
                DataTableConverter.SetValue(dgBoxLot.Rows[TotalRow].DataItem, "LOTID", Util.GetCondition(txtLotId));
            }
            boxingLot_idx++;
            lotCountReverse--;

            DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);

            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dt.Rows.Count));
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private void boxingEnd()
        {
            try
            {
                string boxID = Util.GetCondition(txtBoxId);
                int lot_total_qty;
                string rebox_lotbox_ckh = string.Empty;
                string gubun = string.Empty; //box포장인지 lot포장인지 구분

                string eqsg = string.Empty;
                string proc = string.Empty;

                if (reBoxing) //재포장
                {
                    eqsg = seleted_Box_Eqsgid;
                    proc = seleted_Box_Procid;
                }
                else//최초포장
                {
                    eqsg = lot_eqsgid;
                    proc = lot_proc;
                }

                lot_total_qty = dgBoxLot.GetRowCount();

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                INDATA.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                INDATA.Columns.Add("BOXID", typeof(string));    //투이LOT(처음 LOT)
                INDATA.Columns.Add("PROCID", typeof(string));   //공정ID(포장전 마지막 공정) 
                INDATA.Columns.Add("BOXQTY", typeof(string));   //투입 총수량
                INDATA.Columns.Add("EQSGID", typeof(string));   //라인ID
                INDATA.Columns.Add("USERID", typeof(string));   //사용자ID

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = boxID;
                dr["PROCID"] = proc;
                dr["BOXQTY"] = lot_total_qty;
                dr["EQSGID"] = eqsg;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable IN_LOTID = indataSet.Tables.Add("IN_LOTID");
                IN_LOTID.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgBoxLot.GetRowCount(); i++)
                {
                    string sLotId = Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["LOTID"].Index).Value);

                    DataRow inDataDtl = IN_LOTID.NewRow();
                    inDataDtl["LOTID"] = sLotId;
                    IN_LOTID.Rows.Add(inDataDtl);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if(dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count >0)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("포장이 완료 되었습니다.", null, "완료", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                }
                else
                {
                    throw new Exception("포장 작업 실패 BOXING BIZ 확인 하세요.");
                }               
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void labelTest()
        {
            try
            {
                //x,y 가져오기
                DataTable dt = LoginInfo.CFG_SERIAL_PRINT;

                string startX = "0";
                string startY = "0";
                if (dt.Rows.Count > 0)
                {
                    startX = dt.Rows[0]["X"].ToString();
                    startY = dt.Rows[0]["Y"].ToString();
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                //for (int i = 0; i < inData.Rows.Count; i++)
                //{

                dtRqst.Rows[0]["LBCD"] = "LBL0017";
                dtRqst.Rows[0]["PRMK"] = "Z";
                dtRqst.Rows[0]["RESO"] = "203";
                dtRqst.Rows[0]["PRCN"] = "1";
                dtRqst.Rows[0]["MARH"] = startX;
                dtRqst.Rows[0]["MARV"] = startY;               
                dtRqst.Rows[0]["ATTVAL001"] = "REF : 295B93949R__C";//  inData.Rows[i]["MODELID"].ToString();
                dtRqst.Rows[0]["ATTVAL002"] = "966Wh";//  inData.Rows[i]["LOTID"].ToString();
                dtRqst.Rows[0]["ATTVAL003"] = "LOT0000001";//  inData.Rows[i]["WIPQTY"].ToString();
                dtRqst.Rows[0]["ATTVAL004"] = "11111111";//  inData.Rows[i]["RESNNAME"].ToString();
                dtRqst.Rows[0]["ATTVAL005"] = DateTime.Now.ToString("yyyy.MM.dd");
                dtRqst.Rows[0]["ATTVAL006"] = "";//  dtExpected.SelectedDateTime.ToString("yyyy.MM.dd");
                dtRqst.Rows[0]["ATTVAL007"] = "";//  inData.Rows[i]["PERSON"].ToString();
                dtRqst.Rows[0]["ATTVAL008"] = "";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    try
                    {
                        CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(dtRslt.Rows[0]["LABELCD"].ToString());
                        wndPopup.Show();
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void labelPrint(object sender)
        {
            try
            {
                string print_cnt = string.Empty;
                string print_yn = string.Empty;
                string prodID = string.Empty;
                string boxID = string.Empty;
                DataTable dtzpl;

                //재발행 신규 발행 구분 : PRODID 구분해서 뿌려줌.
                if (rePrint) //재발행
                {
                    prodID = unPack_ProdID;
                    boxID = txtBoxIdR.Text;
                }
                else
                {
                    prodID = "";
                    boxID = txtBoxId.Text;
                }

                //zpl 가져오기
                //string sLOTID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY,string sPRODID
                //dtzpl = Util.getZPL_Pack(boxID, null, null, null, "PACK_INBOX", "LBL0020", "N", "1", prodID, null);

                dtzpl = Util.getZPL_Pack(sLOTID: boxID
                                        , sLABEL_TYPE: LABEL_TYPE_CODE.PACK_INBOX
                                        , sLABEL_CODE: "LBL0020"
                                        , sSAMPLE_FLAG: "N"
                                        , sPRN_QTY: "1"
                                        , sPRODID: prodID
                                        );

                if (dtzpl == null || dtzpl.Rows.Count == 0)
                {
                    return;
                }

                string zpl = Util.NVC(dtzpl.Rows[0]["ZPLSTRING"]);
                //라벨 발행
                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(zpl);
                wndPopup.Show();

                //Util.printLabel_Pack(FrameOperation, loadingIndicator, txtPalleyIdR.Text, "PACK", "N", "1");

                if(!rePrint)
                {
                    return;
                }

                //재발행 일 경우 처리 : 기존 발행 정보 확인
                DataTable dtBoxPrintHistory = setBoxResultList();

                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                {
                    return;
                }

                print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")//print 여부 N인경우 Y로 update
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                }

                //Button btn = sender as Button;

                //if (btn.Name == "btnBoxLabel") //재발행
                //{
                //    DataTable dtWipList = getWipList();
                //    string lotId = Util.GetCondition(txtBoxIdR);

                //    Util.printLabel_Pack(FrameOperation, loadingIndicator, lotId, "PROC", "N", "1");

                //    updateTB_SFC_LABEL_PRT_REQ_HIST(lotId, dtWipList.Rows[0]["BOXSEQ"].ToString());

                //    showLabelPrintInfoPopup(lotId, lotId);
                //}
                //else
                //{
                //    string lotId = Util.GetCondition(txtBoxId);

                //    Util.printLabel_Pack(FrameOperation, loadingIndicator, lotId, "PROC", "N", "1"); //라벨 출력

                //    if (btn.Name == "btnPack" && btn.Content.ToString() == "포장")
                //    {
                //        DataTable dtWipList = getWipList();
                //        updateTB_SFC_LABEL_PRT_REQ_HIST(lotId, dtWipList.Rows[0]["BOXSEQ"].ToString()); //이력테이블 update

                //        showLabelPrintInfoPopup(lotId, lotId);
                //    }

                //}


                /* REAL 버전
                                string print_cnt = string.Empty;
                                string print_yn = string.Empty;

                                Util.printLabel_Pack(FrameOperation, loadingIndicator, txtBoxIdR.Text, "PACK", "N", "1");

                                //재발행 일 경우 처리 : 기존 발행 정보 확인
                                DataTable dtBoxPrintHistory = setBoxResultList();

                                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                                {
                                    return;
                                }

                                print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                                print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")//print 여부 N인경우 Y로 update
                                {
                                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                                }
                  */

                //테스트 버전
                //DataTable dtResult = getZPL_Pack(txtBoxIdR.Text, null, null, null, "", "", "N", "1");
                //DataTable dtResult = getZPL_Pack(null, null, null, null, "PACK", "LBL0017", "N", "1"); //AMDAU0068A

                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                //    CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(zpl);
                //    wndPopup.Show();

                //    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }         
        }

        public static DataTable getZPL_Pack(string sBOXID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY)
        {
            DataTable dtResult = null;
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("PRN_QTY", typeof(string));
                INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));


                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBOXID;
                dr["PROCID"] = sPROCID;
                dr["EQPTID"] = sEQPTID;
                dr["EQSGID"] = sEQSGID;
                dr["LABEL_TYPE"] = sLABEL_TYPE;
                dr["LABEL_CODE"] = sLABEL_CODE;
                dr["PRN_QTY"] = sPRN_QTY;
                dr["SAMPLE_FLAG"] = sSAMPLE_FLAG;

                INDATA.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL_BOX", "INDATA", "OUTDATA", INDATA);
                //new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ZPL_BOX", "INDATA", "OUTDATA", INDATA);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        private void control_Init()
        {
            if (!boxingYn) //포장완료 인 경우
            {
                txtBoxId.Text = "";
                txtLotId.Text = "";
                boxing_lot = "";

                lot_prod = "";
                lot_proc = "";
                lot_eqsgid = "";
                lot_class_old = "";
                txtBoxInfo.Text = "";

                box_prod = "";

                boxingLot_idx = 0;
                btnPack.Tag = ObjectDic.Instance.GetObjectName("신규");
                nbBoxingCnt.Value = 5;
                lotCountReverse = 5;

                tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                chkBoxId.IsChecked = false;
                txtBoxId.IsReadOnly = false;
                //boxLotmax_cnt = 0;
                Util.gridClear(dgBoxLot); //그리드 clear
            }
        }

        private void boxingStatInit()
        {
            boxingYn = false; //대기나 완료 상태로.
            boxingLot_idx = 0;
            boxLotmax_cnt = 5; // boxid에 수량 정보 담고 있음. 현재는 미정의 되어 있어서 테스트용으로 5를 세팅
        }

        private void pack_unPack_init(object sender)
        {
            try
            {
                Button btn = sender as Button;
                string unpack_boxid = string.Empty;

                string boxid = Util.GetCondition(txtBoxId);               
                string prod = string.Empty;
                string eqsg = string.Empty;
                string eqpt = string.Empty;
                string proc = string.Empty;

                if (btn.Name == "btnUnPack") //재포장을 위한 포장해제
                {
                    unpack_boxid = Util.GetCondition(txtBoxId);

                    prod = seleted_Box_Prod;
                    eqsg = seleted_Box_Eqsgid;
                    eqpt = seleted_Box_Eqptid;
                    proc = seleted_Box_Procid;
                }
                else // 리스트에서 포장해제
                {
                    unpack_boxid = Util.GetCondition(txtBoxIdR);
                    prod = unPack_ProdID;
                    eqsg = unPack_EqsgID;
                    eqpt = unPack_EqptID;
                    proc = unPack_ProcID;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY2", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("ERP_IF_FLAG", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));

                DataRow dr = INDATA.NewRow();              
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = unpack_boxid;
                dr["PRODID"] = prod;
                dr["PACK_LOT_TYPE_CODE"] = "LOT";
                dr["UNPACK_QTY"] = DataTableConverter.GetValue(dgBoxhistory.Rows[dgBoxhistory.CurrentRow.Index].DataItem, "BOXLOTCNT");
                dr["UNPACK_QTY2"] = DataTableConverter.GetValue(dgBoxhistory.Rows[dgBoxhistory.CurrentRow.Index].DataItem, "BOXLOTCNT"); ;
                dr["USERID"] = LoginInfo.USERID;
                dr["ERP_IF_FLAG"] = "C";
                dr["NOTE"] = "BOX UNPACK";
                INDATA.Rows.Add(dr);

                //DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_BOX_UNPACK", "INDATA", "OUTDATA", INDATA);
                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_BOX", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    if (btn.Name == "btnUnPack") //재포장시만 처리
                    {
                        txtcnt.Visibility = Visibility.Visible;
                        btnUnPack.Visibility = Visibility.Hidden;

                        btnSelectCacel.Visibility = Visibility.Visible;
                        btncancel.Visibility = Visibility.Visible;
                        btnPack.Content = ObjectDic.Instance.GetObjectName("포장");

                        boxingLot_idx = 0;
                        boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
                        lotCountReverse = boxLotmax_cnt;

                        //dgBoxLot.ItemsSource = null;
                        txtLotId.IsEnabled = true;
                        (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;

                        boxingYn = true;

                        setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");

                        Util.AlertInfo("포장이 해제 되어 재포장 가능합니다.");
                    }
                    else
                    {
                        Util.AlertInfo("포장이 해제 되었습니다.");
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void PopUpOpen(string sMAINFORMPATH, string sMAINFORMNAME)
        {
            Assembly asm = Assembly.LoadFrom("ClientBin\\" + sMAINFORMPATH + ".dll");
            Type targetType = asm.GetType(sMAINFORMPATH + "." + sMAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private DataTable getWipList()
        {
            try
            {
                //DA_PRD_SEL_WIP_PACK_ROUTE
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("TOPCNT", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                dr["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                dr["TOPCNT"] = 10;
                RQSTDT.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LABEL_PRT_REQ_HIST_BYLOT", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void showLabelPrintInfoPopup(string sLotid, string sMLotid)
        {
            try
            {
                PACK001_002_PRINTINFOMATION popup = new PACK001_002_PRINTINFOMATION();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));
                    dtData.Columns.Add("MLOTID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["LOTID"] = sLotid;
                    newRow["MLOTID"] = sMLotid;
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// TB_SFC_LABEL_PRT_REQ_HIST 
        /// PRT_FLAG = 'Y' 로 UPDATE
        /// </summary>
        /// <param name="sScanid"></param>
        /// <param name="sPRT_SEQ"></param>
        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid, string sPRT_SEQ)
        {
            try
            {
                //DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(Int32));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = sScanid;
                dr["PRT_REQ_SEQNO"] = Util.NVC_Int((sPRT_SEQ));
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG", "RQSTDT", "", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBoxResultList()
        {
            try
            {
                //DA_PRD_SEL_BOX_LIST_FOR_LABEL

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = null;
                dr["EQPTID"] = null;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["BOXID"] = Util.GetCondition(txtBoxIdR) == "" ? null : Util.GetCondition(txtBoxIdR);
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LIST_FOR_LABEL1", "RQSTDT", "RSLTDT", RQSTDT);

                return dtboxList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void reSet()
        {
            txtcnt.Visibility = Visibility.Visible;
            btnUnPack.Visibility = Visibility.Hidden;
            btnSelectCacel.Visibility = Visibility.Visible;
            btncancel.Visibility = Visibility.Visible;
            btnPack.Content = ObjectDic.Instance.GetObjectName("포장");
            btnPack.Tag = ObjectDic.Instance.GetObjectName("신규");

            boxingLot_idx = 0;
            nbBoxingCnt.Value = 5;
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt;
            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            

            dgBoxLot.ItemsSource = null;
            (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;
            txtLotId.IsEnabled = true;
            chkBoxId.IsChecked = false;
            chkBoxId.Visibility = Visibility.Visible;
            txtBoxId.IsEnabled = true;
            txtBoxId.IsReadOnly = false;

            boxingYn = false;

            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "대기중");

            txtBoxId.Text = "";
            txtLotId.Text = "";
            txtBoxIdR.Text = "";
                      
            lot_prod = "";
            lot_proc = "";
            lot_eqsgid = "";
            lot_class_old = "";
            txtBoxInfo.Text = "";

            boxing_lot = "";
            box_prod = "";
        }

        private void reSet_Last_Stat()
        {
            txtcnt.Visibility = Visibility.Visible;
            btnUnPack.Visibility = Visibility.Hidden;
            btnSelectCacel.Visibility = Visibility.Visible;
            btncancel.Visibility = Visibility.Visible;
            btnPack.Content = ObjectDic.Instance.GetObjectName("포장");
            btnPack.Tag = ObjectDic.Instance.GetObjectName("신규");

            boxingLot_idx = 0;
            //nbBoxingCnt.Value = 5;
            boxLotmax_cnt = Convert.ToInt32(nbBoxingCnt.Value);
            lotCountReverse = boxLotmax_cnt;
            tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";


            dgBoxLot.ItemsSource = null;
            (dgBoxLot.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn).MaxWidth = 40;
            txtLotId.IsEnabled = true;
            //chkBoxId.IsChecked = false;
            //chkBoxId.Visibility = Visibility.Visible;
            //txtBoxId.IsEnabled = true;
            //txtBoxId.IsReadOnly = false;

            boxingYn = false;

            setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "대기중");

            txtBoxId.Text = "";
            txtLotId.Text = "";
            txtBoxIdR.Text = "";

            lot_prod = "";
            lot_proc = "";
            lot_eqsgid = "";
            lot_class_old = "";
            txtBoxInfo.Text = "";

            boxing_lot = "";
            box_prod = "";
        }

        private void buttonAccess(object sender)
        {
            try
            {
                Button btn = sender as Button;

                string grid_name = "dgBoxhistory";

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
                foreach (var item in ilist)
                {
                    C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;

                        grid_name = presenter.DataGrid.Name;
                    }
                }

                DataRowView drv = row.DataItem as DataRowView;

                seleted_boxid = drv["BOXID"].ToString();

                //그리드에서 적용 버튼 클릭해서 넘어온 경우 : 이미 포장 해제 된 box 일 수 있으므로 확인해줌.
                if (btn.Name == "btnAccept")
                {
                    if (!boxValidation_accept(seleted_boxid))
                    {
                        return;
                    }
                }

                if (drv["PALLETID"] != null)
                {
                    seleted_palletID = drv["PALLETID"].ToString();
                }

                if (!boxingYn)
                {
                    btnPack.Content = ObjectDic.Instance.GetObjectName("포장작업전환");
                    txtBoxId.Text = seleted_boxid;
                    SetBoxkeyDown();
                }
                else
                {
                    Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");
                }

                

                seleted_Box_Prod = drv["PRODID"].ToString(); //PALLET의 제품
                seleted_Box_Procid = drv["PROCID"] == null ? null : drv["PROCID"].ToString(); //PALLET의 공정
                seleted_Box_Eqptid = drv["EQPTID"].ToString(); //PALLET의 설비
                seleted_Box_Eqsgid = drv["EQSGID"].ToString(); //PALLET의 라인
                seleted_Box_PrdClass = drv["PRDCLASS"].ToString(); //PALLET의 라인

                boxLotmax_cnt = Convert.ToInt32(drv["BOXLOTCNT"]); //포장가능수량
                boxingLot_idx = Convert.ToInt32(drv["BOXLOTCNT"]); //포장된 수량

                lotCountReverse = boxLotmax_cnt - boxingLot_idx; //남은수량
                nbBoxingCnt.Value = boxLotmax_cnt;

                txtBoxInfo.Text = seleted_Box_PrdClass + " : " + seleted_Box_Eqsgid + " : " + seleted_Box_Prod;

                setBoxCnt(boxLotmax_cnt, boxingLot_idx, lotCountReverse, "포장중");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SearchBox()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = ""; //Util.GetCondition(cboArea);
                dr["PRODID"] = ""; // Util.GetCondition(cboProductModel);
                dr["MODLID"] = "";  //Util.GetCondition(cboProduct);
                dr["FROMDATE"] = DateTime.Now.AddYears(-10).ToString("yyyyMMdd");
                dr["TODATE"] = Util.GetCondition(dtpDateTo); //dtpDateTo.SelectedDateTime.ToString();
                dr["BOXID"] = Util.GetCondition(txtSearchBox);
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgBoxhistory.ItemsSource = null;
                txtBoxIdR.Text = "";

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                }
                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void toggle_Checked(object sender)
        {
            try
            {
                ToggleButton btn = sender as ToggleButton;

                if (btn == null)
                {
                    return;
                }

                if (btn.Name == "btnExpandFrame2")
                {
                    if(btnExpandFrame3 != null)
                    {
                        btnExpandFrame3.Checked -= btnExpandFrame3_Checked;
                        btnExpandFrame3.IsChecked = true;
                        btnExpandFrame3.Checked += btnExpandFrame3_Checked;
                    }                    
                }
                else
                {
                    btnExpandFrame2.Checked -= btnExpandFrame2_Checked;
                    btnExpandFrame2.IsChecked = true;
                    btnExpandFrame2.Checked += btnExpandFrame2_Checked;
                }

                main_grid.ColumnDefinitions[1].Width = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                main_grid.ColumnDefinitions[0].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void toggle_Unchecked(object sender)
        {
            try
            {
                ToggleButton btn = sender as ToggleButton;

                if (btn == null)
                {
                    return;
                }

                if (btn.Name == "btnExpandFrame2")
                {
                    btnExpandFrame3.Unchecked -= btnExpandFrame3_Unchecked;
                    btnExpandFrame3.IsChecked = false;
                    btnExpandFrame3.Unchecked += btnExpandFrame3_Unchecked;
                }
                else
                {
                    btnExpandFrame2.Unchecked -= btnExpandFrame2_Unchecked;
                    btnExpandFrame2.IsChecked = false;
                    btnExpandFrame2.Unchecked += btnExpandFrame2_Unchecked;
                }

                main_grid.ColumnDefinitions[1].Width = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star); ;
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                main_grid.ColumnDefinitions[0].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setBoxLotCnt()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = box_eqsg == "" ? lot_eqsgid : box_eqsg;
                dr["SHIPTO_ID"] = null;
                dr["PRODID"] = box_prod;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXTOTAL_LOTTOTAL_CNT_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows[0]["LOTCNT"] == null || dtResult.Rows[0]["LOTCNT"].ToString() == "")
                    {
                        ms.AlertInfo("BOX의 [포장 수량]이 정의 되지 않았습니다. Defalut : 5로 세팅됩니다. \nMMD->포장/출하 조건(Pack) 에서 기준정보 등록하세요."); // 추가 : BOX의 [포장 수량]이 정의 되지 않았습니다. Defalut : 5로 세팅됩니다. n\MMD->포장/출하 조건(Pack) 에서 기준정보 등록하세요.
                        nbBoxingCnt.Value = 5;
                    }
                    else
                    {
                        if (Convert.ToInt32(dtResult.Rows[0]["LOTCNT"]) != 0)
                        {
                            nbBoxingCnt.Value = Convert.ToInt32(dtResult.Rows[0]["LOTCNT"]);
                        }
                        else
                        {
                            nbBoxingCnt.Value = 5;
                        }
                    }
                }
                else
                {
                    nbBoxingCnt.Value = 5;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        #endregion

        #endregion


        #region 315H CMA

        #region EVENT
        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtAdvice_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtBatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtpartNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtquantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSerial_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSupplierID_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void btnExpandFrame2_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                toggle_Checked(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExpandFrame2_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                toggle_Unchecked(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExpandFrame3_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                toggle_Checked(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnExpandFrame3_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                toggle_Unchecked(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void cboProduct_315H_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            combo_prd_315H = Util.GetCondition(cboProduct_315H);

            txtpartNumber.Text = combo_prd_315H;
        }

        private void chkBoxid_315H_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)chkBoxid_315H.IsChecked)
            {
                if (boxingYn_315H)
                {
                    chkBoxid_315H.IsChecked = false;
                    txtBoxId_315H.Text = boxing_lot;
                    txtLotId_315H.Text = "";
                    txtBoxId_315H.Text = "";

                    return;
                }

                if (dgResult_315H.GetRowCount() > 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("포장 LIST가 존재합니다. 계속 진행 하시면 LIST가 삭제 됩니다."), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            dgResult_315H.ItemsSource = null;
                        }
                        else
                        {
                            chkBoxid_315H.IsChecked = false;
                            txtBoxId_315H.IsReadOnly = true;
                        }
                    }
                 );
                }

                txtBoxId_315H.IsReadOnly = false;
                txtBoxId_315H.Text = "";
                txtLotId_315H.Text = "";
                reBoxing_315H = false;
                palletYN_315H = false;
                boxingLot_idx_315H = 0;
            }
            else
            {
                if (boxingYn_315H)
                {
                    chkBoxid_315H.IsChecked = true;
                    return;
                }

                dgResult_315H.ItemsSource = null;
                txtBoxId_315H.Text = "";
                txtLotId_315H.Text = "";
                txtBoxId_315H.IsReadOnly = true;
                chkBoxid_315H.IsChecked = false;
                txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("포장대기");
            }
        }

        private void txtBoxId_315H_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (boxingYn_315H)
            {
                txtBoxId_315H.TextChanged -= txtBoxId_315H_TextChanged;
                txtBoxId_315H.Text = boxing_lot_315H;
                txtBoxId_315H.TextChanged += txtBoxId_315H_TextChanged;
            }
        }
        
        private void txtLotId_315H_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotId_315H.Text.Length == 0)
                    {
                        return;
                    }

                    lotInputProcess_315H();
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnInput_315H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotId_315H.Text.Length == 0)
                {
                    return;
                }

                lotInputProcess_315H();

                txtcnt.Text = ObjectDic.Instance.GetObjectName("포장중");
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnReset_315H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!boxingYn)
                {
                    reSet_315H();

                    Util.AlertInfo("작업이 초기화 됐습니다.");
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ObjectDic.Instance.GetObjectName("포장중인 내용이 있습니다. 정말 [작업초기화] 하시겠습니까?"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                    {
                        if (caution_result == MessageBoxResult.OK)
                        {
                            reSet_315H();
                            Util.AlertInfo("작업이 초기화 됐습니다.");
                        }
                        else
                        {
                            return;
                        }
                    }
              );
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgResult_315H_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult_315H.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgResult_315H_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgResult_315H.GetCellFromPoint(pnt);
            if (cell != null)
            {
                dgResult_315H.CurrentCell = cell;
            }
        }

        private void menu_LotDelete_315H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgResult_315H.ItemsSource == null)
                {
                    return;
                }

                if (dgResult_315H.CurrentCell != null)
                {
                    if (reBoxing_315H)
                    {
                        if (unPackYN_315H)
                        {
                            unpackProcess_315H();

                            txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("포장중");
                            boxingYn_315H = true;
                        }
                    }

                    int delete_idx = dgResult_315H.CurrentCell.Row.Index;
                    dgResult_315H.EndNewRow(true);
                    dgResult_315H.RemoveRow(delete_idx);

                    txtquantity.Text = dgResult_315H.GetRowCount().ToString();

                    Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt_315H, Util.NVC(dgResult_315H.GetRowCount()));
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnOutput_315H_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBoxId_315H.Text.Length == 0)
                {
                    if (reBoxing_315H)
                    {
                        return;
                    }

                    if (dgResult_315H.GetRowCount() == 0)
                    {
                        return;
                    }

                    autoBoxIdCreate_315H();
                }

                if (dgResult_315H.GetRowCount() == 0)
                {
                    return;
                }

                if (reBoxing_315H)
                {
                    if (!boxingYn_315H)
                    {
                        return;
                    }
                }

                if (txtSerial.Text != txtBatch.Text)
                {
                    ms.AlertWarning("SFU3376"); //Serial No과 Batch No가 일치 하지 않습니다
                    return;
                }

                boxingEnd_315H(); //포장 완료 함수

                PrintAcess_315H(); //라벨발행               

                reSet_315H();

                Util.AlertInfo("포장/라벨발행 완료");  

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate == null || dtpDate.SelectedDateTime == null)
            {
                return;
            }

            txtBatch.Text = dtpDate.SelectedDateTime.ToString("yyMMdd");
        }

        private void btnBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtBoxId_315H.Text.Length == 0)
            {
                return;
            }

            if (!Convert.ToBoolean(chkBoxid_315H.IsChecked))
            {
                return;
            }

            boxValidation_315H();
        }


        #endregion

        #region METHOD
       

        private void set315H(string lotid, string gubun)
        {
            tcMain.SelectedIndex = 1;

            //init_315H();

            if (gubun == "BOX")
            {
                chkBoxid_315H.IsChecked = true;
                txtBoxId_315H.IsReadOnly = false;
                txtBoxId_315H.Text = lotid;
                txtLotId_315H.Text = "";

                btnBoxSearch_Click(null, null);
            }
            else
            {
                chkBoxid_315H.IsChecked = false;
                txtBoxId_315H.IsReadOnly = true;
                txtLotId_315H.Text = lotid;
                txtBoxId_315H.Text = "";

                lotInputProcess_315H();
            }

            btnExpandFrame2.IsChecked = false;
            btnExpandFrame2_Unchecked(null, null);
        }

        private void init_315H()
        {
            setTexBox_315H();

            InitCombo_315H();
        }

        private void setTexBox_315H()
        {
            txtReceive.Text = "Viridi E-Mobility Technology";
            txtDock.Text = ""; // "TVV";
            txtSupplierAddress.Text = "LG Chem, Ltd";
            txtNetWeight.Text = "144";
            txtGrossWeight.Text = "163";
            txtBoxes.Text = "1";
            txtquantity.Text = "12";
            txtSerial.Text = "BOXID";
            txtBatch.Text = DateTime.Now.ToString("yyMMdd"); // "161207";
        }

        private void InitCombo_315H()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboArea = new C1ComboBox();
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
            C1ComboBox cboProductModel = new C1ComboBox();
            cboProductModel.SelectedValue = "";
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = prdClass_315H;

            //제품코드  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct_315H, CommonCombo.ComboStatus.NONE, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");
        }
        private void boxValidation_315H()
        {
            //이전 포장 작업 유무
            if (boxingYn_315H)
            {
                Util.AlertInfo("이전 포장 작업이 완료 되지 않았습니다.");
                return;
            }

            //입력된 boxid 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = txtBoxId_315H.Text;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID_WITHNOT_LOTBOX", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    reSet_315H();
                    Util.AlertInfo("입력한 BOX가 존재하지 않습니다.");
                    return;
                }

                string BOXING_GUBUN = dtResult.Rows[0]["BOXING_GUBUN"].ToString(); //lot으로 pallet포장한 boxid 인지 체크

                if (BOXING_GUBUN == "BOXING_NO")
                {
                    reSet_315H();
                    Util.AlertInfo("포장,재포장 불가능한 BOX입니다.");
                    return;
                }

                string PalletID = dtResult.Rows[0]["OUTER_BOXID"].ToString(); //pallet에 포장된 boxid 인지 체크

                if (PalletID.Length > 0)
                {
                    reSet_315H();

                    String[] param = { PalletID };
                    //Util.AlertInfo("이미 PALLET( " + PalletID + " )에 포장된 BOX입니다.");
                    Util.AlertInfo("이미 PALLET({0})에 포장된 BOX입니다.", param);
                    return;
                }

                box_prod_315H = dtResult.Rows[0]["PRODID"].ToString(); //box의 prod 담아둠.

                if (Util.GetCondition(cboProduct_315H) != box_prod_315H)
                {
                    Util.AlertInfo("입력한 BOX와 진행중인 제품이 다릅니다.");
                    return;
                }

                foreach (DataRow drw in dtResult.Rows) //이미 포장된 box
                {
                    if (drw["BOXSTAT"].ToString() == "PACKED" && drw["BOXTYPE"].ToString() == "BOX") //if (drw["BOXSTAT"].ToString() == "PACKED") //BOXSTAT 미정의 정의 되면 수정 필요.
                    {
                        if (drw["OUTER_BOXID"] != null && drw["OUTER_BOXID"].ToString().Length > 0 && drw["BOXSTAT"].ToString().Length > 0)
                        {
                            palletYN_315H = true; //팔래트에 이미 포장된 box                            
                        }

                        seleted_BOX_LotCNT_315H = drw["TOTAL_QTY"].ToString(); //box에 포장된 lot의 수량

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("BOXID", typeof(string));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["BOXID"] = txtBoxId_315H.Text;

                        RQSTDT1.Rows.Add(dr1);

                        DataTable dtBoxLots = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXLOTS_SEARCH", "INDATA", "OUTDATA", RQSTDT1);

                        //txtLotId.IsEnabled = false;

                        dgResult_315H.ItemsSource = null;
                        Util.GridSetData(dgResult_315H, dtBoxLots, FrameOperation);
                        boxingLot_idx_315H = dgResult_315H.GetRowCount();
                        Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt_315H, Util.NVC(dtBoxLots.Rows.Count));

                        if (!palletYN_315H)
                        {
                            unPackYN_315H = true;
                        }

                    }
                    else if (drw["BOXSTAT"].ToString() == "CREATED" && drw["BOXTYPE"].ToString() == "BOX")
                    {
                        dgResult_315H.ItemsSource = null;
                        boxingLot_idx_315H = 0;
                        boxing_lot_315H = txtBoxId_315H.Text.ToString();
                    }

                    boxing_lot_315H = drw["BOXID"].ToString();
                    box_prod_315H = drw["PRODID"].ToString();
                    txtquantity.Text = Convert.ToInt32(drw["TOTAL_QTY"]).ToString();
                    txtSerial.Text = drw["BOXID"].ToString();
                    boxStat_315H = drw["BOXSTAT"].ToString(); // packed, created
                    reBoxing_315H = true; //box가 존재하면 box 상태와 상관없이 재포장임.
                    txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("포장가능");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void reSet_315H()
        {
            boxingYn_315H = false;
            reBoxing_315H = false;
            palletYN_315H = false;
            boxingLot_idx_315H = 0;

            txtBoxId_315H.Text = "";
            txtLotId_315H.Text = "";

            chkBoxid_315H.IsChecked = false;
            txtBoxId_315H.IsReadOnly = true;

            dgResult_315H.ItemsSource = null;
            tbBoxingWait_cnt_315H.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("포장대기");

            lot_prod_315H = string.Empty; //이전에 투입한 lot의 prodid         
            lot_proc_315H = string.Empty; //이전에 투입한 lot의 procid
            lot_eqsgid_315H = string.Empty; //이전에 투입한 lot의  eqsgid
            lot_class_old_315H = string.Empty; //이전에 투입한 lot의 class

            box_prod_315H = string.Empty; //현재 포장중니 BOX의 PRODID
            boxing_lot_315H = string.Empty; //현재 포장중인 BOXID

            seleted_Box_Prod_315H = string.Empty;
            seleted_Box_Procid_315H = string.Empty;
            seleted_Box_Eqptid_315H = string.Empty;
            seleted_Box_Eqsgid_315H = string.Empty;

            txtSerial.Text = "BOX ID";
        }

        private void autoBoxIdCreate_315H()
        {
            try
            {
                //boxid 생성 로직
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                RQSTDT.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                RQSTDT.Columns.Add("LOTID", typeof(string));    //투이LOT(처음 LOT)
                RQSTDT.Columns.Add("PROCID", typeof(string));   //ㅍ장공정 ID
                RQSTDT.Columns.Add("PRODID", typeof(string));   //lot의 제품
                RQSTDT.Columns.Add("LOTQTY", typeof(Int32));   //투입 총수량
                RQSTDT.Columns.Add("EQSGID", typeof(string));   //라인ID
                RQSTDT.Columns.Add("USERID", typeof(string));   //사용자ID

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotId_315H.Text;
                dr["PROCID"] = lot_proc_315H; // CMA:P5500, BMA:P9500
                dr["PRODID"] = lot_prod_315H;
                dr["LOTQTY"] = dgResult_315H.GetRowCount().ToString();
                dr["EQSGID"] = lot_eqsgid_315H;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBoxId_315H.Text = dtResult.Rows[0][0].ToString();
                    txtSerial.Text = dtResult.Rows[0][0].ToString();
                    boxing_lot_315H = txtBoxId.Text.ToString();
                    txtquantity.Text = dgResult_315H.GetRowCount().ToString();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void boxingEnd_315H()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                INDATA.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                INDATA.Columns.Add("BOXID", typeof(string));    //투이LOT(처음 LOT)
                INDATA.Columns.Add("PROCID", typeof(string));   //공정ID(포장전 마지막 공정) 
                INDATA.Columns.Add("BOXQTY", typeof(string));   //투입 총수량
                INDATA.Columns.Add("EQSGID", typeof(string));   //라인ID
                INDATA.Columns.Add("USERID", typeof(string));   //사용자ID

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = Util.GetCondition(txtBoxId_315H);
                dr["PROCID"] = lot_proc_315H;
                dr["BOXQTY"] = dgResult_315H.GetRowCount().ToString();
                dr["EQSGID"] = lot_eqsgid_315H;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable IN_LOTID = indataSet.Tables.Add("IN_LOTID");
                IN_LOTID.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgResult_315H.GetRowCount(); i++)
                {
                    string sLotId = Util.NVC(dgResult_315H.GetCell(i, dgResult_315H.Columns["LOTID"].Index).Value);

                    DataRow inDataDtl = IN_LOTID.NewRow();
                    inDataDtl["LOTID"] = sLotId;
                    IN_LOTID.Rows.Add(inDataDtl);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BOXING", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    boxingYn_315H = false;                   
                }
                else
                {
                    throw new Exception("포장 작업 실패 BOXING BIZ 확인 하세요.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //private void PrintProcess_315H()
        //{
        //    if (!bkWorker.IsBusy)
        //    {
        //        blPrintStop_315H = false;
        //        bkWorker.RunWorkerAsync();
        //        btnOutput_315H.Content = ObjectDic.Instance.GetObjectName("취 소");
        //        btnOutput_315H.Foreground = Brushes.White;
        //    }
        //    else
        //    {
        //        btnOutput_315H.Content = ObjectDic.Instance.GetObjectName("출 력");
        //        blPrintStop_315H = true;
        //        btnOutput_315H.Foreground = Brushes.Red;
        //    }

        //}

        private void lotInputProcess_315H()
        {
            try
            {
                if (txtBoxId_315H.Text == "") //BOXID 입력되지 않음
                {
                    boxingLotAddProcess_315H(); //신규포장     

                    txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("포장가능");
                }
                else
                {
                    reBoxingLotAddProcess_315H(); //재포장      

                }
            }
            catch (Exception ex)
            {
                txtLotId_315H.Text = "";
                throw ex;
            }
        }       

        //신규 포장의 경우 lot 추가
        private void boxingLotAddProcess_315H()
        {
            try
            {
                if (!Convert.ToBoolean(chkBoxid_315H.IsChecked)) //BOXID 체크 박스 체크가 풀렸을 경우
                {
                    //grid 중복 체크
                    if (!gridDistinctCheck_315H())
                    {
                        Util.AlertInfo("이미 list에 있는 LOT입니다.");
                        return;
                    }

                    //입력된 lot validation
                    if (!lotValidation_BR_315H()) //if (!lotValidation())
                    {
                        txtLotId_315H.Text = "";
                        return;
                    }

                    if (lot_prod_315H != combo_prd_315H)
                    {
                        Util.AlertInfo("선택한(combo) PROD와 LOT의 PROD가 다릅니다.");
                        return;
                    }

                    //lot을 그리드(dgBoxLot)에 추가
                    addGridLot_315H();

                    //txtBoxId.Text = boxing_lot;

                    txtLotId_315H.Text = "";

                    //boxingYn = true; //박싱중.

                }
                else
                {
                    Util.AlertInfo("###[포장작업]을 계속 하시려면### \n1.BOXID를 입력 후 조회 버튼을 클릭한다. \n2.CHECKBOX 체크를 해제한다.\n두가지 방법중 한가지를 선택후 진행하세요.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //재포장 인 경우 lot 추가
        private void reBoxingLotAddProcess_315H()
        {
            try
            {
                if (gridDistinctCheck_315H()) //그리드 중복 체크
                {
                    //입력된 lotid validation
                    if (!lotValidation_BR_315H()) //if (!lotValidation())
                    {
                        txtLotId_315H.Text = "";
                        return;
                    }

                    if (lot_prod_315H != combo_prd_315H)
                    {
                        Util.AlertInfo("선택한(combo) PROD와 LOT의 PROD가 다릅니다.");
                        return;
                    }

                    if (lot_prod_315H != box_prod_315H)
                    {
                        Util.AlertInfo("BOX의 PROD와 LOT의 PROD가 다릅니다.");
                        return;
                    }

                    if (unPackYN_315H)
                    {
                        unpackProcess_315H();
                    }


                    addGridLot_315H();
                    txtLotId_315H.Text = "";
                    txtcnt_315H.Text = ObjectDic.Instance.GetObjectName("포장중");
                    boxingYn_315H = true;
                }
                else
                {
                    Util.AlertInfo("이미 list에 있는 LOT입니다.");
                    txtLotId.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool gridDistinctCheck_315H()
        {
            try
            {
                DataRowView rowview = null;

                if (dgResult_315H.GetRowCount() == 0)
                {
                    return true;
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dgResult_315H.Rows)
                {

                    if (row.DataItem != null)
                    {
                        rowview = row.DataItem as DataRowView;

                        if (rowview["LOTID"].ToString() == txtLotId_315H.Text.ToString())
                        {                           
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool lotValidation_BR_315H()
        {
            try
            {
                //lot_proc = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));    //투입LOT(처음 LOT)       
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOX_PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = Util.GetCondition(txtLotId_315H); //LOTID
                dr["BOXTYPE"] = "LOT";
                dr["BOX_PRODID"] = lot_prod_315H == "" ? null : lot_prod_315H;
                dr["PRDT_CLSS"] = lot_class_old_315H == "" ? null : lot_class_old_315H;
                dr["EQSGID"] = lot_eqsgid_315H == "" ? null : lot_eqsgid_315H;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_BOXLOT_PALLET", "INDATA", "OUTDATA", RQSTDT);

                //LOT TABLE과 WIP TABLE의 PROD 비교
                if (dtResult.Rows.Count > 0)
                {
                    string lot_class = dtResult.Rows[0]["CLASS"].ToString();
                    string lot_procid = dtResult.Rows[0]["PROCID"].ToString();
                    string lot_prodid = dtResult.Rows[0]["PRODID"].ToString();
                    string lot_eqsg = dtResult.Rows[0]["EQSGID"].ToString();

                    lot_proc_315H = lot_procid;
                    lot_prod_315H = lot_prodid;
                    lot_eqsgid_315H = lot_eqsg;
                    lot_class_old_315H = lot_class;

                    return true;
                }
                else
                {
                    Util.AlertInfo("LOT의 포장 가능 정보가 맞지 않아 포장할 수 없습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void addGridLot_315H()
        {
            if (boxingLot_idx_315H == 0)
            {
                DataTable dtBoxLot = new DataTable();
                dtBoxLot.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtBoxLot.NewRow();
                dr["LOTID"] = Util.GetCondition(txtLotId_315H);

                dtBoxLot.Rows.Add(dr);

                dgResult_315H.ItemsSource = DataTableConverter.Convert(dtBoxLot);

                //boxingYn = true;
            }
            else
            {
                int TotalRow = dgResult_315H.GetRowCount(); //헤더제거

                dgResult_315H.EndNewRow(true);
                DataGridRowAdd(dgResult_315H);

                DataTableConverter.SetValue(dgResult_315H.Rows[TotalRow].DataItem, "LOTID", Util.GetCondition(txtLotId_315H));
            }
            boxingLot_idx_315H++;
            //lotCountReverse--;

            DataTable dt = DataTableConverter.Convert(dgResult_315H.ItemsSource);

            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt_315H, Util.NVC(dt.Rows.Count));


            txtquantity.Text = dgResult_315H.GetRowCount().ToString();
            txtLotId_315H.SelectAll();
            txtLotId_315H.Focus();
        }

        private void unpackProcess_315H()
        {
            try
            {
                if (reBoxing_315H == true && unPackYN_315H == true) //재포장이면서 아직 unpack 하지 않은 경우 uppack 시킴.
                {
                    unPack_315H();

                    unPackYN_315H = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void unPack_315H()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY2", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("ERP_IF_FLAG", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = boxing_lot_315H;
                dr["PRODID"] = box_prod_315H;
                dr["PACK_LOT_TYPE_CODE"] = "LOT";
                dr["UNPACK_QTY"] = seleted_BOX_LotCNT_315H;
                dr["UNPACK_QTY2"] = seleted_BOX_LotCNT_315H;
                dr["USERID"] = LoginInfo.USERID;
                dr["ERP_IF_FLAG"] = "C";
                dr["NOTE"] = "BOX UNPACK";
                INDATA.Rows.Add(dr);

                //DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_BOX_UNPACK", "INDATA", "OUTDATA", INDATA);
                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_BOX", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    boxStat_315H = "CREATED";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PrintAcess_315H()//private void BarcodePrint(bool isTest, string sLabelType, string sLotId, string sProdId, string sProdName, string sQty)
        {
            try
            {
                string I_ATTVAL = string.Empty;
                CMM_ZPL_VIEWER2 wndPopup;

                I_ATTVAL = labelItemsGet_315H();

                getZpl_315H(I_ATTVAL);
                Util.PrintLabel(FrameOperation, loadingIndicator, zpl_315H);

                ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다

                if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                {
                    wndPopup = new CMM_ZPL_VIEWER2(zpl_315H);
                    wndPopup.Show();
                }                
            }
            catch (Exception ex)
            {
                ms.AlertWarning(ex.Message);
            }
        }

        private string labelItemsGet_315H()
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = label_code_315H;

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : 코드=값^코드=값

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData_315H();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        #region sample value 뿌림
                        /*
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();
                        */
                        #endregion

                        #region 화면에서 입력된 값 뿌림                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtInput.Rows[0][item_code].ToString() == "" ? null : dtInput.Rows[0][item_code].ToString();
                        #endregion

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }
                    }
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable getInputData_315H()
        {
            DataTable dt = new DataTable();
            dt.TableName = "INPUTDATA";
            dt.Columns.Add("ITEM001", typeof(string));
            dt.Columns.Add("ITEM002", typeof(string));
            dt.Columns.Add("ITEM003", typeof(string));
            dt.Columns.Add("ITEM004", typeof(string));
            dt.Columns.Add("ITEM005", typeof(string));
            dt.Columns.Add("ITEM006", typeof(string));
            dt.Columns.Add("ITEM007", typeof(string));
            dt.Columns.Add("ITEM008", typeof(string));
            dt.Columns.Add("ITEM009", typeof(string));
            dt.Columns.Add("ITEM010", typeof(string));
            dt.Columns.Add("ITEM011", typeof(string));
            dt.Columns.Add("ITEM012", typeof(string));

            dt.Columns.Add("ITEM013", typeof(string));
            dt.Columns.Add("ITEM014", typeof(string));
            dt.Columns.Add("ITEM015", typeof(string));
            dt.Columns.Add("ITEM016", typeof(string));
            dt.Columns.Add("ITEM017", typeof(string));
            dt.Columns.Add("ITEM018", typeof(string));
            dt.Columns.Add("ITEM019", typeof(string));
            dt.Columns.Add("ITEM020", typeof(string));
            dt.Columns.Add("ITEM021", typeof(string));

            DataRow dr = dt.NewRow();
            dr["ITEM001"] = Util.GetCondition(txtReceive); //VOLVO TORSLANDA MONTERING
            dr["ITEM002"] = Util.GetCondition(txtDock); //TCV
            dr["ITEM003"] = Util.GetCondition(txtAdvice); //61606292
            dr["ITEM004"] = Util.GetCondition(txtAdvice) == "" ? "" : "N" + Util.GetCondition(txtAdvice); //61606292
            dr["ITEM005"] = Util.GetCondition(txtSupplierAddress); // LG Chem, Ltd
            dr["ITEM006"] = Util.GetCondition(txtNetWeight); //115
            dr["ITEM007"] = Util.GetCondition(txtGrossWeight); //160
            dr["ITEM008"] = Util.GetCondition(txtBoxes); //1
            dr["ITEM009"] = Util.GetCondition(txtpartNumber); //31491834
            dr["ITEM010"] = Util.GetCondition(txtpartNumber) == "" ? "" : "P" + Util.GetCondition(txtpartNumber); //31491834
            dr["ITEM011"] = Util.GetCondition(txtquantity); //1
            dr["ITEM012"] = Util.GetCondition(txtquantity) == "" ? "" : "Q" + Util.GetCondition(txtquantity); //1

            dr["ITEM013"] = Util.GetCondition(cboDescription) == null ? "" : Util.GetCondition(cboDescription); //BATTERY PACK
            dr["ITEM014"] = Util.GetCondition(cboDescription) == null ? "" : Util.GetCondition(cboDescription); //355V,25.9A,6500Wh(Usable)
            dr["ITEM015"] = Util.GetCondition(txtSupplierID); //GE2PB
            dr["ITEM016"] = Util.GetCondition(txtSupplierID) == "" ? "" : Util.GetCondition(txtSupplierID); //GE2PB
            dr["ITEM017"] = Util.GetCondition(txtDate); //D160629
            dr["ITEM018"] = Util.GetCondition(txtSerial); //616242017
            dr["ITEM019"] = Util.GetCondition(txtSerial); //616242017
            dr["ITEM020"] = Util.GetCondition(txtBatch); //616242017
            dr["ITEM021"] = Util.GetCondition(txtBatch) == "" ? "" : "H" + Util.GetCondition(txtBatch); //616242017
            dt.Rows.Add(dr);

            return dt;
        }

        private void getZpl_315H(string I_ATTVAL)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("I_LBCD", typeof(string));
                //RQSTDT.Columns.Add("I_PRMK", typeof(string));
                //RQSTDT.Columns.Add("I_RESO", typeof(string));
                //RQSTDT.Columns.Add("I_PRCN", typeof(string));
                //RQSTDT.Columns.Add("I_MARH", typeof(string));
                //RQSTDT.Columns.Add("I_MARV", typeof(string));
                //RQSTDT.Columns.Add("I_ATTVAL", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["I_LBCD"] = label_code_315H;
                //dr["I_PRMK"] = "Z";
                //dr["I_RESO"] = "203";
                //dr["I_PRCN"] = "1";
                //dr["I_MARH"] = "0";
                //dr["I_MARV"] = "0";
                //dr["I_ATTVAL"] = I_ATTVAL;

                //RQSTDT.Rows.Add(dr);

                ////ITEM001=TEST1^ITEM002=TEST2

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_DESIGN_TEST", "INDATA", "OUTDATA", RQSTDT);

                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: label_code_315H
                                                     , sATTVAL: I_ATTVAL
                                                     );

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl_315H = dtResult.Rows[0]["ZPLSTRING"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion



        #endregion


        #region 312,313,515H, 517H  

        #region EVENT
        private void cboProduct2_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            if (dtModelResult != null || dtModelResult.Rows.Count > 0)
            {
                DataRow[] dr = dtModelResult.Select("CBO_CODE = '" + Util.GetCondition(cboProduct2) + "'");

                tabItem.Header = Util.NVC(dr[0]["MODEL"]);

                getValueSetting();
            }
        }

        private void getValueSetting()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LABEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct2); //dtpDateTo.SelectedDateTime.ToString
                dr["LABEL"] = label_code2; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtTextResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtTextResult == null || dtTextResult.Rows.Count == 0)
                {
                    //ms.AlertWarning("추가"); //MMD 기준정보 누락 : 출하처 인쇄항목 관리
                    return;
                }
                else
                {
                    setTextBox();

                    dtpDate2_SelectedDateChanged(null, null);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setTextBox()
        {
            if (dtTextResult == null || dtTextResult.Rows.Count == 0)
            {
                return;
            }

            txtNetWeight2.Text = returnString("ITEM006");        //Net Weight(kg) : 115           
            txtGrossWeight2.Text = returnString("ITEM007");      //Gross Weight(kg) : 160           
            txtDescription22.Text = returnString("ITEM014");     //Description 2 : 355V,25.9A, 6,500Wh(Usable)

            txtReceive2.Text = returnString("ITEM001");          //Receiver : VOLVO TORSLANDA MONTERING
            txtAdvice2.Text = returnString("ITEM003");           //ADVICE NOTE NO : 61606292
            txtDock2.Text = returnString("ITEM002");             //DOCK GATE : TCV
            txtSupplierAddress2.Text = returnString("ITEM005");  //Supplier Address : LG Chem, Ltd
            txtBoxes2.Text = returnString("ITEM008");            //No of Boxes : 1
            txtpartNumber2.Text = "31407014";                    //Part No : 31491834
            txtquantity2.Text = returnString("ITEM011");         //Quantity : 1
            txtSupplierID2.Text = returnString("ITEM015");       //Supplier ID : GE2PB
            txtSerial.Text = returnString("ITEM018");           //Serial No : 312031601
            txtLogistic12.Text = "logic";
            txtLogistic22.Text = "reference";
            txtBatch2.Text = returnString("ITEM020");            //Batch No : 312031601
            txtDescription12.Text = returnString("ITEM013");     //Description : BATTERY PACK
        }

        private DataRow[] selectText(string item_code)
        {
            DataRow[] drs;

            drs = dtTextResult.Select("ITEM_CODE = '" + item_code + "'");
            return drs;
        }

        private string returnString(string item_code)
        {
            return selectText(item_code).Length > 0 ? Util.NVC(selectText(item_code)[0]["ITEM_VALUE"]) : "";
        }
        

        private void dtpDate2_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDate2 == null || dtpDate2.SelectedDateTime == null)
            {
                return;
            }

            if (cboProduct2 == null)
            {
                return;
            }

            //날짜 선택시 Advice Note No, Date 정보를 가져와서 Text 박스에 세팅   
            txtDate2.Text = "D" + dtpDate.SelectedDateTime.ToString("yyyyMMdd");

            string line_no = getLine();

            txtAdvice2.Text = line_no + dtpDate2.SelectedDateTime.ToString("yyMMdd") + nbProductBox.Value.ToString();

        }

        private string getLine()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                //2018.10.08
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct2); //dtpDateTo.SelectedDateTime.ToString();
                //2018.10.08
                dr["PRODID"] = LoginInfo.CFG_EQSG_ID.ToString();

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_WITH_PRODID_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return "8";
                }
                else
                {
                    return dtResult.Rows[0]["LINE_NO"].ToString();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dtpDate2_SelectedDateChanged1(object sender, PropertyChangedEventArgs<double> e)
        {
            dtpDate2_SelectedDateChanged(null, null);
        }

        private void rdb515H_CheckedChanged(object sender, RoutedEventArgs e)
        {
            dtpDate2_SelectedDateChanged(null, null);
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getValueSetting();

                dtpDate2_SelectedDateChanged(null, null);

                Get_Product_Lot();
                
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnPrint2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnPrint2.Content.ToString() == ObjectDic.Instance.GetObjectName("취 소"))
                {
                    bkWorker.Dispose();
                    blPrintStop = true;
                    btnPrint2.Content = ObjectDic.Instance.GetObjectName("출 력");
                    return;
                }

                if (txtSerial2.Text != txtBatch2.Text)
                {
                    ms.AlertWarning("SFU3376"); //Serial No과 Batch No가 일치 하지 않습니다
                    return;
                }

                PrintProcess();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnAdvice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Label Layout의 Advice Note No 부분 출력
                string strZPLString = "^XA^MCY^XZ^XA^LRN^FWN^CFD,24^LH10,10^CI0^PR2^MNY^MTT^MMT^MD0^PON^PMN^XZ^XA";

                if ((bool)chkAutoPrint.IsChecked)
                {
                    strZPLString += string.Format("^A0N,18,20^FO5,0^CI0^FDAdvice Note No (N)^FS");
                    strZPLString += string.Format("^A0N,70,60^FO210,0^CI0^FD{0}^FS", txtAdvice2.Text); //txt312H03
                    strZPLString += string.Format("^BY4,2.8^FO30,60^B3N,N,104,N,N^FDN{0}^FS", txtAdvice2.Text); //txt312H03
                    strZPLString += string.Format("^PQ{0},0,1,Y^XZ", nbPrintCnt_R.Value);  //nuAdviceNoteNo
                    PrintBoxLabel(strZPLString);

                    ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
                }
                else
                {
                    strZPLString += string.Format("^A0N,70,60^FO210,0^CI0^FD{0}^FS", txtAdvice2.Text); //txt312H03
                    strZPLString += string.Format("^BY4,2.8^FO30,60^B3N,N,104,N,N^FDN{0}^FS", txtAdvice2.Text); //txt312H03
                    strZPLString += string.Format("^PQ{0},0,1,Y^XZ", nbPrintCnt_R.Value);  //nuAdviceNoteNo
                    PrintBoxLabel(strZPLString);

                    ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 

                    return;
                }
            }
            catch(Exception ex)
            {
                Util.Alert(ex.Message);
            }            
        }

        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                //2189498 3호기_312H 포장 라벨 변경요청                   

                string strLotId = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "LOTID").ToString();

                //2297504 Volvo 312H (Pack 3호기) Barcode 체계 변경
                //string strRefNo = strLotId.Contains("T") ? strLotId.Substring(0, strLotId.IndexOf('T')) : "";
                string strRefNo = strLotId.Contains("T") ? strLotId.Substring(0, 11) : "";


                txtpartNumber2.Text = strRefNo;
                txtSerial2.Text = strLotId;
                txtBatch2.Text = strRefNo;

                /*
                string strDateCodeYear = "123456789ABCDEFGHJKLMNPRSTVWXY";
                string strDateCodeMonth = "ABCDEFGHJKLM";

                if (strLotId.Length == 16)
                {
                    string strYY = strLotId.Substring(10, 1);
                    string strMM = strLotId.Substring(11, 1);

                    strLotId = string.Format("3{0:00}{1:00}{2:0000}", strDateCodeYear.IndexOf(strYY) + 1, strDateCodeMonth.IndexOf(strMM) + 1, strLotId.Substring(12, strLotId.Length - 12));

                txtpartNumber.Text = strRefNo;                       

                }
                //2189498 3호기_312H 포장 라벨 변경요청

                //2297504 Volvo 312H (Pack 3호기) Barcode 체계 변경
                if (strLotId.Length == 18)
                {
                    //strLotId = strLotId.Substring(strLotId.IndexOf('T') + 1);
                    strLotId = strLotId.Substring(11);
                    txtpartNumber.Text = strRefNo; 
                }

                //to-be
                if (strLotId.Length > 18)
                {
                    strLotId = strLotId.Substring(11)
                    //strLotId = strLotId.Substring(strLotId.IndexOf('T') + 1);;
                    txtpartNumber.Text = strRefNo;
                }

                txtSerial.Text = strLotId;
                txtBatch.Text = strLotId;

                //2534222 3호기_313H 포장박스 라벨 Dock Gate 수정 기능 구현
                txtDock.Text = "NODATE";
                    //Clipboard.SetText(MakeZPLString());
 */
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #region METHOD
        private void set312_313_315_517()
        {
            tcMain.SelectedIndex = 2;

            btnExpandFrame3.IsChecked = false;
            btnExpandFrame3_Unchecked(null, null);
        }

        private void init_312_313_315_515()
        {            
            //dtpDate2_SelectedDateChanged(null, null);

            labelTextSet();

            cboProduct2.SelectedItemChanged -= cboProduct2_seletedItemChanged;

            setCombo();

            cboProduct2.SelectedItemChanged += cboProduct2_seletedItemChanged;

            //라벨발행 클릭후 넘어온 제품 정보를 제품콤보에 바인딩(제약:그냥 tab 클릭시 예외처리)
            if (setComboBinding())
            {
                search2_LINK();
            }
            else
            {
                ms.AlertWarning("넘어온 제품 정보가 기준정보에 세팅한 정보와 다릅니다.");
            }

            //dgResult.ItemsSource = null;

        }

        private bool setComboBinding()
        {
            bool ret = false;

            if(unPack_ProdID.Length == 0) //조회화면 타고 넘어 오지 않은 경우
            {
                return ret;
            }

            for(int i = 0; i < cboProduct2.Items.Count; i++)
            {
                cboProduct2.SelectedIndex = i;
                if(unPack_ProdID == cboProduct2.SelectedValue.ToString()) //넘어오 제품이 세팅된 콤보중에 있을 경우
                {
                    cboProduct2.SelectedValue = unPack_ProdID;

                    ret = true;
                }
                else
                {
                    ret = false;
                }
            }

            return ret;
        }

        private void bkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                //{
                PrintAcess();
                //}));
            }
            catch (Exception ex)
            {
                bkWorker.CancelAsync();
                blPrintStop = true;

                Util.AlertInfo(ex.Message);
            }
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnPrint2.Content = ObjectDic.Instance.GetObjectName("출력");
            blPrintStop = true;
            btnPrint2.Foreground = Brushes.White;            
        }       

        private void search2_LINK()
        {
            try
            {
                //조회 조건들에 해당하는 LOT_ID와 PALLET_ID 가져와서 Grid에 바인딩  
                dtpDate2_SelectedDateChanged(null, null);
                Get_Product_Lot_LINK();                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void labelTextSet()
        {
            string strRefNo = selectBox.Contains("T") ? selectBox.Substring(0, 11) : "";

            txtpartNumber2.Text = strRefNo;
            //txtSerial2.Text = selectBox;
            txtBatch2.Text = strRefNo;
        }       

        private void Get_Product_Lot()
        {
            DataTable dtResult;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct2);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom2);  //dtpDateFrom.SelectedDateTime.ToString();
                dr["TODATE"] = Util.GetCondition(dtpDateTo2); //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_COUNT_VOLVOBMA", "INDATA", "OUTDATA", RQSTDT);

                dgResult.ItemsSource = null;
                tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgResult, dtResult, FrameOperation);

                Util.SetTextBlockText_DataGridRowCount(tbPalletHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void Get_Product_Lot_LINK()
        {
            DataTable dtResult;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
               
                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = Util.GetCondition(txtBoxIdR);

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLETLOT_WITH_BOX_FIND", "INDATA", "OUTDATA", RQSTDT);

                dgResult.ItemsSource = null;
                tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgResult, dtResult, FrameOperation);

                Util.SetTextBlockText_DataGridRowCount(tbPalletHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private string PrintOutDate(DateTime dt)
        {
            System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
            return dt.ToString("dd") + dt.ToString("MMMM", format).Substring(0, 3).ToUpper() + dt.ToString("yyyy");
        }

        private void PrintProcess()
        {
            if (!bkWorker.IsBusy)
            {
                blPrintStop = false;
                bkWorker.RunWorkerAsync();
                btnPrint2.Content = ObjectDic.Instance.GetObjectName("취소");
                btnPrint2.Foreground = Brushes.White;
            }
            else
            {
                btnPrint2.Content = ObjectDic.Instance.GetObjectName("출력");
                blPrintStop = true;
                btnPrint2.Foreground = Brushes.Red;
            }
        }

        private void PrintAcess()
        {
            try
            {
                Util.SetCondition_Thread(tbPrint2_cnt, "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : 0 " + ObjectDic.Instance.GetObjectName("건") + " ]");
                //tbPrint2_cnt.Text = "[인쇄된 수 : 0 건]";
                string I_ATTVAL = string.Empty;
                

                I_ATTVAL = labelItemsGet();

                getZpl(I_ATTVAL);

                for (int i = 0; i <Convert.ToUInt32(Util.GetCondition_Thread(nbPrintCnt)); i++)
                {
                    if (blPrintStop) break;

                    //Util.PrintLabel(FrameOperation, loadingIndicator, zpl2);                   
                    Util.SetCondition_Thread(tbPrint2_cnt, "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : 0 " + ObjectDic.Instance.GetObjectName("건") + " ]");                    
                    System.Threading.Thread.Sleep(Convert.ToInt32(Util.GetCondition_Thread(nbDelay)) * 1000);                   
                }

                if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        CMM_ZPL_VIEWER2 wndPopup;
                        wndPopup = new CMM_ZPL_VIEWER2(zpl2);
                        wndPopup.Show();
                    }));
                }

                ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);                
            }
        }
       
        private string labelItemsGet()
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = label_code2;

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : 코드=값^코드=값

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        #region sample value 뿌림
                        /*
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();
                        */
                        #endregion

                        #region 화면에서 입력된 값 뿌림                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtInput.Rows[0][item_code].ToString();
                        #endregion

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }
                    }
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable getInputData()
        {
            DataTable dt = new DataTable();
            dt.TableName = "INPUTDATA";
            dt.Columns.Add("ITEM001", typeof(string));
            dt.Columns.Add("ITEM002", typeof(string));
            dt.Columns.Add("ITEM003", typeof(string));
            dt.Columns.Add("ITEM004", typeof(string));
            dt.Columns.Add("ITEM005", typeof(string));
            dt.Columns.Add("ITEM006", typeof(string));
            dt.Columns.Add("ITEM007", typeof(string));
            dt.Columns.Add("ITEM008", typeof(string));
            dt.Columns.Add("ITEM009", typeof(string));
            dt.Columns.Add("ITEM010", typeof(string));
            dt.Columns.Add("ITEM011", typeof(string));
            dt.Columns.Add("ITEM012", typeof(string));

            dt.Columns.Add("ITEM013", typeof(string));
            dt.Columns.Add("ITEM014", typeof(string));
            dt.Columns.Add("ITEM015", typeof(string));
            dt.Columns.Add("ITEM016", typeof(string));
            dt.Columns.Add("ITEM017", typeof(string));
            dt.Columns.Add("ITEM018", typeof(string));
            dt.Columns.Add("ITEM019", typeof(string));
            dt.Columns.Add("ITEM020", typeof(string));
            dt.Columns.Add("ITEM021", typeof(string));

            DataRow dr = dt.NewRow();
            dr["ITEM001"] = Util.GetCondition_Thread (txtReceive2); //VOLVO TORSLANDA MONTERING
            dr["ITEM002"] = Util.GetCondition_Thread(txtDock2); //TCV
            dr["ITEM003"] = Util.GetCondition_Thread(txtAdvice2); //61606292
            dr["ITEM004"] = Util.GetCondition_Thread(txtAdvice2); //61606292
            dr["ITEM005"] = Util.GetCondition_Thread(txtSupplierAddress2); // LG Chem, Ltd
            dr["ITEM006"] = Util.GetCondition_Thread(txtNetWeight2); //115
            dr["ITEM007"] = Util.GetCondition_Thread(txtGrossWeight2); //160
            dr["ITEM008"] = Util.GetCondition_Thread(txtBoxes2); //1
            dr["ITEM009"] = Util.GetCondition_Thread(txtpartNumber2); //31491834
            dr["ITEM010"] = Util.GetCondition_Thread(txtpartNumber2); //31491834
            dr["ITEM011"] = Util.GetCondition_Thread(txtquantity2); //1
            dr["ITEM012"] = Util.GetCondition_Thread(txtquantity2); //1

            dr["ITEM013"] = Util.GetCondition_Thread(txtDescription12); //BATTERY PACK
            dr["ITEM014"] = Util.GetCondition_Thread(txtDescription22); //355V,25.9A,6500Wh(Usable)
            dr["ITEM015"] = Util.GetCondition_Thread(txtSupplierID2); //GE2PB
            dr["ITEM016"] = Util.GetCondition_Thread(txtSupplierID2); //GE2PB
            dr["ITEM017"] = Util.GetCondition_Thread(txtDate2); //D160629
            dr["ITEM018"] = Util.GetCondition_Thread(txtSerial2); //616242017
            dr["ITEM019"] = Util.GetCondition_Thread(txtSerial2); //616242017
            dr["ITEM020"] = Util.GetCondition_Thread(txtBatch2); //616242017
            dr["ITEM021"] = Util.GetCondition_Thread(txtBatch2); //616242017
            dt.Rows.Add(dr);

            return dt;
        }

        private void getZpl(string I_ATTVAL)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("I_LBCD", typeof(string));
                //RQSTDT.Columns.Add("I_PRMK", typeof(string));
                //RQSTDT.Columns.Add("I_RESO", typeof(string));
                //RQSTDT.Columns.Add("I_PRCN", typeof(string));
                //RQSTDT.Columns.Add("I_MARH", typeof(string));
                //RQSTDT.Columns.Add("I_MARV", typeof(string));
                //RQSTDT.Columns.Add("I_ATTVAL", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["I_LBCD"] = label_code2;
                //dr["I_PRMK"] = "Z";
                //dr["I_RESO"] = "203";
                //dr["I_PRCN"] = "1";
                //dr["I_MARH"] = "0";
                //dr["I_MARV"] = "0";
                //dr["I_ATTVAL"] = I_ATTVAL;

                //RQSTDT.Rows.Add(dr);

                ////ITEM001=TEST1^ITEM002=TEST2

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_DESIGN_TEST", "INDATA", "OUTDATA", RQSTDT);

                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: label_code2
                                                     , sATTVAL: I_ATTVAL
                                                     );

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl2 = dtResult.Rows[0]["ZPLSTRING"].ToString();

                    //CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(testZpl);
                    //wndPopup.Show();

                    //Util.PrintLabel(FrameOperation, loadingIndicator, testZpl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setTexBox()
        {
            try
            {
                txtReceive2.Text = "VOLVO TORSLANDA MONTERING";
                txtAdvice2.Text = "31607151";
                txtDock2.Text = " TVV ";
                txtSupplierAddress2.Text = "LG Chem, Ltd";
                txtNetWeight2.Text = "150";
                txtGrossWeight2.Text = "180";
                txtBoxes2.Text = "1";
                txtpartNumber2.Text = "31407014";
                txtquantity2.Text = "1";
                txtSupplierID2.Text = "GE2PB";
                txtSerial2.Text = "312031601";
                txtLogistic12.Text = "logic";
                txtLogistic22.Text = "reference";
                txtDate2.Text = "D160629";
                txtBatch2.Text = "312031601";
                txtDescription12.Text = "BATTERY PACK";
                txtDescription22.Text = "375V, 30Ah, 11,250Wh";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PrintBoxLabel(string sZpl)
        {
            try
            {
                CMM_ZPL_VIEWER2 wndPopup;

                wndPopup = new CMM_ZPL_VIEWER2(sZpl);

                //Util.PrintLabel(FrameOperation, loadingIndicator, sZpl);
                System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);

                wndPopup.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setBacode(object sender)
        {
            try
            {
                TextBox tbBox = (TextBox)sender;

                switch (tbBox.Name)
                {
                    case "txtAdvice":
                        bcAdvice.Text = tbBox.Text;
                        break;
                    case "txtpartNumber":
                        bcpartNumber.Text = tbBox.Text;
                        break;
                    case "txtquantity":
                        bcquantity.Text = tbBox.Text;
                        break;
                    case "txtSerial":
                        bcSerial.Text = tbBox.Text;
                        break;
                    case "txtBatch":
                        bcBatch.Text = tbBox.Text;
                        break;

                    case "txtAdvice2":
                        bcAdvice2.Text = tbBox.Text;
                        break;
                    case "txtpartNumber2":
                        bcpartNumber2.Text = tbBox.Text;
                        break;
                    case "txtquantity2":
                        bcquantity2.Text = tbBox.Text;
                        break;
                    case "txtSupplierID2":
                        bcSupplierID2.Text = tbBox.Text;
                        break;
                    case "txtSerial2":
                        bcSerial2.Text = tbBox.Text;
                        break;
                    case "txtBatch2":
                        bcBatch2.Text = tbBox.Text;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #endregion

        
    }
}
