using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;


namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_REPORT : Window
    {
        string sMixLot = "ECPJKM2011";
        string sCoaterLot = "EAPJ7018C1";
        string sRollLot = "ACPJ5011RB";
        string sSliterLot = "BAPJ701S11";

        public CMM_ELEC_REPORT()
        {
            InitializeComponent();
            ClearTextBlocks(grReport);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();

            dialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);

            if (dialog.ShowDialog() != true) return;

            //grReport.Measure(new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight));

            dialog.PrintVisual(grReport, "GMES PRINT");

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sMixLot;
            
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_MIXER", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0) {
                    MIX0.Text = "("+dtRslt.Rows[0]["EQPTNAME"].ToString()+")";
                    MIX1.Text = dtRslt.Rows[0]["LOTID"].ToString();
                    MIX2.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                    MIX3.Text = dtRslt.Rows[0]["TERMDATE"].ToString();
                    MIX4.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                    MIX5.Text = dtRslt.Rows[0]["MIX5"].ToString();
                    MIX6.Text = dtRslt.Rows[0]["MIX6"].ToString();
                    MIX7.Text = dtRslt.Rows[0]["WIPQTY_ED"].ToString();
                    MIX8.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Rows[0]["WIPQTY_ED"].ToString());
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sCoaterLot;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_COATER", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    COATER0.Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    COATER1.Text = "";
                    COATER2.Text = dtRslt.Rows[0]["LOTID"].ToString();
                    COATER3.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                    COATER4.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                    COATER5.Text = "";
                    COATER6.Text = "";
                    COATER7.Text = "";
                    COATER8.Text = "";
                    COATER9.Text = "";
                    COATER10.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER11.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER12.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER13.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER14.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER15.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER16.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER17.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER18.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER19.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER20.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER21.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Rows[0]["WIPQTY_ED"].ToString());
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sRollLot;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_ROLL", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    RP0.Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    RP1.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                    RP2.Text = dtRslt.Rows[0]["USERNAME"].ToString(); 
                    RP3.Text = dtRslt.Rows[0]["WIPQTY_ED"].ToString();
                    RP4.Text = "";
                    RP5.Text = dtRslt.Rows[0]["RP5"].ToString();
                    RP6.Text = dtRslt.Rows[0]["RP6"].ToString();
                    RP7.Text = dtRslt.Rows[0]["RP7"].ToString();
                    RP8.Text = dtRslt.Rows[0]["RP8"].ToString();
                    RP9.Text = dtRslt.Rows[0]["RP9"].ToString();
                    RP10.Text = dtRslt.Rows[0]["RP10"].ToString();
                    RP11.Text = "";
                    RP12.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Rows[0]["WIPQTY_ED"].ToString());
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sSliterLot;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_SLITER", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    SL0.Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    SL1.Text = "S/L " + dtRslt.Rows.Count.ToString() + " CUT";
                    SL2.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                    SL3.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                    SL4.Text = "";
                    SL5.Text = "";
                    SL6.Text = dtRslt.Compute("SUM(WIPQTY_ED)", "").ToString();
                    
                    SL7.Text = dtRslt.Rows[0]["WIPQTY_ED"].ToString();
                    SL8.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    if (dtRslt.Rows.Count > 1)
                    {
                        SL10.Text = dtRslt.Rows[1]["WIPQTY_ED"].ToString();
                        SL11.Text = dtRslt.Rows[1]["WIPNOTE"].ToString();
                    }
                    if (dtRslt.Rows.Count > 2)
                    {
                        SL13.Text = dtRslt.Rows[2]["WIPQTY_ED"].ToString();
                        SL14.Text = dtRslt.Rows[2]["WIPNOTE"].ToString();
                    }
                    if (dtRslt.Rows.Count > 3)
                    {
                        SL16.Text = dtRslt.Rows[3]["WIPQTY_ED"].ToString();
                        SL17.Text = dtRslt.Rows[3]["WIPNOTE"].ToString();
                    }
                    //SL18.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Compute("SUM(WIPQTY_ED)", "").ToString());
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //DataTable dtRqst = new DataTable();
                //dtRqst.Columns.Add("LOTID", typeof(string));
                //dtRqst.Columns.Add("LANGID", typeof(string));

                //DataRow dr = dtRqst.NewRow();

                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["LOTID"] = sRollLot;

                //dtRqst.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_ROLL", "INDATA", "OUTDATA", dtRqst);

                //if (dtRslt.Rows.Count > 0)
                //{
                //    RP0.Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                //    RP1.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                //    RP2.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                //    RP3.Text = dtRslt.Rows[0]["WIPQTY_ED"].ToString();
                //    RP4.Text = "";
                //    RP5.Text = dtRslt.Rows[0]["RP5"].ToString();
                //    RP6.Text = dtRslt.Rows[0]["RP6"].ToString();
                //    RP7.Text = dtRslt.Rows[0]["RP7"].ToString();
                //    RP8.Text = dtRslt.Rows[0]["RP8"].ToString();
                //    RP9.Text = dtRslt.Rows[0]["RP9"].ToString();
                //    RP10.Text = dtRslt.Rows[0]["RP10"].ToString();
                //    RP11.Text = "";
                //    RP12.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                //    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Rows[0]["WIPQTY_ED"].ToString());
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetHeader(string sLotId, string sQty) {
            try
            {
                HEAD_LOTID.Text = sLotId;
                HEAD_LOTID_BAR.Text = "*"+ sLotId+"*";

                HEAD_QTY.Text = sQty;
                HEAD_QTY_BAR.Text = "*" + sQty + "*";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearTextBlocks(DependencyObject obj) {
            try
            {
                TextBlock tb = obj as TextBlock;
                if (tb != null)
                    if(!tb.Name.Equals(""))
                        tb.Text = "";

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    ClearTextBlocks(VisualTreeHelper.GetChild(obj, i));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void buttonGet_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBlocks(grReport);

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = textBox.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_HIERARCHY", "INDATA", "OUTDATA", dtRqst);

                for (int i = 0; i < dtRslt.Rows.Count; i++) {

                    switch (dtRslt.Rows[i]["PROCID"].ToString())
                    {
                        
                        case "E1000":
                            SetMixer(dtRslt.Rows[i]["LOTID"].ToString());
                            break;
                        case "E2000":
                            SetCoater(dtRslt.Rows[i]["LOTID"].ToString());
                            break;
                        case "E3000":
                            SetRoll(dtRslt.Rows[i]["LOTID"].ToString());
                            break;
                        case "E4000":
                            SetSliter(dtRslt.Rows[i]["LOTID"].ToString());
                            break;
                    }


                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetSliter(string sLot)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_SLITER", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    SL0.Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    SL1.Text = "S/L " + dtRslt.Rows.Count.ToString() + " CUT";
                    SL2.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                    SL3.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                    SL4.Text = "";
                    SL5.Text = "";
                    SL6.Text = dtRslt.Compute("SUM(WIPQTY_ED)", "").ToString();

                    SL7.Text = dtRslt.Rows[0]["WIPQTY_ED"].ToString();
                    SL8.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    if (dtRslt.Rows.Count > 1)
                    {
                        SL10.Text = dtRslt.Rows[1]["WIPQTY_ED"].ToString();
                        SL11.Text = dtRslt.Rows[1]["WIPNOTE"].ToString();
                    }
                    if (dtRslt.Rows.Count > 2)
                    {
                        SL13.Text = dtRslt.Rows[2]["WIPQTY_ED"].ToString();
                        SL14.Text = dtRslt.Rows[2]["WIPNOTE"].ToString();
                    }
                    if (dtRslt.Rows.Count > 3)
                    {
                        SL16.Text = dtRslt.Rows[3]["WIPQTY_ED"].ToString();
                        SL17.Text = dtRslt.Rows[3]["WIPNOTE"].ToString();
                    }
                    //SL18.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Compute("SUM(WIPQTY_ED)", "").ToString());
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetRoll(string sLot)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_ROLL", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    RP0.Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    RP1.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                    RP2.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                    RP3.Text = dtRslt.Rows[0]["WIPQTY_ED"].ToString();
                    RP4.Text = "";
                    RP5.Text = dtRslt.Rows[0]["RP5"].ToString();
                    RP6.Text = dtRslt.Rows[0]["RP6"].ToString();
                    RP7.Text = dtRslt.Rows[0]["RP7"].ToString();
                    RP8.Text = dtRslt.Rows[0]["RP8"].ToString();
                    RP9.Text = dtRslt.Rows[0]["RP9"].ToString();
                    RP10.Text = dtRslt.Rows[0]["RP10"].ToString();
                    RP11.Text = "";
                    RP12.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Rows[0]["WIPQTY_ED"].ToString());
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCoater(string sLot)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_COATER", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    COATER0.Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    COATER1.Text = "";
                    COATER2.Text = dtRslt.Rows[0]["LOTID"].ToString();
                    COATER3.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                    COATER4.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                    COATER5.Text = "";
                    COATER6.Text = "";
                    COATER7.Text = "";
                    COATER8.Text = "";
                    COATER9.Text = "";
                    COATER10.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER11.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER12.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER13.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER14.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER15.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER16.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER17.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER18.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER19.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER20.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();
                    COATER21.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Rows[0]["WIPQTY_ED"].ToString());
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMixer(string sLot)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_MIXER", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    MIX0.Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    MIX1.Text = dtRslt.Rows[0]["LOTID"].ToString();
                    MIX2.Text = dtRslt.Rows[0]["WORKDATE"].ToString();
                    MIX3.Text = dtRslt.Rows[0]["TERMDATE"].ToString();
                    MIX4.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                    MIX5.Text = dtRslt.Rows[0]["MIX5"].ToString();
                    MIX6.Text = dtRslt.Rows[0]["MIX6"].ToString();
                    MIX7.Text = dtRslt.Rows[0]["WIPQTY_ED"].ToString();
                    MIX8.Text = dtRslt.Rows[0]["WIPNOTE"].ToString();

                    SetHeader(dtRslt.Rows[0]["LOTID"].ToString(), dtRslt.Rows[0]["WIPQTY_ED"].ToString());
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
