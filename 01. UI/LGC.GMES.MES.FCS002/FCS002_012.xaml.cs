/*************************************************************************************
 Created Date : 2023.01.19
      Creator : KANG DONG HEE
   Decription : Aging Rack 현황
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2023.01.19  DEVELOPER : Initial Created.
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
    public partial class FCS002_012 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Hashtable hash_loss_color = new Hashtable();
        private DataTable dtColor = new DataTable();
        private DataTable dtTemp = new DataTable();
        private DataTable _dtCopy = new DataTable();

        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;
        private string sRACK_ID = string.Empty;
        #endregion

        #region Initialize
        public FCS002_012()
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
            tcRackInfo.SelectedIndex = 0;


            SetTrayStack();

            //Combo Setting
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        
        private void SetTrayStack()
        {
            //Tray 최대 단 수 설정
            int iMaxStg = 8; 

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(string));
          
            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_MAX_STG_MB", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count > 0)
                iMaxStg = Int32.Parse(dtResult.Rows[0]["MAXSTG"].ToString());
           
            TextBlock[] tbRack = {text1RackCnt , text2RackCnt, text3RackCnt, text4RackCnt,
                                  text5RackCnt, text6RackCnt, text7RackCnt, text8RackCnt };
            TextBox[] tbRack1 = {txt1RackCnt , txt2RackCnt, txt3RackCnt, txt4RackCnt,
                                  txt5RackCnt, txt6RackCnt, txt7RackCnt, txt8RackCnt };
            TextBlock[] tbTray = { textTray1, textTray2, textTray3, textTray4, textTray5, textTray6, textTray7, textTray8 };
            TextBox[] tbTray1 = { txtTray1, txtTray2, txtTray3, txtTray4, txtTray5, txtTray6, txtTray7, txtTray8 };
        

            for (int i = iMaxStg; i<8;i++)
            {
                tbRack[i].Visibility = Visibility.Collapsed;
                tbRack1[i].Visibility = Visibility.Collapsed;
                tbTray[i].Visibility = Visibility.Collapsed;
                tbTray1[i].Visibility = Visibility.Collapsed;
            }


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
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboAgingType);
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
                dr["CMCDTYPE"] = "FORM_AGINGSTATUS_MCC";

                dtRqst.Rows.Add(dr);

                dtColor = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", dtRqst);

                SetColorGrid(dtColor);

                hash_loss_color = DataTableConverter.ToHash(dtColor);

                //foreach (DataRow drRslt in dtColor.Rows)
                //{
                //    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                //    cbItem.Content = drRslt["CBO_NAME"];
                //    cbItem.Foreground = ColorToBrush(System.Drawing.Color.FromName(drRslt["ATTRIBUTE2"].ToString()));
                //    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["ATTRIBUTE1"].ToString()));
                //    cboColorLegend.Items.Add(cbItem);
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

            GeColorLegend();
            string[] sFilter1 = { "FORM_AGING_TYPE_CODE", string.Empty };
            _combo.SetCombo(cboAgingType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter1);
            string[] sFilter2 = { "COMBO_RACK_STAT_SET_CODE" };
            _combo.SetCombo(cboSetMode, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "CMN", sFilter: sFilter2);
        }
        #endregion

        #region Event
        private void cboAgingType_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            GetCommonCode();

            cboSCLine.Text = string.Empty;
            string[] sFilter = { EQPT_GR_TYPE_CODE, LANE_ID };
            _combo.SetCombo(cboSCLine, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SCLINE", sFilter: sFilter); //20210331 S/C 호기 필수 값으로 변경
        }

        private void cboSCLine_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            cboRow.Text = string.Empty;
            object[] objParent = { EQPT_GR_TYPE_CODE, LANE_ID, Util.GetCondition(cboSCLine) };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AGING_ROW", objParent: objParent);
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

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_RETRIEVE_RACK_MB", "RQSTDT", "RSLTDT", dtRqst);

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

                                            dgAgingRack.Selection.Clear();
                                            dgAgingRack.ScrollIntoView(iRow, iCol);
                                            dgAgingRack.GetCell(iRow, iCol).Presenter.IsSelected = true;
                                            
                                            C1.WPF.DataGrid.DataGridCell cell = dgAgingRack.GetCell(iRow, iCol);
                                        

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
                            Util.MessageValidation("FM_ME_0322", new string[] { txtTrayID.Text, dtRslt.Rows[0]["AGING_TYPE_NAME"].ToString() + "\n"+  dtRslt.Rows[0]["EQPTNAME"].ToString(), dtRslt.Rows[0]["ROW_NO"].ToString(), dtRslt.Rows[0]["COL_NO"].ToString(), dtRslt.Rows[0]["STAGE_NO"].ToString() });
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

        private void btnAgingTrayList_Click(object sender, RoutedEventArgs e)
        {
            //연계 화면 확인 후 수정
            Load_FCS002_012_AGING_TRAY_LIST(Util.NVC(cboAgingType.SelectedValue), Util.NVC(cboSCLine.SelectedValue), Util.NVC(cboRow.SelectedValue));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void cboSetMode_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.GetCondition(cboSetMode).Equals("DISABLE"))  //입고금지
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
            //if (dgAgingRack.CurrentRow == null || dgAgingRack.SelectedIndex == -1)
            //{
            //    Util.MessageValidation("SUF9002");  //RACK 위치를 먼저 선택해주세요
            //    return;
            //}
            if (string.IsNullOrEmpty(sRACK_ID)) 
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

                if (sStatus.Equals("DISABLE") && string.IsNullOrEmpty(txtRemark.Text.Trim()))
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

                //foreach (C1.WPF.DataGrid.DataGridCell cell in dgAgingRack.Selection.SelectedCells)
                //{
                //    // 20231214 게체 참고 Null Vaildation 추가
                //    if (cell.Presenter.Tag  == null)
                //    {
                //        Util.MessageValidation("SUF9002");  //RACK 위치를 먼저 선택해주세요
                //        return;
                //    }

                //    DataRow dr = dtRqst.NewRow();
                //    dr["RACK_STAT_CODE"] = sStatus;
                //    dr["RACK_ID"] = cell.Presenter.Tag.ToString();

                //    if (EQPT_GR_TYPE_CODE.Equals("3") || EQPT_GR_TYPE_CODE.Equals("4")
                //         || EQPT_GR_TYPE_CODE.Equals("9") || EQPT_GR_TYPE_CODE.Equals("7")) //상온 || 고온  정보삭제일 경우
                //    {
                //        if (sStatus.Equals("INIT_RACK"))
                //        {
                //            dr["RACK_INFO_DEL_FLAG"] = 1;
                //        }
                //    }

                //    dr["RCV_PRHB_RSN"] = Util.GetCondition(txtRemark);
                //    dr["USERID"] = LoginInfo.USERID;
                //    dtRqst.Rows.Add(dr);
                //}

              

                    DataRow dr = dtRqst.NewRow();
                    dr["RACK_STAT_CODE"] = sStatus;
                    dr["RACK_ID"] = sRACK_ID;

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
                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_RACK_EQP_STATUS_MB", "INDATA", "OUTDATA", dtRqst); 
                if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0"))
                {
                    if (sStatus.ToString().Equals("DISABLE"))
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

                    FCS002_001_TRAY_HISTORY wndHist = new FCS002_001_TRAY_HISTORY();
                    wndHist.FrameOperation = FrameOperation;

                    if (wndHist != null)
                    {
                        object[] parameters = new object[6];
                        parameters[0] = Util.GetCondition(cboRow);
                        parameters[1] = string.Format("{0:00}", Convert.ToInt16(sCol));
                        parameters[2] = string.Format("{0:00}", Convert.ToInt16(sStg));
                        parameters[3] = cell.Presenter.Tag.ToString();
                        parameters[4] = "R";
                        parameters[5] = sRACK_ID;

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
            FCS002_001_TRAY_HISTORY window = sender as FCS002_001_TRAY_HISTORY;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void dgAgingRack_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ClearControlRack();
                sRACK_ID = string.Empty;
                // 선택 적재RACK 정보 표시
                tcRackInfo.SelectedIndex = 1;
               
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
                    sRACK_ID = RACKID;
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


        private void btnAbnormal_Click(object sender, RoutedEventArgs e)
        {
            //확인 메세지 팝업

            FCS002_012_ABNORMAL abRack = new FCS002_012_ABNORMAL();
            abRack.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = cboAgingType.GetStringValue("ATTR2");   //LANE_ID
            parameters[1] = cboAgingType.GetStringValue("ATTR1");   //AGING 구분
            parameters[2] = Util.GetCondition(cboRow);   //열
            parameters[3] = Util.GetCondition(cboSCLine);   //EQPID

            C1WindowExtension.SetParameters(abRack, parameters);
            abRack.Closed += new EventHandler(sabRack_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => abRack.ShowModal()));
            abRack.BringToFront();
        }


        private void sabRack_Closed(object sender, EventArgs e)
        {
            FCS002_012_ABNORMAL window = sender as FCS002_012_ABNORMAL;

            this.grdMain.Children.Remove(window);
        }

        private void Load_FCS002_012_AGING_TRAY_LIST(string sAgingType, string sSCLine, string sRow)
        {
            //Tray List
            FCS002_012_AGING_TRAY_LIST TrayList = new FCS002_012_AGING_TRAY_LIST();
            TrayList.FrameOperation = FrameOperation;

            object[] Parameters = new object[19];
            Parameters[0] = sAgingType; //Aging 구분
            Parameters[1] = sSCLine;    //S/C Line
            Parameters[2] = sRow;       //열

            this.FrameOperation.OpenMenuFORM("FCS002_012_AGING_TRAY_LIST", "FCS002_012_AGING_TRAY_LIST", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("AGING_TRAY_LIST"), true, Parameters);
        }

        private void txtTray1_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtTray1.Text)))
                {
                    string sTrayID = Util.NVC(txtTray1.Text);

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = sTrayID; // TRAY ID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void txtTray2_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtTray2.Text)))
                {
                    string sTrayID = Util.NVC(txtTray2.Text);

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = sTrayID; // TRAY ID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void txtTray3_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtTray3.Text)))
                {
                    string sTrayID = Util.NVC(txtTray3.Text);

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = sTrayID; // TRAY ID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void txtTray4_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtTray4.Text)))
                {
                    string sTrayID = Util.NVC(txtTray4.Text);

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = sTrayID; // TRAY ID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void txtTray5_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtTray5.Text)))
                {
                    string sTrayID = Util.NVC(txtTray5.Text);

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = sTrayID; // TRAY ID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void txtTray6_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtTray6.Text)))
                {
                    string sTrayID = Util.NVC(txtTray6.Text);

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = sTrayID; // TRAY ID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void txtTray7_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtTray7.Text)))
                {
                    string sTrayID = Util.NVC(txtTray7.Text);

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = sTrayID; // TRAY ID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void txtTray8_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtTray8.Text)))
                {
                    string sTrayID = Util.NVC(txtTray8.Text);

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = sTrayID; // TRAY ID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
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
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANE_ID"] = LANE_ID;
                dr["EQPT_GR_TYPE_CODE"] = EQPT_GR_TYPE_CODE;
                dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow);
                dr["EQPTID"] = Util.GetCondition(cboSCLine);
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                new ClientProxy().ExecuteService_Multi("BR_GET_AGING_RACK_RETRIEVE_MB", "INDATA", "RACK1,RACK2,CHARGE", (bizResult, bizException) =>
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
                            txtTotalUseRate.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["RATE_ALL"]);   //전체사용률
                            txtRowUseRate.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["CHGRATE1"]);     //열 사용률
                            txtAllLoad.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["STACK_ALL"]);       // 전체 적재율
                            txtLoad.Text =  Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["STACK_PER"]);          // 적재율
                            txtLoadSTG.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["STACK_STG"]);     //적재율(단)
                            txtEmptyCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["EMPTY_TRAY"]);  //공 트레이 입고 수량
                            txtEmptyload.Text = Util.NVC(bizResult.Tables["CHARGE"].Rows[0]["STACK_PER_E"]);
                            // 공 트레이 적재율 : 공 트레이 입고수량 / (전체 렉 갯수 * 6)
                            //if (double.Parse(Util.NVC(bizResult.Tables["RACK1"].Rows[0]["EMPTY_TRAY"].ToString())) > 0)
                            //{
                            //    double empty_tray_per = double.Parse(Util.NVC(bizResult.Tables["RACK1"].Rows[0]["EMPTY_TRAY"])) / (double.Parse(Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKCOUNT"])) * 8);

                            //    txtEmptyload.Text = empty_tray_per.ToString("F");
                            //}
                            //else
                            //{
                            //    txtEmptyload.Text = "0";
                            //}

                            txtRackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKCOUNT"]);         //Rack 수
                            txt1RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_ONE"]);         //1단 Rack 수
                            txt2RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_TWO"]);         //2단 Rack 수
                            txt3RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_THREE"]);       //3단 Rack 수
                            txt4RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_FOUR"]);        //4단 Rack 수
                            txt5RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_FIVE"]);        //5단 Rack 수
                            txt6RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_SIX"]);         //6단 Rack 수
                            txt7RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_SEVEN"]);       //7단 Rack 수
                            txt8RackCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_EIGHT"]);       //8단 Rack 수

                            txtEmptytrayCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACK_EMPTY_TRAY"]);   //공 트레이 입고 수량
                            txtPossibleCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKIBGO"]);      //입고 가능 Rack 수
                            txtImpossibleCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKGEUMGI"]);  //입고 금지 Rack 수

                            txtTroubleCnt.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKTROUBLE"]);    //입고 이상 Rack 수
                            txtImpossible.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKGEUMGI_PER"]); //입고 금지 %
                            txtTrouble.Text = Util.NVC(bizResult.Tables["RACK1"].Rows[0]["RACKTROUBLE_PER"]);   //입고 이상 %
                        }

                        dtTemp = null;
                        if (bizResult.Tables["RACK1"].Rows.Count > 0)
                        {
                            dtTemp = bizResult.Tables["RACK2"];

                            SetRackStatus(dtTemp);
                        }

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
                if (iMaxCol % 2 != 0)
                    iMaxCol = iMaxCol + 1;

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
                    dColumnWidth = (dgAgingRack.ActualWidth) / ((iMaxCol / 2)+1);

                    //Row Height get
                    dRowHeight = (dgAgingRack.ActualHeight) / iRowCnt ; //20210402 Aging Rack 연 설정 부분 수정

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
                string ATEMP = string.Empty;
                for (int k = 0; k < dtRack.Rows.Count; k++)
                {
                    iCOL = int.Parse(dtRack.Rows[k]["EQP_COL_LOC"].ToString());
                    iSTG = int.Parse(dtRack.Rows[k]["EQP_STG_LOC"].ToString());

                    RACK_ID = Util.NVC(dtRack.Rows[k]["RACK_ID"].ToString());
                    STATUS = Util.NVC(dtRack.Rows[k]["STATUS"].ToString());

                    if(rdoTray.IsChecked == true)
                        TRAYCNT = Util.NVC(dtRack.Rows[k]["TRAYCNT"].ToString());
                    else
                        TRAYCNT = Util.NVC(String.Format("{0:0.0}",dtRack.Rows[k]["TMPR_VALUE"]));

                    string BCOLOR = "Black";
                    string FCOLOR = "White";
                    string TEXT = string.Empty;

                    DataRow[] drColor = dtColor.Select("CBO_CODE = '" + STATUS + "'");

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

                //dgAgingRack.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                dgAgingRack.RowHeight = new C1.WPF.DataGrid.DataGridLength(Math.Truncate(dRowHeight), DataGridUnitType.Pixel);
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

        private void SetTempStatus(DataTable dtRack)
        {
            try
            {
                ClearControlHeader();
                ClearControlRack();

                dgAgingRack.Columns.Clear();
                dgAgingRack.Refresh();
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
                //dtRack.Columns.Add("BCOLOR", typeof(string));
                //dtRack.Columns.Add("FCOLOR", typeof(string));
                //dtRack.Columns.Add("TEXT", typeof(string));
                //dtRack.Columns.Add("RACKID", typeof(string));

                string EQPTID = string.Empty;
                int iCOL;
                int iSTG;

                string RACK_ID = string.Empty;
                string STATUS = string.Empty;
                string TRAYCNT = string.Empty;
                string ATEMP = string.Empty;
                for (int k = 0; k < dtRack.Rows.Count; k++)
                {
                    iCOL = int.Parse(dtRack.Rows[k]["EQP_COL_LOC"].ToString());
                    iSTG = int.Parse(dtRack.Rows[k]["EQP_STG_LOC"].ToString());

                    RACK_ID = Util.NVC(dtRack.Rows[k]["RACK_ID"].ToString());
                    STATUS = Util.NVC(dtRack.Rows[k]["STATUS"].ToString());

                    if (rdoTray.IsChecked == true)
                        TRAYCNT = Util.NVC(dtRack.Rows[k]["TRAYCNT"].ToString());
                    else if (rdoTemp.IsChecked == true)
                        TRAYCNT = Util.NVC(String.Format("{0:0.0}", dtRack.Rows[k]["TMPR_VALUE"]));
                    else
                        TRAYCNT = Util.NVC(String.Format("{0:0.0}", dtRack.Rows[k]["AVG_TMPR_VALUE"]));

                    string BCOLOR = "Black";
                    string FCOLOR = "White";
                    string TEXT = string.Empty;

                    DataRow[] drColor = dtColor.Select("CBO_CODE = '" + STATUS + "'");

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

                //dgAgingRack.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                dgAgingRack.RowHeight = new C1.WPF.DataGrid.DataGridLength(Math.Truncate(dRowHeight), DataGridUnitType.Pixel);
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
            txtCol.Text = string.Empty;           //연
            txtTrayID.Text = string.Empty;        //Tray ID

            txtTotalUseRate.Text = string.Empty;    //전체사용률
            txtRowUseRate.Text = string.Empty;      //열 사용률
            txtAllLoad.Text = string.Empty;         //전체 적재율
            txtLoad.Text = string.Empty;            //적재율
            txtLoadSTG.Text = string.Empty;            //적재율
            txtEmptytrayCnt.Text = string.Empty;    //공 트레이 입고 수량
            txtEmptyload.Text = string.Empty;       //공 트레이 적재율

            txtRackCnt.Text = string.Empty;         //Rack 수
            txt1RackCnt.Text = string.Empty;        //1단 Rack 수
            txt2RackCnt.Text = string.Empty;        //2단 Rack 수
            txt3RackCnt.Text = string.Empty;        //3단 Rack 수
            txt4RackCnt.Text = string.Empty;        //4단 Rackㄴ수
            txt5RackCnt.Text = string.Empty;        //5단 Rack 수
            txt6RackCnt.Text = string.Empty;        //6단 Rack 수
            txt7RackCnt.Text = string.Empty;        //7단 Rack 수
            txt8RackCnt.Text = string.Empty;        //8단 Rack 수
            txtEmptyCnt.Text = string.Empty;        //공 트레이 입고 수량
            txtPossibleCnt.Text = string.Empty;     //입고 가능 Rack 수
            txtImpossibleCnt.Text = string.Empty;   //입고 금지 Rack 수
            txtTroubleCnt.Text = string.Empty;      //입고 이상 Rack 수
            txtImpossible.Text = string.Empty;      //입고 금지 %
            txtTrouble.Text = string.Empty;         //입고 이상 %
        }

        private void ClearControlRack()
        {
            txtTray1.Text = string.Empty;         //1단
            txtTray2.Text = string.Empty;         //2단
            txtTray3.Text = string.Empty;         //3단
            txtTray4.Text = string.Empty;         //4단
            txtTray5.Text = string.Empty;         //5단
            txtTray6.Text = string.Empty;         //6단
            txtTray7.Text = string.Empty;         //7단
            txtTray8.Text = string.Empty;         //8단
            txtModel.Text = string.Empty;         //모델명
            txtRoute.Text = string.Empty;         //Route id
            txtInDate.Text = string.Empty;        //입고일자
            txtScheduleDate.Text = string.Empty;  //출고예정일자

            txtSelRow.Text =  string.Empty;       //열
            txtSelCol.Text =  string.Empty;       //연
            txtSelStg.Text = string.Empty;        //단
            txtRemark.Text = string.Empty;        //입고금지사유

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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_RACK_RETRIEVE_INFO_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    DataRow datarow = dtRslt.Rows[0];
                    //txtModel.Text = Util.NVC(datarow["MDLLOT_ID"]);
                    //txtRoute.Text = Util.NVC(datarow["ROUTID"]);
                    //txtInDate.Text = Util.NVC(datarow["STARTTIME"]);
                    //txtScheduleDate.Text = Util.NVC(datarow["PLANTIME"]);

                    foreach (DataRow row in dtRslt.Rows)
                    {
                        switch (Util.NVC(row["CST_LOAD_LOCATION_CODE"]))
                        {
                            case "1":
                                txtTray1.Text = Util.NVC(row["CSTID"]);
                                // 대표트레이의 시작시간, 출고시간을 보여줌 .
                                txtModel.Text = Util.NVC(row["MDLLOT_ID"]);
                                txtRoute.Text = Util.NVC(row["ROUTID"]);
                                txtInDate.Text = Util.NVC(row["STARTTIME"]);
                                txtScheduleDate.Text = Util.NVC(row["PLANTIME"]);
                                break;
                            case "2":
                               // txtTray2.Text = Util.NVC(dtRslt.Rows[1]["CSTID"]);
                                txtTray2.Text = Util.NVC(row["CSTID"]);
                                break;
                            case "3":
                                txtTray3.Text = Util.NVC(row["CSTID"]);
                                break;
                            case "4":
                                txtTray4.Text = Util.NVC(row["CSTID"]);
                                break;
                            case "5":
                                txtTray5.Text = Util.NVC(row["CSTID"]);
                                break;
                            case "6":
                                txtTray6.Text = Util.NVC(row["CSTID"]);
                                break;
                            case "7":
                                txtTray7.Text = Util.NVC(row["CSTID"]);
                                break;
                            case "8":
                                txtTray8.Text = Util.NVC(row["CSTID"]);
                                break;
                        }
                    }
                }
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

        #endregion

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void rdoTray_Checked(object sender, RoutedEventArgs e)
        {
            if (dtTemp.Rows.Count > 0)
            {
                rdoTemp.IsChecked = false;

                SetTempStatus(dtTemp);
            }
        }

        private void rdoTemp_Checked(object sender, RoutedEventArgs e)
        {
            if (dtTemp.Rows.Count > 0)
            {
                rdoTray.IsChecked = false;

                SetTempStatus(dtTemp);
            }
        }
    }
}
