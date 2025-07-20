/*************************************************************************************
 Created Date : 2016.09.21
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - Tray 이동 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.21  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_007_TRAY_MOVE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_007_TRAY_MOVE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _TrayID = string.Empty;

        BizDataSet _Biz = new BizDataSet();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY001_007_TRAY_MOVE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 5)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _TrayID = Util.NVC(tmps[4]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _TrayID = "";
            }

            ApplyPermissions();

            txtSelectId.Text = _TrayID;
            txtTrayId.SelectAll();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            txtMoveId.Text = txtTrayId.Text;
            txtTrayId.Text = "";
        }

        private void txtTrayId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                btnSearch.RaiseEvent(newEventArgs);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayMove())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("이동 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1763",result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [Validation]
        private bool CanTrayMove()
        {
            bool bRet = false;

            if (txtMoveId.Text.Trim().Equals(""))
            {
                //Util.Alert("이동할 Tray ID 가 없습니다.");
                Util.MessageValidation("SFU1768");
                txtMoveId.SelectAll();
                return bRet;
            }
            bRet = true;
            return bRet;
        }

        private bool CanSearch()
        {
            bool bRet = false;

            if (txtTrayId.Text.Trim().Equals(""))
            {
                //Util.Alert("Tray ID 가 없습니다.");
                Util.MessageValidation("SFU1430");
                txtTrayId.SelectAll();
                return bRet;
            }

            //정규식표현 false=>영문과숫자이외문자열비허용
            bool chk = System.Text.RegularExpressions.Regex.IsMatch(txtTrayId.Text.ToUpper(), @"^[a-zA-Z0-9]+$");
            if (!chk)
            {
                //Util.Alert("{0}의 TRAY_ID 특수문자가 있습니다. 생성할 수 없습니다", txtTrayId.Text.ToUpper());
                Util.MessageValidation("SFU1298", txtTrayId.Text.ToUpper());

                txtTrayId.Text = "";
                txtTrayId.SelectAll();
                return bRet;
            }


            //string strTrayPrefix = string.Empty;

            //if (this.m_EqptName.IndexOf("17-1") == 0)
            //{
            //    strTrayPrefix = "LBCD";
            //}
            //else if (this.m_EqptName.IndexOf("17-2") == 0)
            //{
            //    strTrayPrefix = "LBDD";
            //}
            //else if (this.m_EqptName.IndexOf("17-3") == 0)
            //{
            //    strTrayPrefix = "LBED";
            //}
            //else if (this.m_EqptName.IndexOf("17-4") == 0)
            //{
            //    strTrayPrefix = "LBFD";
            //}

            //if (strTrayPrefix == "" || txtTrayId.Text.ToUpper().IndexOf(strTrayPrefix) != 0)
            //{
            //    Util.Alert("81채널 TRAY ID룰에 맞지 않습니다.");
            //    txtTrayId.Text = string.Empty;
            //    txtTrayId.SelectAll();
            //    return bRet;
            //}

            string sRet = string.Empty;
            string sMsg = string.Empty;
            GetTrayInfo(out sRet, out sMsg);
            if (sRet.Equals("NG"))
            {
                Util.Alert(sMsg);
                return bRet;
            }

            if (txtSelectId.Text.ToUpper() == txtTrayId.Text.ToUpper())
            {
                //Util.Alert("변경내용이 없습니다.");
                Util.MessageValidation("SFU1226");
                txtTrayId.Text = string.Empty;
                txtTrayId.SelectAll();
                return bRet;
            }

            // FCS Check...            
            if (!GetFCSTrayCheck(txtTrayId.Text.ToUpper()))
            {
                //Util.Alert("FORMATION에 TRAY ID가 작업중입니다. 활성화에 문의하세요.");
                Util.MessageValidation("SFU1336");
                txtTrayId.Text = string.Empty;
                txtTrayId.SelectAll();
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [BizRule]
        private bool GetFCSTrayCheck(string sTrayID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_FCS_TRAY_CHK_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sTrayID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                if (dtResult == null || dtResult.Rows.Count <= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetTrayInfo(out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_TRAY_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = _LotID;
                newRow["CSTID"] = txtTrayId.Text;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0 && !Util.NVC(dtResult.Rows[0]["CNT"]).Equals("0"))
                {
                    sRet = "NG";
                    sMsg = "SFU3056";   // 이미 입력한 TRAY ID 입니다.
                }
                else
                {
                    sRet = "OK";
                    sMsg = "";
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                sRet = "NG";
                sMsg = ex.Message;
            }
        }

        private void Save()
        {
            try
            {
                //ShowLoadingIndicator();

                //DataTable inTable = _Biz.GetBR_PRD_REG_MOVETRAY_CL();

                //DataRow newRow = inTable.NewRow();
                //newRow["LOTID"] = _LotID;

                //inTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService("", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                //{
                //    try
                //    {
                //        if (searchException != null)
                //        {
                //            HiddenLoadingIndicator();
                //            Util.MessageException(searchException);
                //            return;
                //        }

                //        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                //        this.DialogResult = MessageBoxResult.OK;
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {
                //        HiddenLoadingIndicator();
                //    }
                //});
            }
            catch(Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #endregion


    }
}
