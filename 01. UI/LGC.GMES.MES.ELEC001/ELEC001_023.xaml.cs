/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.01.13  정재홍      [C20211111-000238] - 전극 극성 분리 보관을 위한 인터락 구축 건 
 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_023 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();
        string _WH_ID = string.Empty;
        string _RACK_ID = string.Empty;

        public ELEC001_023()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            ApplyPermissions();
            Initialize();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReceive);
            listAuth.Add(btnReceiveSRS);
            listAuth.Add(btnChange);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            Set_Combo_WHID(cboWHID);
            Set_Combo_WHID(cboWHIDSRS);
        }
        #endregion

        #region Mehod
        private void Set_Combo_WHID(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_PRDT_WH_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.AlertByBiz("DA_BAS_SEL_PRDT_WH_CBO", Exception.Message, Exception.ToString());
                    return;
                }
                if (cbo.Name.Equals("cboWHID"))
                {
                    if (result.Rows.Count > 0)
                    {
                        if (result.Rows[0][1].Equals("SRS"))
                        {
                            result.Rows.RemoveAt(0);
                        }
                    }
                }
                cbo.ItemsSource = DataTableConverter.Convert(result);
                cbo.SelectedIndex = 0;
            });
        }
        private void CheckSelectRack(string RackID, string WHID)//입고
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RACKID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["RACKID"] = RackID;
                dr["WH_ID"] = WHID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_CHK_RACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    if (LOT.IsSelected)//탭분기
                    {
                        this.txtRackID.Text = RackID;
                        this.txtRackID.IsReadOnly = true;
                        this.cboWHID.IsEnabled = false;
                        this.txtLotid.Focus();
                    }
                    else if (SRS.IsSelected)
                    {
                        this.txtRackIDSRS.Text = RackID;
                        this.txtRackIDSRS.IsReadOnly = true;
                        this.cboWHIDSRS.IsEnabled = false;
                        this.txtBoxid.Focus();
                    }
                }
                else
                {
                    //20170417 RACK ID 스캔 시 RACK에 해당하는 창고가 자동으로 선택되게 수정(권병훈C 요청사항)
                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT1";
                    RQSTDT1.Columns.Add("RACK_ID", typeof(string));
                    RQSTDT1.Columns.Add("AREAID", typeof(string));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["RACK_ID"] = RackID;
                    dr1["AREAID"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT1.Rows.Add(dr1);

                    DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_BAS_CHK_RACK_INFO", "RQSTDT", "RSLTDT", RQSTDT1);

                    if (dtResult1.Rows.Count > 0)
                    {
                        if (LOT.IsSelected)//탭분기
                        {
                            this.txtRackID.Text = RackID;
                            this.txtRackID.IsReadOnly = true;
                            this.cboWHID.IsEnabled = false;
                            this.cboWHID.SelectedValue = dtResult1.Rows[0]["WH_ID"].ToString();
                            this.txtLotid.Focus();
                        }
                        else if (SRS.IsSelected)
                        {
                            this.txtRackIDSRS.Text = RackID;
                            this.txtRackIDSRS.IsReadOnly = true;
                            this.cboWHIDSRS.IsEnabled = false;
                            this.cboWHIDSRS.SelectedValue = dtResult1.Rows[0]["WH_ID"].ToString();
                            this.txtBoxid.Focus();
                        }
                    }
                    else
                    {
                        //{0}은(는) 창고에 없는 RACK ID 입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2894", new object[] { RackID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (LOT.IsSelected)//탭분기
                            {
                                this.txtRackID.IsReadOnly = false;
                                this.cboWHID.IsEnabled = true;
                                this.txtRackID.Clear();
                                this.txtRackID.Focus();
                            }
                            else if (SRS.IsSelected)
                            {
                                this.txtRackIDSRS.IsReadOnly = false;
                                this.cboWHIDSRS.IsEnabled = true;
                                this.txtRackIDSRS.Clear();
                                this.txtRackIDSRS.Focus();
                            }
                        });
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    if (LOT.IsSelected)//탭분기
                    {
                        this.txtRackID.Clear();
                        this.txtRackID.Focus();
                    }
                    else if (SRS.IsSelected)
                    {
                        this.txtRackIDSRS.Clear();
                        this.txtRackIDSRS.Focus();
                    }
                });
            }
        }
        private void CheckSelectRack(string RackID)//RACK변경
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RACKID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["RACKID"] = RackID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_CHK_RACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    _WH_ID = dtResult.Rows[0]["WH_ID"].ToString();
                    txtScanID.Focus();
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2890", new object[] { RackID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtRackIDRACK.Clear();
                        this.txtRackIDRACK.Focus();
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void CheckLotID_Biz(string LotID)
        {
            try
            {
                if (LotID == "")
                {
                    Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                    return;
                }
                
                //Grid 생성
                DataTable dtData = new DataTable();
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("SKIDID", typeof(string));
                dtData.Columns.Add("BOXID", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("PRODNAME", typeof(string));
                dtData.Columns.Add("PROCID", typeof(string));
                dtData.Columns.Add("WH_ID", typeof(string));
                dtData.Columns.Add("RACK_ID", typeof(string));
                dtData.AcceptChanges();

                if (LOT.IsSelected)//탭분기
                {
                    if (cboWHID.Text == "")
                    {
                        Util.MessageValidation("SFU2069");   //입고창고를 선택하세요.
                        return;
                    }

                    if (rdoCarrier.IsChecked == true)
                    {
                        string sLotID = SearhCarrierID(LotID);
                        if (string.IsNullOrEmpty(sLotID))
                        {
                            return;
                        }
                        LotID = sLotID;
                    }

                    if (dgReceive.ItemsSource != null)
                    {
                        dtData = DataTableConverter.Convert(dgReceive.ItemsSource);
                        //이미 추가한 LOT은 추가 안되도록
                        for (int icnt = 0; icnt < dtData.Rows.Count; icnt++)
                        {
                            if (dtData.Rows[icnt]["LOTID"].ToString() == LotID.Trim() || dtData.Rows[icnt]["SKIDID"].ToString() == LotID.Trim() || dtData.Rows[icnt]["BOXID"].ToString() == LotID.Trim())
                            {
                                //{0}은(는) 이미 스캔한 값 입니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2882", new object[] { LotID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    this.txtLotid.Clear();
                                    this.txtLotid.Focus();
                                });
                                return;
                            }
                        }
                    }
                }
                else if (SRS.IsSelected)
                {
                    if (cboWHIDSRS.Text == "")
                    {
                        Util.MessageValidation("SFU2069");   //입고창고를 선택하세요.
                        return;
                    }
                    if (dgReceiveSRS.ItemsSource != null)
                    {
                        dtData = DataTableConverter.Convert(dgReceiveSRS.ItemsSource);
                        //이미 추가한 LOT은 추가 안되도록
                        for (int icnt = 0; icnt < dtData.Rows.Count; icnt++)
                        {
                            if (dtData.Rows[icnt]["LOTID"].ToString() == LotID.Trim() || dtData.Rows[icnt]["SKIDID"].ToString() == LotID.Trim() || dtData.Rows[icnt]["BOXID"].ToString() == LotID.Trim())
                            {
                                //{0}은(는) 이미 스캔한 값 입니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2882", new object[] { LotID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                {
                                    this.txtLotid.Clear();
                                    this.txtLotid.Focus();
                                });
                                return;
                            }
                        }
                    }
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("FLAG", typeof(string));

                // CSR : C20211111-000238
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                if (LOT.IsSelected)//탭분기
                {
                    //dr["FLAG"] = rdoLot.IsChecked == true ? "LOT" : "SKID";
                    dr["FLAG"] = rdoSkid.IsChecked == true ? "SKID" : "LOT";
                }
                else if (SRS.IsSelected)
                {
                    dr["FLAG"] = "SRS";
                }

                // CSR : C20211111-000238
                dr["WH_ID"] = _WH_ID;
                dr["RACK_ID"] = _RACK_ID;

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                if (LOT.IsSelected)//탭분기
                {
                    if ((bool)rdoSkid.IsChecked)
                    {
                        dgReceive.Columns["SKIDID"].Visibility = Visibility.Visible;
                    }
                    else if ((bool)rdoLot.IsChecked)
                    {
                        dgReceive.Columns["SKIDID"].Visibility = Visibility.Collapsed;
                    }
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_IN_ELEC_LOT", "INDATA", "OUTDATA", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["LOTID"] = dsResult.Tables["OUTDATA"].Rows[i]["LOTID"].ToString();
                        newRow["SKIDID"] = dsResult.Tables["OUTDATA"].Rows[i]["SKIDID"].ToString();
                        newRow["BOXID"] = dsResult.Tables["OUTDATA"].Rows[i]["BOXID"].ToString();
                        newRow["PRODID"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODID"].ToString();
                        newRow["PRODNAME"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODNAME"].ToString();
                        newRow["PROCID"] = dsResult.Tables["OUTDATA"].Rows[i]["PROCID"].ToString();
                        newRow["WH_ID"] = Util.NVC(cboWHID.Text.ToString().Trim());
                        if (LOT.IsSelected)//탭분기
                        {
                            newRow["RACK_ID"] = Util.NVC(txtRackID.Text.Trim()) == "" ? null : Util.NVC(txtRackID.Text.Trim());
                        }
                        else if (SRS.IsSelected)
                        {
                            newRow["RACK_ID"] = Util.NVC(txtRackIDSRS.Text.Trim()) == "" ? null : Util.NVC(txtRackIDSRS.Text.Trim());
                        }

                        dtData.Rows.Add(newRow);
                    }
                    if (LOT.IsSelected)//탭분기
                    {
                        dgReceive.ItemsSource = DataTableConverter.Convert(dtData);
                        txtLotid.Clear();
                    }
                    else if (SRS.IsSelected)
                    {
                        dgReceiveSRS.ItemsSource = DataTableConverter.Convert(dtData);
                        txtBoxid.Clear();
                    }
                }
                else
                {
                    //{0}은(는) 입고불가한 LOT입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2893", new object[] { LotID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (LOT.IsSelected)//탭분기
                        {
                            this.txtLotid.Clear();
                            this.txtLotid.Focus();
                        }
                        else if (SRS.IsSelected)
                        {
                            this.txtBoxid.Clear();
                            this.txtBoxid.Focus();
                        }
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    if (LOT.IsSelected)//탭분기
                    {
                        this.txtLotid.Clear();
                        this.txtLotid.Focus();
                    }
                    else if (SRS.IsSelected)
                    {
                        this.txtBoxid.Clear();
                        this.txtBoxid.Focus();
                    }
                });
                return;
            }
        }

        private string SearhCarrierID(string sCarrierID)
        {
            string sLotID = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(sCarrierID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return sLotID;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sCarrierID.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                    return sLotID;
                }
                else
                {
                    sLotID = dtLot.Rows[0]["LOTID"].ToString();
                }

                return sLotID;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return "";
            }
        }

        private void CheckMoveID_Biz(string ScanID) //RACK변경
        {
            try
            {
                //Grid 생성
                DataTable dtData = new DataTable();
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("SKIDID", typeof(string));
                dtData.Columns.Add("BOXID", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("AS_RACKID", typeof(string));
                dtData.Columns.Add("TO_RACKID", typeof(string));
                dtData.Columns.Add("WH_ID", typeof(string));
                dtData.AcceptChanges();
                if (dgReceiveRACK.ItemsSource != null || dgReceiveRACK.Rows.Count > 0)
                {
                    dtData = DataTableConverter.Convert(dgReceiveRACK.ItemsSource);
                }

                if (rdoRackCARRIERID.IsChecked == true)
                {
                    string sLotID = SearhCarrierID(ScanID);
                    if (string.IsNullOrEmpty(sLotID))
                    {
                        return;
                    }
                    ScanID = sLotID;
                }

                //이미 추가한 LOT은 추가 안되도록
                for (int icnt = 0; icnt < dtData.Rows.Count; icnt++)
                {
                    if (dtData.Rows[icnt]["LOTID"].ToString() == ScanID.Trim() || dtData.Rows[icnt]["SKIDID"].ToString() == ScanID.Trim() || dtData.Rows[icnt]["BOXID"].ToString() == ScanID.Trim())
                    {
                        //{0}은(는) 이미 스캔한 값 입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2882", new object[] { ScanID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            this.txtScanID.Clear();
                            this.txtScanID.Focus();
                        });
                        return;
                    }
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("FLAG", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = ScanID;
                dr["FLAG"] = rdoRackSRS.IsChecked == true ? "SRS" : rdoRackSKID.IsChecked == true ? "SKID" : "LOT";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["WH_ID"] = _WH_ID;
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_MOVE_ELEC_LOT", "INDATA", "OUTDATA", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    //이미 해당 RACK에 적재된 LOT인지 체크
                    if (this.txtRackIDRACK.Text.ToString() == dsResult.Tables["OUTDATA"].Rows[0]["RACK_ID"].ToString())
                    {
                        //{0}은(는) 이미 해당 RACK에 적재되어있습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2892", new object[] { ScanID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            this.txtScanID.Clear();
                            this.txtScanID.Focus();
                        });
                        return;
                    }

                    #region C20211111-000238
                    DataTable dtRack = new DataTable();
                    dtRack.Columns.Add("WH_ID", typeof(string));
                    dtRack.Columns.Add("RACK_ID", typeof(string));

                    DataRow drRack = dtRack.NewRow();
                    drRack["WH_ID"] = _WH_ID;
                    drRack["RACK_ID"] = _RACK_ID;
                    dtRack.Rows.Add(drRack);

                    DataTable dtRsltChk = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MMD_PRDT_WH_INFO", "RQSTDT", "RSLTDT", dtRack);

                    if (dtRsltChk != null && dtRsltChk.Rows.Count > 0)
                    {
                        if (dtRsltChk.Rows[0]["ELTR_TYPE_CHK_FLAG"].ToString() == "Y")
                        {
                            for (int k = 0; k < dsResult.Tables["OUTDATA"].Rows.Count; k++)
                            {
                                if (dtRsltChk.Rows[0]["ELTR_TYPE_CODE"].ToString() != dsResult.Tables["OUTDATA"].Rows[k]["ELECTRODE"].ToString())
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("100244", new object[] { ScanID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                    {
                                        this.txtScanID.Clear();
                                        this.txtScanID.Focus();
                                    });
                                    return;
                                }
                            }
                        }
                    }
                    #endregion

                    for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["LOTID"] = dsResult.Tables["OUTDATA"].Rows[i]["LOTID"];
                        newRow["SKIDID"] = dsResult.Tables["OUTDATA"].Rows[i]["SKIDID"];
                        newRow["BOXID"] = dsResult.Tables["OUTDATA"].Rows[i]["BOXID"];
                        newRow["PRODID"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODID"];
                        newRow["AS_RACKID"] = dsResult.Tables["OUTDATA"].Rows[i]["RACK_ID"];
                        newRow["TO_RACKID"] = this.txtRackIDRACK.Text.Trim();
                        newRow["WH_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["WH_ID"];
                        dtData.Rows.Add(newRow);
                    }

                    dgReceiveRACK.ItemsSource = DataTableConverter.Convert(dtData);
                    this.txtScanID.Clear();
                    this.txtScanID.Focus();
                }
                else
                {
                    //{0}은(는) 유효하지 않은 LOT ID 입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2891", new object[] { ScanID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtScanID.Clear();
                        this.txtScanID.Focus();
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    this.txtScanID.Clear();
                    this.txtScanID.Focus();
                });
                return;
            }
        }
        private void Save_RTLS_Mapping_Condition()
        {
            try
            {
                // 결합
                string sCondition = "";
                sCondition = "RACK_IN";

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");

                inData.Columns.Add("CONDITION", typeof(string));
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("RACKID", typeof(string));

                if (LOT.IsSelected)
                {
                    for (int i = 0; i < dgReceive.GetRowCount(); i++)
                    {
                        DataRow row = inData.NewRow();
                        row["CONDITION"] = sCondition;
                        row["LOTID"] = DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTID").ToString();
                        row["USERID"] = LoginInfo.USERID;
                        row["RACKID"] = txtRackID != null ? Util.NVC(this.txtRackID.Text.Trim()) : string.Empty;

                        indataSet.Tables["INDATA"].Rows.Add(row);
                    }
                }
                else if (SRS.IsSelected)
                {
                    for (int i = 0; i < dgReceiveSRS.GetRowCount(); i++)
                    {
                        DataRow row = inData.NewRow();
                        row["CONDITION"] = sCondition;
                        row["LOTID"] = DataTableConverter.GetValue(dgReceiveSRS.Rows[i].DataItem, "LOTID").ToString();
                        row["USERID"] = LoginInfo.USERID;
                        row["RACKID"] = txtRackID != null ? Util.NVC(this.txtRackIDSRS.Text.Trim()) : string.Empty;

                        indataSet.Tables["INDATA"].Rows.Add(row);
                    }
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_RTLS_REG_MAPPING_BY_CONDITION", "INDATA", null, indataSet);

            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region Event
        private void txtRackID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                switch (TabInx.SelectedIndex)//선택된 탭에 따라 분기
                {
                    case 0:
                        CheckSelectRack(this.txtRackID.Text.Trim(), Util.NVC(cboWHID.SelectedValue.ToString()));
                        break;
                    case 1:
                        CheckSelectRack(this.txtRackIDSRS.Text.Trim(), Util.NVC(cboWHIDSRS.SelectedValue.ToString()));
                        break;
                    case 2:
                        //RackID가 없을 경우 실행 안되게..
                        if (string.IsNullOrEmpty(txtRackIDRACK.Text))
                        {
                            //스캔한 데이터가 없습니다.
                            Util.MessageInfo("SFU2060", (result) =>
                            {
                                this.txtRackIDRACK.Focus();
                            });
                            return;
                        }
                        CheckSelectRack(this.txtRackIDRACK.Text);
                        break;
                }
            }
        }
        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    // CSR : C20211111-000238
                    _WH_ID = Util.NVC(cboWHID.SelectedValue.ToString());
                    _RACK_ID = Util.NVC(txtRackID.Text.Trim());

                    CheckLotID_Biz(txtLotid.Text.Trim());
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }
        private void txtBoxid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    // CSR : C20211111-000238
                    _WH_ID = Util.NVC(cboWHIDSRS.SelectedValue.ToString());
                    _RACK_ID = Util.NVC(txtRackIDSRS.Text.Trim());

                    CheckLotID_Biz(txtBoxid.Text.Trim());
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }
        private void txtScanID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //RackID가 없을 경우 실행 안되게..
                if (string.IsNullOrEmpty(txtRackIDRACK.Text))
                {
                    //RACK ID를 먼저 스캔하세요.
                    Util.MessageInfo("SFU2843", (result) =>
                    {
                        this.txtScanID.Clear();
                        this.txtRackIDRACK.Focus();
                    });
                    return;
                }
                if (string.IsNullOrEmpty(txtScanID.Text))
                {
                    //스캔한 데이터가 없습니다.
                    Util.MessageInfo("SFU2060", (result) =>
                    {
                        this.txtScanID.Focus();
                    });
                    return;
                }

                // CSR : C20211111-000238
                _RACK_ID = Util.NVC(txtRackIDRACK.Text.Trim());

                CheckMoveID_Biz(txtScanID.Text);
            }
        }
        private void btnReceive_Click(object sender, RoutedEventArgs e)//입고처리
        {
            if (LOT.IsSelected)//탭분기
            {
                if (dgReceive.Rows.Count <= 0)
                {
                    //입고LOT정보가존재하지않습니다.
                    Util.MessageInfo("SFU2933", (Result) =>
                    {
                        txtLotid.Focus();
                    });
                    return;
                }
            }
            else if (SRS.IsSelected)
            {
                if (dgReceiveSRS.Rows.Count <= 0)
                {
                    //입고LOT정보가존재하지않습니다.
                    Util.MessageInfo("SFU2933", (Result) =>
                    {
                        txtBoxid.Focus();
                    });
                    return;
                }
            }

            //입고 하시겠습니까?
            Util.MessageConfirm("SFU2073", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("WH_ID", typeof(string));
                        inData.Columns.Add("RACK_ID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));


                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["WH_ID"] = LOT.IsSelected ? Util.NVC(cboWHID.SelectedValue.ToString()) : Util.NVC(cboWHIDSRS.SelectedValue.ToString());
                        if (LOT.IsSelected)//탭분기
                        {
                            row["RACK_ID"] = txtRackID.Text.Trim() == "" ? null : Util.NVC(txtRackID.Text.Trim());
                        }
                        else if (SRS.IsSelected)
                        {
                            row["RACK_ID"] = txtRackIDSRS.Text.Trim() == "" ? null : Util.NVC(txtRackIDSRS.Text.Trim());
                        }
                        row["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(row);


                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        if (LOT.IsSelected)//탭분기
                        {
                            for (int i = 0; i < dgReceive.GetRowCount(); i++)
                            {
                                row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTID"));
                                inLot.Rows.Add(row);
                            }
                        }
                        else if (SRS.IsSelected)
                        {
                            for (int i = 0; i < dgReceiveSRS.GetRowCount(); i++)
                            {
                                row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReceiveSRS.Rows[i].DataItem, "LOTID"));
                                inLot.Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_IN", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_STOCK_IN", bizException.Message, bizException.Message);
                                return;
                            }

                            Util.AlertInfo("SFU1798");   //입고 처리 되었습니다.
                            //this.Save_RTLS_Mapping_Condition(); //RTLS 대차매핑 처리 //20170328 주석처리
                            Initialize_dgReceive();

                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }
            });
        }
        private void btnChange_Click(object sender, RoutedEventArgs e)//RACK변경
        {
            try
            {
                DataSet inDataSet = null; //INLOT

                if (dgReceiveRACK.Rows.Count <= 0)
                {
                    //변경처리할 LOT 정보가 존재하지 않습니다.
                    Util.MessageInfo("SFU2874", (Result) =>
                    {
                        this.btnInitialize_Click(sender, e);
                    });
                    return;
                }
                #region INDATA
                inDataSet = new DataSet();

                DataTable INDATA = inDataSet.Tables.Add("INDATA"); // new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow newRow = null;
                DataTable dt = ((DataView)dgReceiveRACK.ItemsSource).Table;
                foreach (DataRow drr in dt.Rows)
                {
                    newRow = INDATA.NewRow();
                    newRow["SRCTYPE"] = "UI";
                    newRow["USERID"] = LoginInfo.USERID;
                    INDATA.Rows.Add(newRow);
                    newRow["LOTID"] = drr["LOTID"];
                    newRow["RACK_ID"] = drr["TO_RACKID"];
                }
                #endregion

                new ClientProxy().ExecuteService("BR_PRD_REG_STOCK_MOVE", "INDATA", null, INDATA, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        this.btnInitialize_Click(sender, e);
                        Util.AlertByBiz("BR_PRD_REG_STOCK_MOVE", Exception.Message, Exception.ToString());
                        return;
                    }
                    else
                    {
                        //정상처리되었습니다. 
                        Util.MessageInfo("SFU1275", (Result) =>
                        {
                            this.btnInitialize_Click(sender, e);
                        });
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                this.btnInitialize_Click(sender, e);
                Util.MessageException(ex);
                return;
            }
        }
        private void Initialize_dgReceive()
        {
            switch (TabInx.SelectedIndex)//선택된 탭에 따라 분기
            {
                case 0:
                    Util.gridClear(dgReceive);
                    cboWHID.IsEnabled = true;
                    txtRackID.Clear();
                    txtRackID.IsReadOnly = false;
                    txtLotid.Clear();
                    txtRackID.Focus();
                    break;
                case 1:
                    Util.gridClear(dgReceiveSRS);
                    cboWHIDSRS.IsEnabled = true;
                    txtRackIDSRS.Clear();
                    txtRackIDSRS.IsReadOnly = false;
                    txtBoxid.Clear();
                    txtRackIDSRS.Focus();
                    break;
                case 2:
                    Util.gridClear(dgReceiveRACK);
                    txtRackIDRACK.Clear();
                    txtRackIDRACK.IsReadOnly = false;
                    txtScanID.Clear();
                    txtRackIDRACK.Focus();
                    break;
            }
            return;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                    if (LOT.IsSelected)//탭분기
                    {
                        dgReceive.IsReadOnly = false;
                        dgReceive.RemoveRow(index);
                        dgReceive.IsReadOnly = true;
                    }
                    else if (SRS.IsSelected)
                    {
                        dgReceiveSRS.IsReadOnly = false;
                        dgReceiveSRS.RemoveRow(index);
                        dgReceiveSRS.IsReadOnly = true;
                    }
                    else if (RACKMOVE.IsSelected)
                    {
                        dgReceiveRACK.IsReadOnly = false;
                        dgReceiveRACK.RemoveRow(index);
                        dgReceiveRACK.IsReadOnly = true;
                    }
                }
            });
        }
        private void cboWHID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (LOT.IsSelected)
            {
                txtRackID.Focus();
            }
            else if (SRS.IsSelected)
            {
                txtRackIDSRS.Focus();
            }
        }
        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            Initialize_dgReceive();
        }
        #endregion

    }
}
