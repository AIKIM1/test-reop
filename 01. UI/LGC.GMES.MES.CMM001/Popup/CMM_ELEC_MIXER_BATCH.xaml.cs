using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Text;
using System.Windows;
using System;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_MIXER_BATCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_MIXER_BATCH : C1Window, IWorkArea
    {
        #region Initialize
        private string _MODLID = string.Empty;
        private string _VER_CODE = string.Empty;
        private string _CONFIRM_MSG = string.Empty;
        private bool _IS_LAST_BATCH = false;

        // 2019-08-28 오화백  LOTID, EQPTID 정보 파라미터 추가
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;

        public string _ConfirmMsg
        {
            get { return _CONFIRM_MSG; }
        }

        public bool _IsLastBatch
        {
            get { return _IS_LAST_BATCH;  }
        }

        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_MIXER_BATCH()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            _MODLID = Util.NVC(tmps[0]);
            _VER_CODE = Util.NVC(tmps[1]);
            // 2019-08-28 오화백  LOTID, EQPTID 정보 파라미터 추가
            _LOTID = Util.NVC(tmps[2]);
            _EQPTID = Util.NVC(tmps[3]);

            SetInitData();
            SetMixerInputData(_MODLID, _VER_CODE);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgMixerBatch.ItemsSource == null || dgMixerBatch.Rows.Count < 0)
                return;

            SetMixerInputData();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (dgMixerBatch.ItemsSource == null || dgMixerBatch.Rows.Count < 0)
            {
                Util.MessageValidation("SFU2052");  //입력된 항목이 없습니다.
                return;
            }

            StringBuilder strBuilder = new StringBuilder();
            DataTable dt = ((DataView)dgMixerBatch.ItemsSource).Table;

            foreach( DataRow row in dt.Rows)
            {
                if (string.IsNullOrEmpty(Util.NVC(row["CURR_BTCH"])) || string.IsNullOrEmpty(Util.NVC(row["TOT_BTCH"])))
                {
                    Util.MessageValidation("SFU3693");  //입력된 Batch 수가 0입니다.
                    return;
                }

                // 마지막 Batch인지 확인 하도록 추가 [2018-09-06]
                if (string.Equals(Util.NVC(row["CURR_BTCH"]).Trim(), Util.NVC(row["TOT_BTCH"]).Trim()) && _IS_LAST_BATCH == false)
                    _IS_LAST_BATCH = true;

                strBuilder.Append(Util.NVC(row["MODLID"]));
                strBuilder.Append(" ");
                strBuilder.Append(Util.NVC(row["VER_CODE"]));
                strBuilder.Append(" ");
                strBuilder.Append(Util.NVC(row["CURR_BTCH"]));
                strBuilder.Append(Util.NVC(row["TO_LF"]));
                strBuilder.Append(Util.NVC(row["TOT_BTCH"]));
                strBuilder.Append("\n");
            }

            if ( string.IsNullOrEmpty(strBuilder.ToString().Trim()))
            {
                Util.MessageValidation("SFU2052");  //입력된 항목이 없습니다.
                return;
            }
            SaveMixRptBatch(dt);
            _CONFIRM_MSG = strBuilder.ToString();
            this.DialogResult = MessageBoxResult.OK;
        }
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #region User Method
        private void SetInitData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MODLID");
            dt.Columns.Add("VER_CODE");
            dt.Columns.Add("CURR_BTCH");
            dt.Columns.Add("TO_LF");
            dt.Columns.Add("TOT_BTCH");

            dgMixerBatch.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void SetMixerInputData(string sModel = "", string sVerCode = "")
        {
            DataTable dt = ((DataView)dgMixerBatch.ItemsSource).Table;

            DataRow dr = dt.NewRow();
            dr["MODLID"] = sModel;
            dr["VER_CODE"] = sVerCode;
            dr["CURR_BTCH"] = "";
            dr["TO_LF"] = "/";
            dr["TOT_BTCH"] = "";
            dt.Rows.Add(dr);

            Util.GridSetData(dgMixerBatch, dt, FrameOperation, true);
            dgMixerBatch.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        /// <summary>
        /// 2019-08-28  오화백  : 배치정보 적용시 현배치/계획배치 정보를 MIX 작업일지 테이블에 저장 로직 추가
        /// </summary>
        public void SaveMixRptBatch(DataTable dt)
        {
            try
            {
               
                DataSet inData = new DataSet();
                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));  //소스Type
                inDataTable.Columns.Add("IFMODE", typeof(string));   //인터페이스 모드
                inDataTable.Columns.Add("EQPTID", typeof(string));   //설비
                inDataTable.Columns.Add("LOTID", typeof(string));   //배치ID
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _EQPTID;
                row["LOTID"] = _LOTID;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);
                //아이템 정보
                DataTable inItem = inData.Tables.Add("IN_ITEM");
                inItem.Columns.Add("RPT_ITEM", typeof(string));//작업일지 항목
                inItem.Columns.Add("MIX_SEQS", typeof(string)); //차수
                inItem.Columns.Add("RPT_ITEM_VALUE01", typeof(string));
              
                row = inItem.NewRow();
                row["RPT_ITEM"] = "ADD_INFO";
                row["MIX_SEQS"] = "BATCH_COUNT";
                row["RPT_ITEM_VALUE01"] = dt.Rows[0]["CURR_BTCH"].ToString() + "/" + dt.Rows[0]["TOT_BTCH"].ToString();

                inItem.Rows.Add(row);

                string bizRuleName = "BR_PRD_REG_MIXER_RPT_ITEM";

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_ITEM", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
          
        }


        #endregion
    }
}
