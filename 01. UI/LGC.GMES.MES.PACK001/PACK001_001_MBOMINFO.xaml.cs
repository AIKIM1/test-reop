/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
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
    public partial class PACK001_001_MBOMINFO : C1Window, IWorkArea
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

        private DataView dvRootNodes;

        public PACK001_001_MBOMINFO()
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
                        
                        txtEqsgName.Tag = Util.NVC(dtText.Rows[0]["EQSGID"]);
                        txtEqsgName.Text = Util.NVC(dtText.Rows[0]["EQSGNAME"]);
                        txtProcName.Tag = Util.NVC(dtText.Rows[0]["PROCID"]);
                        txtProcName.Text = Util.NVC(dtText.Rows[0]["PROCNAME"]);
                        txtProdID.Text = Util.NVC(dtText.Rows[0]["PRODID"]);

                        setMbomInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
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
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void txtProdID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtProdID.Text.Length > 0)
                    {
                        setMbomInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        private void setMbomInfo()
        {
            try
            {
                //DA_PRD_SEL_BOX
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProdID.Text;
                dr["PROCID"] = Util.NVC(txtProcName.Tag);
                dr["MTRLID"] = null;
                dr["EQPTID"] = null;
                dr["EQSGID"] = Util.NVC(txtEqsgName.Tag);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MBOM_BY_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                dgBomList.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_MBOM_BY_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        


        #endregion

        
    }
}
