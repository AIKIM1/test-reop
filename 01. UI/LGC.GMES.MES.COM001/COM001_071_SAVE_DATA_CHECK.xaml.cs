/*************************************************************************************
 Created Date : 2024.03.11
      Creator : 안유수
   Decription : LOT 정보 변경 데이터 확인 팝업 창
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.11  안유수 : Initial Created.


**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_071_SAVE_DATA_CHECK : C1Window, IWorkArea
    {
        #region Initialize
        private DataTable dtbefore = new DataTable();
        private string _lotid = string.Empty;
        private string _lottype = string.Empty;
        private string _woid = string.Empty;
        private string _wodetail = string.Empty;
        private string _prodid = string.Empty;
        private string _modelid = string.Empty;
        private string _prodver = string.Empty;
        private string _laneqty = string.Empty;
        private string _markettype = string.Empty;
        private string _username = string.Empty;
        private string _wipnote = string.Empty;

        public IFrameOperation FrameOperation { get; set; }

        public COM001_071_SAVE_DATA_CHECK()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                dtbefore = tmps[0] as DataTable; // 수정전 데이터
                _lotid = Util.NVC(tmps[1]);
                _lottype = Util.NVC(tmps[2]);
                _woid = Util.NVC(tmps[3]);
                _wodetail = Util.NVC(tmps[4]);
                _prodid = Util.NVC(tmps[5]);
                _modelid = Util.NVC(tmps[6]);
                _prodver = Util.NVC(tmps[7]);
                _laneqty = Util.NVC(tmps[8]);
                _markettype = Util.NVC(tmps[9]);
                _username = Util.NVC(tmps[10]);
                _wipnote = Util.NVC(tmps[11]);// 수정할 데이터
            }

            if (dtbefore.Rows.Count != 0)
            {
                txtSelectLotBF.Text = Util.NVC(dtbefore.Rows[0]["LOTID"]);
                txtLotTypeBF.Text = Util.NVC(dtbefore.Rows[0]["LOTTYPE_NAME"]);
                txtSelectWOBF.Text = Util.NVC(dtbefore.Rows[0]["WOID"]);
                txtSelectWODetailBF.Text = Util.NVC(dtbefore.Rows[0]["WO_DETL_ID"]);
                txtSelectProdidBF.Text = Util.NVC(dtbefore.Rows[0]["PRODID"]);
                txtSelectModelidBF.Text = Util.NVC(dtbefore.Rows[0]["MODLID"]);
                txtSelectProdVerBF.Text = Util.NVC(dtbefore.Rows[0]["PROD_VER_CODE"]);
                txtSelectLaneQtyBF.Text = Util.NVC(dtbefore.Rows[0]["LANE_QTY"]);
                txtMarketTypeBF.Text = Util.NVC(dtbefore.Rows[0]["MKT_TYPE_CD"]);
                txtUserName.Text = _username;
                txtWipNote.Text = _wipnote;
            }

            txtSelectLotAF.Text = _lotid;
            txtLotTypeAF.Text = _lottype;
            txtSelectWOAF.Text = _woid;
            txtSelectWODetailAF.Text = _wodetail;
            txtSelectProdidAF.Text = _prodid;
            txtSelectModelidAF.Text = _modelid;
            txtSelectProdVerAF.Text = _prodver;
            txtSelectLaneQtyAF.Text = _laneqty;
            txtMarketTypeAF.Text = _markettype;

            ChangeDataComPare();
        }
        #endregion

        #region Event

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void ChangeDataComPare()
        {
            if (dtbefore.Rows.Count != 0)
            {
                if (txtLotTypeAF.Text != txtLotTypeBF.Text)
                {
                    txtLotTypeAF.Background = new SolidColorBrush(Colors.Orange);
                }
                if (txtSelectWOAF.Text != txtSelectWOBF.Text)
                {
                    txtSelectWOAF.Background = new SolidColorBrush(Colors.Orange);
                }
                if (txtSelectWODetailAF.Text != txtSelectWODetailBF.Text)
                {
                    txtSelectWODetailAF.Background = new SolidColorBrush(Colors.Orange);
                }
                if (txtSelectProdidAF.Text != txtSelectProdidBF.Text)
                {
                    txtSelectProdidAF.Background = new SolidColorBrush(Colors.Orange);
                }
                if (txtSelectModelidAF.Text != txtSelectModelidBF.Text)
                {
                    txtSelectModelidAF.Background = new SolidColorBrush(Colors.Orange);
                }
                if (txtSelectProdVerAF.Text != txtSelectProdVerBF.Text)
                {
                    txtSelectProdVerAF.Background = new SolidColorBrush(Colors.Orange);
                }
                if (txtSelectLaneQtyAF.Text != txtSelectLaneQtyBF.Text)
                {
                    txtSelectLaneQtyAF.Background = new SolidColorBrush(Colors.Orange);
                }
                if (txtMarketTypeAF.Text != txtMarketTypeBF.Text)
                {
                    txtMarketTypeAF.Background = new SolidColorBrush(Colors.Orange);
                }
            }
        }
        #endregion
    }
}
