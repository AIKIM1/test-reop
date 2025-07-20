/*************************************************************************************
 Created Date : 2022.09.15
      Creator : 김호선
   Decription : PACK RTLS CMA,BMA 판정 등록
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.15  김호선 : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_149_JUDG_Save : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _JudgResult = string.Empty;
        private string search_Type = string.Empty;
        private string search_ID = string.Empty;
        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string RESULT
        {
            get { return _JudgResult; }
        }

        public COM001_149_JUDG_Save()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init_cboJudgResult_Scan(cboJudgRslt);
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                search_Type = tmps[0].ToString();
                search_ID   = tmps[1].ToString();

                switch (search_Type)
                {
                    case "Lot":
                        Search_Lot_Judg();
                        break;
                    case "Cst":
                        Search_Cst_Judg();
                        break;
                    case "Pallet":
                        Search_Pallet_Judg();
                        break;
                    case "QRCode":
                        Search_QR_Judg();
                        break;
                }
            }
            else
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            

        }

        private void Search_QR_Judg()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("QRCODE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow row = RQSTDT.NewRow();
                row["QRCODE"] = search_ID;
                row["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_RTLS_GET_QRCODE_INFO", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {

                        Util.MessageException(ex);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        txtNote.Text = bizResult.Rows[0]["JUDG_NOTE"].ToString();
                        Util.GridSetData(dgJudgResult, bizResult, FrameOperation, false);
                    }
                    else
                    {
                        Util.MessageInfo("SFU1905");
                        this.Close();
                    }
                });
            }
            catch (Exception ex)
            {

                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void Search_Lot_Judg()
        {
            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow row = RQSTDT.NewRow();
                row["LOTID"] = search_ID;
                row["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_RTLS_SEL_EM_LOT_UI", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {

                        Util.MessageException(ex);
                        return;
                    }

                    if(bizResult.Rows.Count > 0)
                    {
                        txtNote.Text = bizResult.Rows[0]["JUDG_NOTE"].ToString();
                        Util.GridSetData(dgJudgResult, bizResult, FrameOperation, false);
                    }
                    else
                    {
                        Util.MessageInfo("SFU1905");
                        this.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void Search_Cst_Judg()
        {
            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("PANCAKE_GR_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROD_TYPE", typeof(string));
                DataRow row = RQSTDT.NewRow();
                row["PANCAKE_GR_ID"] = search_ID;
                row["LANGID"] = LoginInfo.LANGID;
                row["PROD_TYPE"] = "Y";
                RQSTDT.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_RTLS_GET_CST_INFO", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {

                        Util.MessageException(ex);
                        return;
                    }
                   
                    DataTable dt = null;
                    DataRow[] result = bizResult.Select("WIPSTAT <> 'TERM'", "");
                    if(result.Length > 0)
                    {
                        dt = result.CopyToDataTable();
                    }
                    if (dt != null)
                    {
                        txtNote.Text = dt.Rows[0]["JUDG_NOTE"].ToString();
                        Util.GridSetData(dgJudgResult, dt, FrameOperation, false);
                    }
                    else
                    {
                        Util.MessageInfo("SFU1905");
                        this.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void Search_Pallet_Judg()
        {
            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("PALLET_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow row = RQSTDT.NewRow();
                row["PALLET_ID"] = search_ID;
                row["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_RTLS_GET_PALLET_INFO", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {

                        Util.MessageException(ex);
                        return;
                    }
                    DataTable dt = null;
                    DataRow[] result = bizResult.Select("WIPSTAT <> 'TERM'", "");
                    if(result.Length > 0)
                    {
                        dt = result.CopyToDataTable();
                    }
                    if (dt != null)
                    {
                        txtNote.Text = dt.Rows[0]["JUDG_NOTE"].ToString();
                        Util.GridSetData(dgJudgResult, dt, FrameOperation, false);
                    }
                    else
                    {
                        Util.MessageInfo("SFU1905");
                        this.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        private void Init_cboJudgResult_Scan(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LOT_USG_TYPE_CODE";
                dr["ATTRIBUTE1"] = "P";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                DataRow ro = dtResult.NewRow();
                ro["CBO_NAME"] = "-SELECT-";
                ro["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(ro, 0);

                cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                cbo.SelectedIndex = 0;
            }
            catch
            {


            }
        }
        #endregion

   
        #region [저장]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!fn_Validation()) return;

                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void Save()
        {
            try
            {
                string sResnCode = string.Empty;

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("EM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EM_JUDG_RSLT_CODE", typeof(string));
                RQSTDT.Columns.Add("FINL_EM_DFCT_CODE", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));
                RQSTDT.Columns.Add("INSUSER", typeof(string));
                RQSTDT.Columns.Add("INSDTTM", typeof(DateTime));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM", typeof(DateTime));
                RQSTDT.Columns.Add("JUDG_NOTE", typeof(string));
                RQSTDT.Columns.Add("SAVE_TYPE", typeof(string));
                DataRow row = null;

                for (int i = 0; i < dgJudgResult.Rows.Count; i++)
                {
                    row = RQSTDT.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgJudgResult.Rows[i].DataItem, "LOTID"));
                    row["EM_TYPE_CODE"] = "OTHER";
                    row["EM_JUDG_RSLT_CODE"] = cboJudgRslt.SelectedValue.ToString();
                    row["FINL_EM_DFCT_CODE"] = null;
                    row["NOTE"] = null;
                    row["INSUSER"] = LoginInfo.USERID;
                    row["INSDTTM"] = DateTime.Now;
                    row["UPDUSER"] = LoginInfo.USERID;
                    row["UPDDTTM"] = DateTime.Now;
                    row["JUDG_NOTE"] = txtNote.Text;
                    row["SAVE_TYPE"] = "Y";
                    RQSTDT.Rows.Add(row);
                }
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("BR_RTLS_LOT_SCAN_SAVE", "RQSTDT", "", RQSTDT, (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {

                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1518");
                    _JudgResult = DataTableConverter.Convert(cboJudgRslt.ItemsSource).Rows[cboJudgRslt.SelectedIndex]["CBO_NAME"].ToString();
                    this.DialogResult =  MessageBoxResult.OK;
                    this.Close();

                });
            }
            catch (Exception)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                throw;
            }
        }
        private bool fn_Validation()
        {

            if (Util.GetCondition(cboJudgRslt) == "")
            {
                Util.MessageValidation("SFU3372");
                return false;
            }
            if(dgJudgResult.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1195");
                return false;
            }
            return true;
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
        #endregion

        #endregion


    }
}