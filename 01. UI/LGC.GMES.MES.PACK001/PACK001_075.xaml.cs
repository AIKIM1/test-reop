using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// Page1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_075 : UserControl, IWorkArea
    {
        //완료 메세지 에러 메세지를 위한 전역 변수 선언
        private Boolean bOkNg;

        int iDetailIndex = 0;
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        //불량유형 code 선택을 위한 전역 변수 선언
        private DataTable dtCodeResult = new DataTable();
        private DataTable defectResult = new DataTable();

        #region [ Init ]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_075()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtArea.Text = LoginInfo.CFG_AREA_NAME;
            txtArea.Tag = LoginInfo.CFG_AREA_ID;

            List<Button> listAuth = new List<Button>();

            listAuth.Add(selectCancel);
            listAuth.Add(Confirm);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            dgLotListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            initCombo();

            GradeProcessInfo.Visibility = Visibility.Collapsed;

            this.Loaded -= C1Window_Loaded;
        }


        #endregion

        #region [ Combo ]

        private void initCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                C1ComboBox[] cboBatTypeChild = { cboBatDetail };
                string[] strFilter = { "PACK_UI_BAT_TYPE" };
                _combo.SetCombo(cboBatType, CommonCombo.ComboStatus.SELECT, cbChild: cboBatTypeChild, sFilter: strFilter, sCase: "COMMCODE");

                C1ComboBox[] cboBatDetailParent = { cboBatType };
                string[] strDetailFilter = { "PACK_UI_BAT_DETAIL" };
                _combo.SetCombo(cboBatDetail, CommonCombo.ComboStatus.SELECT, cbParent: cboBatDetailParent, sFilter: strDetailFilter, sCase: "COMMCODEATTR2");

                String[] sFilter = { "RESP_DEPT" };
                _combo.SetCombo(cboReasonDept, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

                searchProcDefectCode();
                searchScrapCode();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [ Event ]
        private void cboBatDetail_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {

                Util.gridClear(dgpProcDefectCode);
                Util.gridClear(dgpProcDefectCode0);
                Util.gridClear(dgpProcDefectCode1);
                Util.gridClear(dgpProcDefectCode2);
                Util.gridClear(dgpProcDefectCode3);
                Util.gridClear(dgpProcDefectCode4);
                Util.gridClear(dgScrapCode);

                searchProcDefectCode();
                searchScrapCode();
                txtReason.Clear();
                txtWipNote.Clear();
                // 평가감(등급품 제품 변경 - DEVAL_PROCESS) 이 아니라면 사용 안함
                GradeProcessInfo.Visibility = Visibility.Collapsed;
                txtUserName.IsEnabled = false;
                btnUser.IsEnabled = false;
                btnFile.IsEnabled = false;
                SetItemClear();

                // 일괄처리작업유형값 및 일괄처리작업상세유형값에 따른 Control Enable / Disable 처리 수정필요함.
                if (cboBatDetail.SelectedIndex != 0)
                {
                    if (cboBatDetail.SelectedValue.ToString().Equals("REWORK_INPUT") || cboBatDetail.SelectedValue.ToString().Equals("SCRAP_REQUEST_CANCEL"))
                    {
                        dgpProcDefectCode.IsEnabled = false;
                        dgpProcDefectCode0.IsEnabled = false;
                        dgpProcDefectCode1.IsEnabled = false;
                        dgpProcDefectCode2.IsEnabled = false;
                        dgpProcDefectCode3.IsEnabled = false;
                        dgpProcDefectCode4.IsEnabled = false;
                        dgScrapCode.IsEnabled = false;
                        txtReason.IsEnabled = true;
                    }
                    else if (cboBatDetail.SelectedValue.ToString().Equals("REWORK_WAIT"))
                    {
                        dgpProcDefectCode.IsEnabled = true;
                        dgpProcDefectCode0.IsEnabled = true;
                        dgpProcDefectCode1.IsEnabled = true;
                        dgpProcDefectCode2.IsEnabled = true;
                        dgpProcDefectCode3.IsEnabled = true;
                        dgpProcDefectCode4.IsEnabled = true;
                        dgScrapCode.IsEnabled = false;
                        txtReason.IsEnabled = false;
                    }
                    else if (this.cboBatType.SelectedValue.ToString().Equals("REWORK_LOT") && cboBatDetail.SelectedValue.ToString().Equals("SCRAP_WAIT"))
                    {
                        dgpProcDefectCode.IsEnabled = false;
                        dgpProcDefectCode0.IsEnabled = false;
                        dgpProcDefectCode1.IsEnabled = false;
                        dgpProcDefectCode2.IsEnabled = false;
                        dgpProcDefectCode3.IsEnabled = false;
                        dgpProcDefectCode4.IsEnabled = false;
                        dgScrapCode.IsEnabled = false;
                        txtReason.IsEnabled = true;
                    }
                    else if (this.cboBatType.SelectedValue.ToString().Equals("SCRAP_LOT") && cboBatDetail.SelectedValue.ToString().Equals("SCRAP_CONFIRM"))
                    {
                        dgpProcDefectCode.IsEnabled = false;
                        dgpProcDefectCode0.IsEnabled = false;
                        dgpProcDefectCode1.IsEnabled = false;
                        dgpProcDefectCode2.IsEnabled = false;
                        dgpProcDefectCode3.IsEnabled = false;
                        dgpProcDefectCode4.IsEnabled = false;
                        dgScrapCode.IsEnabled = true;
                        txtReason.IsEnabled = false;
                    }
                    else if (cboBatDetail.SelectedValue.ToString().Equals("OCOP_SCRAP_CONFIRM") || cboBatDetail.SelectedValue.ToString().Equals("OCOP_REWORK"))
                    {
                        dgpProcDefectCode.IsEnabled = false;
                        dgpProcDefectCode0.IsEnabled = false;
                        dgpProcDefectCode1.IsEnabled = false;
                        dgpProcDefectCode2.IsEnabled = false;
                        dgpProcDefectCode3.IsEnabled = false;
                        dgpProcDefectCode4.IsEnabled = false;
                        dgScrapCode.IsEnabled = false;
                        txtReason.IsEnabled = false;
                        txtReason.Text = ObjectDic.Instance.GetObjectName("자동");
                    }
                    else if (cboBatType.SelectedValue.ToString().Equals("RETURN"))
                    {
                        dgpProcDefectCode.IsEnabled = false;
                        dgpProcDefectCode0.IsEnabled = false;
                        dgpProcDefectCode1.IsEnabled = false;
                        dgpProcDefectCode2.IsEnabled = false;
                        dgpProcDefectCode3.IsEnabled = false;
                        dgpProcDefectCode4.IsEnabled = false;
                        dgScrapCode.IsEnabled = false;
                        txtReason.Text = ObjectDic.Instance.GetObjectName("자동");
                        txtReason.IsEnabled = false;
                        return;
                    }
                    else if (cboBatType.SelectedValue.ToString().Equals("GRADE_PROCESS"))
                    {
                        GradeProcessInfo.Visibility = Visibility.Visible;
                        dgpProcDefectCode.IsEnabled = false;
                        dgpProcDefectCode0.IsEnabled = false;
                        dgpProcDefectCode1.IsEnabled = false;
                        dgpProcDefectCode2.IsEnabled = false;
                        dgpProcDefectCode3.IsEnabled = false;
                        dgpProcDefectCode4.IsEnabled = false;
                        dgScrapCode.IsEnabled = false;
                        txtReason.Text = ObjectDic.Instance.GetObjectName("자동");
                        txtReason.IsEnabled = false;
                        txtUserName.IsEnabled = true;
                        btnUser.IsEnabled = true;
                        btnFile.IsEnabled = true;

                        Util.gridClear(dgLotList);
                        return;
                    }
                    //2023-04-10 라우트 변경 [확정]버튼 클릭시 (ABnomal case 추가)
                    else if (cboBatType.SelectedValue.ToString().Equals("ROUTE_LOT"))
                    {
                        dgpProcDefectCode.IsEnabled = false;
                        dgpProcDefectCode0.IsEnabled = false;
                        dgpProcDefectCode1.IsEnabled = false;
                        dgpProcDefectCode2.IsEnabled = false;
                        dgpProcDefectCode3.IsEnabled = false;
                        dgpProcDefectCode4.IsEnabled = false;
                        dgScrapCode.IsEnabled = false;
                        txtReason.Text = ObjectDic.Instance.GetObjectName("자동");
                        txtReason.IsEnabled = false;
                        //txtWipNote.Text = "ABNORMAL"; // ABNORMAL : 일괄처리 // NORMAL : PALLET 포장 해제
                        return;
                    }
                    else
                    {
                        dgpProcDefectCode.IsEnabled = true;
                        dgpProcDefectCode0.IsEnabled = true;
                        dgpProcDefectCode1.IsEnabled = true;
                        dgpProcDefectCode2.IsEnabled = true;
                        dgpProcDefectCode3.IsEnabled = true;
                        dgpProcDefectCode4.IsEnabled = true;
                        dgScrapCode.IsEnabled = true;
                        txtReason.IsEnabled = false;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void cboBatType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboBatType.SelectedIndex != 0)
            {
                if (cboBatType.SelectedValue.ToString().Equals("EXTERNAL_RETURN"))
                {
                    this.dgLotList.Columns["OCOP_RTN_FLAG"].Visibility = Visibility.Visible;
                    this.dgLotList.Columns["OCOP_INSP_NAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    this.dgLotList.Columns["OCOP_RTN_FLAG"].Visibility = Visibility.Collapsed;
                    this.dgLotList.Columns["OCOP_INSP_NAME"].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgpProcDefectCode0_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgpProcDefectCode0.SelectedItem as DataRowView;

            string[] prefix = new string[1];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];

            txtReason.Clear();
            Util.gridClear(dgpProcDefectCode);
            Util.gridClear(dgpProcDefectCode1);
            Util.gridClear(dgpProcDefectCode2);
            Util.gridClear(dgpProcDefectCode3);
            Util.gridClear(dgpProcDefectCode4);

            Util.GridSetData(dgpProcDefectCode1, getNextRESNCODE(prefix), FrameOperation, true);
            dgpProcDefectCode1.AllColumnsWidthAuto();
        }

        private void dgpProcDefectCode1_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgpProcDefectCode0.SelectedItem as DataRowView;
            DataRowView r1 = dgpProcDefectCode1.SelectedItem as DataRowView;

            String[] prefix = new string[2];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];

            txtReason.Clear();
            Util.gridClear(dgpProcDefectCode);
            Util.gridClear(dgpProcDefectCode2);
            Util.gridClear(dgpProcDefectCode3);
            Util.gridClear(dgpProcDefectCode4);

            Util.GridSetData(dgpProcDefectCode2, getNextRESNCODE(prefix), FrameOperation, true);
            dgpProcDefectCode2.AllColumnsWidthAuto();
        }

        private void dgpProcDefectCode2_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgpProcDefectCode0.SelectedItem as DataRowView;
            DataRowView r1 = dgpProcDefectCode1.SelectedItem as DataRowView;
            DataRowView r2 = dgpProcDefectCode2.SelectedItem as DataRowView;

            String[] prefix = new string[3];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[2] = r2.Row.ItemArray[0].ToString().Split(':')[0];

            txtReason.Clear();
            Util.gridClear(dgpProcDefectCode);
            Util.gridClear(dgpProcDefectCode3);
            Util.gridClear(dgpProcDefectCode4);

            Util.GridSetData(dgpProcDefectCode3, getNextRESNCODE(prefix), FrameOperation, true);
            dgpProcDefectCode3.AllColumnsWidthAuto();
        }

        private void dgpProcDefectCode3_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgpProcDefectCode0.SelectedItem as DataRowView;
            DataRowView r1 = dgpProcDefectCode1.SelectedItem as DataRowView;
            DataRowView r2 = dgpProcDefectCode2.SelectedItem as DataRowView;
            DataRowView r3 = dgpProcDefectCode3.SelectedItem as DataRowView;

            String[] prefix = new string[4];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[2] = r2.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[3] = r3.Row.ItemArray[0].ToString().Split(':')[0];

            txtReason.Clear();
            Util.gridClear(dgpProcDefectCode);
            Util.gridClear(dgpProcDefectCode4);

            Util.GridSetData(dgpProcDefectCode4, getNextRESNCODE(prefix), FrameOperation, true);
            dgpProcDefectCode4.AllColumnsWidthAuto();
        }

        private void dgpProcDefectCode4_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgpProcDefectCode0.SelectedItem as DataRowView;
            DataRowView r1 = dgpProcDefectCode1.SelectedItem as DataRowView;
            DataRowView r2 = dgpProcDefectCode2.SelectedItem as DataRowView;
            DataRowView r3 = dgpProcDefectCode3.SelectedItem as DataRowView;
            DataRowView r4 = dgpProcDefectCode4.SelectedItem as DataRowView;

            String code = r0.Row.ItemArray[0].ToString().Split(':')[0] + r1.Row.ItemArray[0].ToString().Split(':')[0] + r2.Row.ItemArray[0].ToString().Split(':')[0] + r3.Row.ItemArray[0].ToString().Split(':')[0] + r4.Row.ItemArray[0].ToString().Split(':')[0];

            DataView dv = dtCodeResult.Copy().DefaultView;
            dv.RowFilter = "RESNCODE = '" + code + "'";
            Util.GridSetData(dgpProcDefectCode, dv.ToTable(false), FrameOperation, true);
            DataTableConverter.SetValue(dgpProcDefectCode.Rows[0].DataItem, "CHK", true);
        }

        private void txtLotId_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
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
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            Util.MessageInfo("SFU1190", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    HiddenLoadingIndicator();
                                    return;
                                }
                            });

                        }

                        if (overLapLot(Util.NVC(sPasteStrings[i])))
                        {
                            if (!searchLotId(Util.NVC(sPasteStrings[i])))
                            {
                                return;
                            }

                        }
                        else
                        {
                            Util.MessageInfo("SFU1384");
                            return;
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }

                if (e.Key.Equals(Key.Enter))
                {
                    if (string.IsNullOrWhiteSpace(txtLotId.Text.ToString()))
                    {
                        Util.MessageInfo("SFU1009");
                        return;
                    }

                    if (overLapLot(Util.NVC(txtLotId.Text)))
                    {
                        searchLotId(Util.NVC(txtLotId.Text));
                    }
                    else
                    {
                        Util.MessageInfo("SFU1384");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtLotId.Text = string.Empty;
                HiddenLoadingIndicator();
            }
        }

        private void selectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);

                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgLotList.ItemsSource);

                dt.AcceptChanges();

                foreach (DataRow drDel in dt.Rows)
                {
                    if (drDel["CHK"].Equals(true) || drDel["CHK"].ToString().ToUpper() == "TRUE" || drDel["CHK"].ToString() == "1")// MES 2.0 CHK 컬럼 Bool 오류 Patch
                    {
                        drDel.Delete();
                    }
                }

                dt.AcceptChanges();

                Util.GridSetData(dgLotList, dt, FrameOperation);
                Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Util.NVC(dt.Rows.Count));

                if (dt.Rows.Count == 0)
                {
                    cboBatType.IsEnabled = true;
                    cboBatDetail.IsEnabled = true;
                    SetItemClear();
                    if (defectResult != null && defectResult.Rows.Count > 0)
                    {
                        txtReason.Clear();
                        Util.gridClear(dgpProcDefectCode);
                        Util.gridClear(dgpProcDefectCode0);
                        Util.gridClear(dgpProcDefectCode1);
                        Util.gridClear(dgpProcDefectCode2);
                        Util.gridClear(dgpProcDefectCode3);
                        Util.gridClear(dgpProcDefectCode4);
                        Util.GridSetData(dgpProcDefectCode0, getNextRESNCODE(new string[0]), FrameOperation, true);
                    }
                    else
                    {
                        Util.AlertInfo("SFU5062"); //불량코드가 존재하지 않습니다.
                    }
                }
                else
                {
                    cboBatType.IsEnabled = false;
                    cboBatDetail.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgLotList.ItemsSource);

                dt.AcceptChanges();

                if (dt.Rows.Count > 100)
                {
                    Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                    return;
                }

                if (Util.GetCondition(cboBatType) == "") // 일괄작업유형
                {
                    string sBatType = ObjectDic.Instance.GetObjectName(tbBatType.Text.ToString()).Replace("[#] ", string.Empty);
                    Util.MessageValidation("SFU8393", sBatType); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                    return;
                }

                if (cboBatDetail.SelectedIndex == 0) // 일괄작업상세
                {
                    string sBatDetail = ObjectDic.Instance.GetObjectName(tbBatDetail.Text.ToString()).Replace("[#] ", string.Empty);
                    Util.MessageValidation("SFU8393", sBatDetail); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                    return;
                }

                if (cboBatDetail.SelectedValue.ToString().Equals("SCRAP_CONFIRM"))
                {
                    txtReason.Text = txtWipNote.Text;
                }

                if (string.IsNullOrWhiteSpace(txtWipNote.ToString()))
                {
                    Util.MessageValidation("SFU1590"); //비고를 입력 하세요.
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtReason.Text.ToString()))
                {
                    Util.MessageValidation("SFU1594");//사유를 입력하세요.
                    return;
                }


                if (Util.NVC(cboBatType.SelectedValue).Equals("GRADE_PROCESS"))
                {
                    if (cboTempProdID.SelectedIndex == 0)
                    {
                        string sTempProdID = ObjectDic.Instance.GetObjectName(tbTempProdID.Text.ToString()).Replace("[#] ", string.Empty);
                        Util.MessageValidation("SFU8393", sTempProdID); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                        return;
                    }
                    if (cboNewProdID.SelectedIndex == 0)
                    {
                        string sNewProdID = ObjectDic.Instance.GetObjectName(tbNewProdID.Text.ToString()).Replace("[#] ", string.Empty);
                        Util.MessageValidation("SFU8393", sNewProdID); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtFilePath.Text))
                    {
                        Util.MessageValidation("SFU4037");      // 승인증빙자료가 첨부되지 않았습니다.
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
                    {
                        Util.MessageValidation("SFU3451");      // 요청자를 입력 하세요.
                        return;
                    }
                }

                if (Util.NVC(cboBatType.SelectedValue).Equals("DEFECT_LOT"))
                {
                    this.chkLotProcid(dt);
                }
                else
                {
                    Confirmed(dt);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            attachFile(txtFilePath);
        }

        private void dgScrapeChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                bool WipNoteFlag = false;
                txtReason.Clear();

                RadioButton rbt = sender as RadioButton;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                string strResnName = Util.NVC(DataTableConverter.GetValue(dgScrapCode.Rows[dg.SelectedIndex].DataItem, "RESNNAME"));


                // 추후에는 해당 내용 사용
                // Util.gridFindDataRow_GetValue(ref dgScrapCode, "CHK", "True", "RESNCODE");
                //폐기확정 통일

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (txtWipNote.Text.Equals(Util.NVC(DataTableConverter.GetValue(dgScrapCode.Rows[i].DataItem, "RESNNAME"))))
                    {
                        WipNoteFlag = true;
                    }
                }

                if (cboBatDetail.SelectedIndex > 0) // 일괄작업상세
                {
                    if (cboBatDetail.SelectedValue.Equals("SCRAP_CONFIRM"))
                    {
                        if (String.IsNullOrEmpty(txtWipNote.Text) || txtWipNote.Text.Equals("") || WipNoteFlag == true)
                        {
                            txtReason.Text = "";
                            txtWipNote.Text = "";
                            txtReason.AppendText(strResnName);
                            txtWipNote.AppendText(strResnName);
                        }
                        else
                        {
                            txtReason.Text = txtWipNote.Text;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProcDefectChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {

                if (DataTableConverter.Convert(cboBatDetail.ItemsSource).Rows[cboBatDetail.SelectedIndex]["ATTRIBUTE4"].ToString().Equals("S")
                    && DataTableConverter.Convert(cboBatDetail.ItemsSource).Rows[cboBatDetail.SelectedIndex]["ATTRIBUTE5"].ToString().Equals("TERM"))
                    return;

                txtReason.Clear();

                RadioButton rbt = sender as RadioButton;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                string strResnName = Util.NVC(DataTableConverter.GetValue(dgpProcDefectCode.Rows[dg.SelectedIndex].DataItem, "RESNNAME"));

                txtReason.AppendText(strResnName);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
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
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            Util.MessageInfo("SFU1190", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    HiddenLoadingIndicator();
                                    return;
                                }
                            });

                        }

                        if (overLapLot(Util.NVC(sPasteStrings[i])))
                        {
                            if (!searchLotId(Util.NVC(sPasteStrings[i])))
                            {
                                return;
                            }

                        }
                        else
                        {
                            Util.MessageInfo("SFU1384");
                            return;
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }

                if (e.Key.Equals(Key.Enter))
                {
                    if (string.IsNullOrWhiteSpace(txtLotId.Text.ToString()))
                    {
                        Util.MessageInfo("SFU1009");
                        return;
                    }

                    if (overLapLot(Util.NVC(txtLotId.Text)))
                    {
                        searchLotId(Util.NVC(txtLotId.Text));
                    }
                    else
                    {
                        Util.MessageInfo("SFU1384");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtLotId.Text = string.Empty;
                HiddenLoadingIndicator();
            }
        }

        private void txtLotId_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    //if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    //{
                    //    Util.MessageInfo("FM_ME_0183");
                    //    return;
                    //}    




                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStringsTemp = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string[] sPasteStrings = new string[sPasteStringsTemp.Length];

                    if (sPasteStringsTemp.Length > 1 && string.IsNullOrWhiteSpace(sPasteStringsTemp[sPasteStringsTemp.Length - 1]))
                    {
                        sPasteStrings = new string[sPasteStringsTemp.Length - 1];
                        Array.Copy(sPasteStringsTemp, sPasteStrings, sPasteStringsTemp.Length - 1);
                    }
                    else
                    {
                        sPasteStrings = sPasteStringsTemp;
                    }

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    if (Util.GetCondition(cboBatType) == "") // 일괄작업유형
                    {
                        string sBatType = ObjectDic.Instance.GetObjectName(tbBatType.Text.ToString()).Replace("[#] ", string.Empty);
                        Util.MessageValidation("SFU8393", sBatType); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                        return;
                    }

                    if (cboBatDetail.SelectedIndex == 0) // 일괄작업상세
                    {
                        string sBatDetail = ObjectDic.Instance.GetObjectName(tbBatDetail.Text.ToString()).Replace("[#] ", string.Empty);
                        Util.MessageValidation("SFU8393", sBatDetail); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                        return;
                    }

                    if (cboBatType.SelectedValue.ToString().Equals("GRADE_PROCESS"))
                    {
                        SetExceptionPopUpLotID(sPasteStrings);
                    }
                    else if (cboBatType.SelectedValue.ToString().Equals("RETURN") || cboBatType.SelectedValue.ToString().Equals("EXTERNAL_RETURN"))
                    {
                        SetExceptionPopUpLotID2(sPasteStrings);
                    }
                    else
                    {
                        setNormalSearchLOTID(sPasteStrings);
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //txtLotId.Text = string.Empty;
                HiddenLoadingIndicator();
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtLotId.Text.ToString()))
                    {
                        Util.MessageInfo("SFU1190"); //조회할 LOTID를 입력하세요.
                        return;
                    }

                    if (Util.GetCondition(cboBatType) == "") // 일괄작업유형
                    {
                        string sBatType = ObjectDic.Instance.GetObjectName(tbBatType.Text.ToString()).Replace("[#] ", string.Empty);
                        Util.MessageValidation("SFU8393", sBatType); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                        return;
                    }

                    if (cboBatDetail.SelectedIndex == 0) // 일괄작업상세
                    {
                        string sBatDetail = ObjectDic.Instance.GetObjectName(tbBatDetail.Text.ToString()).Replace("[#] ", string.Empty);
                        Util.MessageValidation("SFU8393", sBatDetail); //선택오류: 필수조건을 선택하지 않았습니다. [%1]
                        return;
                    }

                    if (overLapLot(Util.NVC(txtLotId.Text)))
                    {
                        if (cboBatType.SelectedValue.ToString().Equals("GRADE_PROCESS"))
                        {
                            string[] sPasteStrings = new string[] { Util.NVC(txtLotId.Text) };
                            SetExceptionPopUpLotID(sPasteStrings);
                        }
                        else
                        {
                            if (!searchLotId(Util.NVC(txtLotId.Text)))
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        Util.MessageInfo("SFU1384"); //LOT이 이미 있습니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboBatType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                if (dgLotList.GetRowCount() > 0)
                {
                    iDetailIndex = cboBatDetail.SelectedIndex;

                    Util.MessageConfirm("SFU8330", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Util.gridClear(dgLotList);
                            Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Convert.ToString(dgLotList.GetRowCount()));

                            cboBatType.IsEnabled = true;
                            cboBatDetail.IsEnabled = true;

                            Util.MessageInfo("SFU3377");
                        }
                        else
                        {
                            cboBatType.SelectedIndexChanged -= cboBatType_SelectedIndexChanged;

                            cboBatType.SelectedIndex = e.OldValue;

                            cboBatType.SelectedIndexChanged += cboBatType_SelectedIndexChanged;

                            cboBatDetail.SelectedIndex = iDetailIndex;

                            return;
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void rdoNormalGrade_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.GetCondition(txtCurrProduct) == "")
                {
                    string sCurrProdID = ObjectDic.Instance.GetObjectName(tbCurrProduct.Text.ToString()).Replace("[#] ", string.Empty);
                    Util.MessageValidation("SFU5070", sCurrProdID); // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                    rdoNormalGrade.IsChecked = false;
                    return;
                }

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("PRODID", typeof(string));
                RQSTDT1.Columns.Add("ATTR_FLAG", typeof(string));
                RQSTDT1.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT1.NewRow();
                dr["PRODID"] = Util.NVC(txtCurrProduct.Text);
                dr["ATTR_FLAG"] = 'Y';
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT1.Rows.Add(dr);

                DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_OFFGRADE", "RQSTDT", "RSLTDT", RQSTDT1);

                cboTempProdID.DisplayMemberPath = "CBO_NAME";
                cboTempProdID.SelectedValuePath = "CBO_CODE";
                cboTempProdID.ItemsSource = AddStatus(dtResult1, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboTempProdID.SelectedValue = Util.NVC(txtCurrProduct.Text);
                cboTempProdID.IsEnabled = false;

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("PRODID", typeof(string));
                RQSTDT2.Columns.Add("SHOPID", typeof(string));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["PRODID"] = Util.NVC(txtCurrProduct.Text);
                dr2["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT2.Rows.Add(dr2);

                DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_OFFGRADE_MODEL", "RQSTDT", "RSLTDT", RQSTDT2);

                cboNewProdID.DisplayMemberPath = "CBO_NAME";
                cboNewProdID.SelectedValuePath = "CBO_CODE";
                cboNewProdID.ItemsSource = AddStatus(dtResult2, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboNewProdID.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoModelGrade_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.GetCondition(txtCurrProduct) == "")
                {
                    string sCurrProdID = ObjectDic.Instance.GetObjectName(tbCurrProduct.Text.ToString()).Replace("[#] ", string.Empty);
                    Util.MessageValidation("SFU5070", sCurrProdID); // %1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                    rdoModelGrade.IsChecked = false;
                    return;
                }

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("PRODID", typeof(string));
                RQSTDT1.Columns.Add("ATTR_FLAG", typeof(string));
                RQSTDT1.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT1.NewRow();
                dr["PRODID"] = Util.NVC(txtCurrProduct.Text);
                dr["ATTR_FLAG"] = 'N';
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT1.Rows.Add(dr);

                DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_OFFGRADE", "RQSTDT", "RSLTDT", RQSTDT1);

                cboTempProdID.DisplayMemberPath = "CBO_NAME";
                cboTempProdID.SelectedValuePath = "CBO_CODE";
                cboTempProdID.ItemsSource = AddStatus(dtResult1, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboTempProdID.SelectedIndex = 0;
                cboTempProdID.IsEnabled = true;


                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("PRODID", typeof(string));
                RQSTDT2.Columns.Add("SHOPID", typeof(string));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["PRODID"] = Util.NVC(txtCurrProduct.Text);
                dr2["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT2.Rows.Add(dr2);

                DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_OFFGRADE_MODEL", "RQSTDT", "RSLTDT", RQSTDT2);

                cboNewProdID.DisplayMemberPath = "CBO_NAME";
                cboNewProdID.SelectedValuePath = "CBO_CODE";
                cboNewProdID.ItemsSource = AddStatus(dtResult2, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboNewProdID.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
        #endregion


        #region [Method]
        private void searchProcDefectCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));
                //RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["ACTID"] = "DEFECT_LOT";
                //dr["DFCT_TYPE_CODE"] = sSelectedProcID;
                RQSTDT.Rows.Add(dr);

                dtCodeResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAT_ACTIVITYREASON_DEFECT", "RQSTDT", "RSLTDT", RQSTDT);

                setDefectResult();

                if (defectResult != null && defectResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgpProcDefectCode0, getNextRESNCODE(new string[0]), FrameOperation, true);
                }
                else
                {
                    Util.AlertInfo("SFU5062"); //불량코드가 존재하지 않습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void searchScrapCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));
                //RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["ACTID"] = "SCRAP_LOT";
                //dr["DFCT_TYPE_CODE"] = sSelectedProcID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BAT_ACTIVITYREASON_SCRAP", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgScrapCode, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Boolean searchLotId(string strLotId)
        {
            try
            {
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BATCH_TYPE", typeof(string));
                INDATA.Columns.Add("BATCH_DETAIL", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BATCH_TYPE"] = Util.NVC(cboBatType.SelectedValue);
                dr["BATCH_DETAIL"] = Util.NVC(cboBatDetail.SelectedValue);
                dr["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));

                DataRow drLot = INLOT.NewRow();
                drLot["LOTID"] = strLotId;
                INLOT.Rows.Add(drLot);

                dsInput.Tables.Add(INLOT);

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_BATCH_PROCESSING", "INDATA,INLOT", "OUTDATA,OUTDATA_EXCPTION", dsInput, null);

                if (ds.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string area = LoginInfo.CFG_AREA_ID.ToString();

                    if (!ds.Tables["OUTDATA"].Rows[0]["EQSGID"].ToString().Substring(0, area.Length).Equals(area))
                    {
                        Util.MessageValidation("SFU9220");  //SFU9220 : 설정된 동 정보와 입력한 LOT의 동 정보가 일치하지 않습니다.
                        return false;
                    }

                    if (dgLotList.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgLotList, ds.Tables["OUTDATA"], FrameOperation);
                        Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Util.NVC(ds.Tables["OUTDATA"].Rows.Count));

                        //cboBatType.IsEnabled = false;
                        //cboBatDetail.IsEnabled = false;
                    }
                    else
                    {
                        DataTable dtLotList = DataTableConverter.Convert(dgLotList.ItemsSource);

                        dtLotList.Merge(ds.Tables["OUTDATA"]);

                        Util.GridSetData(dgLotList, dtLotList, FrameOperation);
                        Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Util.NVC(dtLotList.Rows.Count));

                        //cboBatType.IsEnabled = false;
                        //cboBatDetail.IsEnabled = false;
                    }

                }
                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private Boolean batchProcess(string strLotId)
        {
            try
            {
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BATCH_TYPE", typeof(string));
                INDATA.Columns.Add("BATCH_DETAIL", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("PROC_RESNCODE", typeof(string));
                INDATA.Columns.Add("SCRAP_RESNCODE", typeof(string));
                INDATA.Columns.Add("DEFECT_RESNCODE", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("WIPNOTE", typeof(string));
                // 등급품 변경(평가감) INDATA
                INDATA.Columns.Add("CURR_PRODID", typeof(string));
                INDATA.Columns.Add("TEMP_PRODID", typeof(string));
                INDATA.Columns.Add("NEW_PRODID", typeof(string));
                INDATA.Columns.Add("REQ_USERID", typeof(string));
                INDATA.Columns.Add("ATTCH_FILE_CNTT", typeof(Byte[]));
                INDATA.Columns.Add("ATTCH_FILE_NAME", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BATCH_TYPE"] = Util.NVC(cboBatType.SelectedValue);
                dr["BATCH_DETAIL"] = Util.NVC(cboBatDetail.SelectedValue);
                dr["USERID"] = LoginInfo.USERID;
                dr["PROC_RESNCODE"] = Util.gridFindDataRow_GetValue(ref dgpProcDefectCode, "CHK", "True", "RESNCODE");// MES 2.0 CHK 컬럼 Bool 오류 Patch
                dr["SCRAP_RESNCODE"] = //Util.NVC(cboBatDetail.SelectedValue);
                dr["DEFECT_RESNCODE"] = Util.gridFindDataRow_GetValue(ref dgScrapCode, "CHK", "True", "RESNCODE");// MES 2.0 CHK 컬럼 Bool 오류 Patch
                dr["RESNNOTE"] = Util.NVC(txtReason.Text.ToString());
                dr["WIPNOTE"] = Util.NVC(txtWipNote.Text.ToString());
                if (Util.NVC(cboBatType.SelectedValue).Equals("GRADE_PROCESS"))
                {
                    dr["CURR_PRODID"] = Util.NVC(txtCurrProduct.Text.ToString());
                    dr["TEMP_PRODID"] = Util.NVC(cboTempProdID.SelectedValue);
                    dr["NEW_PRODID"] = Util.NVC(cboNewProdID.SelectedValue);
                    dr["REQ_USERID"] = Util.NVC(txtUserName.Tag);
                    dr["ATTCH_FILE_CNTT"] = File.ReadAllBytes(Util.GetCondition(txtFilePath));
                    dr["ATTCH_FILE_NAME"] = System.IO.Path.GetFileName(Util.GetCondition(txtFilePath));
                }

                DataTable dtTemp = DataTableConverter.Convert(dgpProcDefectCode.ItemsSource);

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));

                DataRow drLot = INLOT.NewRow();
                drLot["LOTID"] = strLotId;
                INLOT.Rows.Add(drLot);

                dsInput.Tables.Add(INLOT);

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_BATCH_PROCESSING", "INDATA,INLOT", "", dsInput, null);

                //완료 메세지 에러 메세지를 위한 전역 변수 선언
                bOkNg = true;
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bOkNg = false;
                return false;
            }
        }

        private void chkLotProcid(DataTable dt)
        {
            try
            {
                if (dt.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU1190"); //조회할 LOTID를 입력하세요.
                    return;
                }

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "dtRQSTDT";
                dtRQSTDT.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dtRQSTDT.NewRow();
                    dr["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);

                    dtRQSTDT.Rows.Add(dr);
                }
                DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_PROCID_COMPLETE", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dsResult != null)
                {
                    if (dsResult.Rows.Count > 0)
                    {
                        StringBuilder stringBuilder = new StringBuilder();

                        foreach (DataRow row in dsResult.Rows)
                        {
                            string LOTID = row["LOTID"].ToString();
                            if (stringBuilder.Length > 0)
                            {
                                stringBuilder.Append("\r\n");
                            }
                            stringBuilder.Append(LOTID);
                        }

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU8257"), stringBuilder.ToString(), "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Confirmed(dt);
                            }
                        });

                    }
                    else
                    {
                        Confirmed(dt);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return;
        }

        private void Confirmed(DataTable dt)
        {
            foreach (DataRow drDel in dt.Rows)
            {
                if (drDel["CHK"].Equals(true) || drDel["CHK"].ToString().ToUpper() == "TRUE" || drDel["CHK"].ToString() == "1")// MES 2.0 CHK 컬럼 Bool 오류 Patch
                {
                    bool bchkOK = false;

                    bchkOK = batchProcess(drDel["LOTID"].ToString());

                    if (bchkOK)
                    {
                        drDel.Delete();
                    }
                    else
                    {
                        break;
                    }

                }
            }

            dt.AcceptChanges();

            if (bOkNg)
            {
                Util.MessageInfo("SFU1275");//정상처리되었습니다.
            }

            Util.GridSetData(dgLotList, dt, FrameOperation);
            Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Util.NVC(dt.Rows.Count));

            if (dt.Rows.Count == 0)
            {
                cboBatType.IsEnabled = true;
                cboBatDetail.IsEnabled = true;
            }

            SetItemClear();
        }
        private Boolean overLapLot(string strLotId)
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);

                if (dt.Rows.Count == 0)
                {
                    return true;
                }

                if (dt.Select("LOTID = '" + strLotId + "'").Count() > 0)
                {
                    return false;

                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetUserWindow()
        {
            try
            {
                CMM_PERSON wndPerson = new CMM_PERSON();
                wndPerson.FrameOperation = FrameOperation;

                if (wndPerson != null)
                {
                    string sUserName = txtUserName.Text;

                    object[] Parameters = new object[1];
                    Parameters[0] = sUserName;
                    C1WindowExtension.SetParameters(wndPerson, Parameters);

                    wndPerson.Closed += new EventHandler(wndUser_Closed);
                    grdMain.Children.Add(wndPerson);
                    wndPerson.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;
            }
            else if (wndPerson.DialogResult == MessageBoxResult.Cancel)
            {
                txtUserName.Text = "";
                txtUserName.Tag = "";
            }
        }

        private void attachFile(TextBox txtBox)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "All files (*.*)|*.*";

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    openFileDialog.InitialDirectory = @"\\Client\C$";
                }

                else
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        if (new System.IO.FileInfo(filename).Length > 5 * 1024 * 1024) //파일크기 체크
                        {
                            Util.AlertInfo("SFU1926");  //첨부파일 크기는 5M 이하입니다.

                            txtBox.Text = string.Empty;
                        }
                        else
                        {
                            txtBox.Text = filename;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void setNormalSearchLOTID(string[] sPasteStrings)
        {
            for (int i = 0; i < sPasteStrings.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(sPasteStrings[i]))
                {
                    Util.MessageInfo("SFU1190"); //조회할 LOTID를 입력하세요.
                    return;

                }
                //그리드에 이미 등록 된 LOT인지 체크
                if (overLapLot(Util.NVC(sPasteStrings[i])))
                {
                    if (!searchLotId(Util.NVC(sPasteStrings[i]))) //
                    {
                        return;
                    }

                }
                else
                {
                    Util.MessageInfo("SFU1384"); //LOT이 이미 있습니다.
                    return;
                }

                System.Windows.Forms.Application.DoEvents();
            }
        }

        private void SetExceptionPopUpLotID(string[] sPasteStrings)
        {
            bool bErrorFlag = false;   // PRODID Validation 결과 이상이 있을 경우 true / 정상일 경우 false 
            string sMainPRODID = "";   // 첫 입력LOT의 제품ID  OR  Grid에 이미 데이터가 있다면 Grid의 첫번째 LOT 제품ID
            string sTargetPRODID = ""; // 비교할 대상

            DataSet dsValidation = new DataSet();
            DataTable dtExPopup = dsValidation.Tables.Add("ExceptionTarget");
            dtExPopup.Columns.Add("LOTID", typeof(string));       // 최상위 LOTID (조회 결과LOTID)
            dtExPopup.Columns.Add("WIPSTAT", typeof(string));     // WIP 상태
            dtExPopup.Columns.Add("PRODID", typeof(string));      // 제품ID
            dtExPopup.Columns.Add("GRD_PRODID", typeof(string));  // 등급품코드
            dtExPopup.Columns.Add("NOTE", typeof(string));        // 사유

            DataTable dtValidation = dsValidation.Tables.Add("ProductVali");
            dtValidation.Columns.Add("CHK", typeof(bool));// MES 2.0 CHK 컬럼 Bool 오류 Patch
            dtValidation.Columns.Add("LOTID", typeof(string));
            dtValidation.Columns.Add("PRODID", typeof(string));
            dtValidation.Columns.Add("GRD_PRODID", typeof(string));
            dtValidation.Columns.Add("PRODNAME", typeof(string));
            dtValidation.Columns.Add("PJT", typeof(string));
            dtValidation.Columns.Add("PRDT_CLSS_CODE", typeof(string));
            dtValidation.Columns.Add("EQSGID", typeof(string));
            dtValidation.Columns.Add("EQSGNAME", typeof(string));
            dtValidation.Columns.Add("PROCID", typeof(string));
            dtValidation.Columns.Add("WIPSTAT", typeof(string));
            dtValidation.Columns.Add("WIPSNAME", typeof(string));
            dtValidation.Columns.Add("WIPHOLD", typeof(string));
            dtValidation.Columns.Add("NOTE", typeof(string));


            for (int i = 0; i < sPasteStrings.Length; i++)
            {
                DataSet dsResult = null;

                if (string.IsNullOrWhiteSpace(sPasteStrings[i]))
                {
                    DataRow drMessage = dtExPopup.NewRow();
                    drMessage["LOTID"] = sPasteStrings[i].ToString();
                    drMessage["WIPSTAT"] = null;
                    drMessage["PRODID"] = null;
                    drMessage["GRD_PRODID"] = null;
                    drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU1190");     //SFU1190 : 조회할 LOT ID 를 입력하세요
                    dtExPopup.Rows.Add(drMessage);

                }
                else
                {
                    if (overLapLot(Util.NVC(sPasteStrings[i])))
                    {
                        dsResult = searchLotId2(Util.NVC(sPasteStrings[i]));    // Lot 조회

                        // 입력 lotid의 동정보 불일치
                        if (dsResult == null)
                        {
                            DataRow drMessage = dtExPopup.NewRow();
                            drMessage["LOTID"] = sPasteStrings[i].ToString();
                            drMessage["WIPSTAT"] = null;
                            drMessage["PRODID"] = null;
                            drMessage["GRD_PRODID"] = null;
                            drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU9220");  //SFU9220 : 설정된 동 정보와 입력한 LOT의 동 정보가 일치하지 않습니다.
                            dtExPopup.Rows.Add(drMessage);
                        }

                        // LOT 조회 BR에서 Error 일경우 
                        if (dsResult.Tables["OUTDATA_EXCPTION"].Rows.Count > 0)
                        {
                            if (dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["LOT_UNABLE"].ToString().Equals("Y"))
                            {
                                DataRow drMessage = dtExPopup.NewRow();
                                drMessage["LOTID"] = sPasteStrings[i].ToString();
                                drMessage["WIPSTAT"] = null;
                                drMessage["PRODID"] = null;
                                drMessage["GRD_PRODID"] = null;
                                drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU1905");  //SFU1905 : 조회된 Data가 없습니다.
                                dtExPopup.Rows.Add(drMessage);
                            }
                            else if (dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["WIPSTAT_ERR"].ToString().Equals("TERM") ||
                                     dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["WIPSTAT_ERR"].ToString().Equals("MOVING"))
                            {
                                string[] ParmMessage = { sPasteStrings[i].ToString(), dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["WIPSTAT_ERR"].ToString() };
                                DataRow drMessage = dtExPopup.NewRow();
                                drMessage["LOTID"] = sPasteStrings[i].ToString();
                                drMessage["WIPSTAT"] = dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["WIPSTAT_ERR"].ToString();
                                drMessage["PRODID"] = null;
                                drMessage["GRD_PRODID"] = null;
                                drMessage["NOTE"] = MessageDic.Instance.GetMessage("1027", ParmMessage);  // LOT [%1]의 WIP 상태가 [%2] 입니다.
                                dtExPopup.Rows.Add(drMessage);
                            }
                        }

                        // 조회결과 있을경우
                        if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            bool checkProdID = false;


                            if (dgLotList.Rows.Count == 0 && dtValidation.Rows.Count == 0)
                            {
                                if (Util.NVC(cboBatDetail.SelectedValue).Equals("PROCESS_BACK"))
                                {
                                    sMainPRODID = dsResult.Tables["OUTDATA"].Rows[0]["GRD_PRODID"].ToString();
                                }
                                else
                                {
                                    sMainPRODID = dsResult.Tables["OUTDATA"].Rows[0]["PRODID"].ToString();
                                }
                                checkProdID = true;
                            }
                            else
                            {
                                if (dgLotList.Rows.Count > 0)
                                {
                                    if (Util.NVC(cboBatDetail.SelectedValue).Equals("PROCESS_BACK"))
                                    {
                                        sMainPRODID = DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "GRD_PRODID").ToString();
                                        sTargetPRODID = dsResult.Tables["OUTDATA"].Rows[0]["GRD_PRODID"].ToString();
                                    }
                                    else
                                    {
                                        sMainPRODID = DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "PRODID").ToString();
                                        sTargetPRODID = dsResult.Tables["OUTDATA"].Rows[0]["PRODID"].ToString();
                                    }

                                }
                                else if (dtValidation.Rows.Count > 0)
                                {
                                    if (Util.NVC(cboBatDetail.SelectedValue).Equals("PROCESS_BACK"))
                                    {
                                        sTargetPRODID = dsResult.Tables["OUTDATA"].Rows[0]["GRD_PRODID"].ToString();
                                    }
                                    else
                                    {
                                        sTargetPRODID = dsResult.Tables["OUTDATA"].Rows[0]["PRODID"].ToString();
                                    }
                                }

                                checkProdID = sMainPRODID.Equals(sTargetPRODID);
                            }

                            if (!checkProdID)
                            {
                                bErrorFlag = true;
                                dsResult.Tables["OUTDATA"].Rows[0]["NOTE"] = MessageDic.Instance.GetMessage("SFU4338"); // 동일한 제품만 작업 가능합니다.
                            }

                            if (!Util.NVC(cboBatDetail.SelectedValue).Equals("PROCESS_BACK") &&
                                dsResult.Tables["OUTDATA"].Rows[0]["PROCID"].ToString().Equals("PD000")) // 이미 제품 변경이 이루어진 LOT을 조회시 에러처리 
                            {
                                bErrorFlag = true;
                                dsResult.Tables["OUTDATA"].Rows[0]["NOTE"] = MessageDic.Instance.GetMessage("SFU8397"); // 이미 등급품 변경으로 제품이 변경된 LOT입니다.
                            }
                            else if (Util.NVC(cboBatDetail.SelectedValue).Equals("PROCESS_BACK") &&
                                    !dsResult.Tables["OUTDATA"].Rows[0]["PROCID"].ToString().Equals("PD000"))
                            {
                                bErrorFlag = true;
                                dsResult.Tables["OUTDATA"].Rows[0]["NOTE"] = MessageDic.Instance.GetMessage("SFU8398"); // 제품 변경이력이 없는 LOT입니다.
                            }

                            dtValidation.Merge(dsResult.Tables["OUTDATA"]);

                        }

                    }
                    else
                    {
                        DataRow drMessage = dtExPopup.NewRow();
                        drMessage["LOTID"] = sPasteStrings[i].ToString();
                        drMessage["WIPSTAT"] = null;
                        drMessage["PRODID"] = null;
                        drMessage["GRD_PRODID"] = null;
                        drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU2835");  //SFU2835 : Grid에 중복된 ID가 있습니다.
                        dtExPopup.Rows.Add(drMessage);

                    }
                }

                System.Windows.Forms.Application.DoEvents();
            }

            if (bErrorFlag)
            {
                DataTable dtClone = dtValidation.AsEnumerable().CopyToDataTable();
                int index = 0;
                int iremove = 0;
                foreach (DataRow drMoveError in dtClone.Rows)
                {
                    if (dgLotList.Rows.Count == 0)
                    {
                        DataRow drMessage = dtExPopup.NewRow();
                        drMessage["LOTID"] = drMoveError["LOTID"].ToString();
                        drMessage["WIPSTAT"] = drMoveError["WIPSTAT"].ToString();
                        drMessage["PRODID"] = drMoveError["PRODID"].ToString();
                        drMessage["GRD_PRODID"] = drMoveError["GRD_PRODID"].ToString();
                        drMessage["NOTE"] = drMoveError["NOTE"].ToString();
                        dtExPopup.Rows.Add(drMessage);

                        dtValidation.Rows.RemoveAt(0); // dgLotList 그리드에 정보 안뿌려지게 remove 처리
                    }
                    else if (!string.IsNullOrWhiteSpace(dtClone.Rows[index]["NOTE"].ToString()))
                    {
                        DataRow drMessage = dtExPopup.NewRow();
                        drMessage["LOTID"] = drMoveError["LOTID"].ToString();
                        drMessage["WIPSTAT"] = drMoveError["WIPSTAT"].ToString();
                        drMessage["PRODID"] = drMoveError["PRODID"].ToString();
                        drMessage["GRD_PRODID"] = drMoveError["GRD_PRODID"].ToString();
                        drMessage["NOTE"] = drMoveError["NOTE"].ToString();
                        dtExPopup.Rows.Add(drMessage);

                        dtValidation.Rows.RemoveAt(iremove); // dgLotList 그리드에 정보 안뿌려지게 remove 처리
                    }
                    else
                    {
                        iremove++;
                    }
                    index++;
                }
            }

            if (dtValidation.Rows.Count > 0)
            {
                if (dgLotList.GetRowCount() == 0)
                {
                    Util.GridSetData(dgLotList, dtValidation, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Util.NVC(dtValidation.Rows.Count));

                    txtCurrProduct.Text = sMainPRODID;
                    if (Util.NVC(cboBatDetail.SelectedValue).Equals("PROCESS_BACK"))
                    {
                        rdoNormalGrade.IsChecked = false;
                        rdoModelGrade.IsChecked = false;
                        rdoNormalGrade.IsEnabled = false;
                        rdoModelGrade.IsEnabled = false;

                        cboTempProdID.ItemsSource = null;
                        cboNewProdID.ItemsSource = null;
                        cboTempProdID.IsEnabled = false;
                        cboNewProdID.IsEnabled = false;
                    }
                    // 첫 입력시 등급품전환-일반 이면서 제품타입이 PROD(반제품)을 제외한 나머지는 등급품 처리유형 "등급품 전환" 고정
                    else if (Util.NVC(cboBatDetail.SelectedValue).Equals("DEVALUATION_NORMAL") && dtValidation.Rows[0]["PRODTYPE"].ToString().Equals("PROD"))
                    {
                        rdoNormalGrade.IsChecked = false;
                        rdoModelGrade.IsChecked = false;
                        rdoNormalGrade.IsEnabled = true;
                        rdoModelGrade.IsEnabled = true;
                    }
                    else
                    {
                        if (rdoNormalGrade.IsChecked == true)
                        {
                            rdoNormalGrade.IsChecked = false;
                        }
                        rdoNormalGrade.IsChecked = true;
                        rdoNormalGrade.IsEnabled = false;
                        rdoModelGrade.IsEnabled = false;
                    }

                }
                else
                {
                    DataTable dtLotList = DataTableConverter.Convert(dgLotList.ItemsSource);

                    dtLotList.Merge(dtValidation);

                    Util.GridSetData(dgLotList, dtLotList, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Util.NVC(dtLotList.Rows.Count));

                }

            }

            if (dtExPopup.Rows.Count > 0)
            {
                COM001_035_PACK_EXCEPTION_POPUP popup = new COM001_035_PACK_EXCEPTION_POPUP();
                popup.FrameOperation = FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = dtExPopup;
                    Parameters[1] = "BATCH_PROCESSING_OFFGRADE";

                    C1WindowExtension.SetParameters(popup, Parameters);

                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }
            }
        }

        private void SetExceptionPopUpLotID2(string[] sPasteStrings)
        {
            DataSet dsValidation = new DataSet();
            DataTable dtExPopup = dsValidation.Tables.Add("ExceptionTarget");
            dtExPopup.Columns.Add("LOTID", typeof(string));       // 최상위 LOTID (조회 결과LOTID)
            dtExPopup.Columns.Add("PROCID", typeof(string));      // 공정
            dtExPopup.Columns.Add("WIPHOLD", typeof(string));     // WIP 홀드 여부
            dtExPopup.Columns.Add("RESNCODE", typeof(string));    // 홀드 사유
            dtExPopup.Columns.Add("NOTE", typeof(string));        // 사유

            DataTable dtValidation = dsValidation.Tables.Add("ResonCodeVali");
            //dtValidation.Columns.Add("CHK", typeof(bool));// MES 2.0 CHK 컬럼 Bool 오류 Patch
            dtValidation.Columns.Add("CHK", typeof(long));// 2024.11.25. 김영국 - DataType MisMatch로 로직 수정함.
            dtValidation.Columns.Add("LOTID", typeof(string));
            dtValidation.Columns.Add("PRODID", typeof(string));
            dtValidation.Columns.Add("PRODNAME", typeof(string));
            dtValidation.Columns.Add("PJT", typeof(string));
            dtValidation.Columns.Add("PRDT_CLSS_CODE", typeof(string));
            dtValidation.Columns.Add("EQSGID", typeof(string));
            dtValidation.Columns.Add("EQSGNAME", typeof(string));
            dtValidation.Columns.Add("PROCID", typeof(string));
            dtValidation.Columns.Add("WIPSTAT", typeof(string));
            dtValidation.Columns.Add("WIPSNAME", typeof(string));
            dtValidation.Columns.Add("WIPHOLD", typeof(string));
            dtValidation.Columns.Add("NOTE", typeof(string));


            for (int i = 0; i < sPasteStrings.Length; i++)
            {
                DataSet dsResult = null;

                if (string.IsNullOrWhiteSpace(sPasteStrings[i]))
                {
                    DataRow drMessage = dtExPopup.NewRow();
                    drMessage["LOTID"] = sPasteStrings[i].ToString();
                    drMessage["PROCID"] = null;
                    drMessage["WIPHOLD"] = null;
                    drMessage["RESNCODE"] = null;
                    drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU1190");     //SFU1190 : 조회할 LOT ID 를 입력하세요
                    dtExPopup.Rows.Add(drMessage);

                }
                else
                {
                    if (overLapLot(Util.NVC(sPasteStrings[i])))
                    {
                        dsResult = searchLotId2(Util.NVC(sPasteStrings[i]));    // Lot 조회

                        // 입력 lotid의 동정보 불일치
                        if (dsResult == null)
                        {
                            DataRow drMessage = dtExPopup.NewRow();
                            drMessage["LOTID"] = sPasteStrings[i].ToString();
                            drMessage["PROCID"] = null;
                            drMessage["WIPHOLD"] = null;
                            drMessage["RESNCODE"] = null;
                            drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU9220");  //SFU9220 : 설정된 동 정보와 입력한 LOT의 동 정보가 일치하지 않습니다.
                            dtExPopup.Rows.Add(drMessage);
                        }

                        // LOT 조회 BR에서 Error 일경우 
                        if (dsResult.Tables["OUTDATA_EXCPTION"].Rows.Count > 0)
                        {
                            if (dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["LOT_UNABLE"].ToString().Equals("Y"))
                            {
                                DataRow drMessage = dtExPopup.NewRow();
                                drMessage["LOTID"] = sPasteStrings[i].ToString();
                                drMessage["PROCID"] = null;
                                drMessage["WIPHOLD"] = null;
                                drMessage["RESNCODE"] = null;
                                drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU1905");  //SFU1905 : 조회된 Data가 없습니다.
                                dtExPopup.Rows.Add(drMessage);
                            }
                            else if (!string.IsNullOrEmpty(dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["PROCID"].ToString()))
                            {
                                string[] ParmMessage = { sPasteStrings[i].ToString() };
                                DataRow drMessage = dtExPopup.NewRow();
                                drMessage["LOTID"] = sPasteStrings[i].ToString();
                                drMessage["PROCID"] = dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["PROCID"].ToString();
                                drMessage["WIPHOLD"] = null;
                                drMessage["RESNCODE"] = null;
                                drMessage["NOTE"] = MessageDic.Instance.GetMessage("101541", ParmMessage);  // 현재 LOTID [%1] 는 반품공정이 아닙니다.
                                dtExPopup.Rows.Add(drMessage);
                            }
                            else if (!string.IsNullOrEmpty(dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["RESNCODE"].ToString()))
                            {
                                DataRow drMessage = dtExPopup.NewRow();
                                drMessage["LOTID"] = sPasteStrings[i].ToString();
                                drMessage["PROCID"] = null;
                                drMessage["WIPHOLD"] = dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["WIPHOLD"].ToString();
                                drMessage["RESNCODE"] = dsResult.Tables["OUTDATA_EXCPTION"].Rows[0]["RESNCODE"].ToString();
                                drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU8434");  //SFU8434 : 홀드 사유 확인 후 수동 Release 처리가 필요한 LOT 입니다.
                                dtExPopup.Rows.Add(drMessage);
                            }
                        }

                        // 조회결과 있을경우
                        if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {

                            dtValidation.Merge(dsResult.Tables["OUTDATA"]);

                        }

                    }
                    else
                    {
                        DataRow drMessage = dtExPopup.NewRow();
                        drMessage["LOTID"] = sPasteStrings[i].ToString();
                        drMessage["PROCID"] = null;
                        drMessage["WIPHOLD"] = null;
                        drMessage["RESNCODE"] = null;
                        drMessage["NOTE"] = MessageDic.Instance.GetMessage("SFU2835");  //SFU2835 : Grid에 중복된 ID가 있습니다.
                        dtExPopup.Rows.Add(drMessage);

                    }
                }

                System.Windows.Forms.Application.DoEvents();
            }

            if (dtValidation.Rows.Count > 0)
            {
                if (dgLotList.GetRowCount() == 0)
                {
                    Util.GridSetData(dgLotList, dtValidation, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Util.NVC(dtValidation.Rows.Count));

                }
                else
                {
                    DataTable dtLotList = DataTableConverter.Convert(dgLotList.ItemsSource);

                    dtLotList.Merge(dtValidation);

                    Util.GridSetData(dgLotList, dtLotList, FrameOperation);
                    Util.SetTextBlockText_DataGridRowCount(dgLotListCount, Util.NVC(dtLotList.Rows.Count));

                }

            }

            if (dtExPopup.Rows.Count > 0)
            {
                COM001_035_PACK_EXCEPTION_POPUP popup = new COM001_035_PACK_EXCEPTION_POPUP();
                popup.FrameOperation = FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = dtExPopup;
                    Parameters[1] = "BATCH_PROCESSING_RETURN";

                    C1WindowExtension.SetParameters(popup, Parameters);

                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }
            }
        }

        private DataSet searchLotId2(string strLotId)
        {
            try
            {
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BATCH_TYPE", typeof(string));
                INDATA.Columns.Add("BATCH_DETAIL", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BATCH_TYPE"] = Util.NVC(cboBatType.SelectedValue);
                dr["BATCH_DETAIL"] = Util.NVC(cboBatDetail.SelectedValue);
                dr["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));

                DataRow drLot = INLOT.NewRow();
                drLot["LOTID"] = strLotId;
                INLOT.Rows.Add(drLot);

                dsInput.Tables.Add(INLOT);

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_BATCH_PROCESSING", "INDATA,INLOT", "OUTDATA,OUTDATA_EXCPTION", dsInput, null);

                string area = LoginInfo.CFG_AREA_ID.ToString();

                if (ds.Tables["OUTDATA"].Rows.Count > 0) // 2024.11.06. 김영국 - OUTDATA가 있는 경우만 로직 처리.
                {
                    if (!ds.Tables["OUTDATA"].Rows[0]["EQSGID"].ToString().Substring(0, area.Length).Equals(area))
                    {
                        return null;
                    }
                }

                return ds;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw ex;
            }
        }

        private void SetItemClear()
        {
            // 등급품재고 변경 클리어처리 (초기화)
            txtCurrProduct.Text = "";
            rdoModelGrade.IsChecked = false;
            rdoNormalGrade.IsChecked = false;
            cboTempProdID.SelectedIndex = 0;
            cboTempProdID.ItemsSource = null;
            cboNewProdID.SelectedIndex = 0;
            cboNewProdID.ItemsSource = null;
            txtUserName.Text = "";
            txtUserName.Tag = "";
            txtFilePath.Text = "";
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        #endregion

        private void setDefectResult()
        {
            try
            {
                if (dtCodeResult == null || dtCodeResult.Rows.Count < 1)
                {
                    return;
                }

                DataTable INDATA = dtCodeResult.Copy().DefaultView.ToTable(false, new string[] { "RESNCODE" });
                INDATA.TableName = "INDATA";


                DataColumn col = new DataColumn();

                col.DataType = typeof(string);
                col.ColumnName = "LANGID";
                col.DefaultValue = LoginInfo.LANGID;

                INDATA.Columns.Add(col);

                defectResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DFCT_INFO", "RQSTDT", "RSLTDT", INDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량유형코드선택창 각 유형 grid에 들어갈 데이터 선별 함수
        /// <para>
        /// dsResult.Tables["ACTIVITYREASON"], defectResult 조회가 선행되어야 함
        /// </para>
        /// <param name="pre">이전까지 선택된 모든 code string[].</param>
        /// </summary>
        private DataTable getNextRESNCODE(string[] pre)
        {
            if (pre == null) pre = new string[0];

            if (defectResult != null)
            {
                if (defectResult.Rows.Count > 0)
                {
                    DataTable dt = defectResult.Copy();
                    SortedSet<String> set = new SortedSet<String>();

                    int k = -1;
                    for (int i = 0; i < pre.Length; i++)
                    {
                        DataView dv = dt.DefaultView;
                        dv.RowFilter = dt.Columns[i * 2].ColumnName + " = '" + pre[i] + "' ";
                        dt = dv.ToTable();
                        k = i;
                    }

                    k = (k + 1) * 2;
                    if (dt.Columns.Count <= k + 1)
                    {
                        return null;
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        set.Add(Convert.ToString(dr[k]) + ":" + Convert.ToString(dr[k + 1]));
                    }

                    DataTable resdt = new DataTable();
                    resdt.Columns.Add("RESNCODE", typeof(string));

                    foreach (String val in set)
                    {
                        resdt.Rows.Add(val);
                    }

                    return resdt;
                }
            }

            return null;
        }
    }
}