/*************************************************************************************
 Created Date : 2017.12.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 설비 작업조건 등록 팝업 (신규 모델링 대응)
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.22  INS 김동일K : Initial Created.
  
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_PU_EQPT_COND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_PU_EQPT_COND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _EqptName = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _ProcID = string.Empty;
        private string _LotID2 = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_PU_EQPT_COND()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
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

                if (tmps != null && tmps.Length >= 6)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                    _LotID = Util.NVC(tmps[3]);
                    _WipSeq = Util.NVC(tmps[4]);
                    _EqptName = Util.NVC(tmps[5]);

                    if (tmps.Length >= 7)
                        _LotID2 = Util.NVC(tmps[6]);
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _ProcID = "";
                    _LotID = "";
                    _WipSeq = "";
                    _EqptName = "";
                    _LotID2 = "";
                }

                ApplyPermissions();


                txtLotID.Text = _LotID2.Equals("") ? _LotID : _LotID + ", " + _LotID2;
                txtEqptID.Text = _EqptID;
                txtEqptName.Text = _EqptName;

                GetEqptCondInfo(GetMaxLotInfoEqptCond());

                if (_ProcID.Equals(Process.PACKAGING))
                {
                    if (dgEqptCond.Columns.Contains("UNIT_EQPTNAME"))
                        dgEqptCond.Columns["UNIT_EQPTNAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgEqptCond.Columns.Contains("UNIT_EQPTNAME"))
                        dgEqptCond.Columns["UNIT_EQPTNAME"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            //저장하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetEqptCond();
                }
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgEqptCond_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqptCond_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("INPUT_VALUE"))
                    {
                        if (dataGrid.Columns.Contains("DATA_TYPE"))
                        {
                            string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DATA_TYPE"));
                            if (sDataType.Equals("A"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            else if (sDataType.Equals("B"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgEqptCond_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        #endregion

        #region Mehod

        #region [BizCall]        
        private string GetMaxLotInfoEqptCond()
        {
            try
            {
                //ShowLoadingIndicator();

                string sMaxLot = "";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SEL_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _ProcID;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["SEL_LOTID"] = _LotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_PROC_EQPT_PRDT_SET_ITEM_BF_LOTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("BEFORE_LOTID"))
                {
                    sMaxLot = Util.NVC(dtRslt.Rows[0]["BEFORE_LOTID"]);
                }

                return sMaxLot;
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                //Util.MessageException(ex);
                return "";
            }

        }

        private void GetEqptCondInfo(string sBeforeLot)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_PROC_EQPT_PRDT_SET_ITEM_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["EQSGID"] = _LineID;
                newRow["BEFORE_LOTID"] = sBeforeLot.Equals("") ? "TEMP_LOT" : sBeforeLot;   // NULL 또는 공백으로 BIZ 호출 시 TIMEOUT 으로 인해 임의 값 설정.

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_EQP_SEL_PROC_EQPT_SET_ITEM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgEqptCond.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgEqptCond, searchResult, null, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
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

        private void SetEqptCond()
        {
            try
            {
                ShowLoadingIndicator();

                dgEqptCond.EndEdit();

                if (_ProcID.Equals(Process.PACKAGING))
                {
                    DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                    DataTable inTable = indataSet.Tables["IN_EQP"];

                    DataRow newRow = null;

                    //DataRow newRow = inTable.NewRow();
                    //newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    //newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    //newRow["EQPTID"] = _EqptID;
                    //newRow["UNIT_EQPTID"] = _EqptID;
                    //newRow["USERID"] = LoginInfo.USERID;
                    //newRow["LOTID"] = _LotID;

                    //inTable.Rows.Add(newRow);

                    DataTable in_Data = indataSet.Tables["IN_DATA"];

                    // Biz Core Multi 처리 없으므로 임시로 Unit 단위로 비즈 호출 처리 함.
                    // 추후 Multi Biz 생성 시 처리 방법 변경 필요.
                    string sUnitID = "";
                    for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                    {
                        string sTmp = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "UNIT_EQPTID"));

                        if (i == 0)
                        {
                            sUnitID = sTmp;

                            newRow = null;

                            newRow = inTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            newRow["EQPTID"] = _EqptID;
                            newRow["UNIT_EQPTID"] = sUnitID;
                            newRow["USERID"] = LoginInfo.USERID;
                            newRow["LOTID"] = _LotID;
                            newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                            inTable.Rows.Add(newRow);

                            newRow = null;

                            newRow = in_Data.NewRow();
                            newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                            newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                            in_Data.Rows.Add(newRow);
                        }
                        else
                        {
                            if (sUnitID.Equals(sTmp))
                            {
                                newRow = null;

                                newRow = in_Data.NewRow();
                                newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                                newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                                in_Data.Rows.Add(newRow);
                            }
                            else
                            {
                                // data 존재 시 biz call
                                if (inTable.Rows.Count > 0 && in_Data.Rows.Count > 0)
                                {
                                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                                    inTable.Rows.Clear();
                                    in_Data.Rows.Clear();

                                    //Util.AlertInfo("SFU1275");      //정상 처리 되었습니다.

                                    //GetEqptCondInfo();
                                }

                                sUnitID = sTmp;

                                newRow = null;

                                newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                newRow["EQPTID"] = _EqptID;
                                newRow["UNIT_EQPTID"] = sUnitID;
                                newRow["USERID"] = LoginInfo.USERID;
                                newRow["LOTID"] = _LotID;
                                newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                                inTable.Rows.Add(newRow);

                                newRow = null;

                                newRow = in_Data.NewRow();
                                newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                                newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                                in_Data.Rows.Add(newRow);
                            }
                        }
                    }

                    // 마지막 Unit 처리.
                    if (inTable.Rows.Count > 0 && in_Data.Rows.Count > 0)
                    {
                        new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                        inTable.Rows.Clear();
                        in_Data.Rows.Clear();

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        //GetEqptCondInfo();
                    }
                }
                else if (_ProcID.Equals(Process.NOTCHING))
                {
                    if (_LotID2.Equals(""))
                    {
                        DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                        DataTable inTable = indataSet.Tables["IN_EQP"];

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _EqptID;
                        //newRow["UNIT_EQPTID"] = _EqptID;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["LOTID"] = _LotID;
                        newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                        inTable.Rows.Add(newRow);

                        DataTable in_Data = indataSet.Tables["IN_DATA"];

                        for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                        {
                            newRow = null;

                            newRow = in_Data.NewRow();
                            newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                            newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                            in_Data.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                        inTable.Rows.Clear();
                        in_Data.Rows.Clear();

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
                    }
                    else
                    {
                        // 2 건 저장
                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        //inDataTable.Columns.Add("SUBLOTID", typeof(string));
                        inDataTable.Columns.Add("INPUT_SEQ_NO", typeof(int));
                        //inDataTable.Columns.Add("EVENT_NAME", typeof(string));

                        DataTable in_DATA = indataSet.Tables.Add("IN_DATA");
                        in_DATA.Columns.Add("LOTID", typeof(string));
                        in_DATA.Columns.Add("CLCTITEM", typeof(string));
                        in_DATA.Columns.Add("CLCTITEM_VALUE01", typeof(string));


                        DataTable inTable = indataSet.Tables["IN_EQP"];

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _EqptID;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                        inTable.Rows.Add(newRow);

                        DataTable in_Data = indataSet.Tables["IN_DATA"];

                        for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                        {
                            newRow = null;

                            newRow = in_Data.NewRow();
                            newRow["LOTID"] = _LotID;
                            newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                            newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                            in_Data.Rows.Add(newRow);

                            newRow = null;

                            newRow = in_Data.NewRow();
                            newRow["LOTID"] = _LotID2;
                            newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                            newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                            in_Data.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT_S_NT", "IN_EQP,IN_DATA", null, indataSet);

                        inTable.Rows.Clear();
                        in_Data.Rows.Clear();

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                    }
                }
                else
                {
                    DataSet indataSet = _Biz.GetBR_QCA_REG_EQPT_DATA_CLCT();
                    DataTable inTable = indataSet.Tables["IN_EQP"];

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _EqptID;
                    //newRow["UNIT_EQPTID"] = _EqptID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = _LotID;
                    newRow["INPUT_SEQ_NO"] = 1; // BIZ 내부 사용용 FIX 처리.

                    inTable.Rows.Add(newRow);

                    DataTable in_Data = indataSet.Tables["IN_DATA"];

                    for (int i = 0; i < dgEqptCond.Rows.Count - dgEqptCond.BottomRows.Count; i++)
                    {
                        newRow = null;

                        newRow = in_Data.NewRow();
                        newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "CLCTITEM"));
                        newRow["CLCTITEM_VALUE01"] = Util.NVC(DataTableConverter.GetValue(dgEqptCond.Rows[i].DataItem, "INPUT_VALUE"));

                        in_Data.Rows.Add(newRow);
                    }

                    new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_EQPT_DATA_CLCT", "IN_EQP,IN_DATA", null, indataSet);

                    inTable.Rows.Clear();
                    in_Data.Rows.Clear();

                    Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                    //GetEqptCondInfo();
                }

                GetEqptCondInfo("");
                //btnSave.IsEnabled = false;
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

        #endregion

        #region [Validation]
        private bool CanSave()
        {
            bool bRet = false;

            if (dgEqptCond.ItemsSource == null || dgEqptCond.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1651");      //선택된 항목이 없습니다.
                return bRet;
            }

            if (_LotID.Trim().Length < 1)
            {
                Util.MessageValidation("SFU1195");      //Lot 정보가 없습니다.
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

        #endregion
    }
}
