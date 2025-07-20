/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_075_SELECTPROCESS : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        public PGM_GUI_075_SELECTPROCESS()
        {
            InitializeComponent();
        }

        private DataRow drSelectProcessInfo = null;
        private DataRow drSelectedProcessInfo = null;
        private string sSelectedProcessTitle = string.Empty;

        public DataRow DrSelectedProcessInfo
        {
            get
            {
                return drSelectedProcessInfo;
            }

            set
            {
                drSelectedProcessInfo = value;
            }
        }
        public string SSelectedProcessTitle
        {
            get
            {
                return sSelectedProcessTitle;
            }

            set
            {
                sSelectedProcessTitle = value;
            }
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            setProcessList();
        }
        private void cboEqpt_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            setProcessList();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

            DrSelectedProcessInfo = drSelectProcessInfo;
            sSelectedProcessTitle = "( " + drSelectProcessInfo["LINENAME"].ToString() + " )  " + drSelectProcessInfo["PROCNAME"].ToString();
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = (Button)sender;
            drSelectProcessInfo = (DataRow)clicked.Tag;
            txtSelectedLine.Text = drSelectProcessInfo["LINENAME"].ToString();
            txtSelectedProcess.Text = drSelectProcessInfo["PROCNAME"].ToString();
        }

        #endregion

        #region Mehod

        /// <summary>
        /// 
        /// </summary>
        private void setProcessList()
        {
            gdProcessList.RowDefinitions.Clear();

            DataTable dtLineProcessList = getLineProcessList();
            setGridProcessList(dtLineProcessList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DataTable getLineProcessList()
        {
            #region Sample Table
            DataRow newRow = null;
            DataTable dtTemp = new DataTable();
            dtTemp.Columns.Add("LINEID");
            dtTemp.Columns.Add("LINENAME");
            dtTemp.Columns.Add("PROCNAME");
            dtTemp.Columns.Add("PROCID");
            dtTemp.Columns.Add("PROCSEQ");

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CELL 검사", "CT201", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CELL 컷팅밴딩", "CM202", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CELL 시퀀스", "CM203", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CMA 조립", "MA201", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CMA 용접", "MW201", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L1", "2호기", "CMA 검사", "MT201", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "CELL 검사", "CTP01", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "CELL 폴딩밴딩", "CMP02", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "CMA 조립", "MAP01", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "CMA 용접및검사", "MTP01", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "P1", "PILOT", "BMA 조립", "BTP01", 5 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CELL 검사", "CT301", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CELL 컷팅밴딩", "CM302", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CELL 시퀀스", "CM303", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CMA 조립", "MA301", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CMA 용접", "MW301", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L2", "3호기", "CMA 검사", "MT301", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CELL 검사", "CT401", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CELL 컷팅밴딩", "CM402", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CELL 시퀀스", "CM403", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CMA 조립", "MA401", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CMA 용접", "MW401", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L3", "4호기", "CMA 검사", "MT401", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CELL 검사", "CT501", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CELL 컷팅밴딩", "CM502", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CELL 시퀀스", "CM503", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CMA 조립", "MA501", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CMA 용접", "MW501", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L4", "5호기", "CMA 검사", "MT501", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CELL 검사", "CT601", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CELL 컷팅밴딩", "CM602", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CELL 시퀀스", "CM603", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CMA 조립", "MA601", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CMA 용접", "MW601", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L5", "6호기", "CMA 검사", "MT601", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CELL 검사", "CT701", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CELL 컷팅밴딩", "CM702", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CELL 시퀀스", "CM703", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CMA 조립", "MA701", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L6", "7호기", "CMA 용접", "MW701", 5 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CELL 검사", "CT801", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CELL 컷팅밴딩", "CM802", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CELL 시퀀스", "CM803", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CMA 조립", "MA801", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CMA 용접", "MW801", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L7", "8호기", "CMA 검사", "MT801", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CELL 검사", "CT901", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CELL 컷팅밴딩", "CM902", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CELL 시퀀스", "CM903", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CMA 조립", "MA901", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CMA 용접", "MW901", 5 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L8", "9호기", "CMA 검사", "MT901", 6 };
            dtTemp.Rows.Add(newRow);

            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CELL 검사", "CTA01", 1 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CELL 컷팅밴딩", "CMA02", 2 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CELL 시퀀스", "CMA03", 3 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CMA 조립", "MAA01", 4 };
            dtTemp.Rows.Add(newRow);
            newRow = dtTemp.NewRow();
            newRow.ItemArray = new object[] { "L9", "10호기", "CMA 용접", "MWA01", 5 };
            dtTemp.Rows.Add(newRow);
            #endregion

            return dtTemp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtLineProcessList"></param>
        private void setGridProcessList(DataTable dtLineProcessList)
        {
            //line groupby 추출
            var list = dtLineProcessList.AsEnumerable().GroupBy(r => new
            {
                PRODGROUP = r.Field<string>("LINEID")
            }).Select(g => g.First());
            DataTable dtLineList = list.CopyToDataTable();

            //Line 수 roof
            for (int i = 0; i < dtLineList.Rows.Count; i++)
            {
                var rowMain = new RowDefinition();
                rowMain.Height = GridLength.Auto;
                gdProcessList.RowDefinitions.Add(rowMain);

                //Main Grid 에 subGrid(line 공정) 추가
                Grid grdSubGridNew = new Grid();
                grdSubGridNew.Name = dtLineList.Rows[i]["LINEID"].ToString();
                grdSubGridNew.Tag = dtLineList.Rows[i]["LINEID"];

                //데이터 line의 공정 추출
                DataTable myNewTable = dtLineProcessList.Select("LINEID = '" + dtLineList.Rows[i]["LINEID"].ToString() + "'").CopyToDataTable();

                //첫번째 컬럼 에 라인명 삽입.
                var column = new ColumnDefinition();
                column.Width = new System.Windows.GridLength(100);
                grdSubGridNew.ColumnDefinitions.Add(column);

                System.Windows.Controls.TextBlock newTextBlock = new TextBlock();

                newTextBlock.Text = dtLineList.Rows[i]["LINENAME"].ToString();
                newTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                newTextBlock.Style = (Style)FindResource("Content_InputForm_LabelStyle");
                newTextBlock.SetValue(Grid.RowProperty, 0);
                newTextBlock.SetValue(Grid.ColumnProperty, 0);
                grdSubGridNew.Children.Add(newTextBlock);

                //sub grid에 공정 컬럼 추가
                var columnProcess = new ColumnDefinition();
                columnProcess.Width = GridLength.Auto;
                grdSubGridNew.ColumnDefinitions.Add(columnProcess);

                //공정 컬럼에 panel 추가
                WrapPanel wpProcess = new WrapPanel();
                wpProcess.Width = 750;
                wpProcess.Margin = new Thickness(0, 5, 0, 5);
                //wpProcess.Margin = new Thickness(15);
                wpProcess.SetValue(Grid.RowProperty, 0);
                wpProcess.SetValue(Grid.ColumnProperty, 1);
                grdSubGridNew.Children.Add(wpProcess);

                //line의 공정 수 만큼 panel에 버튼 생성
                for (int c = 0; c < myNewTable.Rows.Count; c++)
                {
                    System.Windows.Controls.Button newBtn = new Button();
                    newBtn.Content = myNewTable.Rows[c]["PROCNAME"].ToString();
                    newBtn.Name = "Button" + myNewTable.Rows[c]["PROCID"].ToString();
                    newBtn.Tag = myNewTable.Rows[c];//myNewTable.Rows[c]["PROCID"].ToString();
                    newBtn.Style = (Style)FindResource("Content_MainButtonNextStyle");
                    newBtn.Width = 150;
                    newBtn.Margin = new Thickness(10, 5, 10, 5);
                    newBtn.Click += btnProcess_Click;
                    wpProcess.Children.Add(newBtn);
                }

                grdSubGridNew.SetValue(Grid.RowProperty, gdProcessList.RowDefinitions.Count - 1);
                gdProcessList.Children.Add(grdSubGridNew);

                var border = new Border();
                border.BorderBrush = Brushes.Gray;
                border.BorderThickness = new Thickness(1);
                border.SetValue(Grid.RowProperty, gdProcessList.RowDefinitions.Count - 1);
                gdProcessList.Children.Add(border);


                var rowEmpty = new RowDefinition();
                rowEmpty.Height = new System.Windows.GridLength(20);
                gdProcessList.RowDefinitions.Add(rowEmpty);
            }
        }



        #endregion
    }
}