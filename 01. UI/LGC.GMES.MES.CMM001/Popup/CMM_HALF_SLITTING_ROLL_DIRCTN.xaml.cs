/*************************************************************************************
 Created Date : 2021.10.19
      Creator : 김지은 책임
   Decription : Ultium Cells GMES 구축 proj. - 무지부/권취 방향 선택
--------------------------------------------------------------------------------------
 [Change History]
  2021.10.19  김지은 책임 : Initial Created.
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
    /// CMM_HALF_SLITTING_ROLL_DIRCTN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_HALF_SLITTING_ROLL_DIRCTN : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        public string SSWDCODE
        {
            get { return sSSWDCode; }
        }

        public string SSWDNAME
        {
            get { return sSSWDName; }
        }

        private string sSSWDCode = string.Empty;  // 무지부/권취 방향 코드
        private string sSSWDName = string.Empty;  // 무지부/권취 방향 이름
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

        public CMM_HALF_SLITTING_ROLL_DIRCTN()
        {
            InitializeComponent();
        }

        #endregion

        #region [Event]

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetComponent();
        }
        
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWorkHalfSlittingSide()) return;

            try
            {
                foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                {
                    if (rdo.IsChecked == true)
                    {
                        sSSWDCode = rdo.Tag.ToString();
                        sSSWDName = rdo.Content.ToString();
                    }
                }

                DialogResult = MessageBoxResult.OK;
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
                RQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "WRK_HALF_SLIT_SIDE";
                dr["ATTRIBUTE1"] = "2";
                dr["ATTRIBUTE3"] = "Y";
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
