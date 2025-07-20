/*************************************************************************************
 Created Date : 2017.05.29
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - SSC 공정진척 화면 - 대기LOT 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.29  INS 김동일K : Initial Created.
  2023.10.10  윤지해  E20230828-000346 btnHoldRelease 권한별로 표시
  2023.12.18  박승렬 : E20231103-001744 동별 공통코드 조회 INDATA 에 USE_FLAG 추가 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_010_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_010_WAITLOT : C1Window, IWorkArea
    {        
        #region Declaration & Constructor

        private string _ProcID = string.Empty;
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();
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
        public ASSY001_010_WAITLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event       

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _ProcID = Util.NVC(tmps[0]);
                _LineID = Util.NVC(tmps[1]);
            }
            else
            {
                _ProcID = "";
                _LineID = "";
            }

            #region 2023.10.10 윤지해 E20230828-000346 권한별 버튼표시 적용
            // 주석처리
            //ApplyPermissions();

            // 2023.10.10 윤지해 E20230828-000346 권한별 버튼표시 적용
            GetButtonPermissionGroup();
            #endregion

            //HOLD 사유
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            GetWaitMagazine();
        }
        #endregion

        #region [HOLD - 저장]
        private void btnExecute_Click(object Sender, RoutedEventArgs e)
        {
            if (!CanExecute())
                return;

            HoldProcess();
        }
        #endregion

        #region [종료]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        
        #region 예상해제일
        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                //Util.AlertInfo("오늘이후날짜만지정가능합니다.");
                Util.MessageValidation("SFU1740");
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitMagazine()
        {
            try
            {
                Util.gridClear(dgWaitLot);

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _ProcID;
                newRow["EQSGID"] = _LineID;                
                newRow["LOTID"] = "";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_SSC", "INDATA", "OUTDATA", inTable, (bizResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgWaitLot, bizResult, null, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
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

        private void HoldProcess()
        {
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("HOLD 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("LANGID", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("HOLD_NOTE", typeof(string));
                        inLot.Columns.Add("RESNCODE", typeof(string));
                        inLot.Columns.Add("HOLD_CODE", typeof(string));
                        inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));


                        DataRow inRow = inDataTable.NewRow();
                        inRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inRow["LANGID"] = LoginInfo.LANGID;
                        inRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inRow["USERID"] = LoginInfo.USERID;

                        inDataTable.Rows.Add(inRow);
                        inRow = null;

                        for (int i = 0; i < dgWaitLot.Rows.Count; i++)
                        {
                            if (!_util.GetDataGridCheckValue(dgWaitLot, "CHK", i)) continue;

                            inRow = inLot.NewRow();
                            inRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "LOTID"));
                            inRow["HOLD_NOTE"] = txtRemark.Text;
                            inRow["RESNCODE"] = cboHoldReason.SelectedValue.ToString();
                            inRow["HOLD_CODE"] = cboHoldReason.SelectedValue.ToString();
                            inRow["UNHOLD_SCHD_DATE"] = dtExpected.SelectedDateTime.ToString("yyyyMMdd");

                            inLot.Rows.Add(inRow);
                        }

                        if (inLot.Rows.Count < 1)
                            return;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, (bizResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                //Util.AlertInfo("정상 처리 되었습니다.");
                                Util.MessageInfo("SFU1275");
                                GetWaitMagazine();

                                txtRemark.Text = "";
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, inDataSet);
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
            });
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
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Validation]
        private bool CanExecute()
        {
            bool bRet = false;

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgWaitLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (cboHoldReason == null || cboHoldReason.SelectedValue == null || cboHoldReason.SelectedValue.ToString().Equals("SELECT"))
            {
                //Util.Alert("HOLD 사유를 선택 하세요.");
                Util.MessageValidation("SFU1342");
                return bRet;
            }

            for (int i = 0; i < dgWaitLot.Rows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgWaitLot, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "WIPHOLD")).Equals("Y"))
                {
                    //Util.Alert("이미 HOLD상태의 LOT이 선택 되었습니다.");
                    Util.MessageValidation("SFU1773");
                    return bRet;
                }
            }

            if (txtRemark.Text.Trim().Equals(""))
            {
                //Util.Alert("LOT HOLD 에 대한 설명을 입력해 주세요.");
                Util.MessageValidation("SFU1214");
                return bRet;
            }

            bRet = true;

            return bRet;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnHoldRelease);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        // 2023.10.10 윤지해 E20230828-000346 권한별 버튼표시 적용
        private bool ChkButtonPermissionSetYN()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ATTR2", typeof(string));
                inTable.Columns.Add("ATTR3", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["COM_TYPE_CODE"] = "PERMISSIONS_PER_BUTTON";
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["ATTR2"] = this.GetType().Name;
                dtRow["ATTR3"] = "HOLD_W";
                dtRow["USE_FLAG"] = "Y";

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // 2023.10.10 윤지해 E20230828-000346 권한별 버튼표시 적용
        private void GetButtonPermissionGroup()
        {
            try
            {
                // Read 권한일 때 btnHoldRelease 숨김
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                {
                    btnHoldRelease.Visibility = Visibility.Collapsed;
                    return;
                }
                if (ChkButtonPermissionSetYN())
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));
                    inTable.Columns.Add("FORMID", typeof(string));

                    DataRow dtRow = inTable.NewRow();
                    dtRow["USERID"] = LoginInfo.USERID;
                    dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dtRow["FORMID"] = this.GetType().Name;

                    inTable.Rows.Add(dtRow);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP", "INDATA", "OUTDATA", inTable);

                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                    {
                        if (dtRslt.Columns.Contains("BTN_PMS_GRP_CODE"))
                        {
                            foreach (DataRow drTmp in dtRslt.Rows)
                            {
                                if (drTmp == null) continue;

                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "HOLD_W": // Hold 버튼 사용 권한
                                        btnHoldRelease.Visibility = Visibility.Visible;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        btnHoldRelease.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    btnHoldRelease.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        private void btnWaitPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                string sLotID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

                if (!sLotID.Equals(""))
                {
                    List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                    DataTable dtRslt = GetThermalPaperPrintingInfo(sLotID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    if (_ProcID.Equals(Process.SSC_BICELL))
                    {
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

                            print.Closed += new EventHandler(printWait_Closed);

                            print.ShowModal();
                        }
                    }
                    else
                    {
                        int iCopys = 2;

                        if (LoginInfo.CFG_THERMAL_COPIES > 0)
                        {
                            iCopys = LoginInfo.CFG_THERMAL_COPIES;
                        }

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

                        dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                        dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                        dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.

                        dicList.Add(dicParam);


                        LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                        print.FrameOperation = FrameOperation;

                        if (print != null)
                        {
                            object[] Parameters = new object[6];
                            Parameters[0] = null;
                            Parameters[1] = _ProcID;
                            Parameters[2] = _LineID;
                            Parameters[3] = _EqptID;
                            Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                            Parameters[5] = "N";   // 디스패치 처리.

                            C1WindowExtension.SetParameters(print, Parameters);

                            print.Closed += new EventHandler(printWait_Closed);

                            print.ShowModal();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void printWait_Closed(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI))
            {
                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI;

                if (window.DialogResult == MessageBoxResult.OK)
                {

                }
            }
            else if (sender.GetType() == typeof(LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD))
            {
                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD;

                if (window.DialogResult == MessageBoxResult.OK)
                {

                }
            }
        }
    }
}
