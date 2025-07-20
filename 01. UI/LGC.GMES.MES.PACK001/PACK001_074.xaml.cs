/*************************************************************************************
 Created Date : 2020.09.15
      Creator : 정용석
  Description : Check MMD Setting Nav.
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.15  정용석 : Initial Created.
  2021.03.20  정용석 : 라인 콤보박스 이벤트 변경 (SelectedIndexChanged -> SelectedValueChanged)
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_074 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        public PACK001_074()
        {
            InitializeComponent();
            InitializeControl();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Method
        private void InitializeControl()
        {
            try
            {
                CommonCombo combo = new CommonCombo();
                combo.SetCombo(this.cboArea, CommonCombo.ComboStatus.NONE, cbChild: new C1ComboBox[] { this.cboEquipmentSegment });
                combo.SetCombo(this.cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: new C1ComboBox[] { this.cboArea });
                this.cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
                this.dgSummary.FrozenColumnCount = 3;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetProductComboBox()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string)); 
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? null : this.cboArea.SelectedValue.ToString();
                if (this.cboEquipmentSegment.SelectedValue == null)
                {
                    dr["EQSGID"] = null;
                }
                else if (string.IsNullOrEmpty(this.cboEquipmentSegment.SelectedValue.ToString()))
                {
                    dr["EQSGID"] = null;
                }
                else
                {
                    dr["EQSGID"] = this.cboEquipmentSegment.SelectedValue.ToString();
                }
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHOP_PRDT_ROUT_CBO", "RQSTDT", "RSLTDT", dt);

                this.cboProduct.DisplayMemberPath = "PRODID";
                this.cboProduct.SelectedValuePath = "PRODID";
                
                DataRow drResult = dtResult.NewRow();
                drResult["PRODID"] = "-ALL-";
                dtResult.Rows.InsertAt(drResult, 0);

                this.cboProduct.ItemsSource = DataTableConverter.Convert(dtResult);
                this.cboProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SearchProcess()
        {
            // Clear
            Util.gridClear(this.dgSummary);
            this.txtSummaryCount.Text = "[ 0 건 ]";
            Util.gridClear(this.dgUnregistered);
            this.txtUnregistredCount.Text = "[ 0 건 ]";

            DataSet dsInputSet = new DataSet();
            DataSet dsOutputSet = new DataSet();

            DataTable dt = new DataTable("IN_DATA");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("ISSUMMARY", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));
            dt.Columns.Add("REGISTER_YN", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? LoginInfo.CFG_AREA_ID : this.cboArea.SelectedValue.ToString();
            dr["EQSGID"] = string.IsNullOrEmpty(this.cboEquipmentSegment.SelectedValue.ToString()) ? LoginInfo.CFG_EQSG_ID : this.cboEquipmentSegment.SelectedValue.ToString();
            if (string.IsNullOrEmpty(this.cboProduct.SelectedValue.ToString()) || this.cboProduct.SelectedValue.ToString().Equals("-ALL-"))
            {
                dr["PRODID"] = null;
            }
            else
            {
                dr["PRODID"] = this.cboProduct.SelectedValue.ToString();
            }
            dr["ISSUMMARY"] = "Y";

            dt.Rows.Add(dr);
            dsInputSet.Tables.Add(dt);
            dsOutputSet = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_MMD_SETTING", "IN_DATA", "OUTDATA_DETAIL,OUTDATA", dsInputSet);

            if (dsOutputSet.Tables["OUTDATA"] != null && dsOutputSet.Tables["OUTDATA"].Rows.Count > 0)
            {
                Util.GridSetData(this.dgSummary, dsOutputSet.Tables["OUTDATA"], FrameOperation);
                this.txtSummaryCount.Text = "[ " + dsOutputSet.Tables["OUTDATA"].Rows.Count.ToString() + " 건 ]";
            }
        }

        private void SearchDetail(int rowIndex)
        {
            Util.gridClear(this.dgUnregistered);
            this.txtUnregistredCount.Text = "[ 0 건 ]";

            DataSet dsInputSet = new DataSet();
            DataSet dsOutputSet = new DataSet();
            DataTable dt = new DataTable("IN_DATA");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("ISSUMMARY", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));
            dt.Columns.Add("REGISTER_YN", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = string.IsNullOrEmpty(this.cboArea.SelectedValue.ToString()) ? LoginInfo.CFG_AREA_ID : this.cboArea.SelectedValue.ToString();
            dr["EQSGID"] = string.IsNullOrEmpty(this.cboEquipmentSegment.SelectedValue.ToString()) ? LoginInfo.CFG_EQSG_ID : this.cboEquipmentSegment.SelectedValue.ToString();
            if (string.IsNullOrEmpty(this.cboProduct.SelectedValue.ToString()) || this.cboProduct.SelectedValue.ToString().Equals("-ALL-"))
            {
                dr["PRODID"] = null;
            }
            else
            {
                dr["PRODID"] = this.cboProduct.SelectedValue.ToString();
            }
            dr["ISSUMMARY"] = "N";
            dr["CMCODE"] = Util.NVC(DataTableConverter.GetValue(this.dgSummary.Rows[rowIndex].DataItem, "CMCODE"));
            dr["REGISTER_YN"] = "N";

            dt.Rows.Add(dr);
            dsInputSet.Tables.Add(dt);

            dsOutputSet = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_MMD_SETTING", "IN_DATA", "OUTDATA_DETAIL,OUTDATA", dsInputSet);

            if (dsOutputSet.Tables["OUTDATA_DETAIL"] != null && dsOutputSet.Tables["OUTDATA_DETAIL"].Rows.Count > 0)
            {
                Util.GridSetData(this.dgUnregistered, dsOutputSet.Tables["OUTDATA_DETAIL"], FrameOperation);
                this.txtUnregistredCount.Text = "[ " + dsOutputSet.Tables["OUTDATA_DETAIL"].Rows.Count.ToString() + " 건 ]";
            }
        }

        private void PopupDetail(int rowIndex)
        {
            PACK001_074_POPUP popup = new PACK001_074_POPUP();
            popup.FrameOperation = this.FrameOperation;
            if (popup != null)
            {
                object[] obj = new object[4];
                obj[0] = Util.NVC(DataTableConverter.GetValue(this.dgSummary.Rows[rowIndex].DataItem, "AREAID"));
                obj[1] = Util.NVC(DataTableConverter.GetValue(this.dgSummary.Rows[rowIndex].DataItem, "EQSGID"));
                obj[2] = this.cboProduct.SelectedValue.ToString();
                obj[3] = Util.NVC(DataTableConverter.GetValue(this.dgSummary.Rows[rowIndex].DataItem, "CMCODE"));
                C1WindowExtension.SetParameters(popup, obj);

                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                this.SearchProcess();
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.SetProductComboBox();
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.SetProductComboBox();
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point point = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell dataGridCell = this.dgSummary.GetCellFromPoint(point);
                if (dataGridCell == null || dataGridCell.Value == null)
                {
                    return;
                }

                switch (dataGridCell.Column.Name.ToUpper())
                {
                    case "REGISTER_CNT":
                        loadingIndicator.Visibility = Visibility.Visible;
                        DoEvents();
                        this.PopupDetail(dataGridCell.Row.Index);
                        break;

                    case "UNREGISTER_CNT":
                        loadingIndicator.Visibility = Visibility.Visible;
                        DoEvents();
                        this.SearchDetail(dataGridCell.Row.Index);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
    }
}