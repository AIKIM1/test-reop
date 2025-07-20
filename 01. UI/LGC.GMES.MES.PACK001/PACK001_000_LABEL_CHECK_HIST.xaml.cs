/*************************************************************************************
Created Date : 2019.11.18
      Creator : 염규범
   Decription : 라벨 체크 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.18 염규범A : Initial Created.

**************************************************************************************/


using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_000_LABEL_CHECK_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string strLabelCode = string.Empty;
        private string strEqsgID = string.Empty;
        private string strProcID = string.Empty;
        private string strEqptId = string.Empty;
        private string strProdId = string.Empty;
        private string strWoId = string.Empty;
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK001_000_LABEL_CHECK_HIST()
        {
            InitializeComponent();
            
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    strLabelCode = Util.NVC(tmps[0]);
                    strEqsgID    = Util.NVC(tmps[1]);
                    strProdId    = Util.NVC(tmps[2]);
                    // strEqptId    = Util.NVC(tmps[3]);
                    strWoId = Util.NVC(tmps[3]);
                    //strWoId      = Util.NVC(tmps[4]);
                }
                else
                {
                    Util.MessageConfirm("정상적인 접근이 아닙니다.", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            this.Close();
                        }
                    });
                }

                LableIHist(strWoId, strLabelCode, strEqsgID, strProdId);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ EVENT ]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region [ Method ]

        #region [ 라벨 샘플 값 ]
        private void LableIHist(string strWoId, string strLabelCode, string strEqsgId, string strProdId)
            {
                try
                {
                    DataTable INDATA = new DataTable();

                    INDATA.TableName = "RQSTDT";
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("WOID",       typeof(string));
                    INDATA.Columns.Add("LABEL_CODE", typeof(string));
                    INDATA.Columns.Add("EQSGID",     typeof(string));
                    INDATA.Columns.Add("PRODID",     typeof(string));


                    DataRow dr = INDATA.NewRow();

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["WOID"]       = strWoId;
                    dr["LABEL_CODE"] = strLabelCode;
                    dr["EQSGID"]     = strEqsgId;
                    dr["PRODID"]     = strProdId;

                    INDATA.Rows.Add(dr);

                    DataTable dtResult = null;
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABLE_CHK_HIST", "RQSTDT", "RSLTDT", INDATA);

                    if (dtResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dbLabelHist, dtResult, FrameOperation, true);

                    }
                    else
                    {
                        Util.MessageInfo("데이터가 없습니다.");
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }

        #endregion

        #endregion

        private void C1Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int temp = 1;
        }
    }
}

