/*************************************************************************************
 Created Date : 2017.09.18
      Creator : 김동일K INS
   Decription : 노칭 전극 입고 확인
--------------------------------------------------------------------------------------
 [Change History]
  2017.09.18  김동일 : Initial Created.
   
**************************************************************************************/


using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_109.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_115 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        public COM001_115()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CHK", typeof(bool));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("WIPSNAME", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("PROCNAME", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQSGNAME", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("AREANAME", typeof(string));
                dt.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dt.Columns.Add("ELEC_TYPE_NAME", typeof(string));
                dt.Columns.Add("WIPQTY", typeof(decimal));
                dt.Columns.Add("PRJT_NAME", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("PRODNAME", typeof(string));
                dt.Columns.Add("MESSAGE", typeof(string));

                dgList.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                    btnSearch.RaiseEvent(newEventArgs);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgList);

                dgList.Columns["MESSAGE"].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMoveLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanMoveLine())
                    return;


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetLotInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null)
            //    {
            //        return;
            //    }

            //    //Grid Data Binding 이용한 Background 색 변경
            //    if (e.Cell.Row.Type == DataGridRowType.Item)
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            //        {
            //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //        else
            //        {
            //            e.Cell.Presenter.Background = null;
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
            //        }
            //    }
            //}));
        }

        private void dgList_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Row.Type == DataGridRowType.Item)
            //        {
            //            e.Cell.Presenter.Background = null;
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
            //        }
            //    }
            //}));
        }

        #endregion

        #region Mehod        

        #region [BizCall]
        private void GetLotInfo()
        {
            try
            {
                if (txtLotID.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU1190"); // 조회할 LOT ID 를 입력하세요.
                    return;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["LOTID"] = txtLotID.Text.Trim();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_SEL_RECEIVE_MTRL_CHECK_NT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        DataTable dtTemp = DataTableConverter.Convert(dgList.ItemsSource);
                        DataRow drTemp = dtTemp.NewRow();

                        if (searchException != null)
                        {
                            //Util.MessageException(searchException);
                            string sMsg = GetMessageException(searchException);

                            drTemp.ItemArray = new object[] { false, txtLotID.Text, "", "", "", "", "", "", "", "", "", "", "", 0, "", "", "", sMsg };
                            dtTemp.Rows.Add(drTemp);

                            dtTemp.AcceptChanges();
                            dgList.ItemsSource = DataTableConverter.Convert(dtTemp);

                            if (dgList.Columns["MESSAGE"].Visibility == Visibility.Collapsed)
                                dgList.Columns["MESSAGE"].Visibility = Visibility.Visible;

                            txtLotID.Text = "";

                            return;
                        }

                        if (searchResult.Rows.Count > 0)
                        {
                            drTemp.ItemArray = new object[] { false,
                                                              txtLotID.Text,
                                                              Util.NVC(searchResult.Rows[0]["WIPSTAT"]),
                                                              Util.NVC(searchResult.Rows[0]["WIPSNAME"]),
                                                              Util.NVC(searchResult.Rows[0]["PROCID"]),
                                                              Util.NVC(searchResult.Rows[0]["PROCNAME"]),
                                                              Util.NVC(searchResult.Rows[0]["EQSGID"]),
                                                              Util.NVC(searchResult.Rows[0]["EQSGNAME"]),
                                                              Util.NVC(searchResult.Rows[0]["EQPTID"]),
                                                              Util.NVC(searchResult.Rows[0]["AREAID"]),
                                                              Util.NVC(searchResult.Rows[0]["AREANAME"]),
                                                              Util.NVC(searchResult.Rows[0]["PRDT_CLSS_CODE"]),
                                                              Util.NVC(searchResult.Rows[0]["ELEC_TYPE_NAME"]),
                                                              decimal.Parse(Util.NVC(searchResult.Rows[0]["WIPQTY"])),
                                                              Util.NVC(searchResult.Rows[0]["PRJT_NAME"]),
                                                              Util.NVC(searchResult.Rows[0]["PRODID"]),
                                                              Util.NVC(searchResult.Rows[0]["PRODNAME"]),
                                                              ""};
                            dtTemp.Rows.Add(drTemp);

                            dtTemp.AcceptChanges();
                            dgList.ItemsSource = DataTableConverter.Convert(dtTemp);
                        }

                        txtLotID.Text = "";

                        txtLotID.Focus();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool CanMoveLine()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgList, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnInit);
            listAuth.Add(btnMoveLine);

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

        public string GetMessageException(Exception ex)
        {
            try
            {
                if (ex == null) return "";

                string sRet = "";

                if (ex.Data.Contains("TYPE"))
                {
                    string conversionLanguage;
                    string exceptionMessage = ex.Message;
                    string exceptionParameter = "";
                    if (ex.Data.Contains("PARA"))
                    {
                        exceptionParameter = ex.Data["PARA"].ToString();
                    }

                    // Code 로 다국어 처리..
                    if (ex.Data.Contains("DATA"))
                    {
                        if (exceptionParameter.Equals(""))
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]));
                        }
                        else
                        {
                            if (exceptionParameter.Contains(":"))
                            {
                                string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries);

                                conversionLanguage = MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]), parameterList);
                            }
                            else
                            {
                                conversionLanguage = MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]), exceptionParameter);
                            }
                        }
                    }
                    else
                    {
                        if (exceptionParameter.Contains(":"))
                        {
                            string sOrg = exceptionMessage;
                            string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries);

                            for (int i = parameterList.Length; i > 0; i--)
                            {
                                sOrg = sOrg.Replace(parameterList[i - 1], "%" + (i));
                            }

                            conversionLanguage = MessageDic.Instance.GetMessage(sOrg, parameterList);
                        }
                        else
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(exceptionMessage);
                        }
                    }
                    sRet = conversionLanguage;
                }

                return sRet;
            }
            catch (Exception exception)
            {
                Util.MessageException(exception);
                return "";
            }
        }
        #endregion

        #endregion

    }
}
