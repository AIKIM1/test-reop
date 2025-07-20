/*************************************************************************************
 Created Date : 2021-04-01
      Creator : 송교진
   Decription : 포장 Pallet 구성(자동) 내 Cell 조회 팝업 화면
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.01  송교진 최초생성





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_011_OCV2_NG_CELL_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_011_OCV2_NG_CELL_LIST()
        {
            InitializeComponent();
            Loaded += BOX001_011_OCV2_NG_CELL_LIST_Loaded;
        }

        private void BOX001_011_OCV2_NG_CELL_LIST_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            DataTable indataSet = tmps[0] as DataTable;

            getCellInfo(indataSet);
        }
        
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Mehod

        /// <summary>
        /// Pallet 내 Cell 상세 조회
        /// </summary>
        /// <param name="dataItem"></param>
        private void getCellInfo(DataTable indataSet)
        {
            try
            {

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_OCV2_BY_OUTER_BOXID", "RQSTDT", "RSLTDT", indataSet);


                dgCellInfo.ItemsSource = DataTableConverter.Convert(dtResult);

                string[] sColumnName = new string[] {"TRAY_ID","OUTER_BOXID"};
                _Util.SetDataGridMergeExtensionCol(dgCellInfo, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

    }
}
