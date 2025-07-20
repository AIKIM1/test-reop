/*************************************************************************************
 Created Date : 2023.11.08
      Creator : 이홍주
   Decription : PACKING LIST발행시 일반 프린터(미리보기) 및 ZPL 출력 지원
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.08    이홍주 : Initial Created.
  2024.01.29    이홍주 : 잔량 포장시 LOTID 혼입 출력





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_PALLET_TAG : C1Window, IWorkArea
    {
        #region Declaration
        C1.C1Report.C1Report cr = null;
        object[] tmps;
        DataTable palletInfo = null;
        DataTable lotInfo = null;
        int startPageNum = 0;
        int endPageNum = 0;
        Util _util = new Util();

        // 프린트 설정용
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;

        DataRow _drPrtInfo = null;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
       

        public string PALLET_ID
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public FCS002_PALLET_TAG()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Form Load Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);

            palletInfo = tmps[0] as DataTable;
            lotInfo = tmps[1] as DataTable;

            PALLET_ID = tmps[2] as string;

            this.Loaded -= Window_Loaded;

            // 미리보기
            PrintView();
        }
        #endregion

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if ((bool)rdoGeneral.IsChecked)
                {
                    for (int iPrint = startPageNum; iPrint <= endPageNum; iPrint++)
                    {
                        ///cr.Fields["PageNum"].Text = Convert.ToString(totalPage) + " - " + Convert.ToString(iPrint);
                        c1DocumentViewer.Document = cr.FixedDocumentSequence;
                        var pm = new C1.C1Preview.C1PrintManager();
                        pm.Document = cr;
                        System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                        if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                            ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                        pm.Print(ps);
                    }

                }
                #region ZPL Print
                else
                {
                    // 발행 가능 여부 체크
                    if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    {
                        return;
                    }

                    DataSet indataSet = new DataSet();                    
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("LANGID");
                    inData.Columns.Add("EQPTID");
                    inData.Columns.Add("BOXID");

                    DataRow newRow = inData.NewRow();

                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                    newRow["BOXID"] = PALLET_ID;
                        
                    inData.Rows.Add(newRow);

                    DataTable inPrint = indataSet.Tables.Add("INPRINT");
                    inPrint.Columns.Add("PRMK");
                    inPrint.Columns.Add("RESO");
                    inPrint.Columns.Add("PRCN");
                    inPrint.Columns.Add("MARH");
                    inPrint.Columns.Add("MARV");
                    inPrint.Columns.Add("DARK");

                    newRow = inPrint.NewRow();
                    newRow["PRMK"] = _sPrt;
                    newRow["RESO"] = _sRes;
                    newRow["PRCN"] = _sCopy;
                    newRow["MARH"] = _sXpos;
                    newRow["MARV"] = _sYpos;
                    newRow["DARK"] = _sDark;
                    inPrint.Rows.Add(newRow);

                    /////
                    try
                    {
   
                        DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_PACKINGLIST_LABEL_MB", "INDATA,INPRINT", "OUTDATA", indataSet);
                        //DataTable dtOutData = ds.Tables["OUTDATA"];

                        ///PALLET_ID = dtOutData.Rows[0]["BOXID"].ToString();
                        if (ds.Tables.Contains("OUTDATA") && ds.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            string sRetVal = ds.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString();    //0:OK,  1:NG:
                            string sZplCode = ds.Tables["OUTDATA"].Rows[0]["ZPLCODE"].ToString();

                            PrintLabel(sZplCode, _drPrtInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }                      
                     
                }
                #endregion ZPL

                this.DialogResult = MessageBoxResult.OK;
                this.Close();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].ToString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                //System.Threading.Thread.Sleep(200);
            }
            else
            {
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region SizeChanged
        private void C1Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            c1DocumentViewer.FitToWidth();
            c1DocumentViewer.FitToHeight();

        }
        #endregion

        #region User Method
        private void PrintView()
        {
            try
            {
                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A6;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                
                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.FCS002.Report.Packing_List.xml"))
                {
                    cr.Load(stream, "Packing_List");

                    for (int col = 0; col < palletInfo.Columns.Count; col++)
                    {
                        string strColName = palletInfo.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName))
                        {
                            cr.Fields[strColName].Text = palletInfo.Rows[0][strColName].ToString();

                            if(strColName == "PALLETID")
                            {
                                cr.Fields["PALLETID_BC"].Text = palletInfo.Rows[0][strColName].ToString();
                            }

                        }
                    }

                    for (int row = 0; row < lotInfo.Rows.Count; row++)
                    {
                        for (int col = 0; col < lotInfo.Columns.Count; col++)
                        {
                            string strLotColName = lotInfo.Columns[col].ColumnName + "_" + row.ToString();
                            string strLotColName_Ori = lotInfo.Columns[col].ColumnName;

                            if (cr.Fields.Contains(strLotColName))
                            {
                                cr.Fields[strLotColName].Text = lotInfo.Rows[row][strLotColName_Ori].ToString();
                            }
                        }
                    }
                }
                
                c1DocumentViewer.Document = cr.FixedDocumentSequence;
                c1DocumentViewer.Zoom = 105;              

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
