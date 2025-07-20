/*************************************************************************************
 Created Date : 2023.01.12
      Creator : 김린겸
   Decription : Cell Pass FCS - REGISTER
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.12 김린겸 : 최초생성
  2023.01.31 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins
  2023.02.01 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins - Control Enable 속성 제어
  2023.02.03 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins

**************************************************************************************/

using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Configuration;
using Microsoft.Win32;
using System.Windows.Threading;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_BOX_FORM_CELL_PASS_FCS_REGISTER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private string _USERID = string.Empty;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = true,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public CMM_BOX_FORM_CELL_PASS_FCS_REGISTER()
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

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        /// 
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _USERID = Util.NVC(tmps[0]) as string;
        }
        #endregion

        #region Event

        private void dgListLoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #region  체크박스 선택 이벤트     
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                if (dgList.Rows.Count == 0)
                {
                    Util.MessageInfo("SFU8896", "Cell ID"); //[%1]을(를) 스캔하거나 입력하십시오.
                    return;
                }

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("ACTID");
                INDATA.Columns.Add("USERID");
                INDATA.Columns.Add("NOTE");

                DataTable INDATA_CELL = indataSet.Tables.Add("INDATA_CELL");
                INDATA_CELL.Columns.Add("SUBLOTID");

                DataRow dr = INDATA.NewRow();
                dr["ACTID"] = "REG_FCS_PASS";
                dr["USERID"] = _USERID;
                dr["NOTE"] = txtREG_RSN_CNTT.Text;
                INDATA.Rows.Add(dr);

                DataTable dt = ((DataView)dgList.ItemsSource).Table;

                DataRow newrow;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    newrow = INDATA_CELL.NewRow();
                    newrow["SUBLOTID"] = dt.Rows[i]["CELLID"].ToString();
                    INDATA_CELL.Rows.Add(newrow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_N_CANCEL_FCS_PASS", "INDATA,INDATA_CELL", null, indataSet);

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
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

        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sCellID = txtCellID.Text.ToString();
                //sCellID.Trim();   //입력된 값 그대로 처리하도록 주석.
                //if (string.IsNullOrEmpty(sCellID) || string.IsNullOrWhiteSpace(sCellID))
                //{
                //    Util.MessageValidation("FM_ME_0256", (result) =>
                //    {
                //        //if (result == MessageBoxResult.OK)
                //        {
                //            txtCellID.Text = String.Empty;
                //            txtCellID.Focus();
                //        }
                //    }); //항목에 빈값이 있습니다.
                //    return;
                //}

                if (dgList.Rows.Count == 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CHK");
                    dt.Columns.Add("CELLID");

                    DataRow dr = dt.NewRow();
                    dr["CHK"] = true;
                    dr["CELLID"] = sCellID;
                    dt.Rows.Add(dr);

                    dgList.ItemsSource = DataTableConverter.Convert(dt);
                }
                else
                {
                    DataTable dt = ((DataView)dgList.ItemsSource).Table;
                    //string sCellID = txtCellID.Text.ToString();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["CELLID"].ToString() == sCellID)
                        {
                            Util.MessageValidation("SFU8894", (result) =>
                            {
                                //if (result == MessageBoxResult.OK)
                                {
                                    txtCellID.Focus();
                                    txtCellID.SelectAll();
                                }
                            }, "Cell ID", sCellID);  //[%1]이(가) 이미 존재 합니다.[%2]
                            return;
                        }
                    }

                    DataRow dr = dt.NewRow();
                    dr["CHK"] = true;
                    dr["CELLID"] = sCellID;
                    dt.Rows.Add(dr);
                }
                txtCellID.Text = String.Empty;
                txtCellID.Focus();
            }
        }

        /// <summary>
        /// 엑셀 업로드 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Control_Enable(false);
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "CELL_PASS_FCS_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "CELLID";
                    //sheet[0, 1].Value = "Tray 위치";
                    sheet[0, 0].Style = sheet[0, 1].Style = styel;
                    
                    sheet.Columns[0].Width = 6000;
                    //sheet.Columns[1].Width = 1500;

                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
                Control_Enable(true);
            }
            catch (Exception ex)
            {
                Control_Enable(true);
                Util.MessageException(ex);
            }
        }

        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            Control_Enable(false);
            GetExcel();
            Control_Enable(true);
        }

        private void GetExcel()
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

        private void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                ArrayList boxList = new ArrayList();

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                if (sheet.Rows.Count <= 1)
                {
                    Util.MessageValidation("SFU1498");  //데이터가 없습니다.
                    return;
                }

                //헤더(0) 번째는 제외. 데이터는 1번째 부터로 인식한다.
                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                {
                    string sCellID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                    //sCellID.Trim();   //입력된 값 그대로 처리하도록 주석.

                    if (string.IsNullOrEmpty(sCellID) || string.IsNullOrWhiteSpace(sCellID))
                    {
                        Util.MessageValidation("SFU8893", "Cell ID", "ROW");  //[%1]이(가) 입력되지 않은 [%2]이(가) 있습니다.
                        return;
                    }

                    if (dgList.GetRowCount() > 0)
                    {
                        for (int inx = 0; inx < dgList.GetRowCount(); inx++)
                        {
                            if (DataTableConverter.GetValue(dgList.Rows[inx].DataItem, "CELLID").ToString() == sCellID)
                            {
                                Util.MessageValidation("SFU8894", "Cell ID", sCellID);  //[%1]이(가) 이미 존재 합니다.[%2]
                                return;
                            }
                        }
                    }

                    for (int rowInx2 = 1; rowInx2 < sheet.Rows.Count; rowInx2++)
                    {
                        if (rowInx == rowInx2)   //동일한 데이터는 중복 판정 제외
                        {
                            continue;
                        }

                        string sCellID2 = Util.NVC(sheet.GetCell(rowInx2, 0).Text);
                        //sCellID2.Trim();   //입력된 값 그대로 처리하도록 주석.
                        if (sCellID == sCellID2)
                        {
                            Util.MessageValidation("SFU8895", "Cell ID", sCellID);  //입력할 파일에 동일한 [%1]가 존재합니다.[%2]
                            return;
                        }
                    }
                    boxList.Add(sCellID);
                }


                for (int i = 0; i < boxList.Count; i++)
                {
                    if (dgList.Rows.Count == 0)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("CHK");
                        dt.Columns.Add("CELLID");

                        DataRow dr = dt.NewRow();
                        dr["CHK"] = true;
                        dr["CELLID"] = boxList[i].ToString();
                        dt.Rows.Add(dr);

                        dgList.ItemsSource = DataTableConverter.Convert(dt);
                    }
                    else
                    {
                        DataTable dt = ((DataView)dgList.ItemsSource).Table;
                        DataRow dr = dt.NewRow();
                        dr["CHK"] = true;
                        dr["CELLID"] = boxList[i].ToString();
                        dt.Rows.Add(dr);
                    }
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

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                return;
            }

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]))
                    dt.Rows[i].Delete();
        }

        private void Control_Enable(bool bEnable)
        {
            btnDownLoad.IsEnabled = bEnable;
            btnExcelUpload.IsEnabled = bEnable;
            btnSave.IsEnabled = bEnable;
            btnDelete.IsEnabled = bEnable;
            btnClose.IsEnabled = bEnable;
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
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
    }
}
