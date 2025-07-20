using System.Windows.Controls.Primitives;

/// <summary>
/// 9441
/// Canvas Control 이동을 위한 객체
/// 2024-04-04 김선준 생성
/// </summary>
namespace LGC.GMES.MES.ControlsLibrary
{
    public interface IMoveThumb
    {
        double X_CODI { get; set; }
        double Y_CODI { get; set; }
        bool ISMOVE { get; set; }
    }

    public interface IResizeThumb
    {
        double RACK_WIDTH { get; set; }
        double RACK_HEIGHT { get; set; }
    }

    public class MoveThumb : Thumb
    {
        public MoveThumb()
        { 
        }         
    }
}
