/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_020 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        #region Declaration & Constructor 
        public ASSY001_020()
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
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            //dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            //dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo combo = new CommonCombo();

            //라인
            string[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: null, sFilter:sFilter1, sCase: "EQUIPMENTSEGMENT");

            if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0; //config 선택말고 전체를 기본으로 하기

            txtToEquipmentSegment.Text = LoginInfo.CFG_EQSG_NAME;

        }

        #endregion

        #region Event
        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgMoveList, "CHK", i)) continue;

                    DataSet indataSet = _Biz.GetBR_PRD_REG_DISPATCH_LOT_LM();
                    DataTable inTable = indataSet.Tables["INDATA"];

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    //newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "EQPTID"));
                    //newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                    //newRow["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "EQSGID"));
                    newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;// Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "EQSGID"));
                    newRow["REWORK"] = "N";
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                    newRow = null;

                    DataTable inDataTable = indataSet.Tables["INLOT"];
                    newRow = inDataTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID"));
                    newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY")));
                    newRow["ACTUQTY"] = 0;
                    newRow["WIPNOTE"] = "";

                    inDataTable.Rows.Add(newRow);
                

                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                Search_data();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;             
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_data();
        }

        private void Search_data()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(String));
                dtRqst.Columns.Add("PROCID", typeof(String));
                dtRqst.Columns.Add("EQSGID", typeof(String));
                dtRqst.Columns.Add("PRODID", typeof(String));
                dtRqst.Columns.Add("LOTID", typeof(String));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = Process.LAMINATION; //라미공정
               
                dtRqst.Rows.Add(dr);

                if (Util.GetCondition(txtLotId).Equals("")) //lot id 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull:true);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = Util.GetCondition(txtLotId);
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_END_LOT_LIST", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgMoveList, dtRslt, FrameOperation);

                txtLotId.Text = "";
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

      
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
      
            Util.gridClear(dgMoveList);
        }


        #endregion

        #region Mehod

        #endregion

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sLotid = txtLotId.Text.ToString().Trim();

                if (sLotid == "" || sLotid == null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("LOT ID 가 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                Search_data();
            }
        }
    }
}

