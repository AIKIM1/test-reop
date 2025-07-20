/*************************************************************************************
 Created Date : 2019.06.17
      Creator : 손우석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2019.06.17 손우석 CSR ID 3970086 GMES 내 보류재고 관리 시스템화 기능 구현_기간별 현황표시, 보류재고 현황 관리 탭 추가. [요청번호]C20190409_70086
 2019.08.30 손우석 CSR ID 3970086 GMES 내 보류재고 관리 시스템화 기능 구현_기간별 현황표시, 보류재고 현황 관리 탭 추가. [요청번호]C20190409_70086
 2020.01.06 손우석 CSR ID 17266 GMES 보류재고 Summary 기능 개선 요청 건 [요청번호 C20200102-000782]
 2020.01.16 손우석 CSR ID 17266 GMES 보류재고 Summary 기능 개선 요청 건 [요청번호 C20200102-000782]
 2020.01.28 손우석 CSR ID 17266 GMES 보류재고 Summary 기능 개선 요청 건 [요청번호 C20200102-000782]
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_042_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util util = new Util();

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        private string sCU = string.Empty;
        private string sPRODID = string.Empty;
        private string sSHOPID = string.Empty;
        private string sAREAID = string.Empty;
        private string sLINEID = string.Empty;
        private string sMODELID = string.Empty;
        private string sEQPTID = string.Empty;
        //2020.01.28
        private string sSLOCID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion Declaration & Constructor 

        #region Initialize
        public PACK001_042_POPUP()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                sCU = Util.NVC(tmps[0]);

                switch (sCU)
                {
                    case "S":
                        cboShop.IsEnabled = false;
                        cboAreaByAreaType.IsEnabled = false;
                        cboEquipmentSegment.IsEnabled = false;
                        cboProductModel.IsEnabled = false;
                        cboSloc.IsEnabled = false;
                        cboProdClass.IsEnabled = false;
                        cboProduct.IsEnabled = false;
                        txtProdName.IsReadOnly = true;
                        txtHoldProd.IsReadOnly = true;
                        dtpWorkStartDay.IsEnabled = false;
                        //txtReason.IsReadOnly = true;
                        RtxtReason.IsReadOnly = true;
                        txtQty.IsReadOnly = true;
                        txtBOMQty.IsReadOnly = true;
                        txtTeam.IsReadOnly = true;
                        txtTeamName.IsReadOnly = true;
                        txtFreUserID.IsReadOnly = true;
                        txtFreUserNameCr.IsReadOnly = true;
                        //txtContent.IsReadOnly = true;
                        RtxtContent.IsReadOnly = true;
                        dtpWorkEndDay.IsEnabled = false;
                        cboComplete.IsEnabled = false;
                        txtETC.IsReadOnly = true;

                        btnSave.Visibility = Visibility.Hidden;
                        //2020.01.06
                        txtPrice.IsReadOnly = true;
                        break;

                    case "U":
                        cboShop.IsEnabled = false;
                        cboAreaByAreaType.IsEnabled = false;
                        cboEquipmentSegment.IsEnabled = false;
                        cboProductModel.IsEnabled = false;
                        cboSloc.IsEnabled = false;
                        cboProdClass.IsEnabled = false;
                        cboProduct.IsEnabled = false;
                        txtProdName.IsReadOnly = true;
                        txtHoldProd.IsReadOnly = true;
                        dtpWorkStartDay.IsEnabled = false;
                        //txtReason.IsReadOnly = true;
                        //txtQty.IsReadOnly = true;
                        txtBOMQty.IsReadOnly = true;
                        txtTeam.IsReadOnly = true;
                        txtTeamName.IsReadOnly = true;
                        txtFreUserID.IsReadOnly = true;
                        txtFreUserNameCr.IsReadOnly = true;
                        //txtContent.IsReadOnly = true;
                        //2020.01.16
                        //dtpWorkEndDay.IsEnabled = false;
                        dtpWorkEndDay.IsEnabled = true;
                        //cboComplete.IsEnabled = false;
                        //2020.01.06
                        txtETC.IsReadOnly = false;
                        //2020.01.06
                        txtPrice.IsReadOnly = false;
                        break;

                    //2019.08.30
                    case "W":
                        break;
                }

                //2019.08.30
                if (sCU != "C")
                {
                    sSHOPID = Util.NVC(tmps[1]);
                    sAREAID = Util.NVC(tmps[2]);
                    sLINEID = Util.NVC(tmps[3]);
                    sMODELID = Util.NVC(tmps[4]);
                    //2020.01.28
                    sSLOCID = Util.NVC(tmps[5]);
                }

                //InitCombo();
                InitCombo(sCU);

                if (sCU == "C")
                {
                    sSHOPID = Util.NVC(tmps[1]);
                    //sAREAID = Util.NVC(tmps[2]);
                    //sLINEID = Util.NVC(tmps[3]);
                    //sMODELID = Util.NVC(tmps[4]);

                    dtpWorkStartDay.SelectedDateTime = DateTime.Today;
                    dtpWorkEndDay.SelectedDateTime = DateTime.Today;

                    txtLOTID.IsReadOnly = false;
                }
                else
                {
                    txtLOTID.IsReadOnly = true;

                    cboShop.SelectedValue = Util.NVC(tmps[1]);
                    cboAreaByAreaType.SelectedValue = Util.NVC(tmps[2]);
                    cboEquipmentSegment.SelectedValue = Util.NVC(tmps[3]);
                    cboProductModel.SelectedValue = Util.NVC(tmps[4]);
                    cboSloc.SelectedValue = Util.NVC(tmps[5]);
                    cboProdClass.SelectedValue = Util.NVC(tmps[6]);
                    cboProduct.SelectedValue = Util.NVC(tmps[7]);
                    txtProdName.Text = Util.NVC(tmps[8]);
                    txtHoldProd.Text = Util.NVC(tmps[9]);

                    string sDate = tmps[10].ToString().Substring(0,4) + "-" + tmps[10].ToString().Substring(4, 2) + "-" +  tmps[10].ToString().Substring(6, 2) + " 00:00:00";
                    DateTime DTSDate = Convert.ToDateTime(sDate);
                    dtpWorkStartDay.SelectedDateTime = DTSDate;

                    //txtReason.Text = Util.NVC(tmps[11]);
                    RtxtReason.AppendText(Util.NVC(tmps[11]));

                    txtQty.Text = Util.NVC(tmps[12]);
                    txtBOMQty.Text = Util.NVC(tmps[13]);
                    txtTeam.Text = Util.NVC(tmps[14]);
                    txtTeamName.Text = Util.NVC(tmps[15]);
                    txtFreUserID.Text = Util.NVC(tmps[16]);
                    txtFreUserNameCr.Text = Util.NVC(tmps[17]);
                    //txtContent.Text = Util.NVC(tmps[18]);
                    RtxtContent.AppendText(Util.NVC(tmps[18]));

                    string eDate = tmps[19].ToString().Substring(0, 4) + "-" + tmps[19].ToString().Substring(4, 2) + "-" + tmps[19].ToString().Substring(6, 2) + " 00:00:00";
                    DateTime DTEDate = Convert.ToDateTime(eDate);
                    dtpWorkEndDay.SelectedDateTime = DTEDate;

                    cboComplete.SelectedValue = Util.NVC(tmps[20]);
                    //2020.01.06
                    //txtETC.Text = Util.NVC(tmps[21]);
                    txtETC.AppendText(Util.NVC(tmps[21]));

                    //2020.01.06
                    txtPrice.Text = Util.NVC(tmps[22]);

                    Detail();

                    //this.DialogResult = MessageBoxResult.Cancel;
                    //this.Close();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void InitCombo(string strGubun)
        {
            Set_Combo_Shop(cboShop);
            Set_Combo_Area(cboAreaByAreaType);
            //2019.08.30
            //Set_Combo_EquipmentSegmant(cboEquipmentSegment);
            //Set_Combo_ProductModel(cboProductModel);
            //SetcboProduct(cboProduct);

            Set_Combo_EquipmentSegmant(cboEquipmentSegment, strGubun);
            Set_Combo_ProductModel(cboProductModel, strGubun);
            SetcboProduct(cboProduct, strGubun);

            SetcboCompleteCode();
            SecboProdClassCode();
            //2020.01.28
            //SetcboSloc(cboSloc);
            SetcboSloc(cboSloc, strGubun);
        }
        #endregion Initialize

        #region Event

        #region Button
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFreUserNameCr.Text))
                {
                    // 사용자를 선택하세요.
                    Util.MessageValidation("SFU4983");
                    return;
                }

                //GetUserWindow();
                GetUserList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion Button

        #region Text
        private void txtFreUserNameCr_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtFreUserNameCr.Text))
                    {
                        // 사용자를 선택하세요.
                        Util.MessageValidation("SFU4983");
                        return;
                    }

                    GetUserList();
                    //GetUserWindow();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtLOTID.Text))
                    {
                        // LOTID를 입력하세요.
                        Util.MessageValidation("SFU1813");
                        return;
                    }

                    Lotmapping();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        private void txtHoldProd_KeyDown(object sender, KeyEventArgs e)
        {
            GetMbom();
        }

        private void txtQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GetDataTypeNumStr(txtQty.Text) == false)
            {
                Util.Alert("SFU2877");  //숫자만 입력 가능합니다.
                txtQty.Text = "";
                return;
            }
        }

        private void txtBOMQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GetDataTypeNumStr(txtBOMQty.Text) == false)
            {
                Util.Alert("SFU2877");  //숫자만 입력 가능합니다.
                txtBOMQty.Text = "";
                return;
            }
        }
        #endregion Text

        #region Combo
        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboShop.SelectedIndex > -1)
            {
                if (sCU == "C")
                {
                    sSHOPID = Convert.ToString(cboShop.SelectedValue);
                    Set_Combo_Area(cboAreaByAreaType);
                }
                else
                {
                    cboShop.SelectedValue = sSHOPID;
                }
            }
            else
            {
                //2020.01.28
                //sSHOPID = string.Empty;
            }
        }
        private void cboAreaByAreaType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboAreaByAreaType.SelectedIndex > -1)
            {
                if (sCU == "C")
                {
                    sAREAID = Convert.ToString(cboAreaByAreaType.SelectedValue);
                    //2019.08.30
                    //Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment, sCU);
                }
                else
                {
                    //2020.01.28
                    ////2019.08.30
                    ////Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    ////Set_Combo_ProductModel(cboProductModel);
                    //Set_Combo_EquipmentSegmant(cboEquipmentSegment, sCU);
                    //Set_Combo_ProductModel(cboProductModel, sCU);
                    //SetcboSloc(cboSloc);
                    ////2019.08.30
                    ////SetcboProduct(cboProduct);
                    //SetcboProduct(cboProduct, sCU);
                }
            }
            else
            {
                //2020.01.28
                //sAREAID = string.Empty;
            }
        }
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedIndex > -1)
            {
                if (sCU == "C")
                {
                    sLINEID = Convert.ToString(cboEquipmentSegment.SelectedValue);

                    if ((sSHOPID != null) & (sAREAID != null))
                    {
                        //2019.08.30
                        //Set_Combo_ProductModel(cboProductModel);
                        Set_Combo_ProductModel(cboProductModel, sCU);
                        //2020.01.28
                        //SetcboSloc(cboSloc);
                        SetcboSloc(cboSloc, sCU);
                        //2019.08.30
                        //SetcboProduct(cboProduct);
                        SetcboProduct(cboProduct, sCU);
                    }
                }
                else
                {

                }
            }
            else
            {
                //2020.01.28
                //sLINEID = string.Empty;
            }
        }
        private void cboProductModel_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                //this.Dispatcher.BeginInvoke(new System.Action(() =>
                //{
                if (cboProductModel.SelectedIndex > -1)
                {
                    if (sCU == "C")
                    {
                        //2019.08.30
                        //SetcboProduct(cboProduct);
                        SetcboProduct(cboProduct, sCU);
                    }
                    else
                    {

                    }
                }
                else
                {
                    //2020.01.28
                    //sMODELID = string.Empty;
                }
                //}));
            }
            catch
            {
            }
        }
        #endregion Combo

        #region Grid
        private void dgLotList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

        }

        private void dgUserChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTableConverter.SetValue(dgUser.Rows[idx].DataItem, "CHK", true);
                //row 색 바꾸기
                dgUser.SelectedIndex = idx;

                txtFreUserID.Text = DataTableConverter.GetValue(dgUser.Rows[idx].DataItem, "USERID").ToString();
                txtFreUserNameCr.Text = DataTableConverter.GetValue(dgUser.Rows[idx].DataItem, "USERNAME").ToString();
                txtTeam.Text = Util.NVC(DataTableConverter.GetValue(dgUser.Rows[idx].DataItem, "DEPTID"));
                txtTeamName.Text = Util.NVC(DataTableConverter.GetValue(dgUser.Rows[idx].DataItem, "DEPTNAME"));

                dgUser.ItemsSource = null;
            }
        }

        private void DataGrid01RowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                int icnt = 0;
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LOTID"] = txtLOTID.Text;

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MAPPINGLOTID", "RQSTDT", "RSLTDT", INDATA);

                if (dtRslt != null)
                {
                    if (dtRslt.Rows.Count > 0)
                    {
                        DataSet dsResult = new DataSet();
                        DataTable inTable = new DataTable();

                        if (dg.ItemsSource == null)
                        {
                            inTable = dsResult.Tables.Add("INDATA");
                        }
                        else
                        {
                            inTable = DataTableConverter.Convert(dgLotList.ItemsSource);
                        }

                        if (dg.ItemsSource != null)
                        {
                            for (int irow = 0; irow < dgLotList.Rows.Count; irow++)
                            {
                                if (DataTableConverter.GetValue(dgLotList.Rows[irow].DataItem, "LOTID").ToString() == txtLOTID.Text)
                                {
                                    icnt = icnt + 1;
                                }
                            }

                            if ((icnt > 0))
                            {
                                Util.Alert("10017");  //입력하려는 값이 이미 존재합니다.
                                return;
                            }
                            else
                            {
                                DataRow dr2 = inTable.NewRow();
                                dr2["LOTID"] = txtLOTID.Text;

                                inTable.Rows.Add(dr2);

                                dgLotList.ItemsSource = DataTableConverter.Convert(inTable);
                            }
                        }
                        else
                        {
                            inTable.Columns.Add("LOTID", typeof(string));

                            DataRow dr2 = inTable.NewRow();
                            dr2["LOTID"] = txtLOTID.Text;

                            inTable.Rows.Add(dr2);

                            dgLotList.ItemsSource = DataTableConverter.Convert(dsResult.Tables["INDATA"]);
                        }

                        txtLOTID.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        #endregion Grid

        #endregion Event

        #region Mehod

        private bool GetDataTypeNumStr(string sInput)
        {
            int iCnt = 0;
            var sChar = sInput.ToCharArray();

            for (int i = 0; i < sChar.Length; i++)
            {
                if (char.IsNumber(sChar[i]) == false)
                {
                    iCnt = iCnt + 1;
                }
            }

            if (iCnt > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void GetUserWindow()
        {
            //CMM_PERSON wndPerson = new CMM_PERSON();
            //wndPerson.FrameOperation = FrameOperation;

            //if (wndPerson != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = txtFreUserNameCr.Text;
            //    C1WindowExtension.SetParameters(wndPerson, Parameters);

            //    wndPerson.Closed += new EventHandler(wndUser_Closed);
            //    Search.Children.Add(wndPerson);
            //    wndPerson.BringToFront();
            //}
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            //CMM_PERSON wndPerson = sender as CMM_PERSON;
            //if (wndPerson.DialogResult == MessageBoxResult.OK)
            //{
            //    txtFreUserNameCr.Text = wndPerson.USERNAME;
            //    txtFreUserID.Text = wndPerson.USERID;
            //    txtTeam.Text = wndPerson.DEPTNAME;
            //}
            //else
            //{
            //    txtFreUserNameCr.Text = string.Empty;
            //    txtFreUserID.Text = string.Empty;
            //    txtTeam.Text = string.Empty;
            //}
        }

        #region Combo
        private void Set_Combo_Shop(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("SYSID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["SYSID"] = LGC.GMES.MES.Common.LoginInfo.SYSID + "_" + LGC.GMES.MES.Common.Common.APP_System;
                dtRQSTDT.Rows.Add(drnewrow);

                //    new ClientProxy().ExecuteService("DA_BAS_SEL_SHOP_BY_USERID_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                //    {
                //        if (Exception != null)
                //        {
                //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                //            return;
                //        }
                //        cbo.ItemsSource = DataTableConverter.Convert(result);
                //        if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sSHOPID) select dr).Count() > 0)
                //        {
                //            cbo.SelectedValue = sSHOPID;
                //        }
                //        else if (result.Rows.Count > 0)
                //        {
                //            cbo.SelectedIndex = 0;
                //        }
                //        else if (result.Rows.Count == 0)
                //        {
                //            cbo.SelectedItem = null;
                //        }
                //    });
                //}
                //catch (Exception ex)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                //    return;
                //}

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHOP_BY_USERID_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dtResult != null && dtResult.Rows.Count == 1)
                {
                    cbo.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (sCU == "C")
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else
                    {
                        cbo.SelectedValue = sSHOPID;
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_Area(C1ComboBox cbo)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { sSHOPID, Area_Type.PACK };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        //2019.08.30
        //private void Set_Combo_EquipmentSegmant(C1ComboBox cbo)
        private void Set_Combo_EquipmentSegmant(C1ComboBox cbo, string strUD)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { cboAreaByAreaType.SelectedValue.ToString() };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);

                if (cbo.SelectedIndex > 0)
                {
                    //2019/08.30
                    //sLINEID = cboEquipmentSegment.SelectedValue.ToString();
                    if (strUD != "C")
                    {
                        cboEquipmentSegment.SelectedValue = sLINEID;
                    }
                    else
                    {
                        sLINEID = cboEquipmentSegment.SelectedValue.ToString();
                        cboEquipmentSegment.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        //2019.08.30
        //private void Set_Combo_ProductModel(C1ComboBox cbo)
        private void Set_Combo_ProductModel(C1ComboBox cbo, string strUD)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                drnewrow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
                drnewrow["USERID"] = LoginInfo.USERID;
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);
                cbo.ItemsSource = DataTableConverter.Convert(dtRslt);
                
                if (dtRslt.Rows.Count > 0)
                {
                    //2019.08.30
                    //cbo.SelectedIndex = 0;
                    if (strUD != "C")
                    {
                        cbo.SelectedValue = sMODELID;
                    }
                    else
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                //new ClientProxy().ExecuteService("DA_BAS_SEL_PRJMODEL_AUTH_MULTI_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                //{
                //    if (Exception != null)
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                //        return;
                //    }
                //     cbo.ItemsSource = DataTableConverter.Convert(result);
                    //if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sSHOPID) select dr).Count() > 0)
                    //{
                    //    cbo.SelectedValue = sMODELID;
                    //}
                    //else if (result.Rows.Count > 0)
                    //{
                    //    cbo.SelectedIndex = 0;
                    //}
                    //else if (result.Rows.Count == 0)
                    //{
                    //    cbo.SelectedItem = null;
                    //}
                //});
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.08.30
        //private void SetcboProduct(C1ComboBox cbo)
        private void SetcboProduct(C1ComboBox cbo, string strUD)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("MODLID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                drnewrow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                drnewrow["PROCID"] = null;
                drnewrow["MODLID"] = cboProductModel.SelectedValue.ToString();
                drnewrow["AREA_TYPE_CODE"] = Area_Type.PACK;
                drnewrow["PRDT_CLSS_CODE"] = null;

                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);
                cbo.ItemsSource = DataTableConverter.Convert(dtRslt);

                if (dtRslt.Rows.Count > 0)
                {
                    //2019.08.30
                    //cbo.SelectedIndex = 0;
                    if (strUD != "C")
                    {
                        cbo.SelectedValue = sPRODID;
                    }
                    else
                    {
                        cbo.SelectedIndex = 0;
                    }
                }

                //new ClientProxy().ExecuteService("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                //{
                //    if (Exception != null)
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                //        return;
                //    }

                //     cbo.ItemsSource = DataTableConverter.Convert(result);
                //if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sSHOPID) select dr).Count() > 0)
                //{
                //    cbo.SelectedValue = sPRODID;
                //}
                //else if (result.Rows.Count > 0)
                //{
                //    cbo.SelectedIndex = 0;
                //}
                //else if (result.Rows.Count == 0)
                //{
                //    cbo.SelectedItem = null;
                //}
                //});
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetcboCompleteCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_STAT";
                dr["ATTRIBUTE1"] = "STAT";
                dr["ATTRIBUTE2"] = null;
                dr["ATTRIBUTE3"] = null;
                dr["ATTRIBUTE4"] = null;
                dr["ATTRIBUTE5"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO_V2", "RQSTDT", "RSLTDT", RQSTDT);

                cboComplete.ItemsSource = DataTableConverter.Convert(dtResult);
                cboComplete.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SecboProdClassCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_STAT";
                dr["ATTRIBUTE1"] = "PROD";
                dr["ATTRIBUTE2"] = null;
                dr["ATTRIBUTE3"] = null;
                dr["ATTRIBUTE4"] = null;
                dr["ATTRIBUTE5"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO_V2", "RQSTDT", "RSLTDT", RQSTDT);

                cboProdClass.ItemsSource = DataTableConverter.Convert(dtResult);
                cboProdClass.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetcboSloc(C1ComboBox cbo, string strCU)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_SLOC_BY_PES", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    //2020.01.28
                    //if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sSHOPID) select dr).Count() > 0)
                    //{
                    //    cbo.SelectedValue = sPRODID;
                    //}
                    //else if (result.Rows.Count > 0)
                    //{
                    //    cbo.SelectedIndex = 0;
                    //}
                    //else if (result.Rows.Count == 0)
                    //{
                    //    cbo.SelectedItem = null;
                    //}
                    if (strCU != "C")
                    {
                        cbo.SelectedValue = sSLOCID;
                    }
                    else
                    {
                        cbo.SelectedIndex = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Combo

        private void GetMbom()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID.ToString();
                dr["PRODID"] = cboProduct.SelectedValue.ToString();
                dr["MTRLID"] = txtHoldProd.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MBOM_QTY", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count == 1)
                {
                    txtBOMQty.Text = dtResult.Rows[0]["PROC_INPUT_QTY"].ToString();
                }
                else
                {
                    txtBOMQty.Text = "1";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetUserList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERNAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERNAME"] = txtFreUserNameCr.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgUser, dtResult, FrameOperation);   
                }
                else
                {
                    txtFreUserID.Text = "";
                    txtFreUserNameCr.Text = "";
                    txtTeam.Text = "";
                    txtTeamName.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Lotmapping()
        {
            try
            {
                DataGrid01RowAdd(dgLotList);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        private void Detail()
        {
            try
            {
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SHOPID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("MTRLID", typeof(string));
                INDATA.Columns.Add("SLOCID", typeof(string));
                INDATA.Columns.Add("OCCR_YMD", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["PRODID"] = cboProduct.SelectedValue.ToString();
                dr["MTRLID"] = txtHoldProd.Text;
                dr["SLOCID"] = cboSloc.SelectedValue.ToString();
                dr["OCCR_YMD"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_HOLD_SUMMARY_DETL", "RQSTDT", "RSLTDT", INDATA);
                Util.GridSetData(dgLotList, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        private void Save()
        {
            try
            {
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result3) =>
                {
                    if (result3 == MessageBoxResult.OK)
                    {
                        DataSet dsInput = new DataSet();

                        if (sCU == "C")
                        {
                            DataTable INDATA = new DataTable();
                            INDATA.Columns.Add("SHOPID", typeof(string));
                            INDATA.Columns.Add("EQSGID", typeof(string));
                            INDATA.Columns.Add("PRODID", typeof(string));
                            INDATA.Columns.Add("MTRLID", typeof(string));
                            INDATA.Columns.Add("SLOCID", typeof(string));
                            INDATA.Columns.Add("OCCR_YMD", typeof(string));
                            INDATA.Columns.Add("OCCR_RSN_CNTT", typeof(string));
                            INDATA.Columns.Add("STCK_QTY", typeof(string));
                            INDATA.Columns.Add("PRDT_BAS_QTY", typeof(string));
                            INDATA.Columns.Add("CHARGE_USERID", typeof(string));
                            INDATA.Columns.Add("PROG_CNTT", typeof(string));
                            INDATA.Columns.Add("CMPL_SCHD_YMD", typeof(string));
                            INDATA.Columns.Add("PACK_HOLD_STCK_CMPL_CODE", typeof(string));
                            INDATA.Columns.Add("NOTE", typeof(string));
                            INDATA.Columns.Add("MTRLTYPE", typeof(string));
                            INDATA.Columns.Add("INSUSER", typeof(string));
                            INDATA.Columns.Add("UPDUSER", typeof(string));
                            //2020.01.06
                            INDATA.Columns.Add("TOTL_PRICE", typeof(int));

                            DataRow drINDATA = INDATA.NewRow();
                            drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID.ToString();
                            drINDATA["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                            drINDATA["PRODID"] = cboProduct.SelectedValue.ToString();
                            drINDATA["MTRLID"] = txtHoldProd.Text;
                            drINDATA["SLOCID"] = cboSloc.SelectedValue.ToString();
                            drINDATA["OCCR_YMD"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");

                            TextRange txtReason = new TextRange(RtxtReason.Document.ContentStart, RtxtReason.Document.ContentEnd);
                            if (txtReason.Text.Length > 1000)
                            {
                                Util.Alert("SFU4243");  //최대 1000자까지 가능합니다.
                                return;
                            }
                            else
                            {
                                drINDATA["OCCR_RSN_CNTT"] = txtReason.Text;
                                //drINDATA["OCCR_RSN_CNTT"] = txtReason.Text;
                            }

                            drINDATA["STCK_QTY"] = txtQty.Text;
                            drINDATA["PRDT_BAS_QTY"] = txtBOMQty.Text;
                            drINDATA["CHARGE_USERID"] = txtFreUserID.Text;

                            TextRange txtContent = new TextRange(RtxtContent.Document.ContentStart, RtxtContent.Document.ContentEnd);
                            //2020.01.06
                            //if (txtContent.Text.Length > 1000)
                            if (txtContent.Text.Length > 2000)
                            {
                                Util.Alert("SFU8144");  //최대 2000자까지 가능합니다.
                                return;
                            }
                            else
                            {
                                drINDATA["PROG_CNTT"] = txtContent.Text;
                                //drINDATA["PROG_CNTT"] = txtContent.Text;
                            }

                            drINDATA["CMPL_SCHD_YMD"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                            drINDATA["PACK_HOLD_STCK_CMPL_CODE"] = cboComplete.SelectedValue.ToString();
                            //2020.01.06
                            //drINDATA["NOTE"] = txtETC.Text;
                            TextRange RtxtETC = new TextRange(txtETC.Document.ContentStart, txtETC.Document.ContentEnd);
                            if (RtxtETC.Text.Length > 1000)
                            {
                                Util.Alert("SFU4243");  //최대 1000자까지 가능합니다.
                                return;
                            }
                            else
                            {
                                drINDATA["NOTE"] = RtxtETC.Text;
                            }
                            drINDATA["MTRLTYPE"] = cboProdClass.SelectedValue.ToString();
                            drINDATA["INSUSER"] = LoginInfo.USERID.ToString();
                            drINDATA["UPDUSER"] = LoginInfo.USERID.ToString();

                            //2020.01.06
                            drINDATA["TOTL_PRICE"] = Int32.Parse(txtPrice.Text);

                            INDATA.Rows.Add(drINDATA);
                            dsInput.Tables.Add(INDATA);

                            DataTable INLOT = new DataTable();
                            INLOT.TableName = "INLOT";
                            INLOT.Columns.Add("LOTID", typeof(string));

                            DataRow drINLOT = null;
                            if (dgLotList.Rows.Count > 0)
                            {
                                for (int i = 0; i < dgLotList.Rows.Count; i++)
                                {
                                    drINLOT = INLOT.NewRow();
                                    drINLOT["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID"));

                                    INLOT.Rows.Add(drINLOT);
                                }
                            }
                            dsInput.Tables.Add(INLOT);

                            dsInput = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_STOCK", "INDATA,INLOT", "OUTDATA", dsInput, null);
                        }
                        else
                        {
                            //DA_PRD_UPD_STOCK_HOLD_SUMMARY
                            DataTable INDATA = new DataTable();
                            INDATA.Columns.Add("SHOPID", typeof(string));
                            INDATA.Columns.Add("EQSGID", typeof(string));
                            INDATA.Columns.Add("PRODID", typeof(string));
                            INDATA.Columns.Add("MTRLID", typeof(string));
                            INDATA.Columns.Add("SLOCID", typeof(string));
                            INDATA.Columns.Add("OCCR_YMD", typeof(string));
                            INDATA.Columns.Add("OCCR_RSN_CNTT", typeof(string));
                            INDATA.Columns.Add("STCK_QTY", typeof(string));
                            INDATA.Columns.Add("PRDT_BAS_QTY", typeof(string));
                            INDATA.Columns.Add("CHARGE_USERID", typeof(string));
                            INDATA.Columns.Add("PROG_CNTT", typeof(string));
                            INDATA.Columns.Add("CMPL_SCHD_YMD", typeof(string));
                            INDATA.Columns.Add("PACK_HOLD_STCK_CMPL_CODE", typeof(string));
                            INDATA.Columns.Add("NOTE", typeof(string));
                            INDATA.Columns.Add("MTRLTYPE", typeof(string));
                            INDATA.Columns.Add("INSUSER", typeof(string));
                            INDATA.Columns.Add("UPDUSER", typeof(string));
                            //2020.01.06
                            INDATA.Columns.Add("TOTL_PRICE", typeof(int));

                            DataRow drINDATA = INDATA.NewRow();
                            drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID.ToString();
                            drINDATA["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                            drINDATA["PRODID"] = cboProduct.SelectedValue.ToString();
                            drINDATA["MTRLID"] = txtHoldProd.Text;
                            drINDATA["SLOCID"] = cboSloc.SelectedValue.ToString();
                            drINDATA["OCCR_YMD"] = dtpWorkStartDay.SelectedDateTime.ToString("yyyyMMdd");
                            //drINDATA["OCCR_RSN_CNTT"] = txtReason.Text;
                            TextRange txtReason = new TextRange(RtxtReason.Document.ContentStart, RtxtReason.Document.ContentEnd);
                            if (txtReason.Text.Length > 1000)
                            {
                                Util.Alert("SFU4243");  //최대 1000자까지 가능합니다.
                                return;
                            }
                            else
                            {
                                drINDATA["OCCR_RSN_CNTT"] = txtReason.Text;
                                //drINDATA["OCCR_RSN_CNTT"] = txtReason.Text;
                            }

                            drINDATA["STCK_QTY"] = txtQty.Text;
                            drINDATA["PRDT_BAS_QTY"] = txtBOMQty.Text;
                            drINDATA["CHARGE_USERID"] = txtFreUserID.Text;
                            
                            TextRange txtContent = new TextRange(RtxtContent.Document.ContentStart, RtxtContent.Document.ContentEnd);
                            //2020.01.06
                            //if (txtContent.Text.Length > 1000)
                            if (txtContent.Text.Length > 2000)
                            {
                                Util.Alert("SFU8144");  //최대 2000자까지 가능합니다.
                                return;
                            }
                            else
                            {
                                //2020.01.06
                                //drINDATA["PROG_CNTT"] = new TextRange(RtxtContent.Document.ContentStart, RtxtContent.Document.ContentEnd);
                                drINDATA["PROG_CNTT"] = txtContent.Text;
                            }

                            drINDATA["CMPL_SCHD_YMD"] = dtpWorkEndDay.SelectedDateTime.ToString("yyyyMMdd");
                            drINDATA["PACK_HOLD_STCK_CMPL_CODE"] = cboComplete.SelectedValue.ToString();
                            //2020.01.06
                            //drINDATA["NOTE"] = txtETC.Text;
                            TextRange RtxtETC = new TextRange(txtETC.Document.ContentStart, txtETC.Document.ContentEnd);
                            if (RtxtETC.Text.Length > 1000)
                            {
                                Util.Alert("SFU4243");  //최대 1000자까지 가능합니다.
                                return;
                            }
                            else
                            {
                                drINDATA["NOTE"] = RtxtETC.Text;
                            }

                            drINDATA["MTRLTYPE"] = cboProdClass.SelectedValue.ToString();
                            drINDATA["INSUSER"] = LoginInfo.USERID.ToString();
                            drINDATA["UPDUSER"] = LoginInfo.USERID.ToString();

                            //2020.01.06
                            drINDATA["TOTL_PRICE"] = Int32.Parse(txtPrice.Text);

                            INDATA.Rows.Add(drINDATA);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCK_HOLD_SUMMARY", "INDATA", "OUTDATA", INDATA);
                        }

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("9003"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result4) =>
                        {
                            if (result4 == MessageBoxResult.OK)
                            {
                                this.Close();
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        #endregion Method       

        //2020.01.06
        private void txtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (ChkAvailabilityDouble(txtPrice.Text))
                {

                }
                else
                {
                    Util.Alert("SFU2877");  //숫자만 입력가능합니다.
                    txtPrice.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ChkAvailabilityDouble(string sInput)
        {
            try
            {
                double iTemp = 0;
                bool bAvailabilityDouble;

                bAvailabilityDouble = double.TryParse(sInput, out iTemp);

                if (bAvailabilityDouble)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
    }
}