/*************************************************************************************
 Created Date : 2021.10.27
      Creator : 강호운
   Decription : 포장기 반송 조건 시간대별 반송 가능 수량 조회 및 MCS 반송 환경 설정
--------------------------------------------------------------------------------------
 [Change History]
 2021.10.27    강호운   신규개발
 2022.04.05    강호운   물류포장유형 추가 트럭킹수량, 충방전 수량 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;


namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_028 : UserControl, IWorkArea
    {
        private DataTable isCreateTable = new DataTable();
        System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        public PACK003_028()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void setTerm()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                for (int i = 0; i < 12; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["CBO_NAME"] = (i + 1) + " : " + (i + 1) + ObjectDic.Instance.GetObjectName("시간");
                    dr["CBO_CODE"] = (i + 1); 
                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();

                cboTerm.ItemsSource = DataTableConverter.Convert(dt);
                cboTerm.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setTime()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                for (int i = 0; i < 10; i++)
                {
                    DataRow dr = dt.NewRow();

                    dr["CBO_NAME"] = (i + 1) + " : " + (i + 1) + ObjectDic.Instance.GetObjectName("분");
                    dr["CBO_CODE"] = (i + 1);
                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();

                cboTime.ItemsSource = DataTableConverter.Convert(dt);
                cboTime.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                setTerm();
                setTime();
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Search();
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Search()
        {
            try
            {
                //ShowLoadingIndicator();

                ShowLoadingIndicator();
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));

                Util.gridClear(dgLineTabSearch);

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("TERM", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TERM"] = cboTerm.SelectedValue;

                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);


                DataSet dsResult = null;
                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_LOGIS_EQPT_PROD_STOCK", "INDATA", "OUTDATA", dsInput, null);
                
                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    if ((dsResult.Tables.IndexOf("OUTDATA") > -1) && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        Util.GridSetData(dgLineTabSearch, dsResult.Tables["OUTDATA"], FrameOperation, false);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgLineTabSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgLineTabSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void chkTime_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if ((bool)cb.IsChecked)
            {
                string timevalue = cboTime.SelectedValue.ToString();
                timer1.Interval = 1000 * timevalue.SafeToInt32();
                // timer event 등록
                timer1.Tick += new EventHandler(timer1_Tick);
                timer1.Start();
            }else
            {
                timer1.Stop();
            }
        }

        private void txtTRF_GROUP_QTY_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                ShowLoadingIndicator();
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));

                string bizRuleName = "DA_MCS_UPD_TB_MMD_TRF_GROUP_GROUP_QTY";

                isCreateTable = DataTableConverter.Convert(dgLineTabSearch.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgLineTabSearch)) return;

                this.dgLineTabSearch.EndEdit();
                this.dgLineTabSearch.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("TRF_GROUPID", typeof(string));
                inDataTable.Columns.Add("PACK_EQPTID", typeof(string));
                inDataTable.Columns.Add("TRF_GROUP_QTY", typeof(string));
                inDataTable.Columns.Add("REQ_MAX_QTY", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                foreach (object modified in dgLineTabSearch.GetModifiedItems())
                {
                    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();
                        param["TRF_GROUP_QTY"] = DataTableConverter.GetValue(modified, "TRF_GROUP_QTY");
                        param["PACK_EQPTID"] = DataTableConverter.GetValue(modified, "PACK_EQPTID");
                        param["TRF_GROUPID"] = DataTableConverter.GetValue(modified, "TRF_GROUPID");
                        param["REQ_MAX_QTY"] = DataTableConverter.GetValue(modified, "REQ_MAX_QTY");
                        param["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(param);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, indataSet);
                Util.MessageInfo("SFU2056", inDataTable.Rows.Count);
                Util.gridClear(dgLineTabSearch);

                inDataTable = new DataTable();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            Search();

        }


        private void cboGr_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }



        public Boolean isNumber(string value)
        {
            Boolean isNumber = true;
            for (int i = 0; i < value.Length; i++)
            {
                char val = Convert.ToChar(value[i]);
                if (!(char.IsDigit(val)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
                {
                    isNumber = false;
                }
            }
            if (!isNumber)
            {
                Util.MessageValidation("SFU3465");
                //e.Cancel = true;
            }
            return isNumber;
        }

        private void dgLineTabSearch_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].SafeToString() != "True" && e.Column != dgLineTabSearch.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }

            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgLineTabSearch.Columns["CHK"]
                 &&  e.Column != this.dgLineTabSearch.Columns["REQ_MAX_QTY"]
                 && e.Column != this.dgLineTabSearch.Columns["TRF_GROUP_QTY"]
                 )
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }
        private void dgLineTabSearch_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (!dg.CurrentCell.IsEditing)
            {
                switch (dg.CurrentCell.Column.Name)
                {
                    case "REQ_MAX_QTY":
                        isNumber(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    case "TRF_GROUP_QTY":
                        isNumber(Convert.ToString(dg.CurrentCell.Value));
                        break;
                    default:
                        break;
                }
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgLineTabSearch.Rows)
            {
                if (true)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "Y");
                }
            }
            dgLineTabSearch.EndEdit();
            dgLineTabSearch.EndEditRow(true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgLineTabSearch.Rows)
            {
                if (true)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "N");
                }
            }
            dgLineTabSearch.EndEdit();
            dgLineTabSearch.EndEditRow(true);
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (FrameOperation.AUTHORITY.ToString().Equals("R"))
            {
                this.dgLineTabSearch.Columns["CHK"].Visibility = Visibility.Collapsed;
            }
        }
    }
}