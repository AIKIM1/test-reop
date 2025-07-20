/*************************************************************************************
 Created Date : 2023.05.09
      Creator : 정재홍 
   Decription : [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건 [CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN Copy]
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.09  정재홍 : Initial Created. [ CSR : E20230210-000354 ]
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_WORK_HALF_SLITTING_ESWA.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_WORK_HALF_SLITTING_ESWA : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _equipmentCode = string.Empty;
        private string _workHalfSlitting = string.Empty;
        #endregion

        #region [Initialize]
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_WORK_HALF_SLITTING_ESWA()
        {
            InitializeComponent();
            SetComponent();
        }

        #endregion

        #region [Event]

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                _workHalfSlitting = Util.NVC(parameters[1]);

                if (!string.IsNullOrEmpty(_workHalfSlitting))
                {
                    foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                    {
                        if (rdo.Tag.ToString().Equals(_workHalfSlitting))
                            rdo.IsChecked = true;
                    }
                }
            }
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWorkHalfSlittingSide()) return;

            try
            {
                string sHSSCode = string.Empty;  //무지부방향
                string sWDCode = string.Empty;  //권취방향

                foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                {
                    if (rdo.IsChecked == true)
                    {
                        sHSSCode = rdo.Tag.ToString().Substring(0, 1);
                        sWDCode = rdo.Tag.ToString().Substring(2, 1);
                    }
                }

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WRK_HALF_SLIT_SIDE", typeof(string));
                inDataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _equipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dr["WRK_HALF_SLIT_SIDE"] = sHSSCode;
                dr["EM_SECTION_ROLL_DIRCTN"] = sWDCode;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_REG_EIOATTR_SLIT_SIDE_ROLL_DIR", "IN_EQP", null, inDataTable, (bizResult, bizException) =>
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

        #region [Method]
        /// <summary>
        /// 무지부 방향 선택 버튼 생성
        /// </summary>
        private void SetComponent()
        {
            DataTable dtRdo = GetCommonCodeAttr();

            foreach (DataRow dr in dtRdo.Rows)
            {
                RadioButton rb = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision" };
                rb.Content = dr["CBO_NAME"];
                rb.Tag = dr["CBO_CODE"];
                wpMain.Children.Add(rb);
            }
        }

        /// <summary>
        /// 공통코드에 등록된 무지부방향 항목 불러오기
        /// </summary>
        /// <returns></returns>
        private DataTable GetCommonCodeAttr()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "WRK_HALF_SLIT_SIDE";
                dr["ATTRIBUTE1"] = "3";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private bool ValidationWorkHalfSlittingSide()
        {
            bool bCheck = false;
            foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
            {
                if (rdo.IsChecked == true)
                {
                    bCheck = true;
                }
            }

            if (!bCheck)
            {
                Util.MessageValidation("SFU6030");  // 무지부 방향을 선택하세요.
                return false;
            }
            return true;
        }
        #endregion

    }
}
