/*************************************************************************************
 Created Date : 2017.01.19
      Creator : 신광희
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - 대기LOT삭제 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.19  신광희 : Initial Created.
  2017.02.08  신광희 : 동적그리드 헤더 다국어 처리 및 메세지 박스 코드 처리
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_WAITLOT_DELETE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_WAITLOT_DELETE : C1Window, IWorkArea
    {


        #region Declaration & Constructor 
        private string _lineCode = string.Empty;
        private string _eqptCode = string.Empty;
        private string _processCode = string.Empty;
        private string _processType = string.Empty;
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

        public CMM_ASSY_WAITLOT_DELETE()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            InitializeDataGrid();
        }

        private void InitializeDataGrid()
        {
            int columnIndex = this.dgWaitLot.Columns["CHK"].Index;

            //ASSY001_007
            if (_processType == ProcessType.Packaging)
            {
                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PR_LOTID",
                    Header = ObjectDic.Instance.GetObjectName("FOLDLOT"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PR_LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "LOTID",
                    Header = ObjectDic.Instance.GetObjectName("바구니ID"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPSNAME",
                    Header = ObjectDic.Instance.GetObjectName("상태"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPSNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = "WIPQTY",
                    Header = ObjectDic.Instance.GetObjectName("수량"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPQTY"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Format = "#,##0"
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PRODID",
                    Header = ObjectDic.Instance.GetObjectName("제품ID"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PRODID"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PRODNAME",
                    Header = ObjectDic.Instance.GetObjectName("제품명"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PRODNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPHOLD",
                    Header = ObjectDic.Instance.GetObjectName("HOLD여부"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPHOLD"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });
            }
            //ASSY001_006
            else if (_processType == ProcessType.Stacking)
            {

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PR_LOTID",
                    Header = ObjectDic.Instance.GetObjectName("라미LOTID"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PR_LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "LOTID",
                    Header = ObjectDic.Instance.GetObjectName("매거진"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "CSTID",
                    Header = ObjectDic.Instance.GetObjectName("카세트"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("CSTID"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPSTAT",
                    Header = ObjectDic.Instance.GetObjectName("상태"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPSTAT"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Visibility = Visibility.Collapsed,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPSNAME",
                    Header = ObjectDic.Instance.GetObjectName("상태"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPSNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = "WIPQTY",
                    Header = ObjectDic.Instance.GetObjectName("수량"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPQTY"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Right,
                    TextWrapping = TextWrapping.Wrap,
                    Format = "#,##0",
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PRODNAME",
                    Header = ObjectDic.Instance.GetObjectName("제품명"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PRODNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPHOLD",
                    Header = ObjectDic.Instance.GetObjectName("HOLD여부"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPHOLD"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });
            }
            //ASSY001_005
            else if (_processType == ProcessType.Fold)
            {
                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PR_LOTID",
                    Header = ObjectDic.Instance.GetObjectName("라미LOTID"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PR_LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "LOTID",
                    Header = ObjectDic.Instance.GetObjectName("매거진"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "CSTID",
                    Header = ObjectDic.Instance.GetObjectName("카세트"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("CSTID"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPSTAT",
                    Header = ObjectDic.Instance.GetObjectName("상태"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPSTAT"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Visibility = Visibility.Collapsed,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPSNAME",
                    Header = ObjectDic.Instance.GetObjectName("상태"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPSNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = "WIPQTY",
                    Header = ObjectDic.Instance.GetObjectName("수량"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPQTY"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Right,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PRODNAME",
                    Header = ObjectDic.Instance.GetObjectName("제품명"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PRODNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPHOLD",
                    Header = ObjectDic.Instance.GetObjectName("HOLD여부"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPHOLD"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });
            }
            //ASSY001_004
            else if (_processType == ProcessType.Lamination)
            {
                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "LOTID",
                    Header = ObjectDic.Instance.GetObjectName("LOTID"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPSNAME",
                    Header = ObjectDic.Instance.GetObjectName("상태"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPSNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PRODID",
                    Header = ObjectDic.Instance.GetObjectName("제품ID"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PRODID"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PRODNAME",
                    Header = ObjectDic.Instance.GetObjectName("제품명"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PRODNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPHOLD",
                    Header = ObjectDic.Instance.GetObjectName("HOLD여부"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPHOLD"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = "WIPQTY",
                    Header = ObjectDic.Instance.GetObjectName("수량"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPQTY"),
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Format = "###,###.##",
                    IsReadOnly = true
                });
            }
            //ASSY001_030
            else if (_processType == ProcessType.FoldedBiCell)
            {
                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PR_LOTID",
                    Header = ObjectDic.Instance.GetObjectName("FOLDLOT"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PR_LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "LOTID",
                    Header = ObjectDic.Instance.GetObjectName("바구니ID"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("LOTID"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPSNAME",
                    Header = ObjectDic.Instance.GetObjectName("상태"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPSNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                {
                    Name = "WIPQTY",
                    Header = ObjectDic.Instance.GetObjectName("수량"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPQTY"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Format = "#,##0"
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PRODID",
                    Header = ObjectDic.Instance.GetObjectName("제품ID"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PRODID"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "PRODNAME",
                    Header = ObjectDic.Instance.GetObjectName("제품명"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PRODNAME"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });

                dgWaitLot.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Name = "WIPHOLD",
                    Header = ObjectDic.Instance.GetObjectName("HOLD여부"),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("WIPHOLD"),
                        Mode = BindingMode.TwoWay
                    },
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true
                });
            }

        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _lineCode = Util.NVC(tmps[0]);
                _eqptCode = Util.NVC(tmps[1]);
                _processCode = Util.NVC(tmps[2]);
                _processType = Util.NVC(tmps[3]);
            }

            InitializeControls();
            ApplyPermissions();
            GetWaitBox();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitBox();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBlock_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitBox();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgWaitLot)) return;
                dgWaitLot.EndEdit();
                dgWaitLot.EndEditRow(true);

                if (!Validation())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteWaitLot();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region Mehod

        private void GetWaitBox()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_FOR_DELETE";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                inTable.Columns.Add("PRODUCT_LEVEL2_CODE", typeof(string));
                inTable.Columns.Add("PRODUCT_LEVEL3_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();

                if (_processType == ProcessType.Notching)
                {

                }
                else if (_processType == ProcessType.Vacuumdry)
                {

                }
                else if (_processType == ProcessType.Lamination)
                {
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = _processCode;
                    newRow["EQPTID"] = _eqptCode;
                    newRow["EQSGID"] = _lineCode;
                    newRow["PRDT_CLSS_CODE"] = "A,C,L"; //A:음극 , C:양극 , L:단면
                    newRow["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? "" : txtLotId.Text.Trim();
                }
                else if (_processType == ProcessType.Fold)
                {
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = _processCode;
                    newRow["EQPTID"] = null;
                    newRow["EQSGID"] = _lineCode;
                    newRow["PRODUCT_LEVEL2_CODE"] = "BC";
                    newRow["PRODUCT_LEVEL3_CODE"] = "AT,CT";    //AT: AType, CT: CType
                    newRow["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? "" : txtLotId.Text.Trim();
                }
                else if (_processType == ProcessType.Stacking)
                {
                    //ASSY001_006_WAITLOT
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = _processCode;
                    newRow["EQPTID"] = null;
                    newRow["EQSGID"] = _lineCode;
                    newRow["PRODUCT_LEVEL2_CODE"] = "HALFTYPE,MONOTYPE";
                    newRow["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? "" : txtLotId.Text.Trim();
                }
                else if (_processType == ProcessType.Packaging)
                {
                    //ASSY001_007_WAITLOT
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = _processCode;
                    newRow["EQPTID"] = null;
                    newRow["EQSGID"] = _lineCode;
                    newRow["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? "" : txtLotId.Text.Trim();
                }
                else if (_processType == ProcessType.FoldedBiCell)
                {
                    //ASSY001_030_WAITLOT
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PROCID"] = _processCode;
                    newRow["EQPTID"] = null;
                    newRow["EQSGID"] = _lineCode;
                    newRow["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? "" : txtLotId.Text.Trim();
                }
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgWaitLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitLot, searchResult, null, true);

                        if (dgWaitLot.CurrentCell != null)
                            dgWaitLot.CurrentCell = dgWaitLot.GetCell(dgWaitLot.CurrentCell.Row.Index, dgWaitLot.Columns.Count - 1);
                        else if (dgWaitLot.Rows.Count > 0)
                            dgWaitLot.CurrentCell = dgWaitLot.GetCell(dgWaitLot.Rows.Count, dgWaitLot.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private bool Validation()
        {
            try
            {
                DataTable dt = ((DataView)dgWaitLot.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<Int32>("CHK") == 1
                                 select t).ToList();

                if (!queryEdit.Any())
                {
                    Util.MessageValidation("10008");    //선택된 데이터가 없습니다.
                    return false;
                }

                if (string.IsNullOrEmpty(txtRemark.Text.Trim()))
                {
                    Util.MessageValidation("SFU1594");      //사유를입력하세요.
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void DeleteWaitLot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_PRD_REG_DELETE_LOT";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow param = inDataTable.NewRow();
                param["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                param["IFMODE"] = LoginInfo.LANGID;
                param["NOTE"] = txtRemark.Text;
                param["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(param);

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("LOTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgWaitLot.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1")
                    {
                        DataRow dr = inInputTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID");
                        inInputTable.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        Util.MessageInfo("SFU1270");      //저장되었습니다.
                        GetWaitBox();
                        txtRemark.Text = string.Empty;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnExecute };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion


    }
}
