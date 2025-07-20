/*************************************************************************************
 Created Date : 2018.06.07
      Creator : 신광희C
   Decription : 전지 5MEGA-GMES 구축 - 이물 신규등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.07  신광희C : Initial Created.
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
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_034_IMPURITY_REGISTER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_034_IMPURITY_REGISTER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _selectedAreaCode = string.Empty;
        private string _selectedEquipmentSegmentCode = string.Empty;
        private string _selectedEquipmentCode = string.Empty;
        public string CollectSeqNo = string.Empty;

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
        public ELEC001_034_IMPURITY_REGISTER()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            if (dpCollectDt != null)
                dpCollectDt.SelectedDateTime = GetSystemTime();

            

            if (teCollectTime != null)
                //teCollectTime.Value = new TimeSpan(12,0,0);


            cboArea.SelectedValue = _selectedAreaCode;
            cboEquipmentSegment.SelectedValue = _selectedEquipmentSegmentCode;
            cboEquipment.SelectedValue = _selectedEquipmentCode;
        }

        private void InitCombo()
        {
            CommonCombo combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboLineParent);
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                InitCombo();
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

                if (tmps != null)
                {
                    _selectedAreaCode = tmps[0].GetString();
                    _selectedEquipmentSegmentCode = tmps[1].GetString();
                    _selectedEquipmentCode = tmps[2].GetString();
                }

                ApplyPermissions();
                InitializeControls();

                Loaded -= C1Window_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Clicked(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveImpurity()) return;
            SaveImpurity();
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipment();
        }

        private void dpCollectDt_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion

        #region Mehod





        #region [BizCall]

        private void SaveImpurity()
        {

            try
            {
                const string bizRuleName = "BR_BAS_REG_NEW_IMPURITY_CLCT_HIST";
                DoEvents();
                DataTable inTable = new DataTable { TableName = "INDATA" };
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCT_DTTM", typeof(DateTime));
                inTable.Columns.Add("USERID", typeof(string));

                DateTime dtClctTime;
                if (teCollectTime.Value.HasValue)
                {
                    var spn = (TimeSpan)teCollectTime.Value;
                    dtClctTime = new DateTime(dpCollectDt.SelectedDateTime.Year, dpCollectDt.SelectedDateTime.Month, dpCollectDt.SelectedDateTime.Day, spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtClctTime = new DateTime(dpCollectDt.SelectedDateTime.Year, dpCollectDt.SelectedDateTime.Month, dpCollectDt.SelectedDateTime.Day);
                }

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["CLCT_DTTM"] = dtClctTime;
                dr["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                CollectSeqNo = dtResult.Rows[0][0].GetString();
                Util.MessageInfoAutoClosing("SFU1275");    //정상 처리 되었습니다.
                Thread.Sleep(2500);
                DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            /*
            try
            {
                const string bizRuleName = "BR_BAS_REG_NEW_IMPURITY_CLCT_HIST";
                ShowLoadingIndicator();
                DataTable inTable = new DataTable { TableName = "INDATA" };
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCT_DTTM", typeof(DateTime));
                inTable.Columns.Add("USERID", typeof(string));

                DateTime dtClctTime;
                if (teCollectTime.Value.HasValue)
                {
                    var spn = (TimeSpan)teCollectTime.Value;
                    dtClctTime = new DateTime(dpCollectDt.SelectedDateTime.Year, dpCollectDt.SelectedDateTime.Month, dpCollectDt.SelectedDateTime.Day, spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtClctTime = new DateTime(dpCollectDt.SelectedDateTime.Year, dpCollectDt.SelectedDateTime.Month, dpCollectDt.SelectedDateTime.Day);
                }

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["CLCT_DTTM"] = dtClctTime;
                dr["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        CollectSeqNo = result.Rows[0][0].GetString();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Thread.Sleep(2500);

                        DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            */
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();
            const string bizRuleName = "BR_CUS_GET_SYSTIME";

            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void SetEquipment()
        {
            try
            {
                cboEquipment.ItemsSource = null;

                const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                dr["PROCID"] = Process.MIXING;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);

                var query = (from t in dtResult.AsEnumerable()
                    where t.Field<string>("CBO_CODE") == LoginInfo.CFG_EQPT_ID
                    select t).FirstOrDefault();
                if (query != null)
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
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

        #region [Validation]

        private bool ValidationSaveImpurity()
        {
            //IMPURITY
            if (cboEquipmentSegment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) || cboEquipmentSegment.SelectedValue.GetString().Equals("SELECT"))
            {
                //라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if(cboEquipment.SelectedIndex < 0 || string.IsNullOrEmpty(cboEquipment.SelectedValue.GetString()) || cboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> {btnSave};
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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }
        #endregion

        #endregion





    }
}
