/*************************************************************************************
 Created Date : 2017.01.14
      Creator : 유관수K
   Decription : 전지 5MEGA-GMES 구축 - Stacking 공정진척 화면 - 설비작업종료 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  유관수K : Initial Created.
  2021.10.18  김지은    SI     롤프레스 공정 종료 시 무지부/권취방향설정 추가
  2023.08.31  김태우    SI     NFF 슬리팅 공정 종료시 상축/하축 에 따라 무지부/권취방향설정 각기 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_COM_EQPT_END : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _Lotid = string.Empty;
        private string _InputQty = string.Empty;
        private string _ParentQty = string.Empty;
        private string _StartTime = string.Empty;
        private string _CutID = string.Empty;
        private bool _isSideRollDirctnUse = false;

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        #endregion

        #region [Initialize]
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_COM_EQPT_END()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            if (ldpEndDate != null)
                ldpEndDate.SelectedDateTime = (DateTime)System.DateTime.Now;
            if (teTimeEditor != null)
                teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            // 수동입력 공정은 설비완공 투입수량으로 자동 처리
            if (string.Equals(_ProcID, Process.HEAT_TREATMENT))
                EqptEndQty.Value = Convert.ToDouble(_ParentQty);
            else
                EqptEndQty.Value = Convert.ToDouble(_InputQty);

            // 무지부/권취 두 방향 모두 사용하는 AREA에선 롤프레스 완공 시 무지부/권취 방향 저장할 수 있도록 함
            if (string.Equals(_ProcID, Process.ROLL_PRESSING) && _Util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                _isSideRollDirctnUse = true;
                this.Height = 380;
                GridLength gLen1 = new GridLength(20);
                GridLength gLen2 = new GridLength(60);
                dgContent.RowDefinitions[1].Height = gLen1;
                dgContent.RowDefinitions[2].Height = gLen2;
                tbSideWd.Visibility = Visibility.Visible;
                wpMain.Visibility = Visibility.Visible;
                tbSideWd2.Visibility = Visibility.Collapsed;
                wpMain2.Visibility = Visibility.Collapsed;
                SetRadioButton();
            }
            else if (string.Equals(_ProcID, Process.SLITTING) && _Util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))    // NFF E4000, ER 일때 사용 
            {
                _isSideRollDirctnUse = true;
                this.Height = 450;
                GridLength gLen1 = new GridLength(20);
                GridLength gLen2 = new GridLength(100);
                dgContent.RowDefinitions[1].Height = gLen1;
                dgContent.RowDefinitions[2].Height = gLen2;
                tbSideWd.Text = ObjectDic.Instance.GetObjectName("NON_COATED_WINDING_DIRCTN_SET_TOP");
                tbSideWd2.Text = ObjectDic.Instance.GetObjectName("NON_COATED_WINDING_DIRCTN_SET_BACK");
                tbSideWd.Visibility = Visibility.Visible;
                tbSideWd2.Visibility = Visibility.Visible;
                wpMain.Visibility = Visibility.Visible;
                wpMain2.Visibility = Visibility.Visible;
                SetRadioButton();
                SetRadioButton2();
            }
            else
            {
                _isSideRollDirctnUse = false;
                this.Height = 320;
                GridLength gLen = new GridLength(0);
                dgContent.RowDefinitions[1].Height = gLen;
                dgContent.RowDefinitions[2].Height = gLen;
                tbSideWd.Visibility = Visibility.Collapsed;
                wpMain.Visibility = Visibility.Collapsed;
                tbSideWd2.Visibility = Visibility.Collapsed;
                wpMain2.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 7)
            {
                _EqptID = Util.NVC(tmps[0]);
                _ProcID = Util.NVC(tmps[1]);
                _Lotid = Util.NVC(tmps[2]);
                _InputQty = Util.NVC(tmps[3]);
                _StartTime = Util.NVC(tmps[4]);
                _CutID = Util.NVC(tmps[5]);
                _ParentQty = Util.NVC(tmps[6]);
            }
            else
            {
                _EqptID = "";
                _ProcID = "";
                _Lotid = "";
                _InputQty = "0";
                _CutID = "";
                _ParentQty = "0";
            }

            ApplyPermissions();
            InitializeControls();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EqptEndQty.Value <= 0)
            {
                Util.Alert("SFU2936");//작업 수량이 없습니다.
                return;
            }

            // 시작시간이 종료시간보다 빠르면 ERROR
            if (Util.NVC_Decimal(Convert.ToDateTime(_StartTime).ToString("yyyyMMddHHmmss")) >
                Util.NVC_Decimal(Convert.ToDateTime(ldpEndDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + teTimeEditor.ToString()).ToString("yyyyMMddHHmmss")))
            {
                Util.MessageValidation("SFU2954");  //종료시간이 시작시간보다 빠를 수는 없습니다.
                return;
            }

            //저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sTmpDttm = ldpEndDate.SelectedDateTime.ToString("yyyyMMdd");
                    string strContent = string.Empty;

                    string strIssueDate = ldpEndDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + teTimeEditor.ToString();

                    #region Lot Info
                    DataSet inDataSet = new DataSet();

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("PROCID", typeof(string));
                    inDataTable.Columns.Add("END_DTTM", typeof(DateTime));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow inDataRow = null;

                    inDataRow = inDataTable.NewRow();
                    inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    inDataRow["EQPTID"] = _EqptID;
                    inDataRow["PROCID"] = _ProcID;
                    inDataRow["END_DTTM"] = strIssueDate;
                    inDataRow["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(inDataRow);
                    #endregion

                    #region Detail Lot
                    DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                    DataRow inLotDataRow = null;
                    InLotdataTable.Columns.Add("LOTID", typeof(string));
                    InLotdataTable.Columns.Add("EQPT_END_QTY", typeof(Decimal));
                    InLotdataTable.Columns.Add("WIPNOTE", typeof(string));
                    if (_isSideRollDirctnUse)
                    {
                        InLotdataTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                        InLotdataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                        if (wpMain2.Visibility == Visibility.Visible)   // 라디오버튼이 2개 일경우
                        {
                            InLotdataTable.Columns.Add("HALF_SLIT_SIDE2", typeof(string));
                            InLotdataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN2", typeof(string));
                        }
                    }

                    if (!_Lotid.Equals(""))
                    {
                        for (int j = 0; j < _Lotid.Split(',').Length; j++)
                        {
                            if (!_Lotid.Split(',')[j].Trim().Equals(""))
                            {
                                inLotDataRow = InLotdataTable.NewRow();
                                inLotDataRow["LOTID"] = _Lotid.Split(',')[j].Trim();
                                inLotDataRow["EQPT_END_QTY"] = Util.NVC_Decimal(EqptEndQty.Value);
                                inLotDataRow["WIPNOTE"] = null; /* Util.NVC(txtRemark.Text); //작업종료 팝업에서 비고 입력란 제거 */
                                if (_isSideRollDirctnUse)
                                {

                                    if (!ValidationWorkHalfSlittingSide()) return;

                                    if (wpMain2.Visibility == Visibility.Visible)   // 라디오버튼이 2개 일경우
                                    {
                                        string sHSSCode = string.Empty;  //무지부방향 상
                                        string sWDCode = string.Empty;  //권취방향 상
                                        string sHSSCode2 = string.Empty;  //무지부방향 하
                                        string sWDCode2 = string.Empty;  //권취방향 하

                                        foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                                        {
                                            if (rdo.IsChecked == true)
                                            {
                                                sHSSCode = rdo.Tag.ToString().Substring(0, 1);
                                                sWDCode = rdo.Tag.ToString().Substring(1, 1);
                                            }
                                        }
                                        foreach (RadioButton rdo in wpMain2.Children.OfType<RadioButton>())
                                        {
                                            if (rdo.IsChecked == true)
                                            {
                                                sHSSCode2 = rdo.Tag.ToString().Substring(0, 1);
                                                sWDCode2 = rdo.Tag.ToString().Substring(1, 1);
                                            }
                                        }
                                        inLotDataRow["HALF_SLIT_SIDE"] = sHSSCode;
                                        inLotDataRow["EM_SECTION_ROLL_DIRCTN"] = sWDCode;
                                        inLotDataRow["HALF_SLIT_SIDE2"] = sHSSCode2;
                                        inLotDataRow["EM_SECTION_ROLL_DIRCTN2"] = sWDCode2;
                                    }
                                    else
                                    {

                                        string sHSSCode = string.Empty;  //무지부방향
                                        string sWDCode = string.Empty;  //권취방향

                                        foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                                        {
                                            if (rdo.IsChecked == true)
                                            {
                                                sHSSCode = rdo.Tag.ToString().Substring(0, 1);
                                                sWDCode = rdo.Tag.ToString().Substring(1, 1);
                                            }
                                        }
                                        inLotDataRow["HALF_SLIT_SIDE"] = sHSSCode;
                                        inLotDataRow["EM_SECTION_ROLL_DIRCTN"] = sWDCode;
                                    }
                                }
                                inDataSet.Tables["INLOT"].Rows.Add(inLotDataRow);
                            }
                        }
                    }
                    #endregion

                    new ClientProxy().ExecuteService_Multi("BR_ACT_REG_EQPT_END_LOT_UI", "INDATA,INLOT", null, (result2, ex) =>
                    {
                        if (ex != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (string.Equals(_ProcID, Process.SLITTING))
                        {
                            DataSet subDataSet = new DataSet();

                            DataTable subDataTable = subDataSet.Tables.Add("IN_EQP");
                            subDataTable.Columns.Add("SRCTYPE", typeof(string));
                            subDataTable.Columns.Add("IFMODE", typeof(string));
                            subDataTable.Columns.Add("EQPTID", typeof(string));
                            subDataTable.Columns.Add("USERID", typeof(string));
                            subDataTable.Columns.Add("SMPL_REG_YN", typeof(string));

                            DataRow subDataRow = null;

                            subDataRow = subDataTable.NewRow();
                            subDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            subDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                            subDataRow["EQPTID"] = _EqptID;
                            subDataRow["USERID"] = LoginInfo.USERID;
                            subDataRow["SMPL_REG_YN"] = "Y";
                            subDataTable.Rows.Add(subDataRow);

                            DataTable subLotdataTable = subDataSet.Tables.Add("IN_LOT");
                            subLotdataTable.Columns.Add("LOTID", typeof(string));
                            DataRow subLotDataRow = null;

                            subLotDataRow = subLotdataTable.NewRow();
                            subLotDataRow["LOTID"] = _CutID;
                            subDataSet.Tables["IN_LOT"].Rows.Add(subLotDataRow);

                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CHK_LOT_SAMPLING_SL", "IN_EQP,IN_LOT", "OUTDATA", subDataSet);

                            DataTable msg = dsRslt.Tables["OUTDATA"];

                            if (msg.Rows.Count > 0)
                            {
                                if (msg.Rows[0]["MSGTYPE"].Equals("OK"))
                                {
                                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                                }
                                else
                                {
                                    Util.MessageInfo(Util.NVC(msg.Rows[0]["MSGCODE"]));
                                }
                            }
                        }
                        else
                        {
                            Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
                        }

                        btnOk_Click(btnClose, null);

                    }, inDataSet);
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void dpSearch_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (sender == null)
                        return;

                    if (ldpEndDate != null)
                    {
                        if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) < Convert.ToDecimal(ldpEndDate.SelectedDateTime.ToString("yyyyMMdd")))
                        {
                            Util.MessageValidation("SFU1739");      //오늘 이후 날짜는 선택할 수 없습니다.
                            ldpEndDate.SelectedDateTime = DateTime.Now;

                            return;
                        }

                        if (Convert.ToDecimal(Convert.ToDateTime(_StartTime).ToString("yyyyMMdd")) > Convert.ToDecimal(ldpEndDate.SelectedDateTime.ToString("yyyyMMdd")))
                        {
                            Util.MessageValidation("SFU2954");      //종료시간이 시작시간보다 빠를 수는 없습니다.
                            ldpEndDate.SelectedDateTime = DateTime.Now;

                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }));
        }

        //작업종료 팝업에서 종료시간을 미래시간으로 입력 못하게 막아야 함
        private void teTimeEditor_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            try
            {
                if (sender == null)
                    return;

                if (teTimeEditor != null)
                {
                    if (new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) < teTimeEditor.Value)
                    {
                        teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        Util.MessageValidation("SFU1912");      //종료시간을 확인 하세요.
                        return;
                    }

                    if (Convert.ToDecimal(Convert.ToDateTime(_StartTime).ToString("yyyyMMdd")) == Convert.ToDecimal(ldpEndDate.SelectedDateTime.ToString("yyyyMMdd")) &&
                           TimeSpan.Parse(Convert.ToDateTime(_StartTime).ToString("HH:mm:ss")) > teTimeEditor.Value)
                    {
                        teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        Util.MessageValidation("SFU2954");      //종료시간이 시작시간보다 빠를 수는 없습니다.
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }

        private void teTimeEditor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            Util.MessageValidation("SFU2095");      // 키보드로 입력할수 없습니다.
            return;
        }

        #endregion

        #region [Method]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetRadioButton()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "WRK_HALF_SLIT_SIDE";
                dr["ATTRIBUTE1"] = "2";
                dr["ATTRIBUTE3"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow drResult in dtResult.Rows)
                {
                    RadioButton rb = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision" };
                    rb.Content = drResult["CBO_NAME"];
                    rb.Tag = drResult["CBO_CODE"];
                    wpMain.Children.Add(rb);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetRadioButton2()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "WRK_HALF_SLIT_SIDE";
                dr["ATTRIBUTE1"] = "2";
                dr["ATTRIBUTE3"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow drResult in dtResult.Rows)
                {
                    RadioButton rb = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision2" };
                    rb.Content = drResult["CBO_NAME"];
                    rb.Tag = drResult["CBO_CODE"];
                    wpMain2.Children.Add(rb);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private bool ValidationWorkHalfSlittingSide()
        {
            bool bCheck = false;
            bool bCheck_2nd = false;
            foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
            {
                if (rdo.IsChecked == true)
                {
                    bCheck = true;
                }
            }
            if (wpMain2.Visibility == Visibility.Visible)   // 라디오버튼이 2개 일경우
            {
                foreach (RadioButton rdo in wpMain2.Children.OfType<RadioButton>())
                {
                    if (rdo.IsChecked == true)
                    {
                        bCheck_2nd = true;
                    }
                }
                if (!bCheck || !bCheck_2nd)
                {
                    Util.MessageValidation("SFU6023");  // 무지부 방향을 선택하세요.
                    return false;
                }
            }
            if (!bCheck)
            {
                Util.MessageValidation("SFU6023");  // 무지부 방향을 선택하세요.
                return false;
            }
            return true;
        }
        #endregion
    }
}