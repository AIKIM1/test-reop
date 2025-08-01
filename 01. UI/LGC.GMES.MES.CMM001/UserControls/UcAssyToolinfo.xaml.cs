﻿/*************************************************************************************
 Created Date : 2021.05.12
      Creator : 김동일
   Decription : 금형관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.05.12  김동일  : 최초 생성
  2023.12.12  윤지해  E20231211-000182 조회용 표준 공구 ID 추가
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyToolinfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyToolinfo : UserControl, IWorkArea
    {
        #region [Constructor & Variables]
        private int selectedHistIdx = -1;
        public UserControl _UCParent { get; set; }
        public string EQPTID { get; set; }
        public string PROD_LOTID { get; set; }

        public TextBox TextToolID { get; set; }

        public IFrameOperation FrameOperation
        {
            get; set;
        }

        public UcAssyToolinfo()
        {
            InitializeComponent();
        }
        #endregion

        #region [Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TextToolID = txtToolID;
            SetComboBox();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInputHistTool();
        }

        private void btnUnmount_Click(object sender, RoutedEventArgs e)
        {
            Unmount();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton.DataContext == null)
                return;

            if ((bool)radioButton.IsChecked)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = (C1.WPF.DataGrid.DataGridCellPresenter)radioButton.Parent;
                if (cellPresenter != null)
                {
                    C1.WPF.DataGrid.C1DataGrid dataGrid = cellPresenter.DataGrid;
                    int rowIdx = cellPresenter.Row.Index;
                    dgInputHist.SelectedIndex = rowIdx;
                    selectedHistIdx = rowIdx;
                }
            }
        }

        private void dgInputHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null || dg.ItemsSource == null)
                return;

            if (e != null && e.Cell != null && e.Cell.Presenter != null)
            {
                try
                {
                    DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                    bool flag = Util.NVC(drv["MOUNT_FLAG"]).Equals("Y") ? true : false;
                    if (flag)
                    {
                        dg.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGreen);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }
        #endregion

        #region [BizCall]
        public void GetInputHistTool()
        {
            if (!CanGetInputHistTool())
            {
                return;
            }

            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("STD_TOOL_ID", typeof(string)); // 2023.12.12 윤지해 E20231211-000182 표준공구ID 추가
                inData.Columns.Add("TOOL_TYPE_CODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));

                DataRow row = inData.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROD_LOTID"] = string.IsNullOrEmpty(PROD_LOTID) ? null : PROD_LOTID;
                row["TOOL_ID"] = string.IsNullOrEmpty(txtToolID.Text) ? null : Util.NVC(txtToolID.Text);
                row["STD_TOOL_ID"] = string.IsNullOrEmpty(txtStdToolID.Text) ? null : Util.NVC(txtStdToolID.Text);  // 2023.12.12 윤지해 E20231211-000182 표준공구ID 추가
                row["TOOL_TYPE_CODE"] = string.IsNullOrEmpty(Util.NVC(cboToolType.SelectedValue)) ? null : Util.NVC(cboToolType.SelectedValue);
                row["EQPTID"] = EQPTID.Equals("SELECT") ? null : EQPTID;
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PROD_INPUT_HIST_TOOL_L", "INDATA", "OUTDATA", inData, (searchResult, exception) =>
                {
                    if (exception != null)
                    {
                        Util.MessageException(exception);
                        return;
                    }

                    Util.gridClear(dgInputHist);
                    Util.GridSetData(dgInputHist, searchResult, this.FrameOperation);
                    SetComboBox();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Unmount()
        {
            if (!CanUnmount())
            {
                return;
            }

            try
            {
                DataSet inDataSet = new DataSet();

                //IN_EQP
                DataTable inEqp = inDataSet.Tables.Add("IN_EQP");
                inEqp.Columns.Add("SRCTYPE", typeof(string));
                inEqp.Columns.Add("IFMODE", typeof(string));
                inEqp.Columns.Add("EQPTID", typeof(string));
                inEqp.Columns.Add("USERID", typeof(string));
                inEqp.Columns.Add("LOTID", typeof(string));

                DataRow dr = inEqp.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EQPTID;
                dr["USERID"] = LoginInfo.USERID;
                dr["LOTID"] = string.IsNullOrEmpty(PROD_LOTID) ? null : PROD_LOTID;
                inEqp.Rows.Add(dr);

                //IN_UBM
                DataTable inUbm = inDataSet.Tables.Add("IN_UBM");
                inUbm.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inUbm.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inUbm.Columns.Add("UBMID", typeof(string));
                inUbm.Columns.Add("UNMOUNT_RESNCODE", typeof(string));
                inUbm.Columns.Add("ACCU_USE_COUNT", typeof(Int32));

                dr = inUbm.NewRow();
                dr["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[selectedHistIdx].DataItem, "EQPT_MOUNT_PSTN_ID"));
                dr["EQPT_MOUNT_PSTN_STATE"] = "A";
                dr["UBMID"] = Util.NVC(DataTableConverter.GetValue(dgInputHist.Rows[selectedHistIdx].DataItem, "TOOL_ID"));
                inUbm.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi("BR_EQP_REG_UNMOUNT_UBM_L", "IN_EQP,IN_UBM", null, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageValidation("SFU1275", (result) =>
                    {
                        GetInputHistTool();
                    });

                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Init Method]
        private void SetComboBox()
        {
            CommonCombo cbo = new CommonCombo();
            //DA_PRD_SEL_TOOL_TYPE_CODE_CBO
            String[] sFilter1 = { EQPTID };
            cbo.SetCombo(cb: cboToolType,
                cs: CommonCombo.ComboStatus.ALL,
                sFilter: sFilter1,
                sCase: "TOOL_TYPE_CODE");
        }

        public void ClearInfo()
        {
            if (dgInputHist != null)
                Util.gridClear(dgInputHist); 
        }
        #endregion

        #region [Valid Method]
        private bool CanUnmount()
        {
            bool bRet = false;

            if (selectedHistIdx == -1)
            {
                //선택한 대상이 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanGetInputHistTool()
        {
            bool bRet = false;

            if (string.IsNullOrEmpty(EQPTID))
            {
                //설비정보가 없습니다.
                Util.MessageValidation("SFU2911");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion
    }
}
