/*************************************************************************************
 Created Date : 2020.12.17
      Creator : 정용석
  Description : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.17  정용석 : Initial Created.
  2021.02.04  정용석 : 조회조건 Tray Type -> Project Name으로 변경, Grid Column Width 조정
  2023.05.04  정용석 : 폴란드 팩 3동 확장
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_009 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();    // Timer
        private DataTable dtData = new DataTable();
        private DataTable dtCBOData = new DataTable();
        private bool isFirstFetchFlag = false;
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK003_009()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        private void InitializeC1DataGrid()
        {
            // Set Banded View Count
            const int HEADERTOPLOWS = 3;
            for (int i = 0; i < HEADERTOPLOWS; i++)
            {
                DataGridColumnHeaderRow dataGridColumnHeaderRow = new DataGridColumnHeaderRow();
                this.dgList.TopRows.Add(new DataGridColumnHeaderRow());
            }

            // Set Column Header
            switch (LoginInfo.CFG_AREA_ID.ToUpper())
            {
                case "P8":
                    this.SetC1DataGridColumn("AREAID,AREAID,AREAID".Split(',').ToList<string>(), "AREAID", false);
                    this.SetC1DataGridColumn("동,동,동".Split(',').ToList<string>(), "AREANAME", true);
                    this.SetC1DataGridColumn("CSTPROD,CSTPROD,CSTPROD".Split(',').ToList<string>(), "CSTPROD", false);
                    this.SetC1DataGridColumn("TRAY_TYPE,TRAY_TYPE,TRAY_TYPE".Split(',').ToList<string>(), "CSTPROD_NAME", false);
                    this.SetC1DataGridColumn("PRODID,PRODID,PRODID".Split(',').ToList<string>(), "PRODID", false);
                    this.SetC1DataGridColumn("PRJT_NAME,PRJT_NAME,PRJT_NAME".Split(',').ToList<string>(), "PRJT_NAME", true);
                    this.SetC1DataGridColumn("IN_STK_QTY,IN_STK_QTY,IN_STK_QTY".Split(',').ToList<string>(), "IN_STK_QTY", true);
                    this.SetC1DataGridColumn("TTL,TTL,TTL".Split(',').ToList<string>(), "TTL", true);
                    this.SetC1DataGridColumn("ON_ROUTE_QTY,REGISTED,IN_BLDG_QTY".Split(',').ToList<string>(), "IN_BLDG_QTY", true);
                    this.SetC1DataGridColumn("ON_ROUTE_QTY,IN_TRANSIT,IN_701_QTY".Split(',').ToList<string>(), "IN_701_QTY", true);
                    this.SetC1DataGridColumn("ON_ROUTE_QTY,IN_TRANSIT,IN_601_QTY".Split(',').ToList<string>(), "IN_601_QTY", true);
                    this.SetC1DataGridColumn("ABNORMAL_QTY,ABNORMAL_QTY,ABNORMAL_QTY".Split(',').ToList<string>(), "ABNORMAL_QTY", true);
                    this.SetC1DataGridColumn("RETURN_QTY,RETURN_QTY,RETURN_QTY".Split(',').ToList<string>(), "RETURN_QTY", true);
                    break;
                case "PA":
                    this.SetC1DataGridColumn("AREAID,AREAID,AREAID".Split(',').ToList<string>(), "AREAID", false);
                    this.SetC1DataGridColumn("동,동,동".Split(',').ToList<string>(), "AREANAME", true);
                    this.SetC1DataGridColumn("CSTPROD,CSTPROD,CSTPROD".Split(',').ToList<string>(), "CSTPROD", false);
                    this.SetC1DataGridColumn("TRAY_TYPE,TRAY_TYPE,TRAY_TYPE".Split(',').ToList<string>(), "CSTPROD_NAME", false);
                    this.SetC1DataGridColumn("PRODID,PRODID,PRODID".Split(',').ToList<string>(), "PRODID", false);
                    this.SetC1DataGridColumn("PRJT_NAME,PRJT_NAME,PRJT_NAME".Split(',').ToList<string>(), "PRJT_NAME", true);
                    this.SetC1DataGridColumn("IN_STK_QTY,IN_STK_QTY,IN_STK_QTY".Split(',').ToList<string>(), "IN_STK_QTY", true);
                    this.SetC1DataGridColumn("TTL,TTL,TTL".Split(',').ToList<string>(), "TTL", true);
                    this.SetC1DataGridColumn("ON_ROUTE_QTY,REGISTED,IN_BLDG_QTY".Split(',').ToList<string>(), "IN_BLDG_QTY", true);
                    this.SetC1DataGridColumn("ON_ROUTE_QTY,IN_TRANSIT,IN_F4_QTY".Split(',').ToList<string>(), "IN_F4_QTY", true);
                    this.SetC1DataGridColumn("ABNORMAL_QTY,ABNORMAL_QTY,ABNORMAL_QTY".Split(',').ToList<string>(), "ABNORMAL_QTY", true);
                    this.SetC1DataGridColumn("RETURN_QTY,RETURN_QTY,RETURN_QTY".Split(',').ToList<string>(), "RETURN_QTY", true);
                    break;
                default:
                    this.SetC1DataGridColumn("AREAID,AREAID,AREAID".Split(',').ToList<string>(), "AREAID", false);
                    this.SetC1DataGridColumn("동,동,동".Split(',').ToList<string>(), "AREANAME", true);
                    this.SetC1DataGridColumn("CSTPROD,CSTPROD,CSTPROD".Split(',').ToList<string>(), "CSTPROD", false);
                    this.SetC1DataGridColumn("TRAY_TYPE,TRAY_TYPE,TRAY_TYPE".Split(',').ToList<string>(), "CSTPROD_NAME", false);
                    this.SetC1DataGridColumn("PRODID,PRODID,PRODID".Split(',').ToList<string>(), "PRODID", false);
                    this.SetC1DataGridColumn("PRJT_NAME,PRJT_NAME,PRJT_NAME".Split(',').ToList<string>(), "PRJT_NAME", true);
                    this.SetC1DataGridColumn("IN_STK_QTY,IN_STK_QTY,IN_STK_QTY".Split(',').ToList<string>(), "IN_STK_QTY", true);
                    this.SetC1DataGridColumn("TTL,TTL,TTL".Split(',').ToList<string>(), "TTL", true);
                    this.SetC1DataGridColumn("ON_ROUTE_QTY,REGISTED,IN_BLDG_QTY".Split(',').ToList<string>(), "IN_BLDG_QTY", true);
                    this.SetC1DataGridColumn("ON_ROUTE_QTY,IN_TRANSIT,IN_701_QTY".Split(',').ToList<string>(), "IN_701_QTY", true);
                    this.SetC1DataGridColumn("ON_ROUTE_QTY,IN_TRANSIT,IN_601_QTY".Split(',').ToList<string>(), "IN_601_QTY", true);
                    this.SetC1DataGridColumn("ABNORMAL_QTY,ABNORMAL_QTY,ABNORMAL_QTY".Split(',').ToList<string>(), "ABNORMAL_QTY", true);
                    this.SetC1DataGridColumn("RETURN_QTY,RETURN_QTY,RETURN_QTY".Split(',').ToList<string>(), "RETURN_QTY", true);
                    break;
            }
        }

        private void SetC1DataGridColumn(List<string> lstHeaderDisplayText, string bindingColumnName, bool isVisible)
        {
            C1.WPF.DataGrid.DataGridTextColumn dataGridTextColumn = new C1.WPF.DataGrid.DataGridTextColumn();
            dataGridTextColumn.Header = lstHeaderDisplayText;
            dataGridTextColumn.HorizontalAlignment = HorizontalAlignment.Center;
            dataGridTextColumn.VerticalAlignment = VerticalAlignment.Center;
            dataGridTextColumn.Binding = new Binding(bindingColumnName);
            dataGridTextColumn.Visibility = (isVisible) ? Visibility.Visible : Visibility.Collapsed;
            this.dgList.Columns.Add(dataGridTextColumn);
        }

        private void Initialize()
        {
            this.InitializeC1DataGrid();
            this.isFirstFetchFlag = false;      // 최초 Form Load시 또는 Timer Stop후에 False로 Set
            this.InitializeComboBox();
            this.SearchProcess();
            this.isFirstFetchFlag = true;

            if (this.txtCycle != null)
            {
                this.timerSetting(this.txtCycle);
            }
        }

        private void timerSetting(C1NumericBox newmericBox)
        {
            if (this.dispatcherTimer != null)
            {
                this.dispatcherTimer.Stop();
                this.dispatcherTimer.Tick -= dispatcherTimer_Tick;
                int intervalMinute = Convert.ToInt32(newmericBox.Value);
                this.dispatcherTimer.Tick += dispatcherTimer_Tick;
                this.dispatcherTimer.Interval = new TimeSpan(0, intervalMinute, 0);
                this.dispatcherTimer.Start();
            }
        }

        private void ExecuteProcess()
        {
            this.dispatcherTimer.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (this.dispatcherTimer != null)
                    {
                        this.dispatcherTimer.Stop();
                    }
                    if (this.dispatcherTimer.Interval.TotalSeconds == 0)
                    {
                        return;
                    }
                    this.SearchProcess();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (this.dispatcherTimer != null && this.dispatcherTimer.Interval.TotalSeconds > 0)
                    {
                        this.dispatcherTimer.Start();
                    }
                }
            }));
        }

        private void SearchProcess()
        {
            try
            {
                DataSet dsResult = this.GetLogisMoveState();

                if (CommonVerify.HasTableInDataSet(dsResult) && CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]))
                {
                    // Member 변수에 수행결과 복사
                    this.dtData = dsResult.Tables["OUTDATA"].Copy();
                    this.SetMultiComboBox();
                    this.GridDataBinding();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMultiComboBox()
        {
            // Declarations...
            if (this.isFirstFetchFlag)
            {
                return;
            }

            DataTable dtComboBoxBindingData = new DataTable();

            if (this.dtData == null || this.dtData.Rows.Count <= 0)
            {
                return;
            }

            // Fetch된 데이터 가지고 ComboData 구성하기
            var query = from d1 in this.dtData.AsEnumerable()
                        group d1 by new
                        {
                            PRJT_NAME = d1.Field<string>("PRJT_NAME"),
                        } into grp
                        select new
                        {
                            PRJT_NAME_CODE = grp.Key.PRJT_NAME,
                            PRJT_NAME = grp.Key.PRJT_NAME
                        };
            this.dtCBOData = PackCommon.queryToDataTable(query);

            // DataBinding
            this.cboPrjtName.ItemsSource = DataTableConverter.Convert(this.dtCBOData);

            // 기존 ComboBox에 바인딩된 데이터중 체크된 항목을 추출해서 체크표시해주기
            if (string.IsNullOrEmpty(this.cboPrjtName.SelectedItemsToString) || this.cboPrjtName.SelectedItemsToString.ToUpper().Contains("ALL"))
            {
                this.cboPrjtName.Check(-1);
            }
            else
            {
                for (int i = 0; i < dtComboBoxBindingData.Rows.Count; ++i)
                {
                    if (this.cboPrjtName.SelectedItemsToString.Contains(dtComboBoxBindingData.Rows[i]["PRJT_NAME"].ToString()))
                    {
                        this.cboPrjtName.Check(i);
                    }
                    else
                    {
                        this.cboPrjtName.Uncheck(i);
                    }
                }
            }
        }

        private void GridDataBinding()
        {
            Util.gridClear(this.dgList);
            if (string.IsNullOrEmpty(this.cboPrjtName.SelectedItemsToString))
            {
                Util.GridSetData(this.dgList, this.dtData, FrameOperation, false);
            }
            else
            {
                DataTable dt = this.dtData.AsEnumerable().Where(x => this.cboPrjtName.SelectedItemsToString.Contains(x.Field<string>("PRJT_NAME"))).CopyToDataTable();
                Util.GridSetData(this.dgList, dt, FrameOperation, false);
            }

            string[] mergeColumnName = new string[] { "AREANAME" };
            PackCommon.SetDataGridMergeExtensionCol(this.dgList, mergeColumnName, ControlsLibrary.DataGridMergeMode.VERTICAL);
        }

        private void InitializeComboBox()
        {
            try
            {
                this.cboPrjtName.isAllUsed = true;
                this.cboPrjtName.ApplyTemplate();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 - Cell 반송, 투입 요청 등록 (NEXT 투입 요청 포함)
        private DataSet GetLogisMoveState()
        {
            DataSet dsResult = new DataSet();
            try
            {
                string bizRuleName = "BR_PRD_SEL_LOGIS_MOVE_STATE";
                //string bizRuleName = "BR_PRD_SEL_LOGIS_MOVE_STATE_COPY";        // For 운영 테스트
                DataSet dsINDATA = new DataSet();

                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtINDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtINDATA.Rows.Add(dr);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(dt => dt.TableName).ToList());
                dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, "OUTDATA", dsINDATA, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dsResult;
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            this.ExecuteProcess();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.dispatcherTimer != null)
            {
                this.dispatcherTimer.Stop();
            }
        }

        private void txtCycle_ValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<double> e)
        {
            C1NumericBox newmericBox = (C1NumericBox)sender;
            if (newmericBox == null)
            {
                return;
            }

            this.timerSetting(newmericBox);
        }
        #endregion
    }
}