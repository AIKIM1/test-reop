/*************************************************************************************
 Created Date : 2024.04.17
      Creator : 김도형
   Decription : 특이사항 ->전극 생산일별 특이사항(NEW)
--------------------------------------------------------------------------------------
 [Change History]
  2024.04.17  김도형 : Initial Created.
  2024.04.17  김도형 : [E20240319-000564] [ESNA]인수인계 화면 개선 요청 건
  2024.04.20  김도형 : [E20240319-000564] [ESNA]인수인계 화면 개선 요청 건( PRODID_BY_PJT)
  2024.06.03  김도형 : [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_401 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string InitLoadYn = "Y";

        private BizDataSet _Biz = new BizDataSet();
        public COM001_401()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion Declaration & Constructor 

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            InitCombo(); // Combo Setting
        }
        #endregion [Form Load]

        #region Initialize
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESS");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sCase: "EQUIPMENT");

            String[] sFilter = { "USE_FLAG" };
            String[] sFilter1 = { "CLSS_TYPE" };

            //사용유무 
            _combo.SetCombo(cboUseFlag, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");
            cboUseFlag.SelectedValue = "Y";
            // 구분
            _combo.SetCombo(cboClssType, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE");
        }

        #endregion

        // [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetDailyRemarksList();
        }
        // [신규]
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cboClssType.Items.Count > 0 && cboClssType.SelectedValue != null && !cboClssType.SelectedValue.Equals("SELECT"))
            {
                if (cboClssType.SelectedValue.ToString().Equals("PROD"))  // 구분이  생산일 경우
                {
                    COM001_401_PROD_DAILY_REMARKS popupProdDailyRemarks = new COM001_401_PROD_DAILY_REMARKS { FrameOperation = FrameOperation };

                    object[] parameters = new object[7];
                    parameters[0] = "INS";                                         // Save Mode : INS :신규, UPD : 수정, CHK:확인
                    parameters[1] = "";                                            // Save Mode = INS 인 경우 INPUTSEQNO = "";

                    if (cboArea.SelectedValue.ToString().Equals(""))
                    {
                        parameters[2] = "SELECT";              // 동
                    }
                    else
                    {
                        parameters[2] = cboArea.SelectedValue.ToString();              // 동
                    }

                    if (cboEquipmentSegment.SelectedValue.ToString().Equals(""))
                    {
                        parameters[3] = "SELECT";  // 라인
                    }
                    else
                    {
                        parameters[3] = cboEquipmentSegment.SelectedValue.ToString();  // 라인
                    }

                    if (cboProcess.SelectedValue.ToString().Equals(""))
                    {
                        parameters[4] = "SELECT";          // 공정
                    }
                    else
                    {
                        parameters[4] = cboProcess.SelectedValue.ToString();           // 공정
                    }

                    if (cboEquipment.SelectedValue.ToString().Equals(""))
                    {
                        parameters[5] = "SELECT";          // 설비
                    }
                    else
                    {
                        parameters[5] = cboEquipment.SelectedValue.ToString();         // 설비
                    }

                    parameters[6] = cboClssType.SelectedValue.ToString();          // 분류유형 
                    C1WindowExtension.SetParameters(popupProdDailyRemarks, parameters);

                    popupProdDailyRemarks.Closed += new EventHandler(popupProdDailyRemarks_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupProdDailyRemarks.ShowModal()));
                }
                else if (cboClssType.SelectedValue.ToString().Equals("LOGI"))  // 구분이  물류 경우 
                {
                    COM001_401_LOGI_DAILY_REMARKS popupLogiDailyRemarks = new COM001_401_LOGI_DAILY_REMARKS { FrameOperation = FrameOperation };
                
                    object[] parameters = new object[7];
                    parameters[0] = "INS";                                         // Save Mode : INS :신규, UPD : 수정, CHK:확인
                    parameters[1] = "";                                            // Save Mode = INS 인 경우 INPUTSEQNO = "";

                    if (cboArea.SelectedValue.ToString().Equals(""))
                    {
                        parameters[2] = "SELECT";              // 동
                    }
                    else
                    {
                        parameters[2] = cboArea.SelectedValue.ToString();              // 동
                    }

                    if (cboEquipmentSegment.SelectedValue.ToString().Equals(""))
                    {
                        parameters[3] = "SELECT";  // 라인
                    }
                    else
                    {
                        parameters[3] = cboEquipmentSegment.SelectedValue.ToString();  // 라인
                    }

                    if (cboProcess.SelectedValue.ToString().Equals(""))
                    {
                        parameters[4] = "SELECT";          // 공정
                    }
                    else
                    {
                        parameters[4] = cboProcess.SelectedValue.ToString();           // 공정
                    }

                    if (cboEquipment.SelectedValue.ToString().Equals(""))
                    {
                        parameters[5] = "SELECT";          // 설비
                    }
                    else
                    {
                        parameters[5] = cboEquipment.SelectedValue.ToString();         // 설비
                    }

                    parameters[6] = cboClssType.SelectedValue.ToString();          // 분류유형
                    C1WindowExtension.SetParameters(popupLogiDailyRemarks, parameters);

                    popupLogiDailyRemarks.Closed += new EventHandler(popupLogiDailyRemarks_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupLogiDailyRemarks.ShowModal()));
                }
                else // 구분이  설비 경우(EQPT)  // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
                {
                    COM001_401_EQPT_DAILY_REMARKS popupEqptDailyRemarks = new COM001_401_EQPT_DAILY_REMARKS { FrameOperation = FrameOperation };

                    object[] parameters = new object[7];
                    parameters[0] = "INS";                                         // Save Mode : INS :신규, UPD : 수정, CHK:확인
                    parameters[1] = "";                                            // Save Mode = INS 인 경우 INPUTSEQNO = "";

                    if (cboArea.SelectedValue.ToString().Equals(""))
                    {
                        parameters[2] = "SELECT";              // 동
                    }
                    else
                    {
                        parameters[2] = cboArea.SelectedValue.ToString();              // 동
                    }

                    if (cboEquipmentSegment.SelectedValue.ToString().Equals(""))
                    {
                        parameters[3] = "SELECT";  // 라인
                    }
                    else
                    {
                        parameters[3] = cboEquipmentSegment.SelectedValue.ToString();  // 라인
                    }

                    if (cboProcess.SelectedValue.ToString().Equals(""))
                    {
                        parameters[4] = "SELECT";          // 공정
                    }
                    else
                    {
                        parameters[4] = cboProcess.SelectedValue.ToString();           // 공정
                    }

                    if (cboEquipment.SelectedValue.ToString().Equals(""))
                    {
                        parameters[5] = "SELECT";          // 설비
                    }
                    else
                    {
                        parameters[5] = cboEquipment.SelectedValue.ToString();         // 설비
                    }

                    parameters[6] = cboClssType.SelectedValue.ToString();          // 분류유형
                    C1WindowExtension.SetParameters(popupEqptDailyRemarks, parameters);

                    popupEqptDailyRemarks.Closed += new EventHandler(popupEqptDailyRemarks_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupEqptDailyRemarks.ShowModal()));
                }
            }
            else
            {
                Util.MessageInfo("SFU4149");    
            }

            return;
        }
        private void popupProdDailyRemarks_Closed(object sender, EventArgs e)
        {
            COM001_401_PROD_DAILY_REMARKS popup = sender as COM001_401_PROD_DAILY_REMARKS;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetDailyRemarksList();
            }             
        }

       private void popupLogiDailyRemarks_Closed(object sender, EventArgs e)
       {
           COM001_401_LOGI_DAILY_REMARKS popup = sender as COM001_401_LOGI_DAILY_REMARKS;
           if (popup != null && popup.DialogResult == MessageBoxResult.OK)
           {
               GetDailyRemarksList();
           }
      
       }

       // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
       private void popupEqptDailyRemarks_Closed(object sender, EventArgs e)
       {
           COM001_401_EQPT_DAILY_REMARKS popup = sender as COM001_401_EQPT_DAILY_REMARKS;
           if (popup != null && popup.DialogResult == MessageBoxResult.OK)
           {
               GetDailyRemarksList();
           }

       }

        // [구분]의 값에 따라 Grid의 Column 변경
        private void cboClssType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboClssType.Items.Count > 0 && cboClssType.SelectedValue != null && !cboClssType.SelectedValue.Equals("SELECT"))
            {
                Util.gridClear(dgPrdtDailyRemarks);
                SetGridColumnViewConfig(cboClssType.SelectedValue.ToString());
                if (InitLoadYn.Equals("Y"))
                {
                    InitLoadYn = "N";
                }
                else
                {
                    GetDailyRemarksList();
                }
            }
        } 

       // [발생일자] - 조회 조건
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
         

        #region Grid Column Button Event
        private void btnGrid_Click(object sender, RoutedEventArgs e)
        {
            Button btnPrint = sender as Button;
            if (btnPrint != null)
            {
                DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                if (string.Equals(btnPrint.Name, "btnDailyRemarksMod"))  // 수정버튼
                {
                    if (!(Util.NVC(dataRow.Row["INSUSER"]).Equals(LoginInfo.USERID)))
                    {
                        Util.MessageInfo("SFU4312");    // 작성자만 수정 가능합니다.
                        return;
                    }

                    if (!(Util.NVC(dataRow.Row["CHKUSER"]).Equals("")))
                    {
                        Util.MessageInfo("SFU9212");    // 확인 처리 되었습니다. 수정 불가
                        return;
                    }

                    if (Util.NVC(dataRow.Row["CLSS_TYPE"]).ToString().Equals("PROD"))  // 구분이  생산일 경우
                    {

                        COM001_401_PROD_DAILY_REMARKS popupProdDailyRemarks = new COM001_401_PROD_DAILY_REMARKS { FrameOperation = FrameOperation };

                        object[] parameters = new object[7];
                        parameters[0] = "UPD";                                              // Save Mode : INS :신규, UPD : 수정, CHK:확인
                        parameters[1] = Util.NVC(dataRow.Row["INPUTSEQNO"]).ToString();     // Save Mode = INS 인 경우 INPUTSEQNO = "";
                        parameters[2] = Util.NVC(dataRow.Row["AREAID"]).ToString();         // 동
                        parameters[3] = Util.NVC(dataRow.Row["EQSGID"]).ToString();         // 라인
                        parameters[4] = Util.NVC(dataRow.Row["PROCID"]).ToString();         // 공정
                        parameters[5] = Util.NVC(dataRow.Row["EQPTID"]).ToString(); ;       // 설비
                        parameters[6] = Util.NVC(dataRow.Row["CLSS_TYPE"]).ToString();      // 분류유형 
                        C1WindowExtension.SetParameters(popupProdDailyRemarks, parameters);

                        popupProdDailyRemarks.Closed += new EventHandler(popupProdDailyRemarks_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => popupProdDailyRemarks.ShowModal()));
                    }
                    else if (Util.NVC(dataRow.Row["CLSS_TYPE"]).ToString().Equals("LOGI")) // 구분이  물류 경우 
                    {
                        COM001_401_LOGI_DAILY_REMARKS popupLogiDailyRemarks = new COM001_401_LOGI_DAILY_REMARKS { FrameOperation = FrameOperation };
                    
                        object[] parameters = new object[7];
                        parameters[0] = "UPD";                                            // Save Mode : INS :신규, UPD : 수정, CHK:확인
                        parameters[1] = Util.NVC(dataRow.Row["INPUTSEQNO"]).ToString();   // Save Mode = INS 인 경우 INPUTSEQNO = "";
                        parameters[2] = Util.NVC(dataRow.Row["AREAID"]).ToString();       // 동
                        parameters[3] = Util.NVC(dataRow.Row["EQSGID"]).ToString();       // 라인
                        parameters[4] = Util.NVC(dataRow.Row["PROCID"]).ToString();       // 공정
                        parameters[5] = Util.NVC(dataRow.Row["EQPTID"]).ToString();       // 설비
                        parameters[6] = Util.NVC(dataRow.Row["CLSS_TYPE"]).ToString();    // 분류유형 
                        C1WindowExtension.SetParameters(popupLogiDailyRemarks, parameters);

                        popupLogiDailyRemarks.Closed += new EventHandler(popupLogiDailyRemarks_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => popupLogiDailyRemarks.ShowModal()));
                    }
                    else // 구분이 설비 경우 (EQPT)  // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
                    {
                        COM001_401_EQPT_DAILY_REMARKS popupEqptDailyRemarks = new COM001_401_EQPT_DAILY_REMARKS { FrameOperation = FrameOperation };

                        object[] parameters = new object[7];
                        parameters[0] = "UPD";                                            // Save Mode : INS :신규, UPD : 수정, CHK:확인
                        parameters[1] = Util.NVC(dataRow.Row["INPUTSEQNO"]).ToString();   // Save Mode = INS 인 경우 INPUTSEQNO = "";
                        parameters[2] = Util.NVC(dataRow.Row["AREAID"]).ToString();       // 동
                        parameters[3] = Util.NVC(dataRow.Row["EQSGID"]).ToString();       // 라인
                        parameters[4] = Util.NVC(dataRow.Row["PROCID"]).ToString();       // 공정
                        parameters[5] = Util.NVC(dataRow.Row["EQPTID"]).ToString();       // 설비
                        parameters[6] = Util.NVC(dataRow.Row["CLSS_TYPE"]).ToString();    // 분류유형 
                        C1WindowExtension.SetParameters(popupEqptDailyRemarks, parameters);

                        popupEqptDailyRemarks.Closed += new EventHandler(popupEqptDailyRemarks_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => popupEqptDailyRemarks.ShowModal()));
                    }
                }
                else if (string.Equals(btnPrint.Name, "btnDailyRemarksCheck")) // 확인버튼
                {

                    string sCheck = string.Empty;

                    sCheck = GetCheckProcessChk(Util.NVC(dataRow.Row["INPUTSEQNO"]).ToString());
                    if (sCheck.Equals("E"))
                    {
                        return;
                    }
                    if (sCheck.Equals("E0"))
                    {
                        Util.MessageInfo("SFU1498"); //  데이터가 없습니다.
                        return;
                    }
                     
                    if (sCheck.Equals("E1"))
                    {
                        Util.MessageInfo("SFU9216");  //  사용유무가 미사용(N) 인 경우에는 확인 처리 할 수 없습니다.
                        return;
                    }

                    if (sCheck.Equals("E2"))
                    {
                        Util.MessageInfo("SFU9213");  // 확인 처리 되었습니다. 재확인 불가 
                        return;
                    }
                  
                     Util.MessageConfirm("SFU2039", (result) =>  // 확인처리 하시겠습니까?
                     {
                         if (result == MessageBoxResult.OK)
                         {
                             SetCheckUser(Util.NVC(dataRow.Row["INPUTSEQNO"]).ToString());
                         }
                     });
                }
            }
        }
        #endregion Grid Column Button Event

        #endregion Event

        #region Mehod

        // Grid Clumn Setting View Y/N
        private void SetGridColumnViewConfig(string strMode)
        {             
            Util.gridClear(dgPrdtDailyRemarks);

            // 공통
            dgPrdtDailyRemarks.Columns["INPUTSEQNO"].Visibility = Visibility.Collapsed;         // 투입일련번호      
            dgPrdtDailyRemarks.Columns["ACTDTTM"].Visibility = Visibility.Visible;              // 발생일시          
            dgPrdtDailyRemarks.Columns["USE_FLAG"].Visibility = Visibility.Visible;             // USE_FLAG   
            dgPrdtDailyRemarks.Columns["AREAID"].Visibility = Visibility.Collapsed;             // AREAID : Collapsed           
            dgPrdtDailyRemarks.Columns["AREANAME"].Visibility = Visibility.Visible;             // 동                
            dgPrdtDailyRemarks.Columns["EQSGID"].Visibility = Visibility.Collapsed;             // EQSGID : Collapsed    
            dgPrdtDailyRemarks.Columns["EQSGNAME"].Visibility = Visibility.Visible;             // 라인              
            dgPrdtDailyRemarks.Columns["PROCID"].Visibility = Visibility.Collapsed;             // PROCID  : Collapsed            
            dgPrdtDailyRemarks.Columns["PROCNAME"].Visibility = Visibility.Visible;             // 공정              
            dgPrdtDailyRemarks.Columns["EQPTID"].Visibility = Visibility.Collapsed;             // EQPTID  : Collapsed            
            dgPrdtDailyRemarks.Columns["EQPTNAME"].Visibility = Visibility.Visible;             // 설비              
            dgPrdtDailyRemarks.Columns["CLSS_TYPE"].Visibility = Visibility.Collapsed;          // 분류유형 : Collapsed           
            dgPrdtDailyRemarks.Columns["CLSS_TYPE_NM"].Visibility = Visibility.Visible;         // 구분       
                   
            // 구분에 따라 생산, 물류, 설비 다르게 적용
            // dgPrdtDailyRemarks.Columns["PRODID"].Visibility = Visibility.Visible;               // 생산 :현재생산모델  , 물류 : 모델     
            // dgPrdtDailyRemarks.Columns["PRE_PRODID"].Visibility = Visibility.Visible;           // 변경모델    
            // dgPrdtDailyRemarks.Columns["MDL_CHG_FLAG"].Visibility = Visibility.Collapsed;       // 모델변경여부           
            // dgPrdtDailyRemarks.Columns["ELTR_POLAR_CODE"].Visibility = Visibility.Collapsed;    // ELTR_POLAR_CODE  : Collapsed   
            // dgPrdtDailyRemarks.Columns["ELTR_POLAR_NM"].Visibility = Visibility.Visible;        // 극성              
            // dgPrdtDailyRemarks.Columns["VER_CODE"].Visibility = Visibility.Visible;             // 버전              
            // dgPrdtDailyRemarks.Columns["SHIPTO_ID"].Visibility = Visibility.Collapsed;          // SHIPTO_ID   : Collapsed        
            // dgPrdtDailyRemarks.Columns["SHIPTO_NAME"].Visibility = Visibility.Visible;          // 출하처            
            // dgPrdtDailyRemarks.Columns["SHIP_FLAG"].Visibility = Visibility.Collapsed;          // 출하여부    : Collapsed     
            // dgPrdtDailyRemarks.Columns["EQPT_REMARKS_COL"].Visibility = Visibility.Visible;     // 설비특이사항      
            // dgPrdtDailyRemarks.Columns["PROD_CATN_COL"].Visibility = Visibility.Visible;        // 생산주의사항      
            // dgPrdtDailyRemarks.Columns["REMARKS_COL"].Visibility = Visibility.Visible;          // 특이사항    
            //----------------------------------------------------------------------------------------------------     
            // 공통 
            dgPrdtDailyRemarks.Columns["INSUSER"].Visibility = Visibility.Collapsed;            // INSUSER           
            dgPrdtDailyRemarks.Columns["INSUSER_NM"].Visibility = Visibility.Visible;           // 등록자            
            dgPrdtDailyRemarks.Columns["INSDTTM"].Visibility = Visibility.Visible;              // 등록일시          
            dgPrdtDailyRemarks.Columns["CHKUSER"].Visibility = Visibility.Collapsed;            // 확인자ID          
            dgPrdtDailyRemarks.Columns["CHKUSER_NM"].Visibility = Visibility.Visible;           // 확인자            
            dgPrdtDailyRemarks.Columns["CHKDTTM"].Visibility = Visibility.Visible;              // 확인일시          

            // 물류 인 경우 

            if (strMode.Equals("PROD"))    // 구분 : 생산
            {
                dgPrdtDailyRemarks.Columns["PRODID"].Visibility = Visibility.Visible;               // 생산 : 현재생산모델  , 물류 : 모델      
                dgPrdtDailyRemarks.Columns["PRE_PRODID"].Visibility = Visibility.Visible;           // 변경모델          
                dgPrdtDailyRemarks.Columns["MDL_CHG_FLAG"].Visibility = Visibility.Visible;         // 모델변경여부 : Collapsed            
                dgPrdtDailyRemarks.Columns["ELTR_POLAR_CODE"].Visibility = Visibility.Collapsed;    // ELTR_POLAR_CODE : Collapsed    
                dgPrdtDailyRemarks.Columns["ELTR_POLAR_NM"].Visibility = Visibility.Collapsed;      // 극성              
                dgPrdtDailyRemarks.Columns["VER_CODE"].Visibility = Visibility.Collapsed;           // 버전              
                dgPrdtDailyRemarks.Columns["SHIPTO_ID"].Visibility = Visibility.Collapsed;          // SHIPTO_ID : Collapsed          
                dgPrdtDailyRemarks.Columns["SHIPTO_NAME"].Visibility = Visibility.Collapsed;        // 출하처            
                dgPrdtDailyRemarks.Columns["SHIP_FLAG"].Visibility = Visibility.Collapsed;          // 출하여부 : Collapsed           
                dgPrdtDailyRemarks.Columns["EQPT_REMARKS_COL"].Visibility = Visibility.Visible;     // 설비특이사항      
                dgPrdtDailyRemarks.Columns["PROD_CATN_COL"].Visibility = Visibility.Visible;        // 생산주의사항      
                dgPrdtDailyRemarks.Columns["REMARKS_COL"].Visibility = Visibility.Visible;          // 특이사항   

                dgPrdtDailyRemarks.Columns["PRODID"].Header = ObjectDic.Instance.GetObjectName("현재생산모델"); //현재생산모델
            }
            else if (strMode.Equals("LOGI"))  // 물류 인 경우 
            {
                dgPrdtDailyRemarks.Columns["PRODID"].Visibility = Visibility.Visible;                   // 생산 :현재생산모델  , 물류 : 모델
                dgPrdtDailyRemarks.Columns["PRE_PRODID"].Visibility = Visibility.Collapsed;             // 변경모델          
                dgPrdtDailyRemarks.Columns["MDL_CHG_FLAG"].Visibility = Visibility.Collapsed;           // 모델변경여부 : Collapsed            
                dgPrdtDailyRemarks.Columns["ELTR_POLAR_CODE"].Visibility = Visibility.Collapsed;        // ELTR_POLAR_CODE : Collapsed  
                dgPrdtDailyRemarks.Columns["ELTR_POLAR_NM"].Visibility = Visibility.Visible;            // 극성              
                dgPrdtDailyRemarks.Columns["VER_CODE"].Visibility = Visibility.Visible;                 // 버전              
                dgPrdtDailyRemarks.Columns["SHIPTO_ID"].Visibility = Visibility.Collapsed;              // SHIPTO_ID  : Collapsed      
                dgPrdtDailyRemarks.Columns["SHIPTO_NAME"].Visibility = Visibility.Visible;              // 출하처            
                dgPrdtDailyRemarks.Columns["SHIP_FLAG"].Visibility = Visibility.Visible;                // 출하여부 : Collapsed                    
                dgPrdtDailyRemarks.Columns["EQPT_REMARKS_COL"].Visibility = Visibility.Collapsed;       // 설비특이사항      
                dgPrdtDailyRemarks.Columns["PROD_CATN_COL"].Visibility = Visibility.Collapsed;          // 생산주의사항      
                dgPrdtDailyRemarks.Columns["REMARKS_COL"].Visibility = Visibility.Visible;              // 특이사항   

                dgPrdtDailyRemarks.Columns["PRODID"].Header = ObjectDic.Instance.GetObjectName("모델"); //모델
            }

            else  // 구분 : 설비(EQPT)  // [E20240513-000346] [ESNA]전극 생산일별 특이사항 화면 개선 건
            {
                dgPrdtDailyRemarks.Columns["PRODID"].Visibility = Visibility.Collapsed;               // 생산 : 현재생산모델  , 물류 : 모델      
                dgPrdtDailyRemarks.Columns["PRE_PRODID"].Visibility = Visibility.Collapsed;           // 변경모델          
                dgPrdtDailyRemarks.Columns["MDL_CHG_FLAG"].Visibility = Visibility.Collapsed;         // 모델변경여부 : Collapsed            
                dgPrdtDailyRemarks.Columns["ELTR_POLAR_CODE"].Visibility = Visibility.Collapsed;      // ELTR_POLAR_CODE : Collapsed    
                dgPrdtDailyRemarks.Columns["ELTR_POLAR_NM"].Visibility = Visibility.Collapsed;        // 극성              
                dgPrdtDailyRemarks.Columns["VER_CODE"].Visibility = Visibility.Collapsed;             // 버전              
                dgPrdtDailyRemarks.Columns["SHIPTO_ID"].Visibility = Visibility.Collapsed;            // SHIPTO_ID : Collapsed          
                dgPrdtDailyRemarks.Columns["SHIPTO_NAME"].Visibility = Visibility.Collapsed;          // 출하처            
                dgPrdtDailyRemarks.Columns["SHIP_FLAG"].Visibility = Visibility.Collapsed;            // 출하여부 : Collapsed           
                dgPrdtDailyRemarks.Columns["EQPT_REMARKS_COL"].Visibility = Visibility.Collapsed;     // 설비특이사항      
                dgPrdtDailyRemarks.Columns["PROD_CATN_COL"].Visibility = Visibility.Collapsed;        // 생산주의사항      
                dgPrdtDailyRemarks.Columns["REMARKS_COL"].Visibility = Visibility.Visible;            // 특이사항    
            }
        }

        private void EQPT_REMARKS_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox txtEqptRemarks = sender as TextBox;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                IList<FrameworkElement> ilist = txtEqptRemarks.GetAllParents();

                foreach (var item in ilist)
                {
                    DataGridRowPresenter presenter = item as DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                    }
                }
                dgPrdtDailyRemarks.SelectedItem = row.DataItem;

                if (txtEqptRemarks != null)
                {
                    txtEqptRemarks.IsReadOnly = true;
                    txtEqptRemarks.SelectionStart = txtEqptRemarks.Text.Length;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PROD_CATN_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox txtProdCatn = sender as TextBox;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                IList<FrameworkElement> ilist = txtProdCatn.GetAllParents();

                foreach (var item in ilist)
                {
                    DataGridRowPresenter presenter = item as DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                    }
                }
                dgPrdtDailyRemarks.SelectedItem = row.DataItem;

                if (txtProdCatn != null)
                {
                    txtProdCatn.IsReadOnly = true;
                    txtProdCatn.SelectionStart = txtProdCatn.Text.Length; 
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void REMARKS_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox txtProdCatn = sender as TextBox;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                IList<FrameworkElement> ilist = txtProdCatn.GetAllParents();

                foreach (var item in ilist)
                {
                    DataGridRowPresenter presenter = item as DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                    }
                }
                dgPrdtDailyRemarks.SelectedItem = row.DataItem;

                if (txtProdCatn != null)
                {
                    txtProdCatn.IsReadOnly = true;
                    txtProdCatn.SelectionStart = txtProdCatn.Text.Length;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [BizCall]

        // [### 조회 ###]
        public void GetDailyRemarksList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 30)
                {
                    Util.MessageValidation("SFU2042", "30");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("CLSS_TYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                // 동을 선택하세요.
                // dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                // if (dr["AREAID"].Equals("")) return;
                //
                //  // 라인을선택하세요
                //  dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                //  if (dr["EQSGID"].Equals("")) return;
                //
                //  // 공정을선택하세요.
                //  dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                //  if (dr["PROCID"].Equals("")) return;
                //
                //  // 설비를 선택하세요.
                //  dr["EQPTID"] = Util.GetCondition(cboEquipment, MessageDic.Instance.GetMessage("SFU1673"));
                //  if (dr["EQPTID"].Equals("")) return;

                // 동을 선택하세요.
                dr["AREAID"] = Util.GetCondition(cboArea) == "" ? (object)DBNull.Value : cboArea.SelectedValue.ToString(); 

                // 라인을선택하세요
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment) == "" ? (object)DBNull.Value : cboEquipmentSegment.SelectedValue.ToString(); 

                // 공정을선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess) == "" ? (object)DBNull.Value : cboProcess.SelectedValue.ToString(); 

                // 설비를 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment) == "" ? (object)DBNull.Value : cboEquipment.SelectedValue.ToString();

                // 발생일자
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);

                // 사용유무.
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag) == "" ? (object)DBNull.Value : cboUseFlag.SelectedValue.ToString(); 

                // 구분.
                dr["CLSS_TYPE"] = Util.GetCondition(cboClssType, MessageDic.Instance.GetMessage("SFU4149"));
                if (dr["CLSS_TYPE"].Equals("")) return;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_DAILY_REMARKS_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgPrdtDailyRemarks, dtRslt, FrameOperation, true);

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

        private string GetCheckProcessChk(string sInputSeqno)
        {
            string sChkResult = "Y";

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("INPUTSEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INPUTSEQNO"] = sInputSeqno;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRDT_DAILY_REMARKS_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {

                    if (Util.NVC(dtResult.Rows[0]["USE_FLAG"].ToString()).Equals("N"))
                    {
                        sChkResult = "E1";
                        return sChkResult;
                    }

                    if (Util.NVC(dtResult.Rows[0]["CHKUSER"].ToString()).Equals(""))
                    {
                        sChkResult = "Y";
                        return sChkResult;
                    }
                    else
                    {
                        sChkResult = "E2";
                        return sChkResult;
                    }
                }
                else
                {
                    sChkResult = "E0";
                    return sChkResult;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                sChkResult = "E";
            }

            return sChkResult;
        }

        private void SetCheckUser(string sInputSeqno)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_UPD_TB_SFC_PRDT_DAILY_REMARKS";             

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("INPUTSEQNO", typeof(string)); 
                inDataTable.Columns.Add("CHKUSER", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["INPUTSEQNO"] = sInputSeqno; 
                dr["CHKUSER"] = LoginInfo.USERID;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException); 
                        }
                        Util.MessageInfo("SFU9214");    // 확인 처리 되었습니다. 
                        GetDailyRemarksList();
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion [BizCall]

        #region [Func]
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        
        #endregion

        #endregion Mehod

    }
}