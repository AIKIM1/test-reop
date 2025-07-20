/*************************************************************************************
 Created Date : 2018.04.09
      Creator : 
   Decription : CANCEL DELETE_LOT
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_CANCEL_DELETE_LOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_CANCEL_DELETE_LOT : C1Window, IWorkArea
    {
        private string procID = string.Empty;
        private string eqsgID = string.Empty;
        private string eqptID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_CANCEL_DELETE_LOT()
        {
            InitializeComponent();
        }

        #region FormLoad Event
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            procID = tmps[0] as string;
            eqsgID = tmps[1] as string;
            eqptID = tmps[2] as string;
        }
        #endregion

        #region Button Event

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DataTable dtDeleteLot = DataTableConverter.Convert(dgLotList.ItemsSource);

            List<DataRow> drList = dtDeleteLot.Select("CHK = 1").ToList();

            if (drList.Count <= 0)
            {
                Util.MessageValidation("SFU1651");	//선택된 항목이 없습니다.
                return;
            }

            // 저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelDeleteLot();
                }
            });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31"); //기간은 {0}일 이내 입니다.
                return;
            }
            SearchDeleteLot();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region Method
        private void CancelDeleteLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = eqsgID;
                newRow["PROCID"] = procID;
                newRow["EQPTID"] = eqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["NOTE"] = txtNOTE.Text;

                inDataTable.Rows.Add(newRow);

                DataTable inLOT = indataSet.Tables.Add("INLOT");
                inLOT.Columns.Add("LOTID", typeof(string));
                inLOT.Columns.Add("PR_LOTID", typeof(string));
                inLOT.Columns.Add("CUT_ID", typeof(string));

                DataTable dtDeleteLot = DataTableConverter.Convert(dgLotList.ItemsSource);

                List<DataRow> drList = dtDeleteLot.Select("CHK = 1").ToList();

                foreach (DataRow row in drList)
                {
                    newRow = null;
                    newRow = inLOT.NewRow();
                    newRow["LOTID"] = Util.NVC(row["LOTID"]);
                    newRow["PR_LOTID"] = Util.NVC(row["PR_LOTID"]);
                    newRow["CUT_ID"] = Util.NVC(row["CUT_ID"]);

                    inLOT.Rows.Add(newRow);

                }
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_DELETE_LOT_ELTR", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        SearchDeleteLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SearchDeleteLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FDATE", typeof(string));
                inTable.Columns.Add("TDATE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = eqsgID;
                newRow["PROCID"] = procID;
                newRow["EQPTID"] = eqptID;
                newRow["FDATE"] = Util.GetCondition(dtpDateFrom);
                newRow["TDATE"] = Util.GetCondition(dtpDateTo);

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_DELETE_LOT", "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgLotList, dtResult, null);
                txtNOTE.Text = "";

                //if (dtResult == null || dtResult.Rows.Count == 0)
                //    Util.MessageInfo("SFU1498");    //데이터가 없습니다.
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
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
        #endregion

    }
}
