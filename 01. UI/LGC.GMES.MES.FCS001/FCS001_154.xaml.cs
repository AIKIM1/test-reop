/*************************************************************************************
 Created Date : 2023.04.10 
 Creator      : 임근영
 Decription   : UNCODE 조회 메뉴
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_154 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        public FCS001_154()
        {

            InitializeComponent();

            //Control Setting
            InitControl();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion


        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();
            this.Loaded -= UserControl_Loaded; 
        }
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();
        }

        private void InitControl()
        {


            dtpFromDatevld.SelectedDateTime = DateTime.Now;
            dtpToDateVld.SelectedDateTime = DateTime.Now;

            dtpFromDateVld2.SelectedDateTime = DateTime.Now;//이력
            dtpToDateVld2.SelectedDateTime = DateTime.Now;

            cobUseYN.SetCommonCode("USE_FLAG", CommonCombo.ComboStatus.NONE, true);   ///////////////////////
            cobUseYN.SelectedValue = "Y";

        }
        #endregion

        private void btnSearchUncode_Click(object sender, RoutedEventArgs e)  //현황
        {

            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string)); ////
                dtRqst.Columns.Add("UN_CODE", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = dtpFromDatevld.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDateVld.SelectedDateTime.ToString("yyyyMMdd");
                dr["USE_FLAG"] = Util.GetCondition(cobUseYN, bAllNull: true);
                if (!string.IsNullOrEmpty(txtUnCode.Text))
                {
                    dr["UN_CODE"] = txtUnCode.Text;  //UNCODE 
                }
                  

                dtRqst.Rows.Add(dr);

                DataTable dtRsltAll = new ClientProxy().ExecuteServiceSync("DA_SEL_UNCODE_USE", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgSearch, dtRsltAll, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchUncode2_Click(object sender, RoutedEventArgs e)  //이력
        {
           // PalletID.Text = string.Empty;
            //UN_CODE2.Text = string.Empty;
            GetHistList();
        }

        private void GetHistList()
        {

            try
            {


                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PLLT_ID", typeof(string)); ////
                dtRqst.Columns.Add("UN_CODE", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = dtpFromDateVld2.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDateVld2.SelectedDateTime.ToString("yyyyMMdd");
                //dr["PLLT_ID"] = PalletID.Text;
                //dr["UN_CODE"] = UN_CODE2.Text;

                if(!string.IsNullOrEmpty(PalletID.Text))
                {
                    dr["PLLT_ID"] = PalletID.Text;
                }

                if (!string.IsNullOrEmpty(UN_CODE2.Text))
                {
                    dr["UN_CODE"] = UN_CODE2.Text;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRsltAll = new ClientProxy().ExecuteServiceSync("DA_SEL_UNCODE_PLLT_HIST", "RQSTDT", "RSLTDT", dtRqst); //이력조회
                Util.GridSetData(dgSearch2, dtRsltAll, FrameOperation, true);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /*private void txtUnCode_TextChanged(object sender, TextChangedEventArgs e) //현황 
        {

            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string)); ////
                dtRqst.Columns.Add("UN_CODE", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USE_FLAG"] = Util.GetCondition(cobUseYN, bAllNull: true);
                if (!string.IsNullOrEmpty(txtUnCode.Text))
                {
                    dr["UN_CODE"] = txtUnCode.Text;  //UNCODE 
                }


                dtRqst.Rows.Add(dr);

                DataTable dtRsltAll = new ClientProxy().ExecuteServiceSync("DA_SEL_UNCODE_USE", "RQSTDT", "RSLTDT", dtRqst);
              Util.GridSetData(dgSearch, dtRsltAll, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        */
    
    }
}

