/*************************************************************************************
 Created Date : 2017.08.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - C 생산 관리 화면 - 입고 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.15  INS 김동일K : Initial Created.

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

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_IN_CPROD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_014_IN_CPROD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _CPROD_WRK_PSTN_ID = string.Empty;
        private string _USERID = string.Empty;
        private string _CPROD_LOTID = string.Empty;
        private int _SEND_QTY = 0;
        private string _worker = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
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
        public ASSY003_014_IN_CPROD()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                InitCombo();
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 작업장 Combo
            String[] sFilter1 = { "CPROD_WRK_PSTN_ID" };
            _combo.SetCombo(cboLocation, CommonCombo.ComboStatus.SELECT, sCase: "CPROD_LOCATION", sFilter: sFilter1);
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if ( tmps != null && tmps.Length == 4 )
                {
                    _CPROD_LOTID = Util.NVC(tmps[0]);
                    _CPROD_WRK_PSTN_ID = Util.NVC(tmps[1]);
                    _SEND_QTY = Util.NVC_Int(tmps[2]);
                    _worker = Util.NVC(tmps[3]);
                }

                else
                {
                    _CPROD_LOTID = "";
                    _CPROD_WRK_PSTN_ID = "";
                    _SEND_QTY = 0;
                    _worker = "";
                }

                SetControls();

                ApplyPermissions();
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void SetControls()
        {
            nbxInQty.Value = _SEND_QTY;
            cboLocation.SelectedValue = _CPROD_WRK_PSTN_ID;

            nbxInQty.Focus();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ( !CanSave() )
                    return;

                Save();
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod
      
        #region [BizCall]
        private void Save()
        {
            try
            {
                ShowLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_IN_CPROD_LOT();

                DataTable INDATA_Table = indataSet.Tables["INDATA"];

                DataRow newRow = INDATA_Table.NewRow();
                newRow["CPROD_WRK_PSTN_ID"] = cboLocation.SelectedValue;
                newRow["USERID"] = _worker;

                INDATA_Table.Rows.Add(newRow);

                newRow = null;

                DataTable IN_LOT_Table = indataSet.Tables["IN_LOT"];

                newRow = IN_LOT_Table.NewRow();
                newRow["CPROD_LOTID"] = _CPROD_LOTID;
                newRow["DFCT_QTY"] = Util.NVC_Int(nbxInQty.Value);
                IN_LOT_Table.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_IN_CPROD_LOT", "INDATA,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if ( searchException != null )
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch ( Exception ex )
                    {
                        Util.MessageException(ex);

                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch ( Exception ex )
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]

        private bool CanSave()
        {
            bool bRet = false;
            if (cboLocation.SelectedIndex <= 0)
            {
                //Util.Alert("작업장을 선택 하세요.");
                Util.MessageValidation("SFU4206");
            }

            if (Util.NVC_Int(nbxInQty.Value) < 1)
            {
                //Util.Alert("수량은 0 보다 커야 합니다.");
                Util.MessageValidation("100057");
            }
            else bRet = true;

            return bRet;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion
    }
}
