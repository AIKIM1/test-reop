/*********************************************************************************************************************************
 Created Date : 2022.03.13
      Creator : 유명환
   Decription : 자재LOT 잔량재공 생성
-----------------------------------------------------------------------------------------------------------------------------------
 [Change History]
-----------------------------------------------------------------------------------------------------------------------------------
       DATE            CSR NO            DEVELOPER            DESCRIPTION
-----------------------------------------------------------------------------------------------------------------------------------
  2024.03.13                               유명환           Initial Created.
  2024.08.27                               유재홍           업체 LOTID 컬럼 추가
 
***********************************************************************************************************************************/


using System;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_CANCEL_TERM_SEPA : C1Window, IWorkArea
    {
        #region Declaration
        private CommonCombo combo = new CommonCombo();
        private Util _Util = new Util();

        private string CSTStatus = string.Empty;
        private string _ProcID = string.Empty;
        private string _ProcName = string.Empty;
        private string _EqptID = string.Empty;
        private string _EqptName = string.Empty;
        

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        #endregion

        public CMM_ASSY_CANCEL_TERM_SEPA()
        {
            InitializeComponent();
        }
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcName = Util.NVC(tmps[2]);
                    _EqptName = Util.NVC(tmps[3]);
                }
                else
                {
                    _ProcID = "";
                    _ProcName = "";
                    _EqptID = "";
                    _EqptName = "";
                }

                ApplyPermissions();
                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }





        #region [ Event ] - dtpDateFrom

        #endregion

        #region [ Event ] - Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {


            SearchData();
        }
        #endregion

        #region [ Method ] - Search
        private void InitCombo()
        {
            txtEquipment.Text = _EqptName;
            txtMatlLossProcess.Text = _ProcName;


        }

        private void init()
        {
            txtUserName.Text = "";
            txtReason.Text = "";
            Util.gridClear(dgSearch);
        }


        private void SearchData()
        {


            try
            {


                init();
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("EQPTID", typeof(string));
                dtINDATA.Columns.Add("MLOTID", typeof(string));

                DataRow Indata = dtINDATA.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = _ProcID;
                Indata["EQPTID"] = _EqptID;
                Indata["MLOTID"] = txtMLotID.Text;


                dtINDATA.Rows.Add(Indata);

                string bizRule = "BR_PRD_SEL_MLOT_WIP_REMAIN_CREATE_ACTID_DETL";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        return;
                    }

                    Util.GridSetData(dgSearch, dtResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion





        #region [ Util ]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region [동] - 조회 조건
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            init();
        }
        #endregion

        #region [라인] - 조회 조건

        #endregion

        #region [공정] - 조회 조건

        #endregion

        #region [설비] - 조회 조건
        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            init();

        }
        #endregion

        private void txtMLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {


                SearchData();
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgSearch, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }
            if (string.IsNullOrEmpty(Util.NVC(txtReason.Text)))
            {
                // 사유를 입력 하세요.
                Util.MessageValidation("SFU1594");
                return;
            }
            if (string.IsNullOrEmpty(Util.NVC(txtUserName.Text)) || string.IsNullOrEmpty(Util.NVC(txtUserName.Tag)))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return;
            }
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    saveRemainLot();
                }
            });
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //CheckBox cb = sender as CheckBox;
                //if (cb?.DataContext == null) return;
                //if (cb.IsChecked == null) return;

                //DataTable dtTo = DataTableConverter.Convert(dgSearchList.ItemsSource);

                //if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                //{
                //    dtTo.Columns.Add("CHK", typeof(Boolean));
                //    dtTo.Columns.Add("PLLT_ID", typeof(string));
                //    dtTo.Columns.Add("SUPPLIER_LOTID", typeof(string));
                //    dtTo.Columns.Add("MTRLID", typeof(string));
                //    dtTo.Columns.Add("MTRLNAME", typeof(string));
                //}

                //if (dtTo.Select("PLLT_ID = '" + DataTableConverter.GetValue(cb.DataContext, "PLLT_ID") + "' AND " +
                //                "SUPPLIER_LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "SUPPLIER_LOTID") + "'").Length > 0) //중복조건 체크
                //{
                //    return;
                //}

                //DataRow dr = dtTo.NewRow();
                //foreach (DataColumn dc in dtTo.Columns)
                //{
                //    if (dc.DataType.Equals(typeof(Boolean)))
                //    {
                //        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                //    }
                //    else
                //    {
                //        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                //    }
                //}

                //dtTo.Rows.Add(dr);
                //dgSearchList.ItemsSource = DataTableConverter.Convert(dtTo);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                //DataTable dtTo = DataTableConverter.Convert(dgSearchList.ItemsSource);
                //DataRow[] dr = dtTo.Select();

                //if (dr.Length > 0)
                //{
                //    dtTo.Rows.Remove(dr[0]);
                //}
                //dgSearchList.ItemsSource = DataTableConverter.Convert(dtTo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void saveRemainLot()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inMLot = indataSet.Tables.Add("IN_LOT");
                inMLot.Columns.Add("MLOTID", typeof(string));
                inMLot.Columns.Add("INPUTSEQNO", typeof(string));

                for (int i = 0; i < dgSearch.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgSearch, "CHK", i)) continue;

                    DataRow newMLotRow = inMLot.NewRow();
                    newMLotRow["MLOTID"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "MLOTID"));
                    newMLotRow["INPUTSEQNO"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "INPUTSEQNO"));

                    inMLot.Rows.Add(newMLotRow);
                }

                DataTable inRequestUser = indataSet.Tables.Add("IN_REQUEST_USER");
                inRequestUser.Columns.Add("RUSERID", typeof(string));
                inRequestUser.Columns.Add("RUSERNAME", typeof(string));
                inRequestUser.Columns.Add("RESON", typeof(string));

                DataRow newRequestUserRow = inRequestUser.NewRow();
                newRequestUserRow["RUSERID"] = Util.NVC(txtUserName.Tag);
                newRequestUserRow["RUSERNAME"] = Util.NVC(txtUserName.Text);
                newRequestUserRow["RESON"] = Util.NVC(txtReason.Text);

                inRequestUser.Rows.Add(newRequestUserRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REMAIN_CREATE_LOT", "IN_LOT,IN_REQUEST_USER", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SearchData();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { }
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
        }
    }
}

