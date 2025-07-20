/*************************************************************************************
 Created Date :   
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  





 
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

namespace LGC.GMES.MES.ELEC001
{
    public partial class PGM_GUI_016_EQPEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string PRODID = string.Empty;
        private string WORKDATE = string.Empty;
        private string LOTID = string.Empty;
        private string STATUS = string.Empty;
        private string EQPTID = string.Empty;

        private string saveQty = string.Empty;
        private string endtime = string.Empty;

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_016_EQPEND()
        {
            InitializeComponent();
        }

      
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 5)
            {
                EQPTID = Util.NVC(tmps[0]);
                LOTID = Util.NVC(tmps[1]);
                STATUS = Util.NVC(tmps[2]);
                PRODID = Util.NVC(tmps[3]);
                WORKDATE = Util.NVC(tmps[4]);
                endtime = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm");
            }
            else
            {
                EQPTID = "";
                LOTID = "";
                STATUS = "";
                PRODID = "";
                WORKDATE = "";
            }
            ApplyPermissions();

            SearchInputLot();
            SearchResultLot(1, LOTID);

        }
        private void dgResultLot_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (!CheckRsltQty(e.Cell.Row.Index))
            {
                return;
            }
            Util.Alert(e.Cell.Row.Index.ToString());
            double ResultQty = double.Parse(e.Cell.Value.ToString());
            double inputQty = double.Parse(saveQty);

            DataTableConverter.SetValue(dgInputLot.Rows[0].DataItem, "REMAINQTY", ResultQty - inputQty);

        }

        #region[button]
        private void btnEqpend_Click(object sender, RoutedEventArgs e)
        {
            Util.Alert("비즈룰 탐");

            SearchInputLot();
            SearchResultLot(2, Util.NVC(dgInputLot.Rows[0].DataItem, "LOTID"));

        }
        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #endregion

        #region Method
        private void SearchInputLot()
        {
            DataTable inTable = _Biz.GetDA_PRD_SEL_INPUTLOT_INFO_NT();

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["LOTID"] = LOTID;

            inTable.Rows.Add(newRow);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUTLOT_TAP", "RQSTDT", "RSLTDT", inTable);
            dgInputLot.ItemsSource = DataTableConverter.Convert(result);

            saveQty = Util.NVC(result.Rows[0]["REMAINQTY"]);

        }
        private void SearchResultLot(int div, string lotid)
        {
            try
            {
                DataTable result = null;
                if (div == 1)
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("PR_LOTID", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["PR_LOTID"] = lotid;

                    inTable.Rows.Add(newRow);

                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESULTLOT_TAP", "RQSTDT", "RSLTDT", inTable);

                    

                }
                else if (div == 2)
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["LOTID"] = lotid;

                    inTable.Rows.Add(newRow);

                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RESULTLOT2_TAP", "RQSTDT", "RSLTDT", inTable);


                }

                result.Columns.Add("ENDTIME", typeof(string));
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    result.Rows[i]["ENDTIME"] = endtime;
                }

                dgResultLot.ItemsSource = DataTableConverter.Convert(result);
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }

        private bool CheckRsltQty(int checkedIndex)
        {
            if (Util.NVC(DataTableConverter.GetValue(dgResultLot.Rows[checkedIndex].DataItem, "WIPQTY")).Equals(0))
            {
                DataTableConverter.SetValue(dgResultLot.Rows[checkedIndex].DataItem, "WIPQTY", "");
                return false;
            }
            return true;
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnEqpend);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

    }
}
