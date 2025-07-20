/*************************************************************************************
 Created Date : 2019.05.29
      Creator : 이제섭
   Decription : CNA 6호 종이박스포장기 BOX 추가 팝업
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_005_ADD_BOX.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class BOX001_005_ADD_BOX : C1Window, IWorkArea
    {
        Util _Util = new Util();

        string sWorker = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_005_ADD_BOX()
        {
            Loaded += BOX001_005_ADD_BOX_Loaded;
            InitializeComponent();
        }
        private void BOX001_005_ADD_BOX_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_005_ADD_BOX_Loaded;
            InitSet();

        }

        private void InitSet()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            txtPalletID.Text = tmps[0] as string;
            sWorker = tmps[1] as string;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {


        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    ////txtLot.Text = sNewLot;

                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void AddBox()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INPALLET");
                inData.Columns.Add("BOXID");
                inData.Columns.Add("USERID");
                inData.Columns.Add("SRCTYPE");

                DataRow inDataRow = inData.NewRow();
                inDataRow["BOXID"] = txtPalletID.Text;
                inDataRow["USERID"] = sWorker;
                inDataRow["SRCTYPE"] = "UI";
                inData.Rows.Add(inDataRow);

                DataTable inBoxTable = inDataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");

                DataRow inBoxdr = inBoxTable.NewRow();
                inBoxdr["BOXID"] = txtBoxID.Text;

                inBoxTable.Rows.Add(inBoxdr);

                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_ADD_BOX_FOR_CNA_CP", "INPALLET,INBOX", null, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

            //정상처리되었습니다.
            Util.MessageInfo("SFU1275");
        }

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddBox();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (txtBoxID.Text == string.Empty)
            {
                //BoxID를 입력하세요.
                Util.MessageInfo("SFU4391");
                return;
            }

            AddBox();
        }
    }
}
