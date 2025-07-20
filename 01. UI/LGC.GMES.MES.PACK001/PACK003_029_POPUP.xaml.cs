/*************************************************************************************
 Created Date : 2024.11.20
      Creator : 최평부
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.11.20  최평부           Initialize(ESST 포장 Batch 취소 기능 추가)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_029_POPUP : C1Window, IWorkArea
    {

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK003_029_POPUP()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        #endregion

        #region Event
        //최초 Load
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }
        #endregion

        #region Method
        public void SearchProcess()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //조회
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                
                DataRow dr = dtRQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                
                dtRQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_LOGIS_PACK_EQPT", dtRQSTDT.TableName, "RSLTDT", dtRQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(this.dgPackList, dtResult, FrameOperation);
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
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

        private void btnGroupCanCelExec_Click(object sender, RoutedEventArgs e)
        {
            var queryValidationCheck = DataTableConverter.Convert(dgPackList.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));

            if (queryValidationCheck.Count() <= 0)
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.                
                return;
            }

            //SFU10030 포장 그룹을 취소처리 하시겠습니까?
            Util.MessageConfirm("SFU10030", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    groupCancelExec((Button)sender);
                }
            });
        }

        private void groupCancelExec(Button button)
        {
            try
            {
                DataTable dtGridData = DataTableConverter.Convert(dgPackList.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).CopyToDataTable();
                var arr = dtGridData.AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => x.Field<string>("PACK_EQPTID")).ToList();

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PACK_EQPTID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                foreach(var item in arr) { 
                    DataRow inRow = inTable.NewRow();
                    inRow["PACK_EQPTID"] = item;
                    inRow["UPDUSER"] = LoginInfo.USERID;
                    inTable.Rows.Add(inRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_PACK_GROUP_CANCEL", "INDATA", null, inTable, (result, ex) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        // 취소되었습니다.
                        Util.MessageValidation("SFU1937");

                        DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex2)
                    {
                        Util.MessageException(ex2);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.DataContext == null)
            {
                return;
            }

            int selectedIndex = ((DataGridCellPresenter)checkBox.Parent).Row.Index;
            if (selectedIndex == -1)
            {
                return;
            }

            string selectedBatchID = Util.NVC(DataTableConverter.GetValue(this.dgPackList.Rows[selectedIndex].DataItem, "SMPL_GR_ID")).ToString();


            // 진행중이 batch 가 없으면 선택 불가하도록
            if (string.IsNullOrEmpty(selectedBatchID))
            {
                Util.Alert("SFU10031");
                DataTableConverter.SetValue(this.dgPackList.Rows[selectedIndex].DataItem, "CHK", false);
                return;
            }
        }
    }
}
