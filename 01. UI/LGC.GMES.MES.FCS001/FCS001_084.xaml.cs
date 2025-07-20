/*************************************************************************************
 Created Date : 2020.12.09
      Creator : 
   Decription : 개발/기술 Sample Cell 추출
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.09  NAME : Initial Created
  2022.03.29  강동희 : 조회조건추가-생산라인,모델,LotType
  2022.06.10  이정미 : 등록Tab 추출여부 Option Button 변경 (Default 자동 -> 수동)
  2022.08.18  조영대 : Loaded 이벤트 제거
  2022.11.16  조영대 : 이전 Tray ID 컬럼 추가
  2022.12.14  조영대 : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2023.01.08  이정미 : 자동추출 기능 미사용시 자동추출 라디오 버튼 비활성화
  2023.04.05  박승렬 : 입력수량 100 -> 5,000 증가 / GetCellInfo() 불러오는 형식 변경 (입력한 cell 수 --> cell을 list화 시킨 후 한번만 호출 /
                       Sample 등록 시 100개씩 나눠서 등록하도록 변경
  2023.07.18  이정미 : Cell 등록 Validation 추가, 수동 추출 Cell 등록 오류 수정
  2023.07.19  최도훈 : 조회 조건 '양불 정보(DFCT_SLOC_SMPL_FLAG)' 추가
  2023.07.21  최도훈 : 조회Tab 일괄선택 기능 추가(불량창고에 있거나 복구된 Cell은 제외), 체크박스 오류 수정
  2023.08.10  김동훈 : BizWF 로직 추가
  2023.09.22  임근영 : [E20230921-001332] 장기보류 CELL (LOT_DETL_TYPE_CODE = 'H')은 양품 추출 불가능 하도록 등록 제한.(20231012 PI요청으로롤백)
  2024.03.28  임정훈 : [E20240325-000358] Lot 유형 양품/폐기대기 선택 시, 기본값='수동'으로 변경
  2024.08.26  이한학 : [E20240814-000810] ESNA_Login Reason Input for Development/Technology Sample Cell(등록 : 사유 입력 추가, 조회 : 사유 컬럼 추가)
  2025.03.17  최경아 : MES2.0 CatchUp ([E20250121-000650] Sample 등록 처리 후 재실행 시 중복등록 오류 수정. 재등록 시 등록 실패한 CELL만 대상으로 재등록 여부(메시지박스) 체크하여 재등록 진행.
                                       [E20240911-001094] Sample Cell 추출 실행 방식을 Backgroud Progress 방식으로 변경 / BizWF 로직 - 다중처리 추가)
  2025.04.11  하유승 : ESHD MES 2.0 PJT 개발/기술 샘플 셀 등록 시 셀 조회에서 DA_SEL_CELL_SAMPLE_YN 호출 할 때 LANGID 변수 넘기도록 수정
  2025.04.14  이현승 : Catch-Up [E20240911-001061] 등급변경 적용 UI 추적성 향상을 위한 MENUID 추가 (개발/기술 SPML Cell 복구)
**************************************************************************************/
using C1.WPF; //20220329_조회조건추가-생산라인,모델,LotType
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using LGC.GMES.MES.CMM001.Extensions;
using System.Configuration;


namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_084 : UserControl, IWorkArea
    {
        public FCS001_084()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util _Util = new Util();
        bool bSampleCellAutoOutFlag = false;

        private bool bSaveRunFlag = false; // 2025.01.20 Sample 등록 처리 진행 중 체크

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();

            dtpFromDate.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-2);
            dtpToDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            bSampleCellAutoOutFlag = _Util.IsAreaCommonCodeUse("FORM_PROC_LOGIC_CODE", "SAMPLE_CELL_AUTO_OUTPUT");

            if (!bSampleCellAutoOutFlag)
            {
                rdoAuto.Visibility = Visibility.Hidden;
            }

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilter = { "FORM_SMPL_TYPE_CODE", "Y", null, null, null, null };
            _combo.SetCombo(cboSampleType, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN_WITH_OPTION");
            _combo.SetCombo(cboSearchSampleType, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "CMN_WITH_OPTION");

            //20220329_조회조건추가-생산라인,모델,LotType START
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            // Lot 유형
            _combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
            //20220329_조회조건추가-생산라인,모델,LotType END

            // 양불 정보
            string[] sFilter1 = { "DFCT_SLOC_SMPL_FLAG" };
            _combo.SetCombo(cboDfctSlocSmplFlag, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter1, sCase: "CMN_WITH_OPTION");
        }

        #endregion

        #region Event

        #region 등록
        private void btnInsertInputCell_Click(object sender, RoutedEventArgs e)
        {
            string sCellID = txtInsertCellId.Text.Trim();

            #region BizWF 
            int RetVal = BizWF_Check(sCellID, "Z");

            if (RetVal != 0)
            {
                //ShowLoadingIndicator();
                //loadingIndicator.Visibility = Visibility.Hidden;
                return;
            }
            #endregion

            bool bRtn = GetCellInfo(sCellID);
            SetInit();
        }

        private void btnInsertClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgInsert);
            txtInsertCellCnt.Text = "0";
            txtInsertCellId.Text = string.Empty;
            bSaveRunFlag = false;
        }

        private void txtInsertCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtInsertCellId.Text.Trim() == string.Empty)
                    return;

                btnInsertInputCell_Click(null, null);
            }
        }

        private void btnInsertSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sSampleType = Util.GetCondition(cboSampleType);

                if (dgInsert.Rows.Count == 0)
                {
                    //등록할 대상이 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0125"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtInsertCellId.SelectAll();
                            txtInsertCellId.Focus();
                        }
                    });
                    return;
                }

                if (string.IsNullOrEmpty(sSampleType) || sSampleType.Equals("SELECT"))
                {
                    //Sample 유형을 선택해주세요.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0310"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtInsertCellId.SelectAll();
                            txtInsertCellId.Focus();
                        }
                    });
                    return;
                }

                if (!string.IsNullOrEmpty(txtReason.Text))
                {
                    int byteLength = GetByteLength(txtReason.Text);

                    if (byteLength > 20)
                    {
                        //Reason(사유) 입력은 20byte 이내로 가능합니다.(한글은 10글자)
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU10024"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtReason.SelectAll();
                                txtReason.Focus();
                            }
                        });
                        return;
                    }
                }

                if ((rdoAuto.IsChecked == false && rdoManual.IsChecked == false) || (rdoAuto.IsVisible == false && rdoAuto.IsChecked == true) || (rdoAuto.IsVisible == false && rdoManual.IsChecked == false))
                {
                    //추출여부를 선택해주세요.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0505"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return;
                }

                //2025.03.17 MES2.0 CatchUp (2025.01.20 김용준 - Sample추출 등록 시 먼저 처리된 CELL ID 목록 중 Fail된 CELL이 존재하면 재실행 팝업 표시)

                bool bResultFlag = false;
                int iFailCount = 0;

                for (int i = 0; i < dgInsert.Rows.Count; i++)
                {
                    string sResult = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "RESULT"));

                    if (string.IsNullOrWhiteSpace(sResult) == false)
                    {
                        if (sResult.Contains("FAIL"))
                        {
                            bResultFlag = true;
                            iFailCount++;
                        }

                        if (sResult.Contains("SUCCESS"))
                        {
                            bResultFlag = true;
                        }
                    }
                }

                if (bResultFlag)
                {
                    // 2025.01.23 - FAIL 건수가 0이면 Grid 초기화 후 메시지 표시. 
                    if (iFailCount == 0)
                    {
                        Util.gridClear(dgInsert);
                        txtInsertCellCnt.Text = "0";
                        txtInsertCellId.Text = string.Empty;
                        bSaveRunFlag = false;

                        //등록할 대상이 존재하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0125"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtInsertCellId.SelectAll();
                                txtInsertCellId.Focus();
                            }
                        });
                        return;
                    }

                    // 샘플 등록 실패한 [%]건에 대해서 재등록 실행하시겠습니까? (성공한 Cell ID는 재등록 대상에서 제외됩니다.)
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0631", iFailCount.ToString()), null, "Information", MessageBoxButton.YesNo, MessageBoxIcon.None, (result_restore) =>
                    {
                        if (result_restore == MessageBoxResult.OK)  // SUCCESS 목록 삭제
                        {
                            DataTable dt = DataTableConverter.Convert(dgInsert.ItemsSource);

                            List<DataRow> drSuccess = new List<DataRow>();

                            foreach (DataRow dr in dt.Rows)
                            {
                                string sResult = dr["RESULT"].ToString();

                                if (string.IsNullOrWhiteSpace(sResult) == false)
                                {
                                    if (sResult.Contains("SUCCESS"))
                                    {
                                        drSuccess.Add(dr);
                                    }

                                    // RESULT 컬럼 값 초기화
                                    dr["RESULT"] = string.Empty;
                                }
                            }

                            foreach (DataRow drRemove in drSuccess)
                            {
                                dt.Rows.Remove(drRemove);
                            }

                            Util.GridSetData(dgInsert, dt, this.FrameOperation);
                            txtInsertCellCnt.Text = dt.Rows.Count.ToString();


                            // SUCCESS 항목 제거 후 실행 목록 Count 체크
                            if (dgInsert.Rows.Count == 0)
                            {
                                //등록할 대상이 존재하지 않습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0125"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtInsertCellId.SelectAll();
                                        txtInsertCellId.Focus();
                                    }
                                });
                                return;
                            }

                            // 샘플 등록 실행

                            List<DataRowView> drInsert = new List<DataRowView>();

                            for (int i = dgInsert.TopRows.Count; i < dgInsert.Rows.Count - dgInsert.BottomRows.Count; i++)
                            {
                                DataRowView drv = dgInsert.Rows[i].DataItem as DataRowView;

                                drInsert.Add(drv);
                            }

                            // 2024.10.08 김용준: Sample Cell 추출 실행 방식을 Backgroud Progress 방식으로 변경
                            string sAutoYN = (bool)rdoAuto.IsChecked ? "N" : "Y";

                            bool bGoodFlag = (bool)rdoGood.IsChecked ? true : false;

                            string sReason = txtReason.Text;

                            object[] argument = new object[5] { drInsert, sSampleType, sAutoYN, bGoodFlag, sReason };

                            int totalCount = drInsert.Count;

                            xProgress.Percent = 0;

                            xProgress.ProgressText = "0 / " + totalCount.Nvc();

                            xProgress.Visibility = Visibility.Visible;

                            if (rdoManual.IsChecked == true)
                            {
                                //(배출여부 : 수동)은 Cell을 추출한 이후에만 선택하셔야 됩니다. 
                                //수동으로 선택할 경우 Selector에서 Cell이 빠지지 않습니다. (수동)선택을 유지하시겠습니까?
                                Util.MessageConfirm("FM_ME_0345", (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        rdoAuto.IsChecked = false;
                                        rdoManual.IsChecked = true;
                                    }

                                    if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                                    {
                                        // Sample 등록 실행 안함. Auto/Manual 상태만 변경. Prograss Visibility 변경
                                        xProgress.Visibility = Visibility.Collapsed;

                                        if (rdoAuto.IsVisible == true)
                                        {
                                            rdoAuto.IsChecked = true;
                                            rdoManual.IsChecked = false;
                                        }

                                        else
                                        {
                                            rdoAuto.IsChecked = false;
                                            rdoManual.IsChecked = true;
                                        }
                                    }

                                    else
                                    {
                                        xProgress.RunWorker(argument);

                                        // Sample Cell 추출 처리 완료 시까지 Control 비활성화
                                        cboSampleType.IsEnabled = false;
                                        txtInsertCellId.IsEnabled = false;
                                        txtReason.IsEnabled = false;
                                        rdoAuto.IsEnabled = false;
                                        rdoManual.IsEnabled = false;
                                        rdoGood.IsEnabled = false;
                                        rdoNG.IsEnabled = false;
                                        btnInsertSave.IsEnabled = false;
                                        btnInsertInputCell.IsEnabled = false;
                                        btnInsertClear.IsEnabled = false;
                                        btnFileReg.IsEnabled = false;

                                        bSaveRunFlag = true;
                                    }
                                });
                            }
                            else
                            {
                                xProgress.RunWorker(argument);

                                // Sample Cell 추출 처리 완료 시까지 Control 비활성화
                                cboSampleType.IsEnabled = false;
                                txtInsertCellId.IsEnabled = false;
                                txtReason.IsEnabled = false;
                                rdoAuto.IsEnabled = false;
                                rdoManual.IsEnabled = false;
                                rdoGood.IsEnabled = false;
                                rdoNG.IsEnabled = false;
                                btnInsertSave.IsEnabled = false;
                                btnInsertInputCell.IsEnabled = false;
                                btnInsertClear.IsEnabled = false;
                                btnFileReg.IsEnabled = false;

                                bSaveRunFlag = true;
                            }

                        }
                        else if (result_restore == MessageBoxResult.No)  // RETURN
                        {
                            return;
                        }
                    });
                }
                else
                {

                    // 샘플 등록 실행

                    List<DataRowView> drInsert = new List<DataRowView>();

                    for (int i = dgInsert.TopRows.Count; i < dgInsert.Rows.Count - dgInsert.BottomRows.Count; i++)
                    {
                        DataRowView drv = dgInsert.Rows[i].DataItem as DataRowView;

                        drInsert.Add(drv);
                    }

                    // 2024.10.08 김용준: Sample Cell 추출 실행 방식을 Backgroud Progress 방식으로 변경
                    string sAutoYN = (bool)rdoAuto.IsChecked ? "N" : "Y";

                    bool bGoodFlag = (bool)rdoGood.IsChecked ? true : false;

                    string sReason = txtReason.Text;

                    object[] argument = new object[5] { drInsert, sSampleType, sAutoYN, bGoodFlag, sReason };

                    int totalCount = drInsert.Count;

                    xProgress.Percent = 0;

                    xProgress.ProgressText = "0 / " + totalCount.Nvc();

                    xProgress.Visibility = Visibility.Visible;

                    if (rdoManual.IsChecked == true)
                    {
                        //(배출여부 : 수동)은 Cell을 추출한 이후에만 선택하셔야 됩니다. 
                        //수동으로 선택할 경우 Selector에서 Cell이 빠지지 않습니다. (수동)선택을 유지하시겠습니까?
                        Util.MessageConfirm("FM_ME_0345", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                rdoAuto.IsChecked = false;
                                rdoManual.IsChecked = true;
                            }

                            if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                            {
                                // Sample 등록 실행 안함. Auto/Manual 상태만 변경. Prograss Visibility 변경
                                xProgress.Visibility = Visibility.Collapsed;

                                if (rdoAuto.IsVisible == true)
                                {
                                    rdoAuto.IsChecked = true;
                                    rdoManual.IsChecked = false;
                                }

                                else
                                {
                                    rdoAuto.IsChecked = false;
                                    rdoManual.IsChecked = true;
                                }
                            }

                            else
                            {
                                xProgress.RunWorker(argument);

                                // Sample Cell 추출 처리 완료 시까지 Control 비활성화
                                cboSampleType.IsEnabled = false;
                                txtInsertCellId.IsEnabled = false;
                                txtReason.IsEnabled = false;
                                rdoAuto.IsEnabled = false;
                                rdoManual.IsEnabled = false;
                                rdoGood.IsEnabled = false;
                                rdoNG.IsEnabled = false;
                                btnInsertSave.IsEnabled = false;
                                btnInsertInputCell.IsEnabled = false;
                                btnInsertClear.IsEnabled = false;
                                btnFileReg.IsEnabled = false;

                                bSaveRunFlag = true;
                            }
                        });
                    }
                    else
                    {
                        xProgress.RunWorker(argument);

                        // Sample Cell 추출 처리 완료 시까지 Control 비활성화
                        cboSampleType.IsEnabled = false;
                        txtInsertCellId.IsEnabled = false;
                        txtReason.IsEnabled = false;
                        rdoAuto.IsEnabled = false;
                        rdoManual.IsEnabled = false;
                        rdoGood.IsEnabled = false;
                        rdoNG.IsEnabled = false;
                        btnInsertSave.IsEnabled = false;
                        btnInsertInputCell.IsEnabled = false;
                        btnInsertClear.IsEnabled = false;
                        btnFileReg.IsEnabled = false;

                        bSaveRunFlag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private int GetByteLength(string str)
        {
            int byteLength = 0;

            foreach (char c in str)
            {
                if (IsKorean(c))
                {
                    byteLength += 2; // 한글은 2byte
                }
                else
                {
                    byteLength += 1; // 그 외의 문자 1byte
                }
            }
            return byteLength;
        }

        private bool IsKorean(char c)
        {
            // 한글 유니코드 범위 확인
            return (c >= '\uAC00' && c<= '\uD7A3');
        }


        private void Insert(string sSampleType)
        {
            //저장하시겠습니까?
            Util.MessageConfirm("FM_ME_0214", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    string BizRuleID = "";
                    if ((bool)rdoGood.IsChecked) BizRuleID = "BR_SET_SMPL_CELL_ALL";
                    else BizRuleID = "BR_SET_NGLOT_SMPL_CELL";

                    int iProcessingCnt = 100; //한번에 처리하는 Tray 건수
                    double dNumberOfProcessingCnt = 0.0;
                    bool bIsOK = true;

                    DataSet indataSet = new DataSet();
                    DataTable dtIndata = indataSet.Tables.Add("INDATA");
                    dtIndata.Columns.Add("USERID", typeof(string));
                    dtIndata.Columns.Add("TD_FLAG", typeof(string));
                    dtIndata.Columns.Add("SMPL_RSN", typeof(string));
                    dtIndata.Columns.Add("SPLT_FLAG", typeof(string));
                    dtIndata.Columns.Add("MENUID", typeof(string));
                    dtIndata.Columns.Add("USER_IP", typeof(string));
                    dtIndata.Columns.Add("PC_NAME", typeof(string));

                    DataTable dtInCell = indataSet.Tables.Add("INCELL");
                    dtInCell.Columns.Add("SUBLOTID", typeof(string));
                    dtInCell.Columns.Add("UNPACK_CELL_YN", typeof(string));

                    DataRow InRow = dtIndata.NewRow();
                    InRow["USERID"] = LoginInfo.USERID;
                    InRow["TD_FLAG"] = sSampleType;
                    InRow["SMPL_RSN"] = txtReason.Text;                    
                    InRow["SPLT_FLAG"] = (rdoAuto.IsChecked == true) ? "N" : "Y";
                    InRow["MENUID"] = LoginInfo.CFG_MENUID;
                    InRow["USER_IP"] = LoginInfo.USER_IP;
                    InRow["PC_NAME"] = LoginInfo.PC_NAME;

                    dtIndata.Rows.Add(InRow);

                    ShowLoadingIndicator();
                    dNumberOfProcessingCnt = Math.Ceiling(dgInsert.Rows.Count / (double)iProcessingCnt);//처리수량

                    for (int k = 0; k < dNumberOfProcessingCnt; k++) //나눠서 처리
                    {
                        dtInCell.Clear();

                        for (int i = (k * (int)iProcessingCnt); i < (k * iProcessingCnt + iProcessingCnt); i++)
                        {
                            if (i >= dgInsert.Rows.Count) break;

                            DataRow RowCell = dtInCell.NewRow();
                            RowCell["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "SUBLOTID"));
                            if ((bool)rdoGood.IsChecked)
                            {
                                RowCell["UNPACK_CELL_YN"] = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "UNPACK_CELL_YN"));
                            }

                            dtInCell.Rows.Add(RowCell);

                        }

                        try
                        {
                            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizRuleID, "INDATA,INCELL", "OUTDATA", indataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            Util.MessageValidation("SFU9200", (k * (int)iProcessingCnt).ToString("#,##0"), (k * iProcessingCnt + iProcessingCnt - 1).ToString("#,##0"));
                            bIsOK = false;
                            return;
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    } // K for문       
                    if (bIsOK)
                    {
                        //저장하였습니다.
                        Util.MessageInfo("FM_ME_0215");
                        dgInsert.ItemsSource = null;
                    }
                    else
                    {
                        //저장실패하였습니다.
                        Util.MessageInfo("FM_ME_0213");
                    }

                }
            });
        }

        private void btnDelCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Sample 등록 실행 중 체크
                if (bSaveRunFlag == true)
                    return;


                DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                if (presenter == null)
                    return;

                int clickedIndex = presenter.Row.Index;
                DataTable dt = DataTableConverter.Convert(dgInsert.ItemsSource);
                dt.Rows.RemoveAt(clickedIndex);
                Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

                txtInsertCellCnt.Text = (int.Parse(txtInsertCellCnt.Text) - 1).ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("CSTID_PV"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

                if (e.Cell.Column.Name.Equals("CHK"))
                {
                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, 0);
                    cell.Presenter.IsEnabled = true;

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SMPL_STAT")).Equals(ObjectDic.Instance.GetObjectName("RESTORE")) // 복구된 셀인 경우
                     || Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_SLOC_SMPL_FLAG")).Equals("Y")) //불량창고의 셀일 경우
                    {
                        cell.Presenter.IsEnabled = false;
                    }
                }

            }));
        }

        private void dgSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
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

        private void btnSearchRecover_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 2021-05-13 grid Check 확인 로직 오류로 제거
                /*int iRCVCnt = 0;

                 for (int i = 0; i < dgSearch.Rows.Count; i++)
                  {
                      if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("0"))
                      {
                          iRCVCnt++;
                      }
                  }
                */
                if (!dgSearch.IsCheckedRow("CHK"))
                {
                    //복구 대상이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0139"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                    return;
                }

                #region BizWF 
                for (int i = 0; i < dgSearch.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("1"))
                    {

                        int RetVal = BizWF_Check(Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "SUBLOTID")), "Z");

                        if (RetVal != 0)
                        {
                            //ShowLoadingIndicator();
                            //loadingIndicator.Visibility = Visibility.Hidden;
                            return;
                        }
                    }
                }
                #endregion                        

                //복구하시겠습니까?
                Util.MessageConfirm("FM_ME_0141", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        DataTable dtIndata = indataSet.Tables.Add("INDATA");
                        dtIndata.Columns.Add("USERID", typeof(string));
                        dtIndata.Columns.Add("TD_FLAG", typeof(string));
                        dtIndata.Columns.Add("GLOT_FLAG", typeof(string));
                        dtIndata.Columns.Add("MENUID", typeof(string)); // 2025.04.09 이현승 : 등급변경 적용 UI 추적성 향상을 위한 MENUID 추가

                        DataTable dtInCell = indataSet.Tables.Add("INCELL");
                        dtInCell.Columns.Add("SUBLOTID", typeof(string));

                        DataRow InRow = dtIndata.NewRow();
                        InRow["USERID"] = LoginInfo.USERID;
                        InRow["TD_FLAG"] = "C";
                        InRow["GLOT_FLAG"] = "Y";
                        InRow["MENUID"] = LoginInfo.CFG_MENUID;

                        for (int i = 0; i < dgSearch.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                DataRow RowCell = dtInCell.NewRow();
                                RowCell["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "SUBLOTID"));
                                dtInCell.Rows.Add(RowCell);
                            }
                        }

                        //기존 Tray로 복구하겠습니까? (버튼을 OK / NO로 생성함)
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0482"), null, "Information", MessageBoxButton.YesNo, MessageBoxIcon.None, (result_restore) =>
                        {
                            if (result_restore == MessageBoxResult.OK) // 기존TRAY로 복구
                            {
                                InRow["GLOT_FLAG"] = "N";
                                dtIndata.Rows.Add(InRow);

                                ShowLoadingIndicator();
                                new ClientProxy().ExecuteService_Multi("BR_SET_SMPL_CELL", "INDATA,INCELL", "OUTDATA", (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }

                                        btnSearch_Click(null, null);

                                    }
                                    catch (Exception ex)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    }
                                    finally
                                    {
                                        HiddenLoadingIndicator();
                                    }
                                }, indataSet);
                            }

                            if (result_restore == MessageBoxResult.No) // 가상 LOT 발번하여 복구
                            {
                                dtIndata.Rows.Add(InRow);

                                ShowLoadingIndicator();
                                new ClientProxy().ExecuteService_Multi("BR_SET_SMPL_CELL", "INDATA,INCELL", "OUTDATA", (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }

                                        btnSearch_Click(null, null);

                                    }
                                    catch (Exception ex)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    }
                                    finally
                                    {
                                        HiddenLoadingIndicator();
                                    }
                                }, indataSet);
                            }
                        });


                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (datagrid.CurrentColumn.Name.Equals("CSTID") || datagrid.CurrentColumn.Name.Equals("CSTID_PV"))
                {
                    FCS001_021 wndRunStart = new FCS001_021();
                    wndRunStart.FrameOperation = FrameOperation;

                    if (wndRunStart != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = cell.Text;

                        this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 전체 선택, 해제
        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "SMPL_STAT").Equals(ObjectDic.Instance.GetObjectName("SAMPLE"))
                && DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "DFCT_SLOC_SMPL_FLAG").Equals("N"))   // 양품창고의 복구되지 않은 셀만 선택
                {
                    DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", false);
            }
        }
        #endregion

        #region 복구
        private void btnRecoverInputCell_Click(object sender, RoutedEventArgs e)
        {
            string[] arrsCellID = txtRecoverCellId.Text.Trim().Split(',');

            foreach (string item in arrsCellID)
            {
                GetRecoverCellInfo(item.Trim());
            }

        }

        private void btnRecoverClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSearch);
            txtRecoverCellCnt.Text = "0";
            txtRecoverCellId.Text = string.Empty;
        }

        private void btnRecoverSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRecover.Rows.Count == 0)
                {
                    //복구 대상이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0139"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtInsertCellId.SelectAll();
                            txtInsertCellId.Focus();
                        }
                    });
                    return;
                }

                //복구하시겠습니까?
                Util.MessageConfirm("FM_ME_0141", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        DataTable dtIndata = indataSet.Tables.Add("INDATA");
                        dtIndata.Columns.Add("USERID", typeof(string));
                        dtIndata.Columns.Add("TD_FLAG", typeof(string));
                        dtIndata.Columns.Add("GLOT_FLAG", typeof(string));
                        dtIndata.Columns.Add("MENUID", typeof(string)); // 2025.04.09 이현승 : 등급변경 적용 UI 추적성 향상을 위한 MENUID 추가

                        DataTable dtInCell = indataSet.Tables.Add("INCELL");
                        dtInCell.Columns.Add("SUBLOTID", typeof(string));

                        DataRow InRow = dtIndata.NewRow();
                        InRow["USERID"] = LoginInfo.USERID;
                        InRow["TD_FLAG"] = "C";
                        InRow["GLOT_FLAG"] = "Y";
                        InRow["MENUID"] = LoginInfo.CFG_MENUID;

                        for (int i = 0; i < dgRecover.Rows.Count; i++)
                        {
                            DataRow RowCell = dtInCell.NewRow();
                            RowCell["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID"));
                            dtInCell.Rows.Add(RowCell);
                        }

                        //기존 Tray로 복구하겠습니까? (버튼을 OK / NO로 생성함)
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0482"), null, "Information", MessageBoxButton.YesNo, MessageBoxIcon.None, (result_restore) =>
                        {
                            if (result_restore == MessageBoxResult.OK)  // 기존TRAY로 복구
                            {
                                InRow["GLOT_FLAG"] = "N";
                                dtIndata.Rows.Add(InRow);

                                ShowLoadingIndicator();
                                new ClientProxy().ExecuteService_Multi("BR_SET_SMPL_CELL", "INDATA,INCELL", "OUTDATA", (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }

                                        if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                        {
                                            //복구 완료하였습니다.
                                            Util.MessageInfo("FM_ME_0140");
                                            dgRecover.ItemsSource = null;

                                        }
                                        else
                                        {
                                            //복구실패하였습니다.
                                            Util.MessageInfo("FM_ME_0311");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    }
                                    finally
                                    {
                                        HiddenLoadingIndicator();
                                    }
                                }, indataSet);
                            }

                            if (result_restore == MessageBoxResult.No) // 가상 LOT 발번하여 복구
                            {
                                dtIndata.Rows.Add(InRow);

                                ShowLoadingIndicator();
                                new ClientProxy().ExecuteService_Multi("BR_SET_SMPL_CELL", "INDATA,INCELL", "OUTDATA", (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }

                                        if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                        {
                                            //복구 완료하였습니다.
                                            Util.MessageInfo("FM_ME_0140");
                                            dgRecover.ItemsSource = null;

                                        }
                                        else
                                        {
                                            //복구실패하였습니다.
                                            Util.MessageInfo("FM_ME_0311");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    }
                                    finally
                                    {
                                        HiddenLoadingIndicator();
                                    }
                                }, indataSet);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtRecoverCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtRecoverCellId.Text.Trim() == string.Empty)
                    return;



                btnRecoverInputCell_Click(null, null);
            }
        }


        private void btnRDelCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region MyRegion
                //Button btnRDelCell = sender as Button;
                //DataTable dt = DataTableConverter.Convert(dgRecover.ItemsSource);
                //if (btnRDelCell != null)
                //{
                //    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                //    if (string.Equals(btnRDelCell.Name, "btnRDelCell"))
                //    {
                //        DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                //        if (presenter == null)
                //            return;

                //        int clickedIndex = presenter.Row.Index;
                //        dt.Rows.RemoveAt(clickedIndex);
                //        Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

                //        txtRecoverCellCnt.Text = dt.Rows.Count.ToString();
                //    }
                //} 
                #endregion

                DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                if (presenter == null)
                    return;

                int clickedIndex = presenter.Row.Index;
                DataTable dt = DataTableConverter.Convert(dgRecover.ItemsSource);
                dt.Rows.RemoveAt(clickedIndex);
                Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

                txtRecoverCellCnt.Text = (int.Parse(txtRecoverCellCnt.Text) - 1).ToString();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtInsertCellId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string _ValueToFind = string.Empty;
                    bool bFlag = false;

                    if (sPasteStrings.Count() > 5001) // 4월 5일 5000개로 수정
                    {
                        Util.MessageValidation("SFU9201");   //최대 5000개 까지 가능합니다. 
                        return;
                    }

                    if (sPasteStrings[0].Trim() == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    #region BizWF & Cell Info
                    // 2024.10.08 김용준 : BizWF 체크 다중처리로 변경
                    if (BizWF_Check_Multi(sPasteString))
                    {
                        GetCellInfo(sPasteString);
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion

        #endregion

        #region Method

        private bool BizWF_Check_Multi(string sCellID)
        {
            try
            {
                string[] stringSeparators = new string[] { "\r\n" };
                string sPasteString = sCellID;
                string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("TD_FLAG", typeof(string));

                int RetVal = -1;
                double dNumberOfProcessingCnt = 0.0;

                int iProcessingCnt = 500;

                dNumberOfProcessingCnt = Math.Ceiling(sPasteStrings.Length / (double)iProcessingCnt);//처리수량

                int iStartNum = 0;
                int iEndNum = 0;

                for (int k = 0; k < dNumberOfProcessingCnt; k++) //나눠서 처리
                {
                    dtRqst.Clear();

                    for (int i = (k * (int)iProcessingCnt); i < (k * iProcessingCnt + iProcessingCnt); i++)
                    {
                        iStartNum = (k * (int)iProcessingCnt);

                        if (i >= sPasteStrings.Length) break;

                        if (string.IsNullOrWhiteSpace(sPasteStrings[i].ToString()) == false)
                        {
                            DataRow dr = dtRqst.NewRow();
                            dr["SUBLOTID"] = sPasteStrings[i].ToString();
                            dr["LANGID"] = LoginInfo.LANGID;
                            dr["TD_FLAG"] = "Z";
                            dtRqst.Rows.Add(dr);
                        }
                        iEndNum = i;
                    }

                    int iA = iStartNum;
                    int iB = iEndNum;
                    if (dtRqst.Rows.Count > 0)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SMPL_COM_CHK_LOOP", "INDATA", "OUTDATA", dtRqst);

                        RetVal = Convert.ToInt16(dtRslt.Rows[0]["RETVAL"]);

                        if (RetVal != 0)
                        {
                            loadingIndicator.Visibility = Visibility.Hidden;
                            Util.MessageValidation("SFU8216");   //데이터 확인이 필요합니다.
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Hidden;
                Util.MessageException(ex);
                return true;
            }
        }

        private bool GetCellInfo(string sCellID)
        {
            try
            {
                //string sCellID = string.Empty;
                if (string.IsNullOrEmpty(sCellID))
                    return false;

                string[] stringSeparators = new string[] { "\r\n" };
                string[] CellStrings = sCellID.Split(stringSeparators, StringSplitOptions.None);

                //스프레드에 있는지 확인
                for (int j = 0; j < CellStrings.Length; j++)
                {


                    for (int i = 0; i < dgInsert.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "SUBLOTID").ToString() == CellStrings[j].ToString())
                        {
                            //목록에 기존재하는 Cell 입니다. 
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0132", sCellID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    SetInit();
                                }
                            });
                            return false;
                        }

                    }
                }
                //4월 5일 CELLID 형식 변경 (기존 : 1개  --> 수정 : 입력받은 모든 CELL을 CELL1,CELL2,CEL3 ...)
                string Temp_CellList = sCellID.Replace("\r", ",");
                string CellList = Temp_CellList.Replace("\n", "");

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SPLT_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = Util.NVC(CellList);
                dr["SPLT_FLAG"] = "N";
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                /* PI 요청으로 롤백 20231012 
                //2023.09.22 장기보류 CELL (LOT_DETL_TYPE_CODE = 'H')은 양품 추출 불가능 하도록 등록 제한.START
                if ((bool)rdoGood.IsChecked)
                {
                    int RETVAL = H_Lot_Check(Temp_CellList, CellList);  // 장기보류 LOT 체크

                    if ( RETVAL == -1 ) //장기보류 LOT 있을때: -1 ELSE: 0 
                    {
                        return false;
                    }
                }
                //END
                */
                string BizRule = "";
                if ((bool)rdoGood.IsChecked) BizRule = "DA_SEL_CELL_SAMPLE_YN";
                else BizRule = "DA_SEL_CELL_SAMPLE_YN_NG_LIST";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(BizRule, "RQSTDT", "RSLTDT", dtRqst);

                // 2024.10.07 김용준 - 실행 결과 표시를 위하여 추가
                dtRslt.Columns.Add("RESULT", typeof(string));

                if (dtRslt.Rows.Count > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgInsert.ItemsSource);

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtRslt.Rows.Count; i++)
                        {
                            DataRow dr1 = dt.NewRow();
                            dr1["SUBLOTID"] = Util.NVC(dtRslt.Rows[i]["SUBLOTID"]);
                            dr1["CSTID"] = Util.NVC(dtRslt.Rows[i]["CSTID"]);
                            dr1["ROUTID"] = Util.NVC(dtRslt.Rows[i]["ROUTID"]);
                            dr1["PROD_LOTID"] = Util.NVC(dtRslt.Rows[i]["PROD_LOTID"]);
                            if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[i]["CSTSLOT"]))) dr1["CSTSLOT"] = Util.NVC_Decimal(dtRslt.Rows[i]["CSTSLOT"]);
                            dr1["ROUT_NAME"] = Util.NVC(dtRslt.Rows[i]["ROUT_NAME"]);
                            dr1["LOTID"] = Util.NVC(dtRslt.Rows[i]["LOTID"]);
                            dr1["EQSGID"] = Util.NVC(dtRslt.Rows[i]["EQSGID"]);
                            //2021.07.05 컬럼 추가
                            dr1["SUBLOTJUDGE"] = Util.NVC(dtRslt.Rows[i]["SUBLOTJUDGE"]);
                            dr1["LOT_DETL_TYPE_CODE"] = Util.NVC(dtRslt.Rows[i]["LOT_DETL_TYPE_CODE"]);
                            dr1["DFCT_YN"] = Util.NVC(dtRslt.Rows[i]["DFCT_YN"]);
                            if ((bool)rdoGood.IsChecked) dr1["UNPACK_CELL_YN"] = Util.NVC(dtRslt.Rows[i]["UNPACK_CELL_YN"]); //양품일 경우 포장 대기 cell 구분
                            // 2024.10.07 김용준 - 실행 결과 표시를 위하여 추가
                            dr1["RESULT"] = Util.NVC(dtRslt.Rows[i]["RESULT"]);

                            dt.Rows.Add(dr1);

                        }
                        Util.GridSetData(dgInsert, dt, FrameOperation, true);
                        txtInsertCellCnt.Text = (int.Parse(txtInsertCellCnt.Text) + dtRslt.Rows.Count).ToString();
                    }
                    else
                    {
                        Util.GridSetData(dgInsert, dtRslt, FrameOperation, true);
                        txtInsertCellCnt.Text = dtRslt.Rows.Count.ToString();
                    }
                }
                else
                {
                    //[Cell ID : {0}]의 정보가 존재하지 않거나, 이미 추출된 Cell 입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0308", sCellID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //SetInit();
                        }
                    });
                    return false;
                }
                //SetInit();
                return true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetRecoverCellInfo(string sCellID)
        {
            try
            {
                //E20231116-000996 : 기술샘플 복구 cell 1개씩 입력에서 콤마로 N개 입력 가능 ex) aaaaa,bbbb,cccc,dddd~~~~
                // string sCellID = string.Empty;

                if (string.IsNullOrEmpty(txtRecoverCellId.Text))
                    return;
                //else
                //    sCellID = txtRecoverCellId.Text.Trim();

                #region BizWF 
                int RetVal = BizWF_Check(sCellID, "Z");

                if (RetVal != 0)
                {
                    //ShowLoadingIndicator();
                    //loadingIndicator.Visibility = Visibility.Hidden;
                    return;
                }
                #endregion

                //스프레드에 있는지 확인
                for (int i = 0; i < dgRecover.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID").ToString() == sCellID)
                    {
                        //목록에 기존재하는 Cell 입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0132"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SetInit();
                            }
                        });
                        return;
                    }
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SPLT_FLAG", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = Util.NVC(sCellID);
                dr["SPLT_FLAG"] = "Y";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                // DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_SAMPLE_YN", "RQSTDT", "RSLTDT", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_RECOVER", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgRecover.ItemsSource);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr1 = dt.NewRow();
                        dr1["SUBLOTID"] = Util.NVC(dtRslt.Rows[0]["SUBLOTID"]);
                        dr1["CSTID"] = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                        dr1["ROUTID"] = Util.NVC(dtRslt.Rows[0]["ROUTID"]);
                        dr1["PROD_LOTID"] = Util.NVC(dtRslt.Rows[0]["PROD_LOTID"]);
                        if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["CSTSLOT"])))
                            dr1["CSTSLOT"] = Util.NVC_Decimal(dtRslt.Rows[0]["CSTSLOT"]);
                        dr1["ROUT_NAME"] = Util.NVC(dtRslt.Rows[0]["ROUT_NAME"]);
                        dr1["LOTID"] = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                        dr1["EQSGID"] = Util.NVC(dtRslt.Rows[0]["EQSGID"]);

                        dt.Rows.Add(dr1);
                        Util.GridSetData(dgRecover, dt, FrameOperation, true);
                        txtRecoverCellCnt.Text = (int.Parse(txtRecoverCellCnt.Text) + dtRslt.Rows.Count).ToString();
                    }
                    else
                    {
                        Util.GridSetData(dgRecover, dtRslt, FrameOperation, true);
                        txtRecoverCellCnt.Text = dtRslt.Rows.Count.ToString();
                    }
                }
                else
                {
                    //[Cell ID : {0}]은 추출된 Cell이 아닙니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0309", sCellID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            RcvInit();
                        }
                    });
                    return;
                }

                RcvInit();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetList()
        {
            try
            {
                chkAll.IsChecked = false;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SMPL_TYPE_CODE", typeof(string));
                //20220329_조회조건추가-생산라인,모델,LotType START
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("DFCT_SLOC_SMPL_FLAG", typeof(string));
                //20220329_조회조건추가-생산라인,모델,LotType END

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                if (!string.IsNullOrEmpty(txtSearchCellId.Text))
                {
                    dr["SUBLOTID"] = Util.GetCondition(txtSearchCellId, bAllNull: true);
                }

                dr["SMPL_TYPE_CODE"] = Util.GetCondition(cboSearchSampleType, bAllNull: true);
                //20220329_조회조건추가-생산라인,모델,LotType START
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true);
                //20220329_조회조건추가-생산라인,모델,LotType END
                dr["DFCT_SLOC_SMPL_FLAG"] = Util.GetCondition(cboDfctSlocSmplFlag, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SAMPLE_CELL", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgSearch, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetInit()
        {
            txtInsertCellId.SelectAll();
            txtInsertCellId.Focus();
        }

        private void RcvInit()
        {
            txtRecoverCellId.SelectAll();
            txtRecoverCellId.Focus();
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

        private void rdoLotType_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgInsert);

            if (sender == null || e == null) return;
            RadioButton rdo = sender as RadioButton;

            if (rdo.Name.Equals(rdoNG.Name))
            {
                rdoManual.IsChecked = true;
                rdoAuto.IsChecked = false;
            }
            else
            {
                if (rdoAuto.IsVisible == true)
                {
                    rdoManual.IsChecked = true;
                    rdoAuto.IsChecked = false;
                }

                else
                {
                    rdoManual.IsChecked = true;
                    rdoAuto.IsChecked = false;
                }

            }
        }

        #endregion

        private void dgSearch_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgSearch.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private int BizWF_Check(string SubLotID, string TD_Flag)
        {
            int RetVal = -1;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("TD_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = SubLotID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TD_FLAG"] = TD_Flag;
                dtRqst.Rows.Add(dr);

                //ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SMPL_COM_CHK", "INDATA", "OUTDATA", dtRqst);

                RetVal = Convert.ToInt16(dtRslt.Rows[0]["RETVAL"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return RetVal;
            }

            return RetVal;
        }

        private object xProgress_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {

            try
            {
                object[] arguments = e.Arguments as object[];

                List<DataRow> processingRows = new List<DataRow>();

                List<DataRowView> drInsert = arguments[0] as List<DataRowView>;

                string sSampleType = arguments[1].Nvc();

                string sAutoYN = arguments[2].Nvc();

                bool bGoodFlag = arguments[3].SafeToBoolean();

                string sReason = arguments[4].Nvc();

                int workCount = 0; //실행 Count

                int totalCount = drInsert.Count;

                string updateResultText = string.Empty;

                int iProcessingCnt = 300; //한번에 처리하는 Tray 건수

                double dNumberOfProcessingCnt = 0.0;

                string BizRuleID = "";

                if (bGoodFlag)
                {
                    BizRuleID = "BR_SET_SMPL_CELL_ALL";
                }
                else
                {
                    BizRuleID = "BR_SET_NGLOT_SMPL_CELL";
                }

                DataSet indataSet = new DataSet();
                DataTable dtIndata = indataSet.Tables.Add("INDATA");
                dtIndata.Columns.Add("USERID", typeof(string));
                dtIndata.Columns.Add("TD_FLAG", typeof(string));
                dtIndata.Columns.Add("SMPL_RSN", typeof(string));
                dtIndata.Columns.Add("SPLT_FLAG", typeof(string));
                dtIndata.Columns.Add("MENUID", typeof(string));
                dtIndata.Columns.Add("USER_IP", typeof(string));
                dtIndata.Columns.Add("PC_NAME", typeof(string));

                DataTable dtInCell = indataSet.Tables.Add("INCELL");
                dtInCell.Columns.Add("SUBLOTID", typeof(string));
                dtInCell.Columns.Add("UNPACK_CELL_YN", typeof(string));

                DataRow InRow = dtIndata.NewRow();
                InRow["USERID"] = LoginInfo.USERID;
                InRow["TD_FLAG"] = sSampleType;
                InRow["SMPL_RSN"] = sReason;
                InRow["SPLT_FLAG"] = sAutoYN;
                InRow["MENUID"] = LoginInfo.CFG_MENUID;
                InRow["USER_IP"] = LoginInfo.USER_IP;
                InRow["PC_NAME"] = LoginInfo.PC_NAME;

                dtIndata.Rows.Add(InRow);

                dNumberOfProcessingCnt = Math.Ceiling(drInsert.Count / (double)iProcessingCnt);//처리수량

                for (int k = 0; k < dNumberOfProcessingCnt; k++) //나눠서 처리
                {
                    dtInCell.Clear();
                    processingRows.Clear();

                    for (int i = (k * (int)iProcessingCnt); i < (k * iProcessingCnt + iProcessingCnt); i++)
                    {
                        if (i >= drInsert.Count) break;

                        workCount++;

                        DataRow RowCell = dtInCell.NewRow();

                        RowCell["SUBLOTID"] = Util.NVC(drInsert[i]["SUBLOTID"]);

                        if (bGoodFlag)
                        {
                            RowCell["UNPACK_CELL_YN"] = Util.NVC(drInsert[i]["UNPACK_CELL_YN"]);
                        }


                        dtInCell.Rows.Add(RowCell);

                        DataRowView drv = drInsert[i] as DataRowView;

                        processingRows.Add(drv.Row);

                    }

                    try
                    {
                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizRuleID, "INDATA,INCELL", "OUTDATA", indataSet);

                        if (dsResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "0")
                        {
                            updateResultText = "SUCCESS";
                        }
                        else
                        {
                            updateResultText = "FAIL";
                        }
                    }
                    catch (Exception ex)
                    {
                        updateResultText = "FAIL : " + ex.Message;
                    }


                    object[] progressArgument = new object[1] { workCount.Nvc() + " / " + totalCount.Nvc() };

                    e.Worker.ReportProgress(Convert.ToInt16((double)workCount / (double)totalCount * 100), progressArgument);

                    foreach (DataRow row in processingRows)
                    {
                        row["RESULT"] = updateResultText;
                    }
                }

                return "COMPLETED";
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] progressArguments = e.Arguments as object[];

                string progressText = progressArguments[0].Nvc();

                xProgress.Percent = percent;

                xProgress.ProgressText = progressText;

                xProgress.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgress_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                object[] arguments = e.Arguments as object[];

                if (e.Result != null && e.Result is string)
                {
                    if (e.Result.Nvc().Equals("COMPLETED"))
                    {
                        Util.AlertInfo("FM_ME_0215");
                    }
                    else
                    {
                        Util.AlertInfo("FM_ME_0213");
                    }
                }
                else if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException(e.Result as Exception);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                xProgress.Visibility = Visibility.Collapsed;

                // Sample Cell 추출 처리 완료 시 Control 활성화
                cboSampleType.IsEnabled = true;
                txtInsertCellId.IsEnabled = true;
                txtReason.IsEnabled = true;
                rdoAuto.IsEnabled = true;
                rdoManual.IsEnabled = true;
                rdoGood.IsEnabled = true;
                rdoNG.IsEnabled = true;
                btnInsertSave.IsEnabled = true;
                btnInsertInputCell.IsEnabled = true;
                btnInsertClear.IsEnabled = true;
                btnFileReg.IsEnabled = true;
            }
        }

        private void btnFileReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {

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

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("SUBLOTID", typeof(string));

                        if (sheet.Rows.Count > 5000)
                        {
                            Util.MessageValidation("SFU9201");   //최대 5000개 까지 가능합니다. 
                            return;
                        }

                        string sPasteString = string.Empty;

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            XLCell cell = sheet.GetCell(rowInx, 0);

                            if (cell != null)
                            {
                                sPasteString = sPasteString + cell.Text + "\r\n";
                            }
                        }

                        if (BizWF_Check_Multi(sPasteString))
                        {
                            GetCellInfo(sPasteString);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        /* PI 요청으로 롤백 20231012 
private int H_Lot_Check(string Temp_CellList, string CellList)
{
   int RETVAL = 0;
   try
   {
           string Hcellid = string.Empty;
           string HcellList = string.Empty;

           DataTable dtRqst = new DataTable();
           dtRqst.TableName = "RQSTDT";
           dtRqst.Columns.Add("SUBLOTID", typeof(string));

           DataRow dr = dtRqst.NewRow();
           dr["SUBLOTID"] = Util.NVC(CellList);

           dtRqst.Rows.Add(dr);

           ShowLoadingIndicator();

           DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_HLOT", "RQSTDT", "RSLTDT", dtRqst);
           if (dtRslt.Rows.Count > 0)  // 장기보류 LOT 있으면.
           {

               for (int i = 0; i < dtRslt.Rows.Count; i++)
               {
                   Hcellid = Util.NVC(dtRslt.Rows[i]["SUBLOTID"]);
                   HcellList += Hcellid + ",";

               }

               string EditCell = HcellList.TrimEnd(',');
               RETVAL = -1;

           LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0497", EditCell), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>  //장기 보류 CELL은 등록 할 수 없습니다.  
           {
                   if (result == MessageBoxResult.OK)
                   {
                       return;
                   }
               });

           }

           else // 장기보류 LOT 없으면 
           {

               return RETVAL;  // RETVAL 0
           }

   }

   catch (Exception ex)
   {
       LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);  
   }
   return RETVAL;  
}*/

    }
}
