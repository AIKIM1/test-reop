/*************************************************************************************
 Created Date : 2017.05.22
      Creator : 이영준S
   Decription : 전지 5MEGA-GMES 구축 - 1차 포장 구성 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2023.05.11  이병윤 : E20230425-000509 라벨타입 선택 개선_popShipto_ValueChanged 수정 
 2023.12.05  이병윤 : E20231122-000976 투입파렛트 추가시 동일한 LOW_SOC_FLAG 체크로직 추가
                                       작업시작에 LOW_SOC_FLAG 컬럼 추가 
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

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_204_RUNSTART : C1Window, IWorkArea
    {
        Util _util = new Util();
        
        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;
        string sPGMID  = string.Empty;
        public string AREAID
        {
            get;
            set;
        }
        public string PALLETID
        {
            get;
            set;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_204_RUNSTART()
        {
            InitializeComponent();
        }

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _AREAID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            sSHFTID = Util.NVC(tmps[2]);
            // E20230425-000509 : 파라메터 추가.
            sPGMID = Util.NVC(tmps[3]);

            InitCombo();
            InitControl();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
          //_combo.SetCombo(cboShipTo, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, null, null }, sCase: "SHIPTO_CP");
            _combo.SetCombo(cboLabelType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //if (cboShipTo.SelectedValue.Equals("SELECT"))
            //{
            //    //	SFU4096	출하처를 선택하세요.	
            //    Util.MessageValidation("SFU4096");
            //    return;
            //}
            if (string.IsNullOrEmpty(Util.NVC(popShipto.SelectedValue)))
            {
                //	SFU4096	출하처를 선택하세요.	
                Util.MessageValidation("SFU4096");
                return;
            }

            if (string.IsNullOrWhiteSpace((string)cboLabelType.SelectedValue) || (string)cboLabelType.SelectedValue == "SELECT")
            {
                //SFU1522 라벨 타입을 선택하세요.	
                Util.MessageValidation("SFU1522");
                return;
            }

            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunStart();
                }
            });
        }
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void RunStart()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SHFT_ID");
                inDataTable.Columns.Add("SHIPTO_ID");
                inDataTable.Columns.Add("LABEL_ID");
                inDataTable.Columns.Add("FIFO_FLAG");
                inDataTable.Columns.Add("LOW_SOC_FLAG");

                DataTable inBoxTable = inDataSet.Tables.Add("INPALLET");
                inBoxTable.Columns.Add("BOXID");

                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = sUSERID;
                newRow["SHFT_ID"] = sSHFTID;
                newRow["SHIPTO_ID"] = popShipto.SelectedValue;
                newRow["LABEL_ID"] = cboLabelType.SelectedValue.ToString();
                newRow["FIFO_FLAG"] = chkFIFO.IsChecked == true ? "Y" : "N";
                newRow["LOW_SOC_FLAG"] = Util.NVC(dgInPallet.GetCell(0, dgInPallet.Columns["LOW_SOC_FLAG"].Index).Value);

                inDataTable.Rows.Add(newRow);
                newRow = null;


                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = Util.NVC(dgInPallet.GetCell(i, dgInPallet.Columns["BOXID"].Index).Value);
                    inBoxTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_2ND_PALLET_NJ", "INDATA,INPALLET", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        AREAID = (string)searchResult.Tables["OUTDATA"].Rows[0]["AREAID"];
                        PALLETID = (string)searchResult.Tables["OUTDATA"].Rows[0]["PALLETID"];

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet
                               );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        private void txtInPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("BOXID");
                    dt.Columns.Add("LANGID");
                    DataRow dr = dt.NewRow();
                    dr["BOXID"] = txtInPalletID.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dt.Rows.Add(dr);                    

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPUT_1ST_PALLET_LIST_NJ", "INDATA", "OUTDATA", dt);

                    if (!dtResult.Columns.Contains("CHK"))
                        dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                    if (dgInPallet.Rows.Count == 1)
                    {
                        txtProdID.Text = dtResult.Rows[0]["PRODID"].ToString();
                        txtProject.Text = dtResult.Rows[0]["PROJECT"].ToString();
                        //   SetShipto(txtProdID.Text);
                        setShipToPopControl(txtProdID.Text.Trim());
                    }

                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgInPallet.ItemsSource);
                        var query = (from t in dtSource.AsEnumerable()
                                     where t.Field<string>("BOXID") == txtInPalletID.Text
                                     select t).Distinct();
                        if (query.Any())
                        {
                            //	SFU1781	이미 추가 된 팔레트 입니다.
                            Util.MessageValidation("SFU1781");
                            return;
                        }
                        if (!dtResult.Rows[0]["PRODID"].ToString().Equals(txtProdID.Text) || !dtResult.Rows[0]["PROJECT"].ToString().Equals(txtProject.Text))
                        {
                            //SFU4338		동일한 제품만 작업 가능합니다.
                            Util.MessageValidation("SFU4338");
                            return;
                        }

                        for(int inx = 0; inx < dtResult.Rows.Count; inx++)
                        {
                            if(dtSource.Rows.Count > 0 && (Util.NVC(dtSource.Rows[0]["AOMM_GRD_CODE"]) != Util.NVC(dtResult.Rows[inx]["AOMM_GRD_CODE"])))
                            {
                                //동일한 AOMM 등급을 선택하세요.
                                Util.MessageValidation("SFU3803");
                                return;
                            }
                            // E20231122-000976 : Low SOC type 체크
                            if (dtSource.Rows.Count > 0 && (Util.NVC(dtSource.Rows[0]["LOW_SOC_FLAG"]) != Util.NVC(dtResult.Rows[inx]["LOW_SOC_FLAG"])))
                            {
                                //Pallet Low SOC type is different
                                Util.MessageValidation("SFU9908");
                                return;
                            }
                        }

                        txtCellQty.Text = (Util.NVC_Int(txtCellQty.Text) + Util.NVC_Int(dtResult.Rows[0]["TOTAL_QTY"])).ToString();
                        txtInboxQty.Text = (Util.NVC_Int(txtInboxQty.Text) + Util.NVC_Int(dtResult.Rows[0]["INBOX_QTY"])).ToString();

                        dtResult.Merge(dtSource);

                        // dgInPallet.ItemsSource = DataTableConverter.Convert(dtResult);
                        Util.GridSetData(dgInPallet, dtResult, FrameOperation, true);
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtInPalletID.Text = string.Empty;
                }
            }
        }

        private void btnInPalletDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgInPallet.ItemsSource);

                foreach (DataRow dr in dt.AsEnumerable().ToList())
                {
                    if (dr["CHK"].Equals(true))
                    {
                        dt.Rows.Remove(dr);
                    }
                }
                dgInPallet.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void SetShipto(String sProdID)
        //{
        //    const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
        //    string[] arrColumn = { "LANGID", "SHOPID", "PRODID" };
        //    string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, sProdID };
        //    string selectedValueText = "CBO_CODE";
        //    string displayMemberText = "CBO_NAME";

        //    CommonCombo.CommonBaseCombo(bizRuleName, cboShipTo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        //}

        private void setShipToPopControl(string prodID)
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID, LoginInfo.LANGID };
            CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
        }

        private void popShipto_ValueChanged(object sender, EventArgs e)
        {
            // E20230425-000509_기존로직 주석처리.
            //DataTable dtRqstDt = new DataTable();
            //dtRqstDt.TableName = "RQSTDT";
            //dtRqstDt.Columns.Add("LANGID", typeof(string));
            //dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));
            //dtRqstDt.Columns.Add("CMCODE", typeof(string));

            //DataRow drnewrow = dtRqstDt.NewRow();
            //drnewrow["LANGID"] = LoginInfo.LANGID;
            //drnewrow["CMCDTYPE"] = "LGE_LABEL_ID_FIXED_FLAG";
            //drnewrow["CMCODE"] = popShipto.SelectedValue; 

            //dtRqstDt.Rows.Add(drnewrow);

            //DataTable dtExist = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqstDt);

            //if (dtExist.Rows.Count == 0)
            //    return;

            //DataTable RQSTDT = new DataTable();
            //RQSTDT.TableName = "RQSTDT";
            //RQSTDT.Columns.Add("LANGID", typeof(string));
            //RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            //DataRow dr = RQSTDT.NewRow();
            //dr["LANGID"] = LoginInfo.LANGID;
            //dr["CMCDTYPE"] = "MOBILE_INBOX_LABEL_TYPE";
            //RQSTDT.Rows.Add(dr);

            //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

            //int idx = 0;

            //for(int i = 0; i < dtResult.Rows.Count; i++)
            //{
            //    if(dtResult.Rows[i]["CBO_CODE"].ToString() == "CB_NORMAL")
            //    {
            //        idx = i;
            //        break;
            //    }
            //}

            //cboLabelType.SelectedIndex = ++idx;

            //E20230425-000509 라벨타입 선택 개선
            // MMD : 출하처 인쇄관리 항목(Cell) 조회
            DataTable dtPrtDt = new DataTable();
            dtPrtDt.TableName = "RQSTDT";
            dtPrtDt.Columns.Add("LANGID", typeof(string));
            dtPrtDt.Columns.Add("PRODID", typeof(string));
            dtPrtDt.Columns.Add("SHOPID", typeof(string));
            dtPrtDt.Columns.Add("SHIPTO_ID", typeof(string));

            DataRow drPrtrow = dtPrtDt.NewRow();
            drPrtrow["LANGID"] = LoginInfo.LANGID;
            drPrtrow["PRODID"] = txtProdID.Text;
            drPrtrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drPrtrow["SHIPTO_ID"] = popShipto.SelectedValue;

            dtPrtDt.Rows.Add(drPrtrow);
            DataTable dtMmdEx = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", "RQSTDT", "RSLTDT", dtPrtDt);

            string attr4 = string.Empty;
            if (dtMmdEx.Rows.Count == 0)
            {
                /*
                 * BOX001_223 : 공정진척(활성화) > 2차 포장 구성(원/각형)
                 * -> MMD:인쇄관리 항목에 등록된 라벨이 없는 경우 Default label: LBL0097 보여준다.
                 * BOX001_204 : Cell포장 > 2차 포장 구성(파우치형)
                 * -> MMD:인쇄관리 항목에 등록된 라벨이 없는 경우 메세지띄우고 작업을 멈춘다.
                 */
                if(sPGMID.Equals("BOX001_223"))
                {
                    attr4 = "LBL0097";
                }
                else
                {
                    // 초기화일 경우 경고메세지 보이지 않게 하기
                    if (!string.IsNullOrEmpty(popShipto.SelectedValue.ToString()))
                    {
                        //SFU9994 : Contact engineers to configure the label
                        Util.MessageValidation("SFU9994");
                    }

                    attr4 = ",";
                }
            }
            else
            {
                for (int i = 0; i < dtMmdEx.Rows.Count; i++)
                {
                    attr4 += dtMmdEx.Rows[i]["CBO_CODE"].ToString() + ",";
                }
            }

            // 라벨타입조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MOBILE_INBOX_LABEL_TYPE";
            dr["ATTRIBUTE4"] = attr4;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO_MULTI", "RQSTDT", "RSLTDT", RQSTDT);

            // SELECT 추가
            DataRow drResult = dtResult.NewRow();
            drResult["CBO_NAME_ONLY"] = "-SELECT-";
            drResult["CBO_CODE"] = "SELECT";
            dtResult.Rows.InsertAt(drResult, 0);
            // 콤보박스 세팅
            cboLabelType.DisplayMemberPath = "CBO_NAME_ONLY";
            cboLabelType.SelectedValuePath = "CBO_CODE";
            cboLabelType.ItemsSource = dtResult.Copy().AsDataView();
            if (dtResult.Rows.Count <= 1)
            {
                cboLabelType.SelectedIndex = 0;
            }
            else
            {
                cboLabelType.SelectedValue = dtResult.Rows[1]["CBO_CODE"].ToString();

                if (cboLabelType.SelectedIndex < 0)
                    cboLabelType.SelectedIndex = 0;
            }
        }
    }
}
