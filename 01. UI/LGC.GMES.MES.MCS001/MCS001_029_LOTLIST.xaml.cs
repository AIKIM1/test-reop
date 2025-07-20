/*************************************************************************************
 Created Date : 2020.11.10
      Creator : 신광희 차장
   Decription : W/O별 현황 Lot List [팝업]
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.10  신광희 차장 : Initial Created.    
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;
namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_029_LOTLIST : C1Window, IWorkArea
    {

		#region Declaration & Constructor 

        private string _areaCode;
        private string _equipmentSegmentCode;
        private string _projectName;
        private string _type;


		public MCS001_029_LOTLIST()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            object[] parameters = C1WindowExtension.GetParameters( this );

            if (parameters != null)
            {
                _areaCode = Util.NVC(parameters[0]);
                _equipmentSegmentCode = Util.NVC(parameters[1]);
                _projectName = Util.NVC(parameters[2]);
                _type = Util.NVC(parameters[3]);

                if (_type == "FOL_EQSG_MODEL_QTY" || _type == "FOL_MODEL_QTY")
                {
                    dgList.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
                    dgList.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                    dgList.Columns["MCS_EQPTNAME"].Visibility = Visibility.Collapsed;
                }
                if (_type == "AN_QTY_AF_NT" || _type == "CA_QTY_AF_NT")
                {
                    dgList.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                 }


                
                
                SelectLotListByWorkOrder();
            }

        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Mehod

        private void SelectLotListByWorkOrder()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_SUMMARY_BY_ASSY_WO_LOT_LIST";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("TYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _areaCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["PRJT_NAME"] = _projectName;
                dr["TYPE"] = _type;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, null, true);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
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


        #endregion

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "VD_QA_RESULT"))
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name) == null || DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).ToString() != "OK")
                     {
                        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["VD_QA_RESULT"].Index).Presenter.Background = new SolidColorBrush(Colors.Red);
                        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["VD_QA_RESULT"].Index).Presenter.FontWeight = FontWeights.Bold;
                        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["VD_QA_RESULT"].Index).Presenter.Foreground = new SolidColorBrush(Colors.White);
                        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["VD_QA_RESULT"].Index).Presenter.Cursor = Cursors.Arrow;
                
                    }
                    //else
                    //{
                    //    if (_type != "FOL_EQSG_MODEL_QTY" || _type != "FOL_MODEL_QTY")
                    //    {
                    //        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["MCS_EQPTNAME"].Index).Presenter.Background = new SolidColorBrush(Colors.Orange);
                    //        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["MCS_EQPTNAME"].Index).Presenter.FontWeight = FontWeights.Bold;
                    //        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["MCS_EQPTNAME"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    //        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns["MCS_EQPTNAME"].Index).Presenter.Cursor = Cursors.Arrow;
                    //    }
                      
                    //}
                  
                }
     
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }
    }
}