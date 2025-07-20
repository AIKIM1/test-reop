/*************************************************************************************
 Created Date : 2022.11.15
      Creator : 신광희
   Decription : ROLLMAP 구간불량 병합 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2022.11.15  : Initial Created.
  2023.01.10  : 신광희 구간불량 병합 BizRule 파라메터 추가(SMPL_FLAG) 에 따른 팝업호출 파라메터 추가(_selectedSampleFlag) 적용
  2023.08.09  : 신광희 소형2170 코터맵 에서 구간불량 병합을 공통 사용할 수 있도록 수정 함.
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_ROLLMAP_TAGSECTION_MERGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _selectedLotId;
        private string _selectedWipSeq;
        private string _selectedStartPosition;
        private string _selectedEndPosition;
        private string _selectedprocessCode;
        private string _selectedEquipmentCode;
        private string _selectedRollMapCollectType;
        private string _selectedSampleFlag;
        private bool _isRollMapPatternApply = false;

        public bool IsUpdated { get; set; }

        public CMM_ROLLMAP_TAGSECTION_MERGE()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitializeCombo()
        {
            SetfaultyTypeCombo(cbofaultyType);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitializeControl()
        {
            InitializeCombo();
        }


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)
            {
                _selectedLotId = Util.NVC(parameters[0]);
                _selectedWipSeq = Util.NVC(parameters[1]);
                _selectedStartPosition = Util.NVC(parameters[2]);
                _selectedEndPosition = Util.NVC(parameters[3]);
                _selectedprocessCode = Util.NVC(parameters[4]);
                _selectedEquipmentCode = Util.NVC(parameters[5]);
                _selectedRollMapCollectType = Util.NVC(parameters[6]);
                _selectedSampleFlag = Util.NVC(parameters[7]);

                if (parameters.Length > 8)
                {
                    _isRollMapPatternApply = (bool)parameters[8];
                    Header = ObjectDic.Instance.GetObjectName("ROLLMAP 구간불량 병합") + " (" + ObjectDic.Instance.GetObjectName("PATTERN") + ")";
                }

                txtStartPosition.Value = _selectedStartPosition.GetDouble();
                txtEndPosition.Value = _selectedEndPosition.GetDouble();
                txtWidth.Value = _selectedEndPosition.GetDouble() - _selectedStartPosition.GetDouble();
            }

            InitializeControl();
            SelectProductQty();
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string bizRuleName = (_isRollMapPatternApply) ? "BR_PRD_REG_MERGE_TAG_SECTION_CY_RM" : "BR_PRD_REG_MERGE_TAG_SECTION_RM";

            DataTable inTable = new DataTable("INDATA");
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            if(_isRollMapPatternApply)
            {
                inTable.Columns.Add("STRT_PTN_PSTN", typeof(decimal));
                inTable.Columns.Add("END_PTN_PSTN", typeof(decimal));
            }
            else
            {
                inTable.Columns.Add("STRT_PSTN", typeof(decimal));
                inTable.Columns.Add("END_PSTN", typeof(decimal));
            }


            inTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));
            

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = _selectedLotId;
            newRow["WIPSEQ"] = _selectedWipSeq;

            if (_isRollMapPatternApply)
            {
                newRow["STRT_PTN_PSTN"] = _selectedStartPosition.GetDouble();
                newRow["END_PTN_PSTN"] = _selectedEndPosition.GetDouble();
            }
            else
            {
                newRow["STRT_PSTN"] = _selectedStartPosition.GetDouble();
                newRow["END_PSTN"] = _selectedEndPosition.GetDouble();
            }

            newRow["ROLLMAP_CLCT_TYPE"] = cbofaultyType.SelectedValue;
            newRow["USERID"] = LoginInfo.USERID;
            newRow["SMPL_FLAG"] = string.IsNullOrEmpty(_selectedSampleFlag) ? null : _selectedSampleFlag;
            inTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
            {
                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }
                IsUpdated = true;
                DialogResult = MessageBoxResult.OK;
            });

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        void SetfaultyTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_BADSECTION";
            string[] arrColumn = { "LANGID", "PROCID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, _selectedprocessCode, _selectedEquipmentCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, _selectedRollMapCollectType);

        }

        private void SelectProductQty()
        {
            //Top, Back 양품량, Foil 양
            //const string bizRuleName = "DA_PRD_SEL_ROLLMAP_PROD_QTY";
            string bizRuleName = (_isRollMapPatternApply) ? "DA_PRD_SEL_ROLLMAP_PROD_QTY_CY" : "DA_PRD_SEL_ROLLMAP_PROD_QTY";

            DataTable inTable = new DataTable();
            inTable.TableName = "RQSTDT";
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("CLCT_SEQNO", typeof(int));

            if (_isRollMapPatternApply)
            {
                inTable.Columns.Add("ADJ_STRT_PTN_PSTN", typeof(decimal));
                inTable.Columns.Add("ADJ_END_PTN_PSTN", typeof(decimal));
            }
            else
            {
                inTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
                inTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            }

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = _selectedEquipmentCode;
            newRow["LOTID"] = _selectedLotId;
            newRow["WIPSEQ"] = _selectedWipSeq;

            if (_isRollMapPatternApply)
            {
                newRow["ADJ_STRT_PTN_PSTN"] = txtStartPosition.Value;
                newRow["ADJ_END_PTN_PSTN"] = txtEndPosition.Value;
            }
            else
            {
                newRow["ADJ_STRT_PSTN"] = txtStartPosition.Value;
                newRow["ADJ_END_PSTN"] = txtEndPosition.Value;
            }

            inTable.Rows.Add(newRow);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                txtTopQty.Value = dtResult.Rows[0]["TOP_PROD_QTY"].GetDouble();
                txtBackQty.Value = dtResult.Rows[0]["BACK_PROD_QTY"].GetDouble();
                txtFoilQty.Value = dtResult.Rows[0]["FOIL_QTY"].GetDouble();
            }
            else
            {
                txtTopQty.Value = double.NaN;
                txtBackQty.Value = double.NaN;
                txtFoilQty.Value = double.NaN;
            }
        }


        #endregion


    }
}
