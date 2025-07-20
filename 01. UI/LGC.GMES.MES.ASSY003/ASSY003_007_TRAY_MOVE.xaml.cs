/*************************************************************************************
 Created Date : 2017.08.10
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - Tray 이동 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.10  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_007_TRAY_MOVE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_007_TRAY_MOVE : C1Window, IWorkArea
    {   
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _TrayID = string.Empty;
        private string _ProdLotID = string.Empty;
        private string _OutLotid = string.Empty;

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
        public ASSY003_007_TRAY_MOVE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 6)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _TrayID = Util.NVC(tmps[4]);
                _OutLotid = Util.NVC(tmps[5]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _TrayID = "";
                _OutLotid = "";
            }

            ApplyPermissions();

            txtSelectId.Text = _TrayID;
            txtTrayId.SelectAll();
        }
        
        private void txtTrayId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //RoutedEventArgs newEventArgs = new RoutedEventArgs(Button.ClickEvent);
                //btnSearch.RaiseEvent(newEventArgs);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayMove())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("이동 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1763", result =>
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

            if (txtTrayId.Text.Trim().Equals(""))
            {
                //Util.Alert("이동할 Tray ID 가 없습니다.");
                Util.MessageValidation("SFU1768");
                txtTrayId.SelectAll();
                return bRet;
            }
            bRet = true;
            return bRet;
        }
        #endregion

        #region [BizRule]
        
        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_MOVETRAY_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotid;
                newRow["CSTID"] = txtSelectId.Text;
                newRow["CSTID_NEW"] = txtTrayId.Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                
                newRow = null;

                //DataTable inSpcl = indataSet.Tables["IN_SPCL"];
                //newRow = inSpcl.NewRow();
                //newRow["SPCL_CST_GNRT_FLAG"] = "N";
                //newRow["SPCL_CST_NOTE"] = "";
                //newRow["SPCL_CST_RSNCODE"] = "";

                //inSpcl.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_TRAY_CHANGE_CL_S", "IN_EQP", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
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
