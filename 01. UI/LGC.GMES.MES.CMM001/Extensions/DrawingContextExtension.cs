using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Extensions
{
    /*************************************************************************************
     Created Date : 2020.12.07
          Creator : 조영대
       Decription : Drawing Extension
    --------------------------------------------------------------------------------------
     [Change History]
      2020.12.07  조영대 : Initial Created.
    **************************************************************************************/
    public static class DrawingContextExtension
    {

        #region 다각형 그리기

        /// <summary>
        /// 다각형 그리기
        /// </summary>
        /// <param name="drawingContext">그리기 컨텍스트</param>
        /// <param name="brush">브러시</param>
        /// <param name="pen">펜</param>
        /// <param name="pointArray">포인트 배열</param>
        /// <param name="fillRule">채우기 규칙</param>

        public static void DrawPolygon(this DrawingContext drawingContext, Brush brush, Pen pen, Point[] pointArray, FillRule fillRule)
        {
            drawingContext.DrawPolygonOrPolyline(brush, pen, pointArray, fillRule, true);
        }
        #endregion

        #region 다각선 그리기

        /// <summary>
        /// 다각선 그리기
        /// </summary>
        /// <param name="drawingContext">그리기 컨텍스트</param>
        /// <param name="brush">브러시</param>
        /// <param name="pen">펜</param>
        /// <param name="pointArray">포인트 배열</param>
        /// <param name="fillRule">채우기 규칙</param>
        public static void DrawPolyline(this DrawingContext drawingContext, Brush brush, Pen pen, Point[] pointArray, FillRule fillRule)
        {
            drawingContext.DrawPolygonOrPolyline(brush, pen, pointArray, fillRule, false);
        }
        #endregion

        #region 다각형 또는 다각선 그리기


        /// <summary>
        /// 다각형 또는 다각선 그리기
        /// </summary>
        /// <param name="drawingContext">그리기 컨텍스트</param>
        /// <param name="brush">브러시</param>
        /// <param name="pen">펜</param>
        /// <param name="pointArray">포인트 배열</param>
        /// <param name="fillRule">채우기 규칙</param>
        /// <param name="drawPolygon">다각형 그리기 여부</param>
        private static void DrawPolygonOrPolyline(this DrawingContext drawingContext, Brush brush, Pen pen, Point[] pointArray,
            FillRule fillRule, bool drawPolygon)
        {
            StreamGeometry streamGeometry = new StreamGeometry();

            streamGeometry.FillRule = fillRule;

            using (StreamGeometryContext context = streamGeometry.Open())
            {
                context.BeginFigure(pointArray[0], true, drawPolygon);

                context.PolyLineTo(pointArray.Skip(1).ToArray(), true, false);
            }

            drawingContext.DrawGeometry(brush, pen, streamGeometry);
        }
        #endregion

        #region 문자열 그리기
        public static void DrawString(this DrawingContext drawingContext,
            Point originPoint,
            string text,
            TextAlignment textAlignment,
            Brush brush,
            FontFamily fontFamily,
            FontStyle fontStyle,
            FontWeight fontWeight,
            FontStretch fontStretch,
            double emSize,            
            VerticalAlignment verticalAlignment,
            TextAlignment horizontalAlignment,
            double rotateAngle)
        {
            Typeface typeFace = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);

            FormattedText formattedText = new FormattedText
            (
                text,
                System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeFace,
                emSize,
                brush
            );

            formattedText.TextAlignment = textAlignment;

            double width = formattedText.Width - formattedText.OverhangLeading;
            double height = formattedText.Height;

            TranslateTransform translateTransform1 = new TranslateTransform();

            translateTransform1.Y = -height / 2;

            if ((textAlignment == TextAlignment.Left) || (textAlignment == TextAlignment.Justify))
            {
                translateTransform1.X = -width / 2;
            }
            else if (textAlignment == TextAlignment.Right)
            {
                translateTransform1.X = width / 2;
            }
            else
            {
                translateTransform1.X = 0;
            }

            RotateTransform rotateTransform = new RotateTransform(rotateAngle);

            Rect rectangle = new Rect(0, 0, width, height);

            if (textAlignment == TextAlignment.Center)
            {
                rectangle.X -= width / 2;
            }
            else if (textAlignment == TextAlignment.Right)
            {
                rectangle.X -= width;
            }

            Rect rotateRectangle = rotateTransform.TransformBounds(rectangle);

            TranslateTransform translateTransform2 = new TranslateTransform(originPoint.X, originPoint.Y);

            if (horizontalAlignment == TextAlignment.Left)
            {
                translateTransform2.X += rotateRectangle.Width / 2;
            }
            else if (horizontalAlignment == TextAlignment.Right)
            {
                translateTransform2.X -= rotateRectangle.Width / 2;
            }

            if (verticalAlignment == VerticalAlignment.Top)
            {
                translateTransform2.Y += rotateRectangle.Height / 2;
            }
            else if (verticalAlignment == VerticalAlignment.Bottom)
            {
                translateTransform2.Y -= rotateRectangle.Height / 2;
            }

            drawingContext.PushTransform(translateTransform2);
            drawingContext.PushTransform(rotateTransform);
            drawingContext.PushTransform(translateTransform1);

            drawingContext.DrawText(formattedText, new Point(0, 0));
            
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.Pop();

            Rect transformRectangle = translateTransform2.TransformBounds
            (
                rotateTransform.TransformBounds
                (
                    translateTransform1.TransformBounds(rectangle)
                )
            );

            //Pen customPen = new Pen(Brushes.Blue, 1);

            //customPen.DashStyle = new DashStyle(new double[] { 5, 5 }, 0);

            //drawingContext.DrawRectangle(null, customPen, transformRectangle);
        }
        #endregion

        #region Rectangle 중앙 찾기
        public static Point GetCenterPoint(this Rect rect)
        {
            return new Point(rect.Left + (rect.Width / 2), rect.Top + (rect.Height / 2));
        }
        #endregion
    }
}
