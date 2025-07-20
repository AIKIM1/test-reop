/*************************************************************************************
 Created Date : 2016.09.03
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Lamination 공정진척 화면 - 실적조회 팝업
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
    /// ASSY001_004_SEARCH_RSLT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_004_SEARCH_RSLT : C1Window, IWorkArea
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
        public ASSY001_004_SEARCH_RSLT()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            dtpDateFrom.Text = System.DateTime.Now.ToLongDateString();
            dtpDateTo.Text = System.DateTime.Now.ToLongDateString();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            
            String[] sFilter = { _LineID, Process.LAMINATION, _EqptID };
            _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "EQUIPMENT_BY_EQPTID");
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
            InitializeControls();
            InitCombo();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetProdList();
        }

        private void dgProdLot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgProdLot.GetCellFromPoint(pnt);

            if (cell != null)
            {
                string sLot = Util.NVC(DataTableConverter.GetValue(dgProdLot.Rows[cell.Row.Index].DataItem, "LOTID"));
                string sWipSeq = Util.NVC(DataTableConverter.GetValue(dgProdLot.Rows[cell.Row.Index].DataItem, "WIPSEQ"));

                GetInputInfo(sLot, sWipSeq);
                GetOutInfo(sLot);
            }
        }

        private void dgOutMaz_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgOutMaz.GetCellFromPoint(pnt);

            if (cell != null)
            {
                string sLot = Util.NVC(DataTableConverter.GetValue(dgOutMaz.Rows[cell.Row.Index].DataItem, "LOTID"));
                
                GetInputInfoOfMaz(sLot);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private void GetProdList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIPINFO_BY_PERIOD_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEqpt.SelectedValue;
                newRow["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_BY_PERIOD_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgProdLot.ItemsSource = DataTableConverter.Convert(searchResult);
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

        private void GetInputInfo(string sLamiLot, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = _Biz.GetDA_PRD_SEL_RUN_IN_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLamiLot;
                newRow["WIPSEQ"] = sWipSeq;
                //newRow["INPUT_LOT_STAT_CODE"] = "ATTACH";
                
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_IN_LOT_LIST_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgInput.ItemsSource = DataTableConverter.Convert(searchResult);
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

        private void GetOutInfo(string sLamiLot)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_MAGAZINE_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = sLamiLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_LOT_LIST_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgOutMaz.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetInputInfoOfMaz(string sMazId)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_IN_MTRL_OF_MAZ_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sMazId;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgMazInput.ItemsSource = DataTableConverter.Convert(searchResult);
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

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearch);

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
