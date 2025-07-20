/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.10.25  주건태 C20171011_02560 : [CSR ID:3502560] GMES 자주검사 조희 개선 요청 건
  2019.04.29  정문교 폴란드3동 대응 Carrier ID(CSTID) 조회조건, 조회칼럼 추가




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_053 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public COM001_053()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);


            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged; //공정변할때 lane_qty, coating ver 보여주기 관리
            CboProcess_SelectedItemChanged(null, null);

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);



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
        }


        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            cboNest.SelectedItemChanged += cboNest_SelectedItemChanged;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }


        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null)
            {
                // Vision Image는 Sliiter 공정에서만 보여지게 추가 ( 2017-01-05 )
                if (cboProcess.SelectedValue.Equals(Process.PACKAGING))
                {
                    c1tabDefault.Visibility = Visibility.Collapsed;
                    c1tabDimen.Visibility = Visibility.Visible;
                    c1tabSealing.Visibility = Visibility.Visible;
                    c1tabTensile.Visibility = Visibility.Visible;

                    txtCstID.IsEnabled = false;
                }
                else
                {
                    c1tabDefault.Visibility = Visibility.Visible;
                    c1tabDimen.Visibility = Visibility.Collapsed;
                    c1tabSealing.Visibility = Visibility.Collapsed;
                    c1tabTensile.Visibility = Visibility.Collapsed;

                    txtCstID.IsEnabled = true;
                }

            }
        }



        #endregion

        #region Mehod
        #region [작업대상 가져오기]
        public void GetList()
        {
            try
            {
                //동을 선택하세요.
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboArea, "SFU1499")))
                    return;

                //라인을 선택하세요.
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboEquipmentSegment, "SFU1223")))
                    return;
                //공정을 선택하세요.
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboProcess, "SFU1459")))
                    return;
                //설비를 선택하세요.
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboEquipment, "SFU1153")))
                    return;

                if (c1tabDefault.Visibility.Equals(Visibility.Visible))
                {

                    GetQuality();
                }

                if (c1tabTensile.Visibility.Equals(Visibility.Visible))
                {
                    GetTensile();
                }

                if (c1tabDimen.Visibility.Equals(Visibility.Visible))
                {
                    GetDimen();
                }

                if (c1tabSealing.Visibility.Equals(Visibility.Visible))
                {
                    GetSealing();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }



        public void GetQuality()
        {
            try
            {
                string bizRuleName = "DA_QCA_SEL_WIPDATACOLLECT_TERM2";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("INSDTTM", typeof(string));
                dtRqst.Columns.Add("INSDTTM_TO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.

                if (string.IsNullOrWhiteSpace(txtLotID.Text) && string.IsNullOrWhiteSpace(txtCstID.Text))
                {
                    dr["INSDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["INSDTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dtRqst.Rows.Add(dr);

                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                    {
                        dr["LOTID"] = txtLotID.Text;
                        dtRqst.Rows.Add(dr);
                    }
                    else
                    {
                        // Carrier ID로 조회시
                        dtRqst = new DataTable();
                        dtRqst.Columns.Add("LANGID", typeof(string));
                        dtRqst.Columns.Add("CSTID", typeof(string));

                        bizRuleName = "DA_QCA_SEL_WIPDATACOLLECT_TERM2_L";

                        DataRow dr2 = dtRqst.NewRow();
                        dr2["LANGID"] = LoginInfo.LANGID;
                        dr2["CSTID"] = txtCstID.Text;
                        dtRqst.Rows.Add(dr2);
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                //     Util.gridClear(dgQualityInfo);
                Util.GridSetData(dgQualityInfo, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetTensile()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("INSDTTM", typeof(string));
                dtRqst.Columns.Add("INSDTTM_TO", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqst.Columns.Add("DIMEN_INSDTTM", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                {
                    dr["LOTID"] = txtLotID.Text;
                }
                else
                {
                    dr["INSDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["INSDTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                dr["CLCTITEM_CLSS4"] = "A";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_TERM2", "INDATA", "OUTDATA", dtRqst);


                Util.gridClear(dgQualityTensileInfo);

                dgQualityTensileInfo.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetDimen()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("INSDTTM", typeof(string));
                dtRqst.Columns.Add("INSDTTM_TO", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqst.Columns.Add("DIMEN_INSDTTM", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                {
                    dr["LOTID"] = txtLotID.Text;
                }
                else
                {
                    dr["INSDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["INSDTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                dr["CLCTITEM_CLSS4"] = "B";
                dr["CLCTITEM_CLSS3"] = Util.GetCondition(cboNest);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_TERM2", "INDATA", "OUTDATA", dtRqst);


                Util.gridClear(dgQualityInfoDimen);

                dgQualityInfoDimen.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        public void GetSealing()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("INSDTTM", typeof(string));
                dtRqst.Columns.Add("INSDTTM_TO", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                dtRqst.Columns.Add("DIMEN_INSDTTM", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                {
                    dr["LOTID"] = txtLotID.Text;
                }
                else
                {
                    dr["INSDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["INSDTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                dr["CLCTITEM_CLSS4"] = "C";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_TERM2", "INDATA", "OUTDATA", dtRqst);


                Util.gridClear(dgQualityInfoSealing);

                dgQualityInfoSealing.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #endregion

        private void cboNest_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetDimen();
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
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

                C1.WPF.DataGrid.C1DataGrid dgSave = dgQualityInfo;

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
                        dr["EQPTID"] = row["EQPTID"];
                        dr["USERID"] = LoginInfo.USERID;
                        dr["CLCTSEQ_ORG"] = row["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");

                        dtRqst.Rows.Add(dr);

                        if (!dict.ContainsKey(row["CLCTSEQ"].ToString()))
                        {
                            dict.Add(row["CLCTSEQ"].ToString(), row["CLCTSEQ"].ToString());
                        }
                    }
                }

                //foreach (DataRowView row in DataGridHandler.GetAddedItems(dgSave))
                //{

                //    //if (!row["CLCTVAL01"].Equals(DBNull.Value))
                //    {
                //        DataRow dr = dtRqst.NewRow();
                //        dr["SRCTYPE"] = "UI";
                //        dr["LOTID"] = _LotId;
                //        dr["WIPSEQ"] = _WipSeq;
                //        dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                //        dr["CLCTITEM"] = row["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                //        dr["CLCTVAL01"] = (!row["CLCTVAL01"].Equals(DBNull.Value) && !row["CLCTVAL01"].Equals("-")) ? row["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
                //        dr["CLCTMAX"] = row["USL"];// DataTableConverter.GetValue(row.DataItem, "USL");
                //        dr["CLCTMIN"] = row["LSL"];// DataTableConverter.GetValue(row.DataItem, "LSL");
                //        dr["EQPTID"] = _Eqpt;
                //        dr["USERID"] = LoginInfo.USERID;
                //        dr["CLCTSEQ_ORG"] = row["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");

                //        dtRqst.Rows.Add(dr);

                //        if (!dict.ContainsKey(row["CLCTSEQ"].ToString()))
                //        {
                //            dict.Add(row["CLCTSEQ"].ToString(), row["CLCTSEQ"].ToString());
                //        }
                //    }
                //}


                if (dtRqst.Rows.Count > 0)
                {
                    //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIPDATACOLLECT", "INDATA", null, dtRqst);

                    foreach (KeyValuePair<string, string> kv in dict) //차수별로 따로 던지기 한꺼번에 던지면 비즈에서 처리안됨
                    {
                        //Console.WriteLine("Key: {0}, Value: {1}", kv.Key, kv.Value);

                        DataTable dtRqst2 = dtRqst.Select("CLCTSEQ_ORG=" + kv.Key).CopyToDataTable();

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst2);
                    }

                    Util.MessageInfo("SFU1270");  //저장되었습니다.
                    //Util.AlertInfo("SFU1270");  
                    GetQuality();
                }
                else
                {
                    Util.MessageInfo("SFU1566");  //변경된 데이터가 없습니다.
                    //Util.AlertInfo("SFU1566");  
                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnQualityTensileSave_Click(object sender, RoutedEventArgs e)
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

                C1.WPF.DataGrid.C1DataGrid dgSave = dgQualityTensileInfo; ;

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
                        dr["EQPTID"] = row["EQPTID"];
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

                    Util.MessageInfo("SFU1270"); //저장되었습니다.
                    //Util.AlertInfo("SFU1270");  
                    GetTensile();
                }
                else
                {
                    Util.MessageInfo("SFU1566"); //변경된 데이터가 없습니다.
                    //Util.AlertInfo("SFU1566");  
                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnQualitySaveDimen_Click(object sender, RoutedEventArgs e)
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

                C1.WPF.DataGrid.C1DataGrid dgSave = dgQualityInfoDimen;

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
                        dr["EQPTID"] = row["EQPTID"];
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

                    Util.MessageInfo("SFU1270");  //저장되었습니다.
                    //Util.AlertInfo("SFU1270"); 
                    GetDimen();
                }
                else
                {
                    Util.MessageInfo("SFU1566");  //변경된 데이터가 없습니다.
                    //Util.AlertInfo("SFU1566");
                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnQualitySaveSealing_Click(object sender, RoutedEventArgs e)
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

                C1.WPF.DataGrid.C1DataGrid dgSave = dgQualityInfoSealing;


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
                        dr["EQPTID"] = row["EQPTID"];
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

                    Util.MessageInfo("SFU1270");  //저장되었습니다.
                    //Util.AlertInfo("SFU1270");  
                    GetSealing();
                }
                else
                {
                    Util.MessageInfo("SFU1566");  //변경된 데이터가 없습니다.
                    //Util.AlertInfo("SFU1566");  
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void btnSearch_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

        }

        //private void DataGridTemplateColumn_GettingCellValue(object sender, C1.WPF.DataGrid.DataGridGettingCellValueEventArgs e)
        //{
        //    //C20171011_02560 : DataGridTemplateColumn 일 경우 엑셀 파일에 값 나오게 하기 위해. DataGridTemplateColumn 일 경우 필요함.
        //    DataRowView drvTmp = (e.Row.DataItem as DataRowView);
        //    //System.Windows.MessageBox.Show((!drvTmp["CLCTVAL01"].Equals(DBNull.Value) && !drvTmp["CLCTVAL01"].Equals("-")) ? drvTmp["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "");
        //    e.Value = (!drvTmp["CLCTVAL01"].Equals(DBNull.Value) && !drvTmp["CLCTVAL01"].Equals("-")) ? drvTmp["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";
        //}

        private void DataGridTemplateColumn_GettingCellValue(object sender, C1.WPF.DataGrid.DataGridGettingCellValueEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.DataGridTemplateColumn dgtc = sender as C1.WPF.DataGrid.DataGridTemplateColumn;
                System.Data.DataRowView drv = e.Row.DataItem as System.Data.DataRowView;

                if (dgtc != null && drv != null && dgtc.Name != null)
                {
                    e.Value = drv[dgtc.Name].ToString();
                }
            }
            catch (Exception ex)
            {
                //오류 발생할 경우 아무 동작도 하지 않음. try catch 없으면 이 로직에서 오류날 경우 복사 자체가 안됨
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
