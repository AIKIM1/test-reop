/*******************************
 Created Date :
      Creator :
   Decription : Recipe No.
--------------------------------
 [Change Histor]y
  2016.08.19  : Initial Created.
  2023.07.25  : 김태우 DAM믹서(E0430) 추가 
  2023.08.30  : 김도형 [E20230306-000498] GMES CSR nonconformities improvement  
  2024.07.25  : 이원열 [E20240712-001591] 공정진척 & 실적확정 투입lot 조회 validation 추가(대체자제)
  2024.08.09  : 배현우 [E20240807-000861] Dam Mixer 공정이 WO공정으로 변경되어 비즈 분기처리 삭제
  2024.08.10  : 이원열 [E20240712-001591] 공정진척 & 실적확정 투입lot 조회 validation 추가(대체자제) - PopupInputMaterial() 로직 변경
  2024.08.30  : 이원열 [E20240830-001478] 투입자재 팝업의 경우 투입 자재가 모두 등록된 경우에도 추가 LOT 을 등록할 수 있도록 Logic 변경
  2024.12.03  : 정재홍 [E20241021-000315] [MES팀] 솔루션ID*설비ID*CP revision 기준으로 자재 투입량 관리를 위한 MES 개선 요청 건 (CatchUp)
********************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// ELEC001_006_LOTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_INPUT_MATERIAL : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _WOID = string.Empty;
        private string _PROCID = string.Empty;
        private string _PRODID = string.Empty;
        private string _PRODVER = string.Empty;
        private decimal _INPUTQTY = 0;
        private string sMTRLID = string.Empty;
        private string sMTRLNAME = string.Empty;
        private string sMTRLDESC = string.Empty;
        private string sGRADE = string.Empty;  //Nick name

        // [E20240712-001591]
        private string _MTRLID = string.Empty;
        private bool _isELEC_MTRL_LOT_VALID_YN = false;

        Util _Util = new Util();

        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_INPUT_MATERIAL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            _LOTID = Util.NVC(tmps[0]);
            _WOID = Util.NVC(tmps[1]);
            _EQPTID = Util.NVC(tmps[2]);
            _EQPTNAME = Util.NVC(tmps[3]);
            _PROCID = Util.NVC(tmps[4]);
            _PRODID = Util.NVC(tmps[5]);
            _PRODVER = Util.NVC(tmps[6]);

            if (!string.IsNullOrEmpty(Util.NVC(tmps[7])))
                _INPUTQTY = Util.NVC_Decimal(tmps[7]);
            if (tmps.Length > 8)
            {

                txtSumInputQty.IsReadOnly = true;
                txtRemainInputQty.IsReadOnly = true;
                //  dgMaterial.Columns["INPUT_QTY"].IsReadOnly = true;

            }

            //[E20240712-001591] start
            CheckUseElecMtrlLotValidation();

            if (_isELEC_MTRL_LOT_VALID_YN)
            {
                btnMtrlLot.Visibility = Visibility.Visible;
                txtInputLotID.IsReadOnly = true;
                btnSaveMaterialLot.IsEnabled = false;
                dgMaterialLot.Columns["INPUT_LOTID"].IsReadOnly = true;
            }
            else
            {
                btnMtrlLot.Visibility = Visibility.Collapsed;
                txtInputLotID.IsReadOnly = false;
            }
            //[E20240712-001591] end


            // SRS코터는 사용 안함
            if (string.Equals(_PROCID, Process.SRS_MIXING))
            {
                panelInput.Visibility = Visibility.Collapsed;
                dgMaterial.Columns["BASE_QTY"].Visibility = Visibility.Collapsed;
            }
            GetMaterial();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        private void btnAddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (dgMaterialLot.ItemsSource == null || dgMaterialLot.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgMaterialLot.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["CHK"] = false;

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["MTRLID"] = sMTRLID;
            dr["MTRLNAME"] = sMTRLNAME;
            dr["GRADE"] = sGRADE;
            dr["MTRLDESC"] = sMTRLDESC;
            dr["DEL_YN"] = "Y";
            dt.Rows.Add(dr);
        }
        private void btnDeleteMaterialLot_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drs = Util.gridGetChecked(ref dgMaterialLot, "CHK");

            if (drs != null)
            {
                //입력한 데이터가 삭제됩니다. 계속 하시겠습니까?
                Util.MessageConfirm("SFU1815", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        bool isUpdate = false;
                        bool isFailure = false;
                        DataRow[] ds = Util.gridGetChecked(ref dgMaterialLot, "CHK");

                        foreach (DataRow data in ds)
                        {
                            if (string.Equals(data["DEL_YN"], "N"))
                            {
                                isUpdate = true;
                                break;
                            }
                        }

                        if (isUpdate)
                            isFailure = SetMaterialLot(_LOTID, "D", "N");

                        // 해당 ROW들 삭제
                        if (isFailure == false)
                        {
                            for (int i = (dgMaterialLot.Rows.Count - 1); i >= 0; i--)
                            {
                                if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgMaterialLot.Rows[i].DataItem, "CHK")), "1"))
                                {
                                    DataTable dt = DataTableConverter.Convert(dgMaterialLot.ItemsSource);
                                    dt.Rows[i].Delete();
                                    dgMaterialLot.ItemsSource = DataTableConverter.Convert(dt);
                                }
                            }
                        }
                    }
                });
            }
        }
        private void btnSaveMaterialLot_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drs = Util.gridGetChecked(ref dgMaterialLot, "CHK");

            if (drs == null)
            {
                Util.MessageValidation("SFU1662");  //선택한 자재가 없습니다.
                return;
            }

            foreach (DataRow dr in drs)
            {
                if (string.IsNullOrEmpty(dr["INPUT_LOTID"].ToString()))
                {
                    Util.MessageValidation("SFU1984");  //투입자재 LOT ID를 입력하세요.
                    return;
                }
            }
            SetMaterialLot(_LOTID, "A", "Y");
            GetMaterialLot(); // 2024.10.24. 김영국 - DB 저장후 재조회를 통해 INPUT_SEQNO를 가지고 온다.
        }
        private void btnSaveMaterial_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

            if (drs == null)
            {
                Util.MessageValidation("SFU1662");  //선택한 자재가 없습니다.
                return;
            }
            //원자재 자리수 체크
            for (int i = 0; i < dgMaterial.Rows.Count; i++)
            {
                int Coomon_Cs2code = 0;
                string Cs2code = "";
                string Clss2Code = "";
                string MtrlName = "";
                string InputSeqNo = "";

                MtrlName = DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "MTRLNAME") == null ? "" : DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "MTRLNAME").ToString();
                Clss2Code = DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "CLSS2_CODE") == null ? "" : DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "CLSS2_CODE").ToString();
                Cs2code = DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "INPUT_QTY") == null ? "0" : DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "INPUT_QTY").ToString();
                InputSeqNo = DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "INPUT_SEQNO") == null ? "0" : DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "INPUT_SEQNO").ToString();

                try
                {

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                    RQSTDT.Columns.Add("CMCODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = "MIXSER_MTRL_CLSS2_CODE";
                    dr["CMCODE"] = LoginInfo.CFG_SHOP_ID + Clss2Code;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count >= 1)
                    {

                        Coomon_Cs2code = Int16.Parse(dtResult.Rows[0]["ATTRIBUTE1"].ToString());

                        if (Math.Truncate(double.Parse(Cs2code)).ToString().Length >= Coomon_Cs2code)
                        {
                            Util.MessageValidation("SFU8108", MtrlName, Coomon_Cs2code.ToString());

                            return;
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }


            foreach (DataRow dr in drs)
            {
                if (string.IsNullOrEmpty(dr["INPUT_QTY"].ToString()) || dr["INPUT_QTY"].ToString().Equals("0.00000"))
                {
                    Util.MessageValidation("SFU2901");  //사용량을 입력하세요.
                    return;
                }
            }


            SetMaterial("N/A", "A");
        }

        private void dgMainMaterial_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.Index == dgMaterial.Columns["INPUT_QTY"].Index)
                {
                    double inputQty = 0;

                    for (int i = 0; i < dgMaterial.Rows.Count; i++)
                        inputQty += string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "INPUT_QTY"))) ?
                            0 : (Convert.ToDouble(DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "INPUT_QTY")) *
                            (string.Equals(DataTableConverter.GetValue(dgMaterial.Rows[i].DataItem, "UNIT"), "TO") ? 1000 : 1));

                    txtSumInputQty.Value = inputQty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMaterial_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (!dgMaterial.CurrentCell.IsEditing)
                {
                    if (dgMaterial.CurrentCell.Column.Name.Equals("MTRLID"))
                    {
                        string sMTRLNAME;
                        string vMTRLID = Util.NVC(DataTableConverter.GetValue(dgMaterial.CurrentRow.DataItem, "MTRLID"));

                        if (vMTRLID.Equals(""))
                        {
                            return;
                        }
                        else
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("MTRLID", typeof(string));

                            DataRow row = dt.NewRow();
                            row["MTRLID"] = vMTRLID;
                            dt.Rows.Add(row);

                            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATERIAL_MTRLDESC", "INDATA", "RSLTDT", dt);
                            if (result.Rows.Count > 0)
                            {
                                sMTRLNAME = result.Rows[0]["MTRLDESC"].ToString();
                                DataTableConverter.SetValue(dgMaterial.Rows[dgMaterial.SelectedIndex].DataItem, "MTRLDESC", sMTRLNAME);

                                DataTable dt2 = (dgMaterial.ItemsSource as DataView).Table;
                                Util.GridSetData(dgMaterial, dt2, FrameOperation, true);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgMaterialChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                dgMaterial.SelectedIndex = idx;

                sMTRLID = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[idx].DataItem, "MTRLID"));
                sMTRLNAME = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[idx].DataItem, "MTRLNAME"));
                sMTRLDESC = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[idx].DataItem, "MTRLDESC"));
                sGRADE = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[idx].DataItem, "GRADE"));

                txtMTRL.Text = sMTRLID + " : " + sMTRLNAME;
                GetMaterialLot();
            }
        }
        #endregion

        #region Mehod
        private void GetMaterial()
        {
            string sProcMtrlInputQtyApplyFlag = "N";   //[E20230306-000498] GMES CSR nonconformities improvement

            try
            {

                if (string.Equals(_PROCID, Process.MIXING))
                {
                    sProcMtrlInputQtyApplyFlag = GetProcMtrlInputQtyApplyFlag();  //[E20230306-000498] GMES CSR nonconformities improvement
                }

                Util.gridClear(dgMaterial);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));

                //[E20230306-000498] GMES CSR nonconformities improvement
                // CSR : E20241021-000315 조건 추가
                if (Util.NVC(sProcMtrlInputQtyApplyFlag).Equals("Y") || string.Equals(_PROCID, Process.CMC) || string.Equals(_PROCID, Process.BS))
                {
                    IndataTable.Columns.Add("PROCID", typeof(string));
                    IndataTable.Columns.Add("EQPTID", typeof(string));
                }
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = _LOTID;
                Indata["WOID"] = _WOID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = _PRODID;
                Indata["PROD_VER_CODE"] = _PRODVER;

                //[E20230306-000498] GMES CSR nonconformities improvement
                // CSR : E20241021-000315 조건 추가
                if (Util.NVC(sProcMtrlInputQtyApplyFlag).Equals("Y") || string.Equals(_PROCID, Process.CMC) || string.Equals(_PROCID, Process.BS))
                {
                    Indata["PROCID"] = _PROCID;
                    Indata["EQPTID"] = _EQPTID;

                }

                IndataTable.Rows.Add(Indata);

                //[E20230306-000498] GMES CSR nonconformities improvement
                string BizName = "";
                if (Util.NVC(sProcMtrlInputQtyApplyFlag).Equals("Y"))
                {
                    BizName = "DA_PRD_SEL_CONSUME_MATERIAL_INPUTQTY_V02";

                }
                else
                {
                    BizName = "DA_PRD_SEL_CONSUME_MATERIAL_INPUTQTY_V01";
                }

                switch (_PROCID)
                {
                    case "E0400":
                    case "E0410":
                    case "E0420":
                        BizName = "DA_PRD_SEL_CONSUME_MATERIAL_INPUTQTY_SOL";
                        break;
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizName, "INDATA", "RSLTDT", IndataTable);
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONSUME_MATERIAL_INPUTQTY_V01", "INDATA", "RSLTDT", IndataTable);

                dtResult.Columns.Add("CHK", typeof(Int16));

                //[E20240712-001591] start                
                dtResult.Columns.Add(new DataColumn() { ColumnName = "VisibilityButton", DataType = typeof(string) });
                dtResult.Columns.Add(new DataColumn() { ColumnName = "INPUT_CHLOTID_READONLY", DataType = typeof(string) });
                //[E20240712-001591] end

                double inputQty = 0;
                foreach (DataRow row in dtResult.Rows)
                {
                    row["CHK"] = 0;
                    //inputQty += string.IsNullOrEmpty(Util.NVC(row["INPUT_QTY"])) ? 0 : Convert.ToDouble(row["INPUT_QTY"]);
                    inputQty += string.IsNullOrEmpty(Util.NVC(row["INPUT_QTY"])) ? 0 : (Convert.ToDouble(row["INPUT_QTY"]) * (string.Equals(row["UNIT"], "TO") ? 1000 : 1));

                    //[E20240712-001591] start
                    if (_isELEC_MTRL_LOT_VALID_YN)
                    {
                        row["VisibilityButton"] = "Visible";
                        row["INPUT_CHLOTID_READONLY"] = "True";

                    }
                    else
                    {
                        row["VisibilityButton"] = "Collapsed";
                        row["INPUT_CHLOTID_READONLY"] = "False";
                    }
                    //[E20240712-001591] end
                }
                txtSumInputQty.Value = inputQty;

                //dgMaterial.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgMaterial, dtResult, null, true);

                // 일반믹서는 저장된 값이 존재하지 않으면 RATIO에 따른 자동 값 SET
                if (!string.Equals(_PROCID, Process.SRS_MIXING) && inputQty == 0)
                    SetAutoMtrlInputQty(_INPUTQTY);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //[E20230306-000498] GMES CSR nonconformities improvement
        private string GetProcMtrlInputQtyApplyFlag()
        {

            string sProcMtrlInputQtyApplyFlag = "N";
            string sCodeType;
            string sCmCode;
            string[] sAttribute = { _PROCID };

            sCodeType = "PROC_MTRL_INPUT_QTY_APPLY_FLAG";
            sCmCode = null;

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sProcMtrlInputQtyApplyFlag = "Y";
                }
                else
                {
                    sProcMtrlInputQtyApplyFlag = "N";
                }

                return sProcMtrlInputQtyApplyFlag;
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
                sProcMtrlInputQtyApplyFlag = "N";
                return sProcMtrlInputQtyApplyFlag;
            }
        }

        private void SetAutoMtrlInputQty(decimal ratioQty)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = _PRODID;
                Indata["PROD_VER_CODE"] = _PRODVER;
                IndataTable.Rows.Add(Indata);

                string BizName = "DA_PRD_SEL_MIXER_INPUT_MTRL";

                switch (_PROCID)
                {
                    case "E0400":
                    case "E0410":
                        BizName = "DA_PRD_SEL_MIXER_INPUT_MTRL_SOL";
                        break;
                }

                DataTable result = new ClientProxy().ExecuteServiceSync(BizName, "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgMaterial.ItemsSource);
                    decimal totalSum = Util.NVC_Decimal(result.Compute("SUM(INPUT_WEIGHT)", string.Empty));
                    decimal mixRatio = 0;
                    decimal inputQty = 0;

                    foreach (DataRow row in result.Rows)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (string.Equals(row["MTRLID"], dt.Rows[i]["MTRLID"]))
                            {
                                mixRatio = Util.NVC_Decimal(row["INPUT_WEIGHT"]) / (totalSum == 0 ? 1 : totalSum);

                                //DataTableConverter.SetValue(dgMaterial.Rows[i].DataItem, "BASE_QTY", GetUnitFormatted(row["INPUT_WEIGHT"]));

                                if (string.Equals(dt.Rows[i]["UNIT"], "TO"))
                                    DataTableConverter.SetValue(dgMaterial.Rows[i].DataItem, "INPUT_QTY", GetUnitFormatted(ratioQty * mixRatio) / 1000);
                                else
                                    DataTableConverter.SetValue(dgMaterial.Rows[i].DataItem, "INPUT_QTY", GetUnitFormatted(ratioQty * mixRatio));

                                break;
                            }
                        }
                    }

                    // 총 투입량 처리 갱신
                    DataTable dtMtrl = DataTableConverter.Convert(dgMaterial.ItemsSource);

                    // 기준수량은 EMPTY로 SET
                    for (int i = 0; i < dtMtrl.Rows.Count; i++)
                        if (string.IsNullOrEmpty(Util.NVC(dtMtrl.Rows[i]["BASE_QTY"])))
                            DataTableConverter.SetValue(dgMaterial.Rows[i].DataItem, "INPUT_QTY", 0);

                    foreach (DataRow row in dtMtrl.Rows)
                        inputQty += string.IsNullOrEmpty(Util.NVC(row["INPUT_QTY"])) ? 0 : (Util.NVC_Decimal(row["INPUT_QTY"]) * (string.Equals(row["UNIT"], "TO") ? 1000 : 1));

                    // 첫 번째 소수점 계산이 부족할 시 TO아닌 첫 번째 ROW에 차이 수량분 추가
                    if (GetUnitFormatted(ratioQty) != GetUnitFormatted(inputQty))
                    {
                        int iIdx = 0;
                        for (int i = 0; i < dtMtrl.Rows.Count; i++)
                        {
                            if (!string.Equals(dtMtrl.Rows[i]["UNIT"], "TO"))
                            {
                                iIdx = i;
                                break;
                            }
                        }

                        decimal firstQty = string.Equals(DataTableConverter.GetValue(dgMaterial.Rows[iIdx].DataItem, "INPUT_QTY"), "") ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(dgMaterial.Rows[iIdx].DataItem, "INPUT_QTY"));
                        if (GetUnitFormatted(ratioQty) > GetUnitFormatted(inputQty))
                            DataTableConverter.SetValue(dgMaterial.Rows[iIdx].DataItem, "INPUT_QTY", firstQty + Math.Abs(GetUnitFormatted(ratioQty) - GetUnitFormatted(inputQty)));
                        else
                            DataTableConverter.SetValue(dgMaterial.Rows[iIdx].DataItem, "INPUT_QTY", firstQty - Math.Abs(GetUnitFormatted(ratioQty) - GetUnitFormatted(inputQty)));
                    }

                    txtSumInputQty.Value = Convert.ToDouble(ratioQty);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private decimal GetUnitFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = "{0:#,##0.000}";
            double dFormat = 0;

            if (string.IsNullOrEmpty(sValue))
                return Util.NVC_Decimal(String.Format(sFormatted, 0));

            if (Double.TryParse(sValue, out dFormat))
                return Util.NVC_Decimal(String.Format(sFormatted, dFormat));

            return Util.NVC_Decimal(String.Format(sFormatted, 0));
        }

        private void GetMaterialLot()
        {
            try
            {
                Util.gridClear(dgMaterialLot);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = _LOTID;
                Indata["MTRLID"] = sMTRLID;
                Indata["WOID"] = _WOID;
                Indata["PRODID"] = _PRODID;
                IndataTable.Rows.Add(Indata);

                string BizName = "DA_PRD_SEL_CONSUME_MATERIAL_INPUTLOT";

                switch (_PROCID)
                {
                    case "E0400":
                    case "E0410":
                    case "E0420":
                    case "E0430":
                        BizName = "DA_PRD_SEL_CONSUME_MATERIAL_INPUTLOT_SOL";
                        break;
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizName, "INDATA", "RSLTDT", IndataTable);

                dtResult.Columns.Add("CHK", typeof(Int16));

                foreach (DataRow row in dtResult.Rows)
                {
                    row["CHK"] = 0;
                }

                //dgMaterialLot.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgMaterialLot, dtResult, null, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetMaterial(string LotID, string PROC_TYPE)
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["LOTID"] = _LOTID;
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataRow inLotDataRow = null;

                DataTable InLotdataTable = inDataSet.Tables.Add("IN_INPUT");
                InLotdataTable.Columns.Add("INPUT_LOTID", typeof(string));
                InLotdataTable.Columns.Add("MTRLID", typeof(string));
                InLotdataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                InLotdataTable.Columns.Add("PROC_TYPE", typeof(string));
                //InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(Int32));
                InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(string));  // 2024.10.10. 김영국 - SEQNO 자리수가 26자리로 넘어와 형변환 Error 발생. String으로 넘기도록 수정함.
                InLotdataTable.Columns.Add("MTRL_HIST_TYPE_CODE", typeof(string));

                DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

                foreach (DataRow row in dt.Rows)
                {
#if false
                    // 테이블 수정되면 전체 처리로 수정 ; 현재 A, D 구분 처리
                    if (row.RowState == DataRowState.Added)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["MTRLID"]).Equals(""))
                            {
                                inLotDataRow = InLotdataTable.NewRow();
                                inLotDataRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]);
                                inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                                inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY"]);
                                inLotDataRow["PROC_TYPE"] = "A";
                                inLotDataRow["INPUT_DTTM"] = Util.NVC(row["INPUT_DTTM"]);
                                InLotdataTable.Rows.Add(inLotDataRow);
                            }
                        }
                    }
                    else if (row.RowState == DataRowState.Deleted)
                    {
                        if (!Util.NVC(row["MTRLID", DataRowVersion.Original]).Equals(""))
                        {
                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID", DataRowVersion.Original]);
                            inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID", DataRowVersion.Original]);
                            inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY", DataRowVersion.Original]);
                            inLotDataRow["PROC_TYPE"] = "D";
                            inLotDataRow["INPUT_DTTM"] = Util.NVC(row["INPUT_DTTM", DataRowVersion.Original]);
                            InLotdataTable.Rows.Add(inLotDataRow);
                        }
                    }

#else
                    //if (Util.NVC_Decimal(row["INPUT_QTY"]) > 0)
                    //{
                    inLotDataRow = InLotdataTable.NewRow();
                    inLotDataRow["INPUT_LOTID"] = LotID;
                    inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                    inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY"]);
                    inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                    //inLotDataRow["INPUT_SEQNO"] = Util.NVC_Int(row["INPUT_SEQNO"]);
                    inLotDataRow["INPUT_SEQNO"] = row["INPUT_SEQNO"]; // 2024.10.10. 김영국 - SEQNO 자리수가 26자리로 넘어와 형변환 Error 발생. String으로 넘기도록 수정함.
                    inLotDataRow["MTRL_HIST_TYPE_CODE"] = "L";
                    InLotdataTable.Rows.Add(inLotDataRow);
                    //}
#endif
                }
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INPUT_LOT_HIST_MX", "INDATA,IN_INPUT", null, inDataSet);

                GetMaterial();

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }
        private bool SetMaterialLot(string LotID, string PROC_TYPE, string sCodeType, TextBox txtBox = null)
        {
            bool isFailure = false;
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["LOTID"] = _LOTID;
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataRow inLotDataRow = null;

                DataTable InLotdataTable = inDataSet.Tables.Add("IN_INPUT");
                InLotdataTable.Columns.Add("INPUT_LOTID", typeof(string));
                InLotdataTable.Columns.Add("MTRLID", typeof(string));
                InLotdataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                InLotdataTable.Columns.Add("PROC_TYPE", typeof(string));
                //InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(Int32));
                InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(string));
                InLotdataTable.Columns.Add("MTRL_HIST_TYPE_CODE", typeof(string));

                DataTable dt = ((DataView)dgMaterialLot.ItemsSource).Table;

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["MTRLID"]).Equals("")) //&& string.Equals(row["DEL_YN"], sCodeType)
                        {
                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]);
                            inLotDataRow["MTRLID"] = sMTRLID;
                            inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["INPUT_QTY"]);
                            inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                            //inLotDataRow["INPUT_SEQNO"] = Util.NVC_Int(row["INPUT_SEQNO"]);
                            inLotDataRow["INPUT_SEQNO"] = row["INPUT_SEQNO"];
                            inLotDataRow["MTRL_HIST_TYPE_CODE"] = "Q";
                            InLotdataTable.Rows.Add(inLotDataRow);
                        }
                    }
                }
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INPUT_LOT_HIST_MX", "INDATA,IN_INPUT", null, inDataSet);

                //GetMaterialLot();

                //Util.MessageInfo("SFU1270");    //저장되었습니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) =>
                {
                    if (txtBox != null)
                    {
                        txtBox.Focus();
                        txtBox.SelectAll();
                    }
                }, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                isFailure = true;
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
            return isFailure;
        }
        #endregion

        private void txtInputLotID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (dgMaterialLot.ItemsSource == null || dgMaterialLot.Rows.Count < 0)
                    return;

                if (string.IsNullOrEmpty(txtInputLotID.Text.Trim()))
                    return;

                DataRow[] row = Util.gridGetChecked(ref dgMaterial, "CHK");
                if (row == null || row.Length == 0)
                    return;

                DataTable dt = ((DataView)dgMaterialLot.ItemsSource).Table;

                //2024.10.24. 김영국 - 투입LOT에 중복 등록하지 않도록 로직 구현함.
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<string>("MTRLID") == sMTRLID
                                   && t.Field<string>("MTRLNAME") == sMTRLNAME
                                   && t.Field<string>("GRADE") == sGRADE
                                   && t.Field<string>("INPUT_LOTID") == txtInputLotID.Text
                                 select t).ToList();
                if (queryEdit.Count > 0)
                {
                    return;
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["CHK"] = false;

                DataRow dr = dt.NewRow();
                dr["CHK"] = true;
                dr["MTRLID"] = sMTRLID;
                dr["MTRLNAME"] = sMTRLNAME;
                dr["GRADE"] = sGRADE;
                dr["MTRLDESC"] = sMTRLDESC;
                dr["DEL_YN"] = "Y";
                dr["INPUT_LOTID"] = txtInputLotID.Text;
                dt.Rows.Add(dr);

                // SetMaterialLot(_LOTID, "A", "Y", txtInputLotID); //2024.10.24. 김영국 - 기존 로직의 문제로 인해 KeyDown시 저장하지 않도록 함. (최재철 책임 요청)

                //Util.GridSetData(dgMaterialLot, dt, FrameOperation, true);

                txtInputLotID.Text = string.Empty;
                //txtInputLotID.Focus();
                //txtInputLotID.SelectAll();
            }
        }

        private void txtChildLotID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (dgMaterialLot.ItemsSource == null || dgMaterialLot.Rows.Count < 0)
                    return;

                //----------------------------------------------- [E20240712-001591] start
                /*
                TextBox txtBox = sender as TextBox;

                C1.WPF.DataGrid.DataGridCellPresenter presenter = txtBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if ( presenter != null || presenter.Cell != null)
                {
                    if (string.IsNullOrEmpty(txtBox.Text.Trim()))
                        return;

                    DataRow[] row = Util.gridGetChecked(ref dgMaterial, "CHK");
                    if (row == null || row.Length == 0)
                        return;

                    if (!string.Equals(row[0]["MTRLID"], DataTableConverter.GetValue(presenter.Cell.Row.DataItem, "MTRLID")))
                    {
                        txtBox.Text = "";
                        return;
                    }

                    DataTable dt = ((DataView)dgMaterialLot.ItemsSource).Table;

                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["CHK"] = false;

                    DataRow dr = dt.NewRow();
                    dr["CHK"] = true;
                    dr["MTRLID"] = sMTRLID;
                    dr["MTRLNAME"] = sMTRLNAME;
                    dr["GRADE"] = sGRADE;
                    dr["MTRLDESC"] = sMTRLDESC;                    
                    dr["DEL_YN"] = "Y";
                    dr["INPUT_LOTID"] = txtBox.Text;
                    dt.Rows.Add(dr);

                    SetMaterialLot(_LOTID, "A", "Y", txtBox);

                    txtBox.Text = "";
                }
                */

                TextBox textBoxLotId = sender as TextBox;
                if (textBoxLotId == null || string.IsNullOrEmpty(textBoxLotId.Text)) return;

                DataRowView drv = ((FrameworkElement)sender).DataContext as DataRowView;
                if (drv == null) return;

                if (_Util.GetDataGridRowCountByCheck(dgMaterial, "CHK") < 1) return;

                //라디오 박스 선택된 row의 MTRLID
                string selectedLotId = _Util.GetDataGridFirstRowBycheck(dgMaterial, "CHK").Field<string>("MTRLID").GetString();

                //KeyDown 이벤트 Row의 MTRLID 
                string materialId = drv.Row["MTRLID"].ToString();

                if (!string.Equals(selectedLotId, materialId)) return;


                //
                DataTable dt = ((DataView)dgMaterialLot.ItemsSource).Table;

                //2024.10.24. 김영국 - 투입LOT에 중복 등록하지 않도록 로직 구현함.
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<string>("MTRLID") == sMTRLID
                                   && t.Field<string>("MTRLNAME") == sMTRLNAME
                                   && t.Field<string>("GRADE") == sGRADE
                                   && t.Field<string>("INPUT_LOTID") == textBoxLotId.Text
                                 select t).ToList();
                if (queryEdit.Count > 0)
                {
                    return;
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["CHK"] = false;

                DataRow dr = dt.NewRow();
                dr["CHK"] = true;
                dr["MTRLID"] = sMTRLID;
                dr["MTRLNAME"] = sMTRLNAME;
                dr["GRADE"] = sGRADE;
                dr["MTRLDESC"] = sMTRLDESC;
                dr["DEL_YN"] = "Y";
                dr["INPUT_LOTID"] = textBoxLotId.Text;
                dt.Rows.Add(dr);

                //SetMaterialLot(_LOTID, "A", "Y", textBoxLotId); //2024.10.24. 김영국 - 기존 로직의 문제로 인해 KeyDown시 저장하지 않도록 함. (최재철 책임 요청)

                textBoxLotId.Text = "";
                //----------------------------------------------- [E20240712-001591] end
            }
        }

        private void txtRemainInputQty_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SetAutoMtrlInputQty(Util.NVC_Decimal(txtRemainInputQty.Value));
                txtRemainInputQty.Value = 0;
            }
        }

        // [E20240712-001591]
        private void btnMtrl_Click(object sender, MouseButtonEventArgs e)
        {
            Button bt = sender as Button;

            if (bt == null || bt.DataContext == null) return;

            DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

            if (drs == null || drs.Length == 0)
            {
                Util.MessageValidation("SFU1662");  //선택한 자재가 없습니다.
                return;
            }

            _MTRLID = string.Empty;
            _MTRLID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRLID"));

            DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

            foreach (DataRow row in dt.Rows)
            {
                if (row["CHK"].ToString().Equals("1") && !row["MTRLID"].ToString().Equals(_MTRLID))
                {
                    Util.MessageValidation("SFU8935");  //자재코트를 먼저 선택하고 자재Lot 버튼 클릭 하세요!
                    return;
                }
            }

            if (_isELEC_MTRL_LOT_VALID_YN)
            {
                // E20240830-001478 start
                ////미 투입자재 존재 유무 Check
                //if (CheckMissedElecMtrlLot())
                //{
                //    //미투입 자재 PopUp
                //    PopupInputMaterial();
                //    return;
                //}

                //미투입 자재 PopUp
                PopupInputMaterial();
                return;

                // E20240830-001478 end
            }
        }

        // [E20240712-001591]
        private void btnMtrlLot_Click(object sender, RoutedEventArgs e)
        {
            if (_isELEC_MTRL_LOT_VALID_YN)
            {
                // E20240830-001478 start
                ////미 투입자재 존재 유무 Check
                //if (CheckMissedElecMtrlLot())
                //{
                //    //미투입 자재 PopUp
                //    PopupInputMaterial();
                //    return;
                //}

                //미투입 자재 PopUp
                PopupInputMaterial();
                return;

                // E20240830-001478 end
            }
        }

        // [E20240712-001591]
        private void CheckUseElecMtrlLotValidation()
        {
            try
            {
                _isELEC_MTRL_LOT_VALID_YN = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ELEC_MTRL_LOT_VALID_YN";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        if (dtResult.Rows[0]["ATTRIBUTE1"].ToString().Equals("Y"))
                            _isELEC_MTRL_LOT_VALID_YN = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // [E20240712-001591]
        private bool CheckMissedElecMtrlLot()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("WOID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = _LOTID;
                dr["WOID"] = _WOID;
                dr["EQPTID"] = _EQPTID;
                dr["PRODID"] = _PRODID;
                dr["PROCID"] = _PROCID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_LOT_MISSED", "RQSTDT", "RSLTDT", RQSTDT);

                //미 투입자재 존재하면
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // [E20240712-001591]
        private void PopupInputMaterial()
        {
            //CMM_ELEC_MISSED_MTRL_INPUT_LOT popupInputMaterial = new CMM_ELEC_MISSED_MTRL_INPUT_LOT { FrameOperation = FrameOperation };

            //if (popupInputMaterial != null)
            //{
            //    object[] Parameters = new object[6];
            //    Parameters[0] = _LOTID;
            //    Parameters[1] = _WOID;
            //    Parameters[2] = _EQPTID;
            //    Parameters[3] = _PROCID;
            //    Parameters[4] = _PRODID;
            //    Parameters[5] = "M";

            //    C1WindowExtension.SetParameters(popupInputMaterial, Parameters);

            //    popupInputMaterial.Closed += new EventHandler(PopupInputMaterial_Closed);
            //    this.Dispatcher.BeginInvoke(new Action(() => popupInputMaterial.ShowModal()));
            //    popupInputMaterial.BringToFront();
            //}

            CMM_ELEC_MTRL_LOT_SEARCH popupInputMaterial = new CMM_ELEC_MTRL_LOT_SEARCH { FrameOperation = FrameOperation };

            /*
            _LOTID = Util.NVC(tmps[0]);
            _WOID = Util.NVC(tmps[1]);
            _MTRLID = Util.NVC(tmps[2]);
            _MTRLNAME = Util.NVC(tmps[3]);
            _MTRLTYPE = Util.NVC(tmps[4]);
            _PROCID = Util.NVC(tmps[5]);
            _PLMTYPE = Util.NVC(tmps[6]);
            _EQPTID = Util.NVC(tmps[7]);
            _PRODID = Util.NVC(tmps[8]);
            _CALLER = Util.NVC(tmps[9]);
            */

            if (popupInputMaterial != null)
            {
                object[] Parameters = new object[10];
                Parameters[0] = _LOTID;
                Parameters[1] = _WOID;
                //Parameters[2] = _EQPTID;
                //Parameters[3] = _PROCID;
                //Parameters[4] = _PRODID;
                Parameters[2] = _Util.GetDataGridFirstRowBycheck(dgMaterial, "CHK").Field<string>("MTRLID").GetString();
                Parameters[3] = string.Empty;
                Parameters[4] = string.Empty;
                Parameters[5] = _PROCID;
                Parameters[6] = string.Empty;
                Parameters[7] = _EQPTID;
                Parameters[8] = _PRODID;
                Parameters[9] = "M";
                C1WindowExtension.SetParameters(popupInputMaterial, Parameters);

                popupInputMaterial.Closed += new EventHandler(PopupInputMaterial_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupInputMaterial.ShowModal()));
                popupInputMaterial.BringToFront();
            }

        }

        // [E20240712-001591]
        private void PopupInputMaterial_Closed(object sender, EventArgs e)
        {
            GetMaterialLot();
        }

    }
}
