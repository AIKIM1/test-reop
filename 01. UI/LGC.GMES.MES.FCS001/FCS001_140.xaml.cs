/*************************************************************************************
 Created Date : 2018.08.29
      Creator : 강민준
   Decription : Tact Time 조회
--------------------------------------------------------------------------------------
 [Change History]
 2022.01.21   안유수   C20220119-000391 T/T 모니터링 조회 조건 변경
 2022.08.11   임정구   C20220517-000215 조립만 나오도록 되어 있어서 활성화 추가
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//추가
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_140 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        int iLoadCnt = 0;   //Form load Cnt

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #region Declaration & Constructor 
        public FCS001_140()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            initcombo();

            //조회조건 한달전 설정
            if (iLoadCnt == 0)
            {
                //dtpDateFrom.SelectedDateTime = dtpDateFrom.SelectedDateTime.Date.AddDays(-7);
                iLoadCnt = 1;
                this.CntInit.Text = "300";
            }
        }
        #endregion


        #region[Click,Key Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }

        private void txtLOTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        //텍스트박스 숫자만 입력
        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
            || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
            || e.Key == Key.Back))
            {
                e.Handled = true;
            }
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


        #region [Combo Box Set] initcombo()
        private void initcombo()
        {
            //동
            CommonCombo_Form ComCombo = new CommonCombo_Form();
            C1ComboBox[] cboPlantChild = { cboEquipmentSegment };
            ComCombo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.NONE, sCase: "PLANT", cbChild: cboPlantChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboEquipment };
            ComCombo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE_SHOPID", cbParent: cboLineParent, cbChild: cboLineChild);

            //설비구분
            string[] sFilterEqpType = { "DEG,EOL" };
            C1ComboBox[] cboEqpKindChild = { cboEquipment };
            ComCombo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "EQUIPMENTGROUP", sFilter: sFilterEqpType, cbChild: cboEqpKindChild);

            //설비
            string[] sFilter = { "M,C" };
            C1ComboBox[] cboEqpParent = { cboEquipmentSegment, cboEqpKind };
            ComCombo.SetCombo(cboEquipment, CommonCombo_Form.ComboStatus.NONE, sCase: "EQUIPMENTFORM", sFilter: sFilter, cbParent: cboEqpParent);
        }
        #endregion


        #region [조회버튼 클릭] SearchData() 
        private void SearchData()
        {
            // 기본조회건수 Set
            if (this.CntInit.Text == null || this.CntInit.Text == "")
            {
                this.CntInit.Text = "300";
            }

            if (Convert.ToInt32(this.CntInit.Text) > 100000)
            {
                Util.MessageValidation("SFU5030");   //최대 100000개 까지 가능합니다.
                return;
            }

            if ((cboEquipment.SelectedIndex) == -1)
            {
                Util.MessageValidation("SFU1672");   //설비 정보가 없습니다.
                return;
            }

            ShowLoadingIndicator();

            string sEqptID = cboEquipment.GetStringValue("MAIN_EQPTID");

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("FROM_DTTM", typeof(string));
            inDataTable.Columns.Add("TO_DTTM", typeof(string));
            inDataTable.Columns.Add("ROW_CNT", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("TO_DTTM1", typeof(string));

            DataRow newRow = inDataTable.NewRow();            
            newRow["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            newRow["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
            newRow["ROW_CNT"] = this.CntInit.Text;
            newRow["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipment.GetStringValue("MAIN_EQPTID"));   //설비
            newRow["TO_DTTM1"] = dtpDateTo.SelectedDateTime.AddDays(+1).ToString("yyyyMMdd");

            inDataTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_SEL_TACTTIME_MONITORING_FORM", "INDATA", "RSLTDT", inDataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }                    
                    Util.GridSetData(dgLotInfo, searchResult, FrameOperation, false);
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
        #endregion
        
       
    }
}
