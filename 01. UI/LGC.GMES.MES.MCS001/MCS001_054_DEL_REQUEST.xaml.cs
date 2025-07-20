/*************************************************************************************
 Created Date : 2021.01.25
      Creator : 조영대
   Decription : 반송요청현황 및 이력 - 반송 요청 삭제
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.28  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_054_DEL_REQUEST : C1Window, IWorkArea
    {
        #region Declaration

        private DataRow drSelect;

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


        public MCS001_054_DEL_REQUEST()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameter = C1WindowExtension.GetParameters(this);
            if (parameter.Length == 1)
            {
                drSelect = parameter[0] as DataRow;
            }

            Initialize();
        }

        private void Initialize()
        {
            txtReqSeqno.Text = Util.NVC(drSelect["REQ_SEQNO"]);
            txtPrcsType.Text = Util.NVC(drSelect["PRCS_TYPE_NAME"]);
            txtReqType.Text = Util.NVC(drSelect["REQ_TYPE_NAME"]);
            txtEqpt.Text = Util.NVC(drSelect["EQPTNAME"]);
            txtPort.Text = Util.NVC(drSelect["PORT_ID"]);
            txtCstId.Text = Util.NVC(drSelect["CSTID"]);
            txtLotId.Text = Util.NVC(drSelect["LOTID"]);
            txtRuleId.Text = Util.NVC(drSelect["RTD_RULE_ID"]);
            txtFilterType.Text = Util.NVC(drSelect["FILTR_TYPE_NAME"]);
            txtSortType.Text = Util.NVC(drSelect["SORT_TYPE_NAME"]);
        }

        #endregion

        #region Event
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 삭제 하시겠습니까?
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null,
                    "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DeleteRequest();
                        }
                    });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Method

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

        private void DeleteRequest()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("REQ_SEQNO", typeof(decimal));
                inTable.Columns.Add("USER_ID", typeof(string));

                DataRow inRow = inTable.NewRow();
                inRow["REQ_SEQNO"] = Util.NVC_Decimal(drSelect["REQ_SEQNO"]);
                inRow["USER_ID"] = LoginInfo.USERID;
                inTable.Rows.Add(inRow);
                
                new ClientProxy().ExecuteService("BR_MHS_DEL_TRF_REQ_UI", "INDATA",  null, inTable, (result, ex) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        // 삭제되었습니다.
                        Util.MessageValidation("SFU1273");

                        DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex2)
                    {
                        Util.MessageException(ex2);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion


    }
}
