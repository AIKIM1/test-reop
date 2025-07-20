/*************************************************************************************
 Created Date : 2018.03.01
      Creator : 오화백
   Decription : 파우치 활성화 불량창고 관리 - 불량창고 대차출고
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_217_DEFECT_OUTPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private bool _load = true;
        private bool _tagPrint = false; //태그여부

        private string _PRINT_CTNR_ID = string.Empty;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_217_DEFECT_OUTPUT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                object[] parameters = C1WindowExtension.GetParameters(this);
                DataTable DefecList = parameters[0] as DataTable;
                if (DefecList == null)
                    return;
                ValidateOpen(DefecList);

                SetGridDefectList(DefecList);
                SetGridDefectCtnr(DefecList);
                txtUserNameCr.Text = LoginInfo.USERNAME;
                txtUserNameCr.Tag = LoginInfo.USERID;

                txtMoveNameCr.Text = LoginInfo.USERNAME;
                txtMoveNameCr.Tag = LoginInfo.USERID;
                SetCombo();
                _load = false;
            }

        }
        private void SetCombo()
        {
            SetAreaCombo();
            SetMoveProcessCombo();
            SetMoveLineCombo();

        }

        #endregion

        #region [공장 변경]
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                return;

            SetMoveProcessCombo();

        }
        #endregion

        #region [공정 변경]
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
                return;

            SetMoveLineCombo();

        }
        #endregion

        #region [공장이동 체크]
        private void chkMoveArea_Checked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = true;
            txtMoveNameCr.IsEnabled = true;
            btnMoveCr.IsEnabled = true;
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }

        private void chkMoveArea_Unchecked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = false;
            txtMoveNameCr.IsEnabled = false;
            btnMoveCr.IsEnabled = false;

        }
        #endregion

        #region 작업자 
        //작업자 버튼 클릭
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        //작업자 
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        #endregion


        #region 인계자
        private void txtMoveNameCr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow_Move();
            }
        }

        private void btnMoveCr_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_Move();
        }
        #endregion
        #region [SPREAD]
        //출고대상 불량 스프레드 이벤트
        private void dgDefectList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("OUTPUTQTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    if (e.Cell.Column.Name.Equals("OUTPUT_AFTER_QTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.WhiteSmoke);
                    }
                }
            }));
        }
        //대차 스프레드
        private void dgDefectCtnr_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (e.Cell.Column.Name.Equals("WIP_QLTY_TYPE_NAME"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }));
        }
        private void dgDefectList_KeyDown(object sender, KeyEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid grd = (sender as C1.WPF.DataGrid.C1DataGrid);
            int index = 0;
            if (e.Key == Key.Enter)
            {

                if (btnPrint.IsEnabled == true) return;
                if (grd != null &&
                        grd.CurrentCell != null &&
                        grd.CurrentCell.Column != null && grd.CurrentCell.Column.Name.Equals("OUTPUTQTY"))
                {
                    //INBOX(LOT) 셋팅
                    DataRow[] drDefectList = DataTableConverter.Convert(dgDefectList.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "LOTID")) + "'");
                    //index 설정해놓음..
                    index = grd.CurrentCell.Row.Index;
                    if (Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "WH_DFEC_QTY")).Replace(",", "")) < Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "OUTPUTQTY")) == string.Empty ? "0" : Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "OUTPUTQTY"))))
                    {
                        //Util.MessageValidation("출고수량이 현재수량이 큽니다"); 
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4592"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SetGridInboxLot(drDefectList, "D");
                                DataTableConverter.SetValue(dgDefectList.Rows[index].DataItem, "OUTPUT_AFTER_QTY", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[index].DataItem, "WH_DFEC_QTY")).Replace(",", "")));
                                DataTableConverter.SetValue(dgDefectList.Rows[index].DataItem, "OUTPUTQTY", 0);
                            }
                        });
                        return;
                    }

                    //출고후 수량 계산
                    Decimal OUTPUT_AFTER_QTY = 0;
                    OUTPUT_AFTER_QTY = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "WH_DFEC_QTY")).Replace(",", "")) - Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "OUTPUTQTY")) == string.Empty ? "0" : Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "OUTPUTQTY")));
                    DataTableConverter.SetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "OUTPUT_AFTER_QTY", OUTPUT_AFTER_QTY);

                  
                    // 출고수량이 공백이거나 0 이면 INBOX(LOT)스프레드에서 제외
                    if (Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "OUTPUTQTY")) == string.Empty ? "0" : Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[grd.CurrentCell.Row.Index].DataItem, "OUTPUTQTY"))) == 0)
                    {
                        SetGridInboxLot(drDefectList, "D");
                    }
                    else
                    {
                        SetGridInboxLot(drDefectList, "A");
                    }


                }
            }
        }

        #endregion



        #region [출고]
        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationOutput())
                return;

            // 출고 하시겠습니까?
            Util.MessageConfirm("SFU3121", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DefectOutput();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (btnPrint.IsEnabled == false)
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            else
            {
                if (_tagPrint == false)
                {
                    // 대차 Sheet를 발행하세요
                    Util.MessageValidation("SFU4593");
                    return;
                }
                else
                {
                    this.DialogResult = MessageBoxResult.OK;
                }
            }
        }
        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (btnPrint.IsEnabled == true)
            {
                if (_tagPrint == false)
                {
                    // 대차 Sheet를 발행하세요
                    Util.MessageValidation("SFU4593");
                    e.Cancel = true;
                }

            }
        }
        #endregion

        #region [대차 Sheet발행]
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint()) return;

            DataTable dt = PrintCartData();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return;
            }
            // Page수 산출
            int PageCount = dt.Rows.Count % 40 != 0 ? (dt.Rows.Count / 40) + 1 : dt.Rows.Count / 40;
            int start = 0;
            int end = 0;
            DataRow[] dr;

            // Page 수만큼 Pallet List를 채운다
            for (int cnt = 0; cnt < PageCount; cnt++)
            {
                start = (cnt * 40) + 1;
                end = ((cnt + 1) * 40);

                dr = dt.Select("ROWNUM >=" + start + "And ROWNUM <=" + end);
                CartRePrint(dr, cnt + 1);
            }
        }
        //대차시트 팝업
        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {


            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;
            popupCartPrint.DefectGroupLotYN = "Y";
            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;


            object[] parameters = new object[5];
            parameters[0] = LoginInfo.CFG_PROC_ID;
            parameters[1] = string.Empty;
            parameters[2] = _PRINT_CTNR_ID;
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartPrint);
                    popupCartPrint.BringToFront();
                    break;
                }
            }
        }
        //대차시트 팝업 닫기
        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _tagPrint = true;
                chkMoveArea.IsEnabled = true;
                cboProcess.IsEnabled = true;
                //txtMoveNameCr.IsEnabled = true;
                //btnMoveCr.IsEnabled = true;
                cboLine.IsEnabled = true;
                btnMove.IsEnabled = true;
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }
        //대차 프린트
        private DataTable PrintCartData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = _PRINT_CTNR_ID;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        //대차 시트 Validation
        private bool ValidationCartRePrint()
        {

            if (dgDefectCtnr.Rows.Count == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region Mehod
        //이동공장조회
        private void SetAreaCombo()
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID };
            string selectedValueText = cboArea.SelectedValuePath;
            string displayMemberText = cboArea.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboArea, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);

            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
        }

        private void SetMoveProcessCombo()
        {
            try
            {
                if (dgDefectList.Rows.Count == 0)
                    return;


                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ROUTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SELF_CHECK", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.SelectedValue ?? null;
                newRow["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[0].DataItem, "ROUTID"));

                if (cboArea.SelectedValue.ToString() == LoginInfo.CFG_AREA_ID)
                {
                    newRow["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[0].DataItem, "PROCID"));
                    newRow["SELF_CHECK"] = "Y";
                }
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_MOVE_PROC_RE", "RQSTDT", "RSLTDT", inTable);

                DataRow drSelect = dtResult.NewRow();
                drSelect[cboProcess.SelectedValuePath] = "SELECT";
                drSelect[cboProcess.DisplayMemberPath] = "- SELECT -";
                dtResult.Rows.InsertAt(drSelect, 0);

                cboProcess.ItemsSource = null;
                cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                cboProcess.SelectedIndex = 0;

                cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMoveLineCombo()
        {
            try
            {
                string EqsgCode = string.Empty;

                if ((bool)chkMoveArea.IsChecked)
                {
                    EqsgCode = cboArea.SelectedValue == null ? null : cboArea.SelectedValue.ToString();
                }
                else
                {
                    EqsgCode = LoginInfo.CFG_AREA_ID;
                }

                const string bizRuleName = "DA_PRD_SEL_CART_MOVE_EQSG_PC";
                string[] arrColumn = { "LANGID", "SHOPID", "PROCID", "AREAID" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, Util.NVC(cboProcess.SelectedValue), EqsgCode };
                string selectedValueText = cboLine.SelectedValuePath;
                string displayMemberText = cboLine.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cboLine, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 출고대상 불량그룹 LOT 정보
        /// </summary>
        private void SetGridDefectList(DataTable List)
        {
            try
            {
                Util.GridSetData(dgDefectList, List, FrameOperation);
              
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }

        /// <summary>
        /// 구성대차
        /// </summary>
        private void SetGridDefectCtnr(DataTable List)
        {
            try
            {

                DataTable dtdefectctnr = DataTableConverter.Convert(dgDefectCtnr.ItemsSource);

                DataRow newRow = null;

                dtdefectctnr = new DataTable();
                dtdefectctnr.Columns.Add("CTNR_ID", typeof(string));
                dtdefectctnr.Columns.Add("PJT", typeof(string));
                dtdefectctnr.Columns.Add("PRODID", typeof(string));
                dtdefectctnr.Columns.Add("MKT_TYPE_NAME", typeof(string));
                dtdefectctnr.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtdefectctnr.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
                dtdefectctnr.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtdefectctnr.Columns.Add("INBOX_COUNT", typeof(string));
                dtdefectctnr.Columns.Add("CELL_QTY", typeof(string));

                newRow = dtdefectctnr.NewRow();
                newRow["CTNR_ID"] = "< NEW >";
                newRow["PJT"] = List.Rows[0]["PJT"].ToString();
                newRow["PRODID"] = List.Rows[0]["PRODID"].ToString();
                newRow["MKT_TYPE_NAME"] = List.Rows[0]["MKT_TYPE_NAME"].ToString();
                newRow["MKT_TYPE_CODE"] = List.Rows[0]["MKT_TYPE_CODE"].ToString();
                newRow["WIP_QLTY_TYPE_NAME"] = ObjectDic.Instance.GetObjectName("불량");
                newRow["WIP_QLTY_TYPE_CODE"] = "N";
                newRow["INBOX_COUNT"] = "0";
                newRow["CELL_QTY"] = "0";
                dtdefectctnr.Rows.Add(newRow);

                Util.GridSetData(dgDefectCtnr, dtdefectctnr, FrameOperation,false);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }


        /// <summary>
        /// INBOX(LOT) 셋팅
        /// </summary>
        private void SetGridInboxLot(DataRow[] drRow, string Chk)
        {
            try
            {

                DataTable DtInboxLot = new DataTable();
                DtInboxLot = new DataTable();
                DtInboxLot.Columns.Add("LOTID_RT", typeof(string));
                DtInboxLot.Columns.Add("NEWLOTID", typeof(string));
                DtInboxLot.Columns.Add("DFCT_RSN_GR_NAME", typeof(string));
                DtInboxLot.Columns.Add("CAPA_GRD_CODE", typeof(string));
                DtInboxLot.Columns.Add("WIPQTY", typeof(string));
                DtInboxLot.Columns.Add("OUTPUTQTY", typeof(string));
                DtInboxLot.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                DtInboxLot.Columns.Add("LOTID", typeof(string));
                DtInboxLot.Columns.Add("RESNGR_ABBR_CODE", typeof(string));
                DtInboxLot.Columns.Add("WH_DFEC_LOT", typeof(string));
                foreach (DataRow dr in drRow)
                {
                    DataRow drBindInBox = DtInboxLot.NewRow();
                    drBindInBox["LOTID_RT"] = dr["LOTID_RT"];  //조립LOT
                    drBindInBox["NEWLOTID"] = "<NEW>";         //불량LOTID
                    drBindInBox["DFCT_RSN_GR_NAME"] = dr["DFCT_RSN_GR_NAME"]; //불량그룹명
                    drBindInBox["CAPA_GRD_CODE"] = dr["CAPA_GRD_CODE"]; //등급
                    drBindInBox["WIPQTY"] = dr["WIPQTY"];               //수량
                    drBindInBox["OUTPUTQTY"] = dr["OUTPUTQTY"];
                    drBindInBox["DFCT_RSN_GR_ID"] = dr["DFCT_RSN_GR_ID"];
                    drBindInBox["LOTID"] = dr["LOTID"];
                     drBindInBox["RESNGR_ABBR_CODE"] = dr["RESNGR_ABBR_CODE"];
                    drBindInBox["WH_DFEC_LOT"] = dr["WH_DFEC_LOT"];
                    DtInboxLot.Rows.Add(drBindInBox);
                }

                //기존spread에서 중복된거 삭제

                DataTable DubInboxLot = DataTableConverter.Convert(dgDefectInbox.ItemsSource);
                if (dgDefectInbox.Rows.Count >0)
                {
                    for(int i=0; i< DubInboxLot.Rows.Count; i++)
                    {
                        if(DubInboxLot.Rows[i]["LOTID"].ToString() == DtInboxLot.Rows[0]["LOTID"].ToString())
                        {
                            DubInboxLot.Rows.Remove(DubInboxLot.Select("LOTID = '" + DtInboxLot.Rows[0]["LOTID"].ToString() + "'")[0]);
                        }
                    }
            
                }
                
                if (Chk == "A")
                {
                    if (dgDefectInbox.Rows.Count > 0)
                    {
                        DubInboxLot.Merge(DtInboxLot);
                        Util.GridSetData(dgDefectInbox, DubInboxLot, FrameOperation, false);
                    }
                    else
                    {
                        Util.GridSetData(dgDefectInbox, DtInboxLot, FrameOperation, false);
                    }
                }
                else
                {
                    if (dgDefectInbox.Rows.Count > 0)
                    {

                       
                        Util.GridSetData(dgDefectInbox, DubInboxLot, FrameOperation, false);
                    }
                }

                //구성대차 INBOX수량, Cell 수량 집계
                decimal SumInboxQty = 0;
                decimal SumCellQty = 0;
                SumInboxQty = dgDefectInbox.Rows.Count;
                for (int i = 0; i < dgDefectInbox.Rows.Count; i++)
                {
                    SumCellQty = SumCellQty + Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgDefectInbox.Rows[i].DataItem, "OUTPUTQTY")).Replace(",", ""));
                }

                DataTableConverter.SetValue(dgDefectCtnr.Rows[0].DataItem, "INBOX_COUNT", SumInboxQty);
                DataTableConverter.SetValue(dgDefectCtnr.Rows[0].DataItem, "CELL_QTY", SumCellQty);


            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }


        /// <summary>
        /// 작업자
        /// </summary>
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtUserNameCr.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        grdMain.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }


            }
        }
       
        /// <summary>
        /// 작업자 팝업닫기
        /// </summary>
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;

            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        
        /// <summary>
        /// 인계자
        /// </summary>
        private void GetUserWindow_Move()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtMoveNameCr.Text;
                wndPerson.Closed += new EventHandler(wndUser_Move_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        grdMain.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }


            }
        }
        /// <summary>
        /// 인계자 팝업닫기
        /// </summary>
        private void wndUser_Move_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtMoveNameCr.Text = wndPerson.USERNAME;
                txtMoveNameCr.Tag = wndPerson.USERID;

            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        /// <summary>
        /// 프로그래스바
        /// </summary>
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 프로그래스바
        /// </summary>
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }




        /// <summary>
        /// 대차 출고
        /// </summary>
        private void DefectOutput()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtDefectCtnr = DataTableConverter.Convert(dgDefectCtnr.ItemsSource);
                //INDATA
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["PROCID"] = LoginInfo.CFG_PROC_ID;
                row["USERID"] = txtUserNameCr.Tag.ToString();
                row["PRODID"] = Convert.ToString(dtDefectCtnr.Rows[0]["PRODID"]); //PRODID
                row["MKT_TYPE_CODE"] = Convert.ToString(dtDefectCtnr.Rows[0]["MKT_TYPE_CODE"]); //MKT_TYPE_CODE
                inDataTable.Rows.Add(row);


                //INLOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("ASSY_LOTID", typeof(string));
                inLot.Columns.Add("DFCT_RSN_GR_ID", typeof(string));
                inLot.Columns.Add("RESNGR_ABBR_CODE", typeof(string));
                inLot.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inLot.Columns.Add("ACTQTY", typeof(decimal));
                inLot.Columns.Add("DFCT_STOCK_LOTID", typeof(string));

                DataTable dtDefectInbox = DataTableConverter.Convert(dgDefectInbox.ItemsSource);


                for (int i = 0; i < dtDefectInbox.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["ASSY_LOTID"] = Convert.ToString(dtDefectInbox.Rows[i]["LOTID_RT"]); //조립LOT
                    row["DFCT_RSN_GR_ID"] = Convert.ToString(dtDefectInbox.Rows[i]["DFCT_RSN_GR_ID"]); //불량그룹코드
                    row["RESNGR_ABBR_CODE"] = Convert.ToString(dtDefectInbox.Rows[i]["RESNGR_ABBR_CODE"]); //불량그룹코드
                    row["CAPA_GRD_CODE"] = Convert.ToString(dtDefectInbox.Rows[i]["CAPA_GRD_CODE"]); //등급
                    row["ACTQTY"] = Convert.ToDecimal(dtDefectInbox.Rows[i]["OUTPUTQTY"]); //Cell수량
                    row["DFCT_STOCK_LOTID"] = Convert.ToString(dtDefectInbox.Rows[i]["WH_DFEC_LOT"]); //불량창고 불량LOT
                 
                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //출고처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_OUT_CTNR", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상처리 되었습니다.
                        btnPrint.IsEnabled = true;
                        btnOutput.IsEnabled = false;
                        _PRINT_CTNR_ID = bizResult.Tables["OUTDATA"].Rows[0]["CTNR_ID"].ToString();

                        DataTableConverter.SetValue(dgDefectCtnr.Rows[0].DataItem, "CTNR_ID", _PRINT_CTNR_ID);
                        SetGridDefectList(_PRINT_CTNR_ID);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_STOCK_OUT_CTNR", ex.Message, ex.ToString());
            }
        }


        //Validation
        private bool ValidationOutput()
        {
            if(dgDefectInbox.Rows.Count == 0)
            {
                //출고 INBOX 정보가 없습니다.
                Util.MessageValidation("SFU4594");
                return false;
            }
            if (txtUserNameCr.Text == string.Empty && txtUserNameCr.Tag == null)
            {
                //작업자 정보를 입력하세요
                Util.MessageValidation("SFU4595");
                return false;
            }
            return true;
        }
        //팝업 오픈시 체크
        private void ValidateOpen(DataTable MastInbox)
        {
            string ChkProdid = MastInbox.Rows[0]["PRODID"].ToString();
            string ChkRoutid = MastInbox.Rows[0]["ROUTID"].ToString();
            string ChkMktTypecode = MastInbox.Rows[0]["MKT_TYPE_CODE"].ToString();

            for (int i = 0; i < MastInbox.Rows.Count; i++)
            {
                //동일한 제품이 아닙니다..
                if (ChkProdid != MastInbox.Rows[i]["PRODID"].ToString())
                {
                    Util.MessageValidation("SFU4178"); 
                    this.DialogResult = MessageBoxResult.Cancel;
                }
                //동일한 라우터가 아닙니다.
                if (ChkRoutid != MastInbox.Rows[i]["ROUTID"].ToString())
                {
                    Util.MessageValidation("SFU4596"); //동일한 라우터가 아닙니다.
                    this.DialogResult = MessageBoxResult.Cancel;
                }
                //동일한 시장유형이 아닙니다.
                if (ChkMktTypecode != MastInbox.Rows[i]["MKT_TYPE_CODE"].ToString())
                {
                    Util.MessageValidation("SFU4271"); //동일한 시장유형이 아닙니다.
                    this.DialogResult = MessageBoxResult.Cancel;
                }
            }
        }
        // <summary>
        /// 불량 LOT List
        /// </summary>
        private void SetGridDefectList(string CtnrId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["CTNR_ID"] = CtnrId;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_DFEC_WH_OUTPUT_INBOX", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.gridClear(dgDefectInbox);
                    Util.GridSetData(dgDefectInbox, dtRslt, FrameOperation, false);

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }


        #endregion

        //동간이동
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMove())
                return;

            // 이동 하시겠습니까?
            Util.MessageConfirm("SFU1763", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CartMove();
                }
            });
        }


        /// <summary>
        /// 대차 이동
        /// </summary>
        private void CartMove()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = DataTableConverter.Convert(cboProcess.ItemsSource);

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("TO_ROUTID", typeof(string));
                inTable.Columns.Add("TO_FLOWID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("MOVE_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("FROM_EQSGID", typeof(string));
                inTable.Columns.Add("FROM_PROCID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["TO_PROCID"] = cboProcess.SelectedValue.ToString();
                newRow["TO_ROUTID"] = dt.Rows[cboProcess.SelectedIndex]["ROUTID_TO"].ToString();
                newRow["TO_FLOWID"] = dt.Rows[cboProcess.SelectedIndex]["FLOWID_TO"].ToString();
                newRow["TO_EQSGID"] = cboLine.SelectedValue.ToString();
                newRow["MOVE_USERID"] = txtUserNameCr.Tag == null ? string.Empty : txtUserNameCr.Tag.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["NOTE"] = string.Empty;
                newRow["FROM_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgDefectList.Rows[0].DataItem, "EQSGID"));
                newRow["FROM_PROCID"] = LoginInfo.CFG_PROC_ID;

                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = _PRINT_CTNR_ID;
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_CTNR", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }
                        this.DialogResult = MessageBoxResult.OK;
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

        //대차이동 Validation
        private bool ValidationMove()
        {
            //DataTable dt = DataTableConverter.Convert(dgDefectCtnr.ItemsSource);
            //DataRow[] dr = dt.Select("CTNR_ID = '" + txtCartID.Text + "'");

            //if (dr.Length == 0)
            //{
            //    // 대차 정보가 없습니다.
            //    Util.MessageValidation("SFU4365");
            //    return false;
            //}

            //if (Util.NVC_Int(dr[0]["CELL_QTY"]) == 0)
            //{
            //    // 대차에 Inbox 정보가 없습니다.
            //    Util.MessageValidation("SFU4375");
            //    return false;
            //}

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 공정을 선택하세요.
                Util.MessageValidation("SFU4362");
                return false;
            }

            if (cboLine.SelectedValue == null || cboLine.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 라인을 선택하세요.
                Util.MessageValidation("SFU4363");
                return false;
            }

            return true;
        }
    }
}