/*************************************************************************************
 Created Date : 2025.05.20
      Creator : 김영택 (ytkim29)
   Decription : ERP 이전전기 조회 (NERP Ver.)
--------------------------------------------------------------------------------------
 [Change History]
  2025.05.20  김영택 : Initial Created.
  2025.07.01  김영택 : seq 검색 수정, 미처리 오류 체크관련 수정  (0702 비정기 배포) 
**************************************************************************************/

using C1.WPF;
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
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_412.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_412 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();

        public COM001_412()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            SetEvent();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnResend);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.MessageValidation("SFU2042", "31");

                    dtpDateFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                    if (LGCdp.Name.Equals("dtpDateTo"))
                        dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                    else
                        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-30);

                    dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                    return;
                }

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }

            }
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            // Factory, 동 설정 
            CommonCombo _combo = new CommonCombo();

            //Shop
            C1ComboBox[] cboShopChild = { cboArea };
            _combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sCase: "SHOP_AUTH");

            C1ComboBox[] cboShopChild2 = { cboAreaquery };
            _combo.SetCombo(cboShop2, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild2, sCase: "SHOP_AUTH");

            //동
            C1ComboBox[] cboAreaParent = { cboShop };

            C1ComboBox[] cboAreaParent2 = { cboShop2 };

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbParent: cboAreaParent);

            _combo.SetCombo(cboAreaquery, CommonCombo.ComboStatus.SELECT, cbParent: cboAreaParent2);

            // 처리유형, 처리결과 (공통코드 이용) 
            //_combo.SetCombo(cboTrsfPostTypeCode, CommonCombo.ComboStatus.ALL, sCase: "");
            //_combo.SetCombo(cboRsltPrcsType, CommonCombo.ComboStatus.ALL, sCase: "");

            // 처리유형 
            CommonCombo.CommonBaseCombo("DA_BAS_SEL_ERP_RSLT_TYPE_CODE", cboRsltTypeCode,
                new string[] { "LANGID", "FACILITY_CODE", "USE_TYPE" }, new string[] { LoginInfo.LANGID, cboArea.SelectedValue.ToString(), "TRSF_POST" },
                CommonCombo.ComboStatus.ALL, "ERP_RSLT_TYPE_CODE", "ERP_RSLT_TYPE_NAME");

            // 처리 결과 
            CommonCombo.CommonBaseCombo("DA_BAS_SEL_ERP_STATUS_NERP", cboErpStatusCode,
                new string[] { "LANGID" }, new string[] { LoginInfo.LANGID },
                CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");

        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void dgErpHist_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        private void dgErpHist_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell != null &&
                e.Cell.Presenter != null &&
                e.Cell.Presenter.Content != null)
            {
                CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":

                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                            if (!chk.IsChecked.Value)
                            {
                                chkAll.IsChecked = false;
                            }
                            else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                            {
                                chkAll.IsChecked = true;
                            }

                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            break;
                    }
                }
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
       
                if ((bool)chkAll.IsChecked)
                {
                    for (int inx = 0; inx < dgErpHist.GetRowCount(); inx++)
                    {
                        DataTableConverter.SetValue(dgErpHist.Rows[inx].DataItem, "CHK", true);
                    }
                }
            }
            catch
            {
            }
        }

        void checkAll2_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)chkAll.IsChecked)
                {
                    for (int inx = 0; inx < dgErpHist2.GetRowCount(); inx++)
                    {
                        DataTableConverter.SetValue(dgErpHist2.Rows[inx].DataItem, "CHK", true);
                    }
                }
            }
            catch
            {
            }
        }


        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
    
            for (int inx = 0; inx < dgErpHist.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgErpHist.Rows[inx].DataItem, "CHK", false);
            }
        }

        void checkAll2_Unchecked(object sender, RoutedEventArgs e)
        {

            for (int inx = 0; inx < dgErpHist2.GetRowCount(); inx++)
            {
                DataTableConverter.SetValue(dgErpHist2.Rows[inx].DataItem, "CHK", false);
            }
        }

        #endregion

        #region Events
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgErpHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() => {

                Util.gridClear(dgInput);

                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null) return;

                C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;


                if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                // 라디오 버튼 강제 선택 및 이벤트 실행 
                var cell = dg.GetCell(currCell.Row.Index, 0);
                var radio = (System.Windows.Controls.RadioButton)cell.Presenter.Content;
                radio.IsChecked = true;

                // ERP_TRNF_SEQNO
                //GetInputData(currCell.Row);

            }));
        }

        #region 조회탭 프로세스 버튼 사용중지 (이력탭에서 실행)
        // 강제취소: FORCE_CANCEL
        private void btnForceCancl_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ForcedSend("FORCE_CANCEL");
                }
            });
        }

        // 재처리: FORCE_REPROCESSING
        private void btnResend_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ForcedSend("FORCE_REPROCESSING");
                }
            });
        }

        // 사용자확인 설정: USER_CHK_SET
        private void btnSetUserConfig_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ForcedSend("USER_CHK_SET");
                }
            });
        }

        // 사용자확인해제: USER_CHK_REL
        private void btnUnSetUserConfig_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ForcedSend("USER_CHK_REL");
                }
            });
        }
        #endregion

        private void dgErpHist2_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll2_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll2_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll2_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll2_Unchecked);
                    }
                }
            }));
        }

        // 강제취소: FORCE_CANCEL
        private void btnForceCancl2_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation("FORCE_CANCEL"))
            {
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ForcedSend2("FORCE_CANCEL");
                }
            });
        }

        // 재처리: FORCE_REPROCESSING
        private void btnResend2_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation("FORCE_REPROCESSING"))
            {
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ForcedSend2("FORCE_REPROCESSING");
                }
            });
        }

        // 사용자확인 설정: USER_CHK_SET
        private void btnSetUserConfig2_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation("USER_CHK_SET"))
            {
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ForcedSend2("USER_CHK_SET");
                }
            });
        }

        // 사용자확인해제: USER_CHK_REL
        private void btnUnSetUserConfig2_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation("USER_CHK_REL"))
            {
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ForcedSend2("USER_CHK_REL");
                }
            });
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtErpSeqNo.Text) || cboAreaquery.SelectedIndex == 0)
            {
                dgErpHist2.ItemsSource = null;
                Util.gridClear(dgErpHist2);
                return;
            }

            GetList2();
        }

        #endregion

        #region Method

        private bool Validation(string command)
        {
            // 체크 항목 개수 if (Convert.ToBoolean(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "CHK")))
            if (!dgErpHist2.Rows.Any(r => Convert.ToBoolean(DataTableConverter.GetValue(r.DataItem, "CHK")) == true))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 이벤트별 체크박스 확인 : FORCE_CANCEL, FORCE_REPROCESSING, USER_CHK_SET, USER_CHK_REL
            // GRID 컬럼 : FORCE_CNCL_ABLE_FLAG, REPROCESS_ABLE_FLAG, USER_CHK_SET_ABLE_FLAG, USER_CHK_REL_ABLE_FLAG

            string col = string.Empty;
            switch (command)
            {
                case "FORCE_CANCEL":
                    col = "FORCE_CNCL_ABLE_FLAG";
                    break;
                case "FORCE_REPROCESSING":
                    col = "REPROCESS_ABLE_FLAG";
                    break;
                case "USER_CHK_SET":
                    col = "USER_CHK_SET_ABLE_FLAG";
                    break;
                default:
                    col = "USER_CHK_REL_ABLE_FLAG";
                    break;
            }

            string message = string.Empty;
            List<string> erpSeqNos = new List<string>();
            int inValidCount = 0;

            foreach (C1.WPF.DataGrid.DataGridRow r in dgErpHist2.Rows)
            {

                if (Convert.ToBoolean(DataTableConverter.GetValue(r.DataItem, "CHK")) == true &&
                    DataTableConverter.GetValue(r.DataItem, col).ToString() != "Y")
                {
                    inValidCount++;
                    erpSeqNos.Add(DataTableConverter.GetValue(r.DataItem, "ERP_TRNF_SEQNO").ToString());
                }
            }

            if (inValidCount > 0)
            {
                string erpSeqNo = "[" + String.Join(", ", erpSeqNos) + "]\r\n";
                //System.Windows.Forms.MessageBox.Show(erpSeqNo);
                // 메세지 창 실행 
                Util.MessageValidation("SFU5191", erpSeqNo);
                return false;
            }

            // 유효성 체크 
            string chkMessage = "OK";
            List<string> chkMsgList = new List<string>();
            foreach (C1.WPF.DataGrid.DataGridRow r in dgErpHist2.Rows)
            {

                if (Convert.ToBoolean(DataTableConverter.GetValue(r.DataItem, "CHK")) == true &&
                    DataTableConverter.GetValue(r.DataItem, col).ToString() == "Y")
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "INDATA";

                    dtRqst.Columns.Add("FACILITY_CODE", typeof(string));
                    dtRqst.Columns.Add("PRCS_TYPE", typeof(string));
                    dtRqst.Columns.Add("TRNF_TYPE_CODE", typeof(string));
                    dtRqst.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                    DataRow dr = dtRqst.NewRow();

                    dr["FACILITY_CODE"] = DataTableConverter.GetValue(r.DataItem, "FACILITY_CODE").ToString();
                    dr["PRCS_TYPE"] = command;
                    dr["TRNF_TYPE_CODE"] = "TRSF_POST";
                    dr["ERP_TRNF_SEQNO"] = DataTableConverter.GetValue(r.DataItem, "ERP_TRNF_SEQNO").ToString();

                    dtRqst.Rows.Add(dr);

                    DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CHK_NERP_TRNF_MANUAL_PRCS", "INDATA", "RSLTDT", dtRqst);

                    if (dt != null && dt.Rows.Count == 1)
                    {
                        string returnMsg = dt.Rows[0]["MESSAGE"].ToString();
                        if (returnMsg != "OK")
                        {
                            chkMsgList.Add(returnMsg);
                        }
                    }
                }
            }// foreach (C1.WPF.DataGrid.DataGridRow r in dgErpHist2.Rows)

            if (chkMsgList.Count > 0)
            {
                chkMessage = String.Join("\r\n", chkMsgList);
                //System.Windows.Forms.MessageBox.Show(chkMessage);
                // 메세지 창 실행 
                Util.MessageValidation("SFU5191", chkMessage);
                return false;
            }

            return true;
        }


        private void GetList()
        {
            ClearGrid();
            Util.gridClear(dgErpHist);
            Util.gridClear(dgInput);

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("MTRLID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("ERP_MNGT_LOTID", typeof(string));
                dtRqst.Columns.Add("FACILITY_CODE", typeof(string));
                dtRqst.Columns.Add("TRSF_POST_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("ERP_ERR_CODE", typeof(string));
                dtRqst.Columns.Add("NON_PRCS_FLAG", typeof(string));
                dtRqst.Columns.Add("EXCL_CNCL_FLAG", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("DAY_CLOSE_HMS", typeof(string));
                dtRqst.Columns.Add("SEARCH_DATE_TYPE", typeof(string));
                dtRqst.Columns.Add("ROWS", typeof(string));

                DataRow dr = dtRqst.NewRow();

                //dr["LANGID"] = LoginInfo.LANGID;
                dr["MTRLID"] = string.IsNullOrWhiteSpace(txtMtrlID.Text) ? null : txtMtrlID.Text;
                dr["LOTID"] = string.IsNullOrWhiteSpace(txtLotID.Text) ? null : txtLotID.Text;
                dr["ERP_MNGT_LOTID"] = string.IsNullOrWhiteSpace(txtErpMngtLotID.Text) ? null : txtErpMngtLotID.Text;
                dr["FACILITY_CODE"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["TRSF_POST_TYPE_CODE"] = Util.GetCondition(cboRsltTypeCode, bAllNull: true); 
                dr["ERP_ERR_CODE"] = Util.GetCondition(cboErpStatusCode, bAllNull: true); 
                dr["NON_PRCS_FLAG"] = chkNonPrcsFlag.IsChecked == true ? "Y" : null; // 2025.07.01 수정
                dr["EXCL_CNCL_FLAG"] = chkExclCnclFlag.IsChecked == true ? "Y" : null;
                dr["FROM_DATE"] = chkNonPrcsFlag.IsChecked == true ? null : dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = chkNonPrcsFlag.IsChecked == true ? null : dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["DAY_CLOSE_HMS"] = GetCloseHMS(); // 일마감시분초
                dr["SEARCH_DATE_TYPE"] = rdoPostDate.IsChecked == true ? "POST_DATE" : "TRNF_DATE";
                dr["ROWS"] = txtCount.Value;


                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_NERP_TRSF_RSLT", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        Util.GridSetData(dgErpHist, result, FrameOperation, true);
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
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetList2()
        {
            dgErpHist2.ItemsSource = null;
            Util.gridClear(dgErpHist2);


            try
            {
                List<string> erpSeqList = new List<string>();
                string clip = String.IsNullOrWhiteSpace(txtErpSeqNo.Text) == true ? Clipboard.GetText() : txtErpSeqNo.Text;
                string[] lines = clip.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                lines.ToList().ForEach(s => erpSeqList.Add(s.Trim('\'')));

                string inErpSeqClause = string.Join(",", erpSeqList.Select(x => $"{x}"));

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FACILITY_CODE", typeof(string));
                dtRqst.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FACILITY_CODE"] = Util.GetCondition(cboAreaquery, bAllNull: true);
                dr["ERP_TRNF_SEQNO"] = inErpSeqClause;

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_NERP_TRSF_RSLT_BY_SEQ", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        Util.GridSetData(dgErpHist2, result, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        txtErpSeqNo.Clear();
                        HiddenLoadingIndicator();
                    }
                });

                //string[] sColumnName = new string[] { "EQSGNAME", "PROCNAME", "EQPTNAME", "PRODNAME", "UNIT_CODE", "WOID", "LOTID" };

                //_Util.SetDataGridMergeExtensionCol(dgErpHist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearGrid()
        {
            dgErpHist.ItemsSource = null;
            dgInput.ItemsSource = null;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void GetInputData(C1.WPF.DataGrid.DataGridRow dataitem)
        {
            string facilityCode = Util.GetCondition(cboArea, bAllNull: true);
            string seqNo = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "ERP_TRNF_SEQNO"));

            dgInput.ItemsSource = null;
            Util.gridClear(dgInput);

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FACILITY_CODE", typeof(string));
                dtRqst.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["FACILITY_CODE"] = facilityCode;
                dr["ERP_TRNF_SEQNO"] = seqNo;

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_NERP_TRSF_RSLT_LOT", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        Util.GridSetData(dgInput, result, FrameOperation, true);
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
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private string GetCloseHMS()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));
            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
            dtRqst.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA", "RQSTDT", "RSLTDT", dtRqst);
            return dt.Rows[0]["DAY_CLOSE_HMS"].ToString();
        }

     
        private void ForcedSend(string prcsType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("FACILITY_CODE", typeof(String));
                RQSTDT.Columns.Add("PRCS_TYPE", typeof(String));
                RQSTDT.Columns.Add("TRNF_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("USER", typeof(String));
                RQSTDT.Columns.Add("ERP_TRNF_SEQNO", typeof(String));
                RQSTDT.Columns.Add("TRGT_TBL_NAME", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY1", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY2", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY3", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY4", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY5", typeof(String));

                for (int i = 0; i < dgErpHist.Rows.Count() - dgErpHist.BottomRows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK").Equals(true) ||
                        Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK")).Equals("1"))
                    {


                        DataRow dr = RQSTDT.NewRow();
                        dr["FACILITY_CODE"] = Util.GetCondition(cboArea, bAllNull: true);
                        dr["PRCS_TYPE"] = prcsType;
                        dr["TRNF_TYPE_CODE"] = "TRSF_POST";
                        dr["USER"] = LoginInfo.USERID;
                        dr["ERP_TRNF_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "ERP_TRNF_SEQNO"));
                        dr["TRGT_TBL_NAME"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "TRGT_TBL_NAME"));
                        dr["TRGT_KEY1"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "TRGT_KEY1"));
                        dr["TRGT_KEY2"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "TRGT_KEY2"));
                        dr["TRGT_KEY3"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "TRGT_KEY3"));
                        dr["TRGT_KEY4"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "TRGT_KEY4"));
                        dr["TRGT_KEY5"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "TRGT_KEY5"));

                        RQSTDT.Rows.Add(dr);
                    }
                }

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_ACT_REG_NERP_TRNF_MANUAL_PRCS", "INDATA", "RSLTDT", RQSTDT);
                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                GetList();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void ForcedSend2(string prcsType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("FACILITY_CODE", typeof(String));
                RQSTDT.Columns.Add("PRCS_TYPE", typeof(String));
                RQSTDT.Columns.Add("TRNF_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("USER", typeof(String));
                RQSTDT.Columns.Add("ERP_TRNF_SEQNO", typeof(String));
                RQSTDT.Columns.Add("TRGT_TBL_NAME", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY1", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY2", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY3", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY4", typeof(String));
                RQSTDT.Columns.Add("TRGT_KEY5", typeof(String));

                for (int i = 0; i < dgErpHist2.Rows.Count() - dgErpHist2.BottomRows.Count; i++)
                {
                    if (Convert.ToBoolean(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "CHK")))
                    {


                        DataRow dr = RQSTDT.NewRow();
                        dr["FACILITY_CODE"] = Util.GetCondition(cboAreaquery, bAllNull: true);
                        dr["PRCS_TYPE"] = prcsType;
                        dr["TRNF_TYPE_CODE"] = "TRSF_POST";
                        dr["USER"] = LoginInfo.USERID;
                        dr["ERP_TRNF_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "ERP_TRNF_SEQNO"));
                        dr["TRGT_TBL_NAME"] = Util.NVC(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "TRGT_TBL_NAME"));
                        dr["TRGT_KEY1"] = Util.NVC(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "TRGT_KEY1"));
                        dr["TRGT_KEY2"] = Util.NVC(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "TRGT_KEY2"));
                        dr["TRGT_KEY3"] = Util.NVC(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "TRGT_KEY3"));
                        dr["TRGT_KEY4"] = Util.NVC(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "TRGT_KEY4"));
                        dr["TRGT_KEY5"] = Util.NVC(DataTableConverter.GetValue(dgErpHist2.Rows[i].DataItem, "TRGT_KEY5"));

                        RQSTDT.Rows.Add(dr);
                    }
                }

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_ACT_REG_NERP_TRNF_MANUAL_PRCS", "INDATA", "RSLTDT", RQSTDT);
                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                GetList2();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }


        #endregion

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgInput);

            RadioButton rb = sender as RadioButton;

            //C1DataGrid dg = ((C1DataGrid)rb.Parent) as C1DataGrid;

            //if (dg == null) return;

            C1.WPF.DataGrid.DataGridCell currCell = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell; //dg.CurrentCell;


            if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;


            // ERP_TRNF_SEQNO
            GetInputData(currCell.Row);
        }

        private void txtErpSeqNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    GetList2();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                GetList2();
                e.Handled = true;
            }
        }
    }
}
