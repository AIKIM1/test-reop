/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2024.05.23  오수현 : E20240312-000577 : Data Matrix Code Sequence 변경, 2D BCR 부분 링크 제거 및 수정
                                       : tbP -> tbBP로 변경




 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Globalization;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_041 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        //string[,] _venderInfo = { { "314333", "MCCM127510A2", "1273102006", "13231" }, 
        //                          { "314333", "MCCM120010A1", "1271024000", "13230" } };
        DataTable _currentData = null;
        bool _stopPrint = false;

        string _sPGM_ID = "BOX001_041";

        #region Declaration & Constructor 



        public BOX001_041()
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
            Initialize();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            InitializeCombo();
            //GetEqptWrkInfo();
            SetEvents();

        }

        private void SetEvents()
        {   /*
            txtBatchNo1.LostFocus -= txtBatchNo1_LostFocus;
            txt12S.MouseEnter += BcrTextMouseEnter;
            txtP.MouseEnter += BcrTextMouseEnter;
            txt1P.MouseEnter += BcrTextMouseEnter;
            txt31P.MouseEnter += BcrTextMouseEnter;
            txt12V.MouseEnter += BcrTextMouseEnter;
            txt10V.MouseEnter += BcrTextMouseEnter;
            txt2P.MouseEnter += BcrTextMouseEnter;
            txt20P.MouseEnter += BcrTextMouseEnter;
            txt6D.MouseEnter += BcrTextMouseEnter;
            txt14D.MouseEnter += BcrTextMouseEnter;
            txt30P.MouseEnter += BcrTextMouseEnter;
            txtZ.MouseEnter += BcrTextMouseEnter;
            txtK.MouseEnter += BcrTextMouseEnter;
            txt16K.MouseEnter += BcrTextMouseEnter;
            txtV.MouseEnter += BcrTextMouseEnter;
            txt3S.MouseEnter += BcrTextMouseEnter;
            txtQ.MouseEnter += BcrTextMouseEnter;
            txt20T.MouseEnter += BcrTextMouseEnter;
            txt1T.MouseEnter += BcrTextMouseEnter;
            txt2T.MouseEnter += BcrTextMouseEnter;
            txt1Z.MouseEnter += BcrTextMouseEnter;

            txt12S.MouseLeave += BcrTextMouseLeave;
            txtP.MouseLeave += BcrTextMouseLeave;
            txt1P.MouseLeave += BcrTextMouseLeave;
            txt31P.MouseLeave += BcrTextMouseLeave;
            txt12V.MouseLeave += BcrTextMouseLeave;
            txt10V.MouseLeave += BcrTextMouseLeave;
            txt2P.MouseLeave += BcrTextMouseLeave;
            txt20P.MouseLeave += BcrTextMouseLeave;
            txt6D.MouseLeave += BcrTextMouseLeave;
            txt14D.MouseLeave += BcrTextMouseLeave;
            txt30P.MouseLeave += BcrTextMouseLeave;
            txtZ.MouseLeave += BcrTextMouseLeave;
            txtK.MouseLeave += BcrTextMouseLeave;
            txt16K.MouseLeave += BcrTextMouseLeave;
            txtV.MouseLeave += BcrTextMouseLeave;
            txt3S.MouseLeave += BcrTextMouseLeave;
            txtQ.MouseLeave += BcrTextMouseLeave;
            txt20T.MouseLeave += BcrTextMouseLeave;
            txt1T.MouseLeave += BcrTextMouseLeave;
            txt2T.MouseLeave += BcrTextMouseLeave;
            txt1Z.MouseLeave += BcrTextMouseLeave;
            */
        }



        private void InitializeCombo()
        {
        }
        #endregion


        #region Methods

        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        private void GetEqptWrkInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.CELL_BOXING;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
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
                });
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
        private void GetSalesOrderInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHIPMENT_NO");
                RQSTDT.Columns.Add("PO_NO");
                RQSTDT.Columns.Add("SHOPID");
                DataRow inDataRow = RQSTDT.NewRow();
                inDataRow["SHIPMENT_NO"] = txtSalesOrder.Text;
                inDataRow["PO_NO"] = null; //뭐 던져야하는지 확인
                inDataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(inDataRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_SHIPMENT_INFO", "RQSTDT", "RSLTDT", RQSTDT, (RSLTDT, ex) =>
                 {
                     if (ex != null)
                     {
                         Util.MessageException(ex);
                         return;
                     }
                     Util.GridSetData(dgSalesOrder, RSLTDT, FrameOperation, false);

                 });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
        private void GetLabelPrintItem()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("SHIPMENT_NO");
                inData.Columns.Add("PO_NO");
                inData.Columns.Add("SHIPTO_ID");
                inData.Columns.Add("SHOPID");
                inData.Columns.Add("AREAID");
                DataRow inDataRow = inData.NewRow();
                inDataRow["SHIPMENT_NO"] = dgSalesOrder.SelectedItem.GetValue("SHIPMENT_NO");
                inDataRow["PO_NO"] = dgSalesOrder.SelectedItem.GetValue("PO_NO");
                inDataRow["SHIPTO_ID"] = "M1008";
                inDataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData.Rows.Add(inDataRow);
                DataSet ds = new DataSet();
                ds = inDataSet;
                string xmltxt = ds.GetXml();
                
                DataSet dsRSLT = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_PRT_MNGT_ITEM_FROM_SO_FM", "INDATA", "OUTINBOX,OUTOUTBOX,OUTPLT", inDataSet);

                _currentData = dsRSLT.Tables["OUTOUTBOX"];
                //dsRSLT.Reset();
                SetMatLabelPreview(grd_LabelPreview, _currentData);
                // bcBoxID.ToolTip = bcBoxID.Text;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetMatLabelPreview(DependencyObject obj, DataTable dt)
        {

            //lbPartNo.Content = MatLabelItems.partNo;
            //lbManuPartNum.Content = MatLabelItems.manuPartNum;
            //lbOrderingCode.Content = MatLabelItems.orderingCode;
            //lbAddInfo.Content = MatLabelItems.addInfo;
            //lbShippingNote.Content = MatLabelItems.shippingNote;
            //lblPartNoVender.Content = MatLabelItems.partnumberVender;
            //lblChargeQty.Content = MatLabelItems.chargeQuantity;
            //lbExpDate.Content = dtpDateCode.SelectedDate.GetValueOrDefault(DateTime.Now).AddDays(365).ToString("ddMMyyyy");
            //bcPartNoSupplier.Text = MatLabelItems.partnumberVender;
            //bcChargeQty.Text = MatLabelItems.chargeQuantity;
            try
            {
                Label lb = obj as Label;
                TextBox tBox = obj as TextBox;
                TextBlock tBlock = obj as TextBlock;
                C1.WPF.BarCode.C1BarCode bcd = obj as C1.WPF.BarCode.C1BarCode;
                C1.WPF.C1NumericBox nb = obj as C1.WPF.C1NumericBox;
                C1.WPF.DateTimeEditors.C1DatePicker dtp = obj as C1.WPF.DateTimeEditors.C1DatePicker;

                if (lb != null && lb.ToolTip != null)
                {
                    lb.Content = dt.Select("MNGT_ITEM_NAME='" + lb.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + lb.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                }
                else if (tBox != null && tBox.ToolTip != null)
                {
                    tBox.Text = dt.Select("MNGT_ITEM_NAME='" + tBox.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + tBox.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                }
                else if (tBlock != null && tBlock.ToolTip != null)
                {
                    tBlock.Text = dt.Select("MNGT_ITEM_NAME='" + tBlock.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + tBlock.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                }
                else if (bcd != null && bcd.ToolTip != null)
                {
                    bcd.Text = dt.Select("MNGT_ITEM_NAME='" + bcd.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + bcd.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                    if (bcd.ToolTip.Equals("BCD_MATRIX"))
                        txtBCR.Text = bcd.Text;
                }
                else if (nb != null && nb.ToolTip != null)
                {
                    txtQuantity.ValueChanged -= txtQuantity_ValueChanged;
                    nb.Value = dt.Select("MNGT_ITEM_NAME='" + nb.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + nb.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].SafeToDouble() : 200;
                    txtQuantity.ValueChanged += txtQuantity_ValueChanged;
                }
                else if (dtp != null && dtp.ToolTip != null)
                {
                    dtp.Text = dt.Select("MNGT_ITEM_NAME='" + dtp.ToolTip + "'").Length > 0 ? dt.Select("MNGT_ITEM_NAME='" + dtp.ToolTip + "'")[0]["MNGT_ITEM_VALUE"].ToString() : "";
                }


                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    SetMatLabelPreview(VisualTreeHelper.GetChild(obj, i), dt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void UpdateLabelPreview()
        {
            if (_currentData == null)
                return;
            try
            {
                //  double aa = txtQuantity.Value;
                DataTable inTarget = _currentData.Copy();
                inTarget.TableName = "INTARGET";
                DataRow drQty = inTarget.Select("MNGT_ITEM_NAME='QUANTITY'").FirstOrDefault();
                DataRow drDateCode = inTarget.Select("MNGT_ITEM_NAME='DATE_CODE'").FirstOrDefault();
                DataRow drBatchNo1 = inTarget.Select("MNGT_ITEM_NAME='BATCH_NO1'").FirstOrDefault();
                DataRow drBatchNo2 = inTarget.Select("MNGT_ITEM_NAME='BATCH_NO2'").FirstOrDefault();

                drQty["MNGT_ITEM_VALUE"] = txtQuantity.Value;
                drDateCode["MNGT_ITEM_VALUE"] = dtpDateCode.SelectedDate.GetValueOrDefault(DateTime.Now).ToString("ddMMyyyy");
                drBatchNo1["MNGT_ITEM_VALUE"] = txtBatchNumber.Text;
                drBatchNo2["MNGT_ITEM_VALUE"] = txtCellRanking.Text;

                DataSet inDataDs = new DataSet();

                DataTable inData = inDataDs.Tables.Add("INDATA");
                inData.Columns.Add("SHIPTO_ID");
                inData.Columns.Add("AREAID");
                DataRow inDataRow = inData.NewRow();
                inDataRow["SHIPTO_ID"] = "M1008";
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData.Rows.Add(inDataRow);

                DataTable inSource = inDataDs.Tables.Add("INSOURCE");
                inSource.Columns.Add("MNGT_ITEM_NAME");
                inSource.Columns.Add("MNGT_ITEM_VALUE");

                inDataDs.Tables.Add(inTarget);

                DataSet ds = new DataSet();
                ds = inDataDs;
                string xmltxt = ds.GetXml();

                DataSet updatedDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_MGNT_ITEM_FM", "INDATA,INSOURCE,INTARGET", "OUTDATA", inDataDs);
                _currentData = updatedDs.Tables["OUTDATA"];
                // DateTime.ParseExact(inTarget.Select("MNGT_ITEM_NAME='DATE_CODE'").FirstOrDefault().GetValue("MNGT_ITEM_VALUE").ToString(), "ddMMyyyy", null);
                SetMatLabelPreview(grd_LabelPreview, _currentData);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void PrintMatLabel()
        {
            string zplCode = string.Empty;
            string lblCode = string.Empty;

            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;
            DataRow drPrtInfo = null;

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                return;
            try
            {
                string sBizRule = "BR_PRD_GET_ZPLCODE_LABEL_FM";

                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("LOTID");
                inData.Columns.Add("PRODID");
                inData.Columns.Add("SHIPTO_ID");
                inData.Columns.Add("LABEL_CODE");
                inData.Columns.Add("USERID");
                inData.Columns.Add("AREAID");
                inData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataRow inDataRow = inData.NewRow();
                inDataRow["LOTID"] = _currentData.Select("MNGT_ITEM_NAME='PACKAGE_ID'").FirstOrDefault().GetValue("MNGT_ITEM_VALUE").ToString(); // _currentData.Rows[0].GetValue("PACKAGE_ID").ToString(); 
                inDataRow["PRODID"] = dgSalesOrder.SelectedItem.GetValue("PRODID").ToString();
                inDataRow["SHIPTO_ID"] = dgSalesOrder.SelectedItem.GetValue("SHIPTO_ID").ToString();
                inDataRow["LABEL_CODE"] = _currentData.Rows[0].GetValue("LABEL_CODE").ToString();
                inDataRow["USERID"] = !String.IsNullOrEmpty(txtWorker.Text) ? txtWorker.Tag : "";
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataRow["PGM_ID"] = _sPGM_ID;
                inDataRow["BZRULE_ID"] = sBizRule;
                inData.Rows.Add(inDataRow);

                DataTable inPrintTable = inDataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");
                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = sCopy;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataTable inItem = _currentData.Copy();
                inItem.TableName = "INITEM";
                inDataSet.Tables.Add(inItem);

                DataSet ds = new DataSet();
                ds = inDataSet;
                string xmltxt = ds.GetXml();

                //DataSet resultSet = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_ZPLCODE_LABEL_FM", "INDATA,INPRINT,INITEM", "OUTDATA", inDataSet, null);
                DataSet resultSet = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT,INITEM", "OUTDATA", inDataSet, null);

                if (resultSet != null && resultSet.Tables.Count > 0)
                {
                    if ((resultSet.Tables.IndexOf("OUTDATA") > -1) && resultSet.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        lblCode = resultSet.Tables["OUTDATA"].Rows[0]["LABEL_CODE"].ToString();
                        zplCode = resultSet.Tables["OUTDATA"].Rows[0]["ZPLCODE"].ToString();
                        if (zplCode.Split(',')[0].Equals("1"))
                        {
                            ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return;
                        }
                        else
                        {
                            zplCode = zplCode.Substring(2);
                        }

                        if (inItem.Select("MNGT_ITEM_NAME='ROHS'").FirstOrDefault().GetValue("MNGT_ITEM_VALUE").Equals("Y"))
                        {

                            zplCode = zplCode.Replace("^GFA", "^GFA,01660,01660,00020," +
                            "00, 00000000000000000C, 000000000000000033, 0000000000000000C0C0, 00000000000000010020, 00000000000000060018, 00000000000000180006, 00000000000000200001, 0000000000"
                            + "0000C00000C0, 0000000000000300000030, 0000000000000C0000000C, 0000000000001000000002, 000000000000600000000180, 000000000001800000000060, 000000000002000000000010, 00"
                            + "000000000C00000000000C, 000000000030000000000003, 00000000004000000000000080, 00000000018000000000000060, 00000000060000000000000018, 00000000080000000000000004, 00"
                            + "000000300000000000000003, 00000000C00000000000000000C0, 0000000100000000000000000020, 0000000600000000000000000018, 0000001800000000000000000006, 0000002000000000"
                            + "000000000001, 000000C000000000000000000000C0, 000003000000000000000000000030, 000004000000000000000000000008, 000018000000000000000000000006, 00006000000000000000"
                            + "000000000180, 00018000000000000000000000000060, 00020000000000000000000000000010, 000C000000000000000000000000000C, 00300000000000000000000000000003, 004000000000"
                            + "0000000000000000000080, 0180000000000000000000000000000060, 0600000000000000000000000000000018, 0800000000000000000000000000000004, 3000000000000000000000000000"
                            + "000003, 400000000000000000000000000000000080, 3000000000000000000000000000000003, 0800000000000000000000000000000004, 0600000000000000000000000000000018, 01800000"
                            + "00000000000000000000000060, 0040000000000000000000000000000080, 00300000000000000000000000000003, 000C000000000000000000000000000C, 0002000000000000000000000000"
                            + "0010, 00018000000000000000000000000060, 00006000000000000000000000000180, 000018000000000000000000000006, 000004000000000000000000000008, 000003000000000000000000"
                            + "000030, 000000C000000000000000000000C0, 0000002000000000000000000001, 0000001800000000000000000006, 0000000600000000000000000018, 0000000100000000000000000020, 0000"
                            + "0000C00000000000000000C0, 00000000300000000000000003, 00000000080000000000000004, 00000000060000000000000018, 00000000018000000000000060, 000000000040000000000000"
                            + "80, 000000000030000000000003, 00000000000C00000000000C, 000000000002000000000010, 000000000001800000000060, 000000000000600000000180, 0000000000001000000002, 00000000"
                            + "00000C0000000C, 0000000000000300000030, 00000000000000C00000C0, 00000000000000200001, 00000000000000180006, 00000000000000060018, 00000000000000010020, 000000000000"
                            + "0000C0C0, 000000000000000033, 00000000000000000C, 00, ");
                        }
                        else
                        {
                            zplCode = zplCode.Replace("^A0N,24,24^FO1205,257^FDRoHS^FS", "");
                            zplCode = zplCode.Replace("^A0N,14,14^FO1204,288^FD2002/95/EC^FS", "");
                        }
                    }
                    PrintLabel(zplCode, drPrtInfo);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Events
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //Validation 추가
            GetSalesOrderInfo();
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //Validation
            if (dgSalesOrder.Rows.Count == 0 || dgSalesOrder.SelectedItem == null || dgSalesOrder.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU4091"); //S/O 정보를 선택하세요.
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }
            if (!string.IsNullOrEmpty(txtBatchNumber.Text) && txtBatchNumber.Text.Length < 8)
            {
                Util.MessageValidation("SFU4102"); //1. Batch No 항목값이 유효하지 않습니다.
                return;
            }

            int prtCount = (int)nbPrintQty.Value;
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4103", prtCount), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
             {//Bosch라벨 " + prtCount + " 장 발행하시겠습니까?
                if (result == MessageBoxResult.OK)
                 {
                     _stopPrint = false;
                     for (int i = 0; i < prtCount; i++)
                     {
                         if (!_stopPrint)
                         {
                             PrintMatLabel();
                             UpdateLabelPreview();
                             txtQuantity.Value = 200;
                             nbPrintQty.Value = nbPrintQty.Value - 1;
                             System.Threading.Thread.Sleep(500);
                             System.Windows.Forms.Application.DoEvents();
                         }
                     }
                     _stopPrint = false;
                 }
             });

        }
        private void shift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = sender as CMM_SHIFT_USER2;

            if (shiftPopup.DialogResult == MessageBoxResult.OK)
            {
                /*
                 * 2018-09-05 오화백
                 * 작업자 정보 저장안하도록 수정
                 */
                txtShift.Text = Util.NVC(shiftPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(shiftPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(shiftPopup.USERNAME);
                txtWorker.Tag = Util.NVC(shiftPopup.USERID);


                //GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(shiftPopup);
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = new CMM_SHIFT_USER2();
            shiftPopup.FrameOperation = this.FrameOperation;

            if (shiftPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = LoginInfo.CFG_EQPT_ID;

                //2018-09-05 오화백 작업자 저장 안되도록 수정
                Parameters[7] = "N"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(shiftPopup, Parameters);

                shiftPopup.Closed += new EventHandler(shift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(shiftPopup);
                shiftPopup.BringToFront();
            }
        }
        private void dgSalesOrder_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            //SetMatLabelItems();
            //SetMatLabelPreview();
            txtBatchNumber.LostFocus -= txtBatchNo1_LostFocus;
            dtpDateCode.IsEnabled = true;
            if (dgSalesOrder.SelectedItem == null)
                return;

            if (!string.IsNullOrEmpty(dgSalesOrder.SelectedItem.GetValue("ADD_INFO").ToString()))
            {
                dtpDateCode.Text = string.Empty;
                lbExpDate.Content = string.Empty;
                dtpDateCode.IsEnabled = false;
                txtBatchNumber.LostFocus += txtBatchNo1_LostFocus;
            }

            GetLabelPrintItem();

        }


        private void dtpDateCode_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //lbExpDate.Content = dtpDateCode.SelectedDate.GetValueOrDefault(DateTime.Now).AddDays(365).ToString("ddMMyyyy");
            UpdateLabelPreview();
        }
        private void txtQuantity_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            //if (_currentData == null)
            //    return;
            //try
            //{
            //    //  double aa = txtQuantity.Value;
            //    DataTable inTarget = _currentData.Copy();
            //    inTarget.TableName = "INTARGET";
            //    DataRow dr = inTarget.Select("MNGT_ITEM_NAME='QUANTITY'").FirstOrDefault();

            //    dr["MNGT_ITEM_VALUE"] = txtQuantity.Value;

            //    DataSet inDataDs = new DataSet();

            //    DataTable inData = inDataDs.Tables.Add("INDATA");
            //    inData.Columns.Add("SHIPTO_ID");
            //    DataRow inDataRow = inData.NewRow();
            //    inDataRow["SHIPTO_ID"] = dgSalesOrder.SelectedItem.GetValue("SHIPTO_ID");
            //    inData.Rows.Add(inDataRow);

            //    DataTable inSource = inDataDs.Tables.Add("INSOURCE");
            //    inSource.Columns.Add("MNGT_ITEM_NAME");
            //    inSource.Columns.Add("MNGT_ITEM_VALUE");

            //    inDataDs.Tables.Add(inTarget);

            //    DataSet ds = new DataSet();
            //    ds = inDataDs;
            //    string xmltxt = ds.GetXml();

            //    DataSet updatedDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_MGNT_ITEM_FM", "INDATA,INSOURCE,INTARGET", "OUTDATA", inDataDs);
            //    _currentData = updatedDs.Tables["OUTDATA"];
            //    SetMatLabelPreview(grd_LabelPreview, _currentData);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}

            UpdateLabelPreview();
        }
        private void txtBatchNo1_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLabelPreview();
        }
        private void txtBatchNo2_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLabelPreview();
        }
        #endregion

        private void btnShippingNotePrint_Click(object sender, RoutedEventArgs e)
        {
            if (dgSalesOrder.SelectedItem == null)
            {
                Util.MessageValidation("SFU4091");//S/O 정보를 선택하세요
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }
            BOX001_041_SHIPPING_NOTE popUp = new BOX001_041_SHIPPING_NOTE { FrameOperation = FrameOperation };
            if (popUp != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = dgSalesOrder.SelectedItem.GetValue("CUST_MTRLID");//Part No
                Parameters[1] = dgSalesOrder.SelectedItem.GetValue("PO_NO"); ;// Purchase(PO No)
                Parameters[2] = dgSalesOrder.SelectedItem.GetValue("SHIPMENT_NO"); //SHIPPING NOTE NO
                Parameters[3] = dgSalesOrder.SelectedItem.GetValue("TOTL_QTY").SafeToInt32(); // TOTL_QTY
                Parameters[4] = _currentData.Select("MNGT_ITEM_NAME='SUPPLIER_ID'").GetValue("MNGT_ITEM_VALUE"); // SUPPLIER_ID
                Parameters[5] = !String.IsNullOrEmpty(txtWorker.Text) ? txtWorker.Tag : "";// 작업자id
                Parameters[6] = lbExpDate.Content;
                C1WindowExtension.SetParameters(popUp, Parameters);
                popUp.ShowModal();
                popUp.CenterOnScreen();
            }
        }

        private void txtBatchNo1_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBatchNumber.Text.Length == 8)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("LOTID");
                    DataRow dr = dt.NewRow();
                    dr["LOTID"] = txtBatchNumber.Text;
                    dt.Rows.Add(dr);

                    DataTable resultDt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROD_DATE_BY_LOTID", "RQSTDT", "RSLTDT", dt);
                    if (resultDt != null || resultDt.Rows.Count > 0)
                    {
                        string prodDate = resultDt.Rows[0].GetValue("PROD_DATE").ToString();
                        string expdate = resultDt.Rows[0].GetValue("PROD_DATE").ToString();

                        dtpDateCode.SelectedDate = DateTime.ParseExact(prodDate, "ddMMyyyy", CultureInfo.InvariantCulture);
                        lbExpDate.Content = expdate;
                        UpdateLabelPreview();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBCR_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBCR.Text))
            {
                ClearBCRView(grd2DBCRContent);
                return;
            }
            /*
            string[] bcrText = txtBCR.Text.Split('@');
            txtHead.Text = bcrText[0] + bcrText[1];
            txt12S.Text = "@" + bcrText[2];
            txtP.Text = "@" + bcrText[3];
            txt1P.Text = "@" + bcrText[4];
            txt31P.Text = "@" + bcrText[5];
            txt12V.Text = "@" + bcrText[6];
            txt10V.Text = "@" + bcrText[7];
            txt2P.Text = "@" + bcrText[8];
            txt20P.Text = "@" + bcrText[9];
            txt6D.Text = "@" + bcrText[10];
            txt14D.Text = "@" + bcrText[11];
            txt30P.Text = "@" + bcrText[12];
            txtZ.Text = "@" + bcrText[13];
            txtK.Text = "@" + bcrText[14];
            txt16K.Text = "@" + bcrText[15];
            txtV.Text = "@" + bcrText[16];
            txt3S.Text = "@" + bcrText[17];
            txtQ.Text = "@" + bcrText[18];
            txt20T.Text = "@" + bcrText[19];
            txt1T.Text = "@" + bcrText[20];
            txt2T.Text = "@" + bcrText[21];
            txt1Z.Text = "@" + bcrText[22];
            txtTail.Text = "@@";
            */

            /* Data Matrix Code Sequence : 
            [)><RS>06<GS>12PGTL3<GS>9K14<GS>1J<GS>14D<GS>16Dproductiondate
            <GS>Pboschpartnumber
            <GS>2P<GS>1Tbatchnumber
            <GS>Qquantity
            <GS>3Q<GS>K<GS>4K<GS>2S<GS>15K<GS>Vsuppliernumber
            <GS>13V<GS>7Q<GS>2K<GS>23PEB/PT
            <GS>30P<GS>12V<GS>1P<GS>33T
            <GS>1Pcellname
            <GS>10Vproductionplant
            <GS>20Pplantcode
            <GS>2Tcellranking<RS><EOT>
            */
            string strBCR = txtBCR.Text.Replace("&lt;RS&gt;", "<RS>").Replace("&lt;GS&gt;", "<GS>");
            int strLength = strBCR.Length;
            txtMatrixCode1.Text = strBCR.Substring(0, strLength / 2);
            txtMatrixCode2.Text = strBCR.Substring(strLength / 2);
        }

        private void ClearBCRView(DependencyObject obj)
        {
            try
            {
                TextBlock tb = obj as TextBlock;

                if (tb != null)
                    tb.Text = string.Empty;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    ClearBCRView(VisualTreeHelper.GetChild(obj, i));

            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private void BcrTextMouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            switch (tb.Name)
            {
                case "txt12S":
                    {
                        tb.ToolTip = "Label Version";
                        break;
                    }
                case "txtP":
                    {
                        tbBP.Foreground = Brushes.Black;
                        tbBP.FontSize = 12;
                        break;
                    }
                case "txt1P":
                    {
                        tb1P.Foreground = Brushes.Black;
                        tb1P.FontSize = 12;
                        break;
                    }
                case "txt31P":
                    {
                        tb31P.Foreground = Brushes.Black;
                        tb31P.FontSize = 12;
                        break;
                    }
                case "txt12V":
                    {
                        tb.ToolTip = "Manufacturer Number";
                        break;
                    }
                case "txt10V":
                    {
                        tb10V.Foreground = Brushes.Black;
                        tb10V.FontSize = 12;
                        break;
                    }
                case "txt2P":
                    {
                        tb2P.Foreground = Brushes.Black;
                        tb2P.FontSize = 12;
                        break;
                    }
                case "txt20P":
                    {
                        tb20P.Foreground = Brushes.Black;
                        tb20P.FontSize = 12;
                        break;
                    }
                case "txt6D":
                    {
                        tb6D.Foreground = Brushes.Black;
                        tb6D.FontSize = 12;
                        break;
                    }
                case "txt14D":
                    {
                        tb14D.Foreground = Brushes.Black;
                        tb14D.FontSize = 12;
                        break;
                    }
                case "txt30P":
                    {
                        tb.ToolTip = "RoHS";
                        break;
                    }
                case "txtZ":
                    {
                        tb.ToolTip = "MS-Level";
                        break;
                    }
                case "txtK":
                    {
                        tbK.Foreground = Brushes.Black;
                        tbK.FontSize = 12;
                        break;
                    }
                case "txt16K":
                    {
                        tb16K.Foreground = Brushes.Black;
                        tb16K.FontSize = 12;
                        break;
                    }
                case "txtV":
                    {
                        tbV.Foreground = Brushes.Black;
                        tbV.FontSize = 12;
                        break;
                    }
                case "txt3S":
                    {
                        tb3S.Foreground = Brushes.Black;
                        tb3S.FontSize = 12;
                        break;
                    }
                case "txtQ":
                    {
                        tbQ.Foreground = Brushes.Black;
                        tbQ.FontSize = 12;
                        break;
                    }
                case "txt20T":
                    {
                        tb.ToolTip = "Batch-Counter";
                        break;
                    }
                case "txt1T":
                    {
                        tb1T.Foreground = Brushes.Black;
                        tb1T.FontSize = 12;
                        break;
                    }
                case "txt2T":
                    {
                        tb2T.Foreground = Brushes.Black;
                        tb2T.FontSize = 12;
                        break;
                    }
                case "txt1Z":
                    {
                        tb1Z.Foreground = Brushes.Black;
                        tb1Z.FontSize = 12;
                        break;
                    }
                default:
                    break;
            }
        }
        void BcrTextMouseEnter(object sender, EventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            switch (tb.Name)
            {
                case "txt12S":
                    {
                        tb.ToolTip = "Label Version";
                        break;
                    }
                case "txtP":
                    {
                        tbBP.Foreground = Brushes.Red;
                        tbBP.FontSize = 15;
                        tbBP.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt1P":
                    {
                        tb1P.Foreground = Brushes.Red;
                        tb1P.FontSize = 15;
                        tb1P.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt31P":
                    {
                        tb31P.Foreground = Brushes.Red;
                        tb31P.FontSize = 15;
                        tb31P.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt12V":
                    {
                        tb.ToolTip = "Manufacturer Number";
                        break;
                    }
                case "txt10V":
                    {
                        tb10V.Foreground = Brushes.Red;
                        tb10V.FontSize = 15;
                        tb10V.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt2P":
                    {
                        tb2P.Foreground = Brushes.Red;
                        tb2P.FontSize = 15;
                        tb2P.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt20P":
                    {
                        tb20P.Foreground = Brushes.Red;
                        tb20P.FontSize = 15;
                        tb20P.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt6D":
                    {
                        tb6D.Foreground = Brushes.Red;
                        tb6D.FontSize = 15;
                        tb6D.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt14D":
                    {
                        tb14D.Foreground = Brushes.Red;
                        tb14D.FontSize = 15;
                        tb14D.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt30P":
                    {
                        tb.ToolTip = "RoHS";
                        break;
                    }
                case "txtZ":
                    {
                        tb.ToolTip = "MS-Level";
                        break;
                    }
                case "txtK":
                    {
                        tbK.Foreground = Brushes.Red;
                        tbK.FontSize = 15;
                        tbK.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt16K":
                    {
                        tb16K.Foreground = Brushes.Red;
                        tb16K.FontSize = 15;
                        tb16K.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txtV":
                    {
                        tbV.Foreground = Brushes.Red;
                        tbV.FontSize = 15;
                        tbV.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt3S":
                    {
                        tb3S.Foreground = Brushes.Red;
                        tb3S.FontSize = 15;
                        tb3S.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txtQ":
                    {
                        tbQ.Foreground = Brushes.Red;
                        tbQ.FontSize = 15;
                        tbQ.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt20T":
                    {
                        tb.ToolTip = "Batch-Counter";
                        break;
                    }
                case "txt1T":
                    {
                        tb1T.Foreground = Brushes.Red;
                        tb1T.FontSize = 15;
                        tb1T.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt2T":
                    {
                        tb2T.Foreground = Brushes.Red;
                        tb2T.FontSize = 15;
                        tb2T.FontWeight = FontWeights.Bold;
                        break;
                    }
                case "txt1Z":
                    {
                        tb1Z.Foreground = Brushes.Red;
                        tb1Z.FontSize = 15;
                        tb1Z.FontWeight = FontWeights.Bold;
                        break;
                    }
                default:
                    break;
            }
        }

        private void txtSalesOrder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetSalesOrderInfo();
            }
        }

        private void txtBatchNo1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtBatchNo1_LostFocus(null, null);
            }
        }

        private void btnCopy2DBcr_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(bcBoxID.Text);
        }

        private void btnCancelPrint_Click(object sender, RoutedEventArgs e)
        {
            _stopPrint = true;
            Util.MessageInfo("SFU1937"); //취소되었습니다.

        }
    }


}
