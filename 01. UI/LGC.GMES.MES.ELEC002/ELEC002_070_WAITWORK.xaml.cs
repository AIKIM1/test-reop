/*************************************************************************************
 Created Date : 2021.01.10
      Creator : ����ȿ
   Decription : ������ ��ȸ
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.10  ����ȿ : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace LGC.GMES.MES.ELEC002
{
    public partial class ELEC002_070_WAITWORK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        public DataTable dtSelect = new DataTable();
        /// <summary>
        /// Parameter
        /// </summary>
        private string _EqsgID = string.Empty;

        public ELEC002_070_WAITWORK()
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

        /// <summary>
        /// Form Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //����� ���Ѻ��� ��ư �����
                List<Button> listAuth = new List<Button>();

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                //������� ����� ���Ѻ��� ��ư �����

                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps.Length > 0)
                {
                    _EqsgID = tmps[0].ToString();

                }

                btnSearch_Click(null, null);

                txtLotId.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        /// <summary>
        /// ��ȸ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgList);

                txtLotId.Text = "";

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            
        }


        /// <summary>
        /// Lot KeyDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                {
                    Util.MessageValidation("SFU3180");  //���� �� �ٿ��ֱ� ����.
                    txtLotId.IsEnabled = false;
                    txtLotId.IsEnabled = true;
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    GetInfo(txtLotId.Text);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// �˾� �ݱ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// ���� �� �ٿ��ֱ� ����.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotId_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Util.MessageValidation("SFU3180");  //���� �� �ٿ��ֱ� ����.
            txtLotId.IsEnabled = false;
            txtLotId.IsEnabled = true;
            return;
        }


        /// <summary>
        /// ���� �� �ٿ��ֱ� ����.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotId_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Util.MessageValidation("SFU3180");  //���� �� �ٿ��ֱ� ����.
            txtLotId.IsEnabled = false;
            txtLotId.IsEnabled = true;
            return;
        }

        #endregion


        #region Mehod

        /// <summary>
        /// ����Ʈ ��ȸ
        /// </summary>
        public void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = _EqsgID;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                string bizRuleName = string.Empty;

                bizRuleName = "DA_PRD_SEL_WAIT_WIP_RW";

                ShowLoadingIndicator();
                DoEvents();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        public void GetInfo(string lotid)
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = _EqsgID;
                dr["LOTID"] = lotid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                string bizRuleName = string.Empty;
                bizRuleName = "DA_PRD_SEL_WAIT_WIP_RW";

                //ShowLoadingIndicator();
                DoEvents();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);



                //DataTable dtTo = DataTableConverter.Convert(dgTarget.ItemsSource);

                //if (dtTo.Columns.Count == 0)//���� ���ε��� �ȵ� ���¶� �÷����� ����. �߰��� ó��
                //{
                //    dtTo.Columns.Add("CHK", typeof(Boolean));
                //    dtTo.Columns.Add("LOTID", typeof(string));
                //    dtTo.Columns.Add("VERIF_SEQS", typeof(string));
                //    dtTo.Columns.Add("WIPQTY", typeof(decimal));
                //    dtTo.Columns.Add("PRJT_NAME", typeof(string));
                //    dtTo.Columns.Add("PRODID", typeof(string));
                //    dtTo.Columns.Add("PROD_VER_CODE", typeof(string));
                //    dtTo.Columns.Add("REWND_WRK_ORD", typeof(string));
                //    dtTo.Columns.Add("WIPHOLD", typeof(string));
                //}


                if (dgTarget.GetRowCount() == 0)
                {
                    Util.GridSetData(dgTarget, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgTarget.ItemsSource);
                    if (dtInfo.Select("LOTID = '" + lotid + "'").Length > 0) //�ߺ����� üũ
                    {
                        return;
                    }
                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgTarget, dtInfo, FrameOperation);

                }
                dtSelect = DataTableConverter.Convert(dgTarget.ItemsSource);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                txtLotId.Clear();
            }
        }

        #region [Validation]

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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #endregion

        private void btnSelect_Click(object sender, RoutedEventArgs e) // �˾��� �����鼭 ������ô ���� ��ܿ� Lot�� ����������..
        {
            if(dtSelect.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1632");
                return;
            }


            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgTarget);
            dtSelect.Clear();
        }
    }
}  