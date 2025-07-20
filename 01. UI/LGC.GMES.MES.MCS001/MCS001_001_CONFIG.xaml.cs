/*************************************************************************************
 Created Date : 2017.01.16
      Creator : 김재호 부장
   Decription : SKID BUFFER 모니터링
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.MCS001
{
	public partial class MCS001_001_CONFIG : C1Window, IWorkArea
	{
		#region Declaration & Constructor 

		private string WH_ID;

        private int i_Max_Col_A = 0;
        private int i_Max_Col_B = 0;

        private Color Color01 = Colors.Red;
        private Color Color02 = Colors.Orange;
        private Color Color03 = Colors.Green;
        private Color ColorV1 = Colors.Yellow;

        private DataTable dtUpdatedEQSG;


        public List<string> flaglist;

		public MCS001_001_CONFIG() {
			InitializeComponent();
            IsUpdated = false;
        }
		public IFrameOperation FrameOperation {
			get;
			set;
		}
		private void C1Window_Loaded( object sender, RoutedEventArgs e ) {
			ApplyPermissions();

			object[] Parameters = C1WindowExtension.GetParameters( this );
			WH_ID = Parameters[0].ToString();

            flaglist = new List<string>();
            flaglist.Add("Y");
            flaglist.Add("N");
			SeachData();

            this.InitCombo();

            this.GenerateColumns();

            this.SearchEQSGColor();

            this.SearchDataEQSG();
            

            this.SearchColorLegend();

            this.InitdtUpdatedEQSG();

            //this.ReorderColumns();
        }

        private void ReorderColumns()
        {
            this.dgRACK3.Columns[1].DisplayIndex = 22;
            this.dgRACK3.Columns[2].DisplayIndex = 21;
            this.dgRACK3.Columns[3].DisplayIndex = 20;
            this.dgRACK3.Columns[4].DisplayIndex = 19;
            this.dgRACK3.Columns[5].DisplayIndex = 18;

            this.dgRACK3.Columns[6].DisplayIndex = 17;
            this.dgRACK3.Columns[7].DisplayIndex = 16;
            this.dgRACK3.Columns[8].DisplayIndex = 15;
            this.dgRACK3.Columns[9].DisplayIndex = 14;
            this.dgRACK3.Columns[10].DisplayIndex = 13;


            this.dgRACK3.Columns[11].DisplayIndex = 12;
            this.dgRACK3.Columns[12].DisplayIndex = 11;
            this.dgRACK3.Columns[13].DisplayIndex = 10;
            this.dgRACK3.Columns[14].DisplayIndex = 9;
            this.dgRACK3.Columns[15].DisplayIndex = 8;

            this.dgRACK3.Columns[16].DisplayIndex = 7;
            this.dgRACK3.Columns[17].DisplayIndex = 6;
            this.dgRACK3.Columns[18].DisplayIndex = 5;
            this.dgRACK3.Columns[19].DisplayIndex = 4;
            this.dgRACK3.Columns[20].DisplayIndex = 3;

            this.dgRACK3.Columns[21].DisplayIndex = 2;
            this.dgRACK3.Columns[22].DisplayIndex = 1;

            this.dgRACK3.Columns[1].IsReadOnly = true;
            this.dgRACK3.Columns[2].IsReadOnly = true;
            this.dgRACK3.Columns[3].IsReadOnly = true;
            this.dgRACK3.Columns[4].IsReadOnly = true;
            this.dgRACK3.Columns[5].IsReadOnly = true;

            this.dgRACK3.Columns[6].IsReadOnly = true;
            this.dgRACK3.Columns[7].IsReadOnly = true;
            this.dgRACK3.Columns[8].IsReadOnly = true;
            this.dgRACK3.Columns[9].IsReadOnly = true;
            this.dgRACK3.Columns[10].IsReadOnly = true;


            this.dgRACK3.Columns[11].IsReadOnly = true;
            this.dgRACK3.Columns[12].IsReadOnly = true;
            this.dgRACK3.Columns[13].IsReadOnly = true;
            this.dgRACK3.Columns[14].IsReadOnly = true;
            this.dgRACK3.Columns[15].IsReadOnly = true;

            this.dgRACK3.Columns[16].IsReadOnly = true;
            this.dgRACK3.Columns[17].IsReadOnly = true;
            this.dgRACK3.Columns[18].IsReadOnly = true;
            this.dgRACK3.Columns[19].IsReadOnly = true;
            this.dgRACK3.Columns[20].IsReadOnly = true;

            this.dgRACK3.Columns[21].IsReadOnly = true;
            this.dgRACK3.Columns[22].IsReadOnly = true;
            this.dgRACK3.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.Column;
            this.dgRACK3.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.MultiRange;

            
            //this.dgRACK3.

            this.dgRACK4.Columns[1].DisplayIndex = 22;
            this.dgRACK4.Columns[2].DisplayIndex = 21;
            this.dgRACK4.Columns[3].DisplayIndex = 20;
            this.dgRACK4.Columns[4].DisplayIndex = 19;
            this.dgRACK4.Columns[5].DisplayIndex = 18;


            this.dgRACK4.Columns[6].DisplayIndex = 17;
            this.dgRACK4.Columns[7].DisplayIndex = 16;
            this.dgRACK4.Columns[8].DisplayIndex = 15;
            this.dgRACK4.Columns[9].DisplayIndex = 14;
            this.dgRACK4.Columns[10].DisplayIndex = 13;


            this.dgRACK4.Columns[11].DisplayIndex = 12;
            this.dgRACK4.Columns[12].DisplayIndex = 11;
            this.dgRACK4.Columns[13].DisplayIndex = 10;
            this.dgRACK4.Columns[14].DisplayIndex = 9;
            this.dgRACK4.Columns[15].DisplayIndex = 8;

            this.dgRACK4.Columns[16].DisplayIndex = 7;
            this.dgRACK4.Columns[17].DisplayIndex = 6;
            this.dgRACK4.Columns[18].DisplayIndex = 5;
            this.dgRACK4.Columns[19].DisplayIndex = 4;
            this.dgRACK4.Columns[20].DisplayIndex = 3;

            this.dgRACK4.Columns[21].DisplayIndex = 2;
            this.dgRACK4.Columns[22].DisplayIndex = 1;

            this.dgRACK4.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.Column;
            this.dgRACK4.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.MultiRange;
        }

        private void ReorderColumnsBack()
        {
            this.dgRACK3.Columns[1].DisplayIndex = 1;
            this.dgRACK3.Columns[2].DisplayIndex = 2;
            this.dgRACK3.Columns[3].DisplayIndex = 3;
            this.dgRACK3.Columns[4].DisplayIndex = 4;
            this.dgRACK3.Columns[5].DisplayIndex = 5;


            this.dgRACK3.Columns[6].DisplayIndex = 6;
            this.dgRACK3.Columns[7].DisplayIndex = 7;
            this.dgRACK3.Columns[8].DisplayIndex = 8;
            this.dgRACK3.Columns[9].DisplayIndex = 9;
            this.dgRACK3.Columns[10].DisplayIndex = 10;


            this.dgRACK3.Columns[11].DisplayIndex = 11;
            this.dgRACK3.Columns[12].DisplayIndex = 12;
            this.dgRACK3.Columns[13].DisplayIndex = 13;
            this.dgRACK3.Columns[14].DisplayIndex = 14;
            this.dgRACK3.Columns[15].DisplayIndex = 15;

            this.dgRACK3.Columns[16].DisplayIndex = 16;
            this.dgRACK3.Columns[17].DisplayIndex = 17;
            this.dgRACK3.Columns[18].DisplayIndex = 18;
            this.dgRACK3.Columns[19].DisplayIndex = 19;
            this.dgRACK3.Columns[20].DisplayIndex = 20;

            this.dgRACK3.Columns[21].DisplayIndex = 21;
            this.dgRACK3.Columns[22].DisplayIndex = 22;

            this.dgRACK4.Columns[1].DisplayIndex = 1;
            this.dgRACK4.Columns[2].DisplayIndex = 2;
            this.dgRACK4.Columns[3].DisplayIndex = 3;
            this.dgRACK4.Columns[4].DisplayIndex = 4;
            this.dgRACK4.Columns[5].DisplayIndex = 5;


            this.dgRACK4.Columns[6].DisplayIndex = 6;
            this.dgRACK4.Columns[7].DisplayIndex = 7;
            this.dgRACK4.Columns[8].DisplayIndex = 8;
            this.dgRACK4.Columns[9].DisplayIndex = 9;
            this.dgRACK4.Columns[10].DisplayIndex = 10;


            this.dgRACK4.Columns[11].DisplayIndex = 11;
            this.dgRACK4.Columns[12].DisplayIndex = 12;
            this.dgRACK4.Columns[13].DisplayIndex = 13;
            this.dgRACK4.Columns[14].DisplayIndex = 14;
            this.dgRACK4.Columns[15].DisplayIndex = 15;

            this.dgRACK4.Columns[16].DisplayIndex = 16;
            this.dgRACK4.Columns[17].DisplayIndex = 17;
            this.dgRACK4.Columns[18].DisplayIndex = 18;
            this.dgRACK4.Columns[19].DisplayIndex = 19;
            this.dgRACK4.Columns[20].DisplayIndex = 20;

            this.dgRACK4.Columns[21].DisplayIndex = 21;
            this.dgRACK4.Columns[22].DisplayIndex = 22;
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions() {
			List<Button> listAuth = new List<Button>();
			//listAuth.Add(btnInReplace);
			Util.pageAuth( listAuth, FrameOperation.AUTHORITY );
		}


        private void InitdtUpdatedEQSG()
        {
            dtUpdatedEQSG = new DataTable();
            dtUpdatedEQSG.Columns.Add("ZONE_ID", typeof(string));
            dtUpdatedEQSG.Columns.Add("X_PSTN", typeof(string));
            dtUpdatedEQSG.Columns.Add("Y_PSTN", typeof(string));
            dtUpdatedEQSG.Columns.Add("Z_PSTN", typeof(string));
            dtUpdatedEQSG.Columns.Add("EQSGID", typeof(string));
        }
        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent() {
			this.Loaded -= C1Window_Loaded;
		}

		private void OnBtnRefresh( object sender, RoutedEventArgs e ) {
			SeachData();
		}


        private void btnSAve_Click(object sender, RoutedEventArgs e)
        {
            // 저장

            if (dgList.ItemsSource != null)
            {



                Util.MessageConfirm("SFU3533", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {




                        foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgList))
                        {
                            string port_id = row["port_id"].ToString();
                            string auto_iss_req_flag = row["auto_iss_req_flag"].ToString();
                            string ELTR_TYPE_CODE = row["ELTR_TYPE_CODE"].ToString();
                            string EQPTID = row["EQPTID"].ToString();

                            // UPDATE TB_MCS_CURR_PORT AUTO_ISS_REQ_FLAG
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.Columns.Add("PORT_ID", typeof(string));
                            RQSTDT.Columns.Add("AUTO_ISS_REQ_FLAG", typeof(string));
                            RQSTDT.Columns.Add("UPDUSER", typeof(string));

                            try
                            {
                                DataRow dr = RQSTDT.NewRow();
                                dr["PORT_ID"] = port_id;
                                dr["AUTO_ISS_REQ_FLAG"] = auto_iss_req_flag;
                                dr["UPDUSER"] = LoginInfo.USERID;

                                RQSTDT.Rows.Add(dr);

                                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_AUTO_ISS_REQ_FLAG", "RQSTDT", "RSLTDT", RQSTDT);
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                            }

                            // UPDATE TB_MCS_PORT ELTR_TYPE_CODE, LINK_EQPTID
                            DataTable RQSTDT2 = new DataTable();
                            RQSTDT2.Columns.Add("PORT_ID", typeof(string));
                            RQSTDT2.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                            RQSTDT2.Columns.Add("EQPTID", typeof(string));
                            RQSTDT2.Columns.Add("UPDUSER", typeof(string));

                            try
                            {
                                DataRow dr = RQSTDT2.NewRow();
                                dr["PORT_ID"] = port_id;
                                dr["ELTR_TYPE_CODE"] = ELTR_TYPE_CODE;
                                dr["EQPTID"] = EQPTID;
                                dr["UPDUSER"] = LoginInfo.USERID;
                                RQSTDT2.Rows.Add(dr);

                                DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_ELTR_TYPE_CODE", "RQSTDT2", "RSLTDT2", RQSTDT2);
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                            }
                        }

                        Util.AlertInfo("SFU1275");      //정상 처리 되었습니다.

                        SeachData();

                        IsUpdated = true;
                    }
                });
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;

        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("EQPTID") || e.Cell.Column.Name.Equals("ELTR_TYPE_CODE"))
                    {
                        if (dataGrid.Columns.Contains("isChecked"))
                        {
                            string sisChecked = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "isChecked"));

                            if (sisChecked == "1")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);

                                e.Cell.Presenter.IsEnabled = false;

                                e.Cell.Presenter.IsCurrent = false;
                            }
                        }
                    }
                }
            }));
        }

        private void dgList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((System.Data.DataRowView)dgList.CurrentRow.DataItem).Row.ItemArray[9].ToString() == "1")
            {
                e.Handled = true;
            }
        }

        private void dgRACK_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Text.Contains("02") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color02);
            }
            else if (e.Cell.Text.Contains("01") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color01);
            }
            else if (e.Cell.Text.Contains("03") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color03);
            }
            else if (e.Cell.Text.Contains("V1") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(ColorV1);
            }
            else if (e.Cell.Text.Contains("PORT") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            }

        }

        private void dgRACK3_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Text.Contains("02") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color02);
            }
            else if (e.Cell.Text.Contains("01") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color01);
            }
            else if (e.Cell.Text.Contains("03") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color03);
            }
            else if (e.Cell.Text.Contains("V1") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(ColorV1);
            }
            else if (e.Cell.Text.Contains("PORT") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void dgRACK2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Text.Contains("02") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color02);
            }
            else if (e.Cell.Text.Contains("01") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color01);
            }
            else if (e.Cell.Text.Contains("03") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color03);
            }
            else if (e.Cell.Text.Contains("V1") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(ColorV1);
            }
            else if (e.Cell.Text.Contains("PORT") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void dgRACK4_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Text.Contains("02") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color02);
            }
            else if (e.Cell.Text.Contains("01") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color01);
            }
            else if (e.Cell.Text.Contains("03") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Color03);
            }
            else if (e.Cell.Text.Contains("V1") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(ColorV1);
            }
            else if (e.Cell.Text.Contains("PORT") && e.Cell.Column.Index > 0)
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.GenerateColumns();
            this.SearchDataEQSG();
            this.InitdtUpdatedEQSG();
            //this.ReorderColumns();
        }

        private void BtnApplyClick(object sender, RoutedEventArgs e)
        {
            //적용  
            try
            {

                //this.ReorderColumnsBack();

                foreach (C1.WPF.DataGrid.DataGridCell dc in dgRACK.Selection.SelectedCells)
                {
                    //string sEQSG = cboEQSG.SelectedValue.ToString().Replace("A5A", "");

                    string sEQSG = ((System.Windows.FrameworkElement)cboEQSG.SelectedItem).Tag.ToString().Replace("A5A", "");
                    //string sEQSG = cboEQSG.SelectedValue.  Sng().Replace("A5A", "");
                    if (dc.Value.ToString() != "PORT" && dc.Column.Index > 0)
                    {
                        dc.Value = sEQSG;

                        string Z_PSTN = String.Empty;

                        if (dc.Row.Index.ToString() == "0")
                        {
                            Z_PSTN = "3";
                        }
                        else if (dc.Row.Index.ToString() == "1")
                        {
                            Z_PSTN = "2";
                        }
                        else if (dc.Row.Index.ToString() == "2")
                        {
                            Z_PSTN = "1";
                        }


                        DataRow dr = dtUpdatedEQSG.NewRow();
                        dr["ZONE_ID"] = 'A';
                        dr["X_PSTN"] = "1";
                        dr["Y_PSTN"] = dc.Column.Index.ToString();
                        dr["Z_PSTN"] = Z_PSTN;
                        dr["EQSGID"] = "A5A" + sEQSG;
                        dtUpdatedEQSG.Rows.Add(dr);

                    }
                }
            }
            catch
            {
            }

            try
            {
                foreach (C1.WPF.DataGrid.DataGridCell dc in dgRACK2.Selection.SelectedCells)
                {
                    string sEQSG = ((System.Windows.FrameworkElement)cboEQSG.SelectedItem).Tag.ToString().Replace("A5A", "");
                    if (dc.Value.ToString() != "PORT" && dc.Column.Index > 0)
                    {
                        dc.Value = sEQSG;

                        string Z_PSTN = String.Empty;

                        if (dc.Row.Index.ToString() == "0")
                        {
                            Z_PSTN = "3";
                        }
                        else if (dc.Row.Index.ToString() == "1")
                        {
                            Z_PSTN = "2";
                        }
                        else if (dc.Row.Index.ToString() == "2")
                        {
                            Z_PSTN = "1";
                        }

                        DataRow dr = dtUpdatedEQSG.NewRow();
                        dr["ZONE_ID"] = 'A';
                        dr["X_PSTN"] = "2";
                        dr["Y_PSTN"] = dc.Column.Index.ToString();
                        dr["Z_PSTN"] = Z_PSTN;
                        dr["EQSGID"] = "A5A" + sEQSG;
                        dtUpdatedEQSG.Rows.Add(dr);
                    }
                }
            }
            catch
            {
            }
            try {

                //foreach (C1.WPF.DataGrid.DataGridColumn dc in dgRACK3.Selection.SelectedColumns)
                //{
                //    int icolindex = dc.Index;
                //    foreach (C1.WPF.DataGrid.DataGridRow dr in dgRACK3.Selection.SelectedRows)
                //    {
                //        int irowindex = dr.Index;

                //        C1.WPF.DataGrid.DataGridCell dcc = dr[irowindex];
                //    }

                //}
                //dgRACK3.sele
                foreach (C1.WPF.DataGrid.DataGridCell dc in dgRACK3.Selection.SelectedCells)
                   // foreach (C1.WPF.DataGrid.DataGridCell dc in dgRACK3.SelectedItem as C1.WPF.DataGrid.DataGridCell[])
                    {
                    string sEQSG = ((System.Windows.FrameworkElement)cboEQSG.SelectedItem).Tag.ToString().Replace("A5A", "");
                    if (dc.Value.ToString() != "PORT" && dc.Column.Index > 0)
                    {
                        dc.Value = sEQSG;

                        string Z_PSTN = String.Empty;

                        if (dc.Row.Index.ToString() == "0")
                        {
                            Z_PSTN = "3";
                        }
                        else if (dc.Row.Index.ToString() == "1")
                        {
                            Z_PSTN = "2";
                        }
                        else if (dc.Row.Index.ToString() == "2")
                        {
                            Z_PSTN = "1";
                        }

                        DataRow dr = dtUpdatedEQSG.NewRow();
                        dr["ZONE_ID"] = 'B';
                        dr["X_PSTN"] = "1";
                        dr["Y_PSTN"] = dc.Column.Index.ToString();
                        dr["Z_PSTN"] = Z_PSTN;
                        dr["EQSGID"] = "A5A" + sEQSG;
                        dtUpdatedEQSG.Rows.Add(dr);
                    }
                }
            }
            catch
            {
            }

            try
            {
                foreach (C1.WPF.DataGrid.DataGridCell dc in dgRACK4.Selection.SelectedCells)
                {
                    string sEQSG = ((System.Windows.FrameworkElement)cboEQSG.SelectedItem).Tag.ToString().Replace("A5A", "");
                    if (dc.Value.ToString() != "PORT" && dc.Column.Index > 0)
                    {
                        dc.Value = sEQSG;

                        string Z_PSTN = String.Empty;

                        if (dc.Row.Index.ToString() == "0")
                        {
                            Z_PSTN = "3";
                        }
                        else if (dc.Row.Index.ToString() == "1")
                        {
                            Z_PSTN = "2";
                        }
                        else if (dc.Row.Index.ToString() == "2")
                        {
                            Z_PSTN = "1";
                        }

                        DataRow dr = dtUpdatedEQSG.NewRow();
                        dr["ZONE_ID"] = 'B';
                        dr["X_PSTN"] = "2";
                        dr["Y_PSTN"] = dc.Column.Index.ToString();
                        dr["Z_PSTN"] = Z_PSTN;
                        dr["EQSGID"] = "A5A" + sEQSG;
                        dtUpdatedEQSG.Rows.Add(dr);
                    }
                }
            }
            catch
            {
            }

            dgRACK.Selection.Clear();
            dgRACK2.Selection.Clear();
            dgRACK3.Selection.Clear();
            dgRACK4.Selection.Clear();

            //this.ReorderColumns();

        }

        private void btnEQSDSave(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sZONE_ID = String.Empty;
                    string sX_PSTN = String.Empty;
                    string sY_PSTN = String.Empty;
                    string sZ_PSTN = String.Empty;
                    string sEQSGID = String.Empty;

                    foreach (DataRow row in dtUpdatedEQSG.Rows)
                    {
                        sZONE_ID = row["ZONE_ID"].ToString();
                        sX_PSTN = row["X_PSTN"].ToString();
                        sY_PSTN = row["Y_PSTN"].ToString();
                        sZ_PSTN = row["Z_PSTN"].ToString();
                        sEQSGID = row["EQSGID"].ToString();

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("ZONE_ID", typeof(string));
                        RQSTDT.Columns.Add("X_PSTN", typeof(string));
                        RQSTDT.Columns.Add("Y_PSTN", typeof(string));
                        RQSTDT.Columns.Add("Z_PSTN", typeof(string));
                        RQSTDT.Columns.Add("LINK_EQSGID", typeof(string));
                        RQSTDT.Columns.Add("UPDUSER", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["ZONE_ID"] = sZONE_ID;
                        dr["X_PSTN"] = sX_PSTN;
                        dr["Y_PSTN"] = sY_PSTN;
                        dr["Z_PSTN"] = sZ_PSTN;
                        dr["LINK_EQSGID"] = sEQSGID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        RQSTDT.Rows.Add(dr);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_LINK_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

                    }

                    this.InitdtUpdatedEQSG();

                    Util.AlertInfo("SFU1275");      //정상 처리 되었습니다.
                }
            }
            );
        }
        #endregion

        #region Mehod
        private void SeachData() {
			DataTable RQSTDT = new DataTable( "RQSTDT" );
			RQSTDT.Columns.Add( "LANGID", typeof( string ) );
			RQSTDT.Columns.Add( "WH_ID", typeof( string ) );

			DataRow dr = RQSTDT.NewRow();
			dr["LANGID"] = LoginInfo.LANGID;
			dr["WH_ID"] = WH_ID;

			RQSTDT.Rows.Add( dr );

			new ClientProxy().ExecuteService("DA_MCS_SEL_CURR_PORT_CONFIG", "RQSTDT", "RSLTDT", RQSTDT, ( result, exception ) => {
				try {
					if( exception != null ) {
						Util.MessageException( exception );
						return;
					}

					Util.GridSetData( dgList, result, FrameOperation );
                    
				} catch( Exception ex ) {
					Util.MessageException( ex );
				} finally {
				}

                // 출고 여부
                SetGridCboItem(dgList.Columns[2]);


                // 연결설비
                DataTable RQSTDT2 = new DataTable("RQSTDT2");
                RQSTDT2.Columns.Add("LANGID", typeof(string));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LANGID"] = LoginInfo.LANGID;

                RQSTDT2.Rows.Add(dr2);

                DataTable dtMappingEQPT = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAPPING_EQPT", "RQSTDT2", "RSLTDT_EQPT", RQSTDT2);
                (dgList.Columns[3] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMappingEQPT);
                //(dgList.Columns[3] as C1.WPF.DataGrid.DataGridComboBoxColumn).au

                //극성
                DataTable dtELTRTYPE = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PORT_ELTR_TYPE", "RQSTDT2", "RSLTDT_ELTRTYPE", RQSTDT2);
                (dgList.Columns[4] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtELTRTYPE);
            } );    
		}

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("auto_iss_req_flag");

            DataRow newRow = null;

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "Y"};
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "N" };
            dt.Rows.Add(newRow);            

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DropDownWidth = 70;
        }      

        /// <summary>
        /// 정보변경버튼으로 정보 변경여부 확인
        /// </summary>
        public bool IsUpdated
        {
            get;
            set;
        }


        private void InitCombo()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("WH_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["WH_ID"] = WH_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_INSP_RSV_RULE_COMBO", "RQSTDT", "RSLTDT", RQSTDT);


            this.cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = dtResult.Copy().AsDataView();

            cbo.SelectedIndex = 0;

            // 현재 설정가져오기

            DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_INSP_RSV_RULE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult2 != null && dtResult2.Rows.Count > 0)
            {
                string sKeyId = dtResult2.Rows[0][0].ToString();

                cbo.SelectedValue = sKeyId;
            }

            DataTable RQDT = new DataTable("RQSTDT");
            RQDT.Columns.Add("LANGID", typeof(string));

            DataRow drColor = RQDT.NewRow();
            drColor["LANGID"] = LoginInfo.LANGID;

            RQDT.Rows.Add(drColor);

            DataTable dtColorResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_EQSGIDCOLOR", "RQSTDT", "RSLTDT", RQDT);

            foreach (DataRow row in dtColorResult.Rows)
            {
                C1ComboBoxItem cbItem1 = new C1ComboBoxItem();
                //cbItem1.
                //cbItem1.DataContext = "KEYID2";
                cbItem1.Content = row["KEYNAME2"].ToString();
                cbItem1.Tag = row["KEYID2"].ToString();
                cbItem1.Background = new BrushConverter().ConvertFromString(row["KEYVALUE2"].ToString()) as SolidColorBrush;

                cboEQSG.Items.Add(cbItem1);
            }

            cboEQSG.SelectedIndex = 0;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("WH_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["WH_ID"] = WH_ID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_INSP_RSV_RULE", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        // 저장
                        // rsv rule 부터 저장하기
                        DataTable RQSTDT2 = new DataTable();
                        RQSTDT2.TableName = "RQSTDT";
                        RQSTDT2.Columns.Add("KEYID", typeof(string));
                        RQSTDT2.Columns.Add("KEYNAME", typeof(string));
                        RQSTDT2.Columns.Add("UPDUSER", typeof(string));

                        DataRow dr2 = RQSTDT2.NewRow();
                        dr2["KEYID"] = cbo.SelectedValue.ToString();
                        dr2["KEYNAME"] = cbo.Text;
                        dr2["UPDUSER"] = LoginInfo.USERID;
                        RQSTDT2.Rows.Add(dr2);

                        DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_MCS_CONFIG", "RQSTDT", "RSLTDT", RQSTDT2);


                    }
                    else
                    {
                        // insert

                        DataTable RQSTDT3 = new DataTable();
                        RQSTDT3.TableName = "RQSTDT";
                        RQSTDT3.Columns.Add("KEYID", typeof(string));
                        RQSTDT3.Columns.Add("KEYNAME", typeof(string));
                        RQSTDT3.Columns.Add("UPDUSER", typeof(string));

                        DataRow dr3 = RQSTDT3.NewRow();
                        dr3["KEYID"] = cbo.SelectedValue.ToString();
                        dr3["KEYNAME"] = cbo.Text;
                        dr3["UPDUSER"] = LoginInfo.USERID;
                        RQSTDT3.Rows.Add(dr3);

                        DataTable dtResult3 = new ClientProxy().ExecuteServiceSync("DA_MCS_INS_MCS_CONFIG", "RQSTDT", "RSLTDT", RQSTDT3);
                        IsUpdated = true;

                    }


                    // COLOR 저장
                    foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgColorLegend))
                    {
                        string sKEYID = String.Empty;
                        string sKEYVALUE = String.Empty;


                        sKEYID = row["KEYID"].ToString();
                        sKEYVALUE = row["KEYVALUE"].ToString();

                        DataTable RQDT = new DataTable();
                        RQDT.TableName = "RQSTDT";
                        RQDT.Columns.Add("KEYID", typeof(string));
                        RQDT.Columns.Add("KEYVALUE", typeof(string));
                        RQDT.Columns.Add("UPDUSER", typeof(string));

                        DataRow dr3 = RQDT.NewRow();
                        dr3["KEYID"] = sKEYID;
                        dr3["KEYVALUE"] = sKEYVALUE;
                        dr3["UPDUSER"] = LoginInfo.USERID;
                        RQDT.Rows.Add(dr3);

                        DataTable dtResult3 = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_COLOR_LEGEND", "RQSTDT", "RSLTDT", RQDT);

                        IsUpdated = true;
                    }


                    // EQSGCOLOR저장
                    foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgEQSGColor))
                    {
                        string sKEYID = String.Empty;
                        string sKEYVALUE = String.Empty;


                        sKEYID = row["KEYID2"].ToString();
                        sKEYVALUE = row["KEYVALUE2"].ToString();

                        DataTable RQDT = new DataTable();
                        RQDT.TableName = "RQSTDT";
                        RQDT.Columns.Add("KEYID", typeof(string));
                        RQDT.Columns.Add("KEYVALUE", typeof(string));
                        RQDT.Columns.Add("UPDUSER", typeof(string));

                        DataRow dr3 = RQDT.NewRow();
                        dr3["KEYID"] = sKEYID;
                        dr3["KEYVALUE"] = sKEYVALUE;
                        dr3["UPDUSER"] = LoginInfo.USERID;
                        RQDT.Rows.Add(dr3);

                        DataTable dtResult3 = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_EQSGCOLOR", "RQSTDT", "RSLTDT", RQDT);

                        IsUpdated = true;
                    }

                    this.SearchEQSGColor();

                    //this.GenerateColumns();
                    //this.SearchDataEQSG();
                    //this.InitdtUpdatedEQSG();

                    Util.MessageInfo("SFU1275");
                }
            });
        }


        private void GenerateColumns()
        {
            try
            {
                dgRACK.Columns.Clear();
                dgRACK2.Columns.Clear();
                dgRACK3.Columns.Clear();
                dgRACK4.Columns.Clear();
            }
            catch
            {
            }

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("WH_ID", typeof(string));
            RQSTDT.Columns.Add("ZONE_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["WH_ID"] = WH_ID;
            dr["ZONE_ID"] = 'A';
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAX_COL", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                i_Max_Col_A = Convert.ToInt16(dtResult.Rows[0][0]);
            }

            RQSTDT.Clear();

            DataRow dr2 = RQSTDT.NewRow();
            dr2["WH_ID"] = WH_ID;
            dr2["ZONE_ID"] = 'B';
            RQSTDT.Rows.Add(dr2);

            DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAX_COL", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult2 != null && dtResult2.Rows.Count > 0)
            {
                i_Max_Col_B = Convert.ToInt16(dtResult2.Rows[0][0]);
            }


            C1.WPF.DataGrid.DataGridTextColumn col = new C1.WPF.DataGrid.DataGridTextColumn();
            col.Header = "1열";
            col.Width = new C1.WPF.DataGrid.DataGridLength(60, DataGridUnitType.Pixel);
            col.Binding = new System.Windows.Data.Binding("qwe");
            dgRACK.Columns.Add(col);

            DataGridExtension.SetIsAlternatingRow(dgRACK, false);
            DataGridExtension.SetIsAlternatingRow(dgRACK2, false);
            DataGridExtension.SetIsAlternatingRow(dgRACK3, false);
            DataGridExtension.SetIsAlternatingRow(dgRACK4, false);
            dgRACK.IsReadOnly = true;
            dgRACK2.IsReadOnly = true;
            dgRACK3.IsReadOnly = true;
            dgRACK4.IsReadOnly = true;

            for (int i = 1; i < i_Max_Col_A + 1; i++)
            {
                C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                column.Header = i.ToString();
                //column.TextTrimming = TextTrimming.CharacterEllipsis;
                column.CellContentStyle = (Style)Application.Current.Resources["Grid_CellContentStyle"];
                
                column.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                column.Binding = new System.Windows.Data.Binding(i.ToString());
                dgRACK.Columns.Add(column);
            }

            DataTable GridData = new DataTable();

            GridData.Columns.Add("qwe", typeof(string)); // 

            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();

            list1.Add("");
            list2.Add("");
            list3.Add("");

            for (int i = 1; i < i_Max_Col_A + 1; i++)
            {
                GridData.Columns.Add(i.ToString(), typeof(string)); //
                list1.Add("");
                list2.Add("");
                list3.Add("");
            }


            object[] rowobject1 = new object[] { list1 };
            object[] rowobject2 = new object[] { list2 };
            object[] rowobject3 = new object[] { list3 };

            GridData.Rows.Add(rowobject1);
            GridData.Rows.Add(rowobject2);
            GridData.Rows.Add(rowobject3);

            this.dgRACK.ItemsSource = DataTableConverter.Convert(GridData);  
            
            //////
            C1.WPF.DataGrid.DataGridTextColumn col2 = new C1.WPF.DataGrid.DataGridTextColumn();
            col2.Header = "2열";
            col2.Width = new C1.WPF.DataGrid.DataGridLength(60, DataGridUnitType.Pixel);
            col2.Binding = new System.Windows.Data.Binding("qwe");
            dgRACK2.Columns.Add(col2);

            for (int i = 1; i < i_Max_Col_A + 1; i++)
            {
                C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                column.Header = i.ToString();
                //column.TextTrimming = TextTrimming.CharacterEllipsis;
                column.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                column.Binding = new System.Windows.Data.Binding(i.ToString());
                dgRACK2.Columns.Add(column);
            }

            DataTable GridData2 = new DataTable();

            GridData2.Columns.Add("qwe", typeof(string)); // 

            List<string> list21 = new List<string>();
            List<string> list22 = new List<string>();
            List<string> list23 = new List<string>();

            list21.Add("");
            list22.Add("");
            list23.Add("");

            for (int i = 1; i < i_Max_Col_A + 1; i++)
            {
                GridData2.Columns.Add(i.ToString(), typeof(string)); //
                list21.Add("");
                list22.Add("");
                list23.Add("");
            }


            object[] rowobject21 = new object[] { list21 };
            object[] rowobject22 = new object[] { list22 };
            object[] rowobject23 = new object[] { list23 };

            GridData2.Rows.Add(rowobject21);
            GridData2.Rows.Add(rowobject22);
            GridData2.Rows.Add(rowobject23);

            this.dgRACK2.ItemsSource = DataTableConverter.Convert(GridData2);






            /////



            DataTable GridData3 = new DataTable();

            GridData3.Columns.Add("qwe", typeof(string)); // 

            List<string> list31 = new List<string>();
            List<string> list32 = new List<string>();
            List<string> list33 = new List<string>();

            list31.Add("");
            list32.Add("");
            list33.Add("");

            for (int i = 1; i < i_Max_Col_B + 1; i++)
            {
                GridData3.Columns.Add(i.ToString(), typeof(string)); //
                list31.Add("");
                list32.Add("");
                list33.Add("");
            }

            object[] rowobject31 = new object[] { list31 };
            object[] rowobject32 = new object[] { list32 };
            object[] rowobject33 = new object[] { list33 };

            GridData3.Rows.Add(rowobject31);
            GridData3.Rows.Add(rowobject32);
            GridData3.Rows.Add(rowobject33);


            C1.WPF.DataGrid.DataGridTextColumn col3 = new C1.WPF.DataGrid.DataGridTextColumn();
            col3.Header = "1열";
            col3.Width = new C1.WPF.DataGrid.DataGridLength(60, DataGridUnitType.Pixel);
            col3.Binding = new System.Windows.Data.Binding("qwe");
            dgRACK3.Columns.Add(col3);

            for (int i = 1; i < i_Max_Col_B + 1; i++)
            {
                C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                column.Header = i.ToString();
                //column.TextTrimming = TextTrimming.CharacterEllipsis;
                column.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                column.Binding = new System.Windows.Data.Binding(i.ToString());
                dgRACK3.Columns.Add(column);
            }

            this.dgRACK3.ItemsSource = DataTableConverter.Convert(GridData3);

            /////
            C1.WPF.DataGrid.DataGridTextColumn col4 = new C1.WPF.DataGrid.DataGridTextColumn();
            col4.Header = "2열";
            col4.Width = new C1.WPF.DataGrid.DataGridLength(60, DataGridUnitType.Pixel);
            col4.Binding = new System.Windows.Data.Binding("qwe");
            dgRACK4.Columns.Add(col4);

            for (int i = 1; i < i_Max_Col_B + 1; i++)
            {
                C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                column.Header = i.ToString();
                //column.TextTrimming = TextTrimming.CharacterEllipsis;
                column.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                column.Binding = new System.Windows.Data.Binding(i.ToString());
                dgRACK4.Columns.Add(column);
            }

            DataTable GridData4 = new DataTable();

            GridData4.Columns.Add("qwe", typeof(string)); // 

            List<string> list41 = new List<string>();
            List<string> list42 = new List<string>();
            List<string> list43 = new List<string>();

            list41.Add("");
            list42.Add("");
            list43.Add("");

            for (int i = 1; i < i_Max_Col_B + 1; i++)
            {
                GridData4.Columns.Add(i.ToString(), typeof(string)); //
                list41.Add("");
                list42.Add("");
                list43.Add("");
            }

            object[] rowobject41 = new object[] { list41 };
            object[] rowobject42 = new object[] { list42 };
            object[] rowobject43 = new object[] { list43 };

            GridData4.Rows.Add(rowobject41);
            GridData4.Rows.Add(rowobject42);
            GridData4.Rows.Add(rowobject43);

            this.dgRACK4.ItemsSource = DataTableConverter.Convert(GridData4);
        }


        private void SearchDataEQSG()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                string sZONE = String.Empty;
                string sRACK_STAT_CODE = String.Empty;
                string sEQSGID = String.Empty;
                string sX_PSTN = String.Empty;
                string sY_PSTN = String.Empty;
                string sZ_PSTN = String.Empty;
                int iZ_PSTN_ADJ = 0;


                ((System.Data.DataView)dgRACK.ItemsSource).Table.Rows[0][0] = ObjectDic.Instance.GetObjectName("3단");
                ((System.Data.DataView)dgRACK.ItemsSource).Table.Rows[1][0] = ObjectDic.Instance.GetObjectName("2단");
                ((System.Data.DataView)dgRACK.ItemsSource).Table.Rows[2][0] = ObjectDic.Instance.GetObjectName("1단");

                ((System.Data.DataView)dgRACK2.ItemsSource).Table.Rows[0][0] = ObjectDic.Instance.GetObjectName("3단");
                ((System.Data.DataView)dgRACK2.ItemsSource).Table.Rows[1][0] = ObjectDic.Instance.GetObjectName("2단");
                ((System.Data.DataView)dgRACK2.ItemsSource).Table.Rows[2][0] = ObjectDic.Instance.GetObjectName("1단");

                ((System.Data.DataView)dgRACK3.ItemsSource).Table.Rows[0][0] = ObjectDic.Instance.GetObjectName("3단");
                ((System.Data.DataView)dgRACK3.ItemsSource).Table.Rows[1][0] = ObjectDic.Instance.GetObjectName("2단");
                ((System.Data.DataView)dgRACK3.ItemsSource).Table.Rows[2][0] = ObjectDic.Instance.GetObjectName("1단");

                ((System.Data.DataView)dgRACK4.ItemsSource).Table.Rows[0][0] = ObjectDic.Instance.GetObjectName("3단");
                ((System.Data.DataView)dgRACK4.ItemsSource).Table.Rows[1][0] = ObjectDic.Instance.GetObjectName("2단");
                ((System.Data.DataView)dgRACK4.ItemsSource).Table.Rows[2][0] = ObjectDic.Instance.GetObjectName("1단");


                foreach (DataRow row in dtResult.Rows)
                {
                    sZONE = row["ZONE_ID"].ToString();
                    sRACK_STAT_CODE = row["RACK_STAT_CODE"].ToString();
                    sEQSGID = row["LINK_EQSGID"].ToString();
                    sX_PSTN = row["X_PSTN"].ToString();
                    sY_PSTN = row["Y_PSTN"].ToString();
                    sZ_PSTN = row["Z_PSTN"].ToString();

                    if (sZ_PSTN == "1")
                    {
                        iZ_PSTN_ADJ = 2;
                    }
                    else if (sZ_PSTN == "2")
                    {
                        iZ_PSTN_ADJ = 1;
                    }
                    else
                    {
                        iZ_PSTN_ADJ = 0;
                    }


                    if (sZONE == "A")
                    {
                        if (sX_PSTN == "1")
                        {
                            if (sRACK_STAT_CODE == "PORT")
                            {
                                ((System.Data.DataView)dgRACK.ItemsSource).Table.Rows[iZ_PSTN_ADJ][sY_PSTN] = "PORT";
                            }
                            else
                            {
                                ((System.Data.DataView)dgRACK.ItemsSource).Table.Rows[iZ_PSTN_ADJ][sY_PSTN] = sEQSGID.Replace("A5A","");
                            }
                        }
                        else if (sX_PSTN == "2")
                        {
                            if (sRACK_STAT_CODE == "PORT")
                            {
                                ((System.Data.DataView)dgRACK2.ItemsSource).Table.Rows[iZ_PSTN_ADJ][sY_PSTN] = "PORT";
                            }
                            else
                            {
                                ((System.Data.DataView)dgRACK2.ItemsSource).Table.Rows[iZ_PSTN_ADJ][sY_PSTN] = sEQSGID.Replace("A5A", "");
                            }
                        }
                    }
                    else if (sZONE == "B")
                    {
                        if (sX_PSTN == "1")
                        {
                            if (sRACK_STAT_CODE == "PORT")
                            {
                                ((System.Data.DataView)dgRACK3.ItemsSource).Table.Rows[iZ_PSTN_ADJ][sY_PSTN] = "PORT";
                            }
                            else
                            {
                                ((System.Data.DataView)dgRACK3.ItemsSource).Table.Rows[iZ_PSTN_ADJ][sY_PSTN] = sEQSGID.Replace("A5A", "");
                            }
                        }
                        else if (sX_PSTN == "2")
                        {
                            if (sRACK_STAT_CODE == "PORT")
                            {
                                ((System.Data.DataView)dgRACK4.ItemsSource).Table.Rows[iZ_PSTN_ADJ][sY_PSTN] = "PORT";
                            }
                            else
                            {
                                ((System.Data.DataView)dgRACK4.ItemsSource).Table.Rows[iZ_PSTN_ADJ][sY_PSTN] = sEQSGID.Replace("A5A", "");
                            }
                        }
                    }

                }


                
            }
        }

        private void SearchColorLegend()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;

            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_COLOR_LEGEND", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) => {
                try
                {
                    if (exception != null)
                    {
                        Util.MessageException(exception);
                        return;
                    }

                    Util.GridSetData(dgColorLegend, result, FrameOperation);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                }

            });
        }

        private void SearchEQSGIDColor()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;

            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_EQSGIDColor", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) => {
                try
                {
                    if (exception != null)
                    {
                        Util.MessageException(exception);
                        return;
                    }

                    //Util.GridSetData(dgColorLegend, result, FrameOperation);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                }

            });
        }

        private void SearchEQSGColor()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;

            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_EQSGIDCOLOR", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) => {
                try
                {
                    if (exception != null)
                    {
                        Util.MessageException(exception);
                        return;
                    }

                    Util.GridSetData(dgEQSGColor, result, FrameOperation);

                    // set color

                    foreach (DataRow row in result.Rows)
                    {
                        if (row["KEYID2"].ToString() == "A5A01")
                        {
                            Color01 = (Color)System.Windows.Media.ColorConverter.ConvertFromString(row["KEYVALUE2"].ToString());
                        }
                        else if (row["KEYID2"].ToString() == "A5A02")
                        {
                            Color02 = (Color)System.Windows.Media.ColorConverter.ConvertFromString(row["KEYVALUE2"].ToString());
                        }
                        else if (row["KEYID2"].ToString() == "A5AV1")
                        {
                            ColorV1 = (Color)System.Windows.Media.ColorConverter.ConvertFromString(row["KEYVALUE2"].ToString());
                        }
                        else if (row["KEYID2"].ToString() == "A5A03")
                        {
                            Color03 = (Color)System.Windows.Media.ColorConverter.ConvertFromString(row["KEYVALUE2"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                }

            });
        }

        #endregion
    }
}