/*************************************************************************************
 Created Date : 2020.09.25
      Creator : 염규범S
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
     Date         Author      CSR         Description...
  2020.09.24    염규범S     SI             Initial Created.
  2021.03.08    김길용      SI             화면 크기 및 비율 수정
**************************************************************************************/


using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;

using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_007_POPUP : C1Window, IWorkArea
    {
        #region Initialize 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK003_007_POPUP()
        {
            InitializeComponent();
            
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            string[] tmp = null;

            if (tmps != null && tmps.Length >= 1)
            {
                string Cstid = Util.NVC(tmps[0]);
                string Pltid = Util.NVC(tmps[1]);
                string Cellid = "";

                LoadSearch(Cstid, Pltid, Cellid);
            }
            else
            {
            }
        }
        #endregion
        #region [ Event ] 
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion
        #region [ Mehod ] 
        //최초 로드 하면서 PalletID로 조회 (CSTID, PALLETID, CELLID 별로 찾도록 구현)
        private void LoadSearch(string Cstid, string Pltid, string Cellid)
        {
            DataSet dsInput = new DataSet();
            try
            {

                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("PLTID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = null;
                dr["CSTID"] = Cstid.ToString() == "" ? null : Cstid.ToString();
                dr["PLTID"] = Pltid.ToString() == "" ? null : Pltid.ToString();
                dr["LOTID"] = Cellid.ToString() == "" ? null : Cellid.ToString();

                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOGIS_PLT_INPUTCELL", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                {

                    if (dtResult.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU1801");
                        txtCellId.Text = "";
                        Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, "0");
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {

                        Util.GridSetData(dgSearchCell, dtResult, FrameOperation);
                        txtCstid.Text = Util.NVC(dtResult.Rows[0]["CSTID"]);
                        txtPalletid.Text = Util.NVC(dtResult.Rows[0]["PLTID"]);
                        txtProid.Text = Util.NVC(dtResult.Rows[0]["PLT_PRODID"]);
                        txtWipstat.Text = Util.NVC(dtResult.Rows[0]["BOXTYPE"]);
                        txtCellqty.Text = Util.NVC(dtResult.Rows[0]["BOXLOTCNT"]);
                        txtInspdttm.Text = Util.NVC(dtResult.Rows[0]["PLT_UPDDTTM"]);
                        txtinboxtype.Text = Util.NVC(dtResult.Rows[0]["INBOXNAME"]);
                        //txttosloc.Text = Util.NVC(dtResult.Rows[0]["FROM_SLOC_ID"]) + " : " + Util.NVC(dtResult.Rows[0]["FROM_SLOC_NAME"]);
                    }

                    Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, Util.NVC(dtResult.Rows.Count));
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
 
        //초기화
        private void clear()
        {
            try
            {
                txtCstid.Text = "";
                txtPalletid.Text = "";
                txtProid.Text = "";
                txtWipstat.Text = "";
                txtCellqty.Text = "";
                txtInspdttm.Text = "";
                txtinboxtype.Text = "";
                //txttosloc.Text = "";

                Util.gridClear(dgSearchCell);
                Util.SetTextBlockText_DataGridRowCount(txRightRowCnt, "0");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //조회 클릭 이벤트
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (!string.IsNullOrWhiteSpace(txtCellId.Text.ToString()))
                {
                    if (rdoCst.IsChecked == true)
                    {
                        string Cstid = txtCellId.Text.ToString();
                        string Pltid = "";
                        string Cellid = "";
                        LoadSearch(Cstid, Pltid, Cellid);
                    }
                    if (rdoPlt.IsChecked == true)
                    {
                        string Cstid = "";
                        string Pltid = txtCellId.Text.ToString();
                        string Cellid = "";
                        LoadSearch(Cstid, Pltid, Cellid);
                    }
                    if (rdoCell.IsChecked == true)
                    {
                        string Cstid = "";
                        string Pltid = "";
                        string Cellid = txtCellId.Text.ToString();
                        LoadSearch(Cstid, Pltid, Cellid);
                    }

                    clear();
                }
                else
                {
                    Util.MessageInfo("SFU1801");
                    txtCellId.Text = "";
                }
               
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }




    }

    #endregion
}

