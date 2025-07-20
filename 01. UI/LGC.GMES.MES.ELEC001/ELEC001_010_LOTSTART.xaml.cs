/*************************************************************************************
 Created Date : 2016.08.19
      Creator : 
   Decription : 작업시작 대기 Lot List 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  2020.07.15  오화백K : DA_PRD_SEL_WAIT_WIP_RP를 BR_PRD_SEL_WAIT_WIP_RP로 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_LOTSTART
    /// </summary>
    public partial class ELEC001_010_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _WORKORDER = string.Empty;
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _LOTID = string.Empty;
        private string _CSTID = string.Empty;
        private string _MTRLID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _PRODID = string.Empty;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        

        public ELEC001_010_LOTSTART()
        {
            InitializeComponent();
            ApplyPermissions();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //object[] tmps = C1WindowExtension.GetParameters(this);

                //if (tmps == null)
                //{
                //    return;
                //}

                //_PROCID = Util.NVC(tmps[0]);
                //_EQPTID = Util.NVC(tmps[1]);
                //_EQSGID = Util.NVC(tmps[2]);
                //_LOTID = Util.NVC(tmps[3]);

                if (!GetLotInfo())
                {
                    this.DialogResult = MessageBoxResult.Cancel;
                    return;
                }
                SelectRowByLotID();

                SetIdentInfo();

                dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;

                if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                {
                    dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                }

                tbxRwCstId.Visibility = Visibility.Collapsed;
                tblRwCstId.Visibility = Visibility.Collapsed;

                if (_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                {
                    tbxRwCstId.Visibility = Visibility.Visible;
                    tblRwCstId.Visibility = Visibility.Visible;
                }

                // TANDEM모델설비만 텐덤 체크
                if (string.Equals(GetTandemEqupment(), "Y"))
                    chkTandemMode.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public ELEC001_010_LOTSTART(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("PROCID")) _PROCID = dicParam["PROCID"];
                if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("LOTID"))  _LOTID  = dicParam["LOTID"];
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return;
            }

            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("IN_EQP");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("MULTI_ROLLPRESS_FLAG", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = _EQPTID;
                        row["USERID"] = LoginInfo.USERID;

                        if (chkTandemMode.Visibility == Visibility.Visible)
                            row["MULTI_ROLLPRESS_FLAG"] = chkTandemMode.IsChecked == true ? "Y" : "N";

                        inData.Rows.Add(row);

                        DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
                        inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string)); //투입 코터lot의 proid
                        inMtrl.Columns.Add("INPUT_LOTID", typeof(string));
                        inMtrl.Columns.Add("CSTID", typeof(string));

                        ////투입자재 장착위치 가져오기
                        DataTable dt = new DataTable();
                        dt.Columns.Add("EQPTID", typeof(string));

                        DataRow tmprow = dt.NewRow();
                        tmprow["EQPTID"] = _EQPTID;
                        dt.Rows.Add(tmprow);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", dt);
                        if (dtResult == null || dtResult.Rows.Count <= 0)
                        {
                            Util.MessageValidation("SFU2019");  //해당 설비의 자재투입부를 MMD에서 입력해주세요.
                            return;
                        }

                        row = inMtrl.NewRow();
                        row["EQPT_MOUNT_PSTN_ID"] = dtResult.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                        row["EQPT_MOUNT_PSTN_STATE"] = "A";
                        row["INPUT_LOTID"] = _LOTID;
                        row["CSTID"] = _CSTID;
                        inMtrl.Rows.Add(row);

                        if(_UNLDR_LOT_IDENT_BAS_CODE == "CST_ID" || _UNLDR_LOT_IDENT_BAS_CODE == "RF_ID")
                        {
                            DataTable inOutput = indataSet.Tables.Add("IN_OUTPUT");
                            inOutput.Columns.Add("OUT_CSTID", typeof(string));

                            DataRow inOutputRow = inOutput.NewRow();
                            inOutputRow["OUT_CSTID"] = tbxRwCstId.Text.ToString(); 
                            inOutput.Rows.Add(inOutputRow);
                        }

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_LOT_RP_EIF", "INDATA,IN_MTRL,IN_OUTPUT", "OUT_LOT,OUT_MSG", indataSet);

                        DataTable msg = dsRslt.Tables["OUT_MSG"];
                        if (msg.Rows[0]["MSGTYPE"].Equals("OK"))
                        {
                            Util.AlertInfo("SFU1275");  //정상처리되었습니다.
                        }
                        else// INFO이거나 NG일 경우
                        {
                            DataTable code = new DataTable();
                            code.Columns.Add("LANGID", typeof(string));
                            code.Columns.Add("MSGID", typeof(string));

                            DataRow rw = code.NewRow();
                            rw["LANGID"] = LoginInfo.LANGID;
                            rw["MSGID"] = "1" + msg.Rows[0]["MSGCODE"]; //기존 메시지 변경 불가로 신규 메시지로 처리

                            code.Rows.Add(rw);

                            DataTable message = new ClientProxy().ExecuteServiceSync("DA_SEL_MESSAGE_LOT_START_RP", "INDATA", "RSLTDT", code);

                            if (message == null)
                            {
                                Util.MessageValidation("SFU1392");  //MESSAGE 테이블에 메세지를 등록해주세요
                                return;
                            }

                            if (message.Rows.Count == 0)
                            {
                                Util.MessageValidation("SFU1392");  //MESSAGE 테이블에 메세지를 등록해주세요
                                return;
                            }

                            //Util.AlertInfo("착공완료 \r\n{0}", new object[] { Convert.ToString(message.Rows[0]["MSGNAME"]) });  //착공완료 \r\n{0}
                            if (string.Equals(msg.Rows[0]["MSGCODE"], "95000")) // 해당BIZ에서 95000번을 샘플링으로 리턴
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2960", new object[] { Convert.ToString(message.Rows[0]["MSGNAME"]) })
                                    , true, true, ObjectDic.Instance.GetObjectName("바코드발행"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (sresult, isBarCode) =>
                                {
                                    if (isBarCode == true)
                                    {
                                        if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                                        {
                                            Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                                            return;
                                        }

                                        try
                                        {
                                            int iSamplingCount = 0; // GetSamplingLabelQty();

                                            #region [샘플링 출하거래처 추가]
                                            string[] sCompany = null;
                                            foreach (KeyValuePair<int, string> items in getSamplingLabelInfo())
                                            {
                                                iSamplingCount = Util.NVC_Int(items.Key);
                                                sCompany = Util.NVC(items.Value).Split(',');
                                            }
                                            #endregion

                                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                                for (int i = 0; i < iSamplingCount; i++)
                                                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["OUT_LOTID"]), _PROCID, i > sCompany.Length - 1 ? "" : sCompany[i]);

                                        }
                                        catch (Exception ex) { Util.MessageException(ex); }
                                    }
                                });
                            }
                            else
                            {
                                Util.AlertInfo("SFU2960", new object[] { Convert.ToString(message.Rows[0]["MSGNAME"]) }); //착공완료\r\n{0}
                            }
                        }

#region RTLS (2동 Only)
                                                /*
                                                if (LoginInfo.CFG_AREA_ID == "E6")
                                                {
                                                    DataTable IndataTable = new DataTable("INDATA");
                                                    IndataTable.Columns.Add("CONDITION", typeof(string));
                                                    IndataTable.Columns.Add("CART_NO", typeof(string));
                                                    IndataTable.Columns.Add("LOTID", typeof(string));
                                                    IndataTable.Columns.Add("USERID", typeof(string));
                                                    IndataTable.Columns.Add("ZONE_ID", typeof(string));

                                                    DataRow Indata = IndataTable.NewRow();
                                                    Indata["CONDITION"] = "INPUT_LOT";
                                                    Indata["CART_NO"] = "";
                                                    Indata["LOTID"] = _LOTID;
                                                    Indata["USERID"] = "MES";
                                                    Indata["ZONE_ID"] = "";
                                                    IndataTable.Rows.Add(Indata);

                                                    //DataTable dtRTLS = new ClientProxy().ExecuteServiceSync("BR_RTLS_REG_MAPPING_BY_CONDITION", "INDATA", null, IndataTable);

                                                    new ClientProxy().ExecuteService("BR_RTLS_REG_MAPPING_BY_CONDITION", "INDATA", null, IndataTable, (dtRTLS, searchException) =>
                                                    {
                                                        if (searchException != null)
                                                        {
                                                            throw searchException; 
                                                        }
                                                    });

                                                }
                                                */
