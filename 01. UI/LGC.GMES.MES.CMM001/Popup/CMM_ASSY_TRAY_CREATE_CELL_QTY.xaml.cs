/*************************************************************************************
 Created Date : 2017.07.07
      Creator : 오화백
   Decription : Tray 생성
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.07  오화백 : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_TRAY_CREATE_CELL_QTY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string sEQPTID = string.Empty; //공정진척중인 설비
        private string sPRODLOTID = string.Empty;  //공정진척중인 선택 LOT
        private string sEQPTLOTID = string.Empty;  //설비LOT(공백)
        private string sCELLCHEKC = string.Empty;  //CELL 관리 호출여부 (Y : CELL 관리 호출, N : 생성호출)
        private string sOUTLOTID = string.Empty;   //OUT_LOT정보 (생성 :공백  CELL관리: OUT_LOT 정보)  
        private string sCellQty = string.Empty;    //CELL 관리호출시 CellQty 정보 가져옴 
        private string sCSTID = string.Empty;      //CELL 관리호출시 TrayID 정보 가져옴
        private string sTrayTag = string.Empty;    //상태체크 (U:수정 R: 읽기전용)
        CommonCombo combo = new CommonCombo();

        public CMM_ASSY_TRAY_CREATE_CELL_QTY()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                sEQPTID = Util.NVC(tmps[0]);
                sPRODLOTID = Util.NVC(tmps[1]);
                sEQPTLOTID = Util.NVC(tmps[2]);
                sCELLCHEKC = Util.NVC(tmps[3]);//CELL 관리 호출여부 (Y : CELL 관리 호출, N : 생성호출)
                sOUTLOTID = Util.NVC(tmps[4]); //OUT_LOT정보 (생성 :공백  CELL관리: OUT_LOT 정보)  
                sCellQty = Util.NVC(tmps[5]);//CELL 관리 호출여부 (Y : CELL 관리 호출, N : 생성호출)
                sCSTID = Util.NVC(tmps[6]); //OUT_LOT정보 (생성 :공백  CELL관리: OUT_LOT 정보)  
                sTrayTag = Util.NVC(tmps[7]);
                if (sCELLCHEKC.Equals("Y"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("TRAY 수정");
                    this.txtTrayID.Text = sCSTID.ToString();
                    this.txtTrayID.IsEnabled = false;
                    this.txtCellQty.Text = sCellQty;

                }
                if (sTrayTag.Equals("R"))
                {
                    this.Header = ObjectDic.Instance.GetObjectName("Tray 조회");
                    btnSave.Visibility = Visibility.Collapsed;
                }
              

                //임시
                //sEQPTID = "N1AWSH521";
                //sPRODLOTID = "GD8QG052L1W1";
                //sEQPTLOTID = null;
                //sCELLCHEKC = "N";
                //sOUTLOTID = "12345";
                //sCellQty = "400";
                //sCSTID = "MFEA120340";

               
            }

            initCombo();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        if (sCELLCHEKC == "Y")
                        {
                            //Cell관리 화면에서 호출
                            SaveCell();
                        }
                        else
                        {
                            SaveTrayID();
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void txtCellQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtCellQty.Text, 0))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void txtTrayID_KeyUp(object sender, KeyEventArgs e)
        {
            //try
            //{
            //    if (txtTrayID.Text != string.Empty)
            //    {
            //        Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]{1,10}$");
            //        Boolean ismatch = regex.IsMatch(txtTrayID.Text);
            //        if (!ismatch)
            //        {
            //            txtTrayID.Text = string.Empty;
            //            txtTrayID.Focus();
            //            Util.MessageValidation("숫자와 영문대문자만 입력가능합니다(글자제한10).");
            //            return;
            //        }
            //    }


            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }
        #endregion

        #region Mehod
        private void initCombo()
        {

        }

        private void SaveTrayID()
        {
            try
            {
                //if (loadingIndicator != null)
                //    loadingIndicator.Visibility = Visibility.Visible;

                string sBizNAme = "BR_PRD_REG_START_OUT_LOT_WS";
                DataSet inDataSet = GetBR_PRD_REG_START_OUT_LOT_WSS();
                DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                DataRow drow = inDataTable.NewRow();
                drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drow["IFMODE"] = IFMODE.IFMODE_OFF;
                drow["EQPTID"] = sEQPTID;
                drow["USERID"] = LoginInfo.USERID;
                drow["PROD_LOTID"] = sPRODLOTID;
                drow["EQPT_LOTID"] = sEQPTLOTID;
                drow["CSTID"] = txtTrayID.Text.Trim();
                drow["OUTPUT_QTY"] = txtCellQty.Text.Trim();
                inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP,IN_CST,IN_INPUT", "RSLTDT", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;

                }, inDataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        private void SaveCell()
        {
            try
            {
                //if (loadingIndicator != null)
                //    loadingIndicator.Visibility = Visibility.Visible;
                string sBizNAme = "BR_PRD_REG_PUT_SUBLOT_IN_CST_WS";
                DataSet inDataSet = GetBR_PRD_REG_PUT_SUBLOT_IN_CST_WS();
                DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                DataRow drow = inDataTable.NewRow();
                drow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drow["IFMODE"] = IFMODE.IFMODE_OFF;
                drow["EQPTID"] = sEQPTID;
                drow["USERID"] = LoginInfo.USERID;
                drow["PROD_LOTID"] = sPRODLOTID;
                drow["EQPT_LOTID"] = sEQPTLOTID;
                drow["OUT_LOTID"] = sOUTLOTID;
                drow["CSTID"] = txtTrayID.Text.Trim();
                drow["OUTPUT_QTY"] = txtCellQty.Text.Trim();
                inDataSet.Tables["IN_EQP"].Rows.Add(drow);
                new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;

                }, inDataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }
        private bool Validation()
        {
            if (txtTrayID.Text.Trim().Length == 0)
            {
                Util.MessageValidation("SFU1626"); //생성할 Tray ID를 입력하세요.
                return false;
            }

            if (txtTrayID.Text.Trim().Length != 10)
            {
                Util.MessageValidation("SFU3675"); //TrayID는 10자리 입니다.
                return false;
            }

            if (txtCellQty.Text.Trim().Length == 0)
            {
                Util.MessageValidation("SFU3685"); //Cell 수량을 입력하세요.
                return false;
            }
          
            if (Convert.ToInt16(txtCellQty.Text) == 0)
            {
                Util.MessageValidation("SFU3677"); //Cell수량은 0 이상이어야 합니다.
                return false;
            }

            Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]{1,10}$");
            Boolean ismatch = regex.IsMatch(txtTrayID.Text);
            if (!ismatch)
            {
                txtTrayID.Focus();
                Util.MessageValidation("SFU3674"); // 숫자와 영문대문자만 입력가능합니다.
                return false;
            }
           
            return true;
        }

        public DataSet GetBR_PRD_REG_START_OUT_LOT_WSS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

            DataTable incst = indataSet.Tables.Add("IN_CST");
            incst.Columns.Add("CSTSLOT", typeof(string));
            incst.Columns.Add("CSTSLOT_F", typeof(string));
            incst.Columns.Add("SUBLOTID", typeof(string));


            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
            inInputLot.Columns.Add("MTRLID", typeof(string));

            return indataSet;
        }

        public DataSet GetBR_PRD_REG_PUT_SUBLOT_IN_CST_WS()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
            inDataTable.Columns.Add("OUT_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));
            return indataSet;
        }


        #endregion

       
    }
}
