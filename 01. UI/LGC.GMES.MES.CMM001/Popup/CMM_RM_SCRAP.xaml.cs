/*************************************************************************************
 Created Date : 2024.05.28
      Creator : 신광희
   Decription : ROLLMAP DATA 수정 (Scrap 으로 발생한 이음매 데이터 수정) CMM_ROLLMAP_SCRAP Copy
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.28  : Initial Created.
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

    public partial class CMM_RM_SCRAP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public bool IsUpdated { get; set; }
        private string _lotId = string.Empty;
        private string _wipSeq = string.Empty;
        private string _equipmentMeasurementCode;
        private string _collectSeq;
        private string _startPosition;
        private string _endPosition;
        private string _scrapQty;

        public CMM_RM_SCRAP()
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
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitializeControl()
        {
            tbRewinderScrapPreviousWidth.Text = ObjectDic.Instance.GetObjectName("M 추가 제거");
            tbRewinderScrapAfterWidth.Text = ObjectDic.Instance.GetObjectName("M 추가 제거");
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)
            {
                _lotId = Util.NVC(parameters[0]);
                _wipSeq = Util.NVC(parameters[1]);
                _equipmentMeasurementCode = Util.NVC(parameters[2]);
                _collectSeq = Util.NVC(parameters[3]);
                _startPosition = Util.NVC(parameters[4]);
                _endPosition = Util.NVC(parameters[5]);
                _scrapQty = Util.NVC(parameters[6]);
                txtStartScrap.Value = _startPosition.GetDouble();
                txtEndScrap.Value = _endPosition.GetDouble();

                txtModifiedStartScrap.Value = _startPosition.GetDouble();
                txtModifiedEndScrap.Value = _endPosition.GetDouble();
            }

            InitializeControl();
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_RM_RPT_SCRAP_UI";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
                inTable.Columns.Add("CLCT_SEQNO", typeof(int));
                inTable.Columns.Add("ADJ_LOTID", typeof(string));
                inTable.Columns.Add("ADJ_WIPSEQ", typeof(decimal));
                inTable.Columns.Add("STRT_PSTN", typeof(decimal));
                inTable.Columns.Add("WND_LEN", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _lotId;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["EQPT_MEASR_PSTN_ID"] = _equipmentMeasurementCode;
                newRow["CLCT_SEQNO"] = _collectSeq;
                newRow["STRT_PSTN"] = txtPreviousPosition.Value;
                newRow["WND_LEN"] = txtAfterPosition.Value;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        IsUpdated = false;
                        return;
                    }
                    else
                    {
                        IsUpdated = true;
                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                });

            }
            catch (Exception ex)
            {
                IsUpdated = false;
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }

        private void txtPreviousPosition_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            C1NumericBox previousNumericBox = sender as C1NumericBox;
            if (previousNumericBox == null) return;

            if (e.Key == Key.Enter)
            {
                if(previousNumericBox.Value > txtStartScrap.Value)
                {
                    Util.MessageInfo("SFU8116", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPreviousPosition.Focus();
                        }
                    }, ObjectDic.Instance.GetObjectName("추가 Scrap 구간"));

                    txtPreviousPosition.Value = 0;
                    txtModifiedStartScrap.Value = txtStartScrap.Value - txtPreviousPosition.Value;
                    return;
                }
                else
                {
                    txtModifiedStartScrap.Value = txtStartScrap.Value - txtPreviousPosition.Value;
                }
                //이벤트가 라우팅 되지 않도록 함.
                e.Handled = true;
            }
        }

        private void txtAfterPosition_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1NumericBox afterNumericBox = sender as C1NumericBox;
            if (afterNumericBox == null) return;

            if (e.Key == Key.Enter)
            {
                txtModifiedEndScrap.Value = txtEndScrap.Value + afterNumericBox.Value;
                e.Handled = true;
            }
        }

        private void txtPreviousPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtPreviousPosition.Value > txtStartScrap.Value)
            {
                Util.MessageInfo("SFU8116", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtPreviousPosition.Focus();
                    }
                }, ObjectDic.Instance.GetObjectName("추가 Scrap 구간"));

                txtPreviousPosition.Value = 0;
                txtModifiedStartScrap.Value = txtStartScrap.Value - txtPreviousPosition.Value;
                return;
            }
            else
            {
                txtModifiedStartScrap.Value = txtStartScrap.Value - txtPreviousPosition.Value;
            }
        }

        private void txtAfterPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            txtModifiedEndScrap.Value = txtEndScrap.Value + txtAfterPosition.Value;
        }



        #endregion

        #region Mehod


        #endregion


    }
}
