/*************************************************************************************
 Created Date : 2020.09.21
      Creator : 김길용
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.21  담당자           Initialize
  2021.01.25  김길용A :  반송요청 시 INSUSER , UPDUSER 변경
  2021.02.04  정용석 : UI Size 변경
**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_002_RESERVATION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string strReturnID = string.Empty;
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK003_002_RESERVATION()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        //최초 Load
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    if (tmps[0] == null || tmps[1] == null)
                    {
                        Util.MessageInfo("SFU1843", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ucPersonInfo.txtUser.Focus();
                            }
                        });

                    }
                    else
                    {
                        ucPersonInfo.UserID = Util.NVC(tmps[0]);
                        ucPersonInfo.UserName = Util.NVC(tmps[1]);
                        ucPersonInfo.txtUser.Text = Util.NVC(tmps[1]);
                    }
                }

                intCombo();
                searchLogisOpt();
                // 사유 강제입력 요청
                this.txtNote.Text = "REQUEST NEXT LOGIS";

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void intCombo()
        {
            try
            {
                C1ComboBox nullCombo = new C1ComboBox();

                CommonCombo _combo = new CommonCombo();
                
                C1ComboBox[] cboEqsgChild = { cboProdId };
                string[] strCboEqsgFilter = { LoginInfo.CFG_AREA_ID, "Y","" };
                _combo.SetCombo(cboEqsgId, CommonCombo.ComboStatus.NONE, cbChild: cboEqsgChild, sFilter: strCboEqsgFilter, sCase: "LOGIS_EQSG_FOR_MEB");

                C1ComboBox[] cboProdParent = { cboEqsgId };
                string[] strComProdFilter = { "Y", null };
                _combo.SetCombo(cboProdId, CommonCombo.ComboStatus.NONE, cbParent: cboProdParent, sFilter:strComProdFilter,  sCase: "LOGIS_PROD");

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ Event ]
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void nbRequestCnt_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            // 사용안함.
            //try
            //{
            //    if (Convert.ToInt32(nbRequestCnt.Value) > 250000)
            //    {
            //        Util.MessageInfo("250000 개를 넘길수 없습니다.", (result) =>
            //        {
            //            if (result == MessageBoxResult.OK)
            //            {
            //                nbRequestCnt.Value = 250000;
            //                return;
            //            }

            //        });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        #endregion

        #region [ Method ]
        private void searchLogisOpt()
        {
            try
            {
                //라인 조건 필수가 될 경우 활성화 처리
                //if(string.IsNullOrEmpty(cboEqsgId.SelectedValue.ToString()))
                //{
                //   Util.MessageInfo("",(result) =>
                //   {
                //       if (result == MessageBoxResult.OK)
                //       {
                //           return;
                //       }
                //    });

                //}

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                //추후 MEB 구분 여부를 위해서 미리 세팅
                //dt.Columns.Add("LOGIS_YN", typeof(string));
                //dt.Columns.Add("MEB_LINE_YN", typeof(string));
                //dt.Columns.Add("TRF_TYPE", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["LOGIS_YN"] = "Y";
                //dr["MEB_LINE_YN"] = null;
                //dr["TRF_TYPE"] = "MANUAL_LOGIS_REQ";
                dr["EQSGID"] = null;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQSG_LOGIS_OPT", "RQSTDT", "RSLTDT", dt, (dtResult, ex) =>
                {
                    ShowLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        HiddenLoadingIndicator();
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgComfhist, dtResult, FrameOperation);
                    }

                    HiddenLoadingIndicator();
                });


            }
            catch(Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        



        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!chkInsertData()) return;

                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("INSUSER", typeof(string));
                dt.Columns.Add("NOTE", typeof(string));
                dt.Columns.Add("UPDUSER", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("TRF_LOT_QTY", typeof(string));

                
                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SRCTYPE"] = "UI";
                dr["INSUSER"] = ucPersonInfo.UserID;
                dr["NOTE"] = txtNote.Text.ToString();
                dr["UPDUSER"] = LoginInfo.USERID;
                dr["EQSGID"] = cboEqsgId.SelectedValue.ToString();
                dr["PRODID"] = cboProdId.SelectedValue.ToString();
                dr["TRF_LOT_QTY"] = Convert.ToString(nbRequestCnt.Value);
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_REG_LOGIS_MANUAL_REQ_NEXT", "INDATA", "", dt, (dtResult, ex) =>
                {
                    ShowLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        HiddenLoadingIndicator();
                        return;
                    }

                    searchLogisOpt();
                    HiddenLoadingIndicator();
                });
            }
            catch(Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private Boolean chkInsertData()
        {
            try
            {
                if (string.IsNullOrEmpty(cboEqsgId.SelectedValue.ToString()))
                {
                    Util.MessageInfo("SFU1223");
                    return false;
                }

                if (string.IsNullOrEmpty(cboProdId.SelectedValue.ToString()))
                {
                    Util.MessageInfo("SFU1895");
                    return false;
                }

                if (string.IsNullOrEmpty(txtNote.Text.ToString()))
                {
                    Util.MessageInfo("SFU1594");
                    return false;
                }

                if (Convert.ToInt32(nbRequestCnt.Value).Equals(0))
                {
                    Util.MessageInfo("SFU1978");
                    return false;
                }

                if (ucPersonInfo.UserID == null || string.IsNullOrEmpty(ucPersonInfo.UserID.ToString()))
                {
                    Util.MessageInfo("SFU1843");
                    return false;
                }

                return true;
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion
    }
}
