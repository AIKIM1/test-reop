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
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_017_LOAD : C1Window, IWorkArea
    {
        public string pSave_Seqno = string.Empty;

        #region Declaration & Constructor 
        public BOX001_017_LOAD()
        {
            InitializeComponent();

            Initialize();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            #region Combo Setting
            CommonCombo combo = new CommonCombo();

            //combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.ALL);
            C1ComboBox[] cboToChild = { cboTransLoc };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboToChild, sCase: "AREA");

            //combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.SELECT);
            C1ComboBox[] cboCompParent = { cboArea };
            combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboCompParent);

            #endregion

        }
        #endregion

        #region Event
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgLoadList, "CHK");

                if (drChk.Length <= 0)
                {
                    //"선택된 항목이 없습니다."
                    Util.MessageValidation("SFU1651");
               //     LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                pSave_Seqno = drChk[0]["SAVE_SEQNO"].ToString();

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                string sArea = string.Empty;
                string sSHIPTO_ID = string.Empty;

                if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("ALL"))
                {
                    sArea = null;
                }
                else
                {
                    sArea = cboArea.SelectedValue.ToString();
                }

                if (cboTransLoc.SelectedIndex < 0 || cboTransLoc.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    sSHIPTO_ID = null;
                }
                else
                {
                    sSHIPTO_ID = cboTransLoc.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("PACK_TMP_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PACK_TMP_TYPE_CODE"] = "RETURN_CELL";
                dr["SHIPTO_ID"] = sSHIPTO_ID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_LOAD_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgLoadList);
                dgLoadList.ItemsSource = DataTableConverter.Convert(SearchResult);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        #region Mehod

        #endregion

        private void dgLoadListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgLoadList.BeginEdit();
                //    dgLoadList.ItemsSource = DataTableConverter.Convert(dt);
                //    dgLoadList.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                //row 색 바꾸기
                dgLoadList.SelectedIndex = idx;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
