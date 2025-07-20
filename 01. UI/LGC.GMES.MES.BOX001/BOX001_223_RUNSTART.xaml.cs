/*************************************************************************************
 Created Date : 2017.05.22
      Creator : 이영준S
   Decription : 전지 5MEGA-GMES 구축 - 1차 포장 구성 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_223_RUNSTART : C1Window, IWorkArea
    {
        Util _util = new Util();

        string sSHOPID = string.Empty;
        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string PALLET_ID
        {
            get;
            set;
        }

        public BOX001_223_RUNSTART()
        {
            InitializeComponent();
        }

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _AREAID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            sSHFTID = Util.NVC(tmps[2]);
            sSHOPID = Util.NVC(tmps[3]);

            InitCombo();
            InitControl();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            //_combo.SetCombo(cboShipTo, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, null, null }, sCase: "SHIPTO_CP");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

        }
        #endregion

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunStart();
                }
            });
        }

        private void RunStart()
        {
            //if (cboShipTo.SelectedValue.Equals("SELECT"))
            if (string.IsNullOrEmpty(popShipto.SelectedValue.ToString()))
            {
                Util.MessageValidation("SFU4096");
                return;
            }

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SHFT_ID");
                inDataTable.Columns.Add("SHIPTO_ID");

                DataTable inBoxTable = inDataSet.Tables.Add("INPALLET");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("PKG_LOTID");

                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = _AREAID;
                newRow["USERID"] = sUSERID;
                newRow["SHFT_ID"] = sSHFTID;
                newRow["SHIPTO_ID"] = popShipto.SelectedValue;
                inDataTable.Rows.Add(newRow);
                newRow = null;


                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["BOXID"].Index).Value);
                    newRow["PKG_LOTID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["LOTID"].Index).Value);
                    inBoxTable.Rows.Add(newRow);
                }


                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_PALLET_NJ", "INDATA,INPALLET", "OUTDATA", inDataSet);
                if (dsResult != null && dsResult.Tables["OUTDATA"] != null)
                {
                    PALLET_ID = dsResult.Tables["OUTDATA"].Rows[0]["PALLETID"].ToString();
                }

                this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void setShipToCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string prodID)
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
            string[] arrCondition = { sSHOPID, prodID, LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, null);
        }
        private void setShipToPopControl(string prodID)
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
            string[] arrCondition = { sSHOPID, prodID, LoginInfo.LANGID };
            CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        private void txtInPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("BOXID");
                    dt.Columns.Add("LANGID");
                    DataRow dr = dt.NewRow();
                    dr["BOXID"] = txtInPalletID.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dt.Rows.Add(dr);
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPUT_INPALLET_LIST_NJ", "INDATA", "OUTDATA", dt);
                    DataTable dtSource = DataTableConverter.Convert(dgInPallet.ItemsSource);
                    var query = (from t in dtSource.AsEnumerable()
                                 where t.Field<string>("BOXID") == txtInPalletID.Text
                                 select t).Distinct();
                    if (query.Any())
                    {
                        Util.MessageValidation("SFU1781");
                        txtInPalletID.Clear();
                        return;
                    }
                    if (dtSource.Rows.Count > 0 && (!dtResult.Rows[0]["PRODID"].ToString().Equals(txtProdID.Text) || !dtResult.Rows[0]["PROJECT"].ToString().Equals(txtProject.Text)))
                    {
                        Util.MessageValidation("SFU4268");
                        txtInPalletID.Clear();
                        return;
                    }
                    if (dtSource.Rows.Count > 0 && !dtResult.Rows[0]["EXP_DOM_TYPE_CODE"].ToString().Equals(txtExpDomCode.Text))
                    {
                        Util.MessageValidation("SFU4269");
                        txtInPalletID.Clear();
                        return;
                    }

                    if (!dtResult.Columns.Contains("CHK"))
                        dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                    if (dtSource.Rows.Count == 0)
                    {
                        txtProdID.Text = dtResult.Rows[0]["PRODID"].ToString();
                        txtProject.Text = dtResult.Rows[0]["PROJECT"].ToString();
                        txtExpDomCode.Text = dtResult.Rows[0]["EXP_DOM_TYPE_CODE"].ToString();
                        //setShipToCombo(cboShipTo,CommonCombo.ComboStatus.SELECT, txtProdID.Text.Trim());
                        setShipToPopControl(txtProdID.Text.Trim());
                    }
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        txtCellQty.Value += int.Parse(dtResult.Rows[0]["TOTAL_QTY"].ToString());
                        txtInboxQty.Value += int.Parse(dtResult.Rows[0]["BOXQTY"].ToString());

                        dtResult.Merge(dtSource);
                        dgInPallet.ItemsSource = DataTableConverter.Convert(dtResult);
                        txtInPalletID.Clear();
                        txtInPalletID.Focus();

                        if (dgInPallet.Rows.Count > 0)
                        {
                            DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        }
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnInPalletDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);

                foreach (DataRow dr in dt.AsEnumerable().ToList())
                {
                    if (dr["CHK"].Equals(true))
                    {
                        dt.Rows.Remove(dr);
                    }
                }
                dgInPallet.ItemsSource = DataTableConverter.Convert(dt);

                if (dt.Rows.Count == 0)
                {
                    txtExpDomCode.Clear();
                    txtProdID.Clear();
                    txtProject.Clear();
                    txtCellQty.Value = 0;
                    txtInboxQty.Value = 0;
                    popShipto.ItemsSource = null;
                    popShipto.SelectedValue = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
