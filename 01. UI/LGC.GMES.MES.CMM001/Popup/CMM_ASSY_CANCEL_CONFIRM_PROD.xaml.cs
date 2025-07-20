/*************************************************************************************
 Created Date : 2018.07.26
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 실적확정취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.26  INS 김동일K : Initial Created.
  
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
using System.Windows.Input;
using System.Windows.Threading;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_CANCEL_CONFIRM_PROD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CANCEL_CONFIRM_PROD : C1Window, IWorkArea
    {

        private string _ProcID = string.Empty;
        private string _EqsgID = string.Empty;
        private string _EqptID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_CANCEL_CONFIRM_PROD()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 4)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _EqsgID = Util.NVC(tmps[1]);
                    _EqptID = Util.NVC(tmps[2]);
                    txtEqptName.Text = Util.NVC(tmps[3]);
                }
                else
                {
                    _ProcID = "";
                    _EqsgID = "";
                    _EqptID = "";
                }

                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotID.Text.Trim().Equals("")) return;

                    GetConfirmLotInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCancelConfirm())
                    return;

                Util.MessageConfirm("SFU4620", (result) =>// %1 취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CancelConfirmLot();
                    }
                }, ObjectDic.Instance.GetObjectName("확정"));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetConfirmLotInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));               
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;                
                dr["PROCID"] = _ProcID;
                dr["LOTID"] = Util.GetCondition(txtLotID);
                dr["EQPTID"] = _EqptID;
                dr["EQSGID"] = _EqsgID;

                dtRqst.Rows.Add(dr);
                
                new ClientProxy().ExecuteService("DA_PRD_SEL_CONFIRM_LOT_LIST", "INDATA", "OUTDATA", dtRqst, (dtRslt, bizError) =>
                {
                    try
                    {
                        if (bizError != null)
                        {
                            Util.MessageException(bizError);
                            return;
                        }

                        Util.GridSetData(dgLotInfo, dtRslt, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private bool CanCancelConfirm()
        {
            bool bRet = false;

            if (dgLotInfo.ItemsSource == null || dgLotInfo.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1195");    // Lot 정보가 없습니다.
                return bRet;
            }

            if (txtRemark.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1590"); // 비고를 입력하세요.
                return bRet;
            }

            // ERP 마감 체크.
            if (ChkErpClose())
            {
                Util.MessageValidation("SFU3494");  // ERP 생산실적이 마감 되었습니다.
                return bRet;
            }

            // ERP 전송 여부 확인.
            if (ChkErpSending(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "LOTID")),
                                    Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "WIPSEQ"))))
            {
                //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID")));
                Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "LOTID")));
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void CancelConfirmLot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;


                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable in_DATA = indataSet.Tables.Add("INLOT");
                in_DATA.Columns.Add("LOTID", typeof(string));
                in_DATA.Columns.Add("WIPNOTE", typeof(string));
               
                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROCID"] = _ProcID;
                newRow["USERID"] = LoginInfo.USERID;                
                
                inDataTable.Rows.Add(newRow);
                
                newRow = null;

                newRow = in_DATA.NewRow();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "LOTID"));
                newRow["WIPNOTE"] = Util.NVC(txtRemark.Text);
                
                in_DATA.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_END_LOT", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        Util.gridClear(dgLotInfo);

                        txtRemark.Text = "";
                        txtLotID.Text = "";
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotID.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU1190");
                    return;
                }

                GetConfirmLotInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ChkErpClose()
        {
            try
            {
                bool bRet = false;
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "LOTID"));
               
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ERP_CLOSING_FLAG_BY_LOTID", "INDATA", "OUTDATA", inTable);

                if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("ERP_CLOSING_FLAG"))
                {
                    if (Util.NVC(searchResult.Rows[0]["ERP_CLOSING_FLAG"]).ToUpper().Equals("CLOSE"))
                    {
                        bRet = true;
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private bool ChkErpSending(string sLotID, string sWipSeq)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ERP_SEND", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {                    
                    if (!(Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("Y") || Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("N"))) // P : ERP 전송 중 , Y : ERP 전송 완료
                    {
                        bRet = true;
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
