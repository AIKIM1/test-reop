using System.Windows;
using C1.WPF;
using System.Windows.Controls;
using System.Data;
using System;
using System.Diagnostics;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System.Reflection;
using LGC.GMES.MES.CMM001.Extensions;
using Process = LGC.GMES.MES.CMM001.Class.Process;

namespace LGC.GMES.MES.CMM001.UserControls
{
    public partial class UcPolymerFormCart
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string ProdLotId { get; set; }
        public string ProdWipSeq { get; set; }

        public Button ButtonCart1 { get; set; }
        public Button ButtonCart2 { get; set; }
        public Button ButtonCart3 { get; set; }
        public Button ButtonCart4 { get; set; }
        public Button ButtonCart5 { get; set; }

        public Grid GridCart1 { get; set; }
        public Grid GridCart2 { get; set; }
        public Grid GridCart3 { get; set; }
        public Grid GridCart4 { get; set; }
        public Grid GridCart5 { get; set; }

        //public Button ButtonCartCreate { get; set; }
        //public Button ButtonCartDelete { get; set; }
        public Button ButtonCartMove { get; set; }
        public Button ButtonCartSelect { get; set; }
        public Button ButtonCartStorage { get; set; }
        public string ProdCartId { get; set; }
        public int CartCount;

        public Grid GridCart { get; set; }

        public UcPolymerFormCart()
        {
            InitializeComponent();
            SetControl();
            SetButtons();
        }
        #endregion

        #region Initialize
        private void SetControl()
        {
            foreach (Grid grid in Util.FindVisualChildren<Grid>(dgCart))
            {
                switch (grid.Name)
                {
                    case "grdCart1":
                        GridCart1 = grid;
                        break;
                    case "grdCart2":
                        GridCart2 = grid;
                        break;
                    case "btnCart3":
                        GridCart3 = grid;
                        break;
                    case "btnCart4":
                        GridCart4 = grid;
                        break;
                    case "btnCart5":
                        GridCart5 = grid;
                        break;
                }
            }
        }

        private void SetButtons()
        {
            ButtonCart1 = btnCart1;
            ButtonCart2 = btnCart2;
            ButtonCart3 = btnCart3;
            ButtonCart4 = btnCart4;
            ButtonCart5 = btnCart5;

            //ButtonCartCreate = btnCartCreate;
            //ButtonCartDelete = ButtonCartDelete;
            ButtonCartMove = btnCartMove;
            ButtonCartStorage = btnCartStorage;
            ButtonCartSelect = btnCartSelect;
            GridCart = dgCart;
        }

        public void InitializeButtonControls()
        {
            for (int i = 1; i <= 5; i++)
            {
                foreach (Grid grid in Util.FindVisualChildren<Grid>(dgCart))
                {
                    if (grid.Name == "grdCart" + i)
                    {
                        grid.Background = new SolidColorBrush(Colors.White);
                    }
                }

                foreach (TextBlock textBlock in Util.FindVisualChildren<TextBlock>(dgCart))
                {
                    if (textBlock.Name == "tbCart" + i)
                    {
                        textBlock.Text = string.Empty;
                    }

                    if (textBlock.Name == "tbCartInBoxCount" + i)
                    {
                        textBlock.Text = string.Empty;
                    }
                }

                foreach (Image image in Util.FindVisualChildren<Image>(dgCart))
                {
                    image.Visibility = Visibility.Collapsed;
                }

                foreach (Button bntton in Util.FindVisualChildren<Button>(dgCart))
                {
                    bntton.Content = string.Empty;
                    bntton.Tag = null;
                }

            }
        }

        public void InitializeControls()
        {

        }

        public void ChangeEquipment(string equipmentCode)
        {
            try
            {
                EquipmentCode = equipmentCode;
                ProdLotId = string.Empty;

                InitializeControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetControlVisibility()
        {
            if (string.Equals(ProcessCode, Process.PolymerOffLineCharacteristic) ||
                string.Equals(ProcessCode, Process.PolymerFinalExternalDSF) ||
                string.Equals(ProcessCode, Process.PolymerFinalExternal))
            {
                dgCart.Visibility = Visibility.Collapsed;
                btnCartCreate.Visibility = Visibility.Collapsed;
                btnCartDelete.Visibility = Visibility.Collapsed;
                btnCartDetail.Visibility = Visibility.Collapsed;
                btnCartRePrint.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event

        private void btnCartCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartCreate()) return;

            // 대차 생성
            CartCreate();
        }

        private void btnCartDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartDelete()) return;

            // 대차 삭제
            CartDelete();
        }

