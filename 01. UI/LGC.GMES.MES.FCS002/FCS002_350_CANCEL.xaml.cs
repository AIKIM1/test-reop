/*************************************************************************************
 Created Date : 2017.11.17
      Creator : 이슬아
   Decription : 전지 5MEGA-GMES 구축 - 포장출고 - 출고취소 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// BOX001_027_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class FCS002_350_CANCEL : C1Window, IWorkArea
    {
        Util _Util = new Util();
        string _Shift;
        string _User;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_350_CANCEL()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitControl();
        }
        
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            ////listAuth.Add(btnOutDel);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _User = Util.NVC(tmps[0]);
            _Shift = Util.NVC(tmps[1]);

            if (string.IsNullOrEmpty(_User))
            {
                // SFU1843 작업자를 입력 해주세요.
                Util.MessageValidation("SFU1843");
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
                return ;
            }

            if (string.IsNullOrEmpty(_Shift))
            {
                // SFU1844 작업조를 입력 해주세요.
                Util.MessageValidation("SFU1844");
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
                return;
            }
            txtPalletID.Focus();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dgPalletID.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU3425"); //선택된 Pallet가 없습니다.
                return;
            }

            //취소 하시겠습니까?
            Util.MessageConfirm("SFU1243", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelShip();
                }
            });
        }

        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {     
                //CancelShip();
                ChkCancelShip();
            }
        }

        private void ChkCancelShip()
        {

            loadingIndicator.Visibility = Visibility.Visible;

            string strPalletID = txtPalletID.Text.Trim();

            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INBOX");
            inDataTable.Columns.Add("BOXID");
            inDataTable.Columns.Add("LANGID");

            DataRow inDataRow = inDataTable.NewRow();
            inDataRow["BOXID"] = strPalletID;
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(inDataRow);

            new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_CANCEL_SHIP_FM", "INBOX", null, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable dtTo = DataTableConverter.Convert(dgPalletID.ItemsSource);
                    if (dtTo == null || dtTo.Rows.Count <= 0)
                    {
                        DataSet dtDataSet = new DataSet();
                        dtTo = dtDataSet.Tables.Add("PALLET");
                        dtTo.Columns.Add("CHK", typeof(bool));
                        dtTo.Columns.Add("PALLETID", typeof(string));
                    }

                    DataRow dr = dtTo.NewRow();
                    dr["CHK"] = false;
                    dr["PALLETID"] = strPalletID;
                    dtTo.Rows.Add(dr);
                    dgPalletID.ItemsSource = DataTableConverter.Convert(dtTo);

                    txtPalletID.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }, indataSet);
        }

        private void CancelShip()
        {
            //if (string.IsNullOrWhiteSpace(txtPalletID.Text))
            //{
            //    // SFU3350  입력오류 : PALLETID 를 입력해 주세요.	
            //    Util.MessageValidation("SFU3350");
            //    return;
            //}

            loadingIndicator.Visibility = Visibility.Visible;

            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID");
            inDataTable.Columns.Add("SHFT_ID");
            inDataTable.Columns.Add("WRK_SUPPLIERID");
            inDataTable.Columns.Add("LANGID");
            inDataTable.Columns.Add("NOTE");

            DataRow inDataRow = inDataTable.NewRow();
            inDataRow["USERID"] = _User;
            inDataRow["SHFT_ID"] = _Shift;
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataRow["NOTE"] = string.Empty;
            inDataTable.Rows.Add(inDataRow);

            DataTable inBoxTable = indataSet.Tables.Add("INBOX");
            inBoxTable.Columns.Add("BOXID");

            for (int inx = 0; inx < dgPalletID.GetRowCount();inx++)
            {
                DataRow newRow = inBoxTable.NewRow();
                newRow["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletID.Rows[inx].DataItem, "PALLETID"));
                inBoxTable.Rows.Add(newRow);
            }
          
            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SHIP_INPALLET_FM", "INDATA,INBOX", null, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }, indataSet);

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtTo = DataTableConverter.Convert(dgPalletID.ItemsSource);
            if (dtTo.Rows.Count > 0)
            {
                DataRow[] drInfo = dtTo.Select("CHK = True");
                foreach (DataRow dr in drInfo) { 
                    dtTo.Rows.Remove(dr);
                    dgPalletID.ItemsSource = DataTableConverter.Convert(dtTo);
                }
            }
        }

    }
}
