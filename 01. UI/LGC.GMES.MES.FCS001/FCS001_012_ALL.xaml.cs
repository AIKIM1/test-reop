/*************************************************************************************
 Created Date : 2020.11.03
      Creator : KANG DONG HEE
   Decription : 전체 Aging Rack 현황
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2020.09.10  DEVELOPER : Initial Created.
  2021.03.31        KDH : 전체 Aging Rack 현황 조회 및 출력 로직 변경
  2021.04.13        KDH : 화면간 이동 시 초기화 현상 제거
  2022.02.08        KDH : S/C 호기 컬럼 추가
  2022.06.29     이정미 : 콤보박스 수정 
  2022.11.22     이정미 : cboAgingType 콤보박스 수정
  2023.11.04     조영대 : 라인 컬럼 추가
**************************************************************************************/
#define SAMPLE_DEV

using C1.WPF;
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
    public partial class FCS001_012_ALL : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public FCS001_012_ALL()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();

            if (IsCommonCode("FORM_AGING_LINE_GROUP"))
            {
                dgAgingStatus.Columns["EQSG_NAME"].Visibility = System.Windows.Visibility.Visible;
                dgAgingStatus.Columns["ROW"].Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                dgAgingStatus.Columns["EQSG_NAME"].Visibility = System.Windows.Visibility.Collapsed;
                dgAgingStatus.Columns["ROW"].Visibility = System.Windows.Visibility.Visible;
            }

            dgAgingStatus.UserConfigExceptColumns.Add("EQSG_NAME");
            dgAgingStatus.UserConfigExceptColumns.Add("EQPT_NAME");
            dgAgingStatus.UserConfigExceptColumns.Add("ROW");

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cbChild = { cboAgingType };
            ComCombo.SetCombo(cboBldgCd, CommonCombo_Form.ComboStatus.ALL, sCase: "BLDG", cbChild: cbChild);
            object[] objParent = { "FORM_AGING_TYPE_CODE","N" };
            ComCombo.SetComboObjParent(cboAgingType, CommonCombo_Form.ComboStatus.ALL, sCase: "SYSTEM_AREA_COMMON_CODE", objParent: objParent);
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void chkOnlyAll_Checked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void chkOnlyAll_Unchecked(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void dgAgingStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    string sROW = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ROW"));
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    //e.Cell.Presenter.Cursor = Cursors.Arrow;

                    //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    //e.Cell.Presenter.FontSize = 12;
                    //e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (!chkOnlyAll.IsChecked.Equals(true) && sROW.Equals("ALL"))
                    {
                        //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                        //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgAgingStatus_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;

            DataView dvResult = dtResult.DefaultView;
            dvResult.Sort = "FLOOR_CODE,AGING_TYPE,EQSGID";
            dtResult = dvResult.ToTable();
        }

        private void dgAgingStatus_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                if (chkOnlyAll.IsChecked.Equals(true))
                {
                    dgAgingStatus.Columns["EQPT_NAME"].Visibility = Visibility.Collapsed; //20220208_S/C 호기 컬럼 추가
                }
                else
                {
                    dgAgingStatus.Columns["EQPT_NAME"].Visibility = Visibility.Visible; //20220208_S/C 호기 컬럼 추가
                }

                btnSearch.IsEnabled = true;
                chkOnlyAll.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        private bool IsCommonCode(string sCodeType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0) return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex); 
            }

            return false;
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AGING_TYPE", typeof(string));
                dtRqst.Columns.Add("ONLY_ALL_YN", typeof(string));
                dtRqst.Columns.Add("BLDG_CD", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AGING_TYPE"] = Util.GetCondition(cboAgingType, bAllNull: true);
                dr["ONLY_ALL_YN"] = ((bool)chkOnlyAll.IsChecked) ? "Y" : "N";
                dr["BLDG_CD"] = Util.GetCondition(cboBldgCd, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                
                btnSearch.IsEnabled = false;
                chkOnlyAll.IsEnabled = false;
                                
                dgAgingStatus.SummaryGroupColumns.Clear();
                
                if (IsCommonCode("FORM_AGING_LINE_GROUP"))
                {
                    dgAgingStatus.Columns["ROW"].Visibility = System.Windows.Visibility.Collapsed;

                    if (chkOnlyAll.IsChecked.Equals(true))
                    {
                        dgAgingStatus.SummaryGroupColumns.Add("FLOOR_NAME");
                    }
                    else
                    {
                        dgAgingStatus.SummaryGroupColumns.Add("FLOOR_NAME");
                        dgAgingStatus.SummaryGroupColumns.Add("AGING_TYPE_NAME");
                        dgAgingStatus.SummaryGroupColumns.Add("EQSG_NAME");
                    }
                }
                else
                {
                    dgAgingStatus.Columns["EQSG_NAME"].Visibility = System.Windows.Visibility.Collapsed;

                    if (chkOnlyAll.IsChecked.Equals(true))
                    {
                        dgAgingStatus.SummaryGroupColumns.Add("FLOOR_NAME");

                        dgAgingStatus.Columns["ROW"].Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        dgAgingStatus.SummaryGroupColumns.Add("FLOOR_NAME");
                        dgAgingStatus.SummaryGroupColumns.Add("AGING_TYPE_NAME");

                        dgAgingStatus.Columns["ROW"].Visibility = System.Windows.Visibility.Visible;
                    }
                }
              
                // 백그라운드로 실행, 실행 완료 후 dgAgingStatus_ExecuteDataCompleted 이벤트 실행
                dgAgingStatus.ExecuteService("BR_GET_AGING_RACK_STATUS_VIEW", "INDATA", "OUTDATA", dtRqst, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
