/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack ID발행 화면 - 발행된 라벨ID 표시 팝업
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
    public partial class PACK001_001_KEYPART_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        

        private string sLotid = string.Empty;
        public string Lotid
        {
            get
            {
                return sLotid;
            }

            set
            {
                sLotid = value;
            }
        }

        private string sProcid = string.Empty;

        public string Procid
        {
            get
            {
                return sProcid;
            }

            set
            {
                sProcid = value;
            }
        }

        public PACK001_001_KEYPART_CHANGE()
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

                        txtLotID.Text = Util.NVC(dtText.Rows[0]["LOTID"]);
                        txtSEQ.Text = Util.NVC(dtText.Rows[0]["SEQ"]);
                        txtKeypart_Before.Text = Util.NVC(dtText.Rows[0]["KEYPARTLOT_BEFORE"]);
                        txtKeypart_After.Focus();
                        Lotid = Util.NVC(dtText.Rows[0]["LOTID"]);
                        Procid = Util.NVC(dtText.Rows[0]["PROCID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void txtKeypart_After_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key != Key.Enter)
                {
                    return;
                }

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PRDT_ATTCH_PSTN_NO", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("LOTMID_BEFORE", typeof(string));
                INDATA.Columns.Add("LOTMID_AFTER", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));


                DataRow dr = INDATA.NewRow();

                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotID.Text;
                dr["PRDT_ATTCH_PSTN_NO"] = txtSEQ.Text;
                dr["PROCID"] = Procid;
                dr["LOTMID_BEFORE"] = txtKeypart_Before.Text;
                dr["LOTMID_AFTER"] = txtKeypart_After.Text;
                dr["USERID"] = LoginInfo.USERID;                
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_KEYPART_CHANGE", "INDATA", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        if (dataException != null)
                        {
                            //Util.AlertByBiz("BR_PRD_REG_KEYPART_CHANGE", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion

        #region Mehod

        #endregion

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PRDT_ATTCH_PSTN_NO", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("LOTMID_BEFORE", typeof(string));
                INDATA.Columns.Add("LOTMID_AFTER", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));


                DataRow dr = INDATA.NewRow();

                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotID.Text;
                dr["PRDT_ATTCH_PSTN_NO"] = txtSEQ.Text;
                dr["PROCID"] = Procid;
                dr["LOTMID_BEFORE"] = txtKeypart_Before.Text;
                dr["LOTMID_AFTER"] = txtKeypart_After.Text;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_KEYPART_CHANGE", "INDATA", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        if (dataException != null)
                        {
                            //Util.AlertByBiz("BR_PRD_REG_KEYPART_CHANGE", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
    }
}
