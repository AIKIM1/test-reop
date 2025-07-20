/*************************************************************************************
 Created Date : 2016.06.16
      Creator : JEONG JONGWON
   Decription : 전지 5MEGA-GMES 구축 - Tray 이동 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  JEONG JONGWON : Initial Created.
  2017.02.21  정문교C       : 디자인 수정 및 로직 구현
  
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_003_TRAY_MOVE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private string _procID = string.Empty;
        private string _lineID = string.Empty;
        private string _eqptID = string.Empty;
        private DataTable _dtFromLot;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public ASSY002_003_TRAY_MOVE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void ASSY002_003_TRAY_MOVE_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _lineID = tmps[1] as string;
            _eqptID = tmps[2] as string;
            _dtFromLot = tmps[3] as DataTable; // as DataTable;

            dgLotFrom.ItemsSource = DataTableConverter.Convert(_dtFromLot);

            GetMoveToLotList();
            GetMoveFromTrayList();

            dgLotTo.CommittedEdit += dgLotTo_CommittedEdit;
            dgTrayFrom.CommittedEdit += dgTrayFrom_CommittedEdit;

        }
        #endregion

        #region [이동]
        private void btnRightMove_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayMove())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 TRAY를 이동 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("선택된 TRAY를 이동 하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveTrayMove();
                }

            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region [DataGrid] - dgLotTo_CommittedEdit, dgTrayFrom_CommittedEdit
        /// <summary>
        /// To Lot List - To Tray List 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLotTo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            DataTable dtLotTo = DataTableConverter.Convert(dgLotTo.ItemsSource);

            string sLotid = dtLotTo.Rows[e.Cell.Row.Index]["LOTID"].ToString();

            foreach (DataRow row in dtLotTo.Rows)
            {
                if (row["LOTID"].Equals(sLotid))
                    continue;
                else
                    row["CHK"] = "0";
            }

            dtLotTo.AcceptChanges();
            Util.GridSetData(dgLotTo, dtLotTo, null, true);

            // To Tray 조회
            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").Equals(1))
                GetMoveToTrayList(sLotid);
            else
                Util.gridClear(dgTrayTo);
        }

        /// <summary>
        /// From Tray List - Cell 수량 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTrayFrom_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            DataTable dtTray = DataTableConverter.Convert(dgTrayFrom.ItemsSource);
            DataRow[] dr = dtTray.Select("CHK = '1'");

            decimal dSum = 0;

            foreach (DataRow row in dr)
            {
                dSum += Util.NVC_Decimal(row["CELLQTY"].ToString());
            }

            txtTrayFromCellSum.Text = dSum.ToString("N0");
        }
        #endregion

        #endregion

        #region User Mehod

        #region [BizCall]
        /// <summary>
        /// To Lot List
        /// </summary>
        private void GetMoveToLotList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_MOVE_TO_LOT_LIST_WS();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROD_LOTID"] = Util.NVC(_dtFromLot.Rows[0]["LOTID"].ToString());

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_TO_LOT_LIST_WS", "INDATA", "OUTDATA", IndataTable);

                Util.GridSetData(dgLotTo, dtResult, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// From Tray List
        /// </summary>
        private void GetMoveFromTrayList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_MOVE_FROM_OUT_LOT_LIST_WS();

                DataRow Indata = IndataTable.NewRow();
                Indata["PROD_LOTID"] = Util.NVC(_dtFromLot.Rows[0]["LOTID"].ToString());
                Indata["PROCID"] = _procID;
                Indata["EQSGID"] = _lineID;
                Indata["EQPTID"] = _eqptID;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_FROM_OUT_LOT_LIST_WS", "INDATA", "OUTDATA", IndataTable);

                Util.GridSetData(dgTrayFrom, dtResult, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// To Tray List
        /// </summary>
        private void GetMoveToTrayList(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_OUT_LOT_LIST_WS();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PR_LOTID"] = sLotID;
                Indata["PROCID"] = _procID;
                Indata["EQSGID"] = _lineID;
                Indata["EQPTID"] = _eqptID;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_WS", "INDATA", "OUTDATA", IndataTable);

                Util.GridSetData(dgTrayTo, dtResult, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SaveTrayMove()
        {
            try
            {
                ShowLoadingIndicator();

                int nrow = _util.GetDataGridCheckFirstRowIndex(dgLotTo, "CHK");
                String sToLot = Util.NVC(DataTableConverter.GetValue(dgLotTo.Rows[nrow].DataItem, "LOTID"));

                DataSet indataSet = _bizRule.GetBR_PRD_REG_MOVE_OUT_LOT_WS();

                DataTable inTable = indataSet.Tables["IN_DATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _eqptID;
                newRow["FROM_PROD_LOTID"] = Util.NVC(_dtFromLot.Rows[0]["LOTID"].ToString());
                newRow["TO_PROD_LOTID"] = sToLot;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                // in_cst 생성
                DataTable dtTray = DataTableConverter.Convert(dgTrayFrom.ItemsSource);
                DataRow[] drTary = dtTray.Select("CHK = '1'");

                DataTable inCSTTable = indataSet.Tables["IN_CST"];

                foreach (DataRow row in drTary)
                {
                    newRow = inCSTTable.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(row["OUT_LOTID"].ToString());
                    newRow["CSTID"] = Util.NVC(row["TRAYID"].ToString());
                    inCSTTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MOVE_OUT_LOT_WS", "IN_DATA,IN_CST", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetMoveToTrayList(sToLot);
                        GetMoveFromTrayList();

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region[[Validation]
        /// <summary>
        /// Tray 이동 가능 체크
        /// </summary>
        /// <returns></returns>
        private bool CanTrayMove()
        {
            int iRow = _util.GetDataGridCheckFirstRowIndex(dgTrayFrom, "CHK");

            if (_util.GetDataGridCheckFirstRowIndex(dgTrayFrom, "CHK") < 0)
            {
                //Util.Alert("이동 가능 Tray를 선택 하세요.");
                Util.MessageValidation("이동 가능 Tray를 선택 하세요.");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgLotTo, "CHK") < 0)
            {
                //Util.Alert("이동할 Lot을 선택 하세요.");
                Util.MessageValidation("이동할 Lot을 선택 하세요.");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
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

        #endregion



    }
}
