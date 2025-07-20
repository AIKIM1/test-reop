/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.10.11  이대근 : [CSR ID:3781337] GMES update to Formation Data save, and Printing Pallet Labels | [요청번호]C20180902_81337
  2018.11.26  이대근 : [CSR ID:3832825] [요청] LG전자 부품식별표 개선 요청 | [요청번호]C20181031_31372
  2019.11.23  이제섭 : CNB Pallet 라벨 발행 로직 추가.
  2020.05.07  이현호 : C20200507-000395 CNA Pallet SGM향 라벨 신규 추가.
  2020.09.08  김동일 : C20200831-000213 CNA SGM향 라벨 수정
  2020.09.10  김동일 : C20200831-000213 CNA SGM향 라벨 수정 (ITEM004, 005 에 하이픈 인쇄 추가)
  2021.01.08  이상훈 : C20210106-000471_GMES中Daimler标签打印 Net weight&Gross weight计算逻辑修改/GMES시스템Daimler라벨N.W&G.W계산 로직 수정
  2023.03.22  이병윤 : E20230209-000102 ESNJ_ESS 특이사항 입력 및 라벨에 입력내용 반영기능 추가요청
  2023.07.26  윤지해 : E20230718-000722 ESNA 자동차 조립일 경우 기존에 프린트 이력이 있으면 확인 메세지 팝업
  2023.08.08  윤지해 : E20230718-000722 ESNA 자동차 조립일 경우 라벨 프린트 FUNCTION 호출 추가
  2023.10.23  이병윤 : E20230704-000646 ESNJ_ESS_add barcode of lot&quantity&model in the pallet label
  2024.03.28  임정훈 : E20240310-002025 NA 포장 Pallet 라벨 발행 신규 라벨(LBL0346) 추가
  2024.06.28  박수미 : E20240624-000068 ASSY LOT별 Cell 조회 DA 변경 (Cell수량 내림차순 정렬)
  2024.07.09  임정훈 : E20240624-000371 NB 포장 Pallet 라벨 발행 신규 라벨(LBL0374) 추가
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
using System.Net;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using System.Globalization;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_013 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private const string _COMPLIANCE_IDICATOR1 = "_5B";
        private const string _COMPLIANCE_IDICATOR2 = "_29";
        private const string _COMPLIANCE_IDICATOR3 = "_3E";
        private const string _RECORD_SEPARATOR = "_1E";
        private const string _DATA_FORMAT = "06";
        private const string _GROUP_SEPARATOR = "_1D";
        private const string _END_OF_TRAILER = "_04";
        private const string _CR = "_0A";
        private const string _LF = "_0D";
        private string _LabelCode = string.Empty;
        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;

        string label_code = string.Empty;
        string zpl = string.Empty;
        string ITEM_REF = string.Empty;

        int chkIdx = -1; // E20230209-000102_Pallet 목록에서 TRAY정보 선택시 그리드 Row Index
        string sPalletId = string.Empty; 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_013()
        {
            InitializeComponent();
            this.Loaded += This_Loaded;

        }

        #endregion

        #region Initialize
        private void This_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= This_Loaded;
            InitPage();
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            try
            {
                ////동,라인,공정,설비 셋팅
                CommonCombo _combo = new CommonCombo();


                //HLGP
                //요청구분
                string[] sFilter = { "CELL_LABLE_USETYPE" };
                _combo.SetCombo(cboUseTypeHLGP, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
                cboUseTypeHLGP.SelectedValue = "MASS";

                string[] sFilter1 = { "CELL_LABLE_USETYPE" };
                _combo.SetCombo(cboUseTypeLGE, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter1);
                cboUseTypeLGE.SelectedValue = "MASS";

                //string[] sFilter1 = { "HLGPVOLTAGE_LABEL" };
                //_combo.SetCombo(cboVoltageHLGP, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void InitPage()
        {
            try
            {
                if (LoginInfo.LOGGEDBYSSO == true)
                {
                    txtWorker.Tag = LoginInfo.USERID;
                    txtWorker.Text = LoginInfo.USERNAME;
                }

                lblShipToName.Content = "";
                lblCustType.Content = "";
                lblShipToID.Content = "";

                Util.gridClear(dgPallet);
                Util.gridClear(dgTray);

                InitCombo();

                ClearTextBoxes(grF2);
                ClearTextBoxes(grHLGP);
                ClearTextBoxes(grCMI);
                ClearTextBoxes(grGM);
                ClearTextBoxes(grFORD);
                ClearTextBoxes(grLGE);
                ClearTextBoxes(grDAIM);

                ctF2.Visibility = Visibility.Collapsed;
                ctFORD.Visibility = Visibility.Collapsed;
                ctCMI.Visibility = Visibility.Collapsed;
                ctGM.Visibility = Visibility.Collapsed;
                ctHLGP.Visibility = Visibility.Collapsed;
                ctLGE.Visibility = Visibility.Collapsed;
                ctDAIM.Visibility = Visibility.Collapsed;
                ctCNB.Visibility = Visibility.Collapsed;

                ctDefault.Visibility = Visibility.Visible;

                rdoA4.Visibility = Visibility.Visible;

                txtPallet.Text = "";

                // 프린터 정보 조회
                _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);
                 //   return;
                txtPrintQty.Text = sCopy;

                lblInfo.Text = MessageDic.Instance.GetMessage("SFU1411");

                // E20230209-000102 : shopId G184인경우만 Note버튼 활성화
                string sShopId = String.Empty;
                sShopId = LoginInfo.CFG_SHOP_ID;
                if (sShopId.Equals("G184"))
                {
                    rdoA4.Visibility = Visibility.Collapsed;
                    btnNote.Visibility = Visibility.Visible;
                    (dgPallet.Columns["SKID_NOTE"] as C1.WPF.DataGrid.DataGridTextColumn).Visibility = Visibility.Visible;

                }
                chkIdx = -1; // E20230209-000102_초기화
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        
        #endregion

        #region Event

        #region [팔레트 엔터키]
        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                                    //리스트에 있는지 체크
                                    DataTable dt = DataTableConverter.Convert(dgPallet.ItemsSource);

                                    if (dt.Rows.Count > 0 && dt.Select("PALLETID = '" + txtPallet.Text + "'").Length > 0) //중복조건 체크
                                    {
                                        Util.Alert("SFU1781"); //"이미추가된팔레트입니다."
                                        return;
                                    }

                                    GetPallet();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [팔레트 그리드에서 선택할때 tray 가져오기]
        private void dgPallet_Choice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                return;
                

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                //if ((bool)rb.IsChecked)
                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                    //if (dt != null)
                    //{
                    //    for (int i = 0; i < dt.Rows.Count; i++)
                    //    {
                    //        DataRow row = dt.Rows[i];

                    //        if (idx == i)
                    //            dt.Rows[i]["CHK"] = true;
                    //        else
                    //            dt.Rows[i]["CHK"] = false;
                    //    }
                    //    dgPallet.BeginEdit();
                    //    dgPallet.ItemsSource = DataTableConverter.Convert(dt);
                    //    dgPallet.EndEdit();
                    //}

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    dgPallet.SelectedIndex = idx;
                    chkIdx = idx; // // E20230209-000102_Pallet 목록에서 TRAY정보 선택시 그리드 Row Index 
                    GetTray(DataTableConverter.GetValue(dgPallet.Rows[idx].DataItem, "PALLETID").ToString());

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [출력 버튼]
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            // E20230718-000722 : ESNA 자동차 조립일 경우 프린트 이력이 있을 경우 확인 메세지 팝업 & function으로 분할
            // E20230209-000102 : shopId G184인경우 기존 lblCustType과 상관없이 신규 라벨 발행
            string sShopId = String.Empty;
            sShopId = LoginInfo.CFG_SHOP_ID;

            if (sShopId.Equals("G451"))
            {
                ChkPrintQty(sShopId);
            }
            else
            {
                SetPrintInfo(sShopId);
            }
        }


        #endregion


        #region [그리드내의 제거 버튼]
        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;


                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                if (dg.GetRowCount() == 0) InitPage();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [Note]
        /// <summary>
        /// E20230209-000102_특이사항 입력 팝업창 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNote_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_013_MSG popupNote = new ASSY001_013_MSG { FrameOperation = FrameOperation };

            int seleted_row = chkIdx; // E20230209-000102_Pallet 목록에서 TRAY정보 선택시 그리드 Row Index 
            if (seleted_row < 0)
            {
                return;
            }
            object[] parameters = new object[2];
            parameters[0] = DataTableConverter.GetValue(dgPallet.Rows[seleted_row].DataItem, "PALLETID");
            sPalletId = Convert.ToString(parameters[0]);
            parameters[1] = DataTableConverter.GetValue(dgPallet.Rows[seleted_row].DataItem, "SKID_NOTE");

            C1WindowExtension.SetParameters(popupNote, parameters);

            popupNote.Closed += popupNote_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupNote.ShowModal()));
            
        }

        /// <summary>
        /// E20230209-000102_특이사항 입력 팝업창 닫기 후처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupNote_Closed(object sender, EventArgs e)
        {
            ASSY001_013_MSG popup = sender as ASSY001_013_MSG;
            if (popup != null)
            {

                DataTableConverter.SetValue(dgPallet.Rows[chkIdx].DataItem, "SKID_NOTE", popup.txtSave.Text);

                double sumWidth = dgPallet.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                double sumHeight = dgPallet.ActualHeight;

                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgPallet.Columns)
                    dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                dgPallet.UpdateLayout();
                dgPallet.Measure(new Size(sumWidth, sumHeight));

                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgPallet.Columns)
                    if (dgc.ActualWidth > 0)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);
            }
        }
        #endregion

        #region [초기화]
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InitPage();
                }
            });
        }
        #endregion
        #endregion


        #region Mehod
        #region [팔레트 가져오기]
        private void GetPallet()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PALLETID", typeof(string));
                dtRqst.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!txtRcvIssId.Text.Equals(""))
                {
                    dr["RCV_ISS_ID"] = txtRcvIssId.Text;
                }
                else
                {
                    dr["PALLETID"] = txtPallet.Text;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_PALLET_INFO", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1994"); //"팔레트 정보가 없습니다."
                }
                //스캔한 팔레트가 있는지 체크해서 동일한 출하처 팔레트만 가능하도록 처리
                else if (dgPallet.GetRowCount() > 0 && !dtRslt.Rows[0]["SHIPTO_ID"].ToString().Equals(lblShipToID.Content))
                {
                    Util.MessageValidation("SFU1503"); //"동일 출하처 팔레트가 아닙니다."
                }
                else if (dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["LABEL_CODE"].ToString() == "")
                {
                    Util.MessageValidation("SFU1520"); //"라벨 기준정보가 등록되지 않은 제품 입니다."
                }
                else if (dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["CUST_TYPE"].ToString() == "")
                {
                    Util.MessageValidation("SFU1996"); //"포장 라벨 고객 유형이 등록되지 않은 제품 입니다."
                }
                else if (dgPallet.GetRowCount() > 0 && dtRslt.Rows[0]["LABEL_CODE"].ToString() == "")
                {

                }
                else
                {
                    if (dgPallet.GetRowCount() == 0)
                    {
                        //dgPallet.ItemsSource = DataTableConverter.Convert(dtRslt);
                        Util.GridSetData(dgPallet, dtRslt, FrameOperation, true);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgPallet.ItemsSource);
                        dtSource.Merge(dtRslt);

                        //    Util.gridClear(dgPallet);
                        //     dgPallet.ItemsSource = DataTableConverter.Convert(dtSource);
                        Util.GridSetData(dgPallet, dtSource, FrameOperation, true);

                    }
                    txtPallet.Text = "";
                    txtPallet.Focus();

                    if (lblCustType.Content.Equals(""))
                    {
                        SetShipTo(dtRslt.Rows[0]["CUST_TYPE"].ToString(), dtRslt.Rows[0]["SHIPTO_ID"].ToString(), dtRslt.Rows[0]["LABEL_CODE"].ToString(), dtRslt.Rows[0]["PRODID"].ToString(), dtRslt.Rows[0]["SHIPDATE"].ToString(), dtRslt.Rows[0]["SHIPTO_NAME"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

}

        private void GetPallet(string PalletID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PALLETID", typeof(string));
                dtRqst.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!txtRcvIssId.Text.Equals(""))
                {
                    dr["RCV_ISS_ID"] = txtRcvIssId.Text;
                }
                else
                {
                    dr["PALLETID"] = PalletID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_PALLET_INFO", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1994"); //"팔레트 정보가 없습니다."
                }
                //스캔한 팔레트가 있는지 체크해서 동일한 출하처 팔레트만 가능하도록 처리
                else if (dgPallet.GetRowCount() > 0 && !dtRslt.Rows[0]["SHIPTO_ID"].ToString().Equals(lblShipToID.Content))
                {
                    Util.MessageValidation("SFU1503"); //"동일 출하처 팔레트가 아닙니다."
                }
                else if (dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["LABEL_CODE"].ToString() == "")
                {
                    Util.MessageValidation("SFU1520"); //"라벨 기준정보가 등록되지 않은 제품 입니다."
                }
                else if (dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["CUST_TYPE"].ToString() == "")
                {
                    Util.MessageValidation("SFU1996"); //"포장 라벨 고객 유형이 등록되지 않은 제품 입니다."
                }
                else if (dgPallet.GetRowCount() > 0 && dtRslt.Rows[0]["LABEL_CODE"].ToString() == "")
                {

                }
                else
                {
                    if (dgPallet.GetRowCount() == 0)
                    {
                        //dgPallet.ItemsSource = DataTableConverter.Convert(dtRslt);
                        Util.GridSetData(dgPallet, dtRslt, FrameOperation, true);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgPallet.ItemsSource);
                        dtSource.Merge(dtRslt);

                        //    Util.gridClear(dgPallet);
                        //     dgPallet.ItemsSource = DataTableConverter.Convert(dtSource);
                        Util.GridSetData(dgPallet, dtSource, FrameOperation, true);

                    }
                    txtPallet.Text = "";
                    txtPallet.Focus();

                    if (lblCustType.Content.Equals(""))
                    {
                        SetShipTo(dtRslt.Rows[0]["CUST_TYPE"].ToString(), dtRslt.Rows[0]["SHIPTO_ID"].ToString(), dtRslt.Rows[0]["LABEL_CODE"].ToString(), dtRslt.Rows[0]["PRODID"].ToString(), dtRslt.Rows[0]["SHIPDATE"].ToString(), dtRslt.Rows[0]["SHIPTO_NAME"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        #endregion

        #region [최초팔레트정보로출하처셋팅]
        private void SetShipTo(string sCustType, string sShipToId, string sLabelCode, string sProdId, string sShipDate, string sShiptoName)
        {
            try
            {
                ctF2.Visibility = Visibility.Collapsed;
                ctFORD.Visibility = Visibility.Collapsed;
                ctGM.Visibility = Visibility.Collapsed;
                ctHLGP.Visibility = Visibility.Collapsed;
                ctLGE.Visibility = Visibility.Collapsed;
                ctDefault.Visibility = Visibility.Collapsed;
                bdListContain.Visibility = Visibility.Collapsed;

                lblShipToName.FontWeight = FontWeights.Bold;
                lblShipToName.FontFamily = new FontFamily("Arial Black");
                lblShipToName.FontSize = 26;
                lblCustType.Content = sCustType;
                lblShipToID.Content = sShipToId;

                //item 정보 조회
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SHIPTO_ID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SHIPTO_ID"] = sShipToId;
                dr["PRODID"] = sProdId;
                dr["LABEL_CODE"] = _LabelCode = sLabelCode;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_MNGT_ITEM", "INDATA", "OUTDATA", dtRqst);

                if (sCustType.Equals("F2"))//F2
                {
                    lblShipToName.Content = "F2";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.DarkRed);
                    ctF2.Visibility = Visibility.Visible;
                    ctF2.IsSelected = true;

                    rdoA4.Visibility = Visibility.Visible;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;

                    SetTextBoxes(grF2, dtRslt);

                    string sVolt = (dtRslt.Select("ITEM_CODE='VOLT_RANGE'").Length > 0) ? dtRslt.Select("ITEM_CODE='VOLT_RANGE'")[0]["ITEM_VALUE"].ToString() : "";
                    string[] sVoltArray = sVolt.Split('|');
                    cboVoltageF2.ItemsSource = sVoltArray;
                    if (cboVoltageF2.Items.Count > 0) cboVoltageF2.SelectedIndex = 0;
                }

                else if (sCustType.Equals("CMI"))//CMI
                {
                    lblShipToName.Content = "CMI";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.DarkRed);
                    ctCMI.Visibility = Visibility.Visible;
                    ctCMI.IsSelected = true;

                    rdoA4.Visibility = Visibility.Collapsed;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;

                    SetTextBoxes(grCMI, dtRslt);
                }

                else if (sCustType.Equals("GM"))//GM
                {
                    lblShipToName.Content = "GM";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.DarkRed);
                    ctGM.Visibility = Visibility.Visible;
                    ctGM.IsSelected = true;

                    rdoA4.Visibility = Visibility.Collapsed;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;


                    SetTextBoxes(grGM, dtRslt);
                }
                else if (sCustType.Equals("FORD"))//FORD
                {
                    lblShipToName.Content = "FORD";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.Blue);
                    ctFORD.Visibility = Visibility.Visible;
                    ctFORD.IsSelected = true;

                    rdoA4.Visibility = Visibility.Collapsed;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;

                    SetTextBoxes(grFORD, dtRslt);

                }
                else if (sCustType.Equals("HLGP")) //HLGP
                {
                    lblShipToName.Content = "HLGP";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    ctHLGP.Visibility = Visibility.Visible;
                    ctHLGP.IsSelected = true;
                    bdListContain.Visibility = Visibility.Visible;

                    rdoA4.Visibility = Visibility.Visible;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;

                    SetTextBoxes(grHLGP, dtRslt);

                    //HLGP VOLT
                    string sVolt = (dtRslt.Select("ITEM_CODE='VOLT_RANGE'").Length > 0) ? dtRslt.Select("ITEM_CODE='VOLT_RANGE'")[0]["ITEM_VALUE"].ToString() : "";
                    string[] sVoltArray = sVolt.Split('|');
                    cboVoltageHLGP.ItemsSource = sVoltArray;
                    if (cboVoltageHLGP.Items.Count > 0) cboVoltageHLGP.SelectedIndex = 0;
                }
                else if (sCustType.Equals("LGE")) //LGE
                {
                    lblShipToName.Content = sShiptoName;
                    lblShipToName.Foreground = new SolidColorBrush(Colors.Red);
                    ctLGE.Visibility = Visibility.Visible;
                    ctLGE.IsSelected = true;
                    bdListContain.Visibility = Visibility.Visible;

                    rdoA4.Visibility = Visibility.Visible;
                    rdoBarcode.Visibility = Visibility.Collapsed;
                    rdoA4.IsChecked = true;

                    if (LoginInfo.CFG_SHOP_ID == "A040")
                    {
                        ChkNone.Visibility = Visibility.Visible;
                        ChkSample.Visibility = Visibility.Visible;
                        ChkToTal.Visibility = Visibility.Visible;
                        rdoPass.Visibility = Visibility.Visible;
                        rdoNoPass.Visibility = Visibility.Visible;
                        rdoBarcode.Visibility = Visibility.Visible;

                    }



                    SetTextBoxes(grLGE, dtRslt);

                    //LGE VOLT
                    string sVolt = (dtRslt.Select("ITEM_CODE='VOLT_RANGE'").Length > 0) ? dtRslt.Select("ITEM_CODE='VOLT_RANGE'")[0]["ITEM_VALUE"].ToString() : "";
                    string[] sVoltArray = sVolt.Split('|');
                    cboVoltageLGE.ItemsSource = sVoltArray;
                    if (cboVoltageLGE.Items.Count > 0) cboVoltageLGE.SelectedIndex = 0;
                }
                else if (sCustType.Equals("DAIMLER")) //Daimler
                {
                    lblShipToName.Content = "Daimler";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.Green);

                    ctDAIM.Visibility = Visibility.Visible;
                    ctDAIM.IsSelected = true;

                    rdoA4.Visibility = Visibility.Visible;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;

                    SetTextBoxes(grDAIM, dtRslt);
                }
                else if (sCustType.Equals("CWA")) //CWA
                {
                    lblShipToName.Content = "CWA";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.DarkRed);

                    rdoA4.Visibility = Visibility.Visible;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;
                }
                else if (sCustType.Equals("CNB")) //CNB
                {
                    lblShipToName.Content = "CNB";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.Green);

                    ctCNB.Visibility = Visibility.Visible;
                    ctCNB.IsSelected = true;

                    txtnetweight.IsReadOnly = true;
                    txtgrossweight.IsReadOnly = true;
                    txtratedpower.IsReadOnly = true;

                    rdoA4.Visibility = Visibility.Visible;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;

                    SetTextBoxes(grCNB, dtRslt);
                }
                else if (sCustType.Equals("SGM")) // SGM 2020.05.07
                {
                    lblShipToName.Content = "SGM";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.Green);

                    ctCNB.Visibility = Visibility.Visible;
                    ctCNB.IsSelected = true;

                    txtnetweight.IsReadOnly = true;
                    txtgrossweight.IsReadOnly = true;
                    txtratedpower.IsReadOnly = true;

                    rdoA4.Visibility = Visibility.Visible;
                    rdoBarcode.Visibility = Visibility.Visible;
                    rdoBarcode.IsChecked = true;

                    SetTextBoxes(grSGM, dtRslt);
                }
                else
                {
                    lblShipToName.Content = "NOT EXISTS";
                    lblShipToName.Foreground = new SolidColorBrush(Colors.Black);
                }

                if (sShipDate != "")
                {
                    dtShip.SelectedDateTime = DateTime.Parse(sShipDate);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

}
        #endregion

        #region [트레이정보 가져오기]
        private void GetTray(string sPalletId)
        {
            try
            { 
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PALLETID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = sPalletId;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_BYPALLETID_CP", "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgTray);
               // dgTray.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgTray, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
}
        #endregion

        #region [실제 출력 바코드]
        private void PrintHLGP(string sPrt, string sRes, string sXpos, string sYpos)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));
                dtRqst.Columns.Add("ATTVAL010", typeof(string));
                dtRqst.Columns.Add("ATTVAL011", typeof(string));
                dtRqst.Columns.Add("ATTVAL012", typeof(string));
                dtRqst.Columns.Add("ATTVAL013", typeof(string));
                dtRqst.Columns.Add("ATTVAL014", typeof(string));
                dtRqst.Columns.Add("ATTVAL015", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                string sUseType = Util.GetCondition(cboUseTypeHLGP, "SFU1655"); // 용도는필수입니다. >> 선택된항목이없습니다.
                if (sUseType.Equals("")) return;
                sUseType = cboUseTypeHLGP.Text;
                sUseType = sUseType.Replace(cboUseTypeHLGP.SelectedValue.ToString() + " : ", "");

                //string sVol = Util.GetCondition(cboVoltageHLGP, "출하전압는필수입니다.");
                //if (sVol.Equals("")) return;
                //sVol = cboVoltageHLGP.Text;
                //sVol = sVol.Replace(cboVoltageHLGP.SelectedValue.ToString() + " : ", "");

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {

                    dtRqst.Rows[0]["LBCD"] = _LabelCode; //"LBL0008"; //HLGP 향
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;
                    dtRqst.Rows[0]["ATTVAL001"] = "■ 용도 (" + sUseType + ")";//용도
                    dtRqst.Rows[0]["ATTVAL002"] = txtCarTypeHLGP.Text;//차종
                    dtRqst.Rows[0]["ATTVAL003"] = txtPartCodeHLGP.Text;//품번
                    dtRqst.Rows[0]["ATTVAL004"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "MODLID")); ;//부품명
                    dtRqst.Rows[0]["ATTVAL005"] = cboVoltageHLGP.Text;//출하전압
                    dtRqst.Rows[0]["ATTVAL006"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")) + " EA";//수량
                    dtRqst.Rows[0]["ATTVAL007"] = txtCompanyNameHLGP.Text;//업체명
                    dtRqst.Rows[0]["ATTVAL008"] = txtCustomerNameHLGP.Text;//장소
                    dtRqst.Rows[0]["ATTVAL009"] = "LINE" + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "LINE_NO"));//라인
                    dtRqst.Rows[0]["ATTVAL010"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE"));//포장일자
                    dtRqst.Rows[0]["ATTVAL011"] = dtShip.SelectedDateTime.ToString("yyyy-MM-dd"); //출하일자
                    dtRqst.Rows[0]["ATTVAL012"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));//Pallet ID BCD
                    dtRqst.Rows[0]["ATTVAL013"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));//Pallet ID
                    dtRqst.Rows[0]["ATTVAL014"] = dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageHLGP.Text.ToUpper().Replace(" ", "");//전압 BCD
                    dtRqst.Rows[0]["ATTVAL015"] = dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageHLGP.Text.ToUpper().Replace(" ", ""); //전압

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    {

                        DataRow drLog = dtLog.NewRow();

                        drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                        drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                        drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                        drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                        drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                        drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                        drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                        drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                        drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                        drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ATTVAL007"];
                        drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ATTVAL008"];
                        drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ATTVAL009"];
                        drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ATTVAL010"];
                        drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ATTVAL011"];
                        drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ATTVAL012"];
                        drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ATTVAL013"];
                        drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ATTVAL014"];
                        drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ATTVAL015"];
                        drLog["INSUSER"] = txtWorker.Tag as string; // LoginInfo.USERID;

                        dtLog.Rows.Add(drLog);

                        PrintLog(dtLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        private void PrintDAIMLER(string sPrt, string sRes, string sXpos, string sYpos)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));
                dtRqst.Columns.Add("ATTVAL010", typeof(string));
                dtRqst.Columns.Add("ATTVAL011", typeof(string));
                dtRqst.Columns.Add("ATTVAL012", typeof(string));
                dtRqst.Columns.Add("ATTVAL013", typeof(string));
                dtRqst.Columns.Add("ATTVAL014", typeof(string));
                dtRqst.Columns.Add("ATTVAL015", typeof(string));
                dtRqst.Columns.Add("ATTVAL016", typeof(string));
                dtRqst.Columns.Add("ATTVAL017", typeof(string));
                dtRqst.Columns.Add("ATTVAL018", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                Double dCellWeight = 0;
                Double dTotalWeight = 0;

                string sShipDate = dtShip.SelectedDateTime.ToString("yyMMdd");

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    dCellWeight = 0;
                    dTotalWeight = 0;
                    //C20210106-000471 계산식 변경
                    //dCellWeight = Math.Round(Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"))) * Convert.ToDouble(txtCellWeightDAIM.Text), 0) + 0.5;
                    //dTotalWeight = Math.Round(dCellWeight + Convert.ToDouble(txtPalletWeightDAIM.Text), 0);

                    //소숫점 2자리까지 표시하고 3자리부터 버림으로 처리함.
                    dCellWeight = Math.Truncate(Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"))) * Convert.ToDouble(txtCellWeightDAIM.Text) * 100 ) / 100;
                    dTotalWeight = Math.Truncate((dCellWeight + Convert.ToDouble(txtPalletWeightDAIM.Text)) * 100) / 100;

                    dtRqst.Rows[0]["LBCD"] = _LabelCode; //"LBL0006"; //DAIMLER 향
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;
                    dtRqst.Rows[0]["ATTVAL001"] = txtReceiverDAIM.Text;
                    dtRqst.Rows[0]["ATTVAL002"] = txtDockGateDAIM.Text;
                    dtRqst.Rows[0]["ATTVAL003"] = txtAdviceNoteNoDAIM.Text + sShipDate;
                    dtRqst.Rows[0]["ATTVAL004"] = txtAdviceNoteNoDAIM.Text + sShipDate;
                    dtRqst.Rows[0]["ATTVAL005"] = txtSupplierDAIM.Text;

                    //C20210106-000471 toint32 사용시 소수점 1자리에서 반올림 처리 하므로 제거함.
                    //dtRqst.Rows[0]["ATTVAL006"] = Convert.ToInt32(dCellWeight).ToString();//셀무게 * 수량 (Cell1개 무게는 모델별 Default Value로 관리)
                    //dtRqst.Rows[0]["ATTVAL007"] = Convert.ToInt32(dTotalWeight.ToString()).ToString();//셀무게 * 수량 + Pallet무게 (Pallet무게는 모델별 Default Value로 관리)
                    dtRqst.Rows[0]["ATTVAL006"] = dCellWeight.ToString();//셀무게 * 수량 (Cell1개 무게는 모델별 Default Value로 관리)
                    dtRqst.Rows[0]["ATTVAL007"] = dTotalWeight.ToString();//셀무게 * 수량 + Pallet무게 (Pallet무게는 모델별 Default Value로 관리)

                    dtRqst.Rows[0]["ATTVAL008"] = txtNoOfBoxesDAIM.Text;
                    dtRqst.Rows[0]["ATTVAL009"] = txtPartNumberDAIM.Text;
                    dtRqst.Rows[0]["ATTVAL010"] = txtPartNumberDAIM.Text;
                    dtRqst.Rows[0]["ATTVAL011"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));
                    dtRqst.Rows[0]["ATTVAL012"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));
                    dtRqst.Rows[0]["ATTVAL013"] = txtDescriptionDAIM.Text;
                    dtRqst.Rows[0]["ATTVAL014"] = txtSupplierIdDAIM.Text;
                    dtRqst.Rows[0]["ATTVAL015"] = txtSupplierIdDAIM.Text;
                    dtRqst.Rows[0]["ATTVAL016"] = "D" + sShipDate;
                    dtRqst.Rows[0]["ATTVAL017"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    dtRqst.Rows[0]["ATTVAL018"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    {

                        DataRow drLog = dtLog.NewRow();

                        drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                        drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                        drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                        drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                        drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                        drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                        drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                        drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                        drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                        drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ATTVAL007"];
                        drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ATTVAL008"];
                        drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ATTVAL009"];
                        drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ATTVAL010"];
                        drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ATTVAL011"];
                        drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ATTVAL012"];
                        drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ATTVAL013"];
                        drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ATTVAL014"];
                        drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ATTVAL015"];
                        drLog["PRT_ITEM16"] = dtRqst.Rows[0]["ATTVAL016"];
                        drLog["PRT_ITEM17"] = dtRqst.Rows[0]["ATTVAL017"];
                        drLog["PRT_ITEM18"] = dtRqst.Rows[0]["ATTVAL018"];
                        drLog["INSUSER"] = txtWorker.Tag as string; //LoginInfo.USERID;

                        dtLog.Rows.Add(drLog);

                        PrintLog(dtLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintFORD(string sPrt, string sRes, string sXpos, string sYpos)
        {
            try
            {

                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));
                dtRqst.Columns.Add("ATTVAL010", typeof(string));
                dtRqst.Columns.Add("ATTVAL011", typeof(string));
                dtRqst.Columns.Add("ATTVAL012", typeof(string));
                dtRqst.Columns.Add("ATTVAL013", typeof(string));
                dtRqst.Columns.Add("ATTVAL014", typeof(string));
                dtRqst.Columns.Add("ATTVAL015", typeof(string));
                dtRqst.Columns.Add("ATTVAL016", typeof(string));
                dtRqst.Columns.Add("ATTVAL017", typeof(string));
                dtRqst.Columns.Add("ATTVAL018", typeof(string));
                dtRqst.Columns.Add("ATTVAL019", typeof(string));
                dtRqst.Columns.Add("ATTVAL020", typeof(string));
                dtRqst.Columns.Add("ATTVAL021", typeof(string));
                dtRqst.Columns.Add("ATTVAL022", typeof(string));
                dtRqst.Columns.Add("ATTVAL023", typeof(string));
                dtRqst.Columns.Add("ATTVAL024", typeof(string));
                dtRqst.Columns.Add("ATTVAL025", typeof(string));
                dtRqst.Columns.Add("ATTVAL026", typeof(string));
                dtRqst.Columns.Add("ATTVAL027", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                Double dCellWeight = 0;
                Double dTotalWeight = 0;



                string sShipDate = String.Format(CultureInfo.GetCultureInfo("en-US"), "{0:yyMMdd}", dtShip.SelectedDateTime).ToUpper();
                string sShipDate1 = String.Format(CultureInfo.GetCultureInfo("en-US"), "{0:ddMMMyyyy}", dtShip.SelectedDateTime).ToUpper();
                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {

                    string _2DBCD = String.Format(
                                                    _COMPLIANCE_IDICATOR1.ToString() +
                                                    _COMPLIANCE_IDICATOR2.ToString() +
                                                    _COMPLIANCE_IDICATOR3.ToString() +
                                                    _RECORD_SEPARATOR.ToString() +
                                                    _DATA_FORMAT +
                                                    _GROUP_SEPARATOR +
                                                    "P{0}" + _GROUP_SEPARATOR.ToString() +
                                                    "Q{1}" + _GROUP_SEPARATOR.ToString() +
                                                    "V{2}" + _GROUP_SEPARATOR.ToString() +
                                                    "D{3}" + _GROUP_SEPARATOR.ToString() +
                                                    "S{4}" + _GROUP_SEPARATOR.ToString() +
                                                    _END_OF_TRAILER.ToString(),
                                                    //----------------------------------------------------------------------------------------------------------
                                                    txtCustPartNoFORD.Text,
                                                    Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")),
                                                    txtSupplierCodeFORD.Text,
                                                    sShipDate,
                                                    Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"))
                                                );

                    dCellWeight = 0;
                    dTotalWeight = 0;
                    //DECODE(GP.QTY, 540, 871, TRUNC((179.3 + GP.QTY * 0.39) / 0.4535)) AS WEIGHT,
                    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")).Equals("540"))
                    {
                        dCellWeight = 871;
                    }
                    else
                    {
                        dCellWeight = (179.3 + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"))) * 0.39) / 0.4535;
                    }

                    DataTable dtIndata = new DataTable();
                    dtIndata.Columns.Add("PALLETID", typeof(string));

                    DataRow drPalletId = dtIndata.NewRow();
                    drPalletId["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    dtIndata.Rows.Add(drPalletId);

                    DataTable dtJulianCode = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_JULIAN_CODE_BY_CELL", "INDATA", "OUTDATA", dtIndata);
                    //dTotalWeight = dCellWeight + Convert.ToDouble(txtPalletWeightDAIM.Text);

                    dtRqst.Rows[0]["LBCD"] = _LabelCode; //"LBL0005"; //FORD 향
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;
                    dtRqst.Rows[0]["ATTVAL001"] = txtSupplierNameFORD.Text;
                    dtRqst.Rows[0]["ATTVAL002"] = txtSupplierCodeFORD.Text;
                    dtRqst.Rows[0]["ATTVAL003"] = txtSupplierCodeFORD.Text;
                    dtRqst.Rows[0]["ATTVAL004"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));
                    dtRqst.Rows[0]["ATTVAL005"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));
                    dtRqst.Rows[0]["ATTVAL006"] = txtContainerFORD.Text;
                    dtRqst.Rows[0]["ATTVAL007"] = Math.Round(dCellWeight, 0).ToString();
                    dtRqst.Rows[0]["ATTVAL008"] = sShipDate1;
                    dtRqst.Rows[0]["ATTVAL009"] = _2DBCD;
                    dtRqst.Rows[0]["ATTVAL010"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "MODLID"));
                    dtRqst.Rows[0]["ATTVAL011"] = txtWattHourRateFORD.Text + "wh";
                    dtRqst.Rows[0]["ATTVAL012"] = txtCustPartNoFORD.Text;
                    dtRqst.Rows[0]["ATTVAL013"] = txtCustPartNoFORD.Text;
                    dtRqst.Rows[0]["ATTVAL014"] = txtStrLoc1FORD.Text;
                    dtRqst.Rows[0]["ATTVAL015"] = txtLineFeedLoc2FORD.Text;
                    dtRqst.Rows[0]["ATTVAL016"] = txtSupplierPartFORD.Text;//Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PRODID"));
                    dtRqst.Rows[0]["ATTVAL017"] = txtCustPartNoFORD.Text;//txtSupplierPartFORD.Text;                    
                    dtRqst.Rows[0]["ATTVAL019"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    dtRqst.Rows[0]["ATTVAL020"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    dtRqst.Rows[0]["ATTVAL021"] = txtCustPlantFORD.Text;
                    dtRqst.Rows[0]["ATTVAL022"] = txtDockCodeFORD.Text;
                    dtRqst.Rows[0]["ATTVAL023"] = txtCustCodeFORD.Text;
                    dtRqst.Rows[0]["ATTVAL018"] = string.Empty;
                    dtRqst.Rows[0]["ATTVAL024"] = string.Empty;
                    dtRqst.Rows[0]["ATTVAL025"] = string.Empty;
                    dtRqst.Rows[0]["ATTVAL026"] = string.Empty;
                    dtRqst.Rows[0]["ATTVAL027"] = string.Empty;

                    if (dtJulianCode.Rows.Count > 0)
                        dtRqst.Rows[0]["ATTVAL018"] = dtJulianCode.Rows[0]["JULIAN_CODE"].ToString() + "[" + dtJulianCode.Rows[0]["CELL_CNT"].ToString() + "]";
                    if (dtJulianCode.Rows.Count > 1)
                        dtRqst.Rows[0]["ATTVAL024"] = dtJulianCode.Rows[1]["JULIAN_CODE"].ToString() + "[" + dtJulianCode.Rows[1]["CELL_CNT"].ToString() + "]";
                    if (dtJulianCode.Rows.Count > 2)
                        dtRqst.Rows[0]["ATTVAL025"] = dtJulianCode.Rows[2]["JULIAN_CODE"].ToString() + "[" + dtJulianCode.Rows[2]["CELL_CNT"].ToString() + "]";
                    if (dtJulianCode.Rows.Count > 3)
                        dtRqst.Rows[0]["ATTVAL026"] = dtJulianCode.Rows[3]["JULIAN_CODE"].ToString() + "[" + dtJulianCode.Rows[3]["CELL_CNT"].ToString() + "]";
                    if (dtJulianCode.Rows.Count > 4)
                        dtRqst.Rows[0]["ATTVAL027"] = dtJulianCode.Rows[4]["JULIAN_CODE"].ToString() + "[" + dtJulianCode.Rows[4]["CELL_CNT"].ToString() + "]";

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    {
                        DataRow drLog = dtLog.NewRow();

                        drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                        drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                        drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                        drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                        drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                        drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                        drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                        drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                        drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                        drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ATTVAL007"];
                        drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ATTVAL008"];
                        drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ATTVAL009"];
                        drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ATTVAL010"];
                        drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ATTVAL011"];
                        drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ATTVAL012"];
                        drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ATTVAL013"];
                        drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ATTVAL014"];
                        drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ATTVAL015"];
                        drLog["PRT_ITEM16"] = dtRqst.Rows[0]["ATTVAL016"];
                        drLog["PRT_ITEM17"] = dtRqst.Rows[0]["ATTVAL017"];
                        drLog["PRT_ITEM18"] = dtRqst.Rows[0]["ATTVAL018"];
                        drLog["PRT_ITEM19"] = dtRqst.Rows[0]["ATTVAL019"];
                        drLog["PRT_ITEM20"] = dtRqst.Rows[0]["ATTVAL020"];
                        drLog["PRT_ITEM21"] = dtRqst.Rows[0]["ATTVAL021"];
                        drLog["PRT_ITEM22"] = dtRqst.Rows[0]["ATTVAL022"];
                        drLog["PRT_ITEM23"] = dtRqst.Rows[0]["ATTVAL023"];
                        drLog["PRT_ITEM24"] = dtRqst.Rows[0]["ATTVAL024"];
                        drLog["PRT_ITEM25"] = dtRqst.Rows[0]["ATTVAL025"];
                        drLog["PRT_ITEM26"] = dtRqst.Rows[0]["ATTVAL026"];
                        drLog["PRT_ITEM27"] = dtRqst.Rows[0]["ATTVAL027"];
                        drLog["INSUSER"] = txtWorker.Tag as string;  //LoginInfo.USERID;

                        dtLog.Rows.Add(drLog);

                        PrintLog(dtLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintF2(string sPrt, string sRes,string sXpos, string sYpos)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    dtRqst.Rows[0]["LBCD"] = _LabelCode;
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;
                    dtRqst.Rows[0]["ATTVAL001"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE"));

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    {

                        DataRow drLog = dtLog.NewRow();

                        drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                        drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                        drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                        drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                        drLog["INSUSER"] = txtWorker.Tag as string; //LoginInfo.USERID;

                        dtLog.Rows.Add(drLog);



                        PrintLog(dtLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintGM(string sPrt, string sRes, string sXpos, string sYpos)
        {


            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));
                dtRqst.Columns.Add("ATTVAL010", typeof(string));
                dtRqst.Columns.Add("ATTVAL011", typeof(string));
                dtRqst.Columns.Add("ATTVAL012", typeof(string));
                dtRqst.Columns.Add("ATTVAL013", typeof(string));
                dtRqst.Columns.Add("ATTVAL014", typeof(string));
                dtRqst.Columns.Add("ATTVAL015", typeof(string));
                dtRqst.Columns.Add("ATTVAL016", typeof(string));
                dtRqst.Columns.Add("ATTVAL017", typeof(string));
                dtRqst.Columns.Add("ATTVAL018", typeof(string));
                dtRqst.Columns.Add("ATTVAL019", typeof(string));
                dtRqst.Columns.Add("ATTVAL020", typeof(string));
                dtRqst.Columns.Add("ATTVAL021", typeof(string));
                dtRqst.Columns.Add("ATTVAL022", typeof(string));
                dtRqst.Columns.Add("ATTVAL023", typeof(string));
                dtRqst.Columns.Add("ATTVAL024", typeof(string));
                dtRqst.Columns.Add("ATTVAL025", typeof(string));
                dtRqst.Columns.Add("ATTVAL026", typeof(string));
                dtRqst.Columns.Add("ATTVAL027", typeof(string));
                dtRqst.Columns.Add("ATTVAL028", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                Double dCellWeight = 0;
                Double dTotalWeight = 0;
                string sLicense = "";


                string sShipDate1 = String.Format(CultureInfo.GetCultureInfo("en-US"), "{0:ddMMMyyyy}", dtShip.SelectedDateTime).ToUpper();



                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    DateTime dOCV2_Date;
                    string sOCV2_Date = string.Empty;
                    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")) != string.Empty)
                    {
                        dOCV2_Date = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")));
                        sOCV2_Date = "OCV DATE: " + string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:ddMMMyyyy}", dOCV2_Date).ToString().ToUpper(); //.ToUpper().ToString();
                    }

                    //if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_WRK_TYPE_CODE")).Equals("UI"))
                    //// EQ 이면 T UI 이면 B 로 보시면 되요 
                    ////'1J UN' || GL.DUNCODE || ' ' || DECODE(GP.LOT_TYPE, 'T', GP.PALLETID, SUBSTR(GP.PALLETID, 1, 2) || SUBSTR(GP.PALLETID, 4, 7)) AS LICENSE,        --LICENSE PLATE
                    //{
                    //    sLicense = "1J UN" + txtDuneCodeGM.Text + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Substring(0,2) + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Substring(3,7);
                    //}
                    //else
                    //{
                    //    sLicense = "1J UN" + txtDuneCodeGM.Text + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    //}

                    sLicense = "1J UN" + txtDuneCodeGM.Text + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));

                    string _2DBCD = _COMPLIANCE_IDICATOR1.ToString() +
                                                  _COMPLIANCE_IDICATOR2.ToString() +
                                                  _COMPLIANCE_IDICATOR3.ToString() +
                                                  _RECORD_SEPARATOR.ToString() +
                                                  _DATA_FORMAT +
                                                  _GROUP_SEPARATOR +
                                                  "P{0}" + _GROUP_SEPARATOR.ToString() +
                                                  "Q{1}" + _GROUP_SEPARATOR.ToString() +
                                                  "1J{2}" + _GROUP_SEPARATOR.ToString() +
                                                  "20L{3}" + _GROUP_SEPARATOR.ToString() +
                                                  "21L{4}" + _GROUP_SEPARATOR.ToString() +
                                                  "B{5}" + _GROUP_SEPARATOR.ToString() +
                                                  "7Q{6}GT" + _RECORD_SEPARATOR.ToString() +
                                                  _END_OF_TRAILER.ToString();


                    _2DBCD = String.Format(_2DBCD,
                                                  //----------------------------------------------------------------------------------------------------------
                                                  txtPartNumberGM.Text,
                                                  Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")),
                                                  "UN" + sLicense.Substring(5, 10).Trim(),
                                                  txtMaterialGM.Text,
                                                  txtPlantGM.Text + " " + txtDockGM.Text,
                                                  txtContainerGM.Text,
                                                  txtGrossWeightGM.Text);

                    //string _2DBCD = String.Format(_COMPLIANCE_IDICATOR1.ToString() +
                    //                              _COMPLIANCE_IDICATOR2.ToString() +
                    //                              _COMPLIANCE_IDICATOR3.ToString() +
                    //                              _RECORD_SEPARATOR.ToString() +
                    //                              _DATA_FORMAT +
                    //                              _GROUP_SEPARATOR +
                    //                              "P{0}" + _GROUP_SEPARATOR.ToString() +
                    //                              "Q{1}" + _GROUP_SEPARATOR.ToString() +
                    //                              "1J{2}" + _GROUP_SEPARATOR.ToString() +
                    //                              "20L{3}" + _GROUP_SEPARATOR.ToString() +
                    //                              "21L{4}" + _GROUP_SEPARATOR.ToString() +
                    //                              "B{5}" + _GROUP_SEPARATOR.ToString() +
                    //                              "7Q{6}GT" + _RECORD_SEPARATOR.ToString() +
                    //                              _END_OF_TRAILER.ToString(),
                    //                              //----------------------------------------------------------------------------------------------------------
                    //                              txtPartNumberGM.Text,
                    //                              Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")),
                    //                              "UN" + sLicense.Substring(5, 10).Trim(),
                    //                              txtMaterialGM.Text,
                    //                              txtPlantGM.Text + " " + txtDockGM.Text,
                    //                              txtContainerGM.Text,
                    //                              txtGrossWeightGM.Text);
                    //"LINE NO");

                    dCellWeight = 0;
                    dTotalWeight = 0;


                    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_WRK_TYPE_CODE")).Equals("EQ") && !Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")).Equals(txtQuantityGM.Text))
                    {
                        dCellWeight = (179.3 + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"))) * 0.39);
                    }
                    else
                    {
                        dCellWeight = Convert.ToDouble(txtGrossWeightGM.Text);
                    }


                    dtRqst.Rows[0]["LBCD"] = _LabelCode; //"LBL0007";
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;
                    dtRqst.Rows[0]["ATTVAL001"] = "";//"29\r\n GWAHAKSANEOP 3 - RO\r\nOKSAN - MYEON\r\nCHEONGWON CHUNGBUK\r\nKOREA";
                    dtRqst.Rows[0]["ATTVAL002"] = "";//"20001 BROWNSTOWN CENTER\nDRIVE BUILDING 10\r\nBROWNSTOWN, MI 48183\r\nUSA";
                    dtRqst.Rows[0]["ATTVAL003"] = txtPlantGM.Text + " " + txtDockGM.Text;
                    dtRqst.Rows[0]["ATTVAL004"] = "";
                    dtRqst.Rows[0]["ATTVAL005"] = _2DBCD;//2D BCD
                    dtRqst.Rows[0]["ATTVAL006"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));
                    dtRqst.Rows[0]["ATTVAL007"] = txtMaterialGM.Text;
                    dtRqst.Rows[0]["ATTVAL008"] = txtReferenceGM.Text;
                    dtRqst.Rows[0]["ATTVAL009"] = txtPartNumberGM.Text;
                    dtRqst.Rows[0]["ATTVAL010"] = txtWattHourGM.Text + " wh";
                    dtRqst.Rows[0]["ATTVAL011"] = sLicense.Substring(0, 3).Trim() + " " + sLicense.Substring(3, 12).Trim() + " " + sLicense.Substring(15).Trim();
                    dtRqst.Rows[0]["ATTVAL012"] = "UN" + " " + txtDuneCodeGM.Text + " " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")); //sLicense.Substring(5);
                    dtRqst.Rows[0]["ATTVAL013"] = sShipDate1;
                    dtRqst.Rows[0]["ATTVAL014"] = txtContainerGM.Text;
                    dtRqst.Rows[0]["ATTVAL015"] = Math.Round(dCellWeight).ToString() + " KG";
                    dtRqst.Rows[0]["ATTVAL016"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    dtRqst.Rows[0]["ATTVAL017"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    dtRqst.Rows[0]["ATTVAL018"] = sOCV2_Date;
                    dtRqst.Rows[0]["ATTVAL019"] = "LINE #" + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "LINE_NO"));
                    dtRqst.Rows[0]["ATTVAL020"] = "LG CHEM MICHIGAN";
                    dtRqst.Rows[0]["ATTVAL021"] = "1 LG WAY";
                    dtRqst.Rows[0]["ATTVAL022"] = "HOLLAND MI 49423";
                    dtRqst.Rows[0]["ATTVAL023"] = "USA";
                    dtRqst.Rows[0]["ATTVAL024"] = "MADE IN USA";
                    dtRqst.Rows[0]["ATTVAL025"] = "20001 BROWNSTOWN CENTER";
                    dtRqst.Rows[0]["ATTVAL026"] = "DRIVE BUILDING 10";
                    dtRqst.Rows[0]["ATTVAL027"] = "BROWNSTOWN, MI 48183";
                    dtRqst.Rows[0]["ATTVAL028"] = "USA";


                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    {

                        DataRow drLog = dtLog.NewRow();

                        drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                        drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                        drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                        drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                        drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                        drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                        drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                        drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                        drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                        drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ATTVAL007"];
                        drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ATTVAL008"];
                        drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ATTVAL009"];
                        drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ATTVAL010"];
                        drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ATTVAL011"];
                        drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ATTVAL012"];
                        drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ATTVAL013"];
                        drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ATTVAL014"];
                        drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ATTVAL015"];
                        drLog["PRT_ITEM16"] = dtRqst.Rows[0]["ATTVAL016"];
                        drLog["PRT_ITEM17"] = dtRqst.Rows[0]["ATTVAL017"];
                        drLog["PRT_ITEM18"] = dtRqst.Rows[0]["ATTVAL018"];
                        drLog["PRT_ITEM19"] = dtRqst.Rows[0]["ATTVAL019"];
                        drLog["PRT_ITEM20"] = dtRqst.Rows[0]["ATTVAL020"];
                        drLog["PRT_ITEM21"] = dtRqst.Rows[0]["ATTVAL021"];
                        drLog["PRT_ITEM22"] = dtRqst.Rows[0]["ATTVAL022"];
                        drLog["PRT_ITEM23"] = dtRqst.Rows[0]["ATTVAL023"];
                        drLog["PRT_ITEM24"] = dtRqst.Rows[0]["ATTVAL024"];
                        drLog["PRT_ITEM25"] = dtRqst.Rows[0]["ATTVAL025"];
                        drLog["PRT_ITEM26"] = dtRqst.Rows[0]["ATTVAL026"];
                        drLog["PRT_ITEM27"] = dtRqst.Rows[0]["ATTVAL027"];
                        drLog["PRT_ITEM28"] = dtRqst.Rows[0]["ATTVAL028"];
                        drLog["INSUSER"] = txtWorker.Tag as string;  //LoginInfo.USERID;

                        dtLog.Rows.Add(drLog);

                        PrintLog(dtLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintCMI(string sPrt, string sRes, string sXpos, string sYpos)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));
                dtRqst.Columns.Add("ATTVAL010", typeof(string));
                dtRqst.Columns.Add("ATTVAL011", typeof(string));
                dtRqst.Columns.Add("ATTVAL012", typeof(string));
                dtRqst.Columns.Add("ATTVAL013", typeof(string));
                dtRqst.Columns.Add("ATTVAL014", typeof(string));
                dtRqst.Columns.Add("ATTVAL015", typeof(string));
                dtRqst.Columns.Add("ATTVAL016", typeof(string));
                dtRqst.Columns.Add("ATTVAL017", typeof(string));
                dtRqst.Columns.Add("ATTVAL018", typeof(string));
                dtRqst.Columns.Add("ATTVAL019", typeof(string));
                dtRqst.Columns.Add("ATTVAL020", typeof(string));
                dtRqst.Columns.Add("ATTVAL021", typeof(string));
                dtRqst.Columns.Add("ATTVAL022", typeof(string));
                dtRqst.Columns.Add("ATTVAL023", typeof(string));
                dtRqst.Columns.Add("ATTVAL024", typeof(string));
                dtRqst.Columns.Add("ATTVAL025", typeof(string));
                dtRqst.Columns.Add("ATTVAL026", typeof(string));
                dtRqst.Columns.Add("ATTVAL027", typeof(string));
                dtRqst.Columns.Add("ATTVAL028", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                Double dCellWeight = 0;
                Double dTotalWeight = 0;
                string sLicense = "";


                string sShipDate1 = String.Format(CultureInfo.GetCultureInfo("en-US"), "{0:ddMMMyyyy}", dtShip.SelectedDateTime).ToUpper();



                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    DateTime dOCV2_Date;
                    string sOCV2_Date = string.Empty;
                    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")) != string.Empty)
                    {
                        dOCV2_Date = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")));
                        sOCV2_Date = "OCV DATE: " + string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:ddMMMyyyy}", dOCV2_Date).ToString().ToUpper(); //.ToUpper().ToString();
                    }

                    //if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_WRK_TYPE_CODE")).Equals("UI"))
                    //// EQ 이면 T UI 이면 B 로 보시면 되요 
                    ////'1J UN' || GL.DUNCODE || ' ' || DECODE(GP.LOT_TYPE, 'T', GP.PALLETID, SUBSTR(GP.PALLETID, 1, 2) || SUBSTR(GP.PALLETID, 4, 7)) AS LICENSE,        --LICENSE PLATE
                    //{
                    //    sLicense = "1J UN" + txtDuneCodeGM.Text + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Substring(0,2) + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Substring(3,7);
                    //}
                    //else
                    //{
                    //    sLicense = "1J UN" + txtDuneCodeGM.Text + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    //}

                    sLicense = "1J UN" + txtDuneCodeCMI.Text + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));

                    string _2DBCD = _COMPLIANCE_IDICATOR1.ToString() +
                                                  _COMPLIANCE_IDICATOR2.ToString() +
                                                  _COMPLIANCE_IDICATOR3.ToString() +
                                                  _RECORD_SEPARATOR.ToString() +
                                                  _DATA_FORMAT +
                                                  _GROUP_SEPARATOR +
                                                  "P{0}" + _GROUP_SEPARATOR.ToString() +
                                                  "Q{1}" + _GROUP_SEPARATOR.ToString() +
                                                  "1J{2}" + _GROUP_SEPARATOR.ToString() +
                                                  "20L{3}" + _GROUP_SEPARATOR.ToString() +
                                                  "21L{4}" + _GROUP_SEPARATOR.ToString() +
                                                  "B{5}" + _GROUP_SEPARATOR.ToString() +
                                                  "7Q{6}GT" + _RECORD_SEPARATOR.ToString() +
                                                  _END_OF_TRAILER.ToString();


                    _2DBCD = String.Format(_2DBCD,
                                                  //----------------------------------------------------------------------------------------------------------
                                                  txtPartNumberCMI.Text,
                                                  Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")),
                                                  "UN" + sLicense.Substring(5, 10).Trim(),
                                                  txtMaterialCMI.Text,
                                                  txtPlantCMI.Text + " " + txtDockCMI.Text,
                                                  txtContainerCMI.Text,
                                                  txtGrossWeightCMI.Text);

                    //string _2DBCD = String.Format(_COMPLIANCE_IDICATOR1.ToString() +
                    //                              _COMPLIANCE_IDICATOR2.ToString() +
                    //                              _COMPLIANCE_IDICATOR3.ToString() +
                    //                              _RECORD_SEPARATOR.ToString() +
                    //                              _DATA_FORMAT +
                    //                              _GROUP_SEPARATOR +
                    //                              "P{0}" + _GROUP_SEPARATOR.ToString() +
                    //                              "Q{1}" + _GROUP_SEPARATOR.ToString() +
                    //                              "1J{2}" + _GROUP_SEPARATOR.ToString() +
                    //                              "20L{3}" + _GROUP_SEPARATOR.ToString() +
                    //                              "21L{4}" + _GROUP_SEPARATOR.ToString() +
                    //                              "B{5}" + _GROUP_SEPARATOR.ToString() +
                    //                              "7Q{6}GT" + _RECORD_SEPARATOR.ToString() +
                    //                              _END_OF_TRAILER.ToString(),
                    //                              //----------------------------------------------------------------------------------------------------------
                    //                              txtPartNumberGM.Text,
                    //                              Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")),
                    //                              "UN" + sLicense.Substring(5, 10).Trim(),
                    //                              txtMaterialGM.Text,
                    //                              txtPlantGM.Text + " " + txtDockGM.Text,
                    //                              txtContainerGM.Text,
                    //                              txtGrossWeightGM.Text);
                    //"LINE NO");

                    dCellWeight = 0;
                    dTotalWeight = 0;


                    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_WRK_TYPE_CODE")).Equals("EQ") && !Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")).Equals(txtQuantityCMI.Text))
                    {
                        dCellWeight = (179.3 + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"))) * 0.39);
                    }
                    else
                    {
                        dCellWeight = Convert.ToDouble(txtGrossWeightCMI.Text);
                    }


                    dtRqst.Rows[0]["LBCD"] = _LabelCode; // "LBL0059" CMI
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;

                    dtRqst.Rows[0]["ATTVAL001"] = "LG CHEM MICHIGAN"; //  "29";
                    dtRqst.Rows[0]["ATTVAL002"] = "1 LG WAY"; //" GWAHAKSANEOP 3-RO";
                    dtRqst.Rows[0]["ATTVAL003"] = "HOLLAND MI 49423"; //"OKSAN-MYEON";
                    dtRqst.Rows[0]["ATTVAL004"] = "USA"; //"CHEONGWON CHUNGBUK";
                    dtRqst.Rows[0]["ATTVAL005"] = "MADE IN USA"; //"KOREA";

                    // 출하처별 변경사항 (TO ADDR)
                    dtRqst.Rows[0]["ATTVAL006"] = txtToAddr1CMI.Text;
                    dtRqst.Rows[0]["ATTVAL007"] = txtToAddr2CMI.Text;
                    dtRqst.Rows[0]["ATTVAL008"] = txtToAddr3CMI.Text;
                    dtRqst.Rows[0]["ATTVAL009"] = txtToAddr4CMI.Text;

                    dtRqst.Rows[0]["ATTVAL010"] = txtPlantCMI.Text + " " + txtDockCMI.Text;

                    dtRqst.Rows[0]["ATTVAL011"] = _2DBCD;//2D BCD

                    dtRqst.Rows[0]["ATTVAL012"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));
                    dtRqst.Rows[0]["ATTVAL013"] = txtMaterialCMI.Text;
                    dtRqst.Rows[0]["ATTVAL014"] = txtReferenceCMI.Text;

                    dtRqst.Rows[0]["ATTVAL015"] = txtPartNumberCMI.Text;

                    dtRqst.Rows[0]["ATTVAL016"] = sLicense.Substring(0, 3).Trim() + " " + sLicense.Substring(3, 12).Trim() + " " + sLicense.Substring(15).Trim();
                    dtRqst.Rows[0]["ATTVAL017"] = "UN" + " " + txtDuneCodeCMI.Text + " " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")); //sLicense.Substring(5);

                    dtRqst.Rows[0]["ATTVAL018"] = sShipDate1;
                    dtRqst.Rows[0]["ATTVAL019"] = txtContainerCMI.Text;
                    dtRqst.Rows[0]["ATTVAL020"] = Math.Round(dCellWeight).ToString() + " KG";

                    dtRqst.Rows[0]["ATTVAL021"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    dtRqst.Rows[0]["ATTVAL022"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));

                    dtRqst.Rows[0]["ATTVAL023"] = sOCV2_Date;
                    dtRqst.Rows[0]["ATTVAL024"] = "HOLLAND LINE #" + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "LINE_NO"));


                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    {

                        DataRow drLog = dtLog.NewRow();

                        drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                        drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                        drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                        drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                        drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                        drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                        drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                        drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                        drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                        drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ATTVAL007"];
                        drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ATTVAL008"];
                        drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ATTVAL009"];
                        drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ATTVAL010"];
                        drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ATTVAL011"];
                        drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ATTVAL012"];
                        drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ATTVAL013"];
                        drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ATTVAL014"];
                        drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ATTVAL015"];
                        drLog["PRT_ITEM16"] = dtRqst.Rows[0]["ATTVAL016"];
                        drLog["PRT_ITEM17"] = dtRqst.Rows[0]["ATTVAL017"];
                        drLog["PRT_ITEM18"] = dtRqst.Rows[0]["ATTVAL018"];
                        drLog["PRT_ITEM19"] = dtRqst.Rows[0]["ATTVAL019"];
                        drLog["PRT_ITEM20"] = dtRqst.Rows[0]["ATTVAL020"];
                        drLog["PRT_ITEM21"] = dtRqst.Rows[0]["ATTVAL021"];
                        drLog["PRT_ITEM22"] = dtRqst.Rows[0]["ATTVAL022"];
                        drLog["PRT_ITEM23"] = dtRqst.Rows[0]["ATTVAL023"];
                        drLog["PRT_ITEM24"] = dtRqst.Rows[0]["ATTVAL024"];
                        drLog["PRT_ITEM25"] = dtRqst.Rows[0]["ATTVAL025"];
                        drLog["PRT_ITEM26"] = dtRqst.Rows[0]["ATTVAL026"];
                        drLog["PRT_ITEM27"] = dtRqst.Rows[0]["ATTVAL027"];
                        drLog["PRT_ITEM28"] = dtRqst.Rows[0]["ATTVAL028"];
                        drLog["INSUSER"] = txtWorker.Tag as string;  //LoginInfo.USERID;

                        dtLog.Rows.Add(drLog);

                        PrintLog(dtLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintCWA(string sPrt, string sRes, string sXpos, string sYpos)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ITEM001", typeof(string));
                dtRqst.Columns.Add("ITEM002", typeof(string));
                dtRqst.Columns.Add("ITEM003", typeof(string));
                dtRqst.Columns.Add("ITEM004", typeof(string));
                dtRqst.Columns.Add("ITEM005", typeof(string));
                dtRqst.Columns.Add("ITEM006", typeof(string));
                dtRqst.Columns.Add("ITEM007", typeof(string));
                dtRqst.Columns.Add("ITEM008", typeof(string));
                dtRqst.Columns.Add("ITEM009", typeof(string));
                dtRqst.Columns.Add("ITEM010", typeof(string));
                dtRqst.Columns.Add("ITEM011", typeof(string));
                dtRqst.Columns.Add("ITEM012", typeof(string));
                dtRqst.Columns.Add("ITEM013", typeof(string));
                dtRqst.Columns.Add("ITEM014", typeof(string));
                dtRqst.Columns.Add("ITEM015", typeof(string));
                dtRqst.Columns.Add("ITEM016", typeof(string));
                dtRqst.Columns.Add("ITEM017", typeof(string));
                dtRqst.Columns.Add("ITEM018", typeof(string));
                dtRqst.Columns.Add("ITEM019", typeof(string));
                dtRqst.Columns.Add("ITEM020", typeof(string));
                dtRqst.Columns.Add("ITEM021", typeof(string));
                dtRqst.Columns.Add("ITEM022", typeof(string));
                dtRqst.Columns.Add("ITEM023", typeof(string));
                dtRqst.Columns.Add("ITEM024", typeof(string));
                dtRqst.Columns.Add("ITEM025", typeof(string));
                dtRqst.Columns.Add("ITEM026", typeof(string));
                dtRqst.Columns.Add("ITEM027", typeof(string));
                dtRqst.Columns.Add("ITEM028", typeof(string));
                dtRqst.Columns.Add("ITEM029", typeof(string));
                dtRqst.Columns.Add("ITEM030", typeof(string));
                dtRqst.Columns.Add("ITEM031", typeof(string));
                dtRqst.Columns.Add("ITEM032", typeof(string));
                dtRqst.Columns.Add("ITEM033", typeof(string));
                dtRqst.Columns.Add("ITEM034", typeof(string));
                dtRqst.Columns.Add("ITEM035", typeof(string));
                dtRqst.Columns.Add("ITEM036", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);
                
                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    dtRqst.Rows[0]["LBCD"] = _LabelCode; // "LBL0181","LBL0346" CWA
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;
                    if (_LabelCode== "LBL0346")
                    {
                        dtRqst.Rows[0]["ITEM001"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        dtRqst.Rows[0]["ITEM002"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        dtRqst.Rows[0]["ITEM003"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE"));
                        DataTable dtRqst1 = new DataTable();
                        dtRqst1.Columns.Add("PALLETID", typeof(string));
                        dtRqst1.Columns.Add("RCV_ISS_ID", typeof(string));
                        dtRqst1.Columns.Add("BOXID", typeof(string));

                        DataRow dr1 = dtRqst1.NewRow();
                        dtRqst1.Rows.Add(dr1);
                        dtRqst1.Rows[0]["PALLETID"] = dtRqst.Rows[0]["ITEM002"];
                        dtRqst1.Rows[0]["RCV_ISS_ID"] = null;
                        dtRqst1.Rows[0]["BOXID"] = null;

                        DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SUM_BYPALLETID_CELLQTY", "INDATA", "OUTDATA", dtRqst1);
                        //Assy lot 별 cell 수량 내림차순 조회 DA로 변경

                        int arraySize = 50; 
                        string[] lotId = new string[arraySize];
                        string[] cellQTY = new string[arraySize];
                        for(int j = 0; j < arraySize; j++)
                        {
                            lotId[j] = "";
                            cellQTY[j] = "";

                        }
                        
                        for (int j=0;j< dtRslt1.Rows.Count; j++)
                        {
                            lotId[j] = Util.NVC(dtRslt1.Rows[j]["LOTID"]).ToString();
                            cellQTY[j] = Util.NVC(dtRslt1.Rows[j]["CELLQTY"]).ToString();
                        }
                        dtRqst.Rows[0]["ITEM004"] = lotId[0];
                        dtRqst.Rows[0]["ITEM005"] = lotId[1];
                        dtRqst.Rows[0]["ITEM006"] = lotId[2];
                        dtRqst.Rows[0]["ITEM007"] = lotId[3];
                        dtRqst.Rows[0]["ITEM008"] = lotId[4];
                        dtRqst.Rows[0]["ITEM009"] = lotId[5];
                        dtRqst.Rows[0]["ITEM010"] = lotId[6];
                        dtRqst.Rows[0]["ITEM011"] = lotId[7];
                        dtRqst.Rows[0]["ITEM012"] = lotId[8];
                        dtRqst.Rows[0]["ITEM013"] = lotId[9];
                        dtRqst.Rows[0]["ITEM014"] = lotId[10];
                        dtRqst.Rows[0]["ITEM015"] = lotId[11];
                        dtRqst.Rows[0]["ITEM016"] = lotId[12];
                        dtRqst.Rows[0]["ITEM017"] = lotId[13];
                        dtRqst.Rows[0]["ITEM018"] = lotId[14];
                        dtRqst.Rows[0]["ITEM019"] = lotId[15];
                        dtRqst.Rows[0]["ITEM020"] = cellQTY[0];
                        dtRqst.Rows[0]["ITEM021"] = cellQTY[1];
                        dtRqst.Rows[0]["ITEM022"] = cellQTY[2];
                        dtRqst.Rows[0]["ITEM023"] = cellQTY[3];
                        dtRqst.Rows[0]["ITEM024"] = cellQTY[4];
                        dtRqst.Rows[0]["ITEM025"] = cellQTY[5];
                        dtRqst.Rows[0]["ITEM026"] = cellQTY[6];
                        dtRqst.Rows[0]["ITEM027"] = cellQTY[7];
                        dtRqst.Rows[0]["ITEM028"] = cellQTY[8];
                        dtRqst.Rows[0]["ITEM029"] = cellQTY[9];
                        dtRqst.Rows[0]["ITEM030"] = cellQTY[10];
                        dtRqst.Rows[0]["ITEM031"] = cellQTY[11];
                        dtRqst.Rows[0]["ITEM032"] = cellQTY[12];
                        dtRqst.Rows[0]["ITEM033"] = cellQTY[13];
                        dtRqst.Rows[0]["ITEM034"] = cellQTY[14];
                        dtRqst.Rows[0]["ITEM035"] = cellQTY[15];
                        dtRqst.Rows[0]["ITEM036"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "LINE_NO"));
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM36", "INDATA", "OUTDATA", dtRqst);
                        if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                        {
                            DataRow drLog = dtLog.NewRow();

                            drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                            drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                            drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                            drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                            drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ITEM001"];
                            drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ITEM002"];
                            drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ITEM003"];
                            drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ITEM004"];
                            drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ITEM005"];
                            drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ITEM006"];
                            drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ITEM007"];
                            drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ITEM008"];
                            drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ITEM009"];
                            drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ITEM010"];
                            drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ITEM011"];
                            drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ITEM012"];
                            drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ITEM013"];
                            drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ITEM014"];
                            drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ITEM015"];
                            drLog["PRT_ITEM16"] = dtRqst.Rows[0]["ITEM016"];
                            drLog["PRT_ITEM17"] = dtRqst.Rows[0]["ITEM017"];
                            drLog["PRT_ITEM18"] = dtRqst.Rows[0]["ITEM018"];
                            drLog["PRT_ITEM19"] = dtRqst.Rows[0]["ITEM019"];
                            drLog["PRT_ITEM20"] = dtRqst.Rows[0]["ITEM020"];
                            drLog["PRT_ITEM21"] = dtRqst.Rows[0]["ITEM021"];
                            drLog["PRT_ITEM22"] = dtRqst.Rows[0]["ITEM022"];
                            drLog["PRT_ITEM23"] = dtRqst.Rows[0]["ITEM023"];
                            drLog["PRT_ITEM24"] = dtRqst.Rows[0]["ITEM024"];
                            drLog["PRT_ITEM25"] = dtRqst.Rows[0]["ITEM025"];
                            drLog["PRT_ITEM26"] = dtRqst.Rows[0]["ITEM026"];
                            drLog["PRT_ITEM27"] = dtRqst.Rows[0]["ITEM027"];
                            drLog["PRT_ITEM28"] = dtRqst.Rows[0]["ITEM028"];
                            drLog["PRT_ITEM29"] = dtRqst.Rows[0]["ITEM029"];
                            drLog["PRT_ITEM30"] = dtRqst.Rows[0]["ITEM030"];
                            drLog["PRT_ITEM31"] = dtRqst.Rows[0]["ITEM031"];
                            drLog["PRT_ITEM32"] = dtRqst.Rows[0]["ITEM032"];
                            drLog["PRT_ITEM33"] = dtRqst.Rows[0]["ITEM033"];
                            drLog["PRT_ITEM34"] = dtRqst.Rows[0]["ITEM034"];
                            drLog["PRT_ITEM35"] = dtRqst.Rows[0]["ITEM035"];
                            drLog["PRT_ITEM36"] = dtRqst.Rows[0]["ITEM036"];
                            drLog["INSUSER"] = txtWorker.Tag as string; //LoginInfo.USERID;

                            dtLog.Rows.Add(drLog);

                            PrintLog(dtLog);
                        }
                    }
                    else//LBL0181
                    {
                        dtRqst.Rows[0]["ATTVAL001"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));
                        dtRqst.Rows[0]["ATTVAL002"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE"));
                        dtRqst.Rows[0]["ATTVAL003"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);
                        if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                        {
                            DataRow drLog = dtLog.NewRow();

                            drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                            drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                            drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                            drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                            drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                            drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                            drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                            drLog["INSUSER"] = txtWorker.Tag as string; //LoginInfo.USERID;

                            dtLog.Rows.Add(drLog);

                            PrintLog(dtLog);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void PrintCNB(string sPrt, string sRes, string sXpos, string sYpos)
        {
            try
            {
                // 상세정보입력 체크박스 선택 시 
                if (chkDetail.IsChecked == true && _LabelCode != "LBL0374")//라벨코드가 신규 라벨이 아닌 경우 로직 추가
                {
                    // 중량 정보 입력 확인
                    if (String.IsNullOrWhiteSpace(txtnetweight.Text.ToString().Trim()) || String.IsNullOrWhiteSpace(txtgrossweight.Text.ToString().Trim()))
                    {
                        // 포장 중량을 입력하세요.
                        Util.MessageInfo("SFU4390");
                        return;
                    }
                    // 출력 정보 입력 확인
                    if (String.IsNullOrWhiteSpace(txtratedpower.Text.ToString().Trim()))
                    {
                        // Rated Power를입력하세요.
                        Util.MessageInfo("SFU8121");
                        return;
                    }
                }

                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ITEM001", typeof(string));
                dtRqst.Columns.Add("ITEM002", typeof(string));
                dtRqst.Columns.Add("ITEM003", typeof(string));
                dtRqst.Columns.Add("ITEM004", typeof(string));
                dtRqst.Columns.Add("ITEM005", typeof(string));
                dtRqst.Columns.Add("ITEM006", typeof(string));
                dtRqst.Columns.Add("ITEM007", typeof(string));
                dtRqst.Columns.Add("ITEM008", typeof(string));
                dtRqst.Columns.Add("ITEM009", typeof(string));
                dtRqst.Columns.Add("ITEM010", typeof(string));
                dtRqst.Columns.Add("ITEM011", typeof(string));
                dtRqst.Columns.Add("ITEM012", typeof(string));
                dtRqst.Columns.Add("ITEM013", typeof(string));
                dtRqst.Columns.Add("ITEM014", typeof(string));
                dtRqst.Columns.Add("ITEM015", typeof(string));
                dtRqst.Columns.Add("ITEM016", typeof(string));
                dtRqst.Columns.Add("ITEM017", typeof(string));
                dtRqst.Columns.Add("ITEM018", typeof(string));
                dtRqst.Columns.Add("ITEM019", typeof(string));
                dtRqst.Columns.Add("ITEM020", typeof(string));
                dtRqst.Columns.Add("ITEM021", typeof(string));
                dtRqst.Columns.Add("ITEM022", typeof(string));
                dtRqst.Columns.Add("ITEM023", typeof(string));
                dtRqst.Columns.Add("ITEM024", typeof(string));
                dtRqst.Columns.Add("ITEM025", typeof(string));
                dtRqst.Columns.Add("ITEM026", typeof(string));
                dtRqst.Columns.Add("ITEM027", typeof(string));
                dtRqst.Columns.Add("ITEM028", typeof(string));
                dtRqst.Columns.Add("ITEM029", typeof(string));
                dtRqst.Columns.Add("ITEM030", typeof(string));
                dtRqst.Columns.Add("ITEM031", typeof(string));
                dtRqst.Columns.Add("ITEM032", typeof(string));
                dtRqst.Columns.Add("ITEM033", typeof(string));
                dtRqst.Columns.Add("ITEM034", typeof(string));
                dtRqst.Columns.Add("ITEM035", typeof(string));
                dtRqst.Columns.Add("ITEM036", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    dtRqst.Rows[0]["LBCD"] = _LabelCode;
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;
                    //여기서 분기
                    if (_LabelCode == "LBL0374")
                    {
                        dtRqst.Rows[0]["ITEM001"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        dtRqst.Rows[0]["ITEM002"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        dtRqst.Rows[0]["ITEM003"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE"));
                        DataTable dtRqst1 = new DataTable();
                        dtRqst1.Columns.Add("PALLETID", typeof(string));
                        dtRqst1.Columns.Add("RCV_ISS_ID", typeof(string));
                        dtRqst1.Columns.Add("BOXID", typeof(string));

                        DataRow dr1 = dtRqst1.NewRow();
                        dtRqst1.Rows.Add(dr1);
                        dtRqst1.Rows[0]["PALLETID"] = dtRqst.Rows[0]["ITEM002"];
                        dtRqst1.Rows[0]["RCV_ISS_ID"] = null;
                        dtRqst1.Rows[0]["BOXID"] = null;

                        DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SUM_BYPALLETID_CELLQTY", "INDATA", "OUTDATA", dtRqst1);

                        int arraySize = 50;
                        string[] lotId = new string[arraySize];
                        string[] cellQTY = new string[arraySize];
                        for (int j = 0; j < arraySize; j++)
                        {
                            lotId[j] = "";
                            cellQTY[j] = "";

                        }

                        for (int j = 0; j < dtRslt1.Rows.Count; j++)
                        {
                            lotId[j] = Util.NVC(dtRslt1.Rows[j]["LOTID"]).ToString();
                            cellQTY[j] = Util.NVC(dtRslt1.Rows[j]["CELLQTY"]).ToString();
                        }
                        dtRqst.Rows[0]["ITEM004"] = lotId[0];
                        dtRqst.Rows[0]["ITEM005"] = lotId[1];
                        dtRqst.Rows[0]["ITEM006"] = lotId[2];
                        dtRqst.Rows[0]["ITEM007"] = lotId[3];
                        dtRqst.Rows[0]["ITEM008"] = lotId[4];
                        dtRqst.Rows[0]["ITEM009"] = lotId[5];
                        dtRqst.Rows[0]["ITEM010"] = lotId[6];
                        dtRqst.Rows[0]["ITEM011"] = lotId[7];
                        dtRqst.Rows[0]["ITEM012"] = lotId[8];
                        dtRqst.Rows[0]["ITEM013"] = lotId[9];
                        dtRqst.Rows[0]["ITEM014"] = lotId[10];
                        dtRqst.Rows[0]["ITEM015"] = lotId[11];
                        dtRqst.Rows[0]["ITEM016"] = lotId[12];
                        dtRqst.Rows[0]["ITEM017"] = lotId[13];
                        dtRqst.Rows[0]["ITEM018"] = lotId[14];
                        dtRqst.Rows[0]["ITEM019"] = lotId[15];
                        dtRqst.Rows[0]["ITEM020"] = cellQTY[0];
                        dtRqst.Rows[0]["ITEM021"] = cellQTY[1];
                        dtRqst.Rows[0]["ITEM022"] = cellQTY[2];
                        dtRqst.Rows[0]["ITEM023"] = cellQTY[3];
                        dtRqst.Rows[0]["ITEM024"] = cellQTY[4];
                        dtRqst.Rows[0]["ITEM025"] = cellQTY[5];
                        dtRqst.Rows[0]["ITEM026"] = cellQTY[6];
                        dtRqst.Rows[0]["ITEM027"] = cellQTY[7];
                        dtRqst.Rows[0]["ITEM028"] = cellQTY[8];
                        dtRqst.Rows[0]["ITEM029"] = cellQTY[9];
                        dtRqst.Rows[0]["ITEM030"] = cellQTY[10];
                        dtRqst.Rows[0]["ITEM031"] = cellQTY[11];
                        dtRqst.Rows[0]["ITEM032"] = cellQTY[12];
                        dtRqst.Rows[0]["ITEM033"] = cellQTY[13];
                        dtRqst.Rows[0]["ITEM034"] = cellQTY[14];
                        dtRqst.Rows[0]["ITEM035"] = cellQTY[15];
                        dtRqst.Rows[0]["ITEM036"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "LINE_NO"));
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM36", "INDATA", "OUTDATA", dtRqst);
                        if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                        {
                            DataRow drLog = dtLog.NewRow();

                            drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                            drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                            drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                            drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                            drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ITEM001"];
                            drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ITEM002"];
                            drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ITEM003"];
                            drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ITEM004"];
                            drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ITEM005"];
                            drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ITEM006"];
                            drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ITEM007"];
                            drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ITEM008"];
                            drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ITEM009"];
                            drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ITEM010"];
                            drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ITEM011"];
                            drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ITEM012"];
                            drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ITEM013"];
                            drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ITEM014"];
                            drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ITEM015"];
                            drLog["PRT_ITEM16"] = dtRqst.Rows[0]["ITEM016"];
                            drLog["PRT_ITEM17"] = dtRqst.Rows[0]["ITEM017"];
                            drLog["PRT_ITEM18"] = dtRqst.Rows[0]["ITEM018"];
                            drLog["PRT_ITEM19"] = dtRqst.Rows[0]["ITEM019"];
                            drLog["PRT_ITEM20"] = dtRqst.Rows[0]["ITEM020"];
                            drLog["PRT_ITEM21"] = dtRqst.Rows[0]["ITEM021"];
                            drLog["PRT_ITEM22"] = dtRqst.Rows[0]["ITEM022"];
                            drLog["PRT_ITEM23"] = dtRqst.Rows[0]["ITEM023"];
                            drLog["PRT_ITEM24"] = dtRqst.Rows[0]["ITEM024"];
                            drLog["PRT_ITEM25"] = dtRqst.Rows[0]["ITEM025"];
                            drLog["PRT_ITEM26"] = dtRqst.Rows[0]["ITEM026"];
                            drLog["PRT_ITEM27"] = dtRqst.Rows[0]["ITEM027"];
                            drLog["PRT_ITEM28"] = dtRqst.Rows[0]["ITEM028"];
                            drLog["PRT_ITEM29"] = dtRqst.Rows[0]["ITEM029"];
                            drLog["PRT_ITEM30"] = dtRqst.Rows[0]["ITEM030"];
                            drLog["PRT_ITEM31"] = dtRqst.Rows[0]["ITEM031"];
                            drLog["PRT_ITEM32"] = dtRqst.Rows[0]["ITEM032"];
                            drLog["PRT_ITEM33"] = dtRqst.Rows[0]["ITEM033"];
                            drLog["PRT_ITEM34"] = dtRqst.Rows[0]["ITEM034"];
                            drLog["PRT_ITEM35"] = dtRqst.Rows[0]["ITEM035"];
                            drLog["PRT_ITEM36"] = dtRqst.Rows[0]["ITEM036"];
                            drLog["INSUSER"] = txtWorker.Tag as string; //LoginInfo.USERID;

                            dtLog.Rows.Add(drLog);

                            PrintLog(dtLog);
                        }
                    }
                    else//LBL0205
                    {

                        dtRqst.Rows[0]["ATTVAL001"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")); // QTY
                                                                                                                                     // 체크박스 체크 시, ReadOnly False
                        if (chkDetail.IsChecked == true)
                        {
                            dtRqst.Rows[0]["ATTVAL002"] = txtnetweight.Text.ToString().Trim(); // NET WEIGHT
                            dtRqst.Rows[0]["ATTVAL003"] = txtgrossweight.Text.ToString().Trim(); // GROSS WEIGHT
                            dtRqst.Rows[0]["ATTVAL004"] = txtratedpower.Text.ToString().Trim(); // RATED POWER
                        }
                        else
                        {
                            dtRqst.Rows[0]["ATTVAL002"] = ""; // NET WEIGHT
                            dtRqst.Rows[0]["ATTVAL003"] = ""; // GROSS WEIGHT
                            dtRqst.Rows[0]["ATTVAL004"] = ""; // RATED POWER
                        }
                        dtRqst.Rows[0]["ATTVAL005"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE")); // PACKAGE DATE
                        dtRqst.Rows[0]["ATTVAL006"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")); // Barcode

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                        if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                        {
                            DataRow drLog = dtLog.NewRow();

                            drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                            drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                            drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                            drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                            drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                            drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                            drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                            drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                            drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                            drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                            drLog["INSUSER"] = txtWorker.Tag as string; //LoginInfo.USERID;

                            dtLog.Rows.Add(drLog);

                            PrintLog(dtLog);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [바코드 프린터 발행용]
        private bool PrintZPL(string sZPL, DataRow drPrtInfo)
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
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
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


        private bool PrintZPL1(string sZPL, DataRow drPrtInfo)
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
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.PrintUsbBarCodeLabelByUniversalTransformationFormat8(sZPL);
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
        #endregion

        #region [A4 발행용]
        private void PrintA4F2(int iPageQty)
        {
            try
            {
                DataTable dtLog = GetLogTable();
              
                List<Dictionary<string, string>> _DictionaryList = new List<Dictionary<string, string>>();

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    Dictionary<string, string> dicParam = new Dictionary<string, string>();
                    dicParam.Add("txtPallet", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")));
                    dicParam.Add("txtCarType", txtCarTypeF2.Text);                
                    dicParam.Add("txtVol", cboVoltageF2.Text);
                  //  dicParam.Add("LIST", "Y");
                    dicParam.Add("PAGE", "PALLET_LOT");
                    _DictionaryList.Add(dicParam);

                    DataRow drLog = dtLog.NewRow();

                    drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    drLog["LABEL_CODE"] = "A4";
                    drLog["PRT_ITEM01"] = dicParam["txtCarType"];
                    drLog["PRT_ITEM02"] = dicParam["txtVol"];                
                    drLog["INSUSER"] = txtWorker.Tag as string;  //LoginInfo.USERID;

                    dtLog.Rows.Add(drLog);
                }

                PrintLog(dtLog);

                ASSY001_013_A4_PRINT wndPrint = new ASSY001_013_A4_PRINT(_DictionaryList);
                wndPrint.FrameOperation = FrameOperation;

                if (wndPrint != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = iPageQty;
                    C1WindowExtension.SetParameters(wndPrint, Parameters);

                    wndPrint.Closed += new EventHandler(wndPrint_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
                    grdMain.Children.Add(wndPrint);
                    wndPrint.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void PrintA4HLGP(int iPageQty)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                string sUseType = Util.GetCondition(cboUseTypeHLGP, "SFU1655"); // 용도는필수입니다. >> 선택된항목이없습니다.
                if (sUseType.Equals("")) return;
                sUseType = cboUseTypeHLGP.Text;
                sUseType = sUseType.Replace(cboUseTypeHLGP.SelectedValue.ToString() + " : ", "");

                List<Dictionary<string, string>> _DictionaryList = new List<Dictionary<string, string>>();

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    dicParam.Add("txtUseTYpe", "용도 (" + sUseType + ")");//용도);
                    dicParam.Add("txtCarType", txtCarTypeHLGP.Text);
                    dicParam.Add("txtLoc", txtCustomerNameHLGP.Text);
                    dicParam.Add("txtPart", txtPartCodeHLGP.Text);
                    dicParam.Add("txtPartName", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "MODLID")));
                    dicParam.Add("txtVol", cboVoltageHLGP.Text);
                    dicParam.Add("txtQty", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")) + " EA");
                    dicParam.Add("txtCust", txtCompanyNameHLGP.Text);
                    dicParam.Add("txtBoxDate", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE")));
                    dicParam.Add("txtShipDate", dtShip.SelectedDateTime.ToString("yyyy-MM-dd"));
                    dicParam.Add("txtPalletBarCode", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")));
                    dicParam.Add("txtPallet", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")));
                    dicParam.Add("txtVolDateBarCode", dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageHLGP.Text.ToUpper().Replace(" ", ""));
                    dicParam.Add("txtVolDate", dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageHLGP.Text.ToUpper());
                    dicParam.Add("txtLine", "LINE " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "LINE_NO")));
                    dicParam.Add("PAGE", "HLGP_PALLET");
                    if ((bool)chkList.IsChecked)
                    {
                        dicParam.Add("LIST", "Y");
                    }
                    else
                    {
                        dicParam.Add("LIST", "N");
                    }
                     


                    _DictionaryList.Add(dicParam);

                    DataRow drLog = dtLog.NewRow();

                    drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    drLog["LABEL_CODE"] = "A4";
                    drLog["PRT_ITEM01"] = dicParam["txtUseTYpe"];
                    drLog["PRT_ITEM02"] = dicParam["txtCarType"];
                    drLog["PRT_ITEM03"] = dicParam["txtLoc"];
                    drLog["PRT_ITEM04"] = dicParam["txtPart"];
                    drLog["PRT_ITEM05"] = dicParam["txtPartName"];
                    drLog["PRT_ITEM06"] = dicParam["txtVol"];
                    drLog["PRT_ITEM07"] = dicParam["txtQty"];
                    drLog["PRT_ITEM08"] = dicParam["txtCust"];
                    drLog["PRT_ITEM09"] = dicParam["txtBoxDate"];
                    drLog["PRT_ITEM10"] = dicParam["txtShipDate"];
                    drLog["PRT_ITEM11"] = dicParam["txtPalletBarCode"];
                    drLog["PRT_ITEM12"] = dicParam["txtPallet"];
                    drLog["PRT_ITEM13"] = dicParam["txtVolDateBarCode"];
                    drLog["PRT_ITEM14"] = dicParam["txtVolDate"];
                    drLog["PRT_ITEM15"] = dicParam["txtLine"];
                    drLog["INSUSER"] = txtWorker.Tag as string;  //LoginInfo.USERID;

                    dtLog.Rows.Add(drLog);

                }

                PrintLog(dtLog);

                ASSY001_013_A4_PRINT wndPrint = new ASSY001_013_A4_PRINT(_DictionaryList);
                wndPrint.FrameOperation = FrameOperation;

                if (wndPrint != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = iPageQty;
                    C1WindowExtension.SetParameters(wndPrint, Parameters);

                    wndPrint.Closed += new EventHandler(wndPrint_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
                    grdMain.Children.Add(wndPrint);
                    wndPrint.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrintA4LGE(int iPageQty)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                string sUseType = Util.GetCondition(cboUseTypeHLGP, "SFU1655"); // 용도는필수입니다. >> 선택된항목이없습니다.
                if (sUseType.Equals("")) return;
                sUseType = cboUseTypeHLGP.Text;
                sUseType = sUseType.Replace(cboUseTypeHLGP.SelectedValue.ToString() + " : ", "");

                List<Dictionary<string, string>> _DictionaryList = new List<Dictionary<string, string>>();

                DataTable dtInfo = DataTableConverter.Convert(dgPallet.ItemsSource);
                int iOcvDate =  dtInfo.Select("OCV_DATE IS NULL").ToList().Count;
                int iOcvElps = dtInfo.Select("OCV2_ELPS_DAYS IS NULL").ToList().Count;
                DataRow drInfo = dtInfo.Select("OCV_PRINT_YN  <> 'Y'").FirstOrDefault();

                if (iOcvDate > 0)
                {
                    Util.MessageValidation("SFU3726"); // OCV DATE가 존재하지 않습니다.
                    return;
                }
                else if (iOcvElps > 0)
                {
                    Util.MessageValidation("SFU4067"); // OCV2_ELPS_DAYS
                    return;
                }
                else if (drInfo != null)
                {
                    //SFU3727	팔레트[%1]의 OCV DATE[%2]로 부터 [%3]일 지나지 않았습니다. 계속 진행하시겠습니까?	
                    string message = MessageDic.Instance.GetMessage("SFU3727");
                    object[] parameters = { drInfo["PALLETID"], drInfo["OCV_DATE"], drInfo["OCV2_ELPS_DAYS"] };

                    message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                    if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            message = message.Replace("%" + (i + 1), parameters[i].ToString());
                        }
                    }
                    
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("SHOPID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        RQSTDT.Rows.Add(dr);

                        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHOP_OCV_CHK", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows[0]["RESULT"].ToString().Equals("N"))
                    {
                        if (System.Windows.Forms.MessageBox.Show(message, "", System.Windows.Forms.MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                        {
                            return;
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(message, "", System.Windows.Forms.MessageBoxButtons.OK);

                        return;
                    }


                   //  Util.MessageInfo("SFU3727", new object[] { drInfo["PALLETID"], drInfo["OCV_DATE"], drInfo["OCV2_ELPS_DAYS"] });
                   
                }

                //for (int i = 0; i < dgPallet.GetRowCount(); i++)
                //{
                //string sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                //string sOcvDate = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE"));
                //string sElpsDays = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV2_ELPS_DAYS"));
                //string sYN = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_PRINT_YN"));

                //if (string.IsNullOrWhiteSpace(sOcvDate))
                //{
                //    Util.MessageValidation("SFU3726"); // OCV DATE가 존재하지 않습니다.
                //    return;
                //}
                //else if (string.IsNullOrWhiteSpace(sElpsDays))
                //{
                //    Util.MessageValidation("SFU4067"); // OCV2_ELPS_DAYS
                //    return;
                //}
                //else if (sYN != "Y"))
                //{
                //    //SFU3727	팔레트[%1]의 OCV DATE[%2]로 부터 [%3]일 지나지 않았습니다. 계속 진행하시겠습니까?	
                //    MessageBoxResult result = Util.MessageConfirm("SFU3727", new object[] { sPalletID, sOcvDate, iElpsDays });
                //    if (result != MessageBoxResult.OK)
                //        return;
                //}
                //}

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    dicParam.Add("txtUseTYpe", (LoginInfo.LANGID == "ko-KR" ? "용도" : "Product Type") + " (" + sUseType + ")");//용도);
                    dicParam.Add("txtCarType", txtCarTypeLGE.Text);
                    dicParam.Add("txtLoc", txtCustomerNameLGE.Text);
                    dicParam.Add("txtRev", txtRev.Text);
                    dicParam.Add("txtPart", txtPartCodeLGE.Text);
                    dicParam.Add("txtPartName", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "MODLID")));
                    dicParam.Add("txtVol", cboVoltageLGE.Text);
                    dicParam.Add("txtQty", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")) + " EA");
                    dicParam.Add("txtCust", txtCompanyNameLGE.Text);
                    dicParam.Add("txtBoxDate", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE")));
                    dicParam.Add("txtShipDate", dtShip.SelectedDateTime.ToString("yyyy-MM-dd"));
                    dicParam.Add("txtPalletBarCode", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")));
                    dicParam.Add("txtPallet", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")));
                    dicParam.Add("txtVolDateBarCode", dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageLGE.Text.ToUpper().Replace(" ", ""));
                    dicParam.Add("txtVolDate", dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageLGE.Text.ToUpper().Replace(" ", ""));
                    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")) != string.Empty)
                    {
                        DateTime dOCV2_Date = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")));
                        dOCV2_Date = dOCV2_Date.AddDays(Int32.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV2_ELPS_DAYS"))));
                        dicParam.Add("txtOcvDate", dOCV2_Date.ToString("yyyy.MM.dd"));
                    }
                    else
                    dicParam.Add("txtOcvDate", string.Empty);
                    dicParam.Add("txtLGEPart", txtPartCodeLGE.Text + ",  " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")));
                    dicParam.Add("txtLGEBarCode", txtPartCodeLGE.Text + " " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")));
                    dicParam.Add("PAGE", "LGE_PALLET");  //

                    if ((bool)ChkNone.IsChecked) { dicParam.Add("txtNone", "Y"); } else { dicParam.Add("txtNone", ""); }
                    if ((bool)ChkSample.IsChecked) { dicParam.Add("txtSample", "Y"); } else { dicParam.Add("txtSample", ""); }
                    if ((bool)ChkToTal.IsChecked) { dicParam.Add("txtTotal", "Y"); } else { dicParam.Add("txtTotal", ""); }
                    if ((bool)rdoPass.IsChecked) { dicParam.Add("txtPass", "합 격"); } else { dicParam.Add("txtPass", "불합격"); }
                    dicParam.Add("txtPassDate", DateTime.Now.ToString("yyyy.MM.dd"));
                    dicParam.Add("txtUser", "박규욱");

                    //DataTable RQSTDT = new DataTable();
                    //RQSTDT.TableName = "RQSTDT";
                    //RQSTDT.Columns.Add("SHOPID", typeof(String));

                    //DataRow dr = RQSTDT.NewRow();
                    //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    //RQSTDT.Rows.Add(dr);

                    //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHOP_BOXING_USER", "RQSTDT", "RSLTDT", RQSTDT);
                    //if (SearchResult.Rows.Count > 0)
                    //{
                    //    dicParam.Add("txtUser", SearchResult.Rows[0]["USERID"].ToString());
                    //}


                    if ((bool)chkList.IsChecked)
                    {
                        dicParam.Add("LIST", "Y");
                    }
                    else
                    {
                        dicParam.Add("LIST", "N");
                    }

                    _DictionaryList.Add(dicParam);


                    DataRow drLog = dtLog.NewRow();

                    drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    drLog["LABEL_CODE"] = "A4";
                    drLog["PRT_ITEM01"] = dicParam["txtUseTYpe"];
                    drLog["PRT_ITEM02"] = dicParam["txtCarType"];
                    drLog["PRT_ITEM03"] = dicParam["txtLoc"];
                    drLog["PRT_ITEM04"] = dicParam["txtPart"];
                    drLog["PRT_ITEM05"] = dicParam["txtPartName"];
                    drLog["PRT_ITEM06"] = dicParam["txtVol"];
                    drLog["PRT_ITEM07"] = dicParam["txtQty"];
                    drLog["PRT_ITEM08"] = dicParam["txtCust"];
                    drLog["PRT_ITEM09"] = dicParam["txtBoxDate"];
                    drLog["PRT_ITEM10"] = dicParam["txtShipDate"];
                    drLog["PRT_ITEM11"] = dicParam["txtPalletBarCode"];
                    drLog["PRT_ITEM12"] = dicParam["txtPallet"];
                    drLog["PRT_ITEM13"] = dicParam["txtVolDateBarCode"];
                    drLog["PRT_ITEM14"] = dicParam["txtVolDate"];
                    drLog["PRT_ITEM15"] = dicParam["txtOcvDate"];
                    drLog["INSUSER"] = txtWorker.Tag as string;  //LoginInfo.USERID;

                    dtLog.Rows.Add(drLog);
                }

                PrintLog(dtLog);

                ASSY001_013_A4_PRINT wndPrint = new ASSY001_013_A4_PRINT(_DictionaryList);
                wndPrint.FrameOperation = FrameOperation;

                if (wndPrint != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = iPageQty;
                    C1WindowExtension.SetParameters(wndPrint, Parameters);

                    wndPrint.Closed += new EventHandler(wndPrint_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
                    grdMain.Children.Add(wndPrint);
                    wndPrint.BringToFront();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void wndPrint_Closed(object sender, EventArgs e) //A4 발행후 처리
        {
            ASSY001_013_A4_PRINT window = sender as ASSY001_013_A4_PRINT;
            grdMain.Children.Remove(window);
        }

        #endregion

        void PrintLog(DataTable dt)
        {
            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_BOX_LABEL_COUNT", "INDATA", null, dt);
                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_LABEL_HIST", "INDATA", null, dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable GetLogTable()
        {
            DataTable dtLog = new DataTable();
            dtLog.Columns.Add("PALLETID", typeof(string));
            dtLog.Columns.Add("LABEL_CODE", typeof(string));
            dtLog.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
            dtLog.Columns.Add("LABEL_PRT_COUNT", typeof(string));
            dtLog.Columns.Add("PRT_ITEM01", typeof(string));
            dtLog.Columns.Add("PRT_ITEM02", typeof(string));
            dtLog.Columns.Add("PRT_ITEM03", typeof(string));
            dtLog.Columns.Add("PRT_ITEM04", typeof(string));
            dtLog.Columns.Add("PRT_ITEM05", typeof(string));
            dtLog.Columns.Add("PRT_ITEM06", typeof(string));
            dtLog.Columns.Add("PRT_ITEM07", typeof(string));
            dtLog.Columns.Add("PRT_ITEM08", typeof(string));
            dtLog.Columns.Add("PRT_ITEM09", typeof(string));
            dtLog.Columns.Add("PRT_ITEM10", typeof(string));
            dtLog.Columns.Add("PRT_ITEM11", typeof(string));
            dtLog.Columns.Add("PRT_ITEM12", typeof(string));
            dtLog.Columns.Add("PRT_ITEM13", typeof(string));
            dtLog.Columns.Add("PRT_ITEM14", typeof(string));
            dtLog.Columns.Add("PRT_ITEM15", typeof(string));
            dtLog.Columns.Add("PRT_ITEM16", typeof(string));
            dtLog.Columns.Add("PRT_ITEM17", typeof(string));
            dtLog.Columns.Add("PRT_ITEM18", typeof(string));
            dtLog.Columns.Add("PRT_ITEM19", typeof(string));
            dtLog.Columns.Add("PRT_ITEM20", typeof(string));
            dtLog.Columns.Add("PRT_ITEM21", typeof(string));
            dtLog.Columns.Add("PRT_ITEM22", typeof(string));
            dtLog.Columns.Add("PRT_ITEM23", typeof(string));
            dtLog.Columns.Add("PRT_ITEM24", typeof(string));
            dtLog.Columns.Add("PRT_ITEM25", typeof(string));
            dtLog.Columns.Add("PRT_ITEM26", typeof(string));
            dtLog.Columns.Add("PRT_ITEM27", typeof(string));
            dtLog.Columns.Add("PRT_ITEM28", typeof(string));
            dtLog.Columns.Add("PRT_ITEM29", typeof(string));
            dtLog.Columns.Add("PRT_ITEM30", typeof(string));
            dtLog.Columns.Add("PRT_ITEM31", typeof(string));
            dtLog.Columns.Add("PRT_ITEM32", typeof(string));
            dtLog.Columns.Add("PRT_ITEM33", typeof(string));
            dtLog.Columns.Add("PRT_ITEM34", typeof(string));
            dtLog.Columns.Add("PRT_ITEM35", typeof(string));
            dtLog.Columns.Add("PRT_ITEM36", typeof(string));
            dtLog.Columns.Add("PRT_ITEM37", typeof(string));
            dtLog.Columns.Add("PRT_ITEM38", typeof(string));
            dtLog.Columns.Add("PRT_ITEM39", typeof(string));
            dtLog.Columns.Add("PRT_ITEM40", typeof(string));
            dtLog.Columns.Add("PRT_ITEM41", typeof(string));
            dtLog.Columns.Add("PRT_ITEM42", typeof(string));
            dtLog.Columns.Add("PRT_ITEM43", typeof(string));
            dtLog.Columns.Add("PRT_ITEM44", typeof(string));
            dtLog.Columns.Add("PRT_ITEM45", typeof(string));
            dtLog.Columns.Add("PRT_ITEM46", typeof(string));
            dtLog.Columns.Add("PRT_ITEM47", typeof(string));
            dtLog.Columns.Add("PRT_ITEM48", typeof(string));
            dtLog.Columns.Add("PRT_ITEM49", typeof(string));
            dtLog.Columns.Add("PRT_ITEM50", typeof(string));
            dtLog.Columns.Add("INSUSER", typeof(string));

            return dtLog;
        }

        void SetTextBoxes(DependencyObject obj, DataTable dt)
        {
            try
            { 
                TextBox tb = obj as TextBox;
                if (tb != null)
                    tb.Text = (dt.Select("ITEM_CODE='"+tb.ToolTip+"'").Length > 0) ? dt.Select("ITEM_CODE='" + tb.ToolTip + "'")[0]["ITEM_VALUE"].ToString() : ""; 
                    //txtPlatGM.Text = (dtRslt.Select("ITEM_CODE='PLANT'").Length > 0) ? dtRslt.Select("ITEM_CODE='PLANT'")[0]["ITEM_VALUE"].ToString() : "";
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    SetTextBoxes(VisualTreeHelper.GetChild(obj, i), dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        void ClearTextBoxes(DependencyObject obj)
        {
            try
            { 
                TextBox tb = obj as TextBox;
                if (tb != null)
                    tb.Text = "";

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    ClearTextBoxes(VisualTreeHelper.GetChild(obj, i));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // E20230718-000722 : ESNA 자동차 조립일 경우 프린트 이력이 있을 경우 확인 메세지 팝업
        private void ChkPrintQty(string sShopId)
        {
            bool chkPrintQty = false;
            for (int i = 0; i < dgPallet.GetRowCount(); i++)
            {
                if (Util.NVC_Int(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "LABEL_PRT_COUNT")) > 0)
                {
                    chkPrintQty = true;
                    break;
                }
                else
                {
                    chkPrintQty = false;
                }
            }
            if (chkPrintQty)
            {
                // SFU3449: 발행된 이력이 존재합니다.계속 진행하시겠습니까?
                Util.MessageConfirm("SFU3449", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetPrintInfo(sShopId);
                    }
                    else
                    {
                        return;
                    }
                });
            }
            else
            {
                SetPrintInfo(sShopId);
            }
        }

        // E20230718-000722 : 기존 btnPrint_Click 이벤트 function으로 분할
        private void SetPrintInfo(string sShopId)
        {
            try
            {
                // 프린터 정보 조회
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                int iQty = Util.NVC_Int(txtPrintQty.Text);

                DataRow drPrtInfo = null;

                if (rdoBarcode.IsChecked == true)
                {
                    //바코드일때 셋팅값 확인
                    if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                        return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                if (sShopId.Equals("G184"))
                {
                    if (rdoA4.IsChecked == true)
                    {
                        return;
                    }
                    else
                    {
                        if (sRes.Equals("300") || sRes.Equals("203"))
                        {
                            // 신규 라벨 발행.
                            PrintESS(sPrt, sRes, sXpos, sYpos);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    //최초 팔레트에 따라 분기할것
                    if (lblCustType.Content.Equals("F2"))//F2
                    {
                        if (rdoA4.IsChecked == true)
                        {
                            PrintA4F2(iQty);
                        }
                        else
                        {
                            PrintF2(sPrt, sRes, sXpos, sYpos);
                        }
                    }

                    else if (lblCustType.Content.Equals("CMI"))//CMI
                    {
                        PrintCMI(sPrt, sRes, sXpos, sYpos);
                    }

                    else if (lblCustType.Content.Equals("GM"))//GM
                    {
                        PrintGM(sPrt, sRes, sXpos, sYpos);
                    }
                    else if (lblCustType.Content.Equals("FORD"))//FORD
                    {
                        PrintFORD(sPrt, sRes, sXpos, sYpos);

                    }
                    else if (lblCustType.Content.Equals("HLGP")) //HLGP
                    {
                        if (rdoA4.IsChecked == true)
                        {
                            PrintA4HLGP(iQty);
                        }
                        else
                        {
                            PrintHLGP(sPrt, sRes, sXpos, sYpos);
                        }
                    }
                    else if (lblCustType.Content.Equals("LGE")) //LGE
                    {
                        //if (rdoBarcode.IsChecked == true)
                        //{
                        //    PrintZPLLGE();
                        //}
                        //else
                        //{
                        PrintA4LGE(iQty);
                        //}
                    }
                    else if (lblCustType.Content.Equals("DAIMLER")) //Daimler
                    {
                        PrintDAIMLER(sPrt, sRes, sXpos, sYpos);
                    }
                    else if (lblCustType.Content.Equals("CWA")) //Daimler
                    {
                        PrintCWA(sPrt, sRes, sXpos, sYpos);
                    }
                    else if (lblCustType.Content.Equals("CNB")) // CNB
                    {
                        PrintCNB(sPrt, sRes, sXpos, sYpos);
                    }
                    else if (lblCustType.Content.Equals("SGM")) // SGM 2020.05.07
                    {
                        PrintSGM(sPrt, sRes, sXpos, sYpos);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        #endregion

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //리스트에 있는지 체크
                DataTable dt = DataTableConverter.Convert(dgPallet.ItemsSource);

                if (dt.Rows.Count > 0 && dt.Select("PALLETID = '" + txtPallet.Text + "'").Length > 0) //중복조건 체크
                {
                    Util.Alert("SFU1781"); //"이미추가된팔레트입니다."
                    return;
                }

                GetPallet();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtPalle_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        //리스트에 있는지 체크
                        DataTable dt = DataTableConverter.Convert(dgPallet.ItemsSource);

                        if (dt.Rows.Count > 0 && dt.Select("PALLETID = '" + sPasteStrings[i].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1781"); //"이미추가된팔레트입니다."
                        }
                        else
                        {
                            GetPallet(sPasteStrings[i]);
                        }
                       
                        //if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Create(sPasteStrings[i]) == false)
                        //    break;
                        System.Windows.Forms.Application.DoEvents();
                    }


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
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
        private string labelItemsGet(int Cnt)
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;
            string I_ATTVAL_MSD = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = "LBL0152";

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : 코드=값^코드=값

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData(Cnt);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        //화면에서 입력된 값 뿌림                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();

                        if (dtInput.Rows[0][item_code].ToString()=="")
                        {
                            item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();
                        }
                        else
                        {
                            item_value = dtInput.Rows[0][item_code].ToString();
                        }

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }

                    }
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PrintZPLLGE()
        {
            try
            {
                string I_ATTVAL = string.Empty;
                CMM_ZPL_VIEWER2 wndPopup;
                Button btn = null;
                string labelCode = string.Empty;
               // int tab_idx = tcMain.SelectedIndex;

                btn = btnPrint;
                labelCode = "LBL0152";
                for (int i = 0; i < dgPallet.Rows.Count; i++)
                {
                    I_ATTVAL = labelItemsGet(i);

                    getZpl(I_ATTVAL, labelCode);

                    if (LoginInfo.USERID.Trim() == "cnseyes")
                    {
                        wndPopup = new CMM_ZPL_VIEWER2(zpl);
                        wndPopup.Show();
                    }
                }

                //for (int i = 0; i < nbPrintCnt.Value; i++)
                //{
                //    if (blPrintStop) break;

                //    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                //    System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);
                //}

                //ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private DataTable getInputData(int Cnt)
        {
            DataTable dt = new DataTable();

            dt.TableName = "INPUTDATA";
            dt.Columns.Add("ITEM001", typeof(string)); 
            dt.Columns.Add("ITEM002", typeof(string));
            dt.Columns.Add("ITEM003", typeof(string));
            dt.Columns.Add("ITEM004", typeof(string));
            dt.Columns.Add("ITEM005", typeof(string));
            dt.Columns.Add("ITEM006", typeof(string));
            dt.Columns.Add("ITEM007", typeof(string));
            dt.Columns.Add("ITEM008", typeof(string));
            dt.Columns.Add("ITEM009", typeof(string));
            dt.Columns.Add("ITEM010", typeof(string));
            dt.Columns.Add("ITEM011", typeof(string));
            dt.Columns.Add("ITEM012", typeof(string));
            dt.Columns.Add("ITEM013", typeof(string));
            dt.Columns.Add("ITEM014", typeof(string));
            dt.Columns.Add("ITEM015", typeof(string));
            dt.Columns.Add("ITEM016", typeof(string));
            dt.Columns.Add("ITEM017", typeof(string));
            dt.Columns.Add("ITEM018", typeof(string));
            dt.Columns.Add("ITEM019", typeof(string));
            dt.Columns.Add("ITEM020", typeof(string));
            dt.Columns.Add("ITEM021", typeof(string));
            dt.Columns.Add("ITEM022", typeof(string));
            dt.Columns.Add("ITEM023", typeof(string));
            dt.Columns.Add("ITEM024", typeof(string));
            dt.Columns.Add("ITEM025", typeof(string));
            dt.Columns.Add("ITEM026", typeof(string));
            dt.Columns.Add("ITEM027", typeof(string));
            dt.Columns.Add("ITEM028", typeof(string));
            dt.Columns.Add("ITEM029", typeof(string));
            dt.Columns.Add("ITEM030", typeof(string));
            dt.Columns.Add("ITEM031", typeof(string));
            dt.Columns.Add("ITEM032", typeof(string));
            dt.Columns.Add("ITEM033", typeof(string));
            dt.Columns.Add("ITEM034", typeof(string));
            dt.Columns.Add("ITEM035", typeof(string));
            dt.Columns.Add("ITEM036", typeof(string));



            DataRow dr = dt.NewRow();
            string sUseType = Util.GetCondition(cboUseTypeHLGP, "SFU1655");
            dr["ITEM001"] = "Product Type" +" (" + sUseType + ")";
            dr["ITEM002"] = null;//dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageLGE.Text.ToUpper().Replace(" ", "");
            dr["ITEM003"] = null;
            dr["ITEM004"] = null;
            dr["ITEM005"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "MODLID"));
            dr["ITEM006"] = null;//Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "PALLETID"));
            dr["ITEM007"] = null;//Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "PALLETID"));
            dr["ITEM008"] = null;
            dr["ITEM009"] = null;
            dr["ITEM010"] = null;
            dr["ITEM011"] = null;
            DateTime dOCV2_Date = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "OCV_DATE")));
            dOCV2_Date = dOCV2_Date.AddDays(Int32.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "OCV2_ELPS_DAYS"))));
            dr["ITEM012"] = dOCV2_Date.ToString("yyyy.MM.dd");
            dr["ITEM013"] = null;
            dr["ITEM014"] = null;
            dr["ITEM015"] = null;
            dr["ITEM016"] =  Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "TOTAL_QTY")) + " EA";
            dr["ITEM017"] = "Packed Date   : " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "PACKDATE"));
            dr["ITEM018"] = null;
            dr["ITEM019"] = null;
            dr["ITEM020"] = "Shipping Date : " + dtShip.SelectedDateTime.ToString("yyyy-MM-dd");
            dr["ITEM021"] = null;
            dr["ITEM022"] = null;
            dr["ITEM023"] = null;
            dr["ITEM024"] = null;
            dr["ITEM025"] = null;
            dr["ITEM026"] = null;
            dr["ITEM027"] = null;
            dr["ITEM028"] = null;
            dr["ITEM029"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "PALLETID"));
            dr["ITEM030"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "PALLETID"));
            dr["ITEM031"] = null;
            dr["ITEM032"] = dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageLGE.Text.ToUpper().Replace(" ", "");
            dr["ITEM033"] = dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageLGE.Text.ToUpper().Replace(" ", "");
            dr["ITEM034"] = null;
            dr["ITEM035"] = txtPartCodeLGE.Text + " " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "TOTAL_QTY"));
            dr["ITEM036"] = txtPartCodeLGE.Text + " " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[Cnt].DataItem, "TOTAL_QTY"));



            dt.Rows.Add(dr);


            //for (int i = 0; i < dgPallet.GetRowCount(); i++)
            //{


            //    dicParam.Add("txtUseTYpe", (LoginInfo.LANGID == "ko-KR" ? "용도" : "Product Type") + " (" + sUseType + ")");//용도);
            //    dicParam.Add("txtCarType", txtCarTypeLGE.Text);
            //    dicParam.Add("txtLoc", txtCustomerNameLGE.Text);
            //    dicParam.Add("txtRev", txtRev.Text);
            //    dicParam.Add("txtPart", txtPartCodeLGE.Text);
            //    dicParam.Add("txtPartName", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "MODLID")));
            //    dicParam.Add("txtVol", cboVoltageLGE.Text);
            //    dicParam.Add("txtQty", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")) + " EA");
            //    dicParam.Add("txtCust", txtCompanyNameLGE.Text);
            //    dicParam.Add("txtBoxDate", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACKDATE")));
            //    dicParam.Add("txtShipDate", dtShip.SelectedDateTime.ToString("yyyy-MM-dd"));
            //    dicParam.Add("txtPalletBarCode", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")));
            //    dicParam.Add("txtPallet", Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")));
            //    dicParam.Add("txtVolDateBarCode", dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageLGE.Text.ToUpper().Replace(" ", ""));
            //    dicParam.Add("txtVolDate", dtShip.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboVoltageLGE.Text.ToUpper().Replace(" ", ""));
            //    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")) != string.Empty)
            //    {
            //        DateTime dOCV2_Date = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")));
            //        dOCV2_Date = dOCV2_Date.AddDays(Int32.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV2_ELPS_DAYS"))));
            //        dicParam.Add("txtOcvDate", dOCV2_Date.ToString("yyyy.MM.dd"));
            //    }
            //    else
            //        dicParam.Add("txtOcvDate", string.Empty);
            //    dicParam.Add("txtLGEPart", txtPartCodeLGE.Text + ",  " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")));
            //    dicParam.Add("txtLGEBarCode", txtPartCodeLGE.Text + " " + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")));
            //    dicParam.Add("PAGE", "LGE_PALLET");  //


            //    if ((bool)chkList.IsChecked)
            //    {
            //        dicParam.Add("LIST", "Y");
            //    }
            //    else
            //    {
            //        dicParam.Add("LIST", "N");
            //    }

            //    DataRow drLog = dtLog.NewRow();

            //    drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
            //    drLog["LABEL_CODE"] = "A4";
            //    drLog["PRT_ITEM01"] = dicParam["txtUseTYpe"];
            //    drLog["PRT_ITEM02"] = dicParam["txtCarType"];
            //    drLog["PRT_ITEM03"] = dicParam["txtLoc"];
            //    drLog["PRT_ITEM04"] = dicParam["txtPart"];
            //    drLog["PRT_ITEM05"] = dicParam["txtPartName"];
            //    drLog["PRT_ITEM06"] = dicParam["txtVol"];
            //    drLog["PRT_ITEM07"] = dicParam["txtQty"];
            //    drLog["PRT_ITEM08"] = dicParam["txtCust"];
            //    drLog["PRT_ITEM09"] = dicParam["txtBoxDate"];
            //    drLog["PRT_ITEM10"] = dicParam["txtShipDate"];
            //    drLog["PRT_ITEM11"] = dicParam["txtPalletBarCode"];
            //    drLog["PRT_ITEM12"] = dicParam["txtPallet"];
            //    drLog["PRT_ITEM13"] = dicParam["txtVolDateBarCode"];
            //    drLog["PRT_ITEM14"] = dicParam["txtVolDate"];
            //    drLog["PRT_ITEM15"] = dicParam["txtOcvDate"];
            //    drLog["INSUSER"] = txtWorker.Tag as string;  //LoginInfo.USERID;

            //    dtLog.Rows.Add(drLog);
            //}

            //PrintLog(dtLog);
            return dt;
        }


        // SGM 2020.05.07
        private void PrintSGM(string sPrt, string sRes, string sXpos, string sYpos) 
        {
            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));
                dtRqst.Columns.Add("ATTVAL010", typeof(string));
                dtRqst.Columns.Add("ATTVAL011", typeof(string));
                dtRqst.Columns.Add("ATTVAL012", typeof(string));
                dtRqst.Columns.Add("ATTVAL013", typeof(string));
                dtRqst.Columns.Add("ATTVAL014", typeof(string));
                dtRqst.Columns.Add("ATTVAL015", typeof(string));
                dtRqst.Columns.Add("ATTVAL016", typeof(string));
                dtRqst.Columns.Add("ATTVAL017", typeof(string));
                dtRqst.Columns.Add("ATTVAL018", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);
                
                string sLicense = "";
                string sPallet = "";


                string sShipDate1 = String.Format(CultureInfo.GetCultureInfo("en-US"), "{0:ddMMMyyyy}", dtShip.SelectedDateTime).ToUpper();

                int sShift = 0;

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    DateTime dExpiration_Date;
                    DateTime dOUT_Date;            // 출고일자
                    string sOUT_Date = string.Empty;
                    string sExpiration_Date = string.Empty;
                    string sExpiration_Date_For_2D = string.Empty;

                    DateTime dOCV2_Date;

                    if (LoginInfo.CFG_SHOP_ID == "G451")
                    {
                        dOUT_Date = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "INSDTTM")));            // 출고일시
                        sOUT_Date = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:yyyy/MM/dd}", dOUT_Date.AddDays(263)).ToString();  // ITEM18:출고일자 +263 days
                    }

                    string sOCV2_Date = string.Empty;
                    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")) != string.Empty)
                    {
                        dOCV2_Date = DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "OCV_DATE")));
                        //sOCV2_Date = "OCV DATE: " + string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:ddMMMyyyy}", dOCV2_Date).ToString().ToUpper(); //.ToUpper().ToString();
                        sOCV2_Date = "OCV DATE: " + string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:yyyy/MM/dd}", dOCV2_Date).ToString();
                        
                        dExpiration_Date = dOCV2_Date.AddDays(270); // Expiration Date：OCV date+270days 

                        sExpiration_Date = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:yyyy/MM/dd}", dExpiration_Date).ToString();
                        sExpiration_Date_For_2D = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:yyyyMMdd}", dExpiration_Date).ToString();
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Length == 10)
                    {
                        sPallet = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Substring(2, Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Length -2);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Length == 9)
                    {
                        sPallet = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Substring(1, Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID")).Length -1);
                    }
                    else
                    {
                        sPallet = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    }
                    
                    sLicense = "1J UN" + txtDunsSGM.Text + sPallet;

                    if(Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_TIME"))) >= 8 && Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_TIME"))) <= 20)
                    {
                        sShift = 1;
                    }
                    else
                    {
                        sShift = 2;
                    }
                    

                    string _2DBCD = _COMPLIANCE_IDICATOR1.ToString() +
                                                  _COMPLIANCE_IDICATOR2.ToString() +
                                                  _COMPLIANCE_IDICATOR3.ToString() +
                                                  _RECORD_SEPARATOR.ToString() +
                                                  _DATA_FORMAT +
                                                  _GROUP_SEPARATOR +
                                                  "P{0}" + _GROUP_SEPARATOR.ToString() +
                                                  "Q{1}" + _GROUP_SEPARATOR.ToString() +
                                                  "1J{2}" + _GROUP_SEPARATOR.ToString() +
                                                  "20L{3}" + _GROUP_SEPARATOR.ToString() +
                                                  "K" + _GROUP_SEPARATOR.ToString() +                                                  
                                                  "2P" + _GROUP_SEPARATOR.ToString() +
                                                  "14D{5}" + _GROUP_SEPARATOR.ToString() +
                                                  "T{4}" + _GROUP_SEPARATOR.ToString() +
                                                  "C" + _RECORD_SEPARATOR.ToString() +
                                                  _END_OF_TRAILER.ToString();


                    _2DBCD = String.Format(_2DBCD,
                                                  //----------------------------------------------------------------------------------------------------------
                                                  txtPartNumberSGM.Text,
                                                  Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY")),
                                                  "UN" + sLicense.Substring(5, 17).Trim(),
                                                  txtPlantSGM.Text + txtDockSGM.Text,
                                                  Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "LINE_NO")) + sShift.ToString() + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_DATE")).Substring(2, 2) + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_DAY_OF_YEAR")).PadLeft(3, '0'),
                                                  sExpiration_Date_For_2D
                                                  );

                    dtRqst.Rows[0]["LBCD"] = _LabelCode; // "LBL0232" SGM
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;

                    dtRqst.Rows[0]["ATTVAL001"] = txtPartNameESGM.Text; //CELL
                    dtRqst.Rows[0]["ATTVAL002"] = txtPartNumberSGM.Text; //24290253
                    dtRqst.Rows[0]["ATTVAL003"] = txtPlantSGM.Text + "-" + txtDockSGM.Text; //"BAP-K2XM";
                    dtRqst.Rows[0]["ATTVAL004"] = "-";
                    dtRqst.Rows[0]["ATTVAL005"] = "-";

                    if (LoginInfo.CFG_SHOP_ID == "G451")
                    {
                        dtRqst.Rows[0]["ATTVAL006"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "INSDTTM")); ;
                    }
                    else
                    {
                        dtRqst.Rows[0]["ATTVAL006"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PACK_DATE"));
                    }
                    dtRqst.Rows[0]["ATTVAL007"] = "SAIC-GM";
                    dtRqst.Rows[0]["ATTVAL008"] = txtPackagenameSGM.Text;
                    dtRqst.Rows[0]["ATTVAL009"] = txtPackageSGM.Text;

                    dtRqst.Rows[0]["ATTVAL010"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));

                    dtRqst.Rows[0]["ATTVAL011"] = sOCV2_Date;

                    dtRqst.Rows[0]["ATTVAL012"] = "UN" + txtDunsSGM.Text + sPallet;
                    dtRqst.Rows[0]["ATTVAL013"] = "UN" + txtDunsSGM.Text + sPallet;
                    dtRqst.Rows[0]["ATTVAL014"] = _2DBCD;//2D BCD

                    dtRqst.Rows[0]["ATTVAL015"] = txtPartNameCSGM.Text;

                    dtRqst.Rows[0]["ATTVAL016"] = txtSupplier.Text.Substring(0, 12);
                    dtRqst.Rows[0]["ATTVAL017"] = txtSupplier.Text.Substring(12, 3);

                    if (LoginInfo.CFG_SHOP_ID == "G451")
                    {
                        dtRqst.Rows[0]["ATTVAL018"] = sOUT_Date;
                    }
                    else
                    {
                        dtRqst.Rows[0]["ATTVAL018"] = sExpiration_Date;
                    }

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    if (PrintZPL1(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                    {

                        DataRow drLog = dtLog.NewRow();

                        drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                        drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                        drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                        drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                        drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                        drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                        drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                        drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                        drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                        drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ATTVAL007"];
                        drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ATTVAL008"];
                        drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ATTVAL009"];
                        drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ATTVAL010"];
                        drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ATTVAL011"];
                        drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ATTVAL012"];
                        drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ATTVAL013"];
                        drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ATTVAL014"];
                        drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ATTVAL015"];
                        drLog["PRT_ITEM16"] = dtRqst.Rows[0]["ATTVAL016"];
                        drLog["PRT_ITEM17"] = dtRqst.Rows[0]["ATTVAL017"];
                        drLog["PRT_ITEM18"] = dtRqst.Rows[0]["ATTVAL018"];
                        drLog["INSUSER"] = txtWorker.Tag as string;  //LoginInfo.USERID;

                        dtLog.Rows.Add(drLog);

                        PrintLog(dtLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [ESS 라벨 인쇄]
        /// <summary>
        /// E20230209-000102 : 
        /// </summary>
        /// <param name="sPrt"></param>
        /// <param name="sRes"></param>
        /// <param name="sXpos"></param>
        /// <param name="sYpos"></param>
        private void PrintESS(string sPrt, string sRes, string sXpos, string sYpos)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));
                dtRqst.Columns.Add("ATTVAL010", typeof(string));
                dtRqst.Columns.Add("ATTVAL011", typeof(string));
                dtRqst.Columns.Add("ATTVAL012", typeof(string));
                dtRqst.Columns.Add("ATTVAL013", typeof(string));
                dtRqst.Columns.Add("ATTVAL014", typeof(string));
                //dtRqst.Columns.Add("ATTVAL015", typeof(string));
                //dtRqst.Columns.Add("ATTVAL016", typeof(string));
                dtRqst.Columns.Add("ATTVAL017", typeof(string));
                dtRqst.Columns.Add("ATTVAL018", typeof(string));
                dtRqst.Columns.Add("ATTVAL019", typeof(string));
                dtRqst.Columns.Add("ATTVAL020", typeof(string));
                dtRqst.Columns.Add("ATTVAL021", typeof(string));
                //dtRqst.Columns.Add("ATTVAL022", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    /* LOT,수량 최대 5까지 표시
                     * E20230704-000646 : 최대4개로 변경
                     *
                     */
                    string[] lot = new String[5];
                    string[] qty = new String[5];
                    string[] bar = new String[5];

                    string cell = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "CELLID"));
                    int cnt = 0;
                    if (! cell.Equals("")) {
                        string[] lotQty = cell.Split(';');

                        cnt = lotQty.Length;
                        
                        for (int ii = 0; ii < cnt; ii++)
                        {
                            lot[ii] = lotQty[ii].Split(':')[0];
                            qty[ii] = lotQty[ii].Split(':')[1];
                            bar[ii] = lot[ii] + " " + qty[ii];
                        }
                    }

                    dtRqst.Rows[0]["LBCD"] = _LabelCode;
                    dtRqst.Rows[0]["PRMK"] = sPrt;
                    dtRqst.Rows[0]["RESO"] = sRes;
                    dtRqst.Rows[0]["PRCN"] = txtPrintQty.Text;
                    dtRqst.Rows[0]["MARH"] = sXpos;
                    dtRqst.Rows[0]["MARV"] = sYpos;
                    dtRqst.Rows[0]["ATTVAL001"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "CUST_TYPE"));
                    dtRqst.Rows[0]["ATTVAL002"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "MODELID")) + "("
                                                + Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "MODLID")) + ")";
                    dtRqst.Rows[0]["ATTVAL003"] = Util.NVC(lot[0]);
                    dtRqst.Rows[0]["ATTVAL004"] = Util.NVC(lot[1]);
                    dtRqst.Rows[0]["ATTVAL005"] = Util.NVC(lot[2]);
                    dtRqst.Rows[0]["ATTVAL006"] = Util.NVC(qty[0]);
                    dtRqst.Rows[0]["ATTVAL007"] = Util.NVC(qty[1]);
                    dtRqst.Rows[0]["ATTVAL008"] = Util.NVC(qty[2]);
                    dtRqst.Rows[0]["ATTVAL009"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "TOTAL_QTY"));
                    dtRqst.Rows[0]["ATTVAL010"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "SHIPDATE"));
                    dtRqst.Rows[0]["ATTVAL011"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                    dtRqst.Rows[0]["ATTVAL012"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "SKID_NOTE"));
                    dtRqst.Rows[0]["ATTVAL013"] = Util.NVC(lot[3]);
                    dtRqst.Rows[0]["ATTVAL014"] = Util.NVC(qty[3]);
                    //dtRqst.Rows[0]["ATTVAL015"] = Util.NVC(lot[4]);
                    //dtRqst.Rows[0]["ATTVAL016"] = Util.NVC(qty[4]);
                    // 바코드 추가
                    dtRqst.Rows[0]["ATTVAL017"] = Util.NVC(bar[0]);
                    dtRqst.Rows[0]["ATTVAL018"] = Util.NVC(bar[1]);
                    dtRqst.Rows[0]["ATTVAL019"] = Util.NVC(bar[2]);
                    dtRqst.Rows[0]["ATTVAL020"] = Util.NVC(bar[3]);
                    dtRqst.Rows[0]["ATTVAL021"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "MODELID"));
                    //dtRqst.Rows[0]["ATTVAL022"] = Util.NVC(bar[4]);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    // 중국어 라벨 타이틀
                    //string s203 = "^FO14,21^GFA,00336,00336,00012,00,00000000020400000040,00040000030600060060,00060000020418060040,00060003FFFFFC060040,06060600020400060C40,06060600020400060C40,06060600020400060C43,06060600100000060C4F,060606001C0018060C73,060606001BFFFC7FCDC3,06060600300040060E43,0FFFFE00200040067C43,00060600600840060C43,00060000F3FC40060C43,08060201E30840060C43,0C060301630840060C43,0C060302630840060C5F,0C060304630840060C46,0C060300630840066C4040,0C06030063F840078C4040,0C0603006308401E0C0040,0C060300630040780C0040,0FFFFF00620040200C00E0,000003006007C00007FFC0,000003006001C0,00000000400080,00,\n"
                    //            + "^FO384,20^GFA,00224,00224,00008,00,00000100000080,000201801FFFC0,1FFF0100180180,01183100180180,01183100180180,01183100180180,01193100180180,3FFFB1001FFF80,01187100180180,011831,03183100000008,02182103FFFFFC,0218010002,0418010006,08180F0006,100602000FFFC0,200600000000C0,000604000000C0,07FFFE00000080,00060000000180,00060000000180,00060000000180,000601800041,3FFFFFC0003F,00000000001E,000000000008,00,\n"
                    //            + "^FO26,133^GFA,00224,00224,00008,00,00802000000080,00C470003FFFC0,08C66000300080,0CCC60003FFF80,06C84000300080,06D0C000300080,04E4C0803FFF80,3FFEFFC0300080,01C1830020000C,03F14207FFFFFC,06D94200400080,0CCA42003FFFC0,10C242002060C0,608446002060C0,018846002060C0,3FFC24003FFFC0,030824002060C0,02182C003FFFC0,06101800206080,01F018000060,00783800006020,00CC6C00FFFFE0,0304C7000060,0C0183C0006008,70060183FFFFFC,0018,00,\n"
                    //            + "^FO14,333^GFA,00240,00240,00012,00,000800808004100004,040800808004180008,240800DDDC0408007FF0,247F81222009FFC04020,24080224200800007FE0,3F880426001800004020,44FFE1FFFC187F804020,44000200042800007FE0,0403067FE8287F804020,070200404048000077E0,0CFFE07FC00800004820,740200404008FF8026,044200400008808033,0422007FE0088081220C,04220040200880812024,040200402008FF832024,040E007FE00880801FE0,040400400008,00,\n"
                    //            + "^FO17,257^GFA,00448,00448,00016,00,004000000800000000000041,00E000000E060002000400718008,00C000010C040003FFFE006187FC,01800000CC04000300040061A408,01FFFE006C041803000403FFF408,030004006FFFFC03000400618408,020004000C040003000400618408,060004001C040003000400618408,0C0104002C0400030004007F87F8,0BFF8400CC042003000400618408,130104030CFFF003000400618408,230104000C000003FFFC00618408,430104000C8000030004007F8408,030104000C600003000400618408,03010400006018030004006187F8,03FF0403FFFFFC0300040061A408,03010C0001A04003000403FFF408,0300FC8003106003000400200408,030018800610C003000400320C08,030000800E090003000400318808,030000803606000300040060C808,03000080C6030003FFFC00C0D008,010000C30671C003000400801008,01E003C00780FC030004010020F8,00FFFF8007003802000002004038,0000000000000000000000008010,00, \n"
                    //            + "^FO49,193^GFA,00336,00336,00012,00,00000000080200000008,004070000C470003FFFC,006040008C6600030008,0030C000CCC60003FFF8,003080006C8400030008,022108006D0C00030008,03FFFC004E4C0803FFF8,01000C03FFEFFC030008,01000C001C1830020000C0,01000C003F14207FFFFFC0,01000C006D9420040008,01000C00CCA42003FFFC,01000C010C242002060C,01FFFC0608446002060C,03100C0018846002060C,000C0003FFC24003FFFC,0086000030824002060C,00C306002182C003FFFC,08C31300610180020608,08C011801F01800006,18C01180078380000602,38C010800CC6C00FFFFE,30C03800304C700006,007FF000C0183C00060080,000000070060183FFFFFC0,000000000180,00, \n"
                    //            + "^XZ";
                    //string s300 = "^FO20,30^GFA,00672,00672,00016,00,00,0000080000000080100000000004,00000E00000000F01C000008000780,00000E00000000E01C00000E0007,00000C00000000E01806000E0007,00000C00000000E0180F000C0007,00000C010007FFFFFFFF800C0207,00C00C00E00000E01800000C0387,00E00C00E00000E01800000C0387,00C00C00C00000E01800000C0307,00C00C00C00000E01800000C030702,00C00C00C00006801000000C03070F8000C00C00C00007000000000C03073F,00C00C00C0000F000002000C4307C7,00C00C00C0000E00000F000CE30707,00C00C00C0001C7FFFFF83FFF33F07,00C00C00C00018000070000C03C707,00C00C00C00038000070000C070707,01FFFFFFC00070000070000C3B0707,00C00C00C00078000070000CC30707,00000C008000FC203070000C030707,00000C000001F83FF870000C030707,03000C006001B8307070000C030707,03C00C00700338307070000C030707,03800C00700438307070000C030707,03800C00700838307070000C03077F,03800C00700038307070000C03071E,03800C00700038307070000C03070E4003800C00700038307070000C1B07004003800C007000383FF070000CE307004003800C00700038307070000F8304004003800C00700038306070003E0300004003800C0070003830007001F80300006003800C0070003820007003E00300006007FFFFFFF000380000700180038000F001000000700038000FF0010001FFFFF0000000007000380003E0000000FFFFC0000000006000380001E0,00000000000020000080,00,00, \n"
                    //            + "^FO569,28^GFA,00504,00504,00012,00,00,00000000200000000080,0000040038000C0001C0,00000E0038000FFFFFC0,07FFFF0030000E0001C0,0030601830000E0001C0,0030601E30000E0001C0,0030600C30000E0001C0,0030600C30000E0001C0,0030600C30000E0001C0,0030610C30000E0001C0,0030638C30000E0001C0,0FFFFFCC30000FFFFFC0,0030600C30000E0001C0,0030600C30000E0001,0030600C300000000002,0030601C300000000007,006060183007FFFFFFFF80,0060600030000060,00E06000300000E0,00C06000300000E0,01806008300000C0,01006007F00001C00040,02004C00F000038000E0,04000E00600003FFFFF0,08000C000000010000E0,00000C018000000000C0,00000C03C000000000C0,00FFFFFFE000000001C0,00000C000000000001C0,00000C000000000001C0,00000C00000000000180,00000C00000000000180,00000C00100000000380,00000C00380000060380,00000C007C000001FF,1FFFFFFFFE000000FE,00000000000000007C,000000000000000030,00,00, \n"
                    //            + "^FO43,195^GFA,00504,00504,00012,00,00,00080020000018000080,000F003800001FFFFFE0,040E183C00001C0000C0,020E1C3800000C0000C0,038E183000000C0000C0,01CE307000000FFFFFC0,01CE606000000C0000C0,00CE406000000C0000C0,008E88E004001C0000C0,000E1CC00E001FFFFFC0,1FFFFEFFFF001C0000C0,001E00C060001C000001,003E01C060000000000380,007FC1C0600FFFFFFFFFC0,00EEF34060,00CE724060,038E3240E00010000040,060E1460E0001FFFFFF0,0C0C0420C000180300E0,10080820C000180300C0,201E0020C000180300C0,001C0030C0001FFFFFC0,00183031C000180300E0,1FFFF8118000180300E0,003030198000180300E0,0060701B8000180300E0,0060600F00001FFFFFE0,00C0E00F0000180300C0,00F0C00E000010030010,001F800F000000030070,0003E01F8000FFFFFFF8,000EF831C0000003,001C3861E0000003,003009C0F80000030003,00E003007F000003000780,07000E001C07FFFFFFFF80,1800380008,0000C0,00,00, \n"
                    //            + "^FO73,285^GFA,00672,00672,00016,00,00,000000200000020008000006000020,00040038000003C00E000007FFFFF8,0007003C000103860F000007000030,00038070000083870E000003000030,0001C0600000E3860C000003000030,0001C0600000738C1C000003FFFFF0,0001C0C00000739818000003000030,0000C0800000339018000003000030,00008100000023A238010007000030,002001038000038730038007FFFFF0,003FFFFF8007FFFFBFFFC007000030,003000030000078030180007000000400030000300000F8070180000000000E00030000300001FF0701803FFFFFFFFF00030000300003BBCD018,003000030000339C9018,003000030000E38C90380004000010,003000030001838518380007FFFFFC,00300003000303010830000600C038,00300003000402020830000600C030,003FFFFF000807800830000600C030,00300003000007000C300007FFFFF0,003020020000060C0C70000600C038,000038000007FFFE0460000600C038,00081C0000000C0C0660000600C038,010E0E010000181C06E0000600C038,010E0700C000181803C00007FFFFF8,010E07047000303803C0000600C030,030E020438003C300380000400C004,070E00043C0007E003C0000000C01C,070E00041C0000F807E0003FFFFFFE,0E0E00040C0003BE0C70000000C0,1E0E000C0C00070E1878000000C0,1C0E000F00000C02703E000000C000C00007FFFE00003800C01FC00000C001E00003FFFC0001C003800701FFFFFFFFE0000000000006000E0002,0000000000000030,00,00, \n"
                    //            + "^FO25,379^GFA,01008,01008,00024,00,00,000400000000020000000000000000000201,00070000000001801C000000000010000301C0,000F0000000001C00E00000600001C000381C18030,000E0000000001800C000007FFFFFC00030181FFF8,000E0000000181800C00000600001800030181C030,001C00000000E1800C00000600001800030191C030,00180000C00071800C060006000018000301B9C030,003FFFFFE00071800C0F0006000018007FFFFDC030,00700001C00031BFFFFF800600001800030181C030,00600001C00021800C00000600001800030181C030,00E00001C00001800C00000600001800030181C030,00C00001C00007800C00000600001800030181C030,01800001C00019800C0000060000180003FF81FFF0,03200181C00071800C00000600001800030181C030,033FFFC1C001C1800C08000600001800030181C030,06300381800F81800C1C000600001800030181C030,0C3003818007018FFFFE0007FFFFF800030181C030,18300381800201880000000600001800030181C030,3030038180000187000000060000180003FF81C030,20300381800001878000000600001800030181C030,00300381800001038006000600001800030181C030,0030038180000003000F000600001800030181C030,003FFF818007FFFFFFFF800600001800030191FFF0,003003818000001D00200006000018000301B9C030,003003018000003880300006000018007FFFFD8030,0030000380000070C0780006000018000000018030,0030007F880000E040F00006000018000084018030,0030001F040003C021C000060000180001C3038030,0030000E0C0007C0330000060000180001E1830030,003000000C001CC01E000006000018000381C30030,003000000C0038C00E000006000018000700E70030,003000000C00C0C027000007FFFFF8000600E60030,003000000C0700C1C3C00006000018000C004C0030,003000000E0800CF01F80006000018001800180030,003C00003F0000FC007FC006000018003000300FF0,001FFFFFFE0001F0003F00060000180060002001E0,0003FFFFF00000C0000E00040000000080004000E0,000000000000000000000000000000000001800080,00,00, \n"
                    //            + "^FO21,492^GFA,00464,00464,00016,00,00002000040080000808000001,018030000E00C0000C0400000180,018020000C0180000C060000010080,0180200018310180180300007FFFC0,1980218017C3FE0018030400600180,1987FFC02104100017FFFF00600180,198020004190180030000000600180,11902000818C0800200000007FFF80,3FF82000200C008060000800600180,318020203FFFFFC070FFFC00600180,219FFFF0600001C0E0000000600180,2180040060000100A00000007FFF80,41800700C3FFF20120000800600180,419806000200200221FFFC00600180,01E0063002002004200000007FFF80,019FFFF80200200020000000600180,0F80060003FFE000208008004180,798406000200200020FFFC0000C0,3183060002000000204008001C6040,018386000200100020400801186030,0181860003FFF80020400801182118,01818600020010002040080318011C,01800600020010002040080318010C,0180060002001000207FF807180108,01807E0003FFF000204008000FFF80,01800C000200100020C0080007FE,010008000200000020,00, \n"
                    //            + "^XZ";
                    // 중국어 라벨 변경
                    string s203 = "^FO23,21^GFA,00448,00448,00016,00,0000000000000000000006,0001C000000F0F0000C00780,0001C000000E0E0000E007,0001C000000E0E1E00E007,0301C0E01FFFFFFF00E0E7,0381C0E0000E0E0000E0E7,0381C0E0000E0E0000E0E70C,0381C0E0007C0C0000E0E73E,0381C0E00078000C00E0E7FC,0381C0E000F1FFDE01FEEF1C,0381C0E000E700700FE0FF1C,07FFFFE001C0007000E3E71C,0301C0E003C0007000FEE71C,0001C00003E7FC7000E0E71C,0001C00007C71C7000E0E71C,0701C0700DC71C7000E0E71C,0701C07819C71C7000E0E7FC,0701C07001C71C7000E0E73C,0701C07001C71C7000E7E71B,0701C07001C7FC7000FCE703,0701C07001C71C7001E0E003,0701C07001C700701F80E00380,0701C07001C000700E00E003C0,07FFFFF001C007F00000FFFF80,0000007001C001E0,00000000018000C0,00,\n"
                                + "^FO318,22^GFA,00336,00336,00012,00,00000030,0000C038007FFFE0,0FFFE038007001C0,00C70738007001C0,00C70738007001C0,00C70738007001C0,00C76738007001C0,3FFFF738007001C0,00C70738007FFFC0,00C707380070,01C707380000000E,01C706381FFFFFFF,03870038000E,03070038000E,060703F8001C,0C01C070003C00E0,1801C000003FFFF0,0001C0C0000000E0,01FFFFE0000000C0,0001C000000001C0,0001C000000001C0,0001C000000001C0,0001C0380001C380,03FFFFFC00007F80,1E00000000001F,00000000000018,00,\n"
                                + "^FO38,136^GFA,00336,00336,00012,00,00700C,00780F0000FFFFE0,0E71CE0000E000C0,07739C0000E000C0,03F71C0000FFFFC0,0376180000E000C0,0071F81C00FFFFC0,1FFFFFFF00E000C0,00F0387000C00006,01FF78703FFFFFFF,03F77870,0673D86000C000E0,1C70D86000FFFFE0,30618CE000E0E0E0,00F00CE000E0E0E0,00E38CE000FFFFE0,1FFF8FC000E0E0E0,01C707C000FFFFE0,038E078000E0E0E0,03EE03800000E030,003E07C00000E070,00778FE003FFFFC0,00E1B8F80000E0,0780703F0000E00E,3C03C0181FFFFFFF,0006,00,\n"
                                + "^FO5,333^GFA,00560,00560,00020,00,0180180000C00C0000181800000070,01E01C0000E00F00003C0E00000070,01C01C0001C19C0C003807000000C030,01C01C0001FFDFFE00380700001FFFF8,1FC3FFF0033031800070070F001C0038,1DC01C00063861C00077FFFF801C0038,1DC01C000C19C0E000E00000001C0038,19DC1C001818E0C000C00000001FFFF8,1FFE1C0C0600E00E01F0001C001C0038,39C01C1E07FFFFFF01E1FFF0001C0038,31CFF7F00E00001C03E00000001FFFF8,31C001C01E60039807E00000001C0038,61C601C0007FFF8006E0001C001C0038,01FC01CC007003800CE1FFF0001C0038,01EFFFFE0070038018E00000001FFFF8,07C001C0007FFF8000E0000000186030,3FC301C00070038000E1FFFC000038,39C181C00070000000E1C01C00071C,01C1E1C0007000C000E1C01C00671C0E,01C0E1C0007FFFE000E1C01C00E70C6780,01C0E1C0007001C000E1C01C00E7006380,01C001C0007001C000E1C01C01C70063C0,01C001C0007001C000E1FFFC03C7007180,01C03F80007FFFC000E1C01C0007FFF0,01C00780007001C000E1C01C0001FFC0,018000000060000000C0,00,\n"
                                + "^FO21,203^GFA,00448,00448,00016,00,0018,003C0000000030180003000C,0038038003BFF83C0FFFFFFE,007FFFC006E1807800E0001C,0078070000E180E000E0001C,00EC0E0000E180C000C01C18,01CE1E0000E1818001C03818,03073C0000E1830001C03838,0603F00000E18E0001803838,0C01E00000E1801E038C3838,0007F00000E1BC3C03FE3838,001E3E000FFFE078038C3038,00F9CFFF00E180E0078C3033,0F81E1F800E181C00F8C7FFF80,3801C0C000E183800F8C0003,0001C1E000E18E001B8C0003,03FFFFF000C1980E038C0007,0031C00001C1801F038CFFF7,0039CC0001C1803C038D8007,0079C7000381807803FC0007,00E1C3C0038180F0038C0007,01C1C1F0070181C0038C0006,0381C07006018700038001CE,0E1FC0780C018E000300007E,1803C030180038000000003C,00030000000060,00,\n"
                                + "^FO21,264^GFA,00448,00448,00016,00,00000C00003806,00300E00003C0780007FFFF0,001C1C000738E70000700060,001E180003B9CE0000700060,000E380001FB8E00007FFFE0,000C300001BB0C0000700060,01C061C00038FC0E007FFFE0,01FFFFC00FFFFFFF80700060,01C001C000781C3800600003,01C001C000FFBC381FFFFFFF80,01C001C001FBBC38,01C001C00339EC3000600070,01C001C00E386C30007FFFF0,01C001C01830C67000707070,01FFFFC00078067000707070,01C700000071C670007FFFF0,0063C0000FFFC7E000707070,0079E1C000E383E0007FFFF0,0670E37001C703C000707070,0670633C01F701C000007018,0E70031C001F03E000007038,1E70031C003BC7F001FFFFE0,3C7007800070DC7C000070,003FFF8003C0381F80007007,000000001E01E00C0FFFFFFF80,000000000003,00,\n"
                                + "^FO291,264^GFA,00560,00560,00020,00,0030000000300C,003C0000001C0F00000000300070E0,007800000C180E00007FFFF80070E3FFC0,0070000007180E00007000700070E38380,00E000E003D80E0C007000700070FB8380,01FFFFE001DFFFFE0070007003FFFF8380,01C000E001980E00007000700070E38380,038000E000380E00007000700070E38380,070000E000F80E0000700070007FE3FF80,0FFFF8E007980E00007000700070E38380,0DC070E01E180E3C007000700070E38380,19C070C00C19FBE0007FFFF00070E38380,31C070C00019C00000700070007FE38380,01C070C00018E000007000700070E38380,01C070C00000601E007000700070E38380,01FFF1C01FFFFFFF007000700070FBFF80,01C071C00003E0300070007007FFFF8380,01C01FCC000F3078007000700000038380,01C0078C001C18E0007000700039830380,01C0030C007C0F80007000700038E70380,01C0000C01EC0E00007000700070F70380,01C0000C070C1B80007FFFF000E07E0380,01C0001E1C0CF1F00070007001C07C0380,00FFFFFE001F807F807000700300381F80,001FFFF0000E001E0060006006007007,00000000000C0000000000000000C0,00,\n"
                                + "^PQ1,0,1,Y \n"
                                + "^XZ";
                    string s300 = "^FO37,30^GFA,00840,00840,00020,00,00,00000600000000018030000000000030,0000078000000001F03E00000380003E,000007C000000001E03C000001E0003C,0000038000000001C03C030001E00038,0000038000000001C03C078001C00038,00C0038018003FFFFFFFFFC001C03038,00F003801E001E01C03C000001C03C38,00F803801E000001C03C000001C03C38,00F003801C000001C03C000001C03838,00F003801C000001C03C000001C038381C,00F003801C000019C03C000001C038387E,00F003801C00001E0000000001C03839FC,00F003801C00003F0000038001C0383F1C,00F003801C00003C000007C001DE38781C,00F003801C000079FFFFFFE07FFF39F81C,00F003801C0000780000780001C03F381C,00F003801C0000F00000780001C0F8381C,00FFFFFFFC0001E00000780001C7B8381C,006003801E0001F00030780001CE38381C,00000380180003F8E078780001C038381C,00000380000007F0FFF8780001C038381C,0000038000000EF0F070780001C038381C,01E003800E001CF0F070780001C038381C,01F003800F8038F0F070780001C038381C,01E003800F0070F0F070780001C0383FFC,01E003800F0000F0F070780001C03838FC,01E003800F0000F0F070780001C038387980,01E003800F0000F0F070780001C3B8380180,01E003800F0000F0FFF0780001DE38380180,01E003800F0000F0F070780001F838300180,01E003800F0000F0F07078000FC038000180,01E003800F0000F0F00078007F0038000180,01E003800F0000F0E0007800FC00380003C0,03E003800F0000F00000780070003C0003E0,03FFFFFFFF0000F0001F780000003FFFFFC0,00C000000F0000F00007F80000001FFFFF80,000000000F0000F00001F0,00000000000000C00000E0,00,00, \n"
                                + "^FO473,31^GFA,00504,00504,00012,00,00,00000000070000000000C0,0000018007C000380001E0,000003E00380003FFFFFF0,03FFFFF00380003C0001E0,001C1C038380003C0001E0,001C1C03C380003C0001E0,001C1C03C380003C0001E0,001C1C03C380003C0001E0,001C1C03C380003C0001E0,001C1CE3C380003C0001E0,001C1DF3C380003C0001E0,0FFFFFFFC380003FFFFFE0,001C1C03C380003C0001E0,001C1C03C380003C000180,001C1C03C3800000000003,003C1C03C38000000000078000381C03C3803FFFFFFFFFC000381C0003801801E0,00701C0003800001E0,00F01C0007800003C0,00E01C01FF800003C0,01C01C007F80000780,03801B001F0000078000F0,060003E00C00000FFFFFF8,1C000380000000070000F0,00000380180000000000E0,000003803C0000000001E0,007FFFFFFE0000000001E0,00300380000000000001E0,00000380000000000001E0,00000380000000000001C0,00000380000000000003C0,00000380000000000003C0,00000380038000001E0780,0000038007C0000003FF80,0FFFFFFFFFE0000001FF,000000000000000000FE,00000000000000000070,00,00, \n"
                                + "^FO59,199^GFA,00504,00504,00012,00,00,00038007000000600000C0,0003C007C00000780001F0,0303870F8000007FFFFFE0,01C3878F000000780000E0,01E38F0F000000780000E0,00F38E0E0000007FFFFFE0,00F39C1E000000780000E0,00F3B81C000000780000E0,0063B31C00C000780000E0,000387BC01F0007FFFFFE0,0FFFFFFFFFF800780000E0,000F80380F0000780000C0,001FC0780F000060000001C0003FF87C0F007FFFFFFFFFE0007BBEEC0E003C,00F39EEC0E,01C38ECC0E000060000060,0383878C1E00007FFFFFF0,0E03830C1E0000700700F0,1C06030E1E0000700700F0,300F800E1C0000700700F0,000F00061C0000700700F0,000E0E073C00007FFFFFF0,1FFFFF073C0000700700F0,001C1E07380000700700F0,00381C03F80000700700F0,00383C03F000007FFFFFF0,00703801F00000700700E0,00FC7801E000000007,000FF003F0000000070038,0001FC07F800000007007C,0003FF0F7C0003FFFFFFFE,000F0F1E3E00000007,003E03BC1F8000000700018000F800F00FF00000070003C003C001C003F03FFFFFFFFFE01E00070001C01E,00001C,00,00, \n"
                                + "^FO20,491^GFA,01008,01008,00024,00,00,0018001E00000070001C0000001801800000000078,001F001E0000007C003E0000003E00E00000000078,001C001E000000F0003C0000003C00F000000000E0,001C001E000000E01878030000380078000000E0C003C0,001C001E060001E03C7007800078007C000000FFFFFFE0,071C001E0F0003FFFEFFFFC000700038060000F00003C0,079C1FFFFF80038E01C3800000F000300F0000F0000380,0F1C0C1E000007070181C00000E7FFFFFF8000F0000380,0F1C001E00000E071C01E00001E00000000000F0000380,0F1C001E00001C078F00E00001C00000000000F0000380,0E1C701E000030070780E00003C00000000000FFFFFF80,0E1DF81E000000000780018003C00000380000F0000380,0FFFFC1E00C00600070003C007E000007C0000F0000380,1C1C001E01E007FFFFFFFFE007C07FFFFE0000F0000380,1C1C7FFFFFF00E00000007C00FC00000000000F0000380,181C3000C0001E00000007000FC00000000000FFFFFF80,181C000070003E1800038E001DC00000000000F0000380,301C00007C003C1FFFFFCC0039C00000180000F0000380,301C38007000001C0007800031C000007C0000F0000380,001CE0007060001C0007800061C0FFFFFE0000F0000380,001F800070F0001C00078000C1C00000000000FFFFFFC0,001E7FFFFFF8001C0007800001C00000000000F00003C0,00FC30007000001C0007800001C00000000000F0600380,07FC00007000001FFFFF800001C060001800000038,3FDC0E007000001C0007800001C07FFFFE0000001E,1F1C07007000001C0006000001C078003C00001E1F,0C1C03807000001C0000000001C078003C00031F0F8060,0C1C03C07000001C0000C00001C078003C00071E070038,001C01E07000001FFFFFE00001C078003C00061E07063E,001C01E07000001C0001E00001C078003C000E1E00061F,001C01E07000001C0001C00001C078003C000E1E00060F,001C00C07000001C0001C00001C078003C001E1E00060F,001C00007000001C0001C00001C078003C003E1E000707,001C00007000001C0001C00001C07FFFFC007E1E000F87,001C003FF000001FFFFFC00001C078003C00380FFFFF80,001C0007F000001C0001C00001C078003C00000FFFFF,001C0003E000001C0001C00001C0700038,001C00018000001C0000000001C0,00,00, \n"
                                + "^FO34,299^GFA,00840,00840,00020,00,0000C0,0000E0,0001F0000000000000C00C000001800018,0001E000C000000001E01E000003F0003C,0003C001E0001FFFFFF01F007FFFFFFFFE,0007FFFFF8000E781C003E0001E0000038,0007C003E00000781C00780001C0000038,000FC007C00000781C00F00001C0038038,001E6007800000781C00E00003C003E038,001C700F800000781C01C0000380038078,0038381F000000781C0380000380038078,00701C3E000000781C0700000780078078,00E00E7C000000781C0C00000700078070,01C00FF8000000781C1803000700078070,030007E0000000781C0007800F0E070070,060007E0000000781CE00FC00FFF070070,00001FF8000000781DF01F000F0E070070,00007C7F00003FFFFFF83E001F0E0700F0,0003F31FF80000781C007C001F0E0700F0,001F83E7FFF800781C00F0003F0E0F00F380,00FC03E0FFC000781C01E0003F0E0FFFFFE0,1F8003C00F8000781C03C0006F0E06000380,000003C0000000701C070000EF0E00000380,000003C0380000701C1E0000CF0E00000380,00FFFFFFFE0000701C3801800F0E00000380,007003C0000000F01C6003C00F0E0000C380,000003C0000000F01C0007F00F0E0001E780,000383C0000000E01C000FC00F0E7FFFF780,0003E3CE000001E01C001F000F0E000007,0007C3C3C00001C01C003E000F0E000007,000F83C1F00001C01C007C000FFE000007,001F03C0FC0003801C00F0000F0E000007,003C03C07E0003801C01E0000F0F00000F,007803C01F0007003C07C0000F0000000F,00F003C01F800E003C0F00000F00000FDE,01C1FFC00F800C003C3C00000E000001FE,07003FC0078018003870000000000000FC,0C000F800300300001C000000000000078,000007000000000007,00,00, \n"
                                + "^FO34,389^GFA,00840,00840,00020,00,00,0000000600000007000E000000C0000180,0003800F80000007800F800000F00003E0,0001C00F800006070E1F000000FFFFFFC0,0000F00E000003870F1E000000F00001C0,0000F01E000003C71E1E000000F00001C0,0000F81C000001E71C1C000000FFFFFFC0,00007838000001E7383C000000F00001C0,00007030000001E77038000000F00001C0,00000060300000C76638018000F00001C0,001C0060780000070F7803E000FFFFFFC0,001FFFFFF8001FFFFFFFFFF000F00001C0,001C00007000001F00701E0000F0000180,001C00007000003F80F01E0000C000000380,001C00007000007FF0F81E00FFFFFFFFFFC0,001C0000700000F77DD81C0078,001C0000700001E73DD81C,001C0000700003871D981C0000C00000C0,001C0000700007070F183C0000FFFFFFE0,001C000070001C0706183C0000E00E01E0,001C00007000380C061C3C0000E00E01E0,001FFFFFF800601F001C380000E00E01E0,001C00007800001E000C380000E00E01E0,001C1C006000001C1C0E780000FFFFFFE0,00000F0000003FFFFE0E780000E00E01E0,00060780000000383C0E700000E00E01E0,00C383E0700000703807F00000E00E01E0,00C3C1E01C0000707807E00000FFFFFFE0,01C381E0CF0000E07003E00000E00E01C0,01C380E0C7C001F8F003C00000000E,03C38000C3E0001FE007E00000000E0070,07838000C3E00003F80FF00000000E00F8,0F838001C1E00007FE1EF80007FFFFFFFC,1F838001C1C0001E1E3C7C0000000E,0E03C001E000007C07783F0000000E0003,0003FFFFE00001F001E01FE000000E000780,0001FFFFC0000780038007E07FFFFFFFFFC0,0000000000003C000E0003803C,000000000000000038,00,00, \n"
                                + "^FO438,394^GFA,01008,01008,00024,00,00,00038000000000070018000000000000000003,0003E00000000007801F000000000000C0000380F0,0003C00000000007001E000000E00000F00003C0F0600F,0007800000000C07001C000000FFFFFFF0000380E07FFF800007800000000787001C000000E00000E0000380E0700F,000F0000180003C7001C000000E00000E0000380EC700F,001E00003C0001E7001C070000E00000E0000380FE700F,001FFFFFFE0001E7001C0F8000E00000E0007FFFFF700F,003C00003C0000E77FFFFFE000E00000E0000380E0700F,003800003C000007001C000000E00000E0000380E0700F,007000003C00000F001C000000E00000E0000380E0700F,00F000003C00003F001C000000E00000E0000380E0700F,01F000603C0000F7001C000000E00000E00003FFE07FFF,01FFFFF83C0003C7001C000000E00000E0000380E0700F,039C00783C001F07001C0E0000E00000E0000380E0700F,071C00703C003E07001C3F0000E00000E0000380E0700F,0E1C00703C001C071FFFFF8000FFFFFFE0000380E0700F,1C1C00703C0000073800000000E00000E0000380E0700F,301C00703C0000071E00000000E00000E00003FFE0700F,001C00703C0000070F80000000E00000E0000380E0700F,001C00703C0000060780070000E00000E0000380E0700F,001C00703800000007800F8000E00000E0000380E0700F,001FFFF038003FFFFFFFFFC000E00000E0000380EC7FFF,001C0070380018003F00300000E00000E0000380FEF00F,001C0060380000007980380000E00000E000FFFFFFF00F,001C001C78C00000F1C07C0000E00000E000000000F00F,001C0007F8C00003E0E0F80000E00000E00000C000F00F,001C0003F0C00007C071E00000E00000E00001E380E00F,001C0001C0C0001FC07B800000E00000E00001F1E1E00F,001C000000C0007FC03E000000E00000E00003C0F1E00F,001C000000C000F3C01E000000E00000E00007807BC00F,001C000000E007C3C00F800000E00000E0000F007B800F,001C000001E01E03C1C3E00000FFFFFFE0000E0037000F,001E000001F07003DF01FC0000E00000E0001C000F000F,001FF0003FF00003FC00FFF000E00000E00038000E03FF,000FFFFFFFE00003F0003FC000E00000E000600018007E,00000FFFFC000003C0000F8000C000000001C00070003C,0000000000000001800000000000000000000000C0,00,00, \n"
                                + "^PQ1,0,1,Y \n"
                                + "^XZ";

                    // 라벨 디자인
                    string sLabel = string.Empty;
                           sLabel = dtRslt.Rows[0]["LABELCD"].ToString();

                    // Unicode -> 16진수 변환
                    string enZpl = string.Empty;
                           enZpl = ZebraEncode(sLabel);

                    // 최종 라벨 디자인
                    string sZPL = string.Empty;
                    // 해상도 구분
                    if (sRes.Equals("300"))
                    {
                        // 추가된 바코드 4개미만일 경우 바코드 출력 삭제 처리
                        string item17 = "^BY1^FO260,301^BCN,41,N,N,N^FD>:^FS";
                        string item18 = "^BY1^FO470,301^BCN,40,N,N,N^FD>:^FS";
                        string item19 = "^BY1^FO686,301^BCN,43,N,N,N^FD>:^FS";
                        string item20 = "^BY1^FO894,301^BCN,41,N,N,N^FD>:^FS";
                        if(cnt == 3)
                        {
                            enZpl = enZpl.Replace(item20, "");
                        }
                        else if(cnt == 2)
                        {
                            enZpl = enZpl.Replace(item20, "").Replace(item19, "");
                        }
                        else if (cnt == 1)
                        {
                            enZpl = enZpl.Replace(item20, "").Replace(item19, "").Replace(item18, "");
                        }
                        else if (cnt == 0)
                        {
                            enZpl = enZpl.Replace(item20, "").Replace(item19, "").Replace(item18, "").Replace(item17, "");
                        }
                        sZPL = enZpl.Replace("^XZ", s300);
                    } else
                    {
                        // 추가된 바코드 4개미만일 경우 바코드 출력 삭제 처리
                        string item17 = "^BY1^FO158,204^BCN,28,N,N^FD>:^FS";
                        string item18 = "^BY1^FO300,204^BCN,27,N,N^FD>:^FS";
                        string item19 = "^BY1^FO447,204^BCN,29,N,N^FD>:^FS";
                        string item20 = "^BY1^FO587,204^BCN,28,N,N^FD>:^FS";
                        if (cnt == 3)
                        {
                            enZpl = enZpl.Replace(item20, "");
                        }
                        else if (cnt == 2)
                        {
                            enZpl = enZpl.Replace(item20, "").Replace(item19, "");
                        }
                        else if (cnt == 1)
                        {
                            enZpl = enZpl.Replace(item20, "").Replace(item19, "").Replace(item18, "");
                        }
                        else if (cnt == 0)
                        {
                            enZpl = enZpl.Replace(item20, "").Replace(item19, "").Replace(item18, "").Replace(item17, "");
                        }
                        sZPL = enZpl.Replace("^XZ", s203);
                    }
                    

                    if (PrintZPL(sZPL, drPrtInfo))
                    { 
                        DataRow drLog = dtLog.NewRow();

                        drLog["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "PALLETID"));
                        drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                        drLog["LABEL_ZPL_CNTT"] = enZpl; // nvarchar(4000)자리수때문에 라벨타이틀 중국어 제외
                        drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                        drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                        drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                        drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                        drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                        drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                        drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                        drLog["PRT_ITEM07"] = dtRqst.Rows[0]["ATTVAL007"];
                        drLog["PRT_ITEM08"] = dtRqst.Rows[0]["ATTVAL008"];
                        drLog["PRT_ITEM09"] = dtRqst.Rows[0]["ATTVAL009"];
                        drLog["PRT_ITEM10"] = dtRqst.Rows[0]["ATTVAL010"];
                        drLog["PRT_ITEM11"] = dtRqst.Rows[0]["ATTVAL011"];
                        drLog["PRT_ITEM12"] = dtRqst.Rows[0]["ATTVAL012"];
                        drLog["PRT_ITEM13"] = dtRqst.Rows[0]["ATTVAL013"];
                        drLog["PRT_ITEM14"] = dtRqst.Rows[0]["ATTVAL014"];
                        //drLog["PRT_ITEM15"] = dtRqst.Rows[0]["ATTVAL015"];
                        //drLog["PRT_ITEM16"] = dtRqst.Rows[0]["ATTVAL016"];
                        drLog["PRT_ITEM17"] = dtRqst.Rows[0]["ATTVAL017"];
                        drLog["PRT_ITEM18"] = dtRqst.Rows[0]["ATTVAL018"];
                        drLog["PRT_ITEM19"] = dtRqst.Rows[0]["ATTVAL019"];
                        drLog["PRT_ITEM20"] = dtRqst.Rows[0]["ATTVAL020"];
                        drLog["PRT_ITEM21"] = dtRqst.Rows[0]["ATTVAL021"];
                        //drLog["PRT_ITEM22"] = dtRqst.Rows[0]["ATTVAL022"];
                        drLog["INSUSER"] = txtWorker.Tag as string;

                        dtLog.Rows.Add(drLog);

                        PrintLog(dtLog);
                        dtLog.Rows.Remove(drLog);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 중국어 깨짐현상으로 16진수로 변환
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string ZebraEncode(string text)
        {
            var ret = new StringBuilder();

            var unicodeCharacterList = new Dictionary<char, string>();
            foreach (var ch in text)
            {
                if (!unicodeCharacterList.ContainsKey(ch))
                {
                    var bytes = Encoding.UTF8.GetBytes(ch.ToString());
                    if (bytes.Length > 1)
                    {
                        var hexCode = string.Empty;
                        foreach (var b in bytes)
                        {
                            hexCode += $"_{BitConverter.ToString(new byte[] { b }).ToLower()}";
                        }

                        unicodeCharacterList[ch] = hexCode;
                    }
                    else
                        unicodeCharacterList[ch] = ch.ToString();

                    ret.Append(unicodeCharacterList[ch]);
                }
                else
                    ret.Append(unicodeCharacterList[ch]);
            };

            return ret.ToString();
        }
        #endregion

        private void getZpl(string I_ATTVAL, string LabelCode)
        {
            try
            {
                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: LabelCode
                                                      , sRESO:"203"
                                                     , sATTVAL: I_ATTVAL
                                                     );

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl = dtResult.Rows[0]["ZPLSTRING"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            txtnetweight.IsReadOnly = false;
            txtgrossweight.IsReadOnly = false;
            txtratedpower.IsReadOnly = false;

            txtnetweight.Text = string.Empty;
            txtgrossweight.Text = string.Empty;
            txtratedpower.Text = string.Empty;

        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            txtnetweight.IsReadOnly = true;
            txtgrossweight.IsReadOnly = true;
            txtratedpower.IsReadOnly = true;

            txtnetweight.Text = string.Empty;
            txtgrossweight.Text = string.Empty;
            txtratedpower.Text = string.Empty;
        }
    }
}
