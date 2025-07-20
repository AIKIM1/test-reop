/*************************************************************************************
 Created Date : 2020.08.11
      Creator : �ֿ켮
   Decription : ������ Lot �̷�ȭ��
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.11  �ֿ켮 : Initial Created.
  2020.11.12  �ֿ켮 : Empty, Full Box ���� ó�� �߰�
  2023.12.12  KY KIM : Full, Empty Box ������ ó��, ��ǰ��ư E-KANBAN ��ǰ��ư ����, �������� ������ ó�� (���������� ���� �����ڵ弳������ visible�ǵ��� ����[CELL_PLT_BCD_USE_AREA]/�ӽ�)
  2024.02.13  KY KIM : Full, Empty Box ���� ����, ��ǰ��ư�� ����
  2024.03.12  KY KIM : ��ǰ��ư ������ ó��
**************************************************************************************/

using C1.C1Preview.Export;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK002_001 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        //��BOX ó�� ������ ����
        string[] sEmptyAvailableStatus = { "WAIT", "PROC", "END", "TERM" };
        //���� ������ ����
        string[] sEditAvailableStatus = { "WAIT", "PROC", "END", "TERM" };
        //���� ��ǰ ������ ����
        string[] sRtnAvailablestat = { "WAIT", "PROC", "END" };
        //Full ������ ����
        string[] sFullAvailablestat = { "PROC", "END", "TERM" };

        //E-kanban �ӽô����� ���� ����
        string Kanban_flag = string.Empty;

        public PACK002_001()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event

        #region Event - UserControl
        //�ٸ� ȭ�鿡�� �Ѿ���� ID�� ��ȸ
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = FrameOperation.Parameters;
                if (tmps != null)
                {
                    if (tmps.Length > 0)
                    {
                        string sLotid = tmps[0].ToString();
                        txtSearchLotId.Text = sLotid;
                        SetInfomation(txtSearchLotId.Text.Trim());
                    }
                }
                //Kanban_Loaded();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);

                txtSearchLotId.SelectAll();
                txtSearchLotId.Focus();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event - TextBox
        private void txtSearchLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSearchLotId.Text.Length > 0)
                    { 
                        SetInfomation(txtSearchLotId.Text.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event - Button

        private void btnFullBox_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidationCheck())
            {
                return;
            }
            //Full Box�� ���� �ϰڽ��ϱ�?
            Util.MessageConfirm("SFU8269", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    FullSBox();
                }
            });
        }
        private void btnEmptyBox_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidationCheck())
            {
                return;
            }
            //�ش� �׸��� ��BOX�� ���� �Ͻðڽ��ϱ�?
            Util.MessageConfirm("SFU8268", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EmptySBox();
                }
            });

        }

        private void btnSearchLotId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSearchLotId.Text.Length > 0)
                {
                    SetInfomation(txtSearchLotId.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        //private void btnManualReturn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!this.ValidationCheck())
        //    {
        //        return;
        //    }
        //    //SFU3613	���� �Ͻðڽ��ϱ�?
        //    Util.MessageConfirm("SFU3613", (result) =>
        //    {
        //        if (result == MessageBoxResult.OK)
        //        {
        //            ReturnSBox();
        //        }
        //    });
        //}
        //private void btnManualReturnForKanban_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //if (sParam == null)
        //        //{
        //        //    ms.AlertInfo("SUF9002"); //����ó���Ǿ����ϴ�.
        //        //    return;
        //        //}

        //        PACK003_035_REMAIN_POPUP popup = new PACK003_035_REMAIN_POPUP();
        //        popup.FrameOperation = this.FrameOperation;

        //        if (popup != null)
        //        {
        //            //object[] Parameters = null;
        //            //Parameters = new object[] { sParam[3], sParam[0] };

        //            //C1WindowExtension.SetParameters(popup, Parameters);

        //            popup.ShowModal();
        //            popup.CenterOnScreen();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        private void btnHideEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidationCheck())
            {
                return;
            }
            //SFU4340	���� �Ͻðڽ��ϱ�?
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EditSBox();
                }
            });
        }
        #endregion

        #endregion

        #region Mehod
        //��3�� ���������� ���� �����ڵ�� ��Ʈ�� ó��
        //private void Kanban_Loaded()
        //{
        //    try
        //    {

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
        //        RQSTDT.Columns.Add("CMCODE", typeof(string));
        //        //RQSTDT.Columns.Add("CMCDIUSE", typeof(string));
                
        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["CMCDTYPE"] = "CELL_PLT_BCD_USE_AREA";
        //        dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
        //        //dr["CMCDIUSE"] = "N";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", RQSTDT);
        //        if (dtReturn.Rows[0]["ATTRIBUTE1"].ToString().Equals("KANBAN"))
        //        {
        //            Kanban_flag = "Y";
        //            //btnFullBox.Visibility = Visibility.Collapsed;
        //            //btnEmptyBox.Visibility = Visibility.Collapsed;
        //            btnManualReturn.Visibility = Visibility.Collapsed;
        //            btnManualReturnForKanban.Visibility = Visibility.Visible;

        //        }
        //        else
        //        {
        //            Kanban_flag = "N";
        //            //btnFullBox.Visibility = Visibility.Visible;
        //            //btnEmptyBox.Visibility = Visibility.Visible;
        //            btnManualReturn.Visibility = Visibility.Visible;
        //            btnManualReturnForKanban.Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}


        //�̷� ��ȸ BIZ : BR_MTRL_SBOXID_HIST
        private void SetInfomation(string sLotid)
        {
            DataSet dsResult = null;
            try
            {
                ClearLotInfoText();

                txtSBoxId.Text = sLotid;

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("S_BOX_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S_BOX_ID"] = sLotid;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_MTRL_SBOXID_HIST", "INDATA", "OUT_DETL,OUT_HIST", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    int iInfoExistChk = 0;

                    txtSearchLotId.Tag = sLotid;

                    if ((dsResult.Tables.IndexOf("OUT_DETL") > -1) && dsResult.Tables["OUT_DETL"].Rows.Count > 0)
                    {
                        SetSBoxInfoText(dsResult.Tables["OUT_DETL"]);
                        iInfoExistChk = dsResult.Tables["OUT_DETL"].Rows.Count;

                        //��BOX ���� ���� ���� üũ
                        btnEmptyBox.IsEnabled = Is_EmptyBox_Available(dsResult.Tables["OUT_DETL"].Rows[0]["PORT_MTRL_GR_CODE"].ToString()
                                                                    , dsResult.Tables["OUT_DETL"].Rows[0]["S_BOX_STAT"].ToString());
                    }

                    if (dsResult.Tables.IndexOf("OUT_HIST") > -1)
                    {
                        if (dsResult.Tables["OUT_HIST"].Rows.Count > 0)
                        {
                            Util.GridSetData(dgActHistory, dsResult.Tables["OUT_HIST"], FrameOperation);
                        }
                    }

                    if (iInfoExistChk == 0)
                    {
                        //LOT������ �����ϴ�.
                        Util.MessageValidation("SFU1386", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSearchLotId.SelectAll();
                                txtSearchLotId.Focus();
                            }
                        });
                    }

                    txtSearchLotId.SelectAll();
                    txtSearchLotId.Focus();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool ValidationCheck()
        {
            bool returnValue = true;
            
            if (this.dgActHistory == null || this.dgActHistory.Rows.Count < 0)
            {
                Util.Alert("9059");     // �����͸� ��ȸ �Ͻʽÿ�.
                return false;
            }

            if (this.dgActHistory.ItemsSource == null)
            {
                Util.Alert("9059");     // �����͸� ��ȸ �Ͻʽÿ�.
                return false;
            }

            if(string.IsNullOrWhiteSpace(this.txtSBoxId.Text.Trim()))
            {
                Util.Alert("10008");  // ���õ� �����Ͱ� �����ϴ�.
                return false;
            }
            return returnValue;
        }
        //��������&��ǰ ó��
        private void ReturnSBox()
        {
            try
            {
                string sLotid = txtSBoxId.Text.Trim();
                int iRcvQty = int.Parse(txtRcvQty.Text.Trim());
                int iDfctQty = int.Parse(txtDfctQty.Text.Trim());
                int iUseQty = int.Parse(txtUseQty.Text.Trim());
                int iRmnQty = int.Parse(txtRmnQty.Text.Trim());

                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("S_BOX_ID", typeof(string));
                dtIndata.Columns.Add("PORT_ID", typeof(string));
                dtIndata.Columns.Add("DFCT_QTY", typeof(int));
                dtIndata.Columns.Add("USE_QTY", typeof(int));
                dtIndata.Columns.Add("RMN_QTY", typeof(int));
                dtIndata.Columns.Add("EQP_OUT_TYPE1", typeof(string));
                dtIndata.Columns.Add("EQP_OUT_TYPE2", typeof(string));
                dtIndata.Columns.Add("PLC_RSN_CODE", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["S_BOX_ID"] = sLotid;

                dr["PORT_ID"] = txtCrrPortId.Text.Trim();
                dr["DFCT_QTY"] = iDfctQty;
                dr["USE_QTY"] = iUseQty;
                dr["RMN_QTY"] = iRmnQty;

                dr["EQP_OUT_TYPE1"] = "3"; //OK,NG,MANUAL[1,2,3]
                dr["EQP_OUT_TYPE2"] = iRmnQty.Equals(0) ? "1" : iRmnQty.Equals(iRcvQty) ? "2" : "3";//EMPTY,FULL,REMAIN[1,2,3]
                dr["PLC_RSN_CODE"] = "6"; //UI������ǰ

                dr["USERID"] = LoginInfo.USERID;
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MTRL_REG_OUT_SBOXID", "INDATA", "OUTDATA", dtIndata);

                if (dtResult != null)
                {
                    //SFU1275	����ó���Ǿ����ϴ�.
                    Util.MessageInfo("SFU1275", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SetInfomation(sLotid);
                        }
                    });
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }

        }

        private void EmptySBox()
        {
            try
            {
                string sLotid = txtSBoxId.Text.Trim();

                if (!sEmptyAvailableStatus.Contains(txtWipStatus.Text.Trim()))
                {
                    //SFU2063	������¸� Ȯ�����ּ���.
                    Util.MessageValidation("SFU2063");
                    return;
                }

                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("S_BOX_ID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));
                dtIndata.Columns.Add("WAIT_FLAG", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["S_BOX_ID"] = sLotid;
                dr["USERID"] = LoginInfo.USERID;
                dr["WAIT_FLAG"] = "N";
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MTRL_REG_EMPTY_SBOXID", "INDATA", null, dtIndata);

                if (dtResult != null)
                {
                    //SFU1275	����ó���Ǿ����ϴ�.
                    Util.MessageInfo("SFU1275", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SetInfomation(sLotid);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void FullSBox()
        {
            try
            {
                string sLotid = txtSBoxId.Text.Trim();

                if (!sFullAvailablestat.Contains(txtWipStatus.Text.Trim()))
                {
                    //SFU2063	������¸� Ȯ�����ּ���.
                    Util.MessageValidation("SFU2063");
                    return;
                }

                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("S_BOX_ID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["S_BOX_ID"] = sLotid;
                dr["USERID"] = LoginInfo.USERID;
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MTRL_REG_FULL_SBOXID", "INDATA", null, dtIndata);

                if (dtResult != null)
                {
                    //SFU1275	����ó���Ǿ����ϴ�.
                    Util.MessageInfo("SFU1275", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SetInfomation(sLotid);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //���� ���� ó��
        private void EditSBox()
        {
            try
            {
                string sLotid = txtSBoxId.Text.Trim();

                int iInput_Qty = int.Parse(txtRcvQty.Text.Trim());
                int iDfct_Qty = int.Parse(txtDfctQty.Text.Trim());
                int iUse_Qty = int.Parse(txtUseQty.Text.Trim());
                int iRmn_Qty = int.Parse(txtRmnQty.Text.Trim());

                if (!sEditAvailableStatus.Contains(txtWipStatus.Text.Trim()))
                {
                    //SFU2063	������¸� Ȯ�����ּ���.
                    Util.MessageValidation("SFU2063");
                    return;
                }

                if(iInput_Qty < iRmn_Qty)
                {
                    //SFU1861	�ܷ��� �� �������� �����ϴ�.
                    Util.MessageValidation("SFU1861");
                    return;
                }

                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "INDATA";
                dtIndata.Columns.Add("S_BOX_ID", typeof(string));
                dtIndata.Columns.Add("S_BOX_TYPE", typeof(string));
                dtIndata.Columns.Add("S_BOX_STAT", typeof(string));
                dtIndata.Columns.Add("CURR_PORT_ID", typeof(string));
                dtIndata.Columns.Add("INPUT_QTY", typeof(int));
                dtIndata.Columns.Add("DFCT_QTY", typeof(int));
                dtIndata.Columns.Add("USE_QTY", typeof(int));
                dtIndata.Columns.Add("RMN_QTY", typeof(int));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["S_BOX_ID"] = sLotid;
                dr["S_BOX_TYPE"] = null;
                dr["S_BOX_STAT"] = txtWipStatus.Text.Trim();
                dr["CURR_PORT_ID"] = txtCrrPortId.Text.Trim();
                dr["INPUT_QTY"] = iInput_Qty;
                dr["DFCT_QTY"] = iDfct_Qty;
                dr["USE_QTY"] = iUse_Qty;
                dr["RMN_QTY"] = iRmn_Qty;
                dr["USERID"] = LoginInfo.USERID;
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MTRL_REG_EDIT_SBOXID", "INDATA", null, dtIndata);

                if (dtResult != null)
                {
                    //SFU1265	�����Ǿ����ϴ�.
                    Util.MessageInfo("SFU1265", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SetInfomation(sLotid);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        //��BOX ó�� ������ ���� �׷����� üũ
        private bool Is_EmptyBox_Available(string sMtrlGrCode, string sSboxStatus)
        {
            bool blRtn = false;

            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("CMCDTYPE", typeof(string));
                dtIndata.Columns.Add("CMCODE", typeof(string));


                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PORT_MTRL_GR_CODE";
                dr["CMCODE"] = sMtrlGrCode;
                dtIndata.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null)
                {
                    if (dtResult.Rows[0]["ATTRIBUTE3"].ToString().Equals("ISEMPTY") || dtResult.Rows[0]["ATTRIBUTE3"].ToString().Equals("Y"))
                    {
                        if (sEmptyAvailableStatus.Contains(sSboxStatus))
                            blRtn = true;
                    }
                }

            }
            catch (Exception ex)
            {
                blRtn = false;
            }

            return blRtn;
        }

        //������ �Ҵ�
        private void SetSBoxInfoText(DataTable dtLotInfo)
        {
            try
            {
                if (dtLotInfo != null)
                {
                    if (dtLotInfo.Rows.Count > 0)
                    {
                        string[] sSboxParams = Util.NVC(dtLotInfo.Rows[0]["S_BOX_ID"]).Split(';');
                        

                        txtEqsgName.Text = Util.NVC(dtLotInfo.Rows[0]["EQSGNAME"]);
                        txtMtrlId.Text = Util.NVC(dtLotInfo.Rows[0]["MTRLID"]);
                        txtMtrlName.Text = Util.NVC(dtLotInfo.Rows[0]["MTRLNAME"]);
                        txtBanderName.Text = Util.NVC(dtLotInfo.Rows[0]["SUPPLIERID"]);
                        txtCrrPortId.Text = Util.NVC(dtLotInfo.Rows[0]["CURR_PORT_ID"]);
                        txtRcvDttm.Text = Util.NVC(dtLotInfo.Rows[0]["RCV_DTTM"]);
                        txtTermDttm.Text = Util.NVC(dtLotInfo.Rows[0]["TERM_DTTM"]);
                        txtRcvQty.Text = Util.NVC(dtLotInfo.Rows[0]["INPUT_QTY"]);
                        txtRmnQty.Text = Util.NVC(dtLotInfo.Rows[0]["RMN_QTY"]);
                        txtUseQty.Text = Util.NVC(dtLotInfo.Rows[0]["USE_QTY"]);
                        txtDfctQty.Text = Util.NVC(dtLotInfo.Rows[0]["DFCT_QTY"]);
                        txtHoldStatus.Text = Util.NVC(dtLotInfo.Rows[0]["HOLD_FLAG"]);
                        txtWipStatus.Text = Util.NVC(dtLotInfo.Rows[0]["S_BOX_STAT"]);

                        txtPrnDate.Text = Util.NVC(dtLotInfo.Rows[0]["PROD_DATE"]);
                        if (sSboxParams.Length > 4)
                            txtSBoxSeq.Text = sSboxParams[4];

                        txtMtrlGrp1.Text = Util.NVC(dtLotInfo.Rows[0]["PORT_MTRL_GR_CODE"]);
                        txtMtrlGrp2.Text = Util.NVC(dtLotInfo.Rows[0]["PORT_MTRL_GR_CODE"]);
                       
                        //btnManualReturn.IsEnabled = sRtnAvailablestat.Contains(dtLotInfo.Rows[0]["S_BOX_STAT"].ToString())
                        //                        && int.Parse(Util.NVC(dtLotInfo.Rows[0]["RMN_QTY"])) > 0 ? true : false;
                        btnFullBox.IsEnabled = sFullAvailablestat.Contains(dtLotInfo.Rows[0]["S_BOX_STAT"].ToString()) ? true : false;

                        if (Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        {
                            if (sRtnAvailablestat.Contains(dtLotInfo.Rows[0]["S_BOX_STAT"].ToString()))
                            {
                                btnHideEdit.Visibility = Visibility.Visible;
                                txtRmnQty.IsReadOnly = false;
                            }
                        }
                        
                    }

                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //���� ���� �ʱ�ȭ
        private void ClearLotInfoText()
        {
            try
            {
                txtEqsgName.Text = string.Empty;
                txtSBoxId.Text = string.Empty;
                txtMtrlId.Text = string.Empty;
                txtMtrlName.Text = string.Empty;
                txtBanderName.Text = string.Empty;
                txtCrrPortId.Text = string.Empty;
                txtRcvDttm.Text = string.Empty;
                txtTermDttm.Text = string.Empty;
                txtRcvQty.Text = string.Empty;
                txtRmnQty.Text = string.Empty;
                txtUseQty.Text = string.Empty;
                txtDfctQty.Text = string.Empty;
                txtHoldStatus.Text = string.Empty;
                txtWipStatus.Text = string.Empty;

                Util.gridClear(dgActHistory);

                //btnManualReturn.IsEnabled = false;
                btnEmptyBox.IsEnabled = false;
                btnFullBox.IsEnabled = false;

                btnHideEdit.Visibility = Visibility.Collapsed;
                txtCrrPortId.IsReadOnly = true;
                txtDfctQty.IsReadOnly = true;
                txtRcvQty.IsReadOnly = true;
                txtUseQty.IsReadOnly = true;
                txtRmnQty.IsReadOnly = true;
                txtWipStatus.IsReadOnly = true;
                txtRmnQty.Tag = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        //���� ���� ��� 15ȸ ��Ŭ��
        private void txtRmnQty_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            try
            {
                if (Util.pageAuthCheck(FrameOperation.AUTHORITY))
                {
                    if (!string.IsNullOrEmpty(txtSBoxId.Text) && e.RightButton == MouseButtonState.Released)
                    {
                        int iHiddenEditCnt;

                        if (int.TryParse(tb.Tag?.ToString() ?? "0", out iHiddenEditCnt))
                        {
                            iHiddenEditCnt++;
                        }
                        else
                        {
                            iHiddenEditCnt = 1;
                        }

                        if (iHiddenEditCnt >= 15)
                        {
                            btnHideEdit.Visibility = Visibility.Visible;
                            txtCrrPortId.IsReadOnly = false;
                            txtDfctQty.IsReadOnly = false;
                            txtRcvQty.IsReadOnly = false;
                            txtUseQty.IsReadOnly = false;
                            txtRmnQty.IsReadOnly = false;
                            txtWipStatus.IsReadOnly = false;
                            tb.Tag = null;
                        }

                        tb.Tag = iHiddenEditCnt;

                    }
                    else
                    {
                        tb.Tag = null;
                    }
                }
            }
            catch { }

        }

        private void txtOnlyNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
                || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
                || e.Key == Key.Back))
            {
                e.Handled = true;
            }
        }
    }
}
