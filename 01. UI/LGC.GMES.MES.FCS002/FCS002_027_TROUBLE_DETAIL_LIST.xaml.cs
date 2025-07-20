/*************************************************************************************
 Created Date : 2023.01.25
      Creator : 강동희
   Decription : Trouble 상세 List
--------------------------------------------------------------------------------------
 [Change History]
   개발일자           CSR ID                   처리자          내용 
  2023.01.25                                 DEVELOPER    Initial Created.
  2025.05.09       MES 소형 Rebuilding Prjt  이준영        콤보박스 Biz Rule 변경   
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_027_TROUBLE_DETAIL_LIST : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();

        private string _sLANE_ID = string.Empty;
        private string _sS70 = string.Empty;
        private string _sTROUBLE_CD = string.Empty;
        private string _sFROM_DATE = string.Empty;
        private string _sTO_DATE = string.Empty;
        private string _sEQPTID = string.Empty;

        private string saveLANE_ID = string.Empty;
        private string saveS70 = string.Empty;
        private string saveFROM_DATE = string.Empty;
        private string saveTO_DATE = string.Empty;
        private string saveEQPTID = string.Empty;

        public FCS002_027_TROUBLE_DETAIL_LIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLaneChild = { cboEqpKind };
            string[] sFilter = { "Y" };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE", cbChild: cboLaneChild, sFilter: sFilter);

            C1ComboBox[] cboEqpKindParent = { cboLane };
            C1ComboBox[] cboEqpKindChild = { cboEqp };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.ALL, cbChild: cboEqpKindChild, cbParent: cboEqpKindParent, sCase: "EQPT_GR_TYPE_CODE");

            C1ComboBox[] cboEqpParent = { cboLane, cboEqpKind };
            string[] sFilter1 = { null, "M" };
            _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANEMB", cbParent: cboEqpParent, sFilter: sFilter1);

            GetTroubleCombo();
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now;
            dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();

            InitCombo();

            object[] tmps = this.FrameOperation.Parameters;

            if (tmps != null && tmps.Length >= 1)
            {
                _sLANE_ID = Util.NVC(tmps[0]);
                _sS70 = Util.NVC(tmps[1]);
                _sTROUBLE_CD = Util.NVC(tmps[2]);
                _sEQPTID = Util.NVC(tmps[7]);


                dtpFromDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(tmps[3]));
                dtpFromTime.DateTime = Util.StringToDateTime(dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + " " + Util.NVC(tmps[4]), "yyyyMMdd HHmmss");
                dtpToDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(tmps[5]));
                dtpToTime.DateTime = Util.StringToDateTime(dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + " " + Util.NVC(tmps[6]), "yyyyMMdd HHmmss");


                cboLane.SelectedValue = _sLANE_ID;
                cboEqpKind.SelectedValue = _sS70;
                cboEqp.SelectedValue = _sEQPTID;
                cboTrouble.SelectedValue = _sTROUBLE_CD;

                GetList(); // 알람분석에서 오픈시에만 자동 조회,
            }
            


            this.Loaded -= UserControl_Loaded;
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dtpDate_DataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTroubleCombo();
        }

        private void dtpDate_DataTimeChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<DateTime> e)
        {
            GetTroubleCombo();
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            GetTroubleCombo();
        }

        private void cboEqpKind_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            GetTroubleCombo();
        }

        private void dgTroubleDetailList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANE_ID", typeof(string));
                inDataTable.Columns.Add("EQP_KIND_CD", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("TROUBLE_CD", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANE_ID"] = cboLane.GetBindValue();
                newRow["EQP_KIND_CD"] = cboEqpKind.GetBindValue();
                newRow["EQPTID"] = cboEqp.GetBindValue();
                newRow["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                newRow["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:59");
                newRow["TROUBLE_CD"] = cboTrouble.GetBindValue();
                newRow["LANGID"] = LoginInfo.LANGID;

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_TROUBLE_LIST_MB", "INDATA", "OUTDATA", inDataTable);
                //dgTroubleDetailList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgTroubleDetailList, dtRslt, this.FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTroubleCombo()
        {
            if (dtpFromDate.SelectedDateTime == null || dtpFromTime.DateTime == null) return;
            if (dtpToDate.SelectedDateTime == null || dtpToTime.DateTime == null) return;

            string fromDate = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
            string toDate = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");

            // 다르면 실행
            if (saveLANE_ID == cboLane.GetStringValue() &&
                saveS70 == cboEqpKind.GetStringValue() &&
                saveEQPTID == cboEqp.GetStringValue() &&
                saveFROM_DATE == fromDate &&
                saveTO_DATE == toDate) return;

            string bizRuleName = "DA_BAS_SEL_COMBO_TROUBLE";
            string[] arrColumn = { "LANGID", "LANE_ID", "S70",  "FROM_DATE", "TO_DATE", "EQPTID" };            
            string[] arrCondition = { LoginInfo.LANGID,
                                      cboLane.GetStringValue() == "" ? null : cboLane.GetStringValue(),
                                      cboEqpKind.GetStringValue() == "" ? null : cboEqpKind.GetStringValue(),
                                      fromDate,
                                      toDate,
                                      cboEqp.GetStringValue() == "" ? null : cboEqp.GetStringValue()
                                    };

            cboTrouble.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, true);

            saveLANE_ID = cboLane.GetStringValue();
            saveS70 = cboEqpKind.GetStringValue();
            saveEQPTID = cboEqp.GetStringValue();
            saveFROM_DATE = fromDate;
            saveTO_DATE = toDate;
        }

        #endregion

    }
}
