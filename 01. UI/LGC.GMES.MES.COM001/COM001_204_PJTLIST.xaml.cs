/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_204_PJTLIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor      
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string PRJ
        {
            get
            {
                return sPRJ;
            }

            set
            {
                sPRJ = value;
            }
        }

        //public string PRODID
        //{
        //    get
        //    {
        //        return sPRODID;
        //    }

        //    set
        //    {
        //        sPRODID = value;
        //    }
        //}

        private string sPRJ = "";
        //private string sPRODID = "";
        private string sAreaid = "";
        private string sWhID = "";


        public COM001_204_PJTLIST()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {

                        sAreaid = Util.NVC(dtText.Rows[0]["AREAID"]);
                        sWhID = Util.NVC(dtText.Rows[0]["WH_ID"]);

                        setWOInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgPrjListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                int iwoPRJIndex = Util.gridFindDataRow(ref dgPjtList, "CHK", "1", false);
                if (iwoPRJIndex == -1)
                {
                    return;
                }

                PRJ = Util.NVC(DataTableConverter.GetValue(dgPjtList.Rows[iwoPRJIndex].DataItem, "PRJ"));
                //PRODID = DataTableConverter.GetValue(dgPjtList.Rows[iwoPRJIndex].DataItem, "PRODID").ToString();

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Mehod

        private void setWOInfo()
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("WH_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = sAreaid;
                Indata["WH_ID"] = sWhID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_BY_PRJ", "RQSTDT", "RSLTDT", IndataTable);

                //dgPlanWorkorderList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgPjtList, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.AlertByBiz("DA_PRD_SEL_STOCK_BY_PRJ", ex.Message, ex.ToString());
            }
        }


        #endregion
    }
}
