/*************************************************************************************
 Created Date : 2020.04.02
      Creator : 
   Decription : 생산설비 품질 승인
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.02  DEVELOPER : Initial Created.
    
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_325_CREATE_FIND_PROD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_325_CREATE_FIND_PROD : C1Window, IWorkArea
    {

        #region Initialize
        private string _PLANDATE = string.Empty;
        private string _SHOPID = string.Empty;
        private string _AREAID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _PROCID = string.Empty;

        private string _PRODID = string.Empty;
        private string _PRJT = string.Empty;

        public string _getProductID
        {
            get { return _PRODID; }
        }

        public string _getPrjt
        {
            get { return _PRJT; }
        }

        public COM001_325_CREATE_FIND_PROD()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;
            _PLANDATE = Util.NVC(tmps[0]);
            _SHOPID = Util.NVC(tmps[1]);
            _AREAID = Util.NVC(tmps[2]);
            _EQSGID = Util.NVC(tmps[3]);
            _EQPTID = Util.NVC(tmps[4]);
            _PROCID = Util.NVC(tmps[5]);

            txtProd.Focus();
            txtProd.SelectAll();
        }
        private void dgResult_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (dgResult.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                if (dgResult.CurrentColumn.Name == "PRODID" || dgResult.CurrentColumn.Name == "PRJT_NAME")
                {
                    _PRODID = Util.NVC(DataTableConverter.GetValue(dgResult.CurrentRow.DataItem, "PRODID"));
                    _PRJT = Util.NVC(DataTableConverter.GetValue(dgResult.CurrentRow.DataItem, "PRJT_NAME"));

                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtProd.Text))
            {
                Util.MessageValidation("SFU8184");  //제품ID나 프로젝트명을 입력하세요
                return;
            }

            Search();
        }

        private void txtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtProd.Text))
                {
                    Util.MessageValidation("SFU8184");  //제품ID나 프로젝트명을 입력하세요
                    return;
                }

                Search();

                if (dgResult.Rows.Count == 1)
                {

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dgResult.CurrentRow.DataItem, "PRODID"));
                    _PRJT = Util.NVC(DataTableConverter.GetValue(dgResult.CurrentRow.DataItem, "PRJT_NAME"));
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
            }
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);

                dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dt.Rows[idx]["CHK"] = 1;
                dt.AcceptChanges();

                dgResult.ItemsSource = DataTableConverter.Convert(dt);

                //row 색 바꾸기
                dgResult.SelectedIndex = idx;
            }
        }

        private void btnSelect_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dgResult.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                return;
            }

            DataRow[] drChk = Util.gridGetChecked(ref dgResult, "CHK");

            if (drChk.Length <= 0)
            {
                Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                return;
            }

            _PRODID = Util.NVC(drChk[0]["PRODID"]);
            _PRJT = Util.NVC(drChk[0]["PRJT_NAME"]);

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region UserMethod

        private void Search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PLAN_DATE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("SEARCHTEXT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _PLANDATE;
                dr["PLAN_DATE"] = _PLANDATE;
                dr["SHOPID"] = _SHOPID;
                dr["AREAID"] = _AREAID;
                dr["EQSGID"] = _EQSGID;
                dr["EQPTID"] = _EQPTID;
                dr["SEARCHTEXT"] =  Util.NVC(txtProd.Text);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_QLTY_PROD_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgResult, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
