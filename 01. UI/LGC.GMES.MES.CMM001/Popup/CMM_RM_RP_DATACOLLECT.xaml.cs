/*************************************************************************************
 Created Date : 2024.08.23
      Creator : 조성근
   Decription : 롤프레스 롤맵 불량정보 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2024.08.23  조성근 : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_RM_RP_DATACOLLECT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_RM_RP_DATACOLLECT : C1Window, IWorkArea
    {
        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private string _processCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _runLotId = string.Empty;
        private string _wipSeq = string.Empty;
        private decimal _laneQty = 0;

        private readonly Util _util = new Util();

        public bool IsUpdated { get; set; }

        public CMM_RM_RP_DATACOLLECT()
        {
            InitializeComponent();
        }
        #endregion

        #region Loaded Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] parameters = C1WindowExtension.GetParameters(this);

                if (parameters != null && parameters.Length > 0)
                {
                    _processCode = Util.NVC(parameters[0]);
                    _equipmentCode = Util.NVC(parameters[1]);
                    _runLotId = Util.NVC(parameters[2]);
                    _wipSeq = Util.NVC(parameters[3]);
                    _laneQty = Util.NVC_Decimal(parameters[4]);

                    InitializeControl();
                    GetDefectList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Method

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE")), "ZZZZZZZZZZ"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }

                        if (string.Equals(e.Cell.Column.Name, "RM_RESNQTY")) 
                        {
                            if(string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DIFF_YN").GetString(),"Y"))
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                            else
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.Background = null;
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        }
                    }
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region Method
        private void InitializeControl()
        {
            Util.gridClear(dgDefect);
            Util.gridClear(dgDefectBack);
        }

        private void GetDefectList()
        {
            try
            {
                const string bizRuleName = "BR_PRD_SEL_RM_RPT_DEFECT_RP";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _runLotId;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUT_PRODUCTION,OUT_BACK", ds);

                if(CommonVerify.HasTableInDataSet(dsResult))
                {
                    #region [Top, Back 영역 그리드 데이터 바인딩 및 소계 처리]                   

                    if (CommonVerify.HasTableRow(dsResult.Tables["OUT_BACK"]))
                    {
                        DataTable dtBack = dsResult.Tables["OUT_BACK"].Copy();

                        var query = dtBack.AsEnumerable().GroupBy(x => new
                        {
                            activityCode = x.Field<string>("ACTID"),
                            activityName = x.Field<string>("ACTNAME"),
                        }).Select(g => new
                        {
                            ActivityCode = g.Key.activityCode,
                            ActivityName = g.Key.activityName.Split(' ')[0] + " " + ObjectDic.Instance.GetObjectName("소계"),
                            ReasonQty = g.Sum(x => x.GetValue("MES_RESNQTY").GetDecimal()),
                            RollMapReasonQty = g.Sum(x => x.GetValue("RM_RESNQTY").GetDecimal()),
                            DifferenceYn = "N",
                            ActivityReasonCode = "ZZZZZZZZZZ",
                            Count = g.Count()
                        }).ToList();


                        if (query.Any())
                        {
                            foreach (var item in query)
                            {
                                DataRow dr = dtBack.NewRow();
                                dr["ACTID"] = item.ActivityCode;
                                dr["ACTNAME"] = item.ActivityName;
                                dr["RESNCODE"] = item.ActivityReasonCode;
                                dr["MES_RESNQTY"] = item.ReasonQty;
                                dr["RM_RESNQTY"] = item.RollMapReasonQty;
                                dr["DIFF_YN"] = item.DifferenceYn;
                                dtBack.Rows.Add(dr);
                            }
                        }

                        dtBack = (from t in dtBack.AsEnumerable()
                                  orderby t.Field<string>("ACTID") ascending, t.Field<string>("RESNCODE")
                                  select t).CopyToDataTable();

                        Util.GridSetData(dgDefectBack, dtBack, FrameOperation, true);
                    }
                    #endregion

                    #region [불량합계 그리드 데이터 바인딩 및 RollMap 데이터 생성]
                    if (CommonVerify.HasTableRow(dsResult.Tables["OUT_PRODUCTION"]))
                    {
                        decimal backDefectQty = 0;
                        decimal backLossQty = 0;

                        DataTable dt = dsResult.Tables["OUT_PRODUCTION"].Copy();
                        dt.Columns.Add(new DataColumn() { ColumnName = "TRGT_NAME", DataType = typeof(string) });

                        foreach (DataRow row in dt.Rows)
                        {
                            row["TRGT_NAME"] = ObjectDic.Instance.GetObjectName("MES");
                        }
                        dt.AcceptChanges();

                        if (CommonVerify.HasTableRow(dsResult.Tables["OUT_BACK"]))
                        {
                            DataTable dtBack = dsResult.Tables["OUT_BACK"].Copy();
                            backDefectQty = dtBack.AsEnumerable().Where(x => x.Field<string>("ACTID") == "DEFECT_LOT").Sum(s => s.GetValue("RM_RESNQTY").GetDecimal());
                            backLossQty = dtBack.AsEnumerable().Where(x => x.Field<string>("ACTID") == "LOSS_LOT").Sum(s => s.GetValue("RM_RESNQTY").GetDecimal());

                        }

                        //불량합계 상단 그리드 RollMap 데이터 생성
                        // ighkds77 수정
                        //RollMap 생산량 = 생산량(MES)
                        //RollMap 양품량 = 생산량(MES) - (불량(Back 합계) + LOSS(Back 합계) + 물품청구(MES))
                        //RollMap 불량 =  (Back 불량 합계) 
                        //RollMap LOSS =  (Back LOSS 합계) 
                        //RollMap 물품청구 =  물품청구(MES)

                        var query = dt.AsEnumerable().GroupBy(x => new
                        { }).Select(g => new
                        {
                            TargetName = ObjectDic.Instance.GetObjectName("ROLLMAP"),
                            
                            GoodQty = g.Sum(x => x.GetValue("WIPQTY_ED").GetDecimal()), //양품량(MES)
                            InputQty = g.Sum(x => x.GetValue("INPUT_QTY").GetDecimal()), //생산량(MES)
                            DefectQty =  backDefectQty,
                            LossQty = backLossQty,
                            ProductRequestQty = g.Sum(x => x.GetValue("CNFM_PRDT_REQ_QTY").GetDecimal()),
                            Count = g.Count()
                        }).ToList();

                        if (query.Any())
                        {
                            foreach (var item in query)
                            {
                                DataRow dr = dt.NewRow();
                                dr["TRGT_NAME"] = item.TargetName;
                                dr["INPUT_QTY"] = item.InputQty;
                                dr["WIPQTY_ED"] = item.InputQty - (item.DefectQty + item.LossQty + item.ProductRequestQty);
                                dr["CNFM_DFCT_QTY"] = item.DefectQty;
                                dr["CNFM_LOSS_QTY"] = item.LossQty;
                                dr["CNFM_PRDT_REQ_QTY"] = item.ProductRequestQty;
                                dt.Rows.Add(dr);
                            }
                        }

                        Util.GridSetData(dgDefect, dt, FrameOperation, true);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private double GetDefectSum(decimal x, double y)
        {
            double defectSum = 0;
            if (y.Equals(0)) return defectSum;

            try
            {
                return x.GetDouble() * y;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion
    }
}
