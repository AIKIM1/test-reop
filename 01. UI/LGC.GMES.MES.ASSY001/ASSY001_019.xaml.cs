/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_019 : UserControl
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public ASSY001_019()
        {
            InitializeComponent();
            InitCombo();

        }




        #endregion

        #region Initialize
        //화면내 combo 셋팅
        private void InitCombo()
        {


            //동,라인,공정,설비 셋팅

            CommonCombo _combo = new CMM001.Class.CommonCombo();

            //라인
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            ////공정
            ////C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            ////_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);

        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string sEqsgid = Util.NVC(cboEquipmentSegment.SelectedValue);
                if (sEqsgid == "" || sEqsgid == "SELECT")
                {
                    Util.AlertInfo("라인을 선택해 주세요");
                    return;
                }

                string sProcid = Util.NVC(cboProcess.SelectedValue);
                if (sProcid == "" || sProcid == "SELECT")
                {
                    Util.AlertInfo("공정을 선택해 주세요");
                    return;
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (txtLotId.Text.Trim() == String.Empty)
                {
                    dr["EQSGID"] = sEqsgid;
                    dr["PROCID"] = sProcid;
                    dr["PRODID"] = null;
                }
                else
                {
                    dr["LOTID"] = txtLotId.Text.Trim();
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_NOTCHING_ENDLOT", "RQSTDT", "RSLTDT", RQSTDT);
                dgLotList.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dgLotList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgLotList.CurrentRow == null || dgLotList.SelectedIndex == -1) return;

                if (e.ChangedButton.ToString().Equals("Left") && dgLotList.CurrentColumn.Name == "CHK")
                {
                    _Util.gridSelectionSingle(dgLotList, "CHK", e);

                    dgLotList.CurrentRow = null;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnRe_Click(object sender, RoutedEventArgs e)
        {
            int iSelCnt = 0;
            string sWipStat = string.Empty;

            try
            {
                for (int i = 0; i < dgLotList.Rows.Count; i++)
                {
                    if (Util.NVC(dgLotList.GetCell(i, dgLotList.Columns["CHK"].Index).Value) == "1")
                    {
                        iSelCnt = iSelCnt + 1;
                        if (Util.NVC(dgLotList.GetCell(i, dgLotList.Columns["WIPSTAT"].Index).Value) == "TERMINATE")
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("TERMINATE된 LOT이 선택되어 복구할 수 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                            return;
                        }
                    }
                }

                if (iSelCnt == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("복구할 LOT을 선택하신 후에 다시 진행하시길 바랍니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    return;
                }


                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 LOT을 복구하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // 복구 처리
                        InLOT_Restore();
                    }
                });

            }
            catch (Exception ex)
            {

            }

            //try
            //{
            //    if (GridUtil.GetCheckedCellCount(spdMList, 1) > 0)
            //    {
            //        string sUserID = "";
            //        if (!MLBUtil.Confirm("복구 하시겠습니까?", out sUserID)) return;

            //        ArrayList columnIndex = GridUtil.GetCheckedRowsIndex(spdMList, 1);

            //        BizData data = new BizData("DO_LOT_RESURRECT");
            //        data.AddTable("RQSTDT");
            //        data.AddColumn("RQSTDT", "I_LOTID", typeof(string));
            //        data.AddColumn("RQSTDT", "I_PROCID", typeof(string));
            //        data.AddColumn("RQSTDT", "I_QTY", typeof(decimal));
            //        data.AddColumn("RQSTDT", "I_NOTE", typeof(string));
            //        data.AddColumn("RQSTDT", "I_USERID", typeof(string));
            //        data.AddColumn("RQSTDT", "I_CRDATE", typeof(DateTime));

            //        foreach (int i in columnIndex)
            //        {
            //            string lotStatus = spdMList_Sheet1.Cells.Get(i, 15).Value.ToString();   // 상태
            //            int lqty = ConvertUtil.ToInt32(spdMList_Sheet1.Cells.Get(i, 10).Value);

            //            if (!string.Equals(lotStatus, "TERMINATE"))
            //            {
            //                if (lqty <= 0)
            //                {
            //                    commMessage.Show(spdMList_Sheet1.Cells.Get(i, 2).Value.ToString() + " Lot의 수량을 0을 입력하였습니다. 0은 반영되지 않습니다.");
            //                    return;
            //                }
            //                else
            //                {
            //                    data.AddRow("RQSTDT");
            //                    data.SetData("RQSTDT", "I_LOTID", spdMList_Sheet1.Cells.Get(i, 2).Value.ToString());
            //                    data.SetData("RQSTDT", "I_PROCID", spdMList_Sheet1.Cells.Get(i, 8).Value.ToString()); // 7
            //                    data.SetData("RQSTDT", "I_QTY", ConvertUtil.ToDecimal(spdMList_Sheet1.Cells.Get(i, 10).Value)); // 9
            //                    data.SetData("RQSTDT", "I_NOTE", txtBigo.Text.Trim()); // 비고
            //                    data.SetData("RQSTDT", "I_USERID", sUserID); // 작업자
            //                    data.SetData("RQSTDT", "I_CRDATE", DateTime.Now); // SYSDATE                                
            //                }
            //            }
            //            else
            //            {
            //                commMessage.Show(spdMList_Sheet1.Cells.Get(i, 2).Value.ToString() + " Lot은 TERMINATE된 상태이기에 복구할 수 없습니다.");
            //                return;
            //            }
            //        }
            //        data.Submit();
            //        MessageBox.Show("정상처리 되었습니다.");

            //        // 조회 로직 수행하는 함수 호출
            //        SelectProcess();
            //    }
            //    else
            //    {
            //        commMessage.Show("복구할 LotID를 선택하신 후에 다시 진행하시길 바랍니다.");
            //        return;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    commMessage.Show(ex.Message);
            //    return;
            //}

        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sEqsgid = Util.NVC(cboEquipmentSegment.SelectedValue);
            if (sEqsgid == "" || sEqsgid == "SELECT")
            {
                sEqsgid = null;
            }

            SetProcess(cboProcess, sEqsgid);

        }

        #endregion

        #region Mehod

        /// <summary>
        /// 투입LOT 복구 처리 
        /// </summary>
        private void InLOT_Restore()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = "";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("???", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        private void SetProcess(C1ComboBox cbo, string sEqsgid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEqsgid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_NC_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow dr2 = dtResult.NewRow();
                dr2["CBO_NAME"] = "-SELECT-";
                dr2["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(dr2, 0);
                cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion
    }
}