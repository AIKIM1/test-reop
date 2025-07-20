/*************************************************************************************
 Created Date : 2016.10.15
      Creator : 
   Decription : Coater 작업시작
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_008_LOTSTART : C1Window, IWorkArea
    {
        #region Initialize

        private static string sEqptID = string.Empty;
        private static string sElectrode = string.Empty;
        private static string sWOID = string.Empty;
        private string sEqsgID = string.Empty;

        DataSet inDataSet = null;
        DataSet _MaterialDataSet = null;
        //DataSet _SlurryDataSet = null;

        Util _Util = new Util();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_008_LOTSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }

            sEqptID = tmps[0].ToString();
            sEqsgID = tmps[1].ToString();

            //dtpDate.SelectedDateTime = System.DateTime.Now;
            //TimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (!GetWorkorder())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }

            SetElectrode();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidData())
                return;

            LotStart();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private bool GetWorkorder()
        {
            bool rslt = false;
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("EQPTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["EQPTID"] = sEqptID;
            IndataTable.Rows.Add(Indata);
            //설비별 workorder 
            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

            if (dtMain.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1906");  //조회된 WorkOrder가 없습니다.
                rslt = false;
            }
            else
            {
                txtWorkOrder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();
                rslt = true;
            }

            return rslt;
        }

        private void SetElectrode()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO", "INDATA", "RSLTDT", IndataTable);

                sElectrode = dtMain.Rows[0]["ELTR_TYPE_CODE"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidData()
        {
            if (txtWorkOrder.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1443");  //작업지시를 선택하세요.
                return false;
            }

            if (txtCoreA.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1335"); //Foil ID를 입력해주세요.
                txtCoreA.Focus();
                return false;
            }

            if (txtCoreB.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1335"); //Foil ID를 입력해주세요.
                txtCoreB.Focus();
                return false;
            }

            if (txtCoreA.Text.Trim() == txtCoreB.Text.Trim())
            {
                Util.MessageValidation("SFU1334");  //Foil ID 가 서로 같습니다.
                return false;
            }

            if (txtSlurry.Text.Trim() == "")
            {
                Util.MessageValidation("SFU1425");  //Slurry ID를 입력 해 주세요.
                txtSlurry.Focus();
                return false;
            }
#if false
            if (txtSlurry.Text.Trim().Substring(1, 1) != sElectrode)
            {
                Util.MessageValidation("SFU1426");  //Slurry ID의 양/음극 이 장비와 같지 않습니다.
                txtSlurry.Focus();
                return false;
            }

            if (!CheckSlurryID(txtSlurry))
                return false;
#endif
            return true;
        }

        private bool CheckSlurryID(TextBox tBox)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("BATCHID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["BATCHID"] = tBox.Text;
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_LOT", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1910"); //존재하지 않는 Slurry(Batch) 입니다.
                    tBox.Focus();
                    return false;
                }
                else
                {
                    string sResult = dtMain.Rows[0].ToString();

                    if (sResult == "OK")
                    {
                        return true;
                    }
                    else
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sResult), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void LotStart()
        {
            inDataSet = new DataSet();
            _MaterialDataSet = new DataSet();

            #region Lot Info
            DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");
            inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
            inLotDataTable.Columns.Add("IFMODE", typeof(string));
            inLotDataTable.Columns.Add("LANGID", typeof(string));
            inLotDataTable.Columns.Add("EQPTID", typeof(string));
            inLotDataTable.Columns.Add("USERID", typeof(string));
            inLotDataTable.Columns.Add("FIRST_FLAG", typeof(string));

            DataRow inLotDataRow = null;

            inLotDataRow = inLotDataTable.NewRow();
            inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inLotDataRow["LANGID"] = LoginInfo.LANGID;
            inLotDataRow["EQPTID"] = sEqptID;
            inLotDataRow["USERID"] = LoginInfo.USERID;
            inLotDataRow["FIRST_FLAG"] = "Y"; //대LOT 발번
            inLotDataTable.Rows.Add(inLotDataRow);
            #endregion

            #region Material
            DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
            InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));

            DataRow inMtrlDataRow = null;
            inMtrlDataRow = InMtrldataTable.NewRow();
            inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = "MTRL_MOUNT_PSTN01";
            inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
            inMtrlDataRow["INPUT_LOTID"] = txtCoreA.Text;

            inMtrlDataRow = InMtrldataTable.NewRow();
            inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = "MTRL_MOUNT_PSTN01";
            inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
            inMtrlDataRow["INPUT_LOTID"] = txtCoreB.Text;

            inMtrlDataRow = InMtrldataTable.NewRow();
            inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = "MTRL_MOUNT_PSTN01";
            inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
            inMtrlDataRow["INPUT_LOTID"] = txtSlurry.Text;

            InMtrldataTable.Rows.Add(inMtrlDataRow);
            #endregion

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_START_LOT_CT_SINGLE", "INDATA,IN_INPUT", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.AlertByBiz("BR_PRD_REG_CREATE_START_LOT_CT_SINGLE", ex.Message, ex.ToString());
                    return;
                }

                Util.AlertInfo("SFU1275"); //정상처리되었습니다.

            }, inDataSet);
        }
        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_ELEC_SLURRY _Slurry = new CMM001.Popup.CMM_ELEC_SLURRY();  // CMM으로 이동
            _Slurry.FrameOperation = FrameOperation;

            if (_Slurry != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Process.COATING;
                Parameters[1] = sEqsgID;

                // Parameter에 EQPTID 추가 ( 2017-01-06 ) => TOP/BACK 동시적용
                Parameters[2] = string.Empty;
                Parameters[3] = sEqptID;

                C1WindowExtension.SetParameters(_Slurry, Parameters);

                _Slurry.Closed += new EventHandler(Slurry_Closed);
                _Slurry.ShowModal();
                _Slurry.CenterOnScreen();
            }
        }

        private void Slurry_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_ELEC_SLURRY _Slurry = sender as CMM001.Popup.CMM_ELEC_SLURRY;

            if (_Slurry.DialogResult == MessageBoxResult.OK)
                txtSlurry.Text = _Slurry._ReturnLotID;
        }
    }
}