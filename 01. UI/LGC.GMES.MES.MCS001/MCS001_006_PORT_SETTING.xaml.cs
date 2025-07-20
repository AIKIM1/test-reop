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
using LGC.GMES.MES.CMM001.Extensions;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using System.Linq;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_006_PORT_SETTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        DataTable _DT_PORT_NAME = null;         //��Ʈ��
        DataTable _DT_AUTO_ISS_REQ_FLAG = null; //MCS_CURR_PORT �ڵ��ݼ۸��
        DataTable _DT_ELTR_TYPE_CODE = null;    //��Ʈ�ؼ�
        DataTable _DT_USE_FLAG = null;          //�������
        DataTable _DT_RETURN_EQPTID = null;     //�ݼۼ���

        string _BeforePortID = string.Empty; //�����ϱ��� PortID
    
        /// <summary>
        ///  Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_006_PORT_SETTING()
        {
            InitializeComponent();
        }


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetPortId(); //PortID ����
            SetReturnEqpt();//�����
            SetAutoIss(); //�ڵ�����û
            _DT_USE_FLAG = new DataTable();
            _DT_USE_FLAG = _dttComCode("IUSE");//�������
            _DT_ELTR_TYPE_CODE = new DataTable();
            _DT_ELTR_TYPE_CODE = _dttComCode("ELTR_TYPE_CODE");//��Ʈ�ؼ�
            GetPortOutEqpt();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnPortSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPortOutEqpt();
        }

        private void btnQualityAdd_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                //    "�߰��Ͻðڽ��ϱ�?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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
        private void btnPortSave_Click(object sender, RoutedEventArgs e)
        {
            if (!PortSaveValidation())
            {
                return;
            }
            //�����Ͻðڽ��ϱ�?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveQuality();
                }
            });
        }
        private void btnQualityInfoSearch_Click(object sender, RoutedEventArgs e)
        {
            GetQualityList();
        }
        #endregion

        #region Mehod
        #region [��ȸ]
        private void GetPortOutEqpt()
        {
            try
            {

                loadingIndicator.Visibility = Visibility.Visible;
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_MCS_SEL_PORT_LINK_EQPT_LOGIS_CMD_PORT_ID", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) => {
                    try
                    {
                        if (exception != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.MessageException(exception);
                            return;
                        }
                        Util.gridClear(dgOutPutPort);
                        Util.GridSetData(dgOutPutPort, result, FrameOperation, false);

                        //�׸��� �޺� ����
                        C1.WPF.DataGrid.DataGridComboBoxColumn PortName = dgOutPutPort.Columns["PORT_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn; //��Ʈ��
                        C1.WPF.DataGrid.DataGridComboBoxColumn IssReqFlag = dgOutPutPort.Columns["AUTO_ISS_REQ_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn; //�ڵ�����û
                        C1.WPF.DataGrid.DataGridComboBoxColumn EltrTypeCode = dgOutPutPort.Columns["ELTR_TYPE_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn; //��Ʈ�ؼ�
                        C1.WPF.DataGrid.DataGridComboBoxColumn UseFlag = dgOutPutPort.Columns["USE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn;//��뿩��
                        C1.WPF.DataGrid.DataGridComboBoxColumn LinkEqptID = dgOutPutPort.Columns["LINK_EQPTID"] as C1.WPF.DataGrid.DataGridComboBoxColumn; //����ݼۼ���
                        C1.WPF.DataGrid.DataGridComboBoxColumn NextLinkEqptID = dgOutPutPort.Columns["NEXT_LINK_EQPTID"] as C1.WPF.DataGrid.DataGridComboBoxColumn;//�����ݼۼ���

                        if (PortName != null)
                            PortName.ItemsSource = DataTableConverter.Convert(_DT_PORT_NAME); //��Ʈ��
                        if (IssReqFlag != null)
                            IssReqFlag.ItemsSource = DataTableConverter.Convert(_DT_AUTO_ISS_REQ_FLAG); //�ڵ�����û
                        if (EltrTypeCode != null)
                            EltrTypeCode.ItemsSource = DataTableConverter.Convert(_DT_ELTR_TYPE_CODE); //��Ʈ�ؼ�
                        if (UseFlag != null)
                            UseFlag.ItemsSource = DataTableConverter.Convert(_DT_USE_FLAG); //��뿩��
                        if (LinkEqptID != null)
                            LinkEqptID.ItemsSource = DataTableConverter.Convert(_DT_RETURN_EQPTID);//����ݼۼ���
                        if (NextLinkEqptID != null)
                            NextLinkEqptID.ItemsSource = DataTableConverter.Convert(_DT_RETURN_EQPTID);//�����ݼۼ���


                        loadingIndicator.Visibility = Visibility.Collapsed;
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
        #endregion

        #region [�߰�]
        private void AddQuality()
        {
            //try
            //{ 


            //    DataTable dtRqst = new DataTable();
            //    dtRqst.Columns.Add("LANGID", typeof(string));
            //    dtRqst.Columns.Add("AREAID", typeof(string));
            //    dtRqst.Columns.Add("PROCID", typeof(string));
            //    dtRqst.Columns.Add("LOTID", typeof(string));
            //    dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
            //    dtRqst.Columns.Add("EQPTID", typeof(string));


            //    DataRow dr = dtRqst.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            //    dr["PROCID"] = _Proc;
            //    dr["LOTID"] = _LotId;
            //    dr["WIPSEQ"] = _WipSeq;
            //    dr["EQPTID"] = _Eqpt;

            //    dtRqst.Rows.Add(dr);

            //    string sBiz = "DA_QCA_SEL_WIPDATACOLLECT_TERM";

            //    if (_Proc.Equals(Process.NOTCHING))
            //    {
            //        sBiz = "DA_QCA_SEL_WIPDATACOLLECT_LOT";
            //    }

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", dtRqst);
            //    //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "OUTDATA", dtRqst);

            //    DataTable dtRqstAdd = new DataTable();
            //    dtRqstAdd.Columns.Add("LANGID", typeof(string));
            //    dtRqstAdd.Columns.Add("AREAID", typeof(string));
            //    dtRqstAdd.Columns.Add("PROCID", typeof(string));
            //    dtRqstAdd.Columns.Add("LOTID", typeof(string));

            //    DataRow drAdd = dtRqstAdd.NewRow();
            //    drAdd["LANGID"] = LoginInfo.LANGID;
            //    drAdd["AREAID"] = LoginInfo.CFG_AREA_ID;
            //    drAdd["PROCID"] = _Proc;
            //    drAdd["LOTID"] = _LotId;

            //    dtRqstAdd.Rows.Add(drAdd);

            //    DataTable dtRsltAdd = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CLCTITEM", "INDATA", "OUTDATA", dtRqstAdd);

            //    //CLCTSEQ TYPE ������ MERGE ��� ���߽�
            //    //dtRsltAdd.Columns["CLCTSEQ"].DataType = typeof(Int16);
            //    //dtRslt.Columns["CLCTSEQ"].DataType = typeof(Int16);

            //    //dtRslt.Merge(dtRsltAdd);

            //    object oMax = dtRslt.Compute("MAX(CLCTSEQ)", String.Empty);

            //    int iMax = 1;
            //    if (!oMax.Equals(DBNull.Value))
            //    {
            //        iMax = Convert.ToInt16(oMax) + 1;
            //    }

            //    int irow = 0;
            //    foreach (DataRow dr1 in dtRsltAdd.Rows)
            //    {
            //        DataRow drNew = dtRslt.NewRow();
            //        drNew["CLCTITEM"] = dr1["CLCTITEM"];
            //        drNew["CLCTNAME"] = dr1["CLCTNAME"];
            //        drNew["CLSS_NAME1"] = dr1["CLSS_NAME1"];
            //        drNew["CLSS_NAME2"] = dr1["CLSS_NAME2"];
            //        drNew["CLCTUNIT"] = dr1["CLCTUNIT"];
            //        drNew["USL"] = dr1["USL"];
            //        drNew["LSL"] = dr1["LSL"];
            //        drNew["CLCTSEQ"] = iMax;
            //        drNew["INSP_VALUE_TYPE_CODE"] = dr1["INSP_VALUE_TYPE_CODE"];
            //        drNew["TEXTVISIBLE"] = dr1["TEXTVISIBLE"];
            //        drNew["COMBOVISIBLE"] = dr1["COMBOVISIBLE"];


            //        dtRslt.Rows.InsertAt(drNew, irow);

            //        irow++;
            //    }

            //    Util.gridClear(dgOutPutPort);

            //    dgOutPutPort.ItemsSource = dtRslt.DefaultView;//DataTableConverter.Convert(dtRslt);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        #endregion

        #region [����]
        private void SaveQuality()
        {
           

            try
            {
                //�̹� ����� ������ ���̺�
                DataTable dtSource = DataTableConverter.Convert(dgOutPutPort.ItemsSource);
                //üũ�� ������
                DataTable dtSaveSource = dtSource.Select("CHK = '" + "1" + "'").CopyToDataTable();

                //�ݼۼ������ üũ
                for (int i=0; i< dtSaveSource.Rows.Count; i++)
                {
                    if(dtSaveSource.Rows[i]["CHK"].ToString() == "1" && dtSaveSource.Rows[i]["SAVE_YN"].ToString() == "N")
                    {
                        //üũ�Ǿ��� ������°� �ƴ� PORT�����͸� ���̺�� ����
                        DataTable dtCheckSource = dtSaveSource.Select("PORT_ID = '" + dtSaveSource.Rows[i]["PORT_ID"].ToString() + "' AND SAVE_YN = '" + "N" + "'").CopyToDataTable();
                        //������� ����
                        //dtCheckSource.DefaultView.Sort = "LOGIS_CMD_SEQNO";

                        //���õ� PORT�� SEQ�� ���� ������
                        int iSavePortSeq = Convert.ToInt16(dtSource.Compute("count([PORT_ID])", "PORT_ID='" + dtSaveSource.Rows[i]["PORT_ID"].ToString() + "' AND SAVE_YN = '" + "Y" + "'"))+1;
                        //���õ� �����͵��� SEQ �ּҰ�
                        int iSavePortMinSeq = Convert.ToInt16(dtCheckSource.Compute("MIN([LOGIS_CMD_SEQNO])",  String.Empty));

                        if(iSavePortSeq != iSavePortMinSeq)
                        {
                            // �ݼۼ�������� Ʋ���ϴ�
                            Util.MessageValidation("�ݼۼ�������� Ʋ���ϴ�.");
                            return;
                        }
                    }
                }


                //Dictionary<string, string> dict = new Dictionary<string, string>();

                //DataTable dtRqst = new DataTable();
                //dtRqst.Columns.Add("SRCTYPE", typeof(string));
                //dtRqst.Columns.Add("LOTID", typeof(string));
                //dtRqst.Columns.Add("WIPSEQ", typeof(Int16));
                //dtRqst.Columns.Add("CLCTSEQ", typeof(Int16));
                //dtRqst.Columns.Add("CLCTITEM", typeof(string));
                //dtRqst.Columns.Add("CLCTVAL01", typeof(string));
                //dtRqst.Columns.Add("CLCTMAX", typeof(string));
                //dtRqst.Columns.Add("CLCTMIN", typeof(string));
                //dtRqst.Columns.Add("EQPTID", typeof(string));
                //dtRqst.Columns.Add("USERID", typeof(string));
                //dtRqst.Columns.Add("CLCTSEQ_ORG", typeof(Int16));

                //foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgOutPutPort))
                //{
                //    //if (!row["CLCTVAL01"].Equals(DBNull.Value))
                //    {
                //        DataRow dr = dtRqst.NewRow();
                //        dr["SRCTYPE"] = "UI";
                //        dr["LOTID"] = row["LOTID"];
                //        dr["WIPSEQ"] = row["WIPSEQ"];
                //        dr["CLCTSEQ"] = row["CLCTSEQ"];// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
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

                //foreach (DataRowView row in DataGridHandler.GetAddedItems(dgOutPutPort))
                //{

                //    //if (!row["CLCTVAL01"].Equals(DBNull.Value))
                //    {
                //        DataRow dr = dtRqst.NewRow();
                //        dr["SRCTYPE"] = "UI";
                //        dr["LOTID"] = _LotId;
                //        dr["WIPSEQ"] = _WipSeq;
                //        dr["CLCTSEQ"] = DBNull.Value;// DataTableConverter.GetValue(row.DataItem, "CLCTSEQ");
                //        dr["CLCTITEM"] = row["CLCTITEM"];// DataTableConverter.GetValue(row.DataItem, "CLCTITEM");
                //        dr["CLCTVAL01"] = (!row["CLCTVAL01"].Equals(DBNull.Value)) ? row["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : ""; // DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
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

                //if (dtRqst.Rows.Count > 0)
                //{

                //    foreach (KeyValuePair<string, string> kv in dict) //�������� ���� ������ �Ѳ����� ������ ����� ó���ȵ�
                //    {
                //        //Console.WriteLine("Key: {0}, Value: {1}", kv.Key, kv.Value);

                //        DataTable dtRqst2 = dtRqst.Select("CLCTSEQ_ORG=" + kv.Key).CopyToDataTable();

                //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, dtRqst2);
                //    }

                //    Util.MessageInfo("SFU1270");      //����Ǿ����ϴ�.
                //    GetQuality();
                //}
                //else
                //{
                //    Util.MessageInfo("SFU1566");      //����ȵ���Ÿ�������ϴ�.
                //}



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [ǰ����ȸ]
        private void GetQualityList()
        {
            //try
            //{
            //    DataTable dtRqst = new DataTable();
            //    dtRqst.Columns.Add("LANGID", typeof(string));
            //    dtRqst.Columns.Add("PROCID", typeof(string));
            //    dtRqst.Columns.Add("LOTID", typeof(string));
            //    dtRqst.Columns.Add("WIPSEQ", typeof(Int16));


            //    DataRow dr = dtRqst.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["PROCID"] = _Proc;
            //    dr["LOTID"] = _LotId;
            //    dr["WIPSEQ"] = _WipSeq;

            //    dtRqst.Rows.Add(dr);

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_DATA_PIVOT", "INDATA", "OUTDATA", dtRqst);

            //    int iMax = 0;
            //    if (dtRslt.Rows.Count > 0 && !dtRslt.Rows[0]["MAX_SEQ"].Equals(DBNull.Value))
            //    {
            //        iMax = Convert.ToInt16(dtRslt.Rows[0]["MAX_SEQ"]);
            //    }

            //    for (int i = 1; i <= iMax; i++) {
            //        dgQualityList.Columns["Q"+i.ToString("0#")].Visibility = Visibility.Visible;
            //    }

            //    Util.gridClear(dgQualityList);
            //    dgQualityList.ItemsSource = DataTableConverter.Convert(dtRslt);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }


        #endregion

        #endregion


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Source.GetType().Name == "C1DataGrid" && (e.Key == Key.Tab || e.Key == Key.Enter))
            {
                e.Handled = true;
            }
            else
                base.OnKeyDown(e);
        }
     
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtSource = DataTableConverter.Convert(dgOutPutPort.ItemsSource);
            DataRow dr = dtSource.NewRow();
            dr["CHK"] = 1;
            dr["PORT_ID"] = string.Empty;
            dr["AUTO_ISS_REQ_FLAG"] = string.Empty;
            dr["ELTR_TYPE_CODE"] = string.Empty;
            dr["USE_FLAG"] = string.Empty;
            dr["LINK_EQPTID"] = string.Empty;
            dr["NEXT_LINK_EQPTID"] = string.Empty;
            dr["SAVE_YN"] = "N";
            dtSource.Rows.InsertAt(dr, 0);
            //dtSource.Rows.Add(dr);
            Util.gridClear(dgOutPutPort);
            Util.GridSetData(dgOutPutPort, dtSource, FrameOperation, false);
        }
        /// <summary>
        /// ROW ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteDtail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MinusProcessDeleteValidation())
                {
                    return;
                }

                DataTable dtSource = DataTableConverter.Convert(dgOutPutPort.ItemsSource);

                if (dtSource.Select("CHK = '" + "1" + "' AND SAVE_YN = '" + "N" + "'").Length == 0) return;
                //���õ� PORT_ID�� ���� DT�� ����
                DataTable dtDeleteSource = dtSource.Select("CHK = '" + "1" + "' AND SAVE_YN = '" + "N" + "'").CopyToDataTable();
                //Row ����
                dtSource.Select("CHK = '" + "1" + "' AND SAVE_YN = '" + "N" + "'").ToList<DataRow>().ForEach(row => row.Delete());
                dtSource.AcceptChanges();
                //SEQ ���� ����
                for(int i=0; i< dtDeleteSource.Rows.Count; i++)
                {
                    if (dtDeleteSource.Rows[i]["PORT_ID"].ToString() != string.Empty)
                    {
                        //����� PORT_ID ���� ���
                        int _SeqNo = Convert.ToInt16(dtSource.Compute("count([PORT_ID])", "PORT_ID='" + dtSource.Rows[i]["PORT_ID"].ToString() + "'"));
                        for (int j = 0; j < dtSource.Rows.Count; j++)
                        {
                            if(dtDeleteSource.Rows[i]["PORT_ID"].ToString() == dtSource.Rows[j]["PORT_ID"].ToString() && dtSource.Rows[j]["SAVE_YN"].ToString() == "N")
                            {
                                dtSource.Rows[j]["LOGIS_CMD_SEQNO"] = _SeqNo;

                                _SeqNo = _SeqNo + 1;
                            }
                        }
                    }
                }

                Util.gridClear(dgOutPutPort);
                Util.GridSetData(dgOutPutPort, dtSource, FrameOperation, false);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        /// <summary>
        /// PORT_ID ��ȸ
        /// </summary>
        public void SetPortId()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
            
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);
                _DT_PORT_NAME = new DataTable();
                _DT_PORT_NAME = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_OUT_PORT_LAMI", "INDATA", "OUTDATA", dtRqst);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        /// <summary>
        /// PORT_ID ��ȸ
        /// </summary>
        public void SetReturnEqpt()
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);
                _DT_RETURN_EQPTID = new DataTable();
                _DT_RETURN_EQPTID = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_RETURN_EQPT", "INDATA", "OUTDATA", dtRqst);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        /// <summary>
        /// �ڵ�����û
        /// </summary>
        public void SetAutoIss()
        {
            try
            {
                _DT_AUTO_ISS_REQ_FLAG = new DataTable();
                _DT_AUTO_ISS_REQ_FLAG.Columns.Add("CBO_CODE", typeof(string));
                _DT_AUTO_ISS_REQ_FLAG.Columns.Add("CBO_NAME", typeof(string));

                DataRow dr = _DT_AUTO_ISS_REQ_FLAG.NewRow();
                dr["CBO_CODE"] ="Y";
                dr["CBO_NAME"] = "Y : Y";
                _DT_AUTO_ISS_REQ_FLAG.Rows.Add(dr);

                dr = _DT_AUTO_ISS_REQ_FLAG.NewRow();
                dr["CBO_CODE"] = "N";
                dr["CBO_NAME"] = "N : N";
                _DT_AUTO_ISS_REQ_FLAG.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        /// <summary>
        /// �����ڵ�
        /// </summary>
        DataTable _dttComCode(string CmcdType)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = CmcdType;
            dtRqst.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "INDATA", "OUTDATA", dtRqst);
         
            return dtResult;
        }
        /// <summary>
        /// PORT_ID ����� ����ݼۼ��� ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOutPutPort_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgOutPutPort.CurrentRow.DataItem as DataRowView;

                if (drv == null) return;

                if (e.Cell.Column.Name == "PORT_ID")
                {
                    DataTable dtSource = DataTableConverter.Convert(dgOutPutPort.ItemsSource);

                    //����� PORT_ID ���� ���
                    int iCnt = Convert.ToInt16(dtSource.Compute("count([PORT_ID])", "PORT_ID='" + Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_ID").ToString()) + "' AND SAVE_YN = '" + "Y" +  "'"))+1;


                    for (int i = 0; i < dtSource.Rows.Count; i++)
                    {
                        if (dtSource.Rows[i]["PORT_ID"].ToString() == dtSource.Rows[e.Cell.Row.Index]["PORT_ID"].ToString() && dtSource.Rows[i]["SAVE_YN"].ToString() == "N")
                        {
                            dtSource.Rows[i]["LOGIS_CMD_SEQNO"] = iCnt;

                            iCnt = iCnt + 1;
                        }

                    }

                    //dtSource.Rows[e.Cell.Row.Index]["LOGIS_CMD_SEQNO"] = iCnt;
                    //�������� PORT_ID ���� ���
                    if (_BeforePortID != string.Empty)
                    {
                        int _SeqNo = Convert.ToInt16(dtSource.Compute("count([PORT_ID])", "PORT_ID='" + _BeforePortID + "' AND SAVE_YN = '" + "Y" + "'"))+1;
                        ////������ PORT ID�� ���� ���������̺��� �ٽ� ����
                        //DataTable dtBeforeSoure = dtSource.Select("PORT_ID = '" + _BeforePortID + "'").CopyToDataTable();

                        //int _SeqNo = BeforeiCnt;
                        for (int i=0; i< dtSource.Rows.Count; i++)
                        {
                           if (dtSource.Rows[i]["PORT_ID"].ToString() == _BeforePortID && dtSource.Rows[i]["SAVE_YN"].ToString() == "N")
                            {
                                dtSource.Rows[i]["LOGIS_CMD_SEQNO"] = _SeqNo;

                                _SeqNo = _SeqNo + 1;
                            }
                             
                        }
           
                    }
               
                    Util.gridClear(dgOutPutPort);
                    Util.GridSetData(dgOutPutPort, dtSource, FrameOperation, false);

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
       
        /// <summary>
        /// ����� PORT_ID�� �޺��ڽ� ������ ���� �ʵ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOutPutPort_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (dgOutPutPort.CurrentCell != null && dgOutPutPort.SelectedIndex > -1)
            {
                if (dgOutPutPort.CurrentCell.Column.Name == "PORT_ID")
                {
                    string sTmpCell = Util.NVC(dgOutPutPort.GetCell(dgOutPutPort.CurrentRow.Index, dgOutPutPort.Columns["SAVE_YN"].Index).Value);
                    if (sTmpCell == "Y")
                    {
                        e.Cancel = true;    // Editing �Ұ���
                               
                    }
                    else
                    {
                        e.Cancel = false;   // Editing ����
                    }
                }
               
            }
        }
        /// <summary>
        /// ����Ǳ� �� PORT_ID (��Ʈ���� ����ɶ�.. ����ݼۼ����� �ٽ� ����ϱ� ���Ͽ�)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOutPutPort_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (e.Column.Name == "PORT_ID")
            {
                _BeforePortID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "PORT_ID"));
            }
        }

        private void dgOutPutPort_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        //�������� �ǹ� ������ ���������� Yellow
                        string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SAVE_YN"));

                        if (e.Cell.Presenter != null && sCheck.Equals("Y"))
                        {
                           e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["PORT_ID"].Index).Presenter.Background = new SolidColorBrush(Colors.WhiteSmoke);
                        }
                       
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutPutPort_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        /// <summary>
        /// ROW ���� Validatio
        /// </summary>
        /// <returns></returns>
        private bool MinusProcessDeleteValidation()
        {
           if(dgOutPutPort.Rows.Count == 0)
            {
                Util.AlertInfo("SFU1905"); //"��ȸ�� Data�� �����ϴ�."
                return false;
            }

            DataRow[] drchk = DataTableConverter.Convert(dgOutPutPort.ItemsSource).Select("CHK = 1");
            if (drchk.Length == 0)
            {
                // ���õ� �׸��� �����ϴ�.
                Util.MessageValidation("SFU1651");
                return false;
            }
           return true;
        }


        /// <summary>
        ///  Port ���� Validatio
        /// </summary>
        /// <returns></returns>
        private bool PortSaveValidation()
        {
            if (dgOutPutPort.Rows.Count == 0)
            {
                Util.AlertInfo("SFU1905"); //"��ȸ�� Data�� �����ϴ�."
                return false;
            }

            DataRow[] drchk = DataTableConverter.Convert(dgOutPutPort.ItemsSource).Select("CHK = 1");
            if (drchk.Length == 0)
            {
                // ���õ� �׸��� �����ϴ�.
                Util.MessageValidation("SFU1651");
                return false;
            }

            foreach (DataRow drDel in drchk)
            {
                
                if(drDel["PORT_ID"].ToString() == string.Empty)
                {
                    //PORT ID�� �����ϼ���
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("PORT ID"));
                    return false;
                }
                if (drDel["AUTO_ISS_REQ_FLAG"].ToString() == string.Empty)
                {
                    // �ڵ�����û�� �����ϼ���
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("�ڵ�����û"));
                    return false;
                }
                if (drDel["ELTR_TYPE_CODE"].ToString() == string.Empty)
                {
                    // �������Ÿ���� �����ϼ���.
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("�������Ÿ��"));
                    return false;
                }
                if (drDel["USE_FLAG"].ToString() == string.Empty)
                {
                    // ��������� �����ϼ���
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("�������"));
                    return false;
                }
                if (drDel["LINK_EQPTID"].ToString() == string.Empty)
                {
                    // ����ݼۼ��� �����ϼ���
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("����ݼۼ���"));
                    return false;
                }
                if (drDel["NEXT_LINK_EQPTID"].ToString() == string.Empty)
                {
                    // �����ݼۼ��� �����ϼ���
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("�����ݼۼ���"));
                    return false;
                }
           
            }

            return true;
        }
    }
}
