/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.03.13  LEEHJ     SI          ����Ȱ��ȭ MES����
  2023.12.01  ������    SI          NFF Ȱ��ȭ MES ���� - Cell �߰�/���� ��� ����, Cell ��ü ��� ����
  2024.01.11  ��ȫ��    SI          NFF Ȱ��ȭ MES ���� - INBOX �ű� ������ EIF�� ���� BIZ�� ����ϰ� ����, ����� ã�ƿͼ� BR_PRD_REG_INBOX_TESLA_NJ_MB ȣ��
  2024.03.20  ��ȫ��    SI          NFF Ȱ��ȭ MES ���� - Cell �߰�/����� Cell, Box�� ���� Pallet�� ������� Ȯ�� GetPackedStat �߰�
  2024.04.17  ��ȫ��    SI          NFF Ȱ��ȭ MES ���� - NFF �ű� INBOX������ �ڸ��� Ȯ��(21,22)
  2024.05.15  ��ȫ��    SI          NFF Ȱ��ȭ MES ���� - OUTBOX ������ INBOX ���������� ����
  2024.07.19  ��ȫ��    SI          NFF Ȱ��ȭ MES ���� - CELL ��ü(����) �̷� �� �߰�
  2025.05.30  �ڳ���                �߼��� MES ������
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.UserControls;
using System.Text.RegularExpressions;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_304 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string _BoxType = string.Empty;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        /*��Ʈ�� ���� ����*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker { get; set; }
        public TextBox txtShift { get; set; }
        public bool IsReadOnly { get; }

        public FCS002_304()
        {
            InitializeComponent();
            Loaded += FCS002_304_Loaded;
        }

        CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();
        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        string Geqsgid = string.Empty;
        string GMoDelLot = string.Empty;
        /// <summary>
        /// Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        List<string> BCDType = new List<string>() { "1D���ڵ�", "2D���ڵ�" };

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void FCS002_304_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FCS002_304_Loaded;

            ApplyPermissions();
            initCombo();
            InitSet();
            SetEvent();

        }

        /// <summary>
        /// ȭ�鳻 ��ư ���� ó��
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnChange1);
            listAuth.Add(btnChange2);
            listAuth.Add(btnChange3);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Initialize

        private void InitSet()
        {

            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
            // Area ����
            ComCombo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AREA_CP");
        }

        private void initCombo()
        {
            ComCombo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "AREA_CP");
        }

        private void SetLine()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MB", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";
                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

        }

        #endregion

        #region Event

        /// <summary>
        /// ��ü �̷� ��ȸ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // ��ȸ �� ���� ������ �ʱ�ȭ
            dgCellChangeHis.ItemsSource = null;

            string from = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
            string to = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("TO_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("CELLID", typeof(string));
                RQSTDT.Columns.Add("INBOXID", typeof(string));  //INBOXID
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if ((!string.IsNullOrWhiteSpace(txtCellid.Text)) || (!string.IsNullOrWhiteSpace(txtInBoxId.Text)))
                {
                    if (!string.IsNullOrWhiteSpace(txtCellid.Text)) dr["CELLID"] = txtCellid.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(txtInBoxId.Text)) dr["INBOXID"] = txtInBoxId.Text.Trim();
                }
                //else
                {
                    dr["FROM_DTTM"] = from;
                    dr["TO_DTTM"] = to;
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                    dr["MDLOT_ID"] = Util.NVC(cboModelLot.SelectedValue) == "" ? null : Util.NVC(cboModelLot.SelectedValue);
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_REPLACE_SUBLOT_HIST_MB", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgCellChangeHis, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }

            String[] sFilter = { sAREAID };    // Area
            ComCombo.SetCombo(cboEquipmentSegment, CommonCombo_Form_MB.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE"); //, sCase: "EQSGID_PACK");

            SetModelLot2(sAREAID);

        }


        private void txtTargetCellID1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    TextBox txtCellid = sender as TextBox;

                    string sCellID = txtCellid.Text.ToString().Replace(Char.ConvertFromUtf32(29), string.Empty).Replace(Char.ConvertFromUtf32(30), string.Empty).Trim();

                    if (sCellID == null)
                    {
                        //��� Cell ID �� �����ϴ�. >> ��� Cell ID�� �Էµ��� �ʾҽ��ϴ�.
                        Util.MessageValidation("SFU1495", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                        });
                        return;
                    }

                    bool b2DBCD = sCellID.Length > 60;

                    DataTable SearchResult = GetCellInfo(sCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //��ȸ�� Data�� �����ϴ�.
                        Util.MessageValidation("SFU1905", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                        });
                        return;
                    }
                    else
                    {

                        try
                        {
                            //CELL ���� BOX�� PALLET�� ���� �������� Ȯ�� 2024.03.20
                            if (GetPackedStat(sCellID) != 0)
                            {

                                //Cell[% 1]�� �̹� ����Ǿ����ϴ�.1123
                                Util.MessageValidation("1123", sCellID);
                                return;

                            }
                        }
                        catch (Exception ex)
                        {
                            return;
                        }
                    }





                    DataTable dtTarget = DataTableConverter.Convert(dgTargetCell1.ItemsSource);

                    dtTarget.Merge(SearchResult);

                    Util.GridSetData(dgTargetCell1, dtTarget, FrameOperation);

                    txtCellid.Text = sCellID;

                    txtSourceCellID1.Focus();
                    txtSourceCellID1.SelectAll();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                }
            }
        }

        private void txtSourceCellID1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (dgTargetCell1.GetRowCount() == 0)
                    {
                        //��� Cell ID�� �Էµ��� �ʾҽ��ϴ�.
                        Util.MessageValidation("SFU1495");
                        return;
                    }

                    string sCellID = string.Empty;
                    sCellID = txtSourceCellID1.Text.Replace(Char.ConvertFromUtf32(29), string.Empty).Replace(Char.ConvertFromUtf32(30), string.Empty).Trim();

                    if (dgSourceCell1.GetRowCount() > 0)
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgSourceCell1.ItemsSource);
                        DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sCellID + "'");

                        if (drList.Length > 0)
                        {
                            // SFU3159 �Ʒ��� List�� �̹� �����ϴ� CELL ID�Դϴ�.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtSourceCellID1.Focus();
                                    txtSourceCellID1.Text = string.Empty;
                                }
                            });

                            txtSourceCellID1.Text = string.Empty;
                            return;
                        }
                    }

                    if (sCellID == null)
                    {
                        //��ü Cell ID �� �����ϴ�.
                        Util.MessageValidation("SFU1462");
                        return;
                    }

                    // ��ü�� ����� �����Ұ��
                    // 90118  �Էµ� CELL�� ������ CELL�� 2���̻� �����մϴ�.
                    if (txtSourceCellID1.Text.Trim() == txtTargetCellID1.Text.Trim())
                    {
                        Util.MessageValidation("90118");
                        return;
                    }


                    try
                    {
                        //CELL ���� BOX�� PALLET�� ���� �������� Ȯ�� 2024.01.30
                        if (GetPackedStat(sCellID) != 0)
                        {

                            //Cell[% 1]�� �̹� ����Ǿ����ϴ�.1123
                            Util.MessageValidation("1123", sCellID);
                            return;

                        }
                    }
                    catch (Exception ex)
                    {
                        return;
                    }

                    string pack = string.Empty;

                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("SUBLOTID");
                    RQSTDT.Columns.Add("FORM_PSTN_NO");


                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    dr["FORM_PSTN_NO"] = string.Empty;

                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_FOR_ADD_BOX_MB", "INDATA", "OUTDATA", RQSTDT);

                    DataTable dtSource = DataTableConverter.Convert(dgSourceCell1.ItemsSource);

                    dtSource.Merge(dtRslt);

                    Util.GridSetData(dgSourceCell1, dtSource, FrameOperation);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                }
            }
        }
        //��ü
        private void btnChange1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTargetCell1.GetRowCount() == 0 || dgSourceCell1.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"�Է� �����Ͱ� �������� �ʽ��ϴ�."              
                    return;
                }

                if (txtReason1.Text == string.Empty)
                {
                    txtReason1.Focus();
                    Util.MessageValidation("SFU1252"); //"��ü ������ �ʼ� �Դϴ�."
                    return;
                }

                if (dgTargetCell1.GetRowCount() <= 0)
                {

                    //SFU1462 ��ü ��� Cell ID�� �Էµ��� �ʾҽ��ϴ�.
                    Util.MessageValidation("SFU1462", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTargetCellID1.Focus();
                            txtTargetCellID1.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dgTargetCell1.GetRowCount() < dgSourceCell1.GetRowCount())
                {

                    //SFU1462 ��ü ��� Cell ID�� �Էµ��� �ʾҽ��ϴ�.
                    Util.MessageValidation("SFU1462", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTargetCellID1.Focus();
                            txtTargetCellID1.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dgTargetCell1.GetRowCount() > dgSourceCell1.GetRowCount())
                {
                    //CELLID�� ��ĵ �Ǵ� �Է��ϼ���.

                    Util.MessageValidation("SFU1462", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtSourceCellID1.Focus();
                            txtSourceCellID1.Text = string.Empty;
                        }
                    });
                    return;
                }

                //SFU1465	��üó�� �Ͻðڽ��ϱ�?	
                Util.MessageConfirm("SFU1465", (result) =>
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgTargetCell1.ItemsSource);
                        DataTable dtTarget = DataTableConverter.Convert(dgSourceCell1.ItemsSource);

                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));

                        DataRow newRow = inDataTable.NewRow();
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                        inDataTable.Rows.Add(newRow);

                        DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
                        inSublotTable.Columns.Add("SUBLOTID");

                        for (int i = 0; i < dgSourceCell1.GetRowCount(); i++)
                        {
                            DataRow newRow1 = inSublotTable.NewRow();
                            newRow1["SUBLOTID"] = Util.NVC(dgSourceCell1.GetCell(i, dgSourceCell1.Columns["SUBLOTID"].Index).Value);

                            inSublotTable.Rows.Add(newRow1);
                        }

                        DataTable inSublotDelTable = indataSet.Tables.Add("INSUBLOT_DELETE");
                        inSublotDelTable.Columns.Add("SUBLOTID");
                        inSublotDelTable.Columns.Add("BOXID");
                        inSublotDelTable.Columns.Add("FORM_TRAY_PSTN_NO");

                        for (int i = 0; i < dgTargetCell1.GetRowCount(); i++)
                        {
                            DataRow newRow2 = inSublotDelTable.NewRow();
                            newRow2["SUBLOTID"] = Util.NVC(dgTargetCell1.GetCell(i, dgTargetCell1.Columns["SUBLOTID"].Index).Value);
                            newRow2["BOXID"] = Util.NVC(dgTargetCell1.GetCell(i, dgTargetCell1.Columns["INBOX"].Index).Value);
                            newRow2["FORM_TRAY_PSTN_NO"] = Util.NVC(dgTargetCell1.GetCell(i, dgTargetCell1.Columns["FORM_TRAY_PSTN_NO"].Index).Value);
                            inSublotDelTable.Rows.Add(newRow2);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SUBLOT_REPLACE_MB", "INDATA,INSUBLOT,INSUBLOT_DELETE", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //����ó���Ǿ����ϴ�.
                                Util.MessageInfo("SFU1275");

                                AllClear1();
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, indataSet);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnInit2_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 �ʱ�ȭ �Ͻðڽ��ϱ�?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear2(true);
                }

            });
        }
        //�߰� ADD
        private void btnChange2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(bool)chkNewBox.IsChecked)
                {
                    if (dgPallet2.GetRowCount() == 0)
                    {
                        Util.MessageValidation("SFU1801"); //"�Է� �����Ͱ� �������� �ʽ��ϴ�."
                        return;
                    }
                }
                else
                {
                    if (txtNewBoxID.IsReadOnly == false)
                    {
                        //�Է¿��� : Boxid TextBox���� ���͸� ġ����
                        Util.MessageValidation("SFU3455");
                        return;
                    }
                }

                if (dgSourceCell2.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"�Է� �����Ͱ� �������� �ʽ��ϴ�."
                    return;
                }

                if (txtReason2.Text == string.Empty)
                {
                    txtReason2.Focus();
                    Util.MessageValidation("SFU1252"); //"��ü ������ �ʼ� �Դϴ�."
                    return;
                }

                //��� �����Ͻðڽ��ϱ�? >> �۾��� �����Ͻðڽ��ϱ�?
                Util.MessageConfirm("SFU1170", (msgresult) =>
                {
                    if (msgresult == MessageBoxResult.OK)
                    {
                        if ((bool)chkNewBox.IsChecked)
                        {
                            CreateInbox();
                        }
                        else
                        {
                            AddCell();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInit3_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 �ʱ�ȭ �Ͻðڽ��ϱ�?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear3(true);
                }

            });
        }
        //���� : REMOVE
        private void btnChange3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0 || dgSourceCell3.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"�Է� �����Ͱ� �������� �ʽ��ϴ�."
                    return;
                }

                if (txtReason3.Text == string.Empty)
                {
                    txtReason3.Focus();
                    Util.MessageValidation("SFU1252"); //"��ü ������ �ʼ� �Դϴ�."
                    return;
                }

                //��� �����Ͻðڽ��ϱ�? >> �۾��� �����Ͻðڽ��ϱ�?
                Util.MessageConfirm("SFU1170", (msgresult) =>
                {
                    if (msgresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                            string sPalletID = string.Empty;

                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));

                            inSublotTable.Columns.Add("SUBLOTID", typeof(string));

                            DataRow newRow = inDataTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID"));

                            inDataTable.Rows.Add(newRow);

                            string sublot = string.Empty;

                            for (int i = 0; i < dgSourceCell3.GetRowCount(); i++)
                            {
                                sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell3.Rows[i].DataItem, "SUBLOTID")).Trim();

                                if (string.IsNullOrWhiteSpace(sublot))
                                {
                                    Util.MessageInfo("SFU1495"); //"��� Cell ID�� �Էµ��� �ʾҽ��ϴ�."
                                    return;
                                }

                                DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                                drsub["SUBLOTID"] = sublot;

                                indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REMOVE_SUBLOT_MB", "INDATA,INSUBLOT", string.Empty, indataSet);

                            // "���������� ó���Ͽ����ϴ�." >>����ó���Ǿ����ϴ�.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID3.Focus();
                                    txtPalletID3.SelectAll();
                                }
                            });

                            AllClear3(true);

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sPalletID = string.Empty;

                    sPalletID = txtPalletID3.Text.ToString().Trim();

                    if (sPalletID == null)
                    {
                        //Pallet ID �� �����ϴ�. >> PALLETID�� �Է����ּ���
                        Util.MessageValidation("SFU1411");
                        return;
                    }

                    GetPalletInfo(dgPallet3, sPalletID);

                    ClearDataGrid(dgSourceCell3);
                    txtSourceCellID3.Focus();
                    txtSourceCellID3.SelectAll();
                    txtSourceCellID3.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private bool SetSourceCellID3(string sSourceCellId)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0)
                {
                    //BoxId ���� �Է��ϼ���
                    Util.MessageValidation("SFU3387");
                    return false;
                }

                if (string.IsNullOrEmpty(sSourceCellId))
                {
                    //��ü Cell ID �� �����ϴ�.
                    Util.MessageValidation("SFU1462");
                    return false;
                }


                try
                {
                    //CELL ���� BOX�� PALLET�� ���� �������� Ȯ�� 2024.01.30
                    if (GetPackedStat(sSourceCellId) != 0)
                    {

                        //Cell[% 1]�� �̹� ����Ǿ����ϴ�.1123
                        Util.MessageValidation("1123", sSourceCellId);
                        return false;

                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell3.ItemsSource);

                if (dtInfo.Rows.Count > 0)
                {
                    DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sSourceCellId + "'");

                    if (drList.Length > 0)
                    {
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSourceCellID3.Focus();
                                txtSourceCellID3.Text = string.Empty;
                            }
                        });

                        txtSourceCellID3.Text = string.Empty;
                        return false;
                    }
                }

                DataSet ds = new DataSet();

                DataTable RQSTDT = ds.Tables.Add("INDATA");
                RQSTDT.Columns.Add("BOXID");

                DataTable INSUBLOT = ds.Tables.Add("INSUBLOT");
                INSUBLOT.Columns.Add("SUBLOTID");

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID"));

                DataRow dr2 = INSUBLOT.NewRow();
                dr2["SUBLOTID"] = sSourceCellId;

                RQSTDT.Rows.Add(dr);
                INSUBLOT.Rows.Add(dr2);

                //ell ���� ���� ���� Check (Tesla ��)
                DataSet dtRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_CHK_UNPACK_SUBLOT_MB", "INDATA,INSUBLOT", "OUTDATA", ds);
                DataTable dtRsltTable = dtRslt.Tables["OUTDATA"];

                dtInfo.Merge(dtRsltTable);

                Util.GridSetData(dgSourceCell3, dtInfo, FrameOperation, true);

                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                txtSourceCellID3.Focus();
                txtSourceCellID3.SelectAll();
            }
        }

        private void txtSourceCellID3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetSourceCellID3(txtSourceCellID3.Text.Trim());
            }
        }

        private void txtPalletID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {

                    string sPalletID = txtPalletID2.Text.Trim();

                    //NFF����
                    if (string.IsNullOrEmpty(sPalletID))
                    {
                        //��ȸ�� BOX ID �� �Է��ϼ���.
                        Util.MessageValidation("SFU1189");
                        return;
                    }

                    if (GetPalletInfo(dgPallet2, sPalletID))
                    {
                        txtSourceCellID2.Focus();
                        txtSourceCellID2.SelectAll();
                    }
                    else
                    {
                        txtPalletID2.SelectAll();
                    }

                    ClearDataGrid(dgSourceCell2);


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private bool SetSourceCellID2(string sSouceCellId, string sPstnNo)
        {
            try
            {
                if (dgPallet2.GetRowCount() == 0 && chkNewBox.IsChecked == false)
                {
                    //BoxId ���� �Է��ϼ���
                    Util.MessageValidation("SFU3387");
                    return false;
                }

                if (sSouceCellId == null)
                {
                    //��ü Cell ID �� �����ϴ�.
                    Util.MessageValidation("SFU1462");
                    return false;
                }

                try
                {
                    //CELL ���� BOX�� PALLET�� ���� �������� Ȯ�� 2024.01.30
                    if (GetPackedStat(sSouceCellId) != 0)
                    {
                        //������¶� ���� �Ұ�
                        //Cell[% 1]�� �̹� ����Ǿ����ϴ�.1123
                        Util.MessageValidation("1123", sSouceCellId);
                        return false;

                    }
                }
                catch (Exception ex)
                {
                    return false;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell2.ItemsSource);

                if (dtInfo.Rows.Count > 0)
                {
                    DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sSouceCellId + "'");

                    if (drList.Length > 0)
                    {
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSourceCellID2.Focus();
                                txtSourceCellID2.Text = string.Empty;
                            }
                        });

                        txtSourceCellID2.Text = string.Empty;
                        return false;
                    }
                }

                string sBizName = "BR_PRD_CHK_SUBLOT_FOR_ADD_BOX_MB";


                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("FORM_PSTN_NO");

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sSouceCellId;
                dr["FORM_PSTN_NO"] = sPstnNo;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", RQSTDT);

                dtInfo.Merge(dtRslt);

                Util.GridSetData(dgSourceCell2, dtInfo, FrameOperation, true);


                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                txtSourceCellID2.Focus();
                txtSourceCellID2.SelectAll();
            }
        }

        private void txtSourceCellID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetSourceCellID2(txtSourceCellID2.Text.Trim(), string.Empty);
            }
        }

        private void btnInit1_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 �ʱ�ȭ �Ͻðڽ��ϱ�?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear1(true);
                }

            });
        }


        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            //�����Ͻðڽ��ϱ�?
            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgSourceCell2.IsReadOnly = false;
                    dgSourceCell2.RemoveRow(index);


                    for (int cnt = 0; cnt < dgSourceCell2.GetRowCount(); cnt++)
                    {
                        DataTableConverter.SetValue(dgSourceCell2.Rows[cnt].DataItem, "SEQ", cnt + 1);
                    }

                    dgSourceCell2.IsReadOnly = true;
                }
            });
        }

        private void txtSourceCellID2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && SetSourceCellID2(sPasteStrings[i].Trim(), string.Empty) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtSourceCellID3_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && SetSourceCellID3(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }


        #region Cell �߰� ���� ���ε�
        /// <summary>
        /// ���� ���ε� ��� �ٿ�ε�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Add_Cell_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "CELLID";
                    sheet[1, 0].Value = "CELLID_001";
                    sheet[2, 0].Value = "CELLID_002";

                    sheet[0, 1].Value = "POSITION";
                    sheet[1, 1].Value = "A01";
                    sheet[2, 1].Value = "A02";

                    sheet[0, 0].Style = sheet[0, 1].Style = styel;
                    sheet.Columns[0].Width = sheet.Columns[1].Width = 1500;

                    c1XLBook1.Save(od.FileName);

                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// ���� ���ε�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)chkNewBox.IsChecked)
                {
                    if (string.IsNullOrWhiteSpace(txtNewBoxID.Text.Trim()) && txtNewBoxID.IsReadOnly == true)
                    {
                        //BoxId ���� �Է��ϼ���
                        Util.MessageValidation("SFU3387");
                        return;
                    }
                }
                else
                {
                    if (dgPallet2.GetRowCount() == 0)
                    {
                        //BoxId ���� �Է��ϼ���
                        Util.MessageValidation("SFU3387");
                        return;
                    }
                }


                if (dgSourceCell2.ItemsSource == null)
                {
                    InitCellList(dgSourceCell2);
                }
                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell2.ItemsSource);

                dtInfo.Clear();


                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        if (sheet.GetCell(0, 0).Text != "CELLID"
                               || sheet.GetCell(0, 1).Text != "POSITION")
                        {
                            //SFU4424	���Ŀ� �´� EXCEL������ ������ �ּ���.
                            Util.MessageValidation("SFU4424");
                            return;
                        }

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // CELLID, POSITION ���Է½� return;
                            if (sheet.GetCell(rowInx, 0) == null
                                || sheet.GetCell(rowInx, 1) == null)
                                return;

                            string inputCellID = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            string inputPstnNO = sheet.GetCell(rowInx, 1).Text.Trim();
                            SetSourceCellID2(inputCellID, inputPstnNO);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void txtinbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                if (!string.IsNullOrWhiteSpace(txtinbox.Text.Trim()))
                {
                    try
                    {

                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(txtoutbox.Text.Trim()))
                {
                    txtoutbox.Focus();
                    txtoutbox.SelectAll();
                }
                else if (string.IsNullOrWhiteSpace(txtinbox.Text.Trim()))
                {
                    txtinbox.Focus();
                    txtinbox.SelectAll();
                }
                else if (!string.IsNullOrWhiteSpace(txtoutbox.Text.Trim()) && !string.IsNullOrWhiteSpace(txtinbox.Text.Trim()))
                {
                    CreateOutBox();
                }
            }
        }

        private void txtoutbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtinbox.Text.Trim()))
                {
                    txtinbox.Focus();
                    txtinbox.SelectAll();
                }
                else if (string.IsNullOrWhiteSpace(txtoutbox.Text.Trim()))
                {
                    txtoutbox.Focus();
                    txtoutbox.SelectAll();
                }
                else if (!string.IsNullOrWhiteSpace(txtoutbox.Text.Trim()) && !string.IsNullOrWhiteSpace(txtinbox.Text.Trim()))
                {
                    CreateOutBox();
                }
            }
        }

        private void btnoutboxrefresh_Click(object sender, RoutedEventArgs e)
        {
            txtoutbox.Clear();
            txtinbox.Clear();

            txtinbox.Focus();
            txtinbox.SelectAll();
        }

        private void btncreoutbox_Click(object sender, RoutedEventArgs e)
        {
            CreateOutBox();
        }


        #endregion

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility;
        }

        private void chkNewBox_Checked(object sender, RoutedEventArgs e)
        {
            if (dgPallet2 != null && dgPallet2.GetRowCount() > 0)
            {
                // SFU3281 �۾����� : �������� �۾��� �ֽ��ϴ�. [���� �����۾� �Ϸ� �ϰų� �׳� �ʱ�ȭ]
                Util.MessageValidation("SFU3281");
                chkNewBox.IsChecked = false;
                return;
            }

            txtPalletID2.IsReadOnly = true;
            txtNewBox.Visibility = Visibility.Visible;
            txtNewBoxID.Visibility = Visibility.Visible;
        }

        private void chkNewBox_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPalletID2.IsReadOnly = false;
            txtNewBoxID.IsReadOnly = false;
            txtNewBoxID.Text = string.Empty;

            txtNewBox.Visibility = Visibility.Collapsed;
            txtNewBoxID.Visibility = Visibility.Collapsed;

        }

        private void txtNewBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {

                    string NewBox = txtNewBoxID.Text.Trim();

                    if (string.IsNullOrWhiteSpace(NewBox))
                    {
                        // Inbox ID�� �Է� �ϼ���.
                        Util.MessageInfo("SFU4517");
                        txtNewBoxID.Focus();
                        txtNewBoxID.SelectAll();
                        return;
                    }

                    // 2023.11.08 NFF�� BoxID Rule �ٸ�
                    //OC2 NFF  INBOX 22�ڸ� (���ⱸ���� �� Ư������ ����)
                    _BoxType = "N";

                    //�Ʒ� BR_PRD_CHK_DUP_BOX_MB BIZ���� ���ⱸ���ڸ� ���� �ϱ⶧���� ���ⱸ���ڰ� ���� ��� ������ �߰���.
                    if (NewBox.Length == 21)
                    {
                        NewBox = NewBox + "A";
                    }//INBOX ID�� 21,22�ڸ��� �ƴϸ�
                    else if (NewBox.Length > 22 || NewBox.Length < 21)
                    {
                        //BOX ID�� �������� �ʴ� �׸��� �ֽ��ϴ�. �ٽ� Ȯ�����ּ���.
                        Util.MessageInfo("FM_ME_0508");
                        return;
                    }

                    if (_BoxType == null)
                    {
                        // Inbox ID�� �Է� �ϼ���.
                        Util.MessageInfo("SFU4517");
                        txtNewBoxID.Focus();
                        txtNewBoxID.SelectAll();
                        return;
                    }

                    DataSet ds = new DataSet();

                    DataTable dtEqp = ds.Tables.Add("IN_EQP");
                    dtEqp.Columns.Add("SRCTYPE", typeof(String));
                    dtEqp.Columns.Add("IFMODE", typeof(String));
                    dtEqp.Columns.Add("EQPTID", typeof(String));
                    dtEqp.Columns.Add("USERID", typeof(String));


                    DataTable dtBox = ds.Tables.Add("IN_BOX");
                    dtBox.Columns.Add("BOXID", typeof(String));
                    dtBox.Columns.Add("BOX_TYPE", typeof(String));

                    DataRow dr = dtEqp.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = string.Empty;
                    dr["USERID"] = string.Empty;


                    dtEqp.Rows.Add(dr);

                    DataRow dr1 = dtBox.NewRow();
                    dr1["BOXID"] = Util.NVC(NewBox);
                    dr1["BOX_TYPE"] = Util.NVC(_BoxType);

                    dtBox.Rows.Add(dr1);
                    //Tesla 1ȸ�� Inbox �ߺ� �߹� ���� üũ
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_CHK_DUP_BOX_MB", "IN_EQP,IN_BOX", "OUTDATA", ds);

                    txtNewBoxID.IsReadOnly = true;


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkNewBox.IsChecked)
            {
                if (dgPallet2.GetRowCount() == 0)
                {
                    // Inbox ID�� �Է� �ϼ���.
                    Util.MessageInfo("SFU4517");
                    txtPalletID2.Focus();
                    return;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtNewBoxID.Text.Trim()))
                {
                    // Inbox ID�� �Է� �ϼ���.
                    Util.MessageInfo("SFU4517");
                    txtNewBoxID.Focus();
                    return;
                }
            }


            DataTable dtList = new DataTable();

            dtList.Columns.Add("SUBLOTID");
            dtList.Columns.Add("FORM_TRAY_PSTN_NO");

            dgSourceCell2.ItemsSource = DataTableConverter.Convert(dtList);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ClearDataGrid(dgSourceCell2);
        }

        #endregion

        #region Mehod

        #region  AREA, PROCID(B1000) �� �´� EQPTID ��������

        #endregion
        private void SetModelLot2(string pAREAID)
        {
            try
            {
                cboModelLot.ItemsSource = null;
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                ///dr["EQSGID"] = str;
                dr["AREAID"] = string.IsNullOrEmpty(pAREAID) ? LoginInfo.CFG_AREA_ID.ToString() : pAREAID.ToString();
                dr["MDLLOT_ID"] = "";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MDLLOT_MULTI_PJT_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow drnewrow = dtResult.NewRow();
                drnewrow["CBO_NAME"] = "-ALL-";
                drnewrow["CBO_CODE"] = DBNull.Value;
                dtResult.Rows.InsertAt(drnewrow, 0);

                cboModelLot.DisplayMemberPath = "CBO_NAME";
                cboModelLot.SelectedValuePath = "CBO_CODE";

                cboModelLot.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));

                cboModelLot.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
            }

        }


        private bool GetPalletInfo(C1.WPF.DataGrid.C1DataGrid grid, string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sPalletID;

                RQSTDT.Rows.Add(dr);
                //InBox ���� ��ȸ(���� ��ü Pallet ���� ��ȸ) 
                DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOX_INFO_FOR_UNPACK_MB", "INDATA", "OUTDATA", RQSTDT);

                if (dtPallet == null || dtPallet.Rows.Count == 0)
                {
                    //BOX ������ �����ϴ�.
                    Util.MessageInfo("SFU1180");
                    return false;
                }

                Util.GridSetData(grid, dtPallet, FrameOperation);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private DataTable GetCellInfo(string sCellID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SUBLOTID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["SUBLOTID"] = sCellID;

            RQSTDT.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("BR_PRD_GET_SUBLOT_FOR_REPLACE_MB", "INDATA", "OUTDATA", RQSTDT);
        }

        private void ClearDataGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.ItemsSource = null;
        }

        private void InitCellList(C1.WPF.DataGrid.C1DataGrid dg)
        {
            DataTable dtInit = new DataTable();

            dtInit.Columns.Add("SUBLOTID", typeof(string));
            dtInit.Columns.Add("FORM_TRAY_PSTN_NO", typeof(string));

            dg.ItemsSource = DataTableConverter.Convert(dtInit);
        }

        private void AllClear1(bool bAll = false)
        {
            if (bAll)
            {
                txtReason1.Text = string.Empty;
            }

            ClearDataGrid(dgTargetCell1);
            ClearDataGrid(dgSourceCell1);

            txtSourceCellID1.Text = string.Empty;
            txtTargetCellID1.Text = string.Empty;
        }

        private void AllClear2(bool bAll = false)
        {
            if (bAll)
            {
                txtReason2.Text = string.Empty;
            }
            ClearDataGrid(dgPallet2);
            ClearDataGrid(dgSourceCell2);

            txtPalletID2.Text = string.Empty;
            txtSourceCellID2.Text = string.Empty;

            chkNewBox.IsChecked = false;

            txtNewBoxID.Text = string.Empty;
            txtNewBoxID.IsReadOnly = false;
        }

        private void AllClear3(bool bAll = false)
        {
            if (bAll)
            {
                txtReason3.Text = string.Empty;
            }
            ClearDataGrid(dgPallet3);
            ClearDataGrid(dgSourceCell3);


            txtPalletID3.Text = string.Empty;
            txtSourceCellID3.Text = string.Empty;
        }

        private void CreateOutBox()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtoutbox.Text))
                {
                    //OUTBOX�� �Է��ϼ���
                    Util.MessageInfo("SFU5008");
                    txtoutbox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtinbox.Text))
                {
                    // Inbox ID�� �Է� �ϼ���.
                    Util.MessageInfo("SFU4517");
                    txtoutbox.Focus();
                    return;
                }

                string sInbox = string.Empty;

                string sInboxTemp = txtinbox.Text.ToString().ToUpper().Trim();
                sInbox = sInboxTemp.Substring(0, 1).ToUpper() == "C" && sInboxTemp.Length == 22 ? sInboxTemp.Substring(0, sInboxTemp.Length - 1) : sInboxTemp;

                DataSet ds = new DataSet();

                DataTable dtIndata = ds.Tables.Add("INDATA");

                dtIndata.Columns.Add("SRCTYPE");
                dtIndata.Columns.Add("IFMODE");
                dtIndata.Columns.Add("EQPTID");
                dtIndata.Columns.Add("INBOXID");
                dtIndata.Columns.Add("OUTBOXID");
                dtIndata.Columns.Add("USERID");
                dtIndata.Columns.Add("TAPING_RESULT");
                dtIndata.Columns.Add("OUTPUT_TYPE");

                DataRow dr = null;
                dr = dtIndata.NewRow();

                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = string.Empty;
                dr["INBOXID"] = sInbox;
                dr["OUTBOXID"] = txtoutbox.Text.Trim();
                dr["USERID"] = LoginInfo.USERID;
                dr["TAPING_RESULT"] = 0;
                dr["OUTPUT_TYPE"] = 0;

                dtIndata.Rows.Add(dr);

                //OUTBOX  ���� (UI) 
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_REG_OUTBOX_MB", "INDATA", null, ds);

                Util.MessageInfo("SFU1275", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtTargetCellID1.Focus();
                        txtTargetCellID1.SelectAll();
                    }
                });
                txtinbox.Text = string.Empty;
                txtoutbox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AddCell()
        {
            try
            {
                string sBizName = "BR_PRD_REG_ADD_SUBLOT_MB"; // string.Empty;

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                string sPalletID = txtPalletID2.Text.Trim(); //INBOXID

                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("BOXID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));

                inSublotTable.Columns.Add("SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("FORM_TRAY_PSTN_NO", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "BOXID"));
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataTable.Rows.Add(newRow);

                string sublot = string.Empty;
                string position = string.Empty;

                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    #region Cell �Է� üũ �� ��ġ �Է� üũ
                    sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID")).Trim();
                    position = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "FORM_TRAY_PSTN_NO")).Trim();

                    if (string.IsNullOrWhiteSpace(sublot))
                    {
                        Util.MessageInfo("SFU1495"); //"��� Cell ID�� �Էµ��� �ʾҽ��ϴ�."
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(position))
                    {
                        Util.MessageInfo("SFU1234"); // ��ġ�� �Էµ��� �ʾҽ��ϴ�.
                        return;
                    }
                    if (position.Length != 3)
                    {
                        Util.MessageInfo("SFU1234"); // ��ġ�� �Էµ��� �ʾҽ��ϴ�.
                        return;
                    }

                    Regex regex = new Regex(@"^[A-Z]");
                    Regex regex2 = new Regex(@"^[0-9]");
                    Boolean ismatch = regex.IsMatch(position.ToString().Substring(0, 1));
                    Boolean ismatch2 = regex2.IsMatch(position.ToString().Substring(1, 2));

                    if (!ismatch || !ismatch2)
                    {
                        Util.MessageInfo("SFU1234"); // ��ġ�� �Էµ��� �ʾҽ��ϴ�.
                        return;
                    }

                    string matchpstn = Regex.Replace(position, @"[^A-Z0-9]", "");

                    if (matchpstn != position)
                    {
                        // ���ڿ� �����빮�ڸ� �Է°����մϴ�.
                        Util.MessageValidation("SFU3674");
                        return;
                    }

                    #endregion

                    DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                    drsub["SUBLOTID"] = sublot;
                    drsub["FORM_TRAY_PSTN_NO"] = position;

                    indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                }

                new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INSUBLOT", string.Empty, indataSet);

                // "���������� ó���Ͽ����ϴ�." >>����ó���Ǿ����ϴ�.
                Util.MessageInfo("SFU1275", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtPalletID2.Focus();
                        txtPalletID2.SelectAll();
                    }
                });

                AllClear2(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void CreateInbox()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataTable inSublotTable = indataSet.Tables.Add("IN_SUBLOT");

                inDataTable.Columns.Add("SRCTYPE", typeof(string)); //OFF, ON
                inDataTable.Columns.Add("IFMODE", typeof(string)); //UI, EIF
                inDataTable.Columns.Add("INNER_BOXID", typeof(string));
                inDataTable.Columns.Add("INNER_BOX_TYPE", typeof(string)); //N:1ȸ�� R:��Ȱ��
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inSublotTable.Columns.Add("SUBLOTID", typeof(string));   //SUBLOT ID
                inSublotTable.Columns.Add("SUBLOT_PSTN_NO", typeof(string)); //SUBLOT ��ġ����

                string NewBox = txtNewBoxID.Text.Trim();

                _BoxType = "N";
                //�Ʒ� BR_PRD_CHK_DUP_BOX_MB BIZ���� ���ⱸ���ڸ� ���� �ϱ⶧���� ���ⱸ���ڰ� ���� ��� ������ �߰���.
                if (NewBox.Length == 21)
                {
                    NewBox = NewBox + "A";
                }//INBOX ID�� 21,22�ڸ��� �ƴϸ�
                else if (NewBox.Length > 22 || NewBox.Length < 21)
                {
                    //BOX ID�� �������� �ʴ� �׸��� �ֽ��ϴ�. �ٽ� Ȯ�����ּ���.
                    Util.MessageInfo("FM_ME_0508");
                    return;
                }

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = "";
                dr["USERID"] = LoginInfo.USERID;
                dr["INNER_BOXID"] = NewBox;
                dr["INNER_BOX_TYPE"] = _BoxType;
                inDataTable.Rows.Add(dr);

                string sublot = string.Empty;
                string position = string.Empty;

                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID")).Trim();
                    position = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "FORM_TRAY_PSTN_NO")).Trim();

                    if (string.IsNullOrWhiteSpace(sublot))
                    {
                        Util.MessageInfo("SFU1495"); //"��� Cell ID�� �Էµ��� �ʾҽ��ϴ�."
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(position))
                    {
                        Util.MessageInfo("SFU1234"); // ��ġ�� �Էµ��� �ʾҽ��ϴ�.
                        return;
                    }
                    if (position.Length != 3)
                    {
                        Util.MessageInfo("SFU1234"); // ��ġ�� �Էµ��� �ʾҽ��ϴ�.
                        return;
                    }

                    Regex regex = new Regex(@"^[A-Z]");
                    Regex regex2 = new Regex(@"^[0-9]");
                    Boolean ismatch = regex.IsMatch(position.ToString().Substring(0, 1));
                    Boolean ismatch2 = regex2.IsMatch(position.ToString().Substring(1, 2));

                    if (!ismatch || !ismatch2)
                    {
                        Util.MessageInfo("SFU1234"); // ��ġ�� �Էµ��� �ʾҽ��ϴ�.
                        return;
                    }


                    string matchpstn = Regex.Replace(position, @"[^A-Z0-9]", "");

                    if (matchpstn != position)
                    {
                        // ���ڿ� �����빮�ڸ� �Է°����մϴ�.
                        Util.MessageValidation("SFU3674");
                        return;
                    }

                    DataRow drsub = inSublotTable.NewRow();
                    drsub["SUBLOTID"] = sublot;
                    //drsub["FORM_TRAY_PSTN_NO"] = position;
                    drsub["SUBLOT_PSTN_NO"] = position;

                    inSublotTable.Rows.Add(drsub);
                }
                new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_INBOX_MB", "INDATA,IN_SUBLOT", "OUTDATA", indataSet);

                // "���������� ó���Ͽ����ϴ�." >>����ó���Ǿ����ϴ�.
                Util.MessageInfo("SFU1275", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtPalletID2.Focus();
                        txtPalletID2.SelectAll();

                        chkNewBox.IsChecked = false;
                        txtNewBox.Visibility = Visibility.Collapsed;
                        txtNewBoxID.Visibility = Visibility.Collapsed;
                    }
                });

                AllClear2(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        /// <summary>
        /// ���� Parameter�� PalletId�� ������� Ȯ��(Packed), Packed���°� �ƴϰų� ����� Pallet�� ������ Cell �߰� ����, ��ü�� ����
        /// </summary>
        /// <returns></returns>

        public int GetPackedStat(string sSublotid)
        {
            //SUBLOTID 
            //INNER_BOXID
            //OUTER_BOXID
            //PALLETID 


            int iReturn = 1;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CELLID"] = sSublotid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PACKED_SUBLOT_MB", "RQSTDT", "RSLTDT", RQSTDT);

                string palletid = dtResult.Rows[0]["PALLETID"].ToString();

                if (palletid == string.Empty)
                {
                    iReturn = 0; // OK
                }
                else
                {
                    iReturn = 1; // ERROR (�̹� �����)
                }

                return iReturn;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw ex;

            }

        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 365)
                {
                    Util.Alert("SFU5033", new object[] { "12" });  // �Ⱓ�� {}�� �̳� �Դϴ�.
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-365);
                    //SetGridDate();
                    return;
                }

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    //SetGridDate();
                    return;
                }
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 365)
                {
                    //Util.Alert("SFU5033", new object[] { "12" });  // �Ⱓ�� {}�� �̳� �Դϴ�.
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+365);
                    return;
                }

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                    //SetGridDate();
                    return;
                }
            }
        }
    }
    #endregion
}
