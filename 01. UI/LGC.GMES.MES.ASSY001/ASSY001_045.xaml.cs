/*************************************************************************************
 Created Date : 2019.10.15
      Creator : 고재영
   Decription : 설비 불량률 Summary 조회
--------------------------------------------------------------------------------------
 [Change History]
 2020.03.03                오름차순, 내림차순 기능 추가
 2020.08.14 0.1  이상훈    C20200814-000077 조회 데이터 확인후 SummaryData() 함수 수행 되도록 변경
 2023.10.30 0.2  강성묵    chack box (Main) and combobox (Mechine) - Nurcholish
 2024.01.08 안유수 E20231227-000372 수집시간(SUM_WRK_DTTM), LOTID, 설비 완공 수량(EQPT_END_QTY) 컬럼 화면 고정 처리
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_045 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        #region Declare
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        #endregion

        #region FrameOperation
        /// <summary>
        /// 
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public ASSY001_045()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
            ComboInitialize();
        }
        #endregion

        #region Combobox Initialize
        /// <summary>
        /// 
        /// </summary>
        private void ComboInitialize()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            //String[] sFilter = { "A7000", Process.LAMINATION, Process.STACKING_FOLDING }; MES 2.0 Lamination Visible
            String[] sFilter = { };
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent, sFilter: sFilter);
            // 2023.10.30 강성묵 chack box (Main) and combobox (Mechine) Nurcholish 2023.10.30  -- cbo Mechine
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboProcess_SelectedValueChanged(null, null);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);

            // 2023.10.30 강성묵 chack box (Main) and combobox (Mechine) Nurcholish 2023.10.30  -- cbo Mechine
            SetMachineEqptCombo(cboEquipment_Machine);
            cboEquipment.SelectedValueChanged += cboEquipment_SelectedValueChanged;

        }
        #endregion

        #endregion

        #region Mehod

        #region SearchData
        /// <summary>
        /// Data를 조회하여 Grid에 바인드 한다
        /// </summary>
        private void SearchData()
        {
            try
            {
                dgLotInfo.ItemsSource = null;

                if (dtpDateFrom.SelectedDateTime.Date > DateTime.Now.Date)
                {
                    Util.MessageValidation("SFU1739"); //오늘 이후 날짜는 선택할 수 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("WRK_DATE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["WRK_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);

                // 2023.10.30 강성묵 chack box (Main) and combobox (Mechine)
                //dr["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipment.SelectedValue);

                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue) == "" ? null : Util.NVC(cboProcess.SelectedValue);

                // 2023.10.30 강성묵 chack box (Main) and combobox (Mechine)
                if (gcMain.Visibility == Visibility.Visible && chkMain.IsChecked == false /*&& cboEquipment_Machine.SelectedValue.GetString() != ""*/) //if chk uncheck and select mechine > input parameter Mechine -- 2023.10.30 Nurcholish
                {
                    dr["EQPTID"] = Convert.ToString(cboEquipment_Machine.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipment_Machine.SelectedValue);
                }
                else
                {
                    dr["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipment.SelectedValue);
                }
                

                dt.Rows.Add(dr);

                //C20200814-000077 조회 데이터 확인후 SummaryData() 함수 수행 되도록 변경
                //DataTable result = SummaryData(new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DFCT_RATE_MNT", "INDATA", "RSLTDT", dt));
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DFCT_RATE_MNT", "INDATA", "RSLTDT", dt);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    DataTable result = SummaryData(dtRslt);


                    if (result.Rows.Count > 0)
                        Util.GridSetData(dgLotInfo, result, FrameOperation, true);
                    else
                        Util.MessageValidation("SFU1498");   //데이터가 없습니다.
                }
                else
                {
                    Util.MessageValidation("SFU1498");   //데이터가 없습니다.
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region SummaryData
        /// <summary>
        /// DB에서 조회한 데이터를 Pivot처리한다
        /// </summary>
        /// <param name="srcDt"></param>
        /// <returns></returns>
        private DataTable SummaryData(DataTable srcDt)
        {
            DataTable newDt = new DataTable();

            Dictionary<string, string> dfctColInfo = new Dictionary<string, string>();
            Dictionary<string, string> wrkDttm = new Dictionary<string, string>();

            foreach (DataRow dr in srcDt.Rows)
            {
                if (!dfctColInfo.ContainsKey(dr["EQPT_DFCT_CODE"].ToString()))
                {
                    dfctColInfo.Add(dr["EQPT_DFCT_CODE"].ToString(), dr["EQPT_DFCT_NAME"].ToString());
                }
                if (!wrkDttm.ContainsKey(dr["SUM_WRK_DTTM"].ToString()))
                {
                    wrkDttm.Add(dr["SUM_WRK_DTTM"].ToString(), dr["SUM_WRK_DTTM"].ToString());
                }
            }

            
            newDt.Columns.Add("SUM_WRK_DTTM");
            newDt.Columns.Add("LOTID");
            newDt.Columns.Add("EQPT_END_QTY");
            newDt.Columns.Add("WRK_DATE");
            newDt.Columns.Add("PROCID");
            newDt.Columns.Add("EQSGID");
            newDt.Columns.Add("EQPTID");
            

            foreach (string dfcd in dfctColInfo.Keys)
            {
                newDt.Columns.Add("LEFT_" + dfctColInfo[dfcd]);
                newDt.Columns.Add("RIGHT_" + dfctColInfo[dfcd]);
            }

            SetGridColumn(newDt.Columns);

            foreach (string wTime in wrkDttm.Keys)
            {
                DataTable tempdt = srcDt.Select("SUM_WRK_DTTM='" + wTime + "'").CopyToDataTable();

                DataRow newdr = newDt.NewRow();
                newdr["WRK_DATE"] = tempdt.Rows[0]["WRK_DATE"];
                newdr["SUM_WRK_DTTM"] = tempdt.Rows[0]["SUM_WRK_DTTM"];
                newdr["LOTID"] = tempdt.Rows[0]["LOTID"];
                newdr["PROCID"] = tempdt.Rows[0]["PROCID"];
                newdr["EQSGID"] = tempdt.Rows[0]["EQSGID"];
                newdr["EQPTID"] = tempdt.Rows[0]["EQPTID"];
                newdr["EQPT_END_QTY"] = tempdt.Rows[0]["EQPT_END_QTY"];

                foreach (DataRow dr in tempdt.Rows)
                {
                    newdr["LEFT_" + dr["EQPT_DFCT_NAME"].ToString()] = dr["LEFT_DFCT_QTY"];
                    newdr["RIGHT_" + dr["EQPT_DFCT_NAME"].ToString()] = dr["RIGHT_DFCT_RATE"];
                }

                newDt.Rows.Add(newdr);

            }
            if(rdoDesc.IsChecked == true)
                newDt = newDt.Select("", "SUM_WRK_DTTM DESC").CopyToDataTable();

            return newDt;
        }
        #endregion

        #region Set Grid Columns
        /// <summary>
        /// Grid의 Column 동적 세팅
        /// </summary>
        /// <param name="dcc"></param>
        private void SetGridColumn(DataColumnCollection dcc)
        {
            dgLotInfo.Columns.Clear();

            foreach (DataColumn dc in dcc)
            {
                C1.WPF.DataGrid.DataGridTextColumn d = new C1.WPF.DataGrid.DataGridTextColumn();
                if (dc.ColumnName.Contains("LEFT_"))
                {
                    List<string> args = new List<string>();
                    args.Add(dc.ColumnName.Replace("LEFT_", "").ToUpper());
                    args.Add("DFCT_QTY");
                    d.Header = args;
                    d.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else if (dc.ColumnName.Contains("RIGHT_"))
                {
                    List<string> args = new List<string>();
                    args.Add(dc.ColumnName.Replace("RIGHT_", "").ToUpper());
                    args.Add("DFCT_RATE");
                    d.Header = args;
                    d.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else if (dc.ColumnName.Contains("WRK_DATE") || dc.ColumnName.Contains("PROCID") || dc.ColumnName.Contains("EQSGID") || dc.ColumnName.Contains("EQPTID"))
                {
                    d.Header = dc.ColumnName;
                    d.HorizontalAlignment = HorizontalAlignment.Left;
                    d.Visibility = Visibility.Collapsed;
                }
                else
                {
                    d.Header = dc.ColumnName;
                    d.HorizontalAlignment = HorizontalAlignment.Left;
                }
                d.Binding = new System.Windows.Data.Binding(dc.ColumnName);
                dgLotInfo.Columns.Add(d);

                dgLotInfo.Refresh();
            }

            //dgLotInfo.TopRows.Add(new C1.WPF.DataGrid.DataGridColumnHeaderRow());
            //dgLotInfo.TopRows.Add(new C1.WPF.DataGrid.DataGridColumnHeaderRow());
        }
        #endregion

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return; ;

            SearchData();
        }
        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }
        #endregion


        #region Edit Chk & Cbo Mechine
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            gcMain.Visibility = Visibility.Collapsed;
            gcMachine.Visibility = Visibility.Collapsed;

            if (Util.NVC(cboProcess.SelectedValue) != "")
            {
                SetMachineUse(cboProcess.SelectedValue.GetString());
            }
        }

        // Nurcholish 2023.10.30 
        /// <summary>
        /// Display value in CBO Mechine
        /// </summary>
        /// <param name="cbo"></param>
        private void SetMachineEqptCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_MACHINE_EQUIPMENT_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(cboArea.SelectedValue.GetString()) ? null : cboArea.SelectedValue.GetString();
            dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
            dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
            dr["EQPTID"] = string.IsNullOrEmpty(cboEquipment.SelectedValue.GetString()) ? null : cboEquipment.SelectedValue.GetString();
            inTable.Rows.Add(dr);
            
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            //DataRow newRow = dtResult.NewRow();
            //newRow["CBO_CODE"] = "";
            //newRow["CBO_NAME"] = "-ALL-";
            //dtResult.Rows.InsertAt(newRow, 0);
            if (dtResult != null)
            {
                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }

        }

        //Nurcholish 2023.10.30  
        /// <summary>
        /// Refresh CBO Machine and chack box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetMachineEqptCombo(cboEquipment_Machine);
            chkMain_Click(null,null);
        }

        //Nurcholish 2023.10.30 
        /// <summary>
        /// Check box for display cbo mechine 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkMain_Click(object sender, RoutedEventArgs e)
        {
            if (gcMain.Visibility == Visibility.Visible && chkMain.IsChecked == false)
            {
                gcMachine.Visibility = Visibility.Visible;
            }
            else
            {
                gcMachine.Visibility = Visibility.Collapsed;
            }
        }
        private void SetMachineUse(string sProcId)
        {
            gcMain.Visibility = Visibility.Collapsed;
            gcMachine.Visibility = Visibility.Collapsed;

            DataTable dtInTable = new DataTable("RQSTDT");
            dtInTable.Columns.Add("AREAID", typeof(string));
            dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            dtInTable.Columns.Add("COM_CODE", typeof(string));

            DataRow drNewRow = dtInTable.NewRow();
            drNewRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            drNewRow["COM_TYPE_CODE"] = "EQPTLOSS_MACHINE_EQPT_MODIFY_PROCESS";
            drNewRow["COM_CODE"] = sProcId;
            dtInTable.Rows.Add(drNewRow);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dtInTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                gcMain.Visibility = Visibility.Visible;

                if (chkMain.IsChecked == false)
                {
                    gcMachine.Visibility = Visibility.Visible;
                }
            }
        }
        #endregion
    }
}
