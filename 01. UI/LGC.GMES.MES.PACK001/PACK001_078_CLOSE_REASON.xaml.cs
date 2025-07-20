/*************************************************************************************
 Created Date : 2021.03.22
      Creator : 김민석
   Decription : CELL 공급 프로젝트 PACK 팝업 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Documents;
using System.Data;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_078_CLOSE_REASON : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public bool bClick = false;
        string strAreaID = string.Empty;
        string strMtrlID = string.Empty;
        CommonCombo _combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_078_CLOSE_REASON()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {

                }

                InitCombo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            try
            {
                String[] sFiltercboAreaRslt = { "CELL_SPLY_REQ_CLSE_RESN" };
                _combo.SetCombo(cboCloseReason, CommonCombo.ComboStatus.SELECT, sFilter: sFiltercboAreaRslt, sCase: "COMMCODE_WITHOUT_CODE");
                
                //SetcboCloseReason();
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Event
        private void btnReqClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sSelectedCloseReason = cboCloseReason.SelectedItem.ToString();

                if (string.IsNullOrEmpty(cboCloseReason.SelectedValue.ToString()) || cboCloseReason.SelectedValue.ToString() == "SELECT")
                {
                    Util.Alert("SFU1651");//선택된 항목이 없습니다.
                    return;
                }

                if(cboCloseReason.SelectedValue.ToString() == "REQ_CLSE_RESN_004")
                {
                    TextRange textRange = new TextRange(txtCloseReason.Document.ContentStart, txtCloseReason.Document.ContentEnd);
                    if (textRange.Text.Equals("\r\n") || textRange.Text.Equals("") || textRange.Text.Trim().Equals(""))
                    {
                        ms.AlertWarning("SFU1594"); //사유를 입력하세요.
                        return;
                    }
                    else
                    {
                        this.DataContext = textRange;
                        this.DialogResult = MessageBoxResult.OK;

                        this.Close();
                    }
                }
                else
                {
                    this.DataContext = ((System.Data.DataRowView)cboCloseReason.SelectedItem).Row.ItemArray[1];
                    this.DialogResult = MessageBoxResult.OK;

                    this.Close();
                }




            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bClick = false;

                if (bClick == false)
                {
                    bClick = true;
                    if (bClick == true)
                    {
                        this.DialogResult = MessageBoxResult.Cancel;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                //HiddenLoadingIndicator();

                bClick = false;
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Mehod

        #region Refresh
        private void Refresh()
        {
            try
            {
                //그리드 clear
                //Util.gridClear(dgCellReq);
                //Util.gridClear(dgCellConf);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Refresh

        #endregion

        #region Grid
        private void dgCellReq_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //2020.06.26
                C1DataGrid dataGrid = (sender as C1DataGrid);

                Action act = () =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                };


            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();

                //bClick = false;
            }
        }
        #endregion

        #region ComboBox
        //private void SetcboCloseReason()
        //{
        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["CMCDTYPE"] = "CELL_SPLY_REQ_CLSE_RESN";

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ALL_CBO", "RQSTDT", "RSLTDT", RQSTDT);

        //        cboCloseReason.ItemsSource = DataTableConverter.Convert(dtResult);

        //        cboCloseReason.DisplayMemberPath = "CBO_NAME";
        //        cboCloseReason.SelectedValuePath = "CBO_CODE";

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        #endregion

        private void cboCloseReason_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(cboCloseReason.SelectedValue.ToString() == "REQ_CLSE_RESN_004")
            {
                txtCloseReason.Background = new SolidColorBrush(Colors.White);
                txtCloseReason.IsEnabled = true;
            }
            else
            {
                txtCloseReason.Background = new SolidColorBrush(Colors.Gray);
                txtCloseReason.IsEnabled = false;
            }
            
        }
    }
}
