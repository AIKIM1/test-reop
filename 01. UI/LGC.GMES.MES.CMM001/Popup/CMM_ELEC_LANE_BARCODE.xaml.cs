/***************************************************************
 Created Date : 2016.08.18전체선택
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - LANE별 바코드 출력 팝업
----------------------------------------------------------------
 [Change History]
  2017.05.12 
  2024.10.09   김도형   : [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
****************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;


namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_LANE_BARCODE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_LANE_BARCODE : C1Window, IWorkArea
    {
        private string _LOTID = string.Empty;
        private string _PROCID = string.Empty;
        private DataRowView dataRowView;

        // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
        private string _LABELCD_SLT_THICK = string.Empty;     // 슬리터 LOT 두께 바코드 LABELCD

        Util _Util = new Util();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_LANE_BARCODE()
        {
            InitializeComponent();
        }

        public CMM_ELEC_LANE_BARCODE(TextBox target)
        {
            InitializeComponent();
            ApplyPermissions();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 3)
            {
                _LOTID = tmps[0].GetString();
                _PROCID = tmps[1].GetString();
                dataRowView = tmps[2] as DataRowView;

            }
            else
            {
                _LOTID = string.Empty;
                _PROCID = string.Empty;
                dataRowView = null;
            }
            GetCutIdList(_LOTID);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            bool isSelected = false;
            for (int i = 0; i < dgCutList.Columns.Count; i++)
                if (dgCutList.Columns[i].Name.Contains("CHK"))
                    for (int j = 0; j < dgCutList.Rows.Count; j++)
                        if (!string.Equals(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, dgCutList.Columns[i].Name), 0))
                        {
                            isSelected = true;
                            break;
                        }

            if (isSelected == false)
            {
                Util.MessageValidation("SFU1364"); // LOT ID가 선택되지 않았습니다.
                return;
            }

            string sEltrSltLotThicknessViewYn = string.Empty;                      // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
            if (_PROCID.Equals(Process.SLITTING))
            {
                sEltrSltLotThicknessViewYn = GetEltrSltLotThicknessViewYn();       // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송

                if (sEltrSltLotThicknessViewYn.Equals("Y") && _LABELCD_SLT_THICK.Equals(""))  // LABELCD_가 없는 경우
                {
                    Util.MessageValidation("SFU4079");  //라벨 정보가 없습니다.
                    return;
                }
            }

            DataTable printDT = GetPrintCount(_LOTID, _PROCID);

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT1"]) > 0)
            {
                // 해당 LOT BARCODE VALID 추가 [2017-09-25]
                string sValidMsg = GetLabelPrintValid(_LOTID, _PROCID);

                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                //Util.MessageConfirm("SFU3463", (sresult) =>
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3463"), sValidMsg, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            for (int i = 0; i < dgCutList.Columns.Count; i++)
                                if (dgCutList.Columns[i].Name.Contains("CHK"))
                                    for (int j = 0; j < dgCutList.Rows.Count; j++)
                                        if (!string.Equals(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, dgCutList.Columns[i].Name), 0))
                                        {
                                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                            {
                                                if (sEltrSltLotThicknessViewYn.Equals("Y") && _PROCID.Equals(Process.SLITTING)) // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                                                {
                                                    PrintLabel_Elec_Thick(FrameOperation, loadingIndicator, Util.NVC(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1))), _PROCID);
                                                }
                                                else
                                                {
                                                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1))), _PROCID);
                                                }
                                            }

                                            Util.UpdatePrintExecCount(Util.NVC(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1))), _PROCID);
                                        }

                            // 회수 갱신
                            DataTableConverter.SetValue(dataRowView, "PRT_COUNT1", Util.NVC_Decimal(DataTableConverter.GetValue(dataRowView, "PRT_COUNT1")) + 1);
                            this.DialogResult = MessageBoxResult.OK;
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    }
                });
            }
            else
            {
                try
                {
                    for (int i = 0; i < dgCutList.Columns.Count; i++)
                        if (dgCutList.Columns[i].Name.Contains("CHK"))
                            for (int j = 0; j < dgCutList.Rows.Count; j++)
                                if (!string.Equals(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, dgCutList.Columns[i].Name), 0))
                                {
                                    for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                    {
                                        if (sEltrSltLotThicknessViewYn.Equals("Y") && _PROCID.Equals(Process.SLITTING)) // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                                        {
                                            PrintLabel_Elec_Thick(FrameOperation, loadingIndicator, Util.NVC(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1))), _PROCID);
                                        }
                                        else
                                        {
                                            Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1))), _PROCID);
                                        }
                                    }
                                        
                                    Util.UpdatePrintExecCount(Util.NVC(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1))), _PROCID);
                                }

                                    // 회수 갱신
                    DataTableConverter.SetValue(dataRowView, "PRT_COUNT1", Util.NVC_Decimal(DataTableConverter.GetValue(dataRowView, "PRT_COUNT1")) + 1);
                    this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetCutIdList(string lot_ID)
        {
            DataTable searchConditionTable = new DataTable();
            searchConditionTable.Columns.Add("LOTID", typeof(string));

            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LOTID"] = lot_ID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("DA_PRD_SEL_SLIT_LOT_LANE", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgCutList, searchResult, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void chkLot_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgCutList.Columns.Count; i++)
                if (dgCutList.Columns[i].Name.Contains("CHK"))
                    for (int j = 0; j < dgCutList.Rows.Count; j++)
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1)))))
                        {
                            DataTableConverter.SetValue(dgCutList.Rows[j].DataItem, dgCutList.Columns[i].Name, true);
                            C1.WPF.DataGrid.DataGridCell cell = dgCutList.GetCell(j, i);
                            
                            if ( cell != null && cell.Presenter != null && cell.Presenter.Content != null )
                            {
                                CheckBox chkBox = cell.Presenter.Content as CheckBox;
                                if (chkBox != null)
                                    chkBox.IsChecked = true;
                            }
                        }
        }

        private void chkLot_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgCutList.Columns.Count; i++)
                if (dgCutList.Columns[i].Name.Contains("CHK"))
                    for (int j = 0; j < dgCutList.Rows.Count; j++)
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1)))))
                        {
                            DataTableConverter.SetValue(dgCutList.Rows[j].DataItem, dgCutList.Columns[i].Name, false);
                            C1.WPF.DataGrid.DataGridCell cell = dgCutList.GetCell(j, i);

                            if (cell != null && cell.Presenter != null && cell.Presenter.Content != null)
                            {
                                CheckBox chkBox = cell.Presenter.Content as CheckBox;
                                if (chkBox != null)
                                    chkBox.IsChecked = false;
                            }
                        }
        }

        private void dgCutList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Column.Name.Contains("CHK"))
                    {
                        CheckBox chkBox = e.Cell.Presenter.Content as CheckBox;

                        if (chkBox != null)
                            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "LOTID_" + e.Cell.Column.Name.Substring(e.Cell.Column.Name.Length - 1, 1)))))
                                chkBox.Visibility = Visibility.Collapsed;
                    }
                }));
            }
        }

        private void dgCutList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Column.Name.Contains("CHK"))
                    {
                        CheckBox chkBox = e.Cell.Presenter.Content as CheckBox;

                        if (chkBox != null)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "LOTID_" + e.Cell.Column.Name.Substring(e.Cell.Column.Name.Length - 1, 1)))))
                                chkBox.Visibility = Visibility.Visible;
                    }
                }));
            }
        }

        private DataTable GetPrintCount(string sLotID, string sProcID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private string GetLabelPrintValid(string sLotID, string sProcID)
        {
            string returnMsg = string.Empty;

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID.Substring(0, 9);
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_VALID_CUT", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    if (string.Equals(sProcID, Process.SLITTING) || string.Equals(sProcID, Process.SRS_SLITTING))
                    {
                        for (int i = 0; i < dgCutList.Columns.Count; i++)
                        {
                            if (dgCutList.Columns[i].Name.Contains("CHK"))
                            {
                                for (int j = 0; j < dgCutList.Rows.Count; j++)
                                {
                                    if (!string.Equals(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, dgCutList.Columns[i].Name), 0))
                                    {
                                        foreach (DataRow row in result.Rows)
                                        {
                                            if (string.Equals(DataTableConverter.GetValue(dgCutList.Rows[j].DataItem, "LOTID_" + dgCutList.Columns[i].Name.Substring(dgCutList.Columns[i].Name.Length - 1, 1)), row["LOTID"]))
                                            {
                                                if (!string.IsNullOrEmpty(Util.NVC(row["RACK_ID"])))
                                                {
                                                    returnMsg = MessageDic.Instance.GetMessage("SFU4125", row["RACK_ID"]); // 보관렉[%1]에 동일한 LOT 이 보관 중입니다.
                                                    return returnMsg;
                                                }
                                                else if (string.Equals(row["MOVE_ORD_STAT_CODE"], "CLOSE_MOVE"))
                                                {
                                                    returnMsg = MessageDic.Instance.GetMessage("SFU4126"); // 조립 인계 완료된 LOT 입니다.
                                                    return returnMsg;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return returnMsg;
        }

        #region    [SLT_LOT_THICKNESS Print Start] 

        // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
        private string GetEltrSltLotThicknessViewYn()
        {
            string sEltrSltLotThicknessViewYn = string.Empty;

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELTR_SLT_LOT_THICKNESS_VIEW_YN";  // 전극 슬리터 LOT 두께 보기 여부
            sCmCode = _PROCID;                              // 공정 

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sEltrSltLotThicknessViewYn = "Y";
                    _LABELCD_SLT_THICK = dtResult.Rows[0]["ATTR1"].ToString();
                }
                else
                {
                    sEltrSltLotThicknessViewYn = "N";
                    _LABELCD_SLT_THICK = string.Empty;
                }

                return sEltrSltLotThicknessViewYn;
            }
            catch (Exception ex)
            {
                sEltrSltLotThicknessViewYn = "N";
                _LABELCD_SLT_THICK = string.Empty;
                //Util.MessageException(ex);
                return sEltrSltLotThicknessViewYn;
            }
        }

        // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
        public void PrintLabel_Elec_Thick(IFrameOperation frameOperation, LoadingIndicator loadingIndicator, string sLotID, string procID, string sSampleCompany = "", string userName = "")
        {
            try
            {
                string blCode = _LABELCD_SLT_THICK;

                //---------------------------------------------------------------------------------------------------------------------------------
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CUT_YN", typeof(string));   // CUT라벨 발행여부
                inTable.Columns.Add("I_LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("I_PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("I_RESO", typeof(string));   // 해상도
                inTable.Columns.Add("I_PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("I_MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("I_MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("I_DARKNESS", typeof(string));   // 농도
                inTable.Columns.Add("SAMPLE_COMPANY", typeof(string));   // 샘플링 출하거래처
                inTable.Columns.Add("CLCTTYPE", typeof(string));   // 설비수집유형 : E
                inTable.Columns.Add("USER_NAME", typeof(string));   // 라벨발행 사용자

                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["LANGID"] = LoginInfo.LANGID;
                        indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                        indata["LOTID"] = sLotID;
                        indata["PROCID"] = procID;

                        if (string.Equals(LoginInfo.CFG_CUT_LABEL, "Y"))
                            indata["CUT_YN"] = LoginInfo.CFG_CUT_LABEL;

                        indata["I_LBCD"] = blCode;
                        indata["I_PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["I_RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["I_PRCN"] = string.IsNullOrEmpty(Util.NVC(row["COPIES"])) ? "1" : Util.NVC(row["COPIES"]);
                        indata["I_MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["I_MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        indata["I_DARKNESS"] = string.IsNullOrEmpty(Util.NVC(row["DARKNESS"])) ? "15" : Util.NVC(row["DARKNESS"]);
                        indata["CLCTTYPE"] = "E";
                        indata["SAMPLE_COMPANY"] = sSampleCompany;
                        indata["USER_NAME"] = string.IsNullOrEmpty(userName) ? null : userName;
                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                if (inTable.Rows.Count < 1)
                    throw new Exception(MessageDic.Instance.GetMessage("SFU3030"));

                string sBizName = "BR_PRD_GET_SLITTING_LOT_THICKNESS_LABEL";

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                    return;

                if (!string.Equals(Util.NVC(dtMain.Rows[0]["I_ATTVAL"]).Substring(0, 1), "0"))
                    throw new Exception(MessageDic.Instance.GetMessage("SFU1309"));

                // 동시에 출력시 순서 뒤바끼는 문제때문에 SLEEP 추가
                foreach (DataRow row in dtMain.Rows)
                {
                    System.Threading.Thread.Sleep(500);
                    Util.PrintLabel(frameOperation, loadingIndicator, Util.NVC(row["I_ATTVAL"]));

                }
            }
            catch (Exception ex) { throw ex; }
        }


        #endregion [SLT_LOT_THICKNESS Print End]
    }
}