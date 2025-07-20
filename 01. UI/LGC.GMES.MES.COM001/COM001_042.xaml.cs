/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_042 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_042()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            InitCombo();
            SetPageAuth();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        private void SetPageAuth()
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHold = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChildHold = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChildHold, cbParent: cboEquipmentSegmentParentHold, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHold = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParentHold, sCase: "PROCESS");


            //그리드 콤보
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            DataRow newRow = null;

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "Y", "Y" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "N", "N" };
            dt.Rows.Add(newRow);

            (dgList.Columns["USE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetEqptList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Save();
                            }
                        });


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }




        #endregion

        #region Mehod

        #region [작업대상 가져오기]
        public void GetEqptList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "동을선택하세요");
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요");
                if (dr["EQSGID"].Equals("")) return;
                dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요");
                if (dr["PROCID"].Equals("")) return;


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_TEST_MODE", "INDATA", "OUTDATA", dtRqst);

                //DataSet inData = new DataSet();

                ////마스터 정보
                //DataTable inDataTable = inData.Tables.Add("IN_EQP");
                //DataTable inDataTable1 = inData.Tables.Add("IN_INPUT");

                //DataSet dtRslt1 = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_CT", "", "IN_EQP,IN_INPUT", inData);

                Util.GridSetData(dgList, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion


        #region [저장 처리]
        private void Save()
        {
            try
            {
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("STRT_DTTM", typeof(DateTime));
                inDataTable.Columns.Add("END_DTTM", typeof(DateTime));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;

                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        row = inDataTable.NewRow();
                        row["EQPTID"] = DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID");
                        row["STRT_DTTM"] = DataTableConverter.GetValue(dgList.Rows[i].DataItem, "STRT_DTTM");
                        row["END_DTTM"] = DataTableConverter.GetValue(dgList.Rows[i].DataItem, "END_DTTM");
                        row["USE_FLAG"] = DataTableConverter.GetValue(dgList.Rows[i].DataItem, "USE_FLAG");
                        row["NOTE"] = DataTableConverter.GetValue(dgList.Rows[i].DataItem, "NOTE");
                        row["USERID"] = LoginInfo.USERID;

                        inDataTable.Rows.Add(row);
                    }
                }

                if (inData.Tables[0].Rows.Count == 0)
                {
                    Util.AlertInfo("10008");    //선택된 데이터가 없습니다.
                    return;
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_EQPT_REG_TEST_MODE", "INDATA", null, inData);

                Util.AlertInfo("SFU1270");  //저장되었습니다.

                GetEqptList();
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.ToString());
            }
        }
        #endregion


        #endregion


    }
}
