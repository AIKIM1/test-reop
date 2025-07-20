/*************************************************************************************
 Created Date : 2017.07.13
      Creator : 
   Decription : 생산 일별 특이사항
--------------------------------------------------------------------------------------
 [Change History]
    2017.11.28  INS염규범 : 신규 생성
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
    public partial class COM001_123_ELEC : UserControl
    {
        #region Declaration & Constructor , Initialize
        string parameter = string.Empty;
        CommonCombo _combo = new CommonCombo();

        public COM001_123_ELEC()
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

        #region [ 콤보박스 만들기 ]
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            //String[] sFilter2 = { cboEquipmentSegment.SelectedValue.ToString(), LoginInfo.CFG_PROC_ID, null };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2);

            //특이사항 항목
            string[] sFilter1 = { "EQSG_NOTE_ITEM_CODE" };
            _combo.SetCombo(cboNoteItemCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
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
                if (Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("TRUE") ||
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    userId = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "UPDUSERID"));
                    count++;
                }
            }


            COM001_123_NOTE_ELEC newNote = new COM001_123_NOTE_ELEC();
            newNote.FrameOperation = FrameOperation;

            if (count == 0)
            {
                if (newNote != null)
                {

                    object[] Parameters = new object[4];

                    Parameters[0] = "";
                    Parameters[1] = "";
                    Parameters[2] = "";
                    Parameters[3] = GetEquipmentWorkDate();

                    C1WindowExtension.SetParameters(newNote, Parameters);

                    newNote.Closed += new EventHandler(newNote_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => newNote.ShowModal()));
                }

            }
            else if (count == 1)
            {

                if (userId != LoginInfo.USERID)
                {
                    Util.MessageInfo("SFU4312");

                    return;
                }


                object[] Parameters = new object[4];
                //cboEquipmentSegment.SelectedValue.ToString();

                for (int i = 0; i < dgEquipmentNote.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("TRUE") ||
                        Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "NOTE_SEQNO"));
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "SPCL_NOTE"));
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "EQPTID"));
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "WRK_DATE"));
                    }
                }
                C1WindowExtension.SetParameters(newNote, Parameters);

                newNote.Closed += new EventHandler(newNote_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => newNote.ShowModal()));

            }
            else
            {
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
                if (Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("TRUE") ||
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
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSG_NOTE_ITEM_CODE", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (dr["EQSGID"].Equals(""))
                {
                    Util.MessageInfo("SFU1223");
                    return;
                }
                
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                if (cboEquipment.SelectedValue.ToString().Equals("")) { }
                else
                {
                    dr["EQPTID"] = cboEquipment.SelectedValue.ToString();
                }
                
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["EQSG_NOTE_ITEM_CODE"] = cboNoteItemCode.SelectedValue.ToString() == "" ? null : cboNoteItemCode.SelectedValue.ToString();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_NOTE_ELEC_LIST", "INDATA", "OUTDATA", dtRqst);

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
            RQSTDT.Columns.Add("EQPTID", typeof(String));
            RQSTDT.Columns.Add("SPCL_NOTE", typeof(String));
            RQSTDT.Columns.Add("DEL_FLAG", typeof(String));
            RQSTDT.Columns.Add("USER_ID", typeof(String));
            RQSTDT.Columns.Add("NOTE_SEQNO", typeof(String));
            RQSTDT.Columns.Add("WRK_DATE", typeof(String));
            RQSTDT.Columns.Add("EQSG_NOTE_ITEM_CODE", typeof(string)); //2021.05.25 C20210518-000427[생산 특이사항 입력 구분자 추가]

            DataRow dr = RQSTDT.NewRow();

            for (int i = 0; i < dgEquipmentNote.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    dr["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "EQSGID"));
                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "EQPTID"));
                    dr["SPCL_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "SPCL_NOTE"));
                    dr["DEL_FLAG"] = 'Y';
                    dr["USER_ID"] = LoginInfo.USERID;
                    dr["NOTE_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "NOTE_SEQNO"));
                    dr["WRK_DATE"] = DataTableConverter.GetValue(dgEquipmentNote.Rows[0].DataItem, "WRK_DATE").GetString().Replace("-", "");
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
            COM001_123_NOTE_ELEC window = sender as COM001_123_NOTE_ELEC;
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
                if (Util.NVC(DataTableConverter.GetValue(dgEquipmentNote.Rows[i].DataItem, "CHK")).Equals("TRUE") ||
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
        //추가
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboEquipmentSegment.SelectedValue) != string.Empty)
            {
            
                    String[] sFilter2 = { cboEquipmentSegment.SelectedValue.ToString(), LoginInfo.CFG_PROC_ID, null };//2017-08-14 권병훈C 요청으로 LoginInfo.CFG_PROC_ID -> "E0500,E1000" 수정
                    //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment};
                    _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter2);
                    //cboEquipment.SelectedIndex = 0;
                    //_combo.SetCombo(EQUIPMENT, CommonCombo.ComboStatus.ALL, sFilter: sFilter2);
                    //EQUIPMENT.SelectedIndex = 0;
                
            }
        }
    }
}