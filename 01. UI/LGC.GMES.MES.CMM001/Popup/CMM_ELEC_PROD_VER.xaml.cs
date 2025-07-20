/*************************************************************************************
 Created Date : 2019.03.29
      Creator : 
   Decription : 계획버전 등록
--------------------------------------------------------------------------------------
 [Change History]



**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;


namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ELEC_PROD_VER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private object[] tmps = null;
        bool isSaved;

        public CMM_ELEC_PROD_VER()
        {
            InitializeComponent();
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
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            isSaved = false;

            SearchData();

        }
        #endregion

        #region Event

        private void FirstCheckIndex()
        {

            for (int i = 0; i < dgSeletedWOList.GetRowCount(); i++)
            {
                if (tmps[2].Equals(Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "WO_DETL_ID"))))
                {
                    DataTableConverter.SetValue(dgSeletedWOList.Rows[i].DataItem, "CHK", true);
                    dgSeletedWOList.ScrollIntoView(i, 0);
                    return;
                }
            }
        }


        private void InitCombo()
        {
            Set_Combo_Ver(cboPlanVer);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSeletedWOList.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgSeletedWOList.Rows[i].DataItem, "CHK", true);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSeletedWOList.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgSeletedWOList.Rows[i].DataItem, "CHK", false);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isValid())
                return;
            else
            {
                SaveData();
            }

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (isSaved)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private void Set_Combo_Ver(C1ComboBox cbo)
        {

            try
            {
                string tmpPRODID = string.Empty;

                for (int i = 0; i < dgSeletedWOList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        tmpPRODID = Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "PRODID"));
                    }
                }
                const string bizRuleName = "DA_PRD_SEL_CONV_RATE";
                string[] arrColumn = { "PRODID", "AREAID" };
                string[] arrCondition = { tmpPRODID, LoginInfo.CFG_AREA_ID };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private bool isValid()
        {
            bool bRet = false;
            for (int _iRow = 0; _iRow < dgSeletedWOList.Rows.Count; _iRow++)
            {
                if (cboPlanVer.SelectedValue == null)
                {
                    Util.Alert("SFU2050");  //계획버전을 선택하세요.
                    return bRet;
                }
            }
            bRet = true;
            return bRet;
        }

        private void selectCurrentWO()
        {
            for (int i = 0; i < dgSeletedWOList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "WO_DETL_ID")).Equals(Util.NVC(tmps[17])))
                {
                    DataTableConverter.SetValue(dgSeletedWOList.Rows[i].DataItem, "CHK", true);
                    dgSeletedWOList.ScrollIntoView(i, 0);
                    return;
                }
            }
        }

        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgSeletedWOList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("WO_DETL_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = tmps.Length == 4 ? Util.NVC(tmps[3]) : Process.ROLL_PRESSING;
                Indata["EQPTID"] = Util.NVC(tmps[1]);
                Indata["WO_DETL_ID"] = Util.NVC(tmps[2]);
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WORKORDER_MO_BY_EQPT_CWA", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        return;
                    }

                    Util.GridSetData(dgSeletedWOList, result, FrameOperation, true);
                    InitCombo();
                    FirstCheckIndex();
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void SaveData()
        {
            try
            {
                if (!Validation()) return;
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                        IndataTable.Columns.Add("PRODID", typeof(string));
                        IndataTable.Columns.Add("EQPTID", typeof(string));
                        IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));

                        for (int _iRow = 0; _iRow < dgSeletedWOList.Rows.Count; _iRow++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow Indata = IndataTable.NewRow();
                                Indata["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[_iRow].DataItem, "WO_DETL_ID"));
                                Indata["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[_iRow].DataItem, "PRODID"));
                                Indata["EQPTID"] = Util.NVC(tmps[1]);
                                Indata["PROD_VER_CODE"] = Util.NVC(cboPlanVer.SelectedValue);
                                Indata["USERID"] = LoginInfo.USERID;
                                IndataTable.Rows.Add(Indata);
                            }
                        }
                        if (IndataTable.Rows.Count != 0)
                        {
                            new ClientProxy().ExecuteService("DA_PRD_UPD_WORKORDER_MO_PROD_VER_CODE_CWA", "INDATA", null, IndataTable, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Visible;

                                if (ex != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                    return;
                                }
                                Util.MessageInfo("SFU1270", MessageResult =>
                                {
                                    this.DialogResult = MessageBoxResult.OK;
                                });

                                //this.dgSeletedWOList.ItemsSource = null;
                                isSaved = true;
                            });
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }

        }
        #endregion

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
            DataRow dtRow = (rb.DataContext as DataRowView).Row;

            for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
            {
                if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                {
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    InitCombo();
                }
                else
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
            }
        }

        private bool Validation()
        {
            if (Util.NVC(cboPlanVer.SelectedValue) == "SELECT" || cboPlanVer.SelectedIndex == 0 || Util.NVC(cboPlanVer.SelectedValue) == null)
            {
                Util.MessageInfo("SFU6031");
                return false;
            }
            else
            {
                return true;
            }
        }




        //private void dgSeletedWOList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        //{
        //    if (sender == null)
        //        return;

        //    RadioButton rb = sender as RadioButton;

        //  //  int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
        // //   DataRow dtRow = (rb.DataContext as DataRowView).Row;

        //    for (int i = 0; i < dgSeletedWOList.GetRowCount(); i++)
        //    {
        //        if(tmps[0].Equals(Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "PROID"))))
        //        {
        //            // DataTableConverter.SetValue(Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "CHK")), true);
        //            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);

        //        }

        //    }
        //}
    }
}
