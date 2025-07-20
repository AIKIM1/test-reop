/*************************************************************************************
 Created Date : 2017.03.17
      Creator : 정문교
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.17  정문교 : Initial Created.

 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using System.Configuration;
using C1.WPF.Excel;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_090 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre2 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        CheckBox chkAll2 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public COM001_090()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveNote);
            listAuth.Add(btnDelHist);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            Init();

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        void Init()
        {
            //C20210210-000208
            //엑셀 업로드시 그리드에 기본으로 데이터 셋이 지정되어 있어야 해서 DUMMY ID(없을거 같은 아무ID) 를 조회 쿼리로 보내서 빈 데이터가 오게 만듬
            GetLotList(true);
        }

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }
        #endregion

        #region [생성,삭제]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            string sMsgCode = string.Empty;

            if (((System.Windows.FrameworkElement)tbcElecPancake.SelectedItem).Name.Equals("Note"))
                sMsgCode = "SFU1241";               // 저장하시겠습니까??
            else
                sMsgCode = "SFU1230";               // 삭제 하시겠습니까?

            Util.MessageConfirm(sMsgCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });

        }
        #endregion

        #region [LOT ID]
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region [그리드 체크박스 클릭]
        private void CHK_Click(object sender, RoutedEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dgCurrent;
            dgCurrent = ((C1.WPF.DataGrid.DataGridCellPresenter)((System.Windows.FrameworkElement)sender).Parent).DataGrid;

            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            {
                DataRow[] drUnchk = DataTableConverter.Convert(dgCurrent.ItemsSource).Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    if (dgCurrent.Name.Equals("dgListNote"))
                    {
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.IsChecked = true;
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                    }
                    else
                    {
                        chkAll2.Checked -= new RoutedEventHandler(checkAll2_Checked);
                        chkAll2.IsChecked = true;
                        chkAll2.Checked += new RoutedEventHandler(checkAll2_Checked);
                    }
                }

            }
            else
            {
                if (dgCurrent.Name.Equals("dgListNote"))
                {
                    chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                    chkAll.IsChecked = false;
                    chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                }
                else
                {
                    chkAll2.Unchecked -= new RoutedEventHandler(checkAll2_Unchecked);
                    chkAll2.IsChecked = false;
                    chkAll2.Unchecked += new RoutedEventHandler(checkAll2_Unchecked);
                }
            }

        }
        #endregion

        #region [그리드 Column Header 로드시 Check ALL]
        private void dgListNote_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));

        }

        private void dgListHist_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre2.Content = chkAll2;
                        e.Column.HeaderPresenter.Content = pre2;
                        chkAll2.Checked -= new RoutedEventHandler(checkAll2_Checked);
                        chkAll2.Unchecked -= new RoutedEventHandler(checkAll2_Unchecked);
                        chkAll2.Checked += new RoutedEventHandler(checkAll2_Checked);
                        chkAll2.Unchecked += new RoutedEventHandler(checkAll2_Unchecked);
                    }
                }
            }));

        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListNote.ItemsSource == null)
                return;

            DataTable dt = ((DataView)dgListNote.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            dgListNote.ItemsSource = DataTableConverter.Convert(dt);
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListNote.ItemsSource == null)
                return;

            DataTable dt = ((DataView)dgListNote.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            dgListNote.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void checkAll2_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListHist.ItemsSource == null)
                return;

            DataTable dt = ((DataView)dgListHist.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            dgListHist.ItemsSource = DataTableConverter.Convert(dt);
        }
        private void checkAll2_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHist.ItemsSource == null)
                return;

            DataTable dt = ((DataView)dgListHist.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            dgListHist.ItemsSource = DataTableConverter.Convert(dt);
        }
        #endregion

        #endregion

        #region Mehod

        #region [대상목록 가져오기]
        public void GetLotList(bool pIsInit = false)
        {
            try
            {
                bool bNote = false;
                TextBox tb = new TextBox();

                if (((System.Windows.FrameworkElement)tbcElecPancake.SelectedItem).Name.Equals("Note"))
                    bNote = true;

                if (bNote)
                {
                    tb = txtLotIDNote;
                    /*
                    if (tb.Text.Trim().Length < 9)
                    {
                        // Lot ID는 9자리 이상 넣어 주세요.
                        Util.MessageInfo("SFU3608");
                        return;
                    }
                    */

                    //C20210210-000208
                    if (pIsInit == false && tb.Text.Trim().Length < 7)
                    {
                        // LOTID %1자리 이상 입력시 조회 가능합니다.
                        Util.MessageInfo("SFU4074", "7");
                        return;
                    }
                }
                else
                {
                    tb = txtLotIDHist;

                    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                    {
                        //Util.AlertInfo("SFU2042", new object[] { "31" });   //기간은 {0}일 이내 입니다.
                        Util.MessageValidation("SFU2042", "31");
                        return;
                    }
                }

                ShowLoadingIndicator();
                DoEvents();

                string sWipstat = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PANCAKE_ID", typeof(string));

                if (bNote.Equals(false))
                {
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("FROM_DATE", typeof(string));
                    inTable.Columns.Add("TO_DATE", typeof(string));
                    inTable.Columns.Add("PROCID", typeof(string));
                }

                DataRow dr = inTable.NewRow();
                if(pIsInit == false)
                {
                    dr["PANCAKE_ID"] = tb.Text;
                }
                else
                {
                    //C20210210-000208
                    //엑셀 업로드시 그리드에 기본으로 데이터 셋이 지정되어 있어야 해서 DUMMY ID(없을거 같은 아무ID) 를 조회 쿼리로 보내서 빈 데이터가 오게 만듬
                    dr["PANCAKE_ID"] = "DUMMY_XXXXXXXXXX";
                }
                
                if (bNote.Equals(false))
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom); ;
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                    dr["PROCID"] = Process.WINDING;
                }

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bNote ? "DA_BAS_SEL_TB_SFC_ELTR_LOT_NOTE" : "DA_BAS_SEL_TB_SFC_ELTR_LOT_NOTE_HIST", "INDATA", "OUTDATA", inTable);

                if (bNote)
                {
                    txtNote.Text = string.Empty;
                    chkAll.IsChecked = false;

                    Util.GridSetData(dgListNote, dtResult, FrameOperation, true);
                }
                else
                {
                    chkAll2.IsChecked = false;
                    Util.GridSetData(dgListHist, dtResult, FrameOperation,true);
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

        #endregion

        #region [특이사항 저장,삭제]
        private void Save()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                C1.WPF.DataGrid.C1DataGrid dgCurrent;
                string sSRCType = string.Empty;

                if (((System.Windows.FrameworkElement)tbcElecPancake.SelectedItem).Name.Equals("Note"))
                {
                    dgCurrent = dgListNote;
                    sSRCType = "INS";
                }
                else
                {
                    dgCurrent = dgListHist;
                    sSRCType = "DEL";
                }

                DataRow[] drLoop = DataTableConverter.Convert(dgCurrent.ItemsSource).Select("CHK = 1");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("PANCAKE_ID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                for (int nrow = 0; nrow < drLoop.Length; nrow++)
                {
                    DataRow dr = inTable.NewRow();

                    dr["SRCTYPE"] = sSRCType;
                    dr["PANCAKE_ID"] = drLoop[nrow]["PANCAKE_ID"];
                    dr["LOTID"] = drLoop[nrow]["PANCAKE_ID"];
                    dr["PROCID"] = Process.WINDING;
                    dr["INPUT_LOTID"] = drLoop[nrow]["PANCAKE_ID"];
                    dr["NOTE"] = sSRCType.Equals("INS") ? txtNote.Text : null;
                    dr["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_BAS_REG_TB_SFC_ELTR_LOT_NOTE", "INDATA", null, inTable);

                if (sSRCType.Equals("INS"))
                {
                    // 저장되었습니다.
                    Util.MessageInfo("SFU1270");
                }
                else
                {
                    // 삭제되었습니다.
                    Util.MessageInfo("SFU1273");
                }

                if (((System.Windows.FrameworkElement)tbcElecPancake.SelectedItem).Name.Equals("Note"))
                {
                    Util.gridClear(dgListNote);
                }
                else
                {
                    GetLotList();
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
        #endregion

        #region [Validation]
        private bool CanSave()
        {
            if (((System.Windows.FrameworkElement)tbcElecPancake.SelectedItem).Name.Equals("Note"))
            {
                DataRow[] drchk = DataTableConverter.Convert(dgListNote.ItemsSource).Select("CHK = 1");

                if (drchk.Length == 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtNote.Text))
                {
                    // 특이사항을 입력 하세요..
                    Util.MessageValidation("SFU1992");
                    return false;
                }

                for (int inx = 0; inx < dgListNote.GetRowCount(); inx++)
                {
                    string isChecked = Util.NVC(DataTableConverter.GetValue(dgListNote.Rows[inx].DataItem, "CHK"));
                    if ("1".Equals(isChecked)) {

                        string lotid = Util.NVC(DataTableConverter.GetValue(dgListNote.Rows[inx].DataItem, "PANCAKE_ID"));

                        if (String.IsNullOrEmpty(lotid) || lotid.Length != 10)
                        {
                            Util.MessageValidation("SFU4045");  //LOT ID는 10자리이며, 숫자 또는 영문대문자만 입력 가능합니다.
                            return false;
                        }

                        DataRow[] dupLot = DataTableConverter.Convert(dgListNote.ItemsSource).Select("CHK = 1 AND PANCAKE_ID = '" + lotid + "'");

                        if (dupLot.Length >= 2)
                        {
                            Util.MessageValidation("SFU1376", lotid);  //LOT ID는 중복 입력할수 없습니다.(%1)
                            return false;
                        }
                    }
                }
            }
            else
            {
                DataRow[] drchk = DataTableConverter.Convert(dgListHist.ItemsSource).Select("CHK = 1");

                if (drchk.Length == 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }
            }

            return true;
        }
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            for (int addrow = 0; addrow < txtRowCnt.Value; addrow++)    //C20210210-000208
            {
                AddDataGridRow(dgListNote);
            }
        }

        private void AddDataGridRow(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();

                if (dg.ItemsSource != null)
                {
                    dt = DataTableConverter.Convert(dg.ItemsSource);
                }
                else
                {
                    dt.Columns.Add("CHK", typeof(Boolean));
                    dt.Columns.Add("PANCAKE_ID", typeof(String));
                    dt.Columns.Add("PRJT_NAME", typeof(String));
                    dt.Columns.Add("PRODID", typeof(String));
                    dt.Columns.Add("PRODNAME", typeof(String));
                    dt.Columns.Add("MODLID", typeof(String));
                    dt.Columns.Add("WIPQTY", typeof(Decimal));
                    dt.Columns.Add("UNIT_CODE", typeof(String));
                    dt.Columns.Add("LOTDTTM_CR", typeof(String));
                    dt.Columns.Add("EDITABLE", typeof(String));
                }

                DataRow dr = dt.NewRow();
                dr["CHK"] = 0;
                dr["EDITABLE"] = "Y";

                dt.Rows.Add(dr);
                dt.AcceptChanges();
                dg.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgListNote_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type.Equals(DataGridRowType.Top) || e.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Column.Name.Equals("PANCAKE_ID"))
            {
                if (!Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "EDITABLE")).Equals("Y"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DeleteNoteValidation())
                {
                    return;
                }

                //삭제하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DeleteNote();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteNote()
        {
            DataTable dtInfo = DataTableConverter.Convert(dgListNote.ItemsSource);

            List<DataRow> drInfo = dtInfo.Select("CHK = 1")?.ToList();
            foreach (DataRow dr in drInfo)
            {
                dtInfo.Rows.Remove(dr);
            }
            Util.GridSetData(dgListNote, dtInfo, FrameOperation, true);
        }

        private bool DeleteNoteValidation()
        {
            DataRow[] drchk = DataTableConverter.Convert(dgListNote.ItemsSource).Select("CHK = 1");

            if (drchk.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private void dgListNote_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (dgListNote.Rows[e.Cell.Row.Index] == null)
                return;

            if (e.Cell.Column.IsReadOnly == false)
            {
                DataTableConverter.SetValue(dgListNote.Rows[e.Cell.Row.Index].DataItem, "CHK", 1);

            }
        }

        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }

        void GetExcel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                // 해더 제외
                DataTable dt = DataTableConverter.Convert(dgListNote.ItemsSource).Clone();

                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                {
                    DataRow dr = dt.NewRow();

                    dr["CHK"] = 0;

                    if (sheet.GetCell(rowInx, 0) == null)
                    {
                        dr["PANCAKE_ID"] = "";
                    }
                    else
                    {
                        dr["PANCAKE_ID"] = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                    }

                    dr["EDITABLE"] = "Y";

                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();
                Util.GridSetData(dgListNote, dt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
    }
}
