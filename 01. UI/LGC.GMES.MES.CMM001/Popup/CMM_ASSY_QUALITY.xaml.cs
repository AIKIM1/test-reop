/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_QUALITY : C1Window, IWorkArea
    {
        private string _Proc = string.Empty;
        private string _Eqpt = string.Empty;
        private string _LotId = string.Empty;
        private string _WipSeq = string.Empty;

        #region Declaration & Constructor 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_QUALITY()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 4)
            {
                _Proc = tmps[0].ToString();
                _Eqpt = tmps[1].ToString();
                _LotId = tmps[2].ToString();
                _WipSeq = tmps[3].ToString();
            }
        }

        private void btnQualitySearch_Click(object sender, RoutedEventArgs e)
        {
            GetQuality();
        }

        private void btnQualityAdd_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                //    "추가하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU2965", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            AddQuality();
                        }
                    });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            SaveQuality();
        }

        private void btnQualityInfoSearch_Click(object sender, RoutedEventArgs e)
        {
            GetQualityList();
        }
        #endregion

        #region Mehod
        #region [조회]
        private void GetQuality()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _Proc;
                dr["LOTID"] = _LotId;
                dr["WIPSEQ"] = _WipSeq;
                dr["EQPTID"] = _Eqpt;

                dtRqst.Rows.Add(dr);

                string sBiz = "DA_QCA_SEL_WIPDATACOLLECT_TERM";

                if (_Proc.Equals(Process.NOTCHING)) {
                    sBiz = "DA_QCA_SEL_WIPDATACOLLECT_LOT";
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", dtRqst);


                Util.gridClear(dgQualityInfo);

                dgQualityInfo.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [추가]
        private void AddQuality()
        {
            try
            { 


                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("EQPTID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _Proc;
                dr["LOTID"] = _LotId;
                dr["WIPSEQ"] = _WipSeq;
                dr["EQPTID"] = _Eqpt;

                dtRqst.Rows.Add(dr);

                string sBiz = "DA_QCA_SEL_WIPDATACOLLECT_TERM";

                if (_Proc.Equals(Process.NOTCHING))
                {
                    sBiz = "DA_QCA_SEL_WIPDATACOLLECT_LOT";
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", dtRqst);
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "OUTDATA", dtRqst);

                DataTable dtRqstAdd = new DataTable();
                dtRqstAdd.Columns.Add("LANGID", typeof(string));
                dtRqstAdd.Columns.Add("AREAID", typeof(string));
                dtRqstAdd.Columns.Add("PROCID", typeof(string));
                dtRqstAdd.Columns.Add("LOTID", typeof(string));

                DataRow drAdd = dtRqstAdd.NewRow();
                drAdd["LANGID"] = LoginInfo.LANGID;
                drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
                drAdd["PROCID"] = _Proc;
                drAdd["LOTID"] = _LotId;

                dtRqstAdd.Rows.Add(drAdd);

                DataTable dtRsltAdd = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM", "INDATA", "OUTDATA", dtRqstAdd);

                //CLCTSEQ TYPE 문제로 MERGE 사용 못했슴
                //dtRsltAdd.Columns["CLCTSEQ"].DataType = typeof(Int16);
                //dtRslt.Columns["CLCTSEQ"].DataType = typeof(Int16);

                //dtRslt.Merge(dtRsltAdd);

                object oMax = dtRslt.Compute("MAX(CLCTSEQ)", String.Empty);

                int iMax = 1;
                if (!oMax.Equals(DBNull.Value))
                {
                    iMax = Convert.ToInt16(oMax) + 1;
                }

                int irow = 0;
                foreach (DataRow dr1 in dtRsltAdd.Rows)
                {
                    DataRow drNew = dtRslt.NewRow();
                    drNew["CLCTITEM"] = dr1["CLCTITEM"];
                    drNew["CLCTNAME"] = dr1["CLCTNAME"];
                    drNew["CLSS_NAME1"] = dr1["CLSS_NAME1"];
                    drNew["CLSS_NAME2"] = dr1["CLSS_NAME2"];
                    drNew["CLCTUNIT"] = dr1["CLCTUNIT"];
                    drNew["USL"] = dr1["USL"];
                    drNew["LSL"] = dr1["LSL"];
                    drNew["CLCTSEQ"] = iMax;
                    drNew["INSP_VALUE_TYPE_CODE"] = dr1["INSP_VALUE_TYPE_CODE"];
                    drNew["TEXTVISIBLE"] = dr1["TEXTVISIBLE"];
                    drNew["COMBOVISIBLE"] = dr1["COMBOVISIBLE"];


                    dtRslt.Rows.InsertAt(drNew, irow);

                    irow++;
                }

                Util.gridClear(dgQualityInfo);

                dgQualityInfo.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [저장]
        private void SaveQuality()
        {
            try
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTSEQ", typeof(Int16));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("CLCTVAL01", typeof(string));
                dtRqst.Columns.Add("CLCTMAX", typeof(string));
                dtRqst.Columns.Add("CLCTMIN", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CLCTSEQ_ORG", typeof(Int16));

                foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgQualityInfo))
                {
                    //if (!row["CLCTVAL01"].Equals(DBNull.Value))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["LOTID"] = row["LOTID"];
                        dr["WIPSEQ"] = row["WIPSEQ"];
                        dr["CLCTSEQ"] = row["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                        dr["CLCTITEM"] = row["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                        dr["CLCTVAL01"] = (!row["CLCTVAL01"].Equals(DBNull.Value) && !row["CLCTVAL01"].Equals("-")) ? row["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
                        dr["CLCTMAX"] = row["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                        dr["CLCTMIN"] = row["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                        dr["EQPTID"] = _Eqpt;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["CLCTSEQ_ORG"] = row["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");

                        dtRqst.Rows.Add(dr);

                        if (!dict.ContainsKey(row["CLCTSEQ"].ToString()))
                        {
                            dict.Add(row["CLCTSEQ"].ToString(), row["CLCTSEQ"].ToString());
                        }
                    }
                }

                foreach (DataRowView row in DataGridHandler.GetAddedItems(dgQualityInfo))
                {

                    //if (!row["CLCTVAL01"].Equals(DBNull.Value))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["LOTID"] = _LotId;
                        dr["WIPSEQ"] = _WipSeq;
                        dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                        dr["CLCTITEM"] = row["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                        dr["CLCTVAL01"] = (!row["CLCTVAL01"].Equals(DBNull.Value)) ? row["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : ""; // DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
                        dr["CLCTMAX"] = row["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                        dr["CLCTMIN"] = row["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                        dr["EQPTID"] = _Eqpt;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["CLCTSEQ_ORG"] = row["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");

                        dtRqst.Rows.Add(dr);

                        if (!dict.ContainsKey(row["CLCTSEQ"].ToString()))
                        {
                            dict.Add(row["CLCTSEQ"].ToString(), row["CLCTSEQ"].ToString());
                        }
                    }
                }

                if (dtRqst.Rows.Count > 0)
                {

                    foreach (KeyValuePair<string, string> kv in dict) //차수별로 따로 던지기 한꺼번에 던지면 비즈에서 처리안됨
                    {
                        //Console.WriteLine("Key: {0}, Value: {1}", kv.Key, kv.Value);

                        DataTable dtRqst2 = dtRqst.Select("CLCTSEQ_ORG=" + kv.Key).CopyToDataTable();

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst2);
                    }

                    Util.MessageInfo("SFU1270");      //저장되었습니다.
                    GetQuality();
                }
                else
                {
                    Util.MessageInfo("SFU1566");      //변경된데이타가없습니다.
                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [품질조회]
        private void GetQualityList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _Proc;
                dr["LOTID"] = _LotId;
                dr["WIPSEQ"] = _WipSeq;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_DATA_PIVOT", "INDATA", "OUTDATA", dtRqst);

                int iMax = 0;
                if (dtRslt.Rows.Count > 0 && !dtRslt.Rows[0]["MAX_SEQ"].Equals(DBNull.Value))
                {
                    iMax = Convert.ToInt16(dtRslt.Rows[0]["MAX_SEQ"]);
                }

                for (int i = 1; i <= iMax; i++) {
                    dgQualityList.Columns["Q"+i.ToString("0#")].Visibility = Visibility.Visible;
                }

                Util.gridClear(dgQualityList);
                dgQualityList.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #endregion

        private void txtVal_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            //DataTableConverter.GetValue(tb.DataContext, "CHK").Equals(0))
            if (tb.Visibility.Equals(Visibility.Visible))
            {
                DataTableConverter.SetValue(tb.DataContext, "CLCTVAL01", tb.Text);
            }

        }

        private void txtVal_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            //DataTableConverter.GetValue(tb.DataContext, "CHK").Equals(0))
            if (tb.Visibility.Equals(Visibility.Visible))
            {
                DataTableConverter.SetValue(tb.DataContext, "CLCTVAL01", tb.Text);
            }
        }

        private void txtVal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            foreach (char c in e.Text)
            {
                if (c != '.' && c != '-')
                {
                    if (!char.IsDigit(c))
                    {
                        e.Handled = true;
                        break;
                    }
                }

            }

        }

        private void CLCTVAL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                int rIdx = 0;
                int cIdx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox n = sender as C1NumericBox;
                    StackPanel panel = n.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    rIdx = p.Cell.Row.Index;
                    cIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;
                }

                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox n = sender as ComboBox;
                    StackPanel panel = n.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    rIdx = p.Cell.Row.Index;
                    cIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;
                }
                else
                    return;

                if (grid.GetRowCount() > ++rIdx)
                {
                    C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                    StackPanel panel = p.Content as StackPanel;

                    for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                    {
                        if(panel.Children[cnt].Visibility == Visibility.Visible)
                            panel.Children[cnt].Focus();
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Source.GetType().Name == "C1DataGrid" && (e.Key == Key.Tab || e.Key == Key.Enter))
            {
                e.Handled = true;
            }
            else
                base.OnKeyDown(e);
        }
        
        private void grid_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("CLCTVAL01"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));

        }
    }
}
