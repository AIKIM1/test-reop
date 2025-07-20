/*************************************************************************************
 Created Date : 2020.12.16
      Creator : Kang Dong Hee
   Decription : JIG 불량 셀 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.16  NAME : Initial Created
  2023.03.09  NAME : JIG 불량 셀 조회 정보조회 UI GMES 이관
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_060.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_060 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public FCS001_060()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);

            //Combo Setting
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgJigDefectCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgJigDefectCellList.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Column.Name == "SUBLOTID")
                {
                    string selectedCellId = dgJigDefectCellList.CurrentCell.Text;
                    object[] parameters = new object[2];
                    parameters[0] = Util.NVC(selectedCellId);
                    parameters[1] = "Y";

                    this.FrameOperation.OpenMenu("SFU010710020", true, parameters);
                }
                else if (cell.Column.Name == "TRAYNO")
                {
                    string selectedTrayNo = dgJigDefectCellList.CurrentCell.Text;
                    object[] parameters = new object[6];
                    parameters[0] = "";
                    parameters[1] = Util.NVC(selectedTrayNo);

                    this.FrameOperation.OpenMenu("SFU010710010", true, parameters);
                }
            }
        }

        private void dgJigDefectCellList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor                    
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.Equals("SUBLOTID") || e.Cell.Column.Name.Equals("TRAYNO"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        #endregion

        #region Method
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            // 설비레인
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE");

            // 공정경로
            _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE");

            // 공정
            // 공정 콤보박스에 J/F Start, J/F End만 표기
            string[] sFilterOper = { "", "J", "", "J0,J9" };
            _combo.SetCombo(cboOper, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP", sFilter: sFilterOper);

            // 등급
            string[] sFilterGrade = { "COMBO_SUBLOTJUDGE_CHANGE" };
            _combo.SetCombo(cboGrade, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterGrade);


        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));
                dtRqst.Columns.Add("LANEID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("GRADE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("CELLID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["LANEID"] = Util.GetCondition(cboLane, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);
                dr["GRADE"] = Util.GetCondition(cboGrade, bAllNull: true);
                dr["LOTID"] = txtLotID.Text.IsNullOrEmpty() ? null : txtLotID.Text;
                dr["CELLID"] = txtCellID.Text.IsNullOrEmpty() ? null : txtCellID.Text;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_JIG_DEFECT_CELL_INFO_LIST", "RQSTDT", "RSLTDT", dtRqst, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgJigDefectCellList);
                        Util.GridSetData(dgJigDefectCellList, result, this.FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
