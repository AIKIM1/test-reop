/*************************************************************************************
 Created Date : 2016.11.16
      Creator : LG CNS 이슬아
   Decription : 믹서실적비교
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.16  이슬아 : 최초 생성
  2021.07.15  김지은 : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
  2025.02.24  이민형 : 날짜 함수 Util.GetConfition 으로 형변환 함수 변경



 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_044 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private const string _BizRule = "DA_PRD_SEL_BTCH_ORD_SUMMARY";
        private readonly List<int> _MergeIndexList = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_044()
        {
            InitializeComponent();         
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Event

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }
   
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetCboEquipment();
        }
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgResult);
        }
        private void dgResult_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            List<DataGridCellsRange> mergeList = new List<DataGridCellsRange>();

            if (grid.Rows.Count > 0)
            {              
                mergeList = AddMergeList(mergeList, grid);

                foreach (var range in mergeList)
                {
                    e.Merge(range);
                }
            }
        }

        #endregion

        #region Mehod

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] { cboLine }, sFilter: new string[] { LoginInfo.CFG_SHOP_ID });
            //라인
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.NONE, cbParent: new C1ComboBox[] { cboArea }, sFilter: new string[] { LoginInfo.CFG_EQSG_ID });

            //생산구분
            string[]  sFilter = new string[] { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;
        }

        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
        }
        
        private void SetCboEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboLine.SelectedValue;
                dr["PROCID"] = Process.MIXING;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";
                cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);
                cboEquipment.isAllUsed = true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void Search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID_LIST", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("BTCH_ID", typeof(string));
                //RQSTDT.Columns.Add("NORMAL", typeof(string));
                //RQSTDT.Columns.Add("PILOT", typeof(string));
                RQSTDT.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["EQSGID"] = cboLine.SelectedValue;
                if (!string.IsNullOrWhiteSpace(cboEquipment.SelectedItemsToString))
                    dr["EQPTID_LIST"] = cboEquipment.SelectedItemsToString;
                if (!string.IsNullOrWhiteSpace(txtProductID.Text))
                    dr["PRODID"] = txtProductID.Text;
                if (!string.IsNullOrWhiteSpace(txtBatchID.Text))
                    dr["BTCH_ID"] = txtBatchID.Text;

                // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
                //if (cboProductDiv.SelectedValue.ToString() == "P")
                //    dr["NORMAL"] = cboProductDiv.SelectedValue.ToString();
                //else if (cboProductDiv.SelectedValue.ToString() == "X")
                //    dr["PILOT"] = cboProductDiv.SelectedValue.ToString();
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(_BizRule, "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgResult, dtResult, FrameOperation, true);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private List<DataGridCellsRange> AddMergeList(List<DataGridCellsRange> mergeList, C1DataGrid grid)
        {
            int sRow = 0;
            int eRow = grid.Rows.Count;
            int col = _MergeIndexList.First();

            mergeList = AddMergeList(mergeList, grid, sRow, eRow, col);
            //for (int row = sRow; row < grid.Rows.Count; row++)
            //{
            //    if (keyWord != grid.GetCell(row, col).Text)
            //    {
            //        mergeList.Add(new DataGridCellsRange(grid.GetCell(sRow, col), grid.GetCell(row - 1, col)));

            //        if (_MergeIndexList.IndexOf(col) != _MergeIndexList.Last())
            //        {
            //            mergeList = AddMergeList(mergeList, grid, sRow, row - 1, col);
            //        }
            //    }
            //}            

            return mergeList;
        }
        
        private List<DataGridCellsRange> AddMergeList(List<DataGridCellsRange> mergeList, C1DataGrid grid, int sRow, int eRow, int col)
        {
            int tRow = sRow;
            string keyWord = grid.GetCell(tRow, col).Text;      
            for (int row = tRow; row < eRow; row++)
            {
                if (keyWord != grid.GetCell(row, col).Text)
                {
                    if (sRow != row - 1)
                    {
                        mergeList.Add(new DataGridCellsRange(grid.GetCell(tRow, col), grid.GetCell(row - 1, col)));

                        if (tRow != row - 1 && _MergeIndexList.IndexOf(col) < _MergeIndexList.Last())
                        {
                            mergeList = AddMergeList(mergeList, grid, tRow, row, _MergeIndexList.IndexOf(col) + 1);
                        }
                    }
                    tRow = row;
                    keyWord = grid.GetCell(tRow, col).Text;
                }
            }
            //mergeList = AddMergeList(mergeList, grid, sRow, eRow - 1, col);
            if(tRow != eRow-1)
                mergeList.Add(new DataGridCellsRange(grid.GetCell(tRow, col), grid.GetCell(eRow - 1, col)));

            if (_MergeIndexList.IndexOf(col) <_MergeIndexList.Last())
            {
                mergeList = AddMergeList(mergeList, grid, tRow, eRow, _MergeIndexList.IndexOf(col) + 1);
            }

            return mergeList;
        }


        #endregion


    }
}
