/*************************************************************************************
 Created Date : 2018.06.28
      Creator : 
   Decription : LOT ID 정보 변경
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_071_ADAPT : C1Window, IWorkArea
    {
        #region Declaration
        private string _LotID = string.Empty;
        private string _cstidManageType = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize


        public COM001_071_ADAPT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length == 1)   //파리미터 1개일 경우
            {
                _LotID = Util.NVC(tmps[0]);
                txtFromLotId.Text = _LotID;
                txtToLotId.Text = _LotID;
            }
            else if (tmps != null && tmps.Length == 2)   //파리미터 2개일 경우 
            {
                _LotID = Util.NVC(tmps[0]);
                txtFromLotId.Text = _LotID;
                txtToLotId.Text = _LotID;

                _cstidManageType = Util.NVC(tmps[1]);
            }
        }

        private void Initialize()
        {

        }

        #endregion

        #region [Button]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnAdapt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.Equals(txtFromLotId.Text.Trim(), txtToLotId.Text.Trim()))
                {
                    Util.MessageInfo("SFU4922");    //From LOT ID 와 To LOT ID 가 동일 합니다.
                    return;
                }
                if (string.IsNullOrEmpty(txtUserID.Text.Trim()))
                {
                    Util.MessageInfo("SFU4976");   //요청자를 확인 하세요.
                    return;
                }

                //트레이 관리 LOT 이고 LOT 에 - 문자가 없으면 LOT ID 확인 필요함
                if ("Y".Equals(_cstidManageType) && txtToLotId.Text.IndexOf('-') <= 0)
                {
                    Util.MessageInfo("SFU5017");   //신규 LOT ID 형식을 확인해 주세요.
                    return;
                }

                //변경 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2875"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                LOTAdapt();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {            
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtUserName.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {
                        txtUserName.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                        txtUserID.Text = dtRslt.Rows[0]["USERID"].ToString();
                    }
                    else
                    {
                        dgPersonSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgPersonSelect);

                        dgPersonSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                        this.Focusable = true;
                        this.Focus();
                        this.Focusable = false;
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
        }
        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtUserID.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtUserName.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());            

            dgPersonSelect.Visibility = Visibility.Collapsed;

        }

        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private void LOTAdapt()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["REQ_USERID"] = Util.NVC(txtUserID.Text);
                newRow["NOTE"] = Util.NVC(txtNote.Text);
                inDataTable.Rows.Add(newRow);

                DataTable inLOT = indataSet.Tables.Add("INLOT");
                inLOT.Columns.Add("FROM_LOTID", typeof(string));
                inLOT.Columns.Add("TO_LOTID", typeof(string));
                inLOT.Columns.Add("TO_CSTID", typeof(string));

                newRow = null;
                newRow = inLOT.NewRow();
                newRow["FROM_LOTID"] = Util.NVC(txtFromLotId.Text);
                newRow["TO_LOTID"] = Util.NVC(txtToLotId.Text);
                //트레이 관리 LOT 이고 LOT 에 - 문자가 포함되어 있으면 - 앞이 트레이 ID
                if ("Y".Equals(_cstidManageType) && txtToLotId.Text.IndexOf('-') > 0)
                {
                    newRow["TO_CSTID"] = txtToLotId.Text.Substring(0, txtToLotId.Text.IndexOf('-'));
                }

                inLOT.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_COPY_LOT", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion


        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

        private void txtToLotId_TextChanged(object sender, TextChangedEventArgs e)
        {
            //트레이 관리 LOT 이고 LOT 에 - 문자가 포함되어 있으면 - 앞이 트레이 ID
            if ( "Y".Equals(_cstidManageType) && txtToLotId.Text.IndexOf('-') > 0)
            {
                txtToCstId.Text = txtToLotId.Text.Substring(0, txtToLotId.Text.IndexOf('-'));
            }
            else
            {
                txtToCstId.Text = string.Empty;
            }
        }
    }
}
