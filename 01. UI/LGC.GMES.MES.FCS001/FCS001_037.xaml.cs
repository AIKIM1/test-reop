/*************************************************************************************
 Created Date : 2020.12.03
      Creator : Kang Dong Hee
   Decription : 상대판정 현황 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.23  NAME : Initial Created
  2021.04.05  KDH : 공정 그룹 추가
  2022.02.22  KDH : AREA 조건 추가
  2022.12.15  형준우 : 초기화, 수동SPEC산출 버튼 클릭 시, 해당 열 데이터를 가져오도록 수정
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_037 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        #endregion

        #region [Initialize]
        public FCS001_037()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting            
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            //공정경로 별 조회
            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboOper };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            C1ComboBox[] cboOperParent = { cboRoute };
            string[] sFilterProcGrCode = { "3,7" };
            ComCombo.SetCombo(cboOper, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent, sFilter: sFilterProcGrCode); //20210405 공정 그룹 추가

            //Lot 별 조회
            C1ComboBox[] cboLineChild2 = { cboLotModel };
            ComCombo.SetCombo(cboLotLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild2);

            C1ComboBox[] cboModelChild2 = { cboLotRoute };
            C1ComboBox[] cboModelParent2 = { cboLotLine };
            ComCombo.SetCombo(cboLotModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild2, cbParent: cboModelParent2);

            C1ComboBox[] cboRouteParent2 = { cboLotLine, cboLotModel };
            C1ComboBox[] cboRouteChild2 = { cboLotOper };
            ComCombo.SetCombo(cboLotRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent2, cbChild: cboRouteChild2);

            C1ComboBox[] cboOperParent2 = { cboLotRoute };
            string[] sFilterProcGrCode1 = { "3,7" };
            ComCombo.SetCombo(cboLotOper, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP", cbParent: cboOperParent2, sFilter: sFilterProcGrCode1); //20210405 공정 그룹 추가
        }
        #endregion

        #region [Method]
        private void GetRouteList()
        {
            try
            {
                Util.gridClear(dgRJudgStusRoute);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RJUDG_ROUTE_SITUATION", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgRJudgStusRoute, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetLotList()
        {
            try
            {
                Util.gridClear(dgRJudgStusLot);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2022.02.22_AREA 조건 추가
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("JUDG_PROG_PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.22_AREA 조건 추가
                dr["EQSGID"] = Util.GetCondition(cboLotLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboLotModel, bAllNull: true);
                dr["PROD_LOTID"] = Util.GetCondition(txtLotId, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboLotRoute, bAllNull: true);
                dr["JUDG_PROG_PROCID"] = Util.GetCondition(cboLotOper, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RJUDG_LOT_SITUATION", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgRJudgStusLot, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SetGridCboItem_RouteTypeCode(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;
                if (!string.IsNullOrEmpty(sCmnCd))
                {
                    dr["CMCODE_LIST"] = sCmnCd;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            SetGridCboItem_RouteTypeCode(dgRJudgStusRoute.Columns["ROUT_TYPE_CODE"], "ROUT_TYPE_CODE");
            GetRouteList();
        }

        private void dgRJudgStusRoute_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgRJudgStusRoute.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    cboLotLine.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgRJudgStusRoute.Rows[cell.Row.Index].DataItem, "EQSGID"));

                    cboLotModel.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgRJudgStusRoute.Rows[cell.Row.Index].DataItem, "MDLLOT_ID"));
                    cboLotRoute.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgRJudgStusRoute.Rows[cell.Row.Index].DataItem, "ROUTID"));
                    cboLotOper.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgRJudgStusRoute.Rows[cell.Row.Index].DataItem, "PROCID"));

                    GetLotList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgRJudgStusLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgRJudgStusLot.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "SPEC_OUT_YN")).Equals("Y"))
                    {
                        //상대판정 SPEC 이력 관리 화면 실행
                        object[] parameters = new object[11];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "EQSGID")).ToString();
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "MDLLOT_ID")).ToString();
                        parameters[2] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID")).ToString();
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "ROUTID")).ToString();
                        parameters[4] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "JUDG_PROCID")).ToString();
                        parameters[5] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "JUDG_PROG_PROCID")).ToString();
                        parameters[6] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "SPEC_CALC_TIME")).ToString();
                        parameters[7] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "SPEC_CALC_TIME")).ToString();
                        parameters[8] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "SPEC_CALC_TIME")).ToString();
                        parameters[9] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "SPEC_CALC_TIME")).ToString();
                        parameters[10] = "Y";                                                                                                   //ACTYN

                        this.FrameOperation.OpenMenu("SFU010710210", true, parameters); //상대판정 SPEC 이력 관리 (FCS001_038 / TSK_820)
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "SPEC_OUT_YN")).Equals("N"))
                    {
                        //상대판정 Tray List 화면 실행
                        object[] parameters = new object[12];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "EQSGID")).ToString();
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "EQSG_NAME")).ToString();
                        parameters[2] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "MDLLOT_ID")).ToString();
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "MDLLOT_NAME")).ToString();
                        parameters[4] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "DAY_GR_LOTID")).ToString();
                        parameters[5] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "ROUTID")).ToString();
                        parameters[6] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "ROUTID")).ToString();
                        parameters[7] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "JUDG_PROCID")).ToString();
                        parameters[8] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "JUDG_PROC_NAME")).ToString();
                        parameters[9] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "JUDG_PROG_PROCID")).ToString();
                        parameters[10] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.Rows[cell.Row.Index].DataItem, "JUDG_PROG_PROC_NAME")).ToString();
                        parameters[11] = "Y";

                        //연계 화면 확인 후 수정
                        this.FrameOperation.OpenMenuFORM("SFU010710211", "FCS001_038_TRAY_LIST", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("상대판정 Tray List"), true, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnLotSearch_Click(object sender, EventArgs e)
        {
            SetGridCboItem_RouteTypeCode(dgRJudgStusLot.Columns["ROUT_TYPE_CODE"], "ROUT_TYPE_CODE");
            GetLotList();
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtLotId.Text)) && (e.Key == Key.Enter))
            {
                btnLotSearch_Click(null, null);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonAccessClear(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void buttonAccessClear(object sender)
        {
            DataGridCellPresenter dc = (DataGridCellPresenter)((Button)sender).Parent;
            C1.WPF.DataGrid.C1DataGrid datagrid = dc.DataGrid;
            int idx = dc.Row.Index;

            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[idx].DataItem, "JUDG_TMP_STOP_FLAG")).Equals("Y"))
                {
                    Util.MessageValidation("FM_ME_0298");  //초기화/재산출 시행 전 해당 상대 판정 Route를 일시정지해야합니다.
                    return;
                }

                Util.MessageConfirm("FM_ME_0292", result => //초기화 대상 Lot : {0}\r\n초기화 대상 Route : {1}\r\n초기화 대상 등급 : {2}\r\n해당 SPEC을 초기화하시겠습니까?
                {
                    if (result == MessageBoxResult.Cancel) return;
                    try
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dtRqst.Columns.Add("ROUTID", typeof(string));
                        dtRqst.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
                        dtRqst.Columns.Add("GRD_ROW_NO", typeof(string));
                        dtRqst.Columns.Add("GRD_COL_NO", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "DAY_GR_LOTID"));
                        dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "ROUTID"));
                        dr["SUBLOT_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "SUBLOT_GRD_CODE"));
                        dr["GRD_ROW_NO"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "GRD_ROW_NO"));
                        dr["GRD_COL_NO"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "GRD_COL_NO"));
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataSet dsRqst = new DataSet();
                        dsRqst.Tables.Add(dtRqst);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REL_JUDG_SPEC_LIST_INIT", "INDATA", "OUTDATA", dsRqst);
                        if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            Util.MessageInfo("FM_ME_0294");  //초기화가 완료되었습니다.
                        }
                        else
                        {
                            Util.Alert("FM_ME_0295");  //초기화 도중 문제가 발생하였습니다.
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, new string[] { Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "DAY_GR_LOTID"))
                    , Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "ROUTID"))
                    , Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "SUBLOT_GRD_CODE")) });

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        private void btnSpec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonAccessSpec(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void buttonAccessSpec(object sender)
        {
            DataGridCellPresenter dc = (DataGridCellPresenter)((Button)sender).Parent;
            C1.WPF.DataGrid.C1DataGrid datagrid = dc.DataGrid;
            int idx = dc.Row.Index;

            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[idx].DataItem, "JUDG_TMP_STOP_FLAG")).Equals("Y"))
                {
                    Util.MessageValidation("FM_ME_0298");  //초기화/재산출 시행 전 해당 상대 판정 Route를 일시정지해야합니다.
                    return;
                }

                Util.MessageConfirm("FM_ME_0293", result => //SPEC 재산출 대상 Lot : {0}\r\nSPEC 재산출 대상 Route : {1}\r\nSPEC 재산출 대상 등급 : {2}\r\n해당 SPEC을 재산출하시겠습니까?
                {
                    if (result == MessageBoxResult.Cancel) return;
                    try
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dtRqst.Columns.Add("ROUTID", typeof(string));
                        dtRqst.Columns.Add("JUDG_PROG_PROCID", typeof(string));
                        dtRqst.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
                        dtRqst.Columns.Add("GRD_ROW_NO", typeof(string));
                        dtRqst.Columns.Add("GRD_COL_NO", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "DAY_GR_LOTID"));
                        dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "ROUTID"));
                        dr["JUDG_PROG_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "JUDG_PROG_PROCID"));
                        dr["SUBLOT_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "SUBLOT_GRD_CODE"));
                        dr["GRD_ROW_NO"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "GRD_ROW_NO"));
                        dr["GRD_COL_NO"] = Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "GRD_COL_NO"));
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataSet dsRqst = new DataSet();
                        dsRqst.Tables.Add(dtRqst);

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REL_JUDG_SPEC_MAN_CALCUL", "INDATA", "OUTDATA", dsRqst);
                        if (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            Util.MessageInfo("FM_ME_0296");  //SPEC 재산출이 완료되었습니다.
                        }
                        else
                        {
                            Util.Alert("FM_ME_0297");  //SPEC 재산출 도중 문제가 발생하였습니다.
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, new string[] { Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "DAY_GR_LOTID"))
                    , Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "ROUTID"))
                    , Util.NVC(DataTableConverter.GetValue(dgRJudgStusLot.CurrentRow.DataItem, "SUBLOT_GRD_CODE")) });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
