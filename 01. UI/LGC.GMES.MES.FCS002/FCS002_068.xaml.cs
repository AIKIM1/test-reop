/*************************************************************************************
 Created Date : 2020.11.30
      Creator : 
   Decription : 설비 Loss 입력 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.30 AKB : Initial Created.
  2022.06.27 강동희 : Ultium Cell 기준으로(MAIN & MACHIN) 수정
  2022.07.07 이정미 : 2022.05.17 기준으로 RollBack
  2022.08.01 조영대 : AREAID 인수 추가(Main 설비만 보기), 
**************************************************************************************/

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_068 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_068()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilterEqpType = { "DEG,EOL" };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "EQUIPMENTGROUP", sFilter: sFilterEqpType);

            dtpFromDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void cboEqpKind_SelectionCommitted(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            btnSearch.PerformClick();
        }

        private void dgEqpLossStatus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgEqpLossStatus.CurrentRow != null && dgEqpLossStatus.CurrentColumn.Name.Equals("JOBDATE"))
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQPTID", typeof(string));
                    RQSTDT.Columns.Add("WRK_DATE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEqpLossStatus.CurrentRow.DataItem, "EQPTID"));
                    dr["WRK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgEqpLossStatus.CurrentRow.DataItem, "JOBDATE"));
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_DETAIL_MB", "RQSTDT", "RSLTDT", RQSTDT);

                    Util.GridSetData(dgEqpLossDetail, dtResult, FrameOperation, true);

                    if (dtResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dgEqpLossDetail, dtResult, FrameOperation, true);

                        string[] sColumnName = new string[] { "EQPTNAME" };
                        _Util.SetDataGridMergeExtensionCol(dgEqpLossDetail, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                    }
                    else
                    {
                        dgEqpLossDetail.ItemsSource = null;
                    }

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgEqpLossStatus_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("JOBDATE"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                if (e.Cell.Column.Name.Equals("NOINPUT_CNT"))
                {
                    if (!(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "NOINPUT_CNT")).Equals("")) || !(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "NOINPUT_CNT")).Equals("0")))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }

                if (e.Cell.Column.Name.Equals("EQPTNAME"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")).Equals("SUM"))
                    {
                        for (int i = 0; i < dgEqpLossStatus.Columns.Count; i++)
                        {
                            C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, i);
                            cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                            cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                        }
                    }
                }
            }));
        }
        #endregion

        #region Method
        /// <summary>
        /// Loss 내역 조회
        /// </summary>
        private void GetList()
        {
            try
            {
                decimal INPUT_RATE = 0;
                int ALL_CNT = 0;
                int INPUT_CNT = 0;
                int NOINPUT_CNT = 0;


                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTTYPE", typeof(string));
                inDataTable.Columns.Add("FROM_WRK_DATE", typeof(string));
                inDataTable.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTTYPE"] = Util.GetCondition(cboEqpKind, bAllNull: true);
                newRow["FROM_WRK_DATE"] = Util.GetCondition(dtpFromDate, bAllNull: true);
                newRow["TO_WRK_DATE"] = Util.GetCondition(dtpToDate, bAllNull: true);

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_KEYIN_RAW_MB", "INDATA", "OUTDATA", inDataTable);
                
                if (dtRslt.Rows.Count > 0)
                {
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        INPUT_RATE += Util.NVC_Decimal(dtRslt.Rows[i]["INPUT_RATE"].ToString());
                        ALL_CNT += Util.NVC_Int(dtRslt.Rows[i]["ALL_CNT"].ToString());
                        INPUT_CNT += Util.NVC_Int(dtRslt.Rows[i]["INPUT_CNT"].ToString());
                        NOINPUT_CNT += Util.NVC_Int(dtRslt.Rows[i]["NOINPUT_CNT"].ToString());
                    }

                    //SUM 추가
                    DataRow dr = dtRslt.NewRow();
                    dr["EQPTNAME"] = "SUM";
                    dr["INPUT_RATE"] = string.Format("{0:N1}", INPUT_RATE / dtRslt.Rows.Count);
                    dr["ALL_CNT"] = ALL_CNT.ToString();
                    dr["INPUT_CNT"] = INPUT_CNT.ToString();
                    dr["NOINPUT_CNT"] = NOINPUT_CNT.ToString();
                    dtRslt.Rows.Add(dr);

                    dgEqpLossStatus.ItemsSource = DataTableConverter.Convert(dtRslt);

                    string[] sColumnName = new string[] { "EQPTNAME" };
                    _Util.SetDataGridMergeExtensionCol(dgEqpLossStatus, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }
                
                dgEqpLossDetail.ItemsSource = null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        
    }
}
