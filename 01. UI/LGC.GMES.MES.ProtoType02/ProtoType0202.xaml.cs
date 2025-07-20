/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType02
{
    public partial class ProtoType0202 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtMain = new DataTable();
        DataTable dtDetail = new DataTable();
        DataRow newRow = null;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ProtoType0202()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        //Button =======================================================================================================
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
           
        }

        //GridSplitter =================================================================================================
        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            FrameOperation.PrintFrameMessage(" Right :" + grdBottomLeft.ActualWidth.ToString() + " Right :" + grdBottomRight.ActualWidth.ToString());
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        #endregion

        #region Mehod



        #endregion
    }
}
