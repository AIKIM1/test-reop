using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_139.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_139 : UserControl, IWorkArea
    {
        public COM001_139()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
        }

        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cboAreaChild = { cboLine };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sFilter: sFilter);


            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);
            cboArea.SelectedValue = "";
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Search_List();
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }
        }

        private void Search_List()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));


            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = Util.NVC(cboArea.SelectedValue.ToString()) == "" ? null : cboArea.SelectedValue.ToString();
            dr["EQSGID"] = Util.NVC(cboLine.SelectedValue.ToString()) == "" ? null : cboLine.SelectedValue.ToString();

            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_SEL_LOCATION_STAT", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                this.drawingCanvas.Children.Clear();
                string sLocation = string.Empty;
                if (dt.Rows.Count > 0)
                {
                    Geometry geoPath = new PathGeometry();
                    foreach (DataRow ro in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(ro["CODI_VALUE"].ToString()))
                        {
                            if (!ro["LOCATION_ID"].ToString().Equals(sLocation))
                            {
                                sLocation = ro["LOCATION_ID"].ToString();
                                geoPath = Geometry.Parse(ro["CODI_VALUE"].ToString());
                                PathGeometry pathGeometry = geoPath.GetFlattenedPathGeometry();
                                PathFigure pathFigure = new PathFigure();
                                pathFigure = pathGeometry.Figures[0];
                                PointCollection pointList = GetFlattenedPointList(pathFigure);

                                Polygon newPolygon = new Polygon();
                                newPolygon.Name = ro["LOCATION_ID"].ToString();
                                newPolygon.Stroke = Brushes.DarkGray;
                                newPolygon.StrokeThickness = 2;
                                newPolygon.Fill = Brushes.Red;
                                newPolygon.Opacity = 1;
                                for (int i = 0; i < pointList.Count; i++)
                                {
                                    newPolygon.Points.Add(pointList[i]);
                                }
                                var minX = pointList.Min(p => p.X);
                                var maxX = pointList.Max(p => p.X);
                                var minY = pointList.Min(p => p.Y);
                                var maxY = pointList.Max(p => p.Y);

                                //폴리곤에 Text 추가
                                TextBox txtBox = new TextBox();
                                txtBox.Name = ro["LOCATION_ID"].ToString();
                                txtBox.Tag = ro["AREAID"].ToString();
                                txtBox.Foreground = Brushes.White;
                                txtBox.Text = ro["POSITION_NAME"].ToString();
                                txtBox.Padding = new Thickness(0, 0, 0, 0);
                                txtBox.Width = maxX - minX;
                                txtBox.Height = maxY - minY;
                                txtBox.Margin = new Thickness(0, 0, 0, 0);
                                txtBox.BorderThickness = new Thickness(0, 0, 0, 0);
                                txtBox.Background = Brushes.Transparent;
                                txtBox.TextWrapping = TextWrapping.Wrap;
                                txtBox.TextAlignment = TextAlignment.Center;
                                txtBox.FontSize = 9;
                                txtBox.FontWeight = FontWeights.Bold;
                                txtBox.IsReadOnly = true;
                                txtBox.MouseDoubleClick += TxtBox_MouseDoubleClick;
                                Canvas.SetLeft(txtBox, minX);
                                Canvas.SetTop(txtBox, minY);

                                this.drawingCanvas.Children.Insert(0, newPolygon);
                                drawingCanvas.Children.Add(txtBox);
                            }

                        }
                    }
                    fn_Set_ToolTip(dt);
                }
            });
        }

        private void TxtBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            string sLocation = txtBox.Name;
            loadingIndicator.Visibility = Visibility.Visible;
            string[] sParam = { txtBox.Name, txtBox.Tag.ToString() };
            this.FrameOperation.OpenMenu("SFU10999508", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;

        }

        private void fn_Set_ToolTip(DataTable dt)
        {
            foreach(UIElement Element in drawingCanvas.Children)
            {
                if (Element is TextBox)
                {
                    var txtBox = Element as TextBox;
                    var result = dt.AsEnumerable().Where(r => r["LOCATION_ID"].ToString() == txtBox.Name).CopyToDataTable();
                    if(result.Rows.Count > 0)
                    {
                        Grid grid = new Grid();
                        for (int i = 0; i < result.Rows.Count + 2; i++)
                        {
                            RowDefinition rowdef = new RowDefinition();
                            rowdef.Height = new GridLength(20);
                            grid.RowDefinitions.Add(rowdef);
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            ColumnDefinition coldef = new ColumnDefinition();
                            if(i == 0)
                            {
                                coldef.Width = new GridLength(120);
                            }
                            else
                            {
                                coldef.Width = new GridLength(60);
                            }
                            grid.ColumnDefinitions.Add(coldef);
                        }

                        //Grid 셋팅
                        TextBlock txtBolck = new TextBlock();
                        txtBolck.Text = txtBox.Name;
                        txtBolck.FontSize = 14;
                        txtBolck.FontWeight = FontWeights.Bold;
                        txtBolck.TextAlignment = TextAlignment.Center;
                        grid.Children.Add(txtBolck);
                        Grid.SetRow(txtBolck, 0);
                        Grid.SetColumn(txtBolck, 0);
                        Grid.SetColumnSpan(txtBolck, 4);

                        if (result.Rows.Count > 1)
                        {
                            TextBlock CellBox = new TextBlock();
                            CellBox.Text = "CELL";
                            CellBox.TextAlignment = TextAlignment.Center;
                            TextBlock ModuleBox = new TextBlock();
                            ModuleBox.Text = "MODULE";
                            ModuleBox.TextAlignment = TextAlignment.Center;
                            TextBlock PackBox = new TextBlock();
                            PackBox.Text = "PACK";
                            PackBox.TextAlignment = TextAlignment.Center;

                            grid.Children.Add(CellBox);
                            Grid.SetRow(CellBox, 1);
                            Grid.SetColumn(CellBox, 1);

                            grid.Children.Add(ModuleBox);
                            Grid.SetRow(ModuleBox, 1);
                            Grid.SetColumn(ModuleBox, 2);

                            grid.Children.Add(PackBox);
                            Grid.SetRow(PackBox, 1);
                            Grid.SetColumn(PackBox, 3);

                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                //명칭
                                TextBlock block = new TextBlock();
                                block.TextAlignment = TextAlignment.Left;
                                block.Text = result.Rows[i]["NAME"].ToString();
                                //Cell
                                TextBlock Cblock = new TextBlock();
                                Cblock.TextAlignment = TextAlignment.Center;
                                Cblock.Text = string.Format("{0} EA {1}", result.Rows[i]["CELL_1"].ToString(), result.Rows[i]["CELL_2"].ToString());
                                //Module
                                TextBlock Mblock = new TextBlock();
                                Mblock.TextAlignment = TextAlignment.Center;
                                Mblock.Text = string.Format("{0} EA {1}", result.Rows[i]["MODULE_1"].ToString(), result.Rows[i]["MODULE_2"].ToString());
                                //Pack
                                TextBlock Pblock = new TextBlock();
                                Pblock.TextAlignment = TextAlignment.Center;
                                Pblock.Text = string.Format("{0} EA {1}", result.Rows[i]["PACK_1"].ToString(), result.Rows[i]["PACK_2"].ToString());

                                grid.Children.Add(block);
                                Grid.SetRow(block, i + 2);
                                Grid.SetColumn(block, 0);

                                grid.Children.Add(Cblock);
                                Grid.SetRow(Cblock, i + 2);
                                Grid.SetColumn(Cblock, 1);

                                grid.Children.Add(Mblock);
                                Grid.SetRow(Mblock, i + 2);
                                Grid.SetColumn(Mblock, 2);


                                grid.Children.Add(Pblock);
                                Grid.SetRow(Pblock, i + 2);
                                Grid.SetColumn(Pblock, 3);
                            }
                        }
                        ToolTipService.SetShowDuration(txtBox, 60000);
                        txtBox.ToolTip = grid;
                    }
                }
            }
        }

        private PointCollection GetFlattenedPointList(PathFigure pathFigure)
        {
            PointCollection pointList = new PointCollection();

            //pointList.Add(pathFigure.StartPoint);

            foreach (PathSegment pathSegment in pathFigure.Segments)
            {
                if (pathSegment is LineSegment)
                {
                    LineSegment lineSegment = pathSegment as LineSegment;
                    pointList.Add(lineSegment.Point);
                }
                else if (pathSegment is PolyLineSegment)
                {
                    PolyLineSegment polyLineSegment = pathSegment as PolyLineSegment;

                    foreach (Point point in polyLineSegment.Points)
                    {
                        pointList.Add(point);
                    }
                }
                else
                {
                    throw new Exception("unknown segment" + pathSegment.GetType().Name);
                }
            }

            return pointList;
        }

        private void btnLotSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLot.Text))
            {
                Util.Alert("SFU1190");  //조회할 LOT ID 를 입력하세요.
                return;
            }

            if (drawingCanvas.Children.Count < 1)
            {
                Util.Alert("SFU8421");  //보관 위치별 수량정보가 없습니다.
                return;

            }

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));



            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = txtLot.Text;

            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_SEL_TB_RTLS_LOT", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                if (dt.Rows.Count < 1)
                {
                    Util.Alert("SFU1009");
                    return;
                }
                string locationID = dt.Rows[0]["LOCATIONID"].ToString();
                Polygon polygon;
                bool chk = false;
                foreach (var el in drawingCanvas.Children)
                {
                    if (el is Polygon)
                    {
                        polygon = (Polygon)el;
                        if (polygon.Name == locationID)
                        {
                            polygon.Fill = Brushes.Green;
                            chk = true;
                        }
                    }
                }
                if (!chk)
                {
                    Util.Alert("SFU8422"); //조회된 보관위치에는 해당 LOT이 존재하지않습니다.
                }

            });
        }
    }
}
