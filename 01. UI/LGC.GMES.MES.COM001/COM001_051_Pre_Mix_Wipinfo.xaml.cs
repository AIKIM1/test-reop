/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_051_Pre_Mix_Wipinfo : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
       


        public COM001_051_Pre_Mix_Wipinfo()
        {
            InitializeComponent();

            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //tmp = C1WindowExtension.GetParameter(this);
            //tmps = C1WindowExtension.GetParameters(this);
            //tmmp01 = tmps[0] as string;
            //tmmp02 = tmps[1] as string;
            //tmmp03 = tmps[2] as string;

            this.Loaded -= Window_Loaded;

            //  Initialize();
            initCombo();

        }

        private void initCombo()
        {
            CommonCombo combo = new CommonCombo();

            string[] sFilter = { "E0700", Wip_State.WAIT };
            combo.SetCombo(cboMixProd, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Event
        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("WIPSTAT", typeof(string));
            dt.Columns.Add("FROM_DATE", typeof(string));
            dt.Columns.Add("TO_DATE", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            row["PROCID"] = Process.PRE_MIXING_PACK;
            row["WIPSTAT"] = Wip_State.WAIT;
            row["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            row["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
            row["PRODID"] = Convert.ToString(cboMixProd.SelectedValue).Equals("") ? null : Convert.ToString(cboMixProd.SelectedValue);
            dt.Rows.Add(row);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_PRD_SEL_BOX_PRE_MIX_WIP", "RQUST", "RSULT", dt,(searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.gridClear(dgWipInfo);

                    Util.GridSetData(dgWipInfo, searchResult, null, false);


                    if (searchResult.Rows.Count > 0)
                    {
                        string[] sColumnName = new string[] { "BOX_STAT" };
                        _Util.SetDataGridMergeExtensionCol(dgWipInfo, sColumnName, DataGridMergeMode.VERTICAL);

                        dgWipInfo.GroupBy(dgWipInfo.Columns["BOX_STAT"], DataGridSortDirection.None);
                        dgWipInfo.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                        DataGridAggregate.SetAggregateFunctions(dgWipInfo.Columns["BOX_STAT"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                        DataGridAggregate.SetAggregateFunctions(dgWipInfo.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }

            });

 

        }

     

    


        #endregion

    


    }
}
