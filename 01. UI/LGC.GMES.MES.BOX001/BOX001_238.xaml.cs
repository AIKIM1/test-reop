/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.09.05  이병윤 E20230704-000395 OUTBOX 생성: INBOX입력시 기존 정규식 제거
                                     CELL 추가/취출 중 남경은 모델/LOT/Graderindata 추가
  2023.10.12  이병윤 E20230704-000395 OUTBOX입력 후INBOX가 입력되지 않을 경우 OUTBOX값을 리셋하고 INBOX입력 알림창을 보여준다.
  2024.03.29  이병윤 E20230704-000395 CELL 취출탭 대상 INBOX, OUTBOX매핑여부 인터락 수정 
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
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.UserControls;
using System.Text.RegularExpressions;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_238 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private bool bPilotChk = false;
        private string _BoxType = string.Empty;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        /*컨트롤 변수 선언*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker { get; set; }
        public TextBox txtShift { get; set; }
        public bool IsReadOnly { get; }

        public BOX001_238()
        {
            InitializeComponent();
        }
        CommonCombo _combo = new CommonCombo();

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

        List<string> BCDType = new List<string>() {"1D바코드", "2D바코드" };

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitSet();
            SetEvent();
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

            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
            // Area 셋팅
            // _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");

            chkChangeYn.IsChecked = true;

            txtSourceCellID.Focus();

            /* 공용 작업조 컨트롤 초기화 */
            ucBoxShift = grdShift.Children[0] as UCBoxShift;
            txtWorker = ucBoxShift.TextWorker;
            txtShift = ucBoxShift.TextShift;
            ucBoxShift.ProcessCode = Process.CELL_BOXING; //작업조 팝업에 넘길 공정
            ucBoxShift.FrameOperation = this.FrameOperation;

            // 오창의 경우 Outbox 생성 탭 숨김, 신규 Inbox 생성 체크박스 숨김 처리
            if (LoginInfo.CFG_SHOP_ID == "A010")
            {
                TabOutbox.Visibility = Visibility.Collapsed;

                chkNewBox.Visibility = Visibility.Collapsed;
            }
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
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
                    if(!string.IsNullOrWhiteSpace(txtBoxId.Text)) dr["BOXID"] = txtBoxId.Text.Trim();
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

                    if(chkChangeYn.IsChecked == true && to_sublotid == string.Empty)
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

                    //GetPalletInfo(sPalletID, sArea);

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
                        //GetPalletInfo(SearchResult.Rows[0]["OUTER_BOXID"].ToString(), LoginInfo.CFG_AREA_ID);
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
                            if(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FROM_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString()
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

        #region Cell 추가 엑셀 업로드
        /// <summary>
        /// 엑셀 업로드 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Add_Cell_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "CELLID";
                    sheet[1, 0].Value = "CELLID_001";
                    sheet[2, 0].Value = "CELLID_002";

                    sheet[0, 1].Value = "POSITION";
                    sheet[1, 1].Value = "A01";
                    sheet[2, 1].Value = "A02";

                    sheet[0, 0].Style = sheet[0, 1].Style = styel;
                    sheet.Columns[0].Width = sheet.Columns[1].Width = 1500;

                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 엑셀 업로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)chkNewBox.IsChecked)
                {
                    if (string.IsNullOrWhiteSpace(txtNewBoxID.Text.Trim()) && txtNewBoxID.IsReadOnly == true)
                    {
                        //BoxId 먼저 입력하세요
                        Util.MessageValidation("SFU3387");
                        return;
                    }
                }
                else
                {
                    if (dgPallet2.GetRowCount() == 0)
                    {
                        //BoxId 먼저 입력하세요
                        Util.MessageValidation("SFU3387");
                        return;
                    }
                }
                

                if (dgSourceCell2.ItemsSource == null)
                {
                    InitCellList(dgSourceCell2);
                }
                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell2.ItemsSource);

                dtInfo.Clear();


                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        if (sheet.GetCell(0, 0).Text != "CELLID"
                               || sheet.GetCell(0, 1).Text != "POSITION")
                        {
                            //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                            Util.MessageValidation("SFU4424");
                            return;
                        }

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // CELLID, POSITION 미입력시 return;
                            if (sheet.GetCell(rowInx, 0) == null
                                || sheet.GetCell(rowInx, 1) == null)
                                return;

                            DataRow dr = dtInfo.NewRow();
                            dr["SUBLOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            dr["FORM_TRAY_PSTN_NO"] = sheet.GetCell(rowInx, 1).Text.Trim();
                            dtInfo.Rows.Add(dr);
                        }

                        if (dtInfo.Rows.Count > 0)
                            dtInfo = dtInfo.DefaultView.ToTable(true);

                        Util.GridSetData(dgSourceCell2, dtInfo, FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void txtinbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtoutbox.Text.Trim()))
                {
                    txtoutbox.Focus();
                    txtoutbox.SelectAll();
                }
                else if (string.IsNullOrWhiteSpace(txtinbox.Text.Trim()))
                {
                    txtinbox.Focus();
                    txtinbox.SelectAll();
                }
                else if (!string.IsNullOrWhiteSpace(txtoutbox.Text.Trim()) && !string.IsNullOrWhiteSpace(txtinbox.Text.Trim()))
                {
                    CreateOutBox();
                }
            }
        }

        private void txtoutbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                inboxCheck();
            }
        }

        

        private void btncreoutbox_Click(object sender, RoutedEventArgs e)
        {
            inboxCheck();
        }

        private void inboxCheck()
        {
            if (string.IsNullOrWhiteSpace(txtinbox.Text.Trim()))
            {
                if (string.IsNullOrWhiteSpace(txtoutbox.Text.Trim()))
                {
                    txtinbox.Focus();
                    txtinbox.SelectAll();
                    return;
                }
                else
                {
                    txtoutbox.Clear();
                    // Inbox ID를 입력 하세요.
                    Util.MessageInfo("SFU4517", result =>
                    {
                        txtinbox.Focus();
                        txtinbox.SelectAll();
                    });
                    return;
                }

            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtoutbox.Text.Trim()))
                {
                    txtoutbox.Focus();
                    txtoutbox.SelectAll();
                    return;
                }
            }
            CreateOutBox();
        }

        private void btnoutboxrefresh_Click(object sender, RoutedEventArgs e)
        {
            txtoutbox.Clear();
            txtinbox.Clear();

            txtinbox.Focus();
            txtinbox.SelectAll();
        }
        #endregion
        #endregion

        #region Mehod

        //private void GetPalletInfo(string sPalletID, string sArea)
        //{
        //    DataTable RQSTDT = new DataTable();
        //    RQSTDT.TableName = "RQSTDT";
        //    RQSTDT.Columns.Add("AREAID", typeof(String));
        //    RQSTDT.Columns.Add("BOXID", typeof(String));

        //    DataRow dr = RQSTDT.NewRow();
        //    dr["AREAID"] = sArea;
        //    dr["BOXID"] = sPalletID;

        //    RQSTDT.Rows.Add(dr);

        //    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_INFO_FOR_UNPACK", "INDATA", "OUTDATA", RQSTDT);

        //    if (SearchResult.Rows.Count == 0)
        //    {
        //        //조회된 Data가 없습니다.
        //        Util.MessageInfo("SFU1905");
        //     //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    ////dgPallet.ItemsSource = DataTableConverter.Convert(SearchResult);
        //    Util.GridSetData(dgPallet, SearchResult, FrameOperation);
        //    txtPalletID.IsReadOnly = true;
        //    txtPalletID.Text = SearchResult.Rows[0]["BOXID"].ToString();
        //    chkChangeYn.IsEnabled = false;
        //}

        private bool GetPalletInfo(C1.WPF.DataGrid.C1DataGrid grid, string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("SHOPID", typeof(String));
                RQSTDT.Columns.Add("BOXID" , typeof(String));
                RQSTDT.Columns.Add("PGM_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["BOXID"]  = sPalletID;
                dr["PGM_ID"] = "BOX001_238";

                RQSTDT.Rows.Add(dr);

                DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOX_INFO_FOR_UNPACK_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (dtPallet == null || dtPallet.Rows.Count == 0)
                {
                    //BOX 정보가 없습니다.
                    Util.MessageValidation("SFU1180");
                    ClearDataGrid(grid);
                    return false;
                }

                // check whether the input INBOX is mapping to OUTBOX.If mapping, should pop_up alarm, and forbid appending/removal
                if (!Util.IsNVC(dtPallet.Rows[0]["OUTER_BOXID2"]) || !Util.IsNVC(dtPallet.Rows[0]["OUTER_BOXID"]))
                {
                    // SFU9901 INBOX is bound to OUTBOX
                    Util.MessageValidation("SFU9901");
                    ClearDataGrid(grid);
                    return false;
                }

                Util.GridSetData(grid, dtPallet, FrameOperation);
           
                return true;
            }
            catch (Exception ex)
            {
                ClearDataGrid(grid);
                Util.MessageException(ex);
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

            dtInit.Columns.Add("SUBLOTID", typeof(string));            
            dtInit.Columns.Add("FORM_TRAY_PSTN_NO", typeof(string));

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

            chkNewBox.IsChecked = false;

            txtNewBoxID.Text = string.Empty;
            txtNewBoxID.IsReadOnly = false;

            txtgridqty.Value = 1;         
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

        private void CreateOutBox()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtoutbox.Text))
                { 
                    //OUTBOX를 입력하세요
                    Util.MessageInfo("SFU5008");
                    txtoutbox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtinbox.Text))
                {
                    // Inbox ID를 입력 하세요.
                    Util.MessageInfo("SFU4517");
                    txtoutbox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843"); //작업자를 선택해 주세요
                    return;
                }

                DataSet ds = new DataSet();

                DataTable dtIndata = ds.Tables.Add("INDATA");

                dtIndata.Columns.Add("SRCTYPE");
                dtIndata.Columns.Add("USERID");
                dtIndata.Columns.Add("SHFTID");
                dtIndata.Columns.Add("AREAID");

                DataRow dr = null;

                dr = dtIndata.NewRow();

                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["USERID"] = txtWorker.Tag as string;
                dr["SHFTID"] = txtShift.Tag as string;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID ;

                dtIndata.Rows.Add(dr);

                DataTable dtInbox = ds.Tables.Add("IN_INBOX");

                dtInbox.Columns.Add("INBOXID");

                dr = dtInbox.NewRow();

                dr["INBOXID"] = Regex.Replace(txtinbox.Text.Trim() , @"[^0-9]", "");

                dtInbox.Rows.Add(dr);

                DataTable dtOutbox = ds.Tables.Add("IN_OUTBOX");

                dtOutbox.Columns.Add("OUTBOXID");

                dr = dtOutbox.NewRow();

                dr["OUTBOXID"] = txtoutbox.Text.Trim();

                dtOutbox.Rows.Add(dr);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTBOX_TESLA_UI_NJ", "INDATA,IN_INBOX,IN_OUTBOX", null, ds);

                Util.MessageInfo("SFU1275", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtTargetCellID1.Focus();
                        txtTargetCellID1.SelectAll();
                    }
                });
                txtinbox.Text = string.Empty;
                txtoutbox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AddCell()
        {
            try
            {
                string sBizName = string.Empty;

                // 남경
                if (LoginInfo.CFG_SHOP_ID == "G182")
                {
                    sBizName = "BR_PRD_REG_ADD_SUBLOT_NJ";
                }
                else // 오창
                {
                    sBizName = "BR_PRD_REG_ADD_SUBLOT_FM";
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                string sPalletID = string.Empty;

                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("BOXID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));

                inSublotTable.Columns.Add("SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("FORM_TRAY_PSTN_NO", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["USERID"] = txtWorker.Tag as string;
                dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "BOXID"));
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataTable.Rows.Add(dr);

                string sublot = string.Empty;
                string position = string.Empty;

                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    #region Cell 입력 체크 및 위치 입력 체크
                    sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID")).Trim();
                    position = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "FORM_TRAY_PSTN_NO")).Trim();

                    if (string.IsNullOrWhiteSpace(sublot))
                    {
                        Util.MessageInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(position))
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }
                    if (position.Length != 3)
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }

                    Regex regex = new Regex(@"^[A-Z]");
                    Regex regex2 = new Regex(@"^[0-9]");
                    Boolean ismatch = regex.IsMatch(position.ToString().Substring(0, 1));
                    Boolean ismatch2 = regex2.IsMatch(position.ToString().Substring(1, 2));

                    if (!ismatch || !ismatch2)
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }

                    string matchpstn = Regex.Replace(position, @"[^A-Z0-9]", "");

                    if (matchpstn != position)
                    {
                        // 숫자와 영문대문자만 입력가능합니다.
                        Util.MessageValidation("SFU3674");
                        return;
                    }

                    #endregion

                    DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                    drsub["SUBLOTID"] = sublot;
                    drsub["FORM_TRAY_PSTN_NO"] = position;

                    indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                }

                new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INSUBLOT", string.Empty, indataSet);

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
                AllClear2(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreateInbox()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQPT");
                DataTable inTrayTable = indataSet.Tables.Add("IN_TRAY");
                DataTable inSublotTable = indataSet.Tables.Add("IN_SUBLOT");

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));

                inTrayTable.Columns.Add("TESLA_TRAY_ID", typeof(string));
                inTrayTable.Columns.Add("TRAY_TYPE", typeof(string));

                inSublotTable.Columns.Add("SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("FORM_TRAY_PSTN_NO", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["USERID"] = txtWorker.Tag as string;
                dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                inDataTable.Rows.Add(dr);

                DataRow dr2 = inTrayTable.NewRow();
                dr2["TESLA_TRAY_ID"] = txtNewBoxID.Text.Trim();


                string NewBox = txtNewBoxID.Text.Trim();

                NewBox = Regex.Replace(NewBox, @"[^0-9]", "");

                //BoxID 8자리 + 방향구분자 1자리 Or BoxID 8자리
                if (NewBox.Length == 8)
                {
                    _BoxType = "N"; //1회용 Inbox
                }
                //BoxID 6자리 + 방향구분자 1자리 Or BoxID 6자리
                else if (NewBox.Length == 7)
                {
                    _BoxType = "R"; // 재활용 Inbox
                }
                else
                {
                    _BoxType = null;
                }


                dr2["TRAY_TYPE"] = _BoxType;
                inTrayTable.Rows.Add(dr2);

                string sublot = string.Empty;
                string position = string.Empty;

                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID")).Trim();
                    position = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "FORM_TRAY_PSTN_NO")).Trim();

                    if (string.IsNullOrWhiteSpace(sublot))
                    {
                        Util.MessageInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(position))
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }
                    if (position.Length != 3)
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }

                    Regex regex = new Regex(@"^[A-Z]");
                    Regex regex2 = new Regex(@"^[0-9]");
                    Boolean ismatch = regex.IsMatch(position.ToString().Substring(0, 1));
                    Boolean ismatch2 = regex2.IsMatch(position.ToString().Substring(1, 2));

                    if (!ismatch || !ismatch2)
                    {
                        Util.MessageInfo("SFU1234"); // 위치가 입력되지 않았습니다.
                        return;
                    }

                    string matchpstn = Regex.Replace(position, @"[^A-Z0-9]", "");

                    if (matchpstn != position)
                    {
                        // 숫자와 영문대문자만 입력가능합니다.
                        Util.MessageValidation("SFU3674");
                        return;
                    }

                    DataRow drsub = inSublotTable.NewRow();
                    drsub["SUBLOTID"] = sublot;
                    drsub["FORM_TRAY_PSTN_NO"] = position;

                    inSublotTable.Rows.Add(drsub);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_INBOX_TESLA_UI_NJ", "IN_EQPT,IN_TRAY,IN_SUBLOT", "OUTDATA", indataSet);

                // "정상적으로 처리하였습니다." >>정상처리되었습니다.
                Util.MessageInfo("SFU1275", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtPalletID.IsReadOnly = false;
                        txtPalletID2.Focus();
                        txtPalletID2.SelectAll();

                        chkNewBox.IsChecked = false;
                        txtNewBox.Visibility = Visibility.Collapsed;
                        txtNewBoxID.Visibility = Visibility.Collapsed;
                        cboLine.Visibility = Visibility.Collapsed;
                        cboLinetxt.Visibility = Visibility.Collapsed;
                    }
                });

                //txtTagPallet2.Text = sPalletID;
                AllClear2(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //모델 Combo Set.
            C1ComboBox[] cboParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: cboParent, sCase: "cboModelLot");
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

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
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
                        dr["2D_BCR_SKIP_FLAG"] = b2DBCD? "N" : "Y";
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
                if (!(bool)chkNewBox.IsChecked)
                {
                    if (dgPallet2.GetRowCount() == 0)
                    {
                        Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                        return;
                    }
                }
                else
                {
                    if (txtNewBoxID.IsReadOnly == false)
                    {
                        //입력오류 : Boxid TextBox에서 엔터를 치세요
                        Util.MessageValidation("SFU3455");
                        return;
                    }
                }

                if (dgSourceCell2.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
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
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1170"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (msgresult) =>
               Util.MessageConfirm("SFU1170", (msgresult) =>
               {
                   if (msgresult == MessageBoxResult.OK)
                   {
                       if ((bool)chkNewBox.IsChecked)
                       {
                           CreateInbox();
                       }
                       else
                       {
                           AddCell();
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
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
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

                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));

                            inSublotTable.Columns.Add("SUBLOTID", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["USERID"] = txtWorker.Tag as string;
                            dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID"));

                            inDataTable.Rows.Add(dr);

                            string sublot = string.Empty;


                            for (int i = 0; i < dgSourceCell3.GetRowCount(); i++)
                            {
                                sublot = Util.NVC(DataTableConverter.GetValue(dgSourceCell3.Rows[i].DataItem, "SUBLOTID")).Trim();

                                if (string.IsNullOrWhiteSpace(sublot))
                                {
                                    Util.MessageInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                                    return;
                                }

                                DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                                drsub["SUBLOTID"] = sublot;

                                indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REMOVE_SUBLOT_NJ", "INDATA,INSUBLOT", string.Empty, indataSet);

                            // "정상적으로 처리하였습니다." >>정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID3.Focus();
                                    txtPalletID3.SelectAll();
                                }
                            });

                            txtTagPallet3.Text = sPalletID;
                            AllClear3(true);

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
                    if (tabCtrl.SelectedItem == tabUnMapping)
                    {
                        tabCollapsed01.Visibility = Visibility.Collapsed;
                        tabCollapsed02.Visibility = Visibility.Collapsed;
                        tabChange.Visibility = Visibility.Collapsed;
                    }

                    string sPalletID = string.Empty;
                    sPalletID = txtPalletID3.Text.Trim();

                    if (Util.NVC(sPalletID).Equals(""))
                    {
                        //조회할 BOX ID 를 입력하세요.
                        Util.MessageValidation("SFU1189", (sresult) =>
                        {
                            if (sresult == MessageBoxResult.OK)
                            {
                                txtPalletID3.Text = string.Empty;
                                txtPalletID3.Focus();
                            }
                        });
                        return;
                    }

                    ClearDataGrid(dgSourceCell3);

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "INDATA";
                    RQSTDT.Columns.Add("SHOPID", typeof(String));
                    RQSTDT.Columns.Add("BOXID" , typeof(String));
                    RQSTDT.Columns.Add("PGM_ID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dr["BOXID"]  = sPalletID;
                    dr["PGM_ID"] = "BOX001_238";

                    RQSTDT.Rows.Add(dr);

                    new ClientProxy().ExecuteService("BR_PRD_GET_BOX_INFO_FOR_UNPACK_NJ", "INDATA", "OUTDATA", RQSTDT, (result, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                ClearDataGrid(dgPallet3);
                                txtSourceCellID3.SelectAll();
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.GridSetData(dgPallet3, result, FrameOperation);
                            txtSourceCellID3.Focus();
                            txtSourceCellID3.SelectAll();
                            txtSourceCellID3.Text = string.Empty;
                        }
                        catch (Exception ex)
                        {
                            txtSourceCellID3.SelectAll();
                            ClearDataGrid(dgPallet3);
                            Util.MessageException(ex);
                        }

                    });

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }


        private void SetResetInbox()
        {
            ClearDataGrid(dgPallet3);
            txtPalletID3.Text = string.Empty;
            txtSourceCellID3.Text = string.Empty;
            txtPalletID3.Focus();
        }

        private bool SetSourceCellID3(string sSourceCellId)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0)
                {
                    //BoxId 먼저 입력하세요
                    Util.MessageValidation("SFU3387");
                    return false;
                }

                //if (!Util.IsNVC(DataTableConverter.GetValue(dgPallet3.Rows[0].DataItem, "OUTER_BOXID2"))
                //    || !Util.IsNVC(DataTableConverter.GetValue(dgPallet3.Rows[0].DataItem, "OUTER_BOXID")))
                //{
                //    // SFU9901 INBOX is bound to OUTBOX
                //    Util.MessageValidation("SFU9901", (sresult) =>
                //    {
                //        if (sresult == MessageBoxResult.OK)
                //        {
                //            SetResetInbox();
                //        }
                //    });
                //    return false;
                //}


                if (string.IsNullOrEmpty(sSourceCellId))
                {
                    //교체 Cell ID 가 없습니다.
                    Util.MessageValidation("SFU1462");
                    return false;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return false;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell3.ItemsSource);

                if (dtInfo.Rows.Count > 0)
                {
                    DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sSourceCellId + "'");

                    if (drList.Length > 0)
                    {
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSourceCellID3.Focus();
                                txtSourceCellID3.Text = string.Empty;
                            }
                        });

                        txtSourceCellID3.Text = string.Empty;
                        return false;
                    }
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("BOXID");

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sSourceCellId;
                dr["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID"));

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_FOR_REMOVE_BOX_NJ", "INDATA", "OUTDATA", RQSTDT);

                dtInfo.Merge(dtRslt);

                Util.GridSetData(dgSourceCell3, dtInfo, FrameOperation, true);

                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                        //조회할 BOX ID 를 입력하세요.
                        Util.MessageValidation("SFU1189");
                        return;
                    }

                    if (GetPalletInfo(dgPallet2, sPalletID))
                    {
                        txtSourceCellID2.Focus();
                        txtSourceCellID2.SelectAll();
                    }
                    else
                    {
                        txtPalletID2.SelectAll();
                    }
                    
                    ClearDataGrid(dgSourceCell2);

                   
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
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
                    //BoxId 먼저 입력하세요
                    Util.MessageValidation("SFU3387");
                    return false;
                }               

                if (sSouceCellId == null)
                {
                    //교체 Cell ID 가 없습니다.
                    Util.MessageValidation("SFU1462");
                    return false;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return false;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSourceCell2.ItemsSource);

                if (dtInfo.Rows.Count > 0)
                {
                    DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sSouceCellId + "'");

                    if (drList.Length > 0)
                    {
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtSourceCellID2.Focus();
                                txtSourceCellID2.Text = string.Empty;
                            }
                        });

                        txtSourceCellID2.Text = string.Empty;
                        return false;
                    }
                }

                string sBizName = string.Empty;

                // 남경 
                if (LoginInfo.CFG_SHOP_ID == "G182")
                {
                    sBizName = "BR_PRD_CHK_SUBLOT_FOR_ADD_BOX_NJ";
                }
                else // 오창
                {
                    sBizName = "BR_PRD_CHK_SUBLOT_FOR_ADD_BOX_FM";
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("USERID");
                RQSTDT.Columns.Add("SHOPID");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("EQSGID");
                // 남경 : 
                if (LoginInfo.CFG_SHOP_ID == "G182")
                {
                    RQSTDT.Columns.Add("PRODID");
                    RQSTDT.Columns.Add("ASSY_LOTID");
                    RQSTDT.Columns.Add("PRDT_GRD_CODE");
                }
                

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sSouceCellId;
                dr["USERID"] = txtWorker.Tag as string;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = null;
                if (LoginInfo.CFG_SHOP_ID == "G182")
                {
                    dr["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgPallet2.Rows[0].DataItem, "PRODID")).Trim();
                    dr["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPallet2.Rows[0].DataItem, "PKG_LOTID")).Trim().Substring(0,8);
                    dr["PRDT_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgPallet2.Rows[0].DataItem, "PRDT_GRD_CODE")).Trim();
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", RQSTDT);

                dtInfo.Merge(dtRslt);

                Util.GridSetData(dgSourceCell2, dtInfo, FrameOperation, true);


                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                        DataTableConverter.SetValue(dgSourceCell3.Rows[cnt].DataItem, "SEQ", cnt+1);
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

        private void txtSourceCellID2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.ImeProcessed)
            {
                e.Handled = true;
            }

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
            if (e.Key == Key.Space || e.Key == Key.ImeProcessed)
            {
                e.Handled = true;
            }
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

                //Pallet Tag 정보Set
                DataTable dtPalletHisCard = setPalletTag(grid, LoginInfo.USERID, palletID, iSelRow);
                                            //SetPalletTag(LoginInfo.USERID, palletID, iSelRow, dtBox, dtAssyLot);
                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[4];
                Parameters[0] = "PalletHis_Tag"; // "PalletHis_Tag";
                Parameters[1] = dtPalletHisCard;
                Parameters[2] = "2";
                Parameters[3] = "Y";

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
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
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
        private DataTable setPalletTag(C1.WPF.DataGrid.C1DataGrid grid, string sUserName, string sPalletID, int iSelRow)
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
            }

            for (int i = 0; i < dtBox.Rows.Count && i<40; i++)
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
  
        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void chkNewBox_Checked(object sender, RoutedEventArgs e)
        {
            txtPalletID2.IsReadOnly = true;
            txtNewBox.Visibility = Visibility.Visible;
            txtNewBoxID.Visibility = Visibility.Visible;

            cboLine.Visibility = Visibility.Visible;
            cboLinetxt.Visibility = Visibility.Visible;
        }

        private void chkNewBox_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPalletID2.IsReadOnly = false;
            txtNewBoxID.IsReadOnly = false;
            txtNewBoxID.Text = string.Empty;

            txtNewBox.Visibility = Visibility.Collapsed;
            txtNewBoxID.Visibility = Visibility.Collapsed;

            cboLine.Visibility = Visibility.Collapsed;
            cboLinetxt.Visibility = Visibility.Collapsed;

        }

        private void txtNewBoxID_KeyDown(object sender, KeyEventArgs e)
       {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string NewBox = txtNewBoxID.Text.Trim();

                    if (string.IsNullOrWhiteSpace(NewBox))
                    {
                        // Inbox ID를 입력 하세요.
                        Util.MessageInfo("SFU4517");
                        txtNewBoxID.Focus();
                        txtNewBoxID.SelectAll();
                        return;
                    }

                    NewBox = Regex.Replace(NewBox, @"[^0-9]", "");

                    //BoxID 8자리 + 방향구분자 1자리 Or BoxID 8자리
                    if (NewBox.Length == 8)
                    {
                        _BoxType = "N"; //1회용 Inbox
                    }
                    //BoxID 6자리 + 방향구분자 1자리 Or BoxID 6자리
                    else if (NewBox.Length == 7)
                    {
                        _BoxType = "R"; // 재활용 Inbox
                    }
                    else
                    {
                        _BoxType = null;
                    }

                    if (_BoxType == null)
                    {
                        // Inbox ID를 입력 하세요.
                        Util.MessageInfo("SFU4517");
                        txtNewBoxID.Focus();
                        txtNewBoxID.SelectAll();
                        return;
                    }

                    DataSet ds = new DataSet();

                    DataTable dtEqp = ds.Tables.Add("IN_EQP");
                    dtEqp.Columns.Add("SRCTYPE", typeof(String));
                    dtEqp.Columns.Add("IFMODE", typeof(String));
                    dtEqp.Columns.Add("EQPTID", typeof(String));
                    dtEqp.Columns.Add("USERID", typeof(String));

                    DataTable dtBox = ds.Tables.Add("IN_BOX");
                    dtBox.Columns.Add("TESLA_TRAY_ID", typeof(String));
                    dtBox.Columns.Add("TRAY_TYPE", typeof(String));

                    DataRow dr = dtEqp.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = string.Empty;
                    dr["USERID"] = string.Empty;

                    dtEqp.Rows.Add(dr);

                    DataRow dr1 = dtBox.NewRow();
                    dr1["TESLA_TRAY_ID"] = Util.NVC(NewBox);
                    dr1["TRAY_TYPE"] = Util.NVC(_BoxType);

                    dtBox.Rows.Add(dr1);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_DUP_BOX_NJ", "IN_EQP,IN_BOX", "OUTDATA", ds);

                    txtNewBoxID.Text = (string)dsResult.Tables["OUTDATA"].Rows[0]["BOXID"];

                    txtNewBoxID.IsReadOnly = true;


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkNewBox.IsChecked)
            {
                if (dgPallet2.GetRowCount() == 0)
                {
                    // Inbox ID를 입력 하세요.
                    Util.MessageInfo("SFU4517");
                    txtPalletID2.Focus();
                    return;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtNewBoxID.Text.Trim()))
                {
                    // Inbox ID를 입력 하세요.
                    Util.MessageInfo("SFU4517");
                    txtNewBoxID.Focus();
                    return;
                }
            }
            

            DataTable dtList = new DataTable();

            //dtList.Columns.Add("SEQ");
            dtList.Columns.Add("SUBLOTID");
            dtList.Columns.Add("FORM_TRAY_PSTN_NO");

            //TextBox에 입력 된 수량만큼 Row 추가
            for (int i = 0; i < txtgridqty.Value; i++)
            {
                DataRow newRow = dtList.NewRow();
               //newRow["SEQ"] = i + 1;
                newRow["SUBLOTID"] = string.Empty;
                newRow["FORM_TRAY_PSTN_NO"] = string.Empty;
                dtList.Rows.Add(newRow);
            }

            dgSourceCell2.ItemsSource = DataTableConverter.Convert(dtList);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ClearDataGrid(dgSourceCell2);
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void Textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"[^a-zA-Z0-9\s]");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void Textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.ImeProcessed)
            {
                e.Handled = true;
            }
        }
    } 
}
