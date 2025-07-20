/*************************************************************************************
 Created Date : 2019-03-06
      Creator : 이동우 사원
   Decription : 하프슬리터로 이동 팝업
--------------------------------------------------------------------------------------
 [Change History]

  
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Input;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_EQPT_COND_SEARCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_MOVE_HALF : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _ProcID = string.Empty;
        private string _EqptID = string.Empty;

        private string _LineName = string.Empty;
        private string _EqptName = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        #endregion

        #region Initialize    
        public CMM_ELEC_MOVE_HALF()
        {
            InitializeComponent();
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
        #endregion

        #region Mehod

        #region Biz

        private void MovetoHalf_Lot_inform()
        {
            try
            {
                string sLotid = txtLOTID.Text.ToString().Trim();
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                    {
                        txtLOTID.Text = "";
                        Util.Alert("SFU1504");  //동일한 LOT이 스캔되었습니다.
                        return;
                    }
                }


                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));


                DataRow newRow = inDataTable.NewRow();
                newRow["LOTID"] = sLotid;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = "EBD01";

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_GET_ANOM_HS_LOT", "RQSTDT", "OUTDATA", inDataTable);

                if (dgLotInfo.GetRowCount() == 0)
                {
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtRslt);
                    txtLOTID.Text = "";
                }
                else
                {
                    DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                    dtSource.Merge(dtRslt);

                    Util.gridClear(dgLotInfo);
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtSource);

                    txtLOTID.Text = "";
                }
                Util.DataGridCheckAllChecked(dgLotInfo,true);
               // Util.GridSetData(dgLotInfo, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }


        #endregion

        #region Func
        private void Init()
        {
            txtLOTID.Text = "";
            txtLOTID.Focus();

        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void InitializeGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                Util.gridClear(dg);

                if (dg == null) return;

                List<C1.WPF.DataGrid.DataGridColumn> dgList = new List<C1.WPF.DataGrid.DataGridColumn>();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    if (col.Name.IndexOf(":") >= 0)
                    {
                        dgList.Add(col);
                    }
                }

                if (dgList.Count > 0)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn col in dgList)
                    {
                        dg.Columns.Remove(col);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeGridHeaders()
        {

        }
        #endregion

        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
          
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgLotInfo);
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }


        private void txtLOTID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MovetoHalf_Lot_inform();
            }
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgLotInfo.Rows.Count == 0) return;

                ShowLoadingIndicator();


                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");

                DataRow newRow = inTable.NewRow();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID_FR", typeof(string));
                inTable.Columns.Add("PROCID_TO", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID_FR"] = Process.SLITTING;
                newRow["PROCID_TO"] = Process.HALF_SLITTING;
                newRow["NOTE"] = "";
                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inMtrlTable = indataSet.Tables.Add("IN_LOT");

                newRow = inMtrlTable.NewRow();
                inMtrlTable.Columns.Add("LOTID", typeof(string));

                DataTable dtProductLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                foreach (DataRow _iRow in dtProductLot.Rows)
                {
                    string a = _iRow["CHK"].ToString();
                    if (_iRow["CHK"].ToString().Equals("True") || _iRow["CHK"].ToString().Equals("1"))
                    {
                        newRow = inMtrlTable.NewRow();

                        newRow["LOTID"] = _iRow["LOTID"];
                        indataSet.Tables["IN_LOT"].Rows.Add(newRow);
                    }
                }

                //inMtrlTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_MOVE_HS", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Util.gridClear(dgLotInfo);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
    }
}