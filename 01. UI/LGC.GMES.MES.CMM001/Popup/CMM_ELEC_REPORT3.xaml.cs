using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_REPORT3 : C1Window, IWorkArea
    {
        string _LOTID = "";
        string _PROCID = "";
        string _CUTID = "";
        string _ENDLOTFLAG = "";
        int _iReportCnt = 0;

        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_REPORT3()
        {
            InitializeComponent();
            ClearTextBlocks(grReport);

            this.Dispatcher.BeginInvoke
            (
                System.Windows.Threading.DispatcherPriority.Input, (System.Threading.ThreadStart)(() =>
                {
                    if (string.Equals(LoginInfo.CFG_CARD_AUTO, "Y") && string.Equals(_ENDLOTFLAG, "Y"))
                        SetHistoryCardPrint();
                }
            ));
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _LOTID = tmps[0].ToString();
            _PROCID = tmps[1].ToString();

            if (tmps.Length > 2)
                _ENDLOTFLAG = tmps[2].ToString();

            SetMixer(_LOTID);
            SetMixer2(_LOTID);
        }

        private void ClearTextBlocks(DependencyObject obj)
        {
            try
            {
                TextBlock tb = obj as TextBlock;

                if (tb != null)
                    if (!tb.Name.Equals(""))
                        tb.Text = "";

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    ClearTextBlocks(VisualTreeHelper.GetChild(obj, i));
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
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                dr["PROCID"] = "E1000";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_FEEDING", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    String sTITLE = ObjectDic.Instance.GetObjectName("FEEDING TANK 이력카드");
                    if (dtRslt.Rows[0]["EQPTSHORTNAME"].ToString().Equals(""))
                    {
                        sTITLE += "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    }
                    else
                    {
                        sTITLE += "(" + dtRslt.Rows[0]["EQPTSHORTNAME"].ToString() + ")";
                    }
                       
                    ((TextBlock)grHeader.FindName("TITLE")).Text = sTITLE;
                    ((TextBlock)grHeader.FindName("LOTID")).Text = dtRslt.Rows[0]["LOTID"].ToString();
                    ((TextBlock)grHeader.FindName("PRODID")).Text = dtRslt.Rows[0]["PRODID"].ToString();
                    ((TextBlock)grHeader.FindName("OUTPUT_QTY")).Text = dtRslt.Rows[0]["OUTPUT_QTY"].ToString() + dtRslt.Rows[0]["UNIT_CODE"].ToString();
                    ((TextBlock)grHeader.FindName("WIPDTTM_ST")).Text = dtRslt.Rows[0]["WIPDTTM_ST"].ToString();
                    ((TextBlock)grHeader.FindName("EQPT_END_DTTM")).Text = dtRslt.Rows[0]["EQPT_END_DTTM"].ToString();
                    ((TextBlock)grHeader.FindName("PRINT_DTTM")).Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    //((TextBlock)grHeader.FindName("REMARK")).Text = dtRslt.Rows[0]["WIP_NOTE"].ToString();
                    ((TextBlock)grHeader.FindName("REMARK")).Text = GetConvertRemark(Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]));
                    ((TextBlock)grHeader.FindName("PRODID")).Text = dtRslt.Rows[0]["PRODID"].ToString();
                    ((TextBlock)grHeader.FindName("MODLID")).Text = dtRslt.Rows[0]["PRJT_NAME"].ToString() + "("+ dtRslt.Rows[0]["PROD_VER_CODE"].ToString() + ")";//dtRslt.Rows[0]["MODLID"].ToString();
                }
                    SetHeader(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMixer2(string sLot)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                dr["PROCID"] = "E1000";
                dr["WIPSEQ"] = "1";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT_MIX", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        if (dtRslt.Rows[i].ItemArray[1].ToString().Equals("SI003-001"))
                            ((TextBlock)grHeader.FindName("SI003_001")).Text = dtRslt.Rows[i].ItemArray[6].ToString() + " " + dtRslt.Rows[i].ItemArray[3].ToString();

                        if (dtRslt.Rows[i].ItemArray[1].ToString().Equals("SI002-001"))
                            ((TextBlock)grHeader.FindName("SI002_001")).Text = dtRslt.Rows[i].ItemArray[6].ToString() + " " + dtRslt.Rows[i].ItemArray[3].ToString();

                        if (dtRslt.Rows[i].ItemArray[1].ToString().Equals("SI005-001"))
                            ((TextBlock)grHeader.FindName("SI005_001")).Text = dtRslt.Rows[i].ItemArray[6].ToString() + " " + dtRslt.Rows[i].ItemArray[3].ToString();

                        if (dtRslt.Rows[i].ItemArray[1].ToString().Equals("SI003-002"))
                            ((TextBlock)grHeader.FindName("SI003_002")).Text = dtRslt.Rows[i].ItemArray[6].ToString() + " " + dtRslt.Rows[i].ItemArray[3].ToString();

                        if (dtRslt.Rows[i].ItemArray[1].ToString().Equals("SI002-002"))
                            ((TextBlock)grHeader.FindName("SI002_002")).Text = dtRslt.Rows[i].ItemArray[6].ToString() + " " + dtRslt.Rows[i].ItemArray[3].ToString();

                        if (dtRslt.Rows[i].ItemArray[1].ToString().Equals("SI005-001"))
                            ((TextBlock)grHeader.FindName("SI005_001")).Text = dtRslt.Rows[i].ItemArray[6].ToString() + " " + dtRslt.Rows[i].ItemArray[3].ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetHistoryCardPrintCount()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inDetailDataRow = inDataTable.NewRow();
            inDetailDataRow["LOTID"] = _LOTID;
            inDetailDataRow["PROCID"] = _PROCID;
            inDataTable.Rows.Add(inDetailDataRow);

            new ClientProxy().ExecuteService("DA_PRD_UPD_PROCESS_LOT_HIST_PRINT_CNT", "INDATA", null, inDataTable, (result, returnEx) =>
            {
                try
                {
                    if (returnEx != null)
                        return;
                }
                catch (Exception ex) { }
            });
        }

        private DataTable GetPrintCount()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = _LOTID;
                Indata["PROCID"] = _PROCID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void SetHeader(DataTable dtRslt)
        {
            LOT_NO_BAR.Text = "*" + dtRslt.Rows[0]["LOTID"].ToString() + "*";
            AREANAME.Text = dtRslt.Rows[0]["AREANAME"].ToString();
            PRINT_DATE.Text = "PRINT DATE : " + dtRslt.Rows[0]["PRINT_DATE"].ToString();
            numCardCopies.Value = LoginInfo.CFG_CARD_COPIES;
        }

        private void SetHistoryCardPrint()
        {
            double scale = 1000 / grReport.ActualHeight;

            this.Width = 560;

            if (scale < 1)
                grReport.LayoutTransform = new ScaleTransform(1, scale);

            grMain.Children.Remove(grReport);

            PrintDialog dialog = new PrintDialog();

            dialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            dialog.PrintTicket.CopyCount = Convert.ToInt32(numCardCopies.Value);
            dialog.PrintTicket.PageOrientation = PageOrientation.Landscape;

            if (LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                dialog.PrintQueue = new PrintQueue(new PrintServer(), LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString());

            FixedDocument document = new FixedDocument();
            document.DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);

            FixedPage page1 = new FixedPage();
            page1.Width = document.DocumentPaginator.PageSize.Width;
            page1.Height = document.DocumentPaginator.PageSize.Height;

            page1.Children.Add(grReport);

            PageContent page1Content = new PageContent();
            ((IAddChild)page1Content).AddChild(page1);
            document.Pages.Add(page1Content);
            try
            {
                dialog.PrintDocument(document.DocumentPaginator, "GMES PRINT");

                // 이력 회수 관리
                SetHistoryCardPrintCount();

                if (string.Equals(LoginInfo.CFG_CARD_AUTO, "Y") && string.Equals(_ENDLOTFLAG, "Y"))
                {
                    this.Close();
                }
                else
                {
                    Util.MessageInfo("SFU1236", (result) =>
                    {
                        this.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1933", (result) =>
                {
                    this.Close();
                });
            }
        }

        private void buttonPrint_Click(object sender, RoutedEventArgs e)
        {
            if (numCardCopies.Value == 0)
                return;

            // 이력카드 PRINT
            DataTable printDT = GetPrintCount();

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT2"]) > 0)
            {
                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                Util.MessageConfirm("SFU3463", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        SetHistoryCardPrint();
                    }
                });
            }
            else
            {
                SetHistoryCardPrint();
            }
        }

        private string GetConvertRemark(string sRemark)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Clear();
            string[] wipNotes = sRemark.Split('|');

            for (int i = 0; i < wipNotes.Length; i++)
            {
                if (!string.IsNullOrEmpty(wipNotes[i]))
                {
                    if (i == 0)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("특이사항") + " : " + wipNotes[i]);
                    else if (i == 1)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("공통특이사항") + " : " + wipNotes[i]);
                    else if (i == 2)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("조정횟수") + " : " + wipNotes[i]);
                    else if (i == 3)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("압연횟수") + " : " + wipNotes[i]);
                    else if (i == 4)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("색지정보") + " : " + wipNotes[i]);
                    else if (i == 5)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("합권이력") + " : " + wipNotes[i]);
                    strBuilder.Append("\n");
                }
            }
            return strBuilder.ToString();
        }
    }
}