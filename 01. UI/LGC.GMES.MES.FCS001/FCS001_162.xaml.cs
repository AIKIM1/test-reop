/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.09.12  양강주 : Initial Created.
  2023.09.20  양강주 : CommonCombo_Form_MB Method -> CommonCombo_Form Method로 변경
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using C1.WPF.DataGrid;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_162 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        /*컨트롤 변수 선언*/
        

        public FCS001_162()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            this.Loaded -= UserControl_Loaded;
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }


        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINEMODEL", cbParent: cboModelParent);

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }


        #endregion

        #region Events

        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetInventoryList();
        }

        #endregion

        #endregion

        #region [Method]

        #region 가용재고 조회 : GetInventoryList()
        /// <summary>
        /// 가용재고 조회
        /// </summary>
        /// <param name="idx"></param>
        private void GetInventoryList(int idx = -1)
        {
            this.ClearValidation();

            if (cboLine.GetBindValue() == null)
            {
                cboLine.SetValidation("SFU4925", tbLine.Text);
                return;
            }

            if (cboModel.GetBindValue() == null)
            {
                cboModel.SetValidation("SFU4925", tbModel.Text);
                return;
            }

            try
            {
                C1DataGrid dg = dgInventoryList;

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LINEID", typeof(string));
                RQSTDT.Columns.Add("MODELID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LINEID"] = Util.NVC(cboLine.SelectedValue);
                dr["MODELID"] = Util.NVC(cboModel.SelectedValue);

                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_DEGAS_EOL_AVAILABLE_INVENTORY_LIST", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dg, bizResult, FrameOperation, true);
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


    }
}
