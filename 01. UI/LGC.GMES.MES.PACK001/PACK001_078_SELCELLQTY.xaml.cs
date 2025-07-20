/*************************************************************************************
 Created Date : 2021.03.22
      Creator : 김민석
   Decription : CELL 공급 프로젝트 PACK 팝업 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_078_SELCELLQTY : C1Window, IWorkArea
    {

        public int BOXQTY
        {
            get
            {
                return iBoxQty;
            }

            set
            {
                iBoxQty = value;
            }
        }

        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        public bool bClick = false;
        string strMtrlID = string.Empty;
        int iBoxQty = 0;
        private Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_078_SELCELLQTY()
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
                    strMtrlID = Util.NVC(tmps[0]);
                    setCellPlt(strMtrlID);
                    this.Focus();
                    //this.Loaded -= new System.Windows.RoutedEventHandler(this.C1Window_Loaded);

                }

                
                this.BringToFront();
                this.Show();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        #region Button

        #region 닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #endregion

        #region Mehod

        #region 조회
        private void setCellPlt(string MtrlId)
        {
            try
            {
                DataSet ds = new DataSet();

                DataTable indata = new DataTable();
                indata.TableName = "RQSTDT";

                indata.Columns.Add("PRODID", typeof(string));


                DataRow dr = indata.NewRow();
                dr["PRODID"] = MtrlId;

                indata.Rows.Add(dr);

                ds.Tables.Add(indata);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO_PACK_COND_AUTO_CSLY2", "RQSTDT", "RSLTDT", indata);

                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Rows[0]["CHK"] = true;

                    Util.GridSetData(dgCellPerPlt, dtRslt, FrameOperation, false);
                    if(dgCellPerPlt.GetRowCount() > 0)
                    {
                        dgCellPerPlt.UpdateLayout();
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Grid
        private void dgCellPerPlt_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (!(e.Cell.Row.Index > 1)) return;  

                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {           
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = DataTableConverter.Convert(dgCellPerPlt.ItemsSource);

            iBoxQty = Util.NVC_Int(DataTableConverter.GetValue(rb.DataContext, "BOX_QTY"));
        }
    }
}
