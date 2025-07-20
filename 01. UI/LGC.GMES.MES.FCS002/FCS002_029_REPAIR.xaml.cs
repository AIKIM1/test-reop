/*************************************************************************************
 Created Date : 2020.12.21
      Creator : PJG
   Decription : Gripper 수리 등록
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.21  PJG : Initial Created
  2022.03.08  KDH : 설비리스트출력 조건 수정
  2022.12.24  LJM : 72개의 Cell로 인해 체크박스 및 텍스트박스 수정
  2023.10.12  주훈: LCI, IR-OCV PIIN 관리 추가 (EqpKind : L, I)

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Collections.Generic;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_029_REPAIR : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        private string _sEqpKind = string.Empty;
        private string _sLaneX = string.Empty;
        private string _sNotUseRowLIst;
        private string _sNotUseColLIst;

        public string EQPKIND
        {
            set { this._sEqpKind = value; }
        }

        public FCS002_029_REPAIR()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;

            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _sEqpKind = tmps[0] as string;

            this.Height = 850;
            this.Width = 1800;

            InitCombo();

            // LCI(L), IROCV(I)  : 2023.10.12 추가
            if (_sEqpKind.Equals("1") || _sEqpKind.Equals("L"))
            {
                tbBoxPos1.Visibility = Visibility.Visible;
                tbBoxPos2.Visibility = Visibility.Visible;
                tbBoxPos3.Visibility = Visibility.Visible;

                cboRow.Visibility = Visibility.Visible;
                cboCol.Visibility = Visibility.Visible;
                cboStg.Visibility = Visibility.Visible;

                // 2025.05.27 |csspoto| : 충방전기 & LCI 에서는 설비 ComboBox & TextBox 제외
                tbEqptID.Visibility = Visibility.Collapsed;
                cboEqp.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbBoxPos1.Visibility = Visibility.Collapsed;
                tbBoxPos2.Visibility = Visibility.Collapsed;
                tbBoxPos3.Visibility = Visibility.Collapsed;

                cboRow.Visibility = Visibility.Collapsed;
                cboCol.Visibility = Visibility.Collapsed;
                cboStg.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region [Method]
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", _sEqpKind };
            _combo.SetCombo(cboEqpKind, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "CMN", sFilter: sFilterEqpType);

            C1ComboBox[] cboLaneMapChild = { cboRow, cboCol, cboStg, cboEqp };

            // 2023.10.12 추가
            // _sEqpKind : LCI(L), IRODV(I)
            //if (_sEqpKind.Equals("8"))
            if (_sEqpKind.Equals("8") || _sEqpKind.Equals("L") || _sEqpKind.Equals("I"))
            {

                string[] sFilterLane = { _sEqpKind };
                _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE_MB", sFilter: sFilterLane, cbChild: cboLaneMapChild);
            }
            else
            {
                string[] sFilterLane = { _sLaneX };
                _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE", sFilter: sFilterLane, cbChild: cboLaneMapChild);
            }

            C1ComboBox[] cboRowMapParent = { cboEqpKind, cboLane };
            _combo.SetCombo(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROW", cbParent: cboRowMapParent);
            _combo.SetCombo(cboCol, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COL", cbParent: cboRowMapParent);
            _combo.SetCombo(cboStg, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "STG", cbParent: cboRowMapParent);

            C1ComboBox[] cboRowMapParent1 = { cboLane, cboEqpKind }; //20220308_설비리스트출력 조건 수정

            // 2023.10.12 추가
            // ORV, IROCV 일 경우 설비를 선택하게함
            if (_sEqpKind.Equals("1") || _sEqpKind.Equals("L"))
                _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPIDBYLANE", cbParent: cboRowMapParent1);
            else
                _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "EQPIDBYLANE", cbParent: cboRowMapParent1);

            string[] sFilter = { "CST_CELL_TYPE_CODE" };
            _combo.SetCombo(cboChannel, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "AREA_COMMON_CODE", sFilter: sFilter);
            if (cboChannel.Items.Count == 2) // SELECT + 1개 
                cboChannel.SelectedIndex = 1;
        }

        private void InitializeDataGrid(string sComCode, C1DataGrid dg)
        {
            try
            {
                _sNotUseRowLIst = string.Empty;
                _sNotUseColLIst = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CST_CELL_TYPE_CODE";
                dr["COM_CODE"] = sComCode;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dg);

                if (dtRslt.Rows.Count > 0)
                {
                    int iColName = 65;
                    string sRowCnt = dtRslt.Rows[0]["ATTR1"].ToString();
                    string sColCnt = dtRslt.Rows[0]["ATTR2"].ToString();
                    _sNotUseRowLIst = dtRslt.Rows[0]["ATTR3"].ToString();
                    _sNotUseColLIst = dtRslt.Rows[0]["ATTR4"].ToString();

                    #region Grid 초기화
                    int iMaxCol;
                    int iMaxRow;
                    List<string> rowList = new List<string>();

                    int iColCount = dg.Columns.Count;
                    for (int i = 0; i < iColCount; i++)
                    {
                        int index = (iColCount - i) - 1;
                        dg.Columns.RemoveAt(index);
                    }

                    iMaxRow = Convert.ToInt16(sRowCnt);
                    iMaxCol = Convert.ToInt16(sColCnt);

                    List<DataTable> dtList = new List<DataTable>();

                    double AAA = Math.Round((dg.ActualWidth - 70) / (iMaxCol - 1), 1);
                    int iColWidth = int.Parse(Math.Truncate(AAA).ToString());

                    for (int i = 0; i < iMaxCol; i++)
                    {
                        SetGridHeaderSingleText(Convert.ToChar(iColName + i).ToString(), dg, iColWidth);
                        SetGridHeaderSingleCheck(Convert.ToChar(iColName + i).ToString(), dg, iColWidth);
                    }

                    //Grid Row 생성
                    int iSeq = 1;
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";

                    for (int iCol = 0; iCol < iMaxCol; iCol++)
                    {
                        dt.Columns.Add(Convert.ToChar(iColName + iCol).ToString() + "_SEQ", typeof(string));
                        dt.Columns.Add(Convert.ToChar(iColName + iCol).ToString() + "_SELECT", typeof(string));

                        if (iCol == 0)
                        {
                            for (int iRow = 0; iRow < iMaxRow; iRow++)
                            {
                                DataRow row1 = dt.NewRow();

                                string[] NotUseRow = _sNotUseRowLIst.Split(',');
                                string[] NotUseCol = _sNotUseColLIst.Split(',');

                                if (NotUseRow.Contains(iRow.ToString()) && NotUseCol.Contains(iCol.ToString()))
                                {
                                    row1[Convert.ToChar(iColName + iCol).ToString() + "_SEQ"] = string.Empty;
                                    row1[Convert.ToChar(iColName + iCol).ToString() + "_SELECT"] = "False";
                                }
                                else
                                {
                                    row1[Convert.ToChar(iColName + iCol).ToString() + "_SEQ"] = iSeq.ToString();
                                    row1[Convert.ToChar(iColName + iCol).ToString() + "_SELECT"] = "False";
                                    iSeq++;
                                }
                                dt.Rows.Add(row1);
                            }
                        }
                        else
                        {
                            for (int iRow = 0; iRow < iMaxRow; iRow++)
                            {
                                string[] NotUseRow = _sNotUseRowLIst.Split(',');
                                string[] NotUseCol = _sNotUseColLIst.Split(',');

                                if (NotUseRow.Contains(iRow.ToString()) && NotUseCol.Contains(iCol.ToString()))
                                {
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString() + "_SEQ"] = string.Empty;
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString() + "_SELECT"] = "False";
                                }
                                else
                                {
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString() + "_SEQ"] = iSeq.ToString();
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString() + "_SELECT"] = "False";
                                    iSeq++;
                                }
                            }
                        }

                    }

                    Util.GridSetData(dg, dt, FrameOperation, true);
                    //dg.UpdateLayout();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridHeaderSingleCheck(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridCheckBoxColumn()
            {
                //Header = sColName,
                Header = new string[] { sColName, "SELECT" }.ToList<string>(),
                Binding = new Binding() { Path = new PropertyPath(sColName + "_SELECT"), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Auto),
                IsReadOnly = false,
                CanUserResize = true
            });
        }

        private void SetGridHeaderSingleText(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                //Header = sColName,
                Header = new string[] { sColName, "SEQ" }.ToList<string>(),
                Binding = new Binding() { Path = new PropertyPath(sColName + "_SEQ"), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Auto),
                IsReadOnly = true,
                CanUserResize = true
            });
        }
        #endregion

        #region [Event]
        private void cboChannel_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(Util.NVC(cboChannel.SelectedValue)))
            {
                InitializeDataGrid(Util.NVC(cboChannel.SelectedValue), dgList);
            }
        }

        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("EQP_ID", typeof(string));
                dtRqst.Columns.Add("CONTENTS", typeof(string));
                dtRqst.Columns.Add("REG_ID", typeof(string));
                dtRqst.Columns.Add("PIN_ROW", typeof(string));
                dtRqst.Columns.Add("PIN_COL", typeof(string));
                dtRqst.Columns.Add("TRAY_LOC", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_COL_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_STG_LOC", typeof(string));
                dtRqst.Columns.Add("EQP_KIND_CD", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                string sContents = Util.GetCondition(txtContents, sMsg: "FM_ME_0117");  //내용을 입력해주세요.
                if (string.IsNullOrEmpty(sContents.ToString())) return;
                string sTrayLoc = Util.GetCondition(cboChannel);
                string sLaneId = Util.GetCondition(cboLane);

                // LCI(L), IROCV(I)  : 2023.10.12 추가
                //string sEqpRowLoc = _sEqpKind.Equals("1") ? Util.GetCondition(cboRow) : null;
                //string sEqpColLoc = _sEqpKind.Equals("1") ? Util.GetCondition(cboCol) : null;
                //string sEqpStgLoc = _sEqpKind.Equals("1") ? Util.GetCondition(cboStg) : null;
                //string sEqpId = _sEqpKind.Equals("8") ? Util.GetCondition(cboEqp) : null;

                string sEqpRowLoc = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboRow) : null;
                string sEqpColLoc = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboCol) : null;
                string sEqpStgLoc = _sEqpKind.Equals("1") || _sEqpKind.Equals("L") ? Util.GetCondition(cboStg) : null;
                string sEqpId = _sEqpKind.Equals("8") || _sEqpKind.Equals("I") ? Util.GetCondition(cboEqp) : null;

                string sEqpKindCd = _sEqpKind;

                for (int iCol = 0; iCol < dgList.Columns.Count; iCol++)
                {
                    for (int iRow = 0; iRow < dgList.Rows.Count; iRow++)
                    {
                        if (dgList.Columns[iCol].Name.Contains("SELECT"))
                        {
                            int idx = dgList.Columns[iCol].Name.LastIndexOf("_");
                            string sPinCol = dgList.Columns[iCol].Name.Remove(idx);// Substring(0, idx);
                            char cPinCol = Convert.ToChar(sPinCol);
                            int iPinCol = Convert.ToInt32(cPinCol);

                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, sPinCol + "_SELECT")).Equals("True"))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["EQP_ID"] = sEqpId;
                                dr["LANE_ID"] = sLaneId;
                                dr["EQP_ROW_LOC"] = sEqpRowLoc;
                                dr["EQP_COL_LOC"] = sEqpColLoc;
                                dr["EQP_STG_LOC"] = sEqpStgLoc;
                                dr["EQP_KIND_CD"] = sEqpKindCd;
                                dr["CONTENTS"] = sContents;
                                dr["REG_ID"] = LoginInfo.USERID;
                                dr["PIN_ROW"] = (iRow - 1).ToString();
                                dr["PIN_COL"] = iPinCol;
                                dr["TRAY_LOC"] = "1";
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }
                    }
                }

                if (dtRqst.Rows.Count == 0)
                {
                    Util.Alert("SFU1654");  // 선택된 요청이 없습니다
                    return;
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_INS_PIN_MAINT_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.MessageInfo("FM_ME_0215");  //저장하였습니다.
                Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Index < 2)
                {
                    return;
                }

                //20220704_검색조건(조회기간) 추가 START
                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                ///////////////////////////////////////////////////////////////////////////////////
                //20220704_검색조건(조회기간) 추가 END

                if (e.Cell.Column.Name.Contains("SEQ"))
                {
                    string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString()));

                    if (string.IsNullOrEmpty(sValue))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                        dgList.GetCell(e.Cell.Row.Index, (e.Cell.Column.Index + 1)).Presenter.Visibility = Visibility.Collapsed;
                        dgList.GetCell(e.Cell.Row.Index, (e.Cell.Column.Index + 1)).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                    }
                }
            }));
        }
    }
}
