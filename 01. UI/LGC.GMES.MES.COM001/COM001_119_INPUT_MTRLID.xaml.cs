/*************************************************************************************
 Created Date : 2017.11.03
      Creator : 
   Decription : 투입자재 선택
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.03  정규환 : Initial Created.


**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_119_INPUT_MTRLID : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private object[] mtrlId = null;

        private string _retWoId = string.Empty;         // WorkOrder
        private string _retProdId = string.Empty;       // 제품
        private string _retProdName = string.Empty;     // 제품명
        private string _retProdUnitCode = string.Empty; // 제품단위
        private string _retIssSlocId = string.Empty;    // 투입자재 저장위치
        private string _retSlocId = string.Empty;       // 제품 저장위치
        private string _retRsvItemNo = string.Empty;    // 투입자재 변경차수
        private string _retRoutNo = string.Empty;       // 제품 라우트
        private string _retUseFlag = string.Empty;      // 사용여부

        public string WO_ID
        {
            get { return _retWoId; }
        }
        public string PROD_ID
        {
            get { return _retProdId; }
        }
        public string PROD_NAME
        {
            get { return _retProdName; }
        }
        public string PROD_UNIT_CODE
        {
            get { return _retProdUnitCode; }
        }

        public string ISS_SLOC_ID
        {
            get { return _retIssSlocId; }
        }
        public string SLOC_ID
        {
            get { return _retSlocId; }
        }

        public string RSV_ITEM_NO
        {
            get { return _retRsvItemNo; }
        }
        public string ROUT_NO
        {
            get { return _retRoutNo; }
        }

        public string USE_FLAG
        {
            get { return _retUseFlag; }
        }

        public COM001_119_INPUT_MTRLID()
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

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            mtrlId = C1WindowExtension.GetParameters(this);

            if (mtrlId == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            // 넘겨 받은 자재id 및 W/O 출력
            txtMtrlId.Text = Util.NVC(mtrlId[0]);
            txtWoId.Text = Util.NVC(mtrlId[1]);

            InitCombo();
            SearchData();
        }
        #endregion

        #region Event
        private void InitCombo()
        {
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectData();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            selectCurrentWo();
        }

        private void selectCurrentWo()
        {
            for (int i = 0; i < dgList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WOID")).Equals(txtWoId.Text.Trim().ToUpper()))
                {
                    DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
                    dgList.ScrollIntoView(i, 0);
                    return;
                }
            }
        }

        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgList);

                DataTable IndataTable = new DataTable("INDATA");

                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));
                IndataTable.Columns.Add("RSV_ITEM_NO", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["MTRLID"] = txtMtrlId.Text; ;

                //if (!string.IsNullOrWhiteSpace(txtWoId.Text))
                    //Indata["WOID"] = txtWoId.Text.Trim().ToUpper();

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("BR_PRD_GET_MATERIAL_WORKORDER", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        return;
                    }

                    Util.GridSetData(dgList, result, FrameOperation, true);
                    if (!String.IsNullOrWhiteSpace(txtWoId.Text))
                    {
                        selectCurrentWo();
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void SelectData()
        {

            int previewrowIdx = _Util.GetDataGridFirstRowIndexByCheck(dgList, "CHK", true);

            if (previewrowIdx < 0)
                Util.Alert("SFU1635");  //선택된 W/O가 없습니다.
            else
            {
                _retWoId = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "WOID"));
                _retProdId = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "PRODID"));
                _retProdName = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "PRODNAME"));
                _retProdUnitCode = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "PROD_UNIT_CODE"));
                _retIssSlocId = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "ISS_SLOC_ID"));
                _retSlocId = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "SLOC_ID"));
                _retRsvItemNo = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "RSV_ITEM_NO"));
                _retRoutNo = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "ROUT_NO"));
                _retUseFlag = Util.NVC(DataTableConverter.GetValue(dgList.Rows[previewrowIdx].DataItem, "USEFLAG"));

                this.DialogResult = MessageBoxResult.OK;
            }
        }
        #endregion

        #region [작업대상 목록 에서 선택]
        private void dgListChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            if (rb?.DataContext == null) return;
            if (rb.IsChecked == null) return;


            DataRowView drv = rb.DataContext as DataRowView;
            if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                }
                //row 색 바꾸기
                dgList.SelectedIndex = idx;
            }
        }
        #endregion
    }
}
