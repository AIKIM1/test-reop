/*************************************************************************************
 Created Date : 2020.11.23
      Creator : 
   Decription : 충방전기 공정별 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.23  DEVELOPER : 박준규

**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_003 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_003()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
      
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetCboLineID();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                //if (checkBox.Count() == 0)
                //{
                //    Util.AlertMsg("ME_0268");  //호기를 하나이상 선택해주세요.
                //    Util.MessageValidation("SFU1261");
                //    return;
                //}

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LINE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LINE_ID"] = Util.NVC(cboLineID.SelectedItemsToString).Equals("") ? string.Empty : Util.NVC(cboLineID.SelectedItemsToString);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_RATE_TOTAL_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgFrmOpStatus, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCboLineID()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRQSTDT.Rows.Add(drnewrow);
                
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MB", "RQSTDT", "RSLTDT", dtRQSTDT);
                
                cboLineID.ItemsSource = DataTableConverter.Convert(result);
                cboLineID.CheckAll();
            }
            catch (Exception ex)
            {

            }
        }

        #endregion



    }
}
