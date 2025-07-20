/*************************************************************************************
 Created Date : 2021.10.14
      Creator : 김지은 책임
   Decription : Ultium Cells GMES 구축 proj. - 무지부/권취 방향설정
--------------------------------------------------------------------------------------
 [Change History]
  2021.10.14  김지은 책임 : Initial Created.
  2023.06.29  주동석 : NND Unwinder 무지부/권취 방향 저장
  2024.03.04  E20240228-000858  이동주 : 저장시 Description으로 ID를 조회하도록 수정
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _equipmentCode = string.Empty;
        private string _workHalfSlitting = string.Empty;
        private string _currPosition = string.Empty;
        private int _currNumber = 0;
        private string _processCode = string.Empty;
        private DataTable _currMtrlDt = new DataTable();

        BizDataSet _bizRule = new BizDataSet();
        #endregion

        #region [Initialize]
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN()
        {
            InitializeComponent();
            //SetComponent(); ProcessCode 를 사용하기 위해 Loaded 로 이동
        }

        #endregion

        #region [Event]

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                _workHalfSlitting = Util.NVC(parameters[1]);
                if (parameters.Length > 2)
                    _processCode = parameters[2] == null ? string.Empty : Util.NVC(parameters[2]);

                SetComponent();

                if (!string.IsNullOrEmpty(_workHalfSlitting))
                {
                    if (_workHalfSlitting.IndexOf(',') > 0)
                    {
                        foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                        {
                            if (rdo.Tag.ToString().Equals(_workHalfSlitting.Split(',')[0]))
                                rdo.IsChecked = true;
                        }

                        foreach (RadioButton rdo in wpMain1.Children.OfType<RadioButton>())
                        {
                            if (rdo.Tag.ToString().Equals(_workHalfSlitting.Split(',')[1]))
                                rdo.IsChecked = true;
                        }
                    }


                    foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                    {
                        if (rdo.Tag.ToString().Equals(_workHalfSlitting))
                            rdo.IsChecked = true;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWorkHalfSlittingSide()) return;

            try
            {
                string bizRuleName = string.Empty;



                string sHSSCode = string.Empty;  //무지부방향
                string sWDCode = string.Empty;  //권취방향
                string sHSSCode1 = string.Empty;  //무지부방향
                string sWDCode1 = string.Empty;  //권취방향
                string sHSSCode2 = string.Empty;  //무지부방향
                string sWDCode2 = string.Empty;  //권취방향
                string sHSSCode3 = string.Empty;  //무지부방향
                string sWDCode3 = string.Empty;  //권취방향
                string sHSSCode4 = string.Empty;  //무지부방향
                string sWDCode4 = string.Empty;  //권취방향

                string sPstnID = string.Empty;
                string sPstnID1 = string.Empty;
                string sPstnID2 = string.Empty;
                string sPstnID3 = string.Empty;
                string sPstnID4 = string.Empty;

                DataTable inDataTable = new DataTable("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inDataTable.Columns.Add("WRK_HALF_SLIT_SIDE", typeof(string));
                inDataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));

                for (int i = 0; i <= _currNumber; i++)
                {
                    DataRow dr = inDataTable.NewRow();

                    switch (i)
                    {
                        case 0:
                            foreach (RadioButton rdo in wpMain.Children.OfType<RadioButton>())
                            {
                                if (rdo.IsChecked == true)
                                {
                                    sHSSCode = rdo.Tag.ToString().Substring(0, 1);
                                    sWDCode = rdo.Tag.ToString().Substring(1, 1);
                                }
                            }

                            foreach (Label lbl in wpMain.Children.OfType<Label>())
                            {
                                if (!((TextBlock)lbl.Content).Text.Equals(""))
                                    sPstnID = ((TextBlock)lbl.Content).Text;
                            }
                            break;
                        case 1:
                            foreach (RadioButton rdo in wpMain1.Children.OfType<RadioButton>())
                            {
                                if (rdo.IsChecked == true)
                                {
                                    sHSSCode = rdo.Tag.ToString().Substring(0, 1);
                                    sWDCode = rdo.Tag.ToString().Substring(1, 1);
                                }
                            }

                            foreach (Label lbl in wpMain1.Children.OfType<Label>())
                            {
                                if (!((TextBlock)lbl.Content).Text.Equals(""))
                                    sPstnID = ((TextBlock)lbl.Content).Text;
                            }
                            break;
                        case 2:
                            foreach (RadioButton rdo in wpMain2.Children.OfType<RadioButton>())
                            {
                                if (rdo.IsChecked == true)
                                {
                                    sHSSCode = rdo.Tag.ToString().Substring(0, 1);
                                    sWDCode = rdo.Tag.ToString().Substring(1, 1);
                                }
                            }

                            foreach (Label lbl in wpMain2.Children.OfType<Label>())
                            {
                                if (!((TextBlock)lbl.Content).Text.Equals(""))
                                    sPstnID = ((TextBlock)lbl.Content).Text;
                            }
                            break;
                        case 3:
                            foreach (RadioButton rdo in wpMain3.Children.OfType<RadioButton>())
                            {
                                if (rdo.IsChecked == true)
                                {
                                    sHSSCode = rdo.Tag.ToString().Substring(0, 1);
                                    sWDCode = rdo.Tag.ToString().Substring(1, 1);
                                }
                            }

                            foreach (Label lbl in wpMain3.Children.OfType<Label>())
                            {
                                if (!((TextBlock)lbl.Content).Text.Equals(""))
                                    sPstnID = ((TextBlock)lbl.Content).Text;
                            }
                            break;
                        case 4:
                            foreach (RadioButton rdo in wpMain4.Children.OfType<RadioButton>())
                            {
                                if (rdo.IsChecked == true)
                                {
                                    sHSSCode = rdo.Tag.ToString().Substring(0, 1);
                                    sWDCode = rdo.Tag.ToString().Substring(1, 1);
                                }
                            }

                            foreach (Label lbl in wpMain4.Children.OfType<Label>())
                            {
                                if (!((TextBlock)lbl.Content).Text.Equals(""))
                                    sPstnID = ((TextBlock)lbl.Content).Text;
                            }
                            break;
                    }

                    if (_currPosition == "Y")
                    {
                        dr["EQPTID"] = _equipmentCode;
                        dr["EQPT_MOUNT_PSTN_ID"] = _currMtrlDt.Select().Where(c => c.Field<string>("EQPT_MOUNT_PSTN_NAME").Equals(sPstnID)).FirstOrDefault()["EQPT_MOUNT_PSTN_ID"];  // 2024.03.04 DJLEE
                        dr["WRK_HALF_SLIT_SIDE"] = sHSSCode;
                        dr["EM_SECTION_ROLL_DIRCTN"] = sWDCode;
                        dr["USERID"] = LoginInfo.USERID;
                    }
                    else
                    {
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["IFMODE"] = IFMODE.IFMODE_OFF;
                        dr["EQPTID"] = _equipmentCode;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["WRK_HALF_SLIT_SIDE"] = sHSSCode;
                        dr["EM_SECTION_ROLL_DIRCTN"] = sWDCode;
                    }
                    inDataTable.Rows.Add(dr);
                }

                if (_currPosition == "Y")
                {
                    bizRuleName = "BR_PRD_REG_CURR_MOUNT_MTRL_SLIT_SIDE_ROLL_DIR";
                }
                else
                {
                    bizRuleName = "BR_PRD_REG_EIOATTR_SLIT_SIDE_ROLL_DIR";
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1270");      //저장되었습니다.
                    DialogResult = MessageBoxResult.OK;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [Method]
        /// <summary>
        /// 무지부 방향 선택 버튼 생성
        /// </summary>
        private void SetComponent()
        {
            DataTable dtRdo = GetCommonCodeAttr();

            DataTable dtUWND = GetCommonUWND();

            _currPosition = (dtUWND != null && dtUWND.Rows.Count > 0) ? dtUWND.Rows[0]["ATTR2"].ToString() : "N";

            if (_currPosition == "Y")
            {
                SetComponentEXP(dtRdo, dtUWND);
                return;
            }

            foreach (DataRow dr in dtRdo.Rows)
            {
                RadioButton rb = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision" };
                rb.Content = dr["CBO_NAME"];
                rb.Tag = dr["CBO_CODE"];
                wpMain.Children.Add(rb);
            }
        }

        /// <summary>
        /// Unwinder 가 여러개 일때 
        /// </summary>
        /// <param name="dtRdo"></param>
        /// <param name="dtUWND"></param>
        private void SetComponentEXP(DataTable dtRdo, DataTable dtUWND)
        {
            DataTable dtCurr = GetCurrMtrl();

            DataRow drCurr = null;

            for (int i = 0; i < dtCurr.Rows.Count; i++)
            {
                _currNumber = i;

                switch (i)
                {
                    case 0:
                        drCurr = dtCurr.Rows[i];

                        wpMain.Children.Add(SetLabel(drCurr["EQPT_MOUNT_PSTN_NAME"].ToString()));
                        wpMain.Children.Add(SetLabel(""));

                        foreach (DataRow dr in dtRdo.Rows)
                        {
                            RadioButton rb = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision" };
                            rb.Content = dr["CBO_NAME"];
                            rb.Tag = dr["CBO_CODE"];
                            wpMain.Children.Add(rb);
                        }
                        break;
                    case 1:
                        drCurr = dtCurr.Rows[i];

                        wpMain1.Children.Add(SetLabel(drCurr["EQPT_MOUNT_PSTN_NAME"].ToString()));
                        wpMain1.Children.Add(SetLabel(""));

                        foreach (DataRow dr in dtRdo.Rows)
                        {
                            RadioButton rb1 = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision1" };
                            rb1.Content = dr["CBO_NAME"];
                            rb1.Tag = dr["CBO_CODE"];
                            wpMain1.Children.Add(rb1);
                        }
                        break;
                    case 2:
                        drCurr = dtCurr.Rows[i];

                        wpMain2.Children.Add(SetLabel(drCurr["EQPT_MOUNT_PSTN_NAME"].ToString()));
                        wpMain2.Children.Add(SetLabel(""));

                        foreach (DataRow dr in dtRdo.Rows)
                        {
                            RadioButton rb2 = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision2" };
                            rb2.Content = dr["CBO_NAME"];
                            rb2.Tag = dr["CBO_CODE"];
                            wpMain2.Children.Add(rb2);
                        }
                        break;
                    case 3:
                        drCurr = dtCurr.Rows[i];

                        wpMain3.Children.Add(SetLabel(drCurr["EQPT_MOUNT_PSTN_NAME"].ToString()));
                        wpMain3.Children.Add(SetLabel(""));

                        foreach (DataRow dr in dtRdo.Rows)
                        {
                            RadioButton rb3 = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision3" };
                            rb3.Content = dr["CBO_NAME"];
                            rb3.Tag = dr["CBO_CODE"];
                            wpMain3.Children.Add(rb3);
                        }
                        break;
                    case 4:
                        drCurr = dtCurr.Rows[i];

                        wpMain4.Children.Add(SetLabel(drCurr["EQPT_MOUNT_PSTN_NAME"].ToString()));
                        wpMain4.Children.Add(SetLabel(""));

                        foreach (DataRow dr in dtRdo.Rows)
                        {
                            RadioButton rb4 = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision4" };
                            rb4.Content = dr["CBO_NAME"];
                            rb4.Tag = dr["CBO_CODE"];
                            wpMain4.Children.Add(rb4);
                        }
                        break;
                }


                //Dictionary<string, RadioButton> mapName = new Dictionary<string, RadioButton>();

                //for (int j = 0; j < dtRdo.Rows.Count; j++)
                //{
                //    RadioButton rb = new RadioButton { Style = Resources["SearchCondition_RadioButtonStyle"] as Style, GroupName = "rdoDivision" };
                //    rb.Content = dtRdo.Rows[j]["CBO_NAME"];
                //    rb.Tag = dtRdo.Rows[j]["CBO_CODE"];
                //    원하는 변수명으로 해당 변수를 선언한 다음 map에다 추가
                //    mapName.Add(String.Format("strName{0}", j.ToString()), rb);

                //    변수 사용할 땐 이름으로 바로 호출
                //    wpMain.Children.Add(mapName["strName" + j]);
                //}
            }
        }

        private Label SetLabel(string LabelText)
        {
            Color FColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//ForegroundColor
            Color BColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//BackgroundColor
            Color C = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");//하얀색
            Color R = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF92D050");//초록색
            Color W = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF00");//노란색
            Color T = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFC000");//빨간색
            Color U = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD0CECE");//회색
            Color F = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");//검정색
            Color B = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF638EC6");//파란색
            Color X = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#00ff0000");//투명
            Color BorderColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEFF1F3");//바탕색  

            System.Windows.Style style = new System.Windows.Style(typeof(System.Windows.Controls.Control));
            BColor = X; FColor = F;
            style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(FColor)));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(BColor)));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(F)));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));

            Label lbl = null;
            lbl = new Label() { Content = new TextBlock() { Text = LabelText, TextWrapping = TextWrapping.Wrap, FontSize = 14, FontWeight = FontWeights.Bold }, Width = 400, Height = 40 };
            lbl.Style = style;
            lbl.SetValue(Grid.ColumnProperty, 0);
            lbl.SetValue(Grid.RowProperty, 0);
            lbl.HorizontalContentAlignment = HorizontalAlignment.Left;
            lbl.VerticalContentAlignment = VerticalAlignment.Top;
            ((TextBlock)lbl.Content).VerticalAlignment = VerticalAlignment.Top;
            ((TextBlock)lbl.Content).Padding = new Thickness(0, 0, 0, 0);
            ((TextBlock)lbl.Content).Margin = new Thickness(0, 0, 0, 0);

            return lbl;
        }

        /// <summary>
        /// 공통코드에 등록된 무지부방향 항목 불러오기
        /// </summary>
        /// <returns></returns>
        private DataTable GetCommonCodeAttr()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "WRK_HALF_SLIT_SIDE";
                dr["ATTRIBUTE1"] = "2";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        /// <summary>
        /// 공통코드에 등록된 무지부방향 항목 불러오기
        /// </summary>
        /// <returns></returns>
        private DataTable GetCommonUWND()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "MNG_SLITTING_SIDE_AREA";
                dr["COM_CODE"] = _processCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private DataTable GetCurrMtrl()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));
            RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = _equipmentCode;
            dr["MOUNT_MTRL_TYPE_CODE"] = "PROD";
            dr["PRDT_CLSS_CODE"] = "APC";
            RQSTDT.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", RQSTDT);

            if (dt.Rows.Count == 0)
            {
                //Util.AlertInfo("조회 가능한 SLOT 정보가 없습니다.");
                Util.MessageValidation("SFU1899");
                return new DataTable();
            }

            _currMtrlDt = dt.Copy();

            return dt;
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
