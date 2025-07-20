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
    public partial class MON001_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public MON001_010()
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
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateFrom.SelectedDateTime = DateTime.Now;

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
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);

                dr["DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HEALTH_INDICATOR_NO_ERP_WO", "INDATA", "OUTDATA", dtRqst);
                
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
