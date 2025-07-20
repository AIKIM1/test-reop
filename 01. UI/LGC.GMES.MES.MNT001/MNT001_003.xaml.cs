using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;

namespace LGC.GMES.MES.MNT001
{
    /// <summary>
    /// MNT001_003.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MNT001_003 : C1Window, IWorkArea
    {
        #region Initialize
        public MNT001_003()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            string sLang = string.Empty;
            if (string.Equals(LoginInfo.LANGID, "ko-KR"))
                sLang = "ko";
            else
                sLang = "en";

            //20240514 PLM -> ESM 작업 조회 시스템 변경으로 인한 URL 변경 테스트
            //운영 배포 시 변경집합번호 48274 버전으로 수정 바랍니다.
            //string url = "http://plm.lgensol.com/plm/ssoGMES.jsp?";
            //string postData = "gemsDoc=" + "Y" + "&bizUnitCode=" + GetDivisionCode() + "&language=" + sLang;
            //System.Diagnostics.Process.Start(url + postData);

            //20240514 PLM -> ESM 작업 조회 시스템 변경으로 인한 URL 변경 테스트
            //string url = "http://sdm.lgensol.com/xmms/webroot/category/categoryFrame.do?process=listForm&selCategoryID=20000003";
            //System.Diagnostics.Process.Start(url);
            /////////////////////////////////////////////////////////////////////
            //20240806 PLM -> ESM 작업 조회 시스템 변경으로 인한 URL 변경 테스트
            //string url = "http://10.94.23.21/xmms/webroot/LegacyLogin.do?systemID=MES";
            //System.Diagnostics.Process.Start(url);

            string url = "http://plm.lgensol.com/plm/ssoGMES.jsp?"; // 2024.11.06. 김영국 - URL 변경 (임성운 선임 요청)
            string postData = "gemsDoc=" + "Y" + "&bizUnitCode=" + GetDivisionCode() + "&language=" + sLang;
            System.Diagnostics.Process.Start(url + postData);
            
            /////////////////////////////////////////////////////////////////////


            this.Close();
        }
        #endregion

        #region Biz Call
        private string GetDivisionCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("SHOPID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHOP_FOR_DVZN_CODE", "INDATA", "RSLTDT", dt);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["DVZN_CODE"]);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return "";
        }
        #endregion
    }
}
