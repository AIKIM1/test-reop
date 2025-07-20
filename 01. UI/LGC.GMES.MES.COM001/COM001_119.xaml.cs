/*************************************************************************************
 Created Date : 2017.10.23
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.13  정규환 : Initial Created.
  2025.02.03  백상우 : [MES2.0] 투입취소 이력탭 보이도록 변경.
  2025.07.08  이민형 : 현재 공장의 Factory만 보여지도록 수정.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using C1.WPF.DataGrid;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;
using System.Globalization;

using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_119.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_119 : UserControl, IWorkArea
    {
        private BizDataSet _Biz = new BizDataSet();
        private string _retMtrlUnitCode = string.Empty; // 자재단위
        private string _retWoId = string.Empty;         // WorkOrder
        private string _retProdId = string.Empty;       // 제품
        private string _retProdName = string.Empty;     // 제품명
        private string _retProdUnitCode = string.Empty; // 제품단위
        private string _retIssSlocId = string.Empty;    // 투입자재 저장위치
        private string _retSlocId = string.Empty;       // 제품 저장위치
        private string _retRsvItemNo = string.Empty;    // 투입자재 변경차수
        private string _retRoutNo = string.Empty;       // 제품 라우트
        private string _retUseFlag = string.Empty;      // 사용여부

        public COM001_119()
        {
            InitializeComponent();
            InitCombo();
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Init();

            this.Loaded -= UserControl_Loaded;
        }

        #region Initialize

        void Init()
        {
            dgInput.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("SHOPID", typeof(string));
            emptyTransferTable.Columns.Add("CALDATE", typeof(string));
            emptyTransferTable.Columns.Add("INPUT_TYPE", typeof(string));
            emptyTransferTable.Columns.Add("WOID", typeof(string));
            emptyTransferTable.Columns.Add("PRODID", typeof(string));
            emptyTransferTable.Columns.Add("MTRLID", typeof(string));
            emptyTransferTable.Columns.Add("MTRL_UNIT_CODE", typeof(string));
            emptyTransferTable.Columns.Add("ENTRY_QTY", typeof(decimal));
            emptyTransferTable.Columns.Add("ISS_SLOC_ID", typeof(string));
            emptyTransferTable.Columns.Add("PROD_UNIT_CODE", typeof(string));
            emptyTransferTable.Columns.Add("SLOC_ID", typeof(string));
            emptyTransferTable.Columns.Add("RSV_ITEM_NO", typeof(string));
            emptyTransferTable.Columns.Add("ROUT_NO", typeof(string));
            emptyTransferTable.Columns.Add("USEFLAG", typeof(string));
            emptyTransferTable.Columns.Add("CHK", typeof(string));
            emptyTransferTable.Columns.Add("ERR_REASON", typeof(string));
            emptyTransferTable.Columns.Add("LOTID", typeof(string));


            dgInput.ItemsSource = DataTableConverter.Convert(emptyTransferTable);

            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_SHOP_CBO", new string[] { "LANGID","SHOPID" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.NONE, dgInput.Columns["SHOPID"], "CBO_CODE", "CBO_NAME");
            CommonCombo.SetDataGridComboItem("DA_PRD_SEL_COMMONCODE_CHK", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "INPUT_ADJUST_TYPE_CODE" }, CommonCombo.ComboStatus.NONE, dgInput.Columns["INPUT_TYPE"], "CMCODE", "CMCDNAME");

            //MES 2.0 - 전극일때 탭 비활성화 (유재홍 선임 요청)
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E"))
            {
                ControlAdjust.Visibility = Visibility.Collapsed;
                AdjustHistory.Visibility = Visibility.Collapsed;
                //CancelHIstory.Visibility = Visibility.Collapsed;
            }
        }

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            C1ComboBox[] cboCancelAreaChild = { cboCancelEquipmentSegment };
            _combo.SetCombo(cboCancelArea, CommonCombo.ComboStatus.NONE, cbChild: cboCancelAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);

            C1ComboBox[] cboCancelEquipmentSegmentParent = { cboCancelArea };
            _combo.SetCombo(cboCancelEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboCancelEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);

            if (cboProcess.Items.Count < 1)
                SetProcess();

            C1ComboBox[] cboCancelProcessParent = { cboCancelEquipmentSegment };
            _combo.SetCombo(cboCancelProcess, CommonCombo.ComboStatus.SELECT, null, cboCancelProcessParent, sCase: "PROCESS");

            if (cboCancelProcess.Items.Count < 1)
                SetCancelProcess();

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;

            cboCancelEquipmentSegment.SelectedItemChanged += cboCancelEquipmentSegment_SelectedItemChanged;
            cboCancelProcess.SelectedItemChanged += CboCancelProcess_SelectedItemChanged;

            //플랜트
            CommonCombo.CommonBaseCombo("DA_BAS_SEL_SHOP_CBO", cboShop, new string[] { "SYSTEM_ID", "LANGID", "USERID", "USE_FLAG", "SHOPID" }, new string[] { LoginInfo.SYSID, LoginInfo.LANGID, LoginInfo.USERID, "Y",LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME");

        }

        #endregion

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                cboProcess.SelectedItemChanged -= CboProcess_SelectedItemChanged;
                SetProcess();
                cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
                SetEquipment();
            }
        }

        private void cboCancelEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboCancelEquipmentSegment.Items.Count > 0 && cboCancelEquipmentSegment.SelectedValue != null && !cboCancelEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                cboCancelProcess.SelectedItemChanged -= CboCancelProcess_SelectedItemChanged;
                SetCancelProcess();
                cboCancelProcess.SelectedItemChanged += CboCancelProcess_SelectedItemChanged;
            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgInputHist);
                Util.gridClear(dgInput);
            }
        }

        private void CboCancelProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboCancelProcess.Items.Count > 0 && cboCancelProcess.SelectedValue != null && !cboCancelProcess.SelectedValue.Equals("SELECT"))
            {
                Util.gridClear(dgCancelHist);
            }
        }
        #endregion

        #region Button 처리

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmSend() == false)
            {
                return;
            }

            string shopId = string.Empty;
            string calDate = string.Empty;
            string Lotid = string.Empty;
            string prodId = string.Empty;
            string inputType = string.Empty;
            string woId = string.Empty;
            string mtrlId = string.Empty;
            string mtrlUnitCode = string.Empty;
            string prodUnitCode = string.Empty;
            decimal enterQty = 0;
            string slocId = string.Empty;
            string issSlocId = string.Empty;
            string rsvItemNo = string.Empty;
            string routNo = string.Empty;
            string useFlag = string.Empty;
            string userId = string.Empty;
            string crrtNote = string.Empty;

            // 전송 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3609"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
            {
                if (vResult == MessageBoxResult.OK)
                {
                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SHOPID", typeof(string));         // 플랜트
                    inDataTable.Columns.Add("CALDATE", typeof(string));        // 전기일
                    inDataTable.Columns.Add("PRODID", typeof(string));         // 제품
                    inDataTable.Columns.Add("INPUT_TYPE", typeof(string));     // 투입 구분
                    inDataTable.Columns.Add("WOID", typeof(string));           // W/O
                    inDataTable.Columns.Add("MTRLID", typeof(string));         // 투입자재
                    inDataTable.Columns.Add("MTRL_UNIT_CODE", typeof(string)); // 투입자재 단위
                    inDataTable.Columns.Add("PROD_UNIT_CODE", typeof(string)); // 제품 단위
                    inDataTable.Columns.Add("ENTRY_QTY", typeof(decimal));     // 투입 수량
                    inDataTable.Columns.Add("SLOC_ID", typeof(string));        // 저장위치
                    inDataTable.Columns.Add("ISS_SLOC_ID", typeof(string));    // 자재 투입 저장위치
                    inDataTable.Columns.Add("RSV_ITEM_NO", typeof(string));    // 품목번호
                    inDataTable.Columns.Add("ROUT_NO", typeof(string));        // 라우트 번호
                    inDataTable.Columns.Add("USEFLAG", typeof(string));        // 사용여부
                    inDataTable.Columns.Add("USERID", typeof(string));         // 요청자
                    inDataTable.Columns.Add("CRRT_NOTE", typeof(string));      // 비고
                    inDataTable.Columns.Add("LOTID", typeof(string));          // 실LOTID

                    for (int nrow = 0; nrow < dgInput.Rows.Count; nrow++)
                    {
                        inputType = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "INPUT_TYPE") as string;
                        woId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "WOID") as string;
                        mtrlId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "MTRLID") as string;
                        prodUnitCode = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "PROD_UNIT_CODE") as string;
                        rsvItemNo = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "RSV_ITEM_NO") as string;
                        routNo = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "ROUT_NO") as string;
                        useFlag = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "USEFLAG") as string;
                        calDate = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "CALDATE") as string;
                        prodId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "PRODID") as string;
                        enterQty = Convert.ToDecimal(DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "ENTRY_QTY"));
                        mtrlUnitCode = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "MTRL_UNIT_CODE") as string;
                        shopId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "SHOPID") as string;
                        slocId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "SLOC_ID") as string;
                        issSlocId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "ISS_SLOC_ID") as string;
                        Lotid = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "LOTID") as string;

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["SHOPID"] = shopId;                 // 플랜트
                        inDataRow["CALDATE"] = calDate;               // 전기일
                        inDataRow["PRODID"] = prodId;                 // 제품
                        inDataRow["INPUT_TYPE"] = inputType;          // 투입 구분
                        inDataRow["WOID"] = woId;                     // W/O
                        inDataRow["MTRLID"] = mtrlId;                 // 투입자재
                        inDataRow["MTRL_UNIT_CODE"] = mtrlUnitCode;   // 투입자재 단위
                        inDataRow["PROD_UNIT_CODE"] = prodUnitCode;   // 제품 단위
                        inDataRow["ENTRY_QTY"] = enterQty;            // 투입 수량
                        inDataRow["SLOC_ID"] = slocId;                // 저장위치
                        inDataRow["ISS_SLOC_ID"] = issSlocId;         // 자재 투입 저장위치
                        inDataRow["RSV_ITEM_NO"] = rsvItemNo;         // 품목번호
                        inDataRow["ROUT_NO"] = routNo;                // 라우트 번호
                        inDataRow["USEFLAG"] = useFlag;               // 사용여부
                        inDataRow["USERID"] = txtReqUser.Tag; ;       // 요청자
                        inDataRow["CRRT_NOTE"] = txtNote.Text; ;      // 비고
                        inDataRow["LOTID"] = Lotid;                   // 실LOTID
                        inDataTable.Rows.Add(inDataRow);
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_MATERIAL_ADJUST", "INDATA", null, (result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.AlertByBiz("BR_PRD_REG_STOCK_MATERIAL_ADJUST", ex.Message, ex.ToString());
                            return;
                        }
                    }, inDataSet);

                    // 전송 완료 되었습니다.
                    Util.MessageInfo("SFU1880");

                    Init();
                }
            }, false, false, string.Empty);
        }

        bool ConfirmSend()
        {
            string shopId = string.Empty;
            string calDate = string.Empty;
            string prodId = string.Empty;
            string inputType = string.Empty;
            string woId = string.Empty;
            string mtrlId = string.Empty;
            string mtrlUnitCode = string.Empty;
            string prodUnitCode = string.Empty;
            decimal enterQty = 0;
            string slocId = string.Empty;
            string issSlocId = string.Empty;
            string rsvItemNo = string.Empty;
            string routNo = string.Empty;
            string useFlag = string.Empty;
            string userId = string.Empty;
            string crrtNote = string.Empty;
            string Lotid = string.Empty;


            if (dgInput.Rows.Count == 0)
                return false;

            for (int nrow = 0; nrow < dgInput.Rows.Count; nrow++)
            {
                inputType = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "INPUT_TYPE") as string;
                woId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "WOID") as string;
                mtrlId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "MTRLID") as string;
                prodUnitCode = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "PROD_UNIT_CODE") as string;
                rsvItemNo = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "RSV_ITEM_NO") as string;
                routNo = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "ROUT_NO") as string;
                useFlag = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "USEFLAG") as string;
                calDate = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "CALDATE") as string;
                prodId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "PRODID") as string;
                enterQty = Convert.ToDecimal(DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "ENTRY_QTY"));
                mtrlUnitCode = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "MTRL_UNIT_CODE") as string;
                shopId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "SHOPID") as string;
                slocId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "SLOC_ID") as string;
                issSlocId = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "ISS_SLOC_ID") as string;
                Lotid = DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "LOTID") as string;


                // {0}이 입력되지 않았습니다.
                if (string.IsNullOrWhiteSpace(shopId))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("플랜트"));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(calDate))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("전기일"));
                    return false;
                }
                try
                {
                    DateTime date = DateTime.ParseExact(calDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                }
                catch
                {
                    Util.MessageValidation("SFU3241", calDate);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(inputType))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("투입구분"));
                    return false;
                }
                if (string.IsNullOrWhiteSpace(woId))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("W/O"));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(prodId))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("제품"));
                    return false;
                }
                //NERP 투입조정은 원부자재 전송X
                string result_Mtrl = ChkMTRL(mtrlId);
                if (result_Mtrl == "MTRL")
                {
                    Util.MessageValidation("SFU2092", (nrow + 1).ToString() + "row ");
                    return false;

                }
                if (string.IsNullOrWhiteSpace(mtrlId) || string.IsNullOrWhiteSpace(rsvItemNo))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("투입자재"));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtNote.Text))
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("비고"));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtReqUser.Text) || txtReqUser.Tag.ToString() == "")
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("요청자"));
                    return false;
                }

                if (enterQty <= 0)
                {
                    Util.MessageValidation("SFU1299", (nrow + 1).ToString() + "row " + ObjectDic.Instance.GetObjectName("수량"));
                    return false;
                }
                string result_Lot = ChkLotid(Lotid, mtrlId, slocId, shopId);
                if (result_Lot != "P")
                {
                    if (result_Lot == "N")
                    {
                        //선택된 LOT과 제품코드가 다릅니다.
                        Util.MessageValidation("SFU2907");
                        // continue;
                    }
                    else if (result_Lot == "F")
                    {
                        Util.MessageValidation("SFU1386");
  
                    }
                    else if (result_Lot == "C")
                    {
                        Util.MessageValidation("SFU2090");

                    }
                    else if (result_Lot == "X")
                    {
                        Util.MessageValidation("SFU2091");

                    }
                    return false;
                }

            }

            return true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgInput);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgInput);
        }

        private void btnFormat_Click(object sender, RoutedEventArgs e)
        {
            COM001_119_UPLOAD_FORMAT _xlsFormat = new COM001_119_UPLOAD_FORMAT();
            _xlsFormat.FrameOperation = FrameOperation;

            if (_xlsFormat != null)
            {
                object[] Parameters = new object[0];

                C1WindowExtension.SetParameters(_xlsFormat, Parameters);

                _xlsFormat.ShowModal();
                _xlsFormat.CenterOnScreen();
            }
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSendCheck(0, dgInput.Rows.Count);
        }

        private void dgInput_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (!dg.CurrentCell.IsEditing)
                {
                    switch (dg.CurrentCell.Column.Name)
                    {
                        case "MTRLID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRLID")?.ToString().Length > 0)
                                SetMtrlUnit(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRLID")?.ToString());
                            break;
                    }
                }

                //dg.EndEdit();
                //ConfirmSendCheck(e.Cell.Row.Index, e.Cell.Row.Index + 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void SetMtrlUnit(string mtrlId)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("MTRLID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["MTRLID"] = mtrlId;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_MATERIAL", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
                DataTableConverter.SetValue(dgInput.Rows[dgInput.CurrentCell.Row.Index].DataItem, "MTRL_UNIT_CODE", "");
            else
                DataTableConverter.SetValue(dgInput.Rows[dgInput.CurrentCell.Row.Index].DataItem, "MTRL_UNIT_CODE", Util.NVC(dt.Rows[0]["MTRLUNIT"]));

            this.dgInput.EndEdit();
            this.dgInput.EndEditRow(true);
        }

        private void dgInput_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK")).ToString().Equals("ERROR"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink"));
                }

            }));

        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow dr2 = dt.NewRow();
                dr2["ENTRY_QTY"] = 0;
                dt.Rows.Add(dr2);
                dt.AcceptChanges();

                dg.ItemsSource = DataTableConverter.Convert(dt);

                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 기존 저장자료는 제외
                if (dg.SelectedIndex > -1)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.SelectedIndex].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #region Excel 업로드
        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }

        void GetExcel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                string mtrlId = string.Empty;
                string woId = string.Empty;

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                // 해더 제외
                DataTable dt = DataTableConverter.Convert(dgInput.ItemsSource).Clone();

                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                {
                    DataRow dr = dt.NewRow();

                    if (sheet.GetCell(rowInx, 0) == null)
                        dr["SHOPID"] = "";
                    else
                        dr["SHOPID"] = Util.NVC(sheet.GetCell(rowInx, 0).Text);

                    if (sheet.GetCell(rowInx, 1) == null)
                        dr["CALDATE"] = "";
                    else
                        dr["CALDATE"] = Util.NVC(sheet.GetCell(rowInx, 1).Text);

                    if (sheet.GetCell(rowInx, 2) == null)
                        dr["INPUT_TYPE"] = "";
                    else
                        dr["INPUT_TYPE"] = Util.NVC(sheet.GetCell(rowInx, 2).Text);

                    if (sheet.GetCell(rowInx, 3) == null)
                        dr["LOTID"] = "";
                    else
                        dr["LOTID"] = Util.NVC(sheet.GetCell(rowInx, 3).Text);


                    if (sheet.GetCell(rowInx, 4) == null)
                        dr["MTRLID"] = "";
                    else
                    {
                        dr["MTRLID"] = Util.NVC(sheet.GetCell(rowInx, 4).Text);
                        mtrlId = Util.NVC(sheet.GetCell(rowInx, 4).Text);
                    }


                    if (sheet.GetCell(rowInx, 5) == null)
                        dr["WOID"] = "";
                    else
                    {
                        dr["WOID"] = Util.NVC(sheet.GetCell(rowInx, 5).Text);
                        woId = Util.NVC(sheet.GetCell(rowInx, 5).Text);
                    }

                    if (sheet.GetCell(rowInx, 6) == null)
                        dr["ENTRY_QTY"] = 0;
                    else
                        dr["ENTRY_QTY"] = Util.NVC(sheet.GetCell(rowInx, 6).Text);
                    
                    if (!string.IsNullOrWhiteSpace(mtrlId) && !string.IsNullOrWhiteSpace(woId))
                    {
                        SetMtrlWoInfo(mtrlId, woId);

                        dr["MTRL_UNIT_CODE"] = _retMtrlUnitCode; // 자재단위
                        dr["WOID"] = _retWoId;                   // WorkOrder
                        dr["PRODID"] = _retProdId;               // 제품
                        dr["PROD_UNIT_CODE"] = _retProdUnitCode; // 제품단위
                        dr["ISS_SLOC_ID"] = _retIssSlocId;       // 투입자재 저장위치
                        dr["SLOC_ID"] = _retSlocId;              // 제품 저장위치
                        dr["RSV_ITEM_NO"] = _retRsvItemNo;       // 투입자재 변경차수
                        dr["ROUT_NO"] = _retRoutNo;              // 제품 라우트
                        dr["USEFLAG"] = _retUseFlag;             // 사용여부
                    }

                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();
                Util.GridSetData(dgInput, dt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        void SetMtrlWoInfo(string mtrlId, string woId)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("WOID", typeof(string));
            IndataTable.Columns.Add("MTRLID", typeof(string));
            IndataTable.Columns.Add("RSV_ITEM_NO", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["WOID"] = woId;
            Indata["MTRLID"] = mtrlId;

            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MATERIAL_WORKORDER", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
            {
                _retMtrlUnitCode = string.Empty; // 자재단위
                _retWoId = string.Empty;         // WorkOrder
                _retProdId = string.Empty;       // 제품
                _retProdName = string.Empty;     // 제품명
                _retProdUnitCode = string.Empty; // 제품단위
                _retIssSlocId = string.Empty;    // 투입자재 저장위치
                _retSlocId = string.Empty;       // 제품 저장위치
                _retRsvItemNo = string.Empty;    // 투입자재 변경차수
                _retRoutNo = string.Empty;       // 제품 라우트
                _retUseFlag = string.Empty;      // 사용여부
            }
            else
            {
                _retMtrlUnitCode = Util.NVC(dt.Rows[0]["MTRL_UNIT_CODE"]); // 자재단위
                _retWoId = Util.NVC(dt.Rows[0]["WOID"]);                   // WorkOrder
                _retProdId = Util.NVC(dt.Rows[0]["PRODID"]);               // 제품
                _retProdName = Util.NVC(dt.Rows[0]["PRODNAME"]);           // 제품명
                _retProdUnitCode = Util.NVC(dt.Rows[0]["PROD_UNIT_CODE"]); // 제품단위
                _retIssSlocId = Util.NVC(dt.Rows[0]["ISS_SLOC_ID"]);       // 투입자재 저장위치
                _retSlocId = Util.NVC(dt.Rows[0]["SLOC_ID"]);              // 제품 저장위치
                _retRsvItemNo = Util.NVC(dt.Rows[0]["RSV_ITEM_NO"]);       // 투입자재 변경차수
                _retRoutNo = Util.NVC(dt.Rows[0]["ROUT_NO"]);              // 제품 라우트
                _retUseFlag = Util.NVC(dt.Rows[0]["USEFLAG"]);             // 사용여부
            }
        }
        #endregion

        void ConfirmSendCheck(int nstartrow, int nendrow)
        {
            string shopId = string.Empty;
            string calDate = string.Empty;
            string prodId = string.Empty;
            string inputType = string.Empty;
            string woId = string.Empty;
            string mtrlId = string.Empty;
            string mtrlUnitCode = string.Empty;
            string prodUnitCode = string.Empty;
            decimal enterQty = 0;
            string slocId = string.Empty;
            string issSlocId = string.Empty;
            string rsvItemNo = string.Empty;
            string routNo = string.Empty;
            string useFlag = string.Empty;
            string userId = string.Empty;
            string crrtNote = string.Empty;
            string Lotid = string.Empty;


            DataTable dtchk = DataTableConverter.Convert(dgInput.ItemsSource);

            for (int nrow = nstartrow; nrow < nendrow; nrow++)
            {
                dtchk.Rows[nrow]["CHK"] = "OK";
                dtchk.Rows[nrow]["ERR_REASON"] = string.Empty;

                calDate = Util.NVC(dtchk.Rows[nrow]["CALDATE"]);
                prodId = Util.NVC(dtchk.Rows[nrow]["PRODID"]);
                enterQty = Util.NVC_Decimal(dtchk.Rows[nrow]["ENTRY_QTY"]);
                prodUnitCode = Util.NVC(dtchk.Rows[nrow]["PROD_UNIT_CODE"]);
                shopId = Util.NVC(dtchk.Rows[nrow]["SHOPID"]);
                slocId = Util.NVC(dtchk.Rows[nrow]["SLOC_ID"]);
                inputType = Util.NVC(dtchk.Rows[nrow]["INPUT_TYPE"]);
                woId = Util.NVC(dtchk.Rows[nrow]["WOID"]);
                mtrlId = Util.NVC(dtchk.Rows[nrow]["MTRLID"]);
                rsvItemNo = Util.NVC(dtchk.Rows[nrow]["RSV_ITEM_NO"]);
                Lotid = Util.NVC(dtchk.Rows[nrow]["LOTID"]);

                //if (string.IsNullOrWhiteSpace(woId) || string.IsNullOrWhiteSpace(mtrlId) || string.IsNullOrWhiteSpace(rsvItemNo))
                //{
                //    dtchk.Rows[nrow]["CHK"] = "ERROR";
                //}

                //if (string.IsNullOrWhiteSpace(shopId) || string.IsNullOrWhiteSpace(slocId) || string.IsNullOrWhiteSpace(inputType) || string.IsNullOrWhiteSpace(prodId))
                //{
                //    dtchk.Rows[nrow]["CHK"] = "ERROR";
                //}

                //if (string.IsNullOrWhiteSpace(calDate))
                //{
                //    dtchk.Rows[nrow]["CHK"] = "ERROR";
                //}
                //try
                //{
                //    DateTime date = DateTime.ParseExact(calDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                //}
                //catch
                //{
                //    dtchk.Rows[nrow]["CHK"] = "ERROR";
                //}

                //if (enterQty <= 0)
                //{
                //    dtchk.Rows[nrow]["CHK"] = "ERROR";
                //}

                //if (string.IsNullOrWhiteSpace(txtNote.Text))
                //{
                //    dtchk.Rows[nrow]["CHK"] = "ERROR";
                //}

                //if (string.IsNullOrWhiteSpace(txtReqUser.Text) || txtReqUser.Tag == "")
                //{
                //    dtchk.Rows[nrow]["CHK"] = "ERROR";
                //}
               

                if (string.IsNullOrWhiteSpace(shopId))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("플랜트"));
                    continue;
                }
                if (string.IsNullOrWhiteSpace(calDate))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("전기일"));
                    continue;
                }
                try
                {
                    DateTime date = DateTime.ParseExact(calDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                }
                catch
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU3241", calDate);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(inputType))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("투입구분"));
                    continue;
                }
                //NERP 투입조정은 원부자재 전송X
                string result_Mtrl = ChkMTRL(mtrlId);
                if (result_Mtrl == "MTRL")
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU2092");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(woId))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("W/O"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(prodId))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("제품"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(mtrlId) || string.IsNullOrWhiteSpace(rsvItemNo))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("투입자재"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(txtNote.Text))
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("비고"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(txtReqUser.Text) || txtReqUser.Tag == "")
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("요청자"));
                    continue;
                }

                if (enterQty <= 0)
                {
                    dtchk.Rows[nrow]["CHK"] = "ERROR";
                    dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1299", ObjectDic.Instance.GetObjectName("수량"));
                    continue;
                }
                //NERP
                string result_Lot = ChkLotid(Lotid, mtrlId, slocId, shopId);
                if (result_Lot != "P")
                {
                    if (result_Lot == "N")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //선택한 LOT의 제품코드와 다릅니다.
                        dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU2907");
                        continue;
                    }
                    else if (result_Lot == "F")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //LOT정보가 없습니다.
                        dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU1386");
                        continue;
                    }
                    else if (result_Lot == "C")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //전송 대상 LOT이 아닙니다.
                        dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU2090");
                        continue;
                    }
                    else if (result_Lot == "X")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //잘못된 저장위치 입니다.
                        dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU2091");
                        continue;
                    }
                    else if (result_Lot == "E")
                    {
                        dtchk.Rows[nrow]["CHK"] = "ERROR";
                        //INPUT DATA ERROR.
                        dtchk.Rows[nrow]["ERR_REASON"] = MessageDic.Instance.GetMessage("SFU4974");
                    }
                }

            }

            dtchk.AcceptChanges();
            Util.GridSetData(dgInput, dtchk, FrameOperation, true);

        }

        #endregion

        #region [설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Process 정보 가져오기]
        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCancelProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboCancelArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboCancelEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboCancelProcess.DisplayMemberPath = "CBO_NAME";
                cboCancelProcess.SelectedValuePath = "CBO_CODE";

                cboCancelProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboCancelProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboCancelProcess.SelectedIndex < 0)
                        cboCancelProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboCancelProcess.Items.Count > 0)
                        cboCancelProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region [조회]
        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetInputHist();
        }
        private void btnSearchAdjust_Click(object sender, RoutedEventArgs e)
        {
            GetAdjustHist();
        }        
        #endregion

        #region [### 작업대상 조회 ###]
        public void GetInputHist()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                DateTime dtEndTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
                DateTime dtStartTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);

                if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
                {
                    //종료일자가 시작일자보다 빠릅니다.
                    Util.MessageValidation("SFU1913");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("WOID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                if (dr["EQSGID"].Equals("")) return;

                //string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                //dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;

                //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                string sEqptID = Util.GetCondition(cboEquipment);
                if (!string.IsNullOrWhiteSpace(sEqptID))
                    dr["EQPTID"] = sEqptID;

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                if (!string.IsNullOrWhiteSpace(txtWO.Text))
                    dr["WOID"] = txtWO.Text.ToUpper();

                if (!string.IsNullOrWhiteSpace(txtModel.Text))
                    dr["MODLID"] = txtModel.Text.ToUpper();

                if (!string.IsNullOrWhiteSpace(txtProdID.Text))
                    dr["PRODID"] = txtProdID.Text.ToUpper();

                if (!string.IsNullOrWhiteSpace(txtPjt.Text))
                    dr["PRJT_NAME"] = txtPjt.Text.ToUpper();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_STOCK_INPUT_HIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgInputHist, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region [### 작업대상 조회 ###]
        public void GetAdjustHist()
        {
            try
            {
                if ((dtpDateToHist.SelectedDateTime - dtpDateFromHist.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                DateTime dtEndTime = new DateTime(dtpDateToHist.SelectedDateTime.Year, dtpDateToHist.SelectedDateTime.Month, dtpDateToHist.SelectedDateTime.Day);
                DateTime dtStartTime = new DateTime(dtpDateFromHist.SelectedDateTime.Year, dtpDateFromHist.SelectedDateTime.Month, dtpDateFromHist.SelectedDateTime.Day);

                if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
                {
                    //종료일자가 시작일자보다 빠릅니다.
                    Util.MessageValidation("SFU1913");
                    return;
                }

                if (Util.NVC(cboShop.SelectedValue) == "SELECT")
                {
                    Util.Alert("SFU1424");  //SHOP을 선택하세요.
                    cboShop.Focus();
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("WOID", typeof(string));
                dtRqst.Columns.Add("MTRLID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFromHist);
                dr["TO_DATE"] = Util.GetCondition(dtpDateToHist);

                dr["SHOPID"] = Util.GetCondition(cboShop, MessageDic.Instance.GetMessage("SFU1424"));
                if (dr["SHOPID"].Equals("")) return;

                if (!string.IsNullOrWhiteSpace(txtWoId.Text))
                    dr["WOID"] = txtWoId.Text.ToUpper();

                if (!string.IsNullOrWhiteSpace(txtMtrlID.Text))
                    dr["MTRLID"] = txtMtrlID.Text.ToUpper();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_STOCK_MATERIAL_INPUT_ADJUST_HIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgAdjustHist, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #region 요청자
        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtReqUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtReqUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtReqUser.Text = wndPerson.USERNAME;
                txtReqUser.Tag = wndPerson.USERID;
            }
        }
        #endregion

        #region 팝업 : W/O 별 투입 자재 선택 화면
        private void btnSelectWoId_Click(object sender, RoutedEventArgs e)
        {
            if (dgInput.CurrentRow.DataItem == null)
                return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

            COM001_119_INPUT_MTRLID _xlsMtrlId = new COM001_119_INPUT_MTRLID();
            _xlsMtrlId.FrameOperation = FrameOperation;

            if (_xlsMtrlId != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = (DataTableConverter.GetValue(dgInput.Rows[rowIndex].DataItem, "MTRLID") ?? String.Empty).ToString();
                Parameters[1] = (DataTableConverter.GetValue(dgInput.Rows[rowIndex].DataItem, "WOID") ?? String.Empty).ToString();

                dgInput.SelectedIndex = rowIndex;

                C1WindowExtension.SetParameters(_xlsMtrlId, Parameters);

                _xlsMtrlId.Closed += new EventHandler(xlsMtrlId_Closed);
                _xlsMtrlId.ShowModal();
                _xlsMtrlId.CenterOnScreen();
            }
        }

        private void xlsMtrlId_Closed(object sender, EventArgs e)
        {
            COM001_119_INPUT_MTRLID mtrlInfo = sender as COM001_119_INPUT_MTRLID;
            if (mtrlInfo != null && mtrlInfo.DialogResult == MessageBoxResult.OK)
            {
                DataTableConverter.SetValue(dgInput.Rows[dgInput.SelectedIndex].DataItem, "WOID", mtrlInfo.WO_ID);
                DataTableConverter.SetValue(dgInput.Rows[dgInput.SelectedIndex].DataItem, "PRODID", mtrlInfo.PROD_ID);
                DataTableConverter.SetValue(dgInput.Rows[dgInput.SelectedIndex].DataItem, "ISS_SLOC_ID", mtrlInfo.ISS_SLOC_ID);
                DataTableConverter.SetValue(dgInput.Rows[dgInput.SelectedIndex].DataItem, "PROD_UNIT_CODE", mtrlInfo.PROD_UNIT_CODE);
                DataTableConverter.SetValue(dgInput.Rows[dgInput.SelectedIndex].DataItem, "SLOC_ID", mtrlInfo.SLOC_ID);
                DataTableConverter.SetValue(dgInput.Rows[dgInput.SelectedIndex].DataItem, "RSV_ITEM_NO", mtrlInfo.RSV_ITEM_NO);
                DataTableConverter.SetValue(dgInput.Rows[dgInput.SelectedIndex].DataItem, "ROUT_NO", mtrlInfo.ROUT_NO);
                DataTableConverter.SetValue(dgInput.Rows[dgInput.SelectedIndex].DataItem, "USEFLAG", mtrlInfo.USE_FLAG);

                this.dgInput.EndEdit();
                this.dgInput.EndEditRow(true);
            }
        }
        #endregion

        private void dgAdjustHist_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            var columnName = e?.Column?.Name;

            if (this.Dispatcher?.HasShutdownFinished == false)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(columnName) && columnName.Equals("CHK"))
                        {
                            if (e.Column.Name.Equals("CHK"))
                            {
                                pre.Content = chkAll;
                                if (e.Column.HeaderPresenter != null) e.Column.HeaderPresenter.Content = pre;
                                chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                                chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }));
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int inx = 0; inx < dgAdjustHist.GetRowCount(); inx++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgAdjustHist.Rows[inx].DataItem, "ERP_ERR_TYPE_CODE")).Equals("FAIL"))
                    {
                        DataTableConverter.SetValue(dgAdjustHist.Rows[inx].DataItem, "CHK", true);
                    }
                }
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int inx = 0; inx < dgAdjustHist.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgAdjustHist.Rows[inx].DataItem, "CHK", false);
            }
        }

        private void CHK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int row_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                if (DataTableConverter.GetValue(dgAdjustHist.Rows[row_index].DataItem, "CHK").Equals("True"))
                {
                    if (DataTableConverter.GetValue(dgAdjustHist.Rows[row_index].DataItem, "ERP_ERR_TYPE_CODE") == null || !DataTableConverter.GetValue(dgAdjustHist.Rows[row_index].DataItem, "ERP_ERR_TYPE_CODE").Equals("FAIL"))
                    {
                        DataTableConverter.SetValue(dgAdjustHist.Rows[row_index].DataItem, "CHK", "False");
                        Util.AlertInfo("SFU4911"); //ERP I/F가 실패일 경우에만 재전송 가능합니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnReSend_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmSend() == false)
            {
                return;
            }

            string erp_trnf_seqno = string.Empty;
            string chk = string.Empty;
            int chk_cnt = 0;

            // 전송 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3609"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
            {
                if (vResult == MessageBoxResult.OK)
                {

                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                    for (int nrow = 0; nrow < dgAdjustHist.GetRowCount(); nrow++)
                    {
                        chk = DataTableConverter.GetValue(dgAdjustHist.Rows[nrow].DataItem, "CHK") as string;
                        erp_trnf_seqno = DataTableConverter.GetValue(dgAdjustHist.Rows[nrow].DataItem, "ERP_TRNF_SEQNO") as string;

                        if (chk == "True")
                        {
                            DataRow inDataRow = inDataTable.NewRow();
                            inDataRow["ERP_TRNF_SEQNO"] = erp_trnf_seqno;

                            inDataTable.Rows.Add(inDataRow);

                            chk_cnt++;
                        }
                    }

                    if (chk_cnt > 0)
                    {
                        new ClientProxy().ExecuteServiceSync("BR_ACT_REG_RESEND_ERP_PROD", "INDATA", null, inDataTable);
                        
                        Util.MessageInfo("SFU1880"); // 전송 완료 되었습니다.

                        Init();
                        GetAdjustHist();
                    }
                    else
                    {
                        // 전송 완료 되었습니다.
                        Util.MessageInfo("SFU1636"); // 선택된 대상이 없습니다.
                    }                       
                }
            }, false, false, string.Empty);
        }

        private void btnCanceSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetCancelHist();
        }

        public void GetCancelHist()
        {
            try
            {
                if ((dtpCancelDateTo.SelectedDateTime - dtpCancelDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.MessageValidation("SFU2042", "31");    //기간은 %1일 이내 입니다.
                    return;
                }

                DateTime dtEndTime = new DateTime(dtpCancelDateTo.SelectedDateTime.Year, dtpCancelDateTo.SelectedDateTime.Month, dtpCancelDateTo.SelectedDateTime.Day);
                DateTime dtStartTime = new DateTime(dtpCancelDateFrom.SelectedDateTime.Year, dtpCancelDateFrom.SelectedDateTime.Month, dtpCancelDateFrom.SelectedDateTime.Day);

                if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
                {
                    Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                    return;
                }

                string areaid = Util.GetCondition(cboCancelArea);
                if (string.IsNullOrEmpty(areaid))
                {
                    Util.MessageValidation("SFU1499");  //동을 선택하세요.
                    return;
                }

                string eqsgid = Util.GetCondition(cboCancelEquipmentSegment);
                if (string.IsNullOrEmpty(eqsgid))
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                string procid = Util.GetCondition(cboCancelProcess);
                if (string.IsNullOrEmpty(procid))
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }

                Util.gridClear(dgCancelHist);

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("WOID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = areaid;
                dr["EQSGID"] = eqsgid;
                dr["PROCID"] = procid;

                //동을 선택하세요.
                //dr["AREAID"] = Util.GetCondition(cboCancelArea, MessageDic.Instance.GetMessage("SFU1499"));
                //if (dr["AREAID"].Equals("")) return;

                //라인을 선택하세요.
                //dr["EQSGID"] = Util.GetCondition(cboCancelEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                //if (dr["EQSGID"].Equals("")) return;

                //공정을 선택하세요.
                //dr["PROCID"] = Util.GetCondition(cboCancelProcess, MessageDic.Instance.GetMessage("SFU1459"));
                //if (dr["PROCID"].Equals("")) return;

                dr["FROM_DATE"] = Util.GetCondition(dtpCancelDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpCancelDateTo);

                if (!string.IsNullOrWhiteSpace(txtCancelWO.Text))
                    dr["WOID"] = txtCancelWO.Text.ToUpper();

                if (!string.IsNullOrWhiteSpace(txtCancelModel.Text))
                    dr["MODLID"] = txtCancelModel.Text.ToUpper();

                if (!string.IsNullOrWhiteSpace(txtCancelProdID.Text))
                    dr["PRODID"] = txtCancelProdID.Text.ToUpper();

                if (!string.IsNullOrWhiteSpace(txtCancelPjt.Text))
                    dr["PRJT_NAME"] = txtCancelPjt.Text.ToUpper();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_STOCK_INPUT_CANCEL_HIST", "INDATA", "OUTDATA", dtRqst);

                if(dtRslt ==  null || dtRslt.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU2816");  //조회 결과가 없습니다.
                }
                else
                {
                    Util.GridSetData(dgCancelHist, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        string ChkLotid(string sLotid, string sPordId, string sSlocid, string sShopId)
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("SLOCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = sShopId;
                Indata["SLOCID"] = sSlocid;
                Indata["LOTID"] = sLotid;
                Indata["PRODID"] = sPordId;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_TRNF_LOT", "INDATA", "OUTDATA", IndataTable);

                if (dt.Rows.Count > 0)
                {
                    return Util.NVC(dt.Rows[0]["RESULT"]).ToString();
                }

                return "E";

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "E";
            }
        }
        string ChkMTRL(string sPordId)
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["MTRLID"] = sPordId;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATERIAL", "RQSTDT", "RSLTDT", IndataTable);

                if (dt.Rows.Count > 0)
                {
                    return Util.NVC(dt.Rows[0]["MTRLTYPE"]).ToString();
                }

                return "X";

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "X";
            }
        }
    }
}
