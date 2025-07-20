/*************************************************************************************
 Created Date : 2023.01.03
      Creator : 이정미
   Decription : 공Tray 상세현황 (ESWA전용)
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.04  NAME  : Initial Created
  2024.02.01  이해령 : 건물코드 수정, Tray Type 멀티박스로 수정, 건물코드에 따른 설비 컬럼 분할
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
using LGC.GMES.MES.CMM001;
using System.Linq;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_018_ESWA : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private bool bFCS001_018_ESWA_EMPTY_TRAY_MGMT = false;

        Util _Util = new Util();

        DataTable dtResult_1;         // MultiSelectionBox 데이터 담기
        bool bComboCheckFlag = false; // 콤보박스 변화 체크     
        int TrayTypeTotalCnt = 0;     // TrayType 총 개수

        public FCS001_018_ESWA()
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
            // 폴란드 숫자 구분 기호
            // 임시적으로 폴란드는 en-US 방식으로 강제 정의
            // 2023.11.08 이준영 수정
            
            //Combo Setting
            InitCombo();
           
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

            //_combo.SetCombo(cboBldgCd, CommonCombo_Form.ComboStatus.ALL, sCase: "BLDG"); //콤보셋팅 공통함수 추가 수정 필요

            string[] sFilter1 = { "FORM_BLDG_CODE" };
            _combo.SetCombo(cboBldgCd, CommonCombo_Form.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilter1);

            //DataRowView drView = cboBldgCd.SelectedItem as DataRowView;
            //string[] sFilter = { "Y", LoginInfo.CFG_AREA_ID };
            //_combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE", sFilter: sFilter);

            SetTrayTypeCombo(cboTrayType);
        }

        private void InitializeDataGrid()
        {
            try
            {
                dgTrayDetailCnt.Columns.Clear();

                dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "TRAY_TYPE",
                    Header = new string[] { "TRAY_TYPE", "TRAY_TYPE" }.ToList<string>(),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("TRAY_TYPE_CODE"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Tag = "TRAY_TYPE",
                });

                dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "TRAY_TYPE_NAME",
                    Header = new string[] { "TRAY_TYPE_NAME", "TRAY_TYPE_NAME" }.ToList<string>(),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("TRAY_TYPE_NAME"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Tag = "TRAY_TYPE_NAME",
                });


                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("ATTR1", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COM_TYPE_CODE"] = "EQP_LOC_GRP_CD";
                dr["ATTR1"] = Util.GetCondition(cboBldgCd, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_SYSTEM_BY_ATTR", "RQSTDT", "RSLTDT", dtRqst); //2021.04.20 Tray 위치 그룹화 대응

                if (dtRslt.Rows.Count > 0)
                {
                    // 폴란드 숫자 구분 기호
                    // 임시적으로 폴란드는 en-US 방식으로 강제 정의
                    // 2023.11.08 이준영 수정
                    if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    {
                        dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                        {

                            Name = "EMPTY_TRAY_ON_CNT_SUM",
                            Header = new string[] { "EMPTY_TRAY_ON_CNT", "합계" }.ToList<string>(),
                            Binding = new Binding()
                            {
                                Path = new PropertyPath("EMPTY_TRAY_ON_CNT_SUM"),
                                Mode = BindingMode.TwoWay
                            },
                            TextWrapping = TextWrapping.Wrap,
                            IsReadOnly = true,
                            Tag = "EMPTY_TRAY_ON_CNT_SUM",
                            Width = new C1.WPF.DataGrid.DataGridLength(100),
                            //Format = "#,##0"
                        });
                    }
                    else
                    {
                        dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                        {

                            Name = "EMPTY_TRAY_ON_CNT_SUM",
                            Header = new string[] { "EMPTY_TRAY_ON_CNT", "합계" }.ToList<string>(),
                            Binding = new Binding()
                            {
                                Path = new PropertyPath("EMPTY_TRAY_ON_CNT_SUM"),
                                Mode = BindingMode.TwoWay
                            },
                            TextWrapping = TextWrapping.Wrap,
                            IsReadOnly = true,
                            Tag = "EMPTY_TRAY_ON_CNT_SUM",
                            Width = new C1.WPF.DataGrid.DataGridLength(100),
                            Format = "#,##0"
                        });
                    }
                    
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        string sCboName = dtRslt.Rows[i]["CBO_NAME"].ToString();
                        string sCboCode = dtRslt.Rows[i]["CBO_CODE"].ToString() + "_ON";

                        // 폴란드 숫자 구분 기호
                        // 임시적으로 폴란드는 en-US 방식으로 강제 정의
                        // 2023.11.08 이준영 수정
                        if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                        {
                            dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                            {
                                Name = Util.NVC(sCboCode),
                                Header = new string[] { "EMPTY_TRAY_ON_CNT", "[*]" + Util.NVC(sCboName) }.ToList<string>(),
                                Binding = new Binding()
                                {
                                    Path = new PropertyPath(Util.NVC(sCboCode)),
                                    Mode = BindingMode.TwoWay
                                },
                                TextWrapping = TextWrapping.Wrap,
                                IsReadOnly = true,
                                Tag = dtRslt.Rows[i]["CBO_CODE"].ToString(),
                                Width = new C1.WPF.DataGrid.DataGridLength(100),
                                //Format = "#,##0"
                            });

                        }
                        else
                        {
                            dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                            {
                                Name = Util.NVC(sCboCode),
                                Header = new string[] { "EMPTY_TRAY_ON_CNT", "[*]" + Util.NVC(sCboName) }.ToList<string>(),
                                Binding = new Binding()
                                {
                                    Path = new PropertyPath(Util.NVC(sCboCode)),
                                    Mode = BindingMode.TwoWay
                                },
                                TextWrapping = TextWrapping.Wrap,
                                IsReadOnly = true,
                                Tag = dtRslt.Rows[i]["CBO_CODE"].ToString(),
                                Width = new C1.WPF.DataGrid.DataGridLength(100),
                                Format = "#,##0"
                            });
                        }
                    }
                }
                HiddenLoadingIndicator();

                ShowLoadingIndicator();
                // 폴란드 숫자 구분 기호
               // 임시적으로 폴란드는 en-US 방식으로 강제 정의
               // 2023.11.08 이준영 수정
               if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
               {

                   dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                   {
                       Name = "EMPTY_TRAY_OFF_CNT_SUM",
                       Header = new string[] { "EMPTY_TRAY_OFF_CNT", "합계" }.ToList<string>(),
                       Binding = new Binding()
                       {
                           Path = new PropertyPath("EMPTY_TRAY_OFF_CNT_SUM"),
                           Mode = BindingMode.TwoWay
                       },
                       TextWrapping = TextWrapping.Wrap,
                       IsReadOnly = true,
                       Tag = "WIPQTY",
                       Width = new C1.WPF.DataGrid.DataGridLength(100),
                       //Format = "#,##0"
                   });
               }

               else
               {
                   dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                   {
                       Name = "EMPTY_TRAY_OFF_CNT_SUM",
                       Header = new string[] { "EMPTY_TRAY_OFF_CNT", "합계" }.ToList<string>(),
                       Binding = new Binding()
                       {
                           Path = new PropertyPath("EMPTY_TRAY_OFF_CNT_SUM"),
                           Mode = BindingMode.TwoWay
                       },
                       TextWrapping = TextWrapping.Wrap,
                       IsReadOnly = true,
                       Tag = "WIPQTY",
                       Width = new C1.WPF.DataGrid.DataGridLength(100),
                       Format = "#,##0"
                   });

               }

               // 폴란드 숫자 구분 기호
               // 임시적으로 폴란드는 en-US 방식으로 강제 정의
               // 2023.11.08 이준영 수정
               if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
               {
                   dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                   {
                       Name = "OFF",//수정
                       Header = new string[] { "EMPTY_TRAY_OFF_CNT", "OFFLINE" }.ToList<string>(),
                       Binding = new Binding()
                       {
                           Path = new PropertyPath("OFF"),//수정
                           Mode = BindingMode.TwoWay
                       },
                       TextWrapping = TextWrapping.Wrap,
                       IsReadOnly = true,
                       Tag = "OFF_CNT",
                       Width = new C1.WPF.DataGrid.DataGridLength(100),
                       //Format = "#,##0"
                   });

                   dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                   {
                       Name = "ETC",//수정
                       Header = new string[] { "EMPTY_TRAY_OFF_CNT", "ETC" }.ToList<string>(),
                       Binding = new Binding()
                       {
                           Path = new PropertyPath("ETC"),//수정
                           Mode = BindingMode.TwoWay
                       },
                       TextWrapping = TextWrapping.Wrap,
                       IsReadOnly = true,
                       Tag = "OFF_ETC",
                       Width = new C1.WPF.DataGrid.DataGridLength(100),
                       Format = "#,##0"
                   });
               }
               else
               {
                   dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                   {
                       Name = "OFF", //수정
                       Header = new string[] { "EMPTY_TRAY_OFF_CNT", "OFFLINE" }.ToList<string>(),
                       Binding = new Binding()
                       {
                           Path = new PropertyPath("OFF"), //수정
                           Mode = BindingMode.TwoWay
                       },
                       TextWrapping = TextWrapping.Wrap,
                       IsReadOnly = true,
                       Tag = "OFF_OFFLINE",
                       Width = new C1.WPF.DataGrid.DataGridLength(100),
                       //Format = "#,##0"
                   });

                   dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                   {
                       Name = "ETC", //수정
                       Header = new string[] { "EMPTY_TRAY_OFF_CNT", "ETC" }.ToList<string>(),
                       Binding = new Binding()
                       {
                           Path = new PropertyPath("ETC"), //수정
                           Mode = BindingMode.TwoWay
                       },
                       TextWrapping = TextWrapping.Wrap,
                       IsReadOnly = true,
                       Tag = "OFF_ETC",
                       Width = new C1.WPF.DataGrid.DataGridLength(100),
                       Format = "#,##0"
                   });

               }

                dgTrayDetailCnt.TopRowHeaderMerge();

                HiddenLoadingIndicator();
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
            InitializeDataGrid();
            GetList();
        }

        private void dgTrayDetailCnt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgTrayDetailCnt == null || dgTrayDetailCnt.CurrentRow == null || dgTrayDetailCnt.CurrentRow.Index < 1)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgTrayDetailCnt.GetCellFromPoint(pnt);

            if (cell != null)
            {
                string sOnOff = string.Empty;

                if (cell.Column.Name.Equals("TRAY_TYPE_CODE") || cell.Column.Name.Equals("TRAY_TYPE_NAME"))
                {
                    return;
                }

                if (cell.Column.Index < dgTrayDetailCnt.Columns["EMPTY_TRAY_OFF_CNT_SUM"].Index)
                {
                    sOnOff = "ON";
                }
                else
                {
                    sOnOff = "OFF";
                }

                string sTrayType = Util.NVC(DataTableConverter.GetValue(dgTrayDetailCnt.Rows[cell.Row.Index].DataItem, "TRAY_TYPE_CODE")).ToString();

                string sLocGrp = string.Empty;
                string sBldgCD = Util.GetCondition(cboBldgCd);
                if (!cell.Column.Name.Equals("EMPTY_TRAY_ON_CNT_SUM") && !cell.Column.Name.Equals("EMPTY_TRAY_OFF_CNT_SUM"))
                {
                    //sLocGrp = cell.Column.Name;
                    sLocGrp = cell.Column.Tag.ToString();
                }

                // 프로그램 ID 확인 후 수정
                object[] parameters = new object[5];
                parameters[0] = sOnOff;
                parameters[1] = sTrayType;
                parameters[2] = sLocGrp;
                parameters[3] = sBldgCD;
                parameters[4] = "Y";

                this.FrameOperation.OpenMenu("SFU010710061", true, parameters); //공 Tray 관리 연계
                
            }
        }

        private void dgTrayDetailCnt_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (!e.Cell.Column.Name.ToString().Equals("TRAY_TYPE_CODE") && !e.Cell.Column.Name.ToString().Equals("TRAY_TYPE_NAME"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold; //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void cboBldgCd_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //CommonCombo_Form _combo = new CommonCombo_Form();

            //DataRowView drView = cboBldgCd.SelectedItem as DataRowView;
            //string[] sFilter = { "Y", LoginInfo.CFG_AREA_ID };
            //_combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE", sFilter: sFilter);

            //SetTrayTypeCombo(cboTrayType);
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
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("BLDG_CODE", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["TRAY_TYPE_CODE"] = (!string.IsNullOrEmpty(cboTrayType.SelectedItemsToString)) ? cboTrayType.SelectedItemsToString : null;
                Indata["BLDG_CODE"] = Util.GetCondition(cboBldgCd, bAllNull: true);

                DataRowView drView = cboBldgCd.SelectedItem as DataRowView;
                dtRqst.Rows.Add(Indata);

                ShowLoadingIndicator();           
                new ClientProxy().ExecuteService("DA_SEL_EMPT_CARRIER_DETAIL", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
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
                            //pivot 처리
                            //pivot 의 row 만들기용 distinct
                            DataTable dtPivot = new DataView(result).ToTable(true, new string[] { "TRAY_TYPE_CODE", "TRAY_TYPE_NAME" });

                            for (int i = dgTrayDetailCnt.Columns["TRAY_TYPE_NAME"].Index +1; i < dgTrayDetailCnt.Columns.Count ; i++)
                            {
                                dtPivot.Columns.Add(dgTrayDetailCnt.Columns[i].Name.ToString(), typeof(Int64));

                            }
                            //pk 지정
                            dtPivot.PrimaryKey = new DataColumn[] { dtPivot.Columns["TRAY_TYPE_CODE"], dtPivot.Columns["TRAY_TYPE_NAME"] };

                            object[] oKeyVal = new object[2];

                            DataView dvPivotData = new DataView(result);
                            dvPivotData.RowFilter = "TRAY_CNT IS NOT NULL";

                            DataTable dtPivotData = dvPivotData.ToTable();

                            for (int i = 0; i < dtPivotData.Rows.Count; i++)
                            {
                                // Set the values of the keys to find.
                                oKeyVal[0] = dtPivotData.Rows[i]["TRAY_TYPE_CODE"].ToString();
                                oKeyVal[1] = dtPivotData.Rows[i]["TRAY_TYPE_NAME"].ToString();

                                DataRow drPivot = dtPivot.Rows.Find(oKeyVal);

                                string sColName = string.Empty;
                                if (dtPivotData.Rows[i]["EQP_LIST_ID"].ToString().Contains("OFF") || dtPivotData.Rows[i]["EQP_LIST_ID"].ToString().Contains("ETC"))
                                {
                                    sColName = dtPivotData.Rows[i]["EQP_LIST_ID"].ToString();
                                }
                                else
                                {
                                    sColName = dtPivotData.Rows[i]["EQP_LIST_ID"].ToString() + "_" + dtPivotData.Rows[i]["ON_OFF"].ToString();
                                }

                                if (dtPivot.Columns.Contains(sColName))
                                {
                                    drPivot[sColName] = dtPivotData.Rows[i]["TRAY_CNT"];
                                }
                            }
                            dtPivot.Columns["TRAY_TYPE_CODE"].AllowDBNull = true;
                            dtPivot.Columns["TRAY_TYPE_NAME"].AllowDBNull = true;

                            dtPivot.PrimaryKey = null;
                            dtPivot.Constraints.Clear();
                                                  
                            Util.GridSetData(dgTrayDetailCnt, dtPivot, FrameOperation, false);

                            AddColSumQty(dgTrayDetailCnt, dtPivot);
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

        private void AddColSumQty(C1.WPF.DataGrid.C1DataGrid datagrid, DataTable dt)
        {
            DataTable preTable = DataTableConverter.Convert(datagrid.ItemsSource);

            int iSumFromCol = datagrid.Columns["EMPTY_TRAY_ON_CNT_SUM"].Index + 1;
            int iSumToCol = datagrid.Columns["EMPTY_TRAY_OFF_CNT_SUM"].Index;
            int iSumEndCol = datagrid.Columns.Count;

            int iOnTotalSumQty = 0;
            int iOffTotalSumQty = 0;
            int iSumQty = 0;

            string ColName = string.Empty;

            foreach (DataRow dr in preTable.Rows)
            {
                iOnTotalSumQty = 0;
                iOffTotalSumQty = 0;
                for (int iCol = iSumFromCol; iCol < iSumEndCol; iCol++)
                {
                    ColName = datagrid.Columns[iCol].Name;

                    if (!string.IsNullOrEmpty(dr[ColName].ToString()))
                    {
                        iSumQty = Convert.ToInt32(Util.NVC(dr[ColName].ToString()));

                        if (iCol < iSumToCol)
                        {
                            iOnTotalSumQty = iOnTotalSumQty + iSumQty;
                        }
                        if (iCol > iSumToCol)
                        {
                            iOffTotalSumQty = iOffTotalSumQty + iSumQty;
                        }
                    }
                }

                dr["EMPTY_TRAY_ON_CNT_SUM"] = iOnTotalSumQty;
                dr["EMPTY_TRAY_OFF_CNT_SUM"] = iOffTotalSumQty;
            }


            Util.GridSetData(datagrid, preTable, FrameOperation, false);
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

        private void SetTrayTypeCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("USE_FLAG", typeof(string));
                dtRqstA.Columns.Add("AREAID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["USE_FLAG"] = "Y";
                drA["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_TRAY_TYPE", "RQSTDT", "RSLTDT", dtRqstA);                

                dtResult_1 = dtResult;
                TrayTypeTotalCnt = dtResult.Rows.Count;

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