        private void btnCartDetail_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartDetail()) return;

            // 대차 상세
            CartDetail();
        }

        private void btnCartRePrint_Click(object sender, RoutedEventArgs e)
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
        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 대차 생성
        /// </summary>
        private void CartCreate()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CTNR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CTNR_TYPE_CODE"] = "CART";
                newRow["WIP_QLTY_TYPE_CODE"] = "G";
                newRow["PROCID"] = ProcessCode;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID; ;
                newRow["EQPTID"] = EquipmentCode;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CREATE_CTNR", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        GetProductCart(false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 대차 삭제
        /// </summary>
        private void CartDelete()
        {
            try
            {
                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));
                inCNTR.Columns.Add("CART_DEL_RSN_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = Util.NVC(ProcessCode);
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = ProdCartId;
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        //SetSelectCart();
                        GetProductCart(false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 대차 출력 자료
        /// </summary>
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
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["CART_ID"] = ProdCartId;
                newRow["PROCID"] = Util.NVC(ProcessCode);
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }

        #endregion

        #region[[Validation]
        private bool ValidationCartCreate()
        {
            if (string.IsNullOrWhiteSpace(EquipmentCode)  || EquipmentCode.Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //if (string.IsNullOrWhiteSpace(ProdLotId))
            //{
            //    // 생산 Lot 정보가 없습니다.
            //    Util.MessageValidation("SFU4014");
            //    return false;
            //}

            if (CartCount == 5)
            {
                // 대차 생성은 5개까지만 가능 합니다.
                Util.MessageValidation("SFU4364");
                return false;
            }

            return true;
        }

        private bool ValidationCartDelete()
        {
            if (string.IsNullOrWhiteSpace(EquipmentCode) || EquipmentCode.Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //if (string.IsNullOrWhiteSpace(ProdLotId))
            //{
            //    // 생산 Lot 정보가 없습니다.
            //    Util.MessageValidation("SFU4014");
            //    return false;
            //}

            //if (ButtonCertSelect.Tag == null)
            //{
            //    // 삭제할 대차를 선택하세요.
            //    Util.MessageValidation("SFU4357");
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(ProdCartId))
            {
                // 삭제할 대차를 선택하세요.
                Util.MessageValidation("SFU4357");
                return false;
            }

            return true;
        }

        private bool ValidationCartDetail()
        {
            if (string.IsNullOrWhiteSpace(EquipmentCode) || EquipmentCode.Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //if (ButtonCertSelect.Tag == null)
            //{
            //    // 상세 조회 대차를 선택하세요.
            //    Util.MessageValidation("SFU4359");
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(ProdCartId))
            {
                // 상세 조회 대차를 선택하세요.
                Util.MessageValidation("SFU4359");
                return false;
            }

            return true;
        }

        private bool ValidationCartRePrint()
        {
            if (string.IsNullOrWhiteSpace(EquipmentCode) || EquipmentCode.Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //if (string.IsNullOrWhiteSpace(ProdLotId))
            //{
            //    // 생산 Lot 정보가 없습니다.
            //    Util.MessageValidation("SFU4014");
            //    return false;
            //}

            //if (ButtonCertSelect.Tag == null)
            //{
            //    // 재발행 대차를 선택하세요.
            //    Util.MessageValidation("SFU4360");
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(ProdCartId))
            {
                // 재발행 대차를 선택하세요.
                Util.MessageValidation("SFU4360");
                return false;
            }


            return true;
        }
        #endregion

        #region [Func]
        //public void SetSelectCart(Button buttonClick = null)
        //{
        //    InitializeControls();

        //    if (buttonClick != null)
        //    {
        //        foreach (Button btn in Util.FindVisualChildren<Button>(dgCart))
        //        {
        //            if (btn.Name.Equals(buttonClick.Name))
        //            {
        //                btn.Background = new SolidColorBrush(Colors.Red);
        //                btn.Foreground = new SolidColorBrush(Colors.Yellow);
        //                ButtonCertSelect.Tag = btn.Tag;
        //            }
        //            else
        //            {
        //                btn.Background = new SolidColorBrush(Colors.White);
        //                btn.Foreground = new SolidColorBrush(Colors.Black);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        CartSelect();

        //        // Clear
        //        foreach (Button btn in Util.FindVisualChildren<Button>(dgCart))
        //        {
        //            btn.Content = string.Empty;
        //            btn.Tag = null;
        //            btn.Background = new SolidColorBrush(Colors.White);
        //            btn.Foreground = new SolidColorBrush(Colors.Black);
        //            ButtonCertSelect.Tag = null;
        //        }

        //        // 설정
        //        _cartCount = _cart.Rows.Count;

        //        for (int row = 0; row < _cart.Rows.Count; row++)
        //        {
        //            string CartName = "btnCart" + (row + 1).ToString();

        //            foreach (Button btn in Util.FindVisualChildren<Button>(dgCart))
        //            {
        //                if (btn.Name.Equals(CartName))
        //                {
        //                    btn.Content = _cart.Rows[row]["CTNR_ID"].ToString() + "\n" + "     (" + _cart.Rows[row]["INBOX_COUNT"].ToString() + ")";
        //                    btn.Tag = _cart.Rows[row]["CTNR_ID"].ToString();

        //                    if (row + 1 == _cart.Rows.Count)
        //                    {
        //                        btn.Background = new SolidColorBrush(Colors.Red);
        //                        btn.Foreground = new SolidColorBrush(Colors.Yellow);
        //                        ButtonCertSelect.Tag = btn.Tag;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //}

        ///// <summary>
        ///// Cart Move 팝업
        ///// </summary>
        //private void CartMove()
        //{
        //    CMM_POLYMER_FORM_CART_MOVE popupCartMove = new CMM_POLYMER_FORM_CART_MOVE();
        //    popupCartMove.FrameOperation = this.FrameOperation;

        //    object[] parameters = new object[5];
        //    parameters[0] = ProcessCode;
        //    parameters[1] = ProcessName;
        //    parameters[2] = EquipmentCode;
        //    parameters[3] = EquipmentName;
        //    parameters[4] = ProdCartId;  // ButtonCertSelect.Tag;

        //    C1WindowExtension.SetParameters(popupCartMove, parameters);

        //    popupCartMove.Closed += new EventHandler(popupCartMove_Closed);

        //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
        //    {
        //        if (tmp.Name == "grdMain")
        //        {
        //            tmp.Children.Add(popupCartMove);
        //            popupCartMove.BringToFront();
        //            break;
        //        }
        //    }
        //}

        //private void popupCartMove_Closed(object sender, EventArgs e)
        //{
        //    CMM_POLYMER_FORM_CART_MOVE popup = sender as CMM_POLYMER_FORM_CART_MOVE;
        //    if (popup != null && popup.DialogResult == MessageBoxResult.OK)
        //    {
        //        //SetSelectCart();
        //        GetProductLot();
        //        GetProductCart();
        //    }

        //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
        //    {
        //        if (tmp.Name == "grdMain")
        //        {
        //            tmp.Children.Remove(popup);
        //            break;
        //        }
        //    }

        //}

        /// <summary>
        /// Cart 상세 팝업
        /// </summary>
        private void CartDetail()
        {
            CMM_POLYMER_FORM_CART_DETAIL popupCartDetail = new CMM_POLYMER_FORM_CART_DETAIL();
            popupCartDetail.FrameOperation = this.FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = ProcessCode;
            parameters[1] = ProcessName;
            parameters[2] = EquipmentCode;
            parameters[3] = EquipmentName;
            parameters[4] = ProdCartId;    // ButtonCertSelect.Tag;

            C1WindowExtension.SetParameters(popupCartDetail, parameters);

            popupCartDetail.Closed += new EventHandler(popupCartDetail_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartDetail);
                    popupCartDetail.BringToFront();
                    break;
                }
            }
        }

        private void popupCartDetail_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_DETAIL popup = sender as CMM_POLYMER_FORM_CART_DETAIL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductCart(false);
            }
            else
            {
                GetProductCart(true);
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



        /// <summary>
        /// Cart 상세 팝업
        /// </summary>
        //private void CartDetail_Box()
        //{
        //    CMM_BOX_CART_DETAIL popupCartDetail_Box = new CMM_BOX_CART_DETAIL();
        //    popupCartDetail_Box.FrameOperation = this.FrameOperation;

        //    object[] parameters = new object[5];
        //    parameters[0] = ProcessCode;
        //    parameters[1] = ProcessName;
        //    parameters[2] = EquipmentCode;
        //    parameters[3] = EquipmentName;
        //    parameters[4] = ProdCartId;    // ButtonCertSelect.Tag;

        //    C1WindowExtension.SetParameters(popupCartDetail_Box, parameters);

        //    popupCartDetail_Box.Closed += new EventHandler(popupCartDetail_Box_Closed);

        //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
        //    {
        //        if (tmp.Name == "grdMain")
        //        {
        //            tmp.Children.Add(popupCartDetail_Box);
        //            popupCartDetail_Box.BringToFront();
        //            break;
        //        }
        //    }
        //}

        //private void popupCartDetail_Box_Closed(object sender, EventArgs e)
        //{
        //    CMM_BOX_CART_DETAIL popup = sender as CMM_BOX_CART_DETAIL;
        //    if (popup != null && popup.DialogResult == MessageBoxResult.OK)
        //    {
        //    }

        //    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
        //    {
        //        if (tmp.Name == "grdMain")
        //        {
        //            tmp.Children.Remove(popup);
        //            break;
        //        }
        //    }

        //}
        /// <summary>
        /// 재발행 팝업
        /// </summary>
        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;

            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;

            object[] parameters = new object[5];
            parameters[0] = ProcessCode;
            parameters[1] = EquipmentCode;
            parameters[2] = ProdCartId;   // ButtonCertSelect.Tag;
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

        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductCart(false);
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

        protected virtual void GetProductLot()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                ////for (int i = 0; i < parameterArrys.Length; i++)
                ////{
                ////    parameterArrys[i] = true;
                ////}

                parameterArrys[0] = false;
                parameterArrys[1] = null;

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        protected virtual void GetProductCart(bool isSelected = true)
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductCart");
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    object[] parameterArrys = new object[parameters.Length];

                    parameterArrys[0] = null;
                    parameterArrys[1] = isSelected;

                    methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
                }
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

        public void SetButtonVisibility()
        {
            if (string.Equals(ProcessCode, Process.CELL_BOXING) ||
                string.Equals(ProcessCode, Process.CELL_BOXING_RETURN))
            {
                btnCartStorage.Visibility = Visibility.Visible;
                btnCartMove.Visibility = Visibility.Collapsed;
                btnCartSelect.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #endregion



    }
}