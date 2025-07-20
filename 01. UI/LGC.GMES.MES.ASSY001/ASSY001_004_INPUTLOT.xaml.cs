/*************************************************************************************
 Created Date : 2020.01.20
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Lamination 공정진척 화면 - 투입 Lot조회
                -> 노칭 공정 Out Lot의 어깨선 불량에 따른 양/음 전극 구분('A','C') 확인용
--------------------------------------------------------------------------------------
 [Change History]
  2020.01.21  최상민         CSR :                        내용: Initial Created.
  
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

    public partial class ASSY001_004_INPUTLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _ProcID = string.Empty;
        private string _EqptID = string.Empty;
        
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
        public ASSY001_004_INPUTLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 3)
            {
                _LineID = Util.NVC(tmps[0]);
                _ProcID = Util.NVC(tmps[1]);
                _EqptID = Util.NVC(tmps[2]);
            }
            else
            {
                _LineID = "";
                _ProcID = "";
                _EqptID = "";
            }
            ApplyPermissions();
            
            GetWaitLot();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {               
                this.Focus();
            })); 
        }
        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
           
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
            
                if (sender == null)                   
                return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = System.DateTime.Now.ToLongDateString();
                    dtPik.SelectedDateTime = System.DateTime.Now;
                    //Util.Alert("SFU1698");      //시작일자 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU1698");
                    e.Handled = false;
                    this.Focus();
                    return;
                }
              
                this.Focus();
            })); 
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

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EQSGID"] = _LineID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = _EqptID;                

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPHISTORY_INPUT_CONTINUITY_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgWaitLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInputLot, searchResult, null, true);

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
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #endregion






    }
}
