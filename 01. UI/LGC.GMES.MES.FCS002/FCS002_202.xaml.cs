/*************************************************************************************
 Created Date : 2022.12.01
      Creator : KIM TAEKYUN
   Decription : Stocker Rack 현황
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2022.12.01  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_202 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Hashtable hash_loss_color = new Hashtable();
        private DataTable _dtColor = new DataTable();
        private DataTable _dtDATA = new DataTable();
        private DataTable dtTemp = new DataTable();
        private DataTable _dtCopy = new DataTable();

        private Point prevRowPos = new Point(0, 0);
        private Point prevColPos = new Point(0, 0);

        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;
        private string _sRackID = string.Empty;

        Util Util = new Util();


        #endregion

        #region Initialize
        public FCS002_202()
        {
            InitializeComponent();
        }

        ///// <summary>
        ///// Frame과 상호작용하기 위한 객체
        ///// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();
            this.Loaded -= UserControl_Loaded; //20210406 화면이동 후 재 Load 이벤트 안 타도록 수정
        }

        

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            //_combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE_MB");
            string[] sFilter1 = { "FORM_STOCKER_TYPE_CODE", string.Empty };
            _combo.SetCombo(cboStockerType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter1);

            string[] sFilter2 = { "COMBO_RACK_STAT_SET_CODE" };
            _combo.SetCombo(cboSetMode, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "CMN", sFilter: sFilter2);

            GeColorLegend();

        }
        private void GetCommonCode()
        {
            try
            {
                LANE_ID = string.Empty;
                EQPT_GR_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_STOCKER_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboStockerType);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SYSTEM_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);
                foreach (DataRow row in dtResult.Rows)
                {
                    EQPT_GR_TYPE_CODE = row["ATTR1"].ToString();
                    LANE_ID = row["ATTR2"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void cboRow_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            //GetList();
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            string sLane = string.Empty;
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            GetCommonCode();
            GetLane(ref sLane);
            //string[] filter = { "", cboLane.SelectedValue.ToString() };
            //string[] filter = { "", sLane };
            //_combo.SetCombo(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROW", sFilter: filter);
               
            cboSCLine.Text = string.Empty;
            string[] sFilter = { EQPT_GR_TYPE_CODE, sLane };
            _combo.SetCombo(cboSCLine, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SCLINE", sFilter: sFilter); //20210331 S/C 호기 필수 값으로 변경

            
            object[] objParent = { EQPT_GR_TYPE_CODE, sLane, Util.GetCondition(cboSCLine) };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AGING_ROW", objParent: objParent);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        private void cboSetMode_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.GetCondition(cboSetMode).Equals("UNUSE"))  //입고금지
            {
                txtRemark.IsEnabled = true;
            }
            else
            {
                txtRemark.Text = string.Empty;
                txtRemark.IsEnabled = false;
            }
        }

        private void dgColorLegend_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) == "CBO_NAME")
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;

                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTRIBUTE2")).ToString()))
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTRIBUTE2")).ToString()) as SolidColorBrush;
                        }
                    }
                }
              
            }));
        }
        
        private void dgStockerRack_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                e.Cell.Presenter.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 216, 216, 216));
                e.Cell.Presenter.BorderThickness = new Thickness(0.5, 0, 0.5, 1);

                bool bRH = false;
                bool bCH = false;

                #region Header Setting
                //ROW Header 설정
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "1")).Equals(string.Empty))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    bRH = true;
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    bRH = false;
                }

                //Column Header 설정
                if (e.Cell.Column.Index == 0)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    bCH = true;
                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    bCH = false;
                }
                #endregion

                if (bRH.Equals(true) || bCH.Equals(true))
                {
                    return;
                }
                else
                {
                    if (_dtCopy.Columns.Count <= e.Cell.Column.Index) return;

                    string ROWNUM = Util.NVC(_dtCopy.Rows[e.Cell.Row.Index][e.Cell.Column.Index]);

                    if (!string.IsNullOrEmpty(ROWNUM))
                    {
                        string BCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "BCOLOR"));
                        string FCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "FCOLOR"));
                        string TEXT = Util.NVC(GetDtRowValue(ROWNUM, "TEXT"));
                        string RACKID = Util.NVC(GetDtRowValue(ROWNUM, "RACKID"));
                        string STATUS = Util.NVC(GetDtRowValue(ROWNUM, "STATUS"));

                        if (!string.IsNullOrEmpty(BCOLOR))
                        {
                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(BCOLOR) as SolidColorBrush;
                        }

                        if (!string.IsNullOrEmpty(FCOLOR))
                        {
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(FCOLOR) as SolidColorBrush;
                        }

                        if (!string.IsNullOrEmpty(STATUS) && STATUS.Equals("15"))
                        {
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(FCOLOR) as SolidColorBrush;
                        }

                        DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), TEXT);
                        e.Cell.Presenter.Tag = RACKID;

                        #region High Rack 여부 테두리 표시
                        string highRackFlag = Util.NVC(GetDtRowValue(ROWNUM, "HIGH_CST_FLAG"));
                        if (highRackFlag.Equals("Y"))
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 236, 128, 147));
                            e.Cell.Presenter.BorderThickness = new Thickness(2, 2, 2, 2);
                        }
                        #endregion
                    }
                    else
                    {
                        //RACK없음
                        e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                        e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), ObjectDic.Instance.GetObjectName("NO_RACK"));
                    }
                }
            }));

            //try
            //{
            //    C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //    dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        if (e.Cell.Presenter == null)
            //            return;

            //        //ROW Header 설정
            //        if (e.Cell.Row.Index == 0 || e.Cell.Row.Index == _dtCopy.Rows.Count || e.Cell.Column.Index == 0)
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
            //            e.Cell.Presenter.FontWeight = FontWeights.Bold;
            //            e.Cell.Presenter.Foreground = Brushes.Black;
            //        }
            //        else
            //        {
            //            e.Cell.Presenter.FontWeight = FontWeights.Normal;

            //            string ROWNUM = _dtCopy.Rows[e.Cell.Row.Index][(e.Cell.Column.Index + 1).ToString()].ToString();

            //            if (!string.IsNullOrEmpty(ROWNUM))
            //            {
            //                string BCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "BCOLOR"));
            //                string FCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "FCOLOR"));
            //                string TEXT = Util.NVC(GetDtRowValue(ROWNUM, "TEXT"));

            //                if (!string.IsNullOrEmpty(BCOLOR))
            //                    e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(BCOLOR) as SolidColorBrush;

            //                if (!string.IsNullOrEmpty(FCOLOR))
            //                    e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(FCOLOR) as SolidColorBrush;

            //                DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), TEXT);
            //                e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //            }
            //            else
            //            {
            //                //설비없음
            //                e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
            //                e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
            //                DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), ObjectDic.Instance.GetObjectName("NO_EQP"));
            //            }

            //            e.Cell.Presenter.Padding = new Thickness(0);
            //            e.Cell.Presenter.Margin = new Thickness(0);
            //            e.Cell.Presenter.BorderBrush = Brushes.DarkGray;
            //            e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 0, 0);
            //        }
            //    }));

            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        private void dgStockerRack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ClearControlRack();

                // 선택 적재RACK 정보 표시
                tcRackInfo.SelectedIndex = 1;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgStockerRack.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0))
                    {
                        return;
                    }

                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();

                    int iRowNum;
                    if (string.IsNullOrEmpty(ROWNUM) || !int.TryParse(ROWNUM, out iRowNum))
                    {
                        return;
                    }

                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "EQP_COL_LOC"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "EQP_STG_LOC"));
                    string RACKID = Util.NVC(GetDtRowValue(ROWNUM, "RACK_ID"));

                    txtSelRow.Text = Util.GetCondition(cboRow);

                    if (!string.IsNullOrEmpty(COL) && !string.IsNullOrEmpty(STG))
                    {
                        txtSelCol.Text = string.Format("{0:00}", Convert.ToInt16(COL));
                        txtSelStg.Text = string.Format("{0:00}", Convert.ToInt16(STG));
                    }

                    if (!string.IsNullOrEmpty(RACKID))
                    {
                        GetEqpInfo(RACKID);
                    }

                    //입고 금지 사유 
                    DataRow[] sRackIDs = dtTemp.Select("RACK_ID = '" + RACKID + "'");
                    txtRemark.Text = Util.NVC(sRackIDs[0]["RCV_PRHB_RSN"]);

                    if (!string.IsNullOrEmpty(txtRemark.Text))
                    {
                        cboSetMode.SelectedIndex = 3;
                        txtRemark.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


            //Point pnt = e.GetPosition(null);
            //C1.WPF.DataGrid.DataGridCell cell = dgStockerRack.GetCellFromPoint(pnt);

            //if (cell != null)
            //{
            //    _sRackID = string.Empty;
            //    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();
            //    if (string.IsNullOrEmpty(ROWNUM)) return;

            //    int i = 0;
            //    if (!int.TryParse(ROWNUM, out i)) return;

            //    _sRackID = Util.NVC(GetDtRowValue(ROWNUM, "RACK_ID"));
            //    string EQP_STG_LOC = Util.NVC(GetDtRowValue(ROWNUM, "EQP_STG_LOC"));
            //    string EQP_COL_LOC = Util.NVC(GetDtRowValue(ROWNUM, "EQP_COL_LOC"));

            //    txtSelRow.Text = Util.GetCondition(cboRow);
            //    txtSelCol.Text = EQP_COL_LOC;
            //    txtSelStg.Text = EQP_STG_LOC;

            //    if (!string.IsNullOrEmpty(_sRackID))
            //    {
            //        GetEqpInfo(_sRackID);
            //    }
            //}
        }

        private void btnTraySearch_Click(object sender, RoutedEventArgs e)
        {

            //try
            //{
            //    if (string.IsNullOrEmpty(txtSearchTray.Text))
            //    {
            //        //TRAY ID를 입력하세요.
            //        Util.MessageValidation("SFU4975");
            //        return;
            //    }

            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("CSTID", typeof(string));
                
            //    DataRow dr = RQSTDT.NewRow();
            //    dr["CSTID"] = txtSearchTray.Text; //TRAY ID를 입력하세요.
            //    RQSTDT.Rows.Add(dr);

            //    ShowLoadingIndicator();
            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_RETRIEVE_RACK_ST310_JDA_MB", "RQSTDT", "RSLTDT", RQSTDT);

            //    if (dtRslt.Rows.Count > 0)
            //    {
            //        //검색하신 Tray ID: %1는\r\n 열 : %2\r\n 연 : %3 \r\n 단 : %4에 입고되어 있습니다.
            //        Util.AlertInfo("FM_ME_0485", new string[] { txtSearchTray.Text, dtRslt.Rows[0]["ROW_NO"].ToString(), dtRslt.Rows[0]["COL_NO"].ToString(), dtRslt.Rows[0]["STAGE_NO"].ToString() });
            //    }
            //    else
            //    {
            //        Util.AlertInfo("FM_ME_0170");  //설비내에 Tray가 존재하지 않습니다.
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
            //finally
            //{
            //    HiddenLoadingIndicator();
            //}
        }

        private void btnTrayList_Click(object sender, RoutedEventArgs e)
        {
            object[] Parameters = new object[2];
            Parameters[0] = cboStockerType.SelectedValue.ToString(); //sOPER

            //연계 화면 확인 후 수정
            this.FrameOperation.OpenMenuFORM("FCS002_202_TRAY_LIST", "FCS002_202_TRAY_LIST", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Stocker Rack 현황"), true, Parameters);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgStockerRack.CurrentRow == null || dgStockerRack.SelectedIndex == -1)
            {
                Util.MessageValidation("SUF9002");  //RACK 위치를 먼저 선택해주세요
                return;
            }

            string sStatus = string.Empty;

            if (string.IsNullOrEmpty(Util.GetCondition(cboSetMode)))
            {
                Util.MessageValidation("FM_ME_0137");  //변경할 상태를 선택해주세요.
                return;
            }
            else
            {
                sStatus = Util.GetCondition(cboSetMode);

                if (sStatus.Equals("UNUSE") && string.IsNullOrEmpty(txtRemark.Text.Trim()))
                {
                    Util.MessageValidation("FM_ME_0196");  //입고금지 내용을 입력해주세요.
                    return;
                }
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("RACK_STAT_CODE", typeof(string));
                dtRqst.Columns.Add("RACK_ID", typeof(string));
                dtRqst.Columns.Add("RACK_INFO_DEL_FLAG", typeof(string));
                dtRqst.Columns.Add("RCV_PRHB_RSN", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridCell cell in dgStockerRack.Selection.SelectedCells)
                {
                    DataRow dr = dtRqst.NewRow();
                    dr["RACK_STAT_CODE"] = sStatus;
                    dr["RACK_ID"] = cell.Presenter.Tag.ToString();

                    if (EQPT_GR_TYPE_CODE.Equals("3") || EQPT_GR_TYPE_CODE.Equals("4")
                         || EQPT_GR_TYPE_CODE.Equals("9") || EQPT_GR_TYPE_CODE.Equals("7")) //상온 || 고온  정보삭제일 경우
                    {
                        if (sStatus.Equals("INIT_RACK"))
                        {
                            dr["RACK_INFO_DEL_FLAG"] = 1;
                        }
                    }

                    dr["RCV_PRHB_RSN"] = Util.GetCondition(txtRemark);
                    dr["USERID"] = LoginInfo.USERID;
                    dtRqst.Rows.Add(dr);
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RACK_EQP_STATUS_MB", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
                {
                    if (sStatus.ToString().Equals("UNUSE"))
                    {
                        Util.MessageValidation("FM_ME_0061");  //Rack 설정 변경에 실패하였습니다.\r\n입고 금지 Rack만 설정 가능합니다.
                        // 입고가능 rack 만 설정가능으로 변경 필요
                    }
                    else if (sStatus.ToString().Equals("USABLE"))
                    {
                        Util.MessageValidation("FM_ME_0062");  //Rack 설정 변경에 실패하였습니다.\r\n입고 금지 Rack만 설정 가능합니다.

                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0061");  //Rack 설정 변경에 실패하였습니다.
                        // RACK 상태가 정상일때 실패,
                    }
                }
                else
                {
                    Util.MessageValidation("FM_ME_0063");  //Rack 설정 변경을 완료하였습니다.
                }

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //if (dgStockerRack.CurrentRow == null || dgStockerRack.SelectedIndex == -1)
            //{
            //    return;
            //}

            //string sStatus = string.Empty;

            //if (string.IsNullOrEmpty(Util.GetCondition(cboSetMode)))
            //{
            //    Util.MessageValidation("FM_ME_0137");  //변경할 상태를 선택해주세요.
            //    return;
            //}
            //else
            //{
            //    sStatus = Util.GetCondition(cboSetMode);

            //    if (sStatus.Equals("UNUSE") && string.IsNullOrEmpty(txtRemark.Text.Trim()))
            //    {
            //        Util.MessageValidation("FM_ME_0196");  //입고금지 내용을 입력해주세요.
            //        return;
            //    }
            //}

            //try
            //{
            //    DataTable dtRqst = new DataTable();
            //    dtRqst.TableName = "INDATA";
            //    dtRqst.Columns.Add("RACK_STAT_CODE", typeof(string));
            //    dtRqst.Columns.Add("RACK_ID", typeof(string));
            //    dtRqst.Columns.Add("RACK_INFO_DEL_FLAG", typeof(string));
            //    dtRqst.Columns.Add("RCV_PRHB_RSN", typeof(string));
            //    dtRqst.Columns.Add("USERID", typeof(string));

            //    foreach (C1.WPF.DataGrid.DataGridCell cell in dgStockerRack.Selection.SelectedCells)
            //    {
            //        DataRow dr = dtRqst.NewRow();
            //        dr["RACK_STAT_CODE"] = sStatus;
            //        dr["RACK_ID"] = _sRackID;
            //        dr["RCV_PRHB_RSN"] = Util.GetCondition(txtRemark);
            //        dr["USERID"] = LoginInfo.USERID;
            //        dtRqst.Rows.Add(dr);
            //    }

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RACK_EQP_STATUS_MB", "INDATA", "OUTDATA", dtRqst);
            //    if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
            //    {
            //        if (sStatus.ToString().Equals("UNUSE"))
            //        {
            //            Util.MessageValidation("FM_ME_0062");  //Rack 설정 변경에 실패하였습니다.\r\n입고 금지 Rack만 설정 가능합니다.
            //        }
            //        else
            //        {
            //            Util.MessageValidation("FM_ME_0061");  //Rack 설정 변경에 실패하였습니다.
            //        }
            //    }
            //    else
            //    {
            //        Util.MessageValidation("FM_ME_0063");  //Rack 설정 변경을 완료하였습니다.
            //        GetList();
            //    }

            //    GetList();
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }


        private void txtSearchTray_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSearchTray.Text.Length != 10)
                    {
                        Util.Alert("FM_ME_0071"); //Tray ID를 정확히 입력해주세요.
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["CSTID"] = txtSearchTray.Text;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_RETRIEVE_RACK_MB", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt.Rows.Count == 1)
                    {
                        if (Util.GetCondition(cboStockerType).ToString().Equals(dtRslt.Rows[0]["AGING_TYPE"].ToString()) && Util.GetCondition(cboRow).ToString().Equals(dtRslt.Rows[0]["ROW_NO"].ToString()))
                        {
                            for (int iRow = 0; iRow < dgStockerRack.Rows.Count; iRow++)
                            {
                                for (int iCol = 0; iCol < dgStockerRack.Columns.Count; iCol++)
                                {
                                    if (iRow > 0 && iCol > 0)
                                    {
                                        if (dgStockerRack.GetCell(iRow, iCol) == null ||
                                            dgStockerRack.GetCell(iRow, iCol).Presenter == null ||
                                            dgStockerRack.GetCell(iRow, iCol).Presenter.Tag == null) continue;

                                        string sEqp = Util.NVC(dgStockerRack.GetCell(iRow, iCol).Presenter.Tag.ToString());

                                        if (dtRslt.Rows[0]["EQPTID"].ToString().Equals(sEqp))
                                        {
                                            dgStockerRack.CurrentCell = dgStockerRack.GetCell(iRow, iCol);
                                            MouseButtonEventArgs mb = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);

                                            dgStockerRack.Selection.Clear();
                                            dgStockerRack.ScrollIntoView(iRow, iCol);
                                            dgStockerRack.GetCell(iRow, iCol).Presenter.IsSelected = true;

                                            C1.WPF.DataGrid.DataGridCell cell = dgStockerRack.GetCell(iRow, iCol);


                                            if (cell != null)
                                            {
                                                GetInfoToCell(cell);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //검색하신 Tray ID: {0}는\r\n Aging : {1}\r\n 열 : {2}\r\n 연 : {3} \r\n 단 : {4}에 입고되어 있습니다.
                            Util.MessageValidation("FM_ME_0322", new string[] { txtSearchTray.Text, dtRslt.Rows[0]["AGING_TYPE_NAME"].ToString(), dtRslt.Rows[0]["ROW_NO"].ToString(), dtRslt.Rows[0]["COL_NO"].ToString(), dtRslt.Rows[0]["STAGE_NO"].ToString() });
                        }
                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0260"); //해당 Tray ID가 존재하지 않습니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //try
            //{
            //    if (e.Key == Key.Enter)
            //    {
            //        if (txtSearchTray.Text.Length != 10)
            //        {
            //            Util.Alert("FM_ME_0071"); //Tray ID를 정확히 입력해주세요.
            //            return;
            //        }

            //        DataTable dtRqst = new DataTable();
            //        dtRqst.TableName = "RQSTDT";
            //        dtRqst.Columns.Add("LANGID", typeof(string));
            //        dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
            //        dtRqst.Columns.Add("AREAID", typeof(string));
            //        dtRqst.Columns.Add("CSTID", typeof(string));

            //        DataRow dr = dtRqst.NewRow();
            //        dr["LANGID"] = LoginInfo.LANGID;
            //        dr["COM_TYPE_CODE"] = "FORM_STOCKER_TYPE_CODE";
            //        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            //        dr["CSTID"] = txtSearchTray.Text;
            //        dtRqst.Rows.Add(dr);

            //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_RETRIEVE_RACK_MB", "RQSTDT", "RSLTDT", dtRqst);

            //        if (dtRslt.Rows.Count == 1)
            //        {
            //            if (Util.GetCondition(cboStockerType).ToString().Equals(dtRslt.Rows[0]["AGING_TYPE"].ToString()) && Util.GetCondition(cboRow).ToString().Equals(dtRslt.Rows[0]["ROW_NO"].ToString()))
            //            {
            //                for (int iRow = 0; iRow < dgStockerRack.Rows.Count; iRow++)
            //                {
            //                    for (int iCol = 0; iCol < dgStockerRack.Columns.Count; iCol++)
            //                    {
            //                        if (iRow > 0 && iCol > 0)
            //                        {
            //                            if (dgStockerRack.GetCell(iRow, iCol) == null ||
            //                                dgStockerRack.GetCell(iRow, iCol).Presenter == null ||
            //                                dgStockerRack.GetCell(iRow, iCol).Presenter.Tag == null) continue;

            //                            string sEqp = Util.NVC(dgStockerRack.GetCell(iRow, iCol).Presenter.Tag.ToString());

            //                            if (dtRslt.Rows[0]["EQPTID"].ToString().Equals(sEqp))
            //                            {
            //                                dgStockerRack.CurrentCell = dgStockerRack.GetCell(iRow, iCol);
            //                                MouseButtonEventArgs mb = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);

            //                                dgStockerRack.Selection.Clear();
            //                                dgStockerRack.ScrollIntoView(iRow, iCol);
            //                                dgStockerRack.GetCell(iRow, iCol).Presenter.IsSelected = true;

            //                                C1.WPF.DataGrid.DataGridCell cell = dgStockerRack.GetCell(iRow, iCol);


            //                                if (cell != null)
            //                                {
            //                                    GetInfoToCell(cell);
            //                                }

            //                                break;
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                //검색하신 Tray ID: {0}는\r\n Aging : {1}\r\n 열 : {2}\r\n 연 : {3} \r\n 단 : {4}에 입고되어 있습니다.
            //                Util.MessageValidation("FM_ME_0322", new string[] { txtSearchTray.Text, dtRslt.Rows[0]["AGING_TYPE_NAME"].ToString(), dtRslt.Rows[0]["ROW_NO"].ToString(), dtRslt.Rows[0]["COL_NO"].ToString(), dtRslt.Rows[0]["STAGE_NO"].ToString() });
            //            }
            //        }
            //        else
            //        {
            //            Util.MessageValidation("FM_ME_0260"); //해당 Tray ID가 존재하지 않습니다.
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }
        #endregion

        #region Method
        private void GetLane(ref string sLane)
        {
            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                
                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_STOCKER_TYPE_CODE";
                dr["COM_CODE"] = cboStockerType.SelectedValue.ToString();
                dr["USE_FLAG"] = 'Y';
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                sLane = dtResult.Rows[0]["ATTR2"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetEqpInfo(string sRackID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["RACK_ID"] = sRackID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_STOCKER_RACK_RETRIEVE_DTL_INFO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    txtTray1.Text = dtRslt.Rows[0]["CSTID"].ToString();
                    txtModel.Text = dtRslt.Rows[0]["PROD_LOTID"].ToString();
                    txtRoute.Text = dtRslt.Rows[0]["ROUTID"].ToString();
                    txtInDate.Text = dtRslt.Rows[0]["STARTTIME"].ToString();
                    txtScheduleDate.Text = dtRslt.Rows[0]["PLANTIME"].ToString();
                }
                if (dtRslt.Rows.Count > 1)
                {
                    txtTray2.Text = dtRslt.Rows[1]["CSTID"].ToString();
                }
                if (dtRslt.Rows.Count > 2)
                {
                    txtTray3.Text = dtRslt.Rows[2]["CSTID"].ToString();
                }
                if (dtRslt.Rows.Count > 3)
                {
                    txtTray4.Text = dtRslt.Rows[3]["CSTID"].ToString();
                }
                if (dtRslt.Rows.Count > 4)
                {
                    txtTray5.Text = dtRslt.Rows[4]["CSTID"].ToString();
                }
                if (dtRslt.Rows.Count > 5)
                {
                    txtTray6.Text = dtRslt.Rows[5]["CSTID"].ToString();
                }
                if (dtRslt.Rows.Count > 6)
                {
                    txtTray7.Text = dtRslt.Rows[6]["CSTID"].ToString();
                }
                if (dtRslt.Rows.Count > 7)
                {
                    txtTray8.Text = dtRslt.Rows[7]["CSTID"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetDtRowValue(string iRowNum, string sFindCol)
        {
            string sRtnValue = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(iRowNum)) return sRtnValue;

                if (dtTemp == null)
                {
                    return sRtnValue;
                }

                if (!dtTemp.Columns.Contains(sFindCol))
                {
                    return sRtnValue;
                }

                if (int.Parse(iRowNum) >= dtTemp.Rows.Count)
                {
                    return sRtnValue;
                }

                sRtnValue = dtTemp.Rows[int.Parse(iRowNum)][sFindCol].ToString();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtnValue;

            //string sRtnValue = string.Empty;
            //try
            //{
            //    if (int.Parse(iRowNum) >= _dtDATA.Rows.Count)
            //        return sRtnValue;

            //    sRtnValue = _dtDATA.Rows[int.Parse(iRowNum)][sFindCol].ToString();
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
            //return sRtnValue;
        }

        private void GetList()
        {
            try
            {
                ClearControlHeader();
                ClearControlRack();

                dgStockerRack.Columns.Clear();
                dgStockerRack.Refresh();
                _dtCopy = null;

                if (string.IsNullOrEmpty(Util.NVC(cboSCLine.SelectedValue)))
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("SC_LINE"));
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(cboRow.SelectedValue)))
                {
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("열"));
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = LANE_ID;
                dr["EQPT_GR_TYPE_CODE"] = EQPT_GR_TYPE_CODE;
                dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow);
                dr["EQPTID"] = Util.GetCondition(cboSCLine);
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);


                DataSet bizResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_STOCKER_RACK_RETRIEVE_MB", "INDATA", "RACK1,RACK2,CHARGE", dsRqst);

                if (bizResult.Tables.Count == 0)
                    return;


                if (bizResult.Tables["RACK1"].Rows.Count > 0 && bizResult.Tables["CHARGE"].Rows.Count > 0)
                {
                    txtTotalUseRate.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["RATE_ALL"]);   //전체사용률
                    txtRowUseRate.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["CHGRATE1"]);     //열 사용률


                    txtRackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKCOUNT"]);         //Rack 수
                    txt1RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_ONE"]);         //1단 Rack 수
                    txt2RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_TWO"]);         //2단 Rack 수
                    txt3RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_THREE"]);       //3단 Rack 수
                    txt4RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_FOUR"]);        //4단 Rack 수
                    txt5RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_FIVE"]);        //5단 Rack 수
                    txt6RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_SIX"]);         //6단 Rack 수
                    txt7RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_SEVEN"]);       //7단 Rack 수
                    txt8RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_EIGHT"]);       //8단 Rack 수

                    txtPossibleCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKIBGO"]);      //입고 가능 Rack 수
                    txtImpossibleCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKGEUMGI"]);  //입고 금지 Rack 수

                    //txtTroubleCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKTROUBLE"]);    //입고 이상 Rack 수
                    txtImpossible.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKGEUMGI_PER"]); //입고 금지 %
                                                                                                        //txtTrouble.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKTROUBLE_PER"]);   //입고 이상 %
                }

                dtTemp = null;
                if (bizResult.Tables["RACK1"].Rows.Count > 0)
                {
                    dtTemp = bizResult.Tables["RACK2"];

                    SetRackStatus1(dtTemp);
                }

                dgStockerRack.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                dgStockerRack.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        
         

            //try
            //{
            //    _dtDATA.Clear();
            //    _dtCopy.Clear();

            //    ClearControlHeader();
            //    ClearControlRack();
            //    _dtCopy = null;

            //    if (string.IsNullOrEmpty(Util.NVC(cboSCLine.SelectedValue)))
            //    {
            //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("SC_LINE"));
            //        return;
            //    }

            //    if (string.IsNullOrEmpty(Util.NVC(cboRow.SelectedValue)))
            //    {
            //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("열"));
            //        return;
            //    }

            //    DataTable dtRqst = new DataTable();
            //    dtRqst.TableName = "RQSTDT";
            //    dtRqst.Columns.Add("LANE_ID", typeof(string));
            //    dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
            //    dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
            //    dtRqst.Columns.Add("EQPTID", typeof(string));

            //    DataRow dr = dtRqst.NewRow();
            //    dr["LANE_ID"] = GetLane();
            //    dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow);
            //    dr["EQPT_GR_TYPE_CODE"] = EQPT_GR_TYPE_CODE;
            //    dr["EQPTID"] = Util.GetCondition(cboSCLine);

            //    dtRqst.Rows.Add(dr);

            //    DataSet dsRqst = new DataSet();
            //    dsRqst.Tables.Add(dtRqst);

            //    ShowLoadingIndicator();

            //    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_STOCKER_RACK_RETRIEVE_MB", "INDATA", "RACK1,RACK2,CHARGE", dsRqst);

            //    txtTotalUseRate.Text = dsRslt.Tables["CHARGE"].Rows[0]["RATE_ALL"].ToString();
            //    txtRowUseRate.Text = dsRslt.Tables["CHARGE"].Rows[0]["CHGRATE1"].ToString();

            //    txtRackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACKCOUNT"].ToString(); //Rack 수
            //    txtPossibleCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACKIBGO"].ToString();//입고 가능한 Rack 수
            //    txtImpossibleCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACKGEUMGI"].ToString();//입고 금지 Rack 수
            //    txt1RackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACK_ONE"].ToString();//1단 Rack 수
            //    txt2RackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACK_TWO"].ToString();//2단 Rack 수
            //    txt3RackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACK_THREE"].ToString();//3단 Rack 수
            //    txt4RackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACK_FOUR"].ToString();//4단 Rack 수
            //    txt5RackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACK_FIVE"].ToString();//5단 Rack 수
            //    txt6RackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACK_SIX"].ToString();//6단 Rack 수
            //    txt7RackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACK_SEVEN"].ToString();//7단 Rack 수
            //    txt8RackCnt.Text = dsRslt.Tables["Rack1"].Rows[0]["RACK_EIGHT"].ToString();//8단 Rack 수

            //    if (dsRslt.Tables["Rack2"].Rows.Count > 0)
            //        SetRackStatus1(dsRslt.Tables["Rack2"])
            //        ;

            //    //GetTestData(ref _dtDATA);
            //    //SetRackStatus(_dtDATA);

            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
            //finally
            //{
            //    HiddenLoadingIndicator();
            //}
        }
        

        private void ClearControlHeader()
        {
            txtTotalUseRate.Text = string.Empty; //전체사용률
            txtRowUseRate.Text = string.Empty; //열 사용률
            txtCol.Text = string.Empty; //연
            txtRackCnt.Text = string.Empty; //Rack 수
            txtPossibleCnt.Text = string.Empty;//입고 가능한 Rack 수
            txtImpossibleCnt.Text = string.Empty;//입고 금지 Rack 수
            txt1RackCnt.Text = string.Empty;//1단 Rack 수
            txt2RackCnt.Text = string.Empty;//2단 Rack 수
            txt3RackCnt.Text = string.Empty;//3단 Rack 수
            txt4RackCnt.Text = string.Empty;//1단 Rack 수
            txt5RackCnt.Text = string.Empty;//2단 Rack 수
            txt6RackCnt.Text = string.Empty;//3단 Rack 수
            txt7RackCnt.Text = string.Empty;//3단 Rack 수
            txt8RackCnt.Text = string.Empty;//3단 Rack 수

            dgStockerRack.ItemsSource = null;
            dgStockerRack.Columns.Clear();
            dgStockerRack.Refresh();
        }

        private void ClearControlRack()
        {

            txtTray1.Text = string.Empty; //1단
            txtTray2.Text = string.Empty; //2단
            txtTray3.Text = string.Empty; //3단
            txtTray4.Text = string.Empty; //4단
            txtTray5.Text = string.Empty; //5단
            txtTray6.Text = string.Empty; //6단
            txtTray7.Text = string.Empty; //7단
            txtTray8.Text = string.Empty; //8단
            txtModel.Text = string.Empty; //모델명
            txtRoute.Text = string.Empty; //route id
            txtInDate.Text = string.Empty; //입고일자
            txtScheduleDate.Text = string.Empty; //출고예정일자

            txtSelRow.Text = "";//열
            txtSelCol.Text = "";//연
            txtSelStg.Text = "";//단
        }

        private void SetRackStatus(DataTable dtRslt)
        {
            try
            {
                // 스프레드 초기화용 변수
                #region 스프레드 초기화용 변수
                int iMaxCol;
                int iMaxStg;
                int iRowCount;
                int iColumnCount;
                double dColumnWidth;
                double dRowHeight;
                #endregion

                #region 충방전기 데이터용 변수
                int iCOL;
                int iSTG;

                string RACK_ID = string.Empty;
                string TRAYCNT = string.Empty;
                string STATUS = string.Empty;
                string ROUTE_TYPE_CD = string.Empty;
                #endregion

                #region Grid 초기화
                iMaxCol = Convert.ToInt16(dtRslt.Compute("MAX(EQP_COL_LOC)", string.Empty));
                iMaxStg = Convert.ToInt16(dtRslt.Compute("MAX(EQP_STG_LOC)", string.Empty));

                //iRowCount = iMaxStg * 2;
                iRowCount = iMaxStg;
                iColumnCount = iMaxCol + 1;   //단 추가.

                //Row Height get
                dRowHeight = 45;

                //Datatable 정의
                DataTable Udt = new DataTable();

                //Column 생성
                for (int i = 0; i < iColumnCount; i++)
                {
                    //GRID Column Create
                    SetGridHeaderSingle((i + 1).ToString(), dgStockerRack, 70);
                    Udt.Columns.Add((i + 1).ToString(), typeof(string));
                }

                //Row 생성
                for (int i = 0; i < iRowCount; i++)
                {
                    DataRow Urow = Udt.NewRow();
                    Udt.Rows.Add(Urow);
                }

                //Row Header 생성
                int iMakeCol = 0;
                DataRow UrowHeader = Udt.NewRow();
                for (int i = 0; i < Udt.Columns.Count; i++)
                {
                    if (i == 0)
                        UrowHeader[i] = string.Empty;
                    else
                    {
                        iMakeCol++;

                        DataRow[] drRow = dtRslt.Select("EQP_STG_LOC = '01' AND EQP_COL_LOC = '" + iMakeCol.ToString("00") + "'");
                        string row = drRow[0][2].ToString();
                        UrowHeader[i] = row + ObjectDic.Instance.GetObjectName("연");
                    }
                }
                #endregion

                #region Data Setting
                //dtRslt Column Add
                dtRslt.Columns.Add("BCOLOR", typeof(string));
                dtRslt.Columns.Add("FCOLOR", typeof(string));
                dtRslt.Columns.Add("TEXT", typeof(string));

                for (int k = 0; k < dtRslt.Rows.Count; k++)
                {
                    iCOL = int.Parse(dtRslt.Rows[k]["EQP_COL_LOC"].ToString());
                    iSTG = int.Parse(dtRslt.Rows[k]["EQP_STG_LOC"].ToString());
                    RACK_ID = Util.NVC(dtRslt.Rows[k]["RACK_ID"].ToString());
                    TRAYCNT = Util.NVC(dtRslt.Rows[k]["TRAYCNT"].ToString());
                    STATUS = Util.NVC(dtRslt.Rows[k]["STATUS"].ToString());
                    ROUTE_TYPE_CD = Util.NVC(dtRslt.Rows[k]["ROUT_TYPE_CODE"].ToString());

                    Udt.Rows[iRowCount - iSTG][0] = iSTG.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
                    Udt.Rows[iRowCount - iSTG][iCOL] = k.ToString();

                    string BCOLOR = "Black";
                    string FCOLOR = "White";
                    string TEXT = string.Empty;

                       DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + STATUS + "'");

                    if (drColor.Length > 0)
                    {
                        BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
                        FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
                    }

                    switch (STATUS)
                    {
                        case "1": //정상 입고
                            if (ROUTE_TYPE_CD.Equals("C") || ROUTE_TYPE_CD.Equals("K") || ROUTE_TYPE_CD.Equals("U") || ROUTE_TYPE_CD.Equals("I"))
                            {
                                //용량 입고
                            }
                            else
                            {
                                //일반 입고
                                BCOLOR = drColor[0]["ATTRIBUTE3"].ToString();
                                FCOLOR = drColor[0]["ATTRIBUTE4"].ToString();
                            }
                            break;
                        case "7": //시간 초과
                            if (ROUTE_TYPE_CD.Equals("C") || ROUTE_TYPE_CD.Equals("K") || ROUTE_TYPE_CD.Equals("U") || ROUTE_TYPE_CD.Equals("I"))
                            {
                                //용량 시간 초과
                            }
                            else
                            {
                                //일반 시간 초과
                                BCOLOR = drColor[0]["ATTRIBUTE3"].ToString();
                                FCOLOR = drColor[0]["ATTRIBUTE4"].ToString();
                            }
                            break;
                    }

                    dtRslt.Rows[k]["BCOLOR"] = BCOLOR;
                    dtRslt.Rows[k]["FCOLOR"] = FCOLOR;
                    dtRslt.Rows[k]["TEXT"] = TRAYCNT;
                }
                #endregion

                #region Grid 조합
                //DataTable Header Insert
                Udt.Rows.InsertAt(UrowHeader, 0);

                _dtCopy = Udt.Copy();
                _dtDATA = dtRslt.Copy();

                dgStockerRack.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                dgStockerRack.ItemsSource = DataTableConverter.Convert(Udt);
                dgStockerRack.UpdateLayout();

                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgStockerRack.Columns)
                    dgc.VerticalAlignment = VerticalAlignment.Center;

                string[] sColumnName = new string[] { "1" };
                Util.SetDataGridMergeExtensionCol(dgStockerRack, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                #endregion

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetRackStatus1(DataTable dtRack)
        {
            try
            {
                _dtCopy = null;

                //Column Width get
                double dColumnWidth = 0;

                //Row Height get
                double dRowHeight = 0;

                int iMaxCol = 0;
                int iMaxStg = 0;

                for (int k = 0; k < dtRack.Rows.Count; k++)
                {
                    int iColNum = int.Parse(dtRack.Rows[k]["EQP_COL_LOC"].ToString());
                    int iStgNum = int.Parse(dtRack.Rows[k]["EQP_STG_LOC"].ToString());

                    if (iMaxCol < iColNum)
                    {
                        iMaxCol = iColNum;
                    }

                    if (iMaxStg < iStgNum)
                    {
                        iMaxStg = iStgNum;
                    }
                }

                if (iMaxCol == 0 || iMaxStg == 0) return;

                int iColCnt = (iMaxCol < 25) ? iMaxCol + 1 : Convert.ToInt32(Math.Ceiling(Convert.ToDouble(iMaxCol) / 2)) + 1; //20210402 Aging Rack 연 설정 부분 수정
                int iRowCnt = (iMaxCol < 25) ? iMaxStg : (iMaxStg * 2) + 2;

                //상단 Datatable, 하단 Datatable 정의
                DataTable Udt = new DataTable();
                DataTable Ddt = new DataTable();

                DataRow UrowHeader = Udt.NewRow();
                DataRow DrowHeader = Ddt.NewRow();

                if (iMaxCol < 25)
                {
                    //Column Width get
                    dColumnWidth = (dgStockerRack.ActualWidth - 50) / iMaxCol;
                    if (dColumnWidth > 50)
                    {
                        dColumnWidth = 150;
                    }

                    //Row Height get
                    dRowHeight = (dgStockerRack.ActualHeight) / iRowCnt;

                    //Column 생성
                    for (int i = 0; i < iColCnt; i++)
                    {
                        //GRID Column Create
                        if (i.Equals(0))
                        {
                            SetGridHeaderSingle((i + 1).ToString(), dgStockerRack, 50);
                            Udt.Columns.Add((i + 1).ToString(), typeof(string));
                        }
                        else
                        {
                            SetGridHeaderSingle((i + 1).ToString(), dgStockerRack, dColumnWidth);
                            Udt.Columns.Add((i + 1).ToString(), typeof(string));
                        }
                    }

                    //Row 생성
                    for (int i = 0; i < iRowCnt; i++)
                    {
                        DataRow Urow = Udt.NewRow();
                        Udt.Rows.Add(Urow);
                    }

                    //Row Header 생성
                    for (int i = 0; i < Udt.Columns.Count; i++)
                    {
                        if (i == 0)
                        {
                            UrowHeader[i] = string.Empty;
                        }
                        else
                        {
                            UrowHeader[i] = (i).ToString() + ObjectDic.Instance.GetObjectName("연");
                        }
                    }

                    for (int k = 0; k < dtRack.Rows.Count; k++)
                    {
                        int iColNum = int.Parse(dtRack.Rows[k]["EQP_COL_LOC"].ToString());
                        int iStgNum = int.Parse(dtRack.Rows[k]["EQP_STG_LOC"].ToString());

                        Udt.Rows[iRowCnt - iStgNum][0] = iStgNum.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
                        Udt.Rows[iRowCnt - iStgNum][iColNum] = k.ToString();
                    }
                }
                else
                {
                    //Column Width get
                    dColumnWidth = (dgStockerRack.ActualWidth - 96) / (iMaxCol / 2);

                    //Row Height get
                    dRowHeight = (dgStockerRack.ActualHeight) / iRowCnt; //20210402 Aging Rack 연 설정 부분 수정

                    for (int i = 0; i < iColCnt; i++)
                    {
                        //GRID Column Create
                        SetGridHeaderSingle((i + 1).ToString(), dgStockerRack, dColumnWidth);
                        Udt.Columns.Add((i + 1).ToString(), typeof(string));
                        Ddt.Columns.Add((i + 1).ToString(), typeof(string));
                    }

                    //Row 생성
                    for (int i = 0; i < iMaxStg; i++)
                    {
                        DataRow Urow = Udt.NewRow();
                        Udt.Rows.Add(Urow);

                        DataRow Drow = Ddt.NewRow();
                        Ddt.Rows.Add(Drow);
                    }

                    //Row Header 생성
                    for (int i = 0; i < Udt.Columns.Count; i++)
                    {
                        if (i == 0)
                            UrowHeader[i] = string.Empty;
                        else
                            UrowHeader[i] = (i).ToString() + ObjectDic.Instance.GetObjectName("연");
                    }

                    //Row Header 생성
                    for (int i = 0; i < Ddt.Columns.Count; i++)
                    {
                        if (i == 0)
                            DrowHeader[i] = string.Empty;
                        else
                            DrowHeader[i] = Convert.ToString(Convert.ToInt32(Math.Ceiling(Convert.ToDouble(iMaxCol) / 2)) + i) + ObjectDic.Instance.GetObjectName("연"); //20210402 Aging Rack 연 설정 부분 수정
                    }

                    for (int k = 0; k < dtRack.Rows.Count; k++)
                    {
                        int iColNum = int.Parse(dtRack.Rows[k]["EQP_COL_LOC"].ToString());
                        int iStgNum = int.Parse(dtRack.Rows[k]["EQP_STG_LOC"].ToString());

                        if (iColNum < iColCnt)
                        {
                            Udt.Rows[iMaxStg - iStgNum][0] = iStgNum.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
                            Udt.Rows[iMaxStg - iStgNum][iColNum] = k.ToString();
                        }
                        else
                        {
                            Ddt.Rows[iMaxStg - iStgNum][0] = iStgNum.ToString() + ObjectDic.Instance.GetObjectName("단");  //단
                            Ddt.Rows[iMaxStg - iStgNum][iColNum - Convert.ToInt32(Math.Ceiling(Convert.ToDouble(iMaxCol) / 2))] = k.ToString(); //20210402 Aging Rack 연 설정 부분 수정
                        }
                    }

                }

                //dtRslt Column Add
                dtRack.Columns.Add("BCOLOR", typeof(string));
                dtRack.Columns.Add("FCOLOR", typeof(string));
                dtRack.Columns.Add("TEXT", typeof(string));
                dtRack.Columns.Add("RACKID", typeof(string));

                string EQPTID = string.Empty;
                int iCOL;
                int iSTG;

                string RACK_ID = string.Empty;
                string STATUS = string.Empty;
                string TRAYCNT = string.Empty;

                for (int k = 0; k < dtRack.Rows.Count; k++)
                {
                    iCOL = int.Parse(dtRack.Rows[k]["EQP_COL_LOC"].ToString());
                    iSTG = int.Parse(dtRack.Rows[k]["EQP_STG_LOC"].ToString());

                    RACK_ID = Util.NVC(dtRack.Rows[k]["RACK_ID"].ToString());
                    STATUS = Util.NVC(dtRack.Rows[k]["STATUS"].ToString());
                    TRAYCNT = Util.NVC(dtRack.Rows[k]["TRAYCNT"].ToString());

                    string BCOLOR = "Black";
                    string FCOLOR = "White";
                    string TEXT = string.Empty;

                    DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + STATUS + "'");

                    if (drColor.Length > 0)
                    {
                        BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
                        FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
                    }

                    dtRack.Rows[k]["BCOLOR"] = BCOLOR;
                    dtRack.Rows[k]["FCOLOR"] = FCOLOR;
                    dtRack.Rows[k]["TEXT"] = TRAYCNT;
                    dtRack.Rows[k]["RACKID"] = RACK_ID;
                }
                //DataTable Header Insert
                Udt.Rows.InsertAt(UrowHeader, 0);
                if (iMaxCol >= 25)
                {
                    Ddt.Rows.InsertAt(DrowHeader, 0);
                }

                //상,하 Merge
                Udt.Merge(Ddt, false, MissingSchemaAction.Add);
                _dtCopy = Udt.Copy();

                dgStockerRack.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                dgStockerRack.ItemsSource = DataTableConverter.Convert(Udt);
                dgStockerRack.UpdateLayout();

                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgStockerRack.Columns)
                    dgc.VerticalAlignment = VerticalAlignment.Center;

                for (int row = 0; row < dgStockerRack.Rows.Count; row++)
                {
                    for (int col = 0; col < dgStockerRack.Columns.Count; col++)
                    {
                        if (dgStockerRack.GetCell(row, col).Presenter == null) continue;

                        dgStockerRack.GetCell(row, col).Presenter.Padding = new Thickness(0);
                        dgStockerRack.GetCell(row, col).Presenter.Margin = new Thickness(0);
                    }
                }

              
               
                    dgStockerRack.FontSize = 12;
                    dgStockerRack.AllColumnsWidthAuto();
                    dgStockerRack.UpdateLayout();

                    double totalColWidth = dgStockerRack.Columns.Sum(x => x.Visibility != Visibility.Visible ? 0 : x.ActualWidth);

                    if (totalColWidth < dgStockerRack.ActualWidth - 70)
                    {
                        for (int col = 0; col < dgStockerRack.Columns.Count; col++)
                        {
                            if (dgStockerRack.Columns[col].Visibility != Visibility.Visible) continue;

                            dgStockerRack.Columns[col].Width = new C1.WPF.DataGrid.DataGridLength(dColumnWidth);
                        }
                    }
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
            });
        }

        private void GetTestData(ref DataTable dt)
        {
            //_dtDATA
            //dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("RACK_ID", typeof(string));
            dt.Columns.Add("EQP_STG_LOC", typeof(int));
            dt.Columns.Add("EQP_COL_LOC", typeof(int));
            dt.Columns.Add("EQP_STATUS_CD", typeof(string));
            dt.Columns.Add("EQP_OP_STATUS_CD", typeof(string));
            dt.Columns.Add("STATUS", typeof(string));
            dt.Columns.Add("PLANTIME", typeof(string));
            dt.Columns.Add("TRAYCNT", typeof(string));
            dt.Columns.Add("RACK_STAT_CODE", typeof(string));
            dt.Columns.Add("ROUT_TYPE_CODE", typeof(string));

            DataRow row1 = dt.NewRow(); row1["RACK_ID"] = "511S010103"; row1["EQP_STG_LOC"] = "3"; row1["EQP_COL_LOC"] = "1"; row1["EQP_STATUS_CD"] = "T"; row1["EQP_OP_STATUS_CD"] = "D"; row1["STATUS"] = "5"; row1["PLANTIME"] = ""; row1["TRAYCNT"] = ""; row1["RACK_STAT_CODE"] = ""; row1["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow(); row2["RACK_ID"] = "511S010203"; row2["EQP_STG_LOC"] = "3"; row2["EQP_COL_LOC"] = "2"; row2["EQP_STATUS_CD"] = "I"; row2["EQP_OP_STATUS_CD"] = "G"; row2["STATUS"] = "2"; row2["PLANTIME"] = ""; row2["TRAYCNT"] = ""; row2["RACK_STAT_CODE"] = ""; row2["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow(); row3["RACK_ID"] = "511S010303"; row3["EQP_STG_LOC"] = "3"; row3["EQP_COL_LOC"] = "3"; row3["EQP_STATUS_CD"] = "R"; row3["EQP_OP_STATUS_CD"] = "G"; row3["STATUS"] = "1"; row3["PLANTIME"] = ""; row3["TRAYCNT"] = "6"; row3["RACK_STAT_CODE"] = ""; row3["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow(); row4["RACK_ID"] = "511S010403"; row4["EQP_STG_LOC"] = "3"; row4["EQP_COL_LOC"] = "4"; row4["EQP_STATUS_CD"] = "I"; row4["EQP_OP_STATUS_CD"] = "G"; row4["STATUS"] = "2"; row4["PLANTIME"] = ""; row4["TRAYCNT"] = ""; row4["RACK_STAT_CODE"] = ""; row4["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow(); row5["RACK_ID"] = "511S010503"; row5["EQP_STG_LOC"] = "3"; row5["EQP_COL_LOC"] = "5"; row5["EQP_STATUS_CD"] = "I"; row5["EQP_OP_STATUS_CD"] = "G"; row5["STATUS"] = "2"; row5["PLANTIME"] = ""; row5["TRAYCNT"] = ""; row5["RACK_STAT_CODE"] = ""; row5["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow(); row6["RACK_ID"] = "511S010603"; row6["EQP_STG_LOC"] = "3"; row6["EQP_COL_LOC"] = "6"; row6["EQP_STATUS_CD"] = "I"; row6["EQP_OP_STATUS_CD"] = "G"; row6["STATUS"] = "2"; row6["PLANTIME"] = ""; row6["TRAYCNT"] = ""; row6["RACK_STAT_CODE"] = ""; row6["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow(); row7["RACK_ID"] = "511S010703"; row7["EQP_STG_LOC"] = "3"; row7["EQP_COL_LOC"] = "7"; row7["EQP_STATUS_CD"] = "I"; row7["EQP_OP_STATUS_CD"] = "G"; row7["STATUS"] = "2"; row7["PLANTIME"] = ""; row7["TRAYCNT"] = ""; row7["RACK_STAT_CODE"] = ""; row7["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row7);
            DataRow row8 = dt.NewRow(); row8["RACK_ID"] = "511S010803"; row8["EQP_STG_LOC"] = "3"; row8["EQP_COL_LOC"] = "8"; row8["EQP_STATUS_CD"] = "I"; row8["EQP_OP_STATUS_CD"] = "G"; row8["STATUS"] = "2"; row8["PLANTIME"] = ""; row8["TRAYCNT"] = ""; row8["RACK_STAT_CODE"] = ""; row8["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row8);
            DataRow row9 = dt.NewRow(); row9["RACK_ID"] = "511S010903"; row9["EQP_STG_LOC"] = "3"; row9["EQP_COL_LOC"] = "9"; row9["EQP_STATUS_CD"] = "R"; row9["EQP_OP_STATUS_CD"] = "G"; row9["STATUS"] = "7"; row9["PLANTIME"] = ""; row9["TRAYCNT"] = "6"; row9["RACK_STAT_CODE"] = ""; row9["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row9);
            DataRow row10 = dt.NewRow(); row10["RACK_ID"] = "511S011003"; row10["EQP_STG_LOC"] = "3"; row10["EQP_COL_LOC"] = "10"; row10["EQP_STATUS_CD"] = "I"; row10["EQP_OP_STATUS_CD"] = "G"; row10["STATUS"] = "2"; row10["PLANTIME"] = ""; row10["TRAYCNT"] = ""; row10["RACK_STAT_CODE"] = ""; row10["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row10);
            DataRow row11 = dt.NewRow(); row11["RACK_ID"] = "511S011103"; row11["EQP_STG_LOC"] = "3"; row11["EQP_COL_LOC"] = "11"; row11["EQP_STATUS_CD"] = "I"; row11["EQP_OP_STATUS_CD"] = "G"; row11["STATUS"] = "2"; row11["PLANTIME"] = ""; row11["TRAYCNT"] = ""; row11["RACK_STAT_CODE"] = ""; row11["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row11);
            DataRow row12 = dt.NewRow(); row12["RACK_ID"] = "511S011203"; row12["EQP_STG_LOC"] = "3"; row12["EQP_COL_LOC"] = "12"; row12["EQP_STATUS_CD"] = "R"; row12["EQP_OP_STATUS_CD"] = "G"; row12["STATUS"] = "1"; row12["PLANTIME"] = ""; row12["TRAYCNT"] = "6"; row12["RACK_STAT_CODE"] = ""; row12["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row12);
            DataRow row13 = dt.NewRow(); row13["RACK_ID"] = "511S011303"; row13["EQP_STG_LOC"] = "3"; row13["EQP_COL_LOC"] = "13"; row13["EQP_STATUS_CD"] = "R"; row13["EQP_OP_STATUS_CD"] = "G"; row13["STATUS"] = "7"; row13["PLANTIME"] = ""; row13["TRAYCNT"] = "6"; row13["RACK_STAT_CODE"] = ""; row13["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row13);
            DataRow row14 = dt.NewRow(); row14["RACK_ID"] = "511S011403"; row14["EQP_STG_LOC"] = "3"; row14["EQP_COL_LOC"] = "14"; row14["EQP_STATUS_CD"] = "R"; row14["EQP_OP_STATUS_CD"] = "G"; row14["STATUS"] = "7"; row14["PLANTIME"] = ""; row14["TRAYCNT"] = "6"; row14["RACK_STAT_CODE"] = ""; row14["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row14);
            DataRow row15 = dt.NewRow(); row15["RACK_ID"] = "511S011503"; row15["EQP_STG_LOC"] = "3"; row15["EQP_COL_LOC"] = "15"; row15["EQP_STATUS_CD"] = "I"; row15["EQP_OP_STATUS_CD"] = "G"; row15["STATUS"] = "2"; row15["PLANTIME"] = ""; row15["TRAYCNT"] = ""; row15["RACK_STAT_CODE"] = ""; row15["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row15);
            DataRow row16 = dt.NewRow(); row16["RACK_ID"] = "511S011603"; row16["EQP_STG_LOC"] = "3"; row16["EQP_COL_LOC"] = "16"; row16["EQP_STATUS_CD"] = "I"; row16["EQP_OP_STATUS_CD"] = "G"; row16["STATUS"] = "2"; row16["PLANTIME"] = ""; row16["TRAYCNT"] = ""; row16["RACK_STAT_CODE"] = ""; row16["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row16);
            DataRow row17 = dt.NewRow(); row17["RACK_ID"] = "511S011703"; row17["EQP_STG_LOC"] = "3"; row17["EQP_COL_LOC"] = "17"; row17["EQP_STATUS_CD"] = "I"; row17["EQP_OP_STATUS_CD"] = "G"; row17["STATUS"] = "2"; row17["PLANTIME"] = ""; row17["TRAYCNT"] = ""; row17["RACK_STAT_CODE"] = ""; row17["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row17);
            DataRow row18 = dt.NewRow(); row18["RACK_ID"] = "511S011803"; row18["EQP_STG_LOC"] = "3"; row18["EQP_COL_LOC"] = "18"; row18["EQP_STATUS_CD"] = "I"; row18["EQP_OP_STATUS_CD"] = "G"; row18["STATUS"] = "2"; row18["PLANTIME"] = ""; row18["TRAYCNT"] = ""; row18["RACK_STAT_CODE"] = ""; row18["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row18);
            DataRow row19 = dt.NewRow(); row19["RACK_ID"] = "511S011903"; row19["EQP_STG_LOC"] = "3"; row19["EQP_COL_LOC"] = "19"; row19["EQP_STATUS_CD"] = "I"; row19["EQP_OP_STATUS_CD"] = "G"; row19["STATUS"] = "2"; row19["PLANTIME"] = ""; row19["TRAYCNT"] = ""; row19["RACK_STAT_CODE"] = ""; row19["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row19);
            DataRow row20 = dt.NewRow(); row20["RACK_ID"] = "511S012003"; row20["EQP_STG_LOC"] = "3"; row20["EQP_COL_LOC"] = "20"; row20["EQP_STATUS_CD"] = "I"; row20["EQP_OP_STATUS_CD"] = "G"; row20["STATUS"] = "2"; row20["PLANTIME"] = ""; row20["TRAYCNT"] = ""; row20["RACK_STAT_CODE"] = ""; row20["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row20);
            DataRow row21 = dt.NewRow(); row21["RACK_ID"] = "511S012103"; row21["EQP_STG_LOC"] = "3"; row21["EQP_COL_LOC"] = "21"; row21["EQP_STATUS_CD"] = "I"; row21["EQP_OP_STATUS_CD"] = "G"; row21["STATUS"] = "2"; row21["PLANTIME"] = ""; row21["TRAYCNT"] = ""; row21["RACK_STAT_CODE"] = ""; row21["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row21);
            DataRow row22 = dt.NewRow(); row22["RACK_ID"] = "511S012203"; row22["EQP_STG_LOC"] = "3"; row22["EQP_COL_LOC"] = "22"; row22["EQP_STATUS_CD"] = "I"; row22["EQP_OP_STATUS_CD"] = "G"; row22["STATUS"] = "2"; row22["PLANTIME"] = ""; row22["TRAYCNT"] = ""; row22["RACK_STAT_CODE"] = ""; row22["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row22);
            DataRow row23 = dt.NewRow(); row23["RACK_ID"] = "511S012303"; row23["EQP_STG_LOC"] = "3"; row23["EQP_COL_LOC"] = "23"; row23["EQP_STATUS_CD"] = "R"; row23["EQP_OP_STATUS_CD"] = "G"; row23["STATUS"] = "1"; row23["PLANTIME"] = ""; row23["TRAYCNT"] = "6"; row23["RACK_STAT_CODE"] = ""; row23["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row23);
            DataRow row24 = dt.NewRow(); row24["RACK_ID"] = "511S012403"; row24["EQP_STG_LOC"] = "3"; row24["EQP_COL_LOC"] = "24"; row24["EQP_STATUS_CD"] = "R"; row24["EQP_OP_STATUS_CD"] = "G"; row24["STATUS"] = "1"; row24["PLANTIME"] = ""; row24["TRAYCNT"] = "6"; row24["RACK_STAT_CODE"] = ""; row24["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row24);
            DataRow row25 = dt.NewRow(); row25["RACK_ID"] = "511S012503"; row25["EQP_STG_LOC"] = "3"; row25["EQP_COL_LOC"] = "25"; row25["EQP_STATUS_CD"] = "R"; row25["EQP_OP_STATUS_CD"] = "G"; row25["STATUS"] = "1"; row25["PLANTIME"] = ""; row25["TRAYCNT"] = "6"; row25["RACK_STAT_CODE"] = ""; row25["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row25);
            DataRow row26 = dt.NewRow(); row26["RACK_ID"] = "511S012603"; row26["EQP_STG_LOC"] = "3"; row26["EQP_COL_LOC"] = "26"; row26["EQP_STATUS_CD"] = "R"; row26["EQP_OP_STATUS_CD"] = "G"; row26["STATUS"] = "1"; row26["PLANTIME"] = ""; row26["TRAYCNT"] = "6"; row26["RACK_STAT_CODE"] = ""; row26["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row26);
            DataRow row27 = dt.NewRow(); row27["RACK_ID"] = "511S012703"; row27["EQP_STG_LOC"] = "3"; row27["EQP_COL_LOC"] = "27"; row27["EQP_STATUS_CD"] = "I"; row27["EQP_OP_STATUS_CD"] = "G"; row27["STATUS"] = "2"; row27["PLANTIME"] = ""; row27["TRAYCNT"] = ""; row27["RACK_STAT_CODE"] = ""; row27["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row27);
            DataRow row28 = dt.NewRow(); row28["RACK_ID"] = "511S012803"; row28["EQP_STG_LOC"] = "3"; row28["EQP_COL_LOC"] = "28"; row28["EQP_STATUS_CD"] = "R"; row28["EQP_OP_STATUS_CD"] = "G"; row28["STATUS"] = "1"; row28["PLANTIME"] = ""; row28["TRAYCNT"] = "6"; row28["RACK_STAT_CODE"] = ""; row28["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row28);
            DataRow row29 = dt.NewRow(); row29["RACK_ID"] = "511S012903"; row29["EQP_STG_LOC"] = "3"; row29["EQP_COL_LOC"] = "29"; row29["EQP_STATUS_CD"] = "R"; row29["EQP_OP_STATUS_CD"] = "G"; row29["STATUS"] = "1"; row29["PLANTIME"] = ""; row29["TRAYCNT"] = "6"; row29["RACK_STAT_CODE"] = ""; row29["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row29);
            DataRow row30 = dt.NewRow(); row30["RACK_ID"] = "511S013003"; row30["EQP_STG_LOC"] = "3"; row30["EQP_COL_LOC"] = "30"; row30["EQP_STATUS_CD"] = "R"; row30["EQP_OP_STATUS_CD"] = "G"; row30["STATUS"] = "1"; row30["PLANTIME"] = ""; row30["TRAYCNT"] = "6"; row30["RACK_STAT_CODE"] = ""; row30["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row30);
            DataRow row31 = dt.NewRow(); row31["RACK_ID"] = "511S013103"; row31["EQP_STG_LOC"] = "3"; row31["EQP_COL_LOC"] = "31"; row31["EQP_STATUS_CD"] = "R"; row31["EQP_OP_STATUS_CD"] = "G"; row31["STATUS"] = "1"; row31["PLANTIME"] = ""; row31["TRAYCNT"] = "6"; row31["RACK_STAT_CODE"] = ""; row31["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row31);
            DataRow row32 = dt.NewRow(); row32["RACK_ID"] = "511S013203"; row32["EQP_STG_LOC"] = "3"; row32["EQP_COL_LOC"] = "32"; row32["EQP_STATUS_CD"] = "I"; row32["EQP_OP_STATUS_CD"] = "G"; row32["STATUS"] = "2"; row32["PLANTIME"] = ""; row32["TRAYCNT"] = ""; row32["RACK_STAT_CODE"] = ""; row32["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row32);
            DataRow row33 = dt.NewRow(); row33["RACK_ID"] = "511S013303"; row33["EQP_STG_LOC"] = "3"; row33["EQP_COL_LOC"] = "33"; row33["EQP_STATUS_CD"] = "I"; row33["EQP_OP_STATUS_CD"] = "G"; row33["STATUS"] = "2"; row33["PLANTIME"] = ""; row33["TRAYCNT"] = ""; row33["RACK_STAT_CODE"] = ""; row33["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row33);
            DataRow row34 = dt.NewRow(); row34["RACK_ID"] = "511S013403"; row34["EQP_STG_LOC"] = "3"; row34["EQP_COL_LOC"] = "34"; row34["EQP_STATUS_CD"] = "I"; row34["EQP_OP_STATUS_CD"] = "G"; row34["STATUS"] = "2"; row34["PLANTIME"] = ""; row34["TRAYCNT"] = ""; row34["RACK_STAT_CODE"] = ""; row34["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row34);
            DataRow row35 = dt.NewRow(); row35["RACK_ID"] = "511S013503"; row35["EQP_STG_LOC"] = "3"; row35["EQP_COL_LOC"] = "35"; row35["EQP_STATUS_CD"] = "I"; row35["EQP_OP_STATUS_CD"] = "G"; row35["STATUS"] = "2"; row35["PLANTIME"] = ""; row35["TRAYCNT"] = ""; row35["RACK_STAT_CODE"] = ""; row35["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row35);
            DataRow row36 = dt.NewRow(); row36["RACK_ID"] = "511S013603"; row36["EQP_STG_LOC"] = "3"; row36["EQP_COL_LOC"] = "36"; row36["EQP_STATUS_CD"] = "I"; row36["EQP_OP_STATUS_CD"] = "G"; row36["STATUS"] = "2"; row36["PLANTIME"] = ""; row36["TRAYCNT"] = ""; row36["RACK_STAT_CODE"] = ""; row36["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row36);
            DataRow row37 = dt.NewRow(); row37["RACK_ID"] = "511S013703"; row37["EQP_STG_LOC"] = "3"; row37["EQP_COL_LOC"] = "37"; row37["EQP_STATUS_CD"] = "I"; row37["EQP_OP_STATUS_CD"] = "G"; row37["STATUS"] = "2"; row37["PLANTIME"] = ""; row37["TRAYCNT"] = ""; row37["RACK_STAT_CODE"] = ""; row37["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row37);
            DataRow row38 = dt.NewRow(); row38["RACK_ID"] = "511S013803"; row38["EQP_STG_LOC"] = "3"; row38["EQP_COL_LOC"] = "38"; row38["EQP_STATUS_CD"] = "I"; row38["EQP_OP_STATUS_CD"] = "G"; row38["STATUS"] = "2"; row38["PLANTIME"] = ""; row38["TRAYCNT"] = ""; row38["RACK_STAT_CODE"] = ""; row38["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row38);
            DataRow row39 = dt.NewRow(); row39["RACK_ID"] = "511S013903"; row39["EQP_STG_LOC"] = "3"; row39["EQP_COL_LOC"] = "39"; row39["EQP_STATUS_CD"] = "I"; row39["EQP_OP_STATUS_CD"] = "G"; row39["STATUS"] = "2"; row39["PLANTIME"] = ""; row39["TRAYCNT"] = ""; row39["RACK_STAT_CODE"] = ""; row39["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row39);
            DataRow row40 = dt.NewRow(); row40["RACK_ID"] = "511S014003"; row40["EQP_STG_LOC"] = "3"; row40["EQP_COL_LOC"] = "40"; row40["EQP_STATUS_CD"] = "R"; row40["EQP_OP_STATUS_CD"] = "G"; row40["STATUS"] = "7"; row40["PLANTIME"] = ""; row40["TRAYCNT"] = "6"; row40["RACK_STAT_CODE"] = ""; row40["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row40);
            DataRow row41 = dt.NewRow(); row41["RACK_ID"] = "511S010102"; row41["EQP_STG_LOC"] = "2"; row41["EQP_COL_LOC"] = "1"; row41["EQP_STATUS_CD"] = "R"; row41["EQP_OP_STATUS_CD"] = "G"; row41["STATUS"] = "7"; row41["PLANTIME"] = ""; row41["TRAYCNT"] = "5"; row41["RACK_STAT_CODE"] = ""; row41["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row41);
            DataRow row42 = dt.NewRow(); row42["RACK_ID"] = "511S010202"; row42["EQP_STG_LOC"] = "2"; row42["EQP_COL_LOC"] = "2"; row42["EQP_STATUS_CD"] = "I"; row42["EQP_OP_STATUS_CD"] = "G"; row42["STATUS"] = "2"; row42["PLANTIME"] = ""; row42["TRAYCNT"] = ""; row42["RACK_STAT_CODE"] = ""; row42["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row42);
            DataRow row43 = dt.NewRow(); row43["RACK_ID"] = "511S010302"; row43["EQP_STG_LOC"] = "2"; row43["EQP_COL_LOC"] = "3"; row43["EQP_STATUS_CD"] = "R"; row43["EQP_OP_STATUS_CD"] = "G"; row43["STATUS"] = "7"; row43["PLANTIME"] = ""; row43["TRAYCNT"] = "6"; row43["RACK_STAT_CODE"] = ""; row43["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row43);
            DataRow row44 = dt.NewRow(); row44["RACK_ID"] = "511S010402"; row44["EQP_STG_LOC"] = "2"; row44["EQP_COL_LOC"] = "4"; row44["EQP_STATUS_CD"] = "R"; row44["EQP_OP_STATUS_CD"] = "G"; row44["STATUS"] = "7"; row44["PLANTIME"] = ""; row44["TRAYCNT"] = "1"; row44["RACK_STAT_CODE"] = ""; row44["ROUT_TYPE_CODE"] = "R"; dt.Rows.Add(row44);
            DataRow row45 = dt.NewRow(); row45["RACK_ID"] = "511S010502"; row45["EQP_STG_LOC"] = "2"; row45["EQP_COL_LOC"] = "5"; row45["EQP_STATUS_CD"] = "I"; row45["EQP_OP_STATUS_CD"] = "G"; row45["STATUS"] = "2"; row45["PLANTIME"] = ""; row45["TRAYCNT"] = ""; row45["RACK_STAT_CODE"] = ""; row45["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row45);
            DataRow row46 = dt.NewRow(); row46["RACK_ID"] = "511S010602"; row46["EQP_STG_LOC"] = "2"; row46["EQP_COL_LOC"] = "6"; row46["EQP_STATUS_CD"] = "R"; row46["EQP_OP_STATUS_CD"] = "G"; row46["STATUS"] = "7"; row46["PLANTIME"] = ""; row46["TRAYCNT"] = "6"; row46["RACK_STAT_CODE"] = ""; row46["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row46);
            DataRow row47 = dt.NewRow(); row47["RACK_ID"] = "511S010702"; row47["EQP_STG_LOC"] = "2"; row47["EQP_COL_LOC"] = "7"; row47["EQP_STATUS_CD"] = "I"; row47["EQP_OP_STATUS_CD"] = "G"; row47["STATUS"] = "2"; row47["PLANTIME"] = ""; row47["TRAYCNT"] = ""; row47["RACK_STAT_CODE"] = ""; row47["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row47);
            DataRow row48 = dt.NewRow(); row48["RACK_ID"] = "511S010802"; row48["EQP_STG_LOC"] = "2"; row48["EQP_COL_LOC"] = "8"; row48["EQP_STATUS_CD"] = "I"; row48["EQP_OP_STATUS_CD"] = "G"; row48["STATUS"] = "2"; row48["PLANTIME"] = ""; row48["TRAYCNT"] = ""; row48["RACK_STAT_CODE"] = ""; row48["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row48);
            DataRow row49 = dt.NewRow(); row49["RACK_ID"] = "511S010902"; row49["EQP_STG_LOC"] = "2"; row49["EQP_COL_LOC"] = "9"; row49["EQP_STATUS_CD"] = "I"; row49["EQP_OP_STATUS_CD"] = "G"; row49["STATUS"] = "2"; row49["PLANTIME"] = ""; row49["TRAYCNT"] = ""; row49["RACK_STAT_CODE"] = ""; row49["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row49);
            DataRow row50 = dt.NewRow(); row50["RACK_ID"] = "511S011002"; row50["EQP_STG_LOC"] = "2"; row50["EQP_COL_LOC"] = "10"; row50["EQP_STATUS_CD"] = "I"; row50["EQP_OP_STATUS_CD"] = "G"; row50["STATUS"] = "2"; row50["PLANTIME"] = ""; row50["TRAYCNT"] = ""; row50["RACK_STAT_CODE"] = ""; row50["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row50);
            DataRow row51 = dt.NewRow(); row51["RACK_ID"] = "511S011102"; row51["EQP_STG_LOC"] = "2"; row51["EQP_COL_LOC"] = "11"; row51["EQP_STATUS_CD"] = "R"; row51["EQP_OP_STATUS_CD"] = "G"; row51["STATUS"] = "7"; row51["PLANTIME"] = ""; row51["TRAYCNT"] = "6"; row51["RACK_STAT_CODE"] = ""; row51["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row51);
            DataRow row52 = dt.NewRow(); row52["RACK_ID"] = "511S011202"; row52["EQP_STG_LOC"] = "2"; row52["EQP_COL_LOC"] = "12"; row52["EQP_STATUS_CD"] = "R"; row52["EQP_OP_STATUS_CD"] = "G"; row52["STATUS"] = "1"; row52["PLANTIME"] = ""; row52["TRAYCNT"] = "6"; row52["RACK_STAT_CODE"] = ""; row52["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row52);
            DataRow row53 = dt.NewRow(); row53["RACK_ID"] = "511S011302"; row53["EQP_STG_LOC"] = "2"; row53["EQP_COL_LOC"] = "13"; row53["EQP_STATUS_CD"] = "I"; row53["EQP_OP_STATUS_CD"] = "G"; row53["STATUS"] = "2"; row53["PLANTIME"] = ""; row53["TRAYCNT"] = ""; row53["RACK_STAT_CODE"] = ""; row53["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row53);
            DataRow row54 = dt.NewRow(); row54["RACK_ID"] = "511S011402"; row54["EQP_STG_LOC"] = "2"; row54["EQP_COL_LOC"] = "14"; row54["EQP_STATUS_CD"] = "I"; row54["EQP_OP_STATUS_CD"] = "G"; row54["STATUS"] = "2"; row54["PLANTIME"] = ""; row54["TRAYCNT"] = ""; row54["RACK_STAT_CODE"] = ""; row54["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row54);
            DataRow row55 = dt.NewRow(); row55["RACK_ID"] = "511S011502"; row55["EQP_STG_LOC"] = "2"; row55["EQP_COL_LOC"] = "15"; row55["EQP_STATUS_CD"] = "R"; row55["EQP_OP_STATUS_CD"] = "G"; row55["STATUS"] = "7"; row55["PLANTIME"] = ""; row55["TRAYCNT"] = "3"; row55["RACK_STAT_CODE"] = ""; row55["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row55);
            DataRow row56 = dt.NewRow(); row56["RACK_ID"] = "511S011602"; row56["EQP_STG_LOC"] = "2"; row56["EQP_COL_LOC"] = "16"; row56["EQP_STATUS_CD"] = "I"; row56["EQP_OP_STATUS_CD"] = "G"; row56["STATUS"] = "2"; row56["PLANTIME"] = ""; row56["TRAYCNT"] = ""; row56["RACK_STAT_CODE"] = ""; row56["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row56);
            DataRow row57 = dt.NewRow(); row57["RACK_ID"] = "511S011702"; row57["EQP_STG_LOC"] = "2"; row57["EQP_COL_LOC"] = "17"; row57["EQP_STATUS_CD"] = "I"; row57["EQP_OP_STATUS_CD"] = "G"; row57["STATUS"] = "2"; row57["PLANTIME"] = ""; row57["TRAYCNT"] = ""; row57["RACK_STAT_CODE"] = ""; row57["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row57);
            DataRow row58 = dt.NewRow(); row58["RACK_ID"] = "511S011802"; row58["EQP_STG_LOC"] = "2"; row58["EQP_COL_LOC"] = "18"; row58["EQP_STATUS_CD"] = "I"; row58["EQP_OP_STATUS_CD"] = "G"; row58["STATUS"] = "2"; row58["PLANTIME"] = ""; row58["TRAYCNT"] = ""; row58["RACK_STAT_CODE"] = ""; row58["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row58);
            DataRow row59 = dt.NewRow(); row59["RACK_ID"] = "511S011902"; row59["EQP_STG_LOC"] = "2"; row59["EQP_COL_LOC"] = "19"; row59["EQP_STATUS_CD"] = "I"; row59["EQP_OP_STATUS_CD"] = "G"; row59["STATUS"] = "2"; row59["PLANTIME"] = ""; row59["TRAYCNT"] = ""; row59["RACK_STAT_CODE"] = ""; row59["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row59);
            DataRow row60 = dt.NewRow(); row60["RACK_ID"] = "511S012002"; row60["EQP_STG_LOC"] = "2"; row60["EQP_COL_LOC"] = "20"; row60["EQP_STATUS_CD"] = "R"; row60["EQP_OP_STATUS_CD"] = "G"; row60["STATUS"] = "7"; row60["PLANTIME"] = ""; row60["TRAYCNT"] = "6"; row60["RACK_STAT_CODE"] = ""; row60["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row60);
            DataRow row61 = dt.NewRow(); row61["RACK_ID"] = "511S012102"; row61["EQP_STG_LOC"] = "2"; row61["EQP_COL_LOC"] = "21"; row61["EQP_STATUS_CD"] = "I"; row61["EQP_OP_STATUS_CD"] = "G"; row61["STATUS"] = "2"; row61["PLANTIME"] = ""; row61["TRAYCNT"] = ""; row61["RACK_STAT_CODE"] = ""; row61["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row61);
            DataRow row62 = dt.NewRow(); row62["RACK_ID"] = "511S012202"; row62["EQP_STG_LOC"] = "2"; row62["EQP_COL_LOC"] = "22"; row62["EQP_STATUS_CD"] = "R"; row62["EQP_OP_STATUS_CD"] = "G"; row62["STATUS"] = "1"; row62["PLANTIME"] = ""; row62["TRAYCNT"] = "6"; row62["RACK_STAT_CODE"] = ""; row62["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row62);
            DataRow row63 = dt.NewRow(); row63["RACK_ID"] = "511S012302"; row63["EQP_STG_LOC"] = "2"; row63["EQP_COL_LOC"] = "23"; row63["EQP_STATUS_CD"] = "R"; row63["EQP_OP_STATUS_CD"] = "G"; row63["STATUS"] = "1"; row63["PLANTIME"] = ""; row63["TRAYCNT"] = "6"; row63["RACK_STAT_CODE"] = ""; row63["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row63);
            DataRow row64 = dt.NewRow(); row64["RACK_ID"] = "511S012402"; row64["EQP_STG_LOC"] = "2"; row64["EQP_COL_LOC"] = "24"; row64["EQP_STATUS_CD"] = "R"; row64["EQP_OP_STATUS_CD"] = "G"; row64["STATUS"] = "1"; row64["PLANTIME"] = ""; row64["TRAYCNT"] = "6"; row64["RACK_STAT_CODE"] = ""; row64["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row64);
            DataRow row65 = dt.NewRow(); row65["RACK_ID"] = "511S012502"; row65["EQP_STG_LOC"] = "2"; row65["EQP_COL_LOC"] = "25"; row65["EQP_STATUS_CD"] = "R"; row65["EQP_OP_STATUS_CD"] = "G"; row65["STATUS"] = "7"; row65["PLANTIME"] = ""; row65["TRAYCNT"] = "6"; row65["RACK_STAT_CODE"] = ""; row65["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row65);
            DataRow row66 = dt.NewRow(); row66["RACK_ID"] = "511S012602"; row66["EQP_STG_LOC"] = "2"; row66["EQP_COL_LOC"] = "26"; row66["EQP_STATUS_CD"] = "I"; row66["EQP_OP_STATUS_CD"] = "G"; row66["STATUS"] = "2"; row66["PLANTIME"] = ""; row66["TRAYCNT"] = ""; row66["RACK_STAT_CODE"] = ""; row66["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row66);
            DataRow row67 = dt.NewRow(); row67["RACK_ID"] = "511S012702"; row67["EQP_STG_LOC"] = "2"; row67["EQP_COL_LOC"] = "27"; row67["EQP_STATUS_CD"] = "R"; row67["EQP_OP_STATUS_CD"] = "G"; row67["STATUS"] = "7"; row67["PLANTIME"] = ""; row67["TRAYCNT"] = "5"; row67["RACK_STAT_CODE"] = ""; row67["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row67);
            DataRow row68 = dt.NewRow(); row68["RACK_ID"] = "511S012802"; row68["EQP_STG_LOC"] = "2"; row68["EQP_COL_LOC"] = "28"; row68["EQP_STATUS_CD"] = "I"; row68["EQP_OP_STATUS_CD"] = "G"; row68["STATUS"] = "2"; row68["PLANTIME"] = ""; row68["TRAYCNT"] = ""; row68["RACK_STAT_CODE"] = ""; row68["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row68);
            DataRow row69 = dt.NewRow(); row69["RACK_ID"] = "511S012902"; row69["EQP_STG_LOC"] = "2"; row69["EQP_COL_LOC"] = "29"; row69["EQP_STATUS_CD"] = "I"; row69["EQP_OP_STATUS_CD"] = "G"; row69["STATUS"] = "2"; row69["PLANTIME"] = ""; row69["TRAYCNT"] = ""; row69["RACK_STAT_CODE"] = ""; row69["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row69);
            DataRow row70 = dt.NewRow(); row70["RACK_ID"] = "511S013002"; row70["EQP_STG_LOC"] = "2"; row70["EQP_COL_LOC"] = "30"; row70["EQP_STATUS_CD"] = "R"; row70["EQP_OP_STATUS_CD"] = "G"; row70["STATUS"] = "7"; row70["PLANTIME"] = ""; row70["TRAYCNT"] = "6"; row70["RACK_STAT_CODE"] = ""; row70["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row70);
            DataRow row71 = dt.NewRow(); row71["RACK_ID"] = "511S013102"; row71["EQP_STG_LOC"] = "2"; row71["EQP_COL_LOC"] = "31"; row71["EQP_STATUS_CD"] = "R"; row71["EQP_OP_STATUS_CD"] = "G"; row71["STATUS"] = "1"; row71["PLANTIME"] = ""; row71["TRAYCNT"] = "6"; row71["RACK_STAT_CODE"] = ""; row71["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row71);
            DataRow row72 = dt.NewRow(); row72["RACK_ID"] = "511S013202"; row72["EQP_STG_LOC"] = "2"; row72["EQP_COL_LOC"] = "32"; row72["EQP_STATUS_CD"] = "R"; row72["EQP_OP_STATUS_CD"] = "G"; row72["STATUS"] = "1"; row72["PLANTIME"] = ""; row72["TRAYCNT"] = "6"; row72["RACK_STAT_CODE"] = ""; row72["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row72);
            DataRow row73 = dt.NewRow(); row73["RACK_ID"] = "511S013302"; row73["EQP_STG_LOC"] = "2"; row73["EQP_COL_LOC"] = "33"; row73["EQP_STATUS_CD"] = "R"; row73["EQP_OP_STATUS_CD"] = "G"; row73["STATUS"] = "1"; row73["PLANTIME"] = ""; row73["TRAYCNT"] = "6"; row73["RACK_STAT_CODE"] = ""; row73["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row73);
            DataRow row74 = dt.NewRow(); row74["RACK_ID"] = "511S013402"; row74["EQP_STG_LOC"] = "2"; row74["EQP_COL_LOC"] = "34"; row74["EQP_STATUS_CD"] = "R"; row74["EQP_OP_STATUS_CD"] = "G"; row74["STATUS"] = "1"; row74["PLANTIME"] = ""; row74["TRAYCNT"] = "6"; row74["RACK_STAT_CODE"] = ""; row74["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row74);
            DataRow row75 = dt.NewRow(); row75["RACK_ID"] = "511S013502"; row75["EQP_STG_LOC"] = "2"; row75["EQP_COL_LOC"] = "35"; row75["EQP_STATUS_CD"] = "R"; row75["EQP_OP_STATUS_CD"] = "G"; row75["STATUS"] = "1"; row75["PLANTIME"] = ""; row75["TRAYCNT"] = "6"; row75["RACK_STAT_CODE"] = ""; row75["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row75);
            DataRow row77 = dt.NewRow(); row77["RACK_ID"] = "511S013702"; row77["EQP_STG_LOC"] = "2"; row77["EQP_COL_LOC"] = "37"; row77["EQP_STATUS_CD"] = "R"; row77["EQP_OP_STATUS_CD"] = "G"; row77["STATUS"] = "1"; row77["PLANTIME"] = ""; row77["TRAYCNT"] = "6"; row77["RACK_STAT_CODE"] = ""; row77["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row77);
            DataRow row78 = dt.NewRow(); row78["RACK_ID"] = "511S013802"; row78["EQP_STG_LOC"] = "2"; row78["EQP_COL_LOC"] = "38"; row78["EQP_STATUS_CD"] = "R"; row78["EQP_OP_STATUS_CD"] = "G"; row78["STATUS"] = "1"; row78["PLANTIME"] = ""; row78["TRAYCNT"] = "6"; row78["RACK_STAT_CODE"] = ""; row78["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row78);
            DataRow row79 = dt.NewRow(); row79["RACK_ID"] = "511S013902"; row79["EQP_STG_LOC"] = "2"; row79["EQP_COL_LOC"] = "39"; row79["EQP_STATUS_CD"] = "R"; row79["EQP_OP_STATUS_CD"] = "G"; row79["STATUS"] = "1"; row79["PLANTIME"] = ""; row79["TRAYCNT"] = "6"; row79["RACK_STAT_CODE"] = ""; row79["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row79);
            DataRow row80 = dt.NewRow(); row80["RACK_ID"] = "511S014002"; row80["EQP_STG_LOC"] = "2"; row80["EQP_COL_LOC"] = "40"; row80["EQP_STATUS_CD"] = "R"; row80["EQP_OP_STATUS_CD"] = "G"; row80["STATUS"] = "1"; row80["PLANTIME"] = ""; row80["TRAYCNT"] = "6"; row80["RACK_STAT_CODE"] = ""; row80["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row80);
            DataRow row81 = dt.NewRow(); row81["RACK_ID"] = "511S010101"; row81["EQP_STG_LOC"] = "1"; row81["EQP_COL_LOC"] = "1"; row81["EQP_STATUS_CD"] = "R"; row81["EQP_OP_STATUS_CD"] = "G"; row81["STATUS"] = "1"; row81["PLANTIME"] = ""; row81["TRAYCNT"] = "1"; row81["RACK_STAT_CODE"] = ""; row81["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row81);
            DataRow row82 = dt.NewRow(); row82["RACK_ID"] = "511S010201"; row82["EQP_STG_LOC"] = "1"; row82["EQP_COL_LOC"] = "2"; row82["EQP_STATUS_CD"] = "R"; row82["EQP_OP_STATUS_CD"] = "G"; row82["STATUS"] = "7"; row82["PLANTIME"] = ""; row82["TRAYCNT"] = "2"; row82["RACK_STAT_CODE"] = ""; row82["ROUT_TYPE_CODE"] = "R"; dt.Rows.Add(row82);
            DataRow row83 = dt.NewRow(); row83["RACK_ID"] = "511S010301"; row83["EQP_STG_LOC"] = "1"; row83["EQP_COL_LOC"] = "3"; row83["EQP_STATUS_CD"] = "R"; row83["EQP_OP_STATUS_CD"] = "G"; row83["STATUS"] = "7"; row83["PLANTIME"] = ""; row83["TRAYCNT"] = "6"; row83["RACK_STAT_CODE"] = ""; row83["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row83);
            DataRow row84 = dt.NewRow(); row84["RACK_ID"] = "511S010401"; row84["EQP_STG_LOC"] = "1"; row84["EQP_COL_LOC"] = "4"; row84["EQP_STATUS_CD"] = "R"; row84["EQP_OP_STATUS_CD"] = "G"; row84["STATUS"] = "7"; row84["PLANTIME"] = ""; row84["TRAYCNT"] = "6"; row84["RACK_STAT_CODE"] = ""; row84["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row84);
            DataRow row85 = dt.NewRow(); row85["RACK_ID"] = "511S010501"; row85["EQP_STG_LOC"] = "1"; row85["EQP_COL_LOC"] = "5"; row85["EQP_STATUS_CD"] = "I"; row85["EQP_OP_STATUS_CD"] = "G"; row85["STATUS"] = "2"; row85["PLANTIME"] = ""; row85["TRAYCNT"] = ""; row85["RACK_STAT_CODE"] = ""; row85["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row85);
            DataRow row86 = dt.NewRow(); row86["RACK_ID"] = "511S010601"; row86["EQP_STG_LOC"] = "1"; row86["EQP_COL_LOC"] = "6"; row86["EQP_STATUS_CD"] = "R"; row86["EQP_OP_STATUS_CD"] = "G"; row86["STATUS"] = "6"; row86["PLANTIME"] = ""; row86["TRAYCNT"] = ""; row86["RACK_STAT_CODE"] = ""; row86["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row86);
            DataRow row87 = dt.NewRow(); row87["RACK_ID"] = "511S010701"; row87["EQP_STG_LOC"] = "1"; row87["EQP_COL_LOC"] = "7"; row87["EQP_STATUS_CD"] = "I"; row87["EQP_OP_STATUS_CD"] = "G"; row87["STATUS"] = "2"; row87["PLANTIME"] = ""; row87["TRAYCNT"] = ""; row87["RACK_STAT_CODE"] = ""; row87["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row87);
            DataRow row88 = dt.NewRow(); row88["RACK_ID"] = "511S010801"; row88["EQP_STG_LOC"] = "1"; row88["EQP_COL_LOC"] = "8"; row88["EQP_STATUS_CD"] = "I"; row88["EQP_OP_STATUS_CD"] = "G"; row88["STATUS"] = "2"; row88["PLANTIME"] = ""; row88["TRAYCNT"] = ""; row88["RACK_STAT_CODE"] = ""; row88["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row88);
            DataRow row89 = dt.NewRow(); row89["RACK_ID"] = "511S010901"; row89["EQP_STG_LOC"] = "1"; row89["EQP_COL_LOC"] = "9"; row89["EQP_STATUS_CD"] = "I"; row89["EQP_OP_STATUS_CD"] = "G"; row89["STATUS"] = "2"; row89["PLANTIME"] = ""; row89["TRAYCNT"] = ""; row89["RACK_STAT_CODE"] = ""; row89["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row89);
            DataRow row90 = dt.NewRow(); row90["RACK_ID"] = "511S011001"; row90["EQP_STG_LOC"] = "1"; row90["EQP_COL_LOC"] = "10"; row90["EQP_STATUS_CD"] = "R"; row90["EQP_OP_STATUS_CD"] = "G"; row90["STATUS"] = "1"; row90["PLANTIME"] = ""; row90["TRAYCNT"] = "6"; row90["RACK_STAT_CODE"] = ""; row90["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row90);
            DataRow row91 = dt.NewRow(); row91["RACK_ID"] = "511S011101"; row91["EQP_STG_LOC"] = "1"; row91["EQP_COL_LOC"] = "11"; row91["EQP_STATUS_CD"] = "I"; row91["EQP_OP_STATUS_CD"] = "G"; row91["STATUS"] = "2"; row91["PLANTIME"] = ""; row91["TRAYCNT"] = ""; row91["RACK_STAT_CODE"] = ""; row91["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row91);
            DataRow row92 = dt.NewRow(); row92["RACK_ID"] = "511S011201"; row92["EQP_STG_LOC"] = "1"; row92["EQP_COL_LOC"] = "12"; row92["EQP_STATUS_CD"] = "R"; row92["EQP_OP_STATUS_CD"] = "G"; row92["STATUS"] = "7"; row92["PLANTIME"] = ""; row92["TRAYCNT"] = "5"; row92["RACK_STAT_CODE"] = ""; row92["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row92);
            DataRow row93 = dt.NewRow(); row93["RACK_ID"] = "511S011301"; row93["EQP_STG_LOC"] = "1"; row93["EQP_COL_LOC"] = "13"; row93["EQP_STATUS_CD"] = "R"; row93["EQP_OP_STATUS_CD"] = "G"; row93["STATUS"] = "7"; row93["PLANTIME"] = ""; row93["TRAYCNT"] = "6"; row93["RACK_STAT_CODE"] = ""; row93["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row93);
            DataRow row94 = dt.NewRow(); row94["RACK_ID"] = "511S011401"; row94["EQP_STG_LOC"] = "1"; row94["EQP_COL_LOC"] = "14"; row94["EQP_STATUS_CD"] = "I"; row94["EQP_OP_STATUS_CD"] = "G"; row94["STATUS"] = "2"; row94["PLANTIME"] = ""; row94["TRAYCNT"] = ""; row94["RACK_STAT_CODE"] = ""; row94["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row94);
            DataRow row95 = dt.NewRow(); row95["RACK_ID"] = "511S011501"; row95["EQP_STG_LOC"] = "1"; row95["EQP_COL_LOC"] = "15"; row95["EQP_STATUS_CD"] = "R"; row95["EQP_OP_STATUS_CD"] = "G"; row95["STATUS"] = "7"; row95["PLANTIME"] = ""; row95["TRAYCNT"] = "6"; row95["RACK_STAT_CODE"] = ""; row95["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row95);
            DataRow row96 = dt.NewRow(); row96["RACK_ID"] = "511S011601"; row96["EQP_STG_LOC"] = "1"; row96["EQP_COL_LOC"] = "16"; row96["EQP_STATUS_CD"] = "R"; row96["EQP_OP_STATUS_CD"] = "G"; row96["STATUS"] = "7"; row96["PLANTIME"] = ""; row96["TRAYCNT"] = "1"; row96["RACK_STAT_CODE"] = ""; row96["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row96);
            DataRow row97 = dt.NewRow(); row97["RACK_ID"] = "511S011701"; row97["EQP_STG_LOC"] = "1"; row97["EQP_COL_LOC"] = "17"; row97["EQP_STATUS_CD"] = "I"; row97["EQP_OP_STATUS_CD"] = "G"; row97["STATUS"] = "2"; row97["PLANTIME"] = ""; row97["TRAYCNT"] = ""; row97["RACK_STAT_CODE"] = ""; row97["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row97);
            DataRow row98 = dt.NewRow(); row98["RACK_ID"] = "511S011801"; row98["EQP_STG_LOC"] = "1"; row98["EQP_COL_LOC"] = "18"; row98["EQP_STATUS_CD"] = "R"; row98["EQP_OP_STATUS_CD"] = "G"; row98["STATUS"] = "7"; row98["PLANTIME"] = ""; row98["TRAYCNT"] = "4"; row98["RACK_STAT_CODE"] = ""; row98["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row98);
            DataRow row99 = dt.NewRow(); row99["RACK_ID"] = "511S011901"; row99["EQP_STG_LOC"] = "1"; row99["EQP_COL_LOC"] = "19"; row99["EQP_STATUS_CD"] = "I"; row99["EQP_OP_STATUS_CD"] = "G"; row99["STATUS"] = "2"; row99["PLANTIME"] = ""; row99["TRAYCNT"] = ""; row99["RACK_STAT_CODE"] = ""; row99["ROUT_TYPE_CODE"] = ""; dt.Rows.Add(row99);
            DataRow row100 = dt.NewRow(); row100["RACK_ID"] = "511S012001"; row100["EQP_STG_LOC"] = "1"; row100["EQP_COL_LOC"] = "20"; row100["EQP_STATUS_CD"] = "R"; row100["EQP_OP_STATUS_CD"] = "G"; row100["STATUS"] = "7"; row100["PLANTIME"] = ""; row100["TRAYCNT"] = "6"; row100["RACK_STAT_CODE"] = ""; row100["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row100);
            DataRow row101 = dt.NewRow(); row101["RACK_ID"] = "511S012101"; row101["EQP_STG_LOC"] = "1"; row101["EQP_COL_LOC"] = "21"; row101["EQP_STATUS_CD"] = "R"; row101["EQP_OP_STATUS_CD"] = "G"; row101["STATUS"] = "7"; row101["PLANTIME"] = ""; row101["TRAYCNT"] = "6"; row101["RACK_STAT_CODE"] = ""; row101["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row101);
            DataRow row102 = dt.NewRow(); row102["RACK_ID"] = "511S012201"; row102["EQP_STG_LOC"] = "1"; row102["EQP_COL_LOC"] = "22"; row102["EQP_STATUS_CD"] = "R"; row102["EQP_OP_STATUS_CD"] = "G"; row102["STATUS"] = "7"; row102["PLANTIME"] = ""; row102["TRAYCNT"] = "3"; row102["RACK_STAT_CODE"] = ""; row102["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row102);
            DataRow row103 = dt.NewRow(); row103["RACK_ID"] = "511S012301"; row103["EQP_STG_LOC"] = "1"; row103["EQP_COL_LOC"] = "23"; row103["EQP_STATUS_CD"] = "R"; row103["EQP_OP_STATUS_CD"] = "G"; row103["STATUS"] = "1"; row103["PLANTIME"] = ""; row103["TRAYCNT"] = "6"; row103["RACK_STAT_CODE"] = ""; row103["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row103);
            DataRow row104 = dt.NewRow(); row104["RACK_ID"] = "511S012401"; row104["EQP_STG_LOC"] = "1"; row104["EQP_COL_LOC"] = "24"; row104["EQP_STATUS_CD"] = "R"; row104["EQP_OP_STATUS_CD"] = "G"; row104["STATUS"] = "7"; row104["PLANTIME"] = ""; row104["TRAYCNT"] = "1"; row104["RACK_STAT_CODE"] = ""; row104["ROUT_TYPE_CODE"] = "R"; dt.Rows.Add(row104);
            DataRow row105 = dt.NewRow(); row105["RACK_ID"] = "511S012501"; row105["EQP_STG_LOC"] = "1"; row105["EQP_COL_LOC"] = "25"; row105["EQP_STATUS_CD"] = "R"; row105["EQP_OP_STATUS_CD"] = "G"; row105["STATUS"] = "7"; row105["PLANTIME"] = ""; row105["TRAYCNT"] = "6"; row105["RACK_STAT_CODE"] = ""; row105["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row105);
            DataRow row106 = dt.NewRow(); row106["RACK_ID"] = "511S012601"; row106["EQP_STG_LOC"] = "1"; row106["EQP_COL_LOC"] = "26"; row106["EQP_STATUS_CD"] = "R"; row106["EQP_OP_STATUS_CD"] = "G"; row106["STATUS"] = "7"; row106["PLANTIME"] = ""; row106["TRAYCNT"] = "6"; row106["RACK_STAT_CODE"] = ""; row106["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row106);
            DataRow row107 = dt.NewRow(); row107["RACK_ID"] = "511S012701"; row107["EQP_STG_LOC"] = "1"; row107["EQP_COL_LOC"] = "27"; row107["EQP_STATUS_CD"] = "R"; row107["EQP_OP_STATUS_CD"] = "G"; row107["STATUS"] = "7"; row107["PLANTIME"] = ""; row107["TRAYCNT"] = "6"; row107["RACK_STAT_CODE"] = ""; row107["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row107);
            DataRow row108 = dt.NewRow(); row108["RACK_ID"] = "511S012801"; row108["EQP_STG_LOC"] = "1"; row108["EQP_COL_LOC"] = "28"; row108["EQP_STATUS_CD"] = "R"; row108["EQP_OP_STATUS_CD"] = "G"; row108["STATUS"] = "7"; row108["PLANTIME"] = ""; row108["TRAYCNT"] = "1"; row108["RACK_STAT_CODE"] = ""; row108["ROUT_TYPE_CODE"] = "R"; dt.Rows.Add(row108);
            DataRow row109 = dt.NewRow(); row109["RACK_ID"] = "511S012901"; row109["EQP_STG_LOC"] = "1"; row109["EQP_COL_LOC"] = "29"; row109["EQP_STATUS_CD"] = "R"; row109["EQP_OP_STATUS_CD"] = "G"; row109["STATUS"] = "1"; row109["PLANTIME"] = ""; row109["TRAYCNT"] = "6"; row109["RACK_STAT_CODE"] = ""; row109["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row109);
            DataRow row110 = dt.NewRow(); row110["RACK_ID"] = "511S013001"; row110["EQP_STG_LOC"] = "1"; row110["EQP_COL_LOC"] = "30"; row110["EQP_STATUS_CD"] = "R"; row110["EQP_OP_STATUS_CD"] = "G"; row110["STATUS"] = "1"; row110["PLANTIME"] = ""; row110["TRAYCNT"] = "6"; row110["RACK_STAT_CODE"] = ""; row110["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row110);
            DataRow row111 = dt.NewRow(); row111["RACK_ID"] = "511S013101"; row111["EQP_STG_LOC"] = "1"; row111["EQP_COL_LOC"] = "31"; row111["EQP_STATUS_CD"] = "R"; row111["EQP_OP_STATUS_CD"] = "G"; row111["STATUS"] = "1"; row111["PLANTIME"] = ""; row111["TRAYCNT"] = "6"; row111["RACK_STAT_CODE"] = ""; row111["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row111);
            DataRow row112 = dt.NewRow(); row112["RACK_ID"] = "511S013201"; row112["EQP_STG_LOC"] = "1"; row112["EQP_COL_LOC"] = "32"; row112["EQP_STATUS_CD"] = "R"; row112["EQP_OP_STATUS_CD"] = "G"; row112["STATUS"] = "7"; row112["PLANTIME"] = ""; row112["TRAYCNT"] = "6"; row112["RACK_STAT_CODE"] = ""; row112["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row112);
            DataRow row113 = dt.NewRow(); row113["RACK_ID"] = "511S013301"; row113["EQP_STG_LOC"] = "1"; row113["EQP_COL_LOC"] = "33"; row113["EQP_STATUS_CD"] = "R"; row113["EQP_OP_STATUS_CD"] = "G"; row113["STATUS"] = "1"; row113["PLANTIME"] = ""; row113["TRAYCNT"] = "6"; row113["RACK_STAT_CODE"] = ""; row113["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row113);
            DataRow row114 = dt.NewRow(); row114["RACK_ID"] = "511S013401"; row114["EQP_STG_LOC"] = "1"; row114["EQP_COL_LOC"] = "34"; row114["EQP_STATUS_CD"] = "R"; row114["EQP_OP_STATUS_CD"] = "G"; row114["STATUS"] = "7"; row114["PLANTIME"] = ""; row114["TRAYCNT"] = "1"; row114["RACK_STAT_CODE"] = ""; row114["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row114);
            DataRow row115 = dt.NewRow(); row115["RACK_ID"] = "511S013501"; row115["EQP_STG_LOC"] = "1"; row115["EQP_COL_LOC"] = "35"; row115["EQP_STATUS_CD"] = "R"; row115["EQP_OP_STATUS_CD"] = "G"; row115["STATUS"] = "7"; row115["PLANTIME"] = ""; row115["TRAYCNT"] = "6"; row115["RACK_STAT_CODE"] = ""; row115["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row115);
            DataRow row116 = dt.NewRow(); row116["RACK_ID"] = "511S013601"; row116["EQP_STG_LOC"] = "1"; row116["EQP_COL_LOC"] = "36"; row116["EQP_STATUS_CD"] = "R"; row116["EQP_OP_STATUS_CD"] = "G"; row116["STATUS"] = "1"; row116["PLANTIME"] = ""; row116["TRAYCNT"] = "6"; row116["RACK_STAT_CODE"] = ""; row116["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row116);
            DataRow row117 = dt.NewRow(); row117["RACK_ID"] = "511S013701"; row117["EQP_STG_LOC"] = "1"; row117["EQP_COL_LOC"] = "37"; row117["EQP_STATUS_CD"] = "R"; row117["EQP_OP_STATUS_CD"] = "G"; row117["STATUS"] = "7"; row117["PLANTIME"] = ""; row117["TRAYCNT"] = "6"; row117["RACK_STAT_CODE"] = ""; row117["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row117);
            DataRow row118 = dt.NewRow(); row118["RACK_ID"] = "511S013801"; row118["EQP_STG_LOC"] = "1"; row118["EQP_COL_LOC"] = "38"; row118["EQP_STATUS_CD"] = "R"; row118["EQP_OP_STATUS_CD"] = "G"; row118["STATUS"] = "7"; row118["PLANTIME"] = ""; row118["TRAYCNT"] = "6"; row118["RACK_STAT_CODE"] = ""; row118["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row118);
            DataRow row119 = dt.NewRow(); row119["RACK_ID"] = "511S013901"; row119["EQP_STG_LOC"] = "1"; row119["EQP_COL_LOC"] = "39"; row119["EQP_STATUS_CD"] = "R"; row119["EQP_OP_STATUS_CD"] = "G"; row119["STATUS"] = "7"; row119["PLANTIME"] = ""; row119["TRAYCNT"] = "1"; row119["RACK_STAT_CODE"] = ""; row119["ROUT_TYPE_CODE"] = "F"; dt.Rows.Add(row119);
            DataRow row120 = dt.NewRow(); row120["RACK_ID"] = "511S014001"; row120["EQP_STG_LOC"] = "1"; row120["EQP_COL_LOC"] = "40"; row120["EQP_STATUS_CD"] = "R"; row120["EQP_OP_STATUS_CD"] = "G"; row120["STATUS"] = "1"; row120["PLANTIME"] = ""; row120["TRAYCNT"] = "6"; row120["RACK_STAT_CODE"] = ""; row120["ROUT_TYPE_CODE"] = "D"; dt.Rows.Add(row120);



        }
        /// <summary>
        /// 사용할 색상정보 가져오기
        /// </summary>
        private void GeColorLegend()
        {
            try
            {
                _dtColor = null;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_STOCKERSTATUS_MCC";

                dtRqst.Rows.Add(dr);
                
                ShowLoadingIndicator();
                _dtColor = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", dtRqst);

                SetColorGrid(_dtColor);
              
                hash_loss_color = DataTableConverter.ToHash(_dtColor);

                foreach (DataRow drRslt in _dtColor.Rows)
                {
                    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                    cbItem.Content = drRslt["CBO_NAME"];
                    cbItem.Foreground = ColorToBrush(System.Drawing.Color.FromName(drRslt["ATTRIBUTE2"].ToString()));
                    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["ATTRIBUTE1"].ToString()));
                    //cboColorLegend.Items.Add(cbItem);
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
        }
        
        private void SetColorGrid(DataTable dt)
        {
           
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                C1DataGrid dgNew = new C1DataGrid();

                C1.WPF.DataGrid.DataGridTextColumn textColumn1 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn1.Header = "Color";
                textColumn1.Binding = new Binding("CBO_NAME");
                textColumn1.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn1.IsReadOnly = true;

                C1.WPF.DataGrid.DataGridTextColumn textColumn2 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn2.Header = "Color";
                textColumn2.Binding = new Binding("ATTRIBUTE1");
                textColumn2.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn2.IsReadOnly = true;
                textColumn2.Visibility = Visibility.Collapsed;

                C1.WPF.DataGrid.DataGridTextColumn textColumn3 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn3.Header = "Color";
                textColumn3.Binding = new Binding("ATTRIBUTE2");
                textColumn3.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn3.IsReadOnly = true;
                textColumn3.Visibility = Visibility.Collapsed;
                
                dgNew.Columns.Add(textColumn1);
                dgNew.Columns.Add(textColumn2);
                dgNew.Columns.Add(textColumn3);

                // dgNew.IsEnabled = false;
                // dgNew.IsReadOnly = true;
                dgNew.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.None;
                dgNew.FrozenColumnCount = 0;
                dgNew.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
                dgNew.LoadedCellPresenter += dgColorLegend_LoadedCellPresenter;
                dgNew.SelectedBackground = null;


                Grid.SetRow(dgNew, 0);
                Grid.SetColumn(dgNew, i + 1);

                dgColor.Children.Add(dgNew);

                DataTable dtRow = new DataTable();
                dtRow.Columns.Add("CBO_NAME", typeof(string));
                dtRow.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRow.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow drRow = dtRow.NewRow();
                drRow["CBO_NAME"] = dt.Rows[i]["CBO_NAME"];
                drRow["ATTRIBUTE1"] = dt.Rows[i]["ATTRIBUTE1"];
                drRow["ATTRIBUTE2"] = dt.Rows[i]["ATTRIBUTE2"];
                dtRow.Rows.Add(drRow);

                Util.GridSetData(dgNew, dtRow, FrameOperation, true);
                
            }


        }

        private string GetLane()
        {
            string sLane = string.Empty;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COM_TYPE_CODE"] = "FORM_STOCKER_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboStockerType, bAllNull: true);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SYSTEM_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                sLane = dtResult.Rows[0]["ATTR2"].ToString();
                return sLane;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sLane;
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

        private void GetInfoToCell(C1.WPF.DataGrid.DataGridCell cell)
        {
            try
            {
                ClearControlRack();

                // 선택 적재RACK 정보 표시
                tcRackInfo.SelectedIndex = 1;

                if (cell != null)
                {
                    if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0))
                    {
                        return;
                    }

                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();

                    int iRowNum;
                    if (string.IsNullOrEmpty(ROWNUM) || !int.TryParse(ROWNUM, out iRowNum))
                    {
                        return;
                    }

                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "EQP_COL_LOC"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "EQP_STG_LOC"));
                    string RACKID = Util.NVC(GetDtRowValue(ROWNUM, "RACK_ID"));

                    txtSelRow.Text = Util.GetCondition(cboRow);

                    if (!string.IsNullOrEmpty(COL) && !string.IsNullOrEmpty(STG))
                    {
                        txtSelCol.Text = string.Format("{0:00}", Convert.ToInt16(COL));
                        txtSelStg.Text = string.Format("{0:00}", Convert.ToInt16(STG));
                    }

                    if (!string.IsNullOrEmpty(RACKID))
                    {
                        GetEqpInfo(RACKID);
                    }

                    //입고 금지 사유 
                    DataRow[] sRackIDs = dtTemp.Select("RACK_ID = '" + RACKID + "'");
                    txtRemark.Text = Util.NVC(sRackIDs[0]["RCV_PRHB_RSN"]);

                    if (!string.IsNullOrEmpty(txtRemark.Text))
                    {
                        cboSetMode.SelectedIndex = 3;
                        txtRemark.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        private void dgStockerRack_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
