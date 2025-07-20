/*************************************************************************************
 Created Date : 2020.11.12
      Creator : 
   Decription : Mixing 작업시작
--------------------------------------------------------------------------------------
 [Change History]
 2023.07.14 김태우 NFF DAM_MIXING 추가
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


namespace LGC.GMES.MES.ELEC003
{
    /// <summary>
    /// ELEC003_LOTSTART_MIXING
    /// </summary>
    public partial class ELEC003_LOTSTART_MIXING : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public string _ProcID = string.Empty;
        public string _EqptID = string.Empty;
        public string _Elec = string.Empty;
        public string _UserID = string.Empty;
        public string _BizRule = string.Empty;
        private string sWO_DETL_ID = string.Empty;

        Util _Util = new Util();
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

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        public ELEC003_LOTSTART_MIXING()
        {
            InitializeComponent();
            InitializeControls();
            ApplyPermissions();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitializeControls()
        {
            dtpStartDateTime.DateTime = DateTime.Now;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //object[] tmps = C1WindowExtension.GetParameters(this);

            //if (tmps == null)
            //{
            //    this.DialogResult = MessageBoxResult.Cancel;
            //    return;
            //}
            //_ProcID = Util.NVC(tmps[0]);
            //_EqptID = Util.NVC(tmps[1]);
            //_Elec = Util.NVC(tmps[2]);

            InitializeControls();


            if (!GetWorkorder())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }

            if (!GetNewBatchID())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }
        }

        public ELEC003_LOTSTART_MIXING(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("PROCID")) _ProcID = dicParam["PROCID"];
                if (dicParam.ContainsKey("EQPTID")) _EqptID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("ELEC"))     _Elec = dicParam["ELEC"];

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LotStart();
                    //this.DialogResult = MessageBoxResult.OK;
                }
            });
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private bool GetWorkorder()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _EqptID;
                IndataTable.Rows.Add(Indata);
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1436");  //W/O 선택 후 작업시작하세요
                    rslt = false;
                }
                else
                {
                    txtWorkOrder.Text = Util.NVC(dtMain.Rows[0]["WOID"]);
                    sWO_DETL_ID = Util.NVC(dtMain.Rows[0]["WO_DETL_ID"]);
                    rslt = true;
                }
                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            //new ClientProxy().ExecuteService("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable, (dtMain, searchException) =>
            //{
            //    try
            //    {
            //        if (searchException != null)
            //        {
            //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //            return;
            //        }
            //        txtWorkOrder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();
            //        return;
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //        return;
            //    }
            //}
            //);
        }
        private bool GetNewBatchID()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SRCTYPE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("CALDATE_YMD", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                Indata["EQPTID"] = _EqptID;
                Indata["CALDATE_YMD"] = dtpStartDateTime.DateTime.Value.ToString("yyyyMMdd");
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                if (_ProcID.Equals(Process.MIXING))
                {
                    _BizRule = "BR_PRD_GET_NEW_LOTID_MX";
                }
                else if (_ProcID.Equals(Process.SRS_MIXING))
                {
                    _BizRule = "BR_PRD_GET_NEW_LOTID_SX";
                }
                else if (_ProcID.Equals(Process.PRE_MIXING))
                {
                    _BizRule = "BR_PRD_GET_NEW_LOTID_PM";
                }
                else if (_ProcID.Equals(Process.BS))
                {
                    _BizRule = "BR_PRD_GET_NEW_LOTID_BS";
                }
                else if (_ProcID.Equals(Process.CMC))
                {
                    _BizRule = "BR_PRD_GET_NEW_LOTID_CMC";
                }
                else if (_ProcID.Equals(Process.InsulationMixing))
                {
                    _BizRule = "BR_PRD_GET_NEW_LOTID_INSULT_MX";
                }
                else if (_ProcID.Equals(Process.DAM_MIXING))
                {
                    _BizRule = "BR_PRD_GET_NEW_LOTID_DAM_MX";
                }

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(_BizRule, "IN_EQP", "OUT_LOT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1311");  //Batch ID 생성을 하지 못했습니다.
                    rslt = false;
                }
                else
                {
                    txtLotID.Text = dtMain.Rows[0]["OUT_LOTID"].ToString();
                    rslt = true;
                }
                return rslt;
            }
            catch (Exception ex)
            {
                //Util.AlertInfo("SFU1311");  //Batch ID 생성을 하지 못했습니다.
                Util.MessageException(ex);
                return false;
            }

            //new ClientProxy().ExecuteService(_BizRule, "IN_EQP", "OUT_LOT", IndataTable, (dtMain, searchException) =>
            //{
            //    try
            //    {
            //        if (searchException != null)
            //        {
            //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //            rslt = false;
            //            return;
            //        }
            //        txtLotID.Text = dtMain.Rows[0]["OUT_LOTID"].ToString();
            //        rslt = true;
            //        return;
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //        rslt = false;
            //        return;
            //    }
            //});

            //return rslt;

        }
        private void LotStart()
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("IFMODE", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("OUT_LOTID", typeof(string));
            IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            Indata["IFMODE"] = IFMODE.IFMODE_OFF;
            Indata["EQPTID"] = _EqptID;
            Indata["OUT_LOTID"] = txtLotID.Text;
            Indata["WO_DETL_ID"] = sWO_DETL_ID;
            Indata["USERID"] = LoginInfo.USERID;
            IndataTable.Rows.Add(Indata);

            if (_ProcID.Equals(Process.MIXING))
            {
                _BizRule = "BR_PRD_REG_START_LOT_MX";
            }
            else if (_ProcID.Equals(Process.SRS_MIXING))
            {
                _BizRule = "BR_PRD_REG_START_LOT_SX";
            }
            else if (_ProcID.Equals(Process.PRE_MIXING))
            {
                _BizRule = "BR_PRD_REG_START_LOT_PM";
            }
            else if (_ProcID.Equals(Process.BS))
            {
                _BizRule = "BR_PRD_REG_START_LOT_BS";
            }
            else if (_ProcID.Equals(Process.CMC))
            {
                _BizRule = "BR_PRD_REG_START_LOT_CMC";
            }
            else if (_ProcID.Equals(Process.InsulationMixing))
            {
                _BizRule = "BR_PRD_REG_START_LOT_INSULT_MX";
            }
            else if (_ProcID.Equals(Process.DAM_MIXING))
            {
                _BizRule = "BR_PRD_REG_START_LOT_DAM_MX";
            }
            new ClientProxy().ExecuteService(_BizRule, "IN_EQP", null, IndataTable, (result, Returnex) =>
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (Returnex != null)
                    {
                        Util.MessageException(Returnex);
                        return;
                    }
                    //정상처리되었습니다.
                    Util.MessageInfo("SFU1275", (mResult) =>
                    {
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    });

                }
                catch (Exception ex)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU1838"), ex.Message, "Info.", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageException(ex);
                }
            });
        }
        #endregion

    }
}
