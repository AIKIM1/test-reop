using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Printing;
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_RMTRL_PALLET_PRINT : C1Window, IWorkArea
    {
        private int iCnt = 1;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_RMTRL_PALLET_PRINT()
        {
            InitializeComponent();
            object[] tmps = C1WindowExtension.GetParameters(this);
            dicParam = tmps[0] as Dictionary<string, string>;

        }
        public CMM_RMTRL_PALLET_PRINT(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;
                if (dicParam.ContainsKey("PALLETID"))
                {
                    txtPALLETID.Text = dicParam["PALLETID"];
                    txtPALLETID2.Text = dicParam["PALLETID"];
                    txtBARCODE_PALLETID.Text = "*" + dicParam["PALLETID"] + "*";
                } 
                if (dicParam.ContainsKey("MTGRNAME")) txtMTGRNAME.Text = dicParam["MTGRNAME"];
                if (dicParam.ContainsKey("MTRLDESC")) txtMTRLDESC.Text = dicParam["MTRLDESC"];
                if (dicParam.ContainsKey("MLOTID")) txtMLOTID.Text = dicParam["MLOTID"];
                if (dicParam.ContainsKey("INSDTTM")) txtINSDTTM.Text = dicParam["INSDTTM"];

                grPrint.Margin = new Thickness(0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
                
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog dialog = new PrintDialog();

                this.Width = 275;
                this.Height = 255;

                if (LoginInfo.CFG_THERMAL_PRINT.Rows.Count > 0 && !string.IsNullOrEmpty(LoginInfo.CFG_THERMAL_PRINT.Rows[0]?[CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME].ToString()))
                    dialog.PrintQueue = new PrintQueue(new PrintServer(), LoginInfo.CFG_THERMAL_PRINT.Rows[0]?[CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME].ToString());

                dialog.PrintVisual(grPrint, "GMES PRINT");
                this.Close();
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1419"));        //Print 실패
                this.DialogResult = MessageBoxResult.Cancel;
                //Util.MessageException(ex);
            }
        }  
    }
}
