/*************************************************************************************
 Created Date : 2016.09.10
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Lamination 공정진척 화면 - 예약List
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.10  INS 김동일K : Initial Created.
  
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
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Data;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_004_IN_RSV_LIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_004_IN_RSV_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
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

        public ASSY001_004_IN_RSV_LIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
            }
            ApplyPermissions();
            GetResvList();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            if (!CanInReplace())
                return;

            ASSY001_004_PAN_REPLACE wndReplace = new ASSY001_004_PAN_REPLACE();
            wndReplace.FrameOperation = FrameOperation;

            if (wndReplace != null)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgReplace, "CHK");

                object[] Parameters = new object[6];
                Parameters[0] = _LineID;
                Parameters[1] = _EqptID;
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgReplace.Rows[idx].DataItem, "INPUT_LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgReplace.Rows[idx].DataItem, "WIPSEQ"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgReplace.Rows[idx].DataItem, "INPUT_QTY"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgReplace.Rows[idx].DataItem, "POSITION"));
                C1WindowExtension.SetParameters(wndReplace, Parameters);

                wndReplace.Closed += new EventHandler(wndReplace_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndReplace.ShowModal()));
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgReplace_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgReplace.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Row.Index < 0)
                    return;

                if (Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "CHK")).Equals(""))
                {
                    DataTableConverter.SetValue(cell.Row.DataItem, "CHK", 0);
                }
                else
                {
                    if(Util.NVC(DataTableConverter.GetValue(cell.Row.DataItem, "SUBLOTID")).Equals(""))
                    {
                        DataTableConverter.SetValue(cell.Row.DataItem, "CHK", 0);
                        //Util.Alert("선택 할 수 없습니다.");
                        Util.MessageValidation("SFU1628");
                        return;
                    }

                    //선택값 셋팅
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgReplace.Rows)
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                    }

                    DataTableConverter.SetValue(cell.Row.DataItem, "CHK", 1);
                }
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetResvList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_RESV_LOT_LIST_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.LAMINATION;
                newRow["EQPTID"] = _EqptID;
                newRow["EQSGID"] = _LineID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_RESV_LOT_LIST_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgReplace.ItemsSource = DataTableConverter.Convert(searchResult);

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

        #endregion

        #region [PopUp Event]
        private void wndReplace_Closed(object sender, EventArgs e)
        {
            ASSY001_004_PAN_REPLACE window = sender as ASSY001_004_PAN_REPLACE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #region [Validation]
        private bool CanInReplace()
        {
            bool bRet = false;

            if (dgReplace.ItemsSource == null || dgReplace.Rows.Count < 1)
                return bRet;

            if (_Util.GetDataGridCheckFirstRowIndex(dgReplace, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReplace);

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

        #endregion


    }
}
