/*************************************************************************************
 Created Date : 2019.10.30
      Creator : 이동우
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리 - UNHOLD 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2019.10.30 기존 BOX213_UNHOLD.xmal Copy 후 작성

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_213_UNHOLD_CWA : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        // string _UserID = string.Empty;
        int allCnt = 0;
        int selectCnt = 0;

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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_213_UNHOLD_CWA()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitCombo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            // 시장유형 MKT_TYPE_CODE
            //DataTable dtMrk = dtTypeCombo("MKT_TYPE_CODE", ComboStatus.ALL);
            //(dgHold.Columns["MKT_TYPE_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMrk);

            // 보류범위 HOLD_TRGT_CODE  
            DataTable dtHold = dtTypeCombo("HOLD_TRGT_CODE", ComboStatus.NONE);
            (dgHold.Columns["HOLD_TRGT_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtHold);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {

            object[] tmps = C1WindowExtension.GetParameters(this);
            DataTable dtInfo = (DataTable)tmps[0];
            string holdGroupID = tmps[2].ToString();
            string holdHoldReason = tmps[3].ToString();
            if (dtInfo == null)
            {
                dtInfo = new DataTable();

                for (int i = 0; i < dgHold.Columns.Count; i++)
                {
                    dtInfo.Columns.Add(dgHold.Columns[i].Name);
                }
            }
            else
                chkAll.IsChecked = true;       

            Util.GridSetData(dgHold, dtInfo, FrameOperation);

            if(tmps[1].ToString() == "Y")
            {
                grdSelect.Visibility = Visibility.Visible;
                grdHoldGroupReason.Visibility = Visibility.Visible;

            }
            txtGroupID.Text = holdGroupID;
            txtHoldReason.Text = holdHoldReason;
            allCnt = int.Parse(tmps[4].ToString());
            selectCnt = allCnt;
            selectCellCount.Text = ObjectDic.Instance.GetObjectName("대상 Cell 목록") + " (" + ObjectDic.Instance.GetObjectName("전체") + " : " + allCnt + ObjectDic.Instance.GetObjectName("개") + ", " + ObjectDic.Instance.GetObjectName("선택") +" : "+selectCnt + ObjectDic.Instance.GetObjectName("개") + ")";

        }

        private void dgHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #region Hold 리스트 추가/제거
        /// <summary>
        /// 엑셀 업로드 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageInfo("개발중");
        }

        /// <summary>
        /// 엑셀 업로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageInfo("개발중");
        }

        /// <summary>
        /// HOLD 리스트 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            Util.GridSetData(dgHold, dt, FrameOperation);
            dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        /// <summary>
        /// HOLD 리스트 제외
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);

            List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            if (drList.Count > 0)
            {
                foreach (DataRow dr in drList)
                {
                    dt.Rows.Remove(dr);
                }
                Util.GridSetData(dgHold, dt, FrameOperation);
                chkAll.IsChecked = false;
            }
        }
        #endregion

        #region 저장/닫기 버튼 이벤트

        /// <summary>
        /// HOLD 해제
        /// BIZ : BR_PRD_REG_ASSY_UNHOLD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // SFU4046 HOLD 해제 하시겠습니까?
            Util.MessageConfirm("SFU4046", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void rdoGroupAll_Checked(object sender, RoutedEventArgs e)
        {
            selectMode.Text = string.Empty;
            selectMode.Text = ObjectDic.Instance.GetObjectName("그룹단위");

            try { txtCellID.IsEnabled = false; }
            catch { }
        }

        private void rdoCell_Checked(object sender, RoutedEventArgs e)
        {
            selectMode.Text = string.Empty;
            selectMode.Text = ObjectDic.Instance.GetObjectName("개별 Cell단위");

            try { txtCellID.IsEnabled = true; }
            catch { }

            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                    selectCnt = 0;
                }
            }

        }

        private void txtCellID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }
                    DataTable dtHold = DataTableConverter.Convert(dgHold.ItemsSource);
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                       // System.Windows.Forms.Application.DoEvents();
                       for(int j = 0; j < dtHold.Rows.Count; j++)
                        {
                            if(dtHold.Rows[j]["STRT_SUBLOTID"].ToString() == sPasteStrings[i])
                            {
                                //dtHold.Rows[j]["CHK"] = "True";
                                DataTableConverter.SetValue(dgHold.Rows[j].DataItem, "CHK", true);
                                System.Windows.Forms.Application.DoEvents();
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {

        }
        #endregion

        #region Biz

        /// <summary>
        /// Hold 해제 
        /// </summary>
        private void Save()
        {
            try
            {
                if (string.IsNullOrEmpty(txtNote.Text))
                {
                    //SFU4301		Hold 해제 사유를 입력하세요.	
                    Util.MessageValidation("SFU4301");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
                if (dtInfo.Rows.Count < 1)
                {
                    return;
                }


                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("UNHOLD_NOTE");
                inDataTable.Columns.Add("SHOPID");


                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("HOLD_ID");
                inHoldTable.Columns.Add("SUBLOTID");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_NOTE"] = txtNote.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataTable.Rows.Add(newRow);
                newRow = null;

                if (LoginInfo.CFG_SHOP_ID.Equals("G182")) // 소형 파우치
                {
                    string sXML = string.Empty;

                    for (int row = 0; row < dtInfo.Rows.Count; row++)
                    {
                        if (row == 0 || row % 500 == 0)
                        {
                            sXML = "<root>";
                        }

                        sXML += "<DT><L>" + dtInfo.Rows[row]["HOLD_ID"] + "</L></DT>";

                        if ((row + 1) % 500 == 0 || row + 1 == dtInfo.Rows.Count)
                        {
                            sXML += "</root>";

                            newRow = inHoldTable.NewRow();
                            newRow["HOLD_ID"] = sXML;
                            inHoldTable.Rows.Add(newRow);
                        }
                    }
                }
                else // 자동차
                {
                    for (int row = 0; row < dtInfo.Rows.Count; row++)
                    {
                        newRow = inHoldTable.NewRow();
                        newRow["HOLD_ID"] = dtInfo.Rows[row]["HOLD_ID"];
                        inHoldTable.Rows.Add(newRow);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ASSY_UNHOLD_ERP", "INDATA,INHOLD", null, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 타입으로 CommonCode 조회
        /// Biz : DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE
        /// </summary>
        /// <param name="sFilter"></param>
        /// <returns></returns>
        private DataTable dtTypeCombo(string sFilter, ComboStatus cs)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
                AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                return dtResult;
            }
            catch (Exception ex)
            {
                return null;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
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


    }
}
