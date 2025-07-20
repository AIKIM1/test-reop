/*************************************************************************************
 Created Date : 2022.10.20
      Creator : Choi WanYoung
   Decription : 재공정보현황(차기공정지연)
--------------------------------------------------------------------------------------
 [Change History]
  2022.10.20  DEVELOPER : Initial Created.0
  2022.12.02  최완영    : UI  컬럼Type 폴란드 기준으로 변경 및 결과 수정
  2023.02.23  조영대    : Tray List 목록 조회시 인수 추가
  2023.08.12  이지은    : 다국어 처리관련 소스 수정
  2024.01.29  이정미    : CELL 전체 합계 데이터 검은색 음영으로 표기, 라인 조회 조건 추가
  2024.07.19  주경호    : [E20240708-000388] [Julia] Formation 1/2/3/4 WIP delay by next process modification > MMD에서 공정별 속성값을 각각 배치하여 보이도록 함
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_150 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DispatcherTimer _timer = new DispatcherTimer();

        DataTable dtColor = new DataTable();
        DataTable dt = new DataTable();
        DataTable dtTimer = new DataTable();
        DataTable dtResult_1;         //Line MultiSelectionBox 데이터 담기
        int intTimer1m = 0;
        int intTimer5m = 0;
        int lineTotalCnt = 0;         //Line 총 개수


        public FCS001_150()
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

        #endregion

        #region Event

        private  void SetCondition()
        {
            SEL_Color_Value("PROCESS_DELAY_COLOR_CODE");

            DataRow[] dr = dt.Select("COM_CODE LIKE 'R%'");
            DataRow[] dr1 = dt.Select("COM_CODE LIKE 'T%'");

            dtColor = dr.CopyToDataTable<DataRow>();
            dtTimer = dr1.CopyToDataTable<DataRow>();


            foreach (DataRow dr2 in dtTimer.AsEnumerable())
            {

                if (dr2["COM_CODE"].ToString().Equals("T1"))
                {
                    // rdoRefresh1m.Content = dr2["ATTR1"].ToString() + dr2["COM_CODE_NAME"].ToString();
                    intTimer1m = Util.NVC_Int(dr2["ATTR1"].ToString());

                }
                else if  (dr2["COM_CODE"].ToString().Equals("T2"))
                {
                    // rdoRefresh5m.Content = dr2["ATTR1"].ToString() + dr2["COM_CODE_NAME"].ToString();
                    intTimer5m = Util.NVC_Int(dr2["ATTR1"].ToString());
                }

            }

        }

        private void InitCombo()
        {
            SetLineCombo(cboLine);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            _timer.Tick += new EventHandler(timer_Tick);

            //Combo Setting
            InitCombo();

            SetCondition();
            GetList();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgWipbyOper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                if (dg.CurrentRow == null || dg.CurrentRow.DataItem == null) return;
                if (dg.CurrentColumn.Name.IndexOf("_TRAY") < 0) return;
                if (dg.CurrentRow.Index == dg.Rows.Count - 1) return;

                if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, dgWipbyOper.CurrentColumn.Name.ToString())).Equals("0")) return;

                string sOPER = string.Empty;
                string sOPER_NAME = string.Empty;
                string sMAX_PROCID_PROC_GR_CODE = string.Empty;
                string sLINE_ID = string.Empty;
                string sLINE_NAME = string.Empty;
                string sROUTE_ID = string.Empty;
                string sROUTE_NAME = string.Empty;
                string sMODEL_ID = string.Empty;
                string sMODEL_NAME = string.Empty;
                string sStatus = string.Empty;
                string sStatusName = string.Empty;
                string sLotID = string.Empty;
                string sSpecial = string.Empty;
                string sSpecialName = string.Empty;
                string sRouteTypeDG = string.Empty;
                string sRouteTypeDGName = string.Empty;
                string sLotType = string.Empty;
                string sLotTypeName = string.Empty;
                string sNextOpType = string.Empty;

                if (dgWipbyOper.CurrentColumn.Name.Equals("PRE2HPCD_TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("PRE2HPCD_CELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("NORMAL2HIGH_TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("NORMAL2HIGH_CELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("HIGH2NORMAL_TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("HIGH2NORMAL_CELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("NORMAL2DEGAS_TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("NORMAL2DEGAS_CELL"))
                {
                    sRouteTypeDG = "D";
                }
                else
                {
                    sRouteTypeDG = "E";
                }
                
                sLINE_ID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LINE_ID"));
                sLINE_NAME = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LINE_NAME"));
                sStatus = "A";

                sNextOpType = dgWipbyOper.CurrentColumn.Name.Replace("_TRAY", "").Replace("_CELL", "");

                Load_FCS001_005_02(sOPER, sOPER_NAME, sLINE_ID, sLINE_NAME, sROUTE_ID, sROUTE_NAME, sMODEL_ID, sMODEL_NAME, sStatus, sStatusName, sLotID, sSpecial, sSpecialName, sRouteTypeDG, sRouteTypeDGName,sLotType, sLotTypeName, sNextOpType);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Load_FCS001_005_02(string sOPER, string sOPER_NAME,
                                         string sLINE_ID, string sLINE_NAME,
                                         string sROUTE_ID, string sROUTE_NAME,
                                         string sMODEL_ID, string sMODEL_NAME,
                                         string sStatus, string sStatusName,
                                         string sLotID, string sSPECIAL_YN,
                                         string sSpecialName,
                                         string sROUTE_TYPE_DG, string sROUTE_TYPE_DG_NAME,
                                         string sLotType, string sLotTypeName,
                                         string sNextOpType)
        {
            //Tray List
            FCS001_005_02 TrayList = new FCS001_005_02();
            TrayList.FrameOperation = FrameOperation;

            object[] Parameters = new object[20];
            Parameters[0] = sOPER; //sOPER
            Parameters[1] = sOPER_NAME; //sOPER_NAME
            Parameters[2] = sLINE_ID; //sLINE_ID
            Parameters[3] = sLINE_NAME; //sLINE_NAME
            Parameters[4] = sROUTE_ID; //sROUTE_ID
            Parameters[5] = sROUTE_NAME; //sROUTE_NAME
            Parameters[6] = sMODEL_ID; //sMODEL_ID
            Parameters[7] = sMODEL_NAME; //sMODEL_NAME
            Parameters[8] = sStatus; //sStatus
            Parameters[9] = sStatusName; //sStatusName
            Parameters[10] = sROUTE_TYPE_DG; //sROUTE_TYPE_DG
            Parameters[11] = sROUTE_TYPE_DG_NAME; //sROUTE_TYPE_DG_NAME
            Parameters[12] = sLotID; //sLotID
            Parameters[13] = sSPECIAL_YN; //sSPECIAL_YN
            Parameters[14] = ""; 
            Parameters[15] = ""; 
            Parameters[16] = "";
            Parameters[17] = sLotType;//sLotType
            Parameters[18] = sLotTypeName; //sLotTypeName
            Parameters[19] = sNextOpType; //sLotTypeName

            this.FrameOperation.OpenMenuFORM("SFU010705052", "FCS001_005_02", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);
        }



        private void dgWipbyOper_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PRE2HPCD_TRAY") || e.Column.Name.Equals("PRE2HPCD_CELL") || e.Column.Name.Equals("NORMAL2HIGH_TRAY") || e.Column.Name.Equals("NORMAL2HIGH_CELL") ||
                        e.Column.Name.Equals("HIGH2NORMAL_TRAY") || e.Column.Name.Equals("HIGH2NORMAL_CELL") || e.Column.Name.Equals("NORMAL2DEGAS_TRAY") || e.Column.Name.Equals("NORMAL2DEGAS_CELL") ||
                        e.Column.Name.Equals("WAITOCV_TRAY") || e.Column.Name.Equals("WAITOCV_CELL") || e.Column.Name.Equals("SHIP2OCV_TRAY") || e.Column.Name.Equals("SHIP2OCV_CELL") ||
                        e.Column.Name.Equals("SHIP2SEL2_TRAY") || e.Column.Name.Equals("SHIP2SEL2_CELL") || e.Column.Name.Equals("SHIP2EOL_TRAY") || e.Column.Name.Equals("SHIP2EOL_CELL"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));

        }

        private void dgWipbyOper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;
                
                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                ///////////////////////////////////////////////////////////////////////////////////

                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    if (e.Cell.Column.Name.Equals("PRE2HPCD_TRAY") || e.Cell.Column.Name.Equals("NORMAL2HIGH_TRAY") ||
                        e.Cell.Column.Name.Equals("HIGH2NORMAL_TRAY") || e.Cell.Column.Name.Equals("NORMAL2DEGAS_TRAY") ||
                        e.Cell.Column.Name.Equals("WAITOCV_TRAY") || e.Cell.Column.Name.Equals("SHIP2OCV_TRAY") ||
                        e.Cell.Column.Name.Equals("SHIP2SEL2_TRAY") || e.Cell.Column.Name.Equals("SHIP2EOL_TRAY"))
                    {
                        if (e.Cell.Row.Index == dgWipbyOper.Rows.Count - 1)
                        {
                            e.Cell.Presenter.Foreground = Brushes.Black;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = Brushes.Blue;
                        }
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    if (e.Cell.Column.Name.Equals("PRE2HPCD_CELL") || e.Cell.Column.Name.Equals("NORMAL2HIGH_CELL") ||
                        e.Cell.Column.Name.Equals("HIGH2NORMAL_CELL") || e.Cell.Column.Name.Equals("NORMAL2DEGAS_CELL") ||
                        e.Cell.Column.Name.Equals("WAITOCV_CELL") || e.Cell.Column.Name.Equals("SHIP2OCV_CELL") ||
                        e.Cell.Column.Name.Equals("SHIP2SEL2_CELL") || e.Cell.Column.Name.Equals("SHIP2EOL_CELL"))
                    {
                        int intValue = 0;
                        
                        if (e.Cell.Row.DataItem != null && e.Cell.Column.Name != null)
                        {
                            intValue = int.Parse(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString()).ToString());
                        }


                        foreach (DataRow dr in dtColor.AsEnumerable())
                        {
                            Boolean bCheck = false;
                            String sCode = dr["COM_CODE"].ToString();

                            switch (e.Cell.Column.Name.ToString())
                            {
                                case "PRE2HPCD_CELL": // Pre->HPCD
                                    if ((sCode.Equals("RPH_A") || sCode.Equals("R_A")) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString()) ||
                                        (sCode.Equals("RPH_C") || sCode.Equals("R_C")) && intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) ||
                                        (sCode.Equals("RPH_B") || sCode.Equals("R_B")) && (intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString())))
                                    {
                                        bCheck = true;
                                    }
                                    break;

                                case "NORMAL2HIGH_CELL": // Normal->High
                                    if ((sCode.Equals("RNH_A") || sCode.Equals("R_A")) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString()) ||
                                        (sCode.Equals("RNH_C") || sCode.Equals("R_C")) && intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) ||
                                        (sCode.Equals("RNH_B") || sCode.Equals("R_B")) && (intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString())))
                                    {
                                        bCheck = true;
                                    }
                                    break;

                                case "HIGH2NORMAL_CELL": // High->Normal
                                    if ((sCode.Equals("RHN_A") || sCode.Equals("R_A")) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString()) ||
                                        (sCode.Equals("RHN_C") || sCode.Equals("R_C")) && intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) ||
                                        (sCode.Equals("RHN_B") || sCode.Equals("R_B")) && (intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString())))
                                    {
                                        bCheck = true;
                                    }
                                    break;

                                case "NORMAL2DEGAS_CELL": // Normal->Degas
                                    if ((sCode.Equals("RND_A") || sCode.Equals("R_A")) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString()) ||
                                        (sCode.Equals("RND_C") || sCode.Equals("R_C")) && intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) ||
                                        (sCode.Equals("RND_B") || sCode.Equals("R_B")) && (intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString())))
                                    {
                                        bCheck = true;
                                    }
                                    break;

                                case "WAITOCV_CELL": // OCV #1 Wait
                                    if ((sCode.Equals("ROW_A") || sCode.Equals("R_A")) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString()) ||
                                        (sCode.Equals("ROW_C") || sCode.Equals("R_C")) && intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) ||
                                        (sCode.Equals("ROW_B") || sCode.Equals("R_B")) && (intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString())))
                                    {
                                        bCheck = true;
                                    }
                                    break;

                                case "SHIP2OCV_CELL": // Ship->PRIVT OCV
                                    if ((sCode.Equals("RSO_A") || sCode.Equals("R_A")) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString()) ||
                                        (sCode.Equals("RSO_C") || sCode.Equals("R_C")) && intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) ||
                                        (sCode.Equals("RSO_B") || sCode.Equals("R_B")) && (intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString())))
                                    {
                                        bCheck = true;
                                    }
                                    break;

                                case "SHIP2SEL2_CELL": // Ship->Selector #2
                                    if ((sCode.Equals("RSS_A") || sCode.Equals("R_A")) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString()) ||
                                        (sCode.Equals("RSS_C") || sCode.Equals("R_C")) && intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) ||
                                        (sCode.Equals("RSS_B") || sCode.Equals("R_B")) && (intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString())))
                                    {
                                        bCheck = true;
                                    }
                                    break;

                                case "SHIP2EOL_CELL": // Ship->EOL
                                    if ((sCode.Equals("RE_A") || sCode.Equals("R_A")) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString()) ||
                                        (sCode.Equals("RE_C") || sCode.Equals("R_C")) && intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) ||
                                        (sCode.Equals("RE_B") || sCode.Equals("R_B")) && (intValue >= Util.NVC_Int(dr["ATTR1"].ToString()) && intValue <= Util.NVC_Int(dr["ATTR2"].ToString())))
                                    {
                                        bCheck = true;
                                    }
                                    break;
                            }

                            if (bCheck)
                            {
                                if(String.IsNullOrWhiteSpace(dr["ATTR3"].ToString()) == false) {
                                    e.Cell.Presenter.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(dr["ATTR3"].ToString());
                                }

                                if(String.IsNullOrWhiteSpace(dr["ATTR4"].ToString()) == false) {
                                    e.Cell.Presenter.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(dr["ATTR4"].ToString());
                                }

                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                    
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LINE_NAME")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            
            }));
            
        }

        public void SEL_Color_Value(string sCmcdType)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_SRT", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["COM_TYPE_CODE"] = sCmcdType;

            IndataTable.Rows.Add(Indata);

            dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_SRT", "RQSTDT", "RSLTDT", IndataTable);

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            GetList();
            _timer.Start();

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        private void dgWipbyOper_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                DataTable dtResult = e.ResultData as DataTable;

                int intHpcdTray = 0;
                int intHpcdCell = 0;
                int intHighTray = 0;
                int intHighCell = 0;
                int intNormalTray = 0;
                int intNormalCell = 0;
                int intDegasTray = 0;
                int intDegasCell = 0;
                int intOcvTray = 0;
                int intOcvCell = 0;
                int intShip2OcvTray = 0;
                int intShip2OcvCell = 0;
                int intShip2Sel2Tray = 0;
                int intShip2Sel2Cell = 0;
                int intShip2EolTray = 0;
                int intShip2EolCell = 0;

                for (int iRow = 0; iRow < dtResult.Rows.Count; iRow++)
                {
                    intHpcdTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PRE2HPCD_TRAY"])));
                    intHpcdCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PRE2HPCD_CELL"])));
                    intHighTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["NORMAL2HIGH_TRAY"])));
                    intHighCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["NORMAL2HIGH_CELL"])));
                    intNormalTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["HIGH2NORMAL_TRAY"])));
                    intNormalCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["HIGH2NORMAL_CELL"])));
                    intDegasTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["NORMAL2DEGAS_TRAY"])));
                    intDegasCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["NORMAL2DEGAS_CELL"])));
                    intOcvTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["WAITOCV_TRAY"])));
                    intOcvCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["WAITOCV_CELL"])));
                    intShip2OcvTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP2OCV_TRAY"])));
                    intShip2OcvCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP2OCV_CELL"])));
                    intShip2Sel2Tray += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP2SEL2_TRAY"])));
                    intShip2Sel2Cell += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP2SEL2_CELL"])));
                    intShip2EolTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP2EOL_TRAY"])));
                    intShip2EolCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP2EOL_CELL"])));
                }

                DataRow newRow = dtResult.NewRow();
                newRow["LINE_NAME"] = ObjectDic.Instance.GetObjectName("ALL_SUM");
                newRow["PRE2HPCD_TRAY"] = intHpcdTray;
                newRow["PRE2HPCD_CELL"] = intHpcdCell;
                newRow["NORMAL2HIGH_TRAY"] = intHighTray;
                newRow["NORMAL2HIGH_CELL"] = intHighCell;
                newRow["HIGH2NORMAL_TRAY"] = intNormalTray;
                newRow["HIGH2NORMAL_CELL"] = intNormalCell;
                newRow["NORMAL2DEGAS_TRAY"] = intDegasTray;
                newRow["NORMAL2DEGAS_CELL"] = intDegasCell;
                newRow["WAITOCV_TRAY"] = intOcvTray;
                newRow["WAITOCV_CELL"] = intOcvCell;
                newRow["SHIP2OCV_TRAY"] = intShip2OcvTray;
                newRow["SHIP2OCV_CELL"] = intShip2OcvCell;
                newRow["SHIP2SEL2_TRAY"] = intShip2Sel2Tray;
                newRow["SHIP2SEL2_CELL"] = intShip2Sel2Cell;
                newRow["SHIP2EOL_TRAY"] = intShip2EolTray;
                newRow["SHIP2EOL_CELL"] = intShip2EolCell;
                dtResult.Rows.Add(newRow);

                dtResult.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWipbyOper_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            btnSearch.IsEnabled = true;
        }

        private void cboLine_SelectionChanged(object sender, EventArgs e)
        {
            if (cboLine.SelectedItems.Count == 0)
            {
                cboLine.CheckAll();
            }
        }

        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANG_ID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANG_ID"] = LoginInfo.LANGID;
                dr["EQSGID"] = (!string.IsNullOrEmpty(cboLine.SelectedItemsToString)) ? cboLine.SelectedItemsToString : null;

                dtRqst.Rows.Add(dr);

                btnSearch.IsEnabled = false;

                // Background 처리, 완료시 dgWipbyOper_ExecuteDataCompleted 이벤트 호출
                dgWipbyOper.ExecuteService("DA_SEL_WIP_RETRIEVE_INFO_WORK_DELAYED", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }     

        #region  radio  button  event
        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rbClick = (RadioButton)sender;

            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                //_timer.Interval = TimeSpan.FromTicks(10000000);  //1초
                _timer.Tick += new EventHandler(timer_Tick);
            }

            if (rbClick.Name.ToString().Equals("rdoRefresh1m") && (rbClick.IsEnabled == true))
            {
                if (!_timer.IsEnabled) _timer.IsEnabled = true;
                _timer.Stop();
                //_timer.Interval = TimeSpan.FromTicks(10000000 * 60 * int.Parse(dtColor.Rows[7]["ATTR1"].ToString()));  //1분 ;
                _timer.Interval = TimeSpan.FromSeconds(60 * intTimer1m);
                _timer.Start();
            }
            else if (rbClick.Name.ToString().Equals("rdoRefresh5m") && (rbClick.IsEnabled == true))
            {
                if (!_timer.IsEnabled) _timer.IsEnabled = true;
                _timer.Stop();
                //_timer.Interval = TimeSpan.FromTicks(10000000 * 60 * int.Parse(dtColor.Rows[8]["ATTR1"].ToString()));   //3분 ;
                _timer.Interval = TimeSpan.FromSeconds(60 * intTimer5m);
                _timer.Start();
            }
            else
            {
                if (_timer.IsEnabled) _timer.IsEnabled = false;

            }
        }

        #endregion  radio  button  event

        private void SetLineCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));
                dtRqstA.Columns.Add("AREAID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                drA["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", dtRqstA);

                dtResult_1 = dtResult;
                lineTotalCnt = dtResult.Rows.Count;

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
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
