

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_092 : UserControl, IWorkArea
    {
        private struct MOVE_TYPE
        {
            public const string DOWN = "DOWN";
            public const string UP = "UP";
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_092()
        {
            InitializeComponent();
            Initialize();
            initCombo();
        }

        #region [Initial ]
        private void Initialize()
        {

        }

        private void initCombo()
        {

            CommonCombo _combo = new CommonCombo();

            //PLANT 콤보
            string[] strCboPlantFilter = { Area_Type.PACK };
            C1ComboBox[] cboShopChild = { cboArea };
            _combo.SetCombo(cboPlant, CommonCombo.ComboStatus.NONE, cbChild: cboShopChild, sFilter: strCboPlantFilter, sCase: "SHOP_AUTH");

            //AREA 콤보
            string[] strCboAreaFilter = { Area_Type.PACK };
            C1ComboBox[] cboAreaParent = { cboPlant };
            C1ComboBox[] cboAreaChild = { cboEqsg };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, cbParent: cboAreaParent, sFilter: strCboAreaFilter, sCase: "AREA_AREATYPE");

            //라인            
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboPrjt };
            _combo.SetCombo(cboEqsg, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild, sCase: "cboLine");

            //모델
            C1ComboBox[] cboProductModelParent = { cboArea, cboEqsg };
            C1ComboBox[] cboProductModelChild = { cboProd };
            _combo.SetCombo(cboPrjt, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL");

            //제품
            C1ComboBox[] cboProductParent = { cboPlant, cboArea, cboEqsg, cboPrjt };
            _combo.SetCombo(cboProd, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT_PACK");


            //2 Tab
            //PLANT 콤보
            string[] strCboTab2PlantFilter = { Area_Type.PACK };
            C1ComboBox[] cboTab2ShopChild = { cboTab2Area };
            _combo.SetCombo(cboTab2Plant, CommonCombo.ComboStatus.NONE, cbChild: cboShopChild, sFilter: strCboPlantFilter, sCase: "SHOP_AUTH");

            //AREA 콤보
            string[] strCboTab2AreaFilter = { Area_Type.PACK };
            C1ComboBox[] cboTab2AreaParent = { cboTab2Plant };
            C1ComboBox[] cboTab2AreaChild = { cboTab2Eqsg };
            _combo.SetCombo(cboTab2Area, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, cbParent: cboAreaParent, sFilter: strCboAreaFilter, sCase: "AREA_AREATYPE");

            //라인            
            C1ComboBox[] cboTab2EquipmentSegmentParent = { cboTab2Area };
            C1ComboBox[] cboTab2EquipmentSegmentChild = { cboTab2Prjt };
            _combo.SetCombo(cboTab2Eqsg, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild, sCase: "cboLine");

            //모델
            C1ComboBox[] cboTab2ProductModelParent = { cboTab2Area, cboTab2Eqsg };
            C1ComboBox[] cboTab2ProductModelChild = { cboTab2Prod };
            _combo.SetCombo(cboTab2Prjt, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL");

            //제품
            C1ComboBox[] cboTab2ProductParent = { cboPlant, cboTab2Area, cboTab2Eqsg, cboTab2Prjt };
            _combo.SetCombo(cboTab2Prod, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT_PACK");

        }


        #endregion

        #region [ XML 이벤트 ] 

        #region [ 1번째 TAB ] 

        #region [ 조회 ]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchLotList();
        }
        #endregion

        #region [ 승인자 Grid ]
        private void btnGrator_Click(object sender, RoutedEventArgs e)
        {
            GetGratorUserWindow();
        }

        private void txtGrator_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetGratorUserWindow();
            }
        }
        private void dgGratorRemover_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

            try
            {
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                OrderSort(dg, "APPR_SEQS");

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ 참조자 Grid ]
        private void btnReferrer_Click(object sender, RoutedEventArgs e)
        {
            GetReferrerUserWindow();
        }

        private void txtReferrer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetReferrerUserWindow();
            }
        }

        private void dgReferrerRemover_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

            try
            {
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                OrderSort(dg, "APPR_SEQS");

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ 참조자 그리드 활성화 ] 
        private void btnRight_Checked(object sender, RoutedEventArgs e)
        {
            if (MainGrid != null)
            {
                MainGrid.ColumnDefinitions[2].Width = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                MainGrid.ColumnDefinitions[2].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
        }

        private void btnRight_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MainGrid != null)
            {
                MainGrid.ColumnDefinitions[2].Width = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                MainGrid.ColumnDefinitions[2].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
        }
        #endregion

        #region [ 팝업 이벤트 ]

        #region [ 승인자 팝업 ]
        private void GetGratorUserWindow()
        {
            //HOLD_CONFIRM_PERSON wndPerson = new HOLD_CONFIRM_PERSON();
            //wndPerson.FrameOperation = this.FrameOperation;

            //if (wndPerson != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = txtGrator.Text;
            //    C1WindowExtension.SetParameters(wndPerson, Parameters);

            //    wndPerson.Closed += new EventHandler(wndGratorUser_Closed);
            //    wndPerson.ShowModal();
            //    wndPerson.CenterOnScreen();
            //    wndPerson.BringToFront();
            //}
        }

        private void wndGratorUser_Closed(object sender, EventArgs e)
        {
            //HOLD_CONFIRM_PERSON wndPerson = sender as HOLD_CONFIRM_PERSON;
            //if (wndPerson.DialogResult == MessageBoxResult.OK)
            //{
            //    DataTable dtTo = null;
            //    dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

            //    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            //    {
            //        dtTo.Columns.Add("APPR_SEQS", typeof(string));
            //        dtTo.Columns.Add("USERID", typeof(string));
            //        dtTo.Columns.Add("USERNAME", typeof(string));
            //        dtTo.Columns.Add("DEPTNAME", typeof(string));
            //    }

            //    if (dtTo.Select("USERID = '" + wndPerson.USERID + "'").Length > 0) //중복조건 체크
            //    {
            //        Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
            //        return;
            //    }

            //    DataRow drFrom = dtTo.NewRow();
            //    drFrom["APPR_SEQS"] = dtTo.Rows.Count + 1;
            //    drFrom["USERID"] = wndPerson.USERID;
            //    drFrom["USERNAME"] = wndPerson.USERNAME;
            //    drFrom["DEPTNAME"] = wndPerson.DEPTNAME;
            //    dtTo.Rows.Add(drFrom);

            //    dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

            //    txtGrator.Text = string.Empty;
            //    txtGrator.Tag = string.Empty;
            //}
        }
        #endregion

        #region [ 참조자 팝업 ]
        private void GetReferrerUserWindow()
        {
            //PERSON_SEARCH wndPerson = new PERSON_SEARCH();
            //wndPerson.FrameOperation = this.FrameOperation;

            //if (wndPerson != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = txtReferrer.Text;
            //    C1WindowExtension.SetParameters(wndPerson, Parameters);

            //    wndPerson.Closed += new EventHandler(wndReferrerUser_Closed);
            //    wndPerson.ShowModal();
            //    wndPerson.CenterOnScreen();
            //    wndPerson.BringToFront();
            //}
        }
        private void wndReferrerUser_Closed(object sender, EventArgs e)
        {
            //PERSON_SEARCH wndPerson = sender as PERSON_SEARCH;
            //if (wndPerson.DialogResult == MessageBoxResult.OK)
            //{
            //    DataTable dtTo = null;
            //    dtTo = DataTableConverter.Convert(dgReferrer.ItemsSource);

            //    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            //    {
            //        dtTo.Columns.Add("APPR_SEQS", typeof(string));
            //        dtTo.Columns.Add("USERID", typeof(string));
            //        dtTo.Columns.Add("USERNAME", typeof(string));
            //        dtTo.Columns.Add("DEPTNAME", typeof(string));
            //    }

            //    if (dtTo.Select("USERID = '" + wndPerson.USERID + "'").Length > 0) //중복조건 체크
            //    {
            //        Util.Alert("SFU1780");  //이미 추가 된 참조자 입니다.
            //        return;
            //    }

            //    DataRow drFrom = dtTo.NewRow();
            //    drFrom["APPR_SEQS"] = dtTo.Rows.Count + 1;
            //    drFrom["USERID"] = wndPerson.USERID;
            //    drFrom["USERNAME"] = wndPerson.USERNAME;
            //    drFrom["DEPTNAME"] = wndPerson.DEPTNAME;
            //    dtTo.Rows.Add(drFrom);

            //    dgReferrer.ItemsSource = DataTableConverter.Convert(dtTo);

            //    txtReferrer.Text = string.Empty;
            //    txtReferrer.Tag = string.Empty;
            //}
        }
        #endregion

        #endregion

        #region [ 그리드 ALL 체크 박스 이벤트 

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.DataGridCheckAllChecked(dgLotList);

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.DataGridCheckAllUnChecked(dgLotList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkBottomHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    Util.DataGridCheckAllChecked(dgBottomList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkBottomHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    Util.DataGridCheckAllUnChecked(dgBottomList);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [ 화살표 이벤트 ]
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            LossLotMove(MOVE_TYPE.DOWN);
        }

        private void btnUpLoss_Click(object sender, RoutedEventArgs e)
        {
            LossLotMove(MOVE_TYPE.UP);
        }
        #endregion

        #region [ txtLotid KeyDown 이벤트 ] 
        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtLotId.Text.ToString()))
                    {
                        Util.MessageInfo("SFU1813", (result) => {
                            if (result == MessageBoxResult.OK)
                            {
                                txtLotId.Focus();
                            }
                        });
                    }

                    SearchLot(txtLotId.Text.ToString());
                }

                if(e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    string strLotid = string.Empty;

                    foreach (string strTempLotId in sPasteStrings)
                    {
                        if (string.IsNullOrWhiteSpace(strLotid))
                        {
                            strLotid = strTempLotId;
                        }
                        else
                        {
                            strLotid = strLotid + "," + strTempLotId;
                        }
                    }

                    SearchLot(strLotid);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ 승인 요청 Click 이벤트 ]
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU2044", (result) => {

                    if (result == MessageBoxResult.OK)
                    {
                        if (txtWipNote.Text == null || string.IsNullOrWhiteSpace(txtWipNote.Text.ToString()))
                        {
                            Util.MessageInfo("SFU1590", (error_result) => {
                                if (error_result == MessageBoxResult.OK)
                                {
                                    txtWipNote.Focus();
                                }
                            });
                            return;
                        }

                        ComfirmInterlcok();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void dgLotList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgLotList.CurrentRow == null || dgLotList.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = dgLotList.CurrentColumn.Name;
                string sChkValue = string.Empty;

                if (!sColName.Contains("CHK"))
                {
                    return;
                }

                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgLotList.CurrentRow.Index;
                    int indexColumn = dgLotList.CurrentColumn.Index;

                    sChkValue = DataTableConverter.GetValue(dgLotList.Rows[indexRow].DataItem, sColName).ToString();

                    if (sChkValue == "0" || sChkValue == "False")
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[indexRow].DataItem, sColName, true);
                    }
                    else if (sChkValue == "1" || sChkValue == "True")
                    {
                        DataTableConverter.SetValue(dgLotList.Rows[indexRow].DataItem, sColName, false);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgBottomList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgBottomList.CurrentRow == null || dgBottomList.SelectedIndex == -1)
                {
                    return;
                }

                string sColName = dgBottomList.CurrentColumn.Name;
                string sChkValue = string.Empty;

                if (!sColName.Contains("CHK"))
                {
                    return;
                }

                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgBottomList.CurrentRow.Index;
                    int indexColumn = dgBottomList.CurrentColumn.Index;

                    sChkValue = DataTableConverter.GetValue(dgBottomList.Rows[indexRow].DataItem, sColName).ToString();

                    if (sChkValue == "0" || sChkValue == "False")
                    {
                        DataTableConverter.SetValue(dgBottomList.Rows[indexRow].DataItem, sColName, true);
                    }
                    else if (sChkValue == "1" || sChkValue == "True")
                    {
                        DataTableConverter.SetValue(dgBottomList.Rows[indexRow].DataItem, sColName, false);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region [ CS 이벤트 ]

        #region [ 차수 정렬 ]
        private void OrderSort(C1.WPF.DataGrid.C1DataGrid dg, string strFiliter)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][strFiliter] = (i + 1);
                }

                Util.gridClear(dg);

                dg.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [ Method ]

        private void LossLotMove(string sMoveArrow)
        {

            if (sMoveArrow == MOVE_TYPE.DOWN)
            {
                if (dgLotList.ItemsSource == null) return;
                if (dgLotList.GetRowCount() == 0) return;
            }
            else
            {
                if (dgBottomList.ItemsSource == null) return;
                if (dgBottomList.GetRowCount() == 0) return;
            }

            DataTable dtTarget = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dgBottomList.ItemsSource : dgLotList.ItemsSource);
            DataTable dtSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dgLotList.ItemsSource : dgBottomList.ItemsSource);
            DataRow newRow = null;

            if (dtTarget.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTarget.Columns.Add("CHK", typeof(Boolean));
                dtTarget.Columns.Add("LOTID", typeof(string));
                dtTarget.Columns.Add("AREAID", typeof(string));
                dtTarget.Columns.Add("AREANAME", typeof(string));
                dtTarget.Columns.Add("EQSGNAME", typeof(string));
                dtTarget.Columns.Add("EQSGID", typeof(string));
                dtTarget.Columns.Add("PRODID", typeof(string));
                dtTarget.Columns.Add("LOTSTAT", typeof(string));
                dtTarget.Columns.Add("PROCNAME", typeof(string));
                dtTarget.Columns.Add("PROCID", typeof(string));
                dtTarget.Columns.Add("WIPSNAME", typeof(string));
                dtTarget.Columns.Add("WIPSTAT", typeof(string));
                dtTarget.Columns.Add("EQPTNAME", typeof(string));
                dtTarget.Columns.Add("EQPTID", typeof(string));
                dtTarget.Columns.Add("BOXID", typeof(string));
                dtTarget.Columns.Add("WIPHOLD", typeof(string));
                dtTarget.Columns.Add("WOID", typeof(string));

            }

            for (int i = dtSource.Rows.Count; i > 0; i--)
            {
                if (string.Equals(dtSource.Rows[i - 1]["CHK"].ToString(), "True") || string.Equals(dtSource.Rows[i - 1]["CHK"].ToString(), "1"))
                {
                    

                    newRow = dtTarget.NewRow();
                    newRow["CHK"] = false;
                    newRow["LOTID"] = dtSource.Rows[i - 1]["LOTID"].ToString();
                    newRow["AREAID"] = dtSource.Rows[i - 1]["AREAID"].ToString();
                    newRow["AREANAME"] = dtSource.Rows[i - 1]["AREANAME"].ToString();
                    newRow["EQSGNAME"] = dtSource.Rows[i - 1]["EQSGNAME"].ToString();
                    newRow["EQSGID"] = dtSource.Rows[i - 1]["EQSGID"].ToString();
                    newRow["PRODID"] = dtSource.Rows[i - 1]["PRODID"].ToString();
                    newRow["LOTSTAT"] = dtSource.Rows[i - 1]["LOTSTAT"].ToString();
                    newRow["PROCNAME"] = dtSource.Rows[i - 1]["PROCNAME"].ToString();
                    newRow["PROCID"] = dtSource.Rows[i - 1]["PROCID"].ToString();
                    newRow["WIPSNAME"] = dtSource.Rows[i - 1]["WIPSNAME"].ToString();
                    newRow["WIPSTAT"] = dtSource.Rows[i - 1]["WIPSTAT"].ToString();
                    newRow["EQPTNAME"] = dtSource.Rows[i - 1]["EQPTNAME"].ToString();
                    newRow["EQPTID"] = dtSource.Rows[i - 1]["EQPTID"].ToString();
                    newRow["BOXID"] = dtSource.Rows[i - 1]["BOXID"].ToString();
                    newRow["WIPHOLD"] = dtSource.Rows[i - 1]["WIPHOLD"].ToString();
                    newRow["WOID"] = dtSource.Rows[i - 1]["WOID"].ToString();

                    if (dtTarget.Select("LOTID = '" + dtSource.Rows[i - 1]["LOTID"].ToString() + "'").Length > 0)
                    {
                        Util.MessageInfo("SFU1384");
                        return;
                    }

                    dtTarget.Rows.Add(newRow);

                    dtSource.Rows[i - 1].Delete();
                }
            }

            dgBottomList.ItemsSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dtTarget : dtSource);
            dgLotList.ItemsSource = DataTableConverter.Convert((sMoveArrow == MOVE_TYPE.DOWN) ? dtSource : dtTarget);

            //chkAll_HOLD.IsChecked = false;
            //chkAll_REQ.IsChecked = false;

        }

        #region [ Biz Caller ]

        private void SearchLot(string strLotId = "")
        {
            DataTable dtReTurn = new DataTable();

            try
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("ISS_RSV_FLAG", typeof(string));

                DataRow drInData = inData.NewRow();
                drInData["LANGID"] = LoginInfo.LANGID;
                drInData["ISS_RSV_FLAG"] = "Y";
                inData.Rows.Add(drInData);

                DataTable inLotId = indataSet.Tables.Add("INLOT");
                inLotId.Columns.Add("LOTID", typeof(string));
                DataRow drInLotId = inLotId.NewRow();
                drInLotId["LOTID"] = strLotId;
                inLotId.Rows.Add(drInLotId);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_SHIP_INTERLOCK", "INDATA,INLOT", "OUTDATA,OUT_ERROR", indataSet);

                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    Util.GridSetData(dgLotList, dsRslt.Tables["OUTDATA"], FrameOperation);
                    txtLotId.Text = "";
                    Clipboard.Clear();
                }
                else
                {
                    Util.MessageInfo("SFU3537");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchLotList()
        {
            DataTable dtReTurn = new DataTable();

            try
            {
                if (!string.IsNullOrWhiteSpace(txtLotId.Text.ToString()))
                {
                    SearchLot(txtLotId.Text.ToString());
                    return;
                }

                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));               
                inData.Columns.Add("ISS_RSV_FLAG", typeof(string));
                inData.Columns.Add("TODATE", typeof(string));
                inData.Columns.Add("FROMDATE", typeof(string));

                DataRow drInData = inData.NewRow();
                drInData["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrWhiteSpace(cboPlant.SelectedValue.ToString()))
                {
                    inData.Columns.Add("SHOPID", typeof(string));
                    drInData["SHOPID"] = cboPlant.SelectedValue.ToString();
                }

                if (!string.IsNullOrWhiteSpace(cboArea.SelectedValue.ToString()))
                {
                    inData.Columns.Add("AREAID", typeof(string));
                    drInData["AREAID"] = cboArea.SelectedValue.ToString();
                }

                if (!string.IsNullOrWhiteSpace(cboEqsg.SelectedValue.ToString()))
                {
                    inData.Columns.Add("EQSGID", typeof(string));
                    drInData["EQSGID"] = cboEqsg.SelectedValue.ToString();
                }

                if (!string.IsNullOrWhiteSpace(cboPrjt.SelectedValue.ToString()))
                {
                    inData.Columns.Add("PRJT", typeof(string));
                    drInData["PRJT"] = cboPrjt.SelectedValue.ToString();
                }

                if (!string.IsNullOrWhiteSpace(cboProd.SelectedValue.ToString()))
                {
                    inData.Columns.Add("PRODID", typeof(string));
                    drInData["PRODID"] = cboProd.SelectedValue.ToString();
                }

                drInData["ISS_RSV_FLAG"] = "N";
                drInData["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss"); // 2024.11.12. 김영국 - 날짜형식 지정요청. (윤주일 책임)
                drInData["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss"); // 2024.11.12. 김영국 - 날짜형식 지정요청. (윤주일 책임)
                inData.Rows.Add(drInData);


                //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_SHIP_INTERLOCK", "INDATA,INLOT", "OUTDATA,OUT_ERROR", indataSet);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPLOT_PACK_LOT_INDEX", "RQSTDT", "RSLTDT", inData);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgLotList, dtRslt, FrameOperation);
                    txtLotId.Text = "";
                }
                else
                {
                    Util.MessageInfo("SFU3537");
                    Util.gridClear(dgLotList);
                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ComfirmInterlcok()
        {
            try
            {
                if(dgBottomList == null || dgBottomList.GetRowCount() == 0)
                {
                    Util.MessageInfo("SFU2052");
                    return;
                }

                DataTable dtInLot = new DataTable();

                dtInLot = DataTableConverter.Convert(dgBottomList.ItemsSource);


                DataSet indataSet = new DataSet();

                DataTable inLotId = indataSet.Tables.Add("INLOT");
                inLotId.Columns.Add("LOTID", typeof(string));

                foreach (DataRow dr in dtInLot.Rows)
                {
                    DataRow drInLotId = inLotId.NewRow();
                    drInLotId["LOTID"] = dr["LOTID"].ToString();
                    inLotId.Rows.Add(drInLotId);
                }

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("ISS_RSV_FLAG", typeof(string));
                inData.Columns.Add("WIPNOTE", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataRow drInData = inData.NewRow();
                drInData["ISS_RSV_FLAG"] = "Y";
                drInData["WIPNOTE"] = txtWipNote.Text.ToString();
                drInData["USERID"] = LoginInfo.USERID;
                inData.Rows.Add(drInData);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_INTERLOCK", "INLOT,INDATA", "", indataSet);

                Util.MessageInfo("SFU1275");
                Util.gridClear(dgBottomList);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }




        #endregion

        #endregion

        #endregion

        private void txtTab2LotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtTab2LotId.Text.ToString()))
                {
                    Util.MessageInfo("SFU1813", (result) => {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTab2LotId.Focus();
                        }
                    });
                }

                SearchLotHist(txtTab2LotId.Text.ToString());
            }

            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                string[] stringSeparators = new string[] { "\r\n" };
                string sPasteString = Clipboard.GetText();
                string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                string strLotid = string.Empty;

                foreach (string strTempLotId in sPasteStrings)
                {
                    if (string.IsNullOrWhiteSpace(strLotid))
                    {
                        strLotid = strTempLotId;
                    }
                    else
                    {
                        strLotid = strLotid + "," + strTempLotId;
                    }
                }

                SearchLotHist(strLotid);
            }
        }


        private void SearchLotHist(string strLotId = "")
        {
            DataTable dtReTurn = new DataTable();

            try
            {

                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));

                DataRow drInData = inData.NewRow();
                drInData["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(strLotId))
                {
                    inData.Columns.Add("LOTID", typeof(string));
                    drInData["LOTID"] = strLotId;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(cboTab2Plant.SelectedValue.ToString()))
                    {
                        inData.Columns.Add("SHOPID", typeof(string));
                        drInData["SHOPID"] = cboTab2Plant.SelectedValue.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(cboTab2Area.SelectedValue.ToString()))
                    {
                        inData.Columns.Add("AREAID", typeof(string));
                        drInData["AREAID"] = cboTab2Area.SelectedValue.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(cboTab2Eqsg.SelectedValue.ToString()))
                    {
                        inData.Columns.Add("EQSGID", typeof(string));
                        drInData["EQSGID"] = cboTab2Eqsg.SelectedValue.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(cboTab2Prjt.SelectedValue.ToString()))
                    {
                        inData.Columns.Add("PRJT", typeof(string));
                        drInData["PRJT"] = cboTab2Prjt.SelectedValue.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(cboTab2Prod.SelectedValue.ToString()))
                    {
                        inData.Columns.Add("PRODID", typeof(string));
                        drInData["PRODID"] = cboTab2Prod.SelectedValue.ToString();
                    }

                    inData.Columns.Add("TODATE", typeof(string));
                    inData.Columns.Add("FROMDATE", typeof(string));
                    drInData["TODATE"] = dtpTab2DateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss"); // 2024.11.22. 김영국 - DateTime 형식 지정.
                    drInData["FROMDATE"] = dtpTab2DateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss"); ; // 2024.11.22. 김영국 - DateTime 형식 지정.

                }

                if ((bool)chkRscIssFlag.IsChecked)
                {
                    inData.Columns.Add("ISS_RSV_FLAG", typeof(string));
                    drInData["ISS_RSV_FLAG"] = "Y";
                }


                inData.Rows.Add(drInData);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPLOT_PACK_SHIP_INTERLOCK", "RQSTDT", "RSLTDT", inData);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgTab2LotList, dtRslt, FrameOperation);
                    txtTab2LotId.Text = "";
                    Clipboard.Clear();
                }
                else
                {
                    Util.MessageInfo("SFU3537");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTab2Search_Click(object sender, RoutedEventArgs e)
        {
            SearchLotHist();
        }
    }
}
