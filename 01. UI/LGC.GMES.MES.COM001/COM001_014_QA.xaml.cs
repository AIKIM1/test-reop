/*************************************************************************************
  Created Date : 2022.08.17
  Creator : 정용석
  Decription : Loss 등록 이전 질문지 Popup
--------------------------------------------------------------------------------------
  [Change History]
  2022.08.17  정용석 : Initial Created.
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_014_QA : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private bool? totalSave = null;
        private DataTable dtQuestionAnswer = new DataTable();
        #endregion

        #region Constructor
        public COM001_014_QA()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties...
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public DataTable STANDARD_PROCESS_QUESTION_TABLE
        {
            get
            {
                return this.dtQuestionAnswer;
            }
            set
            {
                this.dtQuestionAnswer = value;
            }
        }

        public bool? TOTAL_SAVE
        {
            get
            {
                return this.totalSave;
            }
            set
            {
                this.totalSave = value;
            }
        }
        #endregion

        #region Member Function Lists...
        // Loaded Function
        private void Initialize()
        {
            try
            {
                DataTable dtQuestionList = this.GetCommonCodeInfo("STANDARD_PROCESS_QUESTION");
                if (!CommonVerify.HasTableRow(dtQuestionList))
                {
                    this.DialogResult = MessageBoxResult.None;
                    this.Close();
                    return;
                }

                dtQuestionList.Columns.Add("ANSWER", typeof(string));
                dtQuestionList.AcceptChanges();

                foreach (DataRow drQuestionList in dtQuestionList.Rows)
                {
                    drQuestionList["ANSWER"] = string.Empty;
                }
                dtQuestionList.AcceptChanges();

                this.SetDataGridComboBoxData();

                Util.GridSetData(this.dgQuestionList, dtQuestionList, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // DataGridComboBoxColumn Binding Data
        private void SetDataGridComboBoxData()
        {
            DataTable dtYesOrNo = this.GetCommonCodeInfo("YORN");
            if (!CommonVerify.HasTableRow(dtYesOrNo))
            {
                this.Close();
            }

            DataRow drYesOrNo = dtYesOrNo.NewRow();
            drYesOrNo["CBO_CODE"] = string.Empty;
            drYesOrNo["CBO_NAME"] = "-SELECT-";
            dtYesOrNo.Rows.InsertAt(drYesOrNo, 0);

            DataGridComboBoxColumn dataGridComboBoxColumn = (DataGridComboBoxColumn)this.dgQuestionList.Columns["ANSWER"];
            dataGridComboBoxColumn.SelectedValuePath = "CBO_CODE";
            dataGridComboBoxColumn.DisplayMemberPath = "CBO_NAME";
            dataGridComboBoxColumn.ItemsSource = dtYesOrNo.AsDataView();
        }

        // Save Process
        private void SaveProcess()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(this.dgQuestionList.ItemsSource);

                // 답변 선택 안한거 있으면 Interlock
                if (dt.AsEnumerable().Where(x => string.IsNullOrEmpty(x.Field<string>("ANSWER"))).Count() > 0)
                {
                    Util.MessageValidation("SFU8511");      // 답변을 선택해 주세요.
                    return;
                }

                this.dtQuestionAnswer = dt.Copy();
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 순서도 호출 - CommonCode 정보
        private DataTable GetCommonCodeInfo(string cmcdType)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["CBO_CODE"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }
        #endregion

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] obj = C1WindowExtension.GetParameters(this);
            if (obj != null && obj.Length >= 1)
            {
                this.totalSave = Convert.ToBoolean(obj[0]);
            }

            this.Initialize();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.SaveProcess();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.totalSave = null;
            this.DialogResult = MessageBoxResult.None;
            this.Close();
        }
        #endregion

    }
}