/*************************************************************************************
 Created Date : 2017.08.21
      Creator : INS 오화백K
   Decription : 활성화 후공정 - QMS 검사의뢰 - Tag발행
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.21  INS 오화백K : Initial Created.
  2023.03.16  LEEHJ       : 소형활성화 MES 복사
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
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// CMM_ASSY_CANCEL_TERM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_309_PRINT : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private string _REQID = string.Empty;
        private string _INSP_QTY = string.Empty;
        private string _CHECK = string.Empty;
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
        public FCS002_309_PRINT()
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

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps == null)
                    return;
                ApplyPermissions();
                //"R" = QMS의뢰
                //"C" = 의뢰취소
                //"CD" = 물청취소

                _CHECK = Util.NVC(tmps[0]);

                if (_CHECK == "R")
                {
                    _REQID = Util.NVC(tmps[1]);
                    _INSP_QTY = Util.NVC(tmps[2]);
                    DataTable ReqLotInfo = tmps[3] as DataTable;
                    // GetLotInfo(_REQID);
                    GetReqLotInfo(ReqLotInfo);
                }
                else if (_CHECK == "C")
                {
                    _REQID = Util.NVC(tmps[1]);
                    _INSP_QTY = Util.NVC(tmps[2]);
                    GetLotInfo(_REQID);
                }
                else if (_CHECK == "CD")
                {
                    DataTable SEQ = tmps[1] as DataTable;
                    _INSP_QTY = Util.NVC(tmps[2]);
                    GetLotInfo(SEQ);

                }
                this.Loaded -= C1Window_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
               
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                Util.MessageConfirm("SFU4272", (result) =>// 발행하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Tag();
                    }
                });
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

        #region [BizCall]


        private void GetReqLotInfo(DataTable ReqLotInfo)
        {
            try
            {
                Util.GridSetData(dgLotInfo, ReqLotInfo, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void GetLotInfo(string Reqid)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("INSP_REQ_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = null;
                       dr = dtRqst.NewRow();
                       dr["INSP_REQ_ID"] = Reqid.ToString();
                       dr["LANGID"] = LoginInfo.LANGID;
                          dtRqst.Rows.Add(dr);
                
               DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QMS_TAG_SEL", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgLotInfo, dtRslt, FrameOperation, true);
                }
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void GetLotInfo(DataTable Result)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("INSP_REQ_SEQNO", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = null;
                dr = dtRqst.NewRow();
                for (int i = 0; i < Result.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();
                    dr["INSP_REQ_SEQNO"] = Result.Rows[i]["INSP_REQ_SEQNO"].ToString();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QMS_TAG_SEL_DETAIL", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgLotInfo, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void Tag()
        {
            try
            {
               // SET PARAMETER
                object[] parameters = new object[8];
                CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
                popupTagPrint.FrameOperation = this.FrameOperation;
                //남경
                if (LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    //특성 Grader
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "PROCID")).Equals(Process.CircularCharacteristicGrader))
                    {
                        popupTagPrint.QMSRequestPalletYN = "Y";
                    }
                    else
                    {
                        // 초소형 일경우 오창이랑 동일함
                        if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "CLSS3_CODE")).Equals("MCS"))
                        {
                            popupTagPrint.QMSRequestPalletYN = "Y";
                        }
                        else //원각형
                        {
                            popupTagPrint.returnPalletYN = "Y";
                        }
                      
                    }

                }
                else
                {
                    popupTagPrint.QMSRequestPalletYN = "Y";
                }



                


                for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")) == "True"|| Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")) == "1")
                    {

                      
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "PROCID"));
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPTID"));
                        parameters[2] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID"));
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSEQ"));
                        parameters[4] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPQTY")).Replace(",", "")).ToString();
                        parameters[5] ="N";      // 디스패치 처리
                        parameters[6] = "Y";
                        parameters[7] = "N";      // Direct 출력 여부

                       

                    }
                }
                C1WindowExtension.SetParameters(popupTagPrint, parameters);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupTagPrint);
                        popupTagPrint.BringToFront();
                        break;
                    }
                }
                popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
           }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }
        #endregion

        #region [Validation]
        private bool Validation()
        {

            int ChkCount = 0;
            for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")) == "True" || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")) == "1")
                {
                    ChkCount = ChkCount + 1;
                }
            }
            if(ChkCount == 0)
            {
                Util.MessageValidation("SFU3538");
                return false;
            }
            return true;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPrint);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }



        #endregion

        #endregion

        private void dgLotChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        dgLotInfo.SelectedIndex = idx;
                    }
                }
            
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }
    }
}
