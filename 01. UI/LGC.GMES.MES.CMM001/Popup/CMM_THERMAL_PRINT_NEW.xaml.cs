using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Threading;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_THERMAL_PRINT_NEW.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_THERMAL_PRINT_NEW : C1Window, IWorkArea
    {
        private int iCnt = 1;

        private List<Dictionary<string, string>> _dicList;
        private string _ProcID = string.Empty;
        private string _EqsgID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ShowMsg = string.Empty;
        private string _Dispatch = string.Empty;

        BizDataSet _Biz = new BizDataSet();

        Dictionary<string, string> dicParam;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_THERMAL_PRINT_NEW()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                PrintDialog dialog = new PrintDialog();

                this.Width = 242 + 2;
                this.Height = 227 + 30;


                //dialog.PrintTicket.PageMediaSize = new PageMediaSize(250, 240);
                dialog.PrintVisual(grFoldPrint, "GMES PRINT");

            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
                //Util.MessageException(ex);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog dialog = new PrintDialog();

                this.Width = 242 + 2;
                this.Height = 257 + 30;

                grLamiPrint.Margin = new Thickness(0);

                //dialog.PrintTicket.PageMediaSize = new PageMediaSize(250, 270);

                //dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                //for (int i = 0; i < iCnt; i++)
                //{
                dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                //}
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
                //Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 6)
                {
                    _dicList = tmps[0] as List<Dictionary<string, string>>;
                    _ProcID = Util.NVC(tmps[1]);
                    _EqsgID = Util.NVC(tmps[2]);
                    _EqptID = Util.NVC(tmps[3]);
                    _ShowMsg = Util.NVC(tmps[4]);
                    _Dispatch = Util.NVC(tmps[5]);
                }
                else
                {
                    _dicList = null;
                    _ProcID = "";
                    _EqsgID = "";
                    _EqptID = "";
                    _ShowMsg = "Y";
                    _Dispatch = "Y"; // 기본 디스패치 처리.
                }

                this.Hide();

                PrintProcess();

                #region "...."
                //grdEtc.Visibility = Visibility.Collapsed;

                //PrintDialog dialog = new PrintDialog();

                //if (_dicList != null && _dicList.Count > 0)
                //{
                //    if (_ProcID.Equals(Process.STACKING_FOLDING))
                //    {
                //        grLamiPrint.Visibility = Visibility.Collapsed;

                //        for (int x = 0; x < _dicList.Count; x++)
                //        {
                //            ClearParameters();

                //            SetParameters(_dicList[x]);

                //            this.Measure(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight));
                //            this.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight)));
                //            this.UpdateLayout();

                //            System.Threading.Thread.Sleep(400);

                //            //dialog.PrintTicket.CopyCount = iCnt < 1 ? 1 : iCnt;
                //            //dialog.PrintVisual(grFoldPrint, "GMES PRINT");

                //            for (int i = 0; i < iCnt; i++)
                //            {
                //                dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                //            }

                //            SetLabelPrtHist("", null, Util.NVC(dicParam["B_LOTID"]), Util.NVC(dicParam["B_WIPSEQ"]));

                //            if (_Dispatch.Equals("Y"))
                //                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));
                //        }

                //        //this.UpdateLayout();

                //        //Dispatcher.BeginInvoke(new Action(() =>
                //        //{
                //        //    for (int x = 0; x < _dicList.Count; x++)
                //        //    {
                //        //        ClearParameters();

                //        //        SetParameters(_dicList[x]);

                //        //        for (int i = 0; i < iCnt; i++)
                //        //        {
                //        //            dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                //        //        }
                //        //    }

                //        //    this.DialogResult = MessageBoxResult.OK;

                //        //}), DispatcherPriority.ContextIdle, null);
                //    }
                //    else
                //    {
                //        grFoldPrint.Visibility = Visibility.Collapsed;

                //        for (int x = 0; x < _dicList.Count; x++)
                //        {
                //            ClearParameters();

                //            SetParameters(_dicList[x]);

                //            this.Measure(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight));
                //            this.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight)));
                //            this.UpdateLayout();

                //            System.Threading.Thread.Sleep(200);

                //            for (int i = 0; i < iCnt; i++)
                //            {
                //                dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                //            }

                //            SetLabelPrtHist("", null, Util.NVC(dicParam["B_LOTID"]), Util.NVC(dicParam["B_WIPSEQ"]));

                //            if (_Dispatch.Equals("Y"))
                //                SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));
                //        }
                //    }

                //    //인쇄 완료 되었습니다.
                //    if (_ShowMsg.Equals("Y"))
                //        Util.MessageInfo("SFU1236");
                //}
                #endregion

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1419");      //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
            }
        }

        private void PrintProcess()
        {
            try
            {
                grdEtc.Visibility = Visibility.Collapsed;

                if (_ProcID.Equals(Process.STACKING_FOLDING))
                {
                    grLamiPrint.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grFoldPrint.Visibility = Visibility.Collapsed;
                }

                PrintDialog dialog = new PrintDialog();

                if (_dicList != null && _dicList.Count > 0)
                {
                    for (int x = 0; x < _dicList.Count; x++)
                    {
                        ClearParameters();

                        SetParameters(_dicList[x]);

                        if (_ProcID.Equals(Process.STACKING_FOLDING))
                        {
                            this.Measure(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight));
                            this.Arrange(new Rect(new Size(grFoldPrint.ActualWidth, grFoldPrint.ActualHeight)));
                            this.UpdateLayout();

                            //System.Threading.Thread.Sleep(400);

                            //dialog.PrintTicket.CopyCount = iCnt < 1 ? 1 : iCnt;
                            //dialog.PrintVisual(grFoldPrint, "GMES PRINT");

                            Dispatcher.Invoke(new Action(() =>
                            {
                                for (int i = 0; i < iCnt; i++)
                                {
                                    dialog.PrintVisual(grFoldPrint, "GMES PRINT");
                                }

                                SetLabelPrtHist("", null, Util.NVC(dicParam["B_LOTID"]), Util.NVC(dicParam["B_WIPSEQ"]));

                                if (_Dispatch.Equals("Y"))
                                    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                                ////인쇄 완료 되었습니다.
                                //if (_ShowMsg.Equals("Y"))
                                //    Util.MessageInfo("SFU1236");

                            }), DispatcherPriority.ContextIdle, null);                            
                        }
                        else
                        {
                            this.Measure(new Size(grLamiPrint.ActualWidth, grLamiPrint.ActualHeight));
                            this.Arrange(new Rect(new Size(grLamiPrint.ActualWidth, grLamiPrint.ActualHeight)));
                            this.UpdateLayout();

                            //System.Threading.Thread.Sleep(200);

                            Dispatcher.Invoke(new Action(() =>
                            {
                                for (int i = 0; i < iCnt; i++)
                                {
                                    dialog.PrintVisual(grLamiPrint, "GMES PRINT");
                                }

                                SetLabelPrtHist("", null, Util.NVC(dicParam["B_LOTID"]), Util.NVC(dicParam["B_WIPSEQ"]));

                                if (_Dispatch.Equals("Y"))
                                    SetDispatch(Util.NVC(dicParam["B_LOTID"]), Convert.ToDecimal(Util.NVC(dicParam["QTY"])));

                                ////인쇄 완료 되었습니다.
                                //if (_ShowMsg.Equals("Y"))
                                //    Util.MessageInfo("SFU1236");

                            }), DispatcherPriority.ContextIdle, null);                            
                        }
                    }

                    //인쇄 완료 되었습니다.
                    if (_ShowMsg.Equals("Y"))
                        Util.MessageInfo("SFU1236");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetParameters(Dictionary<string, string> dic)
        {
            try
            {
                dicParam = dic;

                if (dicParam == null) return;

                if (dicParam["reportName"].Equals("Fold"))
                {
                    if (dicParam.ContainsKey("LOTID")) LOTID_FOLD.Text = dicParam["LOTID"];
                    if (dicParam.ContainsKey("QTY")) QTY_FOLD.Text = dicParam["QTY"];
                    if (dicParam.ContainsKey("MAGID")) MAGID_FOLD.Text = dicParam["MAGID"];
                    if (dicParam.ContainsKey("MAGID")) BARCODE_FOLD.Text = "*" + dicParam["MAGID"] + "*";
                    if (dicParam.ContainsKey("LARGELOT")) LARGELOT_FOLD.Text = dicParam["LARGELOT"];
                    if (dicParam.ContainsKey("MODEL")) MODEL_FOLD.Text = dicParam["MODEL"];
                    if (dicParam.ContainsKey("REGDATE")) REGDATE_FOLD.Text = dicParam["REGDATE"];
                    if (dicParam.ContainsKey("EQPTNO")) EQPTNO_FOLD.Text = dicParam["EQPTNO"];
                    if (dicParam.ContainsKey("TITLEX")) TITLEX_FOLD.Text = dicParam["TITLEX"].Equals("BASKET ID") ? "BASKET\r\nID" : dicParam["TITLEX"];
                    grFoldPrint.Margin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    if (dicParam.ContainsKey("LOTID")) LOTID_LAMI.Text = dicParam["LOTID"];
                    if (dicParam.ContainsKey("QTY")) QTY_LAMI.Text = dicParam["QTY"];
                    if (dicParam.ContainsKey("MAGID")) MAGID_LAMI.Text = dicParam["MAGID"];
                    if (dicParam.ContainsKey("MAGID")) BARCODE_LAMI.Text = "*" + dicParam["MAGID"] + "*";
                    if (dicParam.ContainsKey("CASTID")) CASTID_LAMI.Text = dicParam["CASTID"];
                    if (dicParam.ContainsKey("MODEL")) MODEL_LAMI.Text = dicParam["MODEL"];
                    if (dicParam.ContainsKey("REGDATE")) REGDATE_LAMI.Text = dicParam["REGDATE"];
                    if (dicParam.ContainsKey("EQPTNO")) EQPTNO_LAMI.Text = dicParam["EQPTNO"];
                    if (dicParam.ContainsKey("CELLTYPE")) CELLTYPE_LAMI.Text = dicParam["CELLTYPE"];
                    if (dicParam.ContainsKey("TITLEX")) TITLEX_LAMI.Text = dicParam["TITLEX"].Equals("MAGAZINE ID") ? "MAGAZINE\r\nID" : dicParam["TITLEX"];
                    grLamiPrint.Margin = new Thickness(0, 0, 0, 0);
                }

                if (dicParam.ContainsKey("PRINTQTY"))
                    iCnt = Util.NVC(dicParam["PRINTQTY"]).Equals("") ? 1 : Convert.ToInt32(Util.NVC(dicParam["PRINTQTY"]));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearParameters()
        {
            try
            {
                // Folding
                LOTID_FOLD.Text = "";
                QTY_FOLD.Text = "";
                MAGID_FOLD.Text = "";
                BARCODE_FOLD.Text = "";
                LARGELOT_FOLD.Text = "";
                MODEL_FOLD.Text = "";
                REGDATE_FOLD.Text = "";
                EQPTNO_FOLD.Text = "";
                TITLEX_FOLD.Text = "";

                // Lami
                LOTID_LAMI.Text = "";
                QTY_LAMI.Text = "";
                MAGID_LAMI.Text = "";
                BARCODE_LAMI.Text = "";
                CASTID_LAMI.Text = "";
                MODEL_LAMI.Text = "";
                REGDATE_LAMI.Text = "";
                EQPTNO_LAMI.Text = "";
                CELLTYPE_LAMI.Text = "";
                TITLEX_LAMI.Text = "";

                iCnt = 1;

                dicParam = null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq)
        {
            try
            {
                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                //newRow["LABEL_CODE"] = "LBL0001";
                //newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = "2";
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                //newRow["PRT_ITEM03"] = "";
                //newRow["PRT_ITEM04"] = "";
                //newRow["PRT_ITEM05"] = "";
                //newRow["PRT_ITEM06"] = "";
                //newRow["PRT_ITEM07"] = "";
                //newRow["PRT_ITEM08"] = "";
                //newRow["PRT_ITEM09"] = "";
                //newRow["PRT_ITEM10"] = "";
                //newRow["PRT_ITEM11"] = "";
                //newRow["PRT_ITEM12"] = "";
                //newRow["PRT_ITEM13"] = "";
                //newRow["PRT_ITEM14"] = "";
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.MessageException(ex);
            }
        }

        private void SetDispatch(string sBoxID, decimal dQty)
        {
            try
            {

                DataSet indataSet = _Biz.GetBR_PRD_REG_DISPATCH_LOT_FD();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["EQSGID"] = _EqsgID;
                newRow["REWORK"] = "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inDataTable = indataSet.Tables["INLOT"];

                newRow = inDataTable.NewRow();
                newRow["LOTID"] = sBoxID;
                newRow["ACTQTY"] = dQty;
                newRow["ACTUQTY"] = 0;
                newRow["WIPNOTE"] = "";

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
