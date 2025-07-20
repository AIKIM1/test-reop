/*************************************************************************************
 Created Date : 2021.04.20
      Creator : 
   Decription : 포일 사용량 조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.20  DEVELOPER : Initial Created.
 
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
    /// COM001_359.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_359 : UserControl, IWorkArea
    {
        // Variable
        #region Variable
        CommonCombo combo = new CommonCombo();
        Util _util = new Util();
        #endregion

        public COM001_359()
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

            dtpDateFrom.SelectedDateTime = System.DateTime.Now.AddDays(-31);
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
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);

            //설비
            SetCombo_Equipment(cboEquipment);
            cboEquipment.SelectedIndex = 0;

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

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCombo_Equipment(cboEquipment);
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Method
        #region SearchData : Data Search
        /// <summary>
        /// Data Search
        /// </summary>
        private void SearchData()
        {
            try
            {
                //초기화
                Util.gridClear(dgResult);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));
                RQSTDT.Columns.Add("FOILID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                string t = Util.GetCondition(cboEquipment);

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["AREAID"] = Util.GetCondition(cboArea);  
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString() == "" ? null : cboEquipmentSegment.SelectedValue.ToString();  
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString() == "" ? null : cboEquipment.SelectedValue.ToString();
                dr["MTRLID"] = (txtMaterialID.Text.Trim() == "" ? null : txtMaterialID.Text.Trim());
                dr["FOILID"] = (txtFoilID.Text.Trim() == "" ? null : txtFoilID.Text.Trim());
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_ELEC_TB_SFC_EQPT_DATA_CLCT", "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        string[] sColumnName = null;

                        Util.GridSetData(dgResult, searchResult, FrameOperation, true);
                        sColumnName = new string[] { "EQPTID", "EQPTNAME" };
                        _util.SetDataGridMergeExtensionCol(dgResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI); //VERTICALHIERARCHI);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private bool ValidationSearch(bool isLot = false)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (!isLot)
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 동을선택하세요
                    Util.MessageValidation("SFU1499");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// CommonCombo 추가시 최신파일로 컴파일이 오류로 소스에서 처리
        /// </summary>
        /// <param name="cbo"></param>
        private void SetCombo_Equipment(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = "E2000";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
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
