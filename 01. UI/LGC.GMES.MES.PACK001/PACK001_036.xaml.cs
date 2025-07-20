/*************************************************************************************
 Created Date : 2018.08.31
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.31  손우석 3779436 Print the label by GMES [요청번호] C20180830_79436
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
    public partial class PACK001_036 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtLabelCodes;
        System.ComponentModel.BackgroundWorker bkWorker;
        
        private string sComScanerMsg = string.Empty;
        private bool blPrintStop = true;
        string label_code = string.Empty; 
        string zpl = string.Empty;

        string ITEM_REF = string.Empty;
        string MSD_Plug = string.Empty;

        public PACK001_036()
        {            
            InitializeComponent();

            this.Loaded += PACK001_036_Loaded;   
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
            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;

            getLabelCode();  // COMMONCODE TABLE에서 해당 화면에서 발행할 LABEL_CODE 가져오기 (CMCDTYPE = "PACK_LABEL_CODE")
        }

        #endregion              

        #region Event
        private void PACK001_036_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_036_Loaded;
        }

        private DataRow[] selectText(DataTable dt, string item_code)
        {
            DataRow[] drs;

            drs = dt.Select("ITEM_CODE = '" + item_code + "'");
            return drs;
        }

        #region Button
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

        #region Text
        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                txtQuantityChange();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtnetweight_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                txtnetweightChange();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtGrossweight_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                txtGrossweightChange();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtratedpower_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                txtratedpowerChange();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Text

        #endregion

        #region Mehod
        private string returnString(DataTable dt, string item_code)
        {
            return selectText(dt, item_code).Length > 0 ? Util.NVC(selectText(dt, item_code)[0]["ITEM_VALUE"]) : "";
        }

        private void getLabelCode()
        {
            try
            {
                string PGMID = string.Empty;
                PGMID = "P036";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CONTRY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "PACK_LABEL_CODE";
                dr["CONTRY"] = PGMID;

                RQSTDT.Rows.Add(dr);

                dtLabelCodes = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_CODE_FIND", "INDATA", "OUTDATA", RQSTDT);

                label_code = dtLabelCodes.Rows[0]["LABEL_CODE1"].ToString();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
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
                CMM_ZPL_VIEWER2 wndPopup;
                Button btn = null;
                string labelCode = string.Empty;
                int tab_idx = tcMain.SelectedIndex;               
              
                btn = btnPrint;
                labelCode = label_code; 

                I_ATTVAL = labelItemsGet(labelCode);

                getZpl(I_ATTVAL, labelCode);

                if (LoginInfo.USERID.Trim() == "ogong")
                {
                    wndPopup = new CMM_ZPL_VIEWER2(zpl);
                    wndPopup.Show();
                }

                for (int i = 0; i < nbPrintCnt.Value; i++)
                {
                    if (blPrintStop) break;

                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                    System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);
                }

                //ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
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
                        //화면에서 입력된 값 뿌림                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtInput.Rows[0][item_code].ToString();

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

            DataRow dr = dt.NewRow();
            dr["ITEM001"] = Util.GetCondition(txtInformation); 
            dr["ITEM002"] = Util.GetCondition(txtQuantity); 
            dr["ITEM003"] = Util.GetCondition(txtnetweight);
            dr["ITEM004"] = Util.GetCondition(txtGrossweight);
            dr["ITEM005"] = Util.GetCondition(txtratedpower);
            dr["ITEM006"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");

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

                if (LoginInfo.USERID.Trim() == "ogong")
                {
                    CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(sZpl);
                    wndPopup.Show();
                }
                //wndPopup.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void txtQuantityChange()
        {
            if (!Util.CheckDecimal(txtQuantity.Text, 0))
            {
                txtQuantity.Text = "";
                return;
            }
        }

        private void txtnetweightChange()
        {
            if (!Util.CheckDecimal(txtQuantity.Text, 0))
            {
                txtnetweight.Text = "";
                return;
            }
        }

        private void txtGrossweightChange()
        {
            if (!Util.CheckDecimal(txtQuantity.Text, 0))
            {
                txtGrossweight.Text = "";
                return;
            }
        }

        private void txtratedpowerChange()
        {
            if (!Util.CheckDecimal(txtQuantity.Text, 0))
            {
                txtratedpower.Text = "";
                return;
            }
        }

        #endregion
    }
}

