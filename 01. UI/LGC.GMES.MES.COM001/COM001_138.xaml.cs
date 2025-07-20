using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_138 : UserControl, IWorkArea
    {
        #region Geomerty 
        public enum DesignType
        {
            Create,
            Edit,
            End
        };
        private DesignType editDesignType = DesignType.End;
        private Polyline _Polyline = null;
        private Point LastPosition;
        private Polygon editPolygon = null;
        private Ellipse editEllipse = null;
        private int editPolygonPartIndex;
        private Geometry locGeometry = null;
        Path locPath = null;
        private Polygon selectPolygon = null;
        #endregion

        #region MyRegion

        public COM001_138()
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
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Search_List();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Search_List()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("LOCATION_PRDT_TYPE_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = Util.NVC(cboArea.SelectedValue.ToString()) == "" ? null : cboArea.SelectedValue.ToString();
            dr["EQSGID"] = Util.NVC(cboLine.SelectedValue.ToString()) == "" ? null : cboLine.SelectedValue.ToString();
            dr["LOCATION_PRDT_TYPE_CODE"] = Util.NVC(cboType.SelectedValue.ToString()) == "" ? null : cboType.SelectedValue.ToString();
            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_SEL_LOCATION_INFO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                Util.GridSetData(dgLoctList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dgLoctList.Rows.Count));
                //캔버스에 조회된 로케이션 표시
                drawingFixCanvas.Children.Clear();
                drawingCanvas.Children.Clear();
                fn_Set_Location_Set(dtResult);
            });
        }

        private void fn_Set_Location_Set(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow ro in dt.Rows)
                {

                    if (!string.IsNullOrEmpty(ro["CODI_VALUE"].ToString()))
                    {
                        string sLocation = ro["LOCATION_ID"].ToString();
                        Geometry geoPath = Geometry.Parse(ro["CODI_VALUE"].ToString());
                        PathGeometry pathGeometry = geoPath.GetFlattenedPathGeometry();
                        PathFigure pathFigure = new PathFigure();
                        pathFigure = pathGeometry.Figures[0];
                        PointCollection pointList = GetFlattenedPointList(pathFigure);
                        Polygon newPolygon = new Polygon();
                        newPolygon.Name = sLocation;
                        newPolygon.Stroke = Brushes.DarkGray;
                        newPolygon.StrokeThickness = 2;
                        newPolygon.Fill = Brushes.Red;
                        newPolygon.Opacity = 0.3;
                        for (int i = 0; i < pointList.Count; i++)
                        {
                            newPolygon.Points.Add(pointList[i]);
                        }
                        this.drawingFixCanvas.Children.Insert(0, newPolygon);
                    }
                }
            }
        }
        private void InitCombo()
        {
            Set_Combo();
            //Set_Combo_Type(cboType);
        }

        private void Set_Combo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cboAreaChild = { cboLine };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sFilter: sFilter);
           

            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboType };

            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent, cbChild: cboLineChild);

            String[] sFilterType = { LoginInfo.LANGID, "RTLS_PRD_TYPE" };
            //C1ComboBox[] cboTypeParent = { cboLine };
            _combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODES", sFilter: sFilterType);
            cboArea.SelectedValue = "";

        }

        void dgLocList_Checked(object sender, RoutedEventArgs e)
        {
            try
            {

                int iListIndex = Util.gridFindDataRow(ref dgLoctList, "CHK", "True", false);


                if (iListIndex == -1)
                {
                    return;
                }

                this.drawingCanvas.Children.Clear();



                if (Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "CODI_VALUE")) == null || Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "CODI_VALUE")) == "")
                {
                    return;
                }
                locPath = new Path();
                locGeometry = new PathGeometry();
                locPath.Stroke = Brushes.Black;
                locPath.StrokeThickness = 2;
                locPath.Fill = Brushes.Red;
                locPath.Opacity = 0.6;
                locGeometry = Geometry.Parse(Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "CODI_VALUE")));
                locPath.Data = locGeometry;
                this.drawingCanvas.Children.Add(locPath);
                editPolygon = null;

            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                
            }

        }

        void dgLocList_Unchecked(object sender, RoutedEventArgs e)
        {

        }
        #endregion


        #region Canvas Event

        private void btnTagetInit_Click(object sender, RoutedEventArgs e)
        {
            int iListIndex = Util.gridFindDataRow(ref dgLoctList, "CHK", "True", false);
            if (iListIndex > -1)
            {
                this.drawingCanvas.Children.Clear();
                editDesignType = DesignType.Create;
                _Polyline = null;
                fn_Set_drawingFixCanvas(iListIndex);
            }
            else
            {
                Util.AlertInfo("SFU4136");
            }

        }
        private void btnTagetEdit_Click(object sender, RoutedEventArgs e)
        {
            int iListIndex = Util.gridFindDataRow(ref dgLoctList, "CHK", "True", false);
            if (iListIndex == -1)
            {
                Util.AlertInfo("SFU4136");
            }
            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "CODI_VALUE"))))
            {
                Util.AlertInfo("SFU9001");
                return;
            }
            if (editDesignType != DesignType.Create)
            {
                if (locGeometry != null)
                {
                    this.drawingCanvas.Children.Clear();
                    PathGeometry pathGeometry = this.locGeometry.GetFlattenedPathGeometry();
                    PathFigure pathFigure = new PathFigure();
                    pathFigure = pathGeometry.Figures[0];
                    PointCollection pointList = GetFlattenedPointList(pathFigure);
                    editPolygon = new Polygon();
                    for (int i = 0; i < pointList.Count; i++)
                    {
                        editPolygon.Points.Add(pointList[i]);
                        Ellipse ellipse = new Ellipse();
                        ellipse.Width = 10;
                        ellipse.Height = 10;
                        ellipse.Stroke = Brushes.Red;
                        ellipse.StrokeThickness = 1;
                        ellipse.Fill = Brushes.Red;
                        ellipse.MouseLeftButtonDown += ellipse_MouseLeftButtonDown;

                        Canvas.SetLeft(ellipse, pointList[i].X - ellipse.Width / 2);
                        Canvas.SetTop(ellipse, pointList[i].Y - ellipse.Height / 2);
                        this.drawingCanvas.Children.Add(ellipse);
                    }
                    editPolygon.Stroke = Brushes.Blue;
                    editPolygon.StrokeThickness = 2;
                    this.drawingCanvas.Children.Insert(0, editPolygon);
                    editDesignType = DesignType.Edit;
                    fn_Set_drawingFixCanvas(iListIndex);
                }
            }
        }

        private void fn_Set_drawingFixCanvas(int iListIndex)
        {
            bool isChecked = false;
            foreach (UIElement Element in drawingFixCanvas.Children)
            {
                if (Element is Polygon)
                {
                    selectPolygon = (Polygon)Element;
                    if (selectPolygon.Name.Equals(Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "LOCATION_ID"))))
                    {
                        drawingFixCanvas.Children.Remove(Element);
                        isChecked = true;
                        break;
                    }
                }
            }
            if(!isChecked)
            {
                selectPolygon = new Polygon();
                selectPolygon.Name = Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "LOCATION_ID"));
                selectPolygon.Stroke = Brushes.DarkGray;
                selectPolygon.StrokeThickness = 2;
                selectPolygon.Fill = Brushes.Red;
                selectPolygon.Opacity = 0.3;
            }
        }

        private void btnTagetSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (editDesignType != DesignType.Edit)
                {
                    return;
                }
                foreach (UIElement element in this.drawingCanvas.Children)
                {
                    Polygon polygon = element as Polygon;

                    if (polygon == null)
                    {
                        continue;
                    }
                    this.editPolygon = polygon;
                }
                if (this.editPolygon != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3533"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                    {
                        if (sresult != MessageBoxResult.OK)
                        {
                            return;
                        }
                        var path = new System.Windows.Shapes.Path();
                        path.Data = this.editPolygon.RenderedGeometry;
                        int iListIndex = Util.gridFindDataRow(ref dgLoctList, "CHK", "True", false);
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LOCATIONID", typeof(string));
                        RQSTDT.Columns.Add("SHOPID", typeof(string));
                        RQSTDT.Columns.Add("LOCATION_TYPE_CODE", typeof(string));
                        RQSTDT.Columns.Add("LOCATIONNAME", typeof(string));
                        RQSTDT.Columns.Add("PSTN_MIN_X", typeof(string));
                        RQSTDT.Columns.Add("PSTN_MIN_Y", typeof(string));
                        RQSTDT.Columns.Add("PSTN_MAX_X", typeof(string));
                        RQSTDT.Columns.Add("PSTN_MAX_Y", typeof(string));
                        RQSTDT.Columns.Add("INSUSER", typeof(string));
                        RQSTDT.Columns.Add("LOCATION_PRDT_TYPE_CODE", typeof(string));
                        RQSTDT.Columns.Add("CODI_VALUE", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["LOCATIONID"] = Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "LOCATION_ID"));
                        dr["SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "SHOPID"));
                        dr["LOCATION_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "LOCATION_TYPE_CODE"));
                        dr["LOCATIONNAME"] = Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "LOCATION_NAME"));
                        dr["PSTN_MIN_X"] = string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "PSTN_MIN_X"))) ? "0" : Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "PSTN_MIN_X"));
                        dr["PSTN_MIN_Y"] = string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "PSTN_MIN_Y"))) ? "0" : Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "PSTN_MIN_Y"));
                        dr["PSTN_MAX_X"] = string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "PSTN_MAX_X"))) ? "0" : Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "PSTN_MAX_X"));
                        dr["PSTN_MAX_Y"] = string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "PSTN_MAX_Y"))) ? "0" : Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "PSTN_MAX_Y"));
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["LOCATION_PRDT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLoctList.Rows[iListIndex].DataItem, "LOCATION_PRDT_TYPE_CODE"));
                        dr["CODI_VALUE"] = path.Data.ToString();
                        RQSTDT.Rows.Add(dr);

                        loadingIndicator.Visibility = Visibility.Visible;
                        new ClientProxy().ExecuteService("DA_RTLS_INS_TB_RTLS_LOCATION", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }
                            Util.MessageInfo("SFU3532");
                            DataTableConverter.SetValue(dgLoctList.Rows[iListIndex].DataItem, "CODI_VALUE", path.Data.ToString());
                            selectPolygon.Points = editPolygon.Points;
                            drawingFixCanvas.Children.Add(selectPolygon);
                            locGeometry = path.Data;
                            drawingCanvas.Children.Clear();
                            editDesignType = DesignType.End;
                        });
                    });
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }

        }


        private void designerCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //신규생성이면
            if (editDesignType.Equals(DesignType.Create))
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    EndPolyLine();
                    return;
                }

                Point mousePoint = e.GetPosition(this.designerCanvas);

                if (this._Polyline == null)
                {
                    this._Polyline = new Polyline();

                    this._Polyline.Stroke = Brushes.Red;
                    this._Polyline.StrokeThickness = 1;
                    this._Polyline.StrokeDashArray = new DoubleCollection();

                    this._Polyline.StrokeDashArray.Add(5);
                    this._Polyline.StrokeDashArray.Add(5);
                    this._Polyline.Points.Add(mousePoint);

                    this.drawingCanvas.Children.Add(this._Polyline);
                }
                this._Polyline.Points.Add(mousePoint);

                Ellipse ellipse = new Ellipse();
                ellipse.Width = 10;
                ellipse.Height = 10;
                ellipse.Stroke = Brushes.Red;
                ellipse.StrokeThickness = 1;
                ellipse.Fill = Brushes.Red;
                ellipse.ToolTip = _Polyline.Points.Count.ToString();
                ellipse.MouseLeftButtonDown += ellipse_MouseLeftButtonDown;

                Canvas.SetLeft(ellipse, mousePoint.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, mousePoint.Y - ellipse.Height / 2);
                this.drawingCanvas.Children.Add(ellipse);
            }
            else
            {
                if (this.editPolygon == null || this.editEllipse == null)
                {
                    return;
                }
                this.LastPosition = e.GetPosition(designerCanvas);
                this.drawingCanvas.CaptureMouse();
            }
        }

        private void designerCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = e.GetPosition(this.designerCanvas);
            XYposition.Text = string.Format("{0} , {1}", mousePoint.X.ToString("#,###"), mousePoint.Y.ToString("#,###"));

            if (editDesignType.Equals(DesignType.Create))
            {
                if (this._Polyline == null)
                {
                    return;
                }
                this._Polyline.Points[this._Polyline.Points.Count - 1] = mousePoint;
            }
            else
            {
                if (editDesignType.Equals(DesignType.Edit))
                {
                    if (editEllipse != null)
                    {
                        Set_LineMove(e);
                    }
                }
            }
        }

        private void designerCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            editEllipse = null;
            this.drawingCanvas.ReleaseMouseCapture();
        }

        private void ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (editDesignType.Equals(DesignType.Edit))
            {
                Ellipse ellipse = sender as Ellipse;
                editEllipse = ellipse;

                foreach (UIElement element in this.drawingCanvas.Children)
                {
                    Polygon polygon = element as Polygon;

                    if (polygon == null)
                    {
                        continue;
                    }
                    Point newPoint = new Point(Canvas.GetLeft(ellipse) + (ellipse.Width / 2), Canvas.GetTop(ellipse) + (ellipse.Height / 2));
                    editPolygonPartIndex = Get_PolygonPartIndex(polygon, newPoint);
                }
            }
        }



        #endregion
        #region Canvas Method
        private void EndPolyLine()
        {
            if (this._Polyline == null)
            {
                return;
            }
            editDesignType = DesignType.Edit; // 추가 생성 불가.
            if (this._Polyline.Points.Count > 3)
            {
                this._Polyline.Points.RemoveAt(this._Polyline.Points.Count - 1);

                Polygon newPolygon = new Polygon();

                newPolygon.Stroke = Brushes.Blue;
                newPolygon.StrokeThickness = 2;
                newPolygon.Points = this._Polyline.Points;
                this.drawingCanvas.Children.Insert(0, newPolygon);
                this.editPolygon = newPolygon;
            }
            else
            {
                Util.Alert("SFU8423");
                btnTagetInit_Click(null, null);
            }

            this.drawingCanvas.Children.Remove(this._Polyline);

        }
        private int Get_PolygonPartIndex(Polygon polygon, Point newPoint)
        {
            const double hit_radius = 2;
            int num_points = polygon.Points.Count;
            int item_index = -1;
            for (int i = 0; i < num_points; i++)
            {
                if (DistanceToPoint(newPoint, polygon.Points[i]) < hit_radius)
                {
                    item_index = i;
                }
            }
            return item_index;
        }
        private void Set_LineMove(MouseEventArgs e)
        {
            Point mousePoint = e.GetPosition(this.designerCanvas);

            double deltaX = mousePoint.X - this.LastPosition.X;
            double deltaY = mousePoint.Y - this.LastPosition.Y;

            Point newPoint = new Point
            (
                this.editPolygon.Points[this.editPolygonPartIndex].X + deltaX,
                this.editPolygon.Points[this.editPolygonPartIndex].Y + deltaY
            );

            this.editPolygon.Points[this.editPolygonPartIndex] = newPoint;
            Canvas.SetLeft(editEllipse, newPoint.X - (editEllipse.Width / 2));
            Canvas.SetTop(editEllipse, newPoint.Y - (editEllipse.Height / 2));

            this.LastPosition = mousePoint;
        }
        public double DistanceToPoint(Point from_point, Point to_point)
        {
            double dx = to_point.X - from_point.X;
            double dy = to_point.Y - from_point.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        #endregion





        /// <summary>
        /// 평탄화 포인트 리스트 구하기
        /// </summary>
        /// <param name="pathFigure">패스 피겨</param>
        /// <returns>평탄화 포인트 리스트</returns>
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

        private void btnTagetAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string MAINFORMPATH = "LGC.GMES.MES.COM001";
                string MAINFORMNAME = "COM001_138_INS_LOCATION";

                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                IWorkArea workArea = obj as IWorkArea;
                workArea.FrameOperation = FrameOperation;

                C1Window popup = obj as C1Window;
                if (popup != null)
                {
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void designerCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            XYposition.Text = string.Format("{0} , {1}", 0,0);
        }
    }
}
