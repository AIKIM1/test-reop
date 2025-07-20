/*****************************************************************************************
 Created Date :
      Creator :
   Decription : Initial Created.
------------------------------------------------------------------------------------------
 [Change Histor]y
  2024.05.20  : [E20240502-001076] Mixer 투입원자재 Tracking 기능 개선- (실적 확정 투입자재 Validation 기능 개발)  Initial Created.
  2024.07.25   : 이원열 [E20240712-001591] 공정진척 & 실적확정 투입lot 조회 validation 추가(대체자제)
  2024.08.10   : 배현우 [E20240807-000861] Dam Mixer 공정이  WO 공정으로 변경됨에따라 비즈 분기 변경
  2024.08.10   : 이원열 [E20240712-001591] 공정진척 & 실적확정 투입lot 조회 validation 추가(대체자제) - btnMtrl_Click() Parameter 추가
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
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// ELEC003_001.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_MISSED_MTRL_INPUT_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LOTID = string.Empty;
        private string _WOID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _PRCSID = string.Empty;
        private string _PRODID = string.Empty;
        private string _MTRLID = string.Empty;

        // [E20240712-001591]
        private string _CALLER = string.Empty;        

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

        public bool bSaveConfirm { get; set; }

        public CMM_ELEC_MISSED_MTRL_INPUT_LOT()
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
            _PRCSID = Util.NVC(tmps[3]);
            _PRODID = Util.NVC(tmps[4]);

            //[E20240712-001591]
            _CALLER = Util.NVC(tmps[5]);

            GetMaterial();            
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
 
        private void btnSaveMaterial_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

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

            SetMaterialLot(_LOTID, "A");

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

            }
        }

        private void btnMtrl_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Button bt = sender as Button;
                
                if (bt == null || bt.DataContext == null) return;

                DataRow[] drs = Util.gridGetChecked(ref dgMaterial, "CHK");

                if (drs == null || drs.Length == 0   )
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

                CMM_ELEC_MTRL_LOT_SEARCH popupMaterial = new CMM_ELEC_MTRL_LOT_SEARCH { FrameOperation = FrameOperation };

                if (popupMaterial != null)
                {
                    // [E20240712-001591] start
                    //object[] Parameters = new object[6];
                    object[] Parameters = new object[10];

                    Parameters[0] = _LOTID;
                    Parameters[1] = _WOID;

                    // [E20240712-001591] start
                    //Parameters[2] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRLID"));                // 자재ID
                    string sMtrlChk = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRLID"));

                    if (sMtrlChk.Substring(0, 1) == "*")
                    {
                        Parameters[2] = sMtrlChk.Substring(1);  // "*" 제거
                    }
                    else
                    {
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRLID"));
                    }
                    // [E20240712-001591] end

                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRLNAME"));              // 자재명
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRLTYPE"));              // 자재TYPE
                    Parameters[5] = _PRCSID;
                    //[E20240712-001591]
                    Parameters[6] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "PLM_TYPE"));
                    Parameters[7] = string.Empty;
                    Parameters[8] = string.Empty;
                    Parameters[9] = string.Empty;

                    C1WindowExtension.SetParameters(popupMaterial, Parameters);
                    popupMaterial.Closed += new EventHandler(popupMaterial_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupMaterial.ShowModal()));
                    popupMaterial.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupMaterial_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_MTRL_LOT_SEARCH popup = sender as CMM_ELEC_MTRL_LOT_SEARCH;


            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (string.IsNullOrEmpty(popup._ReturnLotID))
                    return;
                
                string[] sVal = popup._ReturnLotID.Split(',');

                DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

                //중복Lot Check
                for (int i = sVal.Length; i > 0; i--)
                {
                    string sChkVal = sVal[i - 1].ToString();
                    DataRow[] ChkRows = dt.Select("INPUT_LOTID LIKE '%" + sChkVal + "%'");

                    if (ChkRows.Length > 0)
                    {
                        Util.MessageValidation("SFU1507"); // 동일한 자재LOT ID가 이미 입력되어 있습니다.
                        return;
                    }
                }

                foreach (DataRow row in dt.Rows)
                {
                    if (row["CHK"].ToString().Equals("1") && row["MTRLID"].ToString().Equals(_MTRLID))
                    {
                        if (string.IsNullOrEmpty(row["INPUT_LOTID"].ToString()))
                        {
                            row["INPUT_LOTID"] = popup._ReturnLotID;
                            row["USED_QTY"] = popup._ReturnLotQty;
                        }
                        else
                        {
                            row["INPUT_LOTID"] = row["INPUT_LOTID"].ToString() + "," + popup._ReturnLotID;
                            row["USED_QTY"] = row["USED_QTY"].ToString() + "," + popup._ReturnLotQty;
                        }
                    }
                }

                Util.gridClear(dgMaterial);
                Util.GridSetData(dgMaterial, dt, FrameOperation, true);

                // [E20240712-001591]
                if (_CALLER.Equals("M"))
                {
                    btnSaveMaterial_Click(null, null);
                    btnClose_Click(null, null);
                }
            }
        }

        private void txtChildLotID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key >= System.Windows.Input.Key.D0 && e.Key <= System.Windows.Input.Key.Z) ||
                (e.Key >= System.Windows.Input.Key.NumPad0 && e.Key <= System.Windows.Input.Key.NumPad9))
            {
                Util.MessageValidation("SFU8936"); // 자재 Lot은 돋보기 버튼을 클릭해서 선택 바랍니다.
                return;
            }
        }

        private void Textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"[^a-zA-Z0-9\s]");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                Util.MessageValidation("SFU8936"); // 자재 Lot은 돋보기 버튼을 클릭해서 선택 바랍니다.
                return;
            }
        }

        private void Textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.ImeProcessed)
            {
                e.Handled = true;
                Util.MessageValidation("SFU8936"); // 자재 Lot은 돋보기 버튼을 클릭해서 선택 바랍니다.
                return;
            }
        }
        #endregion

        #region Mehod
        private void GetMaterial()
        {
           
            try
            { 
                Util.gridClear(dgMaterial);

                string BizName = string.Empty;

                DataTable RQSTDT = new DataTable();

                if (_PRCSID == Process.MIXING || _PRCSID == Process.PRE_MIXING || _PRCSID == Process.DAM_MIXING)
                {

                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("WOID", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LOTID"] = _LOTID;
                    dr["WOID"] = _WOID;
                    dr["EQPTID"] = _EQPTID;

                    RQSTDT.Rows.Add(dr);

                    BizName = "DA_PRD_CHK_INPUT_LOT_MISSED";
                }
                else if (_PRCSID == Process.BS || _PRCSID == Process.CMC || _PRCSID == Process.InsulationMixing)
                {
                    RQSTDT.Columns.Add("PRODID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["PRODID"] = _PRODID;
                    dr["LOTID"] = _LOTID;
                    dr["EQPTID"] = _EQPTID;

                    RQSTDT.Rows.Add(dr);

                    BizName = "DA_PRD_CHK_INPUT_LOT_MISSED_SOL";
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizName, "RQSTDT", "RSLTDT", RQSTDT);
                
                dtResult.Columns.Add("CHK", typeof(Int16));
                dtResult.Columns.Add("USED_QTY", typeof(string));

                foreach (DataRow row in dtResult.Rows)
                {
                    row["CHK"] = 0;
                    row["USED_QTY"] = string.Empty;
                }

                txtTitle.Text = ObjectDic.Instance.GetObjectName("미투입자재 :") + " " +  dtResult.Rows.Count.ToString() +" "+  ObjectDic.Instance.GetObjectName("건");
                Util.GridSetData(dgMaterial, dtResult, null, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SetMaterialLot(string LotID, string PROC_TYPE)
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
                InLotdataTable.Columns.Add("INPUT_SEQNO", typeof(Int32));
                InLotdataTable.Columns.Add("MTRL_HIST_TYPE_CODE", typeof(string));

                DataTable dt = ((DataView)dgMaterial.ItemsSource).Table;

                decimal sumInQty = 0; //자재코드별 자재Lot 수량 총합

                foreach (DataRow row in dt.Rows)
                {
                    // [E20240712-001591] start
                    string sMtrlID = Util.NVC(row["MTRLID"]);

                    if (sMtrlID.Substring(0, 1) == "*")
                    {
                        sMtrlID = sMtrlID.Substring(1);  // "*" 제거
                    }
                    // [E20240712-001591] end



                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["MTRLID"]).Equals("")) 
                        {
                            for (int j = 0; j < Util.NVC(row["INPUT_LOTID"]).Split(',').Length; j++)
                            {
                                inLotDataRow = InLotdataTable.NewRow();
                                inLotDataRow["INPUT_LOTID"] = Util.NVC(row["INPUT_LOTID"]).Split(',')[j].Trim();
                                // [E20240712-001591]
                                //inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                                inLotDataRow["MTRLID"] = sMtrlID;

                                inLotDataRow["INPUT_QTY"] = Util.NVC_Decimal(row["USED_QTY"].ToString().Split(',')[j].Trim());
                                inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                                inLotDataRow["INPUT_SEQNO"] = 0;
                                inLotDataRow["MTRL_HIST_TYPE_CODE"] = "Q";
                                InLotdataTable.Rows.Add(inLotDataRow);
                                sumInQty += Util.NVC_Decimal(row["USED_QTY"].ToString().Split(',')[j].Trim());
                            }

                            inLotDataRow = InLotdataTable.NewRow();
                            inLotDataRow["INPUT_LOTID"] = "N/A";
                            // [E20240712-001591]
                            //inLotDataRow["MTRLID"] = Util.NVC(row["MTRLID"]);
                            inLotDataRow["MTRLID"] = sMtrlID;

                            inLotDataRow["INPUT_QTY"] = sumInQty;
                            inLotDataRow["PROC_TYPE"] = PROC_TYPE;
                            inLotDataRow["INPUT_SEQNO"] = 0;
                            inLotDataRow["MTRL_HIST_TYPE_CODE"] = "L";
                            InLotdataTable.Rows.Add(inLotDataRow);

                        }
                    }
                }

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INPUT_LOT_HIST_MX", "INDATA,IN_INPUT", null, inDataSet);

                GetMaterial();
                bSaveConfirm = true;
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
        #endregion

    }
}