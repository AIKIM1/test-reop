/*************************************************************************************
 Created Date : 2017.11.21
      Creator : 이영준
   Decription : 포장 셀 리스트 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.21  이영준 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.IO;
using C1.WPF.DataGrid.Excel;
using C1.WPF.Excel;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_100.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_206 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _util = new Util();
        string _paramPallets = string.Empty;
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

        public BOX001_206()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

            var dr = dt.NewRow();
            dr["CBO_CODE"] = "2003";
            dr["CBO_NAME"] = "Excel File 97-2003 (*.xls)";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["CBO_CODE"] = "2007";
            dr["CBO_NAME"] = "Excel File (.xlsx)";
            dt.Rows.InsertAt(dr, 0);

            cboVersion.DisplayMemberPath = "CBO_NAME";
            cboVersion.SelectedValuePath = "CBO_CODE";
            cboVersion.ItemsSource = DataTableConverter.Convert(dt);

            cboVersion.SelectedIndex = 0;
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            chkDSF.IsChecked = false;
            chkForm.IsChecked = false;
            chkKValue.IsChecked = false;

            InitDataGrid();
        }
        private void InitDataGrid()
        {
            dgCellList.Columns["DSF_OP_TIME"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["DSF_VOLT_VAL"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["DSF_OCV_VAL"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["DSF_IMP_VAL"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["OD_GRADE_CD"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["DSF_VOLT_JUDGE"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["LAST_DCHG_CAPA_ENDTIME"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["LAST_DCHG_CAPA_VAL"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["LAST_KVAL_ENDTIME"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["LAST_KVAL"].Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;
                _paramPallets = ary.GetValue(0).ToString();
                Search();
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _paramPallets = string.Empty;
            Search();
        }

        private void Search()
        {
            string sPalletID = string.IsNullOrEmpty(_paramPallets) ? Util.NVC(txtPalletID.Text.Trim()) : _paramPallets.Split(',')[0].Trim();
            SetHeader(sPalletID);
            DataBind();
        }
        private void SetHeader(string palletID)
        {
            try
            {
                const string bizRule = "DA_PRD_SEL_CELL_MEASR_HEADER_NJ";
                DataTable dt = new DataTable();
                dt.Columns.Add("BOXID");
                DataRow dr = dt.NewRow();
                dr["BOXID"] = palletID;
                dt.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRule, "RQSTDT", "RSLTDT", dt);
                if (dtResult == null)
                {
                    //"검사 데이터가 없습니다."
                    return;
                }
                else
                {
                    if (dtResult.Rows.Count < 1)
                    {

                    }
                    else
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            if (dtResult.Rows[i]["MEASR_ITEM_ID"].Equals("DSF_OP_TIME") || dtResult.Rows[i]["MEASR_ITEM_ID"].Equals("DSF_VOLT_VAL"))
                            {
                                chkIV.IsChecked = true;
                            }
                            else if (dtResult.Rows[i]["MEASR_ITEM_ID"].Equals("DSF_OCV_VAL"))
                            {
                                chkOCV.IsChecked = true;
                            }
                            else if (dtResult.Rows[i]["MEASR_ITEM_ID"].Equals("DSF_IMP_VAL"))
                            {
                                chkIR.IsChecked = true;
                            }
                            else if (dtResult.Rows[i]["MEASR_ITEM_ID"].Equals("LAST_DCHG_CAPA_ENDTIME") || dtResult.Rows[i]["MEASR_ITEM_ID"].Equals("LAST_DCHG_CAPA_VAL"))
                            {
                                chkForm.IsChecked = true;
                            }
                            else if (dtResult.Rows[i]["MEASR_ITEM_ID"].Equals("LAST_KVAL_ENDTIME") || dtResult.Rows[i]["MEASR_ITEM_ID"].Equals("LAST_KVAL"))
                            {
                                chkKValue.IsChecked = true;
                            }
                        }
                        if (chkIV.IsChecked == true && chkOCV.IsChecked == true && chkIR.IsChecked == true && chkAOMM.IsChecked == true)
                        {
                            chkDSF.IsChecked = true;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataBind()
        {
            const string bizRule = "DA_PRD_SEL_CELL_MEASR_DATA_NJ";
            DataTable dt = new DataTable();
            dt.Columns.Add("BOXID");
            dt.Columns.Add("PKG_LOTID");
            dt.Columns.Add("SUBLOTID");
            DataRow dr = dt.NewRow();
            dr["BOXID"] = string.IsNullOrEmpty(_paramPallets) ? Util.NVC(txtPalletID.Text.Trim()) : _paramPallets;
            dr["PKG_LOTID"] = string.IsNullOrEmpty(txtAssyLotID.Text.Trim()) ? null : txtAssyLotID.Text.Trim();
            dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID.Text.Trim()) ? null : txtCellID.Text.Trim();
            dt.Rows.Add(dr);
            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dt, (result, ex) =>
              {
                  loadingIndicator.Visibility = Visibility.Collapsed;
                  if (ex!=null)
                  {
                      Util.MessageException(ex);
                      return;
                  }
                  if(result==null||result.Rows.Count<1)
                  {
                      Util.MessageInfo("SFU2816");
                  }
                  Util.GridSetData(dgCellList, result, FrameOperation, true);
                  Util.GridSetData(dgCellList_SMP, result, FrameOperation, true);

              });
        }
        #endregion

        #region Mehod

        #region [BizCall]


        #endregion


        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #endregion



        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellList.GetRowCount() == 0)
                {
                    //SFU1498	데이터가 없습니다.
                    Util.MessageValidation("SFU1498");
                    return;
                }
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgCellList, cboVersion.SelectedValue.ToString() == "2003" ? ExcelFileFormat.Xls : ExcelFileFormat.Xlsx, dgCellList_Copy);                
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void btnExport_SMP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellList_SMP.GetRowCount() == 0)
                {
                    //SFU1498	데이터가 없습니다.
                    Util.MessageValidation("SFU1498");
                    return;
                }
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgCellList_SMP, cboVersion.SelectedValue.ToString() == "2003" ? ExcelFileFormat.Xls : ExcelFileFormat.Xlsx,dgCellList_SMP_Copy);
              
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #region CheckBox 이벤트

        private void chkDSF_Checked(object sender, RoutedEventArgs e)
        {
            chkIV.IsChecked = true;
            chkOCV.IsChecked = true;
            chkIR.IsChecked = true;
            chkAOMM.IsChecked = true;
        }

        private void chkDSF_Unchecked(object sender, RoutedEventArgs e)
        {
            chkIV.IsChecked = false;
            chkOCV.IsChecked = false;
            chkIR.IsChecked = false;
            chkAOMM.IsChecked = false;
        }

        private void chkIV_Checked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["DSF_VOLT_VAL"].Visibility = Visibility.Visible;
            dgCellList.Columns["DSF_OP_TIME"].Visibility = Visibility.Visible;
        }

        private void chkIV_Unchecked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["DSF_VOLT_VAL"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["DSF_OP_TIME"].Visibility = Visibility.Collapsed;
        }

        private void chkOCV_Checked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["DSF_OCV_VAL"].Visibility = Visibility.Visible;
        }

        private void chkOCV_Unchecked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["DSF_OCV_VAL"].Visibility = Visibility.Collapsed;

        }

        private void chkIR_Checked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["DSF_IMP_VAL"].Visibility = Visibility.Visible;
            dgCellList.Columns["DSF_VOLT_JUDGE"].Visibility = Visibility.Visible;


        }

        private void chkIR_Unchecked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["DSF_IMP_VAL"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["DSF_VOLT_JUDGE"].Visibility = Visibility.Collapsed;
        }

        private void chkForm_Checked(object sender, RoutedEventArgs e)
        {
            chkCapa.IsChecked = true;
        }

        private void chkForm_Unchecked(object sender, RoutedEventArgs e)
        {
            chkCapa.IsChecked = false;
        }
        private void chkCapa_Checked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["LAST_DCHG_CAPA_VAL"].Visibility = Visibility.Visible;
            dgCellList.Columns["LAST_DCHG_CAPA_ENDTIME"].Visibility = Visibility.Visible;

        }

        private void chkCapa_Unchecked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["LAST_DCHG_CAPA_VAL"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["LAST_DCHG_CAPA_ENDTIME"].Visibility = Visibility.Collapsed;
        }

        private void chkKValue_Checked(object sender, RoutedEventArgs e)
        {
            chkKValue2.IsChecked = true;
        }

        private void chkKValue_Unchecked(object sender, RoutedEventArgs e)
        {
            chkKValue2.IsChecked = false;
        }

        private void chkKValue2_Checked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["LAST_KVAL"].Visibility = Visibility.Visible;
            dgCellList.Columns["LAST_KVAL_ENDTIME"].Visibility = Visibility.Visible;
        }

        private void chkKValue2_Unchecked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["LAST_KVAL"].Visibility = Visibility.Collapsed;
            dgCellList.Columns["LAST_KVAL_ENDTIME"].Visibility = Visibility.Collapsed;

        }


        #endregion

        private void previewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void chkAOMM_Checked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["OD_GRADE_CD"].Visibility = Visibility.Visible;
        }

        private void chkAOMM_Unchecked(object sender, RoutedEventArgs e)
        {
            dgCellList.Columns["OD_GRADE_CD"].Visibility = Visibility.Collapsed;
        }
    }
}
