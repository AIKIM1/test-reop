/*************************************************************************************
 Created Date : 2016.11.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.14  DEVELOPER : Initial Created.





 
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
using LGC.GMES.MES.CMM001; 
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_037_SKDPNMAPPING : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        public ASSY001_037_SKDPNMAPPING()
        {
            InitializeComponent();
            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 특별 SKID  사유 Combo
            String[] sFilter3 = { "SPCL_RSNCODE" };
            _combo.SetCombo(cboSkidSplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");

            if (cboSkidSplReason != null && cboSkidSplReason.Items != null && cboSkidSplReason.Items.Count > 0)
                cboSkidSplReason.SelectedIndex = 0; 
            

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtSkidID.Focus();
        }


        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= C1Window_Loaded;
        }

        private void txtSkid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (CheckSkid(txtSkidID.Text))
                {
                    txtLotID.Clear();
                    txtLotID.Focus();
                }
            }
        }
        private void txtLotID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if(string.IsNullOrEmpty(txtSkidID.Text))
                {
                      //입력한 Skid ID 가 없습니다..
                    Util.MessageInfo("SFU2934", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtSkidID.Focus();
                            txtSkidID.SelectAll();
                        }
                    });
                    return;
                }

                int skidCnt = 0;

                for (int i = 0; i < dgEqptCond.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.Convert(dgEqptCond.ItemsSource).Rows[i]["SKIDID"]).Equals(txtSkidID.Text))
                    {
                         
                        skidCnt++; 
                        int  chkSkidQty = getMaxSkidQty(DataTableConverter.Convert(dgEqptCond.ItemsSource).Rows[i]["PANCAKEID"].ToString());
                        if (skidCnt == chkSkidQty)
                        {
                            //SKID에 매핑된 2개 이상의 팬케익 정보가 존재합니다..
                            Util.MessageInfo("100919", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtSkidID.Focus();
                                    txtSkidID.SelectAll();
                                }
                            }, txtSkidID.Text);
                            return;
                        }
                    }
                }

                for (int i = 0; i < dgEqptCond.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.Convert(dgEqptCond.ItemsSource).Rows[i]["PANCAKEID"]).Equals(txtLotID.Text))
                    {
                        
                       //동일한 LOT이 스캔되었습니다.
                        Util.MessageInfo("SFU1504", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtLotID.Focus();
                                txtLotID.SelectAll();
                            }
                        });
                        return;
                    }
                }

                if (!CanSkidSplSave()) return;

                DataTable data = SearchLotInfo(txtLotID.Text);

                if (data.Rows.Count > 0)
                {
                    DrawGrid(txtSkidID.Text, data);
                    txtLotID.Focus();
                    txtLotID.SelectAll();
                }

                else
                {  //잘못된 ID를 스캔하였습니다. PanCake ID를 스캔하여 주시기 바랍니다.
                    Util.MessageInfo("SFU2945", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotID.Focus();
                            txtLotID.SelectAll();
                        }
                    }); 
                     

                }

            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {


            try
            {
               

                DataTable gridList = DataTableConverter.Convert(dgEqptCond.ItemsSource);

                if (gridList.Rows.Count > 0)
                {
                    DataTable saveData = new DataTable();
                    saveData.Columns.Add("SKIDID");
                    saveData.Columns.Add("LOTID");
                    saveData.Columns.Add("SPCL_FLAG");
                    saveData.Columns.Add("SPCL_RSNCODE");
                    saveData.Columns.Add("WIP_REMARKS");

                    for (int i = 0; i < gridList.Rows.Count; i++)
                    {

                        DataRow indata = saveData.NewRow();
                        indata["SKIDID"] = gridList.Rows[i]["SKIDID"];
                        indata["LOTID"] = gridList.Rows[i]["PANCAKEID"]; 

                        indata["SPCL_FLAG"] = gridList.Rows[i]["SPCL_FLAG"];
                        indata["SPCL_RSNCODE"] = gridList.Rows[i]["SPCL_RSNCODE"];
                        indata["WIP_REMARKS"] = gridList.Rows[i]["WIP_REMARKS"]; 

                        saveData.Rows.Add(indata);
                    }
                    var rslt = new ClientProxy().ExecuteServiceSync("BR_MCS_REG_SKID_PANCAKE_MAPPING", "INDATA", "OUTDATA", saveData);
                    Util.MessageInfo("SFU1270");  //저장되었습니다.
                    this.DialogResult = MessageBoxResult.Cancel;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private int getMaxSkidQty(string sLotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotID;

                RQSTDT.Rows.Add(dr);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAX_QTYPERSKID", "INDATA", "RSLTDT", RQSTDT);

                if (dtMain.Rows.Count > 0) return Util.NVC_Int(dtMain.Rows[0]["KEYVALUE"]);
                else return 2;
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return 2;
            } 
        }

        private int getMaxSkidQtyBySkidID(string skidID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SKIDID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SKIDID"] = skidID;

                RQSTDT.Rows.Add(dr); 
                  
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAX_QTYPERSKID_BY_SKID", "INDATA", "RSLTDT", RQSTDT);

                 

                if (dtMain.Rows.Count > 0) return Util.NVC_Int(dtMain.Rows[0]["KEYVALUE"])-1;
                else return 1;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 1;
            }
        }
        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void chkSkidSpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cboSkidSplReason.SelectedIndex !=  0)
            {
                cboSkidSplReason.SelectedIndex = 0;
            }

            if (txtSkidRemark != null)
            {
                txtSkidRemark.Text = "";
            }
        }

        #endregion

        #region Mehod
        private void DrawGrid(string skidId, DataTable lotInfo)
        {
            bool dupFlag = false;
            DataTable setGrid = DataTableConverter.Convert(dgEqptCond.ItemsSource);

            if(!string.IsNullOrEmpty(Util.NVC(lotInfo.Rows[0]["SKIDID"])))
            {
                skidId = Util.NVC(lotInfo.Rows[0]["SKIDID"]);
                dupFlag = true;
            }
            if (setGrid.Rows.Count > 0)
            {
                DataRow dgRow = setGrid.NewRow();
                dgRow["SKIDID"] = skidId;
                dgRow["PANCAKEID"] = lotInfo.Rows[0]["LOTID"];
                dgRow["ELEC_TYPE_CODE"] = lotInfo.Rows[0]["ELEC_TYPE_CODE"];
                dgRow["PJT"] = lotInfo.Rows[0]["PJT"];
                dgRow["ProductID"] = lotInfo.Rows[0]["PRODID"];
                dgRow["ModelID"] = lotInfo.Rows[0]["MODLID"];
                dgRow["Quantity"] = lotInfo.Rows[0]["WIPQTY"];
                dgRow["SPCL_FLAG"] = chkSkidSpl.IsChecked == true ? "Y" : "N";
                dgRow["SPCL_RSNCODENAME"] = chkSkidSpl.IsChecked == true ? cboSkidSplReason.Text : null; 
                dgRow["SPCL_RSNCODE"] = chkSkidSpl.IsChecked == true ? cboSkidSplReason.SelectedValue : null;
                dgRow["WIP_REMARKS"] = chkSkidSpl.IsChecked == true ? txtSkidRemark.Text : null;



                setGrid.Rows.Add(dgRow);
                Util.gridClear(dgEqptCond);
                dgEqptCond.ItemsSource = DataTableConverter.Convert(setGrid);

                txtLotID.Focus();
                txtLotID.SelectAll();

            }
            else if (setGrid.Rows.Count == 0)
            {
                DataTable gridList = new DataTable();
                gridList.Columns.Add("SKIDID", typeof(string));
                gridList.Columns.Add("PANCAKEID", typeof(string));
                gridList.Columns.Add("ELEC_TYPE_CODE", typeof(string));
                gridList.Columns.Add("PJT", typeof(string));
                gridList.Columns.Add("ProductID", typeof(string));
                gridList.Columns.Add("ModelID", typeof(string));
                gridList.Columns.Add("Quantity", typeof(decimal));
                gridList.Columns.Add("SPCL_FLAG", typeof(string));
                gridList.Columns.Add("SPCL_RSNCODENAME", typeof(string));
                gridList.Columns.Add("SPCL_RSNCODE", typeof(string));
                gridList.Columns.Add("WIP_REMARKS", typeof(string));

                DataRow dgRow = gridList.NewRow();
                dgRow["SKIDID"] = skidId;
                dgRow["PANCAKEID"] = lotInfo.Rows[0]["LOTID"];
                dgRow["ELEC_TYPE_CODE"] = lotInfo.Rows[0]["ELEC_TYPE_CODE"];
                dgRow["PJT"] = lotInfo.Rows[0]["PJT"];
                dgRow["ProductID"] = lotInfo.Rows[0]["PRODID"];
                dgRow["ModelID"] = lotInfo.Rows[0]["MODLID"];
                dgRow["Quantity"] = lotInfo.Rows[0]["WIPQTY"];
                dgRow["SPCL_FLAG"] = chkSkidSpl.IsChecked == true ? "Y" : "N";
                dgRow["SPCL_RSNCODENAME"] = chkSkidSpl.IsChecked == true ? cboSkidSplReason.Text : null;
                dgRow["SPCL_RSNCODE"] = chkSkidSpl.IsChecked == true ? cboSkidSplReason.SelectedValue : null;
                dgRow["WIP_REMARKS"] = chkSkidSpl.IsChecked == true ? txtSkidRemark.Text : null;

                gridList.Rows.Add(dgRow);

                dgEqptCond.ItemsSource = DataTableConverter.Convert(gridList);
 
            }
             
            if(dupFlag)
            {
                Util.MessageInfo("SFU3068", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtLotID.Focus();
                        txtLotID.SelectAll();
                    }
                });  //SKIDID가 이미 선택되었습니다.
            }
            
        }


        private DataTable SearchLotInfo(string lotId)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = lotId;

                RQSTDT.Rows.Add(dr);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKIP_MAPP_LOTINFO", "INDATA", "RSLTDT", RQSTDT);

                return dtMain;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private bool CheckSkid(string skidID)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSkidID.Text))
                {
                    //입력한 Skid ID 가 없습니다..
                    Util.MessageInfo("SFU2934", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtSkidID.Focus();
                            txtSkidID.SelectAll();
                        }
                    });
                    return false;
                }


                if (!CheckSkidInWareHouse(skidID)) return false;


                for (int i = 0; i < dgEqptCond.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.Convert(dgEqptCond.ItemsSource).Rows[i]["SKIDID"]).Equals(skidID))
                    { //동일한 SKID ID가 스캔되었습니다.
                        Util.MessageInfo("SFU2862", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSkidID.Focus();
                                txtSkidID.SelectAll();
                            }

                        });
                        return false;
                    }
                }
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("SKIDID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SKIDID"] = skidID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);
                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_CHECK", "INDATA", "RSLTDT", RQSTDT);


                //Util.GridSetData(dgEqptCond, dt, FrameOperation);

                if (dgEqptCond.Rows.Count == 0)
                {
                 //   Util.GridSetData(dgEqptCond,dt, FrameOperation, true); 
                     dgEqptCond.ItemsSource = DataTableConverter.Convert(dt);
                }
                else
                {
                    DataTable setGrid = DataTableConverter.Convert(dgEqptCond.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dgRow = setGrid.NewRow();
                        dgRow["SKIDID"] = Util.NVC(dt.Rows[i]["SKIDID"]);
                        dgRow["PANCAKEID"] = Util.NVC(dt.Rows[i]["PANCAKEID"]);
                        dgRow["ELEC_TYPE_CODE"] = Util.NVC(dt.Rows[i]["ELEC_TYPE_CODE"]);
                        dgRow["PJT"] = Util.NVC(dt.Rows[i]["PJT"]);
                        dgRow["ProductID"] = Util.NVC(dt.Rows[i]["ProductID"]);
                        dgRow["ModelID"] = Util.NVC(dt.Rows[i]["ModelID"]);
                        dgRow["Quantity"] = Util.NVC(dt.Rows[i]["Quantity"]);
                        dgRow["SPCL_FLAG"] = Util.NVC(dt.Rows[i]["SPCL_FLAG"]);
                        dgRow["SPCL_RSNCODENAME"] = Util.NVC(dt.Rows[i]["SPCL_RSNCODENAME"]);
                        dgRow["WIP_REMARKS"] = Util.NVC(dt.Rows[i]["WIP_REMARKS"]);

                        setGrid.Rows.Add(dgRow);
                        Util.gridClear(dgEqptCond);
                        dgEqptCond.ItemsSource = DataTableConverter.Convert(setGrid);

                    }
                }

                
                int chkMaxQtyPerSkid = getMaxSkidQtyBySkidID(skidID);

                
                if (dt.Rows.Count > chkMaxQtyPerSkid)
                {
                    Util.MessageInfo("SKID ID already mapped", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtSkidID.Focus();
                            txtSkidID.SelectAll();
                        }
                    });
                    return false;
                }

               



                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }


        private bool CheckSkidInWareHouse(string skidID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("MCS_CST_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MCS_CST_ID"] = skidID;

                RQSTDT.Rows.Add(dr); 
                

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_RACK_BY_SKID", "RQSTDT", "RSLTDT", RQSTDT);

                 

                if (dt.Rows.Count > 0)
                {
                    Util.MessageInfo("SFU4546", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtSkidID.Clear();
                            txtSkidID.Focus();

                        }
                    }, Util.NVC(dt.Rows[0]["ZONE_ID"]), Util.NVC(dt.Rows[0]["X_PSTN"]), Util.NVC(dt.Rows[0]["Y_PSTN"]), Util.NVC(dt.Rows[0]["Z_PSTN"]), Util.NVC(dt.Rows[0]["MCS_CST_ID"]));  
                    // [%1] ZONE [%2]-[%3]-[%4]에 입고정보가 존재합니다. 해당 SKID[%5]를 사용할 수 없습니다. 생산팀에 문의하세요.


                    return false;


                }

                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }
        #endregion

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                //삭제하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.Columns.Add("LOTID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["LOTID"] = DataTableConverter.Convert(dgEqptCond.ItemsSource).Rows[index]["PANCAKEID"];

                        RQSTDT.Rows.Add(dr);

                        DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_UPD_WIPATTR_CSTID", "INDATA", null, RQSTDT);

                        DataTable dt = DataTableConverter.Convert(dgEqptCond.ItemsSource);

                        dt.Rows.RemoveAt(index);

                        dgEqptCond.ItemsSource = DataTableConverter.Convert(dt);

                        txtLotID.Focus();
                        txtLotID.SelectAll();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool CanSkidSplSave()
        {
            bool bRet = false;

             

            if (chkSkidSpl.IsChecked.HasValue && (bool)chkSkidSpl.IsChecked)
            {
                if (cboSkidSplReason.SelectedValue == null || cboSkidSplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return bRet;
                }

                if (txtSkidRemark.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return bRet;
                }
            }
            else
            {
                if (!txtSkidRemark.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 삭제 하세요.");
                    Util.MessageValidation("SFU1589");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private void btnRowDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

            DataTable dt = DataTableConverter.Convert(dgEqptCond.ItemsSource);

            dt.Rows.RemoveAt(index);

            dgEqptCond.ItemsSource = DataTableConverter.Convert(dt);

            txtLotID.Focus();
            txtLotID.SelectAll();
        }
    }
    }