/*************************************************************************************
 Created Date : 2017.12.08
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - WORKORDER 이전공정 재공조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.08  : Initial Created.
  
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_WORKORDER_WIPLIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_WORKORDER_WIPLIST : C1Window, IWorkArea
    {
        private string _WOID = string.Empty;
        private string _PROCID = string.Empty;
        private string _AREAID = string.Empty;
        private string _EQSGID = string.Empty;

        Util _Util = new Util();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_WORKORDER_WIPLIST()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 1)
            {
                _WOID = tmps[0].ToString();
                _AREAID = tmps[1].ToString();
                _EQSGID = tmps[2].ToString();
                _PROCID = tmps[3].ToString();
            }
            else
            {
                _WOID = "";
                _PROCID = "";
                _AREAID = "";
                _EQSGID = "";
            }

            GetWorkorderWIPLIST();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetWorkorderWIPLIST()
        {
            string ValueToProcid = string.Empty;
            switch (_PROCID)
            {
                case "A7000": // 라미
                    ValueToProcid = "A6000,A5000";  // VD
                    break;
                case "A8000":  // 폴딩
                    ValueToProcid = "A7000"; // 라미
                    break;
                case "A9000":  // 패키징
                    ValueToProcid = "A8000"; // 폴딩
                    break;
            }

            DataTable searchConditionTable = new DataTable();
            searchConditionTable.Columns.Add("LANGID", typeof(string));
            searchConditionTable.Columns.Add("WOID", typeof(string));
            searchConditionTable.Columns.Add("AREAID", typeof(string));
            searchConditionTable.Columns.Add("EQSGID", typeof(string));
            searchConditionTable.Columns.Add("PROCID", typeof(string));

            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["WOID"] = _WOID;
            searchCondition["AREAID"] = _AREAID;
            //searchCondition["EQSGID"] = _EQSGID;
            searchCondition["PROCID"] = ValueToProcid;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_BY_WOID", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.AlertByBiz("DA_PRD_SEL_WIP_BY_WOID", searchException.Message, searchException.ToString());
                        return;
                    }

                    Util.GridSetData(dgWIP, searchResult, FrameOperation, true);
                    if (searchResult.Rows.Count > 0)
                    {
                        string[] sColumnName = new string[] { "PROCNAME", "EQSGNAME", "MODLID" };
                        _Util.SetDataGridMergeExtensionCol(dgWIP, sColumnName, DataGridMergeMode.VERTICAL);
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                }
                finally
                {
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        private void GetDetailLot(string sProcId, string sEqsgid, string sProdId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));

                DataTable dtRslt = new DataTable();

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _AREAID;
                dr["EQSGID"] = sEqsgid;
                dr["PROCID"] = sProcId;
                dr["PRODID"] = sProdId;
                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_DETAIL_BY_WOID", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "PROCNAME", "EQSGNAME" };
                    _Util.SetDataGridMergeExtensionCol(dgLotList, sColumnName, DataGridMergeMode.VERTICAL);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgWIP_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgWIP_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "EQSGID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgWIP_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgWIP.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("PRODID") )
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }
    }
}
