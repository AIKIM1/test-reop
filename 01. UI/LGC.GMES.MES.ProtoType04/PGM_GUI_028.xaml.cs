/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_028 : UserControl
    {
        #region Declaration & Constructor 
        public PGM_GUI_028()
        {

            InitializeComponent();
        }



        #endregion

        #region Initialize

        #endregion

        #region Event

        #endregion

        #region Mehod


        #endregion

        #region PopUp

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_RUNSTART";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_CONFIRM";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }



        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_WAITLOT";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnQuality_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_QUALITY";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnEqpRemark_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_EQPREMARK";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnAbnormal_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_ABNORMAL";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }


        private void btnOutCreate_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_TRAY_CREATE";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }
        private void btnOutMove_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_TRAY_MOVE";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_028_TRAY_CELL";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {

        }










        #endregion


    }
}
