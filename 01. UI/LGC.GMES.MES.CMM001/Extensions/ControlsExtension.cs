/*************************************************************************************
 Created Date : 2021.01.26 
      Creator : 조영대
   Decription : Controls Extension : ControlsExtension.cs
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.28  조영대 : Initial Created
  2021.02.26  조영대 : Method 추가 및 수정
  2021.03.16  조영대 : 콤보 코드 추가 Method
  2021.03.25  조영대 : Filter 인수 추가
  2022.05.27  조영대 : Color 보색 추가
  2022.07.07  조영대 : ComboBoxExtension 관련 수정.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Class
{
    public static class UserControlExtension
    {
        public static void ClearValidation(this C1Window c1Window)
        {
            ClearValidationChild(c1Window);
        }

        public static void ClearValidation(this UserControl userControl)
        {
            ClearValidationChild(userControl);
        }
         
        private static void ClearValidationChild(DependencyObject parent)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Controls.IControlValidation)
                {
                    Controls.IControlValidation ctl = child as Controls.IControlValidation;
                    ctl.ClearValidation();
                }
                else
                {
                    ClearValidationChild(child);
                }
            }
        }
    }

    public static class ComboBoxExtension
    {
        public enum InCodeType
        {
            Bracket,
            Parenthesis,
            Colon
        }

        /// <summary>
        /// ItemsSource Clear
        /// </summary>
        public static void Clear(this System.Windows.Controls.ItemsControl cbo)
        {
            if (cbo == null || cbo.ItemsSource == null) return;

            cbo.ItemsSource = null;
        }

        /// <summary>
        /// Item Clear
        /// </summary>
        public static void ClearItems(this System.Windows.Controls.ItemsControl cbo)
        {
            if (cbo == null || cbo.ItemsSource == null) return;

            DataTable dtClear = DataTableConverter.Convert(cbo.ItemsSource);
            if (dtClear != null && dtClear.Rows.Count > 0)
            {
                dtClear.Rows.Clear();
                cbo.ItemsSource = DataTableConverter.Convert(dtClear);
            }
        }

        /// <summary>
        /// Data Bind 시 사용. null or string.Empty 일때 null 반환        
        /// </summary>
        /// <returns></returns>
        public static object GetBindValue(this C1ComboBox cbo)
        {
            // ex) inData["FIELD"] = Util.NVC(cboCombo.SelectedValue).Equeal(string.Empty) ? null : cboCombo.SelectedValue;
            // ==> inData["FIELD"] = cboCombo.GetBindValue();

            if (cbo == null || cbo.ItemsSource == null) return null;

            if (cbo.SelectedValue == null) return null;
            if (cbo.SelectedValue == DBNull.Value) return null;
            if (cbo.SelectedValue.Equals(string.Empty)) return null;
            if (cbo.SelectedIndex == 0)
            {
                if (cbo.SelectedValue.Equals("SELECT") || cbo.SelectedValue.Equals("-SELECT-") ||
                    cbo.SelectedValue.Equals("ALL") || cbo.SelectedValue.Equals("-ALL-") ||
                    cbo.SelectedValue.Equals("N/A") || cbo.SelectedValue.Equals("-N/A-"))
                {
                    return null;
                }
            }
            return cbo.SelectedValue;
        }

        /// <summary>
        ///  Data Bind 시 사용. ColumnName 에 해당하는 값을 가져온다.
        ///  null or string.Empty 일때 null 반환
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="columnName">값을 가져올 컬럼명</param>
        /// <returns></returns>
        public static object GetBindValue(this C1ComboBox cbo, string columnName)
        {
            if (cbo == null || cbo.ItemsSource == null) return null;

            if (cbo.SelectedValue == null) return null;
            if (cbo.SelectedValue == DBNull.Value) return null;
            if (cbo.SelectedValue.Equals(string.Empty)) return null;

            if (cbo.SelectedItem is DataRowView)
            {
                DataRowView cboView = cbo.SelectedItem as DataRowView;
                if (cboView.Row.Table.Columns.Contains(columnName))
                {
                    if (cboView[columnName] == null) return null;
                    if (cboView[columnName] == System.DBNull.Value) return null;
                    if (cboView[columnName].Equals(string.Empty)) return null;

                    return cboView[columnName];
                }
            }
            return null;
        }

        /// <summary>
        /// Data Bind 시 사용. string 반환   
        /// </summary>
        /// <param name="cbo"></param>
        /// <returns></returns>
        public static string GetStringValue(this C1ComboBox cbo)
        {
            if (cbo == null || cbo.ItemsSource == null) return string.Empty;

            if (cbo.SelectedValue == null) return string.Empty;
            if (cbo.SelectedValue == DBNull.Value) return string.Empty;
            if (cbo.SelectedValue.Equals(string.Empty)) return string.Empty;
            if (cbo.SelectedIndex == 0)
            {
                if (cbo.SelectedValue.Equals("SELECT") || cbo.SelectedValue.Equals("-SELECT-") ||
                    cbo.SelectedValue.Equals("ALL") || cbo.SelectedValue.Equals("-ALL-") ||
                    cbo.SelectedValue.Equals("N/A") || cbo.SelectedValue.Equals("-N/A-"))
                {
                    return string.Empty;
                }
            }
            return cbo.SelectedValue.ToString();
        }

        /// <summary>
        ///  ColumnName 에 해당하는 값을 가져온다.
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="columnName">값을 가져올 컬럼명</param>
        /// <returns></returns>
        public static string GetStringValue(this C1ComboBox cbo, string columnName)
        {
            if (cbo == null || cbo.ItemsSource == null) return string.Empty;

            if (cbo.SelectedValue == null) return string.Empty;
            if (cbo.SelectedValue == DBNull.Value) return string.Empty;
            if (cbo.SelectedValue.Equals(string.Empty)) return string.Empty;
            if (cbo.SelectedIndex == 0)
            {
                if (cbo.SelectedValue.Equals("SELECT") || cbo.SelectedValue.Equals("-SELECT-") ||
                    cbo.SelectedValue.Equals("ALL") || cbo.SelectedValue.Equals("-ALL-") ||
                    cbo.SelectedValue.Equals("N/A") || cbo.SelectedValue.Equals("-N/A-"))
                {
                    return string.Empty;
                }
            }

            if (cbo.SelectedItem is DataRowView)
            {
                DataRowView cboView = cbo.SelectedItem as DataRowView;
                if (cboView.Row.Table.Columns.Contains(columnName))
                {
                    if (cboView[columnName] == null) return null;
                    if (cboView[columnName] == System.DBNull.Value) return string.Empty;
                    if (cboView[columnName].Equals(string.Empty)) return string.Empty;

                    return cboView[columnName].ToString();
                }
            }
            return string.Empty;
        }


        #region 공통코드
        /// <summary>
        /// Common Code 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// < param name="groupCode">공통 코드</param>
        public static void SetCommonCode(this C1ComboBox cbo, string groupCode)
        {
            SetCommonCode(cbo, groupCode, Class.CommonCombo.ComboStatus.NONE, false);
        }

        /// <summary>
        /// Common Code 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="groupCode">공통 코드</param>
        /// <param name="isInCode">코드 보임 유무</param>
        public static void SetCommonCode(this C1ComboBox cbo, string groupCode, bool isInCode)
        {
            SetCommonCode(cbo, groupCode, Class.CommonCombo.ComboStatus.NONE, isInCode);
        }

        /// <summary>
        /// Common Code 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="groupCode">공통 코드</param>
        /// <param name="status">최상단 추가 아이템</param>
        public static void SetCommonCode(this C1ComboBox cbo, string groupCode, Class.CommonCombo.ComboStatus status)
        {
            SetCommonCode(cbo, groupCode, status, true);
        }

        /// <summary>
        /// Common Code 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// < param name="groupCode">공통 코드</param>
        /// <param name = "status" > 최상단 추가 아이템</param>
        /// <param name = "isInCode" > 코드 보임 유무</param>
        public static void SetCommonCode(this C1ComboBox cbo, string groupCode, Class.CommonCombo.ComboStatus status, bool isInCode)
        {
            SetCommonCode(cbo, groupCode, string.Empty, status, isInCode);
        }

        public static void SetCommonCode(this C1ComboBox cbo, string groupCode, string filter, Class.CommonCombo.ComboStatus status, bool isInCode)
        {
            SetCommonCode(cbo, groupCode, filter, status, isInCode, InCodeType.Bracket);
        }

        /// <summary>
        /// Common Code 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// <param name="groupCode">공통 코드</param>
        /// <param name="filter">필터</param>
        /// <param name = "status" > 최상단 추가 아이템</param>
        /// <param name = "isInCode" > 코드 보임 유무</param>
        public static void SetCommonCode(this C1ComboBox cbo, string groupCode, string filter, Class.CommonCombo.ComboStatus status, bool isInCode, InCodeType inCodeType)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow drNew = RQSTDT.NewRow();
                drNew["LANGID"] = LoginInfo.LANGID;
                drNew["CMCDTYPE"] = groupCode;
                RQSTDT.Rows.Add(drNew);

                string bizRuleName = "DA_BAS_SEL_COMMCODE_ALL_CBO";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                if (!filter.Equals(string.Empty))
                {
                    DataView dvResult = dtResult.DefaultView;
                    dvResult.RowFilter = filter;
                    dtResult = dvResult.ToTable();
                }

                if (cbo.DisplayMemberPath == null || Util.NVC(cbo.DisplayMemberPath) == string.Empty) cbo.DisplayMemberPath = "CBO_NAME";
                if (cbo.SelectedValuePath == null || Util.NVC(cbo.SelectedValuePath) == string.Empty) cbo.SelectedValuePath = "CBO_CODE";

                if (isInCode)
                {
                    foreach (DataRow dr in dtResult.Rows)
                    {
                        switch (inCodeType)
                        {
                            case InCodeType.Parenthesis:
                                dr[cbo.DisplayMemberPath] = "(" + dr[cbo.SelectedValuePath].ToString() + ") " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                            case InCodeType.Colon:
                                dr[cbo.DisplayMemberPath] = dr[cbo.SelectedValuePath].ToString() + " : " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                            default:
                                dr[cbo.DisplayMemberPath] = "[" + dr[cbo.SelectedValuePath].ToString() + "] " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                        }
                    }
                }
                
                DataRow newRow = dtResult.NewRow();

                switch (status)
                {
                    case CommonCombo.ComboStatus.ALL:
                        newRow[cbo.SelectedValuePath] = null;
                        newRow[cbo.DisplayMemberPath] = "ALL";
                        dtResult.Rows.InsertAt(newRow, 0);
                        break;

                    case CommonCombo.ComboStatus.SELECT:
                        newRow[cbo.SelectedValuePath] = "SELECT";
                        newRow[cbo.DisplayMemberPath] = "SELECT";
                        dtResult.Rows.InsertAt(newRow, 0);
                        break;

                    case CommonCombo.ComboStatus.NA:
                        newRow[cbo.SelectedValuePath] = null;
                        newRow[cbo.DisplayMemberPath] = "N/A";
                        dtResult.Rows.InsertAt(newRow, 0);
                        break;

                    case CommonCombo.ComboStatus.EMPTY:
                        newRow[cbo.SelectedValuePath] = null;
                        newRow[cbo.DisplayMemberPath] = string.Empty;
                        dtResult.Rows.InsertAt(newRow, 0);
                        break;

                    case CommonCombo.ComboStatus.NONE:
                        break;
                }

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        #endregion



        #region 동별 공통코드
        /// <summary>
        /// 동별 공통코드 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// < param name="groupCode">공통 코드</param>
        public static void SetAreaCommonCode(this C1ComboBox cbo, string groupCode)
        {
            SetAreaCommonCode(cbo, groupCode, Class.CommonCombo.ComboStatus.NONE, false);
        }

        /// <summary>
        /// 동별 공통코드 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="groupCode">공통 코드</param>
        /// <param name="isInCode">코드 보임 유무</param>
        public static void SetAreaCommonCode(this C1ComboBox cbo, string groupCode, bool isInCode)
        {
            SetAreaCommonCode(cbo, groupCode, Class.CommonCombo.ComboStatus.NONE, isInCode);
        }

        /// <summary>
        /// 동별 공통코드 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="groupCode">공통 코드</param>
        /// <param name="status">최상단 추가 아이템</param>
        public static void SetAreaCommonCode(this C1ComboBox cbo, string groupCode, Class.CommonCombo.ComboStatus status)
        {
            SetAreaCommonCode(cbo, groupCode, status, true);
        }

        /// <summary>
        /// 동별 공통코드 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// < param name="groupCode">공통 코드</param>
        /// <param name = "status" > 최상단 추가 아이템</param>
        /// <param name = "isInCode" > 코드 보임 유무</param>
        public static void SetAreaCommonCode(this C1ComboBox cbo, string groupCode, Class.CommonCombo.ComboStatus status, bool isInCode)
        {
            SetAreaCommonCode(cbo, groupCode, string.Empty, status, isInCode);
        }

        public static void SetAreaCommonCode(this C1ComboBox cbo, string groupCode, string filter, Class.CommonCombo.ComboStatus status, bool isInCode)
        {
            SetAreaCommonCode(cbo, groupCode, filter, status, isInCode, InCodeType.Bracket);
        }

        /// <summary>
        /// 동별 공통코드 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// <param name="groupCode">공통 코드</param>
        /// <param name="filter">필터</param>
        /// <param name = "status" > 최상단 추가 아이템</param>
        /// <param name = "isInCode" > 코드 보임 유무</param>
        public static void SetAreaCommonCode(this C1ComboBox cbo, string groupCode, string filter, Class.CommonCombo.ComboStatus status, bool isInCode, InCodeType inCodeType)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow drNew = RQSTDT.NewRow();
                drNew["LANGID"] = LoginInfo.LANGID;
                drNew["AREAID"] = LoginInfo.CFG_AREA_ID;
                drNew["COM_TYPE_CODE"] = groupCode;
                drNew["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(drNew);

                string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                if (!filter.Equals(string.Empty))
                {
                    DataView dvResult = dtResult.DefaultView;
                    dvResult.RowFilter = filter;
                    dtResult = dvResult.ToTable();
                }

                if (cbo.DisplayMemberPath == null || Util.NVC(cbo.DisplayMemberPath) == string.Empty) cbo.DisplayMemberPath = "COM_CODE_NAME";
                if (cbo.SelectedValuePath == null || Util.NVC(cbo.SelectedValuePath) == string.Empty) cbo.SelectedValuePath = "COM_CODE";

                if (isInCode)
                {
                    foreach (DataRow dr in dtResult.Rows)
                    {
                        switch (inCodeType)
                        {
                            case InCodeType.Parenthesis:
                                dr[cbo.DisplayMemberPath] = "(" + dr[cbo.SelectedValuePath].ToString() + ") " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                            case InCodeType.Colon:
                                dr[cbo.DisplayMemberPath] = dr[cbo.SelectedValuePath].ToString() + " : " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                            default:
                                dr[cbo.DisplayMemberPath] = "[" + dr[cbo.SelectedValuePath].ToString() + "] " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                        }
                    }
                }

                DataRow newRow = dtResult.NewRow();

                switch (status)
                {
                    case CommonCombo.ComboStatus.ALL:
                        newRow[cbo.SelectedValuePath] = null;
                        newRow[cbo.DisplayMemberPath] = "ALL";
                        dtResult.Rows.InsertAt(newRow, 0);
                        break;

                    case CommonCombo.ComboStatus.SELECT:
                        newRow[cbo.SelectedValuePath] = "SELECT";
                        newRow[cbo.DisplayMemberPath] = "SELECT";
                        dtResult.Rows.InsertAt(newRow, 0);
                        break;

                    case CommonCombo.ComboStatus.NA:
                        newRow[cbo.SelectedValuePath] = null;
                        newRow[cbo.DisplayMemberPath] = "N/A";
                        dtResult.Rows.InsertAt(newRow, 0);
                        break;

                    case CommonCombo.ComboStatus.EMPTY:
                        newRow[cbo.SelectedValuePath] = null;
                        newRow[cbo.DisplayMemberPath] = string.Empty;
                        dtResult.Rows.InsertAt(newRow, 0);
                        break;

                    case CommonCombo.ComboStatus.NONE:
                        break;
                }

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        #endregion


        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, false, null);
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="selectedValue"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, string selectedValue)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, false, selectedValue);
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="isInCode"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, bool isInCode)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, isInCode, null);
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="selectedValue"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, bool isInCode, string selectedValue)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, "", CommonCombo.ComboStatus.NONE, isInCode, InCodeType.Bracket, selectedValue);            
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="status"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, CommonCombo.ComboStatus status)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, string.Empty, status, false, null);
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="status"></param>
        /// <param name="selectedValue"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, CommonCombo.ComboStatus status, string selectedValue)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, string.Empty, status, false, selectedValue);
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="status"></param>
        /// <param name="isInCode"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, CommonCombo.ComboStatus status, bool isInCode)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, string.Empty, status, isInCode, null);
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="status"></param>
        /// <param name="isInCode"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, string filter, CommonCombo.ComboStatus status, bool isInCode)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, filter, status, isInCode, null);
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="filter"></param>
        /// <param name="status"></param>
        /// <param name="isInCode"></param>
        /// <param name="selectedValue"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, string filter, CommonCombo.ComboStatus status, bool isInCode, string selectedValue)
        {
            SetDataComboItem(cbo, bizRuleName, arrColumn, arrCondition, filter, status, isInCode, InCodeType.Bracket, selectedValue);
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="filter"></param>
        /// <param name="status"></param>
        /// <param name="isInCode"></param>
        /// <param name="inCodeType"></param>
        /// <param name="selectedValue"></param>
        public static void SetDataComboItem(this C1ComboBox cbo, string bizRuleName, string[] arrColumn, string[] arrCondition, string filter, CommonCombo.ComboStatus status, bool isInCode, InCodeType inCodeType, string selectedValue = null)
        {
            // CommonCombo.CommonBaseCombo 에서 복사
            try
            {
                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                        inDataTable.Columns.Add(col, typeof(string));

                    DataRow dr = inDataTable.NewRow();

                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];

                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (dtResult != null)
                {
                    if (!filter.Equals(string.Empty))
                    {
                        DataView dvResult = dtResult.DefaultView;
                        dvResult.RowFilter = filter;
                        dtResult = dvResult.ToTable();
                    }

                    SetDataComboItem(cbo, dtResult, status, isInCode, inCodeType, selectedValue);
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", 
                    System.Windows.MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        public static void SetDataComboItem(this C1ComboBox cbo, string[] strComboData, CommonCombo.ComboStatus status, bool isInCode = false, InCodeType inCodeType = InCodeType.Bracket, string selectedValue = null)
        {
            try
            {
                DataTable dtComboData = new DataTable();
                dtComboData.Columns.Add("CBO_NAME");
                dtComboData.Columns.Add("CBO_CODE");

                for (int index = 0; index < strComboData.Length; index += 2)
                {
                    if (index + 1 >= strComboData.Length) break;

                    DataRow dataRow = dtComboData.NewRow();
                    dataRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName(strComboData[index]);
                    dataRow["CBO_CODE"] = strComboData[index + 1];
                    dtComboData.Rows.Add(dataRow);
                }
                SetDataComboItem(cbo, dtComboData, status, isInCode, inCodeType, selectedValue);
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        public static void SetDataComboItem(this C1ComboBox cbo, DataTable dtComboData, CommonCombo.ComboStatus status, bool isInCode = false, InCodeType inCodeType = InCodeType.Bracket, string selectedValue = null)
        {
            try
            {
                if (cbo == null) return;

                if (cbo.SelectedValuePath == null || cbo.SelectedValuePath.Equals(string.Empty))
                {
                    if (dtComboData.Columns.Contains("CBO_CODE"))
                    {
                        cbo.SelectedValuePath = "CBO_CODE";
                    }
                    else
                    {
                        if (dtComboData.Columns.Count > 0)
                        {
                            cbo.SelectedValuePath = dtComboData.Columns[0].ColumnName;
                        }
                    }
                }

                if (cbo.DisplayMemberPath == null || cbo.DisplayMemberPath.Equals(string.Empty))
                {
                    if (dtComboData.Columns.Contains("CBO_NAME"))
                    {
                        cbo.DisplayMemberPath = "CBO_NAME";
                    }
                    else
                    {
                        if (dtComboData.Columns.Count > 1)
                        {
                            cbo.DisplayMemberPath = dtComboData.Columns[1].ColumnName;
                        }
                    }
                }

                if (isInCode)
                {
                    foreach (DataRow dr in dtComboData.Rows)
                    {
                        switch (inCodeType)
                        {
                            case InCodeType.Parenthesis:
                                dr[cbo.DisplayMemberPath] = "(" + dr[cbo.SelectedValuePath].ToString() + ") " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                            case InCodeType.Colon:
                                dr[cbo.DisplayMemberPath] = dr[cbo.SelectedValuePath].ToString() + " : " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                            default:
                                dr[cbo.DisplayMemberPath] = "[" + dr[cbo.SelectedValuePath].ToString() + "] " + dr[cbo.DisplayMemberPath].ToString();
                                break;
                        }
                    }
                }

                DataRow newRow = dtComboData.NewRow();
                switch (status)
                {
                    case CommonCombo.ComboStatus.ALL:
                        if (dtComboData.Rows.Count == 0 || !dtComboData.Rows[0][cbo.DisplayMemberPath].Equals("ALL"))
                        {
                            newRow[cbo.SelectedValuePath] = null;
                            newRow[cbo.DisplayMemberPath] = "ALL";
                            dtComboData.Rows.InsertAt(newRow, 0);
                        }
                        break;

                    case CommonCombo.ComboStatus.SELECT:
                        if (dtComboData.Rows.Count == 0 || !dtComboData.Rows[0][cbo.DisplayMemberPath].Equals("SELECT"))
                        {
                            newRow[cbo.SelectedValuePath] = "SELECT";
                            newRow[cbo.DisplayMemberPath] = "SELECT";
                            dtComboData.Rows.InsertAt(newRow, 0);
                        }
                        break;

                    case CommonCombo.ComboStatus.NA:
                        if (dtComboData.Rows.Count == 0 || !dtComboData.Rows[0][cbo.DisplayMemberPath].Equals("N/A"))
                        {
                            newRow[cbo.SelectedValuePath] = null;
                            newRow[cbo.DisplayMemberPath] = "N/A";
                            dtComboData.Rows.InsertAt(newRow, 0);
                        }
                        break;

                    case CommonCombo.ComboStatus.EMPTY:
                        if (dtComboData.Rows.Count == 0 || !dtComboData.Rows[0][cbo.DisplayMemberPath].Equals(string.Empty))
                        {
                            newRow[cbo.SelectedValuePath] = null;
                            newRow[cbo.DisplayMemberPath] = string.Empty;
                            dtComboData.Rows.InsertAt(newRow, 0);
                        }                        
                        break;
                }

                cbo.ItemsSource = dtComboData.AsDataView();

                if (!string.IsNullOrEmpty(selectedValue))
                {
                    cbo.SelectedValue = selectedValue;

                    if (cbo.SelectedIndex < 0)
                        cbo.SelectedIndex = 0;
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

    }

    public static class MultiSelectionBoxExtension
    {
        /// <summary>
        /// Data Bind 시 사용. null or string.Empty 일때 null 반환     
        /// </summary>
        /// <returns></returns>
        public static object GetBindValue(this ControlsLibrary.MultiSelectionBox cbo)
        {
            if (cbo == null || cbo.ItemsSource == null) return null;

            if (cbo.SelectedItemsToString == null) return null;
            if (cbo.SelectedItemsToString.Equals(string.Empty)) return null;
            if (cbo.SelectedItemsToString.Equals("ALL") || cbo.SelectedItemsToString.Equals("-ALL-"))
            {
                return null;
            }

            DataTable dtItem = DataTableConverter.Convert(cbo.ItemsSource);
            if (cbo.SelectedItems.Count == dtItem.Rows.Count) return null;

            return cbo.SelectedItemsToString;
        }

        /// <summary>
        /// Data Bind 시 사용. String 반환, 전체 선택일때 String.Empty
        /// </summary>
        /// <returns></returns>
        public static string GetStringValue(this ControlsLibrary.MultiSelectionBox cbo)
        {
            if (cbo == null || cbo.ItemsSource == null) return string.Empty;

            if (cbo.SelectedItemsToString == null) return null;
            if (cbo.SelectedItemsToString.Equals(string.Empty)) return null;
            if (cbo.SelectedItemsToString.Equals("ALL") || cbo.SelectedItemsToString.Equals("-ALL-"))
            {
                return string.Empty;
            }

            DataTable dtItem = DataTableConverter.Convert(cbo.ItemsSource);
            if (cbo.SelectedItems.Count == dtItem.Rows.Count) return string.Empty;

            return cbo.SelectedItemsToString;
        }

        /// <summary>
        ///  ALL 사용시 Null이나 Empty가 아닌 선택된 전체 데이터가져오기     
        /// </summary>
        /// <returns></returns>
        public static object GetAllValue(this ControlsLibrary.MultiSelectionBox cbo)
        {
            if (cbo == null || cbo.ItemsSource == null) return null;

            if (cbo.SelectedItemsToString == null) return null;
            if (cbo.SelectedItemsToString.Equals(string.Empty)) return null;
            DataTable dtItem = DataTableConverter.Convert(cbo.ItemsSource);
             return cbo.SelectedItemsToString;
        }

        /// <summary>
        /// Common Code 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// < param name="groupCode">공통 코드</param>
        public static void SetCommonCode(this ControlsLibrary.MultiSelectionBox cbo, string groupCode)
        {
            SetCommonCode(cbo, groupCode, false);
        }
                
        /// <summary>
        /// Common Code 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// < param name="groupCode">공통 코드</param>
        /// <param name = "isInCode" > 코드 보임 유무</param>
        public static void SetCommonCode(this ControlsLibrary.MultiSelectionBox cbo, string groupCode, bool isInCode)
        {
            SetCommonCode(cbo, groupCode, string.Empty, isInCode);
        }

        /// <summary>
        /// Common Code 설정
        /// </summary>
        /// <param name = "cbo" ></ param >
        /// <param name="groupCode">공통 코드</param>
        /// <param name="filter">필터</param>
        /// <param name = "isInCode" > 코드 보임 유무</param>
        public static void SetCommonCode(this ControlsLibrary.MultiSelectionBox cbo, string groupCode, string filter, bool isInCode)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = groupCode;
                RQSTDT.Rows.Add(dr);

                string bizRuleName = "DA_BAS_SEL_COMMCODE_ALL_CBO";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                if (!filter.Equals(string.Empty))
                {
                    DataView dvResult = dtResult.DefaultView;
                    dvResult.RowFilter = filter;
                    dtResult = dvResult.ToTable();
                }

                if (isInCode && dtResult != null)
                {
                    dtResult.Columns.Add("CBO_NAME2", typeof(string));
                    dtResult.AsEnumerable().ToList<DataRow>()
                        .ForEach(x => x["CBO_NAME2"] = "[" + Util.NVC(x["CBO_CODE"]) + "] " + Util.NVC(x["CBO_NAME"]));
                }

                if (isInCode)
                {
                    cbo.DisplayMemberPath = "CBO_NAME2";
                }
                else
                {
                    cbo.DisplayMemberPath = "CBO_NAME";
                }
                cbo.SelectedValuePath = "CBO_CODE";
                
                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.CheckAll();
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

    }

    public static class TextBoxExtension
    {
        /// <summary>
        /// Data Bind 시 사용. null or string.Empty 일때 null 반환        
        /// </summary>
        /// <returns></returns>
        public static object GetBindValue(this TextBox txt)
        {
            // ex) inData["FIELD"] = Util.NVC(txtTextBox.Text).Equeal(string.Empty) ? null : txtTextBox.Text;
            // ==> inData["FIELD"] = txtTextBox.GetBindValue();
            
            if (txt == null) return null;

            if (txt is UcBaseTextBox)
            {
                UcBaseTextBox baseTxt = txt as UcBaseTextBox;
                if (baseTxt.Text.Trim().Equals(string.Empty)) return null;
                return baseTxt.Text.Trim();
            }
            else
            {
                if (txt.Text.Trim().Equals(string.Empty)) return null;
                return txt.Text.Trim();
            }           
        }
    }

    public static class ButtonExtension
    {
        public static void PerformClick(this Button button)
        {
            button.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent, button));
        }
    }

    public static class ColorExtension
    {
        /// <summary>
        /// 보색
        /// </summary>
        /// <param name="color"></param>
        public static System.Drawing.Color GetNegativeColor(this System.Drawing.Color color)
        {
            byte colorR = Convert.ToByte(255 - color.R);
            byte colorG = Convert.ToByte(255 - color.G);
            byte colorB = Convert.ToByte(255 - color.B);
            return System.Drawing.Color.FromArgb(color.A, colorR, colorG, colorB);
        }

        /// <summary>
        /// 배경색에 따른 전경색(흰색/검은색) 찾기
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static System.Drawing.Color GetBlackAndWhiteTextColor(this System.Drawing.Color color)
        {
            bool isBlack = .222 * color.R + .707 * color.G + .071 * color.B > 128;
            System.Drawing.Color forecolor = isBlack ? System.Drawing.Color.Black : System.Drawing.Color.White;
            return forecolor;
        }
    }

    public static class DependencyObjectExtension
    {
        public static T FindTopParent<T>(this DependencyObject control) where T : DependencyObject
        {
            if (control == null) return null;
            DependencyObject tempControl = control;
            T foundParent = null;

            while ((tempControl = VisualTreeHelper.GetParent(tempControl)) != null)
            {
                if (tempControl is T) foundParent = (T)tempControl;
            }

            return foundParent;
        }

        public static T FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            try
            {
                if (parent == null) return null;

                T foundChild = null;

                int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    T childType = child as T;
                    if (childType == null)
                    {
                        foundChild = FindChild<T>(child, childName);

                        if (foundChild != null) break;
                    }
                    else if (!string.IsNullOrEmpty(childName))
                    {
                        var frameworkElement = child as FrameworkElement;
                        if (frameworkElement != null && frameworkElement.Name == childName)
                        {
                            foundChild = (T)child;
                            break;
                        }
                    }
                    else
                    {
                        foundChild = (T)child;
                        break;
                    }
                }

                return foundChild;
            }
            catch
            {
                return null;
            }
        }

        public static T FindItem<T>(this DependencyObject parent, string itemName) where T : DependencyObject
        {
            if (parent == null) return null;

            if (!(parent is ContextMenu || parent is MenuItem)) return null;

            T foundItem = null;

            int itemCount = 0;
            if (parent is ContextMenu)
            {
                itemCount = (parent as ContextMenu).Items.Count;
            }
            else if (parent is MenuItem)
            {
                itemCount = (parent as MenuItem).Items.Count;
            }

            for (int i = 0; i < itemCount; i++)
            {
                DependencyObject item = null;
                if (parent is ContextMenu)
                {
                    item = (parent as ContextMenu).Items[i] as DependencyObject;
                }
                else if (parent is MenuItem)
                {
                    item = (parent as MenuItem).Items[i] as DependencyObject;
                }

                T itemType = item as T;
                if (itemType == null)
                {
                    foundItem = FindItem<T>(item, itemName);

                    if (foundItem != null) break;
                }
                else if (!string.IsNullOrEmpty(itemName))
                {
                    var frameworkElement = item as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == itemName)
                    {
                        foundItem = (T)item;
                        break;
                    }
                }
                else
                {
                    foundItem = (T)item;
                    break;
                }
            }

            return foundItem;
        }

        public static string FindPageName(this DependencyObject control)
        {
            if (control == null) return null;
            DependencyObject tempControl = control;

            while ((tempControl = VisualTreeHelper.GetParent(tempControl)) != null)
            {
                if ((tempControl is IWorkArea && tempControl is UserControl) ||
                    (tempControl is IWorkArea && tempControl is C1Window))
                {
                    return tempControl.GetType().FullName;
                }
            }

            return string.Empty;
        }

        public static object FindPageControl(this DependencyObject control)
        {
            if (control == null) return null;
            DependencyObject tempControl = control;

            while ((tempControl = VisualTreeHelper.GetParent(tempControl)) != null)
            {
                if ((tempControl is IWorkArea && tempControl is UserControl) ||
                    (tempControl is IWorkArea && tempControl is C1Window))
                {
                    return tempControl;
                }
            }

            return null;
        }
    }

    
}