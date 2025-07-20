/*************************************************************************************
 Created Date : 2020.07.23
      Creator : 염규범
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.07.23  염규범S : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using C1ComboBox = C1.WPF.C1ComboBox;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_070 : UserControl, IWorkArea
    {
        string sSample_flag = string.Empty;

        public PACK001_070()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            setCombo();

            this.Loaded -= C1Window_Loaded;
        }

        private void setCombo()
        {
            CommonCombo _combo = new CommonCombo();

            string strAreagCase = "LABELCODE_BY_PROD";
            String[] sLabelFilter = { null, null, null, LABEL_TYPE_CODE.PACK };

            _combo.SetCombo(cboLabel, CommonCombo.ComboStatus.SELECT, sFilter: sLabelFilter, sCase: strAreagCase);

            cboLabel.SelectedIndex = 0;

            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("CBO_NAME", typeof(string));
            dtResult.Columns.Add("CBO_CODE", typeof(string));

            dtResult.Rows.Add(new object[] { "-SELECT-", "" });
            dtResult.Rows.Add(new object[] { "203 DPI", "203" });
            dtResult.Rows.Add(new object[] { "300 DPI", "300" });


            cboDpi.ItemsSource = DataTableConverter.Convert(dtResult);
            cboDpi.SelectedIndex = 0;
        }

        private void labelPrint()
        {
            try
            {
                string strLotid = string.Empty;
                string strEqsgId = string.Empty;

                if (Util.NVC(cboLabel.SelectedValue.ToString()).Equals("SELECT"))
                {
                    Util.MessageInfo("SFU3732");
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(cboDpi.SelectedValue.ToString())))
                {
                    Util.MessageInfo("SFU7334");
                    return;
                }

                strLotid = mappingLot();

                if (string.IsNullOrEmpty(strLotid))
                {
                    return;
                }

                txtLotId.Text = strLotid;

                strEqsgId = mappingLot(strLotid);

                if (!chkEqsg(strEqsgId))
                {
                    Util.MessageInfo("SFU8234", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPrintIDInput.Focus();
                            txtPrintIDInput.SelectAll();
                        }
                    });
                    return;
                }

                Util.getZPL_Pack_Temp(FrameOperation, loadingIndicator
                                       , sLOTID: strLotid
                                       , sLABEL_TYPE: LABEL_TYPE_CODE.PACK
                                       , sLABEL_CODE: Util.NVC(cboLabel.SelectedValue.ToString())
                                       , sSAMPLE_FLAG: string.IsNullOrWhiteSpace(sSample_flag) ? "N" : sSample_flag
                                       , sPRN_QTY: "1"
                                       , sPRODID: null
                                       , sDPI: Util.NVC(cboDpi.SelectedValue.ToString())
                                       , strUserId: LoginInfo.USERID
                                       );

                //txtPrintIDInput.Text = "";
                txtPrintIDInput.SelectAll();
                txtPrintIDInput.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtPrintIDInput.Focus();
                        txtPrintIDInput.SelectAll();
                    }
                });
            }

        }

        private void txtPrintIDInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                labelPrint();
            }
        }

        private void Printer_Click(object sender, RoutedEventArgs e)
        {
            labelPrint();
        }

        private string mappingLot()
        {
            try
            {
                string strLlotId = string.Empty;

                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LOTID"] = Util.NVC(txtPrintIDInput.Text.ToString().Trim());

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MAPPINGLOTID", "INDATA", "OUTDATA", INDATA);

                if (dtRslt.Rows.Count > 0)
                {
                    strLlotId = dtRslt.Rows[0]["LOTID"].ToString();

                    return strLlotId;
                }
                else
                {
                    Util.MessageInfo("SFU1192", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //txtPrintIDInput.Text = "";
                            txtPrintIDInput.Focus();
                            txtPrintIDInput.SelectAll();
                            return;
                        }
                    });

                    return strLlotId;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private string mappingLot(string strLotId)
        {
            try
            {
                string strEqsgId = string.Empty;

                DataTable INDATA = new DataTable();

                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LOTID"] = strLotId;

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPLOT", "RQSTDT", "RSLTDT", INDATA);

                if (dtRslt.Rows.Count > 0)
                {
                    strEqsgId = dtRslt.Rows[0]["EQSGID"].ToString();

                    return strEqsgId;
                }
                else
                {

                    Util.MessageInfo("SFU1192", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //txtPrintIDInput.Text = "";
                            txtPrintIDInput.Focus();
                            txtPrintIDInput.SelectAll();
                            return;
                        }
                    });
                    return strEqsgId;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }


        private Boolean chkEqsg(string strEqsgId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_UI_REPRINT_EQSGID";
                dr["CMCODE"] = strEqsgId;

                RQSTDT.Rows.Add(dr);

                DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", RQSTDT);

                if (dtAuth.Rows.Count > 0)
                {
                    sSample_flag = dtAuth.Rows[0]["ATTRIBUTE1"].ToString();
                    return true;
                }
                else
                {
                    sSample_flag = string.Empty;
                    return false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

    }
}