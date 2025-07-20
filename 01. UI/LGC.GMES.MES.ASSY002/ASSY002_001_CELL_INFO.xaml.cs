/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;


namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_001_CELL_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public ASSY002_001_CELL_INFO()
        {
            InitializeComponent();
            this.Loaded += ASSY002_001_CELL_INFO_Loaded;
            dgCell.LoadedRowHeaderPresenter += DgCell_LoadedRowHeaderPresenter;
        }

        private void DgCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            _Util.gridLoadedRowHeaderPresenter(sender, e);
        }

        private void ASSY002_001_CELL_INFO_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);
            string sLotID = tmps[0] as string;
            string sTrayID = tmps[1] as string;
            string sCellQty = tmps[2] as string;

            txtLotID.Text = sLotID;
            txtTrayID.Text = sTrayID;
            txtCellQty.Text = "1024";


            SetCombo(cboCellYN);
            SetCombo(cboCellYN01);
            SetCombo(cboCellYN02);
            SetCombo(cboCellYN03);
            SetCombo(cboCellYN04);
            SetCombo(cboCellYN05);
            SetCombo(cboCellYN06);
            SetCombo(cboCellYN07);
            SetCombo(cboCellYN08);
            SetCombo(cboCellYN09);
            SetCombo(cboCellYN10);
            SetCombo(cboCellYN11);
            SetCombo(cboCellYN12);
            SetCombo(cboCellYN13);
            SetCombo(cboCellYN14);
            SetCombo(cboCellYN15);
            SetCombo(cboCellYN16);
            SetCombo(cboCellYN17);
            SetCombo(cboCellYN18);
            SetCombo(cboCellYN19);
            SetCombo(cboCellYN20);
            SetCombo(cboCellYN21);
            SetCombo(cboCellYN22);
            SetCombo(cboCellYN23);
            SetCombo(cboCellYN24);
            SetCombo(cboCellYN25);
            SetCombo(cboCellYN26);
            SetCombo(cboCellYN27);
            SetCombo(cboCellYN28);
            SetCombo(cboCellYN29);
            SetCombo(cboCellYN30);
            SetCombo(cboCellYN31);
            SetCombo(cboCellYN32);

            btnSearch_Click(null, null);

        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PLANT", typeof(string));
                RQSTDT.Columns.Add("LINE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PLANT"] = "";
                dr["LINE"] = "";
                RQSTDT.Rows.Add(dr);


                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_SEL_TRAY_CELL", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgCell.ItemsSource = DataTableConverter.Convert(result);
                    //==========================================================================================================
                    Dictionary<int, System.Windows.Media.Brush> rowborderInfo = new Dictionary<int, System.Windows.Media.Brush>();

                    System.Windows.Media.Brush rowborderColor;

                    for(int i = 8; i < 32; i = i + 8)
                    {
                        rowborderColor = new SolidColorBrush(Colors.SteelBlue);
                        rowborderInfo.Add(i, rowborderColor);
                    }

                    if (dgCell.Resources.Contains("RowBorderInfo"))
                    {
                        dgCell.Resources.Remove("RowBorderInfo");
                    }

                    dgCell.Resources.Add("RowBorderInfo", rowborderInfo);
                    //==========================================================================================================
                    Dictionary<int, System.Windows.Media.Brush> colborderInfo = new Dictionary<int, System.Windows.Media.Brush>();

                    System.Windows.Media.Brush colborderColor;

                    for (int i = 7; i < 32; i = i + 8)
                    {
                        colborderColor = new SolidColorBrush(Colors.SteelBlue);
                        colborderInfo.Add(i, colborderColor);
                    }

                    if (dgCell.Resources.Contains("ColBorderInfo"))
                    {
                        dgCell.Resources.Remove("ColBorderInfo");
                    }

                    dgCell.Resources.Add("ColBorderInfo", colborderInfo);
                    //==========================================================================================================


                });

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_CELL", "RQSTDT", "RSLTDT", RQSTDT);
                //dgCell.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// 선택된 값으로 콤보 초기화 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (chkALL0.IsChecked == true)
            {
                initClear("0");
            }
            else
            {
                initClear("1");
            }
        }

        /// <summary>
        /// cell 선택시 컬럼값을 콤보에 Set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCell_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (dgCell.CurrentRow == null || dgCell.SelectedIndex == -1) return;

            chkALL0.IsChecked = false;
            chkALL1.IsChecked = false;
            int iColumn = dgCell.CurrentColumn.Index;
            txtRow.Text = (iColumn + 1).ToString();

            cboCellYN01.SelectedValue = dgCell.GetCell(0, iColumn).Text.ToString();
            cboCellYN02.SelectedValue = dgCell.GetCell(1, iColumn).Text.ToString();
            cboCellYN03.SelectedValue = dgCell.GetCell(2, iColumn).Text.ToString();
            cboCellYN04.SelectedValue = dgCell.GetCell(3, iColumn).Text.ToString();
            cboCellYN05.SelectedValue = dgCell.GetCell(4, iColumn).Text.ToString();
            cboCellYN06.SelectedValue = dgCell.GetCell(5, iColumn).Text.ToString();
            cboCellYN07.SelectedValue = dgCell.GetCell(6, iColumn).Text.ToString();
            cboCellYN08.SelectedValue = dgCell.GetCell(7, iColumn).Text.ToString();
            cboCellYN09.SelectedValue = dgCell.GetCell(8, iColumn).Text.ToString();
            cboCellYN10.SelectedValue = dgCell.GetCell(9, iColumn).Text.ToString();
            cboCellYN11.SelectedValue = dgCell.GetCell(10, iColumn).Text.ToString();
            cboCellYN12.SelectedValue = dgCell.GetCell(11, iColumn).Text.ToString();
            cboCellYN13.SelectedValue = dgCell.GetCell(12, iColumn).Text.ToString();
            cboCellYN14.SelectedValue = dgCell.GetCell(13, iColumn).Text.ToString();
            cboCellYN15.SelectedValue = dgCell.GetCell(14, iColumn).Text.ToString();
            cboCellYN16.SelectedValue = dgCell.GetCell(15, iColumn).Text.ToString();
            cboCellYN17.SelectedValue = dgCell.GetCell(16, iColumn).Text.ToString();
            cboCellYN18.SelectedValue = dgCell.GetCell(17, iColumn).Text.ToString();
            cboCellYN19.SelectedValue = dgCell.GetCell(18, iColumn).Text.ToString();
            cboCellYN20.SelectedValue = dgCell.GetCell(19, iColumn).Text.ToString();
            cboCellYN21.SelectedValue = dgCell.GetCell(20, iColumn).Text.ToString();
            cboCellYN22.SelectedValue = dgCell.GetCell(21, iColumn).Text.ToString();
            cboCellYN23.SelectedValue = dgCell.GetCell(22, iColumn).Text.ToString();
            cboCellYN24.SelectedValue = dgCell.GetCell(23, iColumn).Text.ToString();
            cboCellYN25.SelectedValue = dgCell.GetCell(24, iColumn).Text.ToString();
            cboCellYN26.SelectedValue = dgCell.GetCell(25, iColumn).Text.ToString();
            cboCellYN27.SelectedValue = dgCell.GetCell(26, iColumn).Text.ToString();
            cboCellYN28.SelectedValue = dgCell.GetCell(27, iColumn).Text.ToString();
            cboCellYN29.SelectedValue = dgCell.GetCell(28, iColumn).Text.ToString();
            cboCellYN30.SelectedValue = dgCell.GetCell(29, iColumn).Text.ToString();
            cboCellYN31.SelectedValue = dgCell.GetCell(30, iColumn).Text.ToString();
            cboCellYN32.SelectedValue = dgCell.GetCell(31, iColumn).Text.ToString();
        }


        /// <summary>
        ///  선택된 열값 Cell에 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string sBinary = string.Empty;
            long lconvert = 0;
            string sConfirm = string.Empty;
            string sWipState = string.Empty;
            int iCol = 0;

            if (sConfirm == "확정")
            {
                // 확정된 Tray 입니다. 수정할 수 없습니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("확정된 Tray 입니다. 수정할 수 없습니다."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (sWipState == "HOLD")
            {
                // Tray 상태가 HOLD 입니다. HOLD 해제를 먼저 하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Tray 상태가 HOLD 입니다. HOLD 해제를 먼저 하세요"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            iCol = Util.StringToInt(txtRow.Text);
            if (iCol <= 0)
            {
                // 열은 0보다 큰수를 입력하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("열은 0보다 큰수를 입력하세요."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            iCol = Util.StringToInt(txtRow.Text) - 1;
            DataTableConverter.SetValue(dgCell.Rows[0].DataItem, dgCell.Columns[iCol].Name, cboCellYN01.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[1].DataItem, dgCell.Columns[iCol].Name, cboCellYN02.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[2].DataItem, dgCell.Columns[iCol].Name, cboCellYN03.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[3].DataItem, dgCell.Columns[iCol].Name, cboCellYN04.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[4].DataItem, dgCell.Columns[iCol].Name, cboCellYN05.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[5].DataItem, dgCell.Columns[iCol].Name, cboCellYN06.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[6].DataItem, dgCell.Columns[iCol].Name, cboCellYN07.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[7].DataItem, dgCell.Columns[iCol].Name, cboCellYN08.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[8].DataItem, dgCell.Columns[iCol].Name, cboCellYN09.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[9].DataItem, dgCell.Columns[iCol].Name, cboCellYN10.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[10].DataItem, dgCell.Columns[iCol].Name, cboCellYN11.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[11].DataItem, dgCell.Columns[iCol].Name, cboCellYN12.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[12].DataItem, dgCell.Columns[iCol].Name, cboCellYN13.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[13].DataItem, dgCell.Columns[iCol].Name, cboCellYN14.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[14].DataItem, dgCell.Columns[iCol].Name, cboCellYN15.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[15].DataItem, dgCell.Columns[iCol].Name, cboCellYN16.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[16].DataItem, dgCell.Columns[iCol].Name, cboCellYN17.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[17].DataItem, dgCell.Columns[iCol].Name, cboCellYN18.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[18].DataItem, dgCell.Columns[iCol].Name, cboCellYN19.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[19].DataItem, dgCell.Columns[iCol].Name, cboCellYN20.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[20].DataItem, dgCell.Columns[iCol].Name, cboCellYN21.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[21].DataItem, dgCell.Columns[iCol].Name, cboCellYN22.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[22].DataItem, dgCell.Columns[iCol].Name, cboCellYN23.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[23].DataItem, dgCell.Columns[iCol].Name, cboCellYN24.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[24].DataItem, dgCell.Columns[iCol].Name, cboCellYN25.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[25].DataItem, dgCell.Columns[iCol].Name, cboCellYN26.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[26].DataItem, dgCell.Columns[iCol].Name, cboCellYN27.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[27].DataItem, dgCell.Columns[iCol].Name, cboCellYN28.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[28].DataItem, dgCell.Columns[iCol].Name, cboCellYN29.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[29].DataItem, dgCell.Columns[iCol].Name, cboCellYN30.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[30].DataItem, dgCell.Columns[iCol].Name, cboCellYN31.SelectedValue.ToString());
            DataTableConverter.SetValue(dgCell.Rows[31].DataItem, dgCell.Columns[iCol].Name, cboCellYN32.SelectedValue.ToString());

            //txt_ROWLOC.Text = IIf(txt_Row.Text.Length = 1, ("0" + txt_Row.Text), txt_Row.Text)

            sBinary = string.Empty;
            sBinary = sBinary + cboCellYN01.Text + cboCellYN02.Text + cboCellYN03.Text + cboCellYN04.Text;
            sBinary = sBinary + cboCellYN05.Text + cboCellYN06.Text + cboCellYN07.Text + cboCellYN08.Text;
            sBinary = sBinary + cboCellYN09.Text + cboCellYN10.Text + cboCellYN11.Text + cboCellYN12.Text;
            sBinary = sBinary + cboCellYN13.Text + cboCellYN14.Text + cboCellYN15.Text + cboCellYN16.Text;
            sBinary = sBinary + cboCellYN17.Text + cboCellYN18.Text + cboCellYN19.Text + cboCellYN20.Text;
            sBinary = sBinary + cboCellYN21.Text + cboCellYN22.Text + cboCellYN23.Text + cboCellYN24.Text;
            sBinary = sBinary + cboCellYN25.Text + cboCellYN26.Text + cboCellYN27.Text + cboCellYN28.Text;
            sBinary = sBinary + cboCellYN29.Text + cboCellYN30.Text + cboCellYN31.Text + cboCellYN32.Text;

            //txt_Binary.Text = sBinary
            lconvert = 0;
            for (int icount = 0; icount <= 31; icount++)
            {
                lconvert = lconvert + Util.StringTolong(sBinary.Substring(icount, 1)) * 2 ^ (32 - icount - 1);
            }

            //txt_Convert.Text = lconvert

        }

        /// <summary>
        /// 입력된 개수만큼 그리드 셀값을 Set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAllUpdate_Click(object sender, RoutedEventArgs e)
        {
            string sConfirm = string.Empty;
            string sWipState = string.Empty;
            int icount = 0;


            if (sConfirm == "확정")
            {
                // 확정된 Tray 입니다. 수정할 수 없습니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("확정된 Tray 입니다. 수정할 수 없습니다."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (sWipState == "HOLD")
            {
                // Tray 상태가 HOLD 입니다. HOLD 해제를 먼저 하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Tray 상태가 HOLD 입니다. HOLD 해제를 먼저 하세요"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            icount = Util.StringToInt(txtCount.Text);
            if (icount > 1024)
            {
                // Cell 갯수는 1024개를 초과 할수 없습니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Cell 갯수는 1024개를 초과 할수 없습니다."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (icount <= 0)
            {
                // Cell 갯수는 0보다 큰수를 입력하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Cell 갯수는 0보다 큰수를 입력하세요."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            //spread reset
            for (int icol = 0; icol < dgCell.Columns.Count; icol++)
            {
                for (int irow = 0; irow < dgCell.Rows.Count; irow++)
                {
                    DataTableConverter.SetValue(dgCell.Rows[irow].DataItem, dgCell.Columns[icol].Name, "");
                }
            }


            // 입력된 갯수만큼 Cell값을 Set.
            int icnt = icount;
            for (int icol = 0; icol < dgCell.Columns.Count; icol++)
            {
                for (int irow = 0; irow < dgCell.Rows.Count; irow++)
                {
                    DataTableConverter.SetValue(dgCell.Rows[irow].DataItem, dgCell.Columns[icol].Name, cboCellYN.SelectedValue.ToString());
                    icnt = icnt - 1;

                    if (icnt == 0)
                    {
                        // 입력된 갯수 만큼 처리 했으면  for 문을 빠져나간다
                        break;
                    }
                }

                if (icnt == 0)
                {
                    // 입력된 갯수 만큼 처리 했으면  for 문을 빠져나간다
                    break;
                }
            }

        }

        #endregion


        #region Mehod

        /// <summary>
        /// 콤보박스 초기화(0,1)
        /// </summary>
        /// <param name="cbo"></param>
        private void SetCombo(ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "0";
                dr["CBO_CODE"] = "0";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["CBO_NAME"] = "1";
                dr["CBO_CODE"] = "1";
                dt.Rows.Add(dr);

                cbo.ItemsSource = DataTableConverter.Convert(dt);

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);

            }
        }

        /// <summary>
        /// 라디오버튼 체크시 선택된 값으로 콤보 초기화
        /// </summary>
        /// <param name="sValue"></param>
        private void initClear(string sValue)
        {
            cboCellYN01.Text = sValue;
            cboCellYN02.Text = sValue;
            cboCellYN03.Text = sValue;
            cboCellYN04.Text = sValue;
            cboCellYN05.Text = sValue;
            cboCellYN06.Text = sValue;
            cboCellYN07.Text = sValue;
            cboCellYN08.Text = sValue;
            cboCellYN09.Text = sValue;
            cboCellYN10.Text = sValue;
            cboCellYN11.Text = sValue;
            cboCellYN12.Text = sValue;
            cboCellYN13.Text = sValue;
            cboCellYN14.Text = sValue;
            cboCellYN15.Text = sValue;
            cboCellYN16.Text = sValue;
            cboCellYN17.Text = sValue;
            cboCellYN18.Text = sValue;
            cboCellYN19.Text = sValue;
            cboCellYN20.Text = sValue;
            cboCellYN21.Text = sValue;
            cboCellYN22.Text = sValue;
            cboCellYN23.Text = sValue;
            cboCellYN24.Text = sValue;
            cboCellYN25.Text = sValue;
            cboCellYN26.Text = sValue;
            cboCellYN27.Text = sValue;
            cboCellYN28.Text = sValue;
            cboCellYN29.Text = sValue;
            cboCellYN30.Text = sValue;
            cboCellYN31.Text = sValue;
            cboCellYN32.Text = sValue;
        }



        #endregion

        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Presenter.Content.GetType() == typeof(System.Windows.Controls.TextBlock))
                {
                    TextBlock tb = e.Cell.Presenter.Content as TextBlock;
                    if (tb != null)
                    {
                        tb.Margin = new Thickness(-2,-10,-2,0);
                    }
                }


                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //Grid Resources 이용한 Border 그리기
                    e.Cell.Presenter.Background = null;

                    int rowidx = e.Cell.Row.Index - dataGrid.TopRows.Count;
                    e.Cell.Presenter.BorderThickness = new Thickness(0);
                    e.Cell.Presenter.LeftLineBrush = null;
                    e.Cell.Presenter.TopLineBrush = null;
                    e.Cell.Presenter.RightLineBrush = null;
                    e.Cell.Presenter.BottomLineBrush = null;
                    if (dataGrid.Resources.Contains("RowBorderInfo"))
                    {
                        Dictionary<int, System.Windows.Media.Brush> rowborderInfo = dataGrid.Resources["RowBorderInfo"] as Dictionary<int, System.Windows.Media.Brush>;

                        if (rowborderInfo.ContainsKey(rowidx))
                        {
                            e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 0, 0);
                            e.Cell.Presenter.LeftLineBrush = null;
                            e.Cell.Presenter.TopLineBrush = rowborderInfo[rowidx];
                            e.Cell.Presenter.RightLineBrush = null;
                            e.Cell.Presenter.BottomLineBrush = null;
                        }
                    }

                    int colidx = e.Cell.Column.Index;
                    if (dataGrid.Resources.Contains("ColBorderInfo"))
                    {
                        Dictionary<int, System.Windows.Media.Brush> colborderInfo = dataGrid.Resources["ColBorderInfo"] as Dictionary<int, System.Windows.Media.Brush>;

                        if (colborderInfo.ContainsKey(colidx))
                        {
                            if (dataGrid.Resources.Contains("RowBorderInfo"))
                            {
                                Dictionary<int, System.Windows.Media.Brush> rowborderInfo = dataGrid.Resources["RowBorderInfo"] as Dictionary<int, System.Windows.Media.Brush>;

                                if (rowborderInfo.ContainsKey(rowidx))
                                {
                                    e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 0, 0);
                                    e.Cell.Presenter.LeftLineBrush = null;
                                    e.Cell.Presenter.TopLineBrush = rowborderInfo[rowidx]; ;
                                    e.Cell.Presenter.RightLineBrush = colborderInfo[colidx];
                                    e.Cell.Presenter.BottomLineBrush = null;
                                }
                                else
                                {
                                    e.Cell.Presenter.BorderThickness = new Thickness(0, 0, 1, 0);
                                    e.Cell.Presenter.LeftLineBrush = null;
                                    e.Cell.Presenter.TopLineBrush = null;
                                    e.Cell.Presenter.RightLineBrush = colborderInfo[colidx];
                                    e.Cell.Presenter.BottomLineBrush = null;
                                }
                            }
                        }
                    }
                }


            }));

        }

    }
}
