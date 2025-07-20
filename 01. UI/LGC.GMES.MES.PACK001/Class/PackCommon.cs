/*************************************************************************************
 Created Date : 2021.05.06
      Creator : 정용석
  Description : Pack UI에서 공통으로 사용되는 함수
--------------------------------------------------------------------------------------
 [Change History]
  2021.05.06 / 정용석 : Initial Created.
  2024.07.23 / 최평부 : GridProperty 항목 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001.Class
{
    static class PackCommon
    {
        // DataGrid Merge
        public static void SetDataGridMergeExtensionCol(C1DataGrid c1DataGrid, string[] arrColumnName, DataGridMergeMode eMergeMode)
        {
            if (c1DataGrid.GetRowCount() <= 0)
            {
                return;
            }

            for (int i = 0; i < arrColumnName.Length; i++)
            {
                DataGridMergeExtension.SetMergeMode(c1DataGrid.Columns[arrColumnName[i].ToString()], eMergeMode); //DataGridMergeMode.VERTICALHIERARCHI);
            }

            c1DataGrid.ReMerge();
        }

        // Pack 포장 라벨 발행 Popup Open
        public static void ShowPalletTag(string formID, string palletID, string equipmentSegmentID, string labelType)
        {
            try
            {
                if (!GetPackApplyLIneByUI(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID))
                {
                    // 기존 Pallet Tag
                    Pallet_Tag popupPalletTag = new Pallet_Tag();

                    if (popupPalletTag != null)
                    {
                        // 태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[3];
                        Parameters[0] = "Pallet_Tag";
                        Parameters[1] = palletID;
                        Parameters[2] = equipmentSegmentID;

                        C1WindowExtension.SetParameters(popupPalletTag, Parameters);
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => popupPalletTag.Show()));
                    }
                }
                else
                {
                    // 신규 Pallet Tag
                    Pallet_Tag_V2 popupPalletTag = new Pallet_Tag_V2();

                    if (popupPalletTag == null)
                    {
                        return;
                    }

                    object[] Parameters = new object[3];
                    Parameters[0] = labelType;
                    Parameters[1] = palletID;
                    Parameters[2] = equipmentSegmentID;

                    C1WindowExtension.SetParameters(popupPalletTag, Parameters);
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => popupPalletTag.Show()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static bool GetPackApplyLIneByUI(string shopID, string areaID)
        {
            bool returnValue = false;
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = "DIFFUSION_SITE";
                drRQSTDT["CBO_CODE"] = "PLT_AREA_TAG";
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    returnValue = false;
                }
                else
                {
                    foreach (DataRow drRSLTDT in dtRSLTDT.Select())
                    {
                        string settingShopID = drRSLTDT["ATTRIBUTE1"].ToString();
                        string settingAreaID = drRSLTDT["ATTRIBUTE2"].ToString();

                        if ((!string.IsNullOrEmpty(settingShopID) && (!string.IsNullOrEmpty(settingShopID) && settingShopID.Contains(shopID))) &&
                            (!string.IsNullOrEmpty(settingAreaID) && (!string.IsNullOrEmpty(settingAreaID) && settingAreaID.Contains(areaID)))
                           )
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return returnValue;
        }

        // MCS AP 정보 가져오기
        public static DataTable GetMCSBizActorServerInfo(string keyGroupID)
        {
            String bizRuleName = "DA_MCS_SEL_MCS_CONFIG_INFO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");
            dtRQSTDT.Columns.Add("KEYGROUPID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["KEYGROUPID"] = keyGroupID;
            dtRQSTDT.Rows.Add(drRQSTDT);

            dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            if (!CommonVerify.HasTableRow(dtRSLTDT))
            {
                return null;
            }

            var query = dtRSLTDT.AsEnumerable().GroupBy(x => x.Field<string>("KEYGROUPID")).Select(grp => new
            {
                BIZACTORIP = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORIP")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORPORT = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORPORT")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORPROTOCOL = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORPROTOCOL")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORSERVICEINDEX = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORSERVICEINDEX")) ? x.Field<string>("KEYVALUE") : string.Empty),
                BIZACTORSERVICEMODE = grp.Max(x => (x.Field<string>("KEYID").ToUpper().Equals("BIZACTORSERVICEMODE")) ? x.Field<string>("KEYVALUE") : string.Empty)
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // QRCode
        public static byte[] GetQRCode(string strQRString)
        {
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrData = qrGen.CreateQrCode(strQRString, QRCodeGenerator.ECCLevel.Q);
            QRCode qr = new QRCode(qrData);
            System.Drawing.Image fullsizeImage = qr.GetGraphic(20);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(100, 100, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Bmp);
            return myResult.ToArray();
        }

        // [7590438713016 건] 표시
        public static void SearchRowCount(ref TextBlock textBlock, int rowCount)
        {
            textBlock.Text = "[ " + rowCount.ToString() + " " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }

        // Linq 결과 DataTable로 변환
        public static DataTable queryToDataTable(IEnumerable<dynamic> records)
        {
            DataTable dt = new DataTable();

            try
            {
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dt;
        }

        // 로딩그림 나오게 해주는거
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        // Grid Check All or Uncheck All
        public static void GridCheckAllFlag(C1DataGrid c1DataGrid, bool isAllChecked, string checkBoxColumnName)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in c1DataGrid.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, checkBoxColumnName, isAllChecked);
            }

            c1DataGrid.EndEdit();
            c1DataGrid.EndEditRow(true);
        }

        // Grid Check ExcepValue All or Uncheck All 
        public static void GridCheckAllFlag_ExcepValue(C1DataGrid c1DataGrid, bool isAllChecked, string checkBoxColumnName, string strExceptColumn = null, string strExceptValue = null)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in c1DataGrid.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[row.Index].DataItem, strExceptColumn)) != strExceptValue)
                {
                    DataTableConverter.SetValue(row.DataItem, checkBoxColumnName, isAllChecked);
                }
            }

            c1DataGrid.EndEdit();
            c1DataGrid.EndEditRow(true);
        }

        // Container Control 안에 들어있는 Control 찾기
        public static Item FindChildControl<Item>(DependencyObject containerObject, string searchItemProperty, string searchKey) where Item : DependencyObject
        {
            // Declarations...
            Item foundChild = null;

            try
            {
                if (containerObject == null)
                {
                    return foundChild;
                }

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(containerObject); i++)
                {
                    var childControl = VisualTreeHelper.GetChild(containerObject, i);

                    if (childControl is ContentControl || childControl is ItemsControl || childControl is Panel)
                    {
                        foundChild = FindChildControl<Item>(childControl, searchItemProperty, searchKey);
                        if (foundChild != null)
                        {
                            break;
                        }
                    }

                    if (childControl.GetType() != typeof(Item))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(searchKey))
                    {
                        var frameworkElement = (FrameworkElement)childControl;

                        if (frameworkElement != null && searchItemProperty.Equals("NAME") && Util.NVC(frameworkElement.Name.ToString()).Equals(searchKey))
                        {
                            foundChild = (Item)childControl;
                            break;
                        }

                        if (frameworkElement != null && searchItemProperty.Equals("TAG") && Util.NVC(frameworkElement.Tag.ToString()).Equals(searchKey))
                        {
                            foundChild = (Item)childControl;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                foundChild = null;
                return foundChild;
            }

            return foundChild;
        }

        // ComboBox Binding
        public static void SetC1ComboBox(DataTable dt, C1ComboBox c1ComboBox, bool isEmptyDataBinding, string title = null)
        {
            List<string> lstValueMemberPathFilter = new List<string>() { "ID", "CODE", "CD", "USE_FLAG", "TYPE" };
            List<string> lstDisplayMemberPathFilter = new List<string>() { "NAME", "NM", "DESC" };

            if (!CommonVerify.HasTableRow(dt) && !isEmptyDataBinding)
            {
                return;
            }

            try
            {
                var selectedValuePath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstValueMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                var displayMemberPath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstDisplayMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                if (selectedValuePath == null || displayMemberPath == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(title))
                {
                    DataRow dr = dt.NewRow();
                    switch (title.ToUpper())
                    {
                        case "ALL":
                            dr[selectedValuePath] = string.Empty;
                            dr[displayMemberPath] = "-ALL-";
                            break;
                        case "SELECT":
                            dr[selectedValuePath] = string.Empty;
                            dr[displayMemberPath] = "-SELECT-";
                            break;
                        case "N/A":
                            dr[selectedValuePath] = string.Empty;
                            dr[displayMemberPath] = "-N/A-";
                            break;
                        default:
                            dr[selectedValuePath] = string.Empty;
                            dr[displayMemberPath] = title;
                            break;
                    }
                    dt.Rows.InsertAt(dr, 0);
                }

                c1ComboBox.SelectedValuePath = selectedValuePath;
                c1ComboBox.DisplayMemberPath = displayMemberPath;
                c1ComboBox.ItemsSource = dt.AsDataView();
                c1ComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // ComboBox Binding (향후 사용하지 말것)
        public static void SetC1ComboBox(DataTable dt, C1ComboBox c1ComboBox, string title = null)
        {
            List<string> lstValueMemberPathFilter = new List<string>() { "ID", "CODE", "CD", "USE_FLAG", "TYPE" };
            List<string> lstDisplayMemberPathFilter = new List<string>() { "NAME", "NM", "DESC" };

            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            try
            {
                var selectedValuePath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstValueMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                var displayMemberPath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstDisplayMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                if (selectedValuePath == null || displayMemberPath == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(title))
                {
                    DataRow dr = dt.NewRow();
                    dr[selectedValuePath] = title.Equals("ALL") ? title : string.Empty;
                    dr[displayMemberPath] = title;
                    dt.Rows.InsertAt(dr, 0);
                }

                c1ComboBox.SelectedValuePath = selectedValuePath;
                c1ComboBox.DisplayMemberPath = displayMemberPath;
                c1ComboBox.ItemsSource = dt.AsDataView();
                c1ComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // MultiComboBox Binding - check All
        public static void SetMultiSelectionComboBox(DataTable dt, MultiSelectionBox multiSelectionBox, ref int bindingRowCount)
        {
            List<string> lstValueMemberPathFilter = new List<string>() { "ID", "CODE", "CD", "USE_FLAG", "TYPE" };
            List<string> lstDisplayMemberPathFilter = new List<string>() { "NAME", "NM", "DESC" };

            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            try
            {
                var selectedValuePath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstValueMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                var displayMemberPath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstDisplayMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                if (selectedValuePath == null || displayMemberPath == null)
                {
                    return;
                }

                multiSelectionBox.isAllUsed = true;
                multiSelectionBox.ApplyTemplate();
                multiSelectionBox.ItemsSource = null;
                multiSelectionBox.DisplayMemberPath = displayMemberPath;
                multiSelectionBox.SelectedValuePath = selectedValuePath;

                if (CommonVerify.HasTableRow(dt))
                {
                    bindingRowCount = dt.Rows.Count;
                    multiSelectionBox.ItemsSource = dt.AsDataView();
                    multiSelectionBox.Check(-1);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // MultiComboBox Binding - check select
        public static void SetMultiSelectionComboBox(DataTable dt, MultiSelectionBox multiSelectionBox, ref int bindingRowCount, string strValue)
        {
            List<string> lstValueMemberPathFilter = new List<string>() { "ID", "CODE", "CD", "USE_FLAG", "TYPE" };
            List<string> lstDisplayMemberPathFilter = new List<string>() { "NAME", "NM", "DESC" };

            if (!CommonVerify.HasTableRow(dt))
            {
                return;
            }

            try
            {
                var selectedValuePath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstValueMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                var displayMemberPath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstDisplayMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                if (selectedValuePath == null || displayMemberPath == null)
                {
                    return;
                }

                multiSelectionBox.isAllUsed = true;
                multiSelectionBox.ApplyTemplate();
                multiSelectionBox.ItemsSource = null;
                multiSelectionBox.DisplayMemberPath = displayMemberPath;
                multiSelectionBox.SelectedValuePath = selectedValuePath;

                if (CommonVerify.HasTableRow(dt))
                {
                    bindingRowCount = dt.Rows.Count;
                    multiSelectionBox.ItemsSource = dt.AsDataView();
                    
                    if (string.IsNullOrWhiteSpace(strValue))
                    { return; }

                    string[] strValueList = strValue.Split(',');

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < strValueList.Count(); j++)
                        {
                            if (Util.NVC(dt.Rows[i][selectedValuePath]) == strValueList[j])
                            {
                                multiSelectionBox.Check(i);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Dynamic Grid Column Add 1호
        public static void AddGridColumn(C1DataGrid c1DataGrid, string columnType, string columnName, bool isVisible)
        {
            switch (columnType.ToUpper())
            {
                case "RADIOBUTTON":
                    C1.WPF.DataGrid.DataGridTemplateColumn dataGridTemplateColumn = new C1.WPF.DataGrid.DataGridTemplateColumn();
                    DataTemplate dataTemplate = new DataTemplate();
                    FrameworkElementFactory radioButton = new FrameworkElementFactory(typeof(RadioButton));
                    radioButton.SetValue(RadioButton.GroupNameProperty, "keyGroup");
                    radioButton.SetValue(RadioButton.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    radioButton.SetValue(RadioButton.VerticalAlignmentProperty, VerticalAlignment.Center);
                    radioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding(columnName));
                    dataTemplate.VisualTree = radioButton;
                    dataGridTemplateColumn.CellTemplate = dataTemplate;
                    dataGridTemplateColumn.Header = columnName;
                    dataGridTemplateColumn.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                    c1DataGrid.Columns.Add(dataGridTemplateColumn);
                    dataTemplate.Seal();
                    break;
                case "CHECKBOX":
                    C1.WPF.DataGrid.DataGridCheckBoxColumn dataGridCheckBoxColumn = new C1.WPF.DataGrid.DataGridCheckBoxColumn();
                    Binding checkBoxBinding = new Binding(columnName);
                    checkBoxBinding.Mode = BindingMode.TwoWay;
                    dataGridCheckBoxColumn.Header = columnName;
                    dataGridCheckBoxColumn.Binding = checkBoxBinding;
                    dataGridCheckBoxColumn.HorizontalAlignment = HorizontalAlignment.Center;
                    dataGridCheckBoxColumn.VerticalAlignment = VerticalAlignment.Center;
                    dataGridCheckBoxColumn.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    dataGridCheckBoxColumn.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                    c1DataGrid.Columns.Add(dataGridCheckBoxColumn);
                    break;
                case "TEXT":
                    C1.WPF.DataGrid.DataGridTextColumn dataGridTextColumn = new C1.WPF.DataGrid.DataGridTextColumn();
                    Binding textBinding = new Binding(columnName);
                    textBinding.Mode = BindingMode.TwoWay;
                    dataGridTextColumn.Header = columnName;
                    dataGridTextColumn.Binding = textBinding;
                    dataGridTextColumn.HorizontalAlignment = HorizontalAlignment.Left;
                    dataGridTextColumn.VerticalAlignment = VerticalAlignment.Center;
                    dataGridTextColumn.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    dataGridTextColumn.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    dataGridTextColumn.IsReadOnly = true;
                    c1DataGrid.Columns.Add(dataGridTextColumn);
                    break;
                default:
                    break;
            }
        }

        // Dynamic Grid Column Add 2호
        public static void AddGridColumn(C1DataGrid c1DataGrid, string columnType, string columnHeaderName, string columnName, bool isVisible)
        {
            switch (columnType.ToUpper())
            {
                case "RADIOBUTTON":
                    C1.WPF.DataGrid.DataGridTemplateColumn dataGridTemplateColumn = new C1.WPF.DataGrid.DataGridTemplateColumn();
                    DataTemplate dataTemplate = new DataTemplate();
                    FrameworkElementFactory radioButton = new FrameworkElementFactory(typeof(RadioButton));
                    radioButton.SetValue(RadioButton.GroupNameProperty, "keyGroup");
                    radioButton.SetValue(RadioButton.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    radioButton.SetValue(RadioButton.VerticalAlignmentProperty, VerticalAlignment.Center);
                    radioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding(columnName));
                    dataTemplate.VisualTree = radioButton;
                    dataGridTemplateColumn.CellTemplate = dataTemplate;
                    dataGridTemplateColumn.Header = columnHeaderName;
                    dataGridTemplateColumn.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                    c1DataGrid.Columns.Add(dataGridTemplateColumn);
                    dataTemplate.Seal();
                    break;
                case "CHECKBOX":
                    C1.WPF.DataGrid.DataGridCheckBoxColumn dataGridCheckBoxColumn = new C1.WPF.DataGrid.DataGridCheckBoxColumn();
                    Binding checkBoxBinding = new Binding(columnName);
                    checkBoxBinding.Mode = BindingMode.TwoWay;
                    dataGridCheckBoxColumn.Header = columnHeaderName;
                    dataGridCheckBoxColumn.Binding = checkBoxBinding;
                    dataGridCheckBoxColumn.HorizontalAlignment = HorizontalAlignment.Center;
                    dataGridCheckBoxColumn.VerticalAlignment = VerticalAlignment.Center;
                    dataGridCheckBoxColumn.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    dataGridCheckBoxColumn.Width = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                    c1DataGrid.Columns.Add(dataGridCheckBoxColumn);
                    break;
                case "TEXT":
                    C1.WPF.DataGrid.DataGridTextColumn dataGridTextColumn = new C1.WPF.DataGrid.DataGridTextColumn();
                    Binding textBinding = new Binding(columnName);
                    textBinding.Mode = BindingMode.TwoWay;
                    dataGridTextColumn.Header = columnHeaderName;
                    dataGridTextColumn.Binding = textBinding;
                    dataGridTextColumn.HorizontalAlignment = HorizontalAlignment.Left;
                    dataGridTextColumn.VerticalAlignment = VerticalAlignment.Center;
                    dataGridTextColumn.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    dataGridTextColumn.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    dataGridTextColumn.IsReadOnly = true;
                    c1DataGrid.Columns.Add(dataGridTextColumn);
                    break;
                default:
                    break;
            }

        }

        // Initialize - Popup Draggable
        public static void SetPopupDraggable(Popup popup, DockPanel dockPanel)
        {
            Thumb thumb = new Thumb()
            {
                Width = 0,
                Height = 0
            };
            DockPanel.SetDock(thumb, System.Windows.Controls.Dock.Bottom);
            dockPanel.Children.Add(thumb);
            dockPanel.MouseDown += (sender, e) =>
            {
                thumb.RaiseEvent(e);
            };
            thumb.DragDelta += (sender, e) =>
            {
                popup.HorizontalOffset += e.HorizontalChange;
                popup.VerticalOffset += e.VerticalChange;
            };
        }

        // Rich Text Box Clear
        public static void ClearRichTextBox(ref RichTextBox richTextBox)
        {
            richTextBox.SelectAll();
            richTextBox.Selection.Text = string.Empty;
            richTextBox.UpdateLayout();
        }

        // Grid Excel Export
        public static void ExcelExport(C1DataGrid c1DataGrid)
        {
            try
            {
                new ExcelExporter().Export(c1DataGrid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Two DataTable is Same!!
        public static bool IsSameDataTable(DataTable dt1, DataTable dt2)
        {
            if (!CommonVerify.HasTableRow(dt1) || !CommonVerify.HasTableRow(dt2))
            {
                return false;
            }

            // Schema Check
            if (!dt1.Columns.OfType<DataColumn>().Except(dt2.Columns.OfType<DataColumn>()).Any() && !dt2.Columns.OfType<DataColumn>().Except(dt1.Columns.OfType<DataColumn>()).Any())
            {
                return false;
            }

            // Data Row Check
            if (dt1.Rows.Count != dt2.Rows.Count)
            {
                return false;
            }

            // Data Value Check
            if (!dt1.AsEnumerable().Except(dt2.AsEnumerable()).Any() && !dt2.AsEnumerable().Except(dt1.AsEnumerable()).Any())
            {
                return false;
            }

            return true;
        }

        // Truncate Time
        public static DateTime TruncateDateTime(DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
            {
                return dateTime;
            }

            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return dateTime;
            }

            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        // Error Popup Show
        public static void Show_EXCEPTION_POPUP(DataTable dt, string exceptionType, IFrameOperation FrameOperation)
        {
            EXCEPTION_POPUP wndPopUp = new EXCEPTION_POPUP();
            wndPopUp.FrameOperation = FrameOperation;

            if (wndPopUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = dt;
                Parameters[1] = exceptionType;

                C1WindowExtension.SetParameters(wndPopUp, Parameters);
                wndPopUp.ShowModal();
                wndPopUp.CenterOnScreen();
                wndPopUp.BringToFront();
            }
        }

    }

    public class GridProperty
    {
        #region Member Variable Lists...
        public string CATEGORY;
        public string DATA_PROPERTIES;
        public string CODE_COLUMN;                  // Popup 안의 DataGrid에 Binding되었을 때의 Code Column
        public string CODENAME_COLUMN;              // Popup 안의 DataGrid에 Binding되었을 때의 CodeName Column
        public string LINKED_VALUEPATH_COLUMN;      // 이벤트를 전달한 부모 DataGrid의 Code Column
        public string LINKED_DISPLAYPATH_COLUMN;    // 이벤트를 전달한 부모 DataGrid의 CodeName Column
        public string GRID_NAME;                    // DataGrid Grid Name
        public bool IS_LASTGRID;                    // Popup에서 조립동, 조립라인, 전극설비처럼 3개의 DataGrid 표출시에 가장 마지막에 위치한 Grid 여부
        public int ROW_INDEX;                       // DataGrid가 위치할 Popup Grid의 Row Index
        public int COLUMN_INDEX;                    // DataGrid가 위치할 Popup Grid의 Column Index
        public bool MULTI_CHECK;                    // DataGrid의 항목 선택시 다중선택여부 (RadioButton으로는 동작 X)

        public string MAIN_EQPTNAME;                // 포장기 연결 MAIN 설비 2024.07.08 BY 최평부 추가
        public string LINK_MAIN_EQPTNAME;           // 포장기 연결 MAIN 설비 2024.07.08 BY 최평부 추가

        
        #endregion
    }
}