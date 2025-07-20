/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.02.16  최경아 : 시생산 CELL과 다른 타입CELL이 혼입불가 및 cell 정보 popup 기능 추가. 




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_300_TRAY_DETL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string sSHOPID = string.Empty;
        string sAREAID = string.Empty;
        string sPALLETID = string.Empty;
        string sTRAYID = string.Empty;
        string sEQSGID = string.Empty;
        string sMDLLOT_ID = string.Empty;
        string sWorker = string.Empty;
        DataTable dtOrigin = new DataTable();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_300_TRAY_DETL()
        {
            InitializeComponent();
            Loaded += BOX001_300_TRAY_DETL_Loaded;
        }

        private void BOX001_300_TRAY_DETL_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_300_TRAY_DETL_Loaded;

            object[] tmps = C1WindowExtension.GetParameters(this);
            sPALLETID = tmps[0] as string;
            sTRAYID = tmps[1] as string;
            decimal iMaxCellQty = Util.NVC_Decimal(tmps[2] as string);
            sSHOPID = tmps[3] as string;
            sAREAID = tmps[4] as string;
            sEQSGID = tmps[5] as string;
            sMDLLOT_ID = tmps[6] as string;
            sWorker = tmps[7] as string;
            getTrayInfo(sPALLETID, sTRAYID, iMaxCellQty);
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //셀 정보 저장 하시겠습니까?
          //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3153"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
          Util.MessageConfirm("SFU3153", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // Cell 정보 변경하는 함수 호출.
                    UpdateCellInformation();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        ///  // 값이 있는 상태라면 변경할 수 없도록 함
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCELLInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (dgCELLInfo.CurrentCell != null && dgCELLInfo.SelectedIndex > -1)
            {
                if (dgCELLInfo.CurrentCell.Column.Name == "CELLID")
                {
                    string sTmpCell = Util.NVC(dgCELLInfo.GetCell(dgCELLInfo.CurrentRow.Index, dgCELLInfo.Columns["CELLID"].Index).Value);
                    if (sTmpCell == string.Empty)
                    {
                        e.Cancel = false;   // Editing 가능
                        DataTableConverter.SetValue(dgCELLInfo.Rows[dgCELLInfo.CurrentRow.Index].DataItem, "ADD_YN", "Y");
                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
                else if (dgCELLInfo.CurrentCell.Column.Name == "CELLID2")
                {
                    string sTmpCell = Util.NVC(dgCELLInfo.GetCell(dgCELLInfo.CurrentRow.Index, dgCELLInfo.Columns["CELLID2"].Index).Value);
                    if (sTmpCell == string.Empty)
                    {
                        e.Cancel = false;   // Editing 가능
                        DataTableConverter.SetValue(dgCELLInfo.Rows[dgCELLInfo.CurrentRow.Index].DataItem, "ADD_YN2", "Y");
                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
            }
        }

        private void dgCELLInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgCELLInfo.CurrentCell != null && dgCELLInfo.SelectedIndex > -1 && e.ChangedButton.ToString().Equals("Left"))
            {
                if (dgCELLInfo.CurrentCell.Column.Name == "CELLID")
                {
                    string sCellid = Util.NVC(dgCELLInfo.GetCell(dgCELLInfo.CurrentRow.Index, dgCELLInfo.Columns["CELLID"].Index).Value);
                    if (sCellid == "")
                    {
                        //commMessage.Show("CellID가 입력되지 ");
                        return;
                    }
                    else if (sCellid.Length != 10)
                    {
                        DataTableConverter.SetValue(dgCELLInfo.Rows[dgCELLInfo.CurrentRow.Index].DataItem, "CELLID", "");
                        Util.MessageValidation("SFU1322");//"CellID는 10자리로 지정되어 있습니다."
                        return;
                    }

                    // 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수 호출
                    //FormationDBLink(sCellid);
                }
                else if (dgCELLInfo.CurrentCell.Column.Name == "CELLID2")
                {
                    string sCellid = Util.NVC(dgCELLInfo.GetCell(dgCELLInfo.CurrentRow.Index, dgCELLInfo.Columns["CELLID2"].Index).Value);
                    if (sCellid == "")
                    {
                        //commMessage.Show("CellID가 입력되지 ");
                        return;
                    }
                    else if (sCellid.Length != 10)
                    {
                        DataTableConverter.SetValue(dgCELLInfo.Rows[dgCELLInfo.CurrentRow.Index].DataItem, "CELLID2", "");
                        Util.MessageValidation("SFU1322");//"CellID는 10자리로 지정되어 있습니다."
                        return;
                    }

                    // 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수 호출
                    //FormationDBLink(sCellid);
                }

            }
        }


        private void dgCELLInfo_CommittedRowEdit(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            int eRow = e.Row.Index;
            if (e.Row.Index > -1)
            {
                if (dgCELLInfo.CurrentCell.Column.Name == "CELLID")
                {
                    string sCellid = Util.NVC(dgCELLInfo.GetCell(e.Row.Index, dgCELLInfo.Columns["CELLID"].Index).Value);
                    if (sCellid == "")
                    {
                        //commMessage.Show("CellID가 입력되지 ");
                        return;
                    }
                    else if (sCellid.Length != 10)
                    {
                        DataTableConverter.SetValue(dgCELLInfo.Rows[e.Row.Index].DataItem, "CELLID", "");
                        Util.MessageValidation("SFU1322");//"CellID는 10자리로 지정되어 있습니다."
                        return;
                    }

                    // 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수 호출
                    FormationDBLink(sCellid, eRow, "CELLID");
                }
                else if (dgCELLInfo.CurrentCell.Column.Name == "CELLID2")
                {
                    string sCellid = Util.NVC(dgCELLInfo.GetCell(e.Row.Index, dgCELLInfo.Columns["CELLID2"].Index).Value);
                    if (sCellid == "")
                    {
                        //commMessage.Show("CellID가 입력되지 ");
                        return;
                    }
                    else if (sCellid.Length != 10)
                    {
                        DataTableConverter.SetValue(dgCELLInfo.Rows[e.Row.Index].DataItem, "CELLID2", "");
                        Util.MessageValidation("SFU1322");//"CellID는 10자리로 지정되어 있습니다."
                        return;
                    }

                    // 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수 호출
                    FormationDBLink(sCellid, eRow, "CELLID2");
                }
            }
        }


        #endregion


        #region Mehod

        /// <summary>
        /// Tray 상세 조회
        /// </summary>
        /// <param name="dataItem"></param>
        private void getTrayInfo(string sPalletID, string sLotid, decimal iMaxCellQty)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("TRAYID", typeof(string));
                RQSTDT.Columns.Add("MAX_CELLQTY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TRAYID"] = sLotid;
                dr["MAX_CELLQTY"] = iMaxCellQty;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_DETL_CP", "RQSTDT", "RSLTDT", RQSTDT);
                dtOrigin = dtResult.Copy();

                //dgCELLInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgCELLInfo, dtResult, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수
        /// </summary>
        private void FormationDBLink(string sCellid, int eRow, string coNm)
        {
            int intoCell = 0;
            for (int iRow = 0; iRow < dtOrigin.Rows.Count; iRow++)
            {   
                if(coNm == "CELLID")
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dtOrigin.Rows[eRow]["CELLID"])) && Util.NVC(dtOrigin.Rows[iRow]["CELLID"]) == sCellid)
                    {
                        intoCell += 1;
                    }
                }
                if (coNm == "CELLID2")
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dtOrigin.Rows[eRow]["CELLID2"])) && Util.NVC(dtOrigin.Rows[iRow]["CELLID2"]) == sCellid)
                    {
                        intoCell += 1;
                    }
                }
            }
            if (intoCell > 0)
            {
                return;
            }

            string cellList = string.Empty;
            DataTable dt = DataTableConverter.Convert(dgCELLInfo.ItemsSource);
            for (int row = 0; row < dt.Rows.Count; row++)
            {
                cellList += "," + dt.Rows[row]["CELLID"].ToString();
            }
            for (int row = 0; row < dt.Rows.Count; row++)
            {
                cellList += "," + dt.Rows[row]["CELLID2"].ToString();
            }

            DataTable cellL = new DataTable();
            cellL.Columns.Add("LANGID");
            cellL.Columns.Add("CELL_LIST");

            DataRow cdr = cellL.NewRow();
            cdr["LANGID"] = LoginInfo.LANGID;
            cdr["CELL_LIST"] = cellList;
            cellL.Rows.Add(cdr);

            DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_PILOT_LIST_UI_BX", "INDATA", "OUTDATA", cellL); // 시생산 cell 조회
            DataTable dnResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_NOT_PILOT_LIST_UI_BX", "INDATA", "OUTDATA", cellL); // 시생산 아닌 cell 조회

            if (dnResult.Rows.Count > 0 && dsResult.Rows.Count > 0) // 시생산 cell과 시생산 아닌 cell 모두 존재하는 경우
            {
                if (dgCELLInfo.CurrentCell.Column.Name == "CELLID")
                {
                    DataTableConverter.SetValue(dgCELLInfo.Rows[eRow].DataItem, "CELLID", "");
                }
                else if (dgCELLInfo.CurrentCell.Column.Name == "CELLID2")
                {
                    DataTableConverter.SetValue(dgCELLInfo.Rows[eRow].DataItem, "CELLID2", "");
                }
                // 시생산 cell 있는 경우 popup
                BOX001_PILOT_DETL pilot_popUp = new BOX001_PILOT_DETL();
                pilot_popUp.FrameOperation = this.FrameOperation;

                if (pilot_popUp != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = dsResult;
                    Parameters[1] = dnResult;

                    C1WindowExtension.SetParameters(pilot_popUp, Parameters);

                    pilot_popUp.Closed += new EventHandler(pilot_wndConfirm_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => pilot_popUp.ShowModal()));
                    pilot_popUp.BringToFront();
                }

                return;
            }

            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.Columns.Add("CELLID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["CELLID"] = sCellid;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtRslt = null;
                //if (sAREAID.Equals("A1"))
                //{
                //    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO", "INDATA", "OUTDATA", RQSTDT);
                //}
                //else if (sAREAID.Equals("A2") || sAREAID.Equals("S2"))
                //{
                //    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO", "INDATA", "OUTDATA", RQSTDT);
                //}
                //else
                //{
                //    return;
                //}

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellid;
                dr["SHOPID"] = sSHOPID;
                dr["AREAID"] = sAREAID;
                dr["EQSGID"] = sEQSGID;
                dr["MDLLOT_ID"] = sMDLLOT_ID;
                dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                dr["INSP_SKIP_FLAG"] = "N";
                dr["2D_BCR_SKIP_FLAG"] = "Y";
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);


                // ClientProxy2007
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_CHK_FORM_DATA_VALIDATION_BX", "INDATA", "OUTDATA", RQSTDT);

            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                Util.MessageException(ex);
                if (dgCELLInfo.CurrentCell.Column.Name == "CELLID")
                {
                    DataTableConverter.SetValue(dgCELLInfo.Rows[eRow].DataItem, "CELLID", "");
                }
                else if (dgCELLInfo.CurrentCell.Column.Name == "CELLID2")
                {
                    DataTableConverter.SetValue(dgCELLInfo.Rows[eRow].DataItem, "CELLID2", "");
                }
                return;
            }
        }

        private void pilot_wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_PILOT_DETL pilot_popup = sender as BOX001_PILOT_DETL;

            this.grdMain.Children.Remove(pilot_popup);
        }

        /// <summary>
        /// Cell 정보 저장
        /// </summary>
        private void UpdateCellInformation()
        {
            int iTotalQty = 0;
            int lsSpreadCount = 0;
            // 현재 보이는 스프레드의 셀 ID가 있는 컬럼이 몇 개인지 카운트함.
            for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
            {
                if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID"].Index).Value) != "")
                {
                    iTotalQty = iTotalQty + 1;
                    if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["ADD_YN"].Index).Value) == "Y")
                    {
                        lsSpreadCount = lsSpreadCount + 1;
                    }
                }

                if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID2"].Index).Value) != "")
                {
                    iTotalQty = iTotalQty + 1;
                    if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["ADD_YN2"].Index).Value) == "Y")
                    {
                        lsSpreadCount = lsSpreadCount + 1;
                    }
                }

            }

            if (lsSpreadCount == 0)
            {
                //저장할 정보가 없습니다. 확인 후 저장하시길 바랍니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0002"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            try
            {
                //BizData data = new BizData("GMTRAY_CELL_SAVE");

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("MODELID", typeof(string));

                DataTable inPalletTable = indataSet.Tables.Add("IN_PALLET");
                inPalletTable.Columns.Add("PALLETID", typeof(string));
                inPalletTable.Columns.Add("BOXID", typeof(string));
                inPalletTable.Columns.Add("PACKING_QTY", typeof(int));

                DataTable inDataSubTable = indataSet.Tables.Add("IN_BOX");
                inDataSubTable.Columns.Add("PSTN_NO", typeof(int));
                inDataSubTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["SRCTYPE"] = "UI";
                inData["IFMODE"] = "OFF";
                inData["USERID"] = LoginInfo.USERID;
                inData["MODELID"] = sMDLLOT_ID;
                inDataTable.Rows.Add(inData);

                DataRow inPallet = inPalletTable.NewRow();
                inPallet["PALLETID"] = sPALLETID;
                inPallet["BOXID"] = sTRAYID;
                inPallet["PACKING_QTY"] = iTotalQty;
                inPalletTable.Rows.Add(inPallet);

                for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID"].Index).Value) != "" && Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["ADD_YN"].Index).Value) == "Y")
                    {
                        DataRow inDataSub = inDataSubTable.NewRow();
                        inDataSub["PSTN_NO"] = Int32.Parse(Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["SEQ_NO"].Index).Value));
                        inDataSub["SUBLOTID"] = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID"].Index).Value);
                        inDataSubTable.Rows.Add(inDataSub);
                        FormationDBLink(Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID"].Index).Value), i, "SAVE");
                    }

                    if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID2"].Index).Value) != "" && Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["ADD_YN2"].Index).Value) == "Y")
                    {
                        DataRow inDataSub = inDataSubTable.NewRow();
                        inDataSub["PSTN_NO"] = Int32.Parse(Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["SEQ_NO2"].Index).Value));
                        inDataSub["SUBLOTID"] = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID2"].Index).Value);
                        inDataSubTable.Rows.Add(inDataSub);
                        FormationDBLink(Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID2"].Index).Value), i, "SAVE");
                    }

                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_PACKING_BY_MANUAL", "IN_EQP, IN_PALLET, IN_BOX", null, indataSet);
                Util.AlertInfo("SFU1275"); //"정상 처리 되었습니다."

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                //Util.AlertByBiz("BR_PRD_REG_PACKING_BY_MANUAL_CP", ex.Message, ex.ToString());
            }
        }

        #endregion
    }
}
