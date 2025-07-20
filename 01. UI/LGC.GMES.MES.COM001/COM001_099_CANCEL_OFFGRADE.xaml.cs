using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_099_CANCEL_OFFGRADE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _isPalletFlag = string.Empty;
        public COM001_099_CANCEL_OFFGRADE()
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
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _isPalletFlag = Util.NVC(tmps[0]);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sLotID = txtLotID.Text.ToString();

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2875", sLotID), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {

                    if (result.ToString().Equals("OK"))
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("LOTID", typeof(string));
                        inTable.Columns.Add("PALLET_FLAG", typeof(string));


                        DataRow dr = inTable.NewRow();
                        dr["USERID"] = LoginInfo.USERID;
                        dr["LOTID"] = sLotID;
                        dr["PALLET_FLAG"] = _isPalletFlag ;
                        inTable.Rows.Add(dr);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_OFFGRADE", "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1889"); //정상 처리 되었습니다.

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, inDataSet);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
