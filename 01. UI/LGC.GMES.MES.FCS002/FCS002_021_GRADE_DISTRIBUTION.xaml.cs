/*************************************************************************************
 Created Date : 2023.02.02
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.02  DEVELOPER : Initial Created.

 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using C1.WPF.C1Chart;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_021_GRADE_DISTRIBUTION : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        private DataTable dtRslt;
        DataTable gradeGrp = new DataTable();

        private string _sCSTID;
        private string _sLOTID;

        public string CSTID
        {
            set { this._sCSTID = value; }
            get { return this._sCSTID; }
        }

        public string LOTID
        {
            set { this._sLOTID = value; }
            get { return this._sLOTID; }
        }
        public FCS002_021_GRADE_DISTRIBUTION()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                txtCSTID.Text = Util.NVC(tmps[0]);
                txtLOTID.Text = Util.NVC(tmps[1]);
            }
            else
            {
                txtCSTID.Text = "";
                txtLOTID.Text = "";
            }
            txtCSTID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
            txtLOTID.Style = Application.Current.Resources["Content_InputForm_ReadOnlyTextBoxStyle"] as Style;
            if (string.IsNullOrEmpty(txtCSTID.Text) && string.IsNullOrEmpty(txtCSTID.Text)) return;
            GetList();
            GetChart();
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (string.IsNullOrEmpty(txtCSTID.Text)) dr["CSTID"] = null;
                else
                {
                    dr["CSTID"] = Util.NVC(txtCSTID.Text);
                }

                if (string.IsNullOrEmpty(txtLOTID.Text))
                    dr["LOTID"] = null;
                else
                    dr["LOTID"] = Util.NVC(txtLOTID.Text);
                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_GRADE_MB", "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgGrade);
                Util.GridSetData(dgGrade, dtRslt, this.FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetChart()
        {
            //Chart Setting
            SetChartData();
            ModifyChart();
        }

        public void SetChartData()
        {
            DataTable gradeGrouped = dtRslt.AsEnumerable().GroupBy(g => new
            {
                GRADE_CD = g.Field<string>("GRADE_CD"),
                GRADE_CNT = g.Field<double>("GRADE_CNT")
            }).Select(s => new
            {
                GRADE_CD = s.Key.GRADE_CD,
                GRADE_CNT = s.Key.GRADE_CNT
            }).ToList().ToDataTable();

            gradeGrp = gradeGrouped;
            chrtGrade.Data.ItemsSource = DataTableConverter.Convert(gradeGrp);
        }
        private void ModifyChart()
        {

            chrtGrade.View.AxisX.FontWeight = FontWeights.Bold;
            chrtGrade.View.AxisX.FontSize = 12;
            chrtGrade.View.AxisX.MajorGridStroke = new SolidColorBrush(Colors.Transparent);

            chrtGrade.View.AxisX.Min = -1;
            chrtGrade.View.AxisX.Max = gradeGrp.Rows.Count;

            chrtGrade.View.AxisY.FontWeight = FontWeights.Bold;
            chrtGrade.View.AxisY.FontSize = 12;
            chrtGrade.View.AxisY.MajorGridStroke = new SolidColorBrush(Colors.LightGray);
        }

        #endregion

        #region [Event]
        //ToolTip 적용
        private void DataSeries_PlotElementLoaded(object sender, EventArgs e)
        {
            PlotElement pe = sender as PlotElement;

            pe.MouseEnter += (send, ea) =>
            {
                chrtGrade.ToolTip = pe.DataPoint.Name + ObjectDic.Instance.GetObjectName("등급") + " " +
                ObjectDic.Instance.GetObjectName("수량") + " : " + pe.DataPoint;
            };
        }
        #endregion

    }
}