#endregion

                        //_LOTID = string.Empty;

                        //정상처리되었습니다.
                        //Util.MessageInfo("SFU1275", (result) =>
                        //{
                            this.DialogResult = MessageBoxResult.OK;
                        //    //this.Close();
                        //});

                    }
                    catch (Exception ex)
                    {
                        //Util.MessageException(ex);
                        //작업시작 예외발생
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU1838"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        Util.MessageException(ex);
                    }

                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _LOTID = string.Empty;
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                if (dg != null)
                {
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                }

                dgLotInfo.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID").ToString();
                _MTRLID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();
                _CSTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CSTID") == null ? "": DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CSTID").ToString();
                txtLotID.Text = _LOTID;
            }
        }
#endregion

#region Mehod
        private bool GetLotInfo()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);
                
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    txtEquipment.Text = dtMain.Rows[0]["EQPTNAME"].ToString();
                    txtWorkorder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();
                    txtLotID.Text = _LOTID;

                    GetLotList();
                    rslt = true;
                }
                else
                {
                    Util.MessageValidation("SFU1436");  //W/O 선택 후 작업시작하세요
                    rslt = false;
                }
                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void GetLotList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                //2020-07-15 오화백 변경
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PROCID"] = _PROCID;
                Indata["EQSGID"] = _EQSGID;
                Indata["WO_DETL_ID"] = txtWorkorder.Text;
                //2020-07-15 오화백 변경
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);


                //2020-07-15 오화백 변경
                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WAIT_WIP_RP", "INDATA", "RSLTDT", IndataTable);
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WAIT_WIP_RP", "INDATA", "RSLTDT", IndataTable);

                dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            //try
            //{
            //    DataTable IndataTable = new DataTable();
            //    IndataTable.Columns.Add("EQPTID", typeof(string));
            //    IndataTable.Columns.Add("EQSGID", typeof(string));
            //    IndataTable.Columns.Add("PROCID", typeof(string));

            //    DataRow Indata = IndataTable.NewRow();
            //    Indata["PROCID"] = _PROCID;
            //    Indata["EQSGID"] = _EQSGID;
            //    Indata["EQPTID"] = _EQPTID;
            //    IndataTable.Rows.Add(Indata);

            //    DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WAIT_LOT", "INDATA", "RSLTDT", IndataTable);
            //    if (dtMain.Rows.Count <= 0 || dtMain == null)
            //    {
            //        dgLotInfo.ItemsSource = null;
            //        return;
            //    }

            //    DataTable dt = new DataTable();
            //    dt.Columns.Add("CHK", typeof(bool));
            //    dt.Columns.Add("LOTID", typeof(string));
            //    dt.Columns.Add("WIPQTY", typeof(string));
            //    dt.Columns.Add("PRODID", typeof(string));
            //    dt.Columns.Add("PRODNAME", typeof(string));

            //    DataRow row = null;
            //    for (int i = 0; i < dtMain.Rows.Count; i++)
            //    {
            //        row = dt.NewRow();
            //        row["CHK"] = dtMain.Rows[i]["CHK"];
            //        row["LOTID"] = dtMain.Rows[i]["PARENTLOT"];
            //        row["WIPQTY"] = dtMain.Rows[i]["WIPQTY"];
            //        row["PRODID"] = dtMain.Rows[i]["PRODID"];
            //        row["PRODNAME"] = dtMain.Rows[i]["PRODNAME"];
            //        dt.Rows.Add(row);


            //    }

            //    dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }
        private void SelectRowByLotID()
        {
            try
            {
                // 그리드에 일치하는 lot 자동선택
                DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                int rIdx = dt.Rows.IndexOf(dt.Select("LOTID = '" + txtLotID.Text + "'").FirstOrDefault());
                int cIdx = dgLotInfo.Columns["CHK"].Index;
                //   dgWaitList.GetCell(0,0).Presenter
                if (rIdx >= 0)
                {
                    dt.Rows[rIdx][cIdx] = true;
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);

                    dgLotInfo.SelectedIndex = rIdx;
                    dgLotInfo.UpdateLayout();
                    dgLotInfo.ScrollIntoView(dgLotInfo.SelectedIndex, 0);

                    _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "LOTID").ToString();
                    // _MTRLID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "PRODID").ToString();
                }
                //txtLOTID.Text = string.Empty;
            }
            catch (Exception ex)
            {
            }
        }

        private string GetTandemEqupment()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLPRESS_TANDEM", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count > 0)
                    return Util.NVC(dt.Rows[0]["TD_ROLLPRESS_ENABLE_FLAG"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return string.Empty;
        }

#endregion

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectRowByLotID();
            }
        }

        private void dgLotInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            dgLotInfo.SelectedIndex = idx;

            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        }


        // 샘플링 라벨발행 수량 / 출하처
        private Dictionary<int, string> getSamplingLabelInfo()
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            Indata["PROCID"] = _PROCID;
            Indata["LOTID"] = _LOTID;
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_CHK_LOT_T1", "INDATA", "OUT_DATA", IndataTable);

            if (dtMain != null && dtMain.Rows.Count > 0)
                return new Dictionary<int, string> { { Util.NVC_Int(dtMain.Rows[0]["OUT_PRINTCNT"]), Util.NVC(dtMain.Rows[0]["OUT_COMPANY"]) } };

            return new Dictionary<int, string> { { 1, string.Empty } };
        }


        private void SetIdentInfo()
        {
            try
            {
                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = _PROCID;
                row["EQSGID"] = _EQSGID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    _LDR_LOT_IDENT_BAS_CODE = result.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _UNLDR_LOT_IDENT_BAS_CODE = result.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
    }
}
