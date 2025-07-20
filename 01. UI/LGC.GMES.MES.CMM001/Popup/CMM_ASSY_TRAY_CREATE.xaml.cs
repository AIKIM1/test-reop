/*************************************************************************************
 Created Date : 2017.07.06
      Creator : 오화백
   Decription : 초소형 Tray 생성
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.06  오화백 : Initial Created.





 
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
    public partial class CMM_ASSY_TRAY_CREATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string sEQPTID = string.Empty; //공정진척중인 설비
        private string sPRODLOTID = string.Empty; //공정진척중인 선택 LOT
        private string sEQPTLOTID = string.Empty; //설비LOT :공백(BIZ 파라미터에서 존재하고 추후 어떻게 될지 모름) 
        
        CommonCombo combo = new CommonCombo();

        public CMM_ASSY_TRAY_CREATE()
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


                ////임시
                //sEQPTID = "N1AASB501";
                //sPRODLOTID = "AD86I10004W1";
                //sEQPTLOTID = null;
               
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
                        SaveTrayID();
                      
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
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

                string sBizNAme = "BR_PRD_REG_START_OUT_LOT_WSS";
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
                inDataSet.Tables["IN_EQP"].Rows.Add(drow);

                new ClientProxy().ExecuteService_Multi(sBizNAme, "IN_EQP,IN_INPUT", "RSLTDT", (Result, ex) =>
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
            if (txtTrayID.Text.Trim().Length != 10)
            {
                Util.MessageValidation("SFU3675"); //TrayID는 10자리 입니다.
                return false;
            }

            Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]{1,10}$");
            Boolean ismatch = regex.IsMatch(txtTrayID.Text);
            if (!ismatch)
            {
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
            
            DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
            inInputLot.Columns.Add("MTRLID", typeof(string));
         
            return indataSet;
        }




        #endregion


    }
}
