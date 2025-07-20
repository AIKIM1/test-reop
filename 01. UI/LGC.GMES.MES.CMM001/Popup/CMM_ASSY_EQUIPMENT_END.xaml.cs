/*************************************************************************************
 Created Date : 2017.03.04
      Creator : 신광희C
   Decription : 전지 5MEGA-GMES 구축 - 조립 원각 초소형 공정진척 화면 - 장비완료 팝업(CMM_ASSY_EQPEND.xaml 참조)
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.25  신광희C : Initial Created.
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_EQUIPMENT_END.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_EQUIPMENT_END : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _equipmentCode = string.Empty;
        private string _prodLotId = string.Empty;
        private string _processCode = string.Empty;
        private string _startDateTime = string.Empty;
        private string _nowDateTime = string.Empty;
        private bool _isSmallType = false;
        private bool _isRework = false;

        private DateTime _dtStartTime;
        private DateTime _dtNowTime;

        private bool bEndSetTime = false;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private Util _util = new Util();
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
        public CMM_ASSY_EQUIPMENT_END()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            if (!DateTime.TryParse(_startDateTime, out _dtStartTime))
            {
                _dtStartTime = DateTime.Now;
            }

            if (!DateTime.TryParse(_nowDateTime, out _dtNowTime))
            {
                _dtNowTime = DateTime.Now;
            }

            if (ldpDatePicker != null)
                ldpDatePicker.SelectedDateTime = _dtNowTime;
            if (teTimeEditor != null)
                teTimeEditor.Value = new TimeSpan(_dtNowTime.Hour, _dtNowTime.Minute, _dtNowTime.Second);
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

                if (tmps != null && tmps.Length >= 7)
                {
                    _processCode = tmps[0].GetString();
                    _equipmentCode = tmps[1].GetString();
                    _prodLotId = tmps[2].GetString();
                    _startDateTime = Util.NVC(tmps[3]);
                    _nowDateTime = Util.NVC(tmps[4]);
                    _isSmallType = (bool) tmps[5];
                    _isRework = (bool) tmps[6];
                }


                ApplyPermissions();
                InitializeControls();
                bEndSetTime = true;

                txtLotID.Text = _prodLotId;
                txtStartTime.Text = _startDateTime;
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

                    DateTime dtTime = new DateTime();
                    if (teTimeEditor.Value != null)
                    {
                        TimeSpan spn = (TimeSpan)teTimeEditor.Value;
                        dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year
                            , ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day
                            , spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                    }

                    if (!(Math.Truncate(dtTime.Subtract(_dtStartTime).TotalSeconds) >= 0))
                    {
                        //Util.MessageValidation("시작시간보다 이전은 선택할 수 없습니다.");
                        Util.MessageValidation("SFU3089");

                        // 시작시간보다 작으면 초기화.
                        if (ldpDatePicker != null)
                            ldpDatePicker.SelectedDateTime = (DateTime) _dtNowTime;
                        if (teTimeEditor != null)
                            teTimeEditor.Value = new TimeSpan(_dtNowTime.Hour, _dtNowTime.Minute, _dtNowTime.Second);
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
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

                    if (teTimeEditor.Value != null)
                    {
                        TimeSpan spn = ((TimeSpan)teTimeEditor.Value);
                        var dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                            spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);

                        if (!(Math.Truncate(dtTime.Subtract(_dtStartTime).TotalSeconds) >= 0))
                        {
                            //Util.MessageValidation("시작시간보다 이전은 선택할 수 없습니다.");
                            Util.MessageValidation("SFU3089");
                            // 시작시간보다 작으면 초기화.
                            if (ldpDatePicker != null)
                                ldpDatePicker.SelectedDateTime = (DateTime) _dtNowTime;
                            if (teTimeEditor != null)
                                teTimeEditor.Value = new TimeSpan(_dtNowTime.Hour, _dtNowTime.Minute, _dtNowTime.Second);
                        }
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

                if (_processCode == Process.WINDING || _processCode == Process.WINDING_POUCH)
                {
                    bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_UI_WN";
                }
                else if (_processCode == Process.ASSEMBLY)
                {
                    if (_isSmallType)
                    {
                        bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_ASS";
                    }
                    else
                    {
                        bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_AS";
                    }
                }
                else if (_processCode == Process.WASHING)
                {
                    if (_isSmallType)
                    {
                        bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_WSS";
                    }
                    else
                    {
                        bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_WS";
                    }
                }
                else
                {
                    bizRuleName = "BR_PRD_REG_EQPT_END_PROD_LOT_UI";
                }

                ShowLoadingIndicator();

                DateTime dtTime;
                if (teTimeEditor.Value.HasValue)
                {
                    var spn = (TimeSpan)teTimeEditor.Value;
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day);
                }

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_EQPEND_CMM();
                DataRow dr = inTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _equipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = _prodLotId;
                dr["END_DTTM"] = dtTime;
                inTable.Rows.Add(dr);
                
                new ClientProxy().ExecuteService(bizRuleName, "IN_EQP", null, inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();
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
                });
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
            List<Button> listAuth = new List<Button> {btnEqpend};
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
