/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_001 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public PGM_GUI_001()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void button_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ProtoType04.ReportSample rs = new LGC.GMES.MES.ProtoType04.ReportSample();

            rs.Show();
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            

            //if (print != null)
            //{

                object[] Parameters = new object[1];

                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                //dicParam.Add("PROC", Process.STACKING_FOLDING);
                dicParam.Add("reportName", "Fold");
                dicParam.Add("LOTID", "LOT212");
                dicParam.Add("QTY", "123");
                dicParam.Add("MAGID", "MAG123");
                dicParam.Add("MAGIDBARCODE", "MAG123");
                dicParam.Add("LARGELOT", "LARGELOT");
                dicParam.Add("MODEL", "MODEL123");
                dicParam.Add("REGDATE", "2016-08-08");
                dicParam.Add("EQPTNO", "EQP111");
                dicParam.Add("TITLEX", "TITLEX123");

                //Parameters[0] = dicParam;
                //C1WindowExtension.SetParameters(print, Parameters);

                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT(dicParam);
                print.FrameOperation = FrameOperation;

                this.Dispatcher.BeginInvoke(new Action(() => print.ShowModal()));
            //}


        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ProtoType04.SubReportSample rs = new LGC.GMES.MES.ProtoType04.SubReportSample();

            rs.Show();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {

            LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = "AAPKO21"; //lotid
                Parameters[1] = "E2000"; //procid

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                //wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            

            
        }
    }
}
