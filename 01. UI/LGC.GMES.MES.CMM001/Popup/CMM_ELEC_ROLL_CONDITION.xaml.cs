/*************************************************************************************
 Created Date : 2019.11.12
      Creator : 
   Decription : GMES 공정진척 UI(ROLL PRESS)에 호기별 모델, 버전, L/R, Lot Type 
                설정기능 추가.
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.22  정재홍 : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건 [방향 ComboBox 바인딩 변경]
  2024.04.17  김도형 : [E20240219-000362] [ESWA PI]ElectrodeProcessProgress_ChangeWOtype
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;


namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ELEC_ROLL_CONDITION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private object[] tmps = null;
        bool isSaved;

        string _EltrTypeCode = string.Empty;   // [E20240219-000362] [ESWA PI]ElectrodeProcessProgress_ChangeWOtype

        private string _sProcID = string.Empty;

        public CMM_ELEC_ROLL_CONDITION()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            Initialize();

            isSaved = false;

        }

        private void Initialize()
        {
            //프로젝트명
            txtPjtName.Text = tmps[3].ToString();

            // 공정
            _sProcID = tmps[4].ToString();

            //LOTTYPE Combo
            CommonCombo combo = new CommonCombo();
            combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT, sCase: "LOTTYPE");

            const string bizRuleName = "DA_PRD_SEL_CONV_RATE";
            string[] arrColumn = { "PRODID", "AREAID" };
            string[] arrCondition = { Util.NVC(tmps[0]), LoginInfo.CFG_AREA_ID };
            string selectedValueText = cboPlanVer.SelectedValuePath;
            string displayMemberText = cboPlanVer.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboPlanVer, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);

            // E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건 [방향 ComboBox 바인딩 변경]
            if (_Util.IsCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(_sProcID, Process.ROLL_PRESSING))
            {
                SetCombo_SideType(cboSideType, "WRK_HALF_SLIT_SIDE", "3");
            }
            else
            {
                SetCombo_SideType(cboSideType, "WRK_HALF_SLIT_SIDE", "1");
            }

            SetSideTypeVisibility();
        }


        private void SetCombo_SideType(C1ComboBox cbo, string sCmcd, string sAttr)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = sCmcd;  //"WRK_HALF_SLIT_SIDE"
            dr["ATTRIBUTE1"] = sAttr;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "RQSTDT", "RSLTDT", RQSTDT);

            DataTable dtCombo = null;

            if (sAttr == "3")
            {
                dtCombo = dtResult;
            }
            else if (sAttr == "1")
            {
                dtCombo = dtResult.Select(" CBO_CODE <> 'A' ").CopyToDataTable();
            }

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";
            cbo.ItemsSource = AddStatus(dtCombo, CommonCombo.ComboStatus.NA, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        #endregion

        #region Event
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ChkValidation())
                return;

            if (_Util.IsCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(_sProcID, Process.ROLL_PRESSING))
            {
                Save_EMRoll();
            }
            else
            {
                Save();
            }
            
        }

        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isSaved)
            {
                Util.Alert("SFU4215");  //저장되지 되지 않은 데이터 입니다.
                e.Cancel = true;
            }
        }
        #endregion

        #region Mehod
        private bool ChkValidation()
        {
            if (cboPlanVer.SelectedIndex == 0)
            {
                Util.Alert("SFU6031"); //선택된 버전이 없습니다. 버전을 선택하세요.
                return false;
            }

            if (_Util.IsCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(_sProcID, Process.ROLL_PRESSING))
            {
                if (!_EltrTypeCode.Equals("A"))  // [E20240219-000362] [ESWA PI]ElectrodeProcessProgress_ChangeWOtype
                {
                    if (cboSideType.SelectedIndex == 0)
                    {
                        Util.Alert("SFU6030"); // 무지부방향을 선택하세요 // [E20240219-000362] [ESWA PI]ElectrodeProcessProgress_ChangeWOtype (SFU6031->SFU6030)
                        return false;
                    }
                }
            }

            if (cboLotType.SelectedIndex == 0)
            {
                Util.Alert("SFU4068"); //선택된 LOTTYPE이 없습니다. LOTTYPE을 선택하세요.
                return false;
            }

            return true;
        }

        private void SetSideTypeVisibility()
        {

            DataTable inData = new DataTable();
            inData.Columns.Add("EQPTID", typeof(string));
            DataRow newRow = inData.NewRow();
            newRow["EQPTID"] = Util.NVC(tmps[1]);
            inData.Rows.Add(newRow);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_INFO", "INDATA", "OUTDATA", inData);
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                if(dtResult.Rows[0]["ELTR_TYPE_CODE"].ToString().Equals("A"))
                {
                    txtSideType.Visibility = Visibility.Collapsed;
                    cboSideType.Visibility = Visibility.Collapsed; 
                }
                _EltrTypeCode = dtResult.Rows[0]["ELTR_TYPE_CODE"].ToString(); // [E20240219-000362] [ESWA PI] ElectrodeProcessProgress_ChangeWOtype
            }
        }

        private void Save()
        {
            DataSet indataSet = new DataSet();

            DataTable inData = indataSet.Tables.Add("INDATA");

            inData.Columns.Add("EQPTID", typeof(string));
            inData.Columns.Add("WO_DETL_ID", typeof(string));
            inData.Columns.Add("PRODID", typeof(string));
            inData.Columns.Add("PROD_VER_CODE", typeof(string));
            inData.Columns.Add("WRK_HALF_SLIT_SIDE", typeof(string));
            inData.Columns.Add("LOTTYPE", typeof(string));
            inData.Columns.Add("USERID", typeof(string));

            DataRow newRow = inData.NewRow();
            newRow["EQPTID"] = Util.NVC(tmps[1]);
            newRow["WO_DETL_ID"] = Util.NVC(tmps[2]);
            newRow["PRODID"] = Util.NVC(tmps[0]);
            newRow["PROD_VER_CODE"] = cboPlanVer.SelectedValue.ToString();

            if (!string.IsNullOrEmpty(cboSideType.Text))
                newRow["WRK_HALF_SLIT_SIDE"] = cboSideType.SelectedValue.ToString();

            newRow["LOTTYPE"] = cboLotType.SelectedValue.ToString();
            newRow["USERID"] = LoginInfo.USERID;

            indataSet.Tables["INDATA"].Rows.Add(newRow);

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_WO_EIO_INFO", "INDATA", null, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.MessageException(Exception);
                }
                else
                {
                    isSaved = true;
                    this.DialogResult = MessageBoxResult.OK;
                }

            }, indataSet);
        }

        private void Save_EMRoll()
        {
            DataSet indataSet = new DataSet();

            DataTable inData = indataSet.Tables.Add("INDATA");

            inData.Columns.Add("EQPTID", typeof(string));
            inData.Columns.Add("WO_DETL_ID", typeof(string));
            inData.Columns.Add("WRK_HALF_SLIT_SIDE", typeof(string));
            inData.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
            inData.Columns.Add("LOTTYPE", typeof(string));
            inData.Columns.Add("UPDUSER", typeof(string));
            inData.Columns.Add("PROD_VER_CODE", typeof(string));

            DataRow newRow = inData.NewRow();
            newRow["EQPTID"] = Util.NVC(tmps[1]);
            newRow["WO_DETL_ID"] = Util.NVC(tmps[2]);
            newRow["PROD_VER_CODE"] = cboPlanVer.SelectedValue.ToString();

            if (!_EltrTypeCode.Equals("A"))  //  2024.04.17  김도형 : [E20240219-000362] [ESWA PI]ElectrodeProcessProgress_ChangeWOtype
            {
                newRow["WRK_HALF_SLIT_SIDE"] = cboSideType.SelectedValue.ToString().Substring(0, 1);
                newRow["EM_SECTION_ROLL_DIRCTN"] = cboSideType.SelectedValue.ToString().Substring(2, 1);
            }
            newRow["LOTTYPE"] = cboLotType.SelectedValue.ToString();
            newRow["UPDUSER"] = LoginInfo.USERID;

            indataSet.Tables["INDATA"].Rows.Add(newRow);

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_LOGIS_COND", "INDATA", null, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.MessageException(Exception);
                }
                else
                {
                    isSaved = true;
                    this.DialogResult = MessageBoxResult.OK;
                }

            }, indataSet);
        }
        #endregion


    }
}
