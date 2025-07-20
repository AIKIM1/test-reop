/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.05.17  김도형    : [E20240408-000359] 电极包装card改善 전극포장card improvement





 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.IO;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.BOX001
{
    public partial class Report_Packing : C1Window, IWorkArea
    {

        C1.C1Report.C1Report cr = null;

        string tmp = string.Empty;
        object[] tmps;
        string tmmp01 = string.Empty;
        DataTable tmmp02;
        DataTable tmmp03;
        DataTable tmmp04;
        DataTable tmmp05;


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Report_Packing()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as DataTable; //dtPackingCard
            tmmp03 = tmps[2] as DataTable; //dtBasicInfo
            tmmp04 = tmps[3] as DataTable; //dtSel01
            tmmp05 = tmps[4] as DataTable; //dtSel02
                        
            this.Loaded -= Window_Loaded;
             
            cr = new C1.C1Report.C1Report();
            cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

            string filename = string.Empty;
            string reportname = string.Empty;

            reportname = tmmp01;
            filename = tmmp01 + ".xml";

            // [E20240408-000359] 电极包装card改善 전극포장card improvement
            if (reportname.Equals("PackingCard_New") || reportname.Equals("PackingCard_New_NJ") || reportname.Equals("PackingCard_2CRT") || reportname.Equals("PackingCard_2CRT_NJ"))
            {
                if (tmmp02 != null && tmmp02.Rows.Count > 0)
                {
                    SetDemandTypeForSkidID();
                }
            }

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + filename))
            {
                cr.Load(stream, reportname);

                for (int col = 0; col < tmmp02.Columns.Count; col++)
                {
                    string strColName = tmmp02.Columns[col].ColumnName;
                    if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp02.Rows[0][strColName].ToString();
                }
            }

            try
            {
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));
                dtRqstDt.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRqstDt.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["CMCDTYPE"] = "PACKING_REPORT_PRINT_QTY";
                drnewrow["ATTRIBUTE1"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["ATTRIBUTE2"] = reportname;

                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTE1", "RQSTDT", "RSLTDT", dtRqstDt);

                if (dtResult != null && dtResult.Rows.Count > 0 && Util.StringToDouble(dtResult.Rows[0]["ATTRIBUTE3"].ToString()) > 0)
                {
                    txtPrintQty.Value = Util.StringToDouble(dtResult.Rows[0]["ATTRIBUTE3"].ToString());
                }
                else
                {
                    if (LoginInfo.CFG_SHOP_ID.Substring(0, 1) == "G")
                    {
                        txtPrintQty.Value = 2;
                    }
                    else
                    {
                        txtPrintQty.Value = 1;
                    }
                }
            }
            catch
            {
                if (LoginInfo.CFG_SHOP_ID.Substring(0, 1) == "G")
                {
                    txtPrintQty.Value = 2;
                }
                else
                {
                    txtPrintQty.Value = 1;
                }
            }
          
            c1DocumentViewer.Document = cr.FixedDocumentSequence;

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
             {
                if (tmmp03.Rows.Count == 2)
                {
              
                    if (tmmp03.Rows[0]["TYPE"].ToString() == "PACK")
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("ISS_QTY", typeof(decimal));
                        inData.Columns.Add("ISS_QTY2", typeof(decimal));
                        inData.Columns.Add("ISS_NOTE", typeof(string));
                        inData.Columns.Add("SHIPTO_ID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
              
                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["FROM_AREAID"] = tmmp03.Rows[0]["FROM_AREAID"].ToString();
                        row["FROM_SLOC_ID"] = tmmp03.Rows[0]["FROM_SLOC_ID"].ToString();
                        row["TO_SHOPID"] = tmmp03.Rows[0]["TO_SHOPID"].ToString();
                        row["TO_AREAID"] = "";
                        row["TO_SLOC_ID"] = tmmp03.Rows[0]["TO_SLOC_ID"].ToString();
                        row["ISS_QTY"] = Convert.ToDecimal(tmmp03.Rows[0]["ISS_QTY"].ToString());
                        row["ISS_QTY2"] = Convert.ToDecimal(tmmp03.Rows[0]["ISS_QTY2"].ToString());
                        row["ISS_NOTE"] = "";
                        row["SHIPTO_ID"] = tmmp03.Rows[0]["SHIPTO_ID"].ToString();
                        row["NOTE"] = "";
                        row["USERID"] = LoginInfo.USERID;
              
                        indataSet.Tables["INDATA"].Rows.Add(row);
              
                        DataTable inBox = indataSet.Tables.Add("INPALLET");
              
                        inBox.Columns.Add("BOXID", typeof(string));
                        inBox.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
              
                        DataRow row2 = inBox.NewRow();
              
                        row2["BOXID"] = tmmp04.Rows[0]["OUTER_BOXID"].ToString();
                        row2["OWMS_BOX_TYPE_CODE"] = tmmp04.Rows[0]["OWMS_BOX_TYPE_CODE"].ToString();
              
                        indataSet.Tables["INPALLET"].Rows.Add(row2);
              
                        DataTable inLot = indataSet.Tables.Add("INBOX");
              
                        inLot.Columns.Add("BOXID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("LOTQTY", typeof(string));
                        inLot.Columns.Add("LOTQTY2", typeof(string));
                        inLot.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
              
                        for (int i = 0; i < tmmp05.Rows.Count; i++)
                        {
                            DataRow row3 = inLot.NewRow();
              
                            row3["BOXID"] = tmmp05.Rows[i]["BOXID"].ToString();
                            row3["LOTID"] = tmmp05.Rows[i]["LOTID"].ToString();
                            row3["LOTQTY"] = tmmp05.Rows[i]["LOTQTY"].ToString();
                            row3["LOTQTY2"] = tmmp05.Rows[i]["LOTQTY2"].ToString();
                            row3["OWMS_BOX_TYPE_CODE"] = tmmp05.Rows[i]["OWMS_BOX_TYPE_CODE"].ToString();
              
                            indataSet.Tables["INBOX"].Rows.Add(row3);
                        }
              
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_PRODUCT_FOR_PACKING", "INDATA,INPALLET,INBOX", null, indataSet);
              
                    }
                    else if (tmmp03.Rows[0]["TYPE"].ToString() == "SHIP")
                    {
              
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("OUTBOXID", typeof(string));
                        inData.Columns.Add("PRODID", typeof(string));
                        inData.Columns.Add("BOXTYPE", typeof(string));
                        inData.Columns.Add("BOXLAYER", typeof(string));
                        inData.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                        inData.Columns.Add("TOTAL_QTY", typeof(decimal));
                        inData.Columns.Add("TOTAL_QTY2", typeof(decimal));
                        inData.Columns.Add("EXP_DOM_TYPE_CODE", typeof(string));
                        inData.Columns.Add("SHIPTO_ID", typeof(string));
                        inData.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
                        inData.Columns.Add("ISS_NOTE", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("SKID_NOTE", typeof(string));

                        for (int i = 0; i < tmmp03.Rows.Count; i++)
                        {
                            DataRow row = inData.NewRow();
                            row["SRCTYPE"] = "UI";
                            row["FROM_AREAID"] = tmmp03.Rows[i]["FROM_AREAID"].ToString();
                            row["FROM_SLOC_ID"] = tmmp03.Rows[i]["FROM_SLOC_ID"].ToString();
                            row["TO_AREAID"] = "";
                            row["TO_SLOC_ID"] = tmmp03.Rows[i]["TO_SLOC_ID"].ToString();
                            row["OUTBOXID"] = tmmp03.Rows[i]["OUTBOXID"].ToString();
                            row["PRODID"] = tmmp03.Rows[i]["PRODID"].ToString();
                            row["BOXTYPE"] = "PLT";
                            row["BOXLAYER"] = "0";
                            row["PACK_LOT_TYPE_CODE"] = "LOT";
                            row["TOTAL_QTY"] = Convert.ToDecimal(tmmp03.Rows[i]["TOTAL_QTY"].ToString());
                            row["TOTAL_QTY2"] = Convert.ToDecimal(tmmp03.Rows[i]["TOTAL_QTY2"].ToString());
                            row["EXP_DOM_TYPE_CODE"] = "E";
                            row["SHIPTO_ID"] = tmmp03.Rows[i]["SHIPTO_ID"].ToString();
                            row["OWMS_BOX_TYPE_CODE"] = tmmp03.Rows[i]["OWMS_BOX_TYPE_CODE"].ToString();
                            row["ISS_NOTE"] = tmmp03.Rows[i]["REMARK"].ToString();
                            row["NOTE"] = tmmp03.Rows[i]["REMARK"].ToString();
                            row["USERID"] = LoginInfo.USERID;
                            row["PROCID"] = tmmp03.Rows[i]["PROCID"].ToString(); //"E7000";
                            row["TO_SHOPID"] = tmmp03.Rows[i]["TO_SHOPID"].ToString();

                            if(tmmp03.Columns.Contains("SKID_NOTE"))
                                row["SKID_NOTE"] = tmmp03.Rows[i]["SKID_NOTE"].ToString();                            

                            indataSet.Tables["INDATA"].Rows.Add(row);
                        }
              
                        DataTable inBox = indataSet.Tables.Add("BOXID");
              
                        inBox.Columns.Add("INBOXID", typeof(string));
                        inBox.Columns.Add("OUTBOXID", typeof(string));
                        inBox.Columns.Add("PRODID", typeof(string));
                        inBox.Columns.Add("BOXTYPE", typeof(string));
                        inBox.Columns.Add("BOXLAYER", typeof(string));
                        inBox.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                        inBox.Columns.Add("TOTAL_QTY", typeof(decimal));
                        inBox.Columns.Add("TOTAL_QTY2", typeof(decimal));
                        inBox.Columns.Add("EXP_DOM_TYPE_CODE", typeof(string));
                        inBox.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
              
                        if (tmmp03.Rows.Count > 0)
                        {
                            for (int i = 0; i < tmmp03.Rows.Count; i++)
                            {
                                DataRow row2 = inBox.NewRow();
              
                                row2["INBOXID"] = tmmp03.Rows[i]["INBOXID"].ToString();  //PACK_NO1
                                row2["OUTBOXID"] = tmmp03.Rows[i]["OUTBOXID"].ToString();// 신규 추가 OUTBOXID 20170606
                                row2["PRODID"] = tmmp03.Rows[i]["PRODID"].ToString();
                                row2["BOXTYPE"] = tmmp03.Rows[i]["PKGWAY"].ToString();
                                row2["BOXLAYER"] = "0";
                                row2["PACK_LOT_TYPE_CODE"] = "LOT";
                                row2["TOTAL_QTY"] = tmmp03.Rows[i]["TOTAL_QTY"].ToString();
                                row2["TOTAL_QTY2"] = tmmp03.Rows[i]["TOTAL_QTY2"].ToString();
                                row2["EXP_DOM_TYPE_CODE"] = "E";
                                row2["OWMS_BOX_TYPE_CODE"] = tmmp03.Rows[i]["OWMS_BOX_TYPE_CODE"].ToString();
              
                                indataSet.Tables["BOXID"].Rows.Add(row2);
                            }
                        }
                        DataTable inLot = indataSet.Tables.Add("INLOT");
              
                        inLot.Columns.Add("INBOXID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("LOTQTY", typeof(string));
                        inLot.Columns.Add("LOTQTY2", typeof(string));
              
                        if (tmmp04 != null)
                            for (int i = 0; i < tmmp04.Rows.Count; i++)
                            {
                                DataRow row3 = inLot.NewRow();
              
                                row3["INBOXID"] = tmmp04.Rows[i]["INBOXID"].ToString();
                                row3["LOTID"] = tmmp04.Rows[i]["LOTID"].ToString();
                                row3["LOTQTY"] = tmmp04.Rows[i]["M_WIPQTY"].ToString();
                                row3["LOTQTY2"] = tmmp04.Rows[i]["CELL_WIPQTY"].ToString();
              
                                indataSet.Tables["INLOT"].Rows.Add(row3);
                            }
              
                        if (tmmp05 != null)
                            for (int i = 0; i < tmmp05.Rows.Count; i++)
                            {
                                DataRow row3 = inLot.NewRow();
              
                                row3["INBOXID"] = tmmp05.Rows[i]["INBOXID"].ToString();
                                row3["LOTID"] = tmmp05.Rows[i]["LOTID"].ToString();
                                row3["LOTQTY"] = tmmp05.Rows[i]["M_WIPQTY"].ToString();
                                row3["LOTQTY2"] = tmmp05.Rows[i]["CELL_WIPQTY"].ToString();
              
                                indataSet.Tables["INLOT"].Rows.Add(row3);
                            }
              
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_LOT_FOR_PACKING_MASTER", "INDATA,BOXID,INLOT", null, indataSet);
              
                    }
                }
                else
                {
                    if (tmmp03.Rows[0]["TYPE"].ToString() == "PACK")
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("ISS_QTY", typeof(decimal));
                        inData.Columns.Add("ISS_QTY2", typeof(decimal));
                        inData.Columns.Add("ISS_NOTE", typeof(string));
                        inData.Columns.Add("SHIPTO_ID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
              
                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["FROM_AREAID"] = tmmp03.Rows[0]["FROM_AREAID"].ToString();
                        row["FROM_SLOC_ID"] = tmmp03.Rows[0]["FROM_SLOC_ID"].ToString();
                        row["TO_SHOPID"] = tmmp03.Rows[0]["TO_SHOPID"].ToString();
                        row["TO_AREAID"] = "";
                        row["TO_SLOC_ID"] = tmmp03.Rows[0]["TO_SLOC_ID"].ToString();
                        row["ISS_QTY"] = Convert.ToDecimal(tmmp03.Rows[0]["ISS_QTY"].ToString());
                        row["ISS_QTY2"] = Convert.ToDecimal(tmmp03.Rows[0]["ISS_QTY2"].ToString());
                        row["ISS_NOTE"] = "";
                        row["SHIPTO_ID"] = tmmp03.Rows[0]["SHIPTO_ID"].ToString();
                        row["NOTE"] = "";
                        row["USERID"] = LoginInfo.USERID;
              
                        indataSet.Tables["INDATA"].Rows.Add(row);
              
              
                        DataTable inBox = indataSet.Tables.Add("INPALLET");
              
                        inBox.Columns.Add("BOXID", typeof(string));
                        inBox.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
              
                        DataRow row2 = inBox.NewRow();
              
                        row2["BOXID"] = tmmp04.Rows[0]["OUTER_BOXID"].ToString();
                        row2["OWMS_BOX_TYPE_CODE"] = tmmp04.Rows[0]["OWMS_BOX_TYPE_CODE"].ToString();
              
                        indataSet.Tables["INPALLET"].Rows.Add(row2);
              
              
                        DataTable inLot = indataSet.Tables.Add("INBOX");
              
                        inLot.Columns.Add("BOXID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("LOTQTY", typeof(string));
                        inLot.Columns.Add("LOTQTY2", typeof(string));
                        inLot.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
              
                        for (int i = 0; i < tmmp05.Rows.Count; i++)
                        {
                            DataRow row3 = inLot.NewRow();
              
                            row3["BOXID"] = tmmp05.Rows[i]["BOXID"].ToString();
                            row3["LOTID"] = tmmp05.Rows[i]["LOTID"].ToString();
                            row3["LOTQTY"] = tmmp05.Rows[i]["LOTQTY"].ToString();
                            row3["LOTQTY2"] = tmmp05.Rows[i]["LOTQTY2"].ToString();
                            row3["OWMS_BOX_TYPE_CODE"] = tmmp05.Rows[i]["OWMS_BOX_TYPE_CODE"].ToString();
              
                            indataSet.Tables["INBOX"].Rows.Add(row3);
                        }
              
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_PRODUCT_FOR_PACKING", "INDATA,INPALLET,INBOX", null, indataSet);
              
                    }
                    else if (tmmp03.Rows[0]["TYPE"].ToString() == "SHIP")
                    {
              
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("OUTBOXID", typeof(string));
                        inData.Columns.Add("PRODID", typeof(string));
                        inData.Columns.Add("BOXTYPE", typeof(string));
                        inData.Columns.Add("BOXLAYER", typeof(string));
                        inData.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                        inData.Columns.Add("TOTAL_QTY", typeof(decimal));
                        inData.Columns.Add("TOTAL_QTY2", typeof(decimal));
                        inData.Columns.Add("EXP_DOM_TYPE_CODE", typeof(string));
                        inData.Columns.Add("SHIPTO_ID", typeof(string));
                        inData.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
                        inData.Columns.Add("ISS_NOTE", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("SKID_NOTE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["FROM_AREAID"] = tmmp03.Rows[0]["FROM_AREAID"].ToString();
                        row["FROM_SLOC_ID"] = tmmp03.Rows[0]["FROM_SLOC_ID"].ToString();
                        row["TO_AREAID"] = "";
                        row["TO_SLOC_ID"] = tmmp03.Rows[0]["TO_SLOC_ID"].ToString();
                        row["OUTBOXID"] = tmmp03.Rows[0]["PACKING_NO"].ToString();
                        row["PRODID"] = tmmp03.Rows[0]["PRODID"].ToString();
                        row["BOXTYPE"] = "PLT";
                        row["BOXLAYER"] = "0";
                        row["PACK_LOT_TYPE_CODE"] = "LOT";
                        row["TOTAL_QTY"] = Convert.ToDecimal(tmmp03.Rows[0]["QTY1"].ToString());
                        row["TOTAL_QTY2"] = Convert.ToDecimal(tmmp03.Rows[0]["QTY2"].ToString());
                        row["EXP_DOM_TYPE_CODE"] = "E";
                        row["SHIPTO_ID"] = tmmp03.Rows[0]["SHIPTO_ID"].ToString();
                        row["OWMS_BOX_TYPE_CODE"] = tmmp03.Rows[0]["OWMS_BOX_TYPE_CODE"].ToString();
                        row["ISS_NOTE"] = tmmp03.Rows[0]["REMARK"].ToString();
                        row["NOTE"] = tmmp03.Rows[0]["REMARK"].ToString();
                        row["USERID"] = LoginInfo.USERID;
                        row["PROCID"] = tmmp03.Rows[0]["PROCID"].ToString();
                        row["TO_SHOPID"] = tmmp03.Rows[0]["TO_SHOPID"].ToString();

                        if (tmmp03.Columns.Contains("SKID_NOTE"))
                            row["SKID_NOTE"] = tmmp03.Rows[0]["SKID_NOTE"].ToString();

                        indataSet.Tables["INDATA"].Rows.Add(row);
              
                        DataTable inBox = indataSet.Tables.Add("BOXID");
              
                        inBox.Columns.Add("INBOXID", typeof(string));
                        inBox.Columns.Add("PRODID", typeof(string));
                        inBox.Columns.Add("BOXTYPE", typeof(string));
                        inBox.Columns.Add("BOXLAYER", typeof(string));
                        inBox.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                        inBox.Columns.Add("TOTAL_QTY", typeof(decimal));
                        inBox.Columns.Add("TOTAL_QTY2", typeof(decimal));
                        inBox.Columns.Add("EXP_DOM_TYPE_CODE", typeof(string));
                        inBox.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
              
                        if (tmmp03.Rows.Count > 0)
                        {
                            DataRow row2 = inBox.NewRow();
              
                            row2["INBOXID"] = tmmp03.Rows[0]["PACK_NO1"].ToString();
                            row2["PRODID"] = tmmp03.Rows[0]["PRODID"].ToString();
                            row2["BOXTYPE"] = tmmp03.Rows[0]["PKGWAY"].ToString();
                            row2["BOXLAYER"] = "0";
                            row2["PACK_LOT_TYPE_CODE"] = "LOT";
                            row2["TOTAL_QTY"] = tmmp03.Rows[0]["TOTAL_QTY"].ToString(); 
                            row2["TOTAL_QTY2"] = tmmp03.Rows[0]["TOTAL_QTY2"].ToString();
                            row2["EXP_DOM_TYPE_CODE"] = "E";
                            row2["OWMS_BOX_TYPE_CODE"] = tmmp03.Rows[0]["OWMS_BOX_TYPE_CODE"].ToString();
              
                            indataSet.Tables["BOXID"].Rows.Add(row2);
                        }
              
                        if (tmmp03 != null && !tmmp03.Rows[0]["PACK_NO2"].ToString().Equals("") && !tmmp03.Rows[0]["TOTAL_QTY4"].ToString().Equals("0"))
                        {
                            DataRow row2 = inBox.NewRow();
              
                            row2["INBOXID"] = tmmp03.Rows[0]["PACK_NO2"].ToString();
                            row2["PRODID"] = tmmp03.Rows[0]["PRODID"].ToString();
                            row2["BOXTYPE"] = tmmp03.Rows[0]["PKGWAY"].ToString();
                            row2["BOXLAYER"] = "0";
                            row2["PACK_LOT_TYPE_CODE"] = "LOT";
                            row2["TOTAL_QTY"] = tmmp03.Rows[0]["TOTAL_QTY3"].ToString(); 
                            row2["TOTAL_QTY2"] = tmmp03.Rows[0]["TOTAL_QTY4"].ToString();
                            row2["EXP_DOM_TYPE_CODE"] = "E";
                            row2["OWMS_BOX_TYPE_CODE"] = tmmp03.Rows[0]["OWMS_BOX_TYPE_CODE"].ToString();
              
                            indataSet.Tables["BOXID"].Rows.Add(row2);
                        }
              
                        DataTable inLot = indataSet.Tables.Add("INLOT");
              
                        inLot.Columns.Add("INBOXID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("LOTQTY", typeof(string));
                        inLot.Columns.Add("LOTQTY2", typeof(string));
              
                        for (int i = 0; i < tmmp04.Rows.Count; i++)
                        {
                            DataRow row3 = inLot.NewRow();
              
                            row3["INBOXID"] = tmmp03.Rows[0]["PACK_NO1"].ToString();
                            row3["LOTID"] = tmmp04.Rows[i]["LOTID"].ToString();
                            row3["LOTQTY"] = tmmp04.Rows[i]["M_WIPQTY"].ToString();
                            row3["LOTQTY2"] = tmmp04.Rows[i]["CELL_WIPQTY"].ToString();
              
                            indataSet.Tables["INLOT"].Rows.Add(row3);
                        }
              
                        if (tmmp05 != null)
                        {
                            for (int i = 0; i < tmmp05.Rows.Count; i++)
                            {
                                DataRow row3 = inLot.NewRow();
              
                                row3["INBOXID"] = tmmp03.Rows[0]["PACK_NO2"].ToString();
                                row3["LOTID"] = tmmp05.Rows[i]["LOTID"].ToString();
                                row3["LOTQTY"] = tmmp05.Rows[i]["M_WIPQTY"].ToString();
                                row3["LOTQTY2"] = tmmp05.Rows[i]["CELL_WIPQTY"].ToString();
              
                                indataSet.Tables["INLOT"].Rows.Add(row3);
                            }
                        }
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_LOT_FOR_PACKING", "INDATA,BOXID,INLOT", null, indataSet);
                    }
              
                }
             
              int iCopies = int.Parse(txtPrintQty.Value.ToString());

               // [E20230227-000318] 전극 포장 이력카드 개선건
               string sEltrPackLabelUseYn;
               string sblCode;
               if (!GetEltrPackLabelUseYn(out  sEltrPackLabelUseYn, out  sblCode))
               { 
                   sEltrPackLabelUseYn = "N";
               }
              
               if(Util.NVC(sEltrPackLabelUseYn).Equals("Y"))
               {
                   GetPackPrintLabelZPL(sblCode, iCopies);  // [E20230227-000318] 전극 포장 이력카드 개선건
               }
               else
               {    
                         // Print수량만큼 출력함...
                         for (int iPrint = 0; iPrint < iCopies; iPrint++)
                         {
                             var pm = new C1.C1Preview.C1PrintManager();
                             pm.Document = cr;
                             System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                   
                             if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                                 ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();
                   
                             pm.Print(ps);
                         }
                   
               }
               this.DialogResult = MessageBoxResult.OK;
               this.Close();              
          }
          catch (Exception ex)
          {
                  Util.MessageException(ex);
          }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #region Lable BarCode Print 
        private bool GetEltrPackLabelUseYn(out string sEltrPackLabelUseYn, out string sblCode)
        {
            bool bRet = false;
            try {
                
                DataTable dt = new DataTable();
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CMCDTYPE"] = "ELTR_PACK_LABEL_USE_YN";
                dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sEltrPackLabelUseYn = dtResult.Rows[0]["ATTRIBUTE1"].ToString();
                    sblCode = dtResult.Rows[0]["ATTRIBUTE2"].ToString();
                    bRet = true;

                }
                else
                {

                    sEltrPackLabelUseYn = "N";
                    sblCode = "";
                    bRet = false;
                } 
            }
            catch (Exception ex)
            {
                sEltrPackLabelUseYn = "N";
                sblCode = "";
                bRet = false;
            }
            return bRet;
        }

        // [E20230227-000318] 전극 포장 이력카드 개선건
        private string GetLotPrjtName(string sProdid)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PRODID"] = sProdid;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTATTR_FOR_PROJECTNAME", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["PROJECTNAME"]); //프로젝트 명[PRJT_NAME]
            }
            catch (Exception ex) {
                return "";
            }

            return "";
        }
        // [E20230227-000318] 전극 포장 이력카드 개선건
        private string GetLotPrdtAbbrCode(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("CSTID", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["CSTID"] = sLotID;
                //Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LANGID"] = "en-US";
                Indata["CMCDTYPE"] = "PRDT_ABBR_CODE";
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNITCODE_BY_CUTID", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["PRDT_ABBR_CODE"]); //극성
            }
            catch (Exception ex)
            {
                return "";
            }

            return "";
        }
        //[E20230227-000318]전극 포장 이력카드 개선건 - LBL0319
        //[E20230504-000891]전극 포장 이력카드 개선건 - LBL0319->LBL0321
        private bool GetPackPrintLabelZPL(string sblCode, int iCopies)
        {
            LoadingIndicator loadingIndicator = new LoadingIndicator(); 
            
            try
            {
                DataRow drPrintInfo;
                string sPrintType;
                string sResolution;
                string sIssueCount;
                string sXposition;
                string sYposition;
                string sDarkness;
                string sPortName; 
                // 체크된 라벨 정보 확인
                if (!Util.GetConfigPrintInfoPack(out sPrintType, out sResolution, out sIssueCount, out sXposition, out sYposition, out sDarkness, out sPortName, out drPrintInfo))
                {
                    Util.MessageValidation("SFU3030");  //프린터 환경설정 정보가 없습니다.
                    return  false;
                }

                if (string.IsNullOrWhiteSpace(Util.NVC(sblCode)) || string.IsNullOrEmpty(Util.NVC(sblCode)))
                {
                    Util.MessageValidation("SFU4079");  //라벨정보가 없습니다..
                    return false;
                }
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("RESO", typeof(string));   // 해상도
                inTable.Columns.Add("PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("DARK", typeof(string));   // strDarkness 
                inTable.Columns.Add("ATTVAL001", typeof(string));   // ATTVAL001
                inTable.Columns.Add("ATTVAL002", typeof(string));   // ATTVAL002
                inTable.Columns.Add("ATTVAL003", typeof(string));   // ATTVAL003
                inTable.Columns.Add("ATTVAL004", typeof(string));   // ATTVAL004
                inTable.Columns.Add("ATTVAL005", typeof(string));   // ATTVAL005
                inTable.Columns.Add("ATTVAL006", typeof(string));   // ATTVAL006
                inTable.Columns.Add("ATTVAL007", typeof(string));   // ATTVAL007
                inTable.Columns.Add("ATTVAL008", typeof(string));   // ATTVAL008
                inTable.Columns.Add("ATTVAL009", typeof(string));   // ATTVAL009
                inTable.Columns.Add("ATTVAL010", typeof(string));   // ATTVAL010
                inTable.Columns.Add("ATTVAL011", typeof(string));   // ATTVAL011
                inTable.Columns.Add("ATTVAL012", typeof(string));   // ATTVAL012
                inTable.Columns.Add("ATTVAL013", typeof(string));   // ATTVAL013
                inTable.Columns.Add("ATTVAL014", typeof(string));   // ATTVAL014
                inTable.Columns.Add("ATTVAL015", typeof(string));   // ATTVAL015
                inTable.Columns.Add("ATTVAL016", typeof(string));   // ATTVAL016
                inTable.Columns.Add("ATTVAL017", typeof(string));   // ATTVAL017
                inTable.Columns.Add("ATTVAL018", typeof(string));   // ATTVAL018 

                DataRow Indata = inTable.NewRow(); 
                Indata["LBCD"] = sblCode; // 라벨코드  : DB에서호출방식으로 변경 필요
                Indata["PRMK"] = sPrintType; // 프린터기종
                Indata["RESO"] = sResolution; // 해상도
                Indata["PRCN"] = sIssueCount; // 출력매수
                Indata["MARH"] = sXposition; // 시작위치H
                Indata["MARV"] = sYposition; // 시작위치V
                Indata["DARK"] = sDarkness; //  Darkness

                Indata["ATTVAL001"] = GetLotPrjtName(tmmp03.Rows[0]["PRODID"].ToString()); // ATTVAL001 :프로젝트 명[PRJT_NAME]
                Indata["ATTVAL002"] = tmmp03.Rows[0]["PRODID"].ToString(); // ATTVAL002 : 제품ID
                Indata["ATTVAL003"] = tmmp02.Rows[0]["HEAD_BARCODE"].ToString().Replace("*", ""); // ATTVAL003 바코드
                Indata["ATTVAL004"] = tmmp02.Rows[0]["PACK_NO"].ToString(); // ATTVAL004 : BOX_ID
                if(tmmp02.Columns.Contains("Total_M"))   //Total_M 컬럼이 없는 경우 : 전극포장및출고(ROLL): BOX001_036 
                {
                    Indata["ATTVAL005"] = ObjectDic.Instance.GetObjectName("C/ROLL_BARCODE_ABBR") + " : " + tmmp02.Rows[0]["Total_M"].ToString(); // ATTVAL005  C/Roll 
                }
                else
                {
                    Indata["ATTVAL005"] = ObjectDic.Instance.GetObjectName("C/ROLL_BARCODE_ABBR") + " :"; // ATTVAL005  C/Roll 
                } 
                Indata["ATTVAL006"] = ObjectDic.Instance.GetObjectName("PROD_DATE_BARCODE_ABBR") + " : " + tmmp02.Rows[0]["REG_DATE1"].ToString().Replace("-","/"); // ATTVAL006 생산일자  
                Indata["ATTVAL007"] = ObjectDic.Instance.GetObjectName("VER_BARCODE_ABBR") + " : " + tmmp02.Rows[0]["V1"].ToString(); // ATTVAL007   버전 
                Indata["ATTVAL008"] = ObjectDic.Instance.GetObjectName("VALID_DATE_BARCODE_ABBR") + " : " + tmmp02.Rows[0]["VLD_DATE1"].ToString().Replace("-", "/");  
                Indata["ATTVAL009"] = GetLotPrdtAbbrCode(tmmp02.Rows[0]["Lot1"].ToString()); // ATTVAL009 극성
                Indata["ATTVAL010"] = "" ; // ATTVAL010
                Indata["ATTVAL011"] = "" ; // ATTVAL011
                Indata["ATTVAL012"] = "" ; // ATTVAL012
                Indata["ATTVAL013"] = "" ; // ATTVAL013
                Indata["ATTVAL014"] = "" ; // ATTVAL014
                Indata["ATTVAL015"] = "" ; // ATTVAL015
                Indata["ATTVAL016"] = "" ; // ATTVAL016
                Indata["ATTVAL017"] = "" ; // ATTVAL017
                Indata["ATTVAL018"] = "";  // ATTVAL018 
                inTable.Rows.Add(Indata);
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM15", "INDATA", "RSLTDT", inTable);
                //인쇄 요청

                if (result != null && result.Rows.Count > 0)
                {
                    string zplCode = Util.NVC(result.Rows[0]["LABELCD"]); //바코드라벨 ZPL

                    if (zplCode.StartsWith("0,"))
                    {
                        zplCode = zplCode.Substring(2);

                        for (int iPrint = 0; iPrint < iCopies; iPrint++)
                        {
                            System.Threading.Thread.Sleep(500);
                            Util.PrintLabel(FrameOperation, loadingIndicator, zplCode);
                        }
                        return true;
                    }
                    else
                    {
                        Util.MessageInfo(zplCode.Substring(2));
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageValidation("SFU1309");  //바코드 프린터 실패
                return false;
            }

            return true;
        }
        #endregion

        // [E20240408-000359] 电极包装card改善 전극포장card improvement
        private void SetDemandTypeForSkidID()
        {
            try
            {
                tmmp02.Columns.Add("DEMAND_TYPE1", typeof(string));
                tmmp02.Columns.Add("DEMAND_TYPE2", typeof(string));

                for (int i = 0; i < tmmp02.Rows.Count; i++)
                {
                    if (!tmmp02.Rows[i]["Lot1"].ToString().Equals(""))
                    {
                        tmmp02.Rows[i]["DEMAND_TYPE1"] = GetDemandTypeForSkidID(tmmp02.Rows[i]["Lot1"].ToString());
                    }
                    if (!tmmp02.Rows[i]["Lot2"].ToString().Equals(""))
                    {
                        tmmp02.Rows[i]["DEMAND_TYPE2"] = GetDemandTypeForSkidID(tmmp02.Rows[i]["Lot2"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return;
        }
        // [E20240408-000359] 电极包装card改善 전극포장card improvement
        private string GetDemandTypeForSkidID (string sSkidID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SKIDID", typeof(string)); 

                DataRow Indata = IndataTable.NewRow();
                Indata["SKIDID"] = sSkidID; 
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DEMAND_TYPE_FOR_SKIDID", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    return result.Rows[0]["DEMAND_TYPE"].ToString();  //DEMAND_TYPE
                } 
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return "";
            }

            return "";
        }
    }
}
