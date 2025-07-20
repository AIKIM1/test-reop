/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.02.01  이윤중 : RFID TAG ID 표기하도록 변경 (Pallet_Tag.xml)
  2023.02.10  조영대 : 인쇄속도 개선(기간별 Pallet 확정 이력 정보 조회 멀티선택 출력)
  2023.03.13  LEEHJ  : 소형활성화 MES 복사
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_Report_Pallet_Hist : C1Window, IWorkArea
    {
        object[] tmps;
        string tmmp01 = string.Empty;
        string sSHOPID = string.Empty;
        string sPalletID = string.Empty;
        string sRfidTagId = string.Empty;
        string sRfidTagIdLeft = string.Empty;
        string sRfidTagIdRight= string.Empty;
        int iCellQty;
        int iSeqNo;

        string sCarType;
        string sVoltRange;
        string sTitle;
        string sDirection;

        DataTable _dtLotInfo;

        DataTable tmmp02;
        int iMAX_PAGE = 0;
        C1.C1Report.C1Report cr = null;
        C1.C1Report.C1Report cr2 = null;
        C1.C1Report.C1Report crVolt = null;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        public FCS002_Report_Pallet_Hist()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            tmps = C1WindowExtension.GetParameters(this);

            tmmp01 = tmps[0] as string;
            if (LoginInfo.CFG_SHOP_ID == "G184" && tmmp01 == "PalletHis_Tag")
            {
                //C20220404-000249 : dd model barcode in Pallet History
                tmmp01 = "PalletHis_Tag_NJ";
            }

            tmmp02 = tmps[1] as DataTable;
            sSHOPID = tmps[4] as string;

            //if(tmps.Length > 5)
            //{
            //    sRfidTagId = tmps[5] as string;
            //}

            this.Loaded -= Window_Loaded;                     
            txtPage.Text = "1";
            txtPage2.Text = "1";

            // Print 수량 셋팅하기
            if (tmps.Length > 2)
            {
                txtPrintQty.Value = Convert.ToInt32(tmps[2] as string);
                txtPrintQty2.Value = Convert.ToInt32(tmps[2] as string);
            }
            else
            {
                txtPrintQty.Value = 1;
            }

            txtPrintQtyVolt.Value = 1;

            iMAX_PAGE = tmmp02.Rows.Count;
            txtPage_ValueChanged(Convert.ToInt32(txtPage.Text)-1);
            GetVoltInfo();

            if (tmps.Length > 3 && Util.NVC(tmps[3]) == "Y" && LoginInfo.LANGID != "en-US")
            {
                Tag2.Visibility = Visibility.Visible;
                txtPage2_ValueChanged(Convert.ToInt32(txtPage2.Text) - 1);
            }
        }

        /// <summary>
        /// 셀출하전압 
        ///  BIZ : BR_PRD_GET_VOLT_RANGE_FOR_LABEL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetVoltInfo()
        {
            try
            {
                DataSet IndataSet = new DataSet();
                DataTable IndataTable = IndataSet.Tables.Add("INDATA");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("BOXID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["BOXID"] = sPalletID;
                Indata["SHOPID"] = sSHOPID;

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_VOLT_RANGE_FOR_LABEL", "INDATA", "OUTDATA,OUTLOT",(result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Tables["OUTLOT"].Rows?.Count > 0)
                        {
                            sCarType = Util.NVC(result.Tables["OUTDATA"].Select("MNGT_ITEM_CODE = 'CARTYPE'").FirstOrDefault()?["MNGT_ITEM_VALUE"]);
                            sVoltRange = Util.NVC(result.Tables["OUTDATA"].Select("MNGT_ITEM_CODE = 'VOLT_RANGE'").FirstOrDefault()?["MNGT_ITEM_VALUE"]);
                            sTitle = Util.NVC(result.Tables["OUTDATA"].Select("MNGT_ITEM_CODE = 'TITLE'").FirstOrDefault()?["MNGT_ITEM_VALUE"]);
                            sDirection = Util.NVC(result.Tables["OUTDATA"].Select("MNGT_ITEM_CODE = 'DIRECTION'").FirstOrDefault()?["MNGT_ITEM_VALUE"]);
                            _dtLotInfo = result.Tables["OUTLOT"].Copy();                            
                            tabVolt.Visibility = Visibility.Visible;
                            previewVolt();
                        }                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                    }
                },IndataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCopies = int.Parse(txtPrintQty.Value.ToString());

                for (int i = 0; i < iMAX_PAGE; i++)
                {
                    txtPage.Text = (i + 1).ToString();
                    txtPage_ValueChanged(i);

                    C1.C1Preview.C1PrintManager pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;

                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                        ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                    // Print수량만큼 출력함...
                    for (int page = 0; page < iCopies; page++)
                    {
                        pm.Print(ps);
                    }
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        private void btnPrint2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCopies = int.Parse(txtPrintQty2.Value.ToString());

                for (int i = 0; i < iMAX_PAGE; i++)
                {
                    txtPage2.Text = (i + 1).ToString();
                    txtPage2_ValueChanged(i);

                    C1.C1Preview.C1PrintManager pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr2;

                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                        ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                    // Print수량만큼 출력함...
                    for (int page = 0; page < iCopies; page++)
                    {
                        pm.Print(ps);
                    }
                }

                //this.DialogResult = MessageBoxResult.OK;
                //this.Close();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        /// <summary>
        /// 총출하수량, 구성차수 수정
        ///  BIZ : BR_PRD_UPD_PACK_WRK_CP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(txtShipQty.Text, out iCellQty))
                {
                    // SFU3435 숫자만 입력해주세요
                    Util.MessageValidation("SFU3435");
                    return;
                }

                if (!int.TryParse(txtConbineSeq.Text, out iSeqNo))
                {
                    // SFU3435 숫자만 입력해주세요
                    Util.MessageValidation("SFU3435");
                    return;
                }

                DataSet IndataSet = new DataSet();
                DataTable IndataTable = IndataSet.Tables.Add("INDATA");
                IndataTable.Columns.Add("BOXID", typeof(string));
                IndataTable.Columns.Add("PACK_WRK_CELL_QTY", typeof(string));
                IndataTable.Columns.Add("PACK_WRK_SEQNO", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["BOXID"] = sPalletID;
                Indata["PACK_WRK_CELL_QTY"] = iCellQty;
                Indata["PACK_WRK_SEQNO"] = iSeqNo; 

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_PACK_WRK_CP", "INDATA", null, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (cr.Fields.Contains("CONBINESEQ1"))
                        {
                            cr.Fields["CONBINESEQ1"].Text = String.Format("{0:#,###0}", iSeqNo);
                        }

                        if (cr.Fields.Contains("SHIPQTY"))
                        {
                            cr.Fields["SHIPQTY"].Text = String.Format("{0:#,###0}", iCellQty);
                        }

                        c1DocumentViewer.Document = cr.FixedDocumentSequence;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                    }
                }, IndataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void previewVolt()
        {
            try
            {
                crVolt = new C1.C1Report.C1Report();
                crVolt.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.FCS002.Report." + "Pallet_Lot.xml"))
                {
                    crVolt.Load(stream, "Pallet_Lot");

                    if (crVolt.Fields.Contains("Title"))
                    {
                        crVolt.Fields["Title"].Text = sTitle; 
                    }
                    if (crVolt.Fields.Contains("Volt"))
                    {
                        crVolt.Fields["Volt"].Text = sVoltRange;
                    }
                    if (crVolt.Fields.Contains("CarType"))
                    {
                        crVolt.Fields["CarType"].Text = sCarType;
                    }
                    if (crVolt.Fields.Contains("Direction"))
                    {
                        crVolt.Fields["Direction"].Text = sDirection;
                    }
                    
                    for (int row = 1; row <= _dtLotInfo.Rows.Count; row++)
                    {
                        crVolt.Fields["LOT" + row].Text = Util.NVC(_dtLotInfo.Rows[row - 1]["LOTID"]);
                        crVolt.Fields["QTY" + row].Text = Util.NVC(_dtLotInfo.Rows[row - 1]["QTY"]);
                    }
                }
                
                c1DocumentViewerVolt.Document = crVolt.FixedDocumentSequence;
                //var pm = new C1.C1Preview.C1PrintManager();
                //pm.Document = crVolt.Document;
                //System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                //if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                //    ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                //pm.Print(ps);
                // SFU1889	정상 처리 되었습니다
                //Util.MessageInfo("SFU1889");
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        private void txtPage_ValueChanged(int iPage)
        {
            try
            {                    
                string filename = string.Empty;
                string reportname = string.Empty;

                //C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                reportname = tmmp01;
                filename = tmmp01 + ".xml";
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                //using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + filename))
                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.FCS002.Report." + filename))
                {
                    cr.Load(stream, reportname);

                    for (int col = 0; col < tmmp02.Columns.Count; col++)
                    {
                        string strColName = tmmp02.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp02.Rows[iPage][strColName].ToString();

                        // ("LOTID_" + L_" + i  "BCD" + i.ToString(), typeof(string));
                    //    else if (strColName == "LOT" + 20*iPage)
                         
                    }

                    if (cr.Fields.Contains("CONBINESEQ1"))
                    {
                        if (int.TryParse(cr.Fields["CONBINESEQ1"].Text, out iSeqNo))
                            txtConbineSeq.Text = iSeqNo.ToString();
                    }

                    if (cr.Fields.Contains("SHIPQTY"))
                    {
                        if (int.TryParse(cr.Fields["SHIPQTY"].Text.Replace(",", string.Empty), out iCellQty))
                            txtShipQty.Text = iCellQty.ToString(); 
                    }

                    if (cr.Fields.Contains("PALLETID"))
                    {
                        sPalletID = cr.Fields["PALLETID"].Text;
                    }

                    //2023.02.01 이윤중 - RFID TAGID 정보 추가 
                    if (cr.Fields.Contains("RFID"))
                    {
                        if(!string.IsNullOrEmpty(sPalletID))
                        {
                            DataTable objectDicIndataTable = new DataTable();
                            objectDicIndataTable.Columns.Add("BOXID", typeof(string));

                            DataRow objectDicIndata = objectDicIndataTable.NewRow();
                            objectDicIndata["BOXID"] = sPalletID;
                            objectDicIndataTable.Rows.Add(objectDicIndata);

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BOX", "INDATA", "OUTDATA", objectDicIndataTable);

                            if(dtResult.Rows.Count > 0)
                            {
                                if(dtResult.Rows[0]["TAG_ID"].ToString().Length > 0)
                                {
                                    sRfidTagId = dtResult.Rows[0]["TAG_ID"].ToString();
                                    int idxSplit = sRfidTagId.Length - 4;
                                    
                                    sRfidTagIdLeft= sRfidTagId.Substring(0, idxSplit);
                                    sRfidTagIdRight = sRfidTagId.Substring(idxSplit, sRfidTagId.Length - idxSplit);

                                    //cr.Fields["RFID"].Text = dtResult.Rows[0]["TAG_ID"].ToString();
                                    //2023.02.01 - 5자리 + 4자리(size+Bold) 나뉘어서 표기하도록 변경
                                    cr.Fields["RFID"].Text = sRfidTagIdLeft;
                                    cr.Fields["RFID2"].Text = sRfidTagIdRight;
                                }
                            }
                        }
                    }

                    if (cr.Fields.Contains("MODEL_TITLE"))
                    {
                        cr.Fields["MODEL_TITLE"].Text = cr.Fields["MODEL"].Text;
                    }

                    if (cr.Fields.Contains("BARCODE2") && tmmp02.Columns.Contains("MODEL") && !string.IsNullOrEmpty(tmmp02.Rows[0]["MODEL"].ToString()))
                    {
                        /*
                        원래는 이 팝업을 호출하는 모든 화면을 수정해서 모델 파라미터를 받아야 하는데 그러기에는 수정해야 화면이 너무 많어서 
                        tmmp02.Rows[0]["MODEL"] 문자열 짤라서 사용함.
                        tmmp02.Rows[0]["MODEL"] 에는 '모델 (프로젝트명)' 이런식으로 데이터가 들어올수도 있고 '모델' 만 들어올수도 있음
                        */
                        string modelTitle = tmmp02.Rows[0]["MODEL"].ToString();
                        int index = modelTitle.IndexOf('(');
                        string model = string.Empty;
                        if (index > 0)
                        {
                            model = modelTitle.Substring(0, index).Trim();
                        }
                        else
                        {
                            model = modelTitle.Substring(0, modelTitle.Length).Trim();
                        }
                        
                        cr.Fields["BARCODE2"].Text = model;
                    }

                    int nCount = cr.Fields.Count;
                    string strLabelName = string.Empty;
                    string strLabelTextName = string.Empty;
                    string strLabelFlag = string.Empty;
                    string strObjectID = string.Empty;
                    string strText = string.Empty;         
                    strLabelFlag = "lbl_";
                    for (int i = 0; i < nCount; i++)
                    {
                        strLabelName = cr.Fields[i].Name;
                        strLabelTextName = cr.Fields[i].Name;
                        if (strLabelName.Contains(strLabelFlag))
                        {
                            strObjectID = strLabelTextName.Remove(0, 4);

                            int idx = strObjectID.IndexOf("_");
                            if (idx >= 0 && strObjectID.Length > idx)
                            {
                                int num = 0;
                                string sNumber = strObjectID.Substring(idx+1);
                                if (int.TryParse(sNumber, out num))
                                {
                                    strObjectID = strObjectID.Substring(0, idx);
                                }
                            }
                            strText = ObjectDic.Instance.GetObjectName(strObjectID.Replace(" ",""));
                            cr.Fields[i].Text = strText;
                        }
                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;                
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage > 1 && iCurPage <= iMAX_PAGE)
            {
                txtPage.Text = (iCurPage - 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            }
                        
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage >= 1 && iCurPage < iMAX_PAGE)
            {
                txtPage.Text = (iCurPage + 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            }

        }

        private void btnDown2_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage2.Text);
            if (iCurPage > 1 && iCurPage <= iMAX_PAGE)
            {
                txtPage2.Text = (iCurPage - 1).ToString();
                txtPage2_ValueChanged(Convert.ToInt32(txtPage2.Text) - 1);
            }

        }

        private void btnUp2_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage2.Text);
            if (iCurPage >= 1 && iCurPage < iMAX_PAGE)
            {
                txtPage2.Text = (iCurPage + 1).ToString();
                txtPage2_ValueChanged(Convert.ToInt32(txtPage2.Text) - 1);
            }

        }

        private void txtPage2_ValueChanged(int iPage)
        {
            try
            {
                string filename = string.Empty;
                string reportname = string.Empty;
                string sLang = "en-US";

                //C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                cr2 = new C1.C1Report.C1Report();
                cr2.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                reportname = tmmp01;
                filename = tmmp01 + ".xml";
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.FCS002.Report." + filename))
                {
                    cr2.Load(stream, reportname);

                    for (int col = 0; col < tmmp02.Columns.Count; col++)
                    {
                        string strColName = tmmp02.Columns[col].ColumnName;
                        if (cr2.Fields.Contains(strColName)) cr2.Fields[strColName].Text = tmmp02.Rows[iPage][strColName].ToString();
                    }

                    if (cr2.Fields.Contains("SHIP_LOC"))
                    {
                        cr2.Fields["SHIP_LOC"].Text = tmmp02.Rows[0]["SHIP_LOC_EN"].ToString();
                    }

                    if (cr2.Fields.Contains("LINE"))
                    {
                        cr2.Fields["LINE"].Text = tmmp02.Rows[0]["LINE_EN"].ToString();
                    }

                    //if (cr2.Fields.Contains("CONBINESEQ1"))
                    //{
                    //    if (int.TryParse(cr2.Fields["CONBINESEQ1"].Text, out iSeqNo))
                    //        txtConbineSeq.Text = iSeqNo.ToString();
                    //}

                    //if (cr2.Fields.Contains("SHIPQTY"))
                    //{
                    //    if (int.TryParse(cr2.Fields["SHIPQTY"].Text.Replace(",", string.Empty), out iCellQty))
                    //        txtShipQty.Text = iCellQty.ToString();
                    //}

                    if (cr2.Fields.Contains("PALLETID"))
                    {
                        sPalletID = cr2.Fields["PALLETID"].Text;
                    }

                    if (cr2.Fields.Contains("MODEL_TITLE"))
                    {
                        cr2.Fields["MODEL_TITLE"].Text = cr2.Fields["MODEL"].Text;
                    }

                    //2023.02.01 이윤중 - RFID TAGID 정보 추가 
                    if (cr2.Fields.Contains("RFID"))
                    {
                        if (!string.IsNullOrEmpty(sRfidTagId))
                        {
                            //cr.Fields["RFID"].Text = dtResult.Rows[0]["TAG_ID"].ToString();
                            //2023.02.01 - 5자리 + 4자리(size+Bold) 나뉘어서 표기하도록 변경
                            cr2.Fields["RFID"].Text = sRfidTagIdLeft;
                            cr2.Fields["RFID2"].Text = sRfidTagIdRight;
                        }
                    }

                    if (cr2.Fields.Contains("BARCODE2") && tmmp02.Columns.Contains("MODEL") && !string.IsNullOrEmpty(tmmp02.Rows[0]["MODEL"].ToString()))
                    {
                        /*
                        원래는 이 팝업을 호출하는 모든 화면을 수정해서 모델 파라미터를 받아야 하는데 그러기에는 수정해야 화면이 너무 많어서 
                        tmmp02.Rows[0]["MODEL"] 문자열 짤라서 사용함.
                        tmmp02.Rows[0]["MODEL"] 에는 '모델 (프로젝트명)' 이런식으로 데이터가 들어올수도 있고 '모델' 만 들어올수도 있음
                        */
                        string modelTitle = tmmp02.Rows[0]["MODEL"].ToString();
                        int index = modelTitle.IndexOf('(');
                        string model = string.Empty;
                        if (index > 0)
                        {
                            model = modelTitle.Substring(0, index).Trim();
                        }
                        else
                        {
                            model = modelTitle.Substring(0, modelTitle.Length).Trim();
                        }
                        cr2.Fields["BARCODE2"].Text = model;
                    }

                    int nCount = cr2.Fields.Count;
                    string strLabelName = string.Empty;
                    string strLabelTextName = string.Empty;
                    string strLabelFlag = string.Empty;
                    string strObjectID = string.Empty;
                    string strText = string.Empty;
                    strLabelFlag = "lbl_";

                    DataTable objectDicIndataTable = new DataTable();
                    objectDicIndataTable.Columns.Add("LANGID", typeof(string));
                    objectDicIndataTable.Columns.Add("OBJECTIUSE", typeof(string));

                    DataRow objectDicIndata = objectDicIndataTable.NewRow();
                    objectDicIndata["LANGID"] = sLang;
                    objectDicIndata["OBJECTIUSE"] = "Y";
                    objectDicIndataTable.Rows.Add(objectDicIndata);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("CUS_SEL_OBJECTDIC_BAS", "INDATA", "OUTDATA", objectDicIndataTable);

                    for (int i = 0; i < nCount; i++)
                    {
                        strLabelName = cr2.Fields[i].Name;
                        strLabelTextName = cr2.Fields[i].Name;
                        if (strLabelName.Contains(strLabelFlag))
                        {
                            strObjectID = strLabelTextName.Remove(0, 4);

                            int idx = strObjectID.IndexOf("_");
                            if (idx >= 0 && strObjectID.Length > idx)
                            {
                                int num = 0;
                                string sNumber = strObjectID.Substring(idx + 1);
                                if (int.TryParse(sNumber, out num))
                                {
                                     strObjectID = strObjectID.Substring(0, idx);                                    
                                }
                            }
                            DataRow dr = dtResult.Select("OBJECTID = '" + strObjectID.Replace(" ", "") + "'").FirstOrDefault();
                            strText = dr == null ? strObjectID : Util.NVC(dr["OBJECTNAME"]);
                            cr2.Fields[i].Text = strText;
                        }
                    }                    
                }

                c1DocumentViewer2.Document = cr2.FixedDocumentSequence;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnPrintVolt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCopies = int.Parse(txtPrintQtyVolt.Value.ToString());
                // Print수량만큼 출력함...
                for (int iPrint = 0; iPrint < iCopies; iPrint++)
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = crVolt;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                        ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                    pm.Print(ps);
                }
               // this.DialogResult = MessageBoxResult.OK;
              //  this.Close();

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
    }
}
