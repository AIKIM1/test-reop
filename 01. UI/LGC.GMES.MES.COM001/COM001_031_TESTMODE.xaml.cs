/*************************************************************************************
 Created Date : 2017.12.19
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.19  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using System;
using System.Data;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_031_TESTMODE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _AreaID = string.Empty;
        private Util _Util = new Util();

        public COM001_031_TESTMODE()
        {
            InitializeComponent();
        }
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region 초기화
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 0)
            {
                _AreaID = Util.NVC(tmps[0]);
            }
            else
            {
                _AreaID = "";
            }

            InitCombo();
            //Search_TestMode();
        }

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

            //공정군
            C1ComboBox[] cboProcessSegmentParent = { cboArea };
            _combo.SetCombo(cboProcessSegment, CommonCombo.ComboStatus.ALL, cbParent: cboProcessSegmentParent);
        }
        #endregion

        #region Mehod     
        private void Search_TestMode()
        {
            try
            {
                DataTable dtRslt = new DataTable();
                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PCSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dr["PCSGID"] = Util.GetCondition(cboProcessSegment, bAllNull: true);

                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TESTMODE_MONITORING", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgList, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU3206");
                return;
            }

            Search_TestMode();
        }
    }
}
