/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_102 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public COM001_102()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// ȭ�鳻 combo ����
        /// </summary>
        private void InitCombo()
        {
           //��,����,����,���� ����
            CommonCombo _combo = new CommonCombo();

            //��
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //����
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //����
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            String[] cbProcess = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, sFilter: cbProcess, sCase: "PROCESS_SORT", cbParent: cbProcessParent);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //����� ���Ѻ��� ��ư �����
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //������� ����� ���Ѻ��� ��ư �����

            dtpDateTo.SelectedDateTime = DateTime.Now;
            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);

        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            //{
            //    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
            //    {
            //        Util.AlertInfo("SFU2042", new object[] { "7" });   //�Ⱓ�� {0}�� �̳� �Դϴ�.
            //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
            //        return;
            //    }

            //    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
            //    {
            //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
            //        return;
            //    }
            //}
        }

        private void chk_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            CheckBox cb = sender as CheckBox;

            if (cb.DataContext == null)
                return;

            if ((bool)cb.IsChecked && (cb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);

                // ��ü Lot üũ ����(������ �ٸ� �� ���ý� ���� check ����)
                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.Rows[idx]["CHK"] = 1;
                dt.AcceptChanges();

                Util.GridSetData(dgResult, dt, null, true);

                //row �� �ٲٱ�
                dgResult.SelectedIndex = idx;
            }

        }

       
        private void dgResult_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (dgResult.Rows[e.Cell.Row.Index] == null)
                return;

            if (e.Cell.Column.IsReadOnly == false)
            {
                DataTableConverter.SetValue(dgResult.Rows[e.Cell.Row.Index].DataItem, "CHK", 1);

                //row �� �ٲٱ�
                dgResult.SelectedIndex = e.Cell.Row.Index;

            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgResult.CommittedEdit -= dgResult_CommittedEdit;

            ClearValue();
            GetResult();

            dgResult.CommittedEdit += dgResult_CommittedEdit;
        }

        private bool ValidateQualityUpdate()
        {
            DataRow[] drUpdate = DataTableConverter.Convert(dgResult.ItemsSource).Select("CHK = 1");

            if (drUpdate.Length == 0)
            {
                // ���õ� �׸��� �����ϴ�.
                Util.MessageValidation("SFU1651");
                return false;
            }

            foreach (DataRow dRow in drUpdate)
            {
                String sClctItem = Util.NVC(dRow["CLCTITEM"]);
                String sClctVal21 = Util.NVC(dRow["CLCTVAL21"]);
                String sProcId = Util.NVC(dRow["PROCID"]);

                //�ܺ��ǿ��� ����, ���尭��(���)/���尭��(����) 21��° �Է°��� �����̸�
                if ("A010".Equals(LoginInfo.CFG_SHOP_ID) && "F6200".Equals(sProcId) && ("SI194-001".Equals(sClctItem) || "SI195-001".Equals(sClctItem)) && String.IsNullOrEmpty(sClctVal21))
                {
                    Util.MessageValidation("SFU4980", 21);
                    return false;
                }
            }

            return true;
        }

        private void QualitySave()
        {
            try
            {
                DataRow[] drUpdate = DataTableConverter.Convert(dgResult.ItemsSource).Select("CHK = 1");

                //if (drUpdate.Length == 0)
                //{
                //    // ���õ� �׸��� �����ϴ�.
                //    Util.MessageValidation("SFU1651");
                //    return;
                //}

                const string bizRuleName = "BR_QCA_REG_WIP_DATA_CLCT";
                string colName = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTITEM", typeof(string));

                for (int col = 0; col < 30; col++)
                {
                    colName = "CLCTVAL" + (col + 1).ToString("00");
                    inTable.Columns.Add(colName, typeof(string));
                }
                inTable.Columns.Add("ACTDTTM", typeof(DateTime));

                foreach (DataRow dr in drUpdate)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = dr["LOTID"];
                    newRow["WIPSEQ"] = dr["WIPSEQ"];
                    newRow["EQPTID"] = dr["EQPTID"];
                    newRow["CLCTSEQ"] = dr["CLCTSEQ"];
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = dr["CLCTITEM"];
                    newRow["ACTDTTM"] = dr["ACTDATE"] + " " + dr["ACTTIME"] + ":00";

                    for (int col = 0; col < 30; col++)
                    {
                        colName = "CLCTVAL" + (col + 1).ToString("00");
                        newRow[colName] = dr[colName];
                    }
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("���� ó�� �Ǿ����ϴ�.");
                        Util.MessageInfo("SFU1889");

                        GetResult();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateQualityUpdate())
            {
                return;
            }

            // �˻� ����� ���� �Ͻðڽ��ϱ�?
            Util.MessageConfirm("SFU2811", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    QualitySave();
                }
            });
        }
        #endregion

        #region Mehod
        public void GetResult()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("DATE_FR", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_FR"] = Util.GetCondition(dtpDateFrom); 
                dr["DATE_TO"] = Util.GetCondition(dtpDateTo);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["LOTID"] = Util.GetCondition(txtLot.Text, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgResult, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        #region [�ʱ�ȭ]
        private void ClearValue()
        {
            Util.gridClear(dgResult);
        }

        #endregion

        #endregion

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
    }
}
