/*************************************************************************************
  Created Date : 2020.06.10
      Creator : 염규범
   Decription : 공정의 작업정보
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.10  염규범 : Initial Created.
**************************************************************************************/
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_062_PROCESSINFO : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_062 PACK001_062;

        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        private string sLineid = string.Empty;
        private string sLineName = string.Empty;
        private string sPcsgid = string.Empty;
        private string sProcid = string.Empty;
        private string sProcName = string.Empty;
        private string sEqptid = string.Empty;
        private string sEqptName = string.Empty;

        public string EQSGID
        {
            get
            {
                return sLineid;
            }

            set
            {
                sLineid = value;
            }
        }

        public string EQSGNAME
        {
            get
            {
                return sLineName;
            }

            set
            {
                sLineName = value;
            }
        }

        public string PROCID
        {
            get
            {
                return sProcid;
            }

            set
            {
                sProcid = value;
            }
        }

        public string PROCNAME
        {
            get
            {
                return sProcName;
            }

            set
            {
                sProcName = value;
            }
        }

        public string EQPTID
        {
            get
            {
                return sEqptid;
            }

            set
            {
                sEqptid = value;
            }
        }

        public string EQPTNAME
        {
            get
            {
                return sEqptName;
            }

            set
            {
                sEqptName = value;
            }
        }

        public string PCSGID
        {
            get
            {
                return sPcsgid;
            }

            set
            {
                sPcsgid = value;
            }
        }

        public PACK001_062_PROCESSINFO()
        {
            InitializeComponent();
        }



        #endregion

        #region Initialize

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            try
            {
                //TimerSetting();
                //timer.IsEnabled = true;
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                //setPlanQty();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {   
            try
            {
                timer.IsEnabled = false;
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //setPlanQty();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        

        #endregion

        #region Mehod

        public void setProcess(DataRow drProcessInfo)
        {
            try
            {

                EQSGID = string.Empty;
                EQSGNAME = string.Empty;
                EQPTID = string.Empty;
                EQPTNAME = string.Empty;
                PROCID = string.Empty;
                PROCNAME = string.Empty;
                PCSGID = string.Empty;

                EQSGID = Util.NVC(drProcessInfo["EQSGID"]);
                EQSGNAME = Util.NVC(drProcessInfo["EQSGNAME"]);
                EQPTID = Util.NVC(drProcessInfo["EQPTID"]);
                EQPTNAME = Util.NVC(drProcessInfo["EQPTNAME"]);
                PROCID = Util.NVC(drProcessInfo["PROCID"]);
                PROCNAME = Util.NVC(drProcessInfo["PROCNAME"]);


                txtSelectedLine.Text = EQSGNAME;
                txtSelectedProcess.Text = PROCNAME;
                txtSelectedEquipment.Text = EQPTNAME;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        #endregion


    }
}
