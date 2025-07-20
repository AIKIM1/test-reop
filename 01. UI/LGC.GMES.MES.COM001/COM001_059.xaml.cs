/*************************************************************************************
 Created Date : 2016.11.23
      Creator : cnslss
   Decription : 설비 별 투입자재 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.


 
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
    public partial class COM001_059 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public COM001_059()
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

        #region Declaration & Constructor 


        #endregion

        #region Initialize

        #endregion
        
        #region Event
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                // Lot 이력조회
                C1ComboBox[] cboAreaChildHistory = { cboEquipmentSegments };
                _combo.SetCombo(cboAreas, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");

                //라인
                C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreas };
                C1ComboBox[] cboEquipmentSegmentChildHistory = { cboProcesss , cboEquipments };
                _combo.SetCombo(cboEquipmentSegments, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChildHistory, cbParent: cboEquipmentSegmentParentHistory, sCase: "EQUIPMENTSEGMENT");

                //공정
                C1ComboBox[] cboProcessParentHistory = { cboEquipmentSegments };
                C1ComboBox[] cboProcessChildHistory = { cboEquipments };
                _combo.SetCombo(cboProcesss, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChildHistory, cbParent: cboProcessParentHistory, sCase: "PROCESS");
                //설비
                C1ComboBox[] cboEquipmentParents = {  cboEquipmentSegments, cboProcesss};
                _combo.SetCombo(cboEquipments, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParents,sCase: "EQUIPMENT");
                // Lot 이력조회
                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "AREA");

                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");
                //공정
                C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cboProcessChild = { cboEquipment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESS");

                //설비
                //C1ComboBox[] cboEquipmentChild = { cboMtrl };
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment,cboProcess };
                //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbChild:cboEquipmentChild,cbParent: cboEquipmentParent, sCase: "EQUIPMENT");
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sCase: "EQUIPMENT");
                //자재
                //C1ComboBox[] cboMountPstParent = { cboEquipmentSegment, cboProcess, cboEquipment };
                //_combo.SetCombo(cboMtrl, CommonCombo.ComboStatus.ALL, cbParent: cboMountPstParent, sCase: "cboMaterialiD");

                //활동명
                _combo.SetCombo(cboActivitiReasonMTRL, CommonCombo.ComboStatus.ALL);

                dtpFrom.SelectedDateTime = System.DateTime.Now;
                dtpTo.SelectedDateTime = System.DateTime.Now;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Convert.ToDecimal(Convert.ToDateTime(dtpFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpTo.SelectedDateTime).ToString("yyyyMMdd")))
                {
                    Util.Alert("SFU1913");      //종료일자가 시작일자보다 빠릅니다.
                    return;
                }

                int PlantStandDate = getPlantStandDate();
                string sArea = cboArea.SelectedValue.ToString();                
                string sLine = cboEquipmentSegment.SelectedValue.ToString();
                string sProcess = sLine == "" ? "" :cboProcess.SelectedValue.ToString(); //line all 조건일때 공정은 모든 공정
                string sEquip = cboEquipment.SelectedValue.ToString();
                string sLotid = txtComId.Text.Trim();
                string sProdid = txtProId.Text.Trim();
                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpTo.SelectedDateTime);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("ACTFROM", typeof(string));
                inTable.Columns.Add("ACTTO", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("PLANT_STAND_DATE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = sLine == "" ? null : sLine;
                newRow["PROCID"] = sProcess == "" ? null : sProcess;
                newRow["EQPTID"] = sEquip == "" ? null : sEquip;
                newRow["MTRLID"] = sProdid == "" ? null : sProdid;
                newRow["INPUT_LOTID"] = sLotid == "" ? null : sLotid;
                newRow["ACTFROM"] = sLotid != "" ? null : _ValueFrom;
                newRow["ACTTO"] = sLotid != "" ? null : _ValueTo;
                newRow["AREAID"] = sArea;
                newRow["ACTID"] = Util.GetCondition(cboActivitiReasonMTRL) == "" ? null : Util.GetCondition(cboActivitiReasonMTRL);
                newRow["PLANT_STAND_DATE"] = PlantStandDate.ToString();

                inTable.Rows.Add(newRow);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_HIST_LIST", "INDATA", "OUTDATA", inTable);

                //dgMtrlHist.ItemsSource = DataTableConverter.Convert(dtMain);
                Util.GridSetData(dgMtrlHist, dtMain, FrameOperation, true);
                string[] sColumnName = new string[] { "PROCNAME", "EQPTNAME", "MTRLNAME", "MTRLID", "ACTNAME", "EQPT_MOUNT_PSTN_NAME", "PROD_LOTID", "OUT_LOTID" , "INPUT_LOTID"};
                _Util.SetDataGridMergeExtensionCol(dgMtrlHist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sProcess = cboProcesss.SelectedValue.ToString();
                string sLine = cboEquipmentSegments.SelectedValue.ToString();
                string sEquip = cboEquipments.SelectedValue == null ? "" : cboEquipments.SelectedValue.ToString();

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
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = sLine;
                newRow["PROCID"] = sProcess;
                newRow["EQPTID"] = sEquip;

                inTable.Rows.Add(newRow);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_LIST", "INDATA", "OUTDATA", inTable);

                //dgMtrlList.ItemsSource = DataTableConverter.Convert(dtMain);
                Util.GridSetData(dgMtrlList, dtMain, FrameOperation, true);
                string[] sColumnName = new string[] { "PROCNAME", "EQPTNAME" };//EQPT_MOUNT_PSTN_ID
                _Util.SetDataGridMergeExtensionCol(dgMtrlList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #region Mehod
        private int getPlantStandDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_DATE_WITH_PLANT", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0 )
                {
                    return Convert.ToInt32(dtResult.Rows[0]["PLANT_STAND_DATE"]);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }
        #endregion

    }


}
