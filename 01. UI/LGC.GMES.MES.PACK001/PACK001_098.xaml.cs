/*************************************************************************************
 Created Date : 2023.05.17
      Creator : 김진수
   Decription : 라벨 발행 및 착/완공 기능
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.24  김진수S : Initial Created.
  2023.06.02  김진수S : Popup창 추가
  2023.06.30  김진수S : 라벨 발행 이력에 화면ID 추가 등록 (최신버전 재 업로드)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.PACK001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using C1ComboBox = C1.WPF.C1ComboBox;




namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_098 : UserControl, IWorkArea
    {
        string sSample_flag = string.Empty;

        public PACK001_098()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            this.Loaded -= C1Window_Loaded;
        }

        #region 처음부분
        private void setCombo()
        {
            CommonCombo _combo = new CommonCombo();

            string strAreagCase = "LABELCODE_BY_PROD";
            String[] sLabelFilter = { null, null, null, LABEL_TYPE_CODE.PACK };

            _combo.SetCombo(cboLabel, CommonCombo.ComboStatus.SELECT, sFilter: sLabelFilter, sCase: strAreagCase);

            cboLabel.SelectedIndex = 0;
            
            DataTable dtResultArea = new DataTable();
            dtResultArea.Columns.Add("CBO_NAME", typeof(string));
            dtResultArea.Columns.Add("CBO_CODE", typeof(string));

            dtResultArea.Rows.Add(new object[] { LoginInfo.CFG_AREA_NAME, LoginInfo.CFG_AREA_ID });

            cboAreaID.ItemsSource = DataTableConverter.Convert(dtResultArea);
            cboAreaID.SelectedIndex = 0;
        }
        private void Initialize()
        {
            //PackCommon.SetC1ComboBox(this.GetAreaInfo(), this.cboAreaID, true);
            setCombo();
            PackCommon.SetC1ComboBox(this.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboEqsgID, true, "SELECT");

            // 8라인 MICA 전용이므로 하드코딩 적용
            cboEqsgID.SelectedValue = "P8Q22";
            cboProcID.SelectedValue = "P5345";

            cboAreaID.IsEnabled = false;
            cboEqsgID.IsEnabled = false;
            cboProcID.IsEnabled = false;
            /*
            cboEqsgID.SelectedValue = LoginInfo.CFG_EQSG_ID;
            cboProcID.SelectedValue = LoginInfo.CFG_PROC_ID;
            cboEqptID.SelectedValue = LoginInfo.CFG_EQPT_ID;
            */

            //setComboSelect(cboEqsgID);
            //setComboSelect(cboEqptID);
            //PackCommon.SetC1ComboBox(this.GetProcessInfo(cboAreaID.SelectedValue.ToString(), cboEqsgID.SelectedValue.ToString()), this.cboProcID, true, "SELECT");
            //PackCommon.SetC1ComboBox(this.GetEquipmentInfo(cboEqsgID.SelectedValue.ToString(),cboProcID.SelectedValue.ToString()), this.cboEqptID, true, "SELECT");

            //this.cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;

        }
        
        private void setComboSelect(C1ComboBox c1ComboBox)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("CBO_NAME", typeof(string));
            dtResult.Columns.Add("CBO_CODE", typeof(string));

            dtResult.Rows.Add(new object[] { "-SELECT-", string.Empty });

            c1ComboBox.ItemsSource = DataTableConverter.Convert(dtResult);
            c1ComboBox.SelectedIndex = 0;
        }

        #endregion

        #region 이벤트

        private void cboAreaByAreaType_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {

            //PackCommon.SetC1ComboBox(this.GetEquipmentSegmentInfo(cboAreaID.SelectedValue.ToString()), this.cboEqsgID, true, "SELECT");
            //setComboSelect(cboEqsgID);
            //setComboSelect(cboEqptID);
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(cboEqsgID.SelectedValue.ToString()))
            {
                PackCommon.SetC1ComboBox(this.GetProcessInfo(cboAreaID.SelectedValue.ToString(), cboEqsgID.SelectedValue.ToString()), this.cboProcID, true, "SELECT");
            }
            else
            {
                setComboSelect(cboProcID);
            }
        }

        private void cboProcess_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(cboProcID.SelectedValue.ToString()))
            {
                PackCommon.SetC1ComboBox(this.GetEquipmentInfo(cboEqsgID.SelectedValue.ToString(), cboProcID.SelectedValue.ToString()), this.cboEqptID, true, "SELECT");
            }
            else
            {
                setComboSelect(cboEqptID);
            }
        }

        private void txtPrintIDInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                labelPrint();
            }
        }

        private void Ready_Click(object sender, RoutedEventArgs e)
        {

            string strProcid = cboProcID.SelectedValue.ToString();
            string strEqptID = cboEqptID.SelectedValue.ToString();
            string strEqsgID = cboEqsgID.SelectedValue.ToString();

            if (string.IsNullOrEmpty(strProcid))
            {
                Util.MessageInfo("SFU1459");        // SFU1459 : 공정을 선택하세요.
                return;
            }
            if (string.IsNullOrEmpty(strEqptID))
            {
                Util.MessageInfo("SFU1673");        // SFU1673 : 설비를 선택하세요.
                return;
            }
            if (string.IsNullOrEmpty(strEqsgID))
            {
                Util.MessageInfo("SFU1255");        // SFU1255 : 라인을 선택 하세요.
                return;
            }

            if (Util.NVC(cboLabel.SelectedValue.ToString()).Equals("SELECT"))
            {
                Util.MessageInfo("SFU3732");        // SFU3732 : 라벨 코드 를 선택하여 주세요
                return;
            }

            strClean("all");
        }


        private void txtLotIDInputInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Interlock_CHK(txtLotIDInput.Text);

            }

        }

        private void txtBarcodeIDInputInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PrintEND();
            }
        }
        
        #endregion

        #region 함수

        private void labelPrint()
        {
            try
            {
                string strLotid = string.Empty;
                string strEqsgId = string.Empty;

                string strProcid = cboProcID.SelectedValue.ToString();
                string strEqptID = cboEqptID.SelectedValue.ToString();
                string strEqsgID = cboEqsgID.SelectedValue.ToString();

                if (string.IsNullOrEmpty(strProcid))
                {
                    Util.MessageInfo("SFU1459");        // SFU1459 : 공정을 선택하세요.
                    return;
                }
                if (string.IsNullOrEmpty(strEqptID))
                {
                    Util.MessageInfo("SFU1673");        // SFU1673 : 설비를 선택하세요.
                    return;
                }
                if (string.IsNullOrEmpty(strEqsgID))
                {
                    Util.MessageInfo("SFU1255");        // SFU1255 : 라인을 선택 하세요.
                    return;
                }

                if (Util.NVC(cboLabel.SelectedValue.ToString()).Equals("SELECT"))
                {
                    Util.MessageInfo("SFU3732");        // SFU3732 : 라벨 코드 를 선택하여 주세요
                    return;
                }
                

                strLotid = mappingLot(Util.NVC(txtPrintIDInput.Text.ToString().Trim()));

                if (string.IsNullOrEmpty(strLotid))
                {
                    return;
                }

                txtLotId.Text = strLotid;

                strEqsgId = mappingLot_EQSGID(strLotid);

                if (!chkEqsg(strEqsgId, strProcid))
                {
                    Util.MessageInfo("SFU10002", (result) =>        // SFU10002 : 프린트 착/완공 권한이 없는 라인 입니다.
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            strClean("start");
                        }
                    });
                    return;
                }
                // 300 DPI 고정 
                getZPL_Pack(strLotid, strProcid, strEqptID, strEqsgID, LABEL_TYPE_CODE.PACK, Util.NVC(cboLabel.SelectedValue.ToString()), "N", "1", "", "300");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        strClean("start");
                    }
                });
            }

        }

        private void Interlock_CHK(string strScanID)
        {
            string strLotid = string.Empty;
            string strEqsgId = string.Empty;

            string strProcid = cboProcID.SelectedValue.ToString();
            string strEqptID = cboEqptID.SelectedValue.ToString();


            strLotid = mappingLot(strScanID);
            
            strEqsgId = mappingLot_EQSGID(strLotid);

            if (!chkEqsg(strEqsgId, strProcid))
            {
                Util.MessageInfo("SFU10002", (result) =>            // SFU10002 :프린트 착/완공 권한이 없는 라인 입니다.
                {
                    if (result == MessageBoxResult.OK)
                    {
                        strClean("end");
                    }
                });
                return;
            }

            txtSCANLOTID.Text = strLotid;

            txtBarcodeIDInput.Focus();
        }
        private void PrintEND()
        {
            //string strLotid = string.Empty;
            string strEqsgId = string.Empty;

            string strProcid = cboProcID.SelectedValue.ToString();
            string strEqptID = cboEqptID.SelectedValue.ToString();

            if (string.IsNullOrEmpty(strProcid))
            {
                Util.MessageInfo("SFU1459");    // SFU1459: 공정을 선택하세요.
                return;
            }
            if (string.IsNullOrEmpty(strEqptID))
            {
                Util.MessageInfo("SFU1673");    // SFU1673 : 설비를 선택하세요.
                return;
            }

            string lotID1 = Util.NVC(txtLotIDInput.Text.ToString().Trim());
            string lotID2 = Util.NVC(txtBarcodeIDInput.Text.ToString().Trim());

            if (lotID1.Equals(lotID2))
            {
                Util.MessageInfo("SFU1505");    // SFU1505 : 동일한 라벨이 스캔되었습니다.
                return;
            }

            string strLotID = BR_PRD_CHK_SCAN_BARCODE(lotID1, lotID2);
            
            if (string.IsNullOrEmpty(strLotID))
            {
                return;
            }

            DataSet dsResult = BR_PRD_REG_OUTPUTASSY(strProcid, strEqptID, strLotID);
            
            if (dsResult != null)
            {
                if (dsResult.Tables["OUTDATA"] != null)
                {
                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        showPopup(strLotID, "SFU1742"); // SFU1742 : 완공되었습니다.
                        strClean("all");
                    }
                }
            }
        }
        #endregion

        #region Biz 부분

        private DataTable GetAreaInfo()
        {
            string bizRuleName = "DA_BAS_SEL_AREA_BY_AREATYPE_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREA_TYPE_CODE"] = Area_Type.PACK;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        private DataTable GetEquipmentSegmentInfo(string strAreaID)
        {
            string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = strAreaID;
                drRQSTDT["EXCEPT_GROUP"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        private DataTable GetProcessInfo(string strAreaID, string strEqsgID)
        {
            string bizRuleName = "DA_BAS_SEL_PROCESS_PACK_CBO_BY_LABELPRINTIUSE";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("LABELPRINTIUSE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(strEqsgID) ? null : strEqsgID;
                drRQSTDT["AREAID"] = string.IsNullOrEmpty(strAreaID) ? null : strAreaID;
                drRQSTDT["LABELPRINTIUSE"] = "Y";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        private DataTable GetEquipmentInfo(string strEqsgID, string strProcID)
        {
            string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = string.IsNullOrEmpty(strEqsgID) ? null : strEqsgID;
                drRQSTDT["PROCID"] = string.IsNullOrEmpty(strProcID) ? null : strProcID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }


        private string BR_PRD_CHK_SCAN_BARCODE(string strScanID, string strBarcodeID)
        {
            string strResult = string.Empty;
            try
            {
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("SCANID", typeof(string));


                DataRow drINDATA = INDATA.NewRow();
                drINDATA["LOTID"] = strScanID;
                drINDATA["SCANID"] = strBarcodeID;
                INDATA.Rows.Add(drINDATA);

                dsInput.Tables.Add(INDATA);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SCAN_BARCODE", "INDATA", "OUTDATA", INDATA);

                if (dtResult.Rows.Count > 0)
                {
                    strResult = dtResult.Rows[0]["LOTID"].ToString();
                }

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_OUTPUTASSY", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return strResult;
        }

        private DataSet BR_PRD_REG_OUTPUTASSY(string procid, string EQPTID, string LotID)
        {
            DataSet dsResult = null;
            try
            {
                //resncode 양품인경우는 OK
                string sResnCode = string.Empty;
                sResnCode = "OK";

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("END_PROCID", typeof(string));
                INDATA.Columns.Add("END_EQPTID", typeof(string));
                INDATA.Columns.Add("STRT_PROCID", typeof(string));
                INDATA.Columns.Add("STRT_EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("WIPNOTE", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("RESNDESC", typeof(string));


                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = LotID;
                drINDATA["END_PROCID"] = procid;
                drINDATA["END_EQPTID"] = EQPTID;
                drINDATA["STRT_PROCID"] = null;
                drINDATA["STRT_EQPTID"] = null;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["WIPNOTE"] = string.Empty;
                drINDATA["RESNCODE"] = sResnCode;
                drINDATA["RESNNOTE"] = string.Empty;
                drINDATA["RESNDESC"] = "";
                INDATA.Rows.Add(drINDATA);

                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                IN_CLCTITEM.Columns.Add("PASSYN", typeof(string));

                DataRow drIN_CLCTITEM = null;

                DataTable IN_CLCTDITEM = new DataTable();
                IN_CLCTDITEM.TableName = "IN_CLCTDITEM";
                IN_CLCTDITEM.Columns.Add("CLCTDITEM", typeof(string));
                IN_CLCTDITEM.Columns.Add("CLCTDVAL", typeof(string));
                //dsInput.Tables.Add(IN_CLCTDITEM);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return dsResult;
        }
        #endregion


        private string mappingLot(string ScanID)
        {
            try
            {
                string strLlotId = string.Empty;

                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LOTID"] = ScanID;

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MAPPINGLOTID", "INDATA", "OUTDATA", INDATA);

                if (dtRslt.Rows.Count > 0)
                {
                    strLlotId = dtRslt.Rows[0]["LOTID"].ToString();

                    return strLlotId;
                }
                else
                {
                    Util.MessageInfo("SFU1192", (result) =>     // SFU1192: Lot 정보를 조회할 수 없습니다.
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //txtPrintIDInput.Text = "";
                            txtPrintIDInput.Focus();
                            txtPrintIDInput.SelectAll();
                            return;
                        }
                    });

                    return strLlotId;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private string mappingLot_EQSGID(string strLotId)
        {
            try
            {
                string strEqsgId = string.Empty;

                DataTable INDATA = new DataTable();

                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LOTID"] = strLotId;

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPLOT", "RQSTDT", "RSLTDT", INDATA);

                if (dtRslt.Rows.Count > 0)
                {
                    strEqsgId = dtRslt.Rows[0]["EQSGID"].ToString();

                    return strEqsgId;
                }
                else
                {
                    Util.MessageInfo("SFU1192", (result) =>     // SFU1192: Lot 정보를 조회할 수 없습니다.
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //txtPrintIDInput.Text = "";
                            txtPrintIDInput.Focus();
                            txtPrintIDInput.SelectAll();
                            return;
                        }
                    });
                    return strEqsgId;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }


        
        private void chkREALTIME_Checked(object sender, RoutedEventArgs e)
        {
                DateTime DateNow = DateTime.Now;
                dtpDateFrom.SelectedDateTime = DateNow;
                dtpDateTo.SelectedDateTime = DateNow;
                dtpDateFrom.IsEnabled = false;
                dtpDateTo.IsEnabled = false;
            
            
        }


        private void chkREALTIME_UnChecked(object sender, RoutedEventArgs e)
        {
            dtpDateFrom.IsEnabled = true;
            dtpDateTo.IsEnabled = true;
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = cboProcID.SelectedValue;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); ;
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); ;
   
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORY_MICA_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgActHistList, dtResult, FrameOperation, true);
                

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private Boolean chkEqsg(string strEqsgId, string strProcID)
        {
            bool bResult = false;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_UI_PRINT_START_COMPLETE_EQSGID";
                dr["CMCODE"] = strEqsgId;

                RQSTDT.Rows.Add(dr);

                DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", RQSTDT);

                if (dtAuth.Rows.Count > 0)
                {
                    //string[] arrProcID = dtAuth.Rows[0]["ATTRIBUTE1"].ToString().Split(',');
                    foreach(string arrProcID in dtAuth.Rows[0]["ATTRIBUTE1"].ToString().Split(','))
                    {
                        if(arrProcID.Equals(strProcID))

                            //sSample_flag = dtAuth.Rows[0]["ATTRIBUTE1"].ToString();
                            bResult = true;
                    }
                }
                else
                {
                    //sSample_flag = string.Empty;
                }

                    return bResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }


        private void getZPL_Pack(string sLOTID, string sPROCID, string sEQPTID, string sEQSGID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY, string sPGM_ID, string sDPI)
        {
            try
            {
                string sTOP = null;
                string sLEFT = null;
                string sDARKNESS = null;


                foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                    {
                        sDPI = string.IsNullOrWhiteSpace(sDPI) ? drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString() : sDPI;
                        sTOP = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                        sLEFT = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                        sDARKNESS = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                        break;
                    }
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));
                INDATA.Columns.Add("PRN_QTY", typeof(string));
                INDATA.Columns.Add("PGM_ID", typeof(string));
                INDATA.Columns.Add("DPI", typeof(string));
                INDATA.Columns.Add("TOP", typeof(string));
                INDATA.Columns.Add("LEFT", typeof(string));
                INDATA.Columns.Add("DARKNESS", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLOTID;
                dr["PROCID"] = sPROCID;
                dr["EQPTID"] = sEQPTID;
                dr["EQSGID"] = sEQSGID;
                dr["LABEL_TYPE"] = sLABEL_TYPE;
                dr["LABEL_CODE"] = sLABEL_CODE;
                dr["SAMPLE_FLAG"] = sSAMPLE_FLAG;
                dr["PRN_QTY"] = sPRN_QTY;
                dr["PGM_ID"] = string.IsNullOrWhiteSpace(sPGM_ID) ?  this.GetType().Name.ToString() : sPGM_ID;
                dr["DPI"] = sDPI;
                dr["TOP"] = sTOP;
                dr["LEFT"] = sLEFT;
                dr["DARKNESS"] = sDARKNESS;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);



                

                //Util.printLabel_Pack(FrameOperation, loadingIndicator, str_LOTID, LABEL_TYPE_CODE.PACK, sLabelCode, "N", "1", Util.NVC(txtLotInfoProductId.Tag));
                //BR_PRD_GET_ZPL
                new ClientProxy().ExecuteService("BR_PRD_REG_PLT_AND_STREND", "INDATA", "OUTDATA", INDATA, (bizResult, bizException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            strClean("start");
                            return;
                        }

                        Dictionary<string, string> dicHeader = new Dictionary<string, string>();

                        if (bizResult.Rows.Count > 0)
                        {
                            foreach (DataRow dc in bizResult.Rows)
                            {
                                string zpl = Util.NVC(dc["ZPLSTRING"].ToString());
                                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                              //  PrintLabel(zpl);
                            }
                            showPopup(sLOTID, "SFU10003");      // SFU10003 : 착공 및 라벨 발행 완료 되었습니다.
                            txtPrintIDInput.SelectAll();
                            txtLotIDInput.Focus();

                        }
                        else
                        {
                            strClean("start");
                        }
                        

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                });
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void strClean(string cType)
        {
            switch (cType)
            {
                case "start":
                    txtPrintIDInput.Text = string.Empty;
                    txtLotId.Text = string.Empty;

                    txtPrintIDInput.Focus();
                    break;


                case "end":
                    txtLotIDInput.Text = string.Empty;
                    txtBarcodeIDInput.Text = string.Empty;
                    txtSCANLOTID.Text = string.Empty;
                    txtBARCODELOTID.Text = string.Empty;
                    txtLotIDInput.Focus();
                    chkREALTIME.SetCurrentValue(CheckBox.IsCheckedProperty, true);
                    btnSearch.PerformClick();
                    break;


                case "all":
                default:
                    txtPrintIDInput.Text = string.Empty;
                    txtLotId.Text = string.Empty;
                    txtLotIDInput.Text = string.Empty;
                    txtBarcodeIDInput.Text = string.Empty;
                    txtSCANLOTID.Text = string.Empty;
                    txtBARCODELOTID.Text = string.Empty;

                    txtPrintIDInput.Focus();
                    break;
            }

        }

        private void showPopup(string sLotid, string sTitle)
        {
            try
            {
                PACK001_098_POPUP popup = new PACK001_098_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("TITLE", typeof(string));
                    dtData.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["TITLE"] = MessageDic.Instance.GetMessage(sTitle);
                    newRow["LOTID"] = sLotid;
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //throw ex;
            }
        }
    }

}