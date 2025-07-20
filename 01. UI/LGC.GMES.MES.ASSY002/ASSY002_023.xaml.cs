/*************************************************************************************
 Created Date : 2018.11.19
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 초소형 CELL 재공 생성 호면 (N5 전용)
--------------------------------------------------------------------------------------
 [Change History]
  2018.11.19  INS 김동일K : Initial Created.
  
**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY002
{
    /// <summary>
    /// ASSY002_023.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY002_023 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        public ASSY002_023()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgReceive.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReceive.Rows[i].DataItem, "CHK", true);
                }
            }

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgReceive.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReceive.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Loaded -= UserControl_Loaded;

                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                initCombo();

                dgReceive.ClipboardPasteMode = DataGridClipboardMode.None;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                if (_Util.GetDataGridCheckFirstRowIndex(dgReceive, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1632");   //선택된 LOT이 없습니다.
                    return;
                }

                //입고 하시겠습니까?                
                Util.MessageConfirm("SFU2073", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            loadingIndicator.Visibility = Visibility.Visible;

                            DataTable indataTable = new DataTable();
                            indataTable.Columns.Add("ASSY_PROC_LOTID", typeof(string));
                            indataTable.Columns.Add("PRODID", typeof(string));                            
                            indataTable.Columns.Add("WIPQTY", typeof(string));
                            indataTable.Columns.Add("WND_GR_CODE", typeof(string));
                            indataTable.Columns.Add("WND_EQPTID", typeof(string));
                            indataTable.Columns.Add("USERID", typeof(string));
                            
                            for (int i = 0; i < dgReceive.GetRowCount(); i++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CHK")).Equals("1"))
                                {
                                    DataRow row = indataTable.NewRow();
                                    row["ASSY_PROC_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "ASSY_PROC_LOTID"));
                                    row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PRODID"));                                    
                                    row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY"));
                                    row["WND_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WND_GR_CODE"));
                                    row["WND_EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WND_EQPTID"));
                                    row["USERID"] = LoginInfo.USERID;

                                    indataTable.Rows.Add(row);
                                }
                            }

                            if (indataTable.Rows.Count < 1)
                            {
                                loadingIndicator.Visibility = Visibility.Hidden;
                                return;
                            }
                            new ClientProxy().ExecuteService("BR_PRD_REG_RCEIVE_MTRL_FOR_NJ_F6200", "INDATA", null, indataTable, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    Util.MessageInfo("SFU1798");   //입고 처리 되었습니다.
                                    Initialize_dgReceive();
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                }

                            });
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 삭제 하시겠습니까?
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataTable dt = DataTableConverter.Convert(dgReceive.ItemsSource);

                            for (int i = dt.Rows.Count - 1; i >= 0; i--)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CHK")).Equals("1"))
                                {
                                    dt.Rows[i].Delete();
                                }
                            }

                            dgReceive.ItemsSource = DataTableConverter.Convert(dt);
                            if (dgReceive.GetRowCount() == 0)
                            {
                                Initialize_dgReceive();
                            }

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgReceive_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                dgReceive.Loaded -= dgReceive_Loaded;
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Initialize_dgReceive();
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgReceive_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && KeyboardUtil.Ctrl)
                {
                    DataTable dt = DataTableConverter.Convert(dgReceive.ItemsSource);

                    for (int i = dgReceive.GetRowCount() - 1; i >= 0; i--)
                    {
                        dt.Rows[i].Delete();
                    }

                    string text = Clipboard.GetText();
                    string[] table = text.Split('\n');

                    if (table == null)
                    {
                        Util.MessageValidation("SFU1482");   //다시 복사 해주세요.
                        return;
                    }
                    if (table.Length == 1)
                    {
                        Initialize_dgReceive();
                        Util.MessageValidation("SFU1482");   //다시 복사 해주세요.
                        return;
                    }


                    for (int i = 0; i < table.Length - 1; i++)
                    {
                        string[] rw = table[i].Split('\t');
                        if (rw == null)
                        {
                            Util.MessageValidation("SFU1498");   //데이터가 없습니다.
                            return;
                        }

                        if (rw.Length != 5)
                        {
                            Util.MessageValidation("SFU5068");   //복사한 양식과 화면이 다릅니다. 다시 복사해 주세요.
                            return;
                        }

                        DataRow row = dt.NewRow();
                        row["CHK"] = true;
                        if (Util.NVC(rw[0]).Trim().Equals(""))
                        {
                            Util.MessageValidation("SFU5070", ObjectDic.Instance.GetObjectName("조립LOTID"));   //%1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                            return;
                        }
                        if (!rw[0].ToString().Equals(System.Text.RegularExpressions.Regex.Replace(rw[0].ToString(), @"[^A-Z0-9]", "")))
                        {
                            //입력한 ID (%1) 에 특수문자가 존재하여 생성할 수 없습니다.
                            Util.MessageValidation("SFU1811", rw[0].ToString());   
                            return;
                        }
                        if (rw[0].ToString().Length != 10)
                        {
                            // LOT ID는 10자리이며, 숫자 또는 영문대문자만 입력 가능합니다.
                            Util.MessageValidation("SFU4045");
                            return;
                        }
                        row["ASSY_PROC_LOTID"] = rw[0];

                        if (Util.NVC(rw[1]).Trim().Equals(""))
                        {
                            Util.MessageValidation("SFU5070", ObjectDic.Instance.GetObjectName("PRODID"));   //%1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                            return;
                        }
                        row["PRODID"] = rw[1];
                        
                        double diQty = 0;
                        if (!double.TryParse(rw[2].ToString(), out diQty))
                        {
                            //숫자필드에 부적절한 값이 입력 되었습니다.
                            Util.MessageValidation("SFU2914");
                            return;
                        }
                        if (diQty <= 0)
                        {
                            // 수량은 0보다 커야 합니다.
                            Util.MessageValidation("SFU1683");
                            return;
                        }
                        row["WIPQTY"] = rw[2];

                        if (Util.NVC(rw[3]).Trim().Equals(""))
                        {
                            Util.MessageValidation("SFU5070", ObjectDic.Instance.GetObjectName("WINDER그룹"));   //%1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                            return;
                        }
                        row["WND_GR_CODE"] = rw[3];

                        if (Util.NVC(rw[4]).Trim().Equals(""))
                        {
                            Util.MessageValidation("SFU5070", ObjectDic.Instance.GetObjectName("와인더설비ID"));   //%1 항목은 필수 입력 항목 입니다. 데이터를 확인하세요.
                            return;
                        }
                        row["WND_EQPTID"] = rw[4].ToString().Replace("\r", "");

                        dt.Rows.Add(row);
                    }

                    dgReceive.BeginEdit();
                    dgReceive.ItemsSource = DataTableConverter.Convert(dt);
                    dgReceive.EndEdit();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgReceive_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("FROMDATE", typeof(string));
                inDataTable.Columns.Add("TODATE", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = Process.SmallExternalTab;
                searchCondition["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                searchCondition["TODATE"] = Util.GetCondition(dtpDateTo);
                inDataTable.Rows.Add(searchCondition);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPCREATE_HIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.GridSetData(dgResult_HIST, searchResult, FrameOperation, true);                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Hidden;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReceive);
            listAuth.Add(btnDelete);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void initCombo()
        {

            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            ////라인
            //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: null, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            ////공정
            //combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboEquipmentSegmentParent);
            
        }

        private void Initialize_dgReceive()
        {
            dgReceive.ItemsSource = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(bool));
            dt.Columns.Add("ASSY_PROC_LOTID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));            
            dt.Columns.Add("WIPQTY", typeof(string));
            dt.Columns.Add("WND_GR_CODE", typeof(string));
            dt.Columns.Add("WND_EQPTID", typeof(string));

            DataRow row = dt.NewRow();
            row["CHK"] = false;
            row["ASSY_PROC_LOTID"] = "";
            row["PRODID"] = "";            
            row["WIPQTY"] = "";
            row["WND_GR_CODE"] = "";
            row["WND_EQPTID"] = "";

            dt.Rows.Add(row);

            dgReceive.BeginEdit();
            dgReceive.ItemsSource = DataTableConverter.Convert(dt);
            dgReceive.EndEdit();

        }
    }
}
