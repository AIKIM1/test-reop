/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_077 : UserControl
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private PGM_GUI_075_PROCESSINFO window_PROCESSINFO = new PGM_GUI_075_PROCESSINFO();
        public PGM_GUI_077()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //�������� load
            setProcessInfo();
        }

        private void btnProcessSelect_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_075_SELECTPROCESS";

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
            PGM_GUI_075_SELECTPROCESS popup = sender as PGM_GUI_075_SELECTPROCESS;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                DataRow drSelectedProcess = popup.DrSelectedProcessInfo;

                if (drSelectedProcess != null)
                {
                    string sSelectedProcessID = drSelectedProcess["PROCNAME"].ToString();//drSelectedProcess["PROCID"].ToString();
                    string sSelectedLineID = drSelectedProcess["LINEID"].ToString();

                    window_PROCESSINFO.setProcess(sSelectedLineID, sSelectedProcessID);

                    tbTitle.Text = popup.SSelectedProcessTitle;
                }
            }
        }



        #endregion

        #region Mehod

        private void setProcessInfo()
        {
            if (dgWorkInfo.Children.Count == 0)
            {
                window_PROCESSINFO.PGM_GUI_077 = this;
                dgWorkInfo.Children.Add(window_PROCESSINFO);
            }


            //if (dgSub.Children.Count == 0)
            //{
            //    window01.ProtoType0205 = this;
            //    dgSub.Children.Add(window01);
            //}
        }


        #endregion
    }
}
