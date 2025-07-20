/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.02.05  정재홍    : [E20240104-001353] - 조회 Box RadioButton 추가 
 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_024 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();
        private string ACTID = "LOCATE_OUT_STORAGE";
        private string RESNGRID = "ELEC_WH_OUT_REASON";
        private string LOTID = string.Empty;
        private string HoldFlag = string.Empty;
        string HoldScanID = string.Empty;
        DataSet inDataSet = null; //INLOT
        decimal _WipQty = 0;
        string _RoutID = string.Empty;
        string _ProcID = string.Empty;
        string _FlowID = string.Empty;

        bool isMultiCopy = false;

        public ELEC001_024()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Initialize();
            //SetcboReasonCode();
            rdoLot.IsChecked = true;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOutput);
            listAuth.Add(btnOutputSRS);
            listAuth.Add(btnMovePC);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo combo = new CommonCombo();
            String[] sFilter1 = { LoginInfo.CFG_EQSG_ID };
            string[] sFilter2 = { "ALL" };
            //SHOP
            C1ComboBox[] cboShopChild = { cboArea, cboEquipmentSegment };
            combo.SetCombo(cboShop, CommonCombo.ComboStatus.NONE, cbChild: cboShopChild, sCase: "SHOPRELEATION");

            //동
            C1ComboBox[] cboAreaParent = { cboShop };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbParent: cboAreaParent, cbChild: cboAreaChild, sCase: "AREA_NO_AUTH", sFilter: sFilter2);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent);

            ////공정
            //combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilter1);
            //combo.SetCombo(cboProcessSRS, CommonCombo.ComboStatus.SELECT, cbChild: null, sFilter: sFilter1);

            SetEvent();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

            txtLotid.Focus();
        }

        #endregion

        #region Mehod
        //BizRule 사용
        private void CheckLotID_Biz(string LotID)
        {
            try
            {
                if (LotID == "")
                {
                    //스캔한 데이터가 없습니다.
                    Util.MessageInfo("SFU2060", (result) =>
                    {
                        this.txtLotid.Focus();
                    });
                    isMultiCopy = false;
                    return;
                }
                //Grid 생성
                DataTable dtData = new DataTable();
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("SKIDID", typeof(string));
                dtData.Columns.Add("BOXID", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("MODELNAME", typeof(string));
                dtData.Columns.Add("WH_ID", typeof(string));
                dtData.Columns.Add("RACK_ID", typeof(string));
                dtData.Columns.Add("WIPHOLD", typeof(string));
                dtData.AcceptChanges();

                if (dgOutput.ItemsSource != null)
                {
                    dtData = DataTableConverter.Convert(dgOutput.ItemsSource);
                }

                //Carrier ID로 LOT 검색
                if (rdoCARRIER.IsChecked == true)
                {
                    string sLotID = SearhCarrierID(LotID);

                    if (string.IsNullOrEmpty(sLotID))
                        return;
                    else
                        LotID = sLotID;
                }
                
                //이미 추가한 LOT은 추가 안되도록
                for (int icnt = 0; icnt < dtData.Rows.Count; icnt++)
                {
                    if (dtData.Rows[icnt]["LOTID"].ToString() == LotID.Trim() || dtData.Rows[icnt]["SKIDID"].ToString() == LotID.Trim())
                    {
                        //{0}은(는) 이미 스캔한 값 입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2882", new object[] { LotID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            this.txtLotid.Clear();
                            this.txtLotid.Focus();
                            
                        });
                        isMultiCopy = false;
                        return;
                    }
                }

                // CSR : [E20240104-001353] 주석처리 SKID & BOX 컬럼 표기
                /*
                if ((bool)rdoSkid.IsChecked)
                {
                    dgOutput.Columns["SKIDID"].Visibility = Visibility.Visible;
                }
                else if ((bool)rdoLot.IsChecked)
                {
                    dgOutput.Columns["SKIDID"].Visibility = Visibility.Collapsed;
                }
                */
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("RESNFLAG", typeof(string));
                //INDATA.Columns.Add("TO_PROCID", typeof(string));
                INDATA.Columns.Add("FLAG", typeof(string));
                INDATA.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("BR_TYPE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["RESNFLAG"] = ""; // (bool)this.chkemergency.IsChecked ? "Y" : "";
                //dr["TO_PROCID"] = Util.NVC(cboProcess.SelectedValue.ToString());
                //dr["FLAG"] = rdoLot.IsChecked == true ? "LOT" : "SKID";
                dr["FLAG"] = rdoSkid.IsChecked == true ? "SKID" : "LOT";
                dr["BLOCK_TYPE_CODE"] = "STOCK_OUT_STORAGE";    //NEW HOLD Type 변수
                dr["BR_TYPE"] = "P_ELEC";                       //OLD BR Search 변수

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                //BR_PRD_CHK_OUT_ELEC_LOT -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                //신규 HOLD 적용을 위해 변경 작업
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    //선입선출 위배여부 [Y: 정상, N:위배]
                    if (dsResult.Tables["OUTDATA"].Rows[0]["CHECK_FLAG"].ToString().Equals("N"))
                    {
                        //선입선출 위반입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("90069", null), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Mresult) =>
                        {
                            this.txtLotid.Clear();
                            this.txtLotid.Focus();
                            isMultiCopy = false;
                        });
                    }
                    else
                    {
                        for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                        {
                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["LOTID"] = dsResult.Tables["OUTDATA"].Rows[i]["LOTID"];
                            newRow["SKIDID"] = dsResult.Tables["OUTDATA"].Rows[i]["SKIDID"];
                            newRow["PRODID"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODID"];
                            newRow["MODELNAME"] = dsResult.Tables["OUTDATA"].Rows[i]["MODELNAME"];
                            newRow["WH_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["WH_ID"];
                            newRow["RACK_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["RACK_ID"];
                            newRow["WIPHOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["WIPHOLD"];
                            _WipQty = (decimal)dsResult.Tables["OUTDATA"].Rows[i]["WIPQTY"];
                            HoldFlag = dsResult.Tables["OUTDATA"].Rows[i]["WIPHOLD"].ToString();
                            dtData.Rows.Add(newRow);
                        }
                        dgOutput.ItemsSource = DataTableConverter.Convert(dtData);
                        this.txtLotid.Clear();
                        this.txtLotid.Focus();
                        isMultiCopy = true;
                    }
                }
                else
                {
                    //{0}은(는) 출고불가한 LOT입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2895", new object[] { LotID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtLotid.Clear();
                        this.txtLotid.Focus();
                    });
                    isMultiCopy = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                isMultiCopy = false;
                throw ex;
            }
        }

        /// <summary>
        /// CSR : E20240104-001353 
        /// Boxid는 별도 BR 호출
        /// </summary>
        /// <param name="LotID"></param>
        private void CheckBoxID_Biz(string LotID)
        {
            try
            {
                if (LotID == "")
                {
                    //스캔한 데이터가 없습니다.
                    Util.MessageInfo("SFU2060", (result) =>
                    {
                        this.txtLotid.Focus();
                    });
                    isMultiCopy = false;
                    return;
                }

                //Grid 생성
                DataTable dtData = new DataTable();
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("SKIDID", typeof(string));
                dtData.Columns.Add("BOXID", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("MODELNAME", typeof(string));
                dtData.Columns.Add("WH_ID", typeof(string));
                dtData.Columns.Add("RACK_ID", typeof(string));
                dtData.Columns.Add("WIPHOLD", typeof(string));
                dtData.AcceptChanges();

                if (dgOutput.ItemsSource != null)
                {
                    dtData = DataTableConverter.Convert(dgOutput.ItemsSource);
                }

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
                        isMultiCopy = false;
                        return;
                    }
                }
                
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("RESNFLAG", typeof(string));
                INDATA.Columns.Add("FLAG", typeof(string));
                INDATA.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("BR_TYPE", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["RESNFLAG"] = ""; 
                dr["FLAG"] = "LOT";
                dr["BLOCK_TYPE_CODE"] = "STOCK_OUT_STORAGE";    //NEW HOLD Type 변수
                dr["BR_TYPE"] = "P_ELEC";                       //OLD BR Search 변수
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);
                
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_OUT_ELEC_BOX", "INDATA", "OUTDATA", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    //선입선출 위배여부 [Y: 정상, N:위배]
                    if (dsResult.Tables["OUTDATA"].Rows[0]["CHECK_FLAG"].ToString().Equals("N"))
                    {
                        //선입선출 위반입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("90069", null), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Mresult) =>
                        {
                            this.txtLotid.Clear();
                            this.txtLotid.Focus();
                            isMultiCopy = false;
                        });
                    }
                    else
                    {
                        for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                        {
                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["LOTID"] = dsResult.Tables["OUTDATA"].Rows[i]["LOTID"];
                            newRow["SKIDID"] = dsResult.Tables["OUTDATA"].Rows[i]["SKIDID"];
                            newRow["BOXID"] = dsResult.Tables["OUTDATA"].Rows[i]["BOXID"];
                            newRow["PRODID"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODID"];
                            newRow["MODELNAME"] = dsResult.Tables["OUTDATA"].Rows[i]["MODELNAME"];
                            newRow["WH_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["WH_ID"];
                            newRow["RACK_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["RACK_ID"];
                            newRow["WIPHOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["WIPHOLD"];
                            _WipQty = (decimal)dsResult.Tables["OUTDATA"].Rows[i]["WIPQTY"];
                            HoldFlag = dsResult.Tables["OUTDATA"].Rows[i]["WIPHOLD"].ToString();
                            dtData.Rows.Add(newRow);
                        }
                        dgOutput.ItemsSource = DataTableConverter.Convert(dtData);
                        this.txtLotid.Clear();
                        this.txtLotid.Focus();
                        isMultiCopy = true;
                    }
                }
                else
                {
                    //{0}은(는) 출고불가한 LOT입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2895", new object[] { LotID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtLotid.Clear();
                        this.txtLotid.Focus();
                    });
                    isMultiCopy = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                isMultiCopy = false;
                throw ex;
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
                return null;
            }
        }
        private void CheckSRSLotID_Biz(string BoxID)
        {
            try
            {
                if (BoxID == "")
                {
                    //스캔한 데이터가 없습니다.
                    Util.MessageInfo("SFU2060", (result) =>
                    {
                        this.txtSRSLotid.Focus();
                    });  
                    return;
                }
                //Grid 생성
                DataTable dtData = new DataTable();
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("BOXID", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("PRODNAME", typeof(string));
                dtData.Columns.Add("WH_ID", typeof(string));
                dtData.Columns.Add("RACK_ID", typeof(string));
                dtData.Columns.Add("WIPHOLD", typeof(string));
                dtData.AcceptChanges();

                if (dgOutputSRS.ItemsSource != null)
                {
                    dtData = DataTableConverter.Convert(dgOutputSRS.ItemsSource);
                }
                //이미 추가한 LOT은 추가 안되도록
                for (int icnt = 0; icnt < dtData.Rows.Count; icnt++)
                {
                    if (dtData.Rows[icnt]["LOTID"].ToString() == BoxID.Trim() || dtData.Rows[icnt]["BOXID"].ToString() == BoxID.Trim())
                    {
                        //{0}은(는) 이미 스캔한 값 입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2882", new object[] { BoxID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            this.txtSRSLotid.Clear();
                            this.txtSRSLotid.Focus();
                        });
                        return;
                    }
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("RESNFLAG", typeof(string));
                //INDATA.Columns.Add("TO_PROCID", typeof(string));
                INDATA.Columns.Add("FLAG", typeof(string));
                INDATA.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("BR_TYPE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = BoxID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["RESNFLAG"] = "";// (bool)this.chkemergencySRS.IsChecked ? "Y" : "";
                //dr["TO_PROCID"] = Util.NVC(cboProcessSRS.SelectedValue.ToString());
                dr["FLAG"] = "SRS";
                dr["BLOCK_TYPE_CODE"] = "STOCK_OUT_STORAGE";    //NEW HOLD Type 변수
                dr["BR_TYPE"] = "P_ELEC";                       //OLD BR Search 변수

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                //BR_PRD_CHK_OUT_ELEC_LOT -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                //신규 HOLD 적용을 위해 변경 작업
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                    { 
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["LOTID"] = dsResult.Tables["OUTDATA"].Rows[i]["LOTID"];
                        newRow["BOXID"] = dsResult.Tables["OUTDATA"].Rows[i]["BOXID"];
                        newRow["PRODID"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODID"];
                        newRow["PRODNAME"] = dsResult.Tables["OUTDATA"].Rows[i]["PRODNAME"];
                        newRow["WH_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["WH_ID"];
                        newRow["RACK_ID"] = dsResult.Tables["OUTDATA"].Rows[i]["RACK_ID"];
                        newRow["WIPHOLD"] = dsResult.Tables["OUTDATA"].Rows[i]["WIPHOLD"];
                        _WipQty = (decimal)dsResult.Tables["OUTDATA"].Rows[i]["WIPQTY"];
                        dtData.Rows.Add(newRow);
                    }
                    dgOutputSRS.ItemsSource = DataTableConverter.Convert(dtData);
                    this.txtSRSLotid.Text = "";

                }
                else
                {
                    //{0}은(는) 출고불가한 LOT입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2895", new object[] { BoxID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtSRSLotid.Clear();
                        this.txtSRSLotid.Focus();
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;             
            }
        }
        private void CheckPancakeID_Biz(string PancakeID)
        {
            try
            {
                if (PancakeID == "")
                {
                    //스캔한 데이터가 없습니다.
                    Util.MessageInfo("SFU2060", (result) =>
                    {
                        this.txtPancakeID.Focus();
                    });
                    return;
                }
                //Grid 생성
                DataTable dtData = new DataTable();
                dtData.Columns.Add("CHK", typeof(string));
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("WIPQTY", typeof(string));
                dtData.Columns.Add("WIPQTY2", typeof(string));
                dtData.Columns.Add("PROCID", typeof(string));
                dtData.Columns.Add("WIPHOLD", typeof(string));
                dtData.AcceptChanges();

                for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "LOTID").ToString() == PancakeID)
                    {
                        //동일한 LOT이 스캔되었습니다.
                        Util.MessageInfo("SFU1504", (result) =>
                        {
                            this.txtPancakeID.Clear();
                            this.txtPancakeID.Focus();
                        });
                        return;
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("RESNFLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = PancakeID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["RESNFLAG"] = "";// (bool)this.chkemergencyMove.IsChecked ? "Y" : "";

                RQSTDT.Rows.Add(dr);
                //DA_BAS_SEL_WIP_WITH_ATTR_MOVE
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_MOVE_NOTCHING_ELEC_LOT", "RQSTDT", "RSLTDT", RQSTDT); //DA_BAS_SEL_WIP_WITH_ATTR_MOVE

                if (SearchResult.Rows.Count < 1)
                {
                    //{0}의 재공정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2883", new object[] { PancakeID }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtPancakeID.Clear();
                        this.txtPancakeID.Focus();
                    });
                    return;
                }
                else
                {
                    if (dgOutputPC.ItemsSource != null && dgOutputPC.Rows.Count > 0)
                    {
                        if (DataTableConverter.GetValue(dgOutputPC.Rows[0].DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                        {
                            //제품ID가 같지 않습니다.
                            Util.MessageInfo("SFU1893", (result) =>
                            {
                                this.txtPancakeID.Clear();
                                this.txtPancakeID.Focus();
                            });
                            return;
                        }
                        if (DataTableConverter.GetValue(dgOutputPC.Rows[0].DataItem, "PROCID").ToString() != SearchResult.Rows[0]["PROCID"].ToString())
                        {
                            //같은공정의 LOT이 아닙니다.
                            Util.MessageInfo("SFU2853", (result) =>
                            {
                                this.txtPancakeID.Clear();
                                this.txtPancakeID.Focus();
                            });
                            return;
                        }
                        else
                        {
                            dtData = DataTableConverter.Convert(dgOutputPC.ItemsSource);
                        }
                    }
                    if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                    {
                        //이동가능한 LOT이 아닙니다.
                        Util.MessageInfo("SFU2925", (result) =>
                        {
                            this.txtPancakeID.Clear();
                            this.txtPancakeID.Focus();
                        });
                        return;
                    }
                    if (SearchResult.Rows[0]["WH_ID"].ToString() == "")
                    {
                        //창고에 입고되지않은 LOT입니다.
                        Util.MessageInfo("SFU2962", (result) =>
                        {
                            this.txtPancakeID.Clear();
                            this.txtPancakeID.Focus();
                        });
                        return;
                    }

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["CHK"] = SearchResult.Rows[0]["CHK"].ToString();
                    newRow["LOTID"] = SearchResult.Rows[0]["LOTID"].ToString();
                    newRow["PRODID"] = SearchResult.Rows[0]["PRODID"].ToString();
                    newRow["WIPQTY"] = SearchResult.Rows[0]["WIPQTY"].ToString();
                    newRow["WIPQTY2"] = SearchResult.Rows[0]["WIPQTY2"].ToString();
                    newRow["PROCID"] = SearchResult.Rows[0]["PROCID"].ToString();
                    newRow["WIPHOLD"] = SearchResult.Rows[0]["WIPHOLD"].ToString();
                    _WipQty = (decimal)SearchResult.Rows[0]["WIPQTY"];
                    dtData.Rows.Add(newRow);
                    dgOutputPC.ItemsSource = DataTableConverter.Convert(dtData);

                    _RoutID = SearchResult.Rows[0]["ROUTID"].ToString();
                    _ProcID = SearchResult.Rows[0]["PROCID"].ToString();
                    _FlowID = SearchResult.Rows[0]["FLOWID"].ToString();

                    this.txtPancakeID.Clear();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //INLOT
        private DataTable _InLot()
        {
            DataTable dt = null;
            switch (TabInx.SelectedIndex)//선택된 탭에 따라 분기
            {
                case 0:
                    dt = ((DataView)dgOutput.ItemsSource).Table;
                    break;
                case 1:
                    dt = ((DataView)dgOutputSRS.ItemsSource).Table;
                    break;
                case 2:
                    dt = ((DataView)dgOutputPC.ItemsSource).Table;
                    break;
            }
            DataTable IndataTable = inDataSet.Tables.Add("INLOT");
            IndataTable.Columns.Add("LOTID", typeof(string));
            foreach (DataRow dr in dt.Rows)
            {
                dr["LOTID"] = dr["LOTID"];
                IndataTable.ImportRow(dr);
            }
            return IndataTable;
        }
        //private DataTable _InResn()
        //{
        //    DataTable dt = null;
        //    DataTable IndataTable = inDataSet.Tables.Add("INRESN");
        //    IndataTable.Columns.Add("RESNCODE", typeof(string));
        //    IndataTable.Columns.Add("RESNQTY", typeof(decimal));
        //    IndataTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
        //    IndataTable.Columns.Add("PROCID_CAUSE", typeof(string));
        //    IndataTable.Columns.Add("RESNNOTE", typeof(string));
        //    DataRow dr = IndataTable.NewRow();
        //    switch (TabInx.SelectedIndex)//선택된 탭에 따라 분기
        //    {
        //        case 0:
        //            dt = ((DataView)dgOutput.ItemsSource).Table;
        //            dr["RESNCODE"] = cboReasonCode.SelectedValue.ToString().Trim();
        //            dr["RESNQTY"] = _WipQty;
        //            dr["RESNCODE_CAUSE"] = null;
        //            dr["PROCID_CAUSE"] = null;
        //            dr["RESNNOTE"] = null;
        //            break;
        //        case 1:
        //            dt = ((DataView)dgOutputSRS.ItemsSource).Table;
        //            dr["RESNCODE"] = cboReasonCodeSRS.SelectedValue.ToString().Trim();
        //            dr["RESNQTY"] = _WipQty;
        //            dr["RESNCODE_CAUSE"] = null;
        //            dr["PROCID_CAUSE"] = null;
        //            dr["RESNNOTE"] = null;
        //            break;
        //        case 2:
        //            dt = ((DataView)dgOutputPC.ItemsSource).Table;
        //            dr["RESNCODE"] = cboReasonCodeMove.SelectedValue.ToString().Trim();
        //            dr["RESNQTY"] = _WipQty;
        //            dr["RESNCODE_CAUSE"] = null;
        //            dr["PROCID_CAUSE"] = null;
        //            dr["RESNNOTE"] = null;
        //            break;
        //    }
        //    IndataTable.Rows.Add(dr);
        //    return IndataTable;
        //}
        private void Save_RTLS_Mapping_Condition()
        {
            try
            {
                string sCondition = "";
                string sZoneID = "";

                // 해체
                sCondition = "RACK_OUT";

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");

                inData.Columns.Add("CONDITION", typeof(string));
                inData.Columns.Add("CART_NO", typeof(string));
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                switch (TabInx.SelectedIndex)//선택된 탭에 따라 분기
                {
                    case 0:
                        for (int i = 0; i < dgOutput.GetRowCount(); i++)
                        {
                            DataRow row = inData.NewRow();
                            row["CONDITION"] = sCondition;
                            row["CART_NO"] = sZoneID; //DataTableConverter.GetValue(dgData.Rows[i].DataItem, "CART_NO").ToString(); //수정필요
                            row["LOTID"] = DataTableConverter.GetValue(dgOutput.Rows[i].DataItem, "LOTID").ToString();
                            row["USERID"] = LoginInfo.USERID;

                            indataSet.Tables["INDATA"].Rows.Add(row);
                        }
                        break;
                    case 1:
                        for (int i = 0; i < dgOutputSRS.GetRowCount(); i++)
                        {
                            DataRow row = inData.NewRow();
                            row["CONDITION"] = sCondition;
                            row["CART_NO"] = sZoneID; //DataTableConverter.GetValue(dgData.Rows[i].DataItem, "CART_NO").ToString(); //수정필요
                            row["LOTID"] = DataTableConverter.GetValue(dgOutputSRS.Rows[i].DataItem, "LOTID").ToString();
                            row["USERID"] = LoginInfo.USERID;

                            indataSet.Tables["INDATA"].Rows.Add(row);
                        }
                        break;
                    case 2:
                        for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
                        {
                            DataRow row = inData.NewRow();
                            row["CONDITION"] = sCondition;
                            row["CART_NO"] = sZoneID; //DataTableConverter.GetValue(dgData.Rows[i].DataItem, "CART_NO").ToString(); //수정필요
                            row["LOTID"] = DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "LOTID").ToString();
                            row["USERID"] = LoginInfo.USERID;

                            indataSet.Tables["INDATA"].Rows.Add(row);
                        }
                        break;
                }
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_RTLS_REG_MAPPING_BY_CONDITION", "INDATA", null, indataSet);
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private bool WipHold()
        {
            switch (TabInx.SelectedIndex)//선택된 탭에 따라 분기
            {
                case 0:
                    for (int i = 0; i < dgOutput.GetRowCount(); i++)
                    {
                        HoldFlag = DataTableConverter.GetValue(dgOutput.Rows[i].DataItem, "WIPHOLD").ToString();
                        if (HoldFlag == "Y")
                        {
                            HoldScanID += DataTableConverter.GetValue(dgOutput.Rows[i].DataItem, "LOTID").ToString() + ", ";
                        }
                    }
                    break;
                case 1:
                    for (int i = 0; i < dgOutputSRS.GetRowCount(); i++)
                    {
                        HoldFlag = DataTableConverter.GetValue(dgOutputSRS.Rows[i].DataItem, "WIPHOLD").ToString();
                        if (HoldFlag == "Y")
                        {
                            HoldScanID += DataTableConverter.GetValue(dgOutputSRS.Rows[i].DataItem, "LOTID").ToString() + ", ";
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
                    {
                        HoldFlag = DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPHOLD").ToString();
                        if (HoldFlag == "Y")
                        {
                            HoldScanID += DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "LOTID").ToString() + ", ";
                        }
                    }
                    break;
            }
            if (HoldScanID != "")
            {
                HoldScanID = HoldScanID.Remove(HoldScanID.Length - 2);
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region Event
        //사유코드 콤보 셋팅
        //private void SetcboReasonCode()
        //{
        //    try
        //    {
        //        DataTable INDATA = new DataTable();
        //        INDATA.TableName = "INDATA";
        //        INDATA.Columns.Add("LANGID", typeof(string));
        //        INDATA.Columns.Add("ACTID", typeof(string));
        //        INDATA.Columns.Add("RESNGRID", typeof(string));

        //        DataRow dr = INDATA.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["ACTID"] = ACTID;
        //        dr["RESNGRID"] = RESNGRID;
        //        INDATA.Rows.Add(dr);

        //        new ClientProxy().ExecuteService("COR_SEL_ACTIVITYREASON_CBO", "INDATA", "OUTDATA", INDATA, (dtResult, Exception) =>
        //        {
        //            if (Exception != null)
        //            {
        //                Util.AlertByBiz("COR_SEL_ACTIVITYREASON_CBO", Exception.Message, Exception.ToString());
        //                return;
        //            }
        //            cboReasonCode.DisplayMemberPath = "RESNNAME";
        //            cboReasonCode.SelectedValuePath = "RESNCODE";
        //            cboReasonCode.ItemsSource = DataTableConverter.Convert(dtResult);
        //            cboReasonCode.SelectedIndex = 0;

        //            cboReasonCodeSRS.DisplayMemberPath = "RESNNAME";
        //            cboReasonCodeSRS.SelectedValuePath = "RESNCODE";
        //            cboReasonCodeSRS.ItemsSource = DataTableConverter.Convert(dtResult);
        //            cboReasonCodeSRS.SelectedIndex = 0;

        //            cboReasonCodeMove.DisplayMemberPath = "RESNNAME";
        //            cboReasonCodeMove.SelectedValuePath = "RESNCODE";
        //            cboReasonCodeMove.ItemsSource = DataTableConverter.Convert(dtResult);
        //            cboReasonCodeMove.SelectedIndex = 0;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //        return;
        //    }
        //}

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    e.Handled = true;
                    if ((bool)rdoBox.IsChecked)
                    {
                        CheckBoxID_Biz(txtLotid.Text.Trim());
                    }
                    else
                    {
                        CheckLotID_Biz(txtLotid.Text.Trim());
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }
        private void txtSRSLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    e.Handled = true;
                    CheckSRSLotID_Biz(txtSRSLotid.Text.Trim());
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }
        private void txtPancakeID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    e.Handled = true;
                    CheckPancakeID_Biz(txtPancakeID.Text.Trim());
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }
        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            if (dgOutput.Rows.Count <= 0)
            {
                //출고처리할 LOT 정보가 존재하지 않습니다.
                Util.AlertInfo("SFU2967");
                return;
            }

            #region INDATA
            inDataSet = new DataSet();

            DataTable INDATA = inDataSet.Tables.Add("INDATA"); // new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("SRCTYPE", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["SRCTYPE"] = "UI";
            dr["USERID"] = LoginInfo.USERID;
            INDATA.Rows.Add(dr);
            #endregion

            #region INLOT
            DataTable InLot = _InLot();
            #endregion

            #region INRESN
            //if ((bool)chkemergency.IsChecked)
            //{
            //    DataTable InResn = _InResn();
            //}
            #endregion

            if (!WipHold())//Hold Lot이 있을 경우
            {
                //Hold상태 반제품입니다. 출고하시겠습니까?
                Util.MessageConfirm("SFU3132", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_OUT", "INDATA,INLOT", null, (Bizresult, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_STOCK_OUT", Exception.Message, Exception.ToString());
                                    return;
                                }
                                else
                                {
                                    //정상처리되었습니다.
                                    Util.MessageInfo("SFU1275", (Result) =>
                                    {
                                        //this.Save_RTLS_Mapping_Condition();//RTLS 대차매핑 처리 //20170328 주석처리
                                        this.Initialize_dgReceive();
                                    });
                                }
                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                    else
                    {
                        this.HoldScanID = string.Empty;
                        this.txtLotid.Clear();
                        this.txtLotid.Focus();
                        return;
                    }
                }, new object[] { Util.NVC(HoldScanID) });
            }
            else
            {
                //출고 하시겠습니까?
                Util.MessageConfirm("SFU3121", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_OUT", "INDATA,INLOT,INRESN", null, (Bizresult, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_STOCK_OUT", Exception.Message, Exception.ToString());
                                    return;
                                }
                                else
                                {
                                    //정상처리되었습니다.
                                    Util.MessageInfo("SFU1275", (Result) =>
                                    {
                                        //this.Save_RTLS_Mapping_Condition();//RTLS 대차매핑 처리 //20170328 주석처리
                                        this.Initialize_dgReceive();
                                    });
                                }
                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });
            }
        }
        private void btnOutputSRS_Click(object sender, RoutedEventArgs e)
        {
            if (dgOutputSRS.Rows.Count <= 0)
            {
                //출고처리할 LOT 정보가 존재하지 않습니다.
                Util.AlertInfo("SFU2967");
                return;
            }

            #region INDATA
            inDataSet = new DataSet();

            DataTable INDATA = inDataSet.Tables.Add("INDATA"); // new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("SRCTYPE", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["SRCTYPE"] = "UI";
            dr["USERID"] = LoginInfo.USERID;
            INDATA.Rows.Add(dr);
            #endregion

            #region INLOT
            DataTable InLot = _InLot();
            #endregion

            #region INRESN
            //if ((bool)chkemergencySRS.IsChecked)
            //{
            //    DataTable InResn = _InResn();
            //}
            #endregion

            if (!WipHold())//Hold Lot이 있을 경우
            {
                //Hold상태 반제품입니다. 출고하시겠습니까?
                Util.MessageConfirm("SFU3132", (Mresult) =>
                {
                    if (Mresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_OUT", "INDATA,INLOT", null, (Bizresult, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_STOCK_OUT", Exception.Message, Exception.ToString());
                                    return;
                                }
                                else
                                {
                                    //정상처리되었습니다.
                                    Util.MessageInfo("SFU1275", (Result) =>
                                    {
                                        //this.Save_RTLS_Mapping_Condition();//RTLS 대차매핑 처리 //20170328 주석처리
                                        this.Initialize_dgReceive();
                                    });
                                }
                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                    else
                    {
                        HoldScanID = string.Empty;
                        return;
                    }
                }, new object[] { Util.NVC(HoldScanID) });
            }
            else
            {
                //출고 하시겠습니까?
                Util.MessageConfirm("SFU3121", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_OUT", "INDATA,INLOT,INRESN", null, (Bizresult, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_STOCK_OUT", Exception.Message, Exception.ToString());
                                    return;
                                }
                                else
                                {
                                    //정상처리되었습니다.
                                    Util.MessageInfo("SFU1275", (Result) =>
                                    {
                                        //this.Save_RTLS_Mapping_Condition();//RTLS 대차매핑 처리 //20170328 주석처리
                                        this.Initialize_dgReceive();
                                    });
                                }
                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }

                    }
                });
            }
           
        }
        private void btnMovePC_Click(object sender, RoutedEventArgs e)//노칭이동
        {
            if (dgOutputPC.Rows.Count <= 0)
            {
                //출고처리할 LOT 정보가 존재하지 않습니다.
                Util.AlertInfo("SFU2967");
                return;
            }
            string sToShop = string.Empty;
            string sToArea = string.Empty;
            string sToEqsg = string.Empty;

            sToShop = Util.GetCondition(cboShop, MessageDic.Instance.GetMessage("SFU1424"));    //Shop을 선택하세요.
            sToArea = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));    //동을 선택하세요.
            sToEqsg = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));    //라인을 선택하세요.

            double dSum = 0;
            double dSum2 = 0;
            double dTotal = 0;
            double dTotal2 = 0;

            for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
            {
                dSum = Double.Parse(Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPQTY")));
                dSum2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPQTY2")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPQTY2")));

                dTotal = dTotal + dSum;
                dTotal2 = dTotal2 + dSum2;
            }

            #region INDATA
            inDataSet = new DataSet();

            DataTable INDATA = inDataSet.Tables.Add("INDATA"); // new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("SRCTYPE", typeof(string));
            INDATA.Columns.Add("PRODID", typeof(string));
            INDATA.Columns.Add("FROM_SHOPID", typeof(string));
            INDATA.Columns.Add("FROM_AREAID", typeof(string));
            INDATA.Columns.Add("FROM_EQSGID", typeof(string));
            INDATA.Columns.Add("FROM_PROCID", typeof(string));
            INDATA.Columns.Add("FROM_PCSGID", typeof(string));
            INDATA.Columns.Add("TO_SHOPID", typeof(string));
            INDATA.Columns.Add("TO_AREAID", typeof(string));
            INDATA.Columns.Add("TO_EQSGID", typeof(string));
            INDATA.Columns.Add("TO_SLOC_ID", typeof(string));
            INDATA.Columns.Add("MOVE_ORD_QTY", typeof(string));
            INDATA.Columns.Add("MOVE_ORD_QTY2", typeof(string));
            INDATA.Columns.Add("INTRANSIT_FLAG", typeof(string));
            INDATA.Columns.Add("NOTE", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["SRCTYPE"] = "UI";
            dr["PRODID"] = DataTableConverter.GetValue(dgOutputPC.Rows[0].DataItem, "PRODID").ToString();
            dr["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["FROM_EQSGID"] = LoginInfo.CFG_EQSG_ID;
            dr["FROM_PROCID"] = LoginInfo.CFG_PROC_ID;
            dr["FROM_PCSGID"] = "E";
            dr["TO_SHOPID"] = sToShop;
            dr["TO_AREAID"] = sToArea;
            dr["TO_EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["TO_SLOC_ID"] = null;
            dr["MOVE_ORD_QTY"] = dTotal;
            dr["MOVE_ORD_QTY2"] = dTotal2;
            dr["INTRANSIT_FLAG"] = "Y";
            dr["NOTE"] = null;
            dr["USERID"] = LoginInfo.USERID;
            INDATA.Rows.Add(dr);
            #endregion

            #region INLOT
            DataTable InLot = _InLot();
            #endregion

            #region INRESN
            //if ((bool)chkemergencyMove.IsChecked)
            //{
            //    DataTable InResn = _InResn();
            //}
            #endregion

            if (!WipHold())//Hold Lot이 있을 경우
            {
                //Hold상태 반제품입니다. 출고하시겠습니까?
                Util.MessageConfirm("SFU3132", (Mresult) =>
                {
                    if (Mresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            //BR_PRD_REG_SEND_PACKLOT_SHOP -> BR_PRD_REG_STOCK_PANCAKE_OUT
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_PANCAKE_OUT", "INDATA,INLOT", null, (Bizresult, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                                    {
                                        this.txtPancakeID.Focus();
                                    });
                                    return;
                                }
                                else
                                {
                                    //정상처리되었습니다. 
                                    Util.MessageInfo("SFU1275", (Result) =>
                                    {
                                        //this.Save_RTLS_Mapping_Condition();//RTLS 대차매핑 처리 //20170328 주석처리
                                        this.Initialize_dgReceive();
                                    });
                                }
                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                    else
                    {
                        HoldScanID = string.Empty;
                        return;
                    }
                }, new object[] { Util.NVC(HoldScanID) });
            }
            else
            {
                //출고 하시겠습니까?
                Util.MessageConfirm("SFU3121", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            //BR_PRD_REG_SEND_PACKLOT_SHOP -> BR_PRD_REG_STOCK_PANCAKE_OUT
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_PANCAKE_OUT", "INDATA,INLOT,INRESN", null, (Bizresult, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                                    Util.MessageInfo(Exception.Message, (Result) =>
                                    {
                                        this.txtPancakeID.Focus();
                                    });
                                    return;
                                }
                                else
                                {
                                    //정상처리되었습니다. 
                                    Util.MessageInfo("SFU1275", (Result) =>
                                    {
                                        //this.Save_RTLS_Mapping_Condition();//RTLS 대차매핑 처리 //20170328 주석처리
                                        this.Initialize_dgReceive();
                                    });
                                }
                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }

                    }
                });
            }

        }
        private void Initialize_dgReceive()
        {
            switch (TabInx.SelectedIndex)//선택된 탭에 따라 분기
            {
                case 0:
                    Util.gridClear(dgOutput);
                    txtLotid.Text = null;
                    txtLotid.Focus();
                    HoldScanID = string.Empty;
                    break;
                case 1:
                    Util.gridClear(dgOutputSRS);
                    txtSRSLotid.Text = null;
                    txtSRSLotid.Focus();
                    HoldScanID = string.Empty;
                    break;
                case 2:
                    Util.gridClear(dgOutputPC);
                    txtPancakeID.Text = null;
                    txtPancakeID.Focus();
                    HoldScanID = string.Empty;
                    break;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                    switch (TabInx.SelectedIndex)//선택된 탭에 따라 분기
                    {
                        case 0:
                            dgOutput.IsReadOnly = false;
                            dgOutput.RemoveRow(index);
                            dgOutput.IsReadOnly = true;
                            break;
                        case 1:
                            dgOutputSRS.IsReadOnly = false;
                            dgOutputSRS.RemoveRow(index);
                            dgOutputSRS.IsReadOnly = true;
                            break;
                        case 2:
                            dgOutputPC.IsReadOnly = false;
                            dgOutputPC.RemoveRow(index);
                            dgOutputPC.IsReadOnly = true;
                            break;
                    }
                }
            });
        }
        
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.txtPancakeID.Focus();
        }
        #endregion
        
        private void rdoLot_Checked(object sender, RoutedEventArgs e)
        {
            tbLotID.Text = ObjectDic.Instance.GetObjectName("Lot ID");
            dgOutput.Columns["SKIDID"].Visibility = Visibility.Collapsed;
            dgOutput.Columns["BOXID"].Visibility = Visibility.Collapsed;
            Util.gridClear(dgOutput);
        }

        private void rdoSkid_Checked(object sender, RoutedEventArgs e)
        {
            tbLotID.Text = ObjectDic.Instance.GetObjectName("Skid ID");
            dgOutput.Columns["SKIDID"].Visibility = Visibility.Visible;
            dgOutput.Columns["BOXID"].Visibility = Visibility.Collapsed;
            Util.gridClear(dgOutput);
        }

        private void rdoCARRIER_Checked(object sender, RoutedEventArgs e)
        {
            tbLotID.Text = ObjectDic.Instance.GetObjectName("Carrier ID");
            dgOutput.Columns["SKIDID"].Visibility = Visibility.Collapsed;
            dgOutput.Columns["BOXID"].Visibility = Visibility.Collapsed;
            Util.gridClear(dgOutput);
        }

        private void rdoBox_Checked(object sender, RoutedEventArgs e)
        {
            tbLotID.Text = ObjectDic.Instance.GetObjectName("Box ID");
            dgOutput.Columns["SKIDID"].Visibility = Visibility.Collapsed;
            dgOutput.Columns["BOXID"].Visibility = Visibility.Visible;
            Util.gridClear(dgOutput);
        }

        private void txtLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                                       
                    ShowLoadingIndicator();

                    if ((bool)rdoCARRIER.IsChecked)
                    {
                        Util.MessageValidation("SFU8926");   //복사 및 붙여넣기 금지.
                        return;
                    }

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if(sPasteStrings.Count() == 1)
                    {
                        return;
                    }

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    if (sPasteStrings[0].Trim() == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Processing(sPasteStrings[i]) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        private bool Multi_Processing(string sLotid)
        {
            try
            {
                if ((bool)rdoBox.IsChecked)
                {
                    CheckBoxID_Biz(sLotid);
                }
                else
                {
                    CheckLotID_Biz(sLotid);
                }

                return isMultiCopy;   
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
    }
}
