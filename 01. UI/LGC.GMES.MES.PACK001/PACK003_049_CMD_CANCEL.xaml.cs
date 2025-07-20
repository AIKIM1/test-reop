/*************************************************************************************
 Created Date : 2024.09.11
      Creator : 오화백
   Decription :  반송지시 취소
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.11  오화백 : Initial Created. 
  2025.06.25  김홍기 : Pack 영역 화면 분리
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_049_CMD_CANCEL : C1Window, IWorkArea
    {
        #region Declaration

        private DataRow drSelect1;
        private DataRow drSelect2;

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


        public PACK003_049_CMD_CANCEL()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameter = C1WindowExtension.GetParameters(this);
            if (parameter.Length == 2)
            {
                drSelect1 = parameter[0] as DataRow;
                drSelect2 = parameter[1] as DataRow;
            }

            Initialize();
        }

        private void Initialize()
        {
            lblMessage.Text = MessageDic.Instance.GetMessage("SFU8325");
            
            lblLoadingOrder.Text = Util.NVC(drSelect1["CST_LOAD_LOCATION_CODE"]);
            lblCarrierId.Text = Util.NVC(drSelect1["CSTID"]);
            lblLotId.Text = Util.NVC(drSelect1["LOTID"]);

            if (drSelect2 != null)
            {
                lblLoadingOrder2.Text = Util.NVC(drSelect2["CST_LOAD_LOCATION_CODE"]);
                lblCarrierId2.Text = Util.NVC(drSelect2["CSTID"]);
                lblLotId2.Text = Util.NVC(drSelect2["LOTID"]);
            }

            lblFromEqpt.Text = Util.NVC(drSelect1["FROM_EQPTNAME"]);
            lblFromPort.Text = Util.NVC(drSelect1["FROM_PORTNAME"]);
            lblToEqpt.Text = Util.NVC(drSelect1["TO_EQPTNAME"]);
            lblToPort.Text = Util.NVC(drSelect1["TO_PORTNAME"]);
        }

        #endregion

        #region Event
        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // %1 (을)를 하시겠습니까?
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4329", ObjectDic.Instance.GetObjectName("CANCEL_RTN_ORDER")), null,
                    "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            OrderCancel();
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

        private void OrderCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                DataRow inRow = inTable.NewRow();
                inRow["CARRIERID"] = lblCarrierId.Text;
                inRow["UPDUSER"] = LoginInfo.USERID;
                inTable.Rows.Add(inRow);

               // new ClientProxy().ExecuteService("BR_MHS_REG_TRF_CMD_CANCEL_BY_UI", "IN_REQ_TRF_INFO", null, inTable, (result, ex) =>
               new ClientProxy().ExecuteService("BR_MHS_REG_CANCEL_CMD", "IN_REQ_TRF_INFO", null, inTable, (result, ex) =>
               {
                   try
                   {
                       HiddenLoadingIndicator();

                       if (ex != null)
                       {
                           Util.MessageException(ex);
                           return;
                       }

                       // 취소되었습니다.
                       Util.MessageValidation("SFU1937");

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
