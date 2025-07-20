/*************************************************************************************
 Created Date : 2017.07.03
      Creator : 두잇 이선규K
   Decription : 전지 5MEGA-GMES 구축 - DSF 대기창고 관리 - 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.03  두잇 이선규K : Initial Created.
  2017.09.18  INS  김동일K : 조립 Prj 에서 활성화 Prj 로 소스코드 이동
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
using System.Windows.Media;

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_051_MONITORING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_051_MONITORING : C1Window, IWorkArea
    {   
        #region Declaration & Constructor

        string _ProcID = string.Empty;
        string _DateFrom = string.Empty;
        string _DateTo = string.Empty;
        string _WarehouseID = string.Empty;

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        #endregion

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_051_MONITORING()
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

                if (tmps != null && tmps.Length >= 4)
                {
                    _ProcID = Util.NVC(tmps[0]);
                    _DateFrom = Util.NVC(tmps[1]);
                    _DateTo = Util.NVC(tmps[2]);
                    _WarehouseID = Util.NVC(tmps[3]);
                }
                else
                {
                    _ProcID = string.Empty;
                    _DateFrom = string.Empty;
                    _DateTo = string.Empty;
                    _WarehouseID = string.Empty;
                }

                ApplyPermissions();
                GetProduct();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [메인 윈도우 이벤트]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetProduct();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion [메인 윈도우 이벤트]

        #region [리스트 그리드 이벤트]
        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    string sPrjtNmae = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRJT_NAME"));

                    if (sPrjtNmae.Equals(ObjectDic.Instance.GetObjectName("Total")))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        #endregion [리스트 그리드 이벤트]

        #region [Biz Call]

        private void GetProduct()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_PRODUCT_STOCK();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _ProcID;
                dr["WH_ID"] = _WarehouseID;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PRODUCT_STOCK", "RQSTDT", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            var tot = searchResult.AsEnumerable()
                                                  .OrderByDescending(o => o.Field<string>("TYPE"))
                                                  .GroupBy(g => new
                                                  {
                                                      TYPE = g.Field<string>("TYPE"),
                                                      TYPENAME = g.Field<string>("TYPENAME")
                                                  })
                                                  .Select(s => new
                                                  {
                                                      TYPE = s.Key.TYPE,
                                                      TYPENAME = s.Key.TYPENAME,
                                                      TOT_QTY = s.Sum(x => x.Field<decimal>("TOT_QTY")),
                                                      HOLD_QTY = s.Sum(x => x.Field<decimal>("HOLD_QTY")),
                                                      DAY_0 = s.Sum(x => x.Field<decimal>("DAY_0")),
                                                      DAY_1 = s.Sum(x => x.Field<decimal>("DAY_1")),
                                                      DAY_7 = s.Sum(x => x.Field<decimal>("DAY_7")),
                                                      DAY_30 = s.Sum(x => x.Field<decimal>("DAY_30")),
                                                      DAY_90 = s.Sum(x => x.Field<decimal>("DAY_90")),
                                                  });

                            DataRow newRow = null;
                            foreach (var totR in tot)
                            {
                                newRow = searchResult.NewRow();

                                newRow["PRJT_NAME"] = ObjectDic.Instance.GetObjectName("Total");
                                newRow["TYPE"] = totR.TYPE;
                                newRow["TYPENAME"] = totR.TYPENAME;
                                newRow["TOT_QTY"] = totR.TOT_QTY;
                                newRow["HOLD_QTY"] = totR.HOLD_QTY;
                                newRow["DAY_0"] = totR.DAY_0;
                                newRow["DAY_1"] = totR.DAY_1;
                                newRow["DAY_7"] = totR.DAY_7;
                                newRow["DAY_30"] = totR.DAY_30;
                                newRow["DAY_90"] = totR.DAY_90;

                                searchResult.Rows.Add(newRow);
                            }
                        }

                        //dgList.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgList, searchResult, null, true);
                        if (dgList.Rows != null && dgList.Rows.Count > 2)
                        {
                            string sTotalDesc = ObjectDic.Instance.GetObjectName("Total");

                            if (DataTableConverter.GetValue(dgList.Rows[dgList.Rows.Count - 1].DataItem, "PRJT_NAME").ToString().Equals(sTotalDesc))
                                if (dgList.Rows[dgList.Rows.Count - 1].Presenter != null)
                                    dgList.Rows[dgList.Rows.Count - 1].Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F7E9D5"));

                            if (DataTableConverter.GetValue(dgList.Rows[dgList.Rows.Count - 2].DataItem, "PRJT_NAME").ToString().Equals(sTotalDesc))
                                if (dgList.Rows[dgList.Rows.Count - 2].Presenter != null)
                                    dgList.Rows[dgList.Rows.Count - 2].Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F7E9D5"));
                        }
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

        #endregion [Biz Call]

        #region [Validation]

        #endregion

        #region [FUNC]

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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnSearch);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion [FUNC]
    }
}
