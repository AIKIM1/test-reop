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
    public partial class BOX001_005_CELL_DETL : C1Window, IWorkArea
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


        public BOX001_005_CELL_DETL()
        {
            InitializeComponent();
            Loaded += BOX001_005_CELL_DETL_Loaded;
        }

        private void BOX001_005_CELL_DETL_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            txtPalletid.Text = tmps[0] as string;

            getCellInfo(txtPalletid.Text.Trim());
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
        private void getCellInfo(string sPalletID)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_DETL_CP", "RQSTDT", "RSLTDT", RQSTDT);


                dgCellInfo.ItemsSource = DataTableConverter.Convert(dtResult);

                string[] sColumnName = new string[] {"TRAYID"};
                _Util.SetDataGridMergeExtensionCol(dgCellInfo, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgCellInfo);
        }
    }
}
