/*************************************************************************************
 Created Date : 2017.01.16
      Creator : 김재호 부장
   Decription : SKID BUFFER 모니터링
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using LGC.GMES.MES.MCS001.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_001_PANCAKE_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_001_PANCAKE_INFO : C1Window, IWorkArea
    {

        // 창고 ID - 폴란드 Skid 창고
        private const string WH_ID = "A5A102";
        public string sRACK_ID;
        private string sPRJT_NAME;
        private string sMCS_CST_ID;
        private string sZONE;

        private string sSPCL_FLAG;


        private bool bIsEmptySKID = false;

        public MCS001_001_PANCAKE_INFO() {
            InitializeComponent();

            IsUpdated = false;
        }

        public IFrameOperation FrameOperation {
            get;

            set;
        }


        private void OnC1WindowLoaded(object sender, RoutedEventArgs e)
        {           
            this.InitGrid();

            this.InitCombo();
        }

        private void InitGrid()
        {
            object[] Parameters = C1WindowExtension.GetParameters(this);
            if (Parameters != null && Parameters.Length != 0)
            {
                sZONE = Parameters[1].ToString();

                SkidRack rack = (SkidRack)Parameters[0];

                txtPancakeRow.Text = rack.Row.ToString();
                txtPancakeColumn.Text = rack.Col.ToString();
                txtPancakeStair.Text = rack.Stair.ToString();
                txtZoneIed.Text = rack.ZoneId;

                sRACK_ID = rack.RackId;
                sPRJT_NAME = rack.ProjectName;
                sMCS_CST_ID = rack.SkidID;

                sSPCL_FLAG = rack.Spcl_Flag;

                if (sMCS_CST_ID.Length == 0)
                {
                    rdoSKID.IsEnabled = true;
                    rdoDeleteRackInfo.IsEnabled = true;
                    rdoLock.IsEnabled = true;
                    rdoUnlock.IsEnabled = true;

                    ChkSPCL.Visibility = Visibility.Hidden;
                }
                else if (sMCS_CST_ID == "CHECK")
                {
                    rdoSKID.IsEnabled = false;
                    rdoDeleteRackInfo.IsEnabled = true;
                    rdoLock.IsEnabled = true;
                    rdoUnlock.IsEnabled = true;
                }
                else if (sMCS_CST_ID == "UNUSE")
                {
                    rdoSKID.IsEnabled = false;
                    rdoDeleteRackInfo.IsEnabled = true;
                    rdoLock.IsEnabled = true;
                    rdoUnlock.IsEnabled = true;
                }
                else
                {
                    rdoSKID.IsEnabled = false;
                    rdoDeleteRackInfo.IsEnabled = true;
                    rdoLock.IsEnabled = false;
                    rdoUnlock.IsEnabled = false;

                    ChkSPCL.Visibility = Visibility.Visible;
                }


                if (sSPCL_FLAG == "Y")
                {
                    rdoUndoSPCL.Visibility = Visibility.Visible;
                }
                else
                {
                    rdoUndoSPCL.Visibility = Visibility.Hidden;
                }


                

                DataTable dt = new DataTable();
                dt.Columns.Add("RACK_ID", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));
                dt.Columns.Add("MCS_CST_ID", typeof(string));
                dt.Columns.Add("WOID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("PRODDESC", typeof(string));
                dt.Columns.Add("PRODNAME", typeof(string));
                dt.Columns.Add("MODLID", typeof(string));
                dt.Columns.Add("WH_RCV_DTTM", typeof(DateTime));
                dt.Columns.Add("WIP_QTY", typeof(decimal));
                dt.Columns.Add("QA", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("VLD_DATE", typeof(string));
                dt.Columns.Add("WIPHOLD", typeof(string));
                dt.Columns.Add("WIPDTTM_ED", typeof(string));
                dt.Columns.Add("SPCL_FLAG", typeof(string));
                dt.Columns.Add("SPCL_RSNCODE", typeof(string));
                dt.Columns.Add("WIP_REMARKS", typeof(string));

                DataRow dr;
                if (rack.UserData.ContainsKey("WOID_1"))
                {
                    dr = dt.NewRow();
                    dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_1") ? rack.UserData["RACK_ID_1"] : "");
                    dr["PRJT_NAME"] = rack.ProjectName;
                    dr["MCS_CST_ID"] = rack.SkidID;
                    dr["WOID"] = (rack.UserData.ContainsKey("WOID_1") ? rack.UserData["WOID_1"] : "");
                    dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_1") ? rack.UserData["PRODID_1"] : "");
                    dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_1") ? rack.UserData["PRODDESC_1"] : "");
                    dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_1") ? rack.UserData["PRODNAME_1"] : "");
                    dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_1") ? rack.UserData["MODLID_1"] : "");
                    dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_1") ? rack.UserData["WH_RCV_DTTM_1"] : DBNull.Value);
                    dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_1") ? rack.UserData["WIP_QTY_1"] : DBNull.Value);
                    dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_1") ? rack.UserData["VLD_DATE_1"] : DBNull.Value);
                    dr["QA"] = rack.PancakeQA1;
                    dr["LOTID"] = rack.PancakeID1;
                    dr["WIPHOLD"] = rack.WIPHOLD1;
                    dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                    dr["SPCL_FLAG"] = rack.Spcl_Flag;
                    dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                    dr["WIP_REMARKS"] = rack.Wip_Remarks;
                    dt.Rows.Add(dr);
                }

                if (rack.UserData.ContainsKey("WOID_2"))
                {
                    dr = dt.NewRow();
                    dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_2") ? rack.UserData["RACK_ID_2"] : "");
                    dr["PRJT_NAME"] = rack.ProjectName;
                    dr["MCS_CST_ID"] = rack.SkidID;
                    dr["WOID"] = (rack.UserData.ContainsKey("WOID_2") ? rack.UserData["WOID_2"] : "");
                    dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_2") ? rack.UserData["PRODID_2"] : "");
                    dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_2") ? rack.UserData["PRODDESC_2"] : "");
                    dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_2") ? rack.UserData["PRODNAME_2"] : "");
                    dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_2") ? rack.UserData["MODLID_2"] : "");
                    dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_2") ? rack.UserData["WH_RCV_DTTM_2"] : DBNull.Value);
                    dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_2") ? rack.UserData["WIP_QTY_2"] : DBNull.Value);
                    dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_2") ? rack.UserData["VLD_DATE_2"] : DBNull.Value);
                    dr["QA"] = rack.PancakeQA2;
                    dr["LOTID"] = rack.PancakeID2;
                    dr["WIPHOLD"] = rack.WIPHOLD2;
                    dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                    dr["SPCL_FLAG"] = rack.Spcl_Flag;
                    dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                    dr["WIP_REMARKS"] = rack.Wip_Remarks;
                    dt.Rows.Add(dr);
                }
                if (rack.UserData.ContainsKey("WOID_3"))
                {
                    dr = dt.NewRow();
                    dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_3") ? rack.UserData["RACK_ID_3"] : "");
                    dr["PRJT_NAME"] = rack.ProjectName;
                    dr["MCS_CST_ID"] = rack.SkidID;
                    dr["WOID"] = (rack.UserData.ContainsKey("WOID_3") ? rack.UserData["WOID_3"] : "");
                    dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_3") ? rack.UserData["PRODID_3"] : "");
                    dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_3") ? rack.UserData["PRODDESC_3"] : "");
                    dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_3") ? rack.UserData["PRODNAME_3"] : "");
                    dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_3") ? rack.UserData["MODLID_3"] : "");
                    dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_3") ? rack.UserData["WH_RCV_DTTM_3"] : DBNull.Value);
                    dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_3") ? rack.UserData["WIP_QTY_3"] : DBNull.Value);
                    dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_3") ? rack.UserData["VLD_DATE_3"] : DBNull.Value);
                    dr["QA"] = rack.PancakeQA3;
                    dr["LOTID"] = rack.PancakeID3;
                    dr["WIPHOLD"] = rack.WIPHOLD3;
                    dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                    dr["SPCL_FLAG"] = rack.Spcl_Flag;
                    dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                    dr["WIP_REMARKS"] = rack.Wip_Remarks;
                    dt.Rows.Add(dr);
                    this.Height = this.Height + 80;
                }
                if (rack.UserData.ContainsKey("WOID_4"))
                {
                    dr = dt.NewRow();
                    dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_4") ? rack.UserData["RACK_ID_4"] : "");
                    dr["PRJT_NAME"] = rack.ProjectName;
                    dr["MCS_CST_ID"] = rack.SkidID;
                    dr["WOID"] = (rack.UserData.ContainsKey("WOID_4") ? rack.UserData["WOID_4"] : "");
                    dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_4") ? rack.UserData["PRODID_4"] : "");
                    dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_4") ? rack.UserData["PRODDESC_4"] : "");
                    dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_4") ? rack.UserData["PRODNAME_4"] : "");
                    dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_4") ? rack.UserData["MODLID_4"] : "");
                    dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_4") ? rack.UserData["WH_RCV_DTTM_4"] : DBNull.Value);
                    dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_4") ? rack.UserData["WIP_QTY_4"] : DBNull.Value);
                    dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_4") ? rack.UserData["VLD_DATE_4"] : DBNull.Value);
                    dr["QA"] = rack.PancakeQA4;
                    dr["LOTID"] = rack.PancakeID4;
                    dr["WIPHOLD"] = rack.WIPHOLD4;
                    dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                    dr["SPCL_FLAG"] = rack.Spcl_Flag;
                    dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                    dr["WIP_REMARKS"] = rack.Wip_Remarks;
                    dt.Rows.Add(dr);
                }
                if (rack.UserData.ContainsKey("WOID_5"))
                {
                    dr = dt.NewRow();
                    dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_5") ? rack.UserData["RACK_ID_5"] : "");
                    dr["PRJT_NAME"] = rack.ProjectName;
                    dr["MCS_CST_ID"] = rack.SkidID;
                    dr["WOID"] = (rack.UserData.ContainsKey("WOID_5") ? rack.UserData["WOID_5"] : "");
                    dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_5") ? rack.UserData["PRODID_5"] : "");
                    dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_5") ? rack.UserData["PRODDESC_5"] : "");
                    dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_5") ? rack.UserData["PRODNAME_5"] : "");
                    dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_5") ? rack.UserData["MODLID_5"] : "");
                    dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_5") ? rack.UserData["WH_RCV_DTTM_5"] : DBNull.Value);
                    dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_5") ? rack.UserData["WIP_QTY_5"] : DBNull.Value);
                    dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_5") ? rack.UserData["VLD_DATE_5"] : DBNull.Value);
                    dr["QA"] = rack.PancakeQA5;
                    dr["LOTID"] = rack.PancakeID5;
                    dr["WIPHOLD"] = rack.WIPHOLD5;
                    dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                    dr["SPCL_FLAG"] = rack.Spcl_Flag;
                    dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                    dr["WIP_REMARKS"] = rack.Wip_Remarks;
                    dt.Rows.Add(dr);
                }
                if (rack.UserData.ContainsKey("WOID_6"))
                {
                    dr = dt.NewRow();
                    dr["RACK_ID"] = (rack.UserData.ContainsKey("RACK_ID_6") ? rack.UserData["RACK_ID_6"] : "");
                    dr["PRJT_NAME"] = rack.ProjectName;
                    dr["MCS_CST_ID"] = rack.SkidID;
                    dr["WOID"] = (rack.UserData.ContainsKey("WOID_6") ? rack.UserData["WOID_6"] : "");
                    dr["PRODID"] = (rack.UserData.ContainsKey("PRODID_6") ? rack.UserData["PRODID_6"] : "");
                    dr["PRODDESC"] = (rack.UserData.ContainsKey("PRODDESC_6") ? rack.UserData["PRODDESC_6"] : "");
                    dr["PRODNAME"] = (rack.UserData.ContainsKey("PRODNAME_6") ? rack.UserData["PRODNAME_6"] : "");
                    dr["MODLID"] = (rack.UserData.ContainsKey("MODLID_6") ? rack.UserData["MODLID_6"] : "");
                    dr["WH_RCV_DTTM"] = (rack.UserData.ContainsKey("WH_RCV_DTTM_6") ? rack.UserData["WH_RCV_DTTM_6"] : DBNull.Value);
                    dr["WIP_QTY"] = (rack.UserData.ContainsKey("WIP_QTY_6") ? rack.UserData["WIP_QTY_6"] : DBNull.Value);
                    dr["VLD_DATE"] = (rack.UserData.ContainsKey("VLD_DATE_6") ? rack.UserData["VLD_DATE_6"] : DBNull.Value);
                    dr["QA"] = rack.PancakeQA6;
                    dr["LOTID"] = rack.PancakeID6;
                    dr["WIPHOLD"] = rack.WIPHOLD6;
                    dr["WIPDTTM_ED"] = rack.Wipdttm_ED;
                    dr["SPCL_FLAG"] = rack.Spcl_Flag;
                    dr["SPCL_RSNCODE"] = rack.Spcl_RsnCode;
                    dr["WIP_REMARKS"] = rack.Wip_Remarks;
                    dt.Rows.Add(dr);
                } 
                Util.GridSetData(this.dgList, dt, null, false);
            }
        }

        /// <summary>
        /// 정보변경버튼으로 정보 변경여부 확인
        /// </summary>
        public bool IsUpdated {
            get;
            set;
        }

        private void OnRdoClick(object sender, RoutedEventArgs e)
        {
            if (rdoLock.IsChecked.Value)
            {
                // 입고 금지
                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                bool b = false;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["LOTID"].ToString().Equals("") == false)
                    {
                        b = true;
                        break;
                    }
                }
                if (b)
                {
                    // rack이 비어있지 않음
                    btnUpdate.IsEnabled = false;
                }
                else {
                    btnUpdate.IsEnabled = true;
                }
                SkidPanel.Visibility = Visibility.Hidden;
                txtMCS_MCS_ID.Text = "";
                this.InitGrid();
            }
            else if (rdoUnlock.IsChecked.Value)
            {
                // 입고 가능
                btnUpdate.IsEnabled = true;
                SkidPanel.Visibility = Visibility.Hidden;
                txtMCS_MCS_ID.Text = "";

                this.InitGrid();              
            }
            else if (rdoDeleteRackInfo.IsChecked.Value)
            {
                // 정보 삭제
                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                bool b = false;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["LOTID"].ToString().Equals("") == false)
                    {
                        b = true;
                        break;
                    }
                }
                if (b)
                {
                    // rack이 비어있지 않음
                    btnUpdate.IsEnabled = true;
                }
                else {
                    btnUpdate.IsEnabled = true;
                }
                SkidPanel.Visibility = Visibility.Hidden;
                txtMCS_MCS_ID.Text = "";
            }
            else if (rdoSKID.IsChecked.Value)
            {
                bIsEmptySKID = false;

                SkidPanel.Visibility = Visibility.Visible;

                txtMCS_MCS_ID.Focus();
            }
        }

        /// <summary>
        /// 정보변경 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnUpdate(object sender, RoutedEventArgs e) {
            Util.MessageConfirm("SFU4524", (result) => {

                if (result == MessageBoxResult.OK) {

                    C1.WPF.DataGrid.DataGridCell cellRack = dgList.GetCell(0, dgList.Columns["RACK_ID"].Index);
                    try {
                        loadingIndicator.Visibility = Visibility.Visible;

                        if (rdoUnlock.IsChecked.Value || rdoLock.IsChecked.Value) {
                            // rack 입고 변경
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("RACK_ID", typeof(string));
                            RQSTDT.Columns.Add("RACK_STAT_CODE", typeof(string));
                            RQSTDT.Columns.Add("UPDUSER", typeof(string));
                            DataRow dr = RQSTDT.NewRow();
                            dr["RACK_ID"] = cellRack.Value.ToString();
                            dr["RACK_STAT_CODE"] = rdoUnlock.IsChecked.Value ? "USABLE"/*가능*/ : "UNUSE"/*금지*/;
                            dr["UPDUSER"] = LoginInfo.USERID;
                            RQSTDT.Rows.Add(dr);                       
                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_RACK_STAT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                            // 정보 변경으로 확인
                            IsUpdated = true;

                            Util.MessageInfo("SFU1275", (result2) =>
                            {
                                this.DialogResult = MessageBoxResult.Cancel;
                            });
                        }
                        else if (rdoDeleteRackInfo.IsChecked.Value) {
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("RACK_ID", typeof(string));
                            RQSTDT.Columns.Add("UPDUSER", typeof(string));                           

                            DataRow dr = RQSTDT.NewRow();
                            dr["RACK_ID"] = cellRack.Value.ToString();
                            dr["UPDUSER"] = LoginInfo.USERID;
                            RQSTDT.Rows.Add(dr);

                            DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_MCS_REG_RACK_INFO", "INDATA", null, RQSTDT);
                            // 정보 변경으로 확인
                            IsUpdated = true;
                            // 삭제의 경우 화면에 보이는 lot정보 제거
                            Util.gridClear(dgList);
                            //}


                            Util.MessageInfo("SFU1275", (result2) =>
                            {
                                this.DialogResult = MessageBoxResult.Cancel;
                            });  
                        }
                        else if (rdoSKID.IsChecked.Value)
                        {
                            // empty SKID 입고
                            if (bIsEmptySKID)
                            {
                                DataTable RQSTDT = new DataTable();
                                RQSTDT.TableName = "RQSTDT";
                                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                                RQSTDT.Columns.Add("MCS_CST_ID", typeof(string));
                                RQSTDT.Columns.Add("UPDUSER", typeof(string));

                                DataRow dr = RQSTDT.NewRow();
                                dr["RACK_ID"] = cellRack.Value.ToString();
                                dr["MCS_CST_ID"] = txtMCS_MCS_ID.Text.Trim().ToUpper();
                                dr["UPDUSER"] = LoginInfo.USERID;
                                RQSTDT.Rows.Add(dr);

                                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_EMPTY_SKID", "RQSTDT", null, RQSTDT);

                                // 정보 변경으로 확인
                                IsUpdated = true;

                                Util.MessageInfo("SFU1275", (result2) =>
                                {
                                    this.DialogResult = MessageBoxResult.Cancel;
                                });
                            }
                           
                            // SKID 입고  BR_MCS_REG_RCV_COMPLETE_UI
                            else {
                                DataTable RQSTDT = new DataTable();
                                RQSTDT.TableName = "RQSTDT";
                                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                                RQSTDT.Columns.Add("MCS_CST_ID", typeof(string));
                                RQSTDT.Columns.Add("WH_ID", typeof(string));
                                RQSTDT.Columns.Add("ZONE_ID", typeof(string));
                                RQSTDT.Columns.Add("USERID", typeof(string));

                                DataRow dr = RQSTDT.NewRow();
                                dr["RACK_ID"] = cellRack.Value.ToString();
                                dr["MCS_CST_ID"] = txtMCS_MCS_ID.Text.Trim().ToUpper();
                                dr["WH_ID"] = WH_ID;
                                dr["ZONE_ID"] = sZONE;
                                dr["USERID"] = LoginInfo.USERID;
                                RQSTDT.Rows.Add(dr);

                                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_MCS_REG_RCV_COMPLETE_UI", "INDATA", null, RQSTDT);

                                // 정보 변경으로 확인
                                IsUpdated = true;

                                Util.MessageInfo("SFU1275", (result2) =>
                                {
                                    this.DialogResult = MessageBoxResult.Cancel;
                                });
                            }
                        }
                        else if (rdoUndoSPCL.IsChecked.Value)
                        {
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("RACK_ID", typeof(string));

                            DataRow dr = RQSTDT.NewRow();
                            dr["RACK_ID"] = cellRack.Value.ToString();
                            RQSTDT.Rows.Add(dr);

                            DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_SPCL_CLEAR", "RQSTDT", null, RQSTDT);

                            // 정보 변경으로 확인
                            IsUpdated = true;

                            Util.MessageInfo("SFU1275", (result2) =>
                            {
                                this.DialogResult = MessageBoxResult.Cancel;
                            });

                        }

                    } catch (Exception ex) {
                        Util.MessageException(ex);

                    } finally {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }

            });

        }

        /// <summary>
        /// 닫기 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBtnClose(object sender, RoutedEventArgs e) {
            this.DialogResult = MessageBoxResult.Cancel;
        }



        private void SearchSKID()
        {
            try
            {

                string sMCS_CST_ID = txtMCS_MCS_ID.Text.ToUpper();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MCS_CST_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MCS_CST_ID"] = sMCS_CST_ID;
                RQSTDT.Rows.Add(dr);

                // validate 필요함
                DataTable dtValidate = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_VALI_SKID", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtValidate != null && dtValidate.Rows.Count > 0)
                {
                    if (dtValidate.Rows[0]["ISEXIST"].ToString() == "T")
                    {
                        string sExistRack = dtValidate.Rows[0]["RACK_ID"].ToString();
                        Util.MessageInfo("SFU4488", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtMCS_MCS_ID.Focus();
                                txtMCS_MCS_ID.SelectAll();
                            }
                        }, new object[] { sExistRack, sMCS_CST_ID });

                        bIsEmptySKID = false;

                        btnUpdate.IsEnabled = false;

                        return;
                    }
                }
                                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_SKID_ID", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("RACK_ID", typeof(string));
                    dt.Columns.Add("PRJT_NAME", typeof(string));
                    dt.Columns.Add("MCS_CST_ID", typeof(string));
                    dt.Columns.Add("WOID", typeof(string));
                    dt.Columns.Add("PRODID", typeof(string));
                    dt.Columns.Add("PRODDESC", typeof(string));
                    dt.Columns.Add("PRODNAME", typeof(string));
                    dt.Columns.Add("MODLID", typeof(string));
                    dt.Columns.Add("WH_RCV_DTTM", typeof(DateTime));
                    dt.Columns.Add("WIP_QTY", typeof(decimal));
                    dt.Columns.Add("QA", typeof(string));
                    dt.Columns.Add("LOTID", typeof(string));
                    dt.Columns.Add("WIPHOLD", typeof(string));
                    dt.Columns.Add("VLD_DATE", typeof(string));
                    dt.Columns.Add("WIPDTTM_ED", typeof(string));
                    //dt.Columns.Add("QA", typeof(string));

                    foreach (DataRow row in dtResult.Rows)
                    {
                        dr = dt.NewRow();
                        dr["RACK_ID"] = sRACK_ID;
                        dr["MCS_CST_ID"] = row["MCS_CST_ID"];
                        dr["LOTID"] = row["LOTID"];
                        dr["PRJT_NAME"] = row["PRJT_NAME"];
                        dr["PRODID"] = row["PRODID"];
                        dr["PRODDESC"] = row["PRODDESC"];
                        dr["PRODNAME"] = row["PRODNAME"];
                        dr["MODLID"] = row["MODLID"];
                        dr["WIP_QTY"] = row["WIPQTY"];
                        dr["WIPHOLD"] = row["WIPHOLD"];
                        dr["WH_RCV_DTTM"] = System.DateTime.Now;
                        dr["VLD_DATE"] = row["VLD_DATE"];
                        dr["WIPDTTM_ED"] = row["WIPDTTM_ED"];
                        string qa = "";
                        switch (row["JUDG_VALUE"].ToString())
                        {
                            case "TERM":
                                qa = ObjectDic.Instance.GetObjectName("TERM");
                                break;

                            case "Y":
                                qa = ObjectDic.Instance.GetObjectName("합격");
                                break;
                            case "F":
                                qa = ObjectDic.Instance.GetObjectName("불합격");
                                break;
                            case "W":
                                qa = ObjectDic.Instance.GetObjectName("검사대기");
                                break;
                            case "I":
                                qa = ObjectDic.Instance.GetObjectName("검사중");
                                break;
                            case "R":
                                qa = ObjectDic.Instance.GetObjectName("재작업");
                                break;
                            default:
                                qa = "";
                                break;
                        }
                        dr["QA"] = qa;
                        dt.Rows.Add(dr);
                    }
                    Util.GridSetData(this.dgList, dt, null, false);

                    btnUpdate.IsEnabled = true;
                }
                else {
                        Util.MessageConfirm("SFU4540", (result) => {

                        bIsEmptySKID = true;

                        if (result == MessageBoxResult.OK)
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("RACK_ID", typeof(string));
                            dt.Columns.Add("PRJT_NAME", typeof(string));
                            dt.Columns.Add("MCS_CST_ID", typeof(string));
                            dt.Columns.Add("WOID", typeof(string));
                            dt.Columns.Add("PRODID", typeof(string));
                            dt.Columns.Add("PRODDESC", typeof(string));
                                dt.Columns.Add("PRODNAME", typeof(string));
                                dt.Columns.Add("MODLID", typeof(string));
                            dt.Columns.Add("WH_RCV_DTTM", typeof(DateTime));
                            dt.Columns.Add("WIP_QTY", typeof(decimal));
                            dt.Columns.Add("QA", typeof(string));
                            dt.Columns.Add("LOTID", typeof(string));
                            dt.Columns.Add("WIPHOLD", typeof(string));
                            dt.Columns.Add("SPCL_FLAG", typeof(string));
                            dt.Columns.Add("SPCL_RSNCODE", typeof(string));
                            dt.Columns.Add("WIP_REMARKS", typeof(string));

                            DataRow drr = dt.NewRow();
                            drr["RACK_ID"] = sRACK_ID;
                            drr["MCS_CST_ID"] = sMCS_CST_ID;                               
                            dt.Rows.Add(drr);
                           
                            Util.GridSetData(this.dgList, dt, null, false);

                            btnUpdate.IsEnabled = true;
                        }
                        else
                        {
                            btnUpdate.IsEnabled = false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                e.Handled = true;
                SearchSKID();                
            }
        }


        private void InitCombo()
        {
            // 특별관리 combo
            CommonCombo _combo = new CommonCombo();

            // 특별 SKID  사유 Combo
            String[] sFilter3 = { "SPCL_RSNCODE" };
            _combo.SetCombo(cboSPCL, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");

            if (cboSPCL != null && cboSPCL.Items != null && cboSPCL.Items.Count > 0)
                cboSPCL.SelectedIndex = 0;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.StackSPCL.Visibility = Visibility.Visible;
            }
            catch
            {
                
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.StackSPCL.Visibility = Visibility.Hidden;
            }
            catch
            {

            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string sSPCLCode = this.cboSPCL.SelectedValue.ToString();
            string sRemarks = this.tbSPCLREMARKS.Text.Trim();
            //string sRackId = rack.RackId;

            if (sSPCLCode == "SELECT")
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4542"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);   //메세지 작성요
                //this.cboSPCL.SelectAll();
                this.cboSPCL.Focus();
                return;
            }




            Util.MessageConfirm("SFU4545", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RACK_ID", typeof(string));
                    RQSTDT.Columns.Add("SPCL_RSNCODE", typeof(string));
                    RQSTDT.Columns.Add("WIP_REMARKS", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["RACK_ID"] = sRACK_ID;
                    dr["SPCL_RSNCODE"] = sSPCLCode;
                    dr["WIP_REMARKS"] = sRemarks;
                    RQSTDT.Rows.Add(dr);

                    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_SPCL", "INDATA", null, RQSTDT);

                    // 정보 변경으로 확인
                    IsUpdated = true;

                    this.cboSPCL.SelectedIndex = 0;
                    this.tbSPCLREMARKS.Text = "";

                    this.ChkSPCL.IsChecked = false;

                    this.StackSPCL.Visibility = Visibility.Hidden;

                    Util.MessageInfo("SFU1275");
                }
            });
                    

        }
    }
}
