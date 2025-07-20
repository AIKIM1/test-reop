/*************************************************************************************
 Created Date : 2017.06.17
      Creator :  
   Decription : CWA Steel Case 구성
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.17  DEVELOPER : CWA Steel Case 구성 (기존 SKID구성 복사)
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media; 


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_307 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        private bool IsMobileAssy = false;
        Util _Util = new Util();
        public DataTable dtPackingCard;
        int checkIndex;

        public COM001_307()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            ApplyPermissions();
            SetMobileAssy();
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnNewSkid);
            listAuth.Add(btnSkidSearch);

            listAuth.Add(btnNewSkid);
            listAuth.Add(btnLotidSearch);
            listAuth.Add(btnAdd);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            SKID_ID.Focus();
        }
        #endregion

        #region #Steel Case
        private void btnNewSkid_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSkidList);
            Util.gridClear(dgLotList);
            SKID_ID.Text = "";
            SkidLotID.Text = "";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int dgCheck = 0;
                int dgCheck2 = 0;

                if (dgSkidList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU7010"); //Steel Case를 구성할 Lot이 존재하지 않습니다.
                    return;
                }

                if (dgSkidList.Rows.Count > 1)
                {
                    for (int i = 0; i < dgSkidList.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dgSkidList.Rows[0].DataItem, "AREAID").ToString() != DataTableConverter.GetValue(dgSkidList.Rows[i].DataItem, "AREAID").ToString())
                        {
                            Util.MessageValidation("SFU4998", DataTableConverter.GetValue(dgSkidList.Rows[0].DataItem, "LOTID").ToString(), DataTableConverter.GetValue(dgSkidList.Rows[i].DataItem, "LOTID").ToString());
                            return;
                        }
                    }
                }

                DataTable dt6 = ((DataView)dgSkidList.ItemsSource).Table;

                if (dgSkidList.Rows.Count >= 1)
                {
                    foreach (DataRow row in dt6.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]) && !Util.NVC(row["LOCATION"]).Equals(""))
                            dgCheck++;
                    }
                }


                var query = from rows in dt6.AsEnumerable()
                            group rows by rows.Field<string>("PRODID") into g
                            select new { prodCount = g.Count() };

                if (query.Count() > 1)
                {
                    Util.MessageValidation("SFU7011"); // 같은 제품만 Steel Case 구성 할 수 있습니다.
                    return;
                }

                var query_mkt = from rows in dt6.AsEnumerable()
                                group rows by rows.Field<string>("MKT_TYPE_CODE") into g
                                select new { prodCount = g.Count() };

                if (query_mkt.Count() > 1)
                {
                    Util.MessageValidation("SFU7012"); // 내수용과 수출품은 동일한 Steel Case로 구성이 불가능합니다.
                    return;
                }

                if (dgLotList.ItemsSource != null)
                {
                    DataTable dt7 = ((DataView)dgLotList.ItemsSource).Table;
                    if (dgLotList.Rows.Count >= 1)
                    {
                        foreach (DataRow row in dt7.Rows)
                        {
                            if (Convert.ToBoolean(row["CHK"]) && !Util.NVC(row["LOCATION"]).Equals(""))
                                dgCheck2++;
                        }
                    }
                }
                else
                {
                    Util.MessageValidation("SFU2906"); //선택된 정보나 변경된 정보가 없습니다.
                    //SKID_ID.Text = "";
                    return;
                }

                if (dgCheck + dgCheck2 == 0)
                {
                    Util.MessageValidation("SFU2906"); //선택된 정보나 변경된 정보가 없습니다.
                    //SKID_ID.Text = "";
                    return;
                }

                if (dgCheck >= 1)
                {
                    //Steel Case가 저장 됩니다. 계속 하시겠습니까?
                    Util.MessageConfirm("SFU7013", (sResult) =>
                    {
                        if (sResult == MessageBoxResult.OK)
                            SetSkid("A");
                    });
                }

                if (dgCheck2 >= 1)
                {
                    //추출한 데이터가 삭제됩니다. 계속 하시겠습니까?
                    Util.MessageConfirm("SFU3220", (sResult) =>
                    {
                        if (sResult == MessageBoxResult.OK)
                            SetSkid("D");
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetSkid(string PROC_TYPE)
        {
            try
            {
                if (PROC_TYPE.Equals("A"))
                {
                    DataSet inDataSet = new DataSet();
                    DataTable dt = ((DataView)dgSkidList.ItemsSource).Table;

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");

                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("ISSUE_FLAG", typeof(string));
                    inDataTable.Columns.Add("SKID_ID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow inDataRow = null;

                    inDataRow = inDataTable.NewRow();
                    inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inDataRow["USERID"] = LoginInfo.USERID;

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["LOCATION"]).Equals(""))
                            {
                                if (Util.NVC(row["LOCATION"]).Equals("LO"))
                                {
                                    if (row["SKID_ID"].Equals(""))
                                    {
                                        inDataRow["ISSUE_FLAG"] = "Y";
                                        inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                        break;
                                    }
                                    else
                                    {
                                        inDataRow["ISSUE_FLAG"] = "N";
                                        inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    inDataSet.Tables["INDATA"].Rows.Add(inDataRow);
                    DataTable InSkidDataTable = inDataSet.Tables.Add("IN_LOT");
                    DataRow inSkidDataRow = null;
                    InSkidDataTable.Columns.Add("LOTID", typeof(string));
                    InSkidDataTable.Columns.Add("PRODID", typeof(string));
                    InSkidDataTable.Columns.Add("CUT_ID", typeof(string));
                    InSkidDataTable.Columns.Add("INPUT_TYPE", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["LOCATION"]).Equals(""))
                            {

                                inSkidDataRow = InSkidDataTable.NewRow();
                                inSkidDataRow["LOTID"] = Util.NVC(row["LOTID"]);
                                inSkidDataRow["PRODID"] = Util.NVC(row["PRODID"]);
                                inSkidDataRow["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                                inSkidDataRow["INPUT_TYPE"] = Util.NVC(row["INPUTFLAG"]);

                                inDataSet.Tables["IN_LOT"].Rows.Add(inSkidDataRow);

                            }
                        }
                    }
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_SKID", "INDATA,IN_LOT", "OUTDATA", inDataSet);

                        if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            SKID_ID.Text = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();
                            Print(SKID_ID.Text.ToString());
                        }
                        else
                        {
                            //SKID_ID.Text = "";
                            Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.

                            return;
                        }


                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                        //SKID_ID.Text = "";

                        return;
                    }
                }

                if (PROC_TYPE.Equals("D"))
                {
                    DataSet inDataSet = new DataSet();
                    DataTable dt = ((DataView)dgLotList.ItemsSource).Table;

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");

                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("ISSUE_FLAG", typeof(string));
                    inDataTable.Columns.Add("SKID_ID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow inDataRow = null;

                    inDataRow = inDataTable.NewRow();
                    inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inDataRow["USERID"] = LoginInfo.USERID;

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["LOCATION"]).Equals(""))
                            {
                                if (Util.NVC(row["LOCATION"]).Equals("HI"))
                                {
                                    inDataRow["ISSUE_FLAG"] = "N";
                                    inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                    break;
                                }
                            }
                        }
                    }

                    inDataSet.Tables["INDATA"].Rows.Add(inDataRow);
                    DataTable InSkidDataTable = inDataSet.Tables.Add("IN_LOT");
                    DataRow inSkidDataRow = null;
                    InSkidDataTable.Columns.Add("LOTID", typeof(string));
                    InSkidDataTable.Columns.Add("PRODID", typeof(string));
                    InSkidDataTable.Columns.Add("CUT_ID", typeof(string));
                    InSkidDataTable.Columns.Add("INPUT_TYPE", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["LOCATION"]).Equals(""))
                            {

                                inSkidDataRow = InSkidDataTable.NewRow();
                                inSkidDataRow["LOTID"] = Util.NVC(row["LOTID"]);
                                inSkidDataRow["PRODID"] = Util.NVC(row["PRODID"]);
                                inSkidDataRow["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                                inSkidDataRow["INPUT_TYPE"] = Util.NVC(row["INPUTFLAG"]);

                                inDataSet.Tables["IN_LOT"].Rows.Add(inSkidDataRow);

                            }
                        }
                    }
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_SKID", "INDATA,IN_LOT", "OUTDATA", inDataSet);

                        if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            SKID_ID.Text = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();
                            //btnSkidSearchs();
                            Print(SKID_ID.Text.ToString());
                            //SKID_ID.Text = "";

                        }
                        else
                        {
                            Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.
                            //SKID_ID.Text = "";

                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //SKID_ID.Text = "";

                        return;
                    }


                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Print(string sSkid_ID)
        {
            try
            {
                dtPackingCard = new DataTable();

                dtPackingCard.Columns.Add("Title", typeof(string));
                dtPackingCard.Columns.Add("SKID_ID", typeof(string));
                dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                dtPackingCard.Columns.Add("PROJECT", typeof(string));
                dtPackingCard.Columns.Add("PRODID", typeof(string));
                dtPackingCard.Columns.Add("VER", typeof(string));
                dtPackingCard.Columns.Add("QTY", typeof(string));
                dtPackingCard.Columns.Add("UNIT", typeof(string));
                dtPackingCard.Columns.Add("PRODDATE", typeof(string));
                dtPackingCard.Columns.Add("VLDDATE", typeof(string));
                dtPackingCard.Columns.Add("TOTAL_QTY", typeof(string));

                DataRow drCrad = null;

                drCrad = dtPackingCard.NewRow();

                drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극STEELCASE구성카드"),
                                                  sSkid_ID,
                                                  sSkid_ID,
                                                  ObjectDic.Instance.GetObjectName("PJT"),
                                                  ObjectDic.Instance.GetObjectName("반제품"),
                                                  ObjectDic.Instance.GetObjectName("버전"),
                                                  ObjectDic.Instance.GetObjectName("수량"),
                                                  ObjectDic.Instance.GetObjectName("단위"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  ObjectDic.Instance.GetObjectName("유효기간"),
                                                  ObjectDic.Instance.GetObjectName("총 수량")
                                               };

                dtPackingCard.Rows.Add(drCrad);

                LGC.GMES.MES.COM001.Report_SteelCase rs = new LGC.GMES.MES.COM001.Report_SteelCase();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[3];
                    Parameters[0] = "Report_SteelCase";
                    Parameters[1] = sSkid_ID;
                    Parameters[2] = dtPackingCard;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(Print_Result);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_SteelCase wndPopup = sender as LGC.GMES.MES.COM001.Report_SteelCase;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    btnSkidSearchs();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSkidOut_Click(object sender, RoutedEventArgs e)
        {
            //아래로 이동
            if (dgSkidList.Rows.Count <= 0)
                return;

            DataTable dt6 = ((DataView)dgSkidList.ItemsSource).Table;

            DataTable dt7 = new DataTable();

            if (dgLotList.Rows.Count >= 1)
            {
                dt7 = ((DataView)dgLotList.ItemsSource).Table;
            }
            else
            {
                dt7.TableName = "dgLotList";
                dt7.Columns.Add("CHK", typeof(String));
                dt7.Columns.Add("LOTID", typeof(String));
                dt7.Columns.Add("PRODID", typeof(String));
                dt7.Columns.Add("PRODNAME", typeof(String));
                dt7.Columns.Add("MODLID", typeof(String));
                dt7.Columns.Add("AREAID", typeof(String));
                dt7.Columns.Add("EQSGID", typeof(String));
                dt7.Columns.Add("PROCID", typeof(String));
                dt7.Columns.Add("WIPQTY", typeof(String));
                dt7.Columns.Add("CUT_ID", typeof(String));
                dt7.Columns.Add("SKID_ID", typeof(String));
                dt7.Columns.Add("INPUTFLAG", typeof(String));
                dt7.Columns.Add("LOCATION", typeof(String));
                dt7.Columns.Add("MKT_TYPE_CODE", typeof(String));
                dt7.Columns.Add("MKT_TYPE_NAME", typeof(String));
            }
            foreach (DataRow row in dt6.Rows)
            {
                if (Convert.ToBoolean(row["CHK"]))
                {
                    if (!Util.NVC(row["LOTID"]).Equals(""))
                    {
                        DataRow dr = dt7.NewRow();
                        dr["CHK"] = true;
                        dr["LOTID"] = Util.NVC(row["LOTID"]);
                        dr["PRODID"] = Util.NVC(row["PRODID"]);
                        dr["PRODNAME"] = Util.NVC(row["PRODNAME"]);
                        dr["MODLID"] = Util.NVC(row["MODLID"]);
                        dr["AREAID"] = Util.NVC(row["AREAID"]);
                        dr["EQSGID"] = Util.NVC(row["EQSGID"]);
                        dr["PROCID"] = Util.NVC(row["PROCID"]);
                        dr["WIPQTY"] = Util.NVC(row["WIPQTY"]);
                        dr["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                        dr["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                        if (Util.NVC(row["INPUTFLAG"]).Equals("A"))
                        {
                            dr["INPUTFLAG"] = "";
                        }
                        else
                        {
                            dr["INPUTFLAG"] = "D";
                        }
                        if (Util.NVC(row["LOCATION"]).Equals("LO"))
                        {
                            dr["LOCATION"] = "";
                        }
                        else
                        {
                            dr["LOCATION"] = "HI";
                        }
                        dr["MKT_TYPE_CODE"] = Util.NVC(row["MKT_TYPE_CODE"]);
                        dr["MKT_TYPE_NAME"] = Util.NVC(row["MKT_TYPE_NAME"]);
                        dt7.Rows.Add(dr);
                    }
                }
            }
            Util.GridSetData(dgLotList, dt7, null, false);
            dgLotList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);

            for (int i = dgSkidList.GetRowCount(); i >= 0; i--)
            {
                DataTable dt = DataTableConverter.Convert(dgSkidList.ItemsSource);
                if (_Util.GetDataGridCheckValue(dgSkidList, "CHK", i) == true)
                {
                    dt.Rows[i].Delete();
                }
                dgSkidList.ItemsSource = DataTableConverter.Convert(dt);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //위로 이동
            if (dgLotList.Rows.Count <= 0)
                return;

            //추가2
            SKID_ID.Clear();
            DataTable dt7 = ((DataView)dgLotList.ItemsSource).Table;

            DataTable dt6 = new DataTable();

            if (dgSkidList.Rows.Count >= 1)
            {
                dt6 = ((DataView)dgSkidList.ItemsSource).Table;
            }
            else
            {
                dt6.TableName = "dgSkidList";
                dt6.Columns.Add("CHK", typeof(String));
                dt6.Columns.Add("SKID_ID", typeof(String));
                dt6.Columns.Add("LOTID", typeof(String));
                dt6.Columns.Add("PRODID", typeof(String));
                dt6.Columns.Add("PRODNAME", typeof(String));
                dt6.Columns.Add("MODLID", typeof(String));
                dt6.Columns.Add("AREAID", typeof(String));
                dt6.Columns.Add("EQSGID", typeof(String));
                dt6.Columns.Add("PROCID", typeof(String));
                dt6.Columns.Add("WIPQTY", typeof(String));
                dt6.Columns.Add("CUT_ID", typeof(String));
                dt6.Columns.Add("INPUTFLAG", typeof(String));
                dt6.Columns.Add("LOCATION", typeof(String));
                dt6.Columns.Add("MKT_TYPE_CODE", typeof(String));
                dt6.Columns.Add("MKT_TYPE_NAME", typeof(String));
            }
            foreach (DataRow row2 in dt7.Rows)
            {
                if (Convert.ToBoolean(row2["CHK"]))
                {
                    if (!Util.NVC(row2["LOTID"]).Equals(""))
                    {
                        DataRow dr = dt6.NewRow();
                        dr["CHK"] = true;
                        dr["SKID_ID"] = SKID_ID.Text;
                        dr["LOTID"] = Util.NVC(row2["LOTID"]);
                        dr["PRODID"] = Util.NVC(row2["PRODID"]);
                        dr["PRODNAME"] = Util.NVC(row2["PRODNAME"]);
                        dr["MODLID"] = Util.NVC(row2["MODLID"]);
                        dr["AREAID"] = Util.NVC(row2["AREAID"]);
                        dr["EQSGID"] = Util.NVC(row2["EQSGID"]);
                        dr["PROCID"] = Util.NVC(row2["PROCID"]);
                        dr["WIPQTY"] = Util.NVC(row2["WIPQTY"]);
                        dr["CUT_ID"] = Util.NVC(row2["CUT_ID"]);
                        dr["MKT_TYPE_CODE"] = Util.NVC(row2["MKT_TYPE_CODE"]);
                        dr["MKT_TYPE_NAME"] = Util.NVC(row2["MKT_TYPE_NAME"]);
                        if (Util.NVC(row2["INPUTFLAG"]).Equals("D"))
                        {
                            dr["INPUTFLAG"] = "";
                        }
                        else
                        {
                            dr["INPUTFLAG"] = "A";
                        }
                        if (Util.NVC(row2["LOCATION"]).Equals("HI"))
                        {
                            dr["LOCATION"] = "";
                        }
                        else
                        {
                            dr["LOCATION"] = "LO";
                        }

                        dt6.Rows.Add(dr);
                    }
                }
            }

            Util.GridSetData(dgSkidList, dt6, null, false);
            dgSkidList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);

            for (int i = dgLotList.GetRowCount(); i >= 0; i--)
            {
                DataTable dt = DataTableConverter.Convert(dgLotList.ItemsSource);
                if (_Util.GetDataGridCheckValue(dgLotList, "CHK", i) == true)
                {
                    dt.Rows[i].Delete();
                }
                dgLotList.ItemsSource = DataTableConverter.Convert(dt);
            }
            SkidLotID.Text = "";
        }

        private void skid_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSkidSearch_Click(sender, e);
            }
        }

        private void SkidLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLotidSearch_Click(sender, e);
            }
        }

        private void btnLotidSearch_Click(object sender, RoutedEventArgs e)
        {  
            btnLotSearchs();           
        }

        private void GridSet_Checked()
        {
            if (dgLotList.Rows.Count > 0)
            {
                for (int i = 0; i < dgLotList.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "CHK", true);
                    dgLotList.ScrollIntoView(i, dgLotList.Columns["CHK"].Index);
                }
            }
        }

        private void btnSkidSearch_Click(object sender, RoutedEventArgs e)
        {     
            btnSkidSearchs();  
        }

        private bool NJ_AreaChk()
        {
            bool area_chk = true;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACKING_VERSION_CHECK_AREA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0 && LoginInfo.CFG_AREA_ID.Equals(dtResult.Rows[0]["CBO_CODE"]))
                {
                    area_chk =  true;
                }
                else
                {
                    area_chk =  false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return area_chk;
        }

        private bool lotVerChk()
        {
            bool ver_chk = true;

            return ver_chk;
        }

        private void btnSkidSearchs()
        {
            try
            {
                //if (SKID_ID.Text.ToString() == "")
                //{
                //    Util.MessageValidation("SFU2934");  //입력한 Skid ID 가 없습니다.
                //    return;
                //}

                // SKID Grid Data 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("SKID_ID", typeof(String));
                RQSTDT.Columns.Add("DateFrom", typeof(String));
                RQSTDT.Columns.Add("DateTo", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                if (SKID_ID.Text.ToString().Trim().Equals(""))
                {

                }
                else
                {
                    dr["SKID_ID"] = SKID_ID.Text.ToString().Trim();
                }
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["DateFrom"] = Util.GetCondition(dtpDateFrom);
                //dr["DateTo"] = Util.GetCondition(dtpDateTo);

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU7014");   //Steel Case 정보가 없습니다.
                    //SKID_ID.Text = "";
                    return;
                }
                else
                {
                    for (int i = 0; i < Result.Rows.Count; i++)
                    {
                        if (!string.Equals(Result.Rows[0]["BOXID"].ToString(), ""))
                        {
                            Util.MessageValidation("SFU2978"); //포장완료한 이력이 있습니다.
                            return;
                        }
                    }
                    Util.gridClear(dgSkidList);
                    Util.GridSetData(dgSkidList, Result, null, false);
                    dgSkidList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    //SKID_ID.Text = "";                
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //SKID_ID.Text = "";
                return;
            }
        }

        private void btnLotSearchs()
        {
            try
            {
                if (SkidLotID.Text.ToString() == "")
                {
                    Util.MessageValidation("SFU1366");  //LOT ID를 입력해주세요
                    return;
                }

                // SKID Grid Data 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = SkidLotID.Text.ToString().Trim();

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1195");  //Lot 정보가 없습니다.
                    SkidLotID.Text = "";
                    return;
                }
                else
                {
                    for (int i = 0; i < Result.Rows.Count; i++)
                    {
                        if (!string.Equals(Result.Rows[0]["BOXID"].ToString(), ""))
                        {
                            Util.MessageValidation("SFU2978"); //포장완료한 이력이 있습니다.
                            return;
                        }
                    }

                    if (IsMobileAssy == true)
                    {
                        string sReturnLine = GetReturnLine(SkidLotID.Text.ToString().Trim());

                        Result.Columns.Add("RTLOCATION");
                        Result.Rows[0]["RTLOCATION"] = sReturnLine;
                    }

                    if (dgLotList.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgLotList, Result, FrameOperation);
                    }
                    else
                    {
                        for (int i = 0; i < dgLotList.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").ToString() == SkidLotID.Text.ToString())
                            {
                                Util.Alert("SFU1914");   //중복 스캔되었습니다.
                                return;
                            }

                            if (Result.Rows[0]["MKT_TYPE_CODE"].ToString() != DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "MKT_TYPE_CODE").ToString())
                            {
                                Util.Alert("SFU7012");   //내수용과 수출품은 동일한 Steel Case로 구성이 불가능합니다.
                                return;
                            }

                            if (Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PRODID").ToString())
                            {
                                Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                                return;
                            }

                            if (IsMobileAssy == true)
                            {
                                if (!string.Equals(Result.Rows[0]["RTLOCATION"], DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "RTLOCATION")))
                                {
                                    Util.Alert("SFU4039");   //반품동이 동일한 LOT이 아닙니다.
                                    return;
                                }
                            }

                            if (NJ_AreaChk())
                            {
                                if (DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "PROD_VER_CODE").ToString() != Result.Rows[0]["PROD_VER_CODE"].ToString())
                                {
                                    Util.Alert("SFU1501");   //동일 버전이 아닙니다.
                                    return;
                                }
                            }
                        }

                        dgLotList.IsReadOnly = false;
                        dgLotList.BeginNewRow();
                        dgLotList.EndNewRow(true);
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CHK", Result.Rows[0]["CHK"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "LOTID", Result.Rows[0]["LOTID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "PRODID", Result.Rows[0]["PRODID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "PRODNAME", Result.Rows[0]["PRODNAME"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "MODLID", Result.Rows[0]["MODLID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "AREAID", Result.Rows[0]["AREAID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "PRODID", Result.Rows[0]["PRODID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "PRODNAME", Result.Rows[0]["PRODNAME"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "MODLID", Result.Rows[0]["MODLID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "EQSGID", Result.Rows[0]["EQSGID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "PROCID", Result.Rows[0]["PROCID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "WIPQTY", Result.Rows[0]["WIPQTY"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "CUT_ID", Result.Rows[0]["CUT_ID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "SKID_ID", Result.Rows[0]["SKID_ID"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "INPUTFLAG", Result.Rows[0]["INPUTFLAG"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "LOCATION", Result.Rows[0]["LOCATION"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "MKT_TYPE_CODE", Result.Rows[0]["MKT_TYPE_CODE"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "MKT_TYPE_NAME", Result.Rows[0]["MKT_TYPE_NAME"].ToString());
                        DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "PROD_VER_CODE", Result.Rows[0]["PROD_VER_CODE"].ToString());

                        if (IsMobileAssy == true)
                            DataTableConverter.SetValue(dgLotList.CurrentRow.DataItem, "RTLOCATION", Result.Rows[0]["RTLOCATION"].ToString());

                        dgLotList.IsReadOnly = true;
                    }
                    GridSet_Checked();
                    SkidLotID.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

        // 체크박스 관련
        #region
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        private void dgSkidList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dgSkidList.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgLotList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dgLotList.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgSkidList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgSkidList.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        private void dgLotList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }
        #endregion

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            if (SKID_ID.Text.ToString() == "")
            {
                Util.MessageValidation("SFU7014");  //Steel Case 정보가 없습니다.
                return;
            }

            if (sKidValidation(SKID_ID.Text.ToString().Trim()))
            {
                Print(SKID_ID.Text.ToString());
            }

        }

        private bool sKidValidation(string Skid_id)
        {
            // SKID Grid Data 조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SKID_ID", typeof(String));

            DataRow dr = RQSTDT.NewRow();

            dr["SKID_ID"] = Skid_id;

            RQSTDT.Rows.Add(dr);

            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

            if (Result.Rows.Count == 0)
            {
                Util.MessageValidation("SFU7014");   //Steel Case 정보가 없습니다.
                return false;
            }
            return true;
        }

        private void btnCutSkid_Click(object sender, RoutedEventArgs e)
        {
            if (dgSkidList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU7015");  //입력한 Steel Case ID 가 없습니다.
                return;
            }

            if (SKID_ID.Text.ToString() == "")
            {
                int chk = 0;
                DataSet inDataSet = new DataSet();
                DataTable dt = ((DataView)dgSkidList.ItemsSource).Table;

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("ISSUE_FLAG", typeof(string));
                inDataTable.Columns.Add("SKID_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataRow["USERID"] = LoginInfo.USERID;

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["LOCATION"]).Equals(""))
                        {
                            if (Util.NVC(row["LOCATION"]).Equals("LO"))
                            {
                                chk++;
                                if (row["SKID_ID"].Equals(""))
                                {
                                    inDataRow["ISSUE_FLAG"] = "Y";
                                    inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                    break;
                                }
                                else
                                {
                                    inDataRow["ISSUE_FLAG"] = "N";
                                    inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                    break;
                                }
                            }
                        }
                    }
                }

                inDataSet.Tables["INDATA"].Rows.Add(inDataRow);
                DataTable InSkidDataTable = inDataSet.Tables.Add("IN_LOT");
                DataRow inSkidDataRow = null;
                InSkidDataTable.Columns.Add("LOTID", typeof(string));
                InSkidDataTable.Columns.Add("PRODID", typeof(string));
                InSkidDataTable.Columns.Add("CUT_ID", typeof(string));
                InSkidDataTable.Columns.Add("INPUT_TYPE", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["LOCATION"]).Equals(""))
                        {

                            inSkidDataRow = InSkidDataTable.NewRow();
                            inSkidDataRow["LOTID"] = Util.NVC(row["LOTID"]);
                            inSkidDataRow["PRODID"] = Util.NVC(row["PRODID"]);
                            inSkidDataRow["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                            inSkidDataRow["INPUT_TYPE"] = Util.NVC(row["INPUTFLAG"]);

                            inDataSet.Tables["IN_LOT"].Rows.Add(inSkidDataRow);
                        }
                    }
                }

                if (chk != 0)
                {
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_SKID", "INDATA,IN_LOT", "OUTDATA", inDataSet);

                        if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            SKID_ID.Text = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();
                        }
                        else
                        {
                            Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.
                            return;
                        }

                        // SKID Grid Data 조회
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("SKID_ID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["SKID_ID"] = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();

                        RQSTDT.Rows.Add(dr);

                        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

                        if (Result.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU7014");   //Steel Case 정보가 없습니다.
                            return;
                        }
                        else
                        {
                            Util.gridClear(dgSkidList);
                            Util.GridSetData(dgSkidList, Result, null, false);
                            dgSkidList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                            checkAll_Checked(null, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                }
            }

            // Skid ID 이미 재발행된 상태에서 Skid 분리를 사용하면 0 + Alphabet으로 표현되어 Naming Rule에서 SEQ를 가져올 수 없어 오류가 발생함
            // 그래서 신규 Skid ID가 발행된 상태에서 Skid 분리 버튼 사용을 금지시킴 [2018-08-21]
            if( !string.IsNullOrEmpty(SKID_ID.Text.ToString()) && SKID_ID.Text.ToString().Length == 9)
            {
                if (string.Equals(SKID_ID.Text.ToString().Substring(7, 1), "0" ))
                {
                    Util.MessageValidation("SFU7016");  //이미 재발행된 Steel Case에서는 Steel Case분리를 사용할 수 없습니다.(분리하실 Pancake를 추가하여 저장하시기 바랍니다.)
                    return;
                }
            }

            Util.MessageConfirm("SFU7017", (sResult) => // Steel Case ID를 분리 하시겠습니까?
            {
                if (sResult == MessageBoxResult.OK)
                    CutSkid(SKID_ID.Text.ToString());
            });
        }

        private void CutSkid(string sSkidID)
        {

            if (SKID_ID.Text.ToString() == "")
            {
                Util.MessageValidation("SFU7015");  //입력한 Steel Case ID 가 없습니다.
                return;
            }

            try
            {
                string sSkidID2 = sSkidID.Substring(0, 8).ToString() + Convert.ToChar((Convert.ToInt64(Util.NVC(sSkidID.Substring(8, 1)), 16)) + 65).ToString();
                int dtchk = 0;
                DataSet inDataSet2 = new DataSet();
                DataTable dt2 = ((DataView)dgSkidList.ItemsSource).Table;

                DataTable inDataTable2 = inDataSet2.Tables.Add("INDATA");

                inDataTable2.Columns.Add("SRCTYPE", typeof(string));
                inDataTable2.Columns.Add("AREAID", typeof(string));
                inDataTable2.Columns.Add("ISSUE_FLAG", typeof(string));
                inDataTable2.Columns.Add("SKID_ID", typeof(string));
                inDataTable2.Columns.Add("USERID", typeof(string));

                DataRow inDataRow2 = null;

                inDataRow2 = inDataTable2.NewRow();
                inDataRow2["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow2["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataRow2["USERID"] = LoginInfo.USERID;
                inDataRow2["ISSUE_FLAG"] = "N";
                inDataRow2["SKID_ID"] = sSkidID2;

                inDataSet2.Tables["INDATA"].Rows.Add(inDataRow2);
                DataTable InSkidDataTable2 = inDataSet2.Tables.Add("IN_LOT");
                DataRow inSkidDataRow2 = null;
                InSkidDataTable2.Columns.Add("LOTID", typeof(string));
                InSkidDataTable2.Columns.Add("PRODID", typeof(string));
                InSkidDataTable2.Columns.Add("CUT_ID", typeof(string));
                InSkidDataTable2.Columns.Add("INPUT_TYPE", typeof(string));

                foreach (DataRow row in dt2.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        long i = 0;
                        char[] code = Util.NVC(row["LOTID"]).Substring(9, 1).ToCharArray(0, 1);
                        if (code[0] > 64)
                        {
                            i = Convert.ToInt64(Util.NVC(code[0] - 48)) - 1;
                        }
                        else
                        {
                            i = Convert.ToInt64(Util.NVC(code[0] - 48));
                        }
                        if ((Convert.ToInt64(i % 2) == 0))
                        {
                            inSkidDataRow2 = InSkidDataTable2.NewRow();
                            inSkidDataRow2["LOTID"] = Util.NVC(row["LOTID"]);
                            inSkidDataRow2["PRODID"] = Util.NVC(row["PRODID"]);
                            inSkidDataRow2["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                            inSkidDataRow2["INPUT_TYPE"] = "A";

                            inDataSet2.Tables["IN_LOT"].Rows.Add(inSkidDataRow2);
                            dtchk++;
                        }
                    }
                }
                if (dtchk == 0)
                {
                    Util.MessageValidation("SFU2906"); //선택된 정보나 변경된 정보가 없습니다.
                    return;
                }
                try
                {
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_SKID", "INDATA,IN_LOT", "OUTDATA", inDataSet2);

                    if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        //SKID_ID.Text = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();
                        checkAll_Unchecked(null, null);
                        Print(sSkidID);
                        Print(sSkidID2);
                    }
                    else
                    {
                        Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.
                        return;
                    }
                }
                catch (Exception ex)
                {

                    Util.MessageException(ex);
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSkidList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        //pre.Content = new HorizontalContentAlignment = "Center";
                        e.Column.HeaderPresenter.Content = pre;

                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgSkidList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgSkidList.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgSkidList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgSkidList.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        }


        private void btnCSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // SKID Grid Data 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CSTID", typeof(String));
                RQSTDT.Columns.Add("DATEFROM", typeof(String));
                RQSTDT.Columns.Add("DATETO", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                if (SKID_ID2.Text.ToString().Trim().Equals(""))
                {
                    dr["DATEFROM"] = Util.GetCondition(dtpDateFrom);
                    dr["DATETO"] = Util.GetCondition(dtpDateTo);
                }
                else
                {
                    dr["CSTID"] = SKID_ID2.Text.ToString().Trim();
                }

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CST_CONF_HIST_V01", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU7014");   //Steel Case 정보가 없습니다.
                    return;
                }
                else
                {
                    Util.gridClear(dgHistList);
                    Util.GridSetData(dgHistList, Result, null, false);
                    dgHistList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //SKID_ID.Text = "";
                return;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                {
                    return;
                }
                if (dgHistList.GetRowCount() < 0)
                {
                    return;
                }

                checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;
                DataTableConverter.SetValue(dgHistList.Rows[checkIndex].DataItem, "CHK", true);
                dgHistList.SelectedIndex = checkIndex;

                for (int i = 0; i < dgHistList.Rows.Count; i++)
                {
                    if (i != checkIndex)
                    {
                        DataTableConverter.SetValue(dgHistList.Rows[i].DataItem, "CHK", false);
                    }
                    else
                    {
                        // SKID 상세 Data 조회
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("CSTID", typeof(String));
                        RQSTDT.Columns.Add("ACTDTTM", typeof(DateTime));

                        DataRow dr = RQSTDT.NewRow();
                        dr["CSTID"] = DataTableConverter.GetValue(dgHistList.Rows[checkIndex].DataItem, "CSTID").ToString().Trim();
                        dr["ACTDTTM"] = DataTableConverter.GetValue(dgHistList.Rows[checkIndex].DataItem, "ACTDTTM").ToString().Trim();

                        RQSTDT.Rows.Add(dr);

                        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CST_CONF_HIST_DETL", "RQSTDT", "RSLTDT", RQSTDT);

                        if (Result.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU7014");   //Steel Case 정보가 없습니다.
                            return;
                        }
                        else
                        {
                            Util.gridClear(dgHistList2);
                            Util.GridSetData(dgHistList2, Result, null, false);
                            dgHistList2.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                        }

                    }

                }
                dgHistList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SKID_ID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnCSearch_Click(sender, e);
            }
        }

        private void SetMobileAssy()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ASSY_RETURN_USELINE"; 
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    if (string.Equals(row["CBO_CODE"], LoginInfo.CFG_AREA_ID))
                    {
                        IsMobileAssy = true;
                        break;
                    }
                }
            }
            catch (Exception ex) { }
        }

        private string GetReturnLine(string sLotID)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LOTID"] = sLotID;

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_AREA", "RQSTDT", "RSLTDT", dt);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["AREAID"]);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return "";
        }
    }
}
