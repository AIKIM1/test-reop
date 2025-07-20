/*************************************************************************************
 Created Date : 2020.12.07
      Creator : kang Dong Hee
   Decription : 2D BCR Data 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.07  NAME : Initial Created
  2022.07.11  강동희 : C20220603-000198 Cell ID 미 입력 시 DB Null 값 처리
  2023.01.02  이정미 : 설비 콤보박스 오류 수정 (NULL 추가)
  2023.01.13  이정미 : Multi Cell 입력 조회 가능한 Tab 추가
  2023.03.09  박승렬 : E20230223-000396 조회 시 다국어 처리
  2023.03.30  이윤중 : E20230317-000165 BCR Data 조회 조건 추가
  2023.10.11  홍석원 : ESNJ PROD_LOTID 조회 기능 추가, ESNJ 조회 DA 분기 처리
  2023.12.15  최도훈 : Header 컬럼 틀 고정, 순번 컬럼 추가
  2024.05.22  양윤호 : E20240523-001448 상/하 조회 조건 추가, 상/하 컬럼 추가
  2025.01.14  최도훈 : 콤보박스 다국어 처리
**************************************************************************************/
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
using C1.WPF.Excel;
using Microsoft.Win32;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_064 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        Util _Util = new Util();
        private bool bUseFlag = false; //2023.10.09 ESNJ 조회 호출 DA 분리 처리
        #endregion

        #region [Initialize]
        public FCS001_064()
        {
            InitializeComponent();

            #region Row Number 표시
            dg2dBCR.RowHeaderWidth = 24; // default : 24

            dg2dBCR.LoadedRowHeaderPresenter += (s, e) =>
            {
                try
                {
                    int topRows = ((C1DataGrid)s).Rows.TopRows.Count;
                    int bottomRows = ((C1DataGrid)s).Rows.BottomRows.Count;
                    int rows = ((C1DataGrid)s).Rows.Count;

                    if (e.Row.Index > topRows - 1 && e.Row.Index < rows - bottomRows)
                    {
                        TextBlock tb = new TextBlock();
                        tb.Text = (e.Row.Index - topRows + 1).ToString();
                        tb.TextAlignment = TextAlignment.Right;
                        tb.Margin = new Thickness(0, 0, 4, 0);

                        e.Row.HeaderPresenter.VerticalContentAlignment = VerticalAlignment.Center;
                        e.Row.HeaderPresenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        e.Row.HeaderPresenter.Content = tb;
                    }

                    int beforeRowInx = e.Row.Index - topRows;
                    int currRowInx = e.Row.Index - topRows + 1;

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        try
                        {
                            if (currRowInx >= 1 && ((int)Math.Log10(currRowInx) - 1) > 0)
                            {
                                dg2dBCR.RowHeaderWidth = 24 + ((int)Math.Log10(currRowInx) - 1) * 7;
                            }
                            else
                            {
                                dg2dBCR.RowHeaderWidth = 24;
                            }
                        }
                        catch
                        { }
                    }));
                }
                catch (Exception ex)
                {
                }
            };
            #endregion
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_064_SearchOnly2DBCR"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            //Combo Setting
            InitCombo();
            //Control Setting
            InitControl();

            if(bUseFlag)
            {
                tbProdLotId.Visibility = Visibility.Visible;
                txtProdLotId.Visibility = Visibility.Visible;
                dg2dBCR.Columns["VERIF_GRD_VALUE1"].Visibility = Visibility.Collapsed;
                dg2dBCR.Columns["PRINT_VALUE"].Visibility = Visibility.Collapsed;
                dg2dBCRList.Columns["VERIF_GRD_VALUE1"].Visibility = Visibility.Collapsed;
                dg2dBCRList.Columns["PRINT_VALUE"].Visibility = Visibility.Collapsed;
            } else
            {
                tbProdLotId.Visibility = Visibility.Collapsed;
                txtProdLotId.Visibility = Visibility.Collapsed;
                dg2dBCR.Columns["VERIF_GRD_VALUE1"].Visibility = Visibility.Visible;
                dg2dBCR.Columns["PRINT_VALUE"].Visibility = Visibility.Visible;
                dg2dBCRList.Columns["VERIF_GRD_VALUE1"].Visibility = Visibility.Visible;
                dg2dBCRList.Columns["PRINT_VALUE"].Visibility = Visibility.Visible;
            }

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            string[] sFilterEqp = { "5", null, null };
            ComCombo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilterEqp);

            string[] sFilterRework = { "COMBO_2D_BCD_RWK_COUNT" };
            ComCombo.SetCombo(cboRework, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilterRework, sCase: "CMN");

            DataTable dtResult = new DataTable(); //조회조건에 상/하 추가

            dtResult.Columns.Add("CBO_CODE", typeof(string));
            dtResult.Columns.Add("CBO_NAME", typeof(string));

            DataRow drSide = dtResult.NewRow();
            drSide["CBO_NAME"] = "T : "+ ObjectDic.Instance.GetObjectName("상");
            drSide["CBO_CODE"] = "T";

            dtResult.Rows.Add(drSide);

            drSide = dtResult.NewRow();
            drSide["CBO_NAME"] = "B : " + ObjectDic.Instance.GetObjectName("하");
            drSide["CBO_CODE"] = "B";

            dtResult.Rows.Add(drSide);

            cboSide.DisplayMemberPath = "CBO_NAME";
            cboSide.SelectedValuePath = "CBO_CODE";

            DataRow drAll = dtResult.NewRow();
            drAll["CBO_NAME"] = "-ALL-";
            drAll["CBO_CODE"] = "";

            dtResult.Rows.InsertAt(drAll, 0);

            cboSide.ItemsSource = dtResult.Copy().AsDataView();

            cboSide.SelectedIndex = 0;

 


        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpFromTime.DateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now;
            dtpToTime.DateTime = DateTime.Now;
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                Util.gridClear(dg2dBCR);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string)); //20230309 CSR:E20230223-000396  다국어처리
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("REWORK_CNT", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("BCR_DATA", typeof(string)); //20230327 BCR_DATA 조회 기능 추가
                dtRqst.Columns.Add("TB_FLAG", typeof(string));
                if (bUseFlag)
                {
                    dtRqst.Columns.Add("PROD_LOTID", typeof(string)); // 2023.09.25 Lot추가
                }

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;    //20230309 CSR:E20230223-000396  다국어처리
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm59");
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dr["REWORK_CNT"] = Util.GetCondition(cboRework, bAllNull: true);
                dr["TB_FLAG"] = Util.GetCondition(cboSide, bAllNull: true);
                //20220711_C20220603-000198 Cell ID 미 입력 시 DB Null 값 처리 START
                if (!string.IsNullOrEmpty(Util.NVC(txtCellId.Text)))
                {
                    dr["SUBLOTID"] = Util.NVC(txtCellId.Text);
                }
                else
                {
                    dr["SUBLOTID"] = DBNull.Value;
                }
                //20220711_C20220603-000198 Cell ID 미 입력 시 DB Null 값 처리 END

                if (!string.IsNullOrEmpty(Util.NVC(txtBcrVal.Text))
                   && txtBcrVal.Text.Length < 10)
                {
                    //BCR Data는 최소 10자리 이상 입력 해주세요. Please enter at least 10 digits for BCR Data.
                    Util.Alert("FM_ME_0474");
                    return;
                }

                if (!string.IsNullOrEmpty(Util.NVC(txtBcrVal.Text)))
                {
                    dr["BCR_DATA"] = Util.NVC(txtBcrVal.Text);
                }
                else
                {
                    dr["BCR_DATA"] = DBNull.Value;
                }


                // 2023.09.25 Lot추가
                if (bUseFlag)
                {
                    if (!string.IsNullOrEmpty(Util.NVC(txtProdLotId.Text)))
                    {
                        dr["PROD_LOTID"] = Util.NVC(txtProdLotId.Text);
                    }
                    else
                    {
                        dr["PROD_LOTID"] = DBNull.Value;
                    }
                }

                dtRqst.Rows.Add(dr);

                string bizName = "DA_SEL_2D_BCR";

                if (bUseFlag)
                {
                    bizName = "DA_SEL_2D_BCR_NJ";
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dg2dBCR, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            DataTable dtInfo = DataTableConverter.Convert(dataGrid.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            try
            {
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("SUBLOTID", typeof(string));
                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // CELL ID;
                            if (sheet.GetCell(rowInx, 0) == null)
                                return;

                            string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                            DataRow dataRow = dataTable.NewRow();
                            dataRow["SUBLOTID"] = CELL_ID;
                            dataTable.Rows.Add(dataRow);
                        }

                        if (dataTable.Rows.Count > 0)
                            dataTable = dataTable.DefaultView.ToTable(true);

                        Util.GridSetData(dataGrid, dataTable, FrameOperation);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtCellId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void txtProdLotId_KeyDown(object sender, KeyEventArgs e)
        {
            // 2023.09.25 Lot 추가
            if ((!string.IsNullOrEmpty(txtProdLotId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }
        private void txtBcrData_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtBcrVal.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnBcrVf_Click(object sender, RoutedEventArgs e)
        {
            FCS001_064_BCR_CHECK BCRCheck = new FCS001_064_BCR_CHECK();
            BCRCheck.FrameOperation = FrameOperation;

            if (BCRCheck != null)
            {
                object[] Parameters = new object[0];

                C1WindowExtension.SetParameters(BCRCheck, Parameters);

                BCRCheck.Closed += new EventHandler(BCRCheck_Closed);
                BCRCheck.ShowModal();
                BCRCheck.CenterOnScreen();
            }

        }

        private void BCRCheck_Closed(object sender, EventArgs e)
        {
            FCS001_064_BCR_CHECK Window = sender as FCS001_064_BCR_CHECK;
            if (Window.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dg2dBCRList);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            dg2dBCRList.ClearRows();
            DataGridRowAdd(dg2dBCRList, Convert.ToInt32(txtCellRow.Text));
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder cellIDs = new StringBuilder();

                DataTable dtRslt = new DataTable();
                for (int iRow = 2; iRow < dg2dBCRList.GetRowCount() + 2; iRow++)
                {
                    int indexCellCol = dg2dBCRList.Columns["SUBLOTID"].Index;
                    string sCellID = string.Empty;
                    string sTemp = Util.NVC(DataTableConverter.GetValue(dg2dBCRList.Rows[iRow].DataItem, "SUBLOTID"));
                    if (sTemp.Trim() == string.Empty)
                        break;

                    sCellID = sTemp;

                    //스프레드에 있는지 확인
                    if (iRow != 0)
                    {
                        for (int i = 0; i <= iRow - 1; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dg2dBCRList.Rows[i].DataItem, "SUBLOTID")).Equals(sCellID))
                            {
                                Util.MessageInfo("FM_ME_0287", new string[] { sCellID });  //[CELL ID : {0}]목록에 기존재하는 Cell 입니다.
                                continue;
                            }
                        }
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;

                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);

                    string bizName = "DA_SEL_2D_BCR_LIST_UI";

                    if (bUseFlag)
                    {
                        bizName = "DA_SEL_2D_BCR_LIST_UI_NJ";
                    }

                    //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                    DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync(bizName, "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                    if (dtRslt1.Rows.Count > 0)
                    {
                        dtRslt.Merge(dtRslt1);
                    }
                    else
                    {
                        cellIDs.Append(sTemp + ", ");
                    }
                }

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    dg2dBCRList.ClearRows();
                }
                else
                {
                    Util.GridSetData(dg2dBCRList, dtRslt, FrameOperation, true);
                }

                if (cellIDs.Length > 0)
                {
                    Util.MessageInfo(MessageDic.Instance.GetMessage("SFU1886") + "\r\n\r\n" + cellIDs.ToString(0, cellIDs.Length - 2));  //정보가 없습니다.
                }

                txtCellTotal.Text = dg2dBCRList.GetRowCount().ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dg2dBCRList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                dg?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (sender == null) return;
                    if (e.Cell.Presenter == null) return;

                    switch (e.Cell.Row.Type)
                    {
                        case DataGridRowType.Top:
                            e.Cell.Presenter.Foreground = Brushes.Black; ;
                            e.Cell.Presenter.Background = null;

                            if (e.Cell.Column.Name.ToString() == "SUBLOTID")
                            {
                                DataGridColumnHeaderPresenter colPre = e.Cell.Presenter.Content as DataGridColumnHeaderPresenter;
                                colPre.Foreground = Brushes.Red;

                            }

                            break;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
    }
}
