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
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_022 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        string _EQSGID = string.Empty;
        string _EQPTID = string.Empty;
        string _SkipFlag = string.Empty;
        string _EqptFlag = string.Empty;
        string _EqptElec = string.Empty;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_022()
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

        #region event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            //initCombo();
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initCombo();
        }

        private void cboEquipmentSegmentVDQA_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] sFilter = { Process.VD_LMN, cboEquipmentSegmentVDQA.SelectedValue.ToString() };
            combo.SetCombo(cboEquipmentVDQA, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "cboVDEquipment");

            DataTable dt = new DataTable();
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));


            DataRow row = dt.NewRow();
            row["PROCID"] = Process.VD_LMN;
            row["EQSGID"] = Convert.ToString(cboEquipmentSegmentVDQA.SelectedValue);
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PROD_SEL_PROCESSEQUIPMENTSEGMENT_VDQA", "RQSTDT", "RSLTDT", dt);
            if (result == null) return;
            if (result.Rows.Count == 0 || result.Rows[0]["LQC_SKIP_FLAG"].Equals(""))
            {
                Util.Alert("LQC_SKIP_FLAG가 없습니다.");
                return;
            }
            _SkipFlag = Convert.ToString(result.Rows[0]["LQC_SKIP_FLAG"]);

        }
   
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            clearGrid();
            SearchData();

        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                int firstindex = _Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2");
                if (firstindex == -1) return;

               
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }
        private void btnMoveLine_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2") == -1)
            {
                Util.Alert("선택한 LOT이 없습니다.");
                return;
            }
            if (_SkipFlag.Equals("N"))
            {

                for (int i = 0; i < dgQAComplete.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "CHK2")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "CHK2")).Equals("True"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "JUDG_VALUE")).Equals("E"))
                        {
                            Util.Alert("[" + Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "LOTID")) + "]은 검사대기 이므로 라미로 이송할 수 없습니다.");
                            return;

                        }

                        if (Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "JUDG_VALUE")).Equals("F") )
                        {
                            Util.Alert("[" + Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "LOTID")) + "]은 불합격이므로 라미로 이송할 수 없습니다.");
                            return;

                        }
                    }
                }

            }


            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Convert.ToString(cboMoveLine.Text) + "라미로 이송 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet inData = new DataSet();

                        DataTable inDataTable = inData.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PASSYN", typeof(string));
                        inDataTable.Columns.Add("TO_EQSGID", typeof(string));
                        inDataTable.Columns.Add("TO_PROCID", typeof(string));

                        DataRow row = null;
                        row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["USERID"] = LoginInfo.USERID;
                        row["PASSYN"] = "Y";
                        row["TO_EQSGID"] = cboMoveLine.SelectedValue.ToString();
                        row["TO_PROCID"] = Process.LAMINATION;

                        inData.Tables["INDATA"].Rows.Add(row);


                        DataTable INLOT = inData.Tables.Add("IN_LOT");
                        INLOT.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dgQAComplete.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "CHK2")).Equals("1"))
                            {
                                row = INLOT.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "LOTID"));
                                inData.Tables["IN_LOT"].Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MOVE_LOT_LINE", "INDATA,IN_LOT", null, inData);
                        Util.AlertInfo("이송완료");

                        int index = _Util.GetDataGridCheckFirstRowIndex(dgEquipment, "CHK");
                        SearchData();
                        DataTableConverter.SetValue(dgEquipment.Rows[index].DataItem, "CHK", 1);
                        setVDFinish(index);
                       

                    }
                    catch (Exception ex)
                    {
                        Util.Alert(ex.ToString());
                    }
                }
            });





        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2") == -1)
            {
                Util.Alert("선택한 LOT이 없습니다.");
                return;
            }

            if (_SkipFlag.Equals("N"))
            {
                //?
            }
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("재작업(V/D) 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet inData = new DataSet();

                        DataTable inDataTable = inData.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("PASSYN", typeof(string));
                        inDataTable.Columns.Add("TO_EQSGID", typeof(string));
                        inDataTable.Columns.Add("TO_PROCID", typeof(string));

                        DataRow row = null;
                        row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["USERID"] = LoginInfo.USERID;
                        row["PASSYN"] = "N";
                        row["TO_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[_Util.GetDataGridCheckFirstRowIndex(dgQAComplete, "CHK2")].DataItem, "EQSGID"));
                        row["TO_PROCID"] = Process.VD_LMN;

                        inData.Tables["INDATA"].Rows.Add(row);


                        DataTable INLOT = inData.Tables.Add("IN_LOT");
                        INLOT.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dgQAComplete.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "CHK2")).Equals("1"))
                            {
                                row = INLOT.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "LOTID"));
                                inData.Tables["IN_LOT"].Rows.Add(row);
                            }
                        }


                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MOVE_LOT_LINE", "INDATA,IN_LOT", null, inData);
                        Util.AlertInfo("이송완료");

                        int index = _Util.GetDataGridCheckFirstRowIndex(dgEquipment, "CHK");
                        SearchData();
                        DataTableConverter.SetValue(dgEquipment.Rows[index].DataItem, "CHK", 1);
                        setVDFinish(index);

                    }
                    catch (Exception ex)
                    {
                        Util.Alert(ex.ToString());
                    }

                }
            });
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            _EQPTID = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "EQPTID"));
            _EQSGID = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "EQSGID"));
            _EqptFlag = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "PRDT_CLSS_CHK_FLAG"));
            _EqptElec = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "PRDT_CLSS_CODE"));

            if (_EqptFlag.Equals("N"))
            {
                Util.Alert("극성이 없은 설비인지 확인해주세요.");
                for (int i = 0; i < dgEquipment.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgEquipment.Rows[i].DataItem, "CHK", false);
                }
                dgQAComplete.ItemsSource = null;
                return;
            }
            if (_EqptFlag.Equals("Y") && _EqptElec.Equals(""))
            {
                Util.Alert("해당설비의 극성데이터가 없습니다.");
                for (int i = 0; i < dgQAComplete.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgEquipment.Rows[i].DataItem, "CHK", false);
                }
                dgQAComplete.ItemsSource = null;
                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "CHK")).Equals("1"))
            {
                for (int i = 0; i < dgEquipment.GetRowCount(); i++)
                {
                    if (i != index)
                    {
                        DataTableConverter.SetValue(dgEquipment.Rows[i].DataItem, "CHK", false);
                    }
                }

                setLineCombo();
                setVDFinish(index);

            }
            else
            {
                dgQAComplete.ItemsSource = null;
            }
        }


        #endregion

        #region Method
        private void initCombo()
        {
            string[] sFilter = { Process.VD_LMN, LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboEquipmentSegmentVDQA, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "cboVDEquipmentSegment");

        }
        private void SearchData()
        {
            try
            {
                dgQAComplete.ItemsSource = null;
               
                DataTable data = new DataTable();
                data.Columns.Add("LANGID", typeof(string));
                data.Columns.Add("PROCID", typeof(string));
                data.Columns.Add("EQSGID", typeof(string));
                data.Columns.Add("EQPTID", typeof(string));

                DataRow row = data.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.VD_LMN;
                row["EQSGID"] = cboEquipmentSegmentVDQA.SelectedValue.ToString();
                row["EQPTID"] = cboEquipmentVDQA.SelectedValue.ToString();
                data.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_VD_QA", "RQSTDT", "RSLTDT", data);
                //if (result.Rows.Count < 0 || result == null)
                //{
                //    Util.Alert("데이터가 없습니다.");
                //    dgEquipment.ItemsSource = null;
                //    return;
                //}

                //dgEquipment.ItemsSource = DataTableConverter.Convert(result);
                Util.GridSetData(dgEquipment, result, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void clearGrid()
        {
            dgQAComplete.ItemsSource = null;
            dgEquipment.ItemsSource = null;
        }

        private void setVDFinish(int index)
        {
            try
            {
                DataTable data = null;
                DataTable result = null;
                DataRow row = null;

                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                row = dt.NewRow();
                row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "EQPTID"));
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_QA", "RQSTDT", "RSLTDT", dt);
                Util.GridSetData(dgQAComplete, result, FrameOperation);

                if (_EqptElec.Equals("C"))
                {
                    SetLargeLotQA();
                }

                if (_SkipFlag.Equals("N"))//QA검사가 필요한 라인
                {
                    if (dgQAComplete.ItemsSource != null)
                    {
                       dgQAComplete.ItemsSource = DataTableConverter.Convert(((DataView)dgQAComplete.ItemsSource).ToTable().Select("JUDG_VALUE = 'Y'").Count() == 0 ? null : ((DataView)dgQAComplete.ItemsSource).ToTable().Select("JUDG_VALUE = 'Y'").CopyToDataTable());
                    }
                }
                 
             //   else
                //{
                //    data = new DataTable();
                //    data.Columns.Add("LANGID", typeof(string));
                //    data.Columns.Add("EQPTID", typeof(string));
                //    data.Columns.Add("PROCID", typeof(string));
                //    data.Columns.Add("WIPSTAT", typeof(string));
                //    data.Columns.Add("CMCDTYPE", typeof(string));
                //    data.Columns.Add("JUDG_VALUE", typeof(string));


                //    DataRow row = data.NewRow();
                //    row["LANGID"] = LoginInfo.LANGID;
                //    row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEquipment.Rows[index].DataItem, "EQPTID"));
                //    row["WIPSTAT"] = Wip_State.END;
                //    row["PROCID"] = Process.VD_LMN;
                //    row["CMCDTYPE"] = "QAJUDGE";
                //    row["JUDG_VALUE"] = "Y";
                //    data.Rows.Add(row);

                //    if (_EqptElec.Equals("C"))//양극
                //    {
                //        result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_QA_LOT_FOR_LAMI", "RQSTDT", "RSLTDT", data);
                //    }
                //    else if (_EqptElec.Equals("A"))
                //    {
                //        result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_QA_LOT_A_FOR_LAMI", "RQSTDT", "RSLTDT", data);
                //    }
                //}

               // Util.GridSetData(dgQAComplete, result, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void SetLargeLotQA()
        {
            DataTable dt = null;
            DataRow row = null;

            for (int i = 0; i < dgQAComplete.GetRowCount(); i++) //LOT별 등록
            {

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "LOTID"));
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QA_INSP_HIST_LOT", "RQSTDT", "RSLTDT", dt); //lot별로 조회
                if (result.Rows.Count == 0 || !result.Rows[0]["JUDG_VALUE"].Equals("RF"))
                {
                    dt = new DataTable();
                    dt.Columns.Add("LANGID", typeof(string));
                    dt.Columns.Add("LOTID_RT", typeof(string));

                    row = dt.NewRow();
                    row["LANGID"] = LoginInfo.LANGID;
                    row["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgQAComplete.Rows[i].DataItem, "LOTID_RT"));
                    dt.Rows.Add(row);

                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_RT_QA", "RQSTDT", "RSLTDT", dt); //대LOT별로 조회
                    if (result.Rows.Count == 0)
                    {
                        DataTableConverter.SetValue(dgQAComplete.Rows[i].DataItem, "JUDG_VALUE", "E");
                        DataTableConverter.SetValue(dgQAComplete.Rows[i].DataItem, "JUDG_NAME", "검사대기");
                            
                        continue;
                    }

                }
                DataTableConverter.SetValue(dgQAComplete.Rows[i].DataItem, "JUDG_VALUE", result.Rows[0]["JUDG_VALUE"]);
                DataTableConverter.SetValue(dgQAComplete.Rows[i].DataItem, "JUDG_NAME", result.Rows[0]["JUDG_NAME"]);

            }
        }

        private void setLineCombo()
        {
            DataTable data = new DataTable();
            data.Columns.Add("LANGID", typeof(string));
            data.Columns.Add("PROCID", typeof(string));
            data.Columns.Add("AREA", typeof(string));

            DataRow row = data.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["PROCID"] = Process.LAMINATION;
            row["AREA"] = LoginInfo.CFG_AREA_ID;
            data.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_LAMI", "RQSTDT", "RSLTDT", data);

            cboMoveLine.SelectedValuePath = "CBO_CODE";
            cboMoveLine.DisplayMemberPath = "CBO_NAME";

            cboMoveLine.ItemsSource = DataTableConverter.Convert(result);
            cboMoveLine.SelectedIndex = 0;
        }




        #endregion

        private void cboEquipmentVDQA_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            dgEquipment.ItemsSource = null;
            dgQAComplete.ItemsSource = null;
        }

      
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
       
        private void dgQAComplete_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK2"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgQAComplete == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgQAComplete.ItemsSource); ;

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK2"] = 1;
            }

            dgQAComplete.ItemsSource = DataTableConverter.Convert(dt);

        }
        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgQAComplete == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgQAComplete.ItemsSource); ;

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK2"] = 0;
            }
            dgQAComplete.ItemsSource = DataTableConverter.Convert(dt);

        }
     
        
    }
}
