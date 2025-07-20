/*************************************************************************************
 Created Date : 2019.02.27
      Creator : 손우석
   Decription : 재고조사
--------------------------------------------------------------------------------------
 [Change History]
  2019.02.27 손우석 CSR ID 3936725 팩공정 PDA 재고실사 기능 요청 건 [요청번호] C20190228_36725
  2019.04.02 손우석 그리드 전체 체크 오류 기능 수정
  2019.04.23 손우석 CSR ID:3989180 PACK GMES 기능개선 CSR 건 | [요청번호]C20190507_89180 
  2019.05.20 손우석 CSR ID:3996705 PACK GMES 기능개선 CSR 건 | [요청번호]C20190507_89180 | [서비스번호]3996705
  2019.12.11 손우석 SM CMI Pack 메시지 다국어 처리 요청
  2020.06.18 김준겸 재고 실사(Pack) Excel Upload/Down 기능 추가 CSR 건    | [요청번호]C20200602-000006
  2024.05.24 윤지해 (수정사항 없음) NERP 대응 프로젝트-차수마감 취소 등 개발 범위에서 제외(현재 미사용)
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Management;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_039.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_039 : UserControl, IWorkArea
    {
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        CommonCombo _combo = new CommonCombo();
        Util _Util = new Util();

        private string _sEqsgID = string.Empty;
        private string _sProcID = string.Empty;
        private string _sProdID = string.Empty;
        private string _sElecType = string.Empty;
        private string _sPrjtName = string.Empty;

        private const string _sLOTID = "LOTID";
        private const string _sBOXID = "BOXID";

        DataView _dvSTCKCNT { get; set; }

        string _sSTCK_CNT_CMPL_FLAG = string.Empty;

        #region Declaration & Constructor
        #region <Hold 변수>
        int TotalRow;
        #endregion

        public PACK001_039()
        {
            InitializeComponent();
        }
        #endregion Declaration & Constructor         

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 동
            // 동의 경우, 우선 데이터 생성을 위해서, 우선 처리
            SetAreaByAreaType();

            InitCombo();

            //cboAreaByAreaType.SelectedValueChanged += cboAreaByAreaType_SelectedValueChanged;
            //ldpMonthShot.DataContextChanged += ldpMonthShot_DataContextChanged;
        }
        /*  우선순위 뒤로 김준겸
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
        }
        */
        private void InitCombo()
        {




            /* 
             * 기존 쓰레드 오류로 인해서, 제외 처리 신규 코딩 
            //라인
            SetCboEQSG(cboEquipmentSegment, cboAreaByAreaType);

            //공정
            SetProcessCombo(cboProc, cboAreaByAreaType);

            //모델
            SetcboProductModel(cboEquipmentSegment, cboAreaByAreaType);
            */

            //cboProduct

            //cboProductPilot

            //제품분류
            //SetcboPrdtClass();
            //C1ComboBox cboSHOPID = new C1ComboBox();
            //cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;

            //C1ComboBox[] ccboPrdtClassChild = { cboProduct };
            //C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAreaByAreaType };
            //_combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.ALL, cbChild: ccboPrdtClassChild, cbParent: cboPrdtClassParent);

            //제품
            //SetcboProduct();

            CommonCombo _combo = new CommonCombo();
            //콤보 없는애 사용용
            C1ComboBox cboSHOP = new C1ComboBox();
           

            C1ComboBox[] cboEqsgParentd = { cboAreaByAreaType };
            string[] sEqsgFilter = { Area_Type.PACK };
            C1ComboBox[] cboEqsgChild = { cboProc, cboProductModel, cboPrdtClass, cboProduct };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEqsgParentd, cbChild: cboEqsgChild, sFilter: sEqsgFilter, sCase: "EQSGID_PACK");

            C1ComboBox[] cboProcessParent = { cboAreaByAreaType, cboEquipmentSegment };
            C1ComboBox[] cboProcessChild   = {cboPrdtClass, cboProduct};
            _combo.SetCombo(cboProc, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, cbChild : cboProcessChild ,sCase: "PROCESSWITHAREANOLOGININFO");


            C1ComboBox[] cboModelParent = { cboAreaByAreaType, cboEquipmentSegment };
            C1ComboBox[] cboModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboModelParent, cbChild: cboModelChild, sCase: "PRJ_MODEL_AUTH");


            C1ComboBox[] cboPrdtClassParent = { cboEquipmentSegment, cboAreaByAreaType, cboProc };
            C1ComboBox[] cboPrdtClassChild = { cboProduct };
            string[] sPrdtClassFileter = { Area_Type.PACK , LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.ALL, cbParent: cboPrdtClassParent, cbChild: cboPrdtClassChild, sFilter:sPrdtClassFileter, sCase: "cboPrdtClassByProcId");

            C1ComboBox[] cboProductParent = { cboSHOP, cboAreaByAreaType, cboEquipmentSegment, cboProc, cboProductModel, cboPrdtClass };
            C1ComboBox[] cboProductChild = { };
            string[] sProductFileter = { Area_Type.PACK };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, cbChild: cboProductChild, sFilter: sProductFileter, sCase: "PRODUCTMULTI");

            object[] objStockSeqShotParent = { cboAreaByAreaType, ldpMonthShot };
            String[] sFilterAll = { "" };
            _combo.SetComboObjParent(cboStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterAll);


            ldpMonthShot.SelectedDataTimeChanged += ldpMonthShot_SelectedDataTimeChanged;

            this.Loaded -= C1Window_Loaded;


        }
        #endregion Initialize

        #region Event

        #region Combo

        #region [ 콤보 변경 으로 인한 EvET 제외 처리 ]

        private void cboAreaByAreaType_SelectedValueChanged_1(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //SetStckCntGrShotCombo(cboStckCntGrShot, cboAreaShot);
                    SetCboEQSG(cboEquipmentSegment, cboAreaByAreaType);

                    SetProcessCombo(cboProc, cboAreaByAreaType);

                    SetcboProductModel(cboEquipmentSegment, cboAreaByAreaType);

                    //SetcboPrdtClass();

                    SetcboProduct();

                    //cboProductModel.isAllUsed = true;
                    //cboProduct.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetProcessCombo(cboProc, cboAreaByAreaType);

                    SetcboProductModel(cboEquipmentSegment, cboAreaByAreaType);

                    //SetcboPrdtClass();

                    SetcboProduct();

                    //cboProductModel.isAllUsed = true;
                    //cboProduct.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }

        private void cboProc_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProductModel(cboEquipmentSegment, cboAreaByAreaType);

                    //SetcboPrdtClass();

                    SetcboProduct();

                    //cboProductModel.isAllUsed = true;
                    //cboProduct.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }



        private void cboProductModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //SetcboPrdtClass();
                    SetcboProduct();
                    //cboProduct.isAllUsed = true;
                }));
            }
            catch
            {
            }
        }

        private void cboPrdtClass_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProduct();
                    //cboProduct.isAllUsed = true;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion Combo

        #region Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (cboAreaByAreaType.SelectedValue.ToString() == "")
            //{
            //    //동은필수입니다.
            //    Util.MessageValidation("SFU3203");
            //    return;
            //}

            SetListStock();
        }

        private void btnExclude_RSLT_Click(object sender, RoutedEventArgs e)
        {
            int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListStock, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return;
            }

            if (string.IsNullOrEmpty(txtExcludeNote_RSLT.Text.Trim()))
            {
                Util.MessageValidation("SFU1590");  //비고를 입력해 주세요.
                return;
            }

            //전산재고 LOTID를 제외 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4212"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    string sSTCK_CNT_EXCL_FLAG = "Y";
                    Exclude_RSLT();
                }
            }
            );
        }

        private void btnDegreeClose_Click(object sender, RoutedEventArgs e)
        {
            if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            {
                Util.MessageValidation("SFU3499"); //마감된 차수입니다.
                return;
            }

            //마감하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1276"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DegreeClose();
                }
            }
            );
        }

        private void btnDegreeAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_011_STOCKCNT_START wndSTOCKCNT_START = new COM001_011_STOCKCNT_START();
                wndSTOCKCNT_START.FrameOperation = FrameOperation;

                if (wndSTOCKCNT_START != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = Convert.ToString(cboAreaByAreaType.SelectedValue);
                    Parameters[1] = ldpMonthShot.SelectedDateTime;

                    C1WindowExtension.SetParameters(wndSTOCKCNT_START, Parameters);

                    wndSTOCKCNT_START.Closed += new EventHandler(wndSTOCKCNT_START_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_START.ShowModal()));
                    wndSTOCKCNT_START.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Button



        #region Grid
        private void chkHeader_RSLT_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
            object objRowIdx = dgListStock.Rows[idx].DataItem;

            if (objRowIdx != null)
            {
                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    if (DataTableConverter.Convert(dgListStock.ItemsSource).Select("CHK = 'True' AND STCK_CNT_EXCL_FLAG <> '" + Util.NVC(DataTableConverter.GetValue(dgListStock.Rows[idx].DataItem, "STCK_CNT_EXCL_FLAG")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4549"); //동일한 재고실사 제외여부만 선택이 가능합니다.
                        DataTableConverter.SetValue(dgListStock.Rows[idx].DataItem, "CHK", false);
                        return;
                    }

                    DataTableConverter.SetValue(dgListStock.Rows[idx].DataItem, "CHK", true);

                    //전산재고 제외/제외취소 버튼 Display
                    SetExcludeDisplay(Util.NVC(DataTableConverter.GetValue(objRowIdx, "STCK_CNT_EXCL_FLAG")));
                }
            }
        }

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

        private void dgListStock_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //2019.04.02
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (string.IsNullOrEmpty(e.Column.Name) == false)
            //    {
            //        if (e.Column.Name.Equals("CHK"))
            //        {
            //            pre.Content = chkAll;
            //            e.Column.HeaderPresenter.Content = pre;
            //            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
            //            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
            //            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
            //            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
            //        }
            //    }
            //}));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                // 동일한 물류단위만 전체 선택 가능하도록
                if (dgListStock.GetRowCount() > 0)
                {
                    if (DataTableConverter.Convert(dgListStock.ItemsSource).Select("STCK_CNT_EXCL_FLAG <> '" + Util.NVC(DataTableConverter.GetValue(dgListStock.Rows[0].DataItem, "STCK_CNT_EXCL_FLAG")) + "'").Length >= 1)
                    {
                        Util.MessageValidation("SFU4550"); //동일한 재고실사 제외여부만 전체선택이 가능합니다.
                        chkAll.IsChecked = false;
                        return;
                    }
                }

                for (int inx = 0; inx < dgListStock.GetRowCount(); inx++)
                {
                    DataTableConverter.SetValue(dgListStock.Rows[inx].DataItem, "CHK", true);
                }

                //2019.04.02
                //전산재고 제외/제외취소 버튼 Display
                //SetExcludeDisplay(Util.NVC(DataTableConverter.GetValue(dgListStock.Rows[0].DataItem, "STCK_CNT_EXCL_FLAG")));
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int inx = 0; inx < dgListStock.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgListStock.Rows[inx].DataItem, "CHK", false);
            }
        }
        #endregion Grid

        private void SetExcludeDisplay(string sSTCK_CNT_EXCL_FLAG)
        {
            if (sSTCK_CNT_EXCL_FLAG.Equals("N"))
            {
                tblExcludeNote_RSLT.Visibility = Visibility.Visible;
                txtExcludeNote_RSLT.Visibility = Visibility.Visible;
                btnExclude_RSLT.Visibility = Visibility.Visible;
            }
            else
            {
                tblExcludeNote_RSLT.Visibility = Visibility.Collapsed;
                txtExcludeNote_RSLT.Visibility = Visibility.Collapsed;
                btnExclude_RSLT.Visibility = Visibility.Collapsed;
            }
        }

        //2019.04.02
        private void chkHeaderAll_RSLT_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgListStock);
        }
        private void chkHeaderAll_RSLT_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgListStock);
        }

        private void wndSTOCKCNT_START_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_011_STOCKCNT_START window = sender as COM001_011_STOCKCNT_START;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SetStockSeq();
                    //CommonCombo _combo = new CommonCombo();
                    //_combo.SetCombo(cboStockSeqShot);
                    //_combo.SetCombo(cboStockSeqUpload);
                    //_combo.SetCombo(cboStockSeqCompare);

                    //Util.gridClear(dgListShot);
                    //Util.gridClear(dgListStock);
                    //Util.gridClear(dgListCompare);
                    //Util.gridClear(dgListCompareDetail);

                    //SetListShot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Event


        //ALLL 제거 처리
        /// <summary>
        /// 동에 대한 구분은,
        /// Pack의 특성상, CWA만은 DB 분리로 인해서, 
        /// 조회를 할수 잇는 동만 표시하기 위해서 분기 처리 
        /// </summary>
        #region Method
        private void SetAreaByAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";
                    dt.Columns.Add("CBO_CODE", typeof(string));
                    dt.Columns.Add("CBO_NAME", typeof(string));

                    DataRow dtDr = dt.NewRow();
                    dtDr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                    dtDr["CBO_NAME"] = LoginInfo.CFG_AREA_NAME;
                    dt.Rows.Add(dtDr);

                    cboAreaByAreaType.ItemsSource = DataTableConverter.Convert(dt);
                    cboAreaByAreaType.SelectedIndex = 0;
                }
                else
                {
                    cboAreaByAreaType.ItemsSource = DataTableConverter.Convert(dtResult);
                    cboAreaByAreaType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCboEQSG(C1ComboBox cboEqsg, C1ComboBox cboArea)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue ?? null;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dRow = dtResult.NewRow();

                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = null;
                dtResult.Rows.InsertAt(dRow, 0);

                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

                cboEquipmentSegment.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void SetEqsgCombo(MultiSelectionBox cboEqsg, MultiSelectionBox cboProc, C1ComboBox cboArea)
        //{
        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("PROCID", typeof(string));
        //        RQSTDT.Columns.Add("AREAID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["PROCID"] = cboProc.SelectedItemsToString ?? null;
        //        dr["AREAID"] = cboArea.SelectedValue ?? null;
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_EQSG_CBO", "RQSTDT", "RSLTDT", RQSTDT);

        //        cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

        //        if (dtResult.Rows.Count > 0)
        //        {
        //            cboEqsg.CheckAll();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        private void SetProcessCombo(C1ComboBox cboProc, C1ComboBox cboArea)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue ?? null;
                dr["STCK_CNT_GR_CODE"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_PROC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dRow = dtResult.NewRow();

                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = null;
                dtResult.Rows.InsertAt(dRow, 0);

                cboProc.ItemsSource = DataTableConverter.Convert(dtResult);

                cboProc.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProductModel(C1ComboBox cboEqsg, C1ComboBox cboArea)
        {
            try
            {
                //string sSelectedValue = cboProductModel.SelectedItemsToString;
                //string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = cboArea.SelectedValue ?? null;
                dr["EQSGID"] = cboEqsg.SelectedValue ?? null;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dRow = dtResult.NewRow();

                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = null;
                dtResult.Rows.InsertAt(dRow, 0);

                cboProduct.ItemsSource = DataTableConverter.Convert(dtResult);

                cboProductModel.ItemsSource = DataTableConverter.Convert(dtResult);

                cboProductModel.SelectedIndex = 0;


                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                //    {
                //        for (int j = 0; j < sSelectedList.Length; j++)
                //        {
                //            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                //            {
                //                cboProductModel.Check(i);
                //                break;
                //            }
                //        }
                //    }
                //    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                //    {
                //        cboProductModel.Check(i);
                //        break;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboPrdtClass()
        {
            try
            {
                //string sSelectedValue = cboPrdtClass.SelectedItemsToString;
                //string[] sSelectedList = sSelectedValue.Split(',');

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                //RQSTDT.Columns.Add("PROCID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //dr["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                //dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                //dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                ////dr["PROCID"] = cboProc.SelectedValue.ToString() == "" ? null : cboProc.SelectedValue.ToString();
                //dr["PROCID"] = null;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //cboPrdtClass.ItemsSource = DataTableConverter.Convert(dtResult);

                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                //    {
                //        for (int j = 0; j < sSelectedList.Length; j++)
                //        {
                //            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                //            {
                //                cboPrdtClass.Check(i);
                //                break;
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProduct()
        {
            try
            {
                //string sSelectedValue = cboProduct.SelectedItemsToString;
                //string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : cboAreaByAreaType.SelectedValue.ToString();
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : cboEquipmentSegment.SelectedValue.ToString();
                //dr["PROCID"] = cboProc.SelectedValue.ToString() == "" ? null : cboProc.SelectedValue.ToString();
                //dr["MODLID"] = cboProductModel.SelectedItemsToString == "" ? null : cboProductModel.SelectedItemsToString;
                dr["MODLID"] = Util.NVC(cboProductModel.SelectedValue) == "" ? null : cboProductModel.SelectedValue.ToString();
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = Util.NVC(cboPrdtClass.SelectedValue) == "" ? null : cboPrdtClass.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dRow = dtResult.NewRow();

                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = null;
                dtResult.Rows.InsertAt(dRow, 0);

                cboProduct.ItemsSource = DataTableConverter.Convert(dtResult);

                cboProduct.SelectedIndex = 0;

                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                //    {
                //        for (int j = 0; j < sSelectedList.Length; j++)
                //        {
                //            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                //            {
                //                cboProduct.Check(i);
                //                break;
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetListStock()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRJTID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int16));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : cboAreaByAreaType.SelectedValue.ToString();

                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);

                if (!Util.GetCondition(cboStockSeqShot, "SFU2958").Equals("")) //차수는필수입니다.
                {
                    dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
                }
                //if (dr["STCK_CNT_SEQNO"].ToString().Equals("")) return;

                if (string.IsNullOrEmpty(txtLotId.Text.Trim()))
                {
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : cboEquipmentSegment.SelectedValue.ToString();
                    dr["PROCID"] = Util.NVC(cboProc.SelectedValue) == "" ? null : cboProc.SelectedValue.ToString();
                    dr["PRODID"] = Util.NVC(cboProduct.SelectedValue) == "" ? null : cboProduct.SelectedValue.ToString();
                    dr["PRDT_CLSS_CODE"] = Util.NVC(cboPrdtClass.SelectedValue) == "" ? null : cboPrdtClass.SelectedValue.ToString();
                    dr["PRJTID"] = Util.NVC(cboProductModel.SelectedValue) == "" ? null : cboProductModel.SelectedValue.ToString();
                    //int iEqsgUploadItemCnt = (cboEqsgUpload.ItemsSource == null ? 0 : ((DataView)cboEqsgUpload.ItemsSource).Count);
                    //int iEqsgUploadSelectedCnt = (cboEqsgUpload.ItemsSource == null ? 0 : cboEqsgUpload.SelectedItemsToString.Split(',').Length);
                    //int iProcUploadItemCnt = (cboProcUpload.ItemsSource == null ? 0 : ((DataView)cboProcUpload.ItemsSource).Count);
                    //int iProcUploadSelectedCnt = (cboProcUpload.ItemsSource == null ? 0 : cboProcUpload.SelectedItemsToString.Split(',').Length);

                    //if (cboProc.SelectedValue.ToString() == null)
                    //{

                    //}
                    //else
                    //{
                    //    dr["PROCID"] = cboProc.SelectedValue.ToString(); //Util.NVC(cboProc.SelectedValue);
                    //}


                    //if (string.IsNullOrEmpty(cboProduct.SelectedValue.ToString()))
                    //{

                    //}
                    //else
                    //{
                    //    dr["PRODID"] = cboProduct.SelectedValue.ToString(); //Util.NVC(cboProduct.SelectedValue);
                    //}


                    //if (string.IsNullOrEmpty(cboPrdtClass.SelectedValue.ToString()))
                    //{

                    //}
                    //else
                    //{
                    //    dr["PRDT_CLSS_CODE"] = cboPrdtClass.SelectedValue.ToString(); //Util.NVC(cboPrdtClass.SelectedValue);
                    //}


                    //if (string.IsNullOrEmpty(cboProductModel.SelectedValue.ToString()))
                    //{

                    //}
                    //else
                    //{
                    //    dr["PRJTID"] = cboProductModel.SelectedValue.ToString(); //Util.NVC(cboProductModel.SelectedValue);
                    //}
                }
                else
                {
                    dr["LOTID"] = Util.NVC(txtLotId.Text);
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_PACK_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListStock, dtRslt, FrameOperation);
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

        private void Exclude_RSLT()
        {
            try
            {
                //INDATA
                this.dgListStock.EndEdit();
                this.dgListStock.EndEditRow(true);

                DataSet inData = new DataSet();
                DataTable RQSTDT = inData.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("REAL_WIP_QTY", typeof(decimal));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataTable dtRSLT = ((DataView)dgListStock.ItemsSource).Table;
                for (int i = 0; i < dtRSLT.Rows.Count; i++)
                {
                    if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
                        dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
                        dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
                        dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
                        dr["REAL_WIP_QTY"] = string.IsNullOrEmpty(dtRSLT.Rows[i]["WIP_QTY"].ToString()) ? 0 : Convert.ToDecimal(dtRSLT.Rows[i]["WIP_QTY"].ToString());
                        dr["USERID"] = LoginInfo.USERID;
                        dr["USE_FLAG"] = "N";

                        RQSTDT.Rows.Add(dr);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_RSLT", "RQSTDT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상처리되었습니다.

                        SetListStock();
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
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_UPD_STCK_CNT_RSLT", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void DegreeClose()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboStockSeqShot));
                dr["AREAID"] = Util.GetCondition(cboAreaByAreaType, "SFU3203"); //동은필수입니다.
                dr["USERID"] = LoginInfo.USERID;

                if (dr["AREAID"].Equals("")) return;

                RQSTDT.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT", "INDATA", null, RQSTDT);

                //_combo.SetCombo(cboStockSeqShot);
                //_combo.SetCombo(cboStockSeqUpload);
                //_combo.SetCombo(cboStockSeqCompare);
                SetStockSeq();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataRow CellGridAdd(DataTable dt)
        {
            try
            {
                if (TotalRow == 0)
                {
                    Util.GridSetData(dgListStock, dt, FrameOperation);
                    TotalRow = dt.Rows.Count;
                    //++TotalRow;
                    return null;
                }

                DataRow dr = dt.NewRow();
                Util.DataGridRowAdd(dgListStock, 1);

                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "CHK", dt.Rows[0]["CHK"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "PRODID", dt.Rows[0]["PRODID"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "WOID", dt.Rows[0]["WOID"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "PRODNAME", dt.Rows[0]["PRODNAME"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "EQSGID", dt.Rows[0]["EQSGID"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "EQSGNAME", dt.Rows[0]["EQSGNAME"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "PROCNAME", dt.Rows[0]["PROCNAME"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "WIPSNAME", dt.Rows[0]["WIPSNAME"].ToString());
                DataTableConverter.SetValue(dgListStock.Rows[TotalRow].DataItem, "BOXID", dt.Rows[0]["BOXID"].ToString());


                ++TotalRow;

                //Util.SetTextBlockText_DataGridRowCount(tbCellInput_cnt, Convert.ToString(TotalRow));

                return dr;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }


        #endregion Method

        private void cboStockSeqShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _dvSTCKCNT = cboStockSeqShot.ItemsSource as DataView;
            txtStckCntCmplFlagShot.Text = string.Empty;

            string sStckCntSeq = cboStockSeqShot.Text;
            if (_dvSTCKCNT != null && _dvSTCKCNT.Count != 0)
            {
                _dvSTCKCNT.RowFilter = "CBO_NAME = '" + sStckCntSeq + "'";
                txtStckCntCmplFlagShot.Text = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_NOTE"].ToString();
                _sSTCK_CNT_CMPL_FLAG = _dvSTCKCNT.ToTable().Rows[0]["STCK_CNT_CMPL_FLAG"].ToString();

                _dvSTCKCNT.RowFilter = null;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgListStock);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExcelLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtDataGrid = new DataTable();
                dtDataGrid.TableName = "dtDataTable";

                dtDataGrid.Columns.Add("CHK", typeof(string));
                dtDataGrid.Columns.Add("AREANAME", typeof(string));
                dtDataGrid.Columns.Add("PROCNAME", typeof(string));
                dtDataGrid.Columns.Add("EQSGNAME ", typeof(string));
                dtDataGrid.Columns.Add("PRODID", typeof(string));
                dtDataGrid.Columns.Add("PRJT_NAME", typeof(string));
                dtDataGrid.Columns.Add("LOTID", typeof(string));
                dtDataGrid.Columns.Add("STCK_CNT_SEQNO", typeof(Int32));
                dtDataGrid.Columns.Add("WIP_QTY", typeof(Int32));
                dtDataGrid.Columns.Add("FLAG", typeof(string));

                TabStckCntExcelImportEditor tsc = new TabStckCntExcelImportEditor(dtDataGrid);
                DataTable dtChild = new DataTable();

                tsc.FrameOperation = this.FrameOperation;
                //frm.FormClosed -= frm.FormClosed;

                tsc.FormClosed += delegate ()
                {
                    if (tsc.DialogResult.Equals(MessageBoxResult.OK))
                    {
                        TotalRow = 0;
                        dgListStock.ItemsSource = null;
                        CellGridAdd(tsc.dtIfMethod);
                    }
                };

                tsc.ShowModal();
                tsc.CenterOnScreen();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cellGridAdd(int TotalRow)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dgListStock)
        {
            dgListStock.BeginNewRow();
            dgListStock.EndNewRow(true);
        }


        //차수 마감, 추가시 콤보 생성
        private void SetStockSeq()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : cboAreaByAreaType.SelectedValue.ToString();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCKCNT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0)
                {
                    cboStockSeqShot.ItemsSource = null;
                    cboStockSeqShot.ItemsSource = DataTableConverter.Convert(dtResult);
                    cboStockSeqShot.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void ldpMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            SetStockSeq();
        }



        //2차 오픈시 주석 해제하기
        //private void cboAreaShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{

        //}

        //private void cboStckCntGrShot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{

        //}

        //private void cboProcShot_SelectionChanged(object sender, EventArgs e)
        //{

        //}

        //private void btnSearchShot_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void dgListCompareDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{

        //}

        //private void dgListCompareChoice_Checked(object sender, RoutedEventArgs e)
        //{

        //}

        //private void dgListCompare_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{

        //}

        //private void btnSearchCompare_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void btnAddCompare_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void cboStockSeqCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{

        //}

        //private void ldpMonthCompare_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        //{

        //}

        //private void cboProcCompare_SelectionChanged(object sender, EventArgs e)
        //{

        //}

        //private void cboStckCntGrCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{

        //}

        //private void cboAreaCompare_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{

        //}

        //private void btnExclude_SNAP_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void btnExclude_SNAP_Cancel_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void btnRowReSet_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void chkHeader_SNAP_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void dgListShot_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        //{

        //}
    }
}
