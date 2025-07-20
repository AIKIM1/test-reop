/*************************************************************************************
 Created Date : 2017.01.06
      Creator : JEONG JONGWON
   Decription : CUT LOT POPUP
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// ELEC001_015_LOTCUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_LOTCUT : C1Window, IWorkArea
    {
        Util _Util = new Util();

        private string procID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_LOTCUT()
        {
            InitializeComponent();
        }

        #region FormLoad Event
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            procID = tmps[0] as string;
        }
        #endregion
        #region TextBox Event
        private void txtCutQty_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (e.Text != ".")
                {
                    if (!char.IsDigit(c))
                    {
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.Key == Key.Enter)
            {
                GetMainCutLot();
            }
        }

        private void txtCutQty_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if( e.Key == Key.Enter)
            {
                SetCutLotSplit();
            }
        }
        #endregion
        #region Grid Event
        private void dgCutList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e != null && string.Equals(e.Cell.Column.Name, "WIPQTY"))
            {
                double dInputQty = Convert.ToDouble(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY"));

                double dTotalCutQty = 0.0;
                for (int i = 0; i < dgCutList.Rows.Count; i++)
                    dTotalCutQty += Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[i].DataItem, "WIPQTY"));

                if (dInputQty < dTotalCutQty)
                {
                    Util.AlertInfo("SFU2831"); //Cut 수량이 투입량을 초과하였습니다.
                    DataTableConverter.SetValue(dgCutList.Rows[e.Cell.Row.Index].DataItem, "WIPQTY", 0.0);
                    return;
                }

                // 변환률 계산
                DataTableConverter.SetValue(dgCutList.Rows[e.Cell.Row.Index].DataItem, "WIPQTY2",
                    Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[e.Cell.Row.Index].DataItem, "WIPQTY"))
                    * Convert.ToDouble(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "CONVERSIONS")));

                // 잔량 계산
                DataTableConverter.SetValue(dgLotList.Rows[0].DataItem, "WIPQTY2", dInputQty - dTotalCutQty);                
            }
        }
        #endregion
        #region Button Event
        private void btnCut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetCutLotSplit();
        }

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dgLotList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2993"); //현재 조회된 Cut Lot 정보가 없습니다.
                return;
            }

            if ( dgCutList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2991"); //현재 Cut 수가 지정되지 않았습니다.
                return;
            }

            for (int i = 0; i < dgCutList.Rows.Count; i++)
            {
                if ( Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[i].DataItem, "WIPQTY")) == 0 )
                {
                    Util.MessageValidation("SFU2078"); //Cut할 수량이 0입니다.
                    dgCutList.SelectedIndex = i;
                    return;
                }
            }

            LotCutProcess();
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region User Method
        private void GetMainCutLot()
        {
            try
            {
                Util.gridClear(dgLotList);
                Util.gridClear(dgCutList);
                txtCutQty.Text = "";

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = txtLotId.Text;
                Indata["PROCID"] = procID;

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPLIT_LOT", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU2829"); //Cut 가능한 Lot이 존재하지 않습니다.
                    //txtLotId.Focus();
                    return;
                }

                dgLotList.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCutLotSplit()
        {
            if (dgLotList.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2993"); //현재 조회된 Cut Lot 정보가 없습니다.
                return;
            }

            if (string.IsNullOrEmpty(txtCutQty.Text) || Convert.ToDouble(txtCutQty.Text) < 1)
            {
                Util.MessageValidation("SFU2830"); //Cut 수량이 0이거나 입력되지 않았습니다.
                //txtCutQty.Focus();
                return;
            }

            Util.gridClear(dgCutList);
            DataTableConverter.SetValue(dgLotList.Rows[0].DataItem, "WIPQTY2",Convert.ToString(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY")));

            if (dgCutList.ItemsSource == null)
            {
                DataTable colDt = new DataTable();
                for (int i = 0; i < dgCutList.Columns.Count; i++)
                    colDt.Columns.Add(dgCutList.Columns[i].Name);

                dgCutList.ItemsSource = DataTableConverter.Convert(colDt);
            }

            DataTable dtCut = ((DataView)dgCutList.ItemsSource).Table;
            for ( int i = 0; i < Convert.ToInt32(txtCutQty.Text); i++)
            {
                DataRow inputRow = dtCut.NewRow();

                inputRow["SEQNO"] = Convert.ToString(i + 1);
                inputRow["WIPQTY"] = 0.0;
                inputRow["WIPQTY2"] = 0.0;
                dtCut.Rows.Add(inputRow);
            }
        }

        private void LotCutProcess()
        {
            // DATA SET
            BizDataSet bizRule = new BizDataSet();
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("IN_INLOT");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("ACTQTY", typeof(double));
            inDataTable.Columns.Add("ACTQTY2", typeof(double));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("IN_OUTLOT");
            inProduct.Columns.Add("ACTQTY", typeof(double));
            inProduct.Columns.Add("ACTQTY2", typeof(double));

            DataTable inTable = indataSet.Tables["IN_INLOT"];
            DataRow newRow = inTable.NewRow();
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            newRow["LOTID"] = txtLotId.Text;
            newRow["PROCID"] = procID;
            newRow["ACTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY2")); 
            newRow["ACTQTY2"] = Convert.ToDouble(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY2"))
                 * Convert.ToDouble(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "CONVERSIONS"));
            newRow["NOTE"] = string.Empty;
            newRow["USERID"] = LoginInfo.USERID;

            inTable.Rows.Add(newRow);

            DataTable inSplit = indataSet.Tables["IN_OUTLOT"];
            for (int i = 0; i < dgCutList.Rows.Count; i++)
            {
                newRow = null;
                newRow = inSplit.NewRow();
                newRow["ACTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[i].DataItem, "WIPQTY"));
                newRow["ACTQTY2"] = Convert.ToDouble(DataTableConverter.GetValue(dgCutList.Rows[i].DataItem, "WIPQTY2"));

                inSplit.Rows.Add(newRow);
            }

            new ClientProxy().ExecuteService_Multi(GetCutLotBizRuleName(), "IN_INLOT,IN_OUTLOT", "OUT_OUTL0T", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable result = bizResult.Tables["OUT_OUTL0T"];

                    if (result.Rows.Count > 0)
                    {
                        StringBuilder resultMsg = new StringBuilder();
                        resultMsg.Append(ObjectDic.Instance.GetObjectName("CUT 결과") + "\r\n");
                        resultMsg.Append("----- " + ObjectDic.Instance.GetObjectName("모LOT 정보") + " -----" + "\r\n");
                        resultMsg.Append(MessageDic.Instance.GetMessage("SFU3224", new object[] {
                            Convert.ToString(result.Rows[0]["LOTID"]), Convert.ToString(DataTableConverter.GetValue(dgLotList.Rows[0].DataItem, "WIPQTY")),
                            Convert.ToString(result.Rows[0]["ACTQTY"]), Convert.ToString(result.Rows[0]["ACTQTY2"]) }) + "\r\n");
                        resultMsg.Append("----- " + ObjectDic.Instance.GetObjectName("자LOT 정보") + " -----" + "\r\n");

                        for (int i = 1; i < result.Rows.Count; i++)
                        {
                            resultMsg.Append(MessageDic.Instance.GetMessage("SFU3225", new object[] {
                                Convert.ToString(result.Rows[i]["LOTID"]), Convert.ToString(result.Rows[i]["ACTQTY"]), Convert.ToString(result.Rows[i]["ACTQTY2"]) }) + "\r\n");
                        }

                        Util.MessageInfo(resultMsg.ToString());
                        resultMsg.Clear();
                    }

                    this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }, indataSet);
        }

        private string GetCutLotBizRuleName()
        {
            string bizRuleName;

            switch (procID)
            {
                case "E2500" :
                    bizRuleName = "BR_PRD_REG_SPLIT_LOT_HS";
                    break;

                case "E3000" :
                default :
                    bizRuleName = "BR_PRD_REG_SPLIT_LOT_RP";
                    break;
            }

            return bizRuleName;
        }
        #endregion
    }
}
