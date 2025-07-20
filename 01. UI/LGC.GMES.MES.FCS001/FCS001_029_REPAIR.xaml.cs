/*************************************************************************************
 Created Date : 2020.12.21
      Creator : PJG
   Decription : Gripper 수리 등록
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.21  PJG : Initial Created
  2022.03.08  KDH : 설비리스트출력 조건 수정
  2022.12.24  LJM : 72개의 Cell로 인해 체크박스 및 텍스트박스 수정
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_029_REPAIR : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        private string _sEqpKind = string.Empty;
        private string _sLaneX = string.Empty;

        public string EQPKIND {
            set { this._sEqpKind = value; }
        }

        public FCS001_029_REPAIR()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;

            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _sEqpKind = tmps[0] as string;

            switch (_sEqpKind)
            {
                case "1": //충방전기
                    _sEqpKind = "1";

                    break;

                case "8": //OCV
                    _sEqpKind = "8";

                    _sLaneX = "ALL";
                    break;

                case "J": //JIG
                    _sEqpKind = "J";

                    _sLaneX = "ALL";

                    break;
            }

            InitCombo();
            InitSpread();
        }

        #endregion

        #region [Method]

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", _sEqpKind };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: sFilterEqpType);

            C1ComboBox[] cboLaneMapChild = { cboRow, cboCol, cboStg, cboEqp };
            string[] sFilterLane = { _sLaneX };
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE", sFilter: sFilterLane, cbChild: cboLaneMapChild);

            C1ComboBox[] cboRowMapParent = { cboEqpKind, cboLane };
            _combo.SetCombo(cboRow, CommonCombo_Form.ComboStatus.NONE, sCase: "ROW", cbParent: cboRowMapParent);
            _combo.SetCombo(cboCol, CommonCombo_Form.ComboStatus.NONE, sCase: "COL", cbParent: cboRowMapParent);
            _combo.SetCombo(cboStg, CommonCombo_Form.ComboStatus.NONE, sCase: "STG", cbParent: cboRowMapParent);

            C1ComboBox[] cboRowMapParent1 = { cboLane, cboEqpKind }; //20220308_설비리스트출력 조건 수정
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPIDBYLANE", cbParent: cboRowMapParent1);

            string[] sFilterTrayLoc = { "TRAY_LOC", null };
            _combo.SetCombo(cboTrayLoc, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: sFilterTrayLoc);
        }

        private void InitSpread()
        {
            DataTable dtAdd = new DataTable();
            dtAdd.TableName = "dtAdd";
            dtAdd.Columns.Add("Pos1", typeof(string));
            dtAdd.Columns.Add("Chk1", typeof(bool));
            dtAdd.Columns.Add("Pos2", typeof(string));
            dtAdd.Columns.Add("Chk2", typeof(bool));

            for (int i = 0; i < 18; i++)
            {
                DataRow drAdd = dtAdd.NewRow();

                drAdd["Pos1"] = (18 + i + 1).ToString();
                drAdd["Chk1"] = false;
                drAdd["Pos2"] = (i + 1).ToString();
                drAdd["Chk2"] = false;

                dtAdd.Rows.Add(drAdd);
            }

            dgList.ItemsSource = DataTableConverter.Convert(dtAdd);
        }

        #endregion

        #region [Event]
        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("EQP_ID", typeof(string));
                dtRqst.Columns.Add("CONTENTS", typeof(string));
                dtRqst.Columns.Add("REG_ID", typeof(string));
                dtRqst.Columns.Add("PIN_ROW", typeof(string));
                dtRqst.Columns.Add("PIN_COL", typeof(string));
                dtRqst.Columns.Add("TRAY_LOC", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_COL_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_STG_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_KIND_CD", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                string sContents = Util.GetCondition(txtContents, sMsg: "FM_ME_0117");  //내용을 입력해주세요.
                if (string.IsNullOrEmpty(sContents.ToString())) return;
                string sTrayLoc = Util.GetCondition(cboTrayLoc);
                string sLaneId = Util.GetCondition(cboLane);
                string sEqpRowLoc = _sEqpKind.Equals("1") ? Util.GetCondition(cboRow) : null;
                string sEqpColLoc = _sEqpKind.Equals("1") ? Util.GetCondition(cboCol) : null;
                string sEqpStgLoc = _sEqpKind.Equals("1") ? Util.GetCondition(cboStg) : null;
                string sEqpId = _sEqpKind.Equals("8") ? Util.GetCondition(cboEqp) : null;
                string sEqpKindCd = _sEqpKind;

                for (int i = 0; i < 18; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "Chk1")).Equals("True"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["EQP_ID"] = sEqpId;
                        dr["LANE_ID"] = sLaneId;
                        dr["EQP_ROW_LOC"] = sEqpRowLoc;
                        dr["EQP_COL_LOC"] = sEqpColLoc;
                        dr["EQP_STG_LOC"] = sEqpStgLoc;
                        dr["EQP_KIND_CD"] = sEqpKindCd;
                        dr["CONTENTS"] = sContents;
                        dr["REG_ID"] = LoginInfo.USERID;
                        dr["PIN_ROW"] = i + 18 + 1;
                        dr["PIN_COL"] = "0";
                        dr["TRAY_LOC"] = sTrayLoc;
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "Chk2")).Equals("True"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["EQP_ID"] = sEqpId;
                        dr["LANE_ID"] = sLaneId;
                        dr["EQP_ROW_LOC"] = sEqpRowLoc;
                        dr["EQP_COL_LOC"] = sEqpColLoc;
                        dr["EQP_STG_LOC"] = sEqpStgLoc;
                        dr["EQP_KIND_CD"] = sEqpKindCd;
                        dr["CONTENTS"] = sContents;
                        dr["REG_ID"] = LoginInfo.USERID;
                        dr["PIN_ROW"] = i + 1;
                        dr["PIN_COL"] = "0";
                        dr["TRAY_LOC"] = sTrayLoc;
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }                
                }

                if (dtRqst.Rows.Count == 0)
                {
                    Util.Alert("SFU1751");  // 위치를 선택 하세요.

                    return;
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_INS_PIN_MAINT", "RQSTDT", "RSLTDT", dtRqst);

                Util.MessageInfo("FM_ME_0215");  //저장하였습니다.
                Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboLane_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sLane = Util.GetCondition(cboLane).ToString();

            if (LoginInfo.CFG_AREA_ID.Equals("A5"))
            {
                if (sLane.Equals("") || string.IsNullOrEmpty(sLane)) return;

                if ((sLane.Substring(4,1).Equals("3")))
                {
                    Util.gridClear(dgList);

                    DataTable dtAddList = new DataTable();
                    dtAddList.TableName = "dtAdd";
                    dtAddList.Columns.Add("Pos1", typeof(string));
                    dtAddList.Columns.Add("Chk1", typeof(bool));
                    dtAddList.Columns.Add("Pos2", typeof(string));
                    dtAddList.Columns.Add("Chk2", typeof(bool));

                    for (int i = 0; i < 54; i++)
                    {
                        DataRow drAdd = dtAddList.NewRow();

                        drAdd["Pos1"] = (18 + i + 1).ToString();
                        drAdd["Chk1"] = false;
                        drAdd["Pos2"] = (i + 1).ToString();
                        drAdd["Chk2"] = false;

                        dtAddList.Rows.Add(drAdd);
                    } 

                    dgList.ItemsSource = DataTableConverter.Convert(dtAddList);
                } 

                else
                {
                    Util.gridClear(dgList);
                    DataTable dtList = new DataTable();
                    dtList.TableName = "dtAdd";
                    dtList.Columns.Add("Pos1", typeof(string));
                    dtList.Columns.Add("Chk1", typeof(bool));
                    dtList.Columns.Add("Pos2", typeof(string));
                    dtList.Columns.Add("Chk2", typeof(bool));

                    for (int i = 0; i < 18; i++)
                    {
                        DataRow drAdd = dtList.NewRow();

                        drAdd["Pos1"] = (18 + i + 1).ToString();
                        drAdd["Chk1"] = false;
                        drAdd["Pos2"] = (i + 1).ToString();
                        drAdd["Chk2"] = false;

                        dtList.Rows.Add(drAdd);
                    } 

                    dgList.ItemsSource = DataTableConverter.Convert(dtList);
                }
            }     
        } 

        #endregion
    }
}
