/*************************************************************************************
 Created Date : 2024.06.09
      Creator : wsx0651 
   Decription : Gmes Purge  Data 복구 화면  (아카아빙 DB -> GMES 운영 DB)
--------------------------------------------------------------------------------------
 [Change History]
  2024.06.09  DEVELOPER : Initial Created.
  
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using System.Xml;
using System.IO;
using System.Text;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_DATA_RECOVERY : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private List<TableInfo_Sting> tableInfos = new List<TableInfo_Sting>();

        public COM001_DATA_RECOVERY()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(RSTR_STD_COL, CommonCombo.ComboStatus.NONE, sCase: "cboRSTR_STD_COL");

        }

        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                Clear();
                GetSummaryData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSummaryData()
        {
            try
            {

                string xml_string = null;
                string Table_Name = null;
                bool Lot_check = false;

                ShowLoadingIndicator();

                // 복원 대상 테이블 조회 PARAM
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("RSTR_STD_COL", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RSTR_STD_COL"] = Util.GetCondition(RSTR_STD_COL, bAllNull: false);

                dtRqst.Rows.Add(dr);

                // 복원 기준을 통해 복원 대상 Table 목록 조회
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RSTR_TBL", "RQSTDT", "RSLTDT", dtRqst);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    string Table_Check = dtRslt.Rows[i]["TBL_NAME"].ToString();

                    if (Table_Check == "LOT")
                    {
                        Lot_check = true;

                    }

                }

                //복원 테이블 중 LoT 테이블이 있을경우 CASCADE 로 묶인 WIP/ WIPATTR / WIPHISTORY / WIPHISTORYATTR / LOTATTR 도 복원 대상에 추가.
                if (Lot_check == true)
                {
                    DataRow dt_LotATTR = dtRslt.NewRow();
                    dt_LotATTR["TBL_NAME"] = "LOTATTR";
                    dtRslt.Rows.Add(dt_LotATTR);

                    DataRow dt_WIP = dtRslt.NewRow();
                    dt_WIP["TBL_NAME"] = "WIP";
                    dtRslt.Rows.Add(dt_WIP);

                    DataRow dt_WIPATTR = dtRslt.NewRow();
                    dt_WIPATTR["TBL_NAME"] = "WIPATTR";
                    dtRslt.Rows.Add(dt_WIPATTR);

                    DataRow dt_HIS = dtRslt.NewRow();
                    dt_HIS["TBL_NAME"] = "WIPHISTORY";
                    dtRslt.Rows.Add(dt_HIS);

                    DataRow dt_HISATTR = dtRslt.NewRow();
                    dt_HISATTR["TBL_NAME"] = "WIPHISTORYATTR";
                    dtRslt.Rows.Add(dt_HISATTR);

                }

                // 복원 데이터 조회 PARAM
                DataTable dtRqst_Data = new DataTable();
                dtRqst_Data.TableName = "INDATA_Second";
                dtRqst_Data.Columns.Add("TBL_NAME", typeof(string));
                dtRqst_Data.Columns.Add("RSTR_STD_COL", typeof(string));
                dtRqst_Data.Columns.Add("RSTR_STD_COL_VALUE", typeof(string));

                DataRow dr_Data = dtRqst_Data.NewRow();
                dr_Data["RSTR_STD_COL"] = Util.GetCondition(RSTR_STD_COL, bAllNull: false);
                dr_Data["RSTR_STD_COL_VALUE"] = Util.NVC(Txt_RSTR_STD_COL_VALUE.Text);
                dr_Data["TBL_NAME"] = null;
                dtRqst_Data.Rows.Add(dr_Data);

                ShowLoadingIndicator();
                // 동적으로 Grid 생성 시 Table 명 및 Grid를 구분하기 위한 Row 정의
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    DataGridsContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    DataGridsContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    dr_Data["TBL_NAME"] = Util.NVC(dtRslt.Rows[i]["TBL_NAME"]);
                    DataTable dtRslt_Search = new ClientProxy().ExecuteServiceSync("DA_SEL_ARCHV_RSTR_DATA", "RQSTDT", "RSLTDT", dtRqst_Data); // 아카이빙 DB 
                    xml_string = Convert.ToString((dtRslt_Search.Rows[0] as DataRow)[0]);
                    Table_Name = Util.NVC(dtRslt.Rows[i]["TBL_NAME"]);

                    if (!string.IsNullOrEmpty(xml_string))
                    {

                        DataTable dataTable = ConvertXmlToDataTable(xml_string);  //GRID 에 보여주는 아카이빙 원본 데이터
                        DataTable dataTable_NULL = dataTable.Copy();
                        DataTable dataTable_CHK_NULL = CheckNull_DataTable(dataTable_NULL);  // 데이터 값이 NULL 일 경우 문자열 NULL로 표시하여 공백과 구분 되도록 가공                 
                        DataGrid dataGrid = CreateDataGrid(dataTable_CHK_NULL, Table_Name); // 가공된 DataTable 을 Grid 로 생성 

                        // 테이블 이름 Block 생성
                        TextBlock Table_Name_TextBlock = new TextBlock
                        {
                            Text = Table_Name,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(3)
                        };

                        Grid.SetRow(Table_Name_TextBlock, i * 2);
                        Grid.SetColumn(Table_Name_TextBlock, 0);
                        DataGridsContainer.Children.Add(Table_Name_TextBlock);

                        Grid.SetRow(dataGrid, i * 2 + 1);
                        Grid.SetColumn(dataGrid, 0);
                        DataGridsContainer.Children.Add(dataGrid);

                        int dataRow_cnt = dataTable.Rows.Count;


                        DataTable Rstr_dataTable = dataTable.Copy();  // 아카이빙 DB 데이터를 GMES DB에 복원 하기위한 컬럼 값 변경 1.BAKDTTM 컬럼 삭제 / 2.UPDDTTM 변경
                        foreach (DataRow row in Rstr_dataTable.Rows)
                        {
                            if (Rstr_dataTable.Columns.Contains("BAKDTTM"))
                            {
                                row["BAKDTTM"] = DBNull.Value;
                            }

                            if (Rstr_dataTable.Columns.Contains("UPDDTTM"))
                            {
                                row["UPDDTTM"] = DateTime.Now.ToString("yyyy-MM-d HH:m:ss:fff");
                            }
                        }

                        if (Rstr_dataTable.Columns.Contains("BAKDTTM"))
                        {
                            Rstr_dataTable.Columns.Remove("BAKDTTM");
                        }

                        var rowDataList = ExtractRowData(Rstr_dataTable);

                        List<List<string>> Columns = new List<List<string>>();
                        List<List<string>> Values = new List<List<string>>();

                        foreach (var tuple in rowDataList)
                        {
                            Columns.Add(tuple.Item1);
                            Values.Add(tuple.Item2);
                        }

                        TableInfo_Sting tableInfo = new TableInfo_Sting(Table_Name, Columns, Values, dataRow_cnt);
                        tableInfos.Add(tableInfo);
                    }
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable ConvertXmlToDataTable(string xmlString)
        {
            DataTable dataTable = new DataTable();
            DataSet dataSet = new DataSet();

            using (StringReader stringReader = new StringReader(xmlString))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader))
                {
                    dataSet.ReadXml(xmlReader);
                    if (dataSet.Tables.Count > 0)
                    {
                        dataTable = dataSet.Tables[0];
                    }
                }
            }

            return dataTable;
        }

        private DataTable CheckNull_DataTable(DataTable datatable)
        {
            foreach (DataRow row in datatable.Rows)
            {
                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    if (row[i] == DBNull.Value)
                    {
                        row[i] = "NULL";
                    }
                }
            }

            return datatable;
        }


        private DataGrid CreateDataGrid(DataTable dataTable, string Table_Name)
        {

            DataGrid dataGrid = new DataGrid
            {
                AutoGenerateColumns = true,
                ItemsSource = dataTable.DefaultView,
                Margin = new Thickness(5),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HeadersVisibility = System.Windows.Controls.DataGridHeadersVisibility.Column,
                CanUserAddRows = false // 빈 행 제거
            };

            dataGrid.AutoGeneratingColumn += (sender, e) =>
            {
                e.Column.IsReadOnly = true;
            };

            return dataGrid;
        }


        public struct TableInfo_Sting // GMES 로 데이터 복구 하기 위해 가공된 데이터들 (BAKDTTM 삭제 및 UPDTTM 컬럼 GETDATE() 로 변경)
        {
            public string TableName;
            public List<List<string>> ColumnNames;
            public List<List<string>> DataValues;
            public int Datarow_cnt;

            public TableInfo_Sting(string tableName, List<List<string>> columnNames, List<List<string>> dataValues, int datarow_cnt)
            {
                TableName = tableName;
                ColumnNames = columnNames;
                DataValues = dataValues;
                Datarow_cnt = datarow_cnt;
            }
        }

        private List<Tuple<List<string>, List<string>>> ExtractRowData(DataTable dataTable)
        {
            var rowDataList = new List<Tuple<List<string>, List<string>>>();


            foreach (DataRow row in dataTable.Rows)
            {
                var columnNames = new List<string>();
                var columnValues = new List<string>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    var value = row[column];

                    if (value != DBNull.Value)
                    {
                        columnNames.Add(column.ColumnName);
                        columnValues.Add(value.ToString());
                    }

                }

                rowDataList.Add(Tuple.Create(columnNames, columnValues));
            }

            return rowDataList;
        }

        private string ConvertColumnListToString(List<string> columnList)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append('(');
            stringBuilder.Append(string.Join(", ", columnList.Select(data => $"{ data}")));
            stringBuilder.AppendLine("),");

            ////마지막 쉼표 제거
            if (stringBuilder.Length > 2)
            {
                stringBuilder.Length -= 2;
            }
            return stringBuilder.ToString().TrimEnd(',', ' ');

        }

        private string ConvertDataListToString(List<string> dataList)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append('(');
            stringBuilder.Append(string.Join(", ", dataList.Select(data => $"'{ data}'")));
            stringBuilder.AppendLine("),");

            //마지막 쉼표 제거
            if (stringBuilder.Length > 2)
            {
                stringBuilder.Length -= 2;
            }
            return stringBuilder.ToString().TrimEnd(',', ' ');

        }

        private void Clear()
        {
            DataGridsContainer.Children.Clear();
            DataGridsContainer.RowDefinitions.Clear();
            tableInfos.Clear();
            Rstr_Result.Text = string.Empty;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            Txt_RSTR_STD_COL_VALUE.Text = string.Empty;
        }

        private void btnRstr_Click(object GridDataList, RoutedEventArgs e)
        {
            try
            {
                Rstr_Result.Text = string.Empty;
                // DataSet 생성
                DataSet indataSet = new DataSet();

                // Insert INDATA 생성
                DataTable dtInsert_Rqst = indataSet.Tables.Add("INSERT_INDATA");
                dtInsert_Rqst.Columns.Add("TBL_NAME", typeof(string));
                dtInsert_Rqst.Columns.Add("Columns", typeof(string));
                dtInsert_Rqst.Columns.Add("Values", typeof(string));

                List<string> TBL_NAME = new List<string>();
                List<string> Columns = new List<string>();
                List<string> Values = new List<string>();

                for (int i = 0; i < tableInfos.Count; i++)
                {
                    for (int j = 0; j < tableInfos[i].Datarow_cnt; j++)
                    {
                        TBL_NAME.Add(tableInfos[i].TableName);
                    }

                    for (int j = 0; j < tableInfos[i].ColumnNames.Count; j++)
                    {
                        string columnNames = ConvertColumnListToString(tableInfos[i].ColumnNames[j]);
                        Columns.Add(columnNames);

                    }

                    for (int j = 0; j < tableInfos[i].DataValues.Count; j++)
                    {
                        string values = ConvertDataListToString(tableInfos[i].DataValues[j]);
                        Values.Add(values);
                    }

                }

                for (int i = 0; i < Columns.Count; i++)
                {
                    DataRow dr_Ins = dtInsert_Rqst.NewRow();
                    dr_Ins["TBL_NAME"] = TBL_NAME[i].ToString();
                    dr_Ins["Columns"] = Columns[i].ToString();
                    dr_Ins["Values"] = Values[i].ToString();
                    dtInsert_Rqst.Rows.Add(dr_Ins);
                }

                // Delete INDATA 생성                
                DataTable dtDelete_Rqst = new DataTable();
                dtDelete_Rqst.TableName = "DELETE_INDATA";
                dtDelete_Rqst.Columns.Add("TBL_NAME", typeof(string));
                dtDelete_Rqst.Columns.Add("RSTR_STD_COL", typeof(string));
                dtDelete_Rqst.Columns.Add("RSTR_STD_COL_VALUE", typeof(string));

                for (int i = 0; i < TBL_NAME.Count; i++)
                {
                    DataRow dr_Del = dtDelete_Rqst.NewRow();
                    dr_Del["RSTR_STD_COL"] = Util.GetCondition(RSTR_STD_COL, bAllNull: false);
                    dr_Del["RSTR_STD_COL_VALUE"] = Util.NVC(Txt_RSTR_STD_COL_VALUE.Text);
                    dr_Del["TBL_NAME"] = TBL_NAME[i].ToString();
                    dtDelete_Rqst.Rows.Add(dr_Del);
                }
                DataTable dtDelete_Rqst_Distinct = dtDelete_Rqst.DefaultView.ToTable(true); // 중복제거 Datatable
                indataSet.Tables.Add(dtDelete_Rqst_Distinct); // 중복 제거 후 dataSet 에 테이블 추가

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_GMES_PURGE_DATA_RECOVERY", "INSERT_INDATA,DELETE_INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Clear();
                            Util.MessageException(bizException);
                            return;
                        }

                        Rstr_Result.Text = string.Empty;

                        StringBuilder infoBuilder = new StringBuilder();

                        for (int i = 0; i < tableInfos.Count; i++)
                        {
                            infoBuilder.AppendLine($"{tableInfos[i].TableName} : {tableInfos[i].Datarow_cnt} Row Recovery Success");
                        }

                        // TextBlock에 정보 표시
                        Rstr_Result.Text = infoBuilder.ToString();
                        DataGridsContainer.Children.Clear();
                        DataGridsContainer.RowDefinitions.Clear();
                        Util.MessageInfo("FM_ME_0140");  //복구 완료하였습니다.
                        ///tableInfos.Clear();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        HiddenLoadingIndicator();
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion
    }

}
