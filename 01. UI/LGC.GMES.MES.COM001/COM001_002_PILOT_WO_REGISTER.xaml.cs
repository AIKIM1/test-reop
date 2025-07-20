/*************************************************************************************
 Created Date : 2019.12.19
      Creator : 정문교 
   Decription : 일 생산계획 > 시생산 WO 등록 팝업 
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.19  DEVELOPER : Initial Created.
  2021.02.08  조영대    : 설비 콤보 초기값 SELECT 로 설정
  2021.03.02  조영대    : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
  2021.06.16  조영대    : TOP_PRODID 선택 콤보 활성화시 믹서공정(E1000) 에 선분산 믹서공정(E0500) 도 추가
  2021.07.13  김지은    : [GM JV Proj.]W/O Type(시생산 샘플) 추가
  2023.09.14  김태균    : TOP_PRODID 선택 콤보를 선분산 믹서 공정도 보여주고 있으나, 
                          BIZ 호출시에는 TOP_PRODID를 E1000만 넘겨 주고 있어 E0500도 추가함 
  2024.08.10  배현우    : [E20240807-000861] Dam Mixer 공정이  WO 공정으로 변경됨에따라 비즈 분기 변경
  2024.08.10  배현우    : [E20240807-000861] Dam Mixer 공정이  WO 공정으로 변경됨에따라 비즈 분기 변경
  2025.03.10  이민형    : [HD_OSS_0057] 시생산샘플 라디오 옵션 컨트롤 제외처리
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_002_PILOT_WO_REGISTER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        string _AreaID = string.Empty;
        string _ProciD = string.Empty;

        public COM001_002_PILOT_WO_REGISTER()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void InitializeControls()
        {
            Util.gridClear(dgList);
            
            dgList.Columns["EQPTNAME"].Visibility = Visibility.Hidden;
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            //_area = tmps[0] as string;
            if(tmps.Length > 0) // 2024.10.11. 김영국 - Main 화면에서 선택된 Process ID를 넘기도록 수정함.
                _ProciD = tmps[0] as string;
        }

        private void SetCombo()
        {
            CommonCombo combo = new CommonCombo();

            C1ComboBox[] cboShopChild = { cboArea, cboEquipmentSegment, cboProcess, cboEquipment };
            combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, sCase: "SHOP");
            cboShop.IsEnabled = false;  // 2021.07.13 : Shop 선택 필요 없음

            //동
            C1ComboBox[] cboAreaParent = { cboShop };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboProcess, cboEquipment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbParent: cboAreaParent, cbChild: cboAreaChild);
            cboArea.IsEnabled = false; // 2021.07.13 : 동선택 필요 없음

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea, cboShop };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent, cbChild: cboProcessChild);

            // 2021.07.13 : W/O Type 추가 - 라디오버튼으로 대체
            //string[] sFilter = { "DEMAND_TYPE" };
            //combo.SetCombo(cboDEMAND_TYPE, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
            //cboDEMAND_TYPE.SelectedValue = "PILOT";
            //cboDEMAND_TYPE.IsEnabled = false;

            // 2020.11.30 : 전극 설비 콤보 및 인수 추가 
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            DataRowView dv = cboArea.SelectedItem as DataRowView;
            if (dv.Row["AREA_TYPE_CODE"].Equals("E"))
            {
                tbkEquipment.Visibility = Visibility.Visible;
                cboEquipment.Visibility = Visibility.Visible;
                dgList.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                cboEquipment.SelectedIndex = 0; //2021.02.08  조영대: 설비 콤보 초기값 SELECT 로 설정
            }
            else
            {
                tbkEquipment.Visibility = Visibility.Hidden;
                cboEquipment.Visibility = Visibility.Hidden;
            }

            // 2021.03.02 : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
            // 2021.06.16 : TOP_PRODID 선택 콤보 활성화시 믹서공정(E1000) 에 선분산 믹서공정(E0500) 도 추가
            if (Util.NVC(cboProcess.SelectedValue).Equals(Process.MIXING) ||
                Util.NVC(cboProcess.SelectedValue).Equals(Process.PRE_MIXING) ||
                Util.NVC(cboProcess.SelectedValue).Equals(Process.DAM_MIXING))
            {
                tbkTopProduct.Visibility = popSearchTopProdID.Visibility = Visibility.Visible;
            }
            else
            {
                tbkTopProduct.Visibility = popSearchTopProdID.Visibility = Visibility.Collapsed;
            }
        }
        
        private void SetControl()
        {
            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void SetRadioBtn()
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
                dr["CMCDTYPE"] = "DEMAND_TYPE";
                dr["ATTRIBUTE1"] = "P"; // 시생산 관련 코드만 조회
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_BY_ATTR1", "RQSTDT", "RSLTDT", RQSTDT);
                
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if(dtResult.Rows[i]["CBO_CODE"] != null 
                        && string.Equals(dtResult.Rows[i]["CBO_CODE"].ToString(), "PILOT_S"))
                    {
                        continue;
                    }

                    RadioButton rb = new RadioButton();
                    rb.Content = dtResult.Rows[i]["CBO_NAME"];
                    rb.Tag = dtResult.Rows[i]["CBO_CODE"];
                    rb.GroupName = "rdoWOType";
                    rb.Margin = new Thickness(4,0,4,0);
                    tbWOType.Children.Add(rb);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            SetParameters();
            SetCombo();
            SetControl();
            SetRadioBtn();  // 2021.07.13 : W/O Type 추가
            SearchProduct();

            // 2024.10.11. 김영국 - Main 화면에서 선택된 Process ID를 넘기도록 수정함.
            if (_ProciD != "")
                cboProcess.SelectedValue = _ProciD;

            this.Loaded -= C1Window_Loaded;
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //SearchProduct();

            // 2020.11.30 : 전극 설비 콤보 및 인수 추가
            DataRowView dv = cboArea.SelectedItem as DataRowView;
            if (dv.Row["AREA_TYPE_CODE"].Equals("E"))
            {
                tbkEquipment.Visibility = Visibility.Visible;
                cboEquipment.Visibility = Visibility.Visible;
            }
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //SearchProduct();

            // 2021.03.02 : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
            // 2021.06.16 : TOP_PRODID 선택 콤보 활성화시 믹서공정(E1000) 에 선분산 믹서공정(E0500) 도 추가
            if (Util.NVC(cboProcess.SelectedValue).Equals(Process.MIXING) ||
                Util.NVC(cboProcess.SelectedValue).Equals(Process.PRE_MIXING) ||
                Util.NVC(cboProcess.SelectedValue).Equals(Process.DAM_MIXING))
            {
                tbkTopProduct.Visibility = popSearchTopProdID.Visibility = Visibility.Visible;
            }
            else
            {
                tbkTopProduct.Visibility = popSearchTopProdID.Visibility = Visibility.Collapsed;
            }            

            //2021.02.08  조영대: 설비 콤보 초기값 SELECT 로 설정
            cboProcess.Dispatcher.BeginInvoke(new Action(() =>
            {
                cboEquipment.SelectedIndex = 0;
            }));            
        }

        private void popSearchProdID_ValueChanged(object sender, EventArgs e)
        {
            //DataTable dt = DataTableConverter.Convert(popSearchProdID.ItemsSource);
            //string prjt_name = string.Empty;
            //foreach (DataRowView drv in dt.DefaultView)
            //{
            //    if (Util.NVC(DataTableConverter.GetValue(drv, "PRODID")).Equals(popSearchProdID.SelectedValue as string))
            //    {
            //        prjt_name = Util.NVC(DataTableConverter.GetValue(drv, "PRJT_NAME"));
            //        break;
            //    }
            //}

            SearchTopProduct();
        }

        /// <summary>
        /// 추가
        /// </summary>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationADD())
                return;

            DataGrid01RowAdd();
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete())
                return;

            DataGrid01RowDelete();
        }

        /// <summary>
        /// 생성
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            // 생성 하시겠습니까 ?
            Util.MessageConfirm("SFU1621", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            });
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 반제품 조회
        /// </summary>
        private void SearchProduct()
        {
            try
            {
                if (cboShop.SelectedValue == null || cboShop.SelectedValue.ToString().Equals("SELECT")) return;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SHOPID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SHOPID"] = cboShop.SelectedValue.ToString();
                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_VW_PRODUCT", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchProdID.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        // 2021.03.02 : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
        private void SearchTopProduct()
        {
            try
            {
                if (cboShop.SelectedValue == null || cboShop.SelectedValue.ToString().Equals("SELECT")) return;
                if (popSearchProdID.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchProdID.SelectedValue.ToString()))
                {
                    popSearchTopProdID.SelectedValue = null;
                    popSearchTopProdID.SelectedText = null;
                    popSearchTopProdID.ItemsSource = null;
                    return;
                }
                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SHOPID"] = cboShop.SelectedValue.ToString();
                newRow["PRODID"] = popSearchProdID.SelectedValue.ToString();
                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_TOP_PRODUCT", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchTopProdID.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveProcess()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("DEMAND_TYPE", typeof(string));  // 2021.07.13 : W/O Type 추가
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("PLAN_QTY", typeof(string));
                inTable.Columns.Add("PLAN_STRT_DATE", typeof(string));
                inTable.Columns.Add("PLAN_END_DATE", typeof(string));
                inTable.Columns.Add("TOP_PRODID", typeof(string));

                ///////////////////////////////// Setting
                #region 2024.10.16. 김영국 - 선택된 WO List 만 저장하도록 수정. (최재철 책임 요청)
                //DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                //foreach (DataRow dr in dt.Rows)
                //{
                //    DataRow newRow = inTable.NewRow();
                //    newRow["SHOPID"] = dr["SHOPID"];
                //    newRow["AREAID"] = dr["AREAID"];
                //    newRow["DEMAND_TYPE"] = dr["DEMAND_TYPE"];  // 2021.07.13 : W/O Type 추가
                //    newRow["EQSGID"] = dr["EQSGID"];
                //    newRow["PROCID"] = dr["PROCID"];
                //    // 2020.11.30 : 전극 설비 콤보 및 인수 추가
                //    if (cboEquipment.Visibility.Equals(Visibility.Visible))
                //    {
                //        newRow["EQPTID"] = dr["EQPTID"];
                //    }
                //    newRow["PRODID"] = dr["PRODID"];
                //    newRow["PLAN_QTY"] = dr["PLAN_QTY"];
                //    newRow["PLAN_STRT_DATE"] = dr["STRT_DTTM"];
                //    newRow["PLAN_END_DATE"] = dr["END_DTTM"];

                //    // 2021.03.02 : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
                //    // 2023.09.14 : E0500 추가.
                //    if ((Util.NVC(dr["PROCID"]).Equals("E1000")) || (Util.NVC(dr["PROCID"]).Equals("E0500")))
                //    {
                //        newRow["TOP_PRODID"] = dr["TOP_PRODID"];
                //    }
                //    inTable.Rows.Add(newRow);
                //} 
                #endregion

                DataRow[] drList = Util.gridGetChecked(ref dgList, "CHK", true);


                foreach (DataRow dr in drList)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["SHOPID"] = dr["SHOPID"];
                    newRow["AREAID"] = dr["AREAID"];
                    newRow["DEMAND_TYPE"] = dr["DEMAND_TYPE"];  // 2021.07.13 : W/O Type 추가
                    newRow["EQSGID"] = dr["EQSGID"];
                    newRow["PROCID"] = dr["PROCID"];
                    // 2020.11.30 : 전극 설비 콤보 및 인수 추가
                    if (cboEquipment.Visibility.Equals(Visibility.Visible))
                    {
                        newRow["EQPTID"] = dr["EQPTID"];
                    }
                    newRow["PRODID"] = dr["PRODID"];
                    newRow["PLAN_QTY"] = dr["PLAN_QTY"];
                    newRow["PLAN_STRT_DATE"] = dr["STRT_DTTM"];
                    newRow["PLAN_END_DATE"] = dr["END_DTTM"];

                    // 2021.03.02 : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
                    // 2023.09.14 : E0500 추가.
                    if ((Util.NVC(dr["PROCID"]).Equals("E1000")) || (Util.NVC(dr["PROCID"]).Equals("E0500")))
                    {
                        newRow["TOP_PRODID"] = dr["TOP_PRODID"];
                    }
                    inTable.Rows.Add(newRow);
                }

                ////////////////////////////////////////////////////////////////////
                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_REG_PILOT_WO_REGISTER", "INDATA", null, inTable, (result, bizException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 생성완료 되었습니다.
                        Util.MessageValidation("SFU1625");

                        InitializeControls();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool ValidationADD()
        {
            if (cboShop.SelectedIndex < 0 || cboShop.SelectedValue.GetString().Equals("SELECT"))
            {
                // Shop을 선택하세요.
                Util.MessageValidation("SFU1424");
                return false;
            }

            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.GetString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboProcess.SelectedIndex < 0 || cboProcess.SelectedValue.GetString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            // 2020.11.30 : 전극 설비 콤보 및 인수 추가
            if (cboEquipment.Visibility.Equals(Visibility.Visible))
            {
                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.GetString().Equals("SELECT"))
                {
                    // 설비를 선택하세요.
                    Util.MessageValidation("SFU1673");
                    return false;
                }
            }

            // 2021.07.13 : W/O Type 추가
            var chkWOType = tbWOType.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true);
            if (chkWOType == null)
            {
                // 선택된 유형코드가 없습니다
                Util.MessageValidation("SFU1642");
                return false;
            }

            if (popSearchProdID.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchProdID.SelectedValue.ToString()))
            {
                // 반제품 정보가 없습니다.
                Util.MessageValidation("SFU1542");
                return false;
            }

            if (dtpDateFrom.SelectedDateTime > dtpDateTo.SelectedDateTime)
            {
                // 계획종료일자가 시작일자보다 빠릅니다.
                Util.MessageValidation("SFU2858");
                return false;

            }

            if (txtPlanQty.Value.ToString() == double.NaN.ToString() || txtPlanQty.Value == 0)
            {
                // 계획수량을 입력하세요.
                Util.MessageValidation("SFU2856");
                return false;
            }

            // 2021.03.02 : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
            if (popSearchTopProdID.Visibility.Equals(Visibility.Visible))
            {
                if (popSearchTopProdID.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchTopProdID.SelectedValue.ToString()))
                {
                    // %1(을)를 선택하세요.
                    Util.MessageValidation("SFU4925", tbkTopProduct.Text);
                    return false;
                }
            }
            return true;
        }

        private bool ValidationDelete()
        {
            if (dgList.Rows.Count == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = True");

            if (drSelect.Length == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            #region 2024.10.16. 김영국 - 선택된 Data가 없는 경우 메세지를 띄우도록 수정.
            //DataRow[] drSelect = DataTableConverter.Convert(dgList.ItemsSource).Select();
            //if (drSelect.Length == 0)
            //{
            //    // 선택된 작업대상이 없습니다.
            //    Util.MessageValidation("SFU1645");
            //    return false;
            //} 
            #endregion

            if (dgList.Rows.Count == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            DataRow[] drSelect = Util.gridGetChecked(ref dgList, "CHK", true);

            if (drSelect.Length == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        #endregion

        #region [Function]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Collapsed)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void DataGrid01RowAdd()
        {
            DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
            if (dt == null || dt.Columns.Count == 0)
            {
                for (int col = 0; col < dgList.Columns.Count; col++)
                {
                    dt.Columns.Add(dgList.Columns[col].Name.ToString(), typeof(string));
                }
            }
            else
            {
                // 2020.11.30 : 전극 설비 콤보 및 인수 추가
                // 2021.03.02 : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
                string selectString =
                    "AREAID = '" + cboArea.SelectedValue.ToString() + "' And " +
                    "EQSGID = '" + cboEquipmentSegment.SelectedValue.ToString() + "' And " +
                    "PROCID = '" + cboProcess.SelectedValue.ToString() + "' And " +
                    "PRODID = '" + popSearchProdID.SelectedValue.ToString() + "' And ";

                if (cboEquipment.Visibility.Equals(Visibility.Visible))
                {
                    selectString += "EQPTID = '" + cboEquipment.SelectedValue.ToString() + "' And ";
                }

                if (cboEquipment.Visibility.Equals(Visibility.Visible))
                {
                    selectString += "TOP_PRODID = '" + popSearchTopProdID.SelectedValue + "' And ";
                }

                selectString +=
                    "STRT_DTTM = '" + dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + "' And " +
                    "END_DTTM = '" + dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + "'";

                DataRow[] dr = dt.Select(selectString);                
                if (dr.Length > 0)
                {
                    // [% 1]은 이미 등록되었습니다.
                    Util.MessageValidation("SFU3471", ObjectDic.Instance.GetObjectName("WO") + " " + ObjectDic.Instance.GetObjectName("생성"));
                    return;
                }
            }

            DataRow newrow = dt.NewRow();
            newrow["CHK"] = false;
            newrow["SHOPID"] = cboShop.SelectedValue.ToString();
            newrow["AREAID"] = cboArea.SelectedValue.ToString();
            newrow["AREANAME"] = cboArea.Text;
            newrow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
            newrow["EQSGNAME"] = cboEquipmentSegment.Text;
            newrow["PROCID"] = cboProcess.SelectedValue.ToString();
            newrow["PROCNAME"] = cboProcess.Text;
            // 2020.11.30 : 전극 설비 콤보 및 인수 추가 
            if (cboEquipment.Visibility.Equals(Visibility.Visible))
            {
                newrow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newrow["EQPTNAME"] = cboEquipment.Text;
            }
            // 2021.07.13 : W/O Type 추가
            var chkWOType = tbWOType.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true);
            newrow["DEMAND_TYPE"] = chkWOType.Tag.ToString();
            newrow["PRODID"] = popSearchProdID.SelectedValue.ToString();
            newrow["STRT_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
            newrow["END_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
            newrow["PLAN_QTY"] = txtPlanQty.Value;

            // 2021.03.02 : 믹서공정(E1000) 일 때만 TOP_PRODID 선택 콤보 추가
            if (popSearchTopProdID.Visibility.Equals(Visibility.Visible))
            {
                newrow["TOP_PRODID"] = popSearchTopProdID.SelectedValue;
            }
            dt.Rows.Add(newrow);

            Util.GridSetData(dgList, dt, null, true);
        }

        private void DataGrid01RowDelete()
        {
            DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
            dt.Select("CHK = True").ToList<DataRow>().ForEach(row => row.Delete());
            dt.AcceptChanges();

            Util.GridSetData(dgList, dt, null, true);
        }

        #endregion


        #endregion

    }
}
