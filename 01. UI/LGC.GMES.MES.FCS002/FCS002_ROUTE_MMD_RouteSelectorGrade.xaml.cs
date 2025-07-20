/*************************************************************************************
 Created Date : 2022.12.15
      Creator : 이윤중
   Decription : Route 정보 조회 화면
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.15  이윤중 : Initial Created. (MMD - [Route 관리] 화면 기반)
  2023.10.17  주훈   : LGC.GMES.MES.FROM001.FROM001_ROUTE_MMD_RouteSelectorGrade 초기 복사
**************************************************************************************/
using System;
using System.Windows;
using System.Data;

using C1.WPF;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// FCS002_ROUTE_MMD_RouteSelectorGrade.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_ROUTE_MMD_RouteSelectorGrade : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _sSLT_GRD_CODE = string.Empty;

        public string _pSLT_GRD_CODE
        {
            get { return _sSLT_GRD_CODE; }
        }
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
        public FCS002_ROUTE_MMD_RouteSelectorGrade()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= C1Window_Loaded;

            object[] param = C1WindowExtension.GetParameters(this);


            string sAREAID = Util.NVC(param[0]);
            string sAREANAME = Util.NVC(param[1]);
            string sROUTID = Util.NVC(param[2]);
            string sROUTNAME = Util.NVC(param[3]);
            string sSLT_GRD_CODE = Util.NVC(param[4]);

            txtAREA.Text = sAREANAME;
            txtAREA.Tag = sAREAID;

            txtROUT.Text = sROUTNAME;
            txtROUT.Tag = sROUTID;

            txtSEL_GRD.Text = sSLT_GRD_CODE;
            txtSEL_GRD.Tag = sSLT_GRD_CODE;

            InitializeGrid();
        }

        //private void btnSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        DataTable dtCurrentGrad = ((DataView)lstGradeList.ItemsSource).Table;

        //        string sSLT_GRD_CODE = string.Empty;
        //        for (int iCnt = 0; iCnt < dtCurrentGrad.Rows.Count; iCnt++)
        //        {
        //            if (dtCurrentGrad.Rows[iCnt]["CHK"].ToString() == "True")
        //            {
        //                sSLT_GRD_CODE = sSLT_GRD_CODE + dtCurrentGrad.Rows[iCnt]["CMCODE"].ToString() + ",";
        //            }
        //        }

        //        _sSLT_GRD_CODE = sSLT_GRD_CODE;

        //        DialogResult = MessageBoxResult.OK;

        //    }
        //    catch (Exception ex)
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        private void InitializeGrid()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "MMD_SEL_ROUT_SLT_GRADE_CBO";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("JUDG_CODE", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = txtAREA.Tag;
                inData["JUDG_CODE"] = null;
                inData["USE_FLAG"] = "Y";

                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataTable dtSelGradList = result;

                    DataTable dtGradList = dtSelGradList.Clone();
                    dtGradList.Columns.Add("CHK", typeof(string));

                    string[] sRwkGradeList = txtSEL_GRD.Tag.ToString().Split(',');

                    for (int iCnt = dtSelGradList.Rows.Count - 1; iCnt >= 0; iCnt--)
                    {
                        DataRow dr = dtGradList.NewRow();
                        dr["CMCODE"] = dtSelGradList.Rows[iCnt]["CMCODE"];
                        dr["CMCDNAME"] = dtSelGradList.Rows[iCnt]["CMCDNAME"];
                        dr["CMCDNAME1"] = dtSelGradList.Rows[iCnt]["CMCDNAME1"];

                        foreach (string sRwkGrade in sRwkGradeList)
                        {
                            if (dtSelGradList.Rows[iCnt]["CMCODE"].ToString() == sRwkGrade)
                            {
                                dr["CHK"] = true;
                                break;
                            }
                            else
                            {
                                dr["CHK"] = false;
                            }
                        }
                        dtGradList.Rows.Add(dr);
                        dtSelGradList.Rows.RemoveAt(iCnt);
                    }

                    dtGradList.AcceptChanges();
                    lstGradeList.ItemsSource = DataTableConverter.Convert(dtGradList);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
    }
}
