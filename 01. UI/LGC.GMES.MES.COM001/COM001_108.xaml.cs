/*************************************************************************************
 Created Date : 2017.09.15
      Creator : 최승혁s
   Decription : GMES  APD data 조회(패키지 주액량 / 포장 비전데이터) - CWA
--------------------------------------------------------------------------------------
 [Change History]
  2017.09.15  최승혁 : Initial Created.
  2019.12.09  이제섭 : CNB 포장기 비전 데이터 조회 추가
  2020.05.20  오화백 : 1) 포장 비전 데이터의 경우 최대 60000건만 조회되도록 함
                       2) 포장 비전 데이터의 경우 한번 조회 될 경우 20000만건씩 조회해서 데이터 테이블로 모아서 한번에 바인딩하도록 처리
  2020.07.24  오화백 : 1) 폴란드 1동/2동 포장 비전 데이터의 경우 최대 60000건만 조회되도록 함
                       2) 폴란드 1동/2동 포장 비전 데이터의 경우 한번 조회 될 경우 20000만건씩 조회해서 데이터 테이블로 모아서 한번에 바인딩하도록 처리
  2021.01.05  조영대 : CNB(A8,S4,AA) 이고 Packaging 일 경우 조회 분기
  2022.08.17  신광희 : UI에서 동 정보로 분기 호출하는 BizRule 부분을 BizRule에서 분기처리를 위해 생성 함(BR_PRD_SEL_APD_DATA_FOR_PACKAGING->AREAID 컬럼 추가)
  2022.12.01  이제섭 : ESWA 4동 포장 Vision 데이터 조회 추가
  2023.03.24  권혜정 : ESNB 2동 컬럼명 수정 및 PKG LOT ID 검색 조건 추가
  2023.05.10  최도훈 : ESHM 1동 포장 Vision 데이터 조회 추가
  2024.01.03  이제섭 : ESWA 3동 조립 -> 활성화로 변경 
  2025.02.13  이제섭 : ESMI 2동 EOL Sealing 검사 데이터 조회 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_108.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_108 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        private DataTable _dtInfo = null;

        public COM001_108()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            DateTime dtNowTime = System.DateTime.Now;
            if (dtpDateFrom != null)
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-1);
            if (tmedtFrom != null)
                tmedtFrom.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);

            if (dtpDateTo != null)
                dtpDateTo.SelectedDateTime = dtNowTime;
            if (tmedtTo != null)
                tmedtTo.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);

            // ESNB2동 PKG LOT ID 검색 추가
            if (LoginInfo.SYSID != "GMES-F-N2")
            {
                lblPKGLOTIDTXT.Visibility = Visibility.Collapsed;
                txtPKGLOTID.Visibility = Visibility.Collapsed;
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };

            // 활성화 시스템 일 때,
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
            {
                C1ComboBox[] cboLineChild = { cboProcess, cboEquipment };

                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "LINE_FCS");

                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cbProcessChild = { cboEquipment };

                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cbProcessChild, cbParent: cbProcessParent);

                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };

                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            }
            // 조립 시스템 일 때,
            else
            {
                C1ComboBox[] cboLineChild = { cboEquipment };

                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cbProcessChild = { cboEquipment };

                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cbProcessChild, cbParent: cbProcessParent);

                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };

                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            }
        }
        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
            InitializeControls();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSearch);

            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                HiddenLoadingIndicator();
                return;
            }

            Util.gridClear(dgAPDDATA_PACKAGING);
            Util.gridClear(dgAPDDATA_VISION);
            Util.gridClear(dgAPDDATA_VISION_CWA3);
            Util.gridClear(dgAPDDATA_VISION_CNB);
            Util.gridClear(dgAPDDATA_VISION_CWA3_NM);
            Util.gridClear(dgAPDDATA_VISION_CNB2);
            Util.gridClear(dgAPDDATA_VISION_UC1);
            Util.gridClear(dgAPDDATA_VISION_ESHM1);
            Util.gridClear(dgAPDDATA_VISION_CWA4);
            GetAPDdataList();
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                if (dgAPDDATA_PACKAGING.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_PACKAGING, "PACKAGING" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else if (dgAPDDATA_VISION.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_VISION, "VISION" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else if (dgAPDDATA_VISION_CNB.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_VISION_CNB, "VISION" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else if (dgAPDDATA_VISION_CWA3.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_VISION_CWA3, "VISION" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else if (dgAPDDATA_VISION_CWA3_NM.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_VISION_CWA3_NM, "VISION" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else if (dgAPDDATA_VISION_CNB2.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_VISION_CNB2, "VISION" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else if (dgAPDDATA_VISION_UC1.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_VISION_UC1, "VISION" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else if (dgAPDDATA_VISION_ESHM1.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_VISION_ESHM1, "VISION" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else if (dgAPDDATA_VISION_CWA4.Visibility == Visibility.Visible)
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgAPDDATA_VISION_CWA4, "VISION" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtLOTID.Text = txtLOTID.Text.Trim();
                if (string.IsNullOrEmpty(txtLOTID.Text))
                {
                    return;
                }
                GetAPDdataList();
            }
        }

        private void txtCELLID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                txtCELLID.Text = txtCELLID.Text.Trim();
                if (string.IsNullOrEmpty(txtCELLID.Text))
                {
                    return;
                }
                GetAPDdataList();

            }

        }

        private void txtCELLID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                string sTemp = string.Empty;
                try
                {
                    //ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 1000)
                    {
                        Util.MessageValidation("SFU4243");   //최대 1000개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {

                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                            break;
                        else
                        {
                            sTemp += sPasteStrings[i] + ',';
                        }
                        if (i == 0)
                        {
                            txtCELLID.Text = sPasteStrings[i];
                        }

                    }

                    GetAPDdataList(sTemp);
                    System.Windows.Forms.Application.DoEvents();


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    // HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        private void txtPKGLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                txtPKGLOTID.Text = txtPKGLOTID.Text.Trim();
                if (string.IsNullOrEmpty(txtPKGLOTID.Text))
                {
                    return;
                }
                GetAPDdataList();

            }

        }
        

        private void cboEquipment_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            setGridVisibility();

            Util.gridClear(dgAPDDATA_PACKAGING);
            Util.gridClear(dgAPDDATA_VISION);
            Util.gridClear(dgAPDDATA_VISION_CWA3);
            Util.gridClear(dgAPDDATA_VISION_CNB);
            Util.gridClear(dgAPDDATA_VISION_CNB2);
            Util.gridClear(dgAPDDATA_VISION_UC1);
            Util.gridClear(dgAPDDATA_VISION_ESHM1);
            Util.gridClear(dgAPDDATA_VISION_CWA4);
        }



        private void setGridVisibility()
        {
            string sEqptGrDetlType = GetEqptGrDetlType();

            if (cboEquipment.SelectedValue == null)
                return;

            if (sEqptGrDetlType != "EOL_DSF") // 선택한 설비가 PKG/Packer인 경우
            {
                // EOL Sealing Grid 숨김처리
                dgAPDDATA_EOL_SEALING_ESMI2.Visibility = Visibility.Collapsed;

                if (cboEquipment.SelectedValue.ToString().IndexOf("PKG") > 0)
                {
                    dgAPDDATA_PACKAGING.Visibility = Visibility.Visible;
                    dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                }
                else if (cboEquipment.SelectedValue.ToString().IndexOf("WRP") > 0 && LoginInfo.SYSID != "GMES-F-NB" && LoginInfo.SYSID != "GMES-F-W3" && LoginInfo.SYSID != "GMES-F-N2" && LoginInfo.SYSID != "GMES-F-OH" && LoginInfo.SYSID != "GMES-F-W4") // CNB,CWA3 포장기가 아니면
                {

                    //2020/10.21 오화백 25라인일 경우 조립3동이랑 동일
                    if (cboEquipmentSegment.SelectedValue.ToString() == "A6J25")
                    {
                        dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Visible;
                        dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;

                    }
                    else
                    {
                        dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION.Visibility = Visibility.Visible;
                        dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                    }


                }

                else if (cboEquipment.SelectedValue.ToString().IndexOf("EOL") > 0 && LoginInfo.SYSID == "GMES-F-W1") // CNB,CWA3 포장기가 아니면
                {

                    //2020/10.21 오화백 25라인일 경우 조립3동이랑 동일
                    if (cboEquipmentSegment.SelectedValue.ToString() == "A6J25")
                    {
                        dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Visible;
                        dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;

                    }
                    else
                    {
                        dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION.Visibility = Visibility.Visible;
                        dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                        dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                    }


                }
                else if (cboEquipment.SelectedValue.ToString().IndexOf("WRP") > 0 && LoginInfo.SYSID == "GMES-F-NB") // CNB 포장기
                {
                    dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB.Visibility = Visibility.Visible;
                    dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                }
                else if (cboEquipment.SelectedValue.ToString().IndexOf("EOL") > 0 && LoginInfo.SYSID == "GMES-F-W3") // CW3 포장기
                {
                    dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Visible;
                    dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                }
                else if (cboEquipment.SelectedValue.ToString().IndexOf("WRP") > 0 && LoginInfo.SYSID == "GMES-F-N2") // ESNB 활성화 2동
                {
                    dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB2.Visibility = Visibility.Visible;
                    dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                }
                else if (cboEquipment.SelectedValue.ToString().IndexOf("EOL") > 0 && LoginInfo.SYSID == "GMES-F-OH") // Ultium Cells 1동
                {
                    dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_UC1.Visibility = Visibility.Visible;
                    dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                }
                else if (cboEquipment.SelectedValue.ToString().IndexOf("EOL") > 0 && LoginInfo.SYSID == "GMES-F-JB") // ESHM 1동
                {
                    dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Visible;
                    dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                }
                else if (cboEquipment.SelectedValue.ToString().IndexOf("EOL") > 0 && LoginInfo.SYSID == "GMES-F-W4") // ESWA 4동
                {
                    dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA4.Visibility = Visibility.Visible;
                }
                else
                {
                    dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                    dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;
                }
            }
            else // 선택한 설비가 PKG/Packer가 아닌 경우
            {
                // PKG/Packer Grid 숨김처리
                dgAPDDATA_PACKAGING.Visibility = Visibility.Collapsed;
                dgAPDDATA_VISION.Visibility = Visibility.Collapsed;
                dgAPDDATA_VISION_CNB.Visibility = Visibility.Collapsed;
                dgAPDDATA_VISION_CWA3.Visibility = Visibility.Collapsed;
                dgAPDDATA_VISION_CWA3_NM.Visibility = Visibility.Collapsed;
                dgAPDDATA_VISION_CNB2.Visibility = Visibility.Collapsed;
                dgAPDDATA_VISION_UC1.Visibility = Visibility.Collapsed;
                dgAPDDATA_VISION_ESHM1.Visibility = Visibility.Collapsed;
                dgAPDDATA_VISION_CWA4.Visibility = Visibility.Collapsed;

                if (LoginInfo.SYSID == "GMES-F-ME") // ESMI 2동
                {
                    dgAPDDATA_EOL_SEALING_ESMI2.Visibility = Visibility.Visible;
                }

            }
        }

        private void dgAPDdata_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null)
            //    {
            //        return;
            //    }

            //    //Grid Data Binding 이용한 Background 색 변경
            //    if (e.Cell.Row.Type == DataGridRowType.Item)
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else
            //        {
            //            e.Cell.Presenter.Background = null;
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
            //        }
            //    }
            //}));
        }

        private void dgAPDdata_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Row.Type == DataGridRowType.Item)
            //        {
            //            e.Cell.Presenter.Background = null;
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
            //        }
            //    }
            //}));
        }

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
        #endregion

        #region Mehod        

        #region [BizCall]
        private void GetAPDdataList(string sTempCellID)
        {
            try
            {


                ShowLoadingIndicator();


                DateTime dtFromTime;
                DateTime dtToTime;
                TimeSpan spn;
                if (tmedtFrom.Value.HasValue)
                {
                    spn = ((TimeSpan)tmedtFrom.Value);
                    dtFromTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtFromTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);
                }

                if (tmedtTo.Value.HasValue)
                {
                    spn = ((TimeSpan)tmedtTo.Value);
                    dtToTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtToTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
                }

                DataTable dt = new DataTable();

                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("CELLID", typeof(string));
                dt.Columns.Add("dtFROM", typeof(DateTime));
                dt.Columns.Add("dtTO", typeof(DateTime));
                dt.Columns.Add("ROW_NUM_FR", typeof(string));
                dt.Columns.Add("ROW_NUM_TO", typeof(string));
                dt.Columns.Add("PKGLOTID", typeof(string));

                DataRow newRow = dt.NewRow();


                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = txtLOTID.Text.Trim() == "" ? null : txtLOTID.Text.Trim();
                newRow["CELLID"] = sTempCellID.Trim() == "" ? null : sTempCellID.Trim();
                newRow["dtFROM"] = dtFromTime;
                newRow["dtTO"] = dtToTime;
                newRow["PKGLOTID"] = txtPKGLOTID.Text.Trim() == "" ? null : txtPKGLOTID.Text.Trim();


                if (!string.IsNullOrEmpty(txtLOTID.Text.Trim()) || !string.IsNullOrEmpty(txtCELLID.Text.Trim()))
                {
                    newRow["dtFROM"] = DBNull.Value;
                    newRow["dtTO"] = DBNull.Value;
                }

                dt.Rows.Add(newRow);
                try
                {
                    if (dgAPDDATA_PACKAGING.Visibility == Visibility.Visible)
                    {
                        const string bizRuleName = "BR_PRD_SEL_APD_DATA_FOR_PACKAGING";

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

                        if (dgAPDDATA_PACKAGING.GetRowCount() == 0)
                        {
                            Util.GridSetData(dgAPDDATA_PACKAGING, dtResult, FrameOperation);
                        }
                        else
                        {
                            DataTable dtInfo = DataTableConverter.Convert(dgAPDDATA_PACKAGING.ItemsSource);

                            // PrimaryKey 셋팅(Merge할때 기본키값을 설정하지 않으면 테이블 Merge시 중복된 데이터가 들어갈수 있습니다.

                            dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["CELLID"] };
                            dtInfo.Merge(dtResult, true);

                            Util.GridSetData(dgAPDDATA_PACKAGING, dtInfo, FrameOperation);
                        }
                    }
                    else if (dgAPDDATA_VISION.Visibility == Visibility.Visible)
                    {

                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;



                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION, _dtInfo, FrameOperation);
                        _dtInfo = null;


                        //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION", "RQSTDT", "RSLTDT", dt);


                        //if (dgAPDDATA_VISION.GetRowCount() == 0)
                        //{
                        //    Util.GridSetData(dgAPDDATA_VISION, dtResult, FrameOperation);
                        //}
                        //else
                        //{
                        //    DataTable dtInfo = DataTableConverter.Convert(dgAPDDATA_VISION.ItemsSource);

                        //    dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["CELLID"] };
                        //    dtInfo.Merge(dtResult, true);

                        //    Util.GridSetData(dgAPDDATA_VISION, dtInfo, FrameOperation);
                        //}
                    }
                    else if (dgAPDDATA_VISION_CNB.Visibility == Visibility.Visible)
                    {
                        //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CNB", "RQSTDT", "RSLTDT", dt);


                        //    if (dgAPDDATA_VISION_CNB.GetRowCount() == 0)
                        //    {
                        //        Util.GridSetData(dgAPDDATA_VISION_CNB, dtResult, FrameOperation);
                        //    }
                        //    else
                        //    {
                        //        DataTable dtInfo = DataTableConverter.Convert(dgAPDDATA_VISION_CNB.ItemsSource);

                        //        dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["CELLID"] };
                        //        dtInfo.Merge(dtResult, true);

                        //        Util.GridSetData(dgAPDDATA_VISION_CNB, dtInfo, FrameOperation);
                        //    }

                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CNB_V01", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CWA3, _dtInfo, FrameOperation);
                        _dtInfo = null;

                    }
                    else if (dgAPDDATA_VISION_CWA3.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CWA3, _dtInfo, FrameOperation);
                        _dtInfo = null;

                        //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_1", "RQSTDT", "RSLTDT", dt);


                        //if (dgAPDDATA_VISION_CWA3.GetRowCount() == 0)
                        //{
                        //    Util.GridSetData(dgAPDDATA_VISION_CWA3, dtResult, FrameOperation);
                        //}
                        //else
                        //{
                        //    DataTable dtInfo = DataTableConverter.Convert(dgAPDDATA_VISION_CWA3.ItemsSource);

                        //    dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["CELLID"] };
                        //    dtInfo.Merge(dtResult, true);

                        //    Util.GridSetData(dgAPDDATA_VISION_CWA3, dtInfo, FrameOperation);
                        //}
                    }
                    else if (dgAPDDATA_VISION_CWA3_NM.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CWA3_NM, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }
                    else if (dgAPDDATA_VISION_CNB2.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        //DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_ESNB2_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;
                            
                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_ENSB2", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CNB2, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    else if (dgAPDDATA_VISION_UC1.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_UC1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_UC1, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    else if (dgAPDDATA_VISION_ESHM1.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_ESHM1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_ESHM1, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    else if (dgAPDDATA_VISION_CWA4.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_ESWA4", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CWA4, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }
                    // ESMI2 EOL Sealing 검사
                    else if (dgAPDDATA_EOL_SEALING_ESMI2.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_EOL_SEALING_ESMI2", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_EOL_SEALING_ESMI2, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    else
                    {
                        return;
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
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비의 EQPT_GR_DETL_TYPE_CODE 조회
        /// </summary>
        /// <returns></returns>
        private string GetEqptGrDetlType()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", inTable);


            // 설비 null일 때 방어로직 추가.
            return dtRslt.Rows.Count > 0 ? Util.NVC(dtRslt.Rows[0]["EQPT_GR_DETL_TYPE_CODE"]) : string.Empty;
        }


        private void GetAPDdataList()
        {
            try
            {

                DateTime dtFromTime;
                DateTime dtToTime;
                TimeSpan spn;
                if (tmedtFrom.Value.HasValue)
                {
                    spn = ((TimeSpan)tmedtFrom.Value);
                    dtFromTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtFromTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);
                }

                if (tmedtTo.Value.HasValue)
                {
                    spn = ((TimeSpan)tmedtTo.Value);
                    dtToTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtToTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
                }

                DataTable dt = new DataTable();

                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("CELLID", typeof(string));
                dt.Columns.Add("dtFROM", typeof(DateTime));
                dt.Columns.Add("dtTO", typeof(DateTime));
                dt.Columns.Add("ROW_NUM_FR", typeof(string));
                dt.Columns.Add("ROW_NUM_TO", typeof(string));
                dt.Columns.Add("PKGLOTID", typeof(string));


                DataRow newRow = dt.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = txtLOTID.Text.Trim() == "" ? null : txtLOTID.Text.Trim();
                newRow["CELLID"] = txtCELLID.Text.Trim() == "" ? null : txtCELLID.Text.Trim();
                newRow["dtFROM"] = dtFromTime;
                newRow["dtTO"] = dtToTime;
                newRow["PKGLOTID"] = txtPKGLOTID.Text.Trim() == "" ? null : txtPKGLOTID.Text.Trim();


                if (!string.IsNullOrEmpty(txtLOTID.Text.Trim()) || !string.IsNullOrEmpty(txtCELLID.Text.Trim()))
                {
                    newRow["dtFROM"] = DBNull.Value;
                    newRow["dtTO"] = DBNull.Value;
                }

                dt.Rows.Add(newRow);


                try
                {
                    if (dgAPDDATA_PACKAGING.Visibility == Visibility.Visible)
                    {

                        // CNB(A8,S4,AA) 이고 Packaging 일 경우 조회 분기
                        //string bizName = "DA_PRD_SEL_APD_DATA_FOR_PACKAGING"; 
                        //if ((LoginInfo.CFG_AREA_ID.Equals("A8") || LoginInfo.CFG_AREA_ID.Equals("S4") || LoginInfo.CFG_AREA_ID.Equals("AA") || LoginInfo.CFG_AREA_ID.Equals("AB")) &&
                        //    Util.NVC(cboProcess.SelectedValue).Equals("A9000"))
                        //{
                        //    bizName = "DA_PRD_SEL_APD_DATA_FOR_PACKAGING_DRB";
                        //}

                        //DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizName, "RQSTDT", "RSLTDT", dt);

                        // UI분리 동정보로 분기 호출하는 BizRule 부분을 BizRule에서 분기처리를 위해 별도 BizRule 생성
                        const string bizRuleName = "BR_PRD_SEL_APD_DATA_FOR_PACKAGING";
                        DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

                        if (dgAPDDATA_PACKAGING.GetRowCount() == 0)
                        {
                            Util.GridSetData(dgAPDDATA_PACKAGING, dtResult, FrameOperation);
                        }
                        else
                        {
                            DataTable dtInfo = DataTableConverter.Convert(dgAPDDATA_PACKAGING.ItemsSource);

                            // PrimaryKey 셋팅(Merge할때 기본키값을 설정하지 않으면 테이블 Merge시 중복된 데이터가 들어갈수 있습니다.

                            dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["CELLID"] };
                            dtInfo.Merge(dtResult, true);

                            Util.GridSetData(dgAPDDATA_PACKAGING, dtInfo, FrameOperation);
                        }

                    }
                    else if (dgAPDDATA_VISION.Visibility == Visibility.Visible)
                    {


                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;



                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION, _dtInfo, FrameOperation);
                        _dtInfo = null;



                        //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION", "RQSTDT", "RSLTDT", dt);

                        //if (dgAPDDATA_VISION.GetRowCount() == 0)
                        //{
                        //    Util.GridSetData(dgAPDDATA_VISION, dtResult, FrameOperation);
                        //}
                        //else
                        //{
                        //    DataTable dtInfo = DataTableConverter.Convert(dgAPDDATA_VISION.ItemsSource);

                        //    dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["CELLID"] };
                        //    dtInfo.Merge(dtResult, true);

                        //    Util.GridSetData(dgAPDDATA_VISION, dtInfo, FrameOperation);
                        //}
                    }
                    else if (dgAPDDATA_VISION_CNB.Visibility == Visibility.Visible)
                    {
                        //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CNB", "RQSTDT", "RSLTDT", dt);


                        //    if (dgAPDDATA_VISION_CNB.GetRowCount() == 0)
                        //    {
                        //        Util.GridSetData(dgAPDDATA_VISION_CNB, dtResult, FrameOperation);
                        //    }
                        //    else
                        //    {
                        //        DataTable dtInfo = DataTableConverter.Convert(dgAPDDATA_VISION_CNB.ItemsSource);

                        //        dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["CELLID"] };
                        //        dtInfo.Merge(dtResult, true);

                        //        Util.GridSetData(dgAPDDATA_VISION_CNB, dtInfo, FrameOperation);
                        //    }

                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CNB_V01", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CNB, _dtInfo, FrameOperation);
                        _dtInfo = null;

                    }
                    else if (dgAPDDATA_VISION_CWA3.Visibility == Visibility.Visible)
                    {

                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CWA3, _dtInfo, FrameOperation);
                        _dtInfo = null;


                    }
                    else if (dgAPDDATA_VISION_CWA3_NM.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CWA3_NM, _dtInfo, FrameOperation);
                        _dtInfo = null;

                        //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_1", "RQSTDT", "RSLTDT", dt);


                        //if (dgAPDDATA_VISION_CWA3.GetRowCount() == 0)
                        //{
                        //    Util.GridSetData(dgAPDDATA_VISION_CWA3, dtResult, FrameOperation);
                        //}
                        //else
                        //{
                        //    DataTable dtInfo = DataTableConverter.Convert(dgAPDDATA_VISION_CWA3.ItemsSource);

                        //    dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["CELLID"] };
                        //    dtInfo.Merge(dtResult, true);

                        //    Util.GridSetData(dgAPDDATA_VISION_CWA3, dtInfo, FrameOperation);
                        //}
                    }
                    else if (dgAPDDATA_VISION_CNB2.Visibility == Visibility.Visible)
                    {

                        //조회 최대 건수
                        // DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_ESNB2_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_ENSB2", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CNB2, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    else if (dgAPDDATA_VISION_UC1.Visibility == Visibility.Visible)
                    {

                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_UC1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_UC1, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    else if (dgAPDDATA_VISION_ESHM1.Visibility == Visibility.Visible)
                    {

                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_ESHM1", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_ESHM1, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    else if (dgAPDDATA_VISION_CWA4.Visibility == Visibility.Visible)
                    {

                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_ESWA4", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_VISION_CWA4, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    // ESMI2 EOL Sealing 검사
                    else if (dgAPDDATA_EOL_SEALING_ESMI2.Visibility == Visibility.Visible)
                    {
                        //조회 최대 건수
                        DataTable dtResultCount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_PACKING_VISION_CWA3_COUNT", "RQSTDT", "RSLTDT", dt);

                        if (dtResultCount.Rows[0]["ROW_COUNT"].ToString() == "0")
                        {
                            //조회된 Data가 없습니다.
                            Util.MessageValidation("SFU1905");
                            return;
                        }

                        if (Convert.ToUInt64(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) > 60000)
                        {
                            Util.MessageValidation("SFU8208", dtResultCount.Rows[0]["ROW_COUNT"].ToString()); //조회수량 [%1] : 최대 60000건까지만 조회할 수 있습니다.
                            return;
                        }

                        // 20000건씩 조회 하기 위해서  몇번 조회할지 계산
                        double ProcesCount = Math.Ceiling(Convert.ToDouble(dtResultCount.Rows[0]["ROW_COUNT"].ToString()) / 20000);

                        double ROW_NUM_FR = 1;
                        double ROW_NUM_TO = 20000;

                        for (int i = 0; i < ProcesCount; i++)
                        {
                            dt.Rows[0]["ROW_NUM_FR"] = ROW_NUM_FR;
                            dt.Rows[0]["ROW_NUM_TO"] = ROW_NUM_TO;

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APD_DATA_FOR_EOL_SEALING_ESMI2", "RQSTDT", "RSLTDT", dt);

                            if (_dtInfo == null)
                            {
                                _dtInfo = dtResult.Copy();
                            }
                            else
                            {
                                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["CELLID"] };
                                _dtInfo.Merge(dtResult, true);
                            }

                            ROW_NUM_FR = ROW_NUM_TO + 1;
                            ROW_NUM_TO = ROW_NUM_TO + 20000;

                        }
                        Util.GridSetData(dgAPDDATA_EOL_SEALING_ESMI2, _dtInfo, FrameOperation);
                        _dtInfo = null;
                    }

                    else
                    {
                        return;
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
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]
        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            // 조회 날짜 최대 체크.
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-31);
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

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






        #endregion

        #endregion
    }
}
