/*************************************************************************************
 Created Date : 2017.12.06
      Creator : 
   Decription : 폴리머 - 설비 완료
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_EQPT_END : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _lotID = string.Empty;        // 공정코드
        private string _startDttm = string.Empty;
        private string _nowDttm = string.Empty;

        private bool _load = true;

        DateTime dtStartTime;
        DateTime dtNowTime;
        private bool bEndSetTime = false;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FORM001_EQPT_END()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                SetControl();
                InitializeUserControls();
            }
        }

        private void InitializeUserControls()
        {
            if (DateTime.TryParse(_startDttm, out dtStartTime))
            {
            }
            else
            {
                dtStartTime = System.DateTime.Now;
            }


            if (DateTime.TryParse(_nowDttm, out dtNowTime))
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
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;
            _lotID = tmps[4] as string;
            _startDttm = tmps[5] as string;
            _nowDttm = tmps[6] as string;

            ApplyPermissions();

            bEndSetTime = true;

            txtLotID.Text = _lotID;
            txtStartTime.Text = _startDttm;
        }
        #endregion

        #region [장비완료시간 설정]
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

        #region [장비완료]
        private void btnEqpend_Click(object sender, RoutedEventArgs e)
        {
            EqptEndProcess();
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]
        private void EqptEndProcess()
        {
            try
            {
                string bizRuleName = string.Empty;

                bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_UI_WN";

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

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUTPUT_QTY", typeof(int));
                inTable.Columns.Add("END_DTTM", typeof(DateTime));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _eqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _lotID;
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

        #endregion

        #region[[Validation]
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
