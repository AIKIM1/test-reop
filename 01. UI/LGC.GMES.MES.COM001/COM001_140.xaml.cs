/*************************************************************************************
 Created Date : 2021.10.01
      Creator : 
   Decription : Carrier 사용횟수 및 세정
--------------------------------------------------------------------------------------
 [Change History]
  2021.10.01  조영대 : Initial Created. 
  2024.06.05  김대현 : E20240520-000267  Carrier 세정 횟수 관리 
  2025.06.06  김선영 : 캐리어 세정 후 Cast Type  오류 수정
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_140 : UserControl, IWorkArea
    {


        #region Declaration & Constructor 
        
        public COM001_140()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        
        private void InitCombo()
        {            
            CommonCombo _combo = new CommonCombo();

            #region 사용현황 탭

            // 동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

            // Carrier 유형
            cboCarrierType.SetCommonCode("CSTTYPE", "ATTR1 = 'Y'", CommonCombo.ComboStatus.NONE, true);

            // 극성 유형
            cboElectrodeType.SetCommonCode("ELTR_TYPE_CODE", CommonCombo.ComboStatus.ALL);

            #endregion
            
            #region 세정 탭

            // 동
            _combo.SetCombo(cboAreaClean, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            #endregion


        }

        #endregion


        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitCombo();

            GetCleanEmptyList();

            this.Loaded -= UserControl_Loaded;
        }

        #region 사용 현황 탭

        private void txtCarrierID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SearchValidation())
                {
                    GetSearchList();
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchValidation())
            {
                GetSearchList();
            }
        }

        #endregion
        
        #region 세정 탭
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearControlClean();
        }

        private void txtCarrierIdClean_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CleanValidation())
            {
                GetCarrierInfo();
            }
        }

        private void btnSaveClean_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dgListClean.IsCheckedRow("CHK"))
                {
                    Util.MessageValidation("SFU1651"); // 선택된 항목이 없습니다.
                    return;
                }

                if (txtUserName.Text.Trim().Equals(string.Empty) || txtUserName.Tag == null)
                {
                    Util.MessageValidation("SFU4011"); // 담당자를 입력 하세요.
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"),
                    null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            DataTable inTable = new DataTable("IN_CARRIER");
                            inTable.Columns.Add("SRCTYPE", typeof(string));
                            inTable.Columns.Add("IFMODE", typeof(string));
                            inTable.Columns.Add("LANGID", typeof(string));
                            inTable.Columns.Add("CSTID", typeof(string));
                            inTable.Columns.Add("USERID", typeof(string));
                            inTable.Columns.Add("COMMENT", typeof(string));

                            DataTable dt = DataTableConverter.Convert(dgListClean.ItemsSource).Select("CHK = True").CopyToDataTable();

                            foreach (DataRow dr in dt.Rows)
                            {
                                DataRow newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = "UI";
                                newRow["IFMODE"] = string.Empty;
                                newRow["LANGID"] = LoginInfo.LANGID;
                                newRow["CSTID"] = dr["CSTID"];
                                newRow["USERID"] = Util.NVC(txtUserName.Tag);
                                newRow["COMMENT"] = txtComment.Text.Trim();
                                inTable.Rows.Add(newRow);
                            }

                            if (inTable.Rows.Count != 0)
                            {
                                DataSet ds = new DataSet();
                                ds.Tables.Add(inTable);
                                string xml = ds.GetXml();
                                
                                ShowLoadingIndicator();

                                new ClientProxy().ExecuteService("BR_PRD_REG_CARRIER_WASHING_CLEAR", "IN_CARRIER", null, inTable, (result, bizException) =>
                                {
                                    try
                                    {
                                        HiddenLoadingIndicator();

                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }
                                        
                                        DataTable dtClean = DataTableConverter.Convert(dgListClean.ItemsSource);
                                        dtClean.AsEnumerable().Where(s => s.Field<String>("CHK").Equals("True")).ToList().ForEach(r => r.Delete());     //2025.06.06  김선영 : 캐리어 세정 후 Cast Type  오류 수정

                                        Util.GridSetData(dgListClean, dtClean, FrameOperation, true);

                                        if (dgListClean.GetRowCount() == 0)
                                        {
                                            ClearControlClean();
                                        }

                                        Util.MessageInfo("SFU1275"); // 정상처리되었습니다.
                                    }
                                    catch (Exception ex)
                                    {
                                        HiddenLoadingIndicator();
                                        Util.MessageException(ex);
                                    }
                                });
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        
        private void lblStdTimesOver_MouseUp(object sender, MouseButtonEventArgs e)
        {
            chkStdTimesOver.IsChecked = !chkStdTimesOver.IsChecked;
        }
        #endregion

        #endregion


        #region Method

        #region 사용 현황 탭

        private bool SearchValidation()
        {
            if (cboArea.GetBindValue() == null)
            {
                // Carrier 관리 동을 선택해주세요
                Util.MessageValidation("SFU4925", lblArea.Text);
                return false;
            }

            if (cboCarrierType.GetBindValue() == null)
            {
                // Carrier 유형을 선택해주세요
                Util.MessageValidation("SFU4925", lblCarrierType.Text);
                return false;
            }

            return true;
        }

        private void GetSearchList()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CSTTYPE", typeof(string));
                dtRqst.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("STD_TIMES_OVER", typeof(string));
                dtRqst.Columns.Add("CARRIER_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.GetBindValue();
                dr["CSTID"] = txtCarrierID.Text.Trim().Equals(string.Empty) ? null : txtCarrierID.Text.Trim();
                dr["STD_TIMES_OVER"] = chkStdTimesOver.IsChecked.Equals(true) ? "Y" : null;

                string bizName = string.Empty;
                dr["CSTTYPE"] = cboCarrierType.GetBindValue();
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.GetBindValue();
                bizName = "DA_PRD_SEL_CARRIER_CLEANING_INFO";


                dtRqst.Rows.Add(dr);
                
                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, FrameOperation, true);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion
        
        #region 세정 탭
 

        private void GetCleanEmptyList()
        {
            try
            {
                if (dgListClean.ItemsSource != null) return;

                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CSTTYPE", typeof(string));
                dtRqst.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("STD_TIMES_OVER", typeof(string));
                dtRqst.Columns.Add("CARRIER_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = "EMPTY";
                dr["CSTTYPE"] = "EMPTY";
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                string bizName = string.Empty;
                bizName = "DA_PRD_SEL_CARRIER_CLEANING_INFO";

                new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgListClean, bizResult, FrameOperation, true);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CleanValidation()
        {
            if (cboArea.GetBindValue() == null)
            {
                // Carrier 관리 동을 선택해주세요
                Util.MessageValidation("SFU4925", lblArea.Text);
                return false;
            }

            if (txtCarrierIdClean.Text.Trim().Equals(string.Empty)) return false;

            return true;
        }

        private void GetCarrierInfo()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("CSTTYPE", typeof(string));
                dtRqst.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("STD_TIMES_OVER", typeof(string));
                dtRqst.Columns.Add("CARRIER_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaClean.GetBindValue();
                dr["CARRIER_ID"] = txtCarrierIdClean.Text.Trim();
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                string bizName = string.Empty;
                bizName = "DA_PRD_SEL_CARRIER_CLEANING_INFO";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizName, "RQSTDT", "RSLTDT", dtRqst);
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU8237"); // 등록되지 않은 CARRIER ID입니다.
                    return;
                }

                if (!Util.NVC(dtResult.Rows[0]["CSTSTAT"]).Equals("E"))
                {
                    Util.MessageValidation("SFU4928", txtCarrierIdClean.Text.Trim()); // %1은 Empty 상태인 캐리어가 아닙니다.
                    return;
                }

                DataTable dtClean = DataTableConverter.Convert(dgListClean.ItemsSource);
                if (dtClean.AsEnumerable().Where(s => s.Field<string>("CSTID").Equals(txtCarrierIdClean.Text.Trim())).Count() > 0)
                {
                    Util.MessageValidation("SFU3471", txtCarrierIdClean.Text.Trim()); // [%1]은 이미 등록되었습니다.
                    return;
                }

                // MES 2.0 ItemArray 위치 오류 Patch
                //dtClean.Rows.Add(dtResult.Rows[0].ItemArray);
                dtClean.AddDataRow(dtResult.Rows[0]);

                Util.GridSetData(dgListClean, dtClean, FrameOperation, true);

                txtCarrierIdClean.Text = string.Empty;
                txtCarrierIdClean.Focus();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;
            }
            else
            {
                txtUserName.Text = string.Empty;
                txtUserName.Tag = null;
            }
        }

        private void ClearControlClean()
        {
            txtCarrierIdClean.Text = string.Empty;

            dgListClean.ClearRows();

            txtUserName.Text = string.Empty;
            txtUserName.Tag = string.Empty;
            txtComment.Text = string.Empty;
        }

        #endregion

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

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


    }
}
