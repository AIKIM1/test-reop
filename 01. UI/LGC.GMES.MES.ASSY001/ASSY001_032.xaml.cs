/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_032 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        string _EQSGID = string.Empty;
        string _EQPTID = string.Empty;
        string _SkipFlag = string.Empty;

        int CommitFlag = -1;
        string LotID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_032()
        {
            InitializeComponent();
        }


        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        #endregion

        #region event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initCombo();
            txtQAUSER.Text = LoginInfo.USERNAME;
        }

        //조회조건 - 라인
        private void cboEquipmentSegmentVDQA_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] sFilter = { Process.VD_LMN, cboEquipmentSegmentVDQA.SelectedValue.ToString() };
            combo.SetCombo(cboEquipmentVDQA, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "cboVDEquipment");
            cboEquipmentVDQA.SelectedIndex = 0;

            DataTable dt = new DataTable();
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));

            DataRow row = dt.NewRow();
            row["PROCID"] = Process.VD_LMN;
            row["EQSGID"] = Convert.ToString(cboEquipmentSegmentVDQA.SelectedValue);
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PROD_SEL_PROCESSEQUIPMENTSEGMENT_VDQA", "RQSTDT", "RSLTDT", dt);
            if (result == null) return;
            if (result.Rows.Count == 0 || result.Rows[0]["LQC_SKIP_FLAG"].Equals(""))
            {
                Util.Alert("LQC_SKIP_FLAG가 없습니다.");
                return;
            }
            _SkipFlag = Convert.ToString(result.Rows[0]["LQC_SKIP_FLAG"]);
        }

        //조회조건 - 설비
        private void cboEquipmentVDQA_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            dgEquipment.ItemsSource = null;
            dgQAComplete.ItemsSource = null;
            dgRun.ItemsSource = null;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            drawingGrids();
        }

        //설비현황 그리드 내 체크박스 선택 시 Event
        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            GetEqptStatInfo(index);

        }

        private void GetEqptStatInfo(int index)
        {
            _EQPTID = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "EQPTID"));
            //_EQSGID = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "EQSGID"));



            if (Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "CHK")).Equals("1"))
            {
                for (int i = 0; i < dgEquipment.GetRowCount(); i++)
                {
                    if (i != index)
                    {
                        DataTableConverter.SetValue(dgEquipment.Rows[i].DataItem, "CHK", false);
                    }
                }

                //VD가동현황 set함수
                SetVDoperation(index);

                setLineCombo();

                //검사 완료 set함수
                setVDFinish(index);
            }
            else
            {
                dgRun.ItemsSource = null;
                dgQAComplete.ItemsSource = null;
            }
        }

        private void dgQAComplete_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK2"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgQAComplete == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgQAComplete.ItemsSource); ;

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK2"] = 1;
            }

            dgQAComplete.ItemsSource = DataTableConverter.Convert(dt);

        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgQAComplete == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgQAComplete.ItemsSource); ;

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK2"] = 0;
            }
            dgQAComplete.ItemsSource = DataTableConverter.Convert(dt);

        }

        //검사확정
        private void btnInspectionConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2") == -1)
            {
                Util.MessageValidation("SFU3365"); //확정할 LOT을 선택하세요
                return;
            }
            for (int i = 0; i < dgQAComplete.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "CHK2")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "CHK2")).Equals("True"))

                {
                    if (DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "JUDG_VALUE").Equals("HOLD") || DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "JUDG_VALUE").Equals("WAIT"))
                    {

                        Util.MessageValidation("SFU3367", DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "LOTID")); //LOT [%1] 은 확정 할 수 없는 상태 입니다. 
                        return;
                    }
                }
            }

            DataSet ds = new DataSet();
            DataTable inLot = ds.Tables.Add("IN_LOT");
            inLot.Columns.Add("LOTID", typeof(string));

            DataRow row2 = null;

            for (int i = 0; i < dgQAComplete.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "CHK2")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "CHK2")).Equals("True"))
                {
                    row2 = inLot.NewRow();
                    row2["LOTID"] = DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "LOTID");
                    inLot.Rows.Add(row2);
                }
            }
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3368"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //   loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_RPD_REG_QA_CONFIRM_FOR_VD", "RQST", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("BR_RPD_REG_QA_CONFIRM_FOR_VD", bizException.Message, bizException.ToString());
                                return;
                            }

                            Util.MessageInfo("SFU3369");//확정완료
                            SearchData();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }, ds);
                }
            });
        }

        private void dgQAComplete_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            int index = e.Cell.Row.Index;
            if (!e.Cell.Column.Name.Equals("JUDG_VALUE"))
            {
                return;
            }

            if (dgQAComplete.ItemsSource == null)
            {
                return;
            }

            LotID = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[index].DataItem, "LOTID"));
            string LOTID_RT = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[index].DataItem, "LOTID_RT"));// ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as C1.WPF.DataGrid.DataGridCellPresenter).Parent)).Row.Index;
            string JUDG_VALUE = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[index].DataItem, "JUDG_VALUE"));
            string EQPT_NO = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[index].DataItem, "EQPT_BTCH_WRK_NO"));

            DataRow dr = ((DataView)dgQAComplete.ItemsSource).Table.Rows[dgQAComplete.CurrentRow.Index];

            if (EQPT_NO.Equals(""))
            {
                //Util.Alert("배치ID가 없습니다.");
                Util.MessageValidation("SFU1561");
                e.Cell.Value = dr[dgQAComplete.CurrentColumn.Name, DataRowVersion.Original];
                return;
            }

            CommitFlag = 0; //수정 후 저장 해야함

        }

        private void cboInspection_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            int index = _Util.GetDataGridCheckFirstRowIndex(dgEquipment, "CHK");
            if (index == -1)
            {
                return;
            }
            setVDFinish(index);
        }
        #endregion

        #region Method
        private void initCombo()
        {
            string[] sFilter = { Process.VD_LMN, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboEquipmentSegmentVDQA, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "cboVDEquipmentSegment");

            string[] str = { "QAJUDGE" };
            combo.SetCombo(cboInspection, CommonCombo.ComboStatus.ALL, sFilter: str);
        }

        private void SearchData()
        {
            try
            {
                _EQPTID = cboEquipmentSegmentVDQA.SelectedValue.ToString();
                dgQAComplete.ItemsSource = null;
                dgRun.ItemsSource = null;

                DataTable data = new DataTable();
                data.Columns.Add("PROCID", typeof(string));
                data.Columns.Add("EQSGID", typeof(string));
                data.Columns.Add("LANGTYPE", typeof(string));

                DataRow row = data.NewRow();
                row["PROCID"] = Process.VD_LMN;
                row["EQSGID"] = _EQPTID;
                row["LANGTYPE"] = LoginInfo.LANGID;

                //if (cboEquipmentVDQA.SelectedValue.ToString().Equals(""))
                //{
                //    row["EQPTID"] = "";
                //}
                //else
                //{
                //    row["EQPTID"] = cboEquipmentVDQA.SelectedValue.ToString();
                //}

                data.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_STAT_VD_QA", "RQSTDT", "RSLTDT", data);
                Util.GridSetData(dgEquipment, result, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void clearGrid()
        {
            dgQAComplete.ItemsSource = null;
            dgEquipment.ItemsSource = null;
            dgRun.ItemsSource = null;
        }

        private void SetVDoperation(int index)
        {
            try
            {
                DataTable result = null;

                DataTable data = new DataTable();
                data.Columns.Add("LANGID", typeof(string));
                data.Columns.Add("EQPTID", typeof(string));
                data.Columns.Add("PROCID", typeof(string));
                data.Columns.Add("EQSGID", typeof(string));


                DataRow row = data.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQPTID"] = _EQPTID;
                row["PROCID"] = Process.VD_LMN;
                row["EQSGID"] = cboEquipmentSegmentVDQA.SelectedValue.ToString();
                data.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_EQPT_RUN_STAT_QA", "RQSTDT", "RSLTDT", data);


                if (result == null || result.Rows.Count <= 0)
                {
                    dgRun.ItemsSource = null;
                    return;
                }
                dgRun.ItemsSource = DataTableConverter.Convert(result);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setVDFinish(int index)
        {
            try
            {
                SetGridCboItem(dgQAComplete.Columns["JUDG_VALUE"]);

                DataTable result = null;

                DataTable data = new DataTable();
                
                data.Columns.Add("EQPTID", typeof(string));
                data.Columns.Add("PROCID", typeof(string));
                data.Columns.Add("WIPSTAT", typeof(string));                
                data.Columns.Add("JUDG_VALUE", typeof(string));
                data.Columns.Add("EQSGID", typeof(string));

                DataRow row = data.NewRow();
                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "EQPTID"));
                row["WIPSTAT"] = Wip_State.END;
                row["PROCID"] = Process.VD_LMN;
                row["JUDG_VALUE"] = Convert.ToString(cboInspection.SelectedValue).Equals("") ? null : Convert.ToString(cboInspection.SelectedValue);
                row["EQSGID"] = Util.GetCondition(cboEquipmentSegmentVDQA, "", true);
                data.Rows.Add(row);


                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_QA_JUDG_RST", "RQSTDT", "RSLTDT", data);//DA_PRD_SEL_WIP_QA_LOT_A


                Util.GridSetData(dgQAComplete, result, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID");
            dt.Columns.Add("CMCDTYPE");

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["CMCDTYPE"] = "QAJUDGE";
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_QMS", "RQSTDT", "RSLTDT", dt);
            if (result.Rows.Count == 0)
            {
                return;
            }
            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(result);
        }

        private void setLineCombo()
        {
            DataTable data = new DataTable();
            data.Columns.Add("LANGID", typeof(string));
            data.Columns.Add("PROCID", typeof(string));
            data.Columns.Add("AREA", typeof(string));

            DataRow row = data.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["PROCID"] = Process.LAMINATION;
            row["AREA"] = LoginInfo.CFG_AREA_ID;
            data.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_LAMI", "RQSTDT", "RSLTDT", data);
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                drawingGrids();

                if(dgQAComplete.GetRowCount()>0)
                {
                    for(int i = 0; i < dgQAComplete.GetRowCount();i++)
                    {
                        string sLotID = dgQAComplete.GetCell(i, dgQAComplete.Columns["LOTID"].Index).Text;
                        if (txtLotId.Text.Trim().ToUpper().Equals(sLotID))
                        {
                            DataTableConverter.SetValue(dgQAComplete.Rows[i].DataItem, "CHK2", true);
                            break;
                        }
                    }
                }
            }
        }

        private void drawingGrids()
        {
            clearGrid();
            SearchData();

            try
            {
                DataTable data = new DataTable();
                data.Columns.Add("PROCID", typeof(string));
                data.Columns.Add("EQSGID", typeof(string));
                data.Columns.Add("LOTID", typeof(string));

                DataRow row = data.NewRow();
                row["PROCID"] = Process.VD_LMN;
                row["EQSGID"] = Convert.ToString(cboEquipmentSegmentVDQA.SelectedValue);
                row["LOTID"] = txtLotId.Text;

                data.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_SEARCH_EQPTID_WHERE_LOTID", "RQSTDT", "RSLTDT", data);

                if (result.Rows.Count == 1)
                {
                    string eqptId = result.Rows[0]["EQPTID"].ToString();
                    int iCol = dgEquipment.Columns["EQPTID"].Index;

                    DataTable dt = ((DataView)dgEquipment.ItemsSource).Table;
                    int index = dt.Rows.IndexOf(dt.Select("EQPTID = '" + eqptId + "'").FirstOrDefault());

                    if (index >= 0)
                    {
                        DataTableConverter.SetValue(dgEquipment.Rows[index].DataItem, "CHK", true);

                        GetEqptStatInfo(index);
                    }
                }
            }
            catch (Exception ex)
            { }
        }


        private bool ValidateInsception()
        {
            try
            {
                if (dgQAComplete.ItemsSource == null)
                {
                    return false;
                }

                if (_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2") == -1)
                {
                    Util.Alert("SFU1632"); // 선택된 LOT이 없습니다.
                    return false;
                }

                DataTable dt = DataTableConverter.Convert(dgQAComplete.ItemsSource);
                if (dt.Rows.Count != 0)
                {
                    if (dt.Select("CHK2 = 1").Count() > 1)
                    {
                        Util.MessageValidation("SFU3364"); //샘플검사LOT 한개만 선택해주세요
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void btnInspection_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInsception())
                return;

            ASSY001_032_QAJUDG wndQA = new ASSY001_032_QAJUDG();
            wndQA.FrameOperation = FrameOperation;

            if (wndQA != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2")].DataItem, "LOTID"));
                Parameters[1] = "";//Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "JUDG_VALUE"));
                Parameters[2] = _EQPTID;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2")].DataItem, "LOTID_RT"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2")].DataItem, "EQPT_BTCH_WRK_NO"));
                Parameters[5] = _EQSGID;
                Parameters[6] = "VD_QA_INSP_RULE_01";
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2")].DataItem, "JUDG_SEQS"));


                C1WindowExtension.SetParameters(wndQA, Parameters);

                wndQA.Closed += new EventHandler(wndQA_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQA.ShowModal()));
                wndQA.BringToFront();
            }

        }
        private void wndQA_Closed(object sender, EventArgs e)
        {
            ASSY001_032_QAJUDG window = sender as ASSY001_032_QAJUDG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                drawingGrids();
            }
        }
    }
}
