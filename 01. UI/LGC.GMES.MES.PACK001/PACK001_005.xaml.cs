/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack Lot 이력화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2017.12.14 손우석 CSR ID:3537853  팩 11호기 바코드 변경이력 추적기능 추가 요청의건 | [요청번호] C20171122_37853
  2018.10.04 손우석 CSR ID:3802384 180927_GMES UI화면 검사수집항목 단위 추가 요청 | [요청번호]C20180927_02384
  2018.10.29 손우석 CSR ID:3821490 GB/T 바코드 조립 이력 조회 기능 요청 건 [요청번호]C20181019_21490
  2018.11.29 손우석 CSR ID:3855538 팩 라벨 관리 강화 시스템 개발 요청 [요청번호]C20181128_55538
  2018.12.17 손우석 오창 요청 - Tree View 자동 선택 기능 해제
  2019.06.19 김도형 C20190515_94246 : GMES LOT 이력 조회 화면 개선 요청
  2019.08.31 손우석 CSR ID 4075577 LOT이력 조회 기능 개선 요청 [요청번호] C20190902_75577
  2020.01.20 손우석 CSR ID 6935 Inspection Data history tap 상 Operator column 추가 요청 건 [요청번호] C20191121-000303
  2020.05.25 손우석 서비스 번호 63351 효율적인 MES 사용을 위한 틀고정 기능 요청 [요청번호] C20200524-000001
  2020.05.27 손우석 서비스 번호 63351 효율적인 MES 사용을 위한 틀고정 기능 요청 [요청번호] C20200524-000001
  2020.06.03 김길용 서비스 번호 43706 GMES 기능 추가요청(이력관리) [요청번호] C20200319-000415
  2020.10.12 염규범 LOT 이력 연속 선택 조건 요청의 건
  2021.06.03 염규범S   C20210423-000047 (175721) GMES LOT이력 화면 유저 편의성 개선 요청 (ESWA)
  2021.10.28 정용석 MES Hold 처리된 모듈에 대하여 수동 공정진척, Lot 이력에서 Keypart 결합/결합해체 불가하도록 인터락 적용
  2022.03.18 이태규 라우트 식별기능 :  ROUNNAME 정보추가 추가[요청번호]
  2022.09.05 염규범 GMES 내 특이사항 이력관리 기입 적용
  2024.02.05 정용석 CSR ID:E20240119-001767 LOT 이력에서 창고 또는 RACK MAPPING 이력 탭 추가
  2025.02.21 이헌수 Lot 이력 조회 공정 표기 방식 변경 (고세범 선임님 요청) :MES 2.0 LOT 이력 GRID ACT_PROCID 추가 및 조회 컬럼 순서 변경
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_005 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        string now_labelcode = string.Empty;
        private DataTable dtHold = new DataTable();

        //2017.12.14
        DataTable dtSearchTable;
        //2018.11.29
        string his_labelcode = string.Empty;
        string str_LOTID = string.Empty;

        public PACK001_005()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        //private DataView dvRootNodes;

        /// <summary>
        /// 우클릭 context menu 오픈시 C1TreeViewItem 을 가져오기위한 전역변수
        /// </summary>
        private C1TreeViewItem c1trItem_By_RightButtonEvent = null;

        #endregion

        #region Initialize
        private void InitDataGrid()
        {
            try
            {
                //Grid 틀고정
                //2021-06-23 염규범 S
                //dgInspectionData.FrozenColumnCount = 6;
                //dgDetailData.FrozenColumnCount = 7;
                //dgLabelHistory.FrozenColumnCount = 7;
                //dgChangeOffData.FrozenColumnCount = 7;

                dgInspectionData.FrozenColumnCount = 5;
                dgDetailData.FrozenColumnCount = 5;
                dgLabelHistory.FrozenColumnCount = 6;
                dgChangeOffData.FrozenColumnCount = 7;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event

        #region Event - UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = FrameOperation.Parameters;

                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                {
                    //ESNJ PACK 소형 조립일 경우
                    Grid.SetColumnSpan(txtLGC_LOTID, 1);
                    tbPR_LOTID.Visibility = Visibility.Visible;
                    txtPR_LOTID.Visibility = Visibility.Visible;
                    // 2022.09.05 염규범 선임
                    // GMES 내 특이사항 이력관리 기입 적용
                    inputLotStatCode.Visibility = Visibility.Collapsed;
                    trKeypart.Visibility = Visibility.Visible;
                    EqptMountPstnId.Visibility = Visibility.Visible;
                    vltgGrdCode.Visibility = Visibility.Visible;
                }
                else
                {
                    //ESNJ PACK 소형 조립이 아닌 경우
                    Grid.SetColumnSpan(txtLGC_LOTID, 1);
                    tbPR_LOTID.Visibility = Visibility.Collapsed;
                    txtPR_LOTID.Visibility = Visibility.Collapsed;
                    // 2022.09.05 염규범 선임
                    // GMES 내 특이사항 이력관리 기입 적용
                    inputLotStatCode.Visibility = Visibility.Visible;
                    trKeypart.Visibility = Visibility.Visible;
                    EqptMountPstnId.Visibility = Visibility.Collapsed;
                    vltgGrdCode.Visibility = Visibility.Collapsed;
                }

                if (tmps != null)
                {
                    if (tmps.Length > 0)
                    {
                        string sLotid = tmps[0].ToString();
                        txtSearchLotId.Text = sLotid;
                        SetInfomation(true, txtSearchLotId.Text.Trim());


                    }
                }

                //2020.05.25
                InitDataGrid();

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Event - TextBox
        private void txtSearchLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSearchLotId.Text.Length > 0)
                    {
                        SetInfomation(true, txtSearchLotId.Text.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - Button

        private void btnGqmsLingk_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.COM001.COM001_331 GqmsSearch = new LGC.GMES.MES.COM001.COM001_331();
            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            object[] Parameters = new object[10];
            Parameters[0] = "COM001_331_Tab_GQMSRESULT";
            Parameters[1] = txtSearchLotId.Text;

            this.FrameOperation.OpenMenu("SFU010160351", true, Parameters);
        }

        private void btnSearchLotId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSearchLotId.Text.Length > 0)
                {
                    SetInfomation(true, txtSearchLotId.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcelActHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgActHistory);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnExcelChageOffData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgChangeOffData);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcelKeypartData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgKeypart);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }
        private void btnExcelInspectionData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgInspectionData);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcepLocationHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(this.dgRackMappingHistory);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        

        private void btnExcelDetailData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgDetailData);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnKeyPartCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                copyClipboardTreeView();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExpandFrameLeft_Unchecked(object sender, RoutedEventArgs e)
        {
            ContentLeft.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Auto);//360
            ContentLeft.RowDefinitions[1].Height = new GridLength(8);

            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();


            //gla.From = new GridLength(0);
            //gla.To = new GridLength(1);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            //ContentLeft.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);

        }

        private void btnExpandFrameLeft_Checked(object sender, RoutedEventArgs e)
        {
            ContentLeft.RowDefinitions[0].Height = new GridLength(0);
            ContentLeft.RowDefinitions[1].Height = new GridLength(0);

            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            //gla.From = new GridLength(1, GridLength.Auto);
            //gla.To = new GridLength(0, GridUnitType.Star);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            //ContentLeft.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnGBTChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                PACK001_005_CHANGEGBT popup = new PACK001_005_CHANGEGBT();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = txtSearchLotId.Text;
                    Parameters[1] = txtGBTId.Text;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2018.11.29
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                now_labelcode = sLabelCode;

                if (sLabelCode.Length > 0)
                {
                    Util.printLabel_Pack(FrameOperation, loadingIndicator, str_LOTID, LABEL_TYPE_CODE.PACK, sLabelCode, "N", "1", Util.NVC(txtLotInfoProductId.Tag));
                }
                else
                {
                    Util.printLabel_Pack(FrameOperation, loadingIndicator, str_LOTID, LABEL_TYPE_CODE.PACK, "N", "1", null);
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event - ContextMenu ItemClick
        private void Item_AllCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                copyClipboardTreeView();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_Copy_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                copyClipboardTreeView_Selected();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_Change_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }


                string sLotid = string.Empty;
                string sLotMid = string.Empty;

                if (c1trItem.ParentItem != null)
                {
                    IList<DependencyObject> itemParentText = new List<DependencyObject>();
                    VTreeHelper.GetChildrenOfType(c1trItem.ParentItem, typeof(TextBlock), ref itemParentText);
                    TextBlock textParentBolock = (TextBlock)itemParentText[0];


                    IList<DependencyObject> itemText = new List<DependencyObject>();
                    VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                    TextBlock textBolock = (TextBlock)itemText[0];

                    sLotid = textParentBolock.Text;
                    sLotMid = textBolock.Text;
                }
                else
                {
                    IList<DependencyObject> itemText = new List<DependencyObject>();
                    VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                    TextBlock textBolock = (TextBlock)itemText[0];

                    sLotid = textBolock.Text;
                }

                // 2021.10.28 MES Hold 처리된 모듈에 대하여 수동 공정진척, Lot 이력에서 Keypart 결합/결합해체 불가하도록 인터락 적용
                if (CommonVerify.HasTableRow(this.dtHold))
                {
                    var masterLOTHold = this.dtHold.AsEnumerable().Where(x => x.Field<string>("ATTACH_LEVEL").Equals("0")).Count();
                    var materialLOTMESHold = this.dtHold.AsEnumerable().Where(x => !x.Field<string>("ATTACH_LEVEL").Equals("0") && x.Field<string>("HOLD_GUBUN").Equals("MES_HOLD")).Count();
                    var materialLOTQMSHold = this.dtHold.AsEnumerable().Where(x => !x.Field<string>("ATTACH_LEVEL").Equals("0") && x.Field<string>("HOLD_GUBUN").Contains("QMS_HOLD")).Count();
                    var materialLOTMEASRHold = this.dtHold.AsEnumerable().Where(x => !x.Field<string>("ATTACH_LEVEL").Equals("0") && x.Field<string>("HOLD_GUBUN").Equals("MEASR_HOLD")).Count();

                    // RESULT 코드 정의
                    // 결합 가능 / 해체 가능: 0
                    // 결합 가능 / 해체 불가: 1
                    // 결합 불가 / 해체 가능: 2
                    // 결합 불가 / 해체 불가: 3
                    int checkResultValue = 0;

                    // 부모 LOT이 HOLD이면 HOLD 구분과 상관없이 KEYPART 결합 / 해체 불가
                    if (masterLOTHold > 0)
                    {
                        checkResultValue = 3;
                    }
                    // 부모 LOT이 HOLD가 아니며 자재 LOT이 MES HOLD 걸렸으면 결합 불가 / 해체 가능
                    else if (masterLOTHold <= 0 && materialLOTMESHold > 0)
                    {
                        checkResultValue = 2;
                    }
                    // 부모 LOT이 HOLD가 아니며 자재 LOT이 OQC HOLD, QMS HOLD가 걸렸으면 결합 / 해체 가능
                    else if (masterLOTHold <= 0 && materialLOTQMSHold > 0)
                    {
                        checkResultValue = 0;
                    }
                    // 부모 LOT이 HOLD가 아니며 자재 LOT이 MEASR HOLD가 걸렸으면 결합 불가 / 해체 가능
                    else if (masterLOTHold <= 0 && materialLOTMEASRHold > 0)
                    {
                        checkResultValue = 2;
                    }
                    else
                    {
                        checkResultValue = 0;
                    }

                    if (checkResultValue.Equals(3))
                    {
                        Util.MessageValidation("SFU8425", sLotid); // LOT [%1]는 HOLD되어 있으므로 KeyPart 결합 및 해체가 불가합니다.
                        return;
                    }
                }

                PACK001_005_LOTCHANGE popup = new PACK001_005_LOTCHANGE();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));
                    dtData.Columns.Add("LOTMID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["LOTID"] = sLotid;
                    newRow["LOTMID"] = sLotMid;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_Scrap_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }
                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];

                PACK001_005_LOTSCRAP popup = new PACK001_005_LOTSCRAP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["LOTID"] = textBolock.Text;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_AddKeyPart_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }
                //1.우클릭위치의 item 의 lotid 추출
                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];
                DataTable dtTrvData = DataTableConverter.Convert(c1trItem.ParentItemsSource);
                DataRow[] drTargetRow = dtTrvData.Select("INPUT_LOTID = '" + textBolock.Text + "'");
                string sLotid = Util.NVC(drTargetRow[0]["INPUT_LOTID"]);

                //2.MBOM 스팩 없는경우 부모ITEM 의 LOTID 추출
                string sMtrl_Mbom_count = Util.NVC(drTargetRow[0]["MTRLMBOM_COUNT"]);
                if (sMtrl_Mbom_count == "0")
                {
                    C1TreeViewItem c1trItemParent = c1trItem.ParentItem;
                    if (c1trItemParent == null)
                    {
                        return;
                    }
                    IList<DependencyObject> itemTextparent = new List<DependencyObject>();
                    VTreeHelper.GetChildrenOfType(c1trItemParent, typeof(TextBlock), ref itemTextparent);
                    TextBlock textBolocktparent = (TextBlock)itemTextparent[0];
                    sLotid = textBolocktparent.Text;
                }



                PACK001_005_ADDKEYPART popup = new PACK001_005_ADDKEYPART();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["LOTID"] = sLotid;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_AddInspectionDate_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }
                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];
                //DataTable dtTrvData = DataTableConverter.Convert(c1trItem.ParentItemsSource);
                //DataRow[] drTargetRow = dtTrvData.Select("INPUT_LOTID = '" + textBolock.Text + "'");

                //string sLotid = Util.NVC(drTargetRow[0]["INPUT_LOTID"]);


                PACK001_005_ADDINSPECTIONDATA popup = new PACK001_005_ADDINSPECTIONDATA();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["LOTID"] = textBolock.Text;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_AddDetailData_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                //C1TreeViewItem c1trItem = trvKeypartList.SelectedItem;

                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }
                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];
                DataTable dtTrvData = DataTableConverter.Convert(c1trItem.ParentItemsSource);
                DataRow[] drTargetRow = dtTrvData.Select("INPUT_LOTID = '" + textBolock.Text + "'");

                string sLotid = Util.NVC(drTargetRow[0]["INPUT_LOTID"]);
                string sprodid = Util.NVC(drTargetRow[0]["MTRLID"]);
                string seqsgid = Util.NVC(drTargetRow[0]["EQSGID"]);

                if (Util.NVC(txtSearchLotId.Tag) == sLotid)
                {
                    sprodid = Util.NVC(txtLotInfoProductId.Tag);
                    seqsgid = Util.NVC(txtLotInfoWipLine.Tag);
                }

                PACK001_005_ADDDETAILDATA popup = new PACK001_005_ADDDETAILDATA();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));
                    dtData.Columns.Add("PRODID", typeof(string));
                    dtData.Columns.Add("EQSGID", typeof(string));


                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["LOTID"] = sLotid;
                    newRow["PRODID"] = sprodid;
                    newRow["EQSGID"] = seqsgid;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_ChangeHistory_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }
                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];

                PACK001_005_CHANGEHISTORY popup = new PACK001_005_CHANGEHISTORY();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["LOTID"] = textBolock.Text;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_LabelPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }
                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];

                DataTable dtTrvData = DataTableConverter.Convert(c1trItem.ParentItemsSource);
                DataRow[] drTargetRow = dtTrvData.Select("INPUT_LOTID = '" + textBolock.Text + "'");

                if (drTargetRow.Length > 0)
                {
                    string sPRDT_CLSS_CODE = string.Empty;
                    string sLOTID = string.Empty;
                    string sProdid = string.Empty;

                    if (Util.NVC(drTargetRow[0]["LOTID"]) == null || Util.NVC(drTargetRow[0]["LOTID"]) == "")
                    {
                        if (txtLotInfoWipProcess.Text.Contains("BMA"))
                        {
                            sPRDT_CLSS_CODE = "BMA";
                        }
                        else if (txtLotInfoWipProcess.Text.Contains("CMA"))
                        {
                            sPRDT_CLSS_CODE = "CMA";
                        }
                        else
                        {
                            sPRDT_CLSS_CODE = "CELL";
                        }


                        sLOTID = Util.NVC(drTargetRow[0]["INPUT_LOTID"]);
                        sProdid = Util.NVC(txtLotInfoProductId.Tag);
                    }
                    else
                    {
                        sPRDT_CLSS_CODE = Util.NVC(drTargetRow[0]["PRDT_CLSS_CODE"]);
                        sLOTID = Util.NVC(drTargetRow[0]["INPUT_LOTID"]);
                        sProdid = Util.NVC(drTargetRow[0]["MTRLID"]);
                    }


                    //string sProdID = Util.NVC(drTargetRow[0]["PRODID"]);

                    //추가
                    string sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                    now_labelcode = sLabelCode;

                    if (sPRDT_CLSS_CODE == "CMA" || sPRDT_CLSS_CODE == "BMA")
                    {
                        if (sLabelCode.Length > 0)
                        {
                            Util.printLabel_Pack(FrameOperation, loadingIndicator, sLOTID, LABEL_TYPE_CODE.PACK, sLabelCode, "N", "1", sProdid);
                        }
                        else
                        {
                            Util.printLabel_Pack(FrameOperation, loadingIndicator, sLOTID, LABEL_TYPE_CODE.PACK, "N", "1", null);
                        }

                        //Util.printLabel_Pack(FrameOperation, loadingIndicator, textBolock.Text, LABEL_TYPE_CODE.PACK, "N", "1", null);
                    }
                    //else if (sLOTID == "")
                    // {
                    //    Util.printLabel_Pack(FrameOperation, loadingIndicator, textBolock.Text, LABEL_TYPE_CODE.PACK, "N", "1", null);
                    //}

                }


            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_ChangeWipResncodeCause_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }
                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];

                PACK001_005_CHANGEWIPREASON popup = new PACK001_005_CHANGEWIPREASON();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["LOTID"] = textBolock.Text;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Item_dgChangeOffData_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                C1TreeViewItem c1trItem = c1trItem_By_RightButtonEvent;
                if (c1trItem == null)
                {

                    return;
                }
                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];

                PACK001_005_CHANGEOFFDATA popup = new PACK001_005_CHANGEOFFDATA();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["LOTID"] = textBolock.Text;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - TreeView
        private void trvKeypartList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1TreeViewItem c1trItem = trvKeypartList.GetNode(pnt);

                if (c1trItem == null)
                {
                    return;
                }

                if ((ToolTip)c1trItem.ToolTip != null)
                {
                    ((ToolTip)c1trItem.ToolTip).IsOpen = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void trvKeypartList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1TreeViewItem c1trItem = trvKeypartList.GetNode(pnt);

                if (c1trItem == null)
                {
                    return;
                }

                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];
                DataTable dtTrvData = DataTableConverter.Convert(c1trItem.ParentItemsSource);
                DataRow[] drTargetRow = dtTrvData.Select("INPUT_LOTID = '" + textBolock.Text + "'");
                string sINPUT_LOT_TYPE_CODE = Util.NVC(drTargetRow[0]["INPUT_LOT_TYPE_CODE"]);

                if (sINPUT_LOT_TYPE_CODE.Equals("PROD") || sINPUT_LOT_TYPE_CODE.Equals(""))
                {
                    if (textBolock.Text != txtTagetLotId.Text)
                    {
                        SetInfomation(false, textBolock.Text);
                    }
                }
                else if (sINPUT_LOT_TYPE_CODE.Equals("BOX"))
                {
                    PACK001_003_BOXINFO popup = new PACK001_003_BOXINFO();
                    popup.FrameOperation = this.FrameOperation;

                    if (popup != null)
                    {
                        DataTable dtData = new DataTable();
                        dtData.Columns.Add("BOXID", typeof(string));

                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["BOXID"] = textBolock.Text;

                        dtData.Rows.Add(newRow);

                        //========================================================================
                        object[] Parameters = new object[1];
                        Parameters[0] = dtData;
                        C1WindowExtension.SetParameters(popup, Parameters);
                        //========================================================================

                        //popup.Closed -= popup_Closed;
                        //popup.Closed += popup_Closed;
                        popup.CenterOnScreen();
                        popup.ShowModal();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void trvKeypartList_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1TreeViewItem c1trItem = trvKeypartList.GetNode(pnt);

                setTreeViewToolTip(c1trItem);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void C1trItem_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                C1TreeViewItem c1trItem = (C1TreeViewItem)sender;
                ((ToolTip)c1trItem.ToolTip).IsOpen = false;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void trvKeypartList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                c1trItem_By_RightButtonEvent = trvKeypartList.GetNode(pnt);

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Event - Grid

        #endregion

        void popup_Closed(object sender, EventArgs e)
        {
            C1Window popup = sender as C1Window;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                SetInfomation(true, txtSearchLotId.Text.Trim());
            }
        }

        #endregion

        #region Mehod

        private void SetInfomation(bool bMainLot_SubLot_Flag, string sLotid)
        {
            //BR_PRD_GET_LOT_INFO_HIST
            DataSet dsResult = null;
            this.dtHold.Clear();
            try
            {
                if (bMainLot_SubLot_Flag)
                {
                    clearLotInfoText();
                }

                txtTagetLotId.Text = sLotid;
                //string sLotid = txtSearchLotId.Text.Trim();

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_LOT_INFO_HIST", "INDATA", "LOT,LOT_INPUT_MTRL,ACTHISTORY,WIPDATACOLLECT_Q,WIPDATACOLLECT_E,LGC_LOTID,LOT_HOLD,LOT_OFFMODE_HIST,LOT_RACK_MAPP_HIST", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    int iInfoExistChk = 0;

                    txtSearchLotId.Tag = sLotid; //조회후 lot id 기억.

                    if ((dsResult.Tables.IndexOf("LOT") > -1) && dsResult.Tables["LOT"].Rows.Count > 0)
                    {
                        if (bMainLot_SubLot_Flag)
                        {
                            setLotInfoText(dsResult.Tables["LOT"]);
                        }

                        sLotid = Util.NVC(dsResult.Tables["LOT"].Rows[0]["LOTID"]);
                        txtSearchLotId.Tag = sLotid;

                        iInfoExistChk += dsResult.Tables["LOT"].Rows.Count;

                        //2018.10.29
                        txtGBTId.Text = Util.NVC(dsResult.Tables["LOT"].Rows[0]["GBTID"]);

                        //2018.11.29
                        str_LOTID = Util.NVC(dsResult.Tables["LOT"].Rows[0]["LOTID"]);

                        //2019.06.19 김도형 C20190515_94246 : GMES LOT 이력 조회 화면 개선 요청
                        txtLGC_LOTID.Text = Util.NVC(dsResult.Tables["LGC_LOTID"].Rows[0]["LGC_LOTID"]);

                        //2019.08.31
                        if (Util.NVC(dsResult.Tables["LOT"].Rows[0]["PRDT_CLSS_CODE"]) == "CELL")
                        {
                            txtCellLIne.Text = Util.NVC(dsResult.Tables["LOT"].Rows[0]["CELL_INPUT_EQSGNAME"]);
                        }
                        else
                        {
                            txtCellLIne.Text = "";
                        }
                        //2021.08.02 라인 이동 이력 확인용 이동않한다면 현재 라인 정보 View
                        txtCreateEqsgid.Text = Util.NVC(dsResult.Tables["LOT"].Rows[0]["GNRT_EQSGNAME"]);

                        //2022.03.17
                        txtRoutName.Text = Util.NVC(dsResult.Tables["LOT"].Rows[0]["ROUTNAME"]);
                        txtPR_LOTID.Text = Util.NVC(dsResult.Tables["LOT"].Rows[0]["PR_LOTID"]);

                        txtOCOP_RTN_FLAG.Text = Util.NVC(dsResult.Tables["LGC_LOTID"].Rows[0]["OCOP_RTN_FLAG"]);
                    }

                    if (dsResult.Tables.IndexOf("LOT_INPUT_MTRL") > -1)
                    {
                        if (bMainLot_SubLot_Flag)
                        {
                            if (dsResult.Tables["LOT_INPUT_MTRL"].Rows.Count > 0)
                            {
                                setTreeView(dsResult.Tables["LOT_INPUT_MTRL"], sLotid);
                            }
                            else
                            {
                                setTreeView(dsResult.Tables["LOT_INPUT_MTRL"], Convert.ToString(dsResult.Tables["LOT"].Rows[0]["LOTID"]));
                            }

                        }
                        iInfoExistChk += dsResult.Tables["LOT_INPUT_MTRL"].Rows.Count;
                    }
                    //if ((dsResult.Tables.IndexOf("LOT_INPUT_MTRL") > -1) && dsResult.Tables["LOT_INPUT_MTRL"].Rows.Count > 0)
                    //{
                    //    if (bMainLot_SubLot_Flag)
                    //    {
                    //        setTreeView(dsResult.Tables["LOT_INPUT_MTRL"], sLotid);
                    //    }
                    //    iInfoExistChk += dsResult.Tables["LOT_INPUT_MTRL"].Rows.Count;
                    //}
                    //else
                    //{
                    //    if (bMainLot_SubLot_Flag)
                    //    {
                    //        setTreeViewDefault(dsResult.Tables["LOT_INPUT_MTRL"], Convert.ToString(dsResult.Tables["MAPPING_LOT"].Rows[0]["LOTID"]));
                    //    }
                    //    iInfoExistChk += dsResult.Tables["MAPPING_LOT"].Rows.Count;
                    //}
                    if ((dsResult.Tables.IndexOf("ACTHISTORY") > -1) && dsResult.Tables["ACTHISTORY"].Rows.Count > 0)
                    {
                        //dgActHistory.ItemsSource = DataTableConverter.Convert(dsResult.Tables["ACTHISTORY"]);
                        Util.GridSetData(dgActHistory, dsResult.Tables["ACTHISTORY"], FrameOperation, false);
                        iInfoExistChk += dsResult.Tables["ACTHISTORY"].Rows.Count;
                    }

                    if ((dsResult.Tables.IndexOf("WIPDATACOLLECT_Q") > -1) && dsResult.Tables["WIPDATACOLLECT_Q"].Rows.Count > 0)
                    {
                        //dgInspectionData.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIPDATACOLLECT_Q"]);
                        Util.GridSetData(dgInspectionData, dsResult.Tables["WIPDATACOLLECT_Q"], FrameOperation, false);
                        iInfoExistChk += dsResult.Tables["WIPDATACOLLECT_Q"].Rows.Count;
                    }

                    if ((dsResult.Tables.IndexOf("WIPDATACOLLECT_E") > -1) && dsResult.Tables["WIPDATACOLLECT_E"].Rows.Count > 0)
                    {
                        //dgDetailData.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIPDATACOLLECT_E"]);
                        Util.GridSetData(dgDetailData, dsResult.Tables["WIPDATACOLLECT_E"], FrameOperation, false);
                        iInfoExistChk += dsResult.Tables["WIPDATACOLLECT_E"].Rows.Count;
                    }
                    if ((dsResult.Tables.IndexOf("LOT_OFFMODE_HIST") > -1) && dsResult.Tables["LOT_OFFMODE_HIST"].Rows.Count > 0)
                    {
                        //dgDetailData.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIPDATACOLLECT_E"]);
                        Util.GridSetData(dgChangeOffData, dsResult.Tables["LOT_OFFMODE_HIST"], FrameOperation, false);
                        iInfoExistChk += dsResult.Tables["LOT_OFFMODE_HIST"].Rows.Count;
                    }
                    //2020.04.09 염규범 S
                    //추후 QMS 연동해서 하는 부분 추가 예정
                    if (this.dtHold.Columns.Count <= 0)
                    {
                        this.dtHold = dsResult.Tables["LOT_HOLD"].Clone();
                    }
                    this.dtHold.Clear();

                    if ((dsResult.Tables.IndexOf("LOT_HOLD") > -1) && dsResult.Tables["LOT_HOLD"].Rows.Count > 0)
                    {
                        txtHoldYN.Text = "Y";
                        txtHoldYN.ToolTip = "HOLD_RESN  : " + dsResult.Tables["LOT_HOLD"].Rows[0]["HOLD_RESN"].ToString() + "\n" +
                                            "HOLD_DTTM : " + dsResult.Tables["LOT_HOLD"].Rows[0]["HOLD_DTTM"].ToString() + "\n" +
                                            "HOLD_CODE : " + dsResult.Tables["LOT_HOLD"].Rows[0]["HOLD_CODE"].ToString() + "\n" +
                                            "HOLD_NOTE : " + dsResult.Tables["LOT_HOLD"].Rows[0]["HOLD_NOTE"].ToString().Trim();

                        string temp = string.Empty;

                        for (int i = 0; i < dsResult.Tables["LOT_HOLD"].Rows.Count; i++)
                        {
                            if (i == 0)
                            {
                                temp = dsResult.Tables["LOT_HOLD"].Rows[i]["HOLD_RESN"].ToString();
                            }
                            else
                            {
                                temp = temp + ',' + dsResult.Tables["LOT_HOLD"].Rows[i]["HOLD_RESN"].ToString();
                            }
                        }

                        txtHold.Text = temp;

                        if (this.dtHold.Columns.Count <= 0)
                        {
                            this.dtHold = dsResult.Tables["LOT_HOLD"].Clone();
                        }
                        this.dtHold.Clear();
                        this.dtHold = dsResult.Tables["LOT_HOLD"].Copy();

                    }
                    else
                    {
                        txtHoldYN.Text = "N";
                        txtHold.Text = ObjectDic.Instance.GetObjectName("홀드 상태가 아닙니다.");
                        txtHoldYN.ToolTip = ObjectDic.Instance.GetObjectName("홀드 상태가 아닙니다.");
                    }

                    // 2024.02.05 CSRID : E20240119-001767 LOT별 RACK MAPPING 이력 조회 추가
                    Util.gridClear(this.dgRackMappingHistory);
                    if ((dsResult.Tables.IndexOf("LOT_RACK_MAPP_HIST") > -1) && dsResult.Tables["LOT_RACK_MAPP_HIST"].Rows.Count > 0)
                    {
                        Util.GridSetData(dgRackMappingHistory, dsResult.Tables["LOT_RACK_MAPP_HIST"], FrameOperation, false);
                        iInfoExistChk += dsResult.Tables["LOT_RACK_MAPP_HIST"].Rows.Count;
                    }

                    if (iInfoExistChk == 0)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1386"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //LOT정보가 없습니다.
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSearchLotId.SelectAll();
                                txtSearchLotId.Focus();
                            }
                        });
                    }
                    txtSearchLotId.SelectAll();
                    txtSearchLotId.Focus();

                    //2017.12.14
                    //searchData(txtSearchLotId.Text);
                    //2018.11.29
                    searchData(str_LOTID);

                    if (!string.IsNullOrEmpty(his_labelcode))
                    {
                        cboLabelCode.SelectedValue = his_labelcode;
                    }


                }

                //2022.09.05
                //염규범S 결합 상태 확인의 건
                //if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                //{
                DataTable indt = new DataTable();

                indt.Columns.Add("LANGID");
                indt.Columns.Add("LOTID");
                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                {
                    indt.Columns.Add("INPUT_LOT_STAT_CODE");
                }

                DataRow drdata = indt.NewRow();

                drdata["LANGID"] = LoginInfo.LANGID;
                drdata["LOTID"] = sLotid;
                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                {
                    drdata["INPUT_LOT_STAT_CODE"] = "ATTACH";
                }


                indt.Rows.Add(drdata);
                DataTable resultInputHist = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INPUT_MTRL_HIST", "RQSTDT", "RSLTDT", indt);

                if (resultInputHist.Rows.Count > 0)
                {
                    Util.GridSetData(dgKeypart, resultInputHist, FrameOperation, false);
                }
                else
                {
                    Util.gridClear(dgKeypart);
                }
                //}
            }
            catch (Exception ex)
            {
                //2020.10.12 염규범S
                //LOT 이력 연속 선택 조건 요청의 건
                //Util.MessageInfo(ex.Message.ToString(), (result) =>
                //{
                //    if (result == MessageBoxResult.OK)
                //    {
                //        txtSearchLotId.SelectAll();
                //        txtSearchLotId.Focus();
                //    }
                //});
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtSearchLotId.SelectAll();
                        txtSearchLotId.Focus();
                    }
                });
                //Util.MessageException(ex);
                // Util.AlertByBiz("BR_PRD_GET_LOT_INFO_HIST", ex.Message, ex.ToString());
            }
            finally
            {
                //2020.10.12 염규범S
                //LOT 이력 연속 선택 조건 요청의 건
                txtSearchLotId.SelectAll();
                txtSearchLotId.Focus();
            }
        }

        //2017.12.14
        private void searchData(string strLotid)
        {
            try
            {
                //2018.11.29
                int iMaxrow = 0;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = strLotid;

                RQSTDT.Rows.Add(dr);

                dtSearchTable = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKLABEL_HIST", "INDATA", "OUTDATA", RQSTDT);

                dgLabelHistory.ItemsSource = null;

                if (dtSearchTable.Rows.Count != 0)
                {
                    Util.GridSetData(dgLabelHistory, dtSearchTable, FrameOperation);

                    //2018.11.29
                    iMaxrow = dtSearchTable.Rows.Count - 1;

                    his_labelcode = Util.NVC(dtSearchTable.Rows[iMaxrow]["LABEL_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setTreeView(DataTable dtInputMtrl, string sLotid)
        {
            try
            {
                DataSet ds = new DataSet();
                DataRow dr = dtInputMtrl.NewRow();
                //if (dtInputMtrl != null)
                //{
                //    if (dtInputMtrl.Rows.Count > 0)
                //    {
                //        dr.ItemArray = new object[] { null, null, Util.NVC(dtInputMtrl.Rows[0]["LOTID"]), null, null, null, null, null, null, null, null };
                //    }
                //    else
                //    {
                //        dr.ItemArray = new object[] { null, null, sLotid, null, null, null, null, null, null, null, null };
                //    }
                //}
                //else
                //{
                //    dr.ItemArray = new object[] { null, null, sLotid, null, null, null, null, null, null, null, null };
                //}
                //if (dtInputMtrl.Rows.Count > 0)
                //{
                // 2024.10.16. 김영국 - SQL 결과에 대한 Json Data 순서 틀어짐에 따른 로직 주석처리.
                //dr.ItemArray = new object[] { null, null, sLotid, sLotid, null, null, null, null, null, null, null, null };
                //}
                //else
                //{
                //    dr.ItemArray = new object[] { null, sLotid, sLotid, sLotid, null, null, null, null, null, null, null, null };
                //}

                // 2024.10.16. 김영국 - SQL 결과에 대한 Json Data 순서 틀어짐에 따라 DataRow에 Column Mapping.
                dr["LOTID_RELATION"] = sLotid;
                dr["INPUT_LOTID"] = sLotid;

                dtInputMtrl.Rows.InsertAt(dr, 0);

                ds.Tables.Add(dtInputMtrl.Copy());
                //ds.Relations.Add("Relations", ds.Tables["LOT_INPUT_MTRL"].Columns["INPUT_LOTID"], ds.Tables["LOT_INPUT_MTRL"].Columns["LOTID"]);
                ds.Relations.Add("Relations", ds.Tables["LOT_INPUT_MTRL"].Columns["LOTID_RELATION"], ds.Tables["LOT_INPUT_MTRL"].Columns["LOTID"]);

                DataView dvRootNodes = ds.Tables["LOT_INPUT_MTRL"].DefaultView;
                dvRootNodes.RowFilter = "LOTID IS NULL";

                //trvKeypartList.ClearValue(ItemsControl.ItemsSourceProperty);
                //trvKeypartList.ClearValue(ItemsControl.ItemTemplateProperty);
                //trvKeypartList.Items.Clear();

                //FrameworkElementFactory labelFactory = new FrameworkElementFactory(typeof(TextBlock));
                //labelFactory.SetBinding(TextBlock.TextProperty, new Binding("INPUT_LOTID"));

                //HierarchicalDataTemplate template = new HierarchicalDataTemplate(typeof(string));
                //template.ItemsSource = new Binding("Relations");
                //template.VisualTree = labelFactory;

                //trvKeypartList.ItemTemplate = template;

                //InitializeComponent();
                trvKeypartList.ItemsSource = dvRootNodes;
                TreeItemExpandAll();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void setTreeViewDefault(DataTable dtInputMtrl, string sLotid)
        //{
        //    try
        //    {
        //        DataSet ds = new DataSet();
        //        DataRow dr = dtInputMtrl.NewRow();

        //        dr.ItemArray = new object[] { null, sLotid, sLotid, null, null, null, null, null, null, null, null, null };
        //        dtInputMtrl.Rows.InsertAt(dr, 0);


        //        ds.Tables.Add(dtInputMtrl.Copy());
        //        ds.Relations.Add("Relations", ds.Tables["LOT_INPUT_MTRL"].Columns["LOTID"], ds.Tables["LOT_INPUT_MTRL"].Columns["LOTID_RELATION"]);
        //        DataView dvRootNodes = ds.Tables["LOT_INPUT_MTRL"].DefaultView;
        //       dvRootNodes.RowFilter = "LOTID_RELATION IS NULL";

        //        //trvKeypartList.ClearValue(ItemsControl.ItemsSourceProperty);
        //        //trvKeypartList.ClearValue(ItemsControl.ItemTemplateProperty);
        //        //trvKeypartList.Items.Clear();

        //        //FrameworkElementFactory labelFactory = new FrameworkElementFactory(typeof(TextBlock));
        //        //labelFactory.SetBinding(TextBlock.TextProperty, new Binding("INPUT_LOTID"));

        //        //HierarchicalDataTemplate template = new HierarchicalDataTemplate(typeof(string));
        //        //template.ItemsSource = new Binding("Relations");
        //        //template.VisualTree = labelFactory;

        //        //trvKeypartList.ItemTemplate = template;

        //        InitializeComponent();
        //        trvKeypartList.ItemsSource = dvRootNodes;
        //        TreeItemExpandAll();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private void setTreeViewToolTip(C1TreeViewItem c1trItem)
        {
            try
            {
                if (c1trItem == null)
                {
                    return;
                }
                if ((ToolTip)c1trItem.ToolTip != null)
                {
                    return;
                }

                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];

                DataTable dtTrvData = DataTableConverter.Convert(c1trItem.ParentItemsSource);
                DataRow[] drTargetRow = dtTrvData.Select("INPUT_LOTID = '" + textBolock.Text + "'");
                if (drTargetRow.Length > 0)
                {
                    string sLotMId = textBolock.Text;
                    string sProcId = Util.NVC(drTargetRow[0]["PROCID"]);
                    string sProcName = Util.NVC(drTargetRow[0]["PROCNAME"]);
                    string sEqptId = Util.NVC(drTargetRow[0]["EQPTID"]);
                    string sEqptName = Util.NVC(drTargetRow[0]["EQPTNAME"]);
                    string sActDtTm = Util.NVC(drTargetRow[0]["INPUT_END_DTTM"]);
                    string sLotType = Util.NVC(drTargetRow[0]["INPUT_LOT_TYPE_CODE"]);
                    string sMtrlId = Util.NVC(drTargetRow[0]["MTRLID"]);
                    string sMtrlQty = Util.NVC(drTargetRow[0]["MTRLMBOM_COUNT"]);
                    string sInputSeq = Util.NVC(drTargetRow[0]["PRDT_ATTCH_PSTN_NO"]);
                    string sPrdtClssCode = Util.NVC(drTargetRow[0]["PRDT_CLSS_CODE"]);

                    string sToolTipText = "";
                    if (sLotType == "BOX")
                    {
                        string sPackingType = "";
                        if (sPrdtClssCode == "PLT")
                        {
                            sPackingType = "PALLET";
                        }
                        else if (sPrdtClssCode == "BOX")
                        {
                            sPackingType = "BOX";
                        }

                        sToolTipText = ObjectDic.Instance.GetObjectName("BOX ID") + "   : " + sLotMId + "\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("포장일시") + " : " + sActDtTm + "\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("포장유형") + " : " + sPackingType + "\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("제품ID") + "  : " + sMtrlId + "\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("포장수량") + " : " + sMtrlQty + "\n";
                    }
                    else
                    {
                        //부모LOT은 NULL입력으로 툴팁생성안함..
                        if (sLotType == "")
                        {
                            return;
                        }
                        sToolTipText = ObjectDic.Instance.GetObjectName("Lot ID") + "   : " + sLotMId + "\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("발생공정") + " : " + sProcName + " (" + sProcId + ")\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("발생설비") + " : " + sEqptName + " (" + sEqptId + ")\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("발생일시") + " : " + sActDtTm + "\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("생산유형") + " : " + sLotType + "\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("자재 ID") + "  : " + sMtrlId + "\n";
                        //sToolTipText += ObjectDic.Instance.GetObjectName("수량") + " : " + sMtrlQty + "\n";
                        sToolTipText += ObjectDic.Instance.GetObjectName("투입순서") + " : " + sInputSeq + "\n";

                    }


                    ToolTip tooltip = new ToolTip { Content = sToolTipText };
                    c1trItem.ToolTip = tooltip;
                    c1trItem.MouseLeave += C1trItem_MouseLeave;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setLotInfoText(DataTable dtLotInfo)
        {
            try
            {
                if (dtLotInfo != null)
                {
                    if (dtLotInfo.Rows.Count > 0)
                    {
                        txtLotInfoCreateDate.Text = Util.NVC(dtLotInfo.Rows[0]["LOTDTTM_CR"]);
                        txtLotInfoLotType.Text = Util.NVC(dtLotInfo.Rows[0]["LOTYNAME"]);
                        txtLotInfoProductId.Text = String.IsNullOrWhiteSpace(dtLotInfo.Rows[0]["GRD_PRODID"].ToString()) ? Util.NVC(dtLotInfo.Rows[0]["PRODID"]) : Util.NVC(dtLotInfo.Rows[0]["PRODID"]) + " (" + Util.NVC(dtLotInfo.Rows[0]["GRD_PRODID"]) + ")";
                        txtLotInfoProductId.Tag = Util.NVC(dtLotInfo.Rows[0]["PRODID"]);
                        txtLotInfoProductName.Text = Util.NVC(dtLotInfo.Rows[0]["PRODNAME"]);
                        txtLotInfoWipState.Text = Util.NVC(dtLotInfo.Rows[0]["WIPSNAME"]);
                        txtLotInfoModel.Text = Util.NVC(dtLotInfo.Rows[0]["MODLNAME"]);
                        txtLotInfoWipProcess.Text = Util.NVC(dtLotInfo.Rows[0]["PROCNAME"]);
                        txtLotInfoWipLine.Text = Util.NVC(dtLotInfo.Rows[0]["EQSGNAME"]);
                        txtLotInfoWipLine.Tag = Util.NVC(dtLotInfo.Rows[0]["EQSGID"]);
                        txtCUST_LOTID.Text = Util.NVC(dtLotInfo.Rows[0]["CUST_LOTID"]);
                        lbLotValidation.Content = Util.NVC(dtLotInfo.Rows[0]["LOT_VALID"]);

                        //2019.08.31
                        if (Util.NVC(dtLotInfo.Rows[0]["PRDT_CLSS_CODE"]) == "CELL")
                        {
                            txtCellLIne.Text = Util.NVC(dtLotInfo.Rows[0]["CELL_INPUT_EQSGNAME"]);
                        }
                        else
                        {
                            txtCellLIne.Text = "";
                        }
                    }

                    setLabelCode(Util.NVC(txtLotInfoProductId.Tag));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setLabelCode(string sProdID)
        {
            try
            {
                if (sProdID.Length > 0)
                {
                    CommonCombo _combo = new CommonCombo();
                    //라벨 세팅
                    String[] sFilter = { sProdID, null, null, LABEL_TYPE_CODE.PACK };
                    _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");

                    int combo_cnt = cboLabelCode.Items.Count;

                    for (int i = 0; i < combo_cnt; i++)
                    {
                        DataRowView drv = cboLabelCode.Items[i] as DataRowView;
                        string temp = drv["CBO_CODE"].ToString();

                        if (now_labelcode == temp)
                        {
                            cboLabelCode.SelectedValue = now_labelcode;
                            break;
                        }
                        else
                        {
                            cboLabelCode.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    cboLabelCode.ItemsSource = null;
                    cboLabelCode.SelectedValue = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void clearLotInfoText()
        {
            try
            {
                txtLotInfoCreateDate.Text = "";
                txtLotInfoLotType.Text = "";
                txtLotInfoProductId.Text = "";
                txtLotInfoProductId.Tag = "";
                txtLotInfoProductName.Text = "";
                txtLotInfoWipState.Text = "";
                txtLotInfoModel.Text = "";
                txtLotInfoWipProcess.Text = "";
                txtLotInfoWipLine.Text = "";
                txtLotInfoWipLine.Tag = null;
                txtCUST_LOTID.Text = "";
                lbLotValidation.Content = "";
                txtTagetLotId.Text = "";
                //2019.06.19 김도형 C20190515_94246 : GMES LOT 이력 조회 화면 개선 요청
                txtLGC_LOTID.Text = "";
                txtHoldYN.Text = "";

                Util.gridClear(dgActHistory);
                Util.gridClear(dgDetailData);
                Util.gridClear(dgInspectionData);
                Util.gridClear(dgChangeOffData);
                Util.gridClear(this.dgRackMappingHistory);

                trvKeypartList.ItemsSource = null;

                //2019.08.31
                txtCellLIne.Text = "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void TreeItemExpandAll()
        {
            IList<DependencyObject> item_First = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref item_First);

            foreach (C1TreeViewItem item in item_First)
            {
                //object ob = item.DataContext;
                //if (ob != null)
                //{
                //    string sLotType = Util.NVC(((DataRowView)ob)["INPUT_LOT_TYPE_CODE"]);
                //    string sPrdtClssCode = Util.NVC(((DataRowView)ob)["PRDT_CLSS_CODE"]);
                //    if (sLotType == "BOX" || sLotType == "") //포장 or 조회 LOT인 경우
                //    {
                //        item.Expanding += ChildItem_Expanding;
                //    }
                //    else if (item.ParentItem == null)
                //    {
                //        if (sPrdtClssCode == "CMA" || sPrdtClssCode == "BMA")
                //        {
                //            item.Expanding += ChildItem_Expanding;
                //        }

                //    }
                //}
                item.Expanding += ChildItem_Expanding;
                TreeItemExpandNodes(item);
            }
        }

        private void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);

                foreach (C1TreeViewItem childItem in items)
                {
                    childItem.Expanding += ChildItem_Expanding;

                    TreeItemExpandNodes(childItem);
                }
            }));
        }

        private void ChildItem_Expanding(object sender, SourcedEventArgs e)
        {
            C1TreeViewItem ChildItem = sender as C1TreeViewItem;
            ChildItem.Expanding -= ChildItem_Expanding;

            object ob = ChildItem.DataContext;
            if (ob != null)
            {
                string sLotType = Util.NVC(((DataRowView)ob)["INPUT_LOT_TYPE_CODE"]);
                string sPrdtClssCode = Util.NVC(((DataRowView)ob)["PRDT_CLSS_CODE"]);


                if (sLotType == "BOX")
                {
                    //팔레트인경우 Gray
                    if (sPrdtClssCode == "PLT")
                    {
                        ChildItem.Foreground = new SolidColorBrush(Colors.Gray);
                        ChildItem.FontWeight = FontWeights.Bold;
                    }
                    //BOX인경우 Green
                    else if (sPrdtClssCode == "BOX")
                    {
                        ChildItem.Foreground = new SolidColorBrush(Colors.Green);
                        ChildItem.FontWeight = FontWeights.Bold;
                    }
                }
                /*
                //KETPART LOT 인경우 Blue
                else if (sLotType == "PROD")
                {
                    ChildItem.Foreground = new SolidColorBrush(Colors.Blue);
                    ChildItem.FontWeight = FontWeights.Normal;
                }
                //KETPART 자재 인경우 Brown
                else if (sLotType == "MTRL")
                {
                    ChildItem.Foreground = new SolidColorBrush(Colors.Brown);
                    ChildItem.FontWeight = FontWeights.Normal;
                }
                //조회LOT 인경우 FONT 굵기 BOLD
                else if (sLotType == "")
                {
                    ChildItem.FontWeight = FontWeights.Bold;
                }
                */
                if (sLotType == "" || sLotType == "PROD" || sLotType == "MTRL")
                {
                    ChildItem.FontWeight = FontWeights.Bold;
                }

                if (ChildItem.ParentItem == null)
                {
                    if (sPrdtClssCode == "CMA" || sPrdtClssCode == "BMA")
                    {
                        ChildItem.Foreground = new SolidColorBrush(Colors.Red);
                        ChildItem.FontWeight = FontWeights.Normal;
                    }

                    //2018.12.17
                    if (LoginInfo.CFG_SHOP_ID == "G481")
                    {
                        //2018.11.29 Root 선택
                        if (((DataRowView)ChildItem.DataContext).Row.ItemArray[2].Equals(str_LOTID))
                        {
                            //2020.10.12 염규범S
                            //LOT 이력 연속 조회 필요성의로 해당 내용 주석 처리
                            //ChildItem.IsSelected = true;
                        }
                    }
                }
            }
        }

        private void PopUpOpen(string sMAINFORMPATH, string sMAINFORMNAME)
        {
            Assembly asm = Assembly.LoadFrom("ClientBin\\" + sMAINFORMPATH + ".dll");
            Type targetType = asm.GetType(sMAINFORMPATH + "." + sMAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        /// <summary>
        /// 트리뷰의 text 클립보드에복사
        /// </summary>
        private void copyClipboardTreeView()
        {
            try
            {

                string strAllNodeText = string.Empty;
                //IList<DependencyObject> items = new List<DependencyObject>();
                //VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(C1TreeViewItem), ref items);

                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(trvKeypartList, typeof(TextBlock), ref itemText);

                for (int i = 0; i < itemText.Count; i++)
                {
                    TextBlock textBolock = (TextBlock)itemText[i];
                    strAllNodeText += string.Format("{0} ", i) + textBolock.Text + Environment.NewLine;
                }

                Clipboard.SetText(strAllNodeText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 트리뷰의 선택한 item의 text 클립보드에복사
        /// </summary>
        private void copyClipboardTreeView_Selected()
        {
            try
            {
                C1TreeViewItem c1trItem = trvKeypartList.SelectedItem;

                if (c1trItem == null)
                {

                    return;
                }

                IList<DependencyObject> itemText = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(c1trItem, typeof(TextBlock), ref itemText);
                TextBlock textBolock = (TextBlock)itemText[0];

                Clipboard.SetText(textBolock.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


    }
}
