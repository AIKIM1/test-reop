/*************************************************************************************
 Created Date : 2022.12.05
      Creator : 
   Decription : Aging 출고 Buffer 관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.05  DEVELOPER : Initial Created.
 
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
    public partial class FCS002_209 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_209()
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
                //DataTable inDataTable = new DataTable();
                //inDataTable.Columns.Add("LANGID", typeof(string));
                //inDataTable.Columns.Add("S71", typeof(string));
                //inDataTable.Columns.Add("S70", typeof(string));
                //inDataTable.Columns.Add("EIOSTAT", typeof(string));

                //DataRow newRow = inDataTable.NewRow();
                //newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["S71"] = Util.GetCondition(cboLane, bAllNull: true);
                //newRow["S70"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                //newRow["EIOSTAT"] = Util.GetCondition(cboEqpStatus, bAllNull: true);

                //inDataTable.Rows.Add(newRow);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_CURRENT_STATUS", "INDATA", "OUTDATA", inDataTable);
                //dgEqpStatus.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}