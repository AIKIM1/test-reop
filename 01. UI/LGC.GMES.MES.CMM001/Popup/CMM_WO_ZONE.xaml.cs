/*************************************************************************************
 Created Date : 2017.02.01
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - SRC 공정 WO Zone 설정 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.01  INS 정문교C : Initial Created.
  
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
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_FCUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_WO_ZONE : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _ResultWOZone = string.Empty;
        private string _SelectWOZone = string.Empty;

        public string SELWOZONE
        {
            get { return _SelectWOZone; }
        }
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

        public CMM_WO_ZONE()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _ResultWOZone = tmps[0] as string;

            SetZone();
        }

        private void cboWOZone_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ////if (cboWOZone.SelectedValue.ToString().Equals(_ResultWOZone))
            ////{
            ////    Util.Alert("이미 선택된 WO ZONE 입니다.");
            ////    cboWOZone.SelectedIndex = 0;
            ////}
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            _SelectWOZone = cboWOZone.SelectedValue.ToString();
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _SelectWOZone = "";
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void SetZone()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter1 = { "MOUNT_PSTN_GR_CODE", Process.SRC };
            _combo.SetCombo(cboWOZone, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTR");

            // 공통 콤보 생성에서 생성후 변경
            DataTable dtCombo = new DataTable();
            dtCombo = DataTableConverter.Convert(cboWOZone.ItemsSource);
            dtCombo.Rows.Add("ALL", "ALL");

            DataView dv = dtCombo.DefaultView;

            if (!string.IsNullOrEmpty(_ResultWOZone))
            {
                dv.RowFilter = "CBO_CODE <> '" + _ResultWOZone + "'";
                dv.RowFilter += " AND CBO_CODE <> 'ALL'";
            }

            cboWOZone.ItemsSource = dv.ToTable().Copy().AsDataView();

            cboWOZone.SelectedIndex = 0;
        }


        #endregion

    }
}
