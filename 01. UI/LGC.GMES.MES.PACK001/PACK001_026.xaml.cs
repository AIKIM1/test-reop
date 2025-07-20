/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.01.30 �տ켮 CSR ID 3553461 GMES ���ϰ˻� �Ƿ� ��� ����� �� ��û��ȣ C20171211_53461
  2018.06.07 �տ켮 ���ϰ˻� ���ø� �˻� ���� �߰�
  2018.07.18 �տ켮 CSR ID 3704751 [1.��������/����] GMES ȭ�鿡 ��ǰ ���� �߰� ��û ��û��ȣ C20180604_04751
  2019.12.23 �տ켮 SM ��ȸ ��� ���� ��û
  2020.01.02 �տ켮 SM ��ȸ ��� ���� ��û
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_026 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util _util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        public PACK001_026()
        {
            InitializeComponent();
            Loaded += PACK001_026_Loaded;
        }

        private void PACK001_026_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PACK001_026_Loaded;
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnQARequest);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //2018-01-30
            txtWorker.Text = LoginInfo.USERID;
            txtWorkerName.Text = LoginInfo.USERNAME;

            SetComboBox();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        #region Event - [1��] ��������

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPalletInfo();
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtPalletID.Text))
                {
                    // Pallet Lot
                    string lsScanID = txtPalletID.Text.Substring(0, 1);

                    // Pallet�� �ƴ϶��, �Ʒ� ���� �������� ����
                    if (lsScanID != "P")
                    {

                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1411"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //PALLETID�� �Է����ּ���
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtPalletID.Focus();
                                txtPalletID.SelectAll();
                            }
                        });
                        return;
                    }
                    else
                    {
                        int trayRows = 0;
                        trayRows = dgTray.GetRowCount();
                        int eCnt = 0;

                        //���� ���� �ߺ� ���� üũ
                        if (trayRows != 0)
                        {
                            for (int i = 0; i < trayRows; i++)
                            {
                                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["BOXID"].Index).Value) == txtPalletID.Text)
                                {
                                    eCnt++;
                                }
                            }

                            if (eCnt != 0)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1776"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                //�̹� ���� ������ �ش� Pallet ID�� �����մϴ�.
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtPalletID.Focus();
                                        txtPalletID.SelectAll();
                                    }
                                });
                                return;
                            }
                            // �ܷ��� �ð��� üũ�Ͽ� 5������ �Ƿ��� Pallet ID�� ���Ƿ��� �� ������ ��.
                            // ������� ��û����.
                            if (Check2QAREQ(txtPalletID.Text.ToString().Trim()) == "NG")
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1301"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                //5�� ���� �Ƿڵ� PALLET ID �Դϴ�.
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtPalletID.Focus();
                                        txtPalletID.SelectAll();
                                    }
                                });
                                return;
                            }

                        }

                        SelectTrayInfo(txtPalletID.Text.Trim());

                        if (dgTray.Rows.Count == 0)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3343"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //DATA����: �̹� �˻��Ƿ� �Ǿ��ų� �������� PALLET �Դϴ�. [�������� Ȯ��]
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID.Focus();
                                    txtPalletID.SelectAll();
                                }
                            });
                            return;
                        }
                        txtPalletID.Text = "";
                    }

                }

                if (dgTray.GetRowCount() > 0)
                    dgTray.ScrollIntoView(dgTray.GetRowCount() - 1, 0);

                //�ϴ� text�ڽ� count ��
                DataTable dtResult = DataTableConverter.Convert(dgTray.ItemsSource);
                SetText_SearchResultCount(txtSelPalletQty, txtSelBoxQty, txtSelLotQty, dtResult);
            }
        }

        /// <summary>
        /// �������� ������ư Ŭ���� �����ϴ� �̺�Ʈ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // �۾��ڿ��� �ٽ� �� �� ���� ���� ����
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            //System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

            string sPalletid = Util.NVC(dgTray.GetCell(iRow, dgTray.Columns["BOXID"].Index).Value);
            //������ Pallet ��ȸ ��Ʈ�� �ش� PalletID üũ�ڽ� ����
            for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
            {
                if (sPalletid == Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["BOXID"].Index).Value))
                {
                    DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", false);
                }
            }
            // ���õ� �� ����
            dgTray.IsReadOnly = false;
            dgTray.RemoveRow(iRow);
            dgTray.IsReadOnly = true;

            //�ϴ� text�ڽ� count ��
            DataTable dtResult = DataTableConverter.Convert(dgTray.ItemsSource);
            SetText_SearchResultCount(txtSelPalletQty, txtSelBoxQty, txtSelLotQty, dtResult);
        }

        private void btnDelete_All_Click(object sender, RoutedEventArgs e)
        {
            for (int iRow = dgTray.Rows.Count - 1; iRow > -1; iRow--)
            {
                //System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

                string sPalletid = Util.NVC(dgTray.GetCell(iRow, dgTray.Columns["BOXID"].Index).Value);
                //������ Pallet ��ȸ ��Ʈ�� �ش� PalletID üũ�ڽ� ����
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (sPalletid == Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["BOXID"].Index).Value))
                    {
                        DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", false);
                        break;
                    }
                }
                // ���õ� �� ����
                dgTray.IsReadOnly = false;
                dgTray.RemoveRow(iRow);
                dgTray.IsReadOnly = true;
            }

            //�ϴ� text�ڽ� count ��
            DataTable dtResult = DataTableConverter.Convert(dgTray.ItemsSource);
            SetText_SearchResultCount(txtSelPalletQty, txtSelBoxQty, txtSelLotQty, dtResult);
        }

        /// <summary>
        /// QA �˻��Ƿ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQARequest_Click(object sender, RoutedEventArgs e)
        {

            if (dgTray.GetRowCount() <= 0)
            {
                ms.AlertWarning("SFU1882"); //�������� ���� �˻��Ƿ�PALLET�� üũ�� �ּ���.
                return;
            }

            DataTable dtDgTray = DataTableConverter.Convert(dgTray.ItemsSource);
            DataColumnCollection columnsDtDgTray = dtDgTray.Columns;

            if(columnsDtDgTray.Contains("OQC_FAIL_YN") && dtDgTray.Select("OQC_FAIL_YN = 'Y'").Length > 0)
            {
                ms.AlertWarning("SFU9012"); //�ش� PALLET �� QMS �������� �ҷ��� LOT�� �����մϴ�.
                return;
            }

            if (columnsDtDgTray.Contains("OQC_HOLD_FLAG") && dtDgTray.Select("OQC_HOLD_FLAG = 'Y'").Length > 0)
            {
                ms.AlertWarning("SFU9014"); //�ش� PALLET �� QMS �������� �ҷ��� LOT�� �����մϴ�.
                return;
            }

            string sShipdate_Schedule = dtpShipDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";

            //string sMsg = "���Ͽ������� " + sShipdate_Schedule.Substring(0, 10) + " �̸� Ȯ�ι�ư�� ���� �ּ���.";
            string sMsg = ms.AlertRetun("SFU3344", sShipdate_Schedule.Substring(0, 10)); //���Ͽ������� %1 �̸� Ȯ�ι�ư�� ���� �ּ���.
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(sMsg, null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    QARequestSend();
                }
            });
        }

        private void btnSelect_All_Click(object sender, RoutedEventArgs e)
        {
            string sPalletid = string.Empty;
            try
            {
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    string sChkValue = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value);
                    sPalletid = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["BOXID"].Index).Value);

                    if (sChkValue == "0")
                    {


                        int trayRows = 0;
                        trayRows = dgTray.GetRowCount();
                        bool bCheck = true;

                        //���� ���� �ߺ� ���� üũ
                        if (trayRows != 0)
                        {
                            for (int j = 0; j < trayRows; j++)
                            {
                                if (Util.NVC(dgTray.GetCell(j, dgTray.Columns["BOXID"].Index).Value) == sPalletid)
                                {
                                    bCheck = false;
                                    break;
                                }
                            }

                        }

                        if (bCheck)
                        {
                            DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", true);

                            //Tray ���� ��ȸ �Լ� ȣ��
                            SelectTrayInfo(sPalletid);
                        }
                        bCheck = true;
                    }
                }
                //�ϴ� text�ڽ� count ��
                DataTable dtResult = DataTableConverter.Convert(dgTray.ItemsSource);
                SetText_SearchResultCount(txtSelPalletQty, txtSelBoxQty, txtSelLotQty, dtResult);

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        private void dgPalletInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (dgPalletInfo.CurrentRow == null || dgPalletInfo.SelectedIndex == -1)
            {
                return;
            }
            else if (e.ChangedButton.ToString().Equals("Left") && dgPalletInfo.CurrentColumn.Name == "CHK")
            {
                string sPalletid = string.Empty;
                string strQmsFailYn = string.Empty;
                string strQmsHoldYn = string.Empty;
                try
                {
                    // Rows Count�� 0���� Ŭ ��쿡�� �̺�Ʈ �߻��ϵ���
                    if (dgPalletInfo.GetRowCount() > 0)
                    {
                        DataTable dt = DataTableConverter.Convert(dgPalletInfo.ItemsSource);

                        string sChkValue = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value);
                        sPalletid        = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["BOXID"].Index).Value);
                        strQmsFailYn     = Util.NVC(DataTableConverter.GetValue(dgPalletInfo.CurrentRow.DataItem, "OQC_FAIL_YN"));
                        strQmsHoldYn = Util.NVC(DataTableConverter.GetValue(dgPalletInfo.CurrentRow.DataItem, "OQC_HOLD_FLAG"));
                        //strQmsFailYn = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["QMS_FAIL_YN"].Index).Value);

                        //MouseUp �̺�Ʈ -> üũ �̺�Ʈ�� �ƴ϶�.. ���� üũ
                        if (sChkValue == "1")
                        {
                            DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);
                            sChkValue = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", true);
                            sChkValue = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value);
                        }


                        if (sChkValue == "1")
                        {
                            if (strQmsHoldYn == "Y")
                            {
                                ms.AlertWarning("SFU9014"); //�ش� PALLET �� HOLD�� �����Դϴ�
                                DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);
                                return;
                            }

                            if (strQmsFailYn == "F")
                            {
                                ms.AlertWarning("SFU9012"); //QMS �������� �ҷ��� ���� �����մϴ�. 
                                DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);
                                return;
                            }

                            int trayRows = 0;
                            trayRows = dgTray.GetRowCount();

                            // �ܷ��� �ð��� üũ�Ͽ� 5������ �Ƿ��� Pallet ID�� ���Ƿ��� �� ������ ��.
                            // ������� ��û����.
                            if (Check2QAREQ(sPalletid) == "NG")
                            {
                                ms.AlertWarning("SFU1301"); //5�� ���� �Ƿڵ� PALLET ID �Դϴ�.
                                DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);
                                return;
                            }

                            //���� ���� �ߺ� ���� üũ
                            if (trayRows != 0)
                            {
                                for (int i = 0; i < trayRows; i++)
                                {
                                    if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["BOXID"].Index).Value) == sPalletid)
                                    {
                                        ms.AlertWarning("SFU1776"); //�̹� ���� ������ �ش� Pallet ID�� �����մϴ�.
                                        return;
                                    }
                                }
                            }

                            //Tray ���� ��ȸ �Լ� ȣ��
                            SelectTrayInfo(sPalletid);
                        }
                        else
                        {
                            int iwoListIndex = Util.gridFindDataRow(ref dgTray, "BOXID", sPalletid, false);
                            if (iwoListIndex > 0)
                            {
                                DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", true);
                                ms.AlertWarning("SFU2015"); //�ش� PALLETID�� �������� ��Ʈ���� ������ �ּ���.
                                return;
                            }
                        }

                        // ��ĵ�� ������ ���� �ٷ� ���̵��� �������� ��ũ�� �ϴ����� �̵�
                        if (dgTray.GetRowCount() > 0)
                            dgTray.ScrollIntoView(dgTray.GetRowCount() - 1, 0);

                        //�ϴ� text�ڽ� count ��
                        DataTable dtResult = DataTableConverter.Convert(dgTray.ItemsSource);
                        SetText_SearchResultCount(txtSelPalletQty, txtSelBoxQty, txtSelLotQty, dtResult);
                    }
                }
                catch (Exception ex)
                {
                    Util.AlertInfo(ex.Message);
                    return;
                }
                finally
                {
                    dgPalletInfo.CurrentRow = null;
                }
            }

        }
        private void dgTray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTray.GetCellFromPoint(pnt);
                BoxInfoPopUp_Open(cell, "INFO_BOX");
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgPalletInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPalletInfo.GetCellFromPoint(pnt);
                BoxInfoPopUp_Open(cell, "INFO_BOX");
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void dgPalletInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                gridLoadCellEventFunction(sender, e);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgTray_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                gridLoadCellEventFunction(sender, e);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnShtInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgTray);

                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", false);
                }

                //�ϴ� text�ڽ� count ��
                DataTable dtResult = DataTableConverter.Convert(dgTray.ItemsSource);
                SetText_SearchResultCount(txtSelPalletQty, txtSelBoxQty, txtSelLotQty, dtResult);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }
        #endregion

        #region Event - [2��] �˻��̷���ȸ
        private void btnQASearch_Click(object sender, RoutedEventArgs e)
        {
            GetQAReqHis();
        }

        private void txtSearch_PalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtSearch_PalletID.Text))
                    {
                        // Pallet Lot
                        string lsScanID = txtSearch_PalletID.Text.ToUpper().Substring(0, 1);

                        // Pallet�� �ƴ϶��, �Ʒ� ���� �������� ����
                        if (lsScanID != "P")
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1411"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //PALLETID�� �Է����ּ���
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtSearch_PalletID.Focus();
                                    txtSearch_PalletID.SelectAll();
                                }
                            });
                            return;

                        }
                        else
                        {
                            bool bIN = false;
                            for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                            {
                                if (txtSearch_PalletID.Text.ToUpper().Trim() == (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["BOXID"].Index).Value)))
                                {
                                    bIN = true;
                                    //sprQASelect.SetViewportTopRow(0, i);
                                    dgQAReqHis.ScrollIntoView(i, 0);

                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                    //�̹� ��ȸ�� Pallet ID �Դϴ�.
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            txtSearch_PalletID.Focus();
                                            txtSearch_PalletID.SelectAll();
                                        }
                                    });
                                    return;
                                }

                            }
                            if (!bIN)
                            {
                                selectQAHistInfo_BY_PALLETID(txtSearch_PalletID.Text.Trim());
                            }
                        }
                    }
                    txtSearch_PalletID.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        //2018-01-30 Start ============================================================================
        //private void chkPalletQty_Click(object sender, RoutedEventArgs e)
        //{
        //    if (chkPalletQty.IsChecked == true)
        //    {
        //        Util.AlertInfo("üũ�� ��� PALLET ���� 5�� üũ���� ���ܽ�ŵ�ϴ�. �����ϼ���!!!");
        //    }
        //}

        //private void chkLine_Click(object sender, RoutedEventArgs e)
        //{
        //    if (chkLine.IsChecked == true)
        //    {
        //        Util.AlertInfo("üũ�� ��� ���� ȥ���� �� �� �ֽ��ϴ�. �����ϼ���!!!");
        //    }
        //}

        //private void btnShift_Click(object sender, RoutedEventArgs e)
        //{
        //    GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
        //    wndPopup.FrameOperation = FrameOperation;

        //    if (wndPopup != null)
        //    {
        //        object[] Parameters = new object[5];
        //        Parameters[0] = LoginInfo.CFG_SHOP_ID;
        //        Parameters[1] = LoginInfo.CFG_AREA_ID;
        //        Parameters[2] = LoginInfo.CFG_EQPT_ID;
        //        Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
        //        Parameters[4] = "";
        //        C1WindowExtension.SetParameters(wndPopup, Parameters);

        //        wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
        //        //grdMain.Children.Add(wndPopup);
        //        wndPopup.BringToFront();
        //    }
        //}

        //private void wndShiftUser_Closed(object sender, EventArgs e)
        //{
        //    GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {
        //        txtWorker.Text = window.USERNAME;
        //        txtWorker.Tag = window.USERID;
        //    }
        //    //grdMain.Children.Remove(window);
        //}

        private void btnQACancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtWorker.Text))
                //{
                //    //�۾��ڸ� ������ �ּ���
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                //    return;
                //}

                //SFU1168 �۾��� ����Ͻðڽ��ϱ�? 
                Util.MessageConfirm("SFU1168", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        if (_util.GetDataGridCheckCnt(dgQAReqHis, "CHK") < 1)
                        {
                            Util.MessageValidation("SFU1636"); //���õ� ����� �����ϴ�.
                            return;
                        }

                        List<DataRow> drList = DataTableConverter.Convert(dgQAReqHis.ItemsSource).Select("CHK = '1' AND INSP_PROG_CODE= 'REQUEST'").ToList();
                        if (drList.Count <= 0)
                        {
                            Util.MessageValidation("SFU1939"); //��� �� �� �ִ� ���°� �ƴմϴ�.	
                            return;
                        }

                        DataTable dtInfo = drList.CopyToDataTable();
                        List<string> sIdList = dtInfo.AsEnumerable().Select(c => c.Field<string>("OQC_INSP_REQ_ID")).Distinct().ToList();

                        DataSet indataSet = new DataSet();
                        DataTable dtIndata = indataSet.Tables.Add("INDATA");
                        dtIndata.Columns.Add("OQC_INSP_REQ_ID", typeof(string));
                        dtIndata.Columns.Add("USERID", typeof(string));

                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = dtIndata.NewRow();
                            drIndata["OQC_INSP_REQ_ID"] = id;
                            drIndata["USERID"] = txtWorker.Text;
                            dtIndata.Rows.Add(drIndata);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_OQC_PACK", "INDATA", null, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            else
                            {
                                Util.MessageInfo("SFU1747");// ��û�Ǿ����ϴ�.	
                                GetQAReqHis();
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }
        //2018-01-30 End   ============================================================================

        private void rdoUseY_Click(object sender, RoutedEventArgs e)
        {
            if (rdoUseY.IsChecked == true)
            {
                btnSave.IsEnabled = true;
                btnRePrint.IsEnabled = false;
                //btnReMail.IsEnabled = false;
                //���� ��ư Ŭ���� ���ÿ� ���� ���� ��ȸ
                //selectQAHistInfo(dtFrom2.Value, dtTo2.Value, cboLine2.SelectedValue.ToString(), cboModel.SelectedValue.ToString());
                GetQAReqHis();


                for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                {
                    if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["DEL_FLAG"].Index).Value) == "D")
                    {
                        //sprQASelect_Sheet1.Rows[i].Visible = true;
                        dgQAReqHis.Rows[i].Visibility = Visibility.Visible;
                        //sprQASelect_Sheet1.Rows[i].ForeColor = Color.OrangeRed;
                        //sprQASelect_Sheet1.Rows[i].Font = new Font("", 9, FontStyle.Strikeout);
                    }

                }

            }
            else if (rdoUseN.IsChecked == true)
            {
                btnSave.IsEnabled = false;
                btnRePrint.IsEnabled = true;
                //btnReMail.IsEnabled = true;
                //selectQAHistInfo(dtFrom2.Value, dtTo2.Value, cboLine2.SelectedValue.ToString(), cboModel.SelectedValue.ToString());
                GetQAReqHis();

                for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                {
                    if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["DEL_FLAG"].Index).Value) == "D")
                    {
                        //sprQASelect_Sheet1.Rows[i].Visible = false;
                        dgQAReqHis.Rows[i].Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// Tag �����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgQAReqHis.CurrentRow != null)
                {
                    string sPalletID = Util.NVC(DataTableConverter.GetValue(dgQAReqHis.CurrentRow.DataItem, "BOXID"));
                    string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgQAReqHis.CurrentRow.DataItem, "EQSGID"));

                    setTagReport(sPalletID, sEQSGID);
                }
                else
                {
                    ms.AlertWarning("SFU1560"); //������ PALLET�� �����ϼ���.
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgQAReqHis_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                gridLoadCellEventFunction(sender, e);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgQAReqHis_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgQAReqHis.GetCellFromPoint(pnt);
                BoxInfoPopUp_Open(cell, "INFO_OQC");
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2018-01-30 Start ============================================================================
        private void dgQAReqHis_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    //row �� �ٲٱ�
                                    dg.SelectedIndex = e.Cell.Row.Index;
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue)
                                   //(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    }
                }
            }));
        }
        //2018-01-30 End   ============================================================================

        #endregion

        #endregion

        #region Mehod

        private void SetComboBox()
        {
            //��¥ �޺� ��
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-1);
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpShipDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpSearch_DateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpSearch_DateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
            //dtpSearch_ShipDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            // Area ����
            String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboPrjModel };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, cbChild: cboAreaChild, sCase: "AREA_AREATYPE");

            //�۾��� ��ȸ ����
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboPrjModel };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild, sCase: "EQUIPMENTSEGMENT");

            C1ComboBox[] cboPrjModelParent = { cboArea, cboEquipmentSegment };
            _combo.SetCombo(cboPrjModel, CommonCombo.ComboStatus.ALL, cbParent: cboPrjModelParent, sCase: "PRJ_MODEL");

            //������� Combo Set.
            //string[] sFilter3 = { "BOXSTAT" };
            //_combo.SetCombo(cboPackStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            //�˻��Ƿ��̷���ȸ


            // Area ����
            String[] sFilter_AreaSearch = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cboSearch_AreaChild = { cboSearch_EquipmentSegment, cboSearch_PrjModel };
            _combo.SetCombo(cboSearch_Area, CommonCombo.ComboStatus.SELECT, sFilter: sFilter_AreaSearch, cbChild: cboSearch_AreaChild, sCase: "AREA_AREATYPE");

            //��ȸ ����
            C1ComboBox[] cboSearch_EquipmentSegmentParent = { cboSearch_Area };
            C1ComboBox[] cboSearch_EquipmentSegmentChild = { cboSearch_PrjModel };
            _combo.SetCombo(cboSearch_EquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboSearch_EquipmentSegmentParent, cbChild: cboSearch_EquipmentSegmentChild, sCase: "EQUIPMENTSEGMENT");


            C1ComboBox[] cboSearch_PrjModelParent = { cboSearch_Area, cboSearch_EquipmentSegment };
            _combo.SetCombo(cboSearch_PrjModel, CommonCombo.ComboStatus.ALL, cbParent: cboSearch_PrjModelParent, sCase: "PRJ_MODEL");

            //������� Combo Set.
            string[] sFilterJUDGE = { "JUDGE_VALUE" };
            _combo.SetCombo(cboSearch_Judge_Value, CommonCombo.ComboStatus.ALL, sFilter: sFilterJUDGE, sCase: "COMMCODE");
        }

        /// <summary>
        /// Pallet ���� ��ȸ
        /// </summary>
        private void GetPalletInfo()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("MODELID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");    // Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");
                dr["AREAID"] = Util.NVC(cboArea.SelectedValue) == "" ? null : Util.NVC(cboArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PALLETID"] = null; // txtPalletID.Text.Trim() == "" ? null : txtPalletID.Text.Trim();
                dr["MODELID"] = Util.NVC(cboPrjModel.SelectedValue) == "" ? null : Util.NVC(cboPrjModel.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_PLT_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                //dgPalletInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);
                //�ϴ� text�ڽ� count ��
                SetText_SearchResultCount(txtPalletQty, txtBoxQty, txtLotQty, dtResult);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_OQC_PLT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// �˻��Ƿ� �̷���ȸ
        /// </summary>
        private void GetQAReqHis()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROMDTTM", typeof(string));
                RQSTDT.Columns.Add("TODTTM", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODELID", typeof(string));
                RQSTDT.Columns.Add("JUDG_VALUE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDTTM"] = dtpSearch_DateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODTTM"] = dtpSearch_DateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = Util.NVC(cboSearch_EquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboSearch_EquipmentSegment.SelectedValue);
                dr["MODELID"] = Util.NVC(cboSearch_PrjModel.SelectedValue) == "" ? null : Util.NVC(cboSearch_PrjModel.SelectedValue);
                dr["JUDG_VALUE"] = Util.NVC(cboSearch_Judge_Value.SelectedValue) == "" ? null : Util.NVC(cboSearch_Judge_Value.SelectedValue);
                //2019.12.23
                //dr["PALLETID"] = null;
                //2020.01.02
                //dr["PALLETID"] = txtSearch_PalletID.Text.Trim();
                dr["PALLETID"] = Util.NVC(txtSearch_PalletID.Text.Trim()) == "" ? null : Util.NVC(txtSearch_PalletID.Text.Trim());
                dr["AREAID"] = Util.NVC(cboSearch_Area.SelectedValue) == "" ? null : Util.NVC(cboSearch_Area.SelectedValue);                
                
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_QAREQ_HIS_PACK", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    //dgQAReqHis.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgQAReqHis, dtResult, FrameOperation, true);
                    // text�ڽ� count ��
                    SetText_SearchResultCount(txtSearch_PalletQty, txtSearch_BoxQty, txtSearch_LotQty, dtResult);

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_QAREQ_HIS_PACK", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PalletID"></param>
        /// <returns></returns>
        private string Check2QAREQ(string sPalletID)
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QAREQ_CHECK_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC(dtResult.Rows[0]["JUDGE"]);
                }
                else
                {
                    return "GOOD";
                }


            }
            catch (Exception ex)
            {
                //return "NG";
                //Util.AlertByBiz("DA_PRD_SEL_QAREQ_CHECK_CP", ex.Message, ex.ToString());
                Util.MessageException(ex);
                return "NG";
            }
        }

        /// <summary>
        /// PalletID�� Tray ���� ��ȸ�ϴ� �Լ�
        /// </summary>
        /// <param name="palletID">Pallet �������忡�� ������ ���� LotID</param>
        private void SelectTrayInfo(string sPalletID)
        {
            //DA_PRD_SEL_OQC_PLT_CP
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = null;
                dr["TO_DTTM"] = null;
                dr["AREAID"] = null;
                dr["EQSGID"] = null;
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_PLT_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    if (dgTray.GetRowCount() == 0)
                    {
                        //dgTray.ItemsSource = DataTableConverter.Convert(dtResult);
                        Util.GridSetData(dgTray, dtResult, FrameOperation, true);
                        setComboBox_SHIPTO(Util.NVC(dtResult.Rows[0]["AREAID"]));
                    }
                    else
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            //MERGE
                            DataTable dtBefore = DataTableConverter.Convert(dgTray.ItemsSource);
                            dtBefore.Merge(dtResult);
                            //dgTray.ItemsSource = DataTableConverter.Convert(dtBefore);
                            Util.GridSetData(dgTray, dtBefore, FrameOperation, true);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// ���ϰ˻��û
        /// </summary>
        private void QARequestSend()
        {
            try
            {
                DataTable dtQaTagetData = DataTableConverter.Convert(dgTray.ItemsSource);

                string shipdate = dtpShipDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                //string modelid = Util.NVC(dgTray.GetCell(0, dgTray.Columns["MODELID"].Index).Value);                
                //string Lineid = Util.NVC(dgTray.GetCell(0, dgTray.Columns["EQSGID"].Index).Value);

                string modelid = Util.NVC(dtQaTagetData.Rows[0]["PROJECTNAME"]);
                string Lineid = Util.NVC(dtQaTagetData.Rows[0]["EQSGID"]);

                //���ϰ˻� �Ƿ� �̷� ����
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SHIPDATE", typeof(DateTime));
                inDataTable.Columns.Add("ISS_SCHD_DATE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("SHIPTO_ID", typeof(string));
                inDataTable.Columns.Add("SHIPTO_NAME", typeof(string));
                inDataTable.Columns.Add("BOXS", typeof(string));

                DataTable inLotTable = indataSet.Tables.Add("INLOT");
                inLotTable.Columns.Add("BOXID", typeof(string));
                inLotTable.Columns.Add("TOTAL_QTY", typeof(decimal));

                int iTotalQty = 0;
                /*
                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    iTotalQty = iTotalQty + Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["LOT_COUNT"].Index).Value);

                    DataRow inLot = inLotTable.NewRow();
                    inLot["BOXID"] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["BOXID"].Index).Value);
                    inLot["TOTAL_QTY"] = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["LOT_COUNT"].Index).Value);
                    inLotTable.Rows.Add(inLot);
                }
                */
                for (int i = 0; i < dtQaTagetData.Rows.Count; i++)
                {
                    iTotalQty = iTotalQty + Util.NVC_Int(dtQaTagetData.Rows[i]["LOT_COUNT"]);

                    DataRow inLot = inLotTable.NewRow();
                    inLot["BOXID"] = Util.NVC(dtQaTagetData.Rows[i]["BOXID"]);
                    inLot["TOTAL_QTY"] = Util.NVC_Int(dtQaTagetData.Rows[i]["LOT_COUNT"]);
                    inLotTable.Rows.Add(inLot);
                }



                DataRow inData = inDataTable.NewRow();
                inData["SHIPDATE"] = Convert.ToDateTime(shipdate);
                inData["ISS_SCHD_DATE"] = dtpShipDate.SelectedDateTime.ToString("yyyyMMdd");
                inData["USERID"] = LoginInfo.USERID;//Util.NVC(cboUser.Text);
                inData["TOTAL_QTY"] = iTotalQty;
                inData["NOTE"] = "";
                inData["SHIPTO_ID"] = cboShipTo.SelectedValue;
                inData["SHIPTO_NAME"] = cboShipTo.Text;

                string boxid_temp = string.Empty;
                /*
                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    if(i != (dgTray.GetRowCount() -1))
                    {
                        boxid_temp += Util.NVC(dgTray.GetCell(i, dgTray.Columns["BOXID"].Index).Value) + ",";
                    }
                    else
                    {
                        boxid_temp += Util.NVC(dgTray.GetCell(i, dgTray.Columns["BOXID"].Index).Value) ;
                    }
                }
                */
                for (int i = 0; i < dtQaTagetData.Rows.Count; i++)
                {
                    if (i != (dtQaTagetData.Rows.Count - 1))
                    {
                        boxid_temp += Util.NVC(dtQaTagetData.Rows[i]["BOXID"]) + ",";
                    }
                    else
                    {
                        boxid_temp += Util.NVC(dtQaTagetData.Rows[i]["BOXID"]);
                    }
                }

                inData["BOXS"] = boxid_temp;
                inDataTable.Rows.Add(inData);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REQUEST_OQC_PACK", "INDATA,INLOT", null, indataSet);

                ms.AlertInfo("SFU1770"); //�̷� ���� �Ϸ�.

                Util.gridClear(dgTray);
                //�ϴ� text�ڽ� count ��
                DataTable dtResult = DataTableConverter.Convert(dgTray.ItemsSource);
                SetText_SearchResultCount(txtSelPalletQty, txtSelBoxQty, txtSelLotQty, dtResult);

                cboShipTo.ItemsSource = null;

                GetPalletInfo();
            }
            catch (Exception ex)

            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void setTagReport(string sPalletID, string sEQSGID)
        {
            PackCommon.ShowPalletTag(this.GetType().Name, sPalletID, sEQSGID, string.Empty);
        }

        private void printPopUp_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.PACK001.Pallet_Tag printPopUp = sender as LGC.GMES.MES.PACK001.Pallet_Tag;
                if (Convert.ToBoolean(printPopUp.DialogResult))
                {

                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        //PALLET ID�� �̷���ȸ
        private void selectQAHistInfo_BY_PALLETID(string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROMDTTM", typeof(string));
                RQSTDT.Columns.Add("TODTTM", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODELID", typeof(string));
                RQSTDT.Columns.Add("JUDG_VALUE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDTTM"] = null;
                dr["TODTTM"] = null;
                dr["EQSGID"] = null;
                dr["MODELID"] = null;
                dr["JUDG_VALUE"] = null;
                dr["PALLETID"] = sPalletID;
                dr["AREAID"] = null;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_QAREQ_HIS_PACK", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (dtResult.Rows.Count > 0)
                    {
                        if (dgQAReqHis.GetRowCount() == 0)
                        {
                            //dgQAReqHis.ItemsSource = DataTableConverter.Convert(dtResult);
                            Util.GridSetData(dgQAReqHis, dtResult, FrameOperation, true);
                            // text�ڽ� count ��
                            SetText_SearchResultCount(txtSearch_PalletQty, txtSearch_BoxQty, txtSearch_LotQty, dtResult);
                        }
                        else
                        {
                            if (dtResult.Rows.Count > 0)
                            {
                                //MERGE
                                DataTable dtBefore = DataTableConverter.Convert(dgQAReqHis.ItemsSource);
                                dtBefore.Merge(dtResult);
                                //dgQAReqHis.ItemsSource = DataTableConverter.Convert(dtBefore);
                                Util.GridSetData(dgQAReqHis, dtBefore, FrameOperation, true);
                                // text�ڽ� count ��
                                SetText_SearchResultCount(txtSearch_PalletQty, txtSearch_BoxQty, txtSearch_LotQty, dtBefore);
                            }
                        }
                    }
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_QAREQ_HIS_PACK", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        //PALLET ID�� ��ȸ �� �̷��� ���� ��� ����
        private void selectQAHistInfo_Made_PALLETID(string sPalletID)
        {

            try
            {
                //BizData data = new BizData("QR_QA_PALLETID", "RSLTDT");

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_MADE_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    if (dgQAReqHis.GetRowCount() == 0)
                    {
                        //dgQAReqHis.ItemsSource = DataTableConverter.Convert(dtResult);
                        Util.GridSetData(dgQAReqHis, dtResult, FrameOperation, true);
                    }
                    else
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            //�������� �ο� �� üũ(���̺� ���� ������)
                            DataTable DT = DataTableConverter.Convert(dgQAReqHis.ItemsSource);

                            DataRow drGet = dtResult.Rows[0];
                            DataRow newDr = DT.NewRow();
                            foreach (DataColumn col in dtResult.Columns)
                            {
                                newDr[col.ColumnName] = drGet[col.ColumnName];
                            }
                            DT.Rows.Add(newDr);
                            //dgQAReqHis.ItemsSource = DataTableConverter.Convert(DT);
                            Util.GridSetData(dgQAReqHis, DT, FrameOperation, true);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
                return;
            }

        }

        private void SetText_SearchResultCount(C1NumericBox txtPalletCount, C1NumericBox txtBoxCount, C1NumericBox txtLotCount, DataTable dtResult)
        {
            try
            {
                int lSumPallet = 0;
                int lSumBox = 0;
                int lSumLot = 0;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    object oPalletQty = dtResult.Compute("Count(BOXID)", "");
                    object oLotQty = dtResult.Compute("Sum(LOT_COUNT)", "");
                    object oBoxQty = dtResult.Compute("Sum(BOX_COUNT)", "");

                    int.TryParse(oPalletQty.ToString(), out lSumPallet);
                    int.TryParse(oBoxQty.ToString(), out lSumBox);
                    int.TryParse(oLotQty.ToString(), out lSumLot);
                }

                txtPalletCount.Value = lSumPallet;
                txtBoxCount.Value = lSumBox;
                txtLotCount.Value = lSumLot;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void gridLoadCellEventFunction(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "BOXID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// �����˾�����
        /// </summary>
        /// <param name="cell">DataGridCell</param>
        /// <param name="sPopUp_Flag">INFO_BOX: �ڽ����� �˾� , INFO_OQC : OQC�����ȸ �˾� ����</param>
        private void BoxInfoPopUp_Open(C1.WPF.DataGrid.DataGridCell cell, string sPopUp_Flag)
        {
            try
            {

                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "BOXID")
                        {

                            if (sPopUp_Flag == "INFO_BOX")
                            {
                                PACK001_003_BOXINFO popup = new PACK001_003_BOXINFO();
                                popup.FrameOperation = this.FrameOperation;

                                if (popup != null)
                                {
                                    DataTable dtData = new DataTable();
                                    dtData.Columns.Add("BOXID", typeof(string));

                                    DataRow newRow = null;
                                    newRow = dtData.NewRow();
                                    newRow["BOXID"] = cell.Text;

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
                            else if (sPopUp_Flag == "INFO_OQC")
                            {
                                PACK001_026_OQC_INFO popup = new PACK001_026_OQC_INFO();
                                popup.FrameOperation = this.FrameOperation;

                                if (popup != null)
                                {
                                    DataTable dtData = new DataTable();
                                    dtData.Columns.Add("BOXID", typeof(string));
                                    //20170324 ==============================================
                                    dtData.Columns.Add("OQC_INSP_REQ_ID", typeof(string));
                                    //20170324 ==============================================

                                    //20170324 ==============================================
                                    string strOIRID = Util.NVC(DataTableConverter.GetValue(dgQAReqHis.Rows[cell.Row.Index].DataItem, "OQC_INSP_REQ_ID"));
                                    //20170324 ==============================================

                                    DataRow newRow = null;
                                    newRow = dtData.NewRow();
                                    newRow["BOXID"] = cell.Text;
                                    //20170324 ==============================================
                                    newRow["OQC_INSP_REQ_ID"] = strOIRID;
                                    //20170324 ==============================================

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
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_SHIPTO(string sAREAID)
        {
            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SHIP_TYPE_CODE", typeof(string));
                dtIndata.Columns.Add("FROM_AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["SHIP_TYPE_CODE"] = Ship_Type.PACK;
                drIndata["FROM_AREAID"] = sAREAID;
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_SHIPTO_BY_FROMAREAID_CBO", "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_SHIPTO_BY_FROMAREAID_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    string sShipSelected = Util.NVC(cboShipTo.SelectedValue);

                    cboShipTo.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        if (sShipSelected.Length > 0)
                        {
                            cboShipTo.SelectedValue = sShipSelected;
                        }
                        else
                        {
                            cboShipTo.SelectedIndex = 0;
                        }
                    }
                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


    }
}
