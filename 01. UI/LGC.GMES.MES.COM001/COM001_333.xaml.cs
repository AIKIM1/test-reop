/*************************************************************************************
 Created Date : 2020.11.19
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 사용자 쿼리 실행 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.19  조영대 : Initial Created.
  2022.09.14  염규범 : MAX_ROW_COUNT  수량 증가 10,000 -> 20,000
  2022.10.14  최재욱 : [C20210928-000120] 권한, 요청자 정보에 따른 Query 조회
  2023.03.21  조영대 : 컬럼명에 / 가 있을경우 제거
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_333 : UserControl, IWorkArea
    {
        #region Declaration & Constructor


        private double DEFAULT_ROW_COUNT = 5000;
        //2022.09.14 염규범S
        //private double MAX_ROW_COUNT = 10000;
        private double MAX_ROW_COUNT = 20000;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize        

        public COM001_333()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            numRowMaxCount.Value = DEFAULT_ROW_COUNT;
        }

        private void InitializeGrid()
        {
            Util.gridClear(dgQuery);
            Util.gridClear(dgParameter);
            Util.gridClear(dgResult);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitializeControls();
            InitializeGrid();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 조회 
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchQuery();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (dgQuery.GetRowCount() == 0) return;

            string query = Util.NVC(DataTableConverter.GetValue(dgQuery.CurrentRow.DataItem, "QRY_CNTT"));
            if (CheckValidaton(query))
            {
                QueryRun(query);
            }
        }

        private void txtUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GetPersonInfo(sender);
            }
        }


        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetPersonInfo(sender);
        }

        private void popupPerson_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                txtSearchUserName.Text = popup.USERNAME;
                txtSearchUserName.Tag = popup.USERID;
            }
        }

        private void numRowMaxCount_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (numRowMaxCount.Value > MAX_ROW_COUNT)
            {
                numRowMaxCount.Value = MAX_ROW_COUNT;
            }
            else if (numRowMaxCount.Value < 1)
            {
                numRowMaxCount.Value = 1;
            }
        }

        private void txtRowMaxCount_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void dgQuery_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            try
            {
                DataRowView drv = dgQuery.SelectedItem as DataRowView;
                if (drv == null) return;


                SearchParameterData(Util.NVC(drv["STORED_QRY_ID"]));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnSearch,
                btnRun
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void GetPersonInfo(object sender)
        {
            if (sender == null) return;

            CMM_PERSON popupPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = txtSearchUserName.Text;
            C1WindowExtension.SetParameters(popupPerson, parameters);

            popupPerson.Closed += new EventHandler(popupPerson_Closed);
            grdMain.Children.Add(popupPerson);
            popupPerson.BringToFront();
        }

		//private void SearchQuery()
		//{
		//    try
		//    {
		//        ShowLoadingIndicator();

		//        InitializeGrid();

		//        DataTable IndataTable = new DataTable();
		//        IndataTable.Columns.Add("LANGID", typeof(string));
		//        IndataTable.Columns.Add("SHOPID", typeof(string));
		//        IndataTable.Columns.Add("AREAID", typeof(string));
		//        IndataTable.Columns.Add("REQ_USERID", typeof(string));
		//        IndataTable.Columns.Add("STORED_QRY_AUTH_CODE", typeof(string));
		//        IndataTable.Columns.Add("STORED_QRY_VLD_TYPE_CODE", typeof(string));
		//        IndataTable.Columns.Add("VLD_DATE", typeof(string));
		//        IndataTable.Columns.Add("USE_FLAG", typeof(string));

		//        DataRow Indata = IndataTable.NewRow();
		//        Indata["LANGID"] = LoginInfo.LANGID;
		//        Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
		//        Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
		//        string reqUserId = string.Empty;
		//        if (Util.NVC(txtSearchUserName.Text).Equals(string.Empty) || Util.NVC(txtSearchUserName.Tag).Equals(string.Empty))
		//        {
		//            Indata["REQ_USERID"] = null;
		//        }
		//        else
		//        {
		//            Indata["REQ_USERID"] = Util.NVC(txtSearchUserName.Tag);
		//        }
		//        Indata["STORED_QRY_AUTH_CODE"] = null;
		//        Indata["STORED_QRY_VLD_TYPE_CODE"] = null;
		//        Indata["VLD_DATE"] = DateTime.Today.ToString("yyyyMMdd");
		//        Indata["USE_FLAG"] = "Y";

		//        IndataTable.Rows.Add(Indata);

		//        new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SOM_STORED_QRY_DRB", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
		//        {
		//            HiddenLoadingIndicator();

		//            if (ex != null)
		//            {
		//                Util.MessageException(ex);
		//                return;
		//            }

		//            Util.GridSetData(dgQuery, result, FrameOperation, false);

		//            if (result != null && result.Rows.Count > 0)
		//            {
		//                Util.gridSetFocusRow(ref dgQuery, 0);
		//            }
		//        });
		//    }
		//    catch (Exception ex)
		//    {
		//        HiddenLoadingIndicator();
		//        Util.MessageException(ex);
		//    }
		//}

		private void SearchQuery()
		{
			try
			{
				ShowLoadingIndicator();

				InitializeGrid();

				DataTable dtResult = new DataTable();
				DataTable dtCommon = new DataTable(); //COMMON 권한
				DataTable dtReg = new DataTable(); //REG 권한

				if (Util.NVC(txtSearchUserName.Text).Equals(string.Empty) || Util.NVC(txtSearchUserName.Tag).Equals(string.Empty))
				{
					dtCommon = getCommData(null);
					dtReg = getRegData(LoginInfo.USERID);
				}
				else
				{
					if (Util.NVC(txtSearchUserName.Tag) == LoginInfo.USERID)
					{
						dtCommon = getCommData(LoginInfo.USERID);
						dtReg = getRegData(LoginInfo.USERID);
					}
					else
					{
						dtCommon = getCommData(Util.NVC(txtSearchUserName.Tag));
					}
				}

                //dtCommon.Merge(dtReg);
                dtCommon.Merge(dtReg, false, MissingSchemaAction.Ignore);
                dtResult = dtCommon;

				HiddenLoadingIndicator();

				Util.gridClear(dgQuery);
				Util.GridSetData(dgQuery, dtResult, null, false);
			}
			catch (Exception ex)
			{
				HiddenLoadingIndicator();
				Util.MessageException(ex);
			}
		}

		private DataTable getCommData(string userId)
		{
			DataTable dt = new DataTable();

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("SHOPID", typeof(string));
			IndataTable.Columns.Add("AREAID", typeof(string));
			IndataTable.Columns.Add("REQ_USERID", typeof(string));
			IndataTable.Columns.Add("STORED_QRY_AUTH_CODE", typeof(string));
			IndataTable.Columns.Add("STORED_QRY_VLD_TYPE_CODE", typeof(string));
			IndataTable.Columns.Add("VLD_DATE", typeof(string));
			IndataTable.Columns.Add("USE_FLAG", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
			Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
			string reqUserId = string.Empty;

			Indata["REQ_USERID"] = userId;

			Indata["STORED_QRY_AUTH_CODE"] = "COMMON";
			Indata["STORED_QRY_VLD_TYPE_CODE"] = null;
			Indata["VLD_DATE"] = DateTime.Today.ToString("yyyyMMdd");
			Indata["USE_FLAG"] = "Y";

			IndataTable.Rows.Add(Indata);

			dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SOM_STORED_QRY_DRB", "RQSTDT", "RSLTDT", IndataTable);

			return dt;
		}


		private DataTable getRegData(string userId)
		{
			DataTable dt = new DataTable();

			DataTable IndataTable = new DataTable();
			IndataTable.Columns.Add("LANGID", typeof(string));
			IndataTable.Columns.Add("SHOPID", typeof(string));
			IndataTable.Columns.Add("AREAID", typeof(string));
			IndataTable.Columns.Add("REQ_USERID", typeof(string));
			IndataTable.Columns.Add("STORED_QRY_AUTH_CODE", typeof(string));
			IndataTable.Columns.Add("STORED_QRY_VLD_TYPE_CODE", typeof(string));
			IndataTable.Columns.Add("VLD_DATE", typeof(string));
			IndataTable.Columns.Add("USE_FLAG", typeof(string));

			DataRow Indata = IndataTable.NewRow();
			Indata["LANGID"] = LoginInfo.LANGID;
			Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
			Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
			string reqUserId = string.Empty;

			Indata["REQ_USERID"] = LoginInfo.USERID;

			Indata["STORED_QRY_AUTH_CODE"] = "REQ";
			Indata["STORED_QRY_VLD_TYPE_CODE"] = null;
			Indata["VLD_DATE"] = DateTime.Today.ToString("yyyyMMdd");
			Indata["USE_FLAG"] = "Y";

			IndataTable.Rows.Add(Indata);

			dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SOM_STORED_QRY_DRB", "RQSTDT", "RSLTDT", IndataTable);

			return dt;
		}

		private void SearchParameterData(string storedQryId)
        {
            try
            {
                Util.gridClear(dgParameter);

                DataTable InDataTable = new DataTable();
                InDataTable.Columns.Add("STORED_QRY_ID", typeof(string));

                DataRow InData = InDataTable.NewRow();
                InData["STORED_QRY_ID"] = storedQryId;
                InDataTable.Rows.Add(InData);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SOM_STORED_QRY_PARA_DRB", "RQSTDT", "RSLTDT", InDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgParameter, result, FrameOperation, false);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private bool CheckValidaton(string query)
        {
            if (query.Trim().Equals(string.Empty))
            {
                Util.MessageValidation("SFU1444", "QRY_CNTT");
                return false;
            }
            // 필수여부 이면서 값이 없으면.. 경고
            if (DataTableConverter.Convert(dgParameter.ItemsSource).AsEnumerable()
                //.Where(x => x.Field<Int32>("MAND_FLAG").Equals(1) && Util.NVC(x.Field<string>("PARA_VALUE")).Equals(string.Empty))
                .Where(x => x.Field<long>("MAND_FLAG").Equals(1) && Util.NVC(x.Field<string>("PARA_VALUE")).Equals(string.Empty)) // 2024.11.05. 김영국 - MAND_FLAG값이 long Type 으로 넘어오는 문제점 발생으로 Type수정.
                .Count() > 0)
            {
                // 필수입력항목을 모두 입력하십시오.
                Util.MessageValidation("SFU4979");
                return false;
            }

            if (!CheckQuery(query)) return false;

            return true;
        }

        private bool CheckQuery(string query)
        {
            StringBuilder newQuery = new StringBuilder();

            // /* */ 제거
            newQuery.Clear();
            int tokenCount = 0;

            for (int index = 0; index < query.Length; index++)
            {
                if (query[index].Equals('/') && query[index + 1].Equals('*'))
                {
                    tokenCount++;
                    index++;
                    continue;
                }
                if (query[index].Equals('*') && query[index + 1].Equals('/'))
                {
                    tokenCount--;
                    index++;
                    continue;
                }

                if (tokenCount.Equals(0)) newQuery.Append(query[index]);
            }
            string lastQuery = newQuery.ToString().ToUpper();

            // -- 제거
            newQuery.Clear();
            tokenCount = 0;

            for (int index = 0; index < lastQuery.Length; index++)
            {
                if (tokenCount.Equals(0) && lastQuery[index].Equals('-') && lastQuery[index + 1].Equals('-'))
                {
                    tokenCount++;
                    index++;
                    continue;
                }
                if (tokenCount > 0 && lastQuery[index].Equals('\r') && lastQuery[index + 1].Equals('\n'))
                {
                    tokenCount = 0;
                    index++;
                }
                if (tokenCount.Equals(0)) newQuery.Append(lastQuery[index]);
            }
            lastQuery = newQuery.ToString().ToUpper();

            // 빈라인 제거
            newQuery.Clear();
            string[] querys1 = lastQuery.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in querys1)
            {
                if (item.Replace("\r", "").Replace("\n", "").Trim().Equals(string.Empty)) continue;
                newQuery.AppendLine(item.Replace("\r", "").Trim());
            }
            newQuery.Remove(newQuery.Length - 2, 2);
            lastQuery = newQuery.ToString().ToUpper();

            // 사용금지 쿼리 확인
            List<string> noQueryToken = new List<string>();

            noQueryToken.Add("CREATE");
            noQueryToken.Add("DROP");
            noQueryToken.Add("INSERT");
            noQueryToken.Add("ALTER");
            noQueryToken.Add("TRUNCATE");
            noQueryToken.Add("UPDATE");
            noQueryToken.Add("DELETE");
            noQueryToken.Add("GRANT");
            noQueryToken.Add("REVOKE");
            noQueryToken.Add("CONNECT");
            noQueryToken.Add("RENAME");


            // 2022-04-13 염규범 S
            // C20220405-000382 요청에 의해, 성능상 문제가 있어, TEMP 테이블 및 커서 사용, 해당 쿼리 DBA(전우석 책임) 에게 검증 받음
            if (LoginInfo.CFG_AREA_ID.StartsWith("P") && Util.NVC(DataTableConverter.GetValue(dgQuery.CurrentRow.DataItem, "QRY_NAME")).Equals("041_SEARCH_FROM_CELL_PLT_TO_OUTER_BOXID"))
            {
                noQueryToken.Remove("CREATE");
                noQueryToken.Remove("DROP");
                noQueryToken.Remove("INSERT");
            }

            List<string> findToken = new List<string>();
            foreach (string noToken in noQueryToken)
            {
                if (lastQuery.IndexOf(noToken) > -1)
                {
                    if (!findToken.Contains(noToken)) findToken.Add(noToken);
                }
            }
            if (findToken.Count > 0)
            {
                Util.MessageInfo("SFU8291", findToken.Aggregate((i, j) => i + ", " + j));
                return false;
            }

            // 마지막 쿼리 찾기
            string[] querys2 = lastQuery.Split(new string[2] { ";", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            lastQuery = querys2.Last();

            // () 제거
            newQuery.Clear();
            tokenCount = 0;

            for (int index = 0; index < lastQuery.Length; index++)
            {
                if (lastQuery[index].Equals('('))
                {
                    tokenCount++;
                    continue;
                }
                if (lastQuery[index].Equals(')'))
                {
                    tokenCount--;
                    continue;
                }

                if (tokenCount.Equals(0)) newQuery.Append(lastQuery[index]);
            }
            lastQuery = newQuery.ToString().ToUpper();

            if (lastQuery.IndexOf("SELECT") > -1) return true;

            Util.MessageInfo("SFU1444", "SELECT");
            return false;
        }

        private void QueryRun(string query)
        {
            try
            {
                Util.gridClear(dgResult);

                // 2022-04-13 염규범 S
                // C20220405-000382 요청에 의해, 성능상 문제가 있어, TEMP 테이블 및 커서 사용, 해당 쿼리 DBA(전우석 책임) 에게 검증 받음
                // PLTID 를 10개 이상일 경우 DA 가 3초 이상으로 문제 발생이 있을수 있어서 Validation 추가
                string strQuery = string.Empty;
                strQuery = ConvertQuery(query.ToString());

                if (strQuery.Equals("ERROR")) return;

                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SQL_QUERY", typeof(string));
                DataRow drNew = dtRqst.NewRow();
                drNew["SQL_QUERY"] = strQuery;
                dtRqst.Rows.Add(drNew);

                new ClientProxy().ExecuteService("DA_PRD_SEL_QUERY_RUN_DRB", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //COL001 컬럼 제거
                    if (bizResult.Columns.Contains("COL001"))
                    {
                        bizResult.Columns.Remove("COL001");
                    }

                    // 컬럼명에 / 가 있을경우 제거
                    foreach (DataColumn col in bizResult.Columns)
                    {
                        if (col.ColumnName.Contains("/"))
                        {
                            col.ColumnName = col.ColumnName.Replace("/", "");
                        }
                    }

                    dgResult.AutoGenerateColumns = true;

                    Util.GridSetData(dgResult, bizResult, FrameOperation, true);

                    txtQueryResult.Text = ObjectDic.Instance.GetObjectName("QUERY_RESULT") + " (Row Count : " +
                             dgResult.GetRowCount().ToString("#,##0") + ")";

                    foreach (C1.WPF.DataGrid.DataGridColumn col in dgResult.Columns)
                    {
                        col.Header = col.Name;
                        if (bizResult.Columns[col.Name].DataType.Equals(typeof(DateTime)))
                        {
                            col.HorizontalAlignment = HorizontalAlignment.Center;
                        }
                    }


                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string ConvertQuery(string query)
        {
            string lastQuery = query.ToUpper();

            //KClastQuery = "SET ROWCOUNT " + Util.NVC(numRowMaxCount.Value) + ";\r\n\r\n" + lastQuery;

            DataTable dt = ((DataView)dgParameter.ItemsSource).Table;
            foreach (DataRow dr in dt.Rows)
            {
                if (ChkParaSpecifyCharCnt(Util.NVC(dr["PARA_VALUE"]).ToUpper()))
                {
                    lastQuery = lastQuery.Replace("@" + Util.NVC(dr["PARA_NAME"]).ToUpper(), "'" + Util.NVC(dr["PARA_VALUE"]).ToUpper() + "'");
                }
                else
                {
                    return "ERROR";
                }
            }

            //lastQuery = lastQuery + "\r\n\r\nSET ROWCOUNT 0;";
            lastQuery = lastQuery + "\r\n\r\nFETCH FIRST " + Util.NVC(numRowMaxCount.Value) + " ROW ONLY";

            return lastQuery;
        }

        private bool ChkParaSpecifyCharCnt(string strParaValue)
        {
            Boolean bChk = true;

            if (string.IsNullOrWhiteSpace(strParaValue)) return bChk;

            // 2022-04-13 염규범 S
            // C20220405-000382 요청에 의해, 성능상 문제가 있어, TEMP 테이블 및 커서 사용, 해당 쿼리 DBA(전우석 책임) 에게 검증 받음
            // PLTID 를 10개 이상일 경우 DA 가 3초 이상으로 문제 발생이 있을수 있어서 Validation 추가
            if (LoginInfo.CFG_AREA_ID.StartsWith("P") && Util.NVC(DataTableConverter.GetValue(dgQuery.CurrentRow.DataItem, "QRY_NAME")).Equals("041_SEARCH_FROM_CELL_PLT_TO_OUTER_BOXID"))
            {
                string strChkChar = ",";
                string[] StringArray = strParaValue.Split(new string[] { strChkChar }, StringSplitOptions.None);

                if(StringArray.Length > 10)
                {
                    // 필수입력항목을 모두 입력하십시오.
                    Util.MessageInfo("SFU8484", StringArray.Length.ToString());
                    bChk = false;
                    return bChk;
                }
            }
            return bChk;
        }
        #endregion

    }
}
