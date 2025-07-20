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

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_CHANGE_AOMM_GRADE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 


        public FORM001_CHANGE_AOMM_GRADE()
        {
            InitializeComponent();
            setComboAommGrade(cboAommGrade);
        }

        private void setComboAommGrade(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "AOMM_GRADE";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
            object[] tmps = C1WindowExtension.GetParameters(this);

        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet inData = new DataSet();

                DataTable RQSTDT = inData.Tables.Add("INDATA");

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SRCTYPE",typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("CTNRID", typeof(string));
                RQSTDT.Columns.Add("AOMM_GRD_CODE", typeof(string));
                RQSTDT.Columns.Add("CTNR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = "UI";
                dr["USERID"] = LoginInfo.USERID;
                dr["CTNRID"] = txtCtnrID.Text.ToString();
                dr["AOMM_GRD_CODE"] = cboAommGrade.SelectedValue.ToString();
                dr["CTNR_TYPE_CODE"] = "CART";

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_PALLET_AOMM_GRADE", "INDATA", "OUTDATA", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    ConfirmNewCtnr(Result.Tables["OUTDATA"].Rows[0]["CTNRID"].ToString());

                }, inData);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmNewCtnr(string sCtnrID)
        {
            try
            {
                FORM001_CHANGE_AOMM_GRADE_CONFIRM popupConfirm = new FORM001_CHANGE_AOMM_GRADE_CONFIRM();
                popupConfirm.FrameOperation = this.FrameOperation;

                object[] parameters = new object[1];

                parameters[0] = sCtnrID;

                C1WindowExtension.SetParameters(popupConfirm, parameters);

                //grdMain.Children.Add(popupConfirm);
                //popupConfirm.BringToFront();

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupConfirm);
                        popupConfirm.BringToFront();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
