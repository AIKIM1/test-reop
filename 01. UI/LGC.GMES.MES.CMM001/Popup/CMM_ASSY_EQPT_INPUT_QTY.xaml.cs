/*************************************************************************************
 Created Date : 2017.07.07
      Creator : 오화백
   Decription : Tray 생성
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.07  오화백 : Initial Created.
  2022.02.07  성민식 : C20230109-000394 설비 투입 수량 변경 시 특정 개수 이상일 시 알림 메세지 추가




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_EQPT_INPUT_QTY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string sPRODLOTID = string.Empty; //공정진척중인 선택 LOT
        private string sEQPTENDQTY = string.Empty; //설비투입수량
        private string sEQSGID = string.Empty; //라인ID

        private int check_QTY = 0; //투입 수량 수정 알람 기준


        CommonCombo combo = new CommonCombo();

        public CMM_ASSY_EQPT_INPUT_QTY()
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
                sPRODLOTID = Util.NVC(tmps[0]);
                sEQPTENDQTY = Util.NVC(tmps[1]);
                sEQSGID = Util.NVC(tmps[2]);


                //테스트
                //sPRODLOTID = "GD8QG052L1W1";
                //sEQPTENDQTY = "300";

                //투입수량 받아서 테스트박스에 입력
                //if (sEQPTENDQTY == "0")
                //{
                //    txtInpuQty.Text = string.Empty;
                //}
                //else
                //{
                //    txtInpuQty.Text = sEQPTENDQTY;
                //}

                txtInpuQty.Text = string.Empty;
            }

            initCombo();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                check_QTY = InputQTY_Check();

                if (!Validation())
                {
                    return;
                }

                if (check_QTY > 0)
                {
                    if (Math.Abs(Convert.ToDecimal(sEQPTENDQTY) - Convert.ToDecimal(txtInpuQty.Text)) >= check_QTY)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU0683", Convert.ToString(check_QTY)), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SaveTrayID();
                            }
                        });
                    }
                    else
                    {
                        //저장하시겠습니까?
                        Util.MessageConfirm("SFU1241", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SaveTrayID();
                            }
                        });
                    }
                }
                else
                {
                    //저장하시겠습니까?
                    Util.MessageConfirm("SFU1241", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SaveTrayID();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }

        }
        private void txtInpuQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtInpuQty.Text, 0))
                {
                    return;
                }

               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

                //string sBizNAme = "BR_PRD_REG_EQTP_END_QTY_WN";
                const string bizRuleName = "BR_PRD_REG_EQTP_INPUT_QTY_WN";

                DataSet inDataSet = GetBR_PRD_REG_EQTP_END_QTY_WN();
                DataTable inDataTable = inDataSet.Tables["IN_DATA"];
                DataRow drow = inDataTable.NewRow();

                drow["PROD_LOTID"] = sPRODLOTID;
                drow["EQPT_INPUT_QTY"] = txtInpuQty.Text;
                drow["USERID"] = LoginInfo.USERID;
           
                inDataSet.Tables["IN_DATA"].Rows.Add(drow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_DATA", null, (Result, ex) =>
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
            if (txtInpuQty.Text.Trim().Length == 0)
            {
                Util.MessageValidation("설비투입 수량을 입력하세요."); //설비투입 수량을 입력하세요..
                return false;
            }

            if (Convert.ToInt16(txtInpuQty.Text) == 0)
            {
                Util.MessageValidation("설비투입 수량은 0 이상이어야 합니다."); //설비투입 수량은 0 이상이어야 합니다.
                return false;
            }
            return true;
        }

        private int InputQTY_Check()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_USE";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("CMCDTYPE", typeof(string));
            inDataTable.Columns.Add("CMCODE", typeof(string));
            inDataTable.Columns.Add("CMCDIUSE", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "EQPT_INPUT_QTY_CHK_WND";
            dr["CMCODE"] = sEQSGID;
            dr["CMCDIUSE"] = "Y";
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (CommonVerify.HasTableRow(searchResult) &&
                !string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["ATTRIBUTE1"].GetString())))
            {
                return Convert.ToInt32(Util.NVC(searchResult.Rows[0]["ATTRIBUTE1"].GetString()));
            }

            return 0;
        }

        public DataSet GetBR_PRD_REG_EQTP_END_QTY_WN()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("EQPT_INPUT_QTY", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            return indataSet;
        }

     


            #endregion


        }
}
