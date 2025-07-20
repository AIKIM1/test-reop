/*************************************************************************************
 Created Date : 2016.11.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - WORKORDER BOM 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.22  INS 김동일K : Initial Created.
  
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_355_USER_DETAIL : C1Window, IWorkArea
    {
        private string[] _USERID = new string[3];

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_355_USER_DETAIL()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            int cnt = 0;

            for(int i = 0; i < tmps.Length; i++)
            {
                if (tmps[i] == null)
                {
                    cnt++;
                    continue;
                }
                _USERID[i] = tmps[i].ToString();
            }

            if(cnt == tmps.Length)
                this.DialogResult = MessageBoxResult.Cancel;

            GetUserDetail();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetUserDetail()
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_PERSON_DETAIL";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));

                // INDATA
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable inUser = inDataSet.Tables.Add("INUSER");
                inUser.Columns.Add("USERID", typeof(string));


                for (int i = 0; i < _USERID.Length; i++)
                {
                    if (string.IsNullOrEmpty(_USERID[i]))
                        continue;

                    newRow = inUser.NewRow();
                    newRow["USERID"] = _USERID[i].ToString();
                    inUser.Rows.Add(newRow);
                }

                if (inUser.Rows.Count == 0)
                    this.DialogResult = MessageBoxResult.Cancel;

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INUSER", "OUTDATA",  (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        dgUser.ItemsSource = DataTableConverter.Convert(bizResult.Tables["OUTDATA"]);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
