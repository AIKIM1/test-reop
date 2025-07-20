/*************************************************************************************
 Created Date : 2020.12.01
      Creator : 김대근
   Decription : 실시간 수율 현황(진행 중 LOT 기준)
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.22  김대근 : Initial Created.  
  2023.06.08  안유수 E20230504-001200 실시간 금형 상태 조회 TOOL_MST 기준정보에서, 현재 MOUNT 된 TOOL 기준으로 조회하도록 조건 변경
  2023.12.20  박성진 E20231211-000182 TOOL마스터 TOOL_TYPE_CODE 표준화에 따른 전극유형코드 변경
  2024.07.03  안유수 E20240424-001166 음영 표시 기능 내 설비 투입위치 ID 하드코딩 제거, 노칭, 패키징공정도 설비 투입위치 ID를 기준으로 Tool 정보 조회하도록 수정,
                                      투입 위치 컬럼 용어 변경 기능 제거, 설비 ID 기준으로 오름차순 정렬 되도록 DA_PRD_SEL_UBM_NOW_COUNT DA 수정
  **************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_348 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        private string[] _posid = null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_348()
        {
            InitializeComponent();
        }





        #endregion

        #region Initialize
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetCboArea();
            SetElectrodeTypeCombo();
            TimerSetting();
            _isLoaded = true;
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        #region Event
        private void btnSearchToolStatus_Click(object sender, RoutedEventArgs e)
        {
            if (cboArea.SelectedValue.Equals("SELECT") || cboProcess.SelectedValue.Equals("SELECT"))
            {
                //선택 조건을 하나 이상 선택하세요.
                Util.MessageValidation("SFU1262");
                return;
            }
            InitDataGridColumns();
            SetToolStatus();
            SetToolMDM();

        }

        private void btnSaveToolMDM_Click(object sender, RoutedEventArgs e)
        {
            SaveToolMDM();
        }

        private void dgToolMDM_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
        }

        private void dgToolMDM_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetCboProc();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.SelectedValue.ToString() == "A5000")
            {
                GrdElecType.Visibility = Visibility.Visible;
            }
            else
            {
                GrdElecType.Visibility = Visibility.Collapsed;
            }

            SetEquipmentSegment();
            SetEquipment();
        }

        #endregion

        #region Method
        private void SetToolStatus()
        {
            try
            {
                DataTable indata = new DataTable();
                indata.TableName = "INDATA";
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("PROCID", typeof(string));
                indata.Columns.Add("EQSGID", typeof(string));
                indata.Columns.Add("EQPTID", typeof(string));
                indata.Columns.Add("ELTR_TYPE", typeof(string));

                DataRow drIndata = indata.NewRow();
                drIndata["LANGID"] = LoginInfo.LANGID;
                //cbo로 변경
                drIndata["AREAID"] = cboArea.SelectedValue;
                drIndata["PROCID"] = cboProcess.SelectedValue;

                if (!string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.ToString()))
                {
                    drIndata["EQSGID"] = cboEquipmentSegment.SelectedValue;
                }

                if (!string.IsNullOrEmpty(cboEquipment.SelectedValue.ToString()))
                {
                    drIndata["EQPTID"] = cboEquipment.SelectedValue;
                }
                if (cboProcess.SelectedValue.ToString() == "A5000")
                {
                    drIndata["ELTR_TYPE"] = cboElecType.SelectedIndex == 0 ? null : cboElecType.SelectedValue.ToString();
                }
                else
                {
                    drIndata["ELTR_TYPE"] = null;
                }
                indata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_UBM_NOW_COUNT", "INDATA", "OUTDATA", indata, (bizResult, exception) =>
                {
                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        DataTable dtPivot = GetPivotToolStatus(bizResult);
                        _posid = Array.ConvertAll(bizResult.Select().Select(x => x["POSID"]).ToArray(), p => (p ?? String.Empty).ToString()).Distinct().ToArray();

                        foreach (DataColumn col in dtPivot.Columns)
                        {
                            if (col.Caption.Equals("ELTR_TYPE"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ELTR_TYPE";
                                dgCol.Visibility = Visibility.Collapsed;
                                dgToolStatus.Columns.Add(dgCol);

                            }
                            if (col.Caption.Equals("POSID"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ADD_COL";
                                dgToolStatus.Columns.Add(dgCol);

                            }
                            if (col.Caption.Equals("POSID_DIFF"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ADD_COL_DIFF";
                                dgCol.Visibility = Visibility.Collapsed;
                                dgToolStatus.Columns.Add(dgCol);

                            }
                        }

                        foreach (DataColumn col in dtPivot.Columns)
                        {
                            if (col.Caption.Equals("ELTR_TYPE"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ELTR_TYPE";
                                dgCol.Visibility = Visibility.Collapsed;
                                dgToolStatus1.Columns.Add(dgCol);
                            }
                            if (col.Caption.Equals("POSID"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ADD_COL";
                                dgToolStatus1.Columns.Add(dgCol);
                            }
                            if (col.Caption.Equals("POSID_DIFF"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ADD_COL_DIFF";
                                dgCol.Visibility = Visibility.Collapsed;
                                dgToolStatus1.Columns.Add(dgCol);
                            }
                        }
                        foreach (DataColumn col in dtPivot.Columns)
                        {
                            if (col.Caption.Equals("ELTR_TYPE"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ELTR_TYPE";
                                dgCol.Visibility = Visibility.Collapsed;
                                dgToolStatus2.Columns.Add(dgCol);
                            }
                            if (col.Caption.Equals("POSID"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ADD_COL";
                                dgToolStatus2.Columns.Add(dgCol);
                            }
                            if (col.Caption.Equals("POSID_DIFF"))
                            {
                                C1.WPF.DataGrid.DataGridTextColumn dgCol = new C1.WPF.DataGrid.DataGridTextColumn();
                                dgCol.Header = "[*]" + col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Right;
                                dgCol.Tag = "ADD_COL_DIFF";
                                dgCol.Visibility = Visibility.Collapsed;
                                dgToolStatus2.Columns.Add(dgCol);
                            }
                        }

                        DataRow[] drTable1 = dtPivot.Select("ROWNUM <= 26");
                        DataTable dtPivot_1 = dtPivot.Clone();

                        for (int row = 0; row < drTable1.Length; row++)
                        {
                            dtPivot_1.ImportRow(drTable1[row]);
                        }
                        dtPivot_1.AcceptChanges();
                        Util.GridSetData(dgToolStatus, dtPivot_1, this.FrameOperation, true);


                        DataRow[] drTable2 = dtPivot.Select("ROWNUM > 26 AND ROWNUM <= 52");
                        DataTable dtPivot_2 = dtPivot.Clone();

                        for (int row = 0; row < drTable2.Length; row++)
                        {
                            dtPivot_2.ImportRow(drTable2[row]);
                        }
                        dtPivot_2.AcceptChanges();
                        Util.GridSetData(dgToolStatus1, dtPivot_2, this.FrameOperation, true);

                        DataRow[] drTable3 = dtPivot.Select("ROWNUM > 52 AND ROWNUM <= '" + dtPivot.Rows.Count + "'");
                        DataTable dtPivot_3 = dtPivot.Clone();

                        for (int row = 0; row < drTable3.Length; row++)
                        {
                            dtPivot_3.ImportRow(drTable3[row]);
                        }
                        dtPivot_3.AcceptChanges();
                        Util.GridSetData(dgToolStatus2, dtPivot_3, this.FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetToolMDM()
        {
            try
            {
                DataTable indata = new DataTable();
                indata.TableName = "INDATA";
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("PROCID", typeof(string));

                DataRow drIndata = indata.NewRow();
                drIndata["AREAID"] = cboArea.SelectedValue;
                drIndata["PROCID"] = cboProcess.SelectedValue;
                indata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_UBM_TOOL_DTT_QTY", "INDATA", "OUTDATA", indata, (bizResult, exception) =>
                {
                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        DataTable dtPivot = GetPivotToolMDM(bizResult);
                        foreach (DataColumn col in dtPivot.Columns)
                        {
                            if (col.Caption.Equals("COM_CODE"))
                            {
                                C1.WPF.DataGrid.DataGridNumericColumn dgCol = new C1.WPF.DataGrid.DataGridNumericColumn();
                                dgCol.Header = col.ColumnName;
                                dgCol.Binding = new Binding(col.ColumnName);
                                dgCol.HorizontalAlignment = HorizontalAlignment.Center;
                                dgCol.MinWidth = 100;
                                dgCol.Tag = "ADD_COL";
                                dgToolMDM.Columns.Add(dgCol);
                            }
                        }

                        Util.GridSetData(dgToolMDM, dtPivot, this.FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveToolMDM()
        {
            try
            {
                DataTable indata = new DataTable();
                indata.TableName = "INDATA";
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("PROCID", typeof(string));
                indata.Columns.Add("COM_CODE", typeof(string));
                indata.Columns.Add("QTY_HD", typeof(decimal));
                indata.Columns.Add("QTY_FD", typeof(decimal));

                DataTable dt = DataTableConverter.Convert(dgToolMDM.ItemsSource);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Columns[i].Caption == null || !dt.Columns[i].Caption.Equals("COM_CODE"))
                    {
                        continue;
                    }

                    DataRow drIndata = indata.NewRow();
                    drIndata["AREAID"] = cboArea.SelectedValue;
                    drIndata["PROCID"] = cboProcess.SelectedValue;
                    drIndata["COM_CODE"] = dt.Columns[i].ColumnName;
                    drIndata["QTY_HD"] = Util.NVC_Decimal(dt.DefaultView[0][i]);
                    drIndata["QTY_FD"] = Util.NVC_Decimal(dt.DefaultView[1][i]);
                    indata.Rows.Add(drIndata);
                }

                new ClientProxy().ExecuteService("DA_PRD_UPD_UBM_TOOL_DTT_QTY", "INDATA", "OUTDATA", indata, (bizResult, exception) =>
                {
                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        while (dgToolMDM.Columns.Count > 1)
                        {
                            dgToolMDM.Columns.RemoveAt(1);
                        }
                        InitDataGridColumns();
                        SetToolStatus();
                        SetToolMDM();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetPivotToolStatus(DataTable src)
        {
            //1. 새로 구성할 테이블의 칼럼을 정한다.
            //2. 다시 한 번 테이블을 돌면서 값을 넣는다.
            DataTable result = new DataTable();
            result.Columns.Add("ROWNUM", typeof(int));
            result.Columns.Add("EQPTID", typeof(string));
            result.Columns.Add("EQPTNAME", typeof(string));
            result.Columns.Add("PROCID", typeof(string));
            //result.Columns.Add("ELTR_TYPE", typeof(string));
            result.Columns.Add("LIMIT_USE_COUNT", typeof(decimal));
            DataTable dtDistinct = src.DefaultView.ToTable(true, new string[] { "POSID" });
            foreach (DataRowView drvDistinct in dtDistinct.DefaultView)
            {
                string colNew = drvDistinct["POSID"].ToString();
                result.Columns.Add(colNew, typeof(string));
                result.Columns[colNew].DefaultValue = "N/A";
                result.Columns[colNew].Caption = "POSID";

                string colNew_DIFF = "DIFF_LIM_ACC_" + drvDistinct["POSID"].ToString();
                result.Columns.Add(colNew_DIFF, typeof(string));
                result.Columns[colNew_DIFF].DefaultValue = "N/A";
                result.Columns[colNew_DIFF].Caption = "POSID_DIFF";
            }
            string colNew_Elec = "ELTR_TYPE";
            result.Columns.Add(colNew_Elec, typeof(string));
            result.Columns[colNew_Elec].DefaultValue = "N/A";
            result.Columns[colNew_Elec].Caption = "ELTR_TYPE";

            //값 넣기
            foreach (DataRowView drvSrc in src.DefaultView)
            {
                string idSrc = drvSrc["EQPTID"].ToString();
                string colSrc = drvSrc["POSID"].ToString();
                string countSrc = drvSrc["ACCU_USE_COUNT"].ToString();

                string countDiff = drvSrc["DIFF_LIM_ACC"].ToString();
                string colDiffSrc = "DIFF_LIM_ACC_" + drvSrc["POSID"].ToString();


                //string countElec = drvSrc["ELTR_TYPE"].ToString();
                //string colElecSrc = "ELTR_TYPE";

                int idIdx = -1;
                for (int i = 0; i < result.DefaultView.Count; i++)
                {
                    if (result.DefaultView[i]["EQPTID"].ToString().Equals(idSrc))
                    {
                        idIdx = i;
                        break;
                    }
                }

                if (idIdx >= 0)
                {
                    if (!string.IsNullOrEmpty(countSrc))
                    {
                        result.Rows[idIdx][colSrc] = countSrc;
                        result.Rows[idIdx][colDiffSrc] = countDiff;

                    }
                }
                else
                {
                    DataRow dr = result.NewRow();
                    dr["EQPTID"] = idSrc;
                    dr["EQPTNAME"] = drvSrc["EQPTNAME"].ToString();
                    dr["PROCID"] = drvSrc["PROCID"].ToString();
                    dr["ELTR_TYPE"] = drvSrc["ELTR_TYPE"].ToString();
                    dr["LIMIT_USE_COUNT"] = Util.NVC_Decimal(drvSrc["LIMIT_USE_COUNT"]);

                    if (!string.IsNullOrEmpty(countSrc))
                    {
                        dr[colSrc] = countSrc;
                        dr[colDiffSrc] = countDiff;
                        //dr[colElecSrc] = countElec;
                    }
                    result.Rows.Add(dr);
                }

            }
            int _RowNum = 1;
            for (int i = 0; i < result.Rows.Count; i++)
            {

                result.Rows[i]["ROWNUM"] = _RowNum;
                _RowNum = _RowNum + 1;
            }

            result.AcceptChanges();
            return result;
        }

        private DataTable GetPivotToolMDM(DataTable src)
        {
            //1. 새로 구성할 테이블의 칼럼을 정한다.
            //2. 다시 한 번 테이블을 돌면서 값을 넣는다.
            DataTable result = new DataTable();
            result.Columns.Add("TIME", typeof(string));

            DataTable dtDistinct = src.DefaultView.ToTable(true, new string[] { "COM_CODE" });
            foreach (DataRowView drvDistinct in dtDistinct.DefaultView)
            {
                string colNew = drvDistinct["COM_CODE"].ToString();
                result.Columns.Add(colNew, typeof(decimal));
                result.Columns[colNew].Caption = "COM_CODE";
            }

            foreach (DataColumn colSrc in src.Columns)
            {
                if (colSrc.ColumnName.Equals("COM_CODE"))
                {
                    continue;
                }

                DataRow dr = result.NewRow();
                dr["TIME"] = colSrc.ColumnName;
                foreach (DataRowView drvSrc in src.DefaultView)
                {
                    dr[drvSrc["COM_CODE"].ToString()] = Util.NVC_Decimal(drvSrc[colSrc.ColumnName]);
                }
                result.Rows.Add(dr);
            }

            result.AcceptChanges();
            return result;
        }

        private string GetAreaTypeCode()
        {
            DataTable indata = new DataTable();
            indata.TableName = "RQSTDT";
            indata.Columns.Add("AREAID", typeof(string));

            DataRow drIndata = indata.NewRow();
            drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
            indata.Rows.Add(drIndata);

            DataTable areaType = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", indata);

            return areaType.DefaultView[0]["AREA_TYPE_CODE"].ToString();
        }

        private void SetCboArea()
        {
            try
            {
                DataTable indata = new DataTable();
                indata.TableName = "RQSTDT";
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("SHOPID", typeof(string));
                indata.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drIndata = indata.NewRow();
                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drIndata["AREA_TYPE_CODE"] = GetAreaTypeCode();
                indata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", indata, (bizResult, exception) =>
                {
                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        DataRow dr = bizResult.NewRow();
                        dr["CBO_CODE"] = "SELECT";
                        dr["CBO_NAME"] = "-SELECT-";
                        bizResult.Rows.InsertAt(dr, 0);

                        cboArea.ItemsSource = bizResult.DefaultView;
                        cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
                        SetCboProc();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetElectrodeTypeCombo()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "TOOL_ELTR_TYPE " };
            string selectedValueText = cboElecType.SelectedValuePath;
            string displayMemberText = cboElecType.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboElecType, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }


        private void SetCboProc()
        {
            try
            {
                DataTable indata = new DataTable();
                indata.TableName = "RQSTDT";
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));

                DataRow drIndata = indata.NewRow();
                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["AREAID"] = cboArea.SelectedValue;
                indata.Rows.Add(drIndata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_TOOL_BY_AREAID", "RQSTDT", "RSLTDT", indata, (bizResult, exception) =>
                {
                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        DataRow dr = bizResult.NewRow();
                        dr["CBO_CODE"] = "SELECT";
                        dr["CBO_NAME"] = "-SELECT-";
                        bizResult.Rows.InsertAt(dr, 0);

                        cboProcess.ItemsSource = bizResult.DefaultView;
                        cboProcess.SelectedIndex = bizResult.Rows.Count == 1 ? 0 : 1;

                        SetEquipmentSegment();
                        SetEquipment();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private void SetEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drIndata = RQSTDT.NewRow();
                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["AREAID"] = cboArea.SelectedValue.ToString();
                drIndata["PROCID"] = cboProcess.SelectedValue.ToString();
                drIndata["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                RQSTDT.Rows.Add(drIndata);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO_MOLD", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow dr = dtResult.NewRow();
                dr["CBO_CODE"] = "";
                dr["CBO_NAME"] = "-ALL-";
                dtResult.Rows.InsertAt(dr, 0);

                cboEquipment.ItemsSource = dtResult.DefaultView;
                cboEquipment.SelectedIndex = 0;


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetEquipmentSegment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drIndata = RQSTDT.NewRow();
                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["AREAID"] = cboArea.SelectedValue.ToString();
                drIndata["PROCID"] = cboProcess.SelectedValue.ToString();

                RQSTDT.Rows.Add(drIndata);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_PROCID_CBO_MOLD", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

                DataRow dr = dtResult.NewRow();
                dr["CBO_CODE"] = "";
                dr["CBO_NAME"] = "-ALL-";
                dtResult.Rows.InsertAt(dr, 0);

                cboEquipmentSegment.ItemsSource = dtResult.DefaultView;
                cboEquipmentSegment.SelectedIndex = 0;


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void InitDataGridColumns()
        {
            while (dgToolStatus.Columns.Count > 1)
            {
                dgToolStatus.Columns.RemoveAt(1);
            }
            while (dgToolStatus1.Columns.Count > 1)
            {
                dgToolStatus1.Columns.RemoveAt(1);
            }
            while (dgToolStatus2.Columns.Count > 1)
            {
                dgToolStatus2.Columns.RemoveAt(1);
            }

            while (dgToolMDM.Columns.Count > 1)
            {
                dgToolMDM.Columns.RemoveAt(1);
            }

            if (dgToolMDM.Rows.Count > 1)
            {
                dgToolMDM.ClearRows();
            }
        }

        #endregion

        private void dgToolStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (Array.Exists(_posid, a => a.Equals(e.Cell.Column.Name)))
                {
                    if (cboProcess.SelectedValue.ToString() == "A5000")
                    {
                        if (dgToolMDM.Rows.Count > 0)
                        {
                            decimal Anode_HD = Convert.ToDecimal(DataTableConverter.GetValue(dgToolMDM.Rows[0].DataItem, "A").ToString().Replace(",", ""));
                            decimal Anode_FD = Convert.ToDecimal(DataTableConverter.GetValue(dgToolMDM.Rows[1].DataItem, "A").ToString().Replace(",", ""));
                            decimal Cathode_HD = Convert.ToDecimal(DataTableConverter.GetValue(dgToolMDM.Rows[0].DataItem, "C").ToString().Replace(",", ""));
                            decimal Cathode_FD = Convert.ToDecimal(DataTableConverter.GetValue(dgToolMDM.Rows[1].DataItem, "C").ToString().Replace(",", ""));


                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE").ToString() == "A")
                            {
                                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name).ToString() != "N/A")
                                {
                                    if (Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) <= Anode_HD)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    }
                                    else if (Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) > Anode_HD && Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) <= Anode_FD)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                    }
                                }
                            }
                            else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE").ToString() == "C")
                            {
                                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name).ToString() != "N/A")
                                {
                                    if (Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) <= Cathode_HD)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    }
                                    else if (Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) > Cathode_HD && Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) <= Cathode_FD)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                    }
                                }
                            }
                        }
                    }
                    else if (cboProcess.SelectedValue.ToString() == "A7000")
                    {
                        if (dgToolMDM.Rows.Count > 0)
                        {
                            decimal Cutter_HD = Convert.ToDecimal(DataTableConverter.GetValue(dgToolMDM.Rows[0].DataItem, "CUTTER").ToString().Replace(",", ""));
                            decimal Cutter_FD = Convert.ToDecimal(DataTableConverter.GetValue(dgToolMDM.Rows[1].DataItem, "CUTTER").ToString().Replace(",", ""));

                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name).ToString() != "N/A")
                            {
                                if (Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) <= Cutter_HD)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                                else if (Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) > Cutter_HD && Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_LIM_ACC_" + e.Cell.Column.Name)) <= Cutter_FD)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                            }
                        }
                    }
                }
            }));
        }

        private void dgToolStatus_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 1;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                _monitorTimer.Start();

            }
        }
        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    if (cboArea.SelectedValue.Equals("SELECT") || cboProcess.SelectedValue.Equals("SELECT"))
                    {
                        //선택 조건을 하나 이상 선택하세요.
                        Util.MessageValidation("SFU1262");
                        return;
                    }
                    InitDataGridColumns();
                    SetToolStatus();
                    SetToolMDM();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipment();
        }
    }
}
