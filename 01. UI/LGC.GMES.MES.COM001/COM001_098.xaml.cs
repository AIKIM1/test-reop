using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_098.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_098 : UserControl, IWorkArea
    {
        #region Initialize
        public COM001_098()
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

            string postData = "gemsDoc=" + "Y" + "&bizUnitCode=" + GetDivisionCode() + "&language=" + sLang;
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(postData);
            string url = "http://plm.lgensol.com/plm/ssoGMES.jsp";
            webBrowser.Navigate(url, string.Empty, bytes, "Content-Type: application/x-www-form-urlencoded");            
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
