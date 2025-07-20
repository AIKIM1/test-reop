/*************************************************************************************
 Created Date : 2023.11.23
      Creator : 이병윤
   Decription : 소형 2동 9,10라인 순환물류 LOT 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.23   이병윤    신규 생성
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_150_LOTID_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor      
   
        private string sLotId = "";       
        private string sTpyId = "";

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        public COM001_150_LOTID_HIST()
        {
            InitializeComponent();
            Loaded += COM001_150_LOTID_HIST_Loaded;
        }
        #endregion
        
        #region Event
        private void COM001_150_LOTID_HIST_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Loaded -= COM001_150_LOTID_HIST_Loaded;
                object[] tmps = C1WindowExtension.GetParameters(this);
                   
                sLotId = tmps[0] as string;
                sTpyId = tmps[1] as string;
                GetLotAll(sTpyId);                   

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Mehod

        public void GetLotAll(string sType)
        {
            try
            {
                string sLOTID = string.Empty;

                if (sType == "BOX")
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("CSTID", typeof(String));

                    DataRow dr2 = RQSTDT.NewRow();
                    dr2["CSTID"] = sLotId;

                    RQSTDT.Rows.Add(dr2);
                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_CURR_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows.Count != 0)
                    {
                        sLOTID = SearchResult.Rows[0]["CURR_LOTID"].ToString();
                    }
                    else
                    { 

                        DataTable SearchResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_WIPHISTORYATTR_LOT", "RQSTDT", "RSLTDT", RQSTDT);
                        if (SearchResult2.Rows.Count != 0)
                        {
                            sLOTID = SearchResult2.Rows[0]["CURR_LOTID"].ToString();
                        }
                        else
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    return;
                                }
                            });
                        }
                    }

                }
                else
                {
                    sLOTID = sLotId;
                }
                GetHistory(sLOTID);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetHistory(string sLot)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                DataRow dr = RQSTDT.NewRow();


                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPACTHISTORY", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgHistory);
                dgHistory.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion      
    }
}
