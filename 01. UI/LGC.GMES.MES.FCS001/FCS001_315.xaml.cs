/*************************************************************************************
 Created Date : 2023.01.04
      Creator : 심찬보
   Decription : 오창 IT 3동 자동차 고전압 활성화 Aging Rack 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.04  DEVELOPER : Initial Created.
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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_315 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Hashtable hash_loss_color = new Hashtable();
        private DataTable dtColor = new DataTable();
        private DataTable dtTemp = new DataTable();
        private DataTable _dtCopy = new DataTable();

        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;

        Util _Util = new Util();
        bool bAreaStateCodeUseFlag = false; // 2023.10.29 화재 알람 초기화 동별코드 추가
        #endregion

        #region Initialize
        public FCS001_315()
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
            bAreaStateCodeUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_012_AREA_RACK_STATE_CODE"); // 2023.10.29 화재 알람 초기화 동별코드 추가
            //Combo Setting
            InitCombo();

            this.Loaded -= UserControl_Loaded; //20210406 화면이동 후 재 Load 이벤트 안 타도록 수정

        }

        /// <summary>
        /// 사용할 색상정보 가져오기
        /// </summary>
        private void GeColorLegend()
        {
            try
            {
                dtColor = null;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_AGINGSTATUS";

                dtRqst.Rows.Add(dr);

                dtColor = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqst);

                hash_loss_color = DataTableConverter.ToHash(dtColor);

                foreach (DataRow drRslt in dtColor.Rows)
                {
                    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                    cbItem.Content = drRslt["CMCDNAME"];
                    cbItem.Foreground = ColorToBrush(System.Drawing.Color.FromName(drRslt["ATTRIBUTE2"].ToString()));
                    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["ATTRIBUTE1"].ToString()));
                    cboColorLegend.Items.Add(cbItem);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
            CommonCombo_Form _combo = new CommonCombo_Form();

            GeColorLegend();
            string[] sFilter1 = { "FORM_AGING_TYPE_CODE_HVF", "N" };
            _combo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.NONE, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter1);
            string[] sFilter2 = { "COMBO_RACK_STAT_SET_CODE" };
            
            string sCase = "CMN";
            //_combo.SetCombo(cboSetMode, CommonCombo_Form.ComboStatus.SELECT, sCase: "CMN", sFilter: sFilter2);
            // 2023.10.29 화재 알람 초기화 동별코드 추가
            if (bAreaStateCodeUseFlag)
            {
                sCase = "AREA_COMMON_CODE";
            };
            _combo.SetCombo(cboSetMode, CommonCombo_Form.ComboStatus.SELECT, sCase: sCase, sFilter: sFilter2);
        }
        #endregion

        #region Event
        private void cboAgingType_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            cboSCLine.Text = string.Empty;
            SetEqptAgingSc(cboSCLine);
        }

        private void cboSCLine_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            cboRow.Text = string.Empty;
            object[] objParent = { cboAgingType.GetStringValue("ATTR1"), cboAgingType.GetStringValue("ATTR2"), Util.GetCondition(cboSCLine) };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form.ComboStatus.NONE, sCase: "AGING_ROW", objParent: objParent);
        }

        private void cboRow_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            //GetList();
        }

        private void txtCol_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtCol.Text)) && (e.Key == Key.Enter))
            {
                int iCol = Convert.ToInt16(txtCol.Text);

                if (iCol < dgAgingRack.Columns.Count)
                {
                    dgAgingRack.ScrollIntoView(0, iCol);
                    for(int i = 0; i < dgAgingRack.Columns.Count;i++)
                    {
                        if (i.Equals(iCol))
                        {
                            dgAgingRack.GetCell(0, i).Presenter.IsSelected = true;
                        }
                        else
                        {
                            dgAgingRack.GetCell(0, i).Presenter.IsSelected = false;
                        }
                    }
                }
            }
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtTrayID.Text.Length != 10)
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
                    dr["CSTID"] = txtTrayID.Text;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_RETRIEVE_RACK", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt.Rows.Count == 1)
                    {
                        if (Util.GetCondition(cboAgingType).ToString().Equals(dtRslt.Rows[0]["AGING_TYPE"].ToString()) && Util.GetCondition(cboRow).ToString().Equals(dtRslt.Rows[0]["ROW_NO"].ToString()))
                        {
                            for (int iRow = 0; iRow < dgAgingRack.Rows.Count; iRow++)
                            {
                                for (int iCol = 0; iCol < dgAgingRack.Columns.Count; iCol++)
                                {
                                    if (iRow > 0 && iCol > 0)
                                    {
                                        if (dgAgingRack.GetCell(iRow, iCol) == null ||
                                            dgAgingRack.GetCell(iRow, iCol).Presenter == null ||
                                            dgAgingRack.GetCell(iRow, iCol).Presenter.Tag == null) continue;

                                        string sEqp = Util.NVC(dgAgingRack.GetCell(iRow, iCol).Presenter.Tag.ToString());

                                        if (dtRslt.Rows[0]["EQPTID"].ToString().Equals(sEqp))
                                        {
                                            dgAgingRack.CurrentCell = dgAgingRack.GetCell(iRow, iCol);
                                            MouseButtonEventArgs mb = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
                                            dgAgingRack_PreviewMouseLeftButtonDown(null, mb);

                                            dgAgingRack.Selection.Clear();
                                            dgAgingRack.ScrollIntoView(iRow, iCol);
                                            dgAgingRack.GetCell(iRow, iCol).Presenter.IsSelected = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //검색하신 Tray ID: {0}는\r\n Aging : {1}\r\n 열 : {2}\r\n 연 : {3} \r\n 단 : {4}에 입고되어 있습니다.
                            Util.AlertInfo("FM_ME_0322", new string[] { txtTrayID.Text, dtRslt.Rows[0]["AGING_TYPE_NAME"].ToString(), dtRslt.Rows[0]["ROW_NO"].ToString(), dtRslt.Rows[0]["COL_NO"].ToString(), dtRslt.Rows[0]["STAGE_NO"].ToString() });
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
        }

        private void btnAllStatus_Click(object sender, RoutedEventArgs e)
        {
            //연계 화면 확인 후 수정
            this.FrameOperation.OpenMenuFORM("SFU010705621", "FCS001_315_ALL", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("고전압 전체 AGING RACK 현황"), true, null);
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {            
            if (dgAgingRack.CurrentRow == null || dgAgingRack.SelectedIndex == -1)
            {
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

            if (sStatus.Equals("INIT_RACK"))
            {
                string sMsg = "FM_ME_0337";  //변경하시겠습니까?

                Util.MessageConfirm(sMsg, (result) =>
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        try
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "INDATA";
                            dtRqst.Columns.Add("RACK_STAT_CODE", typeof(string));
                            dtRqst.Columns.Add("RACK_ID", typeof(string));
                            dtRqst.Columns.Add("RACK_INFO_DEL_FLAG", typeof(string));
                            dtRqst.Columns.Add("RCV_PRHB_RSN", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));

                            foreach (C1.WPF.DataGrid.DataGridCell cell in dgAgingRack.Selection.SelectedCells)
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

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RACK_EQP_STATUS", "INDATA", "OUTDATA", dtRqst);
                            if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
                            {
                                if (sStatus.ToString().Equals("UNUSE"))
                                {
                                    Util.MessageValidation("FM_ME_0062");  //Rack 설정 변경에 실패하였습니다.\r\n입고 금지 Rack만 설정 가능합니다.
                            }
                                else
                                {
                                    Util.MessageValidation("FM_ME_0061");  //Rack 설정 변경에 실패하였습니다.
                            }
                            }
                            else
                            {
                                Util.MessageValidation("FM_ME_0063");  //Rack 설정 변경을 완료하였습니다.
                            GetList();
                            }

                            GetList();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });
            }
            else
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "INDATA";
                    dtRqst.Columns.Add("RACK_STAT_CODE", typeof(string));
                    dtRqst.Columns.Add("RACK_ID", typeof(string));
                    dtRqst.Columns.Add("RACK_INFO_DEL_FLAG", typeof(string));
                    dtRqst.Columns.Add("RCV_PRHB_RSN", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));

                    foreach (C1.WPF.DataGrid.DataGridCell cell in dgAgingRack.Selection.SelectedCells)
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

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RACK_EQP_STATUS", "INDATA", "OUTDATA", dtRqst);
                    if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
                    {
                        if (sStatus.ToString().Equals("UNUSE"))
                        {
                            Util.MessageValidation("FM_ME_0062");  //Rack 설정 변경에 실패하였습니다.\r\n입고 금지 Rack만 설정 가능합니다.
                        }
                        else
                        {
                            Util.MessageValidation("FM_ME_0061");  //Rack 설정 변경에 실패하였습니다.
                        }
                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0063");  //Rack 설정 변경을 완료하였습니다.
                        GetList();
                    }

                    GetList();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }

         
        }

        private void dgAgingRack_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgAgingRack.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0))
                    {
                        return;
                    }

                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();

                    int iRowNum = 0;
                    if (string.IsNullOrEmpty(ROWNUM) || !int.TryParse(ROWNUM, out iRowNum))
                    {
                        return;
                    }

                    string sCol = Util.NVC(GetDtRowValue(ROWNUM, "EQP_COL_LOC"));
                    string sStg = Util.NVC(GetDtRowValue(ROWNUM, "EQP_STG_LOC"));

                    FCS001_001_TRAY_HISTORY wndHist = new FCS001_001_TRAY_HISTORY();
                    wndHist.FrameOperation = FrameOperation;

                    if (wndHist != null)
                    {
                        object[] parameters = new object[5];
                        parameters[0] = Util.GetCondition(cboRow);
                        parameters[1] = string.Format("{0:00}", Convert.ToInt16(sCol));
                        parameters[2] = string.Format("{0:00}", Convert.ToInt16(sStg));
                        parameters[3] = cell.Presenter.Tag.ToString();
                        parameters[4] = "R";

                        C1WindowExtension.SetParameters(wndHist, parameters);
                        wndHist.Closed += new EventHandler(wndHist_Closed);

                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            FCS001_001_TRAY_HISTORY window = sender as FCS001_001_TRAY_HISTORY;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void dgAgingRack_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ClearControlRack();

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgAgingRack.GetCellFromPoint(pnt);

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
                    //dtTemp.AsEnumerable().Where(r => r["RACK_ID"].Equals(RACKID));
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

        private void dgAgingRack_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            //fCell.Font = new Font("맑은 고딕", (chkRackFull.Checked) ? (float)8.00 : (float)11.25, FontStyle.Strikeout);
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
                        //설비없음
                        e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                        e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                        //DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), ObjectDic.Instance.GetObjectName("NO_EQP"));
                    }
                }
            }));
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
        }

        private void txtTray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(((TextBox)sender).Text))
                {
                    return;
                }

                object[] parameters = new object[6];
                string sTrayId = ((TextBox)sender).Text;

                parameters[0] = sTrayId;
                this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회 연계
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                ClearControlHeader();
                ClearControlRack();

                dgAgingRack.Columns.Clear();
                dgAgingRack.Refresh();
                dtTemp = null;
                _dtCopy = null;

                //20210.04.06 KDH: 필수값 체크 로직 추가 START
                //if (string.IsNullOrEmpty(Util.NVC(cboSCLine.SelectedValue)))
                //{
                //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("SC_LINE"));
                //    return;
                //}

                //if (string.IsNullOrEmpty(Util.NVC(cboRow.SelectedValue)))
                //{
                //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("열"));
                //    return;
                //}
                //20210.04.06 KDH: 필수값 체크 로직 추가 END

               DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string)); //20210331 S/C 호기 필수 값으로 변경

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] =  cboAgingType.GetStringValue("ATTR2");
                dr["EQPT_GR_TYPE_CODE"] = cboAgingType.GetStringValue("ATTR1");// EQPT_GR_TYPE_CODE;
                dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow);
                dr["EQPTID"] = Util.GetCondition(cboSCLine); //20210331 S/C 호기 필수 값으로 변경
                //dr["EQP_ROW_LOC"] = "1";
                //dr["EQPTID"] = cboAgingType.GetStringValue("CBO_CODE");
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                new ClientProxy().ExecuteService_Multi("BR_GET_AGING_RACK_RETRIEVE", "INDATA", "RACK1,RACK2,CHARGE", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables["RACK1"].Rows.Count > 0 && bizResult.Tables["CHARGE"].Rows.Count > 0)
                        {
                            txtTotalUseRate.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["RATE_ALL"]);      //전체사용률
                            txtRowUseRate.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["CHGRATE1"]);        //열 사용률
                            txtRackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKCOUNT"]);           //Rack 수
                            txtPossibleCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKIBGO"]);        //입고 가능 Rack 수
                            txtImpossibleCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKGEUMGI"]);    //입고 금지 Rack 수
                            txt1RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_ONE"]);           //1단 Rack 수
                            txt2RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_TWO"]);           //2단 Rack 수
                        }

                        dtTemp = bizResult.Tables["RACK2"];

                        SetRackStatus(dtTemp);

                        /*if (chkRackFull.IsChecked.Equals(true))
                        {
                            dgAgingRack.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            dgAgingRack.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        }*/

                        dgAgingRack.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                        dgAgingRack.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, dsRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetRackStatus(DataTable dtRack)
        {
            try
            {
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

                //int iColCnt = (iMaxCol < 25) ? iMaxCol + 1 : (iMaxCol / 2) + 1; //20210402 Aging Rack 연 설정 부분 수정
                int iColCnt = (iMaxCol < 18) ? iMaxCol + 1 : Convert.ToInt32(Math.Ceiling(Convert.ToDouble(iMaxCol) / 2)) + 1; //20210402 Aging Rack 연 설정 부분 수정
                int iRowCnt = (iMaxCol < 18) ? iMaxStg : (iMaxStg * 2) + 2;

                //상단 Datatable, 하단 Datatable 정의
                DataTable Udt = new DataTable();
                DataTable Ddt = new DataTable();

                DataRow UrowHeader = Udt.NewRow();
                DataRow DrowHeader = Ddt.NewRow();

                if (iMaxCol < 18)
                {
                    //Column Width get
                    dColumnWidth = (dgAgingRack.ActualWidth - 50) / iMaxCol;
                    if (dColumnWidth > 50)
                    {
                        dColumnWidth = 150;
                    }

                    //Row Height get
                    dRowHeight = (dgAgingRack.ActualHeight) / iRowCnt;

                    //Column 생성
                    for (int i = 0; i < iColCnt; i++)
                    {
                        //GRID Column Create
                        if (i.Equals(0))
                        {
                            SetGridHeaderSingle((i + 1).ToString(), dgAgingRack, 50);
                            Udt.Columns.Add((i + 1).ToString(), typeof(string));
                        }
                        else
                        {
                            SetGridHeaderSingle((i + 1).ToString(), dgAgingRack, dColumnWidth);
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
                    dColumnWidth = (dgAgingRack.ActualWidth - 96) / (iMaxCol / 2);

                    //Row Height get
                    //dRowHeight = (dgAgingRack.ActualHeight) / (iRowCnt + 2); //20210402 Aging Rack 연 설정 부분 수정
                    dRowHeight = (dgAgingRack.ActualHeight) / iRowCnt; //20210402 Aging Rack 연 설정 부분 수정

                    for (int i = 0; i < iColCnt; i++)
                    {
                        //GRID Column Create
                        SetGridHeaderSingle((i + 1).ToString(), dgAgingRack, dColumnWidth);
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
                            //DrowHeader[i] = ((iMaxCol / 2) + i).ToString() + ObjectDic.Instance.GetObjectName("연"); //20210402 Aging Rack 연 설정 부분 수정
                            DrowHeader[i] = Convert.ToString(Convert.ToInt32(Math.Ceiling(Convert.ToDouble(iMaxCol) / 2)) + i) + ObjectDic.Instance.GetObjectName("연"); //20210402 Aging Rack 연 설정 부분 수정
                    }

                    for ( int k = 0; k < dtRack.Rows.Count; k++)
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
                            //Ddt.Rows[iMaxStg - iStgNum][iColNum - (iMaxCol / 2)] = k.ToString(); //20210402 Aging Rack 연 설정 부분 수정
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

                    DataRow[] drColor = dtColor.Select("CMCODE = '" + STATUS + "'");

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
                if (iMaxCol >= 18)
                {
                    Ddt.Rows.InsertAt(DrowHeader, 0);
                }

                //상,하 Merge
                Udt.Merge(Ddt, false, MissingSchemaAction.Add);
                _dtCopy = Udt.Copy();

                dgAgingRack.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                dgAgingRack.ItemsSource = DataTableConverter.Convert(Udt);
                dgAgingRack.UpdateLayout();

                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgAgingRack.Columns)
                    dgc.VerticalAlignment = VerticalAlignment.Center;

                for (int row = 0; row < dgAgingRack.Rows.Count; row++)
                {
                    for (int col = 0; col < dgAgingRack.Columns.Count; col++)
                    {
                        if (dgAgingRack.GetCell(row, col).Presenter == null) continue;

                        dgAgingRack.GetCell(row, col).Presenter.Padding = new Thickness(0);
                        dgAgingRack.GetCell(row, col).Presenter.Margin = new Thickness(0);
                    }
                }

                // 한눈에 보기 체크 해제시 컬럼 너비 자동조정.
                if (chkRackFull.IsChecked.Equals(true))
                {
                    dgAgingRack.FontSize = 11;
                }
                else
                {
                    dgAgingRack.FontSize = 12;
                    dgAgingRack.AllColumnsWidthAuto();
                    dgAgingRack.UpdateLayout();

                    double totalColWidth = dgAgingRack.Columns.Sum(x => x.Visibility != Visibility.Visible ? 0 : x.ActualWidth);

                    if (totalColWidth < dgAgingRack.ActualWidth - 70)
                    {
                        for (int col = 0; col < dgAgingRack.Columns.Count; col++)
                        {
                            if (dgAgingRack.Columns[col].Visibility != Visibility.Visible) continue;

                            dgAgingRack.Columns[col].Width = new C1.WPF.DataGrid.DataGridLength(dColumnWidth);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearControlHeader()
        {
            txtTotalUseRate.Text = string.Empty;  //전체사용률
            txtRowUseRate.Text = string.Empty;    //열 사용률
            txtCol.Text = string.Empty;           //연
            txtTrayID.Text = string.Empty;        //Tray ID
            txtRackCnt.Text = string.Empty;       //Rack 수
            txtPossibleCnt.Text = string.Empty;   //입고 가능 Rack 수
            txtImpossibleCnt.Text = string.Empty; //입고 금지 Rack 수
            txt1RackCnt.Text = string.Empty;      //1단 Rack 수
            txt2RackCnt.Text = string.Empty;      //2단 Rack 수
        }

        private void ClearControlRack()
        {
            txtTray1.Text = string.Empty;         //1단
            txtTray2.Text = string.Empty;         //2단
            txtModel.Text = string.Empty;         //모델명
            txtRoute.Text = string.Empty;         //Route id
            txtInDate.Text = string.Empty;        //입고일자
            txtScheduleDate.Text = string.Empty;  //출고예정일자

            txtSelRow.Text =  string.Empty;    //열
            txtSelCol.Text =  string.Empty;    //연
            txtSelStg.Text = string.Empty;    //단

            txtRemark.Text = string.Empty;    //입고금지사유

            cboSetMode.SelectedIndex = 0;
        }

        private void GetEqpInfo(string sEqpId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("RACK_ID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["RACK_ID"] = sEqpId;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RACK_RETRIEVE_INFO", "RQSTDT", "RSLTDT", dtRqst);

                //2023.06.29 Tray가 2단적재일 경우 출고예정일자 비교 후 나중인 출고예정일자를 표시
                if(dtRslt.Rows.Count == 2)
                {
                    int nControl = 0;
                    string[] sArr = new string[dtRslt.Rows.Count];

                    foreach(DataRow row in dtRslt.Rows)
                    {
                        string sPLANTIME = Util.NVC(row["PLANTIME"]);
                        sArr[nControl] = sPLANTIME;
                        nControl++;
                    }

                    if(sArr[0] != "" && sArr[1] != "")  // 1단,2단 출고예정일자가 Null이 아니면
                    {
                        int nResult = DateTime.Compare(Convert.ToDateTime(sArr[0]), Convert.ToDateTime(sArr[1]));

                        switch (nResult)
                        {
                            case 1:  //앞에 날짜가 클경우(1단)
                                foreach (DataRow row in dtRslt.Rows)
                                {
                                    if (Util.NVC(row["PLANTIME"]).Equals(sArr[0]))
                                    {
                                        txtTray1.Text = Util.NVC(row["CSTID"]);
                                        txtModel.Text = Util.NVC(row["MDLLOT_ID"]);
                                        txtRoute.Text = Util.NVC(row["ROUTID"]);
                                        txtInDate.Text = Util.NVC(row["STARTTIME"]);
                                        txtScheduleDate.Text = Util.NVC(row["PLANTIME"]);
                                    }
                                    else
                                    {
                                        txtTray2.Text = Util.NVC(row["CSTID"]);
                                    }
                                }
                                break;

                            case -1:  //뒤에 날짜가 클경우(2단)
                                foreach (DataRow row in dtRslt.Rows)
                                {
                                    if (Util.NVC(row["PLANTIME"]).Equals(sArr[1]))
                                    {
                                        txtTray2.Text = Util.NVC(row["CSTID"]);
                                        txtModel.Text = Util.NVC(row["MDLLOT_ID"]);
                                        txtRoute.Text = Util.NVC(row["ROUTID"]);
                                        txtInDate.Text = Util.NVC(row["STARTTIME"]);
                                        txtScheduleDate.Text = Util.NVC(row["PLANTIME"]);
                                    }
                                    else
                                    {
                                        txtTray1.Text = Util.NVC(row["CSTID"]);
                                    }
                                }
                                break;

                            case 0:  //날짜가 같을경우
                                foreach (DataRow row in dtRslt.Rows)
                                {
                                    if (Util.NVC(row["CST_LOAD_LOCATION_CODE"]).Equals("1"))
                                    {
                                        txtTray1.Text = Util.NVC(row["CSTID"]);
                                        txtModel.Text = Util.NVC(row["MDLLOT_ID"]);
                                        txtRoute.Text = Util.NVC(row["ROUTID"]);
                                        txtInDate.Text = Util.NVC(row["STARTTIME"]);
                                        txtScheduleDate.Text = Util.NVC(row["PLANTIME"]);
                                    }
                                    else
                                    {
                                        txtTray2.Text = Util.NVC(row["CSTID"]);
                                    }
                                }
                                break;
                        }
                    }
                    else  // 1단,2단 출고예정일자가 하나라도 Null일경우 1단으로 표기
                    {
                        foreach (DataRow row in dtRslt.Rows)
                        {
                            if (Util.NVC(row["CST_LOAD_LOCATION_CODE"]).Equals("1"))
                            {
                                txtTray1.Text = Util.NVC(row["CSTID"]);
                                txtModel.Text = Util.NVC(row["MDLLOT_ID"]);
                                txtRoute.Text = Util.NVC(row["ROUTID"]);
                                txtInDate.Text = Util.NVC(row["STARTTIME"]);
                                txtScheduleDate.Text = Util.NVC(row["PLANTIME"]);
                            }
                            else
                            {
                                txtTray2.Text = Util.NVC(row["CSTID"]);
                            }
                        }
                    }

                }
                else if (dtRslt.Rows.Count > 0 && dtRslt.Rows.Count < 2) //2021.04.19 1단/2단 Tray ID 출력 정보 오류 수정 START
                {
                    foreach (DataRow row in dtRslt.Rows)
                    {
                        if (Util.NVC(row["CST_LOAD_LOCATION_CODE"]).Equals("1"))
                        {
                            txtTray1.Text = Util.NVC(row["CSTID"]);
                            txtModel.Text = Util.NVC(row["MDLLOT_ID"]);
                            txtRoute.Text = Util.NVC(row["ROUTID"]);
                            txtInDate.Text = Util.NVC(row["STARTTIME"]);
                            txtScheduleDate.Text = Util.NVC(row["PLANTIME"]);
                        }
                        else
                        {
                            txtTray2.Text = Util.NVC(dtRslt.Rows[1]["CSTID"]);
                        }
                    }
                }

                //if (dtRslt.Rows.Count > 1)
                //{
                //    txtTray2.Text = Util.NVC(dtRslt.Rows[1]["CSTID"]);
                //}
                //2021.04.19 1단/2단 Tray ID 출력 정보 오류 수정 END
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                IsReadOnly = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
            });
       }

        private void SetEqptAgingSc(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ATTR3", typeof(string));
                RQSTDT.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ATTR3"] = "N";
                dr["EQPT_GR_TYPE_CODE"] = cboAgingType.GetStringValue("ATTR1");
                dr["LANE_ID"] = cboAgingType.GetStringValue("ATTR2");

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_EQP_SC_HVF", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnAgingStart_Click(object sender, RoutedEventArgs e)
        {

            if (dgAgingRack.Selection.SelectedCells.Count == 0)
            {
                Util.MessageValidation("SUF9002"); //Rack 위치를 먼저 선택해주세요.
                return;
            }

            if (dgAgingRack.Selection.SelectedCells.Count > 1)
            {
                Util.MessageValidation("FM_ME_0569"); //하나의 Rack만 선택해주세요.
                return;
            }

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgAgingRack.Selection.SelectedCells)
            {

                if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0))
                {
                    Util.MessageValidation("70003"); //Rack 정보가 없습니다.
                    return;
                }
                else
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("RACK_ID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["RACK_ID"] = cell.Presenter.Tag.ToString();
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ENABLE_RACK", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt.Rows.Count == 1)
                    {
                        if (dtRslt.Rows[0]["RACK_STAT_CODE"].ToString().Equals("USING"))
                        {
                            Util.MessageValidation("101124"); //사용중인 RACK입니다.
                            return;
                        }
                        else if (dtRslt.Rows[0]["RACK_STAT_CODE"].ToString().Equals("USABLE") && string.IsNullOrEmpty(dtRslt.Rows[0]["ABNORM_STAT_CODE"].ToString()))
                        {
                            FCS001_315_MANUAL_START AgingStartPopup = new FCS001_315_MANUAL_START();
                            AgingStartPopup.FrameOperation = FrameOperation;

                            object[] parameters = new object[6];
                            parameters[0] = cell.Presenter.Tag.ToString();
                            parameters[1] = dtRslt.Rows[0]["EQPTID"].ToString();
                            parameters[2] = cboAgingType.GetStringValue("CBO_NAME");
                            parameters[3] = txtSelRow.Text;
                            parameters[4] = txtSelCol.Text;
                            parameters[5] = txtSelStg.Text;

                            C1WindowExtension.SetParameters(AgingStartPopup, parameters);
                            AgingStartPopup.Closed += new EventHandler(AgingStartPopup_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => AgingStartPopup.ShowModal()));
                            AgingStartPopup.BringToFront();
                        }

                        else
                        {
                            Util.MessageValidation("70004"); //사용할 수 없는 Rack 입니다.
                            return;
                        }
                    }
                }
            }
        }

        private void btnAgingEnd_Click(object sender, RoutedEventArgs e)
        {

            if (dgAgingRack.Selection.SelectedCells.Count == 0)
            {
                Util.MessageValidation("SUF9002"); //Rack 위치를 먼저 선택해주세요.
                return;
            }

            if (dgAgingRack.Selection.SelectedCells.Count > 1)
            {
                Util.MessageValidation("FM_ME_0569"); //하나의 Rack만 선택해주세요.
                return;
            }

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgAgingRack.Selection.SelectedCells)
            {
                if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0))
                {
                    Util.MessageValidation("70003"); //Rack 정보가 없습니다.
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(txtTray1.Text))
                    {
                        Util.MessageValidation("FM_ME_0563");  //진행중인 TRAY 가 존재하지 않습니다.
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("RACK_ID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["RACK_ID"] = cell.Presenter.Tag.ToString();
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ENABLE_RACK", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtRslt.Rows.Count == 1)
                    {
                        if (dtRslt.Rows[0]["RACK_STAT_CODE"].ToString().Equals("USABLE") || dtRslt.Rows[0]["RACK_STAT_CODE"].ToString().Equals("UNUSE"))
                        {
                            Util.MessageValidation("FM_ME_0563"); //진행중인 TRAY 가 존재하지 않습니다.
                            return;
                        }

                        else if (dtRslt.Rows[0]["RACK_STAT_CODE"].ToString().Equals("USING"))
                        {
                            FCS001_315_MANUAL_END AgingEndPopup = new FCS001_315_MANUAL_END();
                            AgingEndPopup.FrameOperation = FrameOperation;

                            object[] parameters = new object[8];
                            parameters[0] = cell.Presenter.Tag.ToString();
                            parameters[1] = dtRslt.Rows[0]["EQPTID"].ToString();
                            parameters[2] = cboAgingType.GetStringValue("CBO_NAME");
                            parameters[3] = txtSelRow.Text;
                            parameters[4] = txtSelCol.Text;
                            parameters[5] = txtSelStg.Text;
                            parameters[6] = txtTray1.Text;
                            parameters[7] = txtTray2.Text;
                            
                            C1WindowExtension.SetParameters(AgingEndPopup, parameters);
                            AgingEndPopup.Closed += new EventHandler(AgingEndPopup_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => AgingEndPopup.ShowModal()));
                            AgingEndPopup.BringToFront();
                        }

                        else
                        {
                            Util.MessageValidation("70004"); //사용할 수 없는 Rack 입니다.
                            return;
                        }
                    }
                }
            }
        }

        private void AgingStartPopup_Closed(object sender, EventArgs e)
        {
            FCS001_315_MANUAL_START window = sender as FCS001_315_MANUAL_START;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                ClearControlRack();
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void AgingEndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_315_MANUAL_END window = sender as FCS001_315_MANUAL_END;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                ClearControlRack();
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        #endregion

    }
}
