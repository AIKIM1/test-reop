/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_001_WORKORDER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        public PACK001_001_WORKORDER()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                string[] area = { LoginInfo.CFG_AREA_ID.ToString() };
                C1ComboBox[] cboLineChild = { cboProcessSegmentByEqsgid };                
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboLineChild, sFilter: area);
                
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcessSegmentByEqsgid, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);

                

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCheckedRow = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);

                if (iCheckedRow > -1)
                {
                    object CheckDataItem = dgWorkOrderList.Rows[iCheckedRow].DataItem;

                    string sRouteName = cboRouteByPcsgid.Text;
                    string sWoID = Util.NVC(DataTableConverter.GetValue(CheckDataItem, "WOID"));
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1454", sRouteName, sWoID), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    //경로 [{0}] 에\n작업지시[{1}] 를 \n선택 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            setWorkOrder(CheckDataItem);
                        }
                    });
                }
                else
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1443"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //작업지시를 선택하세요
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnWorkOrderSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getWorkOrderList();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgWorkOrderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWorkOrderList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    DataTableConverter.SetValue(dgWorkOrderList.Rows[cell.Row.Index].DataItem, "CHK", "1");
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgWorkOrderList_Checked(object sender, RoutedEventArgs e)
        {
            try
            {

                if (sender == null)
                    return;

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
                    //    dgWorkOrderList.BeginEdit();
                    //    dgWorkOrderList.ItemsSource = DataTableConverter.Convert(dt);
                    //    dgWorkOrderList.EndEdit();
                    //}
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    object CheckDataItem = dgWorkOrderList.Rows[idx].DataItem;
                    string sPCSGID = Util.NVC(DataTableConverter.GetValue(CheckDataItem, "PCSGID"));
                    string sEQSGID = Util.NVC(DataTableConverter.GetValue(CheckDataItem, "EQSGID"));

                    CommonCombo _combo = new CommonCombo();
                    string[] area = { sEQSGID, sPCSGID };
                    _combo.SetCombo(cboRouteByPcsgid, CommonCombo.ComboStatus.NONE, sFilter: area);

                    //row 색 바꾸기
                    dgWorkOrderList.SelectedIndex = idx;
                    
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Mehod

        private void getWorkOrderList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = "A1A02";// Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PCSGID"] = "A";//Util.NVC(cboProcessSegmentByEqsgid.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                dgWorkOrderList.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void setWorkOrder(object CheckDataItem)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("WO_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["PCSGID"] = Util.NVC(DataTableConverter.GetValue(CheckDataItem, "PCSGID"));
                dr["EQSGID"] = Util.NVC(DataTableConverter.GetValue(CheckDataItem, "EQSGID"));
                dr["ROUTID"] = Util.NVC(cboRouteByPcsgid.SelectedValue);
                dr["WO_ID"] = Util.NVC(DataTableConverter.GetValue(CheckDataItem, "WOID"));
                dr["USERID"] = LoginInfo.USERID;
                dr["PRODID"] = Util.NVC(DataTableConverter.GetValue(CheckDataItem, "PRODID"));
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_WO_PACK", "RQSTDT", "", RQSTDT);

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                //this.DialogResult = MessageBoxResult.OK;
                //this.Close();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        
    }
}
