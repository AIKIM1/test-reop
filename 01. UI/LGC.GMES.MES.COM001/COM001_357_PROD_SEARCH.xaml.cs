using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_357_PROD_SEARCH : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private string _processCode = string.Empty;
        public string prodId = string.Empty;
        public string modlId = string.Empty;
        public string prjtName = string.Empty;
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_357_PROD_SEARCH()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(prodId))
                {
                    Util.MessageValidation("SFU1636");
                    return;
                }
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod



        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string prodId = txtProdId.Text.ToString();

                if (string.IsNullOrEmpty(prodId))
                {
                    Util.MessageValidation("SFU4342", "1"); //[%1] 자리수 이상 입력하세요.
                    return;
                }

                SearchProd(prodId);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        private void dgLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            prodId = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString());
        }

        private void SearchProd(string prodId)
        {
            try
            {
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("PRODID", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["PRODID"] = prodId;
                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROD_INFO", "RQSTDT", "RSLTDT", dtRqstDt);

                Util.GridSetData(dgProdId, dtResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgProdChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            prodId = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRODID").ToString());
            modlId = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "MODLID").ToString());
            prjtName = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRJT_NAME").ToString());
        }
    }
    #endregion
}
