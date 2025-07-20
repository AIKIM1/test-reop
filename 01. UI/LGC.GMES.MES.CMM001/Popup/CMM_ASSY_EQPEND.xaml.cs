/*************************************************************************************
 Created Date : 2017.03.04
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 장비완료 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.04  INS 김동일K : Initial Created.
  2017.07.27  INS 신광희C : Winding 공정진척 인 경우 장비완료 시 변경BizRule(BR_PRD_REG_EQPT_END_PROD_LOT_UI_WN)  적용
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
    /// CMM_ASSY_EQPEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_EQPEND : C1Window, IWorkArea
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
        public CMM_ASSY_EQPEND()
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

                if (tmps != null && tmps.Length >= 8)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                    _LotID = Util.NVC(tmps[3]);
                    _WipSeq = Util.NVC(tmps[4]);
                    _StackingYN = Util.NVC(tmps[5]);
                    _StartDttm = Util.NVC(tmps[6]);
                    _NOWDttm = Util.NVC(tmps[7]);
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
            RunComplete_Common();            
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

        #region Mehod

        #region [BizCall]
        private void RunComplete_Common()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (string.Equals(_ProcID, Process.WINDING))
                {
                    bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_UI_WN";
                }
                else
                {
                    bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_UI";
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

                DataTable inTable = _Biz.GetBR_PRD_REG_EQPEND_CMM();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["END_DTTM"] = dtTime;
                
                //newRow["OUTPUT_QTY"] = 0;

                inTable.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService(bizRuleName, "IN_EQP", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void RunComplete_LM()
        {
            try
            {
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

                DataSet indataSet = _Biz.GetBR_PRD_REG_EQPEND_LM();
                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["END_DTTM"] = dtTime;

                //newRow["INPUT_QTY"] = 0;
                //newRow["OUTPUT_QTY"] = 0;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                //newRow = inMtrlTable.NewRow();
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";
                //newRow["INPUT_LOTID"] = "";

                //inMtrlTable.Rows.Add(newRow);
                //newRow = null;

                //DataTable inDfctTable = indataSet.Tables["IN_DEFECT"];
                //newRow = inDfctTable.NewRow();
                //newRow["EQPT_DFCT_CODE"] = "";
                //newRow["DFCT_QTY"] = 0;

                //inDfctTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_PROD_LOT_LM", "IN_EQP,IN_INPUT,IN_DEFECT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private void RunComplete_FD()
        {
            try
            {
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

                DataSet indataSet = _Biz.GetBR_PRD_REG_EQPEND_FD();
                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["END_DTTM"] = dtTime;

                //newRow["INPUT_QTY"] = 0;
                //newRow["OUTPUT_QTY"] = 0;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                //newRow = inMtrlTable.NewRow();
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";
                //newRow["INPUT_LOTID"] = "";

                //inMtrlTable.Rows.Add(newRow);
                //newRow = null;

                //DataTable inDfctTable = indataSet.Tables["IN_DEFECT"];
                //newRow = inDfctTable.NewRow();
                //newRow["EQPT_DFCT_CODE"] = "";
                //newRow["DFCT_QTY"] = 0;

                //inDfctTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_PROD_LOT_FD", "IN_EQP,IN_INPUT,IN_DEFECT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void RunComplete_CL()
        {
            try
            {
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

                DataSet indataSet = _Biz.GetBR_PRD_REG_EQPEND_CL();
                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["END_DTTM"] = dtTime;

                //newRow["INPUT_QTY"] = 0;
                //newRow["OUTPUT_QTY"] = 0;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                //newRow = inMtrlTable.NewRow();
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";
                //newRow["INPUT_LOTID"] = "";

                //inMtrlTable.Rows.Add(newRow);
                //newRow = null;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_PROD_LOT_CL", "IN_EQP,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]

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

        #endregion

        #endregion
        
    }
}
