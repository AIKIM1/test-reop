/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 생산계획 등록
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.07.25  김태우    : DAM 믹서(E0430) 추가

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_001_LOAD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string sSHOPID = string.Empty;
        private string sAREAID = string.Empty;
        private string sLINEID = string.Empty;
        private string sPROCID = string.Empty;
        bool InputRow = false;
        //
        int addRows;

        public COM001_001_LOAD()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            sSHOPID = Util.NVC(tmps[0]);
            sAREAID = Util.NVC(tmps[1]);
            sLINEID = Util.NVC(tmps[2]);
            sPROCID = Util.NVC(tmps[3]);

            InitCombo();
        }
        #endregion

        #region Event
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            Set_Combo_Shop(cboShop);
            Set_Combo_Area(cboArea);
            //Set_Combo_EquipmentSegmant(cboEquipmentSegment);
            //Set_Combo_Process(cboProcess);
            Set_Combo_WOType(cboDEMAND_TYPE);
            Set_PopUp_ProductID();

            //COMMCODE
            string[] sFilter = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMKType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
        }        
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (sender != null)
                {
                    Set_PopUp_ProductID();

                    switch (Util.NVC(cboProcess.SelectedValue))
                    {
                        case "E0400":              
                        case "E0410":
                        case "E0420":
                        case "E0430":
                            dgExcleload.Columns["PRODID"].Visibility = Visibility.Collapsed;
                            dgExcleload.Columns["PRODID2"].Visibility = Visibility.Visible;
                            break;

                        default:
                            dgExcleload.Columns["PRODID"].Visibility = Visibility.Visible;
                            dgExcleload.Columns["PRODID2"].Visibility = Visibility.Collapsed;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    LoadExcelHelper.LoadExcelData(dgExcleload, stream, 0, 1);
                }
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isValid())
                return;
            else
            {
                SaveData();
            }
            
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cboProcess.SelectedValue == null)
            {
                Util.Alert("SFU1459");  //공정을 선택하세요.
                return;
            }
            DataGrid01RowAdd(dgExcleload);
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGrid01RowDelete(dgExcleload);
        }
        private void rowCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    return;
                }
                InputRow = true;
            }
        }
        private void rowCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!InputRow)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("숫자만 입력가능합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                    {
                        rowCount.Focus();
                        rowCount.Clear();
                    });
                    return;
                }
                else
                {
                    btnAdd_Click(sender, e);
                }
            }
        }
        #endregion

        #region Mehod
        private void DataGrid01RowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 여러건 추가 시 안되는 부분 확인
                DataTable dt = new DataTable();
                if (rowCount.Text != "")
                {
                    addRows = int.Parse(rowCount.Text);
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
                if (dg.ItemsSource != null)
                {
                    if (this.rowCount.Text == "")
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dr2["CHK"] = true;
                        dr2["STRT_DTTM"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                        dr2["END_DTTM"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                    else
                    {
                        for (int i = 0; i < addRows; i++)
                        {
                            dt = DataTableConverter.Convert(dg.ItemsSource);
                            DataRow dr2 = dt.NewRow();
                            dr2["CHK"] = true;
                            dr2["STRT_DTTM"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                            dr2["END_DTTM"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                            dt.Rows.Add(dr2);
                            dg.BeginEdit();
                            dg.ItemsSource = DataTableConverter.Convert(dt);
                            dg.EndEdit();
                        }
                    }
                }
                else
                {
                    if (this.rowCount.Text == "")
                    {
                        DataRow dr = dt.NewRow();
                        dr["CHK"] = true;
                        dr["STRT_DTTM"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                        dr["END_DTTM"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                    else
                    {
                        for (int i = 0; i < addRows; i++)
                        {
                            DataRow dr = dt.NewRow();
                            dr["CHK"] = true;
                            dr["STRT_DTTM"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                            dr["END_DTTM"] = string.Format("{0:yyyy-MM-dd}", DateTime.Now.ToString("yyyy-MM-dd"));
                            dt.Rows.Add(dr);
                            dg.BeginEdit();
                            dg.ItemsSource = DataTableConverter.Convert(dt);
                            dg.EndEdit();
                        }
                    }
                    //SetGridCboItem(dg.Columns["CBO_CODE"]);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void DataGrid01RowDelete(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.Rows.Count > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;

                    for (int i = dt.Rows.Count; 0 <= i; i--)
                    {
                        if (_Util.GetDataGridCheckValue(dg, "CHK", i))
                        {
                            dt.Rows[i].Delete();
                            dg.BeginEdit();
                            dg.ItemsSource = DataTableConverter.Convert(dt);
                            dg.EndEdit();
                        }
                    }
                }
                else
                {
                    //삭제할 항목이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1597"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
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

                new ClientProxy().ExecuteService("DA_BAS_SEL_SHOP_BY_USERID_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sSHOPID) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = sSHOPID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboShop_SelectedItemChanged(cbo, null);
                });
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
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = sSHOPID;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sAREAID) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = sAREAID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboArea_SelectedItemChanged(cbo, null);
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_EquipmentSegmant(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = sAREAID; //LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }
                    DataRow dRow = result.NewRow();
                    //dRow["CBO_NAME"] = "-ALL-";
                    //dRow["CBO_CODE"] = "";
                    //result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sLINEID) select dr).Count() > 0) //LoginInfo.CFG_EQSG_ID
                    {
                        cbo.SelectedValue = sLINEID; //LoginInfo.CFG_EQSG_ID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboEquipmentSegment_SelectedItemChanged(cbo, null);
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_Process(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = sLINEID; //LoginInfo.CFG_EQSG_ID;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_FP_PROCESSATTR_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    cbo.ItemsSource = DataTableConverter.Convert(result);

                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(sPROCID) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = sPROCID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_WOType(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CMCDTYPE"] = "DEMAND_TYPE";
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }
                    DataRow dRow = result.NewRow();
                    //dRow["CBO_NAME"] = "-ALL-";
                    //dRow["CBO_CODE"] = "";
                    //result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    cbo.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_PopUp_ProductID()
        {
            try
            {
                // G482 제거 [2019.08.13]
                //if (!LoginInfo.CFG_SHOP_ID.Equals("G482"))
                //    return;

                if (!getMixerPlanShop(LoginInfo.CFG_SHOP_ID))
                    return;

                Util.gridClear(dgExcleload);

                const string bizRuleName = "DA_BAS_SEL_PRODUCT_PRODID_POPUP";
                string selectedValueText = (string)((PopupFindDataColumn)dgExcleload.Columns["PRODID2"]).SelectedValuePath;

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("PROCID", typeof(string));

                string ProcessID = string.Empty;

                switch (Util.NVC(cboProcess.SelectedValue))
                {
                    case "E0400":
                        ProcessID = "BINDERSOLUTION";
                        break;

                    case "E0410":
                        ProcessID = "CMC";
                        break;

                    case "E0420":
                        ProcessID = "AD";
                        break;
                    case "E0430":
                        ProcessID = "DAM";
                        break;
                }

                DataRow dr = inDataTable.NewRow();
                dr["PROCID"] = ProcessID;
                inDataTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
                //DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText });

                PopupFindDataColumn column = dgExcleload.Columns["PRODID2"] as PopupFindDataColumn;
                column.AddMemberPath = "";
                column.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboShop.SelectedIndex > -1)
                {
                    sSHOPID = Convert.ToString(cboShop.SelectedValue);
                    Set_Combo_Area(cboArea);
                }
                else
                {
                    sSHOPID = string.Empty;
                }
            }));
        }
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    sAREAID = Convert.ToString(cboArea.SelectedValue);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    //Set_Combo_Process(cboProcess);
                }
                else
                {
                    sAREAID = string.Empty;
                }
            }));
        }
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    sLINEID = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    Set_Combo_Process(cboProcess);
                }
                else
                {
                    sLINEID = string.Empty;
                }
            }));
        }
        private bool isValid()
        {
            bool bRet = false;
            for (int _iRow = 0; _iRow < dgExcleload.Rows.Count; _iRow++)
            {
                if (cboProcess.SelectedValue == null || string.IsNullOrEmpty(cboProcess.SelectedValue.ToString()))
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return bRet;
                }
                if (DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "PRODID") == null || DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "PRODID").ToString() == "")
                {
                    if (DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "PRODID2") == null || DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "PRODID2").ToString() == "")
                    {
                        Util.MessageValidation("SFU2949");
                        return bRet;
                    }                    
                }
                if (DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "INPUT_QTY") == null || DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "INPUT_QTY").ToString() == "0")
                {
                    Util.MessageValidation("SFU2856"); // 계획수량을 입력하세요.
                    return bRet;
                }
                if (DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "STRT_DTTM"))) > DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "END_DTTM"))))
                {
                    Util.MessageValidation("SFU2858"); // 계획종료일자가 시작일자보다 빠릅니다.
                    return bRet;
                }
                if (DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")) > DateTime.Parse(Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "STRT_DTTM"))))
                {
                    Util.MessageValidation("SFU2857"); // 계획시작일자가 오늘이전입니다.
                    return bRet;
                }
                if (Double.Parse(Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "INPUT_QTY"))) < 0)
                {
                    Util.MessageValidation("SFU2855"); // 계획수량은 정수만 입력가능합니다.
                    return bRet;
                }
                if (cboMKType.SelectedValue == null || string.IsNullOrEmpty(cboMKType.SelectedValue.ToString()) || cboMKType.SelectedValue.ToString().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU4371");  //시장유형을 선택하세요.
                    return bRet;
                }

            }
            bRet = true;
            return bRet;
        }
        private void SaveData()
        {
            try
            {
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        //IndataTable.Columns.Add("WOID", typeof(string));
                        IndataTable.Columns.Add("SHOPID", typeof(string));
                        IndataTable.Columns.Add("AREAID", typeof(string));
                        IndataTable.Columns.Add("EQSGID", typeof(string));
                        IndataTable.Columns.Add("PROCID", typeof(string));
                        IndataTable.Columns.Add("PRODID", typeof(string));
                        IndataTable.Columns.Add("DEMAND_TYPE", typeof(string));
                        IndataTable.Columns.Add("STRT_DTTM", typeof(DateTime));
                        IndataTable.Columns.Add("END_DTTM", typeof(DateTime));
                        IndataTable.Columns.Add("INPUT_QTY", typeof(decimal));
                        IndataTable.Columns.Add("INSUSER", typeof(string));
                        IndataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                        for (int _iRow = 0; _iRow < dgExcleload.Rows.Count; _iRow++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow Indata = IndataTable.NewRow();
                                //Indata["WOID"] = Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "WOID"));
                                Indata["SHOPID"] = Util.NVC(cboShop.SelectedValue);
                                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                                Indata["DEMAND_TYPE"] = Util.NVC(cboDEMAND_TYPE.SelectedValue);
                                Indata["STRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "STRT_DTTM"));
                                Indata["END_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "END_DTTM"));
                                Indata["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "INPUT_QTY"));
                                Indata["INSUSER"] = LoginInfo.USERID;
                                Indata["MKT_TYPE_CODE"] = Util.NVC(cboMKType.SelectedValue);

                                if (Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "PRODID")).Equals(""))
                                {
                                    Indata["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "PRODID2"));
                                }
                                else
                                {
                                    Indata["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgExcleload.Rows[_iRow].DataItem, "PRODID"));
                                }
                                
                                IndataTable.Rows.Add(Indata);
                            }
                        }
                        if(IndataTable.Rows.Count !=0 )
                        {
                            new ClientProxy().ExecuteService("BR_PRD_REG_TB_SFC_FP_DETL_PLAN", "INDATA", null, IndataTable, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                    return;
                                }

                                Util.AlertInfo("SFU1275");  //정상처리되었습니다.
                                this.dgExcleload.ItemsSource = null;
                            });
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
            
        }
        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = "DEMAND_TYPE";
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "INDATA", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
        }

        private bool getMixerPlanShop(string sSHOPID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MES_MIXERPLAN_SHOP";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach (DataRow row in dtResult.Rows)
                {
                    if (string.Equals(Util.NVC(row["CBO_CODE"]), sSHOPID))
                        return true;
                }
            }
            return false;
        }
        #endregion        
    }
}
