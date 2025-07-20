/*************************************************************************************
 Created Date : 2019.04.19
      Creator : 신광희
   Decription : 창고 설정 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.19  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Windows;
using System;
using System.Data;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_018_WAREHOUSE_SETUP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_018_WAREHOUSE_SETUP : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        public bool IsUpdated;
        #endregion


        public MCS001_018_WAREHOUSE_SETUP()
        {
            InitializeComponent();
        }

        #region Initialize

        private void InitializeCombo()
        {
            SetSetUpCombo(cboSetUp);
            SetDataGridUseFlagCombo(dgList.Columns["CMCDIUSE"]);
        }
        
        #endregion

        public IFrameOperation FrameOperation { get; set; }

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_SETUP";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["CMCDTYPE"] = cboSetUp.SelectedValue;
                dr["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboSetUp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboSetUp?.SelectedValue != null)
            {

                switch (cboSetUp.SelectedValue.GetString())
                {
                    case "APCW_PROPER_COUNT_A5":
                    case "APCW_PROPER_COUNT_A6":
                    {
                        dgList.Columns["ATTRIBUTE1"].Header = ObjectDic.Instance.GetObjectName("최대 이송가능 수량(PANCAKE)");
                        break;
                    }
                    case "PCW_E_SKID_TARGET":
                    {
                        dgList.Columns["ATTRIBUTE1"].Header = ObjectDic.Instance.GetObjectName("운영 최소요구 공스키드 수량");
                        break;
                    }
                    case "PCW_CWA_SKID_LSL":
                    {
                        dgList.Columns["ATTRIBUTE1"].Header = ObjectDic.Instance.GetObjectName("운영 스키드MAX");
                        break;
                    }
                    default:
                        dgList.Columns["ATTRIBUTE1"].Header = ObjectDic.Instance.GetObjectName("ATTRIBUTE1");
                        break;
                }

                btnSearch_Click(btnSearch, null);
            }
        }

        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            string[] exceptionColumn = { "CMCODE", "CMCDNAME"};
            Util.DataGridRowEditByCheckBoxColumn(e, exceptionColumn);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSetUp()) return;

            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_REG_PCW_SETTING_VAL";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("TYPECODE", typeof(string));
                inDataTable.Columns.Add("CMCODE", typeof(string));
                inDataTable.Columns.Add("CMCDNAME", typeof(string));
                inDataTable.Columns.Add("CMCDIUSE", typeof(string));
                inDataTable.Columns.Add("ATTRIBUTE1", typeof(string));
                inDataTable.Columns.Add("ATTRIBUTE2", typeof(string));
                inDataTable.Columns.Add("ATTRIBUTE3", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));


                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True")
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["TYPECODE"] = DataTableConverter.GetValue(row.DataItem, "CMCDTYPE");
                        dr["CMCODE"] = DataTableConverter.GetValue(row.DataItem, "CMCODE");
                        dr["CMCDNAME"] = DataTableConverter.GetValue(row.DataItem, "CMCDNAME");
                        dr["CMCDIUSE"] = DataTableConverter.GetValue(row.DataItem, "CMCDIUSE");
                        dr["ATTRIBUTE1"] = DataTableConverter.GetValue(row.DataItem, "ATTRIBUTE1");
                        dr["ATTRIBUTE2"] = DataTableConverter.GetValue(row.DataItem, "ATTRIBUTE2");
                        dr["ATTRIBUTE3"] = DataTableConverter.GetValue(row.DataItem, "ATTRIBUTE3");
                        dr["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        //Util.gridClear(dgList);
                        IsUpdated = true;
                        btnSearch_Click(btnSearch, null);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        #endregion


        #region User Method

        private bool ValidationSearch()
        {
            if (cboSetUp.SelectedValue == null)
            {
                Util.MessageValidation("SFU1678");
                return false;
            }
            return true;
        }


        private bool ValidationSetUp()
        {
            dgList.EndEdit();
            dgList.EndEditRow(true);

            if (!CommonVerify.HasDataGridRow(dgList))
            {
                Util.MessageValidation("SFU5069");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgList, "CHK", true) < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private static void SetSetUpCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "CWA_PCW_SET_UNIT_LIST" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetDataGridUseFlagCombo(C1.WPF.DataGrid.DataGridColumn dgcol)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "CMCDTYPE", "LANGID" };
            string[] arrCondition = { "IUSE", LoginInfo.LANGID };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, dgcol, selectedValueText, displayMemberText);
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
