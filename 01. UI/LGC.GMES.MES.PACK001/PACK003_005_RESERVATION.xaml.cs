/*************************************************************************************
 Created Date : 2020.09.21
      Creator : 김길용
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.21  김길용           Initialize
  2020.12.16  정용석           UI 내용 수정
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_005_RESERVATION : C1Window, IWorkArea
    {
        #region "Member Variable & Constructor"
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_005_RESERVATION()
        {
            InitializeComponent();
        }
        #endregion

        #region "Member Function Lists..."
        private DataTable queryToDataTable(IEnumerable<dynamic> records)
        {
            DataTable dt = new DataTable();
            var firstRow = records.FirstOrDefault();
            if (firstRow == null)
            {
                return null;
            }

            PropertyInfo[] propertyInfos = firstRow.GetType().GetProperties();
            foreach (var propertyinfo in propertyInfos)
            {
                Type propertyType = propertyinfo.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    dt.Columns.Add(propertyinfo.Name, Nullable.GetUnderlyingType(propertyType));
                }
                else
                {
                    dt.Columns.Add(propertyinfo.Name, propertyinfo.PropertyType);
                }
            }

            foreach (var record in records)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    dr[i] = propertyInfos[i].GetValue(record) != null ? propertyInfos[i].GetValue(record) : DBNull.Value;
                }

                dt.Rows.Add(dr);
            }

            dt.AcceptChanges();
            return dt;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeCombo()
        {
            try
            {
                C1ComboBox nullCombo = new C1ComboBox();
                CommonCombo _combo = new CommonCombo();
                string[] filterFromEQSGID = { LoginInfo.CFG_AREA_ID, "Y", "" };
                _combo.SetCombo(cboFromEQSGID, CommonCombo.ComboStatus.ALL, sFilter: filterFromEQSGID, sCase: "LOGIS_EQSG_FOR_MEB");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationCheck()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(this.dgStockerLoadedList.ItemsSource);

                // 선택된거 없으면 Interlock
                var checkedData = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                if (checkedData.Count() <= 0)
                {
                    Util.MessageInfo("SFU1651");    // 선택된 항목이 없습니다.
                    this.dgStockerLoadedList.Focus();
                    return false;
                }

                // 변경 라인 Change ComboBox가 Disabled이면 Interlock
                if (!this.cboChangeRoute.IsEnabled)
                {
                    Util.MessageInfo("SFU1223");
                    this.cboChangeRoute.Focus();
                    return false;
                }

                if (string.IsNullOrEmpty(this.cboChangeRoute.SelectedValue.ToString()))
                {
                    Util.MessageInfo("SFU1223");
                    this.cboChangeRoute.Focus();
                    return false;
                }

                if (string.IsNullOrEmpty(txtNote.Text.ToString()))
                {
                    Util.MessageInfo("SFU1594");
                    this.txtNote.Focus();
                    return false;
                }

                if (ucPersonInfo.UserID == null || string.IsNullOrEmpty(ucPersonInfo.UserID.ToString()))
                {
                    Util.MessageInfo("SFU1843");
                    this.ucPersonInfo.Focus();
                    return false;
                }

                //// 선택된 항목의 라인과 변경하려는 라인콤보박스의 현재값과 비교해서 둘이 동일한 것이 있다면 Interlock
                //var checkLineData = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE") && x.Field<string>("EQSGID").Equals(this.cboChangeRoute.SelectedValue.ToString()));
                //if (checkLineData.Count() <= 0)
                //{
                //    Util.MessageInfo("SFU5168");    // 선택된 PALLET중에 이동할 라인[%1]과 동일한 라인에 투입될 PALLET가 존재합니다.
                //    this.dgStockerLoadedList.Focus();
                //    return false;
                //}
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private DataTable SetInputData()
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("CSTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQSGID"] = string.IsNullOrEmpty(cboFromEQSGID.SelectedValue.ToString()) ? null : cboFromEQSGID.SelectedValue.ToString();
            dr["PRODID"] = this.txtProdID.Text;
            dr["CSTID"] = this.txtCSTID.Text;
            dt.Rows.Add(dr);
            return dt;
        }

        private void SearchProcess()
        {
            try
            {
                Util.gridClear(this.dgStockerLoadedList);
                string bizRuleName = "DA_PRD_SEL_PLT_FOR_MOVE";
                DataTable dtRQSTDT = this.SetInputData();

                new ClientProxy().ExecuteService(bizRuleName, dtRQSTDT.TableName, "RSLTDT", dtRQSTDT, (dtResult, ex) =>
                {
                    ShowLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        HiddenLoadingIndicator();
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgStockerLoadedList, dtResult, FrameOperation);
                    }
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool LineChangeProcess()
        {
            try
            {
                if (!ValidationCheck())
                {
                    return false;
                }

                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("MODLID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("ROUTID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["MODLID"] = null;
                drINDATA["EQSGID"] = null;
                drINDATA["ROUTID"] = this.cboChangeRoute.SelectedValue.ToString();
                drINDATA["USERID"] = this.ucPersonInfo.UserID;
                dtINDATA.Rows.Add(drINDATA);

                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRCV_ISS.Columns.Add("PALLETID", typeof(string));

                DataTable dt = DataTableConverter.Convert(this.dgStockerLoadedList.ItemsSource);
                var checkedData = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                foreach (var item in checkedData)
                {
                    drRCV_ISS = dtRCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = item.Field<string>("RCV_ISS_ID").ToString();
                    drRCV_ISS["PALLETID"] = item.Field<string>("BOXID").ToString();
                    dtRCV_ISS.Rows.Add(drRCV_ISS);
                }

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add(dtINDATA);
                indataSet.Tables.Add(dtRCV_ISS);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PRODUCT_EDIT_PACK", "INDATA,RCV_ISS", "OUTDATA", indataSet, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return true;
        }

        private void SetLineMoveControlEnabled()
        {
            // ALL이 선택되어 있으면 라인변경 관련 Control Disable
            // 그렇지 않으면 라인변경 관련 Control Enable
            if (string.IsNullOrEmpty(this.cboFromEQSGID.SelectedValue.ToString()))
            {
                this.SetLineMoveControlEnabled(false);
            }
            else
            {
                this.SetLineMoveControlEnabled(true);
            }
        }

        private void SetLineMoveControlEnabled(bool isEnabled = true)
        {
            this.cboChangeRoute.IsEnabled = isEnabled;
            this.txtNote.IsEnabled = isEnabled;
            this.btnRequest.IsEnabled = isEnabled;
        }

        private void ChangeRouteComboDataBinding()
        {
            try
            {
                int checkedIndex = Util.gridFindDataRow(ref this.dgStockerLoadedList, "CHK", "True", false);
                if (checkedIndex == -1)
                {
                    return;
                }

                object objEQSGID = DataTableConverter.GetValue(this.dgStockerLoadedList.Rows[checkedIndex].DataItem, "EQSGID");
                object objPRODID = DataTableConverter.GetValue(this.dgStockerLoadedList.Rows[checkedIndex].DataItem, "PRODID");
                if (string.IsNullOrEmpty(Util.NVC(objEQSGID)) || string.IsNullOrEmpty(Util.NVC(objEQSGID)))
                {
                    return;
                }

                // Validation Check...
                DataTable dtMoveData = this.ValidationCheckLineMoveData();
                if (dtMoveData == null)
                {
                    this.cboChangeRoute.ItemsSource = null;
                    return;
                }

                // DA 호출
                string bizRuleName = "DA_SEL_MOVE_LINE_FOR_LOGIS_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));

                foreach (DataRow drMoveData in dtMoveData.Rows)
                {
                    DataRow drRQSTDT = dtRQSTDT.NewRow();
                    drRQSTDT["LANGID"] = LoginInfo.LANGID;
                    drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drRQSTDT["EQSGID"] = drMoveData["EQSGID"].ToString();
                    drRQSTDT["PRODID"] = drMoveData["PRODID"].ToString();
                    dtRQSTDT.Rows.Add(drRQSTDT);
                }

                new ClientProxy().ExecuteService(bizRuleName, dtRQSTDT.TableName, "RSLTDT", dtRQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        this.cboChangeRoute.ItemsSource = DataTableConverter.Convert(dtResult);
                        this.cboChangeRoute.SelectedIndex = dtResult.Rows.Count - 1;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChangeRouteComboDataUnBinding()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(this.dgStockerLoadedList.ItemsSource);
                var checkCountQuery = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                if (checkCountQuery.Count() <= 0)
                {
                    this.cboChangeRoute.SelectedValue = string.Empty;
                    this.cboChangeRoute.ItemsSource = null;
                    return;
                }
                this.ChangeRouteComboDataBinding();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable ValidationCheckLineMoveData()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(this.dgStockerLoadedList.ItemsSource);
                var checkCountQuery = dt.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE"));
                if(checkCountQuery.Count() <= 0)
                {
                    return null;
                }

                // 라인이동 대상 선택 Validation
                // 선택된 EQSGID, PRODID별로 GROUP BY 해서 건수가 2건 이상 나오면 Interlock
                var checkQuery = from d1 in dt.AsEnumerable()
                                 where d1.Field<string>("CHK").ToUpper().Equals("TRUE")
                                 group d1 by new
                                 {
                                     EQSGID = d1.Field<string>("EQSGID"),
                                     PRODID = d1.Field<string>("PRODID")
                                 } into grp
                                 select new
                                 {
                                     EQSGID = grp.Key.EQSGID,
                                     PRODID = grp.Key.PRODID
                                 };

                if (checkQuery.Count() >= 2)
                {
                    Util.MessageInfo("SFU4338");    // 동일한 제품만 작업 가능합니다.
                    this.dgStockerLoadedList.Focus();
                    return null;
                }

                return this.queryToDataTable(checkQuery);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region "Events..."
        private void C1Window_Initialized(object sender, EventArgs e)
        {
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                InitializeCombo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            // 변경하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2875"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    if(!this.LineChangeProcess())
                    {
                        return;
                    }
                    Util.MessageInfo("SFU1166");    // 변경되었습니다.
                    this.SearchProcess();
                }
            });
        }

        private void cboFromEQSGID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.SearchProcess();
            this.ChangeRouteComboDataUnBinding();
            this.SetLineMoveControlEnabled();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.ChangeRouteComboDataBinding();
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ChangeRouteComboDataUnBinding();
        }
        #endregion
    }
}