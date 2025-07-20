/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_201 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        public string sRePrintUser = string.Empty;
        public string sRePrintcomment = string.Empty;

        #region Declaration & Constructor 
        public PGM_GUI_201()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            Initialize();

            //btnSave.IsEnabled = false;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;



            DataTable dtRANID_ADD = new DataTable();
            DataRow newRow = null;

            dtRANID_ADD = new DataTable();
            //dtRANID_ADD.Columns.Add("선택", typeof(string));
            dtRANID_ADD.Columns.Add("LOTID", typeof(string));
            dtRANID_ADD.Columns.Add("모델", typeof(string));
            dtRANID_ADD.Columns.Add("수량", typeof(string));
            dtRANID_ADD.Columns.Add("양/음극여부", typeof(string));

            List<object[]> list_RAN = new List<object[]>();

            for (int i = 1; i < 51; i++)
            {
                list_RAN.Add(new object[] { i, "", "", "" });
            }
            //list_RAN.Add(new object[] { "", "", "", "" });
            //list_RAN.Add(new object[] { "", "", "", "" });

            foreach (object[] item in list_RAN)
            {
                newRow = dtRANID_ADD.NewRow();
                newRow.ItemArray = item;
                dtRANID_ADD.Rows.Add(newRow);
            }

            dgBOXMapping.ItemsSource = DataTableConverter.Convert(dtRANID_ADD);




        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>

        private void Reasonpopup_Closed(object sender, EventArgs e)
        {
            try
            {
                PGM_GUI_201_Reprint_Reason window = sender as PGM_GUI_201_Reprint_Reason;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    sRePrintUser = window.PRINTUSERID;
                    sRePrintcomment = window.PRINTCOMMENT;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void popup_Closed(object sender, EventArgs e)
        {

        }

        #region Button Event

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_201_WAITING_LOT";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iPrintCnt = 2;

                if (_Util.GetDataGridCheckCnt(dgBOXHist, "CHK") < 1)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 박스가 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                for (int i = 0; i > dgBOXHist.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "CHK")) == "True")
                    {
                        if (!DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "M_ROLLID").ToString().Equals(""))
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("기존에 프린트된 라벨입니다. 재프린팅하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    iPrintCnt = 1;

                                    sRePrintUser = "";
                                    sRePrintcomment = "";


                                    // *1
                                    //string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
                                    //string MAINFORMNAME = "PGM_GUI_201_Reprint_Reason";

                                    //Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                                    //Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                                    //object obj = Activator.CreateInstance(targetType);

                                    //IWorkArea workArea = obj as IWorkArea;
                                    //workArea.FrameOperation = FrameOperation;

                                    //C1Window popup = obj as C1Window;
                                    //if (popup != null)
                                    //{
                                    //    popup.Closed -= popup_Closed;
                                    //    popup.Closed += popup_Closed;
                                    //    popup.ShowModal();
                                    //    popup.CenterOnScreen();
                                    //}

                                    // *2
                                    //PGM_GUI_201_Reprint_Reason popup = new PGM_GUI_201_Reprint_Reason(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "LOTID").ToString());
                                    ////popup.ShowDialog();
                                    //popup.ShowModal();

                                    // *3
                                    PGM_GUI_201_Reprint_Reason Reasonpopup = new PGM_GUI_201_Reprint_Reason(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "LOTID").ToString());
                                    Reasonpopup.FrameOperation = this.FrameOperation;

                                    if (Reasonpopup != null)
                                    {
                                        Reasonpopup.Closed -= Reasonpopup_Closed;
                                        Reasonpopup.Closed += Reasonpopup_Closed;
                                        Reasonpopup.ShowModal();
                                        Reasonpopup.CenterOnScreen();
                                    }



                                    if (string.IsNullOrEmpty(sRePrintUser) && string.IsNullOrEmpty(sRePrintcomment))
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("재발행 사유 및 담당자를 입력하지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                        return;
                                    }

                                }
                                else
                                {
                                    return;
                                }
                            });
                        }

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("BOXID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["BOXID"] = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "BOXID").ToString();
                        RQSTDT.Rows.Add(dr);

                        new ClientProxy().ExecuteService("DA_PRD_SEL_BOX_INFO", "RQSTDT", "RSLTDT", RQSTDT, (Boxinfo, ex) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            if (ex != null)
                            {
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

                            if (Boxinfo.Rows.Count > 0)
                            {
                                string sElecrode = string.Empty;

                                for (int iCnt = 0; iCnt < Boxinfo.Rows.Count; iCnt++)
                                {
                                    sElecrode = Get_Elec(Boxinfo.Rows[iCnt]["LOTID"].ToString().Substring(0, 3));

                                    // 양극
                                    if (sElecrode.Equals("C"))
                                    {
                                        DataTable dtShipdata = Get_Ship_Data(Boxinfo.Rows[iCnt]["LOTID"].ToString(), sElecrode);
                                        DataTable dtBarcodedata = Get_Barcode_Data(Boxinfo.Rows[iCnt]["LOTID"].ToString());

                                        if (dtShipdata == null || dtBarcodedata == null)
                                        {
                                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("데이터 확인중 문제가 발생하였습니다. IT 담당자에게 문의하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                            return;
                                        }
                                        else
                                        {
                                            int iChgQty = Convert.ToInt32(dtShipdata.Rows[0]["CHGQTY_ST"].ToString());
                                            int iDefectqty1 = Convert.ToInt32(dtShipdata.Rows[0]["QTY1"].ToString());
                                            int iDefectqty2 = Convert.ToInt32(dtShipdata.Rows[0]["QTY2"].ToString());
                                            int iDefectqty3 = Convert.ToInt32(dtShipdata.Rows[0]["QTY3"].ToString());

                                            int iQty1 = iChgQty - iDefectqty1;
                                            int iQty2 = iChgQty - iDefectqty2;
                                            int iQty3 = iChgQty - iDefectqty3;

                                            int iokPicTotal = iQty1 + iQty2 + iQty3;
                                            int ingPicTotal = iDefectqty1 + iDefectqty2 + iDefectqty3;

                                            // mother roll Id 의 5자리는 Fix.
                                            string sMotherID = "LGC01" +
                                                                dtBarcodedata.Rows[0]["O_MIXING_YEAR"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_MIXING_MONTH"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_COATING_CUTNO"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_RE_YEAR"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_RE_MONTH"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_RE_MC_NO"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_RE_CUTNO"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_RE_POSITION"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_PG_YEAR"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_PG_MONTH"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_PG_DAY"].ToString() +
                                                                dtBarcodedata.Rows[0]["O_NOTHING"].ToString();

                                            string sPgdate = dtBarcodedata.Rows[0]["O_PGYEAR"].ToString() +
                                                             dtBarcodedata.Rows[0]["O_PGMONTH"].ToString() +
                                                             dtBarcodedata.Rows[0]["O_PGDAY"].ToString();

                                            for (int p = 0; p < iPrintCnt; p++)
                                            {
                                                // 양극 바코드 발행 메서드 호출.  
                                                // 최초발행시 2장프린트 구자만D요청
                                                // PrintLabelMLotC600
                                            }



                                            // Print Label 발행 이력 저장
                                            string sUserid = LoginInfo.USERID;
                                            PrintLabelHistory(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "BOXID").ToString(),
                                                              Boxinfo.Rows[iCnt]["LOTID"].ToString(),
                                                              sMotherID,
                                                              "",         //슬리터 Roll Id 가 없음.
                                                              Boxinfo.Rows[iCnt]["RANID"].ToString(),
                                                              iPrintCnt == 1 ? sRePrintUser : sUserid,
                                                              iPrintCnt == 1 ? sRePrintcomment : ""
                                                              );

                                            //출력한 바코드 ID 를 DB 에 저장하는 메서드 호출.
                                            SaveBarcodeId(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "BOXID").ToString(),
                                                          Boxinfo.Rows[iCnt]["LOTID"].ToString(),
                                                          sMotherID,
                                                          ""         //슬리터 Roll Id 가 없음.
                                                          );
                                        }
                                    }
                                    // 음극
                                    else if (sElecrode.Equals("A"))
                                    {
                                        DataTable dtShipdata = Get_Ship_Data(Boxinfo.Rows[iCnt]["LOTID"].ToString(), sElecrode);
                                        DataTable dtBarcodedata_M = Get_Barcode_Data_M(Boxinfo.Rows[iCnt]["LOTID"].ToString());
                                        DataTable dtBarcodedata_C = Get_Barcode_Data_C(Boxinfo.Rows[iCnt]["LOTID"].ToString());

                                        if (dtShipdata == null || dtBarcodedata_M == null || dtBarcodedata_C == null)
                                        {
                                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("데이터 확인중 문제가 발생하였습니다. IT 담당자에게 문의하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                            return;
                                        }
                                        else
                                        {
                                            int iChgQty = Convert.ToInt32(dtShipdata.Rows[0]["CHGQTY_ST"].ToString());
                                            int iDefectqty1 = Convert.ToInt32(dtShipdata.Rows[0]["QTY1"].ToString());
                                            int iDefectqty2 = Convert.ToInt32(dtShipdata.Rows[0]["QTY2"].ToString());
                                            int iDefectqty3 = Convert.ToInt32(dtShipdata.Rows[0]["QTY3"].ToString());

                                            int iQty1 = iChgQty;
                                            int iQty2 = iChgQty;
                                            int iQty3 = iChgQty;

                                            int iokPicTotal = iQty1 + iQty2 + iQty3;
                                            int ingPicTotal = iDefectqty1 + iDefectqty2 + iDefectqty3;

                                            // mother roll Id 의 5자리는 Fix.
                                            string sMotherID = "LGC01" +
                                                                dtBarcodedata_M.Rows[0]["O_YEAR"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_MONTH"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_COATINGCUTNO"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_SLITTING_MONTH"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_SLITTING_DAY"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_SLITTING_MC_NO"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_SLITTING_CUTNO"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_SLITTING_POSITION"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_PG_YEAR"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_PG_MONTH"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_PG_DAY"].ToString() +
                                                                dtBarcodedata_M.Rows[0]["O_PGVDNO"].ToString();

                                            // slit ID
                                            string sSlitId = "O2P" +
                                                            dtBarcodedata_C.Rows[0]["O_COATINGCUTNO"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_SLITTING_MONTH"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_SLITTING_DAY"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_SLITTING_MC_NO"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_SLITTING_CUTNO"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_SLITTING_POSITION"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_PG_YEAR"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_PG_MONTH"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_PG_DAY"].ToString() +
                                                            dtBarcodedata_C.Rows[0]["O_PG_VDNO"].ToString();

                                            // packing year/month/date
                                            string sPkgdate = dtBarcodedata_M.Rows[0]["O_PGYEAR"].ToString() +
                                                              dtBarcodedata_M.Rows[0]["O_PGMONTH"].ToString() +
                                                              dtBarcodedata_M.Rows[0]["O_PGDAY"].ToString();

                                            if (DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "M_ROLLID").ToString().Equals(""))
                                            {
                                                for (int iCnt2 = 0; i < dgBOXHist.Rows.Count; iCnt2++)
                                                {
                                                    if (DataTableConverter.GetValue(dgBOXHist.Rows[iCnt2].DataItem, "C_ROLLID").ToString().Equals(sSlitId))
                                                    {
                                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("중복 Slit Id가 생성되었습니다. IT 담당자에게 문의하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                                        return;
                                                    }
                                                }
                                            }

                                            for (int p = 0; p < iPrintCnt; p++)
                                            {
                                                // 음극 바코드 발행 메서드 호출.
                                                // 최초발행시 2장프린트 구자만D요청
                                                // PrintLabelMLotA600
                                            }



                                            // Print Label 발행 이력 저장
                                            string sUserid = LoginInfo.USERID;
                                            PrintLabelHistory(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "BOXID").ToString(),
                                                              Boxinfo.Rows[iCnt]["LOTID"].ToString(),
                                                              sMotherID,
                                                              sSlitId,
                                                              Boxinfo.Rows[iCnt]["RANID"].ToString(),
                                                              iPrintCnt == 1 ? sRePrintUser : sUserid,
                                                              iPrintCnt == 1 ? sRePrintcomment : ""
                                                              );

                                            //출력한 바코드 ID 를 DB 에 저장하는 메서드 호출.
                                            SaveBarcodeId(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "BOXID").ToString(),
                                                          Boxinfo.Rows[iCnt]["LOTID"].ToString(),
                                                          sMotherID,
                                                          sSlitId
                                                          );

                                        }

                                    }

                                    // Sleep 추가 필요?

                                }
                            }

                        });

                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private string Get_Elec(string sModelID)
        {
            try
            {
                string sElectrode = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MODELID", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["MODELID"] = sModelID;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (Electinfo, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

                    sElectrode = Electinfo.Rows[0]["ELECTRODE"].ToString();
                });

                return sElectrode;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private DataTable Get_Ship_Data(string sLotid, string sElectrode)
        {
            try
            {
                string sBizName = string.Empty;
                DataTable dtResult = new DataTable();

                if (sElectrode.Equals("C"))
                {
                    sBizName = "";
                }
                else if (sElectrode.Equals("A"))
                {
                    sBizName = "";
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);                    

                    //dsResult = DataTableConverter.Convert(result); 
                    dtResult = result;
                });

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private DataTable Get_Barcode_Data(string sLotid)
        {
            try
            {
                string sBizName = string.Empty;
                DataTable dtResult = new DataTable();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);                    

                    dtResult = result;
                });

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private void PrintLabelHistory(string boxId, string lotId, string mRollId, string cRollId, string ranId, string userid, string scomment)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(DateTime));
                RQSTDT.Columns.Add("LOTID", typeof(DateTime));
                RQSTDT.Columns.Add("M_ROLLID", typeof(string));
                RQSTDT.Columns.Add("C_ROLLID", typeof(string));
                RQSTDT.Columns.Add("RANID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("COMMENT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = boxId;
                dr["LOTID"] = lotId;
                dr["M_ROLLID"] = mRollId;
                dr["C_ROLLID"] = cRollId;
                dr["RANID"] = ranId;
                dr["USERID"] = userid;
                dr["COMMENT"] = scomment;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void SaveBarcodeId(string boxId, string lotId, string mRollId, string cRollId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(DateTime));
                RQSTDT.Columns.Add("LOTID", typeof(DateTime));
                RQSTDT.Columns.Add("M_ROLLID", typeof(string));
                RQSTDT.Columns.Add("C_ROLLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = boxId;
                dr["LOTID"] = lotId;
                dr["M_ROLLID"] = mRollId;
                dr["C_ROLLID"] = cRollId;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private DataTable Get_Barcode_Data_M(string sLotid)
        {
            try
            {
                string sBizName = string.Empty;
                DataTable dtResult = new DataTable();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);                    

                    dtResult = result;
                });

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private DataTable Get_Barcode_Data_C(string sLotid)
        {
            try
            {
                string sBizName = string.Empty;
                DataTable dtResult = new DataTable();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);                    

                    dtResult = result;
                });

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgBOXHist.Rows.Count < 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("조회된 내용이 존재하지 않습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_Util.GetDataGridCheckCnt(dgBOXHist, "CHK") < 1)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 박스가 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                for (int i = 0; i > dgBOXHist.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "CHK")) == "True")
                    {
                        // 기존 Biz : BR_D_PKG_SHIPMENT_BOX_V01

                    }
                }
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상적으로 삭제(포장 해체)되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

            fn_Search();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            fn_Search();
        }

        private void fn_Search()
        {
            try
            {
                string sStartdate = dtpDateFrom.ToString();
                string sEnddate = dtpDateTo.ToString();
                string sMoterRoll = txtMotherRollID.ToString().Trim();
                string sSlitRoll = txtSlitRoll.ToString().Trim();
                string sRanid = txtRANID.ToString().Trim();


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STARTDATE", typeof(DateTime));
                RQSTDT.Columns.Add("ENDDATE", typeof(DateTime));
                RQSTDT.Columns.Add("TYPE", typeof(string));
                RQSTDT.Columns.Add("M_ROLLID", typeof(string));
                RQSTDT.Columns.Add("C_ROLLID", typeof(string));
                RQSTDT.Columns.Add("RANID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STARTDATE"] = sStartdate;
                dr["ENDDATE"] = sEnddate;
                dr["TYPE"] = "N";
                dr["M_ROLLID"] = sMoterRoll;
                dr["C_ROLLID"] = sSlitRoll;
                dr["RANID"] = sRanid;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_BOXLOT_INFO_NISSAN", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        //private void btnSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        string sRanid = string.Empty;

        //        if (_Util.GetDataGridCheckCnt(dgBOXHist, "CHK") < 0)
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Ran ID '저장'하시기 전에 하나 이상의 박스를 선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        if (string.IsNullOrEmpty(txtRANID_Save.ToString()))
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Ran ID를 입력하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        sRanid = txtRANID_Save.ToString().Trim();

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("RAN_ID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["RAN_ID"] = sRanid;
        //        RQSTDT.Rows.Add(dr);

        //        new ClientProxy().ExecuteService("DA_PRD_SEL_RANID_VALID", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
        //        {
        //            loadingIndicator.Visibility = Visibility.Collapsed;
        //            if (ex != null)
        //            {
        //                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                return;
        //            }
        //            //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

        //            if (result.Rows[0]["LOTID"].ToString() != null)
        //            {
        //                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("중복된 RANID 입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                return;
        //            }
        //        });

        //        for (int iCnt = 0; iCnt < dgBOXHist.Rows.Count; iCnt++)
        //        {
        //            String sLotid = string.Empty;

        //            DataTable RQSTDT1 = new DataTable();
        //            RQSTDT1.TableName = "RQSTDT";
        //            RQSTDT1.Columns.Add("LOT_ID", typeof(string));

        //            DataRow dr1 = RQSTDT1.NewRow();
        //            dr1["LOT_ID"] = Util.NVC(DataTableConverter.GetValue(dgBOXHist.Rows[iCnt].DataItem, "LOT_ID").ToString());

        //            new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_ELECTRODE", "RQSTDT", "RSLTDT", RQSTDT1, (result, ex) =>
        //            {
        //                loadingIndicator.Visibility = Visibility.Collapsed;
        //                if (ex != null)
        //                {
        //                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    return;
        //                }

        //                if (result.Rows[0]["ELECTRODE"].ToString() == "C")
        //                {
        //                    sRanid = fn_Get_RanID("CATHODE");
        //                }
        //                else
        //                {
        //                    sRanid = fn_Get_RanID("ANODE");
        //                }
        //            });
        //        }

        //        if (sRanid == "" || sRanid == null)
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("극성이 맞지 않습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        // Biz 추가 필요...


        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //    }

        //}

        //private string fn_Get_RanID (string Electro)
        //{
        //    try
        //    { 
        //        String sRanid = string.Empty;

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("ELECTRO", typeof(string));
        //        RQSTDT.Columns.Add("RANID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["ELECTRO"] = Electro;
        //        dr["RANID"] = txtRANID_Save.ToString().Trim();

        //        new ClientProxy().ExecuteService("DA_PRD_SEL_RANID", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
        //        {
        //            loadingIndicator.Visibility = Visibility.Collapsed;
        //            if (ex != null)
        //            {
        //                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                return;
        //            }

        //            sRanid = result.Rows[0]["LOTID"].ToString();

        //        });

        //        return sRanid;
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return null;
        //    }
        //}

        private void btnDelete_Box_Click(object sender, RoutedEventArgs e)
        {
            //C1.WPF.DataGrid.DataGridCell cell = dgBOXMapping.GetCellFromFrameworkElement(sender as Button);
            //int idx = _Util.GetDataGridCheckFirstRowIndex(dgBOXMapping, "CHK");
            //dgBOXMapping.RemoveRow(idx);

            for (int i = 0; i < dgBOXMapping.Rows.Count; i++)
            {
                if ((dgBOXMapping.GetCell(i, dgBOXMapping.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                    (bool)(dgBOXMapping.GetCell(i, dgBOXMapping.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                {
                    dgBOXMapping.RemoveRow(i);
                }
            }
        }

        private void btnMapping_Box_Click(object sender, RoutedEventArgs e)
        {
            string sBox_ID = string.Empty;
            string sRan_ID = string.Empty;
            string sElectrode = string.Empty;

            // Box ID 채번
            //=> QR_GETLOTID 기존 BIZ

            // RAN ID 가져오기
            if (DataTableConverter.GetValue(dgBOXMapping.Rows[0].DataItem, "ELECTRODE").ToString().Equals("A"))
            {
                sRan_ID = GET_RANID("ANODE");
            }
            else
            {
                sRan_ID = GET_RANID("CATHODE");
            }
        }

        private string GET_RANID(string Electro)
        {
            try
            {
                string sRAN_ID = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("DEL_FLAG", typeof(string));
                RQSTDT.Columns.Add("ELECTRO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["DEL_FLAG"] = "A";
                dr["ELECTRO"] = Electro;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

                    sRAN_ID = result.Rows[0]["LOTID"].ToString();

                });

                return sRAN_ID;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }



        #endregion

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (dgBOXMapping.Rows.Count > 0)
                {
                    if (DataTableConverter.GetValue(dgBOXMapping.Rows[0].DataItem, "ELECTRODE").ToString().Equals("A"))
                    {
                        if (dgBOXMapping.Rows.Count > 2)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("박스구성 가능한 팬케익은 3 EA입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    else
                    {
                        if (dgBOXMapping.Rows.Count > 0)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("박스구성 가능한 점보롤은 1 EA입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                if (chkPass.IsChecked.Value)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("패스 상태를 유지 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            fn_Lotid_Chk();
                        }
                    });
                }

            }
        }

        private void fn_Lotid_Chk()
        {
            try
            {
                string sLotid = string.Empty;
                sLotid = txtLOTID.ToString().Trim();

                // 중복 스캔 체크
                for (int i = 0; i < dgBOXMapping.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgBOXMapping.Rows[0].DataItem, "LOTID").ToString().Equals(sLotid))
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("중복 스캔하셨습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

                    if (result.Rows.Count < 1)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("포장창고에 존재 하지 않는 점보롤ID 입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows[0]["BOXCHK"].ToString().Equals("N"))
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("이미 박스구성된 팬케익/점보롤입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dgBOXMapping.Rows.Count == 1)
                    {
                        // 모델 체크
                        // 위의 Biz 에서 모델 정보도 가져올수 있는지 확인 필요

                        // 극성 체크
                        if (!DataTableConverter.GetValue(dgBOXMapping.Rows[0].DataItem, "ELECTRODE").ToString().Equals(result.Rows[0]["ELECTRODE"].ToString()))
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("극이 다릅니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (dgBOXMapping.CurrentRow.DataItem == null)
                return;

            _Util.SetDataGridUncheck(dgBOXMapping, "CHK", ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index);
        }
    }
}