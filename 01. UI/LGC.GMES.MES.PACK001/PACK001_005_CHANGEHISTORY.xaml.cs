/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack Lot이력- LOT변경 이력 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2019.12.11  손우석 SM CMI Pack 메시지 다국어 처리 요청
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_005_CHANGEHISTORY : C1Window, IWorkArea
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



        public PACK001_005_CHANGEHISTORY()
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
                        txtSearchLot.Text = Util.NVC(dtText.Rows[0]["LOTID"]);

                        setDgChangeHistory();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtSearchLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    setDgChangeHistory();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgChangeHistory);
                }
                catch (Exception ex)
                {
                    Util.Alert(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion

        #region Mehod
        private void setDgChangeHistory()
        {
            try
            {
                //DA_PRD_SEL_MTRL_CHANGE_HIST
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtSearchLot.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_CHANGE_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                dgChangeHistory.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                //2019.12.11
                //Util.AlertByBiz("DA_PRD_SEL_MTRL_CHANGE_HIST", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
