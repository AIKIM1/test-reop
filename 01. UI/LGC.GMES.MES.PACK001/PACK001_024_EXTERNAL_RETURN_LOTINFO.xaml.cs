using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_024_EXTERNAL_RETURN_LOTINFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_024_EXTERNAL_RETURN_LOTINFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_024_EXTERNAL_RETURN_LOTINFO()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        txtTargetBMALot.Text = Util.NVC(dtText.Rows[0]["LOTID"]);
                        string sRTN_SALES_ORD_NO = Util.NVC(dtText.Rows[0]["RTN_SALES_ORD_NO"]);
                        setOCOPCMAListInfo(txtTargetBMALot.Text, sRTN_SALES_ORD_NO);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Mehod

        private void setOCOPCMAListInfo(string pLOTID, string pRTN_SALES_ORD_NO)
        {
            try
            {
                viewClear();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("RTN_SALES_ORD_NO", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = pLOTID;
                dr["RTN_SALES_ORD_NO"] = pRTN_SALES_ORD_NO;
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_AS_OCOP_DETL_INFO", "INDATA", "OUTDATA", INDATA);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dgCMALotList.ItemsSource = DataTableConverter.Convert(dtResult);
                }
                else
                {
                    Util.MessageInfo("SFU2816");  // 조회 결과가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void viewClear()
        {
            dgCMALotList.ItemsSource = null;
        }

        
        #endregion


    }
}
