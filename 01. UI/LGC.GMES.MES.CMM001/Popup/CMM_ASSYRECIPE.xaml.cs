/*******************************
 Created Date :
      Creator :
   Decription : Recipe No.
--------------------------------
 [Change History]
  2016.08.19  : Initial Created.
********************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// ELEC001_006_LOTEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSYRECIPE : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _RecipeNo = string.Empty;
        private string _Laneqty = string.Empty;
        private string _PtnQty = string.Empty;
        private string _PRODID = string.Empty;
        private string _PROCID = string.Empty;
        private string _AREAID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _LOTID = string.Empty;
        private string _VERSION_CHK = string.Empty;
        Util _Util = new Util();
        public string _ReturnRecipeNo
        {
            get { return _RecipeNo; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_ASSYRECIPE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            _PRODID = Util.NVC(tmps[0]);
            _PROCID = Util.NVC(tmps[1]);
            _AREAID = Util.NVC(tmps[2]);
            _EQPTID = Util.NVC(tmps[3]);
            _LOTID = Util.NVC(tmps[4]);

            if (tmps.Length > 5)
                _VERSION_CHK = Util.NVC(tmps[5]);

                GetRecipeNo(); 
        }

        //private void Gplm_Gmes_CoterVersion_Check()
        //{
        //    if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
        //    {
        //        string gpCTver = string.Empty;
        //        DataTable gpcode = new DataTable();
        //        gpcode.Columns.Add("LANGID", typeof(string));
        //        gpcode.Columns.Add("CMCDTYPE", typeof(string));
        //        gpcode.Columns.Add("CMCODE", typeof(string));
        //        gpcode.Columns.Add("ATTRIBUTE1", typeof(string));

        //        DataRow gprow = gpcode.NewRow();

        //        gprow["LANGID"] = LoginInfo.LANGID;
        //        gprow["CMCDTYPE"] = "GPLM_VER_CHK";
        //        gprow["CMCODE"] = LoginInfo.CFG_AREA_ID;
        //        gprow["ATTRIBUTE1"] = _PROCID;


        //        gpcode.Rows.Add(gprow);

        //        DataTable gpcomResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "RQSTDT", "RSLTDT", gpcode);

        //        if (gpcomResult.Rows.Count > 0)
        //        {

        //            DataTable IndataTable = new DataTable();
        //            IndataTable.Columns.Add("AREAID", typeof(string));
        //            IndataTable.Columns.Add("PRODID", typeof(string));

        //            DataRow Indata = IndataTable.NewRow();
        //            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
        //            Indata["PRODID"] = _PRODID;
        //            IndataTable.Rows.Add(Indata);

        //            DataTable dtGplmverNo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_GPLM_COTER_MAX_VERSION", "RQSTDT", "RSLTDT", IndataTable);

        //            if (dtGplmverNo.Rows.Count > 0) //GPLM VERSION 정보가 없을때
        //            {
        //                gpCTver = dtGplmverNo.Rows[0]["PROD_VER_CODE"].ToString();

        //                DataTable dtRecipeNo = ((DataView)dgRecipeNo.ItemsSource).ToTable(); // 팝업 체크한 version                    

        //                string chkCTver = dtRecipeNo.AsEnumerable()
        //                .Where(a => a["CHK"].ToString() == "1" || a["CHK"].ToString() == "True")
        //                .Select(a => a["PROD_VER_CODE"]).First().ToString();

        //                if (chkCTver.ToString().IndexOf("Z") == -1)
        //                {
        //                    if (Int32.Parse(chkCTver) != Int32.Parse(gpCTver))
        //                    {
        //                        Util.MessageInfo("SFU8136");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //GPLM에 버전이 안내려올경우 협의 필요
        //            }

        //        }
        //    }
        //}

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            int idx = -1;
            for (int i = 0; i < dgRecipeNo.GetRowCount(); i++)
            {
                if (Util.NVC_Int(DataTableConverter.GetValue(dgRecipeNo.Rows[i].DataItem, "CHK")) == 1)
                {
                    if(DataTableConverter.GetValue(dgRecipeNo.Rows[i].DataItem, "PROD_VER_CODE").ToString() == "선택취소")
                    {
                        _RecipeNo = "";
                    }
                    idx = i;
                    break;
                }

            }

            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");    //선택된 항목이 없습니다.
                return;
            }
            //Gplm_Gmes_CoterVersion_Check();



            this.DialogResult = MessageBoxResult.OK;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _RecipeNo = string.Empty;
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private void GetRecipeNo()
        {
            try
            {
                Util.gridClear(dgRecipeNo);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PRODID"] = _PRODID;
                Indata["AREAID"] = _AREAID;
                Indata["EQPTID"] = _EQPTID;
                Indata["LOTID"] = _LOTID;
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE", "INDATA", "RSLTDT", IndataTable);
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_V01", "INDATA", "RSLTDT", IndataTable);


                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    if (dtMain.Rows.Count > 0)
                    {
                        DataRow drIns = dtMain.NewRow();
                        drIns["PROD_VER_CODE"] = "선택취소";
                        dtMain.Rows.InsertAt(drIns, 0);
                    }
                    else
                    {
                        if (dtMain == null || dtMain.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU4909");  // 해당 제품은 버전 정보가 없습니다.
                            return;
                        }
                    }
                }

                dgRecipeNo.ItemsSource = DataTableConverter.Convert(dtMain);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgRecipeNoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            //if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            if ((bool)rb.IsChecked)
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;

                if (string.Equals(_VERSION_CHK, "Y")
                    //&& !string.Equals(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "LANE_QTY_CNFM_FLAG"), "Y")
                   )
                {
                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, "0");
                    rb.IsChecked = false;
                    return;
                }

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, "1");
                        rb.IsChecked = true;
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, "0");
                    }
                }

                dgRecipeNo.SelectedIndex = idx;


                    _RecipeNo = DataTableConverter.GetValue(dgRecipeNo.Rows[idx].DataItem, "PROD_VER_CODE").ToString();


                // 변환 보고서 지정
                if (docVersion != null)
                {
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.Convert(dg.ItemsSource).Rows[idx]["CHANGE_HIST"])))
                    {
                        docVersion.NavigateToString(Util.NVC(DataTableConverter.Convert(dg.ItemsSource).Rows[idx]["CHANGE_HIST"]));
                        docVersion.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        docVersion.Navigate("about:blank");
                        docVersion.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void docVersion_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            string script = "document.body.style.overflow ='hidden'";
            WebBrowser wb = (WebBrowser)sender;
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
        }

        private void dgRecipeNo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && string.Equals(_VERSION_CHK, "Y"))
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if ((string.Equals(e.Cell.Column.Name, "CHK") || string.Equals(e.Cell.Column.Name, "PROD_VER_CODE") 
                            //|| string.Equals(e.Cell.Column.Name, "LANE_QTY")) && !string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LANE_QTY_CNFM_FLAG"), "Y"
                            ))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        }
                    }
                }));
            }
        }

        private void dgRecipeNo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null && string.Equals(_VERSION_CHK, "Y"))
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));
            }
        }
        #endregion
    }
}