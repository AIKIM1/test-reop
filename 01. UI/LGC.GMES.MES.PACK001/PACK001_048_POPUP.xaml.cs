/*************************************************************************************
 Created Date : 2019.08.07
      Creator : 염규범
  Description :
--------------------------------------------------------------------------------------
 [Change History]
  2019.08.07  염규범    Initialize
  2021.12.24  정용석    부모 Form에서 넘어온 데이터 가지고 Grid Data 표출 (기존 순서도 호출 제외시킴)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_048_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_048_POPUP()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function List

        /// <summary>
        /// name         : ShowLOTList
        /// desc         : 재전송 기준에 맞지 않는 LOTID POPUP
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void ShowLOTList(object obj)
        {
            try
            {
                if (!obj.GetType().Equals(typeof(DataTable)))
                {
                    return;
                }

                DataTable dtNewLOTList = (DataTable)obj;
                if (!CommonVerify.HasTableRow(dtNewLOTList))
                {
                    return;
                }

                var query = dtNewLOTList.AsEnumerable().Select(x => new
                {
                    LOTID = x.Field<string>("LOTID"),
                    PRODNAME = x.Field<string>("PRODNAME"),
                    SHIPTO_NAME = x.Field<string>("SHIPTO_NAME"),
                    CUST_TRNF_PGM_ID = x.Field<string>("CUST_TRNF_PGM_ID"),
                    B2BI_IF_GNRT_DTTM = x.Field<string>("B2BI_IF_GNRT_DTTM"),
                    ERR_MSG = x.Field<string>("ERR_MSG"),
                });

                DataTable dtResult = DataTableConverter.Convert(this.dgExceptLotList.ItemsSource).AsEnumerable().Union(PackCommon.queryToDataTable(query.ToList()).AsEnumerable()).CopyToDataTable();
                Util.GridSetData(this.dgExceptLotList, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// name         : C1Window_Loaded
        /// desc         : C1Window_Loaded
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] objParam = C1WindowExtension.GetParameters(this);
            if (objParam == null || objParam.Length <= 0)
            {
                return;
            }
            this.ShowLOTList(objParam[0]);
        }

        /// <summary>
        /// name         : btnClose_Click
        /// desc         : btnClose_Click
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// name         : dgExceptLotList_LoadedCellPresenter
        /// desc         : 기준에 맞지않는 LOTID Cell 색상 변경
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void dgExceptLotList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (dgExceptLotList == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);

                dgExceptLotList.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;
                    if (e.Cell.Row.Type != DataGridRowType.Item) return;

                    switch (e.Cell.Column.Name)
                    {
                        case "CUST_TRNF_PGM_ID":
                            {
                                if (e.Cell.Text.Equals("0"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                            }
                            break;
                        default:
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                            break;

                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}