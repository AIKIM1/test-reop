/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST_BIZWFLOT_SEARCH : C1Window, IWorkArea
    {
        private DataTable _dtBizWFHeader = null;
        private DataTable _dtBizWFDetail = null;

        private string _BIZ_WF_REQ_DOC_TYPE_CODE = string.Empty;
        private string _BIZ_WF_REQ_DOC_NO = string.Empty;

        private string _reqType = string.Empty;

        public string BIZ_WF_REQ_DOC_TYPE_CODE
        {
            get { return _BIZ_WF_REQ_DOC_TYPE_CODE; }
        }

        public string BIZ_WF_REQ_DOC_NO
        {
            get { return _BIZ_WF_REQ_DOC_NO; }
        }

        public DataTable BizWFHeader
        {
            get { return _dtBizWFHeader; }
        }

        public DataTable BizWFDetail
        {
            get { return _dtBizWFDetail; }
        }

        public COM001_035_REQUEST_BIZWFLOT_SEARCH()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _reqType = Util.NVC(tmps[0]);

                SearchBizWF();
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtBizWFHeader_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    SearchBizWF();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBizWF();
        }

        private void SearchBizWF()
        {
            try
            {
                /*
                if (string.IsNullOrEmpty(txtBizWFHeader.Text))
                {
                    Util.MessageValidation("SFU3792");  //BizWF 요청서 번호를 입력하세요.
                    return;
                }

                int iBizWFHeaderMinLength = 7;
                if (txtBizWFHeader.Text.Trim().Length < iBizWFHeaderMinLength)
                {
                    Util.MessageValidation("SFU3794", iBizWFHeaderMinLength);  //BizWF 요청서 번호는 %1 자리 이상 입력해 주세요.
                    return;
                }
                */

                if (string.IsNullOrEmpty(_reqType))
                {
                    Util.MessageValidation("SFU3537");  //조회된 데이타가 없습니다
                    return;
                }

                ClearGrid();
                ClearProperty();

                SearchBizWFHeader();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearGrid()
        {
            Util.gridClear(dgBizWFLotHeader);
            Util.gridClear(dgBizWFLotDetail);
        }

        private void ClearProperty()
        {
            _dtBizWFHeader = null;
            _dtBizWFDetail = null;

            _BIZ_WF_REQ_DOC_TYPE_CODE = string.Empty;
            _BIZ_WF_REQ_DOC_NO = string.Empty;
        }

        private void SearchBizWFHeader()
        {
            try
            {
                _dtBizWFHeader = null;
                Util.gridClear(dgBizWFLotHeader);

                DataTable dtInTable = new DataTable();
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("SHOPID", typeof(string));
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("REQ_TYPE", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow dr = dtInTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["REQ_TYPE"] = _reqType;
                dr["BIZ_WF_REQ_DOC_NO"] = txtBizWFHeader.Text.Trim();

                dtInTable.Rows.Add(dr);

                const string bizRuleName = "BR_PRD_SEL_ERP_BIZWF_DOC_HEADER";
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtInTable);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                }
                else
                {
                    //dgBizWFLotHeader.ItemsSource = DataTableConverter.Convert(dtRslt);
                    Util.GridSetData(dgBizWFLotHeader, dtRslt, FrameOperation);
                }

                //_dtBizWFHeader 데이터 입력은 Header 그리드 선택 라디오 체크시 수행
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchBizWFDetail(int pHeaderGridIndex)
        {
            try
            {
                _dtBizWFDetail = null;
                Util.gridClear(dgBizWFLotDetail);

                DataTable dtInTable = new DataTable();
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("SHOPID", typeof(string));
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));

                DataRow dr = dtInTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE").ToString();
                dr["BIZ_WF_REQ_DOC_NO"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_NO").ToString();
                
                dtInTable.Rows.Add(dr);

                const string bizRuleName = "BR_PRD_SEL_ERP_BIZWF_DOC_DETAIL";
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtInTable);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU3537");  //조회된 데이타가 없습니다
                }
                else
                {
                    //dgBizWFLotDetail.ItemsSource = DataTableConverter.Convert(dtRslt);
                    Util.GridSetData(dgBizWFLotDetail, dtRslt, FrameOperation);
                }

                _dtBizWFDetail = dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {

        }

        private void rbBizWFChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                {
                    return;
                }

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;

                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;

                        if (dg != null)
                        {
                            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                            for (int i = 0; i < dg.Rows.Count; i++)
                            {
                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", idx == i);
                            }

                            dgBizWFLotHeader.SelectedIndex = idx;

                            SetBizWFHeaderProperty(idx);

                            SearchBizWFDetail(idx);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetBizWFHeaderProperty(int pHeaderGridIndex)
        {
            try
            {
                _dtBizWFHeader = new DataTable();
                _dtBizWFHeader.Columns.Add("BIZ_WF_REQ_DOC_TYPE_CODE", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_REQ_DOC_TYPE_NAME", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_REQ_DOC_NO", typeof(string));
                _dtBizWFHeader.Columns.Add("COMPANY_CODE", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_GNRT_DATE", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_GNRT_HMS", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_GNRT_USERID", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_GNRT_USER_NAME", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_GNRT_USER_EMPNO", typeof(string));
                _dtBizWFHeader.Columns.Add("SHOPID", typeof(string));
                _dtBizWFHeader.Columns.Add("ERP_MOVE_TYPE", typeof(string));
                _dtBizWFHeader.Columns.Add("POST_DATE", typeof(string));
                _dtBizWFHeader.Columns.Add("ERP_DOC_EVD_DATE", typeof(string));
                _dtBizWFHeader.Columns.Add("ERP_DOC_NO", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_APPR_STAT_CODE", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_APPR_STAT_NAME", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_REQ_DOC_STAT_CODE", typeof(string));
                _dtBizWFHeader.Columns.Add("BIZ_WF_REQ_DOC_STAT_NAME", typeof(string));

                DataRow dr = _dtBizWFHeader.NewRow();
                dr["BIZ_WF_REQ_DOC_TYPE_CODE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE").ToString();
                dr["BIZ_WF_REQ_DOC_TYPE_NAME"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_TYPE_NAME") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_TYPE_NAME").ToString();
                dr["BIZ_WF_REQ_DOC_NO"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_NO") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_NO").ToString();
                dr["COMPANY_CODE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "COMPANY_CODE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "COMPANY_CODE").ToString();
                dr["BIZ_WF_GNRT_DATE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_DATE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_DATE").ToString();
                dr["BIZ_WF_GNRT_HMS"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_HMS") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_HMS").ToString();
                dr["BIZ_WF_GNRT_USERID"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_USERID") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_USERID").ToString();
                dr["BIZ_WF_GNRT_USER_NAME"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_USER_NAME") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_USER_NAME").ToString();
                dr["BIZ_WF_GNRT_USER_EMPNO"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_USER_EMPNO") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_GNRT_USER_EMPNO").ToString();
                dr["SHOPID"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "SHOPID") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "SHOPID").ToString();
                dr["ERP_MOVE_TYPE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "ERP_MOVE_TYPE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "ERP_MOVE_TYPE").ToString();
                dr["POST_DATE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "POST_DATE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "POST_DATE").ToString();
                dr["ERP_DOC_EVD_DATE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "ERP_DOC_EVD_DATE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "ERP_DOC_EVD_DATE");
                dr["ERP_DOC_NO"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "ERP_DOC_NO") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "ERP_DOC_NO");
                dr["BIZ_WF_APPR_STAT_CODE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_APPR_STAT_CODE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_APPR_STAT_CODE").ToString();
                dr["BIZ_WF_APPR_STAT_NAME"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_APPR_STAT_NAME") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_APPR_STAT_NAME").ToString();
                dr["BIZ_WF_REQ_DOC_STAT_CODE"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_STAT_CODE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_STAT_CODE").ToString();
                dr["BIZ_WF_REQ_DOC_STAT_NAME"] = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_STAT_NAME") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_STAT_NAME").ToString();
                
                _dtBizWFHeader.Rows.Add(dr);

                _BIZ_WF_REQ_DOC_TYPE_CODE = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_TYPE_CODE").ToString();
                _BIZ_WF_REQ_DOC_NO = DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_NO") == null ? null : DataTableConverter.GetValue(dgBizWFLotHeader.Rows[pHeaderGridIndex].DataItem, "BIZ_WF_REQ_DOC_NO").ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

    }
}