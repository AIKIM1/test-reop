/*************************************************************************************
 Created Date : 2019.12.09
      Creator : 신광희
   Decription : Carrier 사용자재 변경 등록
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.09  신광희 차장 : Initial Created.
  2024.05.24  김동일        [E20240418-001402] - Carrier 입력 시 복사 & 붙여넣기 기능 추가 및 Excel 업로드 기능 추가
**************************************************************************************/

using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;
using System.Linq;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using C1.WPF.Excel;
using System.IO;
using System.Configuration;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_036 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();

        public MCS001_036()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            DataTable bindingTable = Util.MakeDataTable(dgLotList, true);
            dgLotList.ItemsSource = DataTableConverter.Convert(bindingTable);

            txtCarrierId.Focus();
            Loaded -= UserControl_Loaded;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSave()) return;

                int iCnt = _util.GetDataGridRowCountByCheck(dgLotList, "CHK", true);
                if (iCnt > 1)
                {
                    // [%1]개 Carrier의 사용자재를 [%2]로 변경 하시겠습니까?
                    Util.MessageConfirm("SFU5218", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SaveUseMaterialsInfo();
                        }
                    }, iCnt, Util.NVC(cboCarrierType.SelectedValue));
                }
                else
                {
                    SaveUseMaterialsInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete()) return;

            //for (int i = dgLotList.GetRowCount() - 1; i >= 0; i--)
            //{
            //    DataRowView drv = dgLotList.Rows[i].DataItem as DataRowView;
            //    if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CHK").GetString() == "True")
            //    {
            //        dgLotList.RemoveRow(i);
            //    }
            //}

            DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);

            List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            if (drList.Count > 0)
            {
                foreach (DataRow dr in drList)
                {
                    dt.Rows.Remove(dr);
                }
                Util.GridSetData(dgLotList, dt, FrameOperation, true);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (!ValidationSelect()) return;

                SelectUseMaterialsInfo(txtCarrierId.Text);
                txtCarrierId.Text = string.Empty;
                txtCarrierId.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCarrierId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!ValidationSelect()) return;

                    if (!string.IsNullOrEmpty(txtCarrierId.Text.Trim()))
                    {
                        SelectUseMaterialsInfo(txtCarrierId.Text);
                        txtCarrierId.Text = string.Empty;
                        txtCarrierId.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgLotList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgLotList);
        }

        #endregion

        #region Mehod

        private void SelectUseMaterialsInfo(string carrierId)
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_CARRIER_DETAIL_INFO";

                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = carrierId;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(result))
                        {
                            if (result.Rows[0]["CSTSTAT"].GetString() == "U")
                            {
                                //실 Carrier는 사용자재 변경을 할 수 없습니다.
                                Util.MessageInfo("SFU8135", (messageresult) =>
                                {
                                    txtCarrierId.Focus();
                                });
                                return;
                            }

                            if (dgLotList.GetRowCount() > 0)
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "CSTTYPE")).Equals(Util.NVC(result.Rows[0]["CSTTYPE"])))
                                {
                                    // 동일한 캐리어 타입이 아닙니다.
                                    Util.MessageInfo("SFU4927", (messageresult) =>
                                    {
                                        txtCarrierId.Focus();
                                    });
                                    return;
                                }
                            }

                            //txtCarrierType.Text = result.Rows[0]["CSTTYPE_NAME"].GetString();
                            //txtCarrierProductName.Text = result.Rows[0]["CSTPROD_NAME"].GetString();
                            SetCarrierTypeCombo(cboCarrierType, result.Rows[0]["CSTTYPE"].GetString());

                            dgLotList.BeginNewRow();
                            dgLotList.EndNewRow(true);
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CHK", true);
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CSTID", result.Rows[0]["CSTID"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CSTTYPE", result.Rows[0]["CSTTYPE"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CSTPROD_NAME", result.Rows[0]["CSTPROD_NAME"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CSTPROD", result.Rows[0]["CSTPROD"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CSTSTAT", result.Rows[0]["CSTSTAT"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "LOTID", result.Rows[0]["LOTID"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "PROCID", result.Rows[0]["PROCID"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "RACK_ID", result.Rows[0]["RACK_ID"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "OUTER_CSTID", result.Rows[0]["OUTER_CSTID"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CURR_AREAID", result.Rows[0]["CURR_AREAID"].ToString());
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CST_DFCT_FLAG", result.Rows[0]["CST_DFCT_FLAG"].ToString());

                            DataTable dt = ((DataView)dgLotList.ItemsSource).Table;
                            Util.GridSetData(dgLotList, dt, null, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveUseMaterialsInfo()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_CARRIER_CSTPROD";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["CSTPROD"] = cboCarrierType.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inCarrierTable = ds.Tables.Add("INCST");
                inCarrierTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgLotList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True")
                    {
                        DataRow dr = inCarrierTable.NewRow();
                        dr["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                        inCarrierTable.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCST", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                        SetDataGridCheckHeaderInitialize(dgLotList);
                        Util.gridClear(dgLotList);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }

        private void SetCarrierTypeCombo(C1ComboBox cbo, string carrierType)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1", "ATTRIBUTE2" };
            string[] arrCondition = { LoginInfo.LANGID, "CSTPROD", carrierType, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private bool ValidationSelect()
        {
            if (CommonVerify.HasDataGridRow(dgLotList))
            {
                DataTable dt = ((DataView)dgLotList.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                    where t.Field<string>("CSTID") == txtCarrierId.Text
                    select t).ToList();

                if (query.Any())
                {
                    Util.MessageValidation("SFU2051", txtCarrierId.Text);
                    return false;
                }

                if (_util.GetDataGridRowCountByCheck(dgLotList, "CHK", true) >= 100)
                {
                    //한번에 최대 %1개 까지 처리 가능합니다.
                    Util.MessageValidation("SFU5015", 100);
                    return false;
                }

                return true;
            }
            return true;
        }

        private bool ValidationSave()
        {
            if (!CommonVerify.HasDataGridRow(dgLotList))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgLotList, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboCarrierType.SelectedValue == null || string.IsNullOrEmpty(cboCarrierType.SelectedValue.GetString()))
            {
                Util.MessageInfo("MCS0005", ObjectDic.Instance.GetObjectName("변경 사용자재"));
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgLotList, "CHK", true) > 100)
            {
                //한번에 최대 %1개 까지 처리 가능합니다.
                Util.MessageValidation("SFU5015", 100);
                return false;
            }
            
            return true;
        }

        private bool ValidationDelete()
        {
            if (!CommonVerify.HasDataGridRow(dgLotList))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgLotList, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
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


        #endregion

        private void btnDownloadForm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Carrier_Mgmt_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "CARRIER_ID";
                    sheet[1, 0].Value = "CST0000001";
                    sheet[2, 0].Value = "CST0000002";

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

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                
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
                        
                        if (sheet.GetCell(0, 0).Text != "CARRIER_ID")
                        {
                            //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                            Util.MessageValidation("SFU4424");
                            return;
                        }

                        DataTable excelImpTable = new DataTable();
                        excelImpTable.Columns.Add("CSTID", typeof(string));

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // CARRIER_ID 미입력시 return;
                            if (sheet.GetCell(rowInx, 0) == null)
                                continue;

                            DataRow dr = excelImpTable.NewRow();
                            dr["CSTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            excelImpTable.Rows.Add(dr);
                        }

                        if (excelImpTable.Rows.Count > 100)
                        {
                            //한번에 최대 %1개 까지 처리 가능합니다.
                            Util.MessageValidation("SFU5015", 100);
                            return;
                        }

                        Util.gridClear(dgLotList);

                        DataTable dtCarrierList = new DataTable();
                        dtCarrierList = SelectUseMaterialsInfo_Multi(excelImpTable);

                        if (dtCarrierList?.Rows?.Count > 0)
                            SetCarrierTypeCombo(cboCarrierType, Util.NVC(dtCarrierList.Rows[0]["CSTTYPE"]));

                        //if (dtCarrierList.Rows.Count < 1)
                        //{
                        //    //캐리어 정보가 없습니다.
                        //    Util.MessageValidation("SFU4564");
                        //    return;
                        //}

                        //if (dtCarrierList.Rows.Count > 0)
                        //    dtCarrierList = dtCarrierList.DefaultView.ToTable(true);

                        Util.GridSetData(dgLotList, dtCarrierList, FrameOperation, true);                        
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

        private DataTable SelectUseMaterialsInfo_Multi(DataTable dtExcel)
        {
            DataTable dtResult = new DataTable();

            try
            {
                if (dtExcel == null || dtExcel?.Rows?.Count < 1 || dtExcel?.Columns?.Contains("CSTID") == false) return dtResult;

                const string bizRuleName = "DA_MCS_SEL_CARRIER_DETAIL_INFO";

                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                for (int i = 0; i < dtExcel.Rows.Count; i++)
                {
                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = dtExcel.Rows[i]["CSTID"];
                    inTable.Rows.Add(dr);
                }

                dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                
                if (dtResult?.Columns?.Contains("CHK") == false) dtResult.Columns.Add("CHK", typeof(bool));

                if (dtResult?.Rows?.Count > 0)
                {
                    if (dtResult.Columns.Contains("CSTSTAT"))
                    {
                        DataRow[] drs = dtResult.Select("CSTSTAT = 'U'");
                        if (drs.Length > 0)
                        {
                            // 목록에 사용중인 Carrier가 [%1]건 존재 합니다. ([%2])
                            Util.MessageValidation("SFU5217", drs.Length.ToString(), Util.NVC(drs[0]["CSTID"]));

                            //dtResult = dtResult.Select("CSTSTAT <> 'U'").CopyToDataTable();
                            dtResult.Clear();
                            return dtResult;
                        }
                    }

                    if (dtResult.Columns.Contains("CSTTYPE") && dtResult.Columns.Contains("CSTID"))
                    {
                        var queryBase = dtResult.AsEnumerable().Select(row => new
                        {
                            CstID = row.Field<string>("CSTID"),
                            CstType = row.Field<string>("CSTTYPE"),
                        }).Distinct();

                        var query = dtResult.AsEnumerable().GroupBy(x => new
                        {
                            CstType = x.Field<string>("CSTTYPE"),
                        }).Select(g => new
                        {
                            CstType = g.Key.CstType,
                            CstID = queryBase.AsQueryable().Where(x => x.CstType == g.Key.CstType).Select(s => s.CstID).Max(),
                            Count = g.Count()
                        }).ToList();

                        if (query.Any() && query.Count > 1)
                        {
                            // 서로다른 Carrier 타입이 존재 합니다. ([%1]외 [%2]건)
                            Util.MessageInfo("SFU5219", Util.NVC(query[query.Count - 1].CstID), query[query.Count - 1].Count > 1 ? Util.NVC(query[query.Count - 1].Count - 1) : "0");

                            dtResult.Clear();
                            return dtResult;
                        }                        
                    }

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        dtResult.Rows[i]["CHK"] = true;
                    }
                }
                else
                {
                    //캐리어 정보가 없습니다.
                    Util.MessageValidation("SFU4564");
                }

                return dtResult;                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtResult;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void txtCarrierId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        txtCarrierId.Text = string.Empty;
                        Util.MessageValidation("SFU5015", "100");   //한번에 최대 %1개 까지 처리 가능합니다.
                        return;
                    }
                    
                    DataTable dtTemp = new DataTable();
                    dtTemp.Columns.Add("CSTID", typeof(string));

                    for (int inx = 0; inx < sPasteStrings.Length; inx++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[inx]))
                        {
                            break;
                        }

                        DataRow dr = dtTemp.NewRow();
                        dr["CSTID"] = sPasteStrings[inx];
                        dtTemp.Rows.Add(dr);                        
                    }
                    
                    DataTable dtRslt = new DataTable();
                    dtRslt = SelectUseMaterialsInfo_Multi(dtTemp);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                    {
                        return;
                    }

                    if (dgLotList.GetRowCount() > 1)
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "CSTTYPE")).Equals(Util.NVC(dtRslt.Rows[0]["CSTTYPE"])))
                        {
                            // 동일한 캐리어 타입이 아닙니다.
                            Util.MessageInfo("SFU4927", (messageresult) =>
                            {
                                txtCarrierId.Focus();
                            });
                            return;
                        }

                        if (dgLotList.GetRowCount() + dtRslt.Rows.Count > 100)
                        {
                            Util.MessageValidation("SFU5015", "100");   //한번에 최대 %1개 까지 처리 가능합니다.
                            return;
                        }

                        DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);
                        dtRslt.Merge(dt);

                        var query = dtRslt.AsEnumerable().GroupBy(x => new
                        {
                            CstID = x.Field<string>("CSTID"),
                        }).Where(g => g.Count() > 1).Select(g => new
                        {
                            CstID = g.Key.CstID,                            
                            Count = g.Count()
                        }).ToList();

                        if (query.Any() && query.Count > 0)
                        {
                            // 중복 데이터가 존재 합니다. %1
                            Util.MessageInfo("SFU2051", Util.NVC(query[0].CstID));

                            return;
                        }
                    }

                    if (dtRslt.Rows.Count > 0)
                        SetCarrierTypeCombo(cboCarrierType, Util.NVC(dtRslt.Rows[0]["CSTTYPE"]));

                    Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }
    }
}