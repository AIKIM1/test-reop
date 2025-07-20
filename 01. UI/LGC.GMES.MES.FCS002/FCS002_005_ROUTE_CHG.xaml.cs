/*************************************************************************************
 Created Date : 2022.12.14
      Creator : 강동희
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.14  DEVELOPER : Initial Created..
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_005_ROUTE_CHG : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private DataTable dtTrayList = null;

        public DataTable TrayList
        {
            set { this.dtTrayList = value; }
        }
        #endregion

        #region Initialize
        public FCS002_005_ROUTE_CHG()
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

        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            C1ComboBox[] cboRouteChild = { cboOp };
            string[] sFilter1 = { null, null, null, null, null };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

            C1ComboBox[] cboOpParent = { cboRoute };
            ComCombo.SetCombo(cboOp, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROUTE_OP", cbParent: cboOpParent);
        }
        #endregion

        #region Event
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

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_TRAY_OP_COMPARE_MB", "RQSTDT", "RSLTDT", dtTrayList);

                        switch (Convert.ToInt16(dtRslt.Rows[0]["RETVAL"].ToString()))
                        {
                            case 1:
                                Util.AlertInfo("FM_ME_0163");  //선택된 Tray가 모두 같은 공정일때만 변경 가능합니다.
                                return;

                            case 2:
                                Util.AlertInfo("FM_ME_0164");  //선택된 Tray와 변경할 공정이 같은 Aging 공정이어야 변경 가능합니다.
                                return;
                        }

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

                        for (int i = 0; i < dtTrayList.Rows.Count; i++)
                        {
                            DataRow dr = dtRequestData.NewRow();
                            dr["CSTID"] = dtTrayList.Rows[i]["CSTID"];
                            dr["BIZ_NAME"] = ObjectDic.Instance.GetObjectName("ROUTE 변경");  //ROUTE 변경
                            dr["BIZ_ID"] = "BR_SET_TRAY_ROUTE_CHANGE_MB";
                            dr["LOG_YN"] = "Y";
                            dr["PRE_ROUTID"] = dtTrayList.Rows[i]["BF_ROUTE_ID"];
                            dr["AFTER_ROUTID"] = sRoute;
                            dr["AFTER_PROCID"] = sOp;
                            dr["USERID"] = LoginInfo.USERID;
                            dtRequestData.Rows.Add(dr);
                        }

                        FCS002_DataProc FCS002_DataProc = new FCS002_DataProc();
                        FCS002_DataProc.FrameOperation = FrameOperation;

                        object[] parameters = new object[2];
                        parameters[0] = dtRequestData;
                        parameters[1] = "FCS002_005_ROUTE_CHG";
                        this.FrameOperation.OpenMenuFORM("FCS002_DataProc", "FCS002_DataProc", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("FRMDATAPROC"), true, parameters);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
            Close();
        }
        #endregion

        #region Mehod

        #endregion

    }
}
