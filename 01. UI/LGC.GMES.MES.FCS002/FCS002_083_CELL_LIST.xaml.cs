/*************************************************************************************
 Created Date : 2020.12.08
      Creator : 
   Decription : Lot별 등급 수량
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.08  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows.Controls;
using System.Windows;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_083_CELL_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _sLotID;
        private string _sEqptID;
        private string _sGradeCd;

        public FCS002_083_CELL_LIST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion
        
        #region Initialize    

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _sLotID = tmps[0] as string;
            _sEqptID = tmps[1] as string;
            _sGradeCd = tmps[2] as string;

            txtLot.Text = _sLotID;
            cboGrade.SelectedValue = _sGradeCd;

            //조회함수
            GetList();
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilter = { "GR_LOT_LOW_VLTG_SORT_TYPE_CODE" };
            _combo.SetCombo(cboGrade, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN");
        }
        #endregion

        #region Event
        private void dgECell_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (dgECell.CurrentColumn.Name.Equals("CSTID"))
                    {
                        FCS002_021 wndTRAY = new FCS002_021();
                        wndTRAY.FrameOperation = FrameOperation;

                        object[] Parameters = new object[10];
                        Parameters[0] = ""; //Tray ID
                        Parameters[1] = DataTableConverter.GetValue(dgECell.CurrentRow.DataItem, "LOTID").ToString(); //Tray No
                        this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    else if (dgECell.CurrentColumn.Name.Equals("SUBLOTID"))
                    {
                        //Open Cell Form
                        FCS002_022 fcs022 = new FCS002_022();
                        fcs022.FrameOperation = FrameOperation;

                        object[] parameters = new object[2];
                        parameters[0] = DataTableConverter.GetValue(dgECell.CurrentRow.DataItem, "SUBLOTID").ToString();
                        parameters[1] = "Y"; //_sActYN

                        this.FrameOperation.OpenMenuFORM("FCS002_022", "FCS002_022", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Cell 정보조회"), true, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgECell_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    if (e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));
        }

        #endregion

        #region Mehod

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtRqst.Columns.Add("GRADE_CD", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DAY_GR_LOTID"] = _sLotID;
                dr["GRADE_CD"] = _sGradeCd;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOW_VOLT_SORTING_LOT_CELL", "RQSTDT", "RSLTDT", dtRqst);
                
                Util.GridSetData(dgECell, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
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

        
    }
}
