/*************************************************************************************
 Created Date : 2020.09.17
      Creator : 신광희
   Decription : 데이터 출고(팝업) 
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.17  신광희 : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_027_SAVE_DATAISSUE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public MCS001_027_SAVE_DATAISSUE()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;

            try
            {
                const string bizRuleName = "BR_MCS_REG_LOCATED_ON_THING_DATA_OUT";

                loadingIndicator.Visibility = Visibility.Visible;

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inLot = ds.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                DataTable inCst = ds.Tables.Add("INCST");
                inCst.Columns.Add("CSTID", typeof(string));

                DataRow dr = inLot.NewRow();
                dr["LOTID"] = txtLotId.Text;
                inLot.Rows.Add(dr);

                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT,INCST", null, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                }, ds);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Mehod


        private bool Validation()
        {
            if (txtLotId.Text.Trim().Length == 0)
            {
                Util.MessageValidation("SFU1366"); //LOT ID를 입력해주세요
                return false;
            }
           
            return true;
        }


        #endregion

       
    }
}
