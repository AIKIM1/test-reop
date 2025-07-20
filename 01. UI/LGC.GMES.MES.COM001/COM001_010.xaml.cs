/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_010()
        {
            
            InitializeComponent();
            InitCombo();
        }


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        private void btnSearchHold_Click(object sender, RoutedEventArgs e)
        {
            getTrayList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("TRAY_TYPE_CD", typeof(string));
            dtRqst.Columns.Add("LINE_ID", typeof(string));
            dtRqst.Columns.Add("USE_YN", typeof(string));


            foreach (DataRowView row in DataGridHandler.GetModifiedItems(dgTrayList))
            {
                if (row["CHK"].ToString().Equals("1"))
                {
                    DataRow dr = dtRqst.NewRow();
                    dr["TRAY_TYPE_CD"] = row["TRAY_TYPE_CD"];
                    dr["LINE_ID"] = row["LINE_ID"];
                    dr["USE_YN"] = row["USE_YN"];

                    dtRqst.Rows.Add(dr);
                }
            }
            if (dtRqst.Rows.Count > 0) {
                //활성화 리얼로 접속함으로 운영후에 실행가능하도록할것
                DataTable dtRslt = null;
                //if (Util.GetCondition(cboArea).Equals("A1"))
                //{
                //    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("UPD_TM_TRAY_TYPE_EMPTY", "INDATA", "OUTDATA", dtRqst);
                //}
                //else if (Util.GetCondition(cboArea).Equals("A2"))
                //{
                //    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("UPD_TM_TRAY_TYPE_EMPTY", "INDATA", "OUTDATA", dtRqst);
                //}

            }
        }

       

        private void C1ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            C1ComboBox cbo = sender as C1ComboBox;
            DataTable dt = dgTrayList.Resources["COMBOITEM"] as DataTable;
            cbo.ItemsSource = DataTableConverter.Convert(dt);
            cbo.SelectedValueChanged -= C1ComboBox_SelectedValueChanged;
            cbo.SelectedValueChanged += C1ComboBox_SelectedValueChanged;
        }

        private void C1ComboBox_Unloaded(object sender, RoutedEventArgs e)
        {
            C1ComboBox cbo = sender as C1ComboBox;
            cbo.ItemsSource = null;
            cbo = null;
        }

        private void C1ComboBox_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cbo = sender as C1ComboBox;
            System.Data.DataRowView dr = cbo.DataContext as System.Data.DataRowView;
            dr["CHK"] = true;
        }
        #endregion

        #region Mehod
        private void getTrayList()
        {
            try
            {
                SetGridCboItem();
                DataTable dtRslt = null;
                if (Util.GetCondition(cboArea).Equals("A1"))
                {
                    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("GET_AGING_TRAY_EMPTY_GMES", null, "OUTDATA", null);
                }
                else if (Util.GetCondition(cboArea).Equals("A2") || Util.GetCondition(cboArea).Equals("S2"))
                {
                    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("GET_AGING_TRAY_EMPTY_GMES", null, "OUTDATA", null);
                }
                Util.gridClear(dgTrayList);

                DataColumn dcChk = new DataColumn("CHK", typeof(Int16));
                dcChk.DefaultValue = 0;
                dtRslt.Columns.Add(dcChk);
                dgTrayList.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetGridCboItem()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            DataRow newRow = null;

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "Y", "Y" };
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow.ItemArray = new object[] { "N", "N" };
            dt.Rows.Add(newRow);

            if (dgTrayList.Resources.Contains("COMBOITEM"))
            {
                dgTrayList.Resources.Remove("COMBOITEM");
            }
            dgTrayList.Resources.Add("COMBOITEM", dt);
        }


        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            String[] sFilter = { "A040" };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter:sFilter);
        }

        #endregion
    }
}
