/*************************************************************************************
 Created Date : 2016.06.16
      Creator :
   Decription :
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.05.04  손우석    : Excel 이벤트 처리
  2022.02.24  정용석    : BOX 출고상태 추가
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_017 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtSearchResult;
        DataTable dtGridChek;
        public PACK001_017()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
            SetComboBox();

            gdContent.ColumnDefinitions[0].Width = new GridLength(0);
            gdContent.ColumnDefinitions[1].Width = new GridLength(0);
            gdContentTitle.ColumnDefinitions[2].Width = new GridLength(8);

            tbBoxListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //string sPallet = txtPalletId.Text.Length > 0 ? txtPalletId.Text : null;
            //string sBox = txtBoxId.Text.Length > 0 ? txtBoxId.Text : null;
            //string sLot = txtLotId.Text.Length > 0 ? txtLotId.Text : null;
            getBoxList(null);
        }

        private void dgSearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResult.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "LOTID")
                        {
                            this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                        }
                        else if (cell.Column.Name == "BOXID" || cell.Column.Name == "PALLETID")
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
                        else if(cell.Column.Name == "OQC_INSP_REQ_ID")
                        {
                            //PACK001_026_OQC_INFO popup = new PACK001_026_OQC_INFO();
                            //popup.FrameOperation = this.FrameOperation;

                            //if (popup != null)
                            //{
                            //    DataTable dtData = new DataTable();
                            //    dtData.Columns.Add("BOXID", typeof(string));

                            //    DataRow newRow = null;
                            //    newRow = dtData.NewRow();
                            //    newRow["BOXID"] = cell.Text;

                            //    dtData.Rows.Add(newRow);

                            //    //========================================================================
                            //    object[] Parameters = new object[1];
                            //    Parameters[0] = dtData;
                            //    C1WindowExtension.SetParameters(popup, Parameters);
                            //    //========================================================================

                            //    popup.ShowModal();
                            //    popup.CenterOnScreen();
                            //}
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "JUDG_VALUE_NAME" )
                    {
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JUDG_VALUE"));

                        if (sCheck.Equals("N") || sCheck.Equals("F"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (sCheck.Equals("Y") || sCheck.Equals("P"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                    }
                    else if(e.Cell.Column.Name == "DETL_JUDG_VALUE_NAME")
                    {
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DETL_JUDG_VALUE"));

                        if (sCheck.Equals("N") || sCheck.Equals("F"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (sCheck.Equals("Y") || sCheck.Equals("P"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                    }
                    else if (e.Cell.Column.Name == "PALLETID" || e.Cell.Column.Name == "BOXID")
                    {
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JUDG_VALUE"));

                        if (sCheck.Equals("N") || sCheck.Equals("F"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (sCheck.Equals("Y") || sCheck.Equals("P"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                    else if (e.Cell.Column.Name == "LOTID")
                    {
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DETL_JUDG_VALUE"));

                        if (sCheck.Equals("N") || sCheck.Equals("F"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (sCheck.Equals("Y") || sCheck.Equals("P"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
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
                Util.Alert(ex.ToString());
            }


            //link 색변경
            if (e.Cell.Column.Name.Equals("JUDG_VALUE"))
            {
                //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
            }
        }

        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnRight_Checked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[0].Width = new GridLength(300);
                gdContent.ColumnDefinitions[1].Width = new GridLength(8);
                gdContentTitle.ColumnDefinitions[2].Width = new GridLength(290);
            }

        }

        private void btnRight_Unchecked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[0].Width = new GridLength(0);
                gdContent.ColumnDefinitions[1].Width = new GridLength(0);
                gdContentTitle.ColumnDefinitions[2].Width = new GridLength(8);
            }
        }

        private void btnSearch_id_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(richTextIDList.Document.ContentStart, richTextIDList.Document.ContentEnd);

                if (textRange.Text.Length > 0)
                {
                    string[] separators = new string[] { "\r\n" };
                    string[] IDList = textRange.Text.Split(separators, StringSplitOptions.None);

                    if (IDList.Length <= 0)
                    {
                        return;
                    }

                    this.getBoxList(string.Join(",", IDList));
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtIDInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter)
                {
                    return;
                }

                if (txtIDInput.Text.Length <= 0)
                {
                    PackCommon.ClearRichTextBox(ref this.richTextIDList);
                    return;
                }

                TextRange textRange = new TextRange(richTextIDList.Document.ContentStart, richTextIDList.Document.ContentEnd);
                if (string.IsNullOrEmpty(textRange.Text))
                {
                    richTextIDList.AppendText(this.txtIDInput.Text + Environment.NewLine);
                }
                else
                {
                    // 중복 Check
                    string[] separators = new string[] { "\r\n" };
                    string[] arrIDList = textRange.Text.Split(separators, StringSplitOptions.None);
                    if (!Array.Exists(arrIDList, x => x.Equals(this.txtIDInput.Text)))
                    {
                        richTextIDList.AppendText(Environment.NewLine + this.txtIDInput.Text);
                    }
                }
                this.txtIDInput.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {

        }

        private void txtIDInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.V) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.SearchClipboardData();
                e.Handled = true;
            }
        }
        #endregion

        #region Mehod
        private void SetComboBox()
        {
            try
            {
                dateSearchType_Cbo();

                setIDType_Cbo();

                CommonCombo _combo = new CommonCombo();

                //동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboModel };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sFilter: sFilter);

                //라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                C1ComboBox[] cboLineChild = { cboModel };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);

                //모델
                C1ComboBox[] cboProductModelParent = { cboAreaByAreaType, cboEquipmentSegment };
                _combo.SetCombo(cboModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, sCase: "PRJ_MODEL");

                setComboBox_SHIPTO(LoginInfo.CFG_AREA_ID);
            }
            catch(Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dateSearchType_Cbo()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("PALLET구성일"), "PALLET" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("BOX구성일"), "BOX" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("출하예정일"), "OQC" };
            dtResult.Rows.Add(newRow);

            cboPalletConfig.ItemsSource = DataTableConverter.Convert(dtResult);

        }

        private void setIDType_Cbo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("ID", typeof(string));
                dt.Columns.Add("NAME", typeof(string));

                DataRow dr = dt.NewRow();
                dr = dt.NewRow();
                dr.ItemArray = new object[] { 1, ObjectDic.Instance.GetObjectName("PALLET") };
                dt.Rows.Add(dr);
                dr = dt.NewRow();
                dr.ItemArray = new object[] { 2, ObjectDic.Instance.GetObjectName("BOX") };
                dt.Rows.Add(dr);
                dr = dt.NewRow();
                dr.ItemArray = new object[] { 3, ObjectDic.Instance.GetObjectName("LOT") };
                dt.Rows.Add(dr);
                PackCommon.SetC1ComboBox(dt, this.cboID_Type);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getBoxList(string sID_List)
        {
            try
            {
                bool bDateSearch = true;
                bDateSearch = (sID_List != null) ? false : true;

                DataTable dtRQSTDT = new DataTable("RQSTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PRJ_NAME", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE_PALLET", typeof(string));
                dtRQSTDT.Columns.Add("TODATE_PALLET", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE_BOX", typeof(string));
                dtRQSTDT.Columns.Add("TODATE_BOX", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE_OQC", typeof(string));
                dtRQSTDT.Columns.Add("TODATE_OQC", typeof(string));
                dtRQSTDT.Columns.Add("SHIPTO", typeof(string));
                dtRQSTDT.Columns.Add("ID_LIST", typeof(string));
                dtRQSTDT.Columns.Add("ID_TYPE", typeof(Int16));

                DataRow dr = dtRQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = !bDateSearch || Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : Util.NVC(cboAreaByAreaType.SelectedValue);
                dr["EQSGID"] = !bDateSearch || Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PRJ_NAME"] = !bDateSearch || Util.NVC(cboModel.SelectedValue) =="" ? null : Util.NVC(cboModel.SelectedValue);

                dr["FROMDATE_PALLET"] = getSearchDate(bDateSearch, dtpDateFrom, "PALLET", Util.NVC(cboPalletConfig.SelectedValue));
                dr["TODATE_PALLET"] = getSearchDate(bDateSearch, dtpDateTo, "PALLET", Util.NVC(cboPalletConfig.SelectedValue));
                dr["FROMDATE_BOX"] = getSearchDate(bDateSearch, dtpDateFrom, "BOX", Util.NVC(cboPalletConfig.SelectedValue));
                dr["TODATE_BOX"] = getSearchDate(bDateSearch, dtpDateTo, "BOX", Util.NVC(cboPalletConfig.SelectedValue));
                dr["FROMDATE_OQC"] = getSearchDate(bDateSearch, dtpDateFrom, "OQC", Util.NVC(cboPalletConfig.SelectedValue));
                dr["TODATE_OQC"] = getSearchDate(bDateSearch, dtpDateTo, "OQC", Util.NVC(cboPalletConfig.SelectedValue));

                dr["SHIPTO"] = !bDateSearch || Util.NVC(cboOutPlace.SelectedValue) == "" ? null : Util.NVC(cboOutPlace.SelectedValue);
                dr["ID_LIST"] = sID_List;
                dr["ID_TYPE"] = bDateSearch ? 0 : Convert.ToInt32(Util.NVC(this.cboID_Type.SelectedValue));
                dtRQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_PALLET_HIST_PACK", "RQSTDT", "RSLTDT", dtRQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_PALLET_HIST_PACK", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    //dgSearchResult.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbBoxListCount, Util.NVC(dgSearchResult.Rows.Count));
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private string getSearchDate(bool bDateSearch, ControlsLibrary.LGCDatePicker cbo ,string sSearchType , string sSelectedType)
        {
            string sDate = null;
            if (bDateSearch)
            {
                if(sSearchType == sSelectedType)
                {
                    sDate = cbo.SelectedDateTime.ToString("yyyyMMdd");
                }
            }
            return sDate;
        }

        private void setComboBox_SHIPTO(string sAREAID)
        {
            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SHIP_TYPE_CODE", typeof(string));
                dtIndata.Columns.Add("FROM_AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["SHIP_TYPE_CODE"] = Ship_Type.PACK;
                drIndata["FROM_AREAID"] = sAREAID;
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_SHIPTO_BY_FROMAREAID_CBO", "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_SHIPTO_BY_FROMAREAID_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    DataRow dr = dtResult.NewRow();
                    dr["CBO_NAME"] = "-ALL-";
                    dr["CBO_CODE"] = "";
                    dtResult.Rows.InsertAt(dr, 0);

                    cboOutPlace.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboOutPlace.SelectedIndex = 0;
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // 조회 - Group ID 또는 LOTID 붙여넣고 조회
        private void SearchClipboardData()
        {
            try
            {
                PackCommon.ClearRichTextBox(ref this.richTextIDList);

                this.txtIDInput.Text = string.Empty;
                string[] separators = new string[] { "\r\n" };
                string clipboardText = Clipboard.GetText();
                string[] arrIDList = clipboardText.Split(separators, StringSplitOptions.None).Distinct().ToArray();

                if (arrIDList.Count() > 100)
                {
                    Util.MessageValidation("SFU3695");   // 최대 100개 까지 가능합니다.
                    return;
                }

                for (int i = 0; i < arrIDList.Length; i++)
                {
                    if (string.IsNullOrEmpty(arrIDList[i]))
                    {
                        Util.MessageInfo("SFU1190", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                return;
                            }
                        });
                    }
                }

                this.richTextIDList.AppendText(string.Join(Environment.NewLine, arrIDList));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
