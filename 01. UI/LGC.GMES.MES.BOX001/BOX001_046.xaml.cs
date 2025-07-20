using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_046 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_046()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            InitCombo();
        }

        #endregion Declaration & Constructor


        #region Initialize

        private void InitCombo()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("SHOPID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WHID_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboWHID.ItemsSource = DataTableConverter.Convert(dtResult);
            cboWHID.SelectedIndex = 0;
        }

        #endregion Initialize


        #region Event

        private void txtRackID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sRackid = string.Empty;
                    sRackid = txtRackID.Text.Trim();

                    if (sRackid == "")
                    {
                        Util.AlertInfo("SFU2060"); //Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("WH_ID", typeof(string));
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["WH_ID"] = Util.GetCondition(cboWHID);
                    dr["RACK_ID"] = Util.GetCondition(txtRackID);
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RACK", "RQSTDT", "RSLTDT", RQSTDT);

                    txtBoxID.Focus();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    txtRackID.Text = "";
                    txtRackID.Focus();
                }
            }
        }

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sBoxid = string.Empty;
                    sBoxid = txtBoxID.Text.Trim();

                    if (sBoxid == "")
                    {
                        Util.AlertInfo("SFU2060"); //Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    if (Util.GetCondition(txtRackID).Length == 0)
                    {
                        Util.AlertInfo("SFU2843"); //	RACK ID를 먼저 스캔하세요.
                        return;
                    }

                    for (int i = 0; i < dgData.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgData.Rows[i].DataItem, "BOXID")).Equals(sBoxid))
                        {
                            Util.AlertInfo("SFU2014");   //해당 LOT이 이미 존재합니다.
                            txtBoxID.Focus();
                            txtBoxID.Text = "";
                            return;
                        }
                    }

                    //Validation 1 : boxid 출고 유무
                    if (!BoxChk_OUTPUT())
                    {
                        return;
                    }

                    //Validation 2 : boxid 존재 유무
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "INDATA";
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("BOXSTAT", typeof(String));
                    RQSTDT.Columns.Add("LOTID", typeof(String));
                    RQSTDT.Columns.Add("BOXID", typeof(String));
                    RQSTDT.Columns.Add("CSTID", typeof(String));
                    RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));
                    RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                    RQSTDT.Columns.Add("TO_DATE", typeof(String));
                    RQSTDT.Columns.Add("PRODID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["BOXSTAT"] = null;
                    dr["LOTID"] = null;
                    dr["BOXID"] = sBoxid;
                    dr["CSTID"] = null;
                    dr["SHIPTO_ID"] = null;
                    dr["FROM_DATE"] = null;
                    dr["TO_DATE"] = null;
                    dr["PRODID"] = null;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count > 0)
                    {
                        string box_status = BoxChk_RACK();
                        string[] split_text = box_status.Split(':');

                        DataTable dt = new DataTable();

                        switch (split_text[0])
                        {
                            case "NO":
                                Util.AlertInfo("SFU1180");   //	BOX 정보가 없습니다.
                                break;
                            case "Normal":

                                dt = BoxInfo(sBoxid);

                                if (dgData.GetRowCount() == 0)
                                {
                                    dgData.ItemsSource = DataTableConverter.Convert(dt);
                                    Init();
                                }
                                else
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgData.ItemsSource);
                                    dtSource.Merge(dt);

                                    Util.gridClear(dgData);
                                    dgData.ItemsSource = DataTableConverter.Convert(dtSource);
                                }
                                break;

                            case "Same_Rack":                                
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4411", new String[2] { Util.GetCondition(txtBoxID), Util.GetCondition(txtRackID) }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    this.txtBoxID.Clear();
                                    this.txtBoxID.Focus();
                                });

                                break;

                            case "Diff_Rack":
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4410", new String[3] { Util.GetCondition(txtBoxID), split_text[1], Util.GetCondition(txtRackID) }), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                                //BOX[%1] 이미 RACK[%2]에 입고 되어 있습니다. RACK[%3]로 변경하시겠습니까?
                                {
                                    if (caution_result == MessageBoxResult.OK)
                                    {
                                        dt = BoxInfo(sBoxid);

                                        if (dgData.GetRowCount() == 0)
                                        {
                                            dgData.ItemsSource = DataTableConverter.Convert(dt);
                                            Init();
                                        }
                                        else
                                        {
                                            DataTable dtSource = DataTableConverter.Convert(dgData.ItemsSource);
                                            dtSource.Merge(dt);

                                            Util.gridClear(dgData);
                                            dgData.ItemsSource = DataTableConverter.Convert(dtSource);
                                        }
                                    }

                                    txtBoxID.Text = "";
                                    txtBoxID.Focus();
                                }
                                );
                                break;
                            default:
                                break;
                        }

                        DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                        DataGridAggregatesCollection daq = new DataGridAggregatesCollection();
                        DataGridAggregateCount dgcount = new DataGridAggregateCount();
                        DataGridAggregateSum dagsum = new DataGridAggregateSum();
                        dagsum.ResultTemplate = dgData.Resources["ResultTemplate"] as DataTemplate;
                        daq.Add(dgcount);
                        dac.Add(dagsum);
                        DataGridAggregate.SetAggregateFunctions(dgData.Columns["PRODID"], daq);
                        DataGridAggregate.SetAggregateFunctions(dgData.Columns["TOTAL_QTY"], dac);

                        txtBoxID.Focus();
                        txtBoxID.Text = string.Empty;
                    }
                    else
                    {
                        Util.AlertInfo("SFU1180");   //	BOX 정보가 없습니다.
                        txtBoxID.Focus();
                        txtBoxID.Text = string.Empty;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            Init();
            Initialize_dgData();
        }

        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgData.GetRowCount() == 0)
                {
                    Util.AlertInfo("SFU2060");   //스캔한 데이터가 없습니다.
                    return;
                }

                if (Util.GetCondition(txtRackID).Length == 0)
                {
                    Util.AlertInfo("SFU2843"); // 	RACK ID를 먼저 스캔하세요.
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("RACK_ID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("BOXID", typeof(string));
                inData.Columns.Add("WH_ID", typeof(string));

                for (int i = 0; i < dgData.GetRowCount(); i++)
                {
                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["RACK_ID"] = Util.NVC(Util.GetCondition(txtRackID));
                    row["USERID"] = LoginInfo.USERID;
                    row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgData.Rows[i].DataItem, "BOXID"));
                    row["WH_ID"] = Util.GetCondition(cboWHID);
                    inData.Rows.Add(row);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_MOVE_FOR_BOX", "INDATA", null, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.AlertInfo("SFU1798");   //입고 처리 되었습니다.
                    Initialize_dgData();

                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion Event


        #region Method

        private void Init()
        {
            txtBoxID.Text = "";
            txtBoxID.Focus();
        }

        private void Initialize_dgData()
        {
            Util.gridClear(dgData);
            Init();
        }

        private DataTable BoxInfo(String BOXID)
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("BOXID", typeof(String));

                DataRow dr = INDATA.NewRow();
                dr["BOXID"] = BOXID;
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXID", "INDATA", "OUTDATA", INDATA);

                return dtResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool BoxChk_OUTPUT()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("BOXID", typeof(String));

                DataRow dr = INDATA.NewRow();
                dr["BOXID"] = Util.GetCondition(txtBoxID);
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_FOR_ISSUE", "INDATA", "OUTDATA", INDATA);

                if (dtResult == null || dtResult.Rows.Count == 0 || dtResult.Rows[0]["BOX_RCV_ISS_STAT_CODE"].Equals("CANCEL_SHIP"))
                {
                    return true;
                }
                else
                {
                    //출고 이력이 존재합니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3018", new object[] { txtBoxID.Text }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtBoxID.Clear();
                        this.txtBoxID.Focus();
                    });
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string BoxChk_RACK()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("BOXID", typeof(String));

                DataRow dr = INDATA.NewRow();
                dr["BOXID"] = Util.GetCondition(txtBoxID);
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_WH_ID", "INDATA", "OUTDATA", INDATA);

                if (dtResult.Rows.Count == 0) //정상 입고(해당 box가 입고된 이력이 없음)
                {
                    return "NO:";
                }
                else
                {
                    if (dtResult.Rows[0]["RACK_ID"].ToString() == "")
                    {
                        return "Normal:";
                    }
                    else if (Util.GetCondition(txtRackID).Equals(dtResult.Rows[0]["RACK_ID"].ToString()))
                    {
                        return "Same_Rack:";
                    }
                    else
                    {
                        return "Diff_Rack:" + dtResult.Rows[0]["RACK_ID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Method        
    }
}
