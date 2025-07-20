/*********************************************************************************************
 Created Date : 2022.05.17
      Creator : 
   Decription : 전지GMES 구축 - LANE별 바코드 미리보기 팝업
----------------------------------------------------------------
 [Change History]
  2023.04.05   김도형   : [E20230328-000520]Lot Label print 미리보기 개선건
  2024.01.25   김도형   : [E20240115-000103] Slitter history card -> small tag
  2024.07.23   김도형   : [E20240717-000837] OC4동 라벨System변경 요청 件_IT서비스요청서(IM라벨) 
  2024.08.23   김도형   : [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정) 
  2024.10.09   김도형   : [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
**********************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Windows;
using System;
using System.Data;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using QRCoder;
using System.Drawing;
using System.Text;
using System.Collections;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_BARCODE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_BARCODE : C1Window, IWorkArea
    {
        private string eqsgID = string.Empty;
        private string procID = string.Empty;
        private string eqptID = string.Empty;
        private string skidID = string.Empty;
        //public C1DataGrid LOTLIST_GRID { get; set; }

        private string vQA_INSP_TRGT_FLAG = string.Empty;
        private string vLOTID = string.Empty;
        private string userName = string.Empty;
        private string CUT_ID = string.Empty;
        private string sPrtLotid = string.Empty;

        // [E20240717-000837] OC4동 라벨System변경 요청 件_IT서비스요청서(IM라벨)
        private string _ElecProc2DLabelUseYn = "N";         // 전극 공정별 2D Label 사용 여부
        private string _LotCodeCheckResult = "";            // Tesla Lable Lot_Code CheckResult

        // [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정)
        private string _ElecProcContentLabelUseYn = "N";     // 전극 공정별 IM_CONTENT Label 사용 여부
        private string _LotCodeContentLabeCheckResult = "";  // Tesla Lable Lot_Code Content Labe CheckResult

        // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
        private string _LABELCD_SLT_THICK = string.Empty;     // 슬리터 LOT 두께 바코드 LABELCD

        public CMM_ELEC_BARCODE()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            eqsgID = tmps[0] as string;
            procID = tmps[1] as string;
            eqptID = tmps[2] as string;

            if (tmps.Length.Equals(4))
                userName = tmps[3] as string;  // 재와인더 라벨발행 사용자

            if (procID.Equals(Process.SLITTING) || procID.Equals(Process.SRS_SLITTING))
            {
                // [E20230328-000520]Lot Label print 미리보기 개선건
                chkLanePrint.Visibility = Visibility.Visible;
                if (procID.Equals(Process.SLITTING))
                {
                    dgLotList.Columns["BARCODE_PREVIEW"].Visibility = Visibility.Visible;  
                    // [E20240115-000103] Slitter history card ->small tag
                    dgLotList.Columns["SKID_BARCODE"].Visibility = Visibility.Visible; 
                    dgLotList.Columns["SKID_BARCODE_PREVIEW"].Visibility = Visibility.Visible; 
                }

            }
            else if (procID.Equals(Process.SLIT_REWINDING))
                chkSkidCardPrint.Visibility = Visibility.Visible;
            else if (procID.Equals(Process.HEAT_TREATMENT))
                dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Collapsed;

            // [E20240717-000837] OC4동 라벨System변경 요청 件_IT서비스요청서(IM라벨) (E3000,E2100,E41000)
            if (procID.Equals(Process.ROLL_PRESSING) || procID.Equals(Process.SLIT_REWINDING) || procID.Equals(Process.REWINDING)) 
            {
                SetElecProc2DLabelUseYn();

                if (_ElecProc2DLabelUseYn.Equals("Y"))
                {
                    dgLotList.Columns["BARCODE_2D"].Visibility = Visibility.Visible; 
                    dgLotList.Columns["BARCODE_2D_PRT_COUNT"].Visibility = Visibility.Visible;
                }

                // [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정)
                SetElecProcContentLabelUseYn();

                if (_ElecProcContentLabelUseYn.Equals("Y"))
                {
                    dgLotList.Columns["CONTENT_LABEL"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CONTENT_LABEL_PRT_COUNT"].Visibility = Visibility.Visible;
                }
            }

           GetConfirmLotList();
        }
        #endregion

        #region TextBox Event
        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ( e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtLotId.Text.Trim()))
                {
                    GetConfirmLotList(txtLotId.Text.Trim());
                    txtLotId.Focus();
                    txtLotId.SelectAll();
                }
            }
        }
        #endregion
        #region Button Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetConfirmLotList(txtLotId.Text.Trim());
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            Util.PrintLabel_Test(FrameOperation, loadingIndicator);
        }

        private void btnGrid_Click(object sender, RoutedEventArgs e)
        {
            Button btnPrint = sender as Button;
            if ( btnPrint != null)
            {
                DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                skidID = string.Empty;
                string sLotID = string.Empty;
                string sSkidID = string.Empty;

                vQA_INSP_TRGT_FLAG = string.Empty;
                vLOTID = string.Empty;

                if (procID.Equals(Process.SLITTING) || procID.Equals(Process.SRS_SLITTING))
                {
                    sSkidID = Util.NVC(dataRow.Row["LOTID"]);
                    sLotID = Util.NVC(dataRow.Row["LOTID2"]);
                }
                else if (procID.Equals(Process.SLIT_REWINDING))
                {
                    skidID = Util.NVC(dataRow.Row["LOTID2"]);
                    sSkidID = Util.NVC(dataRow.Row["LOTID2"]);
                    sLotID = Util.NVC(dataRow.Row["LOTID"]);
                }
                else
                {
                    sLotID = Util.NVC(dataRow.Row["LOTID"]);

                    vLOTID = Util.NVC(dataRow.Row["LOTID"]);
                    vQA_INSP_TRGT_FLAG = Util.NVC(dataRow.Row["QA_INSP_TRGT_FLAG"]);
                    CUT_ID = Util.NVC(dataRow.Row["CHILD_GR_SEQNO"]);
                }

                if ( string.Equals( btnPrint.Name, "btnBarcode"))
                {
                    if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                    {
                        Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                        return;
                    }

                    if (chkLanePrint.IsChecked == true)
                    {
                        LGC.GMES.MES.CMM001.Popup.CMM_ELEC_LANE_BARCODE wndLaneBarcode = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_LANE_BARCODE();
                        wndLaneBarcode.FrameOperation = FrameOperation;
                        if (wndLaneBarcode != null)
                        {
                            object[] Parameters = new object[5];
                            Parameters[0] = sLotID;
                            Parameters[1] = procID;
                            Parameters[2] = dataRow;

                            C1WindowExtension.SetParameters(wndLaneBarcode, Parameters);

                            this.Dispatcher.BeginInvoke(new Action(() => wndLaneBarcode.ShowModal()));
                        }
                    }
                    else
                    {
                        string sEltrSltLotThicknessViewYn = string.Empty;                      // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                        if (procID.Equals(Process.SLITTING))
                        {
                            sEltrSltLotThicknessViewYn = GetEltrSltLotThicknessViewYn();       // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송

                            if (sEltrSltLotThicknessViewYn.Equals("Y") && _LABELCD_SLT_THICK.Equals(""))  // LABELCD_가 없는 경우
                            {
                                Util.MessageValidation("SFU4079");  //라벨 정보가 없습니다.
                                return;
                            }
                        }

                        DataTable printDT = GetPrintCount(sLotID, procID); 

                        if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT1"]) > 0)
                        {
                            // 해당 LOT BARCODE VALID 추가 [2017-09-25]
                            string sValidMsg = GetLabelPrintValid(sLotID, procID);

                            // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3463"), sValidMsg, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                            //Util.MessageConfirm("SFU3463", (sresult) =>
                            {
                                if (sresult == MessageBoxResult.OK)
                                {
                                    try
                                    {
                                        if (procID.Equals(Process.SLITTING) || procID.Equals(Process.SRS_SLITTING))
                                        {
                                            int iSamplingCount;
                                            DataTable dt = GetCutLotData(Util.NVC(dataRow.Row["LOTID"]));
                                            // C20200409-000294 : 기본 출하지 공란 라벨 -> CWA, CNA, CMI 우선순위 출하지 표기
                                            DataTable sampleDT = new DataTable();
                                            sampleDT.Columns.Add("CUT_ID", typeof(string));
                                            sampleDT.Columns.Add("LOTID", typeof(string));
                                            sampleDT.Columns.Add("COMPANY", typeof(string));
                                            DataRow dRow = null;

                                            foreach (DataRow _iRow in dt.Rows)
                                            {
                                                iSamplingCount = 0;
                                                string[] sCompany = null;
                                                foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                                {
                                                    iSamplingCount = Util.NVC_Int(items.Key);
                                                    sCompany = Util.NVC(items.Value).Split(',');
                                                }
                                                for (int i = 0; i < iSamplingCount; i++)
                                                {
                                                    dRow = sampleDT.NewRow();
                                                    dRow["CUT_ID"] = dataRow["LOTID"];
                                                    dRow["LOTID"] = _iRow["LOTID"];
                                                    dRow["COMPANY"] = i > sCompany.Length - 1 ? "" : sCompany[i];
                                                    sampleDT.Rows.Add(dRow);
                                                }
                                            }
                                            var sortdt = sampleDT.AsEnumerable().OrderBy(x => x.Field<string>("CUT_ID") + x.Field<string>("COMPANY")).CopyToDataTable();
                                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                            {
                                                for (int i = 0; i < sortdt.Rows.Count; i++)
                                                {

                                                    if (sEltrSltLotThicknessViewYn.Equals("Y") && procID.Equals(Process.SLITTING)) // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                                                    {
                                                        PrintLabel_Elec_Thick(FrameOperation, loadingIndicator, Util.NVC(sortdt.DefaultView[i]["LOTID"]), procID, Util.NVC(sortdt.DefaultView[i]["COMPANY"]));
                                                    }
                                                    else
                                                    {
                                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(sortdt.DefaultView[i]["LOTID"]), procID, Util.NVC(sortdt.DefaultView[i]["COMPANY"]));
                                                    }
                                                }
                                            }
                                            for (int i = 0; i < sortdt.Rows.Count; i++)
                                                Util.UpdatePrintExecCount(Util.NVC(sortdt.DefaultView[i]["LOTID"]), procID);
                                            //Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(row["LOTID"]), procID);

                                            // 회수 갱신
                                            DataTableConverter.SetValue(dataRow, "PRT_COUNT1", Util.NVC_Decimal(DataTableConverter.GetValue(dataRow, "PRT_COUNT1")) + 1);
                                        }
                                        else
                                        {
                                            // CSR : [C20220610-000415] - Barcode printing interlock
                                            // BARCODE_REPRINT_AUTH 통해 동과 공정 관리
                                            if (IsAreaCommonCodeUse("BARCODE_REPRINT_AUTH", procID))
                                            {
                                                sPrtLotid = sLotID;

                                                CMM_ELEC_AREA_CODE_AUTH authConfirm = new CMM_ELEC_AREA_CODE_AUTH();
                                                authConfirm.FrameOperation = FrameOperation;

                                                if (authConfirm != null)
                                                {
                                                    object[] Parameters = new object[2];
                                                    Parameters[0] = "BARCODE_REPRINT_AUTH";
                                                    Parameters[1] = procID;

                                                    C1WindowExtension.SetParameters(authConfirm, Parameters);

                                                    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);
                                                    this.Dispatcher.BeginInvoke(new Action(() => authConfirm.ShowModal()));
                                                }
                                            }
                                            else
                                            {

                                                int iSamplingCount = 0; // GetSamplingLabelQty();

                                                #region [샘플링 출하거래처 추가]
                                                string[] sCompany = null;
                                                foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(vLOTID))
                                                {
                                                    iSamplingCount = Util.NVC_Int(items.Key);
                                                    sCompany = Util.NVC(items.Value).Split(',');
                                                }
                                                #endregion

                                                //C20210415-000402 [2021-06-22]
                                                int iFirstCutCNT = 0;

                                                if (procID.Equals(Process.COATING) && (CUT_ID == "1"))
                                                    iFirstCutCNT = LoginInfo.CFG_LABEL_FIRST_CUT_COPIES;

                                                for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES + iFirstCutCNT; ii++)
                                                    for (int i = 0; i < iSamplingCount; i++)
                                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, sLotID, procID, i > sCompany.Length - 1 ? "" : sCompany[i], userName); // 재와인더 라벨 발행 사용자 추가(userID)

                                                Util.UpdatePrintExecCount(sLotID, procID);

                                                // 회수 갱신
                                                DataTableConverter.SetValue(dataRow, "PRT_COUNT1", Util.NVC_Decimal(DataTableConverter.GetValue(dataRow, "PRT_COUNT1")) + 1);
                                            }
                                        }
                                    }
                                    catch (Exception ex) { Util.MessageException(ex); }
                                }
                            });
                        }
                        else
                        {
                            try
                            {
                                if (procID.Equals(Process.SLITTING) || procID.Equals(Process.SRS_SLITTING))
                                {
                                    DataTable dt = GetCutLotData(Util.NVC(dataRow.Row["LOTID"]));
                                    int iSamplingCount;
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        iSamplingCount = 0;
                                        string[] sCompany = null;
                                        foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(row["LOTID"])))
                                        {
                                            iSamplingCount = Util.NVC_Int(items.Key);
                                            sCompany = Util.NVC(items.Value).Split(',');
                                        }
                                        for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                        {
                                            for (int i = 0; i < iSamplingCount; i++)
                                            {
                                                if (sEltrSltLotThicknessViewYn.Equals("Y") && procID.Equals(Process.SLITTING)) // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                                                {
                                                    PrintLabel_Elec_Thick(FrameOperation, loadingIndicator, Util.NVC(row["LOTID"]), procID, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                                }
                                                else
                                                {
                                                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(row["LOTID"]), procID, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                                }
                                            }
                                        }

                                        Util.UpdatePrintExecCount(Util.NVC(row["LOTID"]), procID);
                                    }


                                    //foreach (DataRow row in dt.Rows)
                                    //    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(row["LOTID"]), procID);
                                }
                                else
                                {
                                    //Util.PrintLabel_Elec(FrameOperation, loadingIndicator, sLotID, procID);

                                    int iSamplingCount = 0; 

                                    #region [샘플링 출하거래처 추가]
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(vLOTID))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }
                                    #endregion

                                    //C20210415-000402 [2021-06-22]
                                    int iFirstCutCNT = 0;

                                    if (procID.Equals(Process.COATING) && (CUT_ID == "1"))
                                        iFirstCutCNT = LoginInfo.CFG_LABEL_FIRST_CUT_COPIES;

                                    for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES + iFirstCutCNT; ii++)
                                        for (int i = 0; i < iSamplingCount; i++)
                                            Util.PrintLabel_Elec(FrameOperation, loadingIndicator, sLotID, procID, i > sCompany.Length - 1 ? "" : sCompany[i], userName); // 재와인더 라벨 발행 사용자 추가(userID)

                                    Util.UpdatePrintExecCount(sLotID, procID);
                                }

                                // 회수 갱신
                                DataTableConverter.SetValue(dataRow, "PRT_COUNT1", Util.NVC_Decimal(DataTableConverter.GetValue(dataRow, "PRT_COUNT1")) + 1);

                            }
                            catch (Exception ex) { Util.MessageException(ex); }
                        }
                    }
                }
                else if (string.Equals(btnPrint.Name, "btnBarcodePrev")) // [E20230328-000520]Lot Label print 미리보기 개선건
                { 
                    LGC.GMES.MES.CMM001.Popup.CMM_ELEC_LANE_BARCODE_PREV wndLaneBarcode = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_LANE_BARCODE_PREV();
                    wndLaneBarcode.FrameOperation = FrameOperation;
                    if (wndLaneBarcode != null)
                    {
                        object[] Parameters = new object[5];
                        Parameters[0] = sLotID;
                        Parameters[1] = procID;
                        Parameters[2] = dataRow;

                        C1WindowExtension.SetParameters(wndLaneBarcode, Parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => wndLaneBarcode.ShowModal()));
                    }

                }
                else if (string.Equals(btnPrint.Name, "btnHistoryCard"))  
                {
                    if(procID.Equals(Process.MIXING))
                    {
                        HistoryCardPopup2(sLotID);
                    }
                    else
                    {
                        if (string.Equals(procID, Process.HEAT_TREATMENT))
                        {
                            DataTable heatDt = GetHeatProcessPrintOut(sLotID);
                            if ( heatDt != null && heatDt.Rows.Count > 0 && Util.NVC_Decimal(heatDt.Rows[0]["WAIT_CONF_LOTQTY"]) > 0)
                            {
                                Util.MessageValidation("SFU4944", new object[] { Util.NVC(heatDt.Rows[0]["CUTID"]), Util.NVC(heatDt.Rows[0]["WAIT_CONF_LOTQTY"]) });    //열처리 공정에서는 동일CUT[%1]의 모든LOT이 확정되어야 이력카드발행 가능합니다. 현재 %2개의 LOT이 확정 되지 않았습니다.
                                return;
                            }
                        }
                        HistoryCardPopup(sLotID, sSkidID);
                    }
                }
                else if (string.Equals(btnPrint.Name, "btnSkidBarcodePrev")) // [E20240115-000103] Slitter history card ->small tag
                { 
                    LGC.GMES.MES.CMM001.Popup.CMM_ELEC_SKID_BARCODE_PREV wndLaneBarcode = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_SKID_BARCODE_PREV();
                    wndLaneBarcode.FrameOperation = FrameOperation;
                    if (wndLaneBarcode != null)
                    {
                        object[] Parameters = new object[5];
                        Parameters[0] = sLotID;
                        Parameters[1] = procID;
                        Parameters[2] = dataRow;
                        C1WindowExtension.SetParameters(wndLaneBarcode, Parameters);
                        this.Dispatcher.BeginInvoke(new Action(() => wndLaneBarcode.ShowModal()));
                    }
                }
                if (string.Equals(btnPrint.Name, "btnSkidBarcode"))  // [E20240115-000103] Slitter history card ->small tag
                {
                    if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                    {
                        Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                        return;
                    }
                    SkidBarcodeLablePrint_Elec(sLotID); // Skid Lable(Barcode) Print 
                      
                }

                if (string.Equals(btnPrint.Name, "btnBarcode2D"))  // [E20240717-000837] OC4동 라벨System변경 요청 件_IT서비스요청서(IM라벨)
                {
                    if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                    {
                        Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                        return;
                    }

                    string sBarcode2DPrint_Yn = Util.NVC(dataRow.Row["BARCODE_2D_PRINT_YN"]);     

                    if (!sBarcode2DPrint_Yn.Equals("Y"))
                    {
                        Util.MessageValidation("SFU9221"); // 2D 바코드 출력 대상 LOT이 아닙니다.
                        return;
                    }

                    string sWipSeq = Util.NVC(dataRow.Row["WIPSEQ"]);
                    string sWipdttmEd = Util.NVC(dataRow.Row["WIPDTTM_ED"]);
                    string sGoodqtyLane = Util.NVC(dataRow.Row["GOODQTY_LANE"]);
                    string sGoodqtyLaneUnit_2D = Util.NVC(dataRow.Row["GOODQTY_LANE_UNIT_2D"]);

                    string sLABELCD_2D = Util.NVC(dataRow.Row["LABELCD_2D"]);                 // 2D LABELCD (IM)
                    string sBarcode2D_OutCnt = Util.NVC(dataRow.Row["BARCODE_2D_OUT_COUNT"]); // 고객사 2D Lable(IM) 출력수  
                    string sPartNumber_2D = Util.NVC(dataRow.Row["PART_NUMBER_2D"]);                // Part Number 
                    string sPartDescription_2D = Util.NVC(dataRow.Row["PART_DESCRIPTION_2D"]);      // Part Description 

                    Barcode2DLablePrint_Elec(sLotID, sWipSeq, sWipdttmEd, sGoodqtyLane, sGoodqtyLaneUnit_2D, sLABELCD_2D, sBarcode2D_OutCnt, sPartNumber_2D, sPartDescription_2D);

                    // 회수 갱신
                    // DataTableConverter.SetValue(dataRow, "BARCODE_2D_PRT_COUNT", Util.NVC_Decimal(DataTableConverter.GetValue(dataRow, "BARCODE_2D_PRT_COUNT")) + int.Parse(sBarcode2D_OutCnt));

                }

                if (string.Equals(btnPrint.Name, "btnContentLabel"))    // [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정)
                {
                    if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                    {
                        Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                        return;
                    }

                    string sContentLabelPrintYn = Util.NVC(dataRow.Row["CONTENT_LABEL_PRINT_YN"]);

                    if (!sContentLabelPrintYn.Equals("Y"))
                    {
                        Util.MessageValidation("SFU9223"); // 컨텐츠라벨 발행 대상 LOT이 아닙니다.
                        return;
                    }

                    string sWipSeq_ContentLabel = Util.NVC(dataRow.Row["WIPSEQ"]);
                    string sWipdttmEd_ContentLabel = Util.NVC(dataRow.Row["WIPDTTM_ED"]);
                    string sGoodLaneQty_ContentLabel = Util.NVC(dataRow.Row["GOODQTY_LANE"]);
                    string sLABELCD_ContentLabel = Util.NVC(dataRow.Row["LABELCD_CONTENT_LABEL"]);    // Content Label LABELCD  
                    string sContentLabel_OutCount = Util.NVC(dataRow.Row["CONTENT_LABEL_OUT_COUNT"]); // 고객사 Content Label 출력수  
                    string sTeslaPartNumber = Util.NVC(dataRow.Row["TESLA_PART_NUMBER"]);       // Tesla Part Number

                    ContentLabelPrint_Elec(sLotID, sWipSeq_ContentLabel, sWipdttmEd_ContentLabel, sGoodLaneQty_ContentLabel, sLABELCD_ContentLabel, sContentLabel_OutCount, sTeslaPartNumber);

                }

                int rowIndex = -1;
                for (int i = 0; i < dgLotList.Rows.Count; i++)
                {
                    if (string.Equals(sLotID, DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID")))
                    {
                        rowIndex = i;
                        break;
                    }
                }

                if (rowIndex > -1)
                {
                    if (dgLotList.CurrentCell.Presenter != null)
                        dgLotList.CurrentCell.Presenter.IsSelected = false;

                    dgLotList.CurrentCell = dgLotList.GetCell(rowIndex, dgLotList.Columns["LOTID"].Index);
                    dgLotList.GetCell(rowIndex, dgLotList.Columns["LOTID"].Index).Presenter.IsSelected = true;
                    
                }
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region User Method
        private void GetConfirmLotList(string sLotID = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(sLotID) && sLotID.Trim().Length < 7)
                {
                    Util.MessageInfo("SFU3477");    // 바코드 조회용 LOTID는 7자리 이상 입력해 주세요. 
                    return;
                }

                Util.gridClear(dgLotList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("DATEADD", typeof(Int16));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = eqsgID;
                Indata["PROCID"] = procID;
                Indata["EQPTID"] = eqptID;
                Indata["LOTID"] = sLotID;
                Indata["DATEADD"] = radNow.IsChecked == true ? radNow.Tag : radBefore.Tag;

                IndataTable.Rows.Add(Indata);
                if (procID.Equals(Process.SLITTING) || procID.Equals(Process.SRS_SLITTING))
                {
                    // LOTLIST_GRID = (dgLotList as CMM_ELEC_BARCODE).dgLotInfo;
                    dgLotList.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("SKIDID");

                    // CUT_ID로 입력될시 LOTID로 치환
                    if (!string.IsNullOrEmpty(sLotID))
                    {
                        DataTable cutList = GetCutLotData(sLotID);
                        if (cutList.Rows.Count > 0)
                            Indata["LOTID"] = Util.NVC(cutList.Rows[0]["LOTID"]);
                    }

                    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_SLITTING_V01", "INDATA", "RSLTDT", IndataTable);
                    if (dt.Rows.Count == 0)
                    {
                        if (string.IsNullOrEmpty(sLotID))
                            Util.MessageValidation("SFU2986", new object[] { eqptID });//해당 설비({%1})에 24시간 이내에 확정된 LOT이 존재하지 않습니다.
                        else
                            Util.MessageValidation("SFU3423");

                        return;
                    }
                    Util.GridSetData(dgLotList, dt, FrameOperation, true);
                }
                else
                {
                    dgLotList.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("LOTID");
                    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_LOT_V01", "INDATA", "RSLTDT", IndataTable);
                    if (dt.Rows.Count == 0)
                    {
                        if (string.IsNullOrEmpty(sLotID))
                            Util.MessageValidation("SFU2986", new object[] { eqptID });//해당 설비({%1})에 24시간 이내에 확정된 LOT이 존재하지 않습니다.
                        else
                            Util.MessageValidation("SFU3423");
                    }
                    Util.GridSetData(dgLotList, dt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HistoryCardPopup(string sLotID, string sSkidID)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = sLotID;
                Parameters[1] = procID;
                Parameters[2] = sSkidID;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseHistoryCard);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseHistoryCard(object sender, EventArgs e)
        {
            if (string.Equals(procID, Process.SLIT_REWINDING) && chkSkidCardPrint.IsChecked == true)
            {
                DataTable dtPackingCard = new DataTable();

                dtPackingCard.Columns.Add("Title", typeof(string));
                dtPackingCard.Columns.Add("SKID_ID", typeof(string));
                dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                dtPackingCard.Columns.Add("PROJECT", typeof(string));
                dtPackingCard.Columns.Add("PRODID", typeof(string));
                dtPackingCard.Columns.Add("VER", typeof(string));
                dtPackingCard.Columns.Add("QTY", typeof(string));
                dtPackingCard.Columns.Add("UNIT", typeof(string));
                dtPackingCard.Columns.Add("PRODDATE", typeof(string));
                dtPackingCard.Columns.Add("VLDDATE", typeof(string));
                dtPackingCard.Columns.Add("TOTAL_QTY", typeof(string));

                DataRow drCrad = null;

                drCrad = dtPackingCard.NewRow();

                drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극SKID구성카드"),
                                                  skidID,
                                                  skidID,
                                                  ObjectDic.Instance.GetObjectName("PJT"),
                                                  ObjectDic.Instance.GetObjectName("반제품"),
                                                  ObjectDic.Instance.GetObjectName("버전"),
                                                  ObjectDic.Instance.GetObjectName("수량"),
                                                  ObjectDic.Instance.GetObjectName("단위"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  ObjectDic.Instance.GetObjectName("유효기간"),
                                                  ObjectDic.Instance.GetObjectName("총 수량")
                                               };

                dtPackingCard.Rows.Add(drCrad);

                LGC.GMES.MES.CMM001.CMM_ELEC_SKID_CARD skidCard = new LGC.GMES.MES.CMM001.CMM_ELEC_SKID_CARD();
                skidCard.FrameOperation = this.FrameOperation;

                if (skidCard != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = "Report_SkidCard";
                    Parameters[1] = skidID;
                    Parameters[2] = dtPackingCard;

                    C1WindowExtension.SetParameters(skidCard, Parameters);

                    skidCard.RunSkidPrint();
                }
            }
        }

        private void HistoryCardPopup2(string sLotID)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REPORT3 wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT3();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = sLotID;
                Parameters[1] = procID;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private DataTable GetCutLotData(string sCutID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("CUT_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["CUT_ID"] = sCutID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_BY_CUT_ID", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
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

        private DataTable GetHeatProcessPrintOut(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = Process.HEAT_TREATMENT;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_WIP_HT_ISS_HIST_CARD", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }
        //바코드 발행 실행 횟수 업데이트

        private string GetLabelPrintValid(string sLotID, string sProcID)
        {
            string returnMsg = string.Empty;

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = (string.Equals(sProcID, Process.SLITTING) || string.Equals(sProcID, Process.SRS_SLITTING)) ? sLotID.Substring(0, 9) : sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync((string.Equals(sProcID, Process.SLITTING) || string.Equals(sProcID, Process.SRS_SLITTING)) ?
                    "DA_PRD_SEL_PROCESS_LOT_LABEL_VALID_CUT" : "DA_PRD_SEL_PROCESS_LOT_LABEL_VALID", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    if (string.Equals(sProcID, Process.SLITTING) || string.Equals(sProcID, Process.SRS_SLITTING))
                    {
                        foreach (DataRow row in result.Rows)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["RACK_ID"])))
                            {
                                returnMsg = MessageDic.Instance.GetMessage("SFU4125", row["RACK_ID"]); // 보관렉[%1]에 동일한 LOT 이 보관 중입니다.
                                break;
                            }
                            else if (string.Equals(row["MOVE_ORD_STAT_CODE"], "CLOSE_MOVE"))
                            {
                                returnMsg = MessageDic.Instance.GetMessage("SFU4126"); // 조립 인계 완료된 LOT 입니다.
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["RACK_ID"])))
                            returnMsg = MessageDic.Instance.GetMessage("SFU4125", result.Rows[0]["RACK_ID"]); // 보관렉[%1]에 동일한 LOT 이 보관 중입니다.
                        else if (string.Equals(result.Rows[0]["MOVE_ORD_STAT_CODE"], "CLOSE_MOVE"))
                            returnMsg = MessageDic.Instance.GetMessage("SFU4126"); // 조립 인계 완료된 LOT 입니다.
                    }
                }
            }
            catch (Exception ex) { }

            return returnMsg;
        }

        #region [Sampling]

        // 샘플링 라벨발행 수량 / 출하처
        private Dictionary<int, string> getSamplingLabelInfo(string sLotID)
        {
            if ((string.Equals(procID, Process.ROLL_PRESSING) || (string.Equals(procID, Process.SLITTING))) && string.Equals(getQAInspectFlag(sLotID), "Y"))
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PROCID"] = procID;
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                 DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_CHK_LOT_T1", "INDATA", "OUT_DATA", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return new Dictionary<int, string> { { Util.NVC_Int(dtMain.Rows[0]["OUT_PRINTCNT"]), Util.NVC(dtMain.Rows[0]["OUT_COMPANY"]) } };
            }

            return new Dictionary<int, string> { { 1, string.Empty } };
        }

        private string getQAInspectFlag(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count == 1)
                    return Util.NVC(string.Equals(procID, Process.ROLL_PRESSING) ? result.Rows[0]["QA_INSP_TRGT_FLAG"] : result.Rows[0]["SLIT_QA_INSP_TRGT_FLAG"]);
            }
            catch (Exception ex) { }

            return "";
        }


        #endregion
        #region [Skid BarCode Label Print]
        // [E20240115-000103] Slitter history card ->small tag
        private DataTable GetSkidLabelInfo(string sLotid)
        {
            DataTable dtSkid = new DataTable();
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = "en-US";   // ZPL 출력시 한글깨지는 문제때문에영문으로 지정 LoginInfo.LANGID;
                Indata["LOTID"] = sLotid;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_SKID_LABEL_ELEC", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    dtSkid = result;
                    return dtSkid;
                }
            }
            catch (Exception ex)
            {
                return dtSkid;
            }

            return dtSkid;
        }
        // [E20240115-000103] Slitter history card ->small tag
        private void SkidBarcodeLablePrint_Elec(string sLotid)
        {  
            try
            {
                DataRow drPrintInfo;
                string sPrintType;
                string sResolution;
                string sIssueCount;
                string sXposition;
                string sYposition;
                string sDarkness;
                string sPortName;
                string sblCode = "LBL0325";

                // 체크된 라벨 정보 확인
                if (!Util.GetConfigPrintInfoPack(out sPrintType, out sResolution, out sIssueCount, out sXposition, out sYposition, out sDarkness, out sPortName, out drPrintInfo))
                {
                    Util.MessageValidation("SFU3030");  //프린터 환경설정 정보가 없습니다.
                    return ;
                }

                DataTable  dtLotItem01 = GetSkidLabelInfo(sLotid);

                if (dtLotItem01 == null || dtLotItem01.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1195"); // Lot 정보가 없습니다.
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("RESO", typeof(string));   // 해상도
                inTable.Columns.Add("PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("DARK", typeof(string));   // strDarkness 
                inTable.Columns.Add("ATTVAL001", typeof(string));   // ATTVAL001
                inTable.Columns.Add("ATTVAL002", typeof(string));   // ATTVAL002
                inTable.Columns.Add("ATTVAL003", typeof(string));   // ATTVAL003
                inTable.Columns.Add("ATTVAL004", typeof(string));   // ATTVAL004
                inTable.Columns.Add("ATTVAL005", typeof(string));   // ATTVAL005
                inTable.Columns.Add("ATTVAL006", typeof(string));   // ATTVAL006
                inTable.Columns.Add("ATTVAL007", typeof(string));   // ATTVAL007
                inTable.Columns.Add("ATTVAL008", typeof(string));   // ATTVAL008
                inTable.Columns.Add("ATTVAL009", typeof(string));   // ATTVAL009
                inTable.Columns.Add("ATTVAL010", typeof(string));   // ATTVAL010
                inTable.Columns.Add("ATTVAL011", typeof(string));   // ATTVAL011
                inTable.Columns.Add("ATTVAL012", typeof(string));   // ATTVAL012
                inTable.Columns.Add("ATTVAL013", typeof(string));   // ATTVAL013
                inTable.Columns.Add("ATTVAL014", typeof(string));   // ATTVAL014
                inTable.Columns.Add("ATTVAL015", typeof(string));   // ATTVAL015
                inTable.Columns.Add("ATTVAL016", typeof(string));   // ATTVAL016
                inTable.Columns.Add("ATTVAL017", typeof(string));   // ATTVAL017
                inTable.Columns.Add("ATTVAL018", typeof(string));   // ATTVAL018 

                DataRow Indata = inTable.NewRow();
                Indata["LBCD"] = sblCode; // 라벨코드  : DB에서호출방식으로 변경 필요
                Indata["PRMK"] = sPrintType; // 프린터기종
                Indata["RESO"] = sResolution; // 해상도
                Indata["PRCN"] = sIssueCount; // 출력매수
                Indata["MARH"] = sXposition; // 시작위치H
                Indata["MARV"] = sYposition; // 시작위치V
                Indata["DARK"] = sDarkness; //  Darkness

                Indata["ATTVAL001"] = dtLotItem01.Rows[0]["PRODID"].ToString();
                Indata["ATTVAL002"] = dtLotItem01.Rows[0]["PRJT_NAME"].ToString();
                Indata["ATTVAL003"] = dtLotItem01.Rows[0]["PROD_VER_CODE"].ToString();
                Indata["ATTVAL004"] = dtLotItem01.Rows[0]["SKID_ID"].ToString();
                Indata["ATTVAL005"] = dtLotItem01.Rows[0]["ELEC_TYPE"].ToString();
                Indata["ATTVAL006"] = dtLotItem01.Rows[0]["SKID_ID"].ToString();
                Indata["ATTVAL007"] = dtLotItem01.Rows[0]["LANE_QTY"].ToString();
                Indata["ATTVAL008"] = dtLotItem01.Rows[0]["OUTPUT_CR0_QTY"].ToString() + dtLotItem01.Rows[0]["UNIT_CODE_NAME"].ToString();
                Indata["ATTVAL009"] = dtLotItem01.Rows[0]["COATING_LINE"].ToString();
                Indata["ATTVAL010"] = dtLotItem01.Rows[0]["VLD_DATE"].ToString();  
                Indata["ATTVAL011"] = ""; // ATTVAL011
                Indata["ATTVAL012"] = ""; // ATTVAL012
                Indata["ATTVAL013"] = ""; // ATTVAL013
                Indata["ATTVAL014"] = ""; // ATTVAL014
                Indata["ATTVAL015"] = ""; // ATTVAL015
                Indata["ATTVAL016"] = ""; // ATTVAL016
                Indata["ATTVAL017"] = ""; // ATTVAL017
                Indata["ATTVAL018"] = ""; // ATTVAL018 
                inTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM15", "INDATA", "RSLTDT", inTable);
                //인쇄 요청

                if (result != null && result.Rows.Count > 0)
                {
                    string zplCode = Util.NVC(result.Rows[0]["LABELCD"]); //바코드라벨 ZPL

                    if (zplCode.StartsWith("0,"))
                    {
                        zplCode = zplCode.Substring(2);

                        for (int iPrint = 0; iPrint < LoginInfo.CFG_LABEL_COPIES; iPrint++)
                        {
                            System.Threading.Thread.Sleep(500);
                            Util.PrintLabel(FrameOperation, loadingIndicator, zplCode);
                        }
                        return ;
                    }
                    else
                    {
                        Util.MessageInfo(zplCode.Substring(2));
                        return ;
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageValidation("SFU1309");  //바코드 프린터 실패
                return ;
            }

            return ;
        }
        #endregion [Skid BarCode Label Print]

        // [E20240717-000837] OC4동 라벨System변경 요청 件_IT서비스요청서(IM라벨)
        #region [IM BarCode Label Print Start]
        // [E20240717-000837] OC4동 라벨System변경 요청 件_IT서비스요청서(IM라벨)
        private void SetElecProc2DLabelUseYn()
        {

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELEC_PROC_2D_LABEL_USE_YN";  // 전극 공정별 2D Label 사용 여부
            sCmCode = procID;                         // 공정 

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
                    _ElecProc2DLabelUseYn = "Y";
                }
                else
                {
                    _ElecProc2DLabelUseYn = "N";
                }

                return;
            }
            catch (Exception ex)
            { 
                _ElecProc2DLabelUseYn = "N";
                //Util.MessageException(ex);
                return;
            }
        }

        // [E20240717-000837] OC4동 라벨System변경 요청 件_IT서비스요청서(IM라벨)
        private void Barcode2DLablePrint_Elec(string sLotid, string sWipSeq, string sWipdttmEd, string sGoodQtyLang,string sGoodqtyLaneUnit_2D, string sLABELCD_2D, string sBarcode2D_OutCnt, string sPartNumber_2D, string sPartDescription_2D)
        {
             
            try
            {
                DataRow drPrintInfo;
                string sPrintType;
                string sResolution;
                string sIssueCount;
                string sXposition;
                string sYposition;
                string sDarkness;
                string sPortName;
                string sblCode = sLABELCD_2D;
                string sLotCode = string.Empty; 

                int iBarcode2D_OutCnt = 0; 

                // 발행수
                if (sBarcode2D_OutCnt.Equals(""))
                {
                    iBarcode2D_OutCnt = 0;
                }
                else
                {
                    iBarcode2D_OutCnt = int.Parse(sBarcode2D_OutCnt);
                }

                sLotCode = GetTeslaLabelLotCode(sLotid);   // Tesla Lable Lot_Code

                if (!_LotCodeCheckResult.Equals("SUCCESS"))
                {
                    if(_LotCodeCheckResult.Equals("ERR_MMD"))
                    {
                        Util.MessageValidation("SFU9222");  // 설비에 대한 호기 기준정보가 없습니다.(Tesla)
                    }
                    if (_LotCodeCheckResult.Equals("ERR_RESULT") || _LotCodeCheckResult.Equals(""))
                    {
                        Util.MessageValidation("SFU9226", sLotid);  // 해당 LOT[%1]은 코터 공정 또는 롤프레스 공정 생산 실적이 없습니다.
                    }
                    return;
                }
                if (sLotCode.Equals(""))
                {
                    Util.MessageValidation("SFU9226", sLotid);  // 해당 LOT[%1]은 코터 공정 또는 롤프레스 공정 생산 실적이 없습니다.
                    return;
                }


                // 체크된 라벨 정보 확인
                if (!Util.GetConfigPrintInfoPack(out sPrintType, out sResolution, out sIssueCount, out sXposition, out sYposition, out sDarkness, out sPortName, out drPrintInfo))
                {
                    Util.MessageValidation("SFU3030");  //프린터 환경설정 정보가 없습니다.
                    return;
                }
                
                // LABELCD_2D가 없는 경우
                if (sblCode.Equals(""))
                {
                    Util.MessageValidation("SFU4079");  //라벨 정보가 없습니다.
                    return;
                }

                if (sLotid.Equals(""))
                {
                    Util.MessageValidation("SFU1195"); // Lot 정보가 없습니다.
                    return;
                } 

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LBCD",      typeof(string));   // 라벨코드
                inTable.Columns.Add("PRMK",      typeof(string));   // 프린터기종
                inTable.Columns.Add("RESO",      typeof(string));   // 해상도
                inTable.Columns.Add("PRCN",      typeof(string));   // 출력매수
                inTable.Columns.Add("MARH",      typeof(string));   // 시작위치H
                inTable.Columns.Add("MARV",      typeof(string));   // 시작위치V
                inTable.Columns.Add("DARK",      typeof(string));   // strDarkness 
                inTable.Columns.Add("ATTVAL001", typeof(string));   // ATTVAL001
                inTable.Columns.Add("ATTVAL002", typeof(string));   // ATTVAL002
                inTable.Columns.Add("ATTVAL003", typeof(string));   // ATTVAL003
                inTable.Columns.Add("ATTVAL004", typeof(string));   // ATTVAL004
                inTable.Columns.Add("ATTVAL005", typeof(string));   // ATTVAL005
                inTable.Columns.Add("ATTVAL006", typeof(string));   // ATTVAL006
                inTable.Columns.Add("ATTVAL007", typeof(string));   // ATTVAL007
                inTable.Columns.Add("ATTVAL008", typeof(string));   // ATTVAL008
                inTable.Columns.Add("ATTVAL009", typeof(string));   // ATTVAL009
                inTable.Columns.Add("ATTVAL010", typeof(string));   // ATTVAL010
                inTable.Columns.Add("ATTVAL011", typeof(string));   // ATTVAL011
                inTable.Columns.Add("ATTVAL012", typeof(string));   // ATTVAL012
                inTable.Columns.Add("ATTVAL013", typeof(string));   // ATTVAL013
                inTable.Columns.Add("ATTVAL014", typeof(string));   // ATTVAL014
                inTable.Columns.Add("ATTVAL015", typeof(string));   // ATTVAL015
                inTable.Columns.Add("ATTVAL016", typeof(string));   // ATTVAL016
                inTable.Columns.Add("ATTVAL017", typeof(string));   // ATTVAL017
                inTable.Columns.Add("ATTVAL018", typeof(string));   // ATTVAL018 

                DataRow Indata = inTable.NewRow();
                Indata["LBCD"] = sblCode;                                        // 라벨코드  
                Indata["PRMK"] = sPrintType;                                     // 프린터기종
                Indata["RESO"] = sResolution;                                    // 해상도
                Indata["PRCN"] = sIssueCount;                                    // 출력매수
                Indata["MARH"] = sXposition;                                     // 시작위치H
                Indata["MARV"] = sYposition;                                     // 시작위치V
                Indata["DARK"] = sDarkness;                                      //  Darkness

                Indata["ATTVAL001"] = sPartNumber_2D;                            // 1) Part Number : 고정
                Indata["ATTVAL002"] = sLotCode;                                  // 2) Batch Number : LG+코터호기+롤프레스호기+ LOTID
                Indata["ATTVAL003"] = sPartDescription_2D;                       // 3) Part Description : 고정
                Indata["ATTVAL004"] = sWipdttmEd.Replace("-", "/");              // 4) Manufacture Date : 랏 종료시점
                Indata["ATTVAL005"] = sGoodQtyLang + " " + sGoodqtyLaneUnit_2D;  // 5) Quantity : 양품수량(ea) (양품수량(lane)기준) 단위
                Indata["ATTVAL006"] = sPartNumber_2D + ":"+ sLotCode;            // 6) QR CODE : Part Number:Batch Number : LG+코터호기+롤프레스호기+ LOTID
                Indata["ATTVAL007"] = "";                                        // ATTVAL007
                Indata["ATTVAL008"] = "";                                        // ATTVAL008
                Indata["ATTVAL009"] = "";                                        // ATTVAL009
                Indata["ATTVAL010"] = "";                                        // ATTVAL010
                Indata["ATTVAL011"] = "";                                        // ATTVAL011
                Indata["ATTVAL012"] = "";                                        // ATTVAL012
                Indata["ATTVAL013"] = "";                                        // ATTVAL013
                Indata["ATTVAL014"] = "";                                        // ATTVAL014
                Indata["ATTVAL015"] = "";                                        // ATTVAL015
                Indata["ATTVAL016"] = "";                                        // ATTVAL016
                Indata["ATTVAL017"] = "";                                        // ATTVAL017
                Indata["ATTVAL018"] = "";                                        // ATTVAL018 
                inTable.Rows.Add(Indata); 

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM15", "INDATA", "RSLTDT", inTable);
                //인쇄 요청

                if (result != null && result.Rows.Count > 0)
                {
                    string zplCode = Util.NVC(result.Rows[0]["LABELCD"]); //바코드라벨 ZPL

                    if (zplCode.StartsWith("0,"))
                    {
                        zplCode = zplCode.Substring(2);

                        for (int iPrint = 0; iPrint < iBarcode2D_OutCnt; iPrint++)
                        {
                            System.Threading.Thread.Sleep(500);
                            Util.PrintLabel(FrameOperation, loadingIndicator, zplCode);

                            //라벨 발행 카운터  sLotid 
                            UpdateBarcode2DPrintCount(sLotid);                        
                        }

                        GetConfirmLotList(txtLotId.Text.Trim()); // 목록 재조회

                        return;
                    }
                    else
                    {
                        Util.MessageInfo(zplCode.Substring(2));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageValidation("SFU1309");  //바코드 프린터 실패
                return;
            }

            return;
        }

        private string GetTeslaLabelLotCode(string sLotid)
        {
            string sTeslaLabelLotCode = ""; 

            _LotCodeCheckResult = "";    // Tesla Lable Lot_Code CheckResult
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata   = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = procID;
                Indata["LOTID"]  = sLotid;
                 
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TESLA_LABEL_LOT_CODE_ELEC", "INDATA", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _LotCodeCheckResult = Util.NVC(dtResult.Rows[0]["CHECK_RESULT"].ToString());
                    if (_LotCodeCheckResult.Equals("SUCCESS"))  // 채번이 정상인 경우
                    {
                        sTeslaLabelLotCode = Util.NVC(dtResult.Rows[0]["LOT_CODE"].ToString());
                    }
                    else
                    {
                        sTeslaLabelLotCode = "";
                    }
                }
                else
                {
                    sTeslaLabelLotCode = "";
                    _LotCodeCheckResult = "ERR_RESULT";
                }
            }
            catch (Exception ex)
            {
                _LotCodeCheckResult = "ERR_RESULT";
                sTeslaLabelLotCode = "";
                return sTeslaLabelLotCode;
            }

            return sTeslaLabelLotCode;
        }
 
        private void UpdateBarcode2DPrintCount(string sLotid ) // 재공 2D 바코드 인쇄 횟수 업데이트
        {
            try
            {
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string)); 
                inDataTable.Columns.Add("USERID", typeof(string));   


                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = sLotid; 
                dr["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_TB_SFC_ELTR_LABEL_PRT_COUNT_2D_PRT_COUNT", "INDATA", null, inDataTable);

                return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }

        #endregion [IM BarCode Label Print End]

        // [E20240718-001228] OC4동 라벨System변경 요청 件(컨텐츠라벨 수정)
        #region [CONTENT_LABE Print Start] 
        private void SetElecProcContentLabelUseYn()
        {

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELEC_PROC_IM_CONTENT_LABEL_USE_YN";  // 전극 공정별 IM_CONTENT Label 사용 여부
            sCmCode = procID;                         // 공정 

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
                    _ElecProcContentLabelUseYn = "Y";
                }
                else
                {
                    _ElecProcContentLabelUseYn = "N";
                }

                return;
            }
            catch (Exception ex)
            {
                _ElecProcContentLabelUseYn = "N";
                //Util.MessageException(ex);
                return;
            }
        }


        private void ContentLabelPrint_Elec(string sLotid, string sWipSeq_ContentLabel, string sWipdttmEd_ContentLabel, string sGoodLaneQty_ContentLabel, string sLABELCD_ContentLabel, string sContentLabel_OutCount, string sTeslaPartNumber)
        {

            try
            {
                DataRow drPrintInfo;
                string sPrintType;
                string sResolution;
                string sIssueCount;
                string sXposition;
                string sYposition;
                string sDarkness;
                string sPortName;
                string sblCode = sLABELCD_ContentLabel;
                string sLotCode = string.Empty;
                string sContentLabelId = string.Empty;
                string sLotGrossWeight = string.Empty;

                DataTable dtContentItems = new DataTable();  

                int iContentLabel_OutCnt = 0;

                // 발행수
                if (sContentLabel_OutCount.Equals(""))
                {
                    iContentLabel_OutCnt = 0;
                }
                else
                {
                    iContentLabel_OutCnt = int.Parse(sContentLabel_OutCount);
                }

                // 체크된 라벨 정보 확인
                if (!Util.GetConfigPrintInfoPack(out sPrintType, out sResolution, out sIssueCount, out sXposition, out sYposition, out sDarkness, out sPortName, out drPrintInfo))
                {
                    Util.MessageValidation("SFU3030");  //프린터 환경설정 정보가 없습니다.
                    return;
                }

                // LABELCD_가 없는 경우
                if (sblCode.Equals(""))
                {
                    Util.MessageValidation("SFU4079");  //라벨 정보가 없습니다.
                    return;
                }

                if (sTeslaPartNumber.Equals(""))
                {
                    Util.MessageValidation("SFU9224");  // Tesla Part Number가 없습니다.
                    return;
                }


                if (sLotid.Equals(""))
                {
                    Util.MessageValidation("SFU1195"); // Lot 정보가 없습니다.
                    return;
                }

                sLotCode = GetTeslaLabelLotCodeContentLabe(sLotid);   // Tesla Lable Lot_Code Content Labe

                if (!_LotCodeContentLabeCheckResult.Equals("SUCCESS"))
                {
                    if (_LotCodeContentLabeCheckResult.Equals("ERR_MMD"))
                    {
                        Util.MessageValidation("SFU9222");  // 설비에 대한 호기 기준정보가 없습니다.(Tesla)
                    }
                    if (_LotCodeContentLabeCheckResult.Equals("ERR_RESULT") || _LotCodeContentLabeCheckResult.Equals(""))
                    {
                        Util.MessageValidation("SFU9226", sLotid);  // 해당 LOT[%1]은 코터 공정 또는 롤프레스 공정 생산 실적이 없습니다.
                    }
                    return;
                }
                if (sLotCode.Equals(""))
                {
                    Util.MessageValidation("SFU9226", sLotid);  // 해당 LOT[%1]은 코터 공정 또는 롤프레스 공정 생산 실적이 없습니다.
                    return;
                }


                sLotGrossWeight = GetsLotGrossWeight(sLotid);  // Gross Weight

                 dtContentItems = GetContentLabelBarcodeItems( sLotid, sLotCode, sTeslaPartNumber, sGoodLaneQty_ContentLabel, sLotGrossWeight, sWipdttmEd_ContentLabel);


                if (dtContentItems == null && dtContentItems.Rows.Count == 0)
                {
                     Util.MessageValidation("SFU9225"); // 컨텐츠라벨 바코드 정보가 없습니다.
                    return;
                 }

                sContentLabelId = dtContentItems.Rows[0]["CONTENT_LABEL_ID"].ToString();

                if (sContentLabelId.Equals(""))
                {
                     Util.MessageValidation("SFU9227"); // Content Label ID가 없습니다.
                    return;
                } 

                int iBigQRSize = 0;  //Big QR 이미지 사이즈

                if(sResolution.Equals("203"))  // 해상도 203
                {
                    iBigQRSize = 560;
                }
                else if (sResolution.Equals("300"))  // 해상도 300
                {
                    iBigQRSize = 880;
                }
                else if (sResolution.Equals("600"))  // 해상도 600
                {
                    iBigQRSize = 880;
                }
                else  
                {
                    iBigQRSize = 560;
                }

                sContentLabelId = createQRCODE(sContentLabelId, iBigQRSize); //Big QRCode ZIP 스크립터 생성

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LBCD", typeof(string));        // 라벨코드
                inTable.Columns.Add("PRMK", typeof(string));        // 프린터기종
                inTable.Columns.Add("RESO", typeof(string));        // 해상도
                inTable.Columns.Add("PRCN", typeof(string));        // 출력매수
                inTable.Columns.Add("MARH", typeof(string));        // 시작위치H
                inTable.Columns.Add("MARV", typeof(string));        // 시작위치V
                inTable.Columns.Add("DARK", typeof(string));        // strDarkness 
                inTable.Columns.Add("ATTVAL001", typeof(string));   // ATTVAL001
                inTable.Columns.Add("ATTVAL002", typeof(string));   // ATTVAL002
                inTable.Columns.Add("ATTVAL003", typeof(string));   // ATTVAL003
                inTable.Columns.Add("ATTVAL004", typeof(string));   // ATTVAL004
                inTable.Columns.Add("ATTVAL005", typeof(string));   // ATTVAL005
                inTable.Columns.Add("ATTVAL006", typeof(string));   // ATTVAL006
                inTable.Columns.Add("ATTVAL007", typeof(string));   // ATTVAL007
                inTable.Columns.Add("ATTVAL008", typeof(string));   // ATTVAL008
                inTable.Columns.Add("ATTVAL009", typeof(string));   // ATTVAL009
                inTable.Columns.Add("ATTVAL010", typeof(string));   // ATTVAL010
                inTable.Columns.Add("ATTVAL011", typeof(string));   // ATTVAL011
                inTable.Columns.Add("ATTVAL012", typeof(string));   // ATTVAL012
                inTable.Columns.Add("ATTVAL013", typeof(string));   // ATTVAL013
                inTable.Columns.Add("ATTVAL014", typeof(string));   // ATTVAL014
                inTable.Columns.Add("ATTVAL015", typeof(string));   // ATTVAL015 
                inTable.Columns.Add("ATTVAL016", typeof(string));   // ATTVAL016
                inTable.Columns.Add("ATTVAL017", typeof(string));   // ATTVAL017
                inTable.Columns.Add("ATTVAL018", typeof(string));   // ATTVAL018 

                DataRow Indata = inTable.NewRow();
                Indata["LBCD"] = sblCode;                                                              // 라벨코드  
                Indata["PRMK"] = sPrintType;                                                           // 프린터기종
                Indata["RESO"] = sResolution;                                                          // 해상도
                Indata["PRCN"] = sIssueCount;                                                          // 출력매수
                Indata["MARH"] = sXposition;                                                           // 시작위치H
                Indata["MARV"] = sYposition;                                                           // 시작위치V
                Indata["DARK"] = sDarkness;                                                            //  Darkness
                                                                                                       
                Indata["ATTVAL001"] = dtContentItems.Rows[0]["CTOG"].ToString();                       // ITEM001 Country Of Origin
                Indata["ATTVAL002"] = dtContentItems.Rows[0]["GOOD_LANE_QTY_UNIT_INCLUDE"].ToString(); // ITEM002 QUANTITY, 단위
                Indata["ATTVAL003"] = dtContentItems.Rows[0]["AGGR_WEIGHT_UNIT_INCLUDE"].ToString();   // ITEM003 GROSS WEIGHT, 단위
                Indata["ATTVAL004"] = dtContentItems.Rows[0]["PART_NAME_1"].ToString();                // ITEM004 PART NAME #1
                Indata["ATTVAL005"] = dtContentItems.Rows[0]["PART_NAME_2"].ToString();                // ITEM005 PART NAME #2
                Indata["ATTVAL006"] = dtContentItems.Rows[0]["CONTENT_LABEL_ID"].ToString();           // ITEM006 Contents Label ID
                Indata["ATTVAL007"] = dtContentItems.Rows[0]["PNTR"].ToString();                       // ITEM007 Supplier
                Indata["ATTVAL008"] = dtContentItems.Rows[0]["PNTR_NAME_1"].ToString();                // ITEM008 Supplier Name #1
                Indata["ATTVAL009"] = dtContentItems.Rows[0]["PNTR_NAME_2"].ToString();                // ITEM009 Supplier Name #2
                Indata["ATTVAL010"] = dtContentItems.Rows[0]["PROD_DATE2"].ToString();                 // ITEM010 Print Date
                Indata["ATTVAL011"] = dtContentItems.Rows[0]["LOT_CODE"].ToString();                   // ITEM011 Lot Code
                Indata["ATTVAL012"] = dtContentItems.Rows[0]["PART_NO"].ToString();                    // ITEM012 Tesla Part Number 
                Indata["ATTVAL013"] = dtContentItems.Rows[0]["SML_QR"].ToString();                     // ITEM013 QR Code1
                Indata["ATTVAL014"] = dtContentItems.Rows[0]["MFG_PART_NO"].ToString();                // ITEM014 MFG PART NUMBER : 
                Indata["ATTVAL015"] = dtContentItems.Rows[0]["VLD_DATE2"].ToString();                  // ITEM015 EXP DATE
                Indata["ATTVAL016"] = "";                                                              // ATTVAL016
                Indata["ATTVAL017"] = "";                                                              // ATTVAL017
                Indata["ATTVAL018"] = "";                                                              // ATTVAL018 
                inTable.Rows.Add(Indata);                                                             

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM15", "INDATA", "RSLTDT", inTable);
                //인쇄 요청

                if (result != null && result.Rows.Count > 0)
                {
                    string zplCode = Util.NVC(result.Rows[0]["LABELCD"]); //바코드라벨 ZPL

                    if (zplCode.StartsWith("0,"))
                    {
                        zplCode = zplCode.Substring(2);

                        zplCode = zplCode.Replace("QRCODE_BIG", sContentLabelId);  // QRCODE Big

                        for (int iPrint = 0; iPrint < iContentLabel_OutCnt; iPrint++)
                        {
                            System.Threading.Thread.Sleep(500);
                            Util.PrintLabel(FrameOperation, loadingIndicator, zplCode);                            
                        }

                        //라벨 발행 카운터  sLotid  
                        UpdateContentLabelPrintCount(sLotid); 

                        GetConfirmLotList(txtLotId.Text.Trim()); // 목록 재조회

                        return;
                    }
                    else
                    {
                        Util.MessageInfo(zplCode.Substring(2));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageValidation("SFU1309");  //바코드 프린터 실패
                return;
            }

            return;
        }

        private string GetTeslaLabelLotCodeContentLabe(string sLotid)
        {
            string sTeslaLabelLotCode = "";

            _LotCodeContentLabeCheckResult = "";    // Tesla Lable Lot_Code Content Labe CheckResult
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = procID;
                Indata["LOTID"] = sLotid;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TESLA_LABEL_LOT_CODE_CONTENT_LABEL_ELEC", "INDATA", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _LotCodeContentLabeCheckResult = Util.NVC(dtResult.Rows[0]["CHECK_RESULT"].ToString());
                    if (_LotCodeContentLabeCheckResult.Equals("SUCCESS"))  // 채번이 정상인 경우
                    {
                        sTeslaLabelLotCode = Util.NVC(dtResult.Rows[0]["LOT_CODE"].ToString());
                    }
                    else
                    {
                        sTeslaLabelLotCode = "";
                    }
                }
                else
                {
                    sTeslaLabelLotCode = "";
                    _LotCodeContentLabeCheckResult = "ERR_RESULT";
                }
            }
            catch (Exception ex)
            {
                _LotCodeContentLabeCheckResult = "ERR_RESULT";
                sTeslaLabelLotCode = "";
                return sTeslaLabelLotCode;
            }

            return sTeslaLabelLotCode;
        }


        private string GetsLotGrossWeight(string sLotid)
        {

            string sLotGrossWeight = string.Empty;

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = procID;
                Indata["LOTID"] = sLotid;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_GROSS_WEIGHT_ELEC", "INDATA", "RSLTDT", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sLotGrossWeight = Util.NVC(dtResult.Rows[0]["GROSS_WEIGHT"].ToString());
                }
                else
                {
                    sLotGrossWeight = "";
                }
            }
            catch (Exception ex)
            {
                sLotGrossWeight = "";
                return sLotGrossWeight;
            }

            return sLotGrossWeight;
        }

        private DataTable GetContentLabelBarcodeItems( string sLotId, string sLotCode, string sTeslaPartNumber, string sGoodLaneQty, string sLotGrossWeight,  string sWipdttmEd)
        {
            DataTable dtBarcodeItems = new DataTable();
            try
            {
                DataTable IndataTable = new DataTable(); 
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("LOT_CODE", typeof(string));
                IndataTable.Columns.Add("PART_NO", typeof(string)); 
                IndataTable.Columns.Add("GOOD_LANE_QTY", typeof(string));
                IndataTable.Columns.Add("AGGR_WEIGHT", typeof(string));
                IndataTable.Columns.Add("PROD_DATE", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow(); 
                Indata["LOTID"] = sLotId;
                Indata["LOT_CODE"] = sLotCode;
                Indata["PART_NO"] = sTeslaPartNumber;
                Indata["GOOD_LANE_QTY"] = sGoodLaneQty;
                Indata["AGGR_WEIGHT"] = sLotGrossWeight;
                Indata["PROD_DATE"] = sWipdttmEd;
                Indata["USERID"] = LoginInfo.USERID;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_IM_CONTENT_LABEL_ELEC", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    dtBarcodeItems = result;
                    return dtBarcodeItems;
                }
            }
            catch (Exception ex)
            {
                return dtBarcodeItems;
            }

            return dtBarcodeItems;
        }


        /// <summary> QR코드를 ZPL코드로 변환 </summary>
        /// <param name="QR">QR코드를 만들고자 하는 문자열</param>
        /// /// <param name="size">만들고자하는 크기(픽셀단위)</param>
        private String createQRCODE(String qr, int size)
        {
            int widthByte = size / 8;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrcodedata = qrGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.Q);
            QRCode code = new QRCode(qrcodedata);

            Bitmap nbit = code.GetGraphic(20, Color.Black, Color.White, false);
            Bitmap resized = new Bitmap(nbit, size, size);
            String cuerpo = createBody(resized);
            String QRCODE = encodeHexAscii(cuerpo, widthByte);

            return QRCODE;
        }

        /// <summary>
        /// QR코드를 각 픽셀마다 RGB값을 더해 16진수 값으로 변환
        /// </summary>
        /// <param name="bitmap">QR코드를 bitmap으로 변형한 값.</param>
        private String createBody(Bitmap bitmap)
        {
            StringBuilder sb = new StringBuilder();
            int width = bitmap.Width;
            int height = bitmap.Height;
            int index = 0;
            char[] auxBinaryChar = { '0', '0', '0', '0', '0', '0', '0', '0' };
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    Color c = bitmap.GetPixel(w, h);
                    char auxChar = '1';
                    int totalColor = c.R + c.G + c.B;
                    if (totalColor > 384)
                    {
                        auxChar = '0';
                    }
                    auxBinaryChar[index] = auxChar;
                    index++;
                    if (index == 8)
                    {
                        sb.Append((fourByteBinary(new String(auxBinaryChar))));
                        auxBinaryChar = new char[] { '0', '0', '0', '0', '0', '0', '0', '0' };
                        index = 0;
                    }
                }
                /* (오른쪽 90도 회전)
                for (int h = height - 1; h > -1; h--)
                {
                    Color c = bitmap.GetPixel(w, h);
                    char auxChar = '1';
                    int totalColor = c.R + c.G + c.B;
                    if (totalColor > 384)
                    {
                        auxChar = '0';
                    }
                    auxBinaryChar[index] = auxChar;
                    index++;
                    if (index == 8)
                    {
                        sb.Append((fourByteBinary(new String(auxBinaryChar))));
                        auxBinaryChar = new char[] { '0', '0', '0', '0', '0', '0', '0', '0' };
                        index = 0;
                    }
                }
                */
                sb.Append("\n");

            }
            return sb.ToString();
        }

        /// <summary>
        /// 8비트 String을 16진수로 바꾸는 함수
        /// </summary>
        /// <param name="binaryStr"></param>
        /// <returns></returns>
        private String fourByteBinary(String binaryStr)
        {
            int int2 = System.Convert.ToInt32(binaryStr, 2);
            String int16 = System.Convert.ToString(int2, 16);
            if (int16.Length == 1)
            {
                int16 = '0' + int16;
            }
            return int16.ToUpper();
        }

        /// <summary>
        /// 16진수를 아스키코드로 변환하는 함수
        /// </summary>
        /// <param name="code"></param>
        /// <param name="widthBytes"></param>
        /// <returns></returns>
        private String encodeHexAscii(String code, int widthBytes)
        {
            Hashtable mapCode = new Hashtable();
            mapCode.Add(1, "G");
            mapCode.Add(2, "H");
            mapCode.Add(3, "I");
            mapCode.Add(4, "J");
            mapCode.Add(5, "K");
            mapCode.Add(6, "L");
            mapCode.Add(7, "M");
            mapCode.Add(8, "N");
            mapCode.Add(9, "O");
            mapCode.Add(10, "P");
            mapCode.Add(11, "Q");
            mapCode.Add(12, "R");
            mapCode.Add(13, "S");
            mapCode.Add(14, "T");
            mapCode.Add(15, "U");
            mapCode.Add(16, "V");
            mapCode.Add(17, "W");
            mapCode.Add(18, "X");
            mapCode.Add(19, "Y");
            mapCode.Add(20, "g");
            mapCode.Add(40, "h");
            mapCode.Add(60, "i");
            mapCode.Add(80, "j");
            mapCode.Add(100, "k");
            mapCode.Add(120, "l");
            mapCode.Add(140, "m");
            mapCode.Add(160, "n");
            mapCode.Add(180, "o");
            mapCode.Add(200, "p");
            mapCode.Add(220, "q");
            mapCode.Add(240, "r");
            mapCode.Add(260, "s");
            mapCode.Add(280, "t");
            mapCode.Add(300, "u");
            mapCode.Add(320, "v");
            mapCode.Add(340, "w");
            mapCode.Add(360, "x");
            mapCode.Add(380, "y");
            mapCode.Add(400, "z");

            int maxlinea = widthBytes * 2;
            StringBuilder sbCode = new StringBuilder();
            StringBuilder sbLinea = new StringBuilder();
            String previousLine = null;
            int counter = 1;
            char aux = code[0];
            Boolean firstChar = false;
            for (int i = 1; i < code.Length; i++)
            {
                if (firstChar)
                {
                    aux = code[i];
                    firstChar = false;
                    continue;
                }
                if (code[i] == '\n')
                {
                    if (counter >= maxlinea && aux == '0')
                    {
                        sbLinea.Append(",");
                    }
                    else if (counter >= maxlinea && aux == 'F')
                    {
                        sbLinea.Append("!");
                    }
                    else if (counter > 20)
                    {
                        int multi20 = (counter / 20) * 20;
                        int resto20 = (counter % 20);
                        sbLinea.Append(mapCode[multi20]);
                        if (resto20 != 0)
                        {
                            sbLinea.Append(mapCode[resto20] + aux.ToString());
                        }
                        else
                        {
                            sbLinea.Append(aux);
                        }
                    }
                    else
                    {
                        sbLinea.Append(mapCode[counter] + aux.ToString());
                        if (mapCode[counter] == null)
                        {
                        }
                    }
                    counter = 1;
                    firstChar = true;
                    if (sbLinea.ToString().Equals(previousLine))
                    {
                        sbCode.Append(":");
                    }
                    else
                    {
                        sbCode.Append(sbLinea.ToString());
                    }
                    previousLine = sbLinea.ToString();
                    sbLinea.Length = 0;
                    continue;
                }
                if (aux == code[i])
                {
                    counter++;
                }
                else
                {
                    if (counter > 20)
                    {
                        int multi20 = (counter / 20) * 20;
                        int resto20 = (counter % 20);
                        sbLinea.Append(mapCode[multi20]);
                        if (resto20 != 0)
                        {
                            sbLinea.Append(mapCode[resto20] + aux.ToString());
                        }
                        else
                        {
                            sbLinea.Append(aux);
                        }
                    }
                    else
                    {
                        sbLinea.Append(mapCode[counter] + aux.ToString());
                    }
                    counter = 1;
                    aux = code[i];
                }
            }
            return sbCode.ToString();
        }

        private void UpdateContentLabelPrintCount(string sLotid ) // 재공 2D 바코드 인쇄 횟수 업데이트
        {
            try
            {
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string)); 
                inDataTable.Columns.Add("USERID", typeof(string));


                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = sLotid; 
                dr["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_TB_SFC_ELTR_LABEL_PRT_COUNT_CONTENT_PRT_COUNT", "INDATA", null, inDataTable);

                return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }
        #endregion [CONTENT_LABE Print End]

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
            sCmCode = procID;                              // 공정 

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

        #endregion

        /// <summary>
        /// BarCode 재발시 권한 부여 [C20220610-000415] - Barcode printing interlock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            CMM_ELEC_AREA_CODE_AUTH window = sender as CMM_ELEC_AREA_CODE_AUTH;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                DataRowView dataRow = dgLotList.DataContext as DataRowView;

                int iSamplingCount = 0; // GetSamplingLabelQty();

                #region [샘플링 출하거래처 추가]
                string[] sCompany = null;
                foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(vLOTID))
                {
                    iSamplingCount = Util.NVC_Int(items.Key);
                    sCompany = Util.NVC(items.Value).Split(',');
                }
                #endregion

                //C20210415-000402 [2021-06-22]
                int iFirstCutCNT = 0;

                if (procID.Equals(Process.COATING) && (CUT_ID == "1"))
                    iFirstCutCNT = LoginInfo.CFG_LABEL_FIRST_CUT_COPIES;

                for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES + iFirstCutCNT; ii++)
                    for (int i = 0; i < iSamplingCount; i++)
                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, sPrtLotid, procID, i > sCompany.Length - 1 ? "" : sCompany[i], userName); // 재와인더 라벨 발행 사용자 추가(userID)

                Util.UpdatePrintExecCount(sPrtLotid, procID);

                btnSearch_Click(null, null);
            }
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }
    }
}
