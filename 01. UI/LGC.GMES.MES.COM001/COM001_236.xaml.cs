/*************************************************************************************
 Created Date : 2018.05.17
      Creator : JMK
   Decription : 파우치 활성화 불량 등외품 출고
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.17  JMK : Initial Created.

 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_236 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_236()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOut);
            listAuth.Add(btnOutCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        #region 불량 등외품 출고 

        /// <summary>
        /// 출고 선택
        /// </summary>
        private void rdoOutChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    InitializeUserControls("UserNameOut");

                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        DataTable TempTable = DataTableConverter.Convert(dgListOut.ItemsSource).Select("CTNR_ID = '" + Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[idx].DataItem, "CTNR_ID")) + "'").CopyToDataTable();

                        // row 색 바꾸기
                        dgListOut.SelectedIndex = idx;
                        // 대상목록 
                        Util.GridSetData(dgSelectOut, TempTable, FrameOperation, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 사용자 조회
        /// </summary>
        private void txtUserNameOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                popupUser(txtUserNameOut);
            }
        }
        //사용자 조회 버튼
        private void btnUserOut_Click(object sender, RoutedEventArgs e)
        {
            popupUser(txtUserNameOut);
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void txtCtnrIDOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetListOut(false);
            }
        }

        private void btnSearchOut_Click(object sender, RoutedEventArgs e)
        {
            GetListOut(true);
        }

        /// <summary>
        /// 출고 
        /// </summary>
        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationOut()) return;

            // 출고 하시겠습니까?
            Util.MessageConfirm("SFU3121", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveOut();
                }
            });

        }

        #endregion

        #region 출고 이력/취소

        /// <summary>
        /// 출고취소 선택
        /// </summary>
        private void rdoHistoryChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    InitializeUserControls("UserNameHistory");

                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        // row 색 바꾸기
                        dgListHistory.SelectedIndex = idx;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 사용자 조회
        /// </summary>
        private void txtUserNameHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                popupUser(txtUserNameHistory);
            }
        }
        //사용자 조회 버튼
        private void btnUserHistory_Click(object sender, RoutedEventArgs e)
        {
            popupUser(txtUserNameHistory);
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void txtCtnrIDHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetListHistory(false);
            }
        }

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetListHistory(true);
        }

        /// <summary>
        /// 출고취소
        /// </summary>
        private void btnOutCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel()) return;

            // 출고취소 하시겠습니까?
            Util.MessageConfirm("SFU3136", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveOutCancel();
                }
            });

        }

        #endregion

        #endregion

        #region Mehod

        /// <summary>
        /// 불량 등외품 출고 조회
        /// </summary>
        private void GetListOut(bool bButton)
        {
            try
            {
                InitializeUserControls("UserNameOut");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                if (string.IsNullOrWhiteSpace(txtCtnrIDOut.Text))
                {
                    newRow["AREAID"] = Util.GetCondition(cboAreaOut, "SFU4238");    // 동정보를 선택하세요
                    if (newRow["AREAID"].Equals("")) return;

                    newRow["PROCID"] = Util.GetCondition(cboProcessOut, "SFU1459"); // 공정을 선택하세요
                    if (newRow["PROCID"].Equals("")) return;

                    newRow["EQSGID"] = cboEquipmentSegmentOut.SelectedValue.ToString().Equals("") ? null : Util.NVC(cboEquipmentSegmentOut.SelectedValue);
                    newRow["PJT_NAME"] = string.IsNullOrWhiteSpace(txtPrjtNameOut.Text) ? null : txtPrjtNameOut.Text;
                    newRow["PRODID"] = string.IsNullOrWhiteSpace(txtProdidOut.Text) ? null : txtProdidOut.Text;
                    newRow["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotRTOut.Text) ? null : txtLotRTOut.Text;
                }
                else
                {
                    newRow["CTNR_ID"] = txtCtnrIDOut.Text;
                }

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_MOVE_OWMS_OFFGRD", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgListOut, bizResult, FrameOperation, false);

                        if (bizResult == null || bizResult.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU1905"); // 조회된 Data가 없습니다.

                            if (bButton == false)
                            {
                                txtCtnrIDOut.SelectAll();
                                txtCtnrIDOut.Focus();
                            }
                        }
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

        /// <summary>
        ///  출고 처리
        /// </summary>
        private void SaveOut()
        {
            try
            {
                ShowLoadingIndicator();

                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListOut, "CHK");

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("SM_FLAG", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));
                inCNTR.Columns.Add("CART_DEL_RSN_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "PROCID"));
                newRow["USERID"] = LoginInfo.USERID;
                newRow["ACT_USERID"] = Util.NVC(txtUserNameOut.Tag);
                newRow["WIPNOTE"] = txtNoteOut.Text;
                newRow["SM_FLAG"] = "Y";
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgListOut.Rows[rowIndex].DataItem, "CTNR_ID"));
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MOVE_OWMS_OFFGRD_MCP", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        GetListOut(true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 출고 이력/취소 조회
        /// </summary>
        private void GetListHistory(bool bButton)
        {
            try
            {
                InitializeUserControls("UserNameHistory");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                if (string.IsNullOrWhiteSpace(txtCtnrIDHistory.Text))
                {
                    newRow["FROM_DATE"] = Util.GetCondition(ldpDateFromHistory);
                    newRow["TO_DATE"] = Util.GetCondition(ldpDateToHistory);
                    newRow["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU4238"); // 동정보를 선택하세요
                    if (newRow["AREAID"].Equals("")) return;

                    newRow["PROCID"] = Util.GetCondition(cboProcessHistory, "SFU1459"); // 공정을 선택하세요
                    if (newRow["PROCID"].Equals("")) return;

                    newRow["EQSGID"] = cboEquipmentSegmentHistory.SelectedValue.ToString().Equals("") ? null : Util.NVC(cboEquipmentSegmentHistory.SelectedValue);
                    newRow["PJT_NAME"] = string.IsNullOrWhiteSpace(txtPrjtNameHistory.Text) ? null : txtPrjtNameHistory.Text;
                    newRow["PRODID"] = string.IsNullOrWhiteSpace(txtProdIDHistory.Text) ? null : txtProdIDHistory.Text;
                    newRow["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotRTHistory.Text) ? null : txtLotRTHistory.Text;
                }
                else
                {
                    newRow["CTNR_ID"] = txtCtnrIDHistory.Text;
                }

                // 출고상태 (ALL인경우 콤보의 출고상태값 전부를 넣어준다)
                if (Util.NVC(cboOutStatHistory.SelectedValue).Equals(""))
                {
                    string ActID = string.Empty;

                    for (int row = 1; row < cboOutStatHistory.Items.Count; row++)
                    {
                        ActID += ((DataRowView)cboOutStatHistory.Items[row]).Row.ItemArray[0].ToString() + ",";
                    }

                    newRow["ACTID"] = ActID.Substring(0, ActID.Length - 1);
                }
                else
                {
                    newRow["ACTID"] = Util.NVC(cboOutStatHistory.SelectedValue);
                }

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_CANCEL_MOVE_OWMS_OFFGRD", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgListHistory, bizResult, FrameOperation, false);

                        if (bizResult == null || bizResult.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU1905"); // 조회된 Data가 없습니다.

                            if (bButton == false)
                            {
                                txtCtnrIDHistory.SelectAll();
                                txtCtnrIDHistory.Focus();
                            }
                        }

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

        /// <summary>
        ///  출고취소 처리
        /// </summary>
        private void SaveOutCancel()
        {
            try
            {
                ShowLoadingIndicator();

                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgListHistory, "CHK");

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["ACT_USERID"] = Util.NVC(txtUserNameHistory.Tag);
                newRow["WIPNOTE"] = txtNoteHistory.Text;
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[rowIndex].DataItem, "CTNR_ID"));
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_MOVE_OWMS_OFFGRD_MCP", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        GetListHistory(true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Func]
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            #region 불량등외품 출고  

            //동
            C1ComboBox[] cboAreaChild = { cboProcessOut };
            _combo.SetCombo(cboAreaOut, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");
            cboAreaOut.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboProcessParent = { cboAreaOut };
            C1ComboBox[] cboScrapLine = { cboEquipmentSegmentOut };
            _combo.SetCombo(cboProcessOut, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbChild: cboScrapLine, cbParent: cboProcessParent);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaOut, cboProcessOut };
            _combo.SetCombo(cboEquipmentSegmentOut, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboEquipmentSegmentParent);
            cboEquipmentSegmentOut.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //사용자
            txtUserNameOut.Text = LoginInfo.USERNAME;
            txtUserNameOut.Tag = LoginInfo.USERID;

            #endregion

            #region 출고 이력/취소
            //동
            C1ComboBox[] cboAreaChildHistory = { cboProcessHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");
            cboAreaHistory.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboProcessParentHistory = { cboAreaHistory };
            C1ComboBox[] cboScrapLineHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbChild: cboScrapLineHistory, cbParent: cboProcessParentHistory);

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreaHistory, cboProcessHistory };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT", cbParent: cboEquipmentSegmentParentHistory);
            cboEquipmentSegmentHistory.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //출고상태
            SetComboOutStat(cboOutStatHistory);

            //사용자
            txtUserNameHistory.Text = LoginInfo.USERNAME;
            txtUserNameHistory.Tag = LoginInfo.USERID;

            #endregion
        }

        /// <summary>
        /// 출고 상태 콤보
        /// </summary>
        private void SetComboOutStat(C1ComboBox cbo)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

            DataRow row = dt.NewRow();

            row = dt.NewRow();
            row["CBO_CODE"] = "";
            row["CBO_NAME"] = "ALL";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["CBO_CODE"] = "MOVE_OWMS_OFFGRD";
            row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("출고");
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["CBO_CODE"] = "CANCEL_MOVE_OWMS_OFFGRD";
            row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("출고취소");
            dt.Rows.Add(row);

            cbo.ItemsSource = DataTableConverter.Convert(dt);
            cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// Control Clear
        /// </summary>
        private void InitializeUserControls(string ClearSelect)
        {
            switch (ClearSelect)
            {
                case "UserNameOut":
                    txtUserNameOut.Text = LoginInfo.USERNAME;
                    txtUserNameOut.Tag = LoginInfo.USERID;
                    txtNoteOut.Text = string.Empty;
                    Util.gridClear(dgSelectOut);
                    break;
                case "UserNameHistory":
                    txtUserNameHistory.Text = LoginInfo.USERNAME;
                    txtUserNameHistory.Tag = LoginInfo.USERID;
                    txtNoteHistory.Text = string.Empty;
                    break;
            }


        }

        /// <summary>
        /// 사용자 팝업
        /// </summary>
        private void popupUser(TextBox tx)
        {
            CMM_PERSON popupPerson = new CMM_PERSON();
            popupPerson.FrameOperation = FrameOperation;

            if (popupPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(popupPerson, Parameters);

                Parameters[0] = tx.Text;
                popupPerson.Closed += new EventHandler(popupPerson_Closed);

                grdMain.Children.Add(popupPerson);
                popupPerson.BringToFront();
            }
        }

        // 사용자 팝업닫기
        private void popupPerson_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popupPerson = sender as CMM_PERSON;
            if (popupPerson.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)ctbDefectNonRated.SelectedItem).Name.Equals("DefectNonRatedOut"))
                {
                    txtUserNameOut.Text = popupPerson.USERNAME;
                    txtUserNameOut.Tag = popupPerson.USERID;
                }
                else
                {
                    txtUserNameHistory.Text = popupPerson.USERNAME;
                    txtUserNameHistory.Tag = popupPerson.USERID;
                }
            }

            grdMain.Children.Remove(popupPerson);
        }

        private bool ValidationOut()
        {
            if (string.IsNullOrWhiteSpace(txtUserNameOut.Text) || txtUserNameOut.Tag == null)
            {
                Util.MessageValidation("SFU1592"); // 사용자 정보가 없습니다.
                return false;
            }

            if (dgSelectOut.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3017"); // 출고 대상이 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {
            if (dgListHistory.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgListHistory, "CHK");

            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[rowIndex].DataItem, "ACTID")) == "CANCEL_MOVE_OWMS_OFFGRD")
            {
                // [%1] 대차입니다.
                Util.MessageValidation("SFU4935", ObjectDic.Instance.GetObjectName("출고취소"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserNameHistory.Text) || txtUserNameHistory.Tag == null)
            {
                Util.MessageValidation("SFU1592"); // 사용자 정보가 없습니다.
                return false;
            }

            return true;
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

    }
}
