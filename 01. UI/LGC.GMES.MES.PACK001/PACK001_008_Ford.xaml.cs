/*************************************************************************************
 Created Date : 2019.03.27
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.03.27  손우석 CSR ID 3949723 Ford 48v 라벨 관련 UI 개발 및 기준정보 설정 요청 건 [요청번호] C20190315_49723
  2019.05.21  손우석 SM   라벨코드 표시 순서 변경
  2019.05.24  손우석 SM   화면 설정 및 기준정보 표시 변경
  2020.06.24  손우석 서비스번호 73520 [생산PI팀]Box 라벨 프린터 화면 개선 [요청번호] C20200620-000040
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_008_Ford : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtResult;
        DataTable dtModelResult;       
        DataTable dtTextResult;
        DataTable dtLabelCodes;
        System.ComponentModel.BackgroundWorker bkWorker;

        private string sComScanerMsg = string.Empty;
        private bool blPrintStop = true;
        string label_code = string.Empty; 
        string zpl = string.Empty;

        string strLabel = string.Empty;
        string strVolInfo = string.Empty;
        string strMade = string.Empty;
        string strLOTID = string.Empty;

        string ITEM_REF = string.Empty;
        string MSD_Plug = string.Empty;

        //2019.05.24
        string sPallet = string.Empty;
        string sLot = string.Empty;

        public PACK001_008_Ford()
        {            
            InitializeComponent();

            this.Loaded += PACK001_008_Ford_Loaded;   
        }
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
           cboLabel.Visibility = Visibility.Visible;

            dtpDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now ;
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;         

            txtDate.Text = PrintOutDate(DateTime.Now);

            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            getLabelCode();  // COMMONCODE TABLE에서 해당 화면에서 발행할 LABEL_CODE 가져오기 (CMCDTYPE = "PACK_LABEL_CODE")
            setCombo_PROD(); // LABEL CODE로 제품 정보 가져오기
            setCombo_Out();  // 제품정보로 출하처 정보 가져오기
            setCombo_Label();

            dtpDate.SelectedDataTimeChanged += dtpDate_SelectedDateChanged;            
        }
        #endregion Initialize

        #region Event
        private void PACK001_008_Ford_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_008_Ford_Loaded;
        }

        private void setTextBox()
        {
            if (dtTextResult == null || dtTextResult.Rows.Count == 0)
            {
                return;
            }

            DataTable dt = dtTextResult;

            txtSupplierID.Text = returnString(dt, "ITEM001");
            bcSupplierID.Text = "V" + returnString(dt, "ITEM004");

            strLabel = returnString(dt, "ITEM003");

            txtLGCID.Text = Convert_LabelItems(returnString(dt, "ITEM002").Replace("@PRODID", cboProduct.SelectedValue.ToString()));

            txtquantity.Text = Convert_LabelItems(returnString(dt, "ITEM005").Replace("@PALLETID", sPallet));
            bcquantity.Text = "Q" + Convert_LabelItems(returnString(dt, "ITEM006").Replace("@PALLETID", sPallet));

            txtContainer.Text = returnString(dt, "ITEM007");

            txtGrossWeight.Text = returnString(dt, "ITEM008") + "Kg";

            txtDate.Text = returnString(dt, "ITEM009");

            strVolInfo = returnString(dt, "ITEM010");

            //2019.05.24
            //txtpartNumber.Text = Convert_LabelItems(returnString(dt, "ITEM012").Replace("@LOTID", strLOTID));
            //bcpartNumber.Text = "P" + Convert_LabelItems(returnString(dt, "ITEM013").Replace("@LOTID", strLOTID));
            txtpartNumber.Text = Convert_LabelItems(returnString(dt, "ITEM012").Replace("@PRODID", cboProduct.SelectedValue.ToString()));
            bcpartNumber.Text = "P" + Convert_LabelItems(returnString(dt, "ITEM013").Replace("@PRODID", cboProduct.SelectedValue.ToString()));

            txtStrLoc.Text = returnString(dt, "ITEM014");

            txtLineFeedLoc.Text = returnString(dt, "ITEM015");

            //2019.05.24
            //txtProdid.Text = Convert_LabelItems(returnString(dt, "ITEM016").Replace("@LOTID", strLOTID));
            txtProdid.Text = cboProduct.SelectedValue.ToString();
            txtProdname.Text = Convert_LabelItems(returnString(dt, "ITEM017").Replace("@PRODID", cboProduct.SelectedValue.ToString()));

            txtSerial.Text = Convert_LabelItems(returnString(dt, "ITEM018").Replace("@PRODID", cboProduct.SelectedValue.ToString()));
            bcSerial.Text = "S" + Convert_LabelItems(returnString(dt, "ITEM020").Replace("@PRODID", cboProduct.SelectedValue.ToString()));

            strMade = returnString(dt, "ITEM019");

            txtTO.Text = returnString(dt, "ITEM021");

            txtCustID.Text = returnString(dt, "ITEM022");

            txtDock.Text = returnString(dt, "ITEM023");

            //2019.05.24
            //bcLGCID.Text = Convert_LabelItems(returnString(dt, "ITEM011").Replace("@LOTID", strLOTID));
            //Part Number, Quantity, Supplier Code, Container Type, Gross Weight, ASN Number, Date(YYMMDD 형식), Serial Number, Customer Code, Dock code, Storage Location.
            bcLGCID.Text = txtpartNumber.Text + txtquantity.Text + txtGrossWeight.Text + txtLineFeedLoc.Text + txtCustID.Text + txtDock.Text;
        }

        private string returnString(DataTable dt, string item_code)
        {
            return selectText(dt, item_code).Length > 0 ? Util.NVC(selectText(dt, item_code)[0]["ITEM_VALUE"]) : "";
        }

        #region Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getValueSetting();

                dtpDate_SelectedDateChanged(null, null);

                Get_Product_Lot();
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
                if (btnPrint.Content.ToString() == ObjectDic.Instance.GetObjectName("취 소"))
                {
                    bkWorker.Dispose();
                    blPrintStop = true;
                    btnPrint.Content = ObjectDic.Instance.GetObjectName("출 력");
                    return;
                }

                PrintProcess(btnPrint);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        #endregion Button

        #region Dataset / DataTable / DataRow
        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                strLOTID = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "LOTID").ToString();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dtpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //2019.05.24
                string sYear = string.Empty;
                string sMonth = string.Empty;
                string sDay = string.Empty;

                if (dtpDate == null || dtpDate.SelectedDateTime == null)
                {
                    return;
                }

                if (cboProduct == null)
                {
                    return;
                }

                //2019.05.24
                sYear = dtpDate.SelectedDateTime.ToString("yyyy");

                switch (dtpDate.SelectedDateTime.ToString("MM"))
                {
                    case "01":
                        sMonth = "JAN";
                        break;

                    case "02":
                        sMonth = "FEB";
                        break;

                    case "03":
                        sMonth = "MAR";
                        break;

                    case "04":
                        sMonth = "APR";
                        break;

                    case "05":
                        sMonth = "MAY";
                        break;

                    case "06":
                        sMonth = "JUN";
                        break;

                    case "07":
                        sMonth = "JUL";
                        break;

                    case "08":
                        sMonth = "AUG";
                        break;

                    case "09":
                        sMonth = "SEP";
                        break;

                    case "10":
                        sMonth = "OCT";
                        break;

                    case "11":
                        sMonth = "NOV";
                        break;

                    case "12":
                        sMonth = "DEC";
                        break;
                }                        

                sDay = dtpDate.SelectedDateTime.ToString("dd");

                //txtDate.Text = "D" + dtpDate.SelectedDateTime.ToString("yyMMdd");
                txtDate.Text = sDay + sMonth + sYear;
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private DataRow[] selectText(DataTable dt, string item_code)
        {
            DataRow[] drs;

            drs = dt.Select("ITEM_CODE = '" + item_code + "'");
            return drs;
        }

        #endregion Dataset / DataTable / DataRow

        #region Text

        private void txtpartNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtquantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSupplierID_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSerial_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtLGCID_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        #endregion Text

        #region Combo
        private void cboProduct_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            setCombo_Out();      
        }

        private void cboLabel_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtSeletedLabel.Text = cboLabel.SelectedValue.ToString();
            label_code = cboLabel.SelectedValue.ToString();
        }

        private void cboProduct_Out_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            setCombo_Label();
        }
        #endregion Combo

        #endregion Event

        #region Mehod
        private void getLabelCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CONTRY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "PACK_LABEL_CODE";
                dr["CONTRY"] = "P08F";

                RQSTDT.Rows.Add(dr);

                dtLabelCodes = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_CODE_FIND", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void setCombo_PROD()
        {
            try
            {
                string label_codes = string.Empty;

                if (dtLabelCodes != null && dtLabelCodes.Rows.Count > 0)
                {
                    label_codes = dtLabelCodes.Rows[0]["LABEL_CODE1"].ToString();

                    if (dtLabelCodes.Rows[0]["LABEL_CODE2"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE2"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE3"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE3"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE4"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE4"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE5"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE5"].ToString();
                    }
                }
                else
                {
                    label_codes = "LBL0179";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LABEL_CODE"] = label_codes;

                RQSTDT.Rows.Add(dr);

                dtModelResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROJECTNAME_PRODID_COMBO", "INDATA", "OUTDATA", RQSTDT);

                cboProduct.DisplayMemberPath = "CBO_NAME";
                cboProduct.SelectedValuePath = "CBO_CODE";
                cboProduct.ItemsSource = DataTableConverter.Convert(dtModelResult);

                txtSeletedLabel.Text = label_codes;

                cboProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setCombo_Out()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LABEL_CODE"] = Util.GetCondition(txtSeletedLabel);
                dr["PRODID"] = Util.GetCondition(cboProduct);

                RQSTDT.Rows.Add(dr);

                DataTable dtShipto = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LABEL_SHIPTO", "INDATA", "OUTDATA", RQSTDT);

                cboProduct_Out.DisplayMemberPath = "CBO_NAME";
                cboProduct_Out.SelectedValuePath = "CBO_CODE";
                cboProduct_Out.ItemsSource = DataTableConverter.Convert(dtShipto);

                cboProduct_Out.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setCombo_Label()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["SHIPTO"] = Util.GetCondition(cboProduct_Out);

                RQSTDT.Rows.Add(dr);

                DataTable dtShipto = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_SHIPTO", "INDATA", "OUTDATA", RQSTDT);

                cboLabel.DisplayMemberPath = "CBO_NAME";
                cboLabel.SelectedValuePath = "CBO_CODE";
                cboLabel.ItemsSource = DataTableConverter.Convert(dtShipto);

                cboLabel.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Get_Product_Lot()
        {
            DataTable dtResult;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();                                 
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom); 
                dr["TODATE"] = Util.GetCondition(dtpDateTo);
                dr["LOTID"] = null;
                dr["PALLETID"] = null;

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_COUNT_VOLVOBMA", "INDATA", "OUTDATA", RQSTDT);

                dgResult.ItemsSource = null;
                tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgResult, dtResult, FrameOperation);

                Util.SetTextBlockText_DataGridRowCount(tbPalletHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getValueSetting()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LABEL", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["LABEL"] = Util.GetCondition(txtSeletedLabel);
                dr["SHIPTO_ID"] = Util.GetCondition(cboProduct_Out);
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                dtTextResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtTextResult == null || dtTextResult.Rows.Count == 0)
                {
                    //ms.AlertWarning("추가"); //MMD 기준정보 누락 : 출하처 인쇄항목 관리
                    return;
                }
                else
                {
                    setTextBox();

                    dtpDate_SelectedDateChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getZpl(string I_ATTVAL, string LabelCode)
        {
            try
            {
                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: LabelCode
                                                     , sATTVAL: I_ATTVAL
                                                     );

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl = dtResult.Rows[0]["ZPLSTRING"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string Convert_LabelItems(string strQuery)
        {
            string strRetrun = string.Empty;

            try
            {
                //BR_PRD_SEL_DYNAMIC_SQL
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STRQUERY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STRQUERY"] = strQuery;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_DYNAMIC_SQL", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    strRetrun = dtResult.Rows[0]["RESULT"].ToString();
                }

                return strRetrun;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string labelItemsGet(string labelCode)
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;
            string I_ATTVAL_MSD = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = labelCode;

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : 코드=값^코드=값

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        #region sample value 뿌림
                        /*
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();
                        */
                        #endregion

                        #region 화면에서 입력된 값 뿌림                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtInput.Rows[0][item_code].ToString();
                        #endregion

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }
                    }
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        #endregion Method

        private string PrintOutDate(DateTime dt)
        {
            System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
            return dt.ToString("dd") + dt.ToString("MMMM", format).Substring(0, 3).ToUpper() + dt.ToString("yyyy");
        }

        private void PrintProcess(Button btn)
        {
            if (!bkWorker.IsBusy)
            {
                blPrintStop = false;
                bkWorker.RunWorkerAsync();
                
                btn.Content = ObjectDic.Instance.GetObjectName("취소");
                btn.Foreground = Brushes.White;
            }
            else
            {
                blPrintStop = true;                
                btn.Content = ObjectDic.Instance.GetObjectName("출력");
                btn.Foreground = Brushes.Red;
            }
        }

        private void bkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    PrintAcess();
                }));
            }
            catch (Exception ex)
            {
                bkWorker.CancelAsync();
                blPrintStop = true;

                Util.AlertInfo(ex.Message);
            }
        }

        private void PrintAcess()
        {
            try
            {
                string I_ATTVAL = string.Empty;
                //2020.06.24
                //CMM_ZPL_VIEWER2 wndPopup;
                Button btn = null;
                string labelCode = string.Empty;
                int tab_idx = tcMain.SelectedIndex;               
              
                btn = btnPrint;
                tbPrint2_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : 0 " + ObjectDic.Instance.GetObjectName("건") + "]";
                labelCode = label_code; 

                I_ATTVAL = labelItemsGet(labelCode);

                getZpl(I_ATTVAL, labelCode);

                for (int i = 0; i < nbPrintCnt.Value; i++)
                {
                    if (blPrintStop) break;

                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                    tbPrint2_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : " + (i + 1).ToString() + ObjectDic.Instance.GetObjectName("건") + "]";
                    System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);
                }

                if ((LoginInfo.USERID.Trim() == "ogong") || (LoginInfo.USERID.Trim() == "cnszhftm15") || (LoginInfo.USERID.Trim() == "everystreet"))
                {
                    //2020.06.24
                    //wndPopup = new CMM_ZPL_VIEWER2(zpl);
                    //wndPopup.Show();
                    Preview popup = new Preview(labelCode, zpl);
                    popup.Show();
                }

                //ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.Alert(ex.Message);
            }
        }

        private DataTable getInputData()
        {
            DataTable dt = new DataTable();
          
            dt.TableName = "INPUTDATA";
            dt.Columns.Add("ITEM001", typeof(string));
            dt.Columns.Add("ITEM002", typeof(string));
            dt.Columns.Add("ITEM003", typeof(string));
            dt.Columns.Add("ITEM004", typeof(string));
            dt.Columns.Add("ITEM005", typeof(string));
            dt.Columns.Add("ITEM006", typeof(string));
            dt.Columns.Add("ITEM007", typeof(string));
            dt.Columns.Add("ITEM008", typeof(string));
            dt.Columns.Add("ITEM009", typeof(string));
            dt.Columns.Add("ITEM010", typeof(string));
            dt.Columns.Add("ITEM011", typeof(string));
            dt.Columns.Add("ITEM012", typeof(string));
            dt.Columns.Add("ITEM013", typeof(string));
            dt.Columns.Add("ITEM014", typeof(string));
            dt.Columns.Add("ITEM015", typeof(string));
            dt.Columns.Add("ITEM016", typeof(string));
            dt.Columns.Add("ITEM017", typeof(string));
            dt.Columns.Add("ITEM018", typeof(string));
            dt.Columns.Add("ITEM019", typeof(string));
            dt.Columns.Add("ITEM020", typeof(string));
            dt.Columns.Add("ITEM021", typeof(string));
            dt.Columns.Add("ITEM022", typeof(string));
            dt.Columns.Add("ITEM023", typeof(string));

            DataRow dr = dt.NewRow();

            dr["ITEM001"] = Util.GetCondition(txtSupplierID);
            dr["ITEM002"] = Util.GetCondition(txtLGCID);
            dr["ITEM003"] = strLabel;
            dr["ITEM004"] = bcSupplierID.Text;
            dr["ITEM005"] = Util.GetCondition(txtquantity);
            dr["ITEM006"] = bcquantity.Text;
            dr["ITEM007"] = Util.GetCondition(txtContainer);
            dr["ITEM008"] = Util.GetCondition(txtGrossWeight);
            dr["ITEM009"] = Util.GetCondition(txtDate);
            dr["ITEM010"] = strVolInfo;
            dr["ITEM011"] = bcLGCID.Text;
            dr["ITEM012"] = Util.GetCondition(txtpartNumber);
            dr["ITEM013"] = bcpartNumber.Text;
            dr["ITEM014"] = Util.GetCondition(txtStrLoc);
            dr["ITEM015"] = Util.GetCondition(txtLineFeedLoc);
            dr["ITEM016"] = Util.GetCondition(txtProdid);
            dr["ITEM017"] = Util.GetCondition(txtProdname);
            dr["ITEM018"] = Util.GetCondition(txtSerial);
            dr["ITEM019"] = strMade;
            dr["ITEM020"] = bcSerial.Text;
            dr["ITEM021"] = Util.GetCondition(txtTO);
            dr["ITEM022"] = Util.GetCondition(txtCustID);
            dr["ITEM023"] = Util.GetCondition(txtDock);

            dt.Rows.Add(dr);

            return dt;            
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Button btn = new Button();
            btn = btnPrint;

            btn.Content = ObjectDic.Instance.GetObjectName("출력");
            blPrintStop = true;
            btn.Foreground = Brushes.White;         
        }

        private void PrintBoxLabel(string sZpl)
        {
            try
            {
                Util.PrintLabel(FrameOperation, loadingIndicator, sZpl);
                System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);

                if ((LoginInfo.USERID.Trim() == "ogong") || (LoginInfo.USERID.Trim() == "cnszhftm15") || (LoginInfo.USERID.Trim() == "everystreet"))
                {
                    //CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(sZpl);
                    //wndPopup.Show();
                    Preview popup = new Preview();
                    popup.Show();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setBacode(object sender)
        {
            try
            {
                TextBox tbBox = (TextBox)sender;

                switch (tbBox.Name)
                {
                    case "txtpartNumber":
                        bcpartNumber.Text = tbBox.Text;
                        break;

                    case "txtquantity":
                        bcquantity.Text = tbBox.Text;
                        break;

                    case "txtSupplierID":
                        bcSupplierID.Text = tbBox.Text;
                        break;

                    case "txtSerial":
                        bcSerial.Text = tbBox.Text;
                        break;

                    case "txtLGCID":
                        bcLGCID.Text = tbBox.Text;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

                Util.AlertInfo(ex.Message);
            }
        }

        //2019.05.24
        private void dgResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

                if (cell != null)

                {
                    sPallet = Util.NVC(DataTableConverter.GetValue(dgResult.Rows[cell.Row.Index].DataItem, "PALLETID"));
                    sLot = Util.NVC(DataTableConverter.GetValue(dgResult.Rows[cell.Row.Index].DataItem, "LOTID"));

                    setTextBox();

                    dtpDate_SelectedDateChanged(null, null);

                    txtContainer.Text = sPallet;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
    }
}

