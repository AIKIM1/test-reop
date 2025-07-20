/*************************************************************************************
 Created Date : 2017.09.20
      Creator : 오화백
   Decription : 재튜빙 LOT 생성
--------------------------------------------------------------------------------------
 [Change History]
  2017.09.20  오화백 : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Text.RegularExpressions;
namespace LGC.GMES.MES.COM001
{
    public partial class COM001_112 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        public COM001_112()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }

        #endregion

        #region Initialize


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        private void btnReSet_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput);

            txtRetubingProdId.Text = string.Empty;

        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search(true);
        }
        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletId.Text.Trim();
                    if (dgListInput.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListInput.SelectedIndex = i;
                                dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                txtPalletId.Focus();
                                txtPalletId.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    Search(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRetubing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //재튜빙LOT 생성하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4195"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Retubing_Create();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_INPALLET_FOR_SHIP_FM
        /// </summary>
        private void Search(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
               
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.GetCondition(txtPalletId, "LOTID를 입력하세요"); // LOTID를 입력하세요.
                if (dr["LOTID"].Equals("")) return;
                dtRqst.Rows.Add(dr);

                string sServicName = "";
                if(rdoPallet.IsChecked == true)
                {
                    sServicName = "DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST";
                }
                else
                {
                    sServicName = "BR_PRD_GET_ASSY_LOT_FOR_RETUBE_TRAY";
                }

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST", "INDATA", "OUTDATA", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sServicName, "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletId.Focus();
                            txtPalletId.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgListInput.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListInput);
                        txtRetubingProdId.Text = string.Empty;
                        Util.GridSetData(dgListInput, dtSource, FrameOperation, true);
                        DataTableConverter.SetValue(dgListInput.Rows[dgListInput.Rows.Count - 1].DataItem, "CHK", 1);
                        txtPalletId.Text = string.Empty;
                        txtPalletId.Focus();
                    }
                    else
                    {
                         Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    }
                }
                else
                {
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private bool Validation()
        {
            if (txtRetubingLot.Text == string.Empty)
            {
                Util.MessageValidation("SFU4196"); //재튜빙LOT을 입력하세요.
                return false;
            }

            if (dgListInput.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다..
                return false;
            }

            DataRow[] drUpdate = DataTableConverter.Convert(dgListInput.ItemsSource).Select("CHK = 1");

            if (drUpdate.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (rdoTray.IsChecked == true) //Tray 일 경우 하나의 LOT 만 선택 가능
            {
                if (drUpdate.Length > 1 )
                {
                    // 1개만 선택할 수 있습니다.
                    Util.MessageValidation("SFU4912");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtRetubingProdId.Text))
                {
                    // 변경 제품ID가 입력되지 않았습니다.
                    Util.MessageValidation("SFU4036");
                    return false;
                }
            }
            

            if (txtRetubingLot.Text.Trim().Length != 8)
            {
                Util.MessageValidation("SFU4197"); //재튜빙LOTID는 8자리 입니다.
                return false;
            }

            Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]{1,10}$");
            Boolean ismatch = regex.IsMatch(txtRetubingLot.Text);
            if (!ismatch)
            {
                Util.MessageValidation("SFU3674"); // 숫자와 영문대문자만 입력가능합니다.
                return false;
            }
            return true;
        }


        private void Retubing_Create()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string)); // 재튜빙LOTID
            inDataTable.Columns.Add("RETUBE_PRODID", typeof(string)); // 재튜빙PRODID
            inDataTable.Columns.Add("USERID", typeof(string)); //UserID

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            row["IFMODE"] = IFMODE.IFMODE_OFF;
            row["LOTID"] =  txtRetubingLot.Text.Trim().ToString(); // 재튜빙LOTID
            if (rdoTray.IsChecked == true)
            {
                row["RETUBE_PRODID"] = txtRetubingProdId.Text.Trim().ToString().ToUpper();
            }
            row["USERID"] = LoginInfo.USERID; //사용자ID
            inDataTable.Rows.Add(row);

            //LOT 정보
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("PALLETID", typeof(string));
            inLot.Columns.Add("PRODID", typeof(string)); //제품ID
            inLot.Columns.Add("ASSY_LOTID", typeof(string)); //조립LOTID
            if (rdoTray.IsChecked == true)
            {
                inLot.Columns.Add("CST_GNRT_DTTM", typeof(string)); //트레이 활성화 완료 시간
            }
            

            for (int i = 0; i < dgListInput.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inLot.NewRow();
                    row["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "PALLETID"));
                    row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "PRODID"));
                    row["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "LOTID_RT"));

                    if (rdoTray.IsChecked == true)
                    {
                        row["CST_GNRT_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "CST_GNRT_DTTM"));
                    }

                    inLot.Rows.Add(row);
                }
            }

            try
            {
                string sServicName = "";
                if (rdoPallet.IsChecked == true)
                {
                    sServicName = "BR_PRD_REG_RT_LOT";
                }
                else
                {
                    sServicName = "BR_PRD_REG_RT_LOT_FOR_TRAY";
                }

                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi(sServicName, "INDATA,INLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    Util.gridClear(dgListInput);
                    txtRetubingProdId.Text = string.Empty;
                    txtRetubingLot.Text = string.Empty;
                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_RT_LOT", ex.Message, ex.ToString());

            }
        }



        #endregion

        private void rdoPallet_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput);
            if(txtRetubingProdId != null)
            {
                txtRetubingProdId.Text = string.Empty;
                txtRetubingProdId.IsEnabled = false;
            }
        }

        private void rdoTray_Checked(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput);

            if (txtRetubingProdId != null)
            {
                txtRetubingProdId.Text = string.Empty;
                txtRetubingProdId.IsEnabled = true;
            }
        }

        private void dgListInput_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (rdoTray.IsChecked == true && dgListInput.CurrentColumn.Name == "CHK" && Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[dgListInput.SelectedIndex].DataItem, "CHK")) == "1")
                {
                    txtRetubingProdId.Text = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[dgListInput.SelectedIndex].DataItem, "PRODID"));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }
    }
}
