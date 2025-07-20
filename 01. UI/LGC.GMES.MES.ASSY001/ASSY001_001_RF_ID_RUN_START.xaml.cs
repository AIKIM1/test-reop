/*************************************************************************************
 Created Date : 2019.02.22
      Creator : 오화백K
   Decription : CWA RF_ID 착공
--------------------------------------------------------------------------------------
 [Change History]
  2019.02.22 오화백K : Initial Created.
  
**************************************************************************************/

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
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// CMM_RF_ID_RUN_START.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_001_RF_ID_RUN_START : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ElecType = string.Empty;
        private string _MountPstsID = string.Empty;
        private string _LotID = string.Empty;
        private bool bSave = false;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

     
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_001_RF_ID_RUN_START()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null )
            {
                _LineID = Util.NVC(tmps[0]);   //라인
                _EqptID = Util.NVC(tmps[1]);   //설비
                _ElecType = Util.NVC(tmps[2]); //극성
                _MountPstsID = Util.NVC(tmps[3]); //설비위치
                _LotID = Util.NVC(tmps[4]); //LOT

                if (_LotID == string.Empty)
                    return;
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _ElecType = "";
                _MountPstsID = "";
                _LotID = "";
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    GetNewLotid();
                }
            });

           
     
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }





        #endregion

        #region Mehod
        private bool CanStartRun()
        {
            bool bRet = false;

            if (txtRF_ID.Text.Trim().Length != 10)
            {
                //RF_ID는 10자리 입니다.
                Util.MessageValidation("SFU6019");
                txtRF_ID.Text = string.Empty;
                return bRet;
            }


            if (txtRF_ID.Text.Trim().Equals(""))
            {
                //RF_ID를 입력하세요
                Util.MessageValidation("SFU6020");
                return bRet;
            }
            bRet = true;

            return bRet;
        }

        private void StartRun(string sNewLot)
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inEqp = indataSet.Tables.Add("IN_EQP");
                inEqp.Columns.Add("SRCTYPE", typeof(string));
                inEqp.Columns.Add("IFMODE", typeof(string));
                inEqp.Columns.Add("EQPTID", typeof(string));
                inEqp.Columns.Add("RFID_MODE", typeof(string));
                inEqp.Columns.Add("USERID", typeof(string));


                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));


                DataTable inOutLot = indataSet.Tables.Add("IN_OUTLOT");
                inOutLot.Columns.Add("OUT_CSTID", typeof(string));
                inOutLot.Columns.Add("OUT_LOTID", typeof(string));
                inOutLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inOutLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                DataRow newRow = inEqp.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["RFID_MODE"] = "Y";
                newRow["USERID"] = LoginInfo.USERID;
                inEqp.Rows.Add(newRow);

                DataRow newRow2 = inInput.NewRow();
                newRow2["EQPT_MOUNT_PSTN_ID"] = string.Empty;
                newRow2["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow2["INPUT_LOTID"] = _LotID;
                inInput.Rows.Add(newRow2);

                DataRow dr = inOutLot.NewRow();
                dr["OUT_CSTID"] = txtRF_ID.Text;
                dr["OUT_LOTID"] = sNewLot;
                inOutLot.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_NT_RFID", "IN_EQP,IN_INPUT,IN_OUTLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        bSave = true;

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetNewLotid()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inEqp = indataSet.Tables.Add("IN_EQP");
                inEqp.Columns.Add("SRCTYPE", typeof(string));
                inEqp.Columns.Add("IFMODE", typeof(string));
                inEqp.Columns.Add("EQPTID", typeof(string));
                inEqp.Columns.Add("USERID", typeof(string));


                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inEqp.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                inEqp.Rows.Add(newRow);

                DataRow dr = inInput.NewRow();
                dr["EQPT_MOUNT_PSTN_ID"] = _MountPstsID;
                dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                dr["INPUT_LOTID"] = _LotID;
                inInput.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_NEW_LOTID_NT_EIF", "IN_EQP,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                          string sNewLot = string.Empty;
                        if (bizResult != null && bizResult.Tables[0].Rows.Count > 0)
                        {
                            sNewLot = bizResult.Tables["OUTDATA"].Rows[0]["OUT_LOTID"].ToString();
                            if (sNewLot.Equals(""))
                                return;
                            StartRun(sNewLot);
                        }
      
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


    }
}
