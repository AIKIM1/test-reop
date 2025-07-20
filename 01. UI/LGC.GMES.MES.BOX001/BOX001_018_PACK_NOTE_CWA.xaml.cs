/*************************************************************************************
 Created Date : 2020.10.29
      Creator : 오화백
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.29  오화백 : Initial Created. (폴란드 전용 메뉴로 새로 생성함으로써 팝업도 폴란드 전용으로 뻄)





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Documents;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_018_PACK_NOTE_CWA : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        string sSHOPID = string.Empty;
        string sAREAID = string.Empty;
        string sPALLETID = string.Empty;
        string sTRAYID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_018_PACK_NOTE_CWA()
        {
            InitializeComponent();
            Loaded += BOX001_018_PACK_NOTE_CWA_Loaded;
        }

        private void BOX001_018_PACK_NOTE_CWA_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_018_PACK_NOTE_CWA_Loaded;

            if (AuthCheck())
                chkOut.Visibility = Visibility.Visible;
            else
                chkOut.Visibility = Visibility.Collapsed;

            object[] tmps = C1WindowExtension.GetParameters(this);
            txtPalletid.Text = tmps[0] as string;

           getPalletInfo(txtPalletid.Text.Trim());
        }


        #endregion


        #region Initialize
        private bool AuthCheck()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows?.Count <= 0)
                    return false;
           
                return true; 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion


        #region Event
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            TextRange textRange = new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd);

            if (chkOut.IsChecked == true && string.IsNullOrWhiteSpace(textRange.Text))
            {
                Util.AlertInfo("SFU1993"); //"특이사항을 입력하세요"
                return;
            }

            string note = textRange.Text;
            if (!string.IsNullOrWhiteSpace(textRange.Text))
            {
                int idx = note.LastIndexOf("\r\n");
                note = note.Substring(0, idx);
            }
            //	작업을 진행 하시겠습니까? 
            Util.MessageConfirm("SFU1170", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SavePackNote(txtPalletid.Text.Trim(), note);
                }
            });                   
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion


        #region Mehod

        /// <summary>
        /// Pallet 상세 조회
        /// </summary>
        /// <param name="dataItem"></param>
        private void getPalletInfo(string sPalletID)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sPalletID;
                dr["BOXTYPE"] = "PLT";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "RQSTDT", "RSLTDT", RQSTDT);

                TextRange textRange = new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd);
                textRange.Text = Util.NVC(dtResult.Rows[0]["PACK_NOTE"]);
                chkOut.IsChecked = Util.NVC(dtResult.Rows[0]["ISS_HOLD_FLAG"]) == "R" ? true : false;
                chkPalletHoldFlag.IsChecked = Util.NVC(dtResult.Rows[0]["ISS_HOLD_FLAG"]) == "Y" ? true : false;


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SavePackNote(string sPalletID, string sPackNote)
        {
            try
            {
                // 긴급 출고 Check Box 선택
                if (chkOut.IsChecked == true)
                {
                    const string bizRuleName = "BR_PRD_REG_PALLET_HOLD_NOTE";
                    DataSet inData = new DataSet();

                    //마스터 정보 
                    DataTable inTable = inData.Tables.Add("INDATA");
                    inTable.Columns.Add("BOXID", typeof(string));
                    inTable.Columns.Add("PACK_NOTE", typeof(string));
                    inTable.Columns.Add("ISS_HOLD_FLAG", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));

                    DataRow row = inTable.NewRow();

                    row = inTable.NewRow();
                    row["BOXID"] = sPalletID;
                    row["PACK_NOTE"] = sPackNote;
                    row["ISS_HOLD_FLAG"] = "R";
                    row["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(row);

                    new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, inData);

                    Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
                // 포장 Hold Check Box 선택
                else if (chkPalletHoldFlag.IsChecked == true)
                {
                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
                    inDataTable.Columns.Add("UNHOLD_CHARGE_USERID", typeof(string));
                    inDataTable.Columns.Add("HOLD_NOTE", typeof(string));
                    inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                    inDataTable.Columns.Add("SHOPID", typeof(string));
                    inDataTable.Columns.Add("PACK_HOLD_FLAG", typeof(string));

                    DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                    inHoldTable.Columns.Add("ASSY_LOTID", typeof(string));
                    inHoldTable.Columns.Add("STRT_SUBLOTID", typeof(string));
                    inHoldTable.Columns.Add("END_SUBLOTID", typeof(string));
                    inHoldTable.Columns.Add("HOLD_REG_QTY", typeof(Int32));

                    DataTable inTable = inDataSet.Tables["INDATA"];
                    DataRow newRow = inDataTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["UNHOLD_CHARGE_USERID"] = LoginInfo.USERID;
                    newRow["HOLD_NOTE"] = sPackNote;
                    newRow["HOLD_TRGT_CODE"] = "BOX";
                    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    newRow["PACK_HOLD_FLAG"] = "Y";
                    inDataTable.Rows.Add(newRow);
                    newRow = null;

                    newRow = inHoldTable.NewRow();
                    newRow["ASSY_LOTID"] = sPalletID;
                    inHoldTable.Rows.Add(newRow);

                    const string bizRuleName = "BR_PRD_REG_ASSY_HOLD_PALLET";

                    new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INHOLD", null, (result, exception) =>
                    {
                        try
                        {
                            if (exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }

                            Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            this.DialogResult = MessageBoxResult.Cancel;
                        }
                    }, inDataSet);
                }
                else if (chkPalletHoldFlag.IsChecked == false && chkOut.IsChecked == false)
                {
                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("AREAID");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("UNHOLD_NOTE");
                    inDataTable.Columns.Add("SHOPID");

                    DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                    inHoldTable.Columns.Add("BOXID");

                    DataTable inTable = inDataSet.Tables["INDATA"];
                    DataRow newRow = inDataTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["UNHOLD_NOTE"] = sPackNote;
                    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    inDataTable.Rows.Add(newRow);
                    newRow = null;

                    newRow = inHoldTable.NewRow();
                    newRow["BOXID"] = sPalletID;
                    inHoldTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ASSY_UNHOLD_PALLET", "INDATA,INHOLD", null, (result, exception) =>
                    {
                        try
                        {
                            if (exception != null)
                            {
                                Util.MessageException(exception);
                                return;
                            }

                            Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            this.DialogResult = MessageBoxResult.Cancel;
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, inDataSet);
                }
                // 긴급 출고와 포장 Hold Check Box가 동시에 체크된 경우
                else
                {
                    Util.MessageValidation("SFU4967");  //긴급 출고와 포장 Hold는 동시에 선택할 수 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #endregion

        private void chkPalletHoldFlag_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkPalletHoldFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkPalletHoldFlag.IsChecked == false)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("BOXID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["BOXID"] = txtPalletid.Text.Trim();
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SUBLOT_HOLD_BY_BOX", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["SUBLOT_HOLD_LIST"].ToString() != string.Empty)
                    {
                        //SFU3273 [%1] Hold 해제 후에 포장 Hold를 해제할 수 있습니다.
                        // Util.AlertInfo(dtResult.Rows[0]["SUBLOT_HOLD_LIST"].ToString() + " Hold 해제 후에 포장 Hold를 해제할 수 있습니다.");
                        Util.MessageInfo("SFU3273", dtResult.Rows[0]["SUBLOT_HOLD_LIST"].ToString());
                        chkPalletHoldFlag.IsChecked = true;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkOut_Checked(object sender, RoutedEventArgs e)
        {
            chkPalletHoldFlag.IsChecked = false;
            chkPalletHoldFlag.IsEnabled = false;

            //Util.MessageConfirm("SFU3121", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        chkPalletHoldFlag.IsChecked = false;
            //        chkPalletHoldFlag.IsEnabled = false;
            //    }
            //    else
            //        chkOut.IsChecked = false;
            //});             
        }

        private void chkOut_Unchecked(object sender, RoutedEventArgs e)
        {
           // chkPalletHoldFlag.IsChecked = false;
            chkPalletHoldFlag.IsEnabled = true;
        }
    }
}
