/*************************************************************************************
 Created Date : 2021.01.
      Creator : 
   Decription : 부적합율 실적조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.01. DEVELOPER : Initial Created.
  2022.02.22  KDH : AREA 조건 추가
  2023.08.31  조영대 : 값 범위 증가로 인한 오류 처리. Convert.ToInt16 ==> Convert.ToInt32 로 수정
  2023.10.22  이의철 : ADD J_GR_RATE FOR XAML
  2025.01.15  최도훈 : 비율(%) 0일때 빨간색으로 표시되는 오류 수정
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Collections;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_050 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_050()
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

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);


            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);
        }
        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now;
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string)); //2022.02.22_AREA 조건 추가
                dtRqst.Columns.Add("AREAID", typeof(string)); //2022.02.22_AREA 조건 추가

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                dr["LANGID"] = LoginInfo.LANGID; //2022.02.22_AREA 조건 추가
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.22_AREA 조건 추가
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_JUDG_NG", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgList, dtRslt, this.FrameOperation, true);
                string[] sColumnName = new string[] { "SEL_MODEL", "SEL_ROUTE" };

                _Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                // Summary Row - 합계, 비율
                AddRow(dgList, dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void AddRow(C1.WPF.DataGrid.C1DataGrid datagrid, DataTable dt)
        {
            DataTable preTable = DataTableConverter.Convert(datagrid.ItemsSource);

            if (preTable.Columns.Count == 0)
            {
                preTable = new DataTable();
                foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
                {
                    preTable.Columns.Add(Convert.ToString(col.Name));
                }
            }
            int init = dgList.Columns["SEL_IN_CNT"].Index;
            int iSumFromCol = dgList.Columns["SEL_NG_CNT"].Index;
            int iSumToCol = dgList.Columns["W_GR_CNT"].Index;
            int iTotalSumQty = 0;
            int iSumQty = 0;
            string ColName = string.Empty;

            DataRow row = preTable.NewRow();
            row["SEL_MODEL"] = string.Empty;
            row["SEL_ROUTE"] = string.Empty;
            row["JUDG_OP_ID"] = ObjectDic.Instance.GetObjectName("ToTal");

            //Total 수량
            for (int iCol = init; iCol < iSumFromCol; iCol++)
            {
                ColName = dgList.Columns[iCol].Name;

                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    iSumQty = Convert.ToInt32(Util.NVC(dt.Rows[iRow][ColName]));
                    iTotalSumQty = iTotalSumQty + iSumQty;
                }

                row[ColName] = Convert.ToString(iTotalSumQty);

                iTotalSumQty = 0;
                iSumQty = 0;
            }
            for (int iCol = iSumFromCol; iCol <= iSumToCol; iCol = iCol + 2)
            {
                ColName = dgList.Columns[iCol].Name;

                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    iSumQty = Convert.ToInt32(Util.NVC(dt.Rows[iRow][ColName]));
                    iTotalSumQty = iTotalSumQty + iSumQty;
                }

                row[ColName] = Convert.ToString(iTotalSumQty);

                iTotalSumQty = 0;
                iSumQty = 0;
            }
            //Total 비율
            int iInputCol = dgList.Columns["SEL_IN_CNT"].Index;
            int totalInput = Util.NVC_Int(row["SEL_IN_CNT"]); //전체 input 수량
            int iPerFromCol = dgList.Columns["SEL_NG_RATE"].Index;
            int iPerToCol = dgList.Columns["W_GR_RATE"].Index;
            for (int i = iPerFromCol; i <= iPerToCol; i = i + 2)
            {
                string colGrade = dgList.Columns[i - 1].Name;
                string colPer = dgList.Columns[i].Name;
                //등급별 불량 수량
                int totalNG = Util.NVC_Int(row[colGrade]);
                //등급별 불량률
                if (totalInput == 0) continue;
                else if (totalNG == 0) row[colPer] = 0.00;
                else
                {
                    double per = ((double)totalNG / (double)totalInput) * 100;
                    row[colPer] = per;
                }
            }
            preTable.Rows.Add(row);

            Util.GridSetData(datagrid, preTable, FrameOperation, true);
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
        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Index == dgList.Rows.Count - 1)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                }
                if (e.Cell.Column.Name.Contains("RATE") && e.Cell.Value != null && e.Cell.Row.Index > 1)
                {
                    /*if (!e.Cell.Value.ToString().Equals("0"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }*/
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                    string s = Util.NVC(e.Cell.Value);
                    if (double.Parse(s) >= 1.00)
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                }
            }));
        }
        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            if (e.Row.Index > 1 && e.Row.Index != dgList.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dgList.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }
    }
}
