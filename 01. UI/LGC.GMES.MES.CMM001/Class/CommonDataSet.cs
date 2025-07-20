/*************************************************************************************
 Created Date : 2015.09.30
      Creator : J.H.Lim
   Description : UI에서 공용으로 사용되어지는 Class(콤보 Set 등.)
--------------------------------------------------------------------------------------
 [Change History]
  2015.09.30 / J.H.Lim : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using System.Collections;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.CMM001.Class
{
	
	public partial class CommonDataSet
    {
        public CommonDataSet()
        {        
        }

		/// <summary>
		/// 콤보 바인딩 후 -ALL-, -Select-  추가 enum
		/// </summary>
		public enum ComboStatus
		{
			/// <summary>
			/// 콤보 바인딩 후 ALL 을 최상단에 표시
			/// </summary>
			ALL,

			/// <summary>
			/// 콤보 바인딩 후 Select 을 최상단에 표시 (필수선택 항목에 사용)
			/// </summary>
			SELECT,

			/// <summary>
			/// 콤보 바인딩 후 ALL_PRODUCT 을 최상단에 표시(작업지도서용)
			/// </summary>
			ALL_PRODUCT,

			/// <summary>
			/// 바인딩만 하고 끝 (바인딩후 제일 1번째 항목을 표시) 
			/// </summary>
			NONE
		}

        //전역변수
        public DataTable dtResult = new DataTable();

        BizDataSet _Biz = new BizDataSet();


		/// <summary>
		/// C1DataGrid에 특정 컬럼의 값이 있는 Row Index를 리턴함.
		/// </summary>
		/// <param name="c1Data">C1DataGrid </param>
		/// <param name="colHeader">찾고자 하는 Column명</param>
		/// <param name="selectValue">찾고자 하는 Value값</param>
		/// <returns></returns>
		public int getGridSelectedRow(C1.WPF.DataGrid.C1DataGrid c1Data, string colHeader, string selectValue)
		{
			int result = -1;

			for (int i = 0; i < c1Data.Rows.Count - c1Data.BottomRows.Count; i++)
			{
				if (DataTableConverter.GetValue(c1Data.Rows[i].DataItem, colHeader).ToString().Equals(selectValue))
				{
					result = i;
					return result;
				}
			}

			return result;
		}

        /// <summary>
        /// dsLotInfo 초기화 
        /// </summary>
        public void initDsLotInfo()
        {
            MyClass.dsLotInfo = new DataSet();
        }
        /// <summary>
        /// dsLotInfo초기화 : 특정한 Table 환경 추가
        /// </summary>
        /// <param name="strTableName"></param>
        /// <param name="dt"></param>
        public void AddDtCloneDsLotInfo(string strTableName, DataTable dt)
        {
            if (MyClass.dsLotInfo != null && MyClass.dsLotInfo.Tables != null && MyClass.dsLotInfo.Tables.Count > 0)
            {
                if (MyClass.dsLotInfo.Tables.Contains(strTableName))
                {
                    MyClass.dsLotInfo.Tables.Remove(strTableName);
                }
            }
            else
            {
                MyClass.dsLotInfo = new DataSet();
            }

            DataTable dtTarget = dt.Clone();
            //DataTable dtTarget = dt;

            dtTarget.TableName = strTableName;
            MyClass.dsLotInfo.Tables.Add(dtTarget);
        }

		/// <summary>
		/// dsLotInfo삭제 : 특정한 Table 삭제 20130618
		/// </summary>
		/// <param name="strTableName"></param>
		public void RemoveDtDsLotInfo(string strTableName)
		{
			if (MyClass.dsLotInfo != null && MyClass.dsLotInfo.Tables != null && MyClass.dsLotInfo.Tables.Count > 0)
			{
				if (MyClass.dsLotInfo.Tables.Contains(strTableName))
				{
					MyClass.dsLotInfo.Tables.Remove(strTableName);
				}
			}
			else
			{
				MyClass.dsLotInfo = new DataSet();
			}
		}

        /// <summary>
        /// dsLotInfo삭제 : 특정한 Table을 제외한 다른 Table 삭제 
        /// </summary>
        /// <param name="strTableName"></param>
        public void RemoveAllDtWithoutTargetDsLotInfo(string strTableName)
        {
            if (MyClass.dsLotInfo != null && MyClass.dsLotInfo.Tables != null && MyClass.dsLotInfo.Tables.Count > 0)
            {
                for (int i = 0; i < MyClass.dsLotInfo.Tables.Count; ++i)
                {
                    if (!MyClass.dsLotInfo.Tables[i].TableName.ToString().Equals(strTableName))
                    {
                        MyClass.dsLotInfo.Tables.Remove(MyClass.dsLotInfo.Tables[i].TableName);
                    }
                }
            }
            else
            {
                MyClass.dsLotInfo = new DataSet();
            }
        }

		/// <summary>
		/// dsLotInfo에 특정 Table의 값이 존재하는지 확인
		/// </summary>
		/// <param name="strDtName"></param>
		public bool ChkDataByDsLotInfo(string strDtName)
		{
			DataSet dataSet = MyClass.dsLotInfo;

			if (dataSet == null)
			{
				return false;
			}
			if (!dataSet.Tables.Contains(strDtName))
			{
				return false;
			}
			if (dataSet.Tables[strDtName].Rows.Count <= 0)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// DATATable을 주어진 DATA로 생성
		/// </summary>
		/// <param name="strTableName"></param>
		/// <param name="colName"></param>
		/// <param name="colValue"></param>
		/// <returns></returns>
		public DataTable CreateDataTable(string strTableName, string[] colName, string[,] colValue)
		{
			DataTable dt = new DataTable();
			DataColumn col;
			DataRow row;
			dt.TableName = strTableName;

			for (int i = 0; i < colName.Length; ++i)
			{
				col = new DataColumn();
				col.ColumnName = colName[i].ToString();
				dt.Columns.Add(col);
			}
			for (int r = 0; r < colValue.GetLength(0); ++r)
			{
				row = dt.NewRow();
				for (int c = 0; c < colValue.GetLength(1); ++c)
				{
					row[colName[c]] = colValue[r, c].ToString();
				}
				dt.Rows.Add(row);
			}
			return dt;
		}

		/// <summary>
		/// grid에서 datatable 반환
		/// </summary>
		/// <param name="grid"></param>
		/// <param name="colName"></param>
		/// <returns></returns>
		public DataTable getDataTableByGrid(C1.WPF.DataGrid.C1DataGrid grid, string[] colName)
		{
			// 0 번째 행은 column명
			DataTable dt = new DataTable();

			DataColumn col;
			DataRow row;

			if (grid == null || grid.Rows.Count <= 1)
			{
				return null;
			}
			if (colName.Length != grid.Columns.Count)
			{
				return null;
			}

			for (int i = 0; i < colName.Length; ++i)
			{
				col = new DataColumn();
				col.ColumnName = colName[i].ToString();
				dt.Columns.Add(col);
			}
			for (int r = 0; r < grid.Rows.Count - grid.BottomRows.Count; ++r)
			{
				row = dt.NewRow();
				if (grid.Rows[r].Visibility == System.Windows.Visibility.Visible)
				{
					for (int c = 0; c < grid.Columns.Count; ++c)
					{
						if (grid.GetCell(r, c) != null)
							row[colName[c].ToString()] = Util.NVC(grid.GetCell(r, c).Value);
					}
					dt.Rows.Add(row);
				}
			}
			return dt;
		}

        //===============================================================================================================

        public void SetCOR_SEL_SHOP_ORG_G_TEST(ComboBox cbo, string sShopID, string sOrgCD, string sMultiOrgCD, string sProdID, string sModlID, string sProdType, string sMultisProdType, ComboStatus sComboStatus, ControlsLibrary.LoadingIndicator loadingIndicator, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("PRODNAME", typeof(string));
            dtResult.Columns.Add("PRODID", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V6_음극", "XA20" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.5B 음극", "XA6" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.4 음극", "XAB" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V4 - 2 음극", "XAD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "JH2 음극", "XBD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P2.7 음극", "XBG" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "F2 음극", "XBI" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "B10A 음극", "XBK" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "N2.1 음극", "XBN" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "A5 - A 음극(P36)", "XBP" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.5B 양극", "XC6" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V4 - 2 양극", "XCD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "L4A 양극", "XD6" };
            dtResult.Rows.Add(newRow);

            cbo.ItemsSource = SetCombo(dtResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
            cbo.SelectedIndex = 0;

            if (ACTION_COMPLETED != null)
            {
                Exception searchException = null;
                ACTION_COMPLETED(dtResult, searchException);
            }

            //string[] orgcode = CustomConfig.Instance.CONFIG_COMMON_ORG;
            //Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", Logger.MESSAGE_OPERATION_START);

            //DataTable searchConditionTable = _Biz.GetCOR_SEL_SHOP_ORG_G();
            //DataRow searchCondition = searchConditionTable.NewRow();
            //searchCondition["LANGID"] = LoginInfo.LANGID;
            //searchCondition["SHOPID"] = CustomConfig.Instance.CONFIG_COMMON_SHOP;
            //searchCondition["ORG_CODE"] = sOrgCD;   //Multi Org중에 특정Org만 가져올 경우 사용
            //searchCondition["MULTI_ORG_CODE"] = Util.GetMultOrgCode(orgcode);

            //searchConditionTable.Rows.Add(searchCondition);

            //new ClientProxy().ExecuteService("COR_SEL_SHOP_ORG_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            //{
            //    try
            //    {
            //        if (searchException != null)
            //        {
            //            ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //            return;
            //        }

            //        //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
            //        cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

            //        if (cbo.Items.Count > 0)
            //            cbo.SelectedIndex = 0;

            //        if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);
            //    }
            //    catch (Exception ex)
            //    {
            //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //        Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", ex);
            //    }
            //    finally
            //    {
            //        Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", Logger.MESSAGE_OPERATION_END);
            //    }
            //}
            //);
        }

        public void SetCOR_SEL_SHOP_ORG_G_TEST(C1ComboBox cbo, string sShopID, string sOrgCD, string sMultiOrgCD, string sProdID, string sModlID, string sProdType, string sMultisProdType, ComboStatus sComboStatus, ControlsLibrary.LoadingIndicator loadingIndicator, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("PRODNAME", typeof(string));
            dtResult.Columns.Add("PRODID", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V6_음극", "XA20" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.5B 음극", "XA6" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.4 음극", "XAB" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V4 - 2 음극", "XAD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "JH2 음극", "XBD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P2.7 음극", "XBG" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "F2 음극", "XBI" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "B10A 음극", "XBK" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "N2.1 음극", "XBN" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "A5 - A 음극(P36)", "XBP" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.5B 양극", "XC6" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V4 - 2 양극", "XCD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "L4A 양극", "XD6" };
            dtResult.Rows.Add(newRow);

            cbo.ItemsSource = SetCombo(dtResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
            cbo.SelectedIndex = 0;

            if (ACTION_COMPLETED != null)
            {
                Exception searchException = null;
                ACTION_COMPLETED(dtResult, searchException);
            }

            //string[] orgcode = CustomConfig.Instance.CONFIG_COMMON_ORG;
            //Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", Logger.MESSAGE_OPERATION_START);

            //DataTable searchConditionTable = _Biz.GetCOR_SEL_SHOP_ORG_G();
            //DataRow searchCondition = searchConditionTable.NewRow();
            //searchCondition["LANGID"] = LoginInfo.LANGID;
            //searchCondition["SHOPID"] = CustomConfig.Instance.CONFIG_COMMON_SHOP;
            //searchCondition["ORG_CODE"] = sOrgCD;   //Multi Org중에 특정Org만 가져올 경우 사용
            //searchCondition["MULTI_ORG_CODE"] = Util.GetMultOrgCode(orgcode);

            //searchConditionTable.Rows.Add(searchCondition);

            //new ClientProxy().ExecuteService("COR_SEL_SHOP_ORG_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            //{
            //    try
            //    {
            //        if (searchException != null)
            //        {
            //            ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //            return;
            //        }

            //        //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
            //        cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

            //        if (cbo.Items.Count > 0)
            //            cbo.SelectedIndex = 0;

            //        if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);
            //    }
            //    catch (Exception ex)
            //    {
            //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //        Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", ex);
            //    }
            //    finally
            //    {
            //        Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", Logger.MESSAGE_OPERATION_END);
            //    }
            //}
            //);
        }

        public void SetCOR_SEL_MLOT_TP_CODE_CBO_G_TEST(ControlsLibrary.MultiSelectionBox cbo, string sShopID, string sOrgCode, string sMultOrgCode, string sMlotTpCode, string sSelStoreFlag, string sMlotRcvGrCode, ControlsLibrary.LoadingIndicator loadingIndicator, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("CBO_NAME", typeof(string));
            dtResult.Columns.Add("CBO_CODE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V6_음극", "XA20" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.5B 음극", "XA6" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.4 음극", "XAB" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V4 - 2 음극", "XAD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "JH2 음극", "XBD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P2.7 음극", "XBG" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "F2 음극", "XBI" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "B10A 음극", "XBK" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "N2.1 음극", "XBN" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "A5 - A 음극(P36)", "XBP" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "P1.5B 양극", "XC6" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "V4 - 2 양극", "XCD" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { "L4A 양극", "XD6" };
            dtResult.Rows.Add(newRow);

            cbo.ItemsSource = DataTableConverter.Convert(dtResult);
            if (dtResult.Rows.Count > 0)
            {
                cbo.CheckAll();
            }

            if (ACTION_COMPLETED != null)
            {
                Exception searchException = null;
                ACTION_COMPLETED(dtResult, searchException);
            }

            //Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotType_Loaded", Logger.MESSAGE_OPERATION_START);

            //DataTable searchConditionTable = _Biz.GetCOR_SEL_MLOT_TP_CODE_CBO_G();
            //DataRow searchCondition = searchConditionTable.NewRow();
            //searchCondition["LANGID"] = LoginInfo.LANGID;
            //searchCondition["SHOPID"] = sShopID;
            //searchCondition["ORG_CODE"] = sOrgCode;
            //searchCondition["MULTI_ORG_CODE"] = sMultOrgCode;
            //searchCondition["MLOT_TP_CODE"] = sMlotTpCode;
            //searchCondition["SEL_STORE_FLAG"] = sSelStoreFlag;
            //searchCondition["MLOT_RCV_GR_CODE"] = sMlotRcvGrCode;
            //searchCondition["SUB_FLAG"] = "N";

            //searchConditionTable.Rows.Add(searchCondition);

            //loadingIndicator.Visibility = Visibility.Visible;
            //new ClientProxy().ExecuteService("COR_SEL_MLOT_TP_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            //{
            //    try
            //    {
            //        if (searchException != null)
            //        {
            //            loadingIndicator.Visibility = Visibility.Collapsed;
            //            ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //            return;
            //        }

            //        cbo.ItemsSource = DataTableConverter.Convert(searchResult);
            //        if (searchResult.Rows.Count > 0)
            //        {
            //            cbo.CheckAll();
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        loadingIndicator.Visibility = Visibility.Collapsed;
            //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            //        Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotType_Loaded", ex);
            //    }
            //    finally
            //    {
            //        loadingIndicator.Visibility = Visibility.Collapsed;
            //        Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", Logger.MESSAGE_OPERATION_END);
            //    }
            //}
            //);
        }

        //===============================================================================================================

        /// <summary>
		/// Lot Type 정보 설정
        /// </summary>
        /// <param name="cboLotType"> 콤보박스 Object</param>
        public void COR_SEL_LOTTYPE_CBO(ComboBox cboLotType)
        {
            Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("COR_SEL_LOTTYPE_CBO", "INDATA", "OUTDATA", IndataTable, (LotTypeResult, LotTypeException) =>
            {
                if (LotTypeException != null)
                {
					ControlsLibrary.MessageBox.Show(LotTypeException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }

                cboLotType.ItemsSource = DataTableConverter.Convert(LotTypeResult);

				if (LotTypeResult.Rows.Count > 0)
                    cboLotType.SelectedIndex = 0;

				Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_END);
            }
            );
        }

        /// <summary>
        /// Lot Type 정보 설정
        /// </summary>
        /// <param name="cboLotType"> 콤보박스 Object</param>
        public void COR_SEL_LOTTYPE_CBO(ComboBox cbo, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("COR_SEL_LOTTYPE_CBO", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    // cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );	
        }

		/// <summary>
		/// Lot Type 정보 설정(PM LOT생성(RePelicle))
		/// </summary>
		/// <param name="cboLotType"> 콤보박스 Object</param>
		public void COR_SEL_LOTTYPE_RPMS_PM(ComboBox cbo, string dsgnProdTpCd,  ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("DSGN_PROD_TP_CODE", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["DSGN_PROD_TP_CODE"] = dsgnProdTpCd;

			IndataTable.Rows.Add(Indata);

			new ClientProxy().ExecuteService("COR_SEL_LOTTYPE_RPMS_PM", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}

        /// <summary>
        /// [TS] - CREATE LOT - Lot Type 정보 설정
        /// </summary>
        /// <param name="cboLotType"> 콤보박스 Object</param>
        public void SetCUS_SEL_LOTCREATE_LOTTYPE_CBO(ComboBox cbo, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("CUS_SEL_LOTCREATE_LOTTYPE_CBO", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }


		/// <summary>
		/// 모델 정보 설정
		/// </summary>
		/// <param name="cboProd">콤보박스 Object</param>
		public void COR_SEL_PRODUCT_CBO(ComboBox cboProd, ComboStatus comboStat = ComboStatus.NONE)
		{
			Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_PRODUCT_CBO_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				if (Exception != null)
				{
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				//// 첫행에 Item 추가
				//if (itemString != string.Empty)
				//{
				//	DataRow rowResult = DtResult.NewRow();
				//	rowResult[cboProd.DisplayMemberPath.ToString()] = itemString;
				//	rowResult[cboProd.SelectedValuePath.ToString()] = string.Empty;
				//	DtResult.Rows.InsertAt(rowResult, 0);
				//}
				//cboProd.ItemsSource = DataTableConverter.Convert(DtResult);

				if (comboStat == ComboStatus.NONE)
				{
					cboProd.ItemsSource = DataTableConverter.Convert(DtResult);
				}
				else
				{
					cboProd.ItemsSource = SetCombo(DtResult, cboProd.SelectedValuePath.ToString(), cboProd.DisplayMemberPath.ToString(), comboStat);
				}
				if (DtResult.Rows.Count > 0)
					cboProd.SelectedIndex = 0;

				Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_END);
			}
			);

		}

        /// <summary>
		/// AREA 정보 설정
        /// </summary>
        /// <param name="cboArea">콤보박스 Object</param>
		public void COR_SEL_AREA_BY_SITE_CBO(ComboBox cboArea, string sSelValue = "")
        {
            Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            //IndataTable.Columns.Add("SITEID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            //Indata["SITEID"] = LoginInfo.LANGID;
            IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_AREA_BY_SITE_CBO_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
            {
                if (Exception != null)
                {
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }

                cboArea.ItemsSource = DataTableConverter.Convert(DtResult);

				if (sSelValue != "" && (from DataRow DRow in DtResult.Rows where DRow["AREAID"].Equals(sSelValue) select DRow).Count() > 0)
				{
					// cboArea.SelectedValue = sSelValue;
					for (int i = 0; i < cboArea.Items.Count; i++)
					{
						if (sSelValue == ((DataRowView)cboArea.Items[i]).Row.ItemArray[0].ToString())
						{
							cboArea.SelectedIndex = i;
							return;
						}
					}
				}
				else if (DtResult.Rows.Count > 0)
					cboArea.SelectedIndex = 0;

				Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_END);

            }
            );
        }

		/// <summary>
		/// Shop 과 Site ID 정보 가져오기(From AreaID)
		/// </summary>
		/// <param name="lblShop"></param>
		/// <param name="sAreaID"></param>
		public void GetShop_SiteFromAreaID(TextBlock lblShop, TextBlock lblSite, string sAreaID)
		{
			lblShop.Text = string.Empty;
			lblSite.Text = string.Empty;

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("AREAID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["AREAID"] = sAreaID;
			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_AREA_TBL", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				if (Exception != null)
				{
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				if (DtResult.Rows.Count > 0)
				{
					lblShop.Text = DtResult.Rows[0]["SHOPID"].ToString();
					GetSiteIDFromShopID(lblSite, lblShop.Text.ToString());
				}
			}
			);

		}

		/// <summary>
		/// Shop ID 정보 가져오기(From AreaID)
		/// </summary>
		/// <param name="lblShop"></param>
		/// <param name="sAreaID"></param>
		public void GetShopIDFromAreaID(TextBlock lblShop, string sAreaID)
		{
			lblShop.Text = string.Empty;

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("AREAID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["AREAID"] = sAreaID;
			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_AREA_TBL", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				if (Exception != null)
				{
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				if (DtResult.Rows.Count > 0)
					lblShop.Text = DtResult.Rows[0]["SHOPID"].ToString();
			}
			);

		}

		/// <summary>
		/// Site ID 정보 가져오기(From ShopID)
		/// </summary>
		/// <param name="lblShop"></param>
		/// <param name="sShopID"></param>
		public void GetSiteIDFromShopID(TextBlock lblSite, string sShopID)
		{
			lblSite.Text = string.Empty;

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("SHOPID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["SHOPID"] = sShopID;
			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_SHOP_TBL", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				if (Exception != null)
				{
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				if (DtResult.Rows.Count > 0)
					lblSite.Text = DtResult.Rows[0]["SITEID"].ToString();
			}
			);

		}

        /// <summary>
		/// 라인 정보 설정
        /// </summary>
        /// <param name="cboLine">콤보박스 Object</param>
        /// <param name="sArea">Area ID</param>
		public void COR_SEL_EQUIPMENTSEGMENT_CBO(ComboBox cboLine, string sArea, string sSelValue = "")
        {
            Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREAID"] = sArea;
            IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_EQUIPMENTSEGMENT_CBO_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
            {
                if (Exception != null)
                {
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }

                cboLine.ItemsSource = DataTableConverter.Convert(DtResult);

				if (sSelValue != "" && (from DataRow DRow in DtResult.Rows where DRow["EQSGID"].Equals(sSelValue) select DRow).Count() > 0)
				{
					// cboProd.SelectedValue = sSelValue;
					for (int i = 0; i < cboLine.Items.Count; i++)
					{
						if (sSelValue == ((DataRowView)cboLine.Items[i]).Row.ItemArray[0].ToString())
						{
							cboLine.SelectedIndex = i;
							return;
						}
					}
				}
				else if (DtResult.Rows.Count > 0)
					cboLine.SelectedIndex = 0;

				Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_END);

            }
            );
        }


		/// <summary>
		/// 라우터 Flow 정보 설정
		/// </summary>
		/// <param name="cboFlow">콤보박스 Object</param>
		/// <param name="sRoutID">Route ID</param>
		/// <param name="sFlowType">Flow Type</param>
		public void COR_SEL_ROUTEFLOW_BY_ROUTEID(ComboBox cboFlow, string sRoutID, string sFlowType, string sSelValue = "")
		{
			Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("ROUTID", typeof(string));
			IndataTable.Columns.Add("FLOWTYPE", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["ROUTID"] = sRoutID;
			Indata["FLOWTYPE"] = sFlowType;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_ROUTEFLOW_BY_ROUTEID_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				if (Exception != null)
				{
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				cboFlow.ItemsSource = DataTableConverter.Convert(DtResult);

				if (sSelValue != "" && (from DataRow DRow in DtResult.Rows where DRow["FLOWID"].Equals(sSelValue) select DRow).Count() > 0)
				{
					// cboProd.SelectedValue = sSelValue;
					for (int i = 0; i < cboFlow.Items.Count; i++)
					{
						if (sSelValue == ((DataRowView)cboFlow.Items[i]).Row.ItemArray[0].ToString())
						{
							cboFlow.SelectedIndex = i;
							return;
						}
					}
				}
				else if (DtResult.Rows.Count > 0)
					cboFlow.SelectedIndex = 0;

				Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_END);

			}
			);

		}

		/// <summary>
		/// Start 공정 정보 설정
		/// </summary>
		/// <param name="cboProc">콤보박스 Object</param>
		/// <param name="sRoutID">Route ID</param>
		/// <param name="sFlowID">Flow ID</param>
		public void COR_SEL_STARTPROC_BY_ROUTEFLOW(ComboBox cboProc, string sRoutID, string sFlowID)
		{
			Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("ROUTID", typeof(string));
			IndataTable.Columns.Add("FLOWID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["ROUTID"] = sRoutID;
			Indata["FLOWID"] = sFlowID;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_STARTPROC_BY_ROUTEFLOW", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				if (Exception != null)
				{
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				cboProc.ItemsSource = DataTableConverter.Convert(DtResult);

				if (DtResult.Rows.Count > 0)
					cboProc.SelectedIndex = 0;

				Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_END);
			}
			);
		}


		/// <summary>
		/// Start 공정 정보 설정(PM전용)
		/// </summary>
		/// <param name="cboProc">콤보박스 Object</param>
		/// <param name="sRoutID">Route ID</param>
		/// <param name="sFlowID">Flow ID</param>
		public void BR_CUS_GET_CREATE_LOT_STARPROC_PM(ComboBox cboProc, string sRoutID, string sFlowID, string attribute1)
		{
			Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("ROUTID", typeof(string));
			IndataTable.Columns.Add("FLOWID", typeof(string));
			IndataTable.Columns.Add("ATTRIBUTE1", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["ROUTID"] = sRoutID;
			Indata["FLOWID"] = sFlowID;
			Indata["ATTRIBUTE1"] = attribute1;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("BR_CUS_GET_CREATE_LOT_STARPROC_PM", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				if (Exception != null)
				{
					ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				cboProc.ItemsSource = DataTableConverter.Convert(DtResult);

				if (DtResult.Rows.Count > 0)
					cboProc.SelectedIndex = 0;

				Logger.Instance.WriteLine("[FRAME] Model Combobox", Logger.MESSAGE_OPERATION_END);
			}
			);
		}

        //수입검사 결과서 입력 조회
        public void COR_SEL_MATERIALITEM_IQC_SPEC_G(C1DataGrid dataGrid, string sOrgCode, string sMtrlGrCode, string sMakerCode, string sMtrlClassCode, string sMtrlGrdCode, string InspTpCode, string sClctItem)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_MATERIALITEM_IQC_SPEC_G", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("ORG_CODE", typeof(string));
            IndataTable.Columns.Add("MTRL_GR_CODE", typeof(string));
            IndataTable.Columns.Add("MAKER_CODE", typeof(string));
            IndataTable.Columns.Add("MTRL_CLASS_CODE", typeof(string));
            IndataTable.Columns.Add("MTRL_GRD_CODE", typeof(string));
            IndataTable.Columns.Add("INSP_TP_CODE", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["ORG_CODE"] = sOrgCode;
            Indata["MTRL_GR_CODE"] = sMtrlGrCode;
            Indata["MAKER_CODE"] = sMakerCode;
            Indata["MTRL_CLASS_CODE"] = sMtrlClassCode;
            Indata["MTRL_GRD_CODE"] = sMtrlGrdCode;
            Indata["INSP_TP_CODE"] = InspTpCode;
            Indata["CLCTITEM"] = sClctItem;

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("COR_SEL_MATERIALITEM_IQC_SPEC_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
            {
				try
				{
					if (Exception != null)
					{
						ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

                    DtResult.Columns.Add("CLCTVAL01", typeof(string));
                    DtResult.Columns.Add("CLCTVAL02", typeof(string));
                    DtResult.Columns.Add("CLCTVAL03", typeof(string));
                    DtResult.Columns.Add("CLCTVAL04", typeof(string));
                    DtResult.Columns.Add("CLCTVAL05", typeof(string));
                    DtResult.Columns.Add("CLCTVAL06", typeof(string));
                    DtResult.Columns.Add("CLCTVAL07", typeof(string));
                    DtResult.Columns.Add("CLCTVAL08", typeof(string));
                    DtResult.Columns.Add("CLCTVAL09", typeof(string));
                    DtResult.Columns.Add("CLCTVAL10", typeof(string));
                    DtResult.Columns.Add("JUDGE", typeof(string));
                    DtResult.Columns.Add("GRADE", typeof(string));
                    DtResult.Columns.Add("CLCTAVG", typeof(decimal));
                    
					dataGrid.ItemsSource = DataTableConverter.Convert(DtResult);
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "MaterialLotInput", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_MATERIALITEM_IQC_SPEC_G", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
            
        }

		/// <summary>
		/// LOT이 진행해 온 공정을 Set.
		/// </summary>
		public void COR_SEL_WIPHISTORY_BY_LOTID_G(ComboBox cbo, string lotID, string addItem = "")
		{
			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("LOTID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["LOTID"] = lotID;
			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_WIPHISTORY_BY_LOTID_G", "INDATA", "OUTDATA", IndataTable, (DtResult, dException) =>
			{
				if (dException != null)
				{
					ControlsLibrary.MessageBox.Show(dException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				if (addItem != "")
				{
					DataRow DRow = DtResult.NewRow();
					DRow["PROCID"] = "";
					DRow["PROCNAME"] = addItem;
					DtResult.Rows.InsertAt(DRow, 0);
				}
				cbo.ItemsSource = DataTableConverter.Convert(DtResult);
				if (DtResult.Rows.Count > 0)
					cbo.SelectedIndex = 0;
			}
			);
		}

		/// <summary>
		/// 공정에 해당하는 설비를 Set.
		/// </summary>
		public void COR_SEL_PROCESSEQUIPMENT_PROCID_G(ComboBox cbo, string procID, string addItem = "")
		{
			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("PROCID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["PROCID"] = procID;
			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_PROCESSEQUIPMENT_PROCID_G", "INDATA", "OUTDATA", IndataTable, (DtResult, dException) =>
			{
				if (dException != null)
				{
					ControlsLibrary.MessageBox.Show(dException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				if (addItem != "")
				{
					DataRow DRow = DtResult.NewRow();
					DRow["EQPTID"] = "";
					DRow["EQPTNAME"] = addItem;
					DtResult.Rows.InsertAt(DRow, 0);
				}
				cbo.ItemsSource = DataTableConverter.Convert(DtResult);
				if (DtResult.Rows.Count > 0)
					cbo.SelectedIndex = 0;
			}
			);
		}


		/// <summary>
		/// Area, 공정에 해당하는 설비를 Set.
		/// </summary>
		public void COR_SEL_PROCESSEQUIPMENT_PROCID_G(ComboBox cbo,string areaID, string procID, string addItem = "")
		{
			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("AREAID", typeof(string));
			IndataTable.Columns.Add("PROCID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["AREAID"] = areaID;
			Indata["PROCID"] = procID;
			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("CUS_SEL_EQUIPMENT_CBO_TS1", "INDATA", "OUTDATA", IndataTable, (DtResult, dException) =>
			{
				if (dException != null)
				{
					ControlsLibrary.MessageBox.Show(dException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					return;
				}

				if (addItem != "")
				{
					DataRow DRow = DtResult.NewRow();
					DRow["EQPTID"] = "";
					DRow["EQPTNAME"] = addItem;
					DtResult.Rows.InsertAt(DRow, 0);
				}
				cbo.ItemsSource = DataTableConverter.Convert(DtResult);
				if (DtResult.Rows.Count > 0)
					cbo.SelectedIndex = 0;
			}
			);
		}

		

		/// <summary>
		/// DATATABLE에 KEY 컬럼과 VALUE 컬럼에 한행을 추가하기 위한 함수
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="sValue">Datatable의 컬럼1</param>
		/// <param name="sDisplay">Datatable의 컬럼2</param>
		public IEnumerable SetCombo(DataTable dt, string sValue, string sDisplay, ComboStatus select)
		{
			if (select.Equals(ComboStatus.ALL))
			{
				SetCombo_All(dt, sValue, sDisplay);
			}
			else if (select.Equals(ComboStatus.SELECT))
			{
				SetCombo_Select(dt, sValue, sDisplay);
			}
			else if (select.Equals(ComboStatus.ALL_PRODUCT))
			{
				SetCombo_AllProduct(dt, sValue, sDisplay);
			}
			return dt.Copy().AsDataView();
		}

		/// <summary>
		/// 모든콤보박스의 첫번째 컬럼에 '-ALL-' 값을 넣기위한 함수
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="sValue">combo의 Value</param>
		/// <param name="sDisplay">combo의 Display</param>
		private void SetCombo_All(DataTable dt, string sValue, string sDisplay)
		{
			DataRow dr = dt.NewRow();
			dr[sDisplay] = "-ALL-";
			dr[sValue] = "";
			dt.Rows.InsertAt(dr, 0);
		}

		/// <summary>
		/// 모든콤보박스의 첫번째 컬럼에 'ALL_PRODUCT' 값을 넣기위한 함수(작업지도서용)
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="sValue">combo의 Value</param>
		/// <param name="sDisplay">combo의 Display</param>
		private void SetCombo_AllProduct(DataTable dt, string sValue, string sDisplay)
		{
			DataRow dr = dt.NewRow();
			dr[sDisplay] = "ALL_PRODUCT";
			dr[sValue] = "ALL_PRODUCT";
			dt.Rows.InsertAt(dr, 0);
		}

		/// <summary>
		/// 모든콤보박스의 첫번째 컬럼에 첫번째값 표시
		/// </summary>
		/// <param name="dt"></param>
		private DataTable SetCombo_None(DataTable dt)
		{
			return dt;
		}

		/// <summary>
		/// 모든콤보박스의 첫번째 컬럼에 -Select- 값을 넣기위한 함수
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="sValue">combo의 Value</param>
		/// <param name="sDisplay">combo의 Display</param>
		private void SetCombo_Select(DataTable dt, string sValue, string sDisplay)
		{
			DataRow dr = dt.NewRow();

			dr[sDisplay] = "-SELECT-";
			dr[sValue] = "";

			dt.Rows.InsertAt(dr, 0);
		}


		#region [※ COMMONCODE 테이블의 CMCDTYPE별 CODE 정보 불러오기]
		/// <summary>
		/// 공통코드 불러오기
		/// </summary>
		/// <param name="cboProc">콤보박스 Object</param>
		/// <param name="sCmcdType">코드 TYPE</param>
		/// <param name="sComboStatus">바인딩 후 ALL/Select 추가여부</param> 
        public void COR_SEL_COMMONCODE_G(ComboBox cbo, string sCmcdType, ComboStatus sComboStatus, string sSelValue = "")
        {
            COR_SEL_COMMONCODE_G(cbo, sCmcdType, sComboStatus, sSelValue, null);
        }
        public void COR_SEL_COMMONCODE_G(ComboBox cbo, string sCmcdType, ComboStatus sComboStatus, string sSelValue, Action<DataTable, Exception> ACTION_COMPLETED)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("CMCDTYPE", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["CMCDTYPE"] = sCmcdType;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_COMMONCODE_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				try
				{
					if (Exception != null)
					{
						ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					if (sComboStatus == CommonDataSet.ComboStatus.NONE)
					{
						cbo.ItemsSource = DataTableConverter.Convert(DtResult);
					}
					else
					{
						cbo.ItemsSource = SetCombo(DtResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
					}

					if (sSelValue != "" && (from DataRow DRow in DtResult.Rows where DRow["CMCODE"].Equals(sSelValue) select DRow).Count() > 0)
					{
						for (int i = 0; i < cbo.Items.Count; i++)
						{
							if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
							{
								cbo.SelectedIndex = i;
								return;
							}
						}
					}
					else if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(DtResult, Exception);
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}

        public void COR_SEL_COMMONCODE_ATTRIBUTE_G(ComboBox cbo, string sCmcdType, string sAttribute1, string sAttribute2, string sAttribute3, string sAttribute4, string sAttribute5, ComboStatus sComboStatus, string sSelValue = "")
        {
            COR_SEL_COMMONCODE_ATTRIBUTE_G(cbo, sCmcdType, sAttribute1, sAttribute2, sAttribute3, sAttribute4, sAttribute5, sComboStatus, sSelValue, null);
        }
        public void COR_SEL_COMMONCODE_ATTRIBUTE_G(ComboBox cbo, string sCmcdType, string sAttribute1, string sAttribute2, string sAttribute3, string sAttribute4, string sAttribute5, ComboStatus sComboStatus, string sSelValue, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE1", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE2", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE3", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE4", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE5", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = sCmcdType;
            Indata["ATTRIBUTE1"] = sAttribute1;
            Indata["ATTRIBUTE2"] = sAttribute2;
            Indata["ATTRIBUTE3"] = sAttribute3;
            Indata["ATTRIBUTE4"] = sAttribute4;
            Indata["ATTRIBUTE5"] = sAttribute5;

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("COR_SEL_COMMONCODE_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
            {
                try
                {
                    if (Exception != null)
                    {
                        ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    if (sComboStatus == CommonDataSet.ComboStatus.NONE)
                    {
                        cbo.ItemsSource = DataTableConverter.Convert(DtResult);
                    }
                    else
                    {
                        cbo.ItemsSource = SetCombo(DtResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
                    }

                    if (sSelValue != "" && (from DataRow DRow in DtResult.Rows where DRow["CMCODE"].Equals(sSelValue) select DRow).Count() > 0)
                    {
                        for (int i = 0; i < cbo.Items.Count; i++)
                        {
                            if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
                            {
                                cbo.SelectedIndex = i;
                                return;
                            }
                        }
                    }
                    else if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(DtResult, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        //MultiSelectionBox
        public void COR_SEL_COMMONCODE_Multi_G(MultiSelectionBox cbo, string sCmcdType)
        {
            COR_SEL_COMMONCODE_Multi_G(cbo, sCmcdType, null);
        }
        public void COR_SEL_COMMONCODE_Multi_G(MultiSelectionBox cbo, string sCmcdType, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = sCmcdType;

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("COR_SEL_COMMONCODE_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
            {
                try
                {
                    if (Exception != null)
                    {
                        ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    cbo.ItemsSource = DataTableConverter.Convert(DtResult);
                    cbo.CheckAll();
                   

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(DtResult, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_G", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
		#endregion

        #region [※ 이상부적합의 예약 HOLD 공정 정보 불러오기]
        public void COR_SEL_COMMONCODE_HOLDPROC_G(ComboBox cbo, string sCmcdType, string sAreaId, string sProdId, ComboStatus sComboStatus, string sSelValue)
        {
            if (sAreaId.Equals("")) return;

            Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_HOLDPROC_G", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));


            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = sCmcdType;
            Indata["AREAID"] = sAreaId.Equals("") ? null : sAreaId;
            Indata["PROCID"] = sAreaId.Equals("") ? null : sProdId;


            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("COR_SEL_COMMONCODE_HOLDPROC_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
            {
                try
                {
                    if (Exception != null)
                    {
                        ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    if (sComboStatus == CommonDataSet.ComboStatus.NONE)
                    {
                        cbo.ItemsSource = DataTableConverter.Convert(DtResult);
                    }
                    else
                    {
                        cbo.ItemsSource = SetCombo(DtResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
                    }

                    if (sSelValue != "" && (from DataRow DRow in DtResult.Rows where DRow["CMCODE"].Equals(sSelValue) select DRow).Count() > 0)
                    {
                        for (int i = 0; i < cbo.Items.Count; i++)
                        {
                            if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
                            {
                                cbo.SelectedIndex = i;
                                return;
                            }
                        }
                    }
                    else if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_HOLDPROC_G", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_HOLDPROC_G", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ ORG별 COMMONCODE 테이블의 CMCDTYPE별 CODE 정보 불러오기]
		/// <summary>
		/// 공통코드 불러오기
		/// </summary>
		/// <param name="cboProc">콤보박스 Object</param>
		/// <param name="sCmcdType">코드 TYPE</param>
		/// <param name="sComboStatus">바인딩 후 ALL/Select 추가여부</param> 
		public void COR_SEL_COMMONCODE_BY_ORG_G(ComboBox cbo, string orgCode, string sCmcdType, ComboStatus sComboStatus, string sSelValue = "")
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_BY_ORG_G", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("ORG_CODE", typeof(string));
			IndataTable.Columns.Add("CMCDTYPE", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["ORG_CODE"] = orgCode == "TS2" ? "TS1" : orgCode;
			Indata["CMCDTYPE"] = sCmcdType;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_COMMONCODE_BY_ORG_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				try
				{
					if (Exception != null)
					{
						ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					if (sComboStatus == CommonDataSet.ComboStatus.NONE)
					{
						cbo.ItemsSource = DataTableConverter.Convert(DtResult);
					}
					else
					{
						cbo.ItemsSource = SetCombo(DtResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
					}

					if (sSelValue != "" && (from DataRow DRow in DtResult.Rows where DRow["CMCODE"].Equals(sSelValue) select DRow).Count() > 0)
					{
						for (int i = 0; i < cbo.Items.Count; i++)
						{
							if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
							{
								cbo.SelectedIndex = i;
								return;
							}
						}
					}
					else if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_BY_ORG_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_COMMONCODE_BY_ORG_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ ORG 공통 코드 정보 불러오기]
		/// <summary>
		/// ORG 공통 코드 조회
		/// </summary>
		/// <param name="cbo"></param>
		/// <param name="sPdgrId"></param>
		/// <param name="sComboStatus"></param>
        public void SetCOR_SEL_ORG_COM_CODE_CBO_G(ComboBox cbo, string sShopID, string sOrgCd, string sMultiOrgCd, string sCmcdType, string sCmCode, ComboStatus sComboStatus, Action<DataTable, Exception> ACTION_COMPLETED = null)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCmCode_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_ORG_COM_CODE_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = sShopID;
			searchCondition["ORG_CODE"] = sOrgCd;
			searchCondition["MULTI_ORG_CODE"] = sMultiOrgCd;
			searchCondition["CMCDTYPE"] = sCmcdType;
			searchCondition["CMCODE"] = sCmCode;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_ORG_COM_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}
                    if (cbo != null)
                    {
                        //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                        cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                        if (cbo.Items.Count > 0)
                            cbo.SelectedIndex = 0;
                    }
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCmCode_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCmCode_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}

        public void SetCOR_SEL_ORG_COM_CODE_CBO_G(ComboBox cbo, string sShopID, string sOrgCd, string sMultiOrgCd, string sCmcdType, string sCmCode, string sATTR1, string sATTR2, string sATTR3, string sATTR4, string sATTR5, ComboStatus sComboStatus, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCmCode_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ORG_COM_CODE_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["ORG_CODE"] = sOrgCd;
            searchCondition["MULTI_ORG_CODE"] = sMultiOrgCd;
            searchCondition["CMCDTYPE"] = sCmcdType;
            searchCondition["CMCODE"] = sCmCode;
            searchCondition["ATTRIBUTE1"] = sATTR1;
            searchCondition["ATTRIBUTE2"] = sATTR2;
            searchCondition["ATTRIBUTE3"] = sATTR3;
            searchCondition["ATTRIBUTE4"] = sATTR4;
            searchCondition["ATTRIBUTE5"] = sATTR5;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ORG_COM_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }
                    if (cbo != null)
                    {
                        //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                        cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                        if (cbo.Items.Count > 0)
                            cbo.SelectedIndex = 0;
                    }
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCmCode_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCmCode_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
		#endregion

		#region [※ AREA 정보 불러오기 - SHOP별]
		/// <summary>
		/// AREA 정보 불러오기
		/// </summary>
		/// <param name="cbo">콤보박스 Object</param>
		/// <param name="sShopId">SHOPID</param>
		/// <param name="sComboStatus">바인딩 후 ALL/Select 추가여부</param> 
		public void COR_SEL_AREA_BY_SHOP_CBO_G(ComboBox cbo, string sShopId, ComboStatus sComboStatus, string sSelValue = "")
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_AREA_BY_SHOP_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("SHOPID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["SHOPID"] = sShopId;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_AREA_BY_SHOP_CBO_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				try
				{
					if (Exception != null)
					{
						ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(DtResult, DtResult.Columns[0].ColumnName, DtResult.Columns[1].ColumnName, sComboStatus);
                    
                    if (sSelValue != "" && (from DataRow DRow in DtResult.Rows where DRow["AREAID"].Equals(sSelValue) select DRow).Count() > 0)
                    {
                        // cboProd.SelectedValue = sSelValue;
                        for (int i = 0; i < cbo.Items.Count; i++)
                        {
                            if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
                            {
                                cbo.SelectedIndex = i;
                                return;
                            }
                        }
                    }
                    else if (DtResult.Rows.Count > 0) //2016.02.15 LJH 변경하지 마시오!!!
                        cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_AREA_BY_SHOP_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_AREA_BY_SHOP_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}

		#endregion

		#region [※ AREA 별 공정 정보 불러오기]
		/// <summary>
		/// AREA별 공정 정보 불러오기
		/// </summary>
		/// <param name="cbo">콤보박스 Object</param>
		/// <param name="sAreaId">AREAID</param>
		/// <param name="sComboStatus">바인딩 후 ALL/Select 추가여부</param> 
		public void COR_SEL_PROCESS_BY_AREA_CBO_G(ComboBox cbo, string sAreaId, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PROCESS_BY_AREA_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("AREAID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["AREAID"] = sAreaId;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_PROCESS_BY_AREA_CBO_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				try
				{
					if (Exception != null)
					{
						ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

                    cbo.ItemsSource = SetCombo(DtResult, DtResult.Columns[0].ColumnName, DtResult.Columns[1].ColumnName, sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PROCESS_BY_AREA_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PROCESS_BY_AREA_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}

		#endregion

		#region [※ LOTID 별 공정 정보 불러오기]
		/// <summary>
		/// LOTID 공정 정보 불러오기 added by hataemin
		/// </summary>
		/// <param name="cbo">콤보박스 Object</param>
		/// <param name="sAreaId">AREAID</param>
		/// <param name="sComboStatus">바인딩 후 ALL/Select 추가여부</param> 
		public void COR_SEL_PROCESS_BY_LOTID_CBO_G(ComboBox cbo, string sLotID, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PROCESS_BY_LOTID_CBO", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("LOTID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["LOTID"] = sLotID;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_PROCESS_BY_LOTID_CBO", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				try
				{
					if (Exception != null)
					{
						ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(DtResult, DtResult.Columns[0].ColumnName, DtResult.Columns[1].ColumnName, sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PROCESS_BY_LOTID_CBO", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PROCESS_BY_LOTID_CBO", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}

		#endregion

		#region [※ 제품 TYPE 정보 불러오기]
		/// <summary>
		/// 제품 TYPE 정보 불러오기
		/// </summary>
		/// <param name="cbo">콤보박스 Object</param>
		/// <param name="sPdgrId">PDGRID</param>
		/// <param name="sComboStatus">바인딩 후 ALL/Select 추가여부</param> 
		public void COR_SEL_PRODUCTMODEL_BY_PDGRID_CBO_G(ComboBox cbo, string sPdgrId, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PRODUCTMODEL_BY_PDGRID_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("PDGRID", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["PDGRID"] = sPdgrId;

			IndataTable.Rows.Add(Indata);
			new ClientProxy().ExecuteService("COR_SEL_PRODUCTMODEL_BY_PDGRID_CBO_G", "INDATA", "OUTDATA", IndataTable, (DtResult, Exception) =>
			{
				try
				{
					if (Exception != null)
					{
						ControlsLibrary.MessageBox.Show(Exception.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(DtResult, DtResult.Columns[0].ColumnName, DtResult.Columns[1].ColumnName, sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PRODUCTMODEL_BY_PDGRID_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PRODUCTMODEL_BY_PDGRID_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}

		#endregion

        #region [※ 자재ID 정보 불러오기]
        /// <summary>
        /// 자재ID 정보 불러오기
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="sPdgrId"></param>
        /// <param name="sComboStatus"></param>
        public void SetCOR_SEL_ORG_MTRLID_CBO_G(ComboBox cbo, string sShopID, string sOrgCd, string sMultiOrgCd,string sMtrlType, string sMtrlID, string sClass1, string sClass2, string sClass3, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ORG_MTRLID_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["ORG_CODE"] = sOrgCd;
            searchCondition["MULTI_ORG_CODE"] = sMultiOrgCd;
            searchCondition["MTRLTYPE"] = sMtrlType;
            searchCondition["MTRLID"] = sMtrlID;
            searchCondition["LOCAL_MTRL_CLASS_CODE1"] = sClass1;
            searchCondition["LOCAL_MTRL_CLASS_CODE2"] = sClass2;
            searchCondition["LOCAL_MTRL_CLASS_CODE3"] = sClass3;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ORG_MTRLID_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            ); 
        }

        #endregion

        #region [※ MLOT 입고를 위한 ORG MTRLID 불러오기]
        public void SetBR_COR_SEL_ORG_MTRLID_FOR_STORE_MLOT_G(ComboBox cbo, string sShopID, string sOrgCd, string sMultiOrgCd, string sMtrlType, string sMtrlID, string sMlotTpCd, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetBR_COR_SEL_ORG_MTRLID_FOR_STORE_MLOT_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["ORG_CODE"] = sOrgCd;
            searchCondition["MULTI_ORG_CODE"] = sMultiOrgCd;
            searchCondition["MTRLTYPE"] = sMtrlType;
            searchCondition["MTRLID"] = sMtrlID;
            searchCondition["MLOT_TP_CODE"] = sMlotTpCd;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("BR_COR_SEL_ORG_MTRLID_FOR_STORE_MLOT_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        #endregion

        #region [※ 자재LOT 유형 정보 불러오기]
        /// <summary>
        /// 자재LOT 유형 정보 불러오기
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="sPdgrId"></param>
        /// <param name="sComboStatus"></param>
		public void SetCOR_SEL_MLOT_TP_CODE_CBO_G(ComboBox cbo, string sShopID, string sOrgCode, string sMultOrgCode, string sMlotTpCode, string sSelStoreFlag, string sMlotRcvGrCode, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(cbo, "Loaded");
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotType_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_MLOT_TP_CODE_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = sShopID;
			searchCondition["ORG_CODE"] = sOrgCode;
			searchCondition["MULTI_ORG_CODE"] = sMultOrgCode;
			searchCondition["MLOT_TP_CODE"] = sMlotTpCode;
			searchCondition["SEL_STORE_FLAG"] = sSelStoreFlag;
            searchCondition["MLOT_RCV_GR_CODE"] = sMlotRcvGrCode;

			searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_MLOT_TP_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotType_Loaded", ex);
				}
				finally
				{
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		//멀티선택용...
		public void SetCOR_SEL_MLOT_TP_CODE_CBO_G(ControlsLibrary.MultiSelectionBox cbo , string sShopID, string sOrgCode, string sMultOrgCode, string sMlotTpCode, string sSelStoreFlag, string sMlotRcvGrCode, ControlsLibrary.LoadingIndicator loadingIndicator)
		{
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotType_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_MLOT_TP_CODE_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = sShopID;
			searchCondition["ORG_CODE"] = sOrgCode;
			searchCondition["MULTI_ORG_CODE"] = sMultOrgCode;
			searchCondition["MLOT_TP_CODE"] = sMlotTpCode;
			searchCondition["SEL_STORE_FLAG"] = sSelStoreFlag;
			searchCondition["MLOT_RCV_GR_CODE"] = sMlotRcvGrCode;
			searchCondition["SUB_FLAG"] = "N";

			searchConditionTable.Rows.Add(searchCondition);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("COR_SEL_MLOT_TP_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					if (searchResult.Rows.Count > 0)
					{
						cbo.CheckAll();
					}

				}
				catch (Exception ex)
				{
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotType_Loaded", ex);
				}
				finally
				{
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMtrlId_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
        #endregion

        #region [※ MLOT 입고분류코드 정보 불러오기]
        /// <summary>
        /// MLOT 입고분류코드 정보 불러오기
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="sMlotRcvGrCode"></param>
        /// <param name="sSelStreFlag">MLOT 입고 분류 코드</param>
        /// <param name="sComboStatus">선택 입고 여부</param>
        public void SetCOR_SEL_MLOT_RCV_GR_CODE_CBO_G(ComboBox cbo, string sMlotRcvGrCode, string sSelStreFlag, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotRcvGr_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_MLOT_RCV_GR_CODE_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["MLOT_RCV_GR_CODE"] = sMlotRcvGrCode;
            searchCondition["SEL_STORE_FLAG"] = sSelStreFlag;
            
            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_MLOT_RCV_GR_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotRcvGr_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboMlotRcvGr_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }

        #endregion

        #region [※ 제품ID 정보 불러오기]
        /// <summary>
        /// 제품ID 조회
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="sShopID"></param>
        /// <param name="sProdID"></param>
        /// <param name="sModlID"></param>
        /// <param name="sProdType"></param>
        /// <param name="sComboStatus"></param>
		public void SetCOR_SEL_PRODUCTLIST_G(ComboBox cbo, string sShopID, string sOrgCD, string sMultiOrgCD, string sProdID, string sModlID, string sProdType, string sMultisProdType, ComboStatus sComboStatus, ControlsLibrary.LoadingIndicator loadingIndicator)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProd_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCTLIST_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = sShopID == string.Empty ? null : sShopID;
            searchCondition["ORG_CODE"] = sOrgCD == string.Empty ? null : sOrgCD;
			searchCondition["MULTI_ORG_CODE"] = sMultiOrgCD == string.Empty ? null : sMultiOrgCD;
            searchCondition["PRODID"] = sProdID == string.Empty ? null : sProdID;
            searchCondition["MODLID"] = sModlID == string.Empty ? null : sModlID;
            searchCondition["PRODTYPE"] = sProdType == string.Empty ? null : sProdType;
            searchCondition["MULTI_PRODTYPE"] = sMultisProdType == string.Empty ? null : sMultisProdType;

            searchConditionTable.Rows.Add(searchCondition);

			loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("COR_SEL_PRODUCTLIST_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
						loadingIndicator.Visibility = Visibility.Collapsed;
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;

                }
                catch (Exception ex)
                {
					loadingIndicator.Visibility = Visibility.Collapsed;
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProd_Loaded", ex);
                }
				finally
				{
					loadingIndicator.Visibility = Visibility.Collapsed;
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProd_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );  
        }
        #endregion

        #region [※ 고객사 정보 불러오기]
        public void SetCOR_SEL_SHOP_CUSTOMER_CBO_G(ComboBox cbo, string sShopID, string sCustomerID, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCustomer_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_SHOP_CUSTOMER_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["CUSTOMERID"] = sCustomerID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_SHOP_CUSTOMER_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCustomer_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCustomer_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );

        }
        #endregion

		#region [※ 고객사 정보 불러오기 TS]
		public void SetCOR_CUS_SEL_CUSTOMER_4_EXP_LOT(ComboBox cbo, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCustomer_Loaded", Logger.MESSAGE_OPERATION_START);
			
			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));

			DataRow searchCondition = IndataTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			IndataTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUS_SEL_CUSTOMER_4_EXP_LOT", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCustomer_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCustomer_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);

		}
		#endregion

		#region [※ 제품별 고객사 정보 불러오기(TS)]
		public void SetCOR_SEL_TS_VENDOR_CBO_G(ComboBox cbo, string orgCode, string prodClass1, string prodClass2, string prodClass3, string prodID, ComboStatus sComboStatus, string type = "", string sSelValue = "")
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_VENDOR_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_TS_VENDOR_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ORG_CODE"] = orgCode;
			searchCondition["PROD_CLASS1_CODE"] = prodClass1 == string.Empty ? null : prodClass1;
			searchCondition["PROD_CLASS2_CODE"] = prodClass2 == string.Empty ? null : prodClass2;
			searchCondition["PROD_CLASS3_CODE"] = prodClass3 == string.Empty ? null : prodClass3;
			searchCondition["PRODID"] = prodID == string.Empty ? null : prodID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_TS_VENDOR_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);

					if (type == "SHIP_VENDOR" && searchResult.Rows.Count == 0)
					{
						COR_SEL_COMMONCODE_BY_ORG_G(cbo, orgCode, type, sComboStatus);
					}
					else
					{

						cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

						if (sSelValue != "" && (from DataRow DRow in searchResult.Rows where DRow[cbo.SelectedValuePath.ToString()].Equals(sSelValue) select DRow).Count() > 0)
						{
							// cboProd.SelectedValue = sSelValue;
							for (int i = 0; i < cbo.Items.Count; i++)
							{
								if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
								{
									cbo.SelectedIndex = i;
									return;
								}
							}
						}
						else if (searchResult.Rows.Count > 0)
							cbo.SelectedIndex = 0;

					}

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_VENDOR_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_VENDOR_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
			
		}
		#endregion

		#region [※ 제품별 고객사 정보 불러오기(TS:출하성적서 전송)]
		public void SetCOR_SEL_TS_VENDOR_COC_SEND_CBO(ComboBox cbo, string orgCode, string prodClass1, string prodClass2, string prodClass3, string prodID, ComboStatus sComboStatus, string sSelValue = "")
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_VENDOR_COC_SEND_CBO", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_TS_VENDOR_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ORG_CODE"] = orgCode;
			searchCondition["PROD_CLASS1_CODE"] = prodClass1 == string.Empty ? null : prodClass1;
			searchCondition["PROD_CLASS2_CODE"] = prodClass2 == string.Empty ? null : prodClass2;
			searchCondition["PROD_CLASS3_CODE"] = prodClass3 == string.Empty ? null : prodClass3;
			searchCondition["PRODID"] = prodID == string.Empty ? null : prodID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_TS_VENDOR_COC_SEND_CBO", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (sSelValue != "" && (from DataRow DRow in searchResult.Rows where DRow[cbo.SelectedValuePath.ToString()].Equals(sSelValue) select DRow).Count() > 0)
					{
						// cboProd.SelectedValue = sSelValue;
						for (int i = 0; i < cbo.Items.Count; i++)
						{
							if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
							{
								cbo.SelectedIndex = i;
								return;
							}
						}
					}
					else if (searchResult.Rows.Count > 0)
						cbo.SelectedIndex = 0;
					

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_VENDOR_COC_SEND_CBO", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_VENDOR_COC_SEND_CBO", Logger.MESSAGE_OPERATION_END);
				}
			}
			);

		}
		#endregion

		#region [※ 모델 정보 불러오기(TS)]
		public void SetCOR_SEL_TS_MODEL_CBO_G(ControlsLibrary.MultiSelectionBox cbo, string orgCode, string prodClass1, string prodClass2, string prodClass3, string vendorID)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_MODEL_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_TS_MODEL_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ORG_CODE"] = orgCode;
			searchCondition["PROD_CLASS1_CODE"] = prodClass1 == string.Empty ? null : prodClass1;
			searchCondition["PROD_CLASS2_CODE"] = prodClass2 == string.Empty ? null : prodClass2;
			searchCondition["PROD_CLASS3_CODE"] = prodClass3 == string.Empty ? null : prodClass3;
			searchCondition["CUSTOMERID"] = vendorID == string.Empty ? null : vendorID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_TS_MODEL_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					//if (sMlotStGrCode == "A" || sMlotStGrCode == "B")	// 가용재고(A), 재고(B)
					//{
					//	for (int i = 0; i < searchResult.Rows.Count; i++)
					//	{
					//		string selFlag = Util.NVC(searchResult.Rows[i]["SEL_FLAG"]);
					//		int seqNo = Util.StringToInt(searchResult.Rows[i]["SEQ_NO"].ToString());
					//		if (selFlag == "Y")
					//		{
					//			cbo.Check(seqNo);
					//		}
					//	}
					//}
					//else
					//{
					//	cbo.CheckAll();
					//}

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_MODEL_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_MODEL_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);

		}
		#endregion

		#region [※ 출하처1 불러오기(TS)]
		public void SetCOR_SEL_TS_SHIP_SITE1_CBO_G(ComboBox cbo, string venderID, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_SHIP_SITE1_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = new DataTable();
			searchConditionTable.Columns.Add("LANGID", typeof(string));
			searchConditionTable.Columns.Add("CUSTOMERID", typeof(string));

			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["CUSTOMERID"] = venderID == string.Empty ? null : venderID;
			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_TS_SHIP_SITE1_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);

					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_SHIP_SITE1_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_SHIP_SITE1_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);

		}
		#endregion

		#region [※ 출하처2 불러오기(TS)]
		public void SetCOR_SEL_TS_SHIP_SITE2_CBO_G(ComboBox cbo, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_SHIP_SITE2_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = new DataTable();
			searchConditionTable.Columns.Add("LANGID", typeof(string));

			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_TS_SHIP_SITE2_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);

					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_SHIP_SITE2_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_TS_SHIP_SITE2_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);

		}
		#endregion

		#region [※ 출하성적서 파일 정보 불러오기(TS)]
		public void SetCOR_SEL_SHIP_RPT_FRMT_CBO(ComboBox cbo, string areaID, string prodClass1, string prodClass2, string prodClass3, string prodID, string custID, ComboStatus sComboStatus, string sSelValue = "")
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_SHIP_RPT_FRMT_CBO", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_SHIP_RPT_FRMT_CBO();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["AREAID"] = areaID;
			searchCondition["PROD_CLASS1_CODE"] = prodClass1 == string.Empty ? null : prodClass1;
			searchCondition["PROD_CLASS2_CODE"] = prodClass2 == string.Empty ? null : prodClass2;
			searchCondition["PROD_CLASS3_CODE"] = prodClass3 == string.Empty ? null : prodClass3;
			searchCondition["PRODID"] = prodID == string.Empty ? null : prodID;
			searchCondition["CUSTOMERID"] = custID == string.Empty ? null : custID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_SHIP_RPT_FRMT_CBO", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
					// cbo.DisplayMemberPath.ToString() --> 파일명이므로....
					if (sSelValue != "" && (from DataRow DRow in searchResult.Rows where DRow[cbo.DisplayMemberPath.ToString()].Equals(sSelValue) select DRow).Count() > 0)
					{
						// ItemArray[3] --> 파일명이므로....
						for (int i = 0; i < cbo.Items.Count; i++)
						{
							if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[3].ToString())
							{
								cbo.SelectedIndex = i;
								return;
							}
						}
					}
					else if (searchResult.Rows.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_SHIP_RPT_FRMT_CBO", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_SHIP_RPT_FRMT_CBO", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
			
		}
		#endregion

        #region [※ 주문 상태 정보 불러오기]
        public void SetCOR_SEL_ORDER_ST_CODE_CBO_G(ComboBox cbo, string sOrderStCD, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ORDER_ST_CODE_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["ORDER_ST_CODE"] = sOrderStCD;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ORDER_ST_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ Org Code 정보 불러오기]
        public void SetCOR_SEL_SHOP_ORG_G(ComboBox cbo, string sOrgCD, ComboStatus sComboStatus, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {

			string[] orgcode = CustomConfig.Instance.CONFIG_COMMON_ORG;
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_SHOP_ORG_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = CustomConfig.Instance.CONFIG_COMMON_SHOP;
			searchCondition["ORG_CODE"] = sOrgCD;	//Multi Org중에 특정Org만 가져올 경우 사용
			searchCondition["MULTI_ORG_CODE"] = Util.GetMultOrgCode(orgcode);

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_SHOP_ORG_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            ); 
        }

        //Select Value
        public void SetCOR_SEL_SHOP_ORG_G(ComboBox cbo, string sOrgCD, ComboStatus sComboStatus, string sSelectedValue)
        {

            string[] orgcode = CustomConfig.Instance.CONFIG_COMMON_ORG;
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_SHOP_ORG_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = CustomConfig.Instance.CONFIG_COMMON_SHOP;
            searchCondition["ORG_CODE"] = sOrgCD;	//Multi Org중에 특정Org만 가져올 경우 사용
            searchCondition["MULTI_ORG_CODE"] = Util.GetMultOrgCode(orgcode);

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_SHOP_ORG_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
                    cbo.SelectedValue = sSelectedValue;

                    if (Util.NVC(cbo.SelectedValue) == string.Empty)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrgCd_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ 마감통제 상태 정보 불러오기]
        public void SetCOR_SEL_MNTH_CLOSE_ST_CODE_G(ComboBox cbo, string sCloseStCD, string sCloseStTpCD, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCloseStat_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_MNTH_CLOSE_ST_CODE_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["CLOSE_ST_CODE"] = sCloseStCD;
            searchCondition["CLOSE_ST_TP_CODE"] = sCloseStTpCD;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_MNTH_CLOSE_ST_CODE_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCloseStat_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboCloseStat_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ PRODUCT GROUP 정보 불러오기]
        /// <summary>
        /// PRODUCT GROUP 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_PRODUCTGROUP_CBO(ComboBox cbo, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdGR_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCTGROUP_CBO();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_PRODUCTGROUP_CBO", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdGR_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdGR_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
            
        }
        #endregion

        #region [※ 제품군 정보 불러오기]
        /// <summary>
        /// 제품군 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_PRODUCTGROUP_BY_ORG_CBO(ComboBox cbo, ComboStatus sComboStatus)
        {
            if (CustomConfig.Instance.ConfigSet.Tables.Contains(CustomConfig.CONFIGTABLE_COMMON)
                && CustomConfig.Instance.ConfigSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows.Count > 0)
            {
                Logger.Instance.WriteLine("Product Group Combobox", Logger.MESSAGE_OPERATION_START);

                DataTable pdgrIndataTable = new DataTable();
                pdgrIndataTable.Columns.Add("LANGID", typeof(string));
                pdgrIndataTable.Columns.Add("ORG_CODE", typeof(string));
                pdgrIndataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));

                DataRow pdgrIndata = pdgrIndataTable.NewRow();
                pdgrIndata["LANGID"] = LoginInfo.LANGID;
                pdgrIndata["ORG_CODE"] = null;
                pdgrIndata["MULTI_ORG_CODE"] = Util.getConfigMultiOrgCode();
                pdgrIndataTable.Rows.Add(pdgrIndata);

                new ClientProxy().ExecuteService("COR_SEL_PRODUCTGROUP_BY_ORG_CBO", "INDATA", "OUTDATA", pdgrIndataTable, (pdgrResult, pdgrException) =>
                {
                    try
                    {
                        if (pdgrException != null)
                        {
                            ControlsLibrary.MessageBox.Show(pdgrException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            return;
                        }

                        cbo.ItemsSource = SetCombo(pdgrResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                        if (pdgrResult.Rows.Count > 0)
                            cbo.SelectedIndex = 0;

                    }
                    catch (Exception ex)
                    {
                        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        Logger.Instance.WriteLine(Logger.OPERATION_R + "Product Group Combobox", ex);
                    }
                    finally
                    {
                        Logger.Instance.WriteLine(Logger.OPERATION_R + "Product Group Combobox", Logger.MESSAGE_OPERATION_END);
                    }
                }
                );
            }
        }
        #endregion

        #region [※ MODEL 정보 불러오기]
        /// <summary>
        /// MODEL 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_PRODUCTMODEL_CBO_G(ComboBox cbo, string sPdgrID, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdModel_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCTMODEL_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["PDGRID"] = sPdgrID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_PRODUCTMODEL_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdModel_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdModel_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ PRODUCT 정보 불러오기]
        /// <summary>
        /// PRODUCT 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_PRODUCT_CBO_G(ComboBox cbo, string sModelID, string sProdgrID, string sProdType, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProd_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCT_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["MODELID"] = sModelID;
            searchCondition["PDGRID"] = sProdgrID;
            searchCondition["PRODTYPE"] = sProdType;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_PRODUCT_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProd_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProd_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ PRODUCT BY ROUTE 정보 불러오기]
        /// <summary>
        /// PRODUCT 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_PRODUCT_ROUTE_CBO_G(ComboBox cbo, string sModelID, string sPdgrID, string sProdType, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdRoute_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCT_ROUTE_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["MODELID"] = sModelID;
            searchCondition["PDGRID"] = sPdgrID;
            searchCondition["PRODTYPE"] = sProdType;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_PRODUCT_ROUTE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdRoute_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProdRoute_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            ); 
        }
        #endregion

        #region [※ ACTIVITYREASONGROUP BY PROCID 정보 불러오기]
        /// <summary>
        /// ACTIVITYREASONGROUP 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_ACTIVITYREASONGROUP_BY_PROCID_CBO_G(ComboBox cbo, string sActID, string sProcID, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboReasonGroup_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ACTIVITYREASONGROUP_BY_PROCID_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["ACTID"] = sActID;
            searchCondition["PROCID"] = sProcID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ACTIVITYREASONGROUP_BY_PROCID_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboReasonGroup_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboReasonGroup_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ ACTIVITYREASON BY PROCID 정보 불러오기- 불량코드]
        /// <summary>
        /// ACTIVITYREASON 정보 불러오기 - 불량코드
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_ACTIVITYREASON_BY_PROCID_CBO_G(ComboBox cbo, string sActID, string sProcID, string sResnGrID, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboReasonCode_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ACTIVITYREASON_BY_PROCID_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["ACTID"] = sActID;
            searchCondition["PROCID"] = sProcID;
            searchCondition["RESNGRID"] = sResnGrID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ACTIVITYREASON_BY_PROCID_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboReasonCode_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboReasonCode_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );    
        }
        #endregion

        #region [※ PROCESS BY PRODID 정보 불러오기]
        /// <summary>
        /// PRODID 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_PROCESS_SHOP_CBO_G(ComboBox cbo, string sShopID, string sProdID, string sPcsgID, ComboStatus sComboStatus, string selectedValue = null)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProc_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_PROCESS_SHOP_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["PRODID"] = sProdID == string.Empty ? null : sProdID;
            searchCondition["PCSGID"] = sPcsgID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_PROCESS_SHOP_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                    {
                        if (selectedValue == null)
                        {
                            cbo.SelectedIndex = 0;
                        }
                        else
                        {
                            cbo.SelectedValue = selectedValue;
                        }
                    }

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProc_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboProc_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ From Date, To Date 설정 체크]
        public bool ChkDateValidation(string sDateFrom, string sDateTo)
        {
            bool result = true;

            DateTime dFrom = Convert.ToDateTime(sDateFrom);
            DateTime dTo = Convert.ToDateTime(sDateTo);

            if (dFrom > dTo)
            {
				ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1334"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                result = false;
            }
            return result;
        }
        #endregion

        #region [※ From Date, To Date 설정 일수차이 체크]
        public bool ChkDateGapValidation(string sDateFrom, string sDateTo, int iGapDay)
        {
            bool result = true;

            DateTime dFrom = Convert.ToDateTime(sDateFrom).AddDays(iGapDay);
            DateTime dTo = Convert.ToDateTime(sDateTo);

            if (dFrom <= dTo)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1410", iGapDay), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                result = false;
            }

            return result;
        }
        #endregion

        #region [※ 공급사 정보 불러오기]
        /// <summary>
        /// 공급사 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetGetCOR_SEL_SHOP_SUPPLIER_CBO_G(ComboBox cbo, string sShopID, string sSupplierID, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboSupplierId_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_SHOP_SUPPLIER_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["SUPPLIERID"] = sSupplierID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_SHOP_SUPPLIER_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboSupplierId_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboSupplierId_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ 제조사 정보 불러오기]
        /// <summary>
        /// 제조사 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetGetCOR_SEL_SHOP_MAKER_G(ComboBox cbo, string sShopID, string sOrgCd, string sMultiOrgCd, string sMakerCd, string sSupplierId, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboSupplierId_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_SHOP_MAKER_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["ORG_CODE"] = sOrgCd;
            searchCondition["MULTI_ORG_CODE"] = sMultiOrgCd;
            searchCondition["MAKER_CODE"] = sMakerCd;
            searchCondition["SUPPLIERID"] = sSupplierId;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_SHOP_MAKER_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboSupplierId_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboSupplierId_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ 납기일 변경 사유 코드 ComboBox 조회]
        /// <summary>
        /// 납기일 변경 사유 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCOR_SEL_DUE_DATE_CHG_RSN_CODE(ComboBox cbo, string sShopID, string sOrgCode, string sMultOrgCode, string sDueDateChgRsnCd, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboDueDateRsnCd_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_DUE_DATE_CHG_RSN_CODE();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["ORG_CODE"] = sOrgCode;
            searchCondition["MULTI_ORG_CODE"] = sMultOrgCode;
            searchCondition["DUE_DATE_CHG_RSN_CODE"] = sDueDateChgRsnCd;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_DUE_DATE_CHG_RSN_CODE", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboSupplierId_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboSupplierId_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ W/O상태 ComboBox 조회]
        public void SetCOR_SEL_WOSTAT_CBO_G(ComboBox cbo, string sWoStat, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoStat_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_WOSTAT_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["WOSTAT"] = sWoStat;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_WOSTAT_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoStat_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoStat_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ W/O 분류ComboBox 조회]
        public void SetCOR_SEL_WOTYPE_CBO_G(ComboBox cbo, string sWoType, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoSrcType_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_WOTYPE_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["WOTYPE"] = sWoType;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_WOTYPE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoSrcType_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoSrcType_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ 작업장 ComboBox 조회]
        public void SetCOR_SEL_ERP_DEPT_CBO_G(ComboBox cbo, string sOrgCd, string sMultiOrgCd, string sErpDeptCd, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoSrcType_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ERP_DEPT_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["ORG_CODE"] = sOrgCd;
            searchCondition["MULTI_ORG_CODE"] = sMultiOrgCd;
            searchCondition["ERP_DEPT_CODE"] = sErpDeptCd;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ERP_DEPT_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoSrcType_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboWoSrcType_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ Workorder 수동 생성 사유 조회]
        public void SetCOR_SEL_MANUAL_WO_RSN_CODE_CBO_G(ComboBox cbo, string sManualWoRsnCd, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboRsnCode_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_MANUAL_WO_RSN_CODE_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["MANUAL_WO_RSN_CODE"] = sManualWoRsnCd;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_MANUAL_WO_RSN_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboRsnCode_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboRsnCode_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            ); 
        }
        #endregion

		#region [※ 제품별 CLASS 조회]
		public void SetCOR_SEL_PRODUCT_CLASS_CBO_G(ComboBox cbo, string sOrgCd, string cmcdType, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_CLASS_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCT_CLASS_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ORG_CODE"] = sOrgCd;
			searchCondition["CMCDTYPE"] = cmcdType;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_PRODUCT_CLASS_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_CLASS_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_CLASS_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ 제품 분류에 따른 제품 조회]
		public void SetCOR_SEL_PRODUCT_BY_CLASS_G(ComboBox cbo, string shopID, string orgCd, string prodClass1, string prodClass2,
					string prodClass3, string prodClass4, ComboStatus sComboStatus, ControlsLibrary.LoadingIndicator loadingIndicator)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCT_BY_CLASS_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = shopID == string.Empty ? null : shopID;
			searchCondition["ORG_CODE"] = orgCd == string.Empty ? null : orgCd;
			searchCondition["PROD_CLASS1_CODE"] = prodClass1 == string.Empty ? null : prodClass1;
			searchCondition["PROD_CLASS2_CODE"] = prodClass2 == string.Empty ? null : prodClass2;
			searchCondition["PROD_CLASS3_CODE"] = prodClass3 == string.Empty ? null : prodClass3;
			searchCondition["PROD_CLASS4_CODE"] = prodClass4 == string.Empty ? null : prodClass4;

			searchConditionTable.Rows.Add(searchCondition);
			loadingIndicator.Visibility = System.Windows.Visibility.Visible;
			new ClientProxy().ExecuteService("COR_SEL_PRODUCT_BY_CLASS_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", Logger.MESSAGE_OPERATION_END);
					loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
				}
			}
			);
		}

        
        public void SetCOR_SEL_PRODUCT_BY_CLASS_G_MULTI(LGC.GMES.MES.ControlsLibrary.MultiSelectionBox cbo, string shopID, string orgCd, string prodClass1, string prodClass2,
                string prodClass3, string prodClass4, ComboStatus sComboStatus, ControlsLibrary.LoadingIndicator loadingIndicator)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCT_BY_CLASS_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = shopID == string.Empty ? null : shopID;
            searchCondition["ORG_CODE"] = orgCd == string.Empty ? null : orgCd;
            searchCondition["PROD_CLASS1_CODE"] = prodClass1 == string.Empty ? null : prodClass1;
            searchCondition["PROD_CLASS2_CODE"] = prodClass2 == string.Empty ? null : prodClass2;
            searchCondition["PROD_CLASS3_CODE"] = prodClass3 == string.Empty ? null : prodClass3;
            searchCondition["PROD_CLASS4_CODE"] = prodClass4 == string.Empty ? null : prodClass4;

            searchConditionTable.Rows.Add(searchCondition);
            loadingIndicator.Visibility = System.Windows.Visibility.Visible;
            new ClientProxy().ExecuteService("COR_SEL_PRODUCT_BY_CLASS_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    cbo.ItemsSource = DataTableConverter.Convert(searchResult); // SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (sComboStatus == ComboStatus.ALL)
                    {
                        cbo.CheckAll();
                    }
                    //if (cbo.Items.Count > 0)
                    //  cbo.SelectedIndex = 0;

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", Logger.MESSAGE_OPERATION_END);
                    loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            );
        }

		public void SetCOR_SEL_PRODUCT_BY_CLASS_G(C1ComboBox cbo, string shopID, string orgCd, string prodClass1, string prodClass2,
            string prodClass3, string prodClass4, ComboStatus sComboStatus, ControlsLibrary.LoadingIndicator loadingIndicator)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCT_BY_CLASS_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = shopID == string.Empty ? null : shopID;
			searchCondition["ORG_CODE"] = orgCd == string.Empty ? null : orgCd;
			searchCondition["PROD_CLASS1_CODE"] = prodClass1 == string.Empty ? null : prodClass1;
			searchCondition["PROD_CLASS2_CODE"] = prodClass2 == string.Empty ? null : prodClass2;
			searchCondition["PROD_CLASS3_CODE"] = prodClass3 == string.Empty ? null : prodClass3;
			searchCondition["PROD_CLASS4_CODE"] = prodClass4 == string.Empty ? null : prodClass4;

			searchConditionTable.Rows.Add(searchCondition);
			loadingIndicator.Visibility = System.Windows.Visibility.Visible;
			new ClientProxy().ExecuteService("COR_SEL_PRODUCT_BY_CLASS_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PRODUCT_BY_CLASS_G", Logger.MESSAGE_OPERATION_END);
					loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
				}
			}
			);
		}
		#endregion

	    #region [※ PROCESSEQUIPMENT 정보 불러오기]
        /// <summary>
		/// PROCESSEQUIPMENT 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
		public void SetCOR_SEL_PROCESSEQUIPMENT_G(ComboBox cbo, string procID, string eqptID, ComboStatus sComboStatus)
        {
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PROCESSEQUIPMENT_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_PROCESSEQUIPMENT_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["PROCID"] = procID;
			searchCondition["EQPTID"] = eqptID;

            searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_PROCESSEQUIPMENT_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PROCESSEQUIPMENT_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_PROCESSEQUIPMENT_G", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

		#region [※ LAYER_NAME 정보 불러오기]
		/// <summary>
		/// LAYER_NAME 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
		public void SetCOR_SEL_LAYER_NAME_G(ComboBox cbo, string shopID, string orgCode, string multiOrgCode, ComboStatus sComboStatus)
        {
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_LAYER_NAME_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_LAYER_NAME_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = shopID;
			searchCondition["ORG_CODE"] = orgCode;
			searchCondition["MULTI_ORG_CODE"] = multiOrgCode;

            searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_LAYER_NAME_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_LAYER_NAME_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_LAYER_NAME_G", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ 00~23시 가져오기]
        public void SetTimeCombo(ComboBox cbo)
        {
            try
            {
                DataSet dsTime = new DataSet();
                DataTable dtTime = dsTime.Tables.Add("Time");//new DataTable();
                dtTime.Columns.Add("CBO_NAME", typeof(string));
                dtTime.Columns.Add("CBO_CODE", typeof(string));

                for (int iTime = 23; iTime >= 0; iTime--)
                {
                    string sTime = iTime.ToString().PadLeft(2, '0');

                    DataRow dr = dtTime.NewRow();
                    dr["CBO_NAME"] = sTime;
                    dr["CBO_CODE"] = sTime;
                    dtTime.Rows.InsertAt(dr, 0);
                }
                
                cbo.ItemsSource = DataTableConverter.Convert(dtTime);

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
				ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }
        #endregion

		#region [※ Stocker 정보 불러오기]
		/// <summary>
		/// Stocker 정보 불러오기
		/// </summary>
		/// <param name="cboProd"></param>
		/// <param name="itemString"></param>
		public void SetCUR_CBO_STRK_ID(ComboBox cbo, string tpCode, ComboStatus sComboStatus, string sSelValue = "")
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUR_CBO_STRK_ID", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCUR_CBO_STRK_ID();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["STKR_TYPE"] = tpCode;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUR_CBO_STRK_ID", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (sSelValue != "" && (from DataRow DRow in searchResult.Rows where DRow["CBO_CODE"].Equals(sSelValue) select DRow).Count() > 0)
					{
						// cboProd.SelectedValue = sSelValue;
						for (int i = 0; i < cbo.Items.Count; i++)
						{
							if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
							{
								cbo.SelectedIndex = i;
								return;
							}
						}
					}
					else if (searchResult.Rows.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUR_CBO_STRK_ID", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUR_CBO_STRK_ID", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ Slot 정보 불러오기]
		/// <summary>
		/// Slot 정보 불러오기
		/// </summary>
		/// <param name="cboProd"></param>
		/// <param name="itemString"></param>
		public void SetCUS_CBO_SLT_NO(ComboBox cbo, string tpCode, string stkrID, ComboStatus sComboStatus, string sSelValue = "")
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_CBO_SLT_NO", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCUS_CBO_SLT_NO();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["STKR_TYPE"] = tpCode;
			searchCondition["STKR_ID"] = stkrID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUS_CBO_SLT_NO", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (sSelValue != "" && (from DataRow DRow in searchResult.Rows where DRow["CBO_CODE"].Equals(sSelValue) select DRow).Count() > 0)
					{
						// cboProd.SelectedValue = sSelValue;
						for (int i = 0; i < cbo.Items.Count; i++)
						{
							if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
							{
								cbo.SelectedIndex = i;
								return;
							}
						}
					}
					else if (searchResult.Rows.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_CBO_SLT_NO", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_CBO_SLT_NO", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ RouteFlow에 해당하는 공정 정보 불러오기]
		/// <summary>
		/// RouteFlow에 해당하는 공정 정보 불러오기
		/// </summary>
		/// <param name="cboProd"></param>
		/// <param name="itemString"></param>
		public void SetCOR_SEL_ROUTEFLOW_PATH_OPT(ComboBox cbo, string routID, string flowID, string procID, string folwID_To, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_ROUTEFLOW_PATH_OPT", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_ROUTEFLOW_PATH_OPT();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["FLOWNORM"] = "Y";
			searchCondition["ROUTID"] = routID;
			searchCondition["FLOWID"] = flowID;
			searchCondition["PROCID"] = procID;
			searchCondition["FLOWID_TO"] = folwID_To;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_ROUTEFLOW_PATH_OPT", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_ROUTEFLOW_PATH_OPT", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_ROUTEFLOW_PATH_OPT", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion
		
	    #region [※ RouteStep에 해당하는 공정 정보 불러오기]
		/// <summary>
		/// RouteStep에 해당하는 공정 정보 불러오기
		/// </summary>
		/// <param name="cboProd"></param>
		/// <param name="itemString"></param>
		public void SetCOR_SEL_ROUTESTEP_CBO(ComboBox cbo, string routID, string flowID, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_ROUTESTEP_CBO", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_ROUTESTEP_CBO();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ROUTID"] = routID;
			searchCondition["FLOWID"] = flowID;


			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_ROUTESTEP_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_ROUTESTEP_CBO", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_ROUTESTEP_CBO", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ RoutePath에 해당하는 Next Flow/공정 정보 불러오기]
		/// <summary>
		/// RoutePath에 해당하는 Next Flow/공정 정보 불러오기
		/// </summary>
		/// <param name="cboProd"></param>
		/// <param name="itemString"></param>
		public void SetCOR_SEL_NEXT_PROC_ROUTEFLOW_G(ComboBox cbo, string routID, string flowID, string procID, string nextFlowType, string nextFolwID, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_NEXT_PROC_ROUTEFLOW_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_NEXT_PROC_ROUTEFLOW_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ROUTID"] = routID;
			searchCondition["FLOWID"] = flowID;
			searchCondition["PROCID"] = procID;
			searchCondition["NEXT_FLOWTYPE"] = nextFlowType;
			searchCondition["NEXT_FLOWID"] = nextFolwID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_NEXT_PROC_ROUTEFLOW_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_NEXT_PROC_ROUTEFLOW_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_NEXT_PROC_ROUTEFLOW_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

        #region [※ LOTID에 해당하는 공정 정보 불러오기]
        /// <summary>
        /// RouteStep에 해당하는 공정 정보 불러오기
        /// </summary>
        /// <param name="cboProd"></param>
        /// <param name="itemString"></param>
        public void SetCUS_SEL_PROD_LOT_INPUT_PROCID_CBO_QP1(ComboBox cbo, string sOrgCode, string LotID, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_PROD_LOT_INPUT_PROCID_CBO_QP1", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCUS_SEL_PROD_LOT_INPUT_PROCID_CBO_QP1();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["ORG_CODE"] = sOrgCode;
            searchCondition["LOTID"] = LotID;


            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("CUS_SEL_PROD_LOT_INPUT_PROCID_CBO_QP1", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_PROD_LOT_INPUT_PROCID_CBO_QP1", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_PROD_LOT_INPUT_PROCID_CBO_QP1", Logger.MESSAGE_OPERATION_END);
				}
            }
            );  
        }
        #endregion

        #region [※ LABEL TYPE 정보 불러오기]
		/// <summary>
		/// LABEL TYPE 정보 불러오기
		/// </summary>
		/// <param name="cbo"></param>
		/// <param name="shopID"></param>
		/// <param name="sComboStatus"></param>
        public void SetCOR_SEL_SFU_LABEL_TYPE_G(ComboBox cbo, string shopID, ComboStatus sComboStatus, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_SFU_LABEL_TYPE_G();
            DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["SHOPID"] = shopID;

            searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_SFU_LABEL_TYPE_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

		#region [※ LABEL GROUP 정보 표시하기]
		/// <summary>
		/// LABEL GROUP 정보 표시하기
		/// </summary>
		/// <param name="shopID"></param>
		/// <param name="lblType"></param>
		public void SetCOR_SEL_SFU_LABEL_TYPE_G(TextBox obj, string shopID, string lblType)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_SFU_LABEL_TYPE_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["SHOPID"] = shopID;
			searchCondition["LABEL_TYPE"] = lblType;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_SFU_LABEL_TYPE_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					if (searchResult.Rows.Count > 0)
					{
						obj.Text = Util.NVC(searchResult.Rows[0]["LABEL_GROUP"]);
					}

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

        #region [※ LABEL TYPE 정보 Setting값 설정 후 LABEL GROUP 정보표시하기]
        /// <summary>
        /// LABEL TYPE 정보 불러오기
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="shopID"></param>
        /// <param name="sComboStatus"></param>
        public void SetCOR_SEL_SFU_LABEL_TYPE_G(ComboBox cbo, TextBox txt, string sShopID, string sLabelType, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_SFU_LABEL_TYPE_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["SHOPID"] = sShopID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_SFU_LABEL_TYPE_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);
                    cbo.SelectedValue = sLabelType;

					if (Util.NVC(cbo.SelectedValue) == string.Empty)
					{
						cbo.SelectedIndex = 0;
						txt.Text = string.Empty;
					}
					else
					{
						SetCOR_SEL_SFU_LABEL_TYPE_G(txt, sShopID, Util.NVC(cbo.SelectedValue));
					}
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_SFU_LABEL_TYPE_G", Logger.MESSAGE_OPERATION_END);
				}
            }
            ); 
        }
        #endregion

		#region [※ SMS 전송 처리]
		/// <summary>
		/// SMS 전송 처리
		/// </summary>
		/// <param name="sMsg">Message Content</param>
		/// <param name="sFormUserId">사용자 ID</param>
		/// <param name="sFromNum">송신 Phone No</param>
		/// <param name="sToNum">수신 Phone No</param>
		public void Send_Sms(string sOrgCode, string sMsg, string sFormUserId, string sFromNum, string sToNum)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "Send_Sms", Logger.MESSAGE_OPERATION_START);

			DataTable inDataTable = new DataTable();
			inDataTable.Columns.Add("ORG_CODE", typeof(string));               //ORG
			inDataTable.Columns.Add("TRAN_MSG", typeof(string));               //Message Content
			inDataTable.Columns.Add("TRAN_PHONE", typeof(string));             //수신 Phone No.
			inDataTable.Columns.Add("TRAN_CALLBACK", typeof(string));          //송신 Phone No.

			inDataTable.Columns.Add("USERIUSE", typeof(string));
			inDataTable.Columns.Add("EAI_DATA_CRUD_CD", typeof(string));
			inDataTable.Columns.Add("EAI_CREATED_BY", typeof(string));
			inDataTable.Columns.Add("EAI_TRANSFER_FLAG", typeof(string));         

			DataRow inDataRow = inDataTable.NewRow();
			inDataRow["ORG_CODE"] = sOrgCode;
			inDataRow["TRAN_MSG"] = sMsg;
			inDataRow["TRAN_PHONE"] = sToNum;
			inDataRow["TRAN_CALLBACK"] = sFromNum;
			inDataRow["USERIUSE"] = "Y";
			inDataRow["EAI_DATA_CRUD_CD"] = "C";
			inDataRow["EAI_CREATED_BY"] = sFormUserId;
			inDataRow["EAI_TRANSFER_FLAG"] = "N";

			inDataTable.Rows.Add(inDataRow);

			new ClientProxy().ExecuteService("BR_CUS_REG_UNFIT_SMS_G", "INDATA", null, inDataTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "Send_Sms", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "Send_Sms", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ MAIL 전송 처리]
		/// <summary>
		/// 메일 전송 처리
		/// </summary>
		/// <param name="Hashtable">메일정보</param>
		public void Send_Mail(string sFROMUSERMAIL, string sTOUSERMAIL, string sMAILSUBJECT, string sMAILBODY)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_MAIL_SEND_G", Logger.MESSAGE_OPERATION_START);

			DataTable inDataTable = new DataTable();
			inDataTable.Columns.Add("PI_MAIL_TO", typeof(string));                //수신 Phone No.
			inDataTable.Columns.Add("PI_MAIL_FROM", typeof(string));              //송신 Phone No.
			inDataTable.Columns.Add("PI_SUBJECT", typeof(string));                //Message Content
			inDataTable.Columns.Add("PI_CONTENT", typeof(string));                //사용자 ID  
			inDataTable.Columns.Add("PI_HTML_YN", typeof(string));                //전송 상태
			inDataTable.Columns.Add("PI_SMTP_SERVER", typeof(string));            //전송 상태

			DataRow inDataRow = inDataTable.NewRow();
			inDataRow["PI_MAIL_TO"] = sTOUSERMAIL;
			inDataRow["PI_MAIL_FROM"] = sFROMUSERMAIL;
			inDataRow["PI_SUBJECT"] = sMAILSUBJECT;
			inDataRow["PI_CONTENT"] = sMAILBODY;
			inDataRow["PI_HTML_YN"] = "Y";
			inDataRow["PI_SMTP_SERVER"] = "165.243.97.17";  // "ptms0.lginnotek.com";


			inDataTable.Rows.Add(inDataRow);

			new ClientProxy().ExecuteService("COR_SEL_MAIL_SEND_G", "INDATA", null, inDataTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_MAIL_SEND_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_MAIL_SEND_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

        #region [※ 약품LOT 유형 정보 불러오기]
        /// <summary>
        /// 약품LOT 유형 정보 불러오기
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="sPdgrId"></param>
        /// <param name="sComboStatus"></param>
        public void SetCUS_SEL_CHEM_LOT_TP_CODE_CBO_PM1(ComboBox cbo, string sShopID, string sOrgCd,string sMultOrgCd, string sChemLotTpCd, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(cbo, "Loaded");
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboChemLotid_M_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCUS_SEL_CHEM_LOT_TP_CODE_CBO_PM1();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["ORG_CODE"] = sOrgCd;
            searchCondition["MULTI_ORG_CODE"] = sMultOrgCd;
            searchCondition["CHEM_LOT_TP_CODE"] = sChemLotTpCd;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("CUS_SEL_CHEM_LOT_TP_CODE_CBO_PM1", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					//cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboChemLotid_M_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboChemLotid_M_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }

        #endregion

        #region [※ 약품LOT 기초 정보 불러오기]
        /// <summary>
        /// 약품LOT 기초 정보 불러오기
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="sPdgrId"></param>
        /// <param name="sComboStatus"></param>
        public void SetBR_CUS_GET_CHEM_LOT_SERIAL_PM1(TextBox txtYY, TextBox txtMM, TextBox txtDD)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_CUS_GET_CHEM_LOT_SERIAL_PM1", Logger.MESSAGE_OPERATION_START);

            new ClientProxy().ExecuteService("BR_CUS_GET_CHEM_LOT_SERIAL_PM1", null, "OUTDATA", null, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					txtYY.Text = searchResult.Rows[0]["YEAR_VALUE"].ToString();
					txtMM.Text = searchResult.Rows[0]["MONTH_VALUE"].ToString();
					txtDD.Text = searchResult.Rows[0]["DAY_VALUE"].ToString();
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_CUS_GET_CHEM_LOT_SERIAL_PM1", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_CUS_GET_CHEM_LOT_SERIAL_PM1", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }

        #endregion

        #region [※ 자재ID 선택 시 품명(자재 명) 정보 불러오기]
        /// <summary>
        /// 자재ID 선택 시 품명(자재 명) 정보 불러오기
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="sPdgrId"></param>
        /// <param name="sComboStatus"></param>
        public void SetCOR_SEL_ORG_GD_NAME_G(TextBlock txb, string sShopID, string sOrgCd, string sMultOrgCd, string sMtrlType, string sMtrlId, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(txb, "Loaded");
            Logger.Instance.WriteLine(Logger.OPERATION_R + "txbMtrlName_M", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ORG_GD_NAME_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["ORG_CODE"] = sOrgCd;
            searchCondition["MULTI_ORG_CODE"] = sMultOrgCd;
            searchCondition["MTRLTYPE"] = sMtrlType;
            searchCondition["MTRLID"] = sMtrlId;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ORG_GD_NAME_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					txb.Text = searchResult.Rows[0]["GD_NAME"].ToString();
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "txbMtrlName_M_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "txbMtrlName_M_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );  
        }

        #endregion

        #region [※ 불량1분류 ComboBox 조회]
		public void SetCOR_SEL_ACT_RSN_GR_1_BY_PROCID_CBO_G(ComboBox cbo, string sActID, string sProcID, ComboStatus sComboStatus)
        {
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboActRsnGr1_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_ACT_RSN_GR_1_BY_PROCID_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ACTID"] = sActID;
			searchCondition["PROCID"] = sProcID;

            searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_ACT_RSN_GR_1_BY_PROCID_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboActRsnGr1_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboActRsnGr1_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

		#region [※ 불량2분류 ComboBox 조회]
		public void SetCOR_SEL_ACT_RSN_GR_2_BY_PROCID_CBO_G(ComboBox cbo, string sActID, string sProcID, string RsnGrID, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboActRsnGr2_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_ACT_RSN_GR_2_BY_PROCID_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ACTID"] = sActID;
			searchCondition["PROCID"] = sProcID;
			searchCondition["RESNGRID_PV"] = RsnGrID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_ACT_RSN_GR_2_BY_PROCID_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboActRsnGr2_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboActRsnGr2_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ 비가동 사유코드 ComboBox 조회]
		public void SetCOR_SEL_NOWORKCODE_G(ComboBox cbo, string shopID, string noWorkID, string noWorkID_Pv, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboNoWorkCode_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCOR_SEL_NOWORKCODE_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = shopID;
			searchCondition["NOWORKID"] = noWorkID;
			searchCondition["NOWORKID_PV"] = noWorkID_Pv;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_NOWORKCODE_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboNoWorkCode_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboNoWorkCode_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);			
		}
		#endregion
		
		#region [※ 폐기처리분류(PM/PMP) ComboBox 조회]
		public void SetCUS_SEL_SCRAP_PRCS_GR_CODE_CBO_QP1PM1(ComboBox cbo, string shopID, string orgCode, string multiOrgCode, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboScrapType_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCUS_SEL_SCRAP_PRCS_GR_CODE_CBO_QP1PM1();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = shopID;
			searchCondition["ORG_CODE"] = orgCode;
			searchCondition["MULTI_ORG_CODE"] = multiOrgCode;
			searchCondition["SCRAP_PRCS_GR_CODE"] = null;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUS_SEL_SCRAP_PRCS_GR_CODE_CBO_QP1PM1", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboScrapType_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboScrapType_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ Recipe ComboBox 조회]
		public void SetCUS_SEL_SFU_RECIPE_CBO_G(ComboBox cbo, string eqptID, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "cboRecipe_Loaded", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCUS_SEL_SFU_RECIPE_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["EQPTID"] = eqptID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUS_SEL_SFU_RECIPE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboRecipe_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboRecipe_Loaded", Logger.MESSAGE_OPERATION_END);
				}
			}
			);	
		}
		#endregion

        #region [※ 실적유형 ComboBox 조회]
        public void SetCOR_SEL_ERP_TXN_TP_CODE_CBO_G(ComboBox cbo, string sErpTxnTpCd, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboTxnType_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ERP_TXN_TP_CODE_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["ERP_TXN_ERR_TP_CODE"] = sErpTxnTpCd;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ERP_TXN_TP_CODE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboTxnType_Loaded", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "cboTxnType_Loaded", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion
		
        #region [※ D/S자재 ComboBox 조회]
		public void SetCUS_SEL_DS_MTRLID_CBO_QP1(ComboBox cbo, string orgCode, string prodID, ComboStatus sComboStatus)
        {
			Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_DS_MTRLID_CBO_QP1", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetCUS_SEL_DS_MTRLID_CBO_QP1();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ORG_CODE"] = orgCode;
			searchCondition["PRODID"] = prodID;

            searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUS_SEL_DS_MTRLID_CBO_QP1", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_DS_MTRLID_CBO_QP1", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_DS_MTRLID_CBO_QP1", Logger.MESSAGE_OPERATION_END);
				}
            }
            );
        }
        #endregion

        #region [※ FCCL 설비 Combo조회]
        public void SetCUS_SEL_FCCL_EQUIPMENT_G(ComboBox cbo, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + cbo.Name, Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCUS_SEL_FCCL_EQUIPMENT_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("CUS_SEL_FCCL_EQUIPMENT_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    // cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + cbo.Name, ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + cbo.Name, Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region ORG 별 ATTRDEFINE 조회
        public void GetCUS_SEL_ATTRDEFINE_BY_ORG(C1.WPF.DataGrid.C1DataGrid dataGrid, string sOrgCode, string sTabName)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "GetCUS_SEL_ATTRDEFINE_BY_ORG", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCUS_SEL_ATTRDEFINE_BY_ORG();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["ORG_CODE"] = sOrgCode;
            searchCondition["TABNAME"] = sTabName;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("CUS_SEL_ATTRDEFINE_BY_ORG", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    for (int iAttrCnt = 0; iAttrCnt < searchResult.Rows.Count; iAttrCnt++)
                    {
                        string sName = searchResult.Rows[iAttrCnt]["COLID"].ToString();
                        string sHeader = searchResult.Rows[iAttrCnt]["COLNAME"].ToString();
                        dataGrid.Columns["" + sName + ""].Visibility = System.Windows.Visibility.Visible;
                        dataGrid.Columns["" + sName + ""].Header = sHeader;
                    }
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R +"GetCUS_SEL_ATTRDEFINE_BY_ORG", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "GetCUS_SEL_ATTRDEFINE_BY_ORG", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

		#region 출하회차 Combo 조회
		public void GetCUS_SEL_SHIP_COUNT_CBO(ComboBox cbo, ComboStatus sComboStatus)
		{
			DataTable dResult = new DataTable();
			dResult.Columns.Add("CBO_CODE");
			dResult.Columns.Add("CBO_NAME");

			dResult.Rows.Add("1", "1");
			dResult.Rows.Add("2", "2");
			dResult.Rows.Add("3", "3");
			dResult.Rows.Add("4", "4");
			dResult.Rows.Add("5", "5");
			dResult.Rows.Add("6", "6");
			dResult.Rows.Add("7", "7");
			dResult.Rows.Add("8", "8");
			dResult.Rows.Add("9", "9");
			dResult.Rows.Add("10", "10");

			cbo.ItemsSource = SetCombo(dResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

			if (cbo.Items.Count > 0)
				cbo.SelectedIndex = 0;
		}
		#endregion
		
	    #region [※ 자재 LOT상태 정보 불러오기]
		public void SetCUS_SEL_PM_MLOT_LOT_ST_CODE_QP1PM1(ControlsLibrary.MultiSelectionBox cbo, string sShopID, string sOrgCode, string sMultOrgCode, string sMlotStCode, string sMlotStGrCode
			                                           , string sProcID, string sTgProcID, string sWipStat)
		{

			Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_PM_MLOT_LOT_ST_CODE_QP1PM1", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = new DataTable();
			searchConditionTable.Columns.Add("LANGID", typeof(string));
			searchConditionTable.Columns.Add("SHOPID", typeof(string));
			searchConditionTable.Columns.Add("ORG_CODE", typeof(string));
			searchConditionTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
			searchConditionTable.Columns.Add("MLOT_LOT_ST_CODE", typeof(string));
			searchConditionTable.Columns.Add("MLOT_LOT_ST_GR_CODE", typeof(string));
			searchConditionTable.Columns.Add("PROCID", typeof(string));
			searchConditionTable.Columns.Add("TG_PROCID", typeof(string));
			searchConditionTable.Columns.Add("WIPSTAT", typeof(string));

			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = sShopID;
			searchCondition["ORG_CODE"] = sOrgCode;
			searchCondition["MULTI_ORG_CODE"] = sMultOrgCode;
			searchCondition["MLOT_LOT_ST_CODE"] = sMlotStCode;
			searchCondition["MLOT_LOT_ST_GR_CODE"] = sMlotStGrCode;
			searchCondition["PROCID"] = sProcID;
			searchCondition["TG_PROCID"] = sTgProcID;
			searchCondition["WIPSTAT"] = sWipStat;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUS_SEL_PM_MLOT_LOT_ST_CODE_QP1PM1", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					if (sMlotStGrCode == "A" || sMlotStGrCode == "B")	// 가용재고(A), 재고(B)
					{
						for (int i = 0; i < searchResult.Rows.Count; i++)
						{
							string selFlag =  Util.NVC(searchResult.Rows[i]["SEL_FLAG"]);
							int  seqNo = Util.StringToInt(searchResult.Rows[i]["SEQ_NO"].ToString());
							if (selFlag == "Y")
							{
								cbo.Check(seqNo);
							}
						}
					}
					else 
					{
						cbo.CheckAll();
					}
					
					//if (cbo.Count > 0)
					//	cbo.SelectedIndex = 0;
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_PM_MLOT_LOT_ST_CODE_QP1PM1", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_PM_MLOT_LOT_ST_CODE_QP1PM1", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
        #endregion

		#region [※ 품명 정보 불러오기]
		public void SetCOR_SEL_ORG_GD_NAME_ONLY_CBO_G(ControlsLibrary.MultiSelectionBox cbo, string sShopID, string sOrgCode, string sMultOrgCode
													   , string sMtrlType, string sMtrlID)
		{

			Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_ORG_GD_NAME_ONLY_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = new DataTable();
			searchConditionTable.Columns.Add("LANGID", typeof(string));
			searchConditionTable.Columns.Add("SHOPID", typeof(string));
			searchConditionTable.Columns.Add("ORG_CODE", typeof(string));
			searchConditionTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
			searchConditionTable.Columns.Add("MTRLTYPE", typeof(string));
			searchConditionTable.Columns.Add("MTRLID", typeof(string));

			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = sShopID;
			searchCondition["ORG_CODE"] = sOrgCode;
			searchCondition["MULTI_ORG_CODE"] = sMultOrgCode;
			searchCondition["MTRLTYPE"] = sMtrlType;
			searchCondition["MTRLID"] = sMtrlID;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("COR_SEL_ORG_GD_NAME_ONLY_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					
					if (searchResult.Rows.Count > 0)
					{
						cbo.CheckAll();
					}
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_ORG_GD_NAME_ONLY_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_ORG_GD_NAME_ONLY_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ 품명 정보 불러오기- 자재현황(PM)]
		public void SetCUS_SEL_ORG_GD_NAME_CBO_QP1PM1(ControlsLibrary.MultiSelectionBox cbo, string sShopID, string sOrgCode, string sMultOrgCode
													   , string sMtrlType, string sMtrlID, string sMultiTpCode)
		{

			Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_ORG_GD_NAME_CBO_QP1PM1", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = new DataTable();
			searchConditionTable.Columns.Add("LANGID", typeof(string));
			searchConditionTable.Columns.Add("SHOPID", typeof(string));
			searchConditionTable.Columns.Add("ORG_CODE", typeof(string));
			searchConditionTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
			searchConditionTable.Columns.Add("MTRLTYPE", typeof(string));
			searchConditionTable.Columns.Add("MTRLID", typeof(string));
			searchConditionTable.Columns.Add("MULTI_MLOT_TP_CODE", typeof(string));

			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["SHOPID"] = sShopID;
			searchCondition["ORG_CODE"] = sOrgCode;
			searchCondition["MULTI_ORG_CODE"] = sMultOrgCode;
			searchCondition["MTRLTYPE"] = sMtrlType;
			searchCondition["MTRLID"] = sMtrlID;
			searchCondition["MULTI_MLOT_TP_CODE"] = sMultiTpCode;

			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUS_SEL_ORG_GD_NAME_CBO_QP1PM1", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = DataTableConverter.Convert(searchResult);

					if (searchResult.Rows.Count > 0)
					{
						cbo.CheckAll();
					}
				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_ORG_GD_NAME_CBO_QP1PM1", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_ORG_GD_NAME_CBO_QP1PM1", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ 자재변경상태 불러오기- 자재현황(PM)]
		public void SetCUS_SEL_PM_MLOT_LOT_CHG_ST_CODE_QP1PM1(ComboBox cbo, string sOrgCode, string sMlotStCode, string sMlotTpCode, ComboStatus sComboStatus, string sSelValue = "")
		{

			Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_PM_MLOT_LOT_CHG_ST_CODE_QP1PM1", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = new DataTable();
			searchConditionTable.Columns.Add("LANGID", typeof(string));
			searchConditionTable.Columns.Add("ORG_CODE", typeof(string));
			searchConditionTable.Columns.Add("MLOT_LOT_ST_CODE", typeof(string));
			searchConditionTable.Columns.Add("MLOT_TP_CODE", typeof(string));

			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["LANGID"] = LoginInfo.LANGID;
			searchCondition["ORG_CODE"] = sOrgCode;
			searchCondition["MLOT_LOT_ST_CODE"] = sMlotStCode;
			searchCondition["MLOT_TP_CODE"] = sMlotTpCode;
			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("CUS_SEL_PM_MLOT_LOT_CHG_ST_CODE_QP1PM1", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (sSelValue != "" && (from DataRow DRow in searchResult.Rows where DRow["CBO_CODE"].Equals(sSelValue) select DRow).Count() > 0)
					{
						// cboArea.SelectedValue = sSelValue;
						for (int i = 0; i < cbo.Items.Count; i++)
						{
							if (sSelValue == ((DataRowView)cbo.Items[i]).Row.ItemArray[0].ToString())
							{
								cbo.SelectedIndex = i;
								return;
							}
						}
					}
					else if (searchResult.Rows.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_PM_MLOT_LOT_CHG_ST_CODE_QP1PM1", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_PM_MLOT_LOT_CHG_ST_CODE_QP1PM1", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

		#region [※ PM 상세공정 불러오기]
		/// <summary>
		/// PM 상세공정 불러오기
		/// </summary>
		/// <param name="cboProd"></param>
		/// <param name="itemString"></param>
		public void SetBR_COR_SEL_TG_PROCID_CBO_G(ComboBox cbo, string orgCode, string lotID, string toProcID, ComboStatus sComboStatus)
		{
			Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_COR_SEL_TG_PROCID_CBO_G", Logger.MESSAGE_OPERATION_START);

			DataTable searchConditionTable = _Biz.GetBR_COR_SEL_TG_PROCID_CBO_G();
			DataRow searchCondition = searchConditionTable.NewRow();
			searchCondition["ORG_CODE"] = orgCode;
			searchCondition["LOTID"] = lotID;
			searchCondition["TO_PROCID"] = toProcID;
			searchConditionTable.Rows.Add(searchCondition);

			new ClientProxy().ExecuteService("BR_COR_SEL_TG_PROCID_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
			{
				try
				{
					if (searchException != null)
					{
						ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
						return;
					}

					// cbo.ItemsSource = DataTableConverter.Convert(searchResult);
					cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

					if (cbo.Items.Count > 0)
						cbo.SelectedIndex = 0;

				}
				catch (Exception ex)
				{
					ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_COR_SEL_TG_PROCID_CBO_G", ex);
				}
				finally
				{
					Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_COR_SEL_TG_PROCID_CBO_G", Logger.MESSAGE_OPERATION_END);
				}
			}
			);
		}
		#endregion

        #region [※ PRODUCT BY ROUTE 불러오기]
        public void SetCOR_SEL_PRODUCTROUTE_CBO_G(ComboBox cbo, string sProdId, string sShopId, string sSelVal, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_COR_SEL_TG_PROCID_CBO_G", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_PRODUCTROUTE_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["PRODID"] = sProdId;
            searchCondition["SHOPID"] = sShopId;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_PRODUCTROUTE_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 1 && !String.IsNullOrEmpty(sSelVal))
                    {
                        cbo.SelectedValue = sSelVal;
                    }
                    else
                    {
                        cbo.SelectedIndex = 0;
                    }

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_COR_SEL_TG_PROCID_CBO_G", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetBR_COR_SEL_TG_PROCID_CBO_G", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ 대표 제품ID Combobox 정보 불러오기]
        public void SetCOR_SEL_REP_PRODID_CBO_G(ControlsLibrary.MultiSelectionBox cbo, string sShopID, string sOrgCode, string sMultOrgCode
                                                       , string sProdId, string sModelId, string sProdType, string sMultiProdTp, string sClass1Cd, string sClass2Cd, string sClass3Cd, string sClass4Cd, string sClass5Cd)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_REP_PRODID_CBO_G", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_REP_PRODID_CBO_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["ORG_CODE"] = sOrgCode;
            searchCondition["MULTI_ORG_CODE"] = sMultOrgCode;
            searchCondition["PRODID"] = sProdId;
            searchCondition["MODLID"] = sModelId;
            searchCondition["PRODTYPE"] = sProdType;
            searchCondition["MULTI_PRODTYPE"] = sMultiProdTp;
            searchCondition["PROD_CLASS1_CODE"] = sClass1Cd;
            searchCondition["PROD_CLASS2_CODE"] = sClass2Cd;
            searchCondition["PROD_CLASS3_CODE"] = sClass3Cd;
            searchCondition["PROD_CLASS4_CODE"] = sClass4Cd;
            searchCondition["PROD_CLASS5_CODE"] = sClass5Cd;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_REP_PRODID_CBO_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    cbo.ItemsSource = DataTableConverter.Convert(searchResult);

                    if (searchResult.Rows.Count > 0)
                    {
                        cbo.CheckAll();
                    }
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_REP_PRODID_CBO_G", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_REP_PRODID_CBO_G", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ 배열 ComboBox 넣기]
        public void SetValueCombo(ComboBox cbo, DataTable dtDataInfo, String sCboCodeHeader, String sCboNameHeader, ComboStatus sComboStatus)
        {
            try
            {
                DataTable dtValue = new DataTable();
                dtValue.Columns.Add("CBO_NAME", typeof(string));
                dtValue.Columns.Add("CBO_CODE", typeof(string));

                string sCboCode = String.Empty;
                string sCboName = String.Empty;

                //역순으로 Add
                for (int i = dtDataInfo.Rows.Count; i > 0; i--)
                {
                    sCboCode = dtDataInfo.Rows[i-1][sCboCodeHeader].ToString();
                    sCboName = dtDataInfo.Rows[i-1][sCboNameHeader].ToString();

                    DataRow dr = dtValue.NewRow();
                    dr["CBO_NAME"] = sCboName;
                    dr["CBO_CODE"] = sCboCode;
                    dtValue.Rows.InsertAt(dr, 0);
                }

                //cbo.ItemsSource = DataTableConverter.Convert(dtValue);
                cbo.ItemsSource = SetCombo(dtValue, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }
        #endregion

        #region [※ TOOL 불러오기]
        public void SetCUS_SEL_TOOL_CBO(ComboBox cbo, string sToolItemID, string sToolTpCode, string sProcID, string sLotID, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_TOOL_CBO", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCUS_SEL_TOOL_CBO();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["TOOLITEMID"] = sToolItemID;
            searchCondition["TOOL_TP_CODE"] = sToolTpCode;
            searchCondition["PROCID"] = sProcID;
            searchCondition["LOTID"] = sLotID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("CUS_SEL_TOOL_FCCL", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_TOOL_CBO", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_TOOL_CBO", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ TOOL 사용횟수(설비별) 불러오기]
        public void SetCUS_SEL_TOOL_USEQTY(string sToolItemID, TextBlock tbl, TextBox txt, int iSelectedIndex)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_TOOL_CBO", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCUS_SEL_TOOL_CBO();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["TOOLITEMID"] = sToolItemID;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("CUS_SEL_TOOL_FCCL", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    if (searchResult.Rows.Count > 0)
                    {
                        if (tbl != null)
                        {
                            tbl.Text = searchResult.Rows[iSelectedIndex]["USEQTY"].ToString();
                        }

                        if (txt != null)
                        {
                            txt.Text = searchResult.Rows[iSelectedIndex]["USEQTY"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_TOOL_CBO", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCUS_SEL_TOOL_CBO", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ 툴 오더]
        public void CUS_SEL_TOOL_DRAWING(object CTRL,


            string LANGID,
            string TOOLITEMID,
            string TOOL_DRAWING_NO,
            string DRAWING_TP_CODE,
            string FILE_ID,

            ComboStatus COMBO_STATUS, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_TOOL_DRAWING", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("TOOLITEMID", typeof(string));
            IndataTable.Columns.Add("TOOL_DRAWING_NO", typeof(string));
            IndataTable.Columns.Add("DRAWING_TP_CODE", typeof(string));
            IndataTable.Columns.Add("FILE_ID", typeof(string));




            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["TOOLITEMID"] = TOOLITEMID;
            Indata["TOOL_DRAWING_NO"] = TOOL_DRAWING_NO;
            Indata["DRAWING_TP_CODE"] = DRAWING_TP_CODE;
            Indata["FILE_ID"] = FILE_ID;



            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("CUS_SEL_TOOL_DRAWING", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }


                    if (CTRL is C1DataGrid)
                    {
                        C1DataGrid DATA_GRID = CTRL as C1DataGrid;
                        DATA_GRID.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    else if (CTRL is ComboBox)
                    {
                        ComboBox COMBO_BOX = CTRL as ComboBox;

                        COMBO_BOX.ItemsSource = SetCombo(searchResult, COMBO_BOX.SelectedValuePath.ToString(), COMBO_BOX.DisplayMemberPath.ToString(), COMBO_STATUS);

                        if (COMBO_BOX.Items.Count > 0)
                            COMBO_BOX.SelectedIndex = 0;
                    }

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_TOOL_DRAWING", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_TOOL_DRAWING", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }



        public void COR_SEL_PROCESS_SHOP_CBO(ComboBox COMBO_BOX,

string LANGID,
string SHOPID,
string AREAID,
string PCSGID,
string PROCTYPE,
            ComboStatus COMBO_STATUS, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_TOOL_ITEM", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("PCSGID", typeof(string));
            IndataTable.Columns.Add("PROCTYPE", typeof(string));




            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SHOPID"] = SHOPID;
            Indata["AREAID"] = AREAID;
            Indata["PCSGID"] = PCSGID;
            Indata["PROCTYPE"] = PROCTYPE;



            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("COR_SEL_PROCESS_SHOP_CBO", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }


                    // cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    COMBO_BOX.ItemsSource = SetCombo(searchResult, COMBO_BOX.SelectedValuePath.ToString(), COMBO_BOX.DisplayMemberPath.ToString(), COMBO_STATUS);

                    if (COMBO_BOX.Items.Count > 0)
                        COMBO_BOX.SelectedIndex = 0;

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PROCESS_SHOP_CBO", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_PROCESS_SHOP_CBO", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }


        public void CUS_SEL_TOOL_ITEM(ComboBox COMBO_BOX, string SHOPID, string TOOL_TP_CODE, ComboStatus COMBO_STATUS, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_TOOL_ITEM", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("TOOL_TP_CODE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SHOPID"] = SHOPID;
            Indata["TOOL_TP_CODE"] = TOOL_TP_CODE;


            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("CUS_SEL_TOOL_ITEM", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }


                    // cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    COMBO_BOX.ItemsSource = SetCombo(searchResult, COMBO_BOX.SelectedValuePath.ToString(), COMBO_BOX.DisplayMemberPath.ToString(), COMBO_STATUS);

                    if (COMBO_BOX.Items.Count > 0)
                        COMBO_BOX.SelectedIndex = 0;

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_TOOL_ITEM", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "CUS_SEL_TOOL_ITEM", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        public void COR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO(
            ComboBox COMBO_BOX,
string LANGID,
string SHOPID,
string ORG_CODE,
string MULTI_ORG_CODE,
string CMCDTYPE,
string CMCODE,
string ATTRIBUTE1,
string ATTRIBUTE2,
string ATTRIBUTE3,
string ATTRIBUTE4,
string ATTRIBUTE5,


            ComboStatus COMBO_STATUS, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();

            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("ORG_CODE", typeof(string));
            IndataTable.Columns.Add("MULTI_ORG_CODE", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("CMCODE", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE1", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE2", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE3", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE4", typeof(string));
            IndataTable.Columns.Add("ATTRIBUTE5", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LANGID;
            Indata["SHOPID"] = SHOPID;
            Indata["ORG_CODE"] = ORG_CODE;
            Indata["MULTI_ORG_CODE"] = MULTI_ORG_CODE;
            Indata["CMCDTYPE"] = CMCDTYPE;
            Indata["CMCODE"] = CMCODE;
            Indata["ATTRIBUTE1"] = ATTRIBUTE1;
            Indata["ATTRIBUTE2"] = ATTRIBUTE2;
            Indata["ATTRIBUTE3"] = ATTRIBUTE3;
            Indata["ATTRIBUTE4"] = ATTRIBUTE4;
            Indata["ATTRIBUTE5"] = ATTRIBUTE5;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("COR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }


                    // cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    COMBO_BOX.ItemsSource = SetCombo(searchResult, COMBO_BOX.SelectedValuePath.ToString(), COMBO_BOX.DisplayMemberPath.ToString(), COMBO_STATUS);

                    if (COMBO_BOX.Items.Count > 0)
                        COMBO_BOX.SelectedIndex = 0;

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }






        public void CUS_SEL_TOOL_MAKER_CBO(ComboBox cbo, ComboStatus sComboStatus, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("DSGN_PROD_TP_CODE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;


            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("CUS_SEL_TOOL_MAKER_CBO", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }


                    // cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboLotType_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        #endregion

        #region [※ ORG 공통 코드 Attribute 정보 불러오기]
        public void SetCOR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO(ComboBox cbo, string SHOPID, string ORG_CODE, string MULTI_ORG_CODE, string CMCDTYPE, string CMCODE, 
        string ATTRIBUTE1, string ATTRIBUTE2, string ATTRIBUTE3, string ATTRIBUTE4, string ATTRIBUTE5, 
        string MULTI_ATTRIBUTE1, string MULTI_ATTRIBUTE2, string MULTI_ATTRIBUTE3, string MULTI_ATTRIBUTE4, string MULTI_ATTRIBUTE5, string J36_ATTRIBUTE1, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = SHOPID;
            searchCondition["ORG_CODE"] = ORG_CODE;
            searchCondition["MULTI_ORG_CODE"] = MULTI_ORG_CODE;
            searchCondition["CMCDTYPE"] = CMCDTYPE;
            searchCondition["CMCODE"] = CMCODE;
            searchCondition["ATTRIBUTE1"] = ATTRIBUTE1;
            searchCondition["ATTRIBUTE2"] = ATTRIBUTE2;
            searchCondition["ATTRIBUTE3"] = ATTRIBUTE3;
            searchCondition["ATTRIBUTE4"] = ATTRIBUTE4;
            searchCondition["ATTRIBUTE5"] = ATTRIBUTE5;
            searchCondition["MULTI_ATTRIBUTE1"] = MULTI_ATTRIBUTE1;
            searchCondition["MULTI_ATTRIBUTE2"] = MULTI_ATTRIBUTE2;
            searchCondition["MULTI_ATTRIBUTE3"] = MULTI_ATTRIBUTE3;
            searchCondition["MULTI_ATTRIBUTE4"] = MULTI_ATTRIBUTE4;
            searchCondition["MULTI_ATTRIBUTE5"] = MULTI_ATTRIBUTE5;
            searchCondition["J36_ATTRIBUTE1"] = J36_ATTRIBUTE1;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ ORG 공통 코드 Attribute 정보 Return]
        public void SetCOR_SEL_ORG_COM_CODE_ATTRIBUTE_GRID(DataTable dtGrid, string sPinHoleInspGrade, string sFinalResult, ComboBox cboResultGrade, string SHOPID, string ORG_CODE, string MULTI_ORG_CODE, string CMCDTYPE, string CMCODE,
        string ATTRIBUTE1, string ATTRIBUTE2, string ATTRIBUTE3, string ATTRIBUTE4, string ATTRIBUTE5,
        string MULTI_ATTRIBUTE1, string MULTI_ATTRIBUTE2, string MULTI_ATTRIBUTE3, string MULTI_ATTRIBUTE4, string MULTI_ATTRIBUTE5)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = SHOPID;
            searchCondition["ORG_CODE"] = ORG_CODE;
            searchCondition["MULTI_ORG_CODE"] = MULTI_ORG_CODE;
            searchCondition["CMCDTYPE"] = CMCDTYPE;
            searchCondition["CMCODE"] = CMCODE;
            searchCondition["ATTRIBUTE1"] = ATTRIBUTE1;
            searchCondition["ATTRIBUTE2"] = ATTRIBUTE2;
            searchCondition["ATTRIBUTE3"] = ATTRIBUTE3;
            searchCondition["ATTRIBUTE4"] = ATTRIBUTE4;
            searchCondition["ATTRIBUTE5"] = ATTRIBUTE5;
            searchCondition["MULTI_ATTRIBUTE1"] = MULTI_ATTRIBUTE1;
            searchCondition["MULTI_ATTRIBUTE2"] = MULTI_ATTRIBUTE2;
            searchCondition["MULTI_ATTRIBUTE3"] = MULTI_ATTRIBUTE3;
            searchCondition["MULTI_ATTRIBUTE4"] = MULTI_ATTRIBUTE4;
            searchCondition["MULTI_ATTRIBUTE5"] = MULTI_ATTRIBUTE5;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_ORG_COM_CODE_ATTRIBUTE_CBO", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    int iSUM = 0;

                    //Visual Inspaction Grade등급 별 점수 계산
                    for (int iGrade = 0; iGrade < dtGrid.Rows.Count; iGrade++)
                    {
                        string sGrade = dtGrid.Rows[iGrade]["GRADE"].ToString();
                        for (int iReslt = 0; iReslt < searchResult.Rows.Count; iReslt++)
                        {
                            string sResult = searchResult.Rows[iReslt]["CBO_CODE"].ToString();
                            if (sGrade == sResult)
                            {
                                iSUM = iSUM + int.Parse(searchResult.Rows[iReslt]["ATTRIBUTE1"].ToString());
                            }
                        }
                    }

                    string sResultGrade = String.Empty;

                    if(iSUM == 5)
                    {
                        sResultGrade = "A";
                    }
                    else if(iSUM >= 6 && iSUM < 9)
                    {
                        sResultGrade = "B";
                    }
                     else if(iSUM >= 9 && iSUM <= 15)
                    {
                        sResultGrade = "C";
                    }
                     else
                    {
                        sResultGrade = "";
                    }

                    //PinHole Insp 등급이 있으면
                    if (!String.IsNullOrEmpty(sPinHoleInspGrade))
                    {
                        if (sResultGrade == "A")
                        {
                            sResultGrade = sPinHoleInspGrade;
                        }
                        else if (sResultGrade == "B")
                        {
                            if (sPinHoleInspGrade == "C")
                            {
                                sResultGrade = sPinHoleInspGrade;
                            }
                        }
                    }

                    if (cboResultGrade.Items.Count > 0)
                    {
                        cboResultGrade.SelectedValue = sResultGrade;
                    }

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "cboOrderState_Loaded", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ 자재LOT IQC 결과 정보 불러오기]
        public void SetCOR_SEL_MLOT_IQC_RSLT_G(C1.WPF.DataGrid.C1DataGrid grid, string sSHOPID, string sMTRL_GR_CODE, string sMAKER_CODE, string sMTRL_CLASS_CODE, string sMTRL_GRD_CODE, string sINSP_TP_CODE, string sCLCTITEM, double dMEASR_VALUE, int iCurrentRowIdx, bool bGradeIuse, Action<DataTable, Exception> ACTION_COMPLETED = null)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_MLOT_IQC_RSLT_G", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_MLOT_IQC_RSLT_G();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sSHOPID;
            searchCondition["MTRL_GR_CODE"] = sMTRL_GR_CODE;
            searchCondition["MAKER_CODE"] = sMAKER_CODE;
            searchCondition["MTRL_CLASS_CODE"] = sMTRL_CLASS_CODE;
            searchCondition["MTRL_GRD_CODE"] = sMTRL_GRD_CODE;
            searchCondition["INSP_TP_CODE"] = sINSP_TP_CODE;
            searchCondition["CLCTITEM"] = sCLCTITEM;
            searchCondition["MEASR_VALUE"] = dMEASR_VALUE;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_MLOT_IQC_RSLT_G", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    if (searchResult.Rows.Count > 0)
                    {
                        DataTableConverter.SetValue(grid.Rows[iCurrentRowIdx].DataItem, "JUDGE", searchResult.Rows[0]["IQC_JUDGE"].ToString());

                        if (bGradeIuse == true)
                        {
                            DataTableConverter.SetValue(grid.Rows[iCurrentRowIdx].DataItem, "GRADE", searchResult.Rows[0]["MLOT_GRD_CODE"].ToString());
                        }
                    }

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_MLOT_IQC_RSLT_G", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_MLOT_IQC_RSLT_G", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        #region [※ 설비 상태]
        public void SetCOR_SEL_EIO(TextBlock txt, string sEqptId)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_EIO", Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = new DataTable();
            searchConditionTable.Columns.Add("EQPTID", typeof(string));

            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["EQPTID"] = sEqptId;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_EIO", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                   txt.Tag = searchResult.Rows[0]["EIOSTAT"].ToString();

                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_EIO", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "SetCOR_SEL_EIO", Logger.MESSAGE_OPERATION_END);
                }
            });
        }
        #endregion

        #region [※ 공통 코드조회(tb_mmd_shop_com_code)]
        /// <summary>
        /// 공통 코드조회
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="sPdgrId"></param>
        /// <param name="sComboStatus"></param>
        public void SetCOR_SEL_SHOP_COM_CODE(ComboBox cbo, string sShopID, string sCMCDTYPE, string sCMCODE, ComboStatus sComboStatus)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + cbo.Name, Logger.MESSAGE_OPERATION_START);

            DataTable searchConditionTable = _Biz.GetCOR_SEL_SHOP_COM_CODE();
            DataRow searchCondition = searchConditionTable.NewRow();
            searchCondition["LANGID"] = LoginInfo.LANGID;
            searchCondition["SHOPID"] = sShopID;
            searchCondition["CMCDTYPE"] = sCMCDTYPE;
            searchCondition["CMCODE"] = sCMCODE;

            searchConditionTable.Rows.Add(searchCondition);

            new ClientProxy().ExecuteService("COR_SEL_SHOP_COM_CODE", "INDATA", "OUTDATA", searchConditionTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    //cbo.ItemsSource = DataTableConverter.Convert(searchResult);
                    cbo.ItemsSource = SetCombo(searchResult, cbo.SelectedValuePath.ToString(), cbo.DisplayMemberPath.ToString(), sComboStatus);

                    if (cbo.Items.Count > 0)
                        cbo.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + cbo.Name, ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + cbo.Name, Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }

        #endregion

        #region [※ LOT 정보조회]
        public void SetCOR_SEL_LOT(string sLOTID, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_LOT", Logger.MESSAGE_OPERATION_START);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = sLOTID;

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("COR_SEL_LOT", "INDATA", "OUTDATA", IndataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }

                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(searchResult, searchException);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_LOT", ex);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + "COR_SEL_LOT", Logger.MESSAGE_OPERATION_END);
                }
            }
            );
        }
        #endregion

        public void COR_SEL_AUTHORITYMENU_G(string userID, string formID, Action<bool, string> ACTION_COMPLETED)
        {
            if (userID.Equals(""))
            {   //10000-사용자 ID를 입력하세요.  
                System.Windows.MessageBox.Show(MessageDic.Instance.GetMessage("10000"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DataTable InDataTable = new DataTable();
            InDataTable.Columns.Add("USERID", typeof(string));
            InDataTable.Columns.Add("FORMID", typeof(string));
            InDataTable.Columns.Add("SHOPID", typeof(string));

            DataRow InData = InDataTable.NewRow();
            InData["USERID"] = userID;
            InData["FORMID"] = formID;
            InData["SHOPID"] = CustomConfig.Instance.CONFIG_COMMON_SHOP;
            InDataTable.Rows.Add(InData);

            new ClientProxy().ExecuteService("COR_SEL_AUTHORITYMENU_G", "INDATA", "OUTDATA", InDataTable, (DataResult, DataException) =>
            {
                try
                {
                    if (DataException != null)
                    {
                        System.Windows.MessageBox.Show(MessageDic.Instance.GetMessage(DataException));
                        return;
                    }
                    string sReturn = string.Empty;
                    bool sBoolean = false;

                    if (DataResult.Rows.Count > 0)
                    {
                        sReturn = DataResult.Rows[0]["ACCESS_FLAG"].ToString();

                        if (sReturn == "W")
                        {
                            sBoolean = true;          
                        }
                    }

                    ACTION_COMPLETED(sBoolean, sReturn);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }

            }
            );
        }

    }
}
