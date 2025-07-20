/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        int print_cnt = 0;
        string label_code = "LBL0030";

        bool blPrintStop;
        string strZPL = string.Empty;
        string zpl = string.Empty;
        System.ComponentModel.BackgroundWorker bkWorker;       

        int iStart;
        int iEnd;
        string sPrintZPL;
        int start_no = 0;

        public PACK001_010()
        {
            InitializeComponent();

            this.Loaded += PACK001_010_Loaded;
        }

        private void PACK001_010_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PACK001_010_Loaded;

            Initialize();

            strZPL = GetZplString();

            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);
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
           
        }
        #endregion

        #region Event
        private void wndQAMailSend_Closed(object sender, EventArgs e)
        {
            try
            {
                if (!(bool)chkPrint.IsChecked)
                {
                    return;
                }

                LGC.GMES.MES.PACK001.Report_Multi wndPopup = sender as LGC.GMES.MES.PACK001.Report_Multi;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    if (print_cnt >= Convert.ToInt32(nbPrintLastNo.Value))
                    {
                        nbPrintNo.Value = 0;
                        return;
                    }

                    nbPrintNo.Value++;

                    string temp = "# ";

                    txtPrintCnt.Text = temp + nbPrintNo.Value.ToString();

                    //print();
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void chkPrint_Click(object sender, RoutedEventArgs e)
        {
            //연속발행 checkbox 체크시에만 활성화시킴.
            if (chkPrint.IsChecked.Value)
            {
                nbPrintLastNo.IsReadOnly = false;
            }
            else
            {
                nbPrintLastNo.IsReadOnly = true;
                //nbPrintLastNo.Value = 1;
            }
        }

        private void chkPrint_Checked(object sender, RoutedEventArgs e)
        {
            if (nbPrintLastNo == null)
            {
                return;
            }

            nbPrintLastNo.IsReadOnly = false;
        }

        private void btnReSet_Click(object sender, RoutedEventArgs e)
        {
            nbPrintNo.Value = 1;
            chkPrint.IsChecked = true;
            nbPrintLastNo.Value = 1;

            txtMainTitle.Text = "Entwicklungsmuster";
            txtSubTitle.Text = "Development sample";
            txtLeft1.Text = "Max. Ladespannung";
            txtLeft2.Text = "Max. charging voltage";
            txtCenter1.Text = "14.8V";
            txtRight1.Text = "Module(B1)  Cell(B1)";
            txtRight2.Text = "Configuration 4S3P";
            txtLeft3.Text = "Max. Ladestrom";
            txtLeft4.Text = "Max. charging curren";
            txtCenter2.Text = "200A";
            txtPrintCnt.Text = "#0001";
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintProcess();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        #endregion

        #region Mehod
        private void PrintProcess()
        {
            try
            {
                if (Convert.ToBoolean(chkPrint.IsChecked))
                {
                    if (nbPrintLastNo.Value < nbPrintNo.Value)
                    {
                        ms.AlertWarning("SFU1932"); //출력 시퀀스 오류. 종료 시퀀스가 시작보다 작습니다.
                        return;
                    }

                    if (nbPrintLastNo.Value - nbPrintNo.Value > 50)
                    {
                        ms.AlertWarning("SFU2006"); //한번에 출력 가능 수는 50장 입니다.
                        nbPrintLastNo.Value = nbPrintNo.Value + 49;
                        return;
                    }
                }
                else
                {
                    nbPrintLastNo.Value = nbPrintNo.Value;
                }


                if (!bkWorker.IsBusy)
                {
                    blPrintStop = false;
                    bkWorker.RunWorkerAsync();
                    btnPrint.Content = ObjectDic.Instance.GetObjectName("취소");
                }
                else
                {
                    btnPrint.Content = ObjectDic.Instance.GetObjectName("출력");
                    blPrintStop = true;
                }

                //Clipboard.SetData("Text", sPrintZPL);

                //MessageBox.Show(sPrintZPL);
                //PrintLabel(strZPL);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);

                btnPrint.Content = ObjectDic.Instance.GetObjectName("출력");
            }
        }

        private void bkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {               
                acess_print();

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
                }));
               
            }
            catch (Exception ex)
            {
                blPrintStop = true;
                bkWorker.CancelAsync();

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    //Util.AlertInfo(ex.Message);
                    Util.MessageException(ex);
                }));
            }
        }

        private void acess_print()
        {
            try
            {
                CMM_ZPL_VIEWER2 wndPopup;

                iStart =Convert.ToInt32(Util.GetCondition_Thread(nbPrintNo));    //시작번호
                iEnd = Convert.ToInt32(Util.GetCondition_Thread(nbPrintLastNo)); //마지막번호

                string I_ATTVAL = labelItemsGet();

                getZpl(I_ATTVAL);

                string print_zpl = string.Empty;

                if (Convert.ToBoolean(Util.GetCondition_Thread(chkPrint)))
                {
                    for (int i = iStart; i <= iEnd; i++)
                    {
                        if (blPrintStop) break;

                        if(i == iStart)
                        {
                            print_zpl = zpl;
                        }
                        else
                        {
                            print_zpl = zpl.Replace(string.Format("#{0:0000}", i-1), string.Format("#{0:0000}", i));

                        }

                        this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            PrintLabel_(print_zpl);
                        }));

                        Util.SetCondition_Thread(nbPrintNo, i.ToString());
                        Util.SetCondition_Thread(txtPrintCnt, string.Format("#{0:0000}", i));

                        System.Threading.Thread.Sleep(1000);
                    }
                }
                else
                {
                    print_zpl = zpl.Replace(string.Format("#{0:0000}", start_no == 0 ? 0 : start_no), string.Format("#{0:0000}", Convert.ToInt32(Util.GetCondition_Thread(nbPrintNo))));                    
                    Util.SetCondition_Thread(txtPrintCnt, string.Format("#{0:0000}", iStart));

                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        PrintLabel_(print_zpl);
                    }));                   
                }

                start_no = Convert.ToInt32(Util.GetCondition_Thread(nbPrintNo));

                if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        wndPopup = new CMM_ZPL_VIEWER2(print_zpl);
                        wndPopup.Show();
                    }));
                }
                       
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string labelItemsGet()
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = label_code;

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

                        if(item_code == "ITEM011")
                        {
                            item_value = string.Format("#{0:0000}", Convert.ToInt32(Util.GetCondition_Thread(nbPrintNo)));

                        }
                        else
                        {
                            item_value = dtInput.Rows[0][item_code].ToString();

                        }

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

        private void getZpl(string I_ATTVAL)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("I_LBCD", typeof(string));
                //RQSTDT.Columns.Add("I_PRMK", typeof(string));
                //RQSTDT.Columns.Add("I_RESO", typeof(string));
                //RQSTDT.Columns.Add("I_PRCN", typeof(string));
                //RQSTDT.Columns.Add("I_MARH", typeof(string));
                //RQSTDT.Columns.Add("I_MARV", typeof(string));
                //RQSTDT.Columns.Add("I_ATTVAL", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["I_LBCD"] = label_code;
                //dr["I_PRMK"] = "Z";
                //dr["I_RESO"] = "203";
                //dr["I_PRCN"] = "1";
                //dr["I_MARH"] = "0";
                //dr["I_MARV"] = "0";
                //dr["I_ATTVAL"] = I_ATTVAL;

                //RQSTDT.Rows.Add(dr);

                ////ITEM001=TEST1^ITEM002=TEST2

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_DESIGN_TEST", "INDATA", "OUTDATA", RQSTDT);

                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: label_code
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

            DataRow dr = dt.NewRow();
            dr["ITEM001"] = Util.GetCondition_Thread(txtMainTitle); //Entwicklungsmuster
            dr["ITEM002"] = Util.GetCondition_Thread(txtSubTitle); //Development sample
            dr["ITEM003"] = Util.GetCondition_Thread(txtLeft1); //Max. Ladespannung
            dr["ITEM004"] = Util.GetCondition_Thread(txtLeft2); //Max. charging voltage
            dr["ITEM005"] = Util.GetCondition_Thread(txtLeft3); // Max. Ladestrom
            dr["ITEM006"] = Util.GetCondition_Thread(txtLeft4); //Max. charging current
            dr["ITEM007"] = Util.GetCondition_Thread(txtCenter1); //14.8V
            dr["ITEM008"] = Util.GetCondition_Thread(txtCenter2); //200A
            dr["ITEM009"] = Util.GetCondition_Thread(txtRight1); //Module(B1)  Cell(B1)
            dr["ITEM010"] = Util.GetCondition_Thread(txtRight2); //Configuration 4S3P
            dr["ITEM011"] = Util.GetCondition_Thread(txtPrintCnt); //"#0000"; //#0001
            dr["ITEM012"] = Util.GetCondition_Thread(txtPrintCnt);  //"#0000"; //#0001
            dt.Rows.Add(dr);

            return dt;
        }

        private void PrintLabel_(string sZpl)
        {
            try
            {
                zpl = sZpl;
                Util.PrintLabel(FrameOperation, loadingIndicator, sZpl);
            }
            catch (Exception ex)
            {
                ms.AlertWarning(ex.Message);
            }
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            blPrintStop = true;

            btnPrint.Content = ObjectDic.Instance.GetObjectName("출력");           
            
        }

        private void print()
        {
            try
            {
                DataTable dtPorche = new DataTable();
                dtPorche.TableName = "dtPorche";
                dtPorche.Columns.Add("Main_Title", typeof(string));
                dtPorche.Columns.Add("Sub_Title", typeof(string));
                dtPorche.Columns.Add("Left_1", typeof(string));
                dtPorche.Columns.Add("Left_2", typeof(string));
                dtPorche.Columns.Add("Left_3", typeof(string));
                dtPorche.Columns.Add("Left_4", typeof(string));
                dtPorche.Columns.Add("Right_1", typeof(string));
                dtPorche.Columns.Add("Right_2", typeof(string));
                dtPorche.Columns.Add("Center_1", typeof(string));
                dtPorche.Columns.Add("Center_2", typeof(string));
                dtPorche.Columns.Add("Paging_Cnt", typeof(string));

                DataRow dr = dtPorche.NewRow();
                dr["Main_Title"] = Util.GetCondition(txtMainTitle);
                dr["Sub_Title"] = Util.GetCondition(txtSubTitle);
                dr["Left_1"] = Util.GetCondition(txtLeft1);
                dr["Left_2"] = Util.GetCondition(txtLeft2);
                dr["Left_3"] = Util.GetCondition(txtLeft3);
                dr["Left_4"] = Util.GetCondition(txtLeft4);
                dr["Right_1"] = Util.GetCondition(txtRight1);
                dr["Right_2"] = Util.GetCondition(txtRight2);
                dr["Center_1"] = Util.GetCondition(txtCenter1);
                dr["Center_2"] = Util.GetCondition(txtCenter2);
                dr["Paging_Cnt"] = Util.GetCondition(txtPrintCnt);
                dtPorche.Rows.Add(dr);

                LGC.GMES.MES.PACK001.Report_Multi rs = new LGC.GMES.MES.PACK001.Report_Multi();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[2];
                    Parameters[0] = "Porche"; // "PalletHis_Tag";
                    Parameters[1] = dtPorche;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(wndQAMailSend_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal())); //rs.ShowModal();

                    print_cnt++;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetZplString()
        {

            //    try
            //    {
            //        DataSet dsResult = new DataSet();
            //        this.ExecuteService("", "GET_PORSCHE_BMABARCODE", null, "OUTDATA", new DataSet(), out dsResult);
            //        if (dsResult != null && dsResult.Tables.Contains("OUTDATA"))
            //            return dsResult.Tables["OUTDATA"].Rows[0][0].ToString();
            //        else
            //            return string.Empty;
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //        return string.Empty;
            //    }

            //string sZpl = "^XA^MCY^XZ";
            //sZpl += "^XA^LRN^FWN^CFD,24^LH0,0";
            //sZpl += "^CI0^PR2^MNY^MTT^MMT^MD^PON^PMN^XZ";
            //sZpl += "^XA";
            //sZpl += "^A0N,40,40^FO80,22^CI0^FD$TEXT01^FS";
            //sZpl += "^A0N,23,20^FO155,60^CI0^FD$TEXT02^FS";
            //sZpl += "^A0N,22,20^FO25,85^CI0^FD$TEXT03^FS";
            //sZpl += "^A0N,22,20^FO25,105^CI0^FD$TEXT04^FS";
            //sZpl += "^A0N,35,32^FO220,90^CI0^FD$TEXT05^FS";
            //sZpl += "^A0N,22,20^FO25,134^CI0^FD$TEXT06^FS";
            //sZpl += "^A0N,22,20^FO25,154^CI0^FD$TEXT07^FS";
            //sZpl += "^A0N,35,32^FO220,139^CI0^FD$TEXT08^FS";
            //sZpl += "^A0N,16,14^FO320,95^CI0^FD$TEXT09^FS";
            //sZpl += "^A0N,16,14^FO325,115^CI0^FD$TEXT10^FS";
            //sZpl += "^A0N,35,35^FO340,190^CI0^FD$SEQ01^FS";
            //sZpl += "^PQ1,0,1,Y";
            //sZpl += "^XZ";

            string sZpl = "^ XA";
            sZpl = "^ DFR:TEMP_FMT.ZPL";
            sZpl = "^ XZ";
            sZpl = "^ XA";
            sZpl = "^ XFR:TEMP_FMT.ZPL";
            sZpl = "^ A0N,40,38 ^ FO73,20 ^ FD$ITEM001 ^ FS";
            sZpl = "^ A0N,19,20 ^ FO152,61 ^ FD$ITEM002 ^ FS";
            sZpl = "^ A0N,32,32 ^ FO209,97 ^ FD$ITEM007 ^ FS";
            sZpl = "^ A0N,19,20 ^ FO24,97 ^ FD$ITEM003 ^ FS";
            sZpl = "^ A0N,19,20 ^ FO24,117 ^ FD$ITEM004 ^ FS";
            sZpl = "^ A0N,19,20 ^ FO24,152 ^ FD$ITEM005 ^ FS";
            sZpl = "^ A0N,19,20 ^ FO24,172 ^ FD$ITEM006 ^ FS";
            sZpl = "^ A0N,32,32 ^ FO335,190 ^ FD$ITEM012 ^ FS";
            sZpl = "^ A0N,32,32 ^ FO209,149 ^ FD$ITEM008 ^ FS";
            sZpl = "^ A0N,16,16 ^ FO303,105 ^ FD$ITEM009 ^ FS";
            sZpl = "^ A0N,16,16 ^ FO399,105 ^ FD$ITEM010 ^ FS";
            sZpl = "^ A0N,16,16 ^ FO309,125 ^ FD$ITEM011 ^ FS";
            sZpl = "^ PQ1,0,1,Y";
            sZpl = "^ XZ";
            sZpl = "^ XA";
            sZpl = "^ IDR:TEMP_FMT.ZPL";
            sZpl = "^ XZ";

            return sZpl;
        }
        #endregion
        
    }
}
