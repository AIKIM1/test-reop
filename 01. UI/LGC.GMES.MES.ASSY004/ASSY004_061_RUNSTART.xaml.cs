/*************************************************************************************
 Created Date : 2021.12.15
      Creator : 오화백
   Decription : FOL,STK Rework 작업시작  ESWA
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF.DataGrid;
using System.Threading;
using System.ComponentModel;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_060_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_061_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _AreaID = string.Empty;
        private string _EqsgID = string.Empty;
        private string _EqptID = string.Empty;

        private string _ProdID = string.Empty;
        private string _EqgrID = string.Empty;
        private string _PJT = string.Empty;
        BizDataSet _Biz = new BizDataSet();
        Util _Util = new Util();
        #endregion

        #region Initialize
        public ASSY004_061_RUNSTART()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitializeUserControls()
        {
            grdMsg.Visibility = Visibility.Collapsed;
        }

        private void InitCombo()
        {
            // 시장유형
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMKTtypeCode, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _AreaID = Util.NVC(tmps[0]);
            _EqsgID = Util.NVC(tmps[1]);
            _EqptID = Util.NVC(tmps[2]);
            // PJT 명
            _PJT = Util.NVC(tmps[3]);
            txtPjtName.Text = _PJT; 
            _ProdID = Util.NVC(tmps[4]);
            txtProdID.Text = _ProdID;
            // 설비그룹
            _EqgrID = SearchEqgrid();
           
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            InitCombo();
            SetControl();
        }

      

        /// <summary>
        /// 작업시작
        /// </summary>
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart())
                return;

            //  % 1(을)를 하시겠습니까?
            Util.MessageConfirm("SFU4329", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveRunStart();
                }
            }, ObjectDic.Instance.GetObjectName("작업시작"));
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Method

        #region [BizCall]
        /// <summary>
        /// 설비그룹 조회
        /// </summary>
        private string SearchEqgrid()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["EQPTID"] = _EqptID;
                inDataTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQGRID", "RQSTDT", "RSLTDT", inDataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0]["EQGRID"].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

      

        /// <summary>
        /// 작업시작
        /// </summary>
        private void SaveRunStart()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));
                inData.Columns.Add("RWK_TYPE_CODE", typeof(string));
                inData.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("MTRLID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("ACTQTY", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PRODID"] = _ProdID;
                newRow["RWK_TYPE_CODE"] = (bool)rdoReinspection.IsChecked ? "Q" : "R";
                newRow["MKT_TYPE_CODE"] = cboMKTtypeCode.SelectedValue.ToString();
                inData.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_PROD_LOT_RWK_ST_L", "INDATA,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        string lotID = Util.NVC(bizResult.Tables["OUTDATA"].Rows[0]["LOTID"]);
                        tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", lotID); // [%1] LOT이 생성 되었습니다.

                        grdMsg.Visibility = Visibility.Visible;

                        AsynchronousClose();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]
        private bool ValidationRunStart()
        {
            if (string.IsNullOrWhiteSpace(_ProdID))
            {
                // 선택된 반제품 정보가 없습니다.
                Util.MessageValidation("SFU1638");
                return false;
            }

            if (cboMKTtypeCode.SelectedValue == null || cboMKTtypeCode.SelectedValue.ToString().Equals("SELECT"))
            {
                // 시장유형을 선택하세요.
                Util.MessageValidation("SFU4371");
                return false;
            }

            return true;
        }

        #endregion


        #region [Function]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Collapsed)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void AsynchronousClose()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion


        #endregion



    }
}
