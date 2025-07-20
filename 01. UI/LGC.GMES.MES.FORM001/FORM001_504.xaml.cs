/*************************************************************************************
 Created Date : 2018.09.19
      Creator : 
   Decription : 자동차 활성화 후공정 - 개발 /기술 Sample 처리
--------------------------------------------------------------------------------------
 [Change History]


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_504 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_504()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeUserControls()
        {
            //dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            //dtpDateTo.SelectedDateTime = DateTime.Now;
        }
        private void SetControl()
        {
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 개발기술구분
            string[] sFilter = { "SMPL_TYPE_CODE" };
            _combo.SetCombo(cboGubun, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearch);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeUserControls();
            SetControl();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchProcess(false, tb);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchProcess(true);
        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 개발 /기술 Sample 처리 조회
        /// </summary>
        private void SearchProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtSubLotID"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("Cell ID"));
                            return;
                        }
                        else
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("제품"));
                            return;
                        }
                    }
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SMPL_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateTo);

                if (!string.IsNullOrWhiteSpace(txtSubLotID.Text))
                {
                    newRow["SUBLOTID"] = txtSubLotID.Text;
                }
                else
                {
                    newRow["PRODID"] = txtProdid.Text;
                    newRow["SMPL_TYPE_CODE"] = string.IsNullOrWhiteSpace(cboGubun.SelectedValue.ToString()) ? null : cboGubun.SelectedValue.ToString();
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_SAMPLE_LIST_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
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

    }
}
