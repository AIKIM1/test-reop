/*************************************************************************************
 Created Date : 2020.10.12
      Creator : Kang Dong Hee
   Decription : 공Tray 상세현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.12  NAME : Initial Created
  2021.04.01  KDH : 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
  2021.04.20  KDH : Tray 위치 그룹화 대응
  2021.05.18  조영대 : 공Tray 상세현황 건물콤보박스에 AREAID 정보 연결
  2022.05.13  이정미 : EQP_LOC_GRP_CD 동별 공통코드로 변경
  2023.05.24  이정미 : 공 Tray 집계 시 변환할 수 있는 최댓값 넘어 발생하는 오류 수정
  2023.09.28  형준우 : 데이터그리드헤더 다국어 설정없이 동별공통코드의 설명을 가져오도록 수정
  2023.11.08  이준영 : E20231025-001047 폴란드 숫자 구분 기호로 인해 US 언어로 변경하도록 수정
  2023.12.26  이정미 : 동별공통코드로 화면 분기 
  2024.01.19  남형희 : E20240123-000606 총 합계 수량 표시되도록 수정
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
    public partial class FCS001_018 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private bool bFCS001_018_EMPTY_TRAY_MGMT = false;

        Util _Util = new Util();

        public FCS001_018()
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
            //if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
            //    dgTrayDetailCnt.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
            //else
            //    dgTrayDetailCnt.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);

            //Combo Setting
            InitCombo();
           
            //Spread Setting
            InitializeDataGrid();

            //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능
            bFCS001_018_EMPTY_TRAY_MGMT = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_018_EMPTY_TRAY_MGMT");  

            //조회함수
            GetList();

            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            _combo.SetCombo(cboBldgCd, CommonCombo_Form.ComboStatus.ALL, sCase: "BLDG"); //콤보셋팅 공통함수 추가 수정 필요
            //_combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE"); //콤보셋팅 공통함수 추가 수정 필요

            DataRowView drView = cboBldgCd.SelectedItem as DataRowView;
            string[] sFilter = { "Y", drView["AREAID"]?.ToString() };
            _combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE", sFilter: sFilter);
        }

        private void InitializeDataGrid()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                //dtRqst.Columns.Add("AREAID", typeof(string)); //2021.04.20 Tray 위치 그룹화 대응
                //dtRqst.Columns.Add("CMCDTYPE", typeof(string)); //2021.04.20 Tray 위치 그룹화 대응
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2021.04.20 Tray 위치 그룹화 대응
                //dr["CMCDTYPE"] = "EQP_LOC_GRP_CD"; //2021.04.20 Tray 위치 그룹화 대응
                dr["COM_TYPE_CODE"] = "EQP_LOC_GRP_CD";
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EMPTY_TRAY_EQP_LIST", "RQSTDT", "RSLTDT", dtRqst); //2021.04.20 Tray 위치 그룹화 대응
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_SYSTEM", "RQSTDT", "RSLTDT", dtRqst); //2021.04.20 Tray 위치 그룹화 대응



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
                        //dgTrayDetailCnt.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);


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
                        //2021.04.20 Tray 위치 그룹화 대응 START
                        //string sEqpName = dtRslt.Rows[i]["EQP_NAME"].ToString();
                        //string sEqpCode = dtRslt.Rows[i]["EQPTID"].ToString() + "_ON";

                        //dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                        //{
                        //    Name = Util.NVC(sEqpCode),
                        //    Header = new string[] { "EMPTY_TRAY_ON_CNT", Util.NVC(sEqpName) }.ToList<string>(),
                        //    Binding = new Binding()
                        //    {
                        //        Path = new PropertyPath(Util.NVC(sEqpCode)),
                        //        Mode = BindingMode.TwoWay
                        //    },
                        //    TextWrapping = TextWrapping.Wrap,
                        //    IsReadOnly = true,
                        //    Tag = dtRslt.Rows[i]["EQPTID"].ToString(),
                        //    Format = "#,##0"
                        //});

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
                        
                        //2021.04.20 Tray 위치 그룹화 대응 END
                    }
                }
                HiddenLoadingIndicator();

                ShowLoadingIndicator();
                //DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_EMPTY_TRAY_EQP_LIST", "RQSTDT", "RSLTDT", dtRqst); //2021.04.20 Tray 위치 그룹화 대응
                // DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_CBO", "RQSTDT", "RSLTDT", dtRqst); //2021.04.20 Tray 위치 그룹화 대응
                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_SYSTEM", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt1.Rows.Count > 0)
                {
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
                    

                    for (int i = 0; i < dtRslt1.Rows.Count; i++)
                    {
                        //2021.04.20 Tray 위치 그룹화 대응 START
                        //string sEqpName = dtRslt1.Rows[i]["EQP_NAME"].ToString();
                        //string sEqpCode = dtRslt1.Rows[i]["EQPTID"].ToString() + "_OFF";

                        //if (!Util.NVC(sEqpCode).Equals("ONLINE"))
                        //{
                        //    dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                        //    {
                        //        Name = Util.NVC(sEqpCode),
                        //        Header = new string[] { "EMPTY_TRAY_OFF_CNT", Util.NVC(sEqpName) }.ToList<string>(),
                        //        Binding = new Binding()
                        //        {
                        //            Path = new PropertyPath(sEqpCode),
                        //            Mode = BindingMode.TwoWay
                        //        },
                        //        TextWrapping = TextWrapping.Wrap,
                        //        IsReadOnly = true,
                        //        Tag = dtRslt.Rows[i]["EQPTID"].ToString(),
                        //        Format = "#,##0"
                        //    });
                        //}

                        string sCboName = dtRslt1.Rows[i]["CBO_NAME"].ToString();
                        string sCboCode = dtRslt1.Rows[i]["CBO_CODE"].ToString() + "_OFF";

                        if (!Util.NVC(sCboCode).Equals("ONLINE"))
                        {
                            // 폴란드 숫자 구분 기호
                            // 임시적으로 폴란드는 en-US 방식으로 강제 정의
                            // 2023.11.08 이준영 수정
                            if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                            {

                                dgTrayDetailCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = Util.NVC(sCboCode),
                                    Header = new string[] { "EMPTY_TRAY_OFF_CNT", "[*]" + Util.NVC(sCboName) }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath(sCboCode),
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
                                    Header = new string[] { "EMPTY_TRAY_OFF_CNT", "[*]" + Util.NVC(sCboName) }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath(sCboCode),
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
                        //2021.04.20 Tray 위치 그룹화 대응 END
                    }
                }
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

                if (cell.Row.Index == dgTrayDetailCnt.Rows.Count - 1)
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

                if(bFCS001_018_EMPTY_TRAY_MGMT)
                {
                    this.FrameOperation.OpenMenu("SFU010710061", true, parameters); //공 Tray 관리 연계
                }
                else
                {
                    this.FrameOperation.OpenMenu("SFU010710060", true, parameters); //공 Tray 관리 연계
                }        
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
            CommonCombo_Form _combo = new CommonCombo_Form();

            DataRowView drView = cboBldgCd.SelectedItem as DataRowView;
            string[] sFilter = { "Y", drView["AREAID"]?.ToString() };
            _combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE", sFilter: sFilter);
        }
        #endregion

        #region Method

        private void GetList()
        {

            try
            {
                Util.gridClear(dgTrayDetailCnt);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayType, bAllNull: true);

                DataRowView drView = cboBldgCd.SelectedItem as DataRowView;
                if (!string.IsNullOrEmpty(drView["AREAID"]?.ToString()))
                {
                    Indata["AREAID"] = drView["AREAID"]?.ToString();
                }

                dtRqst.Rows.Add(Indata);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_EMPT_TRAY_DETAIL", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
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

                                string sColName = dtPivotData.Rows[i]["EQP_LIST_ID"].ToString() + "_" + dtPivotData.Rows[i]["ON_OFF"].ToString();
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
                            AddRowSumQty(dgTrayDetailCnt, dtPivot);
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

            //datagrid.ItemsSource = DataTableConverter.Convert(preTable);

            Util.GridSetData(datagrid, preTable, FrameOperation, false);
        }

        private void AddRowSumQty(C1.WPF.DataGrid.C1DataGrid datagrid, DataTable dt)
        {
            DataTable preTable = DataTableConverter.Convert(datagrid.ItemsSource);

            int iSumFromCol = datagrid.Columns["TRAY_TYPE_NAME"].Index + 1;            
            int iSumEndCol = datagrid.Columns.Count;

            int iSumQty = 0;
            string ColName = "";

            DataRow drData = preTable.NewRow();
            drData["TRAY_TYPE_CODE"] = ObjectDic.Instance.GetObjectName("SUM");

            for (int iCol = iSumFromCol; iCol < iSumEndCol; iCol++)
            {
                iSumQty = 0;
                ColName = datagrid.Columns[iCol].Name;

                foreach (DataRow dr in preTable.Rows)
                {
                    if (!string.IsNullOrEmpty(dr[ColName].ToString()))
                    {
                        iSumQty += Convert.ToInt32(Util.NVC(dr[ColName].ToString()));
                    }
                }
                drData[ColName] = iSumQty;
            }
            
            preTable.Rows.Add(drData);            

            Util.GridSetData(datagrid, preTable, FrameOperation, false);
            dgTrayDetailCnt.MergeCells(datagrid.Rows.Count-1, datagrid.Columns["TRAY_TYPE_CODE"].Index, datagrid.Rows.Count - 1, datagrid.Columns["TRAY_TYPE_NAME"].Index);
            
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

    }
}
