/*************************************************************************************
 Created Date : 2017.01.24
      Creator : 
   Decription : 계획버전 등록
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.24  DEVELOPER : Initial Created.


**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_002_PROD_VER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private object[] tmps = null;
        bool isSaved;

        public COM001_002_PROD_VER()
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
            InitCombo();
            SearchData();
        }
        #endregion

        #region Event
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
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["PRODID"] = Util.NVC(tmps[10]);
                drnewrow["AREAID"] = Util.NVC(tmps[4]);
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CONV_RATE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    cbo.SelectedIndex = 0;
                });
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
                IndataTable.Columns.Add("PLANSDATE", typeof(string));
                IndataTable.Columns.Add("PLANEDATE", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("DEMAND_TYPE", typeof(string));
                IndataTable.Columns.Add("PRJT_NAME", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("MODLID", typeof(string));
                IndataTable.Columns.Add("LVL", typeof(string));
                IndataTable.Columns.Add("ROLL_TYPE", typeof(string));
                IndataTable.Columns.Add("FP_DETL_PLAN_STAT_CODE", typeof(string));
                IndataTable.Columns.Add("CNFM_FLAG", typeof(string));
                IndataTable.Columns.Add("WO_STAT_CODE", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = Util.NVC(tmps[0]);
                Indata["PLANSDATE"] = Util.NVC(tmps[1]);
                Indata["PLANEDATE"] = Util.NVC(tmps[2]);
                Indata["SHOPID"] = Util.NVC(tmps[3]);
                Indata["AREAID"] = Util.NVC(tmps[4]);
                Indata["EQSGID"] = Util.NVC(tmps[5]);
                Indata["PROCID"] = Util.NVC(tmps[6]);
                Indata["EQPTID"] = Util.NVC(tmps[7]);
                Indata["DEMAND_TYPE"] = Util.NVC(tmps[8]);
                Indata["PRJT_NAME"] = Util.NVC(tmps[9]);
                Indata["PRODID"] = Util.NVC(tmps[10]);
                Indata["MODLID"] = Util.NVC(tmps[11]);
                Indata["LVL"] = Util.NVC(tmps[12]);
                Indata["ROLL_TYPE"] = Util.NVC(tmps[13]);
                Indata["FP_DETL_PLAN_STAT_CODE"] = Util.NVC(tmps[14]);
                Indata["CNFM_FLAG"] = Util.NVC(tmps[15]);
                Indata["WO_STAT_CODE"] = Util.NVC(tmps[16]);
                Indata["WOID"] = string.Empty;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_FP_DETL_PLAN", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        return;
                    }

                    Util.GridSetData(dgSeletedWOList, result, FrameOperation, true);
                    if (!String.IsNullOrEmpty(Util.NVC(tmps[17])))
                    {
                        selectCurrentWO();
                        tmps[17] = "";
                    }
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
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                        IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));

                        for (int _iRow = 0; _iRow < dgSeletedWOList.Rows.Count; _iRow++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow Indata = IndataTable.NewRow();
                                Indata["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[_iRow].DataItem, "WO_DETL_ID"));
                                Indata["PROD_VER_CODE"] = Util.NVC(cboPlanVer.SelectedValue);
                                Indata["USERID"] = LoginInfo.USERID;
                                IndataTable.Rows.Add(Indata);
                            }
                        }
                        if(IndataTable.Rows.Count !=0 )
                        {
                            new ClientProxy().ExecuteService("BR_PRD_REG_TB_SFC_FP_DETL_PLAN_PROD_VER_CODE", "INDATA", null, IndataTable, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                    return;
                                }

                                Util.AlertInfo("SFU1270");  //저장되었습니다.
                                this.dgSeletedWOList.ItemsSource = null;
                                SearchData();
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
    }
}
