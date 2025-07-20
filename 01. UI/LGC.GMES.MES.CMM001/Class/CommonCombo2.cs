/*************************************************************************************
 Created Date : 2021.01.27
      Creator : 
   Description : LG Enseol UI 에서 사용할 Combo 용 클래스, 
                 CommonCombo.cs 기능 복사
                 운영에 배포시 수정 사항에 대하여 정상 배포되지 않는 현상으로 2021.01.01부터 수정 사항을 CommonCombo에서 CommonCombo2로 이동
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.27 손우석 : Initial Created.
**************************************************************************************/
using System;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Class
{
    public class CommonCombo2
    {
        public CommonCombo2() { }

        #region ComboStatus
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
            /// 바인딩 후 선택 안해도 될경우(선택 안해도 되는 콤보일때 사용)
            /// </summary>
            NA,

            /// <summary>
            /// 바인딩만 하고 끝 (바인딩후 제일 1번째 항목을 표시) 
            /// </summary>
            NONE
        }

        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        #endregion ComboStatus

        #region Event
        private void Cb_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cb = sender as C1ComboBox;
            Hashtable hashCbo = cb.Tag as Hashtable;

            C1ComboBox[] cbChildArray = hashCbo["child_cbo"] as C1ComboBox[];

            if (cb.SelectedValue != null)
            {
                CommonCombo _combo = new CMM001.Class.CommonCombo();

                foreach (C1ComboBox cbChild in cbChildArray)
                {
                    _combo.SetCombo(cbChild);
                }
            }
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            C1ComboBox cb = sender as C1ComboBox;
            Hashtable hashCbo = cb.Tag as Hashtable;

            C1ComboBox[] cbChildArray = hashCbo["child_cbo"] as C1ComboBox[];

            if (cb.SelectedValue != null)
            {
                CommonCombo _combo = new CMM001.Class.CommonCombo();

                foreach (C1ComboBox cbChild in cbChildArray)
                {
                    _combo.SetCombo(cbChild);
                }
            }
        }
        #endregion Event

        public void SetComboObjParent(C1ComboBox cb, ComboStatus cs, C1ComboBox[] cbChild = null, object[] objParent = null, String[] sFilter = null, String sCase = null)
        {
            Hashtable hashTag = new Hashtable();
            if (sCase == null)
            {
                hashTag.Add("combo_case", cb.Name);
            }
            else
            {
                hashTag.Add("combo_case", sCase);
            }
            hashTag.Add("all_status", cs);
            hashTag.Add("child_cbo", cbChild);
            hashTag.Add("parent_obj", objParent);
            hashTag.Add("filter", sFilter);
            cb.Tag = hashTag;

            SetCombo(cb);

            if (hashTag.Contains("child_cbo") && hashTag["child_cbo"] != null)
            {

                cb.SelectedItemChanged -= Cb_SelectedItemChanged;
                cb.SelectedItemChanged += Cb_SelectedItemChanged;
            }
        }


        public void SetCombo(C1ComboBox cb, ComboStatus cs, C1ComboBox[] cbChild = null, C1ComboBox[] cbParent = null, String[] sFilter = null, String sCase = null)
        {
            Hashtable hashTag = new Hashtable();
            if (sCase == null)
            {
                hashTag.Add("combo_case", cb.Name);
            }
            else
            {
                hashTag.Add("combo_case", sCase);
            }
            hashTag.Add("all_status", cs);
            hashTag.Add("child_cbo", cbChild);
            hashTag.Add("parent_cbo", cbParent);
            hashTag.Add("filter", sFilter);
            cb.Tag = hashTag;

            SetCombo(cb);

            if (hashTag.Contains("child_cbo") && hashTag["child_cbo"] != null)
            {
                cb.SelectedItemChanged -= Cb_SelectedItemChanged;
                cb.SelectedItemChanged += Cb_SelectedItemChanged;
            }
        }

        public void SetCombo(C1ComboBox cb)
        {
            try
            {
                Hashtable hashTag = cb.Tag as Hashtable;
                ComboStatus cs = (ComboStatus)Enum.Parse(typeof(ComboStatus), hashTag["all_status"].ToString());
                String[] sFilter = new String[10];


                if (hashTag.Contains("parent_cbo") && hashTag["parent_cbo"] != null)
                {
                    C1ComboBox[] cbParentArray = hashTag["parent_cbo"] as C1ComboBox[];
                    int i = 0;
                    for (i = 0; i < cbParentArray.Length; i++)
                    {
                        if (cbParentArray[i].SelectedValue != null)
                        {
                            sFilter[i] = cbParentArray[i].SelectedValue.ToString();
                        }
                        else
                        {
                            sFilter[i] = "";
                        }
                    }

                    if (hashTag.Contains("filter") && hashTag["filter"] != null)
                    {
                        String[] sFilter1 = hashTag["filter"] as String[];
                        foreach (string s in sFilter1)
                        {
                            sFilter[i] = s;
                            i++;
                        }
                    }
                }
                else if (hashTag.Contains("parent_obj") && hashTag["parent_obj"] != null)
                {
                    object[] objParentArray = hashTag["parent_obj"] as object[];
                    int i = 0;
                    for (i = 0; i < objParentArray.Length; i++)
                    {
                        switch (objParentArray[i].GetType().Name)
                        {
                            case "LGCDatePicker":
                                LGCDatePicker lgcDp = objParentArray[i] as LGCDatePicker;
                                if (lgcDp.DatepickerType.ToString().Equals("Month"))
                                {
                                    sFilter[i] = lgcDp.SelectedDateTime.ToString("yyyyMM");
                                }
                                else
                                {
                                    sFilter[i] = lgcDp.SelectedDateTime.ToString("yyyyMMdd");
                                }
                                break;
                            case "C1ComboBox":
                                C1ComboBox cbObj = objParentArray[i] as C1ComboBox;

                                if (cbObj.SelectedValue != null)
                                {
                                    sFilter[i] = cbObj.SelectedValue.ToString();
                                }
                                else
                                {
                                    sFilter[i] = "";
                                }
                                break;
                            case "TextBox":
                                TextBox tb = objParentArray[i] as TextBox;
                                sFilter[i] = tb.Text;
                                break;
                        }
                    }

                    if (hashTag.Contains("filter") && hashTag["filter"] != null)
                    {
                        String[] sFilter1 = hashTag["filter"] as String[];
                        foreach (string s in sFilter1)
                        {
                            sFilter[i] = s;
                            i++;
                        }

                    }
                }
                else if (hashTag.Contains("filter") && hashTag["filter"] != null)
                {
                    sFilter = hashTag["filter"] as String[];
                }
                else
                {
                    sFilter[0] = "";
                }

                switch (hashTag["combo_case"].ToString())
                {
                    case "PROCESS_PCSGID_V":
                        SetProcessPCSGID_V(cb, cs, sFilter);
                        break;
                    case "BLOCK_REVNO":
                        setComboBlockRevNo(cb, cs, sFilter);
                        break;
                    default:
                        SetDefaultCbo(cb, cs);
                        break;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region Method
        private void SetDefaultCbo(C1ComboBox cbo, ComboStatus cs)
        {
            try
            {
                DataTable dtResult = new DataTable();

                dtResult.Columns.Add("CBO_CODE", typeof(string));
                dtResult.Columns.Add("CBO_NAME", typeof(string));

                DataRow newRow = dtResult.NewRow();

                newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { "NA", "구현안됨" };
                dtResult.Rows.Add(newRow);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetProcessPCSGID_V(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_PCSGID_VERIFI", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }

                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void setComboBlockRevNo(C1ComboBox cbo, ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = string.IsNullOrEmpty(Util.NVC(sFilter[0])) ? LoginInfo.CFG_AREA_ID : sFilter[0];

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_QMS_BLOCK_BAS_REVNO_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #endregion Method
    }
}