/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_002_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _WORKORDER = string.Empty;
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _LOTID = string.Empty;
        private string _MTRLID = string.Empty;

        Util _Util = new Util();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
         //<summary>
         // Frame과 상호작용하기 위한 객체
         //</summary>
 

        public ASSY001_002_LOTSTART()
        {
            InitializeComponent();
        }


        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }

            _PROCID = Util.NVC(tmps[0]);
            _EQPTID = Util.NVC(tmps[1]);

            GetLotInfo();
        }
        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;

            DataSet indataSet = new DataSet();
            DataTable inData = indataSet.Tables.Add("INDATA");
            inData.Columns.Add("SRCTYPE", typeof(string));
            inData.Columns.Add("EQPTID", typeof(string));
            inData.Columns.Add("LOTID", typeof(string));
            inData.Columns.Add("SPLITCNT", typeof(string));
            inData.Columns.Add("USERID", typeof(string));
            inData.Columns.Add("IFMODE", typeof(string));


            DataTable inMtrl = indataSet.Tables.Add("IN_MTRL");
            inMtrl.Columns.Add("LOTID", typeof(string));
            inMtrl.Columns.Add("MTRLID", typeof(string)); //투입 코터lot의 proid
            inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inMtrl.Columns.Add("LOT_TYPE_CODE", typeof(string));


            DataRow row = inData.NewRow();
            row["SRCTYPE"] = "UI";
            row["EQPTID"] = _EQPTID;
            row["LOTID"] = _LOTID;
            row["SPLITCNT"] = 1;
            row["USERID"] = LoginInfo.USERID;
            row["IFMODE"] = "OFF";
            indataSet.Tables["INDATA"].Rows.Add(row);

            DataRow row2 = inMtrl.NewRow();
            row2["LOTID"] = _LOTID;
            row2["MTRLID"] = _MTRLID;
            row2["EQPT_MOUNT_PSTN_ID"] = "MTRL_MOUNT_PSTN01";
            row2["LOT_TYPE_CODE"] = "123";
            // inMtrl.Rows.Add(row2);
            indataSet.Tables["IN_MTRL"].Rows.Add(row2);




            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_LOT_VD", "INDATA,IN_MTRL", "RSLTDT", indataSet);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업 시작"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageInfo("SFU1835");
                _LOTID = string.Empty;
                this.DialogResult = MessageBoxResult.OK;


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("착공에러"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //Util.Alert("착공 에러" + ex.ToString());
            }





        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _LOTID = string.Empty;
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgLotInfo.BeginEdit();
                //    dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
                //    dgLotInfo.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                dgLotInfo.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID").ToString();
                _MTRLID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();

            }
        }
        #endregion

        #region Mehod
        private void GetLotInfo()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    txtEquipment.Text = dtMain.Rows[0]["EQPTNAME"].ToString();
                    txtWorkorder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();

                    GetLotList();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetLotList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PROCID"] = _PROCID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WAIT_WIP", "INDATA", "RSLTDT", IndataTable);

                dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
