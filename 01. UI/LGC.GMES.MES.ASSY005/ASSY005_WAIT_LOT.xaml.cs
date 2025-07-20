/*************************************************************************************
 Created Date : 2020.11.04
      Creator : 정문교
   Decription : CNB2동 증설 - 대기 Pancake, 대기매거진, 대기바구니 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.27  신광희 : Initial Created.
  2021.09.02  조영대 : 폴딩 추가
  2023.02.24  김용군 : ESHM 증설 - AZS-STAKING 대응(제품타입 제거)
  2023.03.27  김용군 : ESHM 증설 - AZS_ECUTTER 투입 대기Lot조회 대응
  202.06.07   김선영 : ESHG 증설 - 대기매거진 조회 - 극성 조건 추가 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
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
//using LGC.GMES.MES.ASSY004;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.ASSY005
{
    /// <summary>
    /// ASSY005_OUTLOT_MERGE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY005_WAIT_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _equipmentSegmentCode = string.Empty;
        private string _processCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _ProdLotID = string.Empty;
        private string _eqptMountPstnID = string.Empty;
        private bool _isWaitUseAuthority = false;

        private string _inputLotID = string.Empty;

        private string _Max_Pre_Proc_End_Day = string.Empty;
        private DateTime _Min_Valid_Date;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();

        private string _equipmentGroupCode = string.Empty;

        // 김용군
        private string _ProcID = string.Empty;
        private string _WoID = string.Empty;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        public C1Window _Parent;

        public string InputLotID
        {
            get { return _inputLotID; }
        }

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

        public ASSY005_WAIT_LOT()
        {
            InitializeComponent();
        }

        // 김용군
        public string EQPTSEGMENT
        {
            get { return _equipmentSegmentCode; }
            set { _equipmentSegmentCode = value; }
        }

        public string EQPTID
        {
            get { return _equipmentCode; }
            set { _equipmentCode = value; }
        }
        public string PROD_WOID
        {
            get { return _WoID; }
            set { _WoID = value; }
        }
        public string LDR_LOT_IDENT_BAS_CODE
        {
            get { return _LDR_LOT_IDENT_BAS_CODE; }
            set { _LDR_LOT_IDENT_BAS_CODE = value; }
        }
        public string PROCID
        {
            get { return _ProcID; }
            set { _ProcID = value; }
        }

        #endregion

        #region Event       
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetControl();
            SetControlVisibility();

            // 선입선출 기준일 조회.
            GetProcMtrlInputRule();

            // 조회
            if (_processCode == Process.LAMINATION)
                SelectPancake();
            else if (_processCode == Process.PACKAGING)
                SelectBox();

            // 김용군 AZS_ECUTTER 공정이면 대기PANCAKE, AZS_STACKING 공정이면 대기매거진 호출
            else if (_processCode == Process.AZS_ECUTTER || _processCode == Process.DNC)   //202.06.07   김선영 : ESHG 증설 - 대기매거진 조회 - 극성 조건 추가
            {
                SelectPancake();
            }
            else if(_processCode == Process.AZS_STACKING)
            {
                SelectStkMagazine();
            }
        }


        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _equipmentSegmentCode = Util.NVC(tmps[0]);
            _processCode = Util.NVC(tmps[1]);
            _equipmentCode = Util.NVC(tmps[2]);
            _ProdLotID = Util.NVC(tmps[3]);
            _eqptMountPstnID = Util.NVC(tmps[4]);
            _isWaitUseAuthority = (bool)tmps[5];
            _equipmentGroupCode = Util.NVC(tmps[6]);
            
            txtEquipment.Text = _equipmentCode;
            txtProdLotID.Text = _ProdLotID;
            txtMountPstnID.Text = _eqptMountPstnID;

            if (_isWaitUseAuthority == false)
                btnInput.Visibility = Visibility.Collapsed;

            if (_processCode == Process.LAMINATION)
                this.Header = ObjectDic.Instance.GetObjectName("대기PANCAKE");
            else if (_processCode == Process.STACKING_FOLDING)
                this.Header = ObjectDic.Instance.GetObjectName("대기매거진");
            // 김용군 AZS_ECUTTER 공정 : 대기PANCAKE, AZS_STACKING 공정 : 대기매거진
            else if (_processCode == Process.AZS_ECUTTER || _processCode == Process.DNC)				//202.06.07   김선영 : ESHG 증설 - 대기매거진 조회 - 극성 조건 추가
                this.Header = ObjectDic.Instance.GetObjectName("대기PANCAKE");
            else if (_processCode == Process.AZS_STACKING)
                this.Header = ObjectDic.Instance.GetObjectName("대기매거진");
            else
                this.Header = ObjectDic.Instance.GetObjectName("대기바구니");

            if (_processCode.Equals(Process.STACKING_FOLDING) && _equipmentGroupCode.Equals(EquipmentGroup.FOLDING))
            {
                rdoMagAType.Content = ObjectDic.Instance.GetObjectName("ATYPE");
                rdoMagAType.Tag = "AT";

                rdoMagCtype.Content = ObjectDic.Instance.GetObjectName("CTYPE");
                rdoMagCtype.Tag = "CT";

            }
            else
            {
                rdoMagAType.Content = ObjectDic.Instance.GetObjectName("HALFTYPE");
                rdoMagAType.Tag = "HC";

                rdoMagCtype.Content = ObjectDic.Instance.GetObjectName("MONOTYPE");
                rdoMagCtype.Tag = "MC";
            }

        }
        
        private void SetControlVisibility()
        {
            tbPancake.Visibility = Visibility.Collapsed;
            tbMagazine.Visibility = Visibility.Collapsed;
            tbBox.Visibility = Visibility.Collapsed;

            if (_processCode == Process.LAMINATION)
                tbPancake.Visibility = Visibility.Visible;
            else if (_processCode == Process.STACKING_FOLDING)
                tbMagazine.Visibility = Visibility.Visible;
            // 김용군 AZS_STACKING 공정인 경우 대기매거진TAB 활성화
            else if (_processCode == Process.AZS_STACKING)
                tbMagazine.Visibility = Visibility.Visible;
            // 김용군 AZS_AZS_ECUTTER 공정인 경우 대기매거진TAB 활성화
            else if (_processCode == Process.AZS_ECUTTER || _processCode == Process.DNC)
                tbPancake.Visibility = Visibility.Visible;
            else
                tbBox.Visibility = Visibility.Visible;

            // 김용군 AZS_STACKING 공정인 경우 제품타입 비활성화 및 재작업매거진생성 버튼 활성화 
            if (_processCode.Equals(Process.AZS_STACKING))
            {
                rdoMagAType.Visibility = Visibility.Collapsed;
                rdoMagCtype.Visibility = Visibility.Collapsed;
                btnWaitMagRework.Visibility = Visibility.Visible;
            }
            else
            {
                btnWaitMagRework.Visibility = Visibility.Collapsed;
            }

        }

        private void rdoWaitMaz_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked.HasValue)
            {
                if ((bool)(sender as RadioButton).IsChecked)
                {
                    SelectMagazine();
                }
            }
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)(((System.Windows.FrameworkElement)sender).Parent)).DataGrid as C1DataGrid;
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().ToUpper().Equals("FALSE")))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                    }

                    // row 색 바꾸기
                    dg.SelectedIndex = idx;

                    // 투입 선택 LOT
                    _inputLotID = dtRow["LOTID"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitPancake_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(_Max_Pre_Proc_End_Day).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else
                    {
                        int iDay = 0;
                        int.TryParse(_Max_Pre_Proc_End_Day, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals(""))
                            {
                                e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_Min_Valid_Date.AddDays(iDay) >= dtValid)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgWaitPancake_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    }
                }
            }));
        }

        private void dgWaitMagazine_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(_Max_Pre_Proc_End_Day).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        int iDay = 0;
                        int.TryParse(_Max_Pre_Proc_End_Day, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals(""))    // 신규 매거진 구성 data의 경우 biz에서 FIFO 무시 됨.
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_Min_Valid_Date.AddDays(iDay) >= dtValid)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgWaitMagazine_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    }
                }
            }));
        }

        private void dgWaitBox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(_Max_Pre_Proc_End_Day).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        int iDay = 0;
                        int.TryParse(_Max_Pre_Proc_End_Day, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals(""))
                            {
                                e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_Min_Valid_Date.AddDays(iDay) >= dtValid)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgWaitBox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    }
                }
            }));
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInput()) return;

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private void GetProcMtrlInputRule()
        {
            try
            {
                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_MTRL_INPUT_RULE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("MAX_PRE_PROC_END_DAY"))
                {
                    _Max_Pre_Proc_End_Day = Util.NVC(dtRslt.Rows[0]["MAX_PRE_PROC_END_DAY"]);
                }
            }
            catch (Exception ex)
            {
                //HideParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetInputMtrlClssCode()
        {
            try
            {
                string sInputMtrlClssCode = "";

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_MTRL_CLSS_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = _equipmentCode;
                newRow["EQPT_MOUNT_PSTN_ID"] = _eqptMountPstnID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_PRDT_CLSS_CODE_BY_MOUNT_PSTN_ID", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sInputMtrlClssCode = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                }
                return sInputMtrlClssCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void SelectPancake()
        {
            try
            {
                string sInMtrlClssCode = GetInputMtrlClssCode();

                DataTable inTable = _Biz.GetDA_PRD_SEL_READY_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROCID"] = _processCode;
                newRow["INPUT_MTRL_CLSS_CODE"] = sInMtrlClssCode;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_LM_BY_LV3_CODE_L", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(bizResult.Rows[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }

                        //dgWaitPancake.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitPancake, bizResult, FrameOperation, true);

                        //lblSelWaitPancakeCnt.Text = (dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count).ToString();

                        if (dgWaitPancake.CurrentCell != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.CurrentCell.Row.Index, dgWaitPancake.Columns.Count - 1);
                        else if (dgWaitPancake.Rows.Count > 0 && dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1) != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectMagazine()
        {
            try
            {
                string sElec = string.Empty;

                if (rdoMagAType.IsChecked.HasValue && (bool)rdoMagAType.IsChecked)
                {
                    sElec = rdoMagAType.Tag.ToString();
                }
                else if (rdoMagCtype.IsChecked.HasValue && (bool)rdoMagCtype.IsChecked)
                {
                    sElec = rdoMagCtype.Tag.ToString();
                }
                else
                    return;

                string sBizName = "DA_PRD_SEL_WAIT_MAG_ST_L";

                if (_processCode.Equals(Process.STACKING_FOLDING) && _equipmentGroupCode.Equals(EquipmentGroup.FOLDING))
                {
                    sBizName = "DA_PRD_SEL_WAIT_MAG_FD_L";
                }

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                if (_processCode.Equals(Process.STACKING_FOLDING) && _equipmentGroupCode.Equals(EquipmentGroup.FOLDING))
                {
                    newRow["PRODUCT_LEVEL2_CODE"] = "BC";
                    newRow["PRODUCT_LEVEL3_CODE"] = sElec;
                }
                else
                {
                    newRow["PRODUCT_LEVEL2_CODE"] = sElec; //BI-CELL, Stacking 경우 Lv2가 Lv3와 동일 코드.
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(sBizName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // FIFO 기준 Date
                        try
                        {
                            if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Columns.Contains("VALID_DATE_YMDHMS"))
                            {
                                DataRow row = (from t in bizResult.AsEnumerable()
                                               where (t.Field<string>("VALID_DATE_YMDHMS") != null)
                                               select t).FirstOrDefault();


                                if (row != null)
                                {
                                    DateTime.TryParse(Util.NVC(row["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        //dgWaitMagazine.ItemsSource = DataTableConverter.Convert(bizResult);
                        Util.GridSetData(dgWaitMagazine, bizResult, FrameOperation, true);

                        if (dgWaitMagazine.CurrentCell != null)
                            dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.CurrentCell.Row.Index, dgWaitMagazine.Columns.Count - 1);
                        else if (dgWaitMagazine.Rows.Count > 0 && dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1) != null)
                            dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectBox()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_CL_L", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(bizResult.Rows[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }


                        Util.GridSetData(dgWaitBox, bizResult, FrameOperation, true);

                        if (dgWaitBox.CurrentCell != null)
                            dgWaitBox.CurrentCell = dgWaitBox.GetCell(dgWaitBox.CurrentCell.Row.Index, dgWaitBox.Columns.Count - 1);
                        else if (dgWaitBox.Rows.Count > 0 && dgWaitBox.GetCell(dgWaitBox.Rows.Count, dgWaitBox.Columns.Count - 1) != null)
                            dgWaitBox.CurrentCell = dgWaitBox.GetCell(dgWaitBox.Rows.Count, dgWaitBox.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
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

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnInput);

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

        #region [Validation]
        private bool ValidationInput()
        {
            if (string.IsNullOrWhiteSpace(_inputLotID))
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(_ProdLotID))
            {
                // 선택된 LOT 이 존재하지 않습니다.
                Util.MessageValidation("SFU1137");
                return false;
            }

            return true;
        }

        // 김용군 재작업매거진 기능
        private void btnWaitMagRework_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ASSY005_005_STACKING_MAGAZINE_CREATE wndMAZCreate = new ASSY005_005_STACKING_MAGAZINE_CREATE();
                wndMAZCreate.FrameOperation = FrameOperation;

                if (wndMAZCreate != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = EQPTSEGMENT;
                    Parameters[1] = EQPTID;
                    Parameters[2] = PROD_WOID;
                    Parameters[3] = _LDR_LOT_IDENT_BAS_CODE;

                    C1WindowExtension.SetParameters(wndMAZCreate, Parameters);

                    wndMAZCreate.Closed += new EventHandler(wndMAZCreate_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndMAZCreate.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 김용군 재작업매거진 기능
        private void wndMAZCreate_Closed(object sender, EventArgs e)
        {
            ASSY005_005_STACKING_MAGAZINE_CREATE window = sender as ASSY005_005_STACKING_MAGAZINE_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            SelectStkMagazine();
        }        

        // 김용군 재작업매거진 기능
        private void ShowParentLoadingIndicator()
        {
            if (_Parent == null)
                return;

            try
            {
                Type type = _Parent.GetType();
                MethodInfo methodInfo = type.GetMethod("ShowLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_Parent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 김용군 재작업매거진 기능
        private void HideParentLoadingIndicator()
        {
            if (_Parent == null)
                return;

            try
            {
                Type type = _Parent.GetType();
                MethodInfo methodInfo = type.GetMethod("HideLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_Parent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 김용군
        private void SelectStkMagazine()
        {
            try
            {
                string sElec = string.Empty;
                string sEltrType_AN = string.Empty;
                string sEltrType_CA = string.Empty;

                //202.06.07   김선영 : ESHG 증설 - 대기매거진 조회 - 극성 조건 추가 , Start 
                string sProduct_level3_code = string.Empty;
                Match match;

                sEltrType_AN = @"AN";
                sEltrType_CA = @"CA";

                match = Regex.Match(_eqptMountPstnID, sEltrType_AN);

                if (match.Success)
                {
                    sProduct_level3_code = match.Value;
                }
                else
                {
                    match = Regex.Match(_eqptMountPstnID, sEltrType_CA);
                    if (match.Success)
                    {
                        sProduct_level3_code = match.Value;
                    }
                }
                //202.06.07   김선영 : ESHG 증설 - 대기매거진 조회 - 극성 조건 추가 , End

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PRODUCT_LEVEL3_CODE"] = sProduct_level3_code;         //202.06.07   김선영 : ESHG 증설 - 대기매거진 조회 - 극성 조건 추가             
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_MAG_STK_ST_L", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // FIFO 기준 Date
                        try
                        {
                            if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Columns.Contains("VALID_DATE_YMDHMS"))
                            {
                                DataRow row = (from t in bizResult.AsEnumerable()
                                               where (t.Field<string>("VALID_DATE_YMDHMS") != null)
                                               select t).FirstOrDefault();


                                if (row != null)
                                {
                                    DateTime.TryParse(Util.NVC(row["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        Util.GridSetData(dgWaitMagazine, bizResult, FrameOperation, true);

                        dgWaitMagazine.Columns["PR_LOTID"].Header = ObjectDic.Instance.GetObjectName("전공정LOT");                        

                        if (dgWaitMagazine.CurrentCell != null)
                            dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.CurrentCell.Row.Index, dgWaitMagazine.Columns.Count - 1);
                        else if (dgWaitMagazine.Rows.Count > 0 && dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1) != null)
                            dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1);

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
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

        #endregion

    }
}
