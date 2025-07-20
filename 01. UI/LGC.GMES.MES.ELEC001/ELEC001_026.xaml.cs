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
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_026 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();
        string _WH_ID = string.Empty;

        public ELEC001_026()
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
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboElecWareHouse };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cbProcessParent, sCase: "PROCESS");

            // 창고명
            C1ComboBox[] cboWareHouseParent = { cboArea };
            C1ComboBox[] cboareHouseChild = { cboElecRack };
            _combo.SetCombo(cboElecWareHouse, CommonCombo.ComboStatus.ALL, cbChild: cboareHouseChild, cbParent: cboWareHouseParent);

            // RACK
            C1ComboBox[] cboRackParent = { cboElecWareHouse };
            _combo.SetCombo(cboElecRack, CommonCombo.ComboStatus.ALL, cbParent: cboRackParent);

            // 양/음극
            String[] sFilter1 = { "", "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODES");

            dtpFrom.SelectedDateTime = System.DateTime.Now;
            dtpTo.SelectedDateTime = System.DateTime.Now;

        }
        #endregion

        #region Mehod
        private void GetElecStock()
        {
            try
            {
                string sProc = cboProcess.SelectedValue.ToString();
                string sWh = cboElecWareHouse.SelectedValue.ToString();
                string sRack = cboElecRack.SelectedValue.ToString();
                string sModel = txtModel.Text;
                string sProd = txtProdCode.Text;
                string sPrdtClss = cboElecType.SelectedValue.ToString();
                string sLot = txtLotID.Text;
                //string sHold = cboLotStatus.SelectedValue.ToString();
                string sHold = "";

                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpTo.SelectedDateTime);

                if (!string.IsNullOrEmpty(txtCARRIERID.Text.Trim()))
                {
                    string sSearchLotID = SearhCarrierID(txtCARRIERID.Text.Trim());

                    if (string.IsNullOrEmpty(sSearchLotID))
                        sLot = txtCARRIERID.Text.Trim(); //조회 안되게 할라구.
                    else
                        sLot = sSearchLotID;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("HOLD_FLAG", typeof(string));
                RQSTDT.Columns.Add("SDATE", typeof(string));
                RQSTDT.Columns.Add("EDATE", typeof(string));

                if (sProc == "")
                    sProc = null;
                if (sWh == "")
                    sWh = null;
                if (sRack == "")
                    sRack = null;
                if (sModel == "")
                    sModel = null;
                if (sProd == "")
                    sProd = null;
                if (sPrdtClss == "")
                    sPrdtClss = null;
                if (sLot == "")
                    sLot = null;
                //if (sHold == "")
                //    sHold = null;
                if ((bool)chkHold.IsChecked)
                {
                    sHold = "Y";
                }
                else
                {
                    sHold = null;
                }

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID.ToString();
                dr["AREAID"] = cboArea.SelectedValue.ToString();

                //dr["PROCID"] = sProc;
                dr["WH_ID"] = sWh;
                dr["RACK_ID"] = sRack;
                dr["MODLID"] = sModel;
                dr["PRODID"] = sProd;
                dr["PRDT_CLSS_CODE"] = sPrdtClss;
                dr["LOTID"] = sLot;
                dr["HOLD_FLAG"] = sHold;
                dr["SDATE"] = _ValueFrom;
                dr["EDATE"] = _ValueTo;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WH_WIPHISTORY", "RQSTDT", "RSLTDT", RQSTDT);

                // dgElecStock.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgReceive, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetElecStock();
        }
      
        #endregion

    }
}
