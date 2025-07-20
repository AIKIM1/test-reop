/*************************************************************************************
 Created Date : 2024.09.05
      Creator : CSP
   Decription : Tray 정보조회 Sample 등록
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.05  CSP : Initial Created

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
    public partial class FCS002_021_SAMPLE : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        private string _sTrayID = string.Empty;
        private string _sLaneX = string.Empty;
        private string _sNotUseRowLIst;
        private string _sNotUseColLIst;
        // 조회한 Tray의 CellID 정보
        private DataTable _dtSublotList;
        // 선택 위치 인덱스 파악을 위한 DataTable
        private DataTable _dtGrid;
        private int iRowHeaderCnt;
        // 샘플처리 Indata

        DataTable dtInCellList;
        public string TRAYID
        {
            set { this._sTrayID = value; }
        }
        public DataTable WIPCELL
        {
            set { this._dtSublotList = value; }
        }

        public FCS002_021_SAMPLE()
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
            _sTrayID = tmps[0] as string;
            _dtSublotList = tmps[1] as DataTable;

            this.Height = 850;
            this.Width = 1800;

            InitCombo();

            InitDataTable();

            iRowHeaderCnt = 2;

            if(!_sTrayID.IsNullOrEmpty())
                InitializeDataGrid();
            else
            {
                Util.Alert("FM_ME_0592");  // tray layout 정보가 없습니다.

                Close();
            }
            _dtGrid = DataTableConverter.Convert(dgList.ItemsSource);




        }

        #endregion

        #region [Method]
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilter = { "FORM_SMPL_TYPE_CODE", "Y", null, null, null, null };
            _combo.SetCombo(cboSampleType, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN_WITH_OPTION");

        }


        private void InitDataTable()
        {
            //DataTable dtIndata = indataSet.Tables.Add("INDATA");
            //dtIndata.Columns.Add("USERID", typeof(string));
            //dtIndata.Columns.Add("TD_FLAG", typeof(string));
            //dtIndata.Columns.Add("SPLT_FLAG", typeof(string));
            //dtIndata.Columns.Add("SMPL_RSN", typeof(string));

            //DataTable dtInCell = indataSet.Tables.Add("INCELL");
            //dtInCell.Columns.Add("SUBLOTID", typeof(string));
            //dtInCell.Columns.Add("UNPACK_CELL_YN", typeof(string));
        }
        private void InitializeDataGrid()
        {
            try
            {
                _sNotUseRowLIst = string.Empty;
                _sNotUseColLIst = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CST_CELL_TYPE_CODE";
                dr["CSTID"] = _sTrayID;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_LAYOUT_UI_MB", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgList);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.Alert("FM_ME_0592");  // tray layout 정보가 없습니다.
                    Close();
                }

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

                    int iColCount = dgList.Columns.Count;
                    for (int i = 0; i < iColCount; i++)
                    {
                        int index = (iColCount - i) - 1;
                        dgList.Columns.RemoveAt(index);
                    }

                    iMaxRow = Convert.ToInt16(sRowCnt);
                    iMaxCol = Convert.ToInt16(sColCnt);

                    List<DataTable> dtList = new List<DataTable>();

                    double AAA = Math.Round((dgList.ActualWidth - 70) / (iMaxCol - 1), 1);
                    int iColWidth = int.Parse(Math.Truncate(AAA).ToString());

                    for (int i = 0; i < iMaxCol; i++)
                    {
                        SetGridHeaderSingleText(Convert.ToChar(iColName + i).ToString(), dgList, iColWidth);
                        SetGridHeaderSingleCheck(Convert.ToChar(iColName + i).ToString(), dgList, iColWidth);
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

                    Util.GridSetData(dgList, dt, FrameOperation, true);
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
                IsReadOnly = true,
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
    

        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
             string sSampleType = Util.GetCondition(cboSampleType);

            if (dtInCellList == null)
            {
                Util.Alert("SFU1654");  // 선택된 요청이 없습니다
                return;
            }
            if (dtInCellList.Rows.Count == 0)
            {
                Util.Alert("SFU1654");  // 선택된 요청이 없습니다
                return;
            }

            if (string.IsNullOrEmpty(sSampleType) || sSampleType.Equals("SELECT"))
            {
                //Sample 유형을 선택해주세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0310"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                      
                    }
                });
                return;
            }

            Insert(sSampleType);

        }

        private void Insert(string sSampleType)
        {
            //저장하시겠습니까?
            Util.MessageConfirm("FM_ME_0214", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    string BizRuleID = "BR_SET_SMPL_CELL_ALL_MB";

                    // ---폐기대기 제거 --
                    //if ((bool)rdoGood.IsChecked)
                    //{
                    //    BizRuleID = "BR_SET_SMPL_CELL_ALL_MB";
                    //}
                    //else
                    //{
                    //    BizRuleID = "BR_SET_NGLOT_SMPL_CELL_MB";
                    //}

                    DataSet indataSet = new DataSet();
                    DataTable dtIndata = indataSet.Tables.Add("INDATA");
                    dtIndata.Columns.Add("USERID", typeof(string));
                    dtIndata.Columns.Add("TD_FLAG", typeof(string));
                    dtIndata.Columns.Add("SPLT_FLAG", typeof(string));
                    dtIndata.Columns.Add("SMPL_RSN", typeof(string));

                    DataTable dtInCell = indataSet.Tables.Add("INCELL");
                    dtInCell.Columns.Add("SUBLOTID", typeof(string));
                    dtInCell.Columns.Add("UNPACK_CELL_YN", typeof(string));

                    DataRow InRow = dtIndata.NewRow();
                    InRow["USERID"] = LoginInfo.USERID;
                    InRow["TD_FLAG"] = sSampleType;
                    InRow["SPLT_FLAG"] = "Y"; // 자동 수동 제거 , 수동으로만 작동
                    InRow["SMPL_RSN"] = txtSMPL_RSN.Text; //샘플 사유 추가 (2024.09.05)
                    //InRow["SPLT_FLAG"] = (rdoAuto.IsChecked == true) ? "N" : "Y";

                    dtIndata.Rows.Add(InRow);

                    for (int i = 0; i < dtInCellList.Rows.Count; i++)
                    {
                        DataRow RowCell = dtInCell.NewRow();
                        
                        RowCell["SUBLOTID"] = Util.NVC(dtInCellList.Rows[i]["SUBLOTID"]);

                        RowCell["UNPACK_CELL_YN"] = Util.NVC(dtInCellList.Rows[i]["UNPACK_CELL_YN"]);
                        // 폐기대기 제거
                        //if ((bool)rdoGood.IsChecked)
                        //{
                        //    RowCell["UNPACK_CELL_YN"] = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "UNPACK_CELL_YN"));
                        //}
                        dtInCell.Rows.Add(RowCell);
                    }
                    
                    new ClientProxy().ExecuteService_Multi(BizRuleID, "INDATA,INCELL", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                //저장하였습니다.
                                Util.MessageInfo("FM_ME_0215");
                                this.DialogResult = MessageBoxResult.OK;
                            }
                            else
                            {
                                //저장실패하였습니다.
                                Util.MessageInfo("FM_ME_0213");
                            }
                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            Close();
                        }
                    }, indataSet);
                }
            });
        }

        private void dgList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataTable dtSave = new DataTable();
                dtSave.TableName = "RQSTDT";
                dtSave.Columns.Add("SUBLOTID", typeof(string));

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);

                if (cell == null)
                {
                    return;
                }

                if (cell != null)
                {
                    if (!cell.Column.Name.Contains("SELECT"))
                    {
                        return;
                    }
                }

                string sChk = DataTableConverter.GetValue(dgList.Rows[cell.Row.Index].DataItem, cell.Column.Name).ToString();

                string sCSTSLOT = _dtGrid.Rows[cell.Row.Index-iRowHeaderCnt][cell.Column.Index-1].ToString();

                DataRow[] drSelect = _dtSublotList.Select("CSTSLOT = '" + sCSTSLOT + "'");

                string sSublotID = string.Empty;
                if (drSelect.Length > 0)
                {
                    sSublotID = Util.NVC(drSelect[0]["SUBLOTID"]);
                }

                bool bChk = false;
                if (sChk.Equals("True"))
                {
                    bChk = true;


                    // Check 해제 시 indata에서 해당 Sublot 삭제

                    DataRow[] drDeleteSelect = dtInCellList.Select("SUBLOTID = '" + sSublotID + "'");
                    if (drDeleteSelect.Length > 0)
                    {
                        dtInCellList.Rows.Remove(drDeleteSelect[0]);
                        DataTableConverter.SetValue(dgList.Rows[cell.Row.Index].DataItem, cell.Column.Name, false);
                        return;
                    }

                }

               

                bool bSMPL_YN = GetCellInfo(sSublotID);

                // 샘플 가능
                if (bSMPL_YN)
                {
                    DataTableConverter.SetValue(dgList.Rows[cell.Row.Index].DataItem, cell.Column.Name, !bChk);
                }
                else
                {
                    DataTableConverter.SetValue(dgList.Rows[cell.Row.Index].DataItem, cell.Column.Name, false);

                    //[Cell ID : {0}]의 정보가 존재하지 않거나, 이미 추출된 Cell 입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0308", sSublotID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //SetInit();
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

            }
        }
        #endregion

        private bool GetCellInfo(string CellID)
        {
            try
            {
                string sCellID = Util.Convert_CellID(CellID);

                if (string.IsNullOrEmpty(sCellID))
                {
                    return false;
                }



                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SPLT_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = Util.NVC(sCellID);
                dr["SPLT_FLAG"] = "N";
                dtRqst.Rows.Add(dr);


                string BizRule = "DA_SEL_CELL_SAMPLE_YN_MB";



                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(BizRule, "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    return false;
                }


                if (dtInCellList ==  null)
                {
                    dtInCellList = new DataTable();
                    dtInCellList.Columns.Add("SUBLOTID", typeof(string));
                    dtInCellList.Columns.Add("UNPACK_CELL_YN", typeof(string));

                    DataRow drInCell = dtInCellList.NewRow();
                    drInCell["SUBLOTID"] = sCellID;
                    drInCell["UNPACK_CELL_YN"] = Util.NVC((dtRslt.Rows[0]["UNPACK_CELL_YN"]));
                    dtInCellList.Rows.Add(drInCell);
                }
                else
                {
                    DataRow drInCell = dtInCellList.NewRow();
                    drInCell["SUBLOTID"] = sCellID;
                    drInCell["UNPACK_CELL_YN"] = Util.NVC((dtRslt.Rows[0]["UNPACK_CELL_YN"]));
                    dtInCellList.Rows.Add(drInCell);
                }
            
                return true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
        }



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

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            dtInCellList.Clear();
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
                            DataTableConverter.SetValue(dgList.Rows[iRow].DataItem, sPinCol + "_SELECT", false);
                        }
                    }
                }
            }
        }
    }
}
