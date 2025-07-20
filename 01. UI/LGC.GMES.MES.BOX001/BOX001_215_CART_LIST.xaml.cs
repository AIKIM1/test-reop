/*************************************************************************************
 Created Date : 2017.12.25
      Creator : 
   Decription : CART 선택
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_215_CART_LIST : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        private string _procID = string.Empty;
        private string _eqsgID = string.Empty;
        private string _eqptID = string.Empty;
        private string _userID = string.Empty;
        private List<string> _cartList = null;

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

        public BOX001_215_CART_LIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

            InitializeUserControls();
            SetControl();
            SetControlVisibility();

        }
        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _procID = tmps[0] as string;
            _eqsgID = tmps[1] as string;
            _eqptID = tmps[2] as string;
            _userID = tmps[3] as string;
            _cartList = tmps[4] as List<string>;

            GetCartList();

        }

        private void SetControlVisibility()
        {
        }

        #endregion

        #region [출하처 그리드에서 선택]

        #endregion

        #region [선택]
        /// <summary>
        /// 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelect())
                return;

            try
            {
                List<int> chkList = _Util.GetDataGridCheckRowIndex(dgCartList, "CHK");
                string container = string.Empty;
                string bizRuleName = "BR_PRD_UPD_ASSIGN_CTNR_FOR_REWORK";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE");
                inDataTable.Columns.Add("IFMODE");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("USERID");
                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["EQPTID"] = _eqptID;
                dr["USERID"] = _userID;
                inDataTable.Rows.Add(dr);

                DataTable inInPutTable = ds.Tables.Add("IN_INPUT");
                inInPutTable.Columns.Add("CTNR_ID");
                foreach (int chk in chkList)
                {
                    container = dgCartList.GetCell(chk, dgCartList.Columns["CTNR_ID"].Index).Value.ToString();
                    dr = inInPutTable.NewRow();
                    dr["CTNR_ID"] = container;
                    inInPutTable.Rows.Add(dr);
                }
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (result, ex) =>
                {
                    if(ex!=null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    this.DialogResult = MessageBoxResult.OK;

                }, ds);



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        #region [닫기]
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        /// <summary>
        /// Cart List 조회
        /// </summary>
        private void GetCartList()
        {
            try
            {
                ShowLoadingIndicator();

                string bizName = "DA_PRD_SEL_CTNR_LIST_FOR_REWORK_NJ";

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _procID;
                newRow["EQSGID"] = _eqsgID;
                newRow["EQPTID"] = _eqptID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizName, "RQSTDT", "RSLTDT", inTable,(dtResult,ex)=>
                {
                    if(ex!=null )
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (!dtResult.Columns.Contains("CHK"))
                    {
                        dtResult.Columns.Add("CHK");
                    }

                    string[] sColumnName = new string[] { "CHK", "CTNR_ID2", "CTNR_ID", "ASSY_LOTID" };

                    Util.GridSetData(dgCartList, dtResult, FrameOperation);
                    _Util.SetDataGridMergeExtensionCol(dgCartList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                    foreach (string cart in _cartList)
                    {
                        //_Util.SetDataGridCheck(dgCartList, "CHK", "CTNR_ID", cart);
                        foreach (DataGridRow dgr in dgCartList.Rows)
                        {
                            if(DataTableConverter.GetValue(dgr.DataItem,"CTNR_ID").ToString().Equals(cart))
                                DataTableConverter.SetValue(dgr.DataItem, "CHK", true);
                        }
                    }

                });

                

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {

            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateSelect()
        {
            DataTable dt = DataTableConverter.Convert(dgCartList.ItemsSource);

            DataRow[] dr = dt.Select("CHK = 'True'");

            if (dr.Length == 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }
            if (dr.Length > 5)
            {
                // "대차 선택은 5개까지만 가능합니다."
                Util.MessageValidation("SFU4389");
                return false;
            }
            //AssyLot = dr[0]["LOTID"].ToString();
            return true;
        }
        #endregion

        #region [Func]
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

        #endregion

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            List<int> chkList = _Util.GetDataGridCheckRowIndex(dgCartList, "CHK");
            foreach (int chk in chkList)
            {
                _Util.SetDataGridUncheck(dgCartList, "CHK", chk);
            }
        }
    }
}
