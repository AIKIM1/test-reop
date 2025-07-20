/*************************************************************************************
 Created Date : 2022.12.29
      Creator : 주동석
   Decription : Tool 타입별 컬럼 관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.29  주동석 : 신규 생성

**************************************************************************************/


using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;
using System.Windows.Media;
using System.Globalization;

using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Linq;

using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_081.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_318 : UserControl
    {
        #region Private 변수
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        DataTable dtHistList = new DataTable();
        string searchFlag = "N";

        int addRows;
        #endregion

        #region Form Load & Init Control
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_318()
        {
            InitializeComponent();
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        #endregion

        #region Events
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SelectToolType();
        }

        private void SelectToolType()
        {
            try
            {
                if (Util.NVC(cboToolTypeCode.SelectedValue).Equals("SELECT"))
                {
                    //공구 유형을 선택 하세요.
                    Util.MessageValidation("SFU6056");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("TOOL_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("TOOL_TYPE_ATTR_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TOOL_TYPE_CODE"] = cboToolTypeCode.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_AREA_TOOL_ATTR_MNGT", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgToolTypeAttr);

                dgToolTypeAttr.BeginEdit();

                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("CMCODE", typeof(string));

                DataRow row = inData.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["CMCODE"] = Util.NVC(cboToolTypeCode.SelectedValue);

                inData.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_COMMONCODE_TOOL_TYPE_CODE_ATTR_CBO", "RQSTDT", "RSLTDT", inData);

                (dgToolTypeAttr.Columns["TOOL_TYPE_ATTR_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

                if (Result.Rows.Count >= 1)
                {
                    dgToolTypeAttr.ItemsSource = DataTableConverter.Convert(Result);
                }

                dgToolTypeAttr.EndEdit();

                searchFlag = "Y";
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #region Functions
        void Init()
        {
            dgToolTypeAttr.ItemsSource = null;

            //CommonCombo combo = new CommonCombo();

            //string[] sFilter = { "RTN_STAT_CODE" };
            //combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //cboStat.SelectedValue = "RETURN_CONFIRM";
        }

        private void InitCombo()
        {
            CommonCombo cbo = new CommonCombo();

            /*============= 등록 Tool 목록 =============*/
            String[] sFilterT = { "", "TOOL_TYPE_CODE" };
            // Tool유형
            cbo.SetCombo(cb: cboToolTypeCode, cs: CommonCombo.ComboStatus.SELECT, sFilter: sFilterT, sCase: "COMMCODES");

            ////String[] sFilterT2 = { "" };
            ////cbo.SetCombo(cboToolTypeCode, cs: CommonCombo.ComboStatus.ALL, sFilter: sFilterT2, sCase: "TOOL_TYPE_CODE_ATTR_CBO");
        }

        #endregion Functions

        #region TextBox Event
        #endregion

        #region METHOD

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgToolTypeAttr.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dt = DataTableConverter.Convert(dgToolTypeAttr.ItemsSource);

                        DataRow[] drChk = dt.Select("CHK = 'True'");

                        //DataRow[] drChk = Util.gridGetChecked(ref dgToolTypeAttr, "CHK");

                        if (drChk.Length == 0)
                        {
                            Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("RQSTDT");
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("TOOL_TYPE_CODE", typeof(string));
                        inData.Columns.Add("TOOL_TYPE_ATTR_CODE", typeof(string));
                        inData.Columns.Add("TOOL_TYPE_ATTR_PSTN", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = null;

                        for (int i = 0; i < drChk.Length; i++)
                        {
                            row = inData.NewRow();
                            row["AREAID"] = LoginInfo.CFG_AREA_ID;
                            row["TOOL_TYPE_CODE"] = Util.NVC(drChk[i]["TOOL_TYPE_CODE"]);
                            row["TOOL_TYPE_ATTR_CODE"] = Util.NVC(drChk[i]["TOOL_TYPE_ATTR_CODE"]);
                            row["TOOL_TYPE_ATTR_PSTN"] = Util.NVC(drChk[i]["TOOL_TYPE_ATTR_PSTN"]);
                            row["USERID"] = LoginInfo.USERID;

                            indataSet.Tables["RQSTDT"].Rows.Add(row);
                        }

                        new ClientProxy().ExecuteService_Multi("DA_BAS_REG_TB_SFC_AREA_TOOL_ATTR_MNGT", "RQSTDT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                else
                                {
                                    //dgToolTypeAttr.ItemsSource = null;

                                    //정상 처리 되었습니다.
                                    Util.Alert("SFU1275");

                                    //DeleteRows();
                                    SelectToolType();

                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );

                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgToolTypeAttr.GetRowCount() == 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                //삭제하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dt = DataTableConverter.Convert(dgToolTypeAttr.ItemsSource);

                        DataRow[] drChk = dt.Select("CHK = 'True'");

                        //DataRow[] drChk = Util.gridGetChecked(ref dgToolTypeAttr, "CHK");

                        if (drChk.Length == 0)
                        {
                            Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("RQSTDT");
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("TOOL_TYPE_CODE", typeof(string));
                        inData.Columns.Add("TOOL_TYPE_ATTR_CODE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = null;

                        for (int i = 0; i < drChk.Length; i++)
                        {
                            row = inData.NewRow();
                            row["AREAID"] = LoginInfo.CFG_AREA_ID;
                            row["TOOL_TYPE_CODE"] = Util.NVC(drChk[i]["TOOL_TYPE_CODE"]);
                            row["TOOL_TYPE_ATTR_CODE"] = Util.NVC(drChk[i]["TOOL_TYPE_ATTR_CODE"]);

                            indataSet.Tables["RQSTDT"].Rows.Add(row);
                        }

                        new ClientProxy().ExecuteService_Multi("DA_BAS_DEL_TB_SFC_AREA_TOOL_ATTR_MNGT", "RQSTDT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                else
                                {
                                    //dgToolTypeAttr.ItemsSource = null;

                                    //정상 처리 되었습니다.
                                    Util.Alert("SFU1275");

                                    //DeleteRows();
                                    SelectToolType();

                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, indataSet
                        );

                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGrid01RowAdd(dgToolTypeAttr);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGrid01RowDelete(dgToolTypeAttr);
        }

        private void DataGrid01RowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (Util.NVC(cboToolTypeCode.SelectedValue).Equals("SELECT"))
                {
                    //공구 유형을 선택 하세요.
                    Util.MessageValidation("SFU6030");
                    return;
                }

                if (searchFlag == "N")
                {
                    // 조회를 먼저 해주세요.
                    Util.MessageValidation("SFU8537");
                    return;
                }

                // 여러건 추가 시 안되는 부분 확인
                DataTable dt = new DataTable();
                //if (rowCount.Value != 0)
                if (Math.Abs(rowCount.Value) > 0)
                {
                    if (rowCount.Value + dg.Rows.Count > 576)
                    {
                        // 최대 ROW수는 576입니다.
                        Util.MessageValidation("SFU4264");
                        return;
                    }
                    else
                    {
                        addRows = int.Parse(rowCount.Value.GetString());
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dr2["CHK"] = true;
                        dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr2["TOOL_TYPE_CODE"] = Util.NVC(cboToolTypeCode.SelectedValue);
                        dr2["TOOL_TYPE_NAME"] = Util.NVC(cboToolTypeCode.Text);
                        dr2["TOOL_TYPE_ATTR_PSTN"] = dgToolTypeAttr.GetRowCount() + 1;

                        dt.Rows.Add(dr2);

                        dg.BeginEdit();

                        DataTable inData = new DataTable();
                        inData.Columns.Add("LANGID", typeof(string));
                        inData.Columns.Add("CMCODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["LANGID"] = LoginInfo.LANGID;
                        row["CMCODE"] = Util.NVC(cboToolTypeCode.SelectedValue);

                        inData.Rows.Add(row);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_COMMONCODE_TOOL_TYPE_CODE_ATTR_CBO", "RQSTDT", "RSLTDT", inData);

                        (dg.Columns["TOOL_TYPE_ATTR_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

                        dg.ItemsSource = DataTableConverter.Convert(dt);


                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        DataRow dr2 = dt.NewRow();
                        dr2["CHK"] = true;
                        dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr2["TOOL_TYPE_CODE"] = Util.NVC(cboToolTypeCode.SelectedValue);
                        dr2["TOOL_TYPE_NAME"] = Util.NVC(cboToolTypeCode.Text);
                        dr2["TOOL_TYPE_ATTR_PSTN"] = dgToolTypeAttr.GetRowCount() + 1;
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();

                        dg.BeginEdit();

                        DataTable inData = new DataTable();
                        inData.Columns.Add("LANGID", typeof(string));
                        inData.Columns.Add("CMCODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["LANGID"] = LoginInfo.LANGID;
                        row["CMCODE"] = Util.NVC(cboToolTypeCode.SelectedValue);

                        inData.Rows.Add(row);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_COMMONCODE_TOOL_TYPE_CODE_ATTR_CBO", "RQSTDT", "RSLTDT", inData);

                        (dg.Columns["TOOL_TYPE_ATTR_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

                        dg.ItemsSource = DataTableConverter.Convert(dt);


                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void DataGrid01RowDelete(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = ((DataView)dg.ItemsSource).Table;

                if (dg.Rows.Count > 0)
                {
                    if (dg.SelectedIndex > -1)
                    {
                        if (Math.Abs(rowCount.Value) > 0)
                        {
                            if (rowCount.Value + dg.Rows.Count > 576)
                            {
                                // 최대 ROW수는 576입니다.
                                Util.MessageValidation("SFU4264");
                                return;
                            }
                            else
                            {
                                addRows = int.Parse(rowCount.Value.GetString());
                            }
                        }

                        if (addRows > 1)
                        {
                            int iSelectIndex = dg.SelectedIndex;

                            for (int i = 0; i < addRows; i++)
                            {
                                dt.Rows[--iSelectIndex].Delete();
                                dg.BeginEdit();
                                dg.ItemsSource = DataTableConverter.Convert(dt);
                                dg.EndEdit();
                            }
                        }
                        else
                        {
                            dt.Rows[dg.SelectedIndex].Delete();
                            dg.BeginEdit();
                            dg.ItemsSource = DataTableConverter.Convert(dt);
                            dg.EndEdit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #endregion

        private void cboToolTypeCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            searchFlag = "N";
            Init();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgToolTypeAttr.ItemsSource == null || dgToolTypeAttr.Rows.Count == 0)
                    return;

                if (dgToolTypeAttr.CurrentRow.DataItem == null)
                    return;
                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                DataTable dt = ((DataView)dgToolTypeAttr.ItemsSource).Table;

                if (Convert.ToBoolean(DataTableConverter.GetValue(dgToolTypeAttr.Rows[rowIndex].DataItem, "CHK")) == true && (dt.Rows[rowIndex]["INSUSER"].ToString() == ""))
                {
                    dgToolTypeAttr.Columns["TOOL_TYPE_ATTR_CODE"].IsReadOnly = false;
                }
                else
                {
                    dgToolTypeAttr.Columns["TOOL_TYPE_ATTR_CODE"].IsReadOnly = true;
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgToolTypeAttr_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                DataRowView drv = e.Row.DataItem as DataRowView;
                bool bchk = Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK"));

                if (bchk == true)
                {
                    if (Convert.ToString(e.Column.Name) == "TOOL_TYPE_ATTR_CODE")
                    {
                        e.Cancel = false;
                    }
                }
                else
                {
                    if (Convert.ToString(e.Column.Name) == "TOOL_TYPE_ATTR_CODE")
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}