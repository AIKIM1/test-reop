/*************************************************************************************
 Created Date : 2020.10.13
      Creator : 
   Decription : 설비 상태 정보
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.13  DEVELOPER : Initial Created.
  2025.05.14  이준영      MES 2.0 전환 - 레인별 설비 그룹 코드에 따라 데이터 조회가 달라 조회 이벤트, DA에서 BIZRULE로 변경
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

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_010()
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
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLaneChild = { cboEqpKind };
            string[] sFilter = { "Y" };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LANE", cbChild: cboLaneChild, sFilter: sFilter);

            C1ComboBox[] cboEqpKindParent = { cboLane };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE", cbParent: cboEqpKindParent);

            string[] sFilterColor = { "EIOSTATCOLOR" };
            _combo.SetCombo(cboEqpStatus, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterColor);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();
                this.Loaded -= UserControl_Loaded;
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

        private void dgEqpStatus_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
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

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_CURRENT_STATUS_MB", "INDATA", "OUTDATA", inDataTable);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_EQP_CURRENT_STATUS_MB", "INDATA", "OUTDATA", inDataTable);
              
                //dgEqpStatus.ItemsSource = DataTableConverter.Convert(dtRslt);


                Util.GridSetData(dgEqpStatus, dtRslt, this.FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}