/*************************************************************************************
 Created Date : 2020.05.26
      Creator : 
   Decription : CELL 이동 처리 및 CMA 완성 LOT 이동 처리
CMA 재공 중 이동은 END로 처리 후 이동
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.23  최우석   CSR ID 61613, 라인 입고 후 Cell 동간 이동 처리 팔레트 조회 및 성능 개선 [요청번호] C20200518-000493
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
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
using System.Windows.Threading;
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_065 : UserControl, IWorkArea
    {
        /// <summary>최초 작업 플레그</summary>
        bool isFistWork = true;

        #region Declaration & Constructor 

        #region 리스트에서 포장해제를 위한 변수
        string unPack_EqsgID = string.Empty;       // 포장해제를 위한 eqsgid
        #endregion

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_065()
        {
            InitializeComponent();

            this.Loaded += PACK001_061_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //날자 초기값 세팅
            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7); //일주일 전 날짜
            dtpDateTo.SelectedDateTime = DateTime.Now;//오늘 날짜

            //2018.05.28
            Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
            Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");

            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, "0");
            Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, "0");

            tbLotInform.Text = "SCAN LOT" + ObjectDic.Instance.GetObjectName("정보");

            setGubunCbo();
            InitCombo();

        }

        private void PACK001_061_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();

                this.Loaded -= PACK001_061_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;

            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = null; // LoginInfo.CFG_EQSG_ID;           
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";

            //From, To 창고
            _combo.SetCombo(cboFromArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
            _combo.SetCombo(cboMoveToArea, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] { cboLocTo });
            _combo.SetCombo(cboLocTo, CommonCombo.ComboStatus.SELECT, cbParent: new C1ComboBox[] { cboMoveToArea, cboProcFrom, cboGubun }, sCase: "cboLocFrom");

            //동
            C1ComboBox[] cboAreaChild = { cboProductModel };
            _combo.SetCombo(cboFilterAreaFrom, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sCase: "AREA");
            _combo.SetCombo(cboFilterAreaTo, CommonCombo.ComboStatus.ALL, sCase: "AREA");

            //모델          
            C1ComboBox[] cboProductModelParent = { cboFilterAreaFrom, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL_AUTH");

            //제품  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboFilterAreaFrom, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

            cboFromArea.SelectedValue = LoginInfo.CFG_AREA_ID;
            cboFilterAreaFrom.SelectedValue = LoginInfo.CFG_AREA_ID;
            cboFilterAreaTo.SelectedIndex = 0;
        }

        private void setGubunCbo()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            dtResult.Rows.Add(new object[] { "-SELECT-", "SELECT" });
            dtResult.Rows.Add(new object[] { "Cell ID", "CELL" });
            dtResult.Rows.Add(new object[] { "CMA ID", "CMA" });

            cboGubun.ItemsSource = DataTableConverter.Convert(dtResult);
            cboGubun.SelectedIndex = 0;

            DataTable dtProcResult = new DataTable();
            dtProcResult.Columns.Add("KEY", typeof(string));
            dtProcResult.Columns.Add("VALUE", typeof(string));

            dtProcResult.Rows.Add(new object[] { "P1000", "P1000" });
            dtProcResult.Rows.Add(new object[] { "P5000", "P5000" });

            cboProcFrom.ItemsSource = DataTableConverter.Convert(dtProcResult);
            cboProcFrom.SelectedIndex = 0;
        }
        #endregion

        #region Grid Event -------------------------------------------------------------------------------------------------------------------------------------

        private void dgPallethistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgPallethistory.Rows.Count == 0 || dgPallethistory == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPallethistory.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;

                string selectPallet = DataTableConverter.GetValue(dgPallethistory.Rows[currentRow].DataItem, "BOXID").ToString();

                txtPalleyIdR.Text = selectPallet;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPalletBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //2018.05.28
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPallethistory.GetCellFromPoint(pnt);
                GridDoubleClickProcess(cell, "INFO_BOX");
                //C1.WPF.DataGrid.C1DataGrid grid = sender as C1.WPF.DataGrid.C1DataGrid;
                //gridDoubleClickProcess(sender, e, grid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GridDoubleClickProcess(C1.WPF.DataGrid.DataGridCell cell, string sPopUp_Flag)
        {
            try
            {
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {

                        if (cell.Column.Name == "BOXID")
                        {
                            PACK001_003_BOXINFO popup = new PACK001_003_BOXINFO();
                            popup.FrameOperation = this.FrameOperation;

                            if (popup != null)
                            {
                                DataTable dtData = new DataTable();
                                dtData.Columns.Add("BOXID", typeof(string));

                                DataRow newRow = null;
                                newRow = dtData.NewRow();
                                newRow["BOXID"] = cell.Text;

                                dtData.Rows.Add(newRow);

                                //========================================================================
                                object[] Parameters = new object[1];
                                Parameters[0] = dtData;
                                C1WindowExtension.SetParameters(popup, Parameters);
                                //========================================================================

                                popup.ShowModal();
                                popup.CenterOnScreen();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.05.28
        private void dgPallethistory_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "BOXID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion  --------------------------------------------------------------------------------------------------------------------------------------------

        #region Object Event -----------------------------------------------------------------------------------------------------------------------------------

        private void cboGubun_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboProcFrom.SelectedIndex = e.NewValue.ToString().Equals("CELL") ? 0 : 1;
            cboMoveToArea.SelectedIndex = 0;
            cboLocTo.SelectedIndex = 0;
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (txtSearchBox.Text.Length == 0)
                    {
                        return;
                    }

                    MovePalletSearch();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        //선택 취소
        private void btnSelectCacel_Click(object sender, RoutedEventArgs e)
        {
            int iRowCount = dgPalletBox.GetRowCount();

            if (iRowCount == 0) return;

            for (int i = iRowCount; 0 <= i; i--)
            {
                if (dgPalletBox.GetCell(i, 0).Value != null)
                {
                    bool isChecked = bool.Parse(dgPalletBox.GetCell(i, 0).Value.ToString());
                    if (isChecked)
                    {
                        dgPalletBox.RemoveRow(i);
                    }
                }
            }

            dgPalletBox.EndEdit();
            dgPalletBox.EndEditRow(true);

            txtCount.Text = dgPalletBox.GetRowCount().ToString();
            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dgPalletBox.GetRowCount()));
        }

        //전체 취소
        private void btncancel_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (dgPalletBox.GetRowCount() == 0) return;

                Util.MessageConfirm("SFU3406", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ReSetWork();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //이동 처리
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtPalletId.Text == "") return;

                if (dgPalletBox.GetRowCount() == 0) return;

                //이동 하시겠습니까?
                Util.MessageConfirm("SFU1763", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        BoxingEnd();
                        ReSetWork();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //작업초기화
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isFistWork)
                {
                    ReSetWork();
                    Util.MessageInfo("SFU3377");//작업이 초기화 됐습니다.
                }
                else
                {
                    //포장중인 작업이 있습니다. 정말 [작업초기화] 하시겠습니까?
                    Util.MessageConfirm("SFU3282", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ReSetWork();
                            Util.MessageInfo("SFU3377");//작업이 초기화 됐습니다.
                        }
                    });

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //LOT ID 입력
        private void txtBoxLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (cboGubun.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageInfo("SFU4149", (action) => { cboGubun.Focus(); }); //구분을 먼저선택하세요.
                        return;
                    }

                    if (cboMoveToArea.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageInfo("SFU3668", (action) => { cboMoveToArea.Focus(); }); //이동위치를 선택해 주세요.
                        return;
                    }

                    if (cboLocTo.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageInfo("SFU3668", (action) => { cboLocTo.Focus(); }); //이동위치를 선택해 주세요.
                        return;
                    }

                    if (!string.IsNullOrEmpty(txtBoxLotID.Text))
                    {
                        string sLotId = txtBoxLotID.Text;
                        string sBoxId = txtPalletId.Text;

                        if (txtPalletId.Text == "" && (bool)chkPalletId.IsChecked && dgPalletBox.Rows.Count == 0)
                        {
                            //투입 첫 LOT으로 이동 팔레트 ID 생성
                            CheckInputLotidMulti(string.Empty, new string[] { sLotId });
                        }
                        else
                        {
                            //투입 LOT 체크
                            CheckInputLotidMulti(sBoxId, new string[] { sLotId });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (action) => { txtBoxLotID.Focus(); });
            }
        }

        //LOT ID 입력 Ctrl+V 용
        private void txtBoxLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (cboGubun.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageInfo("SFU4149", (action) => { cboGubun.Focus(); }); //구분을 먼저선택하세요.
                        return;
                    }

                    if (cboMoveToArea.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageInfo("SFU3668", (action) => { cboMoveToArea.Focus(); }); //이동위치를 선택해 주세요.
                        return;
                    }

                    if (cboLocTo.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageInfo("SFU3668", (action) => { cboLocTo.Focus(); }); //이동위치를 선택해 주세요.
                        return;
                    }

                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n", "\n", "," };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Length > 500)
                    {
                        Util.MessageInfo("SFU8102", (action) => { txtBoxLotID.Focus(); }); //최대 500개 까지 가능합니다.
                        return;
                    }

                    DoEvents();

                    CheckInputLotidMulti(txtPalletId.Text, sPasteStrings);

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (action) => { txtBoxLotID.Focus(); });
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        //Search
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboFilterAreaFrom.SelectedIndex == 0)
                {
                    Util.MessageInfo("SFU1499", (action) => { cboFilterAreaTo.Focus(); }); //이동위치를 선택해 주세요.
                    return;
                }

                MovePalletSearch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPalletLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtPalleyIdR.Text.Length > 0)
                {
                    setTagReport();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion  --------------------------------------------------------------------------------------------------------------------------------------------

        #region User Method  -----------------------------------------------------------------------------------------------------------------------------------

        //작업 초기화
        private void ReSetWork()
        {
            isFistWork = true;

            dgPalletBox.ItemsSource = null;
            txtCount.Text = "0";
            txtPalletId.Text = "";
            txtBoxLotID.Text = "";
            txtBoxingProd.Text = "";
            txtSLocId.Text = "";

            cboFromArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            Util.gridClear(dgPalletBox);
            ObjectEnabled(true);
            Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(dgPalletBox.GetRowCount()));

        }

        //Object Enabled
        private void ObjectEnabled(bool isUsed)
        {
            cboGubun.IsEnabled = isUsed;
            cboMoveToArea.IsEnabled = isUsed;
            cboLocTo.IsEnabled = isUsed;
            //cboFromArea.IsEnabled = isUsed;
        }

        //전체 행 체크
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < dgPalletBox.Rows.Count; i++)
            {
                dgPalletBox.GetCell(i, 0).Value = "true";
            }

            dgPalletBox.EndEdit();
            dgPalletBox.EndEditRow(true);
        }

        //전체 행 체크 해제
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgPalletBox.Rows.Count; i++)
            {
                dgPalletBox.GetCell(i, 0).Value = "false";
            }

            dgPalletBox.EndEdit();
            dgPalletBox.EndEditRow(true);
        }


        //투입LOT 그리드 추가
        private bool GridLotBinding(DataTable dtData)
        {
            try
            {
                List<string> sLotIds = new List<string>();
                DataTable DTTMP = DataTableConverter.Convert(dgPalletBox.ItemsSource);

                if (!dtData.Columns.Contains("CHK"))
                {
                    dtData.Columns.Add("CHK");
                    dtData.Columns["CHK"].DefaultValue = "False";
                }

                DTTMP.Merge(dtData);

                for (int i = 0; i < DTTMP.Rows.Count; i++)
                {
                    sLotIds.Add(DTTMP.Rows[i]["LOTID"].ToString());
                }

                if (sLotIds.Distinct().Count() != sLotIds.Count())
                {
                    //중복 LOT 존재
                    Util.MessageInfo("101026", (action) => { txtBoxLotID.Focus(); });

                    return false;
                }


                Util.GridSetData(dgPalletBox, DTTMP, FrameOperation, true);

                dgPalletBox.EndEdit();
                dgPalletBox.EndEditRow(true);

                if (DTTMP.Rows.Count > 0)
                {
                    ObjectEnabled(false);
                    isFistWork = false;
                }
                else
                {
                    ObjectEnabled(true);
                    isFistWork = true;
                }

                txtCount.Text = DTTMP.Rows.Count.ToString();
                Util.SetTextBlockText_DataGridRowCount(tbBoxingWait_cnt, Util.NVC(DTTMP.Rows.Count));

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (action) => { txtBoxLotID.Focus(); });
                return false;
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void setTagReport()
        {
            PackCommon.ShowPalletTag(this.GetType().Name, txtPalleyIdR.Text.ToString(), unPack_EqsgID, string.Empty);
        }

        private void printPopUp_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.PACK001.Pallet_Tag printPopUp = sender as LGC.GMES.MES.PACK001.Pallet_Tag;
                if (Convert.ToBoolean(printPopUp.DialogResult))
                {

                }
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        #endregion  --------------------------------------------------------------------------------------------------------------------------------------------

        #region BIZ CALL  --------------------------------------------------------------------------------------------------------------------------------------

        //투입 Lot 체크 - BR_PRD_CHK_INPUTLOT_MOVEBOX
        private bool CheckInputLotidMulti(string sBoxId, string[] sLotId)
        {
            bool bRtn;
            try
            {

                DataSet dsInData = new DataSet();
                DataTable INDATA = dsInData.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("FROM_AREAID", typeof(string));
                INDATA.Columns.Add("FROM_SLOC_ID", typeof(string));
                INDATA.Columns.Add("TO_AREAID", typeof(string));
                INDATA.Columns.Add("TO_SLOC_ID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataTable IN_LOT = dsInData.Tables.Add("IN_LOT");
                IN_LOT.Columns.Add("LOTID", typeof(string));
                IN_LOT.Columns.Add("PRODID", typeof(string));
                IN_LOT.Columns.Add("PRDTYPE", typeof(string));

                INDATA.Rows.Add(new object[] { SRCTYPE.SRCTYPE_UI
                                             , LoginInfo.LANGID
                                             , sBoxId.Trim()
                                             , cboFromArea.SelectedValue.ToString()
                                             , txtSLocId.Text.Trim()
                                             , cboMoveToArea.SelectedValue.ToString()
                                             , cboLocTo.SelectedValue.ToString()
                                             , LoginInfo.USERID });

                for (int i = 0; i < sLotId.Length; i++)
                {
                    IN_LOT.Rows.Add(new object[] { sLotId[i].Trim()
                                             , txtBoxingProd.Text.Trim()
                                             , cboGubun.SelectedValue.ToString() });
                }


                DataSet dtResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUTLOT_MOVEBOX", "INDATA,IN_LOT", "OUTDATA", dsInData);

                if (dtResult != null && dtResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    bRtn = true;
                    if (isFistWork)
                    {
                        txtPalletId.Text = dtResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString().Trim();
                        txtBoxingProd.Text = dtResult.Tables["OUTDATA"].Rows[0]["PRODID"].ToString().Trim();
                        txtSLocId.Text = dtResult.Tables["OUTDATA"].Rows[0]["SLOC_ID"].ToString().Trim();
                    }
                    return GridLotBinding(dtResult.Tables["OUTDATA"]);

                }
                else
                {
                    bRtn = false;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bRtn;
        }

        //이동 - BR_PRD_REG_SEND_MOVE_FROM_AREA
        private void BoxingEnd()
        {
            try
            {
                DataSet dsInData = new DataSet();
                DataTable INDATA = dsInData.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PRODTYPE", typeof(string));
                INDATA.Columns.Add("FROM_AREAID", typeof(string));
                INDATA.Columns.Add("FROM_SLOC", typeof(string));
                INDATA.Columns.Add("TO_AREAID", typeof(string));
                INDATA.Columns.Add("TO_SLOC", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataTable IN_LOT = dsInData.Tables.Add("LOTDATA");
                IN_LOT.Columns.Add("LOTID", typeof(string));
                IN_LOT.Columns.Add("PROD_DATE", typeof(string));
                IN_LOT.Columns.Add("PRE_BOXID", typeof(string));
                IN_LOT.Columns.Add("PRE_RCV_ISS_ID", typeof(string));
                IN_LOT.Columns.Add("GBTID", typeof(string));
                IN_LOT.Columns.Add("CUST_LOTID", typeof(string));
                IN_LOT.Columns.Add("LGC_LOTID", typeof(string));
                IN_LOT.Columns.Add("ASSY_LOTID", typeof(string));
                IN_LOT.Columns.Add("INNER_BOXID", typeof(string));

                INDATA.Rows.Add(new object[] { SRCTYPE.SRCTYPE_UI
                                             , LoginInfo.LANGID
                                             , LoginInfo.CFG_SHOP_ID
                                             , txtPalletId.Text.Trim()
                                             , txtBoxingProd.Text.Trim()
                                             , cboGubun.SelectedValue.ToString()
                                             , cboFromArea.SelectedValue.ToString()
                                             , txtSLocId.Text.Trim()
                                             , cboMoveToArea.SelectedValue.ToString()
                                             , cboLocTo.SelectedValue.ToString()
                                             , null
                                             , LoginInfo.USERID });

                DataTable DTTMP = DataTableConverter.Convert(dgPalletBox.ItemsSource);

                for (int i = 0; i < DTTMP.Rows.Count; i++)
                {
                    IN_LOT.Rows.Add(new object[] { DTTMP.Rows[i]["LOTID"].ToString().Trim()
                                                 , DTTMP.Rows[i]["LOTDTTM_CR"].ToString().Trim()
                                                 , DTTMP.Rows[i]["PRE_BOXID"].ToString().Trim()
                                                 , DTTMP.Rows[i]["PRE_RCV_ISS_ID"].ToString().Trim()
                                                 , DTTMP.Rows[i]["GBTID"].ToString().Trim()
                                                 , DTTMP.Rows[i]["CUST_LOTID"].ToString().Trim()
                                                 , DTTMP.Rows[i]["LGC_LOTID"].ToString().Trim()
                                                 , DTTMP.Rows[i]["ASSY_LOTID"].ToString().Trim()
                                                 , DTTMP.Rows[i]["INNER_BOXID"].ToString().Trim() });
            }

                DataSet dtResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SEND_LOT_MOVE_AREA", "INDATA,LOTDATA", null, dsInData);

                if (dtResult != null)
                {

                    MovePalletSearch(txtPalletId.Text, true);

                    ReSetWork();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Search - 
        private void MovePalletSearch(string sBoxId = null, bool isMerge = false)
        {
            try
            {
                string sProdId = Util.GetCondition(cboProduct);
                string sFltAreaTo = Util.GetCondition(cboFilterAreaTo);
                DataTable RQSTDT = new DataTable("RQSTDT");

                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_FROM", typeof(DateTime));
                RQSTDT.Columns.Add("ISS_DTTM_TO", typeof(DateTime));
                RQSTDT.Columns.Add("FROM_AREA", typeof(string));
                RQSTDT.Columns.Add("TO_AREA", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = string.IsNullOrWhiteSpace(txtSearchBox.Text) ? (string.IsNullOrWhiteSpace(sBoxId) ? null : sBoxId) : txtSearchBox.Text.Trim();
                dr["PRODID"] = string.IsNullOrWhiteSpace(sProdId) ? null : sProdId.Trim();
                dr["ISS_DTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
                dr["ISS_DTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();
                dr["FROM_AREA"] = Util.GetCondition(cboFilterAreaFrom);
                dr["TO_AREA"] = string.IsNullOrWhiteSpace(sFltAreaTo) ? null : sFltAreaTo.Trim();


                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_MOVE_BOX", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    txtPalleyIdR.Text = "";

                    if (isMerge)
                    {
                        DataTable DTTMP = DataTableConverter.Convert(dgPallethistory.ItemsSource);
                        dtResult.Merge(DTTMP);
                        Util.GridSetData(dgPallethistory, dtResult, FrameOperation);
                    }
                    else
                    {
                        dgPallethistory.ItemsSource = null;
                        Util.GridSetData(dgPallethistory, dtResult, FrameOperation);
                        txtSearchBox.Text = "";
                    }
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion  --------------------------------------------------------------------------------------------------------------------------------------------

    }
}
