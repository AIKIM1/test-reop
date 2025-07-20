/*************************************************************************************
 Created Date : 2017.08.30
      Creator : 오화백
   Decription : 활성화 재튜빙 생산실적 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.30  오화백 : Initial Created.


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;





namespace LGC.GMES.MES.COM001
{
    public partial class COM001_104 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public COM001_104()
        {
            InitializeComponent();
            InitCombo();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 해더 생성
            SetGridHeader();
            this.Loaded -= UserControl_Loaded;
        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        #endregion


        #region Initialize

        #endregion

        #region Event
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //동
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

                dtpFrom.SelectedDateTime = System.DateTime.Now;
                dtpTo.SelectedDateTime = System.DateTime.Now;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            SetDataBinding();
        }
        #endregion

        #region Mehod
        private void SetGridHeader()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "FORM_RETUBE_RESN_CODE";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                Util.gridClear(dgRetube);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    for (int row = 0; row < dtResult.Rows.Count; row++)
                    {
                        Util.SetGridColumnNumeric(dgRetube, dtResult.Rows[row]["CBO_CODE"].ToString(), null, dtResult.Rows[row]["CBO_NAME"].ToString(), true, true, true, true, -1, HorizontalAlignment.Right, Visibility.Visible, "#,##0");  // 부품 항목 앞으로 위치 이동.
                    }

                    Util.SetGridColumnText(dgRetube, "SHIFT", null, ObjectDic.Instance.GetObjectName("작업조"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgRetube, "WRK_USER_NAME", null, ObjectDic.Instance.GetObjectName("작업자"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgRetube, "WIPDTTM_ED", null, ObjectDic.Instance.GetObjectName("작업일시"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgRetube, "RE_TUBE_FLAG", null, ObjectDic.Instance.GetObjectName("RE_TUBE_FLAG"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                    Util.SetGridColumnText(dgRetube, "RE_TUBE_FLAG_NAME", null, ObjectDic.Instance.GetObjectName("작업유형"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgRetube, "ERP_TRNF_SEQNO", null, ObjectDic.Instance.GetObjectName("ERP전송번호"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgRetube, "ERP_ERR_TYPE_CODE", null, ObjectDic.Instance.GetObjectName("ERP_ERR_TYPE_CODE"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                    Util.SetGridColumnText(dgRetube, "ERP_ERR_TYPE_CODE_NAME", null, ObjectDic.Instance.GetObjectName("ERP처리결과"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgRetube, "ERP_ERR_CAUSE_CNTT", null, ObjectDic.Instance.GetObjectName("ERP오류내용"), false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Visible);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetDataBinding()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("DATE_FR", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
               
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DATE_FR"] = Util.GetCondition(dtpFrom);
                dr["DATE_TO"] = Util.GetCondition(dtpTo);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["PRJT_NAME"] = string.IsNullOrWhiteSpace(txtPjt.Text) ? null : txtPjt.Text;
                dr["PRODID"] = string.IsNullOrWhiteSpace(txtProd.Text) ? null : txtProd.Text;
                dr["LOTID"] = string.IsNullOrWhiteSpace(txtLotRt.Text) ? null : txtLotRt.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SFC_RE_TUBE_WRK_HIST_LIST", "INDATA", "OUTDATA", dtRqst);

                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                Util.GridSetData(dgRetube, dtRslt, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation())
            {
                return;
            }

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;

            inDataTable.Rows.Add(row);

            //제품의뢰
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
         
           
                DataRow[] drSelect = DataTableConverter.Convert(dgRetube.ItemsSource).Select("CHK = 1");

                foreach (DataRow drPrint in drSelect)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = drPrint["LOTID"];
                    inLot.Rows.Add(row);
                }
           

            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_LOT_RT", "INDATA,INLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SetDataBinding();
                        }
                    });
                  


                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_DELETE_LOT_RT", ex.Message, ex.ToString());

            }

        }

        private void dgRetube_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //그룹조회 탭의 전체 선택 
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgRetube.ItemsSource == null) return;

            DataTable dt = ((DataView)dgRetube.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgRetube.ItemsSource == null) return;

            DataTable dt = ((DataView)dgRetube.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();


        }

        private bool Validation()
        {

            if (dgRetube.Rows.Count == 0)
            {

                Util.MessageValidation("SFU1905");//조회된 데이터가 없습니다.
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgRetube, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }
           return true;
        }


    }
}
