/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF.DataGrid;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ASSY_PLAN_CELL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_PLAN_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _AREAID = string.Empty;
        private string _PRODID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _MODLID = string.Empty;

        Util _Util = new Util();
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
        public CMM_ASSY_PLAN_CELL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }
            _AREAID = Util.NVC(tmps[0]);
            _EQSGID = Util.NVC(tmps[1]);
            _PRODID = Util.NVC(tmps[2]);
            _MODLID = Util.NVC(tmps[3]);

        }
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU1241", (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        SavePlanCell();
                        this.DialogResult = MessageBoxResult.OK;
                    }
                });

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void SavePlanCell()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DT", typeof(string));
                dtRqst.Columns.Add("END_DT", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("QTY", typeof(double));

                DataRow Indata = dtRqst.NewRow();
                Indata["FROM_DT"] = dtpFromDate.SelectedDateTime.ToShortDateString();
                Indata["END_DT"] = dtpToDate.SelectedDateTime.ToShortDateString(); ;
                Indata["SHOPID"] = _AREAID;
                Indata["EQSGID"] = _EQSGID;
                Indata["QTY"] = txtCELL.Text;

                dtRqst.Rows.Add(Indata);

                string vBizRule = string.Empty;

                new ClientProxy().ExecuteService(vBizRule, "INDATA", "RSLTDT", dtRqst, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.AlertByBiz(vBizRule, ex.Message, ex.ToString());
                        return;
                    }

                    Util.AlertInfo("SFU1270"); //정상처리되었습니다.
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
