/*************************************************************************************
 Created Date : 2019.08.07
      Creator : 염규범
  Description :
--------------------------------------------------------------------------------------
 [Change History]
  2019.08.07  염규범    Initialize
  2021.12.24  정용석    부모 Form에서 넘어온 데이터 가지고 Grid Data 표출 (기존 순서도 호출 제외시킴)
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_375_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_375_POPUP()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function List
        private void ShowUnwholesomenessLOTList(object obj)
        {
            try
            {
                SelectsId((string)obj);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void SelectsId(string Name)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("NAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["NAME"] = Name;
                RQSTDT.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ID_CONTENTS", "RQSTDT", "RSLTDT", RQSTDT);
                textBox.Text = Util.NVC(dtResult.Rows[0]["CONTENTS"]);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] objParam = C1WindowExtension.GetParameters(this);
            if (objParam == null || objParam.Length <= 0)
            {
                return;
            }
            this.ShowUnwholesomenessLOTList(objParam[0]);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}