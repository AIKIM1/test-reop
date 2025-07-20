/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 작업지시
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2021.01.18  cnskmaru   C20210116-000009 GEMS 게시판 사용 시 오류 수정 요청
  2022.11.22  윤지해     C20221025-000519[생산PI팀] GMES 시스템의 게시판 사용 개선을 위한 검색기능 추가 건
  2023.07.03  조영대    : 게시판 다운로드 등록자일때  파일 다운로드 안되는 현상 수정
**************************************************************************************/

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_047 : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        string sSeqNo = null;
        string parameter = string.Empty;

        public COM001_047()
        {
            InitializeComponent();
            InitCombo();
            this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        private void UserControl_Loaded(object sender, EventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnHold);
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            parameter = C1WindowExtension.GetParameter(this);

            ldpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            ldpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            SetEvent();

            //  if (parameter == "LOAD")
            GetList();
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };

            String[] sFilter1 = { "BOARD_TYPE" };
            _combo.SetCombo(cboBoardType, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE");

            cboBoardType.SelectedItemChanged += CboBoardType_SelectedItemChanged; //
            CboBoardType_SelectedItemChanged(null, null);
        }
        #endregion

        #region Event
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            ldpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            ldpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(ldpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = ldpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(ldpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = ldpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                //link 색변경
                if (e.Cell.Column.Name.Equals("LVL_TITL"))
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                else
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
            }));
        }

        private void CboBoardType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboBoardType.Items.Count > 0 && cboBoardType.SelectedValue != null)
            {
                if (cboBoardType.SelectedValue.Equals("NOTICE"))
                    btnReply.Visibility = Visibility.Collapsed;
                else
                    btnReply.Visibility = Visibility.Visible;
            }

            ClearValue();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            if (dg.CurrentColumn.Name.Equals("LVL_TITL") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
            {
                grWrite.Visibility = Visibility.Collapsed;
                grRead.Visibility = Visibility.Collapsed;
                grModify.Visibility = Visibility.Collapsed;
                btnReply.Visibility = Visibility.Visible;

                sSeqNo = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "REG_SEQNO"));

                ClearValue();   //2025.01.15 추가

                if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "NOTICE_CLSS_CODE")).Equals("NOTICE"))//공지사항이면 무조건 읽기모드
                {
                    grRead.Visibility = Visibility.Visible;
                    GetDetail(sSeqNo);
                    btnReply.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //작성자와 동일하면 수정가능하도록
                    if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "INSUSER")).Equals(LoginInfo.USERID))
                    {
                        grModify.Visibility = Visibility.Visible;
                        GetModify(sSeqNo);
                    }
                    else
                    {
                        grRead.Visibility = Visibility.Visible;
                        GetDetail(sSeqNo);
                    }
                }
            }
        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "All files (*.*)|*.*";

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    openFileDialog.InitialDirectory = @"\\Client\C$";
                }

                else
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        if (new System.IO.FileInfo(filename).Length > 5 * 1024 * 1024) //파일크기 체크
                        {
                            Util.AlertInfo("SFU1926");  //첨부파일 크기는 5M 이하입니다.

                            txtFilePath.Text = "";
                        }
                        else
                        {
                            txtFilePath.Text = filename;
                        }
                    }
                }
            }
            catch { }
        }

        private void btnUploadFileModify_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                openFileDialog.InitialDirectory = @"\\Client\C$";
            }
            else
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    if (new System.IO.FileInfo(filename).Length > 5 * 1024 * 1024) //파일크기 체크
                    {
                        Util.AlertInfo("SFU1926");  //첨부파일 크기는 5M 이하입니다.
                        txtFilePathModify.Text = "";
                    }
                    else
                    {
                        txtFilePathModify.Text = filename;
                    }
                }
            }
        }

        private void btnDownFile_Click(object sender, RoutedEventArgs e)
        {
            if (!Util.GetCondition(txtFilePathRead).Equals("") || !Util.GetCondition(txtFilePathOrg).Equals(""))  //2025.01.15 txtFilePathOrg 추가
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("REG_SEQNO", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["REG_SEQNO"] = sSeqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_SEL_ONE_FILE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0 && !Util.IsNVC(dtRslt.Rows[0]["ATTCH_FILE_NAME"]))
                {
                    //Byte[] bytes = (Byte[])dtRslt.Rows[0]["ATTCH_FILE_CNTT"];
                    Byte[] bytes = Convert.FromBase64String(dtRslt.Rows[0]["ATTCH_FILE_CNTT"].ToString());  //2025.01.15 형변환 로직 변경

                    System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                    dialog.ShowDialog();
                    string selected = dialog.SelectedPath;
                    //C20210116-000009 selected 값이 null 또는 "" 인경우 제외 하도록 변경
                    if (!string.IsNullOrEmpty(selected))
                    {
                        File.WriteAllBytes(selected + @"\" + dtRslt.Rows[0]["ATTCH_FILE_NAME"].ToString(), bytes);
                    }

                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            grWrite.Visibility = Visibility.Visible;
            grRead.Visibility = Visibility.Collapsed;
            grModify.Visibility = Visibility.Collapsed;

            sSeqNo = null;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                Util.AlertInfo("FRA0006"); //제목을 입력하세요.
                txtTitle.Focus();

                return;
            }

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("NOTICE_CLSS_CODE", typeof(string));
            dtRqst.Columns.Add("UPPR_REG_SEQNO", typeof(string));
            dtRqst.Columns.Add("TITL", typeof(string));
            dtRqst.Columns.Add("CNTT", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("ATTCH_FILE_NAME", typeof(string));
            dtRqst.Columns.Add("ATTCH_FILE_CNTT", typeof(Byte[]));

            DataRow dr = dtRqst.NewRow();

            dr["NOTICE_CLSS_CODE"] = Util.GetCondition(cboBoardType);
            dr["UPPR_REG_SEQNO"] = sSeqNo;
            dr["TITL"] = Util.GetCondition(txtTitle);
            dr["CNTT"] = Util.GetCondition(txtContext);
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USERID"] = LoginInfo.USERID;

            if (!Util.GetCondition(txtFilePath).Equals(""))
            {
                dr["ATTCH_FILE_NAME"] = System.IO.Path.GetFileName(Util.GetCondition(txtFilePath));
                dr["ATTCH_FILE_CNTT"] = File.ReadAllBytes(Util.GetCondition(txtFilePath));
            }

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new DataTable();
            dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_INS", "RQSTDT", null, dtRqst);

            GetList();

            Util.AlertInfo("SFU1270");  //저장되었습니다.

            ClearValue();

            grWrite.Visibility = Visibility.Collapsed;
            grRead.Visibility = Visibility.Visible;
        }

        private void btnReply_Click(object sender, RoutedEventArgs e)
        {
            grWrite.Visibility = Visibility.Visible;
            grRead.Visibility = Visibility.Collapsed;

            txtTitle.Text = "[Re]" + txtTitleRead.Text;
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("NOTICE_CLSS_CODE", typeof(string));
            dtRqst.Columns.Add("REG_SEQNO", typeof(string));
            dtRqst.Columns.Add("TITL", typeof(string));
            dtRqst.Columns.Add("CNTT", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("ATTCH_FILE_NAME", typeof(string));
            dtRqst.Columns.Add("ATTCH_FILE_CNTT", typeof(Byte[]));

            DataRow dr = dtRqst.NewRow();

            dr["NOTICE_CLSS_CODE"] = Util.GetCondition(cboBoardType);
            dr["REG_SEQNO"] = sSeqNo;
            dr["TITL"] = Util.GetCondition(txtTitleModify);
            dr["CNTT"] = Util.GetCondition(txtContextModify);
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USERID"] = LoginInfo.USERID;

            if (!Util.GetCondition(txtFilePathModify).Equals("") && !txtFilePathModify.Text.Equals(txtFilePathOrg.Text))
            {
                dr["ATTCH_FILE_NAME"] = System.IO.Path.GetFileName(Util.GetCondition(txtFilePathModify));
                dr["ATTCH_FILE_CNTT"] = File.ReadAllBytes(Util.GetCondition(txtFilePathModify));
            }

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new DataTable();
            dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_UPD", "RQSTDT", null, dtRqst);

            GetList();

            Util.AlertInfo("SFU1265");  //수정되었습니다.

            ClearValue();

            grWrite.Visibility = Visibility.Collapsed;
            grModify.Visibility = Visibility.Collapsed;
            grRead.Visibility = Visibility.Visible;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("NOTICE_CLSS_CODE", typeof(string));
            dtRqst.Columns.Add("REG_SEQNO", typeof(string));
            dtRqst.Columns.Add("TITL", typeof(string));
            dtRqst.Columns.Add("CNTT", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("USEFLAG", typeof(string));

            DataRow dr = dtRqst.NewRow();

            dr["NOTICE_CLSS_CODE"] = Util.GetCondition(cboBoardType);
            dr["REG_SEQNO"] = sSeqNo;
            dr["TITL"] = Util.GetCondition(txtTitleModify);
            dr["CNTT"] = Util.GetCondition(txtContextModify);
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USERID"] = LoginInfo.USERID;
            dr["USEFLAG"] = "N";

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new DataTable();
            dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_UPD", "RQSTDT", null, dtRqst);

            GetList();

            Util.AlertInfo("SFU1273");  //삭제되었습니다.

            ClearValue();

            grWrite.Visibility = Visibility.Collapsed;
            grModify.Visibility = Visibility.Collapsed;
            grRead.Visibility = Visibility.Visible;
        }
        #endregion

        #region Method
        private void ClearValue()
        {
            txtContext.Text = "";
            txtContextRead.Text = "";
            txtContextModify.Text = "";
            txtFilePath.Text = "";
            txtFilePathRead.Text = "";
            txtFilePathModify.Text = "";
            txtTitle.Text = "";
            txtTitleRead.Text = "";
            txtTitleModify.Text = "";
        }

        private void GetDetail(string sSeqNo)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("REG_SEQNO", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["REG_SEQNO"] = sSeqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_SEL_ONE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtTitleRead.Text = dtRslt.Rows[0]["TITL"].ToString();

                    txtContextRead.Text = dtRslt.Rows[0]["CNTT"].ToString();

                    txtFilePathRead.Text = dtRslt.Rows[0]["ATTCH_FILE_NAME"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetModify(string sSeqNo)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("REG_SEQNO", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["REG_SEQNO"] = sSeqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_SEL_ONE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtTitleModify.Text = dtRslt.Rows[0]["TITL"].ToString();

                    txtContextModify.Text = dtRslt.Rows[0]["CNTT"].ToString();

                    txtFilePathModify.Text = dtRslt.Rows[0]["ATTCH_FILE_NAME"].ToString();

                    txtFilePathOrg.Text = dtRslt.Rows[0]["ATTCH_FILE_NAME"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("NOTICE_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("TITL", typeof(string));     // 2022.11.22 추가
                dtRqst.Columns.Add("WRITER", typeof(string));   // 2022.11.22 추가

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["NOTICE_CLSS_CODE"] = Util.GetCondition(cboBoardType);
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(ldpDateTo);
                dr["TITL"] = string.IsNullOrEmpty(txtTitleIn.Text) ? null : txtTitleIn.Text;        // 2022.11.22 추가
                dr["WRITER"] = string.IsNullOrEmpty(txtWriterIn.Text) ? null : txtWriterIn.Text;    // 2022.11.22 추가

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_SEL_LIST", "INDATA", "OUTDATA", dtRqst);

                //dgList.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgList, dtRslt, FrameOperation, true);

                if (parameter == "LOAD")
                    if (dtRslt != null)
                        if (dtRslt.Rows.Count == 0)
                            this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
    }
}