/*************************************************************************************
 Created Date : 2017.09.20
      Creator : 
   Decription : 등외품 Lot, 제품 정보
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_OFFGRD_LOT_TYPE : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        ////private string _procID = string.Empty;        // 공정코드
        ////private string _eqptID = string.Empty;        // 설비코드

        public string Offgrd_LotTypeCode { get; set; }
        public string Offgrd_PrdtSffx { get; set; }

        private bool _load = true;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FORM001_OFFGRD_LOT_TYPE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                _load = false;
            }

        }
        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            ////object[] tmps = C1WindowExtension.GetParameters(this);

            ////_procID = tmps[0] as string;
            ////_eqptID = tmps[1] as string;

            SetOffGradeGrid();
        }

        #endregion

        #region [선택하기]
        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTable dt = DataTableConverter.Convert(dgOffGrade.ItemsSource);

                // 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.Rows[idx]["CHK"] = 1;
                dt.AcceptChanges();

                Util.GridSetData(dgOffGrade, dt, null, true);

                //row 색 바꾸기
                dgOffGrade.SelectedIndex = idx;
            }

        }
        #endregion

        #region [선택]
        /// <summary>
        /// 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgOffGrade, "CHK");

            if (rowIndex < 0)
            {
                // 등외 제품을 선택 하세요.
                Util.MessageValidation("SFU4121");
                return;
            }

            Offgrd_LotTypeCode = Util.NVC(DataTableConverter.GetValue(dgOffGrade.Rows[rowIndex].DataItem, "OFFGRD_LOT_TYPE_CODE"));
            Offgrd_PrdtSffx = Util.NVC(DataTableConverter.GetValue(dgOffGrade.Rows[rowIndex].DataItem, "OFFGRD_PRDT_SFFX"));

            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region [닫기]
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        /// <summary>
        /// 등외품 Lot, 제품 조회
        /// </summary>
        private void SetOffGradeGrid()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_OFFGRD_LOT_PRDT_CODE", "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgOffGrade, dtResult, null, true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        #endregion

        #region [Func]
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

        #endregion



    }
}
