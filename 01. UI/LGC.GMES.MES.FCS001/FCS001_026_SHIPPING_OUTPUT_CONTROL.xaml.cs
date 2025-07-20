/*************************************************************************************
 Created Date :
      Creator : KYS
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.12  김용식 : Initial Created
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_026_SHIPPING_OUTPUT_CONTROL : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        //老常温后端空托盘方向控制(Y : 空托盘进lift, N : 空托盘直行) - EMPTY_TRAY_OUT_CONTROL_DRNORMAL
        //EMPTY_TRAY_OUT_CONTROL_NAGING_BACK
        //EMPTY_TRAY_OUT_CONTROL_NAGING_BACK_LIFT
        //EMPTY_TRAY_OUT_CONTROL_NAGING_BACK_STRAIGHT

        //老常温后端分叉扫码器控制(Y : 老常温前分叉扫码器pass, N : 效率分流input) - EMPTY_TRAY_OUT_CONTROL_PASSYN
        //EMPTY_TRAY_OUT_CONTROL_PASSYN
        //EMPTY_TRAY_OUT_CONTROL_PASSYN_Y
        //EMPTY_TRAY_OUT_CONTROL_PASSYN_N

        //出荷空托盘出库控制(F : 空托盘出荷前端出库, B : 空托盘出荷后端出库) - SHIP_EMPTY_TRAY_OUT_CONTROL
        //SHIP_EMPTY_TRAY_OUT_CONTROL
        //SHIP_EMPTY_TRAY_OUT_CONTROL_FRONT
        //SHIP_EMPTY_TRAY_OUT_CONTROL__BACK

        //3F PreAging空托盘出库控制(F : 3F 空托盘前端出库, B : 3F 空托盘后端出库) - 3F_PRE_EMPTY_TRAY_OUT_CONTROL
        //3F_PRE_EMPTY_TRAY_OUT_CONTROL
        //3F_PRE_EMPTY_TRAY_OUT_CONTROL_FRONT
        //3F_PRE_EMPTY_TRAY_OUT_CONTROL_BACK

        //空托盘老常温后面上端入库(Y : 空托盘老常温后面上端入库, N : 空托盘出厂后面入库) - EMPTY_TRAY_INPUT_CONTROL_NAGING_BACK_UPPER
        //EMPTY_TRAY_INPUT_CONTROL_NAGING_BACK_UPPER
        //EMPTY_TRAY_INPUT_CONTROL_NAGING_BACK_UPPER_Y
        //EMPTY_TRAY_INPUT_CONTROL_NAGING_BACK_UPPER_N


        private string CODE_GRP_A = "EMPTY_TRAY_OUT_CONTROL_NAGING_BACK";
        private string NAME_GRP_A = "老常温后端空托盘方向控制";
        private string CODE_AY = "空托盘进lift";
        private string CODE_AN = "空托盘直行";

        private string CODE_GRP_B = "EMPTY_TRAY_OUT_CONTROL_PASSYN";
        private string NAME_GRP_B = "老常温后端分叉扫码器控制";
        private string CODE_BY = "老常温前分叉扫码器pass";
        private string CODE_BN = "效率分流input";

        private string CODE_GRP_C = "SHIP_EMPTY_TRAY_OUT_CONTROL";
        private string NAME_GRP_C = "出荷空托盘出库控制";
        private string CODE_CF = "空托盘出荷前端出库";
        private string CODE_CB = "空托盘出荷后端出库";

        private string CODE_GRP_D = "3F_PRE_EMPTY_TRAY_OUT_CONTROL";
        private string NAME_GRP_D = "3F PreAging空托盘出库控制";
        private string CODE_DF = "3F 空托盘前端出库";
        private string CODE_DB = "3F 空托盘后端出库";

        private string CODE_GRP_E = "EMPTY_TRAY_INPUT_CONTROL_NAGING_BACK_UPPER";
        private string NAME_GRP_E = "空托盘老常温后面上端入库";
        private string CODE_EY = "空托盘老常温后面上端入库";
        private string CODE_EN = "空托盘出厂后面入库";

        #endregion

        #region [Initialize]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_026_SHIPPING_OUTPUT_CONTROL()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, EventArgs e)
        {
            //Combo Setting
            InitCombo();

            // 조회
            GetShippingOutPutControlStat("SELECT", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_A, NAME_GRP_A, cboA);
            GetShippingOutPutControlStat("SELECT", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_B, NAME_GRP_B, cboB);
            GetShippingOutPutControlStat("SELECT", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_C, NAME_GRP_C, cboC);
            GetShippingOutPutControlStat("SELECT", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_D, NAME_GRP_D, cboD);
            GetShippingOutPutControlStat("SELECT", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_E, NAME_GRP_E, cboE);
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();
            string[] sFilter = { "SHIPPING_OUTPUT_CONTROL", CODE_GRP_A, NAME_GRP_A, null, null, null };
            ComCombo.SetCombo(cboA, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN_WITH_OPTION", sFilter: sFilter);
            sFilter[1] = CODE_GRP_B;
            sFilter[2] = NAME_GRP_B;
            ComCombo.SetCombo(cboB, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN_WITH_OPTION", sFilter: sFilter);
            sFilter[1] = CODE_GRP_C;
            sFilter[2] = NAME_GRP_C;
            ComCombo.SetCombo(cboC, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN_WITH_OPTION", sFilter: sFilter);
            sFilter[1] = CODE_GRP_D;
            sFilter[2] = NAME_GRP_D;
            ComCombo.SetCombo(cboD, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN_WITH_OPTION", sFilter: sFilter);
            sFilter[1] = CODE_GRP_E;
            sFilter[2] = NAME_GRP_E;
            ComCombo.SetCombo(cboE, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN_WITH_OPTION", sFilter: sFilter);
        }
        #endregion

        #region [Method]
        private void GetShippingOutPutControlStat(string sWorkYpe, string sUserID, string sConfType, string sConfKey1, string sConfKey3, C1ComboBox cboTemp)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_TYPE"] = sWorkYpe;
                dr["USERID"] = sUserID;
                dr["CONF_TYPE"] = sConfType;
                dr["CONF_KEY1"] = sConfKey1;
                dr["CONF_KEY2"] = "ESNJ";
                dr["CONF_KEY3"] = sConfKey3;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach (DataRow drConf in dtResult.Rows)
                    {
                        if (CODE_GRP_A.Equals(sConfKey1))
                        {
                            if (Util.NVC(drConf["USER_CONF01"]).Contains("Y"))
                            {
                                cboTemp.SelectedItem = CODE_AY;
                            }
                            else if (Util.NVC(drConf["USER_CONF01"]).Contains("N"))
                            {
                                cboTemp.SelectedItem = CODE_AN;
                            }
                        }
                        else if (CODE_GRP_B.Equals(sConfKey1))
                        {
                            if (Util.NVC(drConf["USER_CONF01"]).Contains("Y"))
                            {
                                cboTemp.SelectedItem = CODE_BY;
                            }
                            else if (Util.NVC(drConf["USER_CONF01"]).Contains("N"))
                            {
                                cboTemp.SelectedItem = CODE_BN;
                            }
                        }
                        else if (CODE_GRP_C.Equals(sConfKey1))
                        {
                            if (Util.NVC(drConf["USER_CONF01"]).Contains("F"))
                            {
                                cboTemp.SelectedItem = CODE_CF;
                            }
                            else if (Util.NVC(drConf["USER_CONF01"]).Contains("B"))
                            {
                                cboTemp.SelectedItem = CODE_CB;
                            }
                        }
                        else if (CODE_GRP_D.Equals(sConfKey1))
                        {
                            if (Util.NVC(drConf["USER_CONF01"]).Contains("F"))
                            {
                                cboTemp.SelectedItem = CODE_DF;
                            }
                            else if (Util.NVC(drConf["USER_CONF01"]).Contains("B"))
                            {
                                cboTemp.SelectedItem = CODE_DB;
                            }
                        }
                        else if (CODE_GRP_E.Equals(sConfKey1))
                        {
                            if (Util.NVC(drConf["USER_CONF01"]).Contains("Y"))
                            {
                                cboTemp.SelectedItem = CODE_EY;
                            }
                            else if (Util.NVC(drConf["USER_CONF01"]).Contains("N"))
                            {
                                cboTemp.SelectedItem = CODE_EN;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetShippingOutPutControlStat(string sWorkYpe, string sUserID, string sConfType, string sConfKey1, string sConfKey3, C1ComboBox cboTemp)
        {
            // 2023.11.27 미선택 에러 방지
            if (cboTemp.SelectedValue == null)
            {
                return;
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));
                dtRqst.Columns.Add("USER_CONF01", typeof(string));

                DataRow drNew = dtRqst.NewRow();
                drNew["WRK_TYPE"] = sWorkYpe;
                drNew["USERID"] = sUserID;
                drNew["CONF_TYPE"] = sConfType;
                drNew["CONF_KEY1"] = sConfKey1;
                drNew["CONF_KEY2"] = "ESNJ";
                drNew["CONF_KEY3"] = sConfKey3;
                drNew["USER_CONF01"] = Util.GetCondition(cboTemp).ToString().Right(1);
                dtRqst.Rows.Add(drNew);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null)
                {
                    if (dtResult.Rows[0]["USERID"].ToString().Equals("OK"))
                    {
                        //this.DialogResult = MessageBoxResult.OK;
                        //Util.AlertInfo("SFU3532");  //
                    }
                    else
                    {
                        this.DialogResult = MessageBoxResult.No;
                        Util.AlertInfo("SFU3535");  //
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 상태를 저장하시겠습니까?
            Util.MessageConfirm("SFU3533", (result) =>
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }

                SetShippingOutPutControlStat("SAVE", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_A, NAME_GRP_A, cboA);
                SetShippingOutPutControlStat("SAVE", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_B, NAME_GRP_B, cboB);
                SetShippingOutPutControlStat("SAVE", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_C, NAME_GRP_C, cboC);
                SetShippingOutPutControlStat("SAVE", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_D, NAME_GRP_D, cboD);
                SetShippingOutPutControlStat("SAVE", "CNV", "SHIPPING_OUTPUT_CONTROL", CODE_GRP_E, NAME_GRP_E, cboE);

            });
             Close();
        }
        
        #endregion


    }
}
