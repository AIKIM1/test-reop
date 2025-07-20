/*************************************************************************************
 Created Date : 2022.09.14
      Creator : Initial Created 
   Decription : [폐기 대기 처리]
                '실물 폐기 처리(Scrap)'를 위해 '폐기 대기 처리'해야 할 Cell이 많은 경우
                'CELL 단위 실물 폐기 처리' 화면에서 이 팝업 사용.
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.14  최도훈 : Initial Created
  2022.09.22  최도훈 : 폐기대기처리 성공시 컬럼 초기화 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_113_SCRAP_STANDBY_PROC : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

        private string sCellId = string.Empty;
        private string sCheckList = string.Empty;

        // 20231213일 불량코드 COMBO추가
        // ReCheck  = DFCT_TYPE_CODE = "A" / 폐기대기 = DFCT_TYPE_CODE = "B";
        private string DFCT_TYPE_CODE = "B"; //

        public string CellId
        {
            get { return sCellId; }
        }

        public FCS002_113_SCRAP_STANDBY_PROC()
        {
            InitializeComponent();
            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }        
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitCombo();
        }
        private void InitCombo()
        {

            // 공통코드에서 가져오기
            // string[] sFilter1 = { "FORM_DFCT_GR_TYPE_CODE" };
            //_combo.SetCombo(cboDfctGrTypeCode, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilter1);

            //string[] sFilter2 = { "FORM_DFCT_TYPE_CODE" };
            //_combo.SetCombo(cboDefectKind, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilter2);

            //--------------------------------------------------------------------------------
            // 20231213일 불량코드 COMBO추가
            //--------------------------------------------------------------------------------
            string[] sFilter1 = { "FORM_DFCT_GR_TYPE_CODE", "FORM_DFCT_GR_TYPE_CODE_MB", "E,C" };
            _combo.SetCombo(cboDfctGrTypeCode, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "DFCT_GR_TYPE_CODE_LV1", sFilter: sFilter1);
            //--------------------------------------------------------------------------------
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                sCellId = Util.NVC(tmps[0]);
                sCheckList = Util.NVC(tmps[1]);
            }
        }
                
        private void cboDfctGrTypeCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] sFilter2 = { "FORM_DFCT_ITEM_CODE", DFCT_TYPE_CODE, Util.NVC(cboDfctGrTypeCode.SelectedValue), "" };
            _combo.SetCombo(cboDefectKind, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "DFCT_GR_TYPE_CODE_LV2", sFilter: sFilter2);
        }

        private void cboDefectKind_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (_combo != null)
            {

                string sDfctGrpTypeCode = Util.NVC(cboDfctGrTypeCode.SelectedValue);
                string sDfctTypeCode = Util.NVC(cboDefectKind.SelectedValue);

                DataTable dtTemp = null;
                //DataTable dt = SetDfctCode(sDfctGrpTypeCode, sDfctTypeCode,);

                //--------------------------------------------------------------------------------
                // 20231213일 불량코드 COMBO추가
                //--------------------------------------------------------------------------------

                cboDefectId.Text = string.Empty;

                DataTable dt = SetDfctCode(sDfctGrpTypeCode, sDfctTypeCode, DFCT_TYPE_CODE);
                //--------------------------------------------------------------------------------

                dtTemp = dt.Copy();

                cboDefectId.ItemsSource = DataTableConverter.Convert(dt);
                cboDefectId.DisplayMemberPath = "CBO_NAME";
                cboDefectId.SelectedValuePath = "CBO_CODE";
                cboDefectId.ItemsSource = dtTemp.AsDataView();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                // %1 (을)를 하시겠습니까?
                Util.MessageConfirm("SFU4329", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveScrapSubLotProcess("N");
                    }
                }, ObjectDic.Instance.GetObjectName("SCRAP_STANDBY_PROC"));
            }
            catch (Exception ex)
            {
                throw ex;
            }                        
        }

        #endregion

        #region Mehod

        // 20231213일 불량코드 COMBO추가
        private DataTable SetDfctCode(string sDfctGrpTypeCode, string sDfctItemCode, string sDfctTypeCode)
        {
            try
            {
                DataTable RSLTDT = new DataTable();
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_ITEM_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DFCT_GR_TYPE_CODE"] = sDfctGrpTypeCode;
                dr["DFCT_ITEM_CODE"] = sDfctItemCode;
                dr["DFCT_TYPE_CODE"] = sDfctTypeCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_FORM_DFCT_CODE_MB", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SaveScrapSubLotProcess(string sLotDetlTypeCode)
        {
            try
            {
                string[] s = sCellId.Split('|');
                string[] isCheck = sCheckList.Split('|');

                if (s.Length == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }


                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("REMARKS_CNTT", typeof(string));
                inTable.Columns.Add("MENUID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(sLotDetlTypeCode);
                newRow["REMARKS_CNTT"] = Util.NVC(txtResnNoteSubLot.Text);
                newRow["MENUID"] = LoginInfo.CFG_MENUID;
                inTable.Rows.Add(newRow);

                DataTable inSubLot = inDataSet.Tables.Add("IN_SUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));
                inSubLot.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                inSubLot.Columns.Add("DFCT_ITEM_CODE", typeof(string));
                inSubLot.Columns.Add("DFCT_CODE", typeof(string));

                for (int i = 0; i < s.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(s[i])) break;

                    if (isCheck[i].Equals("True"))
                    {
                        DataRow newRowSubLot = inSubLot.NewRow();
                        newRowSubLot["SUBLOTID"] = Util.NVC(s[i]);
                        newRowSubLot["DFCT_GR_TYPE_CODE"] = Util.NVC(cboDfctGrTypeCode.SelectedValue);
                        newRowSubLot["DFCT_ITEM_CODE"] = Util.NVC(cboDefectKind.SelectedValue);
                        newRowSubLot["DFCT_CODE"] = Util.NVC(cboDefectId.SelectedValue);
                        inSubLot.Rows.Add(newRowSubLot);
                    }
                }                

                ShowLoadingIndicator();
                //new ClientProxy().ExecuteService_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE", "INDATA,IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
                
                // 20231213일 불량코드 COMBO추가
                new ClientProxy().ExecuteService_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE_MB", "INDATA,IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        if(bizResult.Tables[0].Rows[0]["RETVAL"].ToString() == "0")
                        {
                            Util.MessageValidation("FM_ME_0432");   // 폐기대기 처리 되었습니다.
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        }
                        else
                        {
                            Util.MessageValidation("SFU1497");      // 데이터 처리 중 오류가 발생했습니다
                        }

                        HiddenLoadingIndicator();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
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
