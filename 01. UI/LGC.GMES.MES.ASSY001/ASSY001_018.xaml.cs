/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_018 : UserControl
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public ASSY001_018()
        {
            
            InitializeComponent();
            InitCombo();

        }


        #endregion

        #region Initialize
        //화면내 combo 셋팅
        private void InitCombo()
        {


            //동,라인,공정,설비 셋팅

            CommonCombo _combo = new CMM001.Class.CommonCombo();

            //라인
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboEquipmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentChild, sFilter: sFilter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);

            //모델
            String[] sFilter2 = { LoginInfo.CFG_AREA_ID, null, null };
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, sFilter: sFilter2);

            //Target 라인
            String[] sFilter3 = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboTargetLine, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "EQUIPMENTSEGMENT");   

        }
        #endregion

        #region Event

        private void dgLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {

                //selLotData = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.DataItem;
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
                //    dgLotList.BeginEdit();
                //    dgLotList.ItemsSource = DataTableConverter.Convert(dt);
                //    dgLotList.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
            }
        }


        /// <summary>
        /// SUBLOT 이동 -- 신규Biz:  BR_PRD_REG_MOVE_LINE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("I_MODELID", typeof(string));
                inDataTable.Columns.Add("I_PRODID", typeof(string));

                DataTable inDataMagTable = indataSet.Tables.Add("INDATA_MAG");
                inDataMagTable.Columns.Add("I_MAGID", typeof(string));
                inDataMagTable.Columns.Add("I_MAGQTY", typeof(decimal));

                DataRow inData = inDataTable.NewRow();
                inData["I_MODELID"] = "UI";
                inData["I_PRODID"] = "UI";

                DataRow inDataMag = inDataMagTable.NewRow();
                inDataMag["I_MAGID"] = "UI";
                inDataMag["I_MAGQTY"] = 0;

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("???", "INDATA,INDATA_MAG", "OUTDATA", indataSet);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        #endregion

        #region Mehod
        #endregion
    }
}