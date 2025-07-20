using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_007_TRAY_CLEANING_MGT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_007_TRAY_CLEANING_MGT : C1Window, IWorkArea
    {
        public IFrameOperation FrameOperation { get; set; }
        private string _EqptID = string.Empty;
        private const string __DA_BAS_SEL_CARRIER_NOT_CLEANED_TRAY = "DA_BAS_SEL_CARRIER_NOT_CLEANED_TRAY";
        private const string __DA_BAS_SEL_CARRIER_NOT_CLEANED_TRAY_EA = "DA_BAS_SEL_CARRIER_NOT_CLEANED_TRAY_EA";
        private const string __DA_BAS_UPD_CARRIER_CLEANDATE_INIT = "DA_BAS_UPD_CARRIER_CLEANDATE_INIT";

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public ASSY001_007_TRAY_CLEANING_MGT()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 3)
            {
                _EqptID = Util.NVC(tmps[0]);
            }
            else
            {
                _EqptID = string.Empty;
            }

            InitCombo();

        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //라인
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            cboEquipmentSegment.SelectedValue = _EqptID;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgOutTray);
            Util.gridClear(dgInitTray);

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("EQSGID", typeof(string));
            dtRqst.Columns.Add("DATEDIF", typeof(int));

            DataRow dr = dtRqst.NewRow();
            DataTable dtRslt = new DataTable();

            dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            dr["DATEDIF"] = Convert.ToInt32(nbPassDay.Value);

            dtRqst.Rows.Add(dr);
            dtRslt = new ClientProxy().ExecuteServiceSync(__DA_BAS_SEL_CARRIER_NOT_CLEANED_TRAY, "INDATA", "OUTDATA", dtRqst);

            Util.GridSetData(dgOutTray, dtRslt, FrameOperation, true);
            
        }

        private void btnTrayInit_Click(object sender, RoutedEventArgs e)
        {
            DataTable trayInfo = new DataTable();
            trayInfo.Columns.Add("USERID", typeof(string));
            trayInfo.Columns.Add("TRAYID", typeof(string));

            for (int i = 0; i < dgInitTray.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgInitTray.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgInitTray.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    DataRow row = trayInfo.NewRow();
                    row["USERID"] = LoginInfo.USERID;
                    row["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgInitTray.Rows[i].DataItem, "TRAYID"));
                    trayInfo.Rows.Add(row);
                }
            }

            try
            {
                new ClientProxy().ExecuteService(__DA_BAS_UPD_CARRIER_CLEANDATE_INIT, "INDATA", null, trayInfo, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                });

                btnSearch_Click(null, null);
            }catch(Exception ex)
            {
                Util.AlertByBiz("", ex.Message, ex.ToString());
            }
        }

        private void CheckBox1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgOutTray.Selection.Clear();

                CheckBox cb = sender as CheckBox;

                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
                {
                    DataTable dtTo = DataTableConverter.Convert(dgInitTray.ItemsSource);

                    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo.Columns.Add("CHK", typeof(Boolean));
                        dtTo.Columns.Add("TRAYID", typeof(string));
                        dtTo.Columns.Add("LAST_DTTM", typeof(string));
                        dtTo.Columns.Add("PASSDAYS", typeof(int));
                    }

                    if (dtTo.Select("TRAYID = '" + DataTableConverter.GetValue(cb.DataContext, "TRAYID") + "'").Length > 0) //중복조건 체크
                    {
                        throw new Exception(MessageDic.Instance.GetMessage("SFU1777"));
                    }

                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        if (dc.DataType.Equals(typeof(Boolean)))
                        {
                            dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                        }
                        else
                        {
                            dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                        }
                    }

                    dtTo.Rows.Add(dr);
                    dgInitTray.ItemsSource = DataTableConverter.Convert(dtTo);

                    DataRow[] drUnchk = DataTableConverter.Convert(dgOutTray.ItemsSource).Select("CHK = 0");

                    if (drUnchk.Length == 0)
                    {
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.IsChecked = true;
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                    }

                }
                else//체크 풀릴때
                {
                    chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                    chkAll.IsChecked = false;
                    chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                    DataTable dtTo = DataTableConverter.Convert(dgInitTray.ItemsSource);

                    dtTo.Rows.Remove(dtTo.Select("TRAYID = '" + DataTableConverter.GetValue(cb.DataContext, "TRAYID") + "'")[0]);

                    dgInitTray.ItemsSource = DataTableConverter.Convert(dtTo);
                }
            }catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOutTray.ItemsSource == null) return;

            DataTable dt = ((DataView)dgOutTray.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            chkAllSelect();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgOutTray.ItemsSource == null) return;

            DataTable dt = ((DataView)dgOutTray.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            chkAllClear();
        }

        private void chkAllClear()
        {
            Util.gridClear(dgInitTray);
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgInitTray);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgOutTray.ItemsSource);
            dtSelect = dtTo.Copy();

            dgInitTray.ItemsSource = DataTableConverter.Convert(dtSelect);
        }

        private void tbTrayNo_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (tbTrayNo.Text.Trim() != "")
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("TRAYID", typeof(string));

                        DataRow drr = dtRqst.NewRow();
                        DataTable dtRslt = new DataTable();

                        drr["TRAYID"] = tbTrayNo.Text.Trim();

                        dtRqst.Rows.Add(drr);
                        dtRslt = new ClientProxy().ExecuteServiceSync(__DA_BAS_SEL_CARRIER_NOT_CLEANED_TRAY_EA, "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows.Count < 1)
                        {
                            throw new Exception(MessageDic.Instance.GetMessage("SFU1430")); //Tray가 없습니다
                        }
                        else
                        {
                            CheckBox cb = sender as CheckBox;
                            DataTable dtTo = DataTableConverter.Convert(dgInitTray.ItemsSource);

                            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                            {
                                dtTo.Columns.Add("CHK", typeof(Boolean));
                                dtTo.Columns.Add("TRAYID", typeof(string));
                                dtTo.Columns.Add("LAST_DTTM", typeof(string));
                                dtTo.Columns.Add("PASSDAYS", typeof(int));
                            }

                            if (dtTo.Select("TRAYID = '" + tbTrayNo.Text.Trim() + "'").Length > 0) //중복조건 체크
                            {
                                throw new Exception(MessageDic.Instance.GetMessage("SFU1777")); //존재하는 Tray입니다.
                            }
                            dgInitTray.ItemsSource = DataTableConverter.Convert(dtTo);
                            dgInitTray.IsReadOnly = false;
                            dgInitTray.BeginNewRow();
                            dgInitTray.EndNewRow(true);

                            DataTableConverter.SetValue(dgInitTray.CurrentRow.DataItem, "TRAYID", dtRslt.Rows[0]["TRAYID"].ToString());
                            DataTableConverter.SetValue(dgInitTray.CurrentRow.DataItem, "LAST_DTTM", dtRslt.Rows[0]["LAST_DTTM"].ToString());
                            DataTableConverter.SetValue(dgInitTray.CurrentRow.DataItem, "PASSDAYS", dtRslt.Rows[0]["PASSDAYS"].ToString());

                            dgInitTray.IsReadOnly = true;
                        }
                    }
                    else
                        return;

                    tbTrayNo.Clear();
                }
            }
            catch (Exception ex)
            {
                tbTrayNo.Clear();
                Util.MessageException(ex);
            }
        }
    }
}
