/*************************************************************************************
 Created Date : 2017.01.11
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 설비 작업조건 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.28  INS 김동일K : Initial Created.
  2023.03.13  비즈테크아이 - 노기상 : C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

//C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
using LGC.GMES.MES.Common.Mvvm;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ComponentModel;
using System.Collections;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_EQPT_COND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_EQPT_COND : C1Window, IWorkArea
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

        string _holdTrgtCode = string.Empty;
        #endregion

        #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시  : 
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시  :
        public HoldViewModel Vm { get; }
        #endregion

        public CMM_ASSY_EQPT_COND()
        {  
            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시  :
            Vm = new HoldViewModel();
            DataContext = Vm;
            #endregion

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

                    if (tmps.Length >= 7) { 
                        _LotID2 = Util.NVC(tmps[6]);
                    }

                    _holdTrgtCode = "SETTINNG_VALUE";
                    Vm.Items = new ObservableCollectionHold<HoldModel>(dgEqptCond, _holdTrgtCode);
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
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;


                string strINPUT_VALUE = string.Empty;
                string strCLCTITEM_USL_VALUE = string.Empty;
                string strCLCTITEM_LSL_VALUE = string.Empty;

                decimal decINPUT_VALUE = 0;
                decimal decCLCTITEM_USL_VALUE = 0;
                decimal decCLCTITEM_LSL_VALUE = 0;

                bool isINPUT_VALUE = false;
                bool isCLCTITEM_USL_VALUE = false;
                bool isCLCTITEM_LSL_VALUE = false;

                strINPUT_VALUE = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["INPUT_VALUE"].Index)?.Value);
                isINPUT_VALUE = decimal.TryParse(strINPUT_VALUE, out decINPUT_VALUE);

                strCLCTITEM_USL_VALUE = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["CLCTITEM_USL_VALUE"].Index)?.Value);
                isCLCTITEM_USL_VALUE = decimal.TryParse(strCLCTITEM_USL_VALUE, out decCLCTITEM_USL_VALUE);

                strCLCTITEM_LSL_VALUE = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["CLCTITEM_LSL_VALUE"].Index)?.Value);
                isCLCTITEM_LSL_VALUE = decimal.TryParse(strCLCTITEM_LSL_VALUE, out decCLCTITEM_LSL_VALUE);

                if (Util.NVC(e?.Cell?.Column?.Name) == "INPUT_VALUE")
                {
                    dataGrid.EndEdit();
                    dataGrid.EndEditRow(true);
                    if (isINPUT_VALUE == true && isCLCTITEM_USL_VALUE == true && isCLCTITEM_LSL_VALUE == true)
                    {
                        HoldModel item = (HoldModel)e.Cell.Row.DataItem;

                        if (decINPUT_VALUE < decCLCTITEM_LSL_VALUE)
                        {
                            //입력값이 하한값보다 작습니다
                            Util.MessageInfo("SFU1806");

                            StringBuilder err = new StringBuilder();
                            switch (_holdTrgtCode)
                            {
                                case "SETTINNG_VALUE":
                                    err.Append(item["INPUT_VALUE"]);
                                    break;
                            }
                        }

                        if (decINPUT_VALUE > decCLCTITEM_USL_VALUE)
                        {
                            //입력값이 상한 값 보다 큽니다
                            Util.MessageInfo("SFU1805");

                            StringBuilder err = new StringBuilder();
                            switch (_holdTrgtCode)
                            {
                                case "SETTINNG_VALUE":
                                    err.Append(item["INPUT_VALUE"]);
                                    break;
                            }
                        }

                        if (decINPUT_VALUE > decCLCTITEM_LSL_VALUE && decINPUT_VALUE < decCLCTITEM_USL_VALUE)
                        {

                            //dataGrid.Rows[e.Cell.Row.Index].Errors.Remove(item);

                            foreach (var erritem in dataGrid.Rows[e.Cell.Row.Index].Errors.ToList())
                            {
                                if (erritem.ColumnNames.Contains("INPUT_VALUE"))
                                {
                                    dataGrid.Rows[e.Cell.Row.Index].Errors.Remove(erritem);
                                }
                            }
                        }


                        //if (e.Cell.Row.DataItem != null)
                        //{
                        //    //HoldModel item = (HoldModel)e.Cell.Row.DataItem;
                        //    item.INPUT_VALUE = decINPUT_VALUE.ToString();
                        //}
                    }

                    dataGrid.UpdateLayout();

                }

                if (Util.NVC(e?.Cell?.Column?.Name) == "CLCTITEM_USL_VALUE")
                {
                    if (isCLCTITEM_USL_VALUE == true && isCLCTITEM_LSL_VALUE == true)
                    {
                        if (decCLCTITEM_USL_VALUE < decCLCTITEM_LSL_VALUE)
                        {
                            //입력값이 하한값보다 작습니다
                            Util.MessageInfo("SFU1806");
                        }
                    }
                }

                if (Util.NVC(e?.Cell?.Column?.Name) == "CLCTITEM_LSL_VALUE")
                {
                    if (isCLCTITEM_USL_VALUE == true && isCLCTITEM_LSL_VALUE == true)
                    {
                        if (decCLCTITEM_USL_VALUE < decCLCTITEM_LSL_VALUE)
                        {
                            //하한값이 상한값 보다 클 수 없습니다.
                            Util.MessageInfo("FM_ME0252");
                        }
                    }
                }

                
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



        private void dgEqptCond_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
        }

        private void dgEqptCond_(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void dgEqptCond_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
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

                new ClientProxy().ExecuteService("DA_EQP_SEL_PROC_EQPT_PRDT_SET_ITEM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgEqptCond.ItemsSource = DataTableConverter.Convert(searchResult);
                        #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
                        //--Util.GridSetData(dgEqptCond, searchResult, null, false);
                        //GridSetData<HoldModel>(dgEqptCond, searchResult, FrameOperation);


                        ObservableCollectionHold<HoldModel> _items = new ObservableCollectionHold<HoldModel>();
                        #region 주석처리 - 속도개선 20230322 - C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시 
                        //decimal decINPUT_VALUE = 0;
                        //decimal decCLCTITEM_USL_VALUE = 0;
                        //decimal decCLCTITEM_LSL_VALUE = 0;
                        //bool isINPUT_VALUE = false;
                        //bool isCLCTITEM_USL_VALU = false;
                        //bool isCLCTITEM_LSL_VALUE = false;

                        //Style sRed = new Style(typeof(C1.WPF.DataGrid.DataGridCellPresenter));
                        //////sRed.Setters.Add(new Setter(C1.WPF.DataGrid.DataGridCellPresenter.BackgroundProperty, Brushes.Red));
                        //sRed.Setters.Add(new Setter(C1.WPF.DataGrid.DataGridCellPresenter.ForegroundProperty, Brushes.Red));


                        //Style sNormal = new Style(typeof(C1.WPF.DataGrid.DataGridCellPresenter));
                        //////sNormal.Setters.Add(new Setter(C1.WPF.DataGrid.DataGridCellPresenter.BackgroundProperty, Brushes.White));
                        //sNormal.Setters.Add(new Setter(C1.WPF.DataGrid.DataGridCellPresenter.ForegroundProperty, Brushes.Black));
                        #endregion 주석처리 - 속도개선 20230322 - C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
                        if (searchResult !=null && searchResult.Rows.Count > 0)
                        {
 
                            for (int i= 0; i < searchResult.Rows.Count; i++)
                            {

                                HoldModel holdmodel = new HoldModel();

                                #region 주석처리 - 속도개선 20230322 - C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
                                //isINPUT_VALUE = decimal.TryParse(searchResult.Rows[i]["INPUT_VALUE"].ToString(), out decINPUT_VALUE);
                                //isCLCTITEM_USL_VALU = decimal.TryParse(searchResult.Rows[i]["CLCTITEM_USL_VALUE"].ToString(), out decCLCTITEM_USL_VALUE);
                                //isCLCTITEM_LSL_VALUE = decimal.TryParse(searchResult.Rows[i]["CLCTITEM_LSL_VALUE"].ToString(), out decCLCTITEM_LSL_VALUE);
                                #endregion 주석처리 - 속도개선 20230322 - C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
                                holdmodel.UNIT_EQPTID = searchResult.Rows[i]["UNIT_EQPTID"].ToString();
                                holdmodel.UNIT_EQPTNAME = searchResult.Rows[i]["UNIT_EQPTNAME"].ToString();
                                holdmodel.CLCTITEM = searchResult.Rows[i]["CLCTITEM"].ToString();
                                holdmodel.CLCTNAME = searchResult.Rows[i]["CLCTNAME"].ToString();
                                holdmodel.CLCTUNIT = searchResult.Rows[i]["CLCTUNIT"].ToString();
                                holdmodel.CLCTITEM_USL_VALUE = searchResult.Rows[i]["CLCTITEM_USL_VALUE"].ToString();
                                holdmodel.CLCTITEM_LSL_VALUE = searchResult.Rows[i]["CLCTITEM_LSL_VALUE"].ToString();
                                holdmodel.INPUT_VALUE = searchResult.Rows[i]["INPUT_VALUE"].ToString();
                                holdmodel.DATA_TYPE = searchResult.Rows[i]["DATA_TYPE"].ToString();
                                _items.Add(holdmodel);
                                Vm.Items.Add(holdmodel);
                            }

                            GridSetData<HoldModel>(dgEqptCond, _items, FrameOperation);
                        }
                        #endregion

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
                    #region [C20210615-000524] Added by kimgwango on 2121.12.16
                    foreach (HoldModel item in Vm.Items)
                    {
                        StringBuilder err = new StringBuilder();

                        switch (_holdTrgtCode)
                        {
                            case "SETTINNG_VALUE":
                                err.Append(item["INPUT_VALUE"]);
                                break;
                        }

                        if (!string.IsNullOrEmpty(err.ToString()))
                        {
                            //데이터 확인이 필요합니다.(변경함: <==최대 [%1]까지 등록 가능 합니다.)
                            Util.MessageValidation("SFU8216");
                            try
                            {
                                dgEqptCond.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

                                // Filter 적용 시 Exception 발생
                                int rowIdx = dgEqptCond.Rows.IndexOf(item);
                                dgEqptCond.ScrollIntoView(rowIdx, 0);
                            }
                            catch (Exception ex)
                            {
                            }

                            return;
                        }
                    }
                    #endregion

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


        #region [ UIHelper ]
        public void GridSetData<T>(C1.WPF.DataGrid.C1DataGrid dataGrid, DataTable dt, IFrameOperation iFO, bool isAutoWidth = false) where T : new()
        {
            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시 
            //gridClear(dataGrid);

            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            #endregion

            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
            ObservableCollectionHold<T> _items = (dataGrid.ItemsSource as ObservableCollectionHold<T>);
            _items.Clear();

            DataTableHoldHelper.CopyToObservableCollectionFromTable(dt, _items);
            #endregion

            dataGrid.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

            if (dt.Rows.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(dt.Rows.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && dt.Rows.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    { 
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        public void GridSetData<T>(C1.WPF.DataGrid.C1DataGrid dataGrid, ObservableCollection<T> oc, IFrameOperation iFO, bool isAutoWidth = false) where T : new()
        {
            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시 
            //gridClear(dataGrid);

            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            #endregion

            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
            ObservableCollectionHold<T> _items = (dataGrid.ItemsSource as ObservableCollectionHold<T>);
            _items.Clear();

            if (oc.Count > 0)
            {
                foreach (var item in oc)
                    _items.Add(item);
            }
            #endregion

            dataGrid.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

            if (oc.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(oc.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && oc.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    { 
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        private void DataGridLoaded(object sender, RoutedEventArgs args)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
            double sumHeight = dataGrid.ActualHeight;

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

            dataGrid.UpdateLayout();
            dataGrid.Measure(new Size(sumWidth, sumHeight));

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                if (dgc.ActualWidth > 0)
                    dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);
        }
        #endregion
    }


    #region // C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
    #region [ Model ] - HoldModel
    public class HoldViewModel : BindableBase
    {
        private ObservableCollectionHold<HoldModel> _items;
        public ObservableCollectionHold<HoldModel> Items
        {

            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public C1DelegateCommand SelectAllCommand { get; set; }

        public HoldViewModel()
        {
            Items = new ObservableCollectionHold<HoldModel>();

            SelectAllCommand = new C1DelegateCommand(new Action<object>(p =>
            {
                CheckBox chk = (CheckBox)p;

                // [C20210615-000524] Added by kimgwango on 2021.12.17 --<
                foreach (var item in Items)
                    item.CHK = (bool)chk.IsChecked;
                // [C20210615-000524] Added by kimgwango on 2021.12.17 >--
            }));
        }

        public HoldViewModel(C1DataGrid dataGrid, string holdTrgtCode)
        {
            Items = new ObservableCollectionHold<HoldModel>(dataGrid, holdTrgtCode);
        }
    }

    public class ObservableCollectionHold<T> : ObservableCollection<T>
    {
        private C1DataGrid _dataGrid;
        public C1DataGrid DataGrid
        {
            get { return _dataGrid; }
            set { _dataGrid = value; }
        }

        private string _hold_trgt_code;
        public string HOLD_TRGT_CODE
        {
            get { return _hold_trgt_code; }
            set { _hold_trgt_code = value; }
        }

        private string _INPUT_VALUE;
        public string INPUT_VALUE
        {
            get { return _INPUT_VALUE; }
            set { _INPUT_VALUE = value; }
        }

        private string _CLCTITEM_USL_VALUE;
        public string CLCTITEM_USL_VALUE
        {
            get { return _CLCTITEM_USL_VALUE; }
            set { _CLCTITEM_USL_VALUE = value; }
        }

        private string _CLCTITEM_LSL_VALUE;
        public string CLCTITEM_LSL_VALUE
        {
            get { return _CLCTITEM_LSL_VALUE; }
            set { _CLCTITEM_LSL_VALUE = value; }
        }


        private string _DATA_TYPE;
        public string DATA_TYPE
        {
            get { return _DATA_TYPE; }
            set { _DATA_TYPE = value; }
        }


        private string _UNIT_EQPTID;
        public string UNIT_EQPTID
        {
            get { return _UNIT_EQPTID; }
            set { _UNIT_EQPTID = value; }
        }

        private string _UNIT_EQPTNAME;
        public string UNIT_EQPTNAME
        {
            get { return _UNIT_EQPTNAME; }
            set { _UNIT_EQPTNAME = value; }
        }
        private string _CLCTITEM;
        public string CLCTITEM
        {
            get { return _CLCTITEM; }
            set { _CLCTITEM = value; }
        }
        private string _CLCTUNIT;
        public string CLCTUNIT
        {
            get { return _CLCTUNIT; }
            set { _CLCTUNIT = value; }
        }

        private bool _chk;
        public bool CHK
        {
            get { return _chk; }
            set { _chk = value; }
        }

        public new void Add(T item)
        {
            PropertyInfo p;

            #region Essential Properties

            // find the property for the column
            p = item.GetType().GetProperty("DataGrid");

            // if exists, set the value
            if (p != null)
            {
                p.SetValue(item, this.DataGrid, null);
            }

            // find the property for the column
            p = item.GetType().GetProperty("HOLD_TRGT_CODE");

            // if exists, set the value
            if (p != null && string.IsNullOrEmpty(p.GetValue(item)?.ToString()))
            {
                p.SetValue(item, this.HOLD_TRGT_CODE, null);
            }

            // find the property for the column
            p = item.GetType().GetProperty("INPUT_VALUE");

            // if exists, set the value
            if (p != null && string.IsNullOrEmpty(p.GetValue(item)?.ToString()))
            {
                p.SetValue(item, this.INPUT_VALUE, null);
            }

            // find the property for the column
            p = item.GetType().GetProperty("CLCTITEM_USL_VALUE");

            // if exists, set the value
            if (p != null && string.IsNullOrEmpty(p.GetValue(item)?.ToString()))
            {
                p.SetValue(item, this.CLCTITEM_USL_VALUE, null);
            }

            // find the property for the column
            p = item.GetType().GetProperty("CLCTITEM_LSL_VALUE");

            // if exists, set the value
            if (p != null && string.IsNullOrEmpty(p.GetValue(item)?.ToString()))
            {
                p.SetValue(item, this.CLCTITEM_LSL_VALUE, null);
            }

            // find the property for the column
            p = item.GetType().GetProperty("DATA_TYPE");

            // if exists, set the value
            if (p != null && string.IsNullOrEmpty(p.GetValue(item)?.ToString()))
            {
                p.SetValue(item, this.DATA_TYPE, null);
            }

            #endregion

            // find the property for the column
            p = item.GetType().GetProperty("CHK");

            // if exists, set the value
            if (p != null && this.CHK && !(bool)p.GetValue(item))
            {
                //p.SetValue(item, this.CHK, null);
                p.SetValue(item, true, null);
            }

            base.Add(item);
        }

        public ObservableCollectionHold() : base()
        {
        }

        public ObservableCollectionHold(string holdTrgtCode)
        {
            HOLD_TRGT_CODE = holdTrgtCode;
        }

        public ObservableCollectionHold(C1DataGrid dataGrid, string holdTrgtCode)
        {
            DataGrid = dataGrid;
            HOLD_TRGT_CODE = holdTrgtCode;
        }
    }

    public class HoldModel : BindableBase, IDataErrorInfo
    {
        #region properties for common
        private C1DataGrid _dataGrid;
        public C1DataGrid DataGrid
        {
            get { return _dataGrid; }
            set { _dataGrid = value; }
        }

        private bool _chk;
        public bool CHK
        {

            get { return _chk; }
            set { SetProperty(ref _chk, value); }
        }

        private string _hold_trgt_code;
        public string HOLD_TRGT_CODE
        {
            get { return _hold_trgt_code; }
            set { SetProperty(ref _hold_trgt_code, value); }
        }
        #endregion

        #region properties fo DB
        private string _UNIT_EQPTID;
        public string UNIT_EQPTID
        {

            get { return _UNIT_EQPTID; }
            set { SetProperty(ref _UNIT_EQPTID, value); }

        }

        private string _UNIT_EQPTNAME;
        public string UNIT_EQPTNAME
        {

            get { return _UNIT_EQPTNAME; }
            set { SetProperty(ref _UNIT_EQPTNAME, value); }

        }

        private string _CLCTITEM;
        public string CLCTITEM
        {

            get { return _CLCTITEM; }
            set { SetProperty(ref _CLCTITEM, value); }

        }

        private string _CLCTNAME;
        public string CLCTNAME
        {

            get { return _CLCTNAME; }
            set { SetProperty(ref _CLCTNAME, value); }

        }

        private string _CLCTUNIT;
        public string CLCTUNIT
        {

            get { return _CLCTUNIT; }
            set { SetProperty(ref _CLCTUNIT, value); }

        }

        private string _CLCTITEM_USL_VALUE;
        public string CLCTITEM_USL_VALUE
        {

            get { return _CLCTITEM_USL_VALUE; }
            set { SetProperty(ref _CLCTITEM_USL_VALUE, value); }

        }

        private string _CLCTITEM_LSL_VALUE;
        public string CLCTITEM_LSL_VALUE
        {

            get { return _CLCTITEM_LSL_VALUE; }
            set { SetProperty(ref _CLCTITEM_LSL_VALUE, value); }

        }

        private string _INPUT_VALUE;
        public string INPUT_VALUE
        {

            get { return _INPUT_VALUE; }
            set { SetProperty(ref _INPUT_VALUE, value); }

        }

        private string _DATA_TYPE;
        public string DATA_TYPE
        {

            get { return _DATA_TYPE; }
            set { SetProperty(ref _DATA_TYPE, value); }

        }

        #endregion

        #region IDataErrorInfo
        private string _error;
        public string Error
        {
            get
            {
                // perform item-level validation: Price must be > Spec -Low Value
                //if (Low Value < Setting Value)
                //{
                //    return string.Format("Setting Value must be > Setting Value ({0:c2})", _INPUT_VALUE);
                //}

                return _error;
            }
            set { SetProperty(ref _error, value); }
        }

        public string this[string columnName]
        {
            get
            {
                StringBuilder error = new StringBuilder();
                StringBuilder errorRow = new StringBuilder();
                string rowName = string.Empty;

                #region // C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
                decimal decINPUT_VALUE = 0;
                decimal decCLCTITEM_USL_VALUE = 0;
                decimal decCLCTITEM_LSL_VALUE = 0;
                bool isINPUT_VALUE = false;
                bool isCLCTITEM_USL_VALUE = false;
                bool isCLCTITEM_LSL_VALUE = false;
                #endregion // C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시

                if (DataGrid == null) return string.Empty;

                switch (HOLD_TRGT_CODE)
                {
                    case "SETTINNG_VALUE":
                        #region SETTINNG_VALUE
                        try
                        {
                            switch (columnName)
                            {
                                case "CLCTITEM_USL_VALUE":
                                    if (string.IsNullOrWhiteSpace(CLCTITEM_USL_VALUE))
                                    {
                                        //상한값이 존재하지 않습니다
                                        error.AppendLine(MessageHelper.MessageValidation("FM_ME_0158"));
                                        ////SFU4351		미입력된 항목이 존재합니다.	
                                        ////error.AppendLine(MessageHelper.MessageValidation("SFU4351"));
                                    }

                                    isINPUT_VALUE = decimal.TryParse(INPUT_VALUE, out decINPUT_VALUE);

                                    isCLCTITEM_USL_VALUE = decimal.TryParse(CLCTITEM_USL_VALUE, out decCLCTITEM_USL_VALUE);
                                    isCLCTITEM_LSL_VALUE = decimal.TryParse(CLCTITEM_LSL_VALUE, out decCLCTITEM_LSL_VALUE);

                                    if (isCLCTITEM_USL_VALUE == true && isCLCTITEM_LSL_VALUE == true)
                                    {
                                        if (decCLCTITEM_USL_VALUE < decCLCTITEM_LSL_VALUE)
                                        {
                                            //입력값이 하한값보다 작습니다
                                            error.AppendLine(MessageHelper.MessageValidation("SFU1806"));
                                        }
                                    }

                                    break;
                                case "CLCTITEM_LSL_VALUE":
                                    if (string.IsNullOrWhiteSpace(CLCTITEM_LSL_VALUE))
                                    {
                                        //하한값이 존재하지 않습니다
                                        error.AppendLine(MessageHelper.MessageValidation("FM_ME_0253"));
                                        //////SFU4351		미입력된 항목이 존재합니다.	
                                        ////error.AppendLine(MessageHelper.MessageValidation("SFU4351"));
                                    }

                                    isINPUT_VALUE = decimal.TryParse(INPUT_VALUE, out decINPUT_VALUE);

                                    isCLCTITEM_USL_VALUE = decimal.TryParse(CLCTITEM_USL_VALUE, out decCLCTITEM_USL_VALUE);
                                    isCLCTITEM_LSL_VALUE = decimal.TryParse(CLCTITEM_LSL_VALUE, out decCLCTITEM_LSL_VALUE);

                                    if (isCLCTITEM_USL_VALUE == true && isCLCTITEM_LSL_VALUE ==true)
                                    {
                                        if(decCLCTITEM_USL_VALUE < decCLCTITEM_LSL_VALUE)
                                        {
                                            //하한값이 상한값 보다 클 수 없습니다.
                                            error.AppendLine(MessageHelper.MessageValidation("FM_ME0252"));
                                        }
                                    }

                                    break;
                                case "INPUT_VALUE":



                                    isINPUT_VALUE = decimal.TryParse(INPUT_VALUE, out decINPUT_VALUE);

                                    isCLCTITEM_USL_VALUE = decimal.TryParse(CLCTITEM_USL_VALUE, out decCLCTITEM_USL_VALUE);
                                    isCLCTITEM_LSL_VALUE = decimal.TryParse(CLCTITEM_LSL_VALUE, out decCLCTITEM_LSL_VALUE);

                                    if (isINPUT_VALUE == true && isCLCTITEM_USL_VALUE == true && isCLCTITEM_LSL_VALUE == true )
                                    {
                                        if (string.IsNullOrWhiteSpace(INPUT_VALUE))
                                        {
                                            //SFU4351		미입력된 항목이 존재합니다.	
                                            error.AppendLine(MessageHelper.MessageValidation("SFU4351"));
                                        }

                                        if (decINPUT_VALUE < decCLCTITEM_LSL_VALUE)
                                        {
                                            //입력값이 하한값보다 작습니다
                                            error.AppendLine(MessageHelper.MessageValidation("SFU1806"));
                                        }

                                        if (decINPUT_VALUE > decCLCTITEM_USL_VALUE)
                                        {
                                            //입력값이 상한 값 보다 큽니다
                                            error.AppendLine(MessageHelper.MessageValidation("SFU1805"));
                                        }
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        #endregion
                        break;


                    default:
                        break;

                }

                #region Column Error
                if (error.Length >= 2)
                    error = error.Replace("\r", "", error.Length - 2, 1).Replace("\n", "", error.Length - 1, 1);

                try
                {
                    int idx = DataGrid.Rows.IndexOf(this);
                    if (idx >= 0)
                    {
                        foreach (var item in DataGrid.Rows[idx].Errors.ToList())
                        {
                            if (item.ColumnNames.Contains(columnName))
                            {
                                DataGrid.Rows[idx].Errors.Remove(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(error.ToString()))
                        {
                            string colHeader = DataGrid.Columns[columnName].Header.ToString();
                            error.Insert(0, string.Format("[{0}]:", colHeader));
                            DataGrid.Rows[idx].Errors.Add(new DataGridRowError(error.ToString(), new string[] { columnName }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                #endregion

                #region Row Error
                if (errorRow.Length >= 2)
                    errorRow = errorRow.Replace("\r", "", errorRow.Length - 2, 1).Replace("\n", "", errorRow.Length - 1, 1);

                try
                {
                    int idx = DataGrid.Rows.IndexOf(this);
                    if (idx >= 0)
                    {
                        foreach (var item in DataGrid.Rows[idx].Errors.ToList())
                        {
                            if (item.ColumnNames.Contains(rowName))
                            {
                                DataGrid.Rows[idx].Errors.Remove(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(errorRow.ToString()))
                        {
                            string colHeader = DataGrid.Columns["INPUT_VALUE"].Header.ToString();
                            errorRow.Insert(0, string.Format("[{0}]:", colHeader));
                            DataGrid.Rows[idx].Errors.Add(new DataGridRowError(errorRow.ToString(), new string[] { rowName }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                #endregion

                return error.Append(errorRow).ToString();
            }
        }
        #endregion

        #region [ Util ]

  

    
        #region CELL ID 유효성 체크--//상-하한값이 모두 같을경우 저장시 체크 로직 넣을지 고민 할것
        public bool IsCellIDDuplecate(C1DataGrid grid, HoldModel item)
        {
            bool result = false;

            ObservableCollectionHold<HoldModel> items = (ObservableCollectionHold<HoldModel>)grid.ItemsSource;
            if (items == null) return false;
            if (string.IsNullOrEmpty(item.INPUT_VALUE)) return false;

            foreach (var _item in items)
            {
                if (_item == item)
                    continue;

                if (_item.CLCTITEM_LSL_VALUE == item.CLCTITEM_USL_VALUE)
                {
                    return true;
                }
                    
            }

            return result;
        }
        #endregion

        #endregion
    }

    public class DataTableHoldHelper : DataTableHelper
    {
        public static void CopyToObservableCollectionFromTable<T>(DataTable tbl, ObservableCollectionHold<T> desc) where T : new()
        {
            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                desc.Add(CreateItemFromRow<T>(r));
            }
        }
    }

    #endregion

    #region [ Common ]
    public class MessageHelper
    {
        public static void ResultMessage(object result, IFrameOperation iFO)
        {
            int resultCount = 0;

            if (result == null) return;

            if (result is ICollection)
                resultCount = (result as ICollection).Count;
            else if (result is DataTable)
                resultCount = (result as DataTable).Rows.Count;

            if (resultCount == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(resultCount + ObjectDic.Instance.GetObjectName("건"));
            }
        }

        public static string MessageValidation(string messageId, Action<MessageBoxResult> callback)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            return message;

        }

        /// <summary>
        /// Validation MessageBox 호출 메소드
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="parameters"></param>
        public static string MessageValidation(string messageId, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i]?.ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }

            return message;
        }
    }

    public class GridSetDataHelper
    {
        public static void GridSetData<T>(C1.WPF.DataGrid.C1DataGrid dataGrid, DataTable dt, IFrameOperation iFO, bool isAutoWidth = false) where T : new()
        {
            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시 
            //gridClear(dataGrid);

            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            #endregion

            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
            ObservableCollection<T> _items = (dataGrid.ItemsSource as ObservableCollection<T>);
            _items.Clear();

            List<T> _list = DataTableHelper.CreateListFromTable<T>(dt);
            if (_list.Count > 0)
            {
                foreach (var item in _list)
                    _items.Add(item);
            }
            #endregion

            dataGrid.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

            if (dt.Rows.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(dt.Rows.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && dt.Rows.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    { 
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        public static void GridSetData<T>(C1.WPF.DataGrid.C1DataGrid dataGrid, ObservableCollection<T> oc, IFrameOperation iFO, bool isAutoWidth = false) where T : new()
        {
            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시 
            //gridClear(dataGrid);

            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            #endregion

            #region C20220914-000604(389334) ESNA PKG EQU Work Condition Improve 왕신량 주관 요청 : 설정값 상/하 한치 범위 초과시 붉은 색 표시
            ObservableCollection<T> _items = (dataGrid.ItemsSource as ObservableCollection<T>);
            _items.Clear();

            if (oc.Count > 0)
            {
                foreach (var item in oc)
                    _items.Add(item);
            }
            #endregion

            dataGrid.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

            if (oc.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(oc.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && oc.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    { 
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        private static void DataGridLoaded(object sender, RoutedEventArgs args)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
            double sumHeight = dataGrid.ActualHeight;

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

            dataGrid.UpdateLayout();
            dataGrid.Measure(new Size(sumWidth, sumHeight));

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                if (dgc.ActualWidth > 0)
                    dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);
        }
    }

    public class DataTableHelper
    {
        // function that creates a list of an object from the given data table
        public static List<T> CreateListFromTable<T>(DataTable tbl) where T : new()
        {
            // define return list
            List<T> lst = new List<T>();

            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                lst.Add(CreateItemFromRow<T>(r));
            }

            // return the list
            return lst;
        }

        // function that creates an object from the given data row
        public static T CreateItemFromRow<T>(DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }

        public static void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }
    }
    #endregion
    #endregion
}
