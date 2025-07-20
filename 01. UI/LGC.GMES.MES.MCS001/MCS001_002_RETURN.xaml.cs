/*************************************************************************************
 Created Date : 2018.09.20
      Creator : 오화백
   Decription : 수동반송 팝업
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
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using LGC.GMES.MES.MCS001.Controls;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_002_RETURN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_002_RETURN : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _lotId = string.Empty;   // Lot정보
        private string _Id = string.Empty;      // ID
        private string _Type = string.Empty;    // Type
        private string _Eqgrid = string.Empty;    // EQGIRID
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

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

        public MCS001_002_RETURN()
        {
            InitializeComponent();
        }

        private void InitializeUserControls()
        {

        }
        /// <summary>
        /// 폼로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                object[] tmps = C1WindowExtension.GetParameters(this);
                _lotId = tmps[0] as string;  // Lot정보
                _Id = tmps[1] as string;  // ID
                _Type = tmps[2] as string;  // Type
                _Eqgrid = tmps[3] as string;  // EQGRID
                SetControl();
                SetStockerCombo(cboStocker);
                SetCombo();
                _load = false;
            }
        }
        #endregion

        #region Event

        #region [반송지시]
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationReturn())
                return;

            // 반송하시겠습니까?
            Util.MessageConfirm("반송하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Return();
                }
            });
        }
        #endregion

        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboStocker.SelectedValue == null || cboStocker.SelectedIndex < 0) return;

            if(!string.IsNullOrEmpty(cboType.SelectedValue.GetString()))
                cboType_SelectedValueChanged(cboType, null);
        }

        #region 콤보박스 이벤트 : cboType_SelectedValueChanged()
        /// <summary>
        ///  콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboType.SelectedValue.ToString() == "PORT")
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter2 = { cboStocker?.SelectedValue?.GetString(), _Eqgrid };
                _combo.SetCombo(cboID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "CWALAMIPORT");
            }
            else if (cboType.SelectedValue.ToString() == "RACK")
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = cboStocker.SelectedValue;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_LAMI_RACK_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow drSel = dtResult.NewRow();

                drSel["CBO_CODE"] = "";
                drSel["CBO_NAME"] = "-SELECT-";
                dtResult.Rows.InsertAt(drSel, 0);

                cboID.DisplayMemberPath = "CBO_NAME";
                cboID.SelectedValuePath = "CBO_CODE";
                cboID.ItemsSource = dtResult.Copy().AsDataView();

                cboID.SelectedIndex = 0;
            }
            else
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE", typeof(string));
                dt.Columns.Add("CBO_NAME", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CBO_CODE"] = "";
                dr["CBO_NAME"] = "-SELECT-";
                dt.Rows.Add(dr);

                cboID.DisplayMemberPath = "CBO_NAME";
                cboID.SelectedValuePath = "CBO_CODE";
                cboID.ItemsSource = dt.Copy().AsDataView();

                cboID.SelectedIndex = 0;
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region 반송 : Return()
        /// <summary>
        /// 반송
        /// </summary>
        private void Return()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("FROM_ID", typeof(string));
                inTable.Columns.Add("FROM_TYPE", typeof(string));
                inTable.Columns.Add("TO_ID", typeof(string));
                inTable.Columns.Add("TO_TYPE", typeof(string));
                inTable.Columns.Add("LOGIS_CMD_PRIORITY_NO", typeof(Int32));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["FROM_ID"] = txtID.Text;
                newRow["FROM_TYPE"] = txtType.Text;
                newRow["TO_ID"] = cboID.SelectedValue.ToString();
                newRow["TO_TYPE"] = cboType.SelectedValue.ToString();
                newRow["LOGIS_CMD_PRIORITY_NO"] = txtReturnSeq.Value;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                if(txtAfterLot.Text.Length > 10)
                {
                    string[] strLotid = txtAfterLot.Text.Split(',');
                    for (int i = 0; i < strLotid.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(strLotid[i]))
                        {
                            newRow = inLot.NewRow();
                            newRow["LOTID"] = strLotid[i];
                            inLot.Rows.Add(newRow);
                        }
                    }
                }
                else
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = txtAfterLot.Text;
                    inLot.Rows.Add(newRow);
                }
                
                new ClientProxy().ExecuteService_Multi("BR_MCS_REG_LOGIS_CMD_NSP_NSR_MGV", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);

                    }
                }, inDataSet);

             
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Validation : ValidationReturn()
        /// <summary>
        ///  Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationReturn()
        {
            if (cboID.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU5053"); //ID를 선택하세요
                return false;
            }
            if (cboType.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU5054"); //Type을 선택하세요
                return false;
            }
            if (txtReturnSeq.Value == 0)
            {
                Util.MessageValidation("SFU5055"); //반송순위를 입력하세요
                return false;
            }
            return true;
        }

        #endregion

        #region LoadingIndicator : ShowLoadingIndicator(),HiddenLoadingIndicator()

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


        #endregion

        #region 파라미터 셋팅 : SetControl()

        private void SetControl()
        {
            txtAfterLot.Text = _lotId;
            txtBeforeLot.Text = _lotId;
            txtID.Text = _Id;
            txtType.Text = _Type;
        }

        #endregion

        #region 콤보박스 셋팅 : SetCombo()
        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void SetCombo()
        {

            CommonCombo _combo = new CommonCombo();

            //TYPE
            String[] sFilter1 = { "UI_TYPE" };
            _combo.SetCombo(cboType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            //ID
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_CODE", typeof(string));
            dt.Columns.Add("CBO_NAME", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CBO_CODE"] = "";
            dr["CBO_NAME"] = "-SELECT-";
            dt.Rows.Add(dr);

            cboID.DisplayMemberPath = "CBO_NAME";
            cboID.SelectedValuePath = "CBO_CODE";
            cboID.ItemsSource = dt.Copy().AsDataView();

            cboID.SelectedIndex = 0;
        }

        private void SetStockerCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, _Eqgrid, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }



        #endregion

        #endregion


    }
}