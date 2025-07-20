/*************************************************************************************
 Created Date : 2019.10.28
      Creator : 정문교
   Decription : FOL,STK Rework 작업시작
--------------------------------------------------------------------------------------
 [Change History]
 2019.10.28  정문교 : Initial Created.   폴란드3동 & 빈강용 조립 공정에서 사용
                                         ASSY004_050_RUNSTART Copy ASSY004_060_RUNSTART
 2019.12.02  정문교 : 시장 유형 콤보 추가
 2024.09.05  이원열 : [E20240830-001409] (AZS-STK 재작업 프로덕션 모델 초대)
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
    public partial class ASSY004_060_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _AreaID = string.Empty;
        private string _EqsgID = string.Empty;
        private string _EqptID = string.Empty;

        private string _ProdID = string.Empty;
        private string _EqgrID = string.Empty;
        BizDataSet _Biz = new BizDataSet();
        Util _Util = new Util();
        #endregion

        #region Initialize
        public ASSY004_060_RUNSTART()
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

            // 설비그룹
            _EqgrID = SearchEqgrid();
            // 반제품
            SearchProduct();
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            InitCombo();
            SetControl();
        }

        private void popSearchProdID_ValueChanged(object sender, EventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(popSearchProdID.ItemsSource);
            string prjt_name = string.Empty;
            foreach (DataRowView drv in dt.DefaultView)
            {
                if (Util.NVC(DataTableConverter.GetValue(drv, "PRODID")).Equals(popSearchProdID.SelectedValue as string))
                {
                    prjt_name = Util.NVC(DataTableConverter.GetValue(drv, "PRJT_NAME"));
                    break;
                }
            }

            _ProdID = Util.NVC(popSearchProdID.SelectedValue);
            txtPjtName.Text = prjt_name;
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
        /// 반제품 조회
        /// </summary>
        private void SearchProduct()
        {
            try
            {
                string ClssCode = string.Empty;

                if (_EqgrID == "FOL")
                {
                    ClssCode = "FC";
                }
                // E20240830-001409 : AZS 추가
                else if (_EqgrID == "AZS")
                {
                    ClssCode = "ZC";
                }
                else
                {
                    ClssCode = "SC";
                }

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("CLASS_LEVEL3_CODE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = _AreaID;

                // E20240830-001409 : AZS 추가
                if (_EqgrID == "AZS")
                {
                    newRow["PROCID"] = Process.AZS_STACKING;
                }
                else
                    newRow["PROCID"] = Process.STACKING_FOLDING;


                newRow["CLASS_LEVEL3_CODE"] = ClssCode;
                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_FP_EQSG_PROC_PRDT_BY_PROC", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchProdID.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
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
