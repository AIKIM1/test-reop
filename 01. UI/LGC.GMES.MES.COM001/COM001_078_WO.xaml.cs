/*************************************************************************************
 Created Date : 2019.06.14
      Creator : 
   Decription : 계획버전 등록
--------------------------------------------------------------------------------------
 [Change History]
  2019.06.14  J.S HONG : Initial Created.
  2022.09.22  정재홍   : [C20220802-000526]- [생산PI팀] 재고 비교 및 재고 조정 화면 개선

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
    public partial class COM001_078_WO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private object[] tmps = null;

        public COM001_078_WO()
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

        private string _sWOID = string.Empty;
        private string _sISS_SLOC_ID = string.Empty;
        public string _pWOID
        {
            get { return _sWOID; }
        }

        public string _pISS_SLOC_ID
        {
            get { return _sISS_SLOC_ID; }
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

            SearchData();
        }
        #endregion

        #region Event

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
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        private bool isValid()
        {
            for (int i = 0; i < dgSeletedWOList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    return true;
                }
            }

            Util.Alert("SFU1443");  //작업지시를 선택하세요.
            return false;
        }

        private void selectCurrentWO()
        {
            for (int i = 0; i < dgSeletedWOList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "WOID")).Equals(Util.NVC(tmps[3])))
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
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));
                IndataTable.Columns.Add("ISS_SLOC_ID", typeof(string));
                IndataTable.Columns.Add("DFLT_FLAG", typeof(string));
                IndataTable.Columns.Add("CALDATE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = Util.NVC(tmps[0]);
                Indata["MTRLID"] = Util.NVC(tmps[1]);
                Indata["ISS_SLOC_ID"] = Util.NVC(tmps[2]);
                Indata["DFLT_FLAG"] = "N";
                //Indata["CALDATE"] = Util.NVC(tmps[4]);
                Indata["CALDATE"] = Util.NVC(Convert.ToDateTime(tmps[4]).ToString("yyyyMMdd"));
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WO_MTRL_INPUT", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        return;
                    }

                    // C20220802-000526 - [생산PI팀] 재고 비교 및 재고 조정 화면 개선
                    if (result?.Select("WOID <> 'PPLO'")?.Length > 0)
                    {
                        DataTable dtPPLO = result.Select("WOID <> 'PPLO' ").CopyToDataTable();

                        Util.GridSetData(dgSeletedWOList, dtPPLO, FrameOperation, true);
                    }
                    else
                    {
                        Util.GridSetData(dgSeletedWOList, result, FrameOperation, true);
                    }
                    //Util.GridSetData(dgSeletedWOList, result, FrameOperation, true);
                    if (!String.IsNullOrEmpty(Util.NVC(tmps[3])))
                    {
                        selectCurrentWO();
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
                this.dgSeletedWOList.EndEdit();
                this.dgSeletedWOList.EndEditRow(true);

                string sWorkOrderId = string.Empty;
                string sIssueSaveLocationId = string.Empty;

                for (int i = 0; i < dgSeletedWOList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        sWorkOrderId = Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "WOID"));
                        sIssueSaveLocationId = Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[i].DataItem, "ISS_SLOC_ID"));
                        break;
                    }
                }

                if (sWorkOrderId.Length > 1)
                {
                    _sWOID = sWorkOrderId;
                    _sISS_SLOC_ID = sIssueSaveLocationId;
                }

                DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        private void dgSeletedWOList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            try
            {
                for (int i = 0; i < dgSeletedWOList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSeletedWOList.Rows[i].DataItem, "CHK", false);
                }

                DataTableConverter.SetValue(e.Row.DataItem, "CHK", true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }
        #endregion
    }
}
