/*************************************************************************************
 Created Date : 2019.04.18
      Creator : 비즈테크 이동우S
   Decription : CWA 코터실적확정 시 HOLD Y/N 체크
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.18  비즈테크 이동우S : Initial Created.
  
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

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_FCUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_HOLD_YN : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _HOLDchk = string.Empty;
        private string _LOTID = string.Empty;

        public string HOLDYNCHK
        {
            get { return _HOLDchk; }
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

        public CMM_ELEC_HOLD_YN()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    _LOTID = Util.NVC(tmps[0]);
                }

                getDefectLaneLotList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoChangeCut_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void rdoDelCut_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (rdoConfirm.IsChecked.HasValue && (bool)rdoConfirm.IsChecked)
            {
                _HOLDchk = "N";
            }
            else if (rdoConfirmHold.IsChecked.HasValue && (bool)rdoConfirmHold.IsChecked)
            {
                _HOLDchk = "Y";
            }
            else
            {
                _HOLDchk = "";
            }
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _HOLDchk = "";
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void C1Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Dispatcher.BeginInvoke(new Action(() => btnSelect_Click(null, null)));
        }
        #endregion

        #region Mehod
        private void getDefectLaneLotList()
        {
            try
            {
                DataTable _DefectLane = new DataTable();

                Util.gridClear(dgDefectLane);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = _LOTID;

                IndataTable.Rows.Add(Indata);

                _DefectLane = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_DFCT_LANE_CT", "INDATA", "RSLTDT", IndataTable);

                if (_DefectLane != null && _DefectLane.Rows.Count > 0)
                {
                    Util.GridSetData(dgDefectLane, _DefectLane, FrameOperation);

                    grdDefectLane.Visibility = Visibility.Visible;
                    rdoConfirmHold.IsChecked = true;
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        #endregion

    }
}
