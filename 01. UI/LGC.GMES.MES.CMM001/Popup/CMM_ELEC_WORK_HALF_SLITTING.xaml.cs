/*************************************************************************************
 Created Date : 2019.04.02
      Creator : 신광희 차장
   Decription : 전지 5MEGA-GMES 구축 - 무지부 방향 설정
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.02  신광희 차장 : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_WORK_HALF_SLITTING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_WORK_HALF_SLITTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _equipmentCode = string.Empty;
        private string _workHalfSlitting = string.Empty;

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

        public CMM_ELEC_WORK_HALF_SLITTING()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                _workHalfSlitting = Util.NVC(parameters[1]);

                if (!string.IsNullOrEmpty(_workHalfSlitting))
                {
                    switch (_workHalfSlitting)
                    {
                        case "L":
                            rdoHalfSlitSideLeft.IsChecked = true;
                            break;
                        case "R":
                            rdoHalfSlitSideRight.IsChecked = true;
                            break;
                        default:
                            rdoAll.IsChecked = true;
                            break;
                    }
                }
            }
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWorkHalfSlittingSide()) return;

            try
            {
                const string bizRuleName = "DA_PRD_UPD_WRK_HALF_SLIT_SIDE";

                string workhalfSlittingSideCode = string.Empty;

                if (rdoHalfSlitSideLeft.IsChecked == true)
                    workhalfSlittingSideCode = "L";
                else if(rdoHalfSlitSideRight.IsChecked == true)
                    workhalfSlittingSideCode = "R";
                else if(rdoAll.IsChecked == true)
                    workhalfSlittingSideCode = "A";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WRK_HALF_SLIT_SIDE", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _equipmentCode;
                dr["WRK_HALF_SLIT_SIDE"] = workhalfSlittingSideCode;
                
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1270");      //저장되었습니다.
                    DialogResult = MessageBoxResult.OK;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region Mehod

        private bool ValidationWorkHalfSlittingSide()
        {
            if(rdoHalfSlitSideLeft.IsChecked == false && rdoHalfSlitSideRight.IsChecked == false && rdoAll.IsChecked == false)
            {
                Util.MessageValidation("SFU6030");  // 무지부 방향을 선택하세요.
                return false;
            }
            return true;
        }
        #endregion

    }
}
