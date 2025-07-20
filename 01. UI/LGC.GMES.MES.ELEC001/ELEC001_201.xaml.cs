/*************************************************************************************
 Created Date : 2019.09.09
      Creator : 오화백
   Decription : 믹서 작업일지 이력 화면 
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.09  DEVELOPER : Initial Created.
  2022.10.11  정재홍    : [C20220919-000515] - 특이사항 Tab 항목에 공정진척 특이사항 추가
 
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
using System.Linq;
using C1.WPF.DataGrid;
using System.Globalization;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_201 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private bool _isLoaded;
        private BizDataSet _Biz = new BizDataSet();
        public ELEC001_201()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            GetArea();
    
        }

        /// <summary>
        /// 자주검사 항목콤보 조회
        /// </summary>
        /// <returns></returns>
        public void ItemComb()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.MIXING;
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboSelfInsp.DisplayMemberPath = "CBO_NAME";
                cboSelfInsp.SelectedValuePath = "CBO_CODE";
                cboSelfInsp.ItemsSource = DataTableConverter.Convert(dtResult);

                cboSelfInsp.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            _isLoaded = true;
        }


        #endregion

        #region Event

        #region 조회 - 버튼 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        /// <summary>
        /// loadingIndicator 보이기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

        }
        #endregion

        #region 조회 - Tab : btnSearch_PreviewMouseDown()
        private void TcDataCollect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded)
            { GetList(); }

        }
        #endregion

        #region 설비콤보 선택시 자주검사 항목콤보 조회 : cboEquipment_SelectedValueChanged()
        /// <summary>
        /// 설비콤보선택시 자주검사 항목조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (_isLoaded == true)
                ItemComb();
        }

        #endregion

        #region 자주검사 항목콤보 선택시 자주검사 재조회 : cboSelfInsp_SelectedValueChanged()
        private void cboSelfInsp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(_isLoaded == false)
             GetSelfInsp();
        }

        #endregion

        #region 차수 수량여부, 1~9까지 체크 
        /// <summary>
        /// 원재료 계량 차수 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrmNum_MtrlGrd_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {

                if (!Util.CheckDecimal(txtFrmNum_MtrlGrd.Text, 0))
                {
                    txtFrmNum_MtrlGrd.Text = string.Empty;
                    txtFrmNum_MtrlGrd.Focus();
                    return;
                }
                if (txtFrmNum_MtrlGrd.Text.Length != 1)
                {
                    txtFrmNum_MtrlGrd.Text = string.Empty;
                    txtFrmNum_MtrlGrd.Focus();
                    return;
                }
                if (Convert.ToInt16(txtFrmNum_MtrlGrd.Text) == 0)
                {
                    txtFrmNum_MtrlGrd.Text = string.Empty;
                    txtFrmNum_MtrlGrd.Focus();
                    return;
                }
  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 원재료 계량 차수1 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrmNum_MtrlGrd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMtrlGrd();
            }
        }
        /// <summary>
        /// 원재료 계량 차수 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtToNum_MtrlGrd_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtToNum_MtrlGrd.Text, 0))
                {
                    txtToNum_MtrlGrd.Text = string.Empty;
                    txtToNum_MtrlGrd.Focus();
                    return;
                }
                if (txtToNum_MtrlGrd.Text.Length != 1)
                {
                    txtToNum_MtrlGrd.Text = string.Empty;
                    txtToNum_MtrlGrd.Focus();
                    return;
                }
                if (Convert.ToInt16(txtToNum_MtrlGrd.Text) == 0)
                {
                    txtToNum_MtrlGrd.Text = string.Empty;
                    txtToNum_MtrlGrd.Focus();
                    return;
                }
               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        ///  원재료 계량 차수 2 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtToNum_MtrlGrd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMtrlGrd();
            }
        }
        /// <summary>
        /// 믹스 차수 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrmNum_Mix_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtFrmNum_Mix.Text, 0))
                {
                    txtFrmNum_Mix.Text = string.Empty;
                    txtFrmNum_Mix.Focus();
                    return;
                }
                if (txtFrmNum_Mix.Text.Length != 1)
                {
                    txtFrmNum_Mix.Text = string.Empty;
                    txtFrmNum_Mix.Focus();
                    return;
                }
                if (Convert.ToInt16(txtFrmNum_Mix.Text) == 0)
                {
                    txtFrmNum_Mix.Text = string.Empty;
                    txtFrmNum_Mix.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 믹스 차수 1 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrmNum_Mix_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMix();
            }
        }
        /// <summary>
        /// 믹스 차수 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtToNum_Mix_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtToNum_Mix.Text, 0))
                {
                    txtToNum_Mix.Text = string.Empty;
                    txtToNum_Mix.Focus();
                    return;
                }
                if (txtToNum_Mix.Text.Length != 1)
                {
                    txtToNum_Mix.Text = string.Empty;
                    txtToNum_Mix.Focus();
                    return;
                }
                if (Convert.ToInt16(txtToNum_Mix.Text) == 0)
                {
                    txtToNum_Mix.Text = string.Empty;
                    txtToNum_Mix.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 믹스 차수 2 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtToNum_Mix_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMix();
            }
        }
        /// <summary>
        /// 체크스케일 차수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrmNum_ChkScale_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtFrmNum_ChkScale.Text, 0))
                {
                    txtFrmNum_ChkScale.Text = string.Empty;
                    txtFrmNum_ChkScale.Focus();
                    return;
                }
                if (txtFrmNum_ChkScale.Text.Length != 1)
                {
                    txtFrmNum_ChkScale.Text = string.Empty;
                    txtFrmNum_ChkScale.Focus();
                    return;
                }
                if (Convert.ToInt16(txtFrmNum_ChkScale.Text) == 0)
                {
                    txtFrmNum_ChkScale.Text = string.Empty;
                    txtFrmNum_ChkScale.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 체크스케일 차수 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrmNum_ChkScale_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetChkScale();
            }
        }
        /// <summary>
        /// 체크스케일Out 차수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrmNum_ScaleOut_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtFrmNum_ScaleOut.Text, 0))
                {
                    txtFrmNum_ScaleOut.Text = string.Empty;
                    txtFrmNum_ScaleOut.Focus();
                    return;
                }
                if (txtFrmNum_ScaleOut.Text.Length != 1)
                {
                    txtFrmNum_ScaleOut.Text = string.Empty;
                    txtFrmNum_ScaleOut.Focus();
                    return;
                }
                if (Convert.ToInt16(txtFrmNum_ScaleOut.Text) == 0)
                {
                    txtFrmNum_ScaleOut.Text = string.Empty;
                    txtFrmNum_ScaleOut.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 체크스케일Out 차수 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFrmNum_ScaleOut_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetChkScaleOut();
            }
        }
        #endregion

        #region 동정보 콤보선택 : cboArea_SelectedValueChanged()
        /// <summary>
        /// 동정보선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetEquipmentSegment();
        }

        #endregion

        #region 라인정보 콤보선택 :  cboEquipmentSegment_SelectedValueChanged()
        /// <summary>
        /// 라인정보선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetProcess();
        }


        #endregion

        #endregion

        #region Mehod

        #region 조회 - Tab 선택에 따른  : GetList()
        /// <summary>
        /// 조회
        /// </summary>
        public void GetList()
        {
            try
            {
                //동을 선택하세요.
                if(cboArea.SelectedValue.ToString() == string.Empty)
                {
                    Util.MessageValidation("SFU1499");
                    return;
                }
                //라인을 선택하세요.
                if (cboEquipmentSegment.SelectedValue.ToString() == string.Empty)
                {
                    Util.MessageValidation("SFU1223");
                    return;
                }
                //공정을 선택하세요.
                if (cboProcess.SelectedValue.ToString() == string.Empty)
                {
                    Util.MessageValidation("SFU1459");
                    return;
                }
                //설비를 선택하세요.
               if (cboEquipment.SelectedValue.ToString() == string.Empty)
                {
                    Util.MessageValidation("SFU1153");
                    return;
                }
                ///배치
                if (c1tabBatch.IsSelected.Equals(true))
                {
                    GetBatchInfo();
                }
                //원재료계량
                if (c1tabMtrlGrd.IsSelected.Equals(true))
                {
                    GetMtrlGrd();
                }
                //믹싱
                if (c1tabMix.IsSelected.Equals(true))
                {
                    GetMix();
                }
                //체크스케일
                if (c1tabChkScale.IsSelected.Equals(true))
                {
                    GetChkScale();
                }
                //체크스케일배출
                if (c1tabChkScaleOut.IsSelected.Equals(true))
                {
                    GetChkScaleOut();
                }
                //자재투입량
                if (c1tabInputQty.IsSelected.Equals(true))
                {
                    GetInputQty();
                }
                //자주검사
                if (c1tabSelfInsp.IsSelected.Equals(true))
                {
                    ItemComb();
                    GetSelfInsp();
                }
                //Slurry 이송
                if (c1tabSlurry.IsSelected.Equals(true))
                {
                    GetSlurry();
                }
                //특이사항
                if (c1tabRemark.IsSelected.Equals(true))
                {
                    GetRemark();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - Batch  : GetBatchInfo()
        /// <summary>
        /// 배치
        /// </summary>
        public void GetBatchInfo()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));
                dtRqst.Columns.Add("MIX_SEQS", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));           

                DataRow dr = dtRqst.NewRow();
                dr["RPT_ITEM"] = "ADD_INFO";
                dr["MIX_SEQS"] = "BATCH_COUNT"; // MES 2.0 - 주석처리부분을 활성화 (이상권 책임 요청)
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);

                if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    dgDocument.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                else
                    dgDocument.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgDocument);
                Util.GridSetData(dgDocument, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - 원재료계량 : GetMtrlGrd()
        /// <summary>
        /// 원재료계량
        /// </summary>
        public void GetMtrlGrd()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));
                dtRqst.Columns.Add("MIX_SEQS", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["RPT_ITEM"] = "MTRL_MEASR";
                if (txtFrmNum_MtrlGrd.Text != string.Empty || txtToNum_MtrlGrd.Text != string.Empty)
                {
                    dr["MIX_SEQS"] = txtFrmNum_MtrlGrd.Text + "-" + txtToNum_MtrlGrd.Text;
                }
                else
                {
                    dr["MIX_SEQS"] = null;
                }
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);

                if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    dgMtrlGrd.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                else
                    dgMtrlGrd.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgMtrlGrd);
                Util.GridSetData(dgMtrlGrd, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - 믹스 : GetMix()
        /// <summary>
        /// 믹스
        /// </summary>
        public void GetMix()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));
                dtRqst.Columns.Add("MIX_SEQS", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["RPT_ITEM"] = "MIXING";
                if (txtFrmNum_Mix.Text != string.Empty || txtToNum_Mix.Text != string.Empty)
                {
                    dr["MIX_SEQS"] = txtFrmNum_Mix.Text + "-" + txtToNum_Mix.Text;
                }
                else
                {
                    dr["MIX_SEQS"] = null;
                }
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);

                if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    dgMix.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                else
                    dgMix.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgMix);
                Util.GridSetData(dgMix, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - 체크스케일 : GetChkScale()
        /// <summary>
        /// 체크스케일
        /// </summary>
        public void GetChkScale()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));
                dtRqst.Columns.Add("MIX_SEQS", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["RPT_ITEM"] = "CHECK_SCALE";
                if (txtFrmNum_ChkScale.Text != string.Empty)
                {
                    dr["MIX_SEQS"] = txtFrmNum_ChkScale.Text;
                }
                else
                {
                    dr["MIX_SEQS"] = null;
                }
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);

                if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    dgChkScale.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                else
                    dgChkScale.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgChkScale);
                Util.GridSetData(dgChkScale, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - 체크스케일배출 : GetChkScaleOut()
        /// <summary>
        /// 체크스케일배출
        /// </summary>
        public void GetChkScaleOut()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));
                dtRqst.Columns.Add("MIX_SEQS", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["RPT_ITEM"] = "CHECK_SCALE_OUT";
                if (txtFrmNum_ChkScale.Text != string.Empty)
                {
                    dr["MIX_SEQS"] = txtFrmNum_ChkScale.Text;
                }
                else
                {
                    dr["MIX_SEQS"] = null;
                }
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);
                if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    dgChkScaleOut.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                else
                    dgChkScaleOut.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgChkScaleOut);
                Util.GridSetData(dgChkScaleOut, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - 자재투입량 : GetInputQty()
        /// <summary>
        /// 자재투입량
        /// </summary>
        public void GetInputQty()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_ALL_INPUT_QTY_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);

                if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    dgInputQty.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                else
                    dgInputQty.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgInputQty);
                Util.GridSetData(dgInputQty, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - 자주검사 : GetSelfInsp()
        /// <summary>
        /// 자주검사
        /// </summary>
        public void GetSelfInsp()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_SELF_INSP_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ITEM", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["ITEM"] = cboSelfInsp.SelectedValue.ToString() == string.Empty ? null : cboSelfInsp.SelectedValue.ToString();
                dtRqst.Rows.Add(dr);

                if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    dgSelfInsp.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                else
                    dgSelfInsp.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgSelfInsp);
                Util.GridSetData(dgSelfInsp, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - Slurry 이송 : GetSlurry()
        /// <summary>
        /// Slurry 이송
        /// </summary>
        public void GetSlurry()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));
                dtRqst.Columns.Add("MIX_SEQS", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["RPT_ITEM"] = "SLURRY_MOVE";
                dr["MIX_SEQS"] = null;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);

                if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                    dgSlurry.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
                else
                    dgSlurry.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                Util.gridClear(dgSlurry);
                Util.GridSetData(dgSlurry, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 조회 - 특이사항 : GetRemark()
        /// <summary>
        /// 특이사항
        /// </summary>
        public void GetRemark()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = "DA_PRD_SEL_MIX_WRK_RPT_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("RPT_ITEM", typeof(string));
                dtRqst.Columns.Add("MIX_SEQS", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["RPT_ITEM"] = "ADD_INFO";
                dr["MIX_SEQS"] = "REMARK";
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499");    //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");    //라인정보를 선택하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");    //공정을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153");    //설비를 선택하세요.
                dr["PRODID"] = txtProdId.Text;
                dr["PJT"] = txtPjt.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                Util.gridClear(dgRemark);
                Util.GridSetData(dgRemark, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 로딩 설정 : ShowLoadingIndicator(), HiddenLoadingIndicator()

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
        
        #region 콤보박스 동정보 조회 : GetArea()
        /// <summary>
        /// 동 정보 가져오기
        /// </summary>
        public void GetArea()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT, (result2, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.MessageException(Exception);
                        return;
                    }
                    DataRow dRow = result2.NewRow();
                    dRow["CBO_NAME"] = "-SELECT-";
                    dRow["CBO_CODE"] = "";
                    result2.Rows.InsertAt(dRow, 0);

                    cboArea.ItemsSource = DataTableConverter.Convert(result2);

                    if ((from DataRow drArea in result2.Rows where drArea["CBO_CODE"].Equals(LoginInfo.CFG_AREA_ID) select drArea).Count() > 0)
                    {
                        cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
                    }
                    else if (result2.Rows.Count > 0)
                    {
                        cboArea.SelectedIndex = 0;
                    }
                    else if (result2.Rows.Count == 0)
                    {
                        cboArea.SelectedItem = null;
                    }

                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 콤보박스 라인정보 조회 : GetEquipmentSegment()
        /// <summary>
        /// 라인 정보 가져오기
        /// </summary>
        public void GetEquipmentSegment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT, (result2, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.MessageException(Exception);
                        return;
                    }
                    DataRow dRow = result2.NewRow();
                    dRow["CBO_NAME"] = "-SELECT-";
                    dRow["CBO_CODE"] = "";
                    result2.Rows.InsertAt(dRow, 0);

                    cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(result2);

                    if ((from DataRow drEquipmentSegment in result2.Rows where drEquipmentSegment["CBO_CODE"].Equals(LoginInfo.CFG_EQSG_ID) select drEquipmentSegment).Count() > 0)
                    {
                        cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    }
                    else if (result2.Rows.Count > 0)
                    {
                        cboEquipmentSegment.SelectedIndex = 0;
                    }
                    else if (result2.Rows.Count == 0)
                    {
                        cboEquipmentSegment.SelectedItem = null;
                    }
                    GetProcess();
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 콤보박스 공정정보 조회 : GetProcess()
        /// <summary>
        /// 공정정보 조회
        /// </summary>
        public void GetProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow drRow = RQSTDT.NewRow();
                drRow["LANGID"] = LoginInfo.LANGID;
                drRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                RQSTDT.Rows.Add(drRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_CWA_MIXING", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";
                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);


                if ((from DataRow dr in dtResult.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQPT_ID) select dr).Count() > 0)
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
                }
                else if (dtResult.Rows.Count > 0)
                {
                    cboEquipment.SelectedIndex = 0;
                }
                else if (dtResult.Rows.Count == 0)
                {
                    cboEquipment.SelectedItem = null;
                }

                cboProcess.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 콤보박스 설비정보 조회 : GetEquipment()
        /// <summary>
        /// 설비정보 조회
        /// </summary>
        public void GetEquipment()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                drnewrow["PROCID"] = cboProcess.SelectedValue;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.MessageException(Exception);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    dRow["CBO_NAME"] = "-SELECT-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cboEquipment.ItemsSource = DataTableConverter.Convert(result);

                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(LoginInfo.CFG_EQPT_ID) select dr).Count() > 0)
                    {
                        cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cboEquipment.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cboEquipment.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }
        #endregion

        #endregion

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetEquipment();
        }
    }
}
