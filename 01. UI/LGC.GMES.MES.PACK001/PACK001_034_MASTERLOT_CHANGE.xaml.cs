/*************************************************************************************
 Created Date : 2018.09.07
      Creator : 손홍구
   Decription : MASTER LOT 정보 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.07  Initial Created.

 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_034_MASTERLOT_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private DataSet dsIndataParam = new DataSet();

        public PACK001_034_MASTERLOT_CHANGE()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            DataTable dtText = tmps[0] as DataTable;
            if (dtText.Rows.Count > 0)
            {
                txtLOTID.Text = Util.NVC(dtText.Rows[0]["LOTID"]);
            }
            setComboBox_Area_schd();
        }
        private void C1Window_Closed(object sender, EventArgs e)
        {
            //loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLOTID.Text.Length > 0)
                {
                    //선택한 Lot의 정보를 변경 하시겠습니까?\n\nRout : {0}\n생산적용모델 : {1}
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3599", cboTargetROUTID.Text, cboTargetPRODID.SelectedValue), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            changeInputData();

                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        }
                    });

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTargetAREAID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_EquipmentSegment_schd(Util.NVC(cboTargetAREAID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void cboTargetEQSGID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Route_schd(Util.NVC(cboTargetAREAID.SelectedValue), Util.NVC(cboTargetEQSGID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTargetROUTID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Product_schd(Util.NVC(cboTargetROUTID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTargetPRODID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Process_schd(Util.NVC(cboTargetAREAID.SelectedValue), Util.NVC(cboTargetEQSGID.SelectedValue), Util.NVC(cboTargetROUTID.SelectedValue), Util.NVC(cboTargetPRODID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTargetPROCID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Equipment_schd(Util.NVC(cboTargetAREAID.SelectedValue), Util.NVC(cboTargetEQSGID.SelectedValue), Util.NVC(cboTargetPROCID.SelectedValue));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void setComboBox_Area_schd()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";

                dtIndata.Columns.Add("SYSTEM_ID", typeof(string));
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SHOPID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow drnewrow = dtIndata.NewRow();
                drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["USERID"] = LoginInfo.USERID;
                dtIndata.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetAREAID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_AREA_ID) select dr).Count() > 0)
                    {
                        cboTargetAREAID.SelectedValue = LoginInfo.CFG_AREA_ID;
                    }
                    else if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetAREAID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetAREAID_SelectedValueChanged(null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_EquipmentSegment_schd(string sAREAID)
        {
            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drNewRow = dtIndata.NewRow();

                drNewRow["LANGID"] = LoginInfo.LANGID;
                drNewRow["AREAID"] = sAREAID;
                dtIndata.Rows.Add(drNewRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetEQSGID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQSG_ID) select dr).Count() > 0)
                    {
                        cboTargetEQSGID.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    }
                    else if (dtResult.Rows.Count > 0)
                    {
                        cboTargetEQSGID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetEQSGID_SelectedValueChanged(null, null);
                    }

                }
                );

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Route_schd(string sAREAID, string sEQSGID)
        {
            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["AREAID"] = sAREAID;
                drIndata["EQSGID"] = sEQSGID;
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_ROUT_GROUP_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetROUTID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetROUTID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetROUTID_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Product_schd(string sROUTID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("ROUTID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["ROUTID"] = sROUTID;
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PRODUCT_BY_ROUT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetPRODID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetPRODID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetPRODID_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Process_schd(string sAREAID, string sEQSGID, string sROUTID, string sPRODID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("ROUTID", typeof(string));
                dtIndata.Columns.Add("PRODID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["EQSGID"] = sEQSGID;
                drIndata["ROUTID"] = sROUTID;
                drIndata["PRODID"] = sPRODID;
                drIndata["AREAID"] = sAREAID;
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_ROUT_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetPROCID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_PROC_ID) select dr).Count() > 0)
                    {
                        cboTargetPROCID.SelectedValue = LoginInfo.CFG_PROC_ID;
                    }
                    else if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetPROCID.SelectedIndex = 0;
                    }
                    else
                    {
                        cboTargetPROCID_SelectedValueChanged(null, null);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Equipment_schd(string sAREAID, string sEQSGID, string sPROCID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("PROCID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["EQSGID"] = sEQSGID;
                drIndata["PROCID"] = sPROCID;
                drIndata["AREAID"] = sAREAID;
                dtIndata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_NUMBER_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTargetEQPTID.ItemsSource = DataTableConverter.Convert(dtResult);

                    if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQPT_ID) select dr).Count() > 0)
                    {
                        cboTargetEQPTID.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                    else if (dtIndata.Rows.Count > 0)
                    {
                        cboTargetEQPTID.SelectedIndex = 0;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void changeInputData()
        {
            try
            {
                //MASTER LOT 정보 변경
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("LOTID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("ROUTID", typeof(string));
                dtIndata.Columns.Add("PRODID", typeof(string));
                dtIndata.Columns.Add("PROCID", typeof(string));
                dtIndata.Columns.Add("EQPTID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow drIndata = dtIndata.NewRow();
                drIndata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["LOTID"] = txtLOTID.Text;
                drIndata["AREAID"] = cboTargetAREAID.SelectedValue;
                drIndata["EQSGID"] = cboTargetEQSGID.SelectedValue;
                drIndata["ROUTID"] = cboTargetROUTID.SelectedValue;
                drIndata["PRODID"] = cboTargetPRODID.SelectedValue;
                drIndata["PROCID"] = cboTargetPROCID.SelectedValue;
                drIndata["EQPTID"] = cboTargetEQPTID.SelectedValue;
                drIndata["USERID"] = LoginInfo.USERID;
                dtIndata.Rows.Add(drIndata);

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add(dtIndata);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_MST_SMPL_LOT_LINE_UI", "INDATA", null, indataSet, null);

                ms.AlertInfo("SFU1275"); //정상처리되었습니다.
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Mehod

        #endregion

    }
}
