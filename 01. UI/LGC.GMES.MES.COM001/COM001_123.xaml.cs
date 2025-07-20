/*************************************************************************************
 Created Date : 2017.07.13
      Creator : 
   Decription : 생산 일별 특이사항
--------------------------------------------------------------------------------------
 [Change History]
    2017.11.28  INS염규범 : 신규 생성
    2021.04.07  안인효    : C20210315-000429 (160161) 설비세그먼트 비고 항목 코드 (EQSG_NOTE_ITEM_CODE) 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_123 : UserControl
    {
        #region Declaration & Constructor , Initialize
        string parameter = string.Empty;

        public COM001_123()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region [Click이벤트]

        #region [검색창 클릭]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region [ 새창 호출 ]
        private void btnNewNote_Click(object sender, RoutedEventArgs e)
        {
            string userId = string.Empty;
            int count = 0;

            for (int i = 0; i < dgEquipmentNote.Rows.Count; i++)
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if (DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK").Equals(true) || 
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    userId = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "UPDUSERID"));
                    count++;
                }
            }

          
            COM001_123_NOTE newNote = new COM001_123_NOTE();
            newNote.FrameOperation = FrameOperation;

            if(count == 0) { 
                if (newNote != null)
                {

                    object[] Parameters = new object[6];

                        Parameters[0] = "";
                        Parameters[1] = "";
                        Parameters[2] = "";
                        Parameters[3] = GetEquipmentWorkDate();
                        Parameters[4] = string.Empty;
                        Parameters[5] = string.Empty;

                    C1WindowExtension.SetParameters(newNote, Parameters);

                    newNote.Closed += new EventHandler(newNote_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => newNote.ShowModal()));
                }

            } else if(count == 1){

                if (userId != LoginInfo.USERID)
                {
                    Util.MessageInfo("SFU4312");

                    return;
                }


                object[] Parameters = new object[6];
                //cboEquipmentSegment.SelectedValue.ToString();

                for (int i = 0; i < dgEquipmentNote.Rows.Count; i++)
                {
                    // MES 2.0 CHK 컬럼 Bool 오류 Patch
                    if (DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK").Equals(true) || 
                        Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "NOTE_SEQNO"));
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "SPCL_NOTE"));
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "EQSGID"));
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "WRK_DATE"));
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "EQSG_NOTE_CLSS_CODE"));
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "EQSG_NOTE_ITEM_CODE"));
                    }
                }
                    C1WindowExtension.SetParameters(newNote, Parameters);

                    newNote.Closed += new EventHandler(newNote_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => newNote.ShowModal()));

                }else{

                    Util.MessageInfo("SFU4159");     

                }

        }
        #endregion

        #region [ 삭제 버튼 클릭 ]
        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            int icount = 0;

            if (!CanNote())
            {
                return;
            }

            for (int i = 0; i < dgEquipmentNote.Rows.Count; i++)
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if (DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK").Equals(true) || 
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    icount++;
                }
            }

            if (icount == 1)
            {
                deleteChkNote();
            }
            else
            {
                Util.MessageInfo("SFU4159");
                return;
            }
        }
        #endregion

        #endregion

        #region [ Method ]

        #region [ 데이터 조회 ]
        public void GetList()
            {
            try
            {
                string _equipmentSegment = string.Empty;

                _equipmentSegment = cboEquipmentSegment.SelectedValue.ToString();

                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSG_NOTE_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("EQSG_NOTE_ITEM_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSG_NOTE_CLSS_CODE"] = cboEquipmentSegmentClassCode.SelectedValue.GetString() == string.Empty ? null : cboEquipmentSegmentClassCode.SelectedValue;
                dr["EQSG_NOTE_ITEM_CODE"] = cboEquipmentSegmentClassCodeItem.SelectedValue.GetString() == string.Empty ? null : cboEquipmentSegmentClassCodeItem.SelectedValue;

                if (_equipmentSegment == "")
                    {
                        dtRqst.Columns.Add("FROM_DATE", typeof(string));
                        dtRqst.Columns.Add("TO_DATE", typeof(string));

                        dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                        dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                    }else{
                        dtRqst.Columns.Add("EQSGID", typeof(string));
                        dtRqst.Columns.Add("FROM_DATE", typeof(string));
                        dtRqst.Columns.Add("TO_DATE", typeof(string));

                    dr["EQSGID"] = _equipmentSegment;
                        dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                        dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                    }

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_NOTE_LIST", "INDATA", "OUTDATA", dtRqst);

                    //보류 상태
                    //dgEquipmentNote.Columns["eqsgNote"].Width = "12"; 

                    Util.GridSetData(dgEquipmentNote, dtRslt, FrameOperation, true);
                
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
        #endregion

        #region [ 삭제 ]
        public void deleteChkNote()
        {
            string EQSGID = string.Empty;

            if (dgEquipmentNote.Rows.Count == 0)
            {
                return;
            }

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";

            RQSTDT.Columns.Add("EQSGID", typeof(String));
            RQSTDT.Columns.Add("SPCL_NOTE", typeof(String));
            RQSTDT.Columns.Add("DEL_FLAG", typeof(String));
            RQSTDT.Columns.Add("USER_ID", typeof(String));
            RQSTDT.Columns.Add("NOTE_SEQNO", typeof(String));
            RQSTDT.Columns.Add("WRK_DATE", typeof(String));
            RQSTDT.Columns.Add("EQSG_NOTE_CLSS_CODE", typeof(String));
            RQSTDT.Columns.Add("EQSG_NOTE_ITEM_CODE", typeof(String));

            DataRow dr = RQSTDT.NewRow();

            for (int i = 0; i < dgEquipmentNote.Rows.Count; i++)
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if (DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK").Equals(true) || 
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("1"))
                {

                    dr["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "EQSGID"));
                    dr["SPCL_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "SPCL_NOTE"));
                    dr["DEL_FLAG"] = 'Y';
                    dr["USER_ID"] = LoginInfo.USERID;
                    dr["NOTE_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "NOTE_SEQNO"));
                    dr["WRK_DATE"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[0].DataItem, "WRK_DATE").GetString().Replace("-", "");
                    dr["EQSG_NOTE_CLSS_CODE"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[0].DataItem, "EQSG_NOTE_CLSS_CODE");
                    dr["EQSG_NOTE_ITEM_CODE"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[0].DataItem, "EQSG_NOTE_ITEM_CODE");

                    //Util.GetCondition(Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "WRK_DATE")));
                    // newRow["NOTE_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "NOTE_SEQNO"));
                    //EQSGID = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "EQSGID"));

                    RQSTDT.Rows.Add(dr);
                }
            }

            new ClientProxy().ExecuteService("BR_PRD_REG_LINE_NOTE", "INDATA", "", RQSTDT, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    Util.MessageException(ex); 
                }
                else
                {
                    Util.MessageInfo("SFU3544");     
                    GetList();
                }
            });

        }
        #endregion

        #region [ 자동 REFRESH ]
        private void newNote_Closed(object sender, EventArgs e)
        {
            COM001_123_NOTE window = sender as COM001_123_NOTE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [ 선택내용 없을때 ]
        private bool CanNote()
        {
            bool bRet = false;
            int count = 0;
            for (int i = 0; i < dgEquipmentNote.Rows.Count; i++)
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if (DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK").Equals(true) || 
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    count++;
                }
            }

            if (count == 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }
            else
            {
                bRet = true;
                return bRet;
            }
        }
        #endregion

        #region [ AREAID 로 지역 시간 가져오기 ]
        private static string GetEquipmentWorkDate()
        {
            string returnWorkDate = string.Empty;
            try
            {
                const string bizRuleName = "DA_PRD_SEL_EQPT_NOTE_WORKDATE";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(inData);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inDataTable);
                if (CommonVerify.HasTableRow(dtResult))
                {
                    returnWorkDate = dtResult.Rows[0]["CALDATE_YYYY"] + "-" + dtResult.Rows[0]["CALDATE_MM"] + "-" + dtResult.Rows[0]["CALDATE_DD"];
                }
                return returnWorkDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return returnWorkDate;
            }
        }
        #endregion

        #region [ 콤보박스 만들기 ]
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter);

            //SetEquipmentSegmentClassCodeCombo(cboEquipmentSegmentClassCode);
            //SetEquipmentSegmentClassCodeDataGridCombo();
            SetEquipmentSegmentNoteCombo(cboEquipmentSegmentClassCode, "EQSG_NOTE_CLSS_CODE");
            SetEquipmentSegmentNoteCombo(cboEquipmentSegmentClassCodeItem, "EQSG_NOTE_ITEM_CODE");
        }
        #endregion

        private void SetEquipmentSegmentClassCodeDataGridCombo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_COMMONCODE";
                const string selectedValueText = "CBO_CODE";
                const string displayMemberText = "CBO_NAME";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "EQSG_NOTE_CLSS_CODE";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
                DataRow newRow = dtBinding.NewRow();

                //newRow[selectedValueText] = string.Empty;
                //newRow[displayMemberText] = "-SELECT-";
                //dtBinding.Rows.InsertAt(newRow, 0);

                (dgEquipmentNote.Columns["EQSG_NOTE_CLSS_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtBinding.Copy());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentSegmentClassCodeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "EQSG_NOTE_CLSS_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetEquipmentSegmentNoteCombo(C1ComboBox cbo, string strCode)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, strCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #region [ loading 아이콘 ]
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
        #endregion

        #endregion
    }
}