/*************************************************************************************
 Created Date : 2020.11.18
      Creator : 박준규
   Decription : Lot별 Cell Data 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.18  DEVELOPER : 박준규
  2021.02.23  박수미 
  2022.11.09  조영대    : 조건 날짜 오류 수정 
  2022.11.24  조영대    : 엑셀 다운로드시 숫자형식 변환
  2023.05.09  최도훈    : 인도네시아 조회시 오류나는 현상 수정
  2023.07.25  최도훈    : '설비명 추가' 체크시 Excel 'Download' 버튼 에러나는 현상 수정
  2023.08.31  손동혁    : NA 1동 요청 엑셀 다운로드 시 U1 , 19 케이스 추가 및 Charge시에 FittedCapa 값 보이게 수정
  2024.01.03  조영대    : WA 엑셀 다운로드 Merge 시 Decimal 변환 오류 수정.
  2024.01.23  조영대    : Excel Export 시 컬럼 헤더 다국어 적용.
  2024.02.07  조영대    : Excel Export 시 컬럼 추가에 따른  인덱스 수정
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// Lot별 Cell Data 조회
    /// </summary>
    public partial class FCS001_046 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        bool bUseFlag = false; //2023.08.31 케이스문 U1 동별공통코드 사용 NA1동만 보이게 추가 테스트 후 삭제
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public FCS001_046()
        {
            InitializeComponent();
            InitCombo();
            InitControl();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ///2023.08.31
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_062"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

            this.Loaded -= UserControl_Loaded;
        }
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelChild = { cboRoute };
            string[] sFilter = { null };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent, cbChild: cboModelChild, sFilter: sFilter);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboWorkOP };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.SELECT, sCase: "ROUTE", cbParent: cboRouteParent, cbChild:cboRouteChild);

            C1ComboBox[] cboOperParent = { cboRoute };
            ComCombo.SetCombo(cboWorkOP, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent);
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now;
            dtpToDate.SelectedDateTime = DateTime.Now;
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            // 로딩 창 
            //StartLoader();

            SetCellDataNew();
        }

        private void SetCellDataNew()
        {
            try
            {
                int iLotCnt = 0;

                //선택한 lot tray 조회
                string sLot = "";
                string sFileName = "";
                string sBiz = "";
                DataSet dsDown = new DataSet();

                for (int i = 0; i < dgCellList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        sLot += Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "PROD_LOTID"));
                        sLot += ",";
                        sFileName += Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "PROD_LOTID"));
                        sFileName += "_";
                        iLotCnt++;
                    }
                }
                if (string.IsNullOrEmpty(sLot))
                {
                    //Util.AlertMsg("선택된 LOT이 없습니다.");
                    Util.MessageValidation("SFU1261");
                    return;
                }


                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("WIPDTTM_ST", typeof(DateTime));
                dtRqst.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROD_LOTID"] = sLot;
                dr["WIPDTTM_ST"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["WIPDTTM_ED"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                dr["ROUTID"] = Util.GetCondition(cboRoute);
                if (string.IsNullOrEmpty(dr["ROUTID"].ToString())) { Util.MessageValidation("FM_ME_0411"); return; }
                dtRqst.Rows.Add(dr);

                DataTable dtCellInfo = new ClientProxy().ExecuteServiceSync("DA_SEL_CELLINFO_BY_TRAYNO_NEW", "RQSTDT", "RSLTDT", dtRqst);
                //여기까지 선택한 lot tray 조회

                if (dtCellInfo.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1498");
                    return;
                }
                dtCellInfo.PrimaryKey = new DataColumn[] { dtCellInfo.Columns["LOTID"], dtCellInfo.Columns["CSTSLOT"] };

                DataTable dtOp = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_ROUTE_OP", "RQSTDT", "RSLTDT", dtRqst);
                for (int i = 0; i < dtOp.Rows.Count; i++)
                {
                    dtRqst.Rows[0]["PROCID"] = dtOp.Rows[i]["CBO_CODE"];
                    switch (dtOp.Rows[i]["S27"].ToString())
                    {
                      
                        case "11":
                        case "J1":
                        case "12":
                        case "J2":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_CHG_DISCHG_NEW";
                            break;

                        case "13":
                        case "J3":
                        case "81":
                        case "A1":
                        case "A2":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_OCV_NEW";
                            break;
                        case "17":
                        case "19":
                            sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_MEGA_CHG_NEW";
                            break;
                       
                       case "U1":
                            if (bUseFlag)
                            {
                                sBiz = "DA_SEL_CELLINFO_BY_TRAYNO_HPCD_NEW";
                            }else
                            {
                                continue;
                            }
                            break;
                    
                    
                        default: continue; //위 경우를 제외하고 처리 안함
                    }
                    DataTable dtRsltMeas = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRsltMeas.Rows.Count > 0)
                    {
                        if (bUseFlag)
                        {
                            if (!(dtOp.Rows[i]["S27"].ToString().Equals("12")|| dtOp.Rows[i]["S27"].ToString().Equals("11"))) //DISCHARGE 에서만  FITCAPA 존재 NA 1동 에서는 CHARGE 에서도 FITCAPA 데이터 확인
                            {
                                if (dtRsltMeas.Columns.IndexOf("FITCAPA_VAL") > -1)
                                {
                                    dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("FITCAPA_VAL"));
                                }
                            }
                        }
                        else
                        {
                            if (!dtOp.Rows[i]["S27"].ToString().Equals("12")) //DISCHARGE 에서만  FITCAPA 존재
                            {
                                if (dtRsltMeas.Columns.IndexOf("FITCAPA_VAL") > -1)
                                {
                                    dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("FITCAPA_VAL"));

                                }
                            }
                        }


                        if (!dtOp.Rows[i]["S27"].ToString().Equals("J1")) //JIG CHARGE 에서만  POWER 존재
                        {
                            if (dtRsltMeas.Columns.IndexOf("POWER") > -1)
                            {
                                dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("POWER"));
                            }
                        }

                        if (!dtOp.Rows[i]["S27"].ToString().Equals("J")) //JIG 에서 검색 안하는 경우에는 JIG 설비가 나올수 없음 JIG 끝나고 트레이 갈아탐
                        {
                            if (dtRsltMeas.Columns.IndexOf("EQP") > -1)
                            {
                                if (chkAddEQPName.IsChecked == false)
                                {
                                    dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("EQP"));
                                }
                            }
                        }

                        if (dtRsltMeas.Columns.IndexOf("EQP") > -1 && string.IsNullOrEmpty(dtRsltMeas.Rows[0]["EQP"].ToString()))// 첫번째 row 의 eqp_id 가 없는 경우 제거, JIG 에서 검색 안하는 경우에는 JIG 설비가 나올수 없슴 JIG 끝나고 트레이 갈아탐
                        {
                            //설비명 추가 체크박스
                            if (chkAddEQPName.IsChecked == false)
                            {
                                dtRsltMeas.Columns.RemoveAt(dtRsltMeas.Columns.IndexOf("EQP"));
                            }
                        }
                        LeftOuterJoin(ref dtCellInfo, dtRsltMeas, dtOp.Rows[i]["CBO_NAME"].ToString());
                    }
                    if (bUseFlag)
                    {
                        //NA 1동일 경우 대용량 충방전 및 충방전에서 온도정보 추가 체크박스
                        if ((chkAddTemp.IsChecked == true) && (sBiz == "DA_SEL_CELLINFO_BY_TRAYNO_CHG_DISCHG_NEW" || sBiz == "DA_SEL_CELLINFO_BY_TRAYNO_MEGA_CHG_NEW"))
                        {
                            DataTable dtRsltTemp = new ClientProxy().ExecuteServiceSync("DA_SEL_CELLINFO_BY_TRAYNO_CHG_DISCHG_TEMP_NEW", "RQSTDT", "RSLTDT", dtRqst);
                            LeftOuterJoinTemp(ref dtCellInfo, dtRsltTemp, dtOp.Rows[i]["CBO_NAME"].ToString());
                        }
                    }
                    else
                    {
                        //온도정보 추가 체크박스
                        if ((chkAddTemp.IsChecked == true) && sBiz == "DA_SEL_CELLINFO_BY_TRAYNO_CHG_DISCHG_NEW")
                        {
                            DataTable dtRsltTemp = new ClientProxy().ExecuteServiceSync("DA_SEL_CELLINFO_BY_TRAYNO_CHG_DISCHG_TEMP_NEW", "RQSTDT", "RSLTDT", dtRqst);
                            LeftOuterJoinTemp(ref dtCellInfo, dtRsltTemp, dtOp.Rows[i]["CBO_NAME"].ToString());
                        }
                    }
               
                }

                // 컬럼 수치형 변환
                DataTable dtConvert = dtCellInfo.Clone();
                dtConvert.Locale = System.Globalization.CultureInfo.GetCultureInfo("en-US");
                for (int col = dtConvert.Columns["FINL_JUDG_CODE"].Ordinal + 1; col < dtConvert.Columns.Count; col++)
                {
                    if (dtConvert.Columns[col].ColumnName.Contains("WIPDTTM")) continue;

                    if (dtConvert.Columns[col].ColumnName.Contains("EQP")
                        || dtConvert.Columns[col].ColumnName.Contains("ORI_PROD_LOTID")
                        || dtConvert.Columns[col].ColumnName.Contains("TRAY_TYPE_CODE")
                        )
                    {
                        dtConvert.Columns[col].DataType = typeof(string);
                    }
                    else
                    {
                        dtConvert.Columns[col].DataType = typeof(decimal);
                    }
                }
                dtConvert.Merge(dtCellInfo, true, MissingSchemaAction.Ignore);

                CMM001.Controls.UcBaseDataGrid dgExcel = new CMM001.Controls.UcBaseDataGrid();
                dgExcel.ExcelExportModify += DgExcel_ExcelExportModify;
                dgExcel.ItemsSource = DataTableConverter.Convert(dtConvert);

                // Column Caption 다국어 변환
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgExcel.Columns)
                {
                    if (string.IsNullOrEmpty(Util.NVC(dgc.Header))) continue;

                    string[] tokens = Util.NVC(dgc.Header).Replace("[#] ", "").Split(' ');
                    if (tokens.Length > 1)
                    {
                        string convertToken = ObjectDic.Instance.GetObjectName(tokens.Last());
                        if (!convertToken.Contains("[#] "))
                        {
                            dgc.Header = Util.NVC(dgc.Header).Replace(tokens.Last(), convertToken);
                        }
                    }
                    else
                    {
                        dgc.Header = ObjectDic.Instance.GetObjectName(Util.NVC(dgc.Header)).Replace("[#] ", "");
                    }
                }

                dgExcel.ExcelExport(false);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void DgExcel_ExcelExportModify(object sender, C1XLBook book)
        {
            try
            {
                XLConditionalFormatting conFormat = new XLConditionalFormatting();
                for (int colinx = 0; colinx < book.Sheets[0].Columns.Count; colinx++)
                {
                    if (book.Sheets[0].GetCell(0, colinx).Text.ToUpper().Contains("TEMP"))
                    {
                        conFormat.Ranges.Add(new XLRange(1, colinx, book.Sheets[0].Rows.Count - 1, colinx));
                    }
                }
                XLConditionalFormattingRule conFormatRule = new XLConditionalFormattingRule();
                conFormatRule.Operator = XLConditionalFormattingOperator.GreaterThanOrEqual;
                conFormatRule.FirstFormula = "100";
                conFormatRule.Pattern = new XLPatternFormatting();
                conFormatRule.Pattern.BackColor = System.Windows.Media.Color.FromRgb(255, 199, 206);
                conFormatRule.Priority = 1;
                conFormat.Rules.Add(conFormatRule);

                XLConditionalFormattingCollection conFormatCollection = book.Sheets[0].ConditionalFormattings;
                conFormatCollection.Add(conFormat);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void GetList()
        {
            try
            {
                Util.gridClear(dgCellList);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("START_TIME", typeof(string));
                dtRqst.Columns.Add("END_TIME", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, sMsg: "FM_ME_0411");
                dr["PROCID"] = Util.GetCondition(cboWorkOP, bAllNull: true);
                dr["START_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["END_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");

                if (!string.IsNullOrEmpty(txtLotID.Text.Trim()))
                    dr["PROD_LOTID"] = txtLotID.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_LOT_LIST", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    Util.GridSetData(dgCellList, dtRslt, FrameOperation, true);
                }
                Util.GridSetData(dgCellList, dtRslt, FrameOperation, true);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                //if (dg.CurrentColumn.Name.Equals("TRAY_ID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y"
                //    || dg.CurrentColumn.Name.Equals("LOTID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y")
                //{

                //}
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

        private void LeftOuterJoin(ref DataTable dtCellInfo, DataTable dtRslt1, string sOpName)
        {
            for (int i = 0; i < dtRslt1.Columns.Count; i++)
            {
                if (dtRslt1.Columns[i].ColumnName != "LOTID" && dtRslt1.Columns[i].ColumnName != "CSTSLOT")
                {
                    if (dtRslt1.Columns[i].ColumnName.Equals("EQP") || dtRslt1.Columns[i].ColumnName.Equals("WIPDTTM_ST") || dtRslt1.Columns[i].ColumnName.Equals("WIPDTTM_ED"))
                    {
                        dtCellInfo.Columns.Add(sOpName + " " + dtRslt1.Columns[i].ColumnName, typeof(string));
                    }
                    else
                    {
                        dtCellInfo.Columns.Add(sOpName + " " + dtRslt1.Columns[i].ColumnName, typeof(string));
                    }


                    for (int j = 0; j < dtRslt1.Rows.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString()))
                        {

                            if (dtCellInfo.Rows.Find(new string[] { dtRslt1.Rows[j]["LOTID"].ToString(), dtRslt1.Rows[j]["CSTSLOT"].ToString() }) != null)
                            {
                                dtCellInfo.Rows.Find(new string[] { dtRslt1.Rows[j]["LOTID"].ToString(), dtRslt1.Rows[j]["CSTSLOT"].ToString() })[sOpName + " " + dtRslt1.Columns[i].ColumnName] = dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString();
                            }
                        }
                    }
                }
            }
        }

        private void LeftOuterJoinTemp(ref DataTable dtCellInfo, DataTable dtRslt1, string sOpName)
        {
            for (int i = 0; i < dtRslt1.Columns.Count; i++)
            {
                if (dtRslt1.Columns[i].ColumnName != "LOTID")
                {
                    dtCellInfo.Columns.Add(sOpName + " " + dtRslt1.Columns[i].ColumnName, typeof(Single));

                    for (int j = 0; j < dtRslt1.Rows.Count; j++)
                    {
                        DataRow[] drArray = dtCellInfo.Select("LOTID='" + dtRslt1.Rows[j]["LOTID"].ToString() + "'");

                        for (int k = 0; k < drArray.Length; k++) //동일 트레이는 모두 같은 값으로 셋팅하도록 처리
                        {
                            if (!string.IsNullOrEmpty(dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString()))
                            {
                                dtCellInfo.Rows.Find(new string[] { dtRslt1.Rows[j]["LOTID"].ToString(), drArray[k]["CSTSLOT"].ToString() })[sOpName + " " + dtRslt1.Columns[i].ColumnName] = dtRslt1.Rows[j][dtRslt1.Columns[i].ColumnName].ToString();
                            }
                        }
                    }
                }
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgCellList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgCellList);
        }

        private void dgCellList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
    }
}
