/*************************************************************************************
 Created Date : 2022.05.19
      Creator : 정재홍
   Decription : LOT별 SPC+ Lot Hold 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2022.05.19  DEVELOPER : Initial Created.
  2022.07.14  정재홍    : [C20220517-000233] - SPC+ LOT HOLD / SPC+ LOT HOLD Detail

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_369.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_369 : UserControl, IWorkArea
    {
        // Variable
        #region Variable
        CommonCombo combo = new CommonCombo();
        Util _util = new Util();
        #endregion

        public COM001_369()
        {
            InitializeComponent();
            Initialize();
            this.Loaded += UserControl_Loaded;
        }

        /// <summary>
        /// Frame과 상호 작용 하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            dtpDateFrom.SelectedDateTime = System.DateTime.Now;
            dtpDateTo.SelectedDateTime = System.DateTime.Now;

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            dtpDateFrom2.SelectedDateTime = System.DateTime.Now;
            dtpDateTo2.SelectedDateTime = System.DateTime.Now;

            dtpDateFrom2.SelectedDataTimeChanged += dtpDate2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDate2_SelectedDataTimeChanged;
        }

        #region Initialize
        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void Initialize()
        {
            InitCombo();
        }
        
        /// <summary>
        /// InitCombo 초기화
        /// </summary>
        private void InitCombo()
        {
            
            /// SPC+ Lot Hold History
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent, sCase: "PROCESS");
            cboProcess.SelectedValue = "E2000";
            
            /// SPC+ Lot Hold History Detail
            //동
            C1ComboBox[] cboAreaChild2 = { cboEquipmentSegment2 };
            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild2, sCase: "AREA");
            
            //라인
            C1ComboBox[] cboEquipmentSegmentParent2 = { cboArea2 };
            combo.SetCombo(cboEquipmentSegment2, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParent2, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent2 = { cboEquipmentSegment2 };
            combo.SetCombo(cboProcess2, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent2, sCase: "PROCESS");
            cboProcess2.SelectedValue = "E2000";

            //설비
            //C1ComboBox[] cboEquipmentParent2 = { cboEquipmentSegment2, cboProcess2 };
            //combo.SetCombo(cboEquipment2, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent2);

            //조치시스템
            String[] sFilter1 = { "REL_SYSTEM_ID" };
            combo.SetCombo(cboRelSys, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
        }
        #endregion

        // Event
        /// <summary>
        /// Search Button Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SearchData();
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch2())
                return;

            SearchDataDetail();
        }

        #region [Process 정보 가져오기]
        private void SetProcess(C1ComboBox cb, string sArea, string sEquipmentSegment)
        {
            try
            {
                // 동을 선택하세요.
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cb.DisplayMemberPath = "CBO_NAME";
                cb.SelectedValuePath = "CBO_CODE";

                cb.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cb.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cb.SelectedIndex < 0)
                        cb.SelectedIndex = 0;
                }
                else
                {
                    if (cb.Items.Count > 0)
                        cb.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea2);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess2);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment2.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment2);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess2.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment2.DisplayMemberPath = "CBO_NAME";
                cboEquipment2.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment2.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment2.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment2.SelectedIndex < 0)
                        cboEquipment2.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment2.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Method
        #region Data Search
        /// <summary>
        /// Data Search
        /// </summary>
        private void SearchData()
        {
            try
            {
                //초기화
                Util.gridClear(dgSpcRslt);

                DataTable INDATA = new DataTable();
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("FROM_DATE", typeof(string));
                INDATA.Columns.Add("TO_DATE", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("REL_SYSTEM_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["LOTID"] = Util.NVC(txtLOTID.Text) != "" ? txtLOTID.Text : null;
                dr["PRODID"] = Util.NVC(txtPRODID.Text) != "" ? txtPRODID.Text : null;
                dr["REL_SYSTEM_ID"] = Util.GetCondition(cboRelSys, bAllNull: true);

                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPCPLUS_LOT_HOLD_RSLT_HIST", "RQSTDT", "RSLTDT", INDATA);

                Util.GridSetData(dgSpcRslt, dtResult, FrameOperation, true);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void SearchDataDetail()
        {
            try
            {
                //초기화
                Util.gridClear(dgSPCDetailRslt);

                DataTable INDATA = new DataTable();
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("FROM_DATE", typeof(string));
                INDATA.Columns.Add("TO_DATE", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom2);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo2);
                dr["AREAID"] = Util.GetCondition(cboArea2);
                dr["PROCID"] = Util.GetCondition(cboProcess2, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboEquipment2, bAllNull: true);
                dr["LOTID"] = Util.NVC(txtLOTID2.Text) != "" ? txtLOTID2.Text : null;
                dr["PRODID"] = Util.NVC(txtPRODID2.Text) != "" ? txtPRODID2.Text : null;

                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPCPLUS_LOT_HOLD_RSLT_HIST_DETL", "RQSTDT", "RSLTDT", INDATA);

                Util.GridSetData(dgSPCDetailRslt, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private bool ValidationSearch()
        {
            
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            {
                    // 동을선택하세요
                    Util.MessageValidation("SFU1499");
                    return false;
            }

            return true;
        }

        private bool ValidationSearch2()
        {

            if (cboArea2.SelectedValue == null || cboArea2.SelectedValue.ToString().Equals("SELECT"))
            {
                // 동을선택하세요
                Util.MessageValidation("SFU1499");
                return false;
            }

            return true;
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }

        private void dtpDate2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom2.SelectedDateTime.Year > 1 && dtpDateTo2.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo2.SelectedDateTime - dtpDateFrom2.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom2.SelectedDateTime = dtpDateTo2.SelectedDateTime;
                    return;
                }
            }
        }

        #endregion

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetProcess(cboProcess, cboArea.SelectedValue.ToString(), cboEquipmentSegment.SelectedValue.ToString());
            }
        }

        private void cboEquipmentSegment2_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment2.Items.Count > 0 && cboEquipmentSegment2.SelectedValue != null && !cboEquipmentSegment2.SelectedValue.Equals("SELECT"))
            {
                SetProcess(cboProcess2, cboArea2.SelectedValue.ToString(), cboEquipmentSegment2.SelectedValue.ToString());
            }
        }

        private void cboProcess2_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess2.Items.Count > 0 && cboProcess2.SelectedValue != null && !cboProcess2.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();
            }
        }
    }
}
