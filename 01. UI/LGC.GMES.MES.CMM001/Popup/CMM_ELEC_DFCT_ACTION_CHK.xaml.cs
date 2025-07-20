/*************************************************************************************
 Created Date : 2019.07.04
      Creator : 
   Decription : CWA 슬리터실적확정 시 Defect Lane 체크
--------------------------------------------------------------------------------------
 [Change History]
  2019.07.04  : Initial Created.
  
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
    /// CMM_ELEC_DFCT_ACTION_CHK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_DFCT_ACTION_CHK : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _Lanechk = string.Empty;
        private string _CUTID = string.Empty;

        public string DEFECTLANECHK
        {
            get { return _Lanechk; }
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

        public CMM_ELEC_DFCT_ACTION_CHK()
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
                    _CUTID = Util.NVC(tmps[0]);
                }

                getDefectLaneLotList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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
                IndataTable.Columns.Add("CUT_ID", typeof(string));
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CUT_ID"] = _CUTID;

                IndataTable.Rows.Add(Indata);

                _DefectLane = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_DFCT_LANE_SL", "INDATA", "RSLTDT", IndataTable);

                DataView dv = _DefectLane.DefaultView;
                dv.RowFilter = "ISNULL(DFCT_LANE_FLAG,'N') = 'N'";
                DataTable dt2 = dv.ToTable();

                if (dt2.Rows.Count == _DefectLane.Rows.Count)
                {
                    if (_DefectLane != null && _DefectLane.Rows.Count > 0)
                    {
                        DataView view = _DefectLane.DefaultView;
                        view.RowFilter = "ABNORMAL_FLAG = 'ABNORMAL'";
                        DataTable dt = view.ToTable();

                        Util.GridSetData(dgDefectLane, dt, FrameOperation);

                        foreach (DataRow row in _DefectLane.Rows)
                        {
                            if (string.Equals(Util.NVC(row["ABNORMAL_FLAG"]), "ABNORMAL"))
                            {
                                btnSelect.IsEnabled = false;
                                grdMessage.Visibility = Visibility.Visible;
                                lblMessage.Text = MessageDic.Instance.GetMessage("SFU7025");    //전수불량 미등록 Lane이 있습니다.
                            }
                        }
                    }
                }
                else
                {
                    if (_DefectLane != null && _DefectLane.Rows.Count > 0)
                    {
                        DataView view = _DefectLane.DefaultView;
                        view.RowFilter = "DFCT_LANE_FLAG = 'Y'";
                        DataTable dt = view.ToTable();

                        Util.GridSetData(dgDefectLane, dt, FrameOperation);

                        btnSelect.IsEnabled = true;
                        grdMessage.Visibility = Visibility.Visible;
                        lblMessage.Text = MessageDic.Instance.GetMessage("SFU7028");    //위 팬케잌은 종료처리 됩니다. 확정하시겠습니까?
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
        #endregion


    }
}
