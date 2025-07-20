/*************************************************************************************
 Created Date : 2016.08.17
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Stacking 공정진척 화면 - TEST 라벨 발행 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.17  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_TEST_PRINT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_TEST_PRINT : C1Window, IWorkArea
    {        
        #region Declaration & Constructor
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
        public ASSY003_TEST_PRINT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrint())
                return;

            // 발행..
            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            //폴딩
            dicParam.Add("reportName", "Fold");
            dicParam.Add("LOTID", "TEST");
            dicParam.Add("QTY", Convert.ToDouble(txtQty.Text).ToString());
            dicParam.Add("MAGID", txtId.Text);
            dicParam.Add("MAGIDBARCODE", txtId.Text);
            dicParam.Add("LARGELOT", "");  // 폴딩 LOT의 생성시간(공장시간기준)
            dicParam.Add("MODEL", "");
            dicParam.Add("REGDATE", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dicParam.Add("EQPTNO", "");
            dicParam.Add("TITLEX", "BASKET ID");

            dicParam.Add("PRINTQTY", "2");  // 발행 수

            //Parameters[0] = dicParam;
            //C1WindowExtension.SetParameters(print, Parameters);

            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT(dicParam);
            print.FrameOperation = FrameOperation;

            this.Dispatcher.BeginInvoke(new Action(() => print.ShowModal()));

            this.DialogResult = MessageBoxResult.OK;
        }

        private void txtQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                SetInputQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtQty_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                SetInputQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        private bool CanPrint()
        {
            bool bRet = false;

            if (txtId.Text.Trim().Equals(""))
            {
                //Util.Alert("TEST ID를 입력 하세요.");
                Util.MessageValidation("SFU1427");
                return bRet;
            }

            if (!Util.CheckDecimal(txtQty.Text, 0))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPrint);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetInputQty()
        {
            if (!Util.CheckDecimal(txtQty.Text, 0))
            {
                txtQty.Text = "";
                return;
            }
        }

        #endregion
    }
}
