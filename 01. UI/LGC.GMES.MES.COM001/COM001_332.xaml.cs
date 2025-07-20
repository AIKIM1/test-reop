/*************************************************************************************
 Created Date : 2020.09.09
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - 사용자 Query 등록
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.10  신광희 : Initial Created.
  2020.11.23  조영대 : 사용자 쿼리 등록 추가 개발
  2021-02-03  조영대 : 저장 후 입력 컨트롤 클리어
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
using LGC.GMES.MES.CMM001.Extensions;
using System.Text;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_332 : UserControl, IWorkArea
    {
        #region Declaration & Constructor


        private string selectedRequestUser;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize        

        public COM001_332()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
        }

        private void InitializeGrid()
        {

        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            SetComboCmmCode(cboSearchAuthority);
            SetComboCmmCode(cboSearchUsePeriod);
            SetComboCmmCode(cboSearchUseFlag);
            SetComboCmmCode(cboAuthority);
            SetComboCmmCode(cboUsePeriod);
            SetComboCmmCode(cboUseFlag);

        }


        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitializeControls();
            InitializeGrid();
            InitCombo();
            //SetControls();

            ClearInputControl();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 조회 
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData(string.Empty);

            ClearInputControl();
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearInputControl();
        }



        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgParameter.ItemsSource == null)
                    return;

                DataTable dt = ((DataView)dgParameter.ItemsSource).Table;

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["CHK"] = false;

                DataRow dr = dt.NewRow();
                dr["CHK"] = false;
                dr["PARA_NAME"] = string.Empty;
                dr["PARA_NOTE"] = string.Empty;
                dr["DFLT_VALUE"] = string.Empty;
                dr["MAND_FLAG"] = false;
                dt.Rows.Add(dr);

                dt.AcceptChanges();

                // 스프레드 스크롤 하단으로 이동
                dgParameter.ScrollIntoView(dgParameter.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drs = Util.gridGetChecked(ref dgParameter, "CHK");

                if (drs != null && drs.Length > 0)
                {
                    //입력한 데이터가 삭제됩니다. 계속 하시겠습니까?
                    Util.MessageConfirm("SFU1815", (sResult) =>
                    {
                        if (sResult == MessageBoxResult.OK)
                        {
                            DataRow[] ds = Util.gridGetChecked(ref dgParameter, "CHK");
                            if (ds.Length > 0)
                            {
                                DataTable dt = ds[0].Table;
                                foreach (DataRow data in ds)
                                {
                                    data.Delete();
                                }
                                dt.AcceptChanges();
                                Util.GridSetData(dgParameter, dt, FrameOperation, false);
                            }

                        }
                    });
                }
                else
                {
                    Util.MessageInfo("900");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnMakeParameter_Click(object sender, RoutedEventArgs e)
        {
            CheckParameter(txtQuery.Text.ToUpper(), true);
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CheckValidaton())
            {
                SaveData();
            }
        }

        private void cboUsePeriod_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (cboUsePeriod.SelectedIndex == 2)
            {
                dtpDate.IsEnabled = true;
                dtpDate.SelectedDateTime = DateTime.Today;
            }
            else
            {
                dtpDate.IsEnabled = false;
            }
        }

        private void popupPerson_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (selectedRequestUser == "SearchRequestUser")
                {
                    txtSearchUserName.Text = popup.USERNAME;
                    txtSearchUserName.Tag = popup.USERID;
                }
                else
                {
                    txtUserName.Text = popup.USERNAME;
                    txtUserName.Tag = popup.USERID;
                }
            }
        }

        private void dgParameter_CommittingEdit(object sender, C1.WPF.DataGrid.DataGridEndingEditEventArgs e)
        {
            switch (e.Column.Name)
            {
                case "PARA_NAME":
                    C1.WPF.C1TextBoxBase textBase = e.EditingElement as C1.WPF.C1TextBoxBase;
                    textBase.Text = textBase.Text.ToUpper();
                    break;
            }

        }

        private void dgList_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            try
            {
                DataRowView drv = dgList.SelectedItem as DataRowView;
                if (drv == null) return;

                xItem.DataContext = drv;
                if (xItem.DataContext != null)
                {
                    this.xItem.Visibility = Visibility.Visible;
                }

                txtQueryName.Tag = Util.NVC(drv["STORED_QRY_ID"]);

                txtUserName.Text = Util.NVC(drv["REQ_USER_NAME"]);
                txtUserName.Tag = Util.NVC(drv["REQ_USERID"]);

                dtpDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(drv["VLD_DATE"]));

                SearchParameterData(Util.NVC(drv["STORED_QRY_ID"]));
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataRowView drv = xItem.DataContext as DataRowView;

            if (drv != null)
            {

            }
        }

        #endregion

        #region Mehod
        private void ClearInputControl()
        {
            txtQueryName.Clear();
            txtQueryName.Tag = null;

            txtQueryDescription.Clear();

            cboUsePeriod.SelectedIndex = 0;
            dtpDate.IsEnabled = false;
            dtpDate.SelectedDateTime = DateTime.Today;

            txtUserName.Text = LoginInfo.USERNAME;
            txtUserName.Tag = LoginInfo.USERID;
            cboAuthority.SelectedIndex = 0;
            cboUseFlag.SelectedIndex = 0;
            txtQuery.Clear();

            // Parameter DataGrid Clear
            SearchParameterData("");

            btnAdd.IsEnabled = true;
            btnMakeParameter.IsEnabled = true;
            btnSave.IsEnabled = true;
            
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
            Util.gridClear(dgParameter);
        }

        private void SetComboCmmCode(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            if (cbo == cboSearchAuthority || cbo == cboAuthority)
            {
                drnewrow["CMCDTYPE"] = "QRY_SEARCH_AUTH_TYPE";
            }
            if (cbo == cboSearchUsePeriod || cbo == cboUsePeriod)
            {
                drnewrow["CMCDTYPE"] = "QRY_USE_PERIOD";
            }
            if (cbo == cboSearchUseFlag || cbo == cboUseFlag)
            {
                drnewrow["CMCDTYPE"] = "USE_FLAG";
            }
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (result.Rows.Count > 0)
                {
                    if (cbo == cboSearchUseFlag || cbo == cboUseFlag)
                    {

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 0;

                    }
                    else if (cbo == cboSearchAuthority)
                    {
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-ALL-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 0;
                    }
                    else if (cbo == cboAuthority)
                    {
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-SELECT-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 0;
                    }
                    else if (cbo == cboSearchUsePeriod)
                    {
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-ALL-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 0;
                    }
                    else if (cbo == cboUsePeriod)
                    {
                        DataRow dRow = result.NewRow();
                        dRow["CBO_NAME"] = "-SELECT-";
                        dRow["CBO_CODE"] = "";
                        result.Rows.InsertAt(dRow, 0);

                        cbo.ItemsSource = DataTableConverter.Convert(result);
                        cbo.SelectedIndex = 0;
                    }
                }
                else if (result.Rows.Count == 0)
                {
                    cbo.SelectedItem = null;
                }
            });
        }

        private void GetPersonInfo(object sender)
        {
            if (sender == null) return;

            string userName = string.Empty;
            selectedRequestUser = string.Empty;

            if (sender is TextBox)
            {
                var textbox = sender as TextBox;
                userName = textbox.Text;

                if (textbox.Name == "txtSearchUserName")
                {
                    selectedRequestUser = "SearchRequestUser";
                }
                else
                {
                    selectedRequestUser = "RequestUser";
                }
            }

            if (sender is Button)
            {
                var button = sender as Button;

                if (button.Name == "btnSearchUser")
                {
                    userName = txtSearchUserName.Text;
                    selectedRequestUser = "SearchRequestUser";
                }
                else
                {
                    userName = txtUserName.Text;
                    selectedRequestUser = "RequestUser";
                }
            }


            CMM_PERSON popupPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = userName;
            C1WindowExtension.SetParameters(popupPerson, parameters);

            popupPerson.Closed += new EventHandler(popupPerson_Closed);
            grdMain.Children.Add(popupPerson);
            popupPerson.BringToFront();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {

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

        private void SearchData(string StoredQryId)
        {
            try
            {
                ShowLoadingIndicator();

                InitGrid();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("REQ_USERID", typeof(string));
                IndataTable.Columns.Add("STORED_QRY_AUTH_CODE", typeof(string));
                IndataTable.Columns.Add("STORED_QRY_VLD_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("USE_FLAG", typeof(string));
                
                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["REQ_USERID"] = Util.NVC(txtSearchUserName.Tag).Equals(string.Empty) ? null : Util.NVC(txtSearchUserName.Tag);
                Indata["STORED_QRY_AUTH_CODE"] = Util.NVC(cboSearchAuthority.SelectedValue).Equals(string.Empty) ? null : Util.NVC(cboSearchAuthority.SelectedValue);
                Indata["STORED_QRY_VLD_TYPE_CODE"] = Util.NVC(cboSearchUsePeriod.SelectedValue).Equals(string.Empty) ? null : Util.NVC(cboSearchUsePeriod.SelectedValue);
                Indata["USE_FLAG"] = Util.NVC(cboSearchUseFlag.SelectedValue);

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SOM_STORED_QRY_DRB", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgList, result, FrameOperation, true);

                    if (!StoredQryId.Equals(string.Empty))
                    {
                        Util.gridFindDataRow(ref dgList, "STORED_QRY_ID", StoredQryId, true);
                    }

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
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

        private bool CheckValidaton()
        {
            if (Util.NVC(txtQueryName.Text).Equals(string.Empty))
            {
                // Query 명 (을)를 입력해 주세요.
                Util.MessageValidation("SFU8275", tbkQueryNameLabel.Text);
                return false;
            }

            if (Util.NVC(cboUsePeriod.SelectedValue).Equals(string.Empty))
            {
                // 사용기간 (을)를 선택하세요.
                Util.MessageValidation("SFU4925", tbkUsePeriodLabel.Text);
                return false;
            }

            if (Util.NVC(txtUserName.Text).Equals(string.Empty) || Util.NVC(txtUserName.Tag).Equals(string.Empty))
            {
                // 요청자 (을)를 입력해 주세요.
                Util.MessageValidation("SFU8275", tbkUserNameLabel.Text);
                return false;
            }

            if (Util.NVC(cboAuthority.SelectedValue).Equals(string.Empty))
            {
                // 조회권한 (을)를 선택하세요.
                Util.MessageValidation("SFU4925", tbkAuthorityLabel.Text);
                return false;
            }

            if (Util.NVC(cboUseFlag.SelectedValue).Equals(string.Empty))
            {
                // 사용여부 (을)를 선택하세요.
                Util.MessageValidation("SFU4925", tbkUseFlagLabel.Text);
                return false;
            }

            if (Util.NVC(txtQuery.Text).Equals(string.Empty))
            {
                // 쿼리 내용 (을)를 입력해 주세요.
                Util.MessageValidation("SFU8275", "QRY_CNTT");
                return false;
            }
            

            if (!CheckSelect(Util.NVC(txtQuery.Text).ToUpper()))
            {
                return false;
            }

            if (!CheckParameter(Util.NVC(txtQuery.Text).ToUpper(), false))
            {
                return false;
            }


            return true;
        }
        
        private bool CheckSelect(string query)
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
            noQueryToken.Add("ALTER");
            noQueryToken.Add("TRUNCATE");
            noQueryToken.Add("INSERT");
            noQueryToken.Add("UPDATE");
            noQueryToken.Add("DELETE");
            noQueryToken.Add("GRANT");
            noQueryToken.Add("REVOKE");
            noQueryToken.Add("CONNECT");
            noQueryToken.Add("RENAME");

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

        private bool CheckParameter(string query, bool isMakeParameter)
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

            // @Parameter 추출
            List<string> parameters = new List<string>();
            string[] tokens = lastQuery.Split(new string[12] { " ", ",", "(", ")", "+", "-", "*", "/", "=", "'", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in tokens)
            {
                if (item.Trim().First().Equals('@'))
                {
                    if (!parameters.Contains(item.Trim())) parameters.Add(item.Trim());
                }
            }

            // 선언 변수 찾기
            List<string> removeParameters = new List<string>();
            tokens = lastQuery.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in tokens)
            {
                if (item.Trim().Length > 7 && item.Trim().Substring(0, 7).ToUpper().Equals("DECLARE"))
                {
                    string[] tokens2 = item.Trim().Split(new string[2] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item2 in tokens2)
                    {
                        if (item2.Trim().First().Equals('@'))
                        {
                            if (!removeParameters.Contains(item2.Trim())) removeParameters.Add(item2.Trim());
                        }
                    }
                }
            }

            // 선언 변수 제거
            foreach (string item in removeParameters)
            {
                if (parameters.Contains(item))
                {
                    parameters.Remove(item);
                }
            }

            // Parameter 목록 비교
            List<string> noParameters = new List<string>();
            foreach (string param in parameters)
            {
                if (DataTableConverter.Convert(dgParameter.ItemsSource).AsEnumerable()
                    .Where(x => x.Field<string>("PARA_NAME").Equals(param.Replace("@", ""))).Count().Equals(0))
                {
                    if (!noParameters.Contains(param)) noParameters.Add(param);
                }
            }

            if (noParameters.Count > 0)
            {
                if (isMakeParameter)
                {
                    DataTable dt = ((DataView)dgParameter.ItemsSource).Table;

                    foreach (string item in noParameters)
                    {
                        DataRow dr = dt.NewRow();
                        dr["CHK"] = false;
                        dr["PARA_NAME"] = item.Replace("@", "").Trim().ToUpper();
                        dr["PARA_NOTE"] = string.Empty;
                        dr["DFLT_VALUE"] = string.Empty;
                        dr["MAND_FLAG"] = false;
                        dt.Rows.Add(dr);
                    }
                    dt.AcceptChanges();

                    return true;
                }
                else
                {
                    Util.MessageInfo("SFU8290", "파라미터", noParameters.Aggregate((i, j) => i + ", " + j));
                    return false;
                }                
            }

            return true;
        }

        private string ConvertRunQuery(string query)
        {
            string lastQuery = query.ToUpper();

            //lastQuery = "SET ROWCOUNT 1;\r\n\r\n" + lastQuery;

            DataTable dt = ((DataView)dgParameter.ItemsSource).Table;
            foreach (DataRow dr in dt.Rows)
            {
                lastQuery = lastQuery.Replace("@" + Util.NVC(dr["PARA_NAME"]).ToUpper(), "'" + Util.NVC(dr["DFLT_VALUE"]).ToUpper() + "'");
            }

            //lastQuery = lastQuery + "\r\n\r\nSET ROWCOUNT 0;";
            lastQuery = lastQuery + "\r\n\r\nFETCH FIRST 1 ROW ONLY";

            return lastQuery;
        }
        
        private void SaveData()
        {
            try
            {
                btnSave.IsEnabled = false;

                ShowLoadingIndicator();

                
                DataSet ds = new DataSet();

                // 쿼리 데이터
                DataTable inQryDataTable = ds.Tables.Add("QRYDATA");
                inQryDataTable.Columns.Add("STORED_QRY_ID", typeof(string));
                inQryDataTable.Columns.Add("SYSID", typeof(string));
                inQryDataTable.Columns.Add("SHOPID", typeof(string));
                inQryDataTable.Columns.Add("AREAID", typeof(string));
                inQryDataTable.Columns.Add("QRY_NAME", typeof(string));
                inQryDataTable.Columns.Add("NOTE", typeof(string));
                inQryDataTable.Columns.Add("REQ_USERID", typeof(string));
                inQryDataTable.Columns.Add("STORED_QRY_AUTH_CODE", typeof(string));
                inQryDataTable.Columns.Add("STORED_QRY_VLD_TYPE_CODE", typeof(string));
                inQryDataTable.Columns.Add("VLD_DATE", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT01", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT02", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT03", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT04", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT05", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT06", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT07", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT08", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT09", typeof(string));
                inQryDataTable.Columns.Add("QRY_CNTT10", typeof(string));
                inQryDataTable.Columns.Add("USE_FLAG", typeof(string));
                inQryDataTable.Columns.Add("INSUSER", typeof(string));
                inQryDataTable.Columns.Add("UPDUSER", typeof(string));

                DataRow newRow = inQryDataTable.NewRow();
                if (!Util.NVC(txtQueryName.Tag).Equals(string.Empty))
                {
                    newRow["STORED_QRY_ID"] = Util.NVC(txtQueryName.Tag);
                }
                else
                {
                    newRow["STORED_QRY_ID"] = string.Empty;
                }

                newRow["SYSID"] = LoginInfo.SYSID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["QRY_NAME"] = txtQueryName.Text;
                newRow["NOTE"] = txtQueryDescription.Text;
                newRow["REQ_USERID"] = txtUserName.Tag.ToString();
                newRow["STORED_QRY_AUTH_CODE"] = Util.NVC(cboAuthority.SelectedValue);
                newRow["STORED_QRY_VLD_TYPE_CODE"] = Util.NVC(cboUsePeriod.SelectedValue);
                newRow["VLD_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                
                for (int cnt = 0; cnt < 10; cnt++)
                {
                    if (txtQuery.Text.Length < (cnt * 3500) + 3500)
                    {
                        newRow["QRY_CNTT" + (cnt + 1).ToString("00")] = txtQuery.Text.Substring(cnt * 3000, txtQuery.Text.Length - (cnt * 3000));
                        break;
                    }
                    else
                    {
                        newRow["QRY_CNTT" + (cnt + 1).ToString("00")] = txtQuery.Text.Substring(cnt * 3000, 3000);
                    }

                }

                newRow["USE_FLAG"] = Util.NVC(cboUseFlag.SelectedValue);
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["UPDUSER"] = LoginInfo.USERID;

                inQryDataTable.Rows.Add(newRow);


                // 파라미터 데이터
                DataTable inPrmDataTable = ds.Tables.Add("PRMDATA");
                inPrmDataTable.Columns.Add("PARA_SEQNO", typeof(string));
                inPrmDataTable.Columns.Add("PARA_NAME", typeof(string));
                inPrmDataTable.Columns.Add("PARA_NOTE", typeof(string));
                inPrmDataTable.Columns.Add("DFLT_VALUE", typeof(string));
                inPrmDataTable.Columns.Add("MAND_FLAG", typeof(string));

                int index = 0;
                DataRow[] drSelect = DataTableConverter.Convert(dgParameter.ItemsSource).Select();
                foreach (DataRow dr in drSelect)
                {
                    index++;
                    DataRow drNew = inPrmDataTable.NewRow();
                    drNew["PARA_SEQNO"] = index.ToString();
                    drNew["PARA_NAME"] = dr["PARA_NAME"];
                    drNew["PARA_NOTE"] = dr["PARA_NOTE"];
                    drNew["DFLT_VALUE"] = dr["DFLT_VALUE"];
                    drNew["MAND_FLAG"] = Util.NVC(dr["MAND_FLAG"]).Equals("1") || Util.NVC(dr["MAND_FLAG"]).Equals("True") ? "Y" : "N"; // 2024.11.05. 김영국 - MAND_FLAG값이 Boolean Type 으로 넘어오는 문제점 발생으로 Type수정.
                    inPrmDataTable.Rows.Add(drNew);
                }

                // SQL 체크 데이터
                DataTable dtRqst = ds.Tables.Add("CHKDATA");
                dtRqst.Columns.Add("SQL_QUERY", typeof(string));

                DataRow drNewChk = dtRqst.NewRow();
                drNewChk["SQL_QUERY"] = ConvertRunQuery(Util.NVC(txtQuery.Text));
                dtRqst.Rows.Add(drNewChk);

                string xml = ds.GetXml();

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_SOM_STORED_QRY_DRB", "QRYDATA,PRMDATA,CHKDATA", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            //Util.MessageException(bizException);
                            Util.MessageValidation(bizException.Message);
                            return;
                        }

                        if (bizResult.Tables.Contains("OUTDATA") && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            SearchData(Util.NVC(bizResult.Tables["OUTDATA"].Rows[0]["STORED_QRY_ID"]));                            
                        }

                        //2021-02-03  조영대 : 저장 후 입력 컨트롤 클리어
                        ClearInputControl();

                        Util.AlertInfo("SFU1270");  //저장되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        btnSave.IsEnabled = true;
                    }
                }, ds);
                
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }



        #endregion

 
    }
}
