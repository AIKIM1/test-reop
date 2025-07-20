/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : Cell 교체 처리(취출)
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.07.21  이제섭     : UNCODE 입력 기능 추가에 따라, Pallet Tag 디자인 분리되어 공통코드 조회하여 공통코드에 해당하는 동일 시 Tag 디자인 파일명 분리
  2022.02.19  오화백     : 폴란드 사이트일 경우 대상 Cell이 자동포장일 경우  교체 Cell도 자동포장만 교체 및 투입이 가능하도록 Validation 기능 추가
  2022.04.05  안유수     C20220331-000480 Cell 교체 처리(취출) 화면에 작업자 누락 시 에러 메세지 팝업 창 추가 요청 건



 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using C1.WPF.DataGrid;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_008 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        //bool _bChangeFlag = false;
        //bool _bMappingFlag = false;
        //bool _bUnMappingFlag = false;
        private bool bPilotChk = false;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        private bool bTrayAdminChk = false;
        private int trayCellQty = 0; // mmd tray 수량
        private int trayChkQty = 0;
        private string palletEqsgid = string.Empty;
        private string palletProdid = string.Empty;
        private string palletSrctype = string.Empty;
        private Util _Util = new Util();

        private bool bTop_ProdID_Use_Flag = false;

        public BOX001_008()
        {
            InitializeComponent();
        }
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        List<string> BCDType = new List<string>() { "1D바코드", "2D바코드" };

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Loaded -= UserControl_Loaded;
            InitSet();
            if (GetPilotAdminArea() > 0) // 공통코드로 시생산 제품 관리하는 동에 따라 버튼 Visible
            {
                btnExtra.Visibility = Visibility.Visible;
                this.RegisterName("redBrush", redBrush);
                GetPilotProdMode();

            }
            else
            {
                btnExtra.Visibility = Visibility.Collapsed;
            }

            if (GetTrayAdminArea() > 0)
            {
                bTrayAdminChk = true;
                TargettrayID.Visibility = Visibility.Visible;
                cboTray.Visibility = Visibility.Visible;

            }
            else
            {
                bTrayAdminChk = false;
                TargettrayID.Visibility = Visibility.Collapsed;
                cboTray.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnChange1);
            listAuth.Add(btnChange2);
            listAuth.Add(btnChange3);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Initialize

        private void InitSet()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            dtpDateFrom.Text = DateTime.Now.ToString();
            dtpDateTo.Text = DateTime.Now.ToString();
            // Area 셋팅
            // _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            chkChangeYn.IsChecked = true;

            txtSourceCellID.Focus();



        }

        #endregion

        #region Event

        /// <summary>
        /// 교체 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 조회 전 이전 데이터 초기화
            dgCellChangeHis.ItemsSource = null;

            string from = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";  //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyy-MM-dd") + " 00:00:00";
            string to = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59"; //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyy-MM-dd") + " 23:59:59";

            try
            {
                //BizData data = new BizData("QR_CHANGE_CELL", "RSLTDT");

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("TO_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("CELLID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MDLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if ((!string.IsNullOrWhiteSpace(txtCellid.Text)) || (!string.IsNullOrWhiteSpace(txtBoxId.Text)))
                {
                    if (!string.IsNullOrWhiteSpace(txtCellid.Text)) dr["CELLID"] = txtCellid.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(txtBoxId.Text)) dr["BOXID"] = getPalletBCD(txtBoxId.Text.Trim());  // 팔레트바코드ID -> BOXID
                }
                else
                {
                    dr["FROM_DTTM"] = from;
                    dr["TO_DTTM"] = to;
                    dr["AREAID"] = sAREAID;
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                    dr["MDLOT_ID"] = Util.NVC(cboModelLot.SelectedValue) == "" ? null : Util.NVC(cboModelLot.SelectedValue);
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHANGE_CELL_CP", "RQSTDT", "RSLTDT", RQSTDT);
                ////dgCellChangeHis.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgCellChangeHis, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// CELL 교체처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPallet.GetRowCount() == 0 || dgCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("3"); //"입력데이터가 없습니다."  
                    txtSourceCellID.Focus();
                    return;
                }

                if (txtReason.Text == string.Empty)
                {
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    txtReason.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("REPACK_FLAG", typeof(string));
                inDataTable.Columns.Add("BOXID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                inSublotTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("TO_SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("BCR2D", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["REPACK_FLAG"] = chkChangeYn.IsChecked == true ? "Y" : "N";
                dr["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "BOXID"));
                dr["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID;    // sUser;
                dr["NOTE"] = txtReason.Text.Trim();
                inDataTable.Rows.Add(dr);

                string from_sublotid = string.Empty;
                string to_sublotid = string.Empty;

                for (int i = 2; i < dgCellList.Rows.Count; i++)
                {
                    from_sublotid = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FROM_SUBLOTID"));

                    if (from_sublotid == string.Empty)
                    {
                        Util.MessageValidation("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                        return;
                    }

                    to_sublotid = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "TO_SUBLOTID"));

                    if (chkChangeYn.IsChecked == true && to_sublotid == string.Empty)
                    {
                        Util.MessageValidation("SFU1462"); //"교체 대상 Cell ID가 입력되지 않았습니다."
                        return;
                    }

                    DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                    drsub["FROM_SUBLOTID"] = from_sublotid;
                    drsub["TO_SUBLOTID"] = to_sublotid;
                    drsub["BCR2D"] = string.Empty;

                    indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REPACKING_CELL", "INDATA,INSUBLOT", string.Empty, indataSet);

                // 메시지 출력
                Util.MessageInfo("SFU1275"); //"정상적으로 처리하였습니다." >>정상처리되었습니다.
                AllClear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// EXCEL 교체
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExlChange_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.BOX001";
            string MAINFORMNAME = "BOX001_008_CHANGE_EXL";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sArea = LoginInfo.CFG_AREA_ID;
                    string sPalletID = string.Empty;
                    sPalletID = txtPalletID.Text.ToString().Trim();

                    if (sPalletID == null)
                    {
                        //Pallet ID 가 없습니다. >> PALLETID를 입력해주세요
                        Util.MessageValidation("SFU1411");
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1411"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    GetPalletInfo(sPalletID, sArea);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }
        void popup_Closed(object sender, EventArgs e)
        {

        }




        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            AllClear();
        }

        private void btnBatchDel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSourceCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sSourceCellID = string.Empty;
                    sSourceCellID = txtSourceCellID.Text.ToString().Trim();

                    if (sSourceCellID == null)
                    {
                        //대상 Cell ID 가 없습니다. >> 대상 Cell ID가 입력되지 않았습니다.
                        Util.MessageValidation("SFU1495");
                        //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1495"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataTable SearchResult = GetCellInfo(sSourceCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        Util.MessageInfo("SFU1905");
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dgPallet.GetRowCount() == 0)
                    {
                        GetPalletInfo(SearchResult.Rows[0]["OUTER_BOXID"].ToString(), LoginInfo.CFG_AREA_ID);
                    }
                    else
                    {
                        if (chkChangeYn.IsChecked == true && DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "BOXID").ToString() != SearchResult.Rows[0]["OUTER_BOXID"].ToString())
                        {
                            //동일 팔레트 적재된 Cell 이 아닙니다.
                            Util.MessageValidation("SFU3148");
                            //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3148"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else if (DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                        {
                            //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                            Util.MessageValidation("SFU1502");
                            //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1502"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    int seqno = dgCellList.GetRowCount();

                    if (seqno == 0)
                    {
                        InitCellList();
                    }
                    else
                    {
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FROM_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString()
                                || Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "TO_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString())
                            {
                                //중복된 Cell을 입력하였습니다.
                                Util.MessageValidation("SFU3170");
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3170"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    dgCellList.IsReadOnly = false;
                    dgCellList.BeginNewRow();
                    dgCellList.EndNewRow(true);
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "SEQ_NO", (seqno + 1).ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                    //DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                    dgCellList.IsReadOnly = true;

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                    if (chkChangeYn.IsChecked == true)
                    {
                        txtTargetCellID.Text = string.Empty;
                        txtTargetCellID.Focus();
                        txtTargetCellID.SelectAll();
                    }
                    else
                    {
                        txtSourceCellID.Focus();
                        txtSourceCellID.SelectAll();
                    }
                }
            }
        }

        private void txtTargetCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (dgPallet.GetRowCount() == 0 || dgCellList.GetRowCount() == 0)
                    {
                        //대상 Cell ID 가 없습니다. >> 대상 Cell ID가 입력되지 않았습니다.
                        Util.MessageValidation("SFU1495");
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1495"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string sCellID = string.Empty;
                    sCellID = txtTargetCellID.Text.ToString().Trim();

                    if (sCellID == null)
                    {
                        //교체 Cell ID 가 없습니다.
                        Util.MessageValidation("SFU1462");
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1462"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataTable SearchResult = GetCellInfo(sCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        Util.MessageValidation("SFU1905");
                        //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "BOXID").ToString() == SearchResult.Rows[0]["OUTER_BOXID"].ToString())
                    {
                        //동일 팔레트 존재하는 Cell은 교체 대상에 등록할 수 없습니다.
                        Util.MessageValidation("SFU3149");
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3149"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                    {
                        //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                        Util.MessageValidation("SFU1502");
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1502"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int seqno = dgCellList.GetRowCount();

                    if (seqno == 0)
                    {
                        InitCellList();
                    }
                    else
                    {
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FROM_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString()
                                || Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "TO_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString())
                            {
                                //중복된 Cell을 입력하였습니다.
                                Util.MessageValidation("SFU3170");
                                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3170"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    dgCellList.IsReadOnly = false;
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                    dgCellList.IsReadOnly = true;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                    txtSourceCellID.Text = string.Empty;
                    txtSourceCellID.Focus();
                    txtSourceCellID.SelectAll();
                }
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                }
            });
        }

        private void chkChangeYn_Click(object sender, RoutedEventArgs e)
        {
            if (chkChangeYn.IsChecked == true)
            {
                txtTargetCellID.IsEnabled = true;
            }
            else
            {
                txtTargetCellID.IsEnabled = false;
            }
        }

        #endregion

        #region Mehod

        private void GetPalletInfo(string sPalletID, string sArea)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(String));
            RQSTDT.Columns.Add("BOXID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = sArea;
            dr["BOXID"] = sPalletID;

            RQSTDT.Rows.Add(dr);

            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_INFO_FOR_UNPACK", "INDATA", "OUTDATA", RQSTDT);

            if (SearchResult.Rows.Count == 0)
            {
                //조회된 Data가 없습니다.
                Util.MessageInfo("SFU1905");
                //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            ////dgPallet.ItemsSource = DataTableConverter.Convert(SearchResult);
            Util.GridSetData(dgPallet, SearchResult, FrameOperation);
            txtPalletID.IsReadOnly = true;
            txtPalletID.Text = SearchResult.Rows[0]["BOXID"].ToString();
            chkChangeYn.IsEnabled = false;
        }

        private bool GetPalletInfo(C1.WPF.DataGrid.C1DataGrid grid, string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = null;
                dr["BOXID"] = sPalletID;

                RQSTDT.Rows.Add(dr);

                DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_INFO_FOR_UNPACK", "INDATA", "OUTDATA", RQSTDT);

                if (dtPallet == null || dtPallet.Rows.Count == 0)
                {
                    //팔레트 정보가 없습니다.
                    Util.MessageInfo("SFU1994");
                    return false;
                }
                Util.GridSetData(grid, dtPallet, FrameOperation);
                palletEqsgid = Util.NVC(dtPallet.Rows[0]["EQSGID"]);
                palletProdid = Util.NVC(dtPallet.Rows[0]["PRODID"]);
                palletSrctype = Util.NVC(dtPallet.Rows[0]["PACK_WRK_TYPE_CODE"]);

                if (bTrayAdminChk == true)
                {
                    GetTrayID(sPalletID);
                    AddTrayCell();
                    GetTrayQty(cboTray.SelectedValue.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                cboTray.ItemsSource = null;
                return false;
            }
        }

        private DataTable GetCellInfo(string sCellID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(String));
            RQSTDT.Columns.Add("SUBLOTID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SUBLOTID"] = sCellID;

            RQSTDT.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_INFO_FOR_CP", "INDATA", "OUTDATA", RQSTDT);
        }

        private bool GetCellHoldInfo(string sCellID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("SUBLOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellID;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_HOLD", "INDATA", null, RQSTDT);
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ClearDataGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.ItemsSource = null;
            //dg.BottomRows[0].Visibility = Visibility.Collapsed;
        }

        private void InitCellList()
        {
            DataTable dtInit = new DataTable();

            dtInit.Columns.Add("SEQ_NO", typeof(int));
            dtInit.Columns.Add("FROM_SUBLOTID", typeof(string));
            dtInit.Columns.Add("TO_SUBLOTID", typeof(string));
            dtInit.Columns.Add("2DBARCODE", typeof(string));
            dtInit.Columns.Add("FROM_OUTER_BOXID", typeof(string));
            dtInit.Columns.Add("FROM_INNER_BOXID", typeof(string));
            dtInit.Columns.Add("FROM_BOX_PSTN_NO", typeof(string));
            dtInit.Columns.Add("FROM_LOTID", typeof(string));
            dtInit.Columns.Add("FROM_RCV_ISS_ID", typeof(string));
            dtInit.Columns.Add("TO_OUTER_BOXID", typeof(string));
            dtInit.Columns.Add("TO_INNER_BOXID", typeof(string));
            dtInit.Columns.Add("TO_BOX_PSTN_NO", typeof(string));
            dtInit.Columns.Add("TO_LOTID", typeof(string));
            dtInit.Columns.Add("TO_RCV_ISS_ID", typeof(string));

            dgCellList.ItemsSource = DataTableConverter.Convert(dtInit);
            ////Util.GridSetData(dgCellList, dtInit, FrameOperation);
        }

        private void InitCellList(C1.WPF.DataGrid.C1DataGrid dg)
        {
            DataTable dtInit = new DataTable();

            dtInit.Columns.Add("SEQ", typeof(int));
            dtInit.Columns.Add("SUBLOTID", typeof(string));
            dtInit.Columns.Add("2DBARCODE", typeof(string));
            dtInit.Columns.Add("OUTER_BOXID", typeof(string));
            dtInit.Columns.Add("INNER_BOXID", typeof(string));
            dtInit.Columns.Add("BOXSEQ", typeof(string));
            dtInit.Columns.Add("BOX_PSTN_NO", typeof(string));
            dtInit.Columns.Add("LOTID", typeof(string));
            dtInit.Columns.Add("RCV_ISS_ID", typeof(string));
            dtInit.Columns.Add("PRODID", typeof(string));
            //2022-02-19 오화백  PACK_WRK_TYPE_CODE 추가
            dtInit.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));
            dtInit.Columns.Add("PLLT_BCD_ID", typeof(string));  // 팔레트바코드ID
            dg.ItemsSource = DataTableConverter.Convert(dtInit);
            ////Util.GridSetData(dgCellList, dtInit, FrameOperation);
        }

        private void AllClear()
        {
            ClearDataGrid(dgPallet);
            ClearDataGrid(dgCellList);

            txtPalletID.Text = string.Empty;
            txtReason.Text = string.Empty;
            txtSourceCellID.Text = string.Empty;
            txtTargetCellID.Text = string.Empty;

            txtPalletID.IsReadOnly = false;
            chkChangeYn.IsEnabled = true;
        }

        private void AllClear1(bool bAll = false)
        {
            if (bAll)
            {
                txtTagPallet1.Text = string.Empty;
                txtReason1.Text = string.Empty;
            }

            ClearDataGrid(dgTargetCell1);
            ClearDataGrid(dgSourceCell1);

            lbl2DBCD_Target.Text = string.Empty;
            lbl2DBCD_Source.Text = string.Empty;

            txtSourceCellID1.Text = string.Empty;
            txtTargetCellID1.Text = string.Empty;
        }

        private void AllClear2(bool bAll = false)
        {
            if (bAll)
            {
                txtTagPallet2.Text = string.Empty;
                txtReason2.Text = string.Empty;
            }
            ClearDataGrid(dgPallet2);
            ClearDataGrid(dgSourceCell2);

            txtPalletID2.Text = string.Empty;
            txtSourceCellID2.Text = string.Empty;
        }

        private void AllClear3(bool bAll = false)
        {
            if (bAll)
            {
                txtTagPallet3.Text = string.Empty;
                txtReason3.Text = string.Empty;
            }
            ClearDataGrid(dgPallet3);
            ClearDataGrid(dgSourceCell3);


            txtPalletID3.Text = string.Empty;
            txtSourceCellID3.Text = string.Empty;
        }

        /// <summary>
        /// UNCODE 필수입력 Plant 조회
        /// </summary>
        /// <returns></returns>
        private bool UseCommoncodePlant()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PLT_UNCODE_SHOP";
            dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }

            String[] sFilter = { sAREAID };    // Area
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP"); //, sCase: "EQSGID_PACK");

            //의뢰인 Combo Set.
            //String[] sFilter6 = { sSHOPID, sAREAID, Process.CELL_BOXING };
            // _combo.SetCombo(cboUser, CommonCombo.ComboStatus.SELECT, sFilter: sFilter6, sCase: "PROC_USER");

            // Barcode 속성 표시 여부
            isVisibleBCD(sAREAID);
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //모델 Combo Set.
            C1ComboBox[] cboParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: cboParent, sCase: "cboModelLot");
        }
        private string SelectCellID(string sBCR)
        {
            //QR_TC_2D_BCR_CELL

            try
            {
                //  B[)>06Y8460101010000XHX05024912V631039406TDX00000B59M101222P0017Q0.0002H
                //  B[)>06Y8460101010000XHX05024912V631039406TDX00000B59M101222P0017Q0.0002H
                //  B[)>06Y8460101010000XP2427762112V631039406T1116271I06J100692P0017Q3.7582H\u0004
                //  PB59M10122

                string sCellID = sBCR.Substring(49, 9);

                //BR_PRD_CHK_PACKING_CELL
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA_ROUTE");
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataTable inDataCellTable = indataSet.Tables.Add("INDATA");
                inDataCellTable.Columns.Add("BCR", typeof(string));
                inDataCellTable.Columns.Add("CELL_ID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(inData);

                DataRow inDataCell = inDataCellTable.NewRow();
                inDataCell["BCR"] = sBCR;
                inDataCell["CELL_ID"] = sCellID;
                inDataCellTable.Rows.Add(inDataCell);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_INF_SEL_2DBCR_BY_CELL", "INDATA_ROUTE,INDATA", "OUTDATA", indataSet);

                return dsResult.Tables["OUTDATA"].Rows[0]["CELL_ID"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageValidation("SFU3248", new object[] { sBCR });
                return string.Empty;
            }

        }

        private void txtTargetCellID1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Util.gridClear(dgTargetCell1);
                    TextBox txtCellid = sender as TextBox;


                    string sCellID = txtCellid.Text.ToString().Replace(Char.ConvertFromUtf32(29), string.Empty).Replace(Char.ConvertFromUtf32(30), string.Empty).Trim();
                    lbl2DBCD_Target.Text = string.Empty;
                    txtTagPallet1.Text = string.Empty;

                    if (sCellID == null)
                    {
                        //대상 Cell ID 가 없습니다. >> 대상 Cell ID가 입력되지 않았습니다.
                        Util.MessageValidation("SFU1495", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                        });
                        return;
                    }

                    bool b2DBCD = sCellID.Length > 60;

                    if (b2DBCD)
                    {
                        string sData = SelectCellID(sCellID);

                        if (string.IsNullOrWhiteSpace(sData))
                        {
                            return;
                        }

                        sCellID = sData;
                    }

                    DataTable SearchResult = GetCellInfo(sCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        Util.MessageValidation("SFU1905", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                        });
                        return;
                    }



                    if (string.IsNullOrWhiteSpace(Util.NVC(SearchResult.Rows[0]["OUTER_BOXID"])))
                    {
                        //팔레트 구성된 Cell만 교체 가능합니다.
                        Util.MessageValidation("SFU3483", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                        });
                        return;
                    }

                    Util.GridSetData(dgTargetCell1, SearchResult, FrameOperation);

                    txtCellid.Text = sCellID;
                    lbl2DBCD_Target.Text = !b2DBCD ? ObjectDic.Instance.GetObjectName(BCDType[0]) : ObjectDic.Instance.GetObjectName(BCDType[1]);

                    txtSourceCellID1.Focus();
                    txtSourceCellID1.SelectAll();

                    //if (dgTargetCell1.GetRowCount() > 0)
                    //{
                    //    ClearDataGrid(dgTargetCell1);                        
                    //}

                    //InitCellList(dgTargetCell1);

                    //dgTargetCell1.IsReadOnly = false;
                    //bool bResult = dgTargetCell1.BeginNewRow();
                    //dgTargetCell1.EndNewRow(true);                    
                    //DataTableConverter.SetValue(dgTargetCell1.CurrentRow.DataItem, "SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                    //DataTableConverter.SetValue(dgTargetCell1.CurrentRow.DataItem, "OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                    //DataTableConverter.SetValue(dgTargetCell1.CurrentRow.DataItem, "INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                    //DataTableConverter.SetValue(dgTargetCell1.CurrentRow.DataItem, "BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                    //DataTableConverter.SetValue(dgTargetCell1.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());                    
                    //DataTableConverter.SetValue(dgTargetCell1.CurrentRow.DataItem, "RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                    //DataTableConverter.SetValue(dgTargetCell1.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                    //dgTargetCell1.IsReadOnly = true;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                }
            }
        }

        private void txtSourceCellID1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (dgTargetCell1.GetRowCount() == 0)
                    {
                        //대상 Cell ID가 입력되지 않았습니다.
                        Util.MessageValidation("SFU1495");
                        return;
                    }

                    string sCellID = string.Empty;
                    sCellID = txtSourceCellID1.Text.Replace(Char.ConvertFromUtf32(29), string.Empty).Replace(Char.ConvertFromUtf32(30), string.Empty).Trim();

                    if (sCellID == null)
                    {
                        //교체 Cell ID 가 없습니다.
                        Util.MessageValidation("SFU1462");
                        return;
                    }

                    bool b2DBCD = sCellID.Length > 60;

                    lbl2DBCD_Source.Text = !b2DBCD ? ObjectDic.Instance.GetObjectName(BCDType[0]) : ObjectDic.Instance.GetObjectName(BCDType[1]);

                    if (lbl2DBCD_Target.Text != lbl2DBCD_Source.Text)
                    {
                        // 대상 Cell 바코드와 동일한 타입으로 스캔하세요.
                        Util.MessageValidation("SFU3480");
                        return;
                    }

                    if (b2DBCD)
                    {
                        string sData = SelectCellID(sCellID);

                        if (string.IsNullOrWhiteSpace(sData))
                        {
                            return;
                        }

                        sCellID = txtSourceCellID1.Text = sData;
                    }
                    if (GetCellHoldInfo(sCellID) == false) // Cell 교체 시, Cell Hold 상태 확인 후 Validation.
                    {
                        return;
                    }

                    DataTable SearchResult = GetCellInfo(sCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        Util.MessageValidation("SFU1905");
                        return;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "OUTER_BOXID")) == Util.NVC(SearchResult.Rows[0]["OUTER_BOXID"]))
                    {
                        //동일 팔레트 존재하는 Cell은 교체 대상에 등록할 수 없습니다.
                        Util.MessageValidation("SFU3149");
                        return;
                    }

                    if (DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                    {
                        //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                        Util.MessageValidation("SFU1502");
                        return;
                    }

                    //2022-02-19 Cell Chk Validition 추가  폴란드 사이트만 체크 
                    if (GetCellChkArea() > 0) //공통코드에 등록된 동만 체크함
                    {

                        // 해당 Cell이 자동인지 수동인지 체크
                        // 교체 Cell이 포장되어 있는지 체크                      

                        DataTable SearchSrcType = GetSrcType(sCellID);

                        if (SearchSrcType.Rows.Count > 0)
                        {
                            SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"] = SearchSrcType.Rows[0]["PACK_WRK_TYPE_CODE"];
                        }
                        else
                        {
                            //이력정보가 없다는 뜻은 한번도 포장한 이력이 없다는 뜻임 : 그래서 강제적으로 UI로 입력함
                            SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"] = "UI";
                        }

                        if (DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "PACK_WRK_TYPE_CODE").ToString() == "EQ" && SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"].ToString() != "EQ")
                        {
                            //대상이 Cell이 자동포장일 경우  교체Cell도 자동포장이어야 합니다.
                            Util.MessageValidation("SFU8476");
                            return;
                        }
                    }

                    //2022-02-19 오화백  교체Cell이 PalletID 혹은 트레이정보가 존재할 경우 교체 안되도록 Validation 체크
                    if (Util.NVC(SearchResult.Rows[0]["OUTER_BOXID"]) != string.Empty || Util.NVC(SearchResult.Rows[0]["INNER_BOXID"]) != string.Empty)
                    {
                        //이미 포장된 Cell이  있습니다
                        Util.MessageValidation("SFU8477");
                        return;
                    }
                    //2022-04-06 안유수 C20220331-000480 Cell 교체 처리(취출) 화면에 작업자 누락 시 에러 메세지 팝업 창 추가 요청 건
                    if (string.IsNullOrEmpty(txtWorker.Text))
                    {
                        //작업자를 선택해 주세요
                        Util.MessageValidation("SFU1843");
                        return;
                    }

                    DataSet indataSet = new DataSet();
                    DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                    RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                    RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));
                    RQSTDT.Columns.Add("PILOT_MODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    dr["SHOPID"] = string.Empty;
                    dr["AREAID"] = string.Empty;
                    dr["EQSGID"] = string.Empty;
                    dr["MDLLOT_ID"] = string.Empty;
                    dr["SUBLOT_CHK_SKIP_FLAG"] = "Y";
                    dr["INSP_SKIP_FLAG"] = chkInspectSkip.IsChecked == true ? "Y" : "N";
                    dr["2D_BCR_SKIP_FLAG"] = b2DBCD ? "N" : "Y";
                    dr["USERID"] = txtWorker.Tag.ToString();
                    dr["PILOT_MODE"] = bPilotChk ? "Y" : "N";
                    RQSTDT.Rows.Add(dr);


                    // ClientProxy2007
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", indataSet);


                    ////dgSourceCell1.ItemsSource = DataTableConverter.Convert(SearchResult);
                    Util.GridSetData(dgSourceCell1, SearchResult, FrameOperation);

                    //if (dgSourceCell1.GetRowCount() > 0)
                    //{
                    //    ClearDataGrid(dgSourceCell1);
                    //}

                    //InitCellList(dgSourceCell1);

                    //dgSourceCell1.IsReadOnly = false;
                    //DataTableConverter.SetValue(dgSourceCell1.CurrentRow.DataItem, "SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                    //DataTableConverter.SetValue(dgSourceCell1.CurrentRow.DataItem, "OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                    //DataTableConverter.SetValue(dgSourceCell1.CurrentRow.DataItem, "INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                    //DataTableConverter.SetValue(dgSourceCell1.CurrentRow.DataItem, "BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                    //DataTableConverter.SetValue(dgSourceCell1.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                    //DataTableConverter.SetValue(dgSourceCell1.CurrentRow.DataItem, "RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                    //DataTableConverter.SetValue(dgSourceCell1.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                    //dgSourceCell1.IsReadOnly = true;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                }
            }
        }

        private void btnChange1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTargetCell1.GetRowCount() == 0 || dgSourceCell1.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."              
                    return;
                }

                if (txtReason1.Text == string.Empty)
                {
                    txtReason1.Focus();
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843"); //작업자를 선택해 주세요
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
                Util.MessageConfirm("SFU1170", (msgresult) =>
                {

                    if (msgresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            string sPalletID = string.Empty;

                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("REPACK_FLAG", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("NOTE", typeof(string));
                            inDataTable.Columns.Add("LOTTERM_FLAG", typeof(string));

                            inSublotTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("TO_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("BCR2D", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["REPACK_FLAG"] = "Y";
                            dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "OUTER_BOXID"));
                            dr["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID;    // sUser;
                            dr["NOTE"] = txtReason1.Text.Trim();
                            dr["LOTTERM_FLAG"] = chkLotTerm.IsChecked == true ? "Y" : "N";
                            inDataTable.Rows.Add(dr);

                            string from_sublotid = string.Empty;
                            string to_sublotid = string.Empty;

                            from_sublotid = Util.NVC(DataTableConverter.GetValue(dgTargetCell1.Rows[0].DataItem, "SUBLOTID"));
                            to_sublotid = Util.NVC(DataTableConverter.GetValue(dgSourceCell1.Rows[0].DataItem, "SUBLOTID"));

                            if (from_sublotid == string.Empty)
                            {
                                Util.AlertInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                                return;
                            }

                            if (to_sublotid == string.Empty)
                            {
                                Util.AlertInfo("SFU1462"); //"교체 대상 Cell ID가 입력되지 않았습니다."
                                return;
                            }

                            DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                            drsub["FROM_SUBLOTID"] = from_sublotid;
                            drsub["TO_SUBLOTID"] = to_sublotid;
                            drsub["BCR2D"] = string.Empty;

                            indataSet.Tables["INSUBLOT"].Rows.Add(drsub);

                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REPACKING_CELL", "INDATA,INSUBLOT", string.Empty, indataSet);

                            // //"정상적으로 처리하였습니다." >>정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtTargetCellID1.Focus();
                                    txtTargetCellID1.SelectAll();
                                }
                            });
                            txtTagPallet1.Text = sPalletID;
                            AllClear1();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInit2_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear2(true);
                }

            });
        }

        private void btnChange2_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (dgPallet2.GetRowCount() == 0 || dgSourceCell2.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                    return;
                }

                //2022-02-19 Cell Chk Validition 추가  폴란드 사이트만 체크 
                if (GetCellChkArea() > 0) //공통코드에 등록된 동만 체크함
                {

                    //대상 Pallet는 자동포장
                    if (DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "PACK_WRK_TYPE_CODE").ToString() == "EQ")
                    {
                        int chkQty = 0;
                        for (int i = 0; i < dgSourceCell2.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "PACK_WRK_TYPE_CODE")) != "EQ")
                            {
                                chkQty = chkQty + 1;
                            }
                        }
                        if (chkQty > 0)
                        {
                            //대상이 Cell이 자동포장일 경우  교체Cell도 자동포장이어야 합니다.
                            Util.MessageValidation("SFU8476");
                            return;
                        }
                    }
 
                }
                //2022-02-19 Cell Chk Validition 추가
                // 투입 Cell에 Pallet ID정보가 존재하는지 체크
                int chkPalletQty = 0;
                for (int i = 0; i < dgSourceCell2.Rows.Count; i++)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "OUTER_BOXID")).Equals("") || !Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "INNER_BOXID")).Equals(""))
                    {
                        chkPalletQty = chkPalletQty + 1;
                    }
                }
                if (chkPalletQty > 0)
                {
                    //이미 포장된 Cell이  있습니다
                    Util.MessageValidation("SFU8477");
                    return;
                }

                if (txtReason2.Text == string.Empty)
                {
                    txtReason2.Focus();
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1170"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (msgresult) =>
                Util.MessageConfirm("SFU1170", (msgresult) =>
                {

                    if (msgresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                            string sPalletID = string.Empty;

                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("REPACK_FLAG", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("NOTE", typeof(string));
                            //inDataTable.Columns.Add("LOTTERM_FLAG", typeof(string));

                            inSublotTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("TO_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("BCR2D", typeof(string));
                            inSublotTable.Columns.Add("TRAYID", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["REPACK_FLAG"] = "N";
                            dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "BOXID"));
                            dr["USERID"] = txtWorker.Tag as string; //LoginInfo.USERID;    // sUser;
                            dr["NOTE"] = txtReason2.Text.Trim();
                            //dr["LOTTERM_FLAG"] = chkLotTerm2.IsChecked == true ? "Y" : "N";
                            inDataTable.Rows.Add(dr);

                            string from_sublotid = string.Empty;
                            string to_sublotid = string.Empty;

                            for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                            {
                                from_sublotid = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID"));
                                to_sublotid = null; //Util.NVC(DataTableConverter.GetValue(dgSourceCell1.Rows[0].DataItem, "SUBLOTID"));

                                if (from_sublotid == string.Empty)
                                {
                                    Util.AlertInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                                    return;
                                }

                                DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                                drsub["FROM_SUBLOTID"] = from_sublotid;
                                drsub["TO_SUBLOTID"] = to_sublotid;
                                drsub["BCR2D"] = string.Empty;
                                drsub["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "INNER_BOXID"));
                                indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REPACKING_CELL", "INDATA,INSUBLOT", string.Empty, indataSet);

                            // "정상적으로 처리하였습니다." >>정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID2.Focus();
                                    txtPalletID2.SelectAll();
                                }
                            });

                            txtTagPallet2.Text = sPalletID;
                            AllClear2();
                            if (bTrayAdminChk == true)
                            {
                                GetTrayID(null);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInit3_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear3(true);
                }

            });
        }

        private void btnChange3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0 || dgSourceCell3.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                    return;
                }

                if (txtReason3.Text == string.Empty)
                {
                    txtReason3.Focus();
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1170"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (msgresult) =>
                Util.MessageConfirm("SFU1170", (msgresult) =>
                {

                    if (msgresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                            string sPalletID = string.Empty;

                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("REPACK_FLAG", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("NOTE", typeof(string));
                            inDataTable.Columns.Add("LOTTERM_FLAG", typeof(string));

                            inSublotTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("TO_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("BCR2D", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["REPACK_FLAG"] = "N";
                            dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID"));
                            dr["USERID"] = txtWorker.Tag as string; //LoginInfo.USERID;    // sUser;
                            dr["NOTE"] = txtReason3.Text.Trim();
                            dr["LOTTERM_FLAG"] = "N";
                            inDataTable.Rows.Add(dr);

                            string from_sublotid = string.Empty;
                            string to_sublotid = string.Empty;

                            for (int i = 0; i < dgSourceCell3.GetRowCount(); i++)
                            {
                                from_sublotid = Util.NVC(DataTableConverter.GetValue(dgSourceCell3.Rows[i].DataItem, "SUBLOTID"));
                                to_sublotid = null; //Util.NVC(DataTableConverter.GetValue(dgSourceCell1.Rows[0].DataItem, "SUBLOTID"));

                                if (from_sublotid == string.Empty)
                                {
                                    Util.MessageValidation("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                                    return;
                                }

                                DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                                drsub["FROM_SUBLOTID"] = from_sublotid;
                                drsub["TO_SUBLOTID"] = to_sublotid;
                                drsub["BCR2D"] = string.Empty;

                                indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REPACKING_CELL", "INDATA,INSUBLOT", string.Empty, indataSet);

                            //"정상적으로 처리하였습니다." >>정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID3.Focus();
                                    txtPalletID3.SelectAll();
                                }
                            });

                            txtTagPallet3.Text = sPalletID;
                            AllClear3();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sPalletID = string.Empty;
                    txtTagPallet3.Text = string.Empty;

                    sPalletID = txtPalletID3.Text.ToString().Trim();

                    if (sPalletID == null)
                    {
                        //Pallet ID 가 없습니다. >> PALLETID를 입력해주세요
                        Util.MessageValidation("SFU1411");
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1411"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    GetPalletInfo(dgPallet3, getPalletBCD(sPalletID)); // 팔레트바코드ID -> BoxID

                    ClearDataGrid(dgSourceCell3);
                    txtSourceCellID3.Focus();
                    txtSourceCellID3.SelectAll();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private bool SetSourceCellID3(string sSourceCellId)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0)
                {
                    //대상 팔레트를 먼저 등록하세요.
                    Util.MessageValidation("SFU3147");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3147"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(sSourceCellId))
                {
                    //교체 Cell ID 가 없습니다.
                    Util.MessageValidation("SFU1462");
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1462"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                DataTable SearchResult = GetCellInfo(sSourceCellId);

                if (SearchResult == null || SearchResult.Rows.Count == 0)
                {
                    //조회된 Data가 없습니다.
                    Util.MessageValidation("SFU1905");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID").ToString() != SearchResult.Rows[0]["OUTER_BOXID"].ToString())
                {
                    //팔레트에 등록되지 않은 Cell 입니다.
                    Util.MessageValidation("SFU3176");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3176"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                {
                    //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                    Util.MessageValidation("SFU1502");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1502"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                //dgSourceCell1.ItemsSource = DataTableConverter.Convert(SearchResult);

                if (dgSourceCell3.GetRowCount() == 0)
                {
                    InitCellList(dgSourceCell3);
                }

                for (int i = 0; i < dgSourceCell3.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgSourceCell3.Rows[i].DataItem, "SUBLOTID").ToString() == SearchResult.Rows[0]["SUBLOTID"].ToString())
                    {
                        //이미 입력된 Cell 입니다.
                        Util.MessageValidation("SFU3164");
                        //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3164"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                dgSourceCell3.IsReadOnly = false;
                dgSourceCell3.BeginNewRow();
                dgSourceCell3.EndNewRow(true);
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "SEQ", dgSourceCell3.GetRowCount());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "BOXSEQ", SearchResult.Rows[0]["BOXSEQ"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "PLLT_BCD_ID", SearchResult.Rows[0]["PLLT_BCD_ID"].ToString());
                dgSourceCell3.IsReadOnly = true;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                txtSourceCellID3.Focus();
                txtSourceCellID3.SelectAll();
            }
        }

        private void txtSourceCellID3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetSourceCellID3(txtSourceCellID3.Text.Trim());
            }
        }

        private void txtPalletID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sPalletID = string.Empty;
                    txtTagPallet2.Text = string.Empty;
                    sPalletID = txtPalletID2.Text.Trim();
                    if (sPalletID == null)
                    {
                        //Pallet ID 가 없습니다. >> PALLETID를 입력해주세요
                        Util.MessageValidation("SFU1411");
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1411"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (GetPalletInfo(dgPallet2, getPalletBCD(sPalletID)))  // Pallet Barcode -> BoxID
                    {
                        txtSourceCellID2.Focus();
                        txtSourceCellID2.SelectAll();
                    }
                    else
                    {
                        txtPalletID2.SelectAll();
                    }

                    ////dgPallet2.ItemsSource = DataTableConverter.Convert(dtPallet);
                    //   Util.GridSetData(dgPallet2, dtPallet, FrameOperation);
                    ClearDataGrid(dgSourceCell2);


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private bool SetSourceCellID2(string sSouceCellId)
        {
            try
            {
                if (dgPallet2.GetRowCount() == 0)
                {
                    //대상 팔레트를 먼저 등록하세요.
                    Util.MessageValidation("SFU3147");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3147"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (sSouceCellId == null)
                {
                    //교체 Cell ID 가 없습니다.
                    Util.MessageValidation("SFU1462");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1462"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (GetCellHoldInfo(sSouceCellId) == false) // Cell 추가 시, HOLD 상태 Validation.
                {
                    return false;
                }

                DataTable SearchResult = GetCellInfo(sSouceCellId);


                if (SearchResult == null || SearchResult.Rows.Count == 0)
                {
                    //조회된 Data가 없습니다.
                    Util.MessageValidation("SFU1905");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "BOXID").ToString() == SearchResult.Rows[0]["OUTER_BOXID"].ToString())
                {
                    //동일 팔레트 존재하는 Cell은 추가 대상에 등록할 수 없습니다.
                    Util.MessageValidation("SFU3249");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3249"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                {
                    //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                    Util.MessageValidation("SFU1502");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1502"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID").ToString() == SearchResult.Rows[0]["SUBLOTID"].ToString())
                    {
                        //이미 입력된 Cell 입니다.
                        Util.MessageValidation("SFU3164");
                        //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3164"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return false;
                }

                if (SearchResult.Rows[0]["OUTER_BOXID"].ToString() == string.Empty)
                {

                    DataSet indataSet = new DataSet();
                    DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                    RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                    RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));
                    RQSTDT.Columns.Add("PILOT_MODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = sSouceCellId;
                    dr["SHOPID"] = string.Empty;
                    dr["AREAID"] = string.Empty;
                    dr["EQSGID"] = string.Empty;
                    dr["MDLLOT_ID"] = string.Empty;
                    dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                    dr["INSP_SKIP_FLAG"] = chkInspectSkip2.IsChecked == true ? "Y" : "N";
                    dr["2D_BCR_SKIP_FLAG"] = "Y";
                    dr["USERID"] = txtWorker.Tag.ToString();
                    dr["PILOT_MODE"] = bPilotChk ? "Y" : "N";

                    RQSTDT.Rows.Add(dr);


                    // ClientProxy2007
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", indataSet);
                }

                //2022-02-19 오화백  Cell 체크 Validation 가능한 동 정보 가져오기
                // 해당 Cell이 자동인지 수동인지 체크
                DataTable SearchSrcType = GetSrcType(sSouceCellId);
                if (SearchSrcType.Rows.Count > 0)
                {
                    SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"] = SearchSrcType.Rows[0]["PACK_WRK_TYPE_CODE"];
                }
                else
                {
                    //이력정보가 없다는 뜻은 한번도 포장한 이력이 없다는 뜻임 : 그래서 강제적으로 UI로 입력함
                    SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"] = "UI";
                }
                //SearchResult.Rows[0]["SRCTYPE"] = SearchSrcType.Rows[0]["SRCTYPE"];


                if (dgSourceCell2.GetRowCount() == 0)
                {
                    InitCellList(dgSourceCell2);
                }


                if (bTrayAdminChk == true && palletSrctype == "EQ")
                {
                    if (trayChkQty < trayCellQty)
                    {
                        trayChkQty++;
                    }
                    else
                    {
                        Util.MessageValidation("SFU8210", cboTray.SelectedValue.ToString());
                        return false;
                    }
                }

                dgSourceCell2.IsReadOnly = false;
                dgSourceCell2.BeginNewRow();
                dgSourceCell2.EndNewRow(true);
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "SEQ", dgSourceCell2.GetRowCount());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());

                if (bTrayAdminChk == true)
                {
                    DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "INNER_BOXID", cboTray.SelectedValue.ToString());
                }
                else
                {
                    DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                }

                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "BOXSEQ", SearchResult.Rows[0]["BOXSEQ"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());

                //2022-02-19 오화백  SRCTYPE 추가
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "PACK_WRK_TYPE_CODE", SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "PLLT_BCD_ID", SearchResult.Rows[0]["PLLT_BCD_ID"].ToString());
                dgSourceCell2.IsReadOnly = true;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                txtSourceCellID2.Focus();
                txtSourceCellID2.SelectAll();
            }
        }

        private void txtSourceCellID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetSourceCellID2(txtSourceCellID2.Text.Trim());
            }
        }

        private void btnInit1_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear1(true);
                }

            });
        }

        private void btnDelete3_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgSourceCell3.IsReadOnly = false;
                    dgSourceCell3.RemoveRow(index);

                    for (int cnt = 0; cnt < dgSourceCell3.GetRowCount(); cnt++)
                    {
                        DataTableConverter.SetValue(dgSourceCell3.Rows[cnt].DataItem, "SEQ", cnt + 1);
                    }
                    dgSourceCell3.IsReadOnly = true;
                }
            });
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgSourceCell2.IsReadOnly = false;
                    dgSourceCell2.RemoveRow(index);


                    for (int cnt = 0; cnt < dgSourceCell2.GetRowCount(); cnt++)
                    {
                        DataTableConverter.SetValue(dgSourceCell2.Rows[cnt].DataItem, "SEQ", cnt + 1);
                    }

                    dgSourceCell2.IsReadOnly = true;
                }
            });
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void txtSourceCellID2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && SetSourceCellID2(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtSourceCellID3_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && SetSourceCellID3(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void PrintTag(C1.WPF.DataGrid.C1DataGrid grid, string palletID)
        {
            try
            {
                int iSelRow = 0;

                #region 완제품 ID 존재 여부 체크
                // TOP_PRODID 조회.
                ChkUseTopProdID();

                string sTopProdID = "";
                
                if (bTop_ProdID_Use_Flag)
                {
                    sTopProdID = GetTopProdID(palletID);

                    if (sTopProdID.Equals(""))
                    {
                        // [%1]에 완제품 정보(TOP_PRODID)가 없습니다.
                        Util.MessageValidation("SFU5208", palletID);
                        return;
                    }
                }
                #endregion

                //Pallet Tag 정보Set
                DataTable dtPalletHisCard = setPalletTag(grid, LoginInfo.USERID, palletID, iSelRow, sTopProdID);
                //SetPalletTag(LoginInfo.USERID, palletID, iSelRow, dtBox, dtAssyLot);
                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[5];
                Parameters[0] = UseCommoncodePlant() ? "PalletHis_Tag_UNCODE" : "PalletHis_Tag"; ; // "PalletHis_Tag";
                Parameters[1] = dtPalletHisCard;
                Parameters[2] = "2";
                Parameters[3] = "Y";
                Parameters[4] = sSHOPID;

                LGC.GMES.MES.BOX001.Report_Pallet_Hist rs = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
                C1WindowExtension.SetParameters(rs, Parameters);
                rs.Closed += new EventHandler(rs_Closed);
                // 팝업 화면 숨겨지는 문제 수정.
                //  this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                grdMain.Children.Add(rs);
                rs.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void rs_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.BOX001.Report_Pallet_Hist wndPopup = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
            grdMain.Children.Remove(wndPopup);
            //// 초기화 함수 호출
            //if (tabCtrl.SelectedItem == tabChange)
            //{
            //    _bChangeFlag = false;
            //    AllClear1();
            //}
            //else if (tabCtrl.SelectedItem == tabMapping)
            //{
            //    _bMappingFlag = false;
            //    AllClear2();
            //}
            //else if (tabCtrl.SelectedItem == tabUnMapping)
            //{
            //    _bUnMappingFlag = false;
            //    AllClear3();
            //}
        }
        private void btnTagPrint1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTagPallet1.Text))
                {
                    //Cell 교체 작업 완료 후 재발행이 가능합니다.
                    Util.MessageValidation("SFU3355");
                    return;
                }
                //C1.WPF.DataGrid.C1DataGrid grid = dgTargetCell1;
                //string palletID = Util.NVC(grid.GetCell(0, grid.Columns["OUTER_BOXID"].Index).Value);
                PrintTag(dgTargetCell1, txtTagPallet1.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTagPrint2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTagPallet2.Text))
                {
                    //Cell 추가 작업 완료 후 재발행이 가능합니다.
                    Util.MessageValidation("SFU3356");
                    return;
                }
                //C1.WPF.DataGrid.C1DataGrid grid = dgPallet2;
                //string palletID = Util.NVC(grid.GetCell(0, grid.Columns["BOXID"].Index).Value);
                PrintTag(dgPallet2, txtTagPallet2.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnTagPrint3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTagPallet3.Text))
                {
                    // Cell 취출 작업 완료 후 재발행이 가능합니다.
                    Util.MessageValidation("SFU3357");
                    return;
                }

                //C1.WPF.DataGrid.C1DataGrid grid = dgPallet3;
                //string palletID = Util.NVC(grid.GetCell(0, grid.Columns["BOXID"].Index).Value);
                PrintTag(dgPallet3, txtTagPallet3.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조립LotID, 해당 Lot 별 수량 조회
        /// </summary>
        /// <param name="palletID"></param>
        /// <returns></returns>
        public DataTable SearchAssyLot(string palletID)
        {

            //BizData data = new BizData("QR_GETASSYLOT_PALLETID", "RSLTDT");
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = palletID;
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["SHOPID"] = sSHOPID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_LOT_BY_PALLET", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 result값에 null 대입하고 함수 중단함.
                if (dtResult == null || dtResult.Rows.Count <= 0)
                {
                    return null;
                }

                #region # Data Column 정의
                DataTable lsDataTable = new DataTable();
                lsDataTable.Columns.Add("LOTID", typeof(string));
                lsDataTable.Columns.Add("CELLQTY", typeof(string));
                #endregion

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    DataRow row = lsDataTable.NewRow();
                    row["LOTID"] = Util.NVC(dtResult.Rows[i]["LOTID"].ToString());
                    row["CELLQTY"] = Util.NVC(dtResult.Rows[i]["CELLQTY"].ToString());
                    lsDataTable.Rows.Add(row);
                }

                return lsDataTable;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return null;
            }

        }

        /// <summary>
        /// BOXID, 해당 BOX 별 수량 조회
        /// </summary>
        /// <param name="palletID"></param>
        /// <returns></returns>
        public DataTable SelectTagInformation(string palletID, string sPackWrkType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = palletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 result값에 null 대입하고 함수 중단함.
                if (dtResult.Rows.Count <= 0)
                {
                    return null;
                }

                #region # Data Column 정의
                DataTable lsDataTable = new DataTable();
                lsDataTable.Columns.Add("TRAY_MAGAZINE", typeof(string));
                lsDataTable.Columns.Add("QTY", typeof(string));
                #endregion

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    DataRow row = lsDataTable.NewRow();

                    if (sPackWrkType == "MGZ")
                    {
                        row["TRAY_MAGAZINE"] = Util.NVC(dtResult.Rows[i]["TAG_ID"].ToString());
                    }
                    else
                    {
                        row["TRAY_MAGAZINE"] = Util.NVC(dtResult.Rows[i]["TRAYID"].ToString());
                    }

                    row["QTY"] = Util.NVC(dtResult.Rows[i]["QTY"].ToString());
                    lsDataTable.Rows.Add(row);
                }

                return lsDataTable;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return null;
            }
        }

        private DataTable SelectScanPalletInfo(string palletID)
        {
            DataTable dtResult = new DataTable();

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = sAREAID;
                dr["PALLETID"] = palletID;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INFO_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtResult;
            }
        }
        private DataTable setPalletTag(C1.WPF.DataGrid.C1DataGrid grid, string sUserName, string sPalletID, int iSelRow, string sTopProdID)
        {
            string sProjectName = string.Empty;

            DataTable dtinfo = SelectScanPalletInfo(sPalletID);
            DataTable dtAssyLot = new DataTable();

            // Tray _ MagazineID / 수량 저장을 위한 DataTable
            DataTable dtBox = new DataTable();

            // Palelt ID
            string sPackWrkType = Util.NVC(dtinfo.Rows[iSelRow]["LOT_TYPE"]);

            // 수작업 여부 확인 : MAG 컬럼이 Y면 수작업임
            if (sPackWrkType == "UI")
            {
                // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                dtAssyLot = SearchAssyLot(sPalletID);
            }
            else
            {
                //  Tray / Magazine 정보 조회 함수 호출
                dtBox = SelectTagInformation(sPalletID, sPackWrkType);
                if (dtBox == null)
                {
                    return null;
                }
                else
                {
                    // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                    dtAssyLot = SearchAssyLot(sPalletID);
                }
            }

            //고객 모델 조회
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow drMomel = RQSTDT.NewRow();
                drMomel["PRODID"] = Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]);
                RQSTDT.Rows.Add(drMomel);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTATTR_FOR_PROJECTNAME", "RQSTDT", "RSLTDT", RQSTDT);
                sProjectName = Util.NVC(dtResult.Rows[0]["PROJECTNAME"]);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                //return null;
            }


            //Lot 편차 구하기... 2014.02.20 Add By Airman
            string sLotTerm = GetLotTerm2PalletID(sPalletID);
            
            DataTable dtPalletHisCard = new DataTable();

            dtPalletHisCard.Columns.Add("PALLETID", typeof(string));    //4,3   PALLETID_01
            dtPalletHisCard.Columns.Add("BARCODE1", typeof(string));    //4,9   PALLETID_02
            dtPalletHisCard.Columns.Add("CONBINESEQ1", typeof(string));  //4,17  PALLETD_03

            dtPalletHisCard.Columns.Add("SHIP_LOC", typeof(string));    //5,7   출하처
            dtPalletHisCard.Columns.Add("SHIPDATE", typeof(string));    //5,14  출하예정일
            dtPalletHisCard.Columns.Add("OUTGO", typeof(string));       //6,7   출하지
            dtPalletHisCard.Columns.Add("LOTTERM", typeof(string));     //6,16  LOT편차
            dtPalletHisCard.Columns.Add("NOTE", typeof(string));        //7,7   특이사항
            dtPalletHisCard.Columns.Add("UNCODE", typeof(string));      //UNCODE

            dtPalletHisCard.Columns.Add("PACKDATE", typeof(string));    //8,7   포장작업일자
            dtPalletHisCard.Columns.Add("LINE", typeof(string));        //8,15  생산호기
            dtPalletHisCard.Columns.Add("MODEL", typeof(string));       //9,7   모델
            dtPalletHisCard.Columns.Add("PRODID", typeof(string));      //9,15  제품id
            dtPalletHisCard.Columns.Add("SHIPQTY", typeof(string));     //10,7   출하수량
            dtPalletHisCard.Columns.Add("PARTNO", typeof(string));      //10,15  PART NO
            dtPalletHisCard.Columns.Add("OUTQTY", typeof(string));      //11,7   제품수량
            dtPalletHisCard.Columns.Add("USERID", typeof(string));      //11,15  작업자
            dtPalletHisCard.Columns.Add("CONBINESEQ2", typeof(string)); //12,7   구성차수관리No
            dtPalletHisCard.Columns.Add("SKIPYN", typeof(string));      //12,15  검사조건Skip여부
                                                                        //dtTRAY
            dtPalletHisCard.Columns.Add("SHIP_LOC_EN", typeof(string));
            dtPalletHisCard.Columns.Add("LINE_EN", typeof(string));
            for (int i = 0; i < 40; i++)
            {
                dtPalletHisCard.Columns.Add("TRAY_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("T_" + i.ToString(), typeof(string));
            }
            //lot
            for (int i = 0; i < 20; i++)
            {
                dtPalletHisCard.Columns.Add("LOTID_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("L_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("BCD" + i.ToString(), typeof(string));
            }

            string sShipToID = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_ID"]);
            string sShipToName = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NAME"]);

            DataRow dr = dtPalletHisCard.NewRow();
            dr["PALLETID"] = sPalletID;
            dr["BARCODE1"] = sPalletID;
            dr["LOTTERM"] = sLotTerm;

            if (sShipToID == string.Empty)
            {
                dr["SHIP_LOC"] = "";
                dr["SHIPDATE"] = "";
                dr["OUTGO"] = "";
                dr["NOTE"] = "";
                dr["PACKDATE"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["WIPDTTM_ED"].Index).Value);
                dr["LINE"] = "";
                dr["MODEL"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["MODELID"].Index).Value);
                if (bTop_ProdID_Use_Flag)
                    dr["PRODID"] = sTopProdID.Equals("") ? "" : sTopProdID;// Util.NVC(grid.GetCell(iSelRow, grid.Columns["PRODID"].Index).Value);
                else
                    dr["PRODID"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["PRODID"].Index).Value);
                dr["SHIPQTY"] = "";
                dr["PARTNO"] = "";
                dr["OUTQTY"] = string.Format("{0:#,###}", Util.NVC(grid.GetCell(iSelRow, grid.Columns["QTY"].Index).Value));
                dr["USERID"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["REG_USER"].Index).Value);
                dr["CONBINESEQ2"] = "";
                dr["CONBINESEQ1"] = "";
                dr["SKIPYN"] = "";
                dr["SHIP_LOC_EN"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NAME_EN"]);    //TB_MMD_SHIPTO.SHIPTO_NAME
                dr["LINE_EN"] = Util.NVC(dtinfo.Rows[iSelRow]["EQSGNAME_EN"]);        //EQUIPMENTSEGMENT.EQSGNAME
                dr["UNCODE"] = Util.NVC(dtinfo.Rows[iSelRow]["UN_CODE"]);
            }
            else
            {
                dr["SHIP_LOC"] = sShipToName;
                dr["SHIPDATE"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPDATE_SCHEDULE"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["SHIPDATE_SCHEDULE"].Index).Value);
                dr["OUTGO"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NOTE"]);  //  Util.NVC(grid.GetCell(iSelRow, grid.Columns["SHIPTO_NOTE"].Index).Value);
                dr["NOTE"] = Util.NVC(dtinfo.Rows[iSelRow]["PACK_NOTE"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["PACK_NOTE"].Index).Value);
                dr["PACKDATE"] = Util.NVC(dtinfo.Rows[iSelRow]["WIPDTTM_ED"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["WIPDTTM_ED"].Index).Value);
                dr["LINE"] = Util.NVC(dtinfo.Rows[iSelRow]["EQSGNAME"]);  //  Util.NVC(grid.GetCell(iSelRow, grid.Columns["EQSGNAME"].Index).Value);

                //if (sShipToName == "HLGP")
                //{
                //    if (sProjectName == "" || sProjectName == "N/A")
                //    {
                //        dr["MODEL"] = Util.NVC(dtinfo.Rows[iSelRow]["MODELID"]);  //  Util.NVC(grid.GetCell(iSelRow, grid.Columns["MODELID"].Index).Value);
                //    }
                //    else
                //    {
                //        dr["MODEL"] = sProjectName;
                //    }
                //}
                //else
                //{
                //    dr["MODEL"] = Util.NVC(dtinfo.Rows[iSelRow]["MODELID"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["MODELID"].Index).Value);
                //}
                dr["MODEL"] = Util.NVC(dtinfo.Rows[iSelRow]["MODELID"]) + " (" + sProjectName + ")";
                if (bTop_ProdID_Use_Flag)
                    dr["PRODID"] = sTopProdID.Equals("") ? "" : sTopProdID;// Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["PRODID"].Index).Value);
                else
                    dr["PRODID"] = Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["PRODID"].Index).Value);
                dr["SHIPQTY"] = string.Format("{0:#,###}", Util.NVC(dtinfo.Rows[iSelRow]["SHIPQTY"]));// Util.NVC_Int(grid.GetCell(iSelRow, grid.Columns["SHIPQTY"].Index).Value));
                dr["PARTNO"] = "";
                dr["OUTQTY"] = string.Format("{0:#,###}", Util.NVC(dtinfo.Rows[iSelRow]["QTY"]));// Util.NVC_Int(grid.GetCell(iSelRow, grid.Columns["QTY"].Index).Value));
                dr["USERID"] = Util.NVC(dtinfo.Rows[iSelRow]["REG_USERNAME"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["REG_USERNAME"].Index).Value);
                dr["CONBINESEQ2"] = Util.NVC(dtinfo.Rows[iSelRow]["COMBINESEQ"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["COMBINESEQ"].Index).Value);
                dr["CONBINESEQ1"] = Util.NVC(dtinfo.Rows[iSelRow]["COMBINESEQ"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["COMBINESEQ"].Index).Value);
                dr["SKIPYN"] = Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]) == "Y" ? "SKIP" : "NO SKIP";   // Util.NVC(grid.GetCell(iSelRow, grid.Columns["INSP_SKIP_FLAG"].Index).Value) == "Y" ? "SKIP" : "NO SKIP";

                dr["SHIP_LOC_EN"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NAME_EN"]);
                dr["LINE_EN"] = Util.NVC(dtinfo.Rows[iSelRow]["EQSGNAME_EN"]);
                dr["UNCODE"] = Util.NVC(dtinfo.Rows[iSelRow]["UN_CODE"]);
            }

            for (int i = 0; i < dtBox.Rows.Count && i < 40; i++)
            {
                dr["TRAY_" + i.ToString()] = Util.NVC(dtBox.Rows[i]["TRAY_MAGAZINE"]);
                dr["T_" + i.ToString()] = Util.NVC_Int(dtBox.Rows[i]["QTY"]);
            }

            dtPalletHisCard.Rows.Add(dr);

            for (int cnt = 0; cnt < (dtAssyLot.Rows.Count + 1) / 20; cnt++)
            {
                DataTable dtNew = dtPalletHisCard.Copy();
                dtPalletHisCard.Merge(dtNew);
            }

            for (int i = 0; i < dtAssyLot.Rows.Count; i++)
            {
                int cnt = i / 20;
                dtPalletHisCard.Rows[cnt]["LOTID_" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC(dtAssyLot.Rows[i]["LOTID"]);
                dtPalletHisCard.Rows[cnt]["L_" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC_Int(dtAssyLot.Rows[i]["CELLQTY"]).ToString();
                dtPalletHisCard.Rows[cnt]["BCD" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC(dtAssyLot.Rows[i]["LOTID"]) + " " + Util.NVC_Int(dtAssyLot.Rows[i]["CELLQTY"]).ToString();
            }

            //  dtPalletHisCard.Rows.Add(dr);
            return dtPalletHisCard;
        }

        /// <summary>
        /// LOT 편차 구하기
        /// </summary>
        /// <param name="sPalletID"></param>
        /// <returns></returns>
        private string GetLotTerm2PalletID(string sPalletID)
        {
            // DO_CONFIRM_CHECK
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["OUTER_BOXID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_OUTER_CP", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC_Int(dtResult.Rows[0]["LOTTERM"]).ToString();
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return "0";
            }
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility;
        }

        private void btnPilotProdMode_Click(object sender, RoutedEventArgs e)
        {
            string sMsg = string.Empty;

            sMsg = "SFU2875";

            Util.MessageConfirm(sMsg, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (bPilotChk == false) bPilotChk = true;
                    else bPilotChk = false;
                    GetPilotProdMode();
                    //      this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                }
            });
        }
        private bool GetPilotProdMode()
        {
            try
            {
                bool bRet = false;

                if (bPilotChk == true)
                {
                    ShowPilotProdMode();
                    bRet = true;
                }
                else
                {
                    HidePilotProdMode();
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void ShowPilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Visible;
                //txtPilotProdMode.Text = ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION");
                ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void HidePilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Collapsed;
                //ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }
        private void ColorAnimationInredRectangle(System.Windows.Shapes.Rectangle rect)
        {
            rect.Fill = redBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

            mouseLeftButtonDownStoryboard.Begin(this);
        }
        private int GetPilotAdminArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PILOT_PROD_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            return dtRslt.Rows.Count;
        }

        private int GetTrayAdminArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "BOX_ADD_ADMIN_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            return dtRslt.Rows.Count;
        }

        private void GetTrayID(string sPalletID)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BOX_PALLET_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //if (dtResult.Rows.Count == 0)
                //{
                //    cboTray.DisplayMemberPath = "CBO_NAME";
                //    cboTray.SelectedValuePath = "CBO_CODE";
                //    cboTray.ItemsSource = dtResult.Copy().AsDataView();
                //    cboTray.SelectedValue = "";
                //    return;
                //}

                cboTray.DisplayMemberPath = "CBO_NAME";
                cboTray.SelectedValuePath = "CBO_CODE";
                cboTray.ItemsSource = dtResult.Copy().AsDataView();
                cboTray.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddTrayCell()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "BOX_ADD_ADMIN_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    GetMMDTrayQty();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetMMDTrayQty()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = palletEqsgid;
                dr["PRODID"] = palletProdid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO_PACK_COND_AUTO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    trayCellQty = int.Parse(Util.NVC(dtResult.Rows[0]["TRAY_CELL_QTY"]));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTrayQty(string trayID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = trayID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    trayChkQty = Decimal.ToInt32(Util.NVC_Decimal(dtResult.Rows[0]["TOTAL_QTY"]));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void cboTray_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bTrayAdminChk == true)
            {
                GetTrayQty(cboTray.SelectedValue.ToString());
            }
        }

        //2022-02-19 오화백  Cell 체크 Validation 가능한 동 정보 가져오기
        private int GetCellChkArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "CELL_CHANGE_CHK_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            return dtRslt.Rows.Count;
        }
        //2022-02-19 오화백  Cell 체크 Validation 가능한 동 정보 가져오기
        // Cell 추가 후 대상팔레트가 자동 포장일 경우  추가 Cell이 자동이 아니면 붉은색으로 표시
        private void dgSourceCell2_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgSourceCell2.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (GetCellChkArea() > 0) //공통코드에 등록된 동만 체크함
                {
                    //대상 팔레트가 자동포장이고  투입Cell이 수동포장이면 붉은색으로 표시
                    if (DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "PACK_WRK_TYPE_CODE").ToString() == "EQ" && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PACK_WRK_TYPE_CODE")).Equals("EQ"))
                    {
                        if (e.Cell.Column.Name.Equals("SUBLOTID"))

                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
                
                // 투입 Cell이 Pallet와 매핑되어 있으면 빨간색
                if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUTER_BOXID")).Equals("")|| !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INNER_BOXID")).Equals(""))
                {
                    if (e.Cell.Column.Name.Equals("SUBLOTID"))

                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        //2022-02-19 오화백  Cell 체크 Validation 가능한 동 정보 가져오기
        // Cell 추가 후 대상팔레트가 자동 포장일 경우  추가 Cell이 자동이 아니면 붉은색으로 표시
        private void dgSourceCell2_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        //2022-02-19 오화백  Cell 체크 Validation 가능한 동 정보 가져오기
        // 해체된 Cell의 가장 최종의 포장된 이력이 자동인지 수동인지 체크
        private DataTable GetSrcType(string sCellID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SUBLOTID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["SUBLOTID"] = sCellID;

            RQSTDT.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SUBLOT_SRCTYPE", "INDATA", "OUTDATA", RQSTDT);
        }

        private string GetTopProdID(string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("STR_ID", typeof(string));
                RQSTDT.Columns.Add("GBN_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TYPE_CODE"] = "B";
                dr["STR_ID"] = sPalletID;
                dr["GBN_ID"] = "A";
                RQSTDT.Rows.Add(dr);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TOP_PRODID", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC(dtResult.Rows[0]["TOP_PRODID"]).ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        // 팔레트 바코드 항목 표시 여부
        private void isVisibleBCD(string sAreaID)
        {
            // 파레트 바코드 표시 설정
            if (_Util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgSourceCell1.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgPallet2.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgSourceCell2.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgPallet3.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgSourceCell3.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgCellChangeHis.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                dgSourceCell1.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                dgPallet2.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                dgSourceCell2.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                dgPallet3.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                dgSourceCell3.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                dgCellChangeHis.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
            }
        }

        // 팔레트바코드ID -> BoxID
        private string getPalletBCD(string palletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("CSTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = palletid;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult != null && SearchResult.Rows.Count > 0)
                {
                    return Util.NVC(SearchResult.Rows[0]["CURR_LOTID"]);
                }
                return palletid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void ChkUseTopProdID()
        {
            try
            {
                bTop_ProdID_Use_Flag = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_TOP_PRODID_USE", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0 && Util.NVC(dtResult.Rows[0]["TOP_PRODID_USE_FLAG"]).Equals("Y"))
                {
                    bTop_ProdID_Use_Flag = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
