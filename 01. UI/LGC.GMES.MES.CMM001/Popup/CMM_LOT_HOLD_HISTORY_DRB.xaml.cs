/*************************************************************************************
 Created Date : 2020.10.13
      Creator : 정문교
   Decription : LOT HOLD이력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.13  정문교 : Initial Created.
    
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_LOT_HOLD_HISTORY_DRB.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_LOT_HOLD_HISTORY_DRB : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _lotID = string.Empty; 

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

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

        public CMM_LOT_HOLD_HISTORY_DRB()
        {
            InitializeComponent();
        }

        private void InitializeUserControls()
        {
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _lotID = tmps[0] as string;

            txtLotID.Text = _lotID;
        }

        private void SetControl()
        {
        }

        private void SetCombo()
        {
        }

        private void SetDataGridColumnVisibility()
        {
        }

        #endregion

        #region Event

        /// <summary>
        /// Form Load
        /// </summary>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            SetParameters();
            SetControl();
            SetCombo();
            SetDataGridColumnVisibility();

            // 조회
            SelectHistory();
        }

        /// <summary>
        /// 이력에서 출력
        /// </summary>
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            //라벨 발행용
            DataRow row1 = null;
            DataTable dtLabel = new DataTable();
            dtLabel.Columns.Add("LOTID", typeof(string));
            dtLabel.Columns.Add("RESNNAME", typeof(string));
            dtLabel.Columns.Add("MODELID", typeof(string));
            dtLabel.Columns.Add("WIPQTY", typeof(string));
            dtLabel.Columns.Add("PERSON", typeof(string));

            try
            {
                Button bt = sender as Button;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                row1 = dtLabel.NewRow();
                row1["LOTID"] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
                row1["RESNNAME"] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "HOLDCODENAME"));
                row1["MODELID"] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MODELID"));
                row1["WIPQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPQTY"))).ToString("###,###,##0.##");
                row1["PERSON"] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "ACTIONUSERNAME")); ;
                dtLabel.Rows.Add(row1);

                PrintHoldLabel(dtLabel);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        /// <summary>
        /// 조회
        /// </summary>
        private void SelectHistory()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _lotID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPHOLDHISTORY_DRB", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Func]

        private void PrintHoldLabel(DataTable inData)
        {
            try
            {
                //x,y 가져오기
                DataTable dt = LoginInfo.CFG_SERIAL_PRINT;

                string startX = "0";
                string startY = "0";
                if (dt.Rows.Count > 0)
                {
                    startX = dt.Rows[0]["X"].ToString();
                    startY = dt.Rows[0]["Y"].ToString();
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                for (int i = 0; i < inData.Rows.Count; i++)
                {
                    dtRqst.Rows[0]["LBCD"] = "LBL0013";
                    dtRqst.Rows[0]["PRMK"] = "Z";
                    dtRqst.Rows[0]["RESO"] = "203";
                    dtRqst.Rows[0]["PRCN"] = "1";
                    dtRqst.Rows[0]["MARH"] = startX;
                    dtRqst.Rows[0]["MARV"] = startY;
                    dtRqst.Rows[0]["ATTVAL001"] = inData.Rows[i]["MODELID"].ToString();
                    dtRqst.Rows[0]["ATTVAL002"] = inData.Rows[i]["LOTID"].ToString();
                    dtRqst.Rows[0]["ATTVAL003"] = inData.Rows[i]["WIPQTY"].ToString();
                    dtRqst.Rows[0]["ATTVAL004"] = inData.Rows[i]["RESNNAME"].ToString();
                    dtRqst.Rows[0]["ATTVAL005"] = DateTime.Now.ToString("yyyy.MM.dd");
                    dtRqst.Rows[0]["ATTVAL006"] = DateTime.Now.ToString("yyyy.MM.dd");
                    dtRqst.Rows[0]["ATTVAL007"] = inData.Rows[i]["PERSON"].ToString();
                    dtRqst.Rows[0]["ATTVAL008"] = "";
                    dtRqst.Rows[0]["ATTVAL009"] = "";

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                    try
                    {
                        // 프린터 정보 조회
                        string sPrt = string.Empty;
                        string sRes = string.Empty;
                        string sCopy = string.Empty;
                        string sXpos = string.Empty;
                        string sYpos = string.Empty;
                        string sDark = string.Empty;
                        DataRow drPrtInfo = null;

                        if (!_util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                            return;

                        if (PrintLabel(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo) == false)
                        {
                            Util.MessageValidation("SFU1309"); //Barcode Print 실패.
                            return;
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo == null || drPrtInfo.Table == null)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030")); //프린터 환경설정 정보가 없습니다.

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                //if (drPrtInfo["PORTNAME"].ToString().IndexOf("USB") >= 0)
                if (drPrtInfo["PORTNAME"].ToString().ToUpper().Equals("USB"))
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().ToUpper().IndexOf("COM") >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    Util.MessageValidation("SFU3031"); //프린터 환경설정에 포트명 항목이 없습니다.
                }
            }
            else
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031")); // 프린터 환경설정에 포트명 항목이 없습니다.

                Util.MessageValidation("SFU3031"); // 프린터 환경설정에 포트명 항목이 없습니다.
            }

            return brtndefault;
        }

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

    }
}