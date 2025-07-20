/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : PACK 수동공정진척화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.06.11  손우석 검사 결과값 입력시 문자입력 가능하도록 처리 - 폴란드
  2018.07.04  염규범 이전검사 가져오기 후 결과 저장시 기준정보 문자 비교 추가 - 폴란드
  2019.04.11  손우석 CSR ID 3972370 [G.MES] LGCWA 수동 공정 진척 - 이전 검사값 입력 삭제 요청의 건 [요청번호] C20190411_72370
  2019.09.09  염규범 상한/하한값 소수점 표기로 OK/NG 처리 정살적으로 안되는 내용 수정의 건 ( UI 오류 수정의 건 )
  2019.12.16  손우석 CSR ID 13069 수동공정진척 화면 개선 요청의 건 [요청번호] C20191216-000066
  2019.12.24  손우석 CSR ID 13069 수동공정진척 화면 개선 요청의 건 [요청번호] C20191216-000066 -> 전사 확산
  2020.01.16  손우석 CSR ID 13069 수동공정진척 화면 개선 요청의 건 [요청번호] C20191216-000066 -> 전사 확산 오류 수정
  2020.01.22  손우석 CSR ID 13069 수동공정진척 화면 개선 요청의 건 [요청번호] C20191216-000066 -> 품질 이슈로 인한 인터락 기능으로 해제금지
  2020.02.13  손우석 검사 데이터 입력시 문자, 숫자 비교 구분 개선
  2020.04.22  염규범 서비스 번호 54620 GMES 수동공정진척, 수동공전진적(New) 에서 검사 값 부족할 경우 NG 처리 불가능 [요청번호] C20200422-000144
  2020.04.23  손우석 1. 이전값 적용시, 문자열에 대해서, 양불 판정이 정확히 이루어지지 않는 내용
                     2. 스크롤 이동시에, 측정명에 알수 없는 색이 칠해지는 현상
                     3. 복사 + 붙여넣기 를 이용해서 측정값 입력 가능
                     4. 불량값 입력시 확정 버튼 눌렀을 경우, ‘불량 값이 존재합니다’ 라는 팝업이 뜨지 않고 정상 처리되는 현상
  2020.04.29  손우석 이전값 적용시, 양불 판정이 수정
  2020.04.29  염규범 위에 에러 내용 수정의건 
  2020.04.30  손우석 오창 자동차 1동 파일럿 라인의 경우 데이터 수기 입력해야되므로 그리드에 입력 가능하도록 처리 inputProduct
  2023.07.21  이재호 E20230607-000445 파일럿5 요청 오창 2공장(PE) 수동공정진척 시 수집데이터 입력 가능하도록 임시조치
  2023.11.30  정용석 E20231017-000561 수동공정진척 UI 판정/공정기준 GMES판정/수집항목기준 GMES 판정기준 상이부분 통일화 (기존 숫자만 판정되던거 숫자/문자 모두 판정하도록 변경)
  2024.05.21  권성혁 E20240411-001041 [ESWA Form. Module Production PI] Add Warning Pop-Up for Defect Registration on Finished Modules in CMA/BMA Completion Process
  2024.06.10  강송모 E20240401-000283 불량유형선택방법 변경
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
//2018.06.11
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// cnsjinsunlee04.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_052 : UserControl, IWorkArea
    {
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        string now_labelcode = string.Empty;

        public PACK001_052()
        {
            InitializeComponent();
        }

        private PACK001_001_PROCESSINFO window_PROCESSINFO = new PACK001_001_PROCESSINFO();
        private DataSet dsResult = new DataSet();
        //불량유형 code 선택을 위한 전역 변수 선언
        private DataTable defectResult = new DataTable();
        private bool bMtrlInputProcFlag = false; //자재결합 공정 여부 true:결합공정 false:일반공정
        //2019.12.16
        private string strAu = string.Empty;
        #endregion

        #region Initialize

        #endregion

        #region Event

        #region Event - UserControl
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnConfirm);
                listAuth.Add(btnKeyPartDelete);
                listAuth.Add(btnProdutLotLabel);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                //투입Grid에있는 양품불량 수량 숨김.
                gridPlanQTY.Visibility = Visibility.Collapsed;

                //공정정보 load
                setProcessInfo();

                InitCombo();

                // NJ 라인 특화 UI
                Area_ID_CHK();

                //처음로드시 팝업오픈
                if (!(window_PROCESSINFO.PROCID.Length > 0))
                {
                    btnProcessSelect_Click(null, null);
                }

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
                tbWipListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.F4:
                        txtProdutLotInput.Focus();
                        txtProdutLotInput.SelectAll();
                        break;
                    case Key.F5:
                        txtKeyPartInput.Focus();
                        txtKeyPartInput.SelectAll();
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event - TextBox

        private void txtProdutLotInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        Util.MessageInfo("FM_ME_0183");
                        return;
                    }
                    if (txtProdutLotInput.Text.Length > 0)
                    {
                        if (chkProcessSelect())
                        {
                            return;
                        }
                        Util.gridClear(dgKeyPart);
                        if (lineCHK())
                        {
                            Util.MessageConfirm("SFU5953", result =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    inputProduct(txtProdutLotInput.Text, null);
                                }
                            });
                        }
                        else
                        {
                            inputProduct(txtProdutLotInput.Text, null);
                        }
                    }
                    txtProdutLotInput.Focus();
                    txtProdutLotInput.SelectAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtKeyPartInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtKeyPartInput.Text.Length > 0)
                    {
                        if (chkProcessSelect())
                        {
                            return;
                        }

                        if (chkInputKeypart(txtKeyPartInput.Text))
                        {
                            inputKeyPartProduct();
                            txtKeyPartInput.Focus();
                            txtKeyPartInput.SelectAll();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtKeyPartInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) || e.Key != Key.V)
            {
                return;
            }

            try
            {
                List<string> lstClipboardData = Clipboard.GetText().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                var lstMaterialLOT = lstClipboardData.Where(x => !string.IsNullOrEmpty(x) || (x != "JOBEND"));       // 빈값으로 들어온것 제거 & JOBEND라고 입력된거 제거

                // Validation Check...
                if (lstMaterialLOT.Count() > 100)
                {
                    Util.MessageValidation("SFU8102");   // 최대 100개 까지 가능합니다.
                    return;
                }

                // Input KeyPart TODO!!!
                //this.GetLOTListForWipDelete(lstLOTIDList.ToList<string>());
                foreach (var materialLOT in lstMaterialLOT)
                {
                    if (!this.chkInputKeypart(materialLOT))
                    {
                        break;
                    }
                    // 자재 결합 & 착공
                    // 1개씩 결합하는 도중 Exception이 난 경우 이전까지의 정상결합된 것은 그대로 두고 이후는 무시해버림.
                    if (!inputKeyPartProduct(materialLOT))
                    {
                        break;
                    }
                }

                this.txtKeyPartInput.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event - ComboBox
        private void cboResult_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                //if (Util.NVC(cboResult.SelectedValue) == "OK")
                if (cboResult.SelectedIndex == 0)
                {
                    Util.gridClear(dgDefect);
                    Util.gridClear(dgDefect0);
                    Util.gridClear(dgDefect1);
                    Util.gridClear(dgDefect2);
                    Util.gridClear(dgDefect3);
                    Util.gridClear(dgDefect4);
                }
                else
                {
                    if (dsResult != null)
                    {
                        if (dsResult.Tables.IndexOf("ACTIVITYREASON") > -1)
                        {
                            ////dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["ACTIVITYREASON"]);
                            //Util.GridSetData(dgDefect, dsResult.Tables["ACTIVITYREASON"], FrameOperation, true);

                            setDefectResult();

                            Util.gridClear(dgDefect0);
                            Util.gridClear(dgDefect1);
                            Util.gridClear(dgDefect2);
                            Util.gridClear(dgDefect3);
                            Util.gridClear(dgDefect4);

                            if (defectResult != null && defectResult.Rows.Count > 0)
                            {
                                Util.GridSetData(dgDefect0, getNextRESNCODE(new string[0]), FrameOperation, true);
                            }
                            else
                            {
                                ms.AlertInfo("SFU5062", window_PROCESSINFO.PROCNAME); //불량코드가 존재하지 않습니다.
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void cboListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnProdutLotSearch_Click(null, null);
        }
        #endregion

        #region Event - DataGrid
        private void dgKeyPart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgKeyPart.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    DataTableConverter.SetValue(dgKeyPart.Rows[cell.Row.Index].DataItem, "CHK", true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgKeyPartChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rbt = sender as RadioButton;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "INPUT_LOTID"));
                string sMTRLID = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dg.SelectedIndex].DataItem, "MTRLID"));

                //if(chkInputKeypart)
                DataTable dtMTRL = window_PROCESSINFO.WO_MTRL_INFO;

                DataRow[] drTargetRow = dtMTRL.Select("MTRLID = '" + sMTRLID + "'" + " OR TMP_MTRLID = '" + sMTRLID + "'"); //자재,대체자재 확인
                if (drTargetRow.Length > 0)
                {
                    //keyPart 입력
                    txtKeyPartInput.Text = sSelectLotid;
                }
                else
                {
                    ms.AlertWarning("SFU1297", window_PROCESSINFO.PROCNAME); //{0}에 투입하는 자재가 아닙니다.
                    DataTableConverter.SetValue(dg.Rows[dg.SelectedIndex].DataItem, "CHK", "False");
                }

                //checked Mode 가 'TwoWay' 인경우 화면에 보이지 않는 부분도 체크가 남아있는 문제가 발생하여
                //한번더 체크 해제.
                for (int i = 0; i < dgKeyPart.Rows.Count; i++)
                {
                    if (i != dg.SelectedIndex)
                    {
                        DataTableConverter.SetValue(dgKeyPart.Rows[i].DataItem, "CHK", false);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgWipList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }


        private void dgWipList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            try
            {

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "LOTID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void wipListInputButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgInspection.CanUserSort = false;

                Button bt = sender as Button;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;
                string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[dg.SelectedIndex].DataItem, "LOTID"));

                //text에 입력
                txtProdutLotInput.Text = sSelectLotid;
                //keyPart 입력 추기화
                txtKeyPartInput.Text = "";

                txtConfirmNote.Clear();
                Util.gridClear(dgDefect);
                Util.gridClear(dgDefect0);
                Util.gridClear(dgDefect1);
                Util.gridClear(dgDefect2);
                Util.gridClear(dgDefect3);
                Util.gridClear(dgDefect4);
                Util.gridClear(dgKeyPart);

                if (lineCHK())
                {
                    Util.MessageConfirm("SFU5953", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            inputProduct(sSelectLotid, null);
                        }
                    });
                }
                else
                {
                    inputProduct(sSelectLotid, null);
                }

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        // 제외 라인 확인.
        private bool lineCHK()
        {
            string now_Line = window_PROCESSINFO.EQSGID;
            string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
            dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
            dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
            dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
            dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
            dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["CMCDTYPE"] = "PACK_APPLY_LINE_BY_UI";
            drRQSTDT["CBO_CODE"] = "PACK001_052";
            drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
            drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
            drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
            drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
            drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
            dtRQSTDT.Rows.Add(drRQSTDT);

            dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

            string exclusion_Line_Attr1 = Util.NVC(dtRSLTDT.Rows[0]["ATTRIBUTE1"]);
            string exclusion_Line_Attr2 = Util.NVC(dtRSLTDT.Rows[0]["ATTRIBUTE2"]);
            string exclusion_Line_Attr3 = Util.NVC(dtRSLTDT.Rows[0]["ATTRIBUTE3"]);
            string exclusion_Line_Attr4 = Util.NVC(dtRSLTDT.Rows[0]["ATTRIBUTE4"]);
            string exclusion_Line_Attr5 = Util.NVC(dtRSLTDT.Rows[0]["ATTRIBUTE5"]);

            if (exclusion_Line_Attr1.Contains(now_Line) || exclusion_Line_Attr2.Contains(now_Line) || exclusion_Line_Attr3.Contains(now_Line) ||
                exclusion_Line_Attr4.Contains(now_Line) || exclusion_Line_Attr5.Contains(now_Line))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void dgDefect0_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgDefect0.SelectedItem as DataRowView;

            string[] prefix = new string[1];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];

            txtConfirmNote.Clear();
            Util.gridClear(dgDefect);
            Util.gridClear(dgDefect1);
            Util.gridClear(dgDefect2);
            Util.gridClear(dgDefect3);
            Util.gridClear(dgDefect4);

            Util.GridSetData(dgDefect1, getNextRESNCODE(prefix), FrameOperation, true);
            dgDefect1.AllColumnsWidthAuto();
        }

        private void dgDefect1_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgDefect0.SelectedItem as DataRowView;
            DataRowView r1 = dgDefect1.SelectedItem as DataRowView;

            string[] prefix = new string[2];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];

            txtConfirmNote.Clear();
            Util.gridClear(dgDefect);
            Util.gridClear(dgDefect2);
            Util.gridClear(dgDefect3);
            Util.gridClear(dgDefect4);

            Util.GridSetData(dgDefect2, getNextRESNCODE(prefix), FrameOperation, true);
            dgDefect2.AllColumnsWidthAuto();
        }

        private void dgDefect2_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgDefect0.SelectedItem as DataRowView;
            DataRowView r1 = dgDefect1.SelectedItem as DataRowView;
            DataRowView r2 = dgDefect2.SelectedItem as DataRowView;

            string[] prefix = new string[3];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[2] = r2.Row.ItemArray[0].ToString().Split(':')[0];

            txtConfirmNote.Clear();
            Util.gridClear(dgDefect);
            Util.gridClear(dgDefect3);
            Util.gridClear(dgDefect4);

            Util.GridSetData(dgDefect3, getNextRESNCODE(prefix), FrameOperation, true);
            dgDefect3.AllColumnsWidthAuto();
        }

        private void dgDefect3_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgDefect0.SelectedItem as DataRowView;
            DataRowView r1 = dgDefect1.SelectedItem as DataRowView;
            DataRowView r2 = dgDefect2.SelectedItem as DataRowView;
            DataRowView r3 = dgDefect3.SelectedItem as DataRowView;

            string[] prefix = new string[4];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[2] = r2.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[3] = r3.Row.ItemArray[0].ToString().Split(':')[0];

            txtConfirmNote.Clear();
            Util.gridClear(dgDefect);
            Util.gridClear(dgDefect4);

            Util.GridSetData(dgDefect4, getNextRESNCODE(prefix), FrameOperation, true);
            dgDefect4.AllColumnsWidthAuto();
        }

        private void dgDefect4_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgDefect0.SelectedItem as DataRowView;
            DataRowView r1 = dgDefect1.SelectedItem as DataRowView;
            DataRowView r2 = dgDefect2.SelectedItem as DataRowView;
            DataRowView r3 = dgDefect3.SelectedItem as DataRowView;
            DataRowView r4 = dgDefect4.SelectedItem as DataRowView;

            String code = r0.Row.ItemArray[0].ToString().Split(':')[0] + r1.Row.ItemArray[0].ToString().Split(':')[0] + r2.Row.ItemArray[0].ToString().Split(':')[0] + r3.Row.ItemArray[0].ToString().Split(':')[0] + r4.Row.ItemArray[0].ToString().Split(':')[0];

            DataView dv = dsResult.Tables["ACTIVITYREASON"].Copy().DefaultView;
            dv.RowFilter = "RESNCODE = '" + code + "'";
            Util.GridSetData(dgDefect, dv.ToTable(false), FrameOperation, true);
            DataTableConverter.SetValue(dgDefect.Rows[0].DataItem, "CHK", true);
        }

        private void dgDefect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgDefect.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    DataTableConverter.SetValue(dgDefect.Rows[cell.Row.Index].DataItem, "CHK", true);
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgDefect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Point pnt = e.GetPosition(null);
                //C1.WPF.DataGrid.DataGridCell cell = dgDefect.GetCellFromPoint(pnt);
                //if (cell != null)
                //{
                //    string sRESNDESC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNDESC"));

                //    txtConfirmNote.Text = sRESNDESC;
                //}
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgDefectChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //checked Mode 가 'TwoWay' 인경우 화면에 보이지 않는 부분의 체크가 남아있는 문제가 발생하여
                //한번더 체크 해제.
                RadioButton rbt = sender as RadioButton;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).DataGrid;
                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rbt.Parent).Cell.Row.Index;

                for (int i = 0; i < dgDefect.Rows.Count; i++)
                {
                    if (i != dg.SelectedIndex)
                    {
                        DataTableConverter.SetValue(dgDefect.Rows[i].DataItem, "CHK", false);
                    }
                    else
                    {
                        string sRESNDESC = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNNAME"));

                        txtConfirmNote.Text = sRESNDESC;
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgKeyPart_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "PRDT_ATTCH_PSTN_NO" || e.Cell.Column.Name == "INPUT_LOTID")
                    {
                        e.Cell.Presenter.FontSize = 16;
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontSize = 12;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void menu_KeyPartUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgKeyPart.CurrentCell != null)
                {
                    string sSEQ = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dgKeyPart.CurrentCell.Row.Index].DataItem, "PRDT_ATTCH_PSTN_NO"));
                    string sLOTID = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dgKeyPart.CurrentCell.Row.Index].DataItem, "LOTID"));
                    string sKEYPARTLOT_BEFORE = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[dgKeyPart.CurrentCell.Row.Index].DataItem, "INPUT_LOTID"));

                    PACK001_001_KEYPART_CHANGE popup = new PACK001_001_KEYPART_CHANGE();
                    popup.FrameOperation = this.FrameOperation;

                    if (popup != null)
                    {
                        DataTable dtData = new DataTable();
                        dtData.Columns.Add("LOTID", typeof(string));
                        dtData.Columns.Add("SEQ", typeof(string));
                        dtData.Columns.Add("KEYPARTLOT_BEFORE", typeof(string));
                        dtData.Columns.Add("PROCID", typeof(string));

                        DataRow newRow = null;

                        newRow = dtData.NewRow();
                        newRow["LOTID"] = sLOTID;
                        newRow["SEQ"] = sSEQ;
                        newRow["KEYPARTLOT_BEFORE"] = sKEYPARTLOT_BEFORE;
                        newRow["PROCID"] = window_PROCESSINFO.PROCID;
                        dtData.Rows.Add(newRow);

                        //========================================================================
                        object[] Parameters = new object[1];
                        Parameters[0] = dtData;
                        C1WindowExtension.SetParameters(popup, Parameters);
                        //========================================================================
                        popup.Closed -= popup_KeyPartChange_Closed;
                        popup.Closed += popup_KeyPartChange_Closed;
                        popup.ShowModal();
                        popup.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void popup_KeyPartChange_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_001_KEYPART_CHANGE popup = sender as PACK001_001_KEYPART_CHANGE;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    getKeypart(popup.Lotid);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgKeyPart_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgKeyPart.GetCellFromPoint(pnt);
            if (cell != null)
            {
                dgKeyPart.CurrentCell = cell;
            }
        }

        private void dgWipList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODID"));

                    if (cboLabelCode.IsEnabled)
                    {
                        setLabelCode(sPRODID);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Event - Button
        private void btnProcessSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string MAINFORMPATH = "LGC.GMES.MES.PACK001";
                string MAINFORMNAME = "PACK001_001_SELECTPROCESS";

                Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                object obj = Activator.CreateInstance(targetType);

                IWorkArea workArea = obj as IWorkArea;
                workArea.FrameOperation = FrameOperation;

                C1Window popup = obj as C1Window;
                if (popup != null)
                {
                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_001_SELECTPROCESS popup = sender as PACK001_001_SELECTPROCESS;
                if (popup.DialogResult == MessageBoxResult.OK)
                {

                    DataRow drSelectedProcess = popup.DrSelectedProcessInfo;

                    if (drSelectedProcess != null)
                    {
                        //1.공정선택 완료후 선택라인공정 PACK001_001_SELECTPROCESS 에 넘겨줌 //PACK001_001_SELECTPROCESS에선 선택라인공정의 정보조회표시
                        window_PROCESSINFO.setProcess(drSelectedProcess);

                        //2.선택공정에따른 화면초기화 
                        tbTitle.Text = popup.SSelectedProcessTitle;
                        //btnProcessSelect.Content= popup.SSelectedProcessTitle;

                        //System.Windows.Controls.ContentControl thisParent = this.Parent as System.Windows.Controls.ContentControl;                        
                        //LGC.GMES.MES.MainFrame.MainTabItemLayout thisParentTemplatedParent = thisParent.TemplatedParent as LGC.GMES.MES.MainFrame.MainTabItemLayout;

                        //thisParentTemplatedParent.TitleDepth3 = popup.SSelectedProcessTitle;

                        EnableChange_LabelPrintButton();

                        EnableChange_keypartInputPlace();


                        Refresh();
                        getWipList();
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnProdutLototInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkProcessSelect())
                {
                    return;
                }

                if (txtProdutLotInput.Text.Length > 9)
                {
                    Util.gridClear(dgKeyPart);
                    inputProduct(txtProdutLotInput.Text, null);
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }


        private void btnProdutLotLabelel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkProcessSelect())
                {
                    return;
                }
                if (dgWipList.CurrentRow != null)
                {
                    string sSelectLotid = Util.NVC(DataTableConverter.GetValue(dgWipList.CurrentRow.DataItem, "LOTID"));
                    string sProdid = Util.NVC(DataTableConverter.GetValue(dgWipList.CurrentRow.DataItem, "PRODID"));
                    string sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                    now_labelcode = sLabelCode;

                    if (sLabelCode.Length > 0)
                    {
                        Util.printLabel_Pack(FrameOperation, loadingIndicator, sSelectLotid, LABEL_TYPE_CODE.PACK, sLabelCode, "N", "1", sProdid);
                    }
                    else
                    {
                        Util.printLabel_Pack(FrameOperation, loadingIndicator, sSelectLotid, LABEL_TYPE_CODE.PACK, "N", "1", null);
                    }

                }
                else
                {
                    ms.AlertWarning("SFU1559"); //발행할 LOT을 선택하십시오.
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnProdutLotSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkProcessSelect())
                {
                    return;
                }
                getWipList();
                if (LotCountView.IsVisible)
                {
                    Wip_Input_MTRL_DataSet();
                }

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnKeyPartInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkProcessSelect())
                {
                    return;
                }
                inputKeyPartProduct();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnKeyPartDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                {
                    Util.MessageInfo("FM_ME_0183");
                    return;
                }

                if (chkProcessSelect())
                {
                    return;
                }
                delKeyPart();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkProcessSelect())
                {
                    return;
                }

                if (ConfirmValidation())
                {

                    //양품일경우 VALIDATION
                    if (Util.NVC(cboResult.SelectedValue) == "OK")
                    {
                        #region 판정 OK 선택
                        if (GetDefectCount()) //검사데이터 불량 여부
                        {
                            #region 판정 OK, 검사 데이터 불량
                            #region 2019.12.16 주석
                            //2019.12.16
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1807"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                            ////입력데이터중 불량 판정된 항목이 있습니다. 무시하고 양품 판정 하시겠습니까?
                            //{
                            //    if (sResult == MessageBoxResult.OK)
                            //    {
                            //        //2019.12.16
                            //        //양불판정을 공백으로처리
                            //        //setInspectionOkNg_Manual();

                            //        ConfirmOKNG();
                            //    }
                            //});
                            #endregion 2019.12.16 주석

                            #region 개발자 체크
                            //2020.04.23
                            ////개발자
                            //if (strAu == "DEV")
                            //{
                            #region 2019.12.24 주석
                            //2019.12.24
                            //if (LoginInfo.CFG_SHOP_ID == "A040")
                            //{
                            #endregion 2019.12.24 주석

                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1807"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                            ////입력데이터중 불량 판정된 항목이 있습니다. 무시하고 양품 판정 하시겠습니까?
                            //{
                            //    if (sResult == MessageBoxResult.OK)
                            //    {
                            //        //양불판정을 공백으로처리
                            //        //setInspectionOkNg_Manual();
                            //       ConfirmOKNG();
                            //   }
                            //});

                            #region 2019.12.24 주석
                            //}
                            //else
                            //{
                            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1807"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                            //    //입력데이터중 불량 판정된 항목이 있습니다. 무시하고 양품 판정 하시겠습니까?
                            //    {
                            //        if (sResult == MessageBoxResult.OK)
                            //        {
                            //            //양불판정을 공백으로처리
                            //            setInspectionOkNg_Manual();

                            //            ConfirmOKNG();
                            //        }
                            //    });
                            //}
                            #endregion 2019.12.24 주석
                            //}
                            #endregion 개발자 체크

                            #region 2019.12.24 주석
                            //2019.12.24
                            //else
                            //{
                            //    //오창이 아닌 경우 메시지
                            //    if (LoginInfo.CFG_SHOP_ID != "A040")
                            //    {
                            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1807"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                            //        //입력데이터중 불량 판정된 항목이 있습니다. 무시하고 양품 판정 하시겠습니까?
                            //        {
                            //            if (sResult == MessageBoxResult.OK)
                            //            {
                            //                //양불판정을 공백으로처리
                            //                setInspectionOkNg_Manual();

                            //                ConfirmOKNG();
                            //            }
                            //        });
                            //    }
                            //}
                            #endregion  2019.12.24 주석

                            //2020.01.21
                            //오창 자동차1동 메시지 표시
                            if ((LoginInfo.CFG_SHOP_ID == "A040") ||
                                (LoginInfo.CFG_AREA_ID == "P1") ||
                                ((LoginInfo.CFG_SHOP_ID == "F040") && (LoginInfo.CFG_AREA_ID == "PE"))
                               )
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1807"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                                //입력데이터중 불량 판정된 항목이 있습니다. 무시하고 양품 판정 하시겠습니까?
                                {
                                    if (sResult == MessageBoxResult.OK)
                                    {
                                        //양불판정을 공백으로처리
                                        //setInspectionOkNg_Manual();

                                        ConfirmOKNG();
                                    }
                                });
                            }
                            else
                            {
                                //개발자가 아니고 오창 자동차 1동이 아닌 경우
                                //불량 검사 값이 존재합니다.
                                Util.MessageInfo("SFU8162");
                            }
                            #endregion 판정 OK, 검사 데이터 불량
                        }
                        else
                        {
                            #region 판정 OK, 검사 데이터 양품
                            ConfirmOKNG();
                            #endregion 판정 OK, 검사 데이터 양품
                        }
                        #endregion 판정 OK 선택
                    }
                    else
                    {
                        #region 판정 NG 선택
                        chkLotProcid();
                        #endregion 판정 NG 선택
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnInspectionBefore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkProcessSelect())
                {
                    return;
                }
                //dgInspection.LoadedCellPresenter += new EventHandler<DataGridCellEventArgs>(grid_LoadedCellPresenter);
                setInspectionBefore();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Refresh();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnExpandFrameLeft_Unchecked(object sender, RoutedEventArgs e)
        {
            ContentLeft.RowDefinitions[0].Height = GridLength.Auto;
            ContentLeft.RowDefinitions[1].Height = new GridLength(8);

            gridPlanQTY.Visibility = Visibility.Collapsed;

            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();


            //gla.From = new GridLength(0);
            //gla.To = new GridLength(1);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            //ContentLeft.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);

        }

        private void btnExpandFrameLeft_Checked(object sender, RoutedEventArgs e)
        {
            ContentLeft.RowDefinitions[0].Height = new GridLength(0);
            ContentLeft.RowDefinitions[1].Height = new GridLength(0);

            gridPlanQTY.Visibility = Visibility.Visible;

            //LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            //gla.From = new GridLength(1, GridLength.Auto);
            //gla.To = new GridLength(0, GridUnitType.Star);
            //gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            //ContentLeft.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            ContentRight.RowDefinitions[1].Height = new GridLength(0);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(1, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star); ;
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            ContentRight.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            ContentRight.RowDefinitions[1].Height = new GridLength(8);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0);
            gla.To = new GridLength(1);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            ContentRight.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrameTop_Checked(object sender, RoutedEventArgs e)
        {
            ContentRight.RowDefinitions[1].Height = new GridLength(8);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0);
            gla.To = new GridLength(1);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            ContentRight.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrameTop_Unchecked(object sender, RoutedEventArgs e)
        {
            ContentRight.RowDefinitions[1].Height = new GridLength(0);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(1, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star); ;
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            ContentRight.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        #endregion Event - Button

        #endregion

        #region Method

        #region Method - 화면

        private void Area_ID_CHK()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE";
                DataTable dtRQSTDT = new DataTable();

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["COM_TYPE_CODE"] = "PACK_UI_DISPLAY";
                drRQSTDT["COM_CODE"] = "EQSG_ID";
                dtRQSTDT.Rows.Add(drRQSTDT);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtRQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["ATTR1"]).Contains(LoginInfo.CFG_EQSG_ID))
                    {
                        Wip_Input_MTRL_DataSet();
                    }
                }
                else
                {
                    this.LotCountView.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Wip_Input_MTRL_DataSet()
        {
            try
            {
                int REQUIRED_QTY = 0;
                int GAP = 0;

                string bizRuleName = "DA_PRD_SEL_WIP_INPUT_LOT_QTY_CNJ";
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count == 0)
                {
                    return;
                }

                txtLOT_ID_QTY.Text = Util.NVC(dtResult.Rows[0]["LOT_QTY"]);
                txtKEYPART_ID.Text = Util.NVC(dtResult.Rows[0]["KEYPART_ID"]);
                txtBOM_QTY.Text = Util.NVC(dtResult.Rows[0]["BOM_QTY"]);
                txtASSEMBLED_QTY.Text = Util.NVC(dtResult.Rows[0]["ASSEMBLED_QTY"]);

                REQUIRED_QTY = Convert.ToInt32(txtLOT_ID_QTY.Text) * Convert.ToInt32(txtBOM_QTY.Text);
                txtREQUIRED_QTY.Text = REQUIRED_QTY.ToString();
                GAP = REQUIRED_QTY - Convert.ToInt32(txtASSEMBLED_QTY.Text);
                txtGAP.Text = GAP.ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            try
            {
                dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = DateTime.Now;

                this.cboListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cboListCount, "CBO_NAME", "CBO_CODE", 100, 1000, 100);
                this.cboListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);

                CommonCombo _combo = new CommonCombo();

                String[] sFilter3 = { "JUDGE_OK" };
                _combo.SetCombo(cboResult, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Refresh()
        {
            try
            {
                //재공조회
                getWipList();

                //그리드 clear
                Util.gridClear(dgDefect);
                Util.gridClear(dgDefect0);
                Util.gridClear(dgDefect1);
                Util.gridClear(dgDefect2);
                Util.gridClear(dgDefect3);
                Util.gridClear(dgDefect4);
                Util.gridClear(dgInspection);
                Util.gridClear(dgKeyPart);

                txtTargetLot.Text = string.Empty;
                txtTargeProduct.Text = string.Empty;
                txtProdutLotInput.Text = string.Empty;
                txtKeyPartInput.Text = string.Empty;
                txtConfirmNote.Text = string.Empty;

                //실적 수량 재조회
                DataTable dtPlanQty = window_PROCESSINFO.setPlanQty();
                if (dtPlanQty != null)
                {
                    if (dtPlanQty.Rows.Count > 0)
                    {
                        txtGoodQty.Text = Util.NVC(dtPlanQty.Rows[0]["GOODQTY"]);
                        txtDefectQty.Text = Util.NVC(dtPlanQty.Rows[0]["DEFECTQTY"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void EnableChange_LabelPrintButton()
        {
            try
            {
                string sPcsgID = window_PROCESSINFO.PCSGID;
                if (sPcsgID != null)
                {
                    //공정군이 Cell인경우 재발행버튼 Enable = False
                    //오더에 물린 제품코드와 라우트 공정군으로 조회하기때문에 Cell공정군 정보없음
                    //->Cell공정군은 오더선택하지않기때문에..
                    if (sPcsgID != "C" && sPcsgID != "")
                    {
                        btnProdutLotLabel.IsEnabled = true;
                        cboLabelCode.IsEnabled = true;
                        //버튼 활성화후 라벨코드 셋팅
                        setLabelCode(window_PROCESSINFO.PRODUCTID);
                    }
                    else
                    {
                        btnProdutLotLabel.IsEnabled = false;
                        cboLabelCode.IsEnabled = false;
                    }
                }
                else
                {
                    btnProdutLotLabel.IsEnabled = false;
                    cboLabelCode.IsEnabled = false;
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// keypart결합 관련 화면 Enable 변경
        /// 결합공졍인경우 Enable true
        /// 일반경정인경우 Enable false
        /// </summary>
        private void EnableChange_keypartInputPlace()
        {
            try
            {
                DataTable dtMtrl = window_PROCESSINFO.WO_MTRL_INFO;
                if (dtMtrl != null)
                {
                    if (dtMtrl.Rows.Count > 0)
                    {
                        if (Util.NVC(dtMtrl.Rows[0]["MTRLID"]).Length > 0)
                        {
                            EnableChange_keypartInputPlace(true);
                        }
                        else
                        {
                            EnableChange_keypartInputPlace(false);
                        }
                    }
                    else
                    {
                        EnableChange_keypartInputPlace(false);
                    }
                }
                else
                {
                    EnableChange_keypartInputPlace(false);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void EnableChange_keypartInputPlace(bool bChange)
        {
            try
            {
                bMtrlInputProcFlag = bChange;
                gridWorkInfo.IsEnabled = bChange;
                txtKeyPartInput.IsEnabled = bChange;
                btnKeyPartDelete.IsEnabled = bChange;

                if (bChange)
                {
                    dgWipList.Columns["MTRLQTY"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgWipList.Columns["MTRLQTY"].Visibility = Visibility.Collapsed;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setLabelCode(string sProdID)
        {
            try
            {
                if (sProdID.Length > 0)
                {
                    CommonCombo _combo = new CommonCombo();
                    //라벨 세팅
                    String[] sFilter = { sProdID, null, null, LABEL_TYPE_CODE.PACK };
                    _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");

                    int combo_cnt = cboLabelCode.Items.Count;

                    for (int i = 0; i < combo_cnt; i++)
                    {
                        DataRowView drv = cboLabelCode.Items[i] as DataRowView;
                        string temp = drv["CBO_CODE"].ToString();

                        if (now_labelcode == temp)
                        {
                            cboLabelCode.SelectedValue = now_labelcode;
                            break;
                        }
                        else
                        {
                            cboLabelCode.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    cboLabelCode.ItemsSource = null;
                    cboLabelCode.SelectedValue = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Method - 조회 & 그리드컨트롤

        /// <summary>
        /// 
        /// </summary>
        private void setProcessInfo()
        {
            try
            {
                if (gridWorkInfo.Children.Count == 0)
                {
                    //window_PROCESSINFO.PACK001_001 = this;
                    gridWorkInfo.Children.Add(window_PROCESSINFO);
                }


                //if (dgSub.Children.Count == 0)
                //{
                //    window01.ProtoType0205 = this;
                //    dgSub.Children.Add(window01);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool setAuthClctVal()
        {
            bool bAuth = false;

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USERNAME", typeof(string));
                RQSTDT.Columns.Add("APPR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USERNAME"] = LoginInfo.USERID;
                dr["APPR_CODE"] = "GMES_PACK_SM_CLCTVAL";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPR_PERSON_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    dgInspection.IsReadOnly = false;
                    clctVal.IsReadOnly = false;
                    bAuth = true;
                }
                else
                {
                    if (LoginInfo.CFG_AREA_ID == "P1" || LoginInfo.CFG_AREA_ID == "PE")
                    {
                        dgInspection.IsReadOnly = false;
                        clctVal.IsReadOnly = false;
                        bAuth = true;
                    }
                    else
                    {
                        clctVal.IsReadOnly = true;
                        bAuth = false;
                    }

                }
                return bAuth;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bAuth;
            }
        }

        private void getWipList()
        {
            try
            {
                //window_PROCESSINFO

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("COUNT", typeof(Int64));
                RQSTDT.Columns.Add("UPDDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM_TO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = window_PROCESSINFO.PROCID;
                dr["EQSGID"] = window_PROCESSINFO.EQSGID;
                dr["COUNT"] = cboListCount.SelectedValue;
                dr["UPDDTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["UPDDTTM_TO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                //dgWipList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgWipList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_WIP_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getKeypart(string sLotid)
        {
            try
            {
                //DA_PRD_SEL_TB_SFC_WIP_INPUT_MTRL_HIST
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_WIP_INPUT_MTRL_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgKeyPart.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgKeyPart, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_TB_SFC_WIP_INPUT_MTRL_HIST", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }


        private string SetMESJudge(string CLCTITEM_LSL_VALUE, string CLCTITEM_USL_VALUE, string CLCTVAL)
        {
            string judge = string.Empty;
            double CLCTITEM_LSL_VALUE_DECIMAL = 0;
            double CLCTITEM_USL_VALUE_DECIMAL = 0;
            double CLCTVAL_DECIMAL = 0;
            bool ISNUMERIC_LSL = double.TryParse(CLCTITEM_LSL_VALUE, out CLCTITEM_LSL_VALUE_DECIMAL);
            bool ISNUMERIC_USL = double.TryParse(CLCTITEM_USL_VALUE, out CLCTITEM_USL_VALUE_DECIMAL);
            bool ISNUMERIC_CLCTVAL = double.TryParse(CLCTVAL, out CLCTVAL_DECIMAL);
            bool NOTSET_USL = (CLCTITEM_USL_VALUE == null || string.IsNullOrEmpty(CLCTITEM_USL_VALUE)) ? true : false;
            bool NOTSET_LSL = (CLCTITEM_LSL_VALUE == null || string.IsNullOrEmpty(CLCTITEM_LSL_VALUE)) ? true : false;
            bool NOTSET_CLCTVAL = (CLCTVAL == null || string.IsNullOrEmpty(CLCTVAL)) ? true : false;

            int checkStepNo = 0;
            // USL/LSL 둘다 숫자인경우
            if (ISNUMERIC_USL && ISNUMERIC_LSL && !NOTSET_USL && !NOTSET_LSL && ISNUMERIC_CLCTVAL && !NOTSET_CLCTVAL && ISNUMERIC_CLCTVAL && !NOTSET_CLCTVAL) checkStepNo = 1;    // USL,LSL 둘다 설정되었을 경우
            else if (ISNUMERIC_USL && !NOTSET_USL && NOTSET_LSL && ISNUMERIC_CLCTVAL && !NOTSET_CLCTVAL && ISNUMERIC_CLCTVAL && !NOTSET_CLCTVAL) checkStepNo = 2;    // USL만 설정되었을 경우
            else if (ISNUMERIC_LSL && NOTSET_USL && !NOTSET_LSL && ISNUMERIC_CLCTVAL && !NOTSET_CLCTVAL && ISNUMERIC_CLCTVAL && !NOTSET_CLCTVAL) checkStepNo = 3;    // LSL만 설정되었을 경우
            else if (ISNUMERIC_USL && ISNUMERIC_LSL && !NOTSET_USL && !NOTSET_LSL && ISNUMERIC_CLCTVAL && !NOTSET_CLCTVAL && !ISNUMERIC_CLCTVAL) checkStepNo = 0;    // USL,LSL은 숫자이나 VALUE가 문자인 경우
            // USL/LSL 둘다 문자라면 다 문자 취급
            else if (ISNUMERIC_USL != ISNUMERIC_LSL && !NOTSET_USL && !NOTSET_LSL && CLCTITEM_USL_VALUE == CLCTITEM_LSL_VALUE) checkStepNo = 4;    // USL,LSL 둘다 설정되었을 경우 (상하한이 같은 값이면)
            else if (ISNUMERIC_USL != ISNUMERIC_LSL && !NOTSET_USL && !NOTSET_LSL && CLCTITEM_USL_VALUE != CLCTITEM_LSL_VALUE) checkStepNo = 5;    // USL,LSL 둘다 설정되었을 경우 (상하한이 다른 값이면)
            else if (ISNUMERIC_USL != ISNUMERIC_LSL && !NOTSET_USL && NOTSET_LSL) checkStepNo = 6;    // USL만 설정되었을 경우
            else if (ISNUMERIC_USL != ISNUMERIC_LSL && NOTSET_USL && !NOTSET_LSL) checkStepNo = 7;    // LSL만 설정되었을 경우
            // USL/LSL 둘다 문자라면 다 문자 취급
            else if (!ISNUMERIC_USL && !ISNUMERIC_LSL && !NOTSET_USL && !NOTSET_LSL && CLCTITEM_USL_VALUE == CLCTITEM_LSL_VALUE) checkStepNo = 4;    // USL,LSL 둘다 설정되었을 경우 (상하한이 같은 값이면)
            else if (!ISNUMERIC_USL && !ISNUMERIC_LSL && !NOTSET_USL && !NOTSET_LSL && CLCTITEM_USL_VALUE != CLCTITEM_LSL_VALUE) checkStepNo = 5;    // USL,LSL 둘다 설정되었을 경우 (상하한이 다른 값이면)
            else if (!ISNUMERIC_USL && !ISNUMERIC_LSL && !NOTSET_USL && NOTSET_LSL) checkStepNo = 6;    // USL만 설정되었을 경우
            else if (!ISNUMERIC_USL && !ISNUMERIC_LSL && NOTSET_USL && !NOTSET_LSL) checkStepNo = 7;    // LSL만 설정되었을 경우

            switch (checkStepNo)
            {
                case 1:
                    judge = (CLCTVAL_DECIMAL >= CLCTITEM_LSL_VALUE_DECIMAL && CLCTVAL_DECIMAL <= CLCTITEM_USL_VALUE_DECIMAL) ? "Y" : "N";
                    break;
                case 2:
                    judge = (CLCTVAL_DECIMAL <= CLCTITEM_USL_VALUE_DECIMAL) ? "Y" : "N";
                    break;
                case 3:
                    judge = (CLCTVAL_DECIMAL >= CLCTITEM_LSL_VALUE_DECIMAL) ? "Y" : "N";
                    break;
                case 4:
                    judge = (CLCTVAL == CLCTITEM_LSL_VALUE && CLCTVAL == CLCTITEM_USL_VALUE) ? "Y" : "N";
                    break;
                case 5:
                    judge = (CLCTVAL == CLCTITEM_LSL_VALUE || CLCTVAL == CLCTITEM_USL_VALUE) ? "Y" : "N";
                    break;
                case 6:
                    judge = (CLCTVAL == CLCTITEM_USL_VALUE) ? "Y" : "N";
                    break;
                case 7:
                    judge = (CLCTVAL == CLCTITEM_LSL_VALUE) ? "Y" : "N";
                    break;
                default:
                    judge = "N";
                    break;
            }

            return judge;
        }
        /// <summary>
        /// 이전검사적용
        /// 없는경우 0으로 입력
        /// </summary>
        private void setInspectionBefore()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable("INDATA");
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LOTID"] = txtTargetLot.Text;
                drRQSTDT["PROCID"] = window_PROCESSINFO.PROCID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                DataTable dtRSLTDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_LOT_MAX", "INDATA", "OUTDATA", dtRQSTDT);

                if (!CommonVerify.HasTableRow(dtRSLTDT))
                {
                    setAuthClctVal();
                    return;
                }

                // 이미 임력한 판정값이 있으면, Clear
                for (int rowIndex = 0; rowIndex < this.dgInspection.GetRowCount(); rowIndex++)
                {
                    DataTableConverter.SetValue(this.dgInspection.Rows[rowIndex].DataItem, "CLCTVAL", string.Empty);
                    DataTableConverter.SetValue(this.dgInspection.Rows[rowIndex].DataItem, "PASSYN", string.Empty);
                }

                // 판정값만 가져와서 UI에서 판정
                // 
                foreach (DataRowView drvRSLTDT in dtRSLTDT.AsDataView())
                {
                    int findRowIndex = Util.gridFindDataRow(ref dgInspection, "CLCTITEM", Util.NVC(drvRSLTDT["CLCTITEM"]), false);
                    if (findRowIndex <= -1)
                    {
                        continue;
                    }

                    string CLCTITEM_LSL_VALUE = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[findRowIndex].DataItem, "LCL"));
                    string CLCTITEM_USL_VALUE = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[findRowIndex].DataItem, "UCL"));
                    DataTableConverter.SetValue(this.dgInspection.Rows[findRowIndex].DataItem, "CLCTVAL", drvRSLTDT["CLCTVAL01"].ToString());
                    DataTableConverter.SetValue(this.dgInspection.Rows[findRowIndex].DataItem, "PASSYN", this.SetMESJudge(CLCTITEM_LSL_VALUE, CLCTITEM_USL_VALUE, drvRSLTDT["CLCTVAL01"].ToString()));
                }

                // 색깔 변경
                for (int rowIndex = 0; rowIndex < this.dgInspection.GetRowCount(); rowIndex++)
                {
                    C1.WPF.DataGrid.DataGridCell dataGridCell = dgInspection.GetCell(rowIndex, this.dgInspection.Columns["CLCTVAL"].Index);

                    if (Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[rowIndex].DataItem, "PASSYN")) == "Y")
                    {
                        dataGridCell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        dataGridCell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[rowIndex].DataItem, "PASSYN")) == "N")
                    {
                        dataGridCell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        dataGridCell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        dataGridCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        dataGridCell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }

                this.txtConfirmNote.Text = ms.AlertRetun("SFU1682"); // 수동 검사 이전값 동일 입력
                this.txtConfirmNote.SelectAll();
                this.txtConfirmNote.Focus();
                this.dgInspection.CanUserSort = true;

                // 이전검사값 가져온 경우 수정못하게 막음.
                this.dgInspection.IsReadOnly = true;
                this.clctVal.IsReadOnly = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2018.06.11
        private bool GetDataTypeNumStr(string sInput)
        {
            int iCnt = 0;
            var sChar = sInput.ToCharArray();

            for (int i = 0; i < sChar.Length; i++)
            {
                if (char.IsNumber(sChar[i]) == false)
                {
                    iCnt = iCnt + 1;
                }
            }

            if (iCnt > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //2018.06.11

        #region 글자 포함 확인 정규식
        //2018.07.04, 염규범S 
        // INDATA  : sInput
        // OUTDATA : true, false
        // INDATA 의 내용이 Double 로 변경 가능한가 확인
        private bool ChkAvailabilityDouble(string sInput)
        {
            try
            {
                double iTemp = 0;
                bool bAvailabilityDouble;

                bAvailabilityDouble = double.TryParse(sInput, out iTemp);

                if (bAvailabilityDouble)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion 


        #region 글자 포함 확인 정규식
        //2018.07.04, 염규범S 
        // INDATA  : sInput
        // OUTDATA : true, false
        // INDATA 의 글자안에 숫자가 존재하나 확인
        private bool ChkStringExist(string sInput)
        {
            //int Temp1 = Regex.Replace(sInput, @"\D", "").Length;
            int Temp1 = Regex.Replace(sInput, @"^[+-]?\d*(\.?\d*)$", "").Length;
            int Temp2 = sInput.Length;

            if (Temp1 != Temp2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion 

        /// <summary>
        /// 검사데이터 불량 존재시 true
        /// </summary>
        /// <returns></returns>
        private bool GetDefectCount()
        {
            for (int i = 0; i < dgInspection.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "PASSYN")) == "N" || string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "PASSYN"))))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Method - 투입 삭제 로직
        private bool inputProduct(string LOTID, string materialLOTID)
        {
            defectResult = null;
            bool returnValue = true;
            try
            {
                // 자재 결합 순번 구하기
                // 2022-10-21 자재 TYPE : PROD, MTRL에 상관없이 입력한 순서대로 순번 집어넣는것으로 변경
                int inputSequence = 1;
                if (this.dgKeyPart.GetRowCount() > 0)
                {
                    var query = DataTableConverter.Convert(this.dgKeyPart.ItemsSource).AsEnumerable().OrderByDescending(x => Convert.ToInt32(x.Field<string>("PRDT_ATTCH_PSTN_NO")));
                    inputSequence = (query.Count() > 0) ? Convert.ToInt32(query.Select(x => x.Field<string>("PRDT_ATTCH_PSTN_NO")).FirstOrDefault()) + 1 : 1;
                }

                //int iMaxRow = 0;
                //for (int i = 0; i < dgKeyPart.Rows.Count; i++)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[i].DataItem, "INPUT_LOT_TYPE_CODE")) != "MTRL")
                //    {
                //        iMaxRow = i;
                //    }
                //}
                ////MTRL
                //if (dgKeyPart.Rows.Count > 0)
                //{
                //    object oAttch_Number = DataTableConverter.GetValue(dgKeyPart.Rows[iMaxRow].DataItem, "PRDT_ATTCH_PSTN_NO");
                //    string sINPUT_LOT_TYPE_CODE = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[iMaxRow].DataItem, "INPUT_LOT_TYPE_CODE"));
                //    if (oAttch_Number != null && sINPUT_LOT_TYPE_CODE != "MTRL")
                //    {
                //        inputSequence = Util.NVC_Int(DataTableConverter.GetValue(dgKeyPart.Rows[iMaxRow].DataItem, "PRDT_ATTCH_PSTN_NO")) + 1;
                //    }
                //    else
                //    {
                //        inputSequence = 0;
                //    }
                //}

                //BR_PRD_REG_START_LOT_PACK
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("MLOTID", typeof(string));
                INDATA.Columns.Add("MLOT_INPUTSEQ", typeof(Int32));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("COUNT", typeof(Int64));
                INDATA.Columns.Add("UPDDTTM_FROM", typeof(string));
                INDATA.Columns.Add("UPDDTTM_TO", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LOTID;
                dr["MLOTID"] = materialLOTID;
                dr["MLOT_INPUTSEQ"] = inputSequence;
                dr["PROCID"] = window_PROCESSINFO.PROCID;
                dr["EQPTID"] = window_PROCESSINFO.EQPTID;
                dr["USERID"] = LoginInfo.USERID;
                dr["COUNT"] = cboListCount.SelectedValue;
                dr["UPDDTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["UPDDTTM_TO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                dr["EQSGID"] = window_PROCESSINFO.EQSGID;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);
                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_LOT_PACK_LINE", "INDATA", "WIP_PROC,WIP_ROUTE,TB_SFC_WIP_INPUT_MTRL_HIST,ACTIVITYREASON,PRODUCTPROCESSQUALSPEC,WIPREASONCOLLECT,LOT_INFO,OUTDATA", dsInput, null);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    string sStartedLotid = string.Empty;
                    string sStartedProductID = string.Empty;
                    string sStartedProductName = string.Empty;

                    txtTargetLot.Text = "";
                    txtTargeProduct.Text = "";

                    if ((dsResult.Tables.IndexOf("LOT_INFO") > -1))
                    {
                        if (dsResult.Tables["LOT_INFO"].Rows.Count > 0)
                        {
                            sStartedLotid = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["LOTID"]);
                            sStartedProductID = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODID"]);
                            sStartedProductName = Util.NVC(dsResult.Tables["LOT_INFO"].Rows[0]["PRODNAME"]);

                            txtProdutLotInput.Text = sStartedLotid;
                            txtTargetLot.Text = sStartedLotid;
                            txtTargeProduct.Text = sStartedProductID;
                        }
                    }
                    if ((dsResult.Tables.IndexOf("WIP_PROC") > -1))
                    {
                        //dgWipList.ItemsSource = DataTableConverter.Convert(dsResult.Tables["WIP_PROC"]);
                        Util.GridSetData(dgWipList, dsResult.Tables["WIP_PROC"], FrameOperation, true);
                        Util.SetTextBlockText_DataGridRowCount(tbWipListCount, Util.NVC(dgWipList.Rows.Count));
                        //바인딩후 포커스이동
                        Util.gridFindDataRow(ref dgWipList, "LOTID", sStartedLotid, true);
                    }

                    if ((dsResult.Tables.IndexOf("TB_SFC_WIP_INPUT_MTRL_HIST") > -1))
                    {
                        dgKeyPart.ItemsSource = DataTableConverter.Convert(dsResult.Tables["TB_SFC_WIP_INPUT_MTRL_HIST"]);
                    }

                    if ((dsResult.Tables.IndexOf("PRODUCTPROCESSQUALSPEC") > -1))
                    {
                        dgInspection.ItemsSource = DataTableConverter.Convert(dsResult.Tables["PRODUCTPROCESSQUALSPEC"]);
                        //Column의 Width Auto 속성이 적용이안되서. 셋팅시 Auto로 셋팅.
                        Util.GridAllColumnWidthAuto(ref dgInspection);

                        //2020.04.30
                        //if (LoginInfo.CFG_SHOP_ID != "A040")
                        //{
                        //    dgInspection.IsReadOnly = true;
                        //}
                        //else
                        //{
                        //    if (LoginInfo.CFG_AREA_ID == "P1")
                        //    {
                        //        dgInspection.IsReadOnly = false;
                        //    }
                        //    else
                        //    {
                        //        dgInspection.IsReadOnly = true;
                        //    }
                        //}

                        //2022.09.16 염규범 S
                        //입력 권한 변경의 건 
                        setAuthClctVal();
                    }

                    if ((dsResult.Tables.IndexOf("ACTIVITYREASON") > -1))
                    {
                        if (cboResult.SelectedIndex == 1)
                        {
                            //dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["ACTIVITYREASON"]);
                            //Util.GridAllColumnWidthAuto(ref dgDefect);

                            setDefectResult();

                            Util.gridClear(dgDefect0);
                            Util.gridClear(dgDefect1);
                            Util.gridClear(dgDefect2);
                            Util.gridClear(dgDefect3);
                            Util.gridClear(dgDefect4);

                            if (defectResult != null && defectResult.Rows.Count > 0)
                            {
                                Util.GridSetData(dgDefect0, getNextRESNCODE(new string[0]), FrameOperation, true);
                            }
                            else
                            {
                                ms.AlertInfo("SFU5062", window_PROCESSINFO.PROCNAME); //불량코드가 존재하지 않습니다.
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        private bool inputKeyPartProduct(string materialLOT = null)
        {
            bool returnValue = true;
            try
            {
                // Declarations...
                string inputLOTID = string.Empty;
                string keyPartLOTID = string.Empty;

                keyPartLOTID = (string.IsNullOrEmpty(materialLOT)) ? txtKeyPartInput.Text : materialLOT;

                //JOBEND 바코드 KEYPART에 입력시 KEYPART 결합체크후 -> 라벨발행 -> 초기화
                if (txtTargetLot.Text.Length > 0
                    && keyPartLOTID == "JOBEND"
                    && dgKeyPart.Rows.Count > 0)
                {
                    JobEnd();
                    return true;
                }

                if (dgKeyPart.Rows.Count > 0)
                {
                    inputLOTID = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[0].DataItem, "LOTID"));

                    returnValue = inputProduct(inputLOTID, keyPartLOTID);
                }
                else
                {
                    if (
                        (txtProdutLotInput.Text == txtTargetLot.Text)
                        && (txtProdutLotInput.Text.Length > 0)
                        && (txtTargetLot.Text.Length > 0)
                        )
                    {
                        inputLOTID = txtTargetLot.Text;
                    }
                    returnValue = inputProduct(inputLOTID, keyPartLOTID);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return returnValue;
        }

        /// <summary>
        /// 선택 KeyPart 삭제
        /// </summary>
        private void delKeyPart()
        {
            try
            {
                if (!(dgKeyPart.Rows.Count > 0))
                {
                    if (txtTargetLot.Text.Length > 0)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1279", txtTargetLot.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //"!!!! 주의 !!!!\n\nKEYPART가 모두 삭제되었습니다. 생성된 LOT ID({0})도 삭제 하시겠습니까?.
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                delKeyPartBiz(txtTargetLot.Text, null);

                                Refresh();
                            }
                        });
                    }
                    return;
                }
                int iMLotIndex = Util.gridFindDataRow(ref dgKeyPart, "CHK", "True", false);
                if (iMLotIndex == -1)
                {
                    return;
                }

                string sLotid = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[iMLotIndex].DataItem, "LOTID"));
                string sMLotid = Util.NVC(DataTableConverter.GetValue(dgKeyPart.Rows[iMLotIndex].DataItem, "INPUT_LOTID"));


                DataTable dtResult = delKeyPartBiz(sLotid, sMLotid);
                if (dtResult != null)
                {
                    dgKeyPart.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dgKeyPart.Rows.Count == 0)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1279", sLotid), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //"!!!! 주의 !!!!\n\nKEYPART가 모두 삭제되었습니다. 생성된 LOT ID({0})도 삭제 하시겠습니까?.
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                delKeyPartBiz(sLotid, null);

                                Refresh();
                            }
                        });
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgKeyPart.Rows[dgKeyPart.Rows.Count - 1].DataItem, "CHK", true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 삭제 비즈룰 호출
        /// sMLotid 가 null 인경우 Lot 삭제
        /// </summary>
        /// <param name="sLotid"></param>
        /// <param name="sMLotid"></param>
        /// <returns></returns>
        private DataTable delKeyPartBiz(string sLotid, string sMLotid)
        {
            DataTable dtReturn = null;
            try
            {
                int iDelBeforRowCount = dgKeyPart.Rows.Count;
                DataRow dr = null;
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("MLOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                dr["MLOTID"] = sMLotid;
                dr["PROCID"] = window_PROCESSINFO.PROCID;
                dr["EQPTID"] = window_PROCESSINFO.EQPTID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                dtReturn = new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_KEYPART", "INDATA", "OUTDATA", RQSTDT);
                if (dtReturn != null)
                {
                    if (dtReturn.Rows.Count > 0)
                    {
                        if (iDelBeforRowCount < dtReturn.Rows.Count)
                        {
                            //결합해제하였습니다.
                        }

                    }
                    else
                    {
                        if (sMLotid == null)
                        {
                            ms.AlertInfo("SFU1363", sLotid); //LOT ID({0})를 삭제 하였습니다.
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_DEL_KEYPART", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dtReturn;
        }
        /// <summary>
        /// 완공처리
        /// ※일반공정완공 : BR_PRD_REG_OUTPUTASSY
        ///    -완공처리
        /// ※결합공정완공 : BR_PRD_REG_SUBEND
        ///    -양품:자재결합, 단위완공(검사데이터 저장)
        ///    -불량:자재결합, 완공처리
        /// </summary>
        private void ConfirmOKNG()
        {
            try
            {
                DataSet dsResult = null;
                if (bMtrlInputProcFlag)
                {
                    //결합공정 완공
                    dsResult = BR_PRD_REG_SUBEND();
                }
                else
                {
                    //일반공정 완공
                    dsResult = BR_PRD_REG_OUTPUTASSY();
                }
                if (dsResult != null)
                {
                    if (dsResult.Tables["OUTDATA"] != null)
                    {
                        if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            ms.AlertInfo("SFU1275"); //정상처리 되었습니다.

                            Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private DataSet BR_PRD_REG_OUTPUTASSY()
        {
            DataSet dsResult = null;
            try
            {
                //resncode 양품인경우는 OK
                string sResnCode = string.Empty;
                if (cboResult.SelectedIndex == 0)
                {
                    sResnCode = "OK";
                }
                else
                {
                    //if (dgDefect.Rows.Count > 0)
                    //{
                    //    sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");
                    //}
                    if (dsResult != null && dsResult.Tables.IndexOf("ACTIVITYREASON") > -1 && dsResult.Tables["ACTIVITYREASON"].Rows.Count > 0)
                    {
                        sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");
                    }
                    else
                    {
                        sResnCode = "NG";
                    }
                }

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("END_PROCID", typeof(string));
                INDATA.Columns.Add("END_EQPTID", typeof(string));
                INDATA.Columns.Add("STRT_PROCID", typeof(string));
                INDATA.Columns.Add("STRT_EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("WIPNOTE", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("RESNDESC", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = txtTargetLot.Text;
                drINDATA["END_PROCID"] = window_PROCESSINFO.PROCID;
                drINDATA["END_EQPTID"] = window_PROCESSINFO.EQPTID;
                drINDATA["STRT_PROCID"] = null;
                drINDATA["STRT_EQPTID"] = null;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["WIPNOTE"] = txtConfirmNote.Text;
                drINDATA["RESNCODE"] = sResnCode;
                drINDATA["RESNNOTE"] = txtConfirmNote.Text;
                drINDATA["RESNDESC"] = "";
                INDATA.Rows.Add(drINDATA);

                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                IN_CLCTITEM.Columns.Add("PASSYN", typeof(string));

                DataRow drIN_CLCTITEM = null;
                if (dgInspection.Rows.Count > 0)
                {
                    for (int i = 0; i < dgInspection.Rows.Count; i++)
                    {
                        if (Util.NVC(cboResult.SelectedValue) == "OK")
                        {
                            drIN_CLCTITEM = IN_CLCTITEM.NewRow();
                            drIN_CLCTITEM["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTITEM"));
                            drIN_CLCTITEM["CLCTVAL"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL"));
                            drIN_CLCTITEM["PASSYN"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "PASSYN"));
                            IN_CLCTITEM.Rows.Add(drIN_CLCTITEM);
                        }
                        else if (!(string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL")))))
                        {
                            drIN_CLCTITEM = IN_CLCTITEM.NewRow();
                            drIN_CLCTITEM["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTITEM"));
                            drIN_CLCTITEM["CLCTVAL"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL"));
                            drIN_CLCTITEM["PASSYN"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "PASSYN"));
                            IN_CLCTITEM.Rows.Add(drIN_CLCTITEM);
                        }
                    }
                }
                dsInput.Tables.Add(IN_CLCTITEM);

                DataTable IN_CLCTDITEM = new DataTable();
                IN_CLCTDITEM.TableName = "IN_CLCTDITEM";
                IN_CLCTDITEM.Columns.Add("CLCTDITEM", typeof(string));
                IN_CLCTDITEM.Columns.Add("CLCTDVAL", typeof(string));
                dsInput.Tables.Add(IN_CLCTDITEM);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTPUTASSY", "INDATA,IN_CLCTITEM,IN_CLCTDITEM", "OUTDATA", dsInput, null);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_OUTPUTASSY", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dsResult;
        }

        private DataSet BR_PRD_REG_SUBEND()
        {
            DataSet dsResult = null;
            try
            {
                //resncode 양품인경우는 OK
                string sResnCode = string.Empty;
                if (Util.NVC(cboResult.SelectedValue) == "OK")
                {
                    sResnCode = "OK";
                }
                else
                {
                    //if (dgDefect.Rows.Count > 0)
                    //{
                    //    sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");
                    //}
                    if (dsResult != null && dsResult.Tables.IndexOf("ACTIVITYREASON") > -1 && dsResult.Tables["ACTIVITYREASON"].Rows.Count > 0)
                    {
                        sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");
                    }
                    else
                    {
                        sResnCode = "NG";
                    }
                }

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("END_PROCID", typeof(string));
                INDATA.Columns.Add("END_EQPTID", typeof(string));
                INDATA.Columns.Add("STRT_PROCID", typeof(string));
                INDATA.Columns.Add("STRT_EQPTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("WIPNOTE", typeof(string));
                INDATA.Columns.Add("RESNCODE", typeof(string));
                INDATA.Columns.Add("RESNNOTE", typeof(string));
                INDATA.Columns.Add("RESNDESC", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = txtTargetLot.Text;
                drINDATA["END_PROCID"] = window_PROCESSINFO.PROCID;
                drINDATA["END_EQPTID"] = window_PROCESSINFO.EQPTID;
                drINDATA["STRT_PROCID"] = null;
                drINDATA["STRT_EQPTID"] = null;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["WIPNOTE"] = txtConfirmNote.Text;
                drINDATA["RESNCODE"] = sResnCode;
                drINDATA["RESNNOTE"] = txtConfirmNote.Text;
                drINDATA["RESNDESC"] = "";
                INDATA.Rows.Add(drINDATA);
                dsInput.Tables.Add(INDATA);

                DataTable IN_CLCTITEM = new DataTable();
                IN_CLCTITEM.TableName = "IN_CLCTITEM";
                IN_CLCTITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CLCTITEM.Columns.Add("CLCTVAL", typeof(string));
                IN_CLCTITEM.Columns.Add("PASSYN", typeof(string));

                DataRow drIN_CLCTITEM = null;
                dgInspection.EndEdit();
                if (dgInspection.Rows.Count > 0)
                {
                    for (int i = 0; i < dgInspection.Rows.Count; i++)
                    {
                        if (Util.NVC(cboResult.SelectedValue) == "OK")
                        {
                            drIN_CLCTITEM = IN_CLCTITEM.NewRow();
                            drIN_CLCTITEM["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTITEM"));
                            drIN_CLCTITEM["CLCTVAL"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL"));
                            drIN_CLCTITEM["PASSYN"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "PASSYN"));
                            IN_CLCTITEM.Rows.Add(drIN_CLCTITEM);
                        }
                        else if (!(string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL")))))
                        {
                            drIN_CLCTITEM = IN_CLCTITEM.NewRow();
                            drIN_CLCTITEM["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTITEM"));
                            drIN_CLCTITEM["CLCTVAL"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL"));
                            drIN_CLCTITEM["PASSYN"] = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "PASSYN"));
                            IN_CLCTITEM.Rows.Add(drIN_CLCTITEM);
                        }
                    }
                }
                dsInput.Tables.Add(IN_CLCTITEM);


                DataTable IN_CLCTDITEM = new DataTable();
                IN_CLCTDITEM.TableName = "IN_CLCTDITEM";
                IN_CLCTDITEM.Columns.Add("CLCTDITEM", typeof(string));
                IN_CLCTDITEM.Columns.Add("CLCTDVAL", typeof(string));
                dsInput.Tables.Add(IN_CLCTDITEM);

                DataTable IN_MTRL = new DataTable();
                IN_MTRL.TableName = "IN_MTRL";
                IN_MTRL.Columns.Add("LOTMID", typeof(string));
                IN_MTRL.Columns.Add("INPUTSEQ", typeof(string));
                dsInput.Tables.Add(IN_MTRL);


                //dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SUBEND", "INDATA,IN_CLCTITEM,IN_CLCTDITEM,IN_MTRL", "OUTDATA", dsInput, null);
                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SUBEND_UI", "INDATA,IN_CLCTITEM,IN_CLCTDITEM,IN_MTRL", "OUTDATA", dsInput, null);

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_SUBEND_UI", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dsResult;
        }

        /// <summary>
        /// 'JOBEND'바코드를 키파트입력TEXT에 입력시
        /// 결합수량 확인 -> 바코드발행 -> 초기화
        /// </summary>
        private void JobEnd()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtTargetLot.Text;
                dr["PRODID"] = window_PROCESSINFO.PRODUCTID;
                dr["PROCID"] = window_PROCESSINFO.PROCID;
                dr["EQSGID"] = window_PROCESSINFO.EQSGID;
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_KEYPART", "RQSTDT", "RSLTDT", INDATA);

                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        int iPrintQty = 1;
                        string sPrintQty = "1";

                        foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                            {
                                int.TryParse(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString(), out iPrintQty);
                                break;
                            }
                        }
                        sPrintQty = iPrintQty.ToString();
                        //바코드발행
                        string sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                        if (sLabelCode.Length > 0)
                        {
                            Util.printLabel_Pack(FrameOperation, loadingIndicator, txtTargetLot.Text, LABEL_TYPE_CODE.PACK, sLabelCode, "N", sPrintQty, window_PROCESSINFO.PRODUCTID);
                        }
                        else
                        {
                            Util.printLabel_Pack(FrameOperation, loadingIndicator, txtTargetLot.Text, LABEL_TYPE_CODE.PACK, "N", sPrintQty, null);
                        }

                        //초기화
                        Refresh();
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Method - Validation

        /// <summary>
        /// Confirm Validation
        /// NG : false
        /// </summary>
        /// <returns></returns>
        private bool ConfirmValidation()
        {
            bool bReturn = true;
            try
            {
                //착공LOT 존재 확인.
                if (txtTargetLot.Text == "")
                {
                    ms.AlertWarning("SFU1746"); //완료할 LOT이 없습니다.
                    bReturn = false;
                    txtTargetLot.Focus();
                    return bReturn;
                }

                //불량일경우 VALIDATION
                int iRow = -1;

                //if (cboResult.SelectedIndex == 1)
                if (Util.NVC(cboResult.SelectedValue) == "NG")
                {
                    //불량코드 기준정보가 있는경우에만 선택 validation
                    if (dsResult != null && dsResult.Tables.IndexOf("ACTIVITYREASON") > -1 && dsResult.Tables["ACTIVITYREASON"].Rows.Count > 0)
                    {
                        iRow = Util.gridFindDataRow(ref dgDefect, "CHK", "True", false);

                        if (iRow == -1)
                        {
                            ms.AlertWarning("SFU1639"); //선택된 불량코드가 없습니다
                            bReturn = false;
                            return bReturn;
                        }
                    }
                }

                //수동 처리시 공정변경사유. 필수선택 단,JOBEND 결합공정 인경우 체크 하지않음.
                if (txtConfirmNote.Text.Trim() == "" && txtKeyPartInput.Text != "JOBEND" && !txtKeyPartInput.IsEnabled)
                {
                    ms.AlertWarning("SFU1457"); //공정변경사유는 필수 항목 입니다.
                    bReturn = false;
                    return bReturn;
                }

                if (Util.NVC(cboResult.SelectedValue) == "OK")
                {
                    if (dgInspection.Rows.Count > 0)
                    {
                        for (int i = 0; i < dgInspection.Rows.Count; i++)
                        {
                            string itemVal = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[i].DataItem, "CLCTVAL"));

                            if (itemVal == "")
                            {
                                ms.AlertWarning("SFU1810"); //입력하신 측정값이 옮지 않습니다. 확인 하세요
                                                            //포커스이동
                                Util.gridSetFocusRow(ref dgInspection, i);
                                bReturn = false;
                                return bReturn;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }

        /// <summary>
        /// 공정선택확인
        /// false: 공정선택상태 true: 공정 없음
        /// </summary>
        private bool chkProcessSelect()
        {
            bool bReturn = false;
            try
            {
                if (window_PROCESSINFO.PROCID != null)
                {
                    if (!(window_PROCESSINFO.PROCID.Length > 0))
                    {
                        bReturn = true;
                    }
                }
                else
                {
                    bReturn = true;
                }

                if (bReturn)
                {
                    ms.AlertWarning("SFU1599"); //상단의[공정버튼]으로공정을선택하세요.
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private void chkLotProcid()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "dtRQSTDT";
                dtRQSTDT.Columns.Add("LOTID", typeof(string));


                DataRow dr = dtRQSTDT.NewRow();
                dr["LOTID"] = txtTargetLot.Text;

                dtRQSTDT.Rows.Add(dr);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_PROCID_COMPLETE", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dsResult != null)
                {
                    if (dsResult.Rows.Count > 0)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU8257"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ConfirmOKNG();
                            }
                        });
                    }
                    else
                    {
                        ConfirmOKNG();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return;
        }

        private bool chkInputKeypart(string sInputKeypart)
        {
            bool bReturn = true;
            try
            {
                int iRow = Util.gridFindDataRow(ref dgKeyPart, "INPUT_LOTID", sInputKeypart, false);
                if (iRow >= 0)
                {
                    ms.AlertWarning("SFU1782"); //이미 투입 된 KEYPART 입니다.
                    bReturn = false;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }


        #endregion


        #endregion


        /// <summary>
        /// 
        /// 투입keypart DataGrid
        /// 투입자재수량만큼 로우 추가한후.
        /// dgKeyPart 에 바인딩...
        /// </summary>
        /// <param name="dtINPUT_MTRL_HIST">투입LOT의 결합된 KEYPART정보</param>

        private void keyPartGridSetting(DataTable dtINPUT_MTRL_HIST)
        {
            try
            {

                DataTable dtKeypart = dtINPUT_MTRL_HIST.Clone();

                DataTable dtWO_MTRL_INFO = window_PROCESSINFO.WO_MTRL_INFO.Copy();
                if (dtWO_MTRL_INFO != null)
                {
                    for (int i = 0; i < dtWO_MTRL_INFO.Rows.Count; i++)
                    {
                        string sMTRLID = Util.NVC(dtWO_MTRL_INFO.Rows[i]["MTRLID"]);
                        string sPROC_INPUT_QTY = Util.NVC(dtWO_MTRL_INFO.Rows[i]["PROC_INPUT_QTY"]);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgInspection_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PASSYN")).Equals("Y"))
                    {
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                        if (e.Cell.Column.Name.Equals("CLCTVAL"))
                        {
                            dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("CLCTVAL"))
                        {
                            //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                            dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
                        }
                        else
                        {
                            dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
            }));
        }

        private void dgInspection_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = null;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        // 2023-11-30 : E20231017-000561 (수동공정진척 UI 판정/공정기준 GMES판정/수집항목기준 GMES 판정기준 상이부분 통일화)
        private void SetMESJudge(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid c1DataGrid = sender as C1DataGrid;

            if (!(LoginInfo.CFG_AREA_ID != "P1" && LoginInfo.CFG_AREA_ID != "PE") || !setAuthClctVal())
            {
                return;
            }

            string CLCTITEM_LSL_VALUE = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "LCL"));
            string CLCTITEM_USL_VALUE = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "UCL"));
            string CLCTVAL = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL"));
            DataTableConverter.SetValue(dgInspection.CurrentRow.DataItem, "PASSYN", this.SetMESJudge(CLCTITEM_LSL_VALUE, CLCTITEM_USL_VALUE, CLCTVAL));

            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PASSYN")).Equals("Y"))
            {
                //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                if (e.Cell.Column.Name.Equals("CLCTVAL"))
                {
                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                }
                else
                {
                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
                }
            }
            else
            {
                if (e.Cell.Column.Name.Equals("CLCTVAL"))
                {
                    //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
                }
                else
                {
                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
                }
            }

        }

        private void dgInspection_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.SetMESJudge(sender, e);
            return;
            // 아래는 안씀.
            //try
            //{
            //    C1DataGrid c1DataGrid = sender as C1DataGrid;

            //    if ((LoginInfo.CFG_AREA_ID == "P1") || (LoginInfo.CFG_AREA_ID == "PE"))
            //    {
            //        String LCL = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "LCL"));
            //        String UCL = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "UCL"));
            //        String CLCTVAL = Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL"));

            //        double p1 = 0;
            //        bool isDecimalConvertLCL = double.TryParse(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "LCL")), out p1);
            //        bool isDecimalConvertUCL = double.TryParse(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "UCL")), out p1);
            //        bool isDecimalConvertCLCTVAL = double.TryParse(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL")), out p1);

            //        // CLCTVAL : 숫자또는문자, LCL : 숫자, UCL : 문자 or CLCTVAL : 숫자또는문자, LCL : 문자, UCL : 숫자
            //        if (isDecimalConvertLCL != isDecimalConvertUCL)
            //        {
            //            DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "N");

            //            if (e.Cell.Presenter != null)
            //            {
            //                if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                }
            //                else
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                }
            //            }
            //        }
            //        // CLCTVAL : 숫자, LCL : 숫자, UCL : 숫자
            //        else if (isDecimalConvertLCL && isDecimalConvertUCL && isDecimalConvertCLCTVAL)
            //        {
            //            if (decimal.Parse(LCL) <= decimal.Parse(CLCTVAL) && decimal.Parse(UCL) >= decimal.Parse(CLCTVAL))
            //            {
            //                DataTableConverter.SetValue(dgInspection.CurrentRow.DataItem, "PASSYN", "Y");
            //            }
            //            else
            //            {
            //                DataTableConverter.SetValue(dgInspection.CurrentRow.DataItem, "PASSYN", "N");
            //            }

            //            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PASSYN")).Equals("Y"))
            //            {
            //                //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
            //                if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                {
            //                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
            //                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
            //                }
            //                else
            //                {
            //                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
            //                }
            //            }
            //            else
            //            {
            //                if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                {
            //                    //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
            //                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
            //                }
            //                else
            //                {
            //                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                    dgInspection.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontWeight = FontWeights.Normal;
            //                }
            //            }
            //        }
            //        // CLCTVAL : 문자, LCL : 숫자, UCL : 숫자
            //        else if (isDecimalConvertLCL && isDecimalConvertUCL && !isDecimalConvertCLCTVAL)
            //        {
            //            DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "N");

            //            if (e.Cell.Presenter != null)
            //            {
            //                if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                }
            //                else
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            if (Util.ToString(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL"))).Equals(Util.ToString(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "UCL"))))
            //                || Util.ToString(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL"))).Equals(Util.ToString(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "LCL")))))
            //            {
            //                DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "Y");

            //                if (e.Cell.Presenter != null)
            //                {
            //                    if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
            //                    }
            //                    else
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "N");

            //                if (e.Cell.Presenter != null)
            //                {
            //                    if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                    else
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                }
            //            }
            //        }


            //    }
            //    else if (setAuthClctVal())
            //    {

            //        double p1 = 0;
            //        bool isDecimalConvertLCL = double.TryParse(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "LCL")), out p1);
            //        bool isDecimalConvertUCL = double.TryParse(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "UCL")), out p1);
            //        bool isDecimalConvertCLCTVAL = double.TryParse(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL")), out p1);

            //        if (isDecimalConvertLCL != isDecimalConvertUCL)
            //        {
            //            DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "N");

            //            if (e.Cell.Presenter != null)
            //            {
            //                if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                }
            //                else
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                }
            //            }
            //        }
            //        else if (isDecimalConvertLCL && isDecimalConvertUCL && isDecimalConvertCLCTVAL)
            //        {
            //            if (Util.StringToDouble(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "LCL"))) <= Util.StringToDouble(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL")))
            //                && Util.StringToDouble(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "UCL"))) >= Util.StringToDouble(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL")))
            //                )
            //            {
            //                DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "Y");

            //                if (e.Cell.Presenter != null)
            //                {
            //                    if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
            //                    }
            //                    else
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "N");

            //                if (e.Cell.Presenter != null)
            //                {
            //                    if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                    else
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                }

            //            }
            //        }
            //        // CLCTVAL : 문자, LCL : 숫자, UCL : 숫자
            //        else if (isDecimalConvertLCL && isDecimalConvertUCL && !isDecimalConvertCLCTVAL)
            //        {
            //            DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "N");

            //            if (e.Cell.Presenter != null)
            //            {
            //                if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                }
            //                else
            //                {
            //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            if (Util.ToString(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL"))).Equals(Util.ToString(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "UCL"))))
            //                || Util.ToString(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "CLCTVAL"))).Equals(Util.ToString(Util.NVC(DataTableConverter.GetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "LCL")))))
            //            {
            //                DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "Y");

            //                if (e.Cell.Presenter != null)
            //                {
            //                    if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
            //                    }
            //                    else
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                DataTableConverter.SetValue(dgInspection.Rows[c1DataGrid.CurrentRow.Index].DataItem, "PASSYN", "N");

            //                if (e.Cell.Presenter != null)
            //                {
            //                    if (e.Cell.Column.Name.Equals("CLCTVAL"))
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                    else
            //                    {
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void setDefectResult()
        {
            try
            {
                if (dsResult.Tables["ACTIVITYREASON"] == null || dsResult.Tables["ACTIVITYREASON"].Rows.Count < 1)
                {
                    return;
                }

                DataTable INDATA = dsResult.Tables["ACTIVITYREASON"].Copy().DefaultView.ToTable(false, new string[] { "RESNCODE" });
                INDATA.TableName = "INDATA";


                DataColumn col = new DataColumn();

                col.DataType = typeof(string);
                col.ColumnName = "LANGID";
                col.DefaultValue = LoginInfo.LANGID;

                INDATA.Columns.Add(col);

                defectResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DFCT_INFO", "RQSTDT", "RSLTDT", INDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량유형코드선택창 각 유형 grid에 들어갈 데이터 선별 함수
        /// <para>
        /// dsResult.Tables["ACTIVITYREASON"], defectResult 조회가 선행되어야 함
        /// </para>
        /// <param name="pre">이전까지 선택된 모든 code string[].</param>
        /// </summary>
        private DataTable getNextRESNCODE(string[] pre)
        {
            if (pre == null) pre = new string[0];

            if (defectResult != null)
            {
                if (defectResult.Rows.Count > 0)
                {
                    DataTable dt = defectResult.Copy();
                    SortedSet<String> set = new SortedSet<String>();

                    int k = -1;
                    for (int i = 0; i < pre.Length; i++) {
                        DataView dv = dt.DefaultView;
                        dv.RowFilter = dt.Columns[i * 2].ColumnName + " = '" + pre[i] + "' ";
                        dt = dv.ToTable();
                        k = i;
                    }

                    k = (k + 1) * 2;
                    if (dt.Columns.Count <= k + 1)
                    {
                        return null;
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        set.Add(Convert.ToString(dr[k]) + ":" + Convert.ToString(dr[k + 1]));
                    }

                    DataTable resdt = new DataTable();
                    resdt.Columns.Add("RESNCODE", typeof(string));

                    foreach (String val in set)
                    {
                        resdt.Rows.Add(val);
                    }

                    return resdt;
                }
            }

            return null;
        }
    }
}
