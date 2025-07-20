/*************************************************************************************
 Created Date : 2023.02.10
      Creator : 강성묵
   Decription : 전지 ESHM GMES 구축
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.27  강성묵 : Initial Created. - 슬리팅 레인 번호 변경
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_UPDATE_LANENO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_UPDATE_LANENO : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _ProcID = string.Empty;
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_COM_UPDATE_LANENO()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 1)
            {
                _ProcID = Util.NVC(tmps[0]);
            }
            
            Loaded -= C1Window_Loaded;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtLOTId.Text != "")
            {
                List<string> liLotId = new List<string>();
                liLotId.Add(txtLOTId.Text);

                SelectLotListChildGrSeqNo(liLotId);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveLaneNo();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLOTId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txtLOTId.Text != "")
            {
                List<string> liLotId = new List<string>();
                liLotId.Add(txtLOTId.Text);

                SelectLotListChildGrSeqNo(liLotId);
            }
        }
        private void dgWaitLot_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
        }
        #endregion

        #region Mehod
        private void SelectLotListChildGrSeqNo(List<string> liLotId)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                //Util.gridClear(dgWaitLot);

                string sBizRuleName = "DA_PRD_SEL_LOT_LIST_CHILD_GR_SEQNO";

                DataTable dtInDataTable = new DataTable();

                dtInDataTable.Columns.Add("LANGID", typeof(string));
                dtInDataTable.Columns.Add("PROCID", typeof(string));
                dtInDataTable.Columns.Add("LOTID", typeof(string));
                dtInDataTable.Columns.Add("CSTID", typeof(string));
                
                foreach (string sLotId in liLotId) {
                    DataRow drInData = dtInDataTable.NewRow();
                    drInData["LANGID"] = LoginInfo.LANGID;
                    drInData["PROCID"] = _ProcID;
                    drInData["LOTID"] = sLotId;
                    drInData["CSTID"] = sLotId;
                    dtInDataTable.Rows.Add(drInData);
                }

                new ClientProxy().ExecuteService(sBizRuleName, "RQSTDT", "RSLTDT", dtInDataTable, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult == null)
                    {
                        return;
                    }

                    // Grid 테이블 컬럼 생성
                    DataTable dtUpdateLneNoLotList = null;

                    if (dgWaitLot.ItemsSource != null)
                    {
                        dtUpdateLneNoLotList = DataTableConverter.Convert(dgWaitLot.ItemsSource);
                    }
                    else
                    {
                        dtUpdateLneNoLotList = new DataTable();

                        dtUpdateLneNoLotList.Columns.Add("LOTID", typeof(string));
                        dtUpdateLneNoLotList.Columns.Add("CSTID", typeof(string));
                        dtUpdateLneNoLotList.Columns.Add("PROCID", typeof(string));
                        dtUpdateLneNoLotList.Columns.Add("PROCNAME", typeof(string));
                        dtUpdateLneNoLotList.Columns.Add("CHILD_GR_SEQNO", typeof(string));
                        dtUpdateLneNoLotList.Columns.Add("AFTER_CHILD_GR_SEQNO", typeof(object));
                        dtUpdateLneNoLotList.Columns.Add("AFTER_CHILD_GR_SEQNO_SEL", typeof(string));
                        dtUpdateLneNoLotList.Columns.Add("RSLT_CODE", typeof(string));
                    }

                    foreach (DataRow drResult in dtResult.Rows)
                    {
                        string sResultLotId = Util.NVC(drResult["LOTID"]);
                        string sResultCstId = Util.NVC(drResult["CSTID"]);
                        string sResultProcId = Util.NVC(drResult["PROCID"]);
                        string sResultProcName = Util.NVC(drResult["PROCNAME"]);
                        string sResultChildGrSeqNo = Util.NVC(drResult["CHILD_GR_SEQNO"]);
                        string sResultAfterChildGrSeqNo = Util.NVC(drResult["AFTER_CHILD_GR_SEQNO"]);
                        string sResultRsltCode = Util.NVC(drResult["RSLT_CODE"]);

                        // LotId 중복 체크
                        DataRow[] drFindLot = dtUpdateLneNoLotList.Select("LOTID = '" + sResultLotId + "'");

                        if (drFindLot == null || drFindLot.Length <= 0)
                        {
                            DataRow drAddWaitLot = dtUpdateLneNoLotList.NewRow();

                            drAddWaitLot["LOTID"] = sResultLotId;
                            drAddWaitLot["CSTID"] = sResultCstId;
                            drAddWaitLot["PROCID"] = sResultProcId;
                            drAddWaitLot["PROCNAME"] = sResultProcName;
                            drAddWaitLot["CHILD_GR_SEQNO"] = sResultChildGrSeqNo;

                            // LaneNo ComboBox Data
                            ObservableCollection<SomeModel> smSomeValues = GetAfterChildGrSeqNp(sResultLotId);
                            drAddWaitLot["AFTER_CHILD_GR_SEQNO"] = smSomeValues;

                            drAddWaitLot["RSLT_CODE"] = sResultRsltCode;

                            dtUpdateLneNoLotList.Rows.Add(drAddWaitLot);
                        }
                    }

                    Util.GridSetData(dgWaitLot, dtUpdateLneNoLotList, null, false);

                    txtLOTId.Text = "";
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private class SomeModel
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private ObservableCollection<SomeModel> GetAfterChildGrSeqNp(string sLotId)
        {
            ObservableCollection<SomeModel> ocSomeValues = new ObservableCollection<SomeModel>();

            DataTable dtInDataTable = new DataTable();

            dtInDataTable.Columns.Add("LANGID", typeof(string));
            dtInDataTable.Columns.Add("SHOPID", typeof(string));
            dtInDataTable.Columns.Add("AREAID", typeof(string));
            dtInDataTable.Columns.Add("LOTID", typeof(string));

            DataRow drInData = dtInDataTable.NewRow();
            drInData["LANGID"] = LoginInfo.LANGID;
            drInData["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drInData["AREAID"] = LoginInfo.CFG_AREA_ID;
            drInData["LOTID"] = sLotId;
            dtInDataTable.Rows.Add(drInData);

            string sBizRuleName = "";

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("A") == true)
            {
                // 조립 슬리팅 레인 번호
                sBizRuleName = "DA_BAS_SEL_TRNS_CONDITION_LANE_BY_LOTID_CBO_ASSY";
            }
            else
            {
                // 전극 슬리팅 레인 번호
                sBizRuleName = "DA_BAS_SEL_TRNS_CONDITION_LANE_BY_LOTID_CBO_ELEC";
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizRuleName, "RQSTDT", "RSLTDT", dtInDataTable);

            if(dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach(DataRow dr in dtResult.Rows)
                {
                    string sCboCode = Util.NVC(dr["CBO_CODE"]);
                    string sCboName = Util.NVC(dr["CBO_NAME"]);

                    SomeModel smLaneNo = new SomeModel();
                    smLaneNo.Name = sCboCode;
                    smLaneNo.Value = sCboName;

                    ocSomeValues.Add(smLaneNo);
                }
            }

            return ocSomeValues;
        }

        private void SaveLaneNo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtInDataTable = new DataTable();
                dtInDataTable.Columns.Add("LOTID", typeof(string));
                dtInDataTable.Columns.Add("CHILD_GR_SEQNO", typeof(string));
                dtInDataTable.Columns.Add("USERID", typeof(string));
                dtInDataTable.Columns.Add("SRCTYPE", typeof(string));
                dtInDataTable.Columns.Add("WIPNOTE", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow drRow in dgWaitLot.Rows)
                {
                    if (drRow.Type != C1.WPF.DataGrid.DataGridRowType.Item)
                    {
                        continue;
                    }

                    DataTableConverter.SetValue(drRow.DataItem, "RSLT_CODE", "");
                }

                foreach (C1.WPF.DataGrid.DataGridRow drRow in dgWaitLot.Rows)
                {
                    if (drRow.Type != C1.WPF.DataGrid.DataGridRowType.Item)
                    {
                        continue;
                    }

                    string slotId = Util.NVC(DataTableConverter.GetValue(drRow.DataItem, "LOTID"));
                    string sChildGrSeqNo = Util.NVC(DataTableConverter.GetValue(drRow.DataItem, "CHILD_GR_SEQNO"));
                    string sAfterChildGrSeqNoSel = Util.NVC(DataTableConverter.GetValue(drRow.DataItem, "AFTER_CHILD_GR_SEQNO_SEL"));
                    string sResult = Util.NVC(DataTableConverter.GetValue(drRow.DataItem, "RSLT_CODE"));

                    if (sChildGrSeqNo.Equals(sAfterChildGrSeqNoSel) == true)
                    {
                        continue;
                    }
                    else if(sAfterChildGrSeqNoSel == "")
                    {
                        continue;
                    }

                    dtInDataTable.Clear();
                    DataRow drParam = dtInDataTable.NewRow();
                    drParam["LOTID"] = slotId;
                    drParam["CHILD_GR_SEQNO"] = sAfterChildGrSeqNoSel;
                    drParam["USERID"] = LoginInfo.USERID;
                    drParam["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drParam["WIPNOTE"] = sChildGrSeqNo + " -> " + sAfterChildGrSeqNoSel;

                    dtInDataTable.Rows.Add(drParam);

                    if (dtInDataTable.Rows.Count < 1)
                    {
                        Util.MessageValidation("MMD0002");      //저장할 데이터가 존재하지 않습니다.
                        return;
                    }

                    new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LOTATTR_LANE_NO", "INDATA", null, dtInDataTable);

                    DataTableConverter.SetValue(drRow.DataItem, "CHILD_GR_SEQNO", sAfterChildGrSeqNoSel);
                    DataTableConverter.SetValue(drRow.DataItem, "RSLT_CODE", ObjectDic.Instance.GetObjectName("Complete"));
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                dgWaitLot.Refresh();
            }
        }
        #endregion

    }
}