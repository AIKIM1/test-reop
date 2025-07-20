/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
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

namespace LGC.GMES.MES.MON001
{
    public partial class MON001_004 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public MON001_004()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //IF모드
            String[] sFilter = { "EIOIFMODE"}; 
            _combo.SetCombo(cboIfMode, CommonCombo.ComboStatus.ALL, null, null, sFilter,  "COMMCODE");

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateTo.SelectedDateTime = DateTime.Now;
            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);

        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            //{
            //    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
            //    {
            //        Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
            //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
            //        return;
            //    }

            //    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
            //    {
            //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
            //        return;
            //    }
            //}
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetResult();
        }

        #endregion

        #region Mehod
        public void GetResult()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("DTTMFROM", typeof(string));
                dtRqst.Columns.Add("DTTMTO", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["IFMODE"] = Util.GetCondition(cboIfMode, bAllNull: true);
                dr["DTTMFROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DTTMTO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VALIDATION_BIZRULE", "INDATA", "OUTDATA", dtRqst);
                
                Util.GridSetData(dgResult, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [초기화]
        private void ClearValue()
        {
            Util.gridClear(dgResult);
        }
        #endregion
        
        #endregion

    }
}
