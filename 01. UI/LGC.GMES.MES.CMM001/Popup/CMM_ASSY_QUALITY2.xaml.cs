/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_QUALITY2 : C1Window, IWorkArea
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

        public CMM_ASSY_QUALITY2()
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

            //대표LOT 가져오기
            //DataTable dtRqst = new DataTable();
            //dtRqst.Columns.Add("AREAID", typeof(string));
            //dtRqst.Columns.Add("PROCID", typeof(string));
            //dtRqst.Columns.Add("EQPTID", typeof(string));

            //DataRow dr1 = dtRqst.NewRow();
            //dr1["AREAID"] = LoginInfo.CFG_AREA_ID;
            //dr1["PROCID"] = _Proc;
            //dr1["EQPTID"] = _Eqpt;

            //dtRqst.Rows.Add(dr1);

            //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_REPRESENT_LOT", "INDATA", "OUTDATA", dtRqst);
            //if (dtRslt.Rows.Count > 0) {
            //    _LotId = dtRslt.Rows[0]["LOTID"].ToString();
            //    _WipSeq = dtRslt.Rows[0]["WIPSEQ"].ToString();

            //}


            cboNest.DisplayMemberPath = "CBO_NAME";
            cboNest.SelectedValuePath = "CBO_CODE";


            DataTable dtNest = new DataTable();
            dtNest.Columns.Add("CBO_NAME", typeof(string));
            dtNest.Columns.Add("CBO_CODE", typeof(string));

            for (int i = 1; i < 9; i++)
            {
                DataRow dr = dtNest.NewRow();
                dr["CBO_NAME"] = "Nest" + i.ToString();
                dr["CBO_CODE"] = @"ko-KR\Nest" + i.ToString();

                dtNest.Rows.Add(dr);
            }

            cboNest.ItemsSource = dtNest.Copy().AsDataView();

            cboNest.SelectedIndex = 0;

            setDimenCombo();

            GetQuality("btnQualitySearch");
        }

        private void btnQualitySearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                GetQuality(bt.Name);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnQualityAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "추가하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Button bt = sender as Button;
                            AddQuality(bt.Name);
                        }
                    });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [Dimension 추가]
        private void btnQualityAddDimen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "추가하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.Columns.Add("LOTID", typeof(string));
                            dtRqst.Columns.Add("WIPSEQ", typeof(string));
                            dtRqst.Columns.Add("PROCID", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));
                            dtRqst.Columns.Add("ACTDTTM", typeof(DateTime));
                            dtRqst.Columns.Add("EQPTID", typeof(string));


                            DataRow dr = dtRqst.NewRow();
                            dr["LOTID"] = _LotId;
                            dr["WIPSEQ"] = _WipSeq;
                            dr["PROCID"] = _Proc;
                            dr["USERID"] = LoginInfo.USERID;
                            dr["ACTDTTM"] = DateTime.Now;
                            dr["EQPTID"] = _Eqpt;

                            dtRqst.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_INS_PACKING_DIMEN", "INDATA", null, dtRqst);

                            setDimenCombo();
                        }
                    });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;
                SaveQuality(bt.Name);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnQualityInfoSearch_Click(object sender, RoutedEventArgs e)
        {




        }
        #endregion

        #region Mehod
        #region [조회]
        private void GetQuality(string sType)
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
                dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqst.Columns.Add("DIMEN_INSDTTM", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _Proc;
                dr["LOTID"] = _LotId;
                dr["WIPSEQ"] = _WipSeq;
                dr["EQPTID"] = _Eqpt;

                switch (sType)
                {
                    case "btnQualitySearch":
                    case "btnQualitySave":
                        dr["CLCTITEM_CLSS4"] = "A";
                        break;

                    case "btnQualitySearchDimen":
                    case "btnQualitySaveDimen":
                        dr["CLCTITEM_CLSS4"] = "B";
                        dr["CLCTITEM_CLSS3"] = Util.GetCondition(cboNest);
                        if (cboDimenTime.SelectedValue != null)
                        {
                            dr["DIMEN_INSDTTM"] = cboDimenTime.SelectedValue;
                        }
                        break;

                    case "btnQualitySearchSealing":
                    case "btnQualitySaveSealing":
                        dr["CLCTITEM_CLSS4"] = "C";
                        break;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_TERM", "INDATA", "OUTDATA", dtRqst);

                switch (sType)
                {
                    case "btnQualitySearch":
                    case "btnQualitySave":
                        Util.gridClear(dgQualityInfo);
                        dgQualityInfo.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
                        break;

                    case "btnQualitySearchDimen":
                    case "btnQualitySaveDimen":
                        Util.gridClear(dgQualityInfoDimen);
                        dgQualityInfoDimen.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
                        break;

                    case "btnQualitySearchSealing":
                    case "btnQualitySaveSealing":
                        Util.gridClear(dgQualityInfoSealing);
                        dgQualityInfoSealing.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
                        break;
                }


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [추가]
        private void AddQuality(string sType)
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
                dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _Proc;
                dr["LOTID"] = _LotId;
                dr["WIPSEQ"] = _WipSeq;
                dr["EQPTID"] = _Eqpt;


                switch (sType)
                {
                    case "btnQualityAdd":
                        dr["CLCTITEM_CLSS4"] = "A";
                        break;

                    case "btnQualityAddDimen":
                        dr["CLCTITEM_CLSS4"] = "B";
                        dr["CLCTITEM_CLSS3"] = Util.GetCondition(cboNest);
                        break;

                    case "btnQualityAddSealing":
                        dr["CLCTITEM_CLSS4"] = "C";
                        break;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_TERM", "INDATA", "OUTDATA", dtRqst);


                if ((sType.Equals("btnQualityAddDimen") && dtRslt.Rows.Count > 0))
                {
                }
                else
                {
                    DataTable dtRqstAdd = new DataTable();
                    dtRqstAdd.Columns.Add("LANGID", typeof(string));
                    dtRqstAdd.Columns.Add("AREAID", typeof(string));
                    dtRqstAdd.Columns.Add("PROCID", typeof(string));
                    dtRqstAdd.Columns.Add("LOTID", typeof(string));
                    dtRqstAdd.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                    dtRqstAdd.Columns.Add("CLCTITEM_CLSS3", typeof(string));

                    DataRow drAdd = dtRqstAdd.NewRow();
                    drAdd["LANGID"] = LoginInfo.LANGID;
                    drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drAdd["PROCID"] = _Proc;
                    drAdd["LOTID"] = _LotId;

                    switch (sType)
                    {
                        case "btnQualityAdd":
                            drAdd["CLCTITEM_CLSS4"] = "A";
                            break;

                        case "btnQualityAddDimen":
                            drAdd["CLCTITEM_CLSS4"] = "B";
                            drAdd["CLCTITEM_CLSS3"] = Util.GetCondition(cboNest);
                            break;

                        case "btnQualityAddSealing":
                            drAdd["CLCTITEM_CLSS4"] = "C";
                            break;
                    }

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
                }

                switch (sType)
                {
                    case "btnQualityAdd":
                        Util.gridClear(dgQualityInfo);
                        dgQualityInfo.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
                        break;

                    case "btnQualityAddDimen":
                        Util.gridClear(dgQualityInfoDimen);
                        dgQualityInfoDimen.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
                        break;

                    case "btnQualityAddSealing":
                        Util.gridClear(dgQualityInfoSealing);
                        dgQualityInfoSealing.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
                        break;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion


        #region [저장]
        private void SaveQuality(string sType)
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

                C1.WPF.DataGrid.C1DataGrid dgSave = null;
                switch (sType)
                {
                    case "btnQualitySave":
                        dgSave = dgQualityInfo;
                        break;

                    case "btnQualitySaveDimen":
                        dgSave = dgQualityInfoDimen;
                        break;

                    case "btnQualitySaveSealing":
                        dgSave = dgQualityInfoSealing;
                        break;
                }

                dgSave.EndEdit();


                foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgSave))
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

                foreach (DataRowView row in DataGridHandler.GetAddedItems(dgSave))
                {

                    //if (!row["CLCTVAL01"].Equals(DBNull.Value))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["LOTID"] = _LotId;
                        dr["WIPSEQ"] = _WipSeq;
                        dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
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


                if (dtRqst.Rows.Count > 0)
                {
                    //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIPDATACOLLECT", "INDATA", null, dtRqst);

                    foreach (KeyValuePair<string, string> kv in dict) //차수별로 따로 던지기 한꺼번에 던지면 비즈에서 처리안됨
                    {
                        //Console.WriteLine("Key: {0}, Value: {1}", kv.Key, kv.Value);

                        DataTable dtRqst2 = dtRqst.Select("CLCTSEQ_ORG=" + kv.Key).CopyToDataTable();

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst2);
                    }


                    Util.Alert("SFU1270");      //저장되었습니다.
                    GetQuality(sType);
                }
                else
                {
                    Util.Alert("SFU1566");      //변경된데이타가없습니다.
                }



            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [품질조회]
        //private void GetQualityList()
        //{
        //    try
        //    {
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.Columns.Add("LANGID", typeof(string));
        //        dtRqst.Columns.Add("PROCID", typeof(string));
        //        dtRqst.Columns.Add("LOTID", typeof(string));
        //        dtRqst.Columns.Add("WIPSEQ", typeof(Int16));


        //        DataRow dr = dtRqst.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["PROCID"] = _Proc;
        //        dr["LOTID"] = _LotId;
        //        dr["WIPSEQ"] = _WipSeq;

        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_DATA_PIVOT", "INDATA", "OUTDATA", dtRqst);

        //        int iMax = 0;
        //        if (dtRslt.Rows.Count > 0 && !dtRslt.Rows[0]["MAX_SEQ"].Equals(DBNull.Value))
        //        {
        //            iMax = Convert.ToInt16(dtRslt.Rows[0]["MAX_SEQ"]);
        //        }

        //        for (int i = 1; i <= iMax; i++) {
        //            dgQualityList.Columns["Q"+i.ToString("0#")].Visibility = Visibility.Visible;
        //        }

        //        Util.gridClear(dgQualityList);
        //        dgQualityList.ItemsSource = DataTableConverter.Convert(dtRslt);
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}


        #endregion

        #endregion

        //private void txtVal_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TextBox tb = sender as TextBox;
        //    //DataTableConverter.GetValue(tb.DataContext, "CHK").Equals(0))
        //    if (tb.Visibility.Equals(Visibility.Visible))
        //    {
        //        DataTableConverter.SetValue(tb.DataContext, "CLCTVAL01", tb.Text);
        //    }

        //}

        private void setDimenCombo()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("EQPTID", typeof(string));


            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQPTID"] = _Eqpt;

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_DIMEN_COMBO", "INDATA", "OUTDATA", dtRqst);

            cboDimenTime.DisplayMemberPath = "CBO_NAME";
            cboDimenTime.SelectedValuePath = "CBO_CODE";


            cboDimenTime.ItemsSource = dtRslt.Copy().AsDataView();

            cboDimenTime.SelectedIndex = 0;
        }

        private void txtVal_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            //DataTableConverter.GetValue(tb.DataContext, "CHK").Equals(0))
            if (tb.Visibility.Equals(Visibility.Visible))
            {
                DataTableConverter.SetValue(tb.DataContext, "CLCTVAL01", tb.Text);
            }
        }

        private void cboNest_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (c1tabDimen.IsSelected)
            {
                GetQuality("btnQualitySearchDimen");
            }
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (!_LotId.Equals(String.Empty))
            {
                if (c1tabDimen.IsSelected)
                {
                    GetQuality("btnQualitySearchDimen");
                }

                if (c1tab.IsSelected)
                {
                    GetQuality("btnQualitySearch");
                }

                if (c1tabSealing.IsSelected)
                {
                    GetQuality("btnQualitySearchSealing");
                }
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
                        if (panel.Children[cnt].Visibility == Visibility.Visible)
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