/*************************************************************************************
 Created Date : 2021.01.
      Creator : 
   Decription : 충방전기 온도 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2021.01. DEVELOPER : Initial Created.
  2023.08.16  손동혁 : NA1동 요청사항 온도 기준정보 탭 화면 구현 
  2023.11.21  이의철 : 끝자리 1인 Lane 이외에도 보여주기
  2023.11.22  이의철 : 로딩 이벤트 수정
  2023.12.05  손동혁 : 기준정보 기준 조회 시 레인에 속한 BOX 설비 전부 조회
  2023.12.11  조영대 : 온도그래프 범위 설정 및 선택 강조, 툴팁 적용
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
using System.Windows.Input;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.C1Chart;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_049 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string sLANEID = string.Empty;
        private string sEQPID = string.Empty;

        double height = 0;
        DataTable upjigGrp = new DataTable();
        DataTable upPowerGrp = new DataTable();
        DataTable lowJigGrp = new DataTable();
        DataTable lowPowerGrp = new DataTable();
        bool bUseFlag = false; //2023.08.15 NA1동 이력조회 탭 추가
        bool bFCS001_049_LANE_LAST_CHAR = false; //끝자리 1인 Lane 이외에도 보여주기

        public FCS001_049()
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                bFCS001_049_LANE_LAST_CHAR = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_049_LANE_LAST_CHAR"); //끝자리 1인 Lane 이외에도 보여주기


                /// 2023.08.16
                bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_049"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
                if (bUseFlag)
                {
                    TabEqpStandard.Visibility = System.Windows.Visibility.Visible;
                    btnStandardSearch.Visibility = System.Windows.Visibility.Visible;
                    btnStandardSave.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    TabEqpStandard.Visibility = System.Windows.Visibility.Collapsed;
                    btnStandardSearch.Visibility = System.Windows.Visibility.Collapsed;
                    btnStandardSave.Visibility = System.Windows.Visibility.Collapsed;

                }

                InitCombo();
                InitControl();

                //다른 화면에서 넘어온 경우
                object[] parameters = this.FrameOperation.Parameters;
                if (parameters != null && parameters.Length >= 1)
                {
                    sLANEID = Util.NVC(parameters[0]);
                    sEQPID = Util.NVC(parameters[1]);

                    cboLane.SelectedValue = sLANEID;
                    cboEqp.SelectedValue = sEQPID;

                    GetList();

                }

                this.Loaded -= UserControl_Loaded;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboLaneChild = { cboEqp };

            if (bFCS001_049_LANE_LAST_CHAR.Equals(true)) //끝자리 1인 Lane 이외에도 보여주기)
            {
                string[] sFilterLane = { "" };
                _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LANEFORM", cbChild: cboLaneChild, sFilter: sFilterLane);
            }
            else
            {
                string[] sFilterLane = { "", "1" };
                _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LANEFORM", cbChild: cboLaneChild, sFilter: sFilterLane);
            }
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);


        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnAllCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sCheck = string.Empty;
                string sTEMP_FLAG = string.Empty;
                string sTEMP_VALUE_01 = string.Empty;
                string sTEMP_VALUE_02 = string.Empty;
                string sTEMP_VALUE_03 = string.Empty;
                string sTEMP_VALUE_04 = string.Empty;
                string sTEMP_VALUE_05 = string.Empty;
                string sTEMP_VALUE_06 = string.Empty;
                string sTEMP_VALUE_07 = string.Empty;
                string sTEMP_VALUE_08 = string.Empty;

                for (int idx = 0; idx < dgEqpStandard.Rows.Count; idx++)
                {
                    sCheck = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "CHK"));
                    sTEMP_FLAG = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR_FLAG"));
                    sTEMP_VALUE_01 = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR1_VALUE"));
                    sTEMP_VALUE_02 = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR2_VALUE"));
                    sTEMP_VALUE_03 = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR3_VALUE"));
                    sTEMP_VALUE_04 = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR4_VALUE"));
                    sTEMP_VALUE_05 = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR5_VALUE"));
                    sTEMP_VALUE_06 = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR6_VALUE"));
                    sTEMP_VALUE_07 = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR7_VALUE"));
                    sTEMP_VALUE_08 = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[idx].DataItem, "TMPR8_VALUE"));


                    if (sCheck.Equals("True") || sCheck.Equals("1"))
                    {
                        break;
                    }
                }

                DataTable dt = DataTableConverter.Convert(dgEqpStandard.ItemsSource);
                foreach (DataRow dr in dt.Rows)
                {
                    if (Util.NVC(dr["CHK"]).Equals("True") || Util.NVC(dr["CHK"]).Equals("1"))
                    {
                        dr["TMPR_FLAG"] = sTEMP_FLAG;
                        dr["TMPR1_VALUE"] = sTEMP_VALUE_01;
                        dr["TMPR2_VALUE"] = sTEMP_VALUE_02;
                        dr["TMPR3_VALUE"] = sTEMP_VALUE_03;
                        dr["TMPR4_VALUE"] = sTEMP_VALUE_04;
                        dr["TMPR5_VALUE"] = sTEMP_VALUE_05;
                        dr["TMPR6_VALUE"] = sTEMP_VALUE_06;
                        dr["TMPR7_VALUE"] = sTEMP_VALUE_07;
                        dr["TMPR8_VALUE"] = sTEMP_VALUE_08;

                    }
                }
                Util.GridSetData(dgEqpStandard, dt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEqp.Text = string.Empty;
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboEqpParent = { cboLane };
            string[] sFilterEqp = { "1" };
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "EQPIDBYLANE", cbParent: cboEqpParent, sFilter: sFilterEqp);

        }

        ///2023.08.16
        private void dgEqpStandard_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "CHK", true);    
        }

        ///2023.08.16
        private void btnStandardSearch_Click(object sender, RoutedEventArgs e)
        {
            GetStandardList();
        }

        ///2023.08.16
        private void btnStandrdSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //저장하시겠습니까?
              Util.MessageConfirm("FM_ME_0214", (re) =>
              {
                  if (re != MessageBoxResult.OK)
                  {
                      return;
                  }
                  else
                  {
                      
                      DataTable dtRqst = new DataTable();
                      dtRqst.TableName = "INDATA";
                      dtRqst.Columns.Add("USERID", typeof(string));
                      dtRqst.Columns.Add("LANE_ID", typeof(string));
                      dtRqst.Columns.Add("EQPTID", typeof(string));
                      dtRqst.Columns.Add("TMPR_FLAG", typeof(decimal));
                      dtRqst.Columns.Add("TMPR1_VALUE", typeof(decimal));
                      dtRqst.Columns.Add("TMPR2_VALUE", typeof(decimal));
                      dtRqst.Columns.Add("TMPR3_VALUE", typeof(decimal));
                      dtRqst.Columns.Add("TMPR4_VALUE", typeof(decimal));
                      dtRqst.Columns.Add("TMPR5_VALUE", typeof(decimal));
                      dtRqst.Columns.Add("TMPR6_VALUE", typeof(decimal));
                      dtRqst.Columns.Add("TMPR7_VALUE", typeof(decimal));
                      dtRqst.Columns.Add("TMPR8_VALUE", typeof(decimal));


                      for (int i = 0; i < dgEqpStandard.Rows.Count; i++)
                      {
                          if (Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                          {

                              DataRow dr = dtRqst.NewRow();
                              dr["USERID"] = LoginInfo.USERID;
                              dr["LANE_ID"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "LANE_ID"));
                              dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "EQPTID"));
                              dr["TMPR_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR_FLAG"));
                              dr["TMPR1_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR1_VALUE"));
                              dr["TMPR2_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR2_VALUE"));
                              dr["TMPR3_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR3_VALUE"));
                              dr["TMPR4_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR4_VALUE"));
                              dr["TMPR5_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR5_VALUE"));
                              dr["TMPR6_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR6_VALUE"));
                              dr["TMPR7_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR7_VALUE"));
                              dr["TMPR8_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgEqpStandard.Rows[i].DataItem, "TMPR8_VALUE"));
                              dtRqst.Rows.Add(dr);
                          }
                      }

                      if (dtRqst.Rows.Count == 0)
                      {
                          Util.Alert("SFU1566");  //변경된 데이터가 없습니다.
                          return;
                      }

                  
                      DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_EQP_TEMP_CB", "RQSTDT", "RSLTDT", dtRqst);
                      if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("1"))
                      {
                          Util.Alert("FM_ME_0215");  //저장하였습니다.

                      }
                      else
                      {
                          Util.Alert("FM_ME_0213");  //저장실패하였습니다.
                      }
                  }
            });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }finally
            {
               
            }
           
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgEqpStandard.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgEqpStandard.Rows[i].DataItem, "CHK", true);
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgEqpStandard.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgEqpStandard.Rows[i].DataItem, "CHK", false);
            }
        }

        private void chart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            C1Chart chrt = sender as C1Chart;
            Point pnt = e.GetPosition(null);

            Grid grd = chrt.Parent as Grid;

            if (chrt.Tag.Equals("all"))
            {
                gr0.Visibility = Visibility.Collapsed;
                gr1.Visibility = Visibility.Collapsed;
                gr2.Visibility = Visibility.Collapsed;
                gr3.Visibility = Visibility.Collapsed;

                grd.Visibility = Visibility.Visible;
                grd.Height = grdChrtMain.ActualHeight;
                cl1.Height = grdChrtMain.ActualHeight / 4 * 3;
                cl2.Height = grdChrtMain.ActualHeight / 4 * 3;
                cl3.Height = grdChrtMain.ActualHeight / 4 * 3;
                cl4.Height = grdChrtMain.ActualHeight / 4 * 3;

                chrt.Tag = "one";
            }
            else
            {
                height = grdChrtMain.ActualHeight / 4;
                gr0.Height = height;
                gr1.Height = height;
                gr2.Height = height;
                gr3.Height = height;

                gr0.Visibility = Visibility.Visible;
                gr1.Visibility = Visibility.Visible;
                gr2.Visibility = Visibility.Visible;
                gr3.Visibility = Visibility.Visible;

                cl1.Height = 120;
                cl2.Height = 120;
                cl3.Height = 120;
                cl4.Height = 120;
                chrt.Tag = "all";
            }
        }

        private void dgEqpTemp_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dgEqpTemp.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dgEqpTemp.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void btnConfig0_Click(object sender, RoutedEventArgs e)
        {
            if (numLowLimit0.Value > 0)
            {
                chrUpperJig.View.AxisY.Min = numLowLimit0.Value;
            }
            else
            {
                chrUpperJig.View.AxisY.AutoMin = true;
            }

            if (numMaxLimit0.Value > 0)
            {
                chrUpperJig.View.AxisY.Max = numMaxLimit0.Value;
            }
            else
            {
                chrUpperJig.View.AxisY.AutoMax = true;
            }

            if (numStep0.Value > 0)
            {
                chrUpperJig.View.AxisY.MajorUnit = numStep0.Value;
            }
            else
            {
                chrUpperJig.View.AxisY.MajorUnit = double.NaN;
            }
        }

        private void btnConfig1_Click(object sender, RoutedEventArgs e)
        {
            if (numLowLimit1.Value > 0)
            {
                chrUpperPower.View.AxisY.Min = numLowLimit1.Value;
            }
            else
            {
                chrUpperPower.View.AxisY.AutoMin = true;
            }

            if (numMaxLimit1.Value > 0)
            {
                chrUpperPower.View.AxisY.Max = numMaxLimit1.Value;
            }
            else
            {
                chrUpperPower.View.AxisY.AutoMax = true;
            }

            if (numStep1.Value > 0)
            {
                chrUpperPower.View.AxisY.MajorUnit = numStep1.Value;
            }
            else
            {
                chrUpperPower.View.AxisY.MajorUnit = double.NaN;
            }
        }

        private void btnConfig2_Click(object sender, RoutedEventArgs e)
        {
            if (numLowLimit2.Value > 0)
            {
                chrLowerJig.View.AxisY.Min = numLowLimit2.Value;
            }
            else
            {
                chrLowerJig.View.AxisY.AutoMin = true;
            }

            if (numMaxLimit2.Value > 0)
            {
                chrLowerJig.View.AxisY.Max = numMaxLimit2.Value;
            }
            else
            {
                chrLowerJig.View.AxisY.AutoMax = true;
            }

            if (numStep2.Value > 0)
            {
                chrLowerJig.View.AxisY.MajorUnit = numStep2.Value;
            }
            else
            {
                chrLowerJig.View.AxisY.MajorUnit = double.NaN;
            }
        }

        private void btnConfig3_Click(object sender, RoutedEventArgs e)
        {
            if (numLowLimit3.Value > 0)
            {
                chrLowerPower.View.AxisY.Min = numLowLimit3.Value;
            }
            else
            {
                chrLowerPower.View.AxisY.AutoMin = true;
            }

            if (numMaxLimit3.Value > 0)
            {
                chrLowerPower.View.AxisY.Max = numMaxLimit3.Value;
            }
            else
            {
                chrLowerPower.View.AxisY.AutoMax = true;
            }

            if (numStep3.Value > 0)
            {
                chrLowerPower.View.AxisY.MajorUnit = numStep3.Value;
            }
            else
            {
                chrLowerPower.View.AxisY.MajorUnit = double.NaN;
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
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd083000");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd082959");
                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dtRqst.Rows.Add(dr);

                // Background 실행, 실행후 dgEqpTemp_ExecuteDataCompleted 이벤트 처리
                dgEqpTemp.ExecuteService("DA_SEL_FORMATION_TEMP_HIST", "RQSTDT", "RSLTDT", dtRqst, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgEqpTemp_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;

            SetChartDataTable(dtResult);
            SetChartData();
            ModifyGraph();
        }

        private void GetStandardList()
        {
            try
            {
                /*if (cboEqp.Text.Equals(""))
                {
                    Util.Alert("9080");  //설비를 선택해주세요. 
                    Util.gridClear(dgEqpStandard);
                    return;
                }*/
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
               // dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();

               // dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                dtRqst.Rows.Add(dr);

                // Background 실행
                dgEqpStandard.ExecuteService("DA_SEL_LANE_TEMP_LIST", "RQSTDT", "RSLTDT", dtRqst, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgEqpStandard_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;
            if (!dtResult.Columns.Contains("CHK"))
            {
                dtResult.Columns.Add("CHK", typeof(bool));
            }
        }

        private void dgEqpStandard_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            // 실행 완료
        }

        private void SetChartDataTable(DataTable dt)
        {
            string[] sUpperJig = new string[13];
            sUpperJig[0] = "MEAS_TIME";
            string[] sLowerJig = new string[13];
            sLowerJig[0] = "MEAS_TIME";

            for (int i = 1; i < 13; i++)
            {
                if (i < 10)
                {
                    sUpperJig[i] = "UPPER_JIG_TEMP0" + i + "_VAL";
                    sLowerJig[i] = "LOWER_JIG_TEMP0" + i + "_VAL";
                }
                else
                {
                    sUpperJig[i] = "UPPER_JIG_TEMP" + i + "_VAL";
                    sLowerJig[i] = "LOWER_JIG_TEMP" + i + "_VAL";
                }
            }
            upjigGrp = dt.DefaultView.ToTable(false, sUpperJig);
            lowJigGrp = dt.DefaultView.ToTable(false, sLowerJig);
            string[] sUpperPower = new string[19];
            sUpperPower[0] = "MEAS_TIME";
            string[] sLowerPower = new string[19];
            sLowerPower[0] = "MEAS_TIME";
            for (int i = 1; i < 19; i++)
            {
                if (i < 10)
                {
                    sUpperPower[i] = "UPPER_POWER_TEMP0" + i + "_VAL";
                    sLowerPower[i] = "LOWER_POWER_TEMP0" + i + "_VAL";
                }
                else
                {
                    sUpperPower[i] = "UPPER_POWER_TEMP" + i + "_VAL";
                    sLowerPower[i] = "LOWER_POWER_TEMP" + i + "_VAL";
                }
            }
            upPowerGrp = dt.DefaultView.ToTable(false, sUpperPower);
            lowPowerGrp = dt.DefaultView.ToTable(false, sLowerPower);

        }

        private void SetChartData()
        {
            DataTable upJigGrouped = upjigGrp.AsEnumerable().GroupBy(g => new
            {
                MEAS_TIME = g.Field<DateTime>("MEAS_TIME"),
                TEMP01 = g.Field<double>("UPPER_JIG_TEMP01_VAL"),
                TEMP02 = g.Field<double>("UPPER_JIG_TEMP02_VAL"),
                TEMP03 = g.Field<double>("UPPER_JIG_TEMP03_VAL"),
                TEMP04 = g.Field<double>("UPPER_JIG_TEMP04_VAL"),
                TEMP05 = g.Field<double>("UPPER_JIG_TEMP05_VAL"),
                TEMP06 = g.Field<double>("UPPER_JIG_TEMP06_VAL"),
                TEMP07 = g.Field<double>("UPPER_JIG_TEMP07_VAL"),
                TEMP08 = g.Field<double>("UPPER_JIG_TEMP08_VAL"),
                TEMP09 = g.Field<double>("UPPER_JIG_TEMP09_VAL"),
                TEMP10 = g.Field<double>("UPPER_JIG_TEMP10_VAL"),
                TEMP11 = g.Field<double>("UPPER_JIG_TEMP11_VAL"),
                TEMP12 = g.Field<double>("UPPER_JIG_TEMP12_VAL")
            }).Select(s => new
            {
                MEAS_TIME = s.Key.MEAS_TIME,
                TEMP01 = s.Key.TEMP01,
                TEMP02 = s.Key.TEMP02,
                TEMP03 = s.Key.TEMP03,
                TEMP04 = s.Key.TEMP04,
                TEMP05 = s.Key.TEMP05,
                TEMP06 = s.Key.TEMP06,
                TEMP07 = s.Key.TEMP07,
                TEMP08 = s.Key.TEMP08,
                TEMP09 = s.Key.TEMP09,
                TEMP10 = s.Key.TEMP10,
                TEMP11 = s.Key.TEMP11,
                TEMP12 = s.Key.TEMP12
            }).ToList().ToDataTable();

            upjigGrp = upJigGrouped;
            chrUpperJig.ItemsSource = DataTableConverter.Convert(upjigGrp);


            DataTable upPowGrouped = upPowerGrp.AsEnumerable().GroupBy(g => new
            {
                MEAS_TIME = g.Field<DateTime>("MEAS_TIME"),
                TEMP01 = g.Field<double>("UPPER_POWER_TEMP01_VAL"),
                TEMP02 = g.Field<double>("UPPER_POWER_TEMP02_VAL"),
                TEMP03 = g.Field<double>("UPPER_POWER_TEMP03_VAL"),
                TEMP04 = g.Field<double>("UPPER_POWER_TEMP04_VAL"),
                TEMP05 = g.Field<double>("UPPER_POWER_TEMP05_VAL"),
                TEMP06 = g.Field<double>("UPPER_POWER_TEMP06_VAL"),
                TEMP07 = g.Field<double>("UPPER_POWER_TEMP07_VAL"),
                TEMP08 = g.Field<double>("UPPER_POWER_TEMP08_VAL"),
                TEMP09 = g.Field<double>("UPPER_POWER_TEMP09_VAL"),
                TEMP10 = g.Field<double>("UPPER_POWER_TEMP10_VAL"),
                TEMP11 = g.Field<double>("UPPER_POWER_TEMP11_VAL"),
                TEMP12 = g.Field<double>("UPPER_POWER_TEMP12_VAL"),
                TEMP13 = g.Field<double>("UPPER_POWER_TEMP13_VAL"),
                TEMP14 = g.Field<double>("UPPER_POWER_TEMP14_VAL"),
                TEMP15 = g.Field<double>("UPPER_POWER_TEMP15_VAL"),
                TEMP16 = g.Field<double>("UPPER_POWER_TEMP16_VAL"),
                TEMP17 = g.Field<double>("UPPER_POWER_TEMP17_VAL"),
                TEMP18 = g.Field<double>("UPPER_POWER_TEMP18_VAL")
            }).Select(s => new
            {
                MEAS_TIME = s.Key.MEAS_TIME,
                TEMP01 = s.Key.TEMP01,
                TEMP02 = s.Key.TEMP02,
                TEMP03 = s.Key.TEMP03,
                TEMP04 = s.Key.TEMP04,
                TEMP05 = s.Key.TEMP05,
                TEMP06 = s.Key.TEMP06,
                TEMP07 = s.Key.TEMP07,
                TEMP08 = s.Key.TEMP08,
                TEMP09 = s.Key.TEMP09,
                TEMP10 = s.Key.TEMP10,
                TEMP11 = s.Key.TEMP11,
                TEMP12 = s.Key.TEMP12,
                TEMP13 = s.Key.TEMP13,
                TEMP14 = s.Key.TEMP14,
                TEMP15 = s.Key.TEMP15,
                TEMP16 = s.Key.TEMP16,
                TEMP17 = s.Key.TEMP17,
                TEMP18 = s.Key.TEMP18
            }).ToList().ToDataTable();

            upPowerGrp = upPowGrouped;
            chrUpperPower.ItemsSource = DataTableConverter.Convert(upPowerGrp);

            DataTable lowJigGrouped = lowJigGrp.AsEnumerable().GroupBy(g => new
            {
                MEAS_TIME = g.Field<DateTime>("MEAS_TIME"),
                TEMP01 = g.Field<double>("LOWER_JIG_TEMP01_VAL"),
                TEMP02 = g.Field<double>("LOWER_JIG_TEMP02_VAL"),
                TEMP03 = g.Field<double>("LOWER_JIG_TEMP03_VAL"),
                TEMP04 = g.Field<double>("LOWER_JIG_TEMP04_VAL"),
                TEMP05 = g.Field<double>("LOWER_JIG_TEMP05_VAL"),
                TEMP06 = g.Field<double>("LOWER_JIG_TEMP06_VAL"),
                TEMP07 = g.Field<double>("LOWER_JIG_TEMP07_VAL"),
                TEMP08 = g.Field<double>("LOWER_JIG_TEMP08_VAL"),
                TEMP09 = g.Field<double>("LOWER_JIG_TEMP09_VAL"),
                TEMP10 = g.Field<double>("LOWER_JIG_TEMP10_VAL"),
                TEMP11 = g.Field<double>("LOWER_JIG_TEMP11_VAL"),
                TEMP12 = g.Field<double>("LOWER_JIG_TEMP12_VAL")
            }).Select(s => new
            {
                MEAS_TIME = s.Key.MEAS_TIME,
                TEMP01 = s.Key.TEMP01,
                TEMP02 = s.Key.TEMP02,
                TEMP03 = s.Key.TEMP03,
                TEMP04 = s.Key.TEMP04,
                TEMP05 = s.Key.TEMP05,
                TEMP06 = s.Key.TEMP06,
                TEMP07 = s.Key.TEMP07,
                TEMP08 = s.Key.TEMP08,
                TEMP09 = s.Key.TEMP09,
                TEMP10 = s.Key.TEMP10,
                TEMP11 = s.Key.TEMP11,
                TEMP12 = s.Key.TEMP12
            }).ToList().ToDataTable();

            lowJigGrp = lowJigGrouped;
            chrLowerJig.ItemsSource = DataTableConverter.Convert(lowJigGrp);


            DataTable lowPowGrouped = lowPowerGrp.AsEnumerable().GroupBy(g => new
            {
                MEAS_TIME = g.Field<DateTime>("MEAS_TIME"),
                TEMP01 = g.Field<double>("LOWER_POWER_TEMP01_VAL"),
                TEMP02 = g.Field<double>("LOWER_POWER_TEMP02_VAL"),
                TEMP03 = g.Field<double>("LOWER_POWER_TEMP03_VAL"),
                TEMP04 = g.Field<double>("LOWER_POWER_TEMP04_VAL"),
                TEMP05 = g.Field<double>("LOWER_POWER_TEMP05_VAL"),
                TEMP06 = g.Field<double>("LOWER_POWER_TEMP06_VAL"),
                TEMP07 = g.Field<double>("LOWER_POWER_TEMP07_VAL"),
                TEMP08 = g.Field<double>("LOWER_POWER_TEMP08_VAL"),
                TEMP09 = g.Field<double>("LOWER_POWER_TEMP09_VAL"),
                TEMP10 = g.Field<double>("LOWER_POWER_TEMP10_VAL"),
                TEMP11 = g.Field<double>("LOWER_POWER_TEMP11_VAL"),
                TEMP12 = g.Field<double>("LOWER_POWER_TEMP12_VAL"),
                TEMP13 = g.Field<double>("LOWER_POWER_TEMP13_VAL"),
                TEMP14 = g.Field<double>("LOWER_POWER_TEMP14_VAL"),
                TEMP15 = g.Field<double>("LOWER_POWER_TEMP15_VAL"),
                TEMP16 = g.Field<double>("LOWER_POWER_TEMP16_VAL"),
                TEMP17 = g.Field<double>("LOWER_POWER_TEMP17_VAL"),
                TEMP18 = g.Field<double>("LOWER_POWER_TEMP18_VAL")
            }).Select(s => new
            {
                MEAS_TIME = s.Key.MEAS_TIME,
                TEMP01 = s.Key.TEMP01,
                TEMP02 = s.Key.TEMP02,
                TEMP03 = s.Key.TEMP03,
                TEMP04 = s.Key.TEMP04,
                TEMP05 = s.Key.TEMP05,
                TEMP06 = s.Key.TEMP06,
                TEMP07 = s.Key.TEMP07,
                TEMP08 = s.Key.TEMP08,
                TEMP09 = s.Key.TEMP09,
                TEMP10 = s.Key.TEMP10,
                TEMP11 = s.Key.TEMP11,
                TEMP12 = s.Key.TEMP12,
                TEMP13 = s.Key.TEMP13,
                TEMP14 = s.Key.TEMP14,
                TEMP15 = s.Key.TEMP15,
                TEMP16 = s.Key.TEMP16,
                TEMP17 = s.Key.TEMP17,
                TEMP18 = s.Key.TEMP18
            }).ToList().ToDataTable();

            lowPowerGrp = lowPowGrouped;
            chrLowerPower.ItemsSource = DataTableConverter.Convert(lowPowerGrp);
        }

        private void ModifyGraph()
        {
            chrLowerJig.View.AxisX.IsTime = true;
            chrLowerJig.View.AxisX.AnnoFormat = "yyyy-MM-dd HH:mm:ss";
            chrLowerJig.View.AxisY.AnnoFormat = "#0.##";

            chrLowerPower.View.AxisX.IsTime = true;
            chrLowerPower.View.AxisX.AnnoFormat = "yyyy-MM-dd HH:mm:ss";
            chrLowerPower.View.AxisY.AnnoFormat = "#0.##";

            chrUpperJig.View.AxisX.IsTime = true;
            chrUpperJig.View.AxisX.AnnoFormat = "yyyy-MM-dd HH:mm:ss";
            chrUpperJig.View.AxisY.AnnoFormat = "#0.##";

            chrUpperPower.View.AxisX.IsTime = true;
            chrUpperPower.View.AxisX.AnnoFormat = "yyyy-MM-dd HH:mm:ss";
            chrUpperPower.View.AxisY.AnnoFormat = "#0.##";

            //차트 
            /* height = grdChrtMain.ActualHeight / 5;
             chrLowerJig.Height = height;
             chrLowerPower.Height = height;
             chrUpperJig.Height = height;
             chrUpperPower.Height = height;

             chrLowerJig.Tag = "all";
             chrLowerPower.Tag = "all";
             chrUpperJig.Tag = "all";
             chrUpperPower.Tag = "all";*/
        }

        #endregion


    }
}
