/*****************************************************************************************
 Created Date :
      Creator :
   Decription : Initial Created.
------------------------------------------------------------------------------------------
 [Change History]
  2024.05.20  : [E20240502-001076] Mixer 투입원자재 Tracking 기능 개선- (실적 확정 투입자재 Validation 기능 개발)  Initial Created.
  2024.07.25  : 이원열 [E20240712-001591] 공정진척 & 실적확정 투입lot 조회 validation 추가(대체자제) - Logic 전체 변경
  2024.08.30  : 이원열 [E20240830-001478] 투입자재 팝업의 경우 투입 자재가 모두 등록된 경우에도 추가 LOT 을 등록할 수 있도록 Logic 변경
******************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_MISSED_MTRL_INPUT_LOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_MTRL_LOT_SEARCH : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LOTID = string.Empty;
        private string _WOID = string.Empty;
        private string _MTRLID = string.Empty;
        private string _MTRLNAME = string.Empty;
        private string _MTRLTYPE = string.Empty;
        private string _PROCID = string.Empty;
        private string _LotIDList = string.Empty;
        private string _LotQtyList = string.Empty;
        private string _EQPTID = string.Empty;
        private string _PRODID = string.Empty;
        private string _PLMTYPE = string.Empty;
        private string _CALLER = string.Empty;        
        private string _MTRL_PROD_FLAG = string.Empty;

        DataTable DtInfo = new DataTable();

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LotIDList; }
        }

        public string _ReturnLotQty
        {
            get { return _LotQtyList; }
        }

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
        public CMM_ELEC_MTRL_LOT_SEARCH()
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
            _MTRLID = Util.NVC(tmps[2]);
            _MTRLNAME = Util.NVC(tmps[3]);
            _MTRLTYPE = Util.NVC(tmps[4]);
            _PROCID = Util.NVC(tmps[5]);
            _PLMTYPE = Util.NVC(tmps[6]);
            _EQPTID = Util.NVC(tmps[7]);
            _PRODID = Util.NVC(tmps[8]);
            _CALLER = Util.NVC(tmps[9]);

            if (_CALLER.Equals("M"))
            {                
                GetCHK_INPUT_LOT_MISSED_INFO();
            }

            //SetVisibleObject_MtrlType();
            SetVisibleObject_Component();

            //GetMaterial();
        }

        // [E20240712-001591]
        private void GetCHK_INPUT_LOT_MISSED_INFO()
        {
            try
            {
                Util.gridClear(dgMaterial);

                string BizName = string.Empty;

                DataTable RQSTDT = new DataTable();

                if (_PROCID == Process.MIXING || _PROCID == Process.PRE_MIXING || _PROCID == Process.DAM_MIXING)
                {

                    RQSTDT.Columns.Add("WOID", typeof(string));
                    RQSTDT.Columns.Add("MTRLID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));                    
                    RQSTDT.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["WOID"] = _WOID;
                    dr["MTRLID"] = _MTRLID;
                    dr["LOTID"] = _LOTID;                    
                    dr["EQPTID"] = _EQPTID;

                    RQSTDT.Rows.Add(dr);

                    BizName = "DA_PRD_CHK_INPUT_LOT_INFO";
                }
                else if (_PROCID == Process.BS || _PROCID == Process.CMC || _PROCID == Process.InsulationMixing)
                {
                    RQSTDT.Columns.Add("PRODID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["PRODID"] = _PRODID;
                    dr["LOTID"] = _LOTID;
                    dr["EQPTID"] = _EQPTID;

                    RQSTDT.Rows.Add(dr);

                    BizName = "DA_PRD_CHK_INPUT_LOT_SOL_INFO";
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizName, "RQSTDT", "RSLTDT", RQSTDT);
                
                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                dtResult.Columns.Add("CHK", typeof(Int16));
                dtResult.Columns.Add("USED_QTY", typeof(string));

                foreach (DataRow row in dtResult.Rows)
                {
                    row["CHK"] = 0;
                    row["USED_QTY"] = string.Empty;
                }

                DtInfo = dtResult.Copy();
                
                _MTRLTYPE = DtInfo.Rows[0]["MTRLTYPE"].ToString();
                _PLMTYPE = DtInfo.Rows[0]["PLM_TYPE"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }        

        private bool SetMaterialLot(string LotID, string PROC_TYPE, string _LotIDList, string _LotQtyList)
        {
            bool isFailure = false;

            foreach (DataRow row in DtInfo.Rows)
            {                
                if (string.IsNullOrEmpty(row["INPUT_LOTID"].ToString()))
                {
                    row["INPUT_LOTID"] = _LotIDList;
                    row["USED_QTY"] = _LotQtyList;
                }
                else
                {
                    row["INPUT_LOTID"] = row["INPUT_LOTID"].ToString() + "," + _LotIDList;
                    row["USED_QTY"] = row["USED_QTY"].ToString() + "," + _LotQtyList;
                }                
            }

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
                InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(Int32));
                InLotdataTable.Columns.Add("MTRL_HIST_TYPE_CODE", typeof(string));

                DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

                decimal sumInQty = 0; //자재코드별 자재Lot 수량 총합

                /*
                foreach (DataRow row in dt.Rows)
                {                    
                    if (Convert.ToBoolean(row["CHK"]))
                    {                        
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            inLotDataRow = InLotdataTable.NewRow();

                            //inLotDataRow["INPUT_LOTID"] = Util.NVC(row["LOTID"]).Trim();
                            inLotDataRow["INPUT_LOTID"] = _MTRL_PROD_FLAG.Equals("MTRL") ? Util.NVC(row["SUPPLIER_LOTID"]).Trim() : Util.NVC(row["LOTID"]).Trim();


                            inLotDataRow["MTRLID"] = DtInfo.Rows[0]["MTRLID"].ToString();

                            
                            inLotDataRow["INPUT_QTY"] = _MTRL_PROD_FLAG.Equals("MTRL") ? Util.NVC_Decimal(row["MTRL_QTY"].ToString().Trim()) : Util.NVC_Decimal(row["WIPQTY_ED"].ToString().Trim());

                            inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                            inLotDataRow["INPUT_SEQNO"] = 0;
                            inLotDataRow["MTRL_HIST_TYPE_CODE"] = "Q";
                            InLotdataTable.Rows.Add(inLotDataRow);
                            sumInQty += _MTRL_PROD_FLAG.Equals("MTRL") ? Util.NVC_Decimal(row["MTRL_QTY"].ToString().Trim()) : Util.NVC_Decimal(row["WIPQTY_ED"].ToString().Trim());
                        }

                        inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["INPUT_LOTID"] = "N/A";                            
                        inLotDataRow["MTRLID"] = DtInfo.Rows[0]["MTRLID"].ToString();
                        inLotDataRow["INPUT_QTY"] = sumInQty;
                        inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                        inLotDataRow["INPUT_SEQNO"] = 0;
                        inLotDataRow["MTRL_HIST_TYPE_CODE"] = "L";
                        InLotdataTable.Rows.Add(inLotDataRow);                        
                    }
                }
                */


                foreach (DataRow row in DtInfo.Rows)
                {   
                    if (!Util.NVC(row["MTRLID"]).Equals(""))
                    {
                        for (int j = 0; j < Util.NVC(row["INPUT_LOTID"]).Split(',').Length; j++)
                        {
                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]).Split(',')[j].Trim();                            
                            inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                            inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["USED_QTY"].ToString().Split(',')[j].Trim());
                            inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                            inLotDataRow["INPUT_SEQNO"] = 0;
                            inLotDataRow["MTRL_HIST_TYPE_CODE"] = "Q";
                            InLotdataTable.Rows.Add(inLotDataRow);
                            sumInQty += Util.NVC_Decimal(row["USED_QTY"].ToString().Split(',')[j].Trim());
                        }

                        inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["INPUT_LOTID"] = "N/A";                        
                        inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                        inLotDataRow["INPUT_QTY"] = sumInQty;
                        inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                        inLotDataRow["INPUT_SEQNO"] = 0;
                        inLotDataRow["MTRL_HIST_TYPE_CODE"] = "L";
                        InLotdataTable.Rows.Add(inLotDataRow);
                    }                    
                }


                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INPUT_LOT_HIST_MX", "INDATA,IN_INPUT", null, inDataSet);

                //GetMaterial();                
                Util.MessageInfo("SFU1270");    //저장되었습니다.
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgMaterial);

            txtPalletId.Text = string.Empty;
            txtSupplierId.Text = string.Empty;
            txtLotId.Text = string.Empty;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
       
        private void btnAddMaterialLot_Click(object sender, RoutedEventArgs e)
        {            
            DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

            if (drs.Count() <= 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }
            
            _LotIDList = string.Empty;
            _LotQtyList = string.Empty;

            foreach (DataRow dr in drs)
            {
                if (_MTRL_PROD_FLAG.Equals("MTRL"))
                {
                    if (!string.IsNullOrEmpty(dr["SUPPLIER_LOTID"].ToString()))
                    {
                        _LotIDList += dr["SUPPLIER_LOTID"].ToString() + ",";
                        _LotQtyList += dr["MTRL_QTY"].ToString() + ",";
                    }
                }
                else if (_MTRL_PROD_FLAG.Equals("PROD"))
                {
                    if (!string.IsNullOrEmpty(dr["LOTID"].ToString()))
                    {
                        _LotIDList += dr["LOTID"].ToString() + ",";
                        _LotQtyList += dr["WIPQTY_ED"].ToString() + ",";
                    }
                }
            }

            string sLotIDList = _LotIDList.Substring(0, _LotIDList.Length - 1);
            string sLotQtyList = _LotQtyList.Substring(0, _LotQtyList.Length - 1);

            if (_CALLER.Equals("M"))
            {
                SetMaterialLot(_LOTID, "A", sLotIDList, sLotQtyList);
            }
            else
            {
                _LotIDList = sLotIDList;
                _LotQtyList = sLotQtyList;
            }
            
            btnClose_Click(null, null);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            if (_MTRLTYPE.Equals("MTRL"))
            {
                if (string.IsNullOrEmpty(txtPalletId.Text.ToString()) && string.IsNullOrEmpty(txtSupplierId.Text.ToString()))
                {
                    Util.MessageValidation("SFU5018");  //조회조건이 하나라도 있어야 합니다.
                    return;
                }

            }
            else if (_MTRLTYPE.Equals("PROD"))
            {
                if (string.IsNullOrEmpty(txtLotId.Text.ToString())) 
                {
                    Util.MessageValidation("SFU4494");  //조회조건 입력 후 조회해야합니다.
                    return;
                }
            }

            GetMaterial();
        }

        private void txtPalletId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {

                if (string.IsNullOrEmpty(txtPalletId.Text.Trim()))
                    return;

                SetVisibleObject_MtrlType();
                GetMaterial();
            }
        }

        private void txtSupplierId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {

                if (string.IsNullOrEmpty(txtSupplierId.Text.Trim()))
                    return;

                SetVisibleObject_MtrlType();
                GetMaterial();
            }
        }

        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {

                if (string.IsNullOrEmpty(txtLotId.Text.Trim()))
                    return;
                
                SetVisibleObject_ProdType();
                GetMaterial();
            }
        }

        #endregion

        #region Mehod
        private void GetMaterial()
        {
            try
            {                
                string sPLM_BizName = string.Empty;
                DataTable PLM_RQSTDT = new DataTable();

                sPLM_BizName = "DA_BAS_SEL_WIP_WITH_ATTR";

                PLM_RQSTDT.Columns.Add("LOTID", typeof(string));
                DataRow plm_dr = PLM_RQSTDT.NewRow();

                plm_dr["LOTID"] = string.IsNullOrWhiteSpace(txtLotId.Text) ? null : txtLotId.Text;
                PLM_RQSTDT.Rows.Add(plm_dr);
                DataTable dtPLM_Result = new ClientProxy().ExecuteServiceSync(sPLM_BizName, "RQSTDT", "RSLTDT", PLM_RQSTDT);

                string sBizName = string.Empty;

                DataTable RQSTDT = new DataTable();
                
                if (_MTRL_PROD_FLAG.Equals("MTRL"))
                {
                    if (_CALLER.Equals("M"))
                    {
                        sBizName = "DA_PRD_SEL_MIXER_IWMS_BCD_LOT_BY_MTRLID_M";
                    }
                    else
                    {
                        sBizName = "DA_PRD_SEL_MIXER_IWMS_BCD_LOT_BY_MTRLID";
                    }

                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("MTRLID", typeof(string));
                    RQSTDT.Columns.Add("PLLT_ID", typeof(string));
                    RQSTDT.Columns.Add("SUPPLIER_LOTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["MTRLID"] = _MTRLID;
                    dr["PLLT_ID"] = string.IsNullOrWhiteSpace(txtPalletId.Text) ? null : txtPalletId.Text;
                    dr["SUPPLIER_LOTID"] = string.IsNullOrWhiteSpace(txtSupplierId.Text) ? null : txtSupplierId.Text;

                    RQSTDT.Rows.Add(dr);
                }                
                else if (_MTRL_PROD_FLAG.Equals("PROD"))
                {
                    sBizName = "DA_PRD_SEL_MIXER_WIPLOT_BY_PRODID";

                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("PRODID", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;

                    if (_PLMTYPE.Equals("N"))
                        dr["PRODID"] = _MTRLID;
                    else
                    {
                        if (dtPLM_Result.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                            return;
                        }                            

                        dr["PRODID"] = dtPLM_Result.Rows[0]["PRODID"].ToString();
                    }

                    dr["PROCID"] = _PROCID;
                    dr["LOTID"] = string.IsNullOrWhiteSpace(txtLotId.Text) ? null : txtLotId.Text;

                    RQSTDT.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dgMaterial.ItemsSource != null || dgMaterial.Rows.Count > 0)
                    {
                        DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;
                        dtResult.Columns.Add("CHK", typeof(Int16));

                        foreach (DataRow row in dtResult.Rows)
                        {
                            if (_MTRL_PROD_FLAG.Equals("MTRL"))
                            {
                                DataRow[] ChkMtrlRows = dt.Select("PLLT_ID = '" + row["PLLT_ID"].ToString() + "' AND SUPPLIER_LOTID = '" + row["SUPPLIER_LOTID"].ToString() + "'");

                                if (ChkMtrlRows.Length > 0)
                                {
                                    Util.MessageValidation("SFU1507"); // 동일한 자재LOT ID가 이미 입력되어 있습니다.
                                    return;
                                }

                                DataRow dr = dt.NewRow();
                                dr["MTRLID"] = row["MTRLID"].ToString();
                                dr["MTRLNAME"] = row["MTRLNAME"].ToString();
                                dr["PLLT_ID"] = row["PLLT_ID"].ToString();
                                dr["SUPPLIER_LOTID"] = row["SUPPLIER_LOTID"].ToString();
                                dr["IWMS_2D_BCD_STR"] = row["IWMS_2D_BCD_STR"].ToString();
                                dr["MTRL_QTY"] = row["MTRL_QTY"].ToString();
                                dr["INSDTTM"] = row["INSDTTM"].ToString();
                                dr["SUPPLIERID"] = row["SUPPLIERID"].ToString();
                                dr["SUPPLIERNAME"] = row["SUPPLIERNAME"].ToString();
                                dt.Rows.Add(dr);
                            }
                            else if (_MTRL_PROD_FLAG.Equals("PROD"))
                            {
                                DataRow[] ChkLotRows = dt.Select("LOTID = '" + row["LOTID"].ToString() + "'");

                                if (ChkLotRows.Length > 0)
                                {
                                    Util.MessageValidation("SFU1507"); // 동일한 LOT ID가 있습니다.
                                    return;
                                }

                                DataRow dr = dt.NewRow();
                                dr["LOTID"] = row["LOTID"].ToString();
                                dr["PRODID"] = row["PRODID"].ToString();
                                dr["PRODNAME"] = row["PRODNAME"].ToString();
                                dr["PRJT_NAME"] = row["PRJT_NAME"].ToString();
                                dr["LOTYNAME"] = row["LOTYNAME"].ToString();
                                dr["WIPQTY_ED"] = row["WIPQTY_ED"].ToString();
                                dr["CALDATE"] = row["CALDATE"].ToString();
                                dr["ENDDTTM"] = row["ENDDTTM"].ToString();
                                dt.Rows.Add(dr);
                            }
                        }

                        Util.GridSetData(dgMaterial, dt, null, true);
                    }
                    else
                    {
                        Util.gridClear(dgMaterial);
                        dtResult.Columns.Add("CHK", typeof(Int16));
                        Util.GridSetData(dgMaterial, dtResult, null, true);
                    }

                    txtPalletId.Text = string.Empty;
                    txtSupplierId.Text = string.Empty;
                    txtLotId.Text = string.Empty;
                }
                else
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SetVisibleObject_Component()
        {
            if (_PLMTYPE.Equals("Y"))
            {
                txtPalletId.IsEnabled = true;
                txtSupplierId.IsEnabled = true;
                txtLotId.IsEnabled = true;

                // default
                SetVisibleObject_MtrlType();
            }
            else
            {
                if (_MTRLTYPE.Equals("MTRL"))
                {
                    txtPalletId.IsEnabled = true;
                    txtSupplierId.IsEnabled = true;
                    txtLotId.IsEnabled = false;

                    SetVisibleObject_MtrlType();
                }
                else
                {
                    txtPalletId.IsEnabled = false;
                    txtSupplierId.IsEnabled = false;
                    txtLotId.IsEnabled = true;

                    SetVisibleObject_ProdType();
                }
            }

            txtMtrlId.Text = _MTRLID;
            txtMtrName.Text = _MTRLNAME;
        }
        
        private void SetVisibleObject_MtrlType()
        {
            _MTRL_PROD_FLAG = "MTRL";

            /*원자재 정보*/
            dgMaterial.Columns["MTRLID"].Visibility = Visibility.Visible;
            dgMaterial.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["PLLT_ID"].Visibility = Visibility.Visible;
            dgMaterial.Columns["SUPPLIER_LOTID"].Visibility = Visibility.Visible;
            dgMaterial.Columns["IWMS_2D_BCD_STR"].Visibility = Visibility.Visible;
            dgMaterial.Columns["MTRL_QTY"].Visibility = Visibility.Visible;
            dgMaterial.Columns["INSDTTM"].Visibility = Visibility.Visible;
            dgMaterial.Columns["SUPPLIERID"].Visibility = Visibility.Visible;
            dgMaterial.Columns["SUPPLIERNAME"].Visibility = Visibility.Visible;
            /*반제품 정보*/
            dgMaterial.Columns["LOTID"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["PRODID"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["LOTYNAME"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["WIPQTY_ED"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["CALDATE"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["ENDDTTM"].Visibility = Visibility.Collapsed;
        }
        
        private void SetVisibleObject_ProdType()
        {
            _MTRL_PROD_FLAG = "PROD";

            /*원자재 정보*/
            dgMaterial.Columns["MTRLID"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["PLLT_ID"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["SUPPLIER_LOTID"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["IWMS_2D_BCD_STR"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["MTRL_QTY"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["INSDTTM"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["SUPPLIERID"].Visibility = Visibility.Collapsed;
            dgMaterial.Columns["SUPPLIERNAME"].Visibility = Visibility.Collapsed;
            /*반제품 정보*/
            dgMaterial.Columns["LOTID"].Visibility = Visibility.Visible;
            dgMaterial.Columns["PRODID"].Visibility = Visibility.Visible;
            dgMaterial.Columns["PRODNAME"].Visibility = Visibility.Visible;
            dgMaterial.Columns["PRJT_NAME"].Visibility = Visibility.Visible;
            dgMaterial.Columns["LOTYNAME"].Visibility = Visibility.Visible;
            dgMaterial.Columns["WIPQTY_ED"].Visibility = Visibility.Visible;
            dgMaterial.Columns["CALDATE"].Visibility = Visibility.Visible;
            dgMaterial.Columns["ENDDTTM"].Visibility = Visibility.Visible;
        }

        #endregion

    }
}