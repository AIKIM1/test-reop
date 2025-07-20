/*************************************************************************************
 Created Date : 2023.12.19
      Creator : 손동혁
   Decription : Lead Cutting Tool 관리 (ESNA 법인 별도 화면)
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.19  손동혁 : Initial Created.  
  2024.01.21  손동혁 : 연마 버튼 시 사용중인 도구에 대하여 인터락 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{

    public partial class FCS001_168 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();


        public FCS001_168()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public enum ComboStatus
        {
            /// <summary>
            /// 콤보 바인딩 후 ALL 을 최상단에 표시
            /// </summary>
            ALL,

            /// <summary>
            /// 콤보 바인딩 후 Select 을 최상단에 표시 (필수선택 항목에 사용)
            /// </summary>
            SELECT,

            /// <summary>
            /// 바인딩 후 선택 안해도 될경우(선택 안해도 되는 콤보일때 사용)
            /// </summary>
            NA,

            /// <summary>
            /// 바인딩만 하고 끝 (바인딩후 제일 1번째 항목을 표시) 
            /// </summary>
            NONE,
            /// <summary>
            /// 콤보 바인딩 후 Select 을 최상단에 표시 ALL을  최하단 표시 (필수선택 항목에 사용)
            /// </summary>
            SELECT_ALL,
        }

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");

            cboLine.SelectedValueChanged += cboLine_SelectedValueChanged;

            C1ComboBox[] cboProcGrParent = { cboLine }; //2021.04.09 Line별 공정그룹 Setting으로 수정 START

            SetEquipmentCombo(cboEquipment);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
           
            this.Loaded -= UserControl_Loaded;                                   

            
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
        }
        #endregion
        #region [Method]
        private void GetList()
        {
            try
            {


                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("TOOL_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                dr["EQSGID"] = cboLine.GetBindValue();
                dr["EQPTID"] = cboEquipment.GetBindValue();
                dr["TOOL_ID"] = txtToolId.GetBindValue();
               
                
                dtRqst.Rows.Add(dr);

           
                new ClientProxy().ExecuteService("DA_SEL_TOOL_MONITORING_STATUS_INFO", "INDATA", "OUTDATA", dtRqst, (dtResult, ex) =>
                {
                    
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }
                    Util.GridSetData(dgOutStatusList, dtResult, FrameOperation, true);
                });
                new ClientProxy().ExecuteService("DA_SEL_TOOL_MONITORING_HISTORY_INFO", "INDATA", "OUTDATA", dtRqst, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }
                    Util.GridSetData(dgOutHistList, dtResult, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                btnSearch.IsEnabled = true;
                Util.MessageException(ex);
            }
        }

        public static DataTable ConvertToDataTable(C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn column in dg.Columns)
                    {
                        if (!string.IsNullOrEmpty(column.Name))
                            dt.Columns.Add(column.Name);
                    }
                    return dt;
                }
                else
                {
                    dt = ((DataView)dg.ItemsSource).Table;
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

      

        #endregion
        private void SetEquipmentCombo(C1ComboBox cbo)
        {

            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO_MNT";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboLine.SelectedValue;
            dr["PROCID"] = "FFD101,FF5101";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = AddStatus(dtResult, ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
           // cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;

                if (cbo.SelectedIndex < 0)
                    cbo.SelectedIndex = 0;
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
        }
    
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnPOLISH_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                if (presenter == null)
                    return;

                int clickedIndex = presenter.Row.Index;

                //if (!String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgOutStatusList.Rows[clickedIndex].DataItem, "TOOL_ID"))))
                //   return;

                //[%1]을 연마처리 하시겠습니까?
                string _id = Util.NVC(DataTableConverter.GetValue(dgOutStatusList.Rows[clickedIndex].DataItem, "TOOL_ID"));

                Util.MessageConfirm("SFU8134", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataTable inData = new DataTable();
                            inData.Columns.Add("TOOL_ID", typeof(string));
                            inData.Columns.Add("USERID", typeof(string));
                            inData.Columns.Add("LANGID", typeof(string));


                            DataRow row = inData.NewRow();
                            row["TOOL_ID"] = Util.NVC(DataTableConverter.GetValue(dgOutStatusList.Rows[clickedIndex].DataItem, "TOOL_ID"));
                            row["USERID"] = LoginInfo.USERID;
                            row["LANGID"] = LoginInfo.LANGID;

                            inData.Rows.Add(row);
                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_REG_POLISH_TOOL_F", "RQSTDT", "RSLTDT", inData);

                            //정상처리되었습니다.
                            Util.AlertInfo("SFU1275");  //정상처리되었습니다
                           
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);

                        }
                        finally
                        {
                            GetList();
                        }

                    }
                }, _id);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
     
        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
                case ComboStatus.SELECT_ALL:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);

                    DataRow dr1 = dt.NewRow();
                    dr1[sDisplay] = "-ALL-";
                    dr1[sValue] = "";
                    dt.Rows.InsertAt(dr1, dt.Rows.Count);
                    break;
            }

            return dt;
        }
    }
}
