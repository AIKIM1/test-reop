/*************************************************************************************
 Created Date : 2016.11.22
      Creator : 김광호
   Decription : 전극창고 출고대상 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.22  DEVELOPER : Initial Created.



 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_018 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        string sShopID = LoginInfo.CFG_AREA_ID;

        public ELEC001_018()
        {
            InitializeComponent();
            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, cbChild: cboEquipmentSegmentChild, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbParent: cboProcessParent, sCase: "PROCESS");
            
            // 창고명
            _combo.SetCombo(cboElecWareHouse, CommonCombo.ComboStatus.NONE, sFilter: sFilter1);
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sProc = cboProcess.SelectedValue.ToString();
            if (sProc == "")
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                cboProcess.Focus();
                return;
            }
            dgData2.ItemsSource = null;
            dgData3.ItemsSource = null;
            GetData1(sProc);
        }

        private void dgData1_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            if (dgData1.GetRowCount() < 1)
                return;

            if (dgData1.CurrentRow == null)
                return;

            string sWo = Util.NVC(dgData1.GetCell(dgData1.CurrentRow.Index, dgData1.Columns["WO_DETL_ID"].Index).Value);
            string sProcid = Util.NVC(dgData1.GetCell(dgData1.CurrentRow.Index, dgData1.Columns["PROCID"].Index).Value);
            string sEqptid = Util.NVC(dgData1.GetCell(dgData1.CurrentRow.Index, dgData1.Columns["EQPTID"].Index).Value);
            string sProdid = Util.NVC(dgData1.GetCell(dgData1.CurrentRow.Index, dgData1.Columns["PRODID"].Index).Value);
            string sWHid = cboElecWareHouse.SelectedValue.ToString();
            string sDemand = Util.NVC(dgData1.GetCell(dgData1.CurrentRow.Index, dgData1.Columns["DEMAND_TYPE"].Index).Value);

            GetData2(sWo, sProcid, sEqptid);
            GetData3(sWHid, sProdid, sProcid, sDemand);
        }
        #endregion

        #region Method
        private void GetData1(string sProc)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sProc;
                dr["AREAID"] = sShopID;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_FOR_TARGET_OUT", "RQSTDT", "RSLTDT", RQSTDT);

                dgData1.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetData2(string sWo, string sProcid, string sEqptid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("WO_DETL_ID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WO_DETL_ID"] = sWo;
                dr["PROCID"] = sProcid;
                dr["EQPTID"] = sEqptid;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NEXT_WO_FOR_TARGET_OUT", "RQSTDT", "RSLTDT", RQSTDT);

                dgData2.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetData3(string sWHid, string sProdid, string sProcid, string sDemand)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("DEMAND_TYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WH_ID"] = sWHid;
                dr["PRODID"] = sProdid;
                dr["PROCID"] = sProcid;
                dr["DEMAND_TYPE"] = sDemand;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WH_FOR_TARGET_OUT", "RQSTDT", "RSLTDT", RQSTDT);

                dgData3.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

    }
}
