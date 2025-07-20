/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 작업지시
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2021.01.18  cnskmaru   C20210116-000009 GEMS 게시판 사용 시 오류 수정 요청
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_048 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        string sSeqNo = null;
        string sNoticeClass = "NOTICE";
        public COM001_048()
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
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, EventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnHold);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            ldpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);

            

            this.Loaded -= UserControl_Loaded;
        }

       

        //화면내 combo 셋팅
        private void InitCombo()
        {


        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("LVL_TITL"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }


        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            if (dg.CurrentColumn.Name.Equals("LVL_TITL") && dg.GetRowCount() > 0 && dg.CurrentRow!=null)
            {
                grWrite.Visibility = Visibility.Collapsed;
                grRead.Visibility = Visibility.Collapsed;
                grModify.Visibility = Visibility.Collapsed;


                sSeqNo = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "REG_SEQNO"));

                

                //작성자와 동일하면 수정가능하도록
                //if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "INSUSER")).Equals(LoginInfo.USERID))
                //{
                //    grModify.Visibility = Visibility.Visible;
                //    GetModify(sSeqNo);
                //}
                //else
                //{
                //    grRead.Visibility = Visibility.Visible;
                //    GetDetail(sSeqNo);
                //}

                //공지사항은 관리자만 사용함으로 무조건 수정으로
                grModify.Visibility = Visibility.Visible;
                GetModify(sSeqNo);
            }
        }




        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "All files (*.*)|*.*";
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


        private void btnUploadFileModify_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "All files (*.*)|*.*";
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
            if (!Util.GetCondition(txtFilePathRead).Equals(""))
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("REG_SEQNO", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["REG_SEQNO"] = sSeqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_SEL_ONE_FILE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Byte[] bytes = (Byte[])dtRslt.Rows[0]["ATTCH_FILE_CNTT"];

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
            ldpDateFromNotice.SelectedDateTime = (DateTime)System.DateTime.Now;
            ldpDateToNotice.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(7);
            sSeqNo = null;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("NOTICE_CLSS_CODE", typeof(string));
            dtRqst.Columns.Add("UPPR_REG_SEQNO", typeof(string));
            dtRqst.Columns.Add("TITL", typeof(string));
            dtRqst.Columns.Add("CNTT", typeof(string));
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("DISP_STRT_DATE", typeof(string));
            dtRqst.Columns.Add("DISP_END_DATE", typeof(string));
            dtRqst.Columns.Add("ATTCH_FILE_NAME", typeof(string));
            dtRqst.Columns.Add("ATTCH_FILE_CNTT", typeof(Byte[]));

            DataRow dr = dtRqst.NewRow();

            dr["NOTICE_CLSS_CODE"] = sNoticeClass;
            dr["UPPR_REG_SEQNO"] = sSeqNo;
            dr["TITL"] = Util.GetCondition(txtTitle);
            dr["CNTT"] = Util.GetCondition(txtContext);
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USERID"] = LoginInfo.USERID;
            dr["DISP_STRT_DATE"] = Util.GetCondition(ldpDateFromNotice);
            dr["DISP_END_DATE"] = Util.GetCondition(ldpDateToNotice);

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
            dtRqst.Columns.Add("DISP_STRT_DATE", typeof(string));
            dtRqst.Columns.Add("DISP_END_DATE", typeof(string));

            DataRow dr = dtRqst.NewRow();

            dr["NOTICE_CLSS_CODE"] = sNoticeClass;
            dr["REG_SEQNO"] = sSeqNo;
            dr["TITL"] = Util.GetCondition(txtTitleModify);
            dr["CNTT"] = Util.GetCondition(txtContextModify);
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USERID"] = LoginInfo.USERID;
            dr["DISP_STRT_DATE"] = Util.GetCondition(ldpDateFromModify);
            dr["DISP_END_DATE"] = Util.GetCondition(ldpDateToModify);
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

            dr["NOTICE_CLSS_CODE"] = sNoticeClass;
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

                    if(dtRslt.Rows[0]["DISP_STRT_DATE"] != null && !dtRslt.Rows[0]["DISP_STRT_DATE"].ToString().Equals(""))
                        ldpDateFromModify.SelectedDateTime = DateTime.ParseExact(dtRslt.Rows[0]["DISP_STRT_DATE"].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);

                    if (dtRslt.Rows[0]["DISP_END_DATE"] != null && !dtRslt.Rows[0]["DISP_END_DATE"].ToString().Equals(""))
                        ldpDateToModify.SelectedDateTime = DateTime.ParseExact(dtRslt.Rows[0]["DISP_END_DATE"].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);

                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetList()
        {

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("NOTICE_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["NOTICE_CLSS_CODE"] = sNoticeClass;
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(ldpDateTo);

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_BOARD_SEL_LIST_NOTICE", "INDATA", "OUTDATA", dtRqst);


                Util.GridSetData(dgList, dtRslt, FrameOperation);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }





        #endregion


    }
}
