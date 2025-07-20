using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_PRDT_GPLM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_PRDT_GPLM : C1Window, IWorkArea
    {
        #region Initialize
        private string _PRODID = string.Empty;
        private string _REQSTATUS = string.Empty;

        public IFrameOperation FrameOperation { get; set; }

        public CMM_ASSY_PRDT_GPLM()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            _PRODID = Util.NVC(tmps[0]);
            _REQSTATUS = Util.NVC(tmps[1]);

            // 보안을 위해서 설정
            dgProduct.KeyDown += dg_KeyDown;
            dgProduct.ClipboardCopyMode = C1.WPF.DataGrid.DataGridClipboardMode.None;
            dgProduct.ContextMenu.Visibility = Visibility.Collapsed;

            GetProductGplmData();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                C1DataGrid dg = sender as C1DataGrid;
                DataGridCell dgc = dg.CurrentCell;

                if (dg != null && dgc != null)
                {
                    Clipboard.SetText("");
                }
            }
        }
        #endregion
        #region Biz Call
        private void GetProductGplmData()
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("PRODID", typeof(string));
            IndataTable.Columns.Add("REQSTATUS", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["PRODID"] = _PRODID;
            Indata["REQSTATUS"] = _REQSTATUS;
            Indata["USERID"] = LoginInfo.USERID;
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_CONV_RATE_GPLM_ASSY", "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                        throw searchException;

                    //if (result.Columns.Contains("USERTYPE"))
                    //{
                    //    if (!Util.NVC(result.Rows[0]["USERTYPE"]).Equals("G"))
                    //        return ;
                    //}

                    if (result == null || result.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3696");  // 해당 모델의 작업지시서 LINK정보가 존재하지 않습니다.
                        return;
                    }

                    Util.GridSetData(dgProduct, result, FrameOperation, false);
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }
        #endregion
    }
}
