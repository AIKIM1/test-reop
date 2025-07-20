/*********************************************************************************************************************************************************************************
 Created Date : 2021.06.04
      Creator : 박수미
   Decription : 폐기 셀 등록
----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 [Change History]
  2021.06.04  DEVELOPER : 박수미
  2022.09.14  최도훈 : 폐기대기처리 기능 추가
  2022.12.14  조영대 : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2023.01.31  조영대 : Empty Cell ID 체크
  2023.02.27  박승렬 : CSR ID : E20230222-000005 / Lot type추가(컬럼 추가) : 양산,개발 샘플 등 / Hold 여부 추가(컬럼 추가) : Cell홀드 또는 Lot홀드 시 해당사유 표시
  2023.04.07  이정미 : Tray LotID로 Cell 조회하여 실물 폐기, 폐기대기 처리 가능하도록 기능 추가 
  2023.04.11  이정미 : 실물폐기, 폐기대기 100건씩 처리하도록 수정 
  2023.05.03  최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2023.05.15  주훈종 : CSR ID : E20230513-001839 / ESGM 요청에 의해 CELL 입력순서대로 조회결과 나오도록 수정함
  2023.07.18  조영대 : 취소(복구) 탭 추가(동별공통코드 FORM_SCRAP_CNCL_USER_FLAG 일때 활성화)
  2023.10.04  김최일 : CSR ID : E20231002-000036 / ESGM 요청에 의해 CELL 단위 실물 폐기 처리 조회TAB에 LOT TYPE 컬럼 나오도록 수정함
  2024.01.18  형준우 : 실물 폐기 이벤트 (SetCellScrap) AREAID 추가
  2024.01.30  배준호 : 폐기대기 처리 CELL 처리 기능 추가(기존 Lot만 존재)
  2024.11.06  복현수 : MES 리빌딩 PJT - 인데이터 타입 수정
  2025.03.17  최경아 : MES2.0 CatchUp [E20250121-000650] 폐기처리 동시 처리 수량 동별 공통코드로 수량 조절 가능하도록 변경 (동별 공통코드 - FCS001_113_SCARP_REG_COUNT_OPTION)
  2025.03.25  복현수 : 폐기 대기 처리 조회시 결과 없을 경우 알람 팝업 추가
**********************************************************************************************************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Configuration;
using C1.WPF.Excel;
using Microsoft.Win32;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FCS001_113 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private int cnt_sum;
        private int cnt_error;
        private int iRegCount; //2025.01.20 폐기처리 동시 처리 Count 전역변수

        Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_113()
        {
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            iRegCount = 100; // Default 100개

            //2025.01.17 - 폐기처리 동시 처리 수량 동별 공통코드로 수량 조절 가능하도록 변경
            DataTable dtResult = _Util.GetAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_113_SCARP_REG_COUNT_OPTION"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                int iRet = iRegCount; // 초기값
                bool bRet = Int32.TryParse(Convert.ToString(dtResult.Rows[0]["ATTR2"]), out iRet);

                if (bRet)
                {
                    iRegCount = iRet;
                }
            }

            if (IsRecoverTablView()) Recover.Visibility = System.Windows.Visibility.Visible;
        }

        #region Initialize
        private void InitCombo()
        {
            string[] sFilterSearchShift = { "STM", null, null, null, null, null };
            string[] sFilterOp = { "CLO", "A,B,C,D" };
            string[] sFilterEqpInput = { "EQP_INPUT_YN" };

            CommonCombo_Form _combo = new CommonCombo_Form();
            _combo.SetCombo(cboSearchLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");
            _combo.SetCombo(cboSearchModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL");
        }

        #endregion

        #region [Method]

        #region [등록]
        private void GetCellData(string sCellID = "" ,string sLotID = "" )
        {
            try
            {
                cnt_sum = 0;
                cnt_error = 0;

                txtInsertCellCnt.Text = string.Empty;
                txtBadInsertRow.Text = string.Empty;

                //LOTID로 검색
                if (chkLot.IsChecked.Equals(true))
                {
                    if (string.IsNullOrEmpty(sLotID))
                    {
                        for (int iRow = 0; iRow < dgInputList.Rows.Count; iRow++)
                        {
                            string lot = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[iRow].DataItem, "LOTID"));
                            if (string.IsNullOrEmpty(lot))
                                continue;
                            sLotID += lot;
                            sLotID += ",";
                        }
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = sLotID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_INFO_FOR_SCRAP", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    cnt_sum = dtRslt.Rows.Count;
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        if (!Util.NVC(dtRslt.Rows[i]["AVAIL_YN"]).Equals("Y"))
                            cnt_error++;
                    }

                    //TRAY LOT ID 별로 정렬
                    DataView dv = new DataView(dtRslt);
                    dv.Sort = "LOTID";
                    DataTable dtSort = dv.ToTable();

                    Util.GridSetData(dgInputList, dtSort, this.FrameOperation);

                    txtInsertCellCnt.Text = cnt_sum.ToString();
                    txtBadInsertRow.Text = cnt_error.ToString();
                }

                //SUBLOTID로 검색
                else
                {

                    if (string.IsNullOrEmpty(sCellID))
                    {
                        //SUBLOTID - XADDA14223,XADDA14224,XADDA14225 ...
                        for (int iRow = 0; iRow < dgInputList.Rows.Count; iRow++)
                        {
                            string sublot = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[iRow].DataItem, "SUBLOTID"));
                            if (string.IsNullOrEmpty(sublot))
                                continue;
                            sCellID += sublot;
                            sCellID += ",";
                        }
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SUBLOTID"] = sCellID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_INFO_FOR_SCRAP", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    cnt_sum = dtRslt.Rows.Count;
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        if (!Util.NVC(dtRslt.Rows[i]["AVAIL_YN"]).Equals("Y"))
                            cnt_error++;
                    }

                    //TRAY LOT ID 별로 정렬
                    DataView dv = new DataView(dtRslt);
                    //dv.Sort = "LOTID"; //2023.05.15 , CSR ID : E20230513-001839
                    DataTable dtSort = dv.ToTable();

                    Util.GridSetData(dgInputList, dtSort, this.FrameOperation);

                    txtInsertCellCnt.Text = cnt_sum.ToString();
                    txtBadInsertRow.Text = cnt_error.ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetCellDataRcv(string sCellID = "")
        {
            try
            {
                cnt_sum = 0;
                cnt_error = 0;

                txtInsertCellCntRcv.Text = string.Empty;
                txtBadInsertRowRcv.Text = string.Empty;

                if (string.IsNullOrEmpty(sCellID))
                {
                    sCellID = dgInputListRcv.GetDataTable().AsEnumerable()
                                 .Where(w => !Util.IsNVC(w.Field<string>("SUBLOTID")))
                                 .Select(s => s.Field<string>("SUBLOTID")).Aggregate((x, y) => x + "," + y);

                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SUBLOTIDLIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTIDLIST"] = sCellID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SCRAP_CANCEL_CELL", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
                
                if (!dtRslt.Columns.Contains("CHK")) dtRslt.Columns.Add("CHK", typeof(bool));

                cnt_sum = dtRslt.Rows.Count;
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (!Util.NVC(dtRslt.Rows[i]["AVAIL_YN"]).Equals("Y"))
                        cnt_error++;
                }

                DataView dv = new DataView(dtRslt);
                DataTable dtSort = dv.ToTable();

                Util.GridSetData(dgInputListRcv, dtSort, this.FrameOperation);

                txtInsertCellCnt.Text = cnt_sum.ToString();
                txtBadInsertRow.Text = cnt_error.ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private string LoadExcel()
        {

            DataTable dtInfo = DataTableConverter.Convert(dgInputList.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";

            string sColData = string.Empty;

            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];

                    for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID, LOTID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return sColData;
                        sColData += Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        sColData += ",";
                    }
                }
            }

            return sColData;
        }

        private string LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {

            DataTable dtInfo = DataTableConverter.Convert(dataGrid.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";

            string sColData = string.Empty;

            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];

                    for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID, LOTID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return sColData;
                        sColData += Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        sColData += ",";

                        if (rowInx >= 100)
                        {
                            Util.MessageValidation("SFU3695");
                            break;
                        }
                    }
                }
            }
            return sColData;
        }

        private void SetCellScrap()
        {
            try
            {
                int iProcessingCnt = iRegCount; //2025.01.20 폐기처리 동시 처리 수량 동별 공통코드로 수량 조절 가능하도록 변경
                double dNumberOfProcessingCnt = 0.0;
                bool bIsOK = true;

                Util.MessageConfirm("SFU4191", (result) => //폐기하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;
       
                    if (!dgInputList.IsCheckedRow("CHK"))
                    {
                        Util.AlertInfo("FM_ME_0419"); // 폐기 대상이 없습니다.
                        return;
                    }

                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("MENUID", typeof(string));                    
                    inData.Columns.Add("USER_IP", typeof(string));
                    inData.Columns.Add("PC_NAME", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));

                    DataRow dr = inData.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["USERID"] = LoginInfo.USERID;
                    dr["MENUID"] = LoginInfo.CFG_MENUID;
                    dr["USER_IP"] = LoginInfo.USER_IP;
                    dr["PC_NAME"] = LoginInfo.PC_NAME;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inData.Rows.Add(dr);

                    DataTable inSublot = inDataSet.Tables.Add("INSUBLOT");
                    inSublot.Columns.Add("SUBLOTID", typeof(string));

                    dNumberOfProcessingCnt = Math.Ceiling(dgInputList.Rows.Count / (double)iProcessingCnt);//처리수량

                    for(int i = 0; i < dNumberOfProcessingCnt; i ++ )
                    {
                        inSublot.Clear();

                        for (int k = (i * Convert.ToInt32(iProcessingCnt)); k < (i * iProcessingCnt + iProcessingCnt); k++)
                        {
                            if (k >= dgInputList.Rows.Count) break;

                            if (Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[k].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow dr2 = inSublot.NewRow();
                                dr2["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[k].DataItem, "SUBLOTID"));
                                inSublot.Rows.Add(dr2);
                            }
                        }

                        try
                        {
                            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SCRAP_BY_SUBLOT_LINE", "INDATA,INSUBLOT", "OUTDATA", inDataSet);

                        }
                        catch(Exception ex)
                        {
                            Util.MessageException(ex);
                            Util.MessageValidation("SFU9200", (i * (int)iProcessingCnt).ToString("#,##0"), (i * iProcessingCnt + iProcessingCnt - 1).ToString("#,##0"));
                            bIsOK = false;
                            return;
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    }

                    if (bIsOK)
                    {
                        Util.MessageValidation("FM_ME_0425");   // Cell 실물 폐기 완료되었습니다.
                        GetCellData();
                    }

                    else
                    {
                        Util.MessageValidation("SFU1497");      // 데이터 처리 중 오류가 발생했습니다
                    }

                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetCellScrapRcv()
        {
            try
            {
                Util.MessageConfirm("SFU1227", (result) => //복구하시겠습니까?
                {
                    if (result != MessageBoxResult.OK) return;

                    if (!dgInputListRcv.IsCheckedRow("CHK"))
                    {
                        Util.AlertInfo("FM_ME_0139"); // 복구 대상이 없습니다.
                        return;
                    }

                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("SUBLOTIDLIST", typeof(string));

                    DataRow dr = inData.NewRow();
                    dr["SRCTYPE"] = "UI";
                    dr["IFMODE"] = "OFF";
                    dr["USERID"] = LoginInfo.USERID;
                    dr["SUBLOTIDLIST"] = dgInputListRcv.GetCheckedDataRow("CHK")
                                         .Where(w => !Util.IsNVC(w.Field<string>("SUBLOTID")) && w.Field<string>("AVAIL_YN").Equals("Y"))
                                         .Select(s => s.Field<string>("SUBLOTID")).Aggregate((x, y) => x + "," + y);
                    inData.Rows.Add(dr); 

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_TRANSFER_SUBLOT_SCRAP_CANCEL", "INDATA", "OUTDATA", inDataSet);
                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        DataRow drResult = dsResult.Tables["OUTDATA"].Rows[0];
                        if (drResult != null && drResult["RESULT"].Equals("OK"))
                        {
                            Util.MessageValidation("FM_ME_0140");   // 복구완료 하였습니다.
                            GetCellDataRcv();
                        }
                        else
                        {
                            Util.MessageValidation("SFU1497");      // 데이터 처리 중 오류가 발생했습니다
                        }
                    }
                    else
                    {
                        Util.MessageValidation("SFU1497");      // 데이터 처리 중 오류가 발생했습니다
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [조회]
        private bool IsRecoverTablView()
        {
            try
            {                
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow drNew = RQSTDT.NewRow();
                drNew["LANGID"] = LoginInfo.LANGID;
                drNew["AREAID"] = LoginInfo.CFG_AREA_ID;
                drNew["COM_TYPE_CODE"] = "FORM_SCRAP_CNCL_USER_FLAG";
                drNew["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(drNew);

                string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Rows[0]["ATTR1"].Equals("Y")) return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }
        #endregion

        #region [폐기대기처리]
        private void GetData(string sCellID = "",string sLotID = "") 
        {
            try
            {
                if (chkLotDfct.IsChecked.Equals(true))
                {
                    if (string.IsNullOrEmpty(sLotID))
                    {
                        for (int iRow = 0; iRow < dgInputList2.Rows.Count; iRow++)
                        {
                            string lot = Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[iRow].DataItem, "LOTID"));
                            if (string.IsNullOrEmpty(lot))
                                continue;

                            sLotID += lot;
                            sLotID += ",";
                        }
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = sLotID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_INFO_INSERT_LOSS", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                    dtRslt.Columns.Add("CHK2", typeof(bool));

                    Util.gridClear(dgInputList2);

                    if (dtRslt.Rows.Count > 0)
                    {
                        //TRAY LOT ID 별로 정렬
                        DataView dv = new DataView(dtRslt);
                        dv.Sort = "LOTID";
                        DataTable dtSort = dv.ToTable();

                        Util.GridSetData(dgInputList2, dtSort, this.FrameOperation);
                    }
                    else
                    {
                        Util.Alert("SFU1905"); //조회된 Data가 없습니다.
                    }
                }
                else
                {
                    //SUBLOTID
                    for (int iRow = 0; iRow < dgInputList2.Rows.Count; iRow++)
                    {
                        string sublot = Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[iRow].DataItem, "SUBLOTID"));
                        if (string.IsNullOrEmpty(sublot))
                            continue;
                        sCellID += sublot;
                        sCellID += ",";
                    }


                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = sCellID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_LIST_INFO", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                    dtRslt.Columns.Add("CHK2", typeof(bool));

                    Util.gridClear(dgInputList2);

                    if (dtRslt.Rows.Count > 0)
                    {
                        //TRAY LOT ID 별로 정렬
                        DataView dv = new DataView(dtRslt);
                        dv.Sort = "LOTID";
                        DataTable dtSort = dv.ToTable();

                        Util.GridSetData(dgInputList2, dtSort, this.FrameOperation);
                    }
                    else
                    {
                        Util.Alert("SFU1905"); //조회된 Data가 없습니다.
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #endregion

        #region [Event]
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

        #region [등록 tab]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCellData();
        }
        private void dgInputList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }

                    if (e.Column.Name.Equals("LOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgInputList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            
                 dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name.ToString().Equals("CHK"))
                    {
                      
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AVAIL_YN")).Equals("N"))
                        {
                            e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                }));
            }

            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }               

        private void dgInputList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            dgInputList.ClearRows();
            txtInsertCellCnt.Text = "";
            txtBadInsertRow.Text = "";
            DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        private void btnRefresh2_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            dgInputList2.ClearRows();
            //txtInsertCellCnt2.Text = "";
            //txtBadInsertRow2.Text = "";
            DataGridRowAdd(dgInputList2, Convert.ToInt32(txtRowCntInsertCell2.Text));
        }

        private void btnScrapStandbyProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ClearValidation();

                if (!dgInputList2.IsCheckedRow("CHK2"))
                {
                    Util.AlertInfo("FM_ME_0419"); // 폐기 대상이 없습니다.
                    return;
                }

                // Empty Check
                bool isEmpty = false;

                if (chkLotDfct.IsChecked.Equals(true))
                {
                    for (int i = 2; i < dgInputList2.Rows.Count; i++)
                    {
                        if (dgInputList2.GetValue(i, "SUBLOTID") == null)
                        {
                            //데이터를 조회하십시오.
                            Util.Alert("9059");
                            return;
                        }
                    }
                }
           

                for (int row = 0; row < dgInputList2.Rows.Count; row++)
                {
                    if (!dgInputList2.IsCheckedRow("CHK2", row)) continue;

                    if (string.IsNullOrEmpty(Util.NVC(dgInputList2.GetValue(row, "SUBLOTID"))))
                    {
                        dgInputList2.SetCellValidation(row, "SUBLOTID", "SFU1495");  //대상 Cell ID가 입력되지 않았습니다.
                        isEmpty = true;
                    }
                }

                if (isEmpty)
                {   //대상 Cell ID가 입력되지 않았습니다.
                    Util.AlertInfo("SFU1495");
                    return;
                }

                FCS001_113_SCRAP_STANDBY_PROC fcs001_113_ScrapStandbyProc = new FCS001_113_SCRAP_STANDBY_PROC();
                fcs001_113_ScrapStandbyProc.FrameOperation = FrameOperation;

                if (fcs001_113_ScrapStandbyProc != null)
                {
                    string sCellList = string.Empty;
                    string sCheckList = string.Empty;
                    string sCellRowsCount = dgInputList2.GetRowCount().ToString();

                    for (int i = 0; i < dgInputList2.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "CHK2")).ToUpper().Equals("TRUE"))
                        {
                            sCellList += Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "SUBLOTID")) + "|";
                            sCheckList += Util.NVC(DataTableConverter.GetValue(dgInputList2.Rows[i].DataItem, "CHK2")) + "|";
                        }
                    }

                    if (string.IsNullOrEmpty(sCellList))
                    {
                        Util.MessageInfo("SFU8243");  //CELL을 선택해야합니다.
                        return;
                    }

                    object[] Parameters = new object[3];
                    Parameters[0] = sCellList;
                    Parameters[1] = sCheckList;
                    Parameters[2] = sCellRowsCount;

                    C1WindowExtension.SetParameters(fcs001_113_ScrapStandbyProc, Parameters);

                    fcs001_113_ScrapStandbyProc.Closed += new EventHandler(ScrapStandbyProc_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => fcs001_113_ScrapStandbyProc.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ScrapStandbyProc_Closed(object sender, EventArgs e)
        {
            FCS001_113_SCRAP_STANDBY_PROC window = sender as FCS001_113_SCRAP_STANDBY_PROC;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnRefresh2_Click(null, null);
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SetCellScrap();
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputList.Rows.Count; i++)
            {
                string availYN = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "AVAIL_YN"));
                if (availYN.Equals("Y"))
                {
                    DataTableConverter.SetValue(dgInputList.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgInputList.Rows[i].DataItem, "CHK", false);
            }
        }

        private void chkHeaderAllRcv_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputListRcv.Rows.Count; i++)
            {
                string availYN = Util.NVC(DataTableConverter.GetValue(dgInputListRcv.Rows[i].DataItem, "AVAIL_YN"));
                if (availYN.Equals("Y"))
                {
                    DataTableConverter.SetValue(dgInputListRcv.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void chkHeaderAllRcv_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputListRcv.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgInputListRcv.Rows[i].DataItem, "CHK", false);
            }
        }


        private void chkHeaderAll2_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputList2.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgInputList2.Rows[i].DataItem, "CHK2", true);
            }
        }

        private void chkHeaderAll2_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgInputList2.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgInputList2.Rows[i].DataItem, "CHK2", false);
            }
        }


        private void dgInputList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //폐기 대기 Lot 이 아닌 Cell 선택할 수 없음
            if (e.Column.Name.Equals("CHK"))
            {
                if (!Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "AVAIL_YN")).Equals("Y"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgInputList2_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Column.Name.Equals("CHK2"))
            {
                e.Cancel = false;
            }
        }

        private void dgInputList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            try
            {

                if (e.Cell.Column.Name.ToString().Equals("CHK"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AVAIL_YN")) != "N")
                    {
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [조회 tab]
        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string)); //MES 리빌딩 PJT 2024.11.06
                dtRqst.Columns.Add("TO_DATE", typeof(string)); //MES 리빌딩 PJT 2024.11.06
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");
                dr["EQSGID"] = Util.GetCondition(cboSearchLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboSearchModel, bAllNull: true);
                if (!string.IsNullOrEmpty(Util.NVC(txtSearchCellId.Text)))
                {
                    dr["SUBLOTID"] = Util.NVC(txtSearchCellId.Text);
                }


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SCRAP_CELL", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);

                Util.GridSetData(dgSearch, dtRslt, this.FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            if(chkLot.IsChecked.Equals(true))
            {
                string sLotID = LoadExcel();
                GetCellData(null,sLotID);
            }

            else
            {
                string sCellID = LoadExcel();
                GetCellData(sCellID);
            }
        }

        #endregion

        #region [폐기대기 tab]
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetData();    
        }

        private void btnExcel2_Click(object sender, RoutedEventArgs e)
        {
            if (chkLotDfct.IsChecked.Equals(true))
            {
                string sLotID = LoadExcel();
                GetData(null, sLotID);
            }

            else
            {
                string sCellID = LoadExcel();
                GetData(sCellID);
            }

         
        }
        #endregion

        #endregion

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "]" + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
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


        private void dgSearch_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgInputListRcv_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null) return;

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgInputListRcv_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgInputListRcv_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Column.Name.ToString().Equals("CHK"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AVAIL_YN")).Equals("N"))
                        {
                            e.Cell.Row.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInputListRcv_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            try
            {
                if (e.Cell.Column.Name.ToString().Equals("CHK"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AVAIL_YN")) != "N")
                    {
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInputListRcv_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //처리가능이 아닌 Cell 선택할 수 없음
            if (e.Column.Name.Equals("CHK"))
            {
                if (!Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "AVAIL_YN")).Equals("Y"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void btnRefreshRcv_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            dgInputListRcv.ClearRows();
            txtInsertCellCntRcv.Text = "";
            txtBadInsertRowRcv.Text = "";
            DataGridRowAdd(dgInputListRcv, Convert.ToInt32(txtRowCntInsertCellRcv.Text));
        }


        private void btnSaveRcv_Click(object sender, RoutedEventArgs e)
        {
            SetCellScrapRcv();
        }

        private void btnSearchRcv_Click(object sender, RoutedEventArgs e)
        {
            GetCellDataRcv();
        }

        private void btnExcelRcv_Click(object sender, RoutedEventArgs e)
        {
            string sCellID = LoadExcel(dgInputListRcv);
            if (string.IsNullOrEmpty(sCellID)) return;

            GetCellDataRcv(sCellID);
        }

    }
}
