/*************************************************************************************
 Created Date : 2023.02.08
      Creator : 김영환
   Decription : OCAP Release 요청 대상 Lot 조회
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.08  DEVELOPER : Initial Created.

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
    /// COM001_374.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_374 : UserControl, IWorkArea
    {
        // Variable
        #region Variable
        CommonCombo combo = new CommonCombo();
        Util _util = new Util();
        #endregion

        public COM001_374()
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
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboProcessParent, sCase: "PROCESS");

            //조치시스템
            String[] sFilter1 = { "REL_SYSTEM_ID" };
            combo.SetCombo(cboRelSys, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            //해제결과 
            String[] sFilter2 = { "PROC_FLAG" };
            combo.SetCombo(cboRelRsltFlag, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");
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
            //if (!ValidationSearch())
            //    return;

            SearchData();
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
                Util.gridClear(dgOcapRelLot);

                DataTable INDATA = new DataTable();
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("FROM_DATE", typeof(string));
                INDATA.Columns.Add("TO_DATE", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("ABNORMID", typeof(string));
                INDATA.Columns.Add("REL_SYSTEM_ID", typeof(string));
                INDATA.Columns.Add("REL_PRCS_FLAG", typeof(string));
                INDATA.Columns.Add("REL_RSLT_FLAG", typeof(string));
                INDATA.Columns.Add("REL_RSLT_TRNF_FLAG", typeof(string));

                DataRow dr = INDATA.NewRow();

                if (Util.NVC(txtAbnormID.Text) != "")
                {
                    dr["ABNORMID"] = Util.NVC(txtAbnormID.Text) != "" ? txtAbnormID.Text : null;

                }
                else if (Util.NVC(txtLOTID.Text) != "")
                {
                    dr["LOTID"] = Util.NVC(txtLOTID.Text) != "" ? txtLOTID.Text : null;
                }
                else
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                    dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    dr["REL_SYSTEM_ID"] = Util.GetCondition(cboRelSys, bAllNull: true);
                    dr["REL_RSLT_FLAG"] = Util.GetCondition(cboRelRsltFlag, bAllNull: true);
                }

                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPR_OCAP_CHK", "RQSTDT", "RSLTDT", INDATA);

                Util.GridSetData(dgOcapRelLot, dtResult, FrameOperation, true);
                
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

        #endregion        
    }
}
