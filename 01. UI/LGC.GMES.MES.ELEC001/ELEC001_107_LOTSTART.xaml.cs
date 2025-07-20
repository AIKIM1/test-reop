/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : SRS Coater 작업시작
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

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_107_LOTSTART : C1Window, IWorkArea
    {

        #region Initialize

        private static string sEqptID = string.Empty;
        private static string sElectrode = string.Empty;
        private static string sWOID = string.Empty;
        private static string sLargeLot = string.Empty;
        private static string sWO_DETL_ID = string.Empty;
        private string sEqsgID = string.Empty;

        DataSet inDataSet = null;
        DataSet _MaterialDataSet = null;
        DataSet _SlurryDataSet = null;

        Util _Util = new Util();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_107_LOTSTART()
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
            sLargeLot = tmps[2].ToString();
            sWO_DETL_ID = tmps[3].ToString();
            //dtpDate.SelectedDateTime = System.DateTime.Now;
            TimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (!GetWorkorder())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }
            txtLotID.Text = sLargeLot;
            GetCurrentMount();
            //GetSlurry();

            if (dgSlurry.GetRowCount() == 0 || string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgSlurry.Rows[0].DataItem, "INPUT_LOTID"))))
            {
                Util.Alert("SFU3198"); //해당 설비에 Slurry Batch가 지정되지 않아 작업시작을 할 수 없습니다.
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidData()) return;

            LotStart();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private void GetCurrentMount()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_CT", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    DataTable Foildt = dtMain.Clone();

                    foreach (DataRow _iRow in dtMain.Select("PRDT_CLSS_CODE <> 'ASL'"))
                    {
                        Foildt.ImportRow(_iRow);
                    }
                    dgFoil.ItemsSource = DataTableConverter.Convert(Foildt);
                }
                //이송탱크
                GetSlurry();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetSlurry()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_SRSTANK_CURR_MOUNT", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    DataTable Slurrydt = dtMain.Clone();
                    foreach (DataRow _iRow in dtMain.Rows)
                    {
                        Slurrydt.ImportRow(_iRow);
                    }
                    dgSlurry.ItemsSource = DataTableConverter.Convert(Slurrydt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool GetWorkorder()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1436");  //W/O 선택 후 작업시작하세요.
                    rslt = false;
                }
                else
                {
                    txtWorkOrder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();
                    rslt = true;
                }
                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private bool ValidData()
        {
            if (txtWorkOrder.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1443");  //작업지시를 선택하세요.
                return false;
            }
            //이송탱크 : 
            if (dgSlurry.GetRowCount() == 0)
            {
                Util.MessageValidation("SFU1425");  //Slurry ID를 입력 해 주세요.
                return false;
            }

            if (Convert.ToBoolean(chkFirstFlag.IsChecked))
            {
                //if (dgSlurry.GetRowCount() == 0)
                //{
                //    Util.MessageValidation("SFU1425");  //Slurry ID를 입력 해 주세요.
                //    return false;
                //}

            }
            else
            {
                if (txtLotID.Text.Trim() == "")
                {
                    Util.MessageValidation("SFU1836");  //작업대상 대LOT을 선택 후 시작하십시오.
                    return false;
                }
            }

            return true;
        }
        private void LotStart()
        {
            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    inDataSet = new DataSet();
                _MaterialDataSet = new DataSet();
                _SlurryDataSet = new DataSet();

    #if false
                string sLotID = SearchNewLargeLOTID("D");
    #endif

                #region Lot Info
                DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");
                inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                inLotDataTable.Columns.Add("IFMODE", typeof(string));
                //inLotDataTable.Columns.Add("LANGID", typeof(string));
                inLotDataTable.Columns.Add("EQPTID", typeof(string));
                inLotDataTable.Columns.Add("USERID", typeof(string));
                inLotDataTable.Columns.Add("FIRST_FLAG", typeof(string));
                inLotDataTable.Columns.Add("LOTID", typeof(string));  // 대LOT 정보

                DataRow inLotDataRow = null;

                inLotDataRow = inLotDataTable.NewRow();
                inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                //inLotDataRow["LANGID"] = LoginInfo.LANGID;
                inLotDataRow["EQPTID"] = sEqptID;
                inLotDataRow["USERID"] = LoginInfo.USERID;
                inLotDataRow["FIRST_FLAG"] = chkFirstFlag.IsChecked == true ? "Y" : "N";    // Y: 대LOT 발번, N: CUT Start
                inLotDataRow["LOTID"] = chkFirstFlag.IsChecked == true ? null : sLargeLot;  // 대LOT 정보
                inLotDataTable.Rows.Add(inLotDataRow);
                #endregion

                #region Foil
                DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
                InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow inMtrlDataRow = null;

                DataTable dtFoil = DataTableConverter.Convert(dgFoil.ItemsSource);
                foreach (DataRow _iRow in dtFoil.Rows)
                {
                    if (!_iRow["INPUT_LOTID"].ToString().Equals(""))
                    {
                        inMtrlDataRow = InMtrldataTable.NewRow();
                        inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = _iRow["EQPT_MOUNT_PSTN_ID"];
                        inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        inMtrlDataRow["INPUT_LOTID"] = _iRow["INPUT_LOTID"];
                        InMtrldataTable.Rows.Add(inMtrlDataRow);
                    }
                }

                DataTable dtSlurry = DataTableConverter.Convert(dgSlurry.ItemsSource);
                foreach (DataRow _iRow in dtSlurry.Rows)
                {
                    if (!_iRow["INPUT_LOTID"].ToString().Equals(""))
                    {
                        inMtrlDataRow = InMtrldataTable.NewRow();
                        inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = _iRow["EQPT_MOUNT_PSTN_ID"];
                        inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        inMtrlDataRow["INPUT_LOTID"] = _iRow["INPUT_LOTID"];
                        InMtrldataTable.Rows.Add(inMtrlDataRow);
                    }
                }

                #endregion

                try
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CREATE_START_LOT_CT", "INDATA,IN_INPUT", null, inDataSet);
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

                    //정상처리되었습니다.
                    Util.MessageInfo("SFU1275", (mResult) =>
                    {
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    });
                
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU1838"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);  //작업시작 정보 확인
                    Util.MessageException(ex);
                }
                }
            });

        }
        #endregion

    }
}
