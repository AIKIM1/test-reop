/*************************************************************************************
 Created Date : 2017.02.08
      Creator : INS ������C
   Decription : ���� 5MEGA-GMES ���� - STP ������ô ȭ�� - ���LOT ��ȸ �˾�
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.08  INS ������C : Initial Created.
  2023.10.10  ������  E20230828-000346 btnHoldRelease ���Ѻ��� ǥ��   
  2023.12.18  �ڽ·� : E20231103-001744 ���� �����ڵ� ��ȸ INDATA �� USE_FLAG �߰� 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_009_WAITLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();
        #endregion

        #region Initialize

        /// <summary>
        ///  Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY001_009_WAITLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event       

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _LineID = Util.NVC(tmps[0]);
            }
            else
            {
                _LineID = "";
            }

            #region 2023.10.10 ������ E20230828-000346 ���Ѻ� ��ưǥ�� ����
            // �ּ�ó��
            //ApplyPermissions();

            // 2023.10.10 ������ E20230828-000346 ���Ѻ� ��ưǥ�� ����
            GetButtonPermissionGroup();
            #endregion

            rdoLBaseL.IsChecked = true;

            // HOLD ����
            CommonCombo _combo = new CommonCombo();

            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
        }
        #endregion

        #region [HOLD - ����]
        private void btnExecute_Click(object Sender, RoutedEventArgs e)
        {
            if (!CanExecute())
                return;

            HoldProcess();
        }
        #endregion

        #region [����]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [##### �����ʿ�]
        private void rdoAType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitMagazine();
        }

        private void rdoCType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitMagazine();
        }
        #endregion

        #region ����������
        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                //Util.AlertInfo("�������ĳ�¥�����������մϴ�.");
                Util.MessageValidation("SFU1740");
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// 
        /// </summary>
        private void GetWaitMagazine()
        {
            try
            {
                string sMazType = string.Empty;

                Util.gridClear(dgWaitLot);

                ShowLoadingIndicator();
                
                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.STP;
                //newRow["EQPTID"] = _EqptID;
                newRow["EQSGID"] = _LineID;
                newRow["ELECTYPE"] = sMazType;
                ////newRow["LOTID"] = txtWaitLot == null ? "" : txtWaitLot.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_FD", "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(dgWaitLot, bizResult, null, true);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void HoldProcess()
        {
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("HOLD �Ͻðڽ��ϱ�?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("LANGID", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("HOLD_NOTE", typeof(string));
                        inLot.Columns.Add("RESNCODE", typeof(string));
                        inLot.Columns.Add("HOLD_CODE", typeof(string));
                        inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));


                        DataRow inRow = inDataTable.NewRow();
                        inRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inRow["LANGID"] = LoginInfo.LANGID;
                        inRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inRow["USERID"] = LoginInfo.USERID;

                        inDataTable.Rows.Add(inRow);
                        inRow = null;

                        for (int i = 0; i < dgWaitLot.Rows.Count; i++)
                        {
                            if (!_util.GetDataGridCheckValue(dgWaitLot, "CHK", i)) continue;

                            inRow = inLot.NewRow();
                            inRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "LOTID"));
                            inRow["HOLD_NOTE"] = txtRemark.Text;
                            inRow["RESNCODE"] = cboHoldReason.SelectedValue.ToString();
                            inRow["HOLD_CODE"] = cboHoldReason.SelectedValue.ToString();
                            inRow["UNHOLD_SCHD_DATE"] = dtExpected.SelectedDateTime.ToString("yyyyMMdd");

                            inLot.Rows.Add(inRow);
                        }

                        if (inLot.Rows.Count < 1)
                            return;                        

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, (bizResult, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //���� ó�� �Ǿ����ϴ�.
                                GetWaitMagazine();

                                txtRemark.Text = "";
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, inDataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
            });
        }
        
        #endregion

        #region [Validation]
        private bool CanExecute()
        {
            bool bRet = false;

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgWaitLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("���õ� �׸��� �����ϴ�.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            //if (rdoHold.IsChecked.HasValue && !(bool)rdoHold.IsChecked && rdoRelease.IsChecked.HasValue && !(bool)rdoRelease.IsChecked)
            //{
            //    Util.Alert("Hold �Ǵ� Hold������ ���� �ϼ���.");
            //    return bRet;
            //}

            //if (rdoHold.IsChecked.HasValue && (bool)rdoHold.IsChecked)
            //{
            //    if (txtRemark.Text.Trim().Equals(""))
            //    {
            //        Util.Alert("Hold�� ���� ������ �Է� �ϼ���.");
            //        return bRet;
            //    }
            //}

            if (cboHoldReason == null || cboHoldReason.SelectedValue == null || cboHoldReason.SelectedValue.ToString().Equals("SELECT"))
            {
                //Util.Alert("HOLD ������ ���� �ϼ���.");
                Util.MessageValidation("SFU1342");
                return bRet;
            }

            for (int i = 0; i < dgWaitLot.Rows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgWaitLot, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "WIPHOLD")).Equals("Y"))
                {
                    //Util.Alert("�̹� HOLD������ LOT�� ���� �Ǿ����ϴ�.");
                    Util.MessageValidation("SFU1773");
                    return bRet;
                }
            }

            if (txtRemark.Text.Trim().Equals(""))
            {
                //Util.Alert("LOT HOLD �� ���� ������ �Է��� �ּ���.");
                Util.MessageValidation("SFU1214");
                return bRet;
            }

            //if (rdoRelease.IsChecked.HasValue && (bool)rdoRelease.IsChecked)
            //{
            //if (txtRemark.Text.Trim().Equals(""))
            //    {
            //        Util.Alert("Release�� ���� ������ �Է� �ϼ���.");
            //        return bRet;
            //    }
            //}

            bRet = true;

            return bRet;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnHoldRelease);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 2023.10.10 ������ E20230828-000346 ���Ѻ� ��ưǥ�� ����
        private bool ChkButtonPermissionSetYN()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ATTR2", typeof(string));
                inTable.Columns.Add("ATTR3", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["COM_TYPE_CODE"] = "PERMISSIONS_PER_BUTTON";
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["ATTR2"] = this.GetType().Name;
                dtRow["ATTR3"] = "HOLD_W";
                dtRow["USE_FLAG"] = "Y";

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
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
                Util.MessageException(ex);
                return false;
            }
        }

        // 2023.10.10 ������ E20230828-000346 ���Ѻ� ��ưǥ�� ����
        private void GetButtonPermissionGroup()
        {
            try
            {
                // Read ������ �� btnHoldRelease ����
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                {
                    btnHoldRelease.Visibility = Visibility.Collapsed;
                    return;
                }
                if (ChkButtonPermissionSetYN())
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));
                    inTable.Columns.Add("FORMID", typeof(string));

                    DataRow dtRow = inTable.NewRow();
                    dtRow["USERID"] = LoginInfo.USERID;
                    dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dtRow["FORMID"] = this.GetType().Name;

                    inTable.Rows.Add(dtRow);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP", "INDATA", "OUTDATA", inTable);

                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                    {
                        if (dtRslt.Columns.Contains("BTN_PMS_GRP_CODE"))
                        {
                            foreach (DataRow drTmp in dtRslt.Rows)
                            {
                                if (drTmp == null) continue;

                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "HOLD_W": // Hold ��ư ��� ����
                                        btnHoldRelease.Visibility = Visibility.Visible;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        btnHoldRelease.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    btnHoldRelease.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

    }
}
