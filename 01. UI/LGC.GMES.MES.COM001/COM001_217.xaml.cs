/*************************************************************************************
 Created Date : 2018.02.28
      Creator : 오화백
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2018.02.28  DEVELOPER : Initial Created.
  

 
**************************************************************************************/

using C1.WPF;
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
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
namespace LGC.GMES.MES.COM001
{
    public partial class COM001_217 : UserControl, IWorkArea
    {

        #region Declaration & Constructor       
        Util _Util = new Util();
        string _FLOWID = string.Empty;
        private int _tagPrintCount;
        public COM001_217()
        {
            InitializeComponent();
        }

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
            SetEvent();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnInput);
            listAuth.Add(btnOutput);
        
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);            
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitCombo()
        {
            #region 불량창고 입/출고
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboProcid };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboDefectGroup = { cboProcid, cboDefect_Group };
            String[] sFilterArea = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", sFilter: sFilterArea, cbChild: cboDefectGroup);
            cboProcid.IsEnabled = false;

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea, cboProcid };
            _combo.SetCombo(cboEqsgid, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboEquipmentSegmentParent);
            cboEqsgid.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //불량창고
             _combo.SetCombo(cboWhid, CommonCombo.ComboStatus.SELECT,  sCase: "DEFECT_WH_ID");
             cboWhid.SelectedIndex = 1;

            //불량재공구분
            //String[] sFilter2 = { "", "WIP_PRCS_TYPE_CODE" };
            //_combo.SetCombo(cboWIP_QLTY_TYPE_CODE, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

            String[] sFilter2 = { "WIP_PRCS_TYPE_CODE", "WH" };
            _combo.SetCombo(cboWIP_QLTY_TYPE_CODE, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODEATTR");

          

            //불량그룹
            C1ComboBox[] cboProcidParent = { cboProcid };
            _combo.SetCombo(cboDefect_Group, CommonCombo.ComboStatus.ALL, sCase: "POLYMER_DEFECT_GROUP", cbParent: cboProcidParent);


            #endregion

            #region 입출고 이력

            //공정
            C1ComboBox[] cboDefectGroupHist = { cboDefect_GroupHist };
            String[] sFilterAreaHist = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboProcidHist, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", sFilter: sFilterAreaHist, cbChild: cboDefectGroupHist);
            cboProcidHist.IsEnabled = false;

            //불량그룹
            C1ComboBox[] cboProcidParentHist = { cboProcidHist };
            _combo.SetCombo(cboDefect_GroupHist, CommonCombo.ComboStatus.ALL, sCase: "POLYMER_DEFECT_GROUP", cbParent: cboProcidParentHist);


            //불량창고
            _combo.SetCombo(cboWhidHist, CommonCombo.ComboStatus.SELECT, sCase: "DEFECT_WH_ID");
            cboWhidHist.SelectedIndex = 1;


            //구분
            Set_Combo_Defect_ACT(cboDefectAct_Hist);

            #endregion
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>

        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
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

        #region  불량창고 입/출고 

        #region  불량창고 입/출고  조회  : btnSearch_Click()
        //불량창고 재공 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DEFT_WH();
        }
        #endregion

        #region  불량창고 입/출고 입고 팝업 : btnInput_Click(), popupInplut_Closed()

        //입고
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_217_DEFECT_INPUT popupInplut = new COM001_217_DEFECT_INPUT();
                popupInplut.FrameOperation = this.FrameOperation;
                popupInplut.Closed += new EventHandler(popupInplut_Closed);
                grdMain.Children.Add(popupInplut);
                popupInplut.BringToFront();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        //입고 팝업 닫기
        private void popupInplut_Closed(object sender, EventArgs e)
        {
            COM001_217_DEFECT_INPUT popupInplut = sender as COM001_217_DEFECT_INPUT;


            if (popupInplut.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                DEFT_WH();
            }
            this.grdMain.Children.Remove(popupInplut);
        }

        #endregion
        
        #region  불량창고 입/출고 출고 팝업 : btnOutput_Click(), popupoutput_Closed()


        //출고
        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (!OUTPUT_Validation())
                {
                    return;
                }


                DataTable dtdefect = DataTableConverter.Convert(dgDefectList.ItemsSource);

                DataTable OutPutData = new DataTable();
                OutPutData = dtdefect.Clone();
                DataRow newRow = null;

                for (int i = 0; i < dtdefect.Rows.Count; i++)
                {
                    if (dtdefect.Rows[i]["CHK"].ToString() == "1")
                    {
                        newRow = OutPutData.NewRow();
                        newRow["CHK"] = 0;
                        newRow["LOTID"] = Util.NVC(dtdefect.Rows[i]["LOTID"]).ToString();
                        newRow["PJT"] = Util.NVC(dtdefect.Rows[i]["PJT"]).ToString();
                        newRow["PRODID"] = Util.NVC(dtdefect.Rows[i]["PRODID"]).ToString();
                        newRow["MKT_TYPE_NAME"] = Util.NVC(dtdefect.Rows[i]["MKT_TYPE_NAME"]).ToString();
                        newRow["LOTYNAME"] = Util.NVC(dtdefect.Rows[i]["LOTYNAME"]).ToString();
                        newRow["LOTID_RT"] = Util.NVC(dtdefect.Rows[i]["LOTID_RT"]).ToString();
                        newRow["DFCT_RSN_GR_NAME"] = Util.NVC(dtdefect.Rows[i]["DFCT_RSN_GR_NAME"]).ToString();
                        newRow["CAPA_GRD_CODE"] = Util.NVC(dtdefect.Rows[i]["CAPA_GRD_CODE"]).ToString();
                        newRow["WIPQTY"] = Util.NVC(dtdefect.Rows[i]["WIPQTY"]).ToString();
                        newRow["INPUTDATE"] = Util.NVC(dtdefect.Rows[i]["INPUTDATE"]).ToString();
                        newRow["OUTPUTDATE"] = Util.NVC(dtdefect.Rows[i]["OUTPUTDATE"]).ToString();
                        newRow["MKT_TYPE_CODE"] = Util.NVC(dtdefect.Rows[i]["MKT_TYPE_CODE"]).ToString();
                        newRow["LOTTYPE"] = Util.NVC(dtdefect.Rows[i]["LOTTYPE"]).ToString();
                        newRow["DFCT_RSN_GR_ID"] = Util.NVC(dtdefect.Rows[i]["DFCT_RSN_GR_ID"]).ToString();
                        newRow["WH_DFEC_QTY"] = Util.NVC(dtdefect.Rows[i]["WH_DFEC_QTY"]).ToString();
                        newRow["ROUTID"] = Util.NVC(dtdefect.Rows[i]["ROUTID"]).ToString();
                        newRow["PROCID"] = Util.NVC(dtdefect.Rows[i]["PROCID"]).ToString();
                        newRow["WH_DFEC_LOT"] = Util.NVC(dtdefect.Rows[i]["WH_DFEC_LOT"]).ToString();
                        newRow["RESNGR_ABBR_CODE"] = Util.NVC(dtdefect.Rows[i]["RESNGR_ABBR_CODE"]).ToString();
                        newRow["OUTPUT_AFTER_QTY"] = Util.NVC(dtdefect.Rows[i]["OUTPUT_AFTER_QTY"]).ToString();
                        newRow["OUTPUTQTY"] = Util.NVC(dtdefect.Rows[i]["OUTPUTQTY"]).ToString();
                        newRow["EQSGID"] = Util.NVC(dtdefect.Rows[i]["EQSGID"]).ToString();
                        OutPutData.Rows.Add(newRow);

                    }
                }

                COM001_217_DEFECT_OUTPUT popupoutput = new COM001_217_DEFECT_OUTPUT();
                popupoutput.FrameOperation = this.FrameOperation;
                object[] parameters = new object[1];
                parameters[0] = OutPutData;
                C1WindowExtension.SetParameters(popupoutput, parameters);
                popupoutput.Closed += new EventHandler(popupoutput_Closed);
                grdMain.Children.Add(popupoutput);
                popupoutput.BringToFront();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        //출고팝업 닫기
        private void popupoutput_Closed(object sender, EventArgs e)
        {
            COM001_217_DEFECT_OUTPUT popupoutput = sender as COM001_217_DEFECT_OUTPUT;


            if (popupoutput.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                DEFT_WH();
            }
            this.grdMain.Children.Remove(popupoutput);
        }

        #endregion

        #region  불량창고 입/출고 시트 팝업 : btnPrint_Click(), popupCartPrint_Closed()

        /// <summary>
        ///  시트 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (dgDefectList.ItemsSource == null)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgDefectList.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            _tagPrintCount = drSelect.Length;

            foreach (DataRow drPrint in drSelect)
            {
                POLYMER_TagPrint(drPrint);
            }
        }
        /// <summary>
        /// 시트 발행 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #region  불량창고 입/출고  LOSS 팝업 : btnLoss_Click(), popupInboxLoss_Closed()
        /// <summary>
        /// LOSS처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoss_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInboxLoss()) return;

            InboxLoss();
        }
        /// <summary>
        ///  LOSS 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupInboxLoss_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_DEFECT_LOSS popup = sender as CMM_POLYMER_FORM_CART_DEFECT_LOSS;

            // 재조회
            DEFT_WH();

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

        #region  불량창고 입/출고  Cell 등록 팝업 : btnCellInbput_Click(), popupCell_Closed()
        /// <summary>
        /// Cell 등록 팝업 열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCellInbput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_InputCell())
                {
                    return;
                }
                int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgDefectList, "CHK");
                CMM_POLYMER_CELL_INPUT popupCell = new CMM_POLYMER_CELL_INPUT();

                popupCell.CTNR_DEFC_LOT_CHK = "N";  //대차ID/불량그룹 체크
                popupCell.FrameOperation = this.FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = Util.NVC(dgDefectList.GetCell(idxchk, dgDefectList.Columns["LOTID"].Index).Value);
                C1WindowExtension.SetParameters(popupCell, parameters);
                popupCell.Closed += new EventHandler(popupCell_Closed);
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupCell);
                        popupCell.BringToFront();
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
        /// Cell 등록 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupCell_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_CELL_INPUT popup = sender as CMM_POLYMER_CELL_INPUT;

            // 재조회
            DEFT_WH();

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

        #region 입출고 이력

        #region 입출고 이력 조회 : btnSearch_Hist_Click()
        //이력 조회
        private void btnSearch_Hist_Click(object sender, RoutedEventArgs e)
        {
            DefectHist();
        }
        #endregion

        #region 입출고 이력 대차 ID로 조회 : txtCtnrIDHist_KeyDown()
        //대차ID로 조회
        private void txtCtnrIDHist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtCtnrIDHist.Text)) return;
                DefectHist();
                txtCtnrIDHist.Text = string.Empty;
            }
        }
        #endregion

        #region 입출고 이력 자동 영문 : txtCtnrIDHist_GotFocus()
        //대차ID로 조회
        private void txtCtnrIDHist_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region [인계이력조회] - 기간 선택시 이벤트
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




        #endregion

        #endregion

        #endregion

        #region Method
        
        #region  불량창고 입/출고 

        #region  불량창고 입/출고  조회 : DEFT_WH()
        public void DEFT_WH()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("WH_ID", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                dtRqst.Columns.Add("DFCT_LOT", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = Util.GetCondition(cboProcid, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEqsgid, bAllNull: true);
                //dr["EQSGID"] = Util.GetCondition(cboEqsgid, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                //if (dr["EQSGID"].Equals("")) return;
                dr["WH_ID"] = Util.GetCondition(cboWhid, "SFU4604"); // 불량창고를 선택하세요.
                if (dr["WH_ID"].Equals("")) return;
                //dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboWhid, "SFU4606"); // 불량재공구분을 선택하세요.
                //if (dr["WIP_QLTY_TYPE_CODE"].Equals("")) return;
                dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboWIP_QLTY_TYPE_CODE, bAllNull: true);
                dr["PJT_NAME"] = txtPJT.Text;
                dr["PRODID"] = txtProdID.Text;
                dr["LOTID_RT"] = txtLotID_RT.Text;
                dr["DFCT_RSN_GR_ID"] = Util.GetCondition(cboDefect_Group, bAllNull: true);
                dr["DFCT_LOT"] = txtDefectGroup_Lot.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_DEFC_WH_LIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    dgDefectList.ItemsSource = DataTableConverter.Convert(dtRslt);

                    return;
                }

                Util.GridSetData(dgDefectList, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region  불량창고 입/출고  시트 팝업 열기 : POLYMER_TagPrint()

        private void POLYMER_TagPrint(DataRow dr)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;
            popupCartPrint.DefectGroupLotYN = "Y";


            object[] parameters = new object[5];
            parameters[0] = LoginInfo.CFG_PROC_ID;
            parameters[1] = string.Empty;
            parameters[2] = dr["LOTID"];
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
        #endregion

        #region  불량창고 입/출고 Loss 팝업 열기 : InboxLoss();
        /// <summary>
        /// Inbox Loss처리 팝업
        /// </summary>
        private void InboxLoss()
        {
            CMM_POLYMER_FORM_CART_DEFECT_LOSS popupInboxLoss = new CMM_POLYMER_FORM_CART_DEFECT_LOSS();
            popupInboxLoss.FrameOperation = this.FrameOperation;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgDefectList, "CHK");

            object[] parameters = new object[5];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[idx].DataItem, "PROCID")).ToString(); //공정ID
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[idx].DataItem, "PROCNAME")).ToString(); //공정명
            parameters[2] = string.Empty; //설비 정보
            parameters[3] = string.Empty; //대차ID

            DataTable dt = new DataTable();
            dt.Columns.Add("CTNR_ID", typeof(string));
            dt.Columns.Add("ASSY_LOTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("RESNGRNAME", typeof(string));
            dt.Columns.Add("CAPA_GRD_CODE", typeof(string));
            dt.Columns.Add("CELL_QTY", typeof(decimal));
            dt.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = dt.NewRow();
            dr["CTNR_ID"] = string.Empty;
            dr["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[idx].DataItem, "LOTID_RT")).ToString();
            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[idx].DataItem, "LOTID")).ToString();
            dr["RESNGRNAME"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[idx].DataItem, "DFCT_RSN_GR_NAME")).ToString();
            dr["CAPA_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[idx].DataItem, "CAPA_GRD_CODE")).ToString();
            dr["CELL_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[idx].DataItem, "WIPQTY")).ToString();
            dr["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[idx].DataItem, "WIPSEQ")).ToString();
            dt.Rows.Add(dr);

            parameters[4] = dr;

            C1WindowExtension.SetParameters(popupInboxLoss, parameters);

            popupInboxLoss.Closed += new EventHandler(popupInboxLoss_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupInboxLoss);
                    popupInboxLoss.BringToFront();
                    break;
                }
            }
        }
        #endregion

        #region  불량창고 입/출고 Validation
        /// <summary>
        /// 출고 팝업 Validation
        /// </summary>
        /// <returns></returns>
        private bool OUTPUT_Validation()
        {

            if (dgDefectList.Rows.Count == 1)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgDefectList, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }

            int rowIndex = _Util.GetDataGridCheckFirstRowIndex(dgDefectList, "CHK");
            string _WipPrcs = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[rowIndex].DataItem, "WIP_PRCS_TYPE_CODE")).ToString();
            string _Eqsgid = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[rowIndex].DataItem, "EQSGID")).ToString();

            for (int i = 0; i < dgDefectList.Rows.Count - 2; i++)
            {
                if (DataTableConverter.GetValue(dgDefectList.Rows[i].DataItem, "CHK").ToString() == "1")
                {
                    if (_WipPrcs != Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[i].DataItem, "WIP_PRCS_TYPE_CODE")).ToString())
                    {
                        Util.MessageValidation("SFU4625");//불량재공구분이 틀립니다.
                        return false;
                    }
                    if (_Eqsgid != Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[i].DataItem, "EQSGID")).ToString())
                    {
                        Util.MessageValidation("SFU4168");//라인정보가 틀립니다.
                        return false;
                    }
                }

            }




            return true;
        }

        /// <summary>
        /// LOSS BOX Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationInboxLoss()
        {
            if (dgDefectList.Rows.Count == 1)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgDefectList.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (dr.Length > 1)
            {
                // 한행만 선택 가능 합니다.
                Util.MessageValidation("SFU4023");
                return false;
            }


            return true;
        }


        /// <summary>
        /// Cell 등록 Validation
        /// </summary>
        /// <returns></returns>
        private bool Validation_InputCell()
        {


            if (dgDefectList.Rows.Count == 1)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgDefectList.ItemsSource).Select("CHK = '1'");


            int CheckCount = 0;

            for (int i = 0; i < dgDefectList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }


            return true;
        }

        #endregion

        #endregion

        #region 입출고 이력

        #region 입출고 이력 조회 : DefectHist()
        //이력조회
        private void DefectHist()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("DFCT_LOT", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                dtRqst.Columns.Add("WH_ID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["PROCID"] = Util.GetCondition(cboProcidHist, bAllNull: true);
                dr["PJT_NAME"] = txtPJTHist.Text;
                dr["PRODID"] = txtProdIDHist.Text;
                dr["LOTID_RT"] = txtLotID_RTHist.Text;
                dr["DFCT_LOT"] = Util.GetCondition(txtDefectGroup_LotHist);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DFCT_RSN_GR_ID"] = Util.GetCondition(cboDefect_GroupHist, bAllNull: true);
                dr["WH_ID"] = Util.GetCondition(cboWhidHist, "SFU4604"); // 불량창고를 선택하세요.
                if (dr["WH_ID"].Equals("")) return;
                dr["ACTID"] = Util.GetCondition(cboDefectAct_Hist, bAllNull: true);
                if (txtCtnrIDHist.Text != string.Empty)
                {
                    dr["CTNR_ID"] = txtCtnrIDHist.Text;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_DEFC_WH_LIST_HIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.gridClear(dgDefectListHist);

                    return;
                }
                Util.gridClear(dgDefectListHist);
                Util.GridSetData(dgDefectListHist, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        #region 입출고 이력 폐기 상태 : Set_Combo_Defect_ACT()
        //폐기상태 콤보
        private void Set_Combo_Defect_ACT(C1ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                DataRow row = dt.NewRow();

                row = dt.NewRow();
                row["CBO_CODE"] = "";
                row["CBO_NAME"] = "ALL";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "GO_CTNR_INTO_WAREHOUSE";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("대차 창고 입고");
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "GO_CTNR_OUT_WAREHOUSE";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("대차 창고 출고");
                dt.Rows.Add(row);

                cbo.ItemsSource = DataTableConverter.Convert(dt);

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }
        #endregion

        #endregion

        #endregion


    }
}
