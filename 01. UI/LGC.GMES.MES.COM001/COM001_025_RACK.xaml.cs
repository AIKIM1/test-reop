/*************************************************************************************
 Created Date : 2016.11.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.14  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_025_RACK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;

        public COM001_025_RACK()
        {
            InitializeComponent();
        }

        public COM001_025_RACK(string sRack, string sText, string shipToid = "")
        {
            InitializeComponent();

            string sBizName = "DA_PRD_SEL_STOCK_BY_RACK_LIST";

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("RACK_ID", typeof(string));
                if (sText == "PackElec")
                {
                    IndataTable.Columns.Add("AREAID", typeof(string));
                    IndataTable.Columns.Add("SHIPTOID", typeof(string));
                }

                DataRow Indata = IndataTable.NewRow();
                Indata["RACK_ID"] = sRack.ToString();
                if (sText == "PackElec")
                {
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["SHIPTOID"] = shipToid;
                }
                IndataTable.Rows.Add(Indata);


                if (sText == "COMM_CONV")
                {
                    switch (sRack)
                    {
                        case "2PX984":
                            sBizName = "DA_PRD_SEL_STOCK_BY_RACK_COMM_IN";
                            break;
                        case "2PX983":
                            sBizName = "DA_PRD_SEL_STOCK_BY_RACK_CONV_IN";
                            break;
                        case "2PX982":
                            sBizName = "DA_PRD_SEL_STOCK_BY_RACK_COMM_OUT";
                            break;
                        case "2PX981":
                            sBizName = "DA_PRD_SEL_STOCK_BY_RACK_CONV_OUT";
                            break;
                        default:
                            break;
                    }
                }
                else if (sText == "PackElec")
                {
                    sBizName = "DA_PRD_SEL_STOCK_BY_RACK_LIST_PACKELEC";
                }
                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", IndataTable);


                if (dtMain.Rows.Count > 0)
                {
                    dgList.BeginEdit();
                    dgList.ItemsSource = DataTableConverter.Convert(dtMain);
                    dgList.EndEdit();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

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
        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //사용자 권한별로 버튼 숨기기
        //    List<Button> listAuth = new List<Button>();
        //    //listAuth.Add(btnSave);
        //    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        //    //여기까지 사용자 권한별로 버튼 숨기기
        //}

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

        }      
        #endregion

        #region Mehod     

        #endregion


    }
}
