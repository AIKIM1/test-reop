/*************************************************************************************
 Created Date : 2016.11.05
      Creator : cnslss
   Decription : 생산실적 변경이력
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.04.29  정문교    : 폴란드3동 대응 Carrier ID(CSTID) 조회조건 및 조회 칼럼 추가
  2021.07.15  김지은    : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_019 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public COM001_019()
        {
            InitializeComponent();
            InitCombo();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }
        #endregion


        #region Initialize

        #endregion
        
        #region Event
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");

                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");
                //공정
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cboProcessChild = { cboEquipment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESS");
                //설비
                C1ComboBox[] cboEquipmentChild = { cboProd };
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentChild, cbParent: cboEquipmentParent, sCase: "EQUIPMENT");

                //모델
                C1ComboBox[] cboMountPstParent = { cboEquipmentSegment, cboProcess, cboEquipment };
                _combo.SetCombo(cboProd, CommonCombo.ComboStatus.ALL, cbParent: cboMountPstParent, sCase: "cboProducts");
                //cboEqptModel

                dtpFrom.SelectedDateTime = System.DateTime.Now;
                dtpTo.SelectedDateTime = System.DateTime.Now;

                //생산구분
                string[] sFilter = new string[] { "PRODUCT_DIVISION" };
                _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

                // 생산구분 Default 정상생산
                if (cboProductDiv.Items.Count > 1)
                    cboProductDiv.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDecimal(Convert.ToDateTime(dtpFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            string sProcess = cboProcess.SelectedValue.ToString();
            string sLine = cboEquipmentSegment.SelectedValue.ToString();
            string sEquip = cboEquipment.SelectedValue.ToString();
            string sProd = cboProd.SelectedValue.ToString();
            string sLotId = null;
            string sCstId = null;


            if (sProcess == "")
            {
                sProcess = null;
            }
            if (sLine == "")
            {
                sLine = null;
            }
            if (sEquip == "")
            {
                sEquip = null;
            }
            if (sProd == "")
            {
                sProd = null;
            }

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("ACTID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("PRODID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("ACTFROM", typeof(string));
            inTable.Columns.Add("ACTTO", typeof(string));
            inTable.Columns.Add("CSTID", typeof(string));
            //inTable.Columns.Add("NORMAL", typeof(string));
            //inTable.Columns.Add("PILOT", typeof(string));
            inTable.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["ACTID"] = "MODIFY_LOT";
            newRow["EQSGID"] = sLine;
            newRow["PROCID"] = sProcess;
            newRow["EQPTID"] = sEquip;
            newRow["PRODID"] = sProd;
            if (txtLotId.Text == "")
            {
                newRow["LOTID"] = sLotId;
            }
            else
            {
                newRow["LOTID"] = txtLotId.Text;
            }
            newRow["ACTFROM"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
            newRow["ACTTO"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");

            if (string.IsNullOrWhiteSpace(txtCstId.Text))
            {
                newRow["CSTID"] = sCstId;
            }
            else
            {
                newRow["CSTID"] = txtCstId.Text;
            }

            // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
            //if (cboProductDiv.SelectedValue.ToString() == "P")
            //    newRow["NORMAL"] = cboProductDiv.SelectedValue.ToString();
            //else if (cboProductDiv.SelectedValue.ToString() == "X")
            //    newRow["PILOT"] = cboProductDiv.SelectedValue.ToString();
            newRow["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);

            inTable.Rows.Add(newRow);

            dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MODYFY_LOT", "INDATA", "OUTDATA", inTable);

            dgLotList.ItemsSource = DataTableConverter.Convert(dtMain);

            string[] sColumnName = new string[] { "EQSGNAME", "PROCNAME", "EQPTNAME", "PRODID","PRODNAME","LOTID", "LOTYNAME" };
            _Util.SetDataGridMergeExtensionCol(dgLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
        }
        #endregion

        #region Mehod
        
        #endregion
    }
}
