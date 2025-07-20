/*************************************************************************************
 Created Date : 2023.02.10
      Creator : seonjun
   Decription : 자재 Box 이력 조회 화면
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR                    Description...
  2023.02.10      seonjun                            Initial Created.
***************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_040 : UserControl, IWorkArea
    {
        #region #1. Member Variable Lists...
        private PACK003_040_DataHelper dataHelper = new PACK003_040_DataHelper();

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
          
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion #1. 

        #region #2. Declaration & Constructor
        public PACK003_040()
        {
            InitializeComponent();
        }
        #endregion #2. Declaration & Constructor

        #region #3. UserControl_Loaded
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                 
                this.txtBoxID.Clear();
                this.txtMtrlID.Clear();
                Util.gridClear(dgList); 

                txtBoxID.Focus();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion #3. UserControl_Loaded
          
        #region #5. Event  
        /// <summary>
        /// 자재 Box 보류적재 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgList);
            this.SearchProcess();
        } 
        #endregion #5. Event

        #region #7. Function 공통 조회 함수 모음
        // 조회 - 
        private void SearchProcess()
        {
            this.loadingIndicator.Visibility = Visibility.Visible;

            try
            {
                string sEqsgID = string.Empty;
                string sRackID = string.Empty;
                string sRtnStateCode = string.Empty;
                string sBoxID = string.Empty;
                string sMtrlID = string.Empty;
 

                           //기간은 1년으로 함
                DataTable dtTp2 = this.dataHelper.GetBoxHistoryData(this.txtBoxID.Text.Trim(), this.txtReqNo.Text.Trim(), this.txtMtrlID.Text.Trim(), DateTime.Today.AddYears(-1), DateTime.Today.AddDays(2));

                if (CommonVerify.HasTableRow(dtTp2))
                {
                    Util.GridSetData(this.dgList, dtTp2, FrameOperation); 
                    //PackCommon.SearchRowCount(ref this.tbGrdMainTp2Cnt, this.dgList.GetRowCount());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion #7. Function 공통 조회 함수 모음
    }

    #region #999. DataHelper (Biz 호출)
    internal class PACK003_040_DataHelper
    {
        #region #999-1. Member Variable Lists...
        #endregion #999-1. Member Variable Lists...

        #region #999-2. Constructor
        internal PACK003_040_DataHelper()
        {
        }
        #endregion #999-2. Constructor

        #region #999-3. Member Function Lists... 

        /// <summary>
        /// 자재 Box History 조회
        /// </summary>
        /// <param name="REPACK_BOX_ID">Box ID</param>
        /// <param name="REQ_NO">요청번호</param>
        /// <param name="MTRLID">자재코드</param>
        /// <param name="dteFromDate"></param>
        /// <param name="dteToDate"></param>
        /// <returns></returns>
        internal DataTable GetBoxHistoryData(string REPACK_BOX_ID, string REQ_NO, string MTRLID, DateTime dteFromDate, DateTime dteToDate)
        {
            {
                string bizRuleName = "DA_MTRL_SEL_TB_SFC_PROD_RACK_MTRL_BOX_STCK_RTN_HIS";

                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                try
                {
                    dtRQSTDT.Columns.Add("LANGID", typeof(string));
                    dtRQSTDT.Columns.Add("REPACK_BOX_ID", typeof(string));
                    dtRQSTDT.Columns.Add("REQ_NO", typeof(string));
                    dtRQSTDT.Columns.Add("MTRLID", typeof(string));
                    dtRQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                    dtRQSTDT.Columns.Add("END_DTTM", typeof(DateTime));

                    DataRow drRQSTDT = dtRQSTDT.NewRow();
                    drRQSTDT["LANGID"] = LoginInfo.LANGID;

                    if (!string.IsNullOrEmpty(REPACK_BOX_ID)) drRQSTDT["REPACK_BOX_ID"] = REPACK_BOX_ID;
                    if (!string.IsNullOrEmpty(REQ_NO)) drRQSTDT["REQ_NO"] = REQ_NO;
                    if (!string.IsNullOrEmpty(MTRLID)) drRQSTDT["MTRLID"] = MTRLID;

                    drRQSTDT["FROM_DTTM"] = dteFromDate;
                    drRQSTDT["END_DTTM"] = dteToDate;

                    dtRQSTDT.Rows.Add(drRQSTDT);

                    dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

                return dtRSLTDT;
            }
        }
        #endregion #999-3. Member Function Lists...
    }
    #endregion #999. DataHelper (Biz 호출)
}