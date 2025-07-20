/*************************************************************************************
 Created Date : 2017.05.22
      Creator : 이영준S
   Decription : 전지 5MEGA-GMES 구축 - 1차 포장 구성 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]

 2019.06.20 이제섭 등외품 포장 프로세스 추가.
 2023.11.09 이병윤 E20230424-000080 : 작업시작 전 INBOX의 동일한 LOTTYPE인지 체크 추가.
                                    : 작업시작 LOTTYPE 추가 PALLET 등록시 대차에 포함된 LOTTYPE과 동일하게 생성하기위함.

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
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_201_RUNSTART : C1Window, IWorkArea
    {
        string _LINE = string.Empty;
        string _USERID = string.Empty;
        string _SHFTID = string.Empty;
        string _EQPTID = string.Empty;

        bool _AommGrdChkFlag = false;

        public string EQSGID
        {
            get;
            set;
        }
        public string EQPTID
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

        public BOX001_201_RUNSTART()
        {
            InitializeComponent();
        }

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            InitControl();
            InitCombo();    
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { Process.CELL_BOXING , "MCP" }, sCase: "EQUIPMENT_BY_LINEGRUP_PROCID");
            setEquipmentCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, _LINE, _EQPTID);
            _combo.SetCombo(cboExpDomType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
            _combo.SetCombo(cboProcType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "B" }, sCase: "PROCBYPCSGID");
            _combo.SetCombo(cboLabelType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
            //  _combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "INBOX_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");       

            if (LoginInfo.SYSID == "GMES-S-NJ")
            {
                cboLabelType.SelectedValue = "NORMAL";
            }

            txtPalletID.Focus();
            txtPalletID.SelectAll();
        }

        private void setEquipmentCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string eqsgID = null, string eqptID = null)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO_NJ";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "EQGRID" };
            string[] arrCondition = { LoginInfo.LANGID, eqsgID, Process.CELL_BOXING, "BOX" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, eqptID);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _LINE = Util.NVC(tmps[0]);            
            _EQPTID = Util.NVC(tmps[1]);       
            _USERID = Util.NVC(tmps[2]);
            _SHFTID = Util.NVC(tmps[3]);
        }
        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtPkgLotID.Text = txtPkgLotID.Text.Trim();

                // 저장데이터 존재여부 체크
                if (Util.NVC(cboProcType.SelectedValue) == "SELECT")
                {
                    // SFU4343     포장 구분을 선택하세요.
                    Util.MessageValidation("SFU4343");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPkgLotID.Text))
                {
                    //SFU4147		조립LOTID를 입력하세요
                    Util.MessageValidation("SFU4147");
                    return;
                }
                
                if (Util.NVC(cboEquipment.SelectedValue) == "SELECT")
                {
                    //SFU1673		설비를 선택하세요.	
                    Util.MessageValidation("SFU1673");
                    return;
                }

                if (Util.NVC(cboLabelType.SelectedValue) == "SELECT")
                {
                    //SFU1522	라벨 타입을 선택하세요	
                    Util.MessageValidation("SFU1522");
                    return;
                }

                if (Util.NVC(cboExpDomType.SelectedValue) == "SELECT")
                {
                    //SFU3606		'수출/내수'를 선택해주세요	
                    Util.MessageValidation("SFU3606");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPRODID.Text))
                {
                    //SFU1895	제품을 선택하세요.	
                    Util.MessageValidation("SFU1895");
                    return;
                }

                if (Util.NVC_Int(txtInputQty.Value) == 0)
                {
                    //SFU1953		투입 수량을 입력하세요.	
                    Util.MessageValidation("SFU1953");
                    return;
                }

                if (txtPkgLotID.Text.Length < 8)
                {
                    //SFU4348	올바른 조립LOTID를 입력하세요.	
                    Util.MessageValidation("SFU4348");
                    return;
                }


                DataRow[] drList = dgResult.ItemsSource == null ? null : DataTableConverter.Convert(dgResult.ItemsSource).Select("CHK = 'True'");

                int iProdWeek = 0;

                string sProdWeek = txtPkgLotID.Text.Substring(5, 2);

                if (string.IsNullOrWhiteSpace(txtOffGrade.Text))
                {
                    if (string.IsNullOrWhiteSpace(txtPrdGrade.Text))
                    {
                        //	SFU4344		특성 등급을 선택하세요.	
                        Util.MessageValidation("SFU4344");
                        return;
                    }

                    if (!int.TryParse(sProdWeek, out iProdWeek))
                    {
                        //SFU4348	올바른 조립LOTID를 입력하세요.	
                        Util.MessageValidation("SFU4348");
                        return;
                    }                    

                    if (chkLotMerge.IsChecked != true
                        && iProdWeek > 40)
                    {
                        //SFU4345	LOT 병합을 체크하세요.	
                        Util.MessageValidation("SFU4345");
                        return;
                    }
                }

                // 대차 투입시
                if (drList != null && drList.Count() > 0)
                {
                    // 동일 LOTTYPE 체크 : E20230424-000080
                    if (drList.Select(c => c.Field<string>("LOTTYPE")).Distinct().ToList().Count() > 1)
                    {
                        //SFU9905	the market type of cart is different
                        Util.MessageValidation("SFU9905");
                        return;
                    }

                    if (drList.Select(c => c.Field<string>("PRODID")).Distinct().ToList().Count() > 1)
                    {
                        //SFU4338	동일한 제품만 작업 가능합니다.
                        Util.MessageValidation("SFU4338");
                        return;
                    }

                    if (drList.Select(c => c.Field<string>("PRODID")).Distinct().ToList().Count() > 1)
                    {
                        //SFU4338	동일한 제품만 작업 가능합니다.
                        Util.MessageValidation("SFU4338");
                        return;
                    }

                    if (drList.Select(c => c.Field<string>("PRODWEEK")).Distinct().ToList().Count() > 1)
                    {
                        //SFU4347	동일한 생산주차의 LOT이 아닙니다.
                        Util.MessageValidation("SFU4338");
                        return;
                    }

                    int iCnt = Util.NVC_Int(drList[0]["PRODWEEK"]); 

                    // LOT병합이 아니면 조립LOT이 동일해야 함.
                    if (chkLotMerge.IsChecked != true
                        && drList.Select(c => c.Field<string>("ASSY_LOTID")).Distinct().ToList().Count() > 1)
                    {
                        //SFU4146 동일한 조립LOT이 아닙니다.	
                        Util.MessageValidation("SFU4146");
                        return;
                    }
                
                    // LOT병합시
                    if(chkLotMerge.IsChecked == true)
                    {
                        // 대차의 생산주차 + 40 과 입력한 lot의 생산주차가 같아야 함.
                        if (iCnt != iProdWeek - 40)
                        {
                            //SFU4348	올바른 조립LOTID를 입력하세요.	
                            Util.MessageValidation("SFU4348");
                            return;
                        }
                    }

                    
                }

                if (rdoOffgrd.IsChecked == true && string.IsNullOrEmpty(popShipto.SelectedValue.ToString()))
                {
                    //	SFU4096	출하처를 선택하세요.	
                    Util.MessageValidation("SFU4096");
                    return;
                }

                //SFU1240		작업시작 하시겠습니까?	
                Util.MessageConfirm("SFU1240", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        RunStart();
                    }
                    else
                        loadingIndicator.Visibility = Visibility.Collapsed;
                });
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
        /// Biz : BR_PRD_REG_INPALLET_NJ
        /// </summary>
        private void RunStart()
        {
            try
            {   
                string sAreaID = LoginInfo.CFG_AREA_ID;
                string sEqsgID = LoginInfo.CFG_EQSG_ID;
                string PackingMode = string.Empty;
                if (rdoGeneral.IsChecked == true && rdoOffgrd.IsChecked == false)
                {
                    PackingMode = "GENERAL";
                }
                else if (rdoGeneral.IsChecked == false && rdoOffgrd.IsChecked == true)
                {
                    PackingMode = "OFFGRD";
                }

                DataSet inDataSet = new DataSet(); 

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SHFT_ID");
                inDataTable.Columns.Add("SHIPTO_ID");

                DataTable inBoxTable = inDataSet.Tables.Add("INPALLET");
                inBoxTable.Columns.Add("PKG_LOTID");
                inBoxTable.Columns.Add("PACK_WRK_TYPE_CODE");
                inBoxTable.Columns.Add("EQPTID");
                inBoxTable.Columns.Add("MOBILE_INBOX_LABEL_TYPE");
                inBoxTable.Columns.Add("PRDT_GRD_CODE");
                inBoxTable.Columns.Add("EXP_DOM_TYPE_CODE");
                inBoxTable.Columns.Add("PRODID");
                inBoxTable.Columns.Add("INPUT_QTY");
                inBoxTable.Columns.Add("PACK_NOTE");
                inBoxTable.Columns.Add("SOC_VALUE");
                inBoxTable.Columns.Add("PACK_LOT_TYPE_CODE");
                inBoxTable.Columns.Add("PROCID");
                inBoxTable.Columns.Add("AREAID");
                inBoxTable.Columns.Add("EQSGID");
                inBoxTable.Columns.Add("INBOX_TYPE_CODE");
                inBoxTable.Columns.Add("CTNR_GRADE_MODIFY");
                inBoxTable.Columns.Add("PACKING_MODE");
                inBoxTable.Columns.Add("AOMM_GRD_CODE");
                inBoxTable.Columns.Add("LOTTYPE");
                inBoxTable.Columns.Add("LOW_SOC_FLAG");

                DataTable inCtnrTable = inDataSet.Tables.Add("INCTNR");
                inCtnrTable.Columns.Add("CTNR_ID");
                inCtnrTable.Columns.Add("ASSY_LOTID");
                inCtnrTable.Columns.Add("PRODID");
                inCtnrTable.Columns.Add("PRDT_GRD_CODE");
                inCtnrTable.Columns.Add("EXP_DOM_TYPE_CODE");

                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = _USERID;
                newRow["SHFT_ID"] = _SHFTID;
                newRow["SHIPTO_ID"] = rdoOffgrd.IsChecked == true ? popShipto.SelectedValue.ToString() : null;
                inDataTable.Rows.Add(newRow);

                string lotType = string.Empty;
                if (dgResult.GetRowCount() > 0)
                {
                    DataRow[] drList = DataTableConverter.Convert(dgResult.ItemsSource).Select("CHK = 'True'");                  

                    foreach (DataRow dr in drList)
                    {
                        if(!string.IsNullOrWhiteSpace((string)dr["AREAID"])) sAreaID =(string)dr["AREAID"];
                        if(!string.IsNullOrWhiteSpace((string)dr["EQSGID"])) sEqsgID = (string)dr["EQSGID"];
                        newRow = null;
                        newRow = inCtnrTable.NewRow();
                        newRow["CTNR_ID"] = dr["CTNR_ID"];
                        newRow["ASSY_LOTID"] = dr["ASSY_LOTID"];
                        newRow["PRODID"] = dr["PRODID"];
                        newRow["PRDT_GRD_CODE"] = dr["PRDT_GRD_CODE"];
                        newRow["EXP_DOM_TYPE_CODE"] = dr["EXP_DOM_TYPE_CODE"];
                        lotType = Util.NVC(dr["LOTTYPE"]);

                        inCtnrTable.Rows.Add(newRow);
                    }
                }

                newRow = null;                
                newRow = inBoxTable.NewRow();
                newRow["PKG_LOTID"] = txtPkgLotID.Text;
              //  newRow["PACK_WRK_TYPE_CODE"] = .SelectedValue;
             //   newRow["INBOX_TYPE_CODE"] = cboInboxType.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["MOBILE_INBOX_LABEL_TYPE"] = cboLabelType.SelectedValue;
                newRow["PRDT_GRD_CODE"] = txtPrdGrade.Text;
                newRow["EXP_DOM_TYPE_CODE"] = cboExpDomType.SelectedValue;
                newRow["PRODID"] = txtPRODID.Text;
                newRow["INPUT_QTY"] = txtInputQty.Value;
                newRow["PACK_NOTE"] = new System.Windows.Documents.TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd).Text;
                newRow["SOC_VALUE"] = txtSoc.Text;
                newRow["PACK_LOT_TYPE_CODE"] = chkLotMerge.IsChecked == true ? "LOT_MERGE" : "LOT";
                newRow["PROCID"] = cboProcType.SelectedValue;
                newRow["AREAID"] = sAreaID; // 대차 동 정보 없으면 로그인 동
                newRow["EQSGID"] = sEqsgID; // 대차 라인 정보 없으면 로그인 동    
                newRow["CTNR_GRADE_MODIFY"] = chkGrade.IsChecked == true ? "Y" : "N";
                newRow["PACKING_MODE"] = PackingMode;
                newRow["AOMM_GRD_CODE"] = _AommGrdChkFlag == true ? txtAommGrade.Text : "";
                newRow["LOTTYPE"] = lotType;
                newRow["LOW_SOC_FLAG"] = rdoLowSocN.IsChecked == true ? "N" : rdoLowSocY.IsChecked == true ? "Y" : "N";


                inBoxTable.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPALLET_NJ", "INDATA,INPALLET,INCTNR", "OUTPALLET", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        EQSGID = (string) searchResult.Tables["OUTPALLET"].Rows[0]["EQSGID"];
                        EQPTID = (string)searchResult.Tables["OUTPALLET"].Rows[0]["EQPTID"];
                        PALLETID = (string)searchResult.Tables["OUTPALLET"].Rows[0]["BOXID"];

                        //SFU1275 정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");   

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
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Collapsed;
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnPRODID_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_BOX_PROD popup = new CMM001.Popup.CMM_BOX_PROD();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = txtPRODID.Text;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puProduct_Closed);

                //grdMain.Children.Add(popup);
                //popup.BringToFront();
                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void puProduct_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_BOX_PROD popup = sender as CMM001.Popup.CMM_BOX_PROD;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtPRODID.Text = popup.PRODID;
              //  btnPRODID.Tag = popup.PRODID;
            }
            //this.grdMain.Children.Remove(popup);
        }     

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtPalletID.Text = string.Empty;

            Util.gridClear(dgResult);

            txtPkgLotID.Text = string.Empty;
            txtPrdGrade.Text = string.Empty;
            cboExpDomType.SelectedValue = "SELECT";
            txtPRODID.Text = string.Empty;
            cboProcType.SelectedValue = "SELECT";
        //    cboInboxType.SelectedValue = "SELECT";
            txtSoc.Text = string.Empty;
            txtInputQty.Value = 0;
            chkGrade.Visibility = Visibility.Collapsed;
            chkGrade.IsChecked = false;
            txtAommGrade.Text = string.Empty;
            txtPalletID.Focus();

            SetReadOnly(false);
        }

        private void SetReadOnly(bool bReadOnly)
        {
            // txtPkgLotID.IsReadOnly = bReadOnly;
            txtPrdGrade.IsReadOnly = !string.IsNullOrWhiteSpace(txtPrdGrade.Text); //등급 변경가능하게 수정 2017.01.06
           // cboExpDomType.IsEnabled = !bReadOnly;
            btnPRODID.IsEnabled = !bReadOnly;
            cboProcType.IsEnabled = !bReadOnly;
            //    cboInboxType.IsEnabled = !bReadOnly;
       //     txtSoc.IsReadOnly = bReadOnly;
            txtInputQty.IsReadOnly = bReadOnly;

            txtPalletID.Focus();
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtPalletID.Text))
                    {
                        //SFU3350	입력오류 : PALLETID 를 입력해 주세요.
                        Util.MessageValidation("SFU3350", result =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtPalletID.Focus();
                                txtPalletID.Text = string.Empty;
                            }
                        });
                        return;
                    }      

                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("LANGID");
                    inDataTable.Columns.Add("LOTID");
                    inDataTable.Columns.Add("WIP_QLTY_TYPE_CODE");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["LOTID"] = txtPalletID.Text;
                    newRow["WIP_QLTY_TYPE_CODE"] = (bool)rdoGeneral.IsChecked ? "G" : null;
                    inDataTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService("BR_PRD_GET_INPUT_CTNRLIST_NJ", "INDATA", "OUTPALLET", inDataTable, (dtRslt, searchException) =>
                    {
                        if (searchException != null)
                        {
                            Util.MessageExceptionNoEnter(searchException, result =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID.Focus();
                                    txtPalletID.Text = string.Empty;
                                }
                            });
                        }

                        if (dtRslt != null && dtRslt.Rows.Count > 0)
                        {
                            if (!dtRslt.Columns.Contains("CHK"))
                            {
                                dtRslt.Columns.Add("CHK");
                            }

                            DataTable dtInfo = DataTableConverter.Convert(dgResult.ItemsSource);

                            if (dgResult.GetRowCount() > 0
                                && dtInfo.Select("CTNR_ID = '" + dtRslt.Rows[0]["CTNR_ID"] + "'").Length > 0)
                            {
                                // 그리드 내 중복데이터 존재시 메세지 없이 ruturn;
                                return;
                            }

                            dtRslt.Merge(dtInfo);
                            if (dtRslt.Rows.Count == 1)  dtRslt.Rows[0]["CHK"] = true;
                               
                            Util.GridSetData(dgResult, dtRslt, FrameOperation, true);

                            if (dgResult.GetRowCount() == 1)
                            {
                                GetDetailInfo();
                                setShipToPopControl(txtPRODID.Text.Trim());// 출하처 콤보 Set
                            }

                        }                      
                    });

                    txtPalletID.Focus();
                    txtPalletID.Text = string.Empty;
                }
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

        private void dgResult_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Cell.Column.Name == "CHK" )
            {
                GetDetailInfo();
            }
            
        }

        private void GetDetailInfo()
        {
            List<DataRow> drList = DataTableConverter.Convert(dgResult.ItemsSource).AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString).ToList();

            txtInputQty.Value = drList.Count > 0 ? Util.NVC_Int(drList.Sum(s => s.Field<int>("WIPQTY"))) : 0;

            txtPkgLotID.Text = drList.Select(c => c.Field<string>("ASSY_LOTID")).FirstOrDefault();
            txtPrdGrade.Text = drList.Select(c => c.Field<string>("PRDT_GRD_CODE")).FirstOrDefault();     
            cboExpDomType.SelectedValue = drList.Select(c => c.Field<string>("EXP_DOM_TYPE_CODE")).FirstOrDefault();
            txtPRODID.Text = drList.Select(c => c.Field<string>("PRODID")).FirstOrDefault();
            cboProcType.SelectedValue = drList.Select(c => c.Field<string>("PROCID")).FirstOrDefault();
            //  cboInboxType.SelectedValue = drList.Select(c => c.Field<string>("INBOX_TYPE_CODE")).FirstOrDefault();
            txtSoc.Text = drList.Select(c => c.Field<string>("SOC_VALUE")).FirstOrDefault();
            txtOffGrade.Text = drList.Select(c => c.Field<string>("OFFGRADE_TYPE")).FirstOrDefault();

            List<DataRow> drList_AOMM_GRD_CODE = DataTableConverter.Convert(dgResult.ItemsSource).AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString && c.Field<string>("AOMM_GRD_CODE") != null).ToList();
            txtAommGrade.Text = drList_AOMM_GRD_CODE.Select(c => c.Field<string>("AOMM_GRD_CODE")).FirstOrDefault();

            if (string.Equals(cboProcType.SelectedValue,Process.CELL_BOXING_RETURN))
            {
                chkGrade.Visibility = Visibility.Visible;
            }
            else
            {
                chkGrade.Visibility = Visibility.Collapsed;
                chkGrade.IsChecked = false;
            }

            if (rdoOffgrd.IsChecked == true)
            {
                txtShipto.Visibility = Visibility.Visible;
                popShipto.Visibility = Visibility.Visible;
            }

            setShipToPopControl(txtPRODID.Text.Trim());// 출하처 콤보 Set
            SetAommGrdVisibility(txtPRODID.Text.Trim());

            SetReadOnly(true);
        }

        private void chkLotMerge_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgResult.ItemsSource);

            if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString).Select(c => c.Field<string>("PKG_EQSGID")).Distinct().ToList().Count() > 1)
            {
                //SFU4337		동일한 조립라인만 작업 가능합니다.	
                Util.MessageValidation("SFU4337");
                chkLotMerge.IsChecked = false;
                return;
            }
            else if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString).Select(c => c.Field<string>("EXP_DOM_TYPE_CODE")).Distinct().ToList().Count() > 1)
            {
                //	SFU4346		동일한 수출/내수를 선택하세요.	
                Util.MessageValidation("SFU4346");
                chkLotMerge.IsChecked = false;
                return;
            }

            else if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString).Select(c => c.Field<string>("PRODWEEK")).Distinct().ToList().Count() > 1)
            {
                //SFU4347	동일한 생산주차의 LOT이 아닙니다.
                Util.MessageValidation("SFU4347");
                chkLotMerge.IsChecked = false;
                return;
            }
        }

        private void chkLotMerge_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgResult.ItemsSource);

            if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString).Select(c => c.Field<string>("PKG_EQSGID")).Distinct().ToList().Count() > 1)
            {
                //SFU4337		동일한 조립라인만 작업 가능합니다.	
                Util.MessageValidation("SFU4337");
                chkLotMerge.IsChecked = true;
                return;
            }

            else if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString).Select(c => c.Field<string>("EXP_DOM_TYPE_CODE")).Distinct().ToList().Count() > 1)
            {
                //	SFU4346		동일한 수출/내수를 선택하세요.	
                Util.MessageValidation("SFU4346");
                chkLotMerge.IsChecked = true;
                return;
            }
     
            else if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString).Select(c => c.Field<string>("ASSY_LOTID")).Distinct().ToList().Count() > 1)
            {
                //	SFU4146		동일한 조립LOT이 아닙니다.	
                Util.MessageValidation("SFU4146");
                chkLotMerge.IsChecked = true;
                return;
            }
        }

        private void dgResult_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            DataTable dtInfo = DataTableConverter.Convert(dgResult.ItemsSource);

            string sPkgEqsgID = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["PKG_EQSGID"].Index).Value);
            string sProdWeek = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["PRODWEEK"].Index).Value);
            string sAssyLotID = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["ASSY_LOTID"].Index).Value);
            string sExpDom = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["EXP_DOM_TYPE_CODE"].Index).Value);
            string sAommGrdCode = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["AOMM_GRD_CODE"].Index).Value);
            string sLotType = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["LOTTYPE"].Index).Value);

            // E20230424-000080 : 동일한 LOTTYPE
            if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString
            && c.Field<string>("LOTTYPE") != sLotType).ToList().Count() > 0)
            {
                //SFU9905		the market type of cart is different
                Util.MessageValidation("SFU9905");
                e.Cancel = true;
                return;
            }

            else if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString
            && c.Field<string>("PKG_EQSGID") != sPkgEqsgID).ToList().Count() > 0)
            {
                //SFU4337		동일한 조립라인만 작업 가능합니다.	
                Util.MessageValidation("SFU4337");
                e.Cancel = true;
                return;
            }

            else if (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString
            && c.Field<string>("EXP_DOM_TYPE_CODE") != sExpDom).ToList().Count() > 0)
            {
                //	SFU4346		동일한 수출/내수를 선택하세요.	
                Util.MessageValidation("SFU4346");
                e.Cancel = true;
                return;
            }

            else if (string.IsNullOrEmpty(sAommGrdCode) == false
                && dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString && c.Field<string>("AOMM_GRD_CODE") != sAommGrdCode && c.Field<string>("AOMM_GRD_CODE") != null ).ToList().Count() > 0)
            {
                //동일한 AOMM 등급을 선택하세요.
                Util.MessageValidation("SFU3803");
                e.Cancel = true;
                return;
            }

            else if (chkLotMerge.IsChecked == true  && (dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString && c.Field<string>("PRODWEEK") != sProdWeek).ToList().Count() > 0))
            {
                //SFU4347	동일한 생산주차의 LOT이 아닙니다.
                Util.MessageValidation("SFU4347");
                e.Cancel = true;
                return;
            }
            else if (chkLotMerge.IsChecked != true && ( dtInfo.AsEnumerable().Where(c => c.Field<string>("CHK") == bool.TrueString && c.Field<string>("ASSY_LOTID") != sAssyLotID).ToList().Count() > 0))
            {
                //	SFU4146		동일한 조립LOT이 아닙니다.	
                Util.MessageValidation("SFU4146");
                e.Cancel = true;
                return;
            }

        }

        private void txtPrdGrade_LostFocus(object sender, RoutedEventArgs e) //2017.01.06 입력VALIDATION
        {
            //if (dgResult.ItemsSource == null)
            //{
            //    return;
            //}
            Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]{1,10}$");
            Boolean ismatch = regex.IsMatch(txtPrdGrade.Text);
            if (!ismatch)
            {


                // 숫자와 영문대문자만 입력가능합니다.
                Util.MessageValidation("SFU3674", (result) =>
                {
                    if(result == MessageBoxResult.OK)
                    {
                        txtPrdGrade.Focus();
                        txtPrdGrade.SelectAll();
                    }
                });
                return;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(string.Equals(cboProcType.SelectedValue,Process.CELL_BOXING_RETURN))
                txtPrdGrade.IsReadOnly = false;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPrdGrade.IsReadOnly = true;
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void setShipToPopControl(string prodID)
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID, LoginInfo.LANGID };
            CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
        }

        private void rdoGeneral_Checked(object sender, RoutedEventArgs e)
        {
            if (dgResult.GetRowCount() <= 0)
                return;

            txtShipto.Visibility = Visibility.Collapsed;
            popShipto.Visibility = Visibility.Collapsed;
        }

        private void rdoOffgrd_Checked(object sender, RoutedEventArgs e)
        {
            if (dgResult.GetRowCount() <= 0)
                return;

            txtShipto.Visibility = Visibility.Visible;
            popShipto.Visibility = Visibility.Visible;
        }

        private void SetAommGrdVisibility(string sProdID)
        {
            try
            {
                if (string.IsNullOrEmpty(sProdID))
                {
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "GRADE_CHK_PROD";
                dr["CMCODE"] = sProdID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult != null && dtResult.Rows.Count > 0)
                {
                    _AommGrdChkFlag = true;
                    tbAommGrade.Visibility = Visibility.Visible;
                    txtAommGrade.Visibility = Visibility.Visible;
                }
                else
                {
                    _AommGrdChkFlag = false;
                    tbAommGrade.Visibility = Visibility.Collapsed;
                    txtAommGrade.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
