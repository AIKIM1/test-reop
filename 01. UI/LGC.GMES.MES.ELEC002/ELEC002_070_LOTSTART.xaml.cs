/*************************************************************************************
 Created Date : 2020.09.04
      Creator : 
   Decription : 재와인딩 작업시작
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace LGC.GMES.MES.ELEC002
{
    /// <summary>
    /// RW 작업시작 팝업.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC002_070_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _RUNLOT = string.Empty;
        private string _PRODID = string.Empty;
        private string _SCANID = string.Empty;

        DataTable dtInputLotInfo = null;  // 작업시작Lot

        Util _Util = new Util();

        DateTime lastKeyPress = DateTime.Now;

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ELEC002_070_LOTSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            dtInputLotInfo = tmps[0] as DataTable;
            _PROCID = tmps[1] as string;
            _EQPTID = tmps[2] as string;
            _EQSGID = tmps[3] as string;

            txtBarcode.Focus();
            //dtInputLotInfo.Columns.Add("MERGEQTY", typeof(decimal));
            GetInputLot();
        }

        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            if (dgInputLotInfo.GetRowCount() < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgInputLotInfo, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            if ((chkCore.IsChecked == false) &&(string.IsNullOrEmpty(txtRWCSTID.Text)))
            {
                Util.MessageValidation("SFU7006");  //Carrier ID를 입력하세요.    
                return;
            }

            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        #region # IN_EQP
                        DataTable IN_EQP = indataSet.Tables.Add("IN_EQP");
                        IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                        IN_EQP.Columns.Add("IFMODE", typeof(string));
                        IN_EQP.Columns.Add("EQPTID", typeof(string));
                        IN_EQP.Columns.Add("USERID", typeof(string));

                        DataRow row = IN_EQP.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = _EQPTID;
                        row["USERID"] = LoginInfo.USERID;
                        IN_EQP.Rows.Add(row);
                        #endregion

                        #region # IN_INPUT
                        string _MERGE = string.Empty;
                        DataTable IN_INPUT = indataSet.Tables.Add("IN_INPUT");
                        IN_INPUT.Columns.Add("LOTID", typeof(string));
                        IN_INPUT.Columns.Add("LANE_QTY", typeof(decimal));
                        IN_INPUT.Columns.Add("MERGE", typeof(string));
                        for (int i = 0; i < dgInputLotInfo.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "CHK").Equals("True"))
                            {
                                if (string.Equals(Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "LOTID")), Util.NVC(cboLot.SelectedValue)))
                                    _MERGE = "Y";
                                else
                                    _MERGE = "N";

                                DataRow newRow = IN_INPUT.NewRow();
                                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "LOTID"));
                                newRow["LANE_QTY"] = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "LANE_QTY"));
                                newRow["MERGE"] = _MERGE;
                                IN_INPUT.Rows.Add(newRow);
                            }
                        }
                        #endregion

                        #region # IN_OUTPUT
                        DataTable IN_OUTPUT = indataSet.Tables.Add("IN_OUTPUT");
                        IN_OUTPUT.Columns.Add("OUT_CSTID", typeof(string));
                        DataRow outrow = IN_OUTPUT.NewRow();
                        outrow["OUT_CSTID"] = Util.NVC(txtRWCSTID.Text);
                        IN_OUTPUT.Rows.Add(outrow);
                        #endregion

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MOVE_MERGE_RW", "IN_EQP,IN_INPUT,IN_OUTPUT", null, indataSet);

                        //정상처리되었습니다.
                        Util.MessageInfo("SFU1275", (xresult) =>
                        {
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void cboLotList_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (cboLot.Items.Count > 1)
            {
                MergeSum();
            }
        }
        private void cboLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLot.Items.Count > 1)
            {
                MergeSum();
            }
        }

        private void txtBarcode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl) ||
                e.Key == Key.Insert && Keyboard.IsKeyDown(Key.LeftShift) || e.Key == Key.Insert && Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;
            }
        }

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                txtRWCSTID.Text = txtBarcode.Text;
                if (DateTime.Now.Subtract(lastKeyPress).Milliseconds > 30)
                {
                    txtBarcode.Text = "";
                    txtRWCSTID.Text = "";
                }
                lastKeyPress = DateTime.Now;
            }
            catch (Exception ex) { }
        }
        #endregion

        #region Mehod
        private void initGridTable()
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgInputLotInfo.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            dgInputLotInfo.BeginEdit();
            dgInputLotInfo.ItemsSource = DataTableConverter.Convert(dt);
            dgInputLotInfo.EndEdit();
        }

        private void GetInputLot()
        {
            try
            {
                initGridTable();

                Util.GridSetData(dgInputLotInfo, dtInputLotInfo, FrameOperation);

                //foreach (DataRow row in dtInputLotInfo.Rows)
                //{
                //    //decimal wipqty = decimal.Parse(row.Field<string>("WIPQTY"));
                //    //decimal mergeqty = decimal.Parse(row.Field<string>("WIPQTY"));
                //    //decimal remainqty = wipqty - mergeqty;

                //    row.SetField("CHK", true);
                //    //row.SetField("MERGEQTY", mergeqty);
                //    //row.SetField("REMAIN_QTY", remainqty);
                //}

                //Util.GridSetData(dgInputLotInfo, dtInputLotInfo, FrameOperation);

                if (dtInputLotInfo.Rows.Count > 0)
                    SetLotListCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetLotListCombo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_NAME", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = null;

            for (int i = 0; i < dgInputLotInfo.GetRowCount(); i++)
            {
                row = dt.NewRow();
                row["CBO_NAME"] = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "LOTID"));
                row["CBO_CODE"] = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "LOTID"));
                dt.Rows.Add(row);
            }

            cboLot.DisplayMemberPath = "CBO_CODE";
            cboLot.SelectedValuePath = "CBO_NAME";
            cboLot.ItemsSource = DataTableConverter.Convert(dt);

            cboLot.SelectedIndex = 0;
        }

        private void MergeSum()
        {
            List<DataRow> WipQty = DataTableConverter.Convert(dgInputLotInfo.ItemsSource).AsEnumerable().Where(c => !string.IsNullOrWhiteSpace(c.Field<string>("WIPQTY"))).ToList();

            txtMergeQty.Text = Convert.ToString(WipQty.AsEnumerable().Sum(c => Double.Parse(c.Field<string>("WIPQTY"))));
        }

        private void getOutCarrier()
        {
            try
            {
                DataSet indataSet = new DataSet();
                #region # IN_EQP
                DataTable IN_EQP = indataSet.Tables.Add("IN_EQP");
                IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                IN_EQP.Columns.Add("IFMODE", typeof(string));
                IN_EQP.Columns.Add("EQPTID", typeof(string));
                IN_EQP.Columns.Add("USERID", typeof(string));

                DataRow row = IN_EQP.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _EQPTID;
                row["USERID"] = LoginInfo.USERID;
                IN_EQP.Rows.Add(row);
                #endregion

                #region # IN_OUTPUT
                DataTable IN_OUTPUT = indataSet.Tables.Add("IN_OUTPUT");
                IN_OUTPUT.Columns.Add("OUT_CSTID", typeof(string));
                DataRow outrow = IN_OUTPUT.NewRow();
                outrow["OUT_CSTID"] = Util.NVC(txtRWCSTID.Text);
                IN_OUTPUT.Rows.Add(outrow);
                #endregion

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_RW_UNLDR_INPUT", "IN_EQP,IN_OUTPUT", "OUTDATA", indataSet);

                if (dsRslt != null && dsRslt.Tables.Count > 0 && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    DataTable dtResult = dsRslt.Tables["OUTDATA"];
                    _RUNLOT = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    _PRODID = Util.NVC(dtResult.Rows[0]["PRODID"]);

                    cboLot.SelectedValue = _RUNLOT;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
