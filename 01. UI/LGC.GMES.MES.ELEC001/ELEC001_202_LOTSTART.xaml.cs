/*************************************************************************************
 Created Date : 2023.02.15
      Creator : 정재홍
   Decription : 재와인딩공정 작업시작 [E20230207-000434]
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.15  정재홍 : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;


namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_202_LOTSTART
    /// </summary>
    public partial class ELEC001_202_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _sPROCID = string.Empty;
        private string _sEQSGID = string.Empty;
        private string _sEQPTID = string.Empty;
        private string _sEQPTNM = string.Empty;
        private string _sLOTID = string.Empty;
        private string _sLane = string.Empty;
        private string _sLaneQty = string.Empty;

        public DataTable dtLot = null;
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_202_LOTSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            _sPROCID = parameters[0] as string;
            _sEQSGID = parameters[1] as string;
            _sEQPTID = parameters[2] as string;
            _sEQPTNM = parameters[3] as string;
            _sLOTID = parameters[4] as string;
            _sLane = parameters[5] as string;
            _sLaneQty = parameters[6] as string;

            txtEquipment.Text = "[" + _sEQPTID + "]" + _sEQPTNM;
            txtLotID.Text = _sLOTID;
        }

        /// <summary>
        /// 작업시작
        /// </summary>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationLotStart()) return;

            // 작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LotStart();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void LotStart()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("LANE_QTY", typeof(string));
                inData.Columns.Add("LANE_PTN_QTY", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = "OFF";
                row["EQPTID"] = _sEQPTID;
                row["PROCID"] = _sPROCID;
                row["USERID"] = LoginInfo.USERID;
                row["LANE_QTY"] = _sLane;
                row["LANE_PTN_QTY"] = _sLaneQty;
                inData.Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("INPUTQTY", typeof(decimal));
                inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                inLot.Columns.Add("RESNQTY", typeof(decimal));

                row = inLot.NewRow();
                row["LOTID"] = _sLOTID;
                row["INPUTQTY"] = Util.NVC_Decimal(dtLot.Rows[0]["INPUTQTY"].ToString());
                row["OUTPUTQTY"] = Util.NVC_Decimal(dtLot.Rows[0]["GOODQTY"].ToString());
                row["RESNQTY"] = Util.NVC_Decimal(dtLot.Rows[0]["LOSSQTY"].ToString());
                inLot.Rows.Add(row);

                // 투입자재 장착위치 가져오기
                DataTable dt = GetEqptMountPstn();

                if (dt == null || dt.Rows.Count == 0) return;

                DataTable InInput = indataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                InInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                InInput.Columns.Add("INPUT_LOTID", typeof(string));

                row = InInput.NewRow();
                row["EQPT_MOUNT_PSTN_ID"] = dt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                row["INPUT_LOTID"] = _sLOTID;
                InInput.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_RW_ONLINE", "IN_EQP,IN_LOT,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");     // 정상처리되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /*
        private void LotStart()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable InEqp = inDataSet.Tables.Add("IN_EQP");
                InEqp.Columns.Add("SRCTYPE", typeof(string));
                InEqp.Columns.Add("IFMODE", typeof(string));
                InEqp.Columns.Add("EQPTID", typeof(string));
                InEqp.Columns.Add("USERID", typeof(string));

                DataRow row = InEqp.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _sEQPTID;
                row["USERID"] = LoginInfo.USERID;
                InEqp.Rows.Add(row);

                // 투입자재 장착위치 가져오기
                DataTable dt = GetEqptMountPstn();

                if (dt == null || dt.Rows.Count == 0) return;

                DataTable InInput = inDataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                InInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                InInput.Columns.Add("INPUT_LOTID", typeof(string));

                row = InInput.NewRow();
                row["EQPT_MOUNT_PSTN_ID"] = dt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                row["INPUT_LOTID"] = txtLotID.Text;
                InInput.Rows.Add(row);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_RW_DRB", "IN_EQP,IN_INPUT", "OUT_LOT", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");     // 정상처리되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        */

        private DataTable GetEqptMountPstn()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow tmprow = dt.NewRow();
                tmprow["EQPTID"] = _sEQPTID;
                dt.Rows.Add(tmprow);

                DataTable dtEqptMount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", dt);

                if (!CommonVerify.HasTableRow(dtEqptMount))
                {
                    Util.MessageValidation("SFU2019");  //해당 설비의 자재투입부를 MMD에서 입력해주세요.
                    return null;
                }

                return dtEqptMount;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private bool ValidationLotStart()
        {
            if (string.IsNullOrWhiteSpace(txtLotID.Text))
            {
                Util.MessageValidation("SFU8275", ObjectDic.Instance.GetObjectName("LOT ID")); // %1 (을)를 입력해 주세요.
                return false;
            }
            
            return true;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        
    }
}
