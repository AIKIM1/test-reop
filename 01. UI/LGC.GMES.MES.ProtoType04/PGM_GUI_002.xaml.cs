/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_002 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        public PGM_GUI_002()
        {
            
            InitializeComponent();
            dtpDateMonth.SelectedDateTime = System.DateTime.Now;
            InitCombo();
        }


        #endregion
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #region Initialize
        private void InitGrid()
        {
            Util.gridClear(dgList);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboLine };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboProcess };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);

            //공정
            C1ComboBox[] cbProcessParent = { cboLine };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);


        }
        #endregion

        #region Event

        private void dgList_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetGridDate();
            }));

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            //if (dgList.CurrentColumn.DisplayIndex < 8)
            //{
            //    return;
            //}

            //string _sDay = dgList.CurrentColumn.Name;

            //// 우선순위 조정
            //PGM_GUI_002_Sorting _Sorting = new PGM_GUI_002_Sorting();
            //_Sorting._sEqpt = "Coater1";
            //_Sorting._sMonth = string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime); ;
            //_Sorting._sDay = dgList.CurrentColumn.Name;

            //if (_Sorting != null)
            //{
            //    _Sorting.ShowModal();
            //    _Sorting.CenterOnScreen();
            //}
            //#region 
            ////string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            ////string MAINFORMNAME = "PGM_GUI_002_Sorting";

            ////Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            ////Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            ////object obj = Activator.CreateInstance(targetType);

            ////C1Window popup = obj as C1Window;
            ////if (popup != null)
            ////{
            ////    popup.Closed -= popup_Closed;
            ////    popup.Closed += popup_Closed;
            ////    popup.ShowModal();
            ////    popup.CenterOnScreen();
            ////}
            //#endregion
        }
        #endregion

        #region Mehod

        private void SetGridDate()
        {
            System.DateTime dtNow = new DateTime(dtpDateMonth.SelectedDateTime.Year, dtpDateMonth.SelectedDateTime.Month, 1); //new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            for (int i = 1; i <= dtNow.AddMonths(1).AddDays(-1).Day; i++)
            {
                List<string> sHeader = new List<string>() { dtNow.Year.ToString() + "." + dtNow.Month.ToString(), dtNow.AddDays(i - 1).ToString("ddd"), i.ToString() };
                Util.SetGridColumnNumeric(dgList, i.ToString(), sHeader, i.ToString(), false, false, false, true, 100, HorizontalAlignment.Right, Visibility.Visible);
            }
        }

        private void SearchData()
        {
            for (int i = dgList.Columns.Count; i-- > 8;)
            {
                    dgList.Columns.RemoveAt(i);
            }

            SetGridDate();

            string _ValueToMonth = string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime);
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("MONTH", typeof(string));
            IndataTable.Columns.Add("AREA", typeof(string));
            IndataTable.Columns.Add("LINE", typeof(string));
            IndataTable.Columns.Add("PROCESS", typeof(string));
            IndataTable.Columns.Add("TYPE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["MONTH"] = _ValueToMonth;
            Indata["AREA"] = cboArea.SelectedValue.ToString();
            Indata["LINE"] = cboLine.SelectedValue.ToString();
            Indata["PROCESS"] = cboProcess.SelectedValue.ToString();
            Indata["TYPE"] = "TYPE"; // cboType.SelectedValue.ToString();
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_PRODUCTIONPLAN", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    return;
                }

                dgList.ItemsSource = DataTableConverter.Convert(result);
            });
        }

        void popup_Closed(object sender, EventArgs e)
        {


        }

        private void Sorting_Closed(object sender, EventArgs e)
        {
            PGM_GUI_002_Sorting runStartWindow = sender as PGM_GUI_002_Sorting;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                
            }
        }

#endregion

    }
}
