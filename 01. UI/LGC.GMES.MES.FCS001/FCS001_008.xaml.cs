/*************************************************************************************
 Created Date : 2020.11.17
      Creator : 
   Decription : 전용OCV 호기별 출고 예정 수량
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.17  DEVELOPER : CREATED
  2023.01.01 형준우 : ESWA#1 OCV 설비레벨이 C 로 되어있어, 해당 사항 대응으로 동별공통코드를 활용하여 Machine 설비를 가져오도록 수정
  2024.01.04 조영대 : Machine 콤보 설정시 설비에 대한 조건 불일치로 조회 안되는 현상 수정.
  2024.11.27 이지은 : 더블클릭시 MAIN_EQPTID로 파라미터 던져서 트레이 조회하도록 수정.
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_008 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        bool bEqpMachineUseFlag = false;
        Util _Util = new Util();

        public FCS001_008()
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

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            bEqpMachineUseFlag = _Util.IsAreaCommonCodeUse("EQP_LEVEL_TYPE", "MACHINE");

            if (bEqpMachineUseFlag)
            {
                string[] sFilterM = { "8", null, "C" };
                _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilterM);
            }
            else
            {
                string[] sFilter = { "8", null, "M" };
                _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilter);
            }

            string[] sFilter1 = { null, "8", null, "81" };
            _combo.SetCombo(cboOpId, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP", sFilter: sFilter1);
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("EQPTID_MACHINE", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (bEqpMachineUseFlag)
                {
                    dr["EQPTID_MACHINE"] = Util.GetCondition(cboEqp, bAllNull: true);
                }
                else
                {
                    dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                }                    
                dr["PROCID"] = Util.GetCondition(cboOpId, bAllNull: true);
                dtRqst.Rows.Add(dr);

                // 백그라운드 실행, 실행 후 dgEqpStatus_ExecuteDataCompleted
                dgEqpStatus.ExecuteService("DA_SEL_AGING_UNLOAD_TRAY_CNT_BY_OCV", "RQSTDT", "RSLTDT", dtRqst);
             
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();

        }

        private void dgEqpStatus_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            string[] sColumnName = new string[] { "EQPTNAME" };
            _Util.SetDataGridMergeExtensionCol(dgEqpStatus, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
        }

        private void dgEqpStatus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (cell.Text != datagrid.CurrentColumn.Header.ToString() && datagrid.CurrentRow != null && datagrid.CurrentColumn.Name.Equals("TRAY_CNT"))
                {

                    C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                    FCS001_005_02 TrayList = new FCS001_005_02();
                    TrayList.FrameOperation = FrameOperation;

                    object[] Parameters = new object[19];
                    Parameters[0] = string.Empty; //sOPER
                    Parameters[1] = ""; //sOPER_NAME
                    Parameters[2] = ""; //sLINE_ID
                    Parameters[3] = ""; //sLINE_NAME
                    Parameters[4] = ""; //sROUTE_ID
                    Parameters[5] = ""; //sROUTE_NAME
                    Parameters[6] = ""; //sMODEL_ID
                    Parameters[7] = ""; //sMODEL_NAME
                    Parameters[8] = "S"; //sStatus
                    Parameters[9] = ObjectDic.Instance.GetObjectName("WORK_TRAY"); //sStatusName
                    Parameters[10] = ""; //sROUTE_TYPE_DG
                    Parameters[11] = ""; //sROUTE_TYPE_DG_NAME
                    Parameters[12] = ""; //sLotID
                    Parameters[13] = ""; //sSPECIAL_YN
                    Parameters[14] = DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "MAIN_EQPTID").ToString(); //_sAgingEqpID
                    Parameters[15] = Util.NVC(DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "EQPTNAME")); //_sAgingEqpName

                    string _sDate = Util.NVC(DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "AGING_ISS_SCHD_DTTM"));
                    if (!string.IsNullOrEmpty(_sDate))
                        Parameters[16] = _sDate; //_sOpPlanTime
                    else Parameters[16] = "";

                    Parameters[17] = "";
                    Parameters[18] = "";

                    this.FrameOperation.OpenMenuFORM("SFU010705052", "FCS001_005_02", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqpStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("TRAY_CNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }
        #endregion
    }
}
