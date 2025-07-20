/*************************************************************************************
 Created Date : 2021.02.24
      Creator : 정문교
   Decription : Roll Pressing -> Half Slitting LOT 공정 이동, 이동취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.02.24  정문교 : Initial Created.
   
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
using System.Collections.Generic;

namespace LGC.GMES.MES.ELEC003
{
    /// <summary>
    /// RW 작업시작 팝업.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC003_MOVE_CANCEL_HALFSLITTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _Move_YN = string.Empty;

        Util _Util = new Util();

        DateTime lastKeyPress = DateTime.Now;

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ELEC003_MOVE_CANCEL_HALFSLITTING()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _processCode = tmps[0] as string;
            _equipmentSegmentCode = tmps[1] as string;
            _Move_YN = tmps[2] as string;

            initGridTable();

            if (_Move_YN == "Y")
            {
                this.Header = ObjectDic.Instance.GetObjectName("공정이동");

                btnMoveCancel.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Header = ObjectDic.Instance.GetObjectName("이동취소");

                btnMove.Visibility = Visibility.Collapsed;
            }

            txtLotID.Focus();
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationLotID()) return;

                string LotID = txtLotID.Text.Trim();

                DataTable dt = GetLotInfo(LotID);

                if (dt == null || dt.Rows.Count == 0)
                {
                    //해당하는 LOT정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2025"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotID.Focus();
                            txtLotID.Text = string.Empty;
                            return;
                        }
                    });
                }
                else
                {
                    if (!ValidationWipInfo(dt.Rows[0])) return;

                    GridAddRow(dgLotInfo, dt);

                    txtLotID.Focus();
                    txtLotID.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button dg = sender as Button;
                if (dg != null &&
                    dg.DataContext != null &&
                    (dg.DataContext as DataRowView).Row != null)
                {
                    DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                    DataRow dtRow = (dg.DataContext as DataRowView).Row;

                    dt.Select("LOTID = '" + dtRow["LOTID"] + "'").ToList<DataRow>().ForEach(row => row.Delete());
                    dt.AcceptChanges();
                    Util.GridSetData(dgLotInfo, dt, null);

                    txtLotID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 이동
        /// </summary>
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMove()) return;

            //이동 하시겠습니까?
            Util.MessageConfirm("SFU1763", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Move();
                }
            });
        }

        /// <summary>
        /// 이동 취소
        /// </summary>
        private void btnMoveCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMoveCancel()) return;

            //%1 취소 하시겠습니까?
            Util.MessageConfirm("SFU4620", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    MoveCancel();
                }
            }, ObjectDic.Instance.GetObjectName("이동"));

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private DataTable GetLotInfo(string lotid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("MOVE_YN", typeof(string));

            DataRow row = dt.NewRow();
            row["PROCID"] = _processCode;
            row["EQSGID"] = _equipmentSegmentCode; ;
            row["LOTID"] = lotid;
            row["MOVE_YN"] = _Move_YN;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_WIP_MOVE_HS", "INDATA", "OUTDATA", dt);

            return result;
        }

        private void Move()
        {
            try
            {
                DataTable dtMove = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                DataSet indataSet = new DataSet();
                DataTable InData = indataSet.Tables.Add("INDATA");
                InData.Columns.Add("SRCTYPE", typeof(string));
                InData.Columns.Add("IFMODE", typeof(string));
                InData.Columns.Add("USERID", typeof(string));
                InData.Columns.Add("PROCID_FR", typeof(string));
                InData.Columns.Add("PROCID_TO", typeof(string));
                InData.Columns.Add("NOTE", typeof(string));

                DataTable InLot = indataSet.Tables.Add("IN_LOT");
                InLot.Columns.Add("LOTID", typeof(string));
                /////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = InData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID_FR"] = dtMove.Rows[0]["PROCID"].ToString();
                newRow["PROCID_TO"] = _processCode;
                newRow["NOTE"] = "";
                InData.Rows.Add(newRow);

                foreach (DataRow dr in dtMove.Rows)
                {
                    newRow = InLot.NewRow();
                    newRow["LOTID"] = dr["LOTID"].ToString();
                    InLot.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_MOVE_HS", "INDATA,IN_LOT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();

                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void MoveCancel()
        {
            try
            {
                DataTable dtMoveCancel = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                DataSet indataSet = new DataSet();
                DataTable InData = indataSet.Tables.Add("INDATA");
                InData.Columns.Add("SRCTYPE", typeof(string));
                InData.Columns.Add("IFMODE", typeof(string));
                InData.Columns.Add("USERID", typeof(string));
                InData.Columns.Add("PROCID_FR", typeof(string));
                InData.Columns.Add("NOTE", typeof(string));

                DataTable InLot = indataSet.Tables.Add("IN_LOT");
                InLot.Columns.Add("LOTID", typeof(string));
                /////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = InData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID_FR"] = _processCode;
                newRow["NOTE"] = "";
                InData.Rows.Add(newRow);

                foreach (DataRow dr in dtMoveCancel.Rows)
                {
                    newRow = InLot.NewRow();
                    newRow["LOTID"] = dr["LOTID"].ToString();
                    InLot.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_MOVECANCEL_HS", "INDATA,IN_LOT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();

                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]
        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, _equipmentSegmentCode, _processCode };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, null);
        }

        private void initGridTable()
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgLotInfo.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            dgLotInfo.BeginEdit();
            Util.GridSetData(dgLotInfo, dt, null);
            dgLotInfo.EndEdit();
        }

        private void GridAddRow(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            DataTable preTable = DataTableConverter.Convert(dg.ItemsSource);

            if (preTable.Columns.Count == 0)
            {
                preTable = new DataTable();
                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    preTable.Columns.Add(Convert.ToString(col.Name));
                }
            }

            DataRow row = preTable.NewRow();
            row["LOTID"] = Convert.ToString(dt.Rows[0]["LOTID"]);
            row["PROCID"] = Convert.ToString(dt.Rows[0]["PROCID"]);
            row["PROCNAME"] = Convert.ToString(dt.Rows[0]["PROCNAME"]);
            row["WIPQTY"] = Convert.ToString(dt.Rows[0]["WIPQTY"]);
            row["PRJT_NAME"] = Convert.ToString(dt.Rows[0]["PRJT_NAME"]);
            row["PRODID"] = Convert.ToString(dt.Rows[0]["PRODID"]);
            row["PRODNAME"] = Convert.ToString(dt.Rows[0]["PRODNAME"]);
            row["LANE_QTY"] = Convert.ToString(dt.Rows[0]["LANE_QTY"]);
            preTable.Rows.Add(row);

            Util.GridSetData(dg, preTable, FrameOperation, true);
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

        #region[Validation]
        private bool ValidationLotID()
        {
            if (string.IsNullOrWhiteSpace(txtLotID.Text))
            {
                // LOT ID를 입력해주세요
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1366"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtLotID.Focus();
                        txtLotID.Text = string.Empty;
                    }
                });

                return false;
            }

            return true;
        }

        private bool ValidationWipInfo(DataRow dr)
        {
            if (dr["WIPSTAT"].ToString() != Wip_State.WAIT)
            {
                // 대기LOT이 아닙니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1220"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtLotID.Focus();
                        txtLotID.Text = string.Empty;
                    }
                });

                return false;
            }

            if (dr["WIPHOLD"].ToString() == "Y")
            {
                // HOLD 된 LOT ID 입니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1340"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtLotID.Focus();
                        txtLotID.Text = string.Empty;
                    }
                });

                return false;
            }

            if (_Move_YN == "Y")
            {
                //if (dr["PRDT_CLSS_CODE"].ToString() != dr["ELTR_TYPE_CODE"].ToString())
                //{
                //    // 극성 정보가 다릅니다.
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2057"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            txtLotID.Focus();
                //            txtLotID.Text = string.Empty;
                //        }
                //    });

                //    return false;
                //}
                if (dr["PRDT_CLSS_CODE"].ToString() != "A")
                {
                    // 극성 정보가 다릅니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2057"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotID.Focus();
                            txtLotID.Text = string.Empty;
                        }
                    });

                    return false;
                }
            }

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")).Equals(dr["LOTID"]))
                {
                    // 해당 LOT이 이미 존재합니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2014"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotID.Focus();
                            txtLotID.Text = string.Empty;
                        }
                    });

                    return false;
                }
            }

            return true;
        }

        private bool ValidationMove()
        {
            if (dgLotInfo.GetRowCount() == 0)
            {
                Util.MessageValidation("SFU1651");     //선택된 항목이 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationMoveCancel()
        {
            if (dgLotInfo.GetRowCount() == 0)
            {
                Util.MessageValidation("SFU1651");     //선택된 항목이 없습니다.
                return false;
            }

            return true;
        }

        #endregion

        #endregion

    }
}
