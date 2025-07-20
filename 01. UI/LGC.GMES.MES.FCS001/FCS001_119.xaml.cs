/*************************************************************************************
 Created Date : 2021.11.02
      Creator : 
   Decription : Pallet 라벨 발행(2D)
--------------------------------------------------------------------------------------
 [Change History]
  2021.11.02  강동희 : Initial Created.
  2022.11.29  강신영 : 재발생시  이력관리  추가 및  Line Feed 적용
  2023.02.21  조영대 : 재발행여부 및 발행정보 추가, 발행 이력 목록 추가
  2023.10.16  최도훈 : 조회 조건에 동 필터 추가 요청 반영
  2023.10.25  최도훈 : 라벨 발행 이력을 신규발행/재발행 구분해서 남기도록 수정. Printer 설정 값이 없는 경우 알림창 추가
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_119 : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]

        public FCS001_119()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }
        
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter1 = { "CSTOWNER" };
            _combo.SetCombo(cboCstOwner, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { "CSTPROD" };
            _combo.SetCombo(cboCstProd, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
        }
        #endregion

        #region [Event]
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPalletID.Text.Trim()))
            {
                if (!rdoPublishNo.IsEnabled) rdoPublishNo.IsEnabled = true;
                if (!rdoPublishYes.IsEnabled) rdoPublishYes.IsEnabled = true;
            }
            else
            {
                if (rdoPublishNo.IsEnabled) rdoPublishNo.IsEnabled = false;
                if (rdoPublishYes.IsEnabled) rdoPublishYes.IsEnabled = false;
            }
        }

        private void btnOnePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintClick(1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTwoPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintClick(2);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnForePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintClick(4);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        
        private void rdoPublish_Click(object sender, RoutedEventArgs e)
        {
            if (rdoPublishYes.IsChecked.Equals(true))
            {
                dgPalletList.IsCheckAllColumnUse = false;
            }
            else
            {
                dgPalletList.IsCheckAllColumnUse = true;
            }

            btnSearch.PerformClick();
        }

        private void dgPalletList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (e.Exception == null)
            {
                if (dgPalletList.Rows.Count > 0)
                {
                    dgPalletList.SelectRow(0);
                }
            }
            gdCondition.IsEnabled = true;
        }

        private void dgPalletList_RowIndexChanged(object sender, int beforeRow, int currentRow)
        {
            dgPalletDetlList.ClearRows();
            
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("CMCDTYPE", typeof(string));
            dtRqst.Columns.Add("ATTRIBUTE1", typeof(string));
            dtRqst.Columns.Add("CSTTYPE", typeof(string));
            dtRqst.Columns.Add("CSTPROD", typeof(string));
            dtRqst.Columns.Add("CSTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "CSTPROD";
            dr["ATTRIBUTE1"] = "PT";
            dr["CSTTYPE"] = "PT";
            dr["CSTPROD"] = Util.GetCondition(cboCstProd, bAllNull: true);
            dr["CSTID"] = dgPalletList.GetStringValue(currentRow, "CSTID");
            dtRqst.Rows.Add(dr);

            dgPalletList.Cursor = Cursors.No;


            // Background 처리 완료시 dgPalletDetlList_ExecuteDataCompleted 이벤트 호출
            dgPalletDetlList.ExecuteService("DA_SEL_PRINT_PALLET_DETL_LIST", "RQSTDT", "RSLTDT", dtRqst, true);
        }
        
        private void dgPalletList_CheckAllChanged(object sender, bool isCheck, RoutedEventArgs e)
        {
            this.ClearValidation();

            if (isCheck)
            {
                for (int row = 0; row < dgPalletList.Rows.Count; row++)
                {
                    if (dgPalletList.GetValue(row, "PRINT_FLAG").Equals("Y"))
                    {
                        dgPalletList.SetRowValidation(row, "SFU2928");
                        dgPalletList.SetValue(row, "CHK", false);
                    }
                }

            }
        }

        private void dgPalletDetlList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            dgPalletList.Cursor = Cursors.Arrow;
        }

        #endregion

        #region [Method]

        private void GetList()
        {
            try
            {
                this.ClearValidation();
                dgPalletList.ClearRows();
                dgPalletDetlList.ClearRows();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRqst.Columns.Add("CSTTYPE", typeof(string));
                dtRqst.Columns.Add("CSTOWNER", typeof(string));
                dtRqst.Columns.Add("CSTPROD", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("ROW_CNT", typeof(Int64));
                dtRqst.Columns.Add("PRINT_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CSTPROD";
                dr["ATTRIBUTE1"] = "PT";
                dr["CSTTYPE"] = "PT";
                dr["CSTOWNER"] = Util.GetCondition(cboCstOwner, bAllNull: true);
                dr["CSTPROD"] = Util.GetCondition(cboCstProd, bAllNull: true);
                dr["CSTID"] = string.IsNullOrEmpty(Util.NVC(txtPalletID.Text)) ? null : Util.NVC(txtPalletID.Text);
                dr["ROW_CNT"] = numSelQty.Value;
                if (string.IsNullOrEmpty(txtPalletID.Text.Trim()))
                {
                    if (rdoPublishNo.IsChecked.Equals(true))
                    {
                        dr["PRINT_FLAG"] = "N";
                    }
                    else if (rdoPublishYes.IsChecked.Equals(true))
                    {
                        dr["PRINT_FLAG"] = "Y";
                    }
                }
                dtRqst.Rows.Add(dr);

                gdCondition.IsEnabled = false;

                // 백그라운드 처리. 처리완료 후 dgPalletList_QueryDataCompleted 이벤트 실행
                dgPalletList.ExecuteService("DA_SEL_PRINT_PALLET_LIST", "RQSTDT", "RSLTDT", dtRqst, true);

            }
            catch (Exception ex)
            {
                gdCondition.IsEnabled = false;
                Util.MessageException(ex);
            }
        }

        private void PrintClick(int PrintCnt)
        {            
            try
            {
                this.ClearValidation();
                
                if (!CommonVerify.HasDataGridRow(dgPalletList)) return;
                if (!dgPalletList.IsCheckedRow("CHK")) return;

                List<int> chkRow = dgPalletList.GetCheckedRowIndex("CHK");
                bool isPrint = false;
                foreach (int row in chkRow)
                {
                    DataRow dr = dgPalletList.GetDataRow(row);
                    if (dr != null && dr["PRINT_FLAG"].Equals("Y"))
                    {
                        dgPalletList.SetRowValidation(row, "SFU2928");
                        isPrint = true;
                    }
                }

                if (isPrint)
                {
                    Util.MessageConfirm("FM_ME_0471", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable dt = ((DataView)dgPalletList.ItemsSource).Table;
                            var query = (from t in dt.AsEnumerable()
                                         where t.Field<string>("CHK") == "True"
                                         select t).ToList();

                            if (CommonVerify.HasDataGridRow(dgPalletList) && query.Any())
                            {
                                PrintLabel(query, PrintCnt);
                            }
                        }
                    });
                }
                else
                {
                    DataTable dt = ((DataView)dgPalletList.ItemsSource).Table;
                    var query = (from t in dt.AsEnumerable()
                                 where t.Field<string>("CHK") == "True"
                                 select t).ToList();

                    if (CommonVerify.HasDataGridRow(dgPalletList) && query.Any())
                    {
                        Util.MessageConfirm("SFU1540", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                PrintLabel(query, PrintCnt);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintLabel(List<DataRow> query, int PrintCnt)
        {
            try
            {
                const string bizRuleName = "DA_SEL_LABEL_PRINT_BY_TRAYID";

                const string item001 = "ITEM001";
                const string item002 = "ITEM002";
                const string item003 = "ITEM003";

                string labelCode = "LBL0294";//2D BarCode

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LABEL_CODE", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LABEL_CODE"] = labelCode;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(result))
                    {
                        // 발행여부와 상관없이 이력을 남길때
                        //foreach (var item in query)
                        //{
                        //    // 발행이력 추가
                        //    string IssueID = item["CSTID"].GetString();     // 선택된 항목의 Pallet ID(CSTID)  Items[0].ItemArray[1]
                        //    string IssueIDHeader = IssueID.Substring(0, 4); // IssueID.Substring(0, 4);
                        //    string IssueFlag = "R";                         // I이면 신규발행, R이면 재발행

                        //    InserIssueHist(IssueID, IssueFlag, PrintCnt.ToString(), IssueIDHeader, LoginInfo.USERID);
                        //}

                        if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                        {
                            Util.MessageInfo("SFU5006");  //현재 바코드 설정값이 존재하지 않습니다.(Setting -> 프린터 -> 바코드 설정을 확인하세요)
                            return;
                        }

                        foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()))
                            {
                                // Setting 에 설정된 BarCord Print 정보를 통하여 일치하는 zpl 코드를 가져옴
                                string resolution = dr["DPI"].GetString();
                                string printmodel = dr["PRINTERTYPE"].GetString();
                                string portName = dr["PORTNAME"].GetString();

                                var zplText = (from t in result.AsEnumerable()
                                               where t.Field<string>("PRTR_RESOL_CODE") == resolution
                                                     && t.Field<string>("PRTR_MDL_ID") == printmodel
                                               select new { zplCode = t.Field<string>("DSGN_CNTT") }).FirstOrDefault();

                                if (zplText != null)
                                {
                                    foreach (var item in query)
                                    {
                                        string sITEM001 = string.Empty;
                                        string sITEM002 = string.Empty;
                                        string sITEM003 = string.Empty;
                                        string zplCode = string.Empty;

                                        sITEM001 = item["CSTID"].GetString();
                                        sITEM002 = item["CSTPROD_NAME"].GetString();
                                        sITEM003 = item["CSTID"].GetString();

                                        zplCode =
                                        zplText.zplCode.Replace(item001, sITEM001)
                                            .Replace(item002, sITEM002).Replace(item003, sITEM003);

                                        for (int Cnt = 0; Cnt < PrintCnt; Cnt++)
                                        {
                                            if (Cnt == (PrintCnt - 1))
                                            {
                                                if (chkFeed.IsChecked == true)
                                                {
                                                    string sFeedZpl = " ^XA^A0N,0,0^FO20,20^FD ^FS^PQ1,0,1,Y^XZ";
                                                    zplCode = zplCode + sFeedZpl;
                                                }
                                            }

                                            bool iszplPrint = portName.ToUpper().Equals("USB")
                                                ? FrameOperation.Barcode_ZPL_USB_Print(zplCode)
                                                : FrameOperation.Barcode_ZPL_Print(dr, zplCode);

                                            if (iszplPrint == false)
                                            {
                                                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print Fail"));
                                                return;
                                            }
                                            System.Threading.Thread.Sleep(500);
                                        }

                                        // 발행이력 추가 - 성공한 프린트일 경우에만 이력을 저장한다.
                                        string IssueID = sITEM001;                      // 선택된 항목의 Pallet ID(CSTID)  Items[0].ItemArray[1]
                                        string IssueIDHeader = IssueID.Substring(0, 4); // IssueID.Substring(0, 4);
                                        string IssueFlag = rdoPublishNo.IsChecked.Equals(true) ? "I" : "R";                         // I이면 신규발행, R이면 재발행

                                        InserIssueHist(IssueID, IssueFlag, PrintCnt.ToString(), IssueIDHeader, LoginInfo.USERID);
                                    }
                                }
                                else
                                {
                                    FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print Fail"));
                                    return;
                                }
                            }

                            btnSearch.PerformClick();

                            Util.MessageInfo("FM_ME_0126");  //라벨 발행을 완료하였습니다.
                        }
                    }
                    else
                    {
                        Util.MessageInfo("SFU4089");  //해당 제품에 대한 라벨 기준정보가 없습니다. (labelCode 하드코딩한 부분 삭제시 validation 용) 
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void InserIssueHist(string I_TRAYID, string I_TYPE, string I_POSI, string I_REMARK, string I_USER)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("LABEL_CODE", typeof(string));
                dtRqst.Columns.Add("PRT_QTY", typeof(int));
                dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = I_TRAYID;
                dr["LABEL_CODE"] = I_TYPE;
                dr["PRT_QTY"] = int.Parse(I_POSI);
                dr["REMARKS_CNTT"] = I_REMARK;
                dr["USERID"] = I_USER;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_CST_LABEL_PRT_HIST", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

    }
}
