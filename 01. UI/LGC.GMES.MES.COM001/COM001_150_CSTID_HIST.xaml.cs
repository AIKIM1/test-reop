/*************************************************************************************
 Created Date : 2022.09.23
      Creator : 최상민
   Decription : 소형 2동 9,10라인 순환물류 tray 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.23   최상민    신규 생성     
  2023.02.28   성민식    세부 트레이 정보 확인 툴팁 추가
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_150_CSTID_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor      

        private string sCstId = "";       

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        public COM001_150_CSTID_HIST()
        {
            InitializeComponent();
            Loaded += COM001_150_CSTID_HIST_Loaded;
        }
        #endregion
        
        #region Event
        private void COM001_150_CSTID_HIST_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Loaded -= COM001_150_CSTID_HIST_Loaded;
                object[] tmps = C1WindowExtension.GetParameters(this);
                //if (tmps != null)
                //{                    
                      sCstId = tmps[0] as string;
                      setCSTId_Hist();                   
                //}
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgCstIDHist_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCstIDHist.GetCellFromPoint(pnt);
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                if (cell != null)
                {
                    if (cell.Row.Index < 0)
                        return;


                    if (cell.Column.Name == "LOAD_REP_CSTID")
                    {
                        string sLoadRepCstid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOAD_REP_CSTID"].Index).Value);
                        string sActDTTM = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["ACTDTTM2"].Index).Value);
                        string sBcdScanPstn = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["BCD_SCAN_PSTN"].Index).Value);


                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LOAD_REP_CSTID", typeof(string));
                        RQSTDT.Columns.Add("ACTDTTM", typeof(string));
                        RQSTDT.Columns.Add("BCD_SCAN_PSTN", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["LOAD_REP_CSTID"] = sLoadRepCstid;
                        dr["ACTDTTM"] = sActDTTM;
                        dr["BCD_SCAN_PSTN"] = sBcdScanPstn;
                        RQSTDT.Rows.Add(dr);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CSTID_BY_LOAD_REP_CSTID", "RQSTDT", "RSLTDT", RQSTDT);

                        if (dtResult.Rows.Count <= 0)
                        {
                            return;
                        }

                        string sToolTipText = "";
                        int cnt = 0;

                        for (cnt = 0; cnt < dtResult.Rows.Count; cnt++)
                        {
                            if (cnt == 0)
                            {
                                sToolTipText = Util.NVC(dtResult.Rows[cnt]["CSTID"]);
                            }
                            else
                            {
                                sToolTipText += "\n" + Util.NVC(dtResult.Rows[cnt]["CSTID"]);
                            }

                        }

                        ToolTip toolTip = new ToolTip();
                        Size size = new Size(100, 100);
                        toolTip.PlacementRectangle = new Rect(pnt, size);
                        toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                        toolTip.Content = sToolTipText;
                        toolTip.IsOpen = true;

                        DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 3), IsEnabled = true };
                        timer.Tick += new EventHandler(delegate (object timerSender, EventArgs timerArgs)
                        {
                            if (toolTip != null)
                            {
                                toolTip.IsOpen = false;
                            }
                            toolTip = null;
                            timer = null;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Mehod

        private void setCSTId_Hist()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));                
                IndataTable.Columns.Add("CSTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CSTID"] = sCstId;                
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_CYCLE_TRAY_HIST", "RQSTDT", "RSLTDT", IndataTable);

                //dgPlanWorkorderList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgCstIDHist, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.AlertByBiz("DA_PRD_SEL_LOGIS_CYCLE_TRAY_HIST", ex.Message, ex.ToString());
            }
        }
        #endregion      
    }
}
