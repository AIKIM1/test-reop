using System.Windows;
using C1.WPF;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using LGC.GMES.MES.Common;
using System;

namespace LGC.GMES.MES.CMM001.UserControls
{
    public partial class UcFormSearch
    {
        public C1ComboBox ComboEquipment { get; set; }
        public C1ComboBox ComboNextFlow { get; set; }

        public Button ButtonSearch { get; set; }
        public Button ButtonFlofChange { get; set; }

        public CheckBox CheckRun { get; set; }

        public CheckBox CheckEqpEnd { get; set; }

        public TextBox TextBoxRouteFlow { get; set; }
        public TextBox TextBoxProcName { get; set; }

        public string ProcessCode { get; set; }


        public UcFormSearch()
        {
            InitializeComponent();
            SetControl();
        }

        private void SetControl()
        {
            ComboEquipment = cboEquipment;
            ButtonSearch = btnSearch;
            ButtonFlofChange = btnFlofChange;
            TextBoxRouteFlow = txtRouteFlow;
            TextBoxProcName = txtProcName;
            CheckRun = chkRun;
            CheckEqpEnd = chkEqpEnd;
            chkRun.IsChecked = true;
            chkEqpEnd.IsChecked = false;
        }

        public void SetCheckBoxVisibility()
        {
            //if (string.Equals(ProcessCode, Process.SmallXray))
            //{
            //    spNextFlow.Visibility = Visibility.Visible;
            //    txtRouteFlow.Visibility = Visibility.Visible;
            //    txtProcName.Visibility = Visibility.Visible;
            //    btnFlofChange.Visibility = Visibility.Visible;
            //}
            //else
            //{
                spNextFlow.Visibility = Visibility.Collapsed;
                txtRouteFlow.Visibility = Visibility.Collapsed;
                txtProcName.Visibility = Visibility.Collapsed;
                btnFlofChange.Visibility = Visibility.Collapsed;
            //}

        }

        /// <summary>
        /// 현재설정경로 조회 
        /// </summary>
        public void SetRouteFlow()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_PROC_FLOW_SET", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtRouteFlow.Text = dtResult.Rows[0]["TO_FLOWNAME"].ToString();
                    txtProcName.Text = dtResult.Rows[0]["PROCNAME_TO"].ToString();
                    txtProcName.Tag = dtResult.Rows[0]["PROCID_TO"].ToString();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


    }
}