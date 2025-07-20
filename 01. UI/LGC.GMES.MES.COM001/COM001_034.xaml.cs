/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_034 : UserControl, IWorkArea
    {

        private string _Proc = string.Empty;
        private string _WipSeq = string.Empty;
        private string _Prod = string.Empty;

        #region Declaration & Constructor 
        public COM001_034()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


           

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotStatus();
            GetQuality();
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Mehod
        #region [LOT기본정보조회]
        private void GetLotStatus()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.GetCondition(txtLotId, "LOT은 필수입니다.");

                if (dr["LOTID"].Equals("")) return;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPLOT_STATUS", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count < 1)
                {
                    Util.Alert("SFU1386");  //LOT정보가 없습니다.
                    _Proc = "";
                    _WipSeq = "";
                    _Prod = "";
                    return;
                }
                else
                {
                    txtProc.Text = dtRslt.Rows[0]["PROCNAME"].ToString();
                    txtProdId.Text = dtRslt.Rows[0]["PRODID"].ToString();
                    txtProdName.Text = dtRslt.Rows[0]["PRODNAME"].ToString();
                    txtStartDate.Text = dtRslt.Rows[0]["WIPSDTTM"].ToString();
                    txtStatus.Text = dtRslt.Rows[0]["WIPSTAT"].ToString();

                    _Proc = dtRslt.Rows[0]["PROCID"].ToString();
                    _WipSeq = dtRslt.Rows[0]["WIPSEQ"].ToString();
                    _Prod = dtRslt.Rows[0]["PRODID"].ToString();

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #region [검사항목조회]
        private void GetQuality()
        {
            try
            {
                if (!_WipSeq.Equals("") && !_WipSeq.Equals(String.Empty))
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("PROCID", typeof(string));
                    dtRqst.Columns.Add("PRODID", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(string));
                    dtRqst.Columns.Add("WIPSEQ", typeof(Int16));


                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PROCID"] = _Proc;
                    dr["PRODID"] = _Prod;
                    dr["LOTID"] = Util.GetCondition(txtLotId, "LOT은필수입니다.");
                    dr["WIPSEQ"] = _WipSeq;

                    if (dr["LOTID"].Equals("")) return;

                    dtRqst.Rows.Add(dr);

                    //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "OUTDATA", dtRqst);
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_PROD_LOT", "INDATA", "OUTDATA", dtRqst);

                    //Util.gridClear(dgQualityInfo);
                    //dgQualityInfo.ItemsSource = DataTableConverter.Convert(dtRslt);

                    Util.GridSetData(dgQualityInfo, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion


    }
}
