/*************************************************************************************
 Created Date : 2019.06.24
      Creator : 이영준
   Decription : 전지 5MEGA-GMES 구축 - 코터 공정진척 화면 - 전수 불량 레인 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.06.24  이영준 : Initial Created.
  
**************************************************************************************/


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
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_REG_DFCT_LANE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_REG_DFCT_LANE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LOTID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _PROCID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _MODLID = string.Empty;
        private decimal _ACTQTY = 0;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_REG_DFCT_LANE()
        {
            InitializeComponent();
        } 
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    _LOTID = Util.NVC(tmps[0]);
                    _EQSGID = Util.NVC(tmps[1]);
                    _PROCID = Util.NVC(tmps[2]);
                    _EQPTID = Util.NVC(tmps[3]);
                    _MODLID = Util.NVC(tmps[4]);
                    _ACTQTY = Util.NVC_Decimal(tmps[5]);

                }
                ApplyPermissions();

                getDefectLaneLotList();
                getBackDefect();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region Mehod

        #region [BizCall]        
        private void getDefectLaneLotList()
        {
            try
            {
                DataTable _DefectLane = new DataTable();

                Util.gridClear(dgLaneDfct);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MODLID", typeof(string));
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = _LOTID;
                Indata["EQSGID"] = _EQSGID;
                Indata["PROCID"] = _PROCID;
                Indata["EQPTID"] = _EQPTID;
                Indata["MODLID"] = _MODLID;

                IndataTable.Rows.Add(Indata);

                _DefectLane = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_DFCT_LANE", "INDATA", "RSLTDT", IndataTable);

                if (_DefectLane != null && _DefectLane.Rows.Count > 0)
                {
                    Util.GridSetData(dgLaneDfct, _DefectLane, FrameOperation);
                    getLaneStatus(_DefectLane);
                    dgLaneDfct.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void getBackDefect()
        {
            try
            {
                Util.gridClear(dgDefect);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("RESNPOSITION", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dataRow["LOTID"] = _LOTID;
                dataRow["ACTID"] = "DEFECT_LOT";
                dataRow["RESNPOSITION"] = "DEFECT_BACK";

                dt.Rows.Add(dataRow);

                //C20210222-000365 불량/Loss항목 표준화 적용 DA_PRD_SEL_ACTIVITYREASON_ELEC -> BR_PRD_SEL_ACTIVITYREASON_ELEC 변경
                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", dt);

                DataColumn nColumn = new DataColumn("CHK", typeof(System.String));
                nColumn.DefaultValue = "0";
                result.Columns.Add(nColumn);


                Util.GridSetData(dgDefect, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        #endregion

        #region [Validation]
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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
        private void getLaneStatus(DataTable dt)
        {
            int LaneCount = 0;
            int DefectLaneCount = 0;

            DataRow[] trows = dt.Select("CHK = 'True'");
            LaneCount = Util.NVC_Int(dt.Rows.Count);
            DefectLaneCount = Util.NVC_Int(trows.Length);

            txtDfctLaneQty.Text = Util.NVC(DefectLaneCount);
            txtGoodLaneQty.Text = Util.NVC(LaneCount - DefectLaneCount);
            txtPhysicalLaneQty.Text = Util.NVC(LaneCount);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getDefectLaneLotList();
            getBackDefect();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] DefectChecks = Util.gridGetChecked(ref dgLaneDfct, "CHK");

            if (DefectChecks.Length == dgLaneDfct.Rows.Count)
            {
                Util.MessageValidation("SFU7027");  //전체 Lane을 불량처리 할 수 없습니다.
                return;
            }
            saveDefectLane();
        }
        
        private void saveDefectLane()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dt = DataTableConverter.Convert(dgLaneDfct.ItemsSource);
                        getLaneStatus(dt);

                        string _DELFLAG = string.Empty;

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("CURR_LANE_QTY", typeof(decimal));

                        DataRow inDataRow = null;
                        inDataRow = inData.NewRow();
                        inDataRow["LOTID"] = _LOTID;
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataRow["CURR_LANE_QTY"] = Util.NVC_Decimal(txtGoodLaneQty.Text);
                        inData.Rows.Add(inDataRow);

                        DataTable IndataDetailTable = indataSet.Tables.Add("INRESN");
                        IndataDetailTable.Columns.Add("LANE_NO", typeof(string));
                        IndataDetailTable.Columns.Add("RESNCODE", typeof(string));
                        IndataDetailTable.Columns.Add("RESNQTY", typeof(decimal));
                        IndataDetailTable.Columns.Add("RESNQTY2", typeof(decimal));
                        IndataDetailTable.Columns.Add("ACTUSER", typeof(string));
                        IndataDetailTable.Columns.Add("DEL_FLAG", typeof(string));

                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Util.NVC(inRow["RESNCODE_CHK"]).Equals("INS") && Convert.ToBoolean(inRow["CHK"]) == true)
                            {
                                DataRow inDetailDataRow = null;
                                inDetailDataRow = IndataDetailTable.NewRow();

                                inDetailDataRow["LANE_NO"] = Util.NVC(inRow["LANE_NO"]);
                                inDetailDataRow["RESNCODE"] = Util.NVC(inRow["RESNCODE"]);
                                inDetailDataRow["RESNQTY"] = _ACTQTY;
                                inDetailDataRow["RESNQTY2"] = _ACTQTY;
                                inDetailDataRow["ACTUSER"] = LoginInfo.USERID;
                                inDetailDataRow["DEL_FLAG"] = "N";
                                IndataDetailTable.Rows.Add(inDetailDataRow);
                            }
                            else if (Util.NVC(inRow["RESNCODE_CHK"]).Equals("UPD"))
                            {
                                DataRow inDetailDataRow = null;
                                inDetailDataRow = IndataDetailTable.NewRow();

                                inDetailDataRow["LANE_NO"] = Util.NVC(inRow["LANE_NO"]);
                                inDetailDataRow["RESNCODE"] = Util.NVC(inRow["RESNCODE"]);
                                inDetailDataRow["RESNQTY"] = _ACTQTY;
                                inDetailDataRow["RESNQTY2"] = _ACTQTY;
                                inDetailDataRow["ACTUSER"] = LoginInfo.USERID;
                                if (Util.NVC(inRow["RESNCODE"]).Equals("") && Util.NVC(inRow["RESNCODE_CHK"]).Equals("UPD"))
                                    _DELFLAG = "Y";
                                else
                                    _DELFLAG = "N";
                                inDetailDataRow["DEL_FLAG"] = _DELFLAG;
                                IndataDetailTable.Rows.Add(inDetailDataRow);
                            }
                        }
                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DEFECT_LANE_COATER", "INDATA,INRESN", null, indataSet);

                        Util.AlertInfo("SFU1270");  //저장되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgLaneDfct.ItemsSource == null || dgLaneDfct.Rows.Count < 0)
                return;

            DataRow[] DefectChecks = Util.gridGetChecked(ref dgDefect, "CHK");
            if (DefectChecks == null || DefectChecks.Length == 0)
            {
                Util.MessageValidation("SFU3643");  //불량/Loss 항목을 선택하세요.
                return;
            }

            int iRow = dgLaneDfct.SelectedIndex; 

            if (!Util.NVC(DataTableConverter.GetValue(dgLaneDfct.Rows[iRow].DataItem, "RESNCODE")).Equals(""))
            {
                // 변경하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2875"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTableConverter.SetValue(dgLaneDfct.Rows[dgLaneDfct.CurrentRow.Index].DataItem, "CHK", true);
                        DataTableConverter.SetValue(dgLaneDfct.Rows[dgLaneDfct.CurrentRow.Index].DataItem, "ACTID", Util.NVC(DefectChecks[0]["ACTID"]));
                        DataTableConverter.SetValue(dgLaneDfct.Rows[dgLaneDfct.CurrentRow.Index].DataItem, "RESNCODE", Util.NVC(DefectChecks[0]["RESNCODE"]));
                        DataTableConverter.SetValue(dgLaneDfct.Rows[dgLaneDfct.CurrentRow.Index].DataItem, "RESNNAME", Util.NVC(DefectChecks[0]["RESNNAME"]));
                    }
                });
            }
            else
            {
                DataTableConverter.SetValue(dgLaneDfct.Rows[dgLaneDfct.CurrentRow.Index].DataItem, "CHK", true);
                DataTableConverter.SetValue(dgLaneDfct.Rows[dgLaneDfct.CurrentRow.Index].DataItem, "ACTID", Util.NVC(DefectChecks[0]["ACTID"]));
                DataTableConverter.SetValue(dgLaneDfct.Rows[dgLaneDfct.CurrentRow.Index].DataItem, "RESNCODE", Util.NVC(DefectChecks[0]["RESNCODE"]));
                DataTableConverter.SetValue(dgLaneDfct.Rows[dgLaneDfct.CurrentRow.Index].DataItem, "RESNNAME", Util.NVC(DefectChecks[0]["RESNNAME"]));
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgLaneDfct.ItemsSource == null || dgLaneDfct.Rows.Count < 0)
                return;

            int iRow = dgLaneDfct.SelectedIndex; //((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

            if (Util.NVC(DataTableConverter.GetValue(dgLaneDfct.Rows[iRow].DataItem, "RESNCODE")).Equals(""))
            {
                Util.MessageValidation("SFU4986");  //선택된 Lane이 없습니다.
                return;
            }

            //취소하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataTableConverter.SetValue(dgLaneDfct.Rows[iRow].DataItem, "CHK", false);
                    DataTableConverter.SetValue(dgLaneDfct.Rows[iRow].DataItem, "RESNCODE", string.Empty);
                    DataTableConverter.SetValue(dgLaneDfct.Rows[iRow].DataItem, "RESNNAME", string.Empty);
                }
            });
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, 1);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, 0);
                }
                rb.IsChecked = true;
                dg.SelectedIndex = idx;
            }
        }

        #endregion

        #endregion

    }
}
