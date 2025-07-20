/*************************************************************************************
 Created Date : 2021.07.26
      Creator : 송교진
   Decription : PALLET Hold 해제(Excel) 기능 추가 
--------------------------------------------------------------------------------------
 [Change History]
  2021.07.26  송교진 : Initial Created.
  2024.07.22 유재홍                        BOX001_333로 복사 후 UNHOLD_CODE추가
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_213_RELEASE_BOX_EXCL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_333_RELEASE_BOX_EXCL : C1Window, IWorkArea
    {
        private Util _Util = new Util();

        private DataTable _DtSearchInfo = new DataTable();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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

        public BOX001_333_RELEASE_BOX_EXCL()
        {
            InitializeComponent();
            InitCombo();
        }

        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { "UNHOLD_LOT" };
            _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);


        }


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Initialized(object sender, System.EventArgs e)
        {

        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            InitControl();
        }

        private void btnDownLoad_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();
                
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Release_BOX_HOLD_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "BOXID";
                    sheet[1, 0].Value = "PB23K1B264";
                    sheet[2, 0].Value = "G86AI051019494";

                    sheet[0, 0].Style = styel;
                    sheet.Columns[0].Width = 1500;

                    c1XLBook1.Save(od.FileName);
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnUpLoad_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                dtInfo.Clear();

                _DtSearchInfo.Clear();

                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        if (sheet.GetCell(0, 0).Text != "BOXID")
                        {
                            //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                            Util.MessageValidation("SFU4424");
                            return;
                        }

                        // Pallet BCD 컬럼 활성화 상태일 경우만 getPalletBCD 호출 (boxid, pallet bcd 값 적용)
                        bool _chkPalletbcd = false;
                        if (dgHold.Columns["PLLT_BCD_ID"].Visibility == Visibility.Visible)
                            _chkPalletbcd = true;

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // BOXID 미입력시 return;
                            if (sheet.GetCell(rowInx, 0) == null)
                                return;

                            DataRow dr = dtInfo.NewRow();
                            dr["CHK"] = true;
                            // Pallet BCD 조회
                            string _boxid = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            if (_chkPalletbcd)
                            {
                                dr["BOXID"] = getPalletBCD(_boxid);
                            }
                            else
                            {
                                dr["BOXID"] = _boxid;
                            }
                            //dr["BOXID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            dr["BOXTYPE"] = "PLT";
                            dtInfo.Rows.Add(dr);
                        }

                        DataTable dtBox = new DataTable();

                        dtBox = GetPalletInfo_EXCEL(dtInfo);

                        if (dtBox.Rows.Count < 1)
                        {
                            //입력한 Pallet가 존재하지 않습니다. 
                            Util.MessageValidation("SFU3394");
                            return;
                        }

                        Util.GridSetData(dgHold, dtBox, FrameOperation);
                        tbTotCount.Text = dgHold.GetRowCount().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
            //dt.Columns["ASSY_LOTID"].AllowDBNull = true;
            DataRow dr = dt.NewRow();

            dt.Rows.Add(dr);

            Util.GridSetData(dgHold, dt, FrameOperation);
            dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        /// <summary>
        /// HOLD 리스트 제외
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);

            List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            if (drList.Count > 0)
            {
                foreach (DataRow dr in drList)
                {
                    dt.Rows.Remove(dr);
                }
                Util.GridSetData(dgHold, dt, FrameOperation);
                chkAll.IsChecked = false;
            }
            tbTotCount.Text = dgHold.GetRowCount().ToString();
        }


        private void dgHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtNote.Text))
                {
                    //SFU4301		Hold 해제 사유를 입력하세요.	
                    Util.MessageValidation("SFU4301");
                    return;
                }

                if (_Util.GetDataGridCheckFirstRowIndex(dgHold, "CHK") < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return;
                }
                
                // SFU4046 HOLD 해제 하시겠습니까?
                Util.MessageConfirm("SFU4046", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                }
            }
        }


        private void InitControl()
        {
            _DtSearchInfo = new DataTable();

            for (int i = 0; i < dgHold.Columns.Count; i++)
            {
                _DtSearchInfo.Columns.Add(dgHold.Columns[i].Name);
            }

            Util.GridSetData(dgHold, _DtSearchInfo, FrameOperation);

            _DtSearchInfo.Clear();

            // 팔레트 바코드 표시 여부
            isVisibleBCD(LoginInfo.CFG_AREA_ID);
        }

        private void Save()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("UNHOLD_NOTE");
                inDataTable.Columns.Add("SHOPID");
                inDataTable.Columns.Add("UNHOLD_CODE");

                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("BOXID");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_NOTE"] = txtNote.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["UNHOLD_CODE"] = Util.GetCondition(cboResnCode, "SFU1593"); //사유는필수입니다. >> 사유를 선택하세요.
                if (newRow["UNHOLD_CODE"].Equals(""))
                {
                    return;
                }


                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {
                    newRow = inHoldTable.NewRow();
                    newRow["BOXID"] = dtInfo.Rows[row]["BOXID"]; 
                    inHoldTable.Rows.Add(newRow);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ASSY_UNHOLD_PALLET", "INDATA,INHOLD", null, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }            
        }

        private DataTable GetPalletInfo_EXCEL(DataTable dtExcel)
        {
            try
            {
                DataTable dtResult = new DataTable();
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "INDATA", "OUTDATA", dtExcel);
                dtResult.Columns.Add("CHK");


                if (dtResult?.Rows?.Count > 0 && dtResult.Rows[0]["BOXTYPE"].Equals("PLT"))
                {
                    for(int i=0; i<dtResult.Rows.Count; i++)
                    {
                        dtResult.Rows[i]["CHK"] = true;
                    }
                    return dtResult;
                }

                dtResult.Clear();
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void DgHold_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (Util.NVC(e?.Cell?.Column?.Name) == "BOXID")
            {

                dataGrid.EndEdit();
                dataGrid.EndEditRow(true);

                string sBoxID = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["BOXID"].Index)?.Value);

                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                {
                    if (i == e.Cell.Row.Index || dataGrid.GetCell(i, dataGrid.Columns["BOXID"].Index).Value == null)
                        continue;
                    if (dataGrid.GetCell(i, dataGrid.Columns["BOXID"].Index).Value.Equals(sBoxID))
                    {
                        //이미 조회된 Pallet ID 입니다.
                        Util.MessageValidation("SFU3165");
                        return;
                    }
                }

                DataTable dtRslt = GetPalletInfo(sBoxID);

                if (dtRslt.Rows.Count < 1)
                {
                    //입력한 Pallet가 존재하지 않습니다. 
                    Util.MessageValidation("SFU3394");
                    return;
                }

                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRODID", Util.NVC(dtRslt.Rows[0]["PRODID"]));
                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PACK_EQSGID", Util.NVC(dtRslt.Rows[0]["PACK_EQSGID"]));
                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MDLLOT_ID", Util.NVC(dtRslt.Rows[0]["MDLLOT_ID"]));
                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRJT_NAME", Util.NVC(dtRslt.Rows[0]["PRJT_NAME"]));
                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TOTAL_QTY", Util.NVC(dtRslt.Rows[0]["TOTAL_QTY"]));
                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ISS_HOLD_FLAG", Util.NVC(dtRslt.Rows[0]["ISS_HOLD_FLAG"]));
                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "BOXTYPE", Util.NVC(dtRslt.Rows[0]["BOXTYPE"]));

                // 팔레트 바코드 
                if (dgHold.Columns.Contains("PLLT_BCD_ID"))
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PLLT_BCD_ID", Util.NVC(dtRslt.Rows[0]["PLLT_BCD_ID"]));

                dataGrid.UpdateLayout();
                tbTotCount.Text = dataGrid.GetRowCount().ToString();
            }

        }
        private DataTable GetPalletInfo(string sBoxID)
        {
            try
            {
                DataTable dtResult = new DataTable();

                if (sBoxID.Trim().Equals(""))
                {
                    dtResult.Clear();
                    return dtResult;
                }
                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("BOXTYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["BOXID"] = sBoxID;
                dr["BOXTYPE"] = "PLT";
                inTable.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "INDATA", "OUTDATA", inTable);

                if (dtResult?.Rows?.Count > 0 && dtResult.Rows[0]["BOXTYPE"].Equals("PLT"))
                {
                    return dtResult;
                }

                dtResult.Clear();
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        // 팔레트바코드ID -> BoxID
        private string getPalletBCD(string palletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("CSTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = palletid;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult != null && SearchResult.Rows.Count > 0)
                {
                    return Util.NVC(SearchResult.Rows[0]["CURR_LOTID"]);
                }
                return palletid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        /// <summary>
        /// 팔레트 바코드 표시 여부
        /// </summary>
        /// <param name="sAreaID"></param>
        private void isVisibleBCD(string sAreaID)
        {
            // 팔레트 바코드 표시 설정
            if (_Util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                if (dgHold.Columns.Contains("PLLT_BCD_ID"))
                    dgHold.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgHold.Columns.Contains("PLLT_BCD_ID"))
                    dgHold.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
            }
        }
    }
}
