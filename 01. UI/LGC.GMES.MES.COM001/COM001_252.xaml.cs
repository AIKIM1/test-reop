/*************************************************************************************
 Created Date : 2018.08.29
      Creator : 강민준
   Decription : Tact Time 조회
--------------------------------------------------------------------------------------
 [Change History]
 2022.01.21 안유수   C20220119-000391 T/T 모니터링 조회 조건 변경
 2022.08.11 임정구   C20220517-000215 조립만 나오도록 되어 있어서 활성화 추가
 2022.09.27 정연준                    조회시 콤보박스 C_EQPT가 있다면 EQPTID를 대신해서 넣도록 수정 (EQPTLEVEL이 M인 설비는 실재하지 않아서 대표 설비를 조회)
 2022.10.04 정연준                    공정 조회시 활성화는 별도의 BIZ를 호출하도록 변경 (DGS, EOL, 포장만 표시하기 위함)
 2025.06.28 김선준                    PACK일경우 설비콤보박스수정, LOTID컬럼추가/보이기, PACK전용조회
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_252 : UserControl, IWorkArea
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
        public COM001_252()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            initcombo();

            //조회조건 한달전 설정
            if (iLoadCnt == 0)
            {
                iLoadCnt = 1;
                this.CntInit.Text = "300";
            }

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("P")) //PACK
            {
                //this.dgLotInfo.Columns["LOTID"].Visibility = Visibility.Visible;
                this.tbTerm.Visibility = Visibility.Visible;
                this.tbCntInit.Visibility = Visibility.Collapsed;
                this.CntInit.Visibility = Visibility.Collapsed;
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
            C1ComboBox[] cbAreaCild = { cboEquipmentSegment, cboProcess, cboEquipment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cbAreaCild);

            //라인
            string sCase1 = null;
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F")) // 활성화이면
            {
                sCase1 = "EQUIPMENTSEGMENT_FORM";
            }
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: sCase1);

            //공정
            string sCase2 = null;
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F")) // 활성화이면
            {
                sCase2 = "DA_BAS_SEL_PROCESS_EQPTLOSS_CBO_FORM";
            }
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment, cboArea };
            C1ComboBox[] cbProcessChild = { cboEquipment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cbProcessChild, cbParent: cbProcessParent, sCase: sCase2);

            //설비 
            string sCase3 = null;
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F")) // 활성화이면
            {
                sCase3 = "DA_BAS_SEL_EQUIPMENT_EQPT_TACT_MNTR_CBO";
            }
            else if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("P")) //PACK
            {
                sCase3 = "DA_PRD_SEL_TACTTIME_MONITORING_EQPT_BY_HIS";
            }
            else
            {
                sCase3 = "cboEquipmentEqptLoss";
            }
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent, sCase: sCase3);
        }
        #endregion

        #region [조회버튼 클릭] SearchData() 
        private void SearchData()
        {
            string BizName = "DA_PRD_SEL_TACTTIME_MONITORING";
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("P")) //PACK
            {
                //BizName = "DA_PRD_SEL_TACTTIME_MONITORING_PACK";
            }
            else
            {
                // 기본조회건수 Set
                Int32 iCnt = 0;
                if (this.CntInit.Text == null || string.IsNullOrEmpty(this.CntInit.Text.Trim()) || !Int32.TryParse(this.CntInit.Text.Trim(), out iCnt))
                {
                    this.CntInit.Text = "300";
                }

                if (iCnt > 100000)
                {
                    Util.MessageValidation("SFU5030");   //최대 100000개 까지 가능합니다.
                    return;
                }
            }

            #region 30일초과
            TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

            if (timeSpan.Days < 0)
            {
                Util.MessageValidation("SFU3569");  //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                return;
            }

            if (timeSpan.Days > 30)
            {
                Util.MessageValidation("SFU4466");  //조회기간은 30일을 초과 할 수 없습니다.
                return;
            }

            if (null == this.cboEquipment.ItemsSource || this.cboEquipment.SelectedIndex < 0 || null == this.cboEquipment.SelectedValue)
            {
                Util.MessageValidation("SFU1672");   //설비 정보가 없습니다.
                return;
            }
            #endregion

            ShowLoadingIndicator();

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
            newRow["EQPTID"] = string.IsNullOrEmpty(cboEquipment.GetStringValue("C_EQPT")) ? Convert.ToString(cboEquipment.SelectedValue) : cboEquipment.GetStringValue("C_EQPT");   //설비
            newRow["TO_DTTM1"] = dtpDateTo.SelectedDateTime.AddDays(+1).ToString("yyyyMMdd");

            inDataTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService(BizName, "INDATA", "RSLTDT", inDataTable, (searchResult, searchException) =>
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
