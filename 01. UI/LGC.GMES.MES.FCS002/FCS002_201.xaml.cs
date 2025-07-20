/*************************************************************************************
 Created Date : 2022.12.08
      Creator : 
   Decription : 특별 Aging 현황
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.08  DEVELOPER : Initial Created.
 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_201 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        string _EqptID01 = string.Empty;
        string _EqptID02 = string.Empty;

        public FCS002_201()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                AgingEqptIDForAREA();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                ShowLoadingIndicator();
                DataTable dtRslt = new DataTable();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("COM_CODE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                DataRow newRow = inDataTable.NewRow();
                newRow["COM_CODE"] = _EqptID01;// 시원하게 하드코딩 되어 있었으나, 동별 공통코드로 등록함
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(newRow);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SPECIAL_AGING_TRAY_LIST_MB", "INDATA", "OUTDATA", inDataTable);
                dgList.ItemsSource = DataTableConverter.Convert(dtRslt);

                //----------------------------------------------------------
                //9동은 1시간 Aging 하나밖에 없다고 함
                //inDataTable.Rows.Clear();
                //dtRslt.Clear();

                //newRow["COM_CODE"] = _EqptID02;// 시원하게 하드코딩 되어 있었으나, 동별 공통코드로 등록함
                //newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                //inDataTable.Rows.Add(newRow);

                //dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SPECIAL_AGING_TRAY_LIST_MB", "INDATA", "OUTDATA", inDataTable);
                //dgList2.ItemsSource = DataTableConverter.Convert(dtRslt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void AgingEqptIDForAREA()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "AGING_RACK_EQPTID";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    //해당 동에 특별 Aging용 설비가 등록되어 있지 않습니다.
                    Util.MessageInfo("FM_ME_0481");
                    return;
                }
                else
                {
                    //if ((string.IsNullOrEmpty(dtResult.Rows[0]["ATTR1"].ToString())) || (string.IsNullOrEmpty(dtResult.Rows[1]["ATTR1"].ToString())))
                    if (string.IsNullOrEmpty(dtResult.Rows[0]["ATTR1"].ToString()))
                    {
                        //해당 동에 특별 Aging용 설비가 등록되어 있지 않습니다.
                        Util.MessageInfo("FM_ME_0481");
                        return;
                    }
                    _EqptID01 = dtResult.Rows[0]["COM_CODE"].ToString();
                    //_EqptID02 = dtResult.Rows[1]["COM_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}