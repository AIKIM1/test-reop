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
  2022.12.14  조영대 : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2023.04.11  이정미 : 폐기대기 100건씩 처리하도록 수정 
  2024.01.12  최도훈 : '불량 그룹 유형'에 해당하는 '불량그룹'만 조회되도록 수정
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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_113_SCRAP_STANDBY_PROC : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sCellId = string.Empty;
        private string sCheckList = string.Empty;
        private string sCellCount = string.Empty;

        public string CellId
        {
            get { return sCellId; }
        }

        public FCS001_113_SCRAP_STANDBY_PROC()
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
            CommonCombo_Form _combo = new CommonCombo_Form();

             string[] sFilter1 = { "FORM_DFCT_GR_TYPE_CODE" };
            _combo.SetCombo(cboDfctGrTypeCode, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilter1);
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
                sCellCount = Util.NVC(tmps[2]);
            }
        }
                
        private void cboDefectInfo_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (_combo != null)
            {

                string sDfctGrpTypeCode = Util.NVC(cboDfctGrTypeCode.SelectedValue);

                DataTable dtTemp = null;
                DataTable dt = SetDfctKind(sDfctGrpTypeCode);
                dtTemp = dt.Copy();

                cboDefectKind.ItemsSource = DataTableConverter.Convert(dt);
                cboDefectKind.DisplayMemberPath = "CBO_NAME";
                cboDefectKind.ItemsSource = dtTemp.AsDataView();
            }                                  
        }

        private void cboDefectKind_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (_combo != null)
            {

                string sDfctGrpTypeCode = Util.NVC(cboDfctGrTypeCode.SelectedValue);
                string sDfctTypeCode = Util.NVC(cboDefectKind.SelectedValue.GetValue("CBO_CODE"));

                DataTable dtTemp = null;
                DataTable dt = SetDfctCode(sDfctGrpTypeCode, sDfctTypeCode);
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
        private DataTable SetDfctCode(string sDfctGrpTypeCode, string sDfctTypeCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DFCT_GR_TYPE_CODE"] = sDfctGrpTypeCode;
                dr["DFCT_TYPE_CODE"] = sDfctTypeCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_FORM_DFCT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable SetDfctKind(string sDfctGrpTypeCode)
        {
            DataTable RSLTDT = new DataTable();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USE_FLAG"] = "Y";
                dr["DFCT_GR_TYPE_CODE"] = sDfctGrpTypeCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_DEFECT_KIND", "RQSTDT", "RSLTDT", RQSTDT);

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
                string[] s = sCellId.Split( new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string[] isCheck = sCheckList.Split(new Char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                int iProcessingCnt = 100;
                double dNumberOfProcessingCnt = 0.0;
                bool bIsOK = true;

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
                inTable.Columns.Add("USER_IP", typeof(string));
                inTable.Columns.Add("PC_NAME", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(sLotDetlTypeCode);
                newRow["REMARKS_CNTT"] = Util.NVC(txtResnNoteSubLot.Text);
                newRow["MENUID"] = LoginInfo.CFG_MENUID;
                newRow["USER_IP"] = LoginInfo.USER_IP;
                newRow["PC_NAME"] = LoginInfo.PC_NAME;
                inTable.Rows.Add(newRow);

                DataTable inSubLot = inDataSet.Tables.Add("IN_SUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));
                inSubLot.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                inSubLot.Columns.Add("DFCT_CODE", typeof(string));

                ShowLoadingIndicator();
                dNumberOfProcessingCnt = Math.Ceiling(Convert.ToInt32(sCellCount) / Convert.ToDouble(iProcessingCnt));//처리수량

                for (int i = 0; i < dNumberOfProcessingCnt; i++)
                {
                    inSubLot.Clear();
                    
                    for (int k = (i * Convert.ToInt32(iProcessingCnt)); k < (i * iProcessingCnt + iProcessingCnt); k++)
                    {
                        if (k >= Convert.ToInt32(sCellCount)) break;

                        if (isCheck.Length > k)
                        {
                            if (isCheck[k].Equals("True"))
                            {
                                DataRow RowCell = inSubLot.NewRow();
                                RowCell["SUBLOTID"] = Util.NVC(s[k]);
                                RowCell["DFCT_GR_TYPE_CODE"] = Util.NVC(cboDfctGrTypeCode.SelectedValue);
                                RowCell["DFCT_CODE"] = Util.NVC(cboDefectId.SelectedValue);
                                inSubLot.Rows.Add(RowCell);
                            }
                        }

                        else
                        {
                            continue;
                        } 
                    }

                    try
                    {
                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE", "INDATA,INCELL", "OUTDATA", inDataSet);
                    }

                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        Util.MessageValidation("SFU9200", (i * (int)iProcessingCnt).ToString("#,##0"), (i * iProcessingCnt + iProcessingCnt - 1).ToString("#,##0"));
                        bIsOK = false;
                        return;
                    }

                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                }
                if (bIsOK)
                {
                    Util.MessageValidation("FM_ME_0432");   // 폐기대기 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }

                else
                {
                    Util.MessageValidation("SFU1497");      // 데이터 처리 중 오류가 발생했습니다
                }
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
