/*************************************************************************************
 Created Date : 2017.11.28 
      Creator : INS 염규범S
   Decription : 전지 5MEGA-GMES 구축 - 정보 조회 화면 - 생산 일별 특이사항
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.28  INS 염규범S : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.ComponentModel;
using System.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_123_NOTE_ELEC : C1Window, IWorkArea
    {
        #region [ 함수 정의 ]
        private string _noteSeqNo = string.Empty;
        private string _spclNote = string.Empty;
        private string _eqsgId = string.Empty;
        private string _eqpgld = string.Empty;
        private string _wrkDate = string.Empty;
        private string _regUserId = string.Empty;

        private bool bLoad = false;
        CommonCombo _combo = new CommonCombo();

        #endregion

        #region Initialize        
        public COM001_123_NOTE_ELEC()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region [ 부모창에서 값 받아오며 창열기 ]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _noteSeqNo = Util.NVC(tmps[0]);
            _spclNote = Util.NVC(tmps[1]);
            _eqpgld = Util.NVC(tmps[2]);
            _wrkDate = Util.NVC(tmps[3]);


            bLoad = true;

            InitializeNote();

            InitCombo();

        }
        #endregion

        #endregion

        #region [ Click 이벤트 ]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            saveNote();

        }

        #endregion

        #region  [ Function ] 

        #region [Combo Box 만들기]
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            //특이사항 항목
            string[] sFilter1 = { "EQSG_NOTE_ITEM_CODE" };
            _combo.SetCombo(cboNoteItemCode, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter1);
        }
        #endregion

        #region [BIZ에 값 넣기]
        private void saveNote()
        {
            string EQSGID = string.Empty;


            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";

            RQSTDT.Columns.Add("EQSGID", typeof(String));
            RQSTDT.Columns.Add("EQPTID", typeof(String));
            RQSTDT.Columns.Add("SPCL_NOTE", typeof(String));
            RQSTDT.Columns.Add("DEL_FLAG", typeof(String));
            RQSTDT.Columns.Add("USER_ID", typeof(String));
            RQSTDT.Columns.Add("NOTE_SEQNO", typeof(String));
            RQSTDT.Columns.Add("WRK_DATE", typeof(String));
            RQSTDT.Columns.Add("EQSG_NOTE_ITEM_CODE", typeof(string));  //2021.05.25 C20210518-000427[생산 특이사항 입력 구분자 추가]

            DataRow dr = RQSTDT.NewRow();

            dr["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
            dr["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
            dr["SPCL_NOTE"] = DataTableConverter.GetValue(dqNote.Rows[0].DataItem, "SPCL_NOTE");
            dr["DEL_FLAG"] = 'N';
            dr["USER_ID"] = LoginInfo.USERID;
            dr["WRK_DATE"] = wrkTime.SelectedDateTime.ToString("yyyyMMdd");
            //dr["WRK_DATE"] = DataTableConverter.GetValue(dqNote.Rows[0].DataItem, "WRK_DATE").GetString().Replace("-", "");
            dr["NOTE_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dqNote.Rows[0].DataItem, "NOTE_SEQNO"));
            dr["EQSG_NOTE_ITEM_CODE"] = cboNoteItemCode.SelectedValue.GetString() == "SELECT" ? string.Empty : cboNoteItemCode.SelectedValue;

            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("BR_PRD_REG_LINE_NOTE", "INDATA", "", RQSTDT, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    Util.MessageException(ex);
                }
                else
                {
                    Util.MessageInfo("SFU1270");

                    AsynchronousClose();
                }
            });
        }


        #endregion

        #region [ BIZ에 값 넣기 ]
        private void InitializeNote()
        {
            DataTable dt = MakeDataTable(dqNote);
            DataRow dr = dt.NewRow();
            dr["WRK_DATE"] = _wrkDate;
            string a = GetEquipmentWorkDate();
            dr["USERID"] = LoginInfo.USERNAME;
            dr["SPCL_NOTE"] = _spclNote;
            dr["NOTE_SEQNO"] = _noteSeqNo;
            dr["DEL_FLAG"] = "N";
            //dr["EQSG_ID"] = _eqsgId;

            dt.Rows.Add(dr);
            Util.GridSetData(dqNote, dt, null);

            wrkTime.SelectedDateTime = Convert.ToDateTime(_wrkDate);

            //(DateTime)System.DateTime.Now.AddDays(-7);
        }
        #endregion

        #region [BIZ에 넣을값 세팅]
        private static DataTable MakeDataTable(C1DataGrid dg)
        {
            DataTable dt = new DataTable();
            try
            {
                dt.Columns.Add("WRK_DATE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("SPCL_NOTE", typeof(string));
                dt.Columns.Add("NOTE_SEQNO", typeof(string));
                dt.Columns.Add("DEL_FLAG", typeof(string));
                //dt.Columns.Add("EQSG_ID", typeof(string));
                return dt;
            }
            catch (Exception)
            {
                return dt;
            }
        }
        #endregion

        #region [AREA ID에 따른 날짜 가져오기]
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

        #region [ 자동 닫기 ] 
        private void AsynchronousClose()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region [Loading 표시]
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
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2);
                //cboEquipment.SelectedIndex = 0;
                //_combo.SetCombo(EQUIPMENT, CommonCombo.ComboStatus.ALL, sFilter: sFilter2);
                //EQUIPMENT.SelectedIndex = 0;
            }
        }
    }
}
