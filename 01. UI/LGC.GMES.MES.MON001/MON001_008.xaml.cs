/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.MON001
{
    public partial class MON001_008 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public MON001_008()
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

            //동
            _combo.SetCombo(cboShop, CommonCombo.ComboStatus.ALL);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        private void dgResult1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult1.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("SHOPID", typeof(string));
                    dtRqst.Columns.Add("JUDGE_FLAG", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgResult1.Rows[cell.Row.Index].DataItem, "SHOPID"));

                    if (cell.Column.Name == "TOT_CNT")
                    { dr["JUDGE_FLAG"] = null; }
                    else if (cell.Column.Name == "OK_CNT")
                    { dr["JUDGE_FLAG"] = "OK"; }
                    else if (cell.Column.Name == "NG_CNT")
                    { dr["JUDGE_FLAG"] = "NG"; }

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_NO_RECIPE_DETAIL", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgResult2, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetResult();
        }

        #endregion

        #region Mehod
        public void GetResult()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SHOPID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SHOPID"] = Util.GetCondition(cboShop, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_NO_RECIPE", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgResult1, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [초기화]
        private void ClearValue()
        {
            Util.gridClear(dgResult1);
            Util.gridClear(dgResult2);
        }
        #endregion

        #endregion
    }
}
