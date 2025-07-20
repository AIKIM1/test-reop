/*************************************************************************************
 Created Date : 2019.04.15
      Creator : INS 김동일K
   Decription : CWA3동 증설 - Packaging 공정진척 화면 - Tray 생성 팝업 (ASSY0001.ASSY001_007_TRAY_CREATE 2019/04/15 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.15  INS 김동일K : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;


namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_007_TRAY_CREATE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_007_TRAY_CREATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _TraceYN = string.Empty;
        private string _TrayKind = string.Empty;
        private string _SamePkgLot = string.Empty;

        private string _Create_Out_Lot = string.Empty;
        private string _Create_TrayQty = string.Empty;
        private string _Create_TrayID = string.Empty;

        public string CREATE_OUT_LOT
        {
            get { return _Create_Out_Lot; }
        }

        public string CREATE_TRAY_QTY
        {
            get { return _Create_TrayQty; }
        }

        public string CREATE_TRAYID
        {
            get { return _Create_TrayID; }
        }

        BizDataSet _Biz = new BizDataSet();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY004_007_TRAY_CREATE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 7)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _TraceYN = Util.NVC(tmps[4]);
                _TrayKind = Util.NVC(tmps[5]);
                _SamePkgLot = Util.NVC(tmps[6]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _TraceYN = "";
                _TrayKind = "";
                _SamePkgLot = "";
            }

            ApplyPermissions();

            txtInfo.Text = _TraceYN.Equals("Y") ? "(Create " + _TrayKind + " Cell Tray Type)" : "(Not Use Trace)";

            if (_TraceYN.Equals("Y"))
            {
                txtTrayQty.Text = "0";
                txtTrayQty.IsEnabled = false;
            }

            // 특별 TRAY  사유 Combo
            CommonCombo _combo = new CommonCombo();
            String[] sFilter3 = { "SPCL_RSNCODE" };
            _combo.SetCombo(cboOutTraySplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboOutTraySplReason != null && cboOutTraySplReason.Items != null && cboOutTraySplReason.Items.Count > 0)
                cboOutTraySplReason.SelectedIndex = 0;



            if (_SamePkgLot.Equals("Y")) // 선택한 조립 LOT과 특별트레이로 설정된 조립 LOT과 같은 경우에만 처리.
                GetSpclTrayInfo();
        }

        private void txtTrayId_KeyDown(object sender, KeyEventArgs e)
        {
            // 생성 시 BIZ에서 체크 하여 주석 처리.
            //if (e.Key == Key.Enter)
            //{
            //    DataTable dtRslt = GetTrayInfo(txtTrayId.Text.Trim());

            //    if (dtRslt == null || (dtRslt.Rows.Count > 0 && Util.NVC(dtRslt.Rows[0]["CNT"]).Equals("0")))
            //    {
            //    }
            //    else
            //    {
            //        Util.Alert("이미 존재하는 Tray 입니다.");
            //        txtTrayId.Text = "";
            //        txtTrayId.Focus();
            //    }
            //}
        }

        private void txtTrayQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Util.CheckDecimal(txtTrayQty.Text, 0))
            {
                txtTrayQty.Text = "0";
                return;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateTray())
                return;

            //생성 하시겠습니까?
            Util.MessageConfirm("SFU1621", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (Save())
                    {
                        //셀 추적용
                        if (_TraceYN == "Y")
                        {
                            //switch (_TrayKind)
                            //{
                            //    case "81":
                            //        //frmCNCellIDList.Show(LOTID, EQPTID, trayID, TrayConfirm, MODELID);
                            //        break;
                            //    case "88":
                            //        //frmCNCellIDList.Show(LOTID, EQPTID, trayID, TrayConfirm, MODELID);
                            //        break;
                            //    case "108":
                            //        //frmCNCellIDList2.Show(LOTID, EQPTID, trayID, TrayConfirm, MODELID);
                            //        break;
                            //    case "151":
                            //        //frmCNCellIDList3.Show(LOTID, EQPTID, trayID, TrayConfirm, MODELID);
                            //        break;
                            //    case "66":
                            //        //frmCNCellIDList4.Show(LOTID, EQPTID, trayID, TrayConfirm, MODELID);
                            //        break;
                            //    case "110":
                            //        //frmCNCellIDList5.Show(LOTID, EQPTID, trayID, TrayConfirm, MODELID);
                            //        break;
                            //    case "128":
                            //        //frmCNCellIDList6.Show(LOTID, EQPTID, trayID, TrayConfirm, MODELID);
                            //        break;
                            //    case "132":
                            //        //frmCNCellIDList7.Show(LOTID, EQPTID, trayID, TrayConfirm, MODELID);
                            //        break;
                            //    default:
                            //        break;
                            //}
                        }
                        else
                        {
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        }

                        txtTrayId.Text = "";
                        chkSpecial.IsChecked = false;
                        txtRemark.Text = "";

                        this.DialogResult = MessageBoxResult.OK;
                    }
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizRule]
        private DataTable GetTrayInfo(string sTrayID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_TRAY_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = _LotID;
                newRow["CSTID"] = sTrayID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CNT_TRAY_BY_PROD_LOT", "INDATA", "OUTDATA", inTable);

                HideLoadingIndicator();

                return dtResult;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetSpclTrayInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_SPCL_TRAY_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_SPCL_LOT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["SPCL_PROD_LOTID"]).Equals(_LotID))
                    {
                        chkSpecial.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y") ? true : false;
                        txtRemark.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);

                        if (cboOutTraySplReason != null && cboOutTraySplReason.Items != null && cboOutTraySplReason.Items.Count > 0 && cboOutTraySplReason.Items.CurrentItem != null)
                        {
                            DataView dtview = (cboOutTraySplReason.Items.CurrentItem as DataRowView).DataView;
                            if (dtview != null && dtview.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                            {
                                bool bFnd = false;
                                for (int i = 0; i < dtview.Table.Rows.Count; i++)
                                {
                                    if (Util.NVC(dtview.Table.Rows[i]["CBO_CODE"]).Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"])))
                                    {
                                        cboOutTraySplReason.SelectedIndex = i;
                                        bFnd = true;
                                        break;
                                    }
                                }

                                if (!bFnd && cboOutTraySplReason.Items.Count > 0)
                                    cboOutTraySplReason.SelectedIndex = 0;
                            }
                        }
                        //cboOutTraySplReason.SelectedValue = Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"]);
                    }
                }

                HideLoadingIndicator();
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetTrayValid(out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_CHK_VALID_TRAY();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = "";
                newRow["TRAYID"] = txtTrayId.Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_VALID_TRAYID", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sRet = "OK";
                    sMsg = "";// Util.NVC(dtResult.Rows[0][1]);
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }

                HideLoadingIndicator();
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();

                sRet = "NG";
                sMsg = ex.Message;
            }
        }

        private bool GetFCSTrayCheck(string sTrayID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_FCS_TRAY_CHK_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sTrayID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("", "INDATA", "OUTDATA", inTable);

                HideLoadingIndicator();

                if (dtResult == null || dtResult.Rows.Count <= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private bool Save()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                inTable = indataSet.Tables.Add("IN_SPCL");
                inTable.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
                inTable.Columns.Add("SPCL_CST_NOTE", typeof(string));
                inTable.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = ""; // TRAY MAPPING LOT
                newRow["CSTID"] = txtTrayId.Text;
                newRow["WO_DETL_ID"] = null;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                //newRow = null;

                //DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                //newRow = inMtrlTable.NewRow();
                //newRow["INPUT_LOTID"] = "";
                //newRow["MTRLID"] = "";
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";

                //inMtrlTable.Rows.Add(newRow);

                newRow = null;

                string sRsnCode = cboOutTraySplReason.SelectedValue == null ? "" : cboOutTraySplReason.SelectedValue.ToString();

                inTable = indataSet.Tables["IN_SPCL"];
                newRow = inTable.NewRow();
                newRow["SPCL_CST_GNRT_FLAG"] = (bool)chkSpecial.IsChecked ? "Y" : "N";
                newRow["SPCL_CST_NOTE"] = txtRemark.Text;
                newRow["SPCL_CST_RSNCODE"] = (bool)chkSpecial.IsChecked ? sRsnCode : "";

                inTable.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_OUT_LOT_CL_L", "IN_EQP,IN_INPUT,IN_SPCL", "OUT_LOT", indataSet);
                
                if (CommonVerify.HasTableInDataSet(dsRslt))
                {
                    if (CommonVerify.HasTableRow(dsRslt.Tables["OUT_LOT"]))
                    {
                        if (dsRslt.Tables["OUT_LOT"].Columns.Contains("OUT_LOTID"))
                            _Create_Out_Lot = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["OUT_LOTID"]);
                        else
                            _Create_Out_Lot = "";

                        if (dsRslt.Tables["OUT_LOT"].Columns.Contains("CST_CAPA_QTY"))
                            _Create_TrayQty = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["CST_CAPA_QTY"]);
                        else
                            _Create_TrayQty = "";

                        _Create_TrayID = txtTrayId.Text;
                    }
                }

                HideLoadingIndicator();

                return true;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region [Validation]
        private bool CanCreateTray()
        {
            bool bRet = false;

            string trayID = txtTrayId.Text.ToUpper();
            //정규식표현 false=>영문과숫자이외문자열비허용
            bool chk = System.Text.RegularExpressions.Regex.IsMatch(trayID, @"^[a-zA-Z0-9]+$");

            if (!chk)
            {
                //Util.Alert("입력한 ID ({0}) 에 특수문자가 존재하여 생성할 수 없습니다.", trayID);
                Util.MessageValidation("SFU1811", trayID);
                return bRet;
            }
            if (_TraceYN.Equals("N") && (txtTrayQty.Text.Equals("0") || txtTrayQty.Text.Trim().Length < 1))
            {
                //Util.Alert("수량을 입력하세요.");
                Util.MessageValidation("SFU1684");
                return bRet;
            }

            if (chkSpecial.IsChecked.HasValue && (bool)chkSpecial.IsChecked)
            {
                if (cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return bRet;
                }

                if (txtRemark.Text.Trim().Equals(""))
                {
                    //Util.Alert("특이사항을 입력 하세요.");
                    Util.MessageValidation("SFU1992");
                    return bRet;
                }
            }
            else
            {
                if (!txtRemark.Text.Trim().Equals(""))
                {
                    //Util.Alert("특이사항을 삭제 하세요.");
                    Util.MessageValidation("SFU1991");
                    return bRet;
                }
            }

            // 생성 비즈에서 체크 하므로 주석.
            //string sRet = string.Empty;
            //string sMsg = string.Empty;
            //GetTrayValid(out sRet, out sMsg);

            //if (sRet.Equals("NG"))
            //{
            //    Util.Alert(sMsg);
            //    return bRet;
            //}


            // FCS Check...            
            //if (!GetFCSTrayCheck(trayID))
            //{
            //    Util.Alert("FORMATION에 TRAY ID가 작업중입니다. 활성화에 문의하세요.");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }
        #endregion

        #region[Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion
    }
}
