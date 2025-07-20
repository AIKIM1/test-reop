/*************************************************************************************
 Created Date : 2017.10.07
      Creator : 주건태
   Decription : 활성화 재공 현황 재고 특이사항 일괄 변경
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.17  최초착성.
 
**************************************************************************************/

using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_357_CREATE_HIST : C1Window, IWorkArea
    {
        private string preProdId = string.Empty;
        private string preModlId = string.Empty;
        private string prePrjtName = string.Empty;

        private string prodId = string.Empty;
        private string modlId = string.Empty;
        private string prjtName = string.Empty;

        #region Declaration & Constructor 


        public COM001_357_CREATE_HIST()
        {
            InitializeComponent();
            SetCombo();
        }

        private void SetCombo()
        {
            setComboAssyEquipment(cboModlChgEquipment);
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateSave())
                {
                    return;
                }

                // 저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveModlChgHist();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveModlChgHist()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_ASSY_MODL_CHG_CREATE_HIST";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("USERID", typeof(string));

                // INDATA
                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inModlChg = inDataSet.Tables.Add("INMODLCHG");
                inModlChg.Columns.Add("EQPTID", typeof(string));
                inModlChg.Columns.Add("PREPRJTNAME", typeof(string));
                inModlChg.Columns.Add("PRJTNAME", typeof(string));
                inModlChg.Columns.Add("MODLID", typeof(string));
                inModlChg.Columns.Add("PRODID", typeof(string));
                inModlChg.Columns.Add("MODL_CHG_STRT_DTTM", typeof(string));
                inModlChg.Columns.Add("MODL_CHG_RSLT_QTY", typeof(decimal));

                newRow = inModlChg.NewRow();

                string sEqptId = Util.GetCondition(cboModlChgEquipment);
                newRow["EQPTID"] = sEqptId;

                newRow["PREPRJTNAME"] = prePrjtName;

                newRow["PRJTNAME"] = prjtName;

                newRow["MODLID"] = modlId;

                newRow["PRODID"] = prodId;

                newRow["MODL_CHG_STRT_DTTM"] = Util.StringToDateTime(dtpDateFrom.DateTime.Value.ToString("yyyy-MM-dd HH:mm"));

                newRow["MODL_CHG_RSLT_QTY"] = Decimal.Parse(txtModlChgRsltQty.Text.ToString());


                inModlChg.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INMODLCHG", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다
                        Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #region Mehod



        private bool ValidateSave()
        {

            if (string.IsNullOrEmpty(txtPreProdId.Text.ToString()))
            {
                Util.MessageValidation("SFU8275", "PRE_PRODID");
                return false;
            }
            if (string.IsNullOrEmpty(txtProdId.Text.ToString()))
            {
                Util.MessageValidation("SFU8275", "PRODID");
                return false;
            }
            if (string.IsNullOrEmpty(txtModlChgRsltQty.Text.ToString()))
            {
                Util.MessageValidation("SFU8275", "RSLT_QTY");
                return false;
            }

            return true;
        }

        #endregion

        private void txt_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void setComboAssyEquipment(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ASSY_EQPT_MODL_CHG";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPreProdSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_357_PROD_SEARCH wndPreProdSearch = new COM001_357_PROD_SEARCH();
                wndPreProdSearch.FrameOperation = FrameOperation;

                if (wndPreProdSearch != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = 1;
                    C1WindowExtension.SetParameters(wndPreProdSearch, Parameters);

                    wndPreProdSearch.Closed += new EventHandler(wndPreProdSearch_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndPreProdSearch.ShowModal()));
                    //  grdMain.Children.Add(wndPrint);
                    //  wndPrint.BringToFront(); 
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnProd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_357_PROD_SEARCH wndProdSearch = new COM001_357_PROD_SEARCH();
                wndProdSearch.FrameOperation = FrameOperation;

                if (wndProdSearch != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = 1;
                    C1WindowExtension.SetParameters(wndProdSearch, Parameters);

                    wndProdSearch.Closed += new EventHandler(wndProdSearch_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndProdSearch.ShowModal()));
                    //  grdMain.Children.Add(wndPrint);
                    //  wndPrint.BringToFront(); 
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void wndPreProdSearch_Closed(object sender, EventArgs e)
        {
            COM001_357_PROD_SEARCH window = sender as COM001_357_PROD_SEARCH;
            if (window.DialogResult != MessageBoxResult.Cancel)
            {
                txtPreProdId.Text = window.prodId;

                preProdId = window.prodId;
                preModlId = window.modlId;
                prePrjtName = window.prjtName;
             }
        }

        private void wndProdSearch_Closed(object sender, EventArgs e)
        {
            COM001_357_PROD_SEARCH window = sender as COM001_357_PROD_SEARCH;
            if (window.DialogResult != MessageBoxResult.Cancel)
            {
                txtProdId.Text = window.prodId;

                prodId = window.prodId;
                modlId = window.modlId;
                prjtName = window.prjtName;
            }
        }

    }
}