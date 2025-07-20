/*************************************************************************************
 Created Date : 2017.06.02
      Creator : 두잇 이선규K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - 자재투입 처리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.02 두잇 이선규K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_007_INPUT_OBJECT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_007_INPUT_OBJECT : C1Window, IWorkArea
    {        
        #region Declaration & Constructor         
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _ProdLotID = string.Empty;
        private string _OutLotID = string.Empty;
        private DataTable _CurrIn = null;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_007_INPUT_OBJECT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _EqptID = Util.NVC(tmps[0]);
                _ProcID = Util.NVC(tmps[1]);
                _ProdLotID = Util.NVC(tmps[2]);
                _OutLotID = Util.NVC(tmps[3]);
                _CurrIn = tmps[4] == null ? null : (DataTable)(tmps[4]);
            }
            else
            {
                _EqptID = string.Empty;
                _ProcID = string.Empty;
                _ProdLotID = string.Empty;
                _OutLotID = string.Empty;
                _CurrIn = null;
            }

            ApplyPermissions();
            GetInputObject();

            if (_CurrIn != null && _CurrIn.Rows != null && _CurrIn.Rows.Count > 0)
            {
                if (_ProcID == Process.PACKAGING)
                    txtTopMessage.Text = MessageDic.Instance.GetMessage("SFU1266", Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[0].DataItem, "EQPT_MOUNT_PSTN_NAME"))); // %1에 투입완료되지 않은 바구니가 존재합니다
                else if (_ProcID == Process.STP)
                    txtTopMessage.Text = MessageDic.Instance.GetMessage("100132", Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[0].DataItem, "EQPT_MOUNT_PSTN_NAME"))); // 투입완료되지 않은 매거진이 존재합니다.
            }
            else
            {
                txtTopMessage.Text = string.Empty;
            }                
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (!CanExecute())
                return;

            InputObjectProcess();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetInputObject()
        {
            try
            {
                ShowLoadingIndicator();

                //Util.GridSetData(dgInputObject, _CurrIn, null, true);
                dgInputObject.ItemsSource = DataTableConverter.Convert(_CurrIn);
                chkAll.IsChecked = true;
                checkAll_Checked(null, null);
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

        private void InputObjectProcess()
        {
            try
            {
                ShowLoadingIndicator();

                if (_ProcID == Process.PACKAGING)
                {
                    bool allComplete = true;
                    //DataSet indataSet = _Biz.GetBR_PRD_REG_END_INPUT_IN_LOT_STP();
                    //GetBR_PRD_REG_IN_COMPLETE_CL
                    DataSet indataSet = _Biz.GetBR_PRD_REG_IN_COMPLETE_CL();
                    DataRow newRow = null;

                    DataTable inTable = indataSet.Tables["INDATA"];
                    newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EqptID;
                    newRow["PROCID"] = _ProcID;
                    newRow["PROD_LOTID"] = _ProdLotID;
                    newRow["OUT_LOTID"] = _OutLotID;
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);

                    DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                    for (int i = 0; i < dgInputObject.Rows.Count - dgInputObject.BottomRows.Count; i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgInputObject, "CHK", i))
                        {
                            allComplete = false;
                            continue;
                        }

                        if (!Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                        {
                            newRow = inInputTable.NewRow();
                            newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "INPUT_LOTID"));
                            newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                            newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                            newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "MTRLID"));
                            newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "INPUT_QTY")));

                            inInputTable.Rows.Add(newRow);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPUT_LOT_CL", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            this.DialogResult = allComplete ? MessageBoxResult.OK : MessageBoxResult.Cancel;
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    }, indataSet
                    );
                }
                else if (_ProcID == Process.STP)
                {
                    bool allComplete = true;
                    DataSet indataSet = _Biz.GetBR_PRD_REG_IN_COMPLETE_CL();
                    DataRow newRow = null;

                    DataTable inTable = indataSet.Tables["INDATA"];
                    newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EqptID;
                    newRow["PROCID"] = _ProcID;
                    newRow["PROD_LOTID"] = _ProdLotID;
                    newRow["OUT_LOTID"] = _OutLotID;
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);

                    DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                    for (int i = 0; i < dgInputObject.Rows.Count - dgInputObject.BottomRows.Count; i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgInputObject, "CHK", i))
                        {
                            allComplete = false;
                            continue;
                        }

                        if (!Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                        {
                            newRow = inInputTable.NewRow();
                            newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "INPUT_LOTID"));
                            newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                            newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                            newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "MTRLID"));
                            newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgInputObject.Rows[i].DataItem, "INPUT_QTY")));

                            inInputTable.Rows.Add(newRow);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            this.DialogResult = allComplete ? MessageBoxResult.OK : MessageBoxResult.Cancel;
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    }, indataSet
                    );
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]
        private bool CanExecute()
        {
            bool bRet = false;

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgInputObject, "CHK");
            if (iRow < 0)
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
            listAuth.Add(btnExecute);

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

        #endregion

        #region Grid All CheckBox
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

        private void dgInputObject_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        private void dgInputObject_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
            {
                CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);

                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                            if (!chk.IsChecked.Value)
                            {
                                chkAll.IsChecked = false;
                            }
                            else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                            {
                                chkAll.IsChecked = true;
                            }

                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            break;
                    }
                }
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgInputObject == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgInputObject.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = true;
            }
            dgInputObject.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgInputObject == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgInputObject.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = false;
            }
            dgInputObject.ItemsSource = DataTableConverter.Convert(dt);
        }
        #endregion
    }
}
