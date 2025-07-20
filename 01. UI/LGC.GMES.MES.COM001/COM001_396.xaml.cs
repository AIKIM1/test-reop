/*************************************************************************************
 Created Date : 2023.12.15
      Creator : 백광영
   Decription : 공팔레트 폐기/수리
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_396 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        public COM001_396()
        {
            InitializeComponent();
            Loaded += COM001_396_Loaded;
        }

        private void COM001_396_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= COM001_396_Loaded;
            
            InitCombo();
            InitControls();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            // Scrap
            string[] sFilter1 = { "CST_DFCT_RESNCODE" };
            _combo.SetCombo(cboReason, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter1);

            // Pallet Type
            string[] sFilter2 = { "CSTPROD", "PT" };
            _combo.SetCombo(cboPalletType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODEATTR", sFilter: sFilter2);
        }
        private void InitControls()
        {
            txtPalletBcd.Clear();
            txtPalletBcd.Focus();
            txtPalletStatus.Clear();
            txtLocation.Clear();
            txtRestoreNote.Clear();
            txtRestoreNote.Clear();

            grdScrap.Visibility = Visibility.Collapsed;
            grdRestore.Visibility = Visibility.Collapsed;
        }
        #endregion


        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetScrapList();
        }
        #endregion


        #region [Func]
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

        #region 
        private void GetPalletInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = Util.NVC(txtPalletBcd.Text);
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER", "RQSTDT", "RSLTDT", INDATA);
                if (dtResult?.Rows?.Count > 0)
                {
                    if (string.Equals(Util.NVC(dtResult.Rows[0]["CSTSTAT"]), "U"))
                    {
                        // %1은 Empty 상태인 캐리어가 아닙니다.
                        Util.MessageValidation("SFU4928", result =>
                        {
                            txtPalletBcd.Clear();
                            txtPalletBcd.Focus();
                        }, Util.NVC(dtResult.Rows[0]["CSTID"]));
                        return;
                    }
                    string _dfctFlag = Util.NVC(dtResult.Rows[0]["CST_DFCT_FLAG"]);
                    if (string.Equals(_dfctFlag, "N") || string.IsNullOrEmpty(_dfctFlag))
                    {
                        txtPalletStatus.Text = ObjectDic.Instance.GetObjectName("NORMAL");
                        grdScrap.Visibility = Visibility.Visible;
                        grdRestore.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtPalletStatus.Text = ObjectDic.Instance.GetObjectName("SCRAP");
                        grdScrap.Visibility = Visibility.Collapsed;
                        grdRestore.Visibility = Visibility.Visible;
                    }
                    txtLocation.Text = Util.NVC(dtResult.Rows[0]["CURR_RACK_ID"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, result =>
                {
                    txtPalletBcd.Focus();
                });
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetScrapList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = Util.NVC(txtPalletBcdId.Text);
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_EMPTY_PALLET_SCRAP_LIST", "RQSTDT", "RSLTDT", INDATA);
                if (dtResult?.Rows?.Count > 0)
                {
                    Util.GridSetData(dgPalletHistory, dtResult, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, result =>
                {
                    
                });
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void _Save()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLET_BCD", typeof(string));
                RQSTDT.Columns.Add("CST_DFCT_FLAG", typeof(string));
                RQSTDT.Columns.Add("CST_DFCT_RESNCODE", typeof(string));
                RQSTDT.Columns.Add("CSTPROD", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLET_BCD"] = Util.NVC(txtPalletBcd.Text);
                if (grdScrap.Visibility == Visibility.Visible)
                {
                    dr["CST_DFCT_FLAG"] = "Y";
                    dr["CST_DFCT_RESNCODE"] = Util.GetCondition(cboReason);
                    dr["NOTE"] = Util.NVC(txtScrapNote.Text);
                    dr["ACTID"] = "CARRIER_EMPTY_PALLET_SCRAP";
                }
                else
                {
                    dr["CST_DFCT_FLAG"] = "N";
                    dr["CST_DFCT_RESNCODE"] = "";
                    dr["CSTPROD"] = Util.GetCondition(cboPalletType);
                    dr["NOTE"] = Util.NVC(txtRestoreNote.Text);
                    dr["ACTID"] = "CARRIER_EMPTY_PALLET_RESTORE";
                }
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_REG_EMPTY_PALLET_SCRAP_RESTORE", "INDATA", null, RQSTDT);
                Util.MessageInfo("SFU1275");
                InitControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]
        private void txtPalletBcd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) GetPalletInfo();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.Equals(txtPalletBcd.Text, ""))
            {
                // Pallet BCD를 입력하세요
                Util.MessageValidation("SFU8923", result =>
                {
                    txtPalletBcd.Focus();
                });
                return;                
            }
            if (grdScrap.Visibility == Visibility.Visible)
            {
                string _reason = Util.GetCondition(cboReason, "SFU1593"); //사유를 선택하세요.
                if (_reason.Equals(""))
                {
                    return;
                }
            }
            if (grdRestore.Visibility == Visibility.Visible)
            {
                string _plttype = Util.GetCondition(cboPalletType, "SFU9102"); //Pallet Type을 선택하세요
                if (_plttype.Equals(""))
                {
                    return;
                }
            }
            if (grdScrap.Visibility != Visibility.Visible && grdRestore.Visibility != Visibility.Visible)
            {
                // 조회를 먼저 해주세요.
                Util.MessageValidation("SFU8485");
                return;
            }

            try
            {
                // 저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        _Save();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}