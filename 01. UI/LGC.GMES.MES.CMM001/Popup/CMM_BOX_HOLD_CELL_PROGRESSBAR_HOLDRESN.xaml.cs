/*************************************************************************************
 Created Date : 2020.06.17
      Creator : 이동우
   Decription : 데이터 처리 시 ProgressBar 적용 및 데이터 그리드 행 (진행중/전체)카운트 비동기 적용 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.17 CMM_PROGRESSBAR_SAMPLE Copy 후 수정.
  2024.07.22 유재홍 CMM_BOX_HOLD_CELL_PROGRESSBAR 복사 후 HOLD_CODE 파라미터 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_BOX_HOLD_CELL_PROGRESSBAR.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_BOX_HOLD_CELL_PROGRESSBAR_HOLDRESN : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private DataTable dtInfo = new DataTable();
        private string dtpSchdDate = string.Empty;
        private string txtUser = string.Empty;
        private string txtNote = string.Empty;
        private string _holdTrgtCode = string.Empty;
        private double cellDivideCnt = 0;
        private int tempCellPosition = 0;
        private int tempFixCnt = 0;
        private int loopCnt = 0;
        private int temploopCnt = 0;
        private int tempCnt = 1;
        private bool chkStop = false;
        private string _PackHoldFlag = string.Empty;
        private string _HoldTypeCode = string.Empty;
        private string _WorkType = "HOLD";
        private string _HoldCode = string.Empty;
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


        public CMM_BOX_HOLD_CELL_PROGRESSBAR_HOLDRESN()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        #endregion

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetData();

            ProgressBarSystem();
        }
        #endregion

        #region [저장, 중지, 닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageValidation("SFU8221", dtInfo.Rows.Count, xProgress.Value);
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            chkStop = true;

            Util.MessageConfirm("SFU8220", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    chkStop = true;
                    btnClose_Click(null, null);
                }
                else
                {
                    chkStop = false;
                    ProgressBarSystem();
                }
            });
        }
        #endregion

        #region Mehod

        private async void ProgressBarSystem()
        {
            try
            {

                DataSet inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");

                string bizRuleName = "BR_PRD_REG_ASSY_HOLD";

                if (_WorkType == "RELEASE")
                {
                    //DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SHOPID");
                    inDataTable.Columns.Add("AREAID");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("UNHOLD_NOTE");
                    inDataTable.Columns.Add("HOLD_TYPE_CODE");
                    inDataTable.Columns.Add("HOLD_TRGT_CODE");

                    //DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                    inHoldTable.Columns.Add("STRT_SUBLOTID");

                    DataTable inTable = inDataSet.Tables["INDATA"];
                    DataRow newRow = inDataTable.NewRow();
                    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["UNHOLD_NOTE"] = txtNote;
                    newRow["HOLD_TYPE_CODE"] = _HoldTypeCode;
                    newRow["HOLD_TRGT_CODE"] = _holdTrgtCode;

                    inDataTable.Rows.Add(newRow);

                    bizRuleName = "BR_PRD_REG_CELL_PROD_UNHOLD";
                }
                else
                {
                    inDataTable.Columns.Add("AREAID");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("UNHOLD_SCHD_DATE");
                    inDataTable.Columns.Add("UNHOLD_CHARGE_USERID");
                    inDataTable.Columns.Add("HOLD_NOTE");
                    inDataTable.Columns.Add("HOLD_TRGT_CODE");
                    inDataTable.Columns.Add("SHOPID");
                    inDataTable.Columns.Add("PACK_HOLD_FLAG");
                    inDataTable.Columns.Add("HOLD_TYPE_CODE");

                    inHoldTable.Columns.Add("STRT_SUBLOTID");
                    inHoldTable.Columns.Add("HOLD_REG_QTY");
                    inDataTable.Columns.Add("HOLD_CODE");

                    DataTable inTable = inDataSet.Tables["INDATA"];
                    DataRow newRow = inDataTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["UNHOLD_SCHD_DATE"] = dtpSchdDate;
                    newRow["UNHOLD_CHARGE_USERID"] = txtUser;
                    newRow["HOLD_NOTE"] = txtNote;
                    newRow["HOLD_TRGT_CODE"] = _holdTrgtCode;
                    newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    newRow["PACK_HOLD_FLAG"] = _PackHoldFlag;
                    newRow["HOLD_TYPE_CODE"] = _HoldTypeCode;
                    newRow["HOLD_CODE"] = _HoldCode;

                    inDataTable.Rows.Add(newRow);

                    bizRuleName = "BR_PRD_REG_ASSY_HOLD";
                }

                await System.Threading.Tasks.Task.Run(() =>
                {
                    loadingIndicator.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate () { loadingIndicator.Visibility = Visibility.Visible; }));

                    for (var i = 0; i < loopCnt; i++)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            DataRow newRow2 = inHoldTable.NewRow();

                            for (int j = tempCellPosition; j < dtInfo.Rows.Count; j++)
                            {
                                newRow2 = inHoldTable.NewRow();
                                newRow2["STRT_SUBLOTID"] = dtInfo.Rows[j]["STRT_SUBLOTID"];
                                // 현재는 sublot 만 해당 팝업을 호출하고 있어서 아래와 같이 대응함
                                // lot을 처리할 경우 팝업 호출 시 수량 파라미터 추가하여 처리 필요
                                if (_holdTrgtCode == "SUBLOT" && _WorkType != "RELEASE")
                                {
                                    newRow2["HOLD_REG_QTY"] = 1;
                                }

                                inHoldTable.Rows.Add(newRow2);
                                tempCnt++;
                                if (j + 1 >= Convert.ToInt32(cellDivideCnt))
                                {
                                    tempCellPosition = j + 1;
                                    cellDivideCnt += tempFixCnt;
                                    break;
                                }

                            }

                            loadingIndicator.Visibility = Visibility.Collapsed;

                            try
                            {
                                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INHOLD", null, inDataSet);
                            }
                            catch (Exception e)
                            {
                                Util.MessageException(e);
                                chkStop = true;
                                return;
                            }

                            UpdateProgressBar(tempFixCnt);
                        }),
                        System.Windows.Threading.DispatcherPriority.Input
                    );
                        inHoldTable.Clear();
                        temploopCnt--;
                        if (chkStop == true)
                        {
                            loopCnt = temploopCnt;
                            break;
                        }
                    }

                });

                /*
                if (temploopCnt == 0)
                {
                    btnClose_Click(null, null);
                }
                */
                btnClose_Click(null, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                btnClose_Click(null, null);
            }
        }

        private void UpdateProgressBar(int value)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate {
                    xProgress.Value += value;
                    xTextBlock.Text = xProgress.Value + "/" + (dtInfo.Rows.Count);

                }),
                System.Windows.Threading.DispatcherPriority.Input
            );
        }

        private void SetData()
        {
            object[] tempParameters = C1WindowExtension.GetParameters(this);

            dtInfo = (DataTable)tempParameters[0];
            dtpSchdDate = tempParameters[3].ToString();
            txtUser = tempParameters[4].ToString();
            txtNote = tempParameters[5].ToString();
            _holdTrgtCode = tempParameters[6].ToString();
            _PackHoldFlag = tempParameters[7].ToString();
            _HoldTypeCode = Util.NVC(tempParameters[8]);
            _WorkType = Util.NVC(tempParameters[9]);
            _HoldCode = Util.NVC(tempParameters[10]);

            cellDivideCnt = Double.Parse(tempParameters[2].ToString());
            tempFixCnt = Convert.ToInt32(cellDivideCnt);

            loopCnt = Convert.ToInt32(Math.Ceiling(dtInfo.Rows.Count / cellDivideCnt));
            temploopCnt = loopCnt;

            xProgress.Maximum = dtInfo.Rows.Count;
            xProgress.Value = 0;
            xTextBlock.Text = xProgress.Value + "/" + (dtInfo.Rows.Count);
        }

        #endregion


    }
}