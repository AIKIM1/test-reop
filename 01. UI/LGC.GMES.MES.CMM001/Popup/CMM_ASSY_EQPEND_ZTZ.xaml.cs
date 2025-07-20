/*************************************************************************************
 Created Date : 2023.10.25
      Creator : 김용군
   Decription : ZZS라인 ZTZ 공정진척 - 작업시작(CMM_ASSY_EQPEND Copy 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.25  김용군 : Initial Created.
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_EQPEND_ZTZ.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_EQPEND_ZTZ : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _ProcID = string.Empty;
        private string _StackingYN = string.Empty;
        private string _StartDttm = string.Empty;
        private string _NOWDttm = string.Empty;

        private string _EqptMountPstnId = string.Empty;
        private string _EqptMountPstnState = string.Empty;
        private string _InputLotID = string.Empty;
        private decimal _InputQty = 0;
        private decimal _InputQtyPtn = 0;
        private decimal _OutputQty = 0;
        private decimal _OutputQtyPtn = 0;
        private string _CutFlag = string.Empty;
        private string _EqptDfctCode = string.Empty;
        private decimal _DfctQty = 0;

        DateTime dtStartTime;
        DateTime dtNowTime;

        private bool bEndSetTime = false;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
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
        public CMM_ASSY_EQPEND_ZTZ()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {   
            if (DateTime.TryParse(_StartDttm, out dtStartTime))
            {
            }
            else
            {
                dtStartTime = System.DateTime.Now;                
            }

            
            if (DateTime.TryParse(_NOWDttm, out dtNowTime))
            {
            }
            else
            {
                dtNowTime = System.DateTime.Now;
            }
            if (ldpDatePicker != null)
                ldpDatePicker.SelectedDateTime = dtNowTime;
            if (teTimeEditor != null)
                teTimeEditor.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 16)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                    _LotID = Util.NVC(tmps[3]);
                    _WipSeq = Util.NVC(tmps[4]);
                    _StackingYN = Util.NVC(tmps[5]);
                    _StartDttm = Util.NVC(tmps[6]);
                    _NOWDttm = Util.NVC(tmps[7]);
                    _EqptMountPstnId = Util.NVC(tmps[8]);
                    _EqptMountPstnState = Util.NVC(tmps[9]);
                    _InputLotID = Util.NVC(tmps[10]);
                    _InputQty = Convert.ToDecimal(tmps[11]);
                    _InputQtyPtn = Convert.ToDecimal(tmps[12]);
                    _OutputQty = Convert.ToDecimal(tmps[13]);
                    _OutputQtyPtn = Convert.ToDecimal(tmps[14]);
                    _CutFlag = Util.NVC(tmps[15]);
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _ProcID = "";
                    _LotID = "";
                    _WipSeq = "";
                    _StackingYN = "";
                    _StartDttm = "";
                    _NOWDttm = "";
                    _EqptMountPstnId = "";
                    _EqptMountPstnState = "";
                    _InputLotID = "";
                    _InputQty = 0;
                    _InputQtyPtn = 0;
                    _OutputQty = 0;
                    _OutputQtyPtn = 0;
                    _CutFlag = "";
                }           

                ApplyPermissions();
                
                InitializeControls();

                bEndSetTime = true;

                txtLotID.Text = _LotID;
                txtStartTime.Text = _StartDttm;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnEqpend_Clicked(object sender, RoutedEventArgs e)
        {
            RunComplete_Ztz();
        }

        private void ldpDatePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (sender == null)
                        return;

                    if (!bEndSetTime)
                        return;

                    DateTime dtTime;
                    TimeSpan spn = ((TimeSpan)teTimeEditor.Value);
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);

                    if (Math.Truncate(dtTime.Subtract(dtStartTime).TotalSeconds) >= 0)
                    {
                    }
                    else
                    {
                        //Util.MessageValidation("시작시간보다 이전은 선택할 수 없습니다.");
                        Util.MessageValidation("SFU3089");

                        // 시작시간보다 작으면 초기화.
                        if (ldpDatePicker != null)
                            ldpDatePicker.SelectedDateTime = (DateTime)dtNowTime;
                        if (teTimeEditor != null)
                            teTimeEditor.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);
                    }
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private void teTimeEditor_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (sender == null)
                        return;

                    if (!bEndSetTime)
                        return;

                    DateTime dtTime;
                    TimeSpan spn = ((TimeSpan)teTimeEditor.Value);
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);

                    if (Math.Truncate(dtTime.Subtract(dtStartTime).TotalSeconds) >= 0)
                    {
                    }
                    else
                    {
                        //Util.MessageValidation("시작시간보다 이전은 선택할 수 없습니다.");
                        Util.MessageValidation("SFU3089");

                        // 시작시간보다 작으면 초기화.
                        if (ldpDatePicker != null)
                            ldpDatePicker.SelectedDateTime = (DateTime)dtNowTime;
                        if (teTimeEditor != null)
                            teTimeEditor.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        #endregion

        
        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnEqpend);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

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

        private void RunComplete_Ztz()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (string.Equals(_ProcID, Process.ZTZ))
                {
                    bizRuleName = "BR_PRD_REG_EQPT_END_LOT_ASSY_LS";
                }

                ShowLoadingIndicator();

                DateTime dtTime;
                TimeSpan spn;
                if (teTimeEditor.Value.HasValue)
                {
                    spn = ((TimeSpan)teTimeEditor.Value);
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day);
                }

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_INPUT");
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                inDataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("INPUT_QTY_PTN", typeof(decimal));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));
                inDataTable.Columns.Add("OUTPUT_QTY_PTN", typeof(decimal));
                inDataTable.Columns.Add("CUT_FLAG", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_DEFECT");
                inDataTable.Columns.Add("INPUT_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_DFCT_CODE", typeof(string));
                inDataTable.Columns.Add("DFCT_QTY", typeof(decimal));

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                newRow = inMtrlTable.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = _EqptMountPstnId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = _EqptMountPstnState;
                newRow["INPUT_LOTID"] = _InputLotID;
                newRow["INPUT_QTY"] = _InputQty;
                newRow["INPUT_QTY_PTN"] = _InputQtyPtn;
                newRow["OUTPUT_QTY"] = _OutputQty;
                newRow["OUTPUT_QTY_PTN"] = _OutputQtyPtn;
                newRow["CUT_FLAG"] = _CutFlag;

                inMtrlTable.Rows.Add(newRow);
                newRow = null;

                DataTable inDefectTable = indataSet.Tables["IN_DEFECT"];
                newRow = inDefectTable.NewRow();

                newRow["INPUT_LOTID"] = _InputLotID;
                newRow["EQPT_DFCT_CODE"] = _EqptDfctCode;
                newRow["DFCT_QTY"] = _DfctQty;
       
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_LOT_ASSY_LS", "IN_EQP,IN_INPUT,IN_DEFECT", "OUT_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        this.DialogResult = MessageBoxResult.OK;
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
                , indataSet);
        }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
