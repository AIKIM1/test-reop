/*************************************************************************************
 Created Date : 2021.03.22
      Creator : 김민석
   Decription : CELL 공급 프로젝트 PACK 팝업 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Documents;
using System.Data;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_084_ABNORMAL_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public bool bClick = false;
        string strEqsgID = string.Empty;
        CommonCombo _combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_084_ABNORMAL_POPUP()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    strEqsgID = Util.NVC(tmps[0]);

                    SearchLongTermData();
                    SearchNotInputCellData();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bClick = false;

                if (bClick == false)
                {
                    bClick = true;
                    if (bClick == true)
                    {
                        this.DialogResult = MessageBoxResult.Cancel;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                bClick = false;
                Util.MessageException(ex);
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Mehod

        #region 장기간 미완공 반제품 조회
        private void SearchLongTermData()
        {
            DataSet dsRslt = null;

            try
            {
                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = strEqsgID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_SEL_LOC_MNT_ABNML_LNGTM", "RQSTDT", "RSLTDT", ds, null);

                if (dsRslt != null)
                {
                    Util.GridSetData(dgLongTerm, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                    bClick = false;
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
                Util.MessageException(ex);
            }

        }
        #endregion

        #region Pallet 투입 완료 후 미투입 Cell
        private void SearchNotInputCellData()
        {
            DataSet dsRslt = null;

            try
            {
                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = strEqsgID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_SEL_LOC_MNT_NOT_INPUT_CELL", "RQSTDT", "RSLTDT", ds, null);

                if (dsRslt != null)
                {
                    Util.GridSetData(dgNotInputCell, dsRslt.Tables["RSLTDT"], FrameOperation, false);
                    bClick = false;
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                bClick = false;
                Util.MessageException(ex);
            }

        }
        #endregion

        #region Refresh
        private void Refresh()
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Refresh

        #endregion
        private void dgLongTerm_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.Equals("ELAPSED_DAY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #region Grid

        #endregion

        #region ComboBox

        #endregion


    }
}
