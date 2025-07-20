/*************************************************************************************
 Created Date : 2023.09.13
      Creator : 김대현
   Decription : 설비 Loss 사전등록
--------------------------------------------------------------------------------------
 [Change History]
  2023.09.08 김대현 CSR ID E20230811-001215 설비 Loss 사전등록 
  2023.10.31 김대현 UserControl_Loaded() 에서 DA_PRD_SEL_BAS_TIME_BY_AREA로 파라미터 전송시 JOBDATE가 '0001-01-01'로 가는 현상 수정
  2023.11.07 김대현 조회조건 파라미터에서 공정 제거
  2023.12.27 김대현 사전등록자만 수정/삭제 가능하도록 Validation 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_014_Tab_Reserve.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_014_Tab_Reserve : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        DataTable AreaTime;
        DataTable dtEqptInfo;

        string _grid_eqpt = string.Empty;
        string _grid_area = string.Empty;
        string _grid_proc = string.Empty;
        string _grid_eqsg = string.Empty;
        string _grid_shit = string.Empty;
        string _mode = string.Empty;
        string _insuser = string.Empty; //2023.12.27 추가

        string sSearchDayFrom = string.Empty;
        string sSearchDayTo = string.Empty;
        string sAreaTime = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre1 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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
        #endregion

        #region Initialize
        public COM001_014_Tab_Reserve()
        {
            InitializeComponent();
            InitCombo();
        }

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅(조회 조건)
            SetAreaCombo();
            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;

            SetEquipmentSegmentCombo(Util.GetCondition(cboArea));
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;

            SetProcessCombo(Util.GetCondition(cboEquipmentSegment));
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            
            SetEquipmentCombo();
        }

        private void initInsertCombo()
        {
            //동,라인,공정,설비 셋팅(등록/수정)
            SetUpdAreaCombo();
            cboUpdArea.SelectedValueChanged += cboUpdArea_SelectedValueChanged;

            SetUpdEquipmentSegmentCombo(Util.GetCondition(cboUpdArea));
            cboUpdEquipmentSegment.SelectedValueChanged += cboUpdEquipmentSegment_SelectedValueChanged;

            SetUpdProcessCombo(Util.GetCondition(cboEquipmentSegment));
            cboUpdProcess.SelectedValueChanged += cboUpdProcess_SelectedValueChanged;

            SetUpdEquipmentCombo();

            SetLossCombo(cboLoss);
            SetLastLossCombo(cboLastLoss);
        }

        private void initControls()
        {
            cboUpdArea.SelectedIndex = 0;
            cboUpdEquipmentSegment.SelectedIndex = 0;
            cboUpdProcess.SelectedIndex = 0;
            cboUpdEquipment.SelectedIndex = 0;
            cboUpdEquipment_Multi.UncheckAll();

            cboLoss.SelectedIndex = 0;

            popLossDetl.SelectedValue = string.Empty;
            popLossDetl.SelectedText = string.Empty;

            rtbLossNote.Document.Blocks.Clear();

            txtHiddenSeqno.Text = string.Empty;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReg);
            listAuth.Add(btnDelete);
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(1);
            dtpDateTo.SelectedDateTime = DateTime.Now.AddDays(7);

            dtpUpdDateFrom.SelectedDateTime = DateTime.Now.AddDays(1);
            dtpUpdDateTo.SelectedDateTime = DateTime.Now.AddDays(7);

            sSearchDayFrom = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            sSearchDayTo = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("JOBDATE", typeof(string));

            DataRow row = dt.NewRow();

            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
            row["JOBDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") == "0001-01-01" ? DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") : dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
            dt.Rows.Add(row);

            AreaTime = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAS_TIME_BY_AREA", "RQSTDT", "RSLTDT", dt);
            if (AreaTime.Rows.Count > 0)
            {
                sAreaTime = Util.NVC(AreaTime.Rows[0]["HHMMSS"]);

                if (Convert.ToString(AreaTime.Rows[0]["HHMMSS"]).Equals(""))
                {
                    Util.MessageValidation("SFU3432"); //동별 작업시작 기준정보를 입력해주세요
                    return;
                }
                TimeSpan tmp = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss")).TimeOfDay;

                if (tmp < DateTime.Parse(sAreaTime.Substring(0, 2) + ":" + sAreaTime.Substring(2, 2) + ":" + sAreaTime.Substring(4, 2)).TimeOfDay)
                {
                    dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-1);
                    dtpDateTo.SelectedDateTime = DateTime.Now.AddDays(6);

                    dtpUpdDateFrom.SelectedDateTime = DateTime.Now.AddDays(-1);
                    dtpUpdDateTo.SelectedDateTime = DateTime.Now.AddDays(6);
                }

                dtpUpdFromTime.DateTime = DateTime.Parse(sAreaTime.Substring(0, 2) + ":" + sAreaTime.Substring(2, 2) + ":" + sAreaTime.Substring(4, 2));
                dtpUpdToTime.DateTime = DateTime.Parse(sAreaTime.Substring(0, 2) + ":" + sAreaTime.Substring(2, 2) + ":" + sAreaTime.Substring(4, 2));
            }

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }
        #endregion

        #region SetComboBox
        private void SetAreaCombo()
        {
            cboArea.ItemsSource = null;
            cboArea.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cboArea.DisplayMemberPath = "CBO_NAME";
            cboArea.SelectedValuePath = "CBO_CODE";

            cboArea.ItemsSource = dtResult.Copy().AsDataView();

            if (string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID))
            {
                cboArea.SelectedIndex = 0;
            }
            else
            {
                cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
                if (cboArea.SelectedIndex < 0)
                {
                    cboArea.SelectedIndex = 0;
                }
            }
        }

        private void SetUpdAreaCombo()
        {
            cboUpdArea.ItemsSource = null;
            cboUpdArea.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataRow drResult = dtResult.NewRow();
            drResult["CBO_CODE"] = "-SELECT-";
            drResult["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(drResult, 0);

            cboUpdArea.DisplayMemberPath = "CBO_NAME";
            cboUpdArea.SelectedValuePath = "CBO_CODE";

            cboUpdArea.ItemsSource = dtResult.Copy().AsDataView();

            cboUpdArea.SelectedIndex = 0;
        }

        private void SetEquipmentSegmentCombo(string AreaID)
        {
            cboEquipmentSegment.ItemsSource = null;
            cboEquipmentSegment.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";

                inTable.Columns.Add("INCLUDE_GROUP", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = AreaID;
                dr["INCLUDE_GROUP"] = "AC";

                inTable.Rows.Add(dr);
            }
            else
            {
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = AreaID;

                inTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
            cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

            cboEquipmentSegment.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQSG_ID))
            {
                cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
                if (cboEquipmentSegment.SelectedIndex < 0)
                {
                    cboEquipmentSegment.SelectedIndex = 0;
                }
            }
            else
            {
                cboEquipmentSegment.SelectedIndex = 0;
            }
        }

        private void SetUpdEquipmentSegmentCombo(string AreaID)
        {
            cboUpdEquipmentSegment.ItemsSource = null;
            cboUpdEquipmentSegment.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                bizRuleName = "BR_GET_EQUIPMENTSEGMENT_FORM_LOSS_CBO";

                inTable.Columns.Add("INCLUDE_GROUP", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = AreaID;
                dr["INCLUDE_GROUP"] = "AC";

                inTable.Rows.Add(dr);
            }
            else
            {
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = AreaID;

                inTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataRow drResult = dtResult.NewRow();
            drResult["CBO_CODE"] = "-SELECT-";
            drResult["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(drResult, 0);

            cboUpdEquipmentSegment.DisplayMemberPath = "CBO_NAME";
            cboUpdEquipmentSegment.SelectedValuePath = "CBO_CODE";

            cboUpdEquipmentSegment.ItemsSource = dtResult.Copy().AsDataView();

            cboUpdEquipmentSegment.SelectedIndex = 0;
        }

        private void SetProcessCombo(string EQSGID)
        {
            cboProcess.ItemsSource = null;
            cboProcess.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
            {
                bizRuleName = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";
            }

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = EQSGID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cboProcess.DisplayMemberPath = "CBO_NAME";
            cboProcess.SelectedValuePath = "CBO_CODE";

            cboProcess.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
            {
                cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;
                if (cboProcess.SelectedIndex < 0)
                {
                    cboProcess.SelectedIndex = 0;
                }
            }
            else
            {
                cboProcess.SelectedIndex = 0;
            }
        }

        private void SetUpdProcessCombo(string EQSGID)
        {
            cboUpdProcess.ItemsSource = null;
            cboUpdProcess.Items.Clear();

            string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
            {
                bizRuleName = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";
            }

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = EQSGID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataRow drResult = dtResult.NewRow();
            drResult["CBO_CODE"] = "-SELECT-";
            drResult["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(drResult, 0);

            cboUpdProcess.DisplayMemberPath = "CBO_NAME";
            cboUpdProcess.SelectedValuePath = "CBO_CODE";

            cboUpdProcess.ItemsSource = dtResult.Copy().AsDataView();

            cboUpdProcess.SelectedIndex = 0;
        }

        private void SetEquipmentCombo()
        {
            cboEquipment.ItemsSource = null;
            cboEquipment.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["PROCID"] = cboProcess.SelectedValue;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cboEquipment.DisplayMemberPath = "CBO_NAME";
            cboEquipment.SelectedValuePath = "CBO_CODE";

            cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
            {
                cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                if (cboEquipment.SelectedIndex < 0)
                    cboEquipment.SelectedIndex = 0;
            }
            else
            {
                cboEquipment.SelectedIndex = 0;
            }

            try
            {
                cboEquipment_Multi.ItemsSource = null;

                if (string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.ToString())) return;
                if (string.IsNullOrEmpty(cboProcess.SelectedValue.ToString())) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                dr["PROCID"] = cboProcess.SelectedValue;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_MAIN_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                dtEqptInfo = dtResult.Copy();
                cboEquipment_Multi.DisplayMemberPath = "CBO_NAME";
                cboEquipment_Multi.SelectedValuePath = "CBO_CODE";

                cboEquipment_Multi.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {

            }
        }

        private void SetUpdEquipmentCombo()
        {
            cboUpdEquipment.ItemsSource = null;
            cboUpdEquipment.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = Util.GetCondition(cboUpdEquipmentSegment);
            dr["PROCID"] = Util.GetCondition(cboUpdProcess);
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            DataRow drResult = dtResult.NewRow();
            drResult["CBO_CODE"] = "-SELECT-";
            drResult["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(drResult, 0);

            cboUpdEquipment.DisplayMemberPath = "CBO_NAME";
            cboUpdEquipment.SelectedValuePath = "CBO_CODE";

            cboUpdEquipment.ItemsSource = dtResult.Copy().AsDataView();

            cboUpdEquipment.SelectedIndex = 0;

            try
            {
                cboUpdEquipment_Multi.ItemsSource = null;

                if (string.IsNullOrEmpty(cboUpdEquipmentSegment.SelectedValue.ToString())) return;
                if (string.IsNullOrEmpty(cboUpdProcess.SelectedValue.ToString())) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboUpdEquipmentSegment);
                dr["PROCID"] = Util.GetCondition(cboProcess);
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_MAIN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboUpdEquipment_Multi.DisplayMemberPath = "CBO_NAME";
                cboUpdEquipment_Multi.SelectedValuePath = "CBO_CODE";

                cboUpdEquipment_Multi.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {

            }
        }

        private void SetLossCombo(C1ComboBox cbo)
        {
            string bizRuleName = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("UPPR_LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea.SelectedValue.GetString();
            dr["PROCID"] = cboProcess.SelectedValue.GetString();
            dr["EQPTID"] = cboEquipment.SelectedValue.GetString();
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue.GetString();
            dr["UPPR_LOSS_CODE"] = "10000";
            dr["USERID"] = LoginInfo.USERID;
            RQSTDT.Rows.Add(dr);

            bizRuleName = "DA_BAS_SEL_EQPTLOSSCODE_CBO_ALL";

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

            DataRow drResult = dtResult.NewRow();
            drResult["CBO_CODE"] = "-SELECT-";
            drResult["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(drResult, 0);

            cbo.SelectedValuePath = "CBO_CODE";
            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.ItemsSource = dtResult.Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private void SetLastLossCombo(C1ComboBox cbo)
        {
            string bizRuleName = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = Util.GetCondition(cboEquipment);
            RQSTDT.Rows.Add(dr);

            bizRuleName = "DA_BAS_SEL_LAST_LOSS_CBO_RSV";

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

            DataRow drResult = dtResult.NewRow();
            drResult["CBO_CODE"] = "-SELECT-";
            drResult["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(drResult, 0);

            cbo.SelectedValuePath = "CBO_CODE";
            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.ItemsSource = dtResult.Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }
        #endregion

        #region Method
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private string SelectEquipment()
        {
            string sEqptID = string.Empty;

            for (int i = 0; i < cboEquipment_Multi.SelectedItems.Count; i++)
            {
                if (i < cboEquipment_Multi.SelectedItems.Count - 1)
                {
                    sEqptID += Convert.ToString(cboEquipment_Multi.SelectedItems[i]) + ",";
                }
                else
                {
                    sEqptID += Convert.ToString(cboEquipment_Multi.SelectedItems[i]);
                }
            }

            return sEqptID;
        }

        private string SelectUpdEquipment()
        {
            string sEqptID = string.Empty;

            for (int i = 0; i < cboUpdEquipment_Multi.SelectedItems.Count; i++)
            {
                if (i < cboUpdEquipment_Multi.SelectedItems.Count - 1)
                {
                    sEqptID += Convert.ToString(cboUpdEquipment_Multi.SelectedItems[i]) + ",";
                }
                else
                {
                    sEqptID += Convert.ToString(cboUpdEquipment_Multi.SelectedItems[i]);
                }
            }

            return sEqptID;
        }

        private void SetComboEnabled(bool chk)
        {
            cboUpdArea.IsEnabled = chk;
            cboUpdEquipmentSegment.IsEnabled = chk;
            cboUpdProcess.IsEnabled = chk;
            cboUpdEquipment_Multi.IsEnabled = chk;

            cboLoss.IsEnabled = chk;
            popLossDetl.IsEnabled = chk;
            btnSearchLossDetlCode.IsEnabled = chk;
            cboLastLoss.IsEnabled = chk;

            dtpUpdDateFrom.IsEnabled = chk;
            dtpUpdFromTime.IsEnabled = chk;
            dtpUpdDateTo.IsEnabled = chk;
            dtpUpdToTime.IsEnabled = chk;
        }
        #endregion

        #region Data
        private string GetNowDate()
        {
            string nowDate = string.Empty;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = Util.GetCondition(cboArea);
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", dtRqst);

            nowDate = dtRslt.Rows[0]["CALDATE_YMD"].ToString();

            return nowDate;
        }

        private DataTable GetEquipment(string EqptId)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = EqptId;
            inTable.Rows.Add(newRow);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_STDINFO", "RQSTDT", "RSLTDT", inTable);

            return dtRslt;
        }

        private void SelectProcess()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "INDATA";
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = Util.GetCondition(cboArea);
            dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
            dr["EQPTID"] = SelectEquipment();
            dr["STRT_DTTM"] = sSearchDayFrom;
            dr["END_DTTM"] = sSearchDayTo;
            dr["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(dr);

            try
            {
                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_RSV", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgDetail, RSLTDT, FrameOperation, true);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InsertProcess()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "INDATA";
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("STRT_FULL", typeof(string));
            RQSTDT.Columns.Add("END_FULL", typeof(string));
            RQSTDT.Columns.Add("EQPTID_ALL", typeof(string));

            string[] sEqptId = SelectUpdEquipment().Split(',');

            string strtDttm = dtpUpdDateFrom.SelectedDateTime.ToString("yyyyMMdd") + dtpUpdFromTime.DateTime.Value.ToString("HHmmss");
            string endDttm = dtpUpdDateTo.SelectedDateTime.ToString("yyyyMMdd") + dtpUpdToTime.DateTime.Value.ToString("HHmmss");
            int dateDiff = Convert.ToInt32((DateTime.ParseExact(dtpUpdDateTo.SelectedDateTime.ToString("yyyyMMdd"), "yyyyMMdd", null) - DateTime.ParseExact(dtpUpdDateFrom.SelectedDateTime.ToString("yyyyMMdd"), "yyyyMMdd", null)).TotalDays);

            TimeSpan strtTime = DateTime.ParseExact(strtDttm.Substring(8, 6), "HHmmss", null).TimeOfDay;
            TimeSpan endTime = DateTime.ParseExact(endDttm.Substring(8, 6), "HHmmss", null).TimeOfDay;

            //종료시간이 기준시간보다 크면 loop +1번 수행
            if (endTime > DateTime.ParseExact(sAreaTime, "HHmmss", null).TimeOfDay)
            {
                dateDiff += 1;
            }

            if (strtTime >= DateTime.ParseExact("000000", "HHmmss", null).TimeOfDay && strtTime < DateTime.ParseExact(sAreaTime, "HHmmss", null).TimeOfDay)
            {
                dateDiff += 1;
            }

            //설비 기준
            for (int eqpIdx = 0; eqpIdx < sEqptId.Length; eqpIdx++)
            {
                //일자 기준
                if (dateDiff <= 1)
                {
                    string strtDate = strtDttm;
                    string endDate = endDttm;
                    string wrkDate = dtpUpdDateFrom.SelectedDateTime.ToString("yyyyMMdd");

                    if (!sAreaTime.Equals("000000")
                        && DateTime.ParseExact(strtDate.Substring(8, 6), "HHmmss", null).TimeOfDay < DateTime.ParseExact(sAreaTime, "HHmmss", null).TimeOfDay
                        && DateTime.ParseExact(strtDate.Substring(8, 6), "HHmmss", null).TimeOfDay >= DateTime.ParseExact("000000", "HHmmss", null).TimeOfDay)
                    {
                        wrkDate = DateTime.ParseExact(wrkDate, "yyyyMMdd", null).AddDays(-1).ToString("yyyyMMdd");
                    }

                    DataRow drInData = RQSTDT.NewRow();
                    drInData["AREAID"] = Util.GetCondition(cboUpdArea);
                    drInData["EQPTID"] = sEqptId[eqpIdx];
                    drInData["WRK_DATE"] = wrkDate;
                    drInData["STRT_DTTM"] = strtDate;
                    drInData["END_DTTM"] = endDate;
                    drInData["LOSS_CODE"] = Util.GetCondition(cboLoss);
                    drInData["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                    drInData["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
                    drInData["USERID"] = LoginInfo.USERID;
                    drInData["STRT_FULL"] = strtDttm;
                    drInData["END_FULL"] = endDttm;
                    drInData["EQPTID_ALL"] = SelectUpdEquipment();

                    RQSTDT.Rows.Add(drInData);
                }
                else
                {
                    for(int dayIdx = 0; dayIdx < dateDiff; dayIdx++)
                    {
                        string strtDate = dtpUpdDateFrom.SelectedDateTime.AddDays(dayIdx).ToString("yyyyMMdd");
                        string endDate = dtpUpdDateFrom.SelectedDateTime.AddDays(dayIdx + 1).ToString("yyyyMMdd");
                        string wrkDate = strtDate;

                        if (dayIdx == 0)
                        {
                            //첫번째 Row
                            strtDate = strtDttm;
                        }
                        else
                        {
                            if (RQSTDT.Rows.Count > 0)
                            {
                                strtDate = RQSTDT.Rows[dayIdx - 1]["END_DTTM"].ToString();
                                wrkDate = DateTime.ParseExact(RQSTDT.Rows[dayIdx - 1]["WRK_DATE"].ToString(), "yyyyMMdd", null).AddDays(1).ToString("yyyyMMdd");
                            }
                            else
                            {
                                strtDate = strtDate + sAreaTime;
                            }
                        }

                        if (!sAreaTime.Equals("000000")
                           && DateTime.ParseExact(strtDate.Substring(8, 6), "HHmmss", null).TimeOfDay < DateTime.ParseExact(sAreaTime, "HHmmss", null).TimeOfDay
                           && DateTime.ParseExact(strtDate.Substring(8, 6), "HHmmss", null).TimeOfDay >= DateTime.ParseExact("000000", "HHmmss", null).TimeOfDay)
                        {
                            endDate = wrkDate;
                            wrkDate = DateTime.ParseExact(wrkDate, "yyyyMMdd", null).AddDays(-1).ToString("yyyyMMdd");
                        }

                        if (dayIdx + 1 == dateDiff)
                        {
                            //마지막 Row
                            endDate = endDttm;
                        }
                        else
                        {
                            if (RQSTDT.Rows.Count > 0)
                            {
                                endDate = DateTime.ParseExact(strtDate, "yyyyMMddHHmmss", null).AddDays(1).ToString("yyyyMMddHHmmss");
                            }
                            else
                            {
                                endDate = endDate + sAreaTime;
                            }
                        }

                        DataRow drInData = RQSTDT.NewRow();
                        drInData["AREAID"] = Util.GetCondition(cboUpdArea);
                        drInData["EQPTID"] = sEqptId[eqpIdx];
                        drInData["WRK_DATE"] = wrkDate;
                        drInData["STRT_DTTM"] = strtDate;
                        drInData["END_DTTM"] = endDate;
                        drInData["LOSS_CODE"] = Util.GetCondition(cboLoss);
                        drInData["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                        drInData["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
                        drInData["USERID"] = LoginInfo.USERID;
                        drInData["STRT_FULL"] = strtDttm;
                        drInData["END_FULL"] = endDttm;
                        drInData["EQPTID_ALL"] = SelectUpdEquipment();

                        RQSTDT.Rows.Add(drInData);
                    }
                }
            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_EQPT_EQPTLOSS_INS_RSV", "INDATA", null, RQSTDT);

                btnSearch_Click(null, null);

                Util.AlertInfo("SFU1270");  //저장되었습니다.

                initControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UpdateProcess()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "INDATA";
            RQSTDT.Columns.Add("MODE", typeof(string));
            RQSTDT.Columns.Add("RSV_SEQNO", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("WRK_DATE", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("LOSS_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_DETL_CODE", typeof(string));
            RQSTDT.Columns.Add("LOSS_NOTE", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));
            RQSTDT.Columns.Add("STRT_FULL", typeof(string));
            RQSTDT.Columns.Add("END_FULL", typeof(string));

            string strtDttm = dtpUpdDateFrom.SelectedDateTime.ToString("yyyyMMdd") + dtpUpdFromTime.DateTime.Value.ToString("HHmmss");
            string endDttm = dtpUpdDateTo.SelectedDateTime.ToString("yyyyMMdd") + dtpUpdToTime.DateTime.Value.ToString("HHmmss");
            int dateDiff = Convert.ToInt32((DateTime.ParseExact(dtpUpdDateTo.SelectedDateTime.ToString("yyyyMMdd"), "yyyyMMdd", null) - DateTime.ParseExact(dtpUpdDateFrom.SelectedDateTime.ToString("yyyyMMdd"), "yyyyMMdd", null)).TotalDays);

            TimeSpan strtTime = DateTime.ParseExact(strtDttm.Substring(8, 6), "HHmmss", null).TimeOfDay;
            TimeSpan endTime = DateTime.ParseExact(endDttm.Substring(8, 6), "HHmmss", null).TimeOfDay;

            //종료시간이 기준시간보다 크면 loop +1번 수행
            if (endTime > DateTime.ParseExact(sAreaTime, "HHmmss", null).TimeOfDay)
            {
                dateDiff += 1;
            }

            if (strtTime >= DateTime.ParseExact("000000", "HHmmss", null).TimeOfDay && strtTime < DateTime.ParseExact(sAreaTime, "HHmmss", null).TimeOfDay)
            {
                dateDiff += 1;
            }

            //일자 기준
            if (dateDiff <= 1)
            {
                string strtDate = dtpUpdDateFrom.SelectedDateTime.ToString("yyyyMMdd") + dtpUpdFromTime.DateTime.Value.ToString("HHmmss");
                string endDate = dtpUpdDateTo.SelectedDateTime.ToString("yyyyMMdd") + dtpUpdToTime.DateTime.Value.ToString("HHmmss");
                string wrkDate = dtpUpdDateFrom.SelectedDateTime.ToString("yyyyMMdd");

                if(!sAreaTime.Equals("000000")
                    && DateTime.ParseExact(strtDate.Substring(8, 6), "HHmmss", null).TimeOfDay < DateTime.ParseExact(sAreaTime, "HHmmss", null).TimeOfDay
                    && DateTime.ParseExact(strtDate.Substring(8, 6), "HHmmss", null).TimeOfDay >= DateTime.ParseExact("000000", "HHmmss", null).TimeOfDay)
                {
                    wrkDate = DateTime.ParseExact(wrkDate, "yyyyMMdd", null).AddDays(-1).ToString("yyyyMMdd");
                }

                DataRow drInData = RQSTDT.NewRow();
                drInData["MODE"] = "U";
                drInData["RSV_SEQNO"] = Util.NVC(txtHiddenSeqno.Text);
                drInData["AREAID"] = Util.GetCondition(cboUpdArea);
                drInData["EQPTID"] = Util.GetCondition(cboUpdEquipment);
                drInData["WRK_DATE"] = wrkDate;
                drInData["STRT_DTTM"] = strtDate;
                drInData["END_DTTM"] = endDate;
                drInData["LOSS_CODE"] = Util.GetCondition(cboLoss);
                drInData["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                drInData["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
                drInData["USERID"] = LoginInfo.USERID;
                drInData["STRT_FULL"] = strtDttm;
                drInData["END_FULL"] = endDttm;

                RQSTDT.Rows.Add(drInData);
            }
            else
            {
                for (int dayIdx = 0; dayIdx < dateDiff; dayIdx++)
                {
                    string strtDate = dtpUpdDateFrom.SelectedDateTime.AddDays(dayIdx).ToString("yyyyMMdd");
                    string endDate = dtpUpdDateFrom.SelectedDateTime.AddDays(dayIdx + 1).ToString("yyyyMMdd");
                    string wrkDate = strtDate;

                    if (dayIdx == 0)
                    {
                        //첫번째 Row
                        strtDate = strtDttm;
                    }
                    else
                    {
                        if (RQSTDT.Rows.Count > 0)
                        {
                            strtDate = RQSTDT.Rows[dayIdx - 1]["END_DTTM"].ToString();
                            wrkDate = DateTime.ParseExact(RQSTDT.Rows[dayIdx - 1]["WRK_DATE"].ToString(), "yyyyMMdd", null).AddDays(1).ToString("yyyyMMdd");
                        }
                        else
                        {
                            strtDate = strtDate + sAreaTime;
                        }
                    }

                    if (!sAreaTime.Equals("000000")
                       && DateTime.ParseExact(strtDate.Substring(8, 6), "HHmmss", null).TimeOfDay < DateTime.ParseExact(sAreaTime, "HHmmss", null).TimeOfDay
                       && DateTime.ParseExact(strtDate.Substring(8, 6), "HHmmss", null).TimeOfDay >= DateTime.ParseExact("000000", "HHmmss", null).TimeOfDay)
                    {
                        endDate = wrkDate;
                        wrkDate = DateTime.ParseExact(wrkDate, "yyyyMMdd", null).AddDays(-1).ToString("yyyyMMdd");
                    }

                    if (dayIdx + 1 == dateDiff)
                    {
                        //마지막 Row
                        endDate = endDttm;
                    }
                    else
                    {
                        if (RQSTDT.Rows.Count > 0)
                        {
                            endDate = DateTime.ParseExact(strtDate, "yyyyMMddHHmmss", null).AddDays(1).ToString("yyyyMMddHHmmss");
                        }
                        else
                        {
                            endDate = endDate + sAreaTime;
                        }
                    }

                    DataRow drInData = RQSTDT.NewRow();
                    if (dayIdx == 0)
                    {
                        drInData["MODE"] = "U";
                        drInData["RSV_SEQNO"] = Util.NVC(txtHiddenSeqno.Text);
                    }
                    else
                    {
                        drInData["MODE"] = "I";
                        drInData["RSV_SEQNO"] = null;
                    }
                    drInData["AREAID"] = Util.GetCondition(cboUpdArea);
                    drInData["EQPTID"] = Util.GetCondition(cboUpdEquipment);
                    drInData["WRK_DATE"] = wrkDate;
                    drInData["STRT_DTTM"] = strtDate;
                    drInData["END_DTTM"] = endDate;
                    drInData["LOSS_CODE"] = Util.GetCondition(cboLoss);
                    drInData["LOSS_DETL_CODE"] = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();
                    drInData["LOSS_NOTE"] = new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text;
                    drInData["USERID"] = LoginInfo.USERID;
                    drInData["STRT_FULL"] = strtDttm;
                    drInData["END_FULL"] = endDttm;

                    RQSTDT.Rows.Add(drInData);
                }
            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_EQPT_EQPTLOSS_UPD_RSV", "INDATA", null, RQSTDT);

                btnSearch_Click(null, null);

                Util.AlertInfo("SFU1265");  //수정되었습니다.

                initControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteProcess()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "INDATA";
            RQSTDT.Columns.Add("RSV_SEQNO", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
            RQSTDT.Columns.Add("END_DTTM", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataRow dr = RQSTDT.NewRow();
                    dr["RSV_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "RSV_SEQNO"));
                    dr["AREAID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "AREAID"));
                    dr["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_START"));
                    dr["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "HIDDEN_END"));
                    dr["USERID"] = LoginInfo.USERID;
                    RQSTDT.Rows.Add(dr);
                }
            }

            try
            {
                new ClientProxy().ExecuteServiceSync("BR_EQPT_EQPTLOSS_UPD_RSV_USE_FLAG", "INDATA", null, RQSTDT);

                btnSearch_Click(null, null);

                Util.AlertInfo("SFU1273");  //삭제되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Events

        #region Buttons
        private void btnChgHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                COM001_014_LOSS_RSV_HIST wndHist = new COM001_014_LOSS_RSV_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RSV_SEQNO"));

                    C1WindowExtension.SetParameters(wndHist, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCheck())
            {
                return;
            }

            if (_mode.Equals("I"))
            {
                InsertProcess();
            }
            else if (_mode.Equals("U"))
            {
                UpdateProcess();
            }
            else
            {
                Util.MessageValidation("SFU5186"); //신규등록 버튼을 클릭하거나 수정할 Row를 더블클릭하세요.
                return;
            }
        }

        private void btnReg_Click(object sender, RoutedEventArgs e)
        {
            if (LoginInfo.USERTYPE.Equals("P"))
            {
                Util.MessageValidation("SFU5187"); //공용 PC 사용자는 설비 Loss 사전등록/수정/삭제가 불가합니다.
                return;
            }

            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                //초기화
                initInsertCombo();

                rtbLossNote.Document.Blocks.Clear();

                _grid_area = Util.GetCondition(cboArea);
                _grid_eqsg = Util.GetCondition(cboEquipmentSegment);
                _grid_eqpt = Util.GetCondition(cboEquipment);
                _grid_proc = Util.GetCondition(cboProcess);

                txtHiddenSeqno.Text = string.Empty;

                cboUpdArea.SelectedValue = cboArea.SelectedValue;
                cboUpdEquipmentSegment.SelectedValue = cboEquipmentSegment.SelectedValue;
                cboUpdProcess.SelectedValue = cboProcess.SelectedValue;

                string sEqptId = SelectEquipment();
                string[] Eqptids = sEqptId.Split(',');

                for (int row = 0; row < dtEqptInfo.Rows.Count; row++)
                {
                    for (int idx = 0; idx < Eqptids.Length; idx++)
                    {
                        if (Util.NVC(dtEqptInfo.Rows[row]["CBO_CODE"]).Equals(Eqptids[idx]))
                        {
                            cboUpdEquipment_Multi.Check(row);
                        }
                    }
                }

                cboLoss.SelectedIndex = 0;
                popLossDetl.SelectedValue = string.Empty;
                popLossDetl.SelectedText = string.Empty;
                cboLastLoss.SelectedIndex = 0;

                dtpUpdDateFrom.SelectedDateTime = DateTime.Now.AddDays(1);
                dtpUpdDateTo.SelectedDateTime = DateTime.Now.AddDays(2);

                dtpUpdFromTime.DateTime = DateTime.ParseExact(sAreaTime, "HHmmss", null);
                dtpUpdToTime.DateTime = DateTime.ParseExact(sAreaTime, "HHmmss", null);

                _mode = "I";

                SetComboEnabled(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                _mode = string.Empty;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (new Util().GetDataGridCheckFirstRowIndex(dgDetail, "CHK") == -1)
            {
                Util.MessageValidation("SFU3486"); //선택된 부동내역이 없습니다.
                return;
            }

            if (LoginInfo.USERTYPE.Equals("P"))
            {
                Util.MessageValidation("SFU5187"); //공용 PC 사용자는 설비 Loss 사전등록/수정/삭제가 불가합니다.
                return;
            }

            for (int i = 0; i < dgDetail.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INSUSER")).Equals(LoginInfo.USERID))
                    {
                        Util.MessageValidation("SFU5189");
                        return;
                    }
                }
            }

            Util.MessageConfirm("SFU1230", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteProcess();
                }
            });

            SetComboEnabled(true);
        }

        private void btnSearchLossDetlCode_Click(object sender, RoutedEventArgs e)
        {
            if (!event_valridtion())
            {
                return;
            }
            if (!ValidateFCR())
            {
                return;
            }

            COM001_014_LOSS_DETL_FCR wndLossDetl = new COM001_014_LOSS_DETL_FCR();
            wndLossDetl.FrameOperation = FrameOperation;

            if (wndLossDetl != null)
            {
                #region 
                /* 2022.08.23 C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
                 * 2022.12.20 C20221216-000583 설비 LOSS 등록 부동내역 팝업 시 분류에 속한 LOSS만 조회
                 */
                object[] Parameters = new object[6];
                Parameters[0] = Convert.ToString(_grid_area);
                Parameters[1] = Convert.ToString(_grid_proc);
                Parameters[2] = Convert.ToString(_grid_eqpt);
                Parameters[3] = (cboLoss.SelectedValue.IsNullOrEmpty() || cboLoss.SelectedValue.ToString().Equals("-SELECT-")) ? "" : cboLoss.SelectedValue.ToString();
                Parameters[4] = 'N';
                Parameters[5] = string.Empty;

                #endregion
                C1WindowExtension.SetParameters(wndLossDetl, Parameters);

                wndLossDetl.Closed += new EventHandler(wndLossDetl_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndLossDetl.ShowModal()));
                wndLossDetl.BringToFront();
            }
        }

        public void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _mode = string.Empty;

            try
            {
                ShowLoadingIndicator();
                DoEvents();

                //C20210723 - 000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                if (string.IsNullOrWhiteSpace(sEquipmentSegment) || string.IsNullOrEmpty(sEquipmentSegment))
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                //C20210723-000206 : 생산PI] 와인더 설비 Loss 기타항목 비고내용 입력 필수화
                string sProcess = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProcess) || string.IsNullOrEmpty(sProcess))
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }

                if (cboEquipment_Multi.SelectedItems.Count == 0)
                {
                    Util.MessageValidation("SFU1153"); //설비를 선택하세요
                    return;
                }

                cboEquipment.SelectedValue = cboEquipment_Multi.SelectedItems[0];

                string sEqpt = Util.GetCondition(cboEquipment, "SFU1153"); //설비를 선택하세요
                if (sEqpt.Equals("")) return;

                sSearchDayFrom = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                sSearchDayTo = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                SelectProcess();

                initInsertCombo();

                SetComboEnabled(true);

                initControls();
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

        #region ComboBox
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentSegmentCombo(Util.GetCondition(cboArea));
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcessCombo(Util.GetCondition(cboEquipmentSegment));
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo();
        }

        private void cboUpdArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetUpdEquipmentSegmentCombo(Util.GetCondition(cboUpdArea));
        }

        private void cboUpdEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetUpdProcessCombo(Util.GetCondition(cboUpdEquipmentSegment));
        }

        private void cboUpdProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetUpdEquipmentCombo();
        }

        private void cboLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLoss.Text.Equals("-SELECT-"))
            {
                const string bizRuleName = "DA_BAS_SEL_EQPTLOSSDETLCODE_CBO";
                string[] arrColumn = { "LANGID", "AREAID", "PROCID", "EQPTID", "LOSS_CODE" };
                string IN_AREA = _grid_area.IsNullOrEmpty() ? cboArea.SelectedValue.ToString() : _grid_area;
                string IN_PROC = _grid_proc.IsNullOrEmpty() ? cboProcess.SelectedValue.ToString() : _grid_proc;
                string IN_EQPT = _grid_eqpt.IsNullOrEmpty() ? cboEquipment.SelectedValue.ToString() : _grid_eqpt;
                string IN_LOSSCODE = cboLoss.SelectedValue.IsNullOrEmpty() ? string.Empty : cboLoss.SelectedValue.ToString();
                string[] arrCondition = { LoginInfo.LANGID, IN_AREA, IN_PROC, IN_EQPT, IN_LOSSCODE };
                CommonCombo.SetFindPopupCombo(bizRuleName, popLossDetl, arrColumn, arrCondition, (string)popLossDetl.SelectedValuePath, (string)popLossDetl.DisplayMemberPath);

                popLossDetl.SelectedText = string.Empty;
                popLossDetl.SelectedValue = string.Empty;
            }
            else if (cboLoss.Text.Equals("-SELECT-"))
            {
                popLossDetl.SelectedText = string.Empty;
                popLossDetl.SelectedValue = string.Empty;
            }
            // 2023.02.28 윤지해 CSR ID E20230220-000068	설비 Loss Lv3(부동내용) 기준 FCR 코드(현상/원인/조치) 매칭 변경
            popLossDetl_ValueChanged(null, null);
        }

        private void cboLastLoss_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!cboLastLoss.SelectedValue.Equals("SELECT"))
            {
                string[] sLastLoss = cboLastLoss.SelectedValue.ToString().Split('-');
                string[] sLastText = cboLastLoss.Text.ToString().Split('-');

                cboLoss.SelectedValue = sLastLoss[0];

                if (!sLastLoss[1].Equals(""))
                {
                    popLossDetl.SelectedValue = sLastLoss[1];
                    popLossDetl.SelectedText = string.IsNullOrEmpty(sLastText[1]) ? sLastText[0] : sLastText[1];
                }
            }
        }
        #endregion

        #region DataGrid
        private void dgDetail_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // 비고 칼럼 사이즈
                if (e.Cell.Column.Name.Equals("txtNote"))
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                }
            }));
        }

        private void dgDetail_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre1.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre1;
                            chkAll.Checked -= new RoutedEventHandler(checkAll1_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll1_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll1_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll1_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int idx = dgDetail.CurrentRow == null ? 0 : dgDetail.CurrentRow.Index;

            if (dgDetail.CurrentRow != null)
            {
                string sEqptID = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "EQPTID"));
                string strtDttm = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_START"));
                string endDttm = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "HIDDEN_END"));
                string lossCode = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "LOSS_CODE"));
                string lossDetlCode = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "LOSS_DETL_CODE"));
                string lossDetlName = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "LOSS_DETL_NAME"));
                string lossNote = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "LOSS_NOTE"));
                string sNow = DateTime.Now.ToString("yyyyMMddHHmmss");

                _insuser = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "INSUSER"));  //2023.12.27 추가

                DataTable dtEqpt = GetEquipment(sEqptID);

                initInsertCombo();

                if (dtEqpt != null && dtEqpt.Rows.Count > 0)
                {
                    _mode = "U";
                    cboUpdArea.SelectedValue = Util.NVC(dtEqpt.Rows[0]["AREAID"]);
                    cboUpdEquipmentSegment.SelectedValue = Util.NVC(dtEqpt.Rows[0]["EQSGID"]);
                    cboUpdProcess.SelectedValue = Util.NVC(dtEqpt.Rows[0]["PROCID"]);
                    cboUpdEquipment.SelectedValue = sEqptID;
                    cboUpdEquipment_Multi.Check(sEqptID);

                    dtpUpdDateFrom.SelectedDateTime = DateTime.ParseExact(strtDttm.Substring(0, 8), "yyyyMMdd", null);
                    dtpUpdDateTo.SelectedDateTime = DateTime.ParseExact(endDttm.Substring(0, 8), "yyyyMMdd", null);
                    dtpUpdFromTime.SelectedDateTime = DateTime.ParseExact(strtDttm.Substring(8, 6), "HHmmss", null);
                    dtpUpdToTime.SelectedDateTime = DateTime.ParseExact(endDttm.Substring(8, 6), "HHmmss", null);

                    txtHiddenSeqno.Text = Util.NVC(DataTableConverter.GetValue(dgDetail.CurrentRow.DataItem, "RSV_SEQNO"));
                    cboLoss.SelectedValue = lossCode;
                    popLossDetl.SelectedValue = lossDetlCode;
                    popLossDetl.SelectedText = lossDetlName;
                    rtbLossNote.Document.Blocks.Clear();
                    rtbLossNote.AppendText(lossNote);

                    SetComboEnabled(false);

                    if (DateTime.ParseExact(strtDttm, "yyyyMMddHHmmss", null) > DateTime.ParseExact(sNow, "yyyyMMddHHmmss", null))
                    {
                        dtpUpdDateFrom.IsEnabled = true;
                        dtpUpdFromTime.IsEnabled = true;
                    }

                    if (DateTime.ParseExact(endDttm, "yyyyMMddHHmmss", null) > DateTime.ParseExact(sNow, "yyyyMMddHHmmss", null))
                    {
                        dtpUpdDateTo.IsEnabled = true;
                        dtpUpdToTime.IsEnabled = true;
                    }

                    if (DateTime.ParseExact(strtDttm, "yyyyMMddHHmmss", null) > DateTime.ParseExact(sNow, "yyyyMMddHHmmss", null)
                        && DateTime.ParseExact(endDttm, "yyyyMMddHHmmss", null) > DateTime.ParseExact(sNow, "yyyyMMddHHmmss", null))
                    {
                        cboLoss.IsEnabled = true;
                        popLossDetl.IsEnabled = true;
                        btnSearchLossDetlCode.IsEnabled = true;
                        cboLastLoss.IsEnabled = true;
                    }
                }
            }
        }
        #endregion

        #region Etc
        private void popLossDetl_ValueChanged(object sender, EventArgs e)
        {
            // 부동내용이 바뀌면 현상, 원인, 조치 전체 초기화
            CommonCombo _combo = new CommonCombo();

            string procId = Util.GetCondition(cboProcess);
            string lossCode = Util.GetCondition(cboLoss);
            string lossDetlCode = popLossDetl.SelectedValue.IsNullOrEmpty() ? string.Empty : popLossDetl.SelectedValue.ToString();

            string failCode = string.Empty;
            string causeCode = string.Empty;
            string resolCode = string.Empty;
            string selectedText = string.Empty;
        }

        private void wndLossDetl_Closed(object sender, EventArgs e)
        {
            // 2023.07.18 윤지해 CSR ID E20230703-000158 - COM001_014_LOSS_DETL_FCR로 변경
            COM001_014_LOSS_DETL_FCR window = sender as COM001_014_LOSS_DETL_FCR;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                cboLoss.SelectedValue = window._LOSS_CODE;
                popLossDetl.SelectedValue = window._LOSS_DETL_CODE;
                popLossDetl.SelectedText = window._LOSS_DETL_NAME;

                // 2023.03.07 윤지해 CSR ID E20230220-000068	FCR 초기화
                popLossDetl_ValueChanged(null, null);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            string fromDate = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            string toDate = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
            
            if (!ValidationChkDay(fromDate, toDate))
            {
                dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            string fromDate = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            string toDate = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

            if (!ValidationChkDay(fromDate, toDate))
            {
                dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime;
            }
        }

        private void checkAll1_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgDetail.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void checkAll1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgDetail.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #endregion

        #region Validation
        private bool event_valridtion()
        {
            if (string.IsNullOrEmpty(_grid_eqpt) || _grid_eqpt.Equals(""))
            {
                // 질문1 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            return true;
        }

        private bool ValidateFCR()
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.Equals("") || cboArea.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU3206"); //동을 선택해주세요
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.Equals("") || cboProcess.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU3207"); //공정을 선택해주세요
                return false;
            }
            return true;
        }

        private bool ValidationChkDay(string fromDate, string Todate)
        {
            bool chk = true;

            DateTime dtFromDay = DateTime.ParseExact(fromDate, "yyyyMMdd", null);
            DateTime dtToDay = DateTime.ParseExact(Todate, "yyyyMMdd", null);

            if (dtFromDay > dtToDay)
            {
                chk = false;
            }
            
            return chk;
        }

        private bool ValidationCheck()
        {
            if (LoginInfo.USERTYPE.Equals("P"))
            {
                Util.MessageValidation("SFU5187"); //공용 PC 사용자는 설비 Loss 사전등록/수정/삭제가 불가합니다.
                return false;
            }

            if (_mode.Equals("U") && !_insuser.Equals(LoginInfo.USERID))
            {
                Util.MessageValidation("SFU5188");  //사전등록한 사용자만 수정할 수 있습니다.
                return false;
            }

            //동 Check
            if (cboUpdArea.SelectedIndex < 0 || string.IsNullOrEmpty(cboUpdArea.SelectedValue.ToString()) || cboUpdArea.Text.ToString().Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU3206"); //동을 선택해주세요
                return false;
            }

            //라인 Check
            if (cboUpdEquipmentSegment.SelectedIndex < 0 || string.IsNullOrEmpty(cboUpdEquipmentSegment.SelectedValue.ToString()) || cboUpdEquipmentSegment.Text.ToString().Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return false;
            }

            //공정 Check
            if (cboUpdProcess.SelectedIndex < 0 || string.IsNullOrEmpty(cboUpdProcess.SelectedValue.ToString()) || cboUpdProcess.Text.ToString().Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                return false;
            }

            // 설비 Check
            if (cboUpdEquipment_Multi.SelectedItems.Count == 0)
            {
                Util.MessageValidation("SFU1153"); //설비를 선택하세요
                return false;
            }

            DateTime fromDate = DateTime.ParseExact(dtpUpdDateFrom.SelectedDateTime.ToString("yyyyMMdd") + dtpUpdFromTime.DateTime.Value.ToString("HHmmss"), "yyyyMMddHHmmss", null);
            DateTime toDate = DateTime.ParseExact(dtpUpdDateTo.SelectedDateTime.ToString("yyyyMMdd") + dtpUpdToTime.DateTime.Value.ToString("HHmmss"), "yyyyMMddHHmmss", null);

            if (fromDate >= toDate)
            {
                Util.MessageValidation("SFU1913"); //종료일자가 시작일자보다 빠릅니다.
                return false;
            }

            // Loss Check
            if (cboLoss.SelectedIndex < 0 || string.IsNullOrEmpty(cboLoss.SelectedValue.ToString()) || cboLoss.Text.ToString().Equals("-SELECT-"))
            {
                Util.MessageInfo("SFU3513"); // LOSS는필수항목입니다
                return false;
            }

            // Loss Detail Check
            if (popLossDetl.SelectedValue.IsNullOrEmpty())
            {
                Util.MessageInfo("SFU3631"); // 부동내용을 입력하세요.
                return false;
            }

            if (new TextRange(rtbLossNote.Document.ContentStart, rtbLossNote.Document.ContentEnd).Text.Trim().Length > 1000)
            {
                Util.MessageValidation("SFU5182");  //비고는 최대 1000자 까지 가능합니다.
                rtbLossNote.Focus();
                return false;
            }

            return true;
        }
        #endregion
    }
}
