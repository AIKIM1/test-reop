/*************************************************************************************
 Created Date : 2020.10.12
      Creator : Kang Dong Hee
   Decription : 공Tray 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.12  NAME : Initial Created
  2021.04.01  KDH : 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
  2022.07.18  최도훈 : Headrer부분 코드 명이 보이도록 수정.
  2023.08.28  이의철 : Aging Type combo 추가
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Linq;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_017 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string CMCODE = string.Empty;

        //Aging Type combo 추가
        private bool bFCS001_017_EMPTY_TRAY_EQUIP = false; //동별 공통 코드 사용 여부(Aging Type combo 추가)
        private bool bEMPTY_TRAY_EQUIP_USE_YN = false; //Aging Type combo 사용 여부
        
        Util _Util = new Util();

        public FCS001_017()
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //동별 공통코드 : FCS001_017_EMPTY_TRAY_EQUIP(설비 구분 검색 조건)
            bFCS001_017_EMPTY_TRAY_EQUIP =  _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_017_EMPTY_TRAY_EQUIP");

            //Combo Setting
            InitCombo();

            InitControl(); //Aging Type combo 추가

            //Spread Setting
            InitializeDataGrid();

            //조회함수
            GetList();

            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            _combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE"); //콤보셋팅 공통함수 추가 수정 필요
 
            //Aging Type combo 추가
            string[] sFilterAgingType = { "FORM_AGING_TYPE_CODE", "N" }; 
            _combo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.ALL, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilterAgingType);
        }

        //Aging Type combo 추가
        private void InitControl()
        {
            if (bFCS001_017_EMPTY_TRAY_EQUIP.Equals(false))
            {
                bEMPTY_TRAY_EQUIP_USE_YN = false;

                this.cboAgingType.Visibility = Visibility.Hidden;
                this.chkMappingEqpt.Visibility = Visibility.Hidden;
                this.lblAGING_FLAG.Visibility = Visibility.Hidden;
            }
            else
            {
                this.cboAgingType.Visibility = Visibility.Visible;
                this.chkMappingEqpt.Visibility = Visibility.Visible;
                this.lblAGING_FLAG.Visibility = Visibility.Visible;
            }

            if (bEMPTY_TRAY_EQUIP_USE_YN.Equals(true))
            {

                this.cboAgingType.IsEnabled = true;

                this.dgTrayCnt.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                this.dgTrayCnt.Columns["EQP_ID"].Visibility = Visibility.Visible;

                //header row merge
                dgTrayCnt.TopRowHeaderMerge();

                //row merge
                string[] sColumnName = new string[] { "EQPTNAME", "EQP_ID" };
                _Util.SetDataGridMergeExtensionCol(dgTrayCnt, sColumnName, DataGridMergeMode.VERTICAL);

            }
            else
            {

                this.cboAgingType.IsEnabled = false;

                this.dgTrayCnt.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                this.dgTrayCnt.Columns["EQP_ID"].Visibility = Visibility.Collapsed;

            }
        }

        private void InitializeDataGrid()
        {
            try
            {
                //빌딩개수조회
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CMCDTYPE"] = "BLDG_CODE";
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtRqst.Rows.Add(Indata);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_BLDG_INFO", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                string sBldgName = "[*]" + result.Rows[i]["CBO_NAME"].ToString();
                                string sBldgCode = result.Rows[i]["CBO_CODE"].ToString();
                                string SBldgNo = result.Rows[i]["ATTRIBUTE3"].ToString();

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "RUN_TRAY_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "RUN_TRAY" , "RUN_TRAY" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("RUN_TRAY_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_ON_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY" , "ONLINE" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_ON_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_OFF_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY" , "OFFLINE" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_OFF_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_ONCLEAN_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY" , "CLEANING" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_ONCLEAN_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_DISUSE_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY" , "SCRAP" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_DISUSE_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
            InitControl();
        }

        private void dgTrayCnt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTrayCnt == null || dgTrayCnt.CurrentRow == null || dgTrayCnt.CurrentRow.Index < 1)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTrayCnt.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sTrayType = Util.NVC(DataTableConverter.GetValue(dgTrayCnt.Rows[cell.Row.Index].DataItem, "TRAY_TYPE_CD")).ToString();

                    string sOnOff = string.Empty;
                    string sColTag = cell.Column.Name.ToString();
                    string[] sColTagList = sColTag.Split('_');

                    if ((sColTag.Equals("EMPTY_TRAY_ON_CNT_PKG") || sColTag.Equals("EMPTY_TRAY_OFF_CNT_PKG")) ||
                        (!sColTag.Contains("EMPTY_TRAY_ON_CNT") && !sColTag.Contains("EMPTY_TRAY_OFF_CNT")))
                    {
                        return;
                    }

                    if (sColTag.Contains("EMPTY_TRAY_ON_CNT"))
                    {
                        sOnOff = "ON";
                    }
                    else if (sColTag.Contains("EMPTY_TRAY_OFF_CNT"))
                    {
                        sOnOff = "OFF";
                    }

                    //Aging Type combo 추가
                    string sEqpID = string.Empty;
                    string EqpType = "N";

                    if (bEMPTY_TRAY_EQUIP_USE_YN.Equals(true))
                    {
                        sEqpID = Util.NVC(DataTableConverter.GetValue(dgTrayCnt.Rows[cell.Row.Index].DataItem, "EQP_ID")).ToString();
                        EqpType = "Y";
                    }

                    object[] parameters = new object[8];
                    parameters[0] = sOnOff;
                    parameters[1] = sTrayType;
                    parameters[2] = string.Empty;
                    parameters[3] = string.Empty;
                    parameters[4] = string.Empty;
                    //Aging Type combo 추가
                    parameters[5] = sEqpID; 
                    parameters[6] = CMCODE;
                    parameters[7] = EqpType;   //EqpType 구분


                    if (!sColTagList[4].Equals("PKG"))
                    {
                        parameters[3] = sColTagList[4];
                    }
                    else
                    {
                        parameters[3] = string.Empty;
                    }
                    parameters[4] = string.Empty;

                    this.FrameOperation.OpenMenu("SFU010710060", true, parameters); //공 Tray 관리 화면 연계
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgTrayCnt_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if ((!e.Cell.Column.Name.ToString().Equals("EMPTY_TRAY_ON_CNT_PKG") && !e.Cell.Column.Name.ToString().Equals("EMPTY_TRAY_OFF_CNT_PKG"))
                     && (e.Cell.Column.Name.ToString().Contains("EMPTY_TRAY_ON_CNT") || e.Cell.Column.Name.ToString().Contains("EMPTY_TRAY_OFF_CNT")))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold; //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                Util.gridClear(dgTrayCnt);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CMCODE", typeof(string)); //Aging Type combo 추가
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AGING_TYPE_FLAG", typeof(string));

                CMCODE = Util.GetCondition(cboAgingType, bAllNull: true);

                DataRow dr = dtRqst.NewRow();                
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDTYPE"] = "BLDG_CODE";
                dr["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayType, bAllNull: true);

                if (bEMPTY_TRAY_EQUIP_USE_YN.Equals(true))
                {
                    dr["CMCODE"] = Util.GetCondition(cboAgingType, bAllNull: true); //Aging Type combo 추가
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AGING_TYPE_FLAG"] = "Y";
                }                

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                //Aging Type combo 추가
                string sBiz = "DA_SEL_EMPT_TRAY_TOTAL";
                if (bEMPTY_TRAY_EQUIP_USE_YN.Equals(true))
                {
                    sBiz = "DA_SEL_EMPT_TRAY_TOTAL_UI";
                }
                //new ClientProxy().ExecuteService("DA_SEL_EMPT_TRAY_TOTAL", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                new ClientProxy().ExecuteService(sBiz, "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgTrayCnt, result, FrameOperation, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
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

        #endregion

        private void chkMappingEqpt_Checked(object sender, RoutedEventArgs e)
        {
            bEMPTY_TRAY_EQUIP_USE_YN = true;
            InitControl();
            GetList();
        }

        private void chkMappingEqpt_Unchecked(object sender, RoutedEventArgs e)
        {
            bEMPTY_TRAY_EQUIP_USE_YN = false;
            InitControl();
            GetList();
        }

       
    }
}
