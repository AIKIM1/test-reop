/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created..
  2023.04.06  권혜정 : ROUTE 변경 시, 생산라인/MODEL 자동 선택 및 비활성화
  2023.04.18  권혜정 : 자동 선택 및 비활성화 동별 공통 코드 추가(PROGRAM_BY_FUNC_USE_FLAG/FCS001_005_ROUTE_CHG)
  2023.07.28  권순범 : TRAY LIST화면에서 TRAY 1단만 선택할 시 1,2단 TRAY를 조회해서 같이 라우트변경 할 수 있도록 보완로직 추가



 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_005_ROUTE_CHG : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private DataTable dtTrayList = null;
        private string[] sLineModel = new string[3];

        Util _Util = new Util();
        public bool bUseFlag = false;

        public DataTable TrayList
        {
            set { this.dtTrayList = value; }
        }

        public string[] LineModel
        {
            set { this.sLineModel = value; }
        }
        #endregion

        #region Initialize
        public FCS001_005_ROUTE_CHG()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();
        }
        #endregion

        #region Event

        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboOp };
            string[] sFilter1 = { null, null, null, null, null };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.SELECT, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            C1ComboBox[] cboOpParent = { cboRoute };
            ComCombo.SetCombo(cboOp, CommonCombo_Form.ComboStatus.NONE, sCase: "ROUTE_OP", cbParent: cboOpParent);

            string sLine = sLineModel[1];
            string sModel = sLineModel[2];
            
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_005_ROUTE_CHG");

            if (bUseFlag) //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            {
                if (!string.IsNullOrEmpty(sLine))
                {
                    cboLine.SelectedValue = sLine;
                    cboLine.IsEnabled = false;
                }

                if (!string.IsNullOrEmpty(sModel))
                {
                    cboModel.SelectedValue = sModel;
                    cboModel.IsEnabled = false;
                }
            }
        }

        #endregion

        #region Mehod

        private void SearchTrayStorage(DataTable dtRequestData, string _Route, string _Op)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();

            if (dtTrayList.Rows.Count > 0)
                for (int i = 0; i < dtTrayList.Rows.Count; i++)
                    dic.Add(i + 1, dtTrayList.Rows[i]["CSTID"].ToString());

            for (int i = 0; i < dtTrayList.Rows.Count; i++)
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["CSTID"] = dtTrayList.Rows[i]["CSTID"];
                dtIndata.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_BY_LOAD_REP_CSTID", "RQSTDT", "RSLTDT", dtIndata);

                if (dtRslt.Rows.Count == 2)  //2단적재 Tray면
                {
                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                        if (dtTrayList.Rows[i]["CSTID"] != dtRslt.Rows[j]["CSTID"])
                            if (!dic.ContainsValue(dtRslt.Rows[j]["CSTID"].ToString()))
                            {
                                DataRow dr2 = dtRequestData.NewRow();
                                dr2["CSTID"] = dtRslt.Rows[j]["CSTID"];
                                dr2["BIZ_NAME"] = ObjectDic.Instance.GetObjectName("ROUTE 변경");  //ROUTE 변경
                                dr2["BIZ_ID"] = "BR_SET_TRAY_ROUTE_CHANGE";
                                dr2["LOG_YN"] = "Y";
                                dr2["PRE_ROUTID"] = dtTrayList.Rows[i]["BF_ROUTE_ID"];
                                dr2["AFTER_ROUTID"] = _Route;
                                dr2["AFTER_PROCID"] = _Op;
                                dr2["USERID"] = LoginInfo.USERID;
                                dtRequestData.Rows.Add(dr2);

                                break;
                            }
                }
                else // 2단적재 Tray가 아니면
                {
                    continue;
                }
            }
        }

        #endregion

        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            //공정경로를 변경하시겠습니까?
            Util.MessageConfirm("FM_ME_0104", (result) =>
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    try
                    {
                        string sRoute = Util.GetCondition(cboRoute, sMsg: "FM_ME_0106");  //공정경로를 선택해주세요.
                        if (string.IsNullOrEmpty(sRoute)) return;

                        string sOp = Util.GetCondition(cboOp, sMsg: "FM_ME_0107");  //공정을 선택해주세요.
                        if (string.IsNullOrEmpty(sOp)) return;

                        //Tray 공정 체크
                        if (dtTrayList.Columns.Contains("AFTER_PROCID"))
                            dtTrayList.Columns.Remove(new DataColumn("AFTER_PROCID"));

                        //Tray 공정 체크
                        DataColumn dcAop = new DataColumn("AFTER_PROCID", typeof(string));
                        dcAop.DefaultValue = sOp;
                        dtTrayList.Columns.Add(dcAop);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_TRAY_OP_COMPARE", "RQSTDT", "RSLTDT", dtTrayList);

                        switch (Convert.ToInt16(dtRslt.Rows[0]["RETVAL"].ToString()))
                        {
                            case 1:
                                Util.AlertInfo("FM_ME_0163");  //선택된 Tray가 모두 같은 공정일때만 변경 가능합니다.
                                return;

                            case 2:
                                Util.AlertInfo("FM_ME_0164");  //선택된 Tray와 변경할 공정이 같은 Aging 공정이어야 변경 가능합니다.
                                return;
                        }

                        //2021-05-05 : 건별처리 로직 오류로 인한 수정 (대용량 처리 화면 or 팝업 필요함)
                        // 건별 처리
                        //for (int i = 0; i < dtTrayList.Rows.Count; i++)
                        //{
                        //    DataSet indataSet = new DataSet();
                        //    DataTable dtIndata = indataSet.Tables.Add("INDATA");
                        //    dtIndata.Columns.Add("USERID", typeof(string));
                        //    dtIndata.Columns.Add("EQPTID", typeof(string));

                        //    DataTable dtInCst = indataSet.Tables.Add("IN_CST");
                        //    dtInCst.Columns.Add("CSTID", typeof(string));
                        //    dtInCst.Columns.Add("PRE_ROUTID", typeof(string));
                        //    dtInCst.Columns.Add("AFTER_ROUTID", typeof(string));
                        //    dtInCst.Columns.Add("AFTER_PROCID", typeof(string));
                        //    DataRow InRow = dtIndata.NewRow();
                        //    DataRow RowCell = dtInCst.NewRow();

                        //    InRow["USERID"] = dtTrayList.Rows[i]["MDF_ID"];
                        //    InRow["EQPTID"] = DBNull.Value;
                        //    dtIndata.Rows.Add(InRow);

                        //    RowCell["CSTID"] = dtTrayList.Rows[i]["CSTID"];
                        //    RowCell["PRE_ROUTID"] = dtTrayList.Rows[i]["BF_ROUTE_ID"];
                        //    RowCell["AFTER_ROUTID"] = sRoute;
                        //    RowCell["AFTER_PROCID"] = sOp;
                        //    dtInCst.Rows.Add(RowCell);

                        //    ShowLoadingIndicator();
                        //    new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_ROUTE_CHANGE", "INDATA,IN_CST", "OUTDATA", (bizResult, bizException) =>
                        //    {
                        //        try
                        //        {
                        //            if (bizException != null)
                        //            {
                        //                Util.MessageException(bizException);
                        //                return;
                        //            }

                        //            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                        //            {
                        //                Util.AlertInfo("FM_ME_0136");  //변경완료하였습니다.

                        //            }
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            HiddenLoadingIndicator();
                        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        //        }
                        //        finally
                        //        {
                        //            HiddenLoadingIndicator();
                        //        }
                        //    }, indataSet);
                        //}

                        //대용량 데이터 처리부분 - 대용량 처리용 화면 자체가 개발되어 있지 않으므로 위 개별 처리 로직으로 처리
                        DataTable dtRequestData = new DataTable();

                        //필수 컬럼 Start
                        dtRequestData.Columns.Add("CSTID", typeof(string));
                        dtRequestData.Columns.Add("BIZ_NAME", typeof(string));
                        dtRequestData.Columns.Add("BIZ_ID", typeof(string));
                        dtRequestData.Columns.Add("EXEC_YN", typeof(string));
                        dtRequestData.Columns.Add("LOG_YN", typeof(string));
                        //필수 컬럼 End
                        dtRequestData.Columns.Add("PRE_ROUTID", typeof(string));
                        dtRequestData.Columns.Add("AFTER_ROUTID", typeof(string));
                        dtRequestData.Columns.Add("AFTER_PROCID", typeof(string));
                        dtRequestData.Columns.Add("USERID", typeof(string));

                        //[ksb1223] 1,2단 TRAY를 조회해서 같이 라우트변경 할 수 있도록 보완로직 추가
                        SearchTrayStorage(dtRequestData, sRoute, sOp);

                        //
                        for (int i = 0; i < dtTrayList.Rows.Count; i++)
                        {
                            DataRow dr = dtRequestData.NewRow();
                            dr["CSTID"] = dtTrayList.Rows[i]["CSTID"];
                            dr["BIZ_NAME"] = ObjectDic.Instance.GetObjectName("ROUTE 변경");  //ROUTE 변경
                            dr["BIZ_ID"] = "BR_SET_TRAY_ROUTE_CHANGE";
                            dr["LOG_YN"] = "Y";
                            dr["PRE_ROUTID"] = dtTrayList.Rows[i]["BF_ROUTE_ID"];
                            dr["AFTER_ROUTID"] = sRoute;
                            dr["AFTER_PROCID"] = sOp;
                            dr["USERID"] = LoginInfo.USERID;
                            dtRequestData.Rows.Add(dr);
                        }

                        FCS001_DataProc FCS001_DataProc = new FCS001_DataProc();
                        FCS001_DataProc.FrameOperation = FrameOperation;

                        object[] parameters = new object[2];
                        parameters[0] = dtRequestData;
                        parameters[1] = "FCS001_005_ROUTE_CHG";
                        this.FrameOperation.OpenMenuFORM("FCS001_DataProc", "FCS001_DataProc", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("FRMDATAPROC"), true, parameters);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
            Close();
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
    }
}
