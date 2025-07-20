/*************************************************************************************
 Created Date : 2024.02.01
      Creator : 
   Decription : UcBaseClasses
--------------------------------------------------------------------------------------
 [Change History]
  2024.02.01  조영대 : Initial Created
**************************************************************************************/

using System.Windows;

namespace LGC.GMES.MES.CMM001.Controls
{
    #region Declaration
    public static class GMES
    {
        public static string Version = "2.0";
    }
    #endregion

    #region Aggregate Extension
    public class AggregateExtension : DependencyObject
    {
        public static readonly DependencyProperty ColumnTypeProperty = DependencyProperty.RegisterAttached("ColumnType", typeof(AggregateColumnType), typeof(AggregateExtension), new PropertyMetadata(AggregateColumnType.SUM));

        public static void SetAggregateColumnType(DependencyObject column, AggregateColumnType value)
        {
            column.SetValue(ColumnTypeProperty, value);
        }

        public static AggregateColumnType GetAggregateColumnType(DependencyObject column)
        {
            return (AggregateColumnType)column.GetValue(ColumnTypeProperty);
        }
    }
    #endregion

    #region Cell Border Extension
    public class BorderLineInfo
    {
        public BorderLineInfo() { }

        public BorderLineInfo(int thickness, System.Windows.Media.Brush brush)
        {
            BorderThickness = thickness;
            BorderBrush = brush;
            IsDot = BorderLineStyle.Line;
        }

        public BorderLineInfo(int thickness, System.Windows.Media.Brush brush, BorderLineStyle isDot)
        {
            BorderThickness = thickness;
            BorderBrush = brush;
            IsDot = isDot;
        }

        public int BorderThickness { get; set; }
        public System.Windows.Media.Brush BorderBrush { get; set; }
        public BorderLineStyle IsDot { get; set; }
    }

    public class CellBorderLineInfo
    {
        public CellBorderLineInfo() { }

        public BorderLineInfo LeftBorderLineInfo { get; set; }
        public BorderLineInfo TopBorderLineInfo { get; set; }
        public BorderLineInfo RightBorderLineInfo { get; set; }
        public BorderLineInfo BottomBorderLineInfo { get; set; }
    }

    public class CellAlertInfo
    {
        public CellAlertInfo() { }

        public System.Windows.Media.Brush Background { get; set; }
        public System.Windows.Media.Animation.RepeatBehavior Repeat { get; set; }
        public int SpeedMillisecond { get; set; }
    }
    #endregion
}