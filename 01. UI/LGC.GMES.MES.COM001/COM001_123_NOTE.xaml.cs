/*************************************************************************************
 Created Date : 2017.11.28 
      Creator : INS 염규범S
   Decription : 전지 5MEGA-GMES 구축 - 정보 조회 화면 - 생산 일별 특이사항
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.28  INS 염규범S : Initial Created.
  2021.04.07  안인효      : C20210315-000429 (160161) 설비세그먼트 비고 항목 코드 (EQSG_NOTE_ITEM_CODE) 추가
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
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_123_NOTE : C1Window, IWorkArea
    {
        #region [ 함수 정의 ]
        private string _noteSeqNo = string.Empty;
        private string _eqsgNote = string.Empty;
        private string _eqsgId = string.Empty;
        private string _wrkDate = string.Empty;
        private string _regUserId = string.Empty;
        private string _equipmentSegmentClassCode = string.Empty;
        private string _equipmentSegmentClassCodeItem = string.Empty;

        private bool bLoad = false;

        #endregion

        #region Initialize        
        public COM001_123_NOTE()
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
            _eqsgNote = Util.NVC(tmps[1]);
            _eqsgId = Util.NVC(tmps[2]);
            _wrkDate = Util.NVC(tmps[3]);
            _equipmentSegmentClassCode = Util.NVC(tmps[4]);
            _equipmentSegmentClassCodeItem = Util.NVC(tmps[5]);

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

            DataTable dt = DataTableConverter.Convert(cboEquipmentSegment.ItemsSource);
            if (CommonVerify.HasTableRow(dt) && !string.IsNullOrEmpty(_eqsgId))
            {
                var query = (from t in dt.AsEnumerable()
                    where t.Field<string>("CBO_CODE") == _eqsgId
                    select t).FirstOrDefault();

                if(query != null) cboEquipmentSegment.SelectedValue = _eqsgId;
            }

            //SetEquipmentSegmentClassCodeCombo(cboEquipmentSegmentClassCode);
            //SetEquipmentSegmentItemClassCodeCombo(cboEquipmentSegmentClassCodeItem);
            SetEquipmentSegmentNoteCombo(cboEquipmentSegmentClassCode, "EQSG_NOTE_CLSS_CODE", _equipmentSegmentClassCode);
            SetEquipmentSegmentNoteCombo(cboEquipmentSegmentClassCodeItem, "EQSG_NOTE_ITEM_CODE", _equipmentSegmentClassCodeItem);

        }
        #endregion

        #region [BIZ에 값 넣기]
        private void saveNote() 
        {
            string EQSGID = string.Empty;
             

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";

            RQSTDT.Columns.Add("EQSGID", typeof(String));
            RQSTDT.Columns.Add("SPCL_NOTE", typeof(String));
            RQSTDT.Columns.Add("DEL_FLAG", typeof(String ));
            RQSTDT.Columns.Add("USER_ID", typeof(String));
            RQSTDT.Columns.Add("NOTE_SEQNO", typeof(String));
            RQSTDT.Columns.Add("WRK_DATE", typeof(String));
            RQSTDT.Columns.Add("EQSG_NOTE_CLSS_CODE", typeof(string));
            RQSTDT.Columns.Add("EQSG_NOTE_ITEM_CODE", typeof(string));
            DataRow dr = RQSTDT.NewRow();

            dr["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
            dr["SPCL_NOTE"] = DataTableConverter.GetValue(dqNote.Rows[0].DataItem, "SPCL_NOTE");
            dr["DEL_FLAG"] = 'N';
            dr["USER_ID"] = LoginInfo.USERID;
            dr["WRK_DATE"] = wrkTime.SelectedDateTime.ToString("yyyyMMdd");
            //dr["WRK_DATE"] = DataTableConverter.GetValue(dqNote.Rows[0].DataItem, "WRK_DATE").GetString().Replace("-", "");
            dr["NOTE_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dqNote.Rows[0].DataItem, "NOTE_SEQNO"));
            dr["EQSG_NOTE_CLSS_CODE"] = cboEquipmentSegmentClassCode.SelectedValue.GetString() == "SELECT" ? string.Empty : cboEquipmentSegmentClassCode.SelectedValue;
            dr["EQSG_NOTE_ITEM_CODE"] = cboEquipmentSegmentClassCodeItem.SelectedValue.GetString() == "SELECT" ? string.Empty : cboEquipmentSegmentClassCodeItem.SelectedValue;
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
            dr["SPCL_NOTE"] = _eqsgNote;
            dr["NOTE_SEQNO"] = _noteSeqNo;
            dr["DEL_FLAG"] = "N";
            dr["EQSG_NOTE_CLSS_CODE"] = _equipmentSegmentClassCode;
            dr["EQSG_NOTE_ITEM_CODE"] = _equipmentSegmentClassCodeItem;
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
                dt.Columns.Add("SPCL_NOTE", typeof(string));
                dt.Columns.Add("NOTE_SEQNO", typeof(string));
                dt.Columns.Add("DEL_FLAG", typeof(string));
                dt.Columns.Add("EQSG_NOTE_CLSS_CODE", typeof(string));
                dt.Columns.Add("EQSG_NOTE_ITEM_CODE", typeof(string));
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

        private void SetEquipmentSegmentClassCodeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "EQSG_NOTE_CLSS_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, _equipmentSegmentClassCode);
        }

        private void SetEquipmentSegmentNoteCombo(C1ComboBox cbo, string strCode, string strNoteCode)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, strCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, strNoteCode);
        }

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


    }
}
