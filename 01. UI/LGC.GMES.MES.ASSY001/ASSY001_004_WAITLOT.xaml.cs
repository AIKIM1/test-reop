/*************************************************************************************
 Created Date : 2016.09.03
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Lamination 공정진척 화면 - 대기LOT조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.03  INS 김동일K : Initial Created.
  
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
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_004_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_004_WAITLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
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
        public ASSY001_004_WAITLOT()
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
            rdoCType.IsChecked = true;
            GetWaitLot();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void rdoCType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

        private void rdoAType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

        private void rdoLType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

         private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitPancakePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                string sPancakeID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

                if (!sPancakeID.Equals(""))
                {
                    List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                    DataTable dtRslt = GetThermalPaperPrintingInfo(sPancakeID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;


                    Dictionary<string, string> dicParam = new Dictionary<string, string>();


                    dicParam.Add("PANCAKEID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("TOT_QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("REMAIN_QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("NOTE", "");
                    dicParam.Add("PRINTQTY", "1");  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN(dicParam);
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = null;
                        Parameters[1] = Process.LAMINATION;
                        Parameters[2] = _LineID;
                        Parameters[3] = _EqptID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "N";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(printWaitPancake_Closed);

                        print.ShowModal();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitLot()
        {
            try
            {
                ShowLoadingIndicator();

                string sElect = string.Empty;
                if (rdoAType != null && rdoAType.IsChecked.HasValue && (bool)rdoAType.IsChecked)
                    sElect = "A";
                else if (rdoCType != null && rdoCType.IsChecked.HasValue && (bool)rdoCType.IsChecked)
                    sElect = "C";
                else if (rdoLType != null && rdoLType.IsChecked.HasValue && (bool)rdoLType.IsChecked)
                    sElect = "B";
                else
                    sElect = "";

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.LAMINATION;
                newRow["EQPTID"] = _EqptID;
                newRow["EQSGID"] = _LineID;
                newRow["ELECTYPE"] = sElect;
                newRow["LOTID"] = txtWaitPancakeLot == null ? "" : txtWaitPancakeLot.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgWaitLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitLot, searchResult, null, true);

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

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_REWRK", "INDATA", "OUTDATA", inTable);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if(loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void printWaitPancake_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI_REMAIN;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #endregion






    }
}
