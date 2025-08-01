﻿/*************************************************************************************
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
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_005_TRAY_DETL_NEW : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        string sSHOPID = string.Empty;
        string sAREAID = string.Empty;
        string sPALLETID = string.Empty;
        string sTRAYID = string.Empty;
        string sEQSGID = string.Empty;
        string sMDLLOT_ID = string.Empty;
        string sWorker = string.Empty;

        bool bEdit = false;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_005_TRAY_DETL_NEW()
        {
            InitializeComponent();
            Loaded += BOX001_005_TRAY_DETL_NEW_Loaded;
        }

        private void BOX001_005_TRAY_DETL_NEW_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_005_TRAY_DETL_NEW_Loaded;

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
           // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3153"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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
            bEdit = false;

            if (dgCELLInfo.CurrentCell != null && dgCELLInfo.SelectedIndex > -1)
            {
                if (dgCELLInfo.CurrentCell.Column.Name == "CELLID")
                {
                    string sTmpCell = Util.NVC(dgCELLInfo.GetCell(dgCELLInfo.CurrentRow.Index, dgCELLInfo.Columns["CELLID"].Index).Value);
                    if (sTmpCell == string.Empty)
                    {
                        e.Cancel = false;   // Editing 가능
                        bEdit = true;
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
                        bEdit = true;
                        DataTableConverter.SetValue(dgCELLInfo.Rows[dgCELLInfo.CurrentRow.Index].DataItem, "ADD_YN2", "Y");
                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
                else if (dgCELLInfo.CurrentCell.Column.Name == "CELLID3")
                {
                    string sTmpCell = Util.NVC(dgCELLInfo.GetCell(dgCELLInfo.CurrentRow.Index, dgCELLInfo.Columns["CELLID3"].Index).Value);
                    if (sTmpCell == string.Empty)
                    {
                        e.Cancel = false;   // Editing 가능
                        bEdit = true;
                        DataTableConverter.SetValue(dgCELLInfo.Rows[dgCELLInfo.CurrentRow.Index].DataItem, "ADD_YN3", "Y");
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
                        Util.MessageValidation("SFU1322");//"CellID는 10자리로 지정되어 있습니다."//"CellID는 10자리로 지정되어 있습니다."
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
                else if (dgCELLInfo.CurrentCell.Column.Name == "CELLID3")
                {
                    string sCellid = Util.NVC(dgCELLInfo.GetCell(dgCELLInfo.CurrentRow.Index, dgCELLInfo.Columns["CELLID3"].Index).Value);
                    if (sCellid == "")
                    {
                        //commMessage.Show("CellID가 입력되지 ");
                        return;
                    }
                    else if (sCellid.Length != 10)
                    {
                        DataTableConverter.SetValue(dgCELLInfo.Rows[dgCELLInfo.CurrentRow.Index].DataItem, "CELLID3", "");
                        Util.MessageValidation("SFU1322");//"CellID는 10자리로 지정되어 있습니다."
                        return;
                    }

                    // 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수 호출
                    FormationDBLink(sCellid);
                }
            }
        }


        private void dgCELLInfo_CommittedRowEdit(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
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
                        if (!FormationDBLink(sCellid))
                        {
                            if (bEdit)
                                DataTableConverter.SetValue(dgCELLInfo.Rows[e.Row.Index].DataItem, "CELLID", "");

                            return;
                        }
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
                        if (!FormationDBLink(sCellid))
                        {
                            if (bEdit)
                                DataTableConverter.SetValue(dgCELLInfo.Rows[e.Row.Index].DataItem, "CELLID2", "");

                            return;
                        }
                    }
                    else if (dgCELLInfo.CurrentCell.Column.Name == "CELLID3")
                    {
                        string sCellid = Util.NVC(dgCELLInfo.GetCell(e.Row.Index, dgCELLInfo.Columns["CELLID3"].Index).Value);
                        if (sCellid == "")
                        {
                            //commMessage.Show("CellID가 입력되지 ");
                            return;
                        }
                        else if (sCellid.Length != 10)
                        {
                            DataTableConverter.SetValue(dgCELLInfo.Rows[e.Row.Index].DataItem, "CELLID3", "");
                            Util.MessageValidation("SFU1322");//"CellID는 10자리로 지정되어 있습니다."
                            return;
                        }

                        // 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수 호출
                        if (!FormationDBLink(sCellid))
                        {
                            if (bEdit)
                                DataTableConverter.SetValue(dgCELLInfo.Rows[e.Row.Index].DataItem, "CELLID3", "");

                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                bEdit = false;
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
                //dgCELLInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgCELLInfo, dtResult, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수
        /// </summary>
        private bool FormationDBLink(string sCellid)
        {

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

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellid;
                dr["SHOPID"] = sSHOPID;
                dr["AREAID"] = sAREAID;
                dr["EQSGID"] = sEQSGID;
                dr["MDLLOT_ID"] = sMDLLOT_ID;
                dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                dr["INSP_SKIP_FLAG"] = "N";
                dr["2D_BCR_SKIP_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);


                // ClientProxy2007
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", RQSTDT);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
              //  Util.AlertInfo(ex.Message);
                return false;
            }
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

                if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID3"].Index).Value) != "")
                {
                    iTotalQty = iTotalQty + 1;
                    if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["ADD_YN3"].Index).Value) == "Y")
                    {
                        lsSpreadCount = lsSpreadCount + 1;
                    }
                }
            }

            if (lsSpreadCount == 0)
            {
                //저장할 정보가 없습니다. 확인 후 저장하시길 바랍니다.
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0002"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageValidation("MMD0002");
                return;
            }

            try
            {
                //BizData data = new BizData("GMTRAY_CELL_SAVE");

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("PALLETID", typeof(string));
                inDataTable.Columns.Add("TRAYID", typeof(string));
                inDataTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inDataSubTable = indataSet.Tables.Add("INSUBLOT");
                inDataSubTable.Columns.Add("PACKSEQ", typeof(decimal));
                inDataSubTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["SRCTYPE"] = "UI";
                inData["PALLETID"] = sPALLETID;
                inData["TRAYID"] = sTRAYID;
                inData["TOTAL_QTY"] = iTotalQty;
                inData["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inData);

                for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID"].Index).Value) != "" && Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["ADD_YN"].Index).Value) == "Y")
                    {
                        DataRow inDataSub = inDataSubTable.NewRow();
                        inDataSub["PACKSEQ"] = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["SEQ_NO"].Index).Value);
                        inDataSub["SUBLOTID"] = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID"].Index).Value);
                        inDataSubTable.Rows.Add(inDataSub);

                        FormationDBLink(Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID"].Index).Value));
                    }

                    if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID2"].Index).Value) != "" && Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["ADD_YN2"].Index).Value) == "Y")
                    {
                        DataRow inDataSub = inDataSubTable.NewRow();
                        inDataSub["PACKSEQ"] = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["SEQ_NO2"].Index).Value);
                        inDataSub["SUBLOTID"] = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID2"].Index).Value);
                        inDataSubTable.Rows.Add(inDataSub);

                        FormationDBLink(Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID2"].Index).Value));
                    }

                    if (Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID3"].Index).Value) != "" && Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["ADD_YN3"].Index).Value) == "Y")
                    {
                        DataRow inDataSub = inDataSubTable.NewRow();
                        inDataSub["PACKSEQ"] = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["SEQ_NO3"].Index).Value);
                        inDataSub["SUBLOTID"] = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID3"].Index).Value);
                        inDataSubTable.Rows.Add(inDataSub);

                        FormationDBLink(Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["CELLID3"].Index).Value));
                    }
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACKING_BY_MANUAL_CP", "INDATA,INSUBLOT", null, indataSet);
                Util.MessageInfo("SFU1275"); //"정상 처리 되었습니다."

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
              //  Util.AlertByBiz("BR_PRD_REG_PACKING_BY_MANUAL_CP", ex.Message, ex.ToString());
            }

        }



        #endregion

        
    }
}
