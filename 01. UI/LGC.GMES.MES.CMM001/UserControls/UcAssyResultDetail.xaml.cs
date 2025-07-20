/*************************************************************************************
 Created Date : 2017.05.23
      Creator : 신 광희
   Decription : [조립 - 원각 및 초소형 생산실적 상세 UserControl]
--------------------------------------------------------------------------------------
 [Change History]
 2023.10.18   주동석     : 대차아이디 저장 추가
 2024.01.19   오수현     : E20230901-001504 ESL 및 대차ID 자동리딩 적용 - MMD 와인딩 대차아이디 보기 여부의 기준 변경(동->라인)
 2024.01.25   오수현     : E20230901-001504 ChangeEquipment()함수 호출 부분 추가. 변수 추가(EquipmentSegmentCode, EquipmentCode)
**************************************************************************************/

using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;



namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyResultDetail.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyResultDetail
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public C1DataGrid DgDefectDetail { get; set; }

        public string ProcessCode { get; set; }

        public RichTextBox TextRemark { get; set; }

        public TextBox TextAssyResultQty { get; set; }

        public TextBox TxtPatternResultQty { get; set; }

        public TextBox TxtCSTID { get; set; }

        public TextBox TxtLotID { get; set; }


        public Button ButtonSaveWipHistory { get; set; }
        // 대차ID 저장
        public Button ButtonCSTIDSave { get; set; }

        //추가
        public Button ButtonVersion { get; set; }
        //추가
        public TextBox TxtProdVerCode { get; set; }

        public bool IsSmallType { get; set; }

        public bool IsRework { get; set; }

        public C1DataGrid DgProductLot { get; set; }

        public ComboBox ComboEquipment { get; set; }
        //추가
        public C1DataGrid PRODLOT_GRID { get; set; }
        //추가
        public string EquipmentSegmentCode { get; set; }
        //추가
        public string EquipmentCode { get; set; }

        public UcAssyResultDetail()
        {
            InitializeComponent();
            SetControl();
        }

        #endregion

        #region Initialize
        private void SetControl()
        {
            DgDefectDetail = dgDefectDetail;
            TextRemark = txtRemark;
            TextAssyResultQty = txtAssyResultQty;
            TxtPatternResultQty = txtPatternResultQty;
            TxtCSTID = txtCSTID;
            TxtLotID = txtLotId;

            ButtonSaveWipHistory = btnSaveWipHistory;
            ButtonCSTIDSave = btnCSTIDSave;
            TxtProdVerCode = txtProdVerCode;
            ButtonVersion = btnVersion;

        }

        public void SetControlProperties()
        {
            dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["DEFECTQTY"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["ALPHAQTY_P"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["ALPHAQTY_M"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["REINPUTQTY"].Visibility = Visibility.Collapsed;
            dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Collapsed;

            txtErpSendYn.Visibility = Visibility.Collapsed;

            tbErpSendYn.Visibility = Visibility.Collapsed;

            tbPatternResult.Visibility = Visibility.Collapsed;
            txtPatternResultQty.Visibility = Visibility.Collapsed;
            tbPattern.Visibility = Visibility.Collapsed;

            if (LoginInfo.CFG_SHOP_ID.Equals("A010") &&  string.Equals(ProcessCode, Process.WINDING) && !IsSmallType && SetAssyCSTIDView())
            {
                btnCSTIDSave.Visibility = Visibility.Visible;
                tbCSTID.Visibility = Visibility.Visible;
                txtCSTID.Visibility = Visibility.Visible;
            }
            else
            {
                btnCSTIDSave.Visibility = Visibility.Collapsed;
                tbCSTID.Visibility = Visibility.Collapsed;
                txtCSTID.Visibility = Visibility.Collapsed;
            }

            if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
            {
                //패턴초과 로직 추가 2020-09-01
                tbPatternResult.Visibility = Visibility.Visible;
                txtPatternResultQty.Visibility = Visibility.Visible;
                tbPattern.Visibility = Visibility.Visible;
                tbPatternResult.Text = ObjectDic.Instance.GetObjectName("패턴초과");

                tbAssyResult.Text = ObjectDic.Instance.GetObjectName("추가불량");
                if (IsSmallType)
                {
                    dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
                }
                else
                {
                    dgDefectDetail.Columns["GOODQTY"].IsReadOnly = false;
                    var dataGridNumericColumn = dgDefectDetail.Columns["GOODQTY"] as DataGridNumericColumn;
                    if (dataGridNumericColumn != null)
                    {
                        dataGridNumericColumn.Maximum = IsSmallType ? 99999 : 9999;
                    }
                }
            }
            else if (string.Equals(ProcessCode, Process.ASSEMBLY))
            {
                tbAssyResult.Text = ObjectDic.Instance.GetObjectName("차이수량");
                dgDefectDetail.Columns["OUTPUTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["GOODQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["REINPUTQTY"].Visibility = Visibility.Visible;
                dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
                dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Visible;
                //if(!IsSmallType)
                //    dgDefectDetail.Columns["BOXQTY"].Visibility = Visibility.Visible;
            }
            else if (string.Equals(ProcessCode, Process.WASHING))
            {
                tbAssyResult.Visibility = Visibility.Collapsed;
                TextAssyResultQty.Visibility = Visibility.Collapsed;
                bdAddDefect.Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["EQPTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["INPUTQTY"].Visibility = Visibility.Collapsed;
                dgDefectDetail.Columns["GOODQTY"].IsReadOnly = true;
            }
        }

        #endregion

        #region Event

        private void dgDefectDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Index == dataGrid.Columns["REINPUTQTY"].Index)
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (e.Cell.Column.Index == dataGrid.Columns["GOODQTY"].Index)
                    {
                        if ((string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH)) && !IsSmallType)
                        {
                            var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                            if (convertFromString != null)
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgDefectDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgDefectDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Column != null)
            {
                if (string.Equals(ProcessCode, Process.WINDING) || string.Equals(ProcessCode, Process.WINDING_POUCH))
                {
                    if (e.Cell.Column.Name.Equals("GOODQTY"))
                    {
                        double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                        double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                        double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                        double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                        double eqptqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTQTY").GetDouble();
                        double outputqty = goodqty + defectqty + lossqty + chargeprdqty;
                        double adddefectqty = 0;
                        adddefectqty = eqptqty - (goodqty + defectqty + lossqty + chargeprdqty);
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTPUTQTY", outputqty);

                        if (Math.Abs(adddefectqty) > 0)
                        {
                            txtAssyResultQty.Text = adddefectqty.ToString("##,###");
                            txtAssyResultQty.FontWeight = FontWeights.Bold;
                            txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            txtAssyResultQty.Text = "0";
                            txtAssyResultQty.FontWeight = FontWeights.Normal;
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                            if (convertFromString != null)
                                txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
                else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    if (e.Cell.Column.Name.Equals("REINPUTQTY"))
                    {
                        double inputqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "INPUTQTY").GetDouble();
                        double reinputqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "REINPUTQTY").GetDouble();
                        double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                        double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                        double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                        double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                        double differenceqty = 0;
                        double boxqty = 0;

                        if (!IsSmallType)
                        {
                            boxqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXQTY").GetDouble();
                        }

                        differenceqty = (inputqty + reinputqty) - (goodqty + defectqty + lossqty + chargeprdqty + boxqty).GetDouble();

                        if (Math.Abs(differenceqty) > 0)
                        {
                            txtAssyResultQty.Text = differenceqty.ToString("##,###");
                            txtAssyResultQty.FontWeight = FontWeights.Bold;
                            txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            txtAssyResultQty.Text = "0";
                            txtAssyResultQty.FontWeight = FontWeights.Normal;
                            var convertFromString = ColorConverter.ConvertFromString("#FF4D4C4C");
                            if (convertFromString != null)
                                txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
                else
                {
                    double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                    double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                    double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                    double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();
                    double outputqty = goodqty + defectqty + lossqty + chargeprdqty;

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTPUTQTY", outputqty);
                }
            }
        }
        #endregion

        #region Mehod

        // E20230901-001504 추가
        public void ChangeEquipment(string equipmentCode, string equipmentSegmentCode)
        {
            try
            {
                EquipmentSegmentCode = equipmentSegmentCode;
                EquipmentCode = equipmentCode;

                ClearResultDetailControl();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearResultDetailControl()
        {
            Util.gridClear(dgDefectDetail);

            txtAssyResultQty.Text = string.Empty;
            txtPatternResultQty.Text = string.Empty;
            txtWorkOrder.Text = string.Empty;
            txtLotId.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            txtProdVerCode.Text = string.Empty;
            txtWorkMinute.Text = string.Empty;
            txtRemark.Document.Blocks.Clear();
            txtProdId.Text = string.Empty;

        }

        public void SetResultDetailControl(object selectedItem)
        {
            DataRowView rowview = selectedItem as DataRowView;
            if (rowview == null) return;

            txtWorkOrder.Text = rowview["WOID"].GetString();
            txtLotId.Text = rowview["LOTID"].GetString();
            txtProdId.Text = rowview["PRODID"].GetString();
            txtStartTime.Text = rowview["WIPDTTM_ST"].GetString();
            txtEndTime.Text = rowview["WIPDTTM_ED"].GetString();
            txtProdVerCode.Text = rowview["PROD_VER_CODE"].GetString();

            if (LoginInfo.CFG_SHOP_ID.Equals("A010") && string.Equals(ProcessCode, Process.WINDING) && !IsSmallType)
                txtCSTID.Text = rowview["CSTID"].GetString();

            DateTime dTmpEnd;
            DateTime dTmpStart;

            if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) && string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            {
                if (DateTime.TryParse(txtStartTime.Text, out dTmpStart))
                {
                    txtWorkMinute.Text = Math.Truncate(DateTime.Now.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
                }
            }

            else if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) && !string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            {
                if (DateTime.TryParse(txtEndTime.Text, out dTmpEnd) && DateTime.TryParse(txtStartTime.Text, out dTmpStart))
                    txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
            }

            txtRemark.AppendText(rowview["REMARK"].GetString());
        }

        #region WINDING 공정의 대차ID 항목 표기여부
        private bool SetAssyCSTIDView()
        {
            if (String.IsNullOrEmpty(Util.NVC(EquipmentSegmentCode)))
            {
                return false;
            }

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "ASSY_CSTID_VIEW";
            dr["CBO_CODE"] = EquipmentSegmentCode;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }
        #endregion

        #endregion
    }
}
