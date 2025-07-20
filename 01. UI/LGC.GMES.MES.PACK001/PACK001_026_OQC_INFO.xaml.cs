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
using LGC.GMES.MES.PACK001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_026_OQC_INFO : C1Window, IWorkArea
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

        public PACK001_026_OQC_INFO()
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
                        txtBoxID.Text = Util.NVC(dtText.Rows[0]["BOXID"]);
                        setBoxInfo(txtBoxID.Text);
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
                Util.Alert(ex.ToString());
            }
        }

        private void btnBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBoxID.Text.Length > 0)
                {
                    setBoxInfo(txtBoxID.Text);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxID.Text.Length > 0)
                {
                    setBoxInfo(txtBoxID.Text);
                }
            }
        }


        private void btnBoxTagPrint_Click(object sender, RoutedEventArgs e)
        {
            setTagReport(txtBoxID.Text, Util.NVC(txtInfoBoxID.Tag));
        }


        #endregion

        #region Mehod

        private void setBoxInfo(string sBoxid)
        {
            try
            {
                //DA_PRD_SEL_BOX
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBoxid;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OQC_INFOMATION_PALLET", "INDATA", "PALLET_INFO,PALLET_OQC_LIST", dsInput);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    string sStartedLotid = string.Empty;
                    string sStartedProductid = string.Empty;

                    if ((dsResult.Tables.IndexOf("PALLET_INFO") > -1))
                    {
                        if (dsResult.Tables["PALLET_INFO"].Rows.Count > 0)
                        {
                            //20170324 ==============================================
                            //txtInfoBoxID.Text = Util.NVC(dsResult.Tables["PALLET_INFO"].Rows[0]["BOXID"]);
                            txtInfoBoxID.Text = Util.NVC(dsResult.Tables["PALLET_INFO"].Rows[0]["OQC_INSP_REQ_ID"]);
                            txtInfoBoxID.Tag = Util.NVC(dsResult.Tables["PALLET_INFO"].Rows[0]["EQSGID"]);

                            //20170324 ==============================================
                            txtInfo_ISS_SCHD_DATE.Text = Util.NVC(dsResult.Tables["PALLET_INFO"].Rows[0]["ISS_SCHD_DATE"]);
                            txtInfo_OqcReqDttm.Text = Util.NVC(dsResult.Tables["PALLET_INFO"].Rows[0]["REQ_DTTM"]);  
                            txtInfo_PrjModel.Text = Util.NVC(dsResult.Tables["PALLET_INFO"].Rows[0]["PRJMODEL"]); 
                            txtInfo_PROG_NAME.Text = Util.NVC(dsResult.Tables["PALLET_INFO"].Rows[0]["INSP_PROG_NAME"]);  
                            txtInfo_ShipToName.Text = Util.NVC(dsResult.Tables["PALLET_INFO"].Rows[0]["SHIPTO_NAME"]);
                            
                        }
                        else
                        {
                            txtBoxID.Focus();
                            txtBoxID.SelectAll();

                            txtInfoBoxID.Text = "";
                            txtInfoBoxID.Tag = "";
                            txtInfo_ISS_SCHD_DATE.Text = "";
                            txtInfo_OqcReqDttm.Text = "";
                            txtInfo_PrjModel.Text = "";
                            txtInfo_PROG_NAME.Text = "";
                            txtInfo_ShipToName.Text = "";
                        }
                    }
                    if ((dsResult.Tables.IndexOf("PALLET_OQC_LIST") > -1))
                    {
                        if (dsResult.Tables["PALLET_OQC_LIST"].Rows.Count > 0)
                        {
                            dgOqcLotList.ItemsSource = DataTableConverter.Convert(dsResult.Tables["PALLET_OQC_LIST"]);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // PackCommon에 공통으로 사용하며Show이다. 그러나 해당 팝업창이 ShowModal이라 따로 빼서 ShowModal로 사용함
        private void setTagReport(string sPalletID,string sEQSGID)
        {
            try
            {
                if (!GetPackApplyLIneByUI(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID))
                {
                    // 기존 Pallet Tag
                    Pallet_Tag popupPalletTag = new Pallet_Tag();

                    if (popupPalletTag != null)
                    {
                        // 태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[3];
                        Parameters[0] = "Pallet_Tag";
                        Parameters[1] = sPalletID;
                        Parameters[2] = sEQSGID;

                        C1WindowExtension.SetParameters(popupPalletTag, Parameters);
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => popupPalletTag.Show()));
                    }
                }
                else
                {
                    // 신규 Pallet Tag
                    Pallet_Tag_V2 popupPalletTag = new Pallet_Tag_V2();

                    if (popupPalletTag == null)
                    {
                        return;
                    }

                    object[] Parameters = new object[3];
                    Parameters[0] = string.Empty;
                    Parameters[1] = sPalletID;
                    Parameters[2] = sEQSGID;

                    C1WindowExtension.SetParameters(popupPalletTag, Parameters);
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => popupPalletTag.ShowModal()));
                    popupPalletTag.Focus();

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static bool GetPackApplyLIneByUI(string shopID, string areaID)
        {
            bool returnValue = false;
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = "DIFFUSION_SITE";
                drRQSTDT["CBO_CODE"] = "PLT_AREA_TAG";
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    returnValue = false;
                }
                else
                {
                    foreach (DataRow drRSLTDT in dtRSLTDT.Select())
                    {
                        string settingShopID = drRSLTDT["ATTRIBUTE1"].ToString();
                        string settingAreaID = drRSLTDT["ATTRIBUTE2"].ToString();

                        if ((!string.IsNullOrEmpty(settingShopID) && (!string.IsNullOrEmpty(settingShopID) && settingShopID.Contains(shopID))) &&
                            (!string.IsNullOrEmpty(settingAreaID) && (!string.IsNullOrEmpty(settingAreaID) && settingAreaID.Contains(areaID)))
                           )
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return returnValue;
        }

        private void printPopUp_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.PACK001.Pallet_Tag printPopUp = sender as LGC.GMES.MES.PACK001.Pallet_Tag;
                if (Convert.ToBoolean(printPopUp.DialogResult))
                {

                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }


        #endregion

    }
}
