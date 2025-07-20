/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.03.24  LEEHJ  : 소형활성화MES 복사
  
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_318_CELL : C1Window, IWorkArea
    {
        private string _ProdLotId = string.Empty;
        private string _CstdId = string.Empty;
        private string _LotId = string.Empty;

        #region Declaration & Constructor 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_318_CELL()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 4)
            {
                _ProdLotId = tmps[0].ToString();
                _LotId = tmps[1].ToString();
                _CstdId = tmps[2].ToString();
            }

            txtProdLot.Text = _ProdLotId;
            txtTrayID.Text = _CstdId;

            GetCellList();
        }

 
        #endregion

        #region Mehod
        #region [조회]
        private void GetCellList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROD_LOTID"] = _ProdLotId;
                dr["CSTID"] = _CstdId;
                dr["LOTID"] = _LotId;

                dtRqst.Rows.Add(dr);


                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_CELL_LIST", "INDATA", "OUTDATA", dtRqst);
                dgCellList.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #endregion

    }
}
