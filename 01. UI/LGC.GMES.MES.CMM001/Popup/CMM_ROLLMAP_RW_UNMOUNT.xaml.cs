/*************************************************************************************
 Created Date : 2023.07.07
      Creator : 정기동
   Decription : ESWA 재와인딩 DRB 기능 확산 
--------------------------------------------------------------------------------------
 [Change History]
 2023.07.07  정기동 : Initial Created. (장착취소 기능 추가, 실적 확정 시 잔량 처리 로직 수정)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// RW 작업시작 팝업.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ROLLMAP_RW_UNMOUNT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _prodLotId = string.Empty;
        private string _prodWipseq = string.Empty;
        private bool _isInput;
        private string _ldrLotIdentBasCode = string.Empty;

        Util _Util = new Util();

        DateTime lastKeyPress = DateTime.Now;

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ROLLMAP_RW_UNMOUNT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            _processCode = parameters[0] as string;
            _equipmentSegmentCode = parameters[1] as string;
            _equipmentCode = parameters[2] as string;
            _equipmentName = parameters[3] as string;

            txtEquipment.Text = _equipmentName;

            // 장착취소 LOT 검색 
            GetCurrentMountLotInfo();
            txtLotID.IsEnabled = false;

        }

       
        /// <summary>
        /// 장착취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUnMount_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationLotID()) return;

            // SFU2344 : 장착 취소하시겠습니까?
            string sMessageID = "SFU2344";

            Util.MessageConfirm(sMessageID, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    UnMountProcess();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 장착취소 대상LOT 조회
        /// </summary>
        private void GetCurrentMountLotInfo()
        {

            DataTable mountDt = GetCurrentMount(_equipmentCode);
            if (mountDt == null || mountDt.Rows.Count == 0)
                return;

            txtEquipmentMountPstnId.Text = mountDt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();            
            txtLotID.Text = mountDt.Rows[0]["INPUT_LOTID"].ToString();
  
        }



        /// <summary>
        /// 장착취소
        /// </summary>
        private void UnMountProcess()
        {
            try
            {

                DataTable mountDt = GetCurrentMount(_equipmentCode);
                if (mountDt == null || mountDt.Rows.Count == 0)
                    return;

                // 실행 시에 한번더 체크
                if (!string.Equals(txtLotID.Text.Trim(), mountDt.Rows[0]["INPUT_LOTID"].ToString()))
                {
                    // SFU2343 :  설비에 장착된 LOT과 선택된 LOT이 다릅니다. (%1, %2)
                    Util.MessageInfo("SFU2343", mountDt.Rows[0]["INPUT_LOTID"].ToString(), txtLotID.Text.Trim());
                    return;
                }

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = _processCode;
                inTable.Rows.Add(newRow);

                DataTable inLot = inDataSet.Tables.Add("IN_INPUT");
                inLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inLot.Columns.Add("INPUT_LOTID", typeof(string));

                newRow = inLot.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = mountDt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                newRow["EQPT_MOUNT_PSTN_STATE"] = mountDt.Rows[0]["INPUT_STATE_CODE"].ToString();
                newRow["INPUT_LOTID"] = mountDt.Rows[0]["INPUT_LOTID"].ToString();
                inLot.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_LOT_CANCEL2_RW_DRB", "INDATA,IN_INPUT", null, (result, ex) =>
                {

                    try
                    {
                        HiddenLoadingIndicator();

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.MessageInfo("SFU1275");     // 정상처리되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex2)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex2);
                    }

                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        /// <summary>
        /// 설비 장착정보 조회
        /// </summary>
        private DataTable GetCurrentMount(string sEqptID)
        {
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = sEqptID;
                inTable.Rows.Add(newRow);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_CT", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }
    
        #endregion

        #region [Func]
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

        #region[Validation]
        private bool ValidationLotID()
        {
            if (string.IsNullOrWhiteSpace(txtLotID.Text))
            {
                Util.MessageValidation("SFU1938"); // 취소할 LOT을 선택하세요.
                return false;
            }

            return true;
        }


        #endregion

        #endregion


    }
}
