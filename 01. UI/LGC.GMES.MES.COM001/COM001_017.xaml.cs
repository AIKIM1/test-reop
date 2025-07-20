/*************************************************************************************
 Created Date : 2022.12.26
      Creator : 
   Decription : 설비 Loss 등록 현황
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.26 이윤중 : Initial Created.
  2023.05.18 이윤중 : 설비 구분(cboEqpKind) ComboBox 데이터 조건 EqpType 로직 변경 (공통코드 + 동별 공통코드 사용)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_017.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_017 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        public COM001_017()
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
            CommonCombo_Form _combo = new CommonCombo_Form();

            #region 공통코드 + 동별 공통코드 기준정보에서 EQPTTYPE 정보 사용

            string sEqpType = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CMCODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_EQP_LOSS_PROC_GR_CODE";

            RQSTDT.Rows.Add(dr);

            DataTable dtCommonCode = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_FORM_CBO", "RQSTDT", "RSLTDT", RQSTDT); // DA 변경

            if(dtCommonCode.Rows.Count > 0)
            {
                foreach(DataRow drCommonCode in dtCommonCode.Rows)
                {
                    sEqpType += drCommonCode["CBO_NAME"].ToString().Split(':')[1].Trim() + ",";
                }
            }

            RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

            DataRow dr2 = RQSTDT.NewRow();
            dr2["LANGID"] = LoginInfo.LANGID;
            dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr2["COM_TYPE_CODE"] = "FORM_EQP_LOSS_PROC_GR_CODE";

            RQSTDT.Rows.Add(dr2);

            DataTable dtAreaCommonCode = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT); // DA 변경

            if (dtAreaCommonCode.Rows.Count > 0)
            {
                foreach (DataRow drAreaCommonCode in dtAreaCommonCode.Rows)
                {
                    sEqpType += drAreaCommonCode["CBO_NAME"].ToString().Trim() + ",";
                }
            }

            #endregion

            //string[] sFilterEqpType = { "DEG,EOL" };
            string[] sFilterEqpType = { sEqpType.Substring(0,sEqpType.Length-1) };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "EQUIPMENTGROUP", sFilter: sFilterEqpType);

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

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_DETAIL_GMES", "RQSTDT", "RSLTDT", RQSTDT);

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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_KEYIN_RAW_GMES", "INDATA", "OUTDATA", inDataTable);

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
