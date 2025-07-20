/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_147 : UserControl, IWorkArea
    {
        private DataSet dsResult = new DataSet();
        List<string> _Rtlslist = new List<string>();
        List<string> _RtlsDefect = new List<string>();
        List<string> _RtlsJudge = new List<string>();
        List<string> _RtlsEqsg = new List<string>();
        List<string> _RtlsProc = new List<string>();
        List<string> _RtlsProd = new List<string>();

        string sCellJudg = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_147()
        {
            InitializeComponent();
 
        }

        

        #region Initialize
        #endregion

        #region Event

        #region Event - UserControl

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
                        txtRemark.Clear();
                        txtLotInfoCreateDate.Clear();
                        txtLotInfoLotType.Clear();
                        txtLotInfoProductName.Clear();
                        txtLotInfoProductId.Clear();
                        txtLotInfoWipLine.Clear();
                        txtLotInfoModel.Clear();
                        txtLotInfoWipProcess.Clear();
                        txtLotInfoWipState.Clear();
                        txtDefept.Clear();
                        txtRemark.Clear();
                        txtRTLS.Clear();
                        SetInfomation(true, txtSearchLotId.Text.Trim());

                        //Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - Button

        private void btnSearchLotId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSearchLotId.Text.Length > 0)
                {
                    txtRemark.Clear();
                    txtLotInfoCreateDate.Clear();
                    txtLotInfoLotType.Clear();
                    txtLotInfoProductName.Clear();
                    txtLotInfoProductId.Clear();
                    txtLotInfoWipLine.Clear();
                    txtLotInfoModel.Clear();
                    txtLotInfoWipProcess.Clear();
                    txtLotInfoWipState.Clear();
                    txtDefept.Clear();
                    txtRemark.Clear();
                    txtRTLS.Clear();
                    SetInfomation(true, txtSearchLotId.Text.Trim());

                    SetLotList();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #endregion

        #region Mehod

        private void SetInfomation(bool bMainLot_SubLot_Flag, string sLotid)
        {

            try
            {
                sCellJudg = string.Empty;

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);
                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_RTLS_GET_PACK_LOT_INFO", "INDATA", "LOT_INFO,WIP_PROC,WIP_ROUTE,TB_SFC_WIP_INPUT_MTRL_HIST,ACTIVITYREASON,PRODUCTPROCESSQUALSPEC,WIPREASONCOLLECT,EM_LOT_MNGT,EM_LOT_JUDG", dsInput, null);
                //WIP_PROC,WIP_ROUTE,TB_SFC_WIP_INPUT_MTRL_HIST,
                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    if ((dsResult.Tables.IndexOf("LOT_INFO") > -1))
                    {
                        if (dsResult.Tables["LOT_INFO"].Rows.Count > 0)
                        {
                            txtLotInfoCreateDate.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["LOTDTTM_CR"]);
                            txtLotInfoLotType.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["LOTTYPE"]);
                            txtLotInfoProductName.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODNAME"]);
                            txtLotInfoProductId.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODID"]);
                            txtLotInfoWipLine.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["EQSGNAME"]);
                            txtLotInfoModel.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["MODLNAME"]);
                            txtLotInfoWipProcess.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PROCNAME"]);
                            txtLotInfoWipState.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["WIPSNAME"]);
                            txtInfoBoxFromArea.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["FROM_AREA_NAME"]);
                            txtLotId.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["LOTID"]);
                            txtGrade.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["GRADE"]);


                            if (Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["HOLD_YN"]) == "Y")
                            {
                                txtHoldYN.Text = "Y";
                                txtHoldYN.ToolTip = "HOLD_RESN  : " + dsResult.Tables["LOT_INFO"].Rows[0]["HOLD_RESN"].ToString() + "\n" +
                                                    "HOLD_DTTM : " + dsResult.Tables["LOT_INFO"].Rows[0]["HOLD_DTTM"].ToString() + "\n" +
                                                    "HOLD_CODE : " + dsResult.Tables["LOT_INFO"].Rows[0]["HOLD_CODE"].ToString() + "\n" +
                                                    "HOLD_NOTE : " + dsResult.Tables["LOT_INFO"].Rows[0]["HOLD_NOTE"].ToString().Trim();
                                txtHold.Text = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["HOLD_REMARK"]);
                            }
                            else
                            {
                                txtHoldYN.Text = "N";
                                txtHold.Text = ObjectDic.Instance.GetObjectName("홀드 상태가 아닙니다.");
                                txtHoldYN.ToolTip = ObjectDic.Instance.GetObjectName("홀드 상태가 아닙니다.");
                            }
                        }
                    }

                    if ((dsResult.Tables.IndexOf("ACTIVITYREASON") > -1))
                    {
                        Util.GridSetData(dgDefect, dsResult.Tables["ACTIVITYREASON"], FrameOperation, true);
                    }
                    
                    if (dsResult.Tables["EM_LOT_MNGT"].Rows.Count > 0)
                    {
                        txtDefept.Text = Util.NVC(dsResult.Tables["EM_LOT_MNGT"].Rows[0]["RESNNAME"]);

                        txtRemark.Text = Util.NVC(dsResult.Tables["EM_LOT_MNGT"].Rows[0]["NOTE"]);
                        txtClctitem.Text = Util.NVC(dsResult.Tables["EM_LOT_MNGT"].Rows[0]["ITEM_LIST"]);
                        txtClctitemValue.Text = Util.NVC(dsResult.Tables["EM_LOT_MNGT"].Rows[0]["VALUE_LIST"]);

                        if (dgDefect.Rows.Count > 0)
                        {
                            for (int i = 0; i < dgDefect.Rows.Count; i++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE")).ToString() == Util.NVC(dsResult.Tables["EM_LOT_MNGT"].Rows[0]["FINL_EM_DFCT_CODE"]))
                                {
                                    DataTableConverter.SetValue(dgDefect.Rows[i].DataItem, "CHK", true);
                                }
                            }
                        }
                    }
                    if (dsResult.Tables["EM_LOT_JUDG"].Rows.Count > 0)
                    {
                        sCellJudg = Util.NVC(dsResult.Tables["EM_LOT_JUDG"].Rows[0]["JUDGCODE"]);
                        txtRTLS.Text = Util.NVC(dsResult.Tables["EM_LOT_JUDG"].Rows[0]["JUDGNAME"]);

                    }

                    _Rtlslist.Add(sLotid);
                    _RtlsEqsg.Add(txtLotInfoWipLine.Text);
                    _RtlsProc.Add(txtLotInfoWipProcess.Text);
                    _RtlsDefect.Add(txtDefept.Text);
                    _RtlsProd.Add(txtLotInfoProductId.Text);

                }
            }
            catch (Exception ex)
            {

                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtSearchLotId.SelectAll();
                        txtSearchLotId.Focus();
                    }
                });

            }
            finally
            {

                txtSearchLotId.SelectAll();
                txtSearchLotId.Focus();
            }
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });


            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Save()
        {
            try
            {
                string sResnCode = string.Empty;

                sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");

                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("EM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EM_JUDG_RSLT_CODE", typeof(string));
                RQSTDT.Columns.Add("FINL_EM_DFCT_CODE", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));
                RQSTDT.Columns.Add("INSUSER", typeof(string));
                RQSTDT.Columns.Add("INSDTTM", typeof(DateTime));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM", typeof(DateTime));

                DataRow row = RQSTDT.NewRow();
                row["LOTID"] = txtLotId.Text.Trim();
                row["EM_TYPE_CODE"] = "UI";
                row["EM_JUDG_RSLT_CODE"] = sCellJudg;
                row["FINL_EM_DFCT_CODE"] = sResnCode;
                row["NOTE"] = txtRemark.Text;
                row["INSUSER"] = LoginInfo.USERID;
                row["INSDTTM"] = DateTime.Now;
                row["UPDUSER"] = LoginInfo.USERID;
                row["UPDDTTM"] = DateTime.Now;

                indataSet.Tables["RQSTDT"].Rows.Add(row);


                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_RTLS_INS_EM_LOT_MNGT_UI", "RQSTDT", "", (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1518");
                    

                }, indataSet);

               }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetLotList()
        {
            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("SCANLOT", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("DEFECT", typeof(string));

            for (int i = 0; i < _Rtlslist.Count; i++)
            {
                DataRow dr = dt.NewRow();
                dr["SCANLOT"] = _Rtlslist[i].ToString();
                dr["EQSGID"] = _RtlsEqsg[i].ToString();
                dr["PROCID"] = _RtlsProc[i].ToString();
                dr["PRODID"] = _RtlsProd[i].ToString();
                dr["DEFECT"] = _RtlsDefect[i].ToString();
                dt.Rows.Add(dr);
            }

             Util.GridSetData(dgRtlsList, dt, FrameOperation, true);

        }
        private void dgDefectChoice_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void dgDefect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgRtlsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgRtlsList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "SCANLOT")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
    }
}
