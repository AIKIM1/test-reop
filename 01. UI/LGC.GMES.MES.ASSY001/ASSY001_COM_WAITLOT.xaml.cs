/*************************************************************************************
 Created Date : 2019.04.11
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 조립 공정 공통 - 대기LOT조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.11  INS 김동일K : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_COM_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_COM_WAITLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _ProcDetail = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY001_COM_WAITLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _ProcID = Util.NVC(tmps[2]);
                _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[3]);
                //stk와 fol만 tmps[4]가 존재
                if(tmps.Length == 5)
                    _ProcDetail = Util.NVC(tmps[4]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _ProcID = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";
                _ProcDetail = "";
            }

            ApplyPermissions();

            if (!_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
            {
                dgWaitLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }

            if (_ProcID.Equals(Process.NOTCHING) || _ProcID.Equals(Process.LAMINATION))
            {
                dgWaitLot.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
            }


            if (_ProcID.Equals(Process.NOTCHING) || _ProcID.Equals(Process.VD_LMN))
            {
                dgWaitLot.Columns["LOTID_RT"].Visibility = Visibility.Collapsed;

                dgWaitLot.Columns["PRINT"].Visibility = Visibility.Visible;
            }
            else
            {
                if (LoginInfo.CFG_AREA_ID.Equals("A9"))
                    dgWaitLot.Columns["PRINT"].Visibility = Visibility.Visible;
                else
                    dgWaitLot.Columns["PRINT"].Visibility = Visibility.Collapsed;
            }

            if (_ProcID.Equals(Process.PACKAGING))
            {
                stkProdType.Visibility = Visibility.Collapsed;
                GetWaitLot();
            }
            else
            {
                //ComboBox 완성 후 대기Lot 조회
                GetCellCodeByProcID();
            }

            grdDefect.Visibility = Visibility.Collapsed;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitLot();
                txtWaitPancakeLot.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboCommonCode_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (e.OldValue != null)
                {
                    GetWaitLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                Button bt = sender as Button;

                String sID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

                if (!sID.Equals(""))
                {
                    if (_ProcID.Equals(Process.NOTCHING) || _ProcID.Equals(Process.VD_LMN))
                    {
                        #region ZPL Print
                        // 프린터 정보 조회
                        string sPrt = string.Empty;
                        string sRes = string.Empty;
                        string sCopy = string.Empty;
                        string sXpos = string.Empty;
                        string sYpos = string.Empty;
                        string sDark = string.Empty;
                        string sLBCD = string.Empty;    // 리턴 라벨 타입 코드
                        string sEqpt = string.Empty;
                        DataRow drPrtInfo = null;

                        // 2017-07-04 Lee. D. R
                        // Line별 라벨 독립 발행 기능
                        if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                            return;
                        }
                        else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                        {
                            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                                return;
                        }
                        else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                        {
                            foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                            {
                                if (Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]).Equals(_EqptID))
                                {
                                    sPrt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                    sRes = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                    sCopy = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                    sXpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                    sYpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                    sDark = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                    sEqpt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT].ToString();
                                    drPrtInfo = dr;
                                }
                            }

                            if (sEqpt.Equals(""))
                            {
                                Util.MessageValidation("SFU3615");  //프린터 환경설정에 설비 정보를 확인하세요.
                                return;
                            }
                        }

                        string sZPL = GetPrintInfo(sID, Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ")), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);

                        if (sZPL.Equals(""))
                        {
                            Util.MessageValidation("SFU1498");
                            return;
                        }

                        if (sZPL.StartsWith("0,"))  // ZPL 정상 코드 확인.
                        {
                            if (PrintLabel(sZPL.Substring(2), drPrtInfo))
                                SetLabelPrtHist(sZPL.Substring(2), drPrtInfo, sID, Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ")), sLBCD);
                        }
                        else
                        {
                            Util.Alert(sZPL.Substring(2));
                        }
                        #endregion
                    }
                    else if (_ProcID.Equals(Process.LAMINATION))
                    {
                        #region Lami Wait Thermal Print
                        List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                        DataTable dtRslt = GetThermalPaperPrintingInfo(sID);

                        if (dtRslt == null || dtRslt.Rows.Count < 1)
                            return;

                        Dictionary<string, string> dicParam = new Dictionary<string, string>();


                        dicParam.Add("PANCAKEID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                        dicParam.Add("TOT_QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                        dicParam.Add("REMAIN_QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                        dicParam.Add("NOTE", "");
                        dicParam.Add("PRINTQTY", "1");  // 발행 수

                        dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                        dicList.Add(dicParam);

                        LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN(dicParam);
                        print.FrameOperation = FrameOperation;

                        if (print != null)
                        {
                            object[] Parameters = new object[6];
                            Parameters[0] = null;
                            Parameters[1] = Process.LAMINATION;
                            Parameters[2] = _LineID;
                            Parameters[3] = _EqptID;
                            Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                            Parameters[5] = "N";   // 디스패치 처리.

                            C1WindowExtension.SetParameters(print, Parameters);

                            print.Closed += new EventHandler(printWaitPancake_Closed);

                            print.ShowModal();
                        }
                        #endregion
                    }
                    else if (_ProcID.Equals(Process.STACKING_FOLDING))
                    {
                        #region Stk & Fol Wait Thermal Print
                        if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        {
                            try
                            {
                                using (ThermalPrint thmrPrt = new ThermalPrint())
                                {
                                    thmrPrt.Print(sEqsgID: _LineID,
                                                  sEqptID: _EqptID,
                                                  sProcID: _ProcID,
                                                  inData: GetGroupPrintInfo(sID),
                                                  iType: THERMAL_PRT_TYPE.COM_OUT_RFID_GRP,
                                                  iPrtCnt: 1,
                                                  bSavePrtHist: true,
                                                  bDispatch: false);
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                        else
                        {
                            List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                            DataTable dtRslt = GetThermalPaperPrintingInfo(sID);

                            if (dtRslt == null || dtRslt.Rows.Count < 1)
                                return;

                            Dictionary<string, string> dicParam = new Dictionary<string, string>();

                            //라미
                            dicParam.Add("reportName", "Lami"); //dicParam.Add("reportName", "Fold");
                            dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                            dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                            dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                            dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                            dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                            dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                            dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                            dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                            dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                            dicParam.Add("TITLEX", "MAGAZINE ID");

                            dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                            dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                            dicParam.Add("RE_PRT_YN", "F"); // 재발행 여부.

                            dicList.Add(dicParam);

                            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI(dicParam);
                            print.FrameOperation = FrameOperation;

                            if (print != null)
                            {
                                object[] Parameters = new object[7];
                                Parameters[0] = null;
                                Parameters[1] = _ProcID;
                                Parameters[2] = _LineID;
                                Parameters[3] = _EqptID;
                                Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                                Parameters[5] = "N";   // 디스패치 처리.
                                Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                                C1WindowExtension.SetParameters(print, Parameters);

                                print.Closed += new EventHandler(printWaitMaz_Closed);

                                print.ShowModal();
                            }
                        }
                        #endregion
                    }
                    else if (_ProcID.Equals(Process.PACKAGING))
                    {
                        #region PKG Wait Thermal Print
                        if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        {
                            try
                            {
                                DataTable dtTmp = null;

                                DataTable dtRslt = GetThermalPaperPrintingInfo(sID);

                                if (dtRslt == null || dtRslt.Rows.Count < 1) return;

                                if (!dtRslt.Columns.Contains("DISPATCH_YN"))
                                {
                                    DataColumn dcTmp = new DataColumn("DISPATCH_YN", typeof(string));
                                    dcTmp.DefaultValue = "N";
                                    dtRslt.Columns.Add(dcTmp);
                                }

                                if (!dtRslt.Columns.Contains("RE_PRT_YN"))
                                {
                                    DataColumn dcTmp = new DataColumn("RE_PRT_YN", typeof(string));
                                    dcTmp.DefaultValue = "Y";
                                    dtRslt.Columns.Add(dcTmp);
                                }

                                if (dtTmp == null)
                                    dtTmp = dtRslt.Copy();
                                else
                                    dtTmp.Merge(dtRslt);

                                if (dtTmp == null) return;

                                using (ThermalPrint thmrPrt = new ThermalPrint())
                                {
                                    THERMAL_PRT_TYPE type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD;
                                    if (LoginInfo.CFG_SHOP_ID.Equals("G382"))
                                    {
                                        type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_CARRIER_BCD;
                                    }

                                    thmrPrt.Print(sEqsgID: _LineID,
                                                  sEqptID: _EqptID,
                                                  sProcID: _ProcID,
                                                  inData: dtTmp,
                                                  iType: type,
                                                  iPrtCnt: LoginInfo.CFG_THERMAL_COPIES < 1 ? 1 : LoginInfo.CFG_THERMAL_COPIES,
                                                  bSavePrtHist: true,
                                                  bDispatch: false);
    
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                        else
                        {
                            // 발행..
                            DataTable dtRslt = GetThermalPaperPrintingInfo(sID);


                            if (dtRslt == null || dtRslt.Rows.Count < 1)
                                return;

                            Dictionary<string, string> dicParam = new Dictionary<string, string>();

                            //폴딩
                            dicParam.Add("reportName", "Fold");
                            dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                            dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                            dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                            dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                            dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                            dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                            dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                            dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                            dicParam.Add("TITLEX", "BASKET ID");

                            dicParam.Add("PRINTQTY", "1");  // 발행 수

                            dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                            dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                            dicParam.Add("RE_PRT_YN", "F"); // 재발행 여부.
                            
                            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD printWaitBox = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                            printWaitBox.FrameOperation = FrameOperation;

                            if (printWaitBox != null)
                            {
                                object[] Parameters = new object[6];
                                Parameters[0] = null;
                                Parameters[1] = _ProcID;
                                Parameters[2] = _LineID;
                                Parameters[3] = _EqptID;
                                Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                                Parameters[5] = "N";   // 디스패치 처리.

                                C1WindowExtension.SetParameters(printWaitBox, Parameters);

                                printWaitBox.Closed += new EventHandler(printWaitBox_Closed);

                                printWaitBox.ShowModal();
                            }
                        }                            
                        #endregion
                    }
                }
                else
                {
                    Util.MessageValidation("SFU1361");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        //2019.04.29 김대근 수정
        private void GetWaitLot()
        {
            try
            {
                ShowLoadingIndicator();

                object selectedType = cboCommonCode.SelectedItem == null ? "" : (cboCommonCode.SelectedItem as C1ComboBoxItem).Tag;

                //Column 생성
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("ELECTYPE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                //Lev2,3 : VW_PRODUCT.PRODUCT_LEVELX_CODE와 ELECTYPE을 비교하기 때문에 넣어줬다.
                inTable.Columns.Add("LEV2", typeof(string));
                inTable.Columns.Add("LEV3", typeof(string));

                //Row추가
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _ProcID;
                newRow["EQSGID"] = _LineID;
                newRow["LOTID"] = txtWaitPancakeLot == null ? "" : txtWaitPancakeLot.Text;
                ////RF_ID
                //if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
                //{
                    newRow["CSTID"] = txtWaitPancakeLot == null ? "" : txtWaitPancakeLot.Text;
                //}
                //폴딩
                if (_ProcID.Equals(Process.STACKING_FOLDING) && (_ProcDetail.Equals(EquipmentGroup.FOLDING)))
                {
                    newRow["LEV3"] = selectedType;
                }
                //스태킹
                else if (_ProcID.Equals(Process.STACKING_FOLDING) && (_ProcDetail.Equals(EquipmentGroup.STACKING)))
                {
                    newRow["LEV2"] = selectedType;
                }
                //패키징
                else if (_ProcID.Equals(Process.PACKAGING))
                {
                    //패키징 공정일 경우 새로 추가해줄 Row가 없다.
                }
                else
                {
                    newRow["ELECTYPE"] = selectedType;
                }
                inTable.Rows.Add(newRow);

                //DA 호출
                //각 공정벌 DA를 하나로 통합함.
                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //DA 호출 결과를 datagrid에 삽입
                        Util.GridSetData(dgWaitLot, searchResult, null, true);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        //2019.0429 김대근 수정
        //CommonCode에서 CELL의 타입을 가져온다.
        private void GetCellCodeByProcID()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("CMCDTYPE");
                inTable.Columns.Add("PROCID");
                inTable.Columns.Add("EQSGID");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "ASSY_CELL_TYPE_CODE";
                newRow["PROCID"] = _ProcID;
                newRow["EQSGID"] = _LineID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_COMM_CBO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //DA 호출 결과를 ComboBox에 넣고
                        foreach (var i in searchResult.Rows)
                        {
                            ////A8000일 때
                            //if (_ProcID.Equals(Process.STACKING_FOLDING) && !(i as DataRow).ItemArray[2].Equals(_ProcDetail))
                            //    continue;

                            C1ComboBoxItem item = new C1ComboBoxItem();
                            item.Content = (i as DataRow).ItemArray[1];
                            item.Tag = (i as DataRow).ItemArray[0];
                            cboCommonCode.Items.Add(item);
                        }
                        cboCommonCode.SelectedIndex = 0;

                        //대기Lot 조회
                        GetWaitLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_PROCESS_LOT_LABEL_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness


                // 프린트 라벨 옵션 선택으로 인한, 
                if (!Util.NVC(LoginInfo.CFG_LABEL_TYPE).Equals(""))
                    newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE
                else
                    newRow["LBCD"] = "LBL0001"; // LABEL CODE
                newRow["NT_WAIT_YN"] = "Y"; // 대기 팬케익 재발행 여부.

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_NT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("MMDLBCD"))
                        sOutLBCD = Util.NVC(dtResult.Rows[0]["MMDLBCD"]);

                    return Util.NVC(dtResult.Rows[0]["LABELCD"]);
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq, string sLBCD)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["PRT_ITEM03"] = "WAIT PANCAKE";
                //newRow["PRT_ITEM04"] = "";
                //newRow["PRT_ITEM05"] = "";
                //newRow["PRT_ITEM06"] = "";
                //newRow["PRT_ITEM07"] = "";
                //newRow["PRT_ITEM08"] = "";
                //newRow["PRT_ITEM09"] = "";
                //newRow["PRT_ITEM10"] = "";
                //newRow["PRT_ITEM11"] = "";
                //newRow["PRT_ITEM12"] = "";
                //newRow["PRT_ITEM13"] = "";
                //newRow["PRT_ITEM14"] = "";
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_REWRK", "INDATA", "OUTDATA", inTable);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private DataTable GetGroupPrintInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                
                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_GRP_PRT_INFO_FD", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (Util.NVC(drPrtInfo["PORTNAME"]).ToUpper().Equals("USB"))
                {
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        brtndefault = FrameOperation.PrintUsbBarcodeEquipment(sZPL, _EqptID);
                    }

                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void printWaitMaz_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void printWaitPancake_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void printWaitBox_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #endregion

        private void dgWaitLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if (!_ProcID.Equals(Process.NOTCHING)) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (Util.NVC(cell.Column.Name) == "LOTID")
                    {
                        GetDefectInfo(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name)), Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "WIPSEQ")));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDefectInfo(string sLotID, string sWipseq)
        {
            try
            {
                if (sLotID.Equals(""))
                    return;

                grdDefect.Visibility = Visibility.Visible;
                loadingIndicator.Visibility = Visibility.Visible;
                loadingIndicator2.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));                
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipseq;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MOVE_WIPRESONCOLLECT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgDefect, searchResult, null, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator2.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                loadingIndicator2.Visibility = Visibility.Collapsed;
            }
        }

        private void btnClose2_Click(object sender, RoutedEventArgs e)
        {
            grdDefect.Visibility = Visibility.Collapsed;
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void dgWaitLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;                        
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name).Equals("LOTID") && _ProcID.Equals(Process.NOTCHING))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }
            }));
        }

        private void dgWaitLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }
    }
}
