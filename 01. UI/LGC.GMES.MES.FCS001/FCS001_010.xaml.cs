/*************************************************************************************
 Created Date : 2020.10.13
      Creator : 
   Decription : 설비 상태 정보
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.13  DEVELOPER : Initial Created.





 
**************************************************************************************/



using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_010()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboLaneChild = { cboEqpKind };

            //LANE_GROUP ->LINE으로 변경
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbChild: cboLaneChild);

            C1ComboBox[] cboEqpKindParent = { cboLane };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE", cbParent: cboEqpKindParent);

            string[] sFilter = { "EIOSTATCOLOR" };
            _combo.SetCombo(cboEqpStatus, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Method
        private void GetList()
        {
           try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("S71", typeof(string));
                inDataTable.Columns.Add("S70", typeof(string));
                inDataTable.Columns.Add("EIOSTAT", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["S71"] = Util.GetCondition(cboLane, bAllNull: true);
                newRow["S70"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                newRow["EIOSTAT"] = Util.GetCondition(cboEqpStatus, bAllNull: true);

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_CURRENT_STATUS", "INDATA", "OUTDATA", inDataTable);
                dgEqpStatus.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
