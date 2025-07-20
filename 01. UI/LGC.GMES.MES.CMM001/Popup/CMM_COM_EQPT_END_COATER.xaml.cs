/*************************************************************************************
 Created Date : 2019.10.22
      Creator : 정종원	
   Decription : 실적자동화 코터 공정 관련 팝업 UI 추가
--------------------------------------------------------------------------------------
 [Change History]

 2020-12-21   : CNB2동 DRB 관련 용어변경 
                작업종료 -> 장비완료
                단면길이 -> TOP 코팅길이
                양면길이 -> TOP/BACK 코팅길이
 2021.10.18  김지은    SI     코터 공정 종료 시 무지부/권취방향설정 추가
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

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_COM_EQPT_END_COATER : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _EqptID = string.Empty;
        private string _ProcID = string.Empty;
        private string _Lotid = string.Empty;
        private string _InputQty = string.Empty;
        private string _ParentQty = string.Empty;
        private string _StartTime = string.Empty;
        private string _CutID = string.Empty;
        private string _FinalCut = string.Empty;
        private bool _isSideRollDirctnUse = false;

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        #endregion

        #region [Initialize]
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_COM_EQPT_END_COATER()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            if (ldpEndDate != null)
                ldpEndDate.SelectedDateTime = (DateTime)System.DateTime.Now;
            if (teTimeEditor != null)
                teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            // 무지부/권취 두 방향 모두 사용하는 AREA에선 코터 완공 시 무지부/권취 방향 저장할 수 있도록 함
            if (string.Equals(_ProcID, Process.COATING) && _Util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                _isSideRollDirctnUse = true;
                this.Height = 330;
                GridLength gLen1 = new GridLength(20);
                GridLength gLen2 = new GridLength(30);
                dgContent.RowDefinitions[1].Height = gLen1;
                dgContent.RowDefinitions[2].Height = gLen2;
                tbSideWd.Visibility = Visibility.Visible;
                wpMain.Visibility = Visibility.Visible;
                SetRadioButton();
            }
            else
            {
                _isSideRollDirctnUse = false;
                this.Height = 280;
                GridLength gLen = new GridLength(0);
                dgContent.RowDefinitions[1].Height = gLen;
                dgContent.RowDefinitions[2].Height = gLen;
                tbSideWd.Visibility = Visibility.Collapsed;
                wpMain.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 8)
            {
                _EqptID = Util.NVC(tmps[0]);
                _ProcID = Util.NVC(tmps[1]);
                _Lotid = Util.NVC(tmps[2]);
                _InputQty = Util.NVC(tmps[3]);
                _StartTime = Util.NVC(tmps[4]);
                _CutID = Util.NVC(tmps[5]);
                _ParentQty = Util.NVC(tmps[6]);
                _FinalCut = Util.NVC(tmps[7]);
            }
            else
            {
                _EqptID = "";
                _ProcID = "";
                _Lotid = "";
                _InputQty = "0";
                _CutID = "";
                _ParentQty = "0";
                _FinalCut = "N";
            }

            ApplyPermissions();
            InitializeControls();
            SetDefaultValue();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EqptEndQty.Value <= 0)
            {
                Util.Alert("SFU2936");//작업 수량이 없습니다.
                return;
            }

            if (TopCoatingQty.Value < TopBackCoatingQty.Value)
            {
                Util.Alert("SFU5141");//Top Loss가 0보다 작습니다. (Top수량 확인 필요)
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
                    inDataTable.Columns.Add("FINAL_CUT_FLAG", typeof(string));

                    DataRow inDataRow = null;

                    inDataRow = inDataTable.NewRow();
                    inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    inDataRow["EQPTID"] = _EqptID;
                    inDataRow["PROCID"] = _ProcID;
                    inDataRow["END_DTTM"] = strIssueDate;
                    inDataRow["USERID"] = LoginInfo.USERID;
                    inDataRow["FINAL_CUT_FLAG"] = _FinalCut;
                    inDataTable.Rows.Add(inDataRow);
                    #endregion

                    #region Detail Lot
                    DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                    DataRow inLotDataRow = null;
                    InLotdataTable.Columns.Add("LOTID", typeof(string));
                    InLotdataTable.Columns.Add("EQPT_END_QTY", typeof(Decimal));
                    InLotdataTable.Columns.Add("EQPT_TOP_COATING_QTY", typeof(Decimal));
                    InLotdataTable.Columns.Add("EQPT_TOP_BACK_COATING_QTY", typeof(Decimal));
                    InLotdataTable.Columns.Add("EQPT_TOTL_COATING_QTY", typeof(Decimal));
                    InLotdataTable.Columns.Add("WIPNOTE", typeof(string));
                    if (_isSideRollDirctnUse)
                    {
                        InLotdataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
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
                                inLotDataRow["EQPT_TOP_COATING_QTY"] = Util.NVC_Decimal(TopCoatingQty.Value);
                                inLotDataRow["EQPT_TOP_BACK_COATING_QTY"] = Util.NVC_Decimal(TopBackCoatingQty.Value);
                                inLotDataRow["EQPT_TOTL_COATING_QTY"] = Util.NVC_Decimal(EqptEndQty.Value);
                                inLotDataRow["WIPNOTE"] = null; /* Util.NVC(txtRemark.Text); //작업종료 팝업에서 비고 입력란 제거 */
                                if (_isSideRollDirctnUse)
                                {
                                    if (!ValidationWorkHalfSlittingSide()) return;
                                    
                                    string sWDCode = string.Empty;  //권취방향

                                    foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                                    {
                                        if (rdo.IsChecked == true)
                                        {
                                            sWDCode = rdo.Tag.ToString();
                                        }
                                    }
                                    inLotDataRow["EM_SECTION_ROLL_DIRCTN"] = sWDCode;
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

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.
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
        #endregion

        #region [Method]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        // 설비 총 길이, 단면길이, 양면길이 존재하면 자동 SETUP
        private void SetDefaultValue()
        {
            try
            {
                string sLotID = string.Empty;
                if (!string.IsNullOrEmpty(_Lotid) && string.Equals(_Lotid.Substring(_Lotid.Length - 1, 1), ","))
                    sLotID = _Lotid.Substring(0, _Lotid.Length - 1);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_PRDT_CONV_RATE_FOR_LOT_V01", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    EqptEndQty.Value = Convert.ToDouble(result.Rows[0]["EQPT_INPUT_M_TOTL_QTY"]);
                    TopCoatingQty.Value = Convert.ToDouble(result.Rows[0]["EQPT_INPUT_M_TOP_QTY"]);
                    TopBackCoatingQty.Value = Convert.ToDouble(result.Rows[0]["EQPT_INPUT_M_TOP_BACK_QTY"]);
                }
            }
            catch (Exception ex) { }
        }

        private void SetRadioButton()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "EM_SECTION_ROLL_DIRCTN";
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

        private bool ValidationWorkHalfSlittingSide()
        {
            bool bCheck = false;
            foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
            {
                if (rdo.IsChecked == true)
                {
                    bCheck = true;
                }
            }

            if (!bCheck)
            {
                Util.MessageValidation("SFU6030");  // 무지부 방향을 선택하세요.
                return false;
            }
            return true;
        }
        #endregion
    }
}