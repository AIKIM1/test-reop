/*************************************************************************************
 Created Date : 2018.04.06
      Creator : 허일S
   Decription : 마감용 재공 조회
--------------------------------------------------------------------------------------
 [Change History]




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_041 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public ASSY001_041()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;

        }


        #endregion
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;

            initcombo();

        }
   

       
        #region Event

        #region[재공조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return; ;

            SearchData();
        }

    
        private void txtLOTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }

        }

        #endregion
        
        #endregion

        #region Mehod

        private void initcombo()
        {

            C1ComboBox[] cbAreaCild = { cboEquipmentSegment, cboProcess, cboEquipment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild:cbAreaCild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cbProcessParent = {  cboEquipmentSegment, cboArea};
            C1ComboBox[] cbProcessChild = { cboEquipment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cbProcessChild, cbParent: cbProcessParent);

            C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent:cbEquipmentParent);


        }
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

                if (dtpDateTo.SelectedDateTime.Date > DateTime.Now.Date)
                {
                    Util.MessageValidation("SFU1739"); //오늘 이후 날짜는 선택할 수 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("FROM_DTTM", typeof(string));
                dt.Columns.Add("TO_DTTM", typeof(string));

                DataRow dr = dt.NewRow();
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                dr["AREAID"] = Convert.ToString(cboArea.SelectedValue).Equals("") ? null : Convert.ToString(cboArea.SelectedValue);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipment.SelectedValue);
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue) == ""? null : Util.NVC(cboProcess.SelectedValue);
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_MOUNT_WIP_SNAP", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgLotInfo, result, FrameOperation, true);

             
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        #endregion
    


     
    }
}
